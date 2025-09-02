using System;

namespace Avalonia.Controls.Models.TreeDataGrid;

public interface IValueFilter
{
    bool Passes(object? condition, object? value);
}
/// <summary>
/// Represents an immutable filter that can be applied to a value.
/// </summary>
/// <typeparam name="TValue">The value type.</typeparam>
public interface IValueFilter<TValue>: IValueFilter
{
    /// <summary>
    /// Determines whether the value passes this filter with the given condition.
    /// </summary>
    /// <param name="condition">The filter condition (e.g., filter text, checkbox state).</param>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value passes the filter, otherwise false.</returns>
    bool Passes(object? condition, TValue? value);
    bool IValueFilter.Passes(object? condition, object? value) => value is TValue typedValue && Passes(condition, typedValue);
}
