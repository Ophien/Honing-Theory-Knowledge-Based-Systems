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

#include <map>
#include <vector>
#include <iostream>
#include <queue>
#include <cstdarg>
#include <stdio.h>      /* printf, scanf, puts, NULL */
#include <stdlib.h>     /* srand, rand */
#include <time.h>       /* time */
#include <algorithm>
#include <fstream>
#include <sstream>
#include <ctype.h>
#include "Jzon.h"

using namespace std;

template <class T, class F> class HoningEdge;
template <class T, class F> class HoningNode;
template <class T, class F> class HoningNetwork;
class att;
class textExtractorNode;

class textExtractorNode{
public:
	textExtractorNode(){}
	string ability, value, target;
};

class att{
public:
	att(){ mana = attack = mana = health = 0;}
	int mana, attack, health; string text; string original_name;
	map<string,string> mfs;
};

class dataExtractionObject{
	vector<string> article;
	vector<string> noun;
	vector<string> preposition;
	vector<string> adjective;
	vector<string> abbreviation;
	vector<string> acronym;
	vector<string> alliteration;
	vector<string> adverb;
	vector<string> anagram;
	vector<string> antonym;
	vector<string> compoundword;
	vector<string> contraction;
	vector<string> conjunction;
	vector<string> homephone;
	vector<string> interjection;
	vector<string> metaphor;
	vector<string> opposite;
	vector<string> palindrome;
	vector<string> pronoun;
	vector<string> propernoun;
	vector<string> proverb;
	vector<string> punctuationmark;
	vector<string> quete;
	vector<string> rhyme;
	vector<string> simile;
	vector<string> symbol;
	vector<string> synonym;
	vector<string> verb;
	vector<string> word;
};

template <class T, class F> class HoningEdge{
public:
	HoningEdge(){
	}
	HoningEdge(T value){
	}
	F value;
	HoningNode<T, F>* node;
};

template <class T, class F> class HoningNode{
public:
	HoningNode(){
	}
	HoningNode(T holder){
		this->holder = holder;
	}
	vector<HoningNode<T, F>*> clique_semantic_order;
	map<T, HoningEdge<T, F>*> clique;
	map<T, HoningNode<T, F>*> parents;
	T holder;
};

template <class T, class F> class HoningNetwork{
public:
	HoningNetwork(){
	}
	map<T, HoningNode<T, F>*> network;
};

//-------------------------------------------------------------------------------------------------------------------------

bool isValid(char c){
	if((48 <= c && c <= 57) || 
		(65 <= c && c <= 90) ||
		(97 <= c && c <= 122) ||
		c == '+' || 
		c == '/' || 
		c == '-')
		return true;
	return false;
}

template <class T, class F> bool checkNode(HoningNetwork<T,F>& net, T node){
	map<T, HoningNode<T,F>*>::iterator netIt;
	netIt = net.network.find(node);
	if(netIt == net.network.end())
		return false;
	return true;
}

template <class T, class F> bool checkParent(HoningNode<T,F>& node, T parent){
	map<T, HoningNode<T,F>*>::iterator nodeIt;
	nodeIt = node.parents.find(parent);
	if(nodeIt == node.parents.end())
		return false;
	return true;
}

template <class T, class F> bool checkClique(HoningNode<T,F>& node, T cliqueNode){
	map<T, HoningEdge<T,F>*>::iterator cliqueIt;
	cliqueIt = node.clique.find(cliqueNode);
	if(cliqueIt ==node.clique.end())
		return false;
	return true;
}

template<class T, class F> void insertClique(HoningNode<T,F>& node, HoningEdge<T,F>& edge){
	if(checkClique(node, edge.node))
		return;
	node.clique_semantic_order.push_back(edge.node);
	T cliqueKey = edge.node.holder;
	node.clique[cliqueKey] = node;
}

template<class T, class F> void insertParent(HoningNode<T,F>& node, HoningNode<T,F>& parent){
	if(checkParent(node, parent.holder))
		return;
	T cliqueKey             = parent.holder;
	node.parents[cliqueKey] = &parent;
}

