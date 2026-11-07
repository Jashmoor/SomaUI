using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Linq.Expressions;
using System.Web;

namespace SomaUI;

public partial class DataTable<TItem> where TItem : class {
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Parameter] public IQueryable<TItem>? Items { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment<(IGrouping<object, TItem>, int)>? GroupHeaderTemplate { get; set; }
    [Parameter] public Expression<Func<TItem, object>> GroupData { get; set; } = default!;
    [Parameter] public bool Selectable { get; set; } = true;
    [Parameter] public bool MultiSelect { get; set; } = true;
    [Parameter] public HashSet<TItem>? SelectedItems { get; set; }
    [Parameter] public EventCallback<HashSet<TItem>> SelectedItemsChanged { get; set; }

    private DataGridContext<TItem> Context = default!;
    private DataGridState<TItem> InternalState = new();
    private string? FilterValue { get; set; }
    private HashSet<TItem> _selectedItems { get; set; } = new();

    protected override void OnInitialized() {
        Context = new() {
            State = InternalState
        };
        Context.OnStateChange += HandleStateChange;
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs args) {
        var query = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query;

        var collection = HttpUtility.ParseQueryString(query);

        PropertyColumn<TItem> active_column;
        if (Context.State.SortingState.ActiveColumn is PropertyColumn<TItem> column)
        {
            active_column = column;
        }
        else if (collection.GetValues("sort")?.Any() ?? false)
        {
            active_column = Context.Columns.FirstOrDefault(x => x.Title == collection.GetValues("sort")!.First())!;
        }
    }

    internal async Task ToggleSelection(TItem item) {
        if (!Selectable)
            return;

        if (MultiSelect)
        {
            if (!_selectedItems.Add(item))
                _selectedItems.Remove(item);
        }
        else
        {
            _selectedItems.Clear();
            _selectedItems.Add(item);
        }
        SelectedItems = _selectedItems;
        if (SelectedItemsChanged.HasDelegate)
            await SelectedItemsChanged.InvokeAsync(_selectedItems);

    }

    internal void ToggleAll(bool value) {
        if(_selectedItems.Count == Context.State.PaginationState.PageSize)
        {
            _selectedItems.Clear();
            return;
        }

        foreach(var item in Context.Data)
        {
            _selectedItems.Add(item);
        }
    }

    private async Task ToggleSort(TableHead head) {
        var column = Context.Columns.FirstOrDefault(x => x.Title == head.Title);
        if (column is null | column?.Sortable == false)
            return;
        SortByColumn(column!);
        await ProcessData();

    }

    private async Task ToggleSort(string col) {
        var column = Context.Columns.FirstOrDefault(x => x.Title == col);
        if (column is null | column?.Sortable == false)
            return;
        SortByColumn(column!);
        await ProcessData();
    }

    private void SortByColumn(PropertyColumn<TItem> column) {
        Context.ToggleSort(column);

        var uri = NavigationManager.GetUriWithQueryParameters(new Dictionary<string, object?> {
            ["sort"] = column.Title,
            ["direction"] = Context.State.SortingState.SortDirection switch {
                SortDirection.Ascending => "asc",
                SortDirection.Descending => "des",
                _ => null
            }
        });

        NavigationManager.NavigateTo(uri);
    }

    private async Task ChangePage(int page) {
        Context.State.PaginationState.GoToPage(page);
        await ProcessData();
    }

    private async Task ChangePageSize() {
        Context.State.PaginationState.PageSize = PageSize;
        await ProcessData();
    }

    private async Task ProcessData() {
        try
        {
            var data = Items?.AsEnumerable();
            if (data is null)
            {
                Context.Data = [];
                return;
            }

            data = Categorise(data);
            data = ApplyFiltering(FilterValue, data);
            data = ApplySorting(Context.State.SortingState, data);

            if (GroupData is not null)
            {
                var group = data.AsQueryable().GroupBy(GroupData).OrderBy(x => x.Key).AsEnumerable();
                Context.Grouped = group;
                return;
            }

            data= ApplyPagination(data);
            Context.Data = data.ToList();
        }
        finally
        {
            Loading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task UpdateCategory(string title) {
        if (Context.Categorys.FirstOrDefault(x => x.Title == title) is CategoryColumn<TItem> col)
        {
            Context.SetCategory(col);
        }
        else
        {
            Context.SetCategory(null);
        }
        await ProcessData();
    }

    private IEnumerable<TItem> Categorise(IEnumerable<TItem> data) {
        if (Context.State.SortingState.ActiveCategory is not null)
            return data.Where(Context.State.SortingState.ActiveCategory.ValueExpression);
        return data;
    }

    private IEnumerable<TItem> ApplySorting(SortingState<TItem> state, IEnumerable<TItem> data) {
        if (state.SortDirection == SortDirection.None)
            return data;
        if (state.ActiveColumn is null)
            return data;
        return (state.SortDirection switch {
            SortDirection.Ascending => data.OrderBy(x => state.ActiveColumn!.ValueExpression(x)),
            SortDirection.Descending => data.OrderByDescending(x => state.ActiveColumn!.ValueExpression(x)),
            _ => data
        });
    }

    private IEnumerable<TItem> ApplyFiltering(string? search, IEnumerable<TItem> data) {
        if (FilterValue is null | FilterValue is { Length: <3 })
            return data;
        string search_value = FilterValue!;

        var columns = Context.Columns.Where(x => x.Searchable).ToList();
        if (columns.Count == 0)
            return data;

        var t = data.Where(item => Matches(item, search_value, columns));
        return t;
    }

    private IEnumerable<TItem> ApplyPagination(IEnumerable<TItem> data) {
        Context.State.PaginationState.TotalItems = data.Count();
        return data
            .Skip(Context.State.PaginationState.StartIndex)
            .Take(Context.State.PaginationState.PageSize);
    }

    private bool Matches(TItem data, string searchValue, List<PropertyColumn<TItem>> columns) {
        foreach (var column in columns)
        {
            try
            {
                if (column.ValueExpression is null)
                    continue;

                object res = column.ValueExpression(data);
                if (res == null)
                    continue;
                var value = res.ToString();
                if (!string.IsNullOrEmpty(value) && value.Contains(searchValue, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            catch { }
        }
        return false;
    }

    protected override void OnParametersSet() {
        RefreshData(InternalState);
        InvokeAsync(ProcessData);
    }

    internal void RefreshData(DataGridState<TItem> state) {
        var data = Items?.ToList() ?? [];

        if (data is List<TItem> items)
        {
            Context.State = state;
            Context.Data = items;
        }
    }

    private void HandleStateChange()
        => InvokeAsync(StateHasChanged);

    public override void Dispose() {
        base.Dispose();
        Context.OnStateChange -= HandleStateChange;
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}
