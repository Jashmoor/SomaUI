using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SomaUI;

public interface IPopupService {
    ValueTask AddCLickListeners<T>(string element, DotNetObjectReference<T> reference) where T : class;
    ValueTask<object> CalculatePosition<T>(ElementReference element, string id, DotNetObjectReference<T> callback, object? options = null) where T : class;
    ValueTask ClosePopup(string id, string target);
}