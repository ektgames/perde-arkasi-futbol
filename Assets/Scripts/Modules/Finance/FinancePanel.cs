using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BehindTheScenesFootball.Core;
using BehindTheScenesFootball.Managers;

namespace BehindTheScenesFootball.UI
{
    public class FinancePanel : BaseModulePanel
    {
        private Text balanceValTxt;
        private Text weeklyIncomeTxt;
        private Text weeklyExpenseTxt;
        private Text weeklyNetTxt;
        private Transform detailContent;

        public override void Initialize(UIManager manager, GameObject container)
        {
            base.Initialize(manager, container);

            // 1. Financial Overview Header Card (Y: 0.72f to 0.98f)
            GameObject overviewCard = uiManager.CreatePanelHelper(panelContainer.transform, "OverviewCard", new Color(0.12f, 0.16f, 0.22f, 0.85f));
            SetRectTransform(overviewCard, new Vector2(0.02f, 0.72f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero);

            Outline cardBorder = overviewCard.AddComponent<Outline>();
            cardBorder.effectColor = uiManager.ColorAccent;
            cardBorder.effectDistance = new Vector2(2f, 2f);

            // Left Side: Balance Information
            Text balanceLabel = CreateText(overviewCard.transform, "BalanceLabel", "AJANS KASASI", 44, new Color(0.7f, 0.75f, 0.8f), TextAnchor.MiddleLeft);
            SetRectTransform(balanceLabel, new Vector2(0.03f, 0.65f), new Vector2(0.40f, 0.90f), Vector2.zero, Vector2.zero);
            balanceLabel.fontStyle = FontStyle.Bold;

            balanceValTxt = CreateText(overviewCard.transform, "BalanceVal", "€0", 80, new Color(241f/255f, 196f/255f, 15f/255f), TextAnchor.MiddleLeft);
            SetRectTransform(balanceValTxt, new Vector2(0.03f, 0.10f), new Vector2(0.40f, 0.60f), Vector2.zero, Vector2.zero);
            balanceValTxt.fontStyle = FontStyle.Bold;

            // Right Side: Cash Flow Stats
            weeklyIncomeTxt = CreateText(overviewCard.transform, "WeeklyIncome", "Gelirler:  €0 / hf", 44, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(weeklyIncomeTxt, new Vector2(0.42f, 0.65f), new Vector2(0.97f, 0.90f), Vector2.zero, Vector2.zero);
            weeklyIncomeTxt.fontStyle = FontStyle.Bold;

            weeklyExpenseTxt = CreateText(overviewCard.transform, "WeeklyExpense", "Giderler:  €0 / hf", 44, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(weeklyExpenseTxt, new Vector2(0.42f, 0.38f), new Vector2(0.97f, 0.63f), Vector2.zero, Vector2.zero);
            weeklyExpenseTxt.fontStyle = FontStyle.Bold;

            weeklyNetTxt = CreateText(overviewCard.transform, "WeeklyNet", "Net Akış:  €0 / hf", 46, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(weeklyNetTxt, new Vector2(0.42f, 0.08f), new Vector2(0.97f, 0.33f), Vector2.zero, Vector2.zero);
            weeklyNetTxt.fontStyle = FontStyle.Bold;

            // 2. Scroll View (Y: 0.02f to 0.70f) - Genişliği %96'dan %98'e yükselterek panelleri genişlettik
            GameObject scrollView = uiManager.CreateScrollViewHelper(panelContainer.transform, "FinanceScroll", out detailContent);
            SetRectTransform(scrollView, new Vector2(0.01f, 0.02f), new Vector2(0.99f, 0.70f), Vector2.zero, Vector2.zero);
        }

        public override void Refresh()
        {
            Agency agency = AgencyManager.Instance.ActiveAgency;

            // 1. Calculate Scout Expenses
            int totalWeeklyExpenses = 0;
            foreach (var s in agency.HiredScouts)
            {
                totalWeeklyExpenses += GetScoutWeeklyWage(s.Level);
            }

            // 2. Calculate Client Commissions
            long totalWageCommissions = 0;
            long totalSponsorCommissions = 0;

            foreach (var p in agency.Clients)
            {
                if (p.CurrentContract != null)
                {
                    totalWageCommissions += (long)(p.CurrentContract.WeeklyWage * p.CustomWageCommissionPercent);
                }
                if (p.ActiveSponsor != null)
                {
                    totalSponsorCommissions += (long)(p.ActiveSponsor.WeeklyIncome * p.CustomSponsorCommissionPercent);
                }
            }

            long totalWeeklyRevenue = totalWageCommissions + totalSponsorCommissions;
            long netWeeklyIncome = totalWeeklyRevenue - totalWeeklyExpenses;

            // 3. Update top UI elements
            balanceValTxt.text = $"€{agency.Balance:N0}";
            weeklyIncomeTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"Gelirler:  <color=#2ECC71>+€{totalWeeklyRevenue:N0}</color> / hf");
            weeklyExpenseTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"Giderler:  <color=#E74C3C>-€{totalWeeklyExpenses:N0}</color> / hf");

            if (netWeeklyIncome >= 0)
            {
                weeklyNetTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"Net Akış:  <color=#2ECC71>+€{netWeeklyIncome:N0}</color> / hf");
            }
            else
            {
                weeklyNetTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"Net Akış:  <color=#E74C3C>-€{Mathf.Abs(netWeeklyIncome):N0}</color> / hf");
            }

            // 4. Populate dynamic details list
            foreach (Transform child in detailContent)
            {
                Destroy(child.gameObject);
            }

            if (agency.Clients.Count == 0)
            {
                GameObject row = uiManager.CreatePanelHelper(detailContent, "EmptyInfo", new Color(0.15f, 0.17f, 0.22f, 0.4f));
                LayoutElement le = row.AddComponent<LayoutElement>();
                le.minHeight = 440f;
                le.preferredHeight = 440f;

                Text msg = CreateText(row.transform, "MsgText", "<b>TEMSİLCİSİ OLDUĞUNUZ OYUNCU BULUNMUYOR</b>\n\nFinansal gelir elde edebilmek için öncelikle <b>'Gözlemci Merkezi'</b> sekmesinden gözlemci işe almalı ve liglerde keşfe yollayarak menajeri olmayan oyuncularla sözleşme imzalamalısınız.", 54, Color.white, TextAnchor.MiddleCenter);
                SetRectTransform(msg, Vector2.zero, Vector2.one, new Vector2(25f, 10f), new Vector2(-25f, -10f));
            }
            else
            {
                // Title Row
                GameObject titleRow = uiManager.CreatePanelHelper(detailContent, "TitleRow", new Color(0f, 0f, 0f, 0f));
                LayoutElement titleLe = titleRow.AddComponent<LayoutElement>();
                titleLe.minHeight = 80f;
                titleLe.preferredHeight = 80f;

                Text titleTxt = CreateText(titleRow.transform, "TitleTxt", "AKTİF SÖZLEŞME VE FİNANSAL AKIŞ DETAYLARI", 42, uiManager.ColorAccent, TextAnchor.MiddleCenter);
                SetRectTransform(titleTxt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                titleTxt.fontStyle = FontStyle.Bold;

                // Loop clients
                foreach (var p in agency.Clients)
                {
                    CreateClientFinanceRow(detailContent, p);
                }
            }
        }

        private void CreateClientFinanceRow(Transform parent, Player p)
        {
            GameObject row = uiManager.CreatePanelHelper(parent, "ClientFinanceRow_" + p.Id, new Color(0.15f, 0.17f, 0.22f, 0.7f));
            
            // Satırın içeriğe göre otomatik dikey genişlemesini sağla (böylece asla sıkışma ve çakışma olmaz)
            ContentSizeFitter rowCsf = row.AddComponent<ContentSizeFitter>();
            rowCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            LayoutElement le = row.AddComponent<LayoutElement>();
            le.minHeight = 600f; // Minimum güvenli yükseklik değeri
 
            Outline border = row.AddComponent<Outline>();
            border.effectColor = new Color(255f/255f, 255f/255f, 255f/255f, 0.08f);
            border.effectDistance = new Vector2(1f, 1f);
 
            // Dikey hizalama grubu ekle (spacing ve padding genişletildi, araları açıldı)
            VerticalLayoutGroup vlg = row.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 28f; // Araları açıldı (16'dan 28'e çıkarıldı)
            vlg.padding = new RectOffset(20, 20, 30, 30); // Yatay padding 35'ten 20'ye düşürülerek metne daha fazla yatay alan (genişlik) kazandırıldı
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
 
            // 1. Header (Player name & details)
            Text nameLabel = CreateText(row.transform, "NameLabel", $"★ <b>{p.Name}</b> ({p.Position} | YAŞ {p.Age})", 56, Color.white, TextAnchor.MiddleLeft); // Orijinal boyutuna geri alındı: 56
            nameLabel.fontStyle = FontStyle.Bold;
            nameLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            nameLabel.verticalOverflow = VerticalWrapMode.Overflow;
 
            Text valueLabel = CreateText(row.transform, "ValueLabel", $"Değer: €{p.MarketValue:N0}", 50, new Color(241f/255f, 196f/255f, 15f/255f), TextAnchor.MiddleLeft); // Orijinal boyutuna geri alındı: 50
            valueLabel.fontStyle = FontStyle.Bold;
            valueLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            valueLabel.verticalOverflow = VerticalWrapMode.Overflow;
 
            // 2. Separator Line
            GameObject sep = uiManager.CreatePanelHelper(row.transform, "Separator", new Color(1f, 1f, 1f, 0.1f));
            LayoutElement sepLe = sep.AddComponent<LayoutElement>();
            sepLe.minHeight = 4f;
            sepLe.preferredHeight = 4f;
 
            // 3. Club Section
            Text clubTitle = CreateText(row.transform, "ClubTitle", "KULÜP SÖZLEŞMESİ & MAAŞ", 48, uiManager.ColorAccent, TextAnchor.MiddleLeft); // Orijinal boyutuna geri alındı: 48
            clubTitle.fontStyle = FontStyle.Bold;
 
            string club = p.CurrentContract != null ? p.CurrentContract.ClubName : "Serbest";
            long wage = p.CurrentContract != null ? p.CurrentContract.WeeklyWage : 0;
            long wageCommission = (long)(wage * p.CustomWageCommissionPercent);
 
            string contractDetails = $"Kulüp: <b>{club}</b>   |   Maaş: <b>€{wage:N0}/hf</b>   |   Komisyon: <b>%{(p.CustomWageCommissionPercent * 100f):0.0}</b>   |   Gelir: <color=#2ECC71><b>+€{wageCommission:N0}/hf</b></color>";
 
            Text clubBody = CreateText(row.transform, "ClubBody", contractDetails, 44, Color.white, TextAnchor.MiddleLeft);
            var clubScaler = clubBody.GetComponent<TextScaler>();
            if (clubScaler != null)
            {
                clubScaler.enabled = false;
                Destroy(clubScaler);
            }
            clubBody.fontSize = Mathf.RoundToInt(46f * 1.55f); // Manuel ölçeklendirip 71 yapar, böylece çakışma olmadan büyütür
            clubBody.resizeTextForBestFit = false;
            clubBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            clubBody.verticalOverflow = VerticalWrapMode.Overflow;
 
            // 4. Sponsor Section
            Text sponsorTitle = CreateText(row.transform, "SponsorTitle", "AKTİF SPONSORLUK", 48, uiManager.ColorAccent, TextAnchor.MiddleLeft); // Orijinal boyutuna geri alındı: 48
            sponsorTitle.fontStyle = FontStyle.Bold;
 
            string sponsorBrand = p.ActiveSponsor != null ? p.ActiveSponsor.BrandName : "Sponsor Sözleşmesi Yok";
            long sponsorIncome = p.ActiveSponsor != null ? p.ActiveSponsor.WeeklyIncome : 0;
            long sponsorCommission = (long)(sponsorIncome * p.CustomSponsorCommissionPercent);
 
            string sponsorDetails = $"Marka: <b>{sponsorBrand}</b>   |   Bedel: <b>€{sponsorIncome:N0}/hf</b>   |   Komisyon: <b>%{(p.CustomSponsorCommissionPercent * 100f):0.0}</b>   |   Gelir: <color=#2ECC71><b>+€{sponsorCommission:N0}/hf</b></color>";
 
            Text sponsorBody = CreateText(row.transform, "SponsorBody", sponsorDetails, 44, Color.white, TextAnchor.MiddleLeft);
            var spScaler = sponsorBody.GetComponent<TextScaler>();
            if (spScaler != null)
            {
                spScaler.enabled = false;
                Destroy(spScaler);
            }
            sponsorBody.fontSize = Mathf.RoundToInt(46f * 1.55f); // Manuel ölçeklendirip 71 yapar, böylece çakışma olmadan büyütür
            sponsorBody.resizeTextForBestFit = false;
            sponsorBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            sponsorBody.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private int GetScoutWeeklyWage(int level)
        {
            switch (level)
            {
                case 1: return 500;
                case 2: return 1000;
                case 3: return 1500;
                case 4: return 2500;
                case 5: return 4000;
                default: return 1000;
            }
        }
    }
}
