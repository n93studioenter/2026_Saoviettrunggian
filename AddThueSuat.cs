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
    public partial class AddThueSuat : DevExpress.XtraEditors.XtraForm
    {
        public AddThueSuat()
        {
            InitializeComponent();
        }
        public string password, connectionString;
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

        private void gridView1_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            var getMaso= gridView1.GetRowCellValue(e.RowHandle, "MaSo").ToString();
            var getThueSuat = gridView1.GetRowCellValue(e.RowHandle, "Thuesuat").ToString();
            var updateQr = @"UPDATE ChungTu  SET Thuesuat = ? WHERE MaSo =?";
            var updateParameters = new OleDbParameter[]
            {
        new OleDbParameter("?", getThueSuat), // Cập nhật giá trị TiLe 
        new OleDbParameter("?", getMaso),

            };
            var updateRowsAffected = ExecuteQueryResult(updateQr, updateParameters);
        }

        private void AddThueSuat_Load(object sender, EventArgs e)
        {
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);

            // Xóa phần \bin\Debug để lấy đường dẫn gốc
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));
            string filePath = Path.Combine(rootDirectory, "Hoadon", "invoice.txt");
            //MessageBox.Show(pathThumuc);
            string password = "1@35^7*9)1";
            string dbPath = "";
           

            string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");
           
            //MessageBox.Show(pathThumuc);
            try
            {
                string contents = File.ReadAllText(filePaths);
                dbPath = contents;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi đọc file: " + ex.Message);
            }
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";
            string content = File.ReadAllText(filePath);
            var getsplit = content.Split('_');

            var qrTimct = @"
    SELECT ChungTu.MaSo,Thuesuat,Vattu.SoHieu,Vattu.TenVattu
    FROM ChungTu 
    INNER JOIN Vattu ON Vattu.MaSo = ChungTu.MaVattu 
    where ChungTu.SoHieu=? and ChungTu.NgayCT=? and ChungTu.MaTKCo <> 14038";

            // 3. Tham số — đúng thứ tự khớp với ? trong SQL
            var parameterss = new OleDbParameter[]
            {
    new OleDbParameter("?", getsplit[1]), 
    new OleDbParameter("?", getsplit[3])  // Ép kiểu Date
            };

            // 4. Thực thi
            var kq2 = ExecuteQuery(qrTimct, parameterss);
            foreach(DataRow rows in kq2.Rows)
            {
                rows["TenVattu"] = Helpers.ConvertVniToUnicode(rows["TenVattu"].ToString()) ;
            }
            gridControl1.DataSource = kq2;
        }
    }
}