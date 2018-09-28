using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNN
{

    
    public class KNNEfficiency
    {
        private Dataset dataset;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataset"></param>
        public KNNEfficiency(Dataset dataset)
        {
            this.dataset = dataset;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public double getKNearestWinrates (Instance target, int k)
        {
            double winrate = 0.0;

            foreach (Instance instance in getKNearestInstance(target,k))
            {
                winrate+=instance.getLast();
            }
            return winrate / k;

        }

        public List<Instance> getKNearestInstance(Instance target, int k)
        {
            foreach (Instance instance in dataset.getInstances())
            {
                instance.distanceFrom(target);
            }
            dataset.sort();

            return dataset.getFirstK(k);
        }   
    }
}
