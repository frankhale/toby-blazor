﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TobyBlazor.Components
{
  public partial class YouTube : ComponentBase
  {
    [Parameter] public string VideoId { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; }

    private Timer _timer;
    private bool _apiReady;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
      if (firstRender)
      {
        _timer = new Timer(async (e) =>
        {
          _apiReady = await JsRuntime.InvokeAsync<bool>("checkAPIReady");
          if (!_apiReady) return;

          await JsRuntime.InvokeAsync<string>("playVideo", VideoId);
          await _timer.DisposeAsync();
          _timer = null;
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
      }
    }
  }
}
