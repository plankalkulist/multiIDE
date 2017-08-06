using multiIDE.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using XSpace;

namespace multiIDE.Extras
{
    [MenuBrowsable(false)]
    public partial class ComponentManager : Form, IExtraWorkplaceComponent
    {
        #region Profile properties
        [Category("Profile"), ReadOnly(true)]
        public string DefaultName => "Component Manager";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Version => "1.0.0.0";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Author => "";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Description => "Manages available components types.";
        #endregion

        #region Environment properties
        [Category("Environment"), ReadOnly(true)]
        public IWorkplace ParentWorkplace { get; set; }
        //
        [Category("Environment"), ReadOnly(true)]
        string IComponent.CustomName { get; set; } = "";
        //
        [Category("Environment"), ReadOnly(true)]
        int IComponent.Id { get; set; }
        //
        [Category("Appearance"), DefaultValue("")]
        public string Title
        {
            get
            {
                return _CustomTitle.IsNotNullOrEmpty() ? _CustomTitle
                    : $"{(_Tag != null ? _Tag.ToString() + " -" : "Default")} Component Manager";
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
            get { return _Tag; }
            set
            {
                _Tag = value;
                Title = "";
            }
        }
        //
        private object _Tag = null;
        private string _CustomTitle = "";
        #endregion

        #region Essential properties
        [Category("Essential"), ReadOnly(true)]
        public bool IsInitialized { get; protected set; } = false;
        //
        [Category("Essential"), ReadOnly(true)]
        public object InitializedBy { get; protected set; }
        #endregion

        #region Events
        public event EventHandler Closing;
        #endregion

        public override string ToString() => DefaultName;

        /////
        private ListView[] _RegisteredComponentTypesListViews;
        private TabPage[] _ComponentTabPages;
        private IComponentBuilder<IComponent, ComponentTypeInfo>[] _GeneralizedBuilders;

        public ComponentManager()
        { }

        public void Initialize(object sender)
        {
            InitializeComponent();

            IsInitialized = true;
            InitializedBy = sender;

            _ComponentTabPages = new[]
            {
                tbpMachines,
                tbpCodeEditors,
                tbpInputPorts,
                tbpOutputPorts,
                tbpIdeExtras,
                tbpInputDevices,
                tbpOutputDevices,
                tbpWorkplaceExtras,
                tbpCommons
            };
            _RegisteredComponentTypesListViews = new[]
            {
                lstRegisteredVirtualMachineTypes,
                lstRegisteredCodeEditorTypes,
                lstRegisteredInputPortTypes,
                lstRegisteredOutputPortTypes,
                lstRegisteredIdeExtraTypes,
                lstRegisteredInputDeviceTypes,
                lstRegisteredOutputDeviceTypes,
                lstRegisteredWorkplaceExtraTypes,
                lstRegisteredCommonTypes
            };
            _GeneralizedBuilders = _Tag != null
                ? new[]
                {
                    (_Tag as IWorkplace).VirtualMachineBuilder.ToGeneralBuilderType(),
                    (_Tag as IWorkplace).CodeEditorBuilder.ToGeneralBuilderType(),
                    (_Tag as IWorkplace).InputPortBuilder.ToGeneralBuilderType(),
                    (_Tag as IWorkplace).OutputPortBuilder.ToGeneralBuilderType(),
                    (_Tag as IWorkplace).IdeExtraBuilder.ToGeneralBuilderType(),
                    (_Tag as IWorkplace).InputDeviceBuilder.ToGeneralBuilderType(),
                    (_Tag as IWorkplace).OutputDeviceBuilder.ToGeneralBuilderType(),
                    (_Tag as IWorkplace).WorkplaceExtraBuilder.ToGeneralBuilderType()
                }
                : new[]
                {
                    ComponentBuildersService.DefaultVirtualMachineBuilder.ToGeneralBuilderType(),
                    ComponentBuildersService.DefaultCodeEditorBuilder.ToGeneralBuilderType(),
                    ComponentBuildersService.DefaultInputPortBuilder.ToGeneralBuilderType(),
                    ComponentBuildersService.DefaultOutputPortBuilder.ToGeneralBuilderType(),
                    ComponentBuildersService.DefaultIdeExtraBuilder.ToGeneralBuilderType(),
                    ComponentBuildersService.DefaultInputDeviceBuilder.ToGeneralBuilderType(),
                    ComponentBuildersService.DefaultOutputDeviceBuilder.ToGeneralBuilderType(),
                    ComponentBuildersService.DefaultWorkplaceExtraBuilder.ToGeneralBuilderType(),
                    ComponentBuildersService.CommonBuilder.ToGeneralBuilderType()
                };

            tabComponents.Selected += (s, e) =>
            {
                ComponentTypesListView_SelectedIndexChanged(e.TabPage.Tag, e);
            };

            for (int i = 0; i < _GeneralizedBuilders.Length; i++)
            {
                _RegisteredComponentTypesListViews[i].ItemCheck += ComponentTypesListView_ItemCheck;
                _RegisteredComponentTypesListViews[i].ItemChecked += ComponentTypesListView_ItemChecked;
                _RegisteredComponentTypesListViews[i].SelectedIndexChanged += ComponentTypesListView_SelectedIndexChanged;
                _ComponentTabPages[i].Tag = _RegisteredComponentTypesListViews[i];
                _RegisteredComponentTypesListViews[i].Tag = _GeneralizedBuilders[i];
            }

            if (_Tag != null)
                tabComponents.TabPages.Remove(tbpCommons);

            this.Text = Title;
            this.Show();
            this.Activate();

            RefreshData();
        }

        public void RefreshData(ListView componentTypesListViewOnly = null)
        {
            if (componentTypesListViewOnly == null)
            {
                for (int i = 0; i < _GeneralizedBuilders.Length; i++)
                {
                    _RegisteredComponentTypesListViews[i].Items.Clear();
                    foreach (var componentTypeInfo in _GeneralizedBuilders[i].RegisteredTypes)
                    {
                        var item = new ListViewItem(componentTypeInfo.TypeName)
                        { Checked = !componentTypeInfo.IsHidden };
                        item.SubItems.Add(componentTypeInfo.DefaultName);
                        item.SubItems.Add(componentTypeInfo.Version);
                        item.SubItems.Add(componentTypeInfo.Author);
                        item.SubItems.Add(componentTypeInfo.TypeFullName);
                        item.SubItems.Add(componentTypeInfo.SourceFileName);
                        _RegisteredComponentTypesListViews[i].Items.Add(item);
                    }
                }
            }
            else
            {
                componentTypesListViewOnly.Items.Clear();
                foreach (var componentTypeInfo in (componentTypesListViewOnly.Tag
                        as IComponentBuilder<IComponent, ComponentTypeInfo>).RegisteredTypes)
                {
                    var item = new ListViewItem(componentTypeInfo.TypeName)
                    { Checked = !componentTypeInfo.IsHidden };
                    item.SubItems.Add(componentTypeInfo.DefaultName);
                    item.SubItems.Add(componentTypeInfo.Version);
                    item.SubItems.Add(componentTypeInfo.Author);
                    item.SubItems.Add(componentTypeInfo.TypeFullName);
                    item.SubItems.Add(componentTypeInfo.SourceFileName);
                    componentTypesListViewOnly.Items.Add(item);
                }
            }

            txtDescription.Text = "";
            ComponentTypesListView_SelectedIndexChanged(tabComponents.SelectedTab.Tag, null);
        }

        private void ComponentTypesListView_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var componentTypesListView = sender as ListView;
            if (componentTypesListView.SelectedItems.Count > 1)
                e.NewValue = componentTypesListView.Items[e.Index].Checked ? CheckState.Checked : CheckState.Unchecked;
        }

