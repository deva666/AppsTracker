using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Utils
{
    public sealed class ParallelFilter
    {
        public static List<T[]> CreateFilters<T>(T[] filter) where T : new()
        {
            var procs = Environment.ProcessorCount;

            List<T[]> filtersList = new List<T[]>(procs);
            if (filter.Length >= procs && procs > 1)
            {
                int cycle = Convert.ToInt32(Math.Floor((double)filter.Length / (double)procs));
                int rem = filter.Length % procs;
                if (rem > 0)
                    cycle++;
                int arraySize = filter.Length;
                for (int i = 0; i < procs; i++)
                {
                    T[] array = new T[] { };
                    Array.Resize<T>(ref array, cycle > arraySize ? arraySize : cycle);
                    Array.Copy(filter, i * cycle, array, 0, cycle > arraySize ? arraySize : cycle);
                    arraySize -= cycle;
                    filtersList.Add(array);
                }
            }
            else
                filtersList.Add(filter);

            return filtersList;
        }
    }
}
