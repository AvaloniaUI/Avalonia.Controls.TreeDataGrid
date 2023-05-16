using System;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;

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

        public override void Realize(
            TreeDataGridElementFactory factory,
            ITreeDataGridSelectionInteraction? selection, 
            ICell model,
            int columnIndex,
            int rowIndex)
        {
            DataContext = model;
            base.Realize(factory, selection, model, columnIndex, rowIndex);
        }

        public override void Unrealize()
        {
            DataContext = null;
            base.Unrealize();
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);

            if (ContentTemplate is null && DataContext is TemplateCell cell)
                ContentTemplate = cell.GetCellTemplate(this);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            var cell = DataContext as TemplateCell;

            // If DataContext is null, we're unrealized. Don't clear the content template for unrealized
            // cells because this will mean that when the cell is realized again the template will need
            // to be rebuilt, slowing everything down.
            if (cell is not null)
            {
                Content = cell.Value;

                if (((ILogical)this).IsAttachedToLogicalTree)
                    ContentTemplate = cell.GetCellTemplate(this);
            }
            else
            {
                Content = null;
            }
        }
    }
}
