using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using CollectionExtensions = Avalonia.Controls.Models.TreeDataGrid.CollectionExtensions;

namespace Avalonia.Controls.Primitives
{
    public abstract class TreeDataGridPresenterBase<TItem> : Control, IPanel, IPresenter
    {
        public static readonly DirectProperty<TreeDataGridPresenterBase<TItem>, IElementFactory?> ElementFactoryProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridPresenterBase<TItem>, IElementFactory?>(
                nameof(ElementFactory),
                o => o.ElementFactory,
                (o, v) => o.ElementFactory = v);

        public static readonly DirectProperty<TreeDataGridPresenterBase<TItem>, IReadOnlyList<TItem>?> ItemsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridPresenterBase<TItem>, IReadOnlyList<TItem>?>(
                nameof(Items),
                o => o.Items,
                (o, v) => o.Items = v);

        private static readonly Rect s_invalidViewport = new(double.PositiveInfinity, double.PositiveInfinity, 0, 0);
        private readonly Action<IControl> _recycleElement;
        private readonly Action<IControl, int> _updateElementIndex;
        private int _anchorIndex = -1;
        private IControl? _anchorElement;
        private readonly Controls _children = new();
        private IElementFactory? _elementFactory;
        private ElementFactoryGetArgs? _getArgs;
        private bool _isWaitingForViewportUpdate;
        private IReadOnlyList<TItem>? _items;
        private RealizedElementList _measureElements = new();
        private RealizedElementList _realizedElements = new();
        private ElementFactoryRecycleArgs? _recycleArgs;
        private double _lastEstimatedElementSizeU = 25;

        public TreeDataGridPresenterBase()
        {
            _children.CollectionChanged += OnChildrenChanged;
            _recycleElement = RecycleElement;
            _updateElementIndex = UpdateElementIndex;
            EffectiveViewportChanged += OnEffectiveViewportChanged;
        }

        public IElementFactory? ElementFactory
        {
            get => _elementFactory;
            set => SetAndRaise(ElementFactoryProperty, ref _elementFactory, value);
        }

        public IReadOnlyList<TItem>? Items
        {
            get => _items;
            set
            {
                if (_items != value)
                {
                    if (_items is INotifyCollectionChanged oldIncc)
                        oldIncc.CollectionChanged -= OnItemsCollectionChanged;

                    var oldValue = _items;
                    _items = value;

                    if (_items is INotifyCollectionChanged newIncc)
                        newIncc.CollectionChanged += OnItemsCollectionChanged;

                    RaisePropertyChanged(
                        ItemsProperty,
                        new Optional<IReadOnlyList<TItem>?>(oldValue),
                        new BindingValue<IReadOnlyList<TItem>?>(_items));
                    OnItemsCollectionChanged(null, CollectionExtensions.ResetEvent);
                }
            }
        }

        public IReadOnlyList<IControl?> RealizedElements => _realizedElements.Elements;

        Controls IPanel.Children => _children;

        protected abstract Orientation Orientation { get; }
        protected Rect Viewport { get; private set; } = s_invalidViewport;

