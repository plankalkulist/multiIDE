using System;
using System.ComponentModel;
using System.Text;
using XSpace;

namespace multiIDE.IOPorts
{
    /// <summary>
    /// Provides standard IO mediation between machine and IO devices.
    /// </summary>
    public class StdInputPort : IInputPort
    {
        #region IO Convertings enums
        [Flags]
        public enum InputDeviceThreadSafe : int
        {
            Nothing = 0x00,
            Initialize = 0x01,
            Dispose = 0x02,
            Input = 0x04,
            All = Initialize | Dispose | Input
        }
        //
        public enum InputConvertings : int
        {
            [Description("Not Assigned.")]
            NA = -1,
            [Description("Input performes without converting.")]
            None = 1,
            [Description("Inputing bytes as string bytes with converting from InputDevice Encoding to ParentIDE.Machine's one.")]
            String = 2,
            [Description("Input bytes is parsing as string of InputDevice Encoding then converts to bytes as binary integer number due to ParentIDE.Machine's parameters.")]
            ParsingNumber = 3
        }
        #endregion

        #region Profile properties
        [Category("Profile"), ReadOnly(true)]
        public virtual string DefaultName => "Standard Input Port";
        //
        [Category("Profile"), ReadOnly(true)]
        public virtual string Version => "1.0.0.0";
        //
        [Category("Profile"), ReadOnly(true)]
        public virtual string Author => "";
        //
        [Category("Profile"), ReadOnly(true)]
        public virtual string Description => "Provides standard input interaction.";
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
                    : (_InputDevice != null ? $"{_InputDevice.DefaultName}~{_InputDevice.Id}" : "(empty input port)");
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
        public virtual IInputDevice InputDevice
        {
            get { return _InputDevice; }
            set
            {
                try
                {
                    IOmonitor.IdleForLock(_IOprocessing);
                    //
                    _InputDevice = value;
                    _InputDeviceIsThreadSafeFor = InputDeviceThreadSafe.Nothing;
                    if (_InputDevice != null)
                    {
                        _InputDeviceIsThreadSafeFor |= !_InputDevice.IsLockingRequiredForInitialize()
                                ? InputDeviceThreadSafe.Initialize : InputDeviceThreadSafe.Nothing;
                        _InputDeviceIsThreadSafeFor |= !_InputDevice.IsLockingRequiredForDispose()
                                ? InputDeviceThreadSafe.Dispose : InputDeviceThreadSafe.Nothing;
                        _InputDeviceIsThreadSafeFor |= !_InputDevice.IsLockingRequiredForInput()
                                ? InputDeviceThreadSafe.Input : InputDeviceThreadSafe.Nothing;
                    }
                    InputForeword = _InputForeword;
                    InputAfterword = _InputAfterword;
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
            get { return _InputDevice?.GetStatus(this) ?? 255; }
        }
        //
        [Category("Essential"), ReadOnly(false)]
        public virtual object InputConverting
        {
            get { return _InputConverting; }
            set
            {
                try
                {
                    IOmonitor.IdleForLock(_IOprocessing);
                    //
                    UpdateInputBuffers((InputConvertings)value);
                    _InputConverting = (InputConvertings)value;
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
        public bool InputFromBufferAllowed
        {
            get { return _InputFromBufferAllowed; }
            set
            {
                try
                {
                    IOmonitor.IdleForLock(_IOprocessing);
                    //
                    _InputFromBufferAllowed = value;
                    _InputBuffer = new byte[0];
                    _InBuffPtr = 0;
                    _InputWordBuffer = new string[0];
                    _WdBuffPtr = 0;
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
                    _bInSeparator = _InputDevice != null ? Encoding.Convert(Encoding.Unicode, _InputDevice.CharSet, Encoding.Unicode.GetBytes(new char[] { _Separators[0] })) : null;
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
        public virtual string InputForeword
        {
            get
            {
                return _InputForeword.Replace(Environment.NewLine, "\\n")
                        .Replace(this.Id.ToString(), "\\p").Replace(ParentIDE.Machine.Id.ToString(), "\\m");
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    _InputForeword = "";
                    _bInputForeword = null;
                    return;
                }
                _InputForeword = value.Replace(Environment.NewLine, "\\n");
                if (InputDevice is IOutputDevice)
                {
                    byte[] uniBytes = Encoding.Unicode.GetBytes(_InputForeword.Replace("\\n", Environment.NewLine)
                        .Replace("\\p", this.Id.ToString()).Replace("\\m", ParentIDE.Machine.Id.ToString()));
                    _bInputForeword = Encoding.Convert(Encoding.Unicode, InputDevice.CharSet, uniBytes);
                }
                else
                {
                    _bInputForeword = null;
                }
            }
        }
        //
        [Category("Format"), ReadOnly(false)]
        public virtual string InputAfterword
        {
            get
            {
                return _InputAfterword.Replace(Environment.NewLine, "\\n")
                        .Replace(this.Id.ToString(), "\\p").Replace(ParentIDE.Machine.Id.ToString(), "\\m");
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    _InputAfterword = "";
                    _bInputAfterword = null;
                    return;
                }
                _InputAfterword = value.Replace(Environment.NewLine, "\\n");
                if (InputDevice is IOutputDevice)
                {
                    byte[] uniBytes = Encoding.Unicode.GetBytes(_InputAfterword.Replace("\\n", Environment.NewLine)
                        .Replace("\\p", this.Id.ToString()).Replace("\\m", ParentIDE.Machine.Id.ToString()));
                    _bInputAfterword = Encoding.Convert(Encoding.Unicode, InputDevice.CharSet, uniBytes);
                }
                else
                {
                    _bInputAfterword = null;
                }
            }
        }
        //
        protected IInputDevice _InputDevice;
        protected InputConvertings _InputConverting = InputConvertings.ParsingNumber;
        protected bool _InputFromBufferAllowed = true;
        protected byte[] _InputBuffer = new byte[0];
        protected int _InBuffPtr = 0;
        protected string[] _InputWordBuffer = new string[0];
        protected int _WdBuffPtr = 0;
        protected char[] _Separators = new char[] { ' ', ';' };
        protected byte[] _bInSeparator;
        protected string _InputForeword = "m\\m.p\\p<?:";
        protected string _InputAfterword = "\\n";
        protected byte[] _bInputForeword;
        protected byte[] _bInputAfterword;
        #endregion

        #region Technicals
        [Category("Technical"), ReadOnly(true), Browsable(true)]
        public InputDeviceThreadSafe InputDeviceIsThreadSafeFor => _InputDeviceIsThreadSafeFor;
        //
        protected InputDeviceThreadSafe _InputDeviceIsThreadSafeFor = InputDeviceThreadSafe.Nothing;
        #endregion

        public override string ToString() => (CustomName.IsNotNullOrEmpty()) ? CustomName
                : $"{ParentIDE.Machine} : {DefaultName} {Id}";

        /////
        protected object _IOprocessing = new object();

        public StdInputPort()
        { }

        public StdInputPort(IVirtualMachine parentMachine) : this()
        {
            ParentIDE.Machine = parentMachine;
        }

        public StdInputPort(IVirtualMachine parentMachine, int associatedPortIndex
                , IInputDevice inputPort) : this()
        {
            ParentIDE.Machine = parentMachine;
            Id = associatedPortIndex;

            if (inputPort != null)
            {
                InputDevice = inputPort;
            }
            else
            {
                throw new Exception();
            }
        }

        protected virtual void UpdateInputBuffers(InputConvertings upToNewInputConverting)
        {
            if (InputFromBufferAllowed)
            {
                // converting input buffer
                InputConvertings inputConverting = _InputConverting;
                InputConvertings newInputConverting = upToNewInputConverting;

                switch (inputConverting)
                {
                    case InputConvertings.None:
                        switch (newInputConverting)
                        {
                            case InputConvertings.None:
                                break;
                            case InputConvertings.String:
                                _InputBuffer = Encoding.Convert(_InputDevice.CharSet, ParentIDE.Machine.CharSet, _InputBuffer, _InBuffPtr, _InputBuffer.Length - _InBuffPtr);
                                _InBuffPtr = 0;
                                break;
                            case InputConvertings.ParsingNumber:
                                byte[] unicodeLineBytes = Encoding.Convert(_InputDevice.CharSet, Encoding.Unicode, _InputBuffer, _InBuffPtr, _InputBuffer.Length - _InBuffPtr);
                                char[] unicodeLineChars = new char[Encoding.Unicode.GetCharCount(unicodeLineBytes, 0, unicodeLineBytes.Length)];
                                Encoding.Unicode.GetChars(unicodeLineBytes, 0, unicodeLineBytes.Length, unicodeLineChars, 0);
                                string newLine = new string(unicodeLineChars);
                                _InputWordBuffer = newLine.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                                _WdBuffPtr = 0;
                                _InputBuffer = new byte[0];
                                _InBuffPtr = 0;
                                break;
                            default:
                                _InputBuffer = new byte[0];
                                _InBuffPtr = 0;
                                _InputWordBuffer = new string[0];
                                _WdBuffPtr = 0;
                                break;
                        }
                        break;
                    case InputConvertings.String:
                        switch (newInputConverting)
                        {
                            case InputConvertings.None:
                                _InputBuffer = Encoding.Convert(ParentIDE.Machine.CharSet, _InputDevice.CharSet, _InputBuffer, _InBuffPtr, _InputBuffer.Length - _InBuffPtr);
                                _InBuffPtr = 0;
                                break;
                            case InputConvertings.String:
                                break;
                            case InputConvertings.ParsingNumber:
                                byte[] unicodeLineBytes = Encoding.Convert(ParentIDE.Machine.CharSet, Encoding.Unicode, _InputBuffer, _InBuffPtr, _InputBuffer.Length - _InBuffPtr);
                                char[] unicodeLineChars = new char[Encoding.Unicode.GetCharCount(unicodeLineBytes, 0, unicodeLineBytes.Length)];
                                Encoding.Unicode.GetChars(unicodeLineBytes, 0, unicodeLineBytes.Length, unicodeLineChars, 0);
                                string newLine = new string(unicodeLineChars);
                                _InputWordBuffer = newLine.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                                _WdBuffPtr = 0;
                                _InputBuffer = new byte[0];
                                _InBuffPtr = 0;
                                break;
                            default:
                                _InputBuffer = new byte[0];
                                _InBuffPtr = 0;
                                _InputWordBuffer = new string[0];
                                _WdBuffPtr = 0;
                                break;
                        }
                        break;
                    case InputConvertings.ParsingNumber:
                        switch (newInputConverting)
                        {
                            case InputConvertings.None:
                                string lineBuffer = String.Join(Separators[0] + "", _InputWordBuffer, _WdBuffPtr, _InputWordBuffer.Length - _WdBuffPtr);
                                byte[] unicodeLineBufferBytes = Encoding.Unicode.GetBytes(lineBuffer);
                                _InputBuffer = Encoding.Convert(Encoding.Unicode, _InputDevice.CharSet, unicodeLineBufferBytes);
                                _InBuffPtr = 0;
                                _InputWordBuffer = new string[0];
                                _WdBuffPtr = 0;
                                break;
                            case InputConvertings.String:
                                lineBuffer = String.Join(Separators[0] + "", _InputWordBuffer, _WdBuffPtr, _InputWordBuffer.Length - _WdBuffPtr);
                                unicodeLineBufferBytes = Encoding.Unicode.GetBytes(lineBuffer);
                                _InputBuffer = Encoding.Convert(Encoding.Unicode, ParentIDE.Machine.CharSet, unicodeLineBufferBytes);
                                _InBuffPtr = 0;
                                _InputWordBuffer = new string[0];
                                _WdBuffPtr = 0;
                                break;
                            case InputConvertings.ParsingNumber:
                                break;
                            default:
                                _InputBuffer = new byte[0];
                                _InBuffPtr = 0;
                                _InputWordBuffer = new string[0];
                                _WdBuffPtr = 0;
                                break;
                        }
                        break;
                    default:
                        _InputBuffer = new byte[0];
                        _InBuffPtr = 0;
                        _InputWordBuffer = new string[0];
                        _WdBuffPtr = 0;
                        break;
                }
            }
            else
            {
                _InputBuffer = new byte[0];
                _InBuffPtr = 0;
                _InputWordBuffer = new string[0];
                _WdBuffPtr = 0;
            }
        }

        protected virtual void DirectInput(out byte[] inData, int length)
        {
            bool locked = false;

            try
            {
                if ((InputDeviceThreadSafe.Initialize & _InputDeviceIsThreadSafeFor) == 0)
                {
                    IOmonitor.IdleForLock(_InputDevice, out locked);
                    if (!_InputDevice.IsInitialized) _InputDevice.Initialize(this);
                }
                else
                    if (!_InputDevice.IsInitialized) _InputDevice.Initialize(this);

                if (!locked && (InputDeviceThreadSafe.Input & _InputDeviceIsThreadSafeFor) == 0) IOmonitor.IdleForLock(_InputDevice, out locked);
                {
                    IOmonitor.IdleWhile(() => !_InputDevice.IsReadyToInput);
                    IOutputDevice ioDevice = _InputDevice as IOutputDevice;
                    if (ioDevice != null)
                    {
                        if (ioDevice.IsReadyToOutput && _bInputForeword != null)
                            ioDevice.Output(this, _bInputForeword);

                        _InputDevice.Input(this, out inData, length);

                        if (ioDevice.IsReadyToOutput && _bInputAfterword != null)
                            ioDevice.Output(this, _bInputAfterword);
                    }
                    else
                        _InputDevice.Input(this, out inData, length);
                }
            }
            finally
            {
                if (locked) IOmonitor.Unlock(_InputDevice);
            }
        }

        public virtual int Input(out byte[] inData, int length)
        {
            try
            {
                IOmonitor.IdleForLock(_IOprocessing);
                //
                int resultLength = -1;
                byte[] inBuff;

                int parentMachineRequestCharCount, inputDeviceRequestByteCount;
                Int64 number;

                InputConvertings inputConverting = _InputConverting;

                inData = new byte[length];

                if (!InputFromBufferAllowed)
                    switch (inputConverting)
                    {
                        case InputConvertings.None:
                            DirectInput(out inBuff, length);
                            resultLength = inBuff.Length <= length ? inBuff.Length : length;
                            Array.Copy(inBuff, inData, resultLength);
                            break;
                        case InputConvertings.String:
                            parentMachineRequestCharCount = ParentIDE.Machine.CharSet.GetMaxCharCount(length);
                            inputDeviceRequestByteCount = _InputDevice.CharSet.GetMaxByteCount(parentMachineRequestCharCount);
                            DirectInput(out inBuff, inputDeviceRequestByteCount);
                            inBuff = Encoding.Convert(_InputDevice.CharSet, ParentIDE.Machine.CharSet
                                    , inBuff);
                            resultLength = inBuff.Length <= length ? inBuff.Length : length;
                            Array.Copy(inBuff, inData, resultLength);
                            break;
                        case InputConvertings.ParsingNumber:
                            byte[] inBuff2;
                            parentMachineRequestCharCount = XBinary.MaxDigitCount(256, 10, length) + 1;
                            inputDeviceRequestByteCount = _InputDevice.CharSet.GetMaxByteCount(parentMachineRequestCharCount);
                            DirectInput(out inBuff, inputDeviceRequestByteCount);
                            if (XBinary.TryParseTextNumberBytesToBinaryNumberBytes(inBuff
                                    , _InputDevice.CharSet, out inBuff2, length, ParentIDE.Machine.ByteOrder))
                            {
                                inData = inBuff2;
                                resultLength = length;
                            }
                            else
                                resultLength = 0;
                            break;
                        default:
                            throw new Exception();
                    }
                else
                    // using input buffer:
                    switch (inputConverting)
                    {
                        case InputConvertings.None:
                            if (_InputBuffer.Length == 0)
                            {
                                DirectInput(out _InputBuffer, length);
                            }
                            //
                            if (_InputBuffer.Length == 0)
                            {
                                resultLength = 0;
                            }
                            else if (_InputBuffer.Length - _InBuffPtr <= length)
                            {
                                resultLength = _InputBuffer.Length - _InBuffPtr;
                                Array.Copy(_InputBuffer, _InBuffPtr, inData, 0, resultLength);
                                _InputBuffer = new byte[0];
                                _InBuffPtr = 0;
                            }
                            else if (_InputBuffer.Length - _InBuffPtr > length)
                            {
                                resultLength = length;
                                Array.Copy(_InputBuffer, _InBuffPtr, inData, 0, resultLength);
                                _InBuffPtr += resultLength;
                            }
                            break;
                        case InputConvertings.String:
                            if (_InputBuffer.Length == 0)
                            {
                                parentMachineRequestCharCount = ParentIDE.Machine.CharSet.GetMaxCharCount(length);
                                inputDeviceRequestByteCount = _InputDevice.CharSet.GetMaxByteCount(parentMachineRequestCharCount);
                                DirectInput(out inBuff, inputDeviceRequestByteCount);
                                _InputBuffer = Encoding.Convert(_InputDevice.CharSet, ParentIDE.Machine.CharSet
                                        , inBuff);
                            }
                            //
                            if (_InputBuffer.Length == 0)
                            {
                                resultLength = 0;
                            }
                            else if (_InputBuffer.Length - _InBuffPtr <= length)
                            {
                                resultLength = _InputBuffer.Length - _InBuffPtr;
                                Array.Copy(_InputBuffer, _InBuffPtr, inData, 0, resultLength);
                                _InputBuffer = new byte[0];
                                _InBuffPtr = 0;
                            }
                            else if (_InputBuffer.Length - _InBuffPtr > length)
                            {
                                resultLength = length;
                                Array.Copy(_InputBuffer, _InBuffPtr, inData, 0, resultLength);
                                _InBuffPtr += resultLength;
                            }
                            break;
                        case InputConvertings.ParsingNumber:
                            if (_InputWordBuffer.Length == 0)
                            {
                                parentMachineRequestCharCount = XBinary.MaxDigitCount(256, 10, length) + 1;
                                inputDeviceRequestByteCount = _InputDevice.CharSet.GetMaxByteCount(parentMachineRequestCharCount);
                                DirectInput(out inBuff, inputDeviceRequestByteCount);
                                byte[] unicodeLineBytes = Encoding.Convert(_InputDevice.CharSet, Encoding.Unicode, inBuff);
                                char[] unicodeLineChars = new char[Encoding.Unicode.GetCharCount(unicodeLineBytes, 0, unicodeLineBytes.Length)];
                                Encoding.Unicode.GetChars(unicodeLineBytes, 0, unicodeLineBytes.Length, unicodeLineChars, 0);
                                string newLine = new string(unicodeLineChars);
                                _InputWordBuffer = newLine.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                            }
                            //
                            if (_InputWordBuffer.Length == 0)
                            {
                                resultLength = 0;
                            }
                            else if (_InputWordBuffer.Length - _WdBuffPtr > 0)
                            {
                                if (Int64.TryParse(_InputWordBuffer[_WdBuffPtr], out number))
                                {
                                    inData = XBinary.FormatSignedNumberBytes(BitConverter.GetBytes(number), length
                                            , XBinary.HostMachineByteOrder, ParentIDE.Machine.ByteOrder);
                                    resultLength = length;
                                }
                                else
                                {
                                    resultLength = 0;
                                }
                                //
                                _WdBuffPtr++;
                                if (_InputWordBuffer.Length == _WdBuffPtr)
                                {
                                    _InputWordBuffer = new string[0];
                                    _WdBuffPtr = 0;
                                }
                            }
                            break;
                        default:
                            throw new Exception();
                    }

                return resultLength;
            }
            finally
            {
                //
                IOmonitor.Unlock(_IOprocessing);
            }
        }

        public virtual byte Input()
        {
            byte[] b;

            if (Input(out b, 1) >= 0)
                return b[0];
            else
                return unchecked((byte)-1); // 255
        }

        public virtual void Close()
        {
            try
            {
                IOmonitor.IdleForLock(_IOprocessing);
                //
                bool locked;
                if (_InputDevice != null && _InputDevice.IsInitialized && _InputDevice.InitializedBy == ParentIDE.Machine)
                {
                    locked = false;
                    if (_InputDevice.IsLockingRequiredForDispose()) IOmonitor.IdleForLock(_InputDevice, out locked);
                    _InputDevice.Dispose();
                    if (locked) IOmonitor.Unlock(_InputDevice);
                }
                _InputBuffer = new byte[0];
                _InBuffPtr = 0;
                _InputWordBuffer = new string[0];
                _WdBuffPtr = 0;
            }
            finally
            {
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
