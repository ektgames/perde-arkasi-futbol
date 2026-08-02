using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BehindTheScenesFootball.Core;
using BehindTheScenesFootball.Managers;

namespace BehindTheScenesFootball.UI
{
    public class StoreItem
    {
        public string Id;
        public string Name;
        public string Category;
        public long Price;
        public int RequiredLevel;
        public int RepReward;
    }

    public static class StoreDatabase
    {
        public static List<StoreItem> Items = new List<StoreItem>();

        public static void BuildDatabase()
        {
            if (Items.Count > 0) return;

            // Araçlar - 25 items
            string[] vehicles = {
                "Yol Bisikleti", "Elektrikli Scooter", "Vespa Primavera", "İkinci El Hatchback", "Sıfır Sedan Aile Arabası",
                "Off-road ATV", "Spor Chopper Motosiklet", "Aile SUV Otomobili", "Elektrikli Şehir Arabası", "Klasik Cabriolet Roadster",
                "Premium Executive Sedan", "Elektrikli Spor Otomobil", "Lüks SUV Otomobil", "Amerikan Klasik Muscle", "Grand Tourer Spor Coupe",
                "İtalyan Süper Spor Car", "Pist Odaklı Yarış Arabası", "Zırhlı Makam VIP Minibüs", "Klasik Koleksiyon Yarış Arabası", "Lüks Spor Sürat Teknesi",
                "Özel Yapım Chopper Yat", "Süper Yat (Flybridge)", "Çift Motorlu VIP Helikopter", "Özel Jet (Light Jet)", "Özel Jet (Gulfstream G650)"
            };
            long[] vehiclePrices = {
                120, 450, 3200, 7500, 18000,
                12000, 25000, 35000, 42000, 55000,
                75000, 90000, 120000, 150000, 180000,
                260000, 350000, 450000, 750000, 950000,
                1800000, 4500000, 7200000, 12500000, 25000000
            };
            int[] vehicleReps = {
                1, 2, 4, 8, 15,
                12, 25, 35, 40, 55,
                75, 90, 120, 150, 180,
                280, 380, 500, 850, 1100,
                2200, 6000, 10000, 18000, 40000
            };

            // Konutlar - 25 items
            string[] realEstate = {
                "Paylaşımlı Ofis Odası", "Stüdyo Daire Kira", "1+1 Apartman Dairesi", "Bahçeli Sıra Ev", "Banliyö Müstakil Ev",
                "Göl Kenarı Dağ Evi", "Restorasyonlu Taş Ev", "Orman Eko-Villası", "Modern Dubleks Daire", "Tarihi Yarımada Dairesi",
                "Şehir Merkezi Penthouse Daire", "Boğaz Manzaralı Loft Daire", "Havuzlu Modern Villa", "Dağ Yamacı Akıllı Villa", "Tarihi Yel Değirmeni Konut",
                "Akdeniz Kıyısında Malikane", "Özel Tasarım Kanyon Evi", "Alp Dağlarında Lüks Şale", "Tarihî Konak Malikane", "Boğazda Yalı",
                "Özel Tropik Ada", "Orta Çağ Şatosu", "Miami Beach Sahil Sarayı", "Mega Gökdelen Çatı Katı (Penthouse)", "Özel İklim Kubbeli Saray Kompleksi"
            };
            long[] realEstatePrices = {
                300, 900, 1800, 4200, 12000,
                65000, 110000, 160000, 220000, 310000,
                450000, 650000, 950000, 1400000, 1800000,
                2800000, 4200000, 5800000, 8500000, 12000000,
                18000000, 28000000, 45000000, 75000000, 150000000
            };
            int[] realEstateReps = {
                2, 5, 9, 20, 55,
                70, 120, 180, 250, 350,
                500, 750, 1100, 1600, 2100,
                3500, 5200, 7500, 11000, 16000,
                25000, 40000, 70000, 120000, 250000
            };

            // Lüks & Mobilya - 25 items
            string[] luxury = {
                "Ergonomik Ofis Koltuğu", "Minimalist Çalışma Masası", "Akıllı Masa Lambası", "Kahve Demleme İstasyonu", "Tasarım Kitaplık",
                "Deri Dinlenme Koltuğu", "Akustik Ses Sistemi", "Havalı Süspansiyonlu Yatak", "Modern İtalyan Sehpa", "Antika Duvar Saati",
                "Tasarım Yemek Masası Seti", "Özel Tasarım Kitap Okuma Köşesi", "El Dokuması İpek Halı", "Akıllı Ev Kontrol Paneli Seti", "Özel Ev Sinema Projeksiyonu",
                "Premium Şarap Kavı Dolabı", "Orijinal Yağlı Boya Tablo", "Lüks Mermer Şömine", "Kristal Avize Seti", "Ev İçi Wellness Sauna Odası",
                "Özel Kuyruklu Piyano", "Altın Kaplama Dekorasyon Seti", "Sınırlı Üretim İsviçre Saat Koleksiyonu", "Tarihi Heykel Eseri", "Kraliyet Ailesi Koleksiyon Sandığı"
            };
            long[] luxuryPrices = {
                250, 450, 150, 800, 1200,
                2500, 4500, 6200, 3800, 8500,
                12500, 15000, 22000, 18500, 35000,
                48000, 65000, 85000, 120000, 150000,
                220000, 450000, 850000, 1800000, 3500000
            };
            int[] luxuryReps = {
                1, 2, 1, 4, 6,
                15, 25, 35, 20, 50,
                70, 90, 130, 100, 220,
                300, 450, 600, 900, 1200,
                1800, 3800, 7500, 16000, 32000
            };

            // Ofis Geliştirmeleri - 25 items
            string[] office = {
                "Hızlı Wi-Fi Yönlendirici", "Çift Ekran Monitör Seti", "Ergonomik Klavye & Fare", "Hava Temizleme Cihazı", "Ofis Bitki Seti",
                "Beyaz Akıllı Tahta", "Filtreli Espresso Makinesi", "Ayarlanabilir Ofis Masaları", "Ofis Akustik Bölmeleri", "Güvenlik Kamerası Ağı",
                "Mini Bar ve Snack İstasyonu", "Toplantı Odası Video Konferans Seti", "Özel Ajans Karşılama Bankosu", "Ofis Dinlenme Kapsülü (Nap Pod)", "Özel Sunucu Rafı (Server Rack)",
                "Cam Bölmeli VIP Toplantı Odası", "Ofis İçi Yeşil Duvar (Dikey Bahçe)", "Akıllı Cam Karartma Sistemi", "Şef Odaklı Gurme Ofis Mutfağı", "VR Deneyim ve Eğlence Odası",
                "Özel Güvenlikli Veri Merkezi", "Helikopter Pisti Erişim Yetkisi", "Çatı Katı Sosyal Teras Alanı", "Ajans Özel Basın Toplantısı Salonu", "Gökdelen Katının Tamamı"
            };
            long[] officePrices = {
                120, 600, 250, 350, 180,
                1500, 2800, 5500, 4200, 7500,
                12000, 18000, 25000, 35000, 45000,
                85000, 65000, 110000, 160000, 220000,
                450000, 950000, 1800000, 3200000, 9500000
            };
            int[] officeReps = {
                1, 3, 1, 2, 1,
                8, 15, 30, 22, 45,
                70, 110, 160, 240, 320,
                600, 450, 800, 1200, 1700,
                3800, 8000, 16000, 30000, 90000
            };

            for (int i = 0; i < 25; i++)
            {
                int lvl = 1 + (i / 5);
                
                // Araçlar
                Items.Add(new StoreItem { Id = "V_" + i, Name = vehicles[i], Category = "Araçlar", Price = vehiclePrices[i], RequiredLevel = lvl, RepReward = vehicleReps[i] });
                // Konutlar
                Items.Add(new StoreItem { Id = "R_" + i, Name = realEstate[i], Category = "Konutlar", Price = realEstatePrices[i], RequiredLevel = lvl, RepReward = realEstateReps[i] });
                // Lüks & Mobilya
                Items.Add(new StoreItem { Id = "L_" + i, Name = luxury[i], Category = "Lüks & Mobilya", Price = luxuryPrices[i], RequiredLevel = lvl, RepReward = luxuryReps[i] });
                // Ofis
                Items.Add(new StoreItem { Id = "O_" + i, Name = office[i], Category = "Ofis", Price = officePrices[i], RequiredLevel = lvl, RepReward = officeReps[i] });
            }
        }
    }

