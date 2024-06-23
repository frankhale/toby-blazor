using System.Collections.Generic;
using System.Threading.Tasks;
using TobyBlazor.Models;

namespace TobyBlazor.Data;

public interface IVideoRepository
{
    #region Search & Find

    public Task<List<Video>> SearchAsync(string term);
    public Task<List<Video>> SearchYouTubeAsync(string term);
    public Task<Group> FindGroupByNameAsync(string name);
    public Task<List<Group>> FindGroupAsync(string like);
    public Task<List<Video>> FindVideoAsync(string like);
    public Task<List<Video>> FindVideoByGroupAsync(string group);
    public Task<Video> FindVideoByYtIdAsync(string ytid);
    public Task<Video> FindVideoByYtIdAsync(string ytid, string group);

    #endregion

    #region Get All Videos/Groups/Recently Played

    public Task<List<Video>> AllVideosAsync();
    public Task<List<Video>> VideosByPageAsync(int page, int pageSize);
    public Task<List<Group>> AllGroupsAsync();
    public Task<List<Video>> GetRecentlyPlayedVideosAsync(int count);

    #endregion

    #region Add/Update - Videos/Groups

    public Task AddGroupAsync(string name);
    public Task AddVideoAsync(Video v, string group);
    public Task AddVideoToRecentlyPlayedAsync(Video video);
    public Task UpdateVideoGroupAsync(string ytid, string group);

    #endregion

    #region Delete Videos/Groups

    public Task DeleteGroupAsync(string name);
    public Task DeleteGroupRangeAsync(List<Group> groups);
    public Task DeleteVideoAsync(string ytid, string group);
    public Task DeleteVideoRangeAsync(List<Video> videos);
    public Task DeleteVideoRangeByGroupAsync(string group);

    #endregion

    #region Preferences

    public Task SetCurrentVideoPage(bool recentlyPlayed, int page, int linkPage);
    public Task<Preferences> GetCurrentPreferences();

    #endregion
}