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