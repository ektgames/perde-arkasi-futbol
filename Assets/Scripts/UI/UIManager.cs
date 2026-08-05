using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BehindTheScenesFootball.Core;
using BehindTheScenesFootball.Managers;

namespace BehindTheScenesFootball.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private Canvas mainCanvas;
        
        // Exposed Public Properties for Modules
        public Font DefaultFont => defaultFont;
        public Color ColorAccent => colorAccent;
        public Color ColorTextLight => colorTextLight;
        public Color ColorTextDark => colorTextDark;
        public Color ColorGreen => colorGreen;
        public Color ColorRed => colorRed;
        public Color ColorGold => colorGold;

        // UI Colors (Modern Flat / Dark Theme / Glassmorphism)
        private Color colorBg = new Color(11f / 255f, 12f / 255f, 16f / 255f, 1f); // Deep dark background
        private Color colorPanel = new Color(31f / 255f, 40f / 255f, 51f / 255f, 0.5f); // Semi-transparent slate panel
        private Color colorAccent = new Color(102f / 255f, 252f / 255f, 241f / 255f, 1f); // Cyan neon
        private Color colorTextLight = new Color(197f / 255f, 198f / 255f, 199f / 255f, 1f); // Silver text
        private Color colorTextDark = new Color(11f / 255f, 12f / 255f, 16f / 255f, 1f); // Black text
        private Color colorGreen = new Color(46f / 255f, 125f / 255f, 50f / 255f, 0.35f); // Transparent Forest Green
        private Color colorRed = new Color(198f / 255f, 40f / 255f, 40f / 255f, 0.35f); // Transparent Red
        private Color colorGreyButton = new Color(55f / 255f, 71f / 255f, 79f / 255f, 0.35f); // Transparent Slate Grey
        private Color colorNextWeek = new Color(27f / 255f, 94f / 255f, 32f / 255f, 0.6f); // Semi-transparent Next Week Green
        private Color colorGold = new Color(241f / 255f, 196f / 255f, 15f / 255f, 1f); // Gold / Accent Yellow

        // Header Texts
        private Text moneyText;
        private Text weekText;
        private Text transferSeasonText;
        private Text managerNameText;
        private Text agencyNameText;
        private Text reputationText;

        // Widget Texts
        private Text standingsWidgetText;
        private Text matchesWidgetText;

        // Parent Screens
        private GameObject homeScreenObj;
        private GameObject subpanelContainerObj;
        private Text subpanelTitleText;
        private Transform subpanelContentParent;

        // Subpanel Instances
        private BaseModulePanel activeSubpanel;
        private MyPlayersPanel myPlayersPanel;
        private TalentsPanel talentsPanel;
        private NewsPanel newsPanel;
        private SocialFeedPanel socialFeedPanel;
        private FinancePanel financePanel;
        private PrivatePanel privatePanel;
        private StorePanel storePanel;
        private AllPlayersPanel allPlayersPanel;
        private LeaguesPanel leaguesPanel;
        private ClubsPanel clubsPanel;
        private OnlineMatchPanel onlineMatchPanel;
        private TransfersPanel transfersPanel;
        private Text newsButtonText;
        private Text talentsButtonText;

        // Homepage Standings Widget Elements
        private Text widgetTitleText;
        private Text[] widgetRowLeftTexts = new Text[4];
        private Text[] widgetRowRightTexts = new Text[4];

        private Font defaultFont;
        private Sprite roundedButtonSprite;
        public Sprite RoundedButtonSprite => roundedButtonSprite;
        public string SelectedLeagueName = "Türkiye 1. Ligi";
        private float refreshTimer = 0f;
        private Dictionary<string, Sprite> minifaceCache = new Dictionary<string, Sprite>();

        // State tracking to prevent redundant subpanel rebuilds (which cause flashing/flickering UI elements)
        private int lastRefreshedWeek = -1;
        private long lastRefreshedBalance = -1;
        private int lastRefreshedClientsCount = -1;
        private int lastRefreshedOffersCount = -1;
        private int lastRefreshedMailsCount = -1;
        private BaseModulePanel lastActiveSubpanel = null;
        private static bool isIntroPlayed = false;

        // Settings UI references
        private Text musicToggleBtnLabel;
        private Text musicVolumeValText;
        private Text langTrBtnLabel;
        private Text langEnBtnLabel;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                enabled = false;
                Destroy(gameObject);
                return;
            }

            // Get standard built-in font
            GameObject temp = new GameObject("TempFontHolder");
            Text tempText = temp.AddComponent<Text>();
            defaultFont = tempText.font;
            Destroy(temp);

            if (defaultFont == null)
            {
                defaultFont = Font.CreateDynamicFontFromOSFont("Arial", 14);
            }

            // Dynamically construct UI
            GenerateRoundedSprite();
            CreateUIElements();
            InitializeSubpanels();

            // Initialize AudioManager and play background music
            var audioMgr = BehindTheScenesFootball.Managers.AudioManager.Instance;
        }

        private void Start()
        {
            if (!enabled) return;
            StartIntroSequence();
        }

        private void StartIntroSequence()
        {
            if (isIntroPlayed)
            {
                ShowWelcomeMenu();
                return;
            }

            isIntroPlayed = true;

            if (mainMenuObj != null) mainMenuObj.SetActive(false);
            if (homeScreenObj != null) homeScreenObj.SetActive(false);
            if (subpanelContainerObj != null) subpanelContainerObj.SetActive(false);

            StartCoroutine(IntroSequenceRoutine());
        }

        private System.Collections.IEnumerator IntroSequenceRoutine()
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                Canvas mainCanvas = GameObject.FindObjectOfType<Canvas>();
                if (mainCanvas != null) canvas = mainCanvas.gameObject;
            }
            if (canvas == null)
            {
                ShowWelcomeMenu();
                yield break;
            }

            // Create Intro Overlay Panel
            GameObject introPanel = new GameObject("EktReklamIntroPanel");
            introPanel.transform.SetParent(canvas.transform, false);
            RectTransform rtIntro = introPanel.AddComponent<RectTransform>();
            rtIntro.anchorMin = Vector2.zero;
            rtIntro.anchorMax = Vector2.one;
            rtIntro.sizeDelta = Vector2.zero;

            Image bg = introPanel.AddComponent<Image>();
            bg.color = Color.black;
            bg.raycastTarget = true;

            CanvasGroup cg = introPanel.AddComponent<CanvasGroup>();
            cg.alpha = 1f;

            // Barcode Container Setup
            GameObject barcodeContainer = new GameObject("BarcodeContainer");
            barcodeContainer.transform.SetParent(introPanel.transform, false);
            RectTransform rtBarContainer = barcodeContainer.AddComponent<RectTransform>();
            rtBarContainer.anchorMin = new Vector2(0.5f, 0.5f);
            rtBarContainer.anchorMax = new Vector2(0.5f, 0.5f);
            rtBarContainer.pivot = new Vector2(0.5f, 0.5f);
            rtBarContainer.anchoredPosition = new Vector2(0f, 60f);
            rtBarContainer.sizeDelta = new Vector2(240f, 60f);

            // Pseudo Barcode stripes
            int[] barPattern = { 2, 4, 1, 1, 3, 2, 5, 1, 2, 4, 1, 3, 2, 1, 5, 2, 3, 1, 4, 2, 1, 3, 2 };
            float totalPatternWidth = 0f;
            foreach (int w in barPattern) totalPatternWidth += w * 3f + 2f;

            float curX = -totalPatternWidth / 2f;
            for (int i = 0; i < barPattern.Length; i++)
            {
                float w = barPattern[i] * 3f;
                if (i % 2 == 0)
                {
                    GameObject barGo = new GameObject($"Bar_{i}");
                    barGo.transform.SetParent(barcodeContainer.transform, false);
                    RectTransform rtBar = barGo.AddComponent<RectTransform>();
                    rtBar.anchorMin = new Vector2(0.5f, 0.5f);
                    rtBar.anchorMax = new Vector2(0.5f, 0.5f);
                    rtBar.pivot = new Vector2(0f, 0.5f);
                    rtBar.anchoredPosition = new Vector2(curX, 0f);
                    rtBar.sizeDelta = new Vector2(w, 60f);

                    Image barImg = barGo.AddComponent<Image>();
                    barImg.color = new Color(0.85f, 0.85f, 0.9f, 1f);
                }
                curX += w + 2f;
            }

            // EKT Barcode Label Underneath
            GameObject barcodeLabelGo = new GameObject("BarcodeLabel");
            barcodeLabelGo.transform.SetParent(barcodeContainer.transform, false);
            RectTransform rtBarLabel = barcodeLabelGo.AddComponent<RectTransform>();
            rtBarLabel.anchorMin = new Vector2(0.5f, 0f);
            rtBarLabel.anchorMax = new Vector2(0.5f, 0f);
            rtBarLabel.pivot = new Vector2(0.5f, 1f);
            rtBarLabel.anchoredPosition = new Vector2(0f, -5f);
            rtBarLabel.sizeDelta = new Vector2(240f, 20f);

            Text barcodeLabelTxt = barcodeLabelGo.AddComponent<Text>();
            barcodeLabelTxt.text = "EKT-7928-GAMES";
            barcodeLabelTxt.font = defaultFont;
            barcodeLabelTxt.fontSize = 24;
            barcodeLabelTxt.color = new Color(0.6f, 0.6f, 0.65f, 1f);
            barcodeLabelTxt.alignment = TextAnchor.MiddleCenter;

            // Barcode Scan Laser Line (Core + Glow)
            GameObject laserGlowGo = new GameObject("LaserGlow");
            laserGlowGo.transform.SetParent(barcodeContainer.transform, false);
            RectTransform rtLaserGlow = laserGlowGo.AddComponent<RectTransform>();
            rtLaserGlow.anchorMin = new Vector2(0.5f, 1f);
            rtLaserGlow.anchorMax = new Vector2(0.5f, 1f);
            rtLaserGlow.pivot = new Vector2(0.5f, 0.5f);
            rtLaserGlow.anchoredPosition = new Vector2(0f, 0f);
            rtLaserGlow.sizeDelta = new Vector2(280f, 7f);
            Image laserGlowImg = laserGlowGo.AddComponent<Image>();
            laserGlowImg.color = new Color(1f, 0f, 0.1f, 0.35f);

            GameObject laserCoreGo = new GameObject("LaserCore");
            laserCoreGo.transform.SetParent(laserGlowGo.transform, false);
            RectTransform rtLaserCore = laserCoreGo.AddComponent<RectTransform>();
            rtLaserCore.anchorMin = Vector2.zero;
            rtLaserCore.anchorMax = Vector2.one;
            rtLaserCore.sizeDelta = Vector2.zero;
            Image laserCoreImg = laserCoreGo.AddComponent<Image>();
            laserCoreImg.color = new Color(1f, 0.3f, 0.3f, 1f);

            // Neon Label "EKT GAMES" (Cyan Glow + White Core)
            GameObject neonGlowGo = new GameObject("NeonGlow");
            neonGlowGo.transform.SetParent(introPanel.transform, false);
            RectTransform rtNeonGlow = neonGlowGo.AddComponent<RectTransform>();
            rtNeonGlow.anchorMin = new Vector2(0.5f, 0.5f);
            rtNeonGlow.anchorMax = new Vector2(0.5f, 0.5f);
            rtNeonGlow.pivot = new Vector2(0.5f, 0.5f);
            rtNeonGlow.anchoredPosition = new Vector2(0f, -40f);
            rtNeonGlow.sizeDelta = new Vector2(600f, 80f);

            Text neonGlowTxt = neonGlowGo.AddComponent<Text>();
            neonGlowTxt.text = "EKT GAMES";
            neonGlowTxt.font = defaultFont;
            neonGlowTxt.fontSize = 80;
            neonGlowTxt.fontStyle = FontStyle.Bold;
            neonGlowTxt.alignment = TextAnchor.MiddleCenter;
            neonGlowTxt.color = new Color(0f, 0.7f, 1f, 0f); // Starts invisible

            Outline neonOutline = neonGlowGo.AddComponent<Outline>();
            neonOutline.effectColor = new Color(0f, 0.4f, 0.8f, 0.3f);
            neonOutline.effectDistance = new Vector2(3f, 3f);

            GameObject neonCoreGo = new GameObject("NeonCore");
            neonCoreGo.transform.SetParent(neonGlowGo.transform, false);
            RectTransform rtNeonCore = neonCoreGo.AddComponent<RectTransform>();
            rtNeonCore.anchorMin = Vector2.zero;
            rtNeonCore.anchorMax = Vector2.one;
            rtNeonCore.sizeDelta = Vector2.zero;

            Text neonCoreTxt = neonCoreGo.AddComponent<Text>();
            neonCoreTxt.text = "EKT GAMES";
            neonCoreTxt.font = defaultFont;
            neonCoreTxt.fontSize = 78;
            neonCoreTxt.fontStyle = FontStyle.Bold;
            neonCoreTxt.alignment = TextAnchor.MiddleCenter;
            neonCoreTxt.color = new Color(1f, 1f, 1f, 0f); // Starts invisible

            // Subtitle Presents
            GameObject presentsGo = new GameObject("PresentsSubtitle");
            presentsGo.transform.SetParent(introPanel.transform, false);
            RectTransform rtPresents = presentsGo.AddComponent<RectTransform>();
            rtPresents.anchorMin = new Vector2(0.5f, 0.5f);
            rtPresents.anchorMax = new Vector2(0.5f, 0.5f);
            rtPresents.pivot = new Vector2(0.5f, 0.5f);
            rtPresents.anchoredPosition = new Vector2(0f, -110f);
            rtPresents.sizeDelta = new Vector2(400f, 30f);

            Text presentsTxt = presentsGo.AddComponent<Text>();
            presentsTxt.text = "PRESENTS";
            presentsTxt.font = defaultFont;
            presentsTxt.fontSize = 24;
            presentsTxt.color = new Color(0.6f, 0.6f, 0.7f, 0f);
            presentsTxt.alignment = TextAnchor.MiddleCenter;

            // Animation Loop (Total 6.0 seconds, skippable by tap/click to open game fast)
            float elapsed = 0f;
            bool hasBeeped = false;
            CanvasGroup barcodeCg = barcodeContainer.AddComponent<CanvasGroup>();

            while (elapsed < 4.8f)
            {
                // Clamp delta time to prevent large time jumps during initial editor lag/loading
                elapsed += Mathf.Min(Time.unscaledDeltaTime, 0.05f);

                // Allow players to skip the intro instantly by tapping/clicking (Input System compatible)
                bool skipPressed = false;
                if (UnityEngine.InputSystem.Keyboard.current != null && 
                    (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame || 
                     UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame)) 
                {
                    skipPressed = true;
                }
                if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame) 
                {
                    skipPressed = true;
                }
                if (UnityEngine.InputSystem.Touchscreen.current != null)
                {
                    for (int i = 0; i < UnityEngine.InputSystem.Touchscreen.current.touches.Count; i++)
                    {
                        var touch = UnityEngine.InputSystem.Touchscreen.current.touches[i];
                        if (touch.press.wasPressedThisFrame)
                        {
                            skipPressed = true;
                            break;
                        }
                    }
                }

                // En az 1.5 saniye boyunca intro'nun atlanmasını engelle, böylece barkod ve başlangıç yazısı görünür
                if (skipPressed && elapsed > 1.5f) 
                {
                    break;
                }

                // Phase 1: Barcode Scan (0.0 to 2.0s)
                if (elapsed < 2.0f)
                {
                    float scanAlpha = Mathf.Clamp01((2.0f - elapsed) / 0.3f);
                    barcodeCg.alpha = scanAlpha;

                    float laserProgress = Mathf.Clamp01(elapsed / 1.5f);
                    rtLaserGlow.anchoredPosition = new Vector2(0f, -laserProgress * 60f);

                    // Scan beep sound trigger at 1.5s
                    if (elapsed >= 1.5f && !hasBeeped)
                    {
                        hasBeeped = true;
                        AudioClip beepClip = CreateProceduralBeepClip();
                        if (beepClip != null)
                        {
                            AudioSource.PlayClipAtPoint(beepClip, Camera.main != null ? Camera.main.transform.position : Vector3.zero, 0.4f);
                        }
                    }
                }
                else
                {
                    barcodeContainer.SetActive(false);
                }

                // Phase 2 & 3: Logo flickering & glowing (1.3s'de başlasın, barkod silinmeden hemen önce)
                if (elapsed >= 1.3f)
                {
                    float logoAlpha = Mathf.Clamp01((elapsed - 1.3f) / 0.4f);

                    bool isFlickerOn = true;
                    if (elapsed > 1.3f && elapsed < 2.1f)
                    {
                        isFlickerOn = (Random.value > 0.4f);
                    }

                    float pulse = Mathf.PingPong(Time.unscaledTime * 2.0f, 1f);
                    float neonIntensity = 0.6f + pulse * 0.4f;
                    if (elapsed > 1.3f && elapsed < 2.1f && !isFlickerOn)
                    {
                        neonIntensity = 0.05f;
                    }

                    neonGlowTxt.color = new Color(0f, 0.7f, 1f, neonIntensity * 0.8f * logoAlpha);
                    neonCoreTxt.color = new Color(1f, 1f, 1f, logoAlpha * (isFlickerOn ? 1f : 0.1f));

                    // Presents fade-in (1.4s'de başlasın, logo ile eşzamanlı)
                    float presentsAlpha = Mathf.Clamp01((elapsed - 1.4f) / 0.6f);
                    presentsTxt.color = new Color(0.6f, 0.6f, 0.7f, presentsAlpha);
                }

                // Smoothly fade-out everything as we approach 4.8 seconds
                if (elapsed >= 4.0f)
                {
                    float fadeOutAlpha = Mathf.Clamp01((4.8f - elapsed) / 0.8f);
                    cg.alpha = fadeOutAlpha;
                }

                yield return null;
            }

            // Cleanup & load welcome menu
            Destroy(introPanel);
            ShowWelcomeMenu();
        }

        private AudioClip CreateProceduralBeepClip()
        {
            int sampleRate = 44100;
            float duration = 0.08f; // Short beep tone
            float frequency = 1800f; // High frequency beep
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;
                float phase = 2f * Mathf.PI * frequency * t;
                samples[i] = Mathf.Sin(phase) * 0.4f * (1f - progress);
            }

            AudioClip clip = AudioClip.Create("ScanBeep", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void Update()
        {
            refreshTimer += Time.deltaTime;
            if (refreshTimer >= 0.2f)
            {
                refreshTimer = 0f;
                RefreshUI();
            }
        }

        private void InitializeSubpanels()
        {
            // Instantiates scripts and initializes their components inside subpanel content parent
            myPlayersPanel = subpanelContentParent.gameObject.AddComponent<MyPlayersPanel>();
            myPlayersPanel.Initialize(this, CreateSubpanelContentContainer("MyPlayers"));

            talentsPanel = subpanelContentParent.gameObject.AddComponent<TalentsPanel>();
            talentsPanel.Initialize(this, CreateSubpanelContentContainer("Talents"));

            newsPanel = subpanelContentParent.gameObject.AddComponent<NewsPanel>();
            newsPanel.Initialize(this, CreateSubpanelContentContainer("News"));

            socialFeedPanel = subpanelContentParent.gameObject.AddComponent<SocialFeedPanel>();
            socialFeedPanel.Initialize(this, CreateSubpanelContentContainer("SocialFeed"));

            financePanel = subpanelContentParent.gameObject.AddComponent<FinancePanel>();
            financePanel.Initialize(this, CreateSubpanelContentContainer("Finance"));

            privatePanel = subpanelContentParent.gameObject.AddComponent<PrivatePanel>();
            privatePanel.Initialize(this, CreateSubpanelContentContainer("Private"));

            storePanel = subpanelContentParent.gameObject.AddComponent<StorePanel>();
            storePanel.Initialize(this, CreateSubpanelContentContainer("Store"));

            allPlayersPanel = subpanelContentParent.gameObject.AddComponent<AllPlayersPanel>();
            allPlayersPanel.Initialize(this, CreateSubpanelContentContainer("AllPlayers"));

            leaguesPanel = subpanelContentParent.gameObject.AddComponent<LeaguesPanel>();
            leaguesPanel.Initialize(this, CreateSubpanelContentContainer("Leagues"));

            clubsPanel = subpanelContentParent.gameObject.AddComponent<ClubsPanel>();
            clubsPanel.Initialize(this, CreateSubpanelContentContainer("Clubs"));

            onlineMatchPanel = subpanelContentParent.gameObject.AddComponent<OnlineMatchPanel>();
            onlineMatchPanel.Initialize(this, CreateSubpanelContentContainer("OnlineMatch"));

            transfersPanel = subpanelContentParent.gameObject.AddComponent<TransfersPanel>();
            transfersPanel.Initialize(this, CreateSubpanelContentContainer("Transfers"));
        }

        private GameObject CreateSubpanelContentContainer(string name)
        {
            GameObject container = CreatePanelHelper(subpanelContentParent, name, Color.clear);
            SetRectTransform(container, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            container.SetActive(false);
            return container;
        }

        public void RefreshUI()
        {
            if (SimulationEngine.Instance == null || AgencyManager.Instance == null || AgencyManager.Instance.ActiveAgency == null) return;

            // Update stats texts (Standard build-safe icons)
            var agency = AgencyManager.Instance.ActiveAgency;
            moneyText.text = $"€ {agency.Balance:N0}";
            managerNameText.text = $"★ {agency.ManagerName}";
            agencyNameText.text = $"★ {agency.Name.Replace("Menajerlik", "").Replace("Ajansı", "").Trim()}";

            string rawWeek = $"⌛ Hafta {SimulationEngine.Instance.CurrentWeek}";
            weekText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate(rawWeek);

            string rawTrans = $"⚽ Transfer: {(SimulationEngine.Instance.IsTransferWindowOpen() ? "Açık" : "Kapalı")}";
            transferSeasonText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate(rawTrans);

            string rawRep = $"★ Şirket Seviyesi: {agency.Level} | İtibar: {agency.Reputation}/100";
            reputationText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate(rawRep);

            // Update Standings Widget with actual Selected League standings from database
            if (string.IsNullOrEmpty(SelectedLeagueName) || DatabaseManager.Instance.Leagues.Find(l => l.OriginalName == SelectedLeagueName) == null)
            {
                if (DatabaseManager.Instance.Leagues.Count > 0)
                {
                    League defaultLg = DatabaseManager.Instance.Leagues.Find(l => l.OriginalName == "Türkiye 1. Ligi");
                    if (defaultLg == null) defaultLg = DatabaseManager.Instance.Leagues[0];
                    SelectedLeagueName = defaultLg.OriginalName;
                }
            }
            League targetLeague = DatabaseManager.Instance.Leagues.Find(l => l.OriginalName == SelectedLeagueName);
            if (targetLeague == null) targetLeague = DatabaseManager.Instance.Leagues.Find(l => l.OriginalName == "Türkiye 1. Ligi");

            if (targetLeague != null)
            {
                if (widgetTitleText != null)
                {
                    string localizedTitle = BehindTheScenesFootball.Managers.LocalizationManager.Translate("PUAN DURUMU");
                    string localizedLeagueName = BehindTheScenesFootball.Managers.LocalizationManager.TranslateLeague(targetLeague.Name);
                    widgetTitleText.text = $"⚽ {localizedTitle.ToUpper()} ({localizedLeagueName.ToUpper()})";
                }
                
                List<Club> sortedClubs = new List<Club>(targetLeague.Clubs);
                sortedClubs.Sort((x, y) =>
                {
                    int cmp = y.StandingPoints.CompareTo(x.StandingPoints);
                    if (cmp == 0) cmp = y.StandingGD.CompareTo(x.StandingGD);
                    if (cmp == 0) cmp = y.StandingGF.CompareTo(x.StandingGF);
                    return cmp;
                });

                int limit = Mathf.Min(4, sortedClubs.Count);
                for (int i = 0; i < 4; i++)
                {
                    if (widgetRowLeftTexts[i] == null || widgetRowRightTexts[i] == null) continue;

                    if (i < limit)
                    {
                        Club c = sortedClubs[i];
                        widgetRowLeftTexts[i].text = $"{i + 1}. {c.Name}";
                        widgetRowRightTexts[i].text = $"{c.StandingPoints} P";
                    }
                    else
                    {
                        widgetRowLeftTexts[i].text = "";
                        widgetRowRightTexts[i].text = "";
                    }
                }
            }

            // Right widget displays a static "SOSYAL MEDYA" button inviting players to tap to enter.
            // No dynamic text updates here, to keep the home screen button extremely clean and readable.

            if (newsButtonText != null)
            {
                int activeCount = SimulationEngine.Instance.ActiveOffers.Count + SimulationEngine.Instance.ActiveMails.Count;
                if (activeCount > 0)
                {
                    string rawNewsText = $"Haberler <color=#FF3B30>📩 ({activeCount} Yeni)</color>";
                    newsButtonText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate(rawNewsText);
                }
                else
                {
                    newsButtonText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate("Haberler");
                }
            }

            if (talentsButtonText != null)
            {
                int readyReportsCount = 0;
                foreach (var scout in agency.HiredScouts)
                {
                    if (scout.WeeksRemaining == 0 && !string.IsNullOrEmpty(scout.AssignedLeague) && scout.ScoutedPlayerIds.Count > 0)
                    {
                        int validPlayersCount = 0;
                        foreach (var id in scout.ScoutedPlayerIds)
                        {
                            Player p = DatabaseManager.Instance.GetPlayerById(id);
                            if (p != null && !p.IsAgencyClient)
                            {
                                validPlayersCount++;
                            }
                        }
                        if (validPlayersCount > 0)
                        {
                            readyReportsCount++;
                        }
                    }
                }

                if (readyReportsCount > 0)
                {
                    string rawScoutText = $"Gözlemci Merkezi <color=#FF3B30>🔍 ({readyReportsCount} Rapor)</color>";
                    talentsButtonText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate(rawScoutText);
                }
                else
                {
                    talentsButtonText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate("Gözlemci Merkezi");
                }
            }

            // Refresh active subpanel if open and state has changed
            if (activeSubpanel != null)
            {
                int currentWeek = SimulationEngine.Instance.CurrentWeek;
                long currentBalance = agency.Balance;
                int currentClients = agency.Clients != null ? agency.Clients.Count : 0;
                int currentOffers = SimulationEngine.Instance.ActiveOffers != null ? SimulationEngine.Instance.ActiveOffers.Count : 0;
                int currentMails = SimulationEngine.Instance.ActiveMails != null ? SimulationEngine.Instance.ActiveMails.Count : 0;

                if (activeSubpanel != lastActiveSubpanel ||
                    currentWeek != lastRefreshedWeek ||
                    currentBalance != lastRefreshedBalance ||
                    currentClients != lastRefreshedClientsCount ||
                    currentOffers != lastRefreshedOffersCount ||
                    currentMails != lastRefreshedMailsCount)
                {
                    lastActiveSubpanel = activeSubpanel;
                    lastRefreshedWeek = currentWeek;
                    lastRefreshedBalance = currentBalance;
                    lastRefreshedClientsCount = currentClients;
                    lastRefreshedOffersCount = currentOffers;
                    lastRefreshedMailsCount = currentMails;

                    activeSubpanel.Refresh();
                }
            }
            else
            {
                lastActiveSubpanel = null;
            }
        }

        private string StripLogTime(string rawLog)
        {
            // Remove timestamp bracket e.g. [12:30:15]
            if (rawLog.StartsWith("[") && rawLog.Contains("]"))
            {
                int closeIndex = rawLog.IndexOf("]");
                return rawLog.Substring(closeIndex + 1).Trim();
            }
            return rawLog;
        }

        public void OpenSubpanel(BaseModulePanel panel, string title)
        {
            if (activeSubpanel != null)
            {
                activeSubpanel.Close();
            }

            homeScreenObj.SetActive(false);
            subpanelContainerObj.SetActive(true);
            subpanelTitleText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate(title);
            var localizable = subpanelTitleText.GetComponent<BehindTheScenesFootball.Managers.LocalizableText>();
            if (localizable != null)
            {
                localizable.originalText = title;
            }
            
            activeSubpanel = panel;
            activeSubpanel.Open();
        }

        public void OpenClubDetails(Club club)
        {
            if (clubsPanel != null)
            {
                clubsPanel.SelectClub(club);
                OpenSubpanel(clubsPanel, "KULÜP DETAYLARI");
            }
        }

        public void ReturnToMainMenu()
        {
            if (activeSubpanel != null)
            {
                activeSubpanel.Close();
                activeSubpanel = null;
            }

            subpanelContainerObj.SetActive(false);
            homeScreenObj.SetActive(true);
            RefreshUI();
        }

        #region UI Construction

        private void CreateUIElements()
        {
            // 1. Create Canvas
            GameObject canvasObj = new GameObject("SimulationCanvas");
            canvasObj.transform.SetParent(transform);
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0f; // Scale based on width for mobile portrait

            canvasObj.AddComponent<GraphicRaycaster>();

            // Ensure EventSystem exists and is configured with the correct Input Module
            UnityEngine.EventSystems.EventSystem existingEventSystem = FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            GameObject eventSystemObj;
            if (existingEventSystem == null)
            {
                eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            }
            else
            {
                eventSystemObj = existingEventSystem.gameObject;
            }

            System.Type newModuleType = null;
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                newModuleType = assembly.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule");
                if (newModuleType != null)
                    break;
            }

            if (newModuleType != null)
            {
                var legacyModule = eventSystemObj.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                if (legacyModule != null)
                {
                    Destroy(legacyModule);
                }
                if (eventSystemObj.GetComponent(newModuleType) == null)
                {
                    eventSystemObj.AddComponent(newModuleType);
                }
            }
            else
            {
                if (eventSystemObj.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>() == null)
                {
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
            }

            // 2. Main Background Panel
            GameObject mainPanel = CreatePanelHelper(canvasObj.transform, "MainPanel", colorBg);
            SetRectTransform(mainPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            
            // Try loading background image
            Image bgImg = mainPanel.GetComponent<Image>();
            if (bgImg != null)
            {
                ApplyBackgroundImage(bgImg);
            }

            // 3. Create Home Screen Container
            homeScreenObj = CreatePanelHelper(mainPanel.transform, "HomeScreen", Color.clear);
            SetRectTransform(homeScreenObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // 4. Create Subpanel Container
            subpanelContainerObj = CreatePanelHelper(mainPanel.transform, "SubpanelContainer", Color.clear);
            SetRectTransform(subpanelContainerObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            subpanelContainerObj.SetActive(false);

            // Subpanel Header
            GameObject subHeader = CreatePanelHelper(subpanelContainerObj.transform, "SubHeader", colorPanel);
            SetRectTransform(subHeader, new Vector2(0f, 0.88f), new Vector2(1f, 0.98f), new Vector2(20f, 0f), new Vector2(-20f, -20f));

            // Back Button inside SubHeader (Global Geri butonu)
            GameObject backBtnObj = new GameObject("BtnBack");
            backBtnObj.transform.SetParent(subHeader.transform, false);
            
            RectTransform backRt = backBtnObj.AddComponent<RectTransform>();
            backRt.anchorMin = new Vector2(0f, 0.5f);
            backRt.anchorMax = new Vector2(0f, 0.5f);
            backRt.pivot = new Vector2(0f, 0.5f);
            backRt.anchoredPosition = new Vector2(20f, 0f);
            backRt.sizeDelta = new Vector2(180f, 80f);

            Image backImg = backBtnObj.AddComponent<Image>();
            backImg.color = new Color(231f / 255f, 76f / 255f, 60f / 255f, 1f); // Soft Red
            if (roundedButtonSprite != null)
            {
                backImg.sprite = roundedButtonSprite;
                backImg.type = Image.Type.Sliced;
            }

            Button backBtn = backBtnObj.AddComponent<Button>();
            ConfigureButtonTransition(backBtn);
            backBtn.onClick.AddListener(() => ReturnToMainMenu());

            Text backText = CreateText(backBtnObj.transform, "Label", "GERİ", 38, Color.white, TextAnchor.MiddleCenter);
            SetRectTransform(backText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            backText.fontStyle = FontStyle.Bold;

            subpanelTitleText = CreateText(subHeader.transform, "SubTitle", "PANEL", 60, Color.white, TextAnchor.MiddleCenter);
            SetRectTransform(subpanelTitleText, new Vector2(0.25f, 0f), new Vector2(0.75f, 1f), Vector2.zero, Vector2.zero);
            subpanelTitleText.fontStyle = FontStyle.Bold;

            // Subpanel Content Parent
            GameObject subContentParentObj = CreatePanelHelper(subpanelContainerObj.transform, "SubContentParent", Color.clear);
            SetRectTransform(subContentParentObj, new Vector2(0f, 0f), new Vector2(1f, 0.86f), new Vector2(20f, 20f), new Vector2(-20f, 0f));
            subpanelContentParent = subContentParentObj.transform;

            // --- Home Screen Layout ---
            // Header Stats Block (Expanded height!)
            GameObject header = CreatePanelHelper(homeScreenObj.transform, "HeaderStats", colorPanel);
            SetRectTransform(header, new Vector2(0f, 0.79f), new Vector2(1f, 0.98f), new Vector2(20f, 0f), new Vector2(-20f, -20f));

            // Grid Stats inside header with standard build-safe unicode icons (Enlarged and spaced out!)
            moneyText = CreateText(header.transform, "MoneyText", "€ 0", 54, new Color(46f/255f, 204f/255f, 113f/255f), TextAnchor.MiddleLeft);
            SetRectTransform(moneyText, new Vector2(0.04f, 0.68f), new Vector2(0.48f, 0.95f), Vector2.zero, Vector2.zero);
            moneyText.fontStyle = FontStyle.Bold;

            weekText = CreateText(header.transform, "WeekText", "⌛ Hafta 1", 48, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(weekText, new Vector2(0.04f, 0.36f), new Vector2(0.48f, 0.63f), Vector2.zero, Vector2.zero);
            weekText.fontStyle = FontStyle.Bold;

            transferSeasonText = CreateText(header.transform, "TransferText", "⚽ Transfer: Kapalı", 48, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(transferSeasonText, new Vector2(0.04f, 0.04f), new Vector2(0.48f, 0.31f), Vector2.zero, Vector2.zero);
            transferSeasonText.fontStyle = FontStyle.Bold;

            managerNameText = CreateText(header.transform, "ManagerText", "★ Kasey Sung", 48, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(managerNameText, new Vector2(0.52f, 0.68f), new Vector2(0.90f, 0.95f), Vector2.zero, Vector2.zero);
            managerNameText.fontStyle = FontStyle.Bold;

            agencyNameText = CreateText(header.transform, "AgencyText", "★ Arka Bahçe", 48, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(agencyNameText, new Vector2(0.52f, 0.36f), new Vector2(0.90f, 0.63f), Vector2.zero, Vector2.zero);
            agencyNameText.fontStyle = FontStyle.Bold;

            reputationText = CreateText(header.transform, "RepText", "★ Şirket Seviyesi: 1 | İtibar: 0/100", 66, colorAccent, TextAnchor.MiddleLeft); // 54 -> 66 (Şirket seviyesi yazısını büyüttük)
            SetRectTransform(reputationText, new Vector2(0.52f, 0.04f), new Vector2(0.90f, 0.31f), Vector2.zero, Vector2.zero);
            reputationText.fontStyle = FontStyle.Bold;

            // Pause Button (⏸) at top-right of header
            Text pauseBtnLabel = CreateButtonHelper(header.transform, "BtnPause", "⏸", new Color(0.12f, 0.16f, 0.22f, 0.85f), Color.white, () => TogglePauseMenu(true));
            SetRectTransform(pauseBtnLabel.transform.parent, new Vector2(0.92f, 0.20f), new Vector2(0.98f, 0.80f), Vector2.zero, Vector2.zero);
            pauseBtnLabel.fontSize = 42;
            pauseBtnLabel.fontStyle = FontStyle.Bold;

            // Overview Widgets (Middle - Left: Puan Durumu, Right: Twitter Feed - Expanded taller layout!)
            GameObject widgetsContainer = CreatePanelHelper(homeScreenObj.transform, "WidgetsContainer", Color.clear);
            SetRectTransform(widgetsContainer, new Vector2(0f, 0.52f), new Vector2(1f, 0.78f), new Vector2(20f, 0f), new Vector2(-20f, 0f));

            // Left Widget: Clickable Standings (Stacked on top - Full screen width with expanded height!)
            Color blueBg = new Color(41f / 255f, 128f / 255f, 185f / 255f, 0.25f);
            Color blueBorder = new Color(41f / 255f, 128f / 255f, 185f / 255f, 0.75f);
            
            GameObject leftWidget = CreatePanelHelper(widgetsContainer.transform, "LeftWidget", blueBg);
            SetRectTransform(leftWidget, new Vector2(0f, 0.20f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            Image leftImg = leftWidget.GetComponent<Image>();
            if (leftImg != null && roundedButtonSprite != null)
            {
                leftImg.sprite = roundedButtonSprite;
                leftImg.type = Image.Type.Sliced;
            }
            
            Outline standingOutline = leftWidget.AddComponent<Outline>();
            standingOutline.effectColor = blueBorder;
            standingOutline.effectDistance = new Vector2(3f, 3f);

            Button leftBtn = leftWidget.AddComponent<Button>();
            ConfigureButtonTransition(leftBtn);
            leftBtn.onClick.AddListener(() => {
                OpenSubpanel(leaguesPanel, "LİG DETAYLARI");
            });

            // Widget Header Title (Centered, standard build-safe icon - ENLARGED to 48pt!)
            widgetTitleText = CreateText(leftWidget.transform, "WidgetTitle", "⚽ PUAN DURUMU", 48, Color.white, TextAnchor.MiddleCenter);
            var titleLocalizable = widgetTitleText.GetComponent<BehindTheScenesFootball.Managers.LocalizableText>();
            if (titleLocalizable != null)
            {
                titleLocalizable.originalText = "⚽ PUAN DURUMU";
            }
            SetRectTransform(widgetTitleText, new Vector2(0.05f, 0.84f), new Vector2(0.95f, 0.96f), Vector2.zero, Vector2.zero);
            widgetTitleText.fontStyle = FontStyle.Bold;

            // Widget Rows (Column aligned, left name, right points - ENLARGED to 46pt, uniform gaps!)
            widgetRowLeftTexts[0] = CreateRowText(leftWidget.transform, "RowLeft0", new Vector2(0.05f, 0.67f), new Vector2(0.80f, 0.79f), TextAnchor.MiddleLeft);
            widgetRowRightTexts[0] = CreateRowText(leftWidget.transform, "RowRight0", new Vector2(0.80f, 0.67f), new Vector2(0.95f, 0.79f), TextAnchor.MiddleRight);
            widgetRowRightTexts[0].color = colorAccent;

            widgetRowLeftTexts[1] = CreateRowText(leftWidget.transform, "RowLeft1", new Vector2(0.05f, 0.52f), new Vector2(0.80f, 0.64f), TextAnchor.MiddleLeft);
            widgetRowRightTexts[1] = CreateRowText(leftWidget.transform, "RowRight1", new Vector2(0.80f, 0.52f), new Vector2(0.95f, 0.64f), TextAnchor.MiddleRight);
            widgetRowRightTexts[1].color = colorAccent;

            widgetRowLeftTexts[2] = CreateRowText(leftWidget.transform, "RowLeft2", new Vector2(0.05f, 0.37f), new Vector2(0.80f, 0.49f), TextAnchor.MiddleLeft);
            widgetRowRightTexts[2] = CreateRowText(leftWidget.transform, "RowRight2", new Vector2(0.80f, 0.37f), new Vector2(0.95f, 0.49f), TextAnchor.MiddleRight);
            widgetRowRightTexts[2].color = colorAccent;

            widgetRowLeftTexts[3] = CreateRowText(leftWidget.transform, "RowLeft3", new Vector2(0.05f, 0.22f), new Vector2(0.80f, 0.34f), TextAnchor.MiddleLeft);
            widgetRowRightTexts[3] = CreateRowText(leftWidget.transform, "RowRight3", new Vector2(0.80f, 0.22f), new Vector2(0.95f, 0.34f), TextAnchor.MiddleRight);
            widgetRowRightTexts[3].color = colorAccent;

            // Widget Footer Tap Hint (Centered - ENLARGED to 42pt Bold Italic, completely cleared of Row 3!)
            Text tapHint = CreateText(leftWidget.transform, "TapHint", "Detaylar için dokun...", 42, colorAccent, TextAnchor.MiddleCenter);
            SetRectTransform(tapHint, new Vector2(0.05f, 0.04f), new Vector2(0.95f, 0.16f), Vector2.zero, Vector2.zero);
            tapHint.fontStyle = FontStyle.BoldAndItalic;

            // Right Widget: Clickable Social Media Panel (Stacked on bottom - Full screen width!)
            Color twitterBg = new Color(29f / 255f, 161f / 255f, 242f / 255f, 0.25f);
            Color twitterBorder = new Color(29f / 255f, 161f / 255f, 242f / 255f, 0.75f);

            GameObject rightWidget = CreatePanelHelper(widgetsContainer.transform, "RightWidget", twitterBg);
            SetRectTransform(rightWidget, new Vector2(0f, 0f), new Vector2(1f, 0.14f), Vector2.zero, Vector2.zero);
            Image rightImg = rightWidget.GetComponent<Image>();
            if (rightImg != null && roundedButtonSprite != null)
            {
                rightImg.sprite = roundedButtonSprite;
                rightImg.type = Image.Type.Sliced;
            }
            
            Outline matchesOutline = rightWidget.AddComponent<Outline>();
            matchesOutline.effectColor = twitterBorder;
            matchesOutline.effectDistance = new Vector2(3f, 3f);

            Button rightBtn = rightWidget.AddComponent<Button>();
            ConfigureButtonTransition(rightBtn);
            rightBtn.onClick.AddListener(() => OpenSubpanel(socialFeedPanel, "SOSYAL MEDYA"));

            matchesWidgetText = CreateText(rightWidget.transform, "MatchesWidget", "<b>✉ SOSYAL MEDYA</b>", 54, Color.white, TextAnchor.MiddleCenter);
            SetRectTransform(matchesWidgetText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            matchesWidgetText.fontStyle = FontStyle.Bold;

            // Core Folder Grid Buttons (2x3 green cards - shifted down)
            GameObject coreGrid = CreatePanelHelper(homeScreenObj.transform, "CoreGrid", Color.clear);
            SetRectTransform(coreGrid, new Vector2(0f, 0.18f), new Vector2(1f, 0.51f), new Vector2(20f, 0f), new Vector2(-20f, 0f));

            // Row 1
            Text btnMyPlayers = CreateButtonHelper(coreGrid.transform, "BtnMyPlayers", "Müşterilerim", colorGreen, Color.white, () => OpenSubpanel(myPlayersPanel, "MÜŞTERİLERİM"));
            SetRectTransform(btnMyPlayers.transform.parent, new Vector2(0.02f, 0.68f), new Vector2(0.48f, 0.98f), Vector2.zero, Vector2.zero);
            btnMyPlayers.fontSize = 48;
            btnMyPlayers.fontStyle = FontStyle.Bold;

            talentsButtonText = CreateButtonHelper(coreGrid.transform, "BtnTalents", "Gözlemci Merkezi", colorGreen, Color.white, () => OpenSubpanel(talentsPanel, "GÖZLEMCİ MERKEZİ"));
            SetRectTransform(talentsButtonText.transform.parent, new Vector2(0.52f, 0.68f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero);
            talentsButtonText.fontSize = 48;
            talentsButtonText.fontStyle = FontStyle.Bold;

            // Row 2
            newsButtonText = CreateButtonHelper(coreGrid.transform, "BtnNews", "Haberler", colorGreen, Color.white, () => OpenSubpanel(newsPanel, "HABERLER & E-POSTA"));
            SetRectTransform(newsButtonText.transform.parent, new Vector2(0.02f, 0.35f), new Vector2(0.48f, 0.65f), Vector2.zero, Vector2.zero);
            newsButtonText.fontSize = 48;
            newsButtonText.fontStyle = FontStyle.Bold;

            Text btnFinance = CreateButtonHelper(coreGrid.transform, "BtnFinance", "Finans", colorGreen, Color.white, () => OpenSubpanel(financePanel, "AJANS BÜTÇESİ & FİNANS"));
            SetRectTransform(btnFinance.transform.parent, new Vector2(0.52f, 0.35f), new Vector2(0.98f, 0.65f), Vector2.zero, Vector2.zero);
            btnFinance.fontSize = 48;
            btnFinance.fontStyle = FontStyle.Bold;

            // Row 3
            Text btnPrivate = CreateButtonHelper(coreGrid.transform, "BtnPrivate", "Özel Hayat", colorGreen, Color.white, () => OpenSubpanel(privatePanel, "ÖZEL HAYAT & MÜLKLER"));
            SetRectTransform(btnPrivate.transform.parent, new Vector2(0.02f, 0.02f), new Vector2(0.48f, 0.32f), Vector2.zero, Vector2.zero);
            btnPrivate.fontSize = 48;
            btnPrivate.fontStyle = FontStyle.Bold;

            Text btnStore = CreateButtonHelper(coreGrid.transform, "BtnStore", "Mağaza", colorGreen, Color.white, () => OpenSubpanel(storePanel, "LÜKS MAĞAZA & PRESTİJ"));
            SetRectTransform(btnStore.transform.parent, new Vector2(0.52f, 0.02f), new Vector2(0.98f, 0.32f), Vector2.zero, Vector2.zero);
            btnStore.fontSize = 48;
            btnStore.fontStyle = FontStyle.Bold;

            // Bottom Grid Buttons (Single symmetric row of 4 buttons)
            GameObject bottomGrid = CreatePanelHelper(homeScreenObj.transform, "BottomGrid", Color.clear);
            SetRectTransform(bottomGrid, new Vector2(0f, 0.10f), new Vector2(1f, 0.17f), new Vector2(20f, 0f), new Vector2(-20f, 0f));

            Text btnAllPlayers = CreateButtonHelper(bottomGrid.transform, "BtnAllPlayers", "Tüm Oyuncular", colorGreyButton, Color.white, () => OpenSubpanel(allPlayersPanel, "OYUNCU PİYASASI"));
            SetRectTransform(btnAllPlayers.transform.parent, new Vector2(0.01f, 0.05f), new Vector2(0.24f, 0.95f), Vector2.zero, Vector2.zero);
            btnAllPlayers.fontSize = 42;
            btnAllPlayers.fontStyle = FontStyle.Bold;

            Text btnLeagues = CreateButtonHelper(bottomGrid.transform, "BtnLeagues", "Ligler", colorGreyButton, Color.white, () => OpenSubpanel(leaguesPanel, "LİG DETAYLARI"));
            SetRectTransform(btnLeagues.transform.parent, new Vector2(0.26f, 0.05f), new Vector2(0.49f, 0.95f), Vector2.zero, Vector2.zero);
            btnLeagues.fontSize = 42;
            btnLeagues.fontStyle = FontStyle.Bold;

            Text btnClubs = CreateButtonHelper(bottomGrid.transform, "BtnClubs", "Kulüpler", colorGreyButton, Color.white, () => OpenSubpanel(clubsPanel, "KULÜP BİLGİLERİ"));
            SetRectTransform(btnClubs.transform.parent, new Vector2(0.51f, 0.05f), new Vector2(0.74f, 0.95f), Vector2.zero, Vector2.zero);
            btnClubs.fontSize = 42;
            btnClubs.fontStyle = FontStyle.Bold;

            Text btnTransfers = CreateButtonHelper(bottomGrid.transform, "BtnTransfers", "Transferler", colorGreyButton, Color.white, () => OpenSubpanel(transfersPanel, "YAPILAN TRANSFERLER"));
            SetRectTransform(btnTransfers.transform.parent, new Vector2(0.76f, 0.05f), new Vector2(0.99f, 0.95f), Vector2.zero, Vector2.zero);
            btnTransfers.fontSize = 42;
            btnTransfers.fontStyle = FontStyle.Bold;

            // Large Action Button: Next Week
            GameObject actionPanel = CreatePanelHelper(homeScreenObj.transform, "ActionPanel", Color.clear);
            SetRectTransform(actionPanel, new Vector2(0f, 0.02f), new Vector2(1f, 0.09f), new Vector2(20f, 0f), new Vector2(-20f, 0f));

            Text btnNextWeek = CreateButtonHelper(actionPanel.transform, "BtnNextWeek", "HAFTAYI İLERLET ▶", colorNextWeek, Color.white, () => {
                SimulationEngine.Instance.AdvanceOneWeek();
            });
            SetRectTransform(btnNextWeek.transform.parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            btnNextWeek.fontSize = 54;
            btnNextWeek.fontStyle = FontStyle.Bold;

            // Create Main Menu and setup startup states
            CreateMainMenuScreen(mainPanel.transform);
            homeScreenObj.SetActive(false);
            mainMenuObj.SetActive(true);
        }

        #endregion

        #region UI Construction Helpers

        public GameObject CreatePanelHelper(Transform parent, string name, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            Image img = obj.AddComponent<Image>();
            img.color = color;
            if (color.a == 0f)
            {
                img.raycastTarget = false;
            }
            return obj;
        }

        private Text CreateText(Transform parent, string name, string text, int fontSize, Color color, TextAnchor alignment)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            Text txt = obj.AddComponent<Text>();
            var localizable = obj.AddComponent<BehindTheScenesFootball.Managers.LocalizableText>();
            localizable.originalText = text;
            txt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate(text);
            txt.font = defaultFont;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = alignment;
            txt.supportRichText = true;
            txt.raycastTarget = false;
            
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;

            obj.AddComponent<TextScaler>();
            
            return txt;
        }

        private Text CreateRowText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, TextAnchor alignment)
        {
            Text txt = CreateText(parent, name, "", 46, Color.white, alignment);
            SetRectTransform(txt, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            txt.fontStyle = FontStyle.Bold;
            return txt;
        }

        public Text CreateButtonHelper(Transform parent, string name, string label, Color bgCol, Color labelCol, System.Action onClickAction)
        {
            // Outer container has the semi-transparent color (bgCol)
            GameObject btnObj = CreatePanelHelper(parent, name, bgCol);
            Image img = btnObj.GetComponent<Image>();
            if (img != null && roundedButtonSprite != null)
            {
                img.sprite = roundedButtonSprite;
                img.type = Image.Type.Sliced;
            }
            Button btn = btnObj.AddComponent<Button>();
            ConfigureButtonTransition(btn);

            // Resolve border color based on the target button color
            Color borderColor = Color.white;
            if (bgCol == colorGreen) borderColor = new Color(46f / 255f, 204f / 255f, 113f / 255f, 0.85f); // Glowing Green
            else if (bgCol == colorRed) borderColor = new Color(231f / 255f, 76f / 255f, 60f / 255f, 0.85f); // Glowing Red
            else if (bgCol == colorGreyButton) borderColor = new Color(127f / 255f, 140f / 255f, 141f / 255f, 0.85f); // Glowing Slate Grey
            else if (bgCol == colorNextWeek) borderColor = colorAccent; // Cyan neon for next week
            else if (bgCol == colorAccent) borderColor = colorAccent;

            // Add built-in Outline component to draw a clean border!
            Outline outline = btnObj.AddComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(4f, 4f); // 4px border

            btn.onClick.AddListener(() => onClickAction?.Invoke());

            // Label Text
            Text textObj = CreateText(btnObj.transform, "Label", label, 28, labelCol, TextAnchor.MiddleCenter);
            SetRectTransform(textObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            textObj.fontStyle = FontStyle.Bold;
            textObj.resizeTextForBestFit = true;
            textObj.resizeTextMinSize = 12;
            textObj.resizeTextMaxSize = 42;

            return textObj;
        }

        public GameObject CreateScrollViewHelper(Transform parent, string name, out Transform content)
        {
            GameObject svObj = CreatePanelHelper(parent, name, new Color(0, 0, 0, 0.2f));
            ScrollRect sr = svObj.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            
            // Viewport is slightly shrunken on the right (by 40px) to accommodate the vertical scrollbar
            GameObject viewport = CreatePanelHelper(svObj.transform, "Viewport", Color.clear);
            SetRectTransform(viewport, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-40f, 0f));
            viewport.AddComponent<RectMask2D>();
            sr.viewport = viewport.GetComponent<RectTransform>();

            GameObject contentObj = CreatePanelHelper(viewport.transform, "Content", Color.clear);
            RectTransform contentRt = contentObj.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.padding = new RectOffset(10, 10, 10, 10);

            ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = contentRt;
            content = contentRt.transform;

            // Add programmatic Vertical Scrollbar
            GameObject sbObj = CreatePanelHelper(svObj.transform, "Scrollbar", new Color(0.1f, 0.12f, 0.15f, 0.6f));
            SetRectTransform(sbObj, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-30f, 10f), new Vector2(-5f, -10f));
            
            Scrollbar sb = sbObj.AddComponent<Scrollbar>();
            sb.direction = Scrollbar.Direction.BottomToTop;

            GameObject slidingArea = new GameObject("SlidingArea");
            slidingArea.transform.SetParent(sbObj.transform, false);
            SetRectTransform(slidingArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            GameObject handle = CreatePanelHelper(slidingArea.transform, "Handle", new Color(0.3f, 0.35f, 0.45f, 0.85f));
            SetRectTransform(handle, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image handleImg = handle.GetComponent<Image>();
            if (handleImg != null && roundedButtonSprite != null)
            {
                handleImg.sprite = roundedButtonSprite;
                handleImg.type = Image.Type.Sliced;
            }

            sb.handleRect = handle.GetComponent<RectTransform>();
            sr.verticalScrollbar = sb;

            return svObj;
        }

        public void SetRectTransform(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
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

        public void SetRectTransform(Component comp, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            if (comp != null)
            {
                SetRectTransform(comp.gameObject, anchorMin, anchorMax, offsetMin, offsetMax);
            }
        }

        private void ApplyBackgroundImage(Image bgImageComponent)
        {
            string path = System.IO.Path.Combine(Application.dataPath, "background.jpg");
            if (System.IO.File.Exists(path))
            {
                try
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(path);
                    Texture2D texture = new Texture2D(2, 2);
                    if (texture.LoadImage(bytes))
                    {
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        bgImageComponent.sprite = sprite;
                        // Dim slightly (multiply by dark color) so the overlay text stands out
                        bgImageComponent.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Error loading background image: " + ex.Message);
                    bgImageComponent.color = colorBg;
                }
            }
            else
            {
                bgImageComponent.color = colorBg;
            }
        }

        public string FormatAsTweet(string message)
        {
            if (string.IsNullOrEmpty(message)) return message;
            
            bool isEnglish = BehindTheScenesFootball.Managers.LocalizationManager.CurrentLanguage == "EN";

            var oldState = UnityEngine.Random.state;
            UnityEngine.Random.InitState(message.GetHashCode());

            string[] generalAccounts = isEnglish ? new string[] {
                "@TribuneRumors",
                "@FootballAgenda",
                "@AnalysisNotebook",
                "@CornerTaken",
                "@OffsideFlag",
                "@ReporterSelim",
                "@PunditMert",
                "@InsidePitch",
                "@BehindGoal",
                "@SprintSport",
                "@MarkingAgenda",
                "@OffPitchRumors",
                "@CounterAttack",
                "@PassTraffic",
                "@TacticsBoard"
            } : new string[] {
                "@TribunKulis",
                "@FutbolGundem",
                "@AnalizDefteri",
                "@KornerKullanildi",
                "@OfsaytBayragi",
                "@MuhabirSelim",
                "@YorumcuMert",
                "@SahaninIci",
                "@KaleArkasi",
                "@DeparSpor",
                "@MarkajGundem",
                "@SahaDisiKulis",
                "@KontraAtak",
                "@PasTrafigi",
                "@TaktikTahtasi"
            };

            string[] reporterAccounts = isEnglish ? new string[] {
                "@FabrizioBorsano",
                "@YagizKomuroglu"
            } : new string[] {
                "@FabrizioBorsano",
                "@YagizKömüroglu"
            };

            string randomGeneral = generalAccounts[UnityEngine.Random.Range(0, generalAccounts.Length)];
            string randomReporter = reporterAccounts[UnityEngine.Random.Range(0, reporterAccounts.Length)];

            string translatedMsg = BehindTheScenesFootball.Managers.LocalizationManager.Translate(message);
            string result = translatedMsg;

            if (message.Contains("Temsilcilik sözleşmesi") || message.Contains("Yeni müşteri kazanıldı") || message.Contains("temsilci sözleşmesi"))
            {
                string name = ExtractPlayerName(message);
                result = isEnglish 
                    ? $"<b>@BonservisNet:</b> The agency officially announced signing a contract with young star <b>{name}</b>! #agent #transfer"
                    : $"<b>@BonservisNet:</b> Temsilci ajansı, genç yıldız <b>{name}</b> ile sözleşme imzaladığını resmen duyurdu! #temsilci #transfer";
            }
            else if (message.Contains("TRANSFER BOMBASI"))
            {
                string stripped = translatedMsg.Replace("🔥 TRANSFER BOMBASI: ", "").Replace("TRANSFER BOMBASI: ", "").Replace("🔥 TRANSFER BOMB: ", "").Replace("TRANSFER BOMB: ", "");
                result = isEnglish
                    ? $"<b>@BonservisNet:</b> [OFFICIAL] <b>BREAKING NEWS!</b> {stripped} #transfer #fee"
                    : $"<b>@BonservisNet:</b> [RESMİ] <b>FLAŞ HABER!</b> {stripped} #transfer #bonservis";
            }
            else if (message.Contains("TEKLİF ALINDI") || message.Contains("teklif yaptı"))
            {
                result = isEnglish
                    ? $"<b>{randomReporter}:</b> [EXCLUSIVE NEWS] Common sources confirmed; {translatedMsg} #transfer #rumor"
                    : $"<b>{randomReporter}:</b> [ÖZEL HABER] Müşterek kaynaklar doğruladı; {translatedMsg} #transfer #duyum";
            }
            else if (message.Contains("MÜŞTERİ UYARISI") || message.Contains("KRİZ") || message.Contains("mutsuz"))
            {
                result = isEnglish
                    ? $"<b>{randomReporter}:</b> [HOT DEVELOPMENT] According to sources from the agency corridors; {translatedMsg} #crisis #rumors"
                    : $"<b>{randomReporter}:</b> [SICAK GELİŞME] Ajans koridorlarından alınan bilgilere göre; {translatedMsg} #kriz #kulis";
            }
            else if (message.Contains("SPONSOR ANLAŞMASI") || message.Contains("sponsorluk"))
            {
                result = isEnglish
                    ? $"<b>@BonservisNet:</b> [SPONSOR] {translatedMsg} #business #economy"
                    : $"<b>@BonservisNet:</b> [SPONSOR] {translatedMsg} #ticaret #ekonomi";
            }
            else if (message.Contains("Sözleşme Bitti"))
            {
                result = isEnglish
                    ? $"<b>@BonservisNet:</b> [FREE AGENT] {translatedMsg} #freeagent"
                    : $"<b>@BonservisNet:</b> [SERBEST] {translatedMsg} #serbest";
            }
            else
            {
                result = $"<b>{randomGeneral}:</b> {translatedMsg}";
            }

            UnityEngine.Random.state = oldState;
            return result;
        }

        private string ExtractPlayerName(string log)
        {
            int colonIndex = log.IndexOf(":");
            if (colonIndex != -1)
            {
                string rest = log.Substring(colonIndex + 1).Trim();
                int parenIndex = rest.IndexOf("(");
                if (parenIndex != -1)
                {
                    return rest.Substring(0, parenIndex).Trim();
                }
                return rest;
            }
            return "Oyuncu";
        }

        private void GenerateRoundedSprite()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float radius = 32f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(0, radius - x, x - (size - radius));
                    float dy = Mathf.Max(0, radius - y, y - (size - radius));
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > radius)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else if (dist > radius - 1.5f)
                    {
                        float alpha = 1f - (dist - (radius - 1.5f)) / 1.5f;
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.white);
                    }
                }
            }
            tex.Apply();
            roundedButtonSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        public Sprite GetMiniface(Player p)
        {
            if (minifaceCache.TryGetValue(p.Id, out Sprite sp))
            {
                return sp;
            }

            int size = 512; // 256 -> 512 (Miniface çözünürlüğünü 2 katına çıkararak pürüzsüzlük sağladık)
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear; // Smooth bilinear filtering!

            // Her oyuncunun yüzünün benzersiz olması için detaylı seed karması oluşturuldu
            int seed = (p.Id + p.Name + p.Age + p.OVR + p.Position.ToString()).GetHashCode();
            UnityEngine.Random.State oldState = UnityEngine.Random.state;
            UnityEngine.Random.InitState(seed);

            // Ülkeye göre gerçekçi ten rengi ve saç rengi üretimi
            Color skinCol = new Color(0.96f, 0.85f, 0.76f); // Varsayılan açık ten
            Color hairCol = new Color(0.08f, 0.08f, 0.08f); // Varsayılan siyah saç
            
            string nat = p.Nationality.ToLower();
            int randChoice = UnityEngine.Random.Range(0, 100);

            if (nat.Contains("türkiye") || nat.Contains("turkey"))
            {
                skinCol = (randChoice < 85) ? new Color(0.93f, 0.77f, 0.63f) : new Color(0.96f, 0.85f, 0.76f); // Akdeniz / Açık ten
                int hairRand = UnityEngine.Random.Range(0, 100);
                if (hairRand < 60) hairCol = new Color(0.08f, 0.08f, 0.08f); // Siyah
                else if (hairRand < 95) hairCol = new Color(0.35f, 0.22f, 0.12f); // Kahverengi
                else hairCol = new Color(0.65f, 0.28f, 0.12f); // Kızıl
            }
            else if (nat.Contains("brezilya") || nat.Contains("brazil"))
            {
                if (randChoice < 25) skinCol = new Color(0.32f, 0.20f, 0.12f); // Siyahi
                else if (randChoice < 70) skinCol = new Color(0.74f, 0.55f, 0.40f); // Esmer / Melez
                else skinCol = new Color(0.91f, 0.74f, 0.59f); // Buğday / Beyaz
                hairCol = (UnityEngine.Random.Range(0, 100) < 70) ? new Color(0.08f, 0.08f, 0.08f) : new Color(0.35f, 0.22f, 0.12f);
            }
            else if (nat.Contains("ingiltere") || nat.Contains("england") || 
                     nat.Contains("hollanda") || nat.Contains("netherlands") ||
                     nat.Contains("belçika") || nat.Contains("belgium") ||
                     nat.Contains("almanya") || nat.Contains("germany"))
            {
                if (randChoice < 20)
                {
                    skinCol = new Color(0.32f, 0.20f, 0.12f); // Siyahi
                    hairCol = new Color(0.08f, 0.08f, 0.08f);
                }
                else if (randChoice < 30)
                {
                    skinCol = new Color(0.74f, 0.55f, 0.40f); // Melez / Esmer
                    hairCol = new Color(0.15f, 0.15f, 0.15f);
                }
                else
                {
                    skinCol = new Color(0.96f, 0.85f, 0.76f); // Beyaz / Açık
                    int hairRand = UnityEngine.Random.Range(0, 100);
                    if (hairRand < 40) hairCol = new Color(0.90f, 0.75f, 0.35f); // Sarışın
                    else if (hairRand < 75) hairCol = new Color(0.40f, 0.25f, 0.15f); // Kumral / Kahve
                    else if (hairRand < 92) hairCol = new Color(0.08f, 0.08f, 0.08f); // Esmer (Siyah)
                    else hairCol = new Color(0.65f, 0.28f, 0.12f); // Kızıl
                }
            }
            else if (nat.Contains("fransa") || nat.Contains("france"))
            {
                if (randChoice < 35)
                {
                    skinCol = new Color(0.28f, 0.18f, 0.10f); // Siyahi
                    hairCol = new Color(0.08f, 0.08f, 0.08f);
                }
                else if (randChoice < 50)
                {
                    skinCol = new Color(0.74f, 0.55f, 0.40f); // Esmer / Arap / Melez
                    hairCol = new Color(0.08f, 0.08f, 0.08f);
                }
                else
                {
                    skinCol = new Color(0.96f, 0.85f, 0.76f); // Açık ten
                    int hairRand = UnityEngine.Random.Range(0, 100);
                    if (hairRand < 20) hairCol = new Color(0.90f, 0.75f, 0.35f); // Sarışın
                    else if (hairRand < 65) hairCol = new Color(0.40f, 0.25f, 0.15f); // Kahve
                    else hairCol = new Color(0.08f, 0.08f, 0.08f); // Siyah
                }
            }
            else if (nat.Contains("ispanya") || nat.Contains("spain") || 
                     nat.Contains("italya") || nat.Contains("italy") || 
                     nat.Contains("portekiz") || nat.Contains("portugal") ||
                     nat.Contains("arjantin") || nat.Contains("argentina"))
            {
                skinCol = (randChoice < 85) ? new Color(0.91f, 0.74f, 0.59f) : new Color(0.96f, 0.85f, 0.76f); // Zeytin / Açık ten
                int hairRand = UnityEngine.Random.Range(0, 100);
                if (hairRand < 65) hairCol = new Color(0.08f, 0.08f, 0.08f); // Siyah
                else if (hairRand < 95) hairCol = new Color(0.35f, 0.22f, 0.12f); // Kahve
                else hairCol = new Color(0.88f, 0.72f, 0.32f); // Sarı
            }
            else if (nat.Contains("rusya") || nat.Contains("russia") || 
                     nat.Contains("ukrayna") || nat.Contains("ukraine"))
            {
                skinCol = new Color(0.97f, 0.87f, 0.79f); // Çok açık Slav ten
                int hairRand = UnityEngine.Random.Range(0, 100);
                if (hairRand < 35) hairCol = new Color(0.90f, 0.75f, 0.35f); // Sarışın
                else if (hairRand < 75) hairCol = new Color(0.40f, 0.25f, 0.15f); // Kumral / Açık Kahve
                else hairCol = new Color(0.08f, 0.08f, 0.08f); // Siyah
            }
            else // Afrika ülkeleri veya diğer
            {
                skinCol = new Color(0.20f, 0.12f, 0.06f); // Koyu siyahi
                hairCol = new Color(0.08f, 0.08f, 0.08f);
            }

            // Arka plan ve Jersey renkleri (Benzersizliği korumak için seed'e dayalı)
            Color bgCol = Color.HSVToRGB(UnityEngine.Random.value, 0.45f, 0.3f);
            Color jerseyCol = Color.HSVToRGB(UnityEngine.Random.value, 0.6f, 0.6f);

            int hairStyle = UnityEngine.Random.Range(0, 4);

            // Render loop
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // 512x512'lik dokuyu, orijinal 256x256 tabanlı çizim koordinatlarına ölçekliyoruz
                    Vector2 pt = new Vector2(x * 0.5f, y * 0.5f);

                    // 1. Background Badge Circle
                    float bgAlpha = DrawCircle(pt, new Vector2(128f, 128f), 110f);
                    if (bgAlpha <= 0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    // 2. Base layer is background color
                    Color finalPixel = bgCol;

                    // 3. Draw Jersey / Shoulders
                    float jerseyAlpha = DrawEllipse(pt, new Vector2(128f, -10f), new Vector2(90f, 80f));
                    if (jerseyAlpha > 0f)
                    {
                        finalPixel = Color.Lerp(finalPixel, jerseyCol, jerseyAlpha);
                    }

                    // 4. Draw Neck
                    float neckAlpha = DrawBox(pt, new Vector2(112f, 40f), new Vector2(144f, 95f));
                    if (neckAlpha > 0f)
                    {
                        finalPixel = Color.Lerp(finalPixel, skinCol * 0.85f, neckAlpha); // Shaded skin
                    }

                    // 5. Draw Ears
                    float leftEar = DrawCircle(pt, new Vector2(76f, 140f), 12f);
                    if (leftEar > 0f)
                    {
                        finalPixel = Color.Lerp(finalPixel, skinCol, leftEar);
                    }
                    float rightEar = DrawCircle(pt, new Vector2(180f, 140f), 12f);
                    if (rightEar > 0f)
                    {
                        finalPixel = Color.Lerp(finalPixel, skinCol, rightEar);
                    }

                    // 6. Draw Face
                    float faceAlpha = DrawEllipse(pt, new Vector2(128f, 140f), new Vector2(48f, 56f));
                    if (faceAlpha > 0f)
                    {
                        finalPixel = Color.Lerp(finalPixel, skinCol, faceAlpha);
                    }

                    // 7. Draw Hair (Forehead hair/bangs are drawn before facial features so they do not cover eyes/mouth)
                    float hairAlpha = 0f;
                    if (hairStyle == 0) // Short clean crop
                    {
                        hairAlpha = DrawEllipse(pt, new Vector2(128f, 185f), new Vector2(48f, 24f));
                        hairAlpha = Mathf.Max(hairAlpha, DrawCircle(pt, new Vector2(128f, 182f), 30f));
                    }
                    else if (hairStyle == 1) // Curly Afro
                    {
                        float c1 = DrawCircle(pt, new Vector2(128f, 195f), 20f);
                        float c2 = DrawCircle(pt, new Vector2(105f, 185f), 18f);
                        float c3 = DrawCircle(pt, new Vector2(151f, 185f), 18f);
                        float c4 = DrawCircle(pt, new Vector2(90f, 170f), 15f);
                        float c5 = DrawCircle(pt, new Vector2(166f, 170f), 15f);
                        hairAlpha = Mathf.Max(Mathf.Max(Mathf.Max(Mathf.Max(c1, c2), c3), c4), c5);
                    }
                    else if (hairStyle == 2) // Spiky Hair
                    {
                        float cap = DrawEllipse(pt, new Vector2(128f, 180f), new Vector2(48f, 22f));
                        float s1 = DrawCircle(pt, new Vector2(110f, 192f), 12f);
                        float s2 = DrawCircle(pt, new Vector2(128f, 198f), 14f);
                        float s3 = DrawCircle(pt, new Vector2(146f, 192f), 12f);
                        hairAlpha = Mathf.Max(Mathf.Max(Mathf.Max(cap, s1), s2), s3);
                    }
                    else // Long hair
                    {
                        float cap = DrawEllipse(pt, new Vector2(128f, 185f), new Vector2(48f, 24f));
                        float leftLock = DrawBox(pt, new Vector2(74f, 80f), new Vector2(86f, 155f));
                        float rightLock = DrawBox(pt, new Vector2(170f, 80f), new Vector2(182f, 155f));
                        hairAlpha = Mathf.Max(Mathf.Max(cap, leftLock), rightLock);
                    }

                    // Mask hair to the background badge bounds
                    hairAlpha = Mathf.Min(hairAlpha, bgAlpha);
                    if (hairAlpha > 0f)
                    {
                        finalPixel = Color.Lerp(finalPixel, hairCol, hairAlpha);
                    }

                    // 8. Draw Eyes (Whites & Pupils) - Drawn on top of hair
                    // Left Eye
                    float leWhite = DrawCircle(pt, new Vector2(110f, 146f), 8f);
                    if (leWhite > 0f) finalPixel = Color.Lerp(finalPixel, Color.white, leWhite);
                    float lePupil = DrawCircle(pt, new Vector2(110f, 146f), 4f);
                    if (lePupil > 0f) finalPixel = Color.Lerp(finalPixel, Color.black, lePupil);
                    float leGlint = DrawCircle(pt, new Vector2(112f, 148f), 2f);
                    if (leGlint > 0f) finalPixel = Color.Lerp(finalPixel, Color.white, leGlint);

                    // Right Eye
                    float reWhite = DrawCircle(pt, new Vector2(146f, 146f), 8f);
                    if (reWhite > 0f) finalPixel = Color.Lerp(finalPixel, Color.white, reWhite);
                    float rePupil = DrawCircle(pt, new Vector2(146f, 146f), 4f);
                    if (rePupil > 0f) finalPixel = Color.Lerp(finalPixel, Color.black, rePupil);
                    float reGlint = DrawCircle(pt, new Vector2(148f, 148f), 2f);
                    if (reGlint > 0f) finalPixel = Color.Lerp(finalPixel, Color.white, reGlint);

                    // 9. Draw Eyebrows
                    float leftEyebrow = DrawBox(pt, new Vector2(100f, 154f), new Vector2(120f, 157f));
                    if (leftEyebrow > 0f) finalPixel = Color.Lerp(finalPixel, hairCol * 0.5f, leftEyebrow);
                    float rightEyebrow = DrawBox(pt, new Vector2(136f, 154f), new Vector2(156f, 157f));
                    if (rightEyebrow > 0f) finalPixel = Color.Lerp(finalPixel, hairCol * 0.5f, rightEyebrow);

                    // 9.5. Draw Nose (Nose bridge & nose tip shadow depth!)
                    float noseBridge = DrawBox(pt, new Vector2(126f, 133f), new Vector2(130f, 141f));
                    if (noseBridge > 0f) finalPixel = Color.Lerp(finalPixel, skinCol * 0.75f, noseBridge);
                    float noseTip = DrawCircle(pt, new Vector2(128f, 133f), 4f);
                    if (noseTip > 0f) finalPixel = Color.Lerp(finalPixel, skinCol * 0.8f, noseTip);

                    // 10. Draw Mouth (smiling line - scaled using pt.y instead of unscaled y!)
                    float distToMouthCenter = Vector2.Distance(pt, new Vector2(128f, 126f));
                    if (distToMouthCenter >= 10f && distToMouthCenter <= 13f && pt.y < 122f)
                    {
                        float mouthAlpha = Mathf.Clamp01((1.5f - Mathf.Abs(distToMouthCenter - 11.5f)));
                        finalPixel = Color.Lerp(finalPixel, new Color(0.8f, 0.3f, 0.3f), mouthAlpha);
                    }

                    // 11. Draw facial hair (beards) for older players (40% chance for age >= 22)
                    if (p.Age >= 22 && (Mathf.Abs(seed) % 10 < 4))
                    {
                        float beardAlpha = DrawEllipse(pt, new Vector2(128f, 110f), new Vector2(36f, 32f));
                        beardAlpha = Mathf.Max(beardAlpha, DrawBox(pt, new Vector2(92f, 110f), new Vector2(164f, 138f)));
                        beardAlpha = Mathf.Min(beardAlpha, faceAlpha); // Mask beard to remain inside face shape bounds
                        if (beardAlpha > 0f)
                        {
                            finalPixel = Color.Lerp(finalPixel, hairCol * 0.6f, beardAlpha * 0.75f);
                        }
                    }

                    // Apply final pixel with background alpha mask
                    finalPixel.a = bgAlpha;
                    tex.SetPixel(x, y, finalPixel);
                }
            }

            tex.Apply();
            UnityEngine.Random.state = oldState;

            Sprite s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            minifaceCache[p.Id] = s;
            return s;
        }

        private Dictionary<string, Sprite> flagCache = new Dictionary<string, Sprite>();

        public Sprite GetFlagSprite(string nationality)
        {
            if (string.IsNullOrEmpty(nationality)) nationality = "Unknown";
            string key = nationality.ToLower().Trim();
            if (flagCache.TryGetValue(key, out Sprite sp))
            {
                return sp;
            }

            int w = 64;
            int h = 44;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color red = new Color(231f / 255f, 76f / 255f, 60f / 255f, 1f); // Vibrant red
            Color white = Color.white;
            Color blue = new Color(41f / 255f, 128f / 255f, 185f / 255f, 1f); // Vibrant blue
            Color yellow = new Color(241f / 255f, 196f / 255f, 15f / 255f, 1f); // Vibrant yellow
            Color green = new Color(46f / 255f, 204f / 255f, 113f / 255f, 1f); // Vibrant green
            Color black = new Color(44f / 255f, 62f / 255f, 80f / 255f, 1f); // Solid dark grey/black
            Color gold = new Color(243f / 255f, 156f / 255f, 18f / 255f, 1f); // Gold

            // Default flag (grey border and dark background)
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    tex.SetPixel(x, y, new Color(0.2f, 0.25f, 0.3f));
                }
            }

            if (key.Contains("turkey") || key.Contains("türkiye"))
            {
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        tex.SetPixel(x, y, red);

                Vector2 crescentCenter = new Vector2(26f, 22f);
                Vector2 innerCenter = new Vector2(29f, 22f);
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        Vector2 pt = new Vector2(x, y);
                        if (Vector2.Distance(pt, crescentCenter) < 10f && Vector2.Distance(pt, innerCenter) >= 8f)
                        {
                            tex.SetPixel(x, y, white);
                        }
                    }
                }
                for (int dy = -2; dy <= 2; dy++)
                    tex.SetPixel(40, 22 + dy, white);
                for (int dx = -2; dx <= 2; dx++)
                    tex.SetPixel(40 + dx, 22, white);
            }
            else if (key.Contains("england") || key.Contains("ingiltere"))
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (Mathf.Abs(y - 22) < 4 || Mathf.Abs(x - 32) < 4)
                            tex.SetPixel(x, y, red);
                        else
                            tex.SetPixel(x, y, white);
                    }
                }
            }
            else if (key.Contains("spain") || key.Contains("ispanya"))
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (y < 11 || y >= 33)
                            tex.SetPixel(x, y, red);
                        else
                            tex.SetPixel(x, y, yellow);
                    }
                }
            }
            else if (key.Contains("france") || key.Contains("fransa"))
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (x < 21) tex.SetPixel(x, y, blue);
                        else if (x < 42) tex.SetPixel(x, y, white);
                        else tex.SetPixel(x, y, red);
                    }
                }
            }
            else if (key.Contains("germany") || key.Contains("almanya"))
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (y < 15) tex.SetPixel(x, y, gold);
                        else if (y < 29) tex.SetPixel(x, y, red);
                        else tex.SetPixel(x, y, black);
                    }
                }
            }
            else if (key.Contains("italy") || key.Contains("italya"))
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (x < 21) tex.SetPixel(x, y, green);
                        else if (x < 42) tex.SetPixel(x, y, white);
                        else tex.SetPixel(x, y, red);
                    }
                }
            }
            else if (key.Contains("portugal") || key.Contains("portekiz"))
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (x < 26) tex.SetPixel(x, y, green);
                        else tex.SetPixel(x, y, red);
                    }
                }
                for (int dy = -3; dy <= 3; dy++)
                    for (int dx = -3; dx <= 3; dx++)
                        if (dx * dx + dy * dy < 10)
                            tex.SetPixel(26 + dx, 22 + dy, yellow);
            }
            else if (key.Contains("netherlands") || key.Contains("hollanda"))
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (y < 15) tex.SetPixel(x, y, blue);
                        else if (y < 29) tex.SetPixel(x, y, white);
                        else tex.SetPixel(x, y, red);
                    }
                }
            }
            else if (key.Contains("russia") || key.Contains("rusya"))
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (y < 15) tex.SetPixel(x, y, red);
                        else if (y < 29) tex.SetPixel(x, y, blue);
                        else tex.SetPixel(x, y, white);
                    }
                }
            }
            else if (key.Contains("belgium") || key.Contains("belçika"))
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (x < 21) tex.SetPixel(x, y, black);
                        else if (x < 42) tex.SetPixel(x, y, yellow);
                        else tex.SetPixel(x, y, red);
                    }
                }
            }

            tex.Apply();
            Sprite s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
            flagCache[key] = s;
            return s;
        }

        private float DrawCircle(Vector2 p, Vector2 center, float radius)
        {
            float dist = Vector2.Distance(p, center);
            return Mathf.Clamp01((radius - dist) / 1.5f);
        }

        private float DrawEllipse(Vector2 p, Vector2 center, Vector2 rad)
        {
            float dx = (p.x - center.x) / rad.x;
            float dy = (p.y - center.y) / rad.y;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            float avgRad = (rad.x + rad.y) * 0.5f;
            return Mathf.Clamp01((1f - dist) * avgRad / 1.5f);
        }

        private float DrawBox(Vector2 p, Vector2 min, Vector2 max)
        {
            float dx1 = p.x - min.x;
            float dx2 = max.x - p.x;
            float dy1 = p.y - min.y;
            float dy2 = max.y - p.y;
            float minDist = Mathf.Min(Mathf.Min(dx1, dx2), Mathf.Min(dy1, dy2));
            return Mathf.Clamp01(minDist / 1.5f);
        }

        public void ShowPlayerDetails(Player p, bool allowSigning = false)
        {
            // 1. Create dark fullscreen modal block panel
            GameObject modalObj = new GameObject("PlayerDetailsModal");
            modalObj.transform.SetParent(mainCanvas.transform, false);
            
            RectTransform modalRt = modalObj.AddComponent<RectTransform>();
            modalRt.anchorMin = Vector2.zero;
            modalRt.anchorMax = Vector2.one;
            modalRt.offsetMin = Vector2.zero;
            modalRt.offsetMax = Vector2.zero;

            Image modalImg = modalObj.AddComponent<Image>();
            modalImg.color = new Color(0.05f, 0.08f, 0.12f, 1.0f);

            modalObj.AddComponent<CanvasGroup>();
            Button modalBtn = modalObj.AddComponent<Button>();
            
            // 2. The Card Panel (Wider for Portrait/Mobile Aspect Ratio)
            GameObject cardObj = CreatePanelHelper(modalObj.transform, "DetailCard", new Color(0.12f, 0.15f, 0.20f, 1f));
            SetRectTransform(cardObj, new Vector2(0.04f, 0.03f), new Vector2(0.96f, 0.97f), Vector2.zero, Vector2.zero);
            Image cardImg = cardObj.GetComponent<Image>();
            if (cardImg != null && roundedButtonSprite != null)
            {
                cardImg.sprite = roundedButtonSprite;
                cardImg.type = Image.Type.Sliced;
            }

            Outline cardBorder = cardObj.AddComponent<Outline>();
            cardBorder.effectColor = new Color(255f/255f, 255f/255f, 255f/255f, 0.1f);
            cardBorder.effectDistance = new Vector2(2f, 2f);

            cardObj.AddComponent<Button>(); 
            modalBtn.onClick.AddListener(() => {
                Destroy(modalObj);
                if (activeSubpanel != null) activeSubpanel.Refresh();
            });

            // 3. Top-Right Close Button
            Text closeLabel = CreateButtonHelper(cardObj.transform, "BtnClose", "X", colorRed, Color.white, () => {
                Destroy(modalObj);
                if (activeSubpanel != null) activeSubpanel.Refresh();
            });
            SetRectTransform(closeLabel.transform.parent, new Vector2(0.88f, 0.91f), new Vector2(0.97f, 0.98f), Vector2.zero, Vector2.zero);
            closeLabel.fontSize = 40;
            closeLabel.fontStyle = FontStyle.Bold;

            // 4. Header: Miniface, Name, Position, Nationality, Age
            GameObject faceObj = new GameObject("LargeMiniface");
            faceObj.transform.SetParent(cardObj.transform, false);
            SetRectTransform(faceObj, new Vector2(0.06f, 0.78f), new Vector2(0.28f, 0.94f), Vector2.zero, Vector2.zero);
            Image faceImg = faceObj.AddComponent<Image>();
            faceImg.sprite = GetMiniface(p);

            GameObject headerTextContainer = new GameObject("HeaderTextContainer");
            headerTextContainer.transform.SetParent(cardObj.transform, false);
            SetRectTransform(headerTextContainer, new Vector2(0.29f, 0.78f), new Vector2(0.95f, 0.94f), Vector2.zero, Vector2.zero);

            Text nameTxt = CreateText(headerTextContainer.transform, "Name", p.Name, 56, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(nameTxt, new Vector2(0f, 0.55f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            nameTxt.fontStyle = FontStyle.Bold;

            // Horizontal layout for flag and country metadata to prevent stretching/slipping!
            GameObject metaRow = new GameObject("MetaRow");
            metaRow.transform.SetParent(headerTextContainer.transform, false);
            SetRectTransform(metaRow, new Vector2(0f, 0.05f), new Vector2(1f, 0.45f), Vector2.zero, Vector2.zero);

            HorizontalLayoutGroup hlg = metaRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 18f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            GameObject flagObj = new GameObject("MetaFlag");
            flagObj.transform.SetParent(metaRow.transform, false);
            Image flagImg = flagObj.AddComponent<Image>();
            flagImg.sprite = GetFlagSprite(p.Nationality);
            flagImg.preserveAspect = true;
            LayoutElement flagLe = flagObj.AddComponent<LayoutElement>();
            flagLe.preferredWidth = 48f;
            flagLe.preferredHeight = 33f;

            string posText = p.Position == PlayerPosition.GK ? "Kaleci (GK)" :
                             p.Position == PlayerPosition.DEF ? "Defans (DEF)" :
                             p.Position == PlayerPosition.MID ? "Orta Saha (MID)" : "Forvet (FWD)";
                             
            string metaStr = $"{p.Nationality}  |  {posText}  |  {p.Age}\u00A0Yaş";
            Text metaTxt = CreateText(metaRow.transform, "Meta", metaStr, 38, new Color(0.7f, 0.75f, 0.8f), TextAnchor.MiddleLeft);
            metaTxt.fontStyle = FontStyle.Bold;
            metaTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            metaTxt.verticalOverflow = VerticalWrapMode.Overflow;

            // Separator Line
            GameObject sep = CreatePanelHelper(cardObj.transform, "Separator", new Color(1f, 1f, 1f, 0.1f));
            SetRectTransform(sep, new Vector2(0.05f, 0.75f), new Vector2(0.95f, 0.76f), Vector2.zero, Vector2.zero);

            // GEN & POT Plain Text Row
            Text ovrTxt = CreateText(cardObj.transform, "OvrText", $"GEN: {p.OVR}", 72, new Color(46f/255f, 204f/255f, 113f/255f), TextAnchor.MiddleCenter);
            SetRectTransform(ovrTxt, new Vector2(0.08f, 0.67f), new Vector2(0.48f, 0.73f), Vector2.zero, Vector2.zero);
            ovrTxt.fontStyle = FontStyle.Bold;

            Text potTxt = CreateText(cardObj.transform, "PotText", $"POT: {p.POT}", 72, new Color(52f/255f, 152f/255f, 219f/255f), TextAnchor.MiddleCenter);
            SetRectTransform(potTxt, new Vector2(0.52f, 0.67f), new Vector2(0.92f, 0.73f), Vector2.zero, Vector2.zero);
            potTxt.fontStyle = FontStyle.Bold;
 
            string actualClub = p.IsOnLoan ? p.ParentClubName : (p.CurrentContract != null ? p.CurrentContract.ClubName : "Serbest");
            string wage = p.CurrentContract != null ? $"€{p.CurrentContract.WeeklyWage:N0} / hafta" : "Yok";
            string rep = p.IsAgencyClient ? "<color=#2ECC71><b>Sizin Müşteriniz</b></color>" : 
                            (p.CurrentContract != null ? "<color=#E74C3C><b>Rakip Temsilci</b></color>" : "<b>Boşta (Temsilcisi Yok)</b>");
 
            string happyStr = "";
            string happyColor = "#E74C3C"; // red
            if (p.Happiness >= 80f) { happyStr = "Çok Mutlu (+Perf)"; happyColor = "#2ECC71"; }
            else if (p.Happiness >= 50f) { happyStr = "Mutlu / Dengeli"; happyColor = "#2ECC71"; }
            else if (p.Happiness >= 35f) { happyStr = "Huzursuz / Endişeli"; happyColor = "#F39C12"; }
            else { happyStr = "Krizde / Ayrılmak İstiyor"; happyColor = "#E74C3C"; }

            string val = $"€{p.MarketValue:N0}";
            string sponsorText = p.ActiveSponsor != null 
                ? $"<color=#F1C40F><b>{p.ActiveSponsor.BrandName} ({p.ActiveSponsor.DurationYears} Yıl - €{p.ActiveSponsor.WeeklyIncome:N0}/hafta)</b></color>" 
                : "<b>Sözleşme Yok</b>";

                    // Vertical list of Details Labels
            if (p.IsAgencyClient)
            {
                if (p.IsOnLoan)
                {
                    // 8 labels
                    CreateDetailLabel(cardObj.transform, "LblClub", $"Asıl Kulüp: <b>{actualClub}</b>", new Vector2(0.08f, 0.620f), new Vector2(0.92f, 0.665f));
                    CreateDetailLabel(cardObj.transform, "LblLoanStatus", $"Kiralık: <color=#F1C40F><b>{p.CurrentContract.ClubName} kulübünde kiralık</b></color>", new Vector2(0.08f, 0.572f), new Vector2(0.92f, 0.617f));
                    
                    string roleStr = p.CurrentContract != null ? $"Kadro Rolü: <color=#F1C40F><b>{p.SquadRole}</b></color>" : "Kadro Rolü: <b>Yok</b>";
                    CreateDetailLabel(cardObj.transform, "LblRole", roleStr, new Vector2(0.08f, 0.524f), new Vector2(0.92f, 0.569f));
                    CreateDetailLabel(cardObj.transform, "LblWage", $"Maaş: <b>{wage}</b>", new Vector2(0.08f, 0.476f), new Vector2(0.92f, 0.521f));
                    CreateDetailLabel(cardObj.transform, "LblHappiness", $"Mutluluk: <color={happyColor}><b>%{p.Happiness:0} ({happyStr})</b></color>", new Vector2(0.08f, 0.428f), new Vector2(0.92f, 0.473f));
                    
                    int yrs = p.AgencyContractRemainingWeeks / 52;
                    int wks = p.AgencyContractRemainingWeeks % 52;
                    string durationStr = yrs > 0 ? $"{yrs} Yıl {wks} Hafta" : $"{wks} Hafta";
                    CreateDetailLabel(cardObj.transform, "LblAgentDuration", $"Ajans Sözleşmesi: <color=#58D68D><b>{durationStr}</b></color>", new Vector2(0.08f, 0.380f), new Vector2(0.92f, 0.425f));
                    CreateDetailLabel(cardObj.transform, "LblActiveSponsor", $"Sponsorluk: {sponsorText}", new Vector2(0.08f, 0.332f), new Vector2(0.92f, 0.377f));
                    CreateDetailLabel(cardObj.transform, "LblValue", $"Piyasa Değeri: <color=#F1C40F><b>{val}</b></color>", new Vector2(0.08f, 0.284f), new Vector2(0.92f, 0.329f));
                }
                else
                {
                    // 7 labels
                    CreateDetailLabel(cardObj.transform, "LblClub", $"Kulüp: <b>{actualClub}</b>", new Vector2(0.08f, 0.610f), new Vector2(0.92f, 0.660f));
                    
                    string roleStr = p.CurrentContract != null ? $"Kadro Rolü: <color=#F1C40F><b>{p.SquadRole}</b></color>" : "Kadro Rolü: <b>Yok</b>";
                    CreateDetailLabel(cardObj.transform, "LblRole", roleStr, new Vector2(0.08f, 0.555f), new Vector2(0.92f, 0.605f));
                    CreateDetailLabel(cardObj.transform, "LblWage", $"Maaş: <b>{wage}</b>", new Vector2(0.08f, 0.500f), new Vector2(0.92f, 0.550f));
                    CreateDetailLabel(cardObj.transform, "LblHappiness", $"Mutluluk: <color={happyColor}><b>%{p.Happiness:0} ({happyStr})</b></color>", new Vector2(0.08f, 0.445f), new Vector2(0.92f, 0.495f));
                    
                    int yrs = p.AgencyContractRemainingWeeks / 52;
                    int wks = p.AgencyContractRemainingWeeks % 52;
                    string durationStr = yrs > 0 ? $"{yrs} Yıl {wks} Hafta" : $"{wks} Hafta";
                    CreateDetailLabel(cardObj.transform, "LblAgentDuration", $"Ajans Sözleşmesi: <color=#58D68D><b>{durationStr}</b></color>", new Vector2(0.08f, 0.390f), new Vector2(0.92f, 0.440f));
                    CreateDetailLabel(cardObj.transform, "LblActiveSponsor", $"Sponsorluk: {sponsorText}", new Vector2(0.08f, 0.335f), new Vector2(0.92f, 0.385f));
                    CreateDetailLabel(cardObj.transform, "LblValue", $"Piyasa Değeri: <color=#F1C40F><b>{val}</b></color>", new Vector2(0.08f, 0.280f), new Vector2(0.92f, 0.330f));
                }
            }
            else
            {
                if (p.IsOnLoan)
                {
                    // 7 labels
                    CreateDetailLabel(cardObj.transform, "LblClub", $"Asıl Kulüp: <b>{actualClub}</b>", new Vector2(0.08f, 0.610f), new Vector2(0.92f, 0.660f));
                    CreateDetailLabel(cardObj.transform, "LblLoanStatus", $"Kiralık: <color=#F1C40F><b>{p.CurrentContract.ClubName} kulübünde kiralık</b></color>", new Vector2(0.08f, 0.555f), new Vector2(0.92f, 0.605f));
                    
                    string roleStr = p.CurrentContract != null ? $"Kadro Rolü: <color=#F1C40F><b>{p.SquadRole}</b></color>" : "Kadro Rolü: <b>Yok</b>";
                    CreateDetailLabel(cardObj.transform, "LblRole", roleStr, new Vector2(0.08f, 0.500f), new Vector2(0.92f, 0.550f));
                    CreateDetailLabel(cardObj.transform, "LblWage", $"Maaş: <b>{wage}</b>", new Vector2(0.08f, 0.445f), new Vector2(0.92f, 0.495f));
                    CreateDetailLabel(cardObj.transform, "LblHappiness", $"Mutluluk: <color={happyColor}><b>%{p.Happiness:0} ({happyStr})</b></color>", new Vector2(0.08f, 0.390f), new Vector2(0.92f, 0.440f));
                    CreateDetailLabel(cardObj.transform, "LblActiveSponsor", $"Sponsorluk: {sponsorText}", new Vector2(0.08f, 0.335f), new Vector2(0.92f, 0.385f));
                    CreateDetailLabel(cardObj.transform, "LblValue", $"Piyasa Değeri: <color=#F1C40F><b>{val}</b></color>", new Vector2(0.08f, 0.280f), new Vector2(0.92f, 0.330f));
                }
                else
                {
                    // 6 labels
                    CreateDetailLabel(cardObj.transform, "LblClub", $"Kulüp: <b>{actualClub}</b>", new Vector2(0.08f, 0.605f), new Vector2(0.92f, 0.655f));
                    
                    string roleStr = p.CurrentContract != null ? $"Kadro Rolü: <color=#F1C40F><b>{p.SquadRole}</b></color>" : "Kadro Rolü: <b>Yok</b>";
                    CreateDetailLabel(cardObj.transform, "LblRole", roleStr, new Vector2(0.08f, 0.540f), new Vector2(0.92f, 0.590f));
                    CreateDetailLabel(cardObj.transform, "LblWage", $"Maaş: <b>{wage}</b>", new Vector2(0.08f, 0.475f), new Vector2(0.92f, 0.525f));
                    CreateDetailLabel(cardObj.transform, "LblHappiness", $"Mutluluk: <color={happyColor}><b>%{p.Happiness:0} ({happyStr})</b></color>", new Vector2(0.08f, 0.410f), new Vector2(0.92f, 0.460f));
                    CreateDetailLabel(cardObj.transform, "LblActiveSponsor", $"Sponsorluk: {sponsorText}", new Vector2(0.08f, 0.345f), new Vector2(0.92f, 0.395f));
                    CreateDetailLabel(cardObj.transform, "LblValue", $"Piyasa Değeri: <color=#F1C40F><b>{val}</b></color>", new Vector2(0.08f, 0.280f), new Vector2(0.92f, 0.330f));
                }
            }

            string transferStatusStr = !string.IsNullOrEmpty(p.TransferStatusNote) 
                ? p.TransferStatusNote 
                : (p.IsTransferListed ? "Transfer Listesinde (Ayrılmak İstiyor)" : null);

            if (!string.IsNullOrEmpty(transferStatusStr))
            {
                CreateDetailLabel(cardObj.transform, "LblTransferStatus", $"Transfer Durumu: <color=#F39C12><b>{transferStatusStr}</b></color>", new Vector2(0.08f, 0.232f), new Vector2(0.92f, 0.277f));
            }

            // Traits & Sponsor Area
            CreateDetailBadge(cardObj.transform, "PosTraitBadge", $"✔ {p.PositiveTrait}", new Vector2(0.08f, 0.230f), new Vector2(0.48f, 0.270f), new Color(46f/255f, 204f/255f, 113f/255f, 0.25f), new Color(46f/255f, 204f/255f, 113f/255f), 48, new Color(46f/255f, 204f/255f, 113f/255f, 0.75f));
            CreateDetailBadge(cardObj.transform, "NegTraitBadge", $"✘ {p.NegativeTrait}", new Vector2(0.52f, 0.230f), new Vector2(0.92f, 0.270f), new Color(231f/255f, 76f/255f, 60f/255f, 0.25f), new Color(231f/255f, 76f/255f, 60f/255f), 48, new Color(231f/255f, 76f/255f, 60f/255f, 0.75f));

            if (p.IsAgencyClient)
            {
                bool hasSponsorOffers = p.ActiveSponsor == null && p.PendingSponsorOffers != null && p.PendingSponsorOffers.Count > 0;
                if (hasSponsorOffers)
                {
                    Text btnSponsorOffers = CreateButtonHelper(cardObj.transform, "BtnSponsorOffers", $"SPONSOR TEKLİFLERİ ({p.PendingSponsorOffers.Count})", colorGold, new Color(11f/255f, 12f/255f, 16f/255f, 1f), () => {
                        ShowSponsorOffersList(p, modalObj);
                    });
                    SetRectTransform(btnSponsorOffers.transform.parent, new Vector2(0.08f, 0.155f), new Vector2(0.92f, 0.210f), Vector2.zero, Vector2.zero);
                    btnSponsorOffers.fontSize = 34;
                    btnSponsorOffers.fontStyle = FontStyle.Bold;
                }

                int currentGlobalWeek = SimulationEngine.Instance.CurrentYear * 52 + SimulationEngine.Instance.CurrentWeek;
                int weeksElapsed = currentGlobalWeek - p.LastInteractionGlobalWeek;
                bool canInteract = p.LastInteractionGlobalWeek == -999 || weeksElapsed >= 4;

                // 1. ÖV Button
                Color praiseColor = canInteract ? new Color(52f/255f, 152f/255f, 219f/255f, 1f) : new Color(149f/255f, 165f/255f, 166f/255f, 1f); // Blue / Grey
                string praiseLabel = canInteract ? "ÖV" : $"ÖV ({4 - weeksElapsed} Hf)";
                Text btnPraise = CreateButtonHelper(cardObj.transform, "BtnPraise", praiseLabel, praiseColor, Color.white, () => {
                    p.Happiness = Mathf.Clamp(p.Happiness + UnityEngine.Random.Range(8, 16), 0f, 100f);
                    p.LastInteractionGlobalWeek = currentGlobalWeek;
                    AgencyManager.Instance.LogActivity($"ÖVGÜ: Müşteriniz {p.Name} övgü dolu sözleriniz üzerine motive oldu (Yeni Mutluluk: %{p.Happiness:0}).");
                    Destroy(modalObj);
                    ShowPlayerDetails(p, allowSigning);
                });
                SetRectTransform(btnPraise.transform.parent, new Vector2(0.08f, 0.085f), new Vector2(0.32f, 0.135f), Vector2.zero, Vector2.zero);
                btnPraise.fontSize = 34;
                btnPraise.fontStyle = FontStyle.Bold;
                if (!canInteract)
                {
                    Button b = btnPraise.transform.parent.GetComponent<Button>();
                    if (b != null) b.interactable = false;
                }

                // 2. PRİM Button
                Color bonusColor = canInteract ? new Color(46f/255f, 204f/255f, 113f/255f, 1f) : new Color(149f/255f, 165f/255f, 166f/255f, 1f); // Green / Grey
                string bonusLabel = canInteract ? "PRİM VER (€15K)" : $"PRİM ({4 - weeksElapsed} Hf)";
                Text btnBonus = CreateButtonHelper(cardObj.transform, "BtnBonus", bonusLabel, bonusColor, canInteract ? new Color(11f/255f, 12f/255f, 16f/255f, 1f) : Color.white, () => {
                    if (AgencyManager.Instance.ActiveAgency.Balance < 15000)
                    {
                        AgencyManager.Instance.LogActivity($"Hata: Prim vermek için yetersiz bütçe (Gereken: €15.000).");
                        return;
                    }
                    AgencyManager.Instance.ActiveAgency.Balance -= 15000;
                    p.Happiness = Mathf.Clamp(p.Happiness + UnityEngine.Random.Range(25, 41), 0f, 100f);
                    p.LastInteractionGlobalWeek = currentGlobalWeek;
                    AgencyManager.Instance.LogActivity($"PRİM: Müşteriniz {p.Name} ajansınızdan aldığı €15.000 prim ile çok mutlu oldu (Yeni Mutluluk: %{p.Happiness:0}).");
                    Destroy(modalObj);
                    ShowPlayerDetails(p, allowSigning);
                });
                SetRectTransform(btnBonus.transform.parent, new Vector2(0.36f, 0.085f), new Vector2(0.64f, 0.135f), Vector2.zero, Vector2.zero);
                btnBonus.fontSize = canInteract ? 32 : 34;
                btnBonus.fontStyle = FontStyle.Bold;
                if (!canInteract)
                {
                    Button b = btnBonus.transform.parent.GetComponent<Button>();
                    if (b != null) b.interactable = false;
                }

                // 3. ELEŞTİR Button
                Color warnColor = canInteract ? new Color(230f/255f, 126f/255f, 34f/255f, 1f) : new Color(149f/255f, 165f/255f, 166f/255f, 1f); // Orange / Grey
                string warnLabel = canInteract ? "UYAR" : $"UYAR ({4 - weeksElapsed} Hf)";
                Text btnWarn = CreateButtonHelper(cardObj.transform, "BtnWarn", warnLabel, warnColor, Color.white, () => {
                    p.Happiness = Mathf.Clamp(p.Happiness - UnityEngine.Random.Range(15, 26), 0f, 100f);
                    p.LastInteractionGlobalWeek = currentGlobalWeek;
                    AgencyManager.Instance.LogActivity($"ELEŞTİRİ: Müşteriniz {p.Name} disiplinsiz davranışları nedeniyle uyarıldı, morali bozuldu (Yeni Mutluluk: %{p.Happiness:0}).");
                    Destroy(modalObj);
                    ShowPlayerDetails(p, allowSigning);
                });
                SetRectTransform(btnWarn.transform.parent, new Vector2(0.68f, 0.085f), new Vector2(0.92f, 0.135f), Vector2.zero, Vector2.zero);
                btnWarn.fontSize = 34;
                btnWarn.fontStyle = FontStyle.Bold;
                if (!canInteract)
                {
                    Button b = btnWarn.transform.parent.GetComponent<Button>();
                    if (b != null) b.interactable = false;
                }
            }

            // 6. Bottom Action Button Area (Wider to prevent squishing)
            GameObject actionArea = new GameObject("ActionArea");
            actionArea.transform.SetParent(cardObj.transform, false);
            SetRectTransform(actionArea, new Vector2(0.08f, 0.02f), new Vector2(0.92f, 0.07f), Vector2.zero, Vector2.zero);

            if (p.IsAgencyClient)
            {
                if (p.CurrentContract == null)
                {
                    // Suggest to club split buttons
                    Text releaseLabel = CreateButtonHelper(actionArea.transform, "BtnRelease", "FESHET (BIRAK)", new Color(231f / 255f, 76f / 255f, 60f / 255f, 1f), Color.white, () => {
                        AgencyManager.Instance.TerminateClient(p);
                        Destroy(modalObj);
                        if (activeSubpanel != null) activeSubpanel.Refresh();
                    });
                    SetRectTransform(releaseLabel.transform.parent, new Vector2(0f, 0f), new Vector2(0.35f, 1f), Vector2.zero, Vector2.zero);

                    Text suggestLabel = CreateButtonHelper(actionArea.transform, "BtnSuggest", "KULÜBE ÖNER", new Color(241f / 255f, 196f / 255f, 15f / 255f, 1f), new Color(11f / 255f, 12f / 255f, 16f / 255f, 1f), () => {
                        ShowClubSuggestions(p, modalObj);
                    });
                    SetRectTransform(suggestLabel.transform.parent, new Vector2(0.40f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
                }
                else
                {
                    bool isWindowOpen = SimulationEngine.Instance != null && SimulationEngine.Instance.IsTransferWindowOpen();
                    bool canRenew = p.AgencyContractRemainingWeeks <= 52;

                    if (isWindowOpen)
                    {
                        if (canRenew)
                        {
                            Text releaseLabel = CreateButtonHelper(actionArea.transform, "BtnRelease", "FESHET", new Color(231f / 255f, 76f / 255f, 60f / 255f, 1f), Color.white, () => {
                                AgencyManager.Instance.TerminateClient(p);
                                Destroy(modalObj);
                                if (activeSubpanel != null) activeSubpanel.Refresh();
                            });
                            SetRectTransform(releaseLabel.transform.parent, new Vector2(0f, 0f), new Vector2(0.30f, 1f), Vector2.zero, Vector2.zero);

                            Text renewLabel = CreateButtonHelper(actionArea.transform, "BtnRenew", "YENİLE", new Color(46f / 255f, 204f / 255f, 113f / 255f, 1f), new Color(11f / 255f, 12f / 255f, 16f / 255f, 1f), () => {
                                Destroy(modalObj);
                                ShowSignNegotiation(p, () => {
                                    if (activeSubpanel != null) activeSubpanel.Refresh();
                                });
                            });
                            SetRectTransform(renewLabel.transform.parent, new Vector2(0.33f, 0f), new Vector2(0.63f, 1f), Vector2.zero, Vector2.zero);

                            string loanText = p.IsSuggestedForLoan ? "KİRALIK ÖNERİLDİ" : "KİRALIK ÖNER";
                            Color loanCol = p.IsSuggestedForLoan ? new Color(127f/255f, 140f/255f, 141f/255f, 1f) : new Color(230f/255f, 126f/255f, 34f/255f, 1f);
                            Text loanLabel = CreateButtonHelper(actionArea.transform, "BtnSuggestLoan", loanText, loanCol, Color.white, () => {
                                if (!p.IsSuggestedForLoan)
                                {
                                    p.IsSuggestedForLoan = true;
                                    AgencyManager.Instance.LogActivity($"KİRALIK ÖNERİSİ: {p.Name} transfer piyasasında kiralık kulüplere önerildi. 1 hafta içinde teklifler gelecektir.");
                                    Destroy(modalObj);
                                    ShowPlayerDetails(p, allowSigning);
                                }
                            });
                            SetRectTransform(loanLabel.transform.parent, new Vector2(0.66f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
                        }
                        else
                        {
                            Text releaseLabel = CreateButtonHelper(actionArea.transform, "BtnRelease", "SÖZLEŞMEYİ FESHET (BIRAK)", new Color(231f / 255f, 76f / 255f, 60f / 255f, 1f), Color.white, () => {
                                AgencyManager.Instance.TerminateClient(p);
                                Destroy(modalObj);
                                if (activeSubpanel != null) activeSubpanel.Refresh();
                            });
                            SetRectTransform(releaseLabel.transform.parent, new Vector2(0f, 0f), new Vector2(0.48f, 1f), Vector2.zero, Vector2.zero);

                            string loanText = p.IsSuggestedForLoan ? "KİRALIK ÖNERİLDİ" : "KİRALIK KULÜPLERE ÖNER";
                            Color loanCol = p.IsSuggestedForLoan ? new Color(127f/255f, 140f/255f, 141f/255f, 1f) : new Color(230f/255f, 126f/255f, 34f/255f, 1f);
                            Text loanLabel = CreateButtonHelper(actionArea.transform, "BtnSuggestLoan", loanText, loanCol, Color.white, () => {
                                if (!p.IsSuggestedForLoan)
                                {
                                    p.IsSuggestedForLoan = true;
                                    AgencyManager.Instance.LogActivity($"KİRALIK ÖNERİSİ: {p.Name} transfer piyasasında kiralık kulüplere önerildi. 1 hafta içinde teklifler gelecektir.");
                                    Destroy(modalObj);
                                    ShowPlayerDetails(p, allowSigning);
                                }
                            });
                            SetRectTransform(loanLabel.transform.parent, new Vector2(0.52f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
                        }
                    }
                    else
                    {
                        if (canRenew)
                        {
                            Text releaseLabel = CreateButtonHelper(actionArea.transform, "BtnRelease", "SÖZLEŞMEYİ FESHET (BIRAK)", new Color(231f / 255f, 76f / 255f, 60f / 255f, 1f), Color.white, () => {
                                AgencyManager.Instance.TerminateClient(p);
                                Destroy(modalObj);
                                if (activeSubpanel != null) activeSubpanel.Refresh();
                            });
                            SetRectTransform(releaseLabel.transform.parent, new Vector2(0f, 0f), new Vector2(0.46f, 1f), Vector2.zero, Vector2.zero);

                            Text renewLabel = CreateButtonHelper(actionArea.transform, "BtnRenew", "SÖZLEŞME YENİLE", new Color(46f / 255f, 204f / 255f, 113f / 255f, 1f), new Color(11f / 255f, 12f / 255f, 16f / 255f, 1f), () => {
                                Destroy(modalObj);
                                ShowSignNegotiation(p, () => {
                                    if (activeSubpanel != null) activeSubpanel.Refresh();
                                });
                            });
                            SetRectTransform(renewLabel.transform.parent, new Vector2(0.54f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
                        }
                        else
                        {
                            Text actLabel = CreateButtonHelper(actionArea.transform, "BtnAction", "SÖZLEŞMEYİ FESHET (BIRAK)", new Color(231f / 255f, 76f / 255f, 60f / 255f, 1f), Color.white, () => {
                                AgencyManager.Instance.TerminateClient(p);
                                Destroy(modalObj);
                                if (activeSubpanel != null) activeSubpanel.Refresh();
                            });
                            SetRectTransform(actLabel.transform.parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                        }
                    }
                }
            }
            else
            {
                // Left Button: Favorite Toggle
                string favText = p.IsFavorite ? "★ FAVORİDEN ÇIKAR" : "☆ FAVORİYE EKLE";
                Color favColor = p.IsFavorite ? new Color(231f/255f, 76f/255f, 60f/255f, 1f) : new Color(241f/255f, 196f/255f, 15f/255f, 1f);

                Text favLabel = CreateButtonHelper(actionArea.transform, "BtnFavorite", favText, favColor, Color.white, () => {
                    p.IsFavorite = !p.IsFavorite;
                    Destroy(modalObj);
                    ShowPlayerDetails(p, allowSigning); // Re-open to refresh state!
                });
                SetRectTransform(favLabel.transform.parent, new Vector2(0f, 0f), new Vector2(0.46f, 1f), Vector2.zero, Vector2.zero);

                // Right Button: SÖZLEŞME İMZALA (if allowSigning is true) OR TEMAS KUR (if allowSigning is false)
                if (allowSigning)
                {
                    Text actLabel = CreateButtonHelper(actionArea.transform, "BtnAction", "SÖZLEŞME İMZALA", new Color(46f/255f, 204f/255f, 113f/255f, 1f), new Color(11f/255f, 12f/255f, 16f/255f, 1f), () => {
                        Destroy(modalObj);
                        ShowSignNegotiation(p, () => {
                            if (activeSubpanel != null) activeSubpanel.Refresh();
                        });
                    });
                    SetRectTransform(actLabel.transform.parent, new Vector2(0.54f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
                }
                else
                {
                    string contactText = p.IsContacted ? "TEMAS KURULDU" : "📞 TEMAS KUR";
                    Color contactColor = p.IsContacted ? new Color(0.3f, 0.35f, 0.4f, 1f) : new Color(46f/255f, 204f/255f, 113f/255f, 1f);

                    Text contactLabel = CreateButtonHelper(actionArea.transform, "BtnContact", contactText, contactColor, Color.white, () => {
                        if (!p.IsContacted)
                        {
                            p.IsContacted = true;
                            AgencyManager.Instance.LogActivity($"TEMAS: {p.Name} ile temasa geçildi. Takip listenizde görebilirsiniz.");
                            Destroy(modalObj);
                            ShowPlayerDetails(p, allowSigning);
                        }
                    });
                    SetRectTransform(contactLabel.transform.parent, new Vector2(0.54f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
                }
            }
        }

        private (RectTransform content, GameObject modalObj, GameObject cardObj) CreateScrollableNegotiationCard(Transform parent, Player p, string titleText, System.Action onClose, string descOverride = null)
        {
            // 1. Fullscreen modal background
            GameObject modalObj = new GameObject("NegotiationModal");
            modalObj.transform.SetParent(parent, false);
            
            RectTransform modalRt = modalObj.AddComponent<RectTransform>();
            modalRt.anchorMin = Vector2.zero;
            modalRt.anchorMax = Vector2.one;
            modalRt.offsetMin = Vector2.zero;
            modalRt.offsetMax = Vector2.zero;

            Image modalImg = modalObj.AddComponent<Image>();
            modalImg.color = new Color(0.05f, 0.08f, 0.12f, 1.0f);

            modalObj.AddComponent<CanvasGroup>();
            Button modalBtn = modalObj.AddComponent<Button>();
            modalBtn.onClick.AddListener(() => {
                Destroy(modalObj);
                onClose?.Invoke();
            });

            // 2. Card Panel (Large size)
            GameObject cardObj = CreatePanelHelper(modalObj.transform, "NegoCard", new Color(0.12f, 0.15f, 0.20f, 1f));
            SetRectTransform(cardObj, new Vector2(0.05f, 0.03f), new Vector2(0.95f, 0.97f), Vector2.zero, Vector2.zero);
            Image cardImg = cardObj.GetComponent<Image>();
            if (cardImg != null && roundedButtonSprite != null)
            {
                cardImg.sprite = roundedButtonSprite;
                cardImg.type = Image.Type.Sliced;
            }

            Outline cardBorder = cardObj.AddComponent<Outline>();
            cardBorder.effectColor = new Color(255f/255f, 255f/255f, 255f/255f, 0.1f);
            cardBorder.effectDistance = new Vector2(2f, 2f);

            cardObj.AddComponent<Button>(); // stops click-through

            // Close button at top right
            Text closeLabel = CreateButtonHelper(cardObj.transform, "BtnClose", "X", colorRed, Color.white, () => {
                Destroy(modalObj);
                onClose?.Invoke();
            });
            SetRectTransform(closeLabel.transform.parent, new Vector2(0.88f, 0.92f), new Vector2(0.97f, 0.98f), Vector2.zero, Vector2.zero);
            closeLabel.fontSize = 40;
            closeLabel.fontStyle = FontStyle.Bold;

            // Scroll View Area inside card (Y: 0.02f to 0.90f)
            GameObject scrollObj = new GameObject("NegoScroll");
            scrollObj.transform.SetParent(cardObj.transform, false);
            SetRectTransform(scrollObj, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.90f), Vector2.zero, Vector2.zero);
            ScrollRect scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollObj.transform, false);
            SetRectTransform(viewportObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            viewportObj.AddComponent<Image>().color = Color.clear;
            viewportObj.AddComponent<RectMask2D>();

            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRt = contentObj.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0f, 200f);

            VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 40f; // extra space to prevent overlaps!
            vlg.padding = new RectOffset(30, 30, 30, 30);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRt;
            scrollRect.viewport = viewportObj.GetComponent<RectTransform>();

            // Header Row (Face & Info side-by-side using dynamic HorizontalLayoutGroup)
            GameObject headerContainer = new GameObject("HeaderContainer", typeof(RectTransform));
            headerContainer.transform.SetParent(contentRt, false);

            HorizontalLayoutGroup headerHlg = headerContainer.AddComponent<HorizontalLayoutGroup>();
            headerHlg.spacing = 20;
            headerHlg.childAlignment = TextAnchor.UpperLeft;
            headerHlg.childControlWidth = true;
            headerHlg.childControlHeight = true;
            headerHlg.childForceExpandWidth = false;
            headerHlg.childForceExpandHeight = false;

            ContentSizeFitter headerCsf = headerContainer.AddComponent<ContentSizeFitter>();
            headerCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            headerCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // 1. Miniface (Fixed 200px Width x 240px Height)
            GameObject faceObj = new GameObject("LargeMiniface", typeof(RectTransform));
            faceObj.transform.SetParent(headerContainer.transform, false);
            
            LayoutElement faceLe = faceObj.AddComponent<LayoutElement>();
            faceLe.preferredWidth = 200f;
            faceLe.minWidth = 200f;
            faceLe.preferredHeight = 240f;
            faceLe.minHeight = 240f;

            Image faceImg = faceObj.AddComponent<Image>();
            faceImg.sprite = GetMiniface(p);
            faceImg.preserveAspect = true;

            // 2. Right Text Column (Dynamic Height & Flexible Width)
            GameObject headerTextContainer = new GameObject("HeaderTextContainer", typeof(RectTransform));
            headerTextContainer.transform.SetParent(headerContainer.transform, false);
            
            LayoutElement textLe = headerTextContainer.AddComponent<LayoutElement>();
            textLe.flexibleWidth = 1f;

            VerticalLayoutGroup textVlg = headerTextContainer.AddComponent<VerticalLayoutGroup>();
            textVlg.spacing = 10f;
            textVlg.childAlignment = TextAnchor.UpperLeft;
            textVlg.childControlWidth = true;
            textVlg.childControlHeight = true;
            textVlg.childForceExpandWidth = true;
            textVlg.childForceExpandHeight = false;

            ContentSizeFitter textCsf = headerTextContainer.AddComponent<ContentSizeFitter>();
            textCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            textCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            Text titleTxt = CreateText(headerTextContainer.transform, "NegoTitle", titleText, 44, Color.white, TextAnchor.MiddleLeft);
            titleTxt.fontStyle = FontStyle.Bold;
            var titleScaler = titleTxt.GetComponent<TextScaler>();
            if (titleScaler != null) Destroy(titleScaler);
            titleTxt.fontSize = 44;
            titleTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
            titleTxt.verticalOverflow = VerticalWrapMode.Overflow;

            string posText = p.Position == PlayerPosition.GK ? "Kaleci (GK)" :
                             p.Position == PlayerPosition.DEF ? "Defans (DEF)" :
                             p.Position == PlayerPosition.MID ? "Orta Saha (MID)" : "Forvet (FWD)";
            string descStr = descOverride;
            if (string.IsNullOrEmpty(descStr))
            {
                descStr = $"GEN: {p.OVR} | Yaş: {p.Age} | Pozisyon: {posText}\nSözleşme süresini uzun tutarsanız komisyon oranlarında esneklik payı artar.";
            }
            Text descTxt = CreateText(headerTextContainer.transform, "NegoDesc", descStr, 36, new Color(0.7f, 0.75f, 0.8f), TextAnchor.MiddleLeft);
            var descScaler = descTxt.GetComponent<TextScaler>();
            if (descScaler != null) Destroy(descScaler);
            descTxt.fontSize = 36;
            descTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
            descTxt.verticalOverflow = VerticalWrapMode.Overflow;

            // Separator
            GameObject sep = CreatePanelHelper(contentRt, "Separator", new Color(1f, 1f, 1f, 0.1f));
            LayoutElement sepLe = sep.AddComponent<LayoutElement>();
            sepLe.preferredHeight = 4f;

            return (contentRt, modalObj, cardObj);
        }

        public void ShowClubWageRenegotiation(Player p, System.Action onComplete)
        {
            if (p == null || p.CurrentContract == null)
            {
                if (onComplete != null) onComplete();
                return;
            }

            Club currentClub = DatabaseManager.Instance.GetClubById(p.CurrentContract.ClubId);
            string clubName = currentClub != null ? currentClub.Name : p.CurrentContract.ClubName;

            int currentWage = p.CurrentContract.WeeklyWage;
            int currentYears = p.CurrentContract.DurationYears;
            int maxTotalYears = 5;
            int maxAdditionalYears = Mathf.Max(0, maxTotalYears - currentYears);

            int proposedWage = Mathf.RoundToInt(currentWage * 1.15f);
            int addedYears = 0;

            var (content, overlay, cardObj) = CreateScrollableNegotiationCard(mainCanvas.transform, p, $"{clubName} - Maaş İyileştirme & Zam", null);

            Text wageValText = null;
            Text yearsValText = null;

            // 1. Current Contract Info (Read-Only Summary)
            GameObject infoRow = new GameObject("InfoRow", typeof(RectTransform));
            infoRow.transform.SetParent(content, false);
            LayoutElement infoLe = infoRow.AddComponent<LayoutElement>();
            infoLe.preferredHeight = 120f;
            Text infoTxt = CreateText(infoRow.transform, "InfoTxt", $"Mevcut Sözleşme: <b>€{currentWage:N0} / hafta</b> ({currentYears} Yıl Kalan)", 44, Color.white, TextAnchor.MiddleCenter);
            SetRectTransform(infoTxt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // 2. Wage Demand Row
            GameObject wageRow = new GameObject("WageRow", typeof(RectTransform));
            wageRow.transform.SetParent(content, false);
            LayoutElement wageLe = wageRow.AddComponent<LayoutElement>();
            wageLe.preferredHeight = 180f;

            Text lblWage = CreateText(wageRow.transform, "LblWage", "Yeni Haftalık Maaş:", 44, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(lblWage, new Vector2(0.02f, 0f), new Vector2(0.34f, 1f), Vector2.zero, Vector2.zero);
            lblWage.horizontalOverflow = HorizontalWrapMode.Wrap;

            wageValText = CreateText(wageRow.transform, "WageValText", $"€{proposedWage:N0}", 48, colorAccent, TextAnchor.MiddleRight);
            SetRectTransform(wageValText, new Vector2(0.36f, 0f), new Vector2(0.74f, 1f), Vector2.zero, Vector2.zero);
            wageValText.fontStyle = FontStyle.Bold;

            Text btnWageMinus = CreateButtonHelper(wageRow.transform, "BtnWageMinus", "-", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                proposedWage = Mathf.Max(currentWage, proposedWage - 250);
                wageValText.text = $"€{proposedWage:N0}";
            });
            SetRectTransform(btnWageMinus.transform.parent, new Vector2(0.76f, 0.1f), new Vector2(0.86f, 0.9f), Vector2.zero, Vector2.zero);
            btnWageMinus.fontSize = 48;

            Text btnWagePlus = CreateButtonHelper(wageRow.transform, "BtnWagePlus", "+", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                int maxWage = Mathf.RoundToInt(currentWage * 1.50f);
                proposedWage = Mathf.Min(maxWage, proposedWage + 250);
                wageValText.text = $"€{proposedWage:N0}";
            });
            SetRectTransform(btnWagePlus.transform.parent, new Vector2(0.88f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero, Vector2.zero);
            btnWagePlus.fontSize = 48;

            // 3. Extension Years Row (Capped so currentYears + addedYears <= 5)
            GameObject yearsRow = new GameObject("YearsRow", typeof(RectTransform));
            yearsRow.transform.SetParent(content, false);
            LayoutElement yearsLe = yearsRow.AddComponent<LayoutElement>();
            yearsLe.preferredHeight = 180f;

            Text lblYears = CreateText(yearsRow.transform, "LblYears", "Ek Sözleşme Süresi:", 44, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(lblYears, new Vector2(0.02f, 0f), new Vector2(0.34f, 1f), Vector2.zero, Vector2.zero);
            lblYears.horizontalOverflow = HorizontalWrapMode.Wrap;

            System.Action updateYearsText = () => {
                int totalY = currentYears + addedYears;
                yearsValText.text = addedYears > 0 ? $"+{addedYears} Yıl (Toplam {totalY} Yıl)" : $"Uzatma Yok ({currentYears} Yıl)";
            };

            yearsValText = CreateText(yearsRow.transform, "YearsValText", "", 44, colorAccent, TextAnchor.MiddleRight);
            SetRectTransform(yearsValText, new Vector2(0.36f, 0f), new Vector2(0.74f, 1f), Vector2.zero, Vector2.zero);
            yearsValText.fontStyle = FontStyle.Bold;
            updateYearsText();

            Text btnYearsMinus = CreateButtonHelper(yearsRow.transform, "BtnYearsMinus", "-", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                addedYears = Mathf.Max(0, addedYears - 1);
                updateYearsText();
            });
            SetRectTransform(btnYearsMinus.transform.parent, new Vector2(0.76f, 0.1f), new Vector2(0.86f, 0.9f), Vector2.zero, Vector2.zero);
            btnYearsMinus.fontSize = 48;

            Text btnYearsPlus = CreateButtonHelper(yearsRow.transform, "BtnYearsPlus", "+", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                addedYears = Mathf.Min(maxAdditionalYears, addedYears + 1);
                updateYearsText();
            });
            SetRectTransform(btnYearsPlus.transform.parent, new Vector2(0.88f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero, Vector2.zero);
            btnYearsPlus.fontSize = 48;

            // 4. Feedback Box
            GameObject feedbackPanel = CreatePanelHelper(content, "FeedbackPanel", new Color(0f, 0f, 0f, 0.25f));
            LayoutElement fbLe = feedbackPanel.AddComponent<LayoutElement>();
            fbLe.preferredHeight = 260f;

            Text feedbackTxt = CreateText(feedbackPanel.transform, "FeedbackTxt", $"{clubName} yönetimi zam ve sözleşme uzatma talebinizi değerlendiriyor.", 44, new Color(0.8f, 0.85f, 0.9f), TextAnchor.MiddleCenter);
            SetRectTransform(feedbackTxt, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero);
            feedbackTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
            feedbackTxt.verticalOverflow = VerticalWrapMode.Overflow;

            // 5. Submit Button
            GameObject submitContainer = new GameObject("SubmitContainer", typeof(RectTransform));
            submitContainer.transform.SetParent(content, false);
            LayoutElement subLe = submitContainer.AddComponent<LayoutElement>();
            subLe.preferredHeight = 120f;

            Text btnSubmit = CreateButtonHelper(submitContainer.transform, "BtnSubmitWageNego", "KULÜBE TEKLİFİ SUN", colorGreen, new Color(11f/255f, 12f/255f, 16f/255f, 1f), () => {
                int maxAcceptableWage = Mathf.RoundToInt(currentWage * 1.40f);
                if (proposedWage > maxAcceptableWage)
                {
                    feedbackTxt.text = $"<color=#E74C3C>{clubName} Başkanı: 'Talep ettiğiniz €{proposedWage:N0} maaş kulübümüzün maaş dengesini aşıyor! En fazla €{maxAcceptableWage:N0} verebiliriz.'</color>";
                    return;
                }

                int totalYears = currentYears + addedYears;
                p.CurrentContract.WeeklyWage = proposedWage;
                p.CurrentContract.DurationYears = totalYears;
                p.Happiness = Mathf.Clamp(p.Happiness + 25f, 0f, 100f);

                AgencyManager.Instance.LogActivity($"ZAM ANLAŞMASI: Müşteriniz {p.Name}, {clubName} kulübü ile sözleşmesini yeniledi (Yeni Maaş: €{proposedWage:N0}/hafta, Toplam Süre: {totalYears} Yıl). Oyuncu Morali: +25");

                Destroy(overlay);
                if (onComplete != null) onComplete();
            });
            SetRectTransform(btnSubmit.transform.parent, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.95f), Vector2.zero, Vector2.zero);
        }

        public void ShowClubKeepOfferPopup(Player p, System.Action onComplete)
        {
            if (p == null || p.CurrentContract == null)
            {
                if (onComplete != null) onComplete();
                return;
            }

            Club currentClub = DatabaseManager.Instance.GetClubById(p.CurrentContract.ClubId);
            string clubName = currentClub != null ? currentClub.Name : p.CurrentContract.ClubName;

            string posText = p.Position == PlayerPosition.GK ? "Kaleci (GK)" :
                             p.Position == PlayerPosition.DEF ? "Defans (DEF)" :
                             p.Position == PlayerPosition.MID ? "Orta Saha (MID)" : "Forvet (FWD)";

            string descHeader = $"GEN: {p.OVR} | Yaş: {p.Age} | Pozisyon: {posText}\nKulüp yönetimi kilit oyuncusunu takımda tutmak istiyor.";

            var (content, overlay, cardObj) = CreateScrollableNegotiationCard(mainCanvas.transform, p, $"{clubName} - Kulüp Görüşmesi & Teklifi", null, descHeader);

            // Plain text message (NO gold box, NO border, dynamic vertical height)
            string textStr = $"<color=#F1C40F><b>{clubName} Başkanı:</b></color>\n\n" +
                             $"'{p.Name} kadromuzun en değerli oyuncularından biridir ve onu kesinlikle kaybetmek istemiyoruz!\n\n" +
                             $"Takımdan ayrılmak yerine maaşına zam yaparak kulübümüzde kalmasını teklif ediyoruz. Zam görüşmelerine başlamak ister misiniz?'";

            Text msgTxt = CreateText(content, "PresidentMessageText", textStr, 40, Color.white, TextAnchor.MiddleLeft);
            var msgScaler = msgTxt.GetComponent<TextScaler>();
            if (msgScaler != null) Destroy(msgScaler);
            msgTxt.fontSize = 40;
            msgTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
            msgTxt.verticalOverflow = VerticalWrapMode.Overflow;

            ContentSizeFitter msgCsf = msgTxt.gameObject.AddComponent<ContentSizeFitter>();
            msgCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            msgCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Buttons Container at bottom
            GameObject btnContainer = new GameObject("BtnContainer", typeof(RectTransform));
            btnContainer.transform.SetParent(content, false);
            
            LayoutElement btnLe = btnContainer.AddComponent<LayoutElement>();
            btnLe.preferredHeight = 120f;
            btnLe.minHeight = 120f;

            HorizontalLayoutGroup btnHlg = btnContainer.AddComponent<HorizontalLayoutGroup>();
            btnHlg.spacing = 20;
            btnHlg.childControlWidth = true;
            btnHlg.childControlHeight = true;
            btnHlg.childForceExpandWidth = true;
            btnHlg.childForceExpandHeight = true;

            // Yes (Zam Yap) Button
            Text btnYes = CreateButtonHelper(btnContainer.transform, "BtnYesNego", "EVET (ZAM YAP)", colorGreen, new Color(11f/255f, 12f/255f, 16f/255f, 1f), () => {
                Destroy(overlay);
                ShowClubWageRenegotiation(p, () => {
                    p.IsTransferListed = false;
                    p.TransferStatusNote = "Kulüpte Kalıyor (Zam Yapıldı)";
                    if (onComplete != null) onComplete();
                });
            });
            btnYes.fontSize = 40;
            btnYes.fontStyle = FontStyle.Bold;

            // No (Transfer Listesine Koy) Button
            Text btnNo = CreateButtonHelper(btnContainer.transform, "BtnNoTransfer", "REDDET (TRANSFER ET)", colorRed, Color.white, () => {
                p.IsTransferListed = true;
                p.IsSuggestedForLoan = true;
                p.TransferStatusNote = "Transfer Listesinde (Ayrılmak İstiyor)";
                AgencyManager.Instance.LogActivity($"TRANSFER TALEBİ: {p.Name} kulübün zam teklifini reddetti ve transfer listesine konuldu.");
                Destroy(overlay);
                if (onComplete != null) onComplete();
            });
            btnNo.fontSize = 40;
            btnNo.fontStyle = FontStyle.Bold;
        }

        public void ShowSignNegotiation(Player p, System.Action onSignSuccess)
        {
            // Capacity check first!
            if (AgencyManager.Instance.ActiveAgency.Clients.Count >= AgencyManager.Instance.ActiveAgency.MaxClientsCapacity && !p.IsAgencyClient)
            {
                string capMsg = $"Ajans kapasitesi dolu ({AgencyManager.Instance.ActiveAgency.Clients.Count}/{AgencyManager.Instance.ActiveAgency.MaxClientsCapacity}).";
                AgencyManager.Instance.LogActivity($"Sözleşme başarısız: {capMsg}");
                ShowFeedbackPopup($"HATA: {capMsg}\n\nYeni bir oyuncuyu ajansa katabilmek için ajans seviyenizi yükseltmeli veya mevcut bir oyuncuyla yollarınızı ayırmalısınız.");
                return;
            }
            // OVR cap check first!
            int allowedOvr = 70;
            if (AgencyManager.Instance.ActiveAgency.Level == 2) allowedOvr = 78;
            else if (AgencyManager.Instance.ActiveAgency.Level == 3) allowedOvr = 84;
            else if (AgencyManager.Instance.ActiveAgency.Level == 4) allowedOvr = 90;
            else if (AgencyManager.Instance.ActiveAgency.Level >= 5) allowedOvr = 99;
 
            if (p.OVR > allowedOvr && !p.IsAgencyClient)
            {
                string ovrMsg = $"Ajans seviyeniz bu oyuncuyu temsil etmek için yetersiz! {p.Name} (GEN: {p.OVR}) seviyesindeki bir oyuncuyla anlaşabilmek için ajansınızın seviyesini yükseltmelisiniz. (Mevcut GEN Sınırı: {allowedOvr})";
                AgencyManager.Instance.LogActivity($"Sözleşme başarısız: {ovrMsg}");
                ShowFeedbackPopup($"HATA: {ovrMsg}");
                return;
            }

            // Create scrollable card layout using our helper
            var (content, modalObj, cardObj) = CreateScrollableNegotiationCard(mainCanvas.transform, p, p.IsAgencyClient ? $"{p.Name} ile Sözleşme Yenileme" : $"{p.Name} ile Temsilcilik Pazarlığı", () => {
                if (activeSubpanel != null) activeSubpanel.Refresh();
            });

            // Starting values
            int trPct = 10; // 10%
            int wagePct = 5; // 5%
            int spPct = 10; // 10%
            int yearsOffer = 2; // Default 2 years

            // Value Display Texts
            Text trValText = null;
            Text wageValText = null;
            Text spValText = null;
            Text durValText = null;

            // Interactive negotiation items parented to scroll content with Vertical Layout!
            // Item 1: Transfer Commission
            GameObject trRow = new GameObject("TransferRow", typeof(RectTransform));
            trRow.transform.SetParent(content, false);
            LayoutElement trLe = trRow.AddComponent<LayoutElement>();
            trLe.preferredHeight = 180f;

            Text trLabel = CreateText(trRow.transform, "TrLabel", "Transfer Komisyon Payı:", 58, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(trLabel, new Vector2(0.02f, 0f), new Vector2(0.38f, 1f), Vector2.zero, Vector2.zero);
            trLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            trLabel.verticalOverflow = VerticalWrapMode.Overflow;

            trValText = CreateText(trRow.transform, "TrValText", $"%{trPct}", 64, colorAccent, TextAnchor.MiddleRight);
            SetRectTransform(trValText, new Vector2(0.39f, 0f), new Vector2(0.72f, 1f), Vector2.zero, Vector2.zero);
            trValText.fontStyle = FontStyle.Bold;
            trValText.horizontalOverflow = HorizontalWrapMode.Overflow;
            trValText.verticalOverflow = VerticalWrapMode.Overflow;

            Text trMinus = CreateButtonHelper(trRow.transform, "BtnTrMinus", "-", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                trPct = Mathf.Max(5, trPct - 1);
                trValText.text = $"%{trPct}";
            });
            SetRectTransform(trMinus.transform.parent, new Vector2(0.74f, 0.1f), new Vector2(0.85f, 0.9f), Vector2.zero, Vector2.zero);
            trMinus.fontSize = 54;

            Text trPlus = CreateButtonHelper(trRow.transform, "BtnTrPlus", "+", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                trPct = Mathf.Min(25, trPct + 1);
                trValText.text = $"%{trPct}";
            });
            SetRectTransform(trPlus.transform.parent, new Vector2(0.87f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero, Vector2.zero);
            trPlus.fontSize = 54;

            // Item 2: Wage Commission
            GameObject wageRow = new GameObject("WageRow", typeof(RectTransform));
            wageRow.transform.SetParent(content, false);
            LayoutElement wageLe = wageRow.AddComponent<LayoutElement>();
            wageLe.preferredHeight = 180f;

            Text wageLabel = CreateText(wageRow.transform, "WageLabel", "Maaş Komisyon Payı:", 58, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(wageLabel, new Vector2(0.02f, 0f), new Vector2(0.38f, 1f), Vector2.zero, Vector2.zero);
            wageLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            wageLabel.verticalOverflow = VerticalWrapMode.Overflow;

            wageValText = CreateText(wageRow.transform, "WageValText", $"%{wagePct}", 64, colorAccent, TextAnchor.MiddleRight);
            SetRectTransform(wageValText, new Vector2(0.39f, 0f), new Vector2(0.72f, 1f), Vector2.zero, Vector2.zero);
            wageValText.fontStyle = FontStyle.Bold;
            wageValText.horizontalOverflow = HorizontalWrapMode.Overflow;
            wageValText.verticalOverflow = VerticalWrapMode.Overflow;

            Text wageMinus = CreateButtonHelper(wageRow.transform, "BtnWageMinus", "-", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                wagePct = Mathf.Max(2, wagePct - 1);
                wageValText.text = $"%{wagePct}";
            });
            SetRectTransform(wageMinus.transform.parent, new Vector2(0.74f, 0.1f), new Vector2(0.85f, 0.9f), Vector2.zero, Vector2.zero);
            wageMinus.fontSize = 54;

            Text wagePlus = CreateButtonHelper(wageRow.transform, "BtnWagePlus", "+", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                wagePct = Mathf.Min(15, wagePct + 1);
                wageValText.text = $"%{wagePct}";
            });
            SetRectTransform(wagePlus.transform.parent, new Vector2(0.87f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero, Vector2.zero);
            wagePlus.fontSize = 54;

            // Item 3: Sponsor Commission
            GameObject spRow = new GameObject("SponsorRow", typeof(RectTransform));
            spRow.transform.SetParent(content, false);
            LayoutElement spLe = spRow.AddComponent<LayoutElement>();
            spLe.preferredHeight = 180f;

            Text spLabel = CreateText(spRow.transform, "SpLabel", "Sponsorluk Komisyon Payı:", 58, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(spLabel, new Vector2(0.02f, 0f), new Vector2(0.38f, 1f), Vector2.zero, Vector2.zero);
            spLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            spLabel.verticalOverflow = VerticalWrapMode.Overflow;

            spValText = CreateText(spRow.transform, "SpValText", $"%{spPct}", 64, colorAccent, TextAnchor.MiddleRight);
            SetRectTransform(spValText, new Vector2(0.39f, 0f), new Vector2(0.72f, 1f), Vector2.zero, Vector2.zero);
            spValText.fontStyle = FontStyle.Bold;
            spValText.horizontalOverflow = HorizontalWrapMode.Overflow;
            spValText.verticalOverflow = VerticalWrapMode.Overflow;

            Text spMinus = CreateButtonHelper(spRow.transform, "BtnSpMinus", "-", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                spPct = Mathf.Max(5, spPct - 1);
                spValText.text = $"%{spPct}";
            });
            SetRectTransform(spMinus.transform.parent, new Vector2(0.74f, 0.1f), new Vector2(0.85f, 0.9f), Vector2.zero, Vector2.zero);
            spMinus.fontSize = 54;

            Text spPlus = CreateButtonHelper(spRow.transform, "BtnSpPlus", "+", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                spPct = Mathf.Min(25, spPct + 1);
                spValText.text = $"%{spPct}";
            });
            SetRectTransform(spPlus.transform.parent, new Vector2(0.87f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero, Vector2.zero);
            spPlus.fontSize = 54;

            // Item 4: Contract Duration Years
            GameObject durRow = new GameObject("DurationRow", typeof(RectTransform));
            durRow.transform.SetParent(content, false);
            LayoutElement durLe = durRow.AddComponent<LayoutElement>();
            durLe.preferredHeight = 180f;

            Text durLabel = CreateText(durRow.transform, "DurLabel", "Temsilcilik Süresi (Yıl):", 58, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(durLabel, new Vector2(0.02f, 0f), new Vector2(0.38f, 1f), Vector2.zero, Vector2.zero);
            durLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            durLabel.verticalOverflow = VerticalWrapMode.Overflow;

            durValText = CreateText(durRow.transform, "DurValText", BehindTheScenesFootball.Managers.LocalizationManager.Translate($"{yearsOffer} Yıl"), 64, colorAccent, TextAnchor.MiddleRight);
            SetRectTransform(durValText, new Vector2(0.39f, 0f), new Vector2(0.72f, 1f), Vector2.zero, Vector2.zero);
            durValText.fontStyle = FontStyle.Bold;
            durValText.horizontalOverflow = HorizontalWrapMode.Overflow;
            durValText.verticalOverflow = VerticalWrapMode.Overflow;

            Text durMinus = CreateButtonHelper(durRow.transform, "BtnDurMinus", "-", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                yearsOffer = Mathf.Max(1, yearsOffer - 1);
                durValText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"{yearsOffer} Yıl");
            });
            SetRectTransform(durMinus.transform.parent, new Vector2(0.74f, 0.1f), new Vector2(0.85f, 0.9f), Vector2.zero, Vector2.zero);
            durMinus.fontSize = 54;

            Text durPlus = CreateButtonHelper(durRow.transform, "BtnDurPlus", "+", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                yearsOffer = Mathf.Min(5, yearsOffer + 1);
                durValText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"{yearsOffer} Yıl");
            });
            SetRectTransform(durPlus.transform.parent, new Vector2(0.87f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero, Vector2.zero);
            durPlus.fontSize = 54;

            // Output feedback/rejection box at the bottom
            GameObject feedbackBox = CreatePanelHelper(content, "FeedbackBox", new Color(0.1f, 0.12f, 0.15f, 1f));
            LayoutElement fbLe = feedbackBox.AddComponent<LayoutElement>();
            fbLe.preferredHeight = 320f;
            
            Text feedbackTxt = CreateText(feedbackBox.transform, "FeedbackTxt", "Pazarlık ediliyor...", 56, new Color(0.7f, 0.75f, 0.8f), TextAnchor.MiddleCenter);
            SetRectTransform(feedbackTxt, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero);
            feedbackTxt.resizeTextForBestFit = false;
            feedbackTxt.resizeTextMinSize = 14;
            feedbackTxt.resizeTextMaxSize = 68;

            // Submit Button
            GameObject submitContainer = new GameObject("SubmitContainer", typeof(RectTransform));
            submitContainer.transform.SetParent(content, false);
            LayoutElement subLe = submitContainer.AddComponent<LayoutElement>();
            subLe.preferredHeight = 120f;

            Text submitLabel = CreateButtonHelper(submitContainer.transform, "BtnSubmitOffer", "TEKLİFİ SUN", colorGreen, new Color(11f/255f, 12f/255f, 16f/255f, 1f), () => {
                float transferOffer = trPct / 100f;
                float wageOffer = wagePct / 100f;
                float sponsorOffer = spPct / 100f;

                float durationPressure = (yearsOffer - 1) * 0.01f;

                float maxWagePercent = 0.12f - (p.OVR - 50) * 0.001f - durationPressure; 
                float maxTransferPercent = 0.18f - (p.OVR - 50) * 0.0015f - durationPressure * 1.5f;
                float maxSponsorPercent = 0.18f - (p.OVR - 50) * 0.0015f - durationPressure * 1.5f;

                if (p.PositiveTrait == "Çalışkan")
                {
                    maxWagePercent += 0.01f;
                    maxTransferPercent += 0.01f;
                    maxSponsorPercent += 0.01f;
                }
                if (p.NegativeTrait == "Sadakatsiz" || p.NegativeTrait == "Uyumsuz")
                {
                    maxWagePercent -= 0.015f;
                    maxTransferPercent -= 0.015f;
                    maxSponsorPercent -= 0.015f;
                }
                if (p.NegativeTrait == "Tembel" || p.NegativeTrait == "Güvenilmez")
                {
                    maxWagePercent -= 0.01f;
                    maxTransferPercent -= 0.01f;
                }

                maxWagePercent = Mathf.Clamp(maxWagePercent, 0.03f, 0.15f);
                maxTransferPercent = Mathf.Clamp(maxTransferPercent, 0.05f, 0.25f);
                maxSponsorPercent = Mathf.Clamp(maxSponsorPercent, 0.05f, 0.25f);

                if (wageOffer <= maxWagePercent && transferOffer <= maxTransferPercent && sponsorOffer <= maxSponsorPercent)
                {
                    if (p.IsAgencyClient)
                    {
                        p.CustomTransferCommissionPercent = transferOffer;
                        p.CustomWageCommissionPercent = wageOffer;
                        p.CustomSponsorCommissionPercent = sponsorOffer;
                        p.AgencyContractRemainingWeeks = yearsOffer * 52;
                        
                        SimulationEngine.Instance.ActiveMails.RemoveAll(m => m.PlayerId == p.Id && m.IsRenewalMail);

                        AgencyManager.Instance.LogActivity($"Temsilcilik sözleşmesi yenilendi: {p.Name} ({yearsOffer} Yıl, Trf: %{trPct}, Maaş: %{wagePct}, Sp: %{spPct}).");
                        Destroy(modalObj);
                        onSignSuccess?.Invoke();
                    }
                    else
                    {
                        if (AgencyManager.Instance.TrySignClient(p, transferOffer, wageOffer, sponsorOffer))
                        {
                            p.AgencyContractRemainingWeeks = yearsOffer * 52;
                            Destroy(modalObj);
                            onSignSuccess?.Invoke();
                        }
                    }
                }
                else
                {
                    p.Happiness = Mathf.Clamp(p.Happiness - 5f, 10f, 100f);
                    
                    if (wageOffer > maxWagePercent && transferOffer > maxTransferPercent && sponsorOffer > maxSponsorPercent)
                    {
                        feedbackTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"<color=#E74C3C>{p.Name}: 'Tüm talepleriniz (%{wagePct} maaş, %{trPct} transfer ve {yearsOffer} yıl) benim için çok fazla! Teklifinizi düşürün.'</color>");
                    }
                    else if (wageOffer > maxWagePercent)
                    {
                        feedbackTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"<color=#E74C3C>{p.Name}: 'Bu süre ({yearsOffer} yıl) için maaşımdan keseceğiniz pay (%{wagePct}) çok yüksek, bunu kabul edemem.'</color>");
                    }
                    else if (transferOffer > maxTransferPercent)
                    {
                        feedbackTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"<color=#E74C3C>{p.Name}: 'Seçtiğiniz sözleşme süresine ({yearsOffer} yıl) karşılık transfer payı talebiniz (%{trPct}) çok abartılı.'</color>");
                    }
                    else
                    {
                        feedbackTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"<color=#E74C3C>{p.Name}: 'Sponsorluk komisyon oranı (%{spPct}) bu uzunluktaki sözleşme için çok fazla.'</color>");
                    }
                }
            });
            SetRectTransform(submitLabel.transform.parent, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.95f), Vector2.zero, Vector2.zero);
        }

        public void ConfigureButtonTransition(Button btn)
        {
            if (btn == null) return;
            btn.transition = Selectable.Transition.ColorTint;
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            cb.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            cb.selectedColor = Color.white;
            cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.05f; // fast snappier response
            btn.colors = cb;

            btn.onClick.AddListener(() => {
                if (UnityEngine.EventSystems.EventSystem.current != null)
                {
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                }
            });
        }

        private void CreateDetailLabel(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            Text txt = CreateText(parent, name, label, 46, Color.white, TextAnchor.MiddleLeft);
            var scaler = txt.GetComponent<TextScaler>();
            if (scaler != null) Destroy(scaler);
            txt.fontSize = 46;
            txt.fontStyle = FontStyle.Bold;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.resizeTextForBestFit = false;
            SetRectTransform(txt, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        }

        private void CreateDetailBadge(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Color bgCol, Color textCol, int fontSize, Color borderCol = default(Color))
        {
            GameObject badge = CreatePanelHelper(parent, name, bgCol);
            Image img = badge.GetComponent<Image>();
            if (img != null && roundedButtonSprite != null)
            {
                img.sprite = roundedButtonSprite;
                img.type = Image.Type.Sliced;
            }
            SetRectTransform(badge, anchorMin, anchorMax, Vector2.zero, Vector2.zero);

            if (borderCol != default(Color))
            {
                Outline outl = badge.AddComponent<Outline>();
                outl.effectColor = borderCol;
                outl.effectDistance = new Vector2(2f, 2f);
            }

            Text txt = CreateText(badge.transform, "Text", label, fontSize, textCol, TextAnchor.MiddleCenter);
            SetRectTransform(txt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            txt.fontStyle = FontStyle.Bold;
        }

        public string GetCountryFlagEmoji(string nationality)
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

        private void ShowClubSuggestions(Player p, GameObject detailsModal)
        {
            GameObject overlay = new GameObject("ClubSuggestionsOverlay");
            overlay.transform.SetParent(detailsModal.transform, false);
            SetRectTransform(overlay, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero);

            Image bg = overlay.AddComponent<Image>();
            bg.color = new Color(0.10f, 0.13f, 0.18f, 0.99f);
            if (roundedButtonSprite != null)
            {
                bg.sprite = roundedButtonSprite;
                bg.type = Image.Type.Sliced;
            }

            Outline border = overlay.AddComponent<Outline>();
            border.effectColor = colorAccent;
            border.effectDistance = new Vector2(2f, 2f);

            overlay.AddComponent<CanvasGroup>();

            // Title
            Text title = CreateText(overlay.transform, "Title", "KULÜP TEKLİFLERİ & SÖZLEŞME", 60, Color.white, TextAnchor.MiddleCenter);
            SetRectTransform(title, new Vector2(0.02f, 0.85f), new Vector2(0.98f, 0.97f), Vector2.zero, Vector2.zero);
            title.fontStyle = FontStyle.Bold;

            // Close button inside overlay
            Text btnClose = CreateButtonHelper(overlay.transform, "BtnCloseOverlay", "GERİ DÖN", colorRed, Color.white, () => {
                Destroy(overlay);
            });
            SetRectTransform(btnClose.transform.parent, new Vector2(0.38f, 0.05f), new Vector2(0.62f, 0.15f), Vector2.zero, Vector2.zero);
            btnClose.fontSize = 44;
            btnClose.fontStyle = FontStyle.Bold;

            // Content Container for suggestions (Scrollable)
            Transform listContent;
            GameObject scrollViewObj = CreateScrollViewHelper(overlay.transform, "SuggestionsScrollView", out listContent);
            SetRectTransform(scrollViewObj, new Vector2(0.02f, 0.18f), new Vector2(0.98f, 0.84f), Vector2.zero, Vector2.zero);

            VerticalLayoutGroup vlg = listContent.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                vlg.spacing = 20f;
                vlg.padding = new RectOffset(15, 15, 15, 15);
            }

            // Generate weekly deterministic club suggestions
            int currentWeekNum = SimulationEngine.Instance.CurrentWeek + SimulationEngine.Instance.CurrentYear * 52;
            int seed = p.Id.GetHashCode() + currentWeekNum;
            UnityEngine.Random.State oldState = UnityEngine.Random.state;
            UnityEngine.Random.InitState(seed);

            List<Club> allClubs = new List<Club>(DatabaseManager.Instance.Clubs);
            List<Club> suitable = new List<Club>();

            foreach (var c in allClubs)
            {
                if (p.OVR > 75 && c.Prestige < 60) continue;
                if (p.OVR > 85 && c.Prestige < 75) continue;
                if (p.OVR < 55 && c.Prestige > 65) continue;
                if (p.OVR < 65 && c.Prestige > 80) continue;
                suitable.Add(c);
            }

            List<Club> offers = new List<Club>();
            while (suitable.Count > 0 && offers.Count < 3)
            {
                int idx = UnityEngine.Random.Range(0, suitable.Count);
                offers.Add(suitable[idx]);
                suitable.RemoveAt(idx);
            }

            UnityEngine.Random.state = oldState; // Restore!

            if (offers.Count == 0)
            {
                GameObject row = CreatePanelHelper(listContent, "EmptyOffers", new Color(0f, 0f, 0f, 0f));
                LayoutElement le = row.AddComponent<LayoutElement>();
                le.minHeight = 200f;
                le.preferredHeight = 200f;

                Text msg = CreateText(row.transform, "MsgText", "Bu hafta oyuncunuza uygun transfer teklifi bulunmuyor. Gelecek hafta tekrar deneyebilirsiniz.", 36, Color.white, TextAnchor.MiddleCenter);
                SetRectTransform(msg, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            }
            else
            {
                foreach (var c in offers)
                {
                    // Calculate role
                    string role = "İlk 11 Oyuncusu";
                    int diff = p.OVR - c.Prestige;
                    if (p.Age < 21 && p.POT > p.OVR + 12) role = "Genç Yetenek";
                    else if (diff >= 12) role = "Yıldız Oyuncu";
                    else if (diff >= 5) role = "Önemli Oyuncu";
                    else if (diff >= -3) role = "İlk 11 Oyuncusu";
                    else if (diff >= -10) role = "Rotasyon Oyuncusu";
                    else role = "Yedek Oyuncu";

                    // Calculate wage offer
                    float baseVal = 0f;
                    if (p.OVR < 50) baseVal = UnityEngine.Random.Range(500, 1500);
                    else if (p.OVR < 60) baseVal = Mathf.Lerp(1500, 5000, (p.OVR - 50) / 10f);
                    else if (p.OVR < 70) baseVal = Mathf.Lerp(5000, 15000, (p.OVR - 60) / 10f);
                    else if (p.OVR < 80) baseVal = Mathf.Lerp(15000, 55000, (p.OVR - 70) / 10f);
                    else baseVal = Mathf.Lerp(55000, 150000, (p.OVR - 80) / 19f);

                    float prestigeFactor = c.Prestige / 80f;
                    int offeredWage = Mathf.RoundToInt(baseVal * prestigeFactor);

                    if (role == "Yıldız Oyuncu") offeredWage = Mathf.RoundToInt(offeredWage * 1.30f);
                    else if (role == "Önemli Oyuncu") offeredWage = Mathf.RoundToInt(offeredWage * 1.15f);
                    else if (role == "Rotasyon Oyuncusu") offeredWage = Mathf.RoundToInt(offeredWage * 0.85f);
                    else if (role == "Yedek Oyuncu") offeredWage = Mathf.RoundToInt(offeredWage * 0.70f);

                    offeredWage = Mathf.Clamp(offeredWage, 500, 500000);

                    // Row UI container
                    GameObject row = CreatePanelHelper(listContent, "OfferRow_" + c.Id, new Color(0.15f, 0.17f, 0.22f, 0.8f));
                    LayoutElement le = row.AddComponent<LayoutElement>();
                    le.minHeight = 340f;
                    le.preferredHeight = 340f;

                    if (roundedButtonSprite != null)
                    {
                        Image rowImg = row.GetComponent<Image>();
                        rowImg.sprite = roundedButtonSprite;
                        rowImg.type = Image.Type.Sliced;
                    }

                    Outline rowBorder = row.AddComponent<Outline>();
                    rowBorder.effectColor = new Color(1f, 1f, 1f, 0.05f);
                    rowBorder.effectDistance = new Vector2(1f, 1f);

                    // Info text (Top half, spans almost full width)
                    string infoStr = $"<b>{c.Name}</b>\nRol: <color=#F1C40F><b>{role}</b></color>";
                    Text infoTxt = CreateText(row.transform, "InfoTxt", infoStr, 54, Color.white, TextAnchor.MiddleLeft);
                    SetRectTransform(infoTxt, new Vector2(0.04f, 0.48f), new Vector2(0.96f, 0.92f), Vector2.zero, Vector2.zero);
                    infoTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
                    infoTxt.verticalOverflow = VerticalWrapMode.Overflow;

                    // Wage text (Bottom-left, left aligned)
                    string wageStr = $"Haftalık: <color=#2ECC71><b>€{offeredWage:N0}</b></color>";
                    Text wageTxt = CreateText(row.transform, "WageTxt", wageStr, 52, new Color(0.7f, 0.75f, 0.8f), TextAnchor.MiddleLeft);
                    SetRectTransform(wageTxt, new Vector2(0.04f, 0.08f), new Vector2(0.52f, 0.42f), Vector2.zero, Vector2.zero);
                    wageTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
                    wageTxt.verticalOverflow = VerticalWrapMode.Overflow;

                    // Sign Button (Bottom-right, occupies 40% width)
                    Text btnSign = CreateButtonHelper(row.transform, "BtnSignOffer", "PAZARLIK ET", colorGold, new Color(11f/255f, 12f/255f, 16f/255f, 1f), () => {
                        ShowClubNegotiation(p, c, role, offeredWage, detailsModal);
                    });
                    SetRectTransform(btnSign.transform.parent, new Vector2(0.56f, 0.08f), new Vector2(0.96f, 0.42f), Vector2.zero, Vector2.zero);
                    btnSign.resizeTextForBestFit = false;
                    btnSign.horizontalOverflow = HorizontalWrapMode.Overflow;
                    btnSign.verticalOverflow = VerticalWrapMode.Overflow;
                    btnSign.fontSize = 44;
                }
            }
        }

        private void ShowClubNegotiation(Player p, Club targetClub, string defaultRole, int offeredWage, GameObject detailsModal)
        {
            // Use our helper to create scroll view inside overlay
            var (content, overlay, cardObj) = CreateScrollableNegotiationCard(detailsModal.transform, p, $"{targetClub.Name} Görüşmesi", null);

            // Geri dön button is handled by close button on top-right of the scrollable card

            // Negotiation values
            string[] rolesList = { "Yedek Oyuncu", "Genç Yetenek", "Rotasyon Oyuncusu", "İlk 11 Oyuncusu", "Önemli Oyuncu", "Yıldız Oyuncu" };
            int selectedRoleIdx = 3; // default: İlk 11 Oyuncusu
            for (int i = 0; i < rolesList.Length; i++)
            {
                if (rolesList[i] == defaultRole)
                {
                    selectedRoleIdx = i;
                    break;
                }
            }

            int proposedWage = offeredWage;
            int proposedYears = 3;
            int proposedBonus = Mathf.RoundToInt(offeredWage * 4.5f);

            // Value labels
            Text roleValText = null;
            Text wageValText = null;
            Text yearsValText = null;
            Text bonusValText = null;

            // 1. Role row
            GameObject roleRow = new GameObject("RoleRow", typeof(RectTransform));
            roleRow.transform.SetParent(content, false);
            LayoutElement roleLe = roleRow.AddComponent<LayoutElement>();
            roleLe.preferredHeight = 180f;

            Text lblRole = CreateText(roleRow.transform, "LblRole", "Önerilen Rol:", 48, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(lblRole, new Vector2(0.02f, 0f), new Vector2(0.30f, 1f), Vector2.zero, Vector2.zero);
            lblRole.horizontalOverflow = HorizontalWrapMode.Wrap;
            lblRole.verticalOverflow = VerticalWrapMode.Overflow;

            roleValText = CreateText(roleRow.transform, "RoleValText", BehindTheScenesFootball.Managers.LocalizationManager.Translate(rolesList[selectedRoleIdx]), 48, colorAccent, TextAnchor.MiddleRight);
            SetRectTransform(roleValText, new Vector2(0.32f, 0f), new Vector2(0.74f, 1f), Vector2.zero, Vector2.zero);
            roleValText.fontStyle = FontStyle.Bold;
            roleValText.horizontalOverflow = HorizontalWrapMode.Overflow;
            roleValText.verticalOverflow = VerticalWrapMode.Overflow;

            Text btnRoleMinus = CreateButtonHelper(roleRow.transform, "BtnRoleMinus", "-", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                selectedRoleIdx = Mathf.Max(0, selectedRoleIdx - 1);
                roleValText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate(rolesList[selectedRoleIdx]);
            });
            SetRectTransform(btnRoleMinus.transform.parent, new Vector2(0.76f, 0.1f), new Vector2(0.86f, 0.9f), Vector2.zero, Vector2.zero);
            btnRoleMinus.fontSize = 48;

            Text btnRolePlus = CreateButtonHelper(roleRow.transform, "BtnRolePlus", "+", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                selectedRoleIdx = Mathf.Min(rolesList.Length - 1, selectedRoleIdx + 1);
                roleValText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate(rolesList[selectedRoleIdx]);
            });
            SetRectTransform(btnRolePlus.transform.parent, new Vector2(0.88f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero, Vector2.zero);
            btnRolePlus.fontSize = 48;

            // 2. Wage row
            GameObject wageRow = new GameObject("WageRow", typeof(RectTransform));
            wageRow.transform.SetParent(content, false);
            LayoutElement wageLe = wageRow.AddComponent<LayoutElement>();
            wageLe.preferredHeight = 180f;

            Text lblWage = CreateText(wageRow.transform, "LblWage", "Haftalık Maaş:", 48, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(lblWage, new Vector2(0.02f, 0f), new Vector2(0.30f, 1f), Vector2.zero, Vector2.zero);
            lblWage.horizontalOverflow = HorizontalWrapMode.Wrap;
            lblWage.verticalOverflow = VerticalWrapMode.Overflow;

            wageValText = CreateText(wageRow.transform, "WageValText", $"€{proposedWage:N0}", 48, colorAccent, TextAnchor.MiddleRight);
            SetRectTransform(wageValText, new Vector2(0.32f, 0f), new Vector2(0.74f, 1f), Vector2.zero, Vector2.zero);
            wageValText.fontStyle = FontStyle.Bold;
            wageValText.horizontalOverflow = HorizontalWrapMode.Overflow;
            wageValText.verticalOverflow = VerticalWrapMode.Overflow;

            Text btnWageMinus = CreateButtonHelper(wageRow.transform, "BtnWageMinus", "-", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                proposedWage = Mathf.Max(500, proposedWage - 250);
                wageValText.text = $"€{proposedWage:N0}";
            });
            SetRectTransform(btnWageMinus.transform.parent, new Vector2(0.76f, 0.1f), new Vector2(0.86f, 0.9f), Vector2.zero, Vector2.zero);
            btnWageMinus.fontSize = 48;

            Text btnWagePlus = CreateButtonHelper(wageRow.transform, "BtnWagePlus", "+", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                proposedWage = Mathf.Min(500000, proposedWage + 250);
                wageValText.text = $"€{proposedWage:N0}";
            });
            SetRectTransform(btnWagePlus.transform.parent, new Vector2(0.88f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero, Vector2.zero);
            btnWagePlus.fontSize = 48;

            // 3. Years row
            GameObject yearsRow = new GameObject("YearsRow", typeof(RectTransform));
            yearsRow.transform.SetParent(content, false);
            LayoutElement yearsLe = yearsRow.AddComponent<LayoutElement>();
            yearsLe.preferredHeight = 180f;

            Text lblYears = CreateText(yearsRow.transform, "LblYears", "Sözleşme Süresi:", 48, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(lblYears, new Vector2(0.02f, 0f), new Vector2(0.30f, 1f), Vector2.zero, Vector2.zero);
            lblYears.horizontalOverflow = HorizontalWrapMode.Wrap;
            lblYears.verticalOverflow = VerticalWrapMode.Overflow;

            yearsValText = CreateText(yearsRow.transform, "YearsValText", BehindTheScenesFootball.Managers.LocalizationManager.Translate($"{proposedYears} Yıl"), 48, colorAccent, TextAnchor.MiddleRight);
            SetRectTransform(yearsValText, new Vector2(0.32f, 0f), new Vector2(0.74f, 1f), Vector2.zero, Vector2.zero);
            yearsValText.fontStyle = FontStyle.Bold;
            yearsValText.horizontalOverflow = HorizontalWrapMode.Overflow;
            yearsValText.verticalOverflow = VerticalWrapMode.Overflow;

            Text btnYearsMinus = CreateButtonHelper(yearsRow.transform, "BtnYearsMinus", "-", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                proposedYears = Mathf.Max(1, proposedYears - 1);
                yearsValText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"{proposedYears} Yıl");
            });
            SetRectTransform(btnYearsMinus.transform.parent, new Vector2(0.76f, 0.1f), new Vector2(0.86f, 0.9f), Vector2.zero, Vector2.zero);
            btnYearsMinus.fontSize = 48;

            Text btnYearsPlus = CreateButtonHelper(yearsRow.transform, "BtnYearsPlus", "+", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                proposedYears = Mathf.Min(5, proposedYears + 1);
                yearsValText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"{proposedYears} Yıl");
            });
            SetRectTransform(btnYearsPlus.transform.parent, new Vector2(0.88f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero, Vector2.zero);
            btnYearsPlus.fontSize = 48;

            // 4. Bonus row
            GameObject bonusRow = new GameObject("BonusRow", typeof(RectTransform));
            bonusRow.transform.SetParent(content, false);
            LayoutElement bonusLe = bonusRow.AddComponent<LayoutElement>();
            bonusLe.preferredHeight = 180f;

            Text lblBonus = CreateText(bonusRow.transform, "LblBonus", "İmza Parası (Bonus):", 48, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(lblBonus, new Vector2(0.02f, 0f), new Vector2(0.30f, 1f), Vector2.zero, Vector2.zero);
            lblBonus.horizontalOverflow = HorizontalWrapMode.Wrap;
            lblBonus.verticalOverflow = VerticalWrapMode.Overflow;

            bonusValText = CreateText(bonusRow.transform, "BonusValText", $"€{proposedBonus:N0}", 48, colorAccent, TextAnchor.MiddleRight);
            SetRectTransform(bonusValText, new Vector2(0.32f, 0f), new Vector2(0.74f, 1f), Vector2.zero, Vector2.zero);
            bonusValText.fontStyle = FontStyle.Bold;
            bonusValText.horizontalOverflow = HorizontalWrapMode.Overflow;
            bonusValText.verticalOverflow = VerticalWrapMode.Overflow;

            Text btnBonusMinus = CreateButtonHelper(bonusRow.transform, "BtnBonusMinus", "-", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                proposedBonus = Mathf.Max(0, proposedBonus - 500);
                bonusValText.text = $"€{proposedBonus:N0}";
            });
            SetRectTransform(btnBonusMinus.transform.parent, new Vector2(0.76f, 0.1f), new Vector2(0.86f, 0.9f), Vector2.zero, Vector2.zero);
            btnBonusMinus.fontSize = 48;

            Text btnBonusPlus = CreateButtonHelper(bonusRow.transform, "BtnBonusPlus", "+", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                proposedBonus = Mathf.Min(500000, proposedBonus + 500);
                bonusValText.text = $"€{proposedBonus:N0}";
            });
            SetRectTransform(btnBonusPlus.transform.parent, new Vector2(0.88f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero, Vector2.zero);
            btnBonusPlus.fontSize = 48;

            // Feedback box
            GameObject feedbackPanel = CreatePanelHelper(content, "FeedbackPanel", new Color(0f, 0f, 0f, 0.25f));
            LayoutElement fbLe = feedbackPanel.AddComponent<LayoutElement>();
            fbLe.preferredHeight = 320f;

            Text feedbackTxt = CreateText(feedbackPanel.transform, "FeedbackTxt", "Görüşmeler olumlu geçiyor. Kulübün taleplerinizi değerlendirmesini isteyin.", 56, new Color(0.8f, 0.85f, 0.9f), TextAnchor.MiddleCenter);
            SetRectTransform(feedbackTxt, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero);
            feedbackTxt.resizeTextForBestFit = false;
            feedbackTxt.resizeTextMinSize = 14;
            feedbackTxt.resizeTextMaxSize = 68;

            // Submit Button
            GameObject submitContainer = new GameObject("SubmitContainer", typeof(RectTransform));
            submitContainer.transform.SetParent(content, false);
            LayoutElement subLe = submitContainer.AddComponent<LayoutElement>();
            subLe.preferredHeight = 120f;

            Text btnSubmit = CreateButtonHelper(submitContainer.transform, "BtnSubmitNego", "TEKLİFİ İMZALA", colorGreen, new Color(11f/255f, 12f/255f, 16f/255f, 1f), () => {
                string proposedRole = rolesList[selectedRoleIdx];
                
                System.Func<string, int> getRoleTier = (rName) => {
                    if (rName == "Yıldız Oyuncu") return 5;
                    if (rName == "Önemli Oyuncu") return 4;
                    if (rName == "İlk 11 Oyuncusu") return 3;
                    if (rName == "Rotasyon Oyuncusu") return 2;
                    if (rName == "Genç Yetenek") return 1;
                    return 0; // Yedek Oyuncu
                };
 
                int proposedTier = getRoleTier(proposedRole);
                int defaultTier = getRoleTier(defaultRole);
 
                if (proposedTier > defaultTier + 1)
                {
                    feedbackTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"<color=#E74C3C>{targetClub.Name} Yetkilisi: 'Oyuncu için talep ettiğiniz kadro rolü ({proposedRole}), planladığımız rolün ({defaultRole}) çok üzerinde. Bu teklifi kabul edemeyiz!'</color>");
                    p.Happiness = Mathf.Clamp(p.Happiness - 3f, 10f, 100f);
                    return;
                }

                System.Func<string, float> getRoleRatio = (rName) => {
                    if (rName == "Yıldız Oyuncu") return 1.30f;
                    if (rName == "Önemli Oyuncu") return 1.15f;
                    if (rName == "İlk 11 Oyuncusu") return 1.00f;
                    if (rName == "Genç Yetenek") return 0.80f;
                    if (rName == "Rotasyon Oyuncusu") return 0.85f;
                    return 0.70f;
                };

                int maxClubWage = Mathf.RoundToInt(offeredWage * (getRoleRatio(proposedRole) / getRoleRatio(defaultRole)));
                int maxClubBonus = Mathf.RoundToInt(maxClubWage * 5.0f);

                // Role appropriateness checks:
                if (proposedRole == "Yıldız Oyuncu" && p.OVR < targetClub.Prestige - 4)
                {
                    feedbackTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"<color=#E74C3C>{targetClub.Name} Yetkilisi: 'Bu oyuncunun kalitesi ({p.OVR} GEN) kadromuzda Yıldız rolü almak için yetersiz. En fazla Önemli Oyuncu olabilir!'</color>");
                    p.Happiness = Mathf.Clamp(p.Happiness - 3f, 10f, 100f);
                    return;
                }
                if (proposedRole == "Önemli Oyuncu" && p.OVR < targetClub.Prestige - 10)
                {
                    feedbackTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"<color=#E74C3C>{targetClub.Name} Yetkilisi: 'Bu oyuncu bu kadroda Önemli Oyuncu olamaz! En fazla İlk 11 Oyuncusu rolü verebiliriz.'</color>");
                    p.Happiness = Mathf.Clamp(p.Happiness - 2f, 10f, 100f);
                    return;
                }
                if (proposedWage > maxClubWage)
                {
                    feedbackTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"<color=#E74C3C>{targetClub.Name} Yetkilisi: 'Maaş talebiniz ({proposedWage:C0}) bu rol için bütçe sınırlarimizi ({maxClubWage:C0}) aşıyor! Lütfen talebinizi düşürün.'</color>");
                    p.Happiness = Mathf.Clamp(p.Happiness - 2f, 10f, 100f);
                    return;
                }
                if (proposedBonus > maxClubBonus)
                {
                    feedbackTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"<color=#E74C3C>{targetClub.Name} Yetkilisi: 'Talep ettiğiniz imza parası (€{proposedBonus:N0}) bütçe limitlerimizi (€{maxClubBonus:N0}) aşıyor. Lütfen teklifinizi düşürün.'</color>");
                    p.Happiness = Mathf.Clamp(p.Happiness - 2f, 10f, 100f);
                    return;
                }

                // Sign contract
                Contract newContract = new Contract(targetClub.Id, targetClub.Name, proposedWage, proposedYears, 0);
                targetClub.AddPlayer(p, newContract);
                p.CurrentContract = newContract;
                p.SquadRole = proposedRole;
                p.UpdateMarketValue();

                // Add signing bonus to agency cash balance directly
                AgencyManager.Instance.ActiveAgency.Balance += proposedBonus;

                // Clear pending offers
                p.PendingSponsorOffers.Clear();

                AgencyManager.Instance.LogActivity($"KULÜP ANLAŞMASI: Müşteriniz {p.Name}, {targetClub.Name} kulübü ile {proposedRole} rolünde haftalık €{proposedWage:N0} karşılığında {proposedYears} yıllık sözleşme imzaladı! Ajansımız €{proposedBonus:N0} imza parası kazandı!");

                Destroy(overlay);
                Destroy(detailsModal);
                if (activeSubpanel != null) activeSubpanel.Refresh();
            });
            SetRectTransform(btnSubmit.transform.parent, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.95f), Vector2.zero, Vector2.zero);
        }

        public void ShowTransferNegotiation(TransferOffer offer, System.Action onNegoSuccess)
        {
            Player p = DatabaseManager.Instance.GetPlayerById(offer.PlayerId);
            Club bidderClub = DatabaseManager.Instance.GetClubById(offer.BidderClubId);
            if (p == null || bidderClub == null) return;

            string descStr = offer.IsLoanOffer 
                ? $"Müşteriniz {p.Name} için {bidderClub.Name} kulübünden gelen kiralık teklifinin şartlarını görüşüyorsunuz.\nMaaşını mevcut kulübü ödemeye devam edecektir."
                : $"Müşteriniz {p.Name} için {bidderClub.Name} kulübü ile sözleşme şartlarını görüşüyorsunuz.\nKulübün transfer bütçesi: €{bidderClub.TransferBudget:N0}";
            
            var (content, overlay, cardObj) = CreateScrollableNegotiationCard(mainCanvas.transform, p, offer.IsLoanOffer ? $"{bidderClub.Name} Kiralama Görüşmesi" : $"{bidderClub.Name} Transfer Görüşmesi", null, descStr);

            int proposedFee = offer.TransferFee;
            int proposedWage = offer.OfferedWeeklyWage;
            int proposedYears = offer.ContractLengthYears;
            int proposedBonus = offer.IsLoanOffer ? 0 : Mathf.RoundToInt(offer.OfferedWeeklyWage * 4.5f);

            string[] rolesList = { "Yedek Oyuncu", "Genç Yetenek", "Rotasyon Oyuncusu", "İlk 11 Oyuncusu", "Önemli Oyuncu", "Yıldız Oyuncu" };
            int selectedRoleIdx = 3; // default: İlk 11 Oyuncusu
            
            // Determine default role dynamically based on player OVR and club prestige
            string defaultRole = "İlk 11 Oyuncusu";
            int diff = p.OVR - bidderClub.Prestige;
            if (p.Age < 21 && p.POT > p.OVR + 12 && !offer.IsLoanOffer) defaultRole = "Genç Yetenek";
            else if (diff >= 12) defaultRole = "Yıldız Oyuncu";
            else if (diff >= 5) defaultRole = "Önemli Oyuncu";
            else if (diff >= -3) defaultRole = "İlk 11 Oyuncusu";
            else if (diff >= -10) defaultRole = "Rotasyon Oyuncusu";
            else defaultRole = "Yedek Oyuncu";

            for (int i = 0; i < rolesList.Length; i++)
            {
                if (rolesList[i] == defaultRole)
                {
                    selectedRoleIdx = i;
                    break;
                }
            }

            Text feeValText = null;
            Text roleValText = null;
            Text wageValText = null;
            Text yearsValText = null;
            Text bonusValText = null;

            // Row 1: Transfer Fee / Anlaşma Türü (Read Only)
            GameObject feeRow = new GameObject("FeeRow", typeof(RectTransform));
            feeRow.transform.SetParent(content, false);
            LayoutElement feeLe = feeRow.AddComponent<LayoutElement>();
            feeLe.preferredHeight = 180f;

            Text lblFee = CreateText(feeRow.transform, "LblFee", offer.IsLoanOffer ? "Anlaşma Türü:" : "Bonservis Bedeli:", 48, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(lblFee, new Vector2(0.02f, 0f), new Vector2(0.30f, 1f), Vector2.zero, Vector2.zero);
            lblFee.horizontalOverflow = HorizontalWrapMode.Wrap;
            lblFee.verticalOverflow = VerticalWrapMode.Overflow;

            string feeTextVal = offer.IsLoanOffer ? "Kiralık (Mevcut Kulübü Öder)" : $"€{proposedFee:N0} (Kilitli)";
            feeValText = CreateText(feeRow.transform, "FeeValText", feeTextVal, 48, new Color(0.7f, 0.75f, 0.8f), TextAnchor.MiddleRight);
            SetRectTransform(feeValText, new Vector2(0.32f, 0f), new Vector2(0.98f, 1f), Vector2.zero, Vector2.zero);
            feeValText.fontStyle = FontStyle.Bold;
            feeValText.horizontalOverflow = HorizontalWrapMode.Overflow;
            feeValText.verticalOverflow = VerticalWrapMode.Overflow;

            // Row 2: Role (Kadro Rolü)
            GameObject roleRow = new GameObject("RoleRow", typeof(RectTransform));
            roleRow.transform.SetParent(content, false);
            LayoutElement roleLe = roleRow.AddComponent<LayoutElement>();
            roleLe.preferredHeight = 180f;

            Text lblRole = CreateText(roleRow.transform, "LblRole", "Önerilen Rol:", 48, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(lblRole, new Vector2(0.02f, 0f), new Vector2(0.30f, 1f), Vector2.zero, Vector2.zero);
            lblRole.horizontalOverflow = HorizontalWrapMode.Wrap;
            lblRole.verticalOverflow = VerticalWrapMode.Overflow;

            roleValText = CreateText(roleRow.transform, "RoleValText", BehindTheScenesFootball.Managers.LocalizationManager.Translate(rolesList[selectedRoleIdx]), 48, colorAccent, TextAnchor.MiddleRight);
            SetRectTransform(roleValText, new Vector2(0.32f, 0f), new Vector2(0.74f, 1f), Vector2.zero, Vector2.zero);
            roleValText.fontStyle = FontStyle.Bold;
            roleValText.horizontalOverflow = HorizontalWrapMode.Overflow;
            roleValText.verticalOverflow = VerticalWrapMode.Overflow;

            Text btnRoleMinus = CreateButtonHelper(roleRow.transform, "BtnRoleMinus", "-", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                selectedRoleIdx = Mathf.Max(0, selectedRoleIdx - 1);
                roleValText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate(rolesList[selectedRoleIdx]);
            });
            SetRectTransform(btnRoleMinus.transform.parent, new Vector2(0.76f, 0.1f), new Vector2(0.86f, 0.9f), Vector2.zero, Vector2.zero);
            btnRoleMinus.fontSize = 48;

            Text btnRolePlus = CreateButtonHelper(roleRow.transform, "BtnRolePlus", "+", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                selectedRoleIdx = Mathf.Min(rolesList.Length - 1, selectedRoleIdx + 1);
                roleValText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate(rolesList[selectedRoleIdx]);
            });
            SetRectTransform(btnRolePlus.transform.parent, new Vector2(0.88f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero, Vector2.zero);
            btnRolePlus.fontSize = 48;

            // Row 3: Wage (Oyuncu Maaşı - Hide for Loans)
            if (!offer.IsLoanOffer)
            {
                GameObject wageRow = new GameObject("WageRow", typeof(RectTransform));
                wageRow.transform.SetParent(content, false);
                LayoutElement wageLe = wageRow.AddComponent<LayoutElement>();
                wageLe.preferredHeight = 180f;

                Text lblWage = CreateText(wageRow.transform, "LblWage", "Haftalık Maaş:", 48, Color.white, TextAnchor.MiddleLeft);
                SetRectTransform(lblWage, new Vector2(0.02f, 0f), new Vector2(0.30f, 1f), Vector2.zero, Vector2.zero);
                lblWage.horizontalOverflow = HorizontalWrapMode.Wrap;
                lblWage.verticalOverflow = VerticalWrapMode.Overflow;

                wageValText = CreateText(wageRow.transform, "WageValText", $"€{proposedWage:N0}", 48, colorAccent, TextAnchor.MiddleRight);
                SetRectTransform(wageValText, new Vector2(0.32f, 0f), new Vector2(0.74f, 1f), Vector2.zero, Vector2.zero);
                wageValText.fontStyle = FontStyle.Bold;
                wageValText.horizontalOverflow = HorizontalWrapMode.Overflow;
                wageValText.verticalOverflow = VerticalWrapMode.Overflow;

                Text btnWageMinus = CreateButtonHelper(wageRow.transform, "BtnWageMinus", "-", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                    proposedWage = Mathf.Max(500, proposedWage - 500);
                    wageValText.text = $"€{proposedWage:N0}";
                });
                SetRectTransform(btnWageMinus.transform.parent, new Vector2(0.76f, 0.1f), new Vector2(0.86f, 0.9f), Vector2.zero, Vector2.zero);
                btnWageMinus.fontSize = 48;

                Text btnWagePlus = CreateButtonHelper(wageRow.transform, "BtnWagePlus", "+", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                    proposedWage = Mathf.Min(500000, proposedWage + 500);
                    wageValText.text = $"€{proposedWage:N0}";
                });
                SetRectTransform(btnWagePlus.transform.parent, new Vector2(0.88f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero, Vector2.zero);
                btnWagePlus.fontSize = 48;
            }

            // Row 4: Years (Sözleşme / Kiralama Süresi)
            GameObject yearsRow = new GameObject("YearsRow", typeof(RectTransform));
            yearsRow.transform.SetParent(content, false);
            LayoutElement yearsLe = yearsRow.AddComponent<LayoutElement>();
            yearsLe.preferredHeight = 180f;

            Text lblYears = CreateText(yearsRow.transform, "LblYears", offer.IsLoanOffer ? "Kiralama Süresi:" : "Sözleşme Süresi:", 48, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(lblYears, new Vector2(0.02f, 0f), new Vector2(0.30f, 1f), Vector2.zero, Vector2.zero);
            lblYears.horizontalOverflow = HorizontalWrapMode.Wrap;
            lblYears.verticalOverflow = VerticalWrapMode.Overflow;

            yearsValText = CreateText(yearsRow.transform, "YearsValText", $"{proposedYears} Yıl", 48, colorAccent, TextAnchor.MiddleRight);
            SetRectTransform(yearsValText, new Vector2(0.32f, 0f), new Vector2(0.74f, 1f), Vector2.zero, Vector2.zero);
            yearsValText.fontStyle = FontStyle.Bold;
            yearsValText.horizontalOverflow = HorizontalWrapMode.Overflow;
            yearsValText.verticalOverflow = VerticalWrapMode.Overflow;

            Text btnYearsMinus = CreateButtonHelper(yearsRow.transform, "BtnYearsMinus", "-", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                proposedYears = Mathf.Max(1, proposedYears - 1);
                yearsValText.text = $"{proposedYears} Yıl";
            });
            SetRectTransform(btnYearsMinus.transform.parent, new Vector2(0.76f, 0.1f), new Vector2(0.86f, 0.9f), Vector2.zero, Vector2.zero);
            btnYearsMinus.fontSize = 48;

            Text btnYearsPlus = CreateButtonHelper(yearsRow.transform, "BtnYearsPlus", "+", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                proposedYears = Mathf.Min(5, proposedYears + 1);
                yearsValText.text = $"{proposedYears} Yıl";
            });
            SetRectTransform(btnYearsPlus.transform.parent, new Vector2(0.88f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero, Vector2.zero);
            btnYearsPlus.fontSize = 48;

            // Row 5: Sign-on Bonus (İmza Parası - Hide for Loans)
            if (!offer.IsLoanOffer)
            {
                GameObject bonusRow = new GameObject("BonusRow", typeof(RectTransform));
                bonusRow.transform.SetParent(content, false);
                LayoutElement bonusLe = bonusRow.AddComponent<LayoutElement>();
                bonusLe.preferredHeight = 180f;

                Text lblBonus = CreateText(bonusRow.transform, "LblBonus", "İmza Parası:", 48, Color.white, TextAnchor.MiddleLeft);
                SetRectTransform(lblBonus, new Vector2(0.02f, 0f), new Vector2(0.30f, 1f), Vector2.zero, Vector2.zero);
                lblBonus.horizontalOverflow = HorizontalWrapMode.Wrap;
                lblBonus.verticalOverflow = VerticalWrapMode.Overflow;

                bonusValText = CreateText(bonusRow.transform, "BonusValText", $"€{proposedBonus:N0}", 48, colorAccent, TextAnchor.MiddleRight);
                SetRectTransform(bonusValText, new Vector2(0.32f, 0f), new Vector2(0.74f, 1f), Vector2.zero, Vector2.zero);
                bonusValText.fontStyle = FontStyle.Bold;
                bonusValText.horizontalOverflow = HorizontalWrapMode.Overflow;
                bonusValText.verticalOverflow = VerticalWrapMode.Overflow;

                Text btnBonusMinus = CreateButtonHelper(bonusRow.transform, "BtnBonusMinus", "-", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                    proposedBonus = Mathf.Max(0, proposedBonus - 1000);
                    bonusValText.text = $"€{proposedBonus:N0}";
                });
                SetRectTransform(btnBonusMinus.transform.parent, new Vector2(0.76f, 0.1f), new Vector2(0.86f, 0.9f), Vector2.zero, Vector2.zero);
                btnBonusMinus.fontSize = 48;

                Text btnBonusPlus = CreateButtonHelper(bonusRow.transform, "BtnBonusPlus", "+", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                    proposedBonus = proposedBonus + 1000;
                    bonusValText.text = $"€{proposedBonus:N0}";
                });
                SetRectTransform(btnBonusPlus.transform.parent, new Vector2(0.88f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero, Vector2.zero);
            }

            // Feedback box
            GameObject feedbackPanel = CreatePanelHelper(content, "FeedbackPanel", new Color(0f, 0f, 0f, 0.25f));
            LayoutElement fbLe = feedbackPanel.AddComponent<LayoutElement>();
            fbLe.preferredHeight = 320f;

            Text feedbackTxt = CreateText(feedbackPanel.transform, "FeedbackTxt", offer.IsLoanOffer ? "Oyuncu kiralama ve rol detaylarını yeni kulüple pazarlık ediyorsunuz." : "Oyuncu sözleşme ve rol detaylarını yeni kulüple pazarlık ediyorsunuz.", 56, new Color(0.8f, 0.85f, 0.9f), TextAnchor.MiddleCenter);
            SetRectTransform(feedbackTxt, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero);
            feedbackTxt.resizeTextForBestFit = false;

            // Submit Button
            GameObject submitContainer = new GameObject("SubmitContainer", typeof(RectTransform));
            submitContainer.transform.SetParent(content, false);
            LayoutElement subLe = submitContainer.AddComponent<LayoutElement>();
            subLe.preferredHeight = 120f;

            Text btnSubmit = CreateButtonHelper(submitContainer.transform, "BtnSubmitNego", "ANLAŞMAYI İMZALA", colorGreen, new Color(11f/255f, 12f/255f, 16f/255f, 1f), () => {
                string proposedRole = rolesList[selectedRoleIdx];

                System.Func<string, int> getRoleTier = (rName) => {
                    if (rName == "Yıldız Oyuncu") return 5;
                    if (rName == "Önemli Oyuncu") return 4;
                    if (rName == "İlk 11 Oyuncusu") return 3;
                    if (rName == "Rotasyon Oyuncusu") return 2;
                    if (rName == "Genç Yetenek") return 1;
                    return 0; // Yedek Oyuncu
                };

                int proposedTier = getRoleTier(proposedRole);
                int defaultTier = getRoleTier(defaultRole);

                if (proposedTier > defaultTier + 1)
                {
                    feedbackTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"<color=#E74C3C>{bidderClub.Name} Yetkilisi: 'Oyuncu için talep ettiğiniz kadro rolü ({proposedRole}), planladığımız rolün ({defaultRole}) çok üzerinde. Bu teklifi kabul edemeyiz!'</color>");
                    p.Happiness = Mathf.Clamp(p.Happiness - 3f, 10f, 100f);
                    return;
                }

                // Role evaluations
                if (proposedRole == "Yıldız Oyuncu" && p.OVR < bidderClub.Prestige - 5)
                {
                    feedbackTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"<color=#E74C3C>{bidderClub.Name} Yetkilisi: 'Bu oyuncu bu kadroda Yıldız Oyuncu olamaz! En fazla Önemli Oyuncu rolü verebiliriz.'</color>");
                    p.Happiness = Mathf.Clamp(p.Happiness - 2f, 10f, 100f);
                    return;
                }
                if (proposedRole == "Önemli Oyuncu" && p.OVR < bidderClub.Prestige - 10)
                {
                    feedbackTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"<color=#E74C3C>{bidderClub.Name} Yetkilisi: 'Bu oyuncu bu kadroda Önemli Oyuncu olamaz! En fazla İlk 11 Oyuncusu rolü verebiliriz.'</color>");
                    p.Happiness = Mathf.Clamp(p.Happiness - 2f, 10f, 100f);
                    return;
                }

                if (!offer.IsLoanOffer)
                {
                    // Wage Evaluation:
                    int maxClubWage = Mathf.RoundToInt(offer.OfferedWeeklyWage * 1.25f); // up to 25% increase allowed
                    if (proposedWage > maxClubWage)
                    {
                        feedbackTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"<color=#E74C3C>{bidderClub.Name} Yetkilisi: 'Oyuncu için talep ettiğiniz haftalık maaş ({proposedWage:C0}) maaş limitlerimizi ({maxClubWage:C0}) aşıyor!'</color>");
                        p.Happiness = Mathf.Clamp(p.Happiness - 2f, 10f, 100f);
                        return;
                    }

                    // Bonus Evaluation:
                    int maxClubBonus = Mathf.RoundToInt(offer.OfferedWeeklyWage * 6f); // up to 6x weekly wage allowed
                    if (proposedBonus > maxClubBonus)
                    {
                        feedbackTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"<color=#E74C3C>{bidderClub.Name} Yetkilisi: 'Talep ettiğiniz imza parası ({proposedBonus:C0}) bütçe limitlerimizi ({maxClubBonus:C0}) aşıyor!'</color>");
                        p.Happiness = Mathf.Clamp(p.Happiness - 2f, 10f, 100f);
                        return;
                    }
                }

                // Execute transfer / loan
                if (offer.IsLoanOffer)
                {
                    // For loans, wage is kept the same (current club continues to pay it)
                    Contract newContract = new Contract(bidderClub.Id, bidderClub.Name, p.CurrentContract.WeeklyWage, proposedYears, 0);
                    
                    p.IsOnLoan = true;
                    p.ParentClubId = p.CurrentContract != null ? p.CurrentContract.ClubId : "";
                    p.ParentClubName = p.CurrentContract != null ? p.CurrentContract.ClubName : "Serbest";
                    p.LoanRemainingWeeks = proposedYears * 52;

                    DatabaseManager.Instance.TransferPlayer(p, bidderClub, newContract, 0);
                    
                    p.SquadRole = proposedRole;
                    p.UpdateMarketValue();
                    
                    AgencyManager.Instance.LogActivity($"KİRALAMA BAŞARILI: Müşteriniz {p.Name}, {proposedYears} yıllığına {bidderClub.Name} kulübüne kiralandı!");
                }
                else
                {
                    p.IsOnLoan = false;
                    p.ParentClubId = null;
                    p.ParentClubName = null;
                    p.LoanRemainingWeeks = 0;

                    Contract newContract = new Contract(bidderClub.Id, bidderClub.Name, proposedWage, proposedYears, 0);
                    DatabaseManager.Instance.TransferPlayer(p, bidderClub, newContract, proposedFee);
                    
                    AgencyManager.Instance.CollectTransferCommission(p, proposedFee);
                    AgencyManager.Instance.ActiveAgency.Balance += proposedBonus;

                    p.SquadRole = proposedRole;
                    p.UpdateMarketValue();

                    AgencyManager.Instance.LogActivity($"TRANSFER BAŞARILI: Müşteriniz {p.Name}, €{proposedFee:N0} bonservis bedeliyle {bidderClub.Name} kulübüne transfer oldu! Ajansımız €{proposedBonus:N0} imza parası kazandı.");
                }

                // Clear pending offers for this player
                SimulationEngine.Instance.ActiveOffers.RemoveAll(o => o.PlayerId == p.Id);

                Destroy(overlay);
                onNegoSuccess?.Invoke();
            });
            SetRectTransform(btnSubmit.transform.parent, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.95f), Vector2.zero, Vector2.zero);
        }

        private void ShowSponsorOffersList(Player p, GameObject detailsModal)
        {
            GameObject overlay = new GameObject("SponsorOffersOverlay");
            overlay.transform.SetParent(detailsModal.transform, false);
            SetRectTransform(overlay, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero);

            Image bg = overlay.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.10f, 0.13f, 0.99f);
            if (roundedButtonSprite != null)
            {
                bg.sprite = roundedButtonSprite;
                bg.type = Image.Type.Sliced;
            }

            Outline border = overlay.AddComponent<Outline>();
            border.effectColor = colorAccent;
            border.effectDistance = new Vector2(2f, 2f);

            overlay.AddComponent<CanvasGroup>();

            // Title
            Text title = CreateText(overlay.transform, "Title", "SPONSORLUK TEKLİFLERİ", 60, Color.white, TextAnchor.MiddleCenter);
            SetRectTransform(title, new Vector2(0.02f, 0.85f), new Vector2(0.98f, 0.97f), Vector2.zero, Vector2.zero);
            title.fontStyle = FontStyle.Bold;

            // Close button
            Text btnClose = CreateButtonHelper(overlay.transform, "BtnCloseList", "GERİ DÖN", colorRed, Color.white, () => {
                Destroy(overlay);
            });
            SetRectTransform(btnClose.transform.parent, new Vector2(0.38f, 0.05f), new Vector2(0.62f, 0.15f), Vector2.zero, Vector2.zero);
            btnClose.fontSize = 44;
            btnClose.fontStyle = FontStyle.Bold;

            // Scroll container
            Transform listContent;
            GameObject scrollViewObj = CreateScrollViewHelper(overlay.transform, "OffersScrollView", out listContent);
            SetRectTransform(scrollViewObj, new Vector2(0.02f, 0.18f), new Vector2(0.98f, 0.84f), Vector2.zero, Vector2.zero);

            VerticalLayoutGroup vlg = listContent.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                vlg.spacing = 20f;
                vlg.padding = new RectOffset(15, 15, 15, 15);
            }

            foreach (var offer in p.PendingSponsorOffers)
            {
                GameObject row = CreatePanelHelper(listContent, "SponsorOfferRow_" + offer.BrandName, new Color(0.15f, 0.17f, 0.22f, 0.8f));
                LayoutElement le = row.AddComponent<LayoutElement>();
                le.minHeight = 300f;
                le.preferredHeight = 300f;

                if (roundedButtonSprite != null)
                {
                    Image rowImg = row.GetComponent<Image>();
                    rowImg.sprite = roundedButtonSprite;
                    rowImg.type = Image.Type.Sliced;
                }

                Outline rowBorder = row.AddComponent<Outline>();
                rowBorder.effectColor = new Color(1f, 1f, 1f, 0.05f);
                rowBorder.effectDistance = new Vector2(1f, 1f);

                // Brand details (Top half, spans almost full width)
                string infoStr = $"<b>{offer.BrandName}</b>\nGereken GEN: <b>{offer.MinOVRRequired}</b>";
                Text infoTxt = CreateText(row.transform, "InfoTxt", infoStr, 52, Color.white, TextAnchor.MiddleLeft);
                SetRectTransform(infoTxt, new Vector2(0.04f, 0.48f), new Vector2(0.96f, 0.92f), Vector2.zero, Vector2.zero);
                infoTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
                infoTxt.verticalOverflow = VerticalWrapMode.Overflow;

                // Base income (Bottom-left, left aligned)
                string valStr = $"Taban: <color=#2ECC71><b>€{offer.WeeklyIncome:N0}</b></color>";
                Text valTxt = CreateText(row.transform, "ValTxt", valStr, 48, new Color(0.7f, 0.75f, 0.8f), TextAnchor.MiddleLeft);
                SetRectTransform(valTxt, new Vector2(0.04f, 0.08f), new Vector2(0.52f, 0.42f), Vector2.zero, Vector2.zero);
                valTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
                valTxt.verticalOverflow = VerticalWrapMode.Overflow;

                // Sign Button (Bottom-right, occupies 40% width)
                Text btnNego = CreateButtonHelper(row.transform, "BtnNegoSponsor", "PAZARLIK ET", colorGold, new Color(11f/255f, 12f/255f, 16f/255f, 1f), () => {
                    ShowSponsorNegotiation(p, offer, detailsModal, overlay);
                });
                SetRectTransform(btnNego.transform.parent, new Vector2(0.56f, 0.08f), new Vector2(0.96f, 0.42f), Vector2.zero, Vector2.zero);
                btnNego.resizeTextForBestFit = false;
                btnNego.horizontalOverflow = HorizontalWrapMode.Overflow;
                btnNego.verticalOverflow = VerticalWrapMode.Overflow;
                btnNego.fontSize = 44;
            }
        }

        private void ShowSponsorNegotiation(Player p, Sponsor offer, GameObject detailsModal, GameObject listOverlay)
        {
            var (content, overlay, cardObj) = CreateScrollableNegotiationCard(detailsModal.transform, p, $"{offer.BrandName} Görüşmesi", null);

            int proposedYears = offer.DurationYears;
            int proposedComm = 10; // 10% commission default
            int proposedBonus = 0; // signing bonus default

            Text yearsValText = null;
            Text commValText = null;
            Text bonusValText = null;

            // 1. Years row
            GameObject yearsRow = new GameObject("YearsRow", typeof(RectTransform));
            yearsRow.transform.SetParent(content, false);
            LayoutElement yearsLe = yearsRow.AddComponent<LayoutElement>();
            yearsLe.preferredHeight = 180f;

            Text lblYears = CreateText(yearsRow.transform, "LblYears", "Sözleşme Süresi:", 58, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(lblYears, new Vector2(0.02f, 0f), new Vector2(0.38f, 1f), Vector2.zero, Vector2.zero);
            lblYears.horizontalOverflow = HorizontalWrapMode.Wrap;
            lblYears.verticalOverflow = VerticalWrapMode.Overflow;

            yearsValText = CreateText(yearsRow.transform, "YearsValText", BehindTheScenesFootball.Managers.LocalizationManager.Translate($"{proposedYears} Yıl"), 64, colorAccent, TextAnchor.MiddleRight);
            SetRectTransform(yearsValText, new Vector2(0.39f, 0f), new Vector2(0.72f, 1f), Vector2.zero, Vector2.zero);
            yearsValText.fontStyle = FontStyle.Bold;
            yearsValText.horizontalOverflow = HorizontalWrapMode.Overflow;
            yearsValText.verticalOverflow = VerticalWrapMode.Overflow;

            Text btnYearsMinus = CreateButtonHelper(yearsRow.transform, "BtnYearsMinus", "-", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                proposedYears = Mathf.Max(1, proposedYears - 1);
                yearsValText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"{proposedYears} Yıl");
            });
            SetRectTransform(btnYearsMinus.transform.parent, new Vector2(0.74f, 0.1f), new Vector2(0.85f, 0.9f), Vector2.zero, Vector2.zero);
            btnYearsMinus.fontSize = 54;

            Text btnYearsPlus = CreateButtonHelper(yearsRow.transform, "BtnYearsPlus", "+", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                proposedYears = Mathf.Min(5, proposedYears + 1);
                yearsValText.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"{proposedYears} Yıl");
            });
            SetRectTransform(btnYearsPlus.transform.parent, new Vector2(0.87f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero, Vector2.zero);
            btnYearsPlus.fontSize = 54;

            // 2. Commission row
            GameObject commRow = new GameObject("CommRow", typeof(RectTransform));
            commRow.transform.SetParent(content, false);
            LayoutElement commLe = commRow.AddComponent<LayoutElement>();
            commLe.preferredHeight = 180f;

            Text lblComm = CreateText(commRow.transform, "LblComm", "Ajans Komisyonu:", 58, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(lblComm, new Vector2(0.02f, 0f), new Vector2(0.38f, 1f), Vector2.zero, Vector2.zero);
            lblComm.horizontalOverflow = HorizontalWrapMode.Wrap;
            lblComm.verticalOverflow = VerticalWrapMode.Overflow;

            commValText = CreateText(commRow.transform, "CommValText", $"%{proposedComm}", 64, colorAccent, TextAnchor.MiddleRight);
            SetRectTransform(commValText, new Vector2(0.39f, 0f), new Vector2(0.72f, 1f), Vector2.zero, Vector2.zero);
            commValText.fontStyle = FontStyle.Bold;
            commValText.horizontalOverflow = HorizontalWrapMode.Overflow;
            commValText.verticalOverflow = VerticalWrapMode.Overflow;

            Text btnCommMinus = CreateButtonHelper(commRow.transform, "BtnCommMinus", "-", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                proposedComm = Mathf.Max(5, proposedComm - 1);
                commValText.text = $"%{proposedComm}";
            });
            SetRectTransform(btnCommMinus.transform.parent, new Vector2(0.74f, 0.1f), new Vector2(0.85f, 0.9f), Vector2.zero, Vector2.zero);
            btnCommMinus.fontSize = 54;

            Text btnCommPlus = CreateButtonHelper(commRow.transform, "BtnCommPlus", "+", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                proposedComm = Mathf.Min(25, proposedComm + 1);
                commValText.text = $"%{proposedComm}";
            });
            SetRectTransform(btnCommPlus.transform.parent, new Vector2(0.87f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero, Vector2.zero);
            btnCommPlus.fontSize = 54;

            // 3. Signing Bonus row
            GameObject bonusRow = new GameObject("BonusRow", typeof(RectTransform));
            bonusRow.transform.SetParent(content, false);
            LayoutElement bonusLe = bonusRow.AddComponent<LayoutElement>();
            bonusLe.preferredHeight = 180f;

            Text lblBonus = CreateText(bonusRow.transform, "LblBonus", "İmza Primi (Bonus):", 58, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(lblBonus, new Vector2(0.02f, 0f), new Vector2(0.38f, 1f), Vector2.zero, Vector2.zero);
            lblBonus.horizontalOverflow = HorizontalWrapMode.Wrap;
            lblBonus.verticalOverflow = VerticalWrapMode.Overflow;

            bonusValText = CreateText(bonusRow.transform, "BonusValText", $"€{proposedBonus:N0}", 64, colorAccent, TextAnchor.MiddleRight);
            SetRectTransform(bonusValText, new Vector2(0.39f, 0f), new Vector2(0.72f, 1f), Vector2.zero, Vector2.zero);
            bonusValText.fontStyle = FontStyle.Bold;
            bonusValText.horizontalOverflow = HorizontalWrapMode.Overflow;
            bonusValText.verticalOverflow = VerticalWrapMode.Overflow;

            Text btnBonusMinus = CreateButtonHelper(bonusRow.transform, "BtnBonusMinus", "-", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                proposedBonus = Mathf.Max(0, proposedBonus - 500);
                bonusValText.text = $"€{proposedBonus:N0}";
            });
            SetRectTransform(btnBonusMinus.transform.parent, new Vector2(0.74f, 0.1f), new Vector2(0.85f, 0.9f), Vector2.zero, Vector2.zero);
            btnBonusMinus.fontSize = 54;

            Text btnBonusPlus = CreateButtonHelper(bonusRow.transform, "BtnBonusPlus", "+", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                proposedBonus = Mathf.Min(100000, proposedBonus + 500);
                bonusValText.text = $"€{proposedBonus:N0}";
            });
            SetRectTransform(btnBonusPlus.transform.parent, new Vector2(0.87f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero, Vector2.zero);

            // Feedback box
            GameObject feedbackPanel = CreatePanelHelper(content, "FeedbackPanel", new Color(0f, 0f, 0f, 0.25f));
            LayoutElement fbLe = feedbackPanel.AddComponent<LayoutElement>();
            fbLe.preferredHeight = 320f;

            Text feedbackTxt = CreateText(feedbackPanel.transform, "FeedbackTxt", "Sponsor yetkilileri taleplerinizi bekliyor.", 56, new Color(0.8f, 0.85f, 0.9f), TextAnchor.MiddleCenter);
            SetRectTransform(feedbackTxt, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero);
            feedbackTxt.resizeTextForBestFit = false;
            feedbackTxt.resizeTextMinSize = 14;
            feedbackTxt.resizeTextMaxSize = 68;

            // Submit Button
            GameObject submitContainer = new GameObject("SubmitContainer", typeof(RectTransform));
            submitContainer.transform.SetParent(content, false);
            LayoutElement subLe = submitContainer.AddComponent<LayoutElement>();
            subLe.preferredHeight = 120f;

            Text btnSubmit = CreateButtonHelper(submitContainer.transform, "BtnSubmitNego", "ANLAŞMAYI İMZALA", colorGreen, new Color(11f/255f, 12f/255f, 16f/255f, 1f), () => {
                float maxComm = 0.22f - (p.OVR - 50) * 0.0015f;
                maxComm = Mathf.Clamp(maxComm, 0.08f, 0.20f);
                float proposedCommPct = (float)proposedComm / 100f;

                if (proposedCommPct > maxComm)
                {
                    feedbackTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"<color=#E74C3C>{offer.BrandName} Temsilcisi: 'Talep ettiğiniz %{proposedComm} komisyon oranı bizim için çok fazla. En fazla %{maxComm * 100:0.0} kabul edebiliriz!'</color>");
                    p.Happiness = Mathf.Clamp(p.Happiness - 3f, 10f, 100f);
                    return;
                }

                int maxBonus = Mathf.RoundToInt(offer.WeeklyIncome * 6f * (p.OVR / 65f));
                if (proposedBonus > maxBonus)
                {
                    feedbackTxt.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate($"<color=#E74C3C>{offer.BrandName} Temsilcisi: 'İmza primi talebiniz ({proposedBonus:N0}) marka bütçemizi aşıyor! Maksimum €{maxBonus:N0} prim ödeyebiliriz.'</color>");
                    p.Happiness = Mathf.Clamp(p.Happiness - 2f, 10f, 100f);
                    return;
                }

                // Sign sponsor
                p.PendingSponsorOffers.Remove(offer);
                p.ActiveSponsor = new Sponsor(offer.BrandName, offer.WeeklyIncome, proposedYears, offer.MinOVRRequired);
                p.CustomSponsorCommissionPercent = proposedCommPct;
                AgencyManager.Instance.ActiveAgency.Balance += proposedBonus;

                AgencyManager.Instance.LogActivity($"SPONSORLUK: Müşteriniz {p.Name}, {offer.BrandName} ile {proposedYears} yıllık sponsorluk imzaladı! Ajans kasasına €{proposedBonus:N0} imza primi eklendi (Ajans Komisyonu: %{proposedComm}).");

                Destroy(overlay);
                if (listOverlay != null) Destroy(listOverlay);
                Destroy(detailsModal);
                if (activeSubpanel != null) activeSubpanel.Refresh();
            });
            SetRectTransform(btnSubmit.transform.parent, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.95f), Vector2.zero, Vector2.zero);
        }

        #endregion

        #region Main Menu, Pause Menu & Save/Load System

        private GameObject mainMenuObj;
        private GameObject menuButtonsPanel;
        private GameObject newGameSetupPanel;
        private GameObject settingsPanel;
        private GameObject pauseMenuObj;

        public void ShowWelcomeMenu()
        {
            if (activeSubpanel != null)
            {
                activeSubpanel.Close();
                activeSubpanel = null;
            }

            subpanelContainerObj.SetActive(false);
            homeScreenObj.SetActive(false);
            if (mainMenuObj != null)
            {
                mainMenuObj.SetActive(true);
                menuButtonsPanel.SetActive(true);
                newGameSetupPanel.SetActive(false);
                settingsPanel.SetActive(false);
            }
        }

        [System.Serializable]
        public struct ClubSaveData
        {
            public string Name;
            public int Played;
            public int Wins;
            public int Draws;
            public int Losses;
            public int GF;
            public int GA;
        }

        [System.Serializable]
        public class GameSaveData
        {
            public string AgencyJson;
            public int CurrentWeek;
            public List<ClubSaveData> ClubStandings;
        }

        private InputField CreateInputFieldHelper(Transform parent, string name, string placeholderText, out GameObject fieldObj)
        {
            fieldObj = CreatePanelHelper(parent, name, new Color(0.12f, 0.16f, 0.22f, 0.95f));
            
            Outline border = fieldObj.AddComponent<Outline>();
            border.effectColor = colorAccent;
            border.effectDistance = new Vector2(2f, 2f);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(fieldObj.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.font = defaultFont;
            text.color = Color.white;
            text.fontSize = 42;
            text.alignment = TextAnchor.MiddleLeft;
            SetRectTransform(text, Vector2.zero, Vector2.one, new Vector2(20f, 10f), new Vector2(-20f, -10f));
            text.gameObject.AddComponent<TextScaler>();

            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(fieldObj.transform, false);
            Text placeholder = placeholderObj.AddComponent<Text>();
            var localizable = placeholderObj.AddComponent<BehindTheScenesFootball.Managers.LocalizableText>();
            localizable.originalText = placeholderText;
            placeholder.font = defaultFont;
            placeholder.color = new Color(0.7f, 0.75f, 0.8f, 0.5f);
            placeholder.fontSize = 42;
            placeholder.text = BehindTheScenesFootball.Managers.LocalizationManager.Translate(placeholderText);
            placeholder.alignment = TextAnchor.MiddleLeft;
            SetRectTransform(placeholder, Vector2.zero, Vector2.one, new Vector2(20f, 10f), new Vector2(-20f, -10f));
            placeholder.gameObject.AddComponent<TextScaler>();

            InputField inputField = fieldObj.AddComponent<InputField>();
            inputField.textComponent = text;
            inputField.placeholder = placeholder;
            inputField.transition = Selectable.Transition.None;

            return inputField;
        }

        private void CreateMainMenuScreen(Transform parent)
        {
            mainMenuObj = CreatePanelHelper(parent, "MainMenuScreen", Color.clear);
            SetRectTransform(mainMenuObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // 1. Menu Buttons Panel (Center of screen)
            menuButtonsPanel = CreatePanelHelper(mainMenuObj.transform, "MenuButtonsPanel", Color.clear);
            SetRectTransform(menuButtonsPanel, new Vector2(0.15f, 0.20f), new Vector2(0.85f, 0.80f), Vector2.zero, Vector2.zero);

            // Title inside Main Menu
            Text gameTitleText = CreateText(menuButtonsPanel.transform, "GameTitleText", "PERDE ARKASI FUTBOL", 72, colorAccent, TextAnchor.MiddleCenter);
            SetRectTransform(gameTitleText, new Vector2(0f, 0.80f), new Vector2(1f, 0.95f), Vector2.zero, Vector2.zero);
            gameTitleText.fontStyle = FontStyle.Bold;

            Text btnNewGame = CreateButtonHelper(menuButtonsPanel.transform, "BtnNewGame", "YENİ OYUN", colorGreen, Color.white, () => ShowNewGameSetup());
            SetRectTransform(btnNewGame.transform.parent, new Vector2(0f, 0.58f), new Vector2(1f, 0.72f), Vector2.zero, Vector2.zero);
            btnNewGame.fontSize = 48;
            btnNewGame.fontStyle = FontStyle.Bold;

            Text btnLoadGame = CreateButtonHelper(menuButtonsPanel.transform, "BtnLoadGame", "KAYITLI OYUN YÜKLE", colorGreyButton, Color.white, () => {
                ShowSaveSlotsPopup(false);
            });
            SetRectTransform(btnLoadGame.transform.parent, new Vector2(0f, 0.40f), new Vector2(1f, 0.54f), Vector2.zero, Vector2.zero);
            btnLoadGame.fontSize = 48;
            btnLoadGame.fontStyle = FontStyle.Bold;

            Text btnSettings = CreateButtonHelper(menuButtonsPanel.transform, "BtnSettings", "AYARLAR", colorGreyButton, Color.white, () => ShowSettingsScreen());
            SetRectTransform(btnSettings.transform.parent, new Vector2(0f, 0.22f), new Vector2(1f, 0.36f), Vector2.zero, Vector2.zero);
            btnSettings.fontSize = 48;
            btnSettings.fontStyle = FontStyle.Bold;

            Text btnExit = CreateButtonHelper(menuButtonsPanel.transform, "BtnExit", "ÇIKIŞ", colorRed, Color.white, () => {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
            SetRectTransform(btnExit.transform.parent, new Vector2(0f, 0.04f), new Vector2(1f, 0.18f), Vector2.zero, Vector2.zero);
            btnExit.fontSize = 48;
            btnExit.fontStyle = FontStyle.Bold;

            // 2. New Game Setup Panel (Initially hidden)
            newGameSetupPanel = CreatePanelHelper(mainMenuObj.transform, "NewGameSetupPanel", Color.clear);
            SetRectTransform(newGameSetupPanel, new Vector2(0.15f, 0.20f), new Vector2(0.85f, 0.80f), Vector2.zero, Vector2.zero);
            newGameSetupPanel.SetActive(false);

            Text setupTitleText = CreateText(newGameSetupPanel.transform, "SetupTitleText", "MENAJERLİK KURULUMU", 60, Color.white, TextAnchor.MiddleCenter);
            SetRectTransform(setupTitleText, new Vector2(0f, 0.82f), new Vector2(1f, 0.95f), Vector2.zero, Vector2.zero);
            setupTitleText.fontStyle = FontStyle.Bold;

            // Name Input
            Text nameLabel = CreateText(newGameSetupPanel.transform, "NameLabel", "AD SOYAD", 36, colorAccent, TextAnchor.MiddleLeft);
            SetRectTransform(nameLabel, new Vector2(0f, 0.70f), new Vector2(1f, 0.76f), Vector2.zero, Vector2.zero);
            nameLabel.fontStyle = FontStyle.Bold;

            GameObject nameFieldObj;
            InputField nameInput = CreateInputFieldHelper(newGameSetupPanel.transform, "InputName", "Ad Soyad girin...", out nameFieldObj);
            nameInput.characterLimit = 15; // Karakter limitini 15 yap (Sağ üst panel taşmalarını önlemek için)
            SetRectTransform(nameFieldObj, new Vector2(0f, 0.56f), new Vector2(1f, 0.68f), Vector2.zero, Vector2.zero);

            // Company Input
            Text companyLabel = CreateText(newGameSetupPanel.transform, "CompanyLabel", "ŞİRKET / AJANS İSMİ", 36, colorAccent, TextAnchor.MiddleLeft);
            SetRectTransform(companyLabel, new Vector2(0f, 0.44f), new Vector2(1f, 0.50f), Vector2.zero, Vector2.zero);
            companyLabel.fontStyle = FontStyle.Bold;

            GameObject companyFieldObj;
            InputField companyInput = CreateInputFieldHelper(newGameSetupPanel.transform, "InputCompany", "Şirket ismini girin...", out companyFieldObj);
            companyInput.characterLimit = 20; // Karakter limitini 20 yap (Sağ üst panel taşmalarını önlemek için)
            SetRectTransform(companyFieldObj, new Vector2(0f, 0.30f), new Vector2(1f, 0.42f), Vector2.zero, Vector2.zero);

            // Action Buttons for Setup
            Text btnSetupStart = CreateButtonHelper(newGameSetupPanel.transform, "BtnSetupStart", "OYUNA BAŞLA ▶", colorGreen, Color.white, () => {
                string trimmedName = nameInput.text.Trim();
                string trimmedCompany = companyInput.text.Trim();

                if (string.IsNullOrEmpty(trimmedName) || string.IsNullOrEmpty(trimmedCompany))
                {
                    ShowFeedbackPopup("HATA: Lütfen ad soyad ve şirket ismi alanlarını boş bırakmayın! Her iki alana da en az 1 karakter girilmelidir.");
                    return;
                }

                // Her iki girdide de en az 1 adet harf bulunması kontrolü
                bool nameHasLetter = false;
                foreach (char c in trimmedName) { if (char.IsLetter(c)) { nameHasLetter = true; break; } }
                bool companyHasLetter = false;
                foreach (char c in trimmedCompany) { if (char.IsLetter(c)) { companyHasLetter = true; break; } }

                if (!nameHasLetter || !companyHasLetter)
                {
                    ShowFeedbackPopup("HATA: Ad soyad ve şirket/ajans ismi en az 1 adet harf içermelidir!");
                    return;
                }

                string mName = trimmedName;
                string cName = trimmedCompany;
                
                // Reset simulation and database to start completely fresh!
                DatabaseManager.Instance.ResetDatabase();
                SimulationEngine.Instance.ResetSimulation();
                
                AgencyManager.Instance.InitializeAgency(cName, mName, AgencyManager.Instance.StartingBalance);
                
                mainMenuObj.SetActive(false);
                homeScreenObj.SetActive(true);
                RefreshUI();
            });
            SetRectTransform(btnSetupStart.transform.parent, new Vector2(0f, 0.12f), new Vector2(1f, 0.24f), Vector2.zero, Vector2.zero);
            btnSetupStart.fontSize = 44;
            btnSetupStart.fontStyle = FontStyle.Bold;

            Text btnSetupBack = CreateButtonHelper(newGameSetupPanel.transform, "BtnSetupBack", "GERİ DÖN", colorRed, Color.white, () => {
                newGameSetupPanel.SetActive(false);
                menuButtonsPanel.SetActive(true);
            });
            SetRectTransform(btnSetupBack.transform.parent, new Vector2(0f, 0.00f), new Vector2(1f, 0.09f), Vector2.zero, Vector2.zero);
            btnSetupBack.fontSize = 36;
            btnSetupBack.fontStyle = FontStyle.Bold;

            // 3. Settings Panel (Initially hidden full-screen overlay)
            settingsPanel = CreatePanelHelper(mainCanvas.transform, "SettingsPanel", new Color(0f, 0f, 0f, 0.85f));
            SetRectTransform(settingsPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            settingsPanel.SetActive(false);

            // Centered Settings Card
            GameObject settingsCard = CreatePanelHelper(settingsPanel.transform, "SettingsCard", new Color(0.12f, 0.16f, 0.22f, 0.95f));
            SetRectTransform(settingsCard, new Vector2(0.15f, 0.15f), new Vector2(0.85f, 0.85f), Vector2.zero, Vector2.zero);

            Outline cardBorder = settingsCard.AddComponent<Outline>();
            cardBorder.effectColor = colorAccent;
            cardBorder.effectDistance = new Vector2(2f, 2f);

            Text settingsTitleText = CreateText(settingsCard.transform, "SettingsTitleText", "AYARLAR", 60, Color.white, TextAnchor.MiddleCenter);
            SetRectTransform(settingsTitleText, new Vector2(0f, 0.85f), new Vector2(1f, 0.95f), Vector2.zero, Vector2.zero);
            settingsTitleText.fontStyle = FontStyle.Bold;

            // Müzik Aç/Kapa Etiketi ve Butonu
            Text musicToggleLabel = CreateText(settingsCard.transform, "MusicToggleLabel", "MÜZİK AÇ / KAPAT:", 44, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(musicToggleLabel, new Vector2(0.05f, 0.65f), new Vector2(0.55f, 0.77f), Vector2.zero, Vector2.zero);
            musicToggleLabel.fontStyle = FontStyle.Bold;

            musicToggleBtnLabel = CreateButtonHelper(settingsCard.transform, "BtnMusicToggle", "AÇIK", colorGreen, Color.white, () => {
                var audioMgr = BehindTheScenesFootball.Managers.AudioManager.Instance;
                audioMgr.SetMusicEnabled(!audioMgr.IsMusicEnabled);
                UpdateSettingsUI();
            });
            SetRectTransform(musicToggleBtnLabel.transform.parent, new Vector2(0.60f, 0.65f), new Vector2(0.95f, 0.77f), Vector2.zero, Vector2.zero);
            musicToggleBtnLabel.fontSize = 40;
            musicToggleBtnLabel.fontStyle = FontStyle.Bold;

            // Müzik Ses Seviyesi Etiketi ve Kontrolleri
            Text musicVolumeLabel = CreateText(settingsCard.transform, "MusicVolumeLabel", "MÜZİK SES SEVİYESİ:", 44, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(musicVolumeLabel, new Vector2(0.05f, 0.45f), new Vector2(0.55f, 0.57f), Vector2.zero, Vector2.zero);
            musicVolumeLabel.fontStyle = FontStyle.Bold;

            // Volume adjustment container
            GameObject volContainer = CreatePanelHelper(settingsCard.transform, "VolumeContainer", Color.clear);
            SetRectTransform(volContainer, new Vector2(0.60f, 0.45f), new Vector2(0.95f, 0.57f), Vector2.zero, Vector2.zero);

            Text btnVolMinus = CreateButtonHelper(volContainer.transform, "BtnVolMinus", "-", colorGreyButton, Color.white, () => {
                var audioMgr = BehindTheScenesFootball.Managers.AudioManager.Instance;
                audioMgr.SetVolume(audioMgr.MusicVolume - 0.1f);
                UpdateSettingsUI();
            });
            SetRectTransform(btnVolMinus.transform.parent, new Vector2(0f, 0f), new Vector2(0.28f, 1f), Vector2.zero, Vector2.zero);
            btnVolMinus.fontSize = 44;
            btnVolMinus.fontStyle = FontStyle.Bold;

            musicVolumeValText = CreateText(volContainer.transform, "MusicVolumeValText", "% 50", 40, Color.white, TextAnchor.MiddleCenter);
            SetRectTransform(musicVolumeValText, new Vector2(0.32f, 0f), new Vector2(0.68f, 1f), Vector2.zero, Vector2.zero);
            musicVolumeValText.fontStyle = FontStyle.Bold;

            Text btnVolPlus = CreateButtonHelper(volContainer.transform, "BtnVolPlus", "+", colorGreyButton, Color.white, () => {
                var audioMgr = BehindTheScenesFootball.Managers.AudioManager.Instance;
                audioMgr.SetVolume(audioMgr.MusicVolume + 0.1f);
                UpdateSettingsUI();
            });
            SetRectTransform(btnVolPlus.transform.parent, new Vector2(0.72f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            btnVolPlus.fontSize = 44;
            btnVolPlus.fontStyle = FontStyle.Bold;

            // Dil Seçimi (Language Settings)
            Text langLabel = CreateText(settingsCard.transform, "LangLabel", "DİL / LANGUAGE:", 44, Color.white, TextAnchor.MiddleLeft);
            SetRectTransform(langLabel, new Vector2(0.05f, 0.25f), new Vector2(0.45f, 0.37f), Vector2.zero, Vector2.zero);
            langLabel.fontStyle = FontStyle.Bold;

            GameObject langContainer = CreatePanelHelper(settingsCard.transform, "LangContainer", Color.clear);
            SetRectTransform(langContainer, new Vector2(0.50f, 0.25f), new Vector2(0.95f, 0.37f), Vector2.zero, Vector2.zero);

            langTrBtnLabel = CreateButtonHelper(langContainer.transform, "BtnLangTr", "TÜRKÇE", colorGreyButton, Color.white, () => {
                BehindTheScenesFootball.Managers.LocalizationManager.CurrentLanguage = "TR";
                UpdateSettingsUI();
                UpdateAllLanguages();
                RefreshUI();
            });
            SetRectTransform(langTrBtnLabel.transform.parent, new Vector2(0f, 0f), new Vector2(0.48f, 1f), Vector2.zero, Vector2.zero);
            langTrBtnLabel.fontSize = 44;
            langTrBtnLabel.fontStyle = FontStyle.Bold;

            langEnBtnLabel = CreateButtonHelper(langContainer.transform, "BtnLangEn", "ENGLISH", colorGreyButton, Color.white, () => {
                BehindTheScenesFootball.Managers.LocalizationManager.CurrentLanguage = "EN";
                UpdateSettingsUI();
                UpdateAllLanguages();
                RefreshUI();
            });
            SetRectTransform(langEnBtnLabel.transform.parent, new Vector2(0.52f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            langEnBtnLabel.fontSize = 44;
            langEnBtnLabel.fontStyle = FontStyle.Bold;
            Text btnSettingsBack = CreateButtonHelper(settingsCard.transform, "BtnSettingsBack", "GERİ DÖN", colorRed, Color.white, () => {
                settingsPanel.SetActive(false);
                if (mainMenuObj != null && mainMenuObj.activeSelf)
                {
                    menuButtonsPanel.SetActive(true);
                }
                else
                {
                    TogglePauseMenu(true);
                }
            });
            SetRectTransform(btnSettingsBack.transform.parent, new Vector2(0f, 0.05f), new Vector2(1f, 0.15f), Vector2.zero, Vector2.zero);
            btnSettingsBack.fontSize = 44;
            btnSettingsBack.fontStyle = FontStyle.Bold;
        }

        private void ShowNewGameSetup()
        {
            menuButtonsPanel.SetActive(false);
            newGameSetupPanel.SetActive(true);
        }

        private void ShowSettingsScreen()
        {
            menuButtonsPanel.SetActive(false);
            settingsPanel.SetActive(true);
            UpdateSettingsUI();
        }

        private void UpdateSettingsUI()
        {
            if (musicToggleBtnLabel != null && musicVolumeValText != null)
            {
                var audioMgr = BehindTheScenesFootball.Managers.AudioManager.Instance;
                bool isEnabled = audioMgr.IsMusicEnabled;
                musicToggleBtnLabel.text = BehindTheScenesFootball.Managers.LocalizationManager.T(isEnabled ? "AÇIK" : "KAPALI");

                Image img = musicToggleBtnLabel.transform.parent.GetComponent<Image>();
                if (img != null)
                {
                    img.color = isEnabled ? colorGreen : colorRed;
                }

                musicVolumeValText.text = $"% {Mathf.RoundToInt(audioMgr.MusicVolume * 100f)}";
            }

            if (langTrBtnLabel != null && langEnBtnLabel != null)
            {
                bool isTr = BehindTheScenesFootball.Managers.LocalizationManager.CurrentLanguage == "TR";
                
                Image trImg = langTrBtnLabel.transform.parent.GetComponent<Image>();
                if (trImg != null) trImg.color = isTr ? colorGreen : colorGreyButton;
                
                Image enImg = langEnBtnLabel.transform.parent.GetComponent<Image>();
                if (enImg != null) enImg.color = !isTr ? colorGreen : colorGreyButton;
            }
        }

        private void UpdateAllLanguages()
        {
            var localizables = mainCanvas.GetComponentsInChildren<BehindTheScenesFootball.Managers.LocalizableText>(true);
            foreach (var lt in localizables)
            {
                lt.UpdateLanguage();
            }
        }

        public void ShowFeedbackPopup(string message)
        {
            GameObject popup = CreatePanelHelper(mainCanvas.transform, "FeedbackPopup", new Color(11f/255f, 12f/255f, 16f/255f, 0.95f));
            SetRectTransform(popup, new Vector2(0.20f, 0.40f), new Vector2(0.80f, 0.60f), Vector2.zero, Vector2.zero);

            Outline border = popup.AddComponent<Outline>();
            border.effectColor = colorAccent;
            border.effectDistance = new Vector2(2f, 2f);

            Text text = CreateText(popup.transform, "Text", message, 44, Color.white, TextAnchor.MiddleCenter);
            SetRectTransform(text, new Vector2(0.05f, 0.35f), new Vector2(0.95f, 0.90f), Vector2.zero, Vector2.zero);
            text.fontStyle = FontStyle.Bold;

            Text btnOk = CreateButtonHelper(popup.transform, "BtnOk", "TAMAM", colorGreen, Color.white, () => Destroy(popup));
            SetRectTransform(btnOk.transform.parent, new Vector2(0.35f, 0.08f), new Vector2(0.65f, 0.30f), Vector2.zero, Vector2.zero);
            btnOk.fontSize = 32;
            btnOk.fontStyle = FontStyle.Bold;
        }

        public void TogglePauseMenu(bool show)
        {
            if (show)
            {
                if (pauseMenuObj != null) Destroy(pauseMenuObj);

                pauseMenuObj = CreatePanelHelper(mainCanvas.transform, "PauseMenu", new Color(0f, 0f, 0f, 0.85f));
                SetRectTransform(pauseMenuObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

                // Pause Card - Dikey boyutu 6 buton sığacak şekilde genişletildi (0.23f/0.77f -> 0.15f/0.85f)
                GameObject card = CreatePanelHelper(pauseMenuObj.transform, "PauseCard", new Color(0.12f, 0.16f, 0.22f, 0.95f));
                SetRectTransform(card, new Vector2(0.20f, 0.15f), new Vector2(0.80f, 0.85f), Vector2.zero, Vector2.zero);

                Outline border = card.AddComponent<Outline>();
                border.effectColor = colorAccent;
                border.effectDistance = new Vector2(2f, 2f);

                Text title = CreateText(card.transform, "Title", "OYUN DURAKLATILDI", 60, Color.white, TextAnchor.MiddleCenter);
                SetRectTransform(title, new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.96f), Vector2.zero, Vector2.zero);
                title.fontStyle = FontStyle.Bold;

                Text btnContinue = CreateButtonHelper(card.transform, "BtnContinue", "OYUNA DEVAM ET", colorGreen, Color.white, () => TogglePauseMenu(false));
                SetRectTransform(btnContinue.transform.parent, new Vector2(0.1f, 0.75f), new Vector2(0.9f, 0.84f), Vector2.zero, Vector2.zero);
                btnContinue.fontSize = 42;
                btnContinue.fontStyle = FontStyle.Bold;

                Text btnSave = CreateButtonHelper(card.transform, "BtnSave", "OYUNU KAYDET", colorGreyButton, Color.white, () => {
                    ShowSaveSlotsPopup(true);
                });
                SetRectTransform(btnSave.transform.parent, new Vector2(0.1f, 0.63f), new Vector2(0.9f, 0.72f), Vector2.zero, Vector2.zero);
                btnSave.fontSize = 42;
                btnSave.fontStyle = FontStyle.Bold;

                Text btnLoad = CreateButtonHelper(card.transform, "BtnLoad", "KAYITLI OYUN AÇ", colorGreyButton, Color.white, () => {
                    ShowSaveSlotsPopup(false);
                });
                SetRectTransform(btnLoad.transform.parent, new Vector2(0.1f, 0.51f), new Vector2(0.9f, 0.60f), Vector2.zero, Vector2.zero);
                btnLoad.fontSize = 42;
                btnLoad.fontStyle = FontStyle.Bold;

                // Settings Button (AYARLAR) - Pause menüsüne eklendi
                Text btnSettings = CreateButtonHelper(card.transform, "BtnSettings", "AYARLAR", colorGreyButton, Color.white, () => {
                    TogglePauseMenu(false);
                    ShowSettingsScreen();
                });
                SetRectTransform(btnSettings.transform.parent, new Vector2(0.1f, 0.39f), new Vector2(0.9f, 0.48f), Vector2.zero, Vector2.zero);
                btnSettings.fontSize = 42;
                btnSettings.fontStyle = FontStyle.Bold;

                Text btnMainMenu = CreateButtonHelper(card.transform, "BtnMainMenu", "ANA MENÜYE DÖN", colorGreyButton, Color.white, () => {
                    TogglePauseMenu(false);
                    if (activeSubpanel != null)
                    {
                        activeSubpanel.Close();
                        activeSubpanel = null;
                    }
                    subpanelContainerObj.SetActive(false);
                    homeScreenObj.SetActive(false);
                    mainMenuObj.SetActive(true);
                    menuButtonsPanel.SetActive(true);
                    newGameSetupPanel.SetActive(false);
                    settingsPanel.SetActive(false);
                });
                SetRectTransform(btnMainMenu.transform.parent, new Vector2(0.1f, 0.27f), new Vector2(0.9f, 0.36f), Vector2.zero, Vector2.zero);
                btnMainMenu.fontSize = 42;
                btnMainMenu.fontStyle = FontStyle.Bold;

                Text btnExit = CreateButtonHelper(card.transform, "BtnExit", "ÇIKIŞ", colorRed, Color.white, () => {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                });
                SetRectTransform(btnExit.transform.parent, new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.24f), Vector2.zero, Vector2.zero);
                btnExit.fontSize = 42;
                btnExit.fontStyle = FontStyle.Bold;
            }
            else
            {
                if (pauseMenuObj != null)
                {
                    Destroy(pauseMenuObj);
                    pauseMenuObj = null;
                }
            }
        }

        public void SaveGame(int slotIndex)
        {
            if (AgencyManager.Instance == null || AgencyManager.Instance.ActiveAgency == null || SimulationEngine.Instance == null) return;

            var agency = AgencyManager.Instance.ActiveAgency;
            
            // Sync client IDs and detailed client save data
            agency.ClientPlayerIds.Clear();
            agency.SavedClients.Clear();
            foreach (var client in agency.Clients)
            {
                if (client != null)
                {
                    if (!string.IsNullOrEmpty(client.Id))
                    {
                        agency.ClientPlayerIds.Add(client.Id);
                    }

                    ClientSaveData csd = new ClientSaveData
                    {
                        PlayerId = client.Id,
                        PlayerName = client.Name,
                        CustomTransferCommissionPercent = client.CustomTransferCommissionPercent,
                        CustomWageCommissionPercent = client.CustomWageCommissionPercent,
                        CustomSponsorCommissionPercent = client.CustomSponsorCommissionPercent,
                        AgencyContractRemainingWeeks = client.AgencyContractRemainingWeeks
                    };
                    agency.SavedClients.Add(csd);
                }
            }

            GameSaveData saveData = new GameSaveData();
            saveData.AgencyJson = JsonUtility.ToJson(agency);
            saveData.CurrentWeek = SimulationEngine.Instance.CurrentWeek;
            saveData.ClubStandings = new List<ClubSaveData>();

            // Save standings
            foreach (var club in DatabaseManager.Instance.Clubs)
            {
                ClubSaveData cData = new ClubSaveData();
                cData.Name = club.OriginalName;
                cData.Played = club.StandingPlayed;
                cData.Wins = club.StandingWins;
                cData.Draws = club.StandingDraws;
                cData.Losses = club.StandingLosses;
                cData.GF = club.StandingGF;
                cData.GA = club.StandingGA;
                saveData.ClubStandings.Add(cData);
            }

            string key = "SaveGame_Slot_" + slotIndex;
            PlayerPrefs.SetString(key, JsonUtility.ToJson(saveData));
            PlayerPrefs.Save();
            Debug.Log($"Game Saved to Slot {slotIndex}!");
        }

        public bool LoadGame(int slotIndex)
        {
            string key = "SaveGame_Slot_" + slotIndex;
            if (!PlayerPrefs.HasKey(key)) return false;

            string json = PlayerPrefs.GetString(key);
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
            if (saveData == null) return false;

            // Restore Agency
            Agency loadedAgency = JsonUtility.FromJson<Agency>(saveData.AgencyJson);
            if (loadedAgency == null) return false;

            AgencyManager.Instance.ActiveAgency = loadedAgency;

            // Restore Clients
            loadedAgency.Clients.Clear();
            if (loadedAgency.SavedClients != null && loadedAgency.SavedClients.Count > 0)
            {
                foreach (var csd in loadedAgency.SavedClients)
                {
                    if (csd == null) continue;
                    Player p = DatabaseManager.Instance.Players.Find(x => x.Id == csd.PlayerId);
                    if (p == null && !string.IsNullOrEmpty(csd.PlayerName))
                    {
                        p = DatabaseManager.Instance.Players.Find(x => x.Name.Equals(csd.PlayerName, System.StringComparison.OrdinalIgnoreCase));
                    }
                    if (p != null)
                    {
                        p.CustomTransferCommissionPercent = csd.CustomTransferCommissionPercent;
                        p.CustomWageCommissionPercent = csd.CustomWageCommissionPercent;
                        p.CustomSponsorCommissionPercent = csd.CustomSponsorCommissionPercent;
                        p.AgencyContractRemainingWeeks = csd.AgencyContractRemainingWeeks;
                        loadedAgency.AddClient(p);
                    }
                }
            }
            else if (loadedAgency.ClientPlayerIds != null)
            {
                foreach (var id in loadedAgency.ClientPlayerIds)
                {
                    if (string.IsNullOrEmpty(id)) continue;
                    Player p = DatabaseManager.Instance.Players.Find(x => x.Id == id);
                    if (p == null)
                    {
                        p = DatabaseManager.Instance.Players.Find(x => x.Name.Equals(id, System.StringComparison.OrdinalIgnoreCase));
                    }
                    if (p != null)
                    {
                        loadedAgency.AddClient(p);
                    }
                }
            }

            // Restore Week
            SimulationEngine.Instance.CurrentWeek = saveData.CurrentWeek;

            // Restore Standings
            foreach (var cData in saveData.ClubStandings)
            {
                Club club = DatabaseManager.Instance.Clubs.Find(x => x.OriginalName == cData.Name);
                if (club != null)
                {
                    club.StandingPlayed = cData.Played;
                    club.StandingWins = cData.Wins;
                    club.StandingDraws = cData.Draws;
                    club.StandingLosses = cData.Losses;
                    club.StandingGF = cData.GF;
                    club.StandingGA = cData.GA;
                }
            }

            Debug.Log($"Game Loaded from Slot {slotIndex}!");
            return true;
        }

        private string GetSlotLabel(int slotIndex)
        {
            string key = "SaveGame_Slot_" + slotIndex;
            if (!PlayerPrefs.HasKey(key))
            {
                return BehindTheScenesFootball.Managers.LocalizationManager.Translate($"Slot {slotIndex} [BOŞ]");
            }

            try
            {
                string json = PlayerPrefs.GetString(key);
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
                if (saveData != null)
                {
                    Agency loadedAgency = JsonUtility.FromJson<Agency>(saveData.AgencyJson);
                    if (loadedAgency != null)
                    {
                        string weekWord = BehindTheScenesFootball.Managers.LocalizationManager.Translate("Hafta");
                        return $"Slot {slotIndex}: {loadedAgency.ManagerName} | {loadedAgency.Name} ({weekWord} {saveData.CurrentWeek})";
                    }
                }
            }
            catch (System.Exception)
            {
                // Fallback
            }
            return BehindTheScenesFootball.Managers.LocalizationManager.Translate($"Slot {slotIndex} [KAYITLI]");
        }

        public void ShowSaveSlotsPopup(bool isSaving)
        {
            GameObject popup = CreatePanelHelper(mainCanvas.transform, "SlotsPopup", new Color(0f, 0f, 0f, 0.90f));
            SetRectTransform(popup, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Card
            GameObject card = CreatePanelHelper(popup.transform, "SlotsCard", new Color(0.12f, 0.16f, 0.22f, 0.95f));
            SetRectTransform(card, new Vector2(0.15f, 0.20f), new Vector2(0.85f, 0.80f), Vector2.zero, Vector2.zero);

            Outline border = card.AddComponent<Outline>();
            border.effectColor = colorAccent;
            border.effectDistance = new Vector2(2f, 2f);

            Text title = CreateText(card.transform, "Title", isSaving ? "OYUNU KAYDET" : "KAYITLI OYUN YÜKLE", 60, Color.white, TextAnchor.MiddleCenter);
            SetRectTransform(title, new Vector2(0.05f, 0.85f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero);
            title.fontStyle = FontStyle.Bold;

            // Slot 1 Button
            string s1Text = GetSlotLabel(1);
            Text btnSlot1 = CreateButtonHelper(card.transform, "BtnSlot1", s1Text, isSaving ? colorGreen : (PlayerPrefs.HasKey("SaveGame_Slot_1") ? colorGreen : colorGreyButton), Color.white, () => {
                if (isSaving)
                {
                    SaveGame(1);
                    Destroy(popup);
                    ShowFeedbackPopup("Oyun Slot 1'e başarıyla kaydedildi!");
                }
                else
                {
                    if (PlayerPrefs.HasKey("SaveGame_Slot_1") && LoadGame(1))
                    {
                        Destroy(popup);
                        TogglePauseMenu(false);
                        if (mainMenuObj != null) mainMenuObj.SetActive(false);
                        homeScreenObj.SetActive(true);
                        RefreshUI();
                        ShowFeedbackPopup("Slot 1'deki oyun başarıyla yüklendi!");
                    }
                    else
                    {
                        ShowFeedbackPopup("Bu slotta kayıtlı oyun bulunamadı!");
                    }
                }
            });
            SetRectTransform(btnSlot1.transform.parent, new Vector2(0.05f, 0.60f), new Vector2(0.95f, 0.72f), Vector2.zero, Vector2.zero);
            btnSlot1.fontSize = 36;
            btnSlot1.fontStyle = FontStyle.Bold;

            // Slot 2 Button
            string s2Text = GetSlotLabel(2);
            Text btnSlot2 = CreateButtonHelper(card.transform, "BtnSlot2", s2Text, isSaving ? colorGreen : (PlayerPrefs.HasKey("SaveGame_Slot_2") ? colorGreen : colorGreyButton), Color.white, () => {
                if (isSaving)
                {
                    SaveGame(2);
                    Destroy(popup);
                    ShowFeedbackPopup("Oyun Slot 2'ye başarıyla kaydedildi!");
                }
                else
                {
                    if (PlayerPrefs.HasKey("SaveGame_Slot_2") && LoadGame(2))
                    {
                        Destroy(popup);
                        TogglePauseMenu(false);
                        if (mainMenuObj != null) mainMenuObj.SetActive(false);
                        homeScreenObj.SetActive(true);
                        RefreshUI();
                        ShowFeedbackPopup("Slot 2'deki oyun başarıyla yüklendi!");
                    }
                    else
                    {
                        ShowFeedbackPopup("Bu slotta kayıtlı oyun bulunamadı!");
                    }
                }
            });
            SetRectTransform(btnSlot2.transform.parent, new Vector2(0.05f, 0.42f), new Vector2(0.95f, 0.54f), Vector2.zero, Vector2.zero);
            btnSlot2.fontSize = 36;
            btnSlot2.fontStyle = FontStyle.Bold;

            // Slot 3 Button
            string s3Text = GetSlotLabel(3);
            Text btnSlot3 = CreateButtonHelper(card.transform, "BtnSlot3", s3Text, isSaving ? colorGreen : (PlayerPrefs.HasKey("SaveGame_Slot_3") ? colorGreen : colorGreyButton), Color.white, () => {
                if (isSaving)
                {
                    SaveGame(3);
                    Destroy(popup);
                    ShowFeedbackPopup("Oyun Slot 3'e başarıyla kaydedildi!");
                }
                else
                {
                    if (PlayerPrefs.HasKey("SaveGame_Slot_3") && LoadGame(3))
                    {
                        Destroy(popup);
                        TogglePauseMenu(false);
                        if (mainMenuObj != null) mainMenuObj.SetActive(false);
                        homeScreenObj.SetActive(true);
                        RefreshUI();
                        ShowFeedbackPopup("Slot 3'deki oyun başarıyla yüklendi!");
                    }
                    else
                    {
                        ShowFeedbackPopup("Bu slotta kayıtlı oyun bulunamadı!");
                    }
                }
            });
            SetRectTransform(btnSlot3.transform.parent, new Vector2(0.05f, 0.24f), new Vector2(0.95f, 0.36f), Vector2.zero, Vector2.zero);
            btnSlot3.fontSize = 36;
            btnSlot3.fontStyle = FontStyle.Bold;

            // Cancel Button
            Text btnCancel = CreateButtonHelper(card.transform, "BtnCancel", "İPTAL", colorRed, Color.white, () => Destroy(popup));
            SetRectTransform(btnCancel.transform.parent, new Vector2(0.25f, 0.06f), new Vector2(0.75f, 0.16f), Vector2.zero, Vector2.zero);
            btnCancel.fontSize = 36;
            btnCancel.fontStyle = FontStyle.Bold;
        }

        #endregion
    }
}
