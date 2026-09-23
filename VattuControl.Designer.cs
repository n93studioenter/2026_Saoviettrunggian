namespace SaovietTax
{
    partial class VattuControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VattuControl));
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colMaso = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTenVattu = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colSoHieu = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.slKhachhang = new DevExpress.XtraEditors.ComboBoxEdit();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.txtSearch = new DevExpress.XtraEditors.TextEdit();
            this.txtMaSo = new DevExpress.XtraEditors.TextEdit();
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
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.panelControl2 = new DevExpress.XtraEditors.PanelControl();
            this.btnSearch = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.slKhachhang.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMaSo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtGhichu.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDonvi.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTenvattu.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSohieu.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).BeginInit();
            this.panelControl2.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelControl1
            // 
            this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.labelControl1.Appearance.Options.UseFont = true;
            this.labelControl1.Location = new System.Drawing.Point(12, 8);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(186, 22);
            this.labelControl1.TabIndex = 19;
            this.labelControl1.Text = "Danh sách hàng hoá";
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gridControl1.Location = new System.Drawing.Point(3, 3);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(877, 528);
            this.gridControl1.TabIndex = 16;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colMaso,
            this.colTenVattu,
            this.colSoHieu,
            this.gridColumn1,
            this.gridColumn2});
            this.gridView1.DetailHeight = 295;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsBehavior.Editable = false;
            this.gridView1.OptionsFind.SearchInPreview = true;
            this.gridView1.OptionsSelection.MultiSelect = true;
            this.gridView1.OptionsView.RowAutoHeight = true;
            this.gridView1.OptionsView.ShowGroupPanel = false;
            this.gridView1.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gridView1_RowClick);
            this.gridView1.CustomDrawCell += new DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventHandler(this.gridView1_CustomDrawCell);
            this.gridView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gridView1_KeyDown);
            this.gridView1.DoubleClick += new System.EventHandler(this.gridView1_DoubleClick);
            // 
            // colMaso
            // 
            this.colMaso.Caption = "Mã số";
            this.colMaso.FieldName = "MaSo";
            this.colMaso.Name = "colMaso";
            this.colMaso.OptionsColumn.AllowEdit = false;
            this.colMaso.Width = 53;
            // 
            // colTenVattu
            // 
            this.colTenVattu.Caption = "Tên vật tư";
            this.colTenVattu.FieldName = "TenVattu";
            this.colTenVattu.Name = "colTenVattu";
            this.colTenVattu.OptionsColumn.AllowEdit = false;
            this.colTenVattu.Visible = true;
            this.colTenVattu.VisibleIndex = 1;
            this.colTenVattu.Width = 323;
            // 
            // colSoHieu
            // 
            this.colSoHieu.Caption = "Số hiệu";
            this.colSoHieu.FieldName = "SoHieu";
            this.colSoHieu.Name = "colSoHieu";
            this.colSoHieu.OptionsColumn.AllowEdit = false;
            this.colSoHieu.Visible = true;
            this.colSoHieu.VisibleIndex = 0;
            this.colSoHieu.Width = 86;
            // 
            // gridColumn1
            // 
            this.gridColumn1.Caption = "Đơn giá";
            this.gridColumn1.DisplayFormat.FormatString = "N0";
            this.gridColumn1.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridColumn1.FieldName = "Dongia";
            this.gridColumn1.MinWidth = 22;
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.Visible = true;
            this.gridColumn1.VisibleIndex = 2;
            this.gridColumn1.Width = 82;
            // 
            // gridColumn2
            // 
            this.gridColumn2.Caption = "Số lượng";
            this.gridColumn2.DisplayFormat.FormatString = "N0";
            this.gridColumn2.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridColumn2.FieldName = "SoLuong";
            this.gridColumn2.MinWidth = 22;
            this.gridColumn2.Name = "gridColumn2";
            this.gridColumn2.Visible = true;
            this.gridColumn2.VisibleIndex = 3;
            this.gridColumn2.Width = 82;
            // 
            // slKhachhang
            // 
            this.slKhachhang.Location = new System.Drawing.Point(12, 45);
            this.slKhachhang.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.slKhachhang.Name = "slKhachhang";
            this.slKhachhang.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.slKhachhang.Size = new System.Drawing.Size(880, 23);
            this.slKhachhang.TabIndex = 17;
            // 
            // panelControl1
            // 
            this.panelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelControl1.Controls.Add(this.txtMaSo);
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
            this.panelControl1.Location = new System.Drawing.Point(900, 48);
            this.panelControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(411, 589);
            this.panelControl1.TabIndex = 20;
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(12, 74);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(832, 23);
            this.txtSearch.TabIndex = 22;
            // 
            // txtMaSo
            // 
            this.txtMaSo.Location = new System.Drawing.Point(121, 560);
            this.txtMaSo.Name = "txtMaSo";
            this.txtMaSo.Size = new System.Drawing.Size(109, 23);
            this.txtMaSo.TabIndex = 13;
            this.txtMaSo.Visible = false;
            // 
            // btnThoat
            // 
            this.btnThoat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnThoat.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnThoat.ImageOptions.Image")));
            this.btnThoat.Location = new System.Drawing.Point(316, 201);
            this.btnThoat.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnThoat.Name = "btnThoat";
            this.btnThoat.Size = new System.Drawing.Size(90, 33);
            this.btnThoat.TabIndex = 11;
            this.btnThoat.Text = "Thoát";
            // 
            // btnXoa
            // 
            this.btnXoa.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnXoa.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnXoa.ImageOptions.Image")));
            this.btnXoa.Location = new System.Drawing.Point(220, 201);
            this.btnXoa.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnXoa.Name = "btnXoa";
            this.btnXoa.Size = new System.Drawing.Size(92, 33);
            this.btnXoa.TabIndex = 10;
            this.btnXoa.Text = "Xóa";
            // 
            // btnGhi
            // 
            this.btnGhi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGhi.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnGhi.ImageOptions.Image")));
            this.btnGhi.Location = new System.Drawing.Point(107, 201);
            this.btnGhi.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnGhi.Name = "btnGhi";
            this.btnGhi.Size = new System.Drawing.Size(109, 33);
            this.btnGhi.TabIndex = 9;
            this.btnGhi.Text = "&Ghi";
            this.btnGhi.Click += new System.EventHandler(this.btnGhi_Click);
            // 
            // btnThem
            // 
            this.btnThem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnThem.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnThem.ImageOptions.Image")));
            this.btnThem.Location = new System.Drawing.Point(13, 201);
            this.btnThem.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnThem.Name = "btnThem";
            this.btnThem.Size = new System.Drawing.Size(90, 33);
            this.btnThem.TabIndex = 8;
            this.btnThem.Text = "Thêm";
            this.btnThem.Click += new System.EventHandler(this.btnThem_Click);
            // 
            // txtGhichu
            // 
            this.txtGhichu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGhichu.Location = new System.Drawing.Point(119, 162);
            this.txtGhichu.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtGhichu.Name = "txtGhichu";
            this.txtGhichu.Size = new System.Drawing.Size(287, 23);
            this.txtGhichu.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 165);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 16);
            this.label4.TabIndex = 6;
            this.label4.Text = "Nội dung";
            // 
            // txtDonvi
            // 
            this.txtDonvi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDonvi.Location = new System.Drawing.Point(119, 115);
            this.txtDonvi.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtDonvi.Name = "txtDonvi";
            this.txtDonvi.Size = new System.Drawing.Size(287, 23);
            this.txtDonvi.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 118);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "ĐVT";
            // 
            // txtTenvattu
            // 
            this.txtTenvattu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTenvattu.Location = new System.Drawing.Point(119, 69);
            this.txtTenvattu.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtTenvattu.Name = "txtTenvattu";
            this.txtTenvattu.Size = new System.Drawing.Size(287, 23);
            this.txtTenvattu.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 71);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Tên";
            // 
            // txtSohieu
            // 
            this.txtSohieu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSohieu.Location = new System.Drawing.Point(119, 24);
            this.txtSohieu.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtSohieu.Name = "txtSohieu";
            this.txtSohieu.Size = new System.Drawing.Size(287, 23);
            this.txtSohieu.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 31);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Mã số";
            // 
            // simpleButton1
            // 
            this.simpleButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton1.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("simpleButton1.ImageOptions.SvgImage")));
            this.simpleButton1.Location = new System.Drawing.Point(1271, 3);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(45, 42);
            this.simpleButton1.TabIndex = 21;
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // panelControl2
            // 
            this.panelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panelControl2.Controls.Add(this.gridControl1);
            this.panelControl2.Location = new System.Drawing.Point(12, 103);
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(883, 534);
            this.panelControl2.TabIndex = 22;
            // 
            // btnSearch
            // 
            this.btnSearch.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.btnSearch.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("simpleButton2.ImageOptions.SvgImage")));
            this.btnSearch.Location = new System.Drawing.Point(863, 72);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(29, 29);
            this.btnSearch.TabIndex = 23;
            // 
            // VattuControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.panelControl2);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.slKhachhang);
            this.Name = "VattuControl";
            this.Size = new System.Drawing.Size(1321, 653);
            this.Load += new System.EventHandler(this.VattuControl_Load);
            this.Enter += new System.EventHandler(this.VattuControl_Enter);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.slKhachhang.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMaSo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtGhichu.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDonvi.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTenvattu.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSohieu.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).EndInit();
            this.panelControl2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraGrid.Columns.GridColumn colMaso;
        private DevExpress.XtraGrid.Columns.GridColumn colTenVattu;
        private DevExpress.XtraGrid.Columns.GridColumn colSoHieu;
        private DevExpress.XtraEditors.ComboBoxEdit slKhachhang;
        private DevExpress.XtraEditors.PanelControl panelControl1;
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
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.TextEdit txtSearch;
        private DevExpress.XtraEditors.TextEdit txtMaSo;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
        private DevExpress.XtraEditors.PanelControl panelControl2;
        private DevExpress.XtraEditors.SimpleButton btnSearch;
    }
}
