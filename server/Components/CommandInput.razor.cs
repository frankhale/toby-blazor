﻿using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using TobyBlazor.Data;
using TobyBlazor.Models;

namespace TobyBlazor.Components
{
    public partial class CommandInput : ComponentBase
    {
        private readonly IVideoRepository videos = new VideoRepository();

        private string SearchTerm { get; set; } = "";

        [Parameter]
        public EventCallback<SearchResult> OnSearch { get; set; }

        //FIXME: THIS IS LITERALLY THE DUMBEST CODE I'VE EVERY WRITTEN (eg. GET RID OF ThrowAwayDataItem>)
        public class ThrowAwayDataItem : IDataItem { }

        private SearchResult CreateResult(SearchResultType type, Func<Message> action)
        {
            //FIXME: THIS IS LITERALLY THE DUMBEST CODE I'VE EVERY WRITTEN (eg. GET RID OF ThrowAwayDataItem>)
            return CreateResult<ThrowAwayDataItem>(type, null, null, action);
        }

        private SearchResult CreateResult<T>(SearchResultType type, List<T> data) where T : IDataItem
        {
            return CreateResult(type, null, data, null);
        }

        private SearchResult CreateResult<T>(SearchResultType type, Message message, List<T> data, Func<Message> action) where T : IDataItem
        {
            var result = new SearchResult();

            if (action != null)
            {
                result.Message = action();
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

        private async void OnKeyPress(string key)
        {
            static bool MatchesCommandList(string value, params string[] commands) => (commands.Where(x => x == value).FirstOrDefault() != null) ? true : false;

            if (key == "Enter" && !String.IsNullOrEmpty(SearchTerm))
            {
                var searchValue = SearchTerm.ToLower().Split(new char[] { ' ' }, 2);

                var result = searchValue switch
                {
                    //_ when MatchesCommandList(searchValue[0], "/t", "/test") =>
                    //    CreateResult(SearchResultType.Command,
                    //    () => {
                    //        return new Message() { Value = "This is a test message...", Type = "alert-warning" };
                    //    }),
                    _ when MatchesCommandList(searchValue[0], "/clear") => CreateResult(SearchResultType.Search, new List<Video>()),
                    _ when MatchesCommandList(searchValue[0], "/mg", "/manage") => CreateResult(SearchResultType.Manage, videos.AllVideos()),
                    _ when MatchesCommandList(searchValue[0], "/mgg", "/manage-groups") => CreateResult(SearchResultType.ManageGroups, videos.AllGroups()),
                    _ when MatchesCommandList(searchValue[0], "/crp", "/clear-recently-played") =>
                        CreateResult(SearchResultType.Command,
                            () => {
                                videos.DeleteVideoRangeByGroup("Recently Played");
                                return new Message() { Value = "Deleted all the videos in the Recently Played group", Type = "alert-danger" };
                            }
                        ),
                    _ => CreateResult(SearchResultType.Search, await videos.SearchAsync(SearchTerm))
                };

                await OnSearch.InvokeAsync(result);
            }
        }
    }
}
