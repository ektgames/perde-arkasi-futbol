using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BehindTheScenesFootball.Core;
using BehindTheScenesFootball.Managers;

namespace BehindTheScenesFootball.UI
{
    public class TalentsPanel : BaseModulePanel
    {
        private Transform listContent;
        
        private Text slotTxt;
        private GameObject btnHireObj;
        
        private Scout candidateScout;
        private int currentViewMode = 0; // 0 = Scout list, 1 = League select, 2 = Scout report
        private Scout activeScoutForAction;

        private readonly string[] firstNames = { "Hasan", "Mehmet", "Ali", "Veli", "Can", "Bülent", "Fatih", "Serkan", "Erhan", "Mustafa", "Murat", "Ahmet", "Gökhan", "Hakan" };
        private readonly string[] lastNames = { "Kaya", "Yılmaz", "Şahin", "Demir", "Çelik", "Arslan", "Öztürk", "Koç", "Aydın", "Bulut", "Güler", "Yıldız", "Kartal" };

        private readonly string[] engFirstNames = { "John", "James", "Robert", "Michael", "William", "David", "Richard", "Joseph", "Thomas", "Charles", "Christopher", "Daniel", "Matthew", "Anthony", "Mark", "Donald", "Steven", "Paul", "Andrew", "Joshua" };
        private readonly string[] engLastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Miller", "Davis", "Garcia", "Rodriguez", "Wilson", "Martinez", "Anderson", "Taylor", "Thomas", "Hernandez", "Moore", "Martin", "Jackson", "Thompson", "White" };

        private Text centerLevelTxt;
        private GameObject btnUpgradeObj;

