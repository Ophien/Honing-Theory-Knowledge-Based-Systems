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
#include "Jzon.h"

using namespace std;

template<class T> class honingNode;
template<class T> class honingNetwork;
int readBattlefields(
	const char* file,
	int& start,
	honingNetwork<string> & net,
	vector<vector<string>>& myTurns,
	vector<string>        & heroPlayed,
	vector<vector<string>>& enemyTurns,
	vector<vector<string>>& myCombos,
	vector<vector<string>>& enemyCombos);
int randInt(int min, int max){ 
	if( max < 0) max = 0;
	if( min < 0) min = 0;
	return min + (rand() % (int)(max - min + 1)); }

template<class T> class honingNode{
public:
	honingNode(void){}
	~honingNode(void){}

	map<T, honingNode<T>*> clique;
	map<T, honingNode<T>*> parents;

	T holder;
};

template<class T> class honingNetwork{
public:
	honingNetwork(void){}
	~honingNetwork(void){}

	map<T, honingNode<T>*> network;
};

class att{
public:
	att(){ mana = attack = mana = health = 0;}
	int mana, attack, health; string text; string original_name;
	map<string,string> mfs;
};

int readCardsAttributes(const char* file, map<string, att>& out_att){
	/* MAP-ING */
	//if (out_map == 0x0)
	//	out_map = new honingNet< string >();

	// JSON reading
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
				/* Nome */
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
			_att.mfs[type] = type;
			out_att[card_name] = _att;
			
		}
	}

	return 0;
}

template < class T > bool checkMicrofeature(honingNetwork<T>& net, T seed, T microfeature){
	map<T, honingNode<T>*>::iterator seeker;
	seeker = net.network.find(seed);
	if (seeker == net.network.end())
		return false;

	bool check = false;

	honingNode<T>* node;
	node = (*seeker).second;

	map<T, honingNode<T>*>::iterator it;
	it = node->clique.find(microfeature);
	if (it == node->clique.end())
		check = true;

	return check;
}

