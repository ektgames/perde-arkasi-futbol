using UnityEngine;
using UnityEngine.UI;

namespace BehindTheScenesFootball.UI
{
    public abstract class BaseModulePanel : MonoBehaviour
    {
        protected UIManager uiManager;
        protected GameObject panelContainer;

        public virtual void Initialize(UIManager manager, GameObject container)
        {
            uiManager = manager;
            panelContainer = container;
        }

        public virtual void Open()
        {
            panelContainer.SetActive(true);
            Refresh();
        }

        public virtual void Close()
        {
            panelContainer.SetActive(false);
        }

        public abstract void Refresh();

        protected Text CreateText(Transform parent, string name, string text, int fontSize, Color color, TextAnchor alignment)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            Text txt = obj.AddComponent<Text>();
            var localizable = obj.AddComponent<BehindTheScenesFootball.Managers.LocalizableText>();
            localizable.originalText = text;
            txt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate(text);
            txt.font = uiManager.DefaultFont;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = alignment;
            txt.supportRichText = true;
            txt.raycastTarget = false; // Disable raycast to prevent blocking mouse/touch clicks on parents!
            obj.AddComponent<TextScaler>();
            return txt;
        }
        
        protected void SetRectTransform(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            if (obj != null)
            {
                RectTransform rt = obj.GetComponent<RectTransform>();
                if (rt == null) rt = obj.AddComponent<RectTransform>();
                rt.anchorMin = anchorMin;
                rt.anchorMax = anchorMax;
                rt.offsetMin = offsetMin;
                rt.offsetMax = offsetMax;
            }
        }

        protected void SetRectTransform(Component comp, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            if (comp != null)
            {
                SetRectTransform(comp.gameObject, anchorMin, anchorMax, offsetMin, offsetMax);
            }
        }

        protected string GetFlagEmoji(string nationality)
        {
            if (string.IsNullOrEmpty(nationality)) return "🏳️";
            switch (nationality.ToLower())
            {
                case "turkey": case "türkiye": return "🇹🇷";
                case "england": case "ingiltere": return "🇬🇧";
                case "spain": case "ispanya": return "🇪🇸";
                case "france": case "fransa": return "🇫🇷";
                case "germany": case "almanya": return "🇩🇪";
                case "italy": case "italya": return "🇮🇹";
                case "portugal": case "portekiz": return "🇵🇹";
                case "netherlands": case "hollanda": return "🇳🇱";
                case "russia": case "rusya": return "🇷🇺";
                case "belgium": case "belçika": return "🇧🇪";
                default: return "🏳️";
            }
        }
    }
}
