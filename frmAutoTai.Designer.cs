namespace SaovietTax
{
    partial class frmAutoTai
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAutoTai));
            this.txtUsername = new DevExpress.XtraEditors.TextEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.txtPassword = new DevExpress.XtraEditors.TextEdit();
            this.txtToken = new DevExpress.XtraEditors.TextEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.currentState = new DevExpress.XtraEditors.LabelControl();
            this.stateDetail = new DevExpress.XtraEditors.LabelControl();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.txtUsername.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtToken.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(131, 12);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(213, 23);
            this.txtUsername.TabIndex = 0;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(21, 19);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(58, 16);
            this.labelControl1.TabIndex = 1;
            this.labelControl1.Text = "Username";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(374, 15);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(55, 16);
            this.labelControl2.TabIndex = 2;
            this.labelControl2.Text = "Password";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(446, 12);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(239, 23);
            this.txtPassword.TabIndex = 3;
            // 
            // txtToken
            // 
            this.txtToken.Location = new System.Drawing.Point(131, 53);
            this.txtToken.Name = "txtToken";
            this.txtToken.Size = new System.Drawing.Size(554, 23);
            this.txtToken.TabIndex = 5;
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(21, 56);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(35, 16);
            this.labelControl3.TabIndex = 4;
            this.labelControl3.Text = "Token";
            // 
            // currentState
            // 
            this.currentState.Appearance.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.currentState.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.currentState.Appearance.Options.UseFont = true;
            this.currentState.Appearance.Options.UseForeColor = true;
            this.currentState.Location = new System.Drawing.Point(21, 110);
            this.currentState.Name = "currentState";
            this.currentState.Size = new System.Drawing.Size(85, 16);
            this.currentState.TabIndex = 6;
            this.currentState.Text = "labelControl4";
            // 
            // stateDetail
            // 
            this.stateDetail.Location = new System.Drawing.Point(21, 149);
            this.stateDetail.Name = "stateDetail";
            this.stateDetail.Size = new System.Drawing.Size(75, 16);
            this.stateDetail.TabIndex = 7;
            this.stateDetail.Text = "labelControl4";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(21, 187);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(664, 171);
            this.richTextBox1.TabIndex = 8;
            this.richTextBox1.Text = "";
            // 
            // frmAutoTai
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(698, 371);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.stateDetail);
            this.Controls.Add(this.currentState);
            this.Controls.Add(this.txtToken);
            this.Controls.Add(this.labelControl3);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.txtUsername);
            this.IconOptions.Image = ((System.Drawing.Image)(resources.GetObject("frmAutoTai.IconOptions.Image")));
            this.Name = "frmAutoTai";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Tải hoá đơn tự động";
            this.Load += new System.EventHandler(this.frmAutoTai_Load);
            ((System.ComponentModel.ISupportInitialize)(this.txtUsername.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtToken.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.TextEdit txtUsername;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.TextEdit txtPassword;
        private DevExpress.XtraEditors.TextEdit txtToken;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.LabelControl currentState;
        private DevExpress.XtraEditors.LabelControl stateDetail;
        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}