using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid;
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
using static SaovietTax.frmKhachhang;
using static SaovietTax.frmMain;

namespace SaovietTax
{
    public partial class VattuControl : DevExpress.XtraEditors.XtraUserControl
    {
        public event EventHandler<string> ItemSelected;
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
        public List<HangHoaTK> Suggestions
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
        public void UpdateSuggestions(List<HangHoaTK> newSuggestions)
        {
            this.BringToFront();
            Suggestions = newSuggestions;
            // Hiển thị UserControl nếu có gợi ý 
        }
        public VattuControl()
        {
            InitializeComponent();
        }
        public async Task<List<DTO.VatTu>> LoadDataVattuAsync()
        {
            // Hiển thị popup loading
            List<DTO.VatTu> lstVattu = new List<DTO.VatTu>();

            try
            {
                // 1. Lấy danh sách VatTu từ database

                var queryVatTu = @"SELECT * FROM Vattu";
                var ListVattu = await Task.Run(() => ExecuteQuery(queryVatTu, null));
                var queryMaphanloai = @"SELECT * FROM PhanLoaiVattu";
              //  ListPhanloaiVattu = await Task.Run(() => ExecuteQuery(queryMaphanloai, null));

                // 2. Chuyển đổi chuỗi VNI sang Unicode (nếu cần)
                foreach (DataRow item in ListVattu.Rows)
                {
                    item["TenVattu"] = Helpers.ConvertVniToUnicode(item["TenVattu"].ToString());
                    item["DonVi"] = Helpers.ConvertVniToUnicode(item["DonVi"].ToString());
                }

                // 3. Gom nhóm tất cả MaVatTu để query TonKho 1 lần duy nhất (Batch Query)
                var maVatTuList = ListVattu.Rows
                    .Cast<DataRow>()
                    .Select(row => int.Parse(row["MaSo"].ToString()))
                    .Distinct()
                    .ToList();

                // 4. Lấy dữ liệu TonKho theo danh sách MaVatTu đã gom nhóm
                var queryTonKhoBatch = @"SELECT * FROM TonKho WHERE MaVatTu IN (" +
                                       string.Join(",", maVatTuList) + ")";
                var allTonKho = await Task.Run(() => ExecuteQuery(queryTonKhoBatch, null));

                // 5. Chuyển dữ liệu TonKho thành Dictionary để truy cập nhanh bằng MaVatTu
                var tonKhoDict = allTonKho.Rows
                    .Cast<DataRow>()
                    .GroupBy(row => int.Parse(row["MaVatTu"].ToString()))
                    .ToDictionary(group => group.Key, group => group.First());

                // 6. Xử lý từng VatTu và ánh xạ dữ liệu TonKho tương ứng
                List<Task<DTO.VatTu>> vatTuTasks = new List<Task<DTO.VatTu>>();
                foreach (DataRow item in ListVattu.Rows)
                {
                    vatTuTasks.Add(Task.Run(() =>
                    {
                        var VatTu = new DTO.VatTu
                        {
                            MaSo = int.Parse(item["MaSo"].ToString()),
                            MaPhanLoai = int.Parse(item["MaPhanLoai"].ToString()),
                            TenVattu = item["TenVattu"].ToString(),
                            SoHieu = item["SoHieu"].ToString(),
                            DonVi = item["DonVi"].ToString(),
                            GhiChu = item["GhiChu"].ToString(),
                           // TenMaPhanLoai = ListPhanloaiVattu.AsEnumerable().Where(m => m["MaSo"].ToString() == item["MaPhanLoai"].ToString()).FirstOrDefault()["TenPhanLoai"].ToString()
                        };

                        // Kiểm tra và lấy dữ liệu từ TonKho (nếu có)
                        if (tonKhoDict.TryGetValue(VatTu.MaSo, out DataRow tonKhoRow))
                        {
                            int cnt = 12;
                            while (cnt > 0 && tonKhoRow["Luong_" + cnt].ToString() == "0")
                            {
                                cnt--;
                            }

                            // Lấy số lượng và thành tiền
                            var soluong = tonKhoRow["Luong_" + cnt] != DBNull.Value
                                ? double.Parse(tonKhoRow["Luong_" + cnt].ToString())
                                : 0;
                            VatTu.SoLuong = soluong;

                            var thanhtien = tonKhoRow["Tien_" + cnt] != DBNull.Value
                                ? double.Parse(tonKhoRow["Tien_" + cnt].ToString())
                                : 0;
                            VatTu.ThanhTien = thanhtien;

                            // Tính đơn giá nếu có dữ liệu
                            if (soluong != 0 && thanhtien != 0)
                            {
                                VatTu.Dongia = thanhtien / soluong;
                            }
                        }

                        return VatTu;
                    }));
                }

                // 7. Đợi tất cả các Task hoàn thành và thêm vào danh sách kết quả
                var vatTus = await Task.WhenAll(vatTuTasks);
                lstVattu.AddRange(vatTus);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi (có thể log hoặc hiển thị thông báo)
                Console.WriteLine($"Lỗi khi tải dữ liệu: {ex.Message}");
                throw; // Re-throw nếu cần thiết
            }
            finally
            {
                // Đóng popup loading chỉ khi mọi thứ đã hoàn tất
            }

            return lstVattu;
        }
        public List<DTO.VatTu> lstvt = new List<DTO.VatTu>();
        private async void VattuControl_Load(object sender, EventArgs e)
        {
            lstvt = await LoadDataVattuAsync();
         
            this.Width = (int)(this.Parent.ClientSize.Width * 0.7);
            this.Location = new Point(this.Parent.ClientSize.Width- (int)(this.Width), 0);
            this.BringToFront();
            panelControl2.Width = (int)(this.Width * 0.6)- panelControl1.Width/15;
            panelControl1.Width = (int)(this.Width * 0.4)-20;
            panelControl1.Location= new Point(this.Width- panelControl1.Width-20, panelControl1.Height/8);
            slKhachhang.Width = panelControl2.Width;
            txtSearch.Width = panelControl2.Width- btnSearch.Width-10;
            btnSearch.Location = new Point(txtSearch.Location.X + txtSearch.Width + 10, btnSearch.Location.Y);
            int  padding = 5;
            btnThem.Width = (int)(panelControl1.Width * 0.25)- padding;
            btnThem.Location = new Point(padding, btnThem.Location.Y);
            btnGhi.Width = (int)(panelControl1.Width * 0.25) - padding;
            btnGhi.Location = new Point(btnThem.Location.X + btnThem.Width + padding, btnGhi.Location.Y);
            btnXoa.Width = (int)(panelControl1.Width * 0.25) - padding;
            btnXoa.Location = new Point(btnGhi.Location.X + btnGhi.Width + padding, btnXoa.Location.Y);
            btnThoat.Width = (int)(panelControl1.Width * 0.25) - padding;
            btnThoat.Location = new Point(btnXoa.Location.X + btnXoa.Width + padding, btnThoat.Location.Y);
            simpleButton1.Location = new Point(this.Width - simpleButton1.Width - 10, 10);
            string query = @"SELECT * FROM PhanLoaiVattu";
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
            slKhachhang.Properties.NullText = "Chọn phân loại";
            slKhachhang.Properties.TextEditStyle = TextEditStyles.DisableTextEditor; // Ngăn người dùng nhập trực tiếp

            //
            query = @"SELECT * FROM Vattu";
            tbvattu = ExecuteQuery(query, null);
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
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Hide();    
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
                            plusX = 0;
                        }
                        // Vẽ từ khóa với màu khác

