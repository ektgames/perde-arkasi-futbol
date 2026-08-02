namespace BehindTheScenesFootball.Core
{
    [System.Serializable]
    public class Contract
    {
        public string ClubId;
        public string ClubName;
        public int WeeklyWage;
        public int DurationYears;
        public int ReleaseClause; // 0 means no release clause

        public Contract(string clubId, string clubName, int weeklyWage, int durationYears, int releaseClause = 0)
        {
            ClubId = clubId;
            ClubName = clubName;
            WeeklyWage = weeklyWage;
            DurationYears = durationYears;
            ReleaseClause = releaseClause;
        }
    }
}
