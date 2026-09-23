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
    public partial class frmStatusAuto : DevExpress.XtraEditors.XtraForm
    {
        public frmStatusAuto()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;

            // Đặt kích thước form (tuỳ chỉnh) 

            // Đặt form ở góc phải dưới
            this.Location = new Point(
                Screen.PrimaryScreen.WorkingArea.Right - this.Width,
                Screen.PrimaryScreen.WorkingArea.Bottom - this.Height
            );
        }
        public void LoadMessage(string message)
        {
            progressPanel1.Caption= message;
        }
        public async void SetDescription(string messg)
        {
            progressPanel1.Description= messg;
        }
        private void frmStatusAuto_Load(object sender, EventArgs e)
        {

        }
    }
}