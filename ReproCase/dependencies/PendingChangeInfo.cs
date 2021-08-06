using System;
using System.Collections.Generic;
using System.Reflection;

using UiAvalonia.Table;

namespace PlasticGui.WorkspaceWindow.PendingChanges
{
    public interface ITreeViewNode
    {
        ITreeViewNode GetParent();
        int GetChildrenCount();
        ITreeViewNode GetChild(int position);
        int GetChildPosition(ITreeViewNode child);
    }

    public class PendingChangeInfo :
        ITreeViewNode,
        IPlasticTreeNode,
        Filter.IFilterableRow
    {
        public ChangeInfo ChangeInfo { get { return mChange; } }

        public PendingChangeInfo(
            PendingChangeCategory changeCategory,
            ChangeInfo changeInfo)
        {
            mChangeCategory = changeCategory;
            mChange = changeInfo;
        }

        int IPlasticTreeNode.GetChildrenCount()
        {
            return 0;
        }

        IPlasticTreeNode IPlasticTreeNode.GetParent()
        {
            return mChangeCategory;
        }

        List<IPlasticTreeNode> IPlasticTreeNode.GetChildren()
        {
            return null;
        }

        public ITreeViewNode GetParent()
        {
            return mChangeCategory;
        }

        public int GetChildrenCount()
        {
            return 0;
        }

        public ITreeViewNode GetChild(int position)
        {
            return null;
        }

        public int GetChildPosition(ITreeViewNode child)
        {
            return 0;
        }

        public string GetColumnText(string columnName)
        {
            return columnName;
        }

        public string GetItemString()
        {
            if (string.IsNullOrEmpty(mItemString))
                mItemString = Guid.NewGuid().ToString();

            return mItemString;
        }

        string mItemString;

        ChangeInfo mChange;
        PendingChangeCategory mChangeCategory;
    }
}