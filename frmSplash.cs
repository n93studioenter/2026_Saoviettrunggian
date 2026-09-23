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
    public partial class frmSplash : DevExpress.XtraEditors.XtraForm
    {
        public frmSplash()
        {
            InitializeComponent();
            this.Shown += async (s, e) =>
            {
                // Đảm bảo form đã hiển thị hoàn toàn
                await Task.Delay(50);
            };
        }
        public void Hienthithongtin(string msg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(Hienthithongtin), msg);
                return;
            }

            labelControl1.Text = msg;
            labelControl1.Refresh();   // Ép vẽ lại ngay
            this.Refresh();            // Ép vẽ lại toàn form
            Application.DoEvents();    // Cho phép UI xử lý message
        }
        private async void frmSplash_Load(object sender, EventArgs e)
        {
            // Force render ngay lập tức
            this.Refresh();
            Application.DoEvents();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }

        private void labelControl2_Click(object sender, EventArgs e)
        {

        }
    }
}