        public override void Initialize(UIManager manager, GameObject container)
        {
            base.Initialize(manager, container);

            // 1. Upgrade Header Panel (Y: 0.83f to 0.98f)
            GameObject upgradePanel = uiManager.CreatePanelHelper(panelContainer.transform, "UpgradeHeader", new Color(0.12f, 0.16f, 0.22f, 0.90f));
            SetRectTransform(upgradePanel, new Vector2(0.02f, 0.83f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero);

            Outline upgradeBorder = upgradePanel.AddComponent<Outline>();
            upgradeBorder.effectColor = uiManager.ColorAccent;
            upgradeBorder.effectDistance = new Vector2(2f, 2f);

            Text title = CreateText(upgradePanel.transform, "Title", "GÖZLEMCİ MERKEZİ", 50, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(title, new Vector2(0.04f, 0.50f), new Vector2(0.50f, 0.92f), Vector2.zero, Vector2.zero);
            title.fontStyle = FontStyle.Bold;

            centerLevelTxt = CreateText(upgradePanel.transform, "CenterLevel", "Merkez Seviyesi: Sev 1", 38, new Color(46f/255f, 204f/255f, 113f/255f), TextAnchor.MiddleLeft);
            SetRectTransform(centerLevelTxt, new Vector2(0.04f, 0.08f), new Vector2(0.50f, 0.48f), Vector2.zero, Vector2.zero);
            centerLevelTxt.fontStyle = FontStyle.Bold;

            // Upgrade Button
            Text upgradeLabel = uiManager.CreateButtonHelper(upgradePanel.transform, "BtnUpgrade", "MERKEZİ GELİŞTİR", uiManager.ColorGreen, Color.white, () => {
                UpgradeScoutingCenter();
            });
            btnUpgradeObj = upgradeLabel.transform.parent.gameObject;
            SetRectTransform(btnUpgradeObj, new Vector2(0.52f, 0.12f), new Vector2(0.96f, 0.88f), Vector2.zero, Vector2.zero);
            upgradeLabel.fontSize = 34;
            upgradeLabel.fontStyle = FontStyle.Bold;

            // 2. Staff Hiring Header Panel (Y: 0.68f to 0.81f)
            GameObject headerPanel = uiManager.CreatePanelHelper(panelContainer.transform, "StaffHeader", new Color(0.14f, 0.18f, 0.24f, 0.85f));
            SetRectTransform(headerPanel, new Vector2(0.02f, 0.68f), new Vector2(0.98f, 0.81f), Vector2.zero, Vector2.zero);

            Outline headerBorder = headerPanel.AddComponent<Outline>();
            headerBorder.effectColor = new Color(1f, 1f, 1f, 0.1f);
            headerBorder.effectDistance = new Vector2(1f, 1f);

            slotTxt = CreateText(headerPanel.transform, "Slots", "Gözlemciler: 0 / 3", 38, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(slotTxt, new Vector2(0.04f, 0.10f), new Vector2(0.50f, 0.90f), Vector2.zero, Vector2.zero);
            slotTxt.fontStyle = FontStyle.Bold;

            // Hire Button
            Text hireLabel = uiManager.CreateButtonHelper(headerPanel.transform, "BtnHire", "PERSONEL AL (GÖZLEMCİ)", uiManager.ColorAccent, uiManager.ColorTextDark, () => {
                HireScout();
            });
            btnHireObj = hireLabel.transform.parent.gameObject;
            SetRectTransform(btnHireObj, new Vector2(0.52f, 0.12f), new Vector2(0.96f, 0.88f), Vector2.zero, Vector2.zero);
            hireLabel.fontSize = 34;
            hireLabel.fontStyle = FontStyle.Bold;

            // 3. Scroll View (Y: 0.02f to 0.66f)
            GameObject scrollView = uiManager.CreateScrollViewHelper(panelContainer.transform, "TalentsScroll", out listContent);
            SetRectTransform(scrollView, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.66f), Vector2.zero, Vector2.zero);
        }

        private void UpgradeScoutingCenter()
        {
            Agency agency = AgencyManager.Instance.ActiveAgency;
            if (agency.ScoutingCenterLevel >= 5) return;

            long cost = GetScoutingCenterUpgradeCost(agency.ScoutingCenterLevel);
            if (agency.Balance < cost)
            {
                AgencyManager.Instance.LogActivity($"İptal: Gözlemci Merkezi geliştirmesi için €{cost:N0} bütçe gerekiyor.");
                uiManager.ShowFeedbackPopup(BehindTheScenesFootball.Managers.LocalizationManager.Translate($"Yetersiz Bütçe: €{cost:N0} bütçeniz bulunmamaktadır!"));
                return;
            }

            agency.Balance -= cost;
            agency.ScoutingCenterLevel++;
            candidateScout = null; // Generate new candidate corresponding to updated ScoutingCenterLevel!
            AgencyManager.Instance.LogActivity($"GÖZLEMCİ MERKEZİ GELİŞTİRİLDİ: Seviye {agency.ScoutingCenterLevel} yapıldı! (Bütçe: -€{cost:N0})");
            Refresh();
        }

        public static long GetScoutingCenterUpgradeCost(int currentLevel)
        {
            switch (currentLevel)
            {
                case 1: return 50000;
                case 2: return 150000;
                case 3: return 350000;
                case 4: return 750000;
                default: return 0;
            }
        }

        public override void Refresh()
        {
            Agency agency = AgencyManager.Instance.ActiveAgency;
            int scoutCount = agency.HiredScouts.Count;
            bool isEnglish = BehindTheScenesFootball.Managers.LocalizationManager.CurrentLanguage == "EN";

            // Update Center Level Header & Upgrade Button
            string lvlPrefix = isEnglish ? "Center Level: Lvl" : "Merkez Seviyesi: Sev";
            centerLevelTxt.text = $"{lvlPrefix} {agency.ScoutingCenterLevel}";

            Text upgradeLabel = btnUpgradeObj.GetComponentInChildren<Text>();
            if (agency.ScoutingCenterLevel >= 5)
            {
                if (upgradeLabel != null) upgradeLabel.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate("MAKSİMUM SEVİYE");
                Image upImg = btnUpgradeObj.GetComponent<Image>();
                if (upImg != null) upImg.color = uiManager.ColorGreyButton;
            }
            else
            {
                long upCost = GetScoutingCenterUpgradeCost(agency.ScoutingCenterLevel);
                if (upgradeLabel != null)
                {
                    string upTitle = isEnglish ? "UPGRADE" : "GELİŞTİR";
                    string upLvl = isEnglish ? "Lvl" : "Sev";
                    upgradeLabel.text = $"{upTitle} ({upLvl} {agency.ScoutingCenterLevel + 1})\n€{upCost:N0}";
                }
                Image upImg = btnUpgradeObj.GetComponent<Image>();
                if (upImg != null) upImg.color = uiManager.ColorGreen;
            }

            if (scoutCount < 3)
            {
                if (candidateScout == null)
                {
                    string[] fNames = isEnglish ? engFirstNames : firstNames;
                    string[] lNames = isEnglish ? engLastNames : lastNames;
                    string candidateName = fNames[Random.Range(0, fNames.Length)] + " " + lNames[Random.Range(0, lNames.Length)];
                    int candidateLevel = Mathf.Clamp(agency.ScoutingCenterLevel, 1, 5); // Candidate scout level matches ScoutingCenterLevel!
                    candidateScout = new Scout(candidateName, candidateLevel);
                }
                else
                {
                    string firstName = candidateScout.Name.Split(' ')[0];
                    bool nameIsEnglish = System.Array.IndexOf(engFirstNames, firstName) >= 0;
                    bool nameIsTurkish = System.Array.IndexOf(firstNames, firstName) >= 0;

                    if ((isEnglish && !nameIsEnglish) || (!isEnglish && !nameIsTurkish))
                    {
                        string[] fNames = isEnglish ? engFirstNames : firstNames;
                        string[] lNames = isEnglish ? engLastNames : lastNames;
                        candidateScout.Name = fNames[Random.Range(0, fNames.Length)] + " " + lNames[Random.Range(0, lNames.Length)];
                    }
                }

                string scoutsLbl = isEnglish ? "Scouts:" : "Gözlemciler:";
                string candidateLbl = isEnglish ? "Candidate:" : "Aday:";
                string lvlStr = isEnglish ? "Lvl" : "Sev";
                slotTxt.text = $"{scoutsLbl} {scoutCount}/3\n{candidateLbl} {candidateScout.Name} ({lvlStr}: {candidateScout.Level})";
                long cost = GetRecruitmentCost(candidateScout.Level);

                Text hireLabel = btnHireObj.GetComponentInChildren<Text>();
                if (hireLabel != null)
                {
                    string hireTitle = isEnglish ? "HIRE SCOUT" : "GÖZLEMCİ AL";
                    string costTitle = isEnglish ? "Cost" : "Bedel";
                    hireLabel.text = $"{hireTitle} ({lvlStr} {candidateScout.Level})\n{costTitle}: €{cost:N0}";
                }
                btnHireObj.SetActive(true);
            }
            else
            {
                candidateScout = null;
                slotTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate("Gözlemciler: 3 / 3 (Kapasite Dolu)");
                btnHireObj.SetActive(false);
            }

            // Clear list
            foreach (Transform child in listContent)
            {
                Destroy(child.gameObject);
            }

            if (currentViewMode == 0)
            {
                // Scout List Mode
                if (scoutCount == 0)
                {
                    GameObject row = uiManager.CreatePanelHelper(listContent, "EmptyInfo", new Color(0.15f, 0.17f, 0.22f, 0.4f));
                    LayoutElement le = row.AddComponent<LayoutElement>();
                    le.minHeight = 500f;
                    le.preferredHeight = 500f;

                    Text msg = CreateText(row.transform, "MsgText", "<b>İŞE ALINMIŞ GÖZLEMCİ YOK</b>\n\nYukarıdaki <b>'PERSONEL AL'</b> butonuna tıklayarak aday gözlemciyi işe alabilirsiniz.\n\nGözlemcileri seçtiğiniz liglere göndererek menajeri olmayan potansiyelli yetenekleri keşfedebilirsiniz.", 48, Color.white, TextAnchor.MiddleCenter);
                    SetRectTransform(msg, Vector2.zero, Vector2.one, new Vector2(25f, 10f), new Vector2(-25f, -10f));
                }
                else
                {
                    foreach (var s in agency.HiredScouts)
                    {
                        CreateScoutRow(listContent, s);
                    }
                }
            }
            else if (currentViewMode == 1)
            {
                // League Selection Mode
                GameObject backRow = uiManager.CreatePanelHelper(listContent, "BackRow", new Color(0f,0f,0f,0f));
                LayoutElement backLe = backRow.AddComponent<LayoutElement>();
                backLe.minHeight = 150f;
                backLe.preferredHeight = 150f;

                Text btnBack = uiManager.CreateButtonHelper(backRow.transform, "BtnBack", BehindTheScenesFootball.Managers.LocalizationManager.Translate("GERİ DÖN (İPTAL)"), uiManager.ColorRed, Color.white, () => {
                    currentViewMode = 0;
                    Refresh();
                });
                SetRectTransform(btnBack.transform.parent, new Vector2(0.02f, 0.15f), new Vector2(0.98f, 0.85f), Vector2.zero, Vector2.zero);
                btnBack.fontSize = 42;
                btnBack.fontStyle = FontStyle.Bold;

                foreach (var league in DatabaseManager.Instance.Leagues)
                {
                    CreateLeagueRow(listContent, league);
                }
            }
            else if (currentViewMode == 2)
            {
                // Scout Report Mode
                GameObject backRow = uiManager.CreatePanelHelper(listContent, "BackRow", new Color(0f,0f,0f,0f));
                LayoutElement backLe = backRow.AddComponent<LayoutElement>();
                backLe.minHeight = 150f;
                backLe.preferredHeight = 150f;

                Text btnBack = uiManager.CreateButtonHelper(backRow.transform, "BtnBack", BehindTheScenesFootball.Managers.LocalizationManager.Translate("GÖZLEMCİLERE GERİ DÖN"), uiManager.ColorGreen, Color.white, () => {
                    currentViewMode = 0;
                    Refresh();
                });
                SetRectTransform(btnBack.transform.parent, new Vector2(0.02f, 0.15f), new Vector2(0.98f, 0.85f), Vector2.zero, Vector2.zero);
                btnBack.fontSize = 42;
                btnBack.fontStyle = FontStyle.Bold;

                if (activeScoutForAction == null || activeScoutForAction.ScoutedPlayerIds.Count == 0)
                {
                    GameObject row = uiManager.CreatePanelHelper(listContent, "EmptyReport", new Color(0.15f, 0.17f, 0.22f, 0.4f));
                    LayoutElement le = row.AddComponent<LayoutElement>();
                    le.minHeight = 300f;
                    le.preferredHeight = 300f;

                    Text msg = CreateText(row.transform, "MsgText", BehindTheScenesFootball.Managers.LocalizationManager.Translate("Bu raporda henüz aday oyuncu bulunmamaktadır veya tamamı sözleşme imzalanmıştır."), 48, Color.white, TextAnchor.MiddleCenter);
                    SetRectTransform(msg, Vector2.zero, Vector2.one, new Vector2(20f, 10f), new Vector2(-20f, -10f));
                }
                else
                {
                    List<string> ids = new List<string>(activeScoutForAction.ScoutedPlayerIds);
                    foreach (var id in ids)
                    {
                        Player p = DatabaseManager.Instance.GetPlayerById(id);
                        if (p != null)
                        {
                            // If they are already signed by us, remove them automatically
                            if (p.IsAgencyClient)
                            {
                                activeScoutForAction.ScoutedPlayerIds.Remove(id);
                                continue;
                            }
                            CreatePlayerRow(listContent, p);
                        }
                    }
                }
            }
        }

        private void HireScout()
        {
            Agency agency = AgencyManager.Instance.ActiveAgency;
            if (agency.HiredScouts.Count >= 3 || candidateScout == null) return;

            bool isEnglish = BehindTheScenesFootball.Managers.LocalizationManager.CurrentLanguage == "EN";
            string firstName = candidateScout.Name.Split(' ')[0];
            bool nameIsEnglish = System.Array.IndexOf(engFirstNames, firstName) >= 0;
            bool nameIsTurkish = System.Array.IndexOf(firstNames, firstName) >= 0;

            if ((isEnglish && !nameIsEnglish) || (!isEnglish && !nameIsTurkish))
            {
                string[] fNames = isEnglish ? engFirstNames : firstNames;
                string[] lNames = isEnglish ? engLastNames : lastNames;
                candidateScout.Name = fNames[Random.Range(0, fNames.Length)] + " " + lNames[Random.Range(0, lNames.Length)];
            }

            long cost = GetRecruitmentCost(candidateScout.Level);
            if (agency.Balance < cost)
            {
                AgencyManager.Instance.LogActivity($"İşe Alım Başarısız: Yetersiz bütçe (Gereken: €{cost:N0}).");
                uiManager.ShowFeedbackPopup(BehindTheScenesFootball.Managers.LocalizationManager.Translate($"Yetersiz Bütçe: €{cost:N0} bütçeniz bulunmamaktadır!"));
                return;
            }

            agency.Balance -= cost;
            agency.HiredScouts.Add(candidateScout);
            AgencyManager.Instance.LogActivity($"PERSONEL ALIMI: Gözlemci {candidateScout.Name} (Seviye {candidateScout.Level}) €{cost:N0} karşılığında işe alındı.");

            candidateScout = null; // Clear to roll a new one next time
            Refresh();
        }

        private long GetRecruitmentCost(int level)
        {
            switch (level)
            {
                case 1: return 30000;
                case 2: return 60000;
                case 3: return 100000;
                case 4: return 160000;
                case 5: return 250000;
                default: return 50000;
            }
        }

        private int GetWeeklyWage(int level)
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

        private void FireScout(Scout s)
        {
            if (s == null) return;
            Agency agency = AgencyManager.Instance.ActiveAgency;
            if (agency.HiredScouts.Contains(s))
            {
                agency.HiredScouts.Remove(s);
                candidateScout = null; // Re-enable candidate scout generation
                string logMsg = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"GÖZLEMCİ İŞTEN ÇIKARILDI: Gözlemci {s.Name} ile yollar ayrıldı.");
                AgencyManager.Instance.LogActivity(logMsg);
                uiManager.ShowFeedbackPopup(BehindTheScenesFootball.Managers.LocalizationManager.Translate($"Gözlemci {s.Name} işten çıkarıldı."));
                Refresh();
            }
        }

        private void CreateScoutRow(Transform parent, Scout s)
        {
            GameObject row = uiManager.CreatePanelHelper(parent, "ScoutRow_" + s.Id, new Color(0.12f, 0.14f, 0.18f, 0.75f));
            LayoutElement le = row.AddComponent<LayoutElement>();
            le.minHeight = 240f;
            le.preferredHeight = 240f;

            Outline border = row.AddComponent<Outline>();
            border.effectColor = new Color(255f/255f, 255f/255f, 255f/255f, 0.05f);
            border.effectDistance = new Vector2(1f, 1f);

            // Left: Scout Info
            int wage = GetWeeklyWage(s.Level);
            Text info = CreateText(row.transform, "Info", $"<b>{s.Name}</b>\nSeviye: {s.Level}/5\nMaaş: €{wage:N0}/hf", 42, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(info, new Vector2(0.03f, 0.1f), new Vector2(0.42f, 0.9f), Vector2.zero, Vector2.zero);
            info.fontStyle = FontStyle.Bold;

            // Mid: Status & Actions
            if (string.IsNullOrEmpty(s.AssignedLeague))
            {
                // Status
                Text status = CreateText(row.transform, "Status", "BOŞTA", 42, new Color(0.7f, 0.75f, 0.8f), TextAnchor.MiddleCenter);
                SetRectTransform(status, new Vector2(0.44f, 0.55f), new Vector2(0.78f, 0.9f), Vector2.zero, Vector2.zero);
                status.fontStyle = FontStyle.Bold;

                // Action
                Text btnAction = uiManager.CreateButtonHelper(row.transform, "BtnDeploy", "LİGE GÖNDER", uiManager.ColorAccent, uiManager.ColorTextDark, () => {
                    activeScoutForAction = s;
                    currentViewMode = 1; // League selection
                    Refresh();
                });
                SetRectTransform(btnAction.transform.parent, new Vector2(0.44f, 0.12f), new Vector2(0.78f, 0.48f), Vector2.zero, Vector2.zero);
                btnAction.fontSize = 34;
                btnAction.fontStyle = FontStyle.Bold;
            }
            else if (s.WeeksRemaining > 0)
            {
                // Status
                Text status = CreateText(row.transform, "Status", $"ARAMA YAPIYOR\n({s.AssignedLeague})", 34, uiManager.ColorAccent, TextAnchor.MiddleCenter);
                SetRectTransform(status, new Vector2(0.44f, 0.55f), new Vector2(0.78f, 0.9f), Vector2.zero, Vector2.zero);
                status.fontStyle = FontStyle.Bold;

                // Time progress
                Text time = CreateText(row.transform, "Time", $"{s.WeeksRemaining} HAFTA KALDI", 38, new Color(241f/255f, 196f/255f, 15f/255f), TextAnchor.MiddleCenter);
                SetRectTransform(time, new Vector2(0.44f, 0.12f), new Vector2(0.78f, 0.48f), Vector2.zero, Vector2.zero);
                time.fontStyle = FontStyle.Bold;
            }
            else
            {
                // Open report button
                Text btnAction = uiManager.CreateButtonHelper(row.transform, "BtnOpenReport", "RAPOR", uiManager.ColorGreen, Color.white, () => {
                    activeScoutForAction = s;
                    currentViewMode = 2; // Report listing
                    Refresh();
                });
                SetRectTransform(btnAction.transform.parent, new Vector2(0.44f, 0.55f), new Vector2(0.78f, 0.90f), Vector2.zero, Vector2.zero);
                btnAction.fontSize = 38;
                btnAction.fontStyle = FontStyle.Bold;

                // Redeploy button
                Text btnRedeploy = uiManager.CreateButtonHelper(row.transform, "BtnRedeploy", "YENİ GÖREV", uiManager.ColorAccent, uiManager.ColorTextDark, () => {
                    activeScoutForAction = s;
                    currentViewMode = 1; // League selection
                    Refresh();
                });
                SetRectTransform(btnRedeploy.transform.parent, new Vector2(0.44f, 0.12f), new Vector2(0.78f, 0.48f), Vector2.zero, Vector2.zero);
                btnRedeploy.fontSize = 34;
                btnRedeploy.fontStyle = FontStyle.Bold;
            }

            // Right: Red Dismiss / Fire Button
            Text btnFireLabel = uiManager.CreateButtonHelper(row.transform, "BtnFireScout", "KOV", uiManager.ColorRed, Color.white, () => {
                FireScout(s);
            });
            SetRectTransform(btnFireLabel.transform.parent, new Vector2(0.80f, 0.15f), new Vector2(0.97f, 0.85f), Vector2.zero, Vector2.zero);
            btnFireLabel.fontSize = 36;
            btnFireLabel.fontStyle = FontStyle.Bold;
        }

        private void CreateLeagueRow(Transform parent, League l)
        {
            GameObject row = uiManager.CreatePanelHelper(parent, "LeagueDeployRow_" + l.Name, new Color(0.12f, 0.14f, 0.18f, 0.65f));
            LayoutElement le = row.AddComponent<LayoutElement>();
            le.minHeight = 160f;
            le.preferredHeight = 160f;

            Text info = CreateText(row.transform, "Info", $"<b>{l.Name}</b> ({l.Clubs.Count} Takım)", 46, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(info, new Vector2(0.03f, 0.1f), new Vector2(0.60f, 0.9f), Vector2.zero, Vector2.zero);
            info.fontStyle = FontStyle.Bold;

            Text btnChoose = uiManager.CreateButtonHelper(row.transform, "BtnChoose", "GÖREV Yolla", uiManager.ColorAccent, uiManager.ColorTextDark, () => {
                if (activeScoutForAction != null)
                {
                    activeScoutForAction.AssignedLeague = l.Name;
                    activeScoutForAction.WeeksRemaining = 4;
                    activeScoutForAction.ScoutedPlayerIds.Clear();
                    AgencyManager.Instance.LogActivity($"GÖZLEMCİ GÖREVİ: {activeScoutForAction.Name}, {l.Name} araştırmasına gönderildi (4 Hafta sürecek).");
                }
                currentViewMode = 0; // Return to scout list
                Refresh();
            });
            SetRectTransform(btnChoose.transform.parent, new Vector2(0.65f, 0.15f), new Vector2(0.97f, 0.85f), Vector2.zero, Vector2.zero);
            btnChoose.fontSize = 38;
            btnChoose.fontStyle = FontStyle.Bold;
        }

        private void CreatePlayerRow(Transform parent, Player p)
        {
            GameObject row = uiManager.CreatePanelHelper(parent, "PlayerRow_" + p.Id, new Color(0.12f, 0.14f, 0.18f, 0.75f));
            LayoutElement le = row.AddComponent<LayoutElement>();
            le.minHeight = 230f;
            le.preferredHeight = 230f;

            Outline border = row.AddComponent<Outline>();
            border.effectColor = new Color(255f/255f, 255f/255f, 255f/255f, 0.05f);
            border.effectDistance = new Vector2(1f, 1f);

            // Click row to open details modal (allow signing!)
            Button rowBtn = row.AddComponent<Button>();
            uiManager.ConfigureButtonTransition(rowBtn);
            rowBtn.onClick.AddListener(() => uiManager.ShowPlayerDetails(p, true));

            // Column 1: Face, Name, nationality
            GameObject leftContainer = new GameObject("LeftContainer");
            leftContainer.transform.SetParent(row.transform, false);
            SetRectTransform(leftContainer, new Vector2(0.02f, 0f), new Vector2(0.44f, 1f), Vector2.zero, Vector2.zero);

            GameObject faceObj = new GameObject("Miniface");
            faceObj.transform.SetParent(leftContainer.transform, false);
            SetRectTransform(faceObj, new Vector2(0f, 0.05f), new Vector2(0.20f, 0.95f), Vector2.zero, Vector2.zero);
            Image faceImg = faceObj.AddComponent<Image>();
            faceImg.sprite = uiManager.GetMiniface(p);

            GameObject flagObj = new GameObject("FlagImage");
            flagObj.transform.SetParent(leftContainer.transform, false);
            SetRectTransform(flagObj, new Vector2(0.22f, 0.60f), new Vector2(0.30f, 0.88f), Vector2.zero, Vector2.zero);
            Image flagImg = flagObj.AddComponent<Image>();
            flagImg.sprite = uiManager.GetFlagSprite(p.Nationality);
            flagImg.preserveAspect = true;

            Text nameTxt = CreateText(leftContainer.transform, "Name", $"{p.Name} ({p.Position})", 46, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(nameTxt, new Vector2(0.32f, 0.50f), new Vector2(1f, 0.95f), Vector2.zero, Vector2.zero);
            nameTxt.fontStyle = FontStyle.Bold;
            nameTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
            nameTxt.verticalOverflow = VerticalWrapMode.Overflow;

            string clubName = p.CurrentContract != null ? p.CurrentContract.ClubName : "Serbest";
            Text clubTxt = CreateText(leftContainer.transform, "Club", clubName, 44, new Color(0.75f, 0.8f, 0.85f), TextAnchor.MiddleLeft);
            SetRectTransform(clubTxt, new Vector2(0.22f, 0.05f), new Vector2(1f, 0.45f), Vector2.zero, Vector2.zero);
            clubTxt.fontStyle = FontStyle.Bold;
            clubTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
            clubTxt.verticalOverflow = VerticalWrapMode.Overflow;

            // Rating Cells
            CreateSquareCell(row.transform, "OvrCell", p.OVR.ToString(), new Vector2(0.45f, 0.1f), new Vector2(0.50f, 0.9f), new Color(0.1f, 0.35f, 0.1f, 0.85f), Color.white, 42);
            CreateSquareCell(row.transform, "PotCell", p.POT.ToString(), new Vector2(0.51f, 0.1f), new Vector2(0.56f, 0.9f), new Color(0.2f, 0.6f, 0.9f, 0.85f), Color.white, 42);
            CreateSquareCell(row.transform, "AgeCell", p.Age.ToString(), new Vector2(0.57f, 0.1f), new Vector2(0.62f, 0.9f), new Color(0.18f, 0.22f, 0.25f, 0.85f), Color.white, 42);

            string valStr = GetShortenedValue(p.MarketValue);
            CreateSquareCell(row.transform, "ValueCell", valStr, new Vector2(0.63f, 0.1f), new Vector2(0.73f, 0.9f), new Color(0.15f, 0.17f, 0.20f, 0.85f), new Color(241f/255f, 196f/255f, 15f/255f), 36);

            // Action 1: Sign / Details
            Text signLabel = uiManager.CreateButtonHelper(row.transform, "BtnSign", "TEMSİL ET", uiManager.ColorGreen, Color.white, () => {
                uiManager.ShowPlayerDetails(p, true);
            });
            SetRectTransform(signLabel.transform.parent, new Vector2(0.74f, 0.52f), new Vector2(0.98f, 0.92f), Vector2.zero, Vector2.zero);
            signLabel.fontSize = 32;
            signLabel.fontStyle = FontStyle.Bold;

            // Action 2: Ignore/Dismiss
            Text dismissLabel = uiManager.CreateButtonHelper(row.transform, "BtnDismiss", "YOKSAY", uiManager.ColorRed, Color.white, () => {
                if (activeScoutForAction != null)
                {
                    activeScoutForAction.ScoutedPlayerIds.Remove(p.Id);
                    Refresh();
                }
            });
            SetRectTransform(dismissLabel.transform.parent, new Vector2(0.74f, 0.08f), new Vector2(0.98f, 0.48f), Vector2.zero, Vector2.zero);
            dismissLabel.fontSize = 32;
            dismissLabel.fontStyle = FontStyle.Bold;
        }

        private void CreateSquareCell(Transform parent, string name, string value, Vector2 anchorMin, Vector2 anchorMax, Color bgCol, Color textCol, int fontSize)
        {
            GameObject cell = uiManager.CreatePanelHelper(parent, name, bgCol);
            Image img = cell.GetComponent<Image>();
            if (img != null && uiManager.RoundedButtonSprite != null)
            {
                img.sprite = uiManager.RoundedButtonSprite;
                img.type = Image.Type.Sliced;
            }
            SetRectTransform(cell, anchorMin, anchorMax, Vector2.zero, Vector2.zero);

            Text txt = CreateText(cell.transform, "Text", value, fontSize, textCol, TextAnchor.MiddleCenter);
            SetRectTransform(txt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            txt.fontStyle = FontStyle.Bold;
        }

        private string GetShortenedValue(double value)
        {
            if (value >= 1000000)
            {
                return $"€{(value / 1000000.0):0.0}M";
            }
            if (value >= 1000)
            {
                return $"€{(value / 1000.0):0.0}K";
            }
            return $"€{value}";
        }
    }
}
