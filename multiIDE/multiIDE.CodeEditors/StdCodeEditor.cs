using System;
using System.Windows.Forms;
using System.ComponentModel;
using XSpace;
using System.Collections.Generic;

namespace multiIDE.CodeEditors
{
    public partial class StdCodeEditor : Form, ICodeEditor
    {
        #region Profile properties
        [Category("Profile"), ReadOnly(true)]
        public virtual string DefaultName => "Standard Code Editor";
        //
        [Category("Profile"), ReadOnly(true)]
        public virtual string Version => "1.0.0.0";
        //
        [Category("Profile"), ReadOnly(true)]
        public virtual string Description => "Default code editing tools window.";
        //
        [Category("Profile"), ReadOnly(true)]
        public virtual string Author => "";
        #endregion

        #region Environment properties
        [Category("Environment"), ReadOnly(true)]
        public virtual IIDE ParentIDE
        {
            get
            {
                return _ParentIDE;
            }
            set
            {
                if (_ParentIDE == null) _ParentIDE = value;
            }
        }
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
                return (_ParentIDE.ProgramFile.ShortFileName.IsNotNullOrEmpty() ?
                    _ParentIDE.ProgramFile.ShortFileName : "NoName " + _ParentIDE.ProgramLanguage + " program")
                    + (!_ParentIDE.ProgramFile.IsReadOnly ?
                    (_ParentIDE.ProgramFile.IsChangesSaved ? "" : "*") : " [Read Only]")
                    + " - " + _ParentIDE.Title + " " + _StatusWord + " - Editing";
            }
            set
            {
                _CustomTitle = value;
            }
        }
        //
        protected IIDE _ParentIDE = null;
        protected string _CustomTitle = "";
        #endregion

        #region Events
        public virtual event EventHandler CodeChanged;
        #endregion

        #region Essential properties
        [Category("Essential"), ReadOnly(false)]
        public virtual string CurrentCode
        {
            get
            {
                return txtCode.Text;
            }
            set
            {
                txtCode.Text = value;
            }
        }
        #endregion

        public override string ToString() => (CustomName.IsNotNullOrEmpty()) ? CustomName : DefaultName + ParentIDE.Id.ToString();

        /////
        protected string _StatusWord;
        protected const int _TooFrequentStatusChangingMS = 300;
        protected int _StatusLastMS = 0;

        public StdCodeEditor()
        {
            InitializeComponent();
        }

        public StdCodeEditor(IIDE parentIDE)
        {
            InitializeComponent();
            //
            _ParentIDE = parentIDE;
        }

        #region Environment subs
        public new void Activate()
        {
            if (!this.IsDisposed)
            {
                if (this.InvokeRequired)
                {
                    Action a = new Action(base.Activate);
                    this.BeginInvoke(a);
                }
                else
                {
                    base.Activate();
                }
            }
        }

        public virtual void UpdateTitle()
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

        public void ParentIDE_GotUpdated(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                var a = new Action<object, EventArgs>(_ParentIDE_GotUpdated);
                this.Invoke(a, sender, e);
            }
            else
            {
                _ParentIDE_GotUpdated(sender, e);
            }
        }

        public void AssociatedMachine_StatusChanged(object sender, VirtualMachineStatusChangedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                var a = new Action<object, VirtualMachineStatusChangedEventArgs>(_AssociatedMachine_StatusChanged);
                this.Invoke(a, sender, e);
            }
            else
            {
                _AssociatedMachine_StatusChanged(sender, e);
            }
        }

        public void AssociatedMachine_SetsChanged(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                var a = new Action<object, EventArgs>(_AssociatedMachine_SetsChanged);
                this.Invoke(a, sender, e);
            }
            else
            {
                _AssociatedMachine_SetsChanged(sender, e);
            }
        }

        // // //

        protected void _UpdateTitle()
        {
            if (_ParentIDE != null)
            {
                this.Text = Title;
            }
            else
            {
                this.Text = "--ERROR: SINGLE WINDOW--";
            }
        }

        private void _ParentIDE_GotUpdated(object sender, EventArgs e)
        {
            _UpdateTitle();
        }

        protected void _AssociatedMachine_StatusChanged(object sender, VirtualMachineStatusChangedEventArgs e)
        {
            switch (e.NewStatus)
            {
                case VirtualMachineRunningStatus.StandBy:
                    _StatusWord = "[StandBy]";
                    txtCode.ReadOnly = false;
                    break;
                case VirtualMachineRunningStatus.Runtime:
                    _StatusWord = "[Runtime]";
                    txtCode.ReadOnly = true;
                    break;
                case VirtualMachineRunningStatus.Paused:
                    _StatusWord = "[Paused At " + (_ParentIDE.Machine.NextSymbol + 1).ToString() + "]";
                    txtCode.ReadOnly = true;
                    txtCode.Focus();
                    txtCode.Select(_ParentIDE.Machine.NextSymbol, 1);
                    break;
                case VirtualMachineRunningStatus.Breaking:
                    _StatusWord = "[Breaking...]";
                    txtCode.ReadOnly = true;
                    break;
                case VirtualMachineRunningStatus.Stepping:
                case VirtualMachineRunningStatus.SteppingOver:
                    if (Environment.TickCount - _StatusLastMS < _TooFrequentStatusChangingMS) break;
                    _StatusWord = "[Stepping...]";
                    txtCode.ReadOnly = true;
                    break;
                case VirtualMachineRunningStatus.Pausing:
                    //_StatusWord = "[Pausing...]";
                    txtCode.ReadOnly = true;
                    break;
                default:
                    _StatusWord = "[--unknown--]";
                    txtCode.ReadOnly = true;
                    break;
            }
            UpdateTitle();

            _StatusLastMS = Environment.TickCount;
        }

        protected void _AssociatedMachine_SetsChanged(object sender, EventArgs e)
        {

        }
        #endregion

        public string GetProgramCode()
        {
            return Preprocess(CurrentCode);
        }

        public string Preprocess(string codeText)
        {
            string resultCode;
            int i, j, k;
            int macroStatementStart, macroStatementLength;
            string macroName, macroValue;
            var macroStatements = new List<KeyValuePair<int, int>>();
            var macros = new List<KeyValuePair<string, string>>();
            var commentStatements = new List<KeyValuePair<int, int>>();

            i = codeText.IndexOf("#m\"");
            //
            while (i >= 0 && i + 3 < codeText.Length)
            {
                j = codeText.IndexOf("\"", i + 3);
                if (j > 0)
                {
                    macroStatementStart = i;
                    macroName = codeText.Substring(i + 3, j - i - 3);
                    k = j + 5 < codeText.Length && codeText.Substring(j, 3) == "\":\""
                            ? codeText.IndexOf("\"", j + 3) : -1;
                    if (k > 0)
                    {
                        if (codeText.Substring(k, 2) == "\"#")
                        {
                            macroStatementLength = k + 2 - i;
                            macroValue = codeText.Substring(j + 3, k - j - 3);
                            macroStatements.Add(new KeyValuePair<int, int>(macroStatementStart, macroStatementLength));
                            macros.Add(new KeyValuePair<string, string>(macroName, macroValue));
                            i = k + 2;
                        }
                        else
                        {
                            txtCode.Select(i, k - i);
                            throw new InvalidMacroCodeStatementException($"Invalid Macro Code Statement Exception at {i} : end of statement expected but has not found.");
                        }
                    }
                    else
                    {
                        txtCode.Select(i, j - i);
                        throw new InvalidMacroCodeStatementException($"Invalid Macro Code Statement Exception at {i} : macro name expected but has not found.");
                    }
                }
                else
                {
                    txtCode.Select(i, 3);
                    throw new InvalidMacroCodeStatementException($"Invalid Macro Code Statement Exception at {i} : macro value expected but has not found.");
                }
                //
                if (i + 3 < codeText.Length)
                    i = codeText.IndexOf("#m\"", i + 3);
            }

            resultCode = codeText;

            for (i = macroStatements.Count - 1; i >= 0; i -= 1)
            {
                resultCode = resultCode.Remove(macroStatements[i].Key, macroStatements[i].Value);
            }

            foreach (var macro in macros)
            {
                resultCode = resultCode.Replace(macro.Key, macro.Value);
            }

            return resultCode;
        }

        public virtual void ClearCode()
        {
            txtCode.Clear();
        }

        protected void frmCode_Load(object sender, EventArgs e)
        {
            _ParentIDE.GotUpdated += ParentIDE_GotUpdated;
            _ParentIDE.Machine.StatusChanged += AssociatedMachine_StatusChanged;
            _ParentIDE.Machine.SetsChanged += AssociatedMachine_SetsChanged;
        }

        protected void frmCode_Shown(object sender, EventArgs e)
        {
            _AssociatedMachine_StatusChanged(_ParentIDE.Machine, new VirtualMachineStatusChangedEventArgs(_ParentIDE.Machine.Status, _ParentIDE.Machine.NextSymbol));
        }

        protected void frmCode_Activated(object sender, EventArgs e)
        {
            UpdateTitle();

            ParentIDE.ParentWorkplace.MainFormSelectIDE(ParentIDE);
        }

        protected async void frmCode_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                e.Cancel = !(await ParentIDE.ParentWorkplace.CloseIDEbyUser(_ParentIDE));
            }
        }

        protected void txtCode_TextChanged(object sender, System.EventArgs e)
        {
            (CodeChanged)?.Invoke(this, new EventArgs());
        }
    }
}
