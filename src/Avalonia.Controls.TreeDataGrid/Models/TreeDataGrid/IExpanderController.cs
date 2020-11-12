namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents an element which handles an <see cref="IExpander"/> state change.
    /// </summary>
    public interface IExpanderController
    {
        /// <summary>
        /// Expands an <see cref="IExpander"/>.
        /// </summary>
        /// <param name="expander">The expander.</param>
        /// <returns>
        /// true if the expand operation was sucessful, false if there were no items to expand.
        /// </returns>
        bool TryExpand(IExpander expander);

        /// <summary>
        /// Collapses an <see cref="IExpander"/>.
        /// </summary>
        /// <param name="expander">The expander.</param>
        void Collapse(IExpander expander);
    }
}
