using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BehindTheScenesFootball.Core;
using BehindTheScenesFootball.Managers;

namespace BehindTheScenesFootball.UI
{
    public class NewsPanel : BaseModulePanel
    {
        private Transform listContent;

        public override void Initialize(UIManager manager, GameObject container)
        {
            base.Initialize(manager, container);

            // Create a Scroll View inside this container
            GameObject scrollView = uiManager.CreateScrollViewHelper(panelContainer.transform, "MailsScroll", out listContent);
            SetRectTransform(scrollView, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.82f), Vector2.zero, Vector2.zero);
        }

        public override void Refresh()
        {
            // Clear existing rows
            foreach (Transform child in listContent)
            {
                Destroy(child.gameObject);
            }

            VerticalLayoutGroup vlg = listContent.gameObject.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                vlg.spacing = 25f;
                vlg.padding = new RectOffset(20, 20, 30, 30);
                vlg.childAlignment = TextAnchor.UpperLeft;
                vlg.childControlWidth = true;
                vlg.childControlHeight = false;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;
            }

            ContentSizeFitter csf = listContent.gameObject.GetComponent<ContentSizeFitter>();
            if (csf == null) csf = listContent.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            var offers = SimulationEngine.Instance.ActiveOffers;
            var mails = SimulationEngine.Instance.ActiveMails;
            
            // Spacer
            GameObject spacer = new GameObject("HeaderSpacer");
            spacer.transform.SetParent(listContent, false);
            spacer.AddComponent<LayoutElement>().minHeight = 10f;

