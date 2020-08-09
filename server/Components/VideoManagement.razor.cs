using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TobyBlazor.Data;
using TobyBlazor.Models;

namespace TobyBlazor.Components
{
    public partial class VideoManagement : ComponentBase
    {
        private readonly IVideoRepository videos = new VideoRepository();

        private string SearchTerm { get; set; } = String.Empty;

        [Parameter]
        public EventCallback<string> SearchTermChanged { get; set; }

        private Task OnSearchTermChanged(ChangeEventArgs e)
        {
            SearchTerm = e.Value.ToString();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                SearchButtonDisabled = false;
            }
            else
            {
                SearchButtonDisabled = true;

                Videos = videos.AllVideos();
            }

            return SearchTermChanged.InvokeAsync(SearchTerm);
        }

        private bool DeleteButtonDisabled { get; set; } = true;
        private bool ApplyButtonDisabled { get; set; } = true;
        private bool SearchButtonDisabled { get; set; } = true;

        private List<Video> Videos { get; set; } = new List<Video>();
        private List<string> VideosToDelete = new List<string>();
        private List<Tuple<string, string>> VideoGroupsToChange = new List<Tuple<string, string>>();

        protected override void OnInitialized()
        {
            Videos = videos.AllVideos();
        }

        private string GetVideoThumbnail(string ytid)
        {
            return String.Format("https://i.ytimg.com/vi/{0}/default.jpg", ytid);
        }

        private void Search(string searchTerm)
        {
            if (!String.IsNullOrEmpty(searchTerm))
            {
                Videos = searchTerm switch
                {
                    _ when searchTerm.ToLower().StartsWith("/all") => videos.AllVideos(),
                    _ => videos.FindVideo(searchTerm)
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

        private void OnDeleteButtonClicked()
        {
            if (VideosToDelete.Count > 0)
            {
                var deleteVideos = new List<Video>();

                VideosToDelete.ForEach(x =>
                {
                    var foundVideo = videos.FindVideoByYTId(x);
                    if (foundVideo != null)
                    {
                        deleteVideos.Add(foundVideo);
                    }
                });

                if (deleteVideos.Count > 0)
                {
                    videos.DeleteVideoRange(deleteVideos);
                    VideosToDelete = new List<string>();
                    DeleteButtonDisabled = true;

                    if (!String.IsNullOrEmpty(SearchTerm))
                    {
                        Search(SearchTerm);
                    }
                    else
                    {
                        Videos = videos.AllVideos();
                    }
                }
            }
        }

        private void OnApplyButtonClicked()
        {
            if (VideoGroupsToChange.Count > 0)
            {
                VideoGroupsToChange.ForEach(x =>
                {
                    videos.UpdateVideoGroup(x.Item1, x.Item2);
                });

                VideoGroupsToChange = new List<Tuple<string, string>>();
                ApplyButtonDisabled = true;
            }
        }

        private void OnSearchButtonClicked()
        {
            Search(SearchTerm);
        }
    }
}
