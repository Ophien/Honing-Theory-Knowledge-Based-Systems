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
    enum ExpansionGeneralPolitics
    {
        Lowest_Mana,
        Higher_Mana,
        Random,
        Quantity,
        Weight
    };

    enum PriorityPolitics
    {
        Random,
        First,
        Last,
        HigherWeight,
        LowestWeight,
        MidWeight
    };

    class HSCardCluster
    {
        public HSCardCluster()
        {
            giver = new Dictionary<String, Dictionary<String, HoningNode<String>>>();
            receiver = new Dictionary<String, Dictionary<String, HoningNode<String>>>();
            card = "";
        }

        public String card;
        public Dictionary<String, Dictionary<String, HoningNode<String>>> giver;
        public Dictionary<String, Dictionary<String, HoningNode<String>>> receiver;
    }

    class HSCardExpasionConfiguration
    {
        public HSCardExpasionConfiguration(HSHoningBayes bayes, KNNEfficiency knn)
        {
            this.bayes = bayes;
            this.knn = knn;

            total_mana = 35;
            max_lowerlevel_to_expand = -1;
            single_expasion_quant = 1;
            max_upperLevel_to_expand = 1;
            single_expasion_quant_threshold = 0.9f;

            upperLevel_to_expand = new List<string>();
            lowerLevel_to_expand = new List<string>();

            upperPriorityVector = new float[1];
            lowerPriorityVector = new float[1];

            giver_inflation = true;
            receiver_inflation = true;
            cutByManaCost = true;

            highestSurprise = 0.0;
            maxCards = 0;
        }

        public int maxCards;
        public double highestSurprise;

        public HSHoningBayes bayes;
        public KNNEfficiency knn;

        public float single_expasion_quant_threshold;
        public int total_mana;
        public int single_expasion_quant;
        public int max_lowerlevel_to_expand;
        public int max_upperLevel_to_expand;

        public bool cutByManaCost;
        public bool giver_inflation;
        public bool receiver_inflation;

        public List<String> upperLevel_to_expand;
        public List<String> lowerLevel_to_expand;

        public float[] upperPriorityVector;
        public float[] lowerPriorityVector;
    }

    class HSCardOperator
    {
        public List<String> InflateNeighbours(
            ref Dictionary<String, HoningNode<String>> workspace,
             Dictionary<String, CardObject> cardTable,
            ref HSCardExpasionConfiguration config,
            ref ExpansionGeneralPolitics politics,
            ref Dictionary<String, Boolean> markers,
            ref List<String> combo)
        {
            List<String> res = new List<string>();

            // Inflation window
            int windowSize = 200;
            int displace = 0;
            Random dRange = new Random();

            if (workspace.Count > windowSize)
            {
                displace = dRange.Next(workspace.Count - windowSize);

                for (int i = displace; i < displace + windowSize; i++)
                {
                    res.Add(workspace.ElementAt(i).Value.holder);
                }
            }
            else
            {
                foreach (String k in workspace.Keys)
                {
                    if (!markers.ContainsKey(k))
                        res.Add(workspace[k].holder);
                }
            }

            if (res.Count == 0)
                return res;

            List<String> final_ret = new List<string>();
            Boolean[] check = new Boolean[res.Count];
            Boolean allCheckded = false;
            int cur = 0;

            switch (politics)
            {
                case ExpansionGeneralPolitics.Lowest_Mana:
                    try
                    {
                        res.Sort(
                               delegate(String a, String b)
                               {
                                   if (a == null || b == null)
                                       return -1;
                                   int Ia, Ib;
                                   Int32.TryParse(cardTable[a].cost, out Ia);
                                   Int32.TryParse(cardTable[b].cost, out Ib);
                                   if (Ia > Ib)
                                       return 0;
                                   return -1;
                               }
                        );
                    }
                    catch (ArgumentException)
                    {
                    }

                    cur = 0;
                    while (final_ret.Count < config.single_expasion_quant)
                    {
                        if (allCheckded)
                            break;

                        check[cur] = true;
                        if (!markers.ContainsKey(res[cur]))
                        {
                            markers.Add(res[cur], true);
                            final_ret.Add(res[cur]);
                        }

                        cur++;

                        allCheckded = true;
                        for (int i = 0; i < res.Count; i++)
                        {
                            if (check[i] == false)
                            {
                                allCheckded = false;
                                break;
                            }
                        }
                    }

                    break;
                case ExpansionGeneralPolitics.Higher_Mana:
                    try
                    {
                        res.Sort(
                                delegate(String a, String b)
                                {
                                    if (a == null || b == null)
                                        return -1;
                                    int Ia, Ib;
                                    Int32.TryParse(cardTable[a].cost, out Ia);
                                    Int32.TryParse(cardTable[a].cost, out Ib);
                                    if (Ia < Ib)
                                        return 0;
                                    return -1;
                                }
                        );
                    }
                    catch (ArgumentException)
                    {
                    }

                    cur = 0;
                    while (final_ret.Count < config.single_expasion_quant)
                    {
                        if (allCheckded)
                            break;

                        check[cur] = true;
                        if (!markers.ContainsKey(res[cur]))
                        {
                            markers.Add(res[cur], true);
                            final_ret.Add(res[cur]);
                        }

                        cur++;

                        allCheckded = true;
                        for (int i = 0; i < res.Count; i++)
                        {
                            if (check[i] == false)
                            {
                                allCheckded = false;
                                break;
                            }
                        }
                    }
                    break;
                case ExpansionGeneralPolitics.Random:
                    // Roullet selection

                    Random rand = new Random();
                    int total = config.single_expasion_quant;
                    cur = 0;

                    for (int i = 0; i < res.Count; i++)
                        check[i] = false;

                    while (total > 0)
                    {
                        if (allCheckded)
                            break;

                        if (cur >= res.Count)
                            cur = 0;

                        float t = (float)rand.NextDouble();
                        if (t >= config.single_expasion_quant_threshold)
                        {
                            check[cur] = true;
                            if (!markers.ContainsKey(res[cur]))
                            {
                                markers.Add(res[cur], true);
                                final_ret.Add(res[cur]);
                                total--;
                            }
                        }
                        cur++;

                        allCheckded = true;
                        for (int i = 0; i < res.Count; i++)
                        {
                            if (check[i] == false)
                            {
                                allCheckded = false;
                                break;
                            }
                        }
                    }
                    break;
                case ExpansionGeneralPolitics.Weight:
                    // Do nothing
                    // Calculate edge weights from efficiency and surprise
                    ComboNode node = new ComboNode();

                    foreach (String card in combo)
                    {
                        node.combo.Add(card, card);
                    }

                    total = config.single_expasion_quant;

                    List<KeyValuePair<string, double>> card_creativity_list = new List<KeyValuePair<string, double>>();
                    String hs = "";
                    double localHighSurprise = 0.0;
                    foreach (String s in res)
                    {
                        //hs = s;

                        if (node.combo.ContainsKey(s))
                            continue;

                        node.combo.Add(s, s);

                        // Calculate efficiency
                        Double[] comboArray;
                        config.bayes.GenerateComboVector(ref node, out comboArray);
                        Instance target = new Instance(comboArray);
                        Double newWinrate = config.knn.getKNearestWinrates(target, 10);
                        newWinrate /= 100;

                        // Calculate surprise
                        double[] surpriseVec;
                        double surprise;
                        config.bayes.CalculateSurprise(ref node, 1, out surpriseVec, out surprise, false);

                        // Update globalSurprise
                        if (surprise >= config.highestSurprise)
                        {
                            config.highestSurprise = surprise;
                        }

                        // Calculate criativity
                        double creativityFactor = ((surprise) + newWinrate);

                        if (creativityFactor >= localHighSurprise)
                        {
                            localHighSurprise = creativityFactor;
                            hs = s;
                        }

                        card_creativity_list.Add(new KeyValuePair<string, double>(s, creativityFactor));


                        node.combo.Remove(s);
                    }

                    // Sort creativity combo
                    /*card_creativity_list.Sort(delegate(KeyValuePair<string, double> A, KeyValuePair<string, double> B)
                    {
                        if (A.Value < B.Value)
                            return 0;
                        return -1;
                    });*/

                    if (config.single_expasion_quant == 1)
                    {
                        if (!markers.ContainsKey(hs))
                            final_ret.Add(hs);
                    }
                    else
                    {
                        cur = 0;
                        while (final_ret.Count < config.single_expasion_quant)
                        {
                            if (allCheckded)
                                break;

                            check[cur] = true;
                            if (!markers.ContainsKey(card_creativity_list[cur].Key))
                            {
                                markers.Add(card_creativity_list[cur].Key, true);
                                final_ret.Add(card_creativity_list[cur].Key);
                            }

                            cur++;

                            allCheckded = true;
                            for (int i = 0; i < res.Count; i++)
                            {
                                if (check[i] == false)
                                {
                                    allCheckded = false;
                                    break;
                                }
                            }
                        }
                    }
                    break;
            }

            return final_ret;
        }

        public List<String> RecruitNeighbours(
            ref HoningNetwork<String> net,
            ref Dictionary<String, Dictionary<String, HoningNode<String>>> subcluster,
            ref HSCardExpasionConfiguration config,
            ref Dictionary<String, CardObject> cardTable,
            ref ExpansionGeneralPolitics politic,
            ref PriorityPolitics selectionPriorities,
            ref Dictionary<String, Boolean> markers,
            ref List<String> combo)
        {
            List<String> res = new List<string>();

            if (subcluster.Count == 0)
                return res;

            // Configurating expansion levels
            int upper_level_index = 0;
            if (config.max_upperLevel_to_expand < 0)
            {
                upper_level_index = -1;
            }
            else
            {
                Random rand = new Random();

                switch (selectionPriorities)
                {
                    case PriorityPolitics.Random:
                        upper_level_index = rand.Next(subcluster.Count - 1);
                        break;
                    case PriorityPolitics.First:
                        upper_level_index = 0;
                        break;
                    case PriorityPolitics.Last:
                        upper_level_index = subcluster.Count - 1;
                        break;
                    case PriorityPolitics.HigherWeight:
                        // NOT YET
                        break;
                    case PriorityPolitics.LowestWeight:
                        // NOT YET
                        break;
                    case PriorityPolitics.MidWeight:
                        // NOT YET
                        break;
                }
            }

            Dictionary<String, Dictionary<String, HoningNode<String>>> workingspace = new Dictionary<string, Dictionary<string, HoningNode<string>>>();

            if (upper_level_index < 0)
            {
                workingspace = subcluster;
            }
            else
            {
                workingspace.Add(subcluster.ElementAt(upper_level_index).Key, subcluster.ElementAt(upper_level_index).Value);
            }

            // Inflating solution
            foreach (String key in workingspace.Keys)
            {
                Dictionary<String, HoningNode<String>> dic = workingspace[key];
                List<String> space = InflateNeighbours(ref dic, cardTable, ref config, ref politic, ref markers, ref combo);
                res.AddRange(space);
            }

            return res;
        }

        public void GreedyExpansionDelegated(
            ref HoningNetwork<String> net,
            ref HSCardExpasionConfiguration config,
            ref Dictionary<String, CardObject> cardTable,
            ExpansionGeneralPolitics politic,
            PriorityPolitics giverPriorities,
            PriorityPolitics receiverPriorities,
            String seed,
            out List<List<String>> out_subcomboClusters,
            out List<String> out_simpleComboList)
        {
            HSCardCluster base_cluster = GenerateCardCluster(seed, ref cardTable, ref net);

            out_subcomboClusters = new List<List<string>>();
            out_simpleComboList = new List<string>();

            if (base_cluster == null)
                return;

            if (config.total_mana == 0)
                return;

            List<List<String>> final_form = new List<List<string>>();

            List<String> comboNodes = new List<String>();

            List<String> combo = new List<String>();

            combo.Add(base_cluster.card);

            Dictionary<String, bool> markers = new Dictionary<string, Boolean>();

            List<String> giver_recruiting;
            List<String> receiver_recruiting;

            if (config.giver_inflation)
            {
                giver_recruiting = RecruitNeighbours(ref net, ref base_cluster.giver, ref config, ref cardTable, ref politic, ref giverPriorities, ref markers, ref combo);
                comboNodes.AddRange(giver_recruiting);
                combo.AddRange(giver_recruiting);
            }

            if (config.receiver_inflation)
            {
                receiver_recruiting = RecruitNeighbours(ref net, ref base_cluster.receiver, ref config, ref cardTable, ref politic, ref receiverPriorities, ref markers, ref combo);
                comboNodes.AddRange(receiver_recruiting);
                combo.AddRange(receiver_recruiting);
            }

            final_form.Add(combo);

            // Context filter
            int voidCount = 0;
            for (int i = 0; i < comboNodes.Count; i++)
                if (comboNodes[i] == "")
                    voidCount++;

            while (voidCount > 0)
            {
                comboNodes.Remove("");
                voidCount--;
            }

            while (comboNodes.Count > 0)
            {
                String current = comboNodes.First();
                comboNodes.Remove(current);
                HSCardCluster new_cluster = GenerateCardCluster(current, ref cardTable, ref net);

                combo = new List<String>();
                combo.Add(new_cluster.card);

                if (!markers.ContainsKey(current))
                    markers.Add(current, true);

                if (config.giver_inflation)
                {
                    giver_recruiting = RecruitNeighbours(ref net, ref new_cluster.giver, ref config, ref cardTable, ref politic, ref giverPriorities, ref markers, ref combo);
                    comboNodes.AddRange(giver_recruiting);
                    combo.AddRange(giver_recruiting);
                }

                if (config.receiver_inflation)
                {
                    receiver_recruiting = RecruitNeighbours(ref net, ref new_cluster.receiver, ref config, ref cardTable, ref politic, ref receiverPriorities, ref markers, ref combo);
                    comboNodes.AddRange(receiver_recruiting);
                    combo.AddRange(receiver_recruiting);
                }

                final_form.Add(combo);

                if (final_form.Count == config.maxCards)
                    break;
            }

            // Mana threshold 10 mana = 1 turn, 30 mana = 3 turns combo
            if (config.cutByManaCost)
            {
                Dictionary<String, String> keys = new Dictionary<string, string>();
                int total_mana = 0;
                bool breaked = false;
                foreach (List<String> key in final_form)
                {
                    foreach (String k in key)
                    {
                        String s = k.ToLower();

                        if (keys.ContainsKey(k))
                            continue;

                        int mana_cost;
                        Int32.TryParse(cardTable[s].cost, out mana_cost);
                        total_mana += mana_cost;

                        if (total_mana >= config.total_mana)
                        {
                            breaked = true;
                            break;
                        }

                        keys.Add(k, k);

                        if (keys.Count == config.maxCards)
                        {
                            breaked = true;
                            break;
                        }
                    }

                    if (breaked)
                        break;
                }
                out_simpleComboList = keys.Keys.ToList();
            }
            else
            {
                bool breaked = false;
                foreach (List<String> key in final_form)
                {
                    foreach (String k in key)
                    {
                        out_simpleComboList.Add(k);
                        if (out_simpleComboList.Count == config.maxCards)
                        {
                            breaked = true;
                            break;
                        }
                    }
                    if (breaked)
                        break;
                }
            }

            out_subcomboClusters = final_form;
        }

        public List<String> GreedyExpansion(
            ref HoningNetwork<String> net,
            ref HSCardExpasionConfiguration config,
            ref Dictionary<String, CardObject> cardTable,
            ExpansionGeneralPolitics politic,
            PriorityPolitics giverPriorities,
            PriorityPolitics receiverPriorities,
            String seed)
        {
            HSCardCluster base_cluster = GenerateCardCluster(seed, ref cardTable, ref net);

            if (base_cluster == null)
                return null;

            if (config.total_mana == 0)
                return null;

            List<String> comboNodes = new List<String>();

            List<String> combo = new List<String>();

            combo.Add(base_cluster.card);

            Dictionary<String, bool> markers = new Dictionary<string, Boolean>();

            List<String> giver_recruiting;
            List<String> receiver_recruiting;

            if (config.giver_inflation)
            {
                giver_recruiting = RecruitNeighbours(ref net, ref base_cluster.giver, ref config, ref cardTable, ref politic, ref giverPriorities, ref markers, ref combo);
                comboNodes.AddRange(giver_recruiting);
            }

            if (config.receiver_inflation)
            {
                receiver_recruiting = RecruitNeighbours(ref net, ref base_cluster.receiver, ref config, ref cardTable, ref politic, ref receiverPriorities, ref markers, ref combo);
                comboNodes.AddRange(receiver_recruiting);
            }

            while (comboNodes.Count > 0)
            {
                String current = comboNodes.First();
                comboNodes.Remove(current);
                HSCardCluster new_cluster = GenerateCardCluster(current, ref cardTable, ref net);

                if (!markers.ContainsKey(current))
                    markers.Add(current, true);

                combo.Add(new_cluster.card);

                if (config.giver_inflation)
                {
                    giver_recruiting = RecruitNeighbours(ref net, ref new_cluster.giver, ref config, ref cardTable, ref politic, ref giverPriorities, ref markers, ref combo);
                    comboNodes.AddRange(giver_recruiting);
                }

                if (config.receiver_inflation)
                {
                    receiver_recruiting = RecruitNeighbours(ref net, ref new_cluster.receiver, ref config, ref cardTable, ref politic, ref receiverPriorities, ref markers, ref combo);
                    comboNodes.AddRange(receiver_recruiting);
                }
            }

            // Mana threshold 10 mana = 1 turn, 30 mana = 3 turns combo
            if (config.cutByManaCost)
            {
                List<String> new_output = new List<String>();
                int total_mana = 0;
                foreach (String key in combo)
                {
                    String s = key.ToLower();
                    int mana_cost;
                    Int32.TryParse(cardTable[s].cost, out mana_cost);
                    total_mana += mana_cost;

                    if (total_mana >= config.total_mana)
                        break;

                    new_output.Add(key);

                    if (new_output.Count == config.maxCards)
                        break;
                }

                return new_output;
            }

            return combo;
        }

        public void FilterAbilities(ref Dictionary<String, String> filter, ref Dictionary<String, HoningNode<String>> input)
        {
            List<String> keys = new List<string>();
            foreach (String k in input.Keys)
            {
                if (!filter.ContainsKey(k))
                {
                    keys.Add(k);
                }
            }

            foreach (String k in keys)
            {
                input.Remove(k);
            }
        }

        public bool compareTargets(String input)
        {
            if (
input == "minion"
|| input == "spell"
|| input == "mech"
|| input == "dragon"
|| input == "pirate"
|| input == "murloc"
|| input == "violet apprentice"
|| input == "fireball")
            {
                return true;
            }

            /*
            if (
input == "minion"
|| input == "lightwarden"
|| input == "spell"
|| input == "weapon"
|| input == "mech"
|| input == "silver hand recruit"
|| input == "dragon"
|| input == "ashbringer"
|| input == "defender"
|| input == "pirate"
|| input == "murloc"
|| input == "nerubian"
|| input == "battlecry"
|| input == "leper gnome"
|| input == "violet apprentice"
|| input == "thaddius"
|| input == "echoing ooze"
|| input == "awesome invention"
|| input == "slime"
|| input == "finkle einhorn"
|| input == "beast"
|| input == "grim patron"
|| input == "squire"
|| input == "v-07-tr-0n"
|| input == "whelp"
|| input == "boar"
|| input == "gnoll"
|| input == "baine bloodhoof"
|| input == "dragonling"
|| input == "banana"
|| input == "dream card"
|| input == "whelps"
|| input == "imp"
|| input == "boom bots"
|| input == "spectral spider"
|| input == "damaged golem"
|| input == "rockjaw trogg"
|| input == "taunt"
|| input == "flame of azzinoth"
|| input == "defias bandit"
|| input == "spirit wolve"
|| input == "overload"
|| input == "demon"
|| input == "lord jaraxxus"
|| input == "snake"
|| input == "hyena"
|| input == "hound"
|| input == "fireball"
|| input == "wisp"
|| input == "treant"
|| input == "panther")
            {
                return true;
            }*/

            return false;
        }


        public Dictionary<String, String> GetRealComboPotential(ref Dictionary<String, HoningNode<String>> net, ref Dictionary<String, CardObject> cardTable, ref Dictionary<String, String> filter, int manaFilter)
        {
            Dictionary<String, String> output = new Dictionary<String, String>();

            foreach (String key in net.Keys)
            {
                if (net[key].mark != "terminal")
                    continue;

                CardObject obj = cardTable[key];

                int cardMana;
                Int32.TryParse(obj.cost, out cardMana);

                foreach (CardAbility ab in obj.abilities)
                {
                    bool hasTarget = false;
                    foreach (String target in ab.target)
                    {
                        if (compareTargets(target))
                        {
                            hasTarget = true;
                            break;
                        }
                    }

                    if (hasTarget && filter.ContainsKey(ab.ability) && cardMana <= manaFilter)
                    {
                        output.Add(key, key);
                        break;
                    }
                }
            }

            return output;
        }

        public bool ListCheck(ref List<String> A, ref List<String> B)
        {
            bool contains = false;
            foreach (String tar in B)
            {
                if (A.Contains(tar))
                {
                    contains = true;
                    break;
                }
            }
            return contains;
        }

        public void RemoveUnwantedTargets(ref Dictionary<String, HoningNode<String>> comboByTargets, ref Dictionary<String, CardObject> cardTable, ref List<String> targets, String abilityKey)
        {

            List<String> keys = new List<string>();

            foreach (String sKey in comboByTargets.Keys)
            {
                CardObject card = cardTable[sKey];
                CardAbility ability = ExtractAbilityObject(ref card, abilityKey);
                if (!ListCheck(ref targets, ref ability.target))
                {
                    keys.Add(sKey);
                }
            }

            foreach (String key in keys)
            {
                comboByTargets.Remove(key);
            }
        }

        public double CalculateCreativity(double surprise, double efficiency)
        {
            return (surprise + efficiency) - Math.Sqrt(Math.Abs(surprise - efficiency));
        }

        public void GenerateCardComboTreeSimplified(
String seed,
int maxDepth,
ref System.IO.StreamWriter file,
ref Dictionary<String, bool> visited,
ref ComboNode combos,
ref Dictionary<String, CardObject> cardTable,
ref HoningNetwork<String> net,
ref Dictionary<String, String> filter,
ref HSHoningBayes bayes,
ref KNNEfficiency knn)
        {
            String s = seed.ToLower();

            if (!net.HasNode(s))
                return;

            visited.Add(s, true);
            ComboNode comboInstance = new ComboNode();

            foreach (String comboCard in combos.combo.Keys)
                comboInstance.combo.Add(comboCard, comboCard);

            comboInstance.combo.Add(s, s);

            HoningNode<String> root = net.getHoningNode(s);

            if (root.mark != "terminal")
                return;

            CardObject cardObj = cardTable[s];
            if (comboInstance.combo.Count < maxDepth)
            {
                foreach (CardAbility ability in cardObj.abilities)
                {
                    if ((ability.target[0] == "spell" || ability.target[0] == "minion") && filter.ContainsKey(ability.ability))
                    {
                        HoningNode<String> target = net.getHoningNode(ability.target[0]);

                        foreach (String son in target.parents.Keys)
                        {
                            if (!visited.ContainsKey(son))
                            {
                                GenerateCardComboTreeSimplified(son, maxDepth, ref file, ref visited, ref comboInstance, ref cardTable, ref net, ref filter, ref bayes, ref knn);
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            foreach (String comboCard in comboInstance.combo.Keys)
                file.Write(comboCard + " ");
            file.WriteLine();

            /*
            if (comboInstance.combo.Count > 1)
            {
                // Calculate combo creativity
                Double[] comboArray;
                bayes.GenerateComboVector(ref comboInstance, out comboArray);
                Instance knnInstance = new Instance(comboArray);
                Double newWinrate = knn.getKNearestWinrates(knnInstance, 10);
                newWinrate /= 100;

                // Calculate surprise
                double[] surpriseVec;
                double surprise;
                bayes.CalculateSurprise(ref comboInstance, 1, out surpriseVec, out surprise, false);

                double creativity = CalculateCreativity(surprise, 1.0f);
            }*/
        }

        public void GenerateCardComboTree(
        String seed,
        int maxDepth,
        ref System.IO.StreamWriter file,
        ref Dictionary<String, bool> visited,
        ref ComboNode combos,
        ref Dictionary<String, CardObject> cardTable,
        ref HoningNetwork<String> net,
        ref Dictionary<String, String> filter,
        ref HSHoningBayes bayes,
        ref KNNEfficiency knn)
        {
            String s = seed.ToLower();

            if (!net.HasNode(s))
                return;

            Dictionary<String, bool> visitedInstance = new Dictionary<string, bool>();
            foreach (String visitedCard in visited.Keys)
                visitedInstance.Add(visitedCard, true);
            visitedInstance.Add(s, true);

            ComboNode comboInstance = new ComboNode();

            foreach (String comboCard in combos.combo.Keys)
                comboInstance.combo.Add(comboCard, comboCard);

            comboInstance.combo.Add(s, s);

            HoningNode<String> root = net.getHoningNode(s);

            if (root.mark != "terminal")
                return;

            CardObject cardObj = cardTable[s];
            if (comboInstance.combo.Count < maxDepth)
            {
                foreach (CardAbility ability in cardObj.abilities)
                {
                    if ((ability.target[0] == "spell" || ability.target[0] == "minion") && filter.ContainsKey(ability.ability))
                    {
                        HoningNode<String> target = net.getHoningNode(ability.target[0]);

                        foreach (String son in target.parents.Keys)
                        {
                            if (!visitedInstance.ContainsKey(son))
                            {
                                GenerateCardComboTree(son, maxDepth, ref file, ref visitedInstance, ref comboInstance, ref cardTable, ref net, ref filter, ref bayes, ref knn);
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            foreach (String comboCard in comboInstance.combo.Keys)
                file.Write(comboCard + " ");
            file.WriteLine();

            /*
            if (comboInstance.combo.Count > 1)
            {
                // Calculate combo creativity
                Double[] comboArray;
                bayes.GenerateComboVector(ref comboInstance, out comboArray);
                Instance knnInstance = new Instance(comboArray);
                Double newWinrate = knn.getKNearestWinrates(knnInstance, 10);
                newWinrate /= 100;

                // Calculate surprise
                double[] surpriseVec;
                double surprise;
                bayes.CalculateSurprise(ref comboInstance, 1, out surpriseVec, out surprise, false);

                double creativity = CalculateCreativity(surprise, 1.0f);
            }*/
        }

        public Dictionary<String, HoningNode<String>> GenerateCardCluster(
            String seed,
            ref Dictionary<String, CardObject> cardTable,
            ref HoningNetwork<String> net,
            ref Dictionary<String, String> abilityFilter,
            ref HSHoningBayes bayes,
            ref KNNEfficiency knn,
            int manaCut,
            int k,
            ref double highestSuprise,
            out double cumulativeCreativity,
            out double cumulativeSurprise,
            out double cumulativeEfficiency)
        {
            String s = seed.ToLower();
            cumulativeCreativity = 0.0;
            cumulativeSurprise = 0.0;
            cumulativeEfficiency = 0.0;

            //s = "southsea captain";

            if (!net.HasNode(s))
                return null;

            ComboNode combo = new ComboNode();
            Dictionary<String, HoningNode<String>> comboByTargets = new Dictionary<String, HoningNode<String>>();
            List<KeyValuePair<Double, String>> outCreativity = new List<KeyValuePair<double, string>>();

            comboByTargets.Add(s, net.getHoningNode(s));

            HoningNode<String> lowestNode = null;
            int totalMana = 0;
            int mana;
            Int32.TryParse(cardTable[s].cost, out mana);
            totalMana += mana;

            List<CardObject> cards = new List<CardObject>();
            cards.Add(cardTable[s]);
            combo.combo.Add(s, s);

            Dictionary<String, String> toCheck = new Dictionary<String, String>();
            bool scape = false;
            while (cards.Count > 0)
            {
                CardObject card = cards.First();
                cards.Remove(cards.First());
                double lowest = 0.0;
                String lowestExpansion = "";

                List<HoningNode<String>> searchSpace = new List<HoningNode<String>>();

                foreach (CardAbility a in card.abilities)
                {
                    if (abilityFilter.ContainsKey(a.ability.ToLower()))
                    {
                        Dictionary<String, HoningNode<String>> network = net.getNetwork();

                        if (!network.ContainsKey(a.target[0]))
                            continue;

                        foreach (String c in network[a.target[0]].parents.Keys)
                        {
                            if (!combo.combo.ContainsKey(c) && !toCheck.ContainsKey(c) && network[a.target[0]].parents[c].mark == "terminal")
                                searchSpace.Add(network[a.target[0]].parents[c]);
                        }
                    }
                }

                // Escape I
                if (searchSpace.Count == 0 && card.name.ToLower() == s)
                {
                    scape = true;
                    break;
                }

                if (searchSpace.Count > 0)
                {
                    foreach ( HoningNode<String> parent in searchSpace)
                    {
                        if (combo.combo.ContainsKey(parent.holder))
                            continue;

                        //Calculate combo creativity
                        combo.combo.Add(parent.holder, parent.holder);

                        Double[] comboArray;
                        bayes.GenerateComboVector(ref combo, out comboArray);
                        Instance target = new Instance(comboArray);
                        Double newWinrate = knn.getKNearestWinrates(target, k);
                        newWinrate /= 100;

                        // Calculate surprise
                        double[] surpriseVec;
                        double surprise;
                        bayes.CalculateSurprise(ref combo, 1, out surpriseVec, out surprise, false);

                        if (surprise > highestSuprise)
                            highestSuprise = surprise;

                        double creativity = CalculateCreativity(surprise / highestSuprise, newWinrate);

                        if (creativity > lowest)
                        {
                            lowest = creativity;
                            lowestExpansion = parent.holder;
                            lowestNode = parent;

                            cumulativeCreativity = newWinrate + surprise;
                            cumulativeEfficiency = newWinrate;
                            cumulativeSurprise = surprise;
                        }

                        combo.combo.Remove(parent.holder);
                    }// End cards

                    // Mana cut
                    Int32.TryParse(cardTable[lowestExpansion].cost, out mana);

                    if ((totalMana + mana > manaCut))
                    {
                        if (!toCheck.ContainsKey(lowestExpansion))
                            toCheck.Add(lowestExpansion, lowestExpansion);
                    }
                    else
                    {
                        totalMana += mana;
                        cards.Add(cardTable[lowestExpansion]);
                        combo.combo.Add(lowestExpansion, lowestExpansion);
                        comboByTargets.Add(lowestExpansion, lowestNode);
                        toCheck.Add(lowestExpansion, lowestExpansion);
                    }
                }

                // SCAPE II
                if (totalMana == manaCut || scape)
                    break;

                if (cards.Count == 0)
                {
                    cards.Add(cardTable[s]);
                }
            }

            return comboByTargets;
        }



        public Dictionary<String, HoningNode<String>> GenerateCardClusterRandom(
      String seed,
      ref Dictionary<String, CardObject> cardTable,
      ref HoningNetwork<String> net,
      ref Dictionary<String, String> abilityFilter,
      ref HSHoningBayes bayes,
      ref KNNEfficiency knn,
      int manaCut,
            int k,
            double highestSurprise,
      out double cumulativeCreativity,
      out double cumulativeSurprise,
      out double cumulativeEfficiency)
        {
            String s = seed.ToLower();
            cumulativeCreativity = 0.0;
            cumulativeSurprise = 0.0;
            cumulativeEfficiency = 0.0;

            if (!net.HasNode(s))
                return null;
            ComboNode combo = new ComboNode();
            Dictionary<String, HoningNode<String>> comboByTargets = new Dictionary<String, HoningNode<String>>();
            List<KeyValuePair<Double, String>> outCreativity = new List<KeyValuePair<double, string>>();

            comboByTargets.Add(s, net.getHoningNode(s));


            HoningNode<String> lowestNode = null;
            int totalMana = 0;
            int mana;
            Int32.TryParse(cardTable[s].cost, out mana);
            totalMana += mana;

            List<CardObject> cards = new List<CardObject>();
            cards.Add(cardTable[s]);
            combo.combo.Add(s, s);

            Dictionary<String, String> toCheck = new Dictionary<String, String>();
            bool scape = false;

            while (cards.Count > 0)
            {
                CardObject card = cards.First();
                cards.Remove(cards.First());
                String lowestExpansion = "";

                foreach (CardAbility a in card.abilities)
                {
                    if (abilityFilter.ContainsKey(a.ability.ToLower()))
                    {
                        Dictionary<String, HoningNode<String>> network = net.getNetwork();

                        if (!network.ContainsKey(a.target[0]))
                            continue;

                        List<HoningNode<String>> searchSpace = new List<HoningNode<String>>();

                        foreach (String c in network[a.target[0]].parents.Keys)
                        {
                            if (!combo.combo.ContainsKey(c) && !toCheck.ContainsKey(c) && network[a.target[0]].parents[c].mark == "terminal")
                                searchSpace.Add(network[a.target[0]].parents[c]);
                        }

                        if (searchSpace.Count == 0)
                        {
                            scape = true;
                            break;
                        }

                        Random rand = new Random();
                        int randomParent = rand.Next(searchSpace.Count - 1);
                        lowestNode = searchSpace.ElementAt(randomParent);
                        lowestExpansion = lowestNode.holder;

                        if (combo.combo.ContainsKey(lowestNode.holder))
                            continue;

                        // Mana cut
                        Int32.TryParse(cardTable[lowestExpansion].cost, out mana);

                        if ((totalMana + mana > manaCut))
                        {
                            if (!toCheck.ContainsKey(lowestExpansion))
                                toCheck.Add(lowestExpansion, lowestExpansion);
                            //totalMana = mana;
                        }
                        else
                        {
                            totalMana += mana;
                            cards.Add(cardTable[lowestExpansion]);
                            combo.combo.Add(lowestExpansion, lowestExpansion);
                            comboByTargets.Add(lowestExpansion, lowestNode);
                            toCheck.Add(lowestExpansion, lowestExpansion);
                        }
                    }
                }// End ability

                if (totalMana == manaCut || scape)
                    break;

                if (cards.Count == 0)
                {
                    cards.Add(cardTable[s]);
                }
            }

            Double[] comboArray;
            bayes.GenerateComboVector(ref combo, out comboArray);
            Instance target = new Instance(comboArray);
            Double newWinrate = knn.getKNearestWinrates(target, k);
            newWinrate /= 100;

            // Calculate surprise
            double[] surpriseVec;
            double surprise;
            bayes.CalculateSurprise(ref combo, 1, out surpriseVec, out surprise, false);

            if (surprise > highestSurprise)
                highestSurprise = surprise;

            double creativity = CalculateCreativity(surprise / highestSurprise, newWinrate);

            cumulativeCreativity = creativity;
            cumulativeEfficiency = newWinrate;
            cumulativeSurprise = surprise;

            return comboByTargets;
        }

        public HSCardCluster GenerateCardCluster(String seed, ref Dictionary<String, CardObject> cardTable, ref HoningNetwork<String> net)
        {
            String s = seed.ToLower();

            if (!net.HasNode(s))
                return null;

            HSCardCluster cluster = new HSCardCluster();

            cluster.card = seed;

            List<String> targets = new List<string>();
            CardObject Antonidas = cardTable[s];

            net.getMfList(s, out targets, "essential");

            Dictionary<String, String> selfAbilityFilter = GenerateAbilityFilter();

            foreach (CardAbility a in Antonidas.abilities)
            {
                if (selfAbilityFilter.ContainsKey(a.ability.ToLower()))
                {
                    if (!cluster.receiver.ContainsKey(a.ability))
                    {
                        Dictionary<String, HoningNode<String>> comboByTargets = GetCardsBySingleBound(ref net, a.target[0], "terminal");

                        cluster.receiver.Add(a.ability, comboByTargets);
                    }
                }
            }

            Dictionary<String, HoningNode<String>> giverAbilities;
            net.GetRelationsFromEssential(s, out giverAbilities);
            FilterAbilities(ref selfAbilityFilter, ref giverAbilities);
            foreach (String key in giverAbilities.Keys)
            {
                Dictionary<String, HoningNode<String>> comboByTargets = GetCardsBySingleBound(ref net, key, "terminal");

                RemoveUnwantedTargets(ref comboByTargets, ref cardTable, ref targets, key);

                cluster.giver.Add(key, comboByTargets);
            }

            return cluster;
        }

        public Dictionary<String, HoningNode<String>> GetCardsBySingleBound(ref HoningNetwork<String> net, String target, String filter = null)
        {
            Dictionary<String, HoningNode<String>> recruit = new Dictionary<string, HoningNode<string>>();
            List<String> targets = new List<string>();
            targets.Add(target);
            net.recruitNeurds(targets, out recruit, filter);
            return recruit;
        }

        public List<String> GetCardsByBound(ref HoningNetwork<String> net, List<String> targets)
        {
            Dictionary<String, HoningNode<String>> recruit = new Dictionary<string, HoningNode<string>>();
            net.recruitNeurds(targets, out recruit);
            List<String> ret = new List<string>();
            foreach (String k in recruit.Keys)
            {
                ret.Add(recruit[k].holder);
            }
            return ret;
        }

        public CardAbility ExtractAbilityObject(ref CardObject card, String ability)
        {
            if (card == null)
                return null;

            CardAbility toExtract = null;

            foreach (CardAbility ab in card.abilities)
            {
                if (ab.ability == ability)
                {
                    toExtract = ab;
                    break;
                }
            }

            return toExtract;
        }

        public Dictionary<String, String> GenerateAbilityFilter()
        {
            List<String> ori = new List<string>();
            Dictionary<String, String> dic = new Dictionary<string, string>();

            ori.Add("transform");
            ori.Add("plusHealth");
            ori.Add("swapHealth");
            ori.Add("copy");
            ori.Add("plusAttack");
            ori.Add("spellPower");
            ori.Add("equip");
            ori.Add("plusDurability");
            ori.Add("Triggered");
            ori.Add("secret");
            ori.Add("divineShield");
            ori.Add("reduceCost");
            ori.Add("taunt");
            ori.Add("restore");
            ori.Add("charge");
            ori.Add("windfury");
            ori.Add("stealth");
            ori.Add("swap");
            ori.Add("enrage");
            ori.Add("return");
            ori.Add("immune");
            ori.Add("replaceHero");
            ori.Add("setTurnTime");
            ori.Add("combo");
            //ori.Add("overload");
            ori.Add("spellDamage");
            ori.Add("summon");
            ori.Add("conditional");

            //ori.Add("silence");

            foreach (String k in ori)
            {
                dic.Add(k.ToLower(), k.ToLower());
            }

            return dic;
        }
    }
}
