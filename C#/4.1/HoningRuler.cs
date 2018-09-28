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
    class HoningRuler<T>
    {
        public HoningRuler()
        {
            id_mapping = new Dictionary<T, int>();
            inverse_id_mapping = new Dictionary<int, T>();
        }

        public void BuildRuler(ref HoningNetwork<T> net)
        {
            int single_mf_count = net.leafCount();

            ruler = new float[single_mf_count][];
            for (int i = 0; i < single_mf_count; i++)
            {
                ruler[i] = new float[single_mf_count];
                for (int j = 0; j < single_mf_count; j++)
                    ruler[i][j] = 0.0f;
            }

            net.leafIDMapping(out id_mapping, out inverse_id_mapping);
            total_internal_nodes = single_mf_count;
        }

        public int getRulerID(T key)
        {
            return id_mapping[key];
        }

        public T getInverseID(int id)
        {
            return inverse_id_mapping[id];
        }

        public void printRuler()
        {
            string lines = "";
            System.IO.StreamWriter file = new System.IO.StreamWriter("ruler_net");

            for (int i = 0; i < total_internal_nodes; i++)
            {
                for (int j = 0; j < total_internal_nodes; j++)
                    lines += "\t" + String.Format("  {0:F2}", ruler[i][j]);
                lines += "\r\n";
            }

            file.WriteLine(lines);

            file.Close();
        }

        public void reinforceNet(T seeker, ref HoningNetwork<T> net)
        {
            HoningNode<T> node = net.getHoningNode(seeker);
            if (node == null)
                return;

            int clique_size = node.clique.Count;

            foreach(T k_i in node.clique.Keys)
            {
                foreach (T K_j in node.clique.Keys)
                {
                    int i = getRulerID(k_i);
                    int j = getRulerID(K_j);
                    ruler[i][j] += clique_size / total_internal_nodes;
                }
            }
        }

        public float total_internal_nodes;

        public float[][] ruler;

        Dictionary<T, int> id_mapping;
        Dictionary<int, T> inverse_id_mapping;
    }
}
