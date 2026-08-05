using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehindTheScenesFootball.Core;

namespace BehindTheScenesFootball.Managers
{
    public class SimulationMail
    {
        public string Id;
        public string Sender;
        public string Subject;
        public string Content;
        public string PlayerId;
        public bool IsRenewalMail;
        public int WeeksLeft;

        // Dynamic Relationship Integration
        public bool IsRequest;
        public bool IsCrisis;
        public string RequestType;
        public int HappinessEffect;
        public long MoneyCost;
    }

    public class SimulatedTransfer
    {
        public string PlayerName;
        public string PlayerId;
        public string FromClubName;
        public string ToClubName;
        public int TransferFee;
        public int WeeklyWage;
        public int Week;
        public int Year;
    }

    public class SimulationEngine : MonoBehaviour
    {
        public static SimulationEngine Instance { get; private set; }

        [Header("Calendar settings")]
        public int CurrentYear = 2026;
        public int CurrentWeek = 1; // 1 to 52 weeks in a season

        [Header("Simulation Speed")]
        public float AutoSimDelay = 0.5f;
        public bool IsAutoSimulating = false;

        public List<TransferOffer> ActiveOffers = new List<TransferOffer>();
        public List<SimulationMail> ActiveMails = new List<SimulationMail>();
        public List<SimulatedTransfer> SeasonTransfers = new List<SimulatedTransfer>();

        private Coroutine autoSimCoroutine;

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
            GenerateSeasonFixtures();
        }

        public void ResetSimulation()
        {
            CurrentYear = 2026;
            CurrentWeek = 1;
            IsAutoSimulating = false;
            if (autoSimCoroutine != null)
            {
                StopCoroutine(autoSimCoroutine);
                autoSimCoroutine = null;
            }
            ActiveOffers.Clear();
            ActiveMails.Clear();
            SeasonTransfers.Clear();
            GenerateSeasonFixtures();
        }

        public bool IsTransferWindowOpen()
        {
            // Transfer window is open in off-season (weeks 35 to 52), early weeks (1 to 4), and mid-season (16 to 20)
            return (CurrentWeek >= 35 && CurrentWeek <= 52) || (CurrentWeek >= 1 && CurrentWeek <= 4) || (CurrentWeek >= 16 && CurrentWeek <= 20);
        }

        public void StartAutoSim()
        {
            if (!IsAutoSimulating)
            {
                IsAutoSimulating = true;
                autoSimCoroutine = StartCoroutine(AutoSimLoop());
                AgencyManager.Instance.LogActivity("Otomatik Simülasyon BAŞLADI.");
            }
        }

        public void StopAutoSim()
        {
            if (IsAutoSimulating)
            {
                IsAutoSimulating = false;
                if (autoSimCoroutine != null)
                {
                    StopCoroutine(autoSimCoroutine);
                }
                AgencyManager.Instance.LogActivity("Otomatik Simülasyon DURDURULDU.");
            }
        }

        private IEnumerator AutoSimLoop()
        {
            while (IsAutoSimulating)
            {
                AdvanceOneWeek();
                yield return new WaitForSeconds(AutoSimDelay);
            }
        }