template <class T, class F> HoningNode<T,F>* buildNode(HoningNetwork<T,F>& net, T holder)
{
	HoningNode<T,F>* node;
	if(!checkNode(net, holder)){
		node = new HoningNode<T,F>();
		node->holder = holder;
	}else
		node = net.network[holder];
	return node;
}

template <class T, class F> HoningEdge<T,F>* buildEdge(HoningNode<T,F>& node, F edgeValue){
	HoningEdge<T,F>* edge;
	T key = node.holder;
	if(!checkClique(node, key)){
		edge = new HoningEdge<T,F>();
		edge->value = edgeValue;
		edge->node  = &node;
	}else{
		edge = node.clique[key];
	}
	return edge;
}

template <class T, class F> void insertEdge(HoningNode<T,F>& node, HoningEdge<T,F>& edge, bool replace = false){
	if(replace){
		node.clique[edge.node->holder] = &edge;
		//TODO:deletar objeto que ja existe se enderço for diferente
	}else{
		if(!checkClique(node, edge.node->holder)){
			node.clique[edge.node->holder] = &edge;
			node.clique_semantic_order.push_back(edge.node);
		}
	}
}

template <class T, class F> void insert(HoningNetwork<T,F>& net, HoningNode<T,F>& node){
	if(!checkNode(net, node.holder)){
		T key				= node.holder;
		net.network[key]	= &node;
	}
}

template <class T, class F> void populateNetwork(map<T, att>& atts, HoningNetwork<T,F>& net)
{
	map<T, att>::iterator it = atts.begin();
	for(; it != atts.end(); it++)
	{
		att& currentAtt = (*it).second;
		string currentText = currentAtt.text;
		HoningNode<T,F>* node;
		node = buildNode(net, (*it).first);
		insert(net, *node);
		string cat	 = "";
		int catCount = 0;
		int state	 = 0;
		int i		 = 0;
		for(; i < currentText.length(); i++)
		{
			char readed = currentText.at(i);
			switch(state){
			case 0:
				if(readed == '<')
					state = 2;
				else
					if(isValid(readed)){
						cat  += readed;
						state = 1;
					}
					break;
			case 1:
				if(isValid(readed)){
					cat += readed;
				}else{
					transform(cat.begin(), cat.end(), cat.begin(), ::tolower);
					HoningNode<T,F>* mfNode = buildNode(net, cat);
					HoningEdge<T,F>* edge =	buildEdge(*mfNode, (F)catCount);
					insertEdge(*node, *edge, false);
					insertParent(*mfNode, *node);
					insert(net, *mfNode);
					currentAtt.mfs[mfNode->holder] = mfNode->holder;
					cat.clear();
					catCount++;
				}
				break;
			case 2:
				if(readed == '>')
					state = 0;
				break;
			}
		}
		transform(cat.begin(), cat.end(), cat.begin(), ::tolower);
		HoningNode<T,F>* mfNode = buildNode(net, cat);
		HoningEdge<T,F>* edge =	buildEdge(*mfNode, (F)catCount);
		insertEdge(*node, *edge, false);
		insertParent(*mfNode, *node);
		insert(net, *mfNode);
		currentAtt.mfs[mfNode->holder] = mfNode->holder;
	}
}

//-------------------------------------------------------------------------------------------------------------------------

int readCards(const char* file, map<string, att>& out_att){
	Jzon::Object rootNode;
	Jzon::FileReader::ReadFile(file, rootNode);
	for (Jzon::Object::iterator it = rootNode.begin(); it != rootNode.end(); ++it)
	{
		Jzon::Array insideObj = (*it).second.AsArray();
		Jzon::Array::iterator insideIt = insideObj.begin();
		for (; insideIt != insideObj.end(); insideIt++){
			Jzon::Object out = (*insideIt).AsObject();
			Jzon::Object::iterator obj_it = out.begin();
			att _att;
			string card_name;
			string type;
			for (; obj_it != out.end(); obj_it++){
				string value = (*obj_it).first;
				if (value == "name"){
					card_name = (*obj_it).second.AsValue().ToString();
				}
				if (value == "health"){
					_att.health = (*obj_it).second.AsValue().ToInt();
				}
				if (value == "attack"){
					_att.attack = (*obj_it).second.AsValue().ToInt();
				}
				if (value == "cost"){
					_att.mana = (*obj_it).second.AsValue().ToInt();
				}
				if(value == "text"){
					_att.text = (*obj_it).second.AsValue().ToString();
				}
				if(value == "type"){
					type = (*obj_it).second.AsValue().ToString();
					type.erase(remove_if(type.begin(), type.end(), isspace), type.end());
					transform(type.begin(), type.end(), type.begin(), ::tolower);
				}
			}//end for
			if(type == "hero")
				continue;
			_att.original_name = card_name;
			card_name.erase(remove_if(card_name.begin(), card_name.end(), isspace), card_name.end());
			transform(card_name.begin(), card_name.end(), card_name.begin(), ::toupper);
			//_att.mfs[type] = type;
			out_att[card_name] = _att;
		}
	}
	return 0;
}

