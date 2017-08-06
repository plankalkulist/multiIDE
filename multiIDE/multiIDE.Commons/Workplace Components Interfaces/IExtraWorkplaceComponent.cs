using System;

namespace multiIDE
{
    public interface IExtraWorkplaceComponent : IWorkplaceComponent
    {
        #region Essential properties
        bool IsInitialized { get; }
        object InitializedBy { get; }
        #endregion

        //#region Events
        //event EventHandler Closing;
        //#endregion

        void Initialize(object sender);
    }
}
