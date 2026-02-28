using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gymble.ViewModels
{
    public interface IPager
    {
        int PageSize { get; set; }
        int PageIndex { get; set; }
        int TotalPages { get; }
        string PageStatus { get; }

        IRelayCommand FirstPageCommand { get; }
        IRelayCommand PrevPageCommand { get; }
        IRelayCommand NextPageCommand { get; }
        IRelayCommand LastPageCommand { get; }
    }

    public partial class PagingViewModel<T> : ObservableObject, IPager
    {
        public ObservableCollection<T> Items { get; } = new();

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (SetProperty(ref _pageSize, value))
                {
                    PageIndex = 0;
                    RebuildPage();
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
                    RebuildPage();
                }
            }
        }

        public int TotalCount => Items.Count;

        public int TotalPages
            => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

        private readonly ObservableCollection<T> _pagedItems = new();
        public ReadOnlyObservableCollection<T> PagedItems { get; }

        private int _pageGroupSize = 5;
        public int PageGroupSize
        {
            get => _pageGroupSize;
            set
            {
                if (SetProperty(ref _pageGroupSize, value))
                {
                    OnPropertyChanged(nameof(PageNumbers));
                }
            }
        }

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
            PagedItems = new ReadOnlyObservableCollection<T>(_pagedItems);

            Items.CollectionChanged += Items_CollectionChanged;

            FirstPageCommand = new RelayCommand(() => PageIndex = 0, () => PageIndex > 0);
            PrevPageCommand = new RelayCommand(() => PageIndex--, () => PageIndex > 0);
            NextPageCommand = new RelayCommand(() => PageIndex++, () => PageIndex < TotalPages - 1);
            LastPageCommand = new RelayCommand(() => PageIndex = TotalPages - 1, () => PageIndex < TotalPages - 1);

            GoToPageCommand = new RelayCommand<object?>(
                p =>
                {
                    if (TryGetInt(p, out var idx))
                        PageIndex = idx;
                },
                p => TryGetInt(p, out var idx) && idx >= 0 && idx < TotalPages
            );

            RebuildPage();
        }

        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Items가 줄어서 PageIndex가 범위를 벗어나는 경우 보정
            var fixedIndex = ClampPageIndex(PageIndex);
            if (fixedIndex != PageIndex)
            {
                _pageIndex = fixedIndex;
                OnPropertyChanged(nameof(PageIndex));
            }

            RebuildPage();
        }

        private int ClampPageIndex(int value)
        {
            var maxIndex = TotalPages - 1;
            if (maxIndex < 0) maxIndex = 0;
            return Math.Max(0, Math.Min(value, maxIndex));
        }

        private void RebuildPage()
        {
            var slice = Items.Skip(PageIndex * PageSize).Take(PageSize).ToList();

            _pagedItems.Clear();
            foreach (var item in slice) _pagedItems.Add(item);

            // 숫자/상태 텍스트 갱신
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(PageStatus));
            OnPropertyChanged(nameof(PageNumbers));

            // 버튼 CanExecute 즉시 반영
            CommandManager.InvalidateRequerySuggested();
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
}
