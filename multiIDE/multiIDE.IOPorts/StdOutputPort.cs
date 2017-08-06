using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XSpace;

namespace multiIDE.IOPorts
{
    /// <summary>
    /// Provides standard IO mediation between machine and IO devices.
    /// </summary>
    public class StdOutputPort : IOutputPort
    {
        #region Enums
        [Flags]
        public enum OutputDeviceThreadSafe : int
        {
            Nothing = 0x00,
            Initialize = 0x01,
            Dispose = 0x02,
            Output = 0x04,
            All = Initialize | Dispose | Output
        }
        //
        public enum OutputConvertings : int
        {
            [Description("Not Assigned.")]
            NA = -1,
            [Description("Output performes without converting.")]
            None = 1,
            [Description("Outputing bytes as string bytes with converting from ParentIDE.Machine Encoding to OutputDevice's one.")]
            String = 2,
            [Description("Output bytes is combining as bytes of binary integer number due to ParentIDE.Machine's parameters then converts to string of OutputDevice Encoding.")]
            ParsingNumber = 3
        }
        #endregion

        #region Profile properties
        [Category("Profile"), ReadOnly(true)]
        public virtual string DefaultName => "Standard Output Port";
        //
        [Category("Profile"), ReadOnly(true)]
        public virtual string Version => "1.0.0.0";
        //
        [Category("Profile"), ReadOnly(true)]
        public virtual string Author => "";
        //
        [Category("Profile"), ReadOnly(true)]
        public virtual string Description => "Provides standard output interaction.";
        #endregion

