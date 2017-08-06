using System;
using System.Windows.Forms;
using XSpace;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.ComponentModel;

namespace multiIDE.Machines
{
    public sealed class ClassicBFmachine : IVirtualMachine
    {
        #region Profile properties
        [Category("Profile"), ReadOnly(true)]
        public string DefaultName => "Classic BF virtual machine";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Version => "1.0.0.0 beta";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Author => "";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Description => "Emulates classic BF machine.";
        //
        [Category("Profile"), ReadOnly(true)]
        public string TargetLanguage => "BF";
        //
        [Category("Profile"), ReadOnly(true)]
        public string SupportedLanguages => "BF";
        //
        [Category("Profile"), ReadOnly(true)]
        public string BaseLanguage => "BF";
        //
        [Category("Profile"), ReadOnly(true)]
        public string ProgramFileFilter => "BF Program Files (*.bf)|*.bf";
        #endregion

        #region Environment properties
        [Category("Environment"), ReadOnly(true)]
        public IIDE ParentIDE { get; set; }
        //
        [Category("Environment"), ReadOnly(true)]
        public string CustomName { get; set; } = "";
        //
        [Category("Environment"), ReadOnly(true)]
        public string CurrentLanguage
        {
            get { return mCurrentLanguage; }
            set { if (value != "BF") throw new Exception(); }
        }
        //
        [Category("Environment"), ReadOnly(true)]
        public int Id { get; set; }
        //
        [Category("Appearance"), DefaultValue("")]
        public string Title { get; set; }
        //
        [Category("Environment"), ReadOnly(true)]
        public object Tag { get; set; }
        //
        private string mCurrentLanguage = "BF";
        #endregion

        #region Essential properties
        [Category("Essential"), ReadOnly(false)]
        public int CaretBound
        {
            get
            {
                return _CaretBound;
            }
            set
            {
                if (_Status == VirtualMachineRunningStatus.StandBy) _CaretBound = value;
            }
        }
        //
        [Category("Essential"), ReadOnly(false)]
        public bool RoundCaretRange
        {
            get
            {
                return _RoundCaretRange;
            }
            set
            {
                if (_Status == VirtualMachineRunningStatus.StandBy) _RoundCaretRange = value;
            }
        }
        //
        [Category("Essential"), ReadOnly(false)]
        public bool RoundValueRange
        {
            get
            {
                return _RoundValueRange;
            }
            set
            {
                if (_Status == VirtualMachineRunningStatus.StandBy) _RoundValueRange = value;
            }
        }
        //
        [Category("Essential"), ReadOnly(false)]
        public Encoding CharSet
        {
            get
            {
                return _CharSet;
            }
            set
            {
                if (_Status == VirtualMachineRunningStatus.StandBy) _CharSet = value;
            }
        }
        //
        [Category("Essential"), ReadOnly(false)]
        public ByteOrders ByteOrder
        {
            get
            {
                return _ByteOrder;
            }
            set
            {
                if (_Status == VirtualMachineRunningStatus.StandBy) _ByteOrder = value;
            }
        }
        //
        [Category("Essential"), ReadOnly(false)]
        public byte InterruptingPeriod { get { return _InterruptingPeriod; } set { _InterruptingPeriod = value; } }
        //
        [Category("Running State"), ReadOnly(false)]
        public IInputPort ActionInputPort
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
        [Category("Running State"), ReadOnly(false)]
        public IOutputPort ActionOutputPort
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
        private int _CaretBound;
        private bool _RoundCaretRange;
        private bool _RoundValueRange;
        private Encoding _CharSet;
        private ByteOrders _ByteOrder;
        private byte _InterruptingPeriod;
        private IInputPort _ActionInputPort;
        private IOutputPort _ActionOutputPort;
        #endregion

