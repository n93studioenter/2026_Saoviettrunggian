namespace SaovietTax
{
    partial class vb6TinhGiaVon
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
            this.dtTuNgay = new DevExpress.XtraEditors.DateEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.dtDenngay = new DevExpress.XtraEditors.DateEdit();
            this.txtSoHieu = new DevExpress.XtraEditors.TextEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.progressPanel1 = new DevExpress.XtraWaitForm.ProgressPanel();
            ((System.ComponentModel.ISupportInitialize)(this.dtTuNgay.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTuNgay.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDenngay.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDenngay.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSoHieu.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // dtTuNgay
            // 
            this.dtTuNgay.EditValue = null;
            this.dtTuNgay.Location = new System.Drawing.Point(120, 12);
            this.dtTuNgay.Name = "dtTuNgay";
            this.dtTuNgay.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtTuNgay.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtTuNgay.Size = new System.Drawing.Size(100, 20);
            this.dtTuNgay.TabIndex = 0;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(30, 15);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(40, 13);
            this.labelControl1.TabIndex = 1;
            this.labelControl1.Text = "Từ ngày";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(234, 15);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(47, 13);
            this.labelControl2.TabIndex = 3;
            this.labelControl2.Text = "Đến ngày";
            // 
            // dtDenngay
            // 
            this.dtDenngay.EditValue = null;
            this.dtDenngay.Location = new System.Drawing.Point(324, 12);
            this.dtDenngay.Name = "dtDenngay";
            this.dtDenngay.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtDenngay.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtDenngay.Size = new System.Drawing.Size(100, 20);
            this.dtDenngay.TabIndex = 2;
            // 
            // txtSoHieu
            // 
            this.txtSoHieu.Location = new System.Drawing.Point(500, 12);
            this.txtSoHieu.Name = "txtSoHieu";
            this.txtSoHieu.Size = new System.Drawing.Size(100, 20);
            this.txtSoHieu.TabIndex = 4;
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(437, 19);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(35, 13);
            this.labelControl3.TabIndex = 5;
            this.labelControl3.Text = "Số hiệu";
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(631, 7);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(100, 30);
            this.simpleButton1.TabIndex = 6;
            this.simpleButton1.Text = "Tính";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // progressPanel1
            // 
            this.progressPanel1.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.progressPanel1.Appearance.Options.UseBackColor = true;
            this.progressPanel1.Location = new System.Drawing.Point(30, 54);
            this.progressPanel1.Name = "progressPanel1";
            this.progressPanel1.Size = new System.Drawing.Size(701, 69);
            this.progressPanel1.TabIndex = 7;
            this.progressPanel1.Text = "progressPanel1";
            // 
            // vb6TinhGiaVon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(774, 147);
            this.Controls.Add(this.progressPanel1);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.labelControl3);
            this.Controls.Add(this.txtSoHieu);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.dtDenngay);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.dtTuNgay);
            this.Name = "vb6TinhGiaVon";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "vb6TinhGiaVon";
            ((System.ComponentModel.ISupportInitialize)(this.dtTuNgay.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTuNgay.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDenngay.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDenngay.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSoHieu.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.DateEdit dtTuNgay;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.DateEdit dtDenngay;
        private DevExpress.XtraEditors.TextEdit txtSoHieu;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraWaitForm.ProgressPanel progressPanel1;
    }
}