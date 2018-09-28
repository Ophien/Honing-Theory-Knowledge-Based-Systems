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
template<class T> void  recruitNeurds(
	honingNetwork<T>& net	, 
	vector<T>& seeds		, 
	vector<T>& objectives	,
	vector<T>& bridges		, 
	vector<T>& raw_output,
	bool expand_from_bridges);
template < class T > void padronized_test(
	honingNetwork<T>& net,
	const char* log_file,
	vector<T> objectives,
	int minimumComboThreshold,
	int combosToGenerate,
	bool expand_from_objectives,
	map<int, vector<string>>& combos_to_found,
	map<int, map<T, vector<vector<T>>>>& combos_per_card,
	map<int, int>& combos_found);
int populateNet(const char* file, honingNetwork < string >& out_map);
int readBattlefields(
	const char* file,
	int& start,
	honingNetwork<string> & net,
	vector<vector<string>>& myTurns,
	vector<string>        & heroPlayed,
	vector<vector<string>>& enemyTurns,
	vector<vector<string>>& myCombos,
	vector<vector<string>>& enemyCombos);
template <class T> int compareMicrofeature(vector<honingNode<T>*>& A, vector<honingNode<T>*>& B);
int randInt(int min, int max){ return min + (rand() % (int)(max - min + 1)); }

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

template<class T> void  recruitNeurds(
	honingNetwork<T>& net	,
	vector<T>& seeds		,
	vector<T>& objectives	,
	vector<T>& bridges		,
	vector<honingNode<T>*>& raw_output,
	bool expand_from_obective,
	bool useCardBridges){

	vector<honingNode<T>*> workspace;
	map<T, honingNode<T>*> allBridges;
	vector<honingNode<T>*> selectedBridges;
	map<T, honingNode<T>*> selectedParents;

		// use only the reliable seeds
		for (unsigned int i = 0; i < seeds.size(); i++){
			map<T, honingNode<T>*>::iterator it_s;
			it_s = net.network.find(seeds[i]);
			if (it_s != net.network.end()){
				honingNode<T>* toInsert = (*it_s).second;
				workspace.push_back(toInsert);
			}
		}//end for

		// prepare all the bridges in expansion from seed
		for (unsigned int i = 0; i < workspace.size(); i++){
			map<T, honingNode<T>*>::iterator it;
			honingNode<T>* seeker = net.network[workspace[i]->holder];
			it = seeker->clique.begin();
			for (; it != seeker->clique.end(); it++){
				map<T, honingNode<T>*>::iterator it_seeker;
				honingNode<T>* workingNode = (*it).second;
				it_seeker = allBridges.find(workingNode->holder);
				if (it_seeker == allBridges.end()){
					allBridges[workingNode->holder] = workingNode;
					if (expand_from_obective)
						objectives.push_back(workingNode->holder);
				}
			}//end for	
		}//end for

		// prepare bridges to retraction
		for (unsigned int i = 0; i < bridges.size(); i++){
			map<T, honingNode<T>*>::iterator it;
			T brid_i = bridges[i];
			it = allBridges.find(brid_i);
			if (it != allBridges.end()){
				selectedBridges.push_back(allBridges[brid_i]);
			}
		}

	//	if (expand_from_obective)
	//	for (int i = 0; i < objectives.size(); i++){
	//		selectedBridges.push_back(net.network[objectives[i]]);
	//	}

	// retract to parents
	for (unsigned int i = 0; i < selectedBridges.size(); i++){
		honingNode<T>* currentNode = selectedBridges[i];
		map<T, honingNode<T>*>::iterator parent_seeker;
		parent_seeker = currentNode->parents.begin();
		for (; parent_seeker != currentNode->parents.end(); parent_seeker++){
			honingNode<T>* selectedParent = (*parent_seeker).second;
			map<T, honingNode<T>*>::iterator seeker_p;
			seeker_p = selectedParents.find(selectedParent->holder);
			if (seeker_p == selectedParents.end()){
				selectedParents[selectedParent->holder] = selectedParent;
				//if (expand_from_obective)
				//	raw_output.push_back(selectedParent);
			}
		}
	}// end for


		map<T, honingNode<T>*>::iterator check_obj;
		check_obj = selectedParents.begin();
		for (; check_obj != selectedParents.end(); check_obj++)
		{
			bool all_obj = false;
			for (int i = 0; i < objectives.size(); i++){
				map<T, honingNode<T>*>::iterator it_obj;
				it_obj = (*check_obj).second->clique.find(objectives[i]);

				if (it_obj != (*check_obj).second->clique.end()){
					//vector<T>::iterator it_e = objectives.begin(); // ^^
					//advance(it_e, i);
			
					//objectives.erase(it_e);

					all_obj = true;
					break;
				}
			}

				if (all_obj){
					raw_output.push_back((*check_obj).second);
				}
		}
}

