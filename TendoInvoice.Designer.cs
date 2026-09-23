namespace SaovietTax
{
    partial class TendoInvoice
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
            this.webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.progressPanel1 = new DevExpress.XtraWaitForm.ProgressPanel();
            ((System.ComponentModel.ISupportInitialize)(this.webView21)).BeginInit();
            this.SuspendLayout();
            // 
            // webView21
            // 
            this.webView21.AllowExternalDrop = true;
            this.webView21.CreationProperties = null;
            this.webView21.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView21.Location = new System.Drawing.Point(0, 80);
            this.webView21.Name = "webView21";
            this.webView21.Size = new System.Drawing.Size(377, 18);
            this.webView21.TabIndex = 0;
            this.webView21.Visible = false;
            this.webView21.ZoomFactor = 1D;
            // 
            // progressPanel1
            // 
            this.progressPanel1.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.progressPanel1.Appearance.ForeColor = System.Drawing.Color.White;
            this.progressPanel1.Appearance.Options.UseBackColor = true;
            this.progressPanel1.Appearance.Options.UseForeColor = true;
            this.progressPanel1.AppearanceCaption.ForeColor = System.Drawing.Color.White;
            this.progressPanel1.AppearanceCaption.Options.UseForeColor = true;
            this.progressPanel1.AppearanceDescription.ForeColor = System.Drawing.Color.White;
            this.progressPanel1.AppearanceDescription.Options.UseForeColor = true;
            this.progressPanel1.Location = new System.Drawing.Point(13, 13);
            this.progressPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.progressPanel1.Name = "progressPanel1";
            this.progressPanel1.Size = new System.Drawing.Size(363, 41);
            this.progressPanel1.TabIndex = 1;
            this.progressPanel1.Text = "progressPanel1";
            this.progressPanel1.Click += new System.EventHandler(this.progressPanel1_Click);
            // 
            // TendoInvoice
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Firebrick;
            this.ClientSize = new System.Drawing.Size(389, 67);
            this.Controls.Add(this.progressPanel1);
            this.Controls.Add(this.webView21);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "TendoInvoice";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TendoInvoice";
            this.Load += new System.EventHandler(this.TendoInvoice_Load);
            ((System.ComponentModel.ISupportInitialize)(this.webView21)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private DevExpress.XtraWaitForm.ProgressPanel progressPanel1;
    }
}