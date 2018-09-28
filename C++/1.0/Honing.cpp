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

//-------------------------------DIRETIVAS-------------------------------------
#define OK								 0
#define HONING_BRIDGE					 1
#define HONING_INSERTION_OK				 2
#define HONING_REMOVAL_OK				 3
#define HONING_ALREADY_EXISTS			 4
#define HONING_SEED_INTERSECTION_FAILED -10005
#define HONING_INVALID_EXPANSION_LEVEL  -10004
#define HONING_INVALID_SEED			    -10003
#define HONING_REMOVAL_FAILED		    -10002
#define HONING_INSERTION_FAILED		    -10001
#define ELEMENT_NOT_FOUND			    -10000

//-------------------------------LOCAL INCLUDES--------------------------------
#include "Jzon.h"
#include "HeartStoneContextBuilder.h"

//-------------------------------GLOBAL INCLUDES-------------------------------
#include <map>
#include <vector>
#include <iostream>
#include <queue>
#include <cstdarg>
#include <stdio.h>      /* printf, scanf, puts, NULL */
#include <stdlib.h>     /* srand, rand */
#include <time.h>       /* time */

//-------------------------------MACROS----------------------------------------

//-------------------------------NAMESPACES------------------------------------
using namespace std;

//----------------------------------------------------------------------------------------------------------------------------

/*
Controladores de fluxo
*/
int honingIDs;

//----------------------------------------------------------------------------------------------------------------------------

/*
Assinaturas
*/
template <class T> class honingNode;
template <class T> class honingNet;
template <class T> class honingContext;

int randInt(int min, int max){ return min + (rand() % (int)(max - min + 1)); }

/*
Honing net algorithms
*/
template <class T> int recruitNeurds(
	honingNet<T>* net,
	T*            seed,
	int           level,
	vector<honingNode<T>*>&									 out_microfeatures,
	vector<honingNode<T>*>&									 out_seeds,
	vector<honingNode<T>*>&									 out_bridges);
template <class T> int findClique(
	honingNet<T>* net,
	T*            seed,
	vector<honingNode<T>*>&	out_seeds);
template <class T> bool findMicrofeature(map<T, honingNode<T>*>& bridges, vector<T>& microfeatures);
template <class T> bool intersectMicrofeature(map < T, honingNode<T>*>& bridges, vector<T>& microfeatures);
template <class T> int generateBridges(honingNet<T>* net, map<T, map<T, honingNode<T>*>>& out_bridges, map<T, map<T, honingNode<T>*>>& out_cliques);
template <class T> int generateInternalBridges(honingNet<T>* net, map<T, map<T, honingNode<T>*>>& out_bridges, map<T, map<T, honingNode<T>*>>& out_cliques);
template <class T> int recruitNeurds_microfeatureIntersection(
	honingNet<T>& net,
	T& seed,
	map<T, map<T, honingNode<T>*>>& bridges,
	map<T, map<T, honingNode<T>*>>& cliques,
	vector<T>& seed_features,
	vector<T>& out_features,
	vector<honingNode<T>*>& out_seeds);
template <class T> int recruitNeurds_restrictionChain(
	honingNet<T>& net,
	vector<T>& seeds,
	map<T, map<T, honingNode<T>*>>& bridges,
	map<T, map<T, honingNode<T>*>>& cliques,
	vector<vector<T>>& seed_chains,
	vector<vector<T>>& seed_excludedChains,
	vector<honingNode<T>*>& out_seeds,
	bool expandFromAllBridges = false);
template <class T> int post_expansion(
	map<T, honingNode<T>*>& input,
	map<T, map<T, honingNode<T>*>>& cliques,
	vector<vector<T>>& out_chains,
	vector<vector<T>>& out_excludedChains,
	vector<honingNode<T>*>& out_seeds);
template <class T> int populateCards(
	HeartStoneCardsLocals<T>& cardsContext,
	HeartStoneCardsLocals<T>& spellsContext,
	HeartStoneCardsLocals<T>& weaponsContext,
	honingNet<T>& net,
	map<T, map<T, honingNode<T>*>>& cliques);
template <class T> int populateScenarios(
	HeartStoneGamebuilder<T>& game,
	HeartStoneCardsLocals<T>& cards,
	HeartStoneCardsLocals<T>& spells,
	HeartStoneCardsLocals<T>& weapons,
	honingNet<T>& net,
	int scenarios,
	int maxPlayerCards,
	int minimumPlayerCards,
	int maxEnemyCards,
	int minimumEnemyCards,
	int maximumPlayerBfCards,
	int maximumPlayerBfSpells,
	int minimumPlyaerBfCards,
	int minimumPlayerBfSpells,
	int maximumEnemyBfCards,
	int maximumEnemyBfSpeels,
	int minimumEnemyBfCards,
	int minimumEnemyBfSpeels);
int readBattlefields(
	const char* file,
	int& start,
	map<string, map<string, honingNode<string>*>>& cliques,
	vector<vector<string>>& myTurns,
	vector<string>        & heroPlayed,
	vector<vector<string>>& enemyTurns,
	vector<vector<string>>& myCombos,
	vector<vector<string>>& enemyCombos);
template <class T> void padronizedTests
(
honingNet<T>* net,
map<string, map<string, honingNode<string>*>>& out_brid,
map<string, map<string, honingNode<string>*>>& out_cliques,
const char* battlefields, vector<vector<string>>& objectives,
int minimumComboThreshold,
int combosToGenerate,
map<int, map<string, vector<vector<string>>>>& combos_per_card,
map<int, int>&								  combos_found,
map<int, vector<string>>&					  combos_to_found);

/*
Basic routines
*/
unsigned int genHoningNodeID();
int initHoningLib();
int jzonReading(const char* file, honingNet < string >& out_map);

//----------------------------------------------------------------------------------------------------------------------------

/*
Permite criação de honing nodes
*/
template <class T>
class honingNode{
public:
	/*
	Construtor
	*/
	honingNode(bool autoID = false){
		// Deve ser automático
		if (autoID)
			ID = genHoningNodeID();

		// Deve ser instanciado
		children = new map<T, honingNode<T>*>();
		parents = new map<T, honingNode<T>*>();
	}

	/*
	Limpar mem local, deixa ref dos outros honing nodes
	*/
	~honingNode(void){
		// 1 - limpa sub listas
		(*children).clear();

		// 2 - limpa todos os parents.
		(*parents).clear();

		// 3 - limpa mem.
		delete children;
		delete parents;

		// 4 - reseta ponteiro
		children = 0x0;
		parents = 0x0;
	}

	int insertBridge(T ID){
		map<T, honingNode<T>*>::iterator it;

		it = (*bridges).find(ID);

		if (it == (*brigdes).end())
			return 0x0;


	}

