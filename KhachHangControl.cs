using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
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
using Windows.UI.Xaml.Controls.Primitives;
using static SaovietTax.frmMain;

namespace SaovietTax
{
    public partial class KhachHangControl : UserControl
    {
        public event EventHandler<string> ItemSelected;

        public KhachHangControl()
        {
            InitializeComponent();
        }
        public string SearchText
        {
            get { return txtSearch.Text; } // txtSearch là TextBox trong UserControl
            set { txtSearch.Text = value; }
        }
        public class Item
        {
            public string Name { get; set; }
            public int Id { get; set; }

            public override string ToString()
            {
                return Name; // Hiển thị tên trong ComboBox
            }
        }
        public string Gtext { get; set; }
        private void Reset()
        {
            txtSohieu.Text = string.Empty;
            txtTenvattu.Text = string.Empty;
            txtDonvi.Text = string.Empty;
            txtGhichu.Text = string.Empty;
        }
        public List<HeThongTK> Suggestions
        {
            set
            {
                gridControl1.DataSource = value;
                if (value.Count == 0)
                {
                    txtTenvattu.Text = SearchText;
                }
                else
                {
                    txtTenvattu.Text = "";
                    txtGhichu.Text = string.Empty;
                    txtDonvi.Text = string.Empty;
                    txtSohieu.Text = string.Empty;
                }

            }
        }
        public void Gettext(string text)
        {
            Gtext = text;
        }
        public void UpdateSuggestions(List<HeThongTK> newSuggestions)
        {
            this.BringToFront();
            Suggestions = newSuggestions; 
            // Hiển thị UserControl nếu có gợi ý 
        }
        DataTable tbKhachhang = new DataTable();
        private void KhachHangControl_Load(object sender, EventArgs e)
        {

            this.Width = (int)(this.Parent.ClientSize.Width / 1.65);

            this.Location = new Point(0,0);
            gridControl1.Width= (int)(this.Parent.ClientSize.Width / 2.9);
            this.BringToFront();
            string query = @"SELECT * FROM PhanLoaiKhachHang ORDER BY TenPhanLoai";
            var dt = ExecuteQuery(query, null);
            if (dt != null && dt.Rows.Count > 0)
            {
                slKhachhang.Properties.Items.Clear(); // Xóa các mục cũ

                foreach (DataRow row in dt.Rows)
                {
                    // Thêm từng mục vào ComboBoxEdit
                    slKhachhang.Properties.Items.Add(new Item
                    {
                        Name = Helpers.ConvertVniToUnicode(row["SoHieu"].ToString()) + " - " + Helpers.ConvertVniToUnicode(row["TenPhanLoai"].ToString()),
                        Id = Convert.ToInt32(row["MaSo"])
                    });
                }
            }
            slKhachhang.Properties.NullText = "Chọn Tài khoản";
            slKhachhang.Properties.TextEditStyle = TextEditStyles.DisableTextEditor; // Ngăn người dùng nhập trực tiếp
                                                                                       //
            query = @"SELECT * FROM KhachHang";
            tbKhachhang = ExecuteQuery(query, null);

        }
        string dbPath = "";
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

        private void gridView1_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            string searchTerm = txtSearch.Text.ToString().ToLower();
            if (e.CellValue != null)
            {
                if (e.CellValue.ToString().ToLower().Contains(searchTerm))
                {
                    //  e.DefaultDraw();
                    e.Handled = true; // Đánh dấu rằng sự kiện đã được xử lý
                                      // Tìm vị trí của từ khóa
                    int startIndex = e.CellValue.ToString().ToLower().IndexOf(searchTerm);
                    if (startIndex >= 0)
                    {
                        int plusX = 0;
                        // Vẽ phần văn bản trước từ khóa
                        string beforeText = e.CellValue.ToString().Substring(0, startIndex);
                        e.Cache.DrawString(beforeText, e.Appearance.Font, Brushes.Black, e.Bounds);
                        if (!string.IsNullOrEmpty(beforeText))
                        {
                            plusX = 2;
                        }
                        // Vẽ từ khóa với màu khác

                        string highlightedText = e.CellValue.ToString().Substring(startIndex, searchTerm.Length);
                        e.Cache.DrawString(highlightedText, e.Appearance.Font, Brushes.Blue, e.Bounds.X + e.Cache.CalcTextSize(beforeText, e.Appearance.Font).Width + plusX, e.Bounds.Y);

                        // Vẽ phần văn bản sau từ khóa
                        string afterText = e.CellValue.ToString().Substring(startIndex + searchTerm.Length);
                        e.Cache.DrawString(afterText, e.Appearance.Font, Brushes.Black, e.Bounds.X + e.Cache.CalcTextSize(beforeText + highlightedText, e.Appearance.Font).Width + plusX, e.Bounds.Y);

                    }
                }
            }

        }

