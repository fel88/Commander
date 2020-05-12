using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace commander
{
    public class SubtitlesMetaInfo : MetaInfo
    {
        public List<SubtitleItem> Items = new List<SubtitleItem>();

        public static SubtitlesMetaInfo FromSRT(string text)
        {
            SubtitlesMetaInfo ret = new SubtitlesMetaInfo();
            var split = text.Split(new string[] { Environment.NewLine + Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            foreach (var item in split)
            {
                var spl = item.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                var id = int.Parse(spl[0]);
                var spl2 = spl[1].Split(new string[] { "-->", " " }, StringSplitOptions.RemoveEmptyEntries).ToArray();

                var start = TimeSpan.Parse(spl2[0]);
                var end = TimeSpan.Parse(spl2[1]);
                var spl3 = spl.Skip(2).ToArray();
                var text1 = spl3.Aggregate("", (x, y) => x + y);
                ret.Items.Add(new SubtitleItem() { Text = text1, Start = start, End = end, Id = id });
            }

            return ret;
        }
        public string GetSRT()
        {
            StringBuilder sb = new StringBuilder();
            int index = 1;
            foreach (var item in Items)
            {
                sb.AppendLine(index.ToString());
                index++;
                // 00:00:22,447 --> 00:00:25,417
                sb.AppendLine(item.Start + " --> " + item.End);
                sb.AppendLine(item.Text);
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
    public class SubtitleItem
    {
        public int Id { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public string Text { get; set; }
    }
}