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
    class HoningNode<T>
    {
        public HoningNode()
        {
            clique = new Dictionary<T, HoningNode<T>>();
            parents = new Dictionary<T, HoningNode<T>>();
        }

        public HoningNode(T holder)
        {
            clique = new Dictionary<T, HoningNode<T>>();
            parents = new Dictionary<T, HoningNode<T>>();
            this.holder = holder;
        }

        public Dictionary<T, HoningNode<T>> clique;
        public Dictionary<T, HoningNode<T>> parents;
        public T holder;
    }

    class HoningNetwork<T>
    {
        public HoningNetwork()
        {
            network = new Dictionary<T, HoningNode<T>>();
        }

        public bool checkMicrofeature(T seed, T microfeature)
        {
            bool check = false;

            if (!network.ContainsKey(seed))
                return check;

            HoningNode<T> node = network[seed];

            if (!node.clique.ContainsKey(microfeature))
                return check;

            return check = true;
        }

        public bool match_vector(List<T> A, List<T> B)
        {
            bool check = false;

            if (A.Count != B.Count)
            {
                return check;
            }

            for (int i = 0; i < A.Count; i++)
            {
                check = false;

                for (int j = 0; j < B.Count; j++)
                {
                    if(A[i].Equals(B[j]))
                    {
                        check = true; break;
                    }
                }

                if (!check)
                    return check;
            }

            return check;
        }

        public void insert(T obj)
        {
            if (!network.ContainsKey(obj))
            {
                network.Add(obj, new HoningNode<T>(obj));
            }
        }

        public void remove(T obj)
        {
            if (network.ContainsKey(obj))
            {
                foreach(T key in network[obj].parents.Keys)
                {
                    network[key].clique.Remove(obj);
                }

                foreach (T key in network[obj].clique.Keys)
                {
                    network[key].parents.Remove(obj);
                }

                network.Remove(obj);
            }
        }

        public void setParent(T seed, T clique_obj)
        {
            if (network.ContainsKey(seed) && network.ContainsKey(clique_obj))
            {
                HoningNode<T> node = network[seed];
                if (!node.clique.ContainsKey(clique_obj))
                {
                    HoningNode<T> cliqueNode = network[clique_obj];
                    node.clique.Add(clique_obj, cliqueNode);
                    cliqueNode.parents.Add(seed, node);
                }
            }
        }

        public int countMicrofeatures(List<T> baseVector)
        {
            int count = 0;

            foreach (T value in baseVector)
            {
                HoningNode<T> node = network[value];
                count += node.clique.Count;
            }

            return count;
        }

        public void recruitNeurds(List<T> bridges, Dictionary<T, HoningNode<T>> output)
        {
            foreach (T currentBridge in bridges)
            {
                foreach (KeyValuePair<T, HoningNode<T>> parent in network[currentBridge].parents)
                {
                    output.Add(parent.Key, parent.Value);
                }
            }
        }

        public void contextualFilter(List<T> filter, Dictionary<T, HoningNode<T>> input, Dictionary<T, HoningNode<T>> filtered_output)
        {
            foreach (T filter_objKey in filter)
            {
                if (input.ContainsKey(filter_objKey))
                {
                    filtered_output.Add(filter_objKey, input[filter_objKey]);
                }
            }
        }

        Dictionary<T, HoningNode<T>> network;
    }
}