                        string highlightedText = e.CellValue.ToString().Substring(startIndex, searchTerm.Length);
                        e.Cache.DrawString(highlightedText, e.Appearance.Font, Brushes.Blue, e.Bounds.X + e.Cache.CalcTextSize(beforeText, e.Appearance.Font).Width + plusX, e.Bounds.Y);

                        // Vẽ phần văn bản sau từ khóa
                        string afterText = e.CellValue.ToString().Substring(startIndex + searchTerm.Length);
                        afterText= afterText.Substring(0, Math.Min(afterText.Length,50)); // Giới hạn độ dài của phần sau
                        e.Cache.DrawString(afterText, e.Appearance.Font, Brushes.Black, e.Bounds.X + e.Cache.CalcTextSize(beforeText + highlightedText, e.Appearance.Font).Width + plusX, e.Bounds.Y);

                    }
                }
            }
        }
        DataTable tbvattu = new DataTable();
        private void gridView1_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            // Lấy giá trị của một ô cụ thể trong hàng được nhấp
            var cellValue = gridView1.GetRowCellValue(e.RowHandle, "SoHieu"); // Thay "TênCột" bằng tên cột thực tế 
            var getkh = tbvattu.AsEnumerable()
                .FirstOrDefault(row => row.Field<string>("SoHieu") == cellValue.ToString());
            if (getkh != null)
            {
                txtSohieu.Text = getkh.Field<string>("SoHieu");
                txtTenvattu.Text = Helpers.ConvertVniToUnicode(getkh.Field<string>("TenVattu"));
                txtDonvi.Text = Helpers.ConvertVniToUnicode(getkh.Field<string>("DonVi"));
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

        private void gridView1_DoubleClick(object sender, EventArgs e)
        {
            string soHieu = gridView1.GetFocusedRowCellValue("SoHieu").ToString();
            ItemSelected?.Invoke(this, soHieu); // Gửi SoHieu
        }
        private void Reset()
        {
            txtSohieu.Text = string.Empty;
            txtTenvattu.Text = string.Empty;
            txtDonvi.Text = string.Empty;
            txtGhichu.Text = string.Empty; 
            txtMaSo.Text = "0";
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private async void btnGhi_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSohieu.Text) || string.IsNullOrEmpty(txtTenvattu.Text))
            {
                XtraMessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                var selectedItem = slKhachhang.SelectedItem as Item;
                if (selectedItem == null)
                {
                    XtraMessageBox.Show("Vui lòng chọn Phân loại vật tư!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;

                }
                if (txtMaSo.Text == "0" || string.IsNullOrEmpty(txtMaSo.Text))
                {
                    if (string.IsNullOrEmpty(txtGhichu.Text))
                    {
                        txtGhichu.Text = "xxx";
                    }
                    string query = @"INSERT INTO Vattu (MaPhanLoai, SoHieu, TenVattu, DonVi) VALUES (?, ?, ?, ?)";
                    var parameters = new OleDbParameter[]
                      {
                    new OleDbParameter("?", selectedItem.Id),
                    new OleDbParameter("?", txtSohieu.Text),
                    new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtTenvattu.Text)), 
                    new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtDonvi.Text))
                      };
                    try
                    {
                        int rowsAffected = ExecuteQueryResult(query, parameters);
                        
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

                    string query = @"UPDATE Vattu SET MaPhanLoai=?, SoHieu=?, TenVattu=?, DonVi=?,GhiChu=? WHERE MaSo=?";
                    var parameters = new OleDbParameter[]
                      {
                new OleDbParameter("?", selectedItem.Id),
                new OleDbParameter("?", txtSohieu.Text),
                new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtTenvattu.Text)), 
                new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtDonvi.Text)),
                new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtGhichu.Text)),
                new OleDbParameter("?", txtMaSo.Text)
                      };
                    try
                    {
                        int rowsAffected = ExecuteQueryResult(query, parameters); 
                        gridView1.SetRowCellValue(gridView1.FocusedRowHandle, "SoHieu", txtSohieu.Text);
                        gridView1.SetRowCellValue(gridView1.FocusedRowHandle, "TenVattu", txtTenvattu.Text);
                        gridControl1.RefreshDataSource();
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show("Lỗi khi cập nhật dữ liệu: " + ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                lstvt = await LoadDataVattuAsync();
            }
        }

        private void gridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string soHieu = gridView1.GetFocusedRowCellValue("SoHieu").ToString();
                ItemSelected?.Invoke(this, soHieu); // Gửi SoHieu
                e.Handled = true; // Đánh dấu rằng sự kiện đã được xử lý
            }
            if (e.KeyCode == Keys.Tab)
            {
                e.SuppressKeyPress = true; // Ngăn chặn hành động mặc định của phím Tab

                // Lấy chỉ số hàng hiện tại
                int currentRowHandle = gridView1.FocusedRowHandle;

                // Tính chỉ số hàng tiếp theo
                int nextRowHandle = currentRowHandle + 1;

                // Kiểm tra xem hàng tiếp theo có tồn tại không
                if (nextRowHandle < gridView1.RowCount)
                {
                    gridView1.FocusedRowHandle = nextRowHandle; // Di chuyển đến hàng tiếp theo
                }
                else
                {
                    // Nếu đã ở hàng cuối, có thể quay lại hàng đầu
                    gridView1.FocusedRowHandle = 0; // Quay lại hàng đầu
                }
            }
        }

        private void VattuControl_Enter(object sender, EventArgs e)
        {
            gridControl1.Focus(); // Đặt focus vào gridControl1 khi UserControl được chọn
            gridView1.FocusedRowHandle = 0; // Đặt hàng đầu tiên làm hàng được chọn 
            gridView1.SelectRow(0); // Chọn dòng đầu tiên
        }
    }
}