        public void AdvanceOneWeek()
        {
            // 1. Advance Calendar
            CurrentWeek++;
            if (CurrentWeek > 52)
            {
                CurrentWeek = 1;
                CurrentYear++;
                EndSeasonAll();
            }

            // 2. Simulate League Matches
            SimulateMatchWeek();

            // 3. Player Development
            DevelopPlayers();

            // 4. Financial Commission Ticks
            AgencyManager.Instance.CollectWeeklyRevenues();

            // Tick contract durations
            var clients = new List<Player>(AgencyManager.Instance.ActiveAgency.Clients);
            foreach (var client in clients)
            {
                if (client.AgencyContractRemainingWeeks > 0)
                {
                    client.AgencyContractRemainingWeeks--;
                    
                    // Trigger mail at exactly 26 weeks (6 months) left
                    if (client.AgencyContractRemainingWeeks == 26)
                    {
                        SimulationMail mail = new SimulationMail
                        {
                            Id = System.Guid.NewGuid().ToString(),
                            Sender = client.Name,
                            Subject = $"📩 Temsilcilik Sözleşmesi Yenileme Uyarısı: {client.Name}",
                            Content = $"{client.Name} ile yaptığınız temsilcilik sözleşmesinin bitmesine 6 ay (26 hafta) kaldı! Sözleşmeyi yenilemek ister misiniz?\nEn fazla 5 yıl uzatabilirsiniz. 4. yıldan itibaren (sözleşme süresi bitmeden) yenileme teklifi yapabilirsiniz.",
                            PlayerId = client.Id,
                            IsRenewalMail = true,
                            WeeksLeft = 26
                        };
                        ActiveMails.Add(mail);
                        AgencyManager.Instance.LogActivity($"UYARI! {client.Name} ile olan temsilcilik sözleşmemizin bitmesine 6 ay kaldı. Gelen kutunuza bildirim gönderildi.");
                    }
                    
                    // Expire at 0 weeks
                    if (client.AgencyContractRemainingWeeks <= 0)
                    {
                        AgencyManager.Instance.TerminateClient(client);
                        SimulationMail expireMail = new SimulationMail
                        {
                            Id = System.Guid.NewGuid().ToString(),
                            Sender = "Ajans Bildirim Sistemi",
                            Subject = $"🔴 Temsilcilik Sözleşmesi Sona Erdi: {client.Name}",
                            Content = $"{client.Name} ile olan temsilcilik sözleşmeniz bitti ve oyuncu ajansımızdan ayrıldı.",
                            PlayerId = client.Id,
                            IsRenewalMail = false,
                            WeeksLeft = 0
                        };
                        ActiveMails.Add(expireMail);
                        AgencyManager.Instance.LogActivity($"{client.Name} ile olan temsilcilik sözleşmemiz sona erdi. Oyuncu ajansımızdan ayrıldı.");
                    }
                }
            }

            // Tick loan contracts for all players
            foreach (var p in DatabaseManager.Instance.Players)
            {
                if (p.IsOnLoan && p.LoanRemainingWeeks > 0)
                {
                    p.LoanRemainingWeeks--;
                    if (p.LoanRemainingWeeks <= 0)
                    {
                        p.IsOnLoan = false;
                        Club parentClub = DatabaseManager.Instance.GetClubById(p.ParentClubId);
                        Club tempClub = p.CurrentContract != null ? DatabaseManager.Instance.GetClubById(p.CurrentContract.ClubId) : null;
                        if (parentClub != null)
                        {
                            if (tempClub != null)
                            {
                                tempClub.RemovePlayer(p);
                            }
                            Contract restoredContract = new Contract(parentClub.Id, parentClub.Name, p.CurrentContract != null ? p.CurrentContract.WeeklyWage : 1500, 2);
                            parentClub.AddPlayer(p, restoredContract);
                            
                            if (p.IsAgencyClient)
                            {
                                AgencyManager.Instance.LogActivity($"KİRALIK SONU: Müşteriniz {p.Name} kiralık sözleşmesi bittiği için {parentClub.Name} kulübüne geri döndü.");
                            }
                        }
                    }
                }
            }
 
            // 5. Generate Weekly Client Dialogue Requests (10 Scenarios)
            GenerateWeeklyClientDialogueRequests();
            
            // 4.7. Unhappy Clients Crisis Mail Check
            foreach (var client in AgencyManager.Instance.ActiveAgency.Clients)
            {
                if (client.Happiness < 40f)
                {
                    // 15% chance per week to trigger a crisis mail
                    if (UnityEngine.Random.value < 0.15f)
                    {
                        // Check if a crisis mail for this player is already active to avoid spam
                        bool alreadyHasMail = ActiveMails.Exists(m => m.PlayerId == client.Id && m.Subject.Contains("KRİZ"));
                        if (!alreadyHasMail)
                        {
                            string crisisReason = "";
                            string senderName = client.Name;
                            string subject = $"🚨 KRİZ: {client.Name} Çok Mutsuz!";
                            
                            // Choose reason based on status
                            if (client.CurrentContract == null)
                            {
                                crisisReason = "Kulüpsüz ve boşta kalmaktan dolayı son derece mutsuzum! Menajerim olarak neden bana kulüp bulmuyorsunuz? Acilen bana bir takım ayarlayın, yoksa menajerlik sözleşmemizi tek taraflı feshedeceğim!";
                            }
                            else if (client.Appearances < 5)
                            {
                                crisisReason = $"Kulübüm ({client.CurrentContract.ClubName}) bünyesinde neredeyse hiç süre alamıyorum! Kadro rolümün hakkı verilmiyor. Bana acilen oynayabileceğim yeni bir takım bulmalısınız ya da bu duruma müdahale etmelisiniz!";
                            }
                            else
                            {
                                crisisReason = "Kulübümdeki mevcut konumumdan ve aldığım maaştan ötürü huzursuzum. Bana yeterince ilgi göstermiyorsunuz. Lütfen benimle ilgilenin, moral verin veya prim vererek yanımda olduğunuzu gösterin!";
                            }

                            SimulationMail crisisMail = new SimulationMail
                            {
                                Id = System.Guid.NewGuid().ToString(),
                                Sender = senderName,
                                Subject = subject,
                                Content = crisisReason,
                                PlayerId = client.Id,
                                IsRenewalMail = false,
                                WeeksLeft = 0
                            };
                            ActiveMails.Add(crisisMail);
                            AgencyManager.Instance.LogActivity($"UYARI! Müşteriniz {client.Name} çok mutsuz olduğu için bir kriz maili gönderdi. Gelen kutusunu inceleyin!");
                        }
                    }
                }
            }

            Agency agency = AgencyManager.Instance.ActiveAgency;
            foreach (var scout in agency.HiredScouts)
            {
                // Pay level-based weekly wage
                int scoutWage = 1000;
                switch (scout.Level)
                {
                    case 1: scoutWage = 500; break;
                    case 2: scoutWage = 1000; break;
                    case 3: scoutWage = 1500; break;
                    case 4: scoutWage = 2500; break;
                    case 5: scoutWage = 4000; break;
                }
                agency.Balance = System.Math.Max(0L, agency.Balance - (long)scoutWage);

                if (scout.WeeksRemaining > 0 && !string.IsNullOrEmpty(scout.AssignedLeague))
                {
                    scout.WeeksRemaining--;
                    if (scout.WeeksRemaining == 0)
                    {
                        // Scout finished their mission! Generate report players.
                        // Find players in the assigned league who have no agent.
                        List<Player> candidates = DatabaseManager.Instance.Players.FindAll(p => 
                            !p.IsAgencyClient && 
                            !p.HasAgent && 
                            p.CurrentContract != null &&
                            DatabaseManager.Instance.GetClubById(p.CurrentContract.ClubId) != null &&
                            DatabaseManager.Instance.GetClubById(p.CurrentContract.ClubId).League == scout.AssignedLeague
                        );
 
                        // If not enough candidates, grab any !IsAgencyClient player in this league
                        if (candidates.Count < scout.Level)
                        {
                            candidates = DatabaseManager.Instance.Players.FindAll(p => 
                                !p.IsAgencyClient && 
                                p.CurrentContract != null &&
                                DatabaseManager.Instance.GetClubById(p.CurrentContract.ClubId) != null &&
                                DatabaseManager.Instance.GetClubById(p.CurrentContract.ClubId).League == scout.AssignedLeague
                            );
                        }
 
                        // If still not enough, grab any !IsAgencyClient players in the entire database
                        if (candidates.Count < scout.Level)
                        {
                            candidates = DatabaseManager.Instance.Players.FindAll(p => !p.IsAgencyClient);
                        }
 
                        // Shuffle candidates
                        for (int i = 0; i < candidates.Count; i++)
                        {
                            Player temp = candidates[i];
                            int randomIndex = Random.Range(i, candidates.Count);
                            candidates[i] = candidates[randomIndex];
                            candidates[randomIndex] = temp;
                        }
 
                        // Scout recommends exactly scout.Level players
                        scout.ScoutedPlayerIds.Clear();
                        int recommendedCount = Mathf.Min(scout.Level, candidates.Count);
                        for (int i = 0; i < recommendedCount; i++)
                        {
                            scout.ScoutedPlayerIds.Add(candidates[i].Id);
                        }
                        scout.ReportAgeWeeks = 0;
 
                        AgencyManager.Instance.LogActivity($"GÖZLEMCİ RAPORU HAZIR: {scout.Name} (Seviye {scout.Level}) {scout.AssignedLeague} araştırmasını tamamladı. {recommendedCount} yeni yetenek keşfetti.");
                    }
                }
                else if (scout.WeeksRemaining == 0 && !string.IsNullOrEmpty(scout.AssignedLeague))
                {
                    scout.ReportAgeWeeks++;
                    if (scout.ReportAgeWeeks >= 2)
                    {
                        scout.AssignedLeague = "";
                        scout.ScoutedPlayerIds.Clear();
                        scout.ReportAgeWeeks = 0;
                    }
                }
            }

            // 5. Generate Offers & Interactions
            if (IsTransferWindowOpen())
            {
                GenerateTransferOffers();
                ProcessLoanSuggestions();
                SimulateAITransfers();
            }
            
            GenerateSponsorshipOffers();
            CheckClientEvents();

            // 6. Retirement check (mid-season at Week 26)
            if (CurrentWeek == 26)
            {
                foreach (var player in DatabaseManager.Instance.Players)
                {
                    if (player.Age >= player.RetirementAge && !player.WillRetireAtEndOfSeason)
                    {
                        player.WillRetireAtEndOfSeason = true;
                        
                        // Send mail to the agent if this is their client
                        if (player.IsAgencyClient)
                        {
                            SimulationMail retireMail = new SimulationMail
                            {
                                Id = System.Guid.NewGuid().ToString(),
                                Sender = player.Name,
                                Subject = $"ℹ️ Emeklilik Bildirimi: {player.Name}",
                                Content = $"Müşteriniz {player.Name} ({player.Age} Yaş), bu sezonun sonunda profesyonel futbol kariyerini sonlandıracağını açıkladı. Sezon sonunda ajansımızdan ve oyundan tamamen silinecektir.",
                                PlayerId = player.Id,
                                IsRenewalMail = false,
                                WeeksLeft = 26
                            };
                            ActiveMails.Add(retireMail);
                            AgencyManager.Instance.LogActivity($"EMEKLİLİK KARARI! Müşterimiz {player.Name} sezon sonunda futbolu bırakma kararı aldı.");
                        }
                    }
                }
            }
        }

        public void GenerateSeasonFixtures()
        {
            foreach (var league in DatabaseManager.Instance.Leagues)
            {
                league.Fixtures.Clear();
                int N = league.Clubs.Count;
                if (N != 18) continue; // Safety check

                // Create round-robin pairings (Circle Method)
                for (int round = 0; round < N - 1; round++)
                {
                    for (int i = 0; i < N / 2; i++)
                    {
                        int idx1 = (round + i) % (N - 1);
                        int idx2;
                        if (i == 0)
                        {
                            idx2 = N - 1;
                        }
                        else
                        {
                            idx2 = (round + N - 1 - i) % (N - 1);
                        }

                        Club home = league.Clubs[idx1];
                        Club away = league.Clubs[idx2];

                        // Balance home advantage alternating by round
                        if (round % 2 == 1)
                        {
                            Club temp = home;
                            home = away;
                            away = temp;
                        }

                        // First half of season (Weeks 1-17)
                        MatchFixture firstHalf = new MatchFixture(round + 1, home.Id, away.Id, league.Name);
                        league.Fixtures.Add(firstHalf);

                        // Second half of season (Weeks 18-34) - Swapped home/away
                        MatchFixture secondHalf = new MatchFixture(round + 18, away.Id, home.Id, league.Name);
                        league.Fixtures.Add(secondHalf);
                    }
                }
            }
            AgencyManager.Instance.LogActivity("Tüm liglerin 34 haftalık fikstürü oluşturuldu.");
        }

