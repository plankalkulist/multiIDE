using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using XSpace;
using System.ComponentModel;

namespace multiIDE.Machines
{
    public abstract class AGeneralVirtualMachine : IVirtualMachine, IPausable, ISteppable
    {
        #region Profile properties
        [Category("Profile"), ReadOnly(true)]
        public abstract string DefaultName { get; }
        //
        [Category("Profile"), ReadOnly(true)]
        public abstract string Version { get; }
        //
        [Category("Profile"), ReadOnly(true)]
        public abstract string Author { get; }
        //
        [Category("Profile"), ReadOnly(true)]
        public abstract string Description { get; }
        //
        [Category("Profile"), ReadOnly(true)]
        public abstract string TargetLanguage { get; }
        //
        [Category("Profile"), ReadOnly(true)]
        public abstract string SupportedLanguages { get; }
        //
        [Category("Profile"), ReadOnly(true)]
        public abstract string BaseLanguage { get; }
        //
        [Category("Profile"), ReadOnly(true)]
        public abstract string ProgramFileFilter { get; }
        #endregion

        #region Environment properties
        [Category("Environment"), ReadOnly(true)]
        public virtual IIDE ParentIDE { get; set; }
        //
        [Category("Environment"), ReadOnly(true)]
        public virtual string CustomName { get; set; } = "";
        //
        [Category("Environment"), ReadOnly(true)]
        public virtual int Id { get; set; }
        //
        [Category("Appearance"), DefaultValue("")]
        public virtual string Title
        {
            get
            {
                return _CustomTitle.IsNotNullOrEmpty() ? _CustomTitle
                    : (CustomName.IsNotNullOrEmpty() ? CustomName
                    : $"{DefaultName} id{Id}");
            }
            set
            {
                _CustomTitle = value;
            }
        }
        //
        [Category("Environment"), ReadOnly(true)]
        public virtual object Tag { get; set; }
        //
        protected string _CustomTitle = "";
        #endregion

        #region Essential properties
        [Category("Essential"), ReadOnly(false)]
        public virtual string CurrentLanguage
        {
            get
            {
                return _CurrentLanguage;
            }
            set
            {
                string[] langs = SupportedLanguages.Split(';', ',');
                if (string.IsNullOrEmpty(value) || Array.IndexOf(langs, value) >= 0)
                {
                    _CurrentLanguage = value;
                }
                else
                {
                    throw new ArgumentException();
                }
            }
        }
        //
        [Category("Essential"), ReadOnly(false)]
        public virtual int CaretBound
        {
            get
            {
                return _CaretBound;
            }
            set
            {
                if (_Status == VirtualMachineRunningStatus.StandBy)
                    _CaretBound = value;
                else
                    throw new InvalidSetAtTheMomentException();
            }
        }
        //
        [Category("Essential"), ReadOnly(false)]
        public virtual bool RoundCaretRange
        {
            get
            {
                return _RoundCaretRange;
            }
            set
            {
                if (_Status == VirtualMachineRunningStatus.StandBy)
                    _RoundCaretRange = value;
                else
                    throw new InvalidSetAtTheMomentException();
            }
        }
        //
        [Category("Essential"), ReadOnly(false)]
        public virtual bool RoundValueRange
        {
            get
            {
                return _RoundValueRange;
            }
            set
            {
                if (_Status == VirtualMachineRunningStatus.StandBy)
                    _RoundValueRange = value;
                else
                    throw new InvalidSetAtTheMomentException();
            }
        }
        //
        [Category("Essential"), ReadOnly(false)]
        public virtual Encoding CharSet
        {
            get
            {
                return _CharSet;
            }
            set
            {
                if (_Status == VirtualMachineRunningStatus.StandBy)
                    _CharSet = value;
                else
                    throw new InvalidSetAtTheMomentException();
            }
        }
        //
        [Category("Essential"), ReadOnly(false)]
        public virtual ByteOrders ByteOrder
        {
            get
            {
                return _ByteOrder;
            }
            set
            {
                if (_Status == VirtualMachineRunningStatus.StandBy)
                    _ByteOrder = value;
                else
                    throw new InvalidSetAtTheMomentException();
            }
        }
        //
        [Category("Essential"), ReadOnly(false)]
        public virtual byte InterruptingPeriod
        {
            get
            {
                return _InterruptingPeriod;
            }
            set
            {
                _InterruptingPeriod = value;
            }
        }
        //
        protected string _CurrentLanguage = "";
        protected int _CaretBound;
        protected bool _RoundCaretRange;
        protected bool _RoundValueRange;
        protected Encoding _CharSet;
        protected ByteOrders _ByteOrder;
        protected byte _InterruptingPeriod;
        #endregion

