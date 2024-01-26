using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using ReactiveUI;
using TreeDataGridDemo.Models;

namespace TreeDataGridDemo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Avalonia.Controls.Templates;
    using CommunityToolkit.Mvvm.ComponentModel;

    internal class CountriesPageViewModel : ReactiveObject
    {
        private readonly ObservableCollection<HistoryItemViewModelBase> _data;
        private bool _cellSelection;

        public CountriesPageViewModel()
        {
            _data = new ObservableCollection<HistoryItemViewModelBase>(Enumerable.Range(0, 1000).Select(x=>new HistoryItemViewModelBase()));

            Source = new HierarchicalTreeDataGridSource<HistoryItemViewModelBase>(_data)
            {
                Columns =
                {
                    IndicatorsColumn(),
                    AmountColumn(),
                    LabelsColumn(),
                    ActionsColumn(),
                }
            };
            Source.RowSelection!.SingleSelect = false;
        }



	private static IColumn<HistoryItemViewModelBase> LabelsColumn()
	{
		return new TemplateColumn<HistoryItemViewModelBase>(
			"Labels",
			new FuncDataTemplate<HistoryItemViewModelBase>((node, ns) => new LabelsColumnView(), true),
			null,
			options: new TemplateColumnOptions<HistoryItemViewModelBase>
			{
				CanUserResizeColumn = false,
				CanUserSortColumn = true,
				MinWidth = new GridLength(100, GridUnitType.Pixel)
			},
			width: new GridLength(1, GridUnitType.Star));
	}

	private static IColumn<HistoryItemViewModelBase> AmountColumn()
	{
		return new TextColumn<HistoryItemViewModelBase, string>(
			"Amount (BTC)",
			x => $"1.24",
			options: new TextColumnOptions<HistoryItemViewModelBase>
			{
				CanUserResizeColumn = false,
				CanUserSortColumn = true,
			},
			width: new GridLength(0, GridUnitType.Auto));
	}

	private IColumn<HistoryItemViewModelBase> ActionsColumn()
	{
		return new TemplateColumn<HistoryItemViewModelBase>(
			"Actions",
			new FuncDataTemplate<HistoryItemViewModelBase>((node, ns) => new ActionsColumnView(), true),
			options: new TemplateColumnOptions<HistoryItemViewModelBase>
			{
				CanUserResizeColumn = false,
				CanUserSortColumn = false,
			},
			width: new GridLength(0, GridUnitType.Auto));
	}

        private static IColumn<HistoryItemViewModelBase> IndicatorsColumn()
        {
            return new HierarchicalExpanderColumn<HistoryItemViewModelBase>(
                new TemplateColumn<HistoryItemViewModelBase>(
                    null,
                    new FuncDataTemplate<HistoryItemViewModelBase>((node, ns) => new UserControl(), true),
                    null,
                    options: new TemplateColumnOptions<HistoryItemViewModelBase>
                    {
                        CanUserResizeColumn = false,
                        CanUserSortColumn = true,
                    },
                    width: new GridLength(0, GridUnitType.Auto)),
                x => Enumerable.Empty<HistoryItemViewModelBase>(),
                x => x.HasChildren(),
                x => x.IsExpanded);
        }
        
        public bool CellSelection
        {
            get => _cellSelection;
            set
            {
                if (_cellSelection != value)
                {
                    _cellSelection = value;
                    if (_cellSelection)
                        Source.Selection = new TreeDataGridCellSelectionModel<HistoryItemViewModelBase>(Source) { SingleSelect = false };
                    else
                        Source.Selection = new TreeDataGridRowSelectionModel<HistoryItemViewModelBase>(Source) { SingleSelect = false };
                    this.RaisePropertyChanged();
                }
            }
        }

        public HierarchicalTreeDataGridSource<HistoryItemViewModelBase> Source { get; }

        

        public void RemoveSelected()
        {
            var selection = ((ITreeSelectionModel)Source.Selection!).SelectedIndexes.ToList();

            for (var i = selection.Count - 1; i >= 0; --i)
            {
                _data.RemoveAt(selection[i][0]);
            }
        }
    }
    
    public interface ITreeDataGridExpanderItem
    {
        bool IsExpanded { get; set; }

        bool IsChild { get; set; }

        bool IsLastChild { get; set; }

        bool IsParentPointerOver { get; set; }

        bool IsParentSelected { get; set; }
    }
    
    public partial class HistoryItemViewModelBase : ObservableObject, ITreeDataGridExpanderItem
{
	[ObservableProperty] private bool _isFlashing;
	[ObservableProperty] private bool _isExpanded;
	[ObservableProperty] private bool _isPointerOver;
	[ObservableProperty] private bool _isParentPointerOver;
	[ObservableProperty] private bool _isSelected;
	[ObservableProperty] private bool _isParentSelected;
    
    [ObservableProperty]
    private bool _isButtonVisible;
        
    public ICommand ToggleCommand { get; }

	public HistoryItemViewModelBase()
    {
        IsChild = false;
        
        ToggleCommand = ReactiveCommand.Create(() =>
        {
            IsButtonVisible = !IsButtonVisible;
        });
        
		this.WhenAnyValue(x => x.IsSelected)
			.Do(x =>
			{
				foreach (var child in Children)
				{
					child.IsParentSelected = x;
				}
			})
			.Subscribe();
	}

	/// <summary>
	/// Proxy property to prevent stack overflow due to internal bug in Avalonia where the OneWayToSource Binding
	/// is replaced by a TwoWay one.when
	/// </summary>
	public bool IsPointerOverProxy
	{
		get => IsPointerOver;
		set => IsPointerOver = value;
	}

	public bool IsSelectedProxy
	{
		get => IsSelected;
		set => IsSelected = value;
	}

	public ObservableCollection<HistoryItemViewModelBase> Children { get; } = new();

	public bool IsChild { get; set; }

	public bool IsLastChild { get; set; }

	public ICommand? ShowDetailsCommand { get; protected set; }

	public ICommand? ClipboardCopyCommand { get; protected set; }

	public ICommand? SpeedUpTransactionCommand { get; protected set; }

	public ICommand? CancelTransactionCommand { get; protected set; }

	public bool HasChildren() => Children.Count > 0;

	public static Comparison<HistoryItemViewModelBase?> SortAscending<T>(Func<HistoryItemViewModelBase, T> selector, IComparer<T>? comparer = null)
	{
		return Sort(selector, comparer, reverse: false);
	}

	public static Comparison<HistoryItemViewModelBase?> SortDescending<T>(Func<HistoryItemViewModelBase, T> selector, IComparer<T>? comparer = null)
	{
		return Sort(selector, comparer, reverse: true);
	}

	private static Comparison<HistoryItemViewModelBase?> Sort<T>(Func<HistoryItemViewModelBase, T> selector, IComparer<T>? comparer, bool reverse)
	{
		return (x, y) =>
		{
			var ordering = reverse ? -1 : 1;

			if (x is null && y is null)
			{
				return 0;
			}

			if (x is null)
			{
				return -ordering;
			}

			if (y is null)
			{
				return ordering;
			}

			// Confirmation comparison must be the same for both sort directions..
            var result = 0;
			if (result == 0)
			{
				var xValue = selector(x);
				var yValue = selector(y);

				result =
					comparer?.Compare(xValue, yValue) ??
					Comparer<T>.Default.Compare(xValue, yValue);
				result *= ordering;
			}

			return result;
		};
	}
}
}
