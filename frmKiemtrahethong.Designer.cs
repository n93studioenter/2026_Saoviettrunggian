namespace SaovietTax
{
    partial class frmKiemtrahethong
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
            this.cbbChonthang = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.kTHeThongBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colSTT = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colType = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colSoHD = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colKHHD = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colNgayLap = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colNgayImport = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colMST = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTenKH = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTienTrcThue = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTienThue = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTongTienTT = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn3 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemButtonEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
            this.kTHeThongBindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.radDauvao = new System.Windows.Forms.RadioButton();
            this.radDaura = new System.Windows.Forms.RadioButton();
            this.cbbTrangThai = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.cbbChonthang.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kTHeThongBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemButtonEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kTHeThongBindingSource1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbbTrangThai.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // cbbChonthang
            // 
            this.cbbChonthang.Location = new System.Drawing.Point(106, 12);
            this.cbbChonthang.Name = "cbbChonthang";
            this.cbbChonthang.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbbChonthang.Size = new System.Drawing.Size(359, 23);
            this.cbbChonthang.TabIndex = 0;
            this.cbbChonthang.SelectedIndexChanged += new System.EventHandler(this.cbbChonthang_SelectedIndexChanged);
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(12, 15);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(65, 16);
            this.labelControl1.TabIndex = 1;
            this.labelControl1.Text = "Chọn tháng";
            // 
            // gridControl1
            // 
            this.gridControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridControl1.DataSource = this.kTHeThongBindingSource;
            this.gridControl1.Location = new System.Drawing.Point(12, 48);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemButtonEdit1});
            this.gridControl1.Size = new System.Drawing.Size(1421, 685);
            this.gridControl1.TabIndex = 2;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // kTHeThongBindingSource
            // 
            this.kTHeThongBindingSource.DataSource = typeof(SaovietTax.frmKiemtrahethong.KTHeThong);
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colSTT,
            this.colType,
            this.colSoHD,
            this.colKHHD,
            this.colNgayLap,
            this.colNgayImport,
            this.colMST,
            this.colTenKH,
            this.colTienTrcThue,
            this.colTienThue,
            this.colTongTienTT,
            this.gridColumn2,
            this.gridColumn1,
            this.gridColumn3});
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.CustomDrawCell += new DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventHandler(this.gridView1_CustomDrawCell);
            // 
            // colSTT
            // 
            this.colSTT.FieldName = "STT";
            this.colSTT.MinWidth = 25;
            this.colSTT.Name = "colSTT";
            this.colSTT.OptionsColumn.AllowEdit = false;
            this.colSTT.Visible = true;
            this.colSTT.VisibleIndex = 0;
            this.colSTT.Width = 79;
            // 
            // colType
            // 
            this.colType.FieldName = "Type";
            this.colType.MinWidth = 25;
            this.colType.Name = "colType";
            this.colType.OptionsColumn.AllowEdit = false;
            this.colType.Width = 194;
            // 
            // colSoHD
            // 
            this.colSoHD.FieldName = "SoHD";
            this.colSoHD.MinWidth = 25;
            this.colSoHD.Name = "colSoHD";
            this.colSoHD.OptionsColumn.AllowEdit = false;
            this.colSoHD.Visible = true;
            this.colSoHD.VisibleIndex = 1;
            this.colSoHD.Width = 78;
            // 
            // colKHHD
            // 
            this.colKHHD.FieldName = "KHHD";
            this.colKHHD.MinWidth = 25;
            this.colKHHD.Name = "colKHHD";
            this.colKHHD.OptionsColumn.AllowEdit = false;
            this.colKHHD.Visible = true;
            this.colKHHD.VisibleIndex = 2;
            this.colKHHD.Width = 103;
            // 
            // colNgayLap
            // 
            this.colNgayLap.FieldName = "NgayLap";
            this.colNgayLap.MinWidth = 25;
            this.colNgayLap.Name = "colNgayLap";
            this.colNgayLap.OptionsColumn.AllowEdit = false;
            this.colNgayLap.Visible = true;
            this.colNgayLap.VisibleIndex = 3;
            this.colNgayLap.Width = 111;
            // 
            // colNgayImport
            // 
            this.colNgayImport.FieldName = "NgayImport";
            this.colNgayImport.MinWidth = 25;
            this.colNgayImport.Name = "colNgayImport";
            this.colNgayImport.OptionsColumn.AllowEdit = false;
            this.colNgayImport.Width = 94;
            // 
            // colMST
            // 
            this.colMST.FieldName = "MST";
            this.colMST.MinWidth = 25;
            this.colMST.Name = "colMST";
            this.colMST.OptionsColumn.AllowEdit = false;
            this.colMST.Width = 97;
            // 
            // colTenKH
            // 
            this.colTenKH.FieldName = "TenKH";
            this.colTenKH.MinWidth = 25;
            this.colTenKH.Name = "colTenKH";
            this.colTenKH.OptionsColumn.AllowEdit = false;
            this.colTenKH.Visible = true;
            this.colTenKH.VisibleIndex = 4;
            this.colTenKH.Width = 205;
            // 
            // colTienTrcThue
            // 
            this.colTienTrcThue.Caption = "TienTrcThue";
            this.colTienTrcThue.DisplayFormat.FormatString = "N0";
            this.colTienTrcThue.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colTienTrcThue.FieldName = "TienTrcThue";
            this.colTienTrcThue.MinWidth = 25;
            this.colTienTrcThue.Name = "colTienTrcThue";
            this.colTienTrcThue.Visible = true;
            this.colTienTrcThue.VisibleIndex = 5;
            this.colTienTrcThue.Width = 100;
            // 
            // colTienThue
            // 
            this.colTienThue.Caption = "TienThue";
            this.colTienThue.DisplayFormat.FormatString = "N0";
            this.colTienThue.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colTienThue.FieldName = "TienThue";
            this.colTienThue.MinWidth = 25;
            this.colTienThue.Name = "colTienThue";
            this.colTienThue.Visible = true;
            this.colTienThue.VisibleIndex = 6;
            this.colTienThue.Width = 100;
            // 
            // colTongTienTT
            // 
            this.colTongTienTT.Caption = "TongTienTT";
            this.colTongTienTT.DisplayFormat.FormatString = "N0";
            this.colTongTienTT.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colTongTienTT.FieldName = "TongTienTT";
            this.colTongTienTT.MinWidth = 25;
            this.colTongTienTT.Name = "colTongTienTT";
            this.colTongTienTT.Visible = true;
            this.colTongTienTT.VisibleIndex = 7;
            this.colTongTienTT.Width = 114;
            // 
            // gridColumn2
            // 
            this.gridColumn2.Caption = "Hoá đơn đã nhập";
            this.gridColumn2.FieldName = "IsHD";
            this.gridColumn2.MinWidth = 25;
            this.gridColumn2.Name = "gridColumn2";
            this.gridColumn2.OptionsColumn.AllowEdit = false;
            this.gridColumn2.Visible = true;
            this.gridColumn2.VisibleIndex = 8;
            this.gridColumn2.Width = 133;
            // 
            // gridColumn1
            // 
            this.gridColumn1.Caption = "Hoá đơn đã import";
            this.gridColumn1.FieldName = "IsImport";
            this.gridColumn1.MinWidth = 25;
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.Visible = true;
            this.gridColumn1.VisibleIndex = 9;
            this.gridColumn1.Width = 180;
            // 
            // gridColumn3
            // 
            this.gridColumn3.Caption = "Tải hoá đơn";
            this.gridColumn3.FieldName = "TaiHD";
            this.gridColumn3.MinWidth = 25;
            this.gridColumn3.Name = "gridColumn3";
            this.gridColumn3.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
            this.gridColumn3.Visible = true;
            this.gridColumn3.VisibleIndex = 10;
            this.gridColumn3.Width = 94;
            // 
            // repositoryItemButtonEdit1
            // 
            this.repositoryItemButtonEdit1.AutoHeight = false;
            this.repositoryItemButtonEdit1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.repositoryItemButtonEdit1.Name = "repositoryItemButtonEdit1";
            // 
            // kTHeThongBindingSource1
            // 
            this.kTHeThongBindingSource1.DataSource = typeof(SaovietTax.frmKiemtrahethong.KTHeThong);
            // 
            // radDauvao
            // 
            this.radDauvao.AutoSize = true;
            this.radDauvao.Checked = true;
            this.radDauvao.Location = new System.Drawing.Point(528, 11);
            this.radDauvao.Name = "radDauvao";
            this.radDauvao.Size = new System.Drawing.Size(75, 20);
            this.radDauvao.TabIndex = 3;
            this.radDauvao.TabStop = true;
            this.radDauvao.Text = "Đầu vào";
            this.radDauvao.UseVisualStyleBackColor = true;
            this.radDauvao.CheckedChanged += new System.EventHandler(this.radDauvao_CheckedChanged);
            // 
            // radDaura
            // 
            this.radDaura.AutoSize = true;
            this.radDaura.Location = new System.Drawing.Point(655, 11);
            this.radDaura.Name = "radDaura";
            this.radDaura.Size = new System.Drawing.Size(67, 20);
            this.radDaura.TabIndex = 4;
            this.radDaura.Text = "Đầu ra";
            this.radDaura.UseVisualStyleBackColor = true;
            // 
            // cbbTrangThai
            // 
            this.cbbTrangThai.Location = new System.Drawing.Point(807, 8);
            this.cbbTrangThai.Name = "cbbTrangThai";
            this.cbbTrangThai.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbbTrangThai.Size = new System.Drawing.Size(145, 23);
            this.cbbTrangThai.TabIndex = 5;
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(736, 13);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(59, 16);
            this.labelControl2.TabIndex = 6;
            this.labelControl2.Text = "Trạng thái";
            // 
            // simpleButton1
            // 
            this.simpleButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton1.ImageOptions.Image = global::SaovietTax.Properties.Resources.sendxls_32x32;
            this.simpleButton1.Location = new System.Drawing.Point(1271, 2);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(162, 43);
            this.simpleButton1.TabIndex = 7;
            this.simpleButton1.Text = "Cập nhật file excel";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // frmKiemtrahethong
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1445, 745);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.cbbTrangThai);
            this.Controls.Add(this.radDaura);
            this.Controls.Add(this.radDauvao);
            this.Controls.Add(this.gridControl1);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.cbbChonthang);
            this.Name = "frmKiemtrahethong";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmKiemtrahethong";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.frmKiemtrahethong_Load);
            ((System.ComponentModel.ISupportInitialize)(this.cbbChonthang.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.kTHeThongBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemButtonEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.kTHeThongBindingSource1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbbTrangThai.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.ComboBoxEdit cbbChonthang;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private System.Windows.Forms.BindingSource kTHeThongBindingSource;
        private System.Windows.Forms.BindingSource kTHeThongBindingSource1;
        private DevExpress.XtraGrid.Columns.GridColumn colType;
        private DevExpress.XtraGrid.Columns.GridColumn colSoHD;
        private DevExpress.XtraGrid.Columns.GridColumn colKHHD;
        private DevExpress.XtraGrid.Columns.GridColumn colNgayLap;
        private DevExpress.XtraGrid.Columns.GridColumn colNgayImport;
        private DevExpress.XtraGrid.Columns.GridColumn colMST;
        private DevExpress.XtraGrid.Columns.GridColumn colTenKH;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;
        private DevExpress.XtraGrid.Columns.GridColumn colSTT;
        private System.Windows.Forms.RadioButton radDauvao;
        private System.Windows.Forms.RadioButton radDaura;
        private DevExpress.XtraGrid.Columns.GridColumn colTienTrcThue;
        private DevExpress.XtraGrid.Columns.GridColumn colTienThue;
        private DevExpress.XtraGrid.Columns.GridColumn colTongTienTT;
        private DevExpress.XtraEditors.ComboBoxEdit cbbTrangThai;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn3;
        private DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit repositoryItemButtonEdit1;
    }
}