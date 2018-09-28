using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpHoning
{
    class HSHoningBayes
    {
        public HSHoningBayes(String hero, ref HSCombosParser combosParser, ref HoningNetwork<String> net, ref Dictionary<String, double[]> cardDataset, ref Dictionary<String, CardObject> cardTable, int comboLimit)
        {
            this.net = net;
            this.cardTable = cardTable;
            bayesMatrix = new List<double[]>();
            
            globalSurprise = 0.0f;

            int totalCombos = 0;
            foreach (int key in combosParser.combos_by_quantity[hero].Keys)
            {
                int comboPerQuantity = 0;
                foreach (ComboNode node in combosParser.combos_by_quantity[hero][key])
                {
                    if (comboPerQuantity == comboLimit)
                        break;

                    ComboNode combo = node;
                    double[] bayesCombo;
                    GenerateComboVector(ref combo, ref cardDataset, out bayesCombo);
                    bayesMatrix.Add(bayesCombo);

                    comboPerQuantity++;

                    totalCombos++;
                }
            }

            winrates = new double[totalCombos];

            // Populate winrates
            int currentCombo = 0;
            foreach (int key in combosParser.combos_by_quantity[hero].Keys)
            {
                int comboPerQuantity = 0;
                foreach (ComboNode node in combosParser.combos_by_quantity[hero][key])
                {
                    if (comboPerQuantity == comboLimit)
                        break;

                    winrates[currentCombo] = node.winrate_mean;

                    comboPerQuantity++;

                    currentCombo++;
                }
            }

            CalculateStatistics();
        }

        public void GenerateComboVector(ref ComboNode combo, ref Dictionary<String, double[]> cardVec, out double[] HearthStoneBayesVector)
        {
            int vectorSize = cardVec.ElementAt(0).Value.Length;
            HearthStoneBayesVector = new double[vectorSize];

            // populate vector
            foreach (String card in combo.combo.Keys)
            {
                double[] cardDataset = cardVec[card];
                for (int i = 0; i < cardDataset.Length; i++)
                {
                    HearthStoneBayesVector[i] += cardDataset[i];
                }
            }

            HearthStoneBayesVector[vectorSize - 1] = combo.winrate_median;
        }

        public void CalculateSurprise(ref ComboNode combo, int N, out double[] surpriseVector, ref Dictionary<String, double[]> cardVec, out double surpriseMean, bool updateBayes)
        {
            double[] comboVector;
            GenerateComboVector(ref combo, ref cardVec, out comboVector);
            surpriseMean = 0.0;

            surpriseVector = new double[varianceVector.Length];
            for (int i = 0; i < varianceVector.Length; i++)
            {
                if (varianceVector[i] <= 0.0)
                    surpriseVector[i] = 0.0;
                else
                    surpriseVector[i] = (N / (2 * varianceVector[i])) * (varianceVector[i] + Math.Pow((comboVector[i] - meanVector[i]), 2.0));
                surpriseMean += surpriseVector[i];
            }

            //surpriseMean /= surpriseVector.Length;

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
        private HoningNetwork<String> net;
        private Dictionary<String, CardObject> cardTable; 
    }
}
