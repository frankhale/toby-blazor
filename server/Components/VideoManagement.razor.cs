using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TobyBlazor.Data;
using TobyBlazor.Models;

namespace TobyBlazor.Components
{
  public partial class VideoManagement : ComponentBase
  {
    [Parameter]
    public EventCallback OnDeleteVideo { get; set; }
    [Parameter]
    public List<Video> Videos { get; set; }
    [Parameter]
    public bool RecentlyPlayed { get; set; }

    private readonly VideoRepository _videos = new();
    private string SearchTerm { get; set; } = string.Empty;
    private bool DeleteButtonDisabled { get; set; } = true;
    private bool ApplyButtonDisabled { get; set; } = true;
    private bool SearchButtonDisabled { get; set; } = true;
    private List<string> _videosToDelete = new List<string>();
    private List<Tuple<string, string>> _videoGroupsToChange = new List<Tuple<string, string>>();

    private async Task OnSearchTermChanged(ChangeEventArgs e)
    {
      SearchTerm = e.Value?.ToString();

      if (!string.IsNullOrEmpty(SearchTerm))
      {
        SearchButtonDisabled = false;
      }
      else
      {
        SearchButtonDisabled = true;

        Videos = await _videos.AllVideosAsync();
      }
    }

    private static string GetVideoThumbnail(string ytid)
    {
      return $"https://i.ytimg.com/vi/{ytid}/default.jpg";
    }

    private async Task SearchAsync(string searchTerm)
    {
      if (!string.IsNullOrEmpty(searchTerm))
      {
        Videos = searchTerm switch
        {
          _ when searchTerm.StartsWith("/all", StringComparison.InvariantCultureIgnoreCase) => await _videos.AllVideosAsync(),
          _ => await _videos.FindVideoAsync(searchTerm)
        };
      }
    }

    private void OnMenuItemSelected(Video video, string group)
    {
      var found = _videoGroupsToChange.Find(x => x.Item1 == video.YTId);

      if (video.Group != group && found == null)
      {
        video.Group = group;
        _videoGroupsToChange.Add(new Tuple<string, string>(video.YTId, group));
      }
      else
      {
        if (found != null)
        {
          _videoGroupsToChange.Remove(found);
        }
      }

      ApplyButtonDisabled = _videoGroupsToChange.Count <= 0;
    }

    private void OnCheckboxClicked(string ytid, object checkedValue)
    {
      if ((bool)checkedValue)
      {
        if (!_videosToDelete.Contains(ytid))
        {
          _videosToDelete.Add(ytid);
        }
      }
      else
      {
        _videosToDelete.Remove(ytid);
      }

      DeleteButtonDisabled = _videosToDelete.Count <= 0;
    }

    private async Task OnDeleteButtonClicked()
    {
      if (_videosToDelete.Count > 0)
      {
        var deleteVideos = new List<Video>();

        _videosToDelete.ForEach(async x =>
        {
          var foundVideo = await _videos.FindVideoByYTIdAsync(x);
          Videos.RemoveAll(v => v.YTId == x);

          if (foundVideo != null)
          {
            deleteVideos.Add(foundVideo);
          }
        });

        if (deleteVideos.Count > 0)
        {
          await _videos.DeleteVideoRangeAsync(deleteVideos);

          _videosToDelete = new List<string>();
          DeleteButtonDisabled = true;

          if (!RecentlyPlayed)
          {
            if (!string.IsNullOrEmpty(SearchTerm))
            {
              await SearchAsync(SearchTerm);
            }
            else
            {
              Videos = await _videos.AllVideosAsync();
            }
          }
        }

        await OnDeleteVideo.InvokeAsync(null);
      }
    }

    private void OnApplyButtonClicked()
    {
      if (_videoGroupsToChange.Count > 0)
      {
        _videoGroupsToChange.ForEach(async x =>
        {
          await _videos.UpdateVideoGroupAsync(x.Item1, x.Item2);
        });

        _videoGroupsToChange = new List<Tuple<string, string>>();
        ApplyButtonDisabled = true;
      }
    }

    private async Task OnSearchButtonClicked()
    {
      await SearchAsync(SearchTerm);
    }
  }
}
