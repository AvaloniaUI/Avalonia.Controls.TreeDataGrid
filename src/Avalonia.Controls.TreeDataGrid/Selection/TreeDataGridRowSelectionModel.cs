using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Selection
{
    public class TreeDataGridRowSelectionModel<TModel> : TreeSelectionModelBase<TModel>,
        ITreeDataGridRowSelectionModel<TModel>,
        ITreeDataGridSelectionInteraction
        where TModel : class
    {
        private readonly ITreeDataGridSource<TModel> _source;
        private EventHandler? _viewSelectionChanged;
        private Point _pressedPoint;
        private bool _raiseViewSelectionChanged;
        private int _lastCharPressedTime;
        private string _typedWord = "";

        public TreeDataGridRowSelectionModel(ITreeDataGridSource<TModel> source)
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
                        && anchor?.Rows is HierarchicalRows<TModel> hierarchicalRows && anchorRowIndex > 0)
                    {
                        var newIndex = hierarchicalRows.GetParentRowIndex(AnchorIndex);
                        UpdateSelection(sender, newIndex, true);
                        sender.RowsPresenter.BringIntoView(newIndex);
                        FocusManager.Instance.Focus(sender);
                    }

                    if (!e.Handled && direction == NavigationDirection.Right
                       && anchor?.Rows is HierarchicalRows<TModel> hierarchicalRows2 && hierarchicalRows2[anchorRowIndex].IsExpanded)
                    {
                        var newIndex = anchorRowIndex + 1;
                        UpdateSelection(sender, newIndex, true);
                        sender.RowsPresenter.BringIntoView(newIndex);
                    }
                }
            }

        }

        protected void HandleTextInput(string? text, TreeDataGrid treeDataGrid, int selectedRowIndex)
        {
            if (text != null && treeDataGrid.Columns != null)
            {
                var typedChar = text.ToUpper()[0];

                int now = Environment.TickCount;
                int time = 0;
                if (_lastCharPressedTime > 0)
                {
                    time = now - _lastCharPressedTime;
                }

                string candidatePattern;
                if (time < 500)
                {
                    if (_typedWord.Length == 1 && typedChar == _typedWord[0])
                    {
                        candidatePattern = _typedWord;
                    }
                    else
                    {
                        candidatePattern = _typedWord + typedChar;
                    }
                }
                else
                {
                    candidatePattern = typedChar.ToString();
                }
                foreach (var column in treeDataGrid.Columns)
                {
                    if (column is ITextSearchableColumn<TModel> textSearchableColumn && textSearchableColumn.IsTextSearchEnabled)
                    {
                        Search(treeDataGrid, candidatePattern, selectedRowIndex, textSearchableColumn);
                    }
                    else if (column is HierarchicalExpanderColumn<TModel> hierarchicalColumn &&
                        hierarchicalColumn.Inner is ITextSearchableColumn<TModel> textSearchableColumn2 &&
                        textSearchableColumn2.IsTextSearchEnabled)
                    {
                        Search(treeDataGrid, candidatePattern, selectedRowIndex, textSearchableColumn2);
                    }

                }
                _lastCharPressedTime = now;
            }
        }

        private void Search(TreeDataGrid treeDataGrid, string candidatePattern, int selectedRowIndex, ITextSearchableColumn<TModel> column)
        {
            var found = false;
            for (int i = candidatePattern.Length == 1 ? selectedRowIndex + 1 : selectedRowIndex; i <= _source.Rows.Count - 1; i++)
            {
                found = SearchAndSelectRow(treeDataGrid, candidatePattern, i, (TModel?)_source.Rows[i].Model, column.SelectValue);
                if (found)
                {
                    break;
                }
            }
            if (!found)
            {
                for (int i = 0; i <= selectedRowIndex; i++)
                {
                    found = SearchAndSelectRow(treeDataGrid, candidatePattern, i, (TModel?)_source.Rows[i].Model, column.SelectValue);
                    if (found)
                    {
                        break;
                    }
                }
            }
        }

        void ITreeDataGridSelectionInteraction.OnPreviewKeyDown(TreeDataGrid sender, KeyEventArgs e)
        {

            static bool IsElementFullyVisibleToUser(TransformedBounds controlBounds)
            {
                var rect = controlBounds.Bounds.TransformToAABB(controlBounds.Transform);
                // Round rect.Bottom because sometimes it's value isn't precise.
                return controlBounds.Clip.Contains(rect.TopLeft) &&
                    controlBounds.Clip.Contains(new Point(rect.BottomRight.X, Math.Round(rect.BottomRight.Y, 5, MidpointRounding.ToZero)));
            }

            static bool GetRowIndexIfFullyVisible(IControl? control, out int index)
            {
                if (control is TreeDataGridRow row &&
                    row.TransformedBounds != null &&
                    IsElementFullyVisibleToUser(row.TransformedBounds.Value))
                {
                    index = row.RowIndex;
                    return true;
                }
                index = -1;
                return false;
            }

            void UpdateSelectionAndBringIntoView(int newIndex)
            {
                UpdateSelection(sender, newIndex, true);
                sender.RowsPresenter?.BringIntoView(newIndex);
                sender.Focus();
                e.Handled = true;
            }

            if ((e.Key == Key.PageDown || e.Key == Key.PageUp) && sender.RowsPresenter?.Items != null)
            {
                var children = sender.RowsPresenter.RealizedElements;
                var childrenCount = children.Count;
                if (childrenCount > 0)
                {
                    var newIndex = 0;
                    var isIndexSet = false;
                    if (e.Key == Key.PageDown)
                    {
                        for (int i = childrenCount - 1; i >= 0; i--)
                        {
                            if (GetRowIndexIfFullyVisible(children[i], out var index))
                            {
                                newIndex = index;
                                isIndexSet = true;
                                break;
                            }
                        }
                        if (isIndexSet && SelectedIndex[0] != newIndex)
                        {
                            UpdateSelectionAndBringIntoView(newIndex);
                            return;
                        }
                        else if (childrenCount + newIndex <= sender.RowsPresenter.Items.Count)
                        {
                            newIndex = childrenCount - 2 + newIndex;
                        }
                        else
                        {
                            newIndex = sender.RowsPresenter.Items.Count - 1;
                        }
                    }
                    else if (e.Key == Key.PageUp)
                    {
                        for (int i = 0; i <= childrenCount - 1; i++)
                        {
                            if (GetRowIndexIfFullyVisible(children[i], out var index))
                            {
                                newIndex = index;
                                isIndexSet = true;
                                break;
                            }
                        }
                        if (isIndexSet && SelectedIndex[0] != newIndex)
                        {
                            UpdateSelectionAndBringIntoView(newIndex);
                            return;
                        }
                        else if (isIndexSet && newIndex - childrenCount > 0)
                        {
                            newIndex = newIndex - childrenCount + 2;
                        }
                        else
                        {
                            newIndex = 0;
                        }
                    }
                    UpdateSelectionAndBringIntoView(newIndex);
                }
            }
        }

        private bool SearchAndSelectRow(TreeDataGrid treeDataGrid,
            string candidatePattern, int newIndex, TModel? model, Func<TModel, string?>? valueSelector)
        {
            if (valueSelector != null && model != null)
            {
                var value = valueSelector(model);
                if (value != null && value.ToUpper().StartsWith(candidatePattern))
                {
                    UpdateSelection(treeDataGrid, newIndex, true);
                    treeDataGrid.RowsPresenter?.BringIntoView(newIndex);
                    _typedWord = candidatePattern;
                    return true;
                }
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

        void ITreeDataGridSelectionInteraction.OnTextInput(TreeDataGrid sender, TextInputEventArgs e)
        {
            HandleTextInput(e.Text, sender, _source.Rows.ModelIndexToRowIndex(AnchorIndex));
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

        protected internal override IEnumerable<TModel>? GetChildren(TModel node)
        {
            if (_source is HierarchicalTreeDataGridSource<TModel> treeSource)
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
