using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpHoning
{
    class HoningWebFunctions
    {


        public HoningNetwork<String> populateNetwork(String file_path)
        {
            // Configura builder
            HoningStoneBuilder builder = new HoningStoneBuilder();
            
            // Apenas leitura do json
            String json_network        = builder.ReadHoningStone(file_path);
            
            // Popula e retorna rede pronta para uso
            return builder.BuildNetwork(json_network);
        }

        public void seek_by_card(HoningNetwork<String> net, String card_name, int return_count, float threshold, out List<String> result)
        {
            // Retorno padr'ao
            result = null;

            if (return_count <= 0)
            {
                return;
            }

            // Preparar output
            Dictionary<String, HoningNode<String>> output;
            Dictionary<String, HoningNode<String>> output_spell;
            Dictionary<String, HoningNode<String>> output_others;

            // Resultados para uso no site
            result = new List<String>();

            // Preparar entradas da rede de honing
            HoningNode<String> working_node = net.getHoningNode(card_name);
            List<String>       bridges      = new List<String>();

            if (working_node == null)
            {
                return;
            }

            foreach (KeyValuePair<String, HoningNode<String>> key_pair in working_node.clique)
            {
                bridges.Add(key_pair.Value.holder);
            }

            List<String> spellList  = new List<string>();
            List<String> minionList = new List<string>();
            spellList .Add("spell");
            minionList.Add("minion");

            // Recrutamento de neurds
            net.recruitNeurds(bridges, out output);//, spellList);
            //net.recruitNeurds(bridges, out output_spell, minionList);

            net.keepWithMicrofeature(ref output, out output_others, "minion");
            net.keepWithMicrofeature(ref output, out output_spell , "spell");

           

            // Verifica validade do threshold
            if (threshold > 1.0f)
                threshold = 1.0f;

            Random rand_obj = new Random();

            HoningNode<String> node = net.getHoningNode("Malygos");

           
            

            // Prepara'cao final de output para o site
            while (return_count > 0)
            {
                foreach (KeyValuePair<String, HoningNode<String>> key_pair in output)
                {
                    float rand_prob = (float)rand_obj.NextDouble();

                    if (rand_prob >= threshold)
                    {
                        result.Add(key_pair.Key);
                        return_count--;
                        break;
                    }
                }// Fim foreach
            } // Fim while*/
        }// Fim seek_by_card
    } // Fim HoningWebFunctions
} // Fim SharpHoning