        public void BringIntoView(int index)
        {
            if (_items is null || index < 0 || index >= _items.Count)
                return;

            if (GetRealizedElement(index) is IControl element)
            {
                element.BringIntoView();
            }
            else if (this.GetVisualRoot() is ILayoutRoot root)
            {
                // Create and measure the element to be brought into view. Store it in a field so that
                // it can be re-used in the layout pass.
                _anchorElement = GetOrCreateElement(index);
                _anchorElement.Measure(Size.Infinity);
                _anchorIndex = index;

                // Get the expected position of the elment and put it in place.
                var anchorU = GetOrEstimateElementPosition(index);
                var rect = Orientation == Orientation.Horizontal ?
                    new Rect(anchorU, 0, _anchorElement.DesiredSize.Width, _anchorElement.DesiredSize.Height) :
                    new Rect(0, anchorU, _anchorElement.DesiredSize.Width, _anchorElement.DesiredSize.Height);
                _anchorElement.Arrange(rect);

                // If the item being brought into view was added since the last layout pass then
                // our bounds won't be updated, so any containing scroll viewers will not have an
                // updated extent. Do a layout pass to ensure that the containing scroll viewers
                // will be able to scroll the new item into view.
                if (!Bounds.Contains(rect) && !Viewport.Contains(rect))
                {
                    _isWaitingForViewportUpdate = true;
                    root.LayoutManager.ExecuteLayoutPass();
                    _isWaitingForViewportUpdate = false;
                }

                // Try to bring the item into view and do a layout pass.
                _anchorElement.BringIntoView();

                _isWaitingForViewportUpdate = !Viewport.Contains(rect);
                root.LayoutManager.ExecuteLayoutPass();
                _isWaitingForViewportUpdate = false;

                _anchorElement = null;
                _anchorIndex = -1;
            }
        }

        public IControl? TryGetElement(int index) => GetRealizedElement(index);

        internal void RecycleAllElements() => _realizedElements.RecycleAllElements(_recycleElement);

        protected virtual Rect ArrangeElement(int index, IControl element, Rect rect)
        {
            element.Arrange(rect);
            return rect;
        }

        protected virtual Size MeasureElement(int index, IControl element, Size availableSize)
        {
            element.Measure(availableSize);
            return element.DesiredSize;
        }

        /// <summary>
        /// Gets the initial constraint for the first pass of the two-pass measure.
        /// </summary>
        /// <param name="element">The element being measured.</param>
        /// <param name="index">The index of the element.</param>
        /// <param name="availableSize">The available size.</param>
        /// <returns>The measure constraint for the element.</returns>
        /// <remarks>
        /// The measure pass is split into two parts:
        /// 
        /// - The initial pass is used to determine the "natural" size of the elements. In this
        ///   pass, infinity can be used as the measure constraint if the element has no other
        ///   constraints on its size.
        /// - The final pass is made once the "natural" sizes of the elements are known and any
        ///   layout logic has been run. This pass is needed because controls should not be 
        ///   arranged with a size less than that passed as the constraint during the measure
        ///   pass. This pass is only run if <see cref="InitialMeasurePassComplete"/> returns
        ///   true.
        /// </remarks>
        protected virtual Size GetInitialConstraint(
            IControl element,
            int index,
            Size availableSize)
        {
            return availableSize;
        }

        /// <summary>
        /// Called when the initial pass of the two-pass measure has been completed, in order to determine
        /// whether a final measure pass is necessary.
        /// </summary>
        /// <param name="firstIndex">The index of the first element in <paramref name="elements"/>.</param>
        /// <param name="elements">The elements being measured.</param>
        /// <returns>
        /// true if a final pass should be run; otherwise false.
        /// </returns>
        /// <see cref="GetInitialConstraint(IControl, int, Size)"/>
        protected virtual bool NeedsFinalMeasurePass(
            int firstIndex,
            IReadOnlyList<IControl?> elements) => false;

        /// <summary>
        /// Gets the final constraint for the second pass of the two-pass measure.
        /// </summary>
        /// <param name="element">The element being measured.</param>
        /// <param name="index">The index of the element.</param>
        /// <param name="availableSize">The available size.</param>
        /// <returns>
        /// The measure constraint for the element.
        /// </returns>
        /// <see cref="GetInitialConstraint(IControl, int, Size)"/>
        protected virtual Size GetFinalConstraint(
            IControl element,
            int index,
            Size availableSize)
        {
            return element.DesiredSize;
        }

        protected virtual IControl GetElementFromFactory(TItem item, int index)
        {
            return GetElementFromFactory(item!, index, this);
        }

        protected IControl GetElementFromFactory(object data, int index, IControl parent)
        {
            _getArgs ??= new ElementFactoryGetArgs();
            _getArgs.Data = data;
            _getArgs.Index = index;
            _getArgs.Parent = parent;

            var result = _elementFactory!.GetElement(_getArgs);
            _getArgs.Data = null;
            _getArgs.Parent = null;
            return result;
        }

