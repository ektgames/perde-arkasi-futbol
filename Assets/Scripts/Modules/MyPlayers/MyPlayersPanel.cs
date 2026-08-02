using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BehindTheScenesFootball.Core;
using BehindTheScenesFootball.Managers;

namespace BehindTheScenesFootball.UI
{
    public class MyPlayersPanel : BaseModulePanel
    {
        private Transform listContent;

        private enum SortField
        {
            Player,
            Age,
            Matches,
            Goals,
            Assists
        }

        private SortField currentSortField = SortField.Player;
        private bool isAscending = true; // Name sorted ascending by default

        private Text lblPlayer;
        private Text lblAge;
        private Text lblMatches;
        private Text lblGoals;
        private Text lblAssists;

        public override void Initialize(UIManager manager, GameObject container)
        {
            base.Initialize(manager, container);

            // 1. Column Headers Row at the top of the list view (Fully matching Scroll View bounds)
            GameObject headerRow = uiManager.CreatePanelHelper(panelContainer.transform, "TableHeaderRow", new Color(0.1f, 0.12f, 0.15f, 0.9f));
            SetRectTransform(headerRow, new Vector2(0.02f, 0.83f), new Vector2(0.98f, 0.87f), Vector2.zero, Vector2.zero);

            // Interactive sort buttons stretching fully to 0.98f
            lblPlayer = CreateHeaderButton(headerRow.transform, "LblPlayer", "OYUNCU", new Vector2(0.02f, 0f), new Vector2(0.40f, 1f), SortField.Player);
            lblAge = CreateHeaderButton(headerRow.transform, "LblAge", "YAŞ", new Vector2(0.41f, 0f), new Vector2(0.55f, 1f), SortField.Age);
            lblMatches = CreateHeaderButton(headerRow.transform, "LblApps", "MAÇ", new Vector2(0.56f, 0f), new Vector2(0.70f, 1f), SortField.Matches);
            lblGoals = CreateHeaderButton(headerRow.transform, "LblGoals", "GOL", new Vector2(0.71f, 0f), new Vector2(0.84f, 1f), SortField.Goals);
            lblAssists = CreateHeaderButton(headerRow.transform, "LblAssists", "ASİST", new Vector2(0.85f, 0f), new Vector2(0.98f, 1f), SortField.Assists);

            // 2. Scroll View (Shifted down below headers)
            GameObject scrollView = uiManager.CreateScrollViewHelper(panelContainer.transform, "ClientsScroll", out listContent);
            SetRectTransform(scrollView, new Vector2(0f, 0f), new Vector2(1f, 0.82f), Vector2.zero, Vector2.zero);
        }

