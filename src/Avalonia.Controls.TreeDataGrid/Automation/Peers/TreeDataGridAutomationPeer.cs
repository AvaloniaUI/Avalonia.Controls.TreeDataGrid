using Avalonia.Automation.Peers;

namespace Avalonia.Controls.Automation.Peers;

public class TreeDataGridAutomationPeer : ControlAutomationPeer
{
    public TreeDataGridAutomationPeer(TreeDataGrid owner)
        : base(owner)
    {
    }

    public new TreeDataGrid Owner => (TreeDataGrid)base.Owner;

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.DataGrid;
    }
}
