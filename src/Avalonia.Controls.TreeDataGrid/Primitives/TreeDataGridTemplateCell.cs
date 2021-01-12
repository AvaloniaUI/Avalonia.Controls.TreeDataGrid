using System;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridTemplateCell : TreeDataGridCell
    {
        public static readonly DirectProperty<TreeDataGridTemplateCell, object?> ContentProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridTemplateCell, object?>(
                nameof(Content),
                x => x.Content);

        public static readonly DirectProperty<TreeDataGridTemplateCell, IDataTemplate?> ContentTemplateProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridTemplateCell, IDataTemplate?>(
                nameof(ContentTemplate),
                x => x.ContentTemplate);

        private object? _content;
        private IDataTemplate? _contentTemplate;

        public object? Content 
        { 
            get => _content;
            private set => SetAndRaise(ContentProperty, ref _content, value);
        }
        public IDataTemplate? ContentTemplate 
        { 
            get => _contentTemplate;
            set => SetAndRaise(ContentTemplateProperty, ref _contentTemplate, value);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            var cell = DataContext as TemplateCell;
            Content = cell?.Value;
            ContentTemplate = cell?.CellTemplate;
        }
    }
}