        private void SimulateMatchWeek()
        {
            if (CurrentWeek < 1 || CurrentWeek > 34)
            {
                // Off-season (weeks 35 to 52)
                return;
            }

            foreach (var league in DatabaseManager.Instance.Leagues)
            {
                var weeklyFixtures = league.Fixtures.FindAll(f => f.MatchWeek == CurrentWeek);
                foreach (var fixture in weeklyFixtures)
                {
                    Club home = DatabaseManager.Instance.GetClubById(fixture.HomeClubId);
                    Club away = DatabaseManager.Instance.GetClubById(fixture.AwayClubId);

                    if (home == null || away == null) continue;

                    // Score based on prestige and home advantage (+8 prestige equivalent)
                    int homePower = home.Prestige + 8;
                    int awayPower = away.Prestige;

                    int homeGoals = SimulateGoals(homePower, awayPower);
                    int awayGoals = SimulateGoals(awayPower, homePower);

                    fixture.HomeGoals = homeGoals;
                    fixture.AwayGoals = awayGoals;
                    fixture.Played = true;

                    // Update club standings
                    home.StandingPlayed++;
                    away.StandingPlayed++;
                    home.StandingGF += homeGoals;
                    away.StandingGF += awayGoals;
                    home.StandingGA += awayGoals;
                    away.StandingGA += homeGoals;

                    if (homeGoals > awayGoals)
                    {
                        home.StandingWins++;
                        away.StandingLosses++;
                    }
                    else if (homeGoals < awayGoals)
                    {
                        away.StandingWins++;
                        home.StandingLosses++;
                    }
                    else
                    {
                        home.StandingDraws++;
                        away.StandingDraws++;
                    }

                    // Simulate players performance for both clubs
                    SimulatePlayerStatsForMatch(home, homeGoals, awayGoals);
                    SimulatePlayerStatsForMatch(away, awayGoals, homeGoals);
                }
            }
        }

        private int SimulateGoals(int attPower, int defPower)
        {
            // Base Poisson-like goals logic
            float lambda = 1.1f + (attPower - defPower) * 0.015f;
            lambda = Mathf.Clamp(lambda, 0.4f, 3.5f);

            // Poisson draw
            float L = Mathf.Exp(-lambda);
            int k = 0;
            float p = 1.0f;
            do
            {
                k++;
                p *= Random.value;
            } while (p > L && k < 10);

            return k - 1;
        }

        private List<Player> GetMatchParticipants(Club club)
        {
            List<Player> participants = new List<Player>();
            
            List<Player> gks = club.Roster.FindAll(p => p.Position == PlayerPosition.GK);
            List<Player> defs = club.Roster.FindAll(p => p.Position == PlayerPosition.DEF);
            List<Player> mids = club.Roster.FindAll(p => p.Position == PlayerPosition.MID);
            List<Player> fwds = club.Roster.FindAll(p => p.Position == PlayerPosition.FWD);

            System.Func<Player, float> getPriority = (p) => {
                float priority = p.OVR;
                if (p.SquadRole == "Yıldız Oyuncu") priority += 25f;
                else if (p.SquadRole == "Önemli Oyuncu") priority += 15f;
                else if (p.SquadRole == "İlk 11 Oyuncusu") priority += 5f;
                else if (p.SquadRole == "Rotasyon Oyuncusu") priority -= 5f;
                else if (p.SquadRole == "Genç Yetenek") priority -= 10f;
                else if (p.SquadRole == "Yedek Oyuncu") priority -= 20f;
                return priority;
            };

            gks.Sort((a, b) => getPriority(b).CompareTo(getPriority(a)));
            defs.Sort((a, b) => getPriority(b).CompareTo(getPriority(a)));
            mids.Sort((a, b) => getPriority(b).CompareTo(getPriority(a)));
            fwds.Sort((a, b) => getPriority(b).CompareTo(getPriority(a)));

            List<Player> starters = new List<Player>();
            
            if (gks.Count > 0) starters.Add(gks[0]);
            
            for (int i = 0; i < Mathf.Min(4, defs.Count); i++) starters.Add(defs[i]);
            for (int i = 0; i < Mathf.Min(4, mids.Count); i++) starters.Add(mids[i]);
            for (int i = 0; i < Mathf.Min(2, fwds.Count); i++) starters.Add(fwds[i]);

            participants.AddRange(starters);

            // Substitutes: select up to 3 from remaining backups
            List<Player> backups = new List<Player>();
            foreach (var p in club.Roster)
            {
                if (!starters.Contains(p))
                {
                    backups.Add(p);
                }
            }

            if (backups.Count > 0)
            {
                Dictionary<string, float> scores = new Dictionary<string, float>();
                foreach (var b in backups)
                {
                    scores[b.Id] = getPriority(b) * Random.Range(0.6f, 1.4f);
                }
                
                backups.Sort((x, y) => scores[y.Id].CompareTo(scores[x.Id]));
                
                int subsCount = Mathf.Min(3, backups.Count);
                for (int i = 0; i < subsCount; i++)
                {
                    participants.Add(backups[i]);
                }
            }

            return participants;
        }

        private void SimulatePlayerStatsForMatch(Club club, int goalsScored, int goalsConceded)
        {
            bool cleanSheet = goalsConceded == 0;
            List<Player> players = GetMatchParticipants(club);

            // Playtime happiness decay for roster players who did not play
            foreach (var p in club.Roster)
            {
                if (!players.Contains(p))
                {
                    if (p.SquadRole == "Yıldız Oyuncu")
                    {
                        p.Happiness = Mathf.Clamp(p.Happiness - 5f, 0f, 100f);
                    }
                    else if (p.SquadRole == "Önemli Oyuncu")
                    {
                        p.Happiness = Mathf.Clamp(p.Happiness - 3f, 0f, 100f);
                    }
                    else if (p.SquadRole == "İlk 11 Oyuncusu")
                    {
                        p.Happiness = Mathf.Clamp(p.Happiness - 2f, 0f, 100f);
                    }
                }
            }

            // Distribute goals and assists among FWDs and MIDs weighted by OVR and SquadRole
            List<Player> goalScorers = new List<Player>();
            List<Player> assistProviders = new List<Player>();

            foreach (var p in players)
            {
                int roleWeight = 1;
                if (p.SquadRole == "Yıldız Oyuncu") roleWeight = 4;
                else if (p.SquadRole == "Önemli Oyuncu") roleWeight = 3;
                else if (p.SquadRole == "İlk 11 Oyuncusu") roleWeight = 2;
                else if (p.SquadRole == "Rotasyon Oyuncusu") roleWeight = 1;

                if (p.Position == PlayerPosition.FWD)
                {
                    for (int i = 0; i < 3 * roleWeight; i++) goalScorers.Add(p);
                    for (int i = 0; i < roleWeight; i++) assistProviders.Add(p);
                }
                else if (p.Position == PlayerPosition.MID)
                {
                    for (int i = 0; i < roleWeight; i++) goalScorers.Add(p);
                    for (int i = 0; i < 2 * roleWeight; i++) assistProviders.Add(p);
                }
                else if (p.Position == PlayerPosition.DEF)
                {
                    for (int i = 0; i < roleWeight; i++) assistProviders.Add(p);
                }
            }

            Dictionary<Player, int> matchGoals = new Dictionary<Player, int>();
            Dictionary<Player, int> matchAssists = new Dictionary<Player, int>();

            foreach (var p in players)
            {
                matchGoals[p] = 0;
                matchAssists[p] = 0;
            }

            if (goalScorers.Count > 0)
            {
                for (int g = 0; g < goalsScored; g++)
                {
                    Player scorer = goalScorers[Random.Range(0, goalScorers.Count)];
                    matchGoals[scorer]++;
                }
            }

            if (assistProviders.Count > 0)
            {
                for (int a = 0; a < goalsScored; a++)
                {
                    if (Random.value < 0.7f)
                    {
                        Player provider = assistProviders[Random.Range(0, assistProviders.Count)];
                        matchAssists[provider]++;
                    }
                }
            }

            foreach (var player in players)
            {
                // Generate performance rating based on OVR, form, and contribution
                float baseRating = 6.0f + (player.OVR - 50) * 0.025f + (player.Form - 70) * 0.015f;

                // Happiness-based performance modifier
                if (player.Happiness >= 80f) baseRating += 0.6f;
                else if (player.Happiness < 40f) baseRating -= 0.8f;
                
                baseRating += matchGoals[player] * 1.2f;
                baseRating += matchAssists[player] * 0.6f;

                if (player.Position == PlayerPosition.GK || player.Position == PlayerPosition.DEF)
                {
                    if (cleanSheet) baseRating += 0.8f;
                    else baseRating -= goalsConceded * 0.2f;
                }

                float matchRating = Mathf.Clamp(baseRating + Random.Range(-1.0f, 1.5f), 4.5f, 10.0f);
                player.PlayMatch(matchRating, matchGoals[player], matchAssists[player], cleanSheet);

                // If player is represented by us, award reputation on good performance!
                if (player.IsAgencyClient)
                {
                    if (matchRating >= 8.5f)
                    {
                        AgencyManager.Instance.AddAgencyReputation(2);
                        AgencyManager.Instance.LogActivity($"HARİKA PERFORMANS: Müşterimiz {player.Name} harika bir performans gösterdi (Puan: {matchRating:0.0}). Ajans itibarı arttı (+2).");
                    }
                    else if (matchRating >= 7.5f)
                    {
                        AgencyManager.Instance.AddAgencyReputation(1);
                        AgencyManager.Instance.LogActivity($"İYİ PERFORMANS: Müşterimiz {player.Name} iyi bir maç çıkardı (Puan: {matchRating:0.0}). Ajans itibarı arttı (+1).");
                    }
                }
            }
        }

