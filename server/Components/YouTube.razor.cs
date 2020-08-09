using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TobyBlazor.Components
{
    public partial class YouTube : ComponentBase
    {
        [Inject]
        IJSRuntime jsRuntime { get; set; }

        private Timer _timer;
        private bool playerReady = false;
        private bool apiReady = false;
        private bool playerCreated = false;

        [Parameter]
        public string VideoId { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await jsRuntime.InvokeVoidAsync("initializeYouTubePlayer");

                // Periodically check to see if the api is ready and the player is created
                // When everything is ready dispose of the timer
                _timer = new Timer(async (e) =>
                {
                    playerReady = await jsRuntime.InvokeAsync<bool>("checkPlayer");
                    apiReady = await jsRuntime.InvokeAsync<bool>("checkAPIReady");

                    if (apiReady && !playerCreated)
                    {
                        await jsRuntime.InvokeVoidAsync("createPlayer");
                        playerCreated = true;
                    }

                    if (playerReady)
                    {
                        await jsRuntime.InvokeAsync<string>("playVideo", VideoId);
                        _timer.Dispose();
                    }
                }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
            }
        }
    }
}
