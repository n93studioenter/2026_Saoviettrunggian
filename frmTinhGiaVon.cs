using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaovietTax
{
    public partial class frmTinhGiaVon : DevExpress.XtraEditors.XtraForm
    {
        public frmTinhGiaVon()
        {
            InitializeComponent();
        }
        public System.Data.DataTable ExecuteQuery(string query, params OleDbParameter[] parameters)
        {
            System.Data.DataTable dataTable = new System.Data.DataTable();

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        // Thêm các tham số vào command
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(command))
                        {
                            dataAdapter.Fill(dataTable);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }

            return dataTable; // Trả về DataTable chứa dữ liệu
        }
        public int ExecuteQueryResult(string query, params OleDbParameter[] parameters)
        {
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Kết nối đến cơ sở dữ liệu thành công! " + query);

                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    // Thêm tham số
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    // Thực thi INSERT, UPDATE, DELETE
                    command.ExecuteNonQuery();
                }

                // Lấy ID vừa thêm bằng @@IDENTITY
                using (OleDbCommand idCommand = new OleDbCommand("SELECT @@IDENTITY", connection))
                {
                    object result = idCommand.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        public string dbPath = "";
        public string connectionString = "";
        public void InitDB()
        {
            // 1. Đọc dbPath (rất nhanh)
            if (string.IsNullOrEmpty(dbPath))
            {
                string exeDir = Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location);

                string root = Path.GetFullPath(
                    Path.Combine(exeDir, @"..\.."));

                string pathFile = Path.Combine(root, "hoadon", "dpPath.txt");
                dbPath = File.ReadAllText(pathFile).Trim();
                string fullName = dbPath.Substring(dbPath.LastIndexOf('\\') + 1);
                // fullName = "Thanh Huong BD2026.mdb"

                string fn = fullName.Substring(0, fullName.LastIndexOf('.')).Trim(); 
            }

            // 2. Build connection string (string concat nhanh hơn interpolated)
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString =
                    "Provider=Microsoft.ACE.OLEDB.12.0;" +
                    "Data Source=" + dbPath + ";" +
                    "Jet OLEDB:Database Password=1@35^7*9)1;";
            }

            // 3. MỞ KẾT NỐI NGẮN GỌN + LOAD LICENSE
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {

                }
                // không tránh được
                //LoadLicenseInfo(conn); // truyền connection vào
            }
        }
        DataTable tbChungtu { get; set; }
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            string querydinhdanh = @"SELECT * FROM Vattu WHERE SoHieu = ?"; // Sử dụng ? thay cho @mst trong OleDb
            var resultkm = ExecuteQuery(querydinhdanh, new OleDbParameter("?", textEdit1.Text));
            int mavattu = 0;
            if (resultkm.Rows.Count > 0)
            {
                mavattu = int.Parse(resultkm.Rows[0]["MaSo"].ToString());
            }

            string quct = "select * from ChungTu";
            tbChungtu = ExecuteQuery(quct);
            var chungtutheongay = tbChungtu.AsEnumerable().Where(m => m.Field<DateTime>("NgayCT").Date >= dateEdit1.DateTime.Date && m.Field<DateTime>("NgayCT").Date <= dateEdit2.DateTime.Date);
            var chungtutheovattu=chungtutheongay.Where(m => (mavattu == 0 || m["MaVattu"].ToString() == mavattu.ToString())).CopyToDataTable();
            var lstNhap = chungtutheovattu.AsEnumerable().Where(m => m["MaLoai"].ToString() == "1").CopyToDataTable();
            var lstGV = chungtutheovattu.AsEnumerable().Where(m => m["MaLoai"].ToString() == "2").CopyToDataTable();
            var lstXuat = chungtutheovattu.AsEnumerable().Where(m => m["MaLoai"].ToString() == "8").CopyToDataTable();


            //Tính tồn đầu kỳ
            string qrton = @"SELECT * FROM TonKho WHERE MaVatTu = ?"; // Sử dụng ? thay cho @mst trong OleDb
            var rstonkho = ExecuteQuery(qrton, new OleDbParameter("?", mavattu));
            double SLTon= double.Parse(rstonkho.Rows[0]["Luong_0"].ToString());
            double TienTon = double.Parse(rstonkho.Rows[0]["Tien_0"].ToString());
            //Duyệt danh sách giá vốn để cập nhật lại
            bool Hastinhton = false;
            double currentSL = 0;
            double currentTien = 0;
            double curentGiavon = 0;
            DateTime mocthoigian= dateEdit1.DateTime.Date;
            int LastMaso = 0;
            foreach(DataRow dataRow in lstGV.Rows)
            {
                //Tính nhập xuất trước đó
                //Get Ngay
                
                var getNhapTrcdo = lstNhap.AsEnumerable().Where(m => m.Field<DateTime>("NgayCT").Date < dataRow.Field<DateTime>("NgayCT")).CopyToDataTable();
               
            }
        }

        private void frmTinhGiaVon_Load(object sender, EventArgs e)
        {
            dateEdit1.EditValue = "01/01/2026";
            dateEdit2.EditValue = "31/01/2026";
            InitDB(); 
        }
    }
}