using System;
using System.Collections.Generic;
using System.Linq;

using UiAvalonia.Table;

namespace PlasticGui.WorkspaceWindow.PendingChanges
{
    public class ChangeInfo
    {

    }

    public class PendingChangesTree : IPlasticTree
    {
        public List<PendingChangeCategory> GetNodes()
        {
            if (mFilteredCategories != null)
                return mFilteredCategories;

            return mCategories;
        }

        public void GetCheckedChanges(
            bool bExcludePrivates, out List<ChangeInfo> changes, out List<ChangeInfo> dependenciesCandidates)
        {
            changes = new List<ChangeInfo>();
            dependenciesCandidates = new List<ChangeInfo>();

            List<ChangeInfo> changesCategory;
            List<ChangeInfo> dependenciesCategory;

            foreach (PendingChangeCategory category in mCategories)
            {
                /*category.GetCheckedChanges(
                    bExcludePrivates, out changesCategory, out dependenciesCategory);

                changes.AddRange(changesCategory);
                dependenciesCandidates.AddRange(dependenciesCategory);*/
            }
        }

        public List<ChangeInfo> GetDependenciesCandidates(List<ChangeInfo> selectedChanges,
            bool bExcludePrivates)
        {
            List<ChangeInfo> dependenciesCandidates = new List<ChangeInfo>();

            foreach (PendingChangeCategory category in mCategories)
            {
                /*dependenciesCandidates.AddRange(
                    category.GetDependenciesCandidates(
                        selectedChanges, bExcludePrivates));*/
            }

            return dependenciesCandidates;
        }

        public List<ChangeInfo> GetAllChanges()
        {
            List<ChangeInfo> result = new List<ChangeInfo>();

            foreach (PendingChangeCategory category in GetNodes())
            {
                foreach (PendingChangeInfo change in category.GetCurrentChanges())
                    result.Add(change.ChangeInfo);
            }
            return result;
        }

        public int GetChangesCount()
        {
            int result = 0;
            List<PendingChangeCategory> categories = GetNodes();

            if (categories == null)
                return result;

            foreach (PendingChangeCategory category in categories)
            {
                result += category.GetCurrentChanges().Count;
            }
            return result;
        }

        public void Sort(string key, bool bAscending)
        {
            /*int ini = Environment.TickCount;

            foreach (PendingChangeCategory category in mCategories)
            {
                IComparer<PendingChangeInfo> comparer = category.GetChildrenCount() > 10000 ?
                    GetFastComparer(key, bAscending) : GetComparer(key, bAscending);

                category.SortChildren(comparer);
            }*/


        }

        public void Filter(Filter filter, List<string> columnNames)
        {
            mFilteredCategories = new List<PendingChangeCategory>();

            foreach (PendingChangeCategory category in mCategories)
            {
                category.ApplyFilter(filter, columnNames);

                if (category.GetChildrenCount() > 0)
                    mFilteredCategories.Add(category);
            }
        }

        public void BuildChangeCategories(
            string wkPath, PendingChanges pendingChanges)
        {
            mCategories.Clear();
            mFilteredCategories = null;

            if (pendingChanges.Changed.Count != 0)
            {
                mCategories.Add(BuildChangeCategory(
                    wkPath,
                    pendingChanges.Changed,
                    PendingChangeCategoryType.Changed,
                    "ChangedItemsSelectedSingular",
                    "ChangedItemsSelectedPlural"));
            }

            if (pendingChanges.Deleted.Count != 0)
            {
                mCategories.Add(BuildChangeCategory(
                    wkPath,
                    pendingChanges.Deleted,
                    PendingChangeCategoryType.Deleted,
                    "DeletedItemsSelectedSingular",
                    "DeletedItemsSelectedPlural"));
            }

            if (pendingChanges.Moved.Count != 0)
            {
                mCategories.Add(BuildChangeCategory(
                    wkPath,
                    pendingChanges.Moved,
                    PendingChangeCategoryType.Moved,
                    "MovedItemsSelectedSingular",
                    "MovedItemsSelectedPlural"));
            }

            if (pendingChanges.Added.Count != 0)
            {
                mCategories.Add(BuildChangeCategory(
                    wkPath,
                    pendingChanges.Added,
                    PendingChangeCategoryType.Added,
                    "AddedPrivateItemsSelectedSingular",
                    "AddedPrivateItemsSelectedPlural"));
            }
        }

        int IPlasticTree.GetNodesCount()
        {
            return GetNodes().Count;
        }

        List<IPlasticTreeNode> IPlasticTree.GetNodes()
        {
            return GetNodes().Cast<IPlasticTreeNode>().ToList();
        }

        void IPlasticTree.Filter(Filter filter, List<string> columnNames)
        {
            Filter(filter, columnNames);
        }

        PendingChangeCategory BuildChangeCategory(
            string wkPath,
            List<ChangeInfo> changes,
            PendingChangeCategoryType categoryType,
            string categoryNameSingular,
            string categoryNamePlural)
        {
            PendingChangeCategory category = new PendingChangeCategory(
                categoryNameSingular,
                categoryNamePlural,
                categoryType);

            List<PendingChangeInfo> changesList = new List<PendingChangeInfo>();

            foreach (ChangeInfo change in changes)
            {
                changesList.Add(new PendingChangeInfo(
                    category, change));
            }

            category.SetChanges(changesList);

            return category;
        }

        List<PendingChangeCategory> mCategories = new List<PendingChangeCategory>();
        List<PendingChangeCategory> mFilteredCategories;
    }
}