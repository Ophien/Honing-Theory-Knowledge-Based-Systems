using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpHoning;
using System.Threading;

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


        public double getKNearestWinrates(Instance target, int k)
        {
            PriorityQueue<Instance> pQueue = new PriorityQueue<Instance>();

            List<Instance> dataInst = dataset.getInstances();

            System.Object mlock = new System.Object();

           /*  foreach (Instance instance in dataInst)
             {
                 instance.distanceFrom(target);

                 pQueue.Enqueue(instance);
             }
            */
             Parallel.ForEach(dataInst, instance =>
             {
                 instance.distanceFrom(target);

                 lock(mlock)
                     pQueue.Enqueue(instance);
             });
             
            double winrate = 0.0;

            for (int i = 0; i < k; i++)
            {
                Instance dQueued = pQueue.Dequeue();
                winrate += dQueued.getLast();
            }

            double finalWinrate = winrate / k;

            return finalWinrate;
        }   
    }
}