        private void DevelopPlayers()
        {
            foreach (var player in DatabaseManager.Instance.Players)
            {
                player.Develop();
            }
        }

        private void GenerateTransferOffers()
        {
            foreach (var client in AgencyManager.Instance.ActiveAgency.Clients)
            {
                if (client.CurrentContract == null) continue;
                if (ActiveOffers.Exists(o => o.PlayerId == client.Id)) continue;

                // Track 6 months (26 weeks) stay requirement:
                int elapsedWeeks = (CurrentYear - client.JoinedClubYear) * 52 + (CurrentWeek - client.JoinedClubWeek);
                if (elapsedWeeks < 26) continue;

                float interestChance = 0.02f + (client.AverageRating - 6.0f) * 0.05f + (client.OVR - 60) * 0.005f;
                if (client.OVR > 85) interestChance += 0.05f;

                if (Random.value < interestChance)
                {
                    Club bidder = FindSuitableClubForPlayer(client);
                    if (bidder != null && bidder.Id != client.CurrentContract.ClubId)
                    {
                        int bid = Mathf.RoundToInt(client.MarketValue * Random.Range(0.9f, 1.4f));
                        if (bid > bidder.TransferBudget)
                        {
                            bid = Mathf.RoundToInt(bidder.TransferBudget * 0.7f);
                        }

                        if (bid >= client.MarketValue * 0.6f && bid > 10000)
                        {
                            bool isAcceptedByClub = (bid >= client.MarketValue * 0.90f);

                            if (isAcceptedByClub)
                            {
                                int contractLength = Random.Range(2, 6);
                                int proposedWage = Mathf.RoundToInt(client.CurrentContract.WeeklyWage * Random.Range(1.05f, 1.25f));

                                TransferOffer offer = new TransferOffer(
                                    System.Guid.NewGuid().ToString(),
                                    client.Id,
                                    client.Name,
                                    bidder.Id,
                                    bidder.Name,
                                    client.CurrentContract.ClubId,
                                    client.CurrentContract.ClubName,
                                    bid,
                                    proposedWage,
                                    contractLength
                                );

                                ActiveOffers.Add(offer);
                                AgencyManager.Instance.LogActivity($"KULÜP KABUL ETTİ: {client.CurrentContract.ClubName}, {bidder.Name} kulübünün {client.Name} için yaptığı €{bid:N0} bonservis teklifini kabul etti! Oyuncu sözleşmesi görüşülebilir.");
                            }
                            else
                            {
                                AgencyManager.Instance.LogActivity($"TEKLİF REDDEDİLDİ: {client.CurrentContract.ClubName}, {bidder.Name} kulübünün {client.Name} için yaptığı €{bid:N0} bonservis teklifini düşük bularak reddetti.");
                            }
                        }
                    }
                }
            }
        }

        private Club FindSuitableClubForPlayer(Player player)
        {
            List<Club> candidates = new List<Club>();
            int targetPrestige = Mathf.Clamp(Mathf.RoundToInt(player.OVR * 0.9f + Random.Range(-10, 10)), 10, 100);

            foreach (var club in DatabaseManager.Instance.Clubs)
            {
                if (club.TransferBudget > player.MarketValue * 0.5f)
                {
                    int prestigeDiff = Mathf.Abs(club.Prestige - targetPrestige);
                    if (prestigeDiff <= 25)
                    {
                        candidates.Add(club);
                    }
                }
            }

            if (candidates.Count > 0)
            {
                return candidates[Random.Range(0, candidates.Count)];
            }
            return null;
        }

        private Club FindSuitableLoanClubForPlayer(Player player)
        {
            List<Club> candidates = new List<Club>();
            foreach (var club in DatabaseManager.Instance.Clubs)
            {
                if (club.Id == player.CurrentContract.ClubId) continue;
                int diff = player.OVR - club.Prestige;
                if (diff >= -3 && diff <= 15)
                {
                    candidates.Add(club);
                }
            }
            if (candidates.Count > 0)
            {
                return candidates[Random.Range(0, candidates.Count)];
            }
            return null;
        }
 
        private void ProcessLoanSuggestions()
        {
            var clients = new List<Player>(AgencyManager.Instance.ActiveAgency.Clients);
            foreach (var client in clients)
            {
                if (client.IsSuggestedForLoan)
                {
                    int offerCount = Random.Range(1, 3); // en az 1, en fazla 2
                    for (int i = 0; i < offerCount; i++)
                    {
                        Club bidder = FindSuitableLoanClubForPlayer(client);
                        if (bidder != null && bidder.Id != client.CurrentContract.ClubId)
                        {
                            TransferOffer loanOffer = new TransferOffer(
                                System.Guid.NewGuid().ToString(),
                                client.Id,
                                client.Name,
                                bidder.Id,
                                bidder.Name,
                                client.CurrentContract.ClubId,
                                client.CurrentContract.ClubName,
                                0, // 0 transfer fee
                                client.CurrentContract.WeeklyWage,
                                Random.Range(1, 3) // 1 to 2 years duration
                            );
                            loanOffer.IsLoanOffer = true;
                            ActiveOffers.Add(loanOffer);
                            AgencyManager.Instance.LogActivity($"KİRALIK TEKLİFİ: {bidder.Name}, {client.Name} için kiralık teklifinde bulundu!");
                        }
                    }
                    client.IsSuggestedForLoan = false;
                }
            }
        }

