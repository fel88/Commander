#ifdef HAVE_CONFIG_H
#include "webp/config.h"
#endif


#if defined(__unix__) || defined(__CYGWIN__)
#define _POSIX_C_SOURCE 200112L  // for setenv
#endif


#include <stdio.h>
#include <stdlib.h>
#include <string.h>


#ifdef WEBP_HAVE_QCMS
#include <qcms.h>
#endif

#include "webp/decode.h"
#include "webp/demux.h"

#include "../examples/example_util.h"
#include "../imageio/imageio_util.h"
#include "./unicode.h"

#if defined(_MSC_VER) && _MSC_VER < 1900
#define snprintf _snprintf
#endif

// Unfortunate global variables. Gathered into a struct for comfort.
static struct {
  int has_animation;
  int has_color_profile;
  int done;
  int decoding_error;
  int print_info;
  int only_deltas;
  int use_color_profile;
  int draw_anim_background_color;

  int canvas_width, canvas_height;
  int loop_count;
  uint32_t bg_color;

  const char* file_name;
  WebPData data;
  WebPDecoderConfig config;
  const WebPDecBuffer* pic;
  WebPDemuxer* dmux;
  WebPIterator curr_frame;
  WebPIterator prev_frame;
  WebPChunkIterator iccp;
  int viewport_width, viewport_height;
} kParams;

__declspec(dllexport) int sum(int a, int b) 
{
return a+b;
}

__declspec(dllexport) int test1() 
{
return 5;
}

__declspec(dllexport) int test2(int a) 
{
return a+1;
}
// Sets the previous frame to the dimensions of the canvas and has it dispose
// to background to cause the canvas to be cleared.
static void ClearPreviousFrame(void) {
  WebPIterator* const prev = &kParams.prev_frame;
  prev->width = kParams.canvas_width;
  prev->height = kParams.canvas_height;
  prev->x_offset = prev->y_offset = 0;
  prev->dispose_method = WEBP_MUX_DISPOSE_BACKGROUND;
}