template <class T> bool vectorSearch(T& type, vector<T>& vec){
	bool check = false;
	for(int i = 0; i < vec.size(); i ++){
		if(type == vec[i]){
			check = true;
			break;
		}
	}
	return check;
}

void textExtractor(att& att, vector<string>& output, int t, ...){
	string ret = "";
	va_list va;
	va_start(va, t);
	for(int i = 0 ; i < t; i++){
		string arg = (va_arg(va, char*));
		map<string,string>::iterator it = att.mfs.find(arg);
		if(it != att.mfs.end()){if(!vectorSearch(arg, output)) output.push_back(arg);}
	}
	va_end(va);
}

void textExtractor(att& att, vector<string>& output, vector<string>& var){
	for(unsigned int i = 0 ; i < var.size(); i++){
		string arg = var[i];
		map<string,string>::iterator it = att.mfs.find(arg);
		if(it != att.mfs.end()){if(!vectorSearch(arg, output))output.push_back(arg);}
	}
}

template <class T, class F> void getNumericMicrofeatures(HoningNetwork<T,F>& net, vector<string>& output){
	map<T, HoningNode<T,F>*>::iterator it =  net.network.begin();
	for(;it!=net.network.end(); it++){
		string to_search = (*it).first;
		for(int i = 0; i < to_search.length(); i++){
			if(48 <= to_search.at(i) && to_search.at(i) <= 57){output.push_back(to_search);break;}
		}
	}
}

template <class T, class F> bool microfeatureGroupANDCheck(HoningNode<T,F>& node, int args, ...){
	bool check = true;
	va_list v_list;
	va_start(v_list,args);
	for(int i = 0; i < args; i++){
		string argument = string(va_arg(v_list, char*));
		if(!checkClique(node, argument)){
			check = false;
			break;
		}
	}
	va_end(v_list);

	return check;
}

template <class T, class F> bool microfeatureGroupORCheck(HoningNode<T,F>& node, int args, ...){
	bool check = false;
	va_list v_list;
	va_start(v_list,args);
	for(int i = 0; i < args; i++){
		string argument = string(va_arg(v_list, char*));
		if(checkClique(node, argument)){
			check = true;
			break;
		}
	}
	va_end(v_list);

	return check;
}

template <class T> bool simpleORComparable(T comparable, int args, ...){
	bool check = false;
	va_list list;
	va_start(list, args);
	for(int i = 0; i < args; i++){
		string arg = string(va_arg(list,char*));
		if(comparable.compare(arg) == 0){
			check = true;
			break;
		}
	}
	va_end(list);

	return check;
}

class HSAbilityNode{
public:
	string ability, value, target;
};

void split(const string& ss, char delim, vector<string>& elems) {
    stringstream s(ss);
    string item;
    while (getline(s, item, delim)) {
        elems.push_back(item);
    }
}

void HSparseNumeric(string value, string attack, string health){
	vector<string> splited;
	split(value, '/', splited);
}

void HSabilityComplement(vector<string>& abilities, string ability){
	vector<string> elems;
	split(ability,'/',elems);
	if(elems.size() > 1){
		abilities.push_back("plusAttack");
		abilities.push_back("plusHealth");
	}
}

