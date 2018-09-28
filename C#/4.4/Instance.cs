using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNN
{
    public class Instance: IComparable<Instance>
    {
        List<Double> attributes;
        Double distance;

        public void FromArray(Double[] array){
            foreach(double d in array)
                attributes.Add(d);
        }
                            
        /// <summary>
        /// Constructor - Create a empty Instance
        /// </summary>
        public Instance()
        {
           attributes = new List<Double>();
        }

        public Instance(Double[] array)
        {
            attributes = new List<Double>();
            FromArray(array);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Instance(List<Double> instanceData)
        {
            attributes = instanceData;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Instance(String [] instanceData)
        {
            attributes = new List<Double>();
            foreach (String data in instanceData)
            {
                attributes.Add(Double.Parse(data));
            }
        }

        public double getLast()
        {
            return attributes.Last();
        }


        /// <summary>
        /// Gets the data from Instance
        /// </summary>
        /// <returns></returns>
        private List<Double> getData()
        {
            return attributes;
        }

        /// <summary>
        /// Returns the number of attributes
        /// </summary>
        /// <returns></returns>
        private int numberOfAtribute()
        {
            return attributes.Count;
        }

         /// <summary>
        /// Return the Euclidian distance from another instance
        /// </summary>
        /// <param name="instance"></param>
        public double distanceFrom(Instance instance)
        {
            List<Double> instanceData = instance.getData();
            int numberOfAttributes = numberOfAtribute();
            Double squaredDifference = 0.0;
            for (int i = 0; i < numberOfAttributes-1; i++)
            {
                squaredDifference += Math.Pow(attributes.ElementAt(i) - instanceData.ElementAt(i), 2);
            }
            distance = Math.Sqrt(squaredDifference);
            return distance;
        }

        public int CompareTo(Instance instance)
        {
            if (this.distance > instance.distance)
            {
                return 1;
            }
            else if (this.distance < instance.distance)
            {
                return -1;
            }
            return 0;

        }
    }

   
}