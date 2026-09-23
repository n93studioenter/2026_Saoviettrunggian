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
    public partial class frmXemhoadonInvoicse : DevExpress.XtraEditors.XtraForm
    {
        public vb6Xemhoadon invoicse;
        public frmXemhoadonInvoicse()
        { 
            InitializeComponent();
        }
        public string path { get; set; }
        public string name { get; set; }
        private void webView21_Click(object sender, EventArgs e)
        {

        }

        private void frmXemhoadonInvoicse_Load(object sender, EventArgs e)
        {
            try
            {
                webView21.Source = new Uri(path);
                this.Text = name;
            }
            catch (Exception ex)
            {
            }
        }

        private void frmXemhoadonInvoicse_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(invoicse!=null)
            invoicse.Close();
        }
    }
}