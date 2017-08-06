using System;
using System.Collections.Generic;

namespace multiIDE
{
    public interface IMainForm : IWorkplaceComponent
    {
        #region Essential properties
        IIDE SelectedIDE { get; }
        List<string> RecentFiles { get; }
        #endregion

        #region Environment subs
        void SelectIDE(IIDE ide);
        void ParentWorkplace_GotUpdated(object sender, EventArgs ne);
        void SelectedIDE_GotUpdated(object sender, EventArgs ne);
        void AddRecentFile(string fileName);
        void Activate();
        void UpdateTitle();
        #endregion
    }
}
