using System;
using System.Collections.Generic;

namespace BehindTheScenesFootball.Core
{
    [System.Serializable]
    public class Club
    {
        public string Id;
        
        [UnityEngine.SerializeField]
        private string _name;
        public string Name
        {
            get { return BehindTheScenesFootball.Managers.LocalizationManager.TranslateClub(_name); }
            set { _name = value; }
        }
        public int Prestige; // 1 to 100, determines attractiveness and target caliber
        
        [UnityEngine.SerializeField]
        private string _league;
        public string League
        {
            get { return BehindTheScenesFootball.Managers.LocalizationManager.TranslateLeague(_league); }
            set { _league = value; }
        }

        public string OriginalName => _name;
        public string OriginalLeague => _league;
        public int TransferBudget;
        public int WageBudget; // Maximum weekly wage budget
        
        // Standings stats
        public int StandingPlayed;
        public int StandingWins;
        public int StandingDraws;
        public int StandingLosses;
        public int StandingGF;
        public int StandingGA;

        public int StandingPoints => (StandingWins * 3) + StandingDraws;
        public int StandingGD => StandingGF - StandingGA;

        public void ResetStandings()
        {
            StandingPlayed = 0;
            StandingWins = 0;
            StandingDraws = 0;
            StandingLosses = 0;
            StandingGF = 0;
            StandingGA = 0;
        }
        
        [System.NonSerialized]
        private List<Player> _roster;
        public List<Player> Roster
        {
            get
            {
                if (_roster == null) _roster = new List<Player>();
                return _roster;
            }
        }

        public Club(string name, int prestige, string league, int transferBudget, int wageBudget)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Prestige = prestige;
            League = league;
            TransferBudget = transferBudget;
            WageBudget = wageBudget;
        }

        public void AddPlayer(Player player, Contract contract)
        {
            if (player.CurrentContract != null)
            {
                // Remove from previous club roster if any
                // Handled externally by DatabaseManager/SimulationEngine
            }
            player.CurrentContract = contract;
            if (BehindTheScenesFootball.Managers.SimulationEngine.Instance != null)
            {
                player.JoinedClubWeek = BehindTheScenesFootball.Managers.SimulationEngine.Instance.CurrentWeek;
                player.JoinedClubYear = BehindTheScenesFootball.Managers.SimulationEngine.Instance.CurrentYear;
            }
            if (!Roster.Contains(player))
            {
                Roster.Add(player);
            }

            // Calculate and assign realistic SquadRole based on player OVR and club Prestige
            int diff = player.OVR - Prestige;
            string role = "İlk 11 Oyuncusu";
            if (player.Age < 21 && player.POT > player.OVR + 12) role = "Genç Yetenek";
            else if (diff >= 12) role = "Yıldız Oyuncu";
            else if (diff >= 5) role = "Önemli Oyuncu";
            else if (diff >= -3) role = "İlk 11 Oyuncusu";
            else if (diff >= -10) role = "Rotasyon Oyuncusu";
            else role = "Yedek Oyuncu";

            player.SquadRole = role;

            player.UpdateMarketValue();
        }

        public void RemovePlayer(Player player)
        {
            Roster.Remove(player);
            player.CurrentContract = null;
            player.SquadRole = "Yok";
            player.UpdateMarketValue();
        }
    }
}
