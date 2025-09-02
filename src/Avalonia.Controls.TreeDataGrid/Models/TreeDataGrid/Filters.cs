using System;
using System.Collections.Generic;
using System.Linq;

namespace Avalonia.Controls.Models.TreeDataGrid;

/// <summary>
/// Defines the mode of text filtering.
/// </summary>
public enum TextFilterMode
{
    /// <summary>
    /// Check if the value contains the filter text.
    /// </summary>
    Contains,
    
    /// <summary>
    /// Check if the value starts with the filter text.
    /// </summary>
    StartsWith,
    
    /// <summary>
    /// Check if the value ends with the filter text.
    /// </summary>
    EndsWith
}

/// <summary>
/// A filter that checks text values.
/// </summary>
public class TextValueFilter : IValueFilter<string>
{
    /// <summary>
    /// Gets or sets the filter mode to use.
    /// </summary>
    public TextFilterMode FilterMode { get; set; } = TextFilterMode.Contains;
    
    /// <summary>
    /// Gets or sets whether filtering is case sensitive.
    /// </summary>
    public bool CaseSensitive { get; set; } = false;
    
    /// <summary>
    /// Initializes a new instance of <see cref="TextValueFilter"/> with default settings.
    /// </summary>
    public TextValueFilter()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of <see cref="TextValueFilter"/> with specified filter mode.
    /// </summary>
    /// <param name="mode">The filter mode to use.</param>
    /// <param name="caseSensitive">Whether filtering is case sensitive.</param>
    public TextValueFilter(TextFilterMode mode, bool caseSensitive = false)
    {
        FilterMode = mode;
        CaseSensitive = caseSensitive;
    }
    
    /// <summary>
    /// Determines if the value passes the filter.
    /// </summary>
    /// <param name="condition">The filter condition (should be a string).</param>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value passes the filter, otherwise false.</returns>
    public bool Passes(object? condition, string? value)
    {
        if (condition is not string filterText || string.IsNullOrWhiteSpace(filterText))
            return true;
            
        if (value == null)
            return false;
            
        // Determine string comparison
        StringComparison comparison = CaseSensitive ? 
            StringComparison.Ordinal : 
            StringComparison.OrdinalIgnoreCase;
        
        // Apply filter based on mode
        return FilterMode switch
        {
            TextFilterMode.Contains => value.Contains(filterText, comparison),
            TextFilterMode.StartsWith => value.StartsWith(filterText, comparison),
            TextFilterMode.EndsWith => value.EndsWith(filterText, comparison),
            _ => value.Contains(filterText, comparison) // Default to Contains
        };
    }
}

/// <summary>
/// A filter that checks boolean values.
/// </summary>
public class BooleanValueFilter : IValueFilter<bool>
{
    /// <summary>
    /// Determines if the value passes the filter.
    /// </summary>
    /// <param name="condition">The filter condition (should be a bool? or string).</param>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value passes the filter, otherwise false.</returns>
    bool IValueFilter<bool>.Passes(object? condition, bool value)
    {
        if (condition == null)
            return true;
        // C# does not support nullable at type level 
        // if (value == null)
            // return false;
        if (condition is bool b)
            return b == value;
        if (condition is string s && bool.TryParse(s, out var parsed))
            return parsed == value;
        return false;
    }

    public bool Passes(object? condition, bool? value)
    {
        if (value == null)
            return false;
        return ((IValueFilter<bool>)this).Passes(condition, value);
    }
}

/// <summary>
/// A filter that checks if a value is in a set of allowed values.
/// </summary>
/// <typeparam name="TValue">The value type.</typeparam>
public class SetValueFilter<TValue> : IValueFilter<TValue>
{
    /// <summary>
    /// Determines if the value passes the filter.
    /// </summary>
    /// <param name="condition">The filter condition (should be a collection of TValue or string).</param>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value passes the filter, otherwise false.</returns>
    public bool Passes(object? condition, TValue? value)
    {
        // Handle null condition (no filter)
        if (condition == null)
            return true;
        if (value == null)
            return false;

        // Create a hash set from the condition
        HashSet<TValue> allowedValues;
        var comparer = EqualityComparer<TValue>.Default;
            
        if (condition is IEnumerable<TValue> values)
        {
            allowedValues = new HashSet<TValue>(values, comparer);
        }
        else if (condition is string s && !string.IsNullOrWhiteSpace(s))
        {
            // For string condition, this would depend on TValue
            // This is a simplified example that only works for string values
            if (typeof(TValue) == typeof(string))
            {
                var parts = s.Split(',');
                var stringValues = new HashSet<string>(
                    parts.Select(p => p.Trim()),
                    StringComparer.OrdinalIgnoreCase);
                        
                allowedValues = new HashSet<TValue>(
                    stringValues.Cast<TValue>(),
                    comparer);
            }
            else
            {
                // If we can't parse the condition for non-string types, don't filter
                return true;
            }
        }
        else
        {
            // If we can't use the condition, don't filter
            return true;
        }

        // If no values to filter by, don't filter
        if (allowedValues.Count == 0)
            return true;

        // Check if the value is in the allowed values
        return allowedValues.Contains(value);
    }
}
