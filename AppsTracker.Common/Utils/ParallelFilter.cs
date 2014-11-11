using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppsTracker.Common.Utils;

namespace AppsTracker.Utils
{
    public sealed class ParallelFilter
    {
        public static List<T[]> PartitionEnumerable<T>(IEnumerable<T> items) 
        {
            Ensure.NotNull(items, "items");

            var procs = Environment.ProcessorCount;
            var itemsArray = items.ToArray();

            List<T[]> partitionList = new List<T[]>(procs);
            if (itemsArray.Length >= procs && procs > 1)
            {
                int cycle = Convert.ToInt32(Math.Floor((double)itemsArray.Length / (double)procs));
                int rem = itemsArray.Length % procs;
                if (rem > 0)
                    cycle++;
                int arraySize = itemsArray.Length;
                for (int i = 0; i < procs; i++)
                {
                    T[] array = new T[] { };
                    Array.Resize<T>(ref array, cycle > arraySize ? arraySize : cycle);
                    Array.Copy(itemsArray, i * cycle, array, 0, cycle > arraySize ? arraySize : cycle);
                    arraySize -= cycle;
                    partitionList.Add(array);
                }
            }
            else
                partitionList.Add(itemsArray);

            return partitionList;
        }
    }
}
