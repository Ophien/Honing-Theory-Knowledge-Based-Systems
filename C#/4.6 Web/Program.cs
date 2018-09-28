using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNN;

namespace SharpHoning
{
    public class result
    {
        public result(
            List<String> combo = null,
            double fitness = 0.0,
            double creativity = 0.0,
            double normCreativity = 0.0,
            double surprise = 0.0,
            double normSurprise = 0.0,
            double efficiency = 0.0,
            double normEfficiency = 0.0,
            double highSurp = 0.0,
            double minSurp = 0.0,
            double highEff = 0.0,
            double minEff = 0.0)
        {
            cards = new List<string>();

            if (combo != null)
                foreach (String s in combo)
                    cards.Add(s);

            this.fitness = fitness;
            this.creativity = creativity;
            this.normCreativity = normCreativity;
            this.surprise = surprise;
            this.normSurprise = normSurprise;
            this.efficiency = efficiency;
            this.normEfficiency = normEfficiency;
            this.highSurp = highSurp;
            this.minSurp = minSurp;
            this.highEff = highEff;
            this.minEff = minEff;
        }

        public void writeIntoFile(System.IO.StreamWriter file, bool lite = false)
        {
            if (!lite)
            {
                file.WriteLine("Fitness: " + fitness + "\nRaw creativity: " + creativity + "\nNormalized creativity: " + normCreativity + "\nSurprise " + surprise + "\nNormalized surprise: " + normSurprise + "\nEfficiency: " + efficiency + "\nNormalized efficiency: " + normEfficiency);
                file.WriteLine("Highest surprise: " + highSurp + "\nLowest surprise: " + minSurp);
                file.WriteLine("Highest efficiency: " + highEff + "\nLowest efficiency: " + minEff + "\n");
                file.WriteLine("Cards:\n");
                foreach (String st in cards)
                {
                    file.Write("(" + st + ") ");
                }
                file.WriteLine();
            }
            else
            {
                file.WriteLine("Fitness: " + fitness + "\nRaw creativity: " + creativity + "\nNormalized creativity: " + normCreativity + "\nSurprise " + surprise + "\nNormalized surprise: " + normSurprise + "\nEfficiency: " + efficiency + "\nNormalized efficiency: " + normEfficiency);
            }
        }

        public double fitness, creativity, normCreativity, surprise, normSurprise, efficiency, normEfficiency, highSurp, minSurp, highEff, minEff;
        public List<String> cards;
    }

    public class Program
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

