using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using XSpace;

namespace multiIDE.IODevices
{
    [DefaultProperty("Title")]
    public class ConsoleDevice : IInputDevice, IOutputDevice, IReinitializeable
    {
        #region Profile properties
        [Category("Profile"), ReadOnly(true)]
        public virtual string DefaultName => "Console Device";
        //
        [Category("Profile"), ReadOnly(true)]
        public virtual string Version => "1.0.0.0";
        //
        [Category("Profile"), ReadOnly(true)]
        public virtual string Author => "";
        //
        [Category("Profile"), ReadOnly(true)]
        public virtual string Description => "Provides an user console-like input/output interface via Windows Form.";
        #endregion

        #region Environment properties
        [Category("Environment"), ReadOnly(true)]
        public virtual IWorkplace ParentWorkplace { get; set; }
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
                return _CustomTitle.IsNotNullOrEmpty() ? _CustomTitle :
                    $"Console id{Id} - {(IsInitialized ? InitializedBy.ToString() : "(uninit-ed)")}";
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
        [Category("Essential"), ReadOnly(true)]
        public virtual bool IsInitialized { get; protected set; } = false;
        //
        [Category("Essential"), ReadOnly(true)]
        public virtual object InitializedBy { get; protected set; }
        //
        [Category("Essential"), ReadOnly(true)]
        public virtual bool IsReadyToInput => IsInitialized && (ConsoleForm != null ? !ConsoleForm.IsDisposed
                            && !ConsoleForm.IsBusyInputting && ConsoleForm.PostsToOutputLeft == 0 : false);
        //
        [Category("Essential"), ReadOnly(true)]
        public virtual bool IsReadyToOutput => IsInitialized && (ConsoleForm != null ? !ConsoleForm.IsDisposed
                            && !ConsoleForm.IsBusyInputting : false);
        //
        [Category("Essential")]
        public virtual Encoding CharSet
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
        [Category("Essential"), Browsable(false)]
        public virtual frmConsole ConsoleForm { get; protected set; }
        //
        [Category("Essential"), ReadOnly(true)]
        public virtual Hashtable DataClients
        {
            get
            {
                return (Hashtable)ConsoleForm?.DataClients.Clone();
            }
        }
        //
        [Category("Appearance"), ReadOnly(false)]
        public bool ColorDataClients
        {
            get { return _ColorDataClients; }
            set
            {
                _ColorDataClients = value;
                if (!_ColorDataClients)
                    ConsoleForm.Invoke(new Action(() => { ConsoleForm.Display.ForeColor = System.Drawing.Color.White; }));
            }
        }
        //
        [Category("Appearance"), ReadOnly(false)]
        public int MaxDisplayTextLength { get; set; } = 0x400000;
        //
        [Category("Appearance"), ReadOnly(false)]
        public bool AutoScrollDisplayToEnd { get; set; } = true;
        //
        protected Encoding _CharSet = Encoding.Default;
        protected string _LastDisplayRtf = null;
        protected Hashtable _LastDataClients = null;
        protected List<Color> _LastDataClientsColors = null;
        protected bool _ColorDataClients = true;
        #endregion

        #region Technicals
        [Browsable(false)]
        public object Locking { get; } = new object();
        #endregion

        public override string ToString() => (CustomName.IsNotNullOrEmpty())
                ? CustomName : DefaultName + " id" + Id.ToString();

        /////

        public ConsoleDevice()
        { }

        public virtual byte GetStatus(object sender)
        {
            if (sender is IInputPort)
                return (byte)(this.IsReadyToInput ? 1 : 0);
            if (sender is IOutputPort)
                return (byte)(this.IsReadyToOutput ? 1 : 0);
            return (byte)((this.IsReadyToInput ? 1 : 0) + 2 * (this.IsReadyToOutput ? 1 : 0));
        }

        [LockingRequired]
        public virtual void Initialize(object sender)
        {
            {
                if (IsInitialized)
                    return;

                ConsoleForm = new frmConsole(this);
                ConsoleForm.Show();
                ConsoleForm.Activate();

                InitializedBy = (sender as IIdeComponent)?.ParentIDE.Machine ?? sender;
                IsInitialized = true;

                ConsoleForm.Text = Title;
                ConsoleForm.Display.SelectionColor = System.Drawing.Color.White;
            }
        }

