using System;
using System.Collections.Generic;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Input;

namespace Avalonia.Controls.Selection
{
    public class TreeDataGridRowSelectionModel<T> : TreeSelectionModelBase<T>, 
        ITreeDataGridRowSelectionModel<T>,
        ITreeDataGridSelectionInteraction
    {
        private readonly ITreeDataGridSource<T> _source;
        private EventHandler? _viewSelectionChanged;
        private Point _pressedPoint;

        public TreeDataGridRowSelectionModel(ITreeDataGridSource<T> source)
            : base(source.Items)
        {
            _source = source;
            SelectionChanged += (s,e) => _viewSelectionChanged?.Invoke(this, e);
        }

        event EventHandler? ITreeDataGridSelectionInteraction.SelectionChanged
        {
            add => _viewSelectionChanged += value;
            remove => _viewSelectionChanged -= value;
        }

        bool ITreeDataGridSelectionInteraction.IsRowSelected(IRow rowModel)
        {
            if (rowModel is IModelIndexableRow indexable)
                return IsSelected(indexable.ModelIndexPath);
            return false;
        }

        bool ITreeDataGridSelectionInteraction.IsRowSelected(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < _source.Rows.Count)
            {
                if (_source.Rows[rowIndex] is IModelIndexableRow indexable)
                    return IsSelected(indexable.ModelIndexPath);
            }

            return false;
        }

        void ITreeDataGridSelectionInteraction.OnPointerPressed(TreeDataGrid sender, PointerPressedEventArgs e)
        {
            if (!e.Handled && e.Pointer.Type == PointerType.Mouse)
                PointerSelect(sender, e);
            else
                _pressedPoint = e.GetPosition(sender);
        }

        void ITreeDataGridSelectionInteraction.OnPointerReleased(TreeDataGrid sender, PointerReleasedEventArgs e)
        {
            if (!e.Handled && e.Pointer.Type == PointerType.Touch)
            {
                var p = e.GetPosition(sender);
                if (Math.Abs(p.X - _pressedPoint.X) <= 3 || Math.Abs(p.Y - _pressedPoint.Y) <= 3)
                    PointerSelect(sender, e);
            }
        }

        protected internal override IEnumerable<T>? GetChildren(T node)
        {
            if (_source is HierarchicalTreeDataGridSource<T> treeSource)
            {
                return treeSource.GetModelChildren(node);
            }

            return null;
        }

        private void PointerSelect(TreeDataGrid sender, PointerEventArgs e)
        {
            if (e.Source is IControl source && sender.TryGetRow(source, out var row))
            {
                var point = e.GetCurrentPoint(sender);

                UpdateSelection(
                    sender,
                    row.RowIndex,
                    select: true,
                    rangeModifier: e.KeyModifiers.HasFlag(KeyModifiers.Shift),
                    toggleModifier: e.KeyModifiers.HasFlag(KeyModifiers.Control),
                    rightButton: point.Properties.IsRightButtonPressed);
                e.Handled = true;
            }
        }

        private void UpdateSelection(
            TreeDataGrid treeDataGrid,
            int rowIndex,
            bool select = true,
            bool rangeModifier = false,
            bool toggleModifier = false,
            bool rightButton = false)
        {
            var modelIndex = _source.Rows.RowIndexToModelIndex(rowIndex);

            if (modelIndex == default)
                return;

            var mode = SingleSelect ? SelectionMode.Single : SelectionMode.Multiple;
            var multi = (mode & SelectionMode.Multiple) != 0;
            var toggle = (toggleModifier || (mode & SelectionMode.Toggle) != 0);
            var range = multi && rangeModifier;

            if (!select)
            {
                if (IsSelected(modelIndex) && !treeDataGrid.QueryCancelSelection())
                    Deselect(modelIndex);
            }
            else if (rightButton)
            {
                if (IsSelected(modelIndex) == false && !treeDataGrid.QueryCancelSelection())
                    SelectedIndex = modelIndex;
            }
            else if (range)
            {
                if (!treeDataGrid.QueryCancelSelection())
                {
                    var anchor = AnchorIndex;
                    var i = Math.Max(_source.Rows.ModelIndexToRowIndex(anchor), 0);
                    var step = i < rowIndex ? 1 : -1;

                    using (BatchUpdate())
                    {
                        Clear();

                        while (true)
                        {
                            var m = _source.Rows.RowIndexToModelIndex(i);
                            Select(m);
                            if (i == rowIndex)
                                break;
                            i += step;
                        }
                    }

                    AnchorIndex = anchor;
                }
            }
            else if (multi && toggle)
            {
                if (!treeDataGrid.QueryCancelSelection())
                {
                    if (IsSelected(modelIndex) == true)
                        Deselect(modelIndex);
                    else
                        Select(modelIndex);
                }
            }
            else if (toggle)
            {
                if (!treeDataGrid.QueryCancelSelection())
                    SelectedIndex = (SelectedIndex == rowIndex) ? -1 : modelIndex;
            }
            else if (SelectedIndex != modelIndex || Count > 1)
            {
                if (!treeDataGrid.QueryCancelSelection())
                    SelectedIndex = modelIndex;
            }
        }
    }
}
