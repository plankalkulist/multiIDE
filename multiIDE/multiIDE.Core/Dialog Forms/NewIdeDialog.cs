using System;
using System.Collections.Generic;
using System.Windows.Forms;
using XSpace;

namespace multiIDE.Dialogs
{
    public sealed partial class NewIdeDialog : Form
    {
        public new DialogResult DialogResult { get; private set; }
        //
        public List<VirtualMachineTypeInfo> MachineTypes { get; private set; }
        public List<InputPortTypeInfo> InputPortTypes { get; private set; }
        public List<OutputPortTypeInfo> OutputPortTypes { get; private set; }
        public List<CodeEditorTypeInfo> CodeEditorTypes { get; private set; }
        public bool OfferingConsole { get; private set; }
        //
        public int ChosenMachineTypeIndex { get; private set; }
        public string ChosenMachineTypeFullName { get; private set; }
        //
        public int ChosenLanguageIndex { get; private set; }
        public string ChosenLanguage { get; private set; }
        //
        public int ChosenCodeEditorTypeIndex { get; private set; }
        public string ChosenCodeEditorTypeFullName { get; private set; }
        //
        public bool ChosenWithInputPort { get; private set; }
        public int ChosenInputPortIndex { get; private set; }
        public int ChosenInputPortTypeIndex { get; private set; }
        public string ChosenInputPortTypeFullName { get; private set; }
        //
        public bool ChosenWithOutputPort { get; private set; }
        public int ChosenOutputPortIndex { get; private set; }
        public int ChosenOutputPortTypeIndex { get; private set; }
        public string ChosenOutputPortTypeFullName { get; private set; }
        //
        public bool ChosenWithConsole { get; private set; }

        public NewIdeDialog(List<VirtualMachineTypeInfo> machineTypes, List<CodeEditorTypeInfo> codeEditorTypes
                          , List<InputPortTypeInfo> inputPortTypes, List<OutputPortTypeInfo> outputPortTypes
                          , bool offerConsole
                          , int defaultMachineTypeIndex = 0, int defaultInputPortTypeIndex = 0
                          , int defaultOutputPortTypeIndex = 0, int defaultCodeEditorTypeIndex = 0)
        {
            InitializeComponent();
            
            int i;
            MachineTypes = machineTypes;
            InputPortTypes = inputPortTypes;
            OutputPortTypes = outputPortTypes;
            CodeEditorTypes = codeEditorTypes;

            foreach (VirtualMachineTypeInfo machineTypeInfo in MachineTypes)
            {
                i = lstMachineTypes.Items.Count;
                lstMachineTypes.Items.Add(machineTypeInfo.DefaultName);
                lstMachineTypes.Items[i].ToolTipText = MachineTypes[i].TypeName + " Version: " + MachineTypes[i].Version
                    + (MachineTypes[i].Author.IsNotNullOrEmpty() ? "   by " + MachineTypes[i].Author : "");
            }
            lstMachineTypes.SelectedIndices.Clear();
            lstMachineTypes.SelectedIndices.Add(defaultMachineTypeIndex);

            foreach (CodeEditorTypeInfo codeEditorTypeInfo in CodeEditorTypes)
            {
                i = cmbCodeEditorType.Items.Count;
                cmbCodeEditorType.Items.Add(CodeEditorTypes[i].TypeName + " Version: " + CodeEditorTypes[i].Version
                    + (CodeEditorTypes[i].Author.IsNotNullOrEmpty() ? "   by " + CodeEditorTypes[i].Author : ""));
            }
            cmbCodeEditorType.SelectedItem = cmbCodeEditorType.Items[defaultCodeEditorTypeIndex];

            if (InputPortTypes != null && InputPortTypes.Count > 0)
            {
                foreach (InputPortTypeInfo inputPortTypeInfo in InputPortTypes)
                {
                    i = cmbInputPortType.Items.Count;
                    cmbInputPortType.Items.Add(InputPortTypes[i].TypeName + " Version: " + InputPortTypes[i].Version
                                               + (InputPortTypes[i].Author.IsNotNullOrEmpty()
                                                   ? "   by " + InputPortTypes[i].Author
                                                   : ""));
                }
                cmbInputPortType.SelectedItem = cmbInputPortType.Items[defaultInputPortTypeIndex];
            }
            else
                fraAddInputPort.Enabled = false;

            if (OutputPortTypes != null && OutputPortTypes.Count > 0)
            {
                foreach (OutputPortTypeInfo outputPortTypeInfo in OutputPortTypes)
                {
                    i = cmbOutputPortType.Items.Count;
                    cmbOutputPortType.Items.Add(OutputPortTypes[i].TypeName + " Version: " + OutputPortTypes[i].Version
                                                + (OutputPortTypes[i].Author.IsNotNullOrEmpty()
                                                    ? "   by " + OutputPortTypes[i].Author
                                                    : ""));
                }
                cmbOutputPortType.SelectedItem = cmbOutputPortType.Items[defaultOutputPortTypeIndex];
            }
            else
                fraAddOutputPort.Enabled = false;

            OfferingConsole = offerConsole;
            chkConnectNewConsole.Enabled = OfferingConsole
                && (fraAddInputPort.Enabled || fraAddOutputPort.Enabled);

            ChosenWithInputPort = fraAddInputPort.Enabled && optEmptyInputPort.Checked;
            ChosenWithOutputPort = fraAddOutputPort.Enabled && optEmptyOutputPort.Checked;
            ChosenWithConsole = OfferingConsole && chkConnectNewConsole.Checked;

            cmdCreate.Enabled = false;
            
            lstMachineTypes.SelectedIndexChanged += new EventHandler(this.lstMachines_SelectedIndexChanged);
            lstMachineTypes.DoubleClick += new EventHandler(this.lstMachines_DoubleClick);
            lstLanguages.SelectedIndexChanged += new EventHandler(this.lstLanguages_SelectedIndexChanged);
        }