        public void calculateStatistics(List<result> input, out result sum, out result mean, out result variance, out result deviation, out result median, out result higher, out result lower)
        {
            List<result> op = new List<result>();
            op.AddRange(input);
            sum = new result();
            mean = new result();
            variance = new result();
            deviation = new result();
            median = new result();
            higher = new result();
            lower = new result();

            if (input.Count == 0)
                return;

            lower.creativity = double.MaxValue;
            lower.efficiency = double.MaxValue;
            lower.fitness = double.MaxValue;
            lower.surprise = double.MaxValue;
            lower.normCreativity = double.MaxValue;
            lower.normSurprise = double.MaxValue;
            lower.normEfficiency = double.MaxValue;

            foreach (result r in op)
            {
                if (r.creativity > higher.creativity)
                    higher.creativity = r.creativity;
                if (r.efficiency > higher.efficiency)
                    higher.efficiency = r.efficiency;
                if (r.fitness > higher.fitness)
                    higher.fitness = r.fitness;
                if (r.surprise > higher.surprise)
                    higher.surprise = r.surprise;
                if (r.normCreativity > higher.normCreativity)
                    higher.normCreativity = r.normCreativity;
                if (r.normSurprise > higher.normSurprise)
                    higher.normSurprise = r.normSurprise;
                if (r.normEfficiency > higher.normEfficiency)
                    higher.normEfficiency = r.normEfficiency;

                if (r.creativity < lower.creativity)
                    lower.creativity = r.creativity;
                if (r.efficiency < lower.efficiency)
                    lower.efficiency = r.efficiency;
                if (r.fitness < lower.fitness)
                    lower.fitness = r.fitness;
                if (r.surprise < lower.surprise)
                    lower.surprise = r.surprise;
                if (r.normCreativity < lower.normCreativity)
                    lower.normCreativity = r.normCreativity;
                if (r.normSurprise < lower.normSurprise)
                    lower.normSurprise = r.normSurprise;
                if (r.normEfficiency < lower.normEfficiency)
                    lower.normEfficiency = r.normEfficiency;

                sum.creativity += r.creativity;
                sum.efficiency += r.efficiency;
                sum.fitness += r.fitness;
                sum.surprise += r.surprise;
                sum.normCreativity += r.normCreativity;
                sum.normSurprise += r.normSurprise;
                sum.normEfficiency += r.normEfficiency;
            }

            mean.creativity = sum.creativity / op.Count;
            mean.efficiency = sum.efficiency / op.Count;
            mean.fitness = sum.fitness / op.Count;
            mean.surprise = sum.surprise / op.Count;
            mean.normCreativity = sum.normCreativity / op.Count;
            mean.normSurprise = sum.normSurprise / op.Count;
            mean.normEfficiency = sum.normEfficiency / op.Count;

            for (int i = 0; i < op.Count; i++)
            {
                variance.creativity += Math.Pow(op[i].creativity - mean.creativity, 2.0);
                variance.efficiency += Math.Pow(op[i].efficiency - mean.efficiency, 2.0);
                variance.surprise += Math.Pow(op[i].surprise - mean.surprise, 2.0);
                variance.fitness += Math.Pow(op[i].fitness - mean.fitness, 2.0);
                variance.normCreativity += Math.Pow(op[i].normCreativity - mean.normCreativity, 2.0);
                variance.normSurprise += Math.Pow(op[i].normSurprise - mean.normSurprise, 2.0);
                variance.normEfficiency += Math.Pow(op[i].normEfficiency - mean.normEfficiency, 2.0);
            }

            variance.creativity /= op.Count;
            variance.efficiency /= op.Count;
            variance.surprise /= op.Count;
            variance.fitness /= op.Count;
            variance.normCreativity /= op.Count;
            variance.normSurprise /= op.Count;
            variance.normEfficiency /= op.Count;

            deviation.creativity = Math.Sqrt(variance.creativity);
            deviation.efficiency = Math.Sqrt(variance.efficiency);
            deviation.fitness = Math.Sqrt(variance.fitness);
            deviation.surprise = Math.Sqrt(variance.surprise);
            deviation.normCreativity = Math.Sqrt(variance.normCreativity);
            deviation.normSurprise = Math.Sqrt(variance.normSurprise);
            deviation.normEfficiency = Math.Sqrt(variance.normEfficiency);

            int inputPos = 0;

            if (op.Count % 2 == 0)
            {
                inputPos = op.Count / 2;
                op.Sort(delegate(result A, result B) { return A.creativity.CompareTo(B.creativity); });
                median.creativity = (op[inputPos].creativity + op[inputPos + 1].creativity) / 2;
                op.Sort(delegate(result A, result B) { return A.efficiency.CompareTo(B.efficiency); });
                median.efficiency = (op[inputPos].efficiency + op[inputPos + 1].efficiency) / 2;
                op.Sort(delegate(result A, result B) { return A.surprise.CompareTo(B.surprise); });
                median.surprise = (op[inputPos].surprise + op[inputPos + 1].surprise) / 2;
                op.Sort(delegate(result A, result B) { return A.fitness.CompareTo(B.fitness); });
                median.fitness = (op[inputPos].fitness + op[inputPos + 1].fitness) / 2;
                op.Sort(delegate(result A, result B) { return A.normCreativity.CompareTo(B.normCreativity); });
                median.normCreativity = (op[inputPos].normCreativity + op[inputPos + 1].normCreativity) / 2;
                op.Sort(delegate(result A, result B) { return A.normSurprise.CompareTo(B.normSurprise); });
                median.normSurprise = (op[inputPos].normSurprise + op[inputPos + 1].normSurprise) / 2;
                op.Sort(delegate(result A, result B) { return A.normEfficiency.CompareTo(B.normEfficiency); });
                median.normEfficiency = (op[inputPos].normEfficiency + op[inputPos + 1].normEfficiency) / 2;
            }
            else
            {
                inputPos = op.Count / 2;
                op.Sort(delegate(result A, result B) { return A.creativity.CompareTo(B.creativity); });
                median.creativity = (op[inputPos].creativity);
                op.Sort(delegate(result A, result B) { return A.efficiency.CompareTo(B.efficiency); });
                median.efficiency = (op[inputPos].efficiency);
                op.Sort(delegate(result A, result B) { return A.surprise.CompareTo(B.surprise); });
                median.surprise = (op[inputPos].surprise);
                op.Sort(delegate(result A, result B) { return A.fitness.CompareTo(B.fitness); });
                median.fitness = (op[inputPos].fitness);
                op.Sort(delegate(result A, result B) { return A.normCreativity.CompareTo(B.normCreativity); });
                median.normCreativity = (op[inputPos].normCreativity);
                op.Sort(delegate(result A, result B) { return A.normSurprise.CompareTo(B.normSurprise); });
                median.normSurprise = (op[inputPos].normSurprise);
                op.Sort(delegate(result A, result B) { return A.normEfficiency.CompareTo(B.normEfficiency); });
                median.normEfficiency = (op[inputPos].normEfficiency);
            }
        }

