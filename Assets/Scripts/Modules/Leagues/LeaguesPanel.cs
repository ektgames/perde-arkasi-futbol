using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BehindTheScenesFootball.Core;
using BehindTheScenesFootball.Managers;

namespace BehindTheScenesFootball.UI
{
    public class LeaguesPanel : BaseModulePanel
    {
        private string selectedLeagueName = "Türkiye 1. Ligi";
        private Transform tableContent;
        private Text currentLeagueBtnLabel;
        private GameObject leaguePopup;
        private Transform popupContent;
        private bool isPopupPopulated = false;

        public override void Initialize(UIManager manager, GameObject container)
        {
            base.Initialize(manager, container);

            // 1. League Selector Button at top (Wider and Taller - Standard build-safe icons!)
            Text selectLabel = CreateText(panelContainer.transform, "SelectLabel", "LİG SEÇİN:", 50, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(selectLabel, new Vector2(0.02f, 0.85f), new Vector2(0.28f, 0.96f), Vector2.zero, Vector2.zero);
            selectLabel.fontStyle = FontStyle.Bold;

            currentLeagueBtnLabel = uiManager.CreateButtonHelper(panelContainer.transform, "BtnLeagueSelector", "⚽ Türkiye 1. Ligi (Türkiye) ▼", new Color(0.12f, 0.18f, 0.24f, 0.85f), Color.white, () => ToggleLeaguePopup(true));
            SetRectTransform(currentLeagueBtnLabel.transform.parent, new Vector2(0.29f, 0.85f), new Vector2(0.85f, 0.96f), Vector2.zero, Vector2.zero);
            currentLeagueBtnLabel.fontSize = 44;
            currentLeagueBtnLabel.fontStyle = FontStyle.Bold;

            // Pin to Home Screen Button (📌 Icon Button) - matches the dropdown selector styling
            Text pinBtnLabel = uiManager.CreateButtonHelper(panelContainer.transform, "BtnLeaguePin", "", new Color(0.12f, 0.18f, 0.24f, 0.85f), Color.white, () => PinSelectedLeague());
            SetRectTransform(pinBtnLabel.transform.parent, new Vector2(0.87f, 0.85f), new Vector2(0.98f, 0.96f), Vector2.zero, Vector2.zero);

            // Create a child Image for our custom red pin icon (kept within parent bounds to prevent overlap)
            GameObject iconObj = new GameObject("PinIcon");
            iconObj.transform.SetParent(pinBtnLabel.transform.parent, false);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.color = Color.white; // Keep white color to render natural red pin texture
            iconImg.preserveAspect = true;

            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.15f, 0.15f);
            iconRt.anchorMax = new Vector2(0.85f, 0.85f);
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = Vector2.zero;

            // Load the red pin icon from Assets
            string pinIconPath = System.IO.Path.Combine(Application.dataPath, "red_pin_icon.png");
            if (System.IO.File.Exists(pinIconPath))
            {
                try
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(pinIconPath);
                    Texture2D texture = new Texture2D(2, 2);
                    if (texture.LoadImage(bytes))
                    {
                        // Remove the black background pixels at runtime with a wider threshold (avoids dark borders or checkerboards)
                        Color[] pixels = texture.GetPixels();
                        for (int i = 0; i < pixels.Length; i++)
                        {
                            // If pixel is near-black/dark grey, make it fully transparent
                            if (pixels[i].r < 0.25f && pixels[i].g < 0.25f && pixels[i].b < 0.25f)
                            {
                                pixels[i] = Color.clear;
                            }
                        }
                        texture.SetPixels(pixels);
                        texture.Apply();

                        Sprite pinSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        iconImg.sprite = pinSprite;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error loading red pin icon: {ex.Message}");
                }
            }

            // 2. Table Headers Row
            GameObject headerRow = uiManager.CreatePanelHelper(panelContainer.transform, "TableHeaderRow", new Color(0.1f, 0.12f, 0.15f, 0.9f));
            SetRectTransform(headerRow, new Vector2(0.02f, 0.80f), new Vector2(0.98f, 0.86f), Vector2.zero, Vector2.zero);
            
            Outline headerBorder = headerRow.AddComponent<Outline>();
            headerBorder.effectColor = new Color(127f/255f, 140f/255f, 141f/255f, 0.5f);
            headerBorder.effectDistance = new Vector2(1f, 1f);

            PopulateRow(headerRow, "Sıra", "Takım", "O", "G", "B", "M", "Av", "P", uiManager.ColorAccent, true);

            // 3. Standings Scroll View
            GameObject scrollView = uiManager.CreateScrollViewHelper(panelContainer.transform, "StandingsScroll", out tableContent);
            SetRectTransform(scrollView, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.78f), Vector2.zero, Vector2.zero);

