using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KNN
{
    public class Dataset
    {
        List<Instance> instances;

        public Dataset(String fileName, char delimiter)
        {
            instances = new List<Instance>();
            String line;
            Instance instance;
            try
            {

                using (StreamReader sr = new StreamReader(fileName))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        //Console.WriteLine(line);
                        String[] data = line.Split(delimiter);
                        instance = new Instance(data);
                        instances.Add(instance);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
           
        }


        /// <summary>
        /// Return all Instance
        /// </summary>
        /// <returns></returns>
        public List<Instance> getInstances()
        {
            return instances;
        }

        public List<Instance> getFirstK(int k)
        {
            List<Instance> firstk = new List<Instance>();
            for (int i = 0; i < k; i++)
            {
                firstk.Add(instances.ElementAt(i));
            }

            return firstk;
        }


        /// <summary>
        /// Sorts the dataset using distance value from a Instance target
        /// </summary>
        public void sort()
        {
            instances.Sort();
        }


        
        /// <summary>
        /// Gets the first Instance
        /// </summary>
        /// <returns></returns>
        public Instance getFirst()
        {
            return instances.First();
        }
    }
}
