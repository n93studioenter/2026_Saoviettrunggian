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
        // BẮT BUỘC thêm async vào đây để dùng Task.Run
        private async void simpleButton1_Click(object sender, EventArgs e)
        {
            // Vô hiệu hóa nút bấm để tránh bấm 2 lần
            simpleButton1.Enabled = false;

            // Bọc toàn bộ code cũ của bạn vào Task.Run
            await Task.Run(() =>
            {
                string appPath = Assembly.GetExecutingAssembly().Location;

                // Lấy thư mục chứa ứng dụng
                string directoryPath = Path.GetDirectoryName(appPath);

                // Xóa phần \bin\Debug để lấy đường dẫn gốc
                string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

                // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
                string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");
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
                string password = "1@35^7*9)1";
                connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";

                string qr = "SELECT * FROM License";
                var tbLicense = ExecuteQuery(qr, null);

                if (tbLicense.Rows.Count > 0)
                {
                    var row = tbLicense.Rows[0];
                    finalLock = 0; // Mặc định

                    // Duyệt ngược từ 12 về 1
                    for (int i = 12; i >= 1; i--)
                    {
                        string columnName = "Lock" + i;

                        if (tbLicense.Columns.Contains(columnName))
                        {
                            if (row[columnName] != DBNull.Value && row[columnName].ToString() == "1")
                            {
                                finalLock = i; // Lấy giá trị lớn nhất tìm thấy
                                break; // Tìm thấy gói cao nhất là dừng luôn
                            }
                        }
                    }
                }
                // 1. Thêm ORDER BY vào câu SQL
                var qrTimct = @"SELECT * FROM ChungTu 
            WHERE NgayCT >= @NgayBD AND NgayCT < @NgayKT_NextDay
            ORDER BY NgayCT ASC, MaSo ASC";

                // 2. Xử lý tham số
                var parameterss = new OleDbParameter[]
                {
            new OleDbParameter("@NgayBD", dtTuNgay.DateTime.Date),
            new OleDbParameter("@NgayKT_NextDay", dtDenngay.DateTime.Date.AddDays(1))
                };

                // 3. Thực thi
                var tbchungtu = ExecuteQuery(qrTimct, parameterss);

                // 2. Nhóm chứng từ theo Mã Vật Tư 
                var vattuGroupby = tbchungtu.AsEnumerable()
                                            .GroupBy(m => m["MaVattu"].ToString())
                                            .ToList();

                var danhSachMaVattu = vattuGroupby.Select(g => g.Key).ToList();

                // 3. Lấy danh mục Vật Tư
                string qrvt = "SELECT * FROM Vattu";
                var tbVattu = ExecuteQuery(qrvt, null);

                // 4. Tiến hành lọc Vật Tư
                var queryVattu = tbVattu.AsEnumerable();

                queryVattu = queryVattu.Where(r => danhSachMaVattu.Contains(r["MaSo"].ToString()));

                string soHieuNhap = txtSoHieu.Text.Trim();
                if (!string.IsNullOrEmpty(soHieuNhap))
                {
                    queryVattu = queryVattu.Where(r => r.Field<string>("SoHieu") != null &&
                                                       r.Field<string>("SoHieu").ToLower().Contains(soHieuNhap.ToLower()));
                }

                var tbVattuFiltered = queryVattu.ToList();

                // 5. Duyệt vòng lặp
                foreach (DataRow row in tbVattuFiltered)
                {
                    // --- CHỈ SỬA PHẦN PROGRESS Ở ĐÂY ---
                    // Dùng Invoke để cập nhật UI từ luồng phụ (Bỏ Application.DoEvents)
                    string tenVT = row["TenVatTu"].ToString();
                    string maVT = row["MaSo"].ToString();

                    this.Invoke((MethodInvoker)delegate
                    {
                        progressPanel1.Caption = $"Đang tính giá vốn cho vật tư: {tenVT} ({maVT})";
                    });
                    // -----------------------------------

                    string maVatTu = row["MaSo"].ToString();

                    // Lấy tồn kho theo lock 
                    string qrtk = "SELECT * FROM TonKho where MaVatTu = @MaVatTu";
                    var tbTonKho = ExecuteQuery(qrtk, new OleDbParameter("@MaVatTu", maVatTu));

                    if (tbTonKho != null && tbTonKho.Rows.Count > 0)
                    {
                        double tiennhap_lock = Convert.ToDouble(tbTonKho.Rows[0][$"Tien_Nhap_{finalLock}"]);
                        double luongnhap_lock = Convert.ToDouble(tbTonKho.Rows[0][$"Luong_Nhap_{finalLock}"]);
                        double tienxuat_lock = Convert.ToDouble(tbTonKho.Rows[0][$"Tien_Xuat_{finalLock}"]);
                        double luongxuat_lock = Convert.ToDouble(tbTonKho.Rows[0][$"Luong_Xuat_{finalLock}"]);
                        double tien_lock = Convert.ToDouble(tbTonKho.Rows[0][$"Tien_{finalLock}"]);
                        double luong_lock = Convert.ToDouble(tbTonKho.Rows[0][$"Luong_{finalLock}"]);

                        double luongTon = luong_lock;
                        double tienTon = tien_lock;

                        var chungTuCuaVatTu = vattuGroupby.FirstOrDefault(g => g.Key == maVatTu)?.CopyToDataTable();
                        chungTuCuaVatTu = chungTuCuaVatTu.AsEnumerable().Where(m => !m["SoHieu"].ToString().Contains("GV")).CopyToDataTable();

                        foreach (DataRow ctRow in chungTuCuaVatTu.Rows)
                        {
                            string soHieuCT = ctRow["SoHieu"].ToString();
                            DateTime ngayCT = Convert.ToDateTime(ctRow["NgayCT"]);

                            double MaLoai = Convert.ToDouble(ctRow["MaLoai"]);
                            //Xu lý vào
                            if (MaLoai == 1)
                            {
                                double SoPS2No = Convert.ToDouble(ctRow["SoPS2No"]);
                                double SoPS = Convert.ToDouble(ctRow["SoPS"]);
                                double GV = Math.Round(SoPS / SoPS2No);
                                luongTon += SoPS2No;
                                tienTon += SoPS;
                            }
                            //Xử lý ra
                            if (MaLoai == 8)
                            {
                                double GV = Math.Round(tienTon / luongTon);
                                double SoPS2Co = Convert.ToDouble(ctRow["SoPS2Co"]);
                                double SoPS = Math.Round(GV * SoPS2Co);
                                luongTon -= SoPS2Co;
                                tienTon -= SoPS;

                                string updateQuery = "UPDATE ChungTu SET SoPS = ? WHERE sohieu = ? AND CT_ID LIKE ?";

                                var parameters = new OleDbParameter[]
                                {
                            new OleDbParameter("?", SoPS),
                            new OleDbParameter("?", $"{soHieuCT}GV"),
                            new OleDbParameter("?", $"%{ctRow["MaSo"]}%")
                                };
                                int rowsAffected = ExecuteQueryResult(updateQuery, parameters);
                            }
                        }
                    }
                }
            }); // Kết thúc Task.Run

            // Mở lại nút bấm và hiển thị thông báo sau khi chạy xong
            simpleButton1.Enabled = true;
            progressPanel1.Caption = "Đã tính xong!";
            XtraMessageBox.Show("Đã tính giá vốn xong!");
        }
        // BẮT BUỘC thêm async để dùng Task.Run
        private async void simpleButton1_Clicknew(object sender, EventArgs e)
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
                        WHERE NgayCT >= @NgayBD_Loc AND NgayCT < @NgayKT_Loc";
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
                     WHERE NgayCT >= @NgayBD_Tinh AND NgayCT < @NgayKT_NextDay
                     ORDER BY NgayCT ASC, MaLoai ASC, MaSo ASC";
                    var paramsTinh = new OleDbParameter[]
                    {
                new OleDbParameter("@NgayBD_Tinh", ngayBatDauTinhToan),
                new OleDbParameter("@NgayKT_NextDay", dtDenngay.DateTime.Date.AddDays(1))
                    };
                    var tbchungtu = ExecuteQuery(qrTimct, paramsTinh);

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

                        if (tbTonKho != null && tbTonKho.Rows.Count > 0)
                        {
                            double tienTon = Convert.ToDouble(tbTonKho.Rows[0][$"Tien_{finalLock}"]);
                            double luongTon = Convert.ToDouble(tbTonKho.Rows[0][$"Luong_{finalLock}"]);

                            var chungTuCuaVatTu = vattuGroupby.FirstOrDefault(g => g.Key == maVatTu)?.CopyToDataTable();
                            if (chungTuCuaVatTu == null) continue;

                            // Bỏ qua phiếu GV cũ
                            chungTuCuaVatTu = chungTuCuaVatTu.AsEnumerable()
                                                .Where(m => !m["SoHieu"].ToString().Contains("GV"))
                                                .CopyToDataTable();

                            foreach (DataRow ctRow in chungTuCuaVatTu.Rows)
                            {
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
                                }
                                // Xử lý Xuất kho
                                // Xử lý Xuất kho
                                else if (MaLoai == 8)
                                {
                                    if (luongTon <= 0) continue;

                                    double GV = Math.Round(tienTon / Math.Round(luongTon));
                                    double SoPS2Co = Convert.ToDouble(ctRow["SoPS2Co"]);
                                    double SoPS = Math.Round(GV * SoPS2Co);

                                    luongTon -= SoPS2Co;
                                    tienTon -= SoPS;

                                    string soHieuGV = $"{soHieuCT}GV";

                                    // 1. Kiểm tra xem phiếu GV đã tồn tại chưa
                                    // (Thay đổi: Dùng CT_ID LIKE để đếm chính xác dòng chi tiết đó)
                                    string checkGVQuery = "SELECT COUNT(*) FROM ChungTu WHERE SoHieu = ? AND CT_ID LIKE ?";
                                    var tbCheck = ExecuteQuery(checkGVQuery, new OleDbParameter[]
                                    {
        new OleDbParameter("?", soHieuGV),
        new OleDbParameter("?", $"%{ctRow["MaSo"]}%")
                                    });
                                    int countGV = Convert.ToInt32(tbCheck.Rows[0][0]);

                                    if (countGV > 0)
                                    {
                                        // 2A. Đã có -> Cập nhật lại cột SoPS
                                        // (Thay đổi: Thêm điều kiện CT_ID LIKE để update đúng dòng chi tiết)
                                        string updateQuery = "UPDATE ChungTu SET SoPS = ? WHERE SoHieu = ? AND CT_ID LIKE ?";
                                        var updateParams = new OleDbParameter[]
                                        {
            new OleDbParameter("?", SoPS),
            new OleDbParameter("?", soHieuGV),
            new OleDbParameter("?", $"%{ctRow["MaSo"]}%") // Sử dụng đúng Mã số của dòng ctRow hiện tại
                                        };
                                        ExecuteQueryResult(updateQuery, updateParams);
                                    }
                                }
                            }
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
    }
}