using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BehindTheScenesFootball.Managers;

namespace BehindTheScenesFootball.UI
{
    public class SocialFeedPanel : BaseModulePanel
    {
        private Transform listContent;

        public override void Initialize(UIManager manager, GameObject container)
        {
            base.Initialize(manager, container);

            // Create a Scroll View inside this container
            GameObject scrollView = uiManager.CreateScrollViewHelper(panelContainer.transform, "FeedScroll", out listContent);
            SetRectTransform(scrollView, new Vector2(0f, 0f), new Vector2(1f, 0.88f), Vector2.zero, Vector2.zero);
        }

        public override void Refresh()
        {
            // Clear existing rows
            foreach (Transform child in listContent)
            {
                Destroy(child.gameObject);
            }

            var logs = AgencyManager.Instance.RecentActivityLog;
            
            // Spacer
            GameObject spacer = new GameObject("HeaderSpacer");
            spacer.transform.SetParent(listContent, false);
            spacer.AddComponent<LayoutElement>().minHeight = 10f;

            foreach (var rawLog in logs)
            {
                CreateTweetRow(listContent, rawLog);
            }
        }

        private void CreateTweetRow(Transform parent, string rawLog)
        {
            // Strip timestamp for display
            string stripped = rawLog;
            if (rawLog.StartsWith("[") && rawLog.Contains("]"))
            {
                int closeIndex = rawLog.IndexOf("]");
                stripped = rawLog.Substring(closeIndex + 1).Trim();
            }

            // Translate message to Tweet format
            string tweetText = uiManager.FormatAsTweet(stripped);

            // High quality premium glassmorphism card (8% white transparency with a sleek glowing border)
            GameObject row = uiManager.CreatePanelHelper(parent, "TweetRow", new Color(255f/255f, 255f/255f, 255f/255f, 0.08f));
            
            Outline border = row.AddComponent<Outline>();
            border.effectColor = new Color(255f/255f, 255f/255f, 255f/255f, 0.2f);
            border.effectDistance = new Vector2(2f, 2f);

            LayoutElement le = row.AddComponent<LayoutElement>();
            le.minHeight = 320f;
            le.preferredHeight = 320f;

            // Tweet content text (Enlarged to 52pt!)
            Text textComponent = CreateText(row.transform, "TweetText", tweetText, 52, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(textComponent, Vector2.zero, Vector2.one, new Vector2(30f, 15f), new Vector2(-30f, -15f));
            textComponent.fontStyle = FontStyle.Normal;
        }
    }
}
