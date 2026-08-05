using System;
using System.Collections.Generic;
using UnityEngine;
using BehindTheScenesFootball.Managers;

namespace BehindTheScenesFootball.Core
{
    public enum PlayerPosition
    {
        GK,
        DEF,
        MID,
        FWD
    }

    [System.Serializable]
    public class PlayerHistoryEntry
    {
        public string Year;
        public string ClubName;
        public int Appearances;
        public int Goals;
        public int Assists;
        public int CleanSheets;
        public float AverageRating;
        public float TransferFee;
    }

    [System.Serializable]
    public class Player
    {
        public string Id;
        public string Name;
        public string Nationality = "Türkiye";
        public int Age;
        public PlayerPosition Position;
        
        public int OVR; // Overall Rating (1-99)
        public int POT; // Potential Rating (1-99)
        
        public int MarketValue;
        public bool IsAgencyClient;
        public float Happiness; // 0-100
        public float Form; // 0-100
        
        public Contract CurrentContract;
        public Sponsor ActiveSponsor;
        public System.Collections.Generic.List<Sponsor> PendingSponsorOffers = new System.Collections.Generic.List<Sponsor>();
        
        public float CustomTransferCommissionPercent = 0.10f;
        public float CustomWageCommissionPercent = 0.05f;
        public float CustomSponsorCommissionPercent = 0.10f;
        
        public int AgencyContractRemainingWeeks = 0;
        public int JoinedClubWeek = 1;
        public int JoinedClubYear = 2026;
        public bool IsSuggestedForLoan = false;
        public bool IsOnLoan = false;
        public string ParentClubId;
        public string ParentClubName;
        public int LoanRemainingWeeks = 0;
        public int RetirementAge;
        public bool WillRetireAtEndOfSeason;
        
        public string SquadRole = "İlk 11 Oyuncusu";
        public bool IsFavorite;
        public bool IsContacted;
        public bool HasAgent;
        public int LastInteractionGlobalWeek = -999;
        
        public string PositiveTrait = "";
        public string NegativeTrait = "";
        
        public int RequestsThisSeasonCount = 0;
        public int LastRequestGlobalWeek = -999;
        public bool IsTransferListed = false;
        public string TransferStatusNote = "";
        
        // Career Stats
        public int Goals;
        public int Assists;
        public int CleanSheets;
        public int Appearances;
        public float MatchRatingSum;
        
        public List<PlayerHistoryEntry> History = new List<PlayerHistoryEntry>();
 
        public Player(string name, int age, PlayerPosition position, int ovr, int pot)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Age = age;
            Position = position;
            OVR = ovr;
            POT = pot;
            Happiness = 75f;
            Form = 70f;
            SquadRole = "İlk 11 Oyuncusu";
            
            Goals = 0;
            Assists = 0;
            CleanSheets = 0;
            Appearances = 0;
            MatchRatingSum = 0f;
 
            IsFavorite = false;
            IsContacted = false;
            HasAgent = UnityEngine.Random.value < 0.8f; // 80% have an agent, 20% are free of representation!
 
            // Realistic retirement age bounds
            int baseRetire = (position == PlayerPosition.GK) ? UnityEngine.Random.Range(37, 43) : UnityEngine.Random.Range(34, 39);
            RetirementAge = baseRetire;
            WillRetireAtEndOfSeason = false;
 
            // Randomly assign positive and negative traits (Build-safe strings!)
            string[] posTraits = { "Çalışkan", "Lider", "Büyük Maç" };
            string[] negTraits = { "Tembel", "Uyumsuz", "Sadakatsiz", "Güvenilmez" };
            PositiveTrait = posTraits[UnityEngine.Random.Range(0, posTraits.Length)];
            NegativeTrait = negTraits[UnityEngine.Random.Range(0, negTraits.Length)];
            
            UpdateMarketValue();
        }

        public float AverageRating => Appearances > 0 ? MatchRatingSum / Appearances : 6.0f;

