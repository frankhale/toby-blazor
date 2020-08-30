using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TobyBlazor.Data;
using TobyBlazor.Models;

namespace TobyBlazor.Components
{
    public partial class VideoManagement : ComponentBase
    {
        [Parameter]
        public EventCallback OnDeleteVideo { get; set; }
        [Parameter]
        public List<Video> Videos { get; set; }
        [Parameter]
        public bool RecentlyPlayed { get; set; }

        private readonly IVideoRepository videos = new VideoRepository();
        private string SearchTerm { get; set; } = string.Empty;
        private bool DeleteButtonDisabled { get; set; } = true;
        private bool ApplyButtonDisabled { get; set; } = true;
        private bool SearchButtonDisabled { get; set; } = true;
        private List<string> VideosToDelete = new List<string>();
        private List<Tuple<string, string>> VideoGroupsToChange = new List<Tuple<string, string>>();

        private async Task OnSearchTermChanged(ChangeEventArgs e)
        {
            SearchTerm = e.Value.ToString();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                SearchButtonDisabled = false;
            }
            else
            {
                SearchButtonDisabled = true;

                Videos = await videos.AllVideosAsync();
            }
        }

        private static string GetVideoThumbnail(string ytid)
        {
            return string.Format("https://i.ytimg.com/vi/{0}/default.jpg", ytid);
        }

        private async Task SearchAsync(string searchTerm)
        {
            if (!string.IsNullOrEmpty(searchTerm))
            {
                Videos = searchTerm switch
                {
                    _ when searchTerm.ToLower().StartsWith("/all") => await videos.AllVideosAsync(),
                    _ => await videos.FindVideoAsync(searchTerm)
                };
            }
        }

        private void OnMenuItemSelected(Video video, string group)
        {
            var found = VideoGroupsToChange.Find(x => x.Item1 == video.YTId);

            if (video.Group != group && found == null)
            {
                video.Group = group;
                VideoGroupsToChange.Add(new Tuple<string, string>(video.YTId, group));
            }
            else
            {
                if (found != null)
                {
                    VideoGroupsToChange.Remove(found);
                }
            }

            ApplyButtonDisabled = VideoGroupsToChange.Count <= 0;
        }

        private void OnCheckboxClicked(string ytid, object checkedValue)
        {
            if ((bool)checkedValue)
            {
                if (!VideosToDelete.Contains(ytid))
                {
                    VideosToDelete.Add(ytid);
                }
            }
            else
            {
                if (VideosToDelete.Contains(ytid))
                {
                    VideosToDelete.Remove(ytid);
                }
            }

            DeleteButtonDisabled = VideosToDelete.Count <= 0;
        }

        private async Task OnDeleteButtonClicked()
        {
            if (VideosToDelete.Count > 0)
            {
                var deleteVideos = new List<Video>();

                VideosToDelete.ForEach(async x =>
                {
                    var foundVideo = await videos.FindVideoByYTIdAsync(x);
                    Videos.RemoveAll(v => v.YTId == x);

                    if (foundVideo != null)
                    {
                        deleteVideos.Add(foundVideo);
                    }
                });

                if (deleteVideos.Count > 0)
                {
                    await videos.DeleteVideoRangeAsync(deleteVideos);

                    VideosToDelete = new List<string>();
                    DeleteButtonDisabled = true;

                    if (!RecentlyPlayed)
                    {
                        if (!string.IsNullOrEmpty(SearchTerm))
                        {
                            await SearchAsync(SearchTerm);
                        }
                        else
                        {
                            Videos = await videos.AllVideosAsync();
                        }
                    }
                }

                await OnDeleteVideo.InvokeAsync(null);
            }
        }

        private void OnApplyButtonClicked()
        {
            if (VideoGroupsToChange.Count > 0)
            {
                VideoGroupsToChange.ForEach(async x =>
                {
                    await videos.UpdateVideoGroupAsync(x.Item1, x.Item2);
                });

                VideoGroupsToChange = new List<Tuple<string, string>>();
                ApplyButtonDisabled = true;
            }
        }

        private async Task OnSearchButtonClicked()
        {
            await SearchAsync(SearchTerm);
        }
    }
}
