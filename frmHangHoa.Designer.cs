namespace SaovietTax
{
    partial class frmHangHoa
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmHangHoa));
            this.gridControl2 = new DevExpress.XtraGrid.GridControl();
            this.gridView2 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colSoLuong = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDongia = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colThanhTien = new DevExpress.XtraGrid.Columns.GridColumn();
            this.txtdongia = new DevExpress.XtraEditors.TextEdit();
            this.txtGhichu = new DevExpress.XtraEditors.TextEdit();
            this.btnThem = new DevExpress.XtraEditors.SimpleButton();
            this.label4 = new System.Windows.Forms.Label();
            this.btnGhi = new DevExpress.XtraEditors.SimpleButton();
            this.btnXoa = new DevExpress.XtraEditors.SimpleButton();
            this.label7 = new System.Windows.Forms.Label();
            this.btnSearch = new DevExpress.XtraEditors.SimpleButton();
            this.txtsoluong = new DevExpress.XtraEditors.TextEdit();
            this.btnThoat = new DevExpress.XtraEditors.SimpleButton();
            this.label6 = new System.Windows.Forms.Label();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.textEdit2 = new DevExpress.XtraEditors.TextEdit();
            this.txtMaSo = new DevExpress.XtraEditors.TextEdit();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.textEdit1 = new DevExpress.XtraEditors.TextEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.txtSearch = new DevExpress.XtraEditors.TextEdit();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colTenVattu = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colSoHieu = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDonvitinh = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn3 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colPercent = new DevExpress.XtraGrid.Columns.GridColumn();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.simpleButton2 = new DevExpress.XtraEditors.SimpleButton();
            this.label5 = new System.Windows.Forms.Label();
            this.txtDonvi = new DevExpress.XtraEditors.TextEdit();
            this.label3 = new System.Windows.Forms.Label();
            this.txtTenvattu = new DevExpress.XtraEditors.TextEdit();
            this.comboBoxEdit1 = new DevExpress.XtraEditors.ComboBoxEdit();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSohieu = new DevExpress.XtraEditors.TextEdit();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtdongia.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtGhichu.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtsoluong.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMaSo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtDonvi.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTenvattu.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSohieu.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControl2
            // 
            this.gridControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridControl2.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gridControl2.Location = new System.Drawing.Point(1284, 657);
            this.gridControl2.MainView = this.gridView2;
            this.gridControl2.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gridControl2.Name = "gridControl2";
            this.gridControl2.Size = new System.Drawing.Size(51, 59);
            this.gridControl2.TabIndex = 16;
            this.gridControl2.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView2});
            this.gridControl2.Visible = false;
            // 
            // gridView2
            // 
            this.gridView2.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colSoLuong,
            this.colDongia,
            this.colThanhTien});
            this.gridView2.DetailHeight = 300;
            this.gridView2.GridControl = this.gridControl2;
            this.gridView2.Name = "gridView2";
            this.gridView2.OptionsView.ShowGroupPanel = false;
            // 
            // colSoLuong
            // 
            this.colSoLuong.Caption = "Số lượng";
            this.colSoLuong.DisplayFormat.FormatString = "N2";
            this.colSoLuong.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colSoLuong.FieldName = "SoLuong";
            this.colSoLuong.MinWidth = 25;
            this.colSoLuong.Name = "colSoLuong";
            this.colSoLuong.OptionsColumn.AllowEdit = false;
            this.colSoLuong.Width = 93;
            // 
            // colDongia
            // 
            this.colDongia.Caption = "Đơn giá";
            this.colDongia.DisplayFormat.FormatString = "N2";
            this.colDongia.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colDongia.FieldName = "DonGia";
            this.colDongia.MinWidth = 25;
            this.colDongia.Name = "colDongia";
            this.colDongia.OptionsColumn.AllowEdit = false;
            this.colDongia.Width = 93;
            // 
            // colThanhTien
            // 
            this.colThanhTien.Caption = "Thành tiền";
            this.colThanhTien.DisplayFormat.FormatString = "N2";
            this.colThanhTien.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colThanhTien.FieldName = "ThanhTien";
            this.colThanhTien.MinWidth = 25;
            this.colThanhTien.Name = "colThanhTien";
            this.colThanhTien.OptionsColumn.AllowEdit = false;
            this.colThanhTien.Visible = true;
            this.colThanhTien.VisibleIndex = 0;
            this.colThanhTien.Width = 93;
            // 
            // txtdongia
            // 
            this.txtdongia.Enabled = false;
            this.txtdongia.Location = new System.Drawing.Point(170, 297);
            this.txtdongia.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtdongia.Name = "txtdongia";
            this.txtdongia.Size = new System.Drawing.Size(300, 20);
            this.txtdongia.TabIndex = 36;
            this.txtdongia.Visible = false;
            // 
            // txtGhichu
            // 
            this.txtGhichu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGhichu.Location = new System.Drawing.Point(424, 32);
            this.txtGhichu.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtGhichu.Name = "txtGhichu";
            this.txtGhichu.Size = new System.Drawing.Size(235, 20);
            this.txtGhichu.TabIndex = 20;
            // 
            // btnThem
            // 
            this.btnThem.ImageOptions.Image = global::SaovietTax.Properties.Resources.add_16x16;
            this.btnThem.Location = new System.Drawing.Point(12, 54);
            this.btnThem.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnThem.Name = "btnThem";
            this.btnThem.Size = new System.Drawing.Size(82, 24);
            this.btnThem.TabIndex = 21;
            this.btnThem.Text = "Thêm";
            this.btnThem.Click += new System.EventHandler(this.btnThem_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(379, 34);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(42, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "Ghi chú";
            // 
            // btnGhi
            // 
            this.btnGhi.ImageOptions.Image = global::SaovietTax.Properties.Resources.saveto_16x16;
            this.btnGhi.Location = new System.Drawing.Point(98, 54);
            this.btnGhi.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnGhi.Name = "btnGhi";
            this.btnGhi.Size = new System.Drawing.Size(99, 24);
            this.btnGhi.TabIndex = 22;
            this.btnGhi.Text = "&Ghi";
            this.btnGhi.Click += new System.EventHandler(this.btnGhi_Click);
            // 
            // btnXoa
            // 
            this.btnXoa.ImageOptions.Image = global::SaovietTax.Properties.Resources.cancel_16x16;
            this.btnXoa.Location = new System.Drawing.Point(202, 54);
            this.btnXoa.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnXoa.Name = "btnXoa";
            this.btnXoa.Size = new System.Drawing.Size(102, 24);
            this.btnXoa.TabIndex = 23;
            this.btnXoa.Text = "Xóa";
            this.btnXoa.Click += new System.EventHandler(this.btnXoa_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(86, 205);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(44, 13);
            this.label7.TabIndex = 34;
            this.label7.Text = "Đơn giá";
            this.label7.Visible = false;
            // 
            // btnSearch
            // 
            this.btnSearch.ImageOptions.Image = global::SaovietTax.Properties.Resources.find_16x16;
            this.btnSearch.Location = new System.Drawing.Point(637, 58);
            this.btnSearch.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(20, 16);
            this.btnSearch.TabIndex = 26;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click_1);
            // 
            // txtsoluong
            // 
            this.txtsoluong.Enabled = false;
            this.txtsoluong.Location = new System.Drawing.Point(19, 297);
            this.txtsoluong.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtsoluong.Name = "txtsoluong";
            this.txtsoluong.Size = new System.Drawing.Size(283, 20);
            this.txtsoluong.TabIndex = 31;
            this.txtsoluong.Visible = false;
            // 
            // btnThoat
            // 
            this.btnThoat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnThoat.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnThoat.ImageOptions.Image")));
            this.btnThoat.Location = new System.Drawing.Point(95, 246);
            this.btnThoat.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnThoat.Name = "btnThoat";
            this.btnThoat.Size = new System.Drawing.Size(99, 29);
            this.btnThoat.TabIndex = 25;
            this.btnThoat.Text = "Thoát";
            this.btnThoat.Visible = false;
            this.btnThoat.Click += new System.EventHandler(this.btnThoat_Click_1);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 205);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(49, 13);
            this.label6.TabIndex = 28;
            this.label6.Text = "Số lượng";
            this.label6.Visible = false;
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(536, 327);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(139, 29);
            this.simpleButton1.TabIndex = 35;
            this.simpleButton1.Text = "Đánh giá tương đồng";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click_1);
            // 
            // textEdit2
            // 
            this.textEdit2.Location = new System.Drawing.Point(151, 364);
            this.textEdit2.Name = "textEdit2";
            this.textEdit2.Size = new System.Drawing.Size(378, 20);
            this.textEdit2.TabIndex = 32;
            // 
            // txtMaSo
            // 
            this.txtMaSo.Location = new System.Drawing.Point(105, 212);
            this.txtMaSo.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtMaSo.Name = "txtMaSo";
            this.txtMaSo.Size = new System.Drawing.Size(101, 20);
            this.txtMaSo.TabIndex = 33;
            this.txtMaSo.Visible = false;
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(8, 366);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(132, 13);
            this.labelControl2.TabIndex = 30;
            this.labelControl2.Text = "Tên sản phẩm trên hoá đơn";
            // 
            // textEdit1
            // 
            this.textEdit1.Location = new System.Drawing.Point(150, 337);
            this.textEdit1.Name = "textEdit1";
            this.textEdit1.Size = new System.Drawing.Size(379, 20);
            this.textEdit1.TabIndex = 29;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(12, 340);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(116, 13);
            this.labelControl1.TabIndex = 27;
            this.labelControl1.Text = "Tên sản phẩm trong kho";
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(363, 57);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(297, 20);
            this.txtSearch.TabIndex = 24;
            this.txtSearch.EditValueChanged += new System.EventHandler(this.txtSearch_EditValueChanged);
            // 
            // gridControl1
            // 
            this.gridControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gridControl1.Location = new System.Drawing.Point(9, 91);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(666, 233);
            this.gridControl1.TabIndex = 17;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            this.gridControl1.Click += new System.EventHandler(this.gridControl1_Click);
            this.gridControl1.DoubleClick += new System.EventHandler(this.gridControl1_DoubleClick_1);
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colTenVattu,
            this.colSoHieu,
            this.colDonvitinh,
            this.gridColumn2,
            this.gridColumn1,
            this.gridColumn3,
            this.colPercent});
            this.gridView1.DetailHeight = 300;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsView.ShowGroupPanel = false;
            this.gridView1.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gridView1_RowClick);
            this.gridView1.MouseEnter += new System.EventHandler(this.gridView1_MouseEnter);
            // 
            // colTenVattu
            // 
            this.colTenVattu.Caption = "Tên vật tư";
            this.colTenVattu.FieldName = "TenVattu";
            this.colTenVattu.MinWidth = 25;
            this.colTenVattu.Name = "colTenVattu";
            this.colTenVattu.OptionsColumn.AllowEdit = false;
            this.colTenVattu.Visible = true;
            this.colTenVattu.VisibleIndex = 2;
            this.colTenVattu.Width = 244;
            // 
            // colSoHieu
            // 
            this.colSoHieu.Caption = "Số hiệu";
            this.colSoHieu.FieldName = "SoHieu";
            this.colSoHieu.MinWidth = 25;
            this.colSoHieu.Name = "colSoHieu";
            this.colSoHieu.OptionsColumn.AllowEdit = false;
            this.colSoHieu.Visible = true;
            this.colSoHieu.VisibleIndex = 1;
            this.colSoHieu.Width = 105;
            // 
            // colDonvitinh
            // 
            this.colDonvitinh.Caption = "ĐVT";
            this.colDonvitinh.FieldName = "DonVi";
            this.colDonvitinh.MinWidth = 25;
            this.colDonvitinh.Name = "colDonvitinh";
            this.colDonvitinh.Visible = true;
            this.colDonvitinh.VisibleIndex = 3;
            this.colDonvitinh.Width = 57;
            // 
            // gridColumn2
            // 
            this.gridColumn2.Caption = "Số lượng";
            this.gridColumn2.DisplayFormat.FormatString = "N2";
            this.gridColumn2.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridColumn2.FieldName = "SoLuong";
            this.gridColumn2.MinWidth = 25;
            this.gridColumn2.Name = "gridColumn2";
            this.gridColumn2.Visible = true;
            this.gridColumn2.VisibleIndex = 4;
            this.gridColumn2.Width = 51;
            // 
            // gridColumn1
            // 
            this.gridColumn1.Caption = "Dongia";
            this.gridColumn1.DisplayFormat.FormatString = "N2";
            this.gridColumn1.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridColumn1.FieldName = "Dongia";
            this.gridColumn1.MinWidth = 25;
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.Visible = true;
            this.gridColumn1.VisibleIndex = 5;
            this.gridColumn1.Width = 87;
            // 
            // gridColumn3
            // 
            this.gridColumn3.Caption = "Thành tiền";
            this.gridColumn3.DisplayFormat.FormatString = "N2";
            this.gridColumn3.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridColumn3.FieldName = "ThanhTien";
            this.gridColumn3.MinWidth = 25;
            this.gridColumn3.Name = "gridColumn3";
            this.gridColumn3.Width = 113;
            // 
            // colPercent
            // 
            this.colPercent.Caption = "%";
            this.colPercent.FieldName = "Percent";
            this.colPercent.MinWidth = 27;
            this.colPercent.Name = "colPercent";
            this.colPercent.OptionsColumn.AllowEdit = false;
            this.colPercent.Visible = true;
            this.colPercent.VisibleIndex = 0;
            this.colPercent.Width = 79;
            // 
            // panelControl1
            // 
            this.panelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelControl1.Controls.Add(this.simpleButton2);
            this.panelControl1.Controls.Add(this.label4);
            this.panelControl1.Controls.Add(this.txtGhichu);
            this.panelControl1.Controls.Add(this.label5);
            this.panelControl1.Controls.Add(this.txtDonvi);
            this.panelControl1.Controls.Add(this.btnThem);
            this.panelControl1.Controls.Add(this.btnSearch);
            this.panelControl1.Controls.Add(this.label3);
            this.panelControl1.Controls.Add(this.txtTenvattu);
            this.panelControl1.Controls.Add(this.btnGhi);
            this.panelControl1.Controls.Add(this.comboBoxEdit1);
            this.panelControl1.Controls.Add(this.btnXoa);
            this.panelControl1.Controls.Add(this.label2);
            this.panelControl1.Controls.Add(this.txtSohieu);
            this.panelControl1.Controls.Add(this.label1);
            this.panelControl1.Controls.Add(this.txtSearch);
            this.panelControl1.Location = new System.Drawing.Point(8, 2);
            this.panelControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(667, 83);
            this.panelControl1.TabIndex = 18;
            this.panelControl1.MouseEnter += new System.EventHandler(this.panelControl1_MouseEnter);
            // 
            // simpleButton2
            // 
            this.simpleButton2.ImageOptions.Image = global::SaovietTax.Properties.Resources.lookup_reference_16x16;
            this.simpleButton2.Location = new System.Drawing.Point(733, 56);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(41, 25);
            this.simpleButton2.TabIndex = 25;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(366, 6);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(54, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Danh mục";
            // 
            // txtDonvi
            // 
            this.txtDonvi.Location = new System.Drawing.Point(267, 6);
            this.txtDonvi.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtDonvi.Name = "txtDonvi";
            this.txtDonvi.Size = new System.Drawing.Size(88, 20);
            this.txtDonvi.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(226, 8);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Đơn vị";
            // 
            // txtTenvattu
            // 
            this.txtTenvattu.Location = new System.Drawing.Point(45, 30);
            this.txtTenvattu.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtTenvattu.Name = "txtTenvattu";
            this.txtTenvattu.Size = new System.Drawing.Size(310, 20);
            this.txtTenvattu.TabIndex = 3;
            // 
            // comboBoxEdit1
            // 
            this.comboBoxEdit1.Location = new System.Drawing.Point(424, 2);
            this.comboBoxEdit1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.comboBoxEdit1.Name = "comboBoxEdit1";
            this.comboBoxEdit1.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.comboBoxEdit1.Size = new System.Drawing.Size(235, 20);
            this.comboBoxEdit1.TabIndex = 4;
            this.comboBoxEdit1.SelectedIndexChanged += new System.EventHandler(this.comboBoxEdit1_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 34);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(25, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Tên";
            // 
            // txtSohieu
            // 
            this.txtSohieu.Location = new System.Drawing.Point(45, 5);
            this.txtSohieu.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtSohieu.Name = "txtSohieu";
            this.txtSohieu.Size = new System.Drawing.Size(178, 20);
            this.txtSohieu.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Mã số";
            // 
            // frmHangHoa
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(681, 384);
            this.Controls.Add(this.txtdongia);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtsoluong);
            this.Controls.Add(this.btnThoat);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.textEdit2);
            this.Controls.Add(this.txtMaSo);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.textEdit1);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.gridControl1);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.gridControl2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "frmHangHoa";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Quản lý danh sách vật tư";
            this.Activated += new System.EventHandler(this.frmHangHoa_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmHangHoa_FormClosing);
            this.Load += new System.EventHandler(this.frmHangHoa_Load_1);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmHangHoa_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.frmHangHoa_KeyPress);
            this.MouseEnter += new System.EventHandler(this.frmHangHoa_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.frmHangHoa_MouseLeave);
            this.MouseHover += new System.EventHandler(this.frmHangHoa_MouseHover);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtdongia.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtGhichu.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtsoluong.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMaSo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtDonvi.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTenvattu.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSohieu.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DevExpress.XtraGrid.GridControl gridControl2;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView2;
        private DevExpress.XtraGrid.Columns.GridColumn colSoLuong;
        private DevExpress.XtraGrid.Columns.GridColumn colDongia;
        private DevExpress.XtraGrid.Columns.GridColumn colThanhTien;
        private DevExpress.XtraEditors.TextEdit txtdongia;
        private DevExpress.XtraEditors.TextEdit txtGhichu;
        private DevExpress.XtraEditors.SimpleButton btnThem;
        private System.Windows.Forms.Label label4;
        private DevExpress.XtraEditors.SimpleButton btnGhi;
        private DevExpress.XtraEditors.SimpleButton btnXoa;
        private System.Windows.Forms.Label label7;
        private DevExpress.XtraEditors.SimpleButton btnSearch;
        private DevExpress.XtraEditors.TextEdit txtsoluong;
        private DevExpress.XtraEditors.SimpleButton btnThoat;
        private System.Windows.Forms.Label label6;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.TextEdit textEdit2;
        private DevExpress.XtraEditors.TextEdit txtMaSo;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.TextEdit textEdit1;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.TextEdit txtSearch;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraGrid.Columns.GridColumn colTenVattu;
        private DevExpress.XtraGrid.Columns.GridColumn colSoHieu;
        private DevExpress.XtraGrid.Columns.GridColumn colDonvitinh;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn3;
        private DevExpress.XtraGrid.Columns.GridColumn colPercent;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private System.Windows.Forms.Label label5;
        private DevExpress.XtraEditors.TextEdit txtDonvi;
        private System.Windows.Forms.Label label3;
        private DevExpress.XtraEditors.TextEdit txtTenvattu;
        private DevExpress.XtraEditors.ComboBoxEdit comboBoxEdit1;
        private System.Windows.Forms.Label label2;
        private DevExpress.XtraEditors.TextEdit txtSohieu;
        private System.Windows.Forms.Label label1;
        private DevExpress.XtraEditors.SimpleButton simpleButton2;
    }
}