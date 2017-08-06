using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using XSpace;

namespace multiIDE.Dialogs
{
    public sealed partial class DllImportDialog : Form
    {
        public readonly IComponentBuilder<IComponent, ComponentTypeInfo> BeingUpdatedComponentBuilder;
        public bool FileOpened { get; private set; } = false;
        //
        public new DialogResult DialogResult { get; private set; }
        public string ChosenSourceFileName { get; private set; }
        public List<string> ChosenTypeFullNames { get; private set; }
        public bool FromComponentDefaultNamespace { get; private set; }

        public DllImportDialog(IComponentBuilder<IComponent, ComponentTypeInfo> generalizedComponentBuilderToUpdate)
        {
            InitializeComponent();
            //
            Type componentBuilderType = generalizedComponentBuilderToUpdate.GetType();
            if (componentBuilderType.Name == "BuilderGeneralizer")
                componentBuilderType = generalizedComponentBuilderToUpdate.GetCastedBuilder().GetType();
            try
            {
                Type[] componentBuilderInterfaces = componentBuilderType.GetInterfaces();
                Type[] genArgs = componentBuilderInterfaces[0].GetGenericArguments();
                this.Text = $"Import {genArgs[0].Name}-based Types";
            }
            catch(Exception) // TEMPORARY
            {

            }
            //
            BeingUpdatedComponentBuilder = generalizedComponentBuilderToUpdate;
        }

        private void DLLimportDialog_Load(object sender, EventArgs e)
        {
            optAnyNamespaces.CheckedChanged += (sss, eee) => { OpenFile(); };
            optDefaultNamespace.CheckedChanged += (sss, eee) => { OpenFile(); };
            optShowOnlyNewTypes.CheckedChanged += (sss, eee) => { OpenFile(); };
            //
            optDefaultNamespace.Text = BeingUpdatedComponentBuilder.ComponentDefaultNamespace;
            lstTypeInfosFromSource.Items.Clear();
            cmdSelectAll.Enabled = false;
            cmdDeselectAll.Enabled = false;
            cmdImport.Enabled = false;
        }

        public DialogResult ShowDialog(Form owner)
        {
            this.Owner = owner;
            DialogResult = DialogResult.None;
            this.ShowDialog();

            return DialogResult;
        }