	/*
	Insere filhos dado honingNode
	*/
	int insertChildren(honingNode<T>* obj){
		map<T, honingNode<T>*>::iterator it;

		it = (*children).find(obj->getId());

		if (it != (*children).end())
			return HONING_ALREADY_EXISTS;

		(*children)[obj->getId()] = obj;

		// Conf. parent no filho
		obj->insertParent(this);

		return OK;
	}

	/*
	Conf. ID
	*/
	int insertParent(honingNode<T>* parent){
		map<T, honingNode<T>*>::iterator it;

		it = (*this->parents).find(parent->getId());

		if (it != (*this->parents).end())
			return HONING_ALREADY_EXISTS;

		(*this->parents)[parent->getId()] = parent;

		// Conf. filho no parent
		parent->insertChildren(this);

		return OK;
	}

	/*
	Remove filho e deleta
	*/
	int removeChildrenAndErase(T ID){
		map<T, honingNode<T>*>::iterator it;

		it = (*children).find(ID);

		if (it == (*children).end())
			return ELEMENT_NOT_FOUND;

		// Self erase
		(*it).second->erase();

		return OK;
	}

	/*
	Remove parent dado ID
	*/
	int removeParentAndErase(T ID){
		map<T, honingNode<T>*>::iterator it;

		it = (*parents).find(ID);

		if (it == (*parents).end())
			return ELEMENT_NOT_FOUND;

		// Self erase
		(*it).second->erase();

		return OK;
	}


	/*
	Erase from network (LOCAL)
	*/
	int erase(){
		map<T, honingNode<T>*>::iterator itParent;

		itParent = parents->begin();
		for (; itParent != parents->end(); itParent++)
			(*itParent).second->removeChildren(ID);

		itParent = children->begin();
		for (; itParent != children->end(); itParent++)
			(*itParent).second->removeParent(ID);

		return OK;
	}

	/*
	Retorna ID
	*/
	T getId(){
		return ID;
	}

	/*
	Conf. ID
	*/
	int setId(T ID){
		this->ID = ID;
		return OK;
	}

	/*
	All parents func. ID
	*/
	map<T, honingNode<T>*>* getParents(){
		return parents;
	}

	/*
	All children func. ID
	*/
	map<T, honingNode<T>*>* getChildren(){
		return children;
	}

private:
	/*
	Remove filhos dado ID
	*/
	int removeChildren(T ID){
		map<T, honingNode<T>*>::iterator it;

		it = (*children).find(ID);

		if (it == (*children).end())
			return ELEMENT_NOT_FOUND;

		(*children).erase(ID);

		return OK;
	}

	/*
	Remove parent dado ID
	*/
	int removeParent(T ID){
		map<T, honingNode<T>*>::iterator it;

		it = (*parents).find(ID);

		if (it == (*parents).end())
			return ELEMENT_NOT_FOUND;

		(*parents).erase(ID);

		return OK;
	}


	map<T, honingNode<T>*>* children;
	map<T, honingNode<T>*>* parents;
	T ID;
};

//----------------------------------------------------------------------------------------------------------------------------

/*
Honing network
*/
template <class T>
class honingNet{
public:
	honingNet(){
		// Apenas instancia nodos
		honingNodeNet = new map<T, honingNode<T>*>();
		pathStatus = new map<T, map<T, bool > >();
		bridgeStatus = new map<T, map<T, bool > >();
	}

	~honingNet(){
		// 1 - destruir todos os nodos
		map<T, honingNode<T>*>::iterator itNet;
		itNet = honingNodeNet->begin();
		while (itNet != honingNodeNet->end()){
			// Limpa todos os honing nodes
			delete (*itNet).second;

			// limpa ponteiros
			(*itNet).second = 0x0;

			itNet++;
		}

		// 2 - libera toda a rede
		honingNodeNet->clear();
		pathStatus->clear();

		// 3 - pointer cleaning
		honingNodeNet = 0x0;
		pathStatus = 0x0;
	}

	int setParentHood(T* seed, T* child, bool status, bool bridge){
		map<T, honingNode<T>*>::iterator nodeIns;
		nodeIns = honingNodeNet->find((*seed));

		// 1 - caso nao tenha parente
		if (nodeIns == honingNodeNet->end())
			return HONING_INVALID_SEED;

		nodeIns = honingNodeNet->find((*child));

		if (nodeIns == honingNodeNet->end())
			return ELEMENT_NOT_FOUND;

		((*pathStatus)[(*child)])[(*seed)] = status;
		((*bridgeStatus)[(*child)])[(*seed)] = bridge;

		return OK;
	}

	bool getBPathStatus(honingNode<T>* seed, honingNode<T>* child){
		return ((*bridgeStatus)[child->getId()])[seed->getId()];
	}

	bool getPathStatus(honingNode<T>* seed, honingNode<T>* child){
		return ((*pathStatus)[child->getId()])[seed->getId()];
	}

	int insertHoningNode(T* node, T* parentID = 0x0){
		/*T - working ID */
		T workId = (*node);

		map<T, honingNode<T>*>::iterator nodeIns;
		nodeIns = honingNodeNet->find(workId);
		honingNode<T>* n_node;

		// 1 - caso nao tenha parente
		if (nodeIns == honingNodeNet->end()){
			n_node = new honingNode<T>();
			n_node->setId(workId);
			(*honingNodeNet)[workId] = n_node;
		}
		else
		{
			n_node = (*honingNodeNet)[workId];
		}

		if (parentID == 0x0)
			return HONING_INSERTION_OK;

		// 2 - tem parente
		map<T, honingNode<T>*>::iterator itInternalNet;
		itInternalNet = honingNodeNet->find((*parentID));

		if (itInternalNet == honingNodeNet->end()){
			honingNode<T>* n_p = new honingNode<T>();
			n_p->setId((*parentID));
			(*honingNodeNet)[(*parentID)] = n_p;
			n_p->insertChildren(n_node);

			return HONING_INSERTION_OK;
		}

		// 3 - insere filho
		(*itInternalNet).second->insertChildren(n_node);

		return HONING_INSERTION_OK;
	}

	int removeHoningNode(honingNode<T>* node){
		// 1 - verifica se pode remover
		map<T, honingNode<T>*>::iterator itInternalNet;
		itInternalNet = honingNodeNet->find((*node->getId()));

		if (itInternalNet == honingNodeNet->end())
			return HONING_REMOVAL_FAILED;

		// 2 - guarda ref da mem
		honingNode<T>* memToClean = (*honingNodeNet)[node->getId()];

		// 3 - remove dos microfeatures
		(*honingNodeNet).erase(node->getId());

		// 4 - varre todos os filhos e parents, remove referencias inuteis
		memToClean->erase();

		// 5 - limpa mem
		delete memToClean;

		// 6 - clean pointer
		memToClean = 0x0;

		return HONING_REMOVAL_OK;
	}

