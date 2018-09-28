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
            Dictionary<String, HoningNode<String>> workspace,
            Dictionary<String, CardObject> cardTable,
            HSCardExpasionConfiguration config,
            ExpansionGeneralPolitics politics,
            Dictionary<String, Boolean> markers,
            List<String> combo)
        {
            List<String> res = new List<string>();

            // Inflation window
            int windowSize = 3;
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
                        if (node.combo.ContainsKey(s))
                            continue;

                        node.combo.Add(s,s);

                        // Calculate efficiency
                        Double[] comboArray;
                        config.bayes.GenerateComboVector(ref node, out comboArray);
                        Instance target = new Instance(comboArray);
                        Double newWinrate = config.knn.getKNearestWinrates(target, 5);
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

                        if (surprise >= localHighSurprise)
                        {
                            localHighSurprise = surprise;
                            hs = s;
                        }

                        // Calculate criativity
                        double creativityFactor = ((surprise / config.highestSurprise) + newWinrate) / 2;

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

            Dictionary<String, Dictionary<String, HoningNode<String>>> workingspace = new Dictionary<string,Dictionary<string,HoningNode<string>>>();

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
                List<String> space = InflateNeighbours(dic, cardTable, config, politic, markers, combo);
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
                
                if(!markers.ContainsKey(current))
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
            ori.Add("overload");
            ori.Add("spellDamage");
            ori.Add("summon");

            foreach (String k in ori)
            {
                dic.Add(k.ToLower(), k.ToLower());
            }

            return dic;
        }
    }
}