        private void gridView1_DoubleClick(object sender, EventArgs e)
        {
            string soHieu = gridView1.GetFocusedRowCellValue("SoHieu").ToString();
            ItemSelected?.Invoke(this, soHieu); // Gửi SoHieu
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void gridControl1_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void gridView1_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            // Lấy giá trị của một ô cụ thể trong hàng được nhấp
            var cellValue = gridView1.GetRowCellValue(e.RowHandle, "SoHieu"); // Thay "TênCột" bằng tên cột thực tế 
            var getkh = tbKhachhang.AsEnumerable()
                .FirstOrDefault(row => row.Field<string>("SoHieu") == cellValue.ToString());
            if (getkh != null)
            {
                txtSohieu.Text = getkh.Field<string>("SoHieu");
                txtTenvattu.Text = Helpers.ConvertVniToUnicode(getkh.Field<string>("Ten"));
                txtDonvi.Text = getkh.Field<string>("MST");
                txtGhichu.Text = getkh.Field<string>("Ghichu");
                txtMaSo.Text = getkh.Field<int>("MaSo").ToString();
                //select cho combobox
                int mapl = int.Parse(getkh["MaPhanLoai"].ToString()); 
                int idsl = 0;
                foreach (Item item in slKhachhang.Properties.Items)
                {
                    if (item.Id == mapl)
                    {
                        idsl = slKhachhang.Properties.Items.IndexOf(item);
                        break;
                    }
                }
                slKhachhang.SelectedIndex = idsl; // Chọn phần tử đầu tiên
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {

        }

        private void btnGhi_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSohieu.Text) || string.IsNullOrEmpty(txtTenvattu.Text))
            {
                XtraMessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                var selectedItem = slKhachhang.SelectedItem as Item;
                if(selectedItem==null)
                {
                    XtraMessageBox.Show("Vui lòng chọn Phân loại Khách hàng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;

                }
                if (txtMaSo.Text == "0" || string.IsNullOrEmpty(txtMaSo.Text))
                {
                    if(string.IsNullOrEmpty(txtGhichu.Text))
                    {
                        txtGhichu.Text = "xxx";
                    }
                          string   query = @"INSERT INTO KhachHang (MaPhanLoai, SoHieu, Ten, DiaChi, MST) VALUES (?, ?, ?, ?, ?)";
                          var  parameters = new OleDbParameter[]
                            {
                    new OleDbParameter("?", selectedItem.Id),
                    new OleDbParameter("?", txtSohieu.Text),
                    new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtTenvattu.Text)),
                    new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtGhichu.Text)),
                    new OleDbParameter("?", txtDonvi.Text)
                            };
                    try
                    {
                        int rowsAffected = ExecuteQueryResult(query, parameters);
                        query = @"SELECT * FROM KhachHang";
                        tbKhachhang = ExecuteQuery(query, null);
                        //
                        ItemSelected?.Invoke(this, txtSohieu.Text); // Gửi SoHieu
                                                                    //gridControl1.DataSource = tbKhachhang.AsEnumerable().Where(m => m["SoHieu"].ToString()== txtSohieu.Text).ToList();
                                                                    // gridControl1.RefreshDataSource();   
                        this.Hide();
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show("Lỗi khi cập nhật dữ liệu: " + ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                 
                    string query = @"UPDATE KhachHang SET MaPhanLoai=?, SoHieu=?, Ten=?, DiaChi=?, MST=? WHERE MaSo=?";
                    var parameters = new OleDbParameter[]
                      {
                new OleDbParameter("?", selectedItem.Id),
                new OleDbParameter("?", txtSohieu.Text),
                new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtTenvattu.Text)),
                new OleDbParameter("?", txtGhichu.Text),
                new OleDbParameter("?", txtDonvi.Text),
                new OleDbParameter("?", txtMaSo.Text)
                      };
                    try
                    {
                        int rowsAffected = ExecuteQueryResult(query, parameters);
                        query = @"SELECT * FROM KhachHang";
                        tbKhachhang = ExecuteQuery(query, null);
                        gridView1.SetRowCellValue(gridView1.FocusedRowHandle, "SoHieu", txtSohieu.Text);
                        gridView1.SetRowCellValue(gridView1.FocusedRowHandle, "Ten", txtTenvattu.Text);
                        gridControl1.RefreshDataSource();
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show("Lỗi khi cập nhật dữ liệu: " + ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
        }

        private void labelControl1_Click(object sender, EventArgs e)
        {

        }
    }
}