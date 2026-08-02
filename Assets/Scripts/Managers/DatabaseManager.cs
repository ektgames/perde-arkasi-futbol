using System;
using System.Collections.Generic;
using UnityEngine;
using BehindTheScenesFootball.Core;

namespace BehindTheScenesFootball.Managers
{
    public class DatabaseManager : MonoBehaviour
    {
        public static DatabaseManager Instance { get; private set; }

        public List<Club> Clubs = new List<Club>();
        public List<Player> Players = new List<Player>();
        public List<Sponsor> Sponsors = new List<Sponsor>();
        public List<League> Leagues = new List<League>();

        // Localized name pools
        private string[] trFirst = { "Alperen", "Ahmet", "Barış", "Can", "Cengiz", "Emre", "Ferdi", "Hakan", "İrfan", "Kenan", "Kerem", "Merih", "Orkun", "Salih", "Semih", "Uğurcan", "Yusuf", "Zeki", "Kaan", "Umut", "Cenk", "Abdülkadir", "Taylan", "Berkan", "Yunus", "Arda", "Ozan", "Enes", "Okay", "Dorukhan" };
        private string[] trLast = { "Yılmaz", "Demir", "Çelik", "Şahin", "Yıldız", "Kaya", "Öztürk", "Aydın", "Çetin", "Erdoğan", "Arslan", "Yavuz", "Karaca", "Özer", "Bulut", "Şen", "Koç", "Acar", "Özdemir", "Polat", "Özkan", "Kılıç", "Şener", "Aksoy", "Tekin", "Yalçın", "Avcı", "Sarı", "Güler", "Çakır" };

        private string[] enFirst = { "Harry", "Jack", "Jude", "Bukayo", "Declan", "Marcus", "John", "Luke", "Mason", "Reece", "Trent", "Connor", "James", "Callum", "Harvey", "Jordan", "Cole", "Phil", "Ollie", "Kieran", "Kyle", "Aaron", "Ben", "Dominic", "Lewis", "Jarrod", "Kobbie", "Adam", "Danny", "Mason" };
        private string[] enLast = { "Smith", "Jones", "Taylor", "Brown", "Williams", "Wilson", "Johnson", "Davies", "Robinson", "Wright", "Thompson", "Evans", "Walker", "White", "Roberts", "Green", "Hall", "Wood", "Jackson", "Clarke", "Palmer", "Foden", "Bellingham", "Rice", "Kane", "Trippier", "Rashford", "Sterling", "Pickford", "Henderson" };

