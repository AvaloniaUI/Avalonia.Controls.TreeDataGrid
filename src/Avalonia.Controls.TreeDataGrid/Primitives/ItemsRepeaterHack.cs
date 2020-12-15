using Avalonia.Layout;

namespace Avalonia.Controls.Primitives
{
    public class ItemsRepeaterHack : ItemsRepeater, ILayoutable
    {
        void ILayoutable.ChildDesiredSizeChanged(ILayoutable control)
        {
            // HACK: Don't invalidate measure if the child whose size changed is in the recycle
            // pool. This needs to be implemented on ItemsRepeater itself.
            if (GetElementIndex((IControl)control) >= 0)
                InvalidateMeasure();
        }
    }
}