            if (offers.Count == 0 && mails.Count == 0)
            {
                // No active transfer mails - show general notifications
                GameObject emptyMail = uiManager.CreatePanelHelper(listContent, "EmptyMailRow", new Color(0.15f, 0.17f, 0.22f, 0.6f));
                LayoutElement le = emptyMail.AddComponent<LayoutElement>();
                le.minHeight = 360f;
                le.preferredHeight = 360f;

                Text info = CreateText(emptyMail.transform, "EmptyInfo", "<b>📩 GELEN KUTUSU BOŞ</b>\n\nŞu an aktif bir transfer teklifi veya özel mesaj bulunmuyor.", 48, Color.white, TextAnchor.MiddleCenter);
                SetRectTransform(info, Vector2.zero, Vector2.one, new Vector2(20f, 10f), new Vector2(-20f, -10f));
            }
            else
            {
                foreach (var offer in offers)
                {
                    CreateOfferMailRow(listContent, offer);
                }

                foreach (var mail in mails)
                {
                    CreateSimulationMailRow(listContent, mail);
                }
            }
        }

        private void CreateOfferMailRow(Transform parent, TransferOffer offer)
        {
            GameObject row = uiManager.CreatePanelHelper(parent, "OfferMailRow_" + offer.Id, new Color(0.18f, 0.22f, 0.28f, 0.85f));
            
            Outline border = row.AddComponent<Outline>();
            border.effectColor = uiManager.ColorAccent;
            border.effectDistance = new Vector2(3f, 3f);

            VerticalLayoutGroup layoutGroup = row.AddComponent<VerticalLayoutGroup>();
            layoutGroup.padding = new RectOffset(30, 30, 30, 30);
            layoutGroup.spacing = 20;
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;

            LayoutElement rowLe = row.AddComponent<LayoutElement>();
            rowLe.minHeight = 280f;

            // Body
            string bodyText;
            if (offer.IsLoanOffer)
            {
                bodyText = $"<b>{offer.CurrentClubName}</b>, <b>{offer.BidderClubName}</b> kulübünün <b>kiralama</b> teklifini kabul etti!\n" +
                           $"Oyuncu için önerilen sözleşme şartları: <b>{offer.ContractLengthYears} Yıl Kiralık</b> ({offer.BidderClubName} kulübünde oynaması planlanıyor)";
            }
            else
            {
                bodyText = $"<b>{offer.CurrentClubName}</b>, <b>{offer.BidderClubName}</b> kulübünün <b>€{offer.TransferFee:N0}</b> bonservis teklifini kabul etti!\n" +
                           $"Oyuncu için önerilen sözleşme şartları: <b>€{offer.OfferedWeeklyWage:N0}/hafta</b> ({offer.ContractLengthYears} Yıl)";
            }

            Text body = CreateText(row.transform, "Body", bodyText, 48, Color.white, TextAnchor.MiddleLeft);
            var scaler = body.GetComponent<TextScaler>();
            if (scaler != null) Destroy(scaler);
            body.fontSize = Mathf.RoundToInt(48 * 1.55f);
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Overflow;
            body.resizeTextForBestFit = false;

            // Buttons Container
            GameObject buttonsContainer = uiManager.CreatePanelHelper(row.transform, "ButtonsContainer", Color.clear);
            LayoutElement buttonsLe = buttonsContainer.AddComponent<LayoutElement>();
            buttonsLe.minHeight = 110f;
            buttonsLe.preferredHeight = 110f;

            HorizontalLayoutGroup buttonsLayout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            buttonsLayout.spacing = 20;
            buttonsLayout.childControlHeight = true;
            buttonsLayout.childControlWidth = true;
            buttonsLayout.childForceExpandHeight = true;
            buttonsLayout.childForceExpandWidth = true;

            // Accept Button
            Text acceptLabel = uiManager.CreateButtonHelper(buttonsContainer.transform, "BtnAccept", "KABUL ET", uiManager.ColorGreen, Color.white, () => {
                SimulationEngine.Instance.AcceptTransferOffer(offer.Id);
                Refresh();
            });
            acceptLabel.fontSize = 46;
            acceptLabel.fontStyle = FontStyle.Bold;

            // Nego Button
            Text negoLabel = uiManager.CreateButtonHelper(buttonsContainer.transform, "BtnNego", "PAZARLIK ET", uiManager.ColorAccent, Color.white, () => {
                uiManager.ShowTransferNegotiation(offer, () => Refresh());
            });
            negoLabel.fontSize = 46;
            negoLabel.fontStyle = FontStyle.Bold;

            // Reject Button
            Text rejectLabel = uiManager.CreateButtonHelper(buttonsContainer.transform, "BtnReject", "REDDET", uiManager.ColorRed, Color.white, () => {
                SimulationEngine.Instance.RejectTransferOffer(offer.Id);
                Refresh();
            });
            rejectLabel.fontSize = 46;
            rejectLabel.fontStyle = FontStyle.Bold;
        }

        private void CreateSimulationMailRow(Transform parent, SimulationMail mail)
        {
            Player p = string.IsNullOrEmpty(mail.PlayerId) ? null : DatabaseManager.Instance.GetPlayerById(mail.PlayerId);

            GameObject row = uiManager.CreatePanelHelper(parent, "SimMailRow_" + mail.Id, new Color(0.14f, 0.18f, 0.24f, 0.95f));
            
            Outline border = row.AddComponent<Outline>();
            border.effectColor = new Color(0.2f, 0.6f, 0.9f, 0.5f);
            border.effectDistance = new Vector2(2f, 2f);

            // 1. Dikey Kart Düzeni (Üstte Oyuncu & Metinler, Altta Butonlar)
            VerticalLayoutGroup mainVlg = row.AddComponent<VerticalLayoutGroup>();
            mainVlg.padding = new RectOffset(25, 25, 25, 25);
            mainVlg.spacing = 20;
            mainVlg.childAlignment = TextAnchor.UpperLeft;
            mainVlg.childControlWidth = true;
            mainVlg.childControlHeight = true;
            mainVlg.childForceExpandWidth = true;
            mainVlg.childForceExpandHeight = false;

            ContentSizeFitter rowCsf = row.AddComponent<ContentSizeFitter>();
            rowCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            rowCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            LayoutElement rowLe = row.AddComponent<LayoutElement>();
            rowLe.minHeight = 260f;

            // --- A. ÜST BÖLÜM: Sol (Oyuncu Kartı) - Sağ (Konu ve Mesaj Metni) ---
            GameObject headerContainer = new GameObject("HeaderContainer", typeof(RectTransform));
            headerContainer.transform.SetParent(row.transform, false);

            HorizontalLayoutGroup headerHlg = headerContainer.AddComponent<HorizontalLayoutGroup>();
            headerHlg.spacing = 25;
            headerHlg.childAlignment = TextAnchor.UpperLeft;
            headerHlg.childControlWidth = true;
            headerHlg.childControlHeight = true;
            headerHlg.childForceExpandWidth = false;
            headerHlg.childForceExpandHeight = false;

            ContentSizeFitter headerCsf = headerContainer.AddComponent<ContentSizeFitter>();
            headerCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            headerCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            if (p != null)
            {
                // Sol: Oyuncu Profil Kartı (Sabit 180px Genişlik x 220px Yükseklik)
                GameObject cardObj = uiManager.CreatePanelHelper(headerContainer.transform, "MailPlayerCard", new Color(0.12f, 0.15f, 0.20f, 1f));
                LayoutElement cardLe = cardObj.AddComponent<LayoutElement>();
                cardLe.preferredWidth = 180f;
                cardLe.minWidth = 180f;
                cardLe.preferredHeight = 220f;
                cardLe.minHeight = 220f;
                
                Button cardBtn = cardObj.AddComponent<Button>();
                cardBtn.onClick.AddListener(() => {
                    uiManager.ShowPlayerDetails(p, false);
                });
                uiManager.ConfigureButtonTransition(cardBtn);

                Image cardImg = cardObj.GetComponent<Image>();
                if (cardImg != null && uiManager.RoundedButtonSprite != null)
                {
                    cardImg.sprite = uiManager.RoundedButtonSprite;
                    cardImg.type = Image.Type.Sliced;
                }

                GameObject faceObj = new GameObject("CardFace", typeof(RectTransform));
                faceObj.transform.SetParent(cardObj.transform, false);
                SetRectTransform(faceObj, new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero);
                Image faceImg = faceObj.AddComponent<Image>();
                faceImg.sprite = uiManager.GetMiniface(p);
                faceImg.preserveAspect = true;

                string ratingStr = $"<color=#58D68D><b>{p.OVR}</b></color> {p.Position}";
                Text ovrTxt = CreateText(cardObj.transform, "CardOVR", ratingStr, 34, Color.white, TextAnchor.MiddleCenter);
                SetRectTransform(ovrTxt, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.26f), Vector2.zero, Vector2.zero);
            }

            // Sağ: Metin Sütunu (Konu ve Mesaj)
            GameObject textCol = new GameObject("TextColumn", typeof(RectTransform));
            textCol.transform.SetParent(headerContainer.transform, false);

            LayoutElement textColLe = textCol.AddComponent<LayoutElement>();
            textColLe.flexibleWidth = 1f;

            VerticalLayoutGroup textVlg = textCol.AddComponent<VerticalLayoutGroup>();
            textVlg.spacing = 10;
            textVlg.childAlignment = TextAnchor.UpperLeft;
            textVlg.childControlWidth = true;
            textVlg.childControlHeight = true;
            textVlg.childForceExpandWidth = true;
            textVlg.childForceExpandHeight = false;

            ContentSizeFitter textCsf = textCol.AddComponent<ContentSizeFitter>();
            textCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            textCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Subject (Başlık)
            Text subject = CreateText(textCol.transform, "Subject", mail.Subject, 46, uiManager.ColorAccent, TextAnchor.MiddleLeft);
            subject.fontStyle = FontStyle.Bold;
            var subScaler = subject.GetComponent<TextScaler>();
            if (subScaler != null) Destroy(subScaler);
            subject.horizontalOverflow = HorizontalWrapMode.Wrap;

            // Body (İçerik)
            Text body = CreateText(textCol.transform, "Body", mail.Content, 40, Color.white, TextAnchor.MiddleLeft);
            var bodyScaler = body.GetComponent<TextScaler>();
            if (bodyScaler != null) Destroy(bodyScaler);
            body.fontSize = 40;
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Overflow;

            // --- B. ALT BÖLÜM: Aksiyon Butonları (Tüm Kart Genişliğinde) ---
            if (mail.IsRenewalMail)
            {
                GameObject buttonsContainer = uiManager.CreatePanelHelper(row.transform, "ButtonsContainer", Color.clear);
                LayoutElement buttonsLe = buttonsContainer.AddComponent<LayoutElement>();
                buttonsLe.minHeight = 110f;
                buttonsLe.preferredHeight = 110f;

                HorizontalLayoutGroup buttonsLayout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
                buttonsLayout.spacing = 20;
                buttonsLayout.childControlHeight = true;
                buttonsLayout.childControlWidth = true;
                buttonsLayout.childForceExpandHeight = true;
                buttonsLayout.childForceExpandWidth = true;

                Text renewLabel = uiManager.CreateButtonHelper(buttonsContainer.transform, "BtnRenew", "SÖZLEŞME YENİLE", uiManager.ColorGreen, new Color(11f/255f, 12f/255f, 16f/255f, 1f), () => {
                    if (p != null)
                    {
                        uiManager.ShowSignNegotiation(p, () => Refresh());
                    }
                });
                renewLabel.fontSize = 46;
                renewLabel.fontStyle = FontStyle.Bold;

                Text deleteLabel = uiManager.CreateButtonHelper(buttonsContainer.transform, "BtnDelete", "OKUNDU İŞARETLE", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                    SimulationEngine.Instance.ActiveMails.Remove(mail);
                    Refresh();
                });
                deleteLabel.fontSize = 46;
                deleteLabel.fontStyle = FontStyle.Bold;
            }
            else if (mail.IsRequest)
            {
                GameObject buttonsContainer = uiManager.CreatePanelHelper(row.transform, "ButtonsContainer", Color.clear);
                LayoutElement buttonsLe = buttonsContainer.AddComponent<LayoutElement>();
                buttonsLe.minHeight = 110f;
                buttonsLe.preferredHeight = 110f;

                HorizontalLayoutGroup buttonsLayout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
                buttonsLayout.spacing = 20;
                buttonsLayout.childControlHeight = true;
                buttonsLayout.childControlWidth = true;
                buttonsLayout.childForceExpandHeight = true;
                buttonsLayout.childForceExpandWidth = true;

                string acceptText = BehindTheScenesFootball.Managers.LocalizationManager.Translate("KABUL ET") + (mail.MoneyCost > 0 ? $" (€{mail.MoneyCost:N0})" : "");
                Text acceptLabel = uiManager.CreateButtonHelper(buttonsContainer.transform, "BtnAcceptReq", acceptText, uiManager.ColorGreen, new Color(11f/255f, 12f/255f, 16f/255f, 1f), () => {
                    long currentBal = AgencyManager.Instance.ActiveAgency.Balance;
                    if (currentBal < mail.MoneyCost)
                    {
                        AgencyManager.Instance.LogActivity($"Yetersiz Bütçe: Oyuncunun talebini karşılamak için €{mail.MoneyCost:N0} bütçeniz bulunmamaktadır!");
                        return;
                    }
                    
                    AgencyManager.Instance.ActiveAgency.Balance -= mail.MoneyCost;

                    if (p != null)
                    {
                        p.Happiness = Mathf.Clamp(p.Happiness + mail.HappinessEffect, 10f, 100f);
                        
                        switch (mail.RequestType)
                        {
                            case "Coach":
                                p.OVR = Mathf.Min(99, p.OVR + 1);
                                p.UpdateMarketValue();
                                AgencyManager.Instance.LogActivity($"TALEBİ KARŞILADINIZ: {p.Name} için özel antrenör tuttunuz. (Bütçe: -€{mail.MoneyCost:N0}, GEN: +1, Moral: +{mail.HappinessEffect})");
                                break;

                            case "PR":
                                p.MarketValue = Mathf.RoundToInt(p.MarketValue * 1.03f);
                                AgencyManager.Instance.LogActivity($"TALEBİ KARŞILADINIZ: {p.Name} için PR kampanyası başlattınız. (Bütçe: -€{mail.MoneyCost:N0}, Piyasa Değeri: +%3, Moral: +{mail.HappinessEffect})");
                                break;

                            case "Wage":
                                uiManager.ShowClubWageRenegotiation(p, () => Refresh());
                                AgencyManager.Instance.LogActivity($"TALEBİ KARŞILADINIZ: {p.Name} için kulübü ile maaş zam görüşmelerine başlandı.");
                                break;

                            case "Leadership":
                                p.SquadRole = "Kaptan / Lider";
                                AgencyManager.Instance.LogActivity($"TALEBİ KARŞILADINIZ: {p.Name} takım içi lider olarak desteklendi. (Moral: +{mail.HappinessEffect})");
                                break;

                            case "Transfer":
                                if (p != null && p.CurrentContract != null)
                                {
                                    bool isKeyPlayer = p.OVR >= 68 || p.Form >= 55f;
                                    if (isKeyPlayer)
                                    {
                                        uiManager.ShowClubKeepOfferPopup(p, () => Refresh());
                                    }
                                    else
                                    {
                                        p.IsTransferListed = true;
                                        p.IsSuggestedForLoan = true;
                                        p.TransferStatusNote = "Transfer Listesinde (Ayrılmak İstiyor)";
                                        AgencyManager.Instance.LogActivity($"TALEBİ KARŞILADINIZ: {p.Name} kulübü tarafından transfer listesine konuldu. (Moral: +{mail.HappinessEffect})");
                                    }
                                }
                                else
                                {
                                    if (p != null)
                                    {
                                        p.IsTransferListed = true;
                                        p.IsSuggestedForLoan = true;
                                        p.TransferStatusNote = "Transfer Listesinde (Ayrılmak İstiyor)";
                                    }
                                    AgencyManager.Instance.LogActivity($"TALEBİ KARŞILADINIZ: {p.Name} transfer listesine önerildi.");
                                }
                                break;

                            case "Physio":
                                p.Form = Mathf.Min(100f, p.Form + 15f);
                                AgencyManager.Instance.LogActivity($"TALEBİ KARŞILADINIZ: {p.Name} için fizyoterapist tutuldu. (Form: +15, Moral: +{mail.HappinessEffect})");
                                break;

                            case "Sponsor":
                                Sponsor mockSp = new Sponsor("Red Bull", 3500, 3, 80);
                                if (p.PendingSponsorOffers == null) p.PendingSponsorOffers = new List<Sponsor>();
                                p.PendingSponsorOffers.Add(mockSp);
                                AgencyManager.Instance.LogActivity($"TALEBİ KARŞILADINIZ: {p.Name} için yeni sponsorluk fırsatı bulundu! (Moral: +{mail.HappinessEffect})");
                                break;

                            case "Mental":
                                p.Form = Mathf.Min(100f, p.Form + 10f);
                                p.Happiness = Mathf.Min(100f, p.Happiness + 10f);
                                AgencyManager.Instance.LogActivity($"TALEBİ KARŞILADINIZ: {p.Name} zihinsel koçluk seansı aldı. (Moral: +{mail.HappinessEffect})");
                                break;

                            case "Camp":
                                p.POT = Mathf.Min(99, p.POT + 1);
                                AgencyManager.Instance.LogActivity($"TALEBİ KARŞILADINIZ: {p.Name} gelişim kampına gönderildi. (Potansiyel: +1, Moral: +{mail.HappinessEffect})");
                                break;

                            case "MediaSupport":
                                AgencyManager.Instance.LogActivity($"TALEBİ KARŞILADINIZ: {p.Name} için medyaya destek açıklaması yapıldı. (Moral: +{mail.HappinessEffect})");
                                break;

                            default:
                                AgencyManager.Instance.LogActivity($"TALEBİ KARŞILADINIZ: {p.Name} adlı oyuncunun talebi yerine getirildi. (Moral: +{mail.HappinessEffect})");
                                break;
                        }
                    }

                    SimulationEngine.Instance.ActiveMails.Remove(mail);
                    Refresh();
                });
                acceptLabel.fontSize = 46;
                acceptLabel.fontStyle = FontStyle.Bold;

                Text rejectLabel = uiManager.CreateButtonHelper(buttonsContainer.transform, "BtnRejectReq", "REDDET", uiManager.ColorRed, Color.white, () => {
                    if (p != null)
                    {
                        p.Happiness = Mathf.Clamp(p.Happiness - mail.HappinessEffect, 10f, 100f);
                        AgencyManager.Instance.LogActivity($"TALEBİ REDDETTİNİZ: {p.Name} adlı oyuncunun isteğini reddettiniz. (Moral: -{mail.HappinessEffect})");
                    }
                    
                    SimulationEngine.Instance.ActiveMails.Remove(mail);
                    Refresh();
                });
                rejectLabel.fontSize = 46;
                rejectLabel.fontStyle = FontStyle.Bold;
            }
            else
            {
                GameObject buttonsContainer = uiManager.CreatePanelHelper(row.transform, "ButtonsContainer", Color.clear);
                LayoutElement buttonsLe = buttonsContainer.AddComponent<LayoutElement>();
                buttonsLe.minHeight = 110f;
                buttonsLe.preferredHeight = 110f;

                HorizontalLayoutGroup buttonsLayout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
                buttonsLayout.spacing = 20;
                buttonsLayout.childControlHeight = true;
                buttonsLayout.childControlWidth = true;
                buttonsLayout.childForceExpandHeight = true;
                buttonsLayout.childForceExpandWidth = true;

                Text deleteLabel = uiManager.CreateButtonHelper(buttonsContainer.transform, "BtnDelete", "OKUNDU OLARAK İŞARETLE", new Color(0.18f, 0.22f, 0.25f, 1f), Color.white, () => {
                    SimulationEngine.Instance.ActiveMails.Remove(mail);
                    Refresh();
                });
                deleteLabel.fontSize = 46;
                deleteLabel.fontStyle = FontStyle.Bold;
            }
        }
    }
}
