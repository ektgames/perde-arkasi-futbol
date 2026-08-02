using System.Collections.Generic;

namespace BehindTheScenesFootball.Core
{
    [System.Serializable]
    public class League
    {
        [UnityEngine.SerializeField]
        private string _name;
        public string Name
        {
            get { return BehindTheScenesFootball.Managers.LocalizationManager.TranslateLeague(_name); }
            set { _name = value; }
        }

        [UnityEngine.SerializeField]
        private string _country;
        public string Country
        {
            get { return BehindTheScenesFootball.Managers.LocalizationManager.TranslateCountry(_country); }
            set { _country = value; }
        }

        public string OriginalName => _name;
        public string OriginalCountry => _country;

        public int Tier; // 1, 2, 3
        public List<Club> Clubs = new List<Club>();
        public List<MatchFixture> Fixtures = new List<MatchFixture>();

        public League(string name, string country, int tier)
        {
            Name = name;
            Country = country;
            Tier = tier;
        }
    }

    [System.Serializable]
    public class MatchFixture
    {
        public int MatchWeek; // 1 to 34
        public string HomeClubId;
        public string AwayClubId;
        public bool Played;
        public int HomeGoals;
        public int AwayGoals;
        public string LeagueName;

        public MatchFixture(int matchWeek, string homeClubId, string awayClubId, string leagueName)
        {
            MatchWeek = matchWeek;
            HomeClubId = homeClubId;
            AwayClubId = awayClubId;
            Played = false;
            HomeGoals = 0;
            AwayGoals = 0;
            LeagueName = leagueName;
        }
    }
}