        private void GenerateSponsorshipOffers()
        {
            foreach (var client in AgencyManager.Instance.ActiveAgency.Clients)
            {
                if (client.ActiveSponsor != null) continue;

                // Success check: player has played and rating is at least 6.4 (successful in their club!)
                bool isSuccessful = client.CurrentContract != null && client.Appearances > 0 && client.AverageRating >= 6.4f;
                if (!isSuccessful) continue;

                // 8% chance per week to get a sponsor offer if successful
                if (Random.value < 0.08f)
                {
                    List<Sponsor> candidates = DatabaseManager.Instance.Sponsors.FindAll(s => client.OVR >= s.MinOVRRequired);
                    if (candidates.Count > 0)
                    {
                        Sponsor targetSponsor = candidates[Random.Range(0, candidates.Count)];
                        
                        // Check if they already have a pending offer from this brand
                        if (!client.PendingSponsorOffers.Exists(o => o.BrandName == targetSponsor.BrandName))
                        {
                            int baseWage = Mathf.RoundToInt(targetSponsor.WeeklyIncome * Random.Range(0.85f, 1.25f));
                            int defaultDuration = Random.Range(1, 4);
                            
                            Sponsor offer = new Sponsor(targetSponsor.BrandName, baseWage, defaultDuration, targetSponsor.MinOVRRequired);
                            client.PendingSponsorOffers.Add(offer);

                            // Send mail notification
                            SimulationMail mail = new SimulationMail
                            {
                                Id = System.Guid.NewGuid().ToString(),
                                Sender = $"{targetSponsor.BrandName} Sponsorluk Departmanı",
                                Subject = $"💼 SPONSORLUK TEKLİFİ: {client.Name}",
                                Content = $"Sayın Menajer,\n\nOyuncunuz {client.Name} kulübündeki başarılı performansıyla markamızın dikkatini çekmiştir. Kendisine haftalık taban €{baseWage:N0} bütçeli bir sponsorluk anlaşması sunmak istiyoruz. Detaylı görüşme ve imza işlemleri için lütfen oyuncunun profil sayfasındaki 'Sponsorluk Teklifleri' menüsünü ziyaret edin.\n\nSaygılarımızla,\n{targetSponsor.BrandName}",
                                PlayerId = client.Id,
                                IsRenewalMail = false,
                                WeeksLeft = 0
                            };
                            ActiveMails.Add(mail);
                            AgencyManager.Instance.LogActivity($"TEKLİF: Müşteriniz {client.Name} için {targetSponsor.BrandName} markasından sponsorluk teklifi geldi!");
                        }
                    }
                }
            }
        }

        private void CheckClientEvents()
        {
            foreach (var client in AgencyManager.Instance.ActiveAgency.Clients)
            {
                ProcessDynamicInteractions(client);
            }
        }

        private void ProcessDynamicInteractions(Player client)
        {
            // 1. Request Generation (5% chance per week)
            if (Random.value < 0.05f)
            {
                // To avoid spam, don't trigger if they already have an active request mail
                bool hasActiveRequest = ActiveMails.Exists(m => m.PlayerId == client.Id && m.IsRequest);
                if (!hasActiveRequest)
                {
                    GenerateRequestMail(client);
                }
            }

            // 2. Sudden Crisis/Event Generation (4% chance per week, morale-independent)
            if (Random.value < 0.04f)
            {
                // To avoid spam, don't trigger if they already have an active crisis mail
                bool hasActiveCrisis = ActiveMails.Exists(m => m.PlayerId == client.Id && m.IsCrisis);
                if (!hasActiveCrisis)
                {
                    GenerateCrisisEvent(client);
                }
            }
        }

        private void GenerateRequestMail(Player client)
        {
            string subject = "";
            string content = "";
            string reqType = "";
            int effect = 20;
            long cost = 0;

            if (client.Happiness >= 75f) // High Morale Requests
            {
                int r = Random.Range(0, 4);
                if (r == 0) // PR
                {
                    reqType = "PR";
                    subject = $"📣 PR Kampanyası İsteği: {client.Name}";
                    content = $"Menajerim, son zamanlardaki harika performansım ve yüksek moralim sayesinde sosyal medyada büyük ilgi görüyorum. Bu rüzgarı arkamıza alıp marka değerimi yükseltmek için profesyonel bir PR ajansıyla çalışmak harika olur. PR bütçesi için €5.000 ayırabilir miyiz?";
                    cost = 5000;
                    effect = 15;
                }
                else if (r == 1) // Coach
                {
                    reqType = "Coach";
                    subject = $"🏋️ Özel Bireysel Antrenör Talebi: {client.Name}";
                    content = $"Harika durumdayım ve sınırlarımı daha da zorlamak istiyorum! Kendimi fiziksel olarak bir üst seviyeye taşımak adına bireysel atletik antrenör tutmak istiyorum. Aylık €8.000 bütçeyi ajansımızın üstlenmesini rica ediyorum.";
                    cost = 8000;
                    effect = 25;
                }
                else if (r == 2) // Charity
                {
                    reqType = "Charity";
                    subject = $"🤝 Sosyal Sorumluluk Projesi Bağışı: {client.Name}";
                    content = $"Moralim çok yüksek ve bu olumlu enerjiyi topluma aktarmak istiyorum. Benim adıma çocuk esirgeme kurumuna €6.000 bağış yapıp bunu basına duyurursak hem prestijimiz artar hem de taraftarlarla bağımız güçlenir.";
                    cost = 6000;
                    effect = 20;
                }
                else // Bonus
                {
                    reqType = "Bonus";
                    subject = $"💰 Performans Sadakat Primi İsteği: {client.Name}";
                    content = $"Menajerim, takıma kattığım yüksek katma değer ve moral sayesinde kendimi özel hissediyorum. Sözleşmemize ekstra €10.000 sadakat/imza primi eklemenizi talep ediyorum.";
                    cost = 10000;
                    effect = 20;
                }
            }
            else if (client.Happiness >= 45f) // Normal Morale Requests
            {
                int r = Random.Range(0, 3);
                if (r == 0) // Boots
                {
                    reqType = "Boots";
                    subject = $"👟 Özel Tasarım Krampon Desteği: {client.Name}";
                    content = $"Menajerim, yeni sezon için performansımı artıracak özel karbon tabanlı kramponlar sipariş ettim. €1.500 tutarındaki faturayı ajansımın ödemesini rica ediyorum.";
                    cost = 1500;
                    effect = 15;
                }
                else if (r == 1) // Home
                {
                    reqType = "Home";
                    subject = $"🏠 Taşınma ve Ev Desteği: {client.Name}";
                    content = $"Tesislerimize daha yakın ve sessiz bir muhite taşınmaya karar verdim. Nakliye ve emlakçı komisyonu gibi taşınma masraflarım için ajansımdan €3.000 destek talep ediyorum.";
                    cost = 3000;
                    effect = 15;
                }
                else // Physio
                {
                    reqType = "Physio";
                    subject = $"🩺 Özel Fizyoterapist Desteği: {client.Name}";
                    content = $"Kas sakatlıklarından korunmak ve maç sonu toparlanma süremi hızlandırmak için özel bir fizyoterapist ile anlaştım. Haftalık seans bedeli olan €4.000 tutarını karşılamanızı bekliyorum.";
                    cost = 4000;
                    effect = 15;
                }
            }
            else // Low Morale Requests
            {
                int r = Random.Range(0, 3);
                if (r == 0) // Vacation
                {
                    reqType = "Vacation";
                    subject = $"✈️ Mental İzin ve Aile Ziyareti Desteği: {client.Name}";
                    content = $"Son zamanlarda kendimi hiç iyi hissetmiyorum. Sahadaki form düşüklüğüm beni yıprattı. Hafta sonu kafa dağıtmak ve ailemi ziyaret etmek için bana özel uçuş ve tatil planı hazırlamanızı rica ediyorum. Maliyeti €2.000.";
                    cost = 2000;
                    effect = 15;
                }
                else if (r == 1) // Wage
                {
                    reqType = "Wage";
                    subject = $"💼 Maaş İyileştirme Görüşmesi Talebi: {client.Name}";
                    content = $"Mevcut kulübümden aldığım maaşın yetersiz kaldığını düşünüyorum ve bu durum moralimi bozuyor. Menajerim olarak kulüple acilen masaya oturup maaşıma zam istemenizi talep ediyorum.";
                    cost = 0;
                    effect = 20;
                }
                else // Sponsor
                {
                    reqType = "Sponsor";
                    subject = $"🤝 Yeni Sponsor Arayışı Talebi: {client.Name}";
                    content = $"Ekstra gelir elde edememek canımı sıkıyor. Bana acilen yeni bir sponsor markası bulmanızı istiyorum, marka elçisi olarak kendimi göstermeye hazırım.";
                    cost = 0;
                    effect = 15;
                }
            }

            SimulationMail mail = new SimulationMail
            {
                Id = System.Guid.NewGuid().ToString(),
                Sender = client.Name,
                Subject = subject,
                Content = content,
                PlayerId = client.Id,
                IsRenewalMail = false,
                IsRequest = true,
                IsCrisis = false,
                RequestType = reqType,
                HappinessEffect = effect,
                MoneyCost = cost
            };
            ActiveMails.Add(mail);
            AgencyManager.Instance.LogActivity($"TALEPLER: Müşteriniz {client.Name} sizden bir talepte bulundu! Gelen kutusunu inceleyin.");
        }

