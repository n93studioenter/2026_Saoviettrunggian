using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraWaitForm;
using SaovietTax.DTO;
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
using static SaovietTax.frmMain;

namespace SaovietTax
{
    public partial class frmHangHoa2026 : DevExpress.XtraEditors.XtraForm
    {
        public frmHangHoa2026()
        {
            InitializeComponent();
        }

        #region Variables
        public frmMain frmMain;
        public int Typeform { get; set; }
        public VatTu dtoVatTu { get; set; }
        public FileImportDetail2 currentrow { get; set; }
        public bool isChange = false;
        string dbPath = "";
        string sohieuvt = "";

        private class TonKho
        {
            public int MaSo { get; set; }
            public double SoLuong { get; set; }
            public double DonGia { get; set; }
            public double ThanhTien { get; set; }
        }

        public class Item
        {
            public string Name { get; set; }
            public int Id { get; set; }
            public override string ToString() => Name;
        }
        #endregion

        #region Search & Load Data

        bool firstload = true;
        private System.Threading.CancellationTokenSource _searchCts;

        private void frmHangHoa2026_Load(object sender, EventArgs e)
        {
            var query = @"SELECT * FROM PhanLoaiVattu ORDER BY TenPhanLoai";
            var dt = ExecuteQuery(query, null);

            if (dt != null && dt.Rows.Count > 0)
            {
                comboBoxEdit1.Properties.Items.Clear();
                comboBoxEdit1.Properties.Items.Add(new Item { Name = "Tất cả", Id = -1 });
                foreach (DataRow row in dt.Rows)
                {
                    comboBoxEdit1.Properties.Items.Add(new Item
                    {
                        Name = Helpers.ConvertVniToUnicode(row["SoHieu"].ToString()) + " - " + Helpers.ConvertVniToUnicode(row["TenPhanLoai"].ToString()),
                        Id = Convert.ToInt32(row["MaSo"])
                    });
                }
                txtSohieu.Text = dtoVatTu.SoHieu;
                txtTenvattu.Text = dtoVatTu.TenVattu;
                textEdit2.Text = dtoVatTu.TenVattu;
                txtDonvi.Text = dtoVatTu.DonVi;

                comboBoxEdit1.Properties.NullText = "Chọn Tài khoản";
                comboBoxEdit1.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;

                int idsl = 0;
                if (comboBoxEdit1.Properties.Items.Count > 0)
                {
                    foreach (Item item in comboBoxEdit1.Properties.Items)
                    {
                        if (item.Id == frmMain.currentselectId)
                        {
                            idsl = comboBoxEdit1.Properties.Items.IndexOf(item);
                            break;
                        }
                    }
                    comboBoxEdit1.SelectedIndex = idsl;
                    var selectedItem = comboBoxEdit1.SelectedItem as Item;
                    if (selectedItem != null) LoadData(selectedItem.Id, txtSearch.Text);
                    firstload = false;
                }
            }
            else
            {
                comboBoxEdit1.Properties.Items.Clear();
                comboBoxEdit1.Properties.NullText = "Không có tài khoản nào";
            }

            // Focus vào dòng hàng tương ứng
            DevExpress.XtraGrid.Views.Grid.GridView view = gridControl1.MainView as DevExpress.XtraGrid.Views.Grid.GridView;
            if (view != null)
            {
                for (int i = 0; i < view.RowCount; i++)
                {
                    if (view.GetRowCellValue(i, "SoHieu")?.ToString().ToLower() == txtSohieu.Text.ToLower())
                    {
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            if (gridView1.RowCount > i)
                            {
                                gridView1.FocusedRowHandle = i;
                                gridView1.MakeRowVisible(i);
                                gridView1.SelectRow(i);
                                textEdit1.Text = gridView1.GetRowCellValue(i, "TenVattu")?.ToString();
                                sohieuvt = gridView1.GetRowCellValue(i, "SoHieu")?.ToString();
                            }
                        });
                        return;
                    }
                }
            }
        }

        private void comboBoxEdit1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEdit1.SelectedItem != null && !firstload)
            {
                progressPanel1.Visible = true;
                progressPanel1.Caption = "Đang tải dữ liệu...";
                Application.DoEvents();

                var selectedItem = comboBoxEdit1.SelectedItem as Item;
                if (selectedItem != null)
                {
                    LoadData(selectedItem.Id, txtSearch.Text);
                }
                progressPanel1.Visible = false;
            }
        }

        private async void txtSearch_EditValueChanged_1(object sender, EventArgs e)
        {
            _searchCts?.Cancel();
            _searchCts = new System.Threading.CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                await Task.Delay(500, token);
                token.ThrowIfCancellationRequested();

                var selectedItem = comboBoxEdit1.SelectedItem as Item;
                if (selectedItem != null)
                {
                    LoadData(selectedItem.Id, txtSearch.Text);
                }
            }
            catch (TaskCanceledException) { }
        }

        private void LoadData(int Maso, string keysearch)
        {
            // Lấy danh sách gốc từ frmMain
            var mylstvt = frmMain.lstvt.Where(m => (m.MaPhanLoai == Maso || Maso == -1)
                && (string.IsNullOrEmpty(keysearch)
                    || (Helpers.RemoveVietnameseDiacritics(m.TenVattu).ToLower().Contains(Helpers.RemoveVietnameseDiacritics(keysearch).ToLower()))
                    || m.SoHieu.ToLower().Contains(keysearch.ToLower())));

            // Tính điểm tương đồng cho từng sản phẩm
            foreach (var vt in mylstvt)
            {
                var productSimilarity = CompareProduct(vt.TenVattu, frmMain.TenVTMain);
                vt.Real = productSimilarity;
            }

            // Sắp xếp theo % giống và % tương đồng
            gridControl1.DataSource = mylstvt.OrderByDescending(m => m.Percent).OrderByDescending(m => m.Real).ToList();
            GridStripRow(gridView1);
        }

        #endregion

        #region Grid & Selection

        public void GridStripRow(DevExpress.XtraGrid.Views.Grid.GridView gridView)
        {
            if (gridView != null)
            {
                gridView.OptionsView.EnableAppearanceEvenRow = true;
                gridView.OptionsView.EnableAppearanceOddRow = true;
                gridView.Appearance.EvenRow.BackColor = Color.FromArgb(168, 255, 253);
                gridView.Appearance.EvenRow.ForeColor = Color.Black;
                gridView.Appearance.OddRow.BackColor = Color.White;
                gridView.Appearance.OddRow.ForeColor = Color.Black;
            }
        }

        private void gridView1_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            int rowHandle = e.RowHandle;
            var value = gridView1.GetRowCellValue(rowHandle, "SoHieu")?.ToString();
            txtSohieu.Text = value;
            sohieuvt = value;
            txtTenvattu.Text = gridView1.GetRowCellValue(rowHandle, "TenVattu")?.ToString();
            textEdit1.Text = gridView1.GetRowCellValue(rowHandle, "TenVattu")?.ToString();
            txtDonvi.Text = gridView1.GetRowCellValue(rowHandle, "DonVi")?.ToString();
            txtGhichu.Text = gridView1.GetRowCellValue(rowHandle, "GhiChu")?.ToString();
            txtMaSo.Text = gridView1.GetRowCellValue(rowHandle, "MaSo")?.ToString();

            string query = @"SELECT * FROM TonKho WHERE MaVatTu = ?";
            var parameterss = new OleDbParameter[] { new OleDbParameter("?", txtMaSo.Text) };
            var kq = ExecuteQuery(query, parameterss);

            List<TonKho> lstTonkho = new List<TonKho>();
            if (kq.Rows.Count > 0)
            {
                try
                {
                    TonKho tk = new TonKho();
                    int cnt = 12;
                    while (kq.Rows[0]["Luong_" + cnt] != DBNull.Value && kq.Rows[0]["Luong_" + cnt].ToString() == "0")
                    {
                        cnt++;
                    }
                    tk.SoLuong = kq.Rows[0]["Luong_" + cnt] != DBNull.Value ? double.Parse(kq.Rows[0]["Luong_" + cnt].ToString()) : 0;
                    tk.ThanhTien = kq.Rows[0]["Tien_" + cnt] != DBNull.Value ? double.Parse(kq.Rows[0]["Tien_" + cnt].ToString()) : 0;
                    if (tk.SoLuong != 0 && tk.ThanhTien != 0)
                    {
                        tk.DonGia = Math.Round(double.Parse(kq.Rows[0]["Tien_" + cnt].ToString()) / tk.SoLuong, 2);
                        lstTonkho.Add(tk);
                    }
                }
                catch { }
            }
            gridControl2.DataSource = lstTonkho;
        }

        #endregion

        #region AI/So khớp chuỗi thông minh

        public static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            string result = input.ToLowerInvariant();
            result = result.Normalize(System.Text.NormalizationForm.FormD);

            var sb = new System.Text.StringBuilder();
            foreach (char c in result)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            result = sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
            result = result.Replace("đ", "d").Replace("Đ", "d");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"[^a-z0-9\s\*x]", "");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\s+", " ").Trim();

            return result;
        }

        public static double CompareProduct(string p1, string p2)
        {
            string n1 = Normalize(p1);
            string n2 = Normalize(p2);

            var nums1 = ExtractNumbersAndDimensions(n1);
            var nums2 = ExtractNumbersAndDimensions(n2);

            if (nums1.Count > 0 && nums2.Count > 0)
            {
                int commonNums = nums1.Intersect(nums2).Count();
                if (commonNums == 0) return 0;
            }

            var set1 = GetImportantWords(n1);
            var set2 = GetImportantWords(n2);

            // Tránh nhầm lẫn hương vị (Cam vs Olong)
            string[] flavors = { "cam", "olong", "chanh", "dau", "dao", "vi", "sua", "dua", "xoai" };
            var flavors1 = set1.Intersect(flavors).ToList();
            var flavors2 = set2.Intersect(flavors).ToList();
            if (flavors1.Count > 0 && flavors2.Count > 0 && !flavors1.Intersect(flavors2).Any())
            {
                return 0;
            }

            foreach (var n in nums1) set1.Add(n);
            foreach (var n in nums2) set2.Add(n);

            if (set1.Count == 0 || set2.Count == 0) return 0;

            int common = set1.Intersect(set2).Count();
            int total = set1.Union(set2).Count();

            return total == 0 ? 0 : (double)common / total;
        }

        private static HashSet<string> ExtractNumbersAndDimensions(string input)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var matches = System.Text.RegularExpressions.Regex.Matches(input, @"\b\d+([.,*xX]\d+)?(cm|m|mm|tang|inox)?\b");
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                string val = m.Value.Trim().ToLower();
                if (val.Length > 2 || val.Contains("*") || val.Contains("x") || val.Contains("tang") || val.Contains("inox"))
                {
                    result.Add(val);
                }
            }
            return result;
        }

        private static HashSet<string> GetImportantWords(string input)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in BrandAliases)
            {
                foreach (var alias in pair.Value)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(input, $@"\b{System.Text.RegularExpressions.Regex.Escape(alias)}\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        result.Add(pair.Key);
                        break;
                    }
                }
            }

            foreach (var word in ImportantWords)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(input, $@"\b{System.Text.RegularExpressions.Regex.Escape(word)}\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    result.Add(word);
                }
            }

            return result;
        }

        // 1. Từ điển đồng nghĩa
        public static readonly Dictionary<string, string[]> BrandAliases = new Dictionary<string, string[]>()
        {
            { "chen", new[] { "bat", "to", "chén", "bát", "tô" } },
            { "ly", new[] { "coc", "ly", "cốc", "tach", "tách" } },
            { "muong", new[] { "muong", "muỗng", "thia", "thìa" } },
            { "dia", new[] { "dia", "dĩa" } },
            { "dao", new[] { "dao", "thot", "thớt" } },
            { "inox", new[] { "inox", "thep khong gi", "thép không gỉ", "inoc", "suot" } },
            { "nhua", new[] { "nhua", "nhựa", "composite" } },
            { "thuy tinh", new[] { "thuy tinh", "thủy tinh", "kinh", "kính" } },
            { "2 tang", new[] { "2 tang", "2 tầng", "2t", "hai tang" } },
            { "1 tang", new[] { "1 tang", "1 tầng", "1t", "mot tang" } },
            { "cam", new[] { "cam", "orange" } },
            { "olong", new[] { "olong", "o long", "ô long", "tra olong" } }
        };

        // 2. Từ khóa quan trọng
        public static readonly string[] ImportantWords = new string[]
        {
            "ke", "tu", "ban", "ghe", "giuong", "quat", "bep", "noi", "chao", "am", "binh", "hop",
            "canh", "ngan", "gia vi", "bat", "chen", "ly", "muong", "dia", "dao",
            "inox", "nhua", "thuy tinh", "go", "cao su", "nhom", "sat",
            "304", "201", "430", "dien", "co", "gas", "dung", "treo", "in", "chu",
            "tang", "cm", "m", "lit", "kg", "twister", "tra", "nuoc", "sua", "cam", "olong", "chanh", "dau", "dao", "vi", "pet", "chai", "thung", "loc"
        };

        #endregion

        #region DB Helper

        private DataTable ExecuteQuery(string query, params OleDbParameter[] parameters)
        {
            DataTable dataTable = new DataTable();
            string appPath = Assembly.GetExecutingAssembly().Location;
            string directoryPath = Path.GetDirectoryName(appPath);
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));
            string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");

            try
            {
                dbPath = File.ReadAllText(filePaths);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi đọc file: " + ex.Message);
            }

            string password = "1@35^7*9)1";
            string connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        if (parameters != null) command.Parameters.AddRange(parameters);
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
            return dataTable;
        }

        private int ExecuteQueryResult(string query, params OleDbParameter[] parameters)
        {
            string password = "1@35^7*9)1";
            string connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    if (parameters != null) command.Parameters.AddRange(parameters);
                    return command.ExecuteNonQuery();
                }
            }
        }

        #endregion

        private void gridControl1_DoubleClick(object sender, EventArgs e)
        {
            DevExpress.XtraGrid.Views.Grid.GridView gridView = gridControl1.MainView as DevExpress.XtraGrid.Views.Grid.GridView;
            var hitInfo = gridView.CalcHitInfo(gridView.GridControl.PointToClient(MousePosition));


            // Kiểm tra nếu nhấp vào một ô
            if (hitInfo.InRowCell)
            {
                int columnIndex = hitInfo.Column.VisibleIndex; // Chỉ số cột
                FileImportDetail rowData = gridView.GetRow(hitInfo.RowHandle) as FileImportDetail;
                // Lấy giá trị trong ô đã nhấp
                var hiddenValue = gridView.GetRowCellValue(hitInfo.RowHandle, gridView.Columns["SoHieu"]);
                var hiddenValue2 = gridView.GetRowCellValue(hitInfo.RowHandle, gridView.Columns["DonVi"]);
                var hiddenValue3 = gridView.GetRowCellValue(hitInfo.RowHandle, gridView.Columns["TenVattu"]);
                frmMain.hiddenValue = hiddenValue.ToString();
                frmMain.hiddenValue2 = hiddenValue2.ToString();
                frmMain.hiddenValue3 = hiddenValue3.ToString();
                double newDongia = 0;
                double newsl = 0;
                //Kiểm tra có khác đơn vị tính mà cùng tên không
                bool changePrice = false;
                if (currentrow != null)
                {
                    //Kiem tra them setting
                    string qr = @"select * FROM tbSetting";

                    var kq = ExecuteQuery(qr);
                    int rs = 0;
                    if (kq.Rows.Count > 0 &&
                        kq.Rows[0]["QuyDoiDVT"] != DBNull.Value &&
                        !string.IsNullOrWhiteSpace(kq.Rows[0]["QuyDoiDVT"].ToString()))
                    {
                        int.TryParse(kq.Rows[0]["QuyDoiDVT"].ToString(), out rs);
                    }

                    if (currentrow.Ten == hiddenValue3.ToString() && currentrow.DVT != hiddenValue2 && rs == 1)
                    {
                        string value = XtraInputBox.Show(
                       "Nhập dữ liệu:",
                       "Nhập số lượng quy đổi",
                       ""
                    );

                        if (value != null)
                        {
                            if (double.Parse(value) > 0)
                            {
                                changePrice = true;
                                double newTT = currentrow.TTien / currentrow.Soluong;
                                newDongia = newTT / double.Parse(value);
                                newsl = double.Parse(value) * currentrow.Soluong;
                                var query = @"UPDATE tbimportdetail SET  SoHieu=?,DVT=?,Ten=?,SoLuong=?,DonGia=? where ID=? ";
                                var parameters = new OleDbParameter[]
                                {
                                        new OleDbParameter("?", hiddenValue),
                                        new OleDbParameter("?",  Helpers.ConvertUnicodeToVni(currentrow.DVT.ToString())) ,
                                        new OleDbParameter("?", Helpers.ConvertUnicodeToVni(hiddenValue3.ToString())),
                                         new OleDbParameter("?", newsl),
                                          new OleDbParameter("?", newDongia),
                                        new OleDbParameter("?", currentrow.ID)
                                };
                                var rowsAffected = ExecuteQueryResult(query, parameters);
                            }
                            else
                            {
                                changePrice = true;
                                double newTT = currentrow.TTien / currentrow.Soluong;
                                newsl = currentrow.Soluong / double.Parse(value) * (-1);
                                newDongia = currentrow.TTien / newsl;

                                var query = @"UPDATE tbimportdetail SET  SoHieu=?,DVT=?,Ten=?,SoLuong=?,DonGia=? where ID=? ";
                                var parameters = new OleDbParameter[]
                                {
                                        new OleDbParameter("?", hiddenValue),
                                        new OleDbParameter("?",  Helpers.ConvertUnicodeToVni(currentrow.DVT.ToString())) ,
                                        new OleDbParameter("?", Helpers.ConvertUnicodeToVni(hiddenValue3.ToString())),
                                         new OleDbParameter("?", newsl),
                                          new OleDbParameter("?", newDongia),
                                        new OleDbParameter("?", currentrow.ID)
                                };
                                var rowsAffected = ExecuteQueryResult(query, parameters);
                            }

                        }
                        else
                        {
                            var query = @"UPDATE tbimportdetail SET  SoHieu=?,DVT=?,Ten=? where ID=? ";
                            var parameters = new OleDbParameter[]
                            {
                                        new OleDbParameter("?", hiddenValue),
                                        new OleDbParameter("?",  Helpers.ConvertUnicodeToVni(hiddenValue2.ToString())) ,
                                        new OleDbParameter("?", Helpers.ConvertUnicodeToVni(hiddenValue3.ToString())),
                                        new OleDbParameter("?", currentrow.ID)
                            };
                            var rowsAffected = ExecuteQueryResult(query, parameters);
                        }
                    }
                    else
                    {
                        var query = @"UPDATE tbimportdetail SET  SoHieu=?,DVT=?,Ten=? where ID=? ";
                        var parameters = new OleDbParameter[]
                        {
                                        new OleDbParameter("?", hiddenValue),
                                        new OleDbParameter("?",  Helpers.ConvertUnicodeToVni(hiddenValue2.ToString())) ,
                                        new OleDbParameter("?", Helpers.ConvertUnicodeToVni(hiddenValue3.ToString())),
                                        new OleDbParameter("?", currentrow.ID)
                        };
                        var rowsAffected = ExecuteQueryResult(query, parameters);
                    }
                    this.Close();
                }


                isChange = true;
               
            }
        }
    }
}