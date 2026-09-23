namespace SaovietTax
{
    partial class formtest
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
            this.memoEdit1 = new DevExpress.XtraEditors.MemoEdit();
            this.btnDownloadAllXml = new DevExpress.XtraEditors.SimpleButton();
            this.txtInvoiceNo = new DevExpress.XtraEditors.TextEdit();
            this.btnGetDetail = new DevExpress.XtraEditors.SimpleButton();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize)(this.memoEdit1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtInvoiceNo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // memoEdit1
            // 
            this.memoEdit1.Location = new System.Drawing.Point(23, 98);
            this.memoEdit1.Name = "memoEdit1";
            this.memoEdit1.Size = new System.Drawing.Size(700, 110);
            this.memoEdit1.TabIndex = 0;
            // 
            // btnDownloadAllXml
            // 
            this.btnDownloadAllXml.Location = new System.Drawing.Point(40, 53);
            this.btnDownloadAllXml.Name = "btnDownloadAllXml";
            this.btnDownloadAllXml.Size = new System.Drawing.Size(94, 29);
            this.btnDownloadAllXml.TabIndex = 1;
            this.btnDownloadAllXml.Text = "simpleButton1";
            // 
            // txtInvoiceNo
            // 
            this.txtInvoiceNo.Location = new System.Drawing.Point(209, 50);
            this.txtInvoiceNo.Name = "txtInvoiceNo";
            this.txtInvoiceNo.Size = new System.Drawing.Size(125, 23);
            this.txtInvoiceNo.TabIndex = 2;
            // 
            // btnGetDetail
            // 
            this.btnGetDetail.Location = new System.Drawing.Point(589, 53);
            this.btnGetDetail.Name = "btnGetDetail";
            this.btnGetDetail.Size = new System.Drawing.Size(94, 29);
            this.btnGetDetail.TabIndex = 3;
            this.btnGetDetail.Text = "simpleButton1";
            // 
            // gridControl1
            // 
            this.gridControl1.Location = new System.Drawing.Point(37, 256);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(663, 174);
            this.gridControl1.TabIndex = 4;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            // 
            // formtest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(758, 500);
            this.Controls.Add(this.gridControl1);
            this.Controls.Add(this.btnGetDetail);
            this.Controls.Add(this.txtInvoiceNo);
            this.Controls.Add(this.btnDownloadAllXml);
            this.Controls.Add(this.memoEdit1);
            this.Name = "formtest";
            this.Text = "formtest";
            this.Load += new System.EventHandler(this.formtest_Load);
            ((System.ComponentModel.ISupportInitialize)(this.memoEdit1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtInvoiceNo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.MemoEdit memoEdit1;
        private DevExpress.XtraEditors.SimpleButton btnDownloadAllXml;
        private DevExpress.XtraEditors.TextEdit txtInvoiceNo;
        private DevExpress.XtraEditors.SimpleButton btnGetDetail;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
    }
}