	bool find(T ID){
		map<T, honingNode<T>*>::iterator f_it;
		f_it = honingNodeNet->find(ID);

		if (f_it == honingNodeNet->end())
			return false;

		return true;
	}

	/*
	Get full network
	*/
	map<T, honingNode<T>*>* getNetwork(){
		return honingNodeNet;
	}

private:
	map<T, honingNode<T>*>* honingNodeNet;
	map<T, map<T, bool>  >* pathStatus;
	map<T, map<T, bool>  >* bridgeStatus;
};

//----------------------------------------------------------------------------------------------------------------------------

/*
Retorna todos os neuronios de ligação - Algoritmo Bridge Detection criado por Alysson R. Silva
*/
template <class T> int generateBridges(honingNet<T>* net, map<T, map<T, honingNode<T>*>>& out_bridges, map<T, map<T, honingNode<T>*>>& out_cliques){
	map<T, bool>* recFlags = new map<T, bool>();

	map<T, honingNode<T>*>::iterator net_it = net->getNetwork()->begin();
	for (; net_it != net->getNetwork()->end(); net_it++){
		// 1 - pega seed
		honingNode<T>* seedNode = (*net_it).second;

		// 2 - prepara workspace 
		vector<honingNode<T>*> workSpace;
		workSpace.push_back(seedNode);

		// Expansão (big bang)
		while (workSpace.size() > 0){
			honingNode<T>* seeker = workSpace.back();
			(*recFlags)[seeker->getId()] = true;

			// Basic opt
			workSpace.pop_back();

			// Achou terminal
			if (seeker->getId() != (*seedNode).getId()){
				(out_cliques[seedNode->getId()])[seeker->getId()] = seeker;
				if (net->getBPathStatus(seedNode, seeker) == true)
					(out_bridges[seedNode->getId()])[seeker->getId()] = seeker;
			}

			map<T, honingNode<T>*>::iterator itNet;
			map<T, honingNode<T>*>* i_net = seeker->getChildren();
			itNet = i_net->begin();
			for (; itNet != i_net->end(); itNet++){
				map<T, bool>::iterator flagCheck;
				T cId = ((*itNet).second->getId());
				flagCheck = recFlags->find(cId);

				if (flagCheck == recFlags->end())
					(*recFlags)[cId] = false;

				if ((*recFlags)[cId] == false &&
					net->getPathStatus(seedNode, (*itNet).second) == true/* DEVE TER PATH DIRETO */){
					workSpace.push_back((*itNet).second);
				}
			}// end for

			// --- CLEAN MEN ---
			seeker = 0x0;
			i_net = 0x0;
			// --- CLEAN MEN ---
		} // end while

		// final - clear all mem ---------------------------
		// cleanup
		recFlags->clear();
		seedNode = 0x0;
	}

	// free all
	delete recFlags;
	recFlags = 0x0;
	// final - clear all mem ---------------------------

	return OK;
}

/*
Retorna todos os neuronios de ligação - Algoritmo Bridge Detection criado por Alysson R. Silva
*/
template <class T> int generateInternalBridges(honingNet<T>* net, map<T, map<T, honingNode<T>*>>& out_bridges, map<T, map<T, honingNode<T>*>>& out_cliques){
	map<T, bool>* recFlags = new map<T, bool>();

	map<T, honingNode<T>*>::iterator net_it = net->getNetwork()->begin();
	for (; net_it != net->getNetwork()->end(); net_it++){
		// 1 - pega seed
		honingNode<T>* seedNode = (*net_it).second;

		map<T, map<T, honingNode<T>*>>::iterator s_it = out_cliques.find(seedNode->getId());
		if (s_it != out_cliques.end())
			continue;

		// 2 - prepara workspace 
		vector<honingNode<T>*> workSpace;
		workSpace.push_back(seedNode);

		// Expansão (big bang)
		while (workSpace.size() > 0){
			honingNode<T>* seeker = workSpace.back();
			(*recFlags)[seeker->getId()] = true;

			// Basic opt
			workSpace.pop_back();

			// Achou terminal
			if (seeker->getId() != (*seedNode).getId()){
				(out_cliques[seedNode->getId()])[seeker->getId()] = seeker;

				if (seeker->getChildren()->size() == 0)
					(out_bridges[seedNode->getId()])[seeker->getId()] = seeker;
			}

			map<T, honingNode<T>*>::iterator itNet;
			map<T, honingNode<T>*>* i_net = seeker->getChildren();
			itNet = i_net->begin();
			for (; itNet != i_net->end(); itNet++){
				map<T, bool>::iterator flagCheck;
				T cId = ((*itNet).second->getId());
				flagCheck = recFlags->find(cId);

				if (flagCheck == recFlags->end())
					(*recFlags)[cId] = false;

				if ((*recFlags)[cId] == false){
					workSpace.push_back((*itNet).second);
				}
			}// end for

			// --- CLEAN MEN ---
			seeker = 0x0;
			i_net = 0x0;
			// --- CLEAN MEN ---
		} // end while

		// final - clear all mem ---------------------------
		// cleanup
		recFlags->clear();
		seedNode = 0x0;
	}

	// free all
	delete recFlags;
	recFlags = 0x0;
	// final - clear all mem ---------------------------

	return OK;
}

//----------------------------------------------------------------------------------------------------------------------------

template <class T> bool intersectMicrofeature(map<T, honingNode<T>*>& bridges, vector<T>& microfeatures){
	for (int i = 0; i < microfeatures.size(); i++){
		map<T, honingNode<T>*>::iterator it_ser;
		it_ser = bridges.find(microfeatures[i]);
		if (it_ser == bridges.end())
			return false;
	}
	return true;
}

//----------------------------------------------------------------------------------------------------------------------------

template <class T> bool findMicrofeature(map<T, honingNode<T>*>& bridges, vector<T>& microfeatures){
	for (int i = 0; i < microfeatures.size(); i++){
		map<T, honingNode<T>*>::iterator it_ser;
		it_ser = bridges.find(microfeatures[i]);
		if (it_ser != bridges.end())
			return true;
	}
	return false;
}

