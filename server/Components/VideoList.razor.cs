using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using TobyBlazor.Data;
using TobyBlazor.Models;

namespace TobyBlazor.Components
{
    public partial class VideoList : ComponentBase
    {
        [Parameter]
        public List<Video> Videos { get; set; }

        [Parameter]
        public bool RecentlyPlayed { get; set; } = false;

        [Parameter]
        public EventCallback<Video> OnSelectedVideo { get; set; }

        private readonly IVideoRepository videos = new VideoRepository();
        private Video SelectedVideo { get; set; }

        protected override void OnInitialized()
        {
            if (RecentlyPlayed)
            {
                Videos = videos.GetRecentlyPlayedVideos(5);
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (SelectedVideo != null)
            {
                SelectedVideo = null;
            }
        }

        private async Task VideoSelected(Video video)
        {
            SelectedVideo = video;
            videos.AddVideoToRecentlyPlayed(video);
            await OnSelectedVideo.InvokeAsync(video);
        }
    }
}