            // 4. League Selection Overlay Popup (Hidden by default)
            CreateLeaguePopup();
        }

        public void SelectLeague(string leagueName)
        {
            selectedLeagueName = leagueName;
            if (currentLeagueBtnLabel != null)
            {
                League league = DatabaseManager.Instance.Leagues.Find(l => l.OriginalName == selectedLeagueName);
                if (league != null)
                {
                    currentLeagueBtnLabel.text = $"⚽ {league.Name} ({league.Country}) ▼";
                }
            }
            Refresh();
        }

        public override void Refresh()
        {
            // Clear current list content
            foreach (Transform child in tableContent)
            {
                Destroy(child.gameObject);
            }

            League league = DatabaseManager.Instance.Leagues.Find(l => l.OriginalName == selectedLeagueName);
            if (league == null)
            {
                if (DatabaseManager.Instance.Leagues.Count > 0)
                {
                    league = DatabaseManager.Instance.Leagues.Find(l => l.OriginalName == "Türkiye 1. Ligi");
                    if (league == null) league = DatabaseManager.Instance.Leagues[0];
                    selectedLeagueName = league.OriginalName;
                }
            }
            if (league == null) return;

            // Update selector button text
            currentLeagueBtnLabel.text = $"⚽ {league.Name} ({league.Country}) ▼";

            // Sort clubs according to points, GD, and GF
            List<Club> sortedClubs = new List<Club>(league.Clubs);
            sortedClubs.Sort((x, y) =>
            {
                int cmp = y.StandingPoints.CompareTo(x.StandingPoints);
                if (cmp == 0) cmp = y.StandingGD.CompareTo(x.StandingGD);
                if (cmp == 0) cmp = y.StandingGF.CompareTo(x.StandingGF);
                return cmp;
            });

            // Populate rows
            for (int i = 0; i < sortedClubs.Count; i++)
            {
                Club c = sortedClubs[i];
                int rank = i + 1;

                Color rowBgColor = (i % 2 == 0) ? new Color(0.15f, 0.17f, 0.22f, 0.65f) : new Color(0.12f, 0.14f, 0.18f, 0.65f);
                GameObject row = uiManager.CreatePanelHelper(tableContent, "ClubRow_" + rank, rowBgColor);
                
                LayoutElement le = row.AddComponent<LayoutElement>();
                le.minHeight = 160f;
                le.preferredHeight = 160f;

                Outline rowBorder = row.AddComponent<Outline>();
                rowBorder.effectColor = new Color(255f/255f, 255f/255f, 255f/255f, 0.1f);
                rowBorder.effectDistance = new Vector2(1f, 1f);

                // Highlight rank color depending on standing (Bright coral for relegation instead of dark red)
                Color rankColor = Color.white;
                if (rank == 1) rankColor = new Color(241f/255f, 196f/255f, 15f/255f); // Gold (Champion)
                else if (rank <= 4) rankColor = uiManager.ColorAccent; // Cyan (Promotion / Europe)
                else if (rank >= 15) rankColor = new Color(255f/255f, 130f/255f, 130f/255f); // Bright Coral Pink (Relegation)

                PopulateRow(row, rank.ToString(), c.Name, c.StandingPlayed.ToString(), c.StandingWins.ToString(), c.StandingDraws.ToString(), c.StandingLosses.ToString(), (c.StandingGD >= 0 ? "+" : "") + c.StandingGD, c.StandingPoints.ToString(), rankColor, false);

                // Make row clickable to navigate to club details
                Button btn = row.AddComponent<Button>();
                uiManager.ConfigureButtonTransition(btn);
                btn.onClick.AddListener(() => uiManager.OpenClubDetails(c));
            }
        }