        [LockingRequired, MenuBrowsable(true), AvailableWhenInitializedOnly(true)]
        public virtual void Dispose()
        {
            if (!IsInitialized)
                return;
            
            ConsoleForm?.Dispose();
            ConsoleForm = null;
            _CustomTitle = "";
            IsInitialized = false;
            InitializedBy = null;
        }

        [LockingRequired]
        public virtual void Reinitialize(object sender, bool saveData = true)
        {
            if (!(IsInitialized && ConsoleForm != null))
                return;

            if (saveData)
            {
                string rtfText = "";
                Hashtable dataClients = null;
                List<Color> dataClientsColors = null;
                if (ConsoleForm != null && !ConsoleForm.IsDisposed)
                {
                    rtfText = ConsoleForm.GetDisplayRTFtext();
                    dataClients = ConsoleForm.DataClients;
                    dataClientsColors = ConsoleForm.DataClientsColors;
                }

                frmConsole oldConsole = ConsoleForm;
                ConsoleForm = new frmConsole(this);
                ConsoleForm.Show();
                ConsoleForm.Activate();
                InitializedBy = sender is IInputPort ? (sender as IInputPort).ParentIDE.Machine
                        : sender is IOutputPort ? (sender as IOutputPort).ParentIDE.Machine
                        : sender;
                IsInitialized = true;
                ConsoleForm.Text = Title;

                ConsoleForm.Display.Rtf = rtfText;
                ConsoleForm.DataClients = dataClients;
                ConsoleForm.DataClientsColors = dataClientsColors;
                oldConsole?.DisposeAsync();
                oldConsole = null;
            }
            else
            {
                Dispose();
                Initialize(sender);
            }

        }

        [LockingRequired]
        public virtual void Input(object sender, out byte[] inData, int length)
        {
            if (!IsInitialized)
                throw new Exception();

            IOmonitor.IdleWhile(() => !IsReadyToInput);

            string uniStrData = null;
            ConsoleForm?.Input(sender, out uniStrData, length);

            if (uniStrData == null)
            {
                inData = new byte[0];
                return;
            }

            byte[] uniBytes = Encoding.Unicode.GetBytes(uniStrData);

            if (_CharSet == Encoding.Unicode)
            {
                inData = uniBytes;
            }
            else
            {
                inData = Encoding.Convert(Encoding.Unicode, _CharSet, uniBytes);
            }
        }

        [LockingRequired]
        public virtual void Output(object sender, byte[] outData)
        {
            if (!IsInitialized)
                throw new Exception();

            IOmonitor.IdleWhile(() => !IsReadyToOutput);

            byte[] uniBytes;

            if (_CharSet == Encoding.Unicode)
            {
                uniBytes = outData;
            }
            else
            {
                uniBytes = Encoding.Convert(_CharSet, Encoding.Unicode, outData);
            }

            ConsoleForm?.Output(sender, Encoding.Unicode.GetString(uniBytes));
        }

        [MenuBrowsable(true), AvailableWhenInitializedOnly(true)]
        public virtual void FocusOn()
        {
            if (ConsoleForm != null)
            {
                ConsoleForm?.Show();
                if (ConsoleForm.WindowState == System.Windows.Forms.FormWindowState.Minimized)
                    ConsoleForm.WindowState = FormWindowState.Normal;
                ConsoleForm?.Activate();
            }
        }

        [MenuBrowsable(true), Description("Sayann hheeeeeeyy theereee.")]
        public virtual void SayHeeeeeeyDErrrR()
        {
            System.Windows.Forms.MessageBox.Show("HeeeeeeyDErrrR, its " + this.Title);
        }

        [LockingRequired, MenuBrowsable(true), AvailableWhenInitializedOnly(true)]
        public virtual void ClearDisplay()
        {
            ConsoleForm.Invoke(new Action(() => { ConsoleForm.Display.Clear(); }));
        }

        [LockingRequired, MenuBrowsable(true), AvailableWhenInitializedOnly(true)]
        public virtual void ClearDataClientsList()
        {
            ConsoleForm.DataClients.Clear();
        }
    }
}
