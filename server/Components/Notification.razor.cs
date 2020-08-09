using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace TobyBlazor.Components
{
    public partial class Notification : ComponentBase
    {
        [Parameter]
        public string Message { get; set; }

        [Parameter]
        public string NotificationType { get; set; }

        [Parameter]
        public EventCallback OnClose { get; set; }

        private async Task CallClose()
        {
            await OnClose.InvokeAsync(null);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                await CallClose();
            }
        }

        private async void OnCloseButtonClick()
        {
            await CallClose();
        }
    }
}
