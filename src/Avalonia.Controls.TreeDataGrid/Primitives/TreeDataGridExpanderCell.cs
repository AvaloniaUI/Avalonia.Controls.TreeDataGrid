using System;
using Avalonia;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridExpanderCell : TemplatedControl
    {
        public static readonly DirectProperty<TreeDataGridExpanderCell, int> IndentProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridExpanderCell, int>(
                nameof(Indent),
                o => o.Indent);

        public static readonly DirectProperty<TreeDataGridExpanderCell, bool> IsExpandedProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridExpanderCell, bool>(
                nameof(IsExpanded),
                o => o.IsExpanded,
                (o, v) => o.IsExpanded = v);

        public static readonly DirectProperty<TreeDataGridExpanderCell, bool> ShowExpanderProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridExpanderCell, bool>(
                nameof(ShowExpander),
                o => o.ShowExpander);

        private Decorator? _contentContainer;
        private Type? _contentType;

        public int Indent => (Model?.Row as IIndentedRow)?.Indent ?? 0;

        public bool IsExpanded
        {
            get => Model?.IsExpanded ?? false;
            set
            {
                if (Model is IExpanderCell model)
                    model.IsExpanded = value;
            }
        }

        public bool ShowExpander => Model?.ShowExpander ?? false;

        private IExpanderCell? Model => (IExpanderCell?)DataContext;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            _contentContainer = e.NameScope.Find<Decorator>("PART_Content");
            UpdateContent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            RaisePropertyChanged(IndentProperty, default, default);
            RaisePropertyChanged(IsExpandedProperty, default, default);
            RaisePropertyChanged(ShowExpanderProperty, default, default);
            UpdateContent();
        }

        private void UpdateContent()
        {
            if (_contentContainer is null)
                return;

            var content = Model?.Content;

            if (content is object)
            {
                var contentType = content.GetType();

                if (contentType != _contentType)
                {
                    var owner = this.FindAncestorOfType<TreeDataGrid>();

                    if (owner is null)
                        return;
                    
                    _contentContainer.Child = owner.ElementFactory.Build(content);
                    _contentType = contentType;
                }

                _contentContainer.Child.DataContext = content;
            }
        }
    }
}