        private void GenerateCrisisEvent(Player client)
        {
            int r = Random.Range(0, 20);
            string subject = "";
            string content = "";
            int effect = 0;

            switch (r)
            {
                case 0:
                    subject = $"🚨 GECE KULÜBÜ OLAYI: {client.Name}";
                    content = $"Müşteriniz {client.Name}, dün gece şehir merkezindeki bir gece kulübünde kavgaya karıştı ve karakola götürüldü. Kulüp yönetimi oyuncuya disiplin cezası uyguladı ve morali son derece bozuk.";
                    effect = -25;
                    break;
                case 1:
                    subject = $"🩹 SAKATLIK DEPRESYONU: {client.Name}";
                    content = $"Oyuncunuz {client.Name}, antrenmanda dizinden ciddi bir sakatlık yaşadı. Doktorlar sahalardan bir süre uzak kalacağını belirtti. Oyuncu bu durumdan dolayı psikolojik olarak çökmüş durumda.";
                    effect = -30;
                    break;
                case 2:
                    subject = $"🤬 TARAFTAR LİNCİ: {client.Name}";
                    content = $"Son maçta kaçırdığı net pozisyonların ardından taraftarlar sosyal medyada {client.Name} için linç kampanyası başlattı. Oyuncu gelen hakaretler yüzünden yorumları kapatmak zorunda kaldı.";
                    effect = -20;
                    break;
                case 3:
                    subject = $"⚠️ HOCA İLE TARTIŞMA: {client.Name}";
                    content = $"Oyuncunuz {client.Name}, son antrenmanda teknik direktörün taktik kararlarını herkesin önünde eleştirdiği için kadro dışı bırakıldı. Süresiz olarak altyapıyla antrenmanlara çıkacak.";
                    effect = -25;
                    break;
                case 4:
                    subject = $"🥊 ANTREMANDA KAVGA: {client.Name}";
                    content = $"Dünkü idmanda çift kale maç sırasında {client.Name}, takım arkadaşıyla sert bir ikili mücadeleye girdi ve kavga çıktı. Kulüp her iki oyuncuya da ağır para cezası kesti.";
                    effect = -15;
                    break;
                case 5:
                    subject = $"❌ MİLLİ TAKIM HAYAL KIRIKLIĞI: {client.Name}";
                    content = $"Milli takımın son aday kadrosu açıklandı ve {client.Name} davet edilmedi. Çok uzun süredir bu kadroyu bekleyen oyuncunuz büyük bir motivasyon kaybı yaşıyor.";
                    effect = -15;
                    break;
                case 6:
                    subject = $"📹 SOSYAL MEDYA SKANDALI: {client.Name}";
                    content = $"Oyuncunuz {client.Name}, dün katıldığı canlı yayında mikrofonun açık olduğunu unutarak kulüp yöneticileri hakkında argo ifadeler kullandı. Yönetim acil disiplin kurulu topladı.";
                    effect = -20;
                    break;
                case 7:
                    subject = $"🗞️ Dedikodular: {client.Name}";
                    content = $"Medyada {client.Name}'in kulüpten ayrılmak istediğine dair asılsız iddialar yer aldı. Oyuncu kulüple taraftar arasında kalmaktan dolayı büyük bir zihinsel yıpranma yaşıyor.";
                    effect = -15;
                    break;
                case 8:
                    subject = $"💔 ÖZEL HAYAT KRİZİ: {client.Name}";
                    content = $"Oyuncunuz {client.Name}, uzun süredir birlikte olduğu kız arkadaşıyla yollarını ayırdı. Antrenmanlarda konsantre olmakta büyük zorluk çekiyor.";
                    effect = -20;
                    break;
                case 9:
                    subject = $"⚖️ KONDİSYON VE KİLO ELEŞTİRİLERİ: {client.Name}";
                    content = $"Spor basını, son maçlarda {client.Name}'in kilo aldığını ve fiziksel olarak çok geride kaldığını yazdı. Oyuncunun özgüveni sarsılmış durumda.";
                    effect = -15;
                    break;
                case 10:
                    subject = $"💉 DOPİNG TESTİ GERGİNLİĞİ: {client.Name}";
                    content = $"Rutin doping kontrolü sırasında {client.Name}'in numunesinde teknik bir hata oluştu ve test tekrarlandı. Aklanmış olsa da oyuncu süreç boyunca ciddi bir panik yaşadı.";
                    effect = -10;
                    break;
                case 11:
                    subject = $"✈️ MEMLEKET HASRETİ: {client.Name}";
                    content = $"Şehirdeki yeni yaşamına ve takımın kültürüne bir türlü adapte olamayan {client.Name}, memleket hasreti çektiğini ve yalnız hissettiğini sizinle paylaştı.";
                    effect = -15;
                    break;
                case 12:
                    subject = $"🪑 YEDEK KULÜBESİNDE UNUTULMA: {client.Name}";
                    content = $"Haftalardır yedek kulübesinden çıkamayan {client.Name}, artık teknik direktörün kendisini tamamen gözden çıkardığını düşünüyor ve pes etme aşamasında.";
                    effect = -25;
                    break;
                case 13:
                    subject = $"🌟 HAFTANIN 11'İNE SEÇİLME SEVİNCİ: {client.Name}";
                    content = $"Harika! Oyuncunuz {client.Name}, sergilediği üstün performansla ligde haftanın altın 11'ine seçildi. Morali ve kendine güveni zirve yapmış durumda.";
                    effect = 25;
                    break;
                case 14:
                    subject = $"🎁 SPONSOR JESTİ: {client.Name}";
                    content = $"Aktif sponsor markası, {client.Name}'in son dönemdeki profesyonel duruşunu ödüllendirerek kendisine lüks bir saat hediye etti. Oyuncumuzun keyfi yerinde.";
                    effect = 15;
                    break;
                case 15:
                    subject = $"🚗 MADDİ HASARLI TRAFİK KAZASI: {client.Name}";
                    content = $"Oyuncunuz {client.Name}, sabah antrenmanına gelirken ufak bir zincirleme kazaya karıştı. Fiziksel bir hasarı yok ancak psikolojik olarak sarsıldı.";
                    effect = -10;
                    break;
                case 16:
                    subject = $"🏠 EVİNE HIRSIZ GİRMESİ ŞOKU: {client.Name}";
                    content = $"Takımla deplasmandayken {client.Name}'in evine hırsız girdi ve değerli eşyaları çalındı. Oyuncu güvenlik endişesi yüzünden çok huzursuz.";
                    effect = -25;
                    break;
                case 17:
                    subject = $"🎤 TEKNİK DİREKTÖRDEN ÖVGÜ: {client.Name}";
                    content = $"Teknik direktör dünkü basın toplantısında {client.Name} hakkında övgü dolu sözler sarf ederek onun takımın geleceği olduğunu vurguladı.";
                    effect = 20;
                    break;
                case 18:
                    subject = $"⚽ KARİYER REKORU VE HAT-TRICK: {client.Name}";
                    content = $"İnanılmaz! Oyuncunuz {client.Name}, son maçta 3 gol birden atarak kariyerinin ilk hat-trick'ine imza attı. Taraftarlar onun adını haykırıyor.";
                    effect = 30;
                    break;
                case 19:
                    subject = $"🤢 GIDA ZEHİRLENMESİ: {client.Name}";
                    content = $"Takım yemeği sonrası şiddetli mide ağrısıyla hastaneye kaldırılan {client.Name}'e gıda zehirlenmesi teşhisi konuldu. Hafta sonu maçta oynaması zor görünüyor.";
                    effect = -15;
                    break;
            }

            client.Happiness = Mathf.Clamp(client.Happiness + effect, 10f, 100f);

            SimulationMail mail = new SimulationMail
            {
                Id = System.Guid.NewGuid().ToString(),
                Sender = "Ajans Olay Bildirimi",
                Subject = subject,
                Content = content,
                PlayerId = client.Id,
                IsRenewalMail = false,
                IsRequest = false,
                IsCrisis = true,
                RequestType = "",
                HappinessEffect = effect,
                MoneyCost = 0
            };
            ActiveMails.Add(mail);
            
            if (effect < 0)
            {
                AgencyManager.Instance.LogActivity($"KRİZ! Müşteriniz {client.Name} bir sorunla karşılaştı (Moral etkisi: {effect}). Gelen kutunuzu kontrol edin.");
            }
            else
            {
                AgencyManager.Instance.LogActivity($"GELİŞME! Müşteriniz {client.Name} hakkında olumlu bir olay yaşandı (Moral etkisi: +{effect}). Gelen kutunuzu kontrol edin.");
            }
        }

