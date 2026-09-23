namespace SaovietTax
{
    partial class frmTaikhoan
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTaikhoan));
            this.treeList1 = new DevExpress.XtraTreeList.TreeList();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.txtKyHieu = new DevExpress.XtraEditors.TextEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.txtID = new DevExpress.XtraEditors.TextEdit();
            this.btnThoat = new DevExpress.XtraEditors.SimpleButton();
            this.btnXoa = new DevExpress.XtraEditors.SimpleButton();
            this.btnGhi = new DevExpress.XtraEditors.SimpleButton();
            this.btnThem = new DevExpress.XtraEditors.SimpleButton();
            this.textEdit2 = new DevExpress.XtraEditors.TextEdit();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.textEdit1 = new DevExpress.XtraEditors.TextEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.treeList1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtKyHieu.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtID.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // treeList1
            // 
            this.treeList1.HorzScrollStep = 2;
            this.treeList1.Location = new System.Drawing.Point(25, 19);
            this.treeList1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.treeList1.MinWidth = 16;
            this.treeList1.Name = "treeList1";
            this.treeList1.OptionsBehavior.Editable = false;
            this.treeList1.Size = new System.Drawing.Size(431, 462);
            this.treeList1.TabIndex = 1;
            this.treeList1.TreeLevelWidth = 14;
            this.treeList1.AfterExpand += new DevExpress.XtraTreeList.NodeEventHandler(this.treeList1_AfterExpand);
            this.treeList1.FocusedNodeChanged += new DevExpress.XtraTreeList.FocusedNodeChangedEventHandler(this.treeList1_FocusedNodeChanged);
            this.treeList1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.treeList1_MouseDoubleClick);
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.txtKyHieu);
            this.panelControl1.Controls.Add(this.labelControl3);
            this.panelControl1.Controls.Add(this.txtID);
            this.panelControl1.Controls.Add(this.btnThoat);
            this.panelControl1.Controls.Add(this.btnXoa);
            this.panelControl1.Controls.Add(this.btnGhi);
            this.panelControl1.Controls.Add(this.btnThem);
            this.panelControl1.Controls.Add(this.textEdit2);
            this.panelControl1.Controls.Add(this.labelControl2);
            this.panelControl1.Controls.Add(this.textEdit1);
            this.panelControl1.Controls.Add(this.labelControl1);
            this.panelControl1.Location = new System.Drawing.Point(460, 19);
            this.panelControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(420, 462);
            this.panelControl1.TabIndex = 2;
            // 
            // txtKyHieu
            // 
            this.txtKyHieu.Location = new System.Drawing.Point(78, 319);
            this.txtKyHieu.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtKyHieu.Name = "txtKyHieu";
            this.txtKyHieu.Size = new System.Drawing.Size(320, 23);
            this.txtKyHieu.TabIndex = 18;
            this.txtKyHieu.Visible = false;
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(19, 321);
            this.labelControl3.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(41, 16);
            this.labelControl3.TabIndex = 17;
            this.labelControl3.Text = "Ký hiệu";
            this.labelControl3.Visible = false;
            // 
            // txtID
            // 
            this.txtID.Location = new System.Drawing.Point(15, 146);
            this.txtID.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtID.Name = "txtID";
            this.txtID.Size = new System.Drawing.Size(383, 23);
            this.txtID.TabIndex = 16;
            // 
            // btnThoat
            // 
            this.btnThoat.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnThoat.ImageOptions.Image")));
            this.btnThoat.Location = new System.Drawing.Point(322, 93);
            this.btnThoat.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnThoat.Name = "btnThoat";
            this.btnThoat.Size = new System.Drawing.Size(76, 29);
            this.btnThoat.TabIndex = 15;
            this.btnThoat.Text = "Thoát";
            this.btnThoat.Click += new System.EventHandler(this.btnThoat_Click);
            // 
            // btnXoa
            // 
            this.btnXoa.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnXoa.ImageOptions.Image")));
            this.btnXoa.Location = new System.Drawing.Point(228, 93);
            this.btnXoa.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnXoa.Name = "btnXoa";
            this.btnXoa.Size = new System.Drawing.Size(89, 29);
            this.btnXoa.TabIndex = 14;
            this.btnXoa.Text = "Xóa";
            this.btnXoa.Click += new System.EventHandler(this.btnXoa_Click);
            // 
            // btnGhi
            // 
            this.btnGhi.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnGhi.ImageOptions.Image")));
            this.btnGhi.Location = new System.Drawing.Point(111, 93);
            this.btnGhi.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnGhi.Name = "btnGhi";
            this.btnGhi.Size = new System.Drawing.Size(103, 29);
            this.btnGhi.TabIndex = 13;
            this.btnGhi.Text = "&Ghi";
            this.btnGhi.Click += new System.EventHandler(this.btnGhi_Click);
            // 
            // btnThem
            // 
            this.btnThem.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnThem.ImageOptions.Image")));
            this.btnThem.Location = new System.Drawing.Point(15, 93);
            this.btnThem.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnThem.Name = "btnThem";
            this.btnThem.Size = new System.Drawing.Size(92, 29);
            this.btnThem.TabIndex = 12;
            this.btnThem.Text = "Thêm";
            this.btnThem.Click += new System.EventHandler(this.btnThem_Click);
            // 
            // textEdit2
            // 
            this.textEdit2.Enabled = false;
            this.textEdit2.Location = new System.Drawing.Point(78, 55);
            this.textEdit2.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textEdit2.Name = "textEdit2";
            this.textEdit2.Size = new System.Drawing.Size(320, 23);
            this.textEdit2.TabIndex = 3;
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(19, 57);
            this.labelControl2.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(22, 16);
            this.labelControl2.TabIndex = 2;
            this.labelControl2.Text = "Tên";
            // 
            // textEdit1
            // 
            this.textEdit1.Enabled = false;
            this.textEdit1.Location = new System.Drawing.Point(78, 18);
            this.textEdit1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textEdit1.Name = "textEdit1";
            this.textEdit1.Size = new System.Drawing.Size(320, 23);
            this.textEdit1.TabIndex = 1;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(19, 20);
            this.labelControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(43, 16);
            this.labelControl1.TabIndex = 0;
            this.labelControl1.Text = "Số hiệu";
            // 
            // frmTaikhoan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(891, 488);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.treeList1);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "frmTaikhoan";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hệ thống tài khoản";
            this.Load += new System.EventHandler(this.frmTaikhoan_Load);
            ((System.ComponentModel.ISupportInitialize)(this.treeList1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtKyHieu.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtID.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraTreeList.TreeList treeList1;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.TextEdit textEdit2;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.TextEdit textEdit1;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.SimpleButton btnThoat;
        private DevExpress.XtraEditors.SimpleButton btnXoa;
        private DevExpress.XtraEditors.SimpleButton btnGhi;
        private DevExpress.XtraEditors.SimpleButton btnThem;
        private DevExpress.XtraEditors.TextEdit txtID;
        private DevExpress.XtraEditors.TextEdit txtKyHieu;
        private DevExpress.XtraEditors.LabelControl labelControl3;
    }
}