using System;
using System.ComponentModel;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Input;
using Avalonia.LogicalTree;

namespace Avalonia.Controls.Primitives
{
    [PseudoClasses(":selected", ":editing")]
    public abstract class TreeDataGridCell : TemplatedControl, ITreeDataGridCell
    {
        public static readonly DirectProperty<TreeDataGridCell, bool> IsSelectedProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridCell, bool>(
                nameof(IsSelected),
                o => o.IsSelected,
                (o, v) => o.IsSelected = v);

        private bool _isEditing;
        private bool _isSelected;
        private TreeDataGrid? _treeDataGrid;

        static TreeDataGridCell()
        {
            FocusableProperty.OverrideDefaultValue<TreeDataGridCell>(true);
        }

        public int ColumnIndex { get; private set; } = -1;
        public int RowIndex { get; private set; } = -1;
        public ICell? Model { get; private set; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetAndRaise(IsSelectedProperty, ref _isSelected, value);
        }

        public virtual void Realize(IElementFactory factory, ICell model, int columnIndex, int rowIndex)
        {
            if (columnIndex < 0)
                throw new IndexOutOfRangeException("Invalid column index.");

            ColumnIndex = columnIndex;
            RowIndex = rowIndex;
            Model = model;

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
                (DataContext as IEditableObject)?.BeginEdit();
                PseudoClasses.Add(":editing");
            }
        }

        protected void CancelEdit()
        {
            if (EndEditCore())
                (DataContext as IEditableObject)?.CancelEdit();
        }

        protected void EndEdit()
        {
            if (EndEditCore())
                (DataContext as IEditableObject)?.EndEdit();
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            _treeDataGrid = this.FindLogicalAncestorOfType<TreeDataGrid>();
            base.OnAttachedToLogicalTree(e);

            // The cell may be realized before being parented. In this case raise the CellPrepared event here.
            if (_treeDataGrid is not null && ColumnIndex >= 0 && RowIndex >= 0)
                _treeDataGrid.RaiseCellPrepared(this, ColumnIndex, RowIndex);
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            _treeDataGrid = null;
            base.OnDetachedFromLogicalTree(e);
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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!_isEditing && CanEdit && !e.Handled && e.Key == Key.F2)
            {
                BeginEdit();
                e.Handled = true;
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (!_isEditing && CanEdit && !e.Handled && IsSelected)
            {
                BeginEdit();
                e.Handled = true;
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == IsSelectedProperty)
            {
                PseudoClasses.Set(":selected", change.GetNewValue<bool>());
            }
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
