using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Utilities;
using Avalonia.VisualTree;
using CollectionExtensions = Avalonia.Controls.Models.TreeDataGrid.CollectionExtensions;

namespace Avalonia.Controls.Primitives
{
    public abstract class TreeDataGridPresenterBase<TItem> : Border, IPresenter
    {
#pragma warning disable AVP1002
        public static readonly DirectProperty<TreeDataGridPresenterBase<TItem>, TreeDataGridElementFactory?>
            ElementFactoryProperty =
                AvaloniaProperty.RegisterDirect<TreeDataGridPresenterBase<TItem>, TreeDataGridElementFactory?>(
                    nameof(ElementFactory),
                    o => o.ElementFactory,
                    (o, v) => o.ElementFactory = v);

        public static readonly DirectProperty<TreeDataGridPresenterBase<TItem>, IReadOnlyList<TItem>?> ItemsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridPresenterBase<TItem>, IReadOnlyList<TItem>?>(
                nameof(Items),
                o => o.Items,
                (o, v) => o.Items = v);
#pragma warning restore AVP1002
        private static readonly Rect s_invalidViewport = new(double.PositiveInfinity, double.PositiveInfinity, 0, 0);
        private readonly Action<Control> _recycleElement;
        private readonly Action<Control, int> _updateElementIndex;
        private int _anchorIndex = -1;
        private Control? _anchorElement;
        private TreeDataGridElementFactory? _elementFactory;
        private bool _isWaitingForViewportUpdate;
        private IReadOnlyList<TItem>? _items;
        private RealizedElementList _measureElements = new();
        private RealizedElementList _realizedElements = new();
        private double _lastEstimatedElementSizeU = 25;

        public TreeDataGridPresenterBase()
        {
            _recycleElement = RecycleElement;
            _updateElementIndex = UpdateElementIndex;
            EffectiveViewportChanged += OnEffectiveViewportChanged;
        }

