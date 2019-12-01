using System.Collections.Generic;
using System.Threading.Tasks;
using TobyBlazor.Models;

namespace TobyBlazor.Data
{
    public interface IVideoRepository
    {
        public Task<List<Video>> Search(string term);
        public Task<List<Video>> SearchYouTube(string term);
        public List<Video> AllVideos();
        public List<Video> VideosByPage(int page, int pageSize);
        public List<Group> AllGroups();
        public List<Video> Find(string like);
        public List<Video> FindByGroup(string group);
        public Video FindByYTId(string ytid);
        public Video FindByYTId(string ytid, string group);
        public void Add(Video v, string group);
        public void AddToRecentlyPlayed(Video video);
        public void Delete(string ytid, string group);
        public void DeleteRange(List<Video> videos);
        public void DeleteRangeByGroup(string group);
        public void UpdateGroup(string ytid, string group);
    }
}