        void readResults(String file, out List<result> results, ref Double maxSurp, ref Double minSurp, ref Double maxEff, ref Double minEff, double threshold)
        {
            string[] lines = System.IO.File.ReadAllLines(file);
            results = new List<result>();

            // 0 -- 18 lista de objetos
            int lineCount = lines.Length;
            int resultCount = 66;
            int step = 15;
            int currentLine = 0;
            int linerCount = 0;

            while (currentLine <= lineCount - resultCount)
            {
                result res = new result();

                char[] delim = new char[1];
                delim[0] = ' ';

                String Fitness = lines[currentLine + 0].Split(delim).Last();
                String RawCreativity = lines[currentLine + 1].Split(delim).Last();
                String NormCreativity = lines[currentLine + 2].Split(delim).Last();
                String Surprise = lines[currentLine + 3].Split(delim).Last();
                String NormSurprise = lines[currentLine + 4].Split(delim).Last();
                String Efficiency = lines[currentLine + 5].Split(delim).Last();
                String NormEfficiency = lines[currentLine + 6].Split(delim).Last();

                String highSurprise = lines[currentLine + 7].Split(delim).Last();
                String minSurprise = lines[currentLine + 8].Split(delim).Last();
                String highEfficiency = lines[currentLine + 9].Split(delim).Last();
                String minEfficiency = lines[currentLine + 10].Split(delim).Last();

                delim[0] = ')';

                String[] cards = lines[currentLine + 14].Split(delim);

                double creativity, surprise, efficiency, fitness, NormCrea, NormSur, NormEff, highSur, highEff, minSur, minEf;

                double.TryParse(Fitness, out fitness);
                double.TryParse(RawCreativity, out creativity);
                double.TryParse(Surprise, out surprise);
                double.TryParse(Efficiency, out efficiency);
                double.TryParse(highSurprise, out highSur);
                double.TryParse(minSurprise, out minSur);
                double.TryParse(highEfficiency, out highEff);
                double.TryParse(minEfficiency, out minEf);
                double.TryParse(NormEfficiency, out NormEff);
                double.TryParse(NormSurprise, out NormSur);
                double.TryParse(NormCreativity, out NormCrea);



                foreach (string s in cards)
                {
                    String rawCard = s.Replace("(", "");
                    rawCard = rawCard.Replace(" ", "");
                    //rawCard = rawCard.Replace(")", "");
                    if (rawCard == "")
                        continue;

                    res.cards.Add(rawCard);
                }

                res.creativity = creativity;
                res.surprise = surprise;
                res.efficiency = efficiency;
                res.fitness = fitness;
                res.highSurp = highSur;
                res.minSurp = minSur;
                res.highEff = highEff;
                res.minEff = minEf;
                res.normCreativity = NormCrea;
                res.normSurprise = NormSur;
                res.normEfficiency = NormEff;

                if (fitness > threshold)
                    results.Add(res);

                currentLine += step;
                linerCount += step;
            }
        }

        void recalibrateResults(String file, out List<result> results, ref Double maxSurp, ref Double minSurp, ref Double maxEff, ref Double minEff, double threshold)
        {
            string[] lines = System.IO.File.ReadAllLines(file);
            results = new List<result>();

            // 0 -- 18 lista de objetos
            int lineCount = lines.Length;
            int resultCount = 66;
            int step = 15;
            int currentLine = 0;
            int linerCount = 0;

            while (currentLine <= lineCount - resultCount)
            {
                result res = new result();

                char[] delim = new char[1];
                delim[0] = ' ';

                String Fitness = lines[currentLine + 0].Split(delim).Last();
                String RawCreativity = lines[currentLine + 1].Split(delim).Last();
                String NormCreativity = lines[currentLine + 2].Split(delim).Last();
                String Surprise = lines[currentLine + 3].Split(delim).Last();
                String NormSurprise = lines[currentLine + 4].Split(delim).Last();
                String Efficiency = lines[currentLine + 5].Split(delim).Last();
                String NormEfficiency = lines[currentLine + 6].Split(delim).Last();

                String highSurprise = lines[currentLine + 7].Split(delim).Last();
                String minSurprise = lines[currentLine + 8].Split(delim).Last();
                String highEfficiency = lines[currentLine + 9].Split(delim).Last();
                String minEfficiency = lines[currentLine + 10].Split(delim).Last();

                delim[0] = ')';

                String[] cards = lines[currentLine + 14].Split(delim);

                double creativity, surprise, efficiency, fitness;

                double.TryParse(Fitness, out fitness);

                double.TryParse(RawCreativity, out creativity);
                double.TryParse(Surprise, out surprise);
                double.TryParse(Efficiency, out efficiency);

                if (surprise > maxSurp)
                    maxSurp = surprise;

                if (surprise < minSurp)
                    minSurp = surprise;

                if (efficiency > maxEff)
                    maxEff = efficiency;

                if (efficiency < minEff)
                    minEff = efficiency;

                foreach (string s in cards)
                {
                    String rawCard = s.Replace("(", "");
                    rawCard = rawCard.Replace(" ", "");
                    //rawCard = rawCard.Replace(")", "");
                    if (rawCard == "")
                        continue;

                    res.cards.Add(rawCard);
                }

                res.creativity = creativity;
                res.surprise = surprise;
                res.efficiency = efficiency;

                if (fitness > threshold)
                    results.Add(res);

                currentLine += step;
                linerCount += step;
            }
        }

        void recalculateCreativityAndNormalization(HSCardOperator op, List<result> res, double maxSurp, double minSurp, double maxEff, double minEff)
        {
            foreach (result r in res)
            {
                double normSurp, normEff;
                double fitness = op.CalculateFitness(r.surprise, ref maxSurp, ref minSurp, out normSurp, ref maxEff, ref minEff, out normEff, r.efficiency);
                r.fitness = fitness;
                r.normSurprise = normSurp;
                r.normEfficiency = normEff;
                r.highEff = maxEff;
                r.minEff = minEff;
                r.highSurp = maxSurp;
                r.minSurp = minSurp;
                r.normCreativity = normSurp + normEff;
            }
        }

        Dictionary<String, List<result>> transferToSeedMapping(List<result> input)
        {
            Dictionary<String, List<result>> comboBySeed = new Dictionary<string, List<result>>();
            foreach (result r in input)
            {
                if (!comboBySeed.ContainsKey(r.cards.First()))
                    comboBySeed.Add(r.cards.First(), new List<result>());

                comboBySeed[r.cards.First()].Add(r);
            }
            return comboBySeed;
        }

