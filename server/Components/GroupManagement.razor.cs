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
    private readonly VideoRepository _videos = new();
    private string SearchTerm { get; set; } = string.Empty;
    private bool DeleteButtonDisabled { get; set; } = true;
    private bool AddButtonDisabled { get; set; } = true;
    private bool SearchButtonDisabled { get; set; } = true;
    private List<Group> Groups { get; set; } = [];
    private List<string> _groupsToDelete = [];

    protected override async Task OnInitializedAsync()
    {
      Groups = await _videos.AllGroupsAsync();
    }

    private async Task OnSearchTermChanged(ChangeEventArgs e)
    {
      SearchTerm = e.Value?.ToString();

      if (!string.IsNullOrEmpty(SearchTerm))
      {
        AddButtonDisabled = false;
        SearchButtonDisabled = false;
      }
      else
      {
        AddButtonDisabled = true;
        SearchButtonDisabled = true;

        Groups = await _videos.AllGroupsAsync();
      }
    }

    private async Task Search(string searchTerm)
    {
      if (!string.IsNullOrEmpty(searchTerm))
      {
        Groups = searchTerm switch
        {
          _ when searchTerm.StartsWith("/all", StringComparison.InvariantCultureIgnoreCase) => await _videos.AllGroupsAsync(),
          _ => await _videos.FindGroupAsync(searchTerm)
        };
      }
    }

    private void OnCheckboxClicked(string name, object checkedValue)
    {
      if ((bool)checkedValue)
      {
        if (!_groupsToDelete.Contains(name))
        {
          _groupsToDelete.Add(name);
        }
      }
      else
      {
        if (_groupsToDelete.Contains(name))
        {
          _groupsToDelete.Remove(name);
        }
      }

      DeleteButtonDisabled = _groupsToDelete.Count <= 0;
    }

    private async Task OnDeleteButtonClicked()
    {
      if (_groupsToDelete.Count > 0)
      {
        var deleteGroups = new List<Group>();

        _groupsToDelete.ForEach(async x =>
        {
          var foundGroup = await _videos.FindGroupByNameAsync(x);
          if (foundGroup != null)
          {
            deleteGroups.Add(foundGroup);
          }
        });

        if (deleteGroups.Count > 0)
        {
          await _videos.DeleteGroupRangeAsync(deleteGroups);
          _groupsToDelete = [];
          DeleteButtonDisabled = true;

          Groups = await _videos.AllGroupsAsync();
        }
      }
    }

    private async Task OnAddButtonClicked()
    {
      await _videos.AddGroupAsync(SearchTerm);
      Groups = await _videos.AllGroupsAsync();
      SearchTerm = string.Empty;
      await OnSearchTermChanged(new ChangeEventArgs { Value = string.Empty });
    }

    private async Task OnSearchButtonClicked()
    {
      await Search(SearchTerm);
    }
  }
}
