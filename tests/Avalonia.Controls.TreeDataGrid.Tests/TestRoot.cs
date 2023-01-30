using System;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Styling;

namespace Avalonia.Controls.TreeDataGridTests
{
    public class TestRoot : Decorator, IFocusScope, ILayoutRoot, IInputRoot, IRenderRoot, IStyleHost, ILogicalRoot
    {
        public TestRoot()
        {
            LayoutManager = new LayoutManager(this);
            IsVisible = true;
        }

        public TestRoot(Control child)
            : this()
        {
            Child = child;
        }

        public Size ClientSize { get; set; } = new Size(100, 100);
        public Size MaxClientSize { get; set; } = Size.Infinity;
        public double LayoutScaling { get; set; } = 1;
        public ILayoutManager LayoutManager { get; set; }
        public double RenderScaling => 1;
#pragma warning disable CS8766
        public IRenderer? Renderer { get; set; }
#pragma warning restore CS8766
        public IAccessKeyHandler AccessKeyHandler => throw new NotImplementedException();
        public IKeyboardNavigationHandler KeyboardNavigationHandler => throw new NotImplementedException();
        public IInputElement? PointerOverElement { get; set; }
        public IMouseDevice? MouseDevice { get; set; }
        public bool ShowAccessKeys { get; set; }
        public IStyleHost? StylingParent { get; set; }
        public IRenderTarget CreateRenderTarget() => throw new NotImplementedException();
        public void Invalidate(Rect rect) { }
        public Point PointToClient(PixelPoint p) => p.ToPoint(1);
        public PixelPoint PointToScreen(Point p) => PixelPoint.FromPoint(p, 1);

        protected override Size MeasureOverride(Size availableSize)
        {
            return ClientSize;
        }
    }
}
