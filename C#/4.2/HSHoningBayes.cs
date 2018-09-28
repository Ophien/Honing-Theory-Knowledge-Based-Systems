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

namespace SharpHoning
{
    class HSHoningBayes
    {
        public HSHoningBayes(String hero, ref HSCombosParser combosParser, ref HoningNetwork<String> net, ref Dictionary<String, CardObject> cardTable)
        {
            this.net = net;
            this.cardTable = cardTable;
            bayesMatrix = new List<double[]>();
            
            globalSurprise = 0.0f;

            int totalCombos = 0;
            foreach (int key in combosParser.combos_by_quantity[hero].Keys)
            {
                foreach (ComboNode node in combosParser.combos_by_quantity[hero][key])
                {
                    ComboNode combo = node;
                    double[] bayesCombo;
                    GenerateComboVector(ref combo, out bayesCombo);
                    bayesMatrix.Add(bayesCombo);

                    totalCombos++;
                }
            }

            winrates = new double[totalCombos];

            // Populate winrates
            int currentCombo = 0;
            foreach (int key in combosParser.combos_by_quantity[hero].Keys)
            {
                foreach (ComboNode node in combosParser.combos_by_quantity[hero][key])
                {
                    winrates[currentCombo] = node.winrate_mean;
                    currentCombo++;
                }
            }

            CalculateStatistics();
        }

        public void GenerateComboVector(ref ComboNode combo, out double[] HearthStoneBayesVector)
        {
            net.leafIDMapping(out id_mapping, out inverse_id_mapping);
            int vectorSize = id_mapping.Count;
            HearthStoneBayesVector = new double[vectorSize];

            // populate vector
            foreach (String card in combo.combo.Keys)
            {
                // get all the card abilities and put into the vector
                CardObject cardobj = cardTable[card];
                foreach (CardAbility ability in cardobj.abilities)
                {
                    int vectorId = id_mapping[ability.ability];
                    float abilityValue;
                    float.TryParse(ability.value, out abilityValue);
                    HearthStoneBayesVector[vectorId] += abilityValue;
                }
            }
        }

        public void CalculateSurprise(ref ComboNode combo, int N, out double[] surpriseVector, out double surpriseMean, bool updateBayes)
        {
            double[] comboVector;
            GenerateComboVector(ref combo, out comboVector);
            surpriseMean = 0.0;

            surpriseVector = new double[varianceVector.Length];
            for (int i = 0; i < varianceVector.Length; i++)
            {
                if (varianceVector[i] <= 0.0)
                    surpriseVector[i] = 0.0;
                else
                    surpriseVector[i] = (N / (2 * varianceVector[i])) * (varianceVector[i] + (comboVector[i] - meanVector[i]));
                surpriseMean += surpriseVector[i];
            }

            surpriseMean /= surpriseVector.Length;

            if (updateBayes)
            {
                bayesMatrix.Add(comboVector);
                CalculateStatistics();
            }
        }

        private void CalculateStatistics()
        {
            sumVector = new double[bayesMatrix.First().Length];
            sumVector = new double[bayesMatrix.First().Length];
            meanVector = new double[bayesMatrix.First().Length];
            varianceVector = new double[bayesMatrix.First().Length];
            standardDeviation = new double[bayesMatrix.First().Length];

            // Sum
            for (int i = 0; i < bayesMatrix.First().Length; i++)
            {
                for (int j = 0; j < bayesMatrix.Count; j++)
                {
                    sumVector[i] += bayesMatrix[j][i];
                }
            }

            // Mean
            sumVector.CopyTo(meanVector, 0);

            for (int i = 0; i < sumVector.Length; i++)
                meanVector[i] /= bayesMatrix.Count;

            // Variance
            for (int i = 0; i < bayesMatrix.First().Length; i++)
            {
                for (int j = 0; j < bayesMatrix.Count; j++)
                {
                    varianceVector[i] += Math.Pow((bayesMatrix[j][i] - meanVector[i]), 2.0);
                }
            }

            // Standard deviation
            varianceVector.CopyTo(standardDeviation, 0);
            for (int i = 0; i < standardDeviation.Length; i++)
            {
                standardDeviation[i] = Math.Sqrt(varianceVector[i]);
            }
        }

        public void SaveKNNDataset(String path)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(path);

            int currentCombo = 0;
            foreach(double[] vec in bayesMatrix){
                String line = "";
                for (int i = 0; i < vec.Length; i++)
                {
                    line += vec[i].ToString();
                    if (i < vec.Length - 1)
                        line += ",";

                }
                line += "," + winrates[currentCombo];
                file.WriteLine(line);
                currentCombo++;
            }

            file.Close();
        }

        public double[] standardDeviation;
        public double[] varianceVector;
        public double[] sumVector;
        public double[] meanVector;
        public double[] winrates;
        public double globalSurprise;
        public List<double[]> bayesMatrix;
        public Dictionary<String, int> id_mapping;
        public Dictionary<int, String> inverse_id_mapping;
        private HoningNetwork<String> net;
        private Dictionary<String, CardObject> cardTable; 
    }
}