bool is_number(const std::string& s)
{
    return !s.empty() && std::find_if(s.begin(), 
        s.end(), [](char c) { return !isdigit(c); }) == s.end();
}

void getRealMF(string& mf, string& first, string& second){
	if(mf == "spellPower"){
		first = "spell";
		second = "power";
	}else
	if(mf == "dealDamage"){
		first = "deal";
		second = "damage";
	}else
	if(mf == "plusAttack"){
		first = "attack";
		second = "";
	}else
	if(mf == "loseDurability"){
		first = "lose";
		second = "durability";
	}else
	if(mf == "loseAttack"){
		first = "lose";
		second = "attack";
	}else
	if(mf == "loseHealth")
	{
		first = "lose";
		second = "health";
	}else
	if(mf == "plusHealth")
	{
		first = "health";
		second = "";
	}else
	if(mf == "plusDurability")
	{
		first = "durability";
		second = "";
	}else
	if(mf == "plusMana")
	{
		first = "mana";
		second = "";
	}else
	if(mf == "plusCost")
	{
		first = "cost";
		second = "";
	}else
	if(mf == "reduceCost")
	{
		first = "reduce";
		second = "cost";
	}if(mf == "megaWindfury"){
		first = "mega";
		second = "";
	}if(mf == "divineShield"){
		first = "divine";
		second = "";
	}if(mf == "cantAttack"){
		first = "cant";
		second = "";
	}else{
		first = mf;
		second = "";
	}
}

void HSGetTrueTarget(string& target, string& trueTarget){
		if(target == "minion"){
			trueTarget = "minion";
		}
		if(target == "alliedMinion"){
			trueTarget = "minion";
		}
		if(target == "nextMinions"){
			trueTarget = "next";
		}
		if(target == "allMinion"){
			trueTarget = "all";
		}
		if(target == "allAlliedMinions"){
			trueTarget = "allied";
		}
		if(target == "allEnemyMinions"){
			trueTarget = "enemy";
		}
		if(target == "hero"){
			trueTarget = "hero";
		}
		if(target == "enemyHero"){
			trueTarget = "enemy";
		}
		if(target == "alliedHero"){
			trueTarget = "allied";
		}
		if(target == "character"){
			trueTarget = "character";
		}
		if(target == "alliedCharacter"){
			trueTarget = "allied";
		}
		if(target == "allCharacter"){
			trueTarget = "all";
		}
		if(target == "allAlliedCharacters"){
			trueTarget = "allied";
		}
		if(target == "allEnemyCharacters"){
			trueTarget = "enemy";
		}
		if(target == "legendaryMinion"){
			trueTarget = "legendary";
		}
		if(target == "splitAll"){
			trueTarget = "all";
		}
		if(target == "splitAllEnemy"){
			trueTarget = "all";
		}
		if(target == "splitAllEnemyMinion"){
			trueTarget = "all";
		}
		if(target == "opponentsWeapon"){
			trueTarget = "opponents";
		}
		if(target == "ownWeapon"){
			trueTarget = "own";
		}
		if(target == "hand"){
			trueTarget = "hand";
		}
		if(target == "opponentsHand"){
			trueTarget = "opponents";
		}
		if(target == "ownHand"){
			trueTarget = "own";
		}
		if(target == "opponentsBattlefield"){
			trueTarget = "opponents";
		}
		if(target == "ownBattlefield"){
			trueTarget = "battlefield";
		}
		if(target == "ownManaCrystals"){
			trueTarget = "mana";
		}
		if(target == "opponentsManaCrystals"){
			trueTarget = "mana";
		}
		if(target == "opponentsDeck"){
			trueTarget = "deck";
		}
		if(target == "ownDeck"){
			trueTarget = "own";
		}
		if(target == "alliedHeroPower"){
			trueTarget = "hero";
		}
		if(target == "opponentsCard"){
			trueTarget = "opponents";
		}
}

