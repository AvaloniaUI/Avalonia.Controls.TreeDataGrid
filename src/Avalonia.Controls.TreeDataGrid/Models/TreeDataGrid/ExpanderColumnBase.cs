using System.Collections.Generic;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Base class for columns in an <see cref="HierarchicalTreeDataGridSource{TModel}"/> whose
    /// cells show an expander to reveal nested data.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class ExpanderColumnBase<TModel> : ColumnBase<TModel>
    {
        private GridLength _width;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpanderColumnBase{TModel}"/> class.
        /// </summary>
        /// <param name="owner">
        /// The owner of the column, usually a <see cref="HierarchicalTreeDataGridSource{TModel}"/>.
        /// </param>
        /// <param name="header">The column header.</param>
        /// <param name="width">The column width.</param>
        protected ExpanderColumnBase(
            object? header,
            GridLength width)
        {
            Header = header;
            _width = width;
        }

        public override object? Header { get; }

        public override GridLength Width 
        {
            get => _width;
            set => _width = value;
        }

        /// <summary>
        /// Creates a cell for the column from a model.
        /// </summary>
        /// <param name="row">The row containing the data.</param>
        public abstract ICell CreateCell(HierarchicalRow<TModel> row);

        /// <summary>
        /// Gets the child models for a specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        public abstract IEnumerable<TModel>? GetChildModels(TModel model);
    }
}
