using System;
using System.Diagnostics.CodeAnalysis;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// A row that can be reused for situations where creating a separate object for each row is
    /// not necessary.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <remarks>
    /// In a flat grid where rows cannot be resized, it is not necessary to persist any information
    /// about rows; the same row object can be updated and reused when a new row is requested.
    /// </remarks>
    internal class AnonymousRow<TModel> : IRow<TModel>
    {
        private int _index;
        [AllowNull] private TModel _model;

        public object? Header => _index;
        public TModel Model => _model;
        public int ModelIndex => _index;

        public GridLength Height
        {
            get => GridLength.Auto;
            set { }
        }

        public AnonymousRow<TModel> Update(int index, TModel model)
        {
            _index = index;
            _model = model;
            return this;
        }

        public void UpdateModelIndex(int delta)
        {
            throw new NotSupportedException();
        }
    }
}