template <class T, class F> HSAbilityNode createAbilityNode(HoningNode<T, F>& node, string ability, vector<string>& value, vector<string>& targets){
	HSAbilityNode new_node;
	new_node.ability = ability;
	string first, second;
	getRealMF(ability, first, second);
	if(targets.size() == 1){new_node.target = targets[0];}else
		if(checkClique(node, first) && checkClique(node, second)){
			int __ref_key = node.clique[second]->value;
			int __min_dist,_min_key;__min_dist=999999999;_min_key=999999999;
			int i = 0;
			for(; i < targets.size(); i++){
				string truetarget;HSGetTrueTarget(targets[i], truetarget);
				int __true_target_internal_key = node.clique[truetarget]->value;
				if(abs(__true_target_internal_key - __ref_key) < __min_dist){
					__min_dist = abs(__true_target_internal_key - __ref_key);_min_key = i;
				}
			}
			new_node.target = targets[_min_key];
		}
		if(simpleORComparable<string>(ability, 28, 
			"battlecry", "charge", "combo", "deathrattle", "enrage",
			"freeze", "inspire", "overload", "secret", "silence", 
			"stealth", "taunt", "control", "copy", "counter", "destroy", "discard", 
			"draw", "equip", "forgetful", "immune", "return", "summon", "swap", "transform",
			"divineShield", "megaWindfury", "cantAttack")){
				new_node.value = "1";}else 
		if(simpleORComparable<string>(ability, 11, 
				"spellPower", "dealDamage", "loseDurability", "loseAttack", "loseHealth",
				"plusAttack", "plusHealth", "plusDurability", "plusMana"  , "plusCost"  , 
				"reduceCost")){
					vector<string> elems;
					int firstSemanticKey, secondSemanticKey;
					int selected_value = 0;
					int marked_value = 0;
					int minKey = 100000000000;
					if(checkClique(node, first) && checkClique(node, second)){
						firstSemanticKey  = (int)node.clique[first ]->value;
						secondSemanticKey = (int)node.clique[second]->value;
						for(; selected_value < value.size(); selected_value++){
							string current = value[selected_value];
							int c_key      = node.clique[current]->value;
							if(abs(c_key - firstSemanticKey) < minKey){
								minKey = abs(c_key - firstSemanticKey);
								marked_value = selected_value;
							}
							if(selected_value > firstSemanticKey && selected_value < secondSemanticKey){break;}else if(selected_value > secondSemanticKey){break;
							}
						}
					}else{
						if((first == "attack" || first == "health") &&!checkClique(node, first)){first = "give";}
						if(checkClique<T,F>(node, "weapon"))first = "weapon";

						if(checkClique(node, first))firstSemanticKey  = (int)node.clique[first ]->value;
						else{
							map<T, HoningEdge<T,F>*>::iterator eit = node.clique.begin();
							firstSemanticKey  = (int)(*eit).second->value;
						}

						for(; selected_value < value.size(); selected_value++){
							string current = value[selected_value];
							int c_key      = node.clique[current]->value;
							if(abs(c_key - firstSemanticKey) < minKey){
								minKey = abs(c_key - firstSemanticKey);
								marked_value = selected_value;
							}
							if(selected_value > firstSemanticKey){break;}
						}
					}
					if(value.size()>0){
						split(value[marked_value],'/',elems);
						if(ability.compare("plusHealth")==0)
							if(elems.size() == 1)new_node.value = elems[0];
							else new_node.value = elems[1];
						else new_node.value = elems[0];
					}else{new_node.value = -1;}
			}return new_node;
}

