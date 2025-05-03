using Avalonia.Automation.Peers;
using Avalonia.Controls.Primitives;

namespace Avalonia.Controls.Automation.Peers;

public class TreeDataGridRowAutomationPeer : ControlAutomationPeer
{
    public TreeDataGridRowAutomationPeer(TreeDataGridRow owner)
        : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.DataItem;
    }

    protected override bool IsContentElementCore() => true;

    protected override bool IsControlElementCore() => true;
}