/*
Retorna todos os neurds dados interseção de MF - Algoritmo Microfeature DirectPath criado por Alysson R. Silva
*/
template <class T> int recruitNeurds_microfeatureIntersection(
	/* INPUTS */
	honingNet<T>& net,
	T& seed,
	map<T, map<T, honingNode<T>*>>& bridges,
	map<T, map<T, honingNode<T>*>>& cliques,
	vector<T>& seed_features,
	vector<T>& out_features,

	/* OUTPUTS */
	vector<honingNode<T>*>& out_seeds){

	map<T, honingNode<T>*>* netRef = net.getNetwork();
	map<T, honingNode<T>*>::iterator it_seeker = netRef->find(seed);

	if (it_seeker == netRef->end())
		return HONING_INVALID_SEED;

	honingNode<T>* seedNode = (*net.getNetwork())[seed];
	if (!intersectMicrofeature(bridges[seedNode->getId()], seed_features))
		return HONING_SEED_INTERSECTION_FAILED;

	map<T, map<T, honingNode<T>*>>::iterator it_bridges = bridges.begin();
	for (; it_bridges != bridges.end(); it_bridges++){
		if (findMicrofeature((*it_bridges).second, seed_features) &&
			findMicrofeature(cliques[(*it_bridges).first], out_features) &&
			(*it_bridges).first != seedNode->getId()){
			out_seeds.push_back((*net.getNetwork())[(*it_bridges).first]);
		}
	}

	return OK;
}

//----------------------------------------------------------------------------------------------------------------------------

template <class T> bool checkChain(map<T, honingNode<T>*>& clique, vector<T>& chain){
	// error conditions
	if (chain.size() <= 0 || clique.size() <= 0)
		return false;

	// Chain controller
	unsigned int _chain = 0;

	// 1 - check if chain exists
	map<T, honingNode<T>*>::iterator c_it;

	// 2 - try to find item
	c_it = clique.find(chain[_chain++]);

	if (c_it == clique.end())
		return false;

	vector<honingNode<T>*> workSpace;
	workSpace.push_back((*c_it).second);

	while (_chain < chain.size()){
		honingNode<T>& work = *workSpace.back();
		workSpace.pop_back();

		map<T, honingNode<T>*>& children = *work.getChildren();
		map<T, honingNode<T>*>::iterator ic_it;
		ic_it = children.find(chain[_chain]);

		if (ic_it == children.end())
			return false;

		ic_it = clique.find(chain[_chain++]);

		if (ic_it == clique.end())
			return false;

		// prevent push_back
		if (_chain == chain.size())
			return true;

		workSpace.push_back((*ic_it).second);
	}

	return true;
}

//----------------------------------------------------------------------------------------------------------------------------

template <class T> int post_expansion(
	map<T, honingNode<T>*>& input,
	map<T, map<T, honingNode<T>*>>& cliques,
	vector<vector<T>>& out_chains,
	vector<vector<T>>& out_excludedChains,

	/* OUTPUTS */
	vector<honingNode<T>*>& out_seeds){

	// Filter output and update out_seeds
	map<T, honingNode<T>*>::iterator ret_it = input.begin();
	for (; ret_it != input.end(); ret_it++){
		map<T, honingNode<T>*>& out_seed_clique = cliques[(*ret_it).second->getId()];

		bool c_in = false;
		bool c_nIn = false;

		if (out_chains.size() == 0)
			c_in = true;
		else
			for (unsigned int j = 0; j < out_chains.size(); j++){
				if (checkChain<T>(out_seed_clique, out_chains[j])){
					c_in = true;
					break;
				}
			}// end for

		for (unsigned int j = 0; j < out_excludedChains.size(); j++){
			if (checkChain<T>(out_seed_clique, out_excludedChains[j])){
				c_nIn = true;
				break;
			}
		}// end for

		if (c_in && !c_nIn)
			out_seeds.push_back((*ret_it).second);
	}// end for

	return OK;
}

//----------------------------------------------------------------------------------------------------------------------------

template <class T> void  recruitNeurdsSimplified(
	honingNet<T>& net,
	vector<T>& seeds,
	vector<T>& objectives,
	vector<T>& bridges,
	vector<T>& raw_output){

	map<T, honingNode<T>*>* network = net.getNetwork();

	vector<honingNode<T>*> workspace;
	map<T, honingNode<T>*> allBridges;
	vector<honingNode<T>*> selectedBridges;
	map<T, honingNode<T>*> selectedParents;

	// select reliable seeds
	for (int i = 0; i < seeds.size(); i++){

	}
}

//----------------------------------------------------------------------------------------------------------------------------

template <class T> int recruitNeurds_restrictionChain(
	/* INPUTS */
	honingNet<T>& net,
	vector<T>& seeds,
	map<T, map<T, honingNode<T>*>>& bridges,
	map<T, map<T, honingNode<T>*>>& cliques,
	vector<vector<T>>& seed_chains,
	vector<vector<T>>& seed_excludedChains,

	/* OUTPUTS */
	map<T, honingNode<T>*>& out_parents,
	bool expandFromAllBridges){

	map<T, honingNode<T>*>* netRef = net.getNetwork();
	//map<T, honingNode<T>*> out_parents;
	vector<honingNode<T>*> workPlace;
	map<T, bool		     >* recFlags = new map<T, bool>();

	for (unsigned int i = 0; i < seeds.size(); i++){
		string seed = seeds[i];
		map<T, honingNode<T>*>::iterator it_seeker = netRef->find(seed);

		if (it_seeker == netRef->end())
			return HONING_INVALID_SEED;

		// Algorithm start - bridge pickup phase
		honingNode<T>* seedNode = (*net.getNetwork())[seed];
		map<T, honingNode<T>*>& seedClique = cliques[seed];
		map<T, honingNode<T>*>& seed_bridges = bridges[seed];

		if (expandFromAllBridges){
			map < T, honingNode<T>* >::iterator b_it;
			b_it = seed_bridges.begin();
			for (; b_it != seed_bridges.end(); b_it++)
				workPlace.push_back((*b_it).second);
		}
		else{
			if (seedClique.size() > 0){
				// Verify seed chains
				for (unsigned int i = 0; i < seed_chains.size(); i++){
					if (!checkChain<T>(seedClique, seed_chains[i]))
						continue;

					//use for to mount bridges to expand
					map < T, honingNode<T>* >::iterator b_it;
					b_it = seed_bridges.find(seed_chains[i][seed_chains[i].size() - 1]);
					if (b_it != seed_bridges.end())
						workPlace.push_back(seed_bridges[seed_chains[i][seed_chains[i].size() - 1]]);
				}

				for (unsigned int i = 0; i < seed_excludedChains.size(); i++)
					if (checkChain<T>(seedClique, seed_excludedChains[i]))
						return HONING_INVALID_SEED;
			}
			else
			{
				workPlace.push_back(seedNode);
			}

			// -- clean men ---
			seedNode = 0x0;
			//	net		 = 0x0;
			// -- clean men --
		}//end else
	}

	// mirror seed peekup
	while (workPlace.size() > 0){
		honingNode<T>* retractionSeeker = workPlace.back();
		workPlace.pop_back();

		// fim de expansão
		if ((*(*retractionSeeker).getParents()).size() == 0){
			map<T, honingNode<T>*>::iterator s_it = out_parents.find(retractionSeeker->getId());
			if (s_it == out_parents.end())
				out_parents[retractionSeeker->getId()] = retractionSeeker;
		}

		// conf. ctrl flag
		(*recFlags)[retractionSeeker->getId()] = true;

		map<T, honingNode<T>*>::iterator itRetractionNet;
		map<T, honingNode<T>*>* net = retractionSeeker->getParents();

		itRetractionNet = net->begin();
		for (; itRetractionNet != net->end(); itRetractionNet++){
			map<T, bool>::iterator flagCheck;
			T cId = (*itRetractionNet).second->getId();
			flagCheck = recFlags->find(cId);

			if (flagCheck == recFlags->end())
				(*recFlags)[cId] = false;

			if ((*recFlags)[cId] == false)
				workPlace.push_back((*itRetractionNet).second);
		}// end for

		//---CLEAN mem---
		retractionSeeker = 0x0;
		net = 0x0;
		//---CLEAN mem---
	}// end retração

	return OK;
}

