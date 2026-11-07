using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace SomaUI;

public partial class Card : BaseWithChildComponent {
    [Parameter] public CardSize Size { get; set; } = CardSize.Default;

    private void Context(MouseEventArgs args) {

        var a = args;
    }
}


public enum CardSize {
    Small,
    Default
}