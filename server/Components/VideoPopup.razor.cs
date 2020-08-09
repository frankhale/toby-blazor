using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using TobyBlazor.Data;
using TobyBlazor.Models;

namespace TobyBlazor.Components
{
    public partial class VideoPopup : ComponentBase
    {
        [Inject]
        IJSRuntime JSRuntime { get; set; }

        [Parameter]
        public Video SelectedVideo { get; set; }

        [Parameter]
        public EventCallback OnPopupClosed { get; set; }

        private readonly IVideoRepository videos = new VideoRepository();

        private bool AddedToFavorites = false;

        protected override void OnInitialized()
        {
            AddedToFavorites = IsAddedToFavorites(SelectedVideo);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await OpenModal();
            }
        }

        public bool IsAddedToFavorites(Video video)
        {
            var found = videos.FindVideoByYTId(video.YTId, "Favorites");

            if (found == null || found.Group != "Favorites")
            {
                return false;
            }

            return true;
        }

        public async void OnModalCloseClicked()
        {
            await JSRuntime.InvokeVoidAsync("closeModal", "ytModal");
            await OnPopupClosed.InvokeAsync(EventCallback.Empty);
        }

        private async Task OpenModal()
        {
            await JSRuntime.InvokeVoidAsync("openModal", "ytModal");
        }

        public void OnAddToFavoritesButtonToggle()
        {
            if (!AddedToFavorites)
            {
                AddedToFavorites = true;
                videos.AddVideo(SelectedVideo, "Favorites");
            }
            else
            {
                AddedToFavorites = false;
                videos.DeleteVideo(SelectedVideo.YTId, "Favorites");
            }
        }
    }
}
