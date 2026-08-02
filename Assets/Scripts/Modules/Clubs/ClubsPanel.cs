using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BehindTheScenesFootball.Core;
using BehindTheScenesFootball.Managers;

namespace BehindTheScenesFootball.UI
{
    public class ClubsPanel : BaseModulePanel
    {
        private League activeLeague;
        private Club activeClub;
        
        private Text leagueNameValTxt;
        private Text clubNameValTxt;
        private Text clubMetaInfoTxt;
        private Transform rosterContent;

        public override void Initialize(UIManager manager, GameObject container)
        {
            base.Initialize(manager, container);

            // 1. Club Details Header Area (Teal panel at the top)
            GameObject detailsHeader = uiManager.CreatePanelHelper(panelContainer.transform, "DetailsHeader", new Color(0.12f, 0.16f, 0.22f, 0.85f));
            SetRectTransform(detailsHeader, new Vector2(0.02f, 0.65f), new Vector2(0.98f, 0.95f), Vector2.zero, Vector2.zero);

            Outline headerBorder = detailsHeader.AddComponent<Outline>();
            headerBorder.effectColor = uiManager.ColorAccent;
            headerBorder.effectDistance = new Vector2(2f, 2f);

            // Row 1: League Selector
            Text leagueLabel = CreateText(detailsHeader.transform, "LeagueLabel", "LİG SEÇİN:", 38, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(leagueLabel, new Vector2(0.03f, 0.58f), new Vector2(0.28f, 0.88f), Vector2.zero, Vector2.zero);
            leagueLabel.fontStyle = FontStyle.Bold;

            Text btnLeaguePrev = uiManager.CreateButtonHelper(detailsHeader.transform, "BtnLeaguePrev", "<", uiManager.ColorAccent, uiManager.ColorTextDark, () => {
                NavigateLeague(-1);
            });
            SetRectTransform(btnLeaguePrev.transform.parent, new Vector2(0.30f, 0.58f), new Vector2(0.38f, 0.88f), Vector2.zero, Vector2.zero);
            btnLeaguePrev.fontSize = 42;
            btnLeaguePrev.fontStyle = FontStyle.Bold;

            leagueNameValTxt = CreateText(detailsHeader.transform, "LeagueNameVal", "", 44, Color.white, TextAnchor.MiddleCenter);
            SetRectTransform(leagueNameValTxt, new Vector2(0.40f, 0.58f), new Vector2(0.88f, 0.88f), Vector2.zero, Vector2.zero);
            leagueNameValTxt.fontStyle = FontStyle.Bold;

            Text btnLeagueNext = uiManager.CreateButtonHelper(detailsHeader.transform, "BtnLeagueNext", ">", uiManager.ColorAccent, uiManager.ColorTextDark, () => {
                NavigateLeague(1);
            });
            SetRectTransform(btnLeagueNext.transform.parent, new Vector2(0.90f, 0.58f), new Vector2(0.98f, 0.88f), Vector2.zero, Vector2.zero);
            btnLeagueNext.fontSize = 42;
            btnLeagueNext.fontStyle = FontStyle.Bold;

            // Row 2: Club Selector
            Text clubLabel = CreateText(detailsHeader.transform, "ClubLabel", "TAKIM SEÇİN:", 38, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(clubLabel, new Vector2(0.03f, 0.24f), new Vector2(0.28f, 0.54f), Vector2.zero, Vector2.zero);
            clubLabel.fontStyle = FontStyle.Bold;

            Text btnClubPrev = uiManager.CreateButtonHelper(detailsHeader.transform, "BtnClubPrev", "<", uiManager.ColorAccent, uiManager.ColorTextDark, () => {
                NavigateClub(-1);
            });
            SetRectTransform(btnClubPrev.transform.parent, new Vector2(0.30f, 0.24f), new Vector2(0.38f, 0.54f), Vector2.zero, Vector2.zero);
            btnClubPrev.fontSize = 42;
            btnClubPrev.fontStyle = FontStyle.Bold;

            clubNameValTxt = CreateText(detailsHeader.transform, "ClubNameVal", "", 44, Color.white, TextAnchor.MiddleCenter);
            SetRectTransform(clubNameValTxt, new Vector2(0.40f, 0.24f), new Vector2(0.88f, 0.54f), Vector2.zero, Vector2.zero);
            clubNameValTxt.fontStyle = FontStyle.Bold;

            Text btnClubNext = uiManager.CreateButtonHelper(detailsHeader.transform, "BtnClubNext", ">", uiManager.ColorAccent, uiManager.ColorTextDark, () => {
                NavigateClub(1);
            });
            SetRectTransform(btnClubNext.transform.parent, new Vector2(0.90f, 0.24f), new Vector2(0.98f, 0.54f), Vector2.zero, Vector2.zero);
            btnClubNext.fontSize = 42;
            btnClubNext.fontStyle = FontStyle.Bold;

            // Row 3: Club Meta Info
            clubMetaInfoTxt = CreateText(detailsHeader.transform, "ClubMetaInfo", "", 34, new Color(0.75f, 0.8f, 0.85f), TextAnchor.MiddleCenter);
            SetRectTransform(clubMetaInfoTxt, new Vector2(0.03f, 0.02f), new Vector2(0.97f, 0.20f), Vector2.zero, Vector2.zero);
            clubMetaInfoTxt.fontStyle = FontStyle.Bold;

            // 2. Roster List scroll view (Below detailsHeader)
            GameObject scrollView = uiManager.CreateScrollViewHelper(panelContainer.transform, "RosterScroll", out rosterContent);
            SetRectTransform(scrollView, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.63f), Vector2.zero, Vector2.zero);
        }

        public void SelectClub(Club club)
        {
            activeClub = club;
            if (activeClub != null)
            {
                activeLeague = DatabaseManager.Instance.Leagues.Find(l => l.OriginalName == activeClub.OriginalLeague);
            }
            Refresh();
        }

        public override void Refresh()
        {
            if (activeClub == null)
            {
                if (DatabaseManager.Instance.Clubs.Count > 0)
                {
                    activeClub = DatabaseManager.Instance.Clubs[0];
                }
                else
                {
                    return;
                }
            }

            if (activeLeague == null || activeLeague.OriginalName != activeClub.OriginalLeague)
            {
                activeLeague = DatabaseManager.Instance.Leagues.Find(l => l.OriginalName == activeClub.OriginalLeague);
                if (activeLeague == null && DatabaseManager.Instance.Leagues.Count > 0)
                {
                    activeLeague = DatabaseManager.Instance.Leagues[0];
                }
            }

            // Update selectors
            leagueNameValTxt.text = activeLeague != null ? activeLeague.Name : "Yok";
            clubNameValTxt.text = activeClub.Name;
            clubMetaInfoTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"Bütçe: <color=#2ECC71>€{activeClub.TransferBudget:N0}</color>  |  Limit: <color=#58D68D>€{activeClub.WageBudget:N0}/hafta</color>  |  İtibar: <color=#F1C40F>{activeClub.Prestige}/100</color>");

            // Clear roster items
            foreach (Transform child in rosterContent)
            {
                Destroy(child.gameObject);
            }

            // Group/Sort roster by position (GK -> DEF -> MID -> FWD) then OVR descending
            List<Player> roster = new List<Player>(activeClub.Roster);
            roster.Sort((a, b) => {
                int posA = GetPositionOrder(a.Position);
                int posB = GetPositionOrder(b.Position);
                if (posA != posB) return posA.CompareTo(posB);
                return b.OVR.CompareTo(a.OVR);
            });

            // Populate roster list
            foreach (var p in roster)
            {
                CreatePlayerRow(p);
            }
        }