static void ClearPreviousPic(void) {
  WebPFreeDecBuffer((WebPDecBuffer*)kParams.pic);
  kParams.pic = NULL;
}
__declspec(dllexport) void decodeIteration(int what)
{

  if (what == 0 && !kParams.done) {
    int duration = 0;
    if (kParams.dmux != NULL) {
      WebPIterator* const curr = &kParams.curr_frame;
      if (!WebPDemuxNextFrame(curr)) {
        WebPDemuxReleaseIterator(curr);
        if (WebPDemuxGetFrame(kParams.dmux, 1, curr)) {
          --kParams.loop_count;
          kParams.done = (kParams.loop_count == 0);
          if (kParams.done) return;
          ClearPreviousFrame();
        } else {
          kParams.decoding_error = 1;
          kParams.done = 1;
          return;
        }
      }
      duration = curr->duration;
      // Behavior copied from Chrome, cf:
      // https://cs.chromium.org/chromium/src/third_party/WebKit/Source/
      // platform/graphics/DeferredImageDecoder.cpp?
      // rcl=b4c33049f096cd283f32be9a58b9a9e768227c26&l=246
      if (duration <= 10) duration = 100;
    }
    if (!Decode()) {
      kParams.decoding_error = 1;
      kParams.done = 1;
    } else {
      
    }
  }

}
__declspec(dllexport) int open(char* filePath) 
{
  int c;
  WebPDecoderConfig* const config = &kParams.config;
  WebPIterator* const curr = &kParams.curr_frame;
  

  if (!WebPInitDecoderConfig(config)) {
    fprintf(stderr, "Library version mismatch!\n");
    FREE_WARGV_AND_RETURN(-1);
  }
  config->options.dithering_strength = 50;
  config->options.alpha_dithering_strength = 100;
  kParams.use_color_profile = 1;
  // Background color hidden by default to see transparent areas.
  kParams.draw_anim_background_color = 0;

  
    int parse_error = 0;
    
      kParams.file_name = (const char*)filePath;
    

    

  if (kParams.file_name == NULL) {
    printf("missing input file!!\n");
    //Help();
    FREE_WARGV_AND_RETURN(0);
  }

  if (!ImgIoUtilReadFile(kParams.file_name,
                         &kParams.data.bytes, &kParams.data.size)) {
    return -1;
  }

  if (!WebPGetInfo(kParams.data.bytes, kParams.data.size, NULL, NULL)) {
    fprintf(stderr, "Input file doesn't appear to be WebP format.\n");
   return -1;
  }

  kParams.dmux = WebPDemux(&kParams.data);
  if (kParams.dmux == NULL) {
    fprintf(stderr, "Could not create demuxing object!\n");
  return -1;
  }

  kParams.canvas_width = WebPDemuxGetI(kParams.dmux, WEBP_FF_CANVAS_WIDTH);
  kParams.canvas_height = WebPDemuxGetI(kParams.dmux, WEBP_FF_CANVAS_HEIGHT);
  if (kParams.print_info) {
    printf("Canvas: %d x %d\n", kParams.canvas_width, kParams.canvas_height);
  }

  ClearPreviousFrame();

  memset(&kParams.iccp, 0, sizeof(kParams.iccp));
  kParams.has_color_profile =
      !!(WebPDemuxGetI(kParams.dmux, WEBP_FF_FORMAT_FLAGS) & ICCP_FLAG);
  if (kParams.has_color_profile) {
#ifdef WEBP_HAVE_QCMS
    if (!WebPDemuxGetChunk(kParams.dmux, "ICCP", 1, &kParams.iccp)) return -1;
    printf("VP8X: Found color profile\n");
#else
    fprintf(stderr, "Warning: color profile present, but qcms is unavailable!\n"
            "Build libqcms from Mozilla or Chromium and define WEBP_HAVE_QCMS "
            "before building.\n");
#endif
  }

  if (!WebPDemuxGetFrame(kParams.dmux, 1, curr)) return -1;

  kParams.has_animation = (curr->num_frames > 1);
  kParams.loop_count = (int)WebPDemuxGetI(kParams.dmux, WEBP_FF_LOOP_COUNT);
  kParams.bg_color = WebPDemuxGetI(kParams.dmux, WEBP_FF_BACKGROUND_COLOR);
  printf("VP8X: Found %d images in file (loop count = %d)\n",
         curr->num_frames, kParams.loop_count);

  // Decode first frame
  if (!Decode()) return -1;

  // Position iterator to last frame. Next call to HandleDisplay will wrap over.
  // We take this into account by bumping up loop_count.
  WebPDemuxGetFrame(kParams.dmux, 0, curr);
  if (kParams.loop_count) ++kParams.loop_count;

#if defined(__unix__) || defined(__CYGWIN__)
  // Work around GLUT compositor bug.
  // https://bugs.launchpad.net/ubuntu/+source/freeglut/+bug/369891
  setenv("XLIB_SKIP_ARGB_VISUALS", "1", 1);
#endif


  //if (kParams.has_animation) glutTimerFunc(0, decode_callback, 0);
  decodeIteration(0);

}

__declspec(dllexport) int getWidth()
{

const WebPDecBuffer* const pic = kParams.pic;
  const WebPIterator* const curr = &kParams.curr_frame;
  return pic->width;

}
__declspec(dllexport) int getDuration()
{

const WebPDecBuffer* const pic = kParams.pic;
  const WebPIterator* const curr = &kParams.curr_frame;
  return curr->duration;

}

__declspec(dllexport) int getHeight()
{

const WebPDecBuffer* const pic = kParams.pic;
  const WebPIterator* const curr = &kParams.curr_frame;
  return pic->height;
}

__declspec(dllexport) int getXoffset()
{

const WebPDecBuffer* const pic = kParams.pic;
  const WebPIterator* const curr = &kParams.curr_frame;
  return curr->x_offset;
}
__declspec(dllexport) int getYoffset()
{

const WebPDecBuffer* const pic = kParams.pic;
  const WebPIterator* const curr = &kParams.curr_frame;
  return curr->y_offset;
}

__declspec(dllexport) int getFramesNum()
{
const WebPDecBuffer* const pic = kParams.pic;
  const WebPIterator* const curr = &kParams.curr_frame;
  return curr->num_frames;
}

