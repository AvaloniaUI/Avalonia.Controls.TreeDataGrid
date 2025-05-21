using Avalonia.Automation.Peers;
using Avalonia.Controls.Primitives;

namespace Avalonia.Controls.Automation.Peers;

public class TreeDataGridCellAutomationPeer : ControlAutomationPeer
{
    public TreeDataGridCellAutomationPeer(TreeDataGridCell owner)
        : base(owner)
    {
    }

    public new TreeDataGridCell Owner => (TreeDataGridCell)base.Owner;

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Custom;
    }

    protected override bool IsContentElementCore() => true;

    protected override bool IsControlElementCore() => true;
}
