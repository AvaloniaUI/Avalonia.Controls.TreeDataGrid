namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents an element which handles an <see cref="IExpander"/> state change.
    /// </summary>
    public interface IExpanderController
    {
        /// <summary>
        /// Called by the expander when its <see cref="IExpander.IsExpanded"/> property changes.
        /// </summary>
        /// <param name="expander">The expander whose state has changed.</param>
        void IsExpandedChanged(IExpander expander);
    }
}
