namespace multiIDE.Extras
{
    partial class SettingWindow
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
            this.prgSettings = new System.Windows.Forms.PropertyGrid();
            this.cmdSwitchToDefaults = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // prgSettings
            // 
            this.prgSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.prgSettings.Location = new System.Drawing.Point(0, 0);
            this.prgSettings.Name = "prgSettings";
            this.prgSettings.Size = new System.Drawing.Size(259, 330);
            this.prgSettings.TabIndex = 1;
            // 
            // cmdSwitchToDefaults
            // 
            this.cmdSwitchToDefaults.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cmdSwitchToDefaults.Location = new System.Drawing.Point(26, 336);
            this.cmdSwitchToDefaults.Name = "cmdSwitchToDefaults";
            this.cmdSwitchToDefaults.Size = new System.Drawing.Size(208, 28);
            this.cmdSwitchToDefaults.TabIndex = 2;
            this.cmdSwitchToDefaults.Text = "Switch to defaults";
            this.cmdSwitchToDefaults.UseVisualStyleBackColor = true;
            this.cmdSwitchToDefaults.Click += new System.EventHandler(this.cmdSwitchToDefaults_Click);
            // 
            // SettingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(260, 376);
            this.Controls.Add(this.cmdSwitchToDefaults);
            this.Controls.Add(this.prgSettings);
            this.MaximizeBox = false;
            this.Name = "SettingDialog";
            this.Text = "SettingsDialog";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingDialog_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid prgSettings;
        private System.Windows.Forms.Button cmdSwitchToDefaults;
    }
}