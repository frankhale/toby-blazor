using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TobyBlazor.Models;

namespace TobyBlazor.Data
{
  public class VideoRepository : IVideoRepository
  {
    private readonly DataContext db = new DataContext();

    #region Search & Find
    public async Task<Group> FindGroupByNameAsync(string name)
    {
      return await db.Groups.Where(x => x.Name == name)
                            .FirstOrDefaultAsync();
    }

    public async Task<List<Group>> FindGroupAsync(string like)
    {
      return await db.Groups.Where(x => x.Name.ToLower().Contains(like.ToLower()))
                            .OrderBy(x => x.Name)
                            .ToListAsync();
    }

    public async Task<List<Video>> FindVideoAsync(string like)
    {
      return await db.Videos.Where(x => x.Title.ToLower().Contains(like.ToLower()) && x.Group != "Recently Played")
                            .OrderBy(x => x.Title)
                            .ToListAsync();
    }

    public async Task<List<Video>> FindVideoByGroupAsync(string group)
    {
      return await db.Videos.Where(x => x.Group.ToLower() == group.ToLower())
                            .OrderBy(x => x.Title)
                            .ToListAsync();
    }

    public async Task<Video> FindVideoByYTIdAsync(string ytid)
    {
      return await db.Videos.Where(x => x.YTId == ytid) // && x.Group != "Recently Played")
                            .FirstOrDefaultAsync();
    }

    public async Task<Video> FindVideoByYTIdAsync(string ytid, string group)
    {
      return await db.Videos.Where(x => x.YTId == ytid && x.Group == group)
                            .FirstOrDefaultAsync();
    }