template <class T, class F> void HSGroupAbilityExtractor(HoningNode<T,F>& node, vector<string>& out_abilities){
		if(microfeatureGroupANDCheck(node, 2, "choose", "one"))
			out_abilities.push_back("chooseOne");
		if(microfeatureGroupANDCheck(node, 2, "divine", "shield"))
			out_abilities.push_back("divineShield");
		if(microfeatureGroupANDCheck(node, 1, "windfury") && !microfeatureGroupANDCheck(node, 1, "mega"))
			out_abilities.push_back("windfury");
		if(microfeatureGroupANDCheck(node, 1, "windfury") && microfeatureGroupANDCheck(node, 1, "mega"))
			out_abilities.push_back("megaWindfury");
		if(microfeatureGroupANDCheck(node, 2, "spell", "power"))
			out_abilities.push_back("spellPower");
		if(microfeatureGroupANDCheck(node, 2, "can't", "attack"))
			out_abilities.push_back("cantAttack");
		if(microfeatureGroupANDCheck(node, 2, "deal", "damage"))
			out_abilities.push_back("dealDamage");
		if(microfeatureGroupANDCheck(node, 2, "lose", "armor"))
			out_abilities.push_back("loseArmor");
		if(microfeatureGroupANDCheck(node, 2, "lose", "durability"))
			out_abilities.push_back("loseDurability");
		if(microfeatureGroupANDCheck(node, 2, "lose", "attack"))
			out_abilities.push_back("loseAttack");
		if(microfeatureGroupANDCheck(node, 2, "lose", "health"))
			out_abilities.push_back("loseHealth");
		if(microfeatureGroupANDCheck(node, 2, "reduce", "cost"))
			out_abilities.push_back("reduceCost");
		if(microfeatureGroupANDCheck(node, 2, "give", "attack") || microfeatureGroupANDCheck(node, 1, "attack"))
			out_abilities.push_back("plusAttack");
		if(microfeatureGroupANDCheck(node, 2, "give", "health"))
			out_abilities.push_back("plusHealth");
}

template<class T, class F> void HSGetTargets(HoningNode<T,F>& node, vector<string>& targets){
	if(microfeatureGroupANDCheck(node, 1, "minion"))
		targets.push_back("minion");
	if(microfeatureGroupANDCheck(node, 2, "allied", "minion"))
		targets.push_back("alliedMinion");
	if(microfeatureGroupANDCheck(node, 2, "next", "minions"))
		targets.push_back("nextMinions");
	if(microfeatureGroupANDCheck(node, 2, "all", "minions"))
		targets.push_back("allMinion");
	if(microfeatureGroupANDCheck(node, 3, "all", "allied", "minions"))
		targets.push_back("allAlliedMinions");
	if(microfeatureGroupANDCheck(node, 3, "all", "enemy", "minions"))
		targets.push_back("allEnemyMinions");
	if(microfeatureGroupANDCheck(node, 1, "hero"))
		targets.push_back("hero");
	if(microfeatureGroupANDCheck(node, 2, "enemy", "hero"))
		targets.push_back("enemyHero");
	if(microfeatureGroupANDCheck(node, 2, "allied", "hero"))
		targets.push_back("alliedHero");
	if(microfeatureGroupANDCheck(node, 1, "character"))
		targets.push_back("character");
	if(microfeatureGroupANDCheck(node, 2, "allied", "character"))
		targets.push_back("alliedCharacter");
	if(microfeatureGroupANDCheck(node, 2, "all", "character"))
		targets.push_back("allCharacter");
	if(microfeatureGroupANDCheck(node, 3, "all", "allied", "character"))
		targets.push_back("allAlliedCharacters");
	if(microfeatureGroupANDCheck(node, 3, "all", "enemy", "character"))
		targets.push_back("allEnemyCharacters");
	if(microfeatureGroupANDCheck(node, 2, "legendary", "minion"))
		targets.push_back("legendaryMinion");
	if(microfeatureGroupANDCheck(node, 2, "split", "all"))
		targets.push_back("splitAll");
	if(microfeatureGroupANDCheck(node, 3, "split", "all", "enemy"))
		targets.push_back("splitAllEnemy");
	if(microfeatureGroupANDCheck(node, 3, "split", "all", "enemy", "minions"))
		targets.push_back("splitAllEnemyMinion");
	if(microfeatureGroupANDCheck(node, 2, "opponents", "weapon"))
		targets.push_back("opponentsWeapon");
	if(microfeatureGroupANDCheck(node, 2, "own", "weapon"))
		targets.push_back("ownWeapon");
	if(microfeatureGroupANDCheck(node, 1, "hand"))
		targets.push_back("hand");
	if(microfeatureGroupANDCheck(node, 2, "opponents", "hand"))
		targets.push_back("opponentsHand");
	if(microfeatureGroupANDCheck(node, 2, "own", "hand"))
		targets.push_back("ownHand");
	if(microfeatureGroupANDCheck(node, 2, "opponents", "battlefield"))
		targets.push_back("opponentsBattlefield");
	if(microfeatureGroupANDCheck(node, 2, "own", "battlefield"))
		targets.push_back("ownBattlefield");
	if(microfeatureGroupANDCheck(node, 3, "own", "mana", "crystals"))
		targets.push_back("ownManaCrystals");
	if(microfeatureGroupANDCheck(node, 3, "opponents", "mana", "crystals"))
		targets.push_back("opponentsManaCrystals");
	if(microfeatureGroupANDCheck(node, 2, "opponents", "deck"))
		targets.push_back("opponentsDeck");
	if(microfeatureGroupANDCheck(node, 2, "own", "deck"))
		targets.push_back("ownDeck");
	if(microfeatureGroupANDCheck(node, 3, "allied", "hero", "power"))
		targets.push_back("alliedHeroPower");
	if(microfeatureGroupANDCheck(node, 2, "opponents", "card"))
		targets.push_back("opponentsCard");
}