        List<String> checkInListViableSeeds(Dictionary<String, List<result>> comboBySeed, double threshold)
        {
            List<String> viableSeeds = new List<string>();
            foreach (List<result> r in comboBySeed.Values)
            {
                foreach (result res in r)
                {
                    if (res.fitness > threshold)
                    {
                        viableSeeds.Add(res.cards.First());
                        break;
                    }
                }
            }
            return viableSeeds;
        }

        List<result> getStatisticsVector(List<String> viable, Dictionary<String, List<result>> comboBySeed)
        {
            List<result> res = new List<result>();

            foreach (String v in viable)
            {
                foreach (result i in comboBySeed[v])
                {
                    res.Add(i);
                }
            }

            return res;
        }

        void writeForPlot(string hero,
            result resI,
            result resII,
            result resIII,
            System.IO.StreamWriter meanFitness,
            System.IO.StreamWriter meanEfficiency,
            System.IO.StreamWriter meanSurprise,
            System.IO.StreamWriter meanCreativity)
        {
            // WRITE TO PLOT, dataset output
            meanFitness.WriteLine(hero + " I " + resI.fitness);
            meanFitness.WriteLine(hero + " II " + resII.fitness);
            meanFitness.WriteLine(hero + " III " + resIII.fitness);

            meanEfficiency.WriteLine(hero + " I " + resI.normEfficiency);
            meanEfficiency.WriteLine(hero + " II " + resII.normEfficiency);
            meanEfficiency.WriteLine(hero + " III " + resIII.normEfficiency);

            meanSurprise.WriteLine(hero + " I " + resI.normSurprise);
            meanSurprise.WriteLine(hero + " II " + resII.normSurprise);
            meanSurprise.WriteLine(hero + " III " + resIII.normSurprise);

            meanCreativity.WriteLine(hero + " I " + resI.normCreativity);
            meanCreativity.WriteLine(hero + " II " + resII.normCreativity);
            meanCreativity.WriteLine(hero + " III " + resIII.normCreativity);
        }

        void writeForBoxPlot(
            string hero,
            List<result> res,
            String assetName,
            System.IO.StreamWriter Fitness,
            System.IO.StreamWriter Efficiency,
            System.IO.StreamWriter Surprise,
            System.IO.StreamWriter Creativity,
            System.IO.StreamWriter mormEfficiency,
            System.IO.StreamWriter mormSurprise,
            System.IO.StreamWriter mormCreativity)
        {
            // WRITE TO PLOT, dataset output
            foreach (result r in res)
            {
                Fitness.WriteLine(hero + " " + assetName + " " + r.fitness);
                Efficiency.WriteLine(hero + " " + assetName + " " + r.efficiency);
                Surprise.WriteLine(hero + " " + assetName + " " + r.surprise);
                Creativity.WriteLine(hero + " " + assetName + " " + r.creativity);

                mormEfficiency.WriteLine(hero + " " + assetName + " " + r.normEfficiency);
                mormSurprise.WriteLine(hero + " " + assetName + " " + r.normSurprise);
                mormCreativity.WriteLine(hero + " " + assetName + " " + r.normCreativity);
            }
        }