    public class StorePanel : BaseModulePanel
    {
        private string activeCategory = "Araçlar";
        
        private Text cashText;
        private Text repText;
        private Transform detailContent;

        private Color colorSelected = new Color(46f/255f, 204f/255f, 113f/255f, 1f); // Green
        private Color colorNormal = new Color(0.18f, 0.22f, 0.25f, 1f); // Dark Grey

        public override void Initialize(UIManager manager, GameObject container)
        {
            base.Initialize(manager, container);
            StoreDatabase.BuildDatabase();

            // Shift layout down to Y: 0.65f to 0.83f to prevent overlap with Back Button (which sits at Y: 0.85f to 0.95f)
            GameObject overviewCard = uiManager.CreatePanelHelper(panelContainer.transform, "OverviewCard", new Color(0.12f, 0.16f, 0.22f, 0.85f));
            SetRectTransform(overviewCard, new Vector2(0.02f, 0.65f), new Vector2(0.98f, 0.83f), Vector2.zero, Vector2.zero);

            Outline cardBorder = overviewCard.AddComponent<Outline>();
            cardBorder.effectColor = uiManager.ColorAccent;
            cardBorder.effectDistance = new Vector2(2f, 2f);

            cashText = CreateText(overviewCard.transform, "CashText", "Ajans Kasası: €0", 48, new Color(241f/255f, 196f/255f, 15f/255f), TextAnchor.MiddleLeft);
            SetRectTransform(cashText, new Vector2(0.05f, 0.52f), new Vector2(0.95f, 0.90f), Vector2.zero, Vector2.zero);
            cashText.fontStyle = FontStyle.Bold;

            repText = CreateText(overviewCard.transform, "RepText", "Şirket Seviyesi: 1  |  İtibar: %0", 40, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(repText, new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.50f), Vector2.zero, Vector2.zero);
            repText.fontStyle = FontStyle.Bold;

            // Shift Category Selector Bar down to Y: 0.55f to 0.63f
            GameObject categoryBar = new GameObject("CategoryBar", typeof(RectTransform));
            categoryBar.transform.SetParent(panelContainer.transform, false);
            SetRectTransform(categoryBar, new Vector2(0.02f, 0.55f), new Vector2(0.98f, 0.63f), Vector2.zero, Vector2.zero);

            string[] categories = { "Araçlar", "Konutlar", "Lüks & Mobilya", "Ofis" };
            float step = 0.25f;
            for (int i = 0; i < categories.Length; i++)
            {
                string catName = categories[i];
                float xMin = i * step;
                float xMax = (i + 1) * step - 0.01f;

                Text btnCat = uiManager.CreateButtonHelper(categoryBar.transform, "BtnCat_" + catName, catName, colorNormal, Color.white, () => {
                    activeCategory = catName;
                    RefreshCategoriesUI(categoryBar);
                    RefreshGrid();
                });
                var localizable = btnCat.GetComponent<BehindTheScenesFootball.Managers.LocalizableText>();
                if (localizable != null)
                {
                    localizable.originalText = catName;
                    localizable.isUppercase = true;
                    localizable.UpdateLanguage();
                }
                SetRectTransform(btnCat.transform.parent.gameObject, new Vector2(xMin, 0f), new Vector2(xMax, 1f), Vector2.zero, Vector2.zero);
                btnCat.fontSize = 32;
                btnCat.fontStyle = FontStyle.Bold;
            }

            // Shift Scroll View down to Y: 0.02f to 0.53f
            GameObject scrollView = uiManager.CreateScrollViewHelper(panelContainer.transform, "StoreScroll", out detailContent);
            SetRectTransform(scrollView, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.53f), Vector2.zero, Vector2.zero);

