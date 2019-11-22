using System;
using System.Collections.Generic;
using System.Linq;
using TobyBlazor.Models;

namespace TobyBlazor.Data
{
    public class VideoRepository : IVideoRepository
    {
        private readonly DataContext db = new DataContext();

        public void Add(Video v)
        {
            db.Videos.Add(v);
            db.SaveChanges();
        }

        public void Delete(string ytid)
        {
            if (String.IsNullOrEmpty(ytid)) return;

            db.Videos.Remove(new Video()
            {
                YTId = ytid
            });
            db.SaveChanges();            
        }

        public List<Video> Find(string like)
        {
            return db.Videos.Where(x => x.Title.ToLower()
                            .Contains(like.ToLower()))
                            .Select(x => x)
                            .ToList();
        }

        public List<Video> FindByGroup(string group)
        {            
            return db.Videos.Where(x => x.Group.ToLower() == group.ToLower())
                            .Select(x => x)
                            .ToList();
        }

        public List<Video> All()
        {
            return db.Videos.ToList()
                            .Where(x => x.Group.ToLower() != "recently played")
                            .Select(x => x)
                            .ToList();
        }

        public List<Video> Search(string term)
        {
            List<Video> results = new List<Video>();

            if (String.IsNullOrEmpty(term.Trim())) return results;

            var value = term.ToLower().Split(new char[] { ' ' }, 2);

            return value switch
            {
                _ when value[0] == "/all" => All(),
                _ when value[0] == "/rp" => FindByGroup("recently played"),
                _ when value[0] == "/group" && value.Length > 1 && !String.IsNullOrEmpty(value[1]) => FindByGroup(value[1]),                
                _ => Find(term)
            };
        }
    }
}
