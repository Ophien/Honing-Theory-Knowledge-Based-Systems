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
using Newtonsoft.Json.Linq;

namespace SharpHoning
{
    class ComboNode
    {
        public ComboNode()
        {
            combo = new Dictionary<string, string>();
            winrates = new List<float>();
            winrate_mean = 0.0f;
            winrate_max = 0.0f;
            winrate_min = 0.0f;
            winrate_median = 0.0f;
        }

        public Dictionary<String, String> combo;
        public List<float> winrates;
        public float winrate_mean;
        public float winrate_max;
        public float winrate_min;
        public float winrate_median;
    }

    class HSCombosParser
    {
        public HSCombosParser()
        { 
        }

        public void CalculateWinrates()
        {
            foreach (String key in combos_by_quantity.Keys)
            {
                foreach (int comboQuant in combos_by_quantity[key].Keys)
                {
                    for (int i = 0; i < combos_by_quantity[key][comboQuant].Count; i++ )
                    {
                        ComboNode node = combos_by_quantity[key][comboQuant][i];
                        node.winrates.Sort();
                        CalculeteStatistics(ref node);
                    }
                }
            }
        }

        public void CalculeteStatistics(ref ComboNode node)
        {
            float winRateMin = node.winrates.First();
            float winRateMax = node.winrates.First();
            
            foreach (float winrate in node.winrates)
            {
                node.winrate_mean += winrate;
                if (winrate >= winRateMax)
                    winRateMax = winrate;
                if (winrate <= winRateMin)
                    winRateMin = winrate;
            }

            node.winrate_mean /= node.winrates.Count;
            node.winrate_max = winRateMax;
            node.winrate_min = winRateMin;
            node.winrate_median = node.winrates[(int)(node.winrates.Count / 2)];
        }

        public ComboNode FindCombo(ref Dictionary<String, String> combo, String hero)
        {
            ComboNode combonode = new ComboNode();

            if (!combos_by_quantity.ContainsKey(hero))
                return null;

            int comboSize = combo.Count;
            Dictionary<int, List<ComboNode>> comboDic = combos_by_quantity[hero];
            if (!comboDic.ContainsKey(comboSize))
                return null;

            List<ComboNode> seekingObject = comboDic[comboSize];
            foreach (ComboNode checker in seekingObject)
            {
                if (CheckComboAND(ref combo, ref checker.combo))
                {
                    combonode = checker;
                    break;
                }
            }

            if (combonode.combo.Count == 0)
                return null;

            return combonode;
        }

        public bool CheckComboAND(ref Dictionary<String, String> comboA, ref Dictionary<String, String> comboB)
        {
            foreach (String card in comboA.Keys)
                if (!comboB.ContainsKey(card))
                    return false;

            return true;
        }

        public void PopulateFromHoningNetwork(ref HSDecksParser decks, ref Dictionary<String, CardObject> cardTable, int mana_mult)
        {
            combos_per_card = 10;
            combos_by_quantity = new Dictionary<string, Dictionary<int, List<ComboNode>>>();

            // Hero loop
            foreach (String hero in decks.decks_by_hero.Keys)
            {
                // Decks loop
                foreach (HSDeckInfo deckinfo in decks.decks_by_hero[hero])
                {
                    // Generate honingNet for the deck
                    HoningNetwork<String> deck_net = new HoningNetwork<string>();
                    List<CardObject> cards_objects = new List<CardObject>();
                    HoningStoneBuilder builder = new HoningStoneBuilder();

                    // Generate card table info to populate the honingNetwork
                    foreach (String card in deckinfo.cards)
                    {
                        if (cardTable.ContainsKey(card))
                            cards_objects.Add(cardTable[card]);
                    }

                    builder.PopulateFromCardData(ref deck_net, ref cards_objects);
                    builder.BuildThirdLevel(ref deck_net, ref cards_objects);

                    // Generate all combos
                    HSCardOperator op = new HSCardOperator();
                    HSCardExpasionConfiguration conf = new HSCardExpasionConfiguration(null,null);

                    for (int j = 1; j < mana_mult; j++)
                    {
                        // Mana to spend on combo
                        conf.total_mana = 10 * j;

                        // Same deck insertion flag
                        bool same_deck = false;
                        foreach (String card in deckinfo.cards)
                        {
                            for (int i = 0; i < combos_per_card; i++)
                            {
                                List<List<String>> combo;
                                List<String> sCombo;
                                op.GreedyExpansionDelegated(
                                    ref deck_net,
                                    ref conf,
                                    ref cardTable,
                                    ExpansionGeneralPolitics.Random,
                                    PriorityPolitics.Random,
                                    PriorityPolitics.Random,
                                    card,
                                    out combo,
                                    out sCombo);

                                // Create new combo object(Node) or get an existing one
                                Dictionary<String, String> comboToFind = new Dictionary<string, string>();
                                foreach (String c in sCombo)
                                    comboToFind.Add(c, c);

                                if (comboToFind.Count == 0)
                                    continue;

                                ComboNode new_combo = FindCombo(ref comboToFind, hero);

                                if (new_combo == null)
                                {
                                    new_combo = new ComboNode();
                                    if (!combos_by_quantity.ContainsKey(hero))
                                        combos_by_quantity.Add(hero, new Dictionary<int, List<ComboNode>>());
                                    if (!combos_by_quantity[hero].ContainsKey(comboToFind.Count))
                                        combos_by_quantity[hero].Add(comboToFind.Count, new List<ComboNode>());


                                    // Configura winrate for this combo
                                    float comboWinrate;
                                    float.TryParse(deckinfo.winrate, out comboWinrate);
                                    new_combo.winrates.Add(comboWinrate);
                                    new_combo.combo = comboToFind;

                                    // Add the combo to the master holder
                                    combos_by_quantity[hero][comboToFind.Count].Add(new_combo);
                                }
                                else
                                {
                                    if (!same_deck && new_combo.combo.Count != 0)
                                    {
                                        float comboWinrate;
                                        float.TryParse(deckinfo.winrate, out comboWinrate);
                                        new_combo.winrates.Add(comboWinrate);
                                    }
                                }

                                same_deck = true;
                            }
                        } // end foreach card
                    } // end for manda
                } // end foreach deck
            } // end foreach hero
        }

