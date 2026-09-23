using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
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
    public partial class frmTuKhoaHang : DevExpress.XtraEditors.XtraForm
    {
        public frmTuKhoaHang()
        {
            InitializeComponent();
        }
        public string dbPath = "";
        string connectionString = "";
        private void frmTuKhoaHang_Load(object sender, EventArgs e)
        {
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);

            // Xóa phần \bin\Debug để lấy đường dẫn gốc
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
            string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");
            string pathThumuc = Path.Combine(rootDirectory);
         
            //MessageBox.Show(pathThumuc);
            try
            {
                string content = File.ReadAllText(filePaths);
                dbPath = content;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi đọc file: " + ex.Message);
            }


            // Đọc toàn bộ nội dung tệp
            string password = "1@35^7*9)1";
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";

            LoadData();

        }
        private void LoadData()
        {
            string sql = "select * from tbTuKhoaHangHoa";
            DataTable tbTuKhoaHangHoa = ExecuteQuery(sql); 
            gridControl1.DataSource = tbTuKhoaHangHoa;
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var query = @"INSERT INTO tbTuKhoaHangHoa (Name) VALUES (?)";
            var parameters = new OleDbParameter[]
             {
            new OleDbParameter("?", textEdit1.Text), 
             };
            var rowsAffected = ExecuteQueryResult(query, parameters);
            textEdit1.Text = "";
            LoadData();
        }
        int currentID = 0;
        private void gridView1_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            
        }

        private void gridView1_MouseDown(object sender, MouseEventArgs e)
        {
            GridView view = sender as GridView;

            // Lấy thông tin chính xác vị trí chuột click
            GridHitInfo hitInfo = view.CalcHitInfo(e.Location);

            // Chỉ xử lý khi click vào ô dữ liệu
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                string columnName = hitInfo.Column.FieldName;

                // Lấy giá trị cột "ID" của dòng đang click
                currentID = int.Parse(view.GetRowCellValue(rowHandle, "ID").ToString());  
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            var query = @"Delete from tbTuKhoaHangHoa where ID = ?";
            var parameters = new OleDbParameter[]
             {
            new OleDbParameter("?", currentID),
             };
            var rowsAffected = ExecuteQueryResult(query, parameters);
            LoadData();
        }

        private void gridView1_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            int rowHandle = e.RowHandle;
            int ID = int.Parse(gridView1.GetRowCellValue(rowHandle, "ID").ToString());
            string newvalue = gridView1.GetRowCellValue(rowHandle, "Name").ToString();


            string query = @"UPDATE tbTuKhoaHangHoa 
                 SET Name = ?
                 WHERE ID=?";

            var parameters = new OleDbParameter[]
            {
                new OleDbParameter("?", newvalue),
                new OleDbParameter("?", currentID) 
             };

            // Gọi hàm thực thi câu lệnh SQL
            int rowsAffected = ExecuteQueryResult(query, parameters);
            LoadData();
        }

        private void gridControl1_Click(object sender, EventArgs e)
        {

        }
    }
}