//----------------------------------------------------------------------------------------------------------------------------

/*
Retorna lista de seeds - Algoritmo Microfeature Expansion criado por Alysson R. Silva
*/
template <class T> int recruitNeurds
(/* INPUTS*/ honingNet<T>*  net,
T*            seed,
int          level,

/*OUTPUTS*/
vector<honingNode<T>*>&	out_microfeatures,
vector<honingNode<T>*>& out_seeds,
vector<honingNode<T>*>&	out_bridges){

	// Para buscar deve satisfazer os criterios de seed (1) nao ter parentes e (2) ser um microfeature

	if (seed == 0x0)
		return HONING_INVALID_SEED;

	// Check todos os outputs
	//if (out_microfeatures == 0x0)
	//	out_microfeatures = new vector<honingNode<T>*>();

	//if (out_seeds == 0x0)
	//	out_seeds = new vector<honingNode<T>*>();

	// 1 - pega seed
	honingNode<T>* seedNode = (*(*net).getNetwork())[(*seed)];

	// 2 - prepara flags de controle de recursao
	map<T, bool>* recFlags = new map<T, bool>();

	// 3 - prepara controlador de nivel
	map<T, int>* max_levels = new map<T, int>();
	(*max_levels)[(*seedNode).getId()] = 0; // ref level

	// 4 - prepara workspace 
	vector<honingNode<T>*> workSpace;
	workSpace.push_back(seedNode);

	// Levels node
	map<int, map<T, honingNode<T>*>>* features = new map < int, map<T, honingNode<T>*>>();

	// Expansão (big bang)
	while (workSpace.size() > 0){
		honingNode<T>* seeker = workSpace.back();
		(*recFlags)[seeker->getId()] = true;

		// Basic opt
		workSpace.pop_back();

		if (seeker->getId() != (*seed))
			out_microfeatures.push_back(seeker);

		// Seek sons
		map<T, honingNode<T>*>::iterator itNet;
		map<T, honingNode<T>*>* i_net = seeker->getChildren();

		itNet = i_net->begin();

		// Achou terminal
		if (itNet == i_net->end() &&
			seeker->getId() != (*seed))
			((*features)[(*max_levels)[seeker->getId()]])[seeker->getId()] = seeker;

		for (; itNet != i_net->end(); itNet++){
			map<T, bool>::iterator flagCheck;
			T cId = ((*itNet).second->getId());
			flagCheck = recFlags->find(cId);

			if (flagCheck == recFlags->end())
				(*recFlags)[cId] = false;

			if ((*recFlags)[cId] == false &&
				net->getPathStatus(seedNode, (*itNet).second) == true/* DEVE TER PATH DIRETO */){
				// Conf. Level
				map<T, int>::iterator levelIt;
				levelIt = max_levels->find((*itNet).second->getId());

				if (levelIt == max_levels->end()){
					(*max_levels)[cId] = (*max_levels)[seeker->getId()] + 1;
				}

				workSpace.push_back((*itNet).second);
			}
		}// end for

		// --- CLEAN MEN ---
		seeker = 0x0;
		i_net = 0x0;
		// --- CLEAN MEN ---
	} // end while

	// workspace de retração
	vector<honingNode<T>*> retractionWorkspace;

	// retração (big crunch)
	if (level == -1){
		map<int, map<T, honingNode<T>*>>::iterator itFeatures;
		itFeatures = features->begin();
		for (; itFeatures != features->end(); itFeatures++){
			map<T, honingNode<T>*>::iterator internalNet;
			internalNet = (*itFeatures).second.begin();
			for (; internalNet != (*itFeatures).second.end(); internalNet++){
				retractionWorkspace.push_back((*internalNet).second);

				// Conf. bridges
				out_bridges.push_back((*internalNet).second);
			}
		}// end for
	}// end if
	else{
		map<int, map<T, honingNode<T>*>>::iterator itFeatures;
		itFeatures = features->find(level);

		// Nada para fazer
		if (itFeatures == features->end())
			return HONING_INVALID_EXPANSION_LEVEL;

		map<T, honingNode<T>*>::iterator internalNet;
		internalNet = (*features)[level].begin();
		for (; internalNet != (*features)[level].end(); internalNet++){
			retractionWorkspace.push_back((*internalNet).second);

			// Conf. bridges
			out_bridges.push_back((*internalNet).second);
		}
	}

	// inicio retração
	while (retractionWorkspace.size() > 0){
		honingNode<T>* retractionSeeker = retractionWorkspace.back();
		retractionWorkspace.pop_back();

		// Insere microfeature
		if ((*recFlags)[retractionSeeker->getId()] == false &&
			(*(*retractionSeeker).getParents()).size() != 0)
			out_microfeatures.push_back(retractionSeeker);

		// fim de expansão
		if ((*(*retractionSeeker).getParents()).size() == 0)
			out_seeds.push_back(retractionSeeker);

		// conf. ctrl flag
		(*recFlags)[retractionSeeker->getId()] = true;

		map<T, honingNode<T>*>::iterator itRetractionNet;
		map<T, honingNode<T>*>* net = retractionSeeker->getParents();

		itRetractionNet = net->begin();
		for (; itRetractionNet != net->end(); itRetractionNet++){
			map<T, bool>::iterator flagCheck;
			T cId = (*itRetractionNet).second->getId();
			flagCheck = recFlags->find(cId);

			if (flagCheck == recFlags->end())
				(*recFlags)[cId] = false;

			if ((*recFlags)[cId] == false)
				retractionWorkspace.push_back((*itRetractionNet).second);
		}// end for

		//---CLEAN mem---
		retractionSeeker = 0x0;
		net = 0x0;
		//---CLEAN mem---
	}// end retração

	//--------------------------------------- FIM EXPANSÃO -------------------------------------

	// final - clear all mem ---------------------------
	// 1 - cleanup
	recFlags->clear();
	max_levels->clear();
	features->clear();

	// 2 - free all
	delete recFlags;
	delete max_levels;
	delete features;

	// 3 - clean pointers
	seedNode = 0x0;
	recFlags = 0x0;
	max_levels = 0x0;
	features = 0x0;
	// final - clear all mem ---------------------------

	// FIM algoritmo
	return OK;
}

