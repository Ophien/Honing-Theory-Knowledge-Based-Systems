using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNN;

namespace SharpHoning
{
    class HS_HONING_OBJ
    {
        public void calibration(String hero, HSCardsParser parser, HoningStoneBuilder builder, HSCombosParser combosParser, Dictionary<String, double[]> cardDatasetFull, Dictionary<String, CardObject> cartTable, HSCardOperator op)
        {
            net = new HoningNetwork<string>();

            string[] lines = System.IO.File.ReadAllLines(hero + "_calibration.cal");
            char[] delim = new char[1];
            delim[0] = ' ';
            String highSurprise = lines[0].Split(delim).Last();
            String minSurprise = lines[1].Split(delim).Last();
            String highEfficiency = lines[2].Split(delim).Last();
            String minEfficiency = lines[3].Split(delim).Last();

            double.TryParse(highSurprise, out highSurp);
            double.TryParse(minSurprise, out minSurp);
            double.TryParse(highEfficiency, out highEff);
            double.TryParse(minEfficiency, out minEff);

            List<CardObject> heroCards = parser.objects[hero];
            List<CardObject> neutral = parser.objects["Neutral"];

            builder.PopulateFromCardData(ref net, ref heroCards);
            builder.BuildThirdLevel(ref net, ref heroCards);
            builder.PopulateFromCardData(ref net, ref neutral);
            builder.BuildThirdLevel(ref net, ref neutral);

            fixedBayes = new HSHoningBayes(hero.ToLower(), ref combosParser, ref net, ref cardDatasetFull, ref cartTable, 100000);
            fixedBayes.SaveKNNDataset(hero + "_data.dataset");
            Dataset fixedDataset = new Dataset(hero + "_data.dataset", ',');
            fixedKNN = new KNNEfficiency(fixedDataset);

            dic = net.getNetwork();
            selfAbilityFilter = op.GenerateAbilityFilter();
        }

        public HSHoningBayes fixedBayes;
        public KNNEfficiency fixedKNN;
        public HoningNetwork<String> net;
        public Dictionary<String, HoningNode<String>> dic;
        public Dictionary<String, String> selfAbilityFilter;
        public double highSurp;
        public double minSurp;
        public double highEff;
        public double minEff;
        
    }

    class HS_ARS_WEB
    {
        public void Instantiate(String jsonWithTypes, String ALYCOMBOOK, String allCardsWithAbility)
        {
            liteParser = new HSCardsParser(jsonWithTypes);
            fullParser = new HSCardsParser(allCardsWithAbility, 0);
            fullCParser = new HSCombosParser();fullCParser.PopulateFromJson(ALYCOMBOOK);
            HSBuilder = new HoningStoneBuilder();
            cardOperator = new HSCardOperator();
            cartTable = new Dictionary<string, CardObject>();
            cardsDataSet = cardOperator.generateCardVectors(fullParser, out dataID);

            // Populate all card table
            foreach (string key in liteParser.objects.Keys)
            {
                List<CardObject> cards_objects = liteParser.objects[key];
                HSBuilder.GenCardTableObject(ref cards_objects, ref cartTable);
            }

            honingOBJS = new Dictionary<string, HS_HONING_OBJ>();
            heroes = new String[9];
            heroes[0] = "Shaman"; heroes[1] = "Mage"; heroes[2] = "Warrior"; heroes[3] = "Druid"; heroes[4] = "Rogue"; heroes[5] = "Priest"; heroes[6] = "Paladin"; heroes[7] = "Warlock"; heroes[8] = "Hunter";
            for (int i = 0; i < 9; i++)
            {
                HS_HONING_OBJ h_obj = new HS_HONING_OBJ();
                h_obj.calibration(heroes[i], liteParser, HSBuilder, fullCParser, cardsDataSet, cartTable, cardOperator);
                honingOBJS.Add(heroes[i], h_obj);
            }

            dataID.Clear();
            liteParser.objects.Clear();
            fullParser.objects.Clear();
            fullCParser.combos_by_quantity.Clear();

            dataID = null;
            liteParser.objects = null;
            fullParser.objects = null;
            fullCParser.combos_by_quantity = null;

            GC.Collect(); 
            GC.WaitForPendingFinalizers();
        }

        public List<String> getCombo(String seed, String hero, int totalMana, int windowPercentage, int k, out double fitness, out double surprise, out double efficiency, out double normSurprise, out double normEfficiency)
        {
            List<String> combo = new List<string>();

            combo = cardOperator.GenerateCardCluster(
                    seed,
                    ref cartTable,
                    ref honingOBJS[hero].net,
                    ref honingOBJS[hero].selfAbilityFilter,
                    ref honingOBJS[hero].fixedBayes,
                    ref honingOBJS[hero].fixedKNN,
                    ref cardsDataSet,
                    totalMana,
                    k,
                    windowPercentage,
                    true,
                    ref honingOBJS[hero].highSurp,
                    ref honingOBJS[hero].minSurp,
                    ref honingOBJS[hero].highEff,
                    ref honingOBJS[hero].minEff,
                    out fitness,
                    out surprise,
                    out efficiency,
                    out normSurprise,
                    out normEfficiency).Keys.ToList();

            return combo;
        }

        public HSCardsParser liteParser;
            public HSCardsParser fullParser;
            public HSCombosParser fullCParser;
            public HoningStoneBuilder HSBuilder;
            public Dictionary<String, CardObject> cartTable;
            public HSCardOperator cardOperator;
            public Dictionary<String, int> dataID;
            public Dictionary<String, double[]> cardsDataSet;
            public String[] heroes;
            public Dictionary<String, HS_HONING_OBJ> honingOBJS;
    }
}
