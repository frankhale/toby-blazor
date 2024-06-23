using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TobyBlazor.Data;
using TobyBlazor.Models;

namespace TobyBlazor.Pages
{
  public partial class Index : ComponentBase
  {
    [Inject]
    private IJSRuntime JsRuntime { get; set; }

    private readonly VideoRepository _videos = new();
    private SearchResult Result { get; set; } = new();
    private List<Video> _topFiveRecentlyPlayed = [];
    private Video SelectedVideo { get; set; }

    private bool RecentlyPlayed { get; set; }

    protected override async Task OnInitializedAsync()
    {
      _topFiveRecentlyPlayed = await _videos.GetRecentlyPlayedVideosAsync(5);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
      if (firstRender)
      {
        await JsRuntime.InvokeVoidAsync("setModalCloseStopVideo", "ytModal");
      }
    }

    private void OnSearchResults(SearchResult result)
    {
      Result = result;
      RecentlyPlayed = result.Videos.Any(x => x.Group == "Recently Played");
      this.StateHasChanged();
    }

    private void OnNotificationClose()
    {
      if (Result != null)
      {
        Result = new SearchResult();
      }
    }

    private async Task OnSelectedVideo(Video video)
    {
      _topFiveRecentlyPlayed = await _videos.GetRecentlyPlayedVideosAsync(5);
      SelectedVideo = video;
    }

    private void OnVideoDismissed()
    {
      SelectedVideo = null;
    }

    private async Task OnDeleteVideo()
    {
      _topFiveRecentlyPlayed = await _videos.GetRecentlyPlayedVideosAsync(5);
    }
  }
}