        private string[] esFirst = { "Alvaro", "Rodri", "Gavi", "Pedri", "Dani", "Nico", "Alejandro", "Lamine", "Ferran", "Aymeric", "Robin", "Pau", "Unai", "David", "Kepa", "Mikel", "Martin", "Alex", "Jose", "Marc", "Jesus", "Sergio", "Iago", "Borja", "Gerard", "Koke", "Marcos", "Cesar", "Nacho", "Carvajal" };
        private string[] esLast = { "Garcia", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Perez", "Sanchez", "Ramirez", "Torres", "Flores", "Gomez", "Diaz", "Alvarez", "Ruiz", "Moran", "Merino", "Simon", "Yamal", "Williams", "Olmo", "Laporte", "Le Normand", "Cucurella", "Zubimendi", "Ruiz", "Morata", "Joselu" };

        private string[] frFirst = { "Kylian", "Antoine", "Kingsley", "Ousmane", "Eduardo", "Aurelien", "Dayot", "William", "Ibrahima", "Jules", "Theo", "Lucas", "Mike", "Brice", "Benjamin", "Adrien", "Youssouf", "Randal", "Olivier", "Marcus", "Warren", "Bradley", "Ferland", "Jonathan", "N'Golo" };
        private string[] frLast = { "Mbappe", "Griezmann", "Coman", "Dembele", "Camavinga", "Tchouameni", "Upamecano", "Saliba", "Konate", "Kounde", "Hernandez", "Pavard", "Rabiot", "Fofana", "Kolo Muani", "Giroud", "Thuram", "Zaïre-Emery", "Barcola", "Mendy", "Clauss", "Samba", "Maignan", "Lloris" };

        private string[] deFirst = { "Thomas", "Manuel", "Joshua", "Leroy", "Serge", "Jamal", "Florian", "Kai", "Ilkay", "Leon", "Marc-Andre", "Nico", "Antonio", "Jonathan", "Robin", "Benjamin", "Maximilian", "Robert", "Chris", "Niclas", "Deniz", "Emre", "Jonas", "Waldemar", "David", "Alexander" };
        private string[] deLast = { "Müller", "Neuer", "Kimmich", "Sané", "Gnabry", "Musiala", "Wirtz", "Havertz", "Gündogan", "Goretzka", "ter Stegen", "Schlotterbeck", "Rüdiger", "Tah", "Gosens", "Henrichs", "Raum", "Andrich", "Führich", "Füllkrug", "Undav", "Can", "Hofmann", "Anton", "Baumann" };

        private string[] itFirst = { "Gianluigi", "Alessandro", "Francesco", "Nicolo", "Federico", "Ciro", "Lorenzo", "Giorgio", "Leonardo", "Manuel", "Davide", "Gianluca", "Mateo", "Bryan", "Andrea", "Giacomo", "Matteo", "Moise", "Destiny", "Raoul", "Giovanni", "Guglielmo", "Alex", "Cristiano" };
        private string[] itLast = { "Donnarumma", "Bastoni", "Acerbi", "Barella", "Chiesa", "Immobile", "Pellegrini", "Scalvini", "Mancini", "Locatelli", "Frattesi", "Scamacca", "Retegui", "Cristante", "Bellanova", "Darmian", "Dimarco", "Meret", "Vicario", "Buongiorno", "Gatti", "Cambiaso", "Jorginho", "El Shaarawy" };

        private string[] ptFirst = { "Cristiano", "Bernardo", "Bruno", "Ruben", "Joao", "Rafael", "Diogo", "Goncalo", "Vitinha", "Otavio", "Matheus", "Danilo", "Pepe", "Nuno", "Nelson", "Jose", "Rui", "Pedro", "Antonio", "Francisco", "Florentino", "Ricardo", "Tiago", "Andre", "Fabio" };
        private string[] ptLast = { "Ronaldo", "Silva", "Fernandes", "Dias", "Felix", "Leao", "Jota", "Ramos", "Mendes", "Semedo", "Sa", "Patricio", "Neto", "Neves", "Cancelo", "Dalot", "Inacio", "Palhinha", "Conceicao", "Horta", "Gomes", "Costa", "Guerreiro", "Sanches" };

        private string[] nlFirst = { "Virgil", "Frenkie", "Memphis", "Cody", "Xavi", "Denzel", "Nathan", "Matthijs", "Stefan", "Jeremie", "Quilindschy", "Tijjani", "Joey", "Mats", "Wout", "Brian", "Steven", "Justin", "Mark", "Bart", "Micky", "Ryan", "Teun", "Lutsharel", "Georginio" };
        private string[] nlLast = { "van Dijk", "de Jong", "Depay", "Gakpo", "Simons", "Dumfries", "Ake", "de Ligt", "de Vrij", "Frimpong", "Hartman", "Reijnders", "Veerman", "Wieffer", "Weghorst", "Brobbey", "Bergwijn", "Bijlow", "Flekken", "Verbruggen", "van de Ven", "Gravenberch", "Koopmeiners", "Geertruida", "Wijnaldum" };

        private string[] ruFirst = { "Aleksandr", "Aleksey", "Anton", "Fedor", "Arsen", "Dmitry", "Vyacheslav", "Matvey", "Ivan", "Sergey", "Andrey", "Danil", "Konstantin", "Maksim", "Igor", "Roman", "Evgeny", "Mikhail", "Nikita", "Kirill", "Denis", "Vladislav", "Ilya", "Yuri" };
        private string[] ruLast = { "Golovin", "Miranchuk", "Chalov", "Zakharyan", "Barinov", "Safonov", "Karavaev", "Sobolev", "Mostovoy", "Kuzyaev", "Pinyaev", "Glebov", "Fomin", "Diveev", "Dzhikiya", "Osipenko", "Maksimenko", "Siljanov", "Tyukavin", "Krugovoy", "Karpov", "Cheryshev" };

        private string[] beFirst = { "Kevin", "Romelu", "Eden", "Thibaut", "Jeremy", "Amadou", "Youri", "Lois", "Timothy", "Wout", "Arthur", "Zeno", "Orel", "Koen", "Thomas", "Yannick", "Charles", "Leandro", "Jan", "Toby", "Alexis", "Johan", "Dodi", "Matz" };
        private string[] beLast = { "De Bruyne", "Lukaku", "Hazard", "Courtois", "Doku", "Onana", "Tielemans", "Openda", "Castagne", "Faes", "Theate", "Debast", "Mangala", "Casteels", "Meunier", "Carrasco", "De Ketelaere", "Trossard", "Vertonghen", "Alderweireld", "Saelemaekers", "Bakayoko" };

        private string[] brFirst = { "Neymar", "Vinicius", "Rodrygo", "Richarlison", "Gabriel", "Casemiro", "Bruno", "Lucas", "Alisson", "Ederson", "Marquinhos", "Eder", "Danilo", "Bremer", "Douglas", "Raphinha", "Endrick", "Savio", "Andreas", "Joao", "Matheus", "Pedro", "Vitor", "André" };
        private string[] brLast = { "Junior", "Rodrigues", "Silva", "Guimaraes", "Paqueta", "Becker", "Moraes", "Militao", "Augusto", "Luiz", "Dias", "Roque", "Bento", "Martinelli", "Pereira", "Nascimento", "Coutinho", "Marcelo", "Fred" };

        private class LeagueMeta
        {
            public string Name;
            public string Country;
            public int Tier;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDatabase();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeDatabase()
        {
            GenerateMockSponsors();
            GenerateSimulationLeagues();
        }

        public void ResetDatabase()
        {
            Clubs.Clear();
            Players.Clear();
            Sponsors.Clear();
            Leagues.Clear();
            InitializeDatabase();
        }

        private void GenerateMockSponsors()
        {
            Sponsors.Add(new Sponsor("Nike", 2500, 3, 75));
            Sponsors.Add(new Sponsor("Adidas", 2800, 3, 78));
            Sponsors.Add(new Sponsor("Puma", 1800, 2, 70));
            Sponsors.Add(new Sponsor("Red Bull", 4000, 4, 82));
            Sponsors.Add(new Sponsor("Turkish Airlines", 5000, 5, 85));
            Sponsors.Add(new Sponsor("EA Sports", 3500, 3, 80));
            Sponsors.Add(new Sponsor("Rolex", 8000, 4, 88));
            Sponsors.Add(new Sponsor("Local Kebab", 200, 1, 45));
            Sponsors.Add(new Sponsor("Castrol", 1000, 2, 60));
            Sponsors.Add(new Sponsor("Heineken", 3000, 3, 75));
        }

        private void GenerateSimulationLeagues()
        {
            List<LeagueMeta> leagueMetas = new List<LeagueMeta>
            {
                new LeagueMeta { Name = "Türkiye 1. Ligi", Country = "Türkiye", Tier = 1 },
                new LeagueMeta { Name = "Türkiye 2. Ligi", Country = "Türkiye", Tier = 2 },
                new LeagueMeta { Name = "Türkiye 3. Ligi", Country = "Türkiye", Tier = 3 },
                
                new LeagueMeta { Name = "İngiltere 1. Ligi", Country = "İngiltere", Tier = 1 },
                new LeagueMeta { Name = "İngiltere 2. Ligi", Country = "İngiltere", Tier = 2 },
                new LeagueMeta { Name = "İngiltere 3. Ligi", Country = "İngiltere", Tier = 3 },
                
                new LeagueMeta { Name = "İspanya 1. Ligi", Country = "İspanya", Tier = 1 },
                new LeagueMeta { Name = "İspanya 2. Ligi", Country = "İspanya", Tier = 2 },
                new LeagueMeta { Name = "İspanya 3. Ligi", Country = "İspanya", Tier = 3 },
                
                new LeagueMeta { Name = "Fransa 1. Ligi", Country = "Fransa", Tier = 1 },
                new LeagueMeta { Name = "Fransa 2. Ligi", Country = "Fransa", Tier = 2 },
                new LeagueMeta { Name = "Fransa 3. Ligi", Country = "Fransa", Tier = 3 },
                
                new LeagueMeta { Name = "Almanya 1. Ligi", Country = "Almanya", Tier = 1 },
                new LeagueMeta { Name = "Almanya 2. Ligi", Country = "Almanya", Tier = 2 },
                new LeagueMeta { Name = "Almanya 3. Ligi", Country = "Almanya", Tier = 3 },
                
                new LeagueMeta { Name = "İtalya 1. Ligi", Country = "İtalya", Tier = 1 },
                new LeagueMeta { Name = "İtalya 2. Ligi", Country = "İtalya", Tier = 2 },
                new LeagueMeta { Name = "İtalya 3. Ligi", Country = "İtalya", Tier = 3 },
                
                new LeagueMeta { Name = "Portekiz 1. Ligi", Country = "Portekiz", Tier = 1 },
                new LeagueMeta { Name = "Portekiz 2. Ligi", Country = "Portekiz", Tier = 2 },
                
                new LeagueMeta { Name = "Hollanda 1. Ligi", Country = "Hollanda", Tier = 1 },
                new LeagueMeta { Name = "Hollanda 2. Ligi", Country = "Hollanda", Tier = 2 },
                
                new LeagueMeta { Name = "Rusya 1. Ligi", Country = "Rusya", Tier = 1 },
                new LeagueMeta { Name = "Rusya 2. Ligi", Country = "Rusya", Tier = 2 },
                
                new LeagueMeta { Name = "Belçika 1. Ligi", Country = "Belçika", Tier = 1 },
                new LeagueMeta { Name = "Belçika 2. Ligi", Country = "Belçika", Tier = 2 },
                
                new LeagueMeta { Name = "Brezilya 1. Ligi", Country = "Brezilya", Tier = 1 },
                new LeagueMeta { Name = "Brezilya 2. Ligi", Country = "Brezilya", Tier = 2 },
                new LeagueMeta { Name = "Brezilya 3. Ligi", Country = "Brezilya", Tier = 3 }
            };

            foreach (var meta in leagueMetas)
            {
                League league = new League(meta.Name, meta.Country, meta.Tier);
                string[] clubNames = GetLeagueClubNames(meta.Name);

                for (int i = 0; i < 18; i++)
                {
                    string name = clubNames[i];
                    
                    // Assign prestige and budget depending on index (maintains dynamic performance tiering)
                    int prestige;
                    int transferBudget;
                    int wageBudget;

                    if (meta.Tier == 1)
                    {
                        if (i < 3) { prestige = UnityEngine.Random.Range(85, 95); transferBudget = UnityEngine.Random.Range(80000000, 160000000); wageBudget = UnityEngine.Random.Range(250000, 400000); }
                        else if (i < 8) { prestige = UnityEngine.Random.Range(76, 84); transferBudget = UnityEngine.Random.Range(30000000, 60000000); wageBudget = UnityEngine.Random.Range(100000, 240000); }
                        else if (i < 14) { prestige = UnityEngine.Random.Range(64, 75); transferBudget = UnityEngine.Random.Range(8000000, 20000000); wageBudget = UnityEngine.Random.Range(40000, 90000); }
                        else { prestige = UnityEngine.Random.Range(50, 63); transferBudget = UnityEngine.Random.Range(2000000, 6000000); wageBudget = UnityEngine.Random.Range(15000, 35000); }
                    }
                    else if (meta.Tier == 2)
                    {
                        if (i < 3) { prestige = UnityEngine.Random.Range(64, 72); transferBudget = UnityEngine.Random.Range(6000000, 12000000); wageBudget = UnityEngine.Random.Range(35000, 60000); }
                        else if (i < 8) { prestige = UnityEngine.Random.Range(56, 63); transferBudget = UnityEngine.Random.Range(3000000, 5000000); wageBudget = UnityEngine.Random.Range(20000, 30000); }
                        else if (i < 14) { prestige = UnityEngine.Random.Range(48, 55); transferBudget = UnityEngine.Random.Range(1000000, 2500000); wageBudget = UnityEngine.Random.Range(10000, 18000); }
                        else { prestige = UnityEngine.Random.Range(40, 47); transferBudget = UnityEngine.Random.Range(400000, 800000); wageBudget = UnityEngine.Random.Range(5000, 9000); }
                    }
                    else // Tier 3
                    {
                        if (i < 3) { prestige = UnityEngine.Random.Range(44, 50); transferBudget = UnityEngine.Random.Range(1200000, 2500000); wageBudget = UnityEngine.Random.Range(12000, 18000); }
                        else if (i < 8) { prestige = UnityEngine.Random.Range(38, 43); transferBudget = UnityEngine.Random.Range(600000, 1000000); wageBudget = UnityEngine.Random.Range(7000, 11000); }
                        else if (i < 14) { prestige = UnityEngine.Random.Range(32, 37); transferBudget = UnityEngine.Random.Range(250000, 500000); wageBudget = UnityEngine.Random.Range(4000, 6500); }
                        else { prestige = UnityEngine.Random.Range(24, 31); transferBudget = UnityEngine.Random.Range(80000, 200000); wageBudget = UnityEngine.Random.Range(2000, 3500); }
                    }

                    Club club = new Club(name, prestige, meta.Name, transferBudget, wageBudget);
                    Clubs.Add(club);
                    league.Clubs.Add(club);

                    // Generate exactly 15 squad players for this club (1 GK, 5 DEF, 5 MID, 4 FWD)
                    GenerateSquadForClub(club, meta.Country, meta.Tier);
                }

                Leagues.Add(league);
            }
        }

        private void GenerateSquadForClub(Club club, string country, int tier)
        {
            PlayerPosition[] positions = {
                PlayerPosition.GK, PlayerPosition.GK,
                PlayerPosition.DEF, PlayerPosition.DEF, PlayerPosition.DEF, PlayerPosition.DEF, PlayerPosition.DEF, PlayerPosition.DEF, PlayerPosition.DEF,
                PlayerPosition.MID, PlayerPosition.MID, PlayerPosition.MID, PlayerPosition.MID, PlayerPosition.MID, PlayerPosition.MID, PlayerPosition.MID,
                PlayerPosition.FWD, PlayerPosition.FWD, PlayerPosition.FWD, PlayerPosition.FWD, PlayerPosition.FWD, PlayerPosition.FWD
            };

            foreach (var pos in positions)
            {
                string nationality;
                string fullName = GenerateFictionalPlayerName(country, out nationality);

                int age = UnityEngine.Random.Range(17, 35);
                
                // Determine realistic starting OVR based on age cap and club prestige
                int baseOvr = club.Prestige; // Prestige generally ranges between 50 and 90
                int ovr = baseOvr + UnityEngine.Random.Range(-12, 6);
                
                // Cap young player overalls to ensure realism (Wonderkids grow into superstars!)
                if (age == 17) ovr = Mathf.Clamp(ovr, 45, 68);
                else if (age == 18) ovr = Mathf.Clamp(ovr, 48, 72);
                else if (age == 19) ovr = Mathf.Clamp(ovr, 52, 75);
                else if (age == 20) ovr = Mathf.Clamp(ovr, 55, 78);
                else if (age == 21) ovr = Mathf.Clamp(ovr, 58, 82);
                else if (age == 22) ovr = Mathf.Clamp(ovr, 60, 85);
                else // Peak/Experienced age groups (23-34)
                {
                    ovr = Mathf.Clamp(ovr, 50, 95);
                }

                // Calculate potential dynamically based on age bracket
                int pot = ovr;
                if (age < 22)
                {
                    pot = ovr + UnityEngine.Random.Range(10, 24); // High ceiling for youth
                }
                else if (age < 26)
                {
                    pot = ovr + UnityEngine.Random.Range(4, 15);
                }
                else if (age < 30)
                {
                    pot = ovr + UnityEngine.Random.Range(0, 6);
                }
                pot = Mathf.Clamp(pot, ovr, 99);

                // Hard Tier caps for OVR and POT to guarantee realistic divisions!
                if (tier == 2)
                {
                    ovr = Mathf.Min(ovr, 78); // Tier 2 Cap: 78 OVR
                    pot = Mathf.Min(pot, 88);
                }
                else if (tier == 3)
                {
                    ovr = Mathf.Min(ovr, 68); // Tier 3 Cap: 68 OVR
                    pot = Mathf.Min(pot, 78);
                }
                pot = Mathf.Max(pot, ovr);

                Player p = new Player(fullName, age, pos, ovr, pot);
                p.Nationality = nationality;

                // FIFA / EA FC realistic weekly wage tiers based on OVR
                float wageVal = 0f;
                if (ovr < 50) wageVal = UnityEngine.Random.Range(500, 1500);
                else if (ovr < 60) wageVal = Mathf.Lerp(1500, 5000, (ovr - 50) / 10f);
                else if (ovr < 70) wageVal = Mathf.Lerp(5000, 15000, (ovr - 60) / 10f);
                else if (ovr < 80) wageVal = Mathf.Lerp(15000, 55000, (ovr - 70) / 10f);
                else if (ovr < 85) wageVal = Mathf.Lerp(55000, 110000, (ovr - 80) / 5f);
                else if (ovr < 90) wageVal = Mathf.Lerp(110000, 240000, (ovr - 85) / 5f);
                else wageVal = Mathf.Lerp(240000, 550000, (ovr - 90) / 9f);

                // Adjust based on club prestige (richer clubs pay more, smaller clubs pay less)
                float prestigeFactor = club.Prestige / 80f;
                int wage = Mathf.RoundToInt(wageVal * prestigeFactor);
                wage = Mathf.Clamp(wage, 500, 650000);

                int contractYears = UnityEngine.Random.Range(1, 6);

                // Set a temporary contract first to update market value for release clause calculations
                Contract tempContract = new Contract(club.Id, club.Name, wage, contractYears, 0);
                p.CurrentContract = tempContract;
                p.UpdateMarketValue();

                int releaseClause = 0;
                if (p.OVR > 70 && UnityEngine.Random.value < 0.25f)
                {
                    releaseClause = Mathf.RoundToInt(p.MarketValue * UnityEngine.Random.Range(1.3f, 2.2f));
                }

                Contract contract = new Contract(club.Id, club.Name, wage, contractYears, releaseClause);
                club.AddPlayer(p, contract);

                // Assign sponsor randomly for high OVR
                if (p.OVR >= 72 && UnityEngine.Random.value < 0.35f)
                {
                    List<Sponsor> eligible = Sponsors.FindAll(s => p.OVR >= s.MinOVRRequired);
                    if (eligible.Count > 0)
                    {
                        p.ActiveSponsor = eligible[UnityEngine.Random.Range(0, eligible.Count)];
                    }
                }

                Players.Add(p);
            }
        }

        public Player GenerateRegenPlayer(PlayerPosition position, Club club)
        {
            // Determine tier of the club's league
            int tier = 1;
            if (club.League.Contains("2. Ligi")) tier = 2;
            else if (club.League.Contains("3. Ligi")) tier = 3;

            // 70% chance local league country, 30% chance diverse global pool!
            string country = "Türkiye";
            if (club.League.Contains("İngiltere")) country = "İngiltere";
            else if (club.League.Contains("İspanya")) country = "İspanya";
            else if (club.League.Contains("Fransa")) country = "Fransa";
            else if (club.League.Contains("Almanya")) country = "Almanya";
            else if (club.League.Contains("İtalya")) country = "İtalya";
            else if (club.League.Contains("Portekiz")) country = "Portekiz";
            else if (club.League.Contains("Hollanda")) country = "Hollanda";
            else if (club.League.Contains("Rusya")) country = "Rusya";
            else if (club.League.Contains("Belçika")) country = "Belçika";
            else if (club.League.Contains("Brezilya")) country = "Brezilya";

            if (UnityEngine.Random.value < 0.3f)
            {
                string[] globalPool = { "Arjantin", "Ukrayna", "Amerika", "Nijerya", "Senegal", "Kamerun", "Fildişi Sahili", "Gana" };
                country = globalPool[UnityEngine.Random.Range(0, globalPool.Length)];
            }

            // Map global pool to local name lists for realistic name parts
            string namePoolCountry = country;
            if (country == "Arjantin") namePoolCountry = "İspanya";
            else if (country == "Amerika") namePoolCountry = "İngiltere";
            else if (country == "Ukrayna") namePoolCountry = "Rusya";
            else if (country == "Nijerya" || country == "Senegal" || country == "Kamerun" || country == "Fildişi Sahili" || country == "Gana")
            {
                namePoolCountry = UnityEngine.Random.value < 0.5f ? "Fransa" : "İngiltere";
            }

            string nationality;
            string fullName = GenerateFictionalPlayerName(namePoolCountry, out nationality);
            nationality = country; // Display the correct global country!

            int age = UnityEngine.Random.Range(17, 20); // Start young (Wonderkids)
            
            int ovr = UnityEngine.Random.Range(50, 68);
            if (tier == 2) ovr = Mathf.Clamp(ovr, 45, 60);
            else if (tier == 3) ovr = Mathf.Clamp(ovr, 40, 52);

            int pot = ovr + UnityEngine.Random.Range(12, 26);
            if (tier == 2) pot = Mathf.Min(pot, 80);
            else if (tier == 3) pot = Mathf.Min(pot, 72);
            pot = Mathf.Clamp(pot, ovr, 99);

            Player p = new Player(fullName, age, position, ovr, pot);
            p.Nationality = nationality;

            // 50% chance a youth academy regen has a competitor agent, 50% chance they don't!
            p.HasAgent = UnityEngine.Random.value < 0.5f;

            // Fictional rookie wage
            float wageVal = UnityEngine.Random.Range(800, 2500);
            float prestigeFactor = club.Prestige / 80f;
            int wage = Mathf.RoundToInt(wageVal * prestigeFactor);
            wage = Mathf.Clamp(wage, 500, 15000);

            int contractYears = UnityEngine.Random.Range(3, 6);
            Contract contract = new Contract(club.Id, club.Name, wage, contractYears, 0);
            club.AddPlayer(p, contract);

            return p;
        }

        public string GenerateFictionalPlayerName(string country, out string selectedCountry)
        {
            selectedCountry = country;
            // 30% chance for foreign signing diversity
            if (UnityEngine.Random.value < 0.3f)
            {
                string[] allCountries = { "Türkiye", "İngiltere", "İspanya", "Fransa", "Almanya", "İtalya", "Portekiz", "Hollanda", "Rusya", "Belçika", "Brezilya" };
                selectedCountry = allCountries[UnityEngine.Random.Range(0, allCountries.Length)];
            }

            string first;
            string last;

            switch (selectedCountry)
            {
                case "Türkiye":
                    first = trFirst[UnityEngine.Random.Range(0, trFirst.Length)];
                    last = trLast[UnityEngine.Random.Range(0, trLast.Length)];
                    break;
                case "İngiltere":
                    first = enFirst[UnityEngine.Random.Range(0, enFirst.Length)];
                    last = enLast[UnityEngine.Random.Range(0, enLast.Length)];
                    break;
                case "İspanya":
                    first = esFirst[UnityEngine.Random.Range(0, esFirst.Length)];
                    last = esLast[UnityEngine.Random.Range(0, esLast.Length)];
                    break;
                case "Fransa":
                    first = frFirst[UnityEngine.Random.Range(0, frFirst.Length)];
                    last = frLast[UnityEngine.Random.Range(0, frLast.Length)];
                    break;
                case "Almanya":
                    first = deFirst[UnityEngine.Random.Range(0, deFirst.Length)];
                    last = deLast[UnityEngine.Random.Range(0, deLast.Length)];
                    break;
                case "İtalya":
                    first = itFirst[UnityEngine.Random.Range(0, itFirst.Length)];
                    last = itLast[UnityEngine.Random.Range(0, itLast.Length)];
                    break;
                case "Portekiz":
                    first = ptFirst[UnityEngine.Random.Range(0, ptFirst.Length)];
                    last = ptLast[UnityEngine.Random.Range(0, ptLast.Length)];
                    break;
                case "Hollanda":
                    first = nlFirst[UnityEngine.Random.Range(0, nlFirst.Length)];
                    last = nlLast[UnityEngine.Random.Range(0, nlLast.Length)];
                    break;
                case "Rusya":
                    first = ruFirst[UnityEngine.Random.Range(0, ruFirst.Length)];
                    last = ruLast[UnityEngine.Random.Range(0, ruLast.Length)];
                    break;
                case "Belçika":
                    first = beFirst[UnityEngine.Random.Range(0, beFirst.Length)];
                    last = beLast[UnityEngine.Random.Range(0, beLast.Length)];
                    break;
                case "Brezilya":
                    first = brFirst[UnityEngine.Random.Range(0, brFirst.Length)];
                    last = brLast[UnityEngine.Random.Range(0, brLast.Length)];
                    break;
                default:
                    first = trFirst[UnityEngine.Random.Range(0, trFirst.Length)];
                    last = trLast[UnityEngine.Random.Range(0, trLast.Length)];
                    selectedCountry = "Türkiye";
                    break;
            }

            return first + " " + last;
        }

        public Player GetPlayerById(string id)
        {
            return Players.Find(p => p.Id == id);
        }

        public Club GetClubById(string id)
        {
            return Clubs.Find(c => c.Id == id);
        }

        public Club GetClubByName(string name)
        {
            return Clubs.Find(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public void TransferPlayer(Player player, Club targetClub, Contract newContract, int fee)
        {
            Club currentClub = null;
            if (player.CurrentContract != null)
            {
                currentClub = GetClubById(player.CurrentContract.ClubId);
                if (currentClub != null)
                {
                    currentClub.RemovePlayer(player);
                    currentClub.TransferBudget += fee;
                }
            }

            targetClub.TransferBudget -= fee;

            PlayerHistoryEntry history = new PlayerHistoryEntry
            {
                Year = SimulationEngine.Instance != null ? SimulationEngine.Instance.CurrentYear.ToString() : "Current",
                ClubName = currentClub != null ? currentClub.Name : "Free Agent",
                Appearances = player.Appearances,
                Goals = player.Goals,
                Assists = player.Assists,
                CleanSheets = player.CleanSheets,
                AverageRating = player.AverageRating,
                TransferFee = fee
            };
            player.History.Add(history);

            targetClub.AddPlayer(player, newContract);
            player.UpdateMarketValue();
            player.Happiness = Mathf.Clamp(player.Happiness + 15f, 50f, 100f);
        }

        private string[] GetLeagueClubNames(string leagueName)
        {
            switch (leagueName)
            {
                // Türkiye
                case "Türkiye 1. Ligi":
                    return new string[] { "İstanbul Aslanları", "Kadıköy Kanaryaları", "Boğaz Kartalları", "Karadeniz Fırtınası", "Başkent Gücü", "Adana Şimşekleri", "Horozlar Denizli", "Yeşil Timsahlar", "Anadolu Kaplanları", "Yiğidolar Sivas", "Akdeniz Akrepleri", "Körfez Gücü", "Kırmızı Şimşekler", "İzmir Göztepe", "Kaf-Kaf Karşıyaka", "Toros Kaplanları", "Samsun Kırmızıları", "Başakşehir Baykuşları" };
                case "Türkiye 2. Ligi":
                    return new string[] { "Rize Çaycıları", "Bodrum Mavileri", "Pendik Gücü", "Ümraniye FK", "Eyüp Hilal", "Sakarya Tatangalar", "Altay Siyahlar", "Manisa Tarzanları", "Bolu Yarenleri", "Giresun Çotanaklar", "Gaziantep Şahinler", "Hatay Asileri", "Alanya Portakalları", "Karagümrük Kırmızıları", "Kasımpaşa Lacivertler", "Bandırma Bordo", "Tuzla Tuzcular", "Keçiörengücü" };
                case "Türkiye 3. Ligi":
                    return new string[] { "Diyarbakır Kaplanları", "Van Kedileri", "Mersin İdman", "Çorum Kırmızıları", "Şanlıurfa Sarıları", "Batman Petrol", "Elazığ Gakkoşlar", "Afyon Şimşekler", "Isparta Gülleri", "İnegöl Bordo", "Düzce Yeşiller", "Sarıyer Beyazlar", "Altınordu Gençleri", "Menemen Sarılar", "Fethiye Lacivert", "Uşak Kırmızı", "Kırklareli Spor", "Kastamonu Orman" };
 
                // İngiltere
                case "İngiltere 1. Ligi":
                    return new string[] { "Manchester Kırmızıları", "Manchester Mavileri", "Merseyside Kırmızıları", "Merseyside Mavileri", "Londra Topçuları", "Londra Mavileri", "Londra Horozları", "Londra Çekiçleri", "Kuzey Saksağanları", "Birmingham Aslanları", "Leicester Tilkileri", "Kurtlar Wolverhampton", "Nottingham Ormanı", "Azizler Southampton", "Londra Kartalları", "Londra Arıları", "Yorkshire Beyazları", "Brighton Martıları" };
                case "İngiltere 2. Ligi":
                    return new string[] { "Sheffield Bıçakları", "Sheffield Baykuşları", "Blackburn Nehirleri", "Norwich Kanaryaları", "Koçlar Derby", "Liman Şehri Portsmouth", "Sunderland Kedileri", "Watford Eşekarıları", "Coventry Gökyüzü", "Middlesbrough Kırmızıları", "Fulham Kırları", "Bristol Robins", "Swansea Kuğuları", "Cardiff Mavi Kuşları", "QPR Korucuları", "Millwall Aslanları", "Plymouth Hacılar", "Preston Kuzey" };
                case "İngiltere 3. Ligi":
                    return new string[] { "Wigan Atletik", "Bolton Gezginleri", "Charlton Vadisi", "Reading Kralları", "Blackpool Turuncuları", "Lincoln İblisleri", "Peterborough Maviler", "Barnsley Tykes", "Wycombe Sandalyeciler", "Leyton Oryantal", "Oxford United", "Shrewsbury Salop", "Northampton Kunduracılar", "Bristol Korsanları", "Exeter Yunanlılar", "Port Vale Kimyacılar", "Fleetwood Balıkçılar", "Cambridge Akademisyenler" };
 
                // İspanya
                case "İspanya 1. Ligi":
                    return new string[] { "Madrid Beyazları", "Katalan Mavileri", "Madrid Çizgilileri", "Endülüs Beyazları", "Endülüs Yeşilleri", "Bask Aslanları", "Bask Mavileri", "Valencia Yarasaları", "Sarı Denizaltılar", "Galiçya Gök Mavileri", "Ada Kırmızıları", "Katalonya Kırmızıları", "Kanarya Adaları", "Navarra Kırmızıları", "Getafe Mavileri", "Vallecas Şimşekleri", "Alaves Mavileri", "Granada Kırmızısı" };
                case "İspanya 2. Ligi":
                    return new string[] { "Galiçya Mavileri", "Barselona Beyazları", "Zaragoza Mavileri", "Valladolid Morları", "Tenerife Mavileri", "Oviedo Mavileri", "Elche Yeşilleri", "Levante Kurbağaları", "Eibar Silahşörleri", "Burgos Şövalyeleri", "Leganes Salatalıklar", "Castellon Siyahları", "Murcia Kırmızıları", "Almeria Kırmızıları", "Sporting Gijon", "Huesca Kırmızı Mavileri", "Racing Santander", "Cartagena Siyah Beyaz" };
                case "İspanya 3. Ligi":
                    return new string[] { "Recreativo Yaşlılar", "Castellon Gençleri", "Ibiza Gece", "Malaga Mavileri", "Cordoba Yeşilleri", "Nastic Tarragona", "Ceuta Kuzey", "Melilla Afrika", "Lugo Galiçya", "Ponferradina Mavi Bordo", "Sabadell Katalan", "Alcoyano İnançlılar", "Sestao Nehir", "Barakaldo Sarı Siyah", "Real Union Bask", "Tarazona Kırmızı", "Teruel Çöl", "Antequera Yeşil" };
 
                // Fransa
                case "Fransa 1. Ligi":
                    return new string[] { "Paris Mavileri", "Marsilya Limanı", "Lyon Aslanları", "Monako Sarayı", "Lille Tazıları", "Rennes Kırmızıları", "Nice Kartalları", "Lens Madencileri", "Nantes Kanaryaları", "Strasbourg Mavi Beyaz", "Montpellier Turuncu", "Toulouse Menekşe", "Reims Taç", "Auxerre Beyazları", "Yeşiller Saint-Etienne", "Bordeaux Bağları", "Metz Ejderleri", "Lorient Morinaları" };
                case "Fransa 2. Ligi":
                    return new string[] { "Troyes Mavileri", "Angers Siyah Beyaz", "Brest Limanı", "Caen Vikingleri", "Dijon Hardalları", "Guingamp Kırmızıları", "Nancy Thistle", "Sochaux Aslanları", "Valenciennes Kırmızı", "Le Havre Maviler", "Bastia Şahinleri", "Amiens Tek boynuzlar", "Grenoble Alpleri", "Ajaccio İmparatorları", "Laval Portakalları", "Rodez Kırmızıları", "Pau Yeşilleri", "Paris FC Yeşiller" };
                case "Fransa 3. Ligi":
                    return new string[] { "Nimes Timsahları", "Niort Süvarileri", "Chateauroux Maviler", "Red Star Paris", "Versay Sarayı", "Orleans Arıları", "Le Mans Horozları", "Boulogne Denizciler", "Quevilly Kırmızı", "Concarneau Balıkçı", "Avranches Tepesi", "Epinal Yeşilleri", "Villefranche Kaplan", "Rouen Kırmızıları", "Marignane Maviler", "Martigues Sarılar", "Nancy Gençleri", "Cholet Mavileri" };
 
                // Almanya
                case "Almanya 1. Ligi":
                    return new string[] { "Münih Kırmızıları", "Dortmund Sarıları", "Leipzig Boğaları", "Leverkusen İşçileri", "Frankfurt Kartalları", "Stuttgart Atları", "Bremen Mızıkacıları", "Hamburg Dinozorları", "Köln Tekeleri", "Berlin Birlikleri", "Gelsenkirchen Madencileri", "Mönchengladbach Tayları", "Mainz Karnaval", "Augsburg Fugger", "Wolfsburg Kurtları", "Hoffenheim Köyü", "Freiburg Çamları", "Heidenheim Şatoları" };
                case "Almanya 2. Ligi":
                    return new string[] { "Darmstadt Zambaklar", "Düsseldorf Fortunaları", "Hannover 96lar", "Karlsruhe Mavileri", "Nürnberg Kulübü", "Kaiserslautern Şeytanları", "Hertha Berlin", "Schalke 04 Muadili", "St. Pauli Korsanları", "Rostock Hanse", "Bielefeld Arminia", "Dresden Dinamoları", "Paderborn Mavileri", "Magdeburg Maviler", "Fürth Yaprakları", "Kiel Leylekleri", "Osnabrück Morları", "Wiesbaden Sarılar" };
                case "Almanya 3. Ligi":
                    return new string[] { "Duisburg Zebraları", "1860 Münih Muadili", "Saarbrücken Maviler", "Essen Kırmızı Beyaz", "Halle Kimyagerleri", "Aue Madencileri", "Regensburg Jahn", "Sandhausen Siyah", "Ulm Serçeleri", "Münster Kartalları", "Unterhaching Bob", "Lübeck Yeşil", "Viktoria Köln", "Verl Siyah Beyaz", "Ingolstadt Şanzıman", "Dortmund Rezerv", "Münih Rezerv", "Freiburg Rezerv" };
 
                // İtalya
                case "İtalya 1. Ligi":
                    return new string[] { "Torino Siyah Beyazları", "Milano Kırmızı Siyahları", "Milano Mavi Siyahları", "Napoli Gök Mavileri", "Roma Kurtları", "Roma Kartalları", "Floransa Menekşeleri", "Bergamo Tanrıçaları", "Torino Boğaları", "Cenova Kırmızı Mavileri", "Cenova Çizgilileri", "Bologna Kırmızı Mavileri", "Verona Sarıları", "Sardinya Kırmızı Mavileri", "Lecce Kurtları", "Monza Kırmızıları", "Empoli Mavileri", "Udine Siyah Beyaz" };
                case "İtalya 2. Ligi":
                    return new string[] { "Sicilya Pembeleri", "Bari Horozları", "Venedik Gondolları", "Como Gölleri", "Parma Dükleri", "Cremonese Grileri", "Pisa Kuleleri", "Reggiana Granat", "Catanzaro Kartalları", "Brescia Kırlangıçları", "Modena Kanaryaları", "Spezia Kartalları", "Ternana Canavarları", "Ascoli Ağaçları", "Cosenza Kurtları", "Lecco Gölleri", "Feralpisalo Yeşilleri", "Sudtirol Dağcıları" };
                case "İtalya 3. Ligi":
                    return new string[] { "Padova Kırmızıları", "Vicenza Çizgilileri", "Triestina Alabard", "Pescara Yunusları", "Spal Mavileri", "Perugia Grifonları", "Ancona Kırmızıları", "Lucchese Panterleri", "Siena Siyah Beyaz", "Novara Mavileri", "Pro Vercelli Aslan", "Taranto Yunusları", "Foggia Şeytanları", "Avellino Kurtları", "Benevento Cadıları", "Crotone Köpekbalığı", "Messina Kalkan", "Catania Filleri" };
 
                // Portekiz
                case "Portekiz 1. Ligi":
                    return new string[] { "Lizbon Aslanları", "Ejderha Porto", "Lizbon Kartalları", "Braga Savaşçıları", "Guimaraes Fatihleri", "Famalicao Mavileri", "Arouca Sarı", "Moreira Yeşilleri", "Portimao Siyah", "Faro Kurtları", "Chaves Anahtarları", "Vizela Mavileri", "Estoril Sarıları", "Barcelos Horozları", "Rio Ave Nehir", "Funchal Adalılar", "Ponta Delgada Ada", "Boavista Satranç" };
                case "Portekiz 2. Ligi":
                    return new string[] { "Penafiel Kırmızı", "Feirense Mavileri", "Tondela Yeşilleri", "Academico Viseu", "Mafra Sarıları", "Leiria Kalesi", "Torres Vedras Boğa", "Oliveirense Kırmızı", "Santa Clara Ada", "Lank Vilaverdense", "Belenenses Mavi", "Nacional Funchal", "Maritimo Yeşil Kırmızı", "Porto B Muadili", "Benfica B Muadili", "Sporting B Muadili", "Pacos Ferreira", "Penafiel Gücü" };
 
                // Hollanda
                case "Hollanda 1. Ligi":
                    return new string[] { "Amsterdam Tanrıları", "Rotterdam Limanı", "Eindhoven Çiftçileri", "Enschede Atları", "Alkmaar Peynircileri", "Utrecht Kırmızı", "Arnhem Kartalları", "Nijmegen Kırmızı Yeşil", "Heerenveen Kalpleri", "Zwolle Mavileri", "Almere Kara", "Deventer Kartalları", "Sittard Sarıları", "Waalwijk Mavileri", "Volendam Balıkçı", "Leeuwarden Geyikleri", "Groningen Yeşilleri", "Tilburg Kralları" };
                case "Hollanda 2. Ligi":
                    return new string[] { "Breda Fareleri", "Kerkrade Madencileri", "Venlo VVV", "Den Haag Kuğuları", "Doetinchem Süvarileri", "Emmen Yeşilleri", "Eindhoven FC", "Dordrecht Koyunları", "Maastricht Yıldız", "Den Bosch Ejderler", "Oss Boğaları", "Helmond Kedileri", "Jong Ajax Muadili", "Jong PSV Muadili", "Jong AZ Muadili", "Jong Utrecht Muadili", "Telstar Beyazları", "Cambuur Sarıları" };
 
                // Rusya
                case "Rusya 1. Ligi":
                    return new string[] { "Zenit Sankt-Peterburg", "Lokomotiv Moskova", "CSKA Moskova", "Spartak Moskova", "Krasnodar Boğaları", "Dinamo Moskova", "Rostov Sarı Lacivert", "Soçi Denizciler", "Samara Kanatları", "Rubin Kazan", "Grozny Çeçenleri", "Nizhny Novgorod", "Ural Turuncu", "Orenburg Gazı", "Fakel Meşaleleri", "Baltika Kaliningrad", "Khimki Kırmızı", "Tula Cephane" };
                case "Rusya 2. Ligi":
                    return new string[] { "Torpedo Moskova", "Yaroslavl Şinik", "Saratov Şahinler", "Volgograd Rotor", "Yenisey Sibirya", "Tyumen Kar", "Makhachkala Dinamo", "Chernomorets Deniz", "SKA Khabarovsk", "Neftekhimik Petrol", "Kamaz Kamyon", "Kuban Krasnodar", "Alania Vladikavkaz", "Leningradets", "Sokol Saratov", "Volgar Astrakhan", "Ufa Spor", "Shinnik Yaroslavl" };
 
                // Belçika
                case "Belçika 1. Ligi":
                    return new string[] { "Anderlecht Eflatun", "Brugge Mavileri", "Gent Bufaloları", "Antwerp Kırmızı", "Genk Madencileri", "Liege Kırmızıları", "Charleroi Zebraları", "Kortrijk Kırmızı", "Mechelen Sarı Kırmızı", "Sint-Truiden Kanarya", "Westerlo Sarı Lacivert", "Eupen Siyah", "Leuven Beyaz", "Cercle Brugge", "Union Saint-Gilloise", "Beveren Sarı", "Lierse Kırmızı", "Waregem Yeşil" };
                case "Belçika 2. Ligi":
                    return new string[] { "Lommel Yeşilleri", "Deinze Turuncuları", "Seraing Kırmızı", "Virton Yeşilleri", "Ostend Deniz", "Zulte Waregem", "Patro Eisden", "RFC Liege", "Dender Lacivert", "Beveren Gücü", "Club Brugge B", "Anderlecht B", "Genk B", "Standard Liege B", "Francs Borains", "Lierse Kempen", "RFC Antwerp B", "RFC Gent B" };
 
                // Brezilya
                case "Brezilya 1. Ligi":
                    return new string[] { "Flamengo Kırmızı Siyah", "Palmeiras Yeşilleri", "Sao Paulo Üçrenkliler", "Santos Balıkları", "Gremio Ölümsüzleri", "Internacional Kırmızıları", "Atletico Horozları", "Cruzeiro Tilkileri", "Fluminense Savaşçıları", "Botafogo Yalnız Yıldız", "Vasco Devleri", "Bahia Üçrenkliler", "Fortaleza Aslanları", "Athletico Kasırgaları", "Coritiba Yeşiller", "Goias Papağanları", "Cuiaba Altınları", "Bragantino Boğaları" };
                case "Brezilya 2. Ligi":
                    return new string[] { "Sport Aslanları", "Santos Gençleri", "Ceara Siyah Beyaz", "Goias Gücü", "Coritiba Kaplanları", "Avai Aslanları", "Ponte Preta Köprü", "Guarani Yerlileri", "Novorizontino Kaplan", "Mirassol Sarılar", "America Tavşanları", "Operario Tren", "CRB Kırmızıları", "Vila Nova Kırmızı", "Chapecoense Yeşiller", "Brusque Çizgili", "Ituano Horozları", "Paysandu Mavileri" };
                case "Brezilya 3. Ligi":
                    return new string[] { "Figueirense Kasırga", "CSA Mavi Mutlu", "Botafogo PB Yıldız", "Volta Redonda Çelik", "Ypiranga Sarı Siyah", "Remo Denizciler", "Sao Bernardo Kaplan", "Confianca Ejder", "Ferroviario Ray", "ABC Siyah Beyaz", "Londrina Gök Mavi", "Tombense Kırmızı", "Sampaio Corrêa Üç", "Aparecidense Mavi", "Floresta Orman", "Sao Jose Mavi", "Ypiranga Kaplanları", "Remo Gücü" };

                default:
                    return new string[18];
            }
        }
    }
}
