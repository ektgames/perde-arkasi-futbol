using UnityEngine;
using UnityEngine.UI;

namespace BehindTheScenesFootball.UI
{
    public class OnlineMatchPanel : BaseModulePanel
    {
        public override void Initialize(UIManager manager, GameObject container)
        {
            base.Initialize(manager, container);
            
            // Add placeholder text
            Text desc = CreateText(panelContainer.transform, "PlaceholderText", "ÇEVRİMİÇİ KARŞILAŞMALAR\n\nYakında eklenecek...", 38, Color.white, TextAnchor.MiddleCenter);
            SetRectTransform(desc, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        public override void Refresh() { }
    }
}
