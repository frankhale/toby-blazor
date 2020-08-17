using System;
using System.Collections.Generic;
using System.Linq;

namespace TobyBlazor.Other
{
    public static class HelperExtensions
    {
        // Taken from: https://stackoverflow.com/a/24087164/170217
        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => (Index: i, Value: x))
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        public static string GetDefaultVideoThumbnail(string ytid)
        {
            return String.Format("https://i.ytimg.com/vi/{0}/default.jpg", ytid);
        }
    }
}
