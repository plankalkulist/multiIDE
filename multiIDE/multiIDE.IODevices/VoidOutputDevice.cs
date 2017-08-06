using System.ComponentModel;
using System.Text;
using XSpace;

namespace multiIDE.IODevices
{
    /// <summary>
    /// Void virtual output device for ignoring output data of connected IO port.
    /// </summary>
    [DefaultProperty("Title")]
    public sealed class VoidOutputDevice : IOutputDevice
    {
        #region Profile properties
        [Category("Profile"), ReadOnly(true)]
        public string DefaultName => "Void Output Device";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Version => "1.0.0.0";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Author => "";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Description => "Void virtual output device for ignoring output data of connected IO port.";
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
                return _CustomTitle.IsNotNullOrEmpty()  ? _CustomTitle
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
        public bool IsInitialized { get; private set; } = false;
        //
        [Category("Essential"), ReadOnly(true)]
        public object InitializedBy { get; private set; }
        //
        [Category("Essential"), ReadOnly(true)]
        public bool IsReadyToOutput => IsInitialized;
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
                if (!IsInitialized) _CharSet = value;
            }
        }
        //
        private Encoding _CharSet = Encoding.Default;
        #endregion

        #region Technicals
        public object Locking { get; } = new object();
        #endregion

        public override string ToString() => (CustomName.IsNotNullOrEmpty()) ? CustomName
            : DefaultName + " " + Id.ToString();

        /////

        public VoidOutputDevice()
        { }

        public byte GetStatus(object sender)
        {
            return (byte)(this.IsReadyToOutput ? 1 : 0);
        }

        public void Initialize(object sender)
        {
            InitializedBy = (sender as IIdeComponent)?.ParentIDE.Machine ?? sender;
            IsInitialized = true;
        }

        [MenuBrowsable(true), AvailableWhenInitializedOnly(true)]
        public void Dispose()
        {
            IsInitialized = false;
        }

        public void Output(object sender, byte[] outData)
        { }
    }
}
