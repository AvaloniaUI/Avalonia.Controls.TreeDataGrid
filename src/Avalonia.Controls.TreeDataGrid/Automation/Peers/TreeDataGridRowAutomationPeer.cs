using System;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;

namespace Avalonia.Controls.Automation.Peers;

public class TreeDataGridRowAutomationPeer : ControlAutomationPeer, IToggleProvider, IValueProvider
{
    public TreeDataGridRowAutomationPeer(TreeDataGridRow owner)
        : base(owner)
    {
    }

    public new TreeDataGridRow Owner => (TreeDataGridRow)base.Owner;

    public ToggleState ToggleState
    {
        get
        {
            if (Owner.DataContext is IExpander expander)
            {
                return expander.IsExpanded ? ToggleState.On : ToggleState.Off;
            }
            return ToggleState.Indeterminate;
        }
    }

    public bool IsReadOnly => true;

    public string? Value
    {
        get
        {
            return Owner.Model?.ToString();
        }
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.TreeItem;
    }

    protected override bool IsContentElementCore() => true;

    protected override bool IsControlElementCore() => true;

    public void Toggle()
    {
        if (Owner.DataContext is IExpander expander)
        {
            expander.IsExpanded = !expander.IsExpanded;
        }
    }

    public void SetValue(string? value)
    {
        throw new InvalidOperationException("TreeDataGrid rows are read-only.");
    }
}
