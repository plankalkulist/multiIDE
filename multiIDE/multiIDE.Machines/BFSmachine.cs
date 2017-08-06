using System;
using System.Collections.Generic;
using System.Windows.Forms;
using XSpace;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.ComponentModel;

namespace multiIDE.Machines
{
    public class BFSmachine : AGeneralVirtualMachine, IMultiIOable, IStepOverable, IStartWithSpecifiedStatusable
    {
        #region Profile properties
        [Category("Profile"), ReadOnly(true)]
        public sealed override string DefaultName => "BFS virtual machine";
        //
        [Category("Profile"), ReadOnly(true)]
        public override string Version => "2.0.0.0";
        //
        [Category("Profile"), ReadOnly(true)]
        public override string Author => "Evgeniy Chaev";
        //
        [Category("Profile"), ReadOnly(true)]
        public override string Description => "Emulates BFS machine with classic BF support.";
        //
        [Category("Profile"), ReadOnly(true)]
        public override string TargetLanguage => "BFS";
        //
        [Category("Profile"), ReadOnly(true)]
        public override string SupportedLanguages => "BFS;BF";
        //
        [Category("Profile"), ReadOnly(true)]
        public sealed override string BaseLanguage => "BF";
        //
        [Category("Profile"), ReadOnly(true)]
        public override string ProgramFileFilter => "BFS Program Files (*.bfs)|*.bfs|BF Program Files (*.bf)|*.bf";
        #endregion

        #region Essential properties
        [Category("Essential"), ReadOnly(true)]
        public virtual IInputPort[] InputPorts
        {
            get { return _InputPorts; }
            set { _InputPorts = value; }
        }
        [Category("Essential"), ReadOnly(true)]
        public virtual IOutputPort[] OutputPorts
        {
            get { return _OutputPorts; }
            set { _OutputPorts = value; }
        }
        //
        protected IInputPort[] _InputPorts;
        protected IOutputPort[] _OutputPorts;
        #endregion