int readBattlefields(
	const char* file,
	int& start,
	honingNetwork<string> & net,
	vector<vector<string>>& myTurns,
	vector<string>        & heroPlayed,
	vector<vector<string>>& enemyTurns,
	vector<vector<string>>& myCombos,
	vector<vector<string>>& enemyCombos,
	map<string, string>& invalid_objs){

	Jzon::Array rootNode;
	Jzon::FileReader::ReadFile(file, rootNode);

	Jzon::Array::iterator it = rootNode.begin();
	Jzon::Object fullNode = (*it).AsObject();
	Jzon::Object::iterator obj_it = fullNode.begin();

	/* Nome */
	int currentPlayer = (*obj_it).second.AsValue().ToInt();
	start = currentPlayer;
	it++;

	vector<map<string, string>> myBF;
	vector<map<string, string>> enemyBf;
	int currentBF = 0;

	for (; it != rootNode.end(); it++)
	{
		map<string, string> curMyBF;
		map<string, string> curEnBF;
		string currentHero;
		vector<string> combos;

		//Player battlefield
		Jzon::Object::iterator playerBfIt = (*it).AsObject().begin();
		Jzon::Array::iterator insidePBf = (*playerBfIt).second.AsArray().begin();
		for (; insidePBf != (*playerBfIt).second.AsArray().end(); insidePBf++)
		{
			string card = (*insidePBf).AsValue().ToString();

			card.erase(remove_if(card.begin(), card.end(), isspace), card.end());
			transform(card.begin(), card.end(), card.begin(), ::toupper);

			map < string, honingNode<string>* >::iterator it_s;
			it_s = net.network.find(card);
			if (it_s == net.network.end()){
				map<string, string>::iterator it_ss;
				it_ss = invalid_objs.find(card);
				if (it_ss == invalid_objs.end())
					invalid_objs[card] = card;
				continue;
			}

			if (myBF.size() < 1)
				curMyBF[card] = card;
			else{
				map<string, string>::iterator seeker;
				seeker = myBF[currentBF - 1].find(card);

				if (checkMicrofeature<string>(net, card, "spell") || checkMicrofeature<string>(net, card, "weapon")){
					if (seeker == myBF[currentBF - 1].end())
						curMyBF[card] = card;
				}
				else{
					curMyBF[card] = card;
				}
			}//end else
		}// end for

		//Player hero
		playerBfIt++;
		string hero = (*playerBfIt).second.AsValue().ToString();
		currentHero = hero;

		//Enemy battle field
		playerBfIt++;
		insidePBf = (*playerBfIt).second.AsArray().begin();
		for (; insidePBf != (*playerBfIt).second.AsArray().end(); insidePBf++)
		{
			string card = (*insidePBf).AsValue().ToString();

			card.erase(remove_if(card.begin(), card.end(), isspace), card.end());
			transform(card.begin(), card.end(), card.begin(), ::toupper);

			map < string, honingNode<string>* >::iterator it_s;
			it_s = net.network.find(card);
			if (it_s == net.network.end()){
				map<string, string>::iterator it_ss;
				it_ss = invalid_objs.find(card);
				if (it_ss == invalid_objs.end())
					invalid_objs[card] = card;
				continue;
			}

			if (enemyBf.size() < 1)
				curEnBF[card] = card;
			else{
				map<string, string>::iterator seeker;
				seeker = enemyBf[currentBF - 1].find(card);

				if (checkMicrofeature<string>(net, card, "spell") || checkMicrofeature<string>(net, card, "weapon")){
					if (seeker == enemyBf[currentBF - 1].end())
						curEnBF[card] = card;
				}
				else{
					curEnBF[card] = card;
				}
			}//end else
		}//end for

		//Combo generated
		playerBfIt++;
		insidePBf = (*playerBfIt).second.AsArray().begin();
		for (; insidePBf != (*playerBfIt).second.AsArray().end(); insidePBf++)
		{
			string card = (*insidePBf).AsValue().ToString();

			card.erase(remove_if(card.begin(), card.end(), isspace), card.end());
			transform(card.begin(), card.end(), card.begin(), ::toupper);

			map<string, honingNode<string>*>::iterator net_it;
			net_it = net.network.find(card);

			if (net_it != net.network.end())
				combos.push_back(card);
			else{
				map<string, string>::iterator it_ss;
				it_ss = invalid_objs.find(card);
				if (it_ss == invalid_objs.end())
					invalid_objs[card] = card;
			}
		}

		//myTurns.push_back(myBF);
		heroPlayed.push_back(currentHero);
		//enemyTurns.push_back(enemyBf);

		myBF.push_back(curMyBF);
		enemyBf.push_back(curEnBF);

		vector<string> bf_to_insert;
		vector<string> enemy_bf_to_insert;

		map<string, string>::iterator bfIt = curMyBF.begin();
		map<string, string>::iterator ebfIt = curEnBF.begin();
		for (; bfIt != curMyBF.end(); bfIt++)
			bf_to_insert.push_back((*bfIt).second);

		for (; ebfIt != curEnBF.end(); ebfIt++)
			enemy_bf_to_insert.push_back((*ebfIt).second);

		myTurns.push_back(bf_to_insert);
		enemyTurns.push_back(enemy_bf_to_insert);

		vector<string> empty_combo;
		if (currentPlayer == 1){
			myCombos.push_back(combos);
			enemyCombos.push_back(empty_combo);
		}
		else{
			myCombos.push_back(empty_combo);
			enemyCombos.push_back(combos);
		}

		// Player check
		//if (currentPlayer == 1)
		//	currentPlayer = 2;
		//else
		//	currentPlayer = 1;

		// error handler
		currentBF++;
	}

	return 0;
}

template <class T> bool match_vector(vector<T>& A, vector<T>& B){
	if (A.size() != B.size())
		return false;

	for (int i = 0; i < A.size(); i++){
		bool check = false;

		for (int j = 0; j < B.size(); j++){
			if (A[i] == B[j]){
				check = true;
				break;
			}
		}

		if (check == false)
			return false;
	}

	return true;
}

template <class T> int countMicrofeatures(honingNetwork<T>& net, vector<T>& base){
	int count_base = 0;
	for (unsigned int i = 0; i < base.size(); i++){
		count_base += net.network[base[i]]->clique.size();
	}

	return count_base;
}


void treatLocale(vector<string>& context){
	for (unsigned int i = 0; i < context.size(); i++){
		context[i].erase(remove_if(context[i].begin(), context[i].end(), isspace), context[i].end());
		transform(context[i].begin(), context[i].end(), context[i].begin(), ::toupper);
	}
}