            VerticalLayoutGroup oldVlg = detailContent.GetComponent<VerticalLayoutGroup>();
            if (oldVlg != null) DestroyImmediate(oldVlg);

            GridLayoutGroup grid = detailContent.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(460f, 440f);
            grid.spacing = new Vector2(40f, 40f);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;

            ContentSizeFitter fitter = detailContent.GetComponent<ContentSizeFitter>();
            if (fitter == null) fitter = detailContent.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            RefreshCategoriesUI(categoryBar);
        }

        private void RefreshCategoriesUI(GameObject categoryBar)
        {
            foreach (Transform t in categoryBar.transform)
            {
                Image bg = t.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = t.name == "BtnCat_" + activeCategory ? colorSelected : colorNormal;
                }
            }
        }

        public override void Refresh()
        {
            Agency agency = AgencyManager.Instance.ActiveAgency;
            cashText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"Ajans Kasası: €{agency.Balance:N0}");
            repText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"Şirket Seviyesi: {agency.Level}  |  İtibar: %{agency.Reputation}");

            RefreshGrid();
        }

        private void RefreshGrid()
        {
            // Clear current cards in scroll view
            foreach (Transform child in detailContent)
            {
                Destroy(child.gameObject);
            }

            Agency agency = AgencyManager.Instance.ActiveAgency;
            List<StoreItem> activeItems = StoreDatabase.Items.FindAll(item => item.Category == activeCategory);

            foreach (var item in activeItems)
            {
                // Create with typeof(RectTransform) so GridLayoutGroup can properly layout the cards!
                GameObject card = new GameObject("StoreCard_" + item.Id, typeof(RectTransform));
                card.transform.SetParent(detailContent, false);

                Image cardImg = card.AddComponent<Image>();
                cardImg.color = new Color(0.15f, 0.17f, 0.22f, 0.85f);

                Outline cardBorder = card.AddComponent<Outline>();
                cardBorder.effectColor = new Color(1f, 1f, 1f, 0.05f);
                cardBorder.effectDistance = new Vector2(1f, 1f);

                // 1. Item Name (Top of card)
                Text nameText = CreateText(card.transform, "ItemName", item.Name, 40, Color.white, TextAnchor.MiddleCenter);
                SetRectTransform(nameText, new Vector2(0.05f, 0.65f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero);
                nameText.fontStyle = FontStyle.Bold;

                // 2. Price (Middle of card)
                Text priceText = CreateText(card.transform, "ItemPrice", $"€{item.Price:N0}", 48, new Color(241f/255f, 196f/255f, 15f/255f), TextAnchor.MiddleCenter);
                SetRectTransform(priceText, new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.62f), Vector2.zero, Vector2.zero);
                priceText.fontStyle = FontStyle.Bold;

                // 3. Stats (Reputation & Level info)
                string statsStr = $"⭐ +{item.RepReward} İtibar  |  Sev. {item.RequiredLevel}";
                Text statsText = CreateText(card.transform, "ItemStats", statsStr, 34, new Color(0.7f, 0.75f, 0.8f), TextAnchor.MiddleCenter);
                SetRectTransform(statsText, new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.42f), Vector2.zero, Vector2.zero);
                statsText.fontStyle = FontStyle.Bold;

                // 4. Action Button (Bottom)
                bool isOwned = agency.PurchasedStoreItemIds.Contains(item.Id);
                bool isLocked = agency.Level < item.RequiredLevel;

                string btnText;
                Color btnColor;
                System.Action onClickAction = null;

                if (isOwned)
                {
                    btnText = "SAHİPSİNİZ";
                    btnColor = new Color(46f/255f, 204f/255f, 113f/255f, 0.5f);
                }
                else if (isLocked)
                {
                    btnText = $"KİLİTLİ (SEV. {item.RequiredLevel})";
                    btnColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
                }
                else
                {
                    btnText = "SATIN AL";
                    btnColor = colorSelected;
                    onClickAction = () => {
                        BuyItem(item);
                    };
                }

                Text actionBtn = uiManager.CreateButtonHelper(card.transform, "BtnBuy_" + item.Id, btnText, btnColor, Color.white, onClickAction);
                SetRectTransform(actionBtn.transform.parent.gameObject, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.25f), Vector2.zero, Vector2.zero);
                actionBtn.fontSize = 34;
                actionBtn.fontStyle = FontStyle.Bold;
            }
        }

        private void BuyItem(StoreItem item)
        {
            Agency agency = AgencyManager.Instance.ActiveAgency;
            if (agency.Balance < item.Price)
            {
                AgencyManager.Instance.LogActivity($"Sözleşme başarısız: Ajans kasasında {item.Name} için yeterli bakiye yok! (Fiyat: €{item.Price:N0}, Kasa: €{agency.Balance:N0})");
                return;
            }

            // Deduct cost and add bought item ID
            agency.Balance -= item.Price;
            agency.PurchasedStoreItemIds.Add(item.Id);

            // Add reputation points
            AgencyManager.Instance.AddAgencyReputation(item.RepReward);

            AgencyManager.Instance.LogActivity($"MAĞAZA SATIN ALIMI: €{item.Price:N0} karşılığında '{item.Name}' satın alındı! Ajansımıza +{item.RepReward} İtibar puanı kazandırıldı.");
            Refresh();
        }
    }
}
