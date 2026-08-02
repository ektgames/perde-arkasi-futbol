using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BehindTheScenesFootball.Core;
using BehindTheScenesFootball.Managers;

namespace BehindTheScenesFootball.UI
{
    public class TransfersPanel : BaseModulePanel
    {
        private Transform listContent;

        public override void Initialize(UIManager manager, GameObject container)
        {
            base.Initialize(manager, container);

            // Scroll View inside the container
            GameObject scrollView = uiManager.CreateScrollViewHelper(panelContainer.transform, "TransfersScroll", out listContent);
            SetRectTransform(scrollView, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.96f), Vector2.zero, Vector2.zero);
        }

        public override void Refresh()
        {
            // Clear existing rows
            foreach (Transform child in listContent)
            {
                Destroy(child.gameObject);
            }

            var transfers = SimulationEngine.Instance.SeasonTransfers;

            // Spacer
            GameObject spacer = new GameObject("HeaderSpacer");
            spacer.transform.SetParent(listContent, false);
            spacer.AddComponent<LayoutElement>().minHeight = 10f;

            if (transfers.Count == 0)
            {
                GameObject row = uiManager.CreatePanelHelper(listContent, "EmptyTransfersRow", new Color(0.15f, 0.17f, 0.22f, 0.6f));
                LayoutElement le = row.AddComponent<LayoutElement>();
                le.minHeight = 300f;
                le.preferredHeight = 300f;

                Text msg = CreateText(row.transform, "EmptyInfo", "<b>HAYDİ TRANSFERE!</b>\n\nBu transfer döneminde henüz herhangi bir transfer gerçekleşmedi.", 44, Color.white, TextAnchor.MiddleCenter);
                SetRectTransform(msg, Vector2.zero, Vector2.one, new Vector2(20f, 10f), new Vector2(-20f, -10f));
            }
            else
            {
                // Show transfers in reverse order (newest first!)
                for (int i = transfers.Count - 1; i >= 0; i--)
                {
                    CreateTransferRow(listContent, transfers[i]);
                }
            }
        }

        private void CreateTransferRow(Transform parent, SimulatedTransfer st)
        {
            GameObject row = uiManager.CreatePanelHelper(parent, "TransferRow_" + st.PlayerId + "_" + st.Week, new Color(0.12f, 0.16f, 0.22f, 0.85f));
            
            Outline border = row.AddComponent<Outline>();
            border.effectColor = new Color(0.2f, 0.6f, 0.9f, 0.3f);
            border.effectDistance = new Vector2(1f, 1f);

            LayoutElement le = row.AddComponent<LayoutElement>();
            le.minHeight = 180f;
            le.preferredHeight = 180f;

            // Check if player exists to navigate on click
            Player p = DatabaseManager.Instance.GetPlayerById(st.PlayerId);

            // Left: Player Name and Info
            Text playerTxt = CreateText(row.transform, "PlayerTxt", $"<b>{st.PlayerName}</b>\n{st.FromClubName} ➔ <b>{st.ToClubName}</b>", 42, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(playerTxt, new Vector2(0.04f, 0.1f), new Vector2(0.55f, 0.9f), Vector2.zero, Vector2.zero);

            // Mid/Right: Fee and Wage
            string feeStr = $"Bonservis: <color=#2ECC71>€{st.TransferFee:N0}</color>\nMaaş: <color=#58D68D>€{st.WeeklyWage:N0}/hf</color>";
            Text feeTxt = CreateText(row.transform, "FeeTxt", feeStr, 38, new Color(0.8f, 0.85f, 0.9f), TextAnchor.MiddleRight);
            SetRectTransform(feeTxt, new Vector2(0.58f, 0.1f), new Vector2(0.96f, 0.9f), Vector2.zero, Vector2.zero);

            if (p != null)
            {
                // Make the whole row clickable to open player details modal!
                Button btnRow = row.AddComponent<Button>();
                btnRow.onClick.AddListener(() => {
                    uiManager.ShowPlayerDetails(p, false);
                });
                uiManager.ConfigureButtonTransition(btnRow);
            }
        }
    }
}