        private void ComponentTypesListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var componentTypesListView = sender as ListView;
            var componentBuilder = componentTypesListView.Tag as IComponentBuilder<IComponent, ComponentTypeInfo>;

            if (e.Item.Checked)
                componentBuilder.ShowType(e.Item.SubItems[4].Text);
            else
                componentBuilder.HideType(e.Item.SubItems[4].Text);
        }

        private void ComponentTypesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            var componentTypesListView = sender as ListView;
            var componentBuilder = componentTypesListView.Tag as IComponentBuilder<IComponent, ComponentTypeInfo>;

            if (componentTypesListView.SelectedItems.Count == 0)
            {
                txtDescription.Text = "";
                cmdShow.Enabled = false;
                cmdHide.Enabled = false;
                cmdDelete.Enabled = false;
            }
            else if (componentTypesListView.SelectedItems.Count == 1)
            {
                txtDescription.Text = (componentBuilder.RegisteredTypes).Find(i => i.TypeFullName
                       == componentTypesListView.SelectedItems[0].SubItems[4].Text).Description;
                cmdShow.Enabled = !componentTypesListView.SelectedItems[0].Checked;
                cmdHide.Enabled = !cmdShow.Enabled;
                cmdDelete.Enabled = true;
            }
            else
            {
                txtDescription.Text = "";
                cmdShow.Enabled = true;
                cmdHide.Enabled = true;
                cmdDelete.Enabled = true;
            }
        }

        private void cmdHide_Click(object sender, EventArgs e)
        {
            var componentTypesListView = tabComponents.SelectedTab.Tag as ListView;
            var componentBuilder = componentTypesListView.Tag as IComponentBuilder<IComponent, ComponentTypeInfo>;

            componentTypesListView.ItemCheck -= ComponentTypesListView_ItemCheck;
            { // 
                foreach (ListViewItem componentTypeListItem in componentTypesListView.SelectedItems)
                {
                    componentBuilder.HideType(componentTypeListItem.SubItems[4].Text);
                    componentTypeListItem.Checked = !componentBuilder.RegisteredTypes
                        .Find(i => i.TypeFullName == componentTypeListItem.SubItems[4].Text).IsHidden;
                }
            } // 
            componentTypesListView.ItemCheck += ComponentTypesListView_ItemCheck;

            componentTypesListView.Select();
            ComponentTypesListView_SelectedIndexChanged(tabComponents.SelectedTab.Tag, null);
        }

        private void cmdShow_Click(object sender, EventArgs e)
        {
            var componentTypesListView = tabComponents.SelectedTab.Tag as ListView;
            var componentBuilder = componentTypesListView.Tag as IComponentBuilder<IComponent, ComponentTypeInfo>;

            componentTypesListView.ItemCheck -= ComponentTypesListView_ItemCheck;
            { // 
                foreach (ListViewItem componentTypeListItem in componentTypesListView.SelectedItems)
                {
                    componentBuilder.ShowType(componentTypeListItem.SubItems[4].Text);
                    componentTypeListItem.Checked = !componentBuilder.RegisteredTypes
                        .Find(i => i.TypeFullName == componentTypeListItem.SubItems[4].Text).IsHidden;
                }
            } // 
            componentTypesListView.ItemCheck += ComponentTypesListView_ItemCheck;

            componentTypesListView.Select();
            ComponentTypesListView_SelectedIndexChanged(tabComponents.SelectedTab.Tag, null);
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            var componentTypesListView = tabComponents.SelectedTab.Tag as ListView;
            var componentBuilder = componentTypesListView.Tag as IComponentBuilder<IComponent, ComponentTypeInfo>;

            foreach (ListViewItem componentTypeListItem in componentTypesListView.SelectedItems)
            {
                componentBuilder.RemoveType(componentTypeListItem.SubItems[4].Text);
                componentTypesListView.Items.Remove(componentTypeListItem);
            }

            componentTypesListView.Select();
            ComponentTypesListView_SelectedIndexChanged(tabComponents.SelectedTab.Tag, null);
        }

        private void cmdAddTypes_Click(object sender, EventArgs e)
        {
            var componentTypesListView = tabComponents.SelectedTab.Tag as ListView;
            var componentBuilder = componentTypesListView.Tag as IComponentBuilder<IComponent, ComponentTypeInfo>;

            string sourceFileName = "";
            List<string> typeFullNames = null;

            DialogResult dllImportDialogResult;
            using (var dllImportDialog = new DllImportDialog((dynamic)componentTypesListView.Tag))
            {
                dllImportDialogResult = dllImportDialog.ShowDialog(this);
                if (dllImportDialogResult == DialogResult.OK)
                {
                    sourceFileName = dllImportDialog.ChosenSourceFileName;
                    typeFullNames = dllImportDialog.ChosenTypeFullNames;
                }
            }
            if (dllImportDialogResult != DialogResult.OK)
                return;

            componentBuilder.RegisterTypesFrom(sourceFileName, typeFullNames);
            RefreshData(componentTypesListView);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (_Tag == null)
                ComponentBuildersService.SaveDefaultComponentsTypesSources();

            this.Close();
        }

        private void ComponentManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
        }
    }
}
