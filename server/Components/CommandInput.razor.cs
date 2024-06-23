using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TobyBlazor.Data;
using TobyBlazor.Models;

namespace TobyBlazor.Components
{
  public abstract partial class CommandInput : ComponentBase
  {
    [Parameter]
    public EventCallback<SearchResult> OnSearch { get; set; }

    public abstract class ThrowAwayDataItem : IDataItem { }

    private readonly VideoRepository _videos = new();

    private string SearchTerm { get; set; } = "";

    private static async Task<SearchResult> CreateResult(SearchResultType type, Func<Task<Message>> action)
    {
      return await CreateResult<ThrowAwayDataItem>(type, null, null, action);
    }

    private static async Task<SearchResult> CreateResult<T>(SearchResultType type, List<T> data) where T : IDataItem
    {
      return await CreateResult(type, null, data, null);
    }

    private static async Task<SearchResult> CreateResult<T>(SearchResultType type, Message message, List<T> data, Func<Task<Message>> action) where T : IDataItem
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

      if (data == null) return result;
      result.Type = type;
      result.Videos = data as List<Video>;

      return result;
    }

    private async Task OnKeyPress(string key)
    {
      if (key != "Enter" || string.IsNullOrEmpty(SearchTerm)) return;
      var searchValue = SearchTerm.ToLower().Split([' '], 2);

      var result = searchValue switch
      {
        _ when MatchesCommandList(searchValue[0], "/clear") => CreateResult(SearchResultType.Search, new List<Video>()),
        _ when MatchesCommandList(searchValue[0], "/mg", "/manage") => CreateResult(SearchResultType.Manage, await _videos.AllVideosAsync()),
        _ when MatchesCommandList(searchValue[0], "/mgg", "/manage-groups") => CreateResult(SearchResultType.ManageGroups, await _videos.AllGroupsAsync()),
        _ when MatchesCommandList(searchValue[0], "/mgrp", "/manage-recently-played") => CreateResult(SearchResultType.ManageRecentlyPlayed, await _videos.FindVideoByGroupAsync("Recently Played")),
        _ when MatchesCommandList(searchValue[0], "/crp", "/clear-recently-played") =>
          CreateResult(SearchResultType.Command,
            async () =>
            {
              await _videos.DeleteVideoRangeByGroupAsync("Recently Played");
              return new Message() { Value = "Deleted all the videos in the Recently Played group", Type = "alert-danger" };
            }
          ),
        _ => CreateResult(SearchResultType.Search, await _videos.SearchAsync(SearchTerm))
      };

      await OnSearch.InvokeAsync(await result);

      return;

      static bool MatchesCommandList(string value, params string[] commands) => commands.FirstOrDefault(x => x == value) != null;
    }
  }
}
