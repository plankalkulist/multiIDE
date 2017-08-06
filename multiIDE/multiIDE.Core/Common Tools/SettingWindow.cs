using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using XSpace;

namespace multiIDE.Extras
{
    [MenuBrowsable(false)]
    public sealed partial class SettingWindow : Form, IExtraIdeComponent, IExtraWorkplaceComponent
    {
        #region Profile properties
        [Category("Profile"), ReadOnly(true)]
        public string DefaultName => "Setting Window";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Version => "1.0.0.0";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Author => "";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Description => "Provides setting interaction for any component.";
        #endregion

        #region Environment properties
        [Category("Environment"), ReadOnly(true)]
        public IComponent ParentComponent => (this as IIdeComponent).ParentIDE != null ? (this as IIdeComponent).ParentIDE
            : (this as IWorkplaceComponent).ParentWorkplace as IComponent;
        //
        [Category("Environment"), ReadOnly(true)]
        IIDE IIdeComponent.ParentIDE { get; set; }
        //
        [Category("Environment"), ReadOnly(true)]
        IWorkplace IWorkplaceComponent.ParentWorkplace { get; set; }
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
                    : BeingSet.Title + (ParentComponent != null ? $" - {ParentComponent.Title}" : "") + " - Setting";
            }
            set
            {
                _CustomTitle = value;
                this.Text = Title;
            }
        }
        //
        [Category("Environment"), ReadOnly(true)]
        public new object Tag
        {
            get { return BeingSet; }
            set
            {
                BeingSet = value as IComponent;
                Title = "";
            }
        }
        //
        private string _CustomTitle = "";
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
        private delegate object beingSetObjectDefaultSettinsDelegate(object Object, params object[] args);
        private beingSetObjectDefaultSettinsDelegate _DefaultSettings;

        public SettingWindow()
        { }

        public void Initialize(object sender)
        {
            InitializeComponent();
            prgSettings.SelectedObject = BeingSet;

            Type beingSetObjectType = BeingSet.GetType();
            MethodInfo[] defaultSettingsMethods = beingSetObjectType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod).Where(m => m.Name == "DefaultSettings" && m.GetParameters().Length == 0).ToArray();
            if (defaultSettingsMethods.Length > 0)
            {
                _DefaultSettings = new beingSetObjectDefaultSettinsDelegate(defaultSettingsMethods[0].Invoke);
                cmdSwitchToDefaults.Enabled = true;
            }
            else
            {
                _DefaultSettings = null;
                cmdSwitchToDefaults.Enabled = false;
            }

            IsInitialized = true;
            InitializedBy = sender;

            this.Text = Title;
            this.Show();
            //this.Activate();
        }

        private void cmdSwitchToDefaults_Click(object sender, EventArgs e)
        {
            _DefaultSettings(BeingSet, null);
            prgSettings.Refresh();
        }

        private void SettingDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Closing?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