        #region Environment properties
        [Category("Environment"), ReadOnly(true)]
        public virtual IIDE ParentIDE { get; set; }
        //
        [Category("Environment"), ReadOnly(true)]
        public virtual string CustomName { get; set; } = "";
        //
        [Category("Environment"), ReadOnly(true)]
        public virtual int Id { get; set; } = CommonConstants.UndefinedComponentId;
        //
        [Category("Appearance"), DefaultValue("")]
        public virtual string Title
        {
            get
            {
                return _CustomTitle.IsNotNullOrEmpty() ? _CustomTitle
                    : (_OutputDevice != null ? $"{_OutputDevice.DefaultName}~{_OutputDevice.Id}" : "(empty output port)");
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
        [Category("Essential")]
        public virtual IOutputDevice OutputDevice
        {
            get { return _OutputDevice; }
            set
            {
                try
                {
                    IOmonitor.IdleForLock(_IOprocessing);
                    //
                    _OutputDevice = value;
                    _OutputDeviceIsThreadSafeFor = OutputDeviceThreadSafe.Nothing;
                    if (_OutputDevice != null)
                    {
                        _OutputDeviceIsThreadSafeFor |= !_OutputDevice.IsLockingRequiredForInitialize()
                                ? OutputDeviceThreadSafe.Initialize : OutputDeviceThreadSafe.Nothing;
                        _OutputDeviceIsThreadSafeFor |= !_OutputDevice.IsLockingRequiredForDispose()
                                ? OutputDeviceThreadSafe.Dispose : OutputDeviceThreadSafe.Nothing;
                        _OutputDeviceIsThreadSafeFor |= !_OutputDevice.IsLockingRequiredForOutput()
                                ? OutputDeviceThreadSafe.Output : OutputDeviceThreadSafe.Nothing;
                    }
                    OutputForeword = _OutputForeword;
                    OutputAfterword = _OutputAfterword;
                    Separators = _Separators;
                }
                finally
                {
                    //
                    IOmonitor.Unlock(_IOprocessing);
                }
            }
        }
        //
        [Category("Essential"), ReadOnly(true)]
        public virtual byte Status
        {
            get { return _OutputDevice?.GetStatus(this) ?? 255; }
        }
        //
        [Category("Essential"), ReadOnly(false)]
        public virtual object OutputConverting
        {
            get { return _OutputConverting; }
            set
            {
                try
                {
                    IOmonitor.IdleForLock(_IOprocessing);
                    //
                    _OutputConverting = (OutputConvertings)value;
                }
                finally
                {
                    //
                    IOmonitor.Unlock(_IOprocessing);
                }
            }
        }
        //
        [Category("Essential"), ReadOnly(false)]
        public bool OutputAsyncAllowed
        {
            get { return _OutputAsyncAllowed; }
            set
            {
                try
                {
                    IOmonitor.IdleForLock(_IOprocessing);
                    //
                    _OutputAsyncAllowed = value;
                    lock (_OutputBuffer)
                        _OutputBuffer = new List<byte>();
                    _BackgroundOutputTask = null;
                }
                finally
                {
                    //
                    IOmonitor.Unlock(_IOprocessing);
                }
            }
        }
        //
        [Category("Format"), ReadOnly(false)]
        public virtual char[] Separators
        {
            get { return (char[])_Separators.Clone(); }
            set
            {
                try
                {
                    IOmonitor.IdleForLock(_IOprocessing);
                    //
                    _Separators = value;
                    _bOutSeparator = _OutputDevice != null ? Encoding.Convert(Encoding.Unicode, _OutputDevice.CharSet, Encoding.Unicode.GetBytes(new char[] { _Separators[0] })) : null;
                }
                finally
                {
                    //
                    IOmonitor.Unlock(_IOprocessing);
                }
            }
        }
        //
        [Category("Format"), ReadOnly(false)]
        public virtual string OutputForeword
        {
            get
            {
                return _OutputForeword.Replace(Environment.NewLine, "\\n")
                        .Replace(this.Id.ToString(), "\\p").Replace(ParentIDE.Machine.Id.ToString(), "\\m");
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    _OutputForeword = "";
                    _bOutputForeword = null;
                    return;
                }
                _OutputForeword = value.Replace(Environment.NewLine, "\\n");
                byte[] uniBytes = Encoding.Unicode.GetBytes(_OutputForeword.Replace("\\n", Environment.NewLine)
                    .Replace("\\p", this.Id.ToString()).Replace("\\m", ParentIDE.Machine.Id.ToString()));
                _bOutputForeword = Encoding.Convert(Encoding.Unicode, OutputDevice.CharSet, uniBytes);
            }
        }
        //
        [Category("Format"), ReadOnly(false)]
        public virtual string OutputAfterword
        {
            get
            {
                return _OutputAfterword.Replace(Environment.NewLine, "\\n")
                        .Replace(this.Id.ToString(), "\\p").Replace(ParentIDE.Machine.Id.ToString(), "\\m");
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    _OutputAfterword = "";
                    _bOutputAfterword = null;
                    return;
                }
                _OutputAfterword = value.Replace(Environment.NewLine, "\\n");
                byte[] uniBytes = Encoding.Unicode.GetBytes(_OutputAfterword.Replace("\\n", Environment.NewLine)
                    .Replace("\\p", this.Id.ToString()).Replace("\\m", ParentIDE.Machine.Id.ToString()));
                _bOutputAfterword = Encoding.Convert(Encoding.Unicode, OutputDevice.CharSet, uniBytes);
            }
        }
        //
        protected IOutputDevice _OutputDevice;
        protected OutputConvertings _OutputConverting = OutputConvertings.ParsingNumber;
        protected bool _OutputAsyncAllowed = true;
        protected List<byte> _OutputBuffer = new List<byte>();
        protected Task _BackgroundOutputTask;
        protected char[] _Separators = new char[] { ' ', ';' };
        protected byte[] _bOutSeparator;
        protected string _OutputForeword = "";
        protected string _OutputAfterword = "";
        protected byte[] _bOutputForeword;
        protected byte[] _bOutputAfterword;
        #endregion

