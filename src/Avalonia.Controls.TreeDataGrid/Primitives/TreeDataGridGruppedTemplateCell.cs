using System;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.LogicalTree;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridGruppedTemplateCell : TreeDataGridCell
    {
        private IColumns? _columns;

        public static readonly DirectProperty<TreeDataGridGruppedTemplateCell, IColumns?> ColumnsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridGruppedTemplateCell, IColumns?>(
                nameof(Columns),
                o => o.Columns);

        public IColumns? Columns
        {
            get => _columns;
            private set => SetAndRaise(ColumnsProperty, ref _columns, value);
        }

        public override void Realize(TreeDataGridElementFactory factory,
            ITreeDataGridSelectionInteraction? selection,
            ICell model,
            int columnIndex,
            int rowIndex)
        {
            this.InvalidateMeasure();
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
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property.Name == nameof(Model))
            {
                if (change.NewValue is IGruppedCell grupped)
                {
                    Columns = grupped.Columns;
                }
            }
        }


    }
}
