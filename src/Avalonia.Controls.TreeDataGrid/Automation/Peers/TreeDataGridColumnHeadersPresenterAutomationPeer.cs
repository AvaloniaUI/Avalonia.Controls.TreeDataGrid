using Avalonia.Automation.Peers;
using Avalonia.Controls.Primitives;

namespace Avalonia.Controls.Automation.Peers;

public class TreeDataGridColumnHeadersPresenterAutomationPeer : ControlAutomationPeer
{
    public TreeDataGridColumnHeadersPresenterAutomationPeer(TreeDataGridColumnHeadersPresenter owner)
        : base(owner)
    {
    }

    public new TreeDataGridColumnHeadersPresenter Owner => (TreeDataGridColumnHeadersPresenter)base.Owner;

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Header;
    }

    protected override bool IsContentElementCore() => false;

    protected override bool IsControlElementCore() => true;
}
