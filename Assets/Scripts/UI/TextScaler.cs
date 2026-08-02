using UnityEngine;
using UnityEngine.UI;

namespace BehindTheScenesFootball.UI
{
    [RequireComponent(typeof(Text))]
    public class TextScaler : MonoBehaviour
    {
        private Text targetText;
        private int originalFontSize = -1;
        private const float ScaleFactor = 1.55f; // Devasa büyüme oranı (optimum dengeli)

        private void Awake()
        {
            targetText = GetComponent<Text>();
            ScaleFont();
        }

        private void OnEnable()
        {
            ScaleFont();
        }

        private void Update()
        {
            ScaleFont();
        }

        private void ScaleFont()
        {
            if (targetText == null) return;

            // 1. Font boyutunu ölçeklendir
            int currentSize = targetText.fontSize;
            if (currentSize > 0)
            {
                int expectedScaledSize = Mathf.RoundToInt(originalFontSize * ScaleFactor);
                if (currentSize != expectedScaledSize)
                {
                    originalFontSize = currentSize;
                    targetText.fontSize = Mathf.RoundToInt(currentSize * ScaleFactor);
                }
            }

            // 2. Çakışma ve taşmaları engelleyecek akıllı sığdırma kuralları
            ContentSizeFitter csf = GetComponent<ContentSizeFitter>();
            if (csf != null)
            {
                // ContentSizeFitter bulunan uzun metinlerde (örn. açıklama metinleri) bestFit çakışmaya neden olur.
                // Bu yüzden metnin sarılmasına (Wrap) ve dikeyde serbestçe uzamasına (Overflow) izin veriyoruz.
                targetText.horizontalOverflow = HorizontalWrapMode.Wrap;
                targetText.verticalOverflow = VerticalWrapMode.Overflow;
                targetText.resizeTextForBestFit = false;
            }
            else
            {
                // Butonlar, hücreler, statlar, başlıklar ve oyuncu satırları için:
                // Metnin kendisi için ayrılmış kutuyu aşmasını engellemek için Wrap + Truncate + BestFit aktif edilir.
                // Bu sayede buton veya hücre metinleri kutu sınırlarına sığmak için otomatik küçülür, asla taşmaz.
                targetText.horizontalOverflow = HorizontalWrapMode.Wrap;
                targetText.verticalOverflow = VerticalWrapMode.Truncate;
                targetText.resizeTextForBestFit = true;
                targetText.resizeTextMinSize = 22; // Okunabilir minimum yazı boyutu sınırı
                targetText.resizeTextMaxSize = targetText.fontSize;
            }
        }
    }
}