        private void EndSeasonAll()
        {
            string seasonStr = $"{CurrentYear - 1}-{CurrentYear}";
            AgencyManager.Instance.LogActivity($"*** SEZON SONU: {seasonStr} ***");

            // Process retirements
            List<Player> retiringPlayers = DatabaseManager.Instance.Players.FindAll(p => p.WillRetireAtEndOfSeason);
            foreach (var rp in retiringPlayers)
            {
                if (rp.IsAgencyClient)
                {
                    AgencyManager.Instance.ActiveAgency.RemoveClient(rp);
                }
                
                if (rp.CurrentContract != null)
                {
                    Club c = DatabaseManager.Instance.GetClubById(rp.CurrentContract.ClubId);
                    if (c != null)
                    {
                        c.RemovePlayer(rp);
                        // Generate replacement young Wonderkid!
                        Player regen = DatabaseManager.Instance.GenerateRegenPlayer(rp.Position, c);
                        DatabaseManager.Instance.Players.Add(regen);
                    }
                }
                
                DatabaseManager.Instance.Players.Remove(rp);
                AgencyManager.Instance.LogActivity($"EMEKLİ OLDU: {rp.Name} futbolu bıraktı ve sistemden silindi. Yerine genç bir oyuncu üretildi.");
            }

            foreach (var player in DatabaseManager.Instance.Players)
            {
                Club c = player.CurrentContract != null ? DatabaseManager.Instance.GetClubById(player.CurrentContract.ClubId) : null;
                player.EndSeason(seasonStr, c != null ? c.Name : "Free Agent");

                if (player.CurrentContract != null && player.CurrentContract.DurationYears <= 0)
                {
                    if (c != null)
                    {
                        c.RemovePlayer(player);
                        AgencyManager.Instance.LogActivity($"Sözleşme Bitti: {player.Name} artık Serbest Oyuncu ({c.Name} kulübünden ayrıldı).");
                    }
                }
            }

            foreach (var club in DatabaseManager.Instance.Clubs)
            {
                club.ResetStandings();
                club.TransferBudget = Mathf.RoundToInt(club.TransferBudget * 0.8f + (club.Prestige * 500000));
            }
            
            ActiveOffers.Clear();
            SeasonTransfers.Clear();
            GenerateSeasonFixtures();
        }

        public void AcceptTransferOffer(string offerId)
        {
            TransferOffer offer = ActiveOffers.Find(o => o.Id == offerId);
            if (offer != null)
            {
                Player player = DatabaseManager.Instance.GetPlayerById(offer.PlayerId);
                Club bidderClub = DatabaseManager.Instance.GetClubById(offer.BidderClubId);

                if (player != null && bidderClub != null)
                {
                    if (offer.IsLoanOffer)
                    {
                        player.IsOnLoan = true;
                        player.ParentClubId = player.CurrentContract != null ? player.CurrentContract.ClubId : "";
                        player.ParentClubName = player.CurrentContract != null ? player.CurrentContract.ClubName : "Serbest";
                        player.LoanRemainingWeeks = offer.ContractLengthYears * 52;
                    }
                    else
                    {
                        player.IsOnLoan = false;
                        player.ParentClubId = null;
                        player.ParentClubName = null;
                        player.LoanRemainingWeeks = 0;
                    }

                    Contract newContract = new Contract(bidderClub.Id, bidderClub.Name, offer.OfferedWeeklyWage, offer.ContractLengthYears);
                    int fee = offer.TransferFee;
                    DatabaseManager.Instance.TransferPlayer(player, bidderClub, newContract, fee);

                    if (offer.IsLoanOffer)
                    {
                        AgencyManager.Instance.LogActivity($"KİRALAMA BAŞARILI: Müşteriniz {player.Name}, {offer.ContractLengthYears} yıllığına {bidderClub.Name} kulübüne kiralandı!");
                    }
                    else
                    {
                        AgencyManager.Instance.CollectTransferCommission(player, fee);
                    }
                    ActiveOffers.RemoveAll(o => o.PlayerId == player.Id);
                }
            }
        }

        public void RejectTransferOffer(string offerId)
        {
            TransferOffer offer = ActiveOffers.Find(o => o.Id == offerId);
            if (offer != null)
            {
                ActiveOffers.Remove(offer);
                AgencyManager.Instance.LogActivity($"{offer.PlayerName} için gelen transfer teklifi reddedildi.");
            }
        }

        private void SimulateAITransfers()
        {
            if (!IsTransferWindowOpen()) return;

            // 2 to 5 random transfers per week
            int transferCount = Random.Range(2, 6);
            for (int t = 0; t < transferCount; t++)
            {
                Club buyer = DatabaseManager.Instance.Clubs[Random.Range(0, DatabaseManager.Instance.Clubs.Count)];
                if (buyer.TransferBudget < 20000) continue;

                List<Player> targetCandidates = DatabaseManager.Instance.Players.FindAll(p => 
                    !p.IsAgencyClient && 
                    p.CurrentContract != null && 
                    p.CurrentContract.ClubId != buyer.Id
                );

                if (targetCandidates.Count == 0) continue;

                Player candidate = targetCandidates[Random.Range(0, targetCandidates.Count)];
                Club seller = DatabaseManager.Instance.GetClubById(candidate.CurrentContract.ClubId);
                if (seller == null) continue;

                // Realistic logic: 
                // Prestige difference check: Buyer prestige must be at least: candidate OVR - 15.
                // Prevents 3rd division (prestige 30) from buying 1st division stars (OVR 80).
                if (buyer.Prestige < candidate.OVR - 15) continue;

                // Budget checks
                int fee = Mathf.RoundToInt(candidate.MarketValue * Random.Range(0.9f, 1.3f));
                if (fee > buyer.TransferBudget) continue;

                int proposedWage = Mathf.RoundToInt(candidate.CurrentContract.WeeklyWage * Random.Range(1.1f, 1.3f));
                if (proposedWage > buyer.WageBudget) continue;

                // Execute!
                Contract newContract = new Contract(buyer.Id, buyer.Name, proposedWage, Random.Range(2, 5));
                DatabaseManager.Instance.TransferPlayer(candidate, buyer, newContract, fee);

                // Record simulated transfer
                SimulatedTransfer st = new SimulatedTransfer
                {
                    PlayerName = candidate.Name,
                    PlayerId = candidate.Id,
                    FromClubName = seller.Name,
                    ToClubName = buyer.Name,
                    TransferFee = fee,
                    WeeklyWage = proposedWage,
                    Week = CurrentWeek,
                    Year = CurrentYear
                };
                SeasonTransfers.Add(st);

                // Social Media Post check:
                // Only post on social media if buyer or seller contains one of our agency clients
                bool involvesOurClientClub = false;
                foreach (var client in AgencyManager.Instance.ActiveAgency.Clients)
                {
                    if (client.CurrentContract != null && 
                        (client.CurrentContract.ClubId == buyer.Id || client.CurrentContract.ClubId == seller.Id))
                    {
                        involvesOurClientClub = true;
                        break;
                    }
                }

                if (involvesOurClientClub)
                {
                    AgencyManager.Instance.LogActivity($"🔥 TRANSFER BOMBASI: {buyer.Name}, {seller.Name} kulübünden {candidate.Name} (GEN: {candidate.OVR}) ile sözleşme imzaladı! Bonservis: €{fee:N0}.");
                }
            }
        }

