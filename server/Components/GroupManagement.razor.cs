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
        private string SearchTerm { get; set; } = string.Empty;
        private bool DeleteButtonDisabled { get; set; } = true;
        private bool AddButtonDisabled { get; set; } = true;
        private bool SearchButtonDisabled { get; set; } = true;
        private List<Group> Groups { get; set; } = new List<Group>();
        private List<string> GroupsToDelete = new List<string>();       

        protected override async Task OnInitializedAsync()
        {
            Groups = await videos.AllGroupsAsync();
        }

        private async Task OnSearchTermChanged(ChangeEventArgs e)
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

                Groups = await videos.AllGroupsAsync();
            }
        }

        private async Task Search(string searchTerm)
        {
            if (!String.IsNullOrEmpty(searchTerm))
            {
                Groups = searchTerm switch
                {
                    _ when searchTerm.ToLower().StartsWith("/all") => await videos.AllGroupsAsync(),
                    _ => await videos.FindGroupAsync(searchTerm)
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

        private async Task OnDeleteButtonClicked()
        {
            if (GroupsToDelete.Count > 0)
            {
                var deleteGroups = new List<Group>();

                GroupsToDelete.ForEach(async x =>
                {
                    var foundGroup = await videos.FindGroupByNameAsync(x);
                    if (foundGroup != null)
                    {
                        deleteGroups.Add(foundGroup);
                    }
                });

                if (deleteGroups.Count > 0)
                {
                    await videos.DeleteGroupRangeAsync(deleteGroups);
                    GroupsToDelete = new List<string>();
                    DeleteButtonDisabled = true;

                    Groups = await videos.AllGroupsAsync();
                }
            }
        }

        private async Task OnAddButtonClicked()
        {
            await videos.AddGroupAsync(SearchTerm);
            Groups = await videos.AllGroupsAsync();
            // FIXME: This is not working, boo! Can't bind value and do on-input stupid fucking this doesn't work...!!!?!?!?!?!?!?!
            SearchTerm = string.Empty;
            await OnSearchTermChanged(new ChangeEventArgs() { Value = String.Empty });
        }

        private async Task OnSearchButtonClicked()
        {
            await Search(SearchTerm);
        }
    }
}
