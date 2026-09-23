namespace SaovietTax
{
    partial class SettingTaiCQT
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
            this.checkEdit3 = new DevExpress.XtraEditors.CheckEdit();
            this.checkEdit2 = new DevExpress.XtraEditors.CheckEdit();
            this.checkEdit1 = new DevExpress.XtraEditors.CheckEdit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit3.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit1.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // checkEdit3
            // 
            this.checkEdit3.EditValue = true;
            this.checkEdit3.Location = new System.Drawing.Point(12, 57);
            this.checkEdit3.Name = "checkEdit3";
            this.checkEdit3.Properties.Caption = "Cục Thuế đã nhận hóa đơn có mã khởi tạo từ máy tính tiền";
            this.checkEdit3.Size = new System.Drawing.Size(377, 20);
            this.checkEdit3.TabIndex = 2;
            this.checkEdit3.CheckedChanged += new System.EventHandler(this.checkEdit3_CheckedChanged);
            // 
            // checkEdit2
            // 
            this.checkEdit2.EditValue = true;
            this.checkEdit2.Location = new System.Drawing.Point(12, 31);
            this.checkEdit2.Name = "checkEdit2";
            this.checkEdit2.Properties.Caption = "Cục Thuế đã nhận không mã";
            this.checkEdit2.Size = new System.Drawing.Size(298, 20);
            this.checkEdit2.TabIndex = 1;
            this.checkEdit2.CheckedChanged += new System.EventHandler(this.checkEdit2_CheckedChanged);
            // 
            // checkEdit1
            // 
            this.checkEdit1.EditValue = true;
            this.checkEdit1.Location = new System.Drawing.Point(12, 5);
            this.checkEdit1.Name = "checkEdit1";
            this.checkEdit1.Properties.Caption = "Đã cấp mã hóa đơn";
            this.checkEdit1.Size = new System.Drawing.Size(298, 20);
            this.checkEdit1.TabIndex = 0;
            this.checkEdit1.CheckedChanged += new System.EventHandler(this.checkEdit1_CheckedChanged);
            // 
            // SettingTaiCQT
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(371, 88);
            this.Controls.Add(this.checkEdit3);
            this.Controls.Add(this.checkEdit2);
            this.Controls.Add(this.checkEdit1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SettingTaiCQT";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Thiết lập";
            this.Load += new System.EventHandler(this.SettingTaiCQT_Load);
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit3.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit1.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.CheckEdit checkEdit1;
        private DevExpress.XtraEditors.CheckEdit checkEdit2;
        private DevExpress.XtraEditors.CheckEdit checkEdit3;
    }
}