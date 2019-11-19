using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TobyBlazor.Models
{
    public class Video
    {        
        public int Id { get; set; }        
        public string Title { get; set; }        
        public string YTId { get; set; }        
        public string Group { get; set; }        
    }
}
