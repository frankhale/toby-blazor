using System;

namespace TobyBlazor.Models
{
    public class Video : IDataItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string YTId { get; set; }
        public string Group { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
