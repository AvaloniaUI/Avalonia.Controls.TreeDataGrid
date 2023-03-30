﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls.Templates;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// A column in an <see cref="ITreeDataGridSource"/> which displays its values using a data
    /// template.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TValue">The column data type.</typeparam>
    public class TemplateColumn<TModel> : ColumnBase<TModel>, ITextSearchableColumn<TModel>
    {
        private readonly Comparison<TModel?>? _sortAscending;
        private readonly Comparison<TModel?>? _sortDescending;
        private readonly Func<Control, IDataTemplate> _getCellTemplate;
        private IDataTemplate? _cellTemplate;
        private object? _cellTemplateResourceKey;

        public TemplateColumn(
            object? header,
            IDataTemplate cellTemplate,
            GridLength? width = null,
            ColumnOptions<TModel>? options = null)
            : base(header, width, options)
        {
            _sortAscending = options?.CompareAscending;
            _sortDescending = options?.CompareDescending;
            _getCellTemplate = GetCellTemplate;
            _cellTemplate = cellTemplate;
        }

        public TemplateColumn(
            object? header,
            object cellTemplateResourceKey,
            GridLength? width = null,
            ColumnOptions<TModel>? options = null)
            : base(header, width, options)
        {
            _sortAscending = options?.CompareAscending;
            _sortDescending = options?.CompareDescending;
            _getCellTemplate = GetCellTemplate;
            _cellTemplateResourceKey = cellTemplateResourceKey ??
                throw new ArgumentNullException(nameof(cellTemplateResourceKey));
        }

        public Func<TModel, string?>? TextSearchValueSelector { get; set; }
        public bool IsTextSearchEnabled { get; set; }


        /// <summary>
        /// Gets the template to use to display the contents of a cell that is not in editing mode.
        /// </summary>
        public IDataTemplate GetCellTemplate(Control anchor)
        {
            if (_cellTemplate is not null)
                return _cellTemplate;
            
            _cellTemplate = anchor.FindResource(_cellTemplateResourceKey!) as IDataTemplate;

            if (_cellTemplate is null)
                throw new KeyNotFoundException(
                    $"No data template resource with the key of '{_cellTemplateResourceKey}' " +
                    "could be found for the template column '{Header}'.");

            return _cellTemplate;
        }

        /// <summary>
        /// Creates a cell for this column on the specified row.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>The cell.</returns>
        public override ICell CreateCell(IRow<TModel> row) => new TemplateCell(row.Model, _getCellTemplate);

        public override Comparison<TModel?>? GetComparison(ListSortDirection direction)
        {
            return direction switch
            {
                ListSortDirection.Ascending => _sortAscending,
                ListSortDirection.Descending => _sortDescending,
                _ => null,
            };
        }

        string? ITextSearchableColumn<TModel>.SelectValue(TModel model) => TextSearchValueSelector?.Invoke(model);
    }
}
