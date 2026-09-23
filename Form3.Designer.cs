namespace SaovietTax
{
    partial class Form3
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
            this.comboBoxEdit1 = new DevExpress.XtraEditors.ComboBoxEdit();
            this.comboBoxEdit2 = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.checkEdit1 = new DevExpress.XtraEditors.CheckEdit();
            this.checkEdit2 = new DevExpress.XtraEditors.CheckEdit();
            this.checkEdit3 = new DevExpress.XtraEditors.CheckEdit();
            this.btnExoprt = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit3.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBoxEdit1
            // 
            this.comboBoxEdit1.Location = new System.Drawing.Point(106, 129);
            this.comboBoxEdit1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxEdit1.Name = "comboBoxEdit1";
            this.comboBoxEdit1.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.comboBoxEdit1.Size = new System.Drawing.Size(178, 23);
            this.comboBoxEdit1.TabIndex = 0;
            this.comboBoxEdit1.SelectedIndexChanged += new System.EventHandler(this.comboBoxEdit1_SelectedIndexChanged);
            // 
            // comboBoxEdit2
            // 
            this.comboBoxEdit2.Location = new System.Drawing.Point(106, 181);
            this.comboBoxEdit2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxEdit2.Name = "comboBoxEdit2";
            this.comboBoxEdit2.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.comboBoxEdit2.Size = new System.Drawing.Size(178, 23);
            this.comboBoxEdit2.TabIndex = 1;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(25, 132);
            this.labelControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(52, 16);
            this.labelControl1.TabIndex = 2;
            this.labelControl1.Text = "Từ tháng";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(25, 185);
            this.labelControl2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(59, 16);
            this.labelControl2.TabIndex = 3;
            this.labelControl2.Text = "Đến tháng";
            // 
            // checkEdit1
            // 
            this.checkEdit1.Location = new System.Drawing.Point(30, 10);
            this.checkEdit1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkEdit1.Name = "checkEdit1";
            this.checkEdit1.Properties.Caption = "Sổ nhật ký chung";
            this.checkEdit1.Size = new System.Drawing.Size(176, 20);
            this.checkEdit1.TabIndex = 4;
            // 
            // checkEdit2
            // 
            this.checkEdit2.Location = new System.Drawing.Point(30, 42);
            this.checkEdit2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkEdit2.Name = "checkEdit2";
            this.checkEdit2.Properties.Caption = "Sổ cái tài khoản";
            this.checkEdit2.Size = new System.Drawing.Size(155, 20);
            this.checkEdit2.TabIndex = 5;
            // 
            // checkEdit3
            // 
            this.checkEdit3.Location = new System.Drawing.Point(30, 75);
            this.checkEdit3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkEdit3.Name = "checkEdit3";
            this.checkEdit3.Properties.Caption = "Sổ chi tiết tài khoản";
            this.checkEdit3.Size = new System.Drawing.Size(176, 20);
            this.checkEdit3.TabIndex = 6;
            // 
            // btnExoprt
            // 
            this.btnExoprt.Location = new System.Drawing.Point(106, 231);
            this.btnExoprt.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnExoprt.Name = "btnExoprt";
            this.btnExoprt.Size = new System.Drawing.Size(178, 36);
            this.btnExoprt.TabIndex = 7;
            this.btnExoprt.Text = "Xuất excel";
            this.btnExoprt.Click += new System.EventHandler(this.btnExoprt_Click);
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 260);
            this.Controls.Add(this.btnExoprt);
            this.Controls.Add(this.checkEdit3);
            this.Controls.Add(this.checkEdit2);
            this.Controls.Add(this.checkEdit1);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.comboBoxEdit2);
            this.Controls.Add(this.comboBoxEdit1);
            this.Name = "Form3";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Xuất excel";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Load += new System.EventHandler(this.Form3_Load);
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit3.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.ComboBoxEdit comboBoxEdit1;
        private DevExpress.XtraEditors.ComboBoxEdit comboBoxEdit2;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.CheckEdit checkEdit1;
        private DevExpress.XtraEditors.CheckEdit checkEdit2;
        private DevExpress.XtraEditors.CheckEdit checkEdit3;
        private DevExpress.XtraEditors.SimpleButton btnExoprt;
    }
}