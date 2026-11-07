namespace SomaUI;

internal class AppState {
    public bool NavigationOpen { get; set; }
}

internal class ApplicationContext : StateContext<AppState> {
    public ApplicationContext() : base(new(), "app-state") {
    }

    internal void ToggleNavigation()
        => Modify(state => {
            state.NavigationOpen = !state.NavigationOpen;
        });
}
