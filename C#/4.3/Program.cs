using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNN;

namespace SharpHoning
{
    class Program
    {
        ComboNode ToComboNode(List<String> combo)
        {
            ComboNode node = new ComboNode();
            foreach (String s in combo)
            {
                if (!node.combo.ContainsKey(s))
                    node.combo.Add(s, s);
            }
            return node;
        }

        ComboNode ToComboNode(Dictionary<String, String> combo)
        {
            ComboNode node = new ComboNode();
            foreach (String s in combo.Keys)
            {
                node.combo.Add(s, s);
            }
            return node;
        }



        void ValidationTests(String cardsJson, String combosFile, int k)
        {
            HSCardsParser parser = new HSCardsParser(cardsJson);
            HoningStoneBuilder builder = new HoningStoneBuilder();

            Dictionary<String, CardObject> cardTable = new Dictionary<string, CardObject>();
            HSCardOperator op = new HSCardOperator();

            HSCardsParser fullParser = new HSCardsParser("allCardsWithAbility.json", 0);
            Dictionary<String, int> dataID;
            Dictionary<String, double[]> cardDatasetFull = op.generateCardVectors(fullParser, out dataID);

            // Populate all card table
            foreach (string key in parser.objects.Keys)
            {
                List<CardObject> cards_objects = parser.objects[key];
                builder.GenCardTableObject(ref cards_objects, ref cardTable);
            }

            HSCombosParser combosParser = new HSCombosParser();
            //combosParser.PopulateFromHoningNetwork(ref decksParser, ref cardTable, 5);
            combosParser.PopulateFromJson(combosFile);

            Random rand = new Random();

            List<CardObject> neutral = parser.objects["Neutral"];

            foreach (String hero in parser.objects.Keys)
            {
                // To write results
                System.IO.StreamWriter file = new System.IO.StreamWriter(hero + "_results.dat");
                if (hero != "Mage")
                    continue;

                List<String> honingCombo;

                HoningNetwork<String> net = new HoningNetwork<string>();
                List<CardObject> heroCards = parser.objects[hero];

                builder.PopulateFromCardData(ref net, ref heroCards);
                builder.BuildThirdLevel(ref net, ref heroCards);
                builder.PopulateFromCardData(ref net, ref neutral);
                builder.BuildThirdLevel(ref net, ref neutral);

                HSHoningBayes fixedBayes = new HSHoningBayes(hero.ToLower(), ref combosParser, ref net, ref cardDatasetFull, ref cardTable, 100000);

                fixedBayes.SaveKNNDataset(hero + "_data.dataset");

                Dataset fixedDataset = new Dataset(hero + "_data.dataset", ',');

                KNNEfficiency fixedKNN = new KNNEfficiency(fixedDataset);

                Dictionary<String, HoningNode<String>> dic = net.getNetwork();
                Dictionary<String, String> selfAbilityFilter = op.GenerateAbilityFilter();
                Dictionary<String, String> TerminalsDic = op.GetComboPotential(ref dic, ref cardTable, ref selfAbilityFilter, 123123);
                List<String> Terminals = TerminalsDic.Keys.ToList();

                double[] surprise_vec;
                Double[] comboArray;
                // Tests i and ii control variables

                // Tests i, ii, iii, iv, v control variables
                int mana = 10;
                double surprise;
                double efficiency;
                double fitness;
                double creativity;
                double normCreativity;
                double normSurprise;
                double normEfficiency;
                List<String> combo = new List<string>();
                double highSurp = 0.0;
                double minSurp = double.MaxValue;
                double highEff = 0.0;
                double minEff = double.MaxValue;
                // Tests i, ii, iii, iv, v control variables

                String seed = "";

                // Calibrating surprise
                Console.WriteLine("----------------------------------------------------------------------");
                Console.WriteLine("-                       Calibrating surprise!                        -");
                for (int i = 0; i < 100; i++)
                {
                    int totalMana = 0;
                    List<String> randomComboList = new List<String>();
                    int manaCost = 0;
                    while (totalMana < mana)
                    {
                        int randNode = rand.Next(Terminals.Count);
                        Int32.TryParse(cardTable[Terminals[randNode]].cost, out manaCost);
                        if (manaCost + totalMana <= mana)
                        {
                            randomComboList.Add(Terminals[randNode]);
                            totalMana += manaCost;
                        }
                    }

                    // Surprise
                    ComboNode node = ToComboNode(randomComboList);
                    fixedBayes.CalculateSurprise(ref node, 1, out surprise_vec, ref cardDatasetFull, out surprise, false);

                    // Calculate efficiency
                    fixedBayes.GenerateComboVector(ref node, ref cardDatasetFull, out comboArray);
                    Instance target = new Instance(comboArray);
                    efficiency = fixedKNN.getKNearestWinrates(target, k);

                    if (surprise > highSurp)
                        highSurp = surprise;
                    if (surprise < minSurp)
                        minSurp = surprise;

                    if (efficiency > highEff)
                        highEff = efficiency;
                    if (efficiency < minEff)
                        minEff = efficiency;
                }

                Console.WriteLine("-                       Surprise calibrated!                         -");

                foreach (String c in Terminals)
                {
                    Console.WriteLine("----------------------------------------------------------------------");

                    Console.WriteLine("Hero: " + hero);
                    Console.WriteLine("Seed: " + c);

                    Console.WriteLine();

                    file.WriteLine("----------------------------------------------------------------------");
                    file.WriteLine();
                    file.WriteLine("Hero: " + hero);
                    file.WriteLine("Seed: " + c);

                    // Test all reacheable seeds
                    seed = c;

                    //--------------------------------------------------------------------------------------------------------------------------
                    // (i)totalmente aleatorio (sem honing)
                    int totalMana = 0;
                    List<String> randomComboList = new List<String>();
                    randomComboList.Add(seed.ToLower());
                    int manaCost = 0;
                    Int32.TryParse(cardTable[seed.ToLower()].cost, out manaCost);
                    totalMana += manaCost;
                    while (totalMana < mana)
                    {
                        int randNode = rand.Next(Terminals.Count);
                        Int32.TryParse(cardTable[Terminals[randNode]].cost, out manaCost);
                        if (manaCost + totalMana <= mana)
                        {
                            randomComboList.Add(Terminals[randNode]);
                            totalMana += manaCost;
                        }
                    }

                    // Surprise
                    ComboNode node = ToComboNode(randomComboList);

                    fixedBayes.CalculateSurprise(ref node, 1, out surprise_vec, ref cardDatasetFull, out surprise, false);

                    // Calculate efficiency
                    fixedBayes.GenerateComboVector(ref node, ref cardDatasetFull, out comboArray);
                    Instance target = new Instance(comboArray);
                    efficiency = fixedKNN.getKNearestWinrates(target, k);

                    // Calculate creativity
                    fitness = op.CalculateFitness(surprise, ref highSurp, ref minSurp, out normSurprise, ref highEff, ref minEff, out normEfficiency, efficiency);

                    creativity = surprise + efficiency;
                    normCreativity = normSurprise + normEfficiency;

                    Console.WriteLine("Test I:\n");
                    Console.WriteLine("Fitness: " + fitness + "\nRaw creativity: " + creativity + "\nNormalized creativity: " + normCreativity + "\nSurprise " + surprise + "\nNormalized surprise: " + normSurprise + "\nEfficiency: " + efficiency + "\nNormalized efficiency: " + normEfficiency);
                    Console.WriteLine("Highest surprise: " + highSurp + "\nLowest surprise: " + minSurp);
                    Console.WriteLine("Highest efficiency: " + highEff + "\nLowest efficiency: " + minEff + "\n");
                    Console.WriteLine("Cards:\n");
                    file.WriteLine("Test I:\n");
                    file.WriteLine("Fitness: " + creativity + "\nRaw creativity: " + surprise + efficiency + "\nNormalized creativity: " + normEfficiency + normSurprise + "\nSurprise " + surprise + "\nNormalized surprise: " + normSurprise + "\nEfficiency: " + efficiency + "\nNormalized efficiency: " + normEfficiency);
                    file.WriteLine("Highest surprise: " + highSurp + "\nLowest surprise: " + minSurp);
                    file.WriteLine("Highest efficiency: " + highEff + "\nLowest efficiency: " + minEff + "\n");
                    file.WriteLine();
                    file.WriteLine("Cards: ");
                    file.WriteLine();
                    foreach (String st in randomComboList)
                    {
                        Console.Write("(" + st + ") ");
                        file.Write("(" + st + ") ");
                    }
                    Console.WriteLine("\n");
                    file.WriteLine("\n");

                    //--------------------------------------------------------------------------------------------------------------------------
                    // (ii)honing novo aleatorio
                    honingCombo = op.GenerateCardClusterRandom(
                        c,
                        ref cardTable,
                        ref net,
                        ref selfAbilityFilter,
                        ref fixedBayes,
                        ref fixedKNN,
                        ref cardDatasetFull,
                        mana,
                        k,
                        ref highSurp,
                        ref minSurp,
                        ref highEff,
                        ref minEff,
                        out fitness,
                        out surprise,
                        out efficiency,
                        out normSurprise,
                        out normEfficiency).Keys.ToList();

                    creativity = surprise + efficiency;
                    normCreativity = normSurprise + normEfficiency;

                    Console.WriteLine("Test II:\n");
                    Console.WriteLine("Fitness: " + fitness + "\nRaw creativity: " + creativity + "\nNormalized creativity: " + normCreativity + "\nSurprise " + surprise + "\nNormalized surprise: " + normSurprise + "\nEfficiency: " + efficiency + "\nNormalized efficiency: " + normEfficiency);
                    Console.WriteLine("Highest surprise: " + highSurp + "\nLowest surprise: " + minSurp);
                    Console.WriteLine("Highest efficiency: " + highEff + "\nLowest efficiency: " + minEff + "\n");
                    Console.WriteLine("Cards:\n");
                    file.WriteLine("Test I:\n");
                    file.WriteLine("Fitness: " + creativity + "\nRaw creativity: " + surprise + efficiency + "\nNormalized creativity: " + normEfficiency + normSurprise + "\nSurprise " + surprise + "\nNormalized surprise: " + normSurprise + "\nEfficiency: " + efficiency + "\nNormalized efficiency: " + normEfficiency);
                    file.WriteLine("Highest surprise: " + highSurp + "\nLowest surprise: " + minSurp);
                    file.WriteLine("Highest efficiency: " + highEff + "\nLowest efficiency: " + minEff + "\n");
                    file.WriteLine();
                    file.WriteLine("Cards: ");
                    file.WriteLine();
                    foreach (String st in honingCombo)
                    {
                        Console.Write("(" + st + ") ");
                        file.Write("(" + st + ") ");
                    }
                    Console.WriteLine("\n");
                    file.WriteLine("\n");

                    //--------------------------------------------------------------------------------------------------------------------------

                    
                    // (iii)honing novo (E+S)
                    honingCombo = op.GenerateCardCluster(
                        c,
                        ref cardTable,
                        ref net,
                        ref selfAbilityFilter,
                        ref fixedBayes,
                        ref fixedKNN,
                        ref cardDatasetFull,
                        mana,
                        k,
                        ref highSurp,
                        ref minSurp,
                        ref highEff,
                        ref minEff,
                        out fitness,
                        out surprise,
                        out efficiency,
                        out normSurprise,
                        out normEfficiency).Keys.ToList();

                    creativity = surprise + efficiency;
                    normCreativity = normSurprise + normEfficiency;

                    Console.WriteLine("Test III:\n");
                    Console.WriteLine("Fitness: " + fitness + "\nRaw creativity: " + creativity + "\nNormalized creativity: " + normCreativity + "\nSurprise " + surprise + "\nNormalized surprise: " + normSurprise + "\nEfficiency: " + efficiency + "\nNormalized efficiency: " + normEfficiency);
                    Console.WriteLine("Highest surprise: " + highSurp + "\nLowest surprise: " + minSurp);
                    Console.WriteLine("Highest efficiency: " + highEff + "\nLowest efficiency: " + minEff + "\n");
                    Console.WriteLine("Cards:\n");
                    file.WriteLine("Test I:\n");
                    file.WriteLine("Fitness: " + creativity + "\nRaw creativity: " + surprise + efficiency + "\nNormalized creativity: " + normEfficiency + normSurprise + "\nSurprise " + surprise + "\nNormalized surprise: " + normSurprise + "\nEfficiency: " + efficiency + "\nNormalized efficiency: " + normEfficiency);
                    file.WriteLine("Highest surprise: " + highSurp + "\nLowest surprise: " + minSurp);
                    file.WriteLine("Highest efficiency: " + highEff + "\nLowest efficiency: " + minEff + "\n");
                    file.WriteLine();
                    file.WriteLine("Cards: ");
                    file.WriteLine();
                    foreach (String st in honingCombo)
                    {
                        Console.Write("(" + st + ") ");
                        file.Write("(" + st + ") ");
                    }
                    Console.WriteLine("\n");
                    file.WriteLine("\n");

                }

                file.Close();
            }
        }

        void argTest(string[] args)
        {
            int value;
            Int32.TryParse(args[2], out value);

            Console.WriteLine(args[0]);
            Console.WriteLine(args[1]);
            Console.WriteLine(args[2]);

            ValidationTests(args[0], args[1], value);
        }

        // Apenas para testes
        static void Main(string[] args)
        {
            Program prog = new Program();

            if (args.Length < 3)
            {
                prog.ValidationTests("jsonWithTypes.json", "ALYCOMBOOK.json", 5);
            }
            else
            {
                prog.argTest(args);
            }
        }
    }
}
