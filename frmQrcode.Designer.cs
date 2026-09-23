namespace SaovietTax
{
    partial class frmQrcode
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
            this.pictureBoxQR = new System.Windows.Forms.PictureBox();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxQR)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // simpleButton1
            // 
            this.simpleButton1.ImageOptions.Image = global::SaovietTax.Properties.Resources.print_32x32;
            this.simpleButton1.Location = new System.Drawing.Point(174, 280);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(112, 28);
            this.simpleButton1.TabIndex = 1;
            this.simpleButton1.Text = "Print";
            // 
            // pictureBoxQR
            // 
            this.pictureBoxQR.Location = new System.Drawing.Point(11, 12);
            this.pictureBoxQR.Name = "pictureBoxQR";
            this.pictureBoxQR.Size = new System.Drawing.Size(269, 174);
            this.pictureBoxQR.TabIndex = 0;
            this.pictureBoxQR.TabStop = false;
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.panel1);
            this.panelControl1.Controls.Add(this.labelControl3);
            this.panelControl1.Controls.Add(this.labelControl2);
            this.panelControl1.Controls.Add(this.labelControl1);
            this.panelControl1.Controls.Add(this.pictureBoxQR);
            this.panelControl1.Location = new System.Drawing.Point(1, 2);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(285, 272);
            this.panelControl1.TabIndex = 2;
            // 
            // labelControl1
            // 
            this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl1.Appearance.Options.UseFont = true;
            this.labelControl1.Location = new System.Drawing.Point(11, 192);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(115, 21);
            this.labelControl1.TabIndex = 1;
            this.labelControl1.Text = "labelControl1";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(11, 226);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(75, 16);
            this.labelControl2.TabIndex = 2;
            this.labelControl2.Text = "labelControl2";
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(11, 251);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(75, 16);
            this.labelControl3.TabIndex = 3;
            this.labelControl3.Text = "labelControl3";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.panel1.Location = new System.Drawing.Point(11, 219);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(268, 1);
            this.panel1.TabIndex = 4;
            // 
            // frmQrcode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(295, 322);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.simpleButton1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.IconOptions.Image = global::SaovietTax.Properties.Resources.qr_code;
            this.Name = "frmQrcode";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "In QRcode";
            this.Load += new System.EventHandler(this.frmQrcode_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxQR)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxQR;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private System.Windows.Forms.Panel panel1;
    }
}