        #region Technicals
        [Category("Technical"), ReadOnly(true), Browsable(true)]
        public OutputDeviceThreadSafe OutputDeviceIsThreadSafeFor => _OutputDeviceIsThreadSafeFor;
        //
        protected OutputDeviceThreadSafe _OutputDeviceIsThreadSafeFor = OutputDeviceThreadSafe.Nothing;
        #endregion

        public override string ToString() => (CustomName.IsNotNullOrEmpty()) ? CustomName
                : $"{ParentIDE.Machine} : {DefaultName} {Id}";

        /////
        protected object _IOprocessing = new object();
        protected Action _BackgroundOutputAction;

        public StdOutputPort()
        {
            _BackgroundOutputAction = () =>
            {
                bool locked = false;
                bool __outBuffDone = false;
                Thread.CurrentThread.Name = $"  machine~{ParentIDE.Machine.Id}.outputPort~{this.Id} Buffer Thread";
                //Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
                while (!__outBuffDone)
                {
                    while (!(_OutputDevice != null && _OutputDevice.IsInitialized))
                        Thread.Yield();

                    lock (_OutputDevice.Locking)
                        if (_OutputDevice.IsReadyToOutput)
                            lock (_OutputBuffer)
                            {
                                _OutputDevice.Output(this, _OutputBuffer.ToArray());
                                _OutputBuffer.Clear();
                                _BackgroundOutputTask = null;
                                __outBuffDone = true;
                            }
                }
            };
        }

        public StdOutputPort(IVirtualMachine parentMachine) : this()
        {
            ParentIDE.Machine = parentMachine;
        }

        public StdOutputPort(IVirtualMachine parentMachine, int associatedPortIndex
                , IOutputDevice outputPort) : this()
        {
            ParentIDE.Machine = parentMachine;
            Id = associatedPortIndex;

            if (outputPort != null)
            {
                OutputDevice = outputPort;
            }
            else
            {
                throw new Exception();
            }
        }