        private void GenerateWeeklyClientDialogueRequests()
        {
            var agency = AgencyManager.Instance != null ? AgencyManager.Instance.ActiveAgency : null;
            if (agency == null || agency.Clients == null || agency.Clients.Count == 0) return;

            // 35% chance per week for a client to send a dialogue request if no pending requests exist
            if (UnityEngine.Random.value > 0.35f) return;

            List<Player> eligibleClients = agency.Clients.FindAll(c => c != null && !ActiveMails.Exists(m => m.PlayerId == c.Id && m.IsRequest));
            if (eligibleClients.Count == 0) return;

            Player client = eligibleClients[UnityEngine.Random.Range(0, eligibleClients.Count)];

            // 10 Distinct Dialogue Scenarios
            int scenarioIndex = UnityEngine.Random.Range(0, 10);
            SimulationMail mail = new SimulationMail
            {
                Id = System.Guid.NewGuid().ToString(),
                Sender = client.Name,
                PlayerId = client.Id,
                IsRequest = true,
                IsRenewalMail = false
            };

            switch (scenarioIndex)
            {
                case 0:
                    mail.Subject = $"📩 MÜŞTERİ TALEBİ: Özel Antrenör İsteyi ({client.Name})";
                    mail.Content = $"{client.Name}: 'Patron, son zamanlarda bitiricilik/fizik antrenmanlarımda ekstra gelişime ihtiyacım var. Özel bir antrenör tutmamız durumunda performansımı artırabilirim. Bize maliyeti yaklaşık €15,000 olur.'";
                    mail.RequestType = "Coach";
                    mail.MoneyCost = 15000;
                    mail.HappinessEffect = 15;
                    break;

                case 1:
                    mail.Subject = $"📩 MÜŞTERİ TALEBİ: Basın & İmaj Kampanyası ({client.Name})";
                    mail.Content = $"{client.Name}: 'Menajerim, son haftalarda hakkımda çıkan asılsız haberler imajımı zedeliyor. Profesyonel bir PR ajansıyla anlaşıp imaj çalışması yaparsak hem moralim düzelir hem piyasa değerim artar. (€25,000)'";
                    mail.RequestType = "PR";
                    mail.MoneyCost = 25000;
                    mail.HappinessEffect = 12;
                    break;

                case 2:
                    mail.Subject = $"📩 MÜŞTERİ TALEBİ: Maaş İyileştirmesi ({client.Name})";
                    mail.Content = $"{client.Name}: 'Patron, takımdaki performansıma kıyasla aldığım maaş çok düşük kalıyor. Kulüp yönetimiyle konuşup sözleşmemi iyileştirmemizi rica ediyorum.'";
                    mail.RequestType = "Wage";
                    mail.MoneyCost = 0;
                    mail.HappinessEffect = 10;
                    break;

                case 3:
                    mail.Subject = $"📩 MÜŞTERİ TALEBİ: Liderlik Desteği ({client.Name})";
                    mail.Content = $"{client.Name}: 'Menajerim, soyunma odasında daha fazla sorumluluk almak istiyorum. Kulüp yönetimi ve hocayla konuşup liderlik rolümü pekiştirmeme destek olur musun?'";
                    mail.RequestType = "Leadership";
                    mail.MoneyCost = 5000;
                    mail.HappinessEffect = 18;
                    break;

                case 4:
                    mail.Subject = $"📩 MÜŞTERİ TALEBİ: Transfer Arayışı ({client.Name})";
                    mail.Content = $"{client.Name}: 'Patron, artık bu kulüpte misyonumu tamamladığımı hissediyorum. Önümüzdeki transfer döneminde teklifleri değerlendirip başka bir takıma gitmeme yardımcı olmanı bekliyorum.'";
                    mail.RequestType = "Transfer";
                    mail.MoneyCost = 0;
                    mail.HappinessEffect = 15;
                    break;

                case 5:
                    mail.Subject = $"📩 MÜŞTERİ TALEBİ: Fizyoterapist Desteği ({client.Name})";
                    mail.Content = $"{client.Name}: 'Menajerim, sakatlık riskimi azaltmak ve maç toparlanma süremi kısaltmak için özel bir fizyoterapist ekibiyle çalışmak istiyorum. (€10,000)'";
                    mail.RequestType = "Physio";
                    mail.MoneyCost = 10000;
                    mail.HappinessEffect = 15;
                    break;

                case 6:
                    mail.Subject = $"📩 MÜŞTERİ TALEBİ: Sponsorluk Arayışı ({client.Name})";
                    mail.Content = $"{client.Name}: 'Patron, sahada formum yüksek ama hiç bireysel sponsorluk anlaşmam yok. Bana prestijli bir marka sponsorluğu bulabilir misin?'";
                    mail.RequestType = "Sponsor";
                    mail.MoneyCost = 0;
                    mail.HappinessEffect = 10;
                    break;

                case 7:
                    mail.Subject = $"📩 MÜŞTERİ TALEBİ: Zihinsel Koçluk ({client.Name})";
                    mail.Content = $"{client.Name}: 'Menajerim, üzerimdeki baskı çok arttı ve saha içi odaklanma sorunu yaşıyorum. Spor psikoloğu ile 1 aylık seans alırsak zihinsel dayanıklılığım artacak. (€8,000)'";
                    mail.RequestType = "Mental";
                    mail.MoneyCost = 8000;
                    mail.HappinessEffect = 20;
                    break;

                case 8:
                    mail.Subject = $"📩 MÜŞTERİ TALEBİ: Gelişim Kampı ({client.Name})";
                    mail.Content = $"{client.Name}: 'Patron, potansiyelimi en üst seviyeye çıkarmak için sezon arası kişisel gelişim kampına katılmak istiyorum. (€12,000)'";
                    mail.RequestType = "Camp";
                    mail.MoneyCost = 12000;
                    mail.HappinessEffect = 15;
                    break;

                case 9:
                    mail.Subject = $"📩 MÜŞTERİ TALEBİ: Medya Koruması ({client.Name})";
                    mail.Content = $"{client.Name}: 'Patron, taraftarlar ve sosyal medya son maçtan sonra üzerime çok geliyor. Basına bir açıklama yapıp arkamda olduğunu gösterir misin?'";
                    mail.RequestType = "MediaSupport";
                    mail.MoneyCost = 3000;
                    mail.HappinessEffect = 15;
                    break;
            }

            ActiveMails.Add(mail);
            AgencyManager.Instance.LogActivity($"GELEN MESAJ: {client.Name} size bir talep mesajı gönderdi. Gelen Kutunuzu kontrol edin.");
        }
    }
}