__declspec(dllexport) int getCurrFrame()
{
  
const WebPDecBuffer* const pic = kParams.pic;
  const WebPIterator* const curr = &kParams.curr_frame;
  return curr->frame_num;
}

__declspec(dllexport) void getData(unsigned char* data)
{

const WebPDecBuffer* const pic = kParams.pic;
  const WebPIterator* const curr = &kParams.curr_frame;
 memcpy(data,(char*)pic->u.RGBA.rgba,4* pic->width*pic->height);
    
}

static void ClearParams(void) {
  ClearPreviousPic();
  WebPDataClear(&kParams.data);
  WebPDemuxReleaseIterator(&kParams.curr_frame);
  WebPDemuxReleaseIterator(&kParams.prev_frame);
  WebPDemuxReleaseChunkIterator(&kParams.iccp);
  WebPDemuxDelete(kParams.dmux);
  kParams.dmux = NULL;
}


// -----------------------------------------------------------------------------
// Color profile handling
static int ApplyColorProfile(const WebPData* const profile,
                             WebPDecBuffer* const rgba) {
#ifdef WEBP_HAVE_QCMS
  int i, ok = 0;
  uint8_t* line;
  uint8_t major_revision;
  qcms_profile* input_profile = NULL;
  qcms_profile* output_profile = NULL;
  qcms_transform* transform = NULL;
  const qcms_data_type input_type = QCMS_DATA_RGBA_8;
  const qcms_data_type output_type = QCMS_DATA_RGBA_8;
  const qcms_intent intent = QCMS_INTENT_DEFAULT;

  if (profile == NULL || rgba == NULL) return 0;
  if (profile->bytes == NULL || profile->size < 10) return 1;
  major_revision = profile->bytes[8];

  qcms_enable_iccv4();
  input_profile = qcms_profile_from_memory(profile->bytes, profile->size);
  // qcms_profile_is_bogus() is broken with ICCv4.
  if (input_profile == NULL ||
      (major_revision < 4 && qcms_profile_is_bogus(input_profile))) {
    fprintf(stderr, "Color profile is bogus!\n");
    goto Error;
  }

  output_profile = qcms_profile_sRGB();
  if (output_profile == NULL) {
    fprintf(stderr, "Error creating output color profile!\n");
    goto Error;
  }

  qcms_profile_precache_output_transform(output_profile);
  transform = qcms_transform_create(input_profile, input_type,
                                    output_profile, output_type,
                                    intent);
  if (transform == NULL) {
    fprintf(stderr, "Error creating color transform!\n");
    goto Error;
  }

  line = rgba->u.RGBA.rgba;
  for (i = 0; i < rgba->height; ++i, line += rgba->u.RGBA.stride) {
    qcms_transform_data(transform, line, line, rgba->width);
  }
  ok = 1;

 Error:
  if (input_profile != NULL) qcms_profile_release(input_profile);
  if (output_profile != NULL) qcms_profile_release(output_profile);
  if (transform != NULL) qcms_transform_release(transform);
  return ok;
#else
  (void)profile;
  (void)rgba;
  return 1;
#endif  // WEBP_HAVE_QCMS
}

//------------------------------------------------------------------------------
// File decoding

static int Decode(void) {   // Fills kParams.curr_frame
  const WebPIterator* const curr = &kParams.curr_frame;
  WebPDecoderConfig* const config = &kParams.config;
  WebPDecBuffer* const output_buffer = &config->output;
  int ok = 0;

  ClearPreviousPic();
  output_buffer->colorspace = MODE_RGBA;
  ok = (WebPDecode(curr->fragment.bytes, curr->fragment.size,
                   config) == VP8_STATUS_OK);
  if (!ok) {
    fprintf(stderr, "Decoding of frame #%d failed!\n", curr->frame_num);
  } else {
    kParams.pic = output_buffer;
    if (kParams.use_color_profile) {
      ok = ApplyColorProfile(&kParams.iccp.chunk, output_buffer);
      if (!ok) {
        fprintf(stderr, "Applying color profile to frame #%d failed!\n",
                curr->frame_num);
      }
    }
  }
  return ok;
}



static float GetColorf(uint32_t color, int shift) {
  return ((color >> shift) & 0xff) / 255.f;
}


//------------------------------------------------------------------------------
