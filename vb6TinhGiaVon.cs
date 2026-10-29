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
using Tensorflow;
using static SaovietTax.frmMain;

namespace SaovietTax
{
    public partial class vb6TinhGiaVon : DevExpress.XtraEditors.XtraForm
    {
        public vb6TinhGiaVon()
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
        string dbPath = "";
        string connectionString = "";
        int finalLock = 0;
        public void ResetTonkho(DataTable tbTonKho,int finalLock)
        {
            if (tbTonKho != null && tbTonKho.Rows.Count > 0)
            {
                DataRow row = tbTonKho.Rows[0];
                for (int i = finalLock+1; i <= 12; i++)
                {
                    string tiennhapColumn = $"Tien_Nhap_{i}";
                    string luongnhapColumn = $"Luong_Nhap_{i}";
                    string tienxuatColumn = $"Tien_Xuat_{i}";
                    string luongxuatColumn = $"Luong_Xuat_{i}";
                    string luongColumn = $"Luong_{i}";
                    string tienColumn = $"Tien_{i}";
                    if (tbTonKho.Columns.Contains(tiennhapColumn))
                        row[tiennhapColumn] = 0;
                    if (tbTonKho.Columns.Contains(luongnhapColumn))
                        row[luongnhapColumn] = 0;
                    if (tbTonKho.Columns.Contains(tienxuatColumn))
                        row[tienxuatColumn] = 0;
                    if (tbTonKho.Columns.Contains(luongxuatColumn))
                        row[luongxuatColumn] = 0;
                    if (tbTonKho.Columns.Contains(luongColumn))
                        row[luongColumn] = 0;
                    if (tbTonKho.Columns.Contains(tienColumn))
                        row[tienColumn] = 0;
                }
            }
        }
        // BẮT BUỘC thêm async vào đây để dùng Task.Run
        private async void simpleButton1_Clickold(object sender, EventArgs e)
        {
            // Vô hiệu hóa nút bấm để tránh bấm 2 lần
            simpleButton1.Enabled = false;

            await Task.Run(() =>
            {
                try
                {
                    string appPath = Assembly.GetExecutingAssembly().Location;
                    string directoryPath = Path.GetDirectoryName(appPath);
                    string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));
                    string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");

                    try
                    {
                        string content = File.ReadAllText(filePaths);
                        dbPath = content;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Lỗi khi đọc file: " + ex.Message);
                        return;
                    }

                    string password = "1@35^7*9)1";
                    connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";

                    // --- LẤY THÔNG TIN KHÓA SỔ ---
                    string qr = "SELECT * FROM License";
                    var tbLicense = ExecuteQuery(qr, null);

                    if (tbLicense.Rows.Count > 0)
                    {
                        var row = tbLicense.Rows[0];
                        finalLock = 0;
                        for (int i = 12; i >= 1; i--)
                        {
                            string columnName = "Lock" + i;
                            if (tbLicense.Columns.Contains(columnName))
                            {
                                if (row[columnName] != DBNull.Value && row[columnName].ToString() == "1")
                                {
                                    finalLock = i;
                                    break;
                                }
                            }
                        }
                    }

                    // --- LẤY CHỨNG TỪ ---
                    // Lưu ý: Đã thêm ORDER BY MaLoai ASC để Nhập (1) luôn đứng trước Xuất (8)
                    var qrTimct = @"SELECT * FROM ChungTu 
                            WHERE NgayCT >= @NgayBD AND NgayCT < @NgayKT_NextDay
                            ORDER BY NgayCT ASC, MaLoai ASC, MaSo ASC";

                    var parameterss = new OleDbParameter[]
                    {
                new OleDbParameter("@NgayBD", dtTuNgay.DateTime.Date),
                new OleDbParameter("@NgayKT_NextDay", dtDenngay.DateTime.Date.AddDays(1))
                    };

                    var tbchungtu = ExecuteQuery(qrTimct, parameterss);

                    var vattuGroupby = tbchungtu.AsEnumerable()
                                                .GroupBy(m => m["MaVattu"].ToString())
                                                .ToList();

                    var danhSachMaVattu = vattuGroupby.Select(g => g.Key).ToList();

                    string qrvt = "SELECT * FROM Vattu";
                    var tbVattu = ExecuteQuery(qrvt, null);

                    var queryVattu = tbVattu.AsEnumerable()
                        .Where(r => danhSachMaVattu.Contains(r["MaSo"].ToString()));

                    string soHieuNhap = txtSoHieu.Text.Trim();
                    if (!string.IsNullOrEmpty(soHieuNhap))
                    {
                        queryVattu = queryVattu.Where(r => r.Field<string>("SoHieu") != null &&
                                                           r.Field<string>("SoHieu").ToLower().Contains(soHieuNhap.ToLower()));
                    }

                    var tbVattuFiltered = queryVattu.ToList();

                    // --- VÒNG LẶP TÍNH GIÁ VỐN ---
                    foreach (DataRow row in tbVattuFiltered)
                    {
                        string tenVT = row["TenVatTu"].ToString();
                        string maVatTu = row["MaSo"].ToString();

                        this.Invoke((MethodInvoker)delegate
                        {
                            progressPanel1.Caption = $"Đang tính giá vốn cho vật tư: {tenVT} ({maVatTu})";
                        });

                        string qrtk = "SELECT * FROM TonKho where MaVatTu = @MaVatTu";
                        var tbTonKho = ExecuteQuery(qrtk, new OleDbParameter("@MaVatTu", maVatTu));

                        if (tbTonKho != null && tbTonKho.Rows.Count > 0)
                        {
                            double tien_lock = Convert.ToDouble(tbTonKho.Rows[0][$"Tien_{finalLock}"]);
                            double luong_lock = Convert.ToDouble(tbTonKho.Rows[0][$"Luong_{finalLock}"]);

                            double luongTon = luong_lock;
                            double tienTon = tien_lock;

                            var chungTuCuaVatTu = vattuGroupby.FirstOrDefault(g => g.Key == maVatTu)?.CopyToDataTable();
                            if (chungTuCuaVatTu == null) continue;

                            // Bỏ qua các phiếu GV đã tính trước đó
                            chungTuCuaVatTu = chungTuCuaVatTu.AsEnumerable()
                                                .Where(m => !m["SoHieu"].ToString().Contains("GV"))
                                                .CopyToDataTable();

                            foreach (DataRow ctRow in chungTuCuaVatTu.Rows)
                            {
                                string soHieuCT = ctRow["SoHieu"].ToString();
                                double MaLoai = Convert.ToDouble(ctRow["MaLoai"]);

                                // Xử lý Nhập kho
                                if (MaLoai == 1)
                                {
                                    double SoPS2No = Convert.ToDouble(ctRow["SoPS2No"]);
                                    double SoPS = Convert.ToDouble(ctRow["SoPS"]);

                                    luongTon += SoPS2No;
                                    tienTon += SoPS;
                                }
                                // Xử lý Xuất kho
                                else if (MaLoai == 8)
                                {
                                    if (luongTon <= 0) continue; // Tránh lỗi chia cho 0

                                    // 1. Tính giá bình quân (KHÔNG LÀM TRÒN Ở BƯỚC NÀY)
                                    double GV = tienTon / luongTon;

                                    double SoPS2Co = Convert.ToDouble(ctRow["SoPS2Co"]);

                                    // 2. Tính thành tiền (LÀM TRÒN ĐẾN HÀNG ĐƠN VỊ Ở BƯỚC CUỐI)
                                    double SoPS = Math.Round(GV * SoPS2Co, 0);

                                    // 3. Cập nhật lại tồn kho
                                    luongTon -= SoPS2Co;
                                    tienTon -= SoPS;

                                    // 4. Cập nhật Database
                                    string updateQuery = "UPDATE ChungTu SET SoPS = ? WHERE sohieu = ? AND CT_ID LIKE ?";
                                    var parameters = new OleDbParameter[]
                                    {
                                new OleDbParameter("?", SoPS),
                                new OleDbParameter("?", $"{soHieuCT}GV"),
                                new OleDbParameter("?", $"%{ctRow["MaSo"]}%")
                                    };

                                    ExecuteQueryResult(updateQuery, parameters);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        XtraMessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                }
            });

            simpleButton1.Enabled = true;
            progressPanel1.Caption = "Đã tính xong!";
            XtraMessageBox.Show("Đã tính giá vốn xong!");
        }
        // BẮT BUỘC thêm async để dùng Task.Run
        private async void simpleButton1_Click(object sender, EventArgs e)
        {
            // Vô hiệu hóa nút bấm để tránh bấm 2 lần
            simpleButton1.Enabled = false;
            progressPanel1.Caption = "Đang chuẩn bị...";

            await Task.Run(() =>
            {
                try
                {
                    // --- 1. LẤY ĐƯỜNG DẪN DB ---
                    string appPath = Assembly.GetExecutingAssembly().Location;
                    string directoryPath = Path.GetDirectoryName(appPath);
                    string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));
                    string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");

                    string dbPath = "";
                    try
                    {
                        dbPath = File.ReadAllText(filePaths);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Lỗi khi đọc file: " + ex.Message);
                        return; // Nếu không đọc được file thì dừng code
                    }

                    string password = "1@35^7*9)1";
                    connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";

                    // --- 2. LẤY THÔNG TIN KHÓA SỔ (LICENSE) ---
                    int finalLock = 0;
                    string qr = "SELECT * FROM License";
                    var tbLicense = ExecuteQuery(qr, null);

                    if (tbLicense.Rows.Count > 0)
                    {
                        var rowLicense = tbLicense.Rows[0];
                        for (int i = 12; i >= 1; i--)
                        {
                            string columnName = "Lock" + i;
                            if (tbLicense.Columns.Contains(columnName))
                            {
                                if (rowLicense[columnName] != DBNull.Value && rowLicense[columnName].ToString() == "1")
                                {
                                    finalLock = i;
                                    break;
                                }
                            }
                        }
                    }

                    // --- 3. XÁC ĐỊNH NGÀY BẮT ĐẦU ĐỂ TÍNH GIÁ VỐN (TỪ ĐẦU) ---
                    DateTime ngayBatDauTinhToan;
                    int namTinhToan = dtDenngay.DateTime.Year;

                    if (finalLock > 0)
                    {
                        if (finalLock == 12)
                        {
                            ngayBatDauTinhToan = new DateTime(namTinhToan, 1, 1);
                        }
                        else
                        {
                            ngayBatDauTinhToan = new DateTime(namTinhToan, finalLock + 1, 1);
                        }
                    }
                    else
                    {
                        ngayBatDauTinhToan = new DateTime(namTinhToan, 1, 1);
                    }

                    // --- 4. LỌC DANH SÁCH VẬT TƯ CẦN TÍNH ---
                    var qrLocVatTu = @"SELECT DISTINCT MaVattu FROM ChungTu 
                        WHERE NgayCT >= @NgayBD_Loc AND NgayCT < @NgayKT_Loc and MaVattu <>0 ";
                    var paramsLoc = new OleDbParameter[]
                    {
                new OleDbParameter("@NgayBD_Loc", dtTuNgay.DateTime.Date),
                new OleDbParameter("@NgayKT_Loc", dtDenngay.DateTime.Date.AddDays(1))
                    };
                    var tbLocVatTu = ExecuteQuery(qrLocVatTu, paramsLoc);

                    var danhSachMaVattuCanTinh = tbLocVatTu.AsEnumerable()
                                                .Select(r => r["MaVattu"].ToString())
                                                .ToList();

                    if (!danhSachMaVattuCanTinh.Any())
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            XtraMessageBox.Show("Không có vật tư nào phát sinh trong khoảng thời gian này!");
                        });
                        return;
                    }

                    // --- 5. LẤY TOÀN BỘ CHỨNG TỪ TỪ ĐẦU ĐẾN CUỐI ĐỂ TÍNH TOÁN ---
                    var qrTimct = @"SELECT * FROM ChungTu 
                     WHERE NgayCT >= @NgayBD_Tinh AND NgayCT < @NgayKT_NextDay and MaVattu <> 0
                     ORDER BY NgayCT ASC, MaLoai ASC, MaSo ASC";
                    var paramsTinh = new OleDbParameter[]
                    {
                new OleDbParameter("@NgayBD_Tinh", ngayBatDauTinhToan),
                new OleDbParameter("@NgayKT_NextDay", dtDenngay.DateTime.Date.AddDays(1))
                    };
                    var tbchungtu = ExecuteQuery(qrTimct, paramsTinh);

                    //Lay dnah sach chung tu theo ma vat tu
                    var vattuGroupby = tbchungtu.AsEnumerable()
                                                .GroupBy(m => m["MaVattu"].ToString())
                                                .ToList();

                    // --- 6. LẤY DANH MỤC VẬT TƯ VÀ LỌC THEO TEXTBOX ---
                    string qrvt = "SELECT * FROM Vattu";
                    var tbVattu = ExecuteQuery(qrvt, null);

                    var queryVattu = tbVattu.AsEnumerable()
                        .Where(r => danhSachMaVattuCanTinh.Contains(r["MaSo"].ToString()));

                    string soHieuNhap = txtSoHieu.Text.Trim();
                    if (!string.IsNullOrEmpty(soHieuNhap))
                    {
                        queryVattu = queryVattu.Where(r => r.Field<string>("SoHieu") != null &&
                                                           r.Field<string>("SoHieu").ToLower().Contains(soHieuNhap.ToLower()));
                    }

                    var tbVattuFiltered = queryVattu.ToList();

                    // --- TÍNH TOÁN TỔNG SỐ DÒNG ĐỂ CHIA % ---
                    int totalVatTu = tbVattuFiltered.Count;
                    int currentIndex = 0;

                    // --- 7. VÒNG LẶP TÍNH GIÁ VỐN ---
                    foreach (DataRow row in tbVattuFiltered)
                    {
                      
                        string maVatTu = row["MaSo"].ToString();
                        string tenVT = row["TenVatTu"].ToString();
                        currentIndex++;

                        // Cập nhật tiến trình (Giai đoạn tính toán chiếm 0% -> 90%)
                        int progressPercent = (int)((double)currentIndex / totalVatTu * 90);
                        this.Invoke((MethodInvoker)delegate
                        {
                            progressPanel1.Caption = $"Đang tính ({progressPercent}%) - {currentIndex}/{totalVatTu}: {tenVT} ({maVatTu})";
                        });

                        // Lấy tồn kho gốc
                        string qrtk = "SELECT * FROM TonKho where MaVatTu = @MaVatTu";
                        var tbTonKho = ExecuteQuery(qrtk, new OleDbParameter("@MaVatTu", maVatTu));
                        ResetTonkho(tbTonKho, finalLock);
                        if (tbTonKho != null && tbTonKho.Rows.Count > 0)
                        {
                            //double tienTon = Convert.ToDouble(tbTonKho.Rows[0][$"Tien_{finalLock}"]);
                            //double luongTon = Convert.ToDouble(tbTonKho.Rows[0][$"Luong_{finalLock}"]);
                            double tienTon = Convert.ToDouble(tbTonKho.Rows[0][$"Tien_{finalLock}"]);
                            double luongTon = Convert.ToDouble(tbTonKho.Rows[0][$"Luong_{finalLock}"]);

                            var chungTuCuaVatTu = vattuGroupby.FirstOrDefault(g => g.Key == maVatTu)?.CopyToDataTable();
                            if (chungTuCuaVatTu == null) continue;

                            // Bỏ qua phiếu GV cũ
                            chungTuCuaVatTu = chungTuCuaVatTu.AsEnumerable()
                                                .Where(m => !m["SoHieu"].ToString().Contains("GV"))
                                                .CopyToDataTable();

                            DataRow tbrow = tbTonKho.Rows[0];
                            foreach (DataRow ctRow in chungTuCuaVatTu.Rows)
                            {
                                int thangct= ctRow["ThangCT"] != DBNull.Value ? Convert.ToInt32(ctRow["ThangCT"]) : 0;

                                string soHieuCT = ctRow["SoHieu"].ToString();
                                if (soHieuCT == "708")
                                {
                                    int a = 10; 
                                }
                                double MaLoai = Convert.ToDouble(ctRow["MaLoai"]);

                                // Xử lý Nhập kho
                                if (MaLoai == 1)
                                {
                                    double SoPS2No = Convert.ToDouble(ctRow["SoPS2No"]);
                                    double SoPS = Convert.ToDouble(ctRow["SoPS"]);
                                    luongTon += SoPS2No;
                                    tienTon += SoPS;
                                    tbrow["Luong_Nhap_" + thangct] = Convert.ToDouble(tbrow["Luong_Nhap_" + thangct] != DBNull.Value ? tbrow["Luong_Nhap_" + thangct] : 0) + SoPS2No;
                                    tbrow["Tien_Nhap_" + thangct] = Convert.ToDouble(tbrow["Tien_Nhap_" + thangct] != DBNull.Value ? tbrow["Tien_Nhap_" + thangct] : 0) + SoPS;
                                    tbrow["Luong_" + thangct] = luongTon;
                                    tbrow["Tien_" + thangct] = tienTon;
                                }
                                // Xử lý Xuất kho
                                // Xử lý Xuất kho
                                // Xử lý Xuất kho
                                // Xử lý Xuất kho
                                else if (MaLoai == 8)
                                {
                                    if (luongTon <= 0) continue; // Bỏ qua nếu kho âm hoặc bằng 0

                                    double SoPS2Co = Convert.ToDouble(ctRow["SoPS2Co"]);
                                    double SoPS;

                                    if (SoPS2Co >= luongTon) // Nếu bán nốt số lượng cuối cùng trong kho
                                    {
                                        SoPS = tienTon; // Lấy toàn bộ số tiền còn lại (dù nó là 88.444 hay 88.445), không cần chia
                                    }
                                    else
                                    {
                                        SoPS = Math.Round(tienTon / luongTon * SoPS2Co, MidpointRounding.AwayFromZero);
                                    }
                                    // Cập nhật biến tồn kho
                                    luongTon -= SoPS2Co;
                                    tienTon -= SoPS;

                                    // Ghi vào DataRow (Dùng += để cộng dồn nếu có nhiều phiếu xuất trong tháng)
                                    tbrow["Luong_Xuat_" + thangct] = Convert.ToDouble(tbrow["Luong_Xuat_" + thangct] != DBNull.Value ? tbrow["Luong_Xuat_" + thangct] : 0) + SoPS2Co;
                                    tbrow["Tien_Xuat_" + thangct] = Convert.ToDouble(tbrow["Tien_Xuat_" + thangct] != DBNull.Value ? tbrow["Tien_Xuat_" + thangct] : 0) + SoPS;
                                    tbrow["Luong_" + thangct] = luongTon;
                                    tbrow["Tien_" + thangct] = tienTon;

                                    string soHieuGV = $"{soHieuCT}GV";

                                    // Cập nhật Database
                                    string checkGVQuery = "SELECT COUNT(*) FROM ChungTu WHERE SoHieu = ? AND CT_ID LIKE ?";
                                    var tbCheck = ExecuteQuery(checkGVQuery, new OleDbParameter[]
                                    {
                                new OleDbParameter("?", soHieuGV),
                                new OleDbParameter("?", $"%{ctRow["MaSo"]}%")
                                    });
                                    int countGV = Convert.ToInt32(tbCheck.Rows[0][0]);

                                    if (countGV > 0)
                                    {
                                        string updateQuery = "UPDATE ChungTu SET SoPS = ? WHERE SoHieu = ? AND CT_ID LIKE ?";
                                        var updateParams = new OleDbParameter[]
                                        {
                                    new OleDbParameter("?", SoPS),
                                    new OleDbParameter("?", soHieuGV),
                                    new OleDbParameter("?", $"%{ctRow["MaSo"]}%")
                                        };
                                        ExecuteQueryResult(updateQuery, updateParams);
                                    }
                                }
                            } 
                            UpdateTonkho(tbTonKho, dtTuNgay.DateTime.Month);
                        }
                    }

                    // Hiển thị 100% khi hoàn tất vòng lặp tính toán
                    this.Invoke((MethodInvoker)delegate
                    { 
                        progressPanel1.Caption = "Hoàn tất tính toán (100%)";
                    });
                }
                catch (Exception ex)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        XtraMessageBox.Show("Có lỗi xảy ra trong quá trình tính giá vốn:\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                }
            }); // Kết thúc Task.Run

