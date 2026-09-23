namespace SaovietTax
{
    partial class frmTaiCoQuanThue
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
            this.xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
            this.xtraTabPage1 = new DevExpress.XtraTab.XtraTabPage();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.qLHDBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colLoaiHD = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTotals = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDownloaded = new DevExpress.XtraGrid.Columns.GridColumn();
            this.xtraTabPage2 = new DevExpress.XtraTab.XtraTabPage();
            this.gridControl2 = new DevExpress.XtraGrid.GridControl();
            this.gridView2 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn3 = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).BeginInit();
            this.xtraTabControl1.SuspendLayout();
            this.xtraTabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.qLHDBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            this.xtraTabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).BeginInit();
            this.SuspendLayout();
            // 
            // xtraTabControl1
            // 
            this.xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xtraTabControl1.Location = new System.Drawing.Point(0, 0);
            this.xtraTabControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.xtraTabControl1.Name = "xtraTabControl1";
            this.xtraTabControl1.SelectedTabPage = this.xtraTabPage1;
            this.xtraTabControl1.Size = new System.Drawing.Size(722, 253);
            this.xtraTabControl1.TabIndex = 0;
            this.xtraTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.xtraTabPage1,
            this.xtraTabPage2});
            // 
            // xtraTabPage1
            // 
            this.xtraTabPage1.Controls.Add(this.gridControl1);
            this.xtraTabPage1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.xtraTabPage1.Name = "xtraTabPage1";
            this.xtraTabPage1.Size = new System.Drawing.Size(716, 221);
            this.xtraTabPage1.Text = "Hóa đơn vào";
            // 
            // gridControl1
            // 
            this.gridControl1.DataSource = this.qLHDBindingSource;
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gridControl1.Location = new System.Drawing.Point(7, 10);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(734, 438);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // qLHDBindingSource
            // 
            this.qLHDBindingSource.DataSource = typeof(SaovietTax.frmTaiCoQuanThue.QLHD);
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colLoaiHD,
            this.colTotals,
            this.colDownloaded});
            this.gridView1.DetailHeight = 295;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            // 
            // colLoaiHD
            // 
            this.colLoaiHD.FieldName = "LoaiHD";
            this.colLoaiHD.MinWidth = 23;
            this.colLoaiHD.Name = "colLoaiHD";
            this.colLoaiHD.Visible = true;
            this.colLoaiHD.VisibleIndex = 0;
            this.colLoaiHD.Width = 87;
            // 
            // colTotals
            // 
            this.colTotals.FieldName = "Totals";
            this.colTotals.MinWidth = 23;
            this.colTotals.Name = "colTotals";
            this.colTotals.Visible = true;
            this.colTotals.VisibleIndex = 1;
            this.colTotals.Width = 87;
            // 
            // colDownloaded
            // 
            this.colDownloaded.FieldName = "Downloaded";
            this.colDownloaded.MinWidth = 23;
            this.colDownloaded.Name = "colDownloaded";
            this.colDownloaded.Visible = true;
            this.colDownloaded.VisibleIndex = 2;
            this.colDownloaded.Width = 87;
            // 
            // xtraTabPage2
            // 
            this.xtraTabPage2.Controls.Add(this.gridControl2);
            this.xtraTabPage2.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.xtraTabPage2.Name = "xtraTabPage2";
            this.xtraTabPage2.Size = new System.Drawing.Size(716, 230);
            this.xtraTabPage2.Text = "Hóa đơn ra";
            // 
            // gridControl2
            // 
            this.gridControl2.DataSource = this.qLHDBindingSource;
            this.gridControl2.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gridControl2.Location = new System.Drawing.Point(8, 6);
            this.gridControl2.MainView = this.gridView2;
            this.gridControl2.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gridControl2.Name = "gridControl2";
            this.gridControl2.Size = new System.Drawing.Size(734, 438);
            this.gridControl2.TabIndex = 1;
            this.gridControl2.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView2});
            // 
            // gridView2
            // 
            this.gridView2.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn1,
            this.gridColumn2,
            this.gridColumn3});
            this.gridView2.DetailHeight = 295;
            this.gridView2.GridControl = this.gridControl2;
            this.gridView2.Name = "gridView2";
            // 
            // gridColumn1
            // 
            this.gridColumn1.FieldName = "LoaiHD";
            this.gridColumn1.MinWidth = 23;
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.Visible = true;
            this.gridColumn1.VisibleIndex = 0;
            this.gridColumn1.Width = 87;
            // 
            // gridColumn2
            // 
            this.gridColumn2.FieldName = "Totals";
            this.gridColumn2.MinWidth = 23;
            this.gridColumn2.Name = "gridColumn2";
            this.gridColumn2.Visible = true;
            this.gridColumn2.VisibleIndex = 1;
            this.gridColumn2.Width = 87;
            // 
            // gridColumn3
            // 
            this.gridColumn3.FieldName = "Downloaded";
            this.gridColumn3.MinWidth = 23;
            this.gridColumn3.Name = "gridColumn3";
            this.gridColumn3.Visible = true;
            this.gridColumn3.VisibleIndex = 2;
            this.gridColumn3.Width = 87;
            // 
            // frmTaiCoQuanThue
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(722, 253);
            this.Controls.Add(this.xtraTabControl1);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "frmTaiCoQuanThue";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bảng kê tải XML";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Load += new System.EventHandler(this.frmTaiCoQuanThue_Load);
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).EndInit();
            this.xtraTabControl1.ResumeLayout(false);
            this.xtraTabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.qLHDBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.xtraTabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraTab.XtraTabControl xtraTabControl1;
        private DevExpress.XtraTab.XtraTabPage xtraTabPage1;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraTab.XtraTabPage xtraTabPage2;
        private System.Windows.Forms.BindingSource qLHDBindingSource;
        private DevExpress.XtraGrid.Columns.GridColumn colLoaiHD;
        private DevExpress.XtraGrid.Columns.GridColumn colTotals;
        private DevExpress.XtraGrid.Columns.GridColumn colDownloaded;
        private DevExpress.XtraGrid.GridControl gridControl2;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn3;
    }
}