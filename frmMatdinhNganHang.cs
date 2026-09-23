using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Data.OleDb;
using System.Reflection;
using System.IO;
using static DevExpress.Xpo.Helpers.AssociatedCollectionCriteriaHelper;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Grid;
namespace SaovietTax
{
    public partial class frmMatdinhNganHang : DevExpress.XtraEditors.XtraForm
    {
        public frmMatdinhNganHang()
        {
            InitializeComponent();
        }
       
        private void frmMatdinhNganHang_Load(object sender, EventArgs e)
        {
            LoadData();
            GridColumn deleteColumn = new GridColumn();
            deleteColumn.Caption = "Xóa";
            deleteColumn.UnboundType = DevExpress.Data.UnboundColumnType.Object;
            deleteColumn.FieldName = "Delete"; // Chỉ định FieldName
            deleteColumn.VisibleIndex = gridView1.Columns.Count; // Đặt vị trí cột
            gridView1.Columns.Add(deleteColumn);
            xtraTabControl1.TabPages[0].PageVisible= false; // Ẩn tab đầu tiên
        }
        private void LoadDatachung()
        {
            string querykh2 = @" SELECT *  FROM tbMatdinhnganhang"; // Sử dụng ? thay cho @mst trong OleDb

            var result2 = ExecuteQuery2(querykh2, new OleDbParameter("?", ""));
            gridControl1.DataSource = result2;
        }
        private void LoadData()
        {
            string querykh = @" SELECT *  FROM tbDinhdanhNganhang"; // Sử dụng ? thay cho @mst trong OleDb

            var result = ExecuteQuery(querykh, new OleDbParameter("?", ""));
            gcDinhdanh.DataSource = result;

             querykh = @" SELECT *  FROM tbMatdinhghichu"; // Sử dụng ? thay cho @mst trong OleDb

             result = ExecuteQuery(querykh, new OleDbParameter("?", ""));
            textEdit4.Text = Helpers.ConvertVniToUnicode(result.Rows[0]["Noidung"].ToString());
            textEdit3.Text = result.Rows[0]["TK"].ToString();

            //
            //LoadDatachung();
        }

        private void btnLuudinhdanh_Click(object sender, EventArgs e)
        {
            var query = @"INSERT INTO tbDinhdanhNganhang (Noidung,TK,SoHieu,TK2) VALUES (?, ?,?,?)";
            var parameters = new OleDbParameter[]
            {
            new OleDbParameter("?", txtDiengiai.Text), 
            new OleDbParameter("?", textEdit1.Text),
            new OleDbParameter("?", txtMaKH.Text),
            new OleDbParameter("?", textEdit2.Text),
            };
            int rowsAffected = ExecuteQueryResult(query, parameters);
            LoadData();
        }

        string dbPath = "";
        public string Sohieu { get; set; }
        public DataTable ExecuteQuery2(string query, params OleDbParameter[] parameters)
        {
            DataTable dataTable = new DataTable();

           string  newdbPath = "\\\\192.168.1.90\\Ke toan 2025 New\\1 Copi vao dung1\\Saovietdb.mdb";
            string connectionString = "";
            string password = "1@35^7*9)1";
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={newdbPath};";
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    Console.WriteLine("Kết nối đến cơ sở dữ liệu thành công!");

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
        public int ExecuteQueryResult2(string query, params OleDbParameter[] parameters)
        {
            string connectionString = "";
            string password = "1@35^7*9)1";
            string newdbPath = "\\\\192.168.1.90\\Ke toan 2025 New\\1 Copi vao dung\\Saovietdb.mdb";
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={newdbPath}";
            DataTable dataTable = new DataTable();

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Kết nối đến cơ sở dữ liệu thành công!");

                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    // Thêm các tham số vào command
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    int rowsAffected = command.ExecuteNonQuery(); // Thực thi câu lệnh
                    return rowsAffected;
                }
            }

            return -1;
        }
        private DataTable ExecuteQuery(string query, params OleDbParameter[] parameters)
        {
            DataTable dataTable = new DataTable();
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);

