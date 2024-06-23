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
    [Inject] private IJSRuntime JsRuntime { get; set; }

    [Parameter]
    public Video SelectedVideo { get; set; }

    [Parameter]
    public EventCallback OnPopupClosed { get; set; }

    private readonly VideoRepository _videos = new();
    private bool _addedToFavorites;
    private DotNetObjectReference<VideoPopup> _dotNetObjectReference;

    protected override async Task OnInitializedAsync()
    {
      _dotNetObjectReference = DotNetObjectReference.Create(this);
      _addedToFavorites = await IsAddedToFavorites(SelectedVideo);
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
      await JsRuntime.InvokeVoidAsync("openModal", "ytModal", _dotNetObjectReference);
    }

    public async Task<bool> IsAddedToFavorites(Video video)
    {
      var found = await _videos.FindVideoByYTIdAsync(video.YTId, "Favorites");

      return found is { Group: "Favorites" };
    }

    [JSInvokable("OnModalCloseClicked")]
    public async Task OnModalCloseClicked()
    {
      await JsRuntime.InvokeVoidAsync("closeModal", "ytModal");
      await JsRuntime.InvokeVoidAsync("setModalCloseStopVideo", "ytModal");
      await OnPopupClosed.InvokeAsync(EventCallback.Empty);
    }

    public async Task OnAddToFavoritesButtonToggle()
    {
      if (!_addedToFavorites)
      {
        _addedToFavorites = true;
        await _videos.AddVideoAsync(SelectedVideo, "Favorites");
      }
      else
      {
        _addedToFavorites = false;
        await _videos.DeleteVideoAsync(SelectedVideo.YTId, "Favorites");
      }
    }

    public void Dispose()
    {
      _dotNetObjectReference?.Dispose();
      GC.SuppressFinalize(this);
    }
  }
}
