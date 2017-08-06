namespace multiIDE.Extras
{
    partial class ViewRAM
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
            this.lblCurrentCell = new System.Windows.Forms.Label();
            this.cmdDump = new System.Windows.Forms.Button();
            this.txtDump = new System.Windows.Forms.TextBox();
            this.txtLength = new System.Windows.Forms.TextBox();
            this.txtSince = new System.Windows.Forms.TextBox();
            this.chkAutoDump = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lblCurrentCell
            // 
            this.lblCurrentCell.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrentCell.AutoSize = true;
            this.lblCurrentCell.Location = new System.Drawing.Point(407, 16);
            this.lblCurrentCell.Name = "lblCurrentCell";
            this.lblCurrentCell.Size = new System.Drawing.Size(0, 13);
            this.lblCurrentCell.TabIndex = 8;
            // 
            // cmdDump
            // 
            this.cmdDump.Location = new System.Drawing.Point(232, 11);
            this.cmdDump.Name = "cmdDump";
            this.cmdDump.Size = new System.Drawing.Size(75, 23);
            this.cmdDump.TabIndex = 7;
            this.cmdDump.Text = "DUMP";
            this.cmdDump.UseVisualStyleBackColor = true;
            this.cmdDump.Click += new System.EventHandler(this.cmdDump_Click);
            // 
            // txtDump
            // 
            this.txtDump.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDump.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtDump.Location = new System.Drawing.Point(13, 44);
            this.txtDump.Multiline = true;
            this.txtDump.Name = "txtDump";
            this.txtDump.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDump.Size = new System.Drawing.Size(495, 197);
            this.txtDump.TabIndex = 6;
            // 
            // txtLength
            // 
            this.txtLength.Location = new System.Drawing.Point(119, 13);
            this.txtLength.Name = "txtLength";
            this.txtLength.Size = new System.Drawing.Size(78, 20);
            this.txtLength.TabIndex = 4;
            this.txtLength.Text = "100";
            // 
            // txtSince
            // 
            this.txtSince.Location = new System.Drawing.Point(35, 13);
            this.txtSince.Name = "txtSince";
            this.txtSince.Size = new System.Drawing.Size(78, 20);
            this.txtSince.TabIndex = 5;
            this.txtSince.Text = "0";
            // 
            // chkAutoDump
            // 
            this.chkAutoDump.AutoSize = true;
            this.chkAutoDump.Checked = true;
            this.chkAutoDump.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoDump.Location = new System.Drawing.Point(323, 15);
            this.chkAutoDump.Name = "chkAutoDump";
            this.chkAutoDump.Size = new System.Drawing.Size(79, 17);
            this.chkAutoDump.TabIndex = 9;
            this.chkAutoDump.Text = "Auto Dump";
            this.chkAutoDump.UseVisualStyleBackColor = true;
            // 
            // ViewRAM
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 252);
            this.Controls.Add(this.chkAutoDump);
            this.Controls.Add(this.lblCurrentCell);
            this.Controls.Add(this.cmdDump);
            this.Controls.Add(this.txtDump);
            this.Controls.Add(this.txtLength);
            this.Controls.Add(this.txtSince);
            this.Name = "ViewRAM";
            this.Text = "frmViewRAM";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ViewRAM_FormClosing);
            this.Load += new System.EventHandler(this.frmViewRAM_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Label lblCurrentCell;
        internal System.Windows.Forms.Button cmdDump;
        internal System.Windows.Forms.TextBox txtDump;
        internal System.Windows.Forms.TextBox txtLength;
        internal System.Windows.Forms.TextBox txtSince;
        private System.Windows.Forms.CheckBox chkAutoDump;
    }
}