//----------------------------------------------------------------------------------------------------------------------------

/*
Retorna clique completo - Algoritmo Clique Extraction criado por Alysson R.Silva
*/
template <class T> int findClique(
	/* INPUTS */
	honingNet<T>* net,
	T*            seed,

	/* OUTPUTS */
	vector<honingNode<T>*>&	out_seeds){
	if (seed == 0x0)
		return HONING_INVALID_SEED;

	// Find seed
	if (!net->find((*seed)))
		return HONING_INVALID_SEED;

	// 1 - pega seed
	honingNode<T>* seedNode = (*(*net).getNetwork())[(*seed)];

	//if (out_seeds == 0x0)
	//	out_seeds = new vector<honingNode<T>*>();

	// 2 - prepara flags de controle de recursao
	map<T, bool>* recFlags = new map<T, bool>();

	/* Expansão simples */
	vector<honingNode<T>*> extractionPool;
	extractionPool.push_back(seedNode);

	while (extractionPool.size() > 0){
		honingNode<T>* extracted = extractionPool.back();
		extractionPool.pop_back();

		(*recFlags)[extracted->getId()] = true;

		if (extracted->getId() != (*seed))
			out_seeds.push_back(extracted);

		map<T, honingNode<T>*>::iterator extractionIt;
		extractionIt = extracted->getChildren()->begin();
		for (; extractionIt != extracted->getChildren()->end(); extractionIt++){
			map<T, bool>::iterator flagCheck;
			flagCheck = (*recFlags).find((*extractionIt).second->getId());

			if (flagCheck == recFlags->end())
				(*recFlags)[(*extractionIt).second->getId()] = false;

			if ((*recFlags)[(*extractionIt).second->getId()] == false &&
				net->getPathStatus(seedNode, (*extractionIt).second) == true/* DEVE TER PATH DIRETO */)
				extractionPool.push_back((*extractionIt).second);
		}

		// --- clean mem ---
		extracted = 0x0;
		// --- clean mem ---
	}

	// --- limpa toda mem ---
	// clear reg
	recFlags->clear();

	// free mem
	delete recFlags;

	// clean ptr
	recFlags = 0x0;
	// --- limpa toda mem ---

	return OK;
}

//----------------------------------------------------------------------------------------------------------------------------

template <class T> int populateCards(
	HeartStoneCardsLocals<T>& cardsContext,
	HeartStoneCardsLocals<T>& spellsContext,
	HeartStoneCardsLocals<T>& weaponsContext,
	honingNet<T>& net,
	map<T, map<T, honingNode<T>*>>& cliques){

	map<T, honingNode<T>*>::iterator netIt;
	netIt = net.getNetwork()->begin();

	for (; netIt != net.getNetwork()->end(); netIt++){
		honingNode<T>& currentNode = (*(*netIt).second);

		map<T, honingNode<T>*>& clique = cliques[currentNode.getId()];

		vector<T> checkChainSpell; checkChainSpell.push_back("spell");
		vector<T> checkChainMinion; checkChainMinion.push_back("minion");

		if (currentNode.getParents()->size() == 0){
			if (checkChain(clique, checkChainSpell))
				spellsContext.cards.push_back(currentNode.getId());
			else{
				if (checkChain(clique, checkChainMinion))
					cardsContext.cards.push_back(currentNode.getId());
				else
					weaponsContext.cards.push_back(currentNode.getId());
			}
		}
	}

	return OK;
}

//----------------------------------------------------------------------------------------------------------------------------

template <class T> int populateScenarios(
	HeartStoneGamebuilder<T>& game,
	HeartStoneCardsLocals<T>& cards,
	HeartStoneCardsLocals<T>& spells,
	HeartStoneCardsLocals<T>& weapons,
	honingNet<T>& net,
	int scenarios,
	int maxPlayerCards,
	int minimumPlayerCards,
	int maxEnemyCards,
	int minimumEnemyCards,
	int maximumPlayerBfCards,
	int maximumPlayerBfSpells,
	int minimumPlyaerBfCards,
	int minimumPlayerBfSpells,
	int maximumEnemyBfCards,
	int maximumEnemyBfSpeels,
	int minimumEnemyBfCards,
	int minimumEnemyBfSpeels){

	int totalCards = cards.cards.size();
	int totalSpells = spells.cards.size();
	int totalWeapons = weapons.cards.size();

	for (int i = 0; i < scenarios; i++){
		int playerCards = randInt(minimumPlayerCards, maxPlayerCards);
		int playerBfCards = randInt(minimumPlyaerBfCards, maximumPlayerBfCards);
		int playerSpells = randInt(minimumPlayerBfSpells, maximumPlayerBfSpells);

		int enemyCards = randInt(minimumEnemyCards, maxEnemyCards);
		int enemyBfCards = randInt(minimumEnemyBfCards, maximumEnemyBfCards);
		int enemySpells = randInt(minimumEnemyBfSpeels, maximumEnemyBfSpeels);

		if (playerCards == 0 || enemyBfCards == 0){
			i--;
			continue;
		}

		HeartStoneContext<T> newContext;

		if (totalCards > 0){
			for (int H = 0; H < playerCards; H++){
				int randCard = randInt(0, totalCards - 1); if (randCard < 0) randCard = 0;
				T card = cards.cards.at(randCard);
				newContext.myHand.push_back(card);
			}

			for (int H = 0; H < playerBfCards; H++){
				int randCard = randInt(0, totalCards - 1); if (randCard < 0) randCard = 0;
				T card = cards.cards.at(randCard);
				newContext.myBattleField.push_back(card);
			}

			for (int H = 0; H < enemyCards; H++){
				int randCard = randInt(0, totalCards - 1); if (randCard < 0) randCard = 0;
				T card = cards.cards.at(randCard);
				newContext.enemyHand.push_back(card);
			}

			for (int H = 0; H < enemyBfCards; H++){
				int randCard = randInt(0, totalCards - 1); if (randCard < 0) randCard = 0;
				T card = cards.cards.at(randCard);
				newContext.enemyBattleField.push_back(card);
			}
		}// end if

		if (totalSpells > 0){
			if (newContext.myHand.size() + (unsigned int)playerSpells >= (unsigned int)maxPlayerCards){
				for (unsigned int H = 0; H < newContext.myHand.size() + playerSpells - maxPlayerCards; H++)
					newContext.myHand.pop_back();
			}

			if (newContext.enemyHand.size() + (unsigned int)enemySpells >= (unsigned int)maxEnemyCards){
				for (unsigned int H = 0; H < newContext.enemyHand.size() + enemySpells - maxEnemyCards; H++)
					newContext.enemyHand.pop_back();
			}

			for (int H = 0; H < playerSpells; H++){
				int randCard = randInt(0, totalSpells - 1); if (randCard < 0) randCard = 0;
				T card = spells.cards.at(randCard);
				newContext.myHand.push_back(card);
			}

			for (int H = 0; H < enemySpells; H++){
				int randCard = randInt(0, totalSpells - 1); if (randCard < 0) randCard = 0;
				T card = spells.cards.at(randCard);
				newContext.enemyHand.push_back(card);
			}
		}// end if

		//insert into the general context
		game.scenarios[i + 1] = newContext;

	}//end for

	return OK;
}