        #region Running State properties
        [Category("Running State"), ReadOnly(false)]
        public virtual int ActionInputPortIndex
        {
            get
            {
                return _ActionInputPortIndex;
            }
            set
            {
                if ((value >= 0) && (value < 256))
                {
                    _ActionInputPortIndex = value;
                    _ActionInputPort = _InputPorts[_ActionInputPortIndex];
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }
        //
        [Category("Running State"), ReadOnly(false)]
        public virtual int ActionOutputPortIndex
        {
            get
            {
                return _ActionOutputPortIndex;
            }
            set
            {
                if ((value >= 0) && (value < 256))
                {
                    _ActionOutputPortIndex = value;
                    _ActionOutputPort = _OutputPorts[_ActionOutputPortIndex];
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }
        //
        [Category("Running State"), ReadOnly(true)]
        public override IInputPort ActionInputPort
        {
            get
            {
                return _InputPorts[_ActionInputPortIndex];
            }
            set
            {
                _InputPorts[_ActionInputPortIndex] = value;
            }
        }
        //
        [Category("Running State"), ReadOnly(true)]
        public override IOutputPort ActionOutputPort
        {
            get
            {
                return _OutputPorts[_ActionOutputPortIndex];
            }
            set
            {
                _OutputPorts[_ActionOutputPortIndex] = value;
            }
        }
        //
        protected int _ActionInputPortIndex;
        protected int _ActionOutputPortIndex;
        #endregion

        /////
        protected struct bfsTag
        {
            public string Identificator;
            public int Value;
            public void Reset()
            {
                Identificator = "";
                Value = 0;
            }
        }
        protected struct bfsFunc
        {
            public int CodePtr;
            public int CodeLen;
            public int CallRAM;
            public int FuncSet;
            public int TagSet;
            public int StatSet;
            public int ObjSet;
            public int ArgSet;
            public int LoclSet;
            public void Reset()
            {
                CodePtr = 0;
                CodeLen = 0;
                CallRAM = 0;
                FuncSet = 0;
                TagSet = 0;
                StatSet = 0;
                ObjSet = 0;
                ArgSet = 0;
                LoclSet = 0;
            }
        }
        protected List<bfsTag> _Tags = null;
        protected List<bfsFunc> _Funcs = null;
        protected const int _StatusPausedOutCheckEveryMS = 50;
        //
        protected struct RunArgsLine
        {
            public int CodeStart; public int CodeLen; public int Cycle; public int FuncIndex; public int preFRAM;
            public int preFuncNum; public int preTagNum; public int preStatNum; public int preObjNum;
            public int outFuncNum; public int outTagNum; public int outStatNum; public int outObjNum;
            public VirtualMachineRunResult ReturnValue;
        }
        protected Stack<RunArgsLine> _RunArgsLineStack;

        #region PreSets
        public override void DefaultSettings(bool mute)
        {
            if (_Status != VirtualMachineRunningStatus.StandBy)
                throw new InvalidSetAtTheMomentException();
            _CaretBound = 30000;
            _RoundCaretRange = false;
            _RoundValueRange = true;
            _CharSet = Encoding.Default;
            _ByteOrder = ByteOrders.LittleEndian;
            InterruptingPeriod = 20;
            _ActionInputPort = null;
            _ActionOutputPort = null;
            _InputPorts = new IInputPort[256];
            _OutputPorts = new IOutputPort[256];
            if (!mute) OnSetsChanged();
        }

        public virtual void ClassicMachineSet(bool mute = false)
        {
            if (Status != VirtualMachineRunningStatus.StandBy)
                throw new InvalidSetAtTheMomentException();
            _CaretBound = 30000;
            _RoundCaretRange = true;
            _RoundValueRange = true;
            if (!mute) OnSetsChanged();
        }
        #endregion

        public BFSmachine()
        {
            _Status = VirtualMachineRunningStatus.StandBy;
            DefaultSettings(true);

            MRAM = new byte[_CaretBound - 1];
            _CodeLength = 0;
            _Programmed = false;
            _Status = VirtualMachineRunningStatus.StandBy;
            _NextSymbol = 0;
            _ActionCell = 0;
            _ActionInputPortIndex = 0;
            _ActionOutputPortIndex = 0;

            _Tags = new List<bfsTag>();
            _Funcs = new List<bfsFunc>();
        }

        public BFSmachine(int RAMsize = 30000, bool roundCaretRange = false, bool roundValueRange = true
                        , ByteOrders byteOrder = ByteOrders.LittleEndian, byte interruptingPeriod = 20)
        {
            _CaretBound = RAMsize;
            _RoundCaretRange = roundCaretRange;
            _RoundValueRange = roundValueRange;
            _CharSet = Encoding.Default;
            _ByteOrder = byteOrder;
            _InterruptingPeriod = interruptingPeriod;
            _InputPorts = new IInputPort[256];
            _OutputPorts = new IOutputPort[256];

            MRAM = new byte[_CaretBound - 1];
            _CodeLength = 0;
            _Programmed = false;
            _Status = VirtualMachineRunningStatus.StandBy;
            _NextSymbol = 0;
            _ActionCell = 0;
            _ActionInputPortIndex = 0;
            _ActionOutputPortIndex = 0;

            _Tags = new List<bfsTag>();
            _Funcs = new List<bfsFunc>();
        }

        #region Running subs
        public override async Task<VirtualMachineRunResult> StartAsync()
        {
            if (!_Programmed)
                throw new MachineNotProgrammedYetException();

            VirtualMachineRunResult runResult;

            switch (_Status)
            {
                case VirtualMachineRunningStatus.StandBy:
                    _ActionCell = _CodeLength + 1;

                    _Status = VirtualMachineRunningStatus.Runtime;
                    OnStatusChanged(_Status);

                    try
                    {
                        _RunningCancellationTokenSource = new CancellationTokenSource();
                        _RunningTask = Task.Run<VirtualMachineRunResult>(new Func<VirtualMachineRunResult>(Run)
                                , _RunningCancellationTokenSource.Token);
                        runResult = await _RunningTask;
                    }
                    catch (OperationCanceledException)
                    {
                        throw new RunningTaskHasBeenTerminatedException(_NextSymbol, _ActionCell);
                    }
                    finally
                    {
                        OnStatusChanged(VirtualMachineRunningStatus.StandBy);
                        if (_Status != VirtualMachineRunningStatus.StandBy)
                            _Status = VirtualMachineRunningStatus.StandBy;
                    }

                    return runResult;
                case VirtualMachineRunningStatus.Pausing:
                case VirtualMachineRunningStatus.Paused:
                case VirtualMachineRunningStatus.SteppingOver:
                    _Status = VirtualMachineRunningStatus.Runtime;
                    OnStatusChanged(_Status);
                    return VirtualMachineRunResult.RanSuccessfully;
                case VirtualMachineRunningStatus.Runtime:
                case VirtualMachineRunningStatus.Stepping:
                    throw new MachineIsRunningAlreadyException(_NextSymbol, _ActionCell);
                default:
                    throw new ImpossibleActionAtTheMomentException(_NextSymbol, _ActionCell);
            }
        }

        public virtual async Task<VirtualMachineRunResult> StartAsync(VirtualMachineRunningStatus withSpecifiedStatus)
        {
            if (!(withSpecifiedStatus == VirtualMachineRunningStatus.Runtime
                || withSpecifiedStatus == VirtualMachineRunningStatus.Pausing
                //|| withSpecifiedStatus == RunningStatus.SteppingOver
                //|| withSpecifiedStatus == RunningStatus.Breaking
                //|| withSpecifiedStatus == RunningStatus.Stepping
                ))
                throw new ArgumentException("Specified status should be either Runtime or Pausing.", "withSpecifiedStatus");

            if (!_Programmed)
                throw new MachineNotProgrammedYetException();

            VirtualMachineRunResult RES;

            switch (_Status)
            {
                case VirtualMachineRunningStatus.StandBy:
                    _ActionCell = _CodeLength + 1;

                    _Status = withSpecifiedStatus;
                    OnStatusChanged(_Status);

                    try
                    {
                        RES = await Task.Run<VirtualMachineRunResult>(new Func<VirtualMachineRunResult>(Run)
                                , _RunningCancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        throw new RunningTaskHasBeenTerminatedException(_NextSymbol, _ActionCell);
                    }
                    finally
                    {
                        OnStatusChanged(VirtualMachineRunningStatus.StandBy);
                        if (_Status != VirtualMachineRunningStatus.StandBy)
                            _Status = VirtualMachineRunningStatus.StandBy;
                    }

                    return RES;
                case VirtualMachineRunningStatus.Runtime:
                case VirtualMachineRunningStatus.Stepping:
                    throw new MachineIsRunningAlreadyException(_NextSymbol, _ActionCell);
                default:
                    throw new ImpossibleActionAtTheMomentException(_NextSymbol, _ActionCell);
            }
        }

        public virtual async Task<VirtualMachineActionPosition> StepOverAsync()
        {
            if (!_Programmed)
                throw new MachineNotProgrammedYetException();

            switch (_Status)
            {
                case VirtualMachineRunningStatus.Paused:
                    _Status = VirtualMachineRunningStatus.SteppingOver;
                    OnStatusChanged(_Status);
                    break;
                case VirtualMachineRunningStatus.Runtime:
                case VirtualMachineRunningStatus.Stepping:
                    throw new MachineIsRunningAlreadyException(_NextSymbol, _ActionCell);
                default:
                    throw new ImpossibleActionAtTheMomentException(_NextSymbol, _ActionCell);
            }

            await Task.Run(() =>
            {
                Thread.CurrentThread.Name = $"{this.Title} Stepping-Over Watcher Thread";
                while (_Status != VirtualMachineRunningStatus.Paused)
                {
                    Thread.Sleep(_StatusWaitingCheckEveryTimeMS);
                }
                OnStatusChanged(_Status);
            });

            return (VirtualMachineActionPosition)_NextSymbol;
        }

        public override async Task ResetAsync()
        {
            if (_Status != VirtualMachineRunningStatus.StandBy)
            {
                await BreakAsync();
                OnStatusChanged(_Status);
            }

            _RunningTask = null;
            MRAM = new byte[_CaretBound - 1];
            _CodeLength = 0;
            _Programmed = false;
            _NextSymbol = 0;
            _ActionCell = 0;
            _ActionInputPortIndex = 0;
            _ActionOutputPortIndex = 0;

            if (_Tags != null)
            {
                _Tags.Clear();
            }
            _Tags = new List<bfsTag>();
            if (_Funcs != null)
            {
                _Funcs.Clear();
            }
            _Funcs = new List<bfsFunc>();
        }
        #endregion

        protected VirtualMachineRunResult Run()
        {
            Thread.CurrentThread.Name = $"{this.Title} Running Thread";

            if (_Status == VirtualMachineRunningStatus.Pausing)
                OnStatusChanged(VirtualMachineRunningStatus.Paused);

            if (_Funcs.Count < 1) _Funcs.Add(new bfsFunc());
            DeclareFunc(0, 0);

            _RunArgsLineStack = new Stack<RunArgsLine>();
            RunArgsLine startLine = new RunArgsLine
            {
                preFRAM = -1
            };
            _RunArgsLineStack.Push(startLine);

            try
            {
                Run(_RunArgsLineStack);
            }
            finally
            {
                if (!_RunningCancellationTokenSource.Token.IsCancellationRequested)
                {
                    foreach (IInputPort port in _InputPorts)
                        if (port != null)
                            port.Close();
                    foreach (IOutputPort port in _OutputPorts)
                        if (port != null)
                            port.Close();
                }
                else
                {
                    // TEMPORARY implementation
                    //
                    //
                    foreach (IInputPort port in _InputPorts)
                        if (port != null)
                            Task.Factory.StartNew(() =>
                            {//
                                try
                                {
                                    port.Close();
                                }
                                catch (Exception excepcie)
                                {

                                }
                            }
                            );
                    //
                    foreach (IOutputPort port in _OutputPorts)
                        if (port != null)
                            Task.Factory.StartNew(() =>
                            {// 
                                try
                                {
                                    port.Close();
                                }
                                catch (Exception excepcie)
                                {

                                }
                            }
                            );
                    //
                    //
                    // // //
                }
            }

            return _RunArgsLineStack.Pop().ReturnValue;
        }

        protected void DeclareFunc(int index, int start)
        {
            int A = 0, B = 0, Z = 0;
            string S = null, M = null;
            int FRAMAC = 0;
            int SupRightAC = 0, SupLeftAC = 0;
            int Ending = 0, Repeat = 1;
            int res = 0;
            string RepExp = "";

            bfsFunc F = new bfsFunc();
            F.Reset();
            F.ArgSet = 1;
            if (index == 0)
            {
                A = start;
            }
            else
            {
                A = start;
                while (MRAM[A] == 64 && A < MRAM.Length) // @
                {
                    _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                    A += 1;
                }
            }
            F.CodePtr = A;
            if (index == 0)
            {
                Ending = _CodeLength;
            }
            else
            {
                Ending = XString.FindClosing(MRAM, A, -1, '@', '\\', "#^!*&%ARJ", (char)(34) + "'", (char)(34) + "'");
            }
            F.CodeLen = Ending - F.CodePtr;
            F.LoclSet = A - start + 2;

            for (A = F.CodePtr; A < Ending; A++)
            {
                _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();

                if (RepExp.IsNotNullOrEmpty() && XString.bMid(MRAM, A, B) != RepExp)
                {
                    RepExp = ""; B = 0; Repeat = 1;
                }
                switch (MRAM[A])
                {
                    case 62: // >
                        FRAMAC += Repeat;
                        if (FRAMAC > SupRightAC)
                            SupRightAC = FRAMAC;
                        break;
                    case 60: // <
                        FRAMAC -= Repeat;
                        if (FRAMAC < SupLeftAC)
                            SupLeftAC = FRAMAC;
                        break;
                    case 114: // r
                        FRAMAC = 0;
                        break;
                    case 106: // j
                        FRAMAC = F.LoclSet;
                        break;
                    case 34: // "
                        Z = A;
                        do
                        {
                            _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                            A += 1;
                        } while (MRAM[A] != 34 && A < MRAM.Length);
                        FRAMAC += A - Z - 1;
                        break;
                    case 40: // (
                        do
                        {
                            _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                            A += 1;
                        } while (MRAM[A] != 41 && A < MRAM.Length); // (
                        break;
                    case 91: // [
                        break;
                    case 123: // {
                        S = ""; M = "";
                        Z = A;
                        while (!(M == "}"))
                        {
                            _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                            S += M;
                            A += 1;
                            M = "" + (char)(MRAM[A]);
                        }
                        RepExp = "";
                        B = 1;
                        while (XString.bMid(MRAM, Z - B, B) != XString.bMid(MRAM, A + 1, B))
                        {
                            _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                            B += 1;
                        }
                        RepExp = XString.bMid(MRAM, Z - B, B);
                        if (S.Length == 3 && S[0] == (char)(34) && S[2] == (char)(34))
                        {
                            Repeat = (byte)S[1] - 1;
                        }
                        else if (S == "")
                        {
                        }
                        else if (S == "##")
                        {
                            Repeat = FRAMAC - XString.CountLevel(RepExp, 1, ups: ">", downs: "<") - 1;
                        }
                        else if (S[0] == '#' && S[S.Length - 1] == '#')
                        {
                        }
                        else if (int.TryParse(S, out res))
                        {
                            Repeat = res - 1;
                        }
                        else
                        {
                        }
                        break;
                    case 64: // @
                        do
                        {
                            _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                            A += 1;
                        } while (MRAM[A] != 92); // \

                        break;
                    case 35: // #
                        do
                        {
                            _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                            A += 1;
                        } while (MRAM[A] != 35); // #
                        if (MRAM[A - 1] != 58) // :
                        {
                            do
                            {
                                _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                                A += 1;
                            } while (MRAM[A] != 92); // \
                        }
                        break;
                }
            }
            F.CallRAM = SupRightAC + 1;

            _Funcs[index] = F;
        }

        //protected RunResult Run(int CodeStart, int CodeLen, int Cycle, int FuncIndex, int preFRAM,
        //               int preFuncNum, int preTagNum, int preStatNum, int preObjNum,
        //               ref int outFuncNum, ref int outTagNum, ref int outStatNum, ref int outObjNum)
        //
        //{
        protected void Run(object argsStack)
        {
            RunArgsLine args = (argsStack as Stack<RunArgsLine>).Pop();
            //обработка аргументов
            if (args.Cycle == -1 && MRAM[_ActionCell] == 0)
            {
                args.ReturnValue = VirtualMachineRunResult.RanSuccessfully;
                (argsStack as Stack<RunArgsLine>).Push(args);
                return;
            }
            if (args.CodeStart == 0 && args.CodeLen == 0 && args.Cycle == 0)
            {
                args.CodeStart = _Funcs[args.FuncIndex].CodePtr;
                args.CodeLen = _Funcs[args.FuncIndex].CodeLen;
            }
            //
            int A = 0, RepExpLen = 0, I = 0, L = 0, Y = 0, Z = 0;
            string M = null, S = null;
            byte D = 0;
            int FRAM = 0;
            if (args.preFRAM > -1)
                FRAM = args.preFRAM;
            else
                FRAM = _ActionCell;
            int FRAMAC = 0, CRAMAC = 0;
            int FuncNum = 0, TagNum = 0, StatNum = 0, ObjNum = 0;
            int FN = 0, TN = 0, SN = 0, ObN = 0;
            int RepCount = 1;
            string RepExp = "";
            byte[] ioBuff = null;
            bfsFunc F = new bfsFunc(); bfsTag T = new bfsTag();
            VirtualMachineRunningErrorReaction Reaction = VirtualMachineRunningErrorReaction.Undefined;
            RunArgsLine callArgs;
            args.ReturnValue = VirtualMachineRunResult.RanSuccessfully;
            bool runFin1step = false;

            do
            {
                _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();

                for (A = args.CodeStart; A < args.CodeStart + args.CodeLen; A++)
                {
                    CURR_STEP: // if (RepExp != "" && XString.bMid(MainRAM, A, RepExpLen) != RepExp) { RepExp = ""; RepExpLen = 0; Repeat = 1; }
                    _NextSymbol = A;

                    if (_Status == VirtualMachineRunningStatus.Pausing)
                    {
                        _Status = VirtualMachineRunningStatus.Paused;
                        do
                        {
                            _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                            Thread.Sleep(_StatusPausedOutCheckEveryMS);
                            Application.DoEvents(); // !!!
                        } while (_Status == VirtualMachineRunningStatus.Paused);
                    }
                    if (_Status == VirtualMachineRunningStatus.SteppingOver)
                    {
                        runFin1step = true;
                        StartAsync();
                    }
                    if (_NextSymbol != A)
                    {
                        if (_NextSymbol >= args.CodeStart && _NextSymbol < args.CodeStart + args.CodeLen)
                        {
                            A = _NextSymbol - 1;
                            continue;
                        }
                        else
                        {
                            //_NextSymbol = A;
                            throw new InvalidSetAtTheMomentException();
                        }
                    }
                    if (_Status == VirtualMachineRunningStatus.Breaking)
                    {
                        args.ReturnValue = VirtualMachineRunResult.Broken;
                        (argsStack as Stack<RunArgsLine>).Push(args);
                        return;
                    }
                    if (_Status == VirtualMachineRunningStatus.Stepping)
                        _Status = VirtualMachineRunningStatus.Pausing;

                    _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();

                    switch (MRAM[A])
                    {
                        case 60: // <
                            Z = _ActionCell - RepCount;
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
                                OnError(VirtualMachineRunningErrorId.CellPointerOutOfRange, out Reaction);
                                if (Reaction == VirtualMachineRunningErrorReaction.TryAgain)
                                    goto CURR_STEP;
                                else if (Reaction == VirtualMachineRunningErrorReaction.Miss)
                                    break;
                                else if (Reaction == VirtualMachineRunningErrorReaction.Pause)
                                    goto case 112; // p
                                else if (Reaction == VirtualMachineRunningErrorReaction.Break)
                                    goto case 98; // b
                                else
                                    throw new UnhandledMachineProgramRuntimeErrorException("Cell pointer fell out of the allowable range.", _NextSymbol, _ActionCell);
                            }
                            RepExp = ""; RepExpLen = 0; RepCount = 1;
                            break;
                        case 62: // >
                            Z = _ActionCell + RepCount;
                            if (Z <= _CaretBound)
                            {
                                _ActionCell = Z;
                            }
                            else if (_RoundCaretRange)
                            {
                                _ActionCell = Z - _CaretBound - 1;
                            }
                            else
                            {
                                OnError(VirtualMachineRunningErrorId.CellPointerOutOfRange, out Reaction);
                                if (Reaction == VirtualMachineRunningErrorReaction.TryAgain)
                                    goto CURR_STEP;
                                else if (Reaction == VirtualMachineRunningErrorReaction.Miss)
                                    break;
                                else if (Reaction == VirtualMachineRunningErrorReaction.Pause)
                                    goto case 112; // p
                                else if (Reaction == VirtualMachineRunningErrorReaction.Break)
                                    goto case 98; // b
                                else
                                    throw new UnhandledMachineProgramRuntimeErrorException("Cell pointer fell out of the allowable range.", _NextSymbol, _ActionCell);
                            }
                            RepExp = ""; RepExpLen = 0; RepCount = 1;
                            break;
                        case 43: // +
                            if (_RoundValueRange)
                            {
                                unchecked { MRAM[_ActionCell] += (byte)(RepCount % 256); }
                            }
                            else if (MRAM[_ActionCell] + RepCount <= byte.MaxValue)
                            {
                                MRAM[_ActionCell] += (byte)(RepCount);
                            }
                            else
                            {
                                OnError(VirtualMachineRunningErrorId.CellValueOutOfRange, out Reaction);
                                if (Reaction == VirtualMachineRunningErrorReaction.TryAgain)
                                    goto CURR_STEP;
                                else if (Reaction == VirtualMachineRunningErrorReaction.Miss)
                                    break;
                                else if (Reaction == VirtualMachineRunningErrorReaction.Pause)
                                    goto case 112; // p
                                else if (Reaction == VirtualMachineRunningErrorReaction.Break)
                                    goto case 98; // b
                                else
                                    throw new UnhandledMachineProgramRuntimeErrorException("Cell value fell out of the allowable range.", _NextSymbol, _ActionCell);
                            }
                            RepExp = ""; RepExpLen = 0; RepCount = 1;

                            break;
                        case 45: // -
                            if (_RoundValueRange)
                            {
                                unchecked { MRAM[_ActionCell] -= (byte)(RepCount % 256); }
                            }
                            else if (MRAM[_ActionCell] - RepCount >= 0)
                            {
                                MRAM[_ActionCell] -= (byte)(RepCount);
                            }
                            else
                            {
                                OnError(VirtualMachineRunningErrorId.CellValueOutOfRange, out Reaction);
                                if (Reaction == VirtualMachineRunningErrorReaction.TryAgain)
                                    goto CURR_STEP;
                                else if (Reaction == VirtualMachineRunningErrorReaction.Miss)
                                    break;
                                else if (Reaction == VirtualMachineRunningErrorReaction.Pause)
                                    goto case 112; // p
                                else if (Reaction == VirtualMachineRunningErrorReaction.Break)
                                    goto case 98; // b
                                else
                                    throw new UnhandledMachineProgramRuntimeErrorException("Cell value fell out of the allowable range.", _NextSymbol, _ActionCell);
                            }
                            RepExp = ""; RepExpLen = 0; RepCount = 1;

                            break;
                        case 44: // ,
                            if ((A + 2 < args.CodeStart + args.CodeLen) && (XString.bMid(MRAM, A, 3) == ",>{" || XString.bMid(MRAM, A, 3) == ",<{"))
                            {
                                A += 1;
                            }
                            else if (RepExp == ",>")
                            {
                                RepCount += 1;
                                _InputPorts[_ActionInputPortIndex].Input(out ioBuff, RepCount);
                                Array.Copy(ioBuff, 0, MRAM, _ActionCell, ioBuff.Length);
                                _ActionCell += ioBuff.Length;
                                A += 1;
                            }
                            else if (RepExp == ",<")
                            {
                                RepCount += 1;
                                _InputPorts[_ActionInputPortIndex].Input(out ioBuff, RepCount);
                                for (Y = 0; Y < RepCount; Y++)
                                    MRAM[_ActionCell - Y] = ioBuff[Y];
                                _ActionCell -= RepCount;
                                A += 1;
                            }
                            else
                            {
                                MRAM[_ActionCell] = _InputPorts[_ActionInputPortIndex].Input();
                            }
                            RepExp = ""; RepExpLen = 0; RepCount = 1;
                            break;
                        case 46: // .
                            if ((A + 2 < args.CodeStart + args.CodeLen) && (XString.bMid(MRAM, A, 3) == ".>{" || XString.bMid(MRAM, A, 3) == ".<{"))
                            {
                                A += 1;
                            }
                            else if (RepExp == ".>")
                            {
                                RepCount += 1;
                                ioBuff = new byte[RepCount];
                                Array.Copy(MRAM, _ActionCell, ioBuff, 0, RepCount);
                                _OutputPorts[_ActionOutputPortIndex].Output(ioBuff);
                                _ActionCell += ioBuff.Length;
                                A += 1;
                            }
                            else if (RepExp == ".<")
                            {
                                RepCount += 1;
                                ioBuff = new byte[RepCount];
                                for (Y = 0; Y < RepCount; Y++)
                                    ioBuff[Y] = MRAM[_ActionCell - Y];
                                _OutputPorts[_ActionOutputPortIndex].Output(ioBuff);
                                _ActionCell -= RepCount;
                                A += 1;
                            }
                            else
                            {
                                _OutputPorts[_ActionOutputPortIndex].Output(MRAM[_ActionCell]);
                            }
                            RepExp = ""; RepExpLen = 0; RepCount = 1;
                            break;
                        case 91: // [
                            L = XString.FindClosing(MRAM, A + 1, args.CodeLen - (A - args.CodeStart) - 1, '[', ']', "", "'" + (char)(34), "'" + (char)(34)) - A - 1;
                            callArgs = new RunArgsLine
                            {
                                CodeStart = A + 1,
                                CodeLen = L,
                                Cycle = -1,
                                FuncIndex = args.FuncIndex,
                                preFRAM = FRAM,
                                preFuncNum = args.preFuncNum + FuncNum,
                                preTagNum = args.preTagNum + TagNum,
                                preStatNum = args.preStatNum + StatNum,
                                preObjNum = args.preObjNum + ObjNum,
                            };
                            (argsStack as Stack<RunArgsLine>).Push(callArgs);
                            Run(argsStack);
                            callArgs = (argsStack as Stack<RunArgsLine>).Pop();
                            FuncNum += callArgs.outFuncNum; TagNum += callArgs.outTagNum; StatNum += callArgs.outStatNum; ObjNum += callArgs.outObjNum;

                            A += L + 1;
                            break;
                        case 34: // "
                            A += 1;
                            while (MRAM[A] != 34)
                            {
                                _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                                MRAM[_ActionCell] = MRAM[A];
                                _ActionCell += 1;
                                A += 1;
                            }
                            break;
                        case 35: // #
                            S = ""; M = "";
                            while (M != "#")
                            {
                                S += M;
                                A += 1;
                                M = "" + (char)(MRAM[A]);
                            }
                            if (S == ":")
                            {
                                for (Z = args.preTagNum; Z < args.preTagNum + TagNum; Z++)
                                {
                                    _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                                    if (_Tags[Z].Value == _ActionCell)
                                    {
                                        _Tags.RemoveAt(Z);
                                        TagNum -= 1;
                                    }
                                }
                            }
                            else if (XString.Right(S, 1) == ":")
                            {
                                while (_Tags.Count < args.preTagNum + TagNum + 1)
                                {
                                    _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                                    _Tags.Add(new bfsTag());
                                }
                                T.Identificator = S.Substring(1, S.Length - 1);
                                T.Value = _ActionCell;
                                _Tags[args.preTagNum + TagNum] = T;

                                TagNum += 1;
                            }
                            else
                            {
                                Y = _ActionCell;
                                if (XString.Left(S, 1) == "/")
                                {
                                    for (Z = 0; Z < args.preTagNum + TagNum; Z++)
                                    {
                                        _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                                        if (_Tags[Z].Identificator == S)
                                        {
                                            _ActionCell = _Tags[Z].Value;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    for (Z = args.preTagNum + TagNum - 1; Z >= 0; Z--)
                                    {
                                        _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                                        if (_Tags[Z].Identificator == S)
                                        {
                                            _ActionCell = _Tags[Z].Value;
                                            break;
                                        }
                                    }
                                }
                                L = XString.FindClosing(MRAM, A + 1, args.CodeLen - (A - args.CodeStart) - 1, '#', '\\', "@^ !*&%ARJ", "'" + (char)(34), "'" + (char)(34)) - A - 1;
                                callArgs = new RunArgsLine
                                {
                                    CodeStart = A + 1,
                                    CodeLen = L,
                                    Cycle = 1,
                                    FuncIndex = args.FuncIndex,
                                    preFRAM = FRAM,
                                    preFuncNum = args.preFuncNum + FuncNum,
                                    preTagNum = args.preTagNum + TagNum,
                                    preStatNum = args.preStatNum + StatNum,
                                    preObjNum = args.preObjNum + ObjNum,
                                };
                                (argsStack as Stack<RunArgsLine>).Push(callArgs);
                                Run(argsStack);
                                callArgs = (argsStack as Stack<RunArgsLine>).Pop();
                                FuncNum += callArgs.outFuncNum; TagNum += callArgs.outTagNum; StatNum += callArgs.outStatNum; ObjNum += callArgs.outObjNum;


                                A += L + 1;
                                _ActionCell = Y;
                            }
                            break;
                        case 64: // @
                            FuncNum += 1;
                            //ReDim Preserve Funcs[preFuncNum + FuncNum]
                            F.Reset();
                            while (_Funcs.Count < args.preFuncNum + FuncNum + 1)
                            {
                                _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                                _Funcs.Add(F);
                            }
                            DeclareFunc(args.preFuncNum + FuncNum, A);
                            A += _Funcs[args.preFuncNum + FuncNum].CodeLen + 1;
                            MRAM[_ActionCell] = (byte)(args.preFuncNum + FuncNum);
                            break;
                        case 40: // (
                            FRAMAC = _ActionCell;
                            _ActionCell = FRAM + _Funcs[args.FuncIndex].CallRAM + CRAMAC;
                            break;
                        case 41: // )
                            CRAMAC = _ActionCell - FRAM - _Funcs[args.FuncIndex].CallRAM;
                            _ActionCell = FRAMAC;
                            break;
                        case 102: // f
                            I = MRAM[FRAM + _Funcs[args.FuncIndex].CallRAM];
                            Y = _ActionCell;
                            _ActionCell = FRAM + _Funcs[args.FuncIndex].CallRAM + 1;
                            //Run(0, 0, 0, I, -1, preFuncNum + outFuncNum, preTagNum + TagNum, preStatNum + StatNum, preObjNum + ObjNum,
                            //    ref FN, ref TN, ref SN, ref ObN);
                            callArgs = new RunArgsLine
                            {
                                CodeStart = 0,
                                CodeLen = 0,
                                Cycle = 0,
                                FuncIndex = I,
                                preFRAM = FRAM,
                                preFuncNum = args.preFuncNum + FuncNum,
                                preTagNum = args.preTagNum + TagNum,
                                preStatNum = args.preStatNum + StatNum,
                                preObjNum = args.preObjNum + ObjNum,
                            };
                            (argsStack as Stack<RunArgsLine>).Push(callArgs);
                            Run(argsStack);
                            callArgs = (argsStack as Stack<RunArgsLine>).Pop();
                            FuncNum += callArgs.outFuncNum; TagNum += callArgs.outTagNum; StatNum += callArgs.outStatNum; ObjNum += callArgs.outObjNum;


                            _ActionCell = Y;
                            MRAM[_ActionCell] = MRAM[FRAM + _Funcs[args.FuncIndex].CallRAM + _Funcs[I].LoclSet - 1];
                            break;
                        case 123: // {
                            S = ""; M = ""; Z = A;
                            while (M != "}")
                            {
                                _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                                S += M;
                                A += 1;
                                M = "" + (char)(MRAM[A]);
                            }
                            //
                            RepExp = ""; RepExpLen = 1;
                            while (XString.bMid(MRAM, Z - RepExpLen, RepExpLen) != XString.bMid(MRAM, A + 1, RepExpLen))
                            {
                                _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                                RepExpLen += 1;
                            }
                            RepExp = XString.bMid(MRAM, Z - RepExpLen, RepExpLen);
                            //
                            if (S.Length == 3 && S[0] == (char)(34) && S[2] == (char)(34))
                            {
                                RepCount = (byte)S[1] - 1;
                            }
                            else if (S == "")
                            {
                                RepCount = MRAM[_ActionCell - XString.CountLevel(RepExp, 1, ups: ">", downs: "<")] - 1;
                            }
                            else if (S == "##")
                            {
                                RepCount = _ActionCell - XString.CountLevel(RepExp, 1, ups: ">", downs: "<") - 1;
                            }
                            else if (XString.Left(S, 1) == "#" && XString.Right(S, 1) == "#")
                            {
                                S = XString.Left(S, S.Length - 2);
                                for (Z = 0; Z < args.preTagNum + TagNum; Z++)
                                {
                                    _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                                    if (_Tags[Z].Identificator == S)
                                    {
                                        RepCount = _Tags[Z].Value - 1;
                                        break;
                                    }
                                }
                            }
                            else if (int.TryParse(S, out Z))
                            {
                                RepCount = Z - 1;
                            }
                            else
                            {
                                for (Z = args.preTagNum + TagNum - 1; Z >= 0; Z--)
                                {
                                    _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                                    if (_Tags[Z].Identificator == S)
                                    {
                                        RepCount = MRAM[_Tags[Z].Value] - 1;
                                        break;
                                    }
                                }
                            }
                            //
                            if (RepCount < 0)
                            {
                                RepCount = 0;
                                if (XString.ReplaceChars(MRAM, A + 1, RepExpLen, "+-><", "-+<>") == RepExpLen)
                                {
                                    //Run(A + 1, RepExpLen, 1, args.FuncIndex, FRAM, preFuncNum + FuncNum, preTagNum + TagNum, preStatNum + StatNum, preObjNum + ObjNum,
                                    //    ref FN, ref TN, ref SN, ref ObN);
                                    //FuncNum += FN; TagNum += TN; StatNum += SN; ObjNum += ObN;
                                    callArgs = new RunArgsLine
                                    {
                                        CodeStart = A + 1,
                                        CodeLen = RepExpLen,
                                        Cycle = 1,
                                        FuncIndex = args.FuncIndex,
                                        preFRAM = FRAM,
                                        preFuncNum = args.preFuncNum + FuncNum,
                                        preTagNum = args.preTagNum + TagNum,
                                        preStatNum = args.preStatNum + StatNum,
                                        preObjNum = args.preObjNum + ObjNum,
                                    };
                                    (argsStack as Stack<RunArgsLine>).Push(callArgs);
                                    Run(argsStack);
                                    callArgs = (argsStack as Stack<RunArgsLine>).Pop();
                                    FuncNum += callArgs.outFuncNum; TagNum += callArgs.outTagNum; StatNum += callArgs.outStatNum; ObjNum += callArgs.outObjNum;

                                    XString.ReplaceChars(MRAM, A + 1, RepExpLen, "-+<>", "+-><");
                                    A += RepExpLen;
                                }
                            }
                            else if (!(RepExp == "+" || RepExp == "-" || RepExp == "." || RepExp == ","
                                    || RepExp == ">" || RepExp == "<"
                                    || RepExp == ".>" || RepExp == ".<" || RepExp == ",>" || RepExp == ",<"))
                            {
                                //Run(A + 1, RepExpLen, Repeat, FuncIndex, FRAM, preFuncNum + FuncNum, preTagNum + TagNum, preStatNum + StatNum, preObjNum + ObjNum,
                                //    ref FN, ref TN, ref SN, ref ObN);
                                //FuncNum += FN; TagNum += TN; StatNum += SN; ObjNum += ObN;
                                callArgs = new RunArgsLine
                                {
                                    CodeStart = A + 1,
                                    CodeLen = RepExpLen,
                                    Cycle = RepCount,
                                    FuncIndex = args.FuncIndex,
                                    preFRAM = FRAM,
                                    preFuncNum = args.preFuncNum + FuncNum,
                                    preTagNum = args.preTagNum + TagNum,
                                    preStatNum = args.preStatNum + StatNum,
                                    preObjNum = args.preObjNum + ObjNum,
                                };
                                (argsStack as Stack<RunArgsLine>).Push(callArgs);
                                Run(argsStack);
                                callArgs = (argsStack as Stack<RunArgsLine>).Pop();
                                FuncNum += callArgs.outFuncNum; TagNum += callArgs.outTagNum; StatNum += callArgs.outStatNum; ObjNum += callArgs.outObjNum;

                                A += RepExpLen;
                            }
                            break;
                        case 39: // '
                            do
                            {
                                _RunningCancellationTokenSource.Token.ThrowIfCancellationRequested();
                                A += 1;
                            } while (MRAM[A] != 39); // '
                            break;
                        case 48: // 0
                            MRAM[_ActionCell] = 0;
                            break;
                        case 114: // r
                            _ActionCell = FRAM;
                            break;
                        case 106: // j
                            _ActionCell = FRAM + _Funcs[args.FuncIndex].LoclSet;
                            break;
                        case 112: // p
                            _Status = VirtualMachineRunningStatus.Pausing;
                            OnStatusChanged(VirtualMachineRunningStatus.Paused);
                            break;
                        case 98: // b
                            _Status = VirtualMachineRunningStatus.Breaking;
                            OnStatusChanged(VirtualMachineRunningStatus.Breaking);
                            break;
                        case 59: // ;
                            ActionInputPortIndex = MRAM[_ActionCell];
                            if (InputPorts[_ActionInputPortIndex] != null)
                                MRAM[_ActionCell] = InputPorts[_ActionInputPortIndex].Status;
                            else
                                throw new NullReferenceToActionIOPortException
                                        ("Null reference to Action Input Port.", _NextSymbol, _ActionCell);
                            break;
                        case 58: // :
                            if (InputPorts[MRAM[_ActionCell]] != null)
                            {
                                ActionOutputPortIndex = MRAM[_ActionCell];
                                MRAM[_ActionCell] = OutputPorts[_ActionOutputPortIndex].Status;
                            }
                            else
                                throw new NullReferenceToActionIOPortException
                                        ("Null reference to Action Output Port.", _NextSymbol, _ActionCell);
                            break;
                        case 93: // ]
                            throw new UnexpectedProgramStatementException
                                    ($"Unexpected program statement symbol \"]\".", _NextSymbol, _ActionCell);
                        case 125: // }
                            throw new UnexpectedProgramStatementException
                                    ($"Unexpected program statement symbol \"}}\".", _NextSymbol, _ActionCell);
                    }
                    if (runFin1step)
                    {
                        runFin1step = false;
                        _Status = VirtualMachineRunningStatus.Pausing;
                    }
                    D += 1;
                    if (D > InterruptingPeriod)
                    {
                        Thread.Yield();
                        Application.DoEvents();
                        D = 0;
                    }
                }
                if (args.Cycle > 0) args.Cycle -= 1;
            } while (args.Cycle == -1 && (MRAM[_ActionCell] != 0) || (args.Cycle > 0));

            args.outFuncNum = FuncNum; args.outTagNum = TagNum; args.outStatNum = StatNum; args.outObjNum = ObjNum;
            (argsStack as Stack<RunArgsLine>).Push(args);
        }

        [MenuBrowsable(true), Description("Sayann hheeeeeeyy theereee.")]
        public virtual void SayHeeeeeeyDErrrR()
        {
            System.Windows.Forms.MessageBox.Show("HeeeeeeyDErrrR, its " + this.Title);
        }
    }
}
