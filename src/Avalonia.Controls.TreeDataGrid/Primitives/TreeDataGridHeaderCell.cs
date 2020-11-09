using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Utilities;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridHeaderCell : TemplatedControl
    {
        public static readonly DirectProperty<TreeDataGridHeaderCell, object?> HeaderProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridHeaderCell, object?>(
                nameof(Header),
                o => o.Header);

        private Thumb? _resizer;

        public object? Header => Model?.Header;

        private IColumn? Model => (IColumn?)DataContext;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _resizer = e.NameScope.Find<Thumb>("PART_Resizer");

            if (_resizer is object)
            {
                _resizer.DragDelta += ResizerDragDelta;
            }
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            RaisePropertyChanged(HeaderProperty, default, default);
        }

        private void ResizerDragDelta(object? sender, VectorEventArgs e)
        {
            if (Model is null || MathUtilities.IsZero(e.Vector.X))
                return;

            var width = Model.Width.IsAbsolute ? Model.Width.Value : Bounds.Width;

            if (double.IsNaN(width) || double.IsInfinity(width) || width + e.Vector.X < 0)
                return;

            Model.Width = new GridLength(width + e.Vector.X, GridUnitType.Pixel);

            this.FindAncestorOfType<TreeDataGridPanel>()?.InvalidateAll();
        }
    }
}
