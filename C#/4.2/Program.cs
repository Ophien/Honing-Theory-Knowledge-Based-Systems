/*
Copyright(c) 2015, Alysson Ribeiro da Silva All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met :

1. Redistributions of source code must retain the above copyright notice, this
list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
this list of conditions and the following disclaimer in the documentation
and / or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.
*/

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
                if(!node.combo.ContainsKey(s))
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

        void HoningTest(
            ref HSCardExpasionConfiguration config, 
            HSHoningBayes fixedBayes,
            KNNEfficiency fixedKNN,
            int mana, 
            int maxCards,
            HoningNetwork<String> net,
            Dictionary<String, CardObject> cardTable, 
            String seed, 
            HSCardOperator op,
            ExpansionGeneralPolitics pol, 
            out List<String> combo,
            out double creativity,
            out double efficiency, 
            out double surprise)
        {
            config.maxCards = maxCards;
            config.total_mana = mana;
            List<List<String>> out_subcluster;
            List<String> out_combo;
            op.GreedyExpansionDelegated(
                ref net,
                ref config,
                ref cardTable,
                pol,
                PriorityPolitics.Random,
                PriorityPolitics.Random,
                seed,
                out out_subcluster,
                out out_combo);

            

            // Context filter
            int voidCount = 0;
            for (int i = 0; i < out_combo.Count; i++)
                if (out_combo[i] == "")
                    voidCount++;

            while (voidCount > 0)
            {
                out_combo.Remove("");
                voidCount--;
            }

            // Surprise
            ComboNode comboNode = ToComboNode(out_combo);
            double[] surpriseVector;
            fixedBayes.CalculateSurprise(ref comboNode, 1, out surpriseVector, out surprise, true);

            if (surprise > config.highestSurprise)
                config.highestSurprise = surprise;

            // Calculate efficiency
            Double[] efficiencyVector;
            config.bayes.GenerateComboVector(ref comboNode, out efficiencyVector);
            Instance knnInstance = new Instance(efficiencyVector);
            efficiency = config.knn.getKNearestWinrates(knnInstance, 10);
            
            // Percent
            efficiency /= 100;

            // Calculate creativity
            creativity = ((surprise)+ efficiency);
            
            // Combo return
            combo = out_combo;

            double zero = 0.0;

            if (creativity == 1 / zero)
                creativity = 0.0000001;
        }

        void ValidationTests(String cardsJson, String combosFile, String knnDatase, int k){
            HSCardsParser parser = new HSCardsParser(cardsJson);
            HoningStoneBuilder builder = new HoningStoneBuilder();
            
            Dictionary<String, CardObject> cardTable = new Dictionary<string, CardObject>();
            HSCardOperator op = new HSCardOperator();

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

            foreach (String hero in parser.objects.Keys)
            {
                // To write results
                System.IO.StreamWriter file = new System.IO.StreamWriter(hero + "_results.dat");
                if (hero == "Neutral")
                    continue;

                List<String> honingCombo;
                List<String> highestComboI = new List<String>();
                List<String> highestComboII = new List<String>(); ;
                List<String> highestComboIII = new List<String>(); ;

                HoningNetwork<String> net = new HoningNetwork<string>();
                List<CardObject> heroCards = parser.objects[hero];
                List<CardObject> neutral = parser.objects["Neutral"];

                builder.PopulateFromCardData(ref net, ref heroCards);
                builder.BuildThirdLevel(ref net, ref heroCards);
                builder.PopulateFromCardData(ref net, ref neutral);
                builder.BuildThirdLevel(ref net, ref neutral);

                HSHoningBayes fixedBayes = new HSHoningBayes(hero.ToLower(), ref combosParser, ref net, ref cardTable);

                //fixedBayes.SaveKNNDataset("ARSDataset.dataset");

                Dataset fixedDataset = new Dataset(knnDatase, ',');
                KNNEfficiency fixedKNN = new KNNEfficiency(fixedDataset);

                HSHoningBayes dynamicBayes = new HSHoningBayes(hero.ToLower(), ref combosParser, ref net, ref cardTable);

                Dictionary<String, HoningNode<String>> dic = net.getNetwork();
                Dictionary<String, String> selfAbilityFilter = op.GenerateAbilityFilter();
                Dictionary<String, String> TerminalsDic = op.GetRealComboPotential(ref dic, ref cardTable, ref selfAbilityFilter, 3);
                List<String> Terminals = TerminalsDic.Keys.ToList();

                double[] surprise_vec;
                Double[] comboArray;
                // Tests i and ii control variables

                // Tests i, ii, iii, iv, v control variables
                int mana = 10;
                double surprise;
                double efficiency;
                double creativity;
                List<String> combo = new List<string>();
                // Tests i, ii, iii, iv, v control variables

                // Test I
                double highestCreativityA = 0.0;

                // Test II
                double highestCreativityB = 0.0;

                // Test III
                double highestCreativityC = 0.0;

                double highSurp = 0.0;

                double highestEfficience = 0.0;

                String seed = "";

                foreach (String c in Terminals)
                {
                    Console.WriteLine("Hero: " + hero);
                    Console.WriteLine("Seed: " + c);

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

                    fixedBayes.CalculateSurprise(ref node, 1, out surprise_vec, out surprise, false);

                    // Calculate efficiency
                    fixedBayes.GenerateComboVector(ref node, out comboArray);
                    Instance target = new Instance(comboArray);
                    efficiency = fixedKNN.getKNearestWinrates(target, k);
                    efficiency /= 100;

                    if (surprise > highSurp)
                        highSurp = surprise;

                    if (efficiency > highestEfficience)
                        highestEfficience = efficiency;

                    // Calculate creativity
                    creativity = op.CalculateCreativity(surprise / highSurp, efficiency);

                    // Write in file
                    bool update = false;
                    if (creativity > highestCreativityA)
                    {
                        highestCreativityA = creativity;
                        update = true;
                    }

                    Console.WriteLine("I: " + creativity + " " + surprise / highSurp + " " + efficiency);
                    file.WriteLine("I: " + creativity + " " + surprise + " " + efficiency);
                    foreach (String st in randomComboList)
                    {
                        Console.Write(st + "|");
                        file.Write(st + "|");
                        if (update)
                            highestComboI.Add(st);
                    }
                    Console.WriteLine();
                    file.WriteLine();

                    //--------------------------------------------------------------------------------------------------------------------------
                    // (ii)honing novo aleatorio
                    honingCombo = op.GenerateCardClusterRandom(c, ref cardTable, ref net, ref selfAbilityFilter, ref fixedBayes, ref fixedKNN, mana, k,
                        highSurp,
                        out creativity,
                        out surprise,
                        out efficiency).Keys.ToList();

                    update = false;
                    if (creativity > highestCreativityB)
                    {
                        highestCreativityB = creativity;
                        update = true;
                    }

                    if (efficiency > highestEfficience)
                        highestEfficience = efficiency;

                    file.WriteLine("II: " + creativity + " " + surprise + " " + efficiency);
                    Console.WriteLine("II: " + creativity + " " + surprise + " " + efficiency);
                    foreach (String st in honingCombo)
                    {
                        Console.Write(st + "|");
                        file.Write(st + "|");
                        if (update)
                            highestComboI.Add(st);
                    }
                    file.WriteLine();
                    Console.WriteLine();
                    //--------------------------------------------------------------------------------------------------------------------------
                    // (iii)honing novo (E+S)
                    honingCombo = op.GenerateCardCluster(c, ref cardTable, ref net, ref selfAbilityFilter, ref fixedBayes, ref fixedKNN, mana, k,
                          ref highSurp,
                          out creativity,
                          out surprise,
                          out efficiency).Keys.ToList();

                    update = false;
                    if (creativity > highestCreativityC)
                    {
                        highestCreativityC = creativity;
                        update = true;
                    }

                    if (efficiency > highestEfficience)
                        highestEfficience = efficiency;

                    file.WriteLine("III: " + creativity + " " + surprise + " " + efficiency);
                    Console.WriteLine("III: " + creativity + " " + surprise + " " + efficiency);
                    foreach (String st in honingCombo)
                    {
                        Console.Write(st + "|");
                        file.Write(st + "|");
                        if (update)
                            highestComboI.Add(st);
                    }
                    file.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("----------------------------------------------------------------");
                }

                file.Close();
            }
        }

        void argTest(string[] args)
        {
            int value;
            Int32.TryParse(args[3], out value);

            Console.WriteLine(args[0]);
            Console.WriteLine(args[1]);
            Console.WriteLine(args[2]);
            Console.WriteLine(args[3]);

            ValidationTests(args[0], args[1], args[2], value);
        }

        // Apenas para testes
        static void Main(string[] args)
        {
            Program prog = new Program();
            
            if (args.Length < 4)
            {
                prog.ValidationTests("jsonWithTypes.json", "ALYCOMBOOK.json", "ARSDataset.dataset", 5);
            }
            else
            {
                prog.argTest(args);
            }
        }
    }
}