        public void PopulateFromJson(String path)
        {
            combos_by_quantity = new Dictionary<string, Dictionary<int, List<ComboNode>>>();

            // Le arquivo json e retorna string inteira da rede
            String json = System.IO.File.ReadAllText(path);

            // Ler json com dados da rede
            dynamic stuff = JArray.Parse(json);

            foreach(dynamic hero in stuff){
                String currentHero = hero.hero;
                foreach (dynamic combo in hero.combos)
                {
                    ComboNode node    = new ComboNode();
                    node.winrate_max  = combo.winrate_max;
                    node.winrate_min  = combo.winrate_min;
                    node.winrate_mean = combo.winrate_mean;
                    node.winrate_median = combo.winrate_median;

                    foreach (float w in combo.winrates)
                        node.winrates.Add(w);

                    foreach (String card in combo.cards)
                        node.combo.Add(card, card);

                    if (!combos_by_quantity.ContainsKey(currentHero))
                        combos_by_quantity.Add(currentHero, new Dictionary<int, List<ComboNode>>());

                    Dictionary<int, List<ComboNode>> dic = combos_by_quantity[currentHero];

                    if (!dic.ContainsKey(node.combo.Count))
                        dic.Add(node.combo.Count, new List<ComboNode>());

                    List<ComboNode> nodes = dic[node.combo.Count];

                    nodes.Add(node);
                }
            }
        }

        public void ToJson(String path)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(path);

            //File start vector
            file.WriteLine("[");

            int c = 0;
            foreach (String key in combos_by_quantity.Keys)
            {
                //hero start
                file.WriteLine("{");
                file.WriteLine("\"hero\":\"" + key + "\",");
                // Combos start
                file.WriteLine("\"combos\":[");

                int b = 0;
                foreach (int comboQuant in combos_by_quantity[key].Keys)
                {
                    
                    for (int i = 0; i < combos_by_quantity[key][comboQuant].Count; i++)
                    {
                        string lines = "";
                        ComboNode node = combos_by_quantity[key][comboQuant][i];

                        // combo open
                        file.Write("{");

                        lines += "\"winrate_max\":\"" + node.winrate_max +"\",";
                        lines += "\"winrate_min\":\"" + node.winrate_min + "\",";
                        lines += "\"winrate_mean\":\"" + node.winrate_mean + "\",";
                        lines += "\"winrate_median\":\"" + node.winrate_median + "\",";
                        lines += "\"winrates\":[";
                        for (int j = 0; j < node.winrates.Count; j++)
                        {
                            lines += "\"" + node.winrates[j] + "\"";

                            if (j != node.winrates.Count - 1)
                                lines += ",";
                        }
                        lines += "],";
                        lines += "\"cards\":[";
                        for (int j = 0; j < node.combo.Count; j++)
                        {
                            lines += "\"" + node.combo.ElementAt(j).Value + "\"";

                            if (j != node.combo.Count - 1)
                                lines += ",";
                        }
                        lines += "]";

                        // combo internal
                        file.WriteLine(lines);

                        // combo close
                        file.WriteLine("}");
                       

                        if(b != combos_by_quantity[key].Count - 1)
                            file.WriteLine(",");
                      //      if (i != combos_by_quantity[key][comboQuant].Count - 1)
                               
                    }

                    b++;
                }

                //combos end
                file.WriteLine("]");

                //end hero
                file.WriteLine("}");

                if (c != combos_by_quantity.Count - 1)
                    file.WriteLine(",");

                c++;
            }

            //File end vector
            file.WriteLine("]");

            file.Close();
        }

        public int combos_per_card;
        public Dictionary<String, Dictionary<int, List<ComboNode>>> combos_by_quantity;
    }
}
