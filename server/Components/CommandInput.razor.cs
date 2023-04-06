using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TobyBlazor.Data;
using TobyBlazor.Models;

namespace TobyBlazor.Components
{
  public partial class CommandInput : ComponentBase
  {
    [Parameter]
    public EventCallback<SearchResult> OnSearch { get; set; }

    public class ThrowAwayDataItem : IDataItem { }

    private readonly IVideoRepository videos = new VideoRepository();

    private string SearchTerm { get; set; } = "";

    private async static Task<SearchResult> CreateResult(SearchResultType type, Func<Task<Message>> action)
    {
      return await CreateResult<ThrowAwayDataItem>(type, null, null, action);
    }

    private async static Task<SearchResult> CreateResult<T>(SearchResultType type, List<T> data) where T : IDataItem
    {
      return await CreateResult(type, null, data, null);
    }

    private async static Task<SearchResult> CreateResult<T>(SearchResultType type, Message message, List<T> data, Func<Task<Message>> action) where T : IDataItem
    {
      var result = new SearchResult();

      if (action != null)
      {
        result.Message = await action();
      }
      else
      {
        result.Message = message;
      }

      if (data != null)
      {
        result.Type = type;
        result.Videos = data as List<Video>;
      }

      return result;
    }

    private async Task OnKeyPress(string key)
    {
      static bool MatchesCommandList(string value, params string[] commands) => commands.FirstOrDefault(x => x == value) != null;

      if (key == "Enter" && !string.IsNullOrEmpty(SearchTerm))
      {
        var searchValue = SearchTerm.ToLower().Split(new char[] { ' ' }, 2);

        var result = searchValue switch
        {
          _ when MatchesCommandList(searchValue[0], "/clear") => CreateResult(SearchResultType.Search, new List<Video>()),
          _ when MatchesCommandList(searchValue[0], "/mg", "/manage") => CreateResult(SearchResultType.Manage, await videos.AllVideosAsync()),
          _ when MatchesCommandList(searchValue[0], "/mgg", "/manage-groups") => CreateResult(SearchResultType.ManageGroups, await videos.AllGroupsAsync()),
          _ when MatchesCommandList(searchValue[0], "/mgrp", "/manage-recently-played") => CreateResult(SearchResultType.ManageRecentlyPlayed, await videos.FindVideoByGroupAsync("Recently Played")),
          _ when MatchesCommandList(searchValue[0], "/crp", "/clear-recently-played") =>
              CreateResult(SearchResultType.Command,
                  async () =>
                  {
                    await videos.DeleteVideoRangeByGroupAsync("Recently Played");
                    return new Message() { Value = "Deleted all the videos in the Recently Played group", Type = "alert-danger" };
                  }
              ),
          _ => CreateResult(SearchResultType.Search, await videos.SearchAsync(SearchTerm))
        };

        await OnSearch.InvokeAsync(await result);
      }
    }
  }
}
