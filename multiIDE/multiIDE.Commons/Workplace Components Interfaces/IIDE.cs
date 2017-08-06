using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace multiIDE
{
    public interface IIDE : IWorkplaceComponent, IDisposable
    {
        #region Essential properties
        IVirtualMachine Machine { get; set; }
        ICodeEditor CodeEditor { get; set; }
        List<IExtraIdeComponent> ExtraComponents { get; set; }
        SomeFile ConfigFile { get; set; }
        SomeFile ProgramFile { get; set; }
        SomeFile MachineStateFile { get; set; }
        string ProgramLanguage { get; set; }
        bool AutoResetOnStart { get; set; }
        #endregion

        #region Events
        event EventHandler<ShowMessageEventArgs> ShowingMessage;
        event EventHandler<AskUserEventArgs> AskingUser;
        event EventHandler<SaveFileByUserEventArgs> SavingFileByUser;
        event EventHandler<OpenFileByUserEventArgs> OpeningFileByUser;
        event EventHandler GotUpdated;
        #endregion

        void Dispose();
        Task DisposeAsync(bool immediately = false);

        #region Components managing subs
        IInputPort NewInputPort(InputPortTypeInfo inputPortTypeInfo);
        IOutputPort NewOutputPort(OutputPortTypeInfo outputPortTypeInfo);
        IIdeComponent InitializeNewSettingWindow(IIdeComponent ideComponentToSet);
        IExtraIdeComponent NewExtraIdeComponent(ComponentTypeInfo extraComponentTypeInfo);
        #endregion

        #region Environment subs
        #endregion

        #region Config File subs
        void LoadConfigFile(string fileName);
        void SaveConfigFile();
        void SaveConfigFile(string fileName);
        #endregion

        #region Program File subs
        void NewProgramFile(string shortName = "");
        void OpenProgramFile(string fileName);
        void SaveProgramFile();
        void SaveProgramFile(string fileName);
        //
        // // Driven by user Program file subs:
        bool NewProgramFileByUser();
        bool OpenProgramFileByUser();
        bool SaveProgramFileByUser();
        bool SaveProgramFileAsByUser();
        #endregion

        #region Machine Setting subs
        int MachineInsertInputPort(int associatedInputPortIndex = -2, IInputPort inputPort = null);
        int MachineInsertInputPort(IInputPort inputPort);
        int MachineInsertOutputPort(int associatedOutputPortIndex = -2, IOutputPort outputPort = null);
        int MachineInsertOutputPort(IOutputPort outputPort);
        // // //
        void MachineSetInputDevice(int associatedInputPortIndex, IInputDevice inputDevice = null);
        void MachineSetInputDevice(IInputPort inputPort, IInputDevice inputDevice = null);
        void MachineSetInputDevice(IInputDevice inputDevice = null);
        void MachineSetOutputDevice(int associatedOutputPortIndex, IOutputDevice outputDevice = null);
        void MachineSetOutputDevice(IOutputPort outputPort, IOutputDevice outputDevice = null);
        void MachineSetOutputDevice(IOutputDevice outputDevice = null);
        #endregion

        #region Machine Running subs
        Task<bool> MachineStartAsync(VirtualMachineRunningStatus withStatus = VirtualMachineRunningStatus.Runtime);
        Task MachinePauseAsync();
        Task MachineStepAsync();
        Task MachineStepOverAsync();
        Task MachineBreakAsync();
        Task MachineTerminateAsync();
        Task MachineResetAsync();
        Task<bool> MachineRestartAsync();
        #endregion

        #region Machine State subs
        void CopyMachineStateToBuffer();
        void PasteMachineStateFromBuffer();
        void SaveMachineState();
        void SaveMachineState(string fileName);
        void LoadMachineStateFromFile(string fileName);
        //
        // // Driven by user State file subs:
        bool SaveMachineStateByUser();
        bool SaveMachineStateByUser(string fileName);
        bool LoadMachineStateFromFileByUser(string fileName);
        #endregion
    }
}