//----------------------------------------------------------------------------------------------------------------------------

/*
Permite gerar identificadores para honing nodes
*/
unsigned int genHoningNodeID(){
	int ret = honingIDs;
	honingIDs++;
	return ret;
}

//----------------------------------------------------------------------------------------------------------------------------

class honingComp
{
	bool reverse;
public:
	honingComp(const bool& revparam = false)
	{
		reverse = revparam;
	}
	bool operator() (const pair<string, int>& lhs, const pair<string, int>& rhs) const
	{
		if (reverse) return (lhs.second > rhs.second);
		else return (lhs.second < rhs.second);
	}
};

//----------------------------------------------------------------------------------------------------------------------------

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

//----------------------------------------------------------------------------------------------------------------------------

template <class T> void padronizedTests
(
honingNet<T>* net, 
map<string, map<string, honingNode<string>*>>& out_brid, 
map<string, map<string, honingNode<string>*>>& out_cliques, 
const char* battlefields, vector<vector<string>>& objectives, 
int minimumComboThreshold, 
int combosToGenerate,
map<int, map<string, vector<vector<string>>>>& combos_per_card,
map<int, int>&								  combos_found,
map<int, vector<string>>&					  combos_to_found){
	// populate battlefields
	vector<vector<string>> myBattlefields;
	vector<vector<string>> enemyBattlefields;
	vector<string>		   herosPlayed;
	vector<vector<string>> myCombo;
	vector<vector<string>> enemyCombo;
	int startPlayer;

	readBattlefields(battlefields, startPlayer, out_cliques, myBattlefields, herosPlayed, enemyBattlefields, myCombo, enemyCombo);

	// start tests
	int i = 0;

	if (startPlayer != 1)
		i = 1;

	for (; i < myBattlefields.size(); i += 2)
	{
		if (myCombo[i].size() != minimumComboThreshold)
			continue;

		vector<vector<string>> chain_search;
		vector<vector<string>> excludeChain;

		map<string, string> all_bridges;

		// preparing bridges
		for (int m = 0; m < myBattlefields[i].size(); m++){
			map<string, honingNode<string>*>::iterator internal_it;
			internal_it = out_cliques[myBattlefields[i][m]].begin();
			for (; internal_it != out_cliques[myBattlefields[i][m]].end(); internal_it++){
				map<string, string>::iterator it_seeker;
				it_seeker = all_bridges.find((*internal_it).first);
				if (it_seeker == all_bridges.end())
					all_bridges[(*internal_it).first] = (*internal_it).first;
			}
		}

		for (int m = 0; m < enemyBattlefields[i].size(); m++){
			map<string, honingNode<string>*>::iterator internal_it;
			internal_it = out_cliques[enemyBattlefields[i][m]].begin();
			for (; internal_it != out_cliques[enemyBattlefields[i][m]].end(); internal_it++){
				map<string, string>::iterator it_seeker;
				it_seeker = all_bridges.find((*internal_it).first);
				if (it_seeker == all_bridges.end())
					all_bridges[(*internal_it).first] = (*internal_it).first;
			}
		}

		// converting bridges
		map<string, string>::iterator it_brid;
		it_brid = all_bridges.begin();
		for (; it_brid != all_bridges.end(); it_brid++){
			vector<string> brid;
			brid.push_back((*it_brid).second);
			chain_search.push_back(brid);
		}

		// test for each card from the combo set
		//for (int j = 0; j < myCombo[i].size(); j++){
		map<string, honingNode<string>*> raw;

		vector<string> seeds;

		//Preparing seeds
		seeds.push_back(myCombo[i][0]);
		//seeds.push_back(objectives[objectives.size() - 1]);

		recruitNeurds_restrictionChain<string>(
			*net,						 // network
			seeds,						 // seeds to expand
			out_brid,					 // conection bridges
			out_cliques,				 // cliques
			chain_search,                // o que tem que ter no seed
			excludeChain,				 // o que nao deve ter no seed
			raw,						 // output
			false);						 // expansion type (true = all bridges)

		vector<vector<string>> chain_output;
		vector<vector<string>> excludeChain_output;

		//chain_output.push_back(objectives);

		vector<honingNode<string>*> output;

		post_expansion<string>(
			raw,					     // raw input
			out_cliques,				 // cliques
			objectives,                  // o que deve ter no output
			excludeChain_output,         // o que nao deve ter no output
			output);					 // real output

		//Generate and store combo for that seed from that turn
		for (int t = 0; t < combosToGenerate; t++){
			// selected combos
			vector<string> real_output;

			real_output.push_back(myCombo[i][0]);

			//TREAT COMBOS
			for (int g = 0; g < minimumComboThreshold - 1 /* self add */; g++){
				int rand_combo = randInt(0, output.size() - 1);
				if (rand_combo < 0) rand_combo = 0;
				real_output.push_back(output[rand_combo]->getId());
			}

			if (match_vector(myCombo[i], real_output))
				combos_found[i]++;

			combos_per_card[i][myCombo[i][0]].push_back(real_output);
		}// end for

		// prepare output to validate
		combos_to_found[i] = (myCombo[i]);
	}//end for
}

