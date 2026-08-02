using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BehindTheScenesFootball.Core;
using BehindTheScenesFootball.Managers;

namespace BehindTheScenesFootball.UI
{
    public class PrivatePanel : BaseModulePanel
    {
        private Text statsSummaryText;
        private Transform detailContent;

        public override void Initialize(UIManager manager, GameObject container)
        {
            base.Initialize(manager, container);
            StoreDatabase.BuildDatabase();

            // 1. Top Summary Card (Y: 0.82f to 0.98f)
            GameObject summaryCard = uiManager.CreatePanelHelper(panelContainer.transform, "SummaryCard", new Color(0.12f, 0.16f, 0.22f, 0.85f));
            SetRectTransform(summaryCard, new Vector2(0.02f, 0.82f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero);

            Outline cardBorder = summaryCard.AddComponent<Outline>();
            cardBorder.effectColor = uiManager.ColorAccent;
            cardBorder.effectDistance = new Vector2(2f, 2f);

            statsSummaryText = CreateText(summaryCard.transform, "StatsSummary", "Sahip Olunan Mülkler: 0 Adet\nToplam Yatırım Değeri: €0", 42, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(statsSummaryText, new Vector2(0.05f, 0.10f), new Vector2(0.95f, 0.90f), Vector2.zero, Vector2.zero);
            statsSummaryText.fontStyle = FontStyle.Bold;

            // 2. Scroll View (Y: 0.02f to 0.80f)
            GameObject scrollView = uiManager.CreateScrollViewHelper(panelContainer.transform, "PrivateScroll", out detailContent);
            SetRectTransform(scrollView, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.80f), Vector2.zero, Vector2.zero);

            VerticalLayoutGroup oldVlg = detailContent.GetComponent<VerticalLayoutGroup>();
            if (oldVlg != null) DestroyImmediate(oldVlg);

            GridLayoutGroup grid = detailContent.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(460f, 320f);
            grid.spacing = new Vector2(40f, 40f);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;

            // ContentSizeFitter is already attached, but we set it just in case
            ContentSizeFitter fitter = detailContent.GetComponent<ContentSizeFitter>();
            if (fitter == null) fitter = detailContent.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        public override void Refresh()
        {
            // Clear current cards in scroll view
            foreach (Transform child in detailContent)
            {
                Destroy(child.gameObject);
            }

            Agency agency = AgencyManager.Instance.ActiveAgency;
            StoreDatabase.BuildDatabase();

            // Find all purchased items
            List<StoreItem> ownedItems = StoreDatabase.Items.FindAll(item => agency.PurchasedStoreItemIds.Contains(item.Id));

            // Calculate stats
            long totalValue = 0;
            int totalRep = 0;
            foreach (var item in ownedItems)
            {
                totalValue += item.Price;
                totalRep += item.RepReward;
            }

            statsSummaryText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate(
                                     $"Sahip Olunan Varlıklar: <color=#2ECC71>{ownedItems.Count} Adet</color>\n" +
                                     $"Toplam Yatırım Değeri: <color=#F1C40F>€{totalValue:N0}</color>\n" +
                                     $"Toplam Kazanılan İtibar: <color=#58D68D>+{totalRep} Puan</color>");

            if (ownedItems.Count == 0)
            {
                GameObject emptyCard = new GameObject("EmptyCard", typeof(RectTransform));
                emptyCard.transform.SetParent(detailContent, false);
                LayoutElement emptyLe = emptyCard.AddComponent<LayoutElement>();
                emptyLe.preferredWidth = 960f;
                emptyLe.preferredHeight = 400f;

                Text infoText = CreateText(emptyCard.transform, "InfoText", 
                    "Henüz hiçbir mülk veya lüks ürün satın almadınız.\n\nAna ekrandaki 'Mağaza' sekmesini ziyaret ederek prestijinizi arttıracak yatırımlar yapabilirsiniz.", 
                    38, new Color(0.7f, 0.75f, 0.8f), TextAnchor.MiddleCenter);
                SetRectTransform(infoText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                return;
            }

            foreach (var item in ownedItems)
            {
                GameObject card = new GameObject("OwnedCard_" + item.Id, typeof(RectTransform));
                card.transform.SetParent(detailContent, false);

                Image cardImg = card.AddComponent<Image>();
                cardImg.color = new Color(0.18f, 0.22f, 0.25f, 0.85f);

                Outline cardBorder = card.AddComponent<Outline>();
                cardBorder.effectColor = new Color(46f/255f, 204f/255f, 113f/255f, 0.3f);
                cardBorder.effectDistance = new Vector2(1f, 1f);

                // 1. Category label (Top small)
                string catIcon = item.Category == "Araçlar" ? "🚗" :
                                 item.Category == "Konutlar" ? "🏡" :
                                 item.Category == "Lüks & Mobilya" ? "💎" : "🏢";
                Text catText = CreateText(card.transform, "CatLabel", $"{catIcon} {item.Category.ToUpper()}", 28, uiManager.ColorAccent, TextAnchor.MiddleLeft);
                SetRectTransform(catText, new Vector2(0.06f, 0.80f), new Vector2(0.94f, 0.95f), Vector2.zero, Vector2.zero);
                catText.fontStyle = FontStyle.Bold;

                // 2. Item Name
                Text nameText = CreateText(card.transform, "ItemName", item.Name, 36, Color.white, TextAnchor.MiddleLeft);
                SetRectTransform(nameText, new Vector2(0.06f, 0.45f), new Vector2(0.94f, 0.78f), Vector2.zero, Vector2.zero);
                nameText.fontStyle = FontStyle.Bold;

                // 3. Cost & Rep contributions
                Text detailsText = CreateText(card.transform, "DetailsText", $"Bedel: €{item.Price:N0}   |   İtibar: +{item.RepReward}", 32, new Color(0.7f, 0.75f, 0.8f), TextAnchor.MiddleLeft);
                SetRectTransform(detailsText, new Vector2(0.06f, 0.25f), new Vector2(0.94f, 0.42f), Vector2.zero, Vector2.zero);
                detailsText.fontStyle = FontStyle.Bold;

                // 4. Status Badge (Owned)
                Text badgeText = CreateText(card.transform, "StatusBadge", "✔ AKTİF KULLANIMDA", 28, new Color(46f/255f, 204f/255f, 113f/255f), TextAnchor.MiddleLeft);
                SetRectTransform(badgeText, new Vector2(0.06f, 0.05f), new Vector2(0.94f, 0.22f), Vector2.zero, Vector2.zero);
                badgeText.fontStyle = FontStyle.Bold;
            }
        }
    }
}
