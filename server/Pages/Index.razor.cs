using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;
using TobyBlazor.Data;
using TobyBlazor.Models;

namespace TobyBlazor.Pages
{
    public partial class Index : ComponentBase
    {
        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        private readonly IVideoRepository videos = new VideoRepository();
        private SearchResult Result { get; set; } = new SearchResult();
        private List<Video> topFiveRecentlyPlayed = new List<Video>();
        private Video SelectedVideo { get; set; }


        protected override async Task OnInitializedAsync()
        {
            topFiveRecentlyPlayed = await videos.GetRecentlyPlayedVideosAsync(5);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JsRuntime.InvokeVoidAsync("setModalCloseStopVideo", "ytModal");
            }
        }

        public void OnSearchResults(SearchResult result)
        {
            Result = result;
        }

        public void OnNotificationClose()
        {
            if (Result != null)
            {
                Result = new SearchResult();
            }
        }

        public async Task OnSelectedVideo(Video video)
        {
            topFiveRecentlyPlayed = await videos.GetRecentlyPlayedVideosAsync(5);
            SelectedVideo = video;
        }

        public void OnVideoDismissed()
        {
            SelectedVideo = null;
        }
    }
}