        private void NavigateLeague(int dir)
        {
            var leagues = DatabaseManager.Instance.Leagues;
            if (leagues.Count == 0 || activeLeague == null) return;

            int idx = leagues.FindIndex(l => l.OriginalName == activeLeague.OriginalName);
            if (idx == -1) idx = 0;

            idx = (idx + dir + leagues.Count) % leagues.Count;
            activeLeague = leagues[idx];

            if (activeLeague.Clubs.Count > 0)
            {
                activeClub = activeLeague.Clubs[0];
            }
            Refresh();
        }

        private void NavigateClub(int dir)
        {
            if (activeLeague == null || activeLeague.Clubs.Count == 0) return;

            int idx = activeLeague.Clubs.FindIndex(c => c.Id == activeClub.Id);
            if (idx == -1) idx = 0;

            idx = (idx + dir + activeLeague.Clubs.Count) % activeLeague.Clubs.Count;
            activeClub = activeLeague.Clubs[idx];
            Refresh();
        }

        private int GetPositionOrder(PlayerPosition pos)
        {
            switch (pos)
            {
                case PlayerPosition.GK: return 0;
                case PlayerPosition.DEF: return 1;
                case PlayerPosition.MID: return 2;
                case PlayerPosition.FWD: return 3;
                default: return 4;
            }
        }

