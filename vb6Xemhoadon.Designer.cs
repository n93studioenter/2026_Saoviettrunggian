namespace SaovietTax
{
    partial class vb6Xemhoadon
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
            this.frmWebbrowser = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // frmWebbrowser
            // 
            this.frmWebbrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.frmWebbrowser.Location = new System.Drawing.Point(0, 0);
            this.frmWebbrowser.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.frmWebbrowser.MinimumSize = new System.Drawing.Size(17, 16);
            this.frmWebbrowser.Name = "frmWebbrowser";
            this.frmWebbrowser.Size = new System.Drawing.Size(782, 481);
            this.frmWebbrowser.TabIndex = 0;
            // 
            // vb6Xemhoadon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(782, 481);
            this.Controls.Add(this.frmWebbrowser);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "vb6Xemhoadon";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "vb6Xemhoadon";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.vb6Xemhoadon_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.vb6Xemhoadon_FormClosed);
            this.Load += new System.EventHandler(this.vb6Xemhoadon_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser frmWebbrowser;
    }
}