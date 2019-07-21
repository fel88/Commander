namespace commander
{
    public class ReportSectorInfo
    {
        public float Angle
        {
            get
            {
                return Percentage * 360f;
            }
        }
        public string Name;
        public long Length;
        public float Percentage;
        public string Text
        {
            get
            {
                return $"{Name} {Stuff.GetUserFriendlyFileSize(Length)} ({(Percentage * 100).ToString("F")}%)";
            }
        }
        public object Tag;
    }
}
