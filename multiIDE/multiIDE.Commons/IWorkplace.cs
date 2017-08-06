using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace multiIDE
{
    public interface IWorkplace : IComponent
    {
        #region Essential properties
        IMainForm MainForm { get; }
        List<IIDE> IDEs { get; }
        List<IInputDevice> InputDevices { get; }
        List<IOutputDevice> OutputDevices { get; }
        List<IWorkplaceComponent> ExtraComponents { get; }
        List<IComponent> CommonComponents { get; }
        bool IsInitialized { get; }
        int IdeIdCounter { get; }
        int MachinesIdCounter { get; }
        #endregion

        #region Workplace Component Builders
        IComponentBuilder<IVirtualMachine, VirtualMachineTypeInfo> VirtualMachineBuilder { get; }
        IComponentBuilder<ICodeEditor, CodeEditorTypeInfo> CodeEditorBuilder { get; }
        IComponentBuilder<IInputPort, InputPortTypeInfo> InputPortBuilder { get; }
        IComponentBuilder<IOutputPort, OutputPortTypeInfo> OutputPortBuilder { get; }
        IComponentBuilder<IExtraIdeComponent, ComponentTypeInfo> IdeExtraBuilder { get; }
        IComponentBuilder<IInputDevice, InputDeviceTypeInfo> InputDeviceBuilder { get; }
        IComponentBuilder<IOutputDevice, OutputDeviceTypeInfo> OutputDeviceBuilder { get; }
        IComponentBuilder<IExtraWorkplaceComponent, ComponentTypeInfo> WorkplaceExtraBuilder { get; }
        #endregion

        #region Events
        event EventHandler GotUpdated;
        #endregion

        #region Components managing subs
        bool NewInputDevice(InputDeviceTypeInfo inputDeviceTypeInfo, out IInputDevice newInputDevice);
        bool NewOutputDevice(OutputDeviceTypeInfo outputDeviceTypeInfo, out IOutputDevice newOutputDevice);
        void InitializeNewComponentManager();
        IExtraWorkplaceComponent InitializeNewSettingWindow(IComponent componentToSet);
        IExtraWorkplaceComponent NewExtraWorkplaceComponent(ComponentTypeInfo extraComponentType);
        IComponent NewCommonComponent(ComponentTypeInfo commonComponentType);
        #endregion

        #region Config File subs
        void LoadConfigFile(string fileName);
        void SaveConfigFile();
        void SaveConfigFile(string fileName);
        #endregion

        #region Workplace service subs
        void InitializeWorkplace();
        void OpenWorkplace(string fileName);
        void SaveWorkplace();
        void SaveWorkplace(string fileName);
        Task DisposeWorkplace(bool immediately = false);
        //
        // // Driven by user Workplace subs:
        Task<bool> NewWorkplaceByUser();
        Task<bool> OpenWorkplaceByUser();
        bool SaveWorkplaceByUser();
        bool SaveWorkplaceAsByUser();
        Task<bool> CloseWorkplaceByUser(bool immediately = false);
        #endregion

        #region Application service subs
        Task Exit(bool immediately = false);
        Task<bool> ExitByUser(bool immediately = false);
        #endregion

        #region MainForm service subs
        void MainFormActivate();
        void MainFormSelectIDE(IIDE ide);
        #endregion

        #region IDEs service subs
        IIDE NewIDE(IVirtualMachine machine, ICodeEditor codeEditor
                , IInputPort inputPort = null, int withInputPortIndex = -1
                , IOutputPort outputPort = null, int withOutputPortIndex = -1
                , bool withConsole = false);
        IIDE OpenIDE(string fileName);
        void SaveIDE(IIDE ide);
        void SaveIDEas(IIDE ide, string fileName);
        Task CloseIDE(IIDE ide, bool immediately = false);
        //
        // // Driven by user IDE subs:
        bool NewIDEbyUser(out IIDE newIde);
        bool OpenIDEbyUser(out IIDE ide);
        bool SaveIDEbyUser(IIDE ide);
        bool SaveIDEasByUser(IIDE ide);
        Task<bool> CloseIDEbyUser(IIDE ide, bool immediately = false);
        //
        // // Naming service subs:
        string GetNewProgramFileShortName(IIDE ide);
        string GetNewIDEtitle(IIDE ide);
        #endregion

        #region IO Devices service subs
        void InitializeIOdeviceInIndividualThread(IInputDevice inputDevice);
        void InitializeIOdeviceInIndividualThread(IOutputDevice outputDevice);
        #endregion
    }
}
