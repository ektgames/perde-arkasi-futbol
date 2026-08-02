using System.Collections.Generic;
using UnityEngine;

namespace BehindTheScenesFootball.Core
{
    [System.Serializable]
    public class Agency
    {
        public string Name;
        public string ManagerName;
        public long Balance; // Money in Euros/Dollars
        public int Level; // Agency level stage (1 to 5)
        public int Reputation; // Agency reputation points (0 to 100 per level)
        public float TransferCommissionPercent; // e.g. 0.10f for 10%
        public float WageCommissionPercent; // e.g. 0.05f for 5%
        public float SponsorCommissionPercent; // e.g. 0.08f for 8%
        public List<string> ClientPlayerIds = new List<string>();
        
        [Header("Starting Settings")]
        [System.NonSerialized]
        private List<Player> _clients;
        public List<Player> Clients
        {
            get
            {
                if (_clients == null) _clients = new List<Player>();
                return _clients;
            }
        }

        public int MaxClientsCapacity
        {
            get
            {
                if (Level == 1) return 5;
                if (Level == 2) return 10;
                if (Level == 3) return 30;
                if (Level == 4) return 40;
                return 50; // Level 5
            }
        }

        public List<Scout> HiredScouts = new List<Scout>();
        public List<string> PurchasedStoreItemIds = new List<string>();

        public Agency(string name, string managerName, long startingBalance)
        {
            Name = name;
            ManagerName = managerName;
            Balance = startingBalance;
            Level = 1;
            Reputation = 0; // Starts at 0, goes up to 100 to level up
            TransferCommissionPercent = 0.10f; // 10% standard transfer fee commission
            WageCommissionPercent = 0.05f; // 5% player weekly wage cut
            SponsorCommissionPercent = 0.10f; // 10% sponsor deal commission
        }

        public void AddReputation(int points)
        {
            Reputation += points;
            while (Reputation >= 100 && Level < 5)
            {
                Reputation -= 100;
                Level++;
                // Level up event is logged via AgencyManager
            }
            if (Level >= 5)
            {
                Reputation = UnityEngine.Mathf.Min(100, Reputation); // Cap reputation at 100 at max level
            }
        }

        public void AddClient(Player player)
        {
            if (!Clients.Contains(player))
            {
                Clients.Add(player);
                player.IsAgencyClient = true;
            }
        }

        public void RemoveClient(Player player)
        {
            if (Clients.Contains(player))
            {
                Clients.Remove(player);
                player.IsAgencyClient = false;
            }
        }
    }

    [System.Serializable]
    public class Scout
    {
        public string Id;
        public string Name;
        public int Level; // 1 to 5
        public string AssignedLeague; // e.g. "Türkiye 1. Ligi"
        public int WeeksRemaining; // counts down from 4 to 0
        public int ReportAgeWeeks; // tracks ready report age
        public List<string> ScoutedPlayerIds = new List<string>();

        public Scout(string name, int level)
        {
            Id = System.Guid.NewGuid().ToString();
            Name = name;
            Level = level;
            AssignedLeague = "";
            WeeksRemaining = 0;
            ReportAgeWeeks = 0;
            ScoutedPlayerIds = new List<string>();
        }
    }
}
