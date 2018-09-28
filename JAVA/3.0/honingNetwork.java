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

/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package honing.java;

import java.util.ArrayList;
import java.util.HashMap;

/**
 *
 * @author Alysson
 * @param <T>
 */
public class honingNetwork<T> {

    public honingNetwork() {
        network = new HashMap<>();
    }
    
     public boolean checkMicrofeature(T seed, T microfeature) {
        if (!network.containsKey(seed)) {
            return false;
        }

        boolean check = false;

        honingNode<T> node = network.get(seed);

        if (node.clique.containsKey(microfeature)) {
            check = true;
        }

        return check;
    }

    public boolean match_vector(ArrayList<T> A, ArrayList<T> B) {
        if (A.size() != B.size()) {
            return false;
        }

        for (int i = 0; i < A.size(); i++) {
            boolean check = false;

            for (int j = 0; j < B.size(); j++) {
                if (A.get(i) == B.get(j)) {
                    check = true;
                    break;
                }

                if (check == false) {
                    return false;
                }
            }

        }

        return true;
    }

    public void insert(T obj) {
        if (!network.containsKey(obj)) {
            network.put(obj, new honingNode<>(obj));
        }
    }

    public void remove(T obj) {
        if (network.containsKey(obj)) {
            for (T key : network.get(obj).parents.keySet()) {
                network.get(key).clique.remove(obj);
            }

            for (T key : network.get(obj).clique.keySet()) {
                network.get(key).parents.remove(obj);
            }
            
            network.remove(obj);
        }
    }

    public void setParent(T seed, T clique_obj) {
        if(network.containsKey(seed) && network.containsKey(clique_obj)){
            honingNode<T> node = network.get(seed);
            if(!node.clique.containsKey(clique_obj)){
                honingNode<T> clique_int = network.get(clique_obj);
                node.clique.put(clique_obj, clique_int);
                clique_int.parents.put(seed, node);
            }
        }
    }

    public int countMicrofeatures(ArrayList<T> base) {
        int count_base = 0;

        for (int i = 0; i < base.size(); i++) {
            honingNode<T> node = (honingNode<T>) network.get(base.get(i));
            count_base += node.clique.size();
        }

        return count_base;
    }

    public void recruitNeurds(ArrayList<T> bridges, HashMap<T, honingNode<T>> output) {
        for (int i = 0; i < bridges.size(); i++) {
            honingNode<T> currentBridge = network.get(bridges.get(i));

            for (T key : currentBridge.parents.keySet()) {
                if (!output.containsKey(key)) {
                    output.put(key, currentBridge.parents.get(key));
                }
            }// End for
        }// End for
    }// End void

    public void contextualFilter(ArrayList<T> filter, HashMap<T, honingNode<T>> input, HashMap<T, honingNode<T>> filtered_output) {
        for (int i = 0; i < filter.size(); i++) {
            T key = filter.get(i);
            if (input.containsKey(key)) {
                filtered_output.put(key, input.get(key));
            }
        }// End for
    }// End void

    public HashMap<T, honingNode<T>> network;
}
