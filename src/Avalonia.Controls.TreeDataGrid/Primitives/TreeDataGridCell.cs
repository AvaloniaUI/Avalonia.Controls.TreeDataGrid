using System;
using System.ComponentModel;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using Avalonia.LogicalTree;

namespace Avalonia.Controls.Primitives
{
    [PseudoClasses(":editing")]
    public abstract class TreeDataGridCell : TemplatedControl, ITreeDataGridCell
    {
        public static readonly DirectProperty<TreeDataGridCell, bool> IsSelectedProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridCell, bool>(
                nameof(IsSelected),
                o => o.IsSelected);

        private bool _isEditing;
        private bool _isSelected;
        private TreeDataGrid? _treeDataGrid;
        private Point _pressedPoint;

        static TreeDataGridCell()
        {
            FocusableProperty.OverrideDefaultValue<TreeDataGridCell>(true);
            DoubleTappedEvent.AddClassHandler<TreeDataGridCell>((x, e) => x.OnDoubleTapped(e));
        }

        public int ColumnIndex { get; private set; } = -1;
        public int RowIndex { get; private set; } = -1;
        public ICell? Model { get; private set; }

        public bool IsSelected
        {
            get => _isSelected;
            private set => SetAndRaise(IsSelectedProperty, ref _isSelected, value);
        }

        public virtual void Realize(
            TreeDataGridElementFactory factory,
            ITreeDataGridSelectionInteraction? selection,
            ICell model,
            int columnIndex,
            int rowIndex)
        {
            if (columnIndex < 0)
                throw new IndexOutOfRangeException("Invalid column index.");

            ColumnIndex = columnIndex;
            RowIndex = rowIndex;
            Model = model;
            IsSelected = selection?.IsCellSelected(columnIndex, rowIndex) ?? false;

            _treeDataGrid?.RaiseCellPrepared(this, columnIndex, RowIndex);
        }

        public virtual void Unrealize()
        {
            _treeDataGrid?.RaiseCellClearing(this, ColumnIndex, RowIndex);
            ColumnIndex = RowIndex = -1;
            Model = null;
        }

        protected virtual bool CanEdit => false;

        protected void BeginEdit()
        {
            if (!_isEditing)
            {
                _isEditing = true;
                (Model as IEditableObject)?.BeginEdit();
                PseudoClasses.Add(":editing");
            }
        }

        protected void CancelEdit()
        {
            if (EndEditCore() && Model is IEditableObject editable)
                editable.CancelEdit();
        }

        protected void EndEdit()
        {
            if (EndEditCore() && Model is IEditableObject editable)
                editable.EndEdit();
        }

        protected void SubscribeToModelChanges()
        {
            if (Model is INotifyPropertyChanged inpc)
                inpc.PropertyChanged += OnModelPropertyChanged;
        }

        protected void UnsubscribeFromModelChanges()
        {
            if (Model is INotifyPropertyChanged inpc)
                inpc.PropertyChanged -= OnModelPropertyChanged;
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            _treeDataGrid = this.FindLogicalAncestorOfType<TreeDataGrid>();
            base.OnAttachedToLogicalTree(e);
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            _treeDataGrid = null;
            base.OnDetachedFromLogicalTree(e);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            // The cell may be realized before being parented. In this case raise the CellPrepared event here.
            if (_treeDataGrid is not null && ColumnIndex >= 0 && RowIndex >= 0)
                _treeDataGrid.RaiseCellPrepared(this, ColumnIndex, RowIndex);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var result = base.MeasureOverride(availableSize);

            // HACKFIX for #83. Seems that cells are getting truncated at times due to DPI scaling.
            // New text stack in Avalonia 11.0 should fix this but until then a hack to add a pixel
            // to cell size seems to fix it.
            result = result.Inflate(new Thickness(1, 0));

            return result;
        }

        protected virtual void OnTapped(TappedEventArgs e)
        {
            if (!_isEditing && CanEdit && Model?.SingleTapEdit == true && !e.Handled)
            {
                BeginEdit();
                e.Handled = true;
            }
        }

        protected virtual void OnDoubleTapped(TappedEventArgs e)
        {
            if (!_isEditing && CanEdit && Model?.SingleTapEdit != true && !e.Handled)
            {
                BeginEdit();
                e.Handled = true;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!_isEditing && CanEdit && !e.Handled && e.Key == Key.F2)
            {
                BeginEdit();
                e.Handled = true;
            }
        }

        protected virtual void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (!_isEditing && CanEdit && Model?.SingleTapEdit == true && !e.Handled)
            {
                _pressedPoint = e.GetCurrentPoint(this).Position;
                e.Handled = true;
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (!_isEditing && CanEdit && Model?.SingleTapEdit == true && !e.Handled)
            {
                var point = e.GetCurrentPoint(this).Position;

                if (new Rect(Bounds.Size).ContainsExclusive(point))
                {
                    BeginEdit();
                    e.Handled = true;
                }
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == IsSelectedProperty)
            {
                PseudoClasses.Set(":selected", IsSelected);
            }

            base.OnPropertyChanged(change);
        }

        public void UpdateRowIndex(int index) => RowIndex = index;

        internal void UpdateSelection(ITreeDataGridSelectionInteraction? selection)
        {
            IsSelected = selection?.IsCellSelected(ColumnIndex, RowIndex) ?? false;
        }

        private bool EndEditCore()
        {
            if (_isEditing)
            {
                var restoreFocus = IsKeyboardFocusWithin;
                _isEditing = false;
                PseudoClasses.Remove(":editing");
                if (restoreFocus)
                    Focus();
                return true;
            }

            return false;
        }
    }
}
