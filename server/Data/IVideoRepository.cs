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
        public void AddGroup(string name);
        public List<Group> AllGroups();
        public Group FindGroupByName(string name);
        public List<Group> FindGroup(string like);
        public List<Video> FindVideo(string like);
        public List<Video> FindVideoByGroup(string group);
        public Video FindVideoByYTId(string ytid);
        public Video FindVideoByYTId(string ytid, string group);
        public void AddVideo(Video v, string group);
        public void AddVideoToRecentlyPlayed(Video video);
        public void DeleteGroup(string name);
        public void DeleteGroupRange(List<Group> groups);
        public void DeleteVideo(string ytid, string group);
        public void DeleteVideoRange(List<Video> videos);
        public void DeleteVideoRangeByGroup(string group);
        public void UpdateVideoGroup(string ytid, string group);
    }
}
