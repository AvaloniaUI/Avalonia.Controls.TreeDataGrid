using System.Collections.Generic;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Base class for columns in an <see cref="HierarchicalTreeDataGridSource{TModel}"/> whose
    /// cells show an expander to reveal nested data.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class ExpanderColumnBase<TModel> : ColumnBase<TModel>,
        IExpanderController
    {
        private readonly IExpanderController _owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpanderColumnBase{TModel}"/> class.
        /// </summary>
        /// <param name="owner">
        /// The owner of the column, usually a <see cref="HierarchicalTreeDataGridSource{TModel}"/>.
        /// </param>
        /// <param name="header">The column header.</param>
        /// <param name="width">The column width.</param>
        protected ExpanderColumnBase(
            IExpanderController owner,
            object? header,
            GridLength width)
            : base(header, width)
        {
            _owner = owner;
        }

        /// <summary>
        /// Creates a cell for the column from a model.
        /// </summary>
        /// <param name="index">The index of the cell in the data.</param>
        /// <param name="model">The model from which to select the cell value.</param>
        public abstract ICell CreateCell(IndexPath index, TModel model);

        /// <summary>
        /// Gets the child models for a specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        public abstract IEnumerable<TModel>? GetChildModels(TModel model);

        /// <summary>
        /// Called when a cell in this column is expanded or collapsed.
        /// </summary>
        /// <param name="expander">The cell.</param>
        public void IsExpandedChanged(IExpander expander) => _owner.IsExpandedChanged(expander);
    }
}
