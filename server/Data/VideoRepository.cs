using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.EntityFrameworkCore;
using TobyBlazor.Models;

namespace TobyBlazor.Data;

public class VideoRepository : IVideoRepository
{
    private readonly DataContext _db = new();

    #region Search & Find

    public async Task<Group> FindGroupByNameAsync(string name)
    {
        return await _db.Groups.Where(x => x.Name == name)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Group>> FindGroupAsync(string like)
    {
        return await _db.Groups.Where(x => x.Name.ToLower().Contains(like.ToLower()))
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<List<Video>> FindVideoAsync(string like)
    {
        return await _db.Videos.Where(x => x.Title.ToLower().Contains(like.ToLower()) && x.Group != "Recently Played")
            .OrderBy(x => x.Title)
            .ToListAsync();
    }

    public async Task<List<Video>> FindVideoByGroupAsync(string group)
    {
        return await _db.Videos.Where(x => x.Group.ToLower() == group.ToLower())
            .OrderBy(x => x.Title)
            .ToListAsync();
    }

    public async Task<Video> FindVideoByYtIdAsync(string ytid, string group = "")
    {
        var query = _db.Videos.Where(x => x.YTId == ytid);

        if (!string.IsNullOrEmpty(group)) query = _db.Videos.Where(x => x.YTId == ytid && x.Group == group);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<List<Video>> SearchYouTubeAsync(string term)
    {
        var youtubeService = new YouTubeService(new BaseClientService.Initializer
        {
            ApiKey = Environment.GetEnvironmentVariable("YOUTUBE_API_KEY"),
            ApplicationName = GetType().ToString()
        });

        var searchRequest = youtubeService.Search.List("snippet");
        searchRequest.Q = term;
        searchRequest.MaxResults = 25;

        var searchListResponse = await Task.Run(() => searchRequest.ExecuteAsync());

        List<Video> videos = new();

        foreach (var searchResult in searchListResponse.Items)
            switch (searchResult.Id.Kind)
            {
                case "youtube#video":
                    videos.Add(new Video
                    {
                        Title = searchResult.Snippet.Title,
                        YTId = searchResult.Id.VideoId
                    });

                    break;
            }

        return videos;
    }

    public async Task<List<Video>> SearchAsync(string term)
    {
        if (string.IsNullOrEmpty(term.Trim())) return [];

        static bool HasSubValue(string[] value)
        {
            return value.Length > 1 && !string.IsNullOrEmpty(value[1]);
        }

        static bool MatchesCommandList(string value, params string[] commands)
        {
            return commands.FirstOrDefault(x => x == value) != null;
        }

        var value = term.ToLower().Split([' '], 2);

        return value switch
        {
            _ when MatchesCommandList(value[0], "/ls", "/all") => await AllVideosAsync(),
            _ when MatchesCommandList(value[0], "/fav", "/favorites") => await FindVideoByGroupAsync("Favorites"),
            _ when MatchesCommandList(value[0], "/rp", "/recently-played") => await FindVideoByGroupAsync(
                "Recently Played"),
            _ when MatchesCommandList(value[0], "/g", "/group") && HasSubValue(value) =>
                await FindVideoByGroupAsync(value[1]),
            _ when MatchesCommandList(value[0], "/yt", "/youtube") && HasSubValue(value) =>
                await SearchYouTubeAsync(value[1]),
            _ => await FindVideoAsync(term)
        };
    }

    #endregion

    #region Get All Videos/Groups/Recently Played

    public async Task<List<Video>> AllVideosAsync()
    {
        return await _db.Videos
            .Where(x => x.Group.ToLower() != "recently played")
            .OrderBy(x => x.Title)
            .ToListAsync();
    }

    public async Task<List<Video>> VideosByPageAsync(int page, int pageSize)
    {
        return await _db.Videos
            .Where(x => x.Group.ToLower() != "recently played")
            .OrderBy(x => x.Title)
            .Skip(pageSize * page)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Group>> AllGroupsAsync()
    {
        return await _db.Groups
            .Where(x => x.Name != "Recently Played")
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<List<Video>> GetRecentlyPlayedVideosAsync(int count = 5)
    {
        return await _db.Videos
            .Where(x => x.Group.ToLower() == "recently played")
            .OrderByDescending(x => x.CreateDate)
            .ThenBy(x => x.Title)
            .Take(count)
            .ToListAsync();
    }

    #endregion

    #region Add/Update - Videos/Groups

    public async Task AddVideoAsync(Video v, string group)
    {
        if (string.IsNullOrEmpty(group)) group = "Favorites";

        _db.Videos.Add(new Video
        {
            Title = v.Title,
            YTId = v.YTId,
            Group = group,
            CreateDate = DateTime.Now
        });
        await _db.SaveChangesAsync();
    }

    public async Task AddVideoToRecentlyPlayedAsync(Video video)
    {
        var alreadyInDb = await _db.Videos.Where(x => x.YTId == video.YTId && x.Group == "Recently Played")
            .FirstOrDefaultAsync();

        if (alreadyInDb != null) return;

        var allRpVideos = await _db.Videos.Where(x => x.Group == "Recently Played")
            .OrderByDescending(x => x.CreateDate)
            .ToListAsync();

        if (allRpVideos.Count == 30)
        {
            // delete all recently played
            _db.Videos.RemoveRange(allRpVideos);
            await _db.SaveChangesAsync();

            // add top 29
            _db.Videos.AddRange(allRpVideos.Take(29));
            await _db.SaveChangesAsync();
        }

        _db.Videos.Add(new Video
        {
            Title = video.Title, // WebUtility.HtmlEncode(video.Title),
            YTId = video.YTId,
            Group = "Recently Played",
            CreateDate = DateTime.Now
        });
        await _db.SaveChangesAsync();
    }

    public async Task AddGroupAsync(string name)
    {
        var groupExists = await _db.Groups
            .Where(x => x.Name.ToLower() == name.ToLower())
            .FirstOrDefaultAsync();

        if (groupExists == null)
        {
            _db.Groups.Add(new Group
            {
                Name = name
            });
            await _db.SaveChangesAsync();
        }
    }

    public async Task UpdateVideoGroupAsync(string ytid, string group)
    {
        var foundVideo = await _db.Videos.Where(x => x.YTId == ytid && x.Group != "Recently Played")
            .FirstOrDefaultAsync();

        if (foundVideo != null && !string.IsNullOrEmpty(group))
        {
            foundVideo.Group = group;
            await _db.SaveChangesAsync();
        }
    }

    #endregion

    #region Delete Videos/Groups

    public async Task DeleteVideoAsync(string ytid, string group)
    {
        if (string.IsNullOrEmpty(ytid)) return;

        var found = await _db.Videos.Where(x => x.YTId == ytid && x.Group != "Recently Played").ToListAsync();

        _db.Videos.RemoveRange(found);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteVideoRangeAsync(List<Video> videos)
    {
        foreach (var v in videos)
        {
            var foundVideos = await _db.Videos.Where(x => x.YTId == v.YTId).ToListAsync();

            _db.Videos.RemoveRange(foundVideos);
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteVideoRangeByGroupAsync(string group)
    {
        var videos = await _db.Videos.Where(x => x.Group == group)
            .ToListAsync();

        if (videos.Count > 0)
        {
            _db.RemoveRange(videos);
            await _db.SaveChangesAsync();
        }
    }

    public async Task DeleteGroupAsync(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        var found = await _db.Groups.Where(x => x.Name == name).FirstOrDefaultAsync();

        _db.Groups.Remove(found);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteGroupRangeAsync(List<Group> groups)
    {
        _db.Groups.RemoveRange(groups);
        await _db.SaveChangesAsync();
    }

    #endregion

    #region Preferences

    public async Task SetCurrentVideoPage(bool recentlyPlayed, int page, int linkPage)
    {
        var preferences = await _db.Preferences.FirstOrDefaultAsync();

        if (preferences == null)
        {
            if (recentlyPlayed)
                _db.Preferences.Add(new Preferences
                {
                    CurrentRecentlyPlayedVideoPage = page
                });
            else
                _db.Preferences.Add(new Preferences
                {
                    CurrentVideoPage = page,
                    CurrentVideoPageLinkPage = linkPage
                });
        }
        else
        {
            if (recentlyPlayed)
            {
                preferences.CurrentRecentlyPlayedVideoPage = page;
            }
            else
            {
                preferences.CurrentVideoPage = page;
                preferences.CurrentVideoPageLinkPage = linkPage;
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task<Preferences> GetCurrentPreferences()
    {
        return await _db.Preferences.FirstOrDefaultAsync();
    }

    #endregion
}