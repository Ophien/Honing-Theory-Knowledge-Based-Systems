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
using Newtonsoft.Json.Linq;

namespace SharpHoning
{
    class HoningStoneBuilder
    {
        public String ReadHoningStone(String textFile)
        {
            // Le arquivo json e retorna string inteira da rede
            return System.IO.File.ReadAllText(textFile);
        }

        public List<List<String>> PopulateCombosList(String file_path)
        {
            String json_network = ReadHoningStone(file_path);

            // Popula e retorna rede pronta para uso
            return ComboReader(json_network);
        }

        public void GenCardTableObject(ref List<CardObject> objs, ref Dictionary<String, CardObject> dic)
        {
            foreach (CardObject o in objs)
            {
                if (!dic.ContainsKey(o.name))
                    dic.Add(o.name, o);
            }
        }

        public List<List<String>> ComboReader(String jsonString)
        {
            List<List<String>> ret = new List<List<String>>();

            // Ler json com dados da rede
            dynamic stuff = JArray.Parse(jsonString);

            // Popular rede honing
            foreach (dynamic obj in stuff)
            {
                List<String> new_combo = new List<String>();
                foreach (String mf in obj.combo)
                {
                    // Cria mf internal node para rede simples
                    new_combo.Add(mf);
                }
                if(new_combo.Count > 1)
                    ret.Add(new_combo);
            }

            return ret;
        }

        public void PopulateFromCardData(ref HoningNetwork<String> net, ref List<CardObject> cardData)
        {
            foreach (CardObject card in cardData)
            {
                net.insert(card.name,"terminal");

                net.insert(card.type, "essential");

                net.setParent(card.name, card.type);

                if (card.race != null)
                {
                    net.insert(card.race, "essential");
                    net.setParent(card.name, card.race);
                }

                foreach (CardAbility ab in card.abilities)
                {
                    net.insert(ab.ability, "internal");
                    net.setParent(card.name, ab.ability);
                }
            }
        }

        public void BuildThirdLevel(ref HoningNetwork<String> net, ref List<CardObject> cardData)
        {
            //config targets
            foreach (CardObject card in cardData)
            {
                foreach (CardAbility ab in card.abilities)
                {
                    foreach (String target in ab.target)
                    {
                        net.setParent(ab.ability, target);
                    }
                }
            }

        }

        public HoningNetwork<String> BuildNetwork(String jsonString)
        {
            // Cria rede simples vazia
            HoningNetwork<String> new_network = new HoningNetwork<String>();

            // Ler json com dados da rede
            dynamic stuff = JArray.Parse(jsonString);

            // Popular rede honing
            foreach(dynamic obj in stuff){
                // Persistir carta na rede
                new_network.insert((String)obj.Card_name);

                // Popular microfeatures na rede
                foreach (String mf in obj.Microfeatures)
                {
                    // Cria mf internal node para rede simples
                    new_network.insert(mf.ToLower());
                    
                    // Inserir na rede para manipulacao
                    new_network.setParent((String)obj.Card_name, mf.ToLower());
                }
            }

            // Apenas retorno do objeto
            return new_network;
        }
    } // Fim HoningStoneBuilder
}
