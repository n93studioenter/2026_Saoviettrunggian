using DevExpress.XtraEditors;
using SaovietTax.DTO;
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
    public partial class frmDocnganhang : DevExpress.XtraEditors.XtraForm
    {
        public frmDocnganhang()
        {
            InitializeComponent();
        }

        private void frmDocnganhang_Load(object sender, EventArgs e)
        {
            DataTable dt =
    BankStatementReader.Read(@"C:\Users\Admin\Desktop\nganhang\T7.pdf");

            foreach (DataRow row in dt.Rows)
            {
                Console.WriteLine(
                    $"{row["STT"]} | " +
                    $"{row["SoGiaoDich"]} | " +
                    $"{row["NgayGiaoDich"]} | " +
                    $"{row["NoiDung"]} | " +
                    $"{row["TienRut"]} | " +
                    $"{row["TienGui"]} | " +
                    $"{row["SoDu"]}");
            }
            gridControl1.DataSource= dt;
        }
    }
}