namespace SaovietTax
{
    partial class Assistant
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.btnThucthi = new DevExpress.XtraEditors.SimpleButton();
            this.btnClose = new DevExpress.XtraEditors.SimpleButton();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(3, 45);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(438, 325);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // btnThucthi
            // 
            this.btnThucthi.ImageOptions.Image = global::SaovietTax.Properties.Resources.assignto_32x32;
            this.btnThucthi.Location = new System.Drawing.Point(308, 376);
            this.btnThucthi.Name = "btnThucthi";
            this.btnThucthi.Size = new System.Drawing.Size(133, 40);
            this.btnThucthi.TabIndex = 1;
            this.btnThucthi.Text = "Thực thi";
            this.btnThucthi.Click += new System.EventHandler(this.btnThucthi_Click);
            // 
            // btnClose
            // 
            this.btnClose.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnClose.Location = new System.Drawing.Point(404, 8);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(30, 30);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "✕";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // Assistant
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 425);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnThucthi);
            this.Controls.Add(this.richTextBox1);
            this.Name = "Assistant";
            this.Text = "Assistant";
            this.Load += new System.EventHandler(this.Assistant_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private DevExpress.XtraEditors.SimpleButton btnThucthi;
        private DevExpress.XtraEditors.SimpleButton btnClose;
    }
}