namespace SaovietTax
{
    partial class frmSetting
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
            this.chkSuggest = new DevExpress.XtraEditors.CheckEdit();
            this.checkEdit1 = new DevExpress.XtraEditors.CheckEdit();
            this.checkEdit2 = new DevExpress.XtraEditors.CheckEdit();
            this.checkEdit3 = new DevExpress.XtraEditors.CheckEdit();
            ((System.ComponentModel.ISupportInitialize)(this.chkSuggest.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit3.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // chkSuggest
            // 
            this.chkSuggest.Location = new System.Drawing.Point(10, 10);
            this.chkSuggest.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chkSuggest.Name = "chkSuggest";
            this.chkSuggest.Properties.Caption = "Hiện gợi ý sản phẩm";
            this.chkSuggest.Size = new System.Drawing.Size(147, 19);
            this.chkSuggest.TabIndex = 1;
            this.chkSuggest.CheckedChanged += new System.EventHandler(this.chkSuggest_CheckedChanged);
            // 
            // checkEdit1
            // 
            this.checkEdit1.Location = new System.Drawing.Point(10, 43);
            this.checkEdit1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.checkEdit1.Name = "checkEdit1";
            this.checkEdit1.Properties.Caption = "Tải hoá đơn gốc";
            this.checkEdit1.Size = new System.Drawing.Size(147, 19);
            this.checkEdit1.TabIndex = 2;
            // 
            // checkEdit2
            // 
            this.checkEdit2.Location = new System.Drawing.Point(10, 75);
            this.checkEdit2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.checkEdit2.Name = "checkEdit2";
            this.checkEdit2.Properties.Caption = "Bỏ tự động lọc";
            this.checkEdit2.Size = new System.Drawing.Size(147, 19);
            this.checkEdit2.TabIndex = 3;
            this.checkEdit2.CheckedChanged += new System.EventHandler(this.checkEdit2_CheckedChanged);
            // 
            // checkEdit3
            // 
            this.checkEdit3.Location = new System.Drawing.Point(10, 109);
            this.checkEdit3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.checkEdit3.Name = "checkEdit3";
            this.checkEdit3.Properties.Caption = "Quy đổi đơn vị tính";
            this.checkEdit3.Size = new System.Drawing.Size(147, 19);
            this.checkEdit3.TabIndex = 4;
            this.checkEdit3.CheckedChanged += new System.EventHandler(this.checkEdit3_CheckedChanged);
            // 
            // frmSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(179, 146);
            this.Controls.Add(this.checkEdit3);
            this.Controls.Add(this.checkEdit2);
            this.Controls.Add(this.checkEdit1);
            this.Controls.Add(this.chkSuggest);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "frmSetting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Thiết lập";
            this.Load += new System.EventHandler(this.frmSetting_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chkSuggest.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit3.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.CheckEdit chkSuggest;
        private DevExpress.XtraEditors.CheckEdit checkEdit1;
        private DevExpress.XtraEditors.CheckEdit checkEdit2;
        private DevExpress.XtraEditors.CheckEdit checkEdit3;
    }
}