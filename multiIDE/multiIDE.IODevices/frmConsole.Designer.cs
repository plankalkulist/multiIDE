namespace multiIDE
{
    partial class frmConsole
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
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.rtbDisplay = new System.Windows.Forms.RichTextBox();
            this.cmnDisplay = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SuspendLayout();
            // 
            // rtbDisplay
            // 
            this.rtbDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbDisplay.BackColor = System.Drawing.Color.Black;
            this.rtbDisplay.ContextMenuStrip = this.cmnDisplay;
            this.rtbDisplay.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rtbDisplay.ForeColor = System.Drawing.Color.White;
            this.rtbDisplay.Location = new System.Drawing.Point(0, 0);
            this.rtbDisplay.Name = "rtbDisplay";
            this.rtbDisplay.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbDisplay.Size = new System.Drawing.Size(448, 137);
            this.rtbDisplay.TabIndex = 3;
            this.rtbDisplay.Text = "";
            // 
            // cmnDisplay
            // 
            this.cmnDisplay.Name = "cmnDisplay";
            this.cmnDisplay.Size = new System.Drawing.Size(61, 4);
            this.cmnDisplay.Opening += new System.ComponentModel.CancelEventHandler(this.cmnDisplay_Opening);
            // 
            // frmConsole
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(447, 137);
            this.Controls.Add(this.rtbDisplay);
            this.Name = "frmConsole";
            this.Text = "frmConsole";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmConsole_FormClosing);
            this.Load += new System.EventHandler(this.frmConsole_Load);
            this.ResumeLayout(false);

        }

        #endregion
        public System.ComponentModel.BackgroundWorker backgroundWorker;
        public System.Windows.Forms.RichTextBox rtbDisplay;
        private System.Windows.Forms.ContextMenuStrip cmnDisplay;
    }
}