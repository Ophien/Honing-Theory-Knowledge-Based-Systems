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
            HSCardExpasionConfiguration config, 
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

            // Calculate efficiency
            Double[] efficiencyVector;
            config.bayes.GenerateComboVector(ref comboNode, out efficiencyVector);
            Instance knnInstance = new Instance(efficiencyVector);
            efficiency = config.knn.getKNearestWinrates(knnInstance, 5);

            // Percent
            efficiency /= 100;

            // Calculate creativity
            creativity = ((surprise / config.highestSurprise) + efficiency) / 2;

            // Combo return
            combo = out_combo;
        }

        void ValidationTests(){
            HSCardsParser parser = new HSCardsParser("jsonWithTypes.json");
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
            combosParser.PopulateFromJson("ALYCOMBOOK.json");

            int maxComboSize    = 10;
            //int maxMana         = 50;
            int maxCombos       = 100;

            Random rand = new Random();

            foreach (String hero in parser.objects.Keys)
            {
                if (hero == "Neutral")
                    continue;

                HoningNetwork<String> net       = new HoningNetwork<string>();
                List<CardObject> heroCards      = parser.objects[hero];
                List<CardObject> neutral        = parser.objects["Neutral"];
                
                builder.PopulateFromCardData(ref net, ref heroCards);
                builder.BuildThirdLevel(ref net, ref heroCards);
                builder.PopulateFromCardData(ref net, ref neutral);
                builder.BuildThirdLevel(ref net, ref neutral);

                HSHoningBayes fixedBayes   = new HSHoningBayes(hero.ToLower(), ref combosParser, ref net, ref cardTable);

                fixedBayes.SaveKNNDataset("ARSDataset.dataset");

                Dataset fixedDataset = new Dataset("ARSDataset.dataset", ',');
                KNNEfficiency fixedKNN = new KNNEfficiency(fixedDataset);

                HSHoningBayes dynamicBayes = new HSHoningBayes(hero.ToLower(), ref combosParser, ref net, ref cardTable);

                List<String> Terminals = net.getTerminalList();

                // Random Honing config
                HSCardExpasionConfiguration config = new HSCardExpasionConfiguration(fixedBayes, fixedKNN);
                config.cutByManaCost = false;
                config.max_lowerlevel_to_expand = 1;
                config.giver_inflation = false;

                // Guided Honing config
                HSCardExpasionConfiguration configGuided = new HSCardExpasionConfiguration(fixedBayes, fixedKNN);
                configGuided.cutByManaCost = false;
                configGuided.max_lowerlevel_to_expand = 1;
                configGuided.giver_inflation = false;

                // Tests i and ii control variables
                double ihs = 0.0f;
                double iihs = 0.0f;

                double[] surprise_vec;
                Double[] comboArray;
                // Tests i and ii control variables

                // Tests i, ii, iii, iv, v control variables
                int mana = 50;
                double surprise;
                double efficiency;
                double creativity;
                List<String> combo;
                // Tests i, ii, iii, iv, v control variables

                int combosize = 5;

                for (int combos = 0; combos < maxCombos; combos++)
                {
                    //for (int mana = 2; mana <= maxMana; mana++)
                    //{
                        //for (int combosize = 2; combosize <= maxComboSize; combosize++)
                       // {
                            // Honing shared seed
                            int RandomicSeed = rand.Next(Terminals.Count);
                            String seed = Terminals[RandomicSeed];

                            //--------------------------------------------------------------------------------------------------------------------------
                            // Surpresa estática
                            // (i)totalmente aleatorio (sem honing)
                           /* List<String> randomComboList = new List<String>();
                            while(randomComboList.Count < combosize)
                            {
                                int randNode = rand.Next(Terminals.Count);
                                randomComboList.Add(Terminals[randNode]);
                            }

                            // Surprise
                            ComboNode node = ToComboNode(randomComboList);

                            fixedBayes.CalculateSurprise(ref node, 1, out surprise_vec, out surprise, false);

                            // update surprise
                            if (surprise > ihs)
                                ihs = surprise;

                            // Calculate efficiency
                            fixedBayes.GenerateComboVector(ref node, out comboArray);
                            Instance target = new Instance(comboArray);
                            efficiency = fixedKNN.getKNearestWinrates(target, 5);
                            efficiency /= 100;

                            // Calculate creativity
                            creativity = ((surprise / ihs) + efficiency) / 2;

                            //--------------------------------------------------------------------------------------------------------------------------

                            // (ii)honing velho aleatorio
                            Dictionary<String, String> shcombolist = new Dictionary<String, String>();
                            Dictionary<String, HoningNode<String>> honingOut;
                            List<String> bridges;
                            net.getMfList(seed, out bridges);
                            net.recruitNeurds(bridges, out honingOut, "terminal");

                            List<String> comboH = new List<string>();

                            int limit = combosize;
                            if(honingOut.Count < combosize)
                                limit = honingOut.Count;
                            for (int i = 0; i < limit; i++)
                                comboH.Add(honingOut.ElementAt(i).Key);

                            // Surprise
                            node = ToComboNode(comboH);

                            fixedBayes.CalculateSurprise(ref node, 1, out surprise_vec, out surprise, false);

                            // update surprise
                            if (surprise > iihs)
                                iihs = surprise;

                            // Calculate efficiency
                            fixedBayes.GenerateComboVector(ref node, out comboArray);
                            target = new Instance(comboArray);
                            efficiency = fixedKNN.getKNearestWinrates(target, 5);
                            efficiency /= 100;

                            // Calculate creativity
                            creativity = ((surprise / iihs) + efficiency) / 2;

                            //--------------------------------------------------------------------------------------------------------------------------

                            // (iii)honing novo aleatorio
                            HoningTest(
                                config, 
                                fixedBayes,
                                fixedKNN, 
                                mana, 
                                combosize, 
                                net,
                                cardTable, 
                                seed, 
                                op,
                                ExpansionGeneralPolitics.Random, 
                                out combo,
                                out creativity, 
                                out efficiency, 
                                out surprise);*/

                            //--------------------------------------------------------------------------------------------------------------------------

                            // (iv)busca pela aresta com maior valor eficiência + surpresa (GULOSO)
                            HoningTest(
                                configGuided,
                                fixedBayes, 
                                fixedKNN,
                                mana, 
                                combosize, 
                                net,
                                cardTable,
                                seed,
                                op, 
                                ExpansionGeneralPolitics.Weight, 
                                out combo,
                                out creativity,
                                out efficiency,
                                out surprise);

                            Console.WriteLine("Cluster honing finished runing.");
                            //--------------------------------------------------------------------------------------------------------------------------


                            //--------------------------------------------------------------------------------------------------------------------------

                            // Surpresa dinâmica
                            // (i)totalmente aleatorio (sem honing)
                            // (ii)honing velho aleatorio
                            // (iii)honing novo aleatorio
                            // (iv)busca pela aresta com maior valor eficiência + surpresa
                        //}
                    }
                //}
            }
        }

        // Apenas para testes
        static void Main(string[] args)
        {
            Program prog = new Program();
            prog.ValidationTests();
        }
    }
}
