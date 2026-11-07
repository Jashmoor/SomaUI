namespace SomaUI;

internal class DataGridState<TItem> where TItem : class {
    public int Version { get; set; }
    public SortingState<TItem> SortingState { get; set; } = new();
    public PaginationState PaginationState { get; set; } = new();


    public override int GetHashCode() {
        HashCode hash = new();
        hash.Add(Version);
        hash.Add(SortingState.GetHashCode());
        return hash.ToHashCode();
    }
}

public class PaginationState {
    private int currentPage = 1;
    private int pageSize = 10;
    private int totalItems;

    public int CurrentPage {
        get => currentPage;
        set => currentPage = Math.Max(1, value);
    }

    public int PageSize {
        get => pageSize;
        set {
            pageSize = Math.Max(1, value);
            currentPage = 1;
        }
    }
    public int TotalItems {
        get => totalItems;
        set {
            totalItems = Math.Max(0, value);
            if (currentPage > TotalPages && TotalPages > 0)
                currentPage = TotalPages;
        }
    }

    public int TotalPages => TotalItems == 0
        ? 0
        : (int)Math.Ceiling(TotalItems / (double)PageSize);
    public int StartIndex => (CurrentPage - 1) * PageSize;
    public int EndIndex => Math.Min(StartIndex + PageSize, TotalItems);
    public bool CanGoNext => CurrentPage < TotalPages;
    public bool CanGoPrevious => CurrentPage > 1;
    public int FirstItemIndex => TotalItems == 0
        ? 0
        : StartIndex + 1;
    public int LastItemIndex => EndIndex;
    public void NextPage() {
        if (CanGoNext)
            CurrentPage++;
    }

    public void PreviousPage() {
        if (CanGoPrevious)
            CurrentPage--;
    }

    public void FirstPage()
        => CurrentPage = 1;


    public void LastPage() {
        if (TotalPages > 0)
            CurrentPage = TotalPages;
    }

    public void GoToPage(int page)
       => CurrentPage = Math.Max(1, Math.Min(page, TotalPages > 0 ? TotalPages : 1));

    public void Reset()
        => CurrentPage = 1;

    public override int GetHashCode()
        => HashCode.Combine(CurrentPage, PageSize, TotalItems);
}

public class SortingState<TItem> where TItem : class {
    internal SortDirection SortDirection { get; set; } = SortDirection.None;
    internal string? SortColumn { get; set; }
    internal PropertyColumn<TItem>? ActiveColumn { get; set; }
    internal CategoryColumn<TItem>? ActiveCategory { get; set; }

    public void ToggleSortDirection(PropertyColumn<TItem> column) {
        ActiveColumn = column;
        if (SortColumn == column.Title)
        {
            (SortColumn, SortDirection) = SortDirection switch {
                SortDirection.None => (SortColumn, SortDirection.Ascending),
                SortDirection.Ascending => (SortColumn, SortDirection.Descending),
                SortDirection.Descending => (SortColumn, SortDirection.None),
                _ => (null, SortDirection.None)
            };

            return;
        }
        (SortColumn, SortDirection) = (column.Title, SortDirection.Ascending);
    }
}

internal enum SortDirection {
    Ascending,
    Descending,
    None
}


internal class DataGridContext<TItem> : StateContext<DataGridState<TItem>> where TItem : class {
    public DataGridContext() : base(new(), "data-grid") {
    }

    internal List<PropertyColumn<TItem>> Columns { get; set; } = [];
    internal List<CategoryColumn<TItem>> Categorys { get; set; } = [];
    internal IEnumerable<TItem> Data { get; set; } = [];
    internal IEnumerable<IGrouping<object, TItem>> Grouped { get; set; } = [];
    public bool Loading { get; set; }
    internal bool Ingestion { get; private set; }

    internal void BeginColumnIngestion() {
        Columns.Clear();
        Loading = true;
        Ingestion = true;
    }

    internal void IngestionComplete() {
        Ingestion = false;
        Loading = false;
    }

    internal void AddCategory(CategoryColumn<TItem> category) {
        if (!Categorys.Contains(category))
            Categorys.Add(category);
    }

    internal void RegisterColumn(PropertyColumn<TItem> column) {
        if (Ingestion && !Columns.Contains(column))
            Columns.Add(column);
    }

    internal void SetCategory(CategoryColumn<TItem>? column)
        => Modify(state => {
            state.SortingState.ActiveCategory = column;
        });

    internal void SetSort(string column_title) {
        Modify(state => {
            state.SortingState.SortColumn = column_title;
        });
    }

    internal void ToggleSort(PropertyColumn<TItem> column) {
        Modify(state => {
            state.SortingState.ToggleSortDirection(column);
        });
    }
}

