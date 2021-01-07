using System;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a cell in a <see cref="HierarchicalTreeDataGridSource{TModel}"/> which displays
    /// an expander to reveal nested data.
    /// </summary>
    public interface IExpanderCell : ICell, IExpander
    {
        /// <summary>
        /// Gets the index of the row in the source data.
        /// </summary>
        [Obsolete("TODO: Find a better way to do this.")]
        IndexPath ModelIndexPath { get; }

        /// <summary>
        /// Gets a value indicating whether the expander should be shown.
        /// </summary>
        bool ShowExpander { get; }
    }
}