        private Text CreateHeaderButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, SortField field)
        {
            Text labelTxt = uiManager.CreateButtonHelper(parent, name, label, new Color(0.15f, 0.18f, 0.22f, 0.6f), uiManager.ColorAccent, () => {
                OnHeaderClicked(field);
            });
            SetRectTransform(labelTxt.transform.parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            labelTxt.resizeTextForBestFit = false;
            labelTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            labelTxt.verticalOverflow = VerticalWrapMode.Overflow;
            labelTxt.fontSize = 42;
            labelTxt.fontStyle = FontStyle.Bold;
            return labelTxt;
        }

        private void OnHeaderClicked(SortField field)
        {
            if (currentSortField == field)
            {
                isAscending = !isAscending;
            }
            else
            {
                currentSortField = field;
                // Default sorting: Age/Name ascending (youngest/A-Z first), Stats descending (most first)
                if (field == SortField.Age || field == SortField.Player)
                {
                    isAscending = true;
                }
                else
                {
                    isAscending = false;
                }
            }
            Refresh();
        }

        public override void Refresh()
        {
            // Clear existing rows
            foreach (Transform child in listContent)
            {
                Destroy(child.gameObject);
            }

            // Update header labels to show sorting arrows
            lblPlayer.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"OYUNCU {(currentSortField == SortField.Player ? (isAscending ? "▲" : "▼") : "")}".Trim());
            lblAge.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"YAŞ {(currentSortField == SortField.Age ? (isAscending ? "▲" : "▼") : "")}".Trim());
            lblMatches.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"MAÇ {(currentSortField == SortField.Matches ? (isAscending ? "▲" : "▼") : "")}".Trim());
            lblGoals.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"GOL {(currentSortField == SortField.Goals ? (isAscending ? "▲" : "▼") : "")}".Trim());
            lblAssists.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"ASİST {(currentSortField == SortField.Assists ? (isAscending ? "▲" : "▼") : "")}".Trim());

            var agency = AgencyManager.Instance.ActiveAgency;
            if (agency == null || agency.Clients == null) return;
            
            List<Player> clients = new List<Player>(agency.Clients);

            // Sort clients dynamically based on chosen sort parameters
            clients.Sort((a, b) => {
                int compare = 0;
                switch (currentSortField)
                {
                    case SortField.Player:
                        compare = string.Compare(a.Name, b.Name, System.StringComparison.OrdinalIgnoreCase);
                        break;
                    case SortField.Age:
                        compare = a.Age.CompareTo(b.Age);
                        break;
                    case SortField.Matches:
                        compare = a.Appearances.CompareTo(b.Appearances);
                        break;
                    case SortField.Goals:
                        compare = a.Goals.CompareTo(b.Goals);
                        break;
                    case SortField.Assists:
                        compare = a.Assists.CompareTo(b.Assists);
                        break;
                }
                return isAscending ? compare : -compare;
            });

            foreach (var client in clients)
            {
                CreateClientRow(listContent, client);
            }
        }

        private void CreateClientRow(Transform parent, Player p)
        {
            Color rowBg = p.CurrentContract != null ? new Color(0.12f, 0.35f, 0.18f, 0.75f) : new Color(0.12f, 0.14f, 0.18f, 0.75f);
            GameObject row = uiManager.CreatePanelHelper(parent, "ClientRow", rowBg);
            LayoutElement le = row.AddComponent<LayoutElement>();
            le.minHeight = 230f;
            le.preferredHeight = 230f;

            Outline border = row.AddComponent<Outline>();
            border.effectColor = new Color(255f/255f, 255f/255f, 255f/255f, 0.05f);
            border.effectDistance = new Vector2(1f, 1f);

            // Click row to open details modal popup!
            Button rowBtn = row.AddComponent<Button>();
            uiManager.ConfigureButtonTransition(rowBtn);
            rowBtn.onClick.AddListener(() => uiManager.ShowPlayerDetails(p));

            // --- Column 1: Left Text Block (Only Miniface & Name, vertically centered) ---
            GameObject leftContainer = new GameObject("LeftContainer");
            leftContainer.transform.SetParent(row.transform, false);
            SetRectTransform(leftContainer, new Vector2(0.02f, 0f), new Vector2(0.40f, 1f), Vector2.zero, Vector2.zero);

            // Miniface on the left of leftContainer
            GameObject faceObj = new GameObject("Miniface");
            faceObj.transform.SetParent(leftContainer.transform, false);
            SetRectTransform(faceObj, new Vector2(0.01f, 0.1f), new Vector2(0.22f, 0.9f), Vector2.zero, Vector2.zero);
            Image faceImg = faceObj.AddComponent<Image>();
            faceImg.sprite = uiManager.GetMiniface(p);

            // Name and position (Vertically centered, large and bold)
            Text nameTxt = CreateText(leftContainer.transform, "Name", $"{p.Name} ({p.Position})", 52, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(nameTxt, new Vector2(0.25f, 0.05f), new Vector2(0.98f, 0.95f), Vector2.zero, Vector2.zero);
            nameTxt.fontStyle = FontStyle.Bold;
            nameTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
            nameTxt.verticalOverflow = VerticalWrapMode.Overflow;

            // --- Columns 2 to 5: Dynamic Player Data Cells (Symmetrically spanning 0.41f to 0.98f) ---
            CreateSquareCell(row.transform, "AgeCell", p.Age.ToString(), new Vector2(0.41f, 0.1f), new Vector2(0.55f, 0.9f), new Color(0.18f, 0.22f, 0.25f, 0.85f), Color.white, 48);
            CreateSquareCell(row.transform, "AppsCell", p.Appearances.ToString(), new Vector2(0.56f, 0.1f), new Vector2(0.70f, 0.9f), new Color(0.15f, 0.17f, 0.20f, 0.85f), Color.white, 48);
            CreateSquareCell(row.transform, "GoalsCell", p.Goals.ToString(), new Vector2(0.71f, 0.1f), new Vector2(0.84f, 0.9f), new Color(0.1f, 0.35f, 0.1f, 0.85f), Color.white, 48);
            CreateSquareCell(row.transform, "AssistsCell", p.Assists.ToString(), new Vector2(0.85f, 0.1f), new Vector2(0.98f, 0.9f), new Color(0.2f, 0.6f, 0.9f, 0.85f), Color.white, 48);
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
    }
}
