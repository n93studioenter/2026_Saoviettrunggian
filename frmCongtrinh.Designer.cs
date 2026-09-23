namespace SaovietTax
{
    partial class frmCongtrinh
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCongtrinh));
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colSoHieu = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTenVattu = new DevExpress.XtraGrid.Columns.GridColumn();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.label5 = new System.Windows.Forms.Label();
            this.txtTaikhoan = new DevExpress.XtraEditors.TextEdit();
            this.txtId = new DevExpress.XtraEditors.TextEdit();
            this.simpleButton4 = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton3 = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton2 = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.txtGhichu = new DevExpress.XtraEditors.TextEdit();
            this.label4 = new System.Windows.Forms.Label();
            this.txtDonvi = new DevExpress.XtraEditors.TextEdit();
            this.label3 = new System.Windows.Forms.Label();
            this.txtTenvattu = new DevExpress.XtraEditors.TextEdit();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSohieu = new DevExpress.XtraEditors.TextEdit();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxEdit1 = new DevExpress.XtraEditors.ComboBoxEdit();
            this.txtSearch = new DevExpress.XtraEditors.TextEdit();
            this.btnSearch = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtTaikhoan.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtId.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtGhichu.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDonvi.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTenvattu.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSohieu.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControl1
            // 
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gridControl1.Location = new System.Drawing.Point(9, 72);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(408, 446);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            this.gridControl1.DoubleClick += new System.EventHandler(this.gridControl1_DoubleClick);
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colSoHieu,
            this.colTenVattu});
            this.gridView1.DetailHeight = 295;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsView.GroupDrawMode = DevExpress.XtraGrid.Views.Grid.GroupDrawMode.Office2003;
            this.gridView1.OptionsView.ShowGroupPanel = false;
            this.gridView1.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gridView1_RowClick);
            // 
            // colSoHieu
            // 
            this.colSoHieu.Caption = "Số hiệu";
            this.colSoHieu.FieldName = "SoHieu";
            this.colSoHieu.MinWidth = 23;
            this.colSoHieu.Name = "colSoHieu";
            this.colSoHieu.OptionsColumn.AllowEdit = false;
            this.colSoHieu.Visible = true;
            this.colSoHieu.VisibleIndex = 0;
            this.colSoHieu.Width = 70;
            // 
            // colTenVattu
            // 
            this.colTenVattu.Caption = "Tên vật tư";
            this.colTenVattu.FieldName = "TenVattu";
            this.colTenVattu.MinWidth = 23;
            this.colTenVattu.Name = "colTenVattu";
            this.colTenVattu.OptionsColumn.AllowEdit = false;
            this.colTenVattu.Visible = true;
            this.colTenVattu.VisibleIndex = 1;
            this.colTenVattu.Width = 320;
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.label5);
            this.panelControl1.Controls.Add(this.txtTaikhoan);
            this.panelControl1.Controls.Add(this.txtId);
            this.panelControl1.Controls.Add(this.simpleButton4);
            this.panelControl1.Controls.Add(this.simpleButton3);
            this.panelControl1.Controls.Add(this.simpleButton2);
            this.panelControl1.Controls.Add(this.simpleButton1);
            this.panelControl1.Controls.Add(this.txtGhichu);
            this.panelControl1.Controls.Add(this.label4);
            this.panelControl1.Controls.Add(this.txtDonvi);
            this.panelControl1.Controls.Add(this.label3);
            this.panelControl1.Controls.Add(this.txtTenvattu);
            this.panelControl1.Controls.Add(this.label2);
            this.panelControl1.Controls.Add(this.txtSohieu);
            this.panelControl1.Controls.Add(this.label1);
            this.panelControl1.Location = new System.Drawing.Point(422, 11);
            this.panelControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(464, 507);
            this.panelControl1.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 198);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 16);
            this.label5.TabIndex = 14;
            this.label5.Text = "Tài khoản";
            // 
            // txtTaikhoan
            // 
            this.txtTaikhoan.Location = new System.Drawing.Point(88, 195);
            this.txtTaikhoan.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtTaikhoan.Name = "txtTaikhoan";
            this.txtTaikhoan.Size = new System.Drawing.Size(362, 23);
            this.txtTaikhoan.TabIndex = 13;
            // 
            // txtId
            // 
            this.txtId.Location = new System.Drawing.Point(23, 276);
            this.txtId.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtId.Name = "txtId";
            this.txtId.Size = new System.Drawing.Size(427, 23);
            this.txtId.TabIndex = 12;
            this.txtId.Visible = false;
            // 
            // simpleButton4
            // 
            this.simpleButton4.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("simpleButton4.ImageOptions.Image")));
            this.simpleButton4.Location = new System.Drawing.Point(348, 233);
            this.simpleButton4.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.simpleButton4.Name = "simpleButton4";
            this.simpleButton4.Size = new System.Drawing.Size(102, 29);
            this.simpleButton4.TabIndex = 11;
            this.simpleButton4.Text = "Thoát";
            this.simpleButton4.Click += new System.EventHandler(this.simpleButton4_Click);
            // 
            // simpleButton3
            // 
            this.simpleButton3.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("simpleButton3.ImageOptions.Image")));
            this.simpleButton3.Location = new System.Drawing.Point(237, 233);
            this.simpleButton3.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.simpleButton3.Name = "simpleButton3";
            this.simpleButton3.Size = new System.Drawing.Size(95, 29);
            this.simpleButton3.TabIndex = 10;
            this.simpleButton3.Text = "Xóa";
            this.simpleButton3.Click += new System.EventHandler(this.simpleButton3_Click);
            // 
            // simpleButton2
            // 
            this.simpleButton2.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("simpleButton2.ImageOptions.Image")));
            this.simpleButton2.Location = new System.Drawing.Point(133, 233);
            this.simpleButton2.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(93, 29);
            this.simpleButton2.TabIndex = 9;
            this.simpleButton2.Text = "Ghi";
            this.simpleButton2.Click += new System.EventHandler(this.simpleButton2_Click);
            // 
            // simpleButton1
            // 
            this.simpleButton1.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("simpleButton1.ImageOptions.Image")));
            this.simpleButton1.Location = new System.Drawing.Point(23, 233);
            this.simpleButton1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(94, 29);
            this.simpleButton1.TabIndex = 8;
            this.simpleButton1.Text = "Thêm";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // txtGhichu
            // 
            this.txtGhichu.Location = new System.Drawing.Point(88, 153);
            this.txtGhichu.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtGhichu.Name = "txtGhichu";
            this.txtGhichu.Size = new System.Drawing.Size(362, 23);
            this.txtGhichu.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 156);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 16);
            this.label4.TabIndex = 6;
            this.label4.Text = "Ghi chú";
            // 
            // txtDonvi
            // 
            this.txtDonvi.Location = new System.Drawing.Point(88, 106);
            this.txtDonvi.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtDonvi.Name = "txtDonvi";
            this.txtDonvi.Size = new System.Drawing.Size(362, 23);
            this.txtDonvi.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 109);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "Đơn vị";
            // 
            // txtTenvattu
            // 
            this.txtTenvattu.Location = new System.Drawing.Point(88, 60);
            this.txtTenvattu.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtTenvattu.Name = "txtTenvattu";
            this.txtTenvattu.Size = new System.Drawing.Size(362, 23);
            this.txtTenvattu.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 62);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Tên vật tư";
            // 
            // txtSohieu
            // 
            this.txtSohieu.Location = new System.Drawing.Point(88, 15);
            this.txtSohieu.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtSohieu.Name = "txtSohieu";
            this.txtSohieu.Size = new System.Drawing.Size(362, 23);
            this.txtSohieu.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 22);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Số hiệu";
            // 
            // comboBoxEdit1
            // 
            this.comboBoxEdit1.Location = new System.Drawing.Point(9, 10);
            this.comboBoxEdit1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.comboBoxEdit1.Name = "comboBoxEdit1";
            this.comboBoxEdit1.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.comboBoxEdit1.Size = new System.Drawing.Size(408, 23);
            this.comboBoxEdit1.TabIndex = 8;
            this.comboBoxEdit1.SelectedIndexChanged += new System.EventHandler(this.comboBoxEdit1_SelectedIndexChanged);
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(9, 41);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(367, 23);
            this.txtSearch.TabIndex = 9;
            this.txtSearch.EditValueChanged += new System.EventHandler(this.txtSearch_EditValueChanged);
            // 
            // btnSearch
            // 
            this.btnSearch.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnSearch.ImageOptions.Image")));
            this.btnSearch.Location = new System.Drawing.Point(382, 39);
            this.btnSearch.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(36, 29);
            this.btnSearch.TabIndex = 10;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // frmCongtrinh
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(893, 529);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.comboBoxEdit1);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.gridControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "frmCongtrinh";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Quản lý công trình";
            this.Load += new System.EventHandler(this.frmCongtrinh_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtTaikhoan.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtId.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtGhichu.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDonvi.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTenvattu.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSohieu.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraGrid.Columns.GridColumn colSoHieu;
        private DevExpress.XtraGrid.Columns.GridColumn colTenVattu;
        private DevExpress.XtraEditors.TextEdit txtTenvattu;
        private System.Windows.Forms.Label label2;
        private DevExpress.XtraEditors.TextEdit txtSohieu;
        private System.Windows.Forms.Label label1;
        private DevExpress.XtraEditors.TextEdit txtDonvi;
        private System.Windows.Forms.Label label3;
        private DevExpress.XtraEditors.TextEdit txtGhichu;
        private System.Windows.Forms.Label label4;
        private DevExpress.XtraEditors.SimpleButton simpleButton4;
        private DevExpress.XtraEditors.SimpleButton simpleButton3;
        private DevExpress.XtraEditors.SimpleButton simpleButton2;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.TextEdit txtId;
        private System.Windows.Forms.Label label5;
        private DevExpress.XtraEditors.TextEdit txtTaikhoan;
        private DevExpress.XtraEditors.ComboBoxEdit comboBoxEdit1;
        private DevExpress.XtraEditors.TextEdit txtSearch;
        private DevExpress.XtraEditors.SimpleButton btnSearch;
    }
}