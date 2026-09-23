namespace SaovietTax
{
    partial class frmLichsu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLichsu));
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.dtDenngay = new DevExpress.XtraEditors.DateEdit();
            this.dtTungay = new DevExpress.XtraEditors.DateEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.fileImportBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colID = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colSHDon = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colKHHDon = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colNLap = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTen = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.panelControl2 = new DevExpress.XtraEditors.PanelControl();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtDenngay.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDenngay.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTungay.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTungay.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileImportBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).BeginInit();
            this.panelControl2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelControl1
            // 
            this.panelControl1.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.panelControl1.Appearance.Options.UseBackColor = true;
            this.panelControl1.Controls.Add(this.radioButton2);
            this.panelControl1.Controls.Add(this.radioButton1);
            this.panelControl1.Controls.Add(this.simpleButton1);
            this.panelControl1.Controls.Add(this.dtDenngay);
            this.panelControl1.Controls.Add(this.dtTungay);
            this.panelControl1.Controls.Add(this.labelControl3);
            this.panelControl1.Controls.Add(this.labelControl1);
            this.panelControl1.Location = new System.Drawing.Point(9, 10);
            this.panelControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(1415, 98);
            this.panelControl1.TabIndex = 0;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.radioButton2.Location = new System.Drawing.Point(166, 56);
            this.radioButton2.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(67, 20);
            this.radioButton2.TabIndex = 32;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Đầu ra";
            this.radioButton2.UseVisualStyleBackColor = false;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.radioButton1.Checked = true;
            this.radioButton1.Location = new System.Drawing.Point(11, 56);
            this.radioButton1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(75, 20);
            this.radioButton1.TabIndex = 31;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Đầu vào";
            this.radioButton1.UseVisualStyleBackColor = false;
            // 
            // simpleButton1
            // 
            this.simpleButton1.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("simpleButton1.ImageOptions.Image")));
            this.simpleButton1.Location = new System.Drawing.Point(616, 8);
            this.simpleButton1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(94, 31);
            this.simpleButton1.TabIndex = 30;
            this.simpleButton1.Text = "Lọc";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // dtDenngay
            // 
            this.dtDenngay.EditValue = null;
            this.dtDenngay.Location = new System.Drawing.Point(370, 13);
            this.dtDenngay.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.dtDenngay.Name = "dtDenngay";
            this.dtDenngay.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtDenngay.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtDenngay.Size = new System.Drawing.Size(201, 23);
            this.dtDenngay.TabIndex = 26;
            // 
            // dtTungay
            // 
            this.dtTungay.EditValue = null;
            this.dtTungay.Location = new System.Drawing.Point(93, 13);
            this.dtTungay.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.dtTungay.Name = "dtTungay";
            this.dtTungay.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtTungay.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtTungay.Size = new System.Drawing.Size(170, 23);
            this.dtTungay.TabIndex = 27;
            // 
            // labelControl3
            // 
            this.labelControl3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelControl3.Location = new System.Drawing.Point(281, 16);
            this.labelControl3.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(59, 16);
            this.labelControl3.TabIndex = 25;
            this.labelControl3.Text = "Đến tháng";
            // 
            // labelControl1
            // 
            this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelControl1.Location = new System.Drawing.Point(11, 16);
            this.labelControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(52, 16);
            this.labelControl1.TabIndex = 24;
            this.labelControl1.Text = "Từ tháng";
            // 
            // gridControl1
            // 
            this.gridControl1.DataSource = this.fileImportBindingSource;
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gridControl1.Location = new System.Drawing.Point(3, 3);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1420, 487);
            this.gridControl1.TabIndex = 1;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // fileImportBindingSource
            // 
            this.fileImportBindingSource.DataSource = typeof(SaovietTax.frmMain.FileImport);
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colID,
            this.colSHDon,
            this.colKHHDon,
            this.colNLap,
            this.colTen,
            this.gridColumn1,
            this.gridColumn2});
            this.gridView1.DetailHeight = 295;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsView.ShowGroupPanel = false;
            // 
            // colID
            // 
            this.colID.FieldName = "ID";
            this.colID.MinWidth = 23;
            this.colID.Name = "colID";
            this.colID.OptionsColumn.AllowEdit = false;
            this.colID.Visible = true;
            this.colID.VisibleIndex = 0;
            this.colID.Width = 72;
            // 
            // colSHDon
            // 
            this.colSHDon.Caption = "Số hóa đơn";
            this.colSHDon.FieldName = "SHDon";
            this.colSHDon.MinWidth = 23;
            this.colSHDon.Name = "colSHDon";
            this.colSHDon.OptionsColumn.AllowEdit = false;
            this.colSHDon.Visible = true;
            this.colSHDon.VisibleIndex = 1;
            this.colSHDon.Width = 135;
            // 
            // colKHHDon
            // 
            this.colKHHDon.Caption = "Kí hiệu hóa đơn";
            this.colKHHDon.FieldName = "KHHDon";
            this.colKHHDon.MinWidth = 23;
            this.colKHHDon.Name = "colKHHDon";
            this.colKHHDon.OptionsColumn.AllowEdit = false;
            this.colKHHDon.Visible = true;
            this.colKHHDon.VisibleIndex = 2;
            this.colKHHDon.Width = 141;
            // 
            // colNLap
            // 
            this.colNLap.Caption = "Ngày lập";
            this.colNLap.FieldName = "NLap";
            this.colNLap.MinWidth = 23;
            this.colNLap.Name = "colNLap";
            this.colNLap.OptionsColumn.AllowEdit = false;
            this.colNLap.Visible = true;
            this.colNLap.VisibleIndex = 3;
            this.colNLap.Width = 201;
            // 
            // colTen
            // 
            this.colTen.Caption = "Tên khách hàng";
            this.colTen.FieldName = "Ten";
            this.colTen.MinWidth = 23;
            this.colTen.Name = "colTen";
            this.colTen.OptionsColumn.AllowEdit = false;
            this.colTen.Visible = true;
            this.colTen.VisibleIndex = 4;
            this.colTen.Width = 605;
            // 
            // gridColumn1
            // 
            this.gridColumn1.Caption = "Trạng thái";
            this.gridColumn1.FieldName = "Status";
            this.gridColumn1.MinWidth = 23;
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.OptionsColumn.AllowEdit = false;
            this.gridColumn1.Visible = true;
            this.gridColumn1.VisibleIndex = 5;
            this.gridColumn1.Width = 168;
            // 
            // gridColumn2
            // 
            this.gridColumn2.Caption = "Xoá";
            this.gridColumn2.FieldName = "Xoa";
            this.gridColumn2.MinWidth = 25;
            this.gridColumn2.Name = "gridColumn2";
            this.gridColumn2.Visible = true;
            this.gridColumn2.VisibleIndex = 6;
            this.gridColumn2.Width = 77;
            // 
            // panelControl2
            // 
            this.panelControl2.Controls.Add(this.gridControl1);
            this.panelControl2.Location = new System.Drawing.Point(2, 129);
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(1426, 493);
            this.panelControl2.TabIndex = 2;
            // 
            // frmLichsu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1435, 649);
            this.Controls.Add(this.panelControl2);
            this.Controls.Add(this.panelControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "frmLichsu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Lịch sử import ";
            this.Load += new System.EventHandler(this.frmLichsu_Load);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtDenngay.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDenngay.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTungay.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtTungay.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileImportBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).EndInit();
            this.panelControl2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.DateEdit dtDenngay;
        private DevExpress.XtraEditors.DateEdit dtTungay;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.BindingSource fileImportBindingSource;
        private DevExpress.XtraGrid.Columns.GridColumn colID;
        private DevExpress.XtraGrid.Columns.GridColumn colSHDon;
        private DevExpress.XtraGrid.Columns.GridColumn colKHHDon;
        private DevExpress.XtraGrid.Columns.GridColumn colNLap;
        private DevExpress.XtraGrid.Columns.GridColumn colTen;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;
        private DevExpress.XtraEditors.PanelControl panelControl2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
    }
}