        public void UpdateMarketValue()
        {
            if (CurrentContract == null)
            {
                MarketValue = 0; // Free agent
                return;
            }

            // Deterministic FIFA / EA FC realistic value tiers based on OVR
            float val = 0f;
            if (OVR < 50) val = Mathf.Lerp(50000, 150000, Mathf.Clamp01((OVR - 30) / 20f));
            else if (OVR < 60) val = Mathf.Lerp(150000, 500000, (OVR - 50) / 10f);
            else if (OVR < 70) val = Mathf.Lerp(500000, 2500000, (OVR - 60) / 10f);
            else if (OVR < 80) val = Mathf.Lerp(2500000, 18000000, (OVR - 70) / 10f);
            else if (OVR < 85) val = Mathf.Lerp(18000000, 45000000, (OVR - 80) / 5f);
            else if (OVR < 90) val = Mathf.Lerp(45000000, 110000000, (OVR - 85) / 5f);
            else val = Mathf.Lerp(110000000, 220000000, (OVR - 90) / 9f);

            // Potential boost: high potential wonderkids add significant value!
            if (POT > OVR)
            {
                float potDiff = POT - OVR;
                float potBonusMultiplier = 1f + (potDiff * 0.05f); // up to +100% value
                if (Age < 21) potBonusMultiplier *= 1.3f;
                else if (Age < 25) potBonusMultiplier *= 1.15f;

                val *= potBonusMultiplier;
            }

            // Age decay for older players
            if (Age > 29)
            {
                float decay = Mathf.Clamp(1.0f - (Age - 29) * 0.08f, 0.15f, 1.0f);
                val *= decay;
            }

            // Apply contract factor (low contract length reduces value)
            if (CurrentContract.DurationYears <= 1)
            {
                val *= 0.5f;
            }
            else if (CurrentContract.DurationYears == 2)
            {
                val *= 0.8f;
            }

            MarketValue = Mathf.RoundToInt(Mathf.Clamp(val, 10000, 250000000));
        }

        public void Develop(float trainingIntensity = 1.0f)
        {
            // Determine growth speed based on POT vs OVR and Age
            if (Age >= 33)
            {
                // Decline for older players
                float declineChance = 0.05f * (Age - 32);
                if (UnityEngine.Random.value < declineChance)
                {
                    OVR = Mathf.Max(30, OVR - 1);
                }
            }
            else if (OVR < POT)
            {
                // Youth growth
                float growthChance = 0.02f * (POT - OVR) * trainingIntensity;
                if (Age < 23) growthChance *= 1.5f;
                else if (Age > 28) growthChance *= 0.5f;

                if (UnityEngine.Random.value < growthChance)
                {
                    OVR = Mathf.Min(POT, OVR + 1);
                }
            }
            
            UpdateMarketValue();
        }

        public void PlayMatch(float rating, int goalsScored = 0, int assistsGiven = 0, bool cleanSheet = false)
        {
            Appearances++;
            MatchRatingSum += rating;
            Goals += goalsScored;
            Assists += assistsGiven;
            if (cleanSheet && (Position == PlayerPosition.GK || Position == PlayerPosition.DEF))
            {
                CleanSheets++;
            }

            // Adjust form and happiness based on match rating
            Form = Mathf.Clamp(Form * 0.8f + rating * 10f * 0.2f, 30f, 100f);
            
            float happinessChange = (rating - 6.5f) * 2f;
            Happiness = Mathf.Clamp(Happiness + happinessChange, 20f, 100f);
        }

