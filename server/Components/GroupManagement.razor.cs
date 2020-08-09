using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TobyBlazor.Data;
using TobyBlazor.Models;

namespace TobyBlazor.Components
{
    public partial class GroupManagement : ComponentBase
    {
        private readonly IVideoRepository videos = new VideoRepository();

        private string SearchTerm;

        [Parameter]
        public EventCallback<string> SearchTermChanged { get; set; }

        private Task OnSearchTermChanged(ChangeEventArgs e)
        {
            SearchTerm = e.Value.ToString();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                AddButtonDisabled = false;
                SearchButtonDisabled = false;
            }
            else
            {
                AddButtonDisabled = true;
                SearchButtonDisabled = true;

                Groups = videos.AllGroups();
            }

            return SearchTermChanged.InvokeAsync(SearchTerm);
        }
        
        private bool DeleteButtonDisabled { get; set; } = true;
        private bool AddButtonDisabled { get; set; } = true;
        private bool SearchButtonDisabled { get; set; } = true;

        private List<Group> Groups { get; set; } = new List<Group>();
        private List<string> GroupsToDelete = new List<string>();

        protected override void OnInitialized()
        {
            Groups = videos.AllGroups();
        }

        private void Search(string searchTerm)
        {
            if (!String.IsNullOrEmpty(searchTerm))
            {
                Groups = searchTerm switch
                {
                    _ when searchTerm.ToLower().StartsWith("/all") => videos.AllGroups(),
                    _ => videos.FindGroup(searchTerm)
                };
            }
        }

        private void OnCheckboxClicked(string name, object checkedValue)
        {
            if ((bool)checkedValue)
            {
                if (!GroupsToDelete.Contains(name))
                {
                    GroupsToDelete.Add(name);
                }
            }
            else
            {
                if (GroupsToDelete.Contains(name))
                {
                    GroupsToDelete.Remove(name);
                }
            }

            DeleteButtonDisabled = (GroupsToDelete.Count > 0) ? false : true;
        }

        private void OnDeleteButtonClicked()
        {
            if (GroupsToDelete.Count > 0)
            {
                var deleteGroups = new List<Group>();

                GroupsToDelete.ForEach(x =>
                {
                    var foundGroup = videos.FindGroupByName(x);
                    if (foundGroup != null)
                    {
                        deleteGroups.Add(foundGroup);
                    }
                });

                if (deleteGroups.Count > 0)
                {
                    videos.DeleteGroupRange(deleteGroups);
                    GroupsToDelete = new List<string>();
                    DeleteButtonDisabled = true;

                    Groups = videos.AllGroups();
                }
            }
        }

        private async Task OnAddButtonClicked()
        {
            videos.AddGroup(SearchTerm);
            Groups = videos.AllGroups();
            SearchTerm = String.Empty;
            //await SearchTermChanged.InvokeAsync(String.Empty);
            await OnSearchTermChanged(new ChangeEventArgs() { Value = String.Empty });
        }

        private void OnSearchButtonClicked()
        {
            Search(SearchTerm);
        }
    }
}
