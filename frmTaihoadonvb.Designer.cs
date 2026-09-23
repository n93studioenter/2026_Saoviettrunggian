namespace SaovietTax
{
    partial class frmTaihoadonvb
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
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.progressPanel1 = new DevExpress.XtraWaitForm.ProgressPanel();
            this.SuspendLayout();
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(48, 428);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(94, 29);
            this.simpleButton1.TabIndex = 0;
            this.simpleButton1.Text = "simpleButton1";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // progressPanel1
            // 
            this.progressPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.progressPanel1.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.progressPanel1.Appearance.Options.UseBackColor = true;
            this.progressPanel1.AppearanceCaption.ForeColor = System.Drawing.Color.White;
            this.progressPanel1.AppearanceCaption.Options.UseForeColor = true;
            this.progressPanel1.AppearanceDescription.ForeColor = System.Drawing.Color.White;
            this.progressPanel1.AppearanceDescription.Options.UseForeColor = true;
            this.progressPanel1.Location = new System.Drawing.Point(3, -5);
            this.progressPanel1.Name = "progressPanel1";
            this.progressPanel1.Size = new System.Drawing.Size(361, 66);
            this.progressPanel1.TabIndex = 1;
            this.progressPanel1.Text = "progressPanel1";
            this.progressPanel1.Click += new System.EventHandler(this.progressPanel1_Click);
            // 
            // frmTaihoadonvb
            // 
            this.Appearance.BackColor = System.Drawing.Color.Red;
            this.Appearance.Options.UseBackColor = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 61);
            this.Controls.Add(this.progressPanel1);
            this.Controls.Add(this.simpleButton1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmTaihoadonvb";
            this.Text = "frmTaihoadonvb";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmTaihoadonvb_FormClosed);
            this.Load += new System.EventHandler(this.frmTaihoadonvb_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraWaitForm.ProgressPanel progressPanel1;
    }
}