        protected virtual (int index, double position) GetElementAt(double position) => (-1, -1);
        protected virtual double GetElementPosition(int index) => -1;
        protected abstract void RealizeElement(IControl element, TItem item, int index);
        protected abstract void UpdateElementIndex(IControl element, int index);
        protected abstract void UnrealizeElement(IControl element);

        protected virtual double CalculateSizeU(Size availableSize)
        {
            if (Items is null)
                return 0;

            // Return the estimated size of all items based on the elements currently realized.
            return EstimateElementSizeU() * Items.Count;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (!IsEffectivelyVisible)
                return default;

            if (Items is null || Items.Count == 0)
            {
                _children.Clear();
                return default;
            }

            // If we're bringing an item into view, ignore any layout passes until we receive a new
            // effective viewport.
            if (_isWaitingForViewportUpdate)
            {
                var sizeV = Orientation == Orientation.Horizontal ? DesiredSize.Height : DesiredSize.Width;
                return CalculateDesiredSize(availableSize, sizeV);
            }

            // We handle horizontal and vertical layouts here so X and Y are abstracted to:
            // - Horizontal layouts: U = horizontal, V = vertical
            // - Vertical layouts: U = vertical, V = horizontal
            var viewport = CalculateMeasureViewport();

            // Recycle elements outside of the expected range.
            RecycleElementsBefore(viewport.firstIndex);
            RecycleElementsAfter(viewport.lastIndex);

            // Do the measure, creating/recycling elements as necessary to fill the viewport. Don't
            // write to _realizedElements yet, only _measureElements.
            GenerateElements(availableSize, ref viewport);

            // Now we know what definitely fits, recycle anything left over.
            RecycleElementsAfter(_measureElements.LastModelIndex);

            // Run the final measure pass if necessary.
            if (NeedsFinalMeasurePass(_measureElements.FirstModelIndex, _measureElements.Elements))
            {
                var count = _measureElements.Count;

                for (var i = 0; i < count; ++i)
                {
                    var e = _measureElements.Elements[i]!;
                    var previous = ((ILayoutable)e).PreviousMeasure!.Value;

                    if (HasInfinity(previous))
                    {
                        var index = _measureElements.FirstModelIndex + i;
                        var constraint = GetFinalConstraint(e, index, availableSize);
                        e.Measure(constraint);
                        viewport.measuredV = Math.Max(
                            viewport.measuredV, 
                            Orientation == Orientation.Horizontal ? 
                                e.DesiredSize.Height : e.DesiredSize.Width);
                    }
                }
            }

            // And swap the measureElements and realizedElements collection.
            (_measureElements, _realizedElements) = (_realizedElements, _measureElements);
            _measureElements.ResetForReuse();

            if (_children.Count > _realizedElements.Elements.Count && _realizedElements.Elements.Count > 0 && _realizedElements.Count == Items.Count)
            {
                for (var i = _children.Count - 1; i >= _realizedElements.Elements.Count; i--)
                {
                    if (!_realizedElements.Elements.Contains(_children.ElementAt(i)))
                    {
                        _children.RemoveAt(i);
                    }
                }
            }

            return CalculateDesiredSize(availableSize, viewport.measuredV);
        }

