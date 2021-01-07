using System;
using System.Diagnostics.CodeAnalysis;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    internal class AnonymousRow<TModel> : RowBase<TModel>
    {
        private int _index;
        [AllowNull] private TModel _model;

        public override object? Header => _index;
        public override TModel Model => _model;
        public override int ModelIndex => _index;

        public override GridLength Height
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

        public override void UpdateModelIndex(int delta)
        {
            throw new NotSupportedException();
        }
    }
}
