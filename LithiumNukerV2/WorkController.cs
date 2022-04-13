// System
using System.Collections.Generic;
using System.Linq;

namespace LunaNuker
{
    public class WorkController
    {

        public static List<List<T>> Seperate<T>(List<T> items, int loadCount)
        {
            var loads = new List<List<T>>();

            // Create lists
            for (int x = 0; x < loadCount; x++)
                loads.Add(new List<T>());

            // Actually seperate the loads
            for (int x = 0; x < items.Count; x++)
                loads[x % loadCount].Add(items[x]);

            // Remove blank loads
            loads = loads.Where(load => load.Count > 0).ToList();

            return loads;
        }
    }
}
