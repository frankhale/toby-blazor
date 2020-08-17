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

        protected override async Task OnInitializedAsync()
        {
            AddedToFavorites = await IsAddedToFavorites(SelectedVideo);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await OpenModal();
            }
        }

        private async Task OpenModal()
        {
            await JSRuntime.InvokeVoidAsync("openModal", "ytModal");
        }

        public async Task<bool> IsAddedToFavorites(Video video)
        {
            var found = await videos.FindVideoByYTIdAsync(video.YTId, "Favorites");

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

        public async Task OnAddToFavoritesButtonToggle()
        {
            if (!AddedToFavorites)
            {
                AddedToFavorites = true;
                await videos.AddVideoAsync(SelectedVideo, "Favorites");
            }
            else
            {
                AddedToFavorites = false;
                await videos.DeleteVideoAsync(SelectedVideo.YTId, "Favorites");
            }
        }
    }
}
