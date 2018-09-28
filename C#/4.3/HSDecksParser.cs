using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SharpHoning
{
    class HSDeckInfo
    {
        public HSDeckInfo()
        {
            winrate = "0.0";
            cards = new List<String>();
        }

        public String winrate;
        public String deckName;
        public String hero;
        public List<String> cards;
    }

    class HSDecksParser
    {
        public HSDecksParser(String path)
        {
            decks_by_hero = new Dictionary<string, List<HSDeckInfo>>();

            // Le arquivo json e retorna string inteira da rede
            String json = System.IO.File.ReadAllText(path);

            // Ler json com dados da rede
            dynamic stuff = JArray.Parse(json);

            foreach(dynamic element in stuff){
                String deckName = element.name;
                String deckHero = element.hero;
                String winRate  = element.winrate;

                HSDeckInfo herosCombos = new HSDeckInfo();
                herosCombos.winrate  = winRate;
                herosCombos.deckName = deckName;
                herosCombos.hero     = deckHero;

                foreach (dynamic card in element.cards)
                {
                    String cardName     = card.name;
                    String cardQuantity = card.quantity;

                    herosCombos.cards.Add(cardName.ToLower());
                } // end dynamic cards

                if (!decks_by_hero.ContainsKey(deckHero.ToLower()))
                    decks_by_hero.Add(deckHero.ToLower(), new List<HSDeckInfo>());

                decks_by_hero[deckHero.ToLower()].Add(herosCombos);
            } // end dynamic elements
        }// end method

        public Dictionary<String, List<HSDeckInfo>> decks_by_hero;
    }
}
