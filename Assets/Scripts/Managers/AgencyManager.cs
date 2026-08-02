using System.Collections.Generic;
using UnityEngine;
using BehindTheScenesFootball.Core;

namespace BehindTheScenesFootball.Managers
{
    public class AgencyManager : MonoBehaviour
    {
        public static AgencyManager Instance { get; private set; }

        public Agency ActiveAgency;
        public List<string> RecentActivityLog = new List<string>();
        
        [Header("Starting Settings")]
        public string DefaultAgencyName = "Arka Bahçe Menajerlik";
        public long StartingBalance = 10000; // Starting with €10k

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializeAgency(DefaultAgencyName, "Kasey Sung", StartingBalance);
        }

        public void InitializeAgency(string name, string managerName, long startingBalance)
        {
            ActiveAgency = new Agency(name, managerName, startingBalance);
            LogActivity($"'{name}' Şirketi kuruldu. Başlangıç bütçesi: €{startingBalance:N0}");

            // Pre-hire a starting scout so we have one report ready
            Scout startingScout = new Scout("Ahmet Yılmaz", 1);
            startingScout.WeeksRemaining = 0; // Report is ready!
            startingScout.AssignedLeague = "Türkiye 1. Ligi";
            ActiveAgency.HiredScouts.Add(startingScout);

            // Assign a few random starting clients from DatabaseManager as scout results
            StartCoroutine(AssignStartingClients(startingScout));
        }

        private System.Collections.IEnumerator AssignStartingClients(Scout startingScout)
        {
            // Wait until database manager is initialized
            while (DatabaseManager.Instance == null || DatabaseManager.Instance.Players.Count == 0)
            {
                yield return null;
            }

            // Generate 3 custom starting clients who are free agents (no club) and not signed yet
            for (int i = 0; i < 3; i++)
            {
                string nationality;
                string fullName = DatabaseManager.Instance.GenerateFictionalPlayerName("Türkiye", out nationality);
                int age = Random.Range(17, 24);
                PlayerPosition pos = (PlayerPosition)Random.Range(0, 4);
                int ovr = Random.Range(50, 71);
                int pot = ovr + Random.Range(10, 24);
                
                Player p = new Player(fullName, age, pos, ovr, pot);
                p.Nationality = nationality;
                p.CurrentContract = null; // Free Agent (no club)
                p.SquadRole = "Yok";
                p.HasAgent = false;
                p.IsAgencyClient = false; // NOT signed yet!
                
                p.CustomTransferCommissionPercent = 0.15f;
                p.CustomWageCommissionPercent = 0.10f;
                p.CustomSponsorCommissionPercent = 0.15f;
                
                DatabaseManager.Instance.Players.Add(p);
                startingScout.ScoutedPlayerIds.Add(p.Id); // Add to starting scout's report
                LogActivity($"Ajans Başlangıcı: Keşfedilen Serbest Oyuncu {p.Name} ({p.Position}, GEN: {p.OVR}, Yaş: {p.Age}) gözlemci raporuna eklendi.");
            }
        }

        public void CollectWeeklyRevenues()
        {
            long totalRevenue = 0;
            
            foreach (var client in ActiveAgency.Clients)
            {
                // Commission on weekly wage
                if (client.CurrentContract != null)
                {
                    int wageCut = Mathf.RoundToInt(client.CurrentContract.WeeklyWage * client.CustomWageCommissionPercent);
                    totalRevenue += wageCut;
                }

                // Commission on sponsor deals
                if (client.ActiveSponsor != null)
                {
                    int sponsorCut = Mathf.RoundToInt(client.ActiveSponsor.WeeklyIncome * client.CustomSponsorCommissionPercent);
                    totalRevenue += sponsorCut;
                }
            }

            if (totalRevenue > 0)
            {
                ActiveAgency.Balance += totalRevenue;
                LogActivity($"Müşterilerden haftalık €{totalRevenue:N0} ajans komisyonu tahsil edildi.");
            }
        }