void build_locale_focus(
	vector<string>& contextual_focus_locale_druida,
	vector<string>& contextual_focus_locale_hunter,
	vector<string>& contextual_focus_locale_mage,
	vector<string>& contextual_focus_locale_paladin,
	vector<string>& contextual_focus_locale_priest,
	vector<string>& contextual_focus_locale_rogue,
	vector<string>& contextual_focus_locale_shaman,
	vector<string>& contextual_focus_locale_warlock,
	vector<string>& contextual_focus_locale_warrior){


	contextual_focus_locale_hunter.push_back("FLESHEATINGGHOUL");
	contextual_focus_locale_hunter.push_back("UNLEASHTHEHOUNDS");
	contextual_focus_locale_hunter.push_back("KILLCOMMAND");
	contextual_focus_locale_hunter.push_back("ANIMALCOMPANION");
	contextual_focus_locale_hunter.push_back("EYEINTHESKY");
	contextual_focus_locale_hunter.push_back("WEBSPINNER");
	contextual_focus_locale_hunter.push_back("PISTONS");
	contextual_focus_locale_hunter.push_back("RAGINGWORGEN");
	contextual_focus_locale_hunter.push_back("CANNIBALIZE");
	contextual_focus_locale_hunter.push_back("MICROMACHINE");
	contextual_focus_locale_hunter.push_back("STEADYSHOT");
	contextual_focus_locale_hunter.push_back("SCAVENGINGHYENA");
	contextual_focus_locale_hunter.push_back("JUNGLEPANTHER");
	contextual_focus_locale_hunter.push_back("GLAIVEZOOKA");
	contextual_focus_locale_hunter.push_back("GURUBASHIBERSERKER");
	contextual_focus_locale_hunter.push_back("WELLFED");

	contextual_focus_locale_mage.push_back("DOOMSAYER");
	contextual_focus_locale_mage.push_back("EXPLOSIVESHEEP");
	contextual_focus_locale_mage.push_back("EMPERORTHAURISSAN");
	contextual_focus_locale_mage.push_back("FROSTBOLT");
	contextual_focus_locale_mage.push_back("BLIZZARD");
	contextual_focus_locale_mage.push_back("ARCANEINTELLECT");
	contextual_focus_locale_mage.push_back("MALYGOS");
	contextual_focus_locale_mage.push_back("BLOODMAGETHALNOS");
	contextual_focus_locale_mage.push_back("ICELANCE");
	contextual_focus_locale_mage.push_back("FIREBALL");
	contextual_focus_locale_mage.push_back("FIREBLAST");
	contextual_focus_locale_mage.push_back("ACOLYTEOFPAIN");

	contextual_focus_locale_paladin.push_back("ARCANEGOLEM");
	contextual_focus_locale_paladin.push_back("BLESSINGOFWISDOM");
	contextual_focus_locale_paladin.push_back("IRONBEAKOWL");
	contextual_focus_locale_paladin.push_back("BLUEGILLWARRIOR");
	contextual_focus_locale_paladin.push_back("WOLFRIDER");
	contextual_focus_locale_paladin.push_back("TRUESILVERCHAMPION");
	contextual_focus_locale_paladin.push_back("AVENGINGWRATH");
	contextual_focus_locale_paladin.push_back("ARGENTSQUIRE");
	contextual_focus_locale_paladin.push_back("REINFORCE");
	contextual_focus_locale_paladin.push_back("SWORDOFJUSTICE");
	contextual_focus_locale_paladin.push_back("DEFENDEROFARGUS");
	contextual_focus_locale_paladin.push_back("CONSECRATION");
	contextual_focus_locale_paladin.push_back("BLESSINGOFMIGHT");

	contextual_focus_locale_priest.push_back("NORTHSHIRECLERIC");
	contextual_focus_locale_priest.push_back("THECOIN");
	contextual_focus_locale_priest.push_back("POWERWORD:SHIELD");
	contextual_focus_locale_priest.push_back("THOUGHTSTEAL");
	contextual_focus_locale_priest.push_back("MULTI-SHOT");
	contextual_focus_locale_priest.push_back("RESURRECT");
	contextual_focus_locale_priest.push_back("THOUGHTSTEAL");
	contextual_focus_locale_priest.push_back("IRONFURGRIZZLY");
	contextual_focus_locale_priest.push_back("LESSERHEAL");
	contextual_focus_locale_priest.push_back("EARTHENRINGFARSEER");
	contextual_focus_locale_priest.push_back("PROPHETVELEN");
	contextual_focus_locale_priest.push_back("DIVINESPIRIT");
	contextual_focus_locale_priest.push_back("SHADOWBOXER");
	contextual_focus_locale_priest.push_back("VELEN'SCHOSEN");
	contextual_focus_locale_priest.push_back("CLAW");
	contextual_focus_locale_priest.push_back("BOOTYBAYBODYGUARD");
	contextual_focus_locale_priest.push_back("LIGHTNINGSTORM");
	contextual_focus_locale_priest.push_back("LIGHTWARDEN");

	contextual_focus_locale_rogue.push_back("UNSTABLEGHOUL");
	contextual_focus_locale_rogue.push_back("WILDPYROMANCER");
	contextual_focus_locale_rogue.push_back("GANGUP");
	contextual_focus_locale_rogue.push_back("BETRAYAL");
	contextual_focus_locale_rogue.push_back("COLDLIGHTORACLE");
	contextual_focus_locale_rogue.push_back("PREPARATION");
	contextual_focus_locale_rogue.push_back("VANISH");
	contextual_focus_locale_rogue.push_back("ANTIQUEHEALBOT");
	contextual_focus_locale_rogue.push_back("DAGGERMASTERY");
	contextual_focus_locale_rogue.push_back("IRONBEAKOWL");
	contextual_focus_locale_rogue.push_back("DEADLYPOISON");
	contextual_focus_locale_rogue.push_back("BLADEFLURRY");
	contextual_focus_locale_rogue.push_back("EXPLOSIVESHEEP");
	contextual_focus_locale_rogue.push_back("SAP");

	contextual_focus_locale_shaman.push_back("FIREGUARDDESTROYER");
	contextual_focus_locale_shaman.push_back("FLAMETONGUETOTEM");
	contextual_focus_locale_shaman.push_back("TOTEMICCALL");
	contextual_focus_locale_shaman.push_back("LAVASHOCK");
	contextual_focus_locale_shaman.push_back("NATPAGLE");
	contextual_focus_locale_shaman.push_back("HEX");
	contextual_focus_locale_shaman.push_back("WHIRLINGZAP-O-MATIC");
	contextual_focus_locale_shaman.push_back("MALYGOS");
	contextual_focus_locale_shaman.push_back("LIGHTNINGBOLT");
	contextual_focus_locale_shaman.push_back("DOOMHAMMER");
	contextual_focus_locale_shaman.push_back("ROCKBITERWEAPON");
	contextual_focus_locale_shaman.push_back("LIGHTNINGSTORM");
	contextual_focus_locale_shaman.push_back("BLOODMAGETHALNOS");
	contextual_focus_locale_shaman.push_back("CRACKLE");
	contextual_focus_locale_shaman.push_back("UNSTABLEGHOUL");
	contextual_focus_locale_shaman.push_back("ANNOY-O-TRON");
	contextual_focus_locale_shaman.push_back("MANATIDETOTEM");

	contextual_focus_locale_warlock.push_back("THECOIN");
	contextual_focus_locale_warlock.push_back("ARGENTSQUIRE");
	contextual_focus_locale_warlock.push_back("VOIDWALKER");
	contextual_focus_locale_warlock.push_back("STONETUSKBOAR");
	contextual_focus_locale_warlock.push_back("MORTALCOIL");
	contextual_focus_locale_warlock.push_back("LIFETAP");
	contextual_focus_locale_warlock.push_back("DARKBOMB");
	contextual_focus_locale_warlock.push_back("POWEROVERWHELMING");
	contextual_focus_locale_warlock.push_back("SHADOWFLAME");
	contextual_focus_locale_warlock.push_back("HAUNTEDCREEPER");
	contextual_focus_locale_warlock.push_back("LEPERGNOME");
	contextual_focus_locale_warlock.push_back("DEFENDEROFARGUS");
	contextual_focus_locale_warlock.push_back("ABUSIVESERGEANT");
	contextual_focus_locale_warlock.push_back("FINICKYCLOAKFIELD");
	contextual_focus_locale_warlock.push_back("DIREWOLFALPHA");
	contextual_focus_locale_warlock.push_back("KNIFEJUGGLER");
	contextual_focus_locale_warlock.push_back("ECHOINGOOZE");

	contextual_focus_locale_warrior.push_back("ARMORSMITH");
	contextual_focus_locale_warrior.push_back("INNERRAGE");
	contextual_focus_locale_warrior.push_back("WHIRLWIND");
	contextual_focus_locale_warrior.push_back("EXECUTE");
	contextual_focus_locale_warrior.push_back("ARMORUP!");
	contextual_focus_locale_warrior.push_back("GRIMPATRON");
	contextual_focus_locale_warrior.push_back("CRUELTASKMASTER");
	contextual_focus_locale_warrior.push_back("SHIELDSLAM");
	contextual_focus_locale_warrior.push_back("WHIPPEDINTOSHAPE");
	contextual_focus_locale_warrior.push_back("SHIELDBLOCK");
	contextual_focus_locale_warrior.push_back("RAMPAGE");
	contextual_focus_locale_warrior.push_back("ACOLYTEOFPAIN");
	contextual_focus_locale_warrior.push_back("DEATH'SBITE");
	contextual_focus_locale_warrior.push_back("CRUELTASKMASTER");
	contextual_focus_locale_warrior.push_back("AMANIBERSERKER");
}