        private void PopulateRow(GameObject rowObj, string col1, string col2, string col3, string col4, string col5, string col6, string col7, string col8, Color colColor, bool isHeader)
        {
            // Column 1: Rank (Sıra)
            Text t1 = CreateText(rowObj.transform, "Col1", col1, 48, colColor, TextAnchor.MiddleLeft);
            SetRectTransform(t1, new Vector2(0.02f, 0f), new Vector2(0.12f, 1f), Vector2.zero, Vector2.zero);
            t1.fontStyle = FontStyle.Bold;

            // Column 2: Club Name (Takım) - shifted right (0.14f instead of 0.11f) to prevent touching!
            Text t2 = CreateText(rowObj.transform, "Col2", col2, 48, colColor, TextAnchor.MiddleLeft);
            SetRectTransform(t2, new Vector2(0.14f, 0f), new Vector2(0.54f, 1f), Vector2.zero, Vector2.zero);
            t2.fontStyle = FontStyle.Bold;

            // Column 3: Played (O)
            Text t3 = CreateText(rowObj.transform, "Col3", col3, 48, colColor, TextAnchor.MiddleCenter);
            SetRectTransform(t3, new Vector2(0.54f, 0f), new Vector2(0.61f, 1f), Vector2.zero, Vector2.zero);
            t3.fontStyle = FontStyle.Bold;

            // Column 4: Wins (G)
            Text t4 = CreateText(rowObj.transform, "Col4", col4, 48, colColor, TextAnchor.MiddleCenter);
            SetRectTransform(t4, new Vector2(0.61f, 0f), new Vector2(0.68f, 1f), Vector2.zero, Vector2.zero);
            t4.fontStyle = FontStyle.Bold;

            // Column 5: Draws (B)
            Text t5 = CreateText(rowObj.transform, "Col5", col5, 48, colColor, TextAnchor.MiddleCenter);
            SetRectTransform(t5, new Vector2(0.68f, 0f), new Vector2(0.75f, 1f), Vector2.zero, Vector2.zero);
            t5.fontStyle = FontStyle.Bold;

            // Column 6: Losses (M)
            Text t6 = CreateText(rowObj.transform, "Col6", col6, 48, colColor, TextAnchor.MiddleCenter);
            SetRectTransform(t6, new Vector2(0.75f, 0f), new Vector2(0.82f, 1f), Vector2.zero, Vector2.zero);
            t6.fontStyle = FontStyle.Bold;

            // Column 7: GD (Av)
            Text t7 = CreateText(rowObj.transform, "Col7", col7, 48, colColor, TextAnchor.MiddleCenter);
            SetRectTransform(t7, new Vector2(0.82f, 0f), new Vector2(0.90f, 1f), Vector2.zero, Vector2.zero);
            t7.fontStyle = FontStyle.Bold;

            // Column 8: Points (P)
            Text t8 = CreateText(rowObj.transform, "Col8", col8, 48, colColor, TextAnchor.MiddleCenter);
            SetRectTransform(t8, new Vector2(0.90f, 0f), new Vector2(0.98f, 1f), Vector2.zero, Vector2.zero);
            t8.fontStyle = FontStyle.Bold;
        }

        private void CreateLeaguePopup()
        {
            leaguePopup = uiManager.CreatePanelHelper(panelContainer.transform, "LeagueSelectionPopup", new Color(0.06f, 0.08f, 0.12f, 0.98f));
            SetRectTransform(leaguePopup, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Outline border = leaguePopup.AddComponent<Outline>();
            border.effectColor = uiManager.ColorAccent;
            border.effectDistance = new Vector2(3f, 3f);

            // Title
            Text title = CreateText(leaguePopup.transform, "PopupTitle", "LİGLER LİSTESİ", 54, Color.white, TextAnchor.MiddleCenter);
            SetRectTransform(title, new Vector2(0.1f, 0.88f), new Vector2(0.9f, 0.96f), Vector2.zero, Vector2.zero);
            title.fontStyle = FontStyle.Bold;

            // Scroll View of Leagues
            GameObject scrollView = uiManager.CreateScrollViewHelper(leaguePopup.transform, "PopupLeaguesScroll", out popupContent);
            SetRectTransform(scrollView, new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.86f), Vector2.zero, Vector2.zero);

            // Close button at bottom
            Text closeLabel = uiManager.CreateButtonHelper(leaguePopup.transform, "BtnPopupClose", "KAPAT", uiManager.ColorRed, Color.white, () => ToggleLeaguePopup(false));
            SetRectTransform(closeLabel.transform.parent, new Vector2(0.2f, 0.03f), new Vector2(0.8f, 0.10f), Vector2.zero, Vector2.zero);
            closeLabel.fontSize = 48;
            closeLabel.fontStyle = FontStyle.Bold;

            leaguePopup.SetActive(false);
        }

        private void ToggleLeaguePopup(bool show)
        {
            if (leaguePopup != null)
            {
                leaguePopup.SetActive(show);
                if (show)
                {
                    leaguePopup.transform.SetAsLastSibling();
                    PopulateLeaguePopupIfNeeded();
                }
            }
        }

