using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpHoning
{
    class HoningNode<T>
    {
        public HoningNode()
        {
            clique = new Dictionary<T, HoningNode<T>>();
            parents = new Dictionary<T, HoningNode<T>>();
            mark = "none";
        }

        public HoningNode(T holder)
        {
            clique = new Dictionary<T, HoningNode<T>>();
            parents = new Dictionary<T, HoningNode<T>>();
            this.holder = holder;
            mark = "none";
        }

        public Dictionary<T, HoningNode<T>> clique;
        public Dictionary<T, HoningNode<T>> parents;
        public T holder;
        public String mark;
    } // Fim HoningNode

    class HoningNetwork<T>
    {
        public HoningNetwork()
        {
            network = new Dictionary<T, HoningNode<T>>();
        }

        public int leafCount()
        {
            int count = 0;

            foreach(T key in network.Keys)
            {
                if (network[key].clique.Count == 0)
                    count++;
            }

            return count;
        }

        public void parentIDMapping(out Dictionary<T, int> id_mapping)
        {
            id_mapping = new Dictionary<T, int>();

            int idGen = 0;

            foreach (T key in network.Keys)
            {
                if (network[key].parents.Count == 0)
                {
                    id_mapping.Add(key, idGen);
                    idGen++;
                }
            }
        }

        public void leafIDMapping(out Dictionary<T, int> id_mapping, out Dictionary<int, T> reverse_mapping)
        {
            id_mapping = new Dictionary<T, int>();
            reverse_mapping = new Dictionary<int, T>();

            int idGen = 0;

            foreach (T key in network.Keys)
            {
                if (network[key].mark == "internal")
                {
                    id_mapping.Add(key, idGen);
                    reverse_mapping.Add(idGen, key);
                    idGen++;
                }
            }
        }

        // Op. b'asica para verificac'ao de microfeatures
        public bool checkMicrofeature(T seed, T microfeature)
        {
            bool check = false;

            if (!network.ContainsKey(seed))
                return check;

            HoningNode<T> node = network[seed];

            if (!node.clique.ContainsKey(microfeature))
                return check;

            return check = true;
        }

        // Op. b'asica para verifica'cao de vetores em honing
        public bool match_vector(List<T> A, List<T> B)
        {
            bool check = false;

            if (A.Count != B.Count)
            {
                return check;
            }

            for (int i = 0; i < A.Count; i++)
            {
                check = false;

                for (int j = 0; j < B.Count; j++)
                {
                    if (A[i].Equals(B[j]))
                    {
                        check = true; break;
                    }
                }

                if (!check)
                    return check;
            }

            return check;
        }

        public bool HasNode(T Node)
        {
            if (network.ContainsKey(Node))
                return true;
            return false;
        }

        // Op. b'asica para insers'ao de objetos na rede
        public void insert(T obj, String mark=null)
        {
            if (!network.ContainsKey(obj))
            {
                network.Add(obj, new HoningNode<T>(obj));
                if(mark!=null)
                network[obj].mark = mark;
            }
        }

        // Op. B'asica para remoc'ao de objetos da rede
        public void remove(T obj)
        {
            if (network.ContainsKey(obj))
            {
                foreach (T key in network[obj].parents.Keys)
                {
                    network[key].clique.Remove(obj);
                }

                foreach (T key in network[obj].clique.Keys)
                {
                    network[key].parents.Remove(obj);
                }

                network.Remove(obj);
            }
        }

        // Op. B'asica para configura'cao de parentesco
        public void setParent(T seed, T clique_obj)
        {
            if (network.ContainsKey(seed) && network.ContainsKey(clique_obj))
            {
                HoningNode<T> node = network[seed];
                if (!node.clique.ContainsKey(clique_obj))
                {
                    HoningNode<T> cliqueNode = network[clique_obj];
                    node.clique.Add(clique_obj, cliqueNode);
                    cliqueNode.parents.Add(seed, node);
                }
            }
        }

        // Op. b'asica para contagem de microfeatures
        public int countMicrofeatures(List<T> baseVector)
        {
            int count = 0;

            foreach (T value in baseVector)
            {
                HoningNode<T> node = network[value];
                count += node.clique.Count;
            }

            return count;
        }

        public void removeWithMicrofeature(ref Dictionary<T, HoningNode<T>> input, out Dictionary<T, HoningNode<T>> output, T toRemove)
        {
            output = new Dictionary<T, HoningNode<T>>();

            foreach (KeyValuePair<T, HoningNode<T>> key in input)
            {
                if(!checkMicrofeature(key.Value.holder, toRemove)){
                    output.Add(key.Key, key.Value);
                }
            }
        }

        public void keepWithMicrofeature(ref Dictionary<T, HoningNode<T>> input, out Dictionary<T, HoningNode<T>> output, T toKeep)
        {
            output = new Dictionary<T, HoningNode<T>>();

            foreach (KeyValuePair<T, HoningNode<T>> key in input)
            {
                if (checkMicrofeature(key.Value.holder, toKeep))
                {
                    output.Add(key.Key, key.Value);
                }
            }
        }

        public bool orMicrofeatureCheck(T seed, List<T> checkList)
        {
            foreach(T value in checkList){
                if (checkMicrofeature(seed, value))
                {
                    return true;
                }
            }
            return false;
        }

        // Recrutamento de neurds simplificado
        public void recruitNeurds(List<T> bridges, out Dictionary<T, HoningNode<T>> output, List<T> excluding_checkList = null)
        {
            output = new Dictionary<T, HoningNode<T>>();

            foreach (T currentBridge in bridges)
            {
                if (network[currentBridge].mark == "terminal")
                {
                    output.Add(network[currentBridge].holder, network[currentBridge]);
                    continue;
                }

                foreach (KeyValuePair<T, HoningNode<T>> parent in network[currentBridge].parents)
                {
                    if(excluding_checkList != null)
                        if (orMicrofeatureCheck(parent.Value.holder, excluding_checkList))
                           continue;
                    if (!output.ContainsKey(parent.Key))
                        output.Add(parent.Key, parent.Value);
                }
            }
        }

        // Recrutamento de neurds simplificado
        public void recruitNeurds(List<T> bridges, out Dictionary<T, HoningNode<T>> output, String filter)
        {
            output = new Dictionary<T, HoningNode<T>>();

            foreach (T currentBridge in bridges)
            {
                if (!network.ContainsKey(currentBridge))
                    continue;

                if (network[currentBridge].mark == "terminal")
                {
                    output.Add(network[currentBridge].holder, network[currentBridge]);
                    continue;
                }

                foreach (KeyValuePair<T, HoningNode<T>> parent in network[currentBridge].parents)
                {
                    if (!output.ContainsKey(parent.Key))
                    {
                        if (filter != null)
                        {
                            if (parent.Value.mark == filter)
                                output.Add(parent.Key, parent.Value);
                        }
                        else
                        {
                            output.Add(parent.Key, parent.Value);
                        }
                    }
                }
            }
        }

        public void getMfList(T seed, out List<T> mfs, String filter = null)
        {
            mfs = new List<T>();
            if (!network.ContainsKey(seed))
                return;
            HoningNode<T> node = network[seed];
            foreach (KeyValuePair<T, HoningNode<T>> n in node.clique)
            {
                if (filter != null)
                {
                    if (n.Value.mark == filter)
                    {
                        if (n.Value.mark != "terminal")
                            mfs.Add(n.Value.holder);
                    }
                }
                else
                    if (n.Value.mark != "terminal")
                        mfs.Add(n.Value.holder);
            }
        }

        public void GetRelationsFromEssential(T seed, out Dictionary<T, HoningNode<T>> output)
        {
            output = new Dictionary<T, HoningNode<T>>();

            if (!network.ContainsKey(seed))
                return;
            
            List<T> bridges = new List<T>();
            HoningNode<T> refnode = network[seed];

            foreach (T key in refnode.clique.Keys)
            {
                if (refnode.clique[key].mark == "essential")
                    bridges.Add(key);
            }

            recruitNeurds(bridges, out output, "internal");
        }

        public T getMicrofeatureClass(T seed, ref List<T> classes)
        {
            T result = default(T);

            foreach (T c in classes)
            {
                if (checkMicrofeature(seed, c))
                {
                    result = c;
                    break;
                }
            }

            return result;
        }

        public void classSeparation(ref List<T> classes, ref Dictionary<T, HoningNode<T>> input, out Dictionary<T, Dictionary<T, HoningNode<T>>> output)
        {
            output = new Dictionary<T, Dictionary<T, HoningNode<T>>>();

            foreach (KeyValuePair<T, HoningNode<T>> key in input)
            {
                T type = getMicrofeatureClass(key.Value.holder, ref classes);
                if (type != null)
                {
                    if (!output.ContainsKey(type))
                        output.Add(type, new Dictionary<T, HoningNode<T>>());
                    Dictionary<T, HoningNode<T>> dic = output[type];
                    dic.Add(key.Key, key.Value);
                }
            }
        }

        // Filtro contextual para busca guiada
        public void contextualFilter(List<T> filter, Dictionary<T, HoningNode<T>> input, out Dictionary<T, HoningNode<T>> filtered_output)
        {
            filtered_output = new Dictionary<T, HoningNode<T>>();

            foreach (T filter_objKey in filter)
            {
                if (input.ContainsKey(filter_objKey))
                {
                    filtered_output.Add(filter_objKey, input[filter_objKey]);
                }
            }
        }

        public HoningNode<T> getHoningNode(T obj)
        {
            if (!network.ContainsKey(obj))
            {
                return null;
            }

            return network[obj];
        }

        public List<T> getTerminalList(){
            List<T> n = new List<T>();
            foreach (T key in network.Keys)
            {
                HoningNode<T> current = network[key];
                if (current.mark == "terminal")
                    n.Add(current.holder);
            }
            return n;
        }

        public Dictionary<T, HoningNode<T>> getNetwork()
        {
            return network;
        }

        Dictionary<T, HoningNode<T>> network;
    } // Fim HoningNetwork
} // Fim Namespace
