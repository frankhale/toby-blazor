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
        private readonly IVideoRepository videos = new VideoRepository();

        private string SearchTerm { get; set; } = "";

        [Parameter]
        public EventCallback<SearchResult> OnSearch { get; set; }
        
        public class ThrowAwayDataItem : IDataItem { }

        private async Task<SearchResult> CreateResult(SearchResultType type, Func<Task<Message>> action)
        {            
            return await CreateResult<ThrowAwayDataItem>(type, null, null, action);
        }

        private async Task<SearchResult> CreateResult<T>(SearchResultType type, List<T> data) where T : IDataItem
        {
            return await CreateResult(type, null, data, null);
        }

        private async Task<SearchResult> CreateResult<T>(SearchResultType type, Message message, List<T> data, Func<Task<Message>> action) where T : IDataItem
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

        private async void OnKeyPress(string key)
        {
            static bool MatchesCommandList(string value, params string[] commands) => (commands.Where(x => x == value).FirstOrDefault() != null);

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
                    _ when MatchesCommandList(searchValue[0], "/mg", "/manage") => CreateResult(SearchResultType.Manage, await videos.AllVideosAsync()),
                    _ when MatchesCommandList(searchValue[0], "/mgg", "/manage-groups") => CreateResult(SearchResultType.ManageGroups, await videos.AllGroupsAsync()),
                    _ when MatchesCommandList(searchValue[0], "/crp", "/clear-recently-played") =>
                        CreateResult(SearchResultType.Command,
                            async () => {
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
