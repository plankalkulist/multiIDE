using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using XSpace;

namespace multiIDE.Extras
{
    [InitializeAfterCreate()]
    public partial class ViewRAM : Form, IExtraIdeComponent
    {
        #region Profile properties
        [Category("Profile"), ReadOnly(true)]
        public virtual string DefaultName => "Standard View RAM window";
        //
        [Category("Profile"), ReadOnly(true)]
        public virtual string Version => "1.0.0.0";
        //
        [Category("Profile"), ReadOnly(true)]
        public virtual string Author => "";
        //
        [Category("Profile"), ReadOnly(true)]
        public virtual string Description => "Provides view tools for a virtual machine's RAM.";
        #endregion

        #region Environment properties
        [Category("Environment")]
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
                    : (ParentIDE.Machine != null ? $"{ParentIDE.Machine.Title}" : "") + " - Main RAM";
            }
            set
            {
                _CustomTitle = value;
                this.Text = Title;
            }
        }
        //
        [Category("Environment"), ReadOnly(true)]
        object IComponent.Tag { get; set; }
        //
        protected string _CustomTitle = "";
        #endregion

        #region Essential properties
        [Category("Essential"), ReadOnly(true)]
        public bool IsInitialized { get; protected set; } = false;
        //
        [Category("Essential"), ReadOnly(true)]
        public object InitializedBy { get; protected set; }
        //
        [Category("Essential"), ReadOnly(true)]
        public IComponent BeingSet { get; private set; }
        #endregion

        #region Events
        public event EventHandler Closing;
        #endregion

        /////

        public ViewRAM()
        { }

        public void Initialize(object sender)
        {
            InitializeComponent();
            ParentIDE.Machine.StatusChanged += AssociatedMachine_StatusChanged;

            IsInitialized = true;
            InitializedBy = sender;

            this.Text = Title;
            this.Show();
        }

        private void frmViewRAM_Load(object sender, EventArgs e)
        {
            _UpdateTitle();
        }

        private void ViewRAM_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = e.CloseReason == CloseReason.UserClosing;
            ParentIDE.Machine.StatusChanged -= AssociatedMachine_StatusChanged;
        }

        public void UpdateTitle()
        {
            if (this.InvokeRequired)
            {
                var a = new Action(_UpdateTitle);
                this.Invoke(a);
            }
            else
            {
                _UpdateTitle();
            }
        }

        public void AssociatedMachine_StatusChanged(object sender, VirtualMachineStatusChangedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                var a = new EventHandler<VirtualMachineStatusChangedEventArgs>(_AssociatedMachine_StatusChanged);
                this.Invoke(a, sender, e);
            }
            else
            {
                _AssociatedMachine_StatusChanged(sender, e);
            }
        }

        private void _UpdateTitle()
        {
            if (ParentIDE != null)
            {
                this.Text = ParentIDE.Machine.Title + " - Main RAM View";
            }
            else
            {
                this.Text = "--ERROR: SINGLE WINDOW--";
            }
        }

        private void _AssociatedMachine_StatusChanged(object sender, VirtualMachineStatusChangedEventArgs e)
        {
            if (chkAutoDump.Checked && (e.NewStatus == VirtualMachineRunningStatus.Paused
                    || e.NewStatus == VirtualMachineRunningStatus.StandBy))
            {
                cmdDump_Click(this, new EventArgs());
            }
        }

        private void cmdDump_Click(object sender, System.EventArgs e)
        {
            int A = 0;
            int B = 0;
            int step = 0;
            int since = 0, length = 0;
            string S = null;
            rtbDump.Clear();
            lblCurrentCell.Text = "Current Cell: " + ParentIDE.Machine.ActionCell.ToString();
            step = 10;

            try
            {
                if (int.TryParse(txtSince.Text, out since) && int.TryParse(txtLength.Text, out length))
                {
                    for (A = since; A < since + length; A += step)
                    {
                        rtbDump.AppendText($"{A,5}: ");
                        S = "";
                        if (ParentIDE.Machine.RAM[A] > 31)
                        {
                            S += (char)(ParentIDE.Machine.RAM[A]);
                        }
                        else
                        {
                            S += ".";
                        }
                        rtbDump.AppendText($"{ParentIDE.Machine.RAM[A],3}");

                        for (B = A + 1; B < A + step; B++)
                        {
                            if (ParentIDE.Machine.RAM[B] > 31)
                                S += (char)(ParentIDE.Machine.RAM[B]);
                            else
                                S += ".";
                            rtbDump.AppendText($",{ParentIDE.Machine.RAM[B],3}");
                        }
                        rtbDump.AppendText("  " + S + Environment.NewLine);
                    }

                    if (ParentIDE.Machine.ActionCell >= since && ParentIDE.Machine.ActionCell < since + length)
                    {
                        rtbDump.Select(rtbDump.Text.IndexOf($"{since + ((ParentIDE.Machine.ActionCell - since) / step) * step,5}: ") + 7 + 4 * ((ParentIDE.Machine.ActionCell - since) % step), 3);
                        rtbDump.SelectionBackColor = Color.Aquamarine;
                    }
                }
            }
            catch (NullReferenceException errNRE)
            {
                rtbDump.Text = "Machine have not initialized yet.";
            }
            catch (IndexOutOfRangeException errIOORE)
            {
                rtbDump.Text = "Out of the machine's memory range.";
            }
        }
    }
}