        public DialogResult ShowDialog(Form owner)
        {
            this.Owner = owner;
            DialogResult = DialogResult.None;
            this.ShowDialog();

            return DialogResult;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Hide();
        }

        private void cmdCreate_Click(object sender, EventArgs e)
        {
            if (lstMachineTypes.SelectedItems.Count == 1 && lstLanguages.SelectedItems.Count == 1)
            {
                ChosenMachineTypeIndex = lstMachineTypes.SelectedIndices[0];
                ChosenMachineTypeFullName = MachineTypes[ChosenMachineTypeIndex].TypeFullName;

                ChosenLanguageIndex = lstLanguages.SelectedIndices[0];
                ChosenLanguage = lstLanguages.SelectedItems[0].ToString();

                ChosenCodeEditorTypeIndex = cmbCodeEditorType.SelectedIndex;
                ChosenCodeEditorTypeFullName = CodeEditorTypes[ChosenCodeEditorTypeIndex].TypeFullName;

                int i;
                ChosenInputPortTypeIndex = ChosenWithInputPort
                        ? cmbInputPortType.SelectedIndex : -1;
                ChosenInputPortTypeFullName = ChosenWithInputPort
                        ? InputPortTypes[ChosenInputPortTypeIndex].TypeFullName : "";
                ChosenInputPortIndex = ChosenWithInputPort
                        ? (txtInputPortIndex.Enabled && int.TryParse(txtInputPortIndex.Text, out i)
                        ? i : -1)
                        : CommonConstants.UndefinedComponentId;
                ChosenOutputPortTypeIndex = ChosenWithOutputPort
                        ? cmbOutputPortType.SelectedIndex : -1;
                ChosenOutputPortTypeFullName = ChosenWithOutputPort
                        ? OutputPortTypes[ChosenOutputPortTypeIndex].TypeFullName : "";
                ChosenOutputPortIndex = ChosenWithOutputPort
                        ? (txtOutputPortIndex.Enabled && int.TryParse(txtOutputPortIndex.Text, out i)
                        ? i : -1)
                        : CommonConstants.UndefinedComponentId;

                ChosenWithConsole = OfferingConsole && chkConnectNewConsole.Enabled && chkConnectNewConsole.Checked;

                DialogResult = DialogResult.OK;
                this.Hide();
            }
            else
            {
                MessageBox.Show("You should choose at least both a machine and a language for new IDE.");
            }
        }

