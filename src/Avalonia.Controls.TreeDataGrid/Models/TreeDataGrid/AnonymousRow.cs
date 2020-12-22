using System.Diagnostics.CodeAnalysis;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    internal class AnonymousRow<TModel> : RowBase<TModel>
    {
        private int _index;
        [AllowNull] private TModel _model;

        public override int Index => _index;
        public override object? Header => _index;
        public override TModel Model => _model;

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
    }
}
