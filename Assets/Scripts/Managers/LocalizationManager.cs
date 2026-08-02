using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehindTheScenesFootball.Managers
{
    public static class LocalizationManager
    {
        public static string CurrentLanguage
        {
            get { return PlayerPrefs.GetString("SelectedLanguage", "TR"); }
            set 
            { 
                PlayerPrefs.SetString("SelectedLanguage", value); 
                PlayerPrefs.Save();
            }
        }

        public static string T(string text)
        {
            return Translate(text);
        }

        public static string Translate(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (CurrentLanguage == "TR") return text;

            // Direct Translation Lookup
            if (uiTranslations.TryGetValue(text, out string val))
            {
                return val;
            }
            if (storeTranslations.TryGetValue(text, out string sVal))
            {
                return sVal;
            }

            string result = text;

            // Translate general countries and leagues if present standalone
            if (countryTranslations.TryGetValue(result, out string cVal)) return cVal;
            if (leagueTranslations.TryGetValue(result, out string lVal)) return lVal;

            // Translate common dynamic text patterns (Mails, notifications, logs, popups)
            // 0. Primary High-Priority Long Translations (to prevent substring theft / corruptions)
            result = result.Replace("Sponsor Sözleşmesi Yok", "No Sponsor Contract");
            result = result.Replace("Sayın Menajer,\n\nOyuncunuz", "Dear Manager,\n\nYour player");
            result = result.Replace("kulübündeki başarılı performansıyla markamızın dikkatini çekmiştir. Kendisine haftalık taban", "has attracted our brand's attention with his performance. We would like to offer a base weekly wage of");
            result = result.Replace("bütçeli bir sponsorluk anlaşması sunmak istiyoruz. Detaylı görüşme ve imza işlemleri için lütfen oyuncunun profil sayfasındaki 'Sponsorluk Teklifleri' menüsünü ziyaret edin.\n\nSaygılarımızla,", "sponsorship agreement. Please visit 'Sponsorship Offers' under the player's profile page for details and signing.\n\nBest regards,");
            result = result.Replace("SPONSOR TEKLİFLERİ", "SPONSOR OFFERS");
            result = result.Replace("Ajans Komisyonu:", "Agency Commission:");
            result = result.Replace("İmza Primi (Bonus):", "Signing Bonus:");
            result = result.Replace("KİRALIK KULÜPLERE ÖNER", "SUGGEST TO CLUBS FOR LOAN");
            result = result.Replace("KİRALIK ÖNERİLDİ", "SUGGESTED FOR LOAN");
            result = result.Replace("KİRALIK ÖNER", "SUGGEST FOR LOAN");

            result = System.Text.RegularExpressions.Regex.Replace(result, @"<b>(.*?)</b>, <b>(.*?)</b> kulübünün <b>kiralama</b> teklifini kabul etti!", "<b>$1</b> accepted the loan offer of <b>$2</b>!");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"<b>(.*?)</b>, <b>(.*?)</b> kulübünün <b>kiralama</b> teklifini kabul etti", "<b>$1</b> accepted the loan offer of <b>$2</b>");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"<b>(.*?)</b>, <b>(.*?)</b> kulübünün <b>€(.*?)</b> bonservis teklifini kabul etti!", "<b>$1</b> accepted <b>$2</b>'s transfer offer of <b>€$3</b>!");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"<b>(.*?)</b>, <b>(.*?)</b> kulübünün <b>€(.*?)</b> bonservis teklifini kabul etti", "<b>$1</b> accepted <b>$2</b>'s transfer offer of <b>€$3</b>");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\((.*?) kulübünde oynaması planlanıyor\)", "(planned to play in $1)");

            result = result.Replace("📩 GELEN KUTUSU BOŞ", "📩 INBOX EMPTY");
            result = result.Replace("Şu an aktif bir transfer teklifi veya özel mesaj bulunmuyor.", "There are no active transfer offers or private messages at the moment.");
            result = result.Replace("KİRALIK TRANSFER ANLAŞMASI", "LOAN TRANSFER AGREEMENT");
            result = result.Replace("TRANSFER ANLAŞMASI", "TRANSFER AGREEMENT");
            result = result.Replace("kulübünün kiralama teklifini kabul etti", "accepted the loan offer of");
            result = result.Replace("kulübünün kiralama teklifini kabul etti!", "accepted the loan offer!");
            result = result.Replace("bonservis teklifini kabul etti!", "accepted the transfer fee offer!");
            result = result.Replace("bonservis teklifini kabul etti", "accepted the transfer fee offer of");
            result = result.Replace("Oyuncu için önerilen sözleşme şartları:", "Proposed contract terms for the player:");
            result = result.Replace("Yıl Kiralık", "Years Loan");
            result = result.Replace("kulübünde oynaması planlanıyor", "is planned to play in");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\b1\s+Hafta\b", "1 Week");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\b(\d+)\s+Hafta\b", "$1 Weeks");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\bHafta\b", "Week");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\b1\s+Yıl\b", "1 Year");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\b(\d+)\s+Yıl\b", "$1 Years");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\bYıl\b", "Years");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\bhafta\b", "week");
            result = result.Replace("Gözlemciler:", "Scouts:");
            result = result.Replace("Aday:", "Candidate:");
            result = result.Replace("GÖZLEMCİ AL", "HIRE SCOUT");
            result = result.Replace("Bedel:", "Cost:");
            result = result.Replace("Maaş:", "Salary:");
            result = result.Replace("Kapasite Dolu", "Capacity Full");
            result = result.Replace("İŞE ALINMIŞ GÖZLEMCİ YOK", "NO HIRED SCOUTS");
            result = result.Replace("Yukarıdaki 'PERSONEL AL' butonuna tıklayarak aday gözlemciyi işe alabilirsiniz.", "You can hire the candidate scout by clicking the 'HIRE SCOUT' button above.");
            result = result.Replace("Gözlemcileri seçtiğiniz liglere göndererek menajeri olmayan potansiyelli yetenekleri keşfedebilirsiniz.", "You can discover unrepresented talents by sending scouts to selected leagues.");
            result = result.Replace("BOŞTA", "IDLE");
            result = result.Replace("LİGE GÖNDER", "SEND TO LEAGUE");
            result = result.Replace("ARAMA YAPIYOR", "SEARCHING");
            result = result.Replace("HAFTA KALDI", "WEEKS LEFT");
            result = result.Replace("RAPOR", "REPORT");
            result = result.Replace("YENİ GÖREV", "NEW MISSION");
            result = result.Replace("GERİ DÖN (İPTAL)", "BACK (CANCEL)");
            result = result.Replace("GÖZLEMCİLERE GERİ DÖN", "BACK TO SCOUTS");
            result = result.Replace("Bu raporda henüz aday oyuncu bulunmamaktadır veya tamamı sözleşme imzalanmıştır.", "There are no candidate players in this report yet, or all of them have signed contracts.");
            result = result.Replace("GÖREV Yolla", "SEND ON MISSION");
            result = result.Replace("TEMSİL ET", "REPRESENT");
            result = result.Replace("YOKSAY", "IGNORE");
            result = result.Replace("FAVORİYE EKLE", "ADD TO FAVORITES");
            result = result.Replace("FAVORİDEN ÇIKAR", "REMOVE FROM FAVORITES");
            result = result.Replace("SÖZLEŞME İMZALA", "SIGN CONTRACT");
            result = result.Replace("TEMAS KUR", "CONTACT");
            result = result.Replace("AJANS ETKİLEŞİMİ (MUTLULUK)", "AGENCY INTERACTION (HAPPINESS)");
            result = result.Replace("ÖV", "PRAISE");
            result = result.Replace("PRİM VER", "GIVE BONUS");
            result = result.Replace("UYAR", "WARN");
            result = result.Replace("FESHET (BIRAK)", "TERMINATE (RELEASE)");
            result = result.Replace("KULÜBE ÖNER", "SUGGEST TO CLUB");
            result = result.Replace("KİRALIK ÖNER", "SUGGEST FOR LOAN");
            result = result.Replace("SERBEST BIRAKILDI", "RELEASED");
            result = result.Replace("SÖZLEŞMEYİ FESHET (BIRAK)", "TERMINATE CONTRACT (RELEASE)");
            result = result.Replace("AJANS FİNANS MERKEZİ", "AGENCY FINANCE CENTER");
            result = result.Replace("Ajans Seviyesi:", "Agency Level:");
            result = result.Replace("Haftalık Ajans Geliri:", "Weekly Agency Income:");
            result = result.Replace("Haftalık Personel Gideri:", "Weekly Staff Expense:");
            result = result.Replace("Net Haftalık Gelir:", "Net Weekly Income:");
            result = result.Replace("Ajans Kasası:", "Agency Vault:");
            result = result.Replace("Komisyon Gelirleri:", "Commission Income:");
            result = result.Replace("Sponsor Komisyonu:", "Sponsor Commission:");
            result = result.Replace("Maaş Komisyonu:", "Wage Commission:");
            result = result.Replace("Kasa Özeti:", "Vault Summary:");
            result = result.Replace("Ajans Seviyesini Yükselt", "Upgrade Agency Level");
            result = result.Replace("Müşteri Kapasitesi:", "Client Capacity:");
            result = result.Replace("Maksimum Temsil Gücü:", "Max Represent Power:");
            result = result.Replace("Personel Limiti:", "Staff Limit:");
            result = result.Replace("YÜKSELT", "UPGRADE");
            result = result.Replace("AJANS PRESTİJ & KİŞİSEL SERVET", "AGENCY PRESTIGE & PERSONAL WEALTH");
            result = result.Replace("Kişisel Servetiniz:", "Your Personal Wealth:");
            result = result.Replace("Ajans İtibarı (Reputation):", "Agency Reputation:");
            result = result.Replace("Mülk Satın Al", "Buy Property");
            result = result.Replace("SAHİPSİNİZ", "OWNED");
            result = result.Replace("SATIN AL", "BUY");
            result = result.Replace("LÜKS MAĞAZA", "LUXURY STORE");
            result = result.Replace("LÜKS MAĞAZA & PRESTİJ", "LUXURY STORE & PRESTIGE");
            result = result.Replace("LİGLER", "LEAGUES");
            result = result.Replace("KULÜPLER", "CLUBS");
            result = result.Replace("TRANSFERLER", "TRANSFERS");
            result = result.Replace("LİG DETAYLARI", "LEAGUE DETAILS");
            result = result.Replace("KULÜP BİLGİLERİ", "CLUB INFORMATION");
            result = result.Replace("YAPILAN TRANSFERLER", "COMPLETED TRANSFERS");
            result = result.Replace("OYUNCU PİYASASI", "PLAYER MARKET");
            result = result.Replace("Temsil Edilen Oyuncu Yok", "No Represented Players");
            result = result.Replace("Teklifler", "Offers");
            result = result.Replace("Kadro Rolü:", "Squad Role:");
            result = result.Replace("Mutluluk:", "Morale:");
            result = result.Replace("Piyasa Değeri:", "Market Value:");
            result = result.Replace("Kulüp:", "Club:");
            result = result.Replace(" ile Temsilcilik Pazarlığı", " Representation Negotiation");
            result = result.Replace(" ile Sözleşme Yenileme", " Contract Renewal");
            result = result.Replace("Temsilcilik Pazarlığı", "Representation Negotiation");
            result = result.Replace("Sözleşme Yenileme", "Contract Renewal");
            result = result.Replace("Sözleşme süresini uzun tutarsanız komisyon oranlarında esneklik payı artar.", "Longer contracts increase commission negotiation flexibility.");
            result = result.Replace("Transfer Komisyon Payı:", "Transfer Commission Share:");
            result = result.Replace("Haftalık Maaş Komisyon Payı:", "Weekly Wage Commission Share:");
            result = result.Replace("Sponsorluk Komisyon Payı:", "Sponsor Commission Share:");
            result = result.Replace("Maaş Komisyon Payı:", "Wage Commission Share:");
            result = result.Replace("Temsilcilik Süresi (Yıl):", "Representation Duration (Years):");
            result = result.Replace("Sözleşme Süresi:", "Contract Duration:");
            result = result.Replace("İmza Parası (Bonus):", "Signing Bonus:");
            result = result.Replace(" Görüşmesi", " Negotiation");
            result = result.Replace("Sözleşme Süresi (Yıl):", "Contract Duration (Years):");
            result = result.Replace("TEKLİFİ SUN", "SUBMIT OFFER");
            result = result.Replace("Dil / Language", "Language");
            result = result.Replace("TÜRKÇE", "TURKISH");
            result = result.Replace("İNGİLİZCE", "ENGLISH");
            result = result.Replace("OYUN İÇİ AYARLAR", "GAME SETTINGS");
            result = result.Replace("ANA MENÜYE DÖN", "RETURN TO MAIN MENU");
            result = result.Replace("OYUN DURAKLATILDI", "GAME PAUSED");
            result = result.Replace("Haberler", "News");
            result = result.Replace("Gözlemci Merkezi", "Scouting Center");
            result = result.Replace(" Yeni)", " New)");
            result = result.Replace(" Rapor)", " Report)");
            result = result.Replace("DEVAM ET", "RESUME");
            result = result.Replace("ÖZEL HAYAT & MÜLKLER", "PRIVATE LIFE & PROPERTIES");
            result = result.Replace("AJANS BÜTÇESİ & FİNANS", "AGENCY BUDGET & FINANCE");
            result = result.Replace("HABERLER & E-POSTA", "NEWS & EMAIL");
            result = result.Replace("Gözlemciler: 3 / 3 (Kapasite Dolu)", "Scouts: 3 / 3 (Capacity Full)");
            result = result.Replace("İŞE ALINMIŞ GÖZLEMCİ YOK", "NO HIRED SCOUTS");
            result = result.Replace("Gözlemciler: ", "Scouts: ");
            result = result.Replace("Gözlemciler:", "Scouts:");
            result = result.Replace("Aday: ", "Candidate: ");
            result = result.Replace("Aday:", "Candidate:");
            result = result.Replace("Sev: ", "Lvl: ");
            result = result.Replace("Sev ", "Lvl ");
            result = result.Replace("Seviye: ", "Level: ");
            result = result.Replace("Seviye:", "Level:");
            result = result.Replace("GÖZLEMCİ AL", "HIRE SCOUT");
            result = result.Replace("Bedel: ", "Cost: ");
            result = result.Replace("Bedel:", "Cost:");
            result = result.Replace("Temsil Edilen Oyuncu Yok", "No Represented Players");
            result = result.Replace(" Teklifler", " Offers");
            result = result.Replace(" Takım", " Clubs");
            result = result.Replace("Pozisyon: ", "Position: ");
            result = result.Replace("Kaleci (GK)", "Goalkeeper (GK)");
            result = result.Replace("Defans (DEF)", "Defender (DEF)");
            result = result.Replace("Orta Saha (MID)", "Midfielder (MID)");
            result = result.Replace("Forvet (FWD)", "Forward (FWD)");
            result = result.Replace("Yaş: ", "Age: ");
            result = result.Replace("Sponsor: ", "Sponsor: ");
            result = result.Replace("Müşterilerim", "My Clients");
            result = result.Replace("Hafta ", "Week ");
            result = result.Replace("Bütçe: ", "Budget: ");
            result = result.Replace("İtibar: ", "Reputation: ");
            result = result.Replace("Seviye ", "Level ");
            result = result.Replace(" Ajansı", " Agency");
            // Player Detail / Morale / Traits / Contract Translations
            result = result.Replace(" Yaş", " Years Old");
            result = result.Replace("\u00A0Yaş", " Years Old");
            result = result.Replace("YAŞ", "AGE");
            result = result.Replace("Yok", "None");
            result = result.Replace("Serbest", "Free Agent");
            
            // Happiness / Morale states
            result = result.Replace("Çok Mutlu (+Perf)", "Very Happy (+Perf)");
            result = result.Replace("Mutlu / Dengeli", "Happy / Balanced");
            result = result.Replace("Huzursuz / Endişeli", "Restless / Anxious");
            result = result.Replace("Krizde / Ayrılmak İstiyor", "In Crisis / Wants to Leave");
            result = result.Replace("Mutluluk: ", "Morale: ");
            result = result.Replace("Maaş: ", "Salary: ");
            result = result.Replace("Kadro Rolü: ", "Squad Role: ");
            result = result.Replace("Piyasa Değeri: ", "Market Value: ");
            result = result.Replace("Kulüp: ", "Club: ");
            result = result.Replace("Asıl Kulüp: ", "Parent Club: ");
            result = result.Replace("Kiralık: ", "On Loan: ");
            result = result.Replace(" kulübünde kiralık", " on loan at");
            result = result.Replace("Ajans Sözleşmesi: ", "Agency Contract: ");
            result = result.Replace("/ hafta", "/ week");
            result = result.Replace("Sponsor: ", "Sponsor: ");
            result = result.Replace("Boşta (Temsilcisi Yok)", "Free Agent (No Agent)");
            result = result.Replace("Sizin Müşteriniz", "Your Client");
            result = result.Replace("Rakip Temsilci", "Rival Representative");

            // Traits
            result = result.Replace("Çalışkan", "Hardworking");
            result = result.Replace("Lider", "Leader");
            result = result.Replace("Büyük Maç", "Big Match Player");
            result = result.Replace("Tembel", "Lazy");
            result = result.Replace("Uyumsuz", "Incompatible");
            result = result.Replace("Sadakatsiz", "Disloyal");
            result = result.Replace("Güvenilmez", "Unreliable");

            result = result.Replace("Gelirler: ", "Incomes: ");
            result = result.Replace("Giderler: ", "Expenses: ");
            result = result.Replace("Net Akış: ", "Net Flow: ");
            result = result.Replace("/ hf", "/ wk");
            result = result.Replace("/hf", "/wk");

            // UI Actions
            result = result.Replace("Rol: ", "Role: ");
            result = result.Replace("Haftalık: ", "Weekly: ");
            result = result.Replace("İlk 11 Oyuncusu", "First Team Player");
            result = result.Replace("Genç Yetenek", "Young Prospect");
            result = result.Replace("Yıldız Oyuncu", "Star Player");
            result = result.Replace("Önemli Oyuncu", "Important Player");
            result = result.Replace("Rotasyon Oyuncusu", "Rotation Player");
            result = result.Replace("Yedek Oyuncu", "Backup Player");
            result = result.Replace("Bu hafta oyuncunuza uygun transfer teklifi bulunmuyor. Gelecek hafta tekrar deneyebilirsiniz.", "There are no suitable transfer offers for your player this week. You can try again next week.");
            result = result.Replace("SÖZLEŞME İMZALA", "SIGN CONTRACT");
            result = result.Replace("FAVORİYE EKLE", "ADD TO FAVORITES");
            result = result.Replace("FAVORİDEN ÇIKAR", "REMOVE FROM FAVORITES");
            result = result.Replace("TEMAS KUR", "CONTACT");
            result = result.Replace("TEMAS KURULDU", "CONTACTED");
            result = result.Replace("KİLİTLİ (SEV. ", "LOCKED (LVL. ");
            result = result.Replace("SAHİPSİNİZ", "OWNED");
            result = result.Replace("SATIN AL", "BUY");
            result = result.Replace("Şirket Seviyesi: ", "Company Level: ");
            result = result.Replace("Komisyon: ", "Commission: ");
            result = result.Replace("Gelir: ", "Income: ");
            result = result.Replace("/hf", "/wk");
            result = result.Replace("Marka: ", "Brand: ");
            result = result.Replace("✔ AKTİF KULLANIMDA", "✔ IN ACTIVE USE");
            result = result.Replace("Sahip Olunan Varlıklar: ", "Owned Assets: ");
            result = result.Replace("Sahip Olunan Mülkler: ", "Owned Properties: ");
            result = result.Replace("Toplam Yatırım Değeri: ", "Total Investment Value: ");
            result = result.Replace("Toplam Kazanılan İtibar: ", "Total Reputation Earned: ");
            result = result.Replace(" Adet", " Units");
            result = result.Replace(" Puan", " Points");
            result = result.Replace("Henüz hiçbir mülk veya lüks ürün satın almadınız.", "You have not purchased any properties or luxury items yet.");
            result = result.Replace("Ana ekrandaki 'Mağaza' sekmesini ziyaret ederek prestijinizi arttıracak yatırımlar yapabilirsiniz.", "You can make investments to increase your prestige by visiting the 'Store' tab on the home screen.");
            result = result.Replace("TEMSİLCİSİ OLDUĞUNUZ OYUNCU BULUNMUYOR", "NO PLAYERS UNDER REPRESENTATION");
            result = result.Replace("Finansal gelir elde edebilmek için öncelikle", "To obtain financial income, you must first");
            result = result.Replace("sekmesinden gözlemci işe almalı ve liglerde keşfe yollayarak menajeri olmayan oyuncularla sözleşme imzalamalısınız.", "hire scouts from the 'Scouting Center' tab, send them to search in leagues, and sign contracts with unrepresented players.");
            result = result.Replace("AKTİF SÖZLEŞME VE FİNANSAL AKIŞ DETAYLARI", "ACTIVE CONTRACT & FINANCIAL FLOW DETAILS");
            result = result.Replace("KULÜP SÖZLEŞMESİ & MAAŞ", "CLUB CONTRACT & SALARY");
            result = result.Replace("AKTİF SPONSORLUK", "ACTIVE SPONSORSHIP");
            result = result.Replace("🔍 İZLEME LİSTESİ BOŞ", "🔍 WATCHLIST EMPTY");
            result = result.Replace("Favoriye eklediğiniz veya temasa geçtiğiniz oyuncular burada listelenir.\nOyuncuları gözlemci raporlarından favorileyebilirsiniz.", "Players you add to favorites or contact will be listed here.\nYou can favorite players from scout reports.");
            result = result.Replace("Değer: ", "Value: ");
            result = result.Replace("Asıl Kulüp: ", "Parent Club: ");
            result = result.Replace("kulübünde kiralık", "on loan at");
            result = result.Replace("Kiralık: ", "On Loan: ");

            // Negotiation Feedback Partial Replacements
            result = result.Replace("Pazarlık ediliyor...", "Negotiating...");
            result = result.Replace("Tüm talepleriniz (", "All your demands (");
            result = result.Replace(" maaş, ", " wage, ");
            result = result.Replace(" transfer ve ", " transfer and ");
            result = result.Replace(" yıl) benim için çok fazla! Teklifinizi düşürün.", " years) is too much for me! Lower your offer.");
            result = result.Replace("Bu süre (", "For this duration (");
            result = result.Replace(" yıl) için maaşımdan keseceğiniz pay (", " years), the share you will cut from my salary (");
            result = result.Replace(") çok yüksek, bunu kabul edemem.", ") is too high, I cannot accept this.");
            result = result.Replace("Seçtiğiniz sözleşme süresine (", "Compared to your chosen contract duration (");
            result = result.Replace(" yıl) karşılık transfer payı talebiniz (", " years), your transfer share demand (");
            result = result.Replace(") çok abartılı.", ") is very exaggerated.");
            result = result.Replace("Sponsorluk komisyon oranı (", "The sponsorship commission rate (");
            result = result.Replace(") bu uzunluktaki sözleşme için çok fazla.", ") is too much for this contract length.");
            result = result.Replace("Temsilcilik sözleşmesi yenilendi:", "Representation contract renewed:");



            result = result.Replace("Bonservis: ", "Fee: ");
            result = result.Replace("Bonservis:", "Fee:");
            result = result.Replace("Bonservis Bedeli: ", "Transfer Fee: ");
            result = result.Replace("Bonservis Bedeli:", "Transfer Fee:");

            // Cooldown warning texts
            result = result.Replace("Hf", "Wk");

            // Feedback warnings
            result = result.Replace("HATA: Lütfen ad soyad ve şirket ismi alanlarını boş bırakmayın! Her iki alana da en az 1 karakter girilmelidir.", "ERROR: Please do not leave name/surname and company name fields blank! Both fields must contain at least 1 character.");
            result = result.Replace("HATA: Ad soyad ve şirket/ajans ismi en az 1 adet harf içermelidir!", "ERROR: Name/surname and company/agency name must contain at least 1 letter!");
            result = result.Replace("Ajans kapasitesi dolu", "Agency capacity is full");
            result = result.Replace("Yeni bir oyuncuyu ajansa katabilmek için ajans seviyenizi yükseltmeli veya mevcut bir oyuncuyla yollarınızı ayırmalısınız.", "To add a new player, you must upgrade your agency level or release an existing player.");
            result = result.Replace("Ajans seviyeniz bu oyuncuyu temsil etmek için yetersiz!", "Your agency level is insufficient to represent this player!");
            result = result.Replace("seviyesindeki bir oyuncuyla anlaşabilmek için ajansınızın seviyesini yükseltmelisiniz.", "level player, you must upgrade your agency level.");
            result = result.Replace("Mevcut GEN Sınırı:", "Current OVR Limit:");
            result = result.Replace("Gereken Seviye GEN sınırı:", "Required Level OVR Limit:");
            result = result.Replace("GEN: ", "OVR: ");
            result = result.Replace("GEN ", "OVR ");
            result = result.Replace("Gereken GEN: ", "Required OVR: ");

            // Activity Log Replacements
            result = result.Replace("Tüm liglerin 34 haftalık fikstürü oluşturuldu.", "34-week fixture has been generated for all leagues.");
            result = result.Replace("Ajans Başlangıcı: Keşfedilen Serbest Oyuncu", "Agency Start: Discovered Free Agent");
            result = result.Replace("Ajans Başlangıcı: Keşfedilen", "Agency Start: Discovered");
            result = result.Replace("Serbest Oyuncu", "Free Agent");
            result = result.Replace("Şirketi kuruldu. Başlangıç bütçesi:", "Company founded. Starting budget:");
            result = result.Replace("gözlemci raporuna eklendi.", "added to scout report.");
            result = result.Replace("ÖVGÜ: Müşteriniz", "PRAISE: Your client");
            result = result.Replace("övgü dolu sözleriniz üzerine motive oldu (Yeni Mutluluk:", "was motivated by your praise (New Morale:");
            result = result.Replace("PRİM: Müşteriniz", "BONUS: Your client");
            result = result.Replace("ajansınızdan aldığı €15.000 prim ile çok mutlu oldu (Yeni Mutluluk:", "is very happy with the €15,000 bonus from your agency (New Morale:");
            result = result.Replace("ELEŞTİRİ: Müşteriniz", "CRITICISM: Your client");
            result = result.Replace("disiplinsiz davranışları nedeniyle uyarıldı, morali bozuldu (Yeni Mutluluk:", "was warned due to indiscipline, morale went down (New Morale:");
            result = result.Replace("TALEBİ KARŞILADINIZ:", "REQUEST GRANTED:");
            result = result.Replace("için özel antrenör tuttunuz. (Bütçe:", "hired special coach for. (Budget:");
            result = result.Replace("Moral:", "Morale:");
            result = result.Replace("Temsilcilik Sözleşmesi İmzalandı:", "Representation Contract Signed:");
            result = result.Replace("Müşteriniz", "Your client");
            result = result.Replace("kulübüne kiralık transfer oldu! (Kiralama Bedeli:", "transferred on loan to! (Loan Fee:");
            result = result.Replace("Haftalık Maaş:", "Weekly Wage:");
            result = result.Replace("kulübüne transfer oldu! (Bonservis:", "transferred to! (Fee:");
            result = result.Replace("Sponsorluk Anlaşması:", "Sponsorship Deal:");
            result = result.Replace("ile haftalık €", "with weekly €");
            result = result.Replace("değerinde sponsorluk imzaladı. Komisyonunuz:", "sponsorship signed. Your commission:");
            result = result.Replace("TALEBİ REDDETTİNİZ:", "REQUEST REJECTED:");
            result = result.Replace("adlı oyuncunun isteğini reddettiniz.", "rejected request of.");
            result = result.Replace("GÖZLEMCİ GÖREVİ:", "SCOUT MISSION:");
            result = result.Replace("araştırmasına gönderildi (4 Hafta sürecek).", "sent to research (takes 4 weeks).");
            result = result.Replace("GÖZLEMCİ RAPORU HAZIR:", "SCOUT REPORT READY:");
            result = result.Replace("araştırmasını tamamladı.", "completed research.");
            result = result.Replace("yeni yetenek keşfetti.", "new talents discovered.");
            result = result.Replace("Hata: Prim vermek için yetersiz bütçe (Gereken: €15.000).", "Error: Insufficient budget to give bonus (Required: €15,000).");
            result = result.Replace("Sözleşme başarısız:", "Contract failed:");
            result = result.Replace("yüksek seviyeli bir oyuncu. Gereken Seviye GEN sınırı:", "high level player. Required Level OVR Limit:");
            result = result.Replace("Müşteri Kapasitesi:", "Client Capacity:");
            result = result.Replace("Maksimum Temsil Gücü:", "Max Represent Power:");
            result = result.Replace("Personel Limiti:", "Staff Limit:");
            result = result.Replace("Komisyon Gelirleri:", "Commission Income:");
            result = result.Replace("Net Haftalık Gelir:", "Net Weekly Income:");

            // Crisis & Event & Request Mail translations
            result = result.Replace("📩 Temsilcilik Sözleşmesi Yenileme Uyarısı:", "📩 Representation Contract Renewal Alert:");
            result = result.Replace("ile yaptığınız temsilcilik sözleşmesinin bitmesine 6 ay (26 hafta) kaldı! Sözleşmeyi yenilemek ister misiniz?", "Your representation contract with has only 6 months (26 weeks) remaining! Do you want to renew it?");
            result = result.Replace("En fazla 5 yıl uzatabilirsiniz. 4. yıldan itibaren (sözleşme süresi bitmeden) yenileme teklifi yapabilirsiniz.", "You can extend it by up to 5 years. You can make a renewal offer starting from the 4th year.");
            result = result.Replace("🔴 Temsilcilik Sözleşmesi Sona Erdi:", "🔴 Representation Contract Expired:");
            result = result.Replace("ile olan temsilcilik sözleşmeniz bitti ve oyuncu ajansımızdan ayrıldı.", "representation contract has expired and the player has left our agency.");
            result = result.Replace("Ajans Bildirim Sistemi", "Agency Notification System");
            result = result.Replace("KİRALIK SONU:", "LOAN END:");
            result = result.Replace("kiralık sözleşmesi bittiği için", "has returned because his loan contract expired.");
            result = result.Replace("kulübüne geri döndü.", "club.");
            result = result.Replace("🚨 KRİZ:", "🚨 CRISIS:");
            result = result.Replace("Çok Mutsuz!", "Very Unhappy!");
            result = result.Replace("Kulüpsüz ve boşta kalmaktan dolayı son derece mutsuzum! Menajerim olarak neden bana kulüp bulmuyorsunuz? Acilen bana bir takım ayarlayın, yoksa menajerlik sözleşmemizi tek taraflı feshedeceğim!", "I am extremely unhappy about being clubless and free! Why aren't you finding me a club as my agent? Get me a team immediately, or I will terminate our agency contract unilaterally!");
            result = result.Replace("bünyesinde neredeyse hiç süre alamıyorum! Kadro rolümün hakkı verilmiyor. Bana acilen oynayabileceğim yeni bir takım bulmalısınız ya da bu duruma müdahale etmelisiniz!", "I am getting almost no playing time! My squad role is not respected. You must find me a new team where I can play immediately or intervene in this situation!");
            result = result.Replace("Kulübümdeki mevcut konumumdan ve aldığım maaştan ötürü huzursuzum. Bana yeterince ilgi göstermiyorsunuz. Lütfen benimle ilgilenin, moral verin veya prim vererek yanımda olduğunuzu gösterin!", "I am uneasy about my current position and the wage I receive. You are not paying enough attention to me. Please take care of me, praise me, or give me a bonus to show you are by my side!");
            result = result.Replace("ℹ️ Emeklilik Bildirimi:", "ℹ️ Retirement Notification:");
            result = result.Replace("bu sezonun sonunda profesyonel futbol kariyerini sonlandıracağını açıkladı. Sezon sonunda ajansımızdan ve oyundan tamamen silinecektir.", "announced that he will retire at the end of this season. He will be removed from our agency and the game at the end of the season.");
            result = result.Replace("EMEKLİLİK KARARI! Müşterimiz", "RETIREMENT DECISION! Our client");
            result = result.Replace("sezon sonunda futbolu bırakma kararı aldı.", "decided to retire at the end of the season.");
            result = result.Replace("💼 SPONSORLUK TEKLİFİ:", "💼 SPONSORSHIP OFFER:");
            result = result.Replace("Sponsorluk Departmanı", "Sponsorship Department");
            result = result.Replace("TEKLİF: Müşteriniz", "OFFER: Sponsorship offer for your client");
            result = result.Replace("markasından sponsorluk teklifi geldi!", "sponsorship offer received!");
            // Player Requests translations
            result = result.Replace("📣 PR Kampanyası İsteği:", "📣 PR Campaign Request:");
            result = result.Replace("Menajerim, son zamanlardaki harika performansım ve yüksek moralim sayesinde sosyal medyada büyük ilgi görüyorum. Bu rüzgarı arkamıza alıp marka değerimi yükseltmek için profesyonel bir PR ajansıyla çalışmak harika olur. PR bütçesi için €5.000 ayırabilir miyiz?", "Manager, thanks to my recent great performance and high morale, I am getting a lot of attention on social media. It would be great to take this wind behind us and work with a professional PR agency to increase my brand value. Can we allocate a €5,000 PR budget for this?");

            result = result.Replace("🏋️ Özel Bireysel Antrenör Talebi:", "🏋️ Special Personal Coach Request:");
            result = result.Replace("Harika durumdayım ve sınırlarımı daha da zorlamak istiyorum! Kendimi fiziksel olarak bir üst seviyeye taşımak adına bireysel atletik antrenör tutmak istiyorum. Aylık €8.000 bütçeyi ajansımızın üstlenmesini rica ediyorum.", "I am in great shape and I want to push my limits even further! I want to hire a personal athletic trainer to take myself physically to the next level. I request our agency to cover the monthly €8,000 budget.");

            result = result.Replace("🤝 Sosyal Sorumluluk Projesi Bağışı:", "🤝 Charity Project Donation:");
            result = result.Replace("Moralim çok yüksek ve bu olumlu enerjiyi topluma aktarmak istiyorum. Benim adıma çocuk esirgeme kurumuna €6.000 bağış yapıp bunu basına duyurursak hem prestijimiz artar hem de taraftarlarla bağımız güçlenir.", "My morale is very high and I want to transmit this positive energy to society. If we donate €6,000 to the child protection agency on my behalf and announce it to the press, both our prestige will increase and our bond with the fans will strengthen.");

            result = result.Replace("💰 Performans Sadakat Primi İsteği:", "💰 Performance Loyalty Bonus Request:");
            result = result.Replace("Menajerim, takıma kattığım yüksek katma değer ve moral sayesinde kendimi özel hissediyorum. Sözleşmemize ekstra €10.000 sadakat/imza primi eklemenizi talep ediyorum.", "Manager, I feel special thanks to the high value and morale I bring to the team. I request you to add an extra €10,000 loyalty/signing bonus to our contract.");

            result = result.Replace("👟 Özel Tasarım Krampon Desteği:", "👟 Special Design Boots Support:");
            result = result.Replace("Menajerim, yeni sezon için performansımı artıracak özel karbon tabanlı kramponlar sipariş ettim. €1.500 tutarındaki faturayı ajansımın ödemesini rica ediyorum.", "Manager, I ordered special carbon-soled boots to increase my performance for the new season. I request my agency to pay the €1,500 invoice.");

            result = result.Replace("🏠 Taşınma ve Ev Desteği:", "🏠 Relocation and Housing Support:");
            result = result.Replace("Tesislerimize daha yakın ve sessiz bir muhite taşınmaya karar verdim. Nakliye ve emlakçı komisyonu gibi taşınma masraflarım için ajansımdan €3.000 destek talep ediyorum.", "I decided to move to a quieter neighborhood closer to our facilities. I request €3,000 support from my agency for my relocation expenses such as shipping and real estate agent commission.");

            result = result.Replace("🩺 Özel Fizyoterapist Desteği:", "🩺 Special Physiotherapist Support:");
            result = result.Replace("Kas sakatlıklarından korunmak ve maç sonu toparlanma süremi hızlandırmak için özel bir fizyoterapist ile anlaştım. Haftalık seans bedeli olan €4.000 tutarını karşılamanızı bekliyorum.", "I agreed with a private physiotherapist to prevent muscle injuries and speed up my recovery time after the match. I expect you to cover the €4,000 weekly session fee.");

            result = result.Replace("✈️ Mental İzin ve Aile Ziyareti Desteği:", "✈️ Mental Leave & Family Visit Support:");
            result = result.Replace("Son zamanlarda kendimi hiç iyi hissetmiyorum. Sahadaki form düşüklüğüm beni yıprattı. Hafta sonu kafa dağıtmak ve ailemi ziyaret etmek için bana özel uçuş ve tatil planı hazırlamanızı rica ediyorum. Maliyeti €2.000.", "I haven't been feeling well lately. My low form on the pitch has worn me out. I request you to prepare a special flight and holiday plan for me to clear my head and visit my family this weekend. The cost is €2,000.");

            result = result.Replace("💼 Maaş İyileştirme Görüşmesi Talebi:", "💼 Wage Adjustment Meeting Request:");
            result = result.Replace("Mevcut kulübümden aldığım maaşın yetersiz kaldığını düşünüyorum ve bu durum moralimi bozuyor. Menajerim olarak kulüple acilen masaya oturup maaşıma zam istemenizi talep ediyorum.", "I think the salary I receive from my current club is insufficient and this ruins my morale. As my agent, I request you to sit down with the club immediately and ask for a raise.");

            result = result.Replace("🤝 Yeni Sponsor Arayışı Talebi:", "🤝 New Sponsor Search Request:");
            result = result.Replace("Ekstra gelir elde edememek canımı sıkıyor. Bana acilen yeni bir sponsor markası bulmanızı istiyorum, marka elçisi olarak kendimi göstermeye hazırım.", "Not having extra income bothers me. I want you to find me a new sponsor brand immediately, I am ready to show myself as a brand ambassador.");

            result = result.Replace("TALEPLER: Müşteriniz", "REQUESTS: Your client");
            result = result.Replace("sizden bir talepte bulundu! Gelen kutusunu inceleyin.", "made a request! Please check your inbox.");
            result = result.Replace("🤝 Yeni Sponsor Arayışı Talebi:", "🤝 New Sponsor Search Request:");
            result = result.Replace("Ekstra gelir elde edememek canımı sıkıyor. Bana acilen yeni bir sponsor markası bulmanızı istiyorum, marka elçisi olarak kendimi göstermeye hazırım.", "Not having extra income bothers me. I want you to find me a new sponsor brand immediately, I am ready to show myself as a brand ambassador.");
            result = result.Replace("🚨 GECE KULÜBÜ OLAYI:", "🚨 NIGHTCLUB INCIDENT:");
            result = result.Replace("dün gece şehir merkezindeki bir gece kulübünde kavgaya karıştı ve karakola götürüldü. Kulüp yönetimi oyuncuya disiplin cezası uyguladı ve morali son derece bozuk.", "was involved in a fight at a nightclub downtown last night and was taken to the police station. The club management fined the player, and his morale is extremely low.");
            result = result.Replace("🩹 SAKATLIK DEPRESYONU:", "🩹 INJURY DEPRESSION:");
            result = result.Replace("antrenmanda dizinden ciddi bir sakatlık yaşadı. Doktorlar sahalardan bir süre uzak kalacağını belirtti. Oyuncu bu durumdan dolayı psikolojik olarak çökmüş durumda.", "suffered a serious knee injury in training. Doctors said he will be out for some time. The player is psychologically down.");
            result = result.Replace("🤬 TARAFTAR LİNCİ:", "🤬 FAN BACKLASH:");
            result = result.Replace("Son maçta kaçırdığı net pozisyonların ardından taraftarlar sosyal medyada", "Following missed chances in the last match, fans launched a backlash on social media against");
            result = result.Replace("için linç kampanyası başlattı. Oyuncu gelen hakaretler yüzünden yorumları kapatmak zorunda kaldı.", "The player had to turn off comments due to insults.");
            result = result.Replace("⚠️ HOCA İLE TARTIŞMA:", "⚠️ ARGUMENT WITH COACH:");
            result = result.Replace("son antrenmanda teknik direktörün taktik kararlarını herkesin önünde eleştirdiği için kadro dışı bırakıldı. Süresiz olarak altyapıyla antrenmanlara çıkacak.", "was excluded from the squad for criticizing the manager's tactical decisions in front of everyone. He will train with the youth team indefinitely.");
            result = result.Replace("🥊 ANTREMANDA KAVGA:", "🥊 FIGHT IN TRAINING:");
            result = result.Replace("Dünkü idmanda çift kale maç sırasında", "During yesterday's training match,");
            result = result.Replace("takım arkadaşıyla sert bir ikili mücadeleye girdi ve kavga çıktı. Kulüp her iki oyuncuya da ağır para cezası kesti.", "got into a harsh duel with his teammate and a fight broke out. The club heavily fined both players.");
            result = result.Replace("❌ MİLLİ TAKIM HAYAL KIRIKLIĞI:", "❌ NATIONAL TEAM DISAPPOINTMENT:");
            result = result.Replace("Milli takımın son aday kadrosu açıklandı ve", "The national team squad was announced and");
            result = result.Replace("davet edilmedi. Çok uzun süredir bu kadroyu bekleyen oyuncunuz büyük bir motivasyon kaybı yaşıyor.", "was not invited. Your player, who has been waiting for this for a long time, has lost motivation.");
            result = result.Replace("📹 SOSYAL MEDYA SKANDALI:", "📹 SOCIAL MEDIA SCANDAL:");
            result = result.Replace("dün katıldığı canlı yayında mikrofonun açık olduğunu unutarak kulüp yöneticileri hakkında argo ifadeler kullandı. Yönetim acil disiplin kurulu topladı.", "forgot the microphone was on in a live stream yesterday and used slang expressions about club executives. The board held an emergency meeting.");
            result = result.Replace("🗞️ Dedikodular:", "🗞️ Rumors:");
            result = result.Replace("medyada", "in media");
            result = result.Replace("in kulüpten ayrılmak istediğine dair asılsız iddialar yer aldı. Oyuncu kulüple taraftar arasında kalmaktan dolayı büyük bir zihinsel yıpranma yaşıyor.", "claims that he wants to leave the club were published. The player is suffering from being stuck between the club and the fans.");
            result = result.Replace("💔 ÖZEL HAYAT KRİZİ:", "💔 PRIVATE LIFE CRISIS:");
            result = result.Replace("uzun süredir birlikte olduğu kız arkadaşıyla yollarını ayırdı. Antrenmanlarda konsantre olmakta büyük zorluk çekiyor.", "broke up with his girlfriend. He is having trouble concentrating in training.");
            result = result.Replace("⚖️ KONDİSYON VE KİLO ELEŞTİRİLERİ:", "⚖️ FITNESS & WEIGHT CRITICISM:");
            result = result.Replace("Spor basını, son maçlarda", "The sports press wrote that in recent matches");
            result = result.Replace("in kilo aldığını ve fiziksel olarak çok geride kaldığını yazdı. Oyuncunun özgüveni sarsılmış durumda.", "gained weight and fell physically behind. The player's self-confidence is shaken.");
            result = result.Replace("💉 DOPİNG TESTİ GERGİNLİĞİ:", "💉 DOPING TEST TENSION:");
            result = result.Replace("Rutin doping kontrolü sırasında", "During routine doping control, a technical error occurred in");
            result = result.Replace("in numunesinde teknik bir hata oluştu ve test tekrarlandı. Aklanmış olsa da oyuncu süreç boyunca ciddi bir panik yaşadı.", "s sample and the test was repeated. Although cleared, the player panicked during the process.");
            result = result.Replace("✈️ MEMLEKET HASRETİ:", "✈️ HOMESICKNESS:");
            result = result.Replace("Şehirdeki yeni yaşamına ve takımın kültürüne bir türlü adapte olamayan", "Unable to adapt to his new life and club culture,");
            result = result.Replace("memleket hasreti çektiğini ve yalnız hissettiğini sizinle paylaştı.", "shared that he is homesick and feels lonely.");
            result = result.Replace("🪑 YEDEK KULÜBESİNDE UNUTULMA:", "🪑 FORGOTTEN ON THE BENCH:");
            result = result.Replace("Haftalardır yedek kulübesinden çıkamayan", "Unable to leave the bench for weeks,");
            result = result.Replace("artık teknik direktörün kendisini tamamen gözden çıkardığını düşünüyor ve pes etme aşamasında.", "feels the manager has completely given up on him and is about to give up.");
            result = result.Replace("🌟 HAFTANIN 11'İNE SEÇİLME SEVİNCİ:", "🌟 TEAM OF THE WEEK JOY:");
            result = result.Replace("Harika! Oyuncunuz", "Great! Your player");
            result = result.Replace("sergilediği üstün performansla ligde haftanın altın 11'ine seçildi. Morali ve kendine güveni zirve yapmış durumda.", "was selected to the team of the week with his outstanding performance. His morale and confidence are at peak.");
            result = result.Replace("🎁 SPONSOR JESTİ:", "🎁 SPONSOR GESTURE:");
            result = result.Replace("Aktif sponsor markası,", "The active sponsor brand rewarded");
            result = result.Replace("in son dönemdeki profesyonel duruşunu ödüllendirerek kendisine lüks bir saat hediye etti. Oyuncumuzun keyfi yerinde.", "for his professional stance and gifted him a luxury watch. He is happy.");
            result = result.Replace("🚗 MADDİ HASARLI TRAFİK KAZASI:", "🚗 TRAFFIC ACCIDENT:");
            result = result.Replace("sabah antrenmanına gelirken ufak bir zincirleme kazaya karıştı. Fiziksel bir hasarı yok ancak psikolojik olarak sarsıldı.", "got into a minor accident on his way to morning training. He has no physical injuries but is shaken.");
            result = result.Replace("🏠 EVİNE HIRSIZ GİRMESİ ŞOKU:", "🏠 HOUSE BURGLARY SHOCK:");
            result = result.Replace("Takımla deplasmandayken", "While away with the team,");
            result = result.Replace("in evine hırsız girdi ve değerli eşyaları çalındı. Oyuncu güvenlik endişesi yüzünden çok huzursuz.", "s house was burglarized and valuables were stolen. The player is uneasy due to safety concerns.");
            result = result.Replace("🎤 TEKNİK DİREKTÖRDEN ÖVGÜ:", "🎤 PRAISE FROM MANAGER:");
            result = result.Replace("Teknik direktör dünkü basın toplantısında", "The manager praised");
            result = result.Replace("hakkında övgü dolu sözler sarf ederek onun takımın geleceği olduğunu vurguladı.", "in yesterday's press conference, stating he is the future of the team.");
            result = result.Replace("⚽ KARİYER REKORU VE HAT-TRICK:", "⚽ CAREER RECORD & HAT-TRICK:");
            result = result.Replace("İnanılmaz! Oyuncunuz", "Incredible! Your player");
            result = result.Replace("son maçta 3 gol birden atarak kariyerinin ilk hat-trick'ine imza attı. Taraftarlar onun adını haykırıyor.", "scored 3 goals in the last match, making his first career hat-trick. Fans are shouting his name.");
            result = result.Replace("🤢 GIDA ZEHİRLENMESİ:", "🤢 FOOD POISONING:");
            result = result.Replace("Takım yemeği sonrası şiddetli mide ağrısıyla hastaneye kaldırılan", "Hospitalized with severe stomach pain after the team meal,");
            result = result.Replace("e gıda zehirlenmesi teşhisi konuldu. Hafta sonu maçta oynaması zor görünüyor.", "was diagnosed with food poisoning. It looks difficult for him to play this weekend.");
            result = result.Replace("Ajans Olay Bildirimi", "Agency Event Notification");

            // dynamic replacements for sponsor negotiation & club negotiation
            result = result.Replace(" Temsilcisi: '", " Representative: '");
            result = result.Replace("'Talep ettiğiniz %", "'The %");
            result = result.Replace(" komisyon oranı bizim için çok fazla. En fazla %", " commission rate you demanded is too much for us. We can accept at most %");
            result = result.Replace(" kabul edebiliriz!'", "!'");
            result = result.Replace("'İmza primi talebiniz (", "'Your signing bonus demand (");
            result = result.Replace(") marka bütçemizi aşıyor! Maksimum €", ") exceeds our brand budget! We can pay maximum €");
            result = result.Replace(" prim ödeyebiliriz.'", " as a bonus.'");
            result = result.Replace("Sponsor yetkilileri taleplerinizi bekliyor.", "Sponsor executives are waiting for your demands.");
            result = result.Replace("Görüşmeler olumlu geçiyor. Kulübün taleplerinizi değerlendirmesini isteyin.", "Negotiations are going well. Ask the club to evaluate your demands.");
            result = result.Replace(" Yetkilisi: '", " Representative: '");
            result = result.Replace("Oyuncu için talep ettiğiniz kadro rolü (", "The squad role you demanded for the player (");
            result = result.Replace("), planladığımız rolün (", ") is way above what we planned (");
            result = result.Replace(") çok üzerinde. Bu teklifi kabul edemeyiz!'", "). We cannot accept this offer!'");
            result = result.Replace("Bu oyuncunun kalitesi (", "The quality of this player (");
            result = result.Replace(" GEN) kadromuzda Yıldız rolü almak için yetersiz. En fazla Önemli Oyuncu olabilir!", " OVR) is insufficient for a Star role in our squad. He can be an Important Player at most!");
            result = result.Replace("Bu oyuncunun kalitesi (", "The quality of this player (");
            result = result.Replace(" OVR) kadromuzda Yıldız rolü almak için yetersiz. En fazla Önemli Oyuncu olabilir!", " OVR) is insufficient for a Star role in our squad. He can be an Important Player at most!");
            result = result.Replace("Bu oyuncu bu kadroda Yıldız Oyuncu olamaz! En fazla Önemli Oyuncu rolü verebiliriz.", "This player cannot be a Star Player in this squad! We can give Important Player role at most.");
            result = result.Replace("Bu oyuncu bu kadroda Önemli Oyuncu olamaz! En fazla İlk 11 Oyuncusu rolü verebiliriz.", "This player cannot be an Important Player in this squad! We can give First Team Player role at most.");
            result = result.Replace("Maaş talebiniz (", "Your wage demand (");
            result = result.Replace(") bu rol için bütçe sınırlarimizi (", ") exceeds our budget limits (");
            result = result.Replace(") aşıyor! Lütfen talebinizi düşürün.", ") for this role! Please lower your demand.");
            result = result.Replace("Talep ettiğiniz imza parası (", "Your signing bonus demand (");
            result = result.Replace(") bütçe limitlerimizi (", ") exceeds our budget limits (");
            result = result.Replace(") aşıyor. Lütfen teklifinizi düşürün.", "). Please lower your offer.");
            result = result.Replace("Talep ettiğiniz imza parası (€", "Your signing bonus demand (€");
            result = result.Replace(") bütçe limitlerimizi (€", ") exceeds our budget limits (€");
            result = result.Replace(") aşıyor. Lütfen teklifinizi düşürün.", "). Please lower your offer.");
            result = result.Replace("Oyuncu için talep ettiğiniz haftalık maaş (", "The weekly wage you demanded for the player (");
            result = result.Replace(") maaş limitlerimizi (", ") exceeds our wage limits (");
            result = result.Replace(") aşıyor!", ")!");
            result = result.Replace("Talep ettiğiniz imza parası (", "Your signing bonus demand (");
            result = result.Replace(") bütçe limitlerimizi (", ") exceeds our budget limits (");
            result = result.Replace(") aşıyor!", ")!");
            result = result.Replace("Müşteriniz ", "Your client ");
            result = result.Replace(" için ", " for ");
            result = result.Replace(" kulübünden gelen kiralık teklifinin şartlarını görüşüyorsunuz.\nMaaşını mevcut kulübü ödemeye devam edecektir.", " is negotiating the terms of the loan offer from. \nHis current club will continue to pay his salary.");
            result = result.Replace(" kulübü ile sözleşme şartlarını görüşüyorsunuz.\nKulübün transfer bütçesi: ", " club. \nClub's transfer budget: ");
            result = result.Replace(" Kiralama Görüşmesi", " Loan Negotiation");
            result = result.Replace(" Transfer Görüşmesi", " Transfer Negotiation");
            result = result.Replace(" Anlaşma Türü:", " Agreement Type:");
            result = result.Replace(" Bonservis Bedeli:", " Transfer Fee:");
            result = result.Replace("Kiralık (Mevcut Kulübü Öder)", "Loan (Paid by Current Club)");
            result = result.Replace(" (Kilitli)", " (Locked)");
            result = result.Replace("Önerilen Rol:", "Proposed Role:");
            result = result.Replace("Haftalık Maaş:", "Weekly Wage:");
            result = result.Replace("Temsilcilik sözleşmesi yenilendi:", "Representation contract renewed:");
            result = result.Replace(" Kiralama Teklifi", " Loan Offer");
            result = result.Replace(" Transfer Teklifi", " Transfer Offer");
            result = result.Replace("Kulüp Anlaşması:", "Club Deal:");
            result = result.Replace(" kulübü ile ", " with ");
            result = result.Replace(" rolünde haftalık ", " in role, weekly ");
            result = result.Replace(" karşılığında ", " for ");
            result = result.Replace(" yıllık sözleşme imzaladı! Ajansımız ", " years contract signed! Our agency earned ");
            result = result.Replace(" imza parası kazandı!", " signing bonus!");

            // Dynamic country & league translations within dynamic strings
            foreach (var kp in countryTranslations)
            {
                result = result.Replace(kp.Key, kp.Value);
            }
            foreach (var kp in leagueTranslations)
            {
                result = result.Replace(kp.Key, kp.Value);
            }

            return result;
        }

        public static string TranslateClub(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            if (CurrentLanguage == "TR") return name;

            if (clubTranslations.TryGetValue(name, out string val))
            {
                return val;
            }

            return name;
        }

        public static string TranslateLeague(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            if (CurrentLanguage == "TR") return name;

            if (leagueTranslations.TryGetValue(name, out string val))
            {
                return val;
            }

            return name;
        }

        public static string TranslateCountry(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            if (CurrentLanguage == "TR") return name;

            if (countryTranslations.TryGetValue(name, out string val))
            {
                return val;
            }

            return name;
        }

        private static Dictionary<string, string> uiTranslations = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "PUAN DURUMU", "STANDINGS" },
            { "⚽ PUAN DURUMU", "⚽ STANDINGS" },
            { "⚽ Transfer: Açık", "⚽ Transfer: Open" },
            { "⚽ Transfer: Kapalı", "⚽ Transfer: Closed" },
            { "LİGLER LİSTESİ", "LEAGUE LIST" },
            { "KAPAT", "CLOSE" },
            { "KULÜP TEKLİFLERİ & SÖZLEŞME", "CLUB OFFERS & CONTRACT" },
            { "TAMAM", "OK" },
            { "AÇIK", "ON" },
            { "KAPALI", "OFF" },
            { "MÜZİK AÇ / KAPAT:", "MUSIC ON / OFF:" },
            { "MÜZİK SES SEVİYESİ:", "MUSIC VOLUME:" },
            { "DİL / LANGUAGE:", "LANGUAGE:" },
            { "İPTAL", "CANCEL" },
            { "EVET", "YES" },
            { "HAYIR", "NO" },
            { "YENİ OYUN", "NEW GAME" },
            { "AYARLAR", "SETTINGS" },
            { "OYUNDAN ÇIKIŞ", "EXIT GAME" },
            { "PERDE ARKASI FUTBOL", "BEHIND THE SCENES FOOTBALL" },
            { "KAYITLI OYUN YÜKLE", "LOAD SAVED GAME" },
            { "ÇIKIŞ", "EXIT" },
            { "MENAJERLİK KURULUMU", "MANAGER SETUP" },
            { "HATA: Lütfen ad soyad ve şirket ismi alanlarını boş bırakmayın! Her iki alana da en az 1 karakter girilmelidir.", "ERROR: Please do not leave Name Surname and Company Name fields blank! At least 1 character must be entered in both fields." },
            { "HATA: Ad soyad ve şirket/ajans ismi en az 1 adet harf içermelidir!", "ERROR: Name Surname and Company/Agency name must contain at least 1 letter!" },
            { "Müzik Ses Seviyesi", "Music Volume" },
            { "OYUNU KAYDET", "SAVE GAME" },
            { "Slot 1 [BOŞ]", "Slot 1 [EMPTY]" },
            { "Slot 2 [BOŞ]", "Slot 2 [EMPTY]" },
            { "Slot 3 [BOŞ]", "Slot 3 [EMPTY]" },
            { "Slot 1 [KAYITLI]", "Slot 1 [SAVED]" },
            { "Slot 2 [KAYITLI]", "Slot 2 [SAVED]" },
            { "Slot 3 [KAYITLI]", "Slot 3 [SAVED]" },
            { "Oyun Slot 1'e başarıyla kaydedildi!", "Game successfully saved to Slot 1!" },
            { "Oyun Slot 2'e başarıyla kaydedildi!", "Game successfully saved to Slot 2!" },
            { "Oyun Slot 3'e başarıyla kaydedildi!", "Game successfully saved to Slot 3!" },
            { "Slot 1'deki oyun başarıyla yüklendi!", "Game from Slot 1 successfully loaded!" },
            { "Slot 2'deki oyun başarıyla yüklendi!", "Game from Slot 2 successfully loaded!" },
            { "Slot 3'deki oyun başarıyla yüklendi!", "Game from Slot 3 successfully loaded!" },
            { "GERİ DÖN", "BACK" },
            { "AD SOYAD", "NAME SURNAME" },
            { "ŞİRKET / AJANS İSMİ", "COMPANY / AGENCY NAME" },
            { "OYUNA BAŞLA ▶", "START GAME ▶" },
            { "Ad Soyad girin...", "Enter Name Surname..." },
            { "Şirket ismini girin...", "Enter Company Name..." },
            { "Bütçe", "Budget" },
            { "Hafta", "Week" },
            { "Menü", "Menu" },
            { "Müşterilerim", "My Clients" },
            { "Gözlemci Merkezi", "Scouting Center" },
            { "Haberler", "News" },
            { "Finans", "Finance" },
            { "Özel Hayat", "Private Life" },
            { "Mağaza", "Store" },
            { "Tüm Oyuncular", "All Players" },
            { "Ligler", "Leagues" },
            { "Kulüpler", "Clubs" },
            { "Transferler", "Transfers" },
            { "HAFTAYI İLERLET ▶", "PROCEED WEEK ▶" },
            { "GELEN KUTUSU BOŞ", "INBOX EMPTY" },
            { "Şu an aktif bir transfer teklifi veya özel mesaj bulunmuyor.", "There are no active transfer offers or private messages at the moment." },
            { "KABUL ET", "ACCEPT" },
            { "PAZARLIK ET", "NEGOTIATE" },
            { "REDDET", "REJECT" },
            { "SÖZLEŞME YENİLE", "RENEW CONTRACT" },
            { "OKUNDU İŞARETLE", "MARK AS READ" },
            { "OKUNDU OLARAK İŞARETLE", "MARK AS READ" },
            { "BOŞTA", "IDLE" },
            { "LİGE GÖNDER", "SEND TO LEAGUE" },
            { "ARAMA YAPIYOR", "SEARCHING" },
            { "RAPOR", "REPORT" },
            { "YENİ GÖREV", "NEW MISSION" },
            { "GERİ DÖN (İPTAL)", "BACK (CANCEL)" },
            { "GÖZLEMCİLERE GERİ DÖN", "BACK TO SCOUTS" },
            { "Bu raporda henüz aday oyuncu bulunmamaktadır veya tamamı sözleşme imzalanmıştır.", "There are no candidate players in this report yet, or all of them have signed contracts." },
            { "GÖREV Yolla", "SEND ON MISSION" },
            { "TEMSİL ET", "REPRESENT" },
            { "YOKSAY", "IGNORE" },
            { "FAVORİYE EKLE", "ADD TO FAVORITES" },
            { "FAVORİDEN ÇIKAR", "REMOVE FROM FAVORITES" },
            { "SÖZLEŞME İMZALA", "SIGN CONTRACT" },
            { "TEMAS KUR", "CONTACT" },
            { "AJANS ETKİLEŞİMİ (MUTLULUK)", "AGENCY INTERACTION (HAPPINESS)" },
            { "ÖV", "PRAISE" },
            { "PRİM VER (€15K)", "GIVE BONUS (€15K)" },
            { "UYAR", "WARN" },
            { "FESHET (BIRAK)", "TERMINATE (RELEASE)" },
            { "KULÜBE ÖNER", "SUGGEST TO CLUB" },
            { "KİRALIK ÖNER", "SUGGEST FOR LOAN" },
            { "SERBEST BIRAKILDI", "RELEASED" },
            { "SÖZLEŞMEYİ FESHET (BIRAK)", "TERMINATE CONTRACT (RELEASE)" },
            { "AJANS FİNANS MERKEZİ", "AGENCY FINANCE CENTER" },
            { "Ajans Seviyesi:", "Agency Level:" },
            { "Haftalık Ajans Geliri:", "Weekly Agency Income:" },
            { "Haftalık Personel Gideri:", "Weekly Staff Expense:" },
            { "Net Haftalık Gelir:", "Net Weekly Income:" },
            { "Ajans Kasası:", "Agency Vault:" },
            { "Komisyon Gelirleri:", "Commission Income:" },
            { "Sponsor Komisyonu:", "Sponsor Commission:" },
            { "Maaş Komisyonu:", "Wage Commission:" },
            { "Kasa Özeti:", "Vault Summary:" },
            { "Ajans Seviyesini Yükselt", "Upgrade Agency Level" },
            { "Müşteri Kapasitesi:", "Client Capacity:" },
            { "Maksimum Temsil Gücü:", "Max Represent Power:" },
            { "Personel Limiti:", "Staff Limit:" },
            { "YÜKSELT", "UPGRADE" },
            { "AJANS PRESTİJ & KİŞİSEL SERVET", "AGENCY PRESTIGE & PERSONAL WEALTH" },
            { "Kişisel Servetiniz:", "Your Personal Wealth:" },
            { "Ajans İtibarı (Reputation):", "Agency Reputation:" },
            { "Mülk Satın Al", "Buy Property" },
            { "SAHİPSİNİZ", "OWNED" },
            { "SATIN AL", "BUY" },
            { "LÜKS MAĞAZA", "LUXURY STORE" },
            { "LÜKS MAĞAZA & PRESTİJ", "LUXURY STORE & PRESTIGE" },
            { "LİG DETAYLARI", "LEAGUE DETAILS" },
            { "KULÜP BİLGİLERİ", "CLUB INFORMATION" },
            { "YAPILAN TRANSFERLER", "COMPLETED TRANSFERS" },
            { "OYUNCU PİYASASI", "PLAYER MARKET" },
            { "Temsil Edilen Oyuncu Yok", "No Represented Players" },
            { "Teklifler", "Offers" },
            { "Kadro Rolü:", "Squad Role:" },
            { "Maaş:", "Salary:" },
            { "Mutluluk:", "Morale:" },
            { "Piyasa Değeri:", "Market Value:" },
            { "Kulüp:", "Club:" },
            { "Temsilcilik Pazarlığı", "Representation Negotiation" },
            { "Sözleşme Yenileme", "Contract Renewal" },
            { "Sözleşme süresini uzun tutarsanız komisyon oranlarında esneklik payı artar.", "Longer contracts increase commission negotiation flexibility." },
            { "Transfer Komisyon Payı:", "Transfer Commission Share:" },
            { "Haftalık Maaş Komisyon Payı:", "Weekly Wage Commission Share:" },
            { "Sponsorluk Komisyon Payı:", "Sponsor Commission Share:" },
            { "Sözleşme Süresi (Yıl):", "Contract Duration (Years):" },
            { "TEKLİFİ SUN", "SUBMIT OFFER" },
            { "Dil / Language", "Language" },
            { "TÜRKÇE", "TURKISH" },
            { "İNGİLİZCE", "ENGLISH" },
            { "OYUN İÇİ AYARLAR", "GAME SETTINGS" },
            { "ANA MENÜYE DÖN", "RETURN TO MAIN MENU" },
            { "OYUN DURAKLATILDI", "GAME PAUSED" },
            { "DEVAM ET", "RESUME" },
            { "ÖZEL HAYAT & MÜLKLER", "PRIVATE LIFE & PROPERTIES" },
            { "AJANS BÜTÇESİ & FİNANS", "AGENCY BUDGET & FINANCE" },
            { "HABERLER & E-POSTA", "NEWS & EMAIL" },
            { "Gözlemciler: 3 / 3 (Kapasite Dolu)", "Scouts: 3 / 3 (Capacity Full)" },
            { "İŞE ALINMIŞ GÖZLEMCİ YOK", "NO HIRED SCOUTS" },
            { "SOSYAL MEDYA", "SOCIAL MEDIA" },
            { "<b>✉ SOSYAL MEDYA</b>", "<b>✉ SOCIAL MEDIA</b>" },
            { "Detaylar için dokun...", "Tap for details..." },
            { "SPONSORLUK TEKLİFLERİ", "SPONSORSHIP OFFERS" },
            { "TEKLİFİ İMZALA", "SIGN OFFER" },
            { "ANLAŞMAYI İMZALA", "SIGN AGREEMENT" },
            { "📩 GELEN KUTUSU BOŞ", "📩 INBOX IS EMPTY" },
            { "Yedek Oyuncu", "Backup Player" },
            { "Genç Yetenek", "Young Prospect" },
            { "Rotasyon Oyuncusu", "Rotation Player" },
            { "İlk 11 Oyuncusu", "First Team Player" },
            { "Önemli Oyuncu", "Important Player" },
            { "Yıldız Oyuncu", "Star Player" },
            { "Kadro Rolü: Yok", "Squad Role: None" },
            { "GERİ", "BACK" },
            { "MÜŞTERİLERİM", "MY CLIENTS" },
            { "GÖZLEMCİ MERKEZİ", "SCOUTING CENTER" },
            { "OYUNCU", "PLAYER" },
            { "OYUNCU ▲", "PLAYER ▲" },
            { "OYUNCU ▼", "PLAYER ▼" },
            { "YAŞ", "AGE" },
            { "YAŞ ▲", "AGE ▲" },
            { "YAŞ ▼", "AGE ▼" },
            { "MAÇ", "MATCHES" },
            { "MAÇ ▲", "MATCHES ▲" },
            { "MAÇ ▼", "MATCHES ▼" },
            { "GOL", "GOALS" },
            { "GOL ▲", "GOALS ▲" },
            { "GOL ▼", "GOALS ▼" },
            { "ASİST", "ASSISTS" },
            { "ASİST ▲", "ASSISTS ▲" },
            { "ASİST ▼", "ASSISTS ▼" },
            { "KULÜP DETAYLARI", "CLUB DETAILS" },
            { "LİG SEÇİN:", "SELECT LEAGUE:" },
            { "Sıra", "Pos" },
            { "Takım", "Club" },
            { "O", "P" },
            { "G", "W" },
            { "B", "D" },
            { "M", "L" },
            { "Av", "GD" },
            { "P", "PTS" },
            { "TAKIM SEÇİN:", "SELECT CLUB:" },
            { "ÇEVRİMİÇİ KARŞILAŞMALAR\n\nYakında eklenecek...", "ONLINE MATCHES\n\nComing soon..." },
            { "<b>HAYDİ TRANSFERE!</b>\n\nBu transfer döneminde henüz herhangi bir transfer gerçekleşmedi.", "<b>LET'S TRADE!</b>\n\nNo transfers have occurred in this transfer window yet." }
        };

        private static Dictionary<string, string> storeTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Vehicles (Araçlar)
            { "Yol Bisikleti", "Road Bike" },
            { "Elektrikli Scooter", "Electric Scooter" },
            { "Vespa Primavera", "Vespa Primavera" },
            { "İkinci Hand Hatchback", "Used Hatchback" },
            { "İkinci El Hatchback", "Used Hatchback" },
            { "Sıfır Sedan Aile Arabası", "Brand New Sedan Family Car" },
            { "Off-road ATV", "Off-road ATV" },
            { "Spor Chopper Motosiklet", "Sport Chopper Motorcycle" },
            { "Aile SUV Otomobili", "Family SUV Car" },
            { "Elektrikli Şehir Arabası", "Electric City Car" },
            { "Klasik Cabriolet Roadster", "Classic Cabriolet Roadster" },
            { "Premium Executive Sedan", "Premium Executive Sedan" },
            { "Elektrikli Spor Otomobil", "Electric Sports Car" },
            { "Lüks SUV Otomobil", "Luxury SUV Car" },
            { "Amerikan Klasik Muscle", "American Classic Muscle" },
            { "Grand Tourer Spor Coupe", "Grand Tourer Sports Coupe" },
            { "İtalyan Süper Spor Car", "Italian Super Sports Car" },
            { "Pist Odaklı Yarış Arabası", "Track-focused Racing Car" },
            { "Zırhlı Makam VIP Minibüs", "Armored Executive VIP Van" },
            { "Klasik Koleksiyon Yarış Arabası", "Classic Collection Race Car" },
            { "Lüks Spor Sürat Teknesi", "Luxury Sport Speedboat" },
            { "Özel Yapım Chopper Yat", "Custom Chopper Yacht" },
            { "Süper Yat (Flybridge)", "Superyacht (Flybridge)" },
            { "Çift Motorlu VIP Helikopter", "Twin-Engine VIP Helicopter" },
            { "Özel Jet (Light Jet)", "Private Jet (Light Jet)" },
            { "Özel Jet (Gulfstream G650)", "Private Jet (Gulfstream G650)" },

            // Real Estate (Konutlar)
            { "Paylaşımlı Ofis Odası", "Shared Office Space" },
            { "Stüdyo Daire Kira", "Rental Studio Apartment" },
            { "1+1 Apartman Dairesi", "1+1 Apartment Flat" },
            { "Bahçeli Sıra Ev", "Townhouse with Garden" },
            { "Banliyö Müstakil Ev", "Suburban Detached House" },
            { "Göl Kenarı Dağ Evi", "Lakeside Cabin" },
            { "Restorasyonlu Taş Ev", "Restored Stone House" },
            { "Orman Eko-Villası", "Forest Eco-Villa" },
            { "Modern Dubleks Daire", "Modern Duplex Apartment" },
            { "Tarihi Yarımada Dairesi", "Historical Peninsula Apartment" },
            { "Şehir Merkezi Penthouse Daire", "Downtown Penthouse Apartment" },
            { "Boğaz Manzaralı Loft Daire", "Bosphorus View Loft Apartment" },
            { "Havuzlu Modern Villa", "Modern Villa with Pool" },
            { "Dağ Yamacı Akıllı Villa", "Mountain Slope Smart Villa" },
            { "Tarihi Yel Değirmeni Konut", "Historical Windmill Home" },
            { "Akdeniz Kıyısında Malikane", "Mediterranean Coast Mansion" },
            { "Özel Tasarım Kanyon Evi", "Custom Design Canyon House" },
            { "Alp Dağlarında Lüks Şale", "Luxury Chalet in the Alps" },
            { "Tarihî Konak Malikane", "Historical Mansion House" },
            { "Boğazda Yalı", "Bosphorus Waterfront Mansion" },
            { "Özel Tropik Ada", "Private Tropical Island" },
            { "Orta Çağ Şatosu", "Medieval Castle" },
            { "Miami Beach Sahil Sarayı", "Miami Beach Oceanfront Palace" },
            { "Mega Gökdelen Çatı Katı (Penthouse)", "Mega Skyscraper Penthouse" },
            { "Özel İklim Kubbeli Saray Kompleksi", "Private Climate Dome Palace Complex" },

            // Luxury (Lüks & Mobilya)
            { "Ergonomik Ofis Koltuğu", "Ergonomic Office Chair" },
            { "Minimalist Çalışma Masası", "Minimalist Study Desk" },
            { "Akıllı Masa Lambası", "Smart Desk Lamp" },
            { "Kahve Demleme İstasyonu", "Coffee Brewing Station" },
            { "Tasarım Kitaplık", "Designer Bookcase" },
            { "Deri Dinlenme Koltuğu", "Leather Recliner" },
            { "Akustik Ses Sistemi", "Acoustic Sound System" },
            { "Havalı Süspansiyonlu Yatak", "Air Suspension Bed" },
            { "Modern İtalyan Sehpa", "Modern Italian Coffee Table" },
            { "Antika Duvar Saati", "Antique Wall Clock" },
            { "Tasarım Yemek Masası Seti", "Designer Dining Table Set" },
            { "Özel Tasarım Kitap Okuma Köşesi", "Custom Reading Nook" },
            { "El Dokuması İpek Halı", "Hand-woven Silk Carpet" },
            { "Akıllı Ev Kontrol Paneli Seti", "Smart Home Control Panel Kit" },
            { "Özel Ev Sinema Projeksiyonu", "Custom Home Cinema Projector" },
            { "Premium Şarap Kavı Dolabı", "Premium Wine Cellar Cabinet" },
            { "Orijinal Yağlı Boya Tablo", "Original Oil Painting" },
            { "Lüks Mermer Şömine", "Luxury Marble Fireplace" },
            { "Kristal Avize Seti", "Crystal Chandelier Set" },
            { "Ev İçi Wellness Sauna Odası", "In-Home Wellness Sauna Room" },
            { "Özel Kuyruklu Piyano", "Custom Grand Piano" },
            { "Altın Kaplama Dekorasyon Seti", "Gold Plated Decoration Set" },
            { "Sınırlı Üretim İsviçre Saat Koleksiyonu", "Limited Edition Swiss Watch Collection" },
            { "Tarihi Heykel Eseri", "Historical Sculpture Art" },
            { "Kraliyet Ailesi Koleksiyon Sandığı", "Royal Family Collection Chest" },

            // Office (Ofis)
            { "Hızlı Wi-Fi Yönlendirici", "High-Speed Wi-Fi Router" },
            { "Çift Ekran Monitör Seti", "Dual Screen Monitor Set" },
            { "Ergonomik Klavye & Fare", "Ergonomic Keyboard & Mouse" },
            { "Hava Temizleme Cihazı", "Air Purifier" },
            { "Ofis Bitki Seti", "Office Plant Set" },
            { "Beyaz Akıllı Tahta", "Smart Whiteboard" },
            { "Filtreli Espresso Makinesi", "Filtered Espresso Machine" },
            { "Ayarlanabilir Ofis Masaları", "Adjustable Standing Desks" },
            { "Ofis Akustik Bölmeleri", "Office Acoustic Panels" },
            { "Güvenlik Kamerası Ağı", "Security Camera Network" },
            { "Mini Bar ve Snack İstasyonu", "Mini Bar & Snack Station" },
            { "Toplantı Odası Video Konferans Seti", "Meeting Room Video Conference Kit" },
            { "Özel Ajans Karşılama Bankosu", "Custom Agency Reception Desk" },
            { "Ofis Dinlenme Kapsülü (Nap Pod)", "Office Nap Pod" },
            { "Özel Sunucu Rafı (Server Rack)", "Custom Server Rack" },
            { "Cam Bölmeli VIP Toplantı Odası", "VIP Meeting Room with Glass Partitions" },
            { "Ofis İçi Yeşil Duvar (Dikey Bahçe)", "In-Office Green Wall (Vertical Garden)" },
            { "Akıllı Cam Karartma Sistemi", "Smart Glass Dimming System" },
            { "Şef Odaklı Gurme Ofis Mutfağı", "Chef-Oriented Gourmet Office Kitchen" },
            { "VR Deneyim ve Eğlence Odası", "VR Experience & Entertainment Room" },
            { "Özel Güvenlikli Veri Merkezi", "Private Secure Data Center" },
            { "Helikopter Pisti Erişim Yetkisi", "Helicopter Pad Access Authorization" },
            { "Çatı Katı Sosyal Teras Alanı", "Penthouse Social Terrace Area" },
            { "Ajans Özel Basın Toplantısı Salonu", "Agency Private Press Conference Hall" },
            { "Gökdelen Katının Tamamı", "Entire Skyscraper Floor" },

            // Categories
            { "Araçlar", "Vehicles" },
            { "Konutlar", "Real Estate" },
            { "Lüks & Mobilya", "Luxury & Furniture" },
            { "Ofis", "Office" }
        };

        private static Dictionary<string, string> leagueTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Türkiye 1. Ligi", "Turkish 1st Division" },
            { "Türkiye 2. Ligi", "Turkish 2nd Division" },
            { "Türkiye 3. Ligi", "Turkish 3rd Division" },
            { "İngiltere 1. Ligi", "English 1st Division" },
            { "İngiltere 2. Ligi", "English 2nd Division" },
            { "İngiltere 3. Ligi", "English 3rd Division" },
            { "İspanya 1. Ligi", "Spanish 1st Division" },
            { "İspanya 2. Ligi", "Spanish 2nd Division" },
            { "İspanya 3. Ligi", "Spanish 3rd Division" },
            { "Fransa 1. Ligi", "French 1st Division" },
            { "Fransa 2. Ligi", "French 2nd Division" },
            { "Fransa 3. Ligi", "French 3rd Division" },
            { "Almanya 1. Ligi", "German 1st Division" },
            { "Almanya 2. Ligi", "German 2nd Division" },
            { "Almanya 3. Ligi", "German 3rd Division" },
            { "İtalya 1. Ligi", "Italian 1st Division" },
            { "İtalya 2. Ligi", "Italian 2nd Division" },
            { "İtalya 3. Ligi", "Italian 3rd Division" },
            { "Portekiz 1. Ligi", "Portuguese 1st Division" },
            { "Portekiz 2. Ligi", "Portuguese 2nd Division" },
            { "Hollanda 1. Ligi", "Dutch 1st Division" },
            { "Hollanda 2. Ligi", "Dutch 2nd Division" },
            { "Rusya 1. Ligi", "Russian 1st Division" },
            { "Rusya 2. Ligi", "Russian 2nd Division" },
            { "Belçika 1. Ligi", "Belgian 1st Division" },
            { "Belçika 2. Ligi", "Belgian 2nd Division" },
            { "Brezilya 1. Ligi", "Brazilian 1st Division" },
            { "Brezilya 2. Ligi", "Brazilian 2nd Division" },
            { "Brezilya 3. Ligi", "Brazilian 3rd Division" }
        };

        private static Dictionary<string, string> countryTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Türkiye", "Turkey" },
            { "İngiltere", "England" },
            { "İspanya", "Spain" },
            { "Fransa", "France" },
            { "Almanya", "Germany" },
            { "İtalya", "Italy" },
            { "Portekiz", "Portugal" },
            { "Hollanda", "Netherlands" },
            { "Rusya", "Russia" },
            { "Belçika", "Belgium" },
            { "Brezilya", "Brazil" }
        };

        private static Dictionary<string, string> clubTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Türkiye 1
            { "İstanbul Aslanları", "Istanbul Lions" },
            { "Kadıköy Kanaryaları", "Kadikoy Canaries" },
            { "Boğaz Kartalları", "Bosphorus Eagles" },
            { "Karadeniz Fırtınası", "Black Sea Storm" },
            { "Başkent Gücü", "Capital Power" },
            { "Adana Şimşekleri", "Adana Lightnings" },
            { "Horozlar Denizli", "Denizli Roosters" },
            { "Yeşil Timsahlar", "Green Crocodiles" },
            { "Anadolu Kaplanları", "Anatolian Tigers" },
            { "Yiğidolar Sivas", "Sivas Yigidos" },
            { "Akdeniz Akrepleri", "Mediterranean Scorpions" },
            { "Körfez Gücü", "Gulf Power" },
            { "Kırmızı Şimşekler", "Red Lightnings" },
            { "İzmir Göztepe", "Izmir Goztepe" },
            { "Kaf-Kaf Karşıyaka", "Kaf-Kaf Karsiyaka" },
            { "Toros Kaplanları", "Taurus Tigers" },
            { "Samsun Kırmızıları", "Samsun Reds" },
            { "Başakşehir Baykuşları", "Basaksehir Owls" },

            // Türkiye 2
            { "Rize Çaycıları", "Rize Tea Makers" },
            { "Bodrum Mavileri", "Bodrum Blues" },
            { "Pendik Gücü", "Pendik Power" },
            { "Ümraniye FK", "Umraniye FC" },
            { "Eyüp Hilal", "Eyup Crescent" },
            { "Sakarya Tatangalar", "Sakarya Tatangas" },
            { "Altay Siyahlar", "Altay Blacks" },
            { "Manisa Tarzanları", "Manisa Tarzans" },
            { "Bolu Yarenleri", "Bolu Companions" },
            { "Giresun Çotanaklar", "Giresun Cotanaks" },
            { "Gaziantep Şahinler", "Gaziantep Falcons" },
            { "Hatay Asileri", "Hatay Rebels" },
            { "Alanya Portakalları", "Alanya Oranges" },
            { "Karagümrük Kırmızıları", "Karagumruk Reds" },
            { "Kasımpaşa Lacivertler", "Kasimpasa Navies" },
            { "Bandırma Bordo", "Bandirma Maroon" },
            { "Tuzla Tuzcular", "Tuzla Saltmakers" },
            { "Keçiörengücü", "Keciorengucu" },

            // Türkiye 3
            { "Diyarbakır Kaplanları", "Diyarbakir Tigers" },
            { "Van Kedileri", "Van Cats" },
            { "Mersin İdman", "Mersin Athletic" },
            { "Çorum Kırmızıları", "Corum Reds" },
            { "Şanlıurfa Sarıları", "Sanliurfa Yellows" },
            { "Batman Petrol", "Batman Petroleum" },
            { "Elazığ Gakkoşlar", "Elazig Gakkoshes" },
            { "Afyon Şimşekler", "Afyon Lightnings" },
            { "Isparta Gülleri", "Isparta Roses" },
            { "İnegöl Bordo", "Inegol Maroon" },
            { "Düzce Yeşiller", "Duzce Greens" },
            { "Sarıyer Beyazlar", "Sariyer Whites" },
            { "Altınordu Gençleri", "Altinordu Youth" },
            { "Menemen Sarılar", "Menemen Yellows" },
            { "Fethiye Lacivert", "Fethiye Navy" },
            { "Uşak Kırmızı", "Usak Red" },
            { "Kırklareli Spor", "Kirklareli Sport" },
            { "Kastamonu Orman", "Kastamonu Forest" },

            // İngiltere 1
            { "Manchester Kırmızıları", "Manchester Reds" },
            { "Manchester Mavileri", "Manchester Blues" },
            { "Merseyside Kırmızıları", "Merseyside Reds" },
            { "Merseyside Mavileri", "Merseyside Blues" },
            { "Londra Topçuları", "London Gunners" },
            { "Londra Mavileri", "London Blues" },
            { "Londra Horozları", "London Spurs" },
            { "Londra Çekiçleri", "London Hammers" },
            { "Kuzey Saksağanları", "Northern Magpies" },
            { "Birmingham Aslanları", "Birmingham Lions" },
            { "Leicester Tilkileri", "Leicester Foxes" },
            { "Kurtlar Wolverhampton", "Wolves Wolverhampton" },
            { "Nottingham Ormanı", "Nottingham Forest" },
            { "Azizler Southampton", "Saints Southampton" },
            { "Londra Kartalları", "London Eagles" },
            { "Londra Arıları", "London Bees" },
            { "Yorkshire Beyazları", "Yorkshire Whites" },
            { "Brighton Martıları", "Brighton Seagulls" },

            // İngiltere 2
            { "Sheffield Bıçakları", "Sheffield Blades" },
            { "Sheffield Baykuşları", "Sheffield Owls" },
            { "Blackburn Nehirleri", "Blackburn Rovers" },
            { "Norwich Kanaryaları", "Norwich Canaries" },
            { "Koçlar Derby", "Derby Rams" },
            { "Liman Şehri Portsmouth", "Portsmouth Port City" },
            { "Sunderland Kedileri", "Sunderland Black Cats" },
            { "Watford Eşekarıları", "Watford Hornets" },
            { "Coventry Gökyüzü", "Coventry Sky Blues" },
            { "Middlesbrough Kırmızıları", "Middlesbrough Reds" },
            { "Fulham Kırları", "Fulham Cottagers" },
            { "Bristol Robins", "Bristol Robins" },
            { "Swansea Kuğuları", "Swansea Swans" },
            { "Cardiff Mavi Kuşları", "Cardiff Bluebirds" },
            { "QPR Korucuları", "QPR Rangers" },
            { "Millwall Aslanları", "Millwall Lions" },
            { "Plymouth Hacılar", "Plymouth Pilgrims" },
            { "Preston Kuzey", "Preston North End" },

            // İngiltere 3
            { "Wigan Atletik", "Wigan Athletic" },
            { "Bolton Gezginleri", "Bolton Wanderers" },
            { "Charlton Vadisi", "Charlton Valley" },
            { "Reading Kralları", "Reading Royals" },
            { "Blackpool Turuncuları", "Blackpool Tangerines" },
            { "Lincoln İblisleri", "Lincoln Imps" },
            { "Peterborough Maviler", "Peterborough Posh" },
            { "Barnsley Tykes", "Barnsley Tykes" },
            { "Wycombe Sandalyeciler", "Wycombe Chairboys" },
            { "Leyton Oryantal", "Leyton Orient" },
            { "Oxford United", "Oxford United" },
            { "Shrewsbury Salop", "Shrewsbury Town" },
            { "Northampton Kunduracılar", "Northampton Cobblers" },
            { "Bristol Korsanları", "Bristol Rovers" },
            { "Exeter Yunanlılar", "Exeter Grecians" },
            { "Port Vale Kimyacılar", "Port Vale Valiants" },
            { "Fleetwood Balıkçılar", "Fleetwood Cod Army" },
            { "Cambridge Akademisyenler", "Cambridge U's" },

            // İspanya 1
            { "Madrid Beyazları", "Madrid Whites" },
            { "Katalan Mavileri", "Catalan Blues" },
            { "Madrid Çizgilileri", "Madrid Stripes" },
            { "Endülüs Beyazları", "Andalusia Whites" },
            { "Endülüs Yeşilleri", "Andalusia Greens" },
            { "Bask Aslanları", "Basque Lions" },
            { "Bask Mavileri", "Basque Blues" },
            { "Valencia Yarasaları", "Valencia Bats" },
            { "Sarı Denizaltılar", "Yellow Submarines" },
            { "Galiçya Gök Mavileri", "Galician Sky Blues" },
            { "Ada Kırmızıları", "Island Reds" },
            { "Katalonya Kırmızıları", "Catalonia Reds" },
            { "Kanarya Adaları", "Canary Islands" },
            { "Navarra Kırmızıları", "Navarre Reds" },
            { "Getafe Mavileri", "Getafe Blues" },
            { "Vallecas Şimşekleri", "Vallecas Lightnings" },
            { "Alaves Mavileri", "Alaves Blues" },
            { "Granada Kırmızısı", "Granada Reds" },

            // İspanya 2
            { "Galiçya Mavileri", "Galician Blues" },
            { "Barselona Beyazları", "Barcelona Whites" },
            { "Zaragoza Mavileri", "Zaragoza Blues" },
            { "Valladolid Morları", "Valladolid Violets" },
            { "Tenerife Mavileri", "Tenerife Blues" },
            { "Oviedo Mavileri", "Oviedo Blues" },
            { "Elche Yeşilleri", "Elche Greens" },
            { "Levante Kurbağaları", "Levante Frogs" },
            { "Eibar Silahşörleri", "Eibar Gunsmiths" },
            { "Burgos Şövalyeleri", "Burgos Knights" },
            { "Leganes Salatalıklar", "Leganes Pepineros" },
            { "Castellon Siyahları", "Castellon Blacks" },
            { "Murcia Kırmızıları", "Murcia Reds" },
            { "Almeria Kırmızıları", "Almeria Reds" },
            { "Sporting Gijon", "Sporting Gijon" },
            { "Huesca Kırmızı Mavileri", "Huesca Red Blues" },
            { "Racing Santander", "Racing Santander" },
            { "Cartagena Siyah Beyaz", "Cartagena Black Whites" },

            // İspanya 3
            { "Recreativo Yaşlılar", "Recreativo Elders" },
            { "Castellon Gençleri", "Castellon Youth" },
            { "Ibiza Gece", "Ibiza Nights" },
            { "Malaga Mavileri", "Malaga Blues" },
            { "Cordoba Yeşilleri", "Cordoba Greens" },
            { "Nastic Tarragona", "Nastic Tarragona" },
            { "Ceuta Kuzey", "Ceuta North" },
            { "Melilla Afrika", "Melilla Africa" },
            { "Lugo Galiçya", "Lugo Galicia" },
            { "Ponferradina Mavi Bordo", "Ponferradina Blue Maroon" },
            { "Sabadell Katalan", "Sabadell Catalan" },
            { "Alcoyano İnançlılar", "Alcoyano Believers" },
            { "Sestao Nehir", "Sestao River" },
            { "Barakaldo Sarı Siyah", "Barakaldo Yellow Black" },
            { "Real Union Bask", "Real Union Basque" },
            { "Tarazona Kırmızı", "Tarazona Reds" },
            { "Teruel Çöl", "Teruel Desert" },
            { "Antequera Yeşil", "Antequera Greens" },

            // Fransa 1
            { "Paris Mavileri", "Paris Blues" },
            { "Marsilya Limanı", "Marseille Harbor" },
            { "Lyon Aslanları", "Lyon Lions" },
            { "Monako Sarayı", "Monaco Palace" },
            { "Lille Tazıları", "Lille Mastiffs" },
            { "Rennes Kırmızıları", "Rennes Reds" },
            { "Nice Kartalları", "Nice Eagles" },
            { "Lens Madencileri", "Lens Miners" },
            { "Nantes Kanaryaları", "Nantes Canaries" },
            { "Strasbourg Mavi Beyaz", "Strasbourg Blue Whites" },
            { "Montpellier Turuncu", "Montpellier Orange" },
            { "Toulouse Menekşe", "Toulouse Violets" },
            { "Reims Taç", "Reims Crown" },
            { "Auxerre Beyazları", "Auxerre Whites" },
            { "Yeşiller Saint-Etienne", "Saint-Etienne Greens" },
            { "Bordeaux Bağları", "Bordeaux Vineyards" },
            { "Metz Ejderleri", "Metz Dragons" },
            { "Lorient Morinaları", "Lorient Codfishes" },

            // Fransa 2
            { "Troyes Mavileri", "Troyes Blues" },
            { "Angers Siyah Beyaz", "Angers Black Whites" },
            { "Brest Limanı", "Brest Harbor" },
            { "Caen Vikingleri", "Caen Vikings" },
            { "Dijon Hardalları", "Dijon Mustards" },
            { "Guingamp Kırmızıları", "Guingamp Reds" },
            { "Nancy Thistle", "Nancy Thistle" },
            { "Sochaux Aslanları", "Sochaux Lions" },
            { "Valenciennes Kırmızı", "Valenciennes Reds" },
            { "Le Havre Maviler", "Le Havre Blues" },
            { "Bastia Şahinleri", "Bastia Falcons" },
            { "Amiens Tek boynuzlar", "Amiens Unicorns" },
            { "Grenoble Alpleri", "Grenoble Alps" },
            { "Ajaccio İmparatorları", "Ajaccio Emperors" },
            { "Laval Portakalları", "Laval Oranges" },
            { "Rodez Kırmızıları", "Rodez Reds" },
            { "Pau Yeşilleri", "Pau Greens" },
            { "Paris FC Yeşiller", "Paris FC Greens" },

            // Fransa 3
            { "Nimes Timsahları", "Nimes Crocodiles" },
            { "Niort Süvarileri", "Niort Cavalrymen" },
            { "Chateauroux Maviler", "Chateauroux Blues" },
            { "Red Star Paris", "Red Star Paris" },
            { "Versay Sarayı", "Versailles Palace" },
            { "Orleans Arıları", "Orleans Bees" },
            { "Le Mans Horozları", "Le Mans Roosters" },
            { "Boulogne Denizciler", "Boulogne Sailors" },
            { "Quevilly Kırmızı", "Quevilly Reds" },
            { "Concarneau Balıkçı", "Concarneau Fishermen" },
            { "Avranches Tepesi", "Avranches Hill" },
            { "Epinal Yeşilleri", "Epinal Greens" },
            { "Villefranche Kaplan", "Villefranche Tigers" },
            { "Rouen Kırmızıları", "Rouen Reds" },
            { "Marignane Maviler", "Marignane Blues" },
            { "Martigues Sarılar", "Martigues Yellows" },
            { "Nancy Gençleri", "Nancy Youth" },
            { "Cholet Mavileri", "Cholet Blues" },

            // Almanya 1
            { "Münih Kırmızıları", "Munich Reds" },
            { "Dortmund Sarıları", "Dortmund Yellows" },
            { "Leipzig Boğaları", "Leipzig Bulls" },
            { "Leverkusen İşçileri", "Leverkusen Workers" },
            { "Frankfurt Kartalları", "Frankfurt Eagles" },
            { "Stuttgart Atları", "Stuttgart Horses" },
            { "Bremen Mızıkacıları", "Bremen Musicians" },
            { "Hamburg Dinozorları", "Hamburg Dinosaurs" },
            { "Köln Tekeleri", "Cologne Billy Goats" },
            { "Berlin Birlikleri", "Union Berlin" },
            { "Gelsenkirchen Madencileri", "Schalke Miners" },
            { "Mönchengladbach Tayları", "Gladbach Foals" },
            { "Mainz Karnaval", "Mainz Carnival" },
            { "Augsburg Fugger", "Augsburg Fuggers" },
            { "Wolfsburg Kurtları", "Wolfsburg Wolves" },
            { "Hoffenheim Köyü", "Hoffenheim Village" },
            { "Freiburg Çamları", "Freiburg Pines" },
            { "Heidenheim Şatoları", "Heidenheim Castles" },

            // Almanya 2
            { "Darmstadt Zambaklar", "Darmstadt Lilies" },
            { "Düsseldorf Fortunaları", "Dusseldorf Fortunas" },
            { "Hannover 96lar", "Hannover 96" },
            { "Karlsruhe Mavileri", "Karlsruhe Blues" },
            { "Nürnberg Kulübü", "Nuremberg Club" },
            { "Kaiserslautern Şeytanları", "Lautern Red Devils" },
            { "Hertha Berlin", "Hertha Berlin" },
            { "Schalke 04 Muadili", "Schalke 04 Gelsenkirchen" },
            { "St. Pauli Korsanları", "St. Pauli Pirates" },
            { "Rostock Hanse", "Hansa Rostock" },
            { "Bielefeld Arminia", "Arminia Bielefeld" },
            { "Dresden Dinamoları", "Dynamo Dresden" },
            { "Paderborn Mavileri", "Paderborn Blues" },
            { "Magdeburg Maviler", "Magdeburg Blues" },
            { "Fürth Yaprakları", "Greuther Furth Clovers" },
            { "Kiel Leylekleri", "Holstein Kiel Storks" },
            { "Osnabrück Morları", "Osnabruck Purples" },
            { "Wiesbaden Sarılar", "Wehen Wiesbaden Yellows" },

            // Almanya 3
            { "Duisburg Zebraları", "Duisburg Zebras" },
            { "1860 Münih Muadili", "1860 Munich" },
            { "Saarbrücken Maviler", "Saarbrücken Blues" },
            { "Essen Kırmızı Beyaz", "Rot-Weiss Essen" },
            { "Halle Kimyagerleri", "Hallescher Chemists" },
            { "Aue Madencileri", "Erzgebirge Aue Miners" },
            { "Regensburg Jahn", "Jahn Regensburg" },
            { "Sandhausen Siyah", "Sandhausen Black" },
            { "Ulm Serçeleri", "Ulm Sparrows" },
            { "Münster Kartalları", "Preussen Munster Eagles" },
            { "Unterhaching Bob", "Unterhaching Bobs" },
            { "Lübeck Yeşil", "Lubeck Greens" },
            { "Viktoria Köln", "Viktoria Cologne" },
            { "Verl Siyah Beyaz", "SC Verl Black Whites" },
            { "Ingolstadt Şanzıman", "Ingolstadt Gearboxes" },
            { "Dortmund Rezerv", "Dortmund Reserve" },
            { "Münih Rezerv", "Munich Reserve" },
            { "Freiburg Rezerv", "Freiburg Reserve" },

            // İtalya 1
            { "Torino Siyah Beyazları", "Turin Black Whites" },
            { "Milano Kırmızı Siyahları", "Milan Red Blacks" },
            { "Milano Mavi Siyahları", "Milan Blue Blacks" },
            { "Napoli Gök Mavileri", "Napoli Sky Blues" },
            { "Roma Kurtları", "Rome Wolves" },
            { "Roma Kartalları", "Rome Eagles" },
            { "Floransa Menekşeleri", "Florence Violets" },
            { "Bergamo Tanrıçaları", "Bergamo Goddesses" },
            { "Torino Boğaları", "Turin Bulls" },
            { "Cenova Kırmızı Mavileri", "Genoa Red Blues" },
            { "Cenova Çizgilileri", "Genoa Stripes" },
            { "Bologna Kırmızı Mavileri", "Bologna Red Blues" },
            { "Verona Sarıları", "Verona Yellows" },
            { "Sardinya Kırmızı Mavileri", "Sardinia Red Blues" },
            { "Lecce Kurtları", "Lecce Wolves" },
            { "Monza Kırmızıları", "Monza Reds" },
            { "Empoli Mavileri", "Empoli Blues" },
            { "Udine Siyah Beyaz", "Udinese Black White" },

            // İtalya 2
            { "Sicilya Pembeleri", "Sicilian Pinks" },
            { "Bari Horozları", "Bari Roosters" },
            { "Venedik Gondolları", "Venice Gondolas" },
            { "Como Gölleri", "Como Lakes" },
            { "Parma Dükleri", "Parma Dukes" },
            { "Cremonese Grileri", "Cremonese Grays" },
            { "Pisa Kuleleri", "Pisa Towers" },
            { "Reggiana Granat", "Reggiana Granatas" },
            { "Catanzaro Kartalları", "Catanzaro Eagles" },
            { "Brescia Kırlangıçları", "Brescia Swallows" },
            { "Modena Kanaryaları", "Modena Canaries" },
            { "Spezia Kartalları", "Spezia Eagles" },
            { "Ternana Canavarları", "Ternana Monsters" },
            { "Ascoli Ağaçları", "Ascoli Trees" },
            { "Cosenza Kurtları", "Cosenza Wolves" },
            { "Lecco Gölleri", "Lecco Lakes" },
            { "Feralpisalo Yeşilleri", "Feralpisalo Greens" },
            { "Sudtirol Dağcıları", "Sudtirol Mountaineers" },

            // İtalya 3
            { "Padova Kırmızıları", "Padova Reds" },
            { "Vicenza Çizgilileri", "Vicenza Stripes" },
            { "Triestina Alabard", "Triestina Halberds" },
            { "Pescara Yunusları", "Pescara Dolphins" },
            { "Spal Mavileri", "Spal Blues" },
            { "Perugia Grifonları", "Perugia Griffins" },
            { "Ancona Kırmızıları", "Ancona Reds" },
            { "Lucchese Panterleri", "Lucchese Panthers" },
            { "Siena Siyah Beyaz", "Siena Black Whites" },
            { "Novara Mavileri", "Novara Blues" },
            { "Pro Vercelli Aslan", "Pro Vercelli Lions" },
            { "Taranto Yunusları", "Taranto Dolphins" },
            { "Foggia Şeytanları", "Foggia Devils" },
            { "Avellino Kurtları", "Avellino Wolves" },
            { "Benevento Cadıları", "Benevento Witches" },
            { "Crotone Köpekbalığı", "Crotone Sharks" },
            { "Messina Kalkan", "Messina Shield" },
            { "Catania Filleri", "Catania Elephants" },

            // Portekiz 1
            { "Lizbon Aslanları", "Lisbon Lions" },
            { "Ejderha Porto", "Porto Dragons" },
            { "Lizbon Kartalları", "Lisbon Eagles" },
            { "Braga Savaşçıları", "Braga Warriors" },
            { "Guimaraes Fatihleri", "Guimaraes Conquerors" },
            { "Famalicao Mavileri", "Famalicao Blues" },
            { "Arouca Sarı", "Arouca Yellow" },
            { "Moreira Yeşilleri", "Moreira Greens" },
            { "Portimao Siyah", "Portimao Black" },
            { "Faro Kurtları", "Faro Wolves" },
            { "Chaves Anahtarları", "Chaves Keys" },
            { "Vizela Mavileri", "Vizela Blues" },
            { "Estoril Sarıları", "Estoril Yellows" },
            { "Barcelos Horozları", "Barcelos Roosters" },
            { "Rio Ave Nehir", "Rio Ave River" },
            { "Funchal Adalılar", "Funchal Islanders" },
            { "Ponta Delgada Ada", "Ponta Delgada Island" },
            { "Boavista Satranç", "Boavista Chess" },

            // Portekiz 2
            { "Penafiel Kırmızı", "Penafiel Reds" },
            { "Feirense Mavileri", "Feirense Blues" },
            { "Tondela Yeşilleri", "Tondela Greens" },
            { "Academico Viseu", "Academico Viseu" },
            { "Mafra Sarıları", "Mafra Yellows" },
            { "Leiria Kalesi", "Leiria Castle" },
            { "Torres Vedras Boğa", "Torres Vedras Bulls" },
            { "Oliveirense Kırmızı", "Oliveirense Reds" },
            { "Santa Clara Ada", "Santa Clara Island" },
            { "Lank Vilaverdense", "Lank Vilaverdense" },
            { "Belenenses Mavi", "Belenenses Blues" },
            { "Nacional Funchal", "Nacional Funchal" },
            { "Maritimo Yeşil Kırmızı", "Maritimo Green Reds" },
            { "Porto B Muadili", "Porto B" },
            { "Benfica B Muadili", "Benfica B" },
            { "Sporting B Muadili", "Sporting Lisbon B" },
            { "Pacos Ferreira", "Pacos Ferreira" },
            { "Penafiel Gücü", "Penafiel Power" },

            // Hollanda 1
            { "Amsterdam Tanrıları", "Amsterdam Gods" },
            { "Rotterdam Limanı", "Rotterdam Port" },
            { "Eindhoven Çiftçileri", "Eindhoven Farmers" },
            { "Enschede Atları", "Enschede Horses" },
            { "Alkmaar Peynircileri", "Alkmaar Cheesemakers" },
            { "Utrecht Kırmızı", "Utrecht Reds" },
            { "Arnhem Kartalları", "Arnhem Eagles" },
            { "Nijmegen Kırmızı Yeşil", "Nijmegen Red Greens" },
            { "Heerenveen Kalpleri", "Heerenveen Hearts" },
            { "Zwolle Mavileri", "Zwolle Blues" },
            { "Almere Kara", "Almere Black" },
            { "Deventer Kartalları", "Deventer Eagles" },
            { "Sittard Sarıları", "Sittard Yellows" },
            { "Waalwijk Mavileri", "Waalwijk Blues" },
            { "Volendam Balıkçı", "Volendam Fishermen" },
            { "Leeuwarden Geyikleri", "Leeuwarden Deers" },
            { "Groningen Yeşilleri", "Groningen Greens" },
            { "Tilburg Kralları", "Tilburg Kings" },

            // Hollanda 2
            { "Breda Fareleri", "Breda Rats" },
            { "Kerkrade Madencileri", "Kerkrade Miners" },
            { "Venlo VVV", "Venlo VVV" },
            { "Den Haag Kuğuları", "Den Haag Swans" },
            { "Doetinchem Süvarileri", "Doetinchem Cavalrymen" },
            { "Emmen Yeşilleri", "Emmen Greens" },
            { "Eindhoven FC", "FC Eindhoven" },
            { "Dordrecht Koyunları", "Dordrecht Sheeps" },
            { "Maastricht Yıldız", "Maastricht Star" },
            { "Den Bosch Ejderler", "Den Bosch Dragons" },
            { "Oss Boğaları", "Oss Bulls" },
            { "Helmond Kedileri", "Helmond Cats" },
            { "Jong Ajax Muadili", "Jong Ajax" },
            { "Jong PSV Muadili", "Jong PSV" },
            { "Jong AZ Muadili", "Jong AZ" },
            { "Jong Utrecht Muadili", "Jong Utrecht" },
            { "Telstar Beyazları", "Telstar Whites" },
            { "Cambuur Sarıları", "Cambuur Yellows" },

            // Rusya 1
            { "Zenit Sankt-Peterburg", "Zenit St. Petersburg" },
            { "Lokomotiv Moskova", "Lokomotiv Moscow" },
            { "CSKA Moskova", "CSKA Moscow" },
            { "Spartak Moskova", "Spartak Moscow" },
            { "Krasnodar Boğaları", "Krasnodar Bulls" },
            { "Dinamo Moskova", "Dinamo Moscow" },
            { "Rostov Sarı Lacivert", "Rostov Yellow Blues" },
            { "Soçi Denizciler", "Sochi Sailors" },
            { "Samara Kanatları", "Samara Wings" },
            { "Rubin Kazan", "Rubin Kazan" },
            { "Grozny Çeçenleri", "Grozny Chechens" },
            { "Nizhny Novgorod", "Nizhny Novgorod" },
            { "Ural Turuncu", "Ural Orange" },
            { "Orenburg Gazı", "Orenburg Gas" },
            { "Fakel Meşaleleri", "Fakel Torches" },
            { "Baltika Kaliningrad", "Baltika Kaliningrad" },
            { "Khimki Kırmızı", "Khimki Reds" },
            { "Tula Cephane", "Tula Arsenal" },

            // Rusya 2
            { "Torpedo Moskova", "Torpedo Moscow" },
            { "Yaroslavl Şinik", "Shinnik Yaroslavl" },
            { "Saratov Şahinler", "Sokol Saratov" },
            { "Volgograd Rotor", "Rotor Volgograd" },
            { "Yenisey Sibirya", "Yenisey Siberia" },
            { "Tyumen Kar", "Tyumen Snow" },
            { "Makhachkala Dinamo", "Dinamo Makhachkala" },
            { "Chernomorets Deniz", "Chernomorets Sea" },
            { "SKA Khabarovsk", "SKA Khabarovsk" },
            { "Neftekhimik Petrol", "Neftekhimik Petroleum" },
            { "Kamaz Kamyon", "Kamaz Trucks" },
            { "Kuban Krasnodar", "Kuban Krasnodar" },
            { "Alania Vladikavkaz", "Alania Vladikavkaz" },
            { "Leningradets", "Leningradets" },
            { "Sokol Saratov", "Sokol Saratov" },
            { "Volgar Astrakhan", "Volgar Astrakhan" },
            { "Ufa Spor", "Ufa Sport" },
            { "Shinnik Yaroslavl", "Shinnik Yaroslavl" },

            // Belçika 1
            { "Anderlecht Eflatun", "Anderlecht Purples" },
            { "Brugge Mavileri", "Bruges Blues" },
            { "Gent Bufaloları", "Gent Buffaloes" },
            { "Antwerp Kırmızı", "Antwerp Reds" },
            { "Genk Madencileri", "Genk Miners" },
            { "Liege Kırmızıları", "Standard Liege Reds" },
            { "Charleroi Zebraları", "Charleroi Zebras" },
            { "Kortrijk Kırmızı", "Kortrijk Reds" },
            { "Mechelen Sarı Kırmızı", "Mechelen Yellow Reds" },
            { "Sint-Truiden Kanarya", "Sint-Truiden Canaries" },
            { "Westerlo Sarı Lacivert", "Westerlo Yellow Blues" },
            { "Eupen Siyah", "Eupen Black" },
            { "Leuven Beyaz", "Leuven White" },
            { "Cercle Brugge", "Cercle Bruges" },
            { "Union Saint-Gilloise", "Union Saint-Gilloise" },
            { "Beveren Sarı", "Beveren Yellows" },
            { "Lierse Kırmızı", "Lierse Reds" },
            { "Waregem Yeşil", "Zulte Waregem Greens" },

            // Belçika 2
            { "Lommel Yeşilleri", "Lommel Greens" },
            { "Deinze Turuncuları", "Deinze Oranges" },
            { "Seraing Kırmızı", "Seraing Reds" },
            { "Virton Yeşilleri", "Virton Greens" },
            { "Ostend Deniz", "Ostend Sea" },
            { "Zulte Waregem", "Zulte Waregem" },
            { "Patro Eisden", "Patro Eisden" },
            { "RFC Liege", "RFC Liege" },
            { "Dender Lacivert", "Dender Navy" },
            { "Beveren Gücü", "Beveren Power" },
            { "Club Brugge B", "Club Bruges B" },
            { "Anderlecht B", "Anderlecht B" },
            { "Genk B", "Genk B" },
            { "Standard Liege B", "Standard Liege B" },
            { "Francs Borains", "Francs Borains" },
            { "Lierse Kempen", "Lierse Kempen" },
            { "RFC Antwerp B", "Royal Antwerp B" },
            { "RFC Gent B", "Gent B" },

            // Brezilya 1
            { "Flamengo Kırmızı Siyah", "Flamengo Red Black" },
            { "Palmeiras Yeşilleri", "Palmeiras Greens" },
            { "Sao Paulo Üçrenkliler", "Sao Paulo Tricolors" },
            { "Santos Balıkları", "Santos Fishes" },
            { "Gremio Ölümsüzleri", "Gremio Immortals" },
            { "Internacional Kırmızıları", "Internacional Reds" },
            { "Atletico Horozları", "Atletico Roosters" },
            { "Cruzeiro Tilkileri", "Cruzeiro Foxes" },
            { "Fluminense Savaşçıları", "Fluminense Warriors" },
            { "Botafogo Yalnız Yıldız", "Botafogo Lone Star" },
            { "Vasco Devleri", "Vasco Giants" },
            { "Bahia Üçrenkliler", "Bahia Tricolors" },
            { "Fortaleza Aslanları", "Fortaleza Lions" },
            { "Athletico Kasırgaları", "Athletico Hurricanes" },
            { "Coritiba Yeşiller", "Coritiba Greens" },
            { "Goias Papağanları", "Goias Parrots" },
            { "Cuiaba Altınları", "Cuiaba Golds" },
            { "Bragantino Boğaları", "Bragantino Bulls" },

            // Brezilya 2
            { "Sport Aslanları", "Sport Lions" },
            { "Santos Gençleri", "Santos Youth" },
            { "Ceara Siyah Beyaz", "Ceara Black Whites" },
            { "Goias Gücü", "Goias Power" },
            { "Coritiba Kaplanları", "Coritiba Tigers" },
            { "Avai Aslanları", "Avai Lions" },
            { "Ponte Preta Köprü", "Ponte Preta Bridge" },
            { "Guarani Yerlileri", "Guarani Indians" },
            { "Novorizontino Kaplan", "Novorizontino Tigers" },
            { "Mirassol Sarılar", "Mirassol Yellows" },
            { "America Tavşanları", "America Rabbits" },
            { "Operario Tren", "Operario Train" },
            { "CRB Kırmızıları", "CRB Reds" },
            { "Vila Nova Kırmızı", "Vila Nova Reds" },
            { "Chapecoense Yeşiller", "Chapecoense Greens" },
            { "Brusque Çizgili", "Brusque Striped" },
            { "Ituano Horozları", "Ituano Roosters" },
            { "Paysandu Mavileri", "Paysandu Blues" },

            // Brezilya 3
            { "Figueirense Kasırga", "Figueirense Hurricane" },
            { "CSA Mavi Mutlu", "CSA Blue Happy" },
            { "Botafogo PB Yıldız", "Botafogo PB Star" },
            { "Volta Redonda Çelik", "Volta Redonda Steel" },
            { "Ypiranga Sarı Siyah", "Ypiranga Yellow Black" },
            { "Remo Denizciler", "Remo Sailors" },
            { "Sao Bernardo Kaplan", "Sao Bernardo Tigers" },
            { "Confianca Ejder", "Confianca Dragons" },
            { "Ferroviario Ray", "Ferroviario Rail" },
            { "ABC Siyah Beyaz", "ABC Black White" },
            { "Londrina Gök Mavi", "Londrina Sky Blue" },
            { "Tombense Kırmızı", "Tombense Reds" },
            { "Sampaio Corrêa Üç", "Sampaio Correa Three" },
            { "Aparecidense Mavi", "Aparecidense Blues" },
            { "Floresta Orman", "Floresta Jungle" },
            { "Sao Jose Mavi", "Sao Jose Blues" },
            { "Ypiranga Kaplanları", "Ypiranga Tigers" },
            { "Remo Gücü", "Remo Power" }
        };
    }

    [UnityEngine.RequireComponent(typeof(UnityEngine.UI.Text))]
    public class LocalizableText : MonoBehaviour
    {
        public string originalText;
        public bool isUppercase;
        private UnityEngine.UI.Text textComponent;

        private void Awake()
        {
            textComponent = GetComponent<UnityEngine.UI.Text>();
        }

        public void UpdateLanguage()
        {
            if (textComponent == null) textComponent = GetComponent<UnityEngine.UI.Text>();
            if (textComponent != null && !string.IsNullOrEmpty(originalText))
            {
                string translated = LocalizationManager.Translate(originalText);
                textComponent.text = isUppercase ? translated.ToUpper() : translated;
            }
        }
    }
}
