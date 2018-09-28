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
