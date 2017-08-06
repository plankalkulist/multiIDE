namespace multiIDE.Dialogs
{
    partial class NewIdeDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lstMachineTypes = new System.Windows.Forms.ListView();
            this.cmdCreate = new System.Windows.Forms.Button();
            this.lstLanguages = new System.Windows.Forms.ListBox();
            this.fraAddInputPort = new System.Windows.Forms.GroupBox();
            this.txtInputPortIndex = new System.Windows.Forms.TextBox();
            this.lblInputPortIndex = new System.Windows.Forms.Label();
            this.cmbInputPortType = new System.Windows.Forms.ComboBox();
            this.optNoneInputPort = new System.Windows.Forms.RadioButton();
            this.optEmptyInputPort = new System.Windows.Forms.RadioButton();
            this.txtMachineDescription = new System.Windows.Forms.TextBox();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.tipMain = new System.Windows.Forms.ToolTip(this.components);
            this.cmbCodeEditorType = new System.Windows.Forms.ComboBox();
            this.lblCodeEditor = new System.Windows.Forms.Label();
            this.fraAddOutputPort = new System.Windows.Forms.GroupBox();
            this.txtOutputPortIndex = new System.Windows.Forms.TextBox();
            this.lblOutputPortIndex = new System.Windows.Forms.Label();
            this.cmbOutputPortType = new System.Windows.Forms.ComboBox();
            this.optNoneOutputPort = new System.Windows.Forms.RadioButton();
            this.optEmptyOutputPort = new System.Windows.Forms.RadioButton();
            this.chkConnectNewConsole = new System.Windows.Forms.CheckBox();
            this.lblComponentNotification = new System.Windows.Forms.Label();
            this.fraAddInputPort.SuspendLayout();
            this.fraAddOutputPort.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstMachineTypes
            // 
            this.lstMachineTypes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstMachineTypes.Location = new System.Drawing.Point(12, 12);
            this.lstMachineTypes.MultiSelect = false;
            this.lstMachineTypes.Name = "lstMachineTypes";
            this.lstMachineTypes.ShowItemToolTips = true;
            this.lstMachineTypes.Size = new System.Drawing.Size(390, 186);
            this.lstMachineTypes.TabIndex = 0;
            this.lstMachineTypes.UseCompatibleStateImageBehavior = false;
            this.lstMachineTypes.View = System.Windows.Forms.View.Tile;
            // 
            // cmdCreate
            // 
            this.cmdCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCreate.Location = new System.Drawing.Point(302, 484);
            this.cmdCreate.Name = "cmdCreate";
            this.cmdCreate.Size = new System.Drawing.Size(119, 23);
            this.cmdCreate.TabIndex = 1;
            this.cmdCreate.Text = "Create";
            this.cmdCreate.UseVisualStyleBackColor = true;
            this.cmdCreate.Click += new System.EventHandler(this.cmdCreate_Click);
            // 
            // lstLanguages
            // 
            this.lstLanguages.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstLanguages.FormattingEnabled = true;
            this.lstLanguages.Location = new System.Drawing.Point(418, 12);
            this.lstLanguages.Name = "lstLanguages";
            this.lstLanguages.Size = new System.Drawing.Size(173, 186);
            this.lstLanguages.TabIndex = 2;
            // 
            // fraAddInputPort
            // 
            this.fraAddInputPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.fraAddInputPort.Controls.Add(this.txtInputPortIndex);
            this.fraAddInputPort.Controls.Add(this.lblInputPortIndex);
            this.fraAddInputPort.Controls.Add(this.cmbInputPortType);
            this.fraAddInputPort.Controls.Add(this.optNoneInputPort);
            this.fraAddInputPort.Controls.Add(this.optEmptyInputPort);
            this.fraAddInputPort.Location = new System.Drawing.Point(12, 327);
            this.fraAddInputPort.Name = "fraAddInputPort";
            this.fraAddInputPort.Size = new System.Drawing.Size(281, 95);
            this.fraAddInputPort.TabIndex = 3;
            this.fraAddInputPort.TabStop = false;
            this.fraAddInputPort.Text = "Add Input Port";
            // 
            // txtInputPortIndex
            // 
            this.txtInputPortIndex.Location = new System.Drawing.Point(127, 41);
            this.txtInputPortIndex.Name = "txtInputPortIndex";
            this.txtInputPortIndex.Size = new System.Drawing.Size(40, 20);
            this.txtInputPortIndex.TabIndex = 3;
            this.txtInputPortIndex.Text = "0";
            // 
            // lblInputPortIndex
            // 
            this.lblInputPortIndex.AutoSize = true;
            this.lblInputPortIndex.Location = new System.Drawing.Point(64, 44);
            this.lblInputPortIndex.Name = "lblInputPortIndex";
            this.lblInputPortIndex.Size = new System.Drawing.Size(66, 13);
            this.lblInputPortIndex.TabIndex = 2;
            this.lblInputPortIndex.Text = "at port slot #";
            // 
            // cmbInputPortType
            // 
            this.cmbInputPortType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbInputPortType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbInputPortType.FormattingEnabled = true;
            this.cmbInputPortType.Location = new System.Drawing.Point(21, 65);
            this.cmbInputPortType.Name = "cmbInputPortType";
            this.cmbInputPortType.Size = new System.Drawing.Size(251, 21);
            this.cmbInputPortType.TabIndex = 1;
            // 
            // optNoneInputPort
            // 
            this.optNoneInputPort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.optNoneInputPort.Location = new System.Drawing.Point(6, 19);
            this.optNoneInputPort.Name = "optNoneInputPort";
            this.optNoneInputPort.Size = new System.Drawing.Size(266, 17);
            this.optNoneInputPort.TabIndex = 0;
            this.optNoneInputPort.Text = "Don\'t";
            this.optNoneInputPort.UseVisualStyleBackColor = true;
            this.optNoneInputPort.CheckedChanged += new System.EventHandler(this.optWithInputPort_CheckedChanged);
            // 
            // optEmptyInputPort
            // 
            this.optEmptyInputPort.AutoSize = true;
            this.optEmptyInputPort.Checked = true;
            this.optEmptyInputPort.Location = new System.Drawing.Point(6, 42);
            this.optEmptyInputPort.Name = "optEmptyInputPort";
            this.optEmptyInputPort.Size = new System.Drawing.Size(54, 17);
            this.optEmptyInputPort.TabIndex = 0;
            this.optEmptyInputPort.TabStop = true;
            this.optEmptyInputPort.Text = "Empty";
            this.optEmptyInputPort.UseVisualStyleBackColor = true;
            this.optEmptyInputPort.CheckedChanged += new System.EventHandler(this.optWithInputPort_CheckedChanged);
            // 
            // txtMachineDescription
            // 
            this.txtMachineDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMachineDescription.Location = new System.Drawing.Point(12, 208);
            this.txtMachineDescription.Multiline = true;
            this.txtMachineDescription.Name = "txtMachineDescription";
            this.txtMachineDescription.ReadOnly = true;
            this.txtMachineDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMachineDescription.Size = new System.Drawing.Size(579, 79);
            this.txtMachineDescription.TabIndex = 5;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(455, 484);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(119, 23);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmbCodeEditorType
            // 
            this.cmbCodeEditorType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbCodeEditorType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCodeEditorType.FormattingEnabled = true;
            this.cmbCodeEditorType.Location = new System.Drawing.Point(101, 293);
            this.cmbCodeEditorType.Name = "cmbCodeEditorType";
            this.cmbCodeEditorType.Size = new System.Drawing.Size(409, 21);
            this.cmbCodeEditorType.TabIndex = 6;
            // 
            // lblCodeEditor
            // 
            this.lblCodeEditor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCodeEditor.AutoSize = true;
            this.lblCodeEditor.Location = new System.Drawing.Point(30, 296);
            this.lblCodeEditor.Name = "lblCodeEditor";
            this.lblCodeEditor.Size = new System.Drawing.Size(65, 13);
            this.lblCodeEditor.TabIndex = 7;
            this.lblCodeEditor.Text = "Code Editor:";
            // 
            // fraAddOutputPort
            // 
            this.fraAddOutputPort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fraAddOutputPort.Controls.Add(this.txtOutputPortIndex);
            this.fraAddOutputPort.Controls.Add(this.lblOutputPortIndex);
            this.fraAddOutputPort.Controls.Add(this.cmbOutputPortType);
            this.fraAddOutputPort.Controls.Add(this.optNoneOutputPort);
            this.fraAddOutputPort.Controls.Add(this.optEmptyOutputPort);
            this.fraAddOutputPort.Location = new System.Drawing.Point(310, 327);
            this.fraAddOutputPort.Name = "fraAddOutputPort";
            this.fraAddOutputPort.Size = new System.Drawing.Size(281, 95);
            this.fraAddOutputPort.TabIndex = 8;
            this.fraAddOutputPort.TabStop = false;
            this.fraAddOutputPort.Text = "Add Output Port";
            // 
            // txtOutputPortIndex
            // 
            this.txtOutputPortIndex.Location = new System.Drawing.Point(127, 41);
            this.txtOutputPortIndex.Name = "txtOutputPortIndex";
            this.txtOutputPortIndex.Size = new System.Drawing.Size(40, 20);
            this.txtOutputPortIndex.TabIndex = 3;
            this.txtOutputPortIndex.Text = "0";
            // 
            // lblOutputPortIndex
            // 
            this.lblOutputPortIndex.AutoSize = true;
            this.lblOutputPortIndex.Location = new System.Drawing.Point(64, 44);
            this.lblOutputPortIndex.Name = "lblOutputPortIndex";
            this.lblOutputPortIndex.Size = new System.Drawing.Size(66, 13);
            this.lblOutputPortIndex.TabIndex = 2;
            this.lblOutputPortIndex.Text = "at port slot #";
            // 
            // cmbOutputPortType
            // 
            this.cmbOutputPortType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbOutputPortType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOutputPortType.FormattingEnabled = true;
            this.cmbOutputPortType.Location = new System.Drawing.Point(21, 65);
            this.cmbOutputPortType.Name = "cmbOutputPortType";
            this.cmbOutputPortType.Size = new System.Drawing.Size(251, 21);
            this.cmbOutputPortType.TabIndex = 1;
            // 
            // optNoneOutputPort
            // 
            this.optNoneOutputPort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.optNoneOutputPort.Location = new System.Drawing.Point(6, 19);
            this.optNoneOutputPort.Name = "optNoneOutputPort";
            this.optNoneOutputPort.Size = new System.Drawing.Size(266, 17);
            this.optNoneOutputPort.TabIndex = 0;
            this.optNoneOutputPort.Text = "Don\'t";
            this.optNoneOutputPort.UseVisualStyleBackColor = true;
            this.optNoneOutputPort.CheckedChanged += new System.EventHandler(this.optWithOutputPort_CheckedChanged);
            // 
            // optEmptyOutputPort
            // 
            this.optEmptyOutputPort.AutoSize = true;
            this.optEmptyOutputPort.Checked = true;
            this.optEmptyOutputPort.Location = new System.Drawing.Point(6, 42);
            this.optEmptyOutputPort.Name = "optEmptyOutputPort";
            this.optEmptyOutputPort.Size = new System.Drawing.Size(54, 17);
            this.optEmptyOutputPort.TabIndex = 0;
            this.optEmptyOutputPort.TabStop = true;
            this.optEmptyOutputPort.Text = "Empty";
            this.optEmptyOutputPort.UseVisualStyleBackColor = true;
            this.optEmptyOutputPort.CheckedChanged += new System.EventHandler(this.optWithOutputPort_CheckedChanged);
            // 
            // chkConnectNewConsole
            // 
            this.chkConnectNewConsole.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkConnectNewConsole.Location = new System.Drawing.Point(33, 428);
            this.chkConnectNewConsole.Name = "chkConnectNewConsole";
            this.chkConnectNewConsole.Size = new System.Drawing.Size(469, 17);
            this.chkConnectNewConsole.TabIndex = 9;
            this.chkConnectNewConsole.Text = "Connect new Standard Console Device to that ports";
            this.chkConnectNewConsole.UseVisualStyleBackColor = true;
            // 
            // lblComponentNotification
            // 
            this.lblComponentNotification.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblComponentNotification.AutoSize = true;
            this.lblComponentNotification.Location = new System.Drawing.Point(30, 457);
            this.lblComponentNotification.Name = "lblComponentNotification";
            this.lblComponentNotification.Size = new System.Drawing.Size(472, 13);
            this.lblComponentNotification.TabIndex = 10;
            this.lblComponentNotification.Text = "Notice that you can add and manage I/O ports and devices at any stage of using th" +
    "e IDE.";
            // 
            // NewIDEdialog
            // 
            this.AcceptButton = this.cmdCreate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(603, 519);
            this.Controls.Add(this.lblComponentNotification);
            this.Controls.Add(this.chkConnectNewConsole);
            this.Controls.Add(this.fraAddOutputPort);
            this.Controls.Add(this.lblCodeEditor);
            this.Controls.Add(this.cmbCodeEditorType);
            this.Controls.Add(this.txtMachineDescription);
            this.Controls.Add(this.fraAddInputPort);
            this.Controls.Add(this.lstLanguages);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdCreate);
            this.Controls.Add(this.lstMachineTypes);
            this.Name = "NewIDEdialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New IDE";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NewIdeDialog_FormClosing);
            this.fraAddInputPort.ResumeLayout(false);
            this.fraAddInputPort.PerformLayout();
            this.fraAddOutputPort.ResumeLayout(false);
            this.fraAddOutputPort.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lstMachineTypes;
        private System.Windows.Forms.Button cmdCreate;
        private System.Windows.Forms.ListBox lstLanguages;
        private System.Windows.Forms.GroupBox fraAddInputPort;
        private System.Windows.Forms.TextBox txtInputPortIndex;
        private System.Windows.Forms.Label lblInputPortIndex;
        private System.Windows.Forms.ComboBox cmbInputPortType;
        private System.Windows.Forms.RadioButton optNoneInputPort;
        private System.Windows.Forms.RadioButton optEmptyInputPort;
        private System.Windows.Forms.TextBox txtMachineDescription;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.ToolTip tipMain;
        private System.Windows.Forms.ComboBox cmbCodeEditorType;
        private System.Windows.Forms.Label lblCodeEditor;
        private System.Windows.Forms.GroupBox fraAddOutputPort;
        private System.Windows.Forms.TextBox txtOutputPortIndex;
        private System.Windows.Forms.Label lblOutputPortIndex;
        private System.Windows.Forms.ComboBox cmbOutputPortType;
        private System.Windows.Forms.RadioButton optNoneOutputPort;
        private System.Windows.Forms.RadioButton optEmptyOutputPort;
        private System.Windows.Forms.CheckBox chkConnectNewConsole;
        private System.Windows.Forms.Label lblComponentNotification;
    }
}