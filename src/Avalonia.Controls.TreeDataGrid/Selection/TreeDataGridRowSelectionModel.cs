using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.LogicalTree;

namespace Avalonia.Controls.Selection
{
    public class TreeDataGridRowSelectionModel<T> : TreeSelectionModelBase<T>,
        ITreeDataGridRowSelectionModel<T>,
        ITreeDataGridSelectionInteraction
    {
        private readonly ITreeDataGridSource<T> _source;
        private EventHandler? _viewSelectionChanged;
        private Point _pressedPoint;
        private bool _raiseViewSelectionChanged;
        private int _lastPageSelectedIndex;

        public TreeDataGridRowSelectionModel(ITreeDataGridSource<T> source)
            : base(source.Items)
        {
            _source = source;
            SelectionChanged += (s, e) =>
            {
                if (!IsSourceCollectionChanging)
                    _viewSelectionChanged?.Invoke(this, e);
                else
                    _raiseViewSelectionChanged = true;
            };
        }

        event EventHandler? ITreeDataGridSelectionInteraction.SelectionChanged
        {
            add => _viewSelectionChanged += value;
            remove => _viewSelectionChanged -= value;
        }

        IEnumerable? ITreeDataGridSelection.Source
        {
            get => Source;
            set => Source = value;
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

        void ITreeDataGridSelectionInteraction.OnKeyDown(TreeDataGrid sender, KeyEventArgs e)
        {
            if (sender.RowsPresenter is null)
                return;

            if (!e.Handled)
            {
                var direction = e.Key.ToNavigationDirection();
                var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
                var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

                if (direction.HasValue)
                {
                    var anchorRowIndex = _source.Rows.ModelIndexToRowIndex(AnchorIndex);

                    sender.RowsPresenter.BringIntoView(anchorRowIndex);

                    var anchor = sender.TryGetRow(anchorRowIndex);

                    if (anchor is not null && !ctrl)
                    {
                        e.Handled = TryKeyExpandCollapse(sender, direction.Value, anchor);
                    }

                    if (!e.Handled && (!ctrl || shift))
                    {
                        e.Handled = MoveSelection(sender, direction.Value, shift, anchor);
                    }

                    if (!e.Handled && direction == NavigationDirection.Left
                        && anchor?.Rows is HierarchicalRows<T> hierarchicalRows && anchorRowIndex > 0)
                    {
                        var newIndex = hierarchicalRows.GetParentRowIndex(AnchorIndex);
                        UpdateSelection(sender, newIndex, true);
                        sender.RowsPresenter.BringIntoView(newIndex);
                        FocusManager.Instance.Focus(sender);
                    }

                    if (!e.Handled && direction == NavigationDirection.Right
                       && anchor?.Rows is HierarchicalRows<T> hierarchicalRows2 && hierarchicalRows2[anchorRowIndex].IsExpanded)
                    {
                        var newIndex = anchorRowIndex + 1;
                        UpdateSelection(sender, newIndex, true);
                        sender.RowsPresenter.BringIntoView(newIndex);
                    }
                }
            }

        }
        void ITreeDataGridSelectionInteraction.OnPreviewKeyDown(TreeDataGrid sender, KeyEventArgs e)
        {
            if (e.Key == Key.PageDown)
            {
                var children = sender.RowsPresenter.GetLogicalChildren();
                var childrenCount = children.Count();
                if (_lastPageSelectedIndex > 0&&SelectedIndex == _lastPageSelectedIndex)
                {
                    int newIndex;
                    if (childrenCount + _lastPageSelectedIndex <= sender.RowsPresenter!.Items!.Count)
                    {
                        newIndex = childrenCount - 1 + _lastPageSelectedIndex;
                    }
                    else
                    {
                        newIndex = sender.RowsPresenter!.Items!.Count - 1;
                    }
                    UpdateSelection(sender, newIndex, true);
                    sender.RowsPresenter!.BringIntoView(newIndex);
                    _lastPageSelectedIndex = newIndex;

                }
                if (_lastPageSelectedIndex == 0||SelectedIndex!=_lastPageSelectedIndex)
                {
                    var max = -1;
                    for (int i = 0; i <= childrenCount - 1; i++)
                    {
                        if ((children.ElementAt(i) as TreeDataGridRow)!.RowIndex > max)
                        {
                            var element = (children.ElementAt(i) as TreeDataGridRow);
                            var tb = element!.TransformedBounds!.Value;
                            if (tb!.Clip.Contains(tb!.Bounds.TransformToAABB(tb!.Transform)) == true)
                            {
                                max = element!.RowIndex;
                            }
                        }
                    }
                    _lastPageSelectedIndex = max;
                    UpdateSelection(sender, max, true);
                    sender.RowsPresenter!.BringIntoView(max);
                }
            }

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

        protected override void OnSourceCollectionChangeFinished()
        {
            if (_raiseViewSelectionChanged)
            {
                _viewSelectionChanged?.Invoke(this, EventArgs.Empty);
                _raiseViewSelectionChanged = false;
            }
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
                    toggleModifier: e.KeyModifiers.HasFlag(AvaloniaLocator.Current.GetService<PlatformHotkeyConfiguration>().CommandModifiers),
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
                    var anchor = RangeAnchorIndex;
                    var i = Math.Max(_source.Rows.ModelIndexToRowIndex(anchor), 0);
                    var step = i < rowIndex ? 1 : -1;

                    using (BatchUpdate())
                    {
                        Clear();

                        while (true)
                        {
                            var m = _source.Rows.RowIndexToModelIndex(i);
                            Select(m);
                            anchor = m;
                            if (i == rowIndex)
                                break;
                            i += step;
                        }
                    }
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

        private bool TryKeyExpandCollapse(
            TreeDataGrid treeDataGrid,
            NavigationDirection direction,
            TreeDataGridRow focused)
        {
            if (treeDataGrid.RowsPresenter is null || focused.RowIndex < 0)
                return false;

            var row = _source.Rows[focused.RowIndex];

            if (row is IExpander expander)
            {
                if (direction == NavigationDirection.Right && !expander.IsExpanded)
                {
                    expander.IsExpanded = true;
                    return true;
                }
                else if (direction == NavigationDirection.Left && expander.IsExpanded)
                {
                    expander.IsExpanded = false;
                    return true;
                }
            }

            return false;
        }

        private bool MoveSelection(
            TreeDataGrid treeDataGrid,
            NavigationDirection direction,
            bool rangeModifier,
            TreeDataGridRow? focused)
        {
            if (treeDataGrid.RowsPresenter is null || _source.Columns.Count == 0 || _source.Rows.Count == 0)
                return false;

            var currentRowIndex = focused?.RowIndex ?? _source.Rows.ModelIndexToRowIndex(SelectedIndex);
            int newRowIndex;

            if (direction == NavigationDirection.First || direction == NavigationDirection.Last)
            {
                newRowIndex = direction == NavigationDirection.First ? 0 : _source.Rows.Count - 1;
            }
            else
            {
                (var x, var y) = direction switch
                {
                    NavigationDirection.Up => (0, -1),
                    NavigationDirection.Down => (0, 1),
                    NavigationDirection.Left => (-1, 0),
                    NavigationDirection.Right => (1, 0),
                    _ => (0, 0)
                };

                newRowIndex = Math.Max(0, Math.Min(currentRowIndex + y, _source.Rows.Count - 1));
            }

            if (newRowIndex != currentRowIndex)
                UpdateSelection(treeDataGrid, newRowIndex, true, rangeModifier);

            if (newRowIndex != currentRowIndex)
            {
                treeDataGrid.RowsPresenter?.BringIntoView(newRowIndex);
                treeDataGrid.TryGetRow(newRowIndex)?.Focus();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