template <class T> void microfeatureInterpreter(map<T, att>& atts, honingNetwork<T>& net, map<T, T>& microfeatures_filter){
	map<T, T> mfs_extracted;

	map<T, honingNode<T>*>::iterator it;
	it = net.network.begin();
	for(; it != net.network.end(); it++){
		map<T, att>::iterator it_seek;
		it_seek = atts.find((*it).first);
		if(it_seek == atts.end())
			continue;

		// read all microfeatures
		string text = (*it_seek).second.text;
		
		if(text.length() <= 0)
			continue;

		string concat;
		for(unsigned int i = 0; i < text.length(); i++){
			char read = text.at(i);
			
			if((65 <= read && read <= 90) || (97 <= read && read <= 122)){
				concat += read;
			}else{
				if(concat.length() > 3)
				{
					concat.erase(remove_if(concat.begin(), concat.end(), isspace), concat.end());
					transform(concat.begin(), concat.end(), concat.begin(), ::tolower);

					map<T,T>::iterator seeker;
					seeker = mfs_extracted.find(concat);

					map<T,T>::iterator filter_seeker;
					filter_seeker = microfeatures_filter.find(concat);
				
					if(seeker == mfs_extracted.end() && filter_seeker != microfeatures_filter.end()){
						mfs_extracted[concat] = concat;
					}
				}

				concat.clear();
			}//end else


		}//end for

		//transfer all microfeatures
		map<T, T>::iterator transfer_it;
		transfer_it = mfs_extracted.begin();
		for(; transfer_it != mfs_extracted.end(); transfer_it++){
			map<T,T>::iterator finder = (*it_seek).second.mfs.find((*transfer_it).first);
			if(finder == (*it_seek).second.mfs.end())
				(*it_seek).second.mfs[(*transfer_it).first] = (*transfer_it).first;
		}
		mfs_extracted.clear();
	}//end big for
}

