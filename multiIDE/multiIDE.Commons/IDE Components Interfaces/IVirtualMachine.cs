using System;
using System.Text;
using System.Threading.Tasks;
using XSpace;

namespace multiIDE
{
    #region Virtual Machine Parameters Types
    public enum VirtualMachineActionPosition : int
    {
        Invalid = -1
    }
    //
    public enum VirtualMachineRunningErrorId : int
    {
        Undefined = -1,
        CellValueOutOfRange = 1,
        CellPointerOutOfRange = 2
    }
    //
    public enum VirtualMachineRunningErrorReaction : int
    {
        Undefined = -1,
        Break = 0,
        Pause = 1,
        TryAgain = 2,
        Miss = 3
    }
    //
    public enum VirtualMachineRunningStatus : int
    {
        Undefined = -1,
        StandBy = 12,
        Runtime = 4,
        Pausing = 0,
        Paused = 8,
        Breaking = 3,
        Stepping = 1,
        SteppingOver = 2
    }
    //
    [Flags]
    public enum VirtualMachineRunResult : int
    {
        Undefined = -1,
        RanSuccessfully = 0,
        Broken = 1
    }
    //
    public enum VirtualMachineStateShotResult : int
    {

    }

    #region Events' members
    public class VirtualMachineStatusChangedEventArgs : EventArgs
    {
        public VirtualMachineRunningStatus NewStatus { get; private set; }
        public int NextSymbol { get; private set; }
        public VirtualMachineStatusChangedEventArgs(VirtualMachineRunningStatus newStatus, int nextSymbol)
        {
            NewStatus = newStatus;
            NextSymbol = nextSymbol;
        }
    }
    public delegate void VirtualMachineErrorEventHandler(object sender, VirtualMachineRunningErrorId errorNumber, out VirtualMachineRunningErrorReaction reaction);
    #endregion
    #endregion

    public interface IVirtualMachine : IIdeComponent
    {
        #region Profile properties
        string TargetLanguage { get; }
        string SupportedLanguages { get; }
        string BaseLanguage { get; }
        string ProgramFileFilter { get; }
        #endregion
                
        #region Essential properties
        string CurrentLanguage { get; set; }
        int CaretBound { get; set; }
        bool RoundCaretRange { get; set; }
        bool RoundValueRange { get; set; }
        Encoding CharSet { get; set; }
        ByteOrders ByteOrder { get; set; }
        byte InterruptingPeriod { get; set; }
        #endregion

        #region Running State properties
        Task<VirtualMachineRunResult> RunningTask { get; }
        byte[] RAM { get; set; }
        bool Programmed { get; }
        int CodeLength { get; }
        VirtualMachineRunningStatus Status { get; }
        int NextSymbol { get; set; }
        int ActionCell { get; set; }
        IInputPort ActionInputPort { get; set; }
        IOutputPort ActionOutputPort { get; set; }
        #endregion

        #region Operators
        byte this[int index] { get; set; }
        #endregion

        #region Events
        event EventHandler<VirtualMachineStatusChangedEventArgs> StatusChanged;
        event VirtualMachineErrorEventHandler Error;
        event EventHandler SetsChanged;
        #endregion

        string ToString(); // it is recommended to override Object.ToString() method basing on Profile and Environment properties, look at AGeneralVirtualMachine for example.
        int GetHashCode(); // it is recommended to override Object.GetHashCode() method basing on Profile and Environment properties, look at AGeneralVirtualMachine for example.
        void DefaultSettings(bool mute = false);
        void LoadProgramCode(string programCode);

        #region Running subs
        Task<VirtualMachineRunResult> StartAsync();
        Task<VirtualMachineActionPosition> BreakAsync();
        Task TerminateAsync();
        Task ResetAsync();
        #endregion
    }
}