        #region Running State properties
        [Category("Running State"), ReadOnly(true)]
        public virtual Task<VirtualMachineRunResult> RunningTask
        {
            get { return _RunningTask; }
        }
        //
        [Category("Running State"), ReadOnly(false)]
        public virtual byte[] RAM
        {
            get
            {
                return MRAM;
            }
            set
            {
                if (_Status == VirtualMachineRunningStatus.StandBy)
                {
                    if (MRAM.Length == value.Length)
                    {
                        MRAM = value;
                    }
                    else if (MRAM.Length > value.Length)
                    {
                        MRAM.Initialize();
                        Array.Copy(value, MRAM, value.Length);
                    }
                }
                else
                    throw new InvalidSetAtTheMomentException();
            }
        }
        //
        [Category("Running State"), ReadOnly(false)]
        public virtual int CodeLength
        {
            get { return _CodeLength; }
        }
        //
        [Category("Running State"), ReadOnly(false)]
        public virtual bool IsProgrammed
        {
            get { return _IsProgrammed; }
        }
        //
        [Category("Running State"), ReadOnly(false)]
        public virtual VirtualMachineRunningStatus Status
        {
            get { return _Status; }
        }
        //
        [Category("Running State"), ReadOnly(false)]
        public virtual int NextSymbol
        {
            get
            {
                return _NextSymbol;
            }
            set
            {
                if (_NextSymbol > _CaretBound)
                    throw new ArgumentOutOfRangeException();
                if (_Status != VirtualMachineRunningStatus.Paused && _Status != VirtualMachineRunningStatus.StandBy)
                    throw new InvalidSetAtTheMomentException();
                _NextSymbol = value;
            }
        }
        //
        [Category("Running State"), ReadOnly(false)]
        public virtual int ActionCell
        {
            get
            {
                return _ActionCell;
            }
            set
            {
                if (_Status == VirtualMachineRunningStatus.Paused
                        || _Status == VirtualMachineRunningStatus.StandBy)
                    _ActionCell = value;
                else
                    throw new InvalidSetAtTheMomentException();
            }
        }
        //
        [Category("Running State"), ReadOnly(true)]
        public virtual IInputPort ActionInputPort
        {
            get
            {
                return _ActionInputPort;
            }
            set
            {
                _ActionInputPort = value;
            }
        }
        //
        [Category("Running State"), ReadOnly(true)]
        public virtual IOutputPort ActionOutputPort
        {
            get
            {
                return _ActionOutputPort;
            }
            set
            {
                _ActionOutputPort = value;
            }
        }
        //
        protected Task<VirtualMachineRunResult> _RunningTask;
        protected byte[] MRAM;
        protected int _CodeLength;
        protected bool _IsProgrammed;
        protected VirtualMachineRunningStatus _Status;
        protected int _NextSymbol;
        protected int _ActionCell;
        protected IInputPort _ActionInputPort;
        protected IOutputPort _ActionOutputPort;
        #endregion

        #region Operators
        public virtual byte this[int index]
        {
            get { return MRAM[index]; }
            set { MRAM[index] = value; }
        }
        #endregion