        public void CollectTransferCommission(Player player, int fee)
        {
            int commission = Mathf.RoundToInt(fee * player.CustomTransferCommissionPercent);
            ActiveAgency.Balance += commission;
            AddAgencyReputation(20); // Boost reputation by 20 points!
            LogActivity($"KOMİSYON! €{fee:N0} tutarındaki transferden %{(player.CustomTransferCommissionPercent * 100):0} oranında €{commission:N0} kazanıldı ({player.Name}). Ajans İtibarı arttı (+20).");
        }

        public void AddAgencyReputation(int points)
        {
            int oldLevel = ActiveAgency.Level;
            ActiveAgency.AddReputation(points);
            if (ActiveAgency.Level > oldLevel)
            {
                LogActivity($"★★★ TEBRİKLER! Ajansımız SEVİYE {ActiveAgency.Level} oldu! Kapasite: {ActiveAgency.MaxClientsCapacity} oyuncu. ★★★");
            }
        }

        public bool TrySignClient(Player player, float transferPct, float wagePct, float sponsorPct)
        {
            if (player.IsAgencyClient)
            {
                LogActivity($"{player.Name} zaten bu ajans tarafından temsil ediliyor.");
                return false;
            }

             // Client capacity check based on Level
            if (ActiveAgency.Clients.Count >= ActiveAgency.MaxClientsCapacity)
            {
                LogActivity($"Sözleşme başarısız: Şirket kapasitesi dolu ({ActiveAgency.Clients.Count}/{ActiveAgency.MaxClientsCapacity}). Şirket seviyenizi yükseltmelisiniz.");
                return false;
            }

            // Players will only sign if agency level is high enough relative to player OVR
            // Level 1: signs OVR <= 70
            // Level 2: signs OVR <= 78
            // Level 3: signs OVR <= 84
            // Level 4: signs OVR <= 90
            // Level 5: signs OVR <= 99 (all players)
            int allowedOvr = 70;
            if (ActiveAgency.Level == 2) allowedOvr = 78;
            else if (ActiveAgency.Level == 3) allowedOvr = 84;
            else if (ActiveAgency.Level == 4) allowedOvr = 90;
            else if (ActiveAgency.Level >= 5) allowedOvr = 99;

            if (player.OVR > allowedOvr)
            {
                LogActivity($"Sözleşme başarısız: {player.Name} (GEN: {player.OVR}) yüksek seviyeli bir oyuncu. Bu oyuncuyu kazanmak için Şirket Seviyesini yükseltmelisiniz (Şu anki Seviye: {ActiveAgency.Level}, Gereken GEN Sınırı: {allowedOvr}).");
                return false;
            }

            // Apply custom negotiated percentages
            player.CustomTransferCommissionPercent = transferPct;
            player.CustomWageCommissionPercent = wagePct;
            player.CustomSponsorCommissionPercent = sponsorPct;

            ActiveAgency.AddClient(player);
            AddAgencyReputation(8); // Gain +8 reputation for signing a new client!
            LogActivity($"Yeni müşteri kazanıldı: {player.Name} (GEN: {player.OVR}, Yaş: {player.Age}, Trf: %{(transferPct*100):0}, Maaş: %{(wagePct*100):0}, Sp: %{(sponsorPct*100):0}). Ajans itibarı arttı (+8).");
            return true;
        }

        public void TerminateClient(Player player)
        {
            if (ActiveAgency.Clients.Contains(player))
            {
                ActiveAgency.RemoveClient(player);
                ActiveAgency.Reputation = Mathf.Max(0, ActiveAgency.Reputation - 5); // Deduct 5 points on drop
                LogActivity($"Müşteri sözleşmesi tek taraflı feshedildi: {player.Name}. Ajans itibarı düştü (-5).");
            }
        }

        public void LogActivity(string message)
        {
            RecentActivityLog.Insert(0, $"[{System.DateTime.Now:HH:mm:ss}] {message}");
            if (RecentActivityLog.Count > 50)
            {
                RecentActivityLog.RemoveAt(RecentActivityLog.Count - 1);
            }
            Debug.Log(message);
        }
    }
}