        #region Running State properties
        [Category("Running State"), ReadOnly(true)]
        public Task<VirtualMachineRunResult> RunningTask
        {
            get { return _RunningTask; }
        }
        //
        [Category("Running State"), ReadOnly(false)]
        public byte[] RAM
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
            }
        }
        //
        [Category("Running State"), ReadOnly(false)]
        public int CodeLength { get { return _CodeLength; } }
        //
        [Category("Running State"), ReadOnly(false)]
        public bool Programmed { get { return _Programmed; } }
        //
        [Category("Running State"), ReadOnly(false)]
        public VirtualMachineRunningStatus Status { get { return _Status; } }
        //
        [Category("Running State"), ReadOnly(false)]
        public int NextSymbol
        {
            get
            {
                return _NextSymbol;
            }
            set
            {
                if (_Status == VirtualMachineRunningStatus.StandBy) _NextSymbol = value;
            }
        }
        //
        [Category("Running State"), ReadOnly(false)]
        public int ActionCell
        {
            get
            {
                return _ActionCell;
            }
            set
            {
                if (_Status == VirtualMachineRunningStatus.StandBy) _ActionCell = value;
            }
        }
        //
        private Task<VirtualMachineRunResult> _RunningTask;
        private byte[] MRAM;
        private int _CodeLength;
        private bool _Programmed;
        private VirtualMachineRunningStatus _Status;
        private int _NextSymbol;
        private int _ActionCell;
        #endregion

        #region Operators
        public byte this[int index]
        {
            get { return MRAM[index]; }
            set { MRAM[index] = value; }
        }
        #endregion

        #region Events
        public event EventHandler<VirtualMachineStatusChangedEventArgs> StatusChanged;
        public event VirtualMachineErrorEventHandler Error;
        public event EventHandler SetsChanged;
        #endregion

        public override string ToString() => (CustomName.IsNotNullOrEmpty()) ? CustomName
                                : DefaultName + Id.ToString();

        public override int GetHashCode() => DefaultName.GetHashCode() ^ Version.GetHashCode() ^ Author.GetHashCode()
                                ^ TargetLanguage.GetHashCode() ^ SupportedLanguages.GetHashCode() ^ BaseLanguage.GetHashCode()
                                ^ (Id.GetHashCode() * 7);

        /////
        private const int _StatusWaitingCheckEveryTimeMS = 10;
        private CancellationTokenSource _RunningCancellationTokenSource = new CancellationTokenSource();

        public void DefaultSettings(bool mute = false)
        {
            if (_Status != VirtualMachineRunningStatus.StandBy)
                return;
            _CaretBound = 30000;
            _RoundCaretRange = false;
            _RoundValueRange = true;
            _CharSet = Encoding.Default;
            _ByteOrder = ByteOrders.LittleEndian;
            _InterruptingPeriod = 20;
            if (!mute) SetsChanged?.Invoke(this, new EventArgs());
        }

        public ClassicBFmachine()
        {
            _Status = VirtualMachineRunningStatus.StandBy;
            DefaultSettings(true);
        }

        public ClassicBFmachine(int caretBound = 30000, bool overBound = true,
                          bool unLimit = true, byte interruptingPeriod = 20)
        {
            _CaretBound = caretBound;
            _RoundCaretRange = overBound;
            _RoundValueRange = unLimit;
            _CharSet = Encoding.Default;
            _ByteOrder = ByteOrders.LittleEndian;
            _InterruptingPeriod = interruptingPeriod;

            _Status = VirtualMachineRunningStatus.StandBy;

            ResetAsync();
        }

        public void LoadProgramCode(string programCode)
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
            _Programmed = true;
        }

        #region Running subs
        public async Task<VirtualMachineRunResult> StartAsync()
        {
            if (!_Programmed)
                throw new MachineNotProgrammedYetException();

            VirtualMachineRunResult RES;

            if (_Status == VirtualMachineRunningStatus.StandBy)
            {
                _ActionCell = _CodeLength + 1;
                _Status = VirtualMachineRunningStatus.Runtime;
                StatusChanged?.Invoke(this, new VirtualMachineStatusChangedEventArgs(_Status,_NextSymbol));

                RES = await Task<VirtualMachineRunResult>.Factory.StartNew(() => Run(0, 0, 0));

                if (_Status != VirtualMachineRunningStatus.StandBy)
                {
                    _Status = VirtualMachineRunningStatus.StandBy;
                    StatusChanged?.Invoke(this, new VirtualMachineStatusChangedEventArgs(_Status, _NextSymbol));
                }
                return RES;
            }
            else
            {
                throw new ImpossibleActionAtTheMomentException(_NextSymbol, _ActionCell);
            }
        }

        public async Task<VirtualMachineActionPosition> BreakAsync()
        {
            if (!_Programmed) return VirtualMachineActionPosition.Invalid;
            if (_Status == VirtualMachineRunningStatus.Runtime)
                _Status = VirtualMachineRunningStatus.Breaking;
            else
                return VirtualMachineActionPosition.Invalid;
            StatusChanged?.Invoke(this, new VirtualMachineStatusChangedEventArgs(_Status, _NextSymbol));
            return (VirtualMachineActionPosition)_NextSymbol;
        }

        public async Task TerminateAsync()
        {
            if (!_Programmed)
                throw new MachineNotProgrammedYetException();

            if (_RunningTask.Status == TaskStatus.Running)
            {
                _RunningCancellationTokenSource.Cancel();

                await Task.Run(() =>
                {
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

        public async Task ResetAsync()
        {
            if (_Status == VirtualMachineRunningStatus.Runtime)
            {
                BreakAsync();
                while (_Status != VirtualMachineRunningStatus.StandBy) Application.DoEvents();
            }
            _RunningTask = null;
            MRAM = new byte[_CaretBound - 1];
            _CodeLength = 0;
            _Programmed = false;
            if (_Status != VirtualMachineRunningStatus.StandBy)
            {
                _Status = VirtualMachineRunningStatus.StandBy;
                StatusChanged?.Invoke(this, new VirtualMachineStatusChangedEventArgs(_Status, _NextSymbol));
            }
            _NextSymbol = 0;
            _ActionCell = 0;
        }
        #endregion

        private VirtualMachineRunResult Run(int CodeStart, int CodeLen, int Cycle)
        {
            //обработка аргументов
            if (Cycle != 0 && MRAM[_ActionCell] == 0) return VirtualMachineRunResult.RanSuccessfully;
            if (CodeStart == 0 && CodeLen == 0 && Cycle == 0)
            {
                Thread.CurrentThread.Name = this.ParentIDE.Title + " Running Thread";
                Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
            }
            //
            int A = 0, L = 0, Z = 0;
            byte D = 0;
            VirtualMachineRunningErrorReaction Reaction = VirtualMachineRunningErrorReaction.Undefined;

            do
            {
                for (A = CodeStart; A < CodeStart + CodeLen; A++)
                {
                CURR_STEP: _NextSymbol = A;
                    if (_NextSymbol != A) { A = _NextSymbol - 1; continue; }
                    if (_Status == VirtualMachineRunningStatus.Breaking) return VirtualMachineRunResult.Broken;
                    switch (MRAM[A])
                    {
                        case 62: // >
                            Z = _ActionCell + 1;
                            if (Z <= _CaretBound)
                            {
                                _ActionCell = Z;
                            }
                            else if (_RoundCaretRange)
                            {
                                _ActionCell = 0;
                            }
                            else
                            {
                                Error?.Invoke(this, VirtualMachineRunningErrorId.CellPointerOutOfRange, out Reaction);
                                if (Reaction == VirtualMachineRunningErrorReaction.TryAgain) goto CURR_STEP;
                            }
                            break;
                        case 60: // <
                            Z = _ActionCell - 1;
                            if (Z >= 0)
                            {
                                _ActionCell = Z;
                            }
                            else if (_RoundCaretRange)
                            {
                                _ActionCell = _CaretBound + Z + 1;
                            }
                            else
                            {
                                Error?.Invoke(this, VirtualMachineRunningErrorId.CellPointerOutOfRange, out Reaction);
                                if (Reaction == VirtualMachineRunningErrorReaction.TryAgain) goto CURR_STEP;
                            }
                            break;
                        case 43: // +
                            Z = MRAM[_ActionCell] + 1;
                            if (Z <= 255)
                            {
                                MRAM[_ActionCell] = (byte)Z;
                            }
                            else if (_RoundValueRange)
                            {
                                MRAM[_ActionCell] = (byte)(0 + Z - 255 - 1);
                            }
                            else
                            {
                                Error?.Invoke(this, VirtualMachineRunningErrorId.CellValueOutOfRange, out Reaction);
                                if (Reaction == VirtualMachineRunningErrorReaction.TryAgain) goto CURR_STEP;
                            }
                            break;
                        case 45: // -
                            Z = MRAM[_ActionCell] - 1;
                            if (Z >= 0)
                            {
                                MRAM[_ActionCell] = (byte)Z;
                            }
                            else if (_RoundValueRange)
                            {
                                MRAM[_ActionCell] = (byte)(255 - 0 + Z + 1);
                            }
                            else
                            {
                                Error?.Invoke(this, VirtualMachineRunningErrorId.CellValueOutOfRange, out Reaction);
                                if (Reaction == VirtualMachineRunningErrorReaction.TryAgain) goto CURR_STEP;
                            }
                            break;
                        case 46: // .
                            _ActionOutputPort.Output(MRAM[_ActionCell]);
                            break;
                        case 44: // ,
                            MRAM[ActionCell] = _ActionInputPort.Input();
                            break;
                        case 91: // [
                            L = XString.FindClosing(MRAM, A + 1, CodeLen - (A - CodeStart) - 1, '[', ']', "", "'" + (char)(34), "'" + (char)(34)) - A - 1;
                            Run(A + 1, L, -1);
                            A += L + 1;
                            break;
                        case 93: // ]
                            MessageBox.Show(": Неожиданный символ " + (char)(34) + "]" + (char)(34) + " в позиции " + A.ToString() + (char)(10) + (char)(10) + "Завершение цикла без его начала", "Сообщение BF Machine", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            break;
                    }
                }
                D += 1;
                if (D > _InterruptingPeriod)
                {
                    Application.DoEvents();
                    D = 0;
                }
            } while ((Cycle != 0) && (MRAM[_ActionCell] != 0));

            return VirtualMachineRunResult.RanSuccessfully;
        }
    }
}
