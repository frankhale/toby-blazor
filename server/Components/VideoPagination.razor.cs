using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using TobyBlazor.Models;
using TobyBlazor.Other;

namespace TobyBlazor.Components
{
    public partial class VideoPagination : ComponentBase
    {
        [Parameter]
        public List<Video> Videos { get; set; }

        [Parameter]
        public RenderFragment<List<Video>> Content { get; set; }

        private const int ChunkSize = 10;
        private List<List<Video>> Pages { get; set; }
        private List<List<int>> PageIndices { get; set; }
        private int CurrentPage { get; set; } = 1;
        private int CurrentPageLinkPage { get; set; } = 1;
        private string Message { get; set; } = String.Empty;
        private bool PreviousButtonDisabled { get; set; } = false;
        private bool NextButtonDisabled { get; set; } = false;

        protected override void OnParametersSet()
        {
            InitializePages(true);
        }

        private void InitializePages(bool resetCurrentPage = false)
        {
            if (resetCurrentPage)
            {
                CurrentPage = 1;
                CurrentPageLinkPage = 1;
            }

            Pages = Videos.ChunkBy(ChunkSize);
            PageIndices = Enumerable.Range(1, Pages.Count).ToList().ChunkBy(ChunkSize);

            if (PageIndices.Count <= 0)
            {
                Message = "There are no results";
            }

            TogglePrevNextButtonsDisabled();

            //Console.WriteLine("Total Videos = {0}", Videos.Count);
            //Console.WriteLine("Total Pages = {0}", Pages.Count);
            //Console.WriteLine("Total Page Indicies = {0}", PageIndices.Count);
            //Console.WriteLine("Total Link Pages = {0}", Pages.Count / ChunkSize);
            //Console.WriteLine("Total Links Left Over = {0}", Pages.Count % ChunkSize);
        }

        private void TogglePrevNextButtonsDisabled()
        {
            PreviousButtonDisabled = CurrentPageLinkPage != 1 ? false : true;
            NextButtonDisabled = CurrentPageLinkPage < PageIndices.Count ? false : true;
        }

        private void OnPageButtonClick(int page)
        {
            CurrentPage = page;
        }

        private void OnPreviousButtonClick()
        {
            if (CurrentPageLinkPage > 1)
            {
                CurrentPageLinkPage -= 1;
            }

            TogglePrevNextButtonsDisabled();
        }

        private void OnNextButtonClick()
        {
            if (CurrentPageLinkPage < PageIndices.Count)
            {
                CurrentPageLinkPage += 1;
            }

            TogglePrevNextButtonsDisabled();
        }

        public void OnNotificationClose()
        {
            Message = String.Empty;
        }
    }
}
