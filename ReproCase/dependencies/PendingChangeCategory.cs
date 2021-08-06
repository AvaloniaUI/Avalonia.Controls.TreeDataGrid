using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UiAvalonia.Table;

namespace PlasticGui.WorkspaceWindow.PendingChanges
{
    public enum PendingChangeCategoryType
    {
        None,
        Changed,
        Added,
        Deleted,
        Moved
    }

    public class PendingChangeCategory :
        ITreeViewNode,
        IPlasticTreeNode
    {
        public PendingChangeCategoryType Type { get { return mType; } }
        //For testing purposes
        public string CategoryNameSingular { get { return mCategoryNameSingular; } }
        public string CategoryNamePlural { get { return mCategoryNamePlural; } }
        public object Tag;

        public PendingChangeCategory(
            string categoryNameSingular,
            string categoryNamePlural,
            PendingChangeCategoryType type)
        {
            mCategoryNameSingular = categoryNameSingular;
            mCategoryNamePlural = categoryNamePlural;
            mType = type;
        }

        int IPlasticTreeNode.GetChildrenCount()
        {
            return GetChildrenCount();
        }

        IPlasticTreeNode IPlasticTreeNode.GetParent()
        {
            return null;
        }

        List<IPlasticTreeNode> IPlasticTreeNode.GetChildren()
        {
            List<IPlasticTreeNode> result = new List<IPlasticTreeNode>();

            foreach (PendingChangeInfo change in GetCurrentChanges())
                result.Add(change);

            return result;
        }

        public ITreeViewNode GetParent()
        {
            return null;
        }

        public int GetChildrenCount()
        {
            return GetCurrentChanges().Count;
        }

        public ITreeViewNode GetChild(int position)
        {
            return (ITreeViewNode)GetCurrentChanges()[position];
        }

        public int GetChildPosition(ITreeViewNode child)
        {
            return mChangesHash[(PendingChangeInfo)child];
        }

        public void SetChanges(List<PendingChangeInfo> changes)
        {
            mChanges = changes;
            mFilteredChanges = null;
            mChangesHash = new Dictionary<PendingChangeInfo, int>();

            UpdateHash();
        }

        public void SortChildren(IComparer<PendingChangeInfo> comparer)
        {
            List<PendingChangeInfo> currentChanges = GetCurrentChanges();

            PendingChangeInfo[] result = currentChanges.ToArray();

            currentChanges.Clear();
            currentChanges.AddRange(result);

            UpdateHash();
        }

        public void ApplyFilter(Filter filter, List<string> columnNames)
        {
            if (filter.IsEmpty)
            {
                mFilteredChanges = null;
                UpdateHash();
                mCheckedElementsCount = -1;
                return;
            }

            mFilteredChanges = new List<PendingChangeInfo>();

            foreach (PendingChangeInfo change in mChanges)
            {
                if (filter.IsMatch(change, columnNames))
                    mFilteredChanges.Add(change);
            }

            UpdateHash();
            mCheckedElementsCount = -1;
        }

        public bool IsChecked()
        {
            int checkedChangesCount = GetCheckedChangesCount();

            if (checkedChangesCount == 0)
                return false;

            return checkedChangesCount == GetCurrentChanges().Count;
        }

        public bool IsPartiallyChecked()
        {
            int checkedChangesCount = GetCheckedChangesCount();

            return checkedChangesCount > 0
                && checkedChangesCount < GetCurrentChanges().Count;
        }

        public void UpdateCheckedState(bool isChecked)
        {
            mCheckedElementsCount = -1;

            foreach (PendingChangeInfo change in GetCurrentChanges())
            {
                //change.UpdateCheckedState(isChecked);
            }

            if (isChecked)
            {
                mCheckedElementsCount = GetCurrentChanges().Count;
                return;
            }

            mCheckedElementsCount = 0;
        }

        public string GetHeaderText()
        {
            int currentChangesCount = GetCurrentChanges().Count;

            return currentChangesCount == 1 ?
                string.Format(mCategoryNameSingular, GetCheckedChangesCount()) :
                string.Format(mCategoryNamePlural, GetCheckedChangesCount(), currentChangesCount);
        }

        /*public void GetCheckedChanges(
            bool bExcludePrivates, out List<ChangeInfo> changes,
            out List<ChangeInfo> dependenciesCandidates)
        {
            changes = new List<ChangeInfo>();
            dependenciesCandidates = new List<ChangeInfo>();

            HashSet<PendingChangeInfo> indexedCurrentChanges =
                new HashSet<PendingChangeInfo>(GetCurrentChanges());

            foreach (PendingChangeInfo change in mChanges)
            {
                if (ChangeInfoType.IsIgnored(change.ChangeInfo))
                    continue;

                if (bExcludePrivates && ChangeInfoType.IsPrivate(change.ChangeInfo))
                    continue;

                if (!change.IsChecked())
                {
                    dependenciesCandidates.Add(change.ChangeInfo);
                    continue;
                }

                if (mFilteredChanges == null)
                {
                    changes.Add(change.ChangeInfo);
                    continue;
                }

                if (indexedCurrentChanges.Contains(change))
                {
                    changes.Add(change.ChangeInfo);
                    continue;
                }

                dependenciesCandidates.Add(change.ChangeInfo);
            }
        }*/

        /*public List<ChangeInfo> GetDependenciesCandidates(
            List<ChangeInfo> selectedChanges, bool bExcludePrivates)
        {
            HashSet<ChangeInfo> indexedSelectedChanges =
                new HashSet<ChangeInfo>(selectedChanges);

            List<ChangeInfo> candidates = new List<ChangeInfo>();

            foreach (PendingChangeInfo currentChange in GetCurrentChanges())
            {
                if (ChangeInfoType.IsIgnored(currentChange.ChangeInfo))
                    continue;

                if (bExcludePrivates && ChangeInfoType.IsPrivate(currentChange.ChangeInfo))
                    continue;

                if (!indexedSelectedChanges.Contains(currentChange.ChangeInfo))
                    candidates.Add(currentChange.ChangeInfo);
            }

            return candidates;
        }*/

        public List<PendingChangeInfo> GetCurrentChanges()
        {
            if (mFilteredChanges != null)
                return mFilteredChanges;
            return mChanges;
        }

        internal void UpdateCheckedElementsCount(bool isChecked)
        {
            if (mCheckedElementsCount == -1)
                return;

            if (isChecked)
            {
                mCheckedElementsCount++;
                return;
            }

            mCheckedElementsCount--;
        }

        public int GetCheckedChangesCount()
        {
            if (mCheckedElementsCount != -1)
                return mCheckedElementsCount;

            mCheckedElementsCount = 0;

            foreach (PendingChangeInfo change in GetCurrentChanges())
            {
                if (false)
                    continue;

                mCheckedElementsCount++;
            }

            return mCheckedElementsCount;
        }

        void UpdateHash()
        {
            int position = 0;

            mChangesHash.Clear();
            foreach (PendingChangeInfo change in GetCurrentChanges())
            {
                mChangesHash.Add(change, position++);
            }
        }

        List<PendingChangeInfo> mChanges;
        List<PendingChangeInfo> mFilteredChanges;
        Dictionary<PendingChangeInfo, int> mChangesHash;

        int mCheckedElementsCount = -1;

        string mCategoryNameSingular;
        string mCategoryNamePlural;
        PendingChangeCategoryType mType;
    }
}