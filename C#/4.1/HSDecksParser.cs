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
