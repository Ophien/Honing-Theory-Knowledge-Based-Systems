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