        void extractViableSeeds(HSCardOperator op, String hero,
                        double thresHold,
            int seedFrom,
            System.IO.StreamWriter meanFitness,
            System.IO.StreamWriter meanEfficiency,
            System.IO.StreamWriter meanSurprise,
            System.IO.StreamWriter meanCreativity)
        {
            List<result> resI;
            List<result> resII;
            List<result> resIII;
            double maxEff, minEff, maxSurp, minSurp;

            maxSurp = 0.0;
            minSurp = double.MaxValue;
            maxEff = 0.0;
            minEff = double.MaxValue;

            readResults(hero + "_results_I_RENORMALIZED_REVISED.dat", out resI, ref maxSurp, ref minSurp, ref maxEff, ref minEff, 0);
            readResults(hero + "_results_II_RENORMALIZED_REVISED.dat", out resII, ref maxSurp, ref minSurp, ref maxEff, ref minEff, 0);
            readResults(hero + "_results_III_RENORMALIZED_REVISED.dat", out resIII, ref maxSurp, ref minSurp, ref maxEff, ref minEff, 0);

            Dictionary<String, List<result>> comboBySeed = transferToSeedMapping(resI);
            Dictionary<String, List<result>> comboBySeedII = transferToSeedMapping(resII);
            Dictionary<String, List<result>> comboBySeedIII = transferToSeedMapping(resIII);

            List<String> viableSeeds = new List<string>();

            switch (seedFrom)
            {
                case 1:
                    viableSeeds = checkInListViableSeeds(comboBySeed, thresHold);
                    break;
                case 2:
                    viableSeeds = checkInListViableSeeds(comboBySeedII, thresHold);
                    break;
                case 3:
                    viableSeeds = checkInListViableSeeds(comboBySeedIII, thresHold);
                    break;
            }


            List<result> rI = getStatisticsVector(viableSeeds, comboBySeed);
            List<result> rII = getStatisticsVector(viableSeeds, comboBySeedII);
            List<result> rIII = getStatisticsVector(viableSeeds, comboBySeedIII);

            result sumI, varianceI, meanI, medianI, deviationI, maxI, minI;
            calculateStatistics(rI, out sumI, out meanI, out varianceI, out deviationI, out medianI, out maxI, out minI);

            result sumII, varianceII, meanII, medianII, deviationII, maxII, minII;
            calculateStatistics(rII, out sumII, out meanII, out varianceII, out deviationII, out medianII, out maxII, out minII);

            result sumIII, varianceIII, meanIII, medianIII, deviationIII, maxIII, minIII;
            calculateStatistics(rIII, out sumIII, out meanIII, out varianceIII, out deviationIII, out medianIII, out maxIII, out minIII);

            //To plot
            writeForPlot(hero, maxI, maxII, maxIII, meanFitness, meanEfficiency, meanSurprise, meanCreativity);

            System.IO.StreamWriter I = new System.IO.StreamWriter(hero + "_viableI.dat");
            System.IO.StreamWriter II = new System.IO.StreamWriter(hero + "_viableII.dat");
            System.IO.StreamWriter III = new System.IO.StreamWriter(hero + "_viableIII.dat");

            // Write statistics delimiter
            I.WriteLine("@");
            II.WriteLine("@");
            III.WriteLine("@");

            I.WriteLine("Sum:");
            sumI.writeIntoFile(I, true);
            I.WriteLine("----------------------------------------------------------------------");
            I.WriteLine("Variance:");
            varianceI.writeIntoFile(I, true);
            I.WriteLine("----------------------------------------------------------------------");
            I.WriteLine("Mean:");
            meanI.writeIntoFile(I, true);
            I.WriteLine("----------------------------------------------------------------------");
            I.WriteLine("Median:");
            medianI.writeIntoFile(I, true);
            I.WriteLine("----------------------------------------------------------------------");
            I.WriteLine("Deviation:");
            deviationI.writeIntoFile(I, true);
            I.WriteLine("----------------------------------------------------------------------");
            I.WriteLine("Max:");
            maxI.writeIntoFile(I, true);
            I.WriteLine("----------------------------------------------------------------------");
            I.WriteLine("Min:");
            minI.writeIntoFile(I, true);
            I.WriteLine("----------------------------------------------------------------------");

            II.WriteLine("Sum:");
            sumII.writeIntoFile(II, true);
            II.WriteLine("----------------------------------------------------------------------");
            II.WriteLine("Variance:");
            varianceII.writeIntoFile(II, true);
            II.WriteLine("----------------------------------------------------------------------");
            II.WriteLine("Mean:");
            meanII.writeIntoFile(II, true);
            II.WriteLine("----------------------------------------------------------------------");
            II.WriteLine("Median:");
            medianII.writeIntoFile(II, true);
            II.WriteLine("----------------------------------------------------------------------");
            II.WriteLine("Deviation:");
            deviationII.writeIntoFile(II, true);
            II.WriteLine("----------------------------------------------------------------------");
            II.WriteLine("Max:");
            maxII.writeIntoFile(II, true);
            II.WriteLine("----------------------------------------------------------------------");
            II.WriteLine("Min:");
            minII.writeIntoFile(II, true);
            II.WriteLine("----------------------------------------------------------------------");


            III.WriteLine("Sum:");
            sumIII.writeIntoFile(III, true);
            III.WriteLine("----------------------------------------------------------------------");
            III.WriteLine("Variance:");
            varianceIII.writeIntoFile(III, true);
            III.WriteLine("----------------------------------------------------------------------");
            III.WriteLine("Mean:");
            meanIII.writeIntoFile(III, true);
            III.WriteLine("----------------------------------------------------------------------");
            III.WriteLine("Median:");
            medianIII.writeIntoFile(III, true);
            III.WriteLine("----------------------------------------------------------------------");
            III.WriteLine("Deviation:");
            deviationIII.writeIntoFile(III, true);
            III.WriteLine("----------------------------------------------------------------------");
            III.WriteLine("Max:");
            maxIII.writeIntoFile(III, true);
            III.WriteLine("----------------------------------------------------------------------");
            III.WriteLine("Min:");
            minIII.writeIntoFile(III, true);
            III.WriteLine("----------------------------------------------------------------------");

            I.Close();
            II.Close();
            III.Close();
        }