template <class T> void buildFilter(map<T,T>& mf_filter){
	mf_filter["swap"] = "swap";
	mf_filter["takes"] = "takes";
	mf_filter["attack"] = "attack";
	mf_filter["minion"] = "minion";
	mf_filter["spell"] = "spell";
	mf_filter["taunt"] = "taunt";
	mf_filter["charge"] = "charge";
	mf_filter["battlecry"] = "battlecry";
	mf_filter["choose"] = "choose";
	mf_filter["combo"] = "combo";
	mf_filter["counter"] = "counter";
	mf_filter["deathrattle"] = "deathrattle";
	mf_filter["divine"] = "divine";
	mf_filter["shield"] = "shield";
	mf_filter["enrage"] = "enrage";
	mf_filter["freeze"] = "freeze";
	mf_filter["immune"] = "immune";
	mf_filter["mega"] = "mega";
	mf_filter["windfury"] = "windfury";
	mf_filter["overload"] = "overload";
	mf_filter["secret"] = "secret";
	mf_filter["silence"] = "silence";
	mf_filter["stealth"] = "stealth";
	mf_filter["damage"] = "damage";
	mf_filter["summon"] = "summon";
	mf_filter["transform"] = "transform";
	mf_filter["windfury"] = "windfury";
	mf_filter["destroy"] = "destroy";
	mf_filter["draw"] = "draw";
	mf_filter["copy"] = "copy";
	mf_filter["clumsy"] = "clumsy";
	mf_filter["deal"] = "deal";
	mf_filter["discard"] = "discard";
	mf_filter["enchant"] = "enchant";
	mf_filter["elusive"] = "elusive";
	mf_filter["equip"] = "equip";
	mf_filter["generate"] = "generate";
	mf_filter["mind"] = "mind";
	mf_filter["poison"] = "poison";
	mf_filter["return"] = "return";
	mf_filter["shuffle"] = "shuffle";
	mf_filter["give"] = "give";
	mf_filter["restore"] = "restore";
	mf_filter["health"] = "health";
	mf_filter["reduce"] = "reduce";
	mf_filter["cost"] = "cost";
	mf_filter["hand"] = "hand";
	mf_filter["hero"] = "hero";
	mf_filter["set"] = "set";
}

