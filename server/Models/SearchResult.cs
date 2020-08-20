using System.Collections.Generic;

namespace TobyBlazor.Models
{
    public enum SearchResultType
    {
        Command,
        Manage,
        ManageGroups,
        ManageRecentlyPlayed,
        Search
    }

    public class SearchResult
    {
        public SearchResultType Type { get; set; }
        public List<Video> Videos { get; set; } = new List<Video>();
        public Message Message { get; set; } = new Message();
    }
}
