namespace SaovietTax
{
    partial class SuggestVattu
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SuggestVattu));
            this.lblText = new DevExpress.XtraEditors.LabelControl();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.vattugoiyBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colTen = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colSoHieu = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colPercent = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDonVi = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDongia = new DevExpress.XtraGrid.Columns.GridColumn();
            this.suggestVattuBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.vattugoiyBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.suggestVattuBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // lblText
            // 
            this.lblText.Appearance.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            this.lblText.Appearance.Options.UseFont = true;
            this.lblText.Location = new System.Drawing.Point(15, 11);
            this.lblText.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(177, 17);
            this.lblText.TabIndex = 3;
            this.lblText.Text = "Danh sách hàng hoá gợi ý";
            // 
            // simpleButton1
            // 
            this.simpleButton1.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.simpleButton1.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("simpleButton1.ImageOptions.SvgImage")));
            this.simpleButton1.Location = new System.Drawing.Point(824, 3);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(38, 31);
            this.simpleButton1.TabIndex = 4;
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // gridControl1
            // 
            this.gridControl1.DataSource = this.vattugoiyBindingSource;
            this.gridControl1.Location = new System.Drawing.Point(3, 42);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(859, 491);
            this.gridControl1.TabIndex = 5;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            this.gridControl1.DoubleClick += new System.EventHandler(this.gridControl1_DoubleClick);
            // 
            // vattugoiyBindingSource
            // 
            this.vattugoiyBindingSource.DataSource = typeof(SaovietTax.frmMain.Vattugoiy);
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colTen,
            this.colSoHieu,
            this.colPercent,
            this.colDonVi,
            this.colDongia});
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            // 
            // colTen
            // 
            this.colTen.Caption = "Tên vật tư";
            this.colTen.FieldName = "Ten";
            this.colTen.MinWidth = 25;
            this.colTen.Name = "colTen";
            this.colTen.OptionsColumn.AllowEdit = false;
            this.colTen.Visible = true;
            this.colTen.VisibleIndex = 1;
            this.colTen.Width = 459;
            // 
            // colSoHieu
            // 
            this.colSoHieu.Caption = "Số hiệu";
            this.colSoHieu.FieldName = "SoHieu";
            this.colSoHieu.MinWidth = 25;
            this.colSoHieu.Name = "colSoHieu";
            this.colSoHieu.OptionsColumn.AllowEdit = false;
            this.colSoHieu.Visible = true;
            this.colSoHieu.VisibleIndex = 2;
            this.colSoHieu.Width = 119;
            // 
            // colPercent
            // 
            this.colPercent.Caption = "Tỉ lệ";
            this.colPercent.FieldName = "Percent";
            this.colPercent.MinWidth = 25;
            this.colPercent.Name = "colPercent";
            this.colPercent.OptionsColumn.AllowEdit = false;
            this.colPercent.Visible = true;
            this.colPercent.VisibleIndex = 0;
            this.colPercent.Width = 62;
            // 
            // colDonVi
            // 
            this.colDonVi.Caption = "Đơn vị tính";
            this.colDonVi.FieldName = "DonVi";
            this.colDonVi.MinWidth = 25;
            this.colDonVi.Name = "colDonVi";
            this.colDonVi.OptionsColumn.AllowEdit = false;
            this.colDonVi.Visible = true;
            this.colDonVi.VisibleIndex = 3;
            this.colDonVi.Width = 72;
            // 
            // colDongia
            // 
            this.colDongia.Caption = "Đơn giá";
            this.colDongia.DisplayFormat.FormatString = "N0";
            this.colDongia.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colDongia.FieldName = "Dongia";
            this.colDongia.MinWidth = 25;
            this.colDongia.Name = "colDongia";
            this.colDongia.OptionsColumn.AllowEdit = false;
            this.colDongia.Visible = true;
            this.colDongia.VisibleIndex = 4;
            this.colDongia.Width = 126;
            // 
            // suggestVattuBindingSource
            // 
            this.suggestVattuBindingSource.DataSource = typeof(SaovietTax.SuggestVattu);
            // 
            // SuggestVattu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControl1);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.lblText);
            this.Name = "SuggestVattu";
            this.Size = new System.Drawing.Size(865, 536);
            this.Load += new System.EventHandler(this.SuggestVattu_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.vattugoiyBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.suggestVattuBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DevExpress.XtraEditors.LabelControl lblText;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private System.Windows.Forms.BindingSource suggestVattuBindingSource;
        private System.Windows.Forms.BindingSource vattugoiyBindingSource;
        private DevExpress.XtraGrid.Columns.GridColumn colTen;
        private DevExpress.XtraGrid.Columns.GridColumn colSoHieu;
        private DevExpress.XtraGrid.Columns.GridColumn colPercent;
        private DevExpress.XtraGrid.Columns.GridColumn colDonVi;
        private DevExpress.XtraGrid.Columns.GridColumn colDongia;
    }
}
