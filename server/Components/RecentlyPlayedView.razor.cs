using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using TobyBlazor.Models;

namespace TobyBlazor.Components
{
    public partial class RecentlyPlayedView : ComponentBase
    {
        [Parameter]
        public List<Video> Videos { get; set; }

        private Video SelectedVideo { get; set; }

        [Parameter]
        public EventCallback<Video> OnSelectedVideo { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (SelectedVideo != null)
            {
                await Task.Run(() => { SelectedVideo = null; });
            }
        }
        private async Task VideoSelected(Video video)
        {
            SelectedVideo = video;
            await OnSelectedVideo.InvokeAsync(video);
        }
    }
}
