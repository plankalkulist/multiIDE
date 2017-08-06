namespace multiIDE.Dialogs
{
    partial class DllImportDialog
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
            this.lstTypeInfosFromSource = new System.Windows.Forms.ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblSourceFileName = new System.Windows.Forms.Label();
            this.txtSourceFileName = new System.Windows.Forms.TextBox();
            this.cmdChooseFile = new System.Windows.Forms.Button();
            this.lblChooseTypesToImport = new System.Windows.Forms.Label();
            this.lblInternal = new System.Windows.Forms.Label();
            this.lblNamespace = new System.Windows.Forms.Label();
            this.optDefaultNamespace = new System.Windows.Forms.RadioButton();
            this.optAnyNamespaces = new System.Windows.Forms.RadioButton();
            this.cmdSelectAll = new System.Windows.Forms.Button();
            this.cmdDeselectAll = new System.Windows.Forms.Button();
            this.cmdImport = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdDeselect = new System.Windows.Forms.Button();
            this.cmdSelect = new System.Windows.Forms.Button();
            this.tipMain = new System.Windows.Forms.ToolTip(this.components);
            this.optShowOnlyNewTypes = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lstTypeInfosFromSource
            // 
            this.lstTypeInfosFromSource.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstTypeInfosFromSource.CheckBoxes = true;
            this.lstTypeInfosFromSource.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.lstTypeInfosFromSource.ForeColor = System.Drawing.Color.Black;
            this.lstTypeInfosFromSource.FullRowSelect = true;
            this.lstTypeInfosFromSource.LabelWrap = false;
            this.lstTypeInfosFromSource.Location = new System.Drawing.Point(12, 79);
            this.lstTypeInfosFromSource.Name = "lstTypeInfosFromSource";
            this.lstTypeInfosFromSource.ShowItemToolTips = true;
            this.lstTypeInfosFromSource.Size = new System.Drawing.Size(669, 173);
            this.lstTypeInfosFromSource.TabIndex = 7;
            this.lstTypeInfosFromSource.UseCompatibleStateImageBehavior = false;
            this.lstTypeInfosFromSource.View = System.Windows.Forms.View.Details;
            this.lstTypeInfosFromSource.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lstTypeInfosFromSource_ItemCheck);
            this.lstTypeInfosFromSource.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lstTypeInfosFromSource_ItemChecked);
            this.lstTypeInfosFromSource.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lstTypeInfosFromSource_ItemSelectionChanged);
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Type Name";
            this.columnHeader2.Width = 135;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Default Name";
            this.columnHeader3.Width = 179;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Version";
            this.columnHeader4.Width = 75;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Author";
            this.columnHeader5.Width = 93;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Type Full Name";
            this.columnHeader6.Width = 174;
            // 
            // lblSourceFileName
            // 
            this.lblSourceFileName.AutoSize = true;
            this.lblSourceFileName.Location = new System.Drawing.Point(13, 13);
            this.lblSourceFileName.Name = "lblSourceFileName";
            this.lblSourceFileName.Size = new System.Drawing.Size(89, 13);
            this.lblSourceFileName.TabIndex = 8;
            this.lblSourceFileName.Text = "Source file name:";
            // 
            // txtSourceFileName
            // 
            this.txtSourceFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSourceFileName.Location = new System.Drawing.Point(118, 10);
            this.txtSourceFileName.Name = "txtSourceFileName";
            this.txtSourceFileName.ReadOnly = true;
            this.txtSourceFileName.Size = new System.Drawing.Size(416, 20);
            this.txtSourceFileName.TabIndex = 9;
            // 
            // cmdChooseFile
            // 
            this.cmdChooseFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdChooseFile.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmdChooseFile.Location = new System.Drawing.Point(540, 8);
            this.cmdChooseFile.Name = "cmdChooseFile";
            this.cmdChooseFile.Size = new System.Drawing.Size(36, 22);
            this.cmdChooseFile.TabIndex = 10;
            this.cmdChooseFile.Text = "...";
            this.cmdChooseFile.UseVisualStyleBackColor = true;
            this.cmdChooseFile.Click += new System.EventHandler(this.cmdChooseFile_Click);
            // 
            // lblChooseTypesToImport
            // 
            this.lblChooseTypesToImport.AutoSize = true;
            this.lblChooseTypesToImport.Location = new System.Drawing.Point(13, 63);
            this.lblChooseTypesToImport.Name = "lblChooseTypesToImport";
            this.lblChooseTypesToImport.Size = new System.Drawing.Size(117, 13);
            this.lblChooseTypesToImport.TabIndex = 11;
            this.lblChooseTypesToImport.Text = "Choose types to import:";
            // 
            // lblInternal
            // 
            this.lblInternal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInternal.AutoSize = true;
            this.lblInternal.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblInternal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblInternal.Location = new System.Drawing.Point(582, 13);
            this.lblInternal.Name = "lblInternal";
            this.lblInternal.Size = new System.Drawing.Size(84, 13);
            this.lblInternal.TabIndex = 12;
            this.lblInternal.Text = "multiIDE Internal";
            this.lblInternal.Click += new System.EventHandler(this.lblInternal_Click);
            // 
            // lblNamespace
            // 
            this.lblNamespace.AutoSize = true;
            this.lblNamespace.Location = new System.Drawing.Point(13, 38);
            this.lblNamespace.Name = "lblNamespace";
            this.lblNamespace.Size = new System.Drawing.Size(91, 13);
            this.lblNamespace.TabIndex = 13;
            this.lblNamespace.Text = "From namespace:";
            // 
            // optDefaultNamespace
            // 
            this.optDefaultNamespace.AutoSize = true;
            this.optDefaultNamespace.Checked = true;
            this.optDefaultNamespace.Location = new System.Drawing.Point(195, 36);
            this.optDefaultNamespace.Name = "optDefaultNamespace";
            this.optDefaultNamespace.Size = new System.Drawing.Size(137, 17);
            this.optDefaultNamespace.TabIndex = 14;
            this.optDefaultNamespace.TabStop = true;
            this.optDefaultNamespace.Text = "default namespace only";
            this.optDefaultNamespace.UseVisualStyleBackColor = true;
            // 
            // optAnyNamespaces
            // 
            this.optAnyNamespaces.AutoSize = true;
            this.optAnyNamespaces.Location = new System.Drawing.Point(131, 36);
            this.optAnyNamespaces.Name = "optAnyNamespaces";
            this.optAnyNamespaces.Size = new System.Drawing.Size(43, 17);
            this.optAnyNamespaces.TabIndex = 14;
            this.optAnyNamespaces.Text = "Any";
            this.optAnyNamespaces.UseVisualStyleBackColor = true;
            // 
            // cmdSelectAll
            // 
            this.cmdSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdSelectAll.Location = new System.Drawing.Point(180, 258);
            this.cmdSelectAll.Name = "cmdSelectAll";
            this.cmdSelectAll.Size = new System.Drawing.Size(73, 23);
            this.cmdSelectAll.TabIndex = 15;
            this.cmdSelectAll.Text = "Select All";
            this.cmdSelectAll.UseVisualStyleBackColor = true;
            this.cmdSelectAll.Click += new System.EventHandler(this.cmdSelectAll_Click);
            // 
            // cmdDeselectAll
            // 
            this.cmdDeselectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdDeselectAll.Location = new System.Drawing.Point(259, 258);
            this.cmdDeselectAll.Name = "cmdDeselectAll";
            this.cmdDeselectAll.Size = new System.Drawing.Size(73, 23);
            this.cmdDeselectAll.TabIndex = 15;
            this.cmdDeselectAll.Text = "Deselect All";
            this.cmdDeselectAll.UseVisualStyleBackColor = true;
            this.cmdDeselectAll.Click += new System.EventHandler(this.cmdDeselectAll_Click);
            // 
            // cmdImport
            // 
            this.cmdImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdImport.Enabled = false;
            this.cmdImport.Location = new System.Drawing.Point(440, 258);
            this.cmdImport.Name = "cmdImport";
            this.cmdImport.Size = new System.Drawing.Size(101, 23);
            this.cmdImport.TabIndex = 15;
            this.cmdImport.Text = "Import";
            this.cmdImport.UseVisualStyleBackColor = true;
            this.cmdImport.Click += new System.EventHandler(this.cmdImport_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(560, 258);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(101, 23);
            this.cmdCancel.TabIndex = 15;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdDeselect
            // 
            this.cmdDeselect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdDeselect.Enabled = false;
            this.cmdDeselect.Location = new System.Drawing.Point(101, 258);
            this.cmdDeselect.Name = "cmdDeselect";
            this.cmdDeselect.Size = new System.Drawing.Size(73, 23);
            this.cmdDeselect.TabIndex = 15;
            this.cmdDeselect.Text = "Deselect";
            this.cmdDeselect.UseVisualStyleBackColor = true;
            this.cmdDeselect.Click += new System.EventHandler(this.cmdDeselect_Click);
            // 
            // cmdSelect
            // 
            this.cmdSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdSelect.Enabled = false;
            this.cmdSelect.Location = new System.Drawing.Point(22, 258);
            this.cmdSelect.Name = "cmdSelect";
            this.cmdSelect.Size = new System.Drawing.Size(73, 23);
            this.cmdSelect.TabIndex = 15;
            this.cmdSelect.Text = "Select";
            this.cmdSelect.UseVisualStyleBackColor = true;
            this.cmdSelect.Click += new System.EventHandler(this.cmdSelect_Click);
            // 
            // optShowOnlyNewTypes
            // 
            this.optShowOnlyNewTypes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.optShowOnlyNewTypes.AutoSize = true;
            this.optShowOnlyNewTypes.Location = new System.Drawing.Point(542, 62);
            this.optShowOnlyNewTypes.Name = "optShowOnlyNewTypes";
            this.optShowOnlyNewTypes.Size = new System.Drawing.Size(124, 17);
            this.optShowOnlyNewTypes.TabIndex = 16;
            this.optShowOnlyNewTypes.Text = "Show only new ones";
            this.optShowOnlyNewTypes.UseVisualStyleBackColor = true;
            // 
            // DllImportDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(693, 293);
            this.Controls.Add(this.optShowOnlyNewTypes);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdImport);
            this.Controls.Add(this.cmdDeselectAll);
            this.Controls.Add(this.cmdSelect);
            this.Controls.Add(this.cmdDeselect);
            this.Controls.Add(this.cmdSelectAll);
            this.Controls.Add(this.optAnyNamespaces);
            this.Controls.Add(this.optDefaultNamespace);
            this.Controls.Add(this.lblNamespace);
            this.Controls.Add(this.lblInternal);
            this.Controls.Add(this.lblChooseTypesToImport);
            this.Controls.Add(this.cmdChooseFile);
            this.Controls.Add(this.txtSourceFileName);
            this.Controls.Add(this.lblSourceFileName);
            this.Controls.Add(this.lstTypeInfosFromSource);
            this.Name = "DllImportDialog";
            this.Text = "Import Component Types";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DLLimportDialog_FormClosing);
            this.Load += new System.EventHandler(this.DLLimportDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lstTypeInfosFromSource;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.Label lblSourceFileName;
        private System.Windows.Forms.TextBox txtSourceFileName;
        private System.Windows.Forms.Button cmdChooseFile;
        private System.Windows.Forms.Label lblChooseTypesToImport;
        private System.Windows.Forms.Label lblInternal;
        private System.Windows.Forms.Label lblNamespace;
        private System.Windows.Forms.RadioButton optDefaultNamespace;
        private System.Windows.Forms.RadioButton optAnyNamespaces;
        private System.Windows.Forms.Button cmdSelectAll;
        private System.Windows.Forms.Button cmdDeselectAll;
        private System.Windows.Forms.Button cmdImport;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdDeselect;
        private System.Windows.Forms.Button cmdSelect;
        private System.Windows.Forms.ToolTip tipMain;
        private System.Windows.Forms.CheckBox optShowOnlyNewTypes;
    }
}