        private void CreatePlayerRow(Player p)
        {
            GameObject row = uiManager.CreatePanelHelper(rosterContent, "RosterRow_" + p.Id, new Color(0.15f, 0.17f, 0.22f, 0.6f));
            
            LayoutElement le = row.AddComponent<LayoutElement>();
            le.minHeight = 260f;
            le.preferredHeight = 260f;

            Outline border = row.AddComponent<Outline>();
            border.effectColor = new Color(255f/255f, 255f/255f, 255f/255f, 0.1f);
            border.effectDistance = new Vector2(1f, 1f);

            bool isClient = AgencyManager.Instance.ActiveAgency.Clients.Contains(p);
            string starPrefix = isClient ? "<color=#F1C40F>★ </color>" : "";

            // Left: Player metadata (Name, age, position)
            Text info = CreateText(row.transform, "Info", $"{starPrefix}<b>{p.Name}</b> (YAŞ {p.Age} | {p.Position})", 54, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(info, new Vector2(0.03f, 0.5f), new Vector2(0.60f, 0.95f), Vector2.zero, Vector2.zero);
            info.fontStyle = FontStyle.Bold;

            Text value = CreateText(row.transform, "Value", $"Değer: €{p.MarketValue:N0}", 48, new Color(241f/255f, 196f/255f, 15f/255f), TextAnchor.MiddleLeft);
            SetRectTransform(value, new Vector2(0.03f, 0.05f), new Vector2(0.60f, 0.5f), Vector2.zero, Vector2.zero);
            value.fontStyle = FontStyle.Bold;

            // Mid: GEN/POT
            Text rating = CreateText(row.transform, "Ratings", $"GEN: <b>{p.OVR}</b>\nPOT: <b>{p.POT}</b>", 54, uiManager.ColorAccent, TextAnchor.MiddleRight);
            SetRectTransform(rating, new Vector2(0.52f, 0f), new Vector2(0.72f, 1f), Vector2.zero, Vector2.zero);
            rating.fontStyle = FontStyle.Bold;

            // Right: sign button or represented badge
            if (isClient)
            {
                Text representedText = CreateText(row.transform, "ClientBadge", "Temsilcimiz", 48, uiManager.ColorGreen, TextAnchor.MiddleCenter);
                SetRectTransform(representedText, new Vector2(0.74f, 0.15f), new Vector2(0.98f, 0.85f), Vector2.zero, Vector2.zero);
                representedText.fontStyle = FontStyle.Bold;
            }
            else
            {
                string agentStatus = p.HasAgent ? "Rakip Temsilci" : "Temsilcisi Yok";
                Text statusText = CreateText(row.transform, "AgentStatusText", agentStatus, 44, new Color(0.6f, 0.65f, 0.7f), TextAnchor.MiddleCenter);
                SetRectTransform(statusText, new Vector2(0.74f, 0.15f), new Vector2(0.98f, 0.85f), Vector2.zero, Vector2.zero);
                statusText.fontStyle = FontStyle.Bold;
            }
        }
    }
}
