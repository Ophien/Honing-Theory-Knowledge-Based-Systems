using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNN
{
    public class Instance: IComparable<Instance>
    {
        public Double[] attributes;
        public Double distance;

        public void FromArray(Double[] array){
            attributes = array;
        }
                            
        /// <summary>
        /// Constructor - Create a empty Instance
        /// </summary>
        public Instance()
        {
        }

        public Instance(Double[] array)
        {
            FromArray(array);
        }

        public double getLast()
        {
            return attributes[attributes.Length-1];
        }


        /// <summary>
        /// Gets the data from Instance
        /// </summary>
        /// <returns></returns>
        private Double[] getData()
        {
            return attributes;
        }

        /// <summary>
        /// Returns the number of attributes
        /// </summary>
        /// <returns></returns>
        private int numberOfAtribute()
        {
            return attributes.Length;
        }

         /// <summary>
        /// Return the Euclidian distance from another instance
        /// </summary>
        /// <param name="instance"></param>
        public double distanceFrom(Instance instance)
        {
            Double squaredDifference = 0.0;
            for (int i = 0; i < instance.attributes.Length-1; i++)
            {
                squaredDifference += Math.Pow(attributes[i] - instance.attributes[i], 2);
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