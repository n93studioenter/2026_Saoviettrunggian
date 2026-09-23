namespace SaovietTax
{
    partial class frmStatusAuto
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
            this.progressPanel1 = new DevExpress.XtraWaitForm.ProgressPanel();
            this.SuspendLayout();
            // 
            // progressPanel1
            // 
            this.progressPanel1.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.progressPanel1.Appearance.Options.UseBackColor = true;
            this.progressPanel1.AppearanceDescription.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.progressPanel1.AppearanceDescription.ForeColor = System.Drawing.Color.Crimson;
            this.progressPanel1.AppearanceDescription.Options.UseFont = true;
            this.progressPanel1.AppearanceDescription.Options.UseForeColor = true;
            this.progressPanel1.Caption = "Đang tải hoá đơn đầu vào";
            this.progressPanel1.Description = "Công ty tnhh Thạnh hương bến đình";
            this.progressPanel1.Location = new System.Drawing.Point(12, 11);
            this.progressPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.progressPanel1.Name = "progressPanel1";
            this.progressPanel1.Size = new System.Drawing.Size(461, 47);
            this.progressPanel1.TabIndex = 0;
            this.progressPanel1.Text = "progressPanel1";
            // 
            // frmStatusAuto
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 69);
            this.Controls.Add(this.progressPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "frmStatusAuto";
            this.Text = "frmStatusAuto";
            this.Load += new System.EventHandler(this.frmStatusAuto_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraWaitForm.ProgressPanel progressPanel1;
    }
}