        private void DevelopPlayer()
        {
            float matchRatio = Mathf.Clamp01((float)Appearances / 34f);
            float avgRating = AverageRating;

            // Dynamic Potential adjustments based on matches played (under 25 years old)
            if (Age < 25)
            {
                if (Appearances < 8)
                {
                    int potDecay = UnityEngine.Random.Range(3, 7);
                    POT = Mathf.Max(OVR, POT - potDecay);
                    if (IsAgencyClient && potDecay > 0)
                    {
                        AgencyManager.Instance.LogActivity($"POTANSİYEL KAYBI: Müşteriniz {Name} az süre aldığı için gelişim potansiyeli geriledi (Yeni POT: {POT}).");
                    }
                }
                else if (Appearances < 18)
                {
                    int potDecay = UnityEngine.Random.Range(1, 3);
                    POT = Mathf.Max(OVR, POT - potDecay);
                }
                else if (Appearances >= 28)
                {
                    int potBoost = UnityEngine.Random.Range(1, 4);
                    POT = Mathf.Min(99, POT + potBoost);
                    if (IsAgencyClient)
                    {
                        AgencyManager.Instance.LogActivity($"DİNAMİK POTANSİYEL: Müşteriniz {Name} düzenli ilk 11 oynadığı için potansiyeli yükseldi (Yeni POT: {POT}).");
                    }
                }
            }
            
            if (Age < 24)
            {
                int growthCapacity = POT - OVR;
                if (growthCapacity > 0)
                {
                    float growthPoints = (growthCapacity * 0.15f) * matchRatio;
                    float ratingBonus = 0f;
                    if (Appearances > 5 && avgRating > 6.8f)
                    {
                        ratingBonus = (avgRating - 6.8f) * 2f;
                    }
                    
                    int finalGrowth = Mathf.RoundToInt(growthPoints + ratingBonus);
                    
                    // Guarantee minimum growth for active youth
                    if (matchRatio > 0.7f && finalGrowth < 2) finalGrowth = UnityEngine.Random.Range(2, 5);
                    else if (matchRatio > 0.4f && finalGrowth < 1) finalGrowth = UnityEngine.Random.Range(1, 3);
                    
                    finalGrowth = Mathf.Clamp(finalGrowth, 0, 6);
                    
                    if (finalGrowth > 0)
                    {
                        OVR = Mathf.Min(OVR + finalGrowth, POT);
                        if (IsAgencyClient)
                        {
                            AgencyManager.Instance.LogActivity($"GELİŞİM: Müşteriniz {Name} ({Age} Yaş, {Position}), bu sezon {Appearances} maça çıkarak GEN puanını +{finalGrowth} yükseltti (Yeni GEN: {OVR}).");
                        }
                    }
                }
            }
            else if (Age <= 29)
            {
                int growthCapacity = POT - OVR;
                if (growthCapacity > 0)
                {
                    float growthPoints = (growthCapacity * 0.05f) * matchRatio;
                    int finalGrowth = Mathf.RoundToInt(growthPoints + (Appearances > 5 && avgRating > 7.1f ? 1 : 0));
                    
                    finalGrowth = Mathf.Clamp(finalGrowth, 0, 3);
                    if (finalGrowth > 0)
                    {
                        OVR = Mathf.Min(OVR + finalGrowth, POT);
                        if (IsAgencyClient)
                        {
                            AgencyManager.Instance.LogActivity($"GELİŞİM: Müşteriniz {Name} ({Age} Yaş, {Position}), bu sezon {Appearances} maça çıkarak GEN puanını +{finalGrowth} yükseltti (Yeni GEN: {OVR}).");
                        }
                    }
                }
            }
            else // Age >= 30
            {
                float baseDecline = (Age - 29) * 0.5f;
                float declineReduction = baseDecline * matchRatio * 0.8f;
                if (Appearances > 5 && avgRating > 7.1f)
                {
                    declineReduction += 0.5f;
                }
                
                int finalDecline = Mathf.RoundToInt(baseDecline - declineReduction);
                finalDecline = Mathf.Clamp(finalDecline, 0, 4);
                
                if (finalDecline > 0)
                {
                    OVR = Mathf.Max(OVR - finalDecline, 45);
                    POT = Mathf.Max(POT - Mathf.RoundToInt(finalDecline * 0.5f), OVR);
                    
                    if (IsAgencyClient)
                    {
                        AgencyManager.Instance.LogActivity($"GERİLEME: Müşteriniz {Name} ({Age} Yaş, {Position}), ilerleyen yaşı nedeniyle GEN puanından {finalDecline} kaybetti (Yeni GEN: {OVR}).");
                    }
                }
            }
            
            // Recalculate market value based on new stats
            UpdateMarketValue();
        }

        public void EndSeason(string year, string currentClubName)
        {
            // Calculate overall growth/decline first before stats reset!
            DevelopPlayer();

            // Record season history
            History.Add(new PlayerHistoryEntry
            {
                Year = year,
                ClubName = currentClubName,
                Appearances = Appearances,
                Goals = Goals,
                Assists = Assists,
                CleanSheets = CleanSheets,
                AverageRating = AverageRating,
                TransferFee = 0 // Will be set by transfer actions if sold
            });

            // Reset season stats
            Appearances = 0;
            Goals = 0;
            Assists = 0;
            CleanSheets = 0;
            MatchRatingSum = 0f;
            
            Age++;
            
            if (CurrentContract != null)
            {
                CurrentContract.DurationYears--;
            }
            
            if (ActiveSponsor != null)
            {
                ActiveSponsor.DurationYears--;
                if (ActiveSponsor.DurationYears <= 0)
                {
                    ActiveSponsor = null;
                }
            }

            UpdateMarketValue();
        }
    }
}