class att{
public:
	att(){ mana = attack = mana = health = 0;}
	int mana, attack, health;
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
			int cost;
			int attack;
			int health;

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
			}//end for

			card_name.erase(remove_if(card_name.begin(), card_name.end(), isspace), card_name.end());
			transform(card_name.begin(), card_name.end(), card_name.begin(), ::toupper);

			out_att[card_name] = _att;
		}
	}

	return 0;
}

int populateNet(const char* file, honingNetwork < string >& out_map){
	/* MAP-ING */
	//if (out_map == 0x0)
	//	out_map = new honingNet< string >();

	// JSON reading
	Jzon::Array rootNode;
	Jzon::FileReader::ReadFile(file, rootNode);

	for (Jzon::Array::iterator it = rootNode.begin(); it != rootNode.end(); it++)
	{
		Jzon::Object fullNode = (*it).AsObject();
		Jzon::Object::iterator obj_it = fullNode.begin();

		/* Nome */
		string card_name = (*obj_it).second.AsValue().ToString();
		card_name.erase(remove_if(card_name.begin(), card_name.end(), isspace), card_name.end());
		transform(card_name.begin(), card_name.end(), card_name.begin(), ::toupper);

		obj_it++;

		/* Text */
		string card_text = (*obj_it).second.AsValue().ToString();
		obj_it++;

		/* INSERT CARD */
		honingNode<string>* last = 0x0;

		map<string, honingNode<string>*>::iterator it_i;
		it_i = out_map.network.find(card_name);
		if (it_i == out_map.network.end()){
			honingNode<string>* newNode = new honingNode<string>();
			newNode->holder = card_name;
			out_map.network[card_name] = newNode;

			last = newNode;
		}
		else{
			last = out_map.network[card_name];
		}

		/* Array */
		Jzon::Array::iterator microfeatures_it = (*obj_it).second.AsArray().begin();
		for (; microfeatures_it != (*obj_it).second.AsArray().end(); microfeatures_it++){
			Jzon::Object::iterator m_level_n = (*microfeatures_it).AsObject().begin();
			for (; m_level_n != (*microfeatures_it).AsObject().end(); m_level_n++){
				string card_name_internal = ((*m_level_n).second.AsValue().ToString());

				card_name_internal.erase(remove_if(card_name_internal.begin(), card_name_internal.end(), isspace), card_name_internal.end());
				transform(card_name_internal.begin(), card_name_internal.end(), card_name_internal.begin(), ::tolower);

				it_i = out_map.network.find(card_name_internal);
				if (it_i == out_map.network.end()){
					honingNode<string>* newNode_internal = new honingNode<string>();
					newNode_internal->holder = card_name_internal;
					out_map.network[card_name_internal] = newNode_internal;

				}

				map<string, honingNode<string>*>::iterator i_it;
				i_it = last->clique.find(card_name_internal);
				if (i_it == last->clique.end()){
					last->clique[card_name_internal] = out_map.network[card_name_internal];
				}

				honingNode<string>* h = out_map.network[card_name_internal];
				h->parents[last->holder] = last;
			}
		}//end for
	}// end for

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

template <class T> int getTotalManaFromCombo(vector<T>& combo, map<T, att>& atts){
	int total_mana = 0;
	for (int i = 0; i < combo.size(); i++){
		total_mana += atts[combo[i]].mana;
	}
	return total_mana;
}

template <class T> bool objectiveCheck(honingNetwork<T>& net, vector<T>& ojectives, vector<T>& combo){
	int check = 0;

	map<T, T> micro;
	for (int i = 0; i < combo.size(); i++){
		map<T, honingNode<T>*>::iterator it;
		it = net.network[combo[i]]->clique.begin();
		for (; it != net.network[combo[i]]->clique.end(); it++){
			map<T, T>::iterator itInt;
			itInt = micro.find((*it).second->holder);
			if (itInt == micro.end()){
				micro[(*it).second->holder] = (*it).second->holder;
			}
		}
	}

	bool response = true;

	for (int i = 0; i < ojectives.size();i++){
		map<T, T>::iterator it_seek;
		it_seek = micro.find(ojectives[i]);
		if (it_seek == micro.end()){
			response = false;
			break;
		}
	}

	return response;
}

template < class T > void padronized_test(
	honingNetwork<T>& net,
	map<T, att>& atts,
	const char* log_file,
	vector<T> objectives,
	vector<T> contextural_focus_locale,
	int minimumComboThreshold, 
	int combosToGenerate, 
	bool expand_from_objectives,
	bool use_focus,
	/*output*/
	map<int, vector<int>>& diff,
	map<int, vector<string>>& combos_to_found,
	map<int, map<T, vector<vector<T>>>>& combos_per_card,
	map<int, int>& combos_found,
	map<string, string>& invalid_cards){

	// populate battlefields
	vector<vector<T>> myBattlefields;
	vector<vector<T>> enemyBattlefields;
	vector<T>		  herosPlayed;
	vector<vector<T>> myCombo;
	vector<vector<T>> enemyCombo;
	int startPlayer;

	readBattlefields(log_file, startPlayer, net, myBattlefields, herosPlayed, enemyBattlefields, myCombo, enemyCombo, invalid_cards);

	// start tests
	unsigned int i = 0;

	if (startPlayer != 1)
		i = 1;

	for (; i < myBattlefields.size(); i += 1)
	{
		if (myCombo[i].size() >= minimumComboThreshold)
			continue;

		vector<T> chain_search;
		vector<T> seeds;
		vector<honingNode<T>*> raw_output;

		map<T, T> all_bridges;

		if (expand_from_objectives){
			for (int j = 0; j < myCombo[i].size(); j++){
				map<T, honingNode<T>*>::iterator it_int;
				it_int = net.network[myCombo[i][j]]->clique.begin();
				for (; it_int != net.network[myCombo[i][j]]->clique.end(); it_int++){
					all_bridges[(*it_int).second->holder] = (*it_int).second->holder;
				}
			}
		}

		// preparing bridges
		for (int m = 0; m < myBattlefields[i].size(); m++){
			map<T, honingNode<T>*>::iterator internal_it;

			map<T, honingNode<T>*>::iterator i_seek = net.network.find(myBattlefields[i][m]);

			if (i_seek == net.network.end())
				continue;

			internal_it = net.network[myBattlefields[i][m]]->clique.begin();
			for (; internal_it != net.network[myBattlefields[i][m]]->clique.end(); internal_it++){
				map<T, T>::iterator it_seeker;
				it_seeker = all_bridges.find((*internal_it).first);
				if (it_seeker == all_bridges.end())
					all_bridges[(*internal_it).first] = (*internal_it).first;
			}
		}

		for (int m = 0; m < enemyBattlefields[i].size(); m++){
			map<T, honingNode<T>*>::iterator internal_it;

			map<T, honingNode<T>*>::iterator i_seek = net.network.find(enemyBattlefields[i][m]);

			if (i_seek == net.network.end())
				continue;

			internal_it = net.network[enemyBattlefields[i][m]]->clique.begin();
			for (; internal_it != net.network[enemyBattlefields[i][m]]->clique.end(); internal_it++){
				map<T, T>::iterator it_seeker;
				it_seeker = all_bridges.find((*internal_it).first);
				if (it_seeker == all_bridges.end())
					all_bridges[(*internal_it).first] = (*internal_it).first;
			}
		}

		// converting bridges
		map<T, T>::iterator it_brid;
		it_brid = all_bridges.begin();
		for (; it_brid != all_bridges.end(); it_brid++)
			chain_search.push_back((*it_brid).second);

		// expand from objectives
		map<T, honingNode<T>*> raw;
		vector<honingNode<T>*> final_form;

		//Preparing seeds
		seeds.push_back(myCombo[i][0]);

		recruitNeurds<T>(net, seeds, objectives, chain_search, raw_output, expand_from_objectives, true);

		// Contextual focus filter
		for (int d = 0; d < raw_output.size(); d++)
			raw[raw_output[d]->holder] = raw_output[d];

		for (int d = 0; d < contextural_focus_locale.size(); d++)
		{
			if (use_focus){
				map<T, honingNode<T>*>::iterator it_seeker;
				it_seeker = raw.find(contextural_focus_locale[d]);
				if (it_seeker != raw.end())
					final_form.push_back(raw[contextural_focus_locale[d]]);
			}
		}

		map<T, honingNode<T>*>::iterator itSec = raw.begin();
		while (itSec != raw.end()){
			final_form.push_back((*itSec).second);
			itSec++;
		}
		

		int max_mana = getTotalManaFromCombo<T>(myCombo[i], atts);

		int t = 0;
		//Generate and store combo for that seed from that turn
		while (combos_per_card[i][myCombo[i][0]].size() < combosToGenerate){
			// selected combos
			vector<T> real_output;

			real_output.push_back(myCombo[i][0]);

			//TREAT COMBOS
			int g = 0;
			while (g < myCombo[i].size() - 1){
				int rand_combo = randInt(0, final_form.size() - 1);
				if (rand_combo < 0) rand_combo = 0;
				real_output.push_back(final_form[rand_combo]->holder);
				g++;
			}

			int mana_new = getTotalManaFromCombo<T>(real_output, atts);

			if (t == 100){
				combos_per_card[i][myCombo[i][0]].push_back(real_output);

				int combodiff = compareMicrofeature<string>(net, myCombo[i], real_output);
				diff[i].push_back(combodiff);
				t = 0;
			}
			else
				if ((mana_new <= max_mana) && objectiveCheck(net, objectives, real_output)){
					// optimization
					if (match_vector(myCombo[i], real_output))
						combos_found[i]++;

					combos_per_card[i][myCombo[i][0]].push_back(real_output);
					t = 0;

					int combodiff = compareMicrofeature<string>(net, myCombo[i], real_output);
					diff[i].push_back(combodiff);

				}
				else
					t++;
		}// end for

		objectives.clear();

		// prepare output to validate
		combos_to_found[i] = (myCombo[i]);
	}//end for
}

template <class T> int compareMicrofeature(honingNetwork<T>& net, vector<T>& base, vector<T>& B){
	int count_base = 0;
	for (int i = 0; i < base.size(); i++){
		count_base += net.network[base[i]]->clique.size();
	}

	int count_b = 0;
	for (int i = 0; i < B.size(); i++){
		count_b += net.network[B[i]]->clique.size();
	}

	if (count_base >= count_b)
		return 0;
	else
		return abs(count_base - count_b);
}

void treatLocale(vector<string>& context){
	for (int i = 0; i < context.size(); i++){
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
	contextual_focus_locale_druida.push_back("Naturalize");
	contextual_focus_locale_druida.push_back("shape shift");
	contextual_focus_locale_druida.push_back("Doomsayer");
	contextual_focus_locale_druida.push_back("Explosive Sheep");
	contextual_focus_locale_druida.push_back("Unstable Ghoul");
	contextual_focus_locale_druida.push_back("Wild Pyromancer");
	contextual_focus_locale_druida.push_back("Healing Touch");
	contextual_focus_locale_druida.push_back("Coldlight Oracle");
	contextual_focus_locale_druida.push_back("Shade of Naxxramas");
	contextual_focus_locale_druida.push_back("Poison Seeds");
	contextual_focus_locale_druida.push_back("Swipe");
	contextual_focus_locale_druida.push_back("Starfall");
	contextual_focus_locale_druida.push_back("Abomination");
	contextual_focus_locale_druida.push_back("Antique Healbot");
	contextual_focus_locale_druida.push_back("Force of Nature");
	contextual_focus_locale_druida.push_back("Baron Geddon");
	contextual_focus_locale_druida.push_back("Ragnaros the Firelord");
	contextual_focus_locale_druida.push_back("Tree of Life");
	contextual_focus_locale_druida.push_back("Mark of the Wild");
	contextual_focus_locale_druida.push_back("Excess Mana");
	contextual_focus_locale_druida.push_back("the coin");
	contextual_focus_locale_druida.push_back("consume");
	contextual_focus_locale_druida.push_back("hero power");

	contextual_focus_locale_hunter.push_back("Steady Shot");
	contextual_focus_locale_hunter.push_back("Webspinner");
	contextual_focus_locale_hunter.push_back("Glaivezooka");
	contextual_focus_locale_hunter.push_back("Explosive Trap");
	contextual_focus_locale_hunter.push_back("Micro Machine");
	contextual_focus_locale_hunter.push_back("Scavenging Hyena");
	contextual_focus_locale_hunter.push_back("Stonespliter Trogg");
	contextual_focus_locale_hunter.push_back("Animal Companion");
	contextual_focus_locale_hunter.push_back("Kill Command");
	contextual_focus_locale_hunter.push_back("Unleash the Hounds");
	contextual_focus_locale_hunter.push_back("Flesheating Ghoul");
	contextual_focus_locale_hunter.push_back("Raging Worgen");
	contextual_focus_locale_hunter.push_back("Shade of Naxxramas");
	contextual_focus_locale_hunter.push_back("Burly Rockjaw Trogg");
	contextual_focus_locale_hunter.push_back("Grim Patron");
	contextual_focus_locale_hunter.push_back("Gurubashi Berserker");
	contextual_focus_locale_hunter.push_back("the coin");
	contextual_focus_locale_hunter.push_back("consume");
	contextual_focus_locale_hunter.push_back("heropower");

	contextual_focus_locale_mage.push_back("Fireblast");
	contextual_focus_locale_mage.push_back("Ice Lance");
	contextual_focus_locale_mage.push_back("Frostbolt");
	contextual_focus_locale_mage.push_back("Bloodmage Thalnos");
	contextual_focus_locale_mage.push_back("Doomsayer");
	contextual_focus_locale_mage.push_back("Explosive Sheep");
	contextual_focus_locale_mage.push_back("Mad Scientist");
	contextual_focus_locale_mage.push_back("Arcane Intellect");
	contextual_focus_locale_mage.push_back("Frost Nova");
	contextual_focus_locale_mage.push_back("Ice Barrier");
	contextual_focus_locale_mage.push_back("Ice Block");
	contextual_focus_locale_mage.push_back("Acolyte of Pain");
	contextual_focus_locale_mage.push_back("Fireball");
	contextual_focus_locale_mage.push_back("Emperor Thaurissan");
	contextual_focus_locale_mage.push_back("Flamestrike");
	contextual_focus_locale_mage.push_back("Malygos");
	contextual_focus_locale_mage.push_back("Pyroblast");
	contextual_focus_locale_mage.push_back("the coin");
	contextual_focus_locale_mage.push_back("consume");
	contextual_focus_locale_mage.push_back("heropower");

	contextual_focus_locale_paladin.push_back("Reinforce");
	contextual_focus_locale_paladin.push_back("Blessing of Might");
	contextual_focus_locale_paladin.push_back("Blessing of Wisdom");
	contextual_focus_locale_paladin.push_back("Argent Squire");
	contextual_focus_locale_paladin.push_back("Leper Gnome");
	contextual_focus_locale_paladin.push_back("Equality");
	contextual_focus_locale_paladin.push_back("Bluegill Warrior");
	contextual_focus_locale_paladin.push_back("Ironbeak Owl");
	contextual_focus_locale_paladin.push_back("Sword of Justice");
	contextual_focus_locale_paladin.push_back("Divine Favor");
	contextual_focus_locale_paladin.push_back("Arcane Golem");
	contextual_focus_locale_paladin.push_back("Wolfrider");
	contextual_focus_locale_paladin.push_back("Truesilver Champion");
	contextual_focus_locale_paladin.push_back("Consecration");
	contextual_focus_locale_paladin.push_back("Defender of Argus");
	contextual_focus_locale_paladin.push_back("Leeroy Jenkins");
	contextual_focus_locale_paladin.push_back("Sludge Belcher");
	contextual_focus_locale_paladin.push_back("Avenging Wrath");
	contextual_focus_locale_paladin.push_back("the coin");
	contextual_focus_locale_paladin.push_back("consume");
	contextual_focus_locale_paladin.push_back("heropower");

	contextual_focus_locale_priest.push_back("Lesser Heal");
	contextual_focus_locale_priest.push_back("Mind Spike");
	contextual_focus_locale_priest.push_back("Mind Shatter");
	contextual_focus_locale_priest.push_back("Power Word: Shield");
	contextual_focus_locale_priest.push_back("Lightwarder");
	contextual_focus_locale_priest.push_back("Northshire Cleric");
	contextual_focus_locale_priest.push_back("Divine Spirit");
	contextual_focus_locale_priest.push_back("Mind Blast");
	contextual_focus_locale_priest.push_back("Resurrect");
	contextual_focus_locale_priest.push_back("Shadow Word: Pain");
	contextual_focus_locale_priest.push_back("Annoy-o-Tron");
	contextual_focus_locale_priest.push_back("Shadowboxer");
	contextual_focus_locale_priest.push_back("Shadow Word: Death");
	contextual_focus_locale_priest.push_back("Thoughtsteal");
	contextual_focus_locale_priest.push_back("Velen's Chosen");
	contextual_focus_locale_priest.push_back("Lightspawn");
	contextual_focus_locale_priest.push_back("Holy Nova");
	contextual_focus_locale_priest.push_back("Holy Fire");
	contextual_focus_locale_priest.push_back("Prophet Velen");
	contextual_focus_locale_priest.push_back("the coin");
	contextual_focus_locale_priest.push_back("consume");
	contextual_focus_locale_priest.push_back("heropower");

	contextual_focus_locale_rogue.push_back("Dagger Mastery");
	contextual_focus_locale_rogue.push_back("Preparation");
	contextual_focus_locale_rogue.push_back("Conceal");
	contextual_focus_locale_rogue.push_back("Deadly Poison");
	contextual_focus_locale_rogue.push_back("Betrayal");
	contextual_focus_locale_rogue.push_back("Balde Flurry");
	contextual_focus_locale_rogue.push_back("Gang Up");
	contextual_focus_locale_rogue.push_back("Sap");
	contextual_focus_locale_rogue.push_back("Doomsayer");
	contextual_focus_locale_rogue.push_back("Explosive Sheep");
	contextual_focus_locale_rogue.push_back("Ironbeak Owl");
	contextual_focus_locale_rogue.push_back("Unstable Ghoul");
	contextual_focus_locale_rogue.push_back("Wild Pyromancer");
	contextual_focus_locale_rogue.push_back("Coldlight Oracle");
	contextual_focus_locale_rogue.push_back("Antique Healbot");
	contextual_focus_locale_rogue.push_back("Vanish");
	contextual_focus_locale_rogue.push_back("the coin");
	contextual_focus_locale_rogue.push_back("consume");
	contextual_focus_locale_rogue.push_back("heropower");

	contextual_focus_locale_shaman.push_back("Totemic Call");
	contextual_focus_locale_shaman.push_back("Lightning Bolt");
	contextual_focus_locale_shaman.push_back("Rockbiter Weapon");
	contextual_focus_locale_shaman.push_back("Crackle");
	contextual_focus_locale_shaman.push_back("Lava Shock");
	contextual_focus_locale_shaman.push_back("Annoy-o-Tron");
	contextual_focus_locale_shaman.push_back("Bloodmage Thalnos");
	contextual_focus_locale_shaman.push_back("Flametongue Totem");
	contextual_focus_locale_shaman.push_back("Nat Pagle");
	contextual_focus_locale_shaman.push_back("Unstable Ghoul");
	contextual_focus_locale_shaman.push_back("Whirling Zap-o-matic");
	contextual_focus_locale_shaman.push_back("Hex");
	contextual_focus_locale_shaman.push_back("Lightning Storm");
	contextual_focus_locale_shaman.push_back("Mana Tide Totem");
	contextual_focus_locale_shaman.push_back("Fireguard Destroyer");
	contextual_focus_locale_shaman.push_back("Doomhammer");
	contextual_focus_locale_shaman.push_back("Emperor Thaurissan");
	contextual_focus_locale_shaman.push_back("Ragnaros the Firelord");
	contextual_focus_locale_shaman.push_back("Malygos");
	contextual_focus_locale_shaman.push_back("the coin");
	contextual_focus_locale_shaman.push_back("consume");
	contextual_focus_locale_shaman.push_back("heropower");

	contextual_focus_locale_warlock.push_back("Life Tap");
	contextual_focus_locale_warlock.push_back("Mortal Coil");
	contextual_focus_locale_warlock.push_back("Power Overwhelming");
	contextual_focus_locale_warlock.push_back("Abusive Sergeant");
	contextual_focus_locale_warlock.push_back("Argent Squire");
	contextual_focus_locale_warlock.push_back("Leper Gnome");
	contextual_focus_locale_warlock.push_back("Stonetusk Boar");
	contextual_focus_locale_warlock.push_back("Voidwalker");
	contextual_focus_locale_warlock.push_back("Darkbomb");
	contextual_focus_locale_warlock.push_back("Dire Wolf Alpha");
	contextual_focus_locale_warlock.push_back("Echoing Ooze");
	contextual_focus_locale_warlock.push_back("Haunted Creeper");
	contextual_focus_locale_warlock.push_back("Knife Juggler");
	contextual_focus_locale_warlock.push_back("Shattered Sun Cleric");
	contextual_focus_locale_warlock.push_back("Shadowflame");
	contextual_focus_locale_warlock.push_back("Defender of Argus");
	contextual_focus_locale_warlock.push_back("Doomguard");
	contextual_focus_locale_warlock.push_back("Ragnaros the Firelord");
	contextual_focus_locale_warlock.push_back("Majordomo Executus");
	contextual_focus_locale_warlock.push_back("Consume");
	contextual_focus_locale_warlock.push_back("the coin");
	contextual_focus_locale_warlock.push_back("consume");
	contextual_focus_locale_warlock.push_back("heropower");

	contextual_focus_locale_warrior.push_back("Armor Up");
	contextual_focus_locale_warrior.push_back("Inner Rage");
	contextual_focus_locale_warrior.push_back("Execute");
	contextual_focus_locale_warrior.push_back("Shield Slam");
	contextual_focus_locale_warrior.push_back("Whirlwind");
	contextual_focus_locale_warrior.push_back("Rampage");
	contextual_focus_locale_warrior.push_back("Amani Berserker");
	contextual_focus_locale_warrior.push_back("Armorsmith");
	contextual_focus_locale_warrior.push_back("Cruel Taskmaster");
	contextual_focus_locale_warrior.push_back("Bouncing Blade");
	contextual_focus_locale_warrior.push_back("Shield Block");
	contextual_focus_locale_warrior.push_back("Acolyte of Pain");
	contextual_focus_locale_warrior.push_back("Raging Worgen");
	contextual_focus_locale_warrior.push_back("Death's Bite");
	contextual_focus_locale_warrior.push_back("Brawl");
	contextual_focus_locale_warrior.push_back("Grim Patron");
	contextual_focus_locale_warrior.push_back("Shieldmaiden");
	contextual_focus_locale_warrior.push_back("the coin");
	contextual_focus_locale_warrior.push_back("consume");
	contextual_focus_locale_warrior.push_back("heropower");
}

void test_to_fileA(char* file_name, char* logFile, honingNetwork<string>& net, map<string, att>& atts, vector<string>& focus, vector<string> objectives, map<string, string>& invlid){
	FILE * file;
	file = fopen(file_name, "w");

	map<int, vector<string>> combos_to_found;

	int test_count[] = { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 1000, 10000 };

	string testName = file_name;
	fprintf(file, "Test 1 %s\n", file_name);
	fprintf(file, "[total,combo,tam.combo,acertos]\n");

	for (int i = 0; i < 12; i++){
		map<int, map<string, vector<vector<string>>>> combos_per_card;
		map<int, int>								  combos_found;
		map<int, vector<int>> diff;
		padronized_test<string>(
			net,
			atts,
			logFile,
			objectives,
			focus,
			10, // combo size
			test_count[i],      // combos to generate
			false,
			true,
			diff,
			combos_to_found,
			combos_per_card,
			combos_found,
			invlid);

		map<int, vector<string>>::iterator it;
		it = combos_to_found.begin();

		for (; it != combos_to_found.end(); it++){
			int combo = (*it).first;
			int tamCombo = (*it).second.size();
			int acertos;

			map<int, int>::iterator its = combos_found.find(combo);

			if (its == combos_found.end()){
				acertos = 0;
			}
			else
				acertos = combos_found[combo];
			fprintf(file, "%i,\t %i,\t %i, \t%i\n", test_count[i], combo, tamCombo, acertos);
		}
	}

	map<int, vector<string>>::iterator it;
	it = combos_to_found.begin();
	for (; it != combos_to_found.end(); it++){
		fprintf(file, "Combo %i: ", (*it).first);
		vector<string>& combo = (*it).second;
		for (int i = 0; i < combo.size(); i++){
			fprintf(file, "%s\t", combo[i].c_str());
		}
		fprintf(file, "\n");
	}

	fclose(file);
}

void test_to_fileB(char* file_name, char* logFile, honingNetwork<string>& net, map<string, att>& atts, vector<string>& focus, vector<string> objectives, map<string, string>& invlid){
	FILE * file;
	file = fopen(file_name, "w");

	map<int, vector<string>> combos_to_found;
	map<int, int> diff;

	int test_count[] = { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 1000, 10000 };

	string testName = file_name;
	fprintf(file, "Test 2 %s\n", file_name);
	fprintf(file, "[total|combo|tam.combo.base|microfeatures a mais]\n");

	for (int i = 0; i < 12; i++){
		map<int, map<string, vector<vector<string>>>> combos_per_card;
		map<int, int>								  combos_found;
		map<int, vector<int>> diff;



		padronized_test<string>(
			net,
			atts,
			logFile,
			objectives,
			focus,
			10, // combo size
			test_count[i],      // combos to generate
			true,
			false,
			diff,
			combos_to_found,
			combos_per_card,
			combos_found,
			invlid);

		map<int, vector<string>>::iterator it;
		it = combos_to_found.begin();

		int cur_combo = 0;
		for (; it != combos_to_found.end(); it++){
			int combo = (*it).first;
			int tamCombo = (*it).second.size();
			
			for (int j = 0; j < test_count[i]; j++){
				int diffcombo = diff[cur_combo][j];
				fprintf(file, "%i,\t %i,\t %i,\t %i\n", test_count[i], combo, tamCombo, diffcombo);
			}
			cur_combo++;
		}
	}

	fclose(file);
}

int main(int argc, char* argv[]){
	srand((unsigned int)time(NULL));
	
	map<string, att> out_att;
	readCardsAttributes("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\AllSets.json", out_att);

	honingNetwork<string> druid;
	honingNetwork<string> hunter;
	honingNetwork<string> mage;
	honingNetwork<string> paladin;
	honingNetwork<string> priest;
	honingNetwork<string> rogue;
	honingNetwork<string> shaman;
	honingNetwork<string> warlock;
	honingNetwork<string> warrior;

	populateNet("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\druid.json", druid);
	populateNet("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\neutrals.json", druid);
	populateNet("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\hunter.json", hunter);
	populateNet("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\neutrals.json", hunter);
	populateNet("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\mage.json", mage);
	populateNet("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\neutrals.json", mage);
	populateNet("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\paladin.json", paladin);
	populateNet("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\neutrals.json", paladin);
	populateNet("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\priest.json", priest);
	populateNet("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\neutrals.json", priest);
	populateNet("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\rogue.json", rogue);
	populateNet("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\neutrals.json", rogue);
	populateNet("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\shaman.json", shaman);
	populateNet("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\neutrals.json", shaman);
	populateNet("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\warlock.json", warlock);
	populateNet("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\neutrals.json", warlock);
	populateNet("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\warrior.json", warrior);
	populateNet("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\microfeatures_heros\\neutrals.json", warrior);

	vector<string> objectives;

	vector<string> contextual_focus_locale_druida;
	vector<string> contextual_focus_locale_hunter;
	vector<string> contextual_focus_locale_mage;
	vector<string> contextual_focus_locale_paladin;
	vector<string> contextual_focus_locale_priest;
	vector<string> contextual_focus_locale_rogue;
	vector<string> contextual_focus_locale_shaman;
	vector<string> contextual_focus_locale_warlock;
	vector<string> contextual_focus_locale_warrior;

	build_locale_focus(
		contextual_focus_locale_druida,
		contextual_focus_locale_hunter,
		contextual_focus_locale_mage,
		contextual_focus_locale_paladin,
		contextual_focus_locale_priest,
		contextual_focus_locale_rogue,
		contextual_focus_locale_shaman,
		contextual_focus_locale_warlock,
		contextual_focus_locale_warrior);

	treatLocale(contextual_focus_locale_druida);
	treatLocale(contextual_focus_locale_hunter);
	treatLocale(contextual_focus_locale_mage);
	treatLocale(contextual_focus_locale_paladin);
	treatLocale(contextual_focus_locale_priest);
	treatLocale(contextual_focus_locale_rogue);
	treatLocale(contextual_focus_locale_shaman);
	treatLocale(contextual_focus_locale_warlock);
	treatLocale(contextual_focus_locale_warrior);

	map<string, string> invalid_objs;

	test_to_fileB("Druid", "F:\\doc-Alysson\\Projetos\\VisualStudio projects\\LogsCabra_Tratados\\10_combo_druid.json",
		druid, out_att, contextual_focus_locale_druida, objectives, invalid_objs);
	test_to_fileB("Hunter", "F:\\doc-Alysson\\Projetos\\VisualStudio projects\\LogsCabra_Tratados\\10_combo_hunter.json",
	hunter, out_att, contextual_focus_locale_hunter, objectives, invalid_objs);
	test_to_fileB("Mage", "F:\\doc-Alysson\\Projetos\\VisualStudio projects\\LogsCabra_Tratados\\10_combo_mage.json",
	mage, out_att, contextual_focus_locale_mage, objectives, invalid_objs);
	test_to_fileB("Paladin", "F:\\doc-Alysson\\Projetos\\VisualStudio projects\\LogsCabra_Tratados\\10_combo_paladin.json",
	paladin, out_att, contextual_focus_locale_paladin, objectives, invalid_objs);
	test_to_fileB("Priest", "F:\\doc-Alysson\\Projetos\\VisualStudio projects\\LogsCabra_Tratados\\10_combo_priest.json",
	priest, out_att, contextual_focus_locale_priest, objectives, invalid_objs);
	test_to_fileB("Rogue", "F:\\doc-Alysson\\Projetos\\VisualStudio projects\\LogsCabra_Tratados\\10_combo_rogue.json",
	rogue, out_att, contextual_focus_locale_rogue, objectives, invalid_objs);
	test_to_fileB("Shaman", "F:\\doc-Alysson\\Projetos\\VisualStudio projects\\LogsCabra_Tratados\\10_combo_shaman.json",
		shaman, out_att, contextual_focus_locale_shaman, objectives, invalid_objs);
	test_to_fileB("Warlock", "F:\\doc-Alysson\\Projetos\\VisualStudio projects\\LogsCabra_Tratados\\10_combo_warlock.json",
		warlock, out_att, contextual_focus_locale_warlock, objectives, invalid_objs);
	test_to_fileB("Warrior", "F:\\doc-Alysson\\Projetos\\VisualStudio projects\\LogsCabra_Tratados\\10_combo_warrior.json",
		warrior, out_att, contextual_focus_locale_warrior, objectives, invalid_objs);
	
	/*test_to_fileA("Druid", "F:\\doc-Alysson\\Projetos\\VisualStudio projects\\LogsCabra_Tratados\\10_combo_druid.json", 
		net, out_att, contextual_focus_locale_druida, objectives, invalid_objs);
	test_to_fileA("Hunter", "F:\\doc-Alysson\\Projetos\\VisualStudio projects\\LogsCabra_Tratados\\10_combo_hunter.json",
		net, out_att, contextual_focus_locale_hunter, objectives, invalid_objs);
	test_to_fileA("Mage", "F:\\doc-Alysson\\Projetos\\VisualStudio projects\\LogsCabra_Tratados\\10_combo_mage.json",
		net, out_att, contextual_focus_locale_mage, objectives, invalid_objs);
	test_to_fileA("Paladin", "F:\\doc-Alysson\\Projetos\\VisualStudio projects\\LogsCabra_Tratados\\10_combo_paladin.json",
		net, out_att, contextual_focus_locale_paladin, objectives, invalid_objs);
	test_to_fileA("Priest", "F:\\doc-Alysson\\Projetos\\VisualStudio projects\\LogsCabra_Tratados\\10_combo_priest.json",
		net, out_att, contextual_focus_locale_priest, objectives, invalid_objs);
	test_to_fileA("Rogue", "F:\\doc-Alysson\\Projetos\\VisualStudio projects\\LogsCabra_Tratados\\10_combo_rogue.json",
		net, out_att, contextual_focus_locale_rogue, objectives, invalid_objs);*/
	//test_to_fileA("Shaman", "F:\\doc-Alysson\\Projetos\\VisualStudio projects\\LogsCabra_Tratados\\10_combo_shaman.json",
	//	net, out_att, contextual_focus_locale_shaman, objectives, invalid_objs);
	//test_to_fileA("Warlock", "F:\\doc-Alysson\\Projetos\\VisualStudio projects\\LogsCabra_Tratados\\10_combo_warlock.json",
	//	net, out_att, contextual_focus_locale_warlock, objectives, invalid_objs);
	//test_to_fileA("Warrior", "F:\\doc-Alysson\\Projetos\\VisualStudio projects\\LogsCabra_Tratados\\10_combo_warrior.json",
	//	net, out_att, contextual_focus_locale_warrior, objectives, invalid_objs);

	return 0;
}