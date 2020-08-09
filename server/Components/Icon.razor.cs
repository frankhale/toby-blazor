using Microsoft.AspNetCore.Components;

namespace TobyBlazor.Components
{
    public partial class Icon : ComponentBase
    {
        [Parameter]
        public string Path { get; set; }

        [Parameter]
        public int Width { get; set; } = 16;

        [Parameter]
        public int Height { get; set; } = 16;

        [Parameter]
        public bool Invert { get; set; }

        [Parameter]
        public string Title { get; set; }
    }
}