        private void GenerateElements(Size availableSize, ref MeasureViewport viewport)
        {
            _ = Items ?? throw new AvaloniaInternalException("Items may not be null.");

            var horizontal = Orientation == Orientation.Horizontal;
            var index = viewport.firstIndex;
            var u = viewport.startU;

            // The layout is likely invalid. Don't create any elements and instead rely on our previous
            // element size estimates to calculate a new desired size and trigger a new layout pass.
            if (index >= Items.Count)
                return;

            do
            {
                var e = GetOrCreateElement(index);
                var constraint = GetInitialConstraint(e, index, availableSize);
                var slot = MeasureElement(index, e, constraint);
                var sizeU = horizontal ? slot.Width : slot.Height;

                _measureElements.Add(index, e, u, sizeU);
                viewport.measuredV = Math.Max(viewport.measuredV, horizontal ? slot.Height : slot.Width);

                u += sizeU;
                ++index;
            } while (u < viewport.viewportUEnd && index < Items.Count);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var orientation = Orientation;
            var u = _realizedElements.StartU;

            for (var i = 0; i < _realizedElements.Count; ++i)
            {
                var e = _realizedElements.Elements[i];

                if (e is object)
                {
                    var sizeU = _realizedElements.SizeU[i];
                    Debug.Assert(!double.IsNaN(sizeU));
                    
                    var rect = orientation == Orientation.Horizontal ?
                        new Rect(u, 0, sizeU, finalSize.Height) :
                        new Rect(0, u, finalSize.Width, sizeU);
                    rect = ArrangeElement(i + _realizedElements.FirstModelIndex, e, rect);
                    u += orientation == Orientation.Horizontal ? rect.Width : rect.Height;
                }
            }

            return finalSize;
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromLogicalTree(e);
            RecycleAllElements();
        }

        protected virtual void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
        {
            Viewport = e.EffectiveViewport;
            _isWaitingForViewportUpdate = false;
            InvalidateMeasure();
        }

        private Size CalculateDesiredSize(Size availableSize, double sizeV)
        {
            var sizeU = CalculateSizeU(availableSize);

            if (double.IsInfinity(sizeU) || double.IsNaN(sizeU))
                throw new InvalidOperationException("Invalid calculated size.");

            return Orientation == Orientation.Horizontal ?
                new Size(sizeU, sizeV) :
                new Size(sizeV, sizeU);
        }

        private MeasureViewport CalculateMeasureViewport()
        {
            // If the control has not yet been laid out then the effective viewport won't have been set.
            // Try to work it out from an ancestor control.
            var viewport = Viewport != s_invalidViewport ? Viewport : EstimateViewport();

            // Get the viewport in the orientation direction.
            var viewportStart = Orientation == Orientation.Horizontal ? viewport.X : viewport.Y;
            var viewportEnd = Orientation == Orientation.Horizontal ? viewport.Right : viewport.Bottom;

            var (firstIndex, firstIndexU) = GetOrCalculateElementAt(viewportStart);
            var (lastIndex, _) = GetOrCalculateElementAt(viewportEnd);
            var estimatedElementSize = -1.0;

            if (firstIndex == -1)
            {
                estimatedElementSize = EstimateElementSizeU();
                firstIndex = (int)(viewportStart / estimatedElementSize);
                firstIndexU = firstIndex * estimatedElementSize;
            }

            if (lastIndex == -1)
            {
                if (estimatedElementSize == -1)
                    estimatedElementSize = EstimateElementSizeU();
                lastIndex = (int)(viewportEnd / estimatedElementSize);
            }

            return new MeasureViewport
            {
                firstIndex = firstIndex,
                lastIndex = lastIndex,
                viewportUStart = viewportStart,
                viewportUEnd = viewportEnd,
                startU = firstIndexU,
            };
        }

        private (int index, double position) GetOrCalculateElementAt(double position)
        {
            var (i, p) = GetElementAt(position);
            return i >= 0 ? (i, p) : _realizedElements.GetModelIndexAt(position);
        }

        private IControl GetOrCreateElement(int index)
        {
            var e = GetRealizedElement(index) ?? GetRecycledOrCreateElement(index);
            InvalidateHack(e);
            return e;
        }

        private double GetOrEstimateElementPosition(int index)
        {
            var u = GetElementPosition(index);

            if (u >= 0)
                return u;

            var estimatedElementSize = EstimateElementSizeU();
            return index * estimatedElementSize;
        }

