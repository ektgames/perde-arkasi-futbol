namespace BehindTheScenesFootball.Core
{
    [System.Serializable]
    public class Sponsor
    {
        public string BrandName;
        public int WeeklyIncome;
        public int DurationYears;
        public int MinOVRRequired;

        public Sponsor(string brandName, int weeklyIncome, int durationYears, int minOVRRequired)
        {
            BrandName = brandName;
            WeeklyIncome = weeklyIncome;
            DurationYears = durationYears;
            MinOVRRequired = minOVRRequired;
        }
    }
}
