using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TobyBlazor.Models;

namespace TobyBlazor.Data
{
    public interface IVideoRepository
    {
        public List<Video> Search(string term);
        public List<Video> All();
        public List<Video> Find(string like);
        public List<Video> FindByGroup(string group);        
        public void Add(Video v);
        public void Delete(string ytid);        
    }
}