template <class T> void buildInternalNet(honingNetwork<T>& net, map<T, att>& atts){
	map<T, honingNode<T>*>::iterator net_seek;
	net_seek = net.network.begin();

	for(; net_seek != net.network.end(); net_seek++){
		map<T,att>::iterator mf_seeker;
		mf_seeker = atts.find((*net_seek).first);
		if(mf_seeker != atts.end()){
			
			//create all microfeatures
			map<T,T>::iterator mf_it;
			mf_it = (*mf_seeker).second.mfs.begin();
			for(; mf_it != (*mf_seeker).second.mfs.end(); mf_it++){
				map<T, honingNode<T>*>::iterator it_insertion;
				it_insertion = net.network.find((*mf_it).first);

				honingNode<T>* newNode;

				if(it_insertion == net.network.end()){
					newNode = new honingNode<T>();
					newNode->holder = (*mf_it).first;
					net.network[(*mf_it).first] = newNode;
				}else{
					newNode = net.network[(*mf_it).first];
				}

				map<T, honingNode<T>*>::iterator parent_seeker;
				parent_seeker = newNode->parents.find((*net_seek).first);

				//insert parent
				if(parent_seeker == newNode->parents.end())
					newNode->parents[(*net_seek).first] = (*net_seek).second;

				//insert clique
				map<T, honingNode<T>*>::iterator clique_seeker;
				clique_seeker = (*net_seek).second->clique.find(newNode->holder);

				if(clique_seeker == (*net_seek).second->clique.end())
					(*net_seek).second->clique[newNode->holder] = newNode;
			}//end for
		}//end if
	}//end for
}

template <class T> void populateNetComplete(honingNetwork<T>& net, map<T,att>& atts){
	map<T,att>::iterator atts_it;
	atts_it = atts.begin();
	for(; atts_it != atts.end(); atts_it++){
		honingNode<string>* newNode = new honingNode<string>();
		newNode->holder = (*atts_it).first;
		net.network[(*atts_it).first] = newNode;	
	}
}

template <class T> void recruitNeurds(honingNetwork<T>& net, vector<T>& bridges, map<T, honingNode<T>*>& output){
	for(unsigned int i = 0; i < bridges.size(); i++){
		honingNode<T>* currentBridge = net.network[bridges[i]];
		map<T, honingNode<T>*>::iterator bridge_it;
		bridge_it = currentBridge->parents.begin();
		for(;bridge_it != currentBridge->parents.end(); bridge_it++){
			map<T, honingNode<T>*>::iterator output_it;
			output_it = output.find((*bridge_it).first);
			if(output_it == output.end()){
				output[(*bridge_it).first] = (*bridge_it).second;
			}
		}
	}
}

