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