        void recalibration(HSCardOperator op, String hero,
            double thresHold,
            int seedFrom,
            System.IO.StreamWriter meanFitness,
            System.IO.StreamWriter meanEfficiency,
            System.IO.StreamWriter meanSurprise,
            System.IO.StreamWriter meanCreativity)
        {
            List<result> resI;
            List<result> resII;
            List<result> resIII;
            double maxEff, minEff, maxSurp, minSurp;

            maxSurp = 0.0;
            minSurp = double.MaxValue;
            maxEff = 0.0;
            minEff = double.MaxValue;

            recalibrateResults(hero + "_BACKresults_I.dat", out resI, ref maxSurp, ref minSurp, ref maxEff, ref minEff, 0);
            recalibrateResults(hero + "_BACKresults_II.dat", out resII, ref maxSurp, ref minSurp, ref maxEff, ref minEff, 0);
            recalibrateResults(hero + "_BACKresults_III.dat", out resIII, ref maxSurp, ref minSurp, ref maxEff, ref minEff, 0);

            recalculateCreativityAndNormalization(op, resI, maxSurp, minSurp, maxEff, minEff);
            result sumI, varianceI, meanI, medianI, deviationI, maxI, minI;
            calculateStatistics(resI, out sumI, out meanI, out varianceI, out deviationI, out medianI, out maxI, out minI);

            recalculateCreativityAndNormalization(op, resII, maxSurp, minSurp, maxEff, minEff);
            result sumII, varianceII, meanII, medianII, deviationII, maxII, minII;
            calculateStatistics(resII, out sumII, out meanII, out varianceII, out deviationII, out medianII, out maxII, out minII);

            recalculateCreativityAndNormalization(op, resIII, maxSurp, minSurp, maxEff, minEff);
            result sumIII, varianceIII, meanIII, medianIII, deviationIII, maxIII, minIII;
            calculateStatistics(resIII, out sumIII, out meanIII, out varianceIII, out deviationIII, out medianIII, out maxIII, out minIII);

            System.IO.StreamWriter I = new System.IO.StreamWriter(hero + "_results_I_RENORMALIZED_REVISED.dat");
            System.IO.StreamWriter II = new System.IO.StreamWriter(hero + "_results_II_RENORMALIZED_REVISED.dat");
            System.IO.StreamWriter III = new System.IO.StreamWriter(hero + "_results_III_RENORMALIZED_REVISED.dat");

            foreach (result r in resI)
                r.writeIntoFile(I);

            foreach (result r in resII)
                r.writeIntoFile(II);

            foreach (result r in resIII)
                r.writeIntoFile(III);

            //To plot
            //writeForBoxPlot(hero, resI, resIII, meanFitness, meanEfficiency, meanSurprise, meanCreativity);
            //writeForPlot(hero, deviationI, deviationII, deviationIII, meanFitness, meanEfficiency, meanSurprise, meanCreativity);

            // Write statistics delimiter
            I.WriteLine("@");
            II.WriteLine("@");
            III.WriteLine("@");

            I.WriteLine("Sum:");
            sumI.writeIntoFile(I, true);
            I.WriteLine("----------------------------------------------------------------------");
            I.WriteLine("Variance:");
            varianceI.writeIntoFile(I, true);
            I.WriteLine("----------------------------------------------------------------------");
            I.WriteLine("Mean:");
            meanI.writeIntoFile(I, true);
            I.WriteLine("----------------------------------------------------------------------");
            I.WriteLine("Median:");
            medianI.writeIntoFile(I, true);
            I.WriteLine("----------------------------------------------------------------------");
            I.WriteLine("Deviation:");
            deviationI.writeIntoFile(I, true);
            I.WriteLine("----------------------------------------------------------------------");
            I.WriteLine("Max:");
            maxI.writeIntoFile(I, true);
            I.WriteLine("----------------------------------------------------------------------");
            I.WriteLine("Min:");
            minI.writeIntoFile(I, true);
            I.WriteLine("----------------------------------------------------------------------");

            II.WriteLine("Sum:");
            sumII.writeIntoFile(II, true);
            II.WriteLine("----------------------------------------------------------------------");
            II.WriteLine("Variance:");
            varianceII.writeIntoFile(II, true);
            II.WriteLine("----------------------------------------------------------------------");
            II.WriteLine("Mean:");
            meanII.writeIntoFile(II, true);
            II.WriteLine("----------------------------------------------------------------------");
            II.WriteLine("Median:");
            medianII.writeIntoFile(II, true);
            II.WriteLine("----------------------------------------------------------------------");
            II.WriteLine("Deviation:");
            deviationII.writeIntoFile(II, true);
            II.WriteLine("----------------------------------------------------------------------");
            II.WriteLine("Max:");
            maxII.writeIntoFile(II, true);
            II.WriteLine("----------------------------------------------------------------------");
            II.WriteLine("Min:");
            minII.writeIntoFile(II, true);
            II.WriteLine("----------------------------------------------------------------------");


            III.WriteLine("Sum:");
            sumIII.writeIntoFile(III, true);
            III.WriteLine("----------------------------------------------------------------------");
            III.WriteLine("Variance:");
            varianceIII.writeIntoFile(III, true);
            III.WriteLine("----------------------------------------------------------------------");
            III.WriteLine("Mean:");
            meanIII.writeIntoFile(III, true);
            III.WriteLine("----------------------------------------------------------------------");
            III.WriteLine("Median:");
            medianIII.writeIntoFile(III, true);
            III.WriteLine("----------------------------------------------------------------------");
            III.WriteLine("Deviation:");
            deviationIII.writeIntoFile(III, true);
            III.WriteLine("----------------------------------------------------------------------");
            III.WriteLine("Max:");
            maxIII.writeIntoFile(III, true);
            III.WriteLine("----------------------------------------------------------------------");
            III.WriteLine("Min:");
            minIII.writeIntoFile(III, true);
            III.WriteLine("----------------------------------------------------------------------");

            I.Close();
            II.Close();
            III.Close();
        }

