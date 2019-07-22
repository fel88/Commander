using System.Collections.Generic;
using System.Drawing;

namespace GamesPack1
{
    public class PuzzleItem
    {
        public PuzzleItem Parent;
        public List<PuzzleItem> Connected = new List<PuzzleItem>();
        public int Z;
        public Bitmap Bmp;
        public Point Position { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Ang { get; set; }

        public PointF TopLeftShift;
        public PointF CenterShift;


        internal void Attach(PuzzleItem puzzleItem)
        {
            Connected.Add(puzzleItem);
            puzzleItem.Parent = this;
        }
    }
}