        public TreeDataGridElementFactory? ElementFactory
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
                        oldValue,
                        _items);
                    OnItemsCollectionChanged(null, CollectionExtensions.ResetEvent);
                }
            }
        }

        public IReadOnlyList<Control?> RealizedElements => _realizedElements.Elements;

        protected abstract Orientation Orientation { get; }
        protected Rect Viewport { get; private set; } = s_invalidViewport;

        public void BringIntoView(int index, Rect? rect = null)
        {
            if (_items is null || index < 0 || index >= _items.Count)
                return;

            if (GetRealizedElement(index) is Control element)
            {
                if (rect.HasValue)
                    element.BringIntoView(rect.Value);
                else
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
                var elementRect = Orientation == Orientation.Horizontal ?
                    new Rect(anchorU, 0, _anchorElement.DesiredSize.Width, _anchorElement.DesiredSize.Height) :
                    new Rect(0, anchorU, _anchorElement.DesiredSize.Width, _anchorElement.DesiredSize.Height);
                _anchorElement.Arrange(elementRect);

                // If the item being brought into view was added since the last layout pass then
                // our bounds won't be updated, so any containing scroll viewers will not have an
                // updated extent. Do a layout pass to ensure that the containing scroll viewers
                // will be able to scroll the new item into view.
                if (!Bounds.Contains(elementRect) && !Viewport.Contains(elementRect))
                {
                    _isWaitingForViewportUpdate = true;
                    root.LayoutManager.ExecuteLayoutPass();
                    _isWaitingForViewportUpdate = false;
                }

                // Try to bring the item into view and do a layout pass.
                if (rect.HasValue)
                    _anchorElement.BringIntoView(rect.Value);
                else
                    _anchorElement.BringIntoView();

                _isWaitingForViewportUpdate = !Viewport.Contains(elementRect);
                root.LayoutManager.ExecuteLayoutPass();
                _isWaitingForViewportUpdate = false;

                _anchorElement = null;
                _anchorIndex = -1;
            }
        }

        public Control? TryGetElement(int index) => GetRealizedElement(index);

        internal void RecycleAllElements() => _realizedElements.RecycleAllElements(_recycleElement);

        protected virtual Rect ArrangeElement(int index, Control element, Rect rect)
        {
            element.Arrange(rect);
            return rect;
        }

        protected virtual Size MeasureElement(int index, Control element, Size availableSize)
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
            Control element,
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
        /// <see cref="GetInitialConstraint(Control, int, Size)"/>
        protected virtual bool NeedsFinalMeasurePass(
            int firstIndex,
            IReadOnlyList<Control?> elements) => false;

        /// <summary>
        /// Gets the final constraint for the second pass of the two-pass measure.
        /// </summary>
        /// <param name="element">The element being measured.</param>
        /// <param name="index">The index of the element.</param>
        /// <param name="availableSize">The available size.</param>
        /// <returns>
        /// The measure constraint for the element. The constraint must not contain infinity values.
        /// </returns>
        /// <see cref="GetInitialConstraint(Control, int, Size)"/>
        protected virtual Size GetFinalConstraint(
            Control element,
            int index,
            Size availableSize)
        {
            return element.DesiredSize;
        }

        protected virtual Control GetElementFromFactory(TItem item, int index)
        {
            return GetElementFromFactory(item!, index, this);
        }

        protected Control GetElementFromFactory(object data, int index, Control parent)
        {
            return _elementFactory!.GetOrCreateElement(data, parent);
        }

        protected virtual (int index, double position) GetElementAt(double position) => (-1, -1);
        protected virtual double GetElementPosition(int index) => -1;
        protected abstract void RealizeElement(Control element, TItem item, int index);
        protected abstract void UpdateElementIndex(Control element, int index);
        protected abstract void UnrealizeElement(Control element);

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
                TrimUnrealizedChildren();
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
            RecycleElementsAfter(viewport.estimatedLastIndex);

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
                    var e = _measureElements.Elements[i];
                    if (e is not null)
                    {
                        var previous = LayoutInformation.GetPreviousMeasureConstraint(e)!.Value;
                        if (HasInfinity(previous))
                        {
                            var index = _measureElements.FirstModelIndex + i;
                            var constraint = GetFinalConstraint(e, index, availableSize);
                            e.Measure(constraint);
                        }
                    }
                }
            }

            // And swap the measureElements and realizedElements collection.
            (_measureElements, _realizedElements) = (_realizedElements, _measureElements);
            _measureElements.ResetForReuse();

            TrimUnrealizedChildren();

            return CalculateDesiredSize(availableSize, viewport.measuredV);
        }

        private void GenerateElements(Size availableSize, ref MeasureViewport viewport)
        {
            _ = Items ?? throw new AvaloniaInternalException("Items may not be null.");

            var horizontal = Orientation == Orientation.Horizontal;
            var index = viewport.firstIndex;
            var u = viewport.startU;

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

            var (firstIndex, firstIndexU) = GetElementAt(viewportStart);
            var (lastIndex, _) = GetElementAt(viewportEnd);
            var estimatedElementSize = -1.0;
            var itemCount = Items?.Count ?? 0;

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
                firstIndex = MathUtilities.Clamp(firstIndex, 0, itemCount - 1),
                estimatedLastIndex = MathUtilities.Clamp(lastIndex, 0, itemCount - 1),
                viewportUStart = viewportStart,
                viewportUEnd = viewportEnd,
                startU = firstIndexU,
            };
        }

        private Control GetOrCreateElement(int index)
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

        private Control? GetRealizedElement(int index)
        {
            if (_anchorIndex == index)
                return _anchorElement;
            return _realizedElements.GetElement(index);
        }

        private Control GetRecycledOrCreateElement(int index)
        {
            var item = Items![index];
            var e = GetElementFromFactory(item, index);
            e.IsVisible = true;
            RealizeElement(e, item, index);
            if (e.GetVisualParent() is null)
            {
                ((ISetLogicalParent)e).SetParent(this);
                VisualChildren.Add(e);
            }
            return e;
        }

        private double EstimateElementSizeU()
        {
            var count = _realizedElements.Count;
            var divisor = 0.0;
            var total = 0.0;

            for (var i = 0; i < count; ++i)
            {
                if (_realizedElements.Elements[i] is object)
                {
                    total += _realizedElements.SizeU[i];
                    ++divisor;
                }
            }

            if (divisor == 0 || total == 0)
                return _lastEstimatedElementSizeU;

            _lastEstimatedElementSizeU = total / divisor;
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
                if (!c.Bounds.Equals(default) && c.TransformToVisual(this) is Matrix transform)
                {
                    viewport = new Rect(0, 0, c.Bounds.Width, c.Bounds.Height)
                        .TransformToAABB(transform);
                    break;
                }

                c = c?.GetVisualParent();
            }


            return viewport;
        }

        private void RecycleElement(Control element)
        {
            UnrealizeElement(element);
            element.IsVisible = false;

            // Hackfix for https://github.com/AvaloniaUI/Avalonia/issues/7552
            element.Measure(default);

            ElementFactory!.RecycleElement(element);
        }

        private void RecycleElementsAfter(int index)
        {
            _realizedElements.RecycleElementsAfter(index, _recycleElement);
        }

        private void RecycleElementsBefore(int index)
        {
            _realizedElements.RecycleElementsBefore(index, _recycleElement);
        }

        private void TrimUnrealizedChildren()
        {
            var count = Items?.Count ?? 0;
            var children = VisualChildren;

            if (children.Count > _realizedElements.Elements.Count && _realizedElements.Count == count)
            {
                for (var i = children.Count - 1; i >= _realizedElements.Elements.Count; i--)
                {
                    if (!_realizedElements.Elements.Contains(children.ElementAt(i)))
                    {
                        children.RemoveAt(i);
                    }
                }
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
                    _realizedElements.ItemsRemoved(e.OldStartingIndex, e.OldItems!.Count, _updateElementIndex,
                        _recycleElement);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    RecycleAllElements();
                    break;
            }

            InvalidateMeasure();
        }

        private static void InvalidateHack(Control c)
        {
            bool HasInvalidations(Control c)
            {
                if (!c.IsMeasureValid)
                    return true;

                // TODO: not sure if this is correct.
                foreach (var visualChild in c.GetVisualChildren())
                {
                    if (visualChild is Control child && (!child.IsMeasureValid || HasInvalidations(child)))
                        return true;
                }

                return false;
            }

            void Invalidate(Control c)
            {
                c.InvalidateMeasure();

                // TODO: double check again.
                foreach (var visualChild in c.GetVisualChildren())
                {
                    if (visualChild is Control child)
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
            public int estimatedLastIndex;
            public double viewportUStart;
            public double viewportUEnd;
            public double measuredV;
            public double startU;
        }
    }
}
