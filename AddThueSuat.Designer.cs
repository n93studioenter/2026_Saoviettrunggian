namespace SaovietTax
{
    partial class AddThueSuat
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
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colSoHieu = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colTenvattu = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colThuesuat = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colMaSo = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControl1
            // 
            this.gridControl1.Location = new System.Drawing.Point(1, 3);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(768, 290);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colSoHieu,
            this.colTenvattu,
            this.colThuesuat,
            this.colMaSo});
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gridView1_CellValueChanged);
            // 
            // colSoHieu
            // 
            this.colSoHieu.Caption = "SoHieu";
            this.colSoHieu.FieldName = "SoHieu";
            this.colSoHieu.Name = "colSoHieu";
            this.colSoHieu.OptionsColumn.AllowEdit = false;
            this.colSoHieu.Visible = true;
            this.colSoHieu.VisibleIndex = 1;
            this.colSoHieu.Width = 249;
            // 
            // colTenvattu
            // 
            this.colTenvattu.Caption = "TenVattu";
            this.colTenvattu.FieldName = "TenVattu";
            this.colTenvattu.Name = "colTenvattu";
            this.colTenvattu.OptionsColumn.AllowEdit = false;
            this.colTenvattu.Visible = true;
            this.colTenvattu.VisibleIndex = 2;
            this.colTenvattu.Width = 383;
            // 
            // colThuesuat
            // 
            this.colThuesuat.Caption = "Thuesuat";
            this.colThuesuat.FieldName = "Thuesuat";
            this.colThuesuat.Name = "colThuesuat";
            this.colThuesuat.Visible = true;
            this.colThuesuat.VisibleIndex = 3;
            this.colThuesuat.Width = 115;
            // 
            // colMaSo
            // 
            this.colMaSo.Caption = "MaSo";
            this.colMaSo.FieldName = "MaSo";
            this.colMaSo.Name = "colMaSo";
            this.colMaSo.Visible = true;
            this.colMaSo.VisibleIndex = 0;
            // 
            // AddThueSuat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(781, 305);
            this.Controls.Add(this.gridControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "AddThueSuat";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AddThueSuat";
            this.Load += new System.EventHandler(this.AddThueSuat_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraGrid.Columns.GridColumn colSoHieu;
        private DevExpress.XtraGrid.Columns.GridColumn colThuesuat;
        private DevExpress.XtraGrid.Columns.GridColumn colTenvattu;
        private DevExpress.XtraGrid.Columns.GridColumn colMaSo;
    }
}