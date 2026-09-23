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
    public partial class frmSetting : DevExpress.XtraEditors.XtraForm
    {
        public frmSetting()
        {
            InitializeComponent();
        }
       
        public frmMain frmMain { get; set; }
        private void frmSetting_Load(object sender, EventArgs e)
        {
            chkSuggest.Checked = frmMain.IsPopup();
            checkEdit2.Checked = frmMain.IsPopup2();
            checkEdit3.Checked= frmMain.IsPopup3();
        }

        private void chkSuggest_CheckedChanged(object sender, EventArgs e)
        {
            frmMain.UpdatePopup(chkSuggest.Checked ? 1 : 0);
        }

        private void checkEdit2_CheckedChanged(object sender, EventArgs e)
        {
            frmMain.UpdatePopup2(checkEdit2.Checked ? 1 : 0);
        }

        private void checkEdit3_CheckedChanged(object sender, EventArgs e)
        {
            frmMain.UpdatePopup3(checkEdit3.Checked ? 1 : 0);
        }
    }
}