//-----------------------------------------------------------------------------------------------------------------------------

/*
Trate toda inicialização da honingLib
*/
int initHoningLib(){
	honingIDs = 0;

	return OK;
}

//----------------------------------------------------------------------------------------------------------------------------
int jzonReading(const char* file, honingNet < string >& out_map){
	/* MAP-ING */
	//if (out_map == 0x0)
	//	out_map = new honingNet< string >();

	// JSON reading
	Jzon::Array rootNode;
	Jzon::FileReader::ReadFile(file, rootNode);

	for (Jzon::Array::iterator it = rootNode.begin(); it != rootNode.end(); ++it)
	{
		Jzon::Object fullNode = (*it).AsObject();
		Jzon::Object::iterator obj_it = fullNode.begin();

		/* Nome */
		string card_name = (*obj_it).second.AsValue().ToString();
		obj_it++;

		/* Text */
		string card_text = (*obj_it).second.AsValue().ToString();
		obj_it++;

		/* INSERT CARD */
		out_map.insertHoningNode(&card_name);

		/* Array */
		Jzon::Array::iterator microfeatures_it = (*obj_it).second.AsArray().begin();
		for (; microfeatures_it != (*obj_it).second.AsArray().end(); microfeatures_it++){

			Jzon::Object::iterator m_level = (*microfeatures_it).AsObject().begin();
			string father = ((*m_level).second.AsValue().ToString());

			out_map.insertHoningNode(&father, &card_name);

			// Set path status - 1
			bool bridge = false;
			Jzon::Object::iterator m_level_a = m_level; m_level_a++;
			if (m_level_a == (*microfeatures_it).AsObject().end())
				bridge = true;
			out_map.setParentHood(&card_name, &father, true, bridge);

			// -------------------------- BUFERIZAÇÃO ---------------------------------
			/*cliques[card_name][father] = (*out_map.getNetwork())[father];

			Jzon::Object::iterator m_level_a = m_level; m_level_a++;
			if (m_level_a == (*microfeatures_it).AsObject().end())
			out_brid[card_name][father] = (*out_map.getNetwork())[father];*/
			// -------------------------- BUFERIZAÇÃO ---------------------------------

			Jzon::Object::iterator m_level_n = (*microfeatures_it).AsObject().begin().operator++();
			for (; m_level_n != (*microfeatures_it).AsObject().end(); m_level_n++){
				string me = ((*m_level_n).second.AsValue().ToString());
				out_map.insertHoningNode(&me, &father);

				// Conf. path status - 2
				bridge = false;
				Jzon::Object::iterator m_level_a = m_level_n; m_level_a++;
				if (m_level_a == (*microfeatures_it).AsObject().end())
					bridge = true;
				out_map.setParentHood(&card_name, &me, true, bridge);

				// -------------------------- BUFERIZAÇÃO ---------------------------------
				/*cliques[card_name][me] = (*out_map.getNetwork())[me];

				Jzon::Object::iterator m_level_a = m_level_n; m_level_a++;
				if (m_level_a == (*microfeatures_it).AsObject().end())
				out_brid[card_name][me] = (*out_map.getNetwork())[me];*/
				// -------------------------- BUFERIZAÇÃO ---------------------------------

				// Conf. last conf.
				m_level = m_level_n;
				father = ((*m_level).second.AsValue().ToString());
			}
		}//end for
	}// end for

	return OK;
}

//----------------------------------------------------------------------------------------------------------------------------

int readBattlefields(
	const char* file,
	int& start,
	map<string, map<string, honingNode<string>*>>& cliques,
	vector<vector<string>>& myTurns,
	vector<string>        & heroPlayed,
	vector<vector<string>>& enemyTurns,
	vector<vector<string>>& myCombos,
	vector<vector<string>>& enemyCombos){

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

	// Check restr.
	vector<string> chainCheck; chainCheck.push_back("spell"); chainCheck.push_back("weapon");

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

			if (card == "Shapeshift")
				continue;

			if (myBF.size() < 1)
				curMyBF[card] = card;
			else{
				map<string, string>::iterator seeker;
				seeker = myBF[currentBF - 1].find(card);

				map<string, honingNode<string>*>& out_seed_clique = cliques[card];
				if (checkChain(out_seed_clique, chainCheck)){
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

			if (card == "Shapeshift")
				continue;

			if (enemyBf.size() < 1)
				curEnBF[card] = card;
			else{
				map<string, string>::iterator seeker;
				seeker = enemyBf[currentBF - 1].find(card);

				map<string, honingNode<string>*>& out_seed_clique = cliques[card];
				if (checkChain(out_seed_clique, chainCheck)){
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

			if (card == "Shapeshift")
				continue;

			combos.push_back(card);
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
		if (currentPlayer == 1)
			currentPlayer = 2;
		else
			currentPlayer = 1;

		// error handler
		currentBF++;
	}

	return OK;
}

/*
Execução de testes
*/
int main(int argc, char* argv[]){
	srand((unsigned int)time(NULL));

	vector<vector<string>> objectives;
	vector<string> a;
	a.push_back("deal");
	a.push_back("damage");
	a.push_back("hero");

	vector<string> b;
	b.push_back("give");
	b.push_back("health");
	b.push_back("minion");

	vector<string> e;
	e.push_back("spell");

	vector<string> f;
	f.push_back("taunt");

	vector<string> c;
	c.push_back("deal");
	c.push_back("damage");
	c.push_back("minion");

	vector<string> d;
	d.push_back("give");
	d.push_back("attack");
	d.push_back("minion");

	objectives.push_back(a);
	objectives.push_back(b);
	objectives.push_back(c);
	objectives.push_back(d);
	objectives.push_back(e);
	objectives.push_back(f);

	map<int, map<string, vector<vector<string>>>> combos_per_card;
	map<int, int>								  combos_found;
	map<int, vector<string>>					  combos_to_found;

	// create network
	honingNet<string>* net = new honingNet<string>();
	jzonReading("F:\\doc-Alysson\\Projetos\\VisualStudio projects\\Honing\\Debug\\microfeature.json", *net);

	// bufferization
	map<string, map<string, honingNode<string>*>> out_brid;
	map<string, map<string, honingNode<string>*>> out_cliques;

	generateBridges		   <string>(net, out_brid, out_cliques);
	generateInternalBridges<string>(net, out_brid, out_cliques);

	padronizedTests(
		net,
		out_brid, 
		out_cliques,
		"F:\\doc-Alysson\\Projetos\\VisualStudio projects\\Honing\\Debug\\json17.json", 
		objectives, 
		3, 
		1000000,
		combos_per_card,
		combos_found,
		combos_to_found);

	return OK;
}