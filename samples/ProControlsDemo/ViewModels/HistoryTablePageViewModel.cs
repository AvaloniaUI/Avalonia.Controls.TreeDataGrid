using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using ProControlsDemo.Views.History.Columns;

namespace ProControlsDemo.ViewModels
{
    internal class HistoryItemViewModelBase
    {
        public bool IsFlashing { get; set; }

        public int OrderIndex { get; set; }

        public DateTimeOffset Date { get; set; }

        public string DateString { get; set; } = "";

        public bool IsConfirmed { get; set; }
        
	    public int Id { get; }

	    public List<string>? FilteredLabel { get; set; }

	    public List<string>? Label { get; set; }

	    public bool IsCoinJoin { get; set; }

	    public string? Balance { get; set; }

	    public string? OutgoingAmount { get; set; }

	    public string? IncomingAmount { get; set; }

	    public static Comparison<HistoryItemViewModelBase?> SortAscending<T>(Func<HistoryItemViewModelBase, T> selector)
	    {
		    return (x, y) =>
		    {
			    if (x is null && y is null)
			    {
				    return 0;
			    }
			    else if (x is null)
			    {
				    return -1;
			    }
			    else if (y is null)
			    {
				    return 1;
			    }
			    else
			    {
				    return Comparer<T>.Default.Compare(selector(x), selector(y));
			    }
		    };
	    }

	    public static Comparison<HistoryItemViewModelBase?> SortDescending<T>(Func<HistoryItemViewModelBase, T> selector)
	    {
		    return (x, y) =>
		    {
			    if (x is null && y is null)
			    {
				    return 0;
			    }
			    else if (x is null)
			    {
				    return 1;
			    }
			    else if (y is null)
			    {
				    return -1;
			    }
			    else
			    {
				    return Comparer<T>.Default.Compare(selector(y), selector(x));
			    }
		    };
	    }
    }

    internal class HistoryTablePageViewModel
    {
        private readonly ObservableCollection<HistoryItemViewModelBase> _data;

        public HistoryTablePageViewModel()
        {
            var transactions = new List<HistoryItemViewModelBase>();

            var rand = new Random(100);
            
            for (var i = 0; i < 10_000; i++)
            {
                var item = new HistoryItemViewModelBase()
                {
                    IsConfirmed = rand.NextDouble() > 0.5,
                    DateString = $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm}",
                    Label = new List<string>(new [] { "Label1", "Label2" }),
                    IncomingAmount = $"{rand.NextDouble()}",
                    OutgoingAmount = $"{rand.NextDouble()}",
                    Balance = $"{rand.NextDouble()}",
                };
                transactions.Add(item);
            }

            _data = new ObservableCollection<HistoryItemViewModelBase>(transactions);

			// [Column]			[View]						[Header]		[Width]		[MinWidth]		[MaxWidth]	[CanUserSort]
			// Indicators		IndicatorsColumnView		-				Auto		80				-			false
			// Date				DateColumnView				Date / Time		Auto		150				-			true
			// Labels			LabelsColumnView			Labels			*			75				-			false
			// Incoming			IncomingColumnView			Incoming (₿)	Auto		120				150			true
			// Outgoing			OutgoingColumnView			Outgoing (₿)	Auto		120				150			true
			// Balance			BalanceColumnView			Balance (₿)		Auto		120				150			true

			IControl IndicatorsColumnTemplate(HistoryItemViewModelBase node, INameScope ns) => new IndicatorsColumnView() { Height = 37.5, MinWidth = 80 };
			IControl DateColumnTemplate(HistoryItemViewModelBase node, INameScope ns) => new DateColumnView() { Height = 37.5, MinWidth = 150 };
			IControl LabelsColumnTemplate(HistoryItemViewModelBase node, INameScope ns) => new LabelsColumnView() { Height = 37.5, MinWidth = 75 };
			IControl IncomingColumnTemplate(HistoryItemViewModelBase node, INameScope ns) => new IncomingColumnView() { Height = 37.5, MinWidth = 120, MaxWidth = 150 };
			IControl OutgoingColumnTemplate(HistoryItemViewModelBase node, INameScope ns) => new OutgoingColumnView() { Height = 37.5, MinWidth = 120, MaxWidth = 150 };
			IControl BalanceColumnTemplate(HistoryItemViewModelBase node, INameScope ns) => new BalanceColumnView() { Height = 37.5, MinWidth = 120, MaxWidth = 150 };

