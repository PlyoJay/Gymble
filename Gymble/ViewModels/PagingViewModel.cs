using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gymble.Models;
using System.Collections.ObjectModel;

public interface IPager
{
    int PageSize { get; set; }
    int PageIndex { get; set; }      // 0-based
    int TotalPages { get; }
    int TotalCount { get; }
    string PageStatus { get; }

    IRelayCommand FirstPageCommand { get; }
    IRelayCommand PrevPageCommand { get; }
    IRelayCommand NextPageCommand { get; }
    IRelayCommand LastPageCommand { get; }

    IRelayCommand<object?> GoToPageCommand { get; }
}

public partial class PagingViewModel<T> : ObservableObject, IPager
{
    public ObservableCollection<T> Items { get; } = new();

    private int _pageSize = 15;
    public int PageSize
    {
        get => _pageSize;
        set
        {
            var v = Math.Max(1, value);
            if (SetProperty(ref _pageSize, v))
            {
                PageIndex = 0;
                RequestPage?.Invoke();
                UpdateUi();
            }
        }
    }

    private int _pageIndex; // 0-based
    public int PageIndex
    {
        get => _pageIndex;
        set
        {
            var clamped = ClampPageIndex(value);
            if (SetProperty(ref _pageIndex, clamped))
            {
                RequestPage?.Invoke();
                UpdateUi();
            }
        }
    }

    private int _totalCount;
    public int TotalCount
    {
        get => _totalCount;
        private set
        {
            if (SetProperty(ref _totalCount, value))
            {
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(PageStatus));
                OnPropertyChanged(nameof(PageNumbers));
                UpdateCommands();
            }
        }
    }

    public int TotalPages
        => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

    private int _pageGroupSize = 5;
    public int PageGroupSize
    {
        get => _pageGroupSize;
        set
        {
            var v = Math.Max(1, value);
            if (SetProperty(ref _pageGroupSize, v))
                OnPropertyChanged(nameof(PageNumbers));
        }
    }

    // ✅ 외부(상위 VM)에서 "페이지를 다시 로드"하도록 연결
    public Action? RequestPage { get; set; }

    // 🔹 현재 그룹에 표시할 페이지 인덱스들 (0-based)
    public IEnumerable<int> PageNumbers
    {
        get
        {
            if (TotalPages <= 0) yield break;

            var groupIndex = PageIndex / PageGroupSize;
            var start = groupIndex * PageGroupSize;
            var end = Math.Min(start + PageGroupSize - 1, TotalPages - 1);

            for (int i = start; i <= end; i++)
                yield return i;
        }
    }

    // 보기용 표시: "101–150 / 4,327 (3/87)"
    public string PageStatus
    {
        get
        {
            if (TotalCount == 0) return "0 / 0 (0/0)";
            var start = PageIndex * PageSize + 1;
            var end = Math.Min((PageIndex + 1) * PageSize, TotalCount);
            return $"{start}–{end} / {TotalCount:N0} ({PageIndex + 1}/{TotalPages})";
        }
    }

    public IRelayCommand FirstPageCommand { get; }
    public IRelayCommand PrevPageCommand { get; }
    public IRelayCommand NextPageCommand { get; }
    public IRelayCommand LastPageCommand { get; }
    public IRelayCommand<object?> GoToPageCommand { get; }

    public PagingViewModel()
    {
        FirstPageCommand = new RelayCommand(() => PageIndex = 0, () => PageIndex > 0);
        PrevPageCommand = new RelayCommand(() => PageIndex--, () => PageIndex > 0);
        NextPageCommand = new RelayCommand(() => PageIndex++, () => PageIndex < TotalPages - 1);
        LastPageCommand = new RelayCommand(() => PageIndex = TotalPages - 1, () => PageIndex < TotalPages - 1);

        GoToPageCommand = new RelayCommand<object?>(
            p =>
            {
                if (TryGetInt(p, out var idx))
                    PageIndex = idx; // 0-based idx
            },
            p => TryGetInt(p, out var idx) && idx >= 0 && idx < TotalPages
        );

        UpdateUi();
    }

    /// <summary>
    /// ✅ 서비스/레포에서 받은 PagedResult를 한 번에 반영
    /// </summary>
    public void ApplyPage(IReadOnlyList<T> rows, int totalCount, int page /*1-based*/)
    {
        Items.Clear();
        foreach (var r in rows) Items.Add(r);

        TotalCount = Math.Max(0, totalCount);

        // page(1-based) -> PageIndex(0-based)
        _pageIndex = ClampPageIndex(Math.Max(0, page - 1));
        OnPropertyChanged(nameof(PageIndex));

        UpdateUi();
    }

    private void UpdateUi()
    {
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(PageStatus));
        OnPropertyChanged(nameof(PageNumbers));
        UpdateCommands();
    }

    private void UpdateCommands()
    {
        (FirstPageCommand as RelayCommand)?.NotifyCanExecuteChanged();
        (PrevPageCommand as RelayCommand)?.NotifyCanExecuteChanged();
        (NextPageCommand as RelayCommand)?.NotifyCanExecuteChanged();
        (LastPageCommand as RelayCommand)?.NotifyCanExecuteChanged();
        (GoToPageCommand as RelayCommand<object?>)?.NotifyCanExecuteChanged();
    }

    private int ClampPageIndex(int value)
    {
        var maxIndex = TotalPages - 1;
        if (maxIndex < 0) maxIndex = 0;
        return Math.Max(0, Math.Min(value, maxIndex));
    }

    private static bool TryGetInt(object? p, out int idx)
    {
        switch (p)
        {
            case int i:
                idx = i; return true;
            case string s when int.TryParse(s, out var v):
                idx = v; return true;
            default:
                idx = 0; return false;
        }
    }
}