        #region Events
        public virtual event EventHandler<VirtualMachineStatusChangedEventArgs> StatusChanged;
        public virtual event VirtualMachineErrorEventHandler Error;
        public virtual event EventHandler SetsChanged;
        //
        protected virtual void OnStatusChanged(VirtualMachineRunningStatus newStatus)
        {
            StatusChanged?.Invoke(this, new VirtualMachineStatusChangedEventArgs(newStatus, NextSymbol));
        }
        //
        protected virtual void OnError(VirtualMachineRunningErrorId errorNumber, out VirtualMachineRunningErrorReaction reaction)
        {
            reaction = VirtualMachineRunningErrorReaction.Undefined;
            Error?.Invoke(this, errorNumber, out reaction);
        }
        //
        protected virtual void OnSetsChanged()
        {
            SetsChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        public override string ToString() => (CustomName.IsNotNullOrEmpty()) ? CustomName
                                : $"{DefaultName} id{Id}";

        public override int GetHashCode() => DefaultName.GetHashCode() ^ Version.GetHashCode() ^ Author.GetHashCode()
                                ^ TargetLanguage.GetHashCode() ^ SupportedLanguages.GetHashCode() ^ BaseLanguage.GetHashCode()
                                ^ (Id.GetHashCode() * 7);

        /////
        protected const int _StatusWaitingCheckEveryTimeMS = 10;
        protected CancellationTokenSource _RunningCancellationTokenSource;

        public virtual void DefaultSettings(bool mute)
        {
            if (_Status != VirtualMachineRunningStatus.StandBy)
                return;

            _CaretBound = 30000;
            _RoundCaretRange = false;
            _RoundValueRange = true;
            _CharSet = Encoding.Default;
            _ByteOrder = ByteOrders.LittleEndian;
            InterruptingPeriod = 20;
            _ActionInputPort = null;
            _ActionOutputPort = null;
            if (!mute) OnSetsChanged();
        }

        public virtual void DefaultSettings()
        {
            DefaultSettings(false);
        }

        public virtual void LoadProgramCode(string programCode)
        {
            if (_Status != VirtualMachineRunningStatus.StandBy)
                throw new InvalidSetAtTheMomentException();

            if (!(programCode.Length < _CaretBound))
                throw new TooBigProgramCodeException();

            if (MRAM == null || MRAM.Length < 1)
            {
                MRAM = new byte[_CaretBound - 1];
            }

            _CodeLength = programCode.Length;

            for (int A = 0; A < _CodeLength; A++)
            {
                MRAM[A] = (byte)(programCode[A]);
            }
            _IsProgrammed = true;
        }

        #region Running subs
        public abstract Task<VirtualMachineRunResult> StartAsync();
        //
        public virtual async Task<VirtualMachineActionPosition> PauseAsync()
        {
            if (!_IsProgrammed)
                throw new MachineNotProgrammedYetException();

            if (_Status == VirtualMachineRunningStatus.Runtime || _Status == VirtualMachineRunningStatus.Stepping)
            {
                _Status = VirtualMachineRunningStatus.Pausing;
                OnStatusChanged(_Status);

                await Task.Run(() =>
                {
                    Thread.CurrentThread.Name = $"{this.Title} Pausing Watcher Thread";

                    while (_Status != VirtualMachineRunningStatus.Paused && _Status!= VirtualMachineRunningStatus.StandBy)
                    {
                        Thread.Sleep(_StatusWaitingCheckEveryTimeMS);
                    }
                    OnStatusChanged(_Status);
                });

                return (VirtualMachineActionPosition)_NextSymbol;
            }
            else
            {
                throw new ImpossibleActionAtTheMomentException(_NextSymbol, _ActionCell);
            }
        }
        //
        public virtual async Task<VirtualMachineActionPosition> StepAsync()
        {
            if (!_IsProgrammed)
                throw new MachineNotProgrammedYetException();

            switch (_Status)
            {
                case VirtualMachineRunningStatus.Paused:
                    _Status = VirtualMachineRunningStatus.Stepping;
                    OnStatusChanged(_Status);
                    break;
                default:
                    throw new MachineIsRunningAlreadyException(_NextSymbol, _ActionCell);
            }

            await Task.Run(() =>
            {
                Thread.CurrentThread.Name = $"{this.Title} Stepping Watcher Thread";

                while (_Status != VirtualMachineRunningStatus.Paused && _Status != VirtualMachineRunningStatus.StandBy)
                {
                    Thread.Sleep(_StatusWaitingCheckEveryTimeMS);
                }
                OnStatusChanged(_Status);
            });

            return (VirtualMachineActionPosition)_NextSymbol;
        }
        //
        public virtual async Task<VirtualMachineActionPosition> BreakAsync()
        {
            if (!_IsProgrammed)
                throw new MachineNotProgrammedYetException();

            if (_Status != VirtualMachineRunningStatus.StandBy && _Status != VirtualMachineRunningStatus.Breaking)
            {
                _Status = VirtualMachineRunningStatus.Breaking;
                OnStatusChanged(_Status);

                await Task.Run(() =>
                {
                    Thread.CurrentThread.Name = $"{this.Title} Breaking Watcher Thread";

                    while (_Status != VirtualMachineRunningStatus.StandBy)
                    {
                        Thread.Sleep(_StatusWaitingCheckEveryTimeMS);
                    }
                });

                return (VirtualMachineActionPosition)_NextSymbol;
            }
            else
            {
                throw new ImpossibleActionAtTheMomentException(_NextSymbol, _ActionCell);
            }
        }
        //
        public virtual async Task TerminateAsync()
        {
            if (!_IsProgrammed)
                throw new MachineNotProgrammedYetException();

            if (_RunningTask?.Status == TaskStatus.Running)
            {
                _RunningCancellationTokenSource.Cancel();

                await Task.Run(() =>
                {
                    Thread.CurrentThread.Name = $"{this.Title} Terminating Watcher Thread";

                    while (_Status != VirtualMachineRunningStatus.StandBy)
                    {
                        Thread.Sleep(_StatusWaitingCheckEveryTimeMS);
                    }
                });
            }
            else
            {
                throw new ImpossibleActionAtTheMomentException(_NextSymbol, _ActionCell);
            }
        }
        //
        public virtual async Task ResetAsync()
        {
            if (_Status != VirtualMachineRunningStatus.StandBy)
            {
                await BreakAsync();
                OnStatusChanged(_Status);
            }

            _RunningTask = null;
            MRAM = new byte[_CaretBound - 1];
            _CodeLength = 0;
            _IsProgrammed = false;
            _NextSymbol = 0;
            _ActionCell = 0;
        }
        #endregion
    }
}
