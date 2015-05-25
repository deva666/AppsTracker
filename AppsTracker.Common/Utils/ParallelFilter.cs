#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using AppsTracker.Common.Utils;

namespace AppsTracker.Utils
{
    public sealed class ParallelFilter
    {
        public static List<T[]> PartitionEnumerable<T>(IEnumerable<T> items)
        {
            Ensure.NotNull(items, "items");

            var procCount = Environment.ProcessorCount;
            var itemsArray = items.ToArray();

            var partitionList = new List<T[]>(procCount);
            if (itemsArray.Length >= procCount && procCount > 1)
            {
                int cycle = Convert.ToInt32(Math.Floor((double)itemsArray.Length / (double)procCount));
                int rem = itemsArray.Length % procCount;
                if (rem > 0)
                    cycle++;
                int arraySize = itemsArray.Length;
                for (int i = 0; i < procCount; i++)
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
