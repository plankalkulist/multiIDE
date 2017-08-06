using System;
using System.Collections;
using System.ComponentModel;
using System.Text;
using XSpace;

namespace multiIDE.IODevices
{
    /// <summary>
    /// Virtual input device producing a constant sequence of bytes repeatedly.
    /// </summary>
    [DefaultProperty("ConstantInputBytes")]
    public sealed class ConstantInputDevice : IInputDevice
    {
        #region Profile properties
        [Category("Profile"), ReadOnly(true)]
        public string DefaultName => "Constant Input Device";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Version => "1.0.0.0";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Author => "";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Description => "Virtual input device producing a constant sequence of bytes repeatedly.";
        #endregion

        #region Environment properties
        [Category("Environment"), ReadOnly(true)]
        public IWorkplace ParentWorkplace { get; set; }
        //
        [Category("Environment"), ReadOnly(true)]
        public string CustomName { get; set; } = "";
        //
        [Category("Environment"), ReadOnly(true)]
        public int Id { get; set; } = CommonConstants.UndefinedComponentId;
        //
        [Category("Appearance"), DefaultValue("")]
        public string Title
        {
            get
            {
                return _CustomTitle.IsNotNullOrEmpty() ? _CustomTitle
                    : (IsInitialized ? InitializedBy.ToString() : "(uninit-ed)")
                    + $" - {DefaultName}"
                    + $" id{Id}";
            }
            set
            {
                _CustomTitle = value;
            }
        }
        //
        [Category("Environment"), ReadOnly(true)]
        public object Tag { get; set; }
        //
        private string _CustomTitle = "";
        #endregion

        #region Essential properties
        [Category("Essential"), ReadOnly(true)]
        public bool IsInitialized { get; private set; }
        //
        [Category("Essential"), ReadOnly(true)]
        public object InitializedBy { get; private set; }
        //
        [Category("Essential"), ReadOnly(true)]
        public bool IsReadyToInput => IsInitialized;
        //
        [Category("Essential")]
        public Encoding CharSet
        {
            get
            {
                return _CharSet;
            }
            set
            {
                if (!IsInitialized)
                {
                    _CharSet = value;
                }
                else
                {
                    throw new Exception();
                }
            }
        }
        //
        [Category("Essential")]
        public byte[] ConstantInputBytes
        {
            get
            {
                return _ConstantInputBytes;
            }
            set
            {
                if (!IsInitialized)
                {
                    _ConstantInputBytes = value;
                }
                else
                {
                    throw new Exception();
                }
            }
        }
        //
        [Category("Essential")]
        public string ConstantInputString
        {
            get
            {
                byte[] uniBytes = Encoding.Convert(_CharSet, Encoding.Unicode, _ConstantInputBytes);
                return Encoding.Unicode.GetString(uniBytes);
            }
            set
            {
                if (!IsInitialized)
                {
                    byte[] uniBytes = Encoding.Unicode.GetBytes(value);
                    _ConstantInputBytes = Encoding.Convert(Encoding.Unicode, _CharSet, uniBytes);
                }
                else
                {
                    throw new Exception();
                }
            }
        }
        //
        [Category("Essential"), ReadOnly(true)]
        public Hashtable DataClients
        {
            get
            {
                return (Hashtable)_DataClients.Clone();
            }
        }
        //
        private Encoding _CharSet = Encoding.Default;
        private byte[] _ConstantInputBytes = new byte[0];
        private Hashtable _DataClients = new Hashtable();
        #endregion

        #region Technicals
        public object Locking { get; } = new object();
        #endregion

        public override string ToString() => (CustomName.IsNotNullOrEmpty()) ? CustomName
                : DefaultName + " " + Id.ToString();

        /////

        public ConstantInputDevice()
        { }

        public byte GetStatus(object sender)
        {
            return (byte)(this.IsReadyToInput ? 1 : 0);
        }

        public void Initialize(object sender)
        {
            InitializedBy = (sender as IIdeComponent)?.ParentIDE.Machine ?? sender;
            _DataClients = new Hashtable();
            IsInitialized = true;
        }

        [MenuBrowsable(true), AvailableWhenInitializedOnly(true)]
        public void Dispose()
        {
            IsInitialized = false;
        }

        public void Input(object sender, out byte[] inData, int length)
        {
            int inputAt = 0;
            if (_DataClients.ContainsKey(sender.ToString()))
                inputAt = (int)_DataClients[sender.ToString()];
            else
                _DataClients.Add(sender.ToString(), 0);

            inData = new byte[length];
            for (int i = 0; i < length; i++)
                inData[i] = _ConstantInputBytes[(inputAt + i) % _ConstantInputBytes.Length];

            _DataClients[sender.ToString()] = (inputAt + length) % _ConstantInputBytes.Length;
        }
    }
}