        List<String> FUNCAO_PARA_O_HUGO(
            HSCardsParser liteParser,
            HSCardsParser fullParser,
            HSCombosParser fullCParser,
            HoningStoneBuilder HSBuilder,
            Dictionary<String, CardObject> cartTable,
            HSCardOperator cardOperator,
            Dictionary<String, int> dataID,
            Dictionary<String, double[]> cardsDataSet,
            HSHoningBayes fixedBayes,
            KNNEfficiency fixedKNN,
            String hero,
            String seed)
        {
            List<String> combo = new List<string>();

            HoningNetwork<String> net = new HoningNetwork<string>();

            List<CardObject> heroCards = liteParser.objects[hero];
            List<CardObject> neutralCards = liteParser.objects["Neutral"];

            HSBuilder.PopulateFromCardData(ref net, ref heroCards);
            HSBuilder.BuildThirdLevel(ref net, ref heroCards);
            HSBuilder.PopulateFromCardData(ref net, ref neutralCards);
            HSBuilder.BuildThirdLevel(ref net, ref neutralCards);

            fixedBayes = new HSHoningBayes(hero.ToLower(), ref fullCParser, ref net, ref cardsDataSet, ref cartTable, 100000);
            Dataset fixedDataset = new Dataset(hero + "_data.dataset", ',');
            fixedKNN = new KNNEfficiency(fixedDataset);

            Dictionary<String, String> selfAbilityFilter = cardOperator.GenerateAbilityFilter();

            int mana = 10;
            double surprise;
            double efficiency;
            double fitness;
            double normSurprise;
            double normEfficiency;
            double highSurp = 0.0;
            double minSurp = double.MaxValue;
            double highEff = 0.0;
            double minEff = double.MaxValue;

            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine("-                       Loading calibrated surprise!                 -");

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

            seed = seed.ToLower();

            combo = cardOperator.GenerateCardCluster(
                                seed,
                                ref cartTable,
                                ref net,
                                ref selfAbilityFilter,
                                ref fixedBayes,
                                ref fixedKNN,
                                ref cardsDataSet,
                                mana,
                                5,
                                80,
                                true,
                                ref highSurp,
                                ref minSurp,
                                ref highEff,
                                ref minEff,
                                out fitness,
                                out surprise,
                                out efficiency,
                                out normSurprise,
                                out normEfficiency).Keys.ToList();

            return combo;
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

            String currentDirectory = "results";

            bool exists = System.IO.Directory.Exists(currentDirectory);

            if (!exists)
                System.IO.Directory.CreateDirectory(currentDirectory);

            // boxplot files
            System.IO.StreamWriter Fitness = new System.IO.StreamWriter(currentDirectory + "//fit.txt");
            System.IO.StreamWriter Efficiency = new System.IO.StreamWriter(currentDirectory + "//eff.txt");
            System.IO.StreamWriter Surprise = new System.IO.StreamWriter(currentDirectory + "//sur.txt");
            System.IO.StreamWriter Creativity = new System.IO.StreamWriter(currentDirectory + "//crea.txt");
            System.IO.StreamWriter fileNormEfficiency = new System.IO.StreamWriter(currentDirectory + "//norm_eff.txt");
            System.IO.StreamWriter fileNormSurprise = new System.IO.StreamWriter(currentDirectory + "//norm_sur.txt");
            System.IO.StreamWriter fileNormCreativity = new System.IO.StreamWriter(currentDirectory + "//norm_crea.txt");

            Fitness.WriteLine("Hero Algorithm Value");
            Efficiency.WriteLine("Hero Algorithm Value");
            Surprise.WriteLine("Hero Algorithm Value");
            Creativity.WriteLine("Hero Algorithm Value");
            fileNormEfficiency.WriteLine("Hero Algorithm Value");
            fileNormSurprise.WriteLine("Hero Algorithm Value");
            fileNormCreativity.WriteLine("Hero Algorithm Value");

            foreach (String hero in parser.objects.Keys)
            {
                //if (hero == "Mage" || hero == "Warrior" || hero == "Shaman" || hero == "Priest" || hero == "Warlock" || hero == "Paladin" || hero == "Rogue" || hero == "Druid" || hero == "Hunter" || hero == "Neutral")
                if (hero == "Neutral")
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
                Dictionary<String, String> TerminalsDic = op.GetComboPotential(ref dic, ref cardTable, ref selfAbilityFilter, 10);
                List<String> Terminals = TerminalsDic.Keys.ToList();

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

                Console.WriteLine("----------------------------------------------------------------------");
                Console.WriteLine("-                       Loading calibrated surprise!                 -");

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

                int totalTestsPerSeed = 5;

                List<result> listresII = new List<result>();
                List<result> listresI = new List<result>();

                int seedCount = 1;
                foreach (String c in Terminals)
                {
                    Console.WriteLine("----------------------------------------------------------------------");

                    Console.WriteLine("Hero: " + hero);
                    Console.WriteLine("Seed: " + c);
                    Console.WriteLine("Seed " + seedCount + " of " + Terminals.Count);

                    Console.WriteLine();

                    // Test all reacheable seeds
                    seed = c;

                    for (int i = 0; i < totalTestsPerSeed; i++)
                    {
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
                            80,
                            true,
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

                        result res = new result(honingCombo, 0.0, creativity, 0.0, surprise, 0.0, efficiency, 0.0, highSurp, minSurp, highEff, minEff);
                        listresII.Add(res);

                        // (i)totalmente aleatorio (sem honing)
                        int totalMana = 0;
                        honingCombo = new List<string>();
                        honingCombo.Add(seed.ToLower());
                        int manaCost = 0;
                        Int32.TryParse(cardTable[seed.ToLower()].cost, out manaCost);
                        totalMana += manaCost;
                        while (totalMana < mana)
                        {
                            int randNode = rand.Next(Terminals.Count);
                            Int32.TryParse(cardTable[Terminals[randNode]].cost, out manaCost);
                            if (manaCost + totalMana <= mana)
                            {
                                honingCombo.Add(Terminals[randNode]);
                                totalMana += manaCost;
                            }
                        }

                        // Calculate surprise
                        double[] surpriseVec;
                        fixedBayes.CalculateSurprise(ref honingCombo, 1, out surpriseVec, ref cardDatasetFull, out surprise, false);

                        Double[] comboArray;
                        fixedBayes.GenerateComboVector(ref honingCombo, ref cardDatasetFull, out comboArray);
                        Instance target = new Instance(comboArray);

                        efficiency = fixedKNN.getKNearestWinrates(target, k);

                        creativity = surprise + efficiency;
                        // normCreativity = normSurprise + normEfficiency;

                        res = new result(honingCombo, 0.0, creativity, 0.0, surprise, 0.0, efficiency, 0.0, highSurp, minSurp, highEff, minEff);
                        listresI.Add(res);
                    }
                    seedCount++;
                }
                seedCount = 0;

                // To write results
                System.IO.StreamWriter resultFileI = new System.IO.StreamWriter(currentDirectory + "//" + hero + "_results_I" + ".dat");
                System.IO.StreamWriter resultFileII = new System.IO.StreamWriter(currentDirectory + "//" + hero + "_results_II" + ".dat");

                recalculateLimits(listresII, ref highSurp, ref minSurp, ref minEff, ref highEff);
                recalculateLimits(listresI, ref highSurp, ref minSurp, ref minEff, ref highEff);

                List<result> newresultII = renormalizeSimple(listresII, ref highSurp, ref minSurp, ref minEff, ref highEff);
                List<result> newresultI = renormalizeSimple(listresI, ref highSurp, ref minSurp, ref minEff, ref highEff);

                //extractViableSeeds(op, hero, 0, 1, meanFitness, meanEfficiency, meanSurprise, meanCreativity);
                writeForBoxPlot(hero, newresultI, "I", Fitness, Efficiency, Surprise, Creativity, fileNormEfficiency, fileNormSurprise, fileNormCreativity);
                writeForBoxPlot(hero, newresultII, "II", Fitness, Efficiency, Surprise, Creativity, fileNormEfficiency, fileNormSurprise, fileNormCreativity);

                // Write to file
                foreach (result r in newresultI)
                    r.writeIntoFile(resultFileI);

                // Write to file
                foreach (result r in newresultII)
                    r.writeIntoFile(resultFileII);

                resultFileI.Close();
                resultFileII.Close();
            }// end hero

            // boxplot closing
            Fitness.Close();
            Efficiency.Close();
            Surprise.Close();
            Creativity.Close();
            fileNormCreativity.Close();
            fileNormEfficiency.Close();
            fileNormSurprise.Close();
        }

