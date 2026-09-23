namespace SaovietTax
{
    partial class KhachHangControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KhachHangControl));
            this.slKhachhang = new DevExpress.XtraEditors.ComboBoxEdit();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colMaso = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTenVattu = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colSoHieu = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colMST = new DevExpress.XtraGrid.Columns.GridColumn();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.pictureEdit1 = new DevExpress.XtraEditors.PictureEdit();
            this.btnThoat = new DevExpress.XtraEditors.SimpleButton();
            this.btnXoa = new DevExpress.XtraEditors.SimpleButton();
            this.btnGhi = new DevExpress.XtraEditors.SimpleButton();
            this.btnThem = new DevExpress.XtraEditors.SimpleButton();
            this.txtGhichu = new DevExpress.XtraEditors.TextEdit();
            this.label4 = new System.Windows.Forms.Label();
            this.txtDonvi = new DevExpress.XtraEditors.TextEdit();
            this.label3 = new System.Windows.Forms.Label();
            this.txtTenvattu = new DevExpress.XtraEditors.TextEdit();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSohieu = new DevExpress.XtraEditors.TextEdit();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSearch = new DevExpress.XtraEditors.SimpleButton();
            this.txtSearch = new DevExpress.XtraEditors.TextEdit();
            this.txtMaSo = new DevExpress.XtraEditors.TextEdit();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.slKhachhang.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureEdit1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtGhichu.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDonvi.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTenvattu.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSohieu.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMaSo.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // slKhachhang
            // 
            this.slKhachhang.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.slKhachhang.Location = new System.Drawing.Point(10, 54);
            this.slKhachhang.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.slKhachhang.Name = "slKhachhang";
            this.slKhachhang.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.slKhachhang.Size = new System.Drawing.Size(719, 23);
            this.slKhachhang.TabIndex = 8;
            // 
            // gridControl1
            // 
            this.gridControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gridControl1.Location = new System.Drawing.Point(10, 88);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(719, 546);
            this.gridControl1.TabIndex = 9;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            this.gridControl1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.gridControl1_MouseClick);
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colMaso,
            this.colTenVattu,
            this.colSoHieu,
            this.colMST});
            this.gridView1.DetailHeight = 295;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsFind.SearchInPreview = true;
            this.gridView1.OptionsView.ShowGroupPanel = false;
            this.gridView1.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gridView1_RowClick);
            this.gridView1.CustomDrawCell += new DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventHandler(this.gridView1_CustomDrawCell);
            this.gridView1.DoubleClick += new System.EventHandler(this.gridView1_DoubleClick);
            // 
            // colMaso
            // 
            this.colMaso.Caption = "Mã số";
            this.colMaso.FieldName = "MaSo";
            this.colMaso.MinWidth = 23;
            this.colMaso.Name = "colMaso";
            this.colMaso.OptionsColumn.AllowEdit = false;
            this.colMaso.Width = 61;
            // 
            // colTenVattu
            // 
            this.colTenVattu.Caption = "Tên khách hàng";
            this.colTenVattu.FieldName = "Ten";
            this.colTenVattu.MinWidth = 23;
            this.colTenVattu.Name = "colTenVattu";
            this.colTenVattu.OptionsColumn.AllowEdit = false;
            this.colTenVattu.Visible = true;
            this.colTenVattu.VisibleIndex = 0;
            this.colTenVattu.Width = 557;
            // 
            // colSoHieu
            // 
            this.colSoHieu.Caption = "Số hiệu";
            this.colSoHieu.FieldName = "SoHieu";
            this.colSoHieu.MinWidth = 23;
            this.colSoHieu.Name = "colSoHieu";
            this.colSoHieu.OptionsColumn.AllowEdit = false;
            this.colSoHieu.Visible = true;
            this.colSoHieu.VisibleIndex = 1;
            this.colSoHieu.Width = 84;
            // 
            // colMST
            // 
            this.colMST.Caption = "MST";
            this.colMST.FieldName = "MST";
            this.colMST.MinWidth = 23;
            this.colMST.Name = "colMST";
            this.colMST.Width = 128;
            // 
            // panelControl1
            // 
            this.panelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelControl1.Controls.Add(this.pictureEdit1);
            this.panelControl1.Controls.Add(this.btnThoat);
            this.panelControl1.Controls.Add(this.btnXoa);
            this.panelControl1.Controls.Add(this.btnGhi);
            this.panelControl1.Controls.Add(this.btnThem);
            this.panelControl1.Controls.Add(this.txtGhichu);
            this.panelControl1.Controls.Add(this.label4);
            this.panelControl1.Controls.Add(this.txtDonvi);
            this.panelControl1.Controls.Add(this.label3);
            this.panelControl1.Controls.Add(this.txtTenvattu);
            this.panelControl1.Controls.Add(this.label2);
            this.panelControl1.Controls.Add(this.txtSohieu);
            this.panelControl1.Controls.Add(this.label1);
            this.panelControl1.Location = new System.Drawing.Point(733, 57);
            this.panelControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(436, 590);
            this.panelControl1.TabIndex = 10;
            // 
            // pictureEdit1
            // 
            this.pictureEdit1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureEdit1.EditValue = ((object)(resources.GetObject("pictureEdit1.EditValue")));
            this.pictureEdit1.Location = new System.Drawing.Point(21, 267);
            this.pictureEdit1.Name = "pictureEdit1";
            this.pictureEdit1.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Auto;
            this.pictureEdit1.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Stretch;
            this.pictureEdit1.Size = new System.Drawing.Size(410, 299);
            this.pictureEdit1.TabIndex = 12;
            // 
            // btnThoat
            // 
            this.btnThoat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnThoat.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnThoat.ImageOptions.Image")));
            this.btnThoat.Location = new System.Drawing.Point(322, 201);
            this.btnThoat.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnThoat.Name = "btnThoat";
            this.btnThoat.Size = new System.Drawing.Size(109, 34);
            this.btnThoat.TabIndex = 11;
            this.btnThoat.Text = "Thoát";
            this.btnThoat.Click += new System.EventHandler(this.btnThoat_Click);
            // 
            // btnXoa
            // 
            this.btnXoa.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnXoa.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnXoa.ImageOptions.Image")));
            this.btnXoa.Location = new System.Drawing.Point(230, 201);
            this.btnXoa.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnXoa.Name = "btnXoa";
            this.btnXoa.Size = new System.Drawing.Size(88, 34);
            this.btnXoa.TabIndex = 10;
            this.btnXoa.Text = "Xóa";
            // 
            // btnGhi
            // 
            this.btnGhi.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGhi.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnGhi.ImageOptions.Image")));
            this.btnGhi.Location = new System.Drawing.Point(129, 201);
            this.btnGhi.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnGhi.Name = "btnGhi";
            this.btnGhi.Size = new System.Drawing.Size(97, 34);
            this.btnGhi.TabIndex = 9;
            this.btnGhi.Text = "&Ghi";
            this.btnGhi.Click += new System.EventHandler(this.btnGhi_Click);
            // 
            // btnThem
            // 
            this.btnThem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnThem.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnThem.ImageOptions.Image")));
            this.btnThem.Location = new System.Drawing.Point(23, 201);
            this.btnThem.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnThem.Name = "btnThem";
            this.btnThem.Size = new System.Drawing.Size(100, 34);
            this.btnThem.TabIndex = 8;
            this.btnThem.Text = "Thêm";
            this.btnThem.Click += new System.EventHandler(this.btnThem_Click);
            // 
            // txtGhichu
            // 
            this.txtGhichu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGhichu.Location = new System.Drawing.Point(89, 162);
            this.txtGhichu.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtGhichu.Name = "txtGhichu";
            this.txtGhichu.Size = new System.Drawing.Size(342, 23);
            this.txtGhichu.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 165);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(46, 16);
            this.label4.TabIndex = 6;
            this.label4.Text = "Địa chỉ";
            // 
            // txtDonvi
            // 
            this.txtDonvi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDonvi.Location = new System.Drawing.Point(89, 115);
            this.txtDonvi.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtDonvi.Name = "txtDonvi";
            this.txtDonvi.Size = new System.Drawing.Size(342, 23);
            this.txtDonvi.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 118);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "MST";
            // 
            // txtTenvattu
            // 
            this.txtTenvattu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTenvattu.Location = new System.Drawing.Point(89, 69);
            this.txtTenvattu.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtTenvattu.Name = "txtTenvattu";
            this.txtTenvattu.Size = new System.Drawing.Size(342, 23);
            this.txtTenvattu.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 71);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Tên";
            // 
            // txtSohieu
            // 
            this.txtSohieu.Location = new System.Drawing.Point(89, 25);
            this.txtSohieu.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtSohieu.Name = "txtSohieu";
            this.txtSohieu.Size = new System.Drawing.Size(342, 23);
            this.txtSohieu.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 31);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Mã số";
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnSearch.ImageOptions.Image")));
            this.btnSearch.Location = new System.Drawing.Point(582, 222);
            this.btnSearch.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(36, 29);
            this.btnSearch.TabIndex = 14;
            this.btnSearch.Visible = false;
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(184, 229);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(207, 23);
            this.txtSearch.TabIndex = 13;
            this.txtSearch.Visible = false;
            // 
            // txtMaSo
            // 
            this.txtMaSo.Location = new System.Drawing.Point(69, 194);
            this.txtMaSo.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtMaSo.Name = "txtMaSo";
            this.txtMaSo.Size = new System.Drawing.Size(409, 23);
            this.txtMaSo.TabIndex = 15;
            this.txtMaSo.Visible = false;
            // 
            // simpleButton1
            // 
            this.simpleButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton1.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("simpleButton1.ImageOptions.SvgImage")));
            this.simpleButton1.Location = new System.Drawing.Point(1125, 9);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(44, 42);
            this.simpleButton1.TabIndex = 11;
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // labelControl1
            // 
            this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.labelControl1.Appearance.Options.UseFont = true;
            this.labelControl1.Location = new System.Drawing.Point(10, 17);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(208, 22);
            this.labelControl1.TabIndex = 16;
            this.labelControl1.Text = "Danh sách khách hàng";
            this.labelControl1.Click += new System.EventHandler(this.labelControl1_Click);
            // 
            // KhachHangControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.txtMaSo);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.gridControl1);
            this.Controls.Add(this.slKhachhang);
            this.Name = "KhachHangControl";
            this.Size = new System.Drawing.Size(1186, 661);
            this.Load += new System.EventHandler(this.KhachHangControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.slKhachhang.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureEdit1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtGhichu.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDonvi.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTenvattu.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSohieu.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMaSo.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.ComboBoxEdit slKhachhang;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraGrid.Columns.GridColumn colMaso;
        private DevExpress.XtraGrid.Columns.GridColumn colTenVattu;
        private DevExpress.XtraGrid.Columns.GridColumn colSoHieu;
        private DevExpress.XtraGrid.Columns.GridColumn colMST;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.TextEdit txtMaSo;
        private DevExpress.XtraEditors.SimpleButton btnThoat;
        private DevExpress.XtraEditors.SimpleButton btnXoa;
        private DevExpress.XtraEditors.SimpleButton btnGhi;
        private DevExpress.XtraEditors.SimpleButton btnThem;
        private DevExpress.XtraEditors.TextEdit txtGhichu;
        private System.Windows.Forms.Label label4;
        private DevExpress.XtraEditors.TextEdit txtDonvi;
        private System.Windows.Forms.Label label3;
        private DevExpress.XtraEditors.TextEdit txtTenvattu;
        private System.Windows.Forms.Label label2;
        private DevExpress.XtraEditors.TextEdit txtSohieu;
        private System.Windows.Forms.Label label1;
        private DevExpress.XtraEditors.SimpleButton btnSearch;
        private DevExpress.XtraEditors.TextEdit txtSearch;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.PictureEdit pictureEdit1;
        private DevExpress.XtraEditors.LabelControl labelControl1;
    }
}