        private IControl? GetRealizedElement(int index)
        {
            if (_anchorIndex == index)
                return _anchorElement;
            return _realizedElements.GetElement(index);
        }

        private IControl GetRecycledOrCreateElement(int index)
        {
            var item = Items![index];
            var e = GetElementFromFactory(item, index);
            e.IsVisible = true;
            RealizeElement(e, item, index);
            if (e.Parent is null)
                _children.Add(e);
            return e;
        }

        private double EstimateElementSizeU()
        {
            var result = _realizedElements.EstimateElementSizeU();
            if (result >= 0)
                _lastEstimatedElementSizeU = result;
            return _lastEstimatedElementSizeU;    
        }

        private Rect EstimateViewport()
        {
            var c = this.GetVisualParent();
            var viewport = new Rect();

            if (c is null)
            {
                return viewport;
            }

            while (c is not null)
            {
                if (!c.Bounds.IsEmpty && c.TransformToVisual(this) is Matrix transform)
                {
                    viewport = new Rect(0, 0, c.Bounds.Width, c.Bounds.Height)
                        .TransformToAABB(transform);
                    break;
                }

                c = c?.GetVisualParent();
            }


            return viewport;
        }

        private void RecycleElement(IControl element)
        {
            UnrealizeElement(element);
            element.IsVisible = false;

            // Hackfix for https://github.com/AvaloniaUI/Avalonia/issues/7553
            element.Measure(Size.Empty);

            _recycleArgs ??= new ElementFactoryRecycleArgs();
            _recycleArgs.Element = element;
            _recycleArgs.Parent = this;
            ElementFactory!.RecycleElement(_recycleArgs);
            _recycleArgs.Element = null;
            _recycleArgs.Parent = null;
        }

        private void RecycleElementsAfter(int index)
        {
            _realizedElements.RecycleElementsAfter(index, _recycleElement);
        }

        private void RecycleElementsBefore(int index)
        {
            _realizedElements.RecycleElementsBefore(index, _recycleElement);
        }

        private void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            void Add(IList items)
            {
                foreach (var i in items)
                {
                    if (i is IControl c)
                    {
                        LogicalChildren.Add(c);
                        VisualChildren.Add(c);
                    }
                }
            }

            void Remove(IList items)
            {
                foreach (var i in items)
                {
                    if (i is IControl c)
                    {
                        LogicalChildren.Remove(c);
                        VisualChildren.Remove(c);
                    }
                }
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(e.NewItems!);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    Remove(e.OldItems!);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    Remove(e.OldItems!);
                    Add(e.NewItems!);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException();
            }
        }

        private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _realizedElements.ItemsInserted(e.NewStartingIndex, e.NewItems!.Count, _updateElementIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    _realizedElements.ItemsRemoved(e.OldStartingIndex, e.OldItems!.Count, _updateElementIndex, _recycleElement);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    RecycleAllElements();
                    break;
            }

            InvalidateMeasure();
        }

        private static void InvalidateHack(IControl c)
        {
            bool HasInvalidations(IControl c)
            {
                if (!c.IsMeasureValid)
                    return true;

                for (var i = 0; i < c.VisualChildren.Count; ++i)
                {
                    if (c.VisualChildren[i] is IControl child)
                    {
                        if (!child.IsMeasureValid || HasInvalidations(child))
                            return true;
                    }
                }

                return false;
            }

            void Invalidate(IControl c)
            {
                c.InvalidateMeasure();
                for (var i = 0; i < c.VisualChildren.Count; ++i)
                {
                    if (c.VisualChildren[i] is IControl child)
                        Invalidate(child);
                }
            }

            if (HasInvalidations(c))
                Invalidate(c);
        }

        private static bool HasInfinity(Size s) => double.IsInfinity(s.Width) || double.IsInfinity(s.Height);

        private struct MeasureViewport
        {
            public int firstIndex;
            public int lastIndex;
            public double viewportUStart;
            public double viewportUEnd;
            public double measuredV;
            public double startU;
        }
    }
}