        private void cmdChooseFile_Click(object sender, System.EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = (Program.TheMainForm.SelectedIDE != null && (Program.TheMainForm.SelectedIDE?.ProgramFile.FileName.IsNotNullOrEmpty() ?? false)) ? Path.GetDirectoryName(Program.TheMainForm.SelectedIDE?.ProgramFile.FileName)
                                : (Program.TheMainForm.RecentFiles.Count > 2) ? Path.GetDirectoryName(Program.TheMainForm.RecentFiles[0])
                                : Path.GetDirectoryName(Application.ExecutablePath);
            openFileDialog.Filter = "DLL Files (*.dll)|*.dll|All Files (*.*)|*.*";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (new StreamReader(openFileDialog.FileName)) { }
                    txtSourceFileName.Text = openFileDialog.FileName;
                    OpenFile();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                    return;
                }
            }
            else
            {
                return;
            }
        }

        private void lblInternal_Click(object sender, EventArgs e)
        {
            txtSourceFileName.Text = BeingUpdatedComponentBuilder.InternalSourceKeyword;
            OpenFile();
        }

        private void OpenFile()
        {
            List<ComponentTypeInfo> typeInfosFromSource = BeingUpdatedComponentBuilder
                .GetTypesInfosFrom(txtSourceFileName.Text, optDefaultNamespace.Checked);

            IEnumerable<ComponentTypeInfo> originalTypeInfosFromSource = typeInfosFromSource
                .Where<ComponentTypeInfo>(i => !optShowOnlyNewTypes.Checked || !BeingUpdatedComponentBuilder.HasTypeInfo(i));
            
            lstTypeInfosFromSource.Items.Clear();
            foreach (var componentTypeInfo in originalTypeInfosFromSource)
            {
                var item = new ListViewItem(componentTypeInfo.TypeName);
                item.SubItems.Add(componentTypeInfo.DefaultName);
                item.SubItems.Add(componentTypeInfo.Version);
                item.SubItems.Add(componentTypeInfo.Author);
                item.SubItems.Add(componentTypeInfo.TypeFullName);
                if (!optShowOnlyNewTypes.Checked && BeingUpdatedComponentBuilder.HasTypeInfo(componentTypeInfo))
                    item.ForeColor = System.Drawing.Color.LightSlateGray;
                lstTypeInfosFromSource.Items.Add(item);
            }

            FileOpened = true;
            cmdSelectAll.Enabled = true;
            cmdDeselectAll.Enabled = false;
        }

        private void lstTypeInfosFromSource_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (lstTypeInfosFromSource.SelectedItems.Count > 1)
            {
                cmdSelect.Enabled = true;
                cmdDeselect.Enabled = true;
            }
            else if (lstTypeInfosFromSource.SelectedItems.Count == 1)
            {
                cmdSelect.Enabled = !lstTypeInfosFromSource.SelectedItems[0].Checked;
                cmdDeselect.Enabled = !cmdSelect.Enabled;
            }
            else
            {
                cmdSelect.Enabled = false;
                cmdDeselect.Enabled = false;
            }
        }

        private void lstTypeInfosFromSource_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (lstTypeInfosFromSource.SelectedItems.Count > 1)
                e.NewValue = lstTypeInfosFromSource.Items[e.Index].Checked ? CheckState.Checked : CheckState.Unchecked;
        }

        private void lstTypeInfosFromSource_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            foreach (var item in lstTypeInfosFromSource.CheckedItems.Cast<ListViewItem>().Where<ListViewItem>(i => i.ForeColor != System.Drawing.Color.Black))
                item.Checked = false;

            if (lstTypeInfosFromSource.CheckedItems.Count == lstTypeInfosFromSource.Items.Count)
            {
                cmdSelectAll.Enabled = false;
                cmdDeselectAll.Enabled = true;
                cmdImport.Enabled = true;
            }
            else if (lstTypeInfosFromSource.CheckedItems.Count > 0)
            {
                cmdSelectAll.Enabled = true;
                cmdDeselectAll.Enabled = true;
                cmdImport.Enabled = true;
            }
            else if (lstTypeInfosFromSource.CheckedItems.Count == 0)
            {
                cmdSelectAll.Enabled = true;
                cmdDeselectAll.Enabled = false;
                cmdImport.Enabled = false;
            }
        }

        private void cmdSelect_Click(object sender, EventArgs e)
        {
            lstTypeInfosFromSource.ItemCheck -= lstTypeInfosFromSource_ItemCheck;
            foreach (ListViewItem typeInfoItem in lstTypeInfosFromSource.SelectedItems)
                typeInfoItem.Checked = true;
            lstTypeInfosFromSource.ItemCheck += lstTypeInfosFromSource_ItemCheck;
            lstTypeInfosFromSource_ItemChecked(null, null);
        }

        private void cmdDeselect_Click(object sender, EventArgs e)
        {
            lstTypeInfosFromSource.ItemCheck -= lstTypeInfosFromSource_ItemCheck;
            foreach (ListViewItem typeInfoItem in lstTypeInfosFromSource.SelectedItems)
                typeInfoItem.Checked = false;
            lstTypeInfosFromSource.ItemCheck += lstTypeInfosFromSource_ItemCheck;
            lstTypeInfosFromSource_ItemChecked(null, null);
        }

        private void cmdSelectAll_Click(object sender, EventArgs e)
        {
            lstTypeInfosFromSource.ItemCheck -= lstTypeInfosFromSource_ItemCheck;
            foreach (ListViewItem typeInfoItem in lstTypeInfosFromSource.Items)
                typeInfoItem.Checked = true;
            lstTypeInfosFromSource.ItemCheck += lstTypeInfosFromSource_ItemCheck;
            lstTypeInfosFromSource_ItemChecked(null, null);
        }

        private void cmdDeselectAll_Click(object sender, EventArgs e)
        {
            lstTypeInfosFromSource.ItemCheck -= lstTypeInfosFromSource_ItemCheck;
            foreach (ListViewItem typeInfoItem in lstTypeInfosFromSource.Items)
                typeInfoItem.Checked = false;
            lstTypeInfosFromSource.ItemCheck += lstTypeInfosFromSource_ItemCheck;
            lstTypeInfosFromSource_ItemChecked(null, null);
        }

        private void cmdImport_Click(object sender, EventArgs e)
        {
            ChosenSourceFileName = txtSourceFileName.Text;
            ChosenTypeFullNames = lstTypeInfosFromSource.Items.Cast<ListViewItem>()
                    .Where(i => i.Checked)
                    .Select(i => i.SubItems[4].Text)
                    .ToList();
            FromComponentDefaultNamespace = optDefaultNamespace.Checked;
            DialogResult = DialogResult.OK;
            this.Hide();
        }

        private void DLLimportDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
                DialogResult = DialogResult.Cancel;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Hide();
        }
    }
}