            // Xóa phần \bin\Debug để lấy đường dẫn gốc
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
            string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");
            try
            {
                string content = File.ReadAllText(filePaths);
                dbPath = content;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi đọc file: " + ex.Message);
            }
            string connectionString = "";
            string password = "1@35^7*9)1";
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    Console.WriteLine("Kết nối đến cơ sở dữ liệu thành công!");

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
        private int ExecuteQueryResult(string query, params OleDbParameter[] parameters)
        {
            string connectionString = "";
            string password = "1@35^7*9)1";
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";
            DataTable dataTable = new DataTable();

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Kết nối đến cơ sở dữ liệu thành công!");

                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    // Thêm các tham số vào command
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    int rowsAffected = command.ExecuteNonQuery(); // Thực thi câu lệnh
                    return rowsAffected;
                }
            }

            return -1;
        }

        private void gridView2_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            GridView gridView = gcDinhdanh.MainView as GridView;
            // Lấy thông tin về hàng và cột của ô đã thay đổi
            int rowHandle = e.RowHandle;
            string columnName = e.Column.FieldName; // Tên cột
            //Lấy current data row
            int ID = int.Parse(gridView.GetRowCellValue(rowHandle, gridView.Columns["ID"]).ToString());
            string Noidung = gridView.GetRowCellValue(rowHandle, gridView.Columns["Noidung"]).ToString();
            string TK = gridView.GetRowCellValue(rowHandle, gridView.Columns["TK"]).ToString();  
            string sql = "UPDATE tbDinhdanhNganhang SET Noidung = ?, TK = ?  WHERE ID = ?";
            OleDbParameter[] parameters = new OleDbParameter[]
 { 
           new OleDbParameter("?",Noidung),
                 new OleDbParameter("?",TK), 
                new OleDbParameter("?",ID)
 };
            int resl = ExecuteQueryResult(sql, parameters);
        }

        private void gridView2_RowCellClick(object sender, RowCellClickEventArgs e)
        {
            var getNAmecol = e.Column;
            if (getNAmecol.ToString() == "Xóa")
            {
                if (XtraMessageBox.Show("Bạn có chắc chắn muốn xóa hàng này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var getID = gridView2.GetRowCellValue(e.RowHandle, "ID");
                    string sql = "DELETE FROM tbDinhdanhNganhang WHERE ID = ?";
                    OleDbParameter[] parameters = new OleDbParameter[]
                {
        new OleDbParameter("?", getID),
                };
                    int resl = ExecuteQueryResult(sql, parameters);
                    LoadData();
                }
               
            }
        }

        private void txtMaKH_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                frmKhachhang frmKhachhang = new frmKhachhang();
                frmKhachhang.frmMatdinhNganHang = this; // Truyền tham chiếu đến frmMatdinhNganHang
                frmKhachhang.ShowDialog();
                txtMaKH.Text = Sohieu; // Lấy giá trị từ frmKhachhang
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            string sql = "UPDATE tbMatdinhghichu SET TK = ?, Noidung = ?";
            OleDbParameter[] parameters = new OleDbParameter[]
             {
                       new OleDbParameter("?",textEdit3.Text),
                       new OleDbParameter("?",Helpers.ConvertUnicodeToVni(textEdit4.Text)), 
             };
            try
            {
                int resl = ExecuteQueryResult(sql, parameters);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật: " + ex.Message);
            }   
        }
        private void ResetData()
        {
            txtNoidungchung.Text = "";
            txtTKChung.Text = "";
        }   
        private void btnLuuchung_Click(object sender, EventArgs e)
        {
            var query = @"INSERT INTO tbMatdinhnganhang (Noidung,Tk) VALUES (?, ?)";
            var parameters = new OleDbParameter[]
            {
            new OleDbParameter("?", txtNoidungchung.Text),
            new OleDbParameter("?", txtTKChung.Text),
            };
            int rowsAffected = ExecuteQueryResult2(query, parameters);
            LoadDatachung();
        }

        private void gridView1_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            if (e.Column.FieldName == "Delete")
            {
                RepositoryItemButtonEdit buttonEdit = new RepositoryItemButtonEdit();
                buttonEdit.Buttons[0].Kind = DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph;
                buttonEdit.Buttons[0].Caption = "Xóa";
                buttonEdit.ButtonClick += ButtonEdit_ButtonClick; // Gán sự kiện click
                e.RepositoryItem = buttonEdit;
            }
        }
        private void ButtonEdit_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            // Lấy hàng hiện tại
            var rowHandle = gridView1.FocusedRowHandle;
            if (XtraMessageBox.Show("Bạn có chắc chắn muốn xóa hàng này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var getID = gridView1.GetRowCellValue(rowHandle, "ID");
                string sql = "DELETE FROM tbMatdinhnganhang WHERE ID = ?";
                OleDbParameter[] parameters = new OleDbParameter[]
            {
        new OleDbParameter("?", getID),
            };
                int resl = ExecuteQueryResult2(sql, parameters);
                LoadDatachung();
            }
        }

        private void gridView1_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
           
        }

        private void gridView1_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            GridView gridView = gridControl1.MainView as GridView;
            // Lấy thông tin về hàng và cột của ô đã thay đổi
            int rowHandle = e.RowHandle;
            string columnName = e.Column.FieldName; // Tên cột
            //Lấy current data row
            int ID = int.Parse(gridView.GetRowCellValue(rowHandle, gridView.Columns["ID"]).ToString());
            string Noidung = gridView.GetRowCellValue(rowHandle, gridView.Columns["Noidung"]).ToString();
            string TK = gridView.GetRowCellValue(rowHandle, gridView.Columns["Tk"]).ToString();
            string sql = "UPDATE tbMatdinhnganhang SET Noidung = ?, Tk = ?  WHERE ID = ?";
            OleDbParameter[] parameters = new OleDbParameter[]
 {
           new OleDbParameter("?",Noidung),
                 new OleDbParameter("?",TK),
                new OleDbParameter("?",ID)
 };
            int resl = ExecuteQueryResult2(sql, parameters);
        }
    }
}