        public virtual void Output(byte[] outData)
        {
            try
            {
                IOmonitor.IdleForLock(_IOprocessing);
                //
                byte[] outdevData;

                switch (_OutputConverting)
                {
                    case OutputConvertings.String:
                        outdevData = Encoding.Convert(ParentIDE.Machine.CharSet, _OutputDevice.CharSet
                                , outData);
                        break;
                    case OutputConvertings.ParsingNumber:
                        outdevData = XBinary.ConvertBinaryNumberBytesToTextNumberBytes(outData
                                , ParentIDE.Machine.ByteOrder, _OutputDevice.CharSet);
                        Array.Resize<byte>(ref outdevData, outdevData.Length + _bOutSeparator.Length);
                        Array.Copy(_bOutSeparator, 0, outdevData, outdevData.Length
                                - _bOutSeparator.Length, _bOutSeparator.Length);
                        break;
                    case OutputConvertings.None:
                        outdevData = outData;
                        break;
                    default:
                        throw new Exception();
                }

                bool outDone, locked;

                if (!OutputAsyncAllowed)
                {
                    locked = false;
                    if ((OutputDeviceThreadSafe.Initialize & _OutputDeviceIsThreadSafeFor) == 0)
                    {
                        IOmonitor.IdleForLock(_OutputDevice, out locked);
                        if (!_OutputDevice.IsInitialized) _OutputDevice.Initialize(this);
                    }
                    else
                        if (!_OutputDevice.IsInitialized) _OutputDevice.Initialize(this);

                    if (!locked && (OutputDeviceThreadSafe.Output & _OutputDeviceIsThreadSafeFor) == 0) IOmonitor.IdleForLock(_OutputDevice, out locked);
                    if (_bOutputForeword != null) _OutputDevice.Output(this, _bOutputForeword);
                    _OutputDevice.Output(this, outdevData);
                    if (_bOutputAfterword != null) _OutputDevice.Output(this, _bOutputAfterword);
                    if (locked) IOmonitor.Unlock(_OutputDevice);
                }
                else
                // using output buffer:
                {
                    if (_BackgroundOutputTask == null)
                    // I. if buffer is empty
                    {
                        locked = false;
                        if ((OutputDeviceThreadSafe.Initialize & _OutputDeviceIsThreadSafeFor) == 0)
                        {
                            IOmonitor.TryLock(_OutputDevice, out locked);
                            if (locked && !_OutputDevice.IsInitialized) _OutputDevice.Initialize(this);
                        }
                        else
                            if (!_OutputDevice.IsInitialized) _OutputDevice.Initialize(this);

                        // try instant output synchronously
                        outDone = false;
                        if ((OutputDeviceThreadSafe.Output & _OutputDeviceIsThreadSafeFor) == 0)
                        {
                            if (locked || IOmonitor.TryLock(_OutputDevice))
                            {
                                if (_OutputDevice.IsReadyToOutput)
                                {
                                    if (_bOutputForeword != null) _OutputDevice.Output(this, _bOutputForeword);
                                    _OutputDevice.Output(this, outdevData);
                                    if (_bOutputAfterword != null) _OutputDevice.Output(this, _bOutputAfterword);
                                    outDone = true;
                                }
                                IOmonitor.Unlock(_OutputDevice);
                            }
                        }
                        else
                         if (_OutputDevice.IsReadyToOutput)
                        {
                            if (_bOutputForeword != null) _OutputDevice.Output(this, _bOutputForeword);
                            _OutputDevice.Output(this, outdevData);
                            if (_bOutputAfterword != null) _OutputDevice.Output(this, _bOutputAfterword);
                            outDone = true;
                        }

                        // if sync output didn't occure, starting async task for outputting buffer
                        if (!outDone)
                        {
                            lock (_OutputBuffer)
                            {
                                _OutputBuffer.AddRange(outdevData);
                                _BackgroundOutputTask = Task.Run(_BackgroundOutputAction);
                            }
                        }
                    }
                    else
                    // II. if buffer is not empty
                    {
                        // initializing output device if it needs to
                        if ((OutputDeviceThreadSafe.Initialize & _OutputDeviceIsThreadSafeFor) == 0)
                        {
                            if (IOmonitor.TryLock(_OutputDevice))
                            {
                                if (!_OutputDevice.IsInitialized) _OutputDevice.Initialize(this);
                                IOmonitor.Unlock(_OutputDevice);
                            }
                        }
                        else
                            if (!_OutputDevice.IsInitialized) _OutputDevice.Initialize(this);

                        // populating buffer
                        lock (_OutputBuffer)
                            _OutputBuffer.AddRange(outdevData);
                    }
                }
            }
            finally
            {
                IOmonitor.Unlock(_OutputDevice);
                //
                IOmonitor.Unlock(_IOprocessing);
            }
        }

        public virtual void Output(byte outByte)
        {
            Output(new byte[] { outByte });
        }

        public virtual void Close()
        {
            bool locked = false;
            try
            {
                IOmonitor.IdleForLock(_IOprocessing);
                //
                if (_OutputDevice != null && _OutputDevice.IsInitialized && _OutputDevice.InitializedBy == ParentIDE.Machine)
                {
                    locked = false;
                    if (_OutputDevice.IsLockingRequiredForDispose()) IOmonitor.IdleForLock(_OutputDevice, out locked);
                    _OutputDevice.Dispose();
                    if (locked) IOmonitor.Unlock(_OutputDevice);
                }
                _OutputBuffer.Clear();
            }
            finally
            {
                if (locked) IOmonitor.Unlock(_OutputDevice);
                //
                IOmonitor.Unlock(_IOprocessing);
            }
        }

        [MenuBrowsable(true), Description("Sayann hheeeeeeyy theereee.")]
        public virtual void SayHeeeeeeyDErrrR()
        {
            System.Windows.Forms.MessageBox.Show("HeeeeeeyDErrrR, its " + this.Title);
        }
    }
}