template <class T> void contextualFilter(vector<T>& filter, map<T, honingNode<T>*>& input, map<T, honingNode<T>*>& filtered_output){
	for(unsigned int i = 0; i < filter.size(); i++){
		map<T, honingNode<T>*>::iterator filter_seeker;
		filter_seeker = input.find(filter[i]);
		if(filter_seeker != input.end()){
			map<T, honingNode<T>*>::iterator output_seeker;
			output_seeker = filtered_output.find(filter[i]);
			if(output_seeker == filtered_output.end()){
				filtered_output[filter[i]] = (*filter_seeker).second;
			}
		}
	}
}

void saveNetIntoFile(map<string, att>& out_att){
	FILE * file;
	file = fopen("network", "w");
	fprintf(file, "[");
	map<string, att>::iterator it = out_att.begin();
	for(unsigned int g = 0;it != out_att.end(); it++, g++){
		fprintf(file, "{\"card_name\":\"%s\",", (*it).second.original_name.c_str());
		fprintf(file, "\"standardized_card_name\":\"%s\",", (*it).first.c_str());
		fprintf(file, "\"mana\":\"%d\",", (*it).second.mana);
		fprintf(file, "\"attack\":\"%d\",", (*it).second.attack);
		fprintf(file, "\"health\":\"%d\",", (*it).second.health);
		map<string, string>::iterator it_s;
		it_s = (*it).second.mfs.begin();
		for(unsigned int i = 0; it_s != (*it).second.mfs.end(); it_s++, i++){
			if(i < (*it).second.mfs.size() - 1)
				fprintf(file, "\"mf%d\":\"%s\",", i, (*it_s).second.c_str());
			else
				fprintf(file, "\"mf%d\":\"%s\"", i, (*it_s).second.c_str());
		}
		if(g < out_att.size() - 1)
			fprintf(file, "},");
		else
			fprintf(file, "}");
	}
	fprintf(file, "]");
	fclose(file);
}

template <class T> void overfeatureSelection(){
}

int main(int argc, char* argv[]){
	srand((unsigned int)time(NULL));
	
	/** Building Network **/
	map<string, string>		mf_filter;
	map<string, att>		out_att;
	honingNetwork<string>	net;
	
	readCardsAttributes		("E:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\AllSets.json", out_att);
	buildFilter				(mf_filter);
	populateNetComplete		(net, out_att);
	microfeatureInterpreter	(out_att, net, mf_filter);
	buildInternalNet		(net, out_att);

	/** Test Case **/
	vector<string> contextual_focus_locale_druida;
	vector<string> bridges;
	vector<string> objectives;
	map<string, honingNode<string>*> output;
	map<string, honingNode<string>*> contextualFilter_output;

	contextual_focus_locale_druida.push_back("FORCEOFNATURE");
	contextual_focus_locale_druida.push_back("SWIPE");
	contextual_focus_locale_druida.push_back("SHAPESHIFT");
	contextual_focus_locale_druida.push_back("COLDLIGHTORACLE");
	contextual_focus_locale_druida.push_back("STARFALL");
	contextual_focus_locale_druida.push_back("HEALINGTOUCH");
	contextual_focus_locale_druida.push_back("CONSUME");
	contextual_focus_locale_druida.push_back("POISONSEEDS");
	contextual_focus_locale_druida.push_back("UNSTABLEGHOUL");
	contextual_focus_locale_druida.push_back("EXPLOSIVESHEEP");
	contextual_focus_locale_druida.push_back("NATURALIZE");
	contextual_focus_locale_druida.push_back("TREEOFLIFE");
	contextual_focus_locale_druida.push_back("DOOMSAYER");
	contextual_focus_locale_druida.push_back("RAGNAROSTHEFIRELORD");
	contextual_focus_locale_druida.push_back("SHADEOFNAXXRAMAS");
	contextual_focus_locale_druida.push_back("WILDPYROMANCER");
	contextual_focus_locale_druida.push_back("ANTIQUEHEALBOT");

	bridges.push_back("damage");
	bridges.push_back("deal");
	bridges.push_back("heropower");
	bridges.push_back("minion");

	treatLocale(contextual_focus_locale_druida);

	// simple recruitment
	recruitNeurds(net, bridges, output);

	// contextual filter
	contextualFilter(contextual_focus_locale_druida, output, contextualFilter_output);

	// selection by objetive

	saveNetIntoFile(out_att);

	return 0;
}