using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using TobyBlazor.Models;

namespace TobyBlazor.Data
{
    public class VideoRepository : IVideoRepository
    {
        private readonly DataContext db = new DataContext();

        public void Add(Video v, string group)
        {
            if (String.IsNullOrEmpty(group))
            {
                group = "Favorites";
            }

            db.Videos.Add(new Video()
            {
                Title = SecurityElement.Escape(v.Title),
                YTId = v.YTId,
                Group = group,
                CreateDate = DateTime.Now
            });
            db.SaveChanges();
        }

        public void AddToRecentlyPlayed(Video video)
        {
            if (db.Videos.Where(x => x.YTId == video.YTId && x.Group == "Recently Played")
                         .FirstOrDefault() != null) return;

            var allRPVideos = db.Videos.Where(x => x.Group == "Recently Played")
                                       .OrderByDescending(x => x.CreateDate)
                                       .ToList();

            if (allRPVideos.Count == 30)
            {
                // delete all recently played
                db.Videos.RemoveRange(allRPVideos);
                db.SaveChanges();

                // add top 29
                db.Videos.AddRange(allRPVideos.Take(29));
                db.SaveChanges();
            }

            db.Videos.Add(new Video()
            {
                Title = SecurityElement.Escape(video.Title),
                YTId = video.YTId,
                Group = "Recently Played",
                CreateDate = DateTime.Now
            });
            db.SaveChanges();
        }

        public void Delete(string ytid, string group)
        {
            if (String.IsNullOrEmpty(ytid)) return;

            var found = db.Videos.Where(x => x.YTId == ytid && x.Group != "Recently Played").ToList();

            db.Videos.RemoveRange(found);
            db.SaveChanges();
        }

        public void DeleteRange(List<Video> videos)
        {
            db.Videos.RemoveRange(videos);
            db.SaveChanges();
        }

        public void DeleteRangeByGroup(string group)
        {
            var videos = db.Videos.Where(x => x.Group == group)
                                  .ToList();

            if (videos.Count > 0)
            {
                db.RemoveRange(videos);
                db.SaveChanges();
            }
        }

        public List<Video> Find(string like)
        {
            return db.Videos.Where(x => x.Title.ToLower().Contains(like.ToLower()) && x.Group != "Recently Played")
                            .OrderBy(x => x.Title)
                            .ToList();
        }

        public List<Video> FindByGroup(string group)
        {
            return db.Videos.Where(x => x.Group.ToLower() == group.ToLower())
                            .OrderBy(x => x.Title)
                            .ToList();
        }

        public Video FindByYTId(string ytid)
        {
            return db.Videos.Where(x => x.YTId == ytid && x.Group != "Recently Played")
                            .FirstOrDefault();
        }

        public Video FindByYTId(string ytid, string group)
        {
            return db.Videos.Where(x => x.YTId == ytid && x.Group == group)
                            .FirstOrDefault();
        }

        public List<Video> AllVideos()
        {
            return db.Videos.Where(x => x.Group.ToLower() != "recently played")
                            .OrderBy(x => x.Title)
                            .ToList();
        }

        public List<Video> VideosByPage(int page, int pageSize)
        {
            return db.Videos.Where(x => x.Group.ToLower() != "recently played")
                .OrderBy(x => x.Title)
                .Skip(pageSize * page)
                .Take(pageSize)
                .ToList();
        }

        public List<Group> AllGroups()
        {
            return db.Groups.OrderBy(x => x.Name)
                            .ToList();
        }

        public void UpdateGroup(string ytid, string group)
        {
            var foundVideo = db.Videos.Where(x => x.YTId == ytid && x.Group != "Recently Played").FirstOrDefault();

            if (foundVideo != null)
            {
                if (!String.IsNullOrEmpty(group))
                {
                    foundVideo.Group = group;
                    db.SaveChanges();
                }
            }
        }

        public async Task<List<Video>> SearchYouTube(string term)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = Environment.GetEnvironmentVariable("YOUTUBE_API_KEY"),
                ApplicationName = this.GetType().ToString()
            });

            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Q = term;
            searchRequest.MaxResults = 25;

            var searchListResponse = await Task.Run(() => searchRequest.ExecuteAsync());

            List<Video> videos = new List<Video>();

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

        public async Task<List<Video>> Search(string term)
        {
            if (String.IsNullOrEmpty(term.Trim())) return new List<Video>();

            static bool HasSubValue(string[] value) => value.Length > 1 && !String.IsNullOrEmpty(value[1]);
            static bool MatchesCommandList(string value, params string[] commands) => (commands.Where(x => x == value).FirstOrDefault() != null) ? true : false;

            var value = term.ToLower().Split(new char[] { ' ' }, 2);

            return value switch
            {
                _ when MatchesCommandList(value[0], "/ls", "/all") => AllVideos(),
                _ when MatchesCommandList(value[0], "/fav", "/favorites") => FindByGroup("Favorites"),
                _ when MatchesCommandList(value[0], "/rp", "/recently-played") => FindByGroup("Recently Played"),
                _ when MatchesCommandList(value[0], "/g", "/group") && HasSubValue(value) => FindByGroup(value[1]),
                _ when MatchesCommandList(value[0], "/yt", "/youtube") && HasSubValue(value) => await SearchYouTube(value[1]),
                _ => Find(term)
            };
        }
    }
}
