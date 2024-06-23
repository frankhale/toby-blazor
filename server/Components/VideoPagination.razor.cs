using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TobyBlazor.Data;
using TobyBlazor.Other;

namespace TobyBlazor.Components
{
  public partial class VideoPagination : ComponentBase
  {
    [Parameter]
    public List<Models.Video> Videos { get; set; }

    [Parameter]
    public RenderFragment<List<Models.Video>> Content { get; set; }

    [Parameter]
    public bool RecentlyPlayed { get; set; }

    private readonly VideoRepository videos = new();
    private const int ChunkSize = 10;
    private List<List<Models.Video>> Pages { get; set; }
    private List<List<int>> PageIndices { get; set; }
    private int CurrentPage { get; set; } = 1;
    private int CurrentPageLinkPage { get; set; } = 1;
    private string Message { get; set; } = string.Empty;
    private bool PreviousButtonDisabled { get; set; }
    private bool NextButtonDisabled { get; set; }

    protected override async Task OnParametersSetAsync()
    {
      await base.OnParametersSetAsync();
      await InitializePages(true);
    }

    private async Task InitializePages(bool resetCurrentPage = false)
    {
      var preferences = await videos.GetCurrentPreferences();
      var preferredPage = 1;
      var preferredLinkPage = 1;

      Pages = Videos.ChunkBy(ChunkSize);
      PageIndices = Enumerable.Range(1, Pages.Count).ToList().ChunkBy(ChunkSize);

      if (preferences != null)
      {
        preferredPage = (RecentlyPlayed) ? preferences.CurrentRecentlyPlayedVideoPage : preferences.CurrentVideoPage;
        preferredLinkPage = preferences.CurrentVideoPageLinkPage;

        if (preferredPage == 0 || preferredPage > Pages.Count)
        {
          preferredPage = 1;
        }

        if (preferredLinkPage == 0 || preferredLinkPage > PageIndices.Count)
        {
          preferredLinkPage = 1;
        }
      }

      if (resetCurrentPage)
      {
        CurrentPage = preferredPage;
        CurrentPageLinkPage = preferredLinkPage;
      }

      if (PageIndices.Count <= 0)
      {
        Message = "There are no results";
      }

      TogglePrevNextButtonsDisabled();
    }

    private void TogglePrevNextButtonsDisabled()
    {
      PreviousButtonDisabled = CurrentPageLinkPage == 1;
      NextButtonDisabled = CurrentPageLinkPage >= PageIndices.Count;
    }

    private async Task OnPageButtonClick(int page)
    {
      await videos.SetCurrentVideoPage(RecentlyPlayed, page, CurrentPageLinkPage);

      CurrentPage = page;
    }

    private async Task OnPreviousButtonClick()
    {
      if (CurrentPageLinkPage > 1)
      {
        CurrentPageLinkPage -= 1;
      }

      await videos.SetCurrentVideoPage(RecentlyPlayed, CurrentPage, CurrentPageLinkPage);

      TogglePrevNextButtonsDisabled();
    }

    private async Task OnNextButtonClick()
    {
      if (CurrentPageLinkPage < PageIndices.Count)
      {
        CurrentPageLinkPage += 1;
      }

      await videos.SetCurrentVideoPage(RecentlyPlayed, CurrentPage, CurrentPageLinkPage);

      TogglePrevNextButtonsDisabled();
    }

    private void OnNotificationClose() => Message = string.Empty;
  }
}