        private void PopulateLeaguePopupIfNeeded()
        {
            if (DatabaseManager.Instance == null || DatabaseManager.Instance.Leagues == null) return;

            foreach (Transform child in popupContent)
            {
                Destroy(child.gameObject);
            }

            // Group leagues by country
            Dictionary<string, List<League>> leaguesByCountry = new Dictionary<string, List<League>>();
            foreach (var league in DatabaseManager.Instance.Leagues)
            {
                string origCountry = league.OriginalCountry;
                if (!leaguesByCountry.ContainsKey(origCountry))
                {
                    leaguesByCountry[origCountry] = new List<League>();
                }
                leaguesByCountry[origCountry].Add(league);
            }

            // Order of countries to display in dropdown
            List<string> countryOrder = new List<string> {
                "Türkiye", "İngiltere", "İspanya", "Fransa", "Almanya", "İtalya", "Portekiz", "Hollanda", "Rusya", "Belçika", "Brezilya"
            };

            foreach (var country in countryOrder)
            {
                if (!leaguesByCountry.ContainsKey(country)) continue;

                // Add a bold Country Header
                GameObject headerObj = new GameObject("CountryHeader_" + country);
                headerObj.transform.SetParent(popupContent, false);
                LayoutElement headerLe = headerObj.AddComponent<LayoutElement>();
                headerLe.minHeight = 85f;
                headerLe.preferredHeight = 85f;

                Text headerTxt = CreateText(headerObj.transform, "Label", $"▬▬▬ {country} ▬▬▬", 42, uiManager.ColorAccent, TextAnchor.MiddleCenter);
                var localizable = headerTxt.GetComponent<BehindTheScenesFootball.Managers.LocalizableText>();
                if (localizable != null)
                {
                    localizable.originalText = $"▬▬▬ {country} ▬▬▬";
                    localizable.isUppercase = true;
                    localizable.UpdateLanguage();
                }
                SetRectTransform(headerTxt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                headerTxt.fontStyle = FontStyle.Bold;

                // List the leagues under this country header
                foreach (var league in leaguesByCountry[country])
                {
                    string leagueOrigName = league.OriginalName;
                    Text btnTxt = uiManager.CreateButtonHelper(popupContent, "BtnLeague_" + leagueOrigName, league.OriginalName, new Color(0.15f, 0.2f, 0.25f, 0.8f), Color.white, () => {
                        selectedLeagueName = leagueOrigName;
                        ToggleLeaguePopup(false);
                        Refresh();
                    });
                    
                    var btnLocalizable = btnTxt.GetComponent<BehindTheScenesFootball.Managers.LocalizableText>();
                    if (btnLocalizable != null)
                    {
                        btnLocalizable.originalText = league.OriginalName;
                        btnLocalizable.UpdateLanguage();
                    }
                    
                    LayoutElement le = btnTxt.transform.parent.gameObject.AddComponent<LayoutElement>();
                    le.minHeight = 150f;
                    le.preferredHeight = 150f;

                    btnTxt.fontSize = 48;
                    btnTxt.fontStyle = FontStyle.Bold;
                }
            }

            isPopupPopulated = true;
        }

        public override void Open()
        {
            panelContainer.SetActive(true);
            
            selectedLeagueName = uiManager.SelectedLeagueName;
            if (string.IsNullOrEmpty(selectedLeagueName) || DatabaseManager.Instance.Leagues.Find(l => l.OriginalName == selectedLeagueName) == null)
            {
                if (DatabaseManager.Instance.Leagues.Count > 0)
                {
                    League defaultLg = DatabaseManager.Instance.Leagues.Find(l => l.OriginalName == "Türkiye 1. Ligi");
                    if (defaultLg == null) defaultLg = DatabaseManager.Instance.Leagues[0];
                    selectedLeagueName = defaultLg.OriginalName;
                    uiManager.SelectedLeagueName = selectedLeagueName;
                }
            }

            if (currentLeagueBtnLabel != null)
            {
                League league = DatabaseManager.Instance.Leagues.Find(l => l.OriginalName == selectedLeagueName);
                if (league != null)
                {
                    currentLeagueBtnLabel.text = $"⚽ {league.Name} ({league.Country}) ▼";
                }
            }

            Refresh();
        }

        private void PinSelectedLeague()
        {
            uiManager.SelectedLeagueName = selectedLeagueName;
            uiManager.RefreshUI();
        }
    }
}
