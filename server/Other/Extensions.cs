using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TobyBlazor.Other
{
    
    public static class ListExtensions
    {
        // Taken from: https://stackoverflow.com/a/24087164/170217
        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}
