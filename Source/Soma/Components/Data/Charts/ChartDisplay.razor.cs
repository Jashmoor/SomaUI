using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace SomaUI;

public partial class ChartDisplay : BaseComponent, IDisposable {
    private const string __Javascript = "./_content/SomaUI/Components/Data/Charts/ChartDisplay.razor.js";
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Parameter] public RenderFragment ChildContent { get; set; } = default!;
    [Parameter] public ChartConfig Config { get; set; } = new();

    internal ChartContext Context { get; set; } = new();
    private IJSObjectReference? JSObject { get; set; } = default!;
    internal double? Width { get; set; } = 0;
    internal double? Height { get; set; } = 0;

    private string ViewBox => $"0 0 {F(Width.Value)} {F(Height.Value)}";

    private bool __dimensions_set = false;
    internal string? ClipPath { get; set; }

    protected override void OnInitialized() {
        base.OnInitialized();
        ClipPath = $"clipPath{UniqueID}";
    }

    private readonly record struct ClientRectange {
        public double Width { get; init; }
        public double Height { get; init; }
    }

    public bool Scaled { get; set; } = false;

    private static string F(double value) => value.ToString(CultureInfo.InvariantCulture);

    protected override async Task OnAfterRenderAsync(bool first) {
        if (first)
        {
            JSObject ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", __Javascript);
            var rectangle = await JSObject.InvokeAsync<ClientRectange>("CreateChart", Element, Reference);

            if (!__dimensions_set)
            {
                __dimensions_set = true;
                await Resize(rectangle.Width, rectangle.Height);
            }
        }
        await base.OnAfterRenderAsync(first);
    }

    protected override void OnParametersSet() {
        Context.Config = Config;
        Context.OnStateChanged = () => InvokeAsync(StateHasChanged);
    }

    internal async Task Refresh(bool force = true) {
        if (__dimensions_set)
        {
            //var stateHasChanged = UpdateScales();
            var stateHasChanged = true;
            if (stateHasChanged || force)
            {
                Context.Width = Width ?? 0;
                Context.Height = Height ?? 0;
                Scaled = true;
                StateHasChanged();
            }
        }
    }

    [JSInvokable]
    public async Task Resize(double width, double height) {
        var stateHasChanged = false;

        if (width != Width)
        {
            Width = width;
            stateHasChanged = true;
        }

        if (height != Height)
        {
            Height = height;
            stateHasChanged = true;
        }

        if (stateHasChanged)
            await Refresh();
    }

    double mouseX;
    double mouseY;

    double? clickX;
    double? clickY;
    [JSInvokable]
    public async Task MouseMove(double x, double y) {
        mouseX = x;
        mouseY = y;
        await InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public async Task Click(double x, double y) {
        clickX = x;
        clickY = y;
        await InvokeAsync(StateHasChanged);
    }

    public override void Dispose() {
        base.Dispose();

        if (JsRuntime != null)
            InvokeAsync(() => JSObject?.InvokeVoidAsync("DestroyChart", Element));
        try
        {
            JSObject?.DisposeAsync();
        }
        catch { }
        GC.SuppressFinalize(this);
    }
}