        private void NewIdeDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing) DialogResult = DialogResult.Cancel;
        }

        private void lstMachines_DoubleClick(object sender, EventArgs e)
        {
            int selMachine = lstMachineTypes.SelectedIndices[0];
            lstLanguages.SelectedItems.Clear();
            lstLanguages.SetSelected(lstLanguages.Items.IndexOf(MachineTypes[lstMachineTypes.SelectedIndices[0]].TargetLanguage), true);
            cmdCreate_Click(new object(), new EventArgs());
        }

        private void lstMachines_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstMachineTypes.SelectedIndices.Count == 1)
            {
                foreach (var machineTypeItem in lstMachineTypes.Items)
                {
                    (machineTypeItem as ListViewItem).BackColor = lstMachineTypes.BackColor;
                    (machineTypeItem as ListViewItem).ForeColor = lstMachineTypes.ForeColor;
                }
                lstMachineTypes.SelectedItems[0].BackColor = System.Drawing.Color.Blue;
                lstMachineTypes.SelectedItems[0].ForeColor = System.Drawing.Color.White;

                lstLanguages.Items.Clear();
                foreach (string lang in MachineTypes[lstMachineTypes.SelectedIndices[0]].SupportedLanguages)
                {
                    lstLanguages.Items.Add(lang);
                }
                lstLanguages.SetSelected(lstLanguages.Items.IndexOf(MachineTypes[lstMachineTypes.SelectedIndices[0]].TargetLanguage), true);

                txtMachineDescription.Text = MachineTypes[lstMachineTypes.SelectedIndices[0]].TypeName + " Version: " + MachineTypes[lstMachineTypes.SelectedIndices[0]].Version
                    + (MachineTypes[lstMachineTypes.SelectedIndices[0]].Author.IsNotNullOrEmpty() ? "   by " + MachineTypes[lstMachineTypes.SelectedIndices[0]].Author : "")
                    + Environment.NewLine + "   from " + MachineTypes[lstMachineTypes.SelectedIndices[0]].SourceFileName.ShrinkFileName(75)
                    + Environment.NewLine + MachineTypes[lstMachineTypes.SelectedIndices[0]].Description;

                lblInputPortIndex.Enabled = cmbInputPortType.Enabled
                    && Type.GetType(MachineTypes[lstMachineTypes.SelectedIndices[0]].AssemblyQualifiedTypeName).GetInterface("IMultiIOable") != null;
                txtInputPortIndex.Enabled = lblInputPortIndex.Enabled;
                lblOutputPortIndex.Enabled = cmbOutputPortType.Enabled
                    && Type.GetType(MachineTypes[lstMachineTypes.SelectedIndices[0]].AssemblyQualifiedTypeName).GetInterface("IMultiIOable") != null;
                txtOutputPortIndex.Enabled = lblOutputPortIndex.Enabled;
            }
            else
            {
                lstMachineTypes.SelectedItems.Clear();
                foreach (var machineTypeItem in lstMachineTypes.Items)
                {
                    (machineTypeItem as ListViewItem).BackColor = lstMachineTypes.BackColor;
                    (machineTypeItem as ListViewItem).ForeColor = lstMachineTypes.ForeColor;
                }
                txtMachineDescription.Text = "";
                cmdCreate.Enabled = false;
            }
        }

        private void lstLanguages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstMachineTypes.SelectedIndices.Count == 1)
            {
                cmdCreate.Enabled = true;
            }
            else
            {
                cmdCreate.Enabled = false;
                lstLanguages.SelectedItems.Clear();
                lstLanguages.SetSelected(lstLanguages.Items.IndexOf(MachineTypes[lstMachineTypes.SelectedIndices[0]].TargetLanguage), true);
            }
        }

        private void optWithInputPort_CheckedChanged(object sender, EventArgs e)
        {
            ChosenWithInputPort = optEmptyInputPort.Checked;
            cmbInputPortType.Enabled = optEmptyInputPort.Checked;
            lblInputPortIndex.Enabled = optEmptyInputPort.Checked && Type.GetType(MachineTypes[lstMachineTypes.SelectedIndices[0]].AssemblyQualifiedTypeName).GetInterface("IMultiIOable") != null;
            txtInputPortIndex.Enabled = lblInputPortIndex.Enabled;
            //
            chkConnectNewConsole.Enabled = OfferingConsole && (ChosenWithInputPort || ChosenWithOutputPort);
        }

        private void optWithOutputPort_CheckedChanged(object sender, EventArgs e)
        {
            ChosenWithOutputPort = optEmptyOutputPort.Checked;
            cmbOutputPortType.Enabled = optEmptyOutputPort.Checked;
            lblOutputPortIndex.Enabled = optEmptyOutputPort.Checked && Type.GetType(MachineTypes[lstMachineTypes.SelectedIndices[0]].AssemblyQualifiedTypeName).GetInterface("IMultiIOable") != null;
            txtOutputPortIndex.Enabled = lblOutputPortIndex.Enabled;
            //
            chkConnectNewConsole.Enabled = OfferingConsole && (ChosenWithInputPort || ChosenWithOutputPort);
        }
    }
}
