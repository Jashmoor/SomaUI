using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SomaUI;

internal class PopupService(IJSRuntime javascript) : IPopupService {
    private const string __Javascript = "./_content/SomaUI/js/popup-service.js";
    private IJSObjectReference? _Module;

    private async ValueTask<IJSObjectReference> GetModule() {
        _Module ??= await javascript.InvokeAsync<IJSObjectReference>("import", __Javascript);
        return _Module;
    }

    public async ValueTask<object> CalculatePosition<T>(ElementReference element, string id, DotNetObjectReference<T> dotnet, object? options = null) where T : class {
        try
        {
            var module = await GetModule();
            return await module.InvokeAsync<object>("CalculatePosition", element, id, dotnet, options ?? new());

        }
        catch (Exception e)
        {
            return new {};
        }
    }

    public async ValueTask ClosePopup(string id, string target) {
        var module = await GetModule();
        await module.InvokeVoidAsync("ClosePopup", id, target);
    }

    public async ValueTask AddCLickListeners<T>(string element, DotNetObjectReference<T> reference) where T : class {
        var module = await GetModule();
        await module.InvokeVoidAsync("AddClickOutsideHandler", element, reference);
    }
}