            // Mở lại nút bấm và thông báo
            simpleButton1.Enabled = true;
            progressPanel1.Caption = "Sẵn sàng";

            XtraMessageBox.Show("Đã tính giá vốn xong!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        public void UpdateTonkho(DataTable tbTonkho, int currentMonth)
        {
            if (tbTonkho == null || tbTonkho.Rows.Count == 0) return;

            DataRow row = tbTonkho.Rows[0];

            // Lấy số dư của tháng hiện tại (tháng đang tính)
            double luongTonHienTai = row[$"Luong_{currentMonth}"] != DBNull.Value ? Convert.ToDouble(row[$"Luong_{currentMonth}"]) : 0;
            double tienTonHienTai = row[$"Tien_{currentMonth}"] != DBNull.Value ? Convert.ToDouble(row[$"Tien_{currentMonth}"]) : 0;

            // --- BƯỚC 1: LAN TRUYỀN SỐ DƯ SANG CÁC THÁNG SAU ---
            for (int i = currentMonth + 1; i <= 12; i++)
            {
                string colLuongNhap = $"Luong_Nhap_{i}";
                string colLuongXuat = $"Luong_Xuat_{i}";
                string colLuongTon = $"Luong_{i}";
                string colTienTon = $"Tien_{i}";

                if (tbTonkho.Columns.Contains(colLuongNhap))
                {
                    double nhap = row[colLuongNhap] != DBNull.Value ? Convert.ToDouble(row[colLuongNhap]) : 0;
                    double xuat = row[colLuongXuat] != DBNull.Value ? Convert.ToDouble(row[colLuongXuat]) : 0;

                    if (nhap == 0 && xuat == 0)
                    {
                        // Nếu tháng sau KHÔNG có phát sinh -> Chỉ copy Tồn (Lượng và Tiền)
                        row[colLuongTon] = luongTonHienTai;
                        row[colTienTon] = tienTonHienTai;

                        // Không đụng chạm gì đến Luong_Nhap, Tien_Nhap, Luong_Xuat, Tien_Xuat của tháng đó
                    }
                    else
                    {
                        // Nếu tháng sau CÓ phát sinh (tính lùi ngày) -> Phải tính dây chuyền tiếp
                        double tienNhap = row[$"Tien_Nhap_{i}"] != DBNull.Value ? Convert.ToDouble(row[$"Tien_Nhap_{i}"]) : 0;
                        double tienXuat = row[$"Tien_Xuat_{i}"] != DBNull.Value ? Convert.ToDouble(row[$"Tien_Xuat_{i}"]) : 0;

                        luongTonHienTai = luongTonHienTai + nhap - xuat;
                        tienTonHienTai = tienTonHienTai + tienNhap - tienXuat;

                        row[colLuongTon] = luongTonHienTai;
                        row[colTienTon] = tienTonHienTai;
                    }
                }
            }

            // --- BƯỚC 2: LƯU XUỐNG DATABASE ---
            List<string> setClauses = new List<string>();
            List<OleDbParameter> updateParams = new List<OleDbParameter>();

            for (int i = 1; i <= 12; i++)
            {
                // Chỉ lấy đúng các cột cần thiết để Update (bao gồm cả Nhập, Xuất, và Tồn)
                string colTienNhap = $"Tien_Nhap_{i}";
                string colLuongNhap = $"Luong_Nhap_{i}";
                string colTienXuat = $"Tien_Xuat_{i}";
                string colLuongXuat = $"Luong_Xuat_{i}";
                string colLuongTon = $"Luong_{i}";
                string colTienTon = $"Tien_{i}";

                if (tbTonkho.Columns.Contains(colTienNhap))
                {
                    setClauses.Add($"{colTienNhap} = ?, {colLuongNhap} = ?, {colTienXuat} = ?, {colLuongXuat} = ?, {colLuongTon} = ?, {colTienTon} = ?");

                    updateParams.Add(new OleDbParameter("?", row[colTienNhap] != DBNull.Value ? row[colTienNhap].ToString() : "0"));
                    updateParams.Add(new OleDbParameter("?", row[colLuongNhap] != DBNull.Value ? row[colLuongNhap].ToString() : "0"));
                    updateParams.Add(new OleDbParameter("?", row[colTienXuat] != DBNull.Value ? row[colTienXuat].ToString() : "0"));
                    updateParams.Add(new OleDbParameter("?", row[colLuongXuat] != DBNull.Value ? row[colLuongXuat].ToString() : "0"));
                    updateParams.Add(new OleDbParameter("?", row[colLuongTon] != DBNull.Value ? row[colLuongTon].ToString() : "0"));
                    updateParams.Add(new OleDbParameter("?", row[colTienTon] != DBNull.Value ? row[colTienTon].ToString() : "0"));
                }
            }

            // Ghép câu lệnh: UPDATE TonKho SET [các cụm] WHERE MaSo = ?
            // Đã sửa lỗi: Nối chuỗi trực tiếp không dùng dấu phẩy trước WHERE
            string updateQuery = "UPDATE TonKho SET " + string.Join(", ", setClauses) + " WHERE MaSo = ?";

            // Thêm tham số cho WHERE
            updateParams.Add(new OleDbParameter("?", row["MaSo"].ToString()));

            // Thực thi
            ExecuteQueryResult(updateQuery, updateParams.ToArray());
        }
    }
}