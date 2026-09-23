using DevExpress.CodeParser;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaovietTax
{
    public partial class FrmMessage : DevExpress.XtraEditors.XtraForm
    {
        public FrmMessage(string text)
        {
            InitializeComponent();
            this.TopMost = true;
            this.Opacity = 0; // Bắt đầu trong suốt
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;

            // Đặt kích thước form (tuỳ chỉnh) 

            // Đặt form ở góc phải dưới
            this.Location = new Point(
                Screen.PrimaryScreen.WorkingArea.Right - this.Width,
                Screen.PrimaryScreen.WorkingArea.Bottom - this.Height
            );
            labelControl1.Text = $"Đang import hoá đơn tự động cho";
            labelControl2.Text = text;
        }
        private System.Windows.Forms.Timer animationTimer;
        private int startY;
        private int endY;
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            bool complete = true;

            // Fade in
            if (this.Opacity < 1)
            {
                this.Opacity += 0.05;
                if (this.Opacity > 1) this.Opacity = 1;
                complete = false;
            }

            // Slide up
            if (this.Location.Y > endY)
            {
                this.Location = new Point(this.Location.X, this.Location.Y - 20);
                if (this.Location.Y < endY) this.Location = new Point(this.Location.X, endY);
                complete = false;
            }

            if (complete)
            {
                animationTimer.Stop();
                animationTimer.Dispose();
            }
            timer1.Start();
        }
        private void FrmMessage_Load(object sender, EventArgs e)
        {
            startY = Screen.PrimaryScreen.WorkingArea.Height;
            endY = (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 2 + (Screen.PrimaryScreen.WorkingArea.Height - this.Height) /2+30;

            this.Location = new Point(this.Location.X, startY);

            // Bắt đầu animation
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 5;
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
           
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}