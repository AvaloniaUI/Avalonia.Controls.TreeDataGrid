using System;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Interface for filter controls that can be added to column headers.
    /// </summary>
    public interface IFilterControl
    {
        /// <summary>
        /// Gets the visual control element.
        /// </summary>
        Control Control { get; }

        /// <summary>
        /// Event raised when the filter value changes.
        /// </summary>
        event EventHandler<FilterValueChangedEventArgs>? FilterValueChanged;
    }
    
    /// <summary>
    /// Event arguments for filter value changes.
    /// </summary>
    public class FilterValueChangedEventArgs : EventArgs
    {
        public IFilterableColumn Column { get; }
        /// <summary>
        /// Gets the new filter value.
        /// </summary>
        public object? FilterCondition { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterValueChangedEventArgs"/> class.
        /// </summary>
        /// <param name="filterCondition">The new filter value.</param>
        public FilterValueChangedEventArgs(IFilterableColumn column, object? filterCondition)
        {
            Column = column;
            FilterCondition = filterCondition;
        }
    }
    
    /// <summary>
    /// Interface for creating filter controls for column headers.
    /// </summary>
    public interface IFilterControlFactory
    {
        /// <summary>
        /// Creates a filter control for a column header.
        /// </summary>
        /// <param name="column">The column for which to create a filter control.</param>
        /// <param name="initialValue">The initial filter value.</param>
        /// <returns>A filter control that can be used to filter the column, or null if no filter is available.</returns>
        IFilterControl? CreateFilterControl(IColumn column, object? initialValue);
    }

    /// <summary>
    /// A text filter control implementation.
    /// </summary>
    public class TextFilterControl : IFilterControl
    {
        private readonly TextBox _textBox;
        
        /// <summary>
        /// Gets the visual control element.
        /// </summary>
        public Control Control => _textBox;
        
        /// <summary>
        /// Gets or sets the current filter value.
        /// </summary>
        public object? FilterValue 
        { 
            get => _textBox.Text;
            set => _textBox.Text = value?.ToString() ?? string.Empty;
        }
        
        /// <summary>
        /// Event raised when the filter value changes.
        /// </summary>
        public event EventHandler<FilterValueChangedEventArgs>? FilterValueChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFilterControl"/> class.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="watermark">The watermark to display in the text box.</param>
        /// <param name="initialValue">The initial filter value.</param>
        public TextFilterControl(IFilterableColumn column, string watermark, object? initialValue)
        {
            _textBox = new TextBox
            {
                Text = initialValue?.ToString() ?? string.Empty,
                Watermark = watermark,
                Margin = new Thickness(2, 1),
                FontSize = 11,
                Height = 20
            };
            
            _textBox.GetObservable(TextBox.TextProperty).Subscribe(text =>
            {
                FilterValueChanged?.Invoke(this, new FilterValueChangedEventArgs(
                    column,
                    string.IsNullOrWhiteSpace(text) ? null : text));
            });
        }
    }
    
    /// <summary>
    /// A checkbox filter control implementation.
    /// </summary>
    public class CheckBoxFilterControl : IFilterControl
    {
        private readonly CheckBox _checkBox;
        
        /// <summary>
        /// Gets the visual control element.
        /// </summary>
        public Control Control => _checkBox;
        
        /// <summary>
        /// Gets or sets the current filter value.
        /// </summary>
        public object? FilterValue 
        { 
            get => _checkBox.IsChecked;
            set => _checkBox.IsChecked = value as bool?;
        }
        
        /// <summary>
        /// Event raised when the filter value changes.
        /// </summary>
        public event EventHandler<FilterValueChangedEventArgs>? FilterValueChanged;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckBoxFilterControl"/> class.
        /// </summary>
        /// <param name="isThreeState">Whether the checkbox should support three states.</param>
        /// <param name="initialValue">The initial filter value.</param>
        public CheckBoxFilterControl(IFilterableColumn column, bool isThreeState, object? initialValue)
        {
            bool? initialChecked = null;
            if (initialValue is bool boolValue)
            {
                initialChecked = boolValue;
            }
            else if (initialValue is string strValue && bool.TryParse(strValue, out var parsedValue))
            {
                initialChecked = parsedValue;
            }
            
            _checkBox = new CheckBox
            {
                IsThreeState = isThreeState,
                IsChecked = initialChecked,
                Margin = new Thickness(4),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            _checkBox.GetObservable(ToggleButton.IsCheckedProperty).Subscribe(isChecked =>
            {
                FilterValueChanged?.Invoke(this, new FilterValueChangedEventArgs(column, isChecked));
            });
        }
    }
}
