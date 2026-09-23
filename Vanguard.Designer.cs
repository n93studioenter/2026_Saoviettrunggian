namespace SaovietTax
{
    partial class Vanguard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Vanguard));
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.warningDataBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colThang = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colHoadonthieu = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colImportloi = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colHangam = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colHethongTK = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colHoadonthua = new DevExpress.XtraGrid.Columns.GridColumn();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.svgImageBox1 = new DevExpress.XtraEditors.SvgImageBox();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningDataBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.svgImageBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControl1
            // 
            this.gridControl1.DataSource = this.warningDataBindingSource;
            this.gridControl1.Location = new System.Drawing.Point(977, 381);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(91, 44);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            this.gridControl1.Visible = false;
            // 
            // warningDataBindingSource
            // 
            this.warningDataBindingSource.DataSource = typeof(SaovietTax.Vanguard.WarningData);
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colThang,
            this.colHoadonthieu,
            this.colImportloi,
            this.colHangam,
            this.colHethongTK,
            this.colHoadonthua});
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsView.ShowGroupPanel = false;
            // 
            // colThang
            // 
            this.colThang.Caption = "Tháng";
            this.colThang.FieldName = "Thang";
            this.colThang.MinWidth = 25;
            this.colThang.Name = "colThang";
            this.colThang.Visible = true;
            this.colThang.VisibleIndex = 0;
            this.colThang.Width = 59;
            // 
            // colHoadonthieu
            // 
            this.colHoadonthieu.Caption = "Hoá đơn chưa nhập";
            this.colHoadonthieu.FieldName = "Hoadonthieu";
            this.colHoadonthieu.MinWidth = 25;
            this.colHoadonthieu.Name = "colHoadonthieu";
            this.colHoadonthieu.Visible = true;
            this.colHoadonthieu.VisibleIndex = 1;
            this.colHoadonthieu.Width = 246;
            // 
            // colImportloi
            // 
            this.colImportloi.Caption = "Import lỗi";
            this.colImportloi.FieldName = "Importloi";
            this.colImportloi.MinWidth = 25;
            this.colImportloi.Name = "colImportloi";
            this.colImportloi.Visible = true;
            this.colImportloi.VisibleIndex = 2;
            this.colImportloi.Width = 171;
            // 
            // colHangam
            // 
            this.colHangam.Caption = "Hàng âm";
            this.colHangam.FieldName = "Hangam";
            this.colHangam.MinWidth = 25;
            this.colHangam.Name = "colHangam";
            this.colHangam.Visible = true;
            this.colHangam.VisibleIndex = 3;
            this.colHangam.Width = 120;
            // 
            // colHethongTK
            // 
            this.colHethongTK.Caption = "Hệ thống tài khoản chưa cân";
            this.colHethongTK.FieldName = "HethongTK";
            this.colHethongTK.MinWidth = 25;
            this.colHethongTK.Name = "colHethongTK";
            this.colHethongTK.Visible = true;
            this.colHethongTK.VisibleIndex = 4;
            this.colHethongTK.Width = 212;
            // 
            // colHoadonthua
            // 
            this.colHoadonthua.Caption = "Hoá đơn thừa";
            this.colHoadonthua.FieldName = "HoaDonThua";
            this.colHoadonthua.MinWidth = 25;
            this.colHoadonthua.Name = "colHoadonthua";
            this.colHoadonthua.Visible = true;
            this.colHoadonthua.VisibleIndex = 5;
            this.colHoadonthua.Width = 125;
            // 
            // simpleButton1
            // 
            this.simpleButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton1.ImageOptions.Image = global::SaovietTax.Properties.Resources.delete_32x32;
            this.simpleButton1.Location = new System.Drawing.Point(375, 8);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(41, 38);
            this.simpleButton1.TabIndex = 1;
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // svgImageBox1
            // 
            this.svgImageBox1.Location = new System.Drawing.Point(12, 8);
            this.svgImageBox1.Name = "svgImageBox1";
            this.svgImageBox1.Size = new System.Drawing.Size(60, 47);
            this.svgImageBox1.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("svgImageBox1.SvgImage")));
            this.svgImageBox1.TabIndex = 2;
            this.svgImageBox1.Text = "svgImageBox1";
            // 
            // labelControl1
            // 
            this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl1.Appearance.ForeColor = System.Drawing.Color.Black;
            this.labelControl1.Appearance.Options.UseFont = true;
            this.labelControl1.Appearance.Options.UseForeColor = true;
            this.labelControl1.Location = new System.Drawing.Point(81, 22);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(210, 14);
            this.labelControl1.TabIndex = 3;
            this.labelControl1.Text = "CẢNH BÁO HỆ THỐNG THEO THÁNG";
            // 
            // panelControl1
            // 
            this.panelControl1.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.panelControl1.Appearance.Options.UseBackColor = true;
            this.panelControl1.Location = new System.Drawing.Point(12, 61);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(409, 534);
            this.panelControl1.TabIndex = 4;
            // 
            // Vanguard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(428, 607);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.svgImageBox1);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.gridControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Vanguard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Vanguard";
            this.Load += new System.EventHandler(this.Vanguard_Load);
            this.Shown += new System.EventHandler(this.Vanguard_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningDataBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.svgImageBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private System.Windows.Forms.BindingSource warningDataBindingSource;
        private DevExpress.XtraGrid.Columns.GridColumn colHoadonthieu;
        private DevExpress.XtraGrid.Columns.GridColumn colImportloi;
        private DevExpress.XtraGrid.Columns.GridColumn colHangam;
        private DevExpress.XtraGrid.Columns.GridColumn colHethongTK;
        private DevExpress.XtraGrid.Columns.GridColumn colThang;
        private DevExpress.XtraGrid.Columns.GridColumn colHoadonthua;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.SvgImageBox svgImageBox1;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.PanelControl panelControl1;
    }
}