template <class T, class F> void microfeaturesInterpreter(HoningNetwork<T,F>& net, map<T, att>& atts, map<T, vector<HSAbilityNode>>& treatedTexts){
	// Numeric
	vector<string> out_numeric;
	getNumericMicrofeatures(net, out_numeric);

	map<T, att>::iterator it = atts.begin();
	for(; it != atts.end(); it++){
		T currentKey = (*it).first;
		HoningNode<T,F>* currentNode = net.network[currentKey];

		vector<string> abilities;
		textExtractor((*it).second, abilities, 25, 
			"battlecry", "charge"  , "combo"   , "deathrattle", "enrage"	,
			"freeze"   , "inspire" , "overload", "secret"     , "silence"	, 
			"stealth"  , "taunt"   , "control" , "copy"       , "counter"	, 
			"destroy"  , "discard" , "draw"    , "equip"      , "forgetful" ,
			"immune"   , "return"  , "summon"  , "swap"       , "transform");

		HSGroupAbilityExtractor(*currentNode, abilities);

		vector<string> targets;
		HSGetTargets(*currentNode, targets);

		if(targets.size() == 0)
			targets.push_back("self");

		vector<string> values;
		textExtractor((*it).second, values,	out_numeric);

		for(int i = 0; i < values.size(); i++)
			HSabilityComplement(abilities, values[i]);

		for(int i = 0; i < abilities.size(); i++){
			HSAbilityNode aNode;
			aNode = createAbilityNode(*currentNode, abilities[i], values, targets);
			treatedTexts[currentKey].push_back(aNode);
		}

		textExtractorNode new_data;
	}
}

void writeJson(const char* fileName, map<string, vector<HSAbilityNode>>& input_data){
	FILE * file;
	file = fopen(fileName, "w");

	fprintf(file,"[");
	map<string, vector<HSAbilityNode>>::iterator it;
	it = input_data.begin();
	for(;it != input_data.end(); it++){
		fprintf(file,"{");
		fprintf(file,"\"card_name\":\"%s\",",(*it).first.c_str());
		fprintf(file,"\"card_data\":[");
		vector<HSAbilityNode>& currentNode = (*it).second;
		for(int i = 0; i < currentNode.size(); i++){
			fprintf(file,"{");
			fprintf(file,"\"ability\":\"%s\","	, currentNode[i].ability.c_str());
			fprintf(file,"\"value\":\"%s\","	, currentNode[i].value.c_str()	);
			fprintf(file,"\"target\":\"%s\""	, currentNode[i].target.c_str()	);
			if(i == currentNode.size() - 1)
				fprintf(file,"}");
			else
				fprintf(file,"},");
		}
		fprintf(file,"]");
		fprintf(file,"},");
	}
	fprintf(file,"]");
	fclose(file);
}

int main(int argc, char* argv[]){
	srand((unsigned int)time(NULL));

	map<string, string>			mf_filter;
	map<string, att>			out_att;
	HoningNetwork<string, int>	network;
	map<string, vector<HSAbilityNode>> outputAbilities;

	readCards("E:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\AllSets.json", out_att);

	populateNetwork(out_att, network);

	microfeaturesInterpreter(network, out_att, outputAbilities);

	writeJson("output_text_data.json",outputAbilities);

	return 0;
}