			Source = new FlatTreeDataGridSource<HistoryItemViewModelBase>(_data)
            {
                Columns =
                {
	                // Indicators
                    new TemplateColumn<HistoryItemViewModelBase>(
                        null,
                        new FuncDataTemplate<HistoryItemViewModelBase>(IndicatorsColumnTemplate, true),
                        options: new ColumnOptions<HistoryItemViewModelBase>
                        {
                            CanUserResizeColumn = false,
                            CanUserSortColumn = false
                        },
                        width: new GridLength(0, GridUnitType.Auto)),
                    // Date
                    new TemplateColumn<HistoryItemViewModelBase>(
	                    "Date / Time",
	                    new FuncDataTemplate<HistoryItemViewModelBase>(DateColumnTemplate, true),
	                    options: new ColumnOptions<HistoryItemViewModelBase>
	                    {
		                    CanUserResizeColumn = false,
		                    CanUserSortColumn = true,
		                    CompareAscending = HistoryItemViewModelBase.SortAscending(x => x.Date),
		                    CompareDescending = HistoryItemViewModelBase.SortDescending(x => x.Date),
	                    },
	                    width: new GridLength(0, GridUnitType.Auto)),
                    // Labels
                    new TemplateColumn<HistoryItemViewModelBase>(
	                    "Labels",
	                    new FuncDataTemplate<HistoryItemViewModelBase>(LabelsColumnTemplate, true),
	                    options: new ColumnOptions<HistoryItemViewModelBase>
	                    {
		                    CanUserResizeColumn = false,
		                    CanUserSortColumn = false
	                    },
                        // TODO: 
                        // STAR SIZING DOES NOT WORK HERE!
	                    //width: new GridLength(1, GridUnitType.Star)),
                        width: new GridLength(0, GridUnitType.Auto)),
                    // Incoming
                    new TemplateColumn<HistoryItemViewModelBase>(
	                    "Incoming (₿)",
	                    new FuncDataTemplate<HistoryItemViewModelBase>(IncomingColumnTemplate, true),
	                    options: new ColumnOptions<HistoryItemViewModelBase>
	                    {
		                    CanUserResizeColumn = false,
		                    CanUserSortColumn = true,
		                    CompareAscending = HistoryItemViewModelBase.SortAscending(x => x.IncomingAmount),
		                    CompareDescending = HistoryItemViewModelBase.SortDescending(x => x.IncomingAmount),
	                    },
	                    width: new GridLength(0, GridUnitType.Auto)),
                    // Outgoing
                    new TemplateColumn<HistoryItemViewModelBase>(
	                    "Outgoing (₿)",
	                    new FuncDataTemplate<HistoryItemViewModelBase>(OutgoingColumnTemplate, true),
	                    options: new ColumnOptions<HistoryItemViewModelBase>
	                    {
		                    CanUserResizeColumn = false,
		                    CanUserSortColumn = true,
		                    CompareAscending = HistoryItemViewModelBase.SortAscending(x => x.OutgoingAmount),
		                    CompareDescending = HistoryItemViewModelBase.SortDescending(x => x.OutgoingAmount),
	                    },
	                    width: new GridLength(0, GridUnitType.Auto)),
                    // Balance
                    new TemplateColumn<HistoryItemViewModelBase>(
	                    "Balance (₿)",
	                    new FuncDataTemplate<HistoryItemViewModelBase>(BalanceColumnTemplate, true),
	                    options: new ColumnOptions<HistoryItemViewModelBase>
	                    {
		                    CanUserResizeColumn = false,
		                    CanUserSortColumn = true,
		                    CompareAscending = HistoryItemViewModelBase.SortAscending(x => x.Balance),
		                    CompareDescending = HistoryItemViewModelBase.SortDescending(x => x.Balance),
	                    },
	                    width: new GridLength(0, GridUnitType.Auto)),
                }
            };
        }

        public FlatTreeDataGridSource<HistoryItemViewModelBase> Source { get; }
        
    }
}