        double normalize(double value, double max, double min)
        {
            double ret = (value - min) / (max - min);
            return ret;
        }

        double fitness(double normSurprise, double normEfficiency)
        {
            double dist = Math.Abs(normSurprise - normEfficiency);
            double subTerm = dist;

            double fitness = (normSurprise + normEfficiency + 1) - Math.Sqrt(subTerm + 1);

            return fitness;
        }

        void recalculateLimits(List<result> reslist, ref double maxs, ref double mins, ref double minef, ref double maxef)
        {
            foreach (result r in reslist)
            {
                if (r.surprise > maxs)
                    maxs = r.surprise;
                if (r.surprise < mins)
                    mins = r.surprise;
                if (r.efficiency > maxef)
                    maxef = r.efficiency;
                if (r.efficiency < minef)
                    minef = r.efficiency;
            }
        }

        List<result> renormalizeSimple(List<result> reslist, ref double maxs, ref double mins, ref double minef, ref double maxef)
        {
            List<result> newresults = new List<result>();

            foreach (result r in reslist)
            {
                result nr = new result();

                double normSurprise = normalize(r.surprise, maxs, mins);
                double normEfficiency = normalize(r.efficiency, maxef, minef);
                double fit = fitness(normSurprise, normEfficiency);

                nr.highEff = maxef;
                nr.minEff = minef;
                nr.highSurp = maxs;
                nr.minSurp = mins;
                nr.surprise = r.surprise;
                nr.efficiency = r.efficiency;
                nr.creativity = nr.surprise + nr.efficiency;
                nr.fitness = fit;
                nr.normSurprise = normSurprise;
                nr.normEfficiency = normEfficiency;
                nr.normCreativity = nr.normSurprise + nr.normEfficiency;

                foreach (String cards in r.cards)
                    nr.cards.Add(cards);

                newresults.Add(nr);
            }

            return newresults;
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
            double a, b, c, d, e, f;
            HS_ARS_WEB h = new HS_ARS_WEB();
            h.Instantiate("jsonWithTypes.json", "ALYCOMBOOK.json", "allCardsWithAbility.json");
            List<String> combo = h.getCombo("healing touch", "Druid", 8, 20, 10, out a, out b, out c, out e, out f);
            
            //Program prog = new Program();
            //HSDecksParser p = new HSDecksParser("decks_winrate_30_cars_craft_wins_losses_winrate.json");

            /*if (args.Length < 3)
            {
                prog.ValidationTests("jsonWithTypes.json", "ALYCOMBOOK.json", 5);
            }
            else
            {
                prog.argTest(args);
            }*/
        }
    }
}
