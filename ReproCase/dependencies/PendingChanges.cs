using System;
using System.Collections.Generic;

namespace PlasticGui.WorkspaceWindow.PendingChanges
{
    public class PendingChanges
    {
        public List<ChangeInfo> Added { get { return mAdded; } }
        public List<ChangeInfo> Deleted { get { return mDeleted; } }
        public List<ChangeInfo> Changed { get { return mChanged; } }
        public List<ChangeInfo> Moved { get { return mMoved; } }

        List<ChangeInfo> mAdded = new List<ChangeInfo>();
        List<ChangeInfo> mDeleted = new List<ChangeInfo>();
        List<ChangeInfo> mChanged = new List<ChangeInfo>();
        List<ChangeInfo> mMoved = new List<ChangeInfo>();


    }
}