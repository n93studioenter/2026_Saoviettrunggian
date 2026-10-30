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
        public string ChungTu2TKNC(int loai, int p = 0)
        {
            string sh = (p > 0) ? "P" : "";
            string result = "";

            switch (loai)
            {
                case 0:
                    result = " (ChungTu" + sh + " INNER JOIN HethongTK ON ChungTu" + sh + ".MaTKNo = HethongTK.MaSo) INNER JOIN HethongTK AS TK ON ChungTu" + sh + ".MaTKCo = TK.MaSo ";
                    break;

                case 10:
                    result = " (ChungTu" + sh + " INNER JOIN HethongTK ON ChungTu" + sh + ".MaTKTCNo = HethongTK.MaSo) INNER JOIN HethongTK AS TK ON ChungTu" + sh + ".MaTKTCCo = TK.MaSo ";
                    break;

                case -1:
                    result = " ChungTu" + sh + " INNER JOIN HethongTK ON ChungTu" + sh + ".MaTKNo = HethongTK.MaSo ";
                    break;

                case 1:
                    result = " ChungTu" + sh + " INNER JOIN HethongTK ON ChungTu" + sh + ".MaTKCo = HethongTK.MaSo ";
                    break;

                case -2:
                    result = " ChungTu" + sh + " INNER JOIN HethongTK ON ChungTu" + sh + ".MaTKTCNo = HethongTK.MaSo ";
                    break;

                case 2:
                    result = " ChungTu" + sh + " INNER JOIN HethongTK ON ChungTu" + sh + ".MaTKTCCo = HethongTK.MaSo ";
                    break;

                default:
                    result = ""; // Trường hợp mặc định nếu loai không khớp
                    break;
            }

            return result;
        }
        public const int TKDT_ID = 5000;
        public void TinhGVBHBQ(int tdau, int tcuoi, int tl, long mvt, int loai)
        {
            int OutCost = 0;

            if (OutCost == 0)
            {
                // =========================================================
                // BƯỚC 1: XÓA DỮ LIỆU CŨ (Tránh bị nhân đôi dòng GV)
                // =========================================================
                string deleteQuery1 = @"DELETE FROM ChungTu 
                                WHERE MaLoai = 2 AND SoHieu LIKE '%GV' 
                                AND ThangCT >= ? AND ThangCT <= ?";
                if (mvt > 0) deleteQuery1 += " AND MaVattu = ?";

                List<OleDbParameter> paramsDelete1 = new List<OleDbParameter>
        {
            new OleDbParameter("?", tdau),
            new OleDbParameter("?", tcuoi)
        };
                if (mvt > 0) paramsDelete1.Add(new OleDbParameter("?", mvt));

                ExecuteQueryResult(deleteQuery1, paramsDelete1.ToArray());


                // =========================================================
                // BƯỚC 2: LẤY TÀI KHOẢN MẶC ĐỊNH
                // =========================================================
                long m1 = 151; // ID tài khoản Nợ mặc định
                string m1_Text = "151"; // Số hiệu tài khoản Nợ mặc định

                long n1 = 39;  // ID tài khoản Có mặc định
                string n1_Text = "156"; // Số hiệu tài khoản Có mặc định

                // Lấy tài khoản 632
                string sqlM1 = "SELECT TOP 1 MaSo, SoHieu FROM HethongTK WHERE SoHieu LIKE ? AND TKCon = 0 ORDER BY SoHieu";
                var tbM1 = ExecuteQuery(sqlM1, new OleDbParameter[] { new OleDbParameter("?", "632%") });
                if (tbM1 != null && tbM1.Rows.Count > 0)
                {
                    m1 = Convert.ToInt64(tbM1.Rows[0]["MaSo"]);
                    m1_Text = tbM1.Rows[0]["SoHieu"].ToString();
                }

                // Lấy tài khoản 156
                string sqlN1 = "SELECT TOP 1 MaSo, SoHieu FROM HethongTK WHERE SoHieu LIKE ? AND TKCon = 0 ORDER BY SoHieu";
                var tbN1 = ExecuteQuery(sqlN1, new OleDbParameter[] { new OleDbParameter("?", "156%") });
                if (tbN1 != null && tbN1.Rows.Count > 0)
                {
                    n1 = Convert.ToInt64(tbN1.Rows[0]["MaSo"]);
                    n1_Text = tbN1.Rows[0]["SoHieu"].ToString();
                }


                // =========================================================
                // BƯỚC 3: LẤY DANH SÁCH CHỨNG TỪ CẦN TẠO DÒNG GIÁ VỐN
                // =========================================================
                string sqlSelect = @"
            SELECT ChungTu.MaCT, ChungTu.MaSo, ChungTu.SoHieu, ChungTu.ThangCT, ChungTu.NgayCT, ChungTu.NgayGS, 
                   ChungTu.MaNguon, ChungTu.MaKho, ChungTu.DienGiai, 
                   KhoHang.MaTKGV, PhanLoaiVattu.MaTK,
                   ChungTu.SoPS, ChungTu.SoPS2No, ChungTu.SoPS2Co, ChungTu.MaVattu, ChungTu.GhiChu, 
                   ChungTu.MaDT, ChungTu.MaDT1, ChungTu.MaDT2, ChungTu.MaDT3, ChungTu.MaKH, 
                   ChungTu.CTGS, ChungTu.MaKHC, ChungTu.MaTP, ChungTu.DVT, ChungTu.User_ID, 
                   ChungTu.MaNV, ChungTu.HanTT, ChungTu.SH1, ChungTu.T1, ChungTu.TLCK, ChungTu.CK
            FROM (((" + ChungTu2TKNC(1) + @") 
            INNER JOIN KhoHang ON ChungTu.MaKho = KhoHang.MaSo) 
            INNER JOIN Vattu ON ChungTu.MaVattu = Vattu.MaSo) 
            INNER JOIN PhanLoaiVattu ON Vattu.MaPhanLoai = PhanLoaiVattu.MaSo
            WHERE ChungTu.MaLoai = 8 AND ChungTu.MaVattu > 0 AND TK_ID = ? 
              AND (Not HethongTK.SoHieu LIKE '5113%') 
              AND ChungTu.ThangCT >= ? AND ChungTu.ThangCT <= ?";

                if (mvt > 0) sqlSelect += " AND ChungTu.MaVattu = ?";
                sqlSelect += " ORDER BY ChungTu.NgayCT, ChungTu.MaCT";

                List<OleDbParameter> paramsSelect = new List<OleDbParameter>
        {
            new OleDbParameter("?", TKDT_ID),
            new OleDbParameter("?", tdau),
            new OleDbParameter("?", tcuoi)
        };
                if (mvt > 0) paramsSelect.Add(new OleDbParameter("?", mvt));

                var tbCanTaoGV = ExecuteQuery(sqlSelect, paramsSelect.ToArray());


                // =========================================================
                // BƯỚC 4: DUYỆT VÀ INSERT TỪNG DÒNG
                // =========================================================
                if (tbCanTaoGV != null && tbCanTaoGV.Rows.Count > 0)
                {
                    foreach (DataRow row in tbCanTaoGV.Rows)
                    {
                        // Xử lý IIF cho MaTKGV (Nếu = 0 thì lấy m1)
                        long maTKGV = (row["MaTKGV"] != DBNull.Value && Convert.ToInt64(row["MaTKGV"]) != 0)
                                      ? Convert.ToInt64(row["MaTKGV"]) : m1;
                        string maTKGV_Text = (maTKGV == m1) ? m1_Text : "632";

                        // Xử lý IIF cho MaTK (Nếu = 0 thì lấy n1)
                        long maTKPL = (row["MaTK"] != DBNull.Value && Convert.ToInt64(row["MaTK"]) != 0)
                                      ? Convert.ToInt64(row["MaTK"]) : n1;
                        string maTKPL_Text = (maTKPL == n1) ? n1_Text : "156";

                        // Lấy MaSo gốc an toàn
                        long maSoGoc = row["MaSo"] != DBNull.Value ? Convert.ToInt64(row["MaSo"]) : 0;

                        // SỬA LẠI TÊN CỘT CHO ĐÚNG VỚI DATABASE: MaTKTCNo và MaTKTCCo
                        string sqlInsert = @"
                    INSERT INTO ChungTu (MaCT, MaLoai, SoHieu, ThangCT, NgayCT, NgayGS, MaNguon, MaKho, DienGiai, 
                                         MaTkNo, MaTkCo, MaTKTCNo, MaTKTCCo, SoPS, SoPS2No, SoPS2Co, MaVattu, GhiChu, CT_ID, 
                                         MaDT, MaDT1, MaDT2, MaDT3, MaKH, CTGS, MaKHC, MaTP, DVT, User_ID, 
                                         MaNV, HanTT, SH1, T1, TLCK, CK) 
                    VALUES (?, 2, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                        // Ép kiểu cẩn thận sang Int32 và Double để tránh lỗi "Multiple-step OLE DB"
                        List<OleDbParameter> insertParams = new List<OleDbParameter>
                {
                    new OleDbParameter("?", Convert.ToInt32(1000000 + Convert.ToInt64(row["MaCT"].ToString()))),
                    new OleDbParameter("?", row["SoHieu"].ToString() + "GV"),
                    new OleDbParameter("?", Convert.ToInt32(row["ThangCT"])),
                    new OleDbParameter("?", row["NgayCT"] != DBNull.Value ? row["NgayCT"] : DBNull.Value),
                    new OleDbParameter("?", row["NgayGS"] != DBNull.Value ? row["NgayGS"] : DBNull.Value),
                    new OleDbParameter("?", row["MaNguon"] != DBNull.Value ? row["MaNguon"] : DBNull.Value),
                    new OleDbParameter("?", row["MaKho"] != DBNull.Value ? row["MaKho"] : DBNull.Value),
                    new OleDbParameter("?", row["DienGiai"] != DBNull.Value ? row["DienGiai"] : DBNull.Value),
                    new OleDbParameter("?", Convert.ToInt32(maTKGV)), // MaTkNo
                    new OleDbParameter("?", Convert.ToInt32(maTKPL)), // MaTkCo
                    
                    // 2 THAM SỐ MỚI CHO MaTKTCNo VÀ MaTKTCCo
                    new OleDbParameter("?", maTKGV_Text), // MaTKTCNo
                    new OleDbParameter("?", maTKPL_Text), // MaTKTCCo

                    new OleDbParameter("?", Convert.ToDouble(row["SoPS"])),
                    new OleDbParameter("?", Convert.ToDouble(row["SoPS2No"])),
                    new OleDbParameter("?", Convert.ToDouble(row["SoPS2Co"])),
                    new OleDbParameter("?", row["MaVattu"] != DBNull.Value ? row["MaVattu"] : DBNull.Value),
                    new OleDbParameter("?", row["GhiChu"] != DBNull.Value ? row["GhiChu"] : DBNull.Value),
                    new OleDbParameter("?", Convert.ToInt32(maSoGoc + 500000000)), // CT_ID
                    new OleDbParameter("?", row["MaDT"] != DBNull.Value ? row["MaDT"] : DBNull.Value),
                    new OleDbParameter("?", row["MaDT1"] != DBNull.Value ? row["MaDT1"] : DBNull.Value),
                    new OleDbParameter("?", row["MaDT2"] != DBNull.Value ? row["MaDT2"] : DBNull.Value),
                    new OleDbParameter("?", row["MaDT3"] != DBNull.Value ? row["MaDT3"] : DBNull.Value),
                    new OleDbParameter("?", row["MaKH"] != DBNull.Value ? row["MaKH"] : DBNull.Value),
                    new OleDbParameter("?", row["CTGS"] != DBNull.Value ? row["CTGS"] : DBNull.Value),
                    new OleDbParameter("?", row["MaKHC"] != DBNull.Value ? row["MaKHC"] : DBNull.Value),
                    new OleDbParameter("?", row["MaTP"] != DBNull.Value ? row["MaTP"] : DBNull.Value),
                    new OleDbParameter("?", row["DVT"] != DBNull.Value ? row["DVT"] : DBNull.Value),
                    new OleDbParameter("?", row["User_ID"] != DBNull.Value ? row["User_ID"] : DBNull.Value),
                    new OleDbParameter("?", row["MaNV"] != DBNull.Value ? row["MaNV"] : DBNull.Value),
                    new OleDbParameter("?", row["HanTT"] != DBNull.Value ? row["HanTT"] : DBNull.Value),
                    new OleDbParameter("?", row["SH1"] != DBNull.Value ? row["SH1"] : DBNull.Value),
                    new OleDbParameter("?", row["T1"] != DBNull.Value ? row["T1"] : DBNull.Value),
                    new OleDbParameter("?", row["TLCK"] != DBNull.Value ? row["TLCK"] : DBNull.Value),
                    new OleDbParameter("?", row["CK"] != DBNull.Value ? row["CK"] : DBNull.Value)
                };

                        ExecuteQueryResult(sqlInsert, insertParams.ToArray());
                    }
                }
            }
        }
        private async void simpleButton1_Click(object sender, EventArgs e)
        {
           
            // Vô hiệu hóa nút bấm để tránh bấm 2 lần
            simpleButton1.Enabled = false;
            progressPanel1.Visible = true;  
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
                        return;
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
                    if (checkEdit1.Checked)
                        TinhGVBHBQ(finalLock, dtDenngay.DateTime.Month, 6, 0, 2);
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

                    // --- TÍNH TOÁN TỔNG SỐ CHỨNG TỪ ĐỂ CHIA % ---
                    int totalChungTu = 0;
                    foreach (DataRow row in tbVattuFiltered)
                    {
                        string maVT = row["MaSo"].ToString();
                        var tempChungTu = vattuGroupby.FirstOrDefault(g => g.Key == maVT)?.ToList();
                        if (tempChungTu != null)
                        {
                            // Chỉ đếm những chứng từ không chứa chữ "GV"
                            totalChungTu += tempChungTu.Count(m => !m["SoHieu"].ToString().Contains("GV"));
                        }
                    }

                    int currentChungTuCount = 0;

                    // --- 7. VÒNG LẶP TÍNH GIÁ VỐN ---
                    foreach (DataRow row in tbVattuFiltered)
                    {
                        string maVatTu = row["MaSo"].ToString();
                        string tenVT = row["TenVatTu"].ToString();

                        // Lấy tồn kho gốc
                        string qrtk = "SELECT * FROM TonKho where MaVatTu = @MaVatTu";
                        var tbTonKho = ExecuteQuery(qrtk, new OleDbParameter("@MaVatTu", maVatTu));
                        ResetTonkho(tbTonKho, finalLock);
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

                            DataRow tbrow = tbTonKho.Rows[0];
                            foreach (DataRow ctRow in chungTuCuaVatTu.Rows)
                            {
                                // --- CẬP NHẬT TIẾN TRÌNH THEO TỪNG CHỨNG TỪ ---
                                currentChungTuCount++;
                                int progressPercent = (int)((double)currentChungTuCount / totalChungTu * 90);

                                // Cứ 20 chứng từ cập nhật UI 1 lần để không làm chậm vòng lặp
                                if (currentChungTuCount % 20 == 0 || currentChungTuCount == totalChungTu)
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        progressPanel1.Caption = $"Đang tính ({progressPercent}%) - Chứng từ {currentChungTuCount}/{totalChungTu}: {Helpers.ConvertVniToUnicode(tenVT)} ({maVatTu})";
                                    });
                                }
                                // ------------------------------------------------

                                int thangct = ctRow["ThangCT"] != DBNull.Value ? Convert.ToInt32(ctRow["ThangCT"]) : 0;
                                string soHieuCT = ctRow["SoHieu"].ToString();
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
                                else if (MaLoai == 8)
                                {
                                    if (luongTon <= 0) continue;

                                    double SoPS2Co = Convert.ToDouble(ctRow["SoPS2Co"]);
                                    double SoPS;

                                    if (SoPS2Co >= luongTon) // Nếu bán nốt số lượng cuối cùng trong kho
                                    {
                                        SoPS = tienTon; // Lấy toàn bộ số tiền còn lại
                                    }
                                    else
                                    {
                                        SoPS = Math.Round(tienTon / luongTon * SoPS2Co, MidpointRounding.AwayFromZero);
                                    }

                                    luongTon -= SoPS2Co;
                                    tienTon -= SoPS;

                                    tbrow["Luong_Xuat_" + thangct] = Convert.ToDouble(tbrow["Luong_Xuat_" + thangct] != DBNull.Value ? tbrow["Luong_Xuat_" + thangct] : 0) + SoPS2Co;
                                    tbrow["Tien_Xuat_" + thangct] = Convert.ToDouble(tbrow["Tien_Xuat_" + thangct] != DBNull.Value ? tbrow["Tien_Xuat_" + thangct] : 0) + SoPS;
                                    tbrow["Luong_" + thangct] = luongTon;
                                    tbrow["Tien_" + thangct] = tienTon;

                                    string soHieuGV = $"{soHieuCT}GV";

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
                            UpdateTonkho(tbTonKho, finalLock);
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