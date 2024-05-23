using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using TobyBlazor.Data;
using TobyBlazor.Models;

namespace TobyBlazor.Components
{
  public partial class VideoPopup : ComponentBase, IDisposable
  {
    [Inject]
    IJSRuntime JSRuntime { get; set; }

    [Parameter]
    public Video SelectedVideo { get; set; }

    [Parameter]
    public EventCallback OnPopupClosed { get; set; }

    private readonly VideoRepository videos = new();

    private bool AddedToFavorites;

    private DotNetObjectReference<VideoPopup> dotNetObjectReference;

    protected override async Task OnInitializedAsync()
    {
      dotNetObjectReference = DotNetObjectReference.Create(this);
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
      await JSRuntime.InvokeVoidAsync("openModal", "ytModal", dotNetObjectReference);
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

    [JSInvokable("OnModalCloseClicked")]
    public async Task OnModalCloseClicked()
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

    public void Dispose() => dotNetObjectReference?.Dispose();
  }
}
