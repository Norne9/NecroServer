using System;
using System.Collections.Generic;
using System.Text;

namespace NecroServer
{
    public static class SplitList
    {
        public static IEnumerable<List<T>> Split<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
        }
    }
}
