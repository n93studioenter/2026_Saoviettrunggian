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
    public partial class SettingTaiCQT : DevExpress.XtraEditors.XtraForm
    {
        public SettingTaiCQT()
        {
            InitializeComponent();
        }
        public frmMain frmMain;
        private void checkEdit1_CheckedChanged(object sender, EventArgs e)
        {
            frmMain.hddvcoma = checkEdit1.Checked;

        }

        private void SettingTaiCQT_Load(object sender, EventArgs e)
        {
            checkEdit1.Checked = frmMain.hddvcoma;
            checkEdit2.Checked = frmMain.hddvknm;
            checkEdit3.Checked = frmMain.hddvmtt;
        }

        private void checkEdit2_CheckedChanged(object sender, EventArgs e)
        {
            frmMain.hddvknm= checkEdit2.Checked;
        }

        private void checkEdit3_CheckedChanged(object sender, EventArgs e)
        {
            frmMain.hddvmtt= checkEdit3.Checked;
        }
    }
}