    public async Task<List<Video>> SearchYouTubeAsync(string term)
    {
      var youtubeService = new YouTubeService(new BaseClientService.Initializer()
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
      {
        switch (searchResult.Id.Kind)
        {
          case "youtube#video":
            videos.Add(new Video()
            {
              Title = searchResult.Snippet.Title,
              YTId = searchResult.Id.VideoId
            });

            break;
        }
      }

      return videos;
    }

    public async Task<List<Video>> SearchAsync(string term)
    {
      if (string.IsNullOrEmpty(term.Trim())) return new List<Video>();

      static bool HasSubValue(string[] value) => value.Length > 1 && !string.IsNullOrEmpty(value[1]);
      static bool MatchesCommandList(string value, params string[] commands) => (commands.FirstOrDefault(x => x == value) != null);

      var value = term.ToLower().Split(new char[] { ' ' }, 2);

      return value switch
      {
        _ when MatchesCommandList(value[0], "/ls", "/all") => await AllVideosAsync(),
        _ when MatchesCommandList(value[0], "/fav", "/favorites") => await FindVideoByGroupAsync("Favorites"),
        _ when MatchesCommandList(value[0], "/rp", "/recently-played") => await FindVideoByGroupAsync("Recently Played"),
        _ when MatchesCommandList(value[0], "/g", "/group") && HasSubValue(value) => await FindVideoByGroupAsync(value[1]),
        _ when MatchesCommandList(value[0], "/yt", "/youtube") && HasSubValue(value) => await SearchYouTubeAsync(value[1]),
        _ => await FindVideoAsync(term)
      };
    }
    #endregion

    #region Get All Videos/Groups/Recently Played
    public async Task<List<Video>> AllVideosAsync()
    {
      return await db.Videos
                     .Where(x => x.Group.ToLower() != "recently played")
                     .OrderBy(x => x.Title)
                     .ToListAsync();
    }

    public async Task<List<Video>> VideosByPageAsync(int page, int pageSize)
    {
      return await db.Videos
                     .Where(x => x.Group.ToLower() != "recently played")
                     .OrderBy(x => x.Title)
                     .Skip(pageSize * page)
                     .Take(pageSize)
                     .ToListAsync();
    }

    public async Task<List<Group>> AllGroupsAsync()
    {
      return await db.Groups
                     .OrderBy(x => x.Name)
                     .ToListAsync();
    }

    public async Task<List<Video>> GetRecentlyPlayedVideosAsync(int count = 5)
    {
      return await db.Videos
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
      if (string.IsNullOrEmpty(group))
      {
        group = "Favorites";
      }

      db.Videos.Add(new Video()
      {
        Title = v.Title,
        YTId = v.YTId,
        Group = group,
        CreateDate = DateTime.Now
      });
      await db.SaveChangesAsync();
    }

    public async Task AddVideoToRecentlyPlayedAsync(Video video)
    {
      var alreadyInDb = await db.Videos.Where(x => x.YTId == video.YTId && x.Group == "Recently Played").FirstOrDefaultAsync();

      if (alreadyInDb != null) return;

      var allRPVideos = await db.Videos.Where(x => x.Group == "Recently Played")
                                 .OrderByDescending(x => x.CreateDate)
                                 .ToListAsync();

      if (allRPVideos.Count == 30)
      {
        // delete all recently played
        db.Videos.RemoveRange(allRPVideos);
        await db.SaveChangesAsync();

        // add top 29
        db.Videos.AddRange(allRPVideos.Take(29));
        await db.SaveChangesAsync();
      }

      db.Videos.Add(new Video()
      {
        Title = video.Title, // WebUtility.HtmlEncode(video.Title),
        YTId = video.YTId,
        Group = "Recently Played",
        CreateDate = DateTime.Now
      });
      await db.SaveChangesAsync();
    }

    public async Task AddGroupAsync(string name)
    {
      var groupExists = await db.Groups
                                .Where(x => x.Name.ToLower() == name.ToLower())
                                .FirstOrDefaultAsync();

      if (groupExists == null)
      {
        db.Groups.Add(new Group()
        {
          Name = name
        });
        await db.SaveChangesAsync();
      }
    }

    public async Task UpdateVideoGroupAsync(string ytid, string group)
    {
      var foundVideo = await db.Videos.Where(x => x.YTId == ytid && x.Group != "Recently Played").FirstOrDefaultAsync();

      if (foundVideo != null && !string.IsNullOrEmpty(group))
      {
        foundVideo.Group = group;
        await db.SaveChangesAsync();
      }
    }
    #endregion

    #region Delete Videos/Groups
    public async Task DeleteVideoAsync(string ytid, string group)
    {
      if (string.IsNullOrEmpty(ytid)) return;

      var found = await db.Videos.Where(x => x.YTId == ytid && x.Group != "Recently Played").ToListAsync();

      db.Videos.RemoveRange(found);
      await db.SaveChangesAsync();
    }

    public async Task DeleteVideoRangeAsync(List<Video> videos)
    {
      foreach (var v in videos)
      {
        var foundVideos = await db.Videos.Where(x => x.YTId == v.YTId).ToListAsync();

        if (foundVideos != null)
        {
          db.Videos.RemoveRange(foundVideos);
        }
      }

      await db.SaveChangesAsync();
    }

    public async Task DeleteVideoRangeByGroupAsync(string group)
    {
      var videos = await db.Videos.Where(x => x.Group == group)
                            .ToListAsync();

      if (videos.Count > 0)
      {
        db.RemoveRange(videos);
        await db.SaveChangesAsync();
      }
    }

    public async Task DeleteGroupAsync(string name)
    {
      if (string.IsNullOrEmpty(name)) return;

      var found = await db.Groups.Where(x => x.Name == name).FirstOrDefaultAsync();

      db.Groups.Remove(found);
      await db.SaveChangesAsync();
    }

    public async Task DeleteGroupRangeAsync(List<Group> groups)
    {
      db.Groups.RemoveRange(groups);
      await db.SaveChangesAsync();
    }
    #endregion

    #region Preferences
    public async Task SetCurrentVideoPage(bool recentlyPlayed, int page, int linkPage)
    {
      var preferences = await db.Preferences.FirstOrDefaultAsync();

      if (preferences == null)
      {
        if (recentlyPlayed)
        {
          db.Preferences.Add(new Preferences
          {
            CurrentRecentlyPlayedVideoPage = page
          });

        }
        else
        {
          db.Preferences.Add(new Preferences
          {
            CurrentVideoPage = page,
            CurrentVideoPageLinkPage = linkPage
          });
        }
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
      await db.SaveChangesAsync();
    }

    public async Task<Preferences> GetCurrentPreferences()
    {
      return await db.Preferences.FirstOrDefaultAsync();
    }
    #endregion
  }
}
