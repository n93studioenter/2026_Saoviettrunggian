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
        public void ResetTonkho(DataTable tbTonKho, int finalLock)
        {
            if (tbTonKho != null && tbTonKho.Rows.Count > 0)
            {
                DataRow row = tbTonKho.Rows[0];
                for (int i = finalLock + 1; i <= 12; i++)
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
        // =====================================================================
        // HÀM PHỤ TRỢ 1: CỘNG DỒN AN TOÀN
        // =====================================================================
        private static void AddTo(DataRow r, string col, double v)
        {
            r[col] = (r[col] == DBNull.Value ? 0d : Convert.ToDouble(r[col])) + v;
        }

        // =====================================================================
        // HÀM PHỤ TRỢ 2: TẠO CÂU LỆNH UPDATE TỒN KHO (Dùng cho Transaction)
        // =====================================================================
        public string BuildUpdateTonKhoQuery(DataRow row)
        {
            List<string> setClauses = new List<string>();
            for (int i = 1; i <= 12; i++)
            {
                string colTienNhap = $"Tien_Nhap_{i}";
                string colLuongNhap = $"Luong_Nhap_{i}";
                string colTienXuat = $"Tien_Xuat_{i}";
                string colLuongXuat = $"Luong_Xuat_{i}";
                string colLuongTon = $"Luong_{i}";
                string colTienTon = $"Tien_{i}";

                if (row.Table.Columns.Contains(colTienNhap))
                {
                    setClauses.Add($"{colTienNhap} = {GetVal(row[colTienNhap])}, {colLuongNhap} = {GetVal(row[colLuongNhap])}, {colTienXuat} = {GetVal(row[colTienXuat])}, {colLuongXuat} = {GetVal(row[colLuongXuat])}, {colLuongTon} = {GetVal(row[colLuongTon])}, {colTienTon} = {GetVal(row[colTienTon])}");
                }
            }
            return $"UPDATE TonKho SET {string.Join(", ", setClauses)} WHERE MaSo = '{row["MaSo"]}'";
        }

        private string GetVal(object val)
        {
            return val != DBNull.Value ? Convert.ToDouble(val).ToString() : "0";
        }

        // =====================================================================
        // HÀM CHÍNH 1: TÍNH GIÁ VỐN BÌNH QUÂN LIÊN HOÀN
        // =====================================================================
        public void TinhGVBHBQ(int tdau, int tcuoi, int tl, long mvt, int loai, Action<int> progressCallback = null)
        {
            progressCallback?.Invoke(0);

            // ===== BƯỚC 1: XÓA DỮ LIỆU CŨ =====
            string deleteQuery = @"DELETE FROM ChungTu 
                           WHERE MaLoai = 2 AND SoHieu LIKE '%GV' 
                           AND ThangCT >= ? AND ThangCT <= ?";
            var pDel = new List<OleDbParameter>
    {
        new OleDbParameter("?", tdau),
        new OleDbParameter("?", tcuoi)
    };
            if (mvt > 0)
            {
                deleteQuery += " AND MaVattu = ?";
                pDel.Add(new OleDbParameter("?", mvt));
            }
            ExecuteQueryResult(deleteQuery, pDel.ToArray());
            progressCallback?.Invoke(20);

            // ===== BƯỚC 2: LẤY TÀI KHOẢN MẶC ĐỊNH =====
            (long id, string text) GetDefaultTK(string prefix, long defId, string defText)
            {
                var tb = ExecuteQuery(
                    "SELECT TOP 1 MaSo, SoHieu FROM HethongTK WHERE SoHieu LIKE ? AND TKCon = 0 ORDER BY SoHieu",
                    new[] { new OleDbParameter("?", prefix + "%") });
                return (tb != null && tb.Rows.Count > 0)
                    ? (Convert.ToInt64(tb.Rows[0]["MaSo"]), tb.Rows[0]["SoHieu"].ToString())
                    : (defId, defText);
            }

            var (m1, m1_Text) = GetDefaultTK("632", 151, "151");
            var (n1, n1_Text) = GetDefaultTK("156", 39, "156");
            progressCallback?.Invoke(40);

            // ===== BƯỚC 3: INSERT ... SELECT (TỐI ƯU TỐC ĐỘ) =====
            string m1Lit = m1_Text.Replace("'", "''");
            string n1Lit = n1_Text.Replace("'", "''");

            string tkNo = $"IIf(IsNull(KhoHang.MaTKGV) OR KhoHang.MaTKGV = 0, {m1}, KhoHang.MaTKGV)";
            string tkCo = $"IIf(IsNull(PhanLoaiVattu.MaTK) OR PhanLoaiVattu.MaTK = 0, {n1}, PhanLoaiVattu.MaTK)";

            string sql = $@"
INSERT INTO ChungTu (MaCT, MaLoai, SoHieu, ThangCT, NgayCT, NgayGS, MaNguon, MaKho, DienGiai,
                     MaTkNo, MaTkCo, MaTKTCNo, MaTKTCCo, SoPS, SoPS2No, SoPS2Co, MaVattu, GhiChu, CT_ID,
                     MaDT, MaDT1, MaDT2, MaDT3, MaKH, CTGS, MaKHC, MaTP, DVT, User_ID,
                     MaNV, HanTT, SH1, T1, TLCK, CK)
SELECT 1000000 + ChungTu.MaCT, 2, ChungTu.SoHieu & 'GV', ChungTu.ThangCT, ChungTu.NgayCT, ChungTu.NgayGS,
       ChungTu.MaNguon, ChungTu.MaKho, ChungTu.DienGiai,
       {tkNo}, {tkCo},
       IIf(({tkNo}) = {m1}, '{m1Lit}', '632'),
       IIf(({tkCo}) = {n1}, '{n1Lit}', '156'),
       ChungTu.SoPS, ChungTu.SoPS2No, ChungTu.SoPS2Co, ChungTu.MaVattu, ChungTu.GhiChu,
       ChungTu.MaSo + 500000000,
       ChungTu.MaDT, ChungTu.MaDT1, ChungTu.MaDT2, ChungTu.MaDT3, ChungTu.MaKH, ChungTu.CTGS,
       ChungTu.MaKHC, ChungTu.MaTP, ChungTu.DVT, ChungTu.User_ID,
       ChungTu.MaNV, ChungTu.HanTT, ChungTu.SH1, ChungTu.T1, ChungTu.TLCK, ChungTu.CK
FROM (((" + ChungTu2TKNC(1) + $@") 
INNER JOIN KhoHang ON ChungTu.MaKho = KhoHang.MaSo) 
INNER JOIN Vattu ON ChungTu.MaVattu = Vattu.MaSo) 
INNER JOIN PhanLoaiVattu ON Vattu.MaPhanLoai = PhanLoaiVattu.MaSo
WHERE ChungTu.MaLoai = 8 AND ChungTu.MaVattu > 0 AND TK_ID = ? 
  AND (Not HethongTK.SoHieu LIKE '5113%') 
  AND ChungTu.ThangCT >= ? AND ChungTu.ThangCT <= ?";

            var pIns = new List<OleDbParameter>
    {
        new OleDbParameter("?", TKDT_ID),
        new OleDbParameter("?", tdau),
        new OleDbParameter("?", tcuoi)
    };
            if (mvt > 0)
            {
                sql += " AND ChungTu.MaVattu = ?";
                pIns.Add(new OleDbParameter("?", mvt));
            }

            ExecuteQueryResult(sql, pIns.ToArray());
            progressCallback?.Invoke(100);
        }

        // =====================================================================
        // HÀM CHÍNH 2: SỰ KIỆN CLICK NÚT TÍNH GIÁ VỐN
        // =====================================================================
        private async void simpleButton1_Click(object sender, EventArgs e)
        {
            simpleButton1.Enabled = false;
            progressPanel1.Visible = true;
            progressPanel1.Caption = "Đang chuẩn bị...";

            await Task.Run(() =>
            {
                try
                {
                    string appPath = Assembly.GetExecutingAssembly().Location;
                    string directoryPath = Path.GetDirectoryName(appPath);
                    string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));
                    string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");

                    string dbPath = "";
                    try { dbPath = File.ReadAllText(filePaths); }
                    catch (Exception ex) { Console.WriteLine("Lỗi khi đọc file: " + ex.Message); return; }

                    string password = "1@35^7*9)1";
                    connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";

                    // --- 2. LẤY THÔNG TIN KHÓA SỔ (LICENSE) ---
                    int finalLock = 0;
                    var tbLicense = ExecuteQuery("SELECT * FROM License", null);
                    if (tbLicense.Rows.Count > 0)
                    {
                        var rowLicense = tbLicense.Rows[0];
                        for (int i = 12; i >= 1; i--)
                        {
                            string columnName = "Lock" + i;
                            if (tbLicense.Columns.Contains(columnName) && rowLicense[columnName] != DBNull.Value && rowLicense[columnName].ToString() == "1")
                            {
                                finalLock = i; break;
                            }
                        }
                    }

                    if (checkEdit1.Checked)
                    {
                        TinhGVBHBQ(finalLock, dtDenngay.DateTime.Month, 6, 0, 2, (percent) =>
                        {
                            this.BeginInvoke((MethodInvoker)(() => progressPanel1.Caption = $"Đang lập lại chứng từ giá vốn: {percent}%"));
                        });
                    }

                    // --- 3. XÁC ĐỊNH NGÀY BẮT ĐẦU ---
                    DateTime ngayBatDauTinhToan;
                    int namTinhToan = dtDenngay.DateTime.Year;
                    if (finalLock > 0 && finalLock < 12)
                        ngayBatDauTinhToan = new DateTime(namTinhToan, finalLock + 1, 1);
                    else
                        ngayBatDauTinhToan = new DateTime(namTinhToan, 1, 1);

                    // --- 4. LỌC DANH SÁCH VẬT TƯ CẦN TÍNH ---
                    var qrLocVatTu = @"SELECT DISTINCT MaVattu FROM ChungTu WHERE NgayCT >= @NgayBD_Loc AND NgayCT < @NgayKT_Loc and MaVattu <>0 ";
                    var paramsLoc = new OleDbParameter[] { new OleDbParameter("@NgayBD_Loc", dtTuNgay.DateTime.Date), new OleDbParameter("@NgayKT_Loc", dtDenngay.DateTime.Date.AddDays(1)) };
                    var tbLocVatTu = ExecuteQuery(qrLocVatTu, paramsLoc);

                    var danhSachMaVattuCanTinh = new HashSet<string>(tbLocVatTu.AsEnumerable().Select(r => r["MaVattu"].ToString()));
                    if (danhSachMaVattuCanTinh.Count == 0)
                    {
                        this.Invoke((MethodInvoker)delegate { XtraMessageBox.Show("Không có vật tư nào phát sinh!"); });
                        return;
                    }

                    // --- 5. LẤY CHỨNG TỪ (ĐÃ KHÔI PHỤC ORDER BY CHUẨN) ---
                    var qrTimct = @"SELECT MaSo, MaVattu, ThangCT, SoHieu, MaLoai, SoPS, SoPS2No, SoPS2Co 
                            FROM ChungTu 
                            WHERE NgayCT >= @NgayBD_Tinh AND NgayCT < @NgayKT_NextDay and MaVattu <> 0
                            ORDER BY NgayCT ASC, MaLoai ASC, MaSo ASC"; // <-- Khôi phục đúng logic gốc

                    var paramsTinh = new OleDbParameter[] { new OleDbParameter("@NgayBD_Tinh", ngayBatDauTinhToan), new OleDbParameter("@NgayKT_NextDay", dtDenngay.DateTime.Date.AddDays(1)) };
                    var tbchungtu = ExecuteQuery(qrTimct, paramsTinh);

                    var ctByVt = tbchungtu.AsEnumerable()
                                          .Where(m => !m["SoHieu"].ToString().Contains("GV"))
                                          .ToLookup(m => m["MaVattu"].ToString());

                    // --- 6. LẤY DANH MỤC VẬT TƯ VÀ LỌC ---
                    var tbVattu = ExecuteQuery("SELECT MaSo, SoHieu, TenVatTu FROM Vattu", null);
                    var queryVattu = tbVattu.AsEnumerable().Where(r => danhSachMaVattuCanTinh.Contains(r["MaSo"].ToString()));

                    string soHieuNhap = txtSoHieu.Text.Trim();
                    if (!string.IsNullOrEmpty(soHieuNhap))
                        queryVattu = queryVattu.Where(r => r.Field<string>("SoHieu") != null && r.Field<string>("SoHieu").ToLower().Contains(soHieuNhap.ToLower()));

                    var tbVattuFiltered = queryVattu.ToList();

                    int totalChungTu = tbVattuFiltered.Sum(row => ctByVt[row["MaSo"].ToString()].Count());
                    int currentChungTuCount = 0;

                    var gvUpdates = new List<Tuple<double, string, long>>();
                    var tonKhoUpdates = new List<DataRow>();

                    // --- 7. VÒNG LẶP TÍNH TOÁN ---
                    foreach (DataRow row in tbVattuFiltered)
                    {
                        string maVatTu = row["MaSo"].ToString();
                        string tenVT = row["TenVatTu"].ToString();

                        var tbTonKho = ExecuteQuery("SELECT * FROM TonKho where MaVatTu = @MaVatTu", new OleDbParameter("@MaVatTu", maVatTu));
                        ResetTonkho(tbTonKho, finalLock);
                        if (tbTonKho == null || tbTonKho.Rows.Count == 0) continue;

                        double tienTon = Convert.ToDouble(tbTonKho.Rows[0][$"Tien_{finalLock}"]);
                        double luongTon = Convert.ToDouble(tbTonKho.Rows[0][$"Luong_{finalLock}"]);

                        if (!ctByVt.Contains(maVatTu)) continue;

                        DataRow tbrow = tbTonKho.Rows[0];
                        foreach (DataRow ctRow in ctByVt[maVatTu])
                        {
                            currentChungTuCount++;
                            if (currentChungTuCount % 100 == 0 || currentChungTuCount == totalChungTu)
                            {
                                int progressPercent = (int)((double)currentChungTuCount / totalChungTu * 90);
                                string cap = $"Đang tính ({progressPercent}%) - {Helpers.ConvertVniToUnicode(tenVT)} ({maVatTu})";
                                this.BeginInvoke((MethodInvoker)(() => progressPanel1.Caption = cap));
                            }

                            int thangct = ctRow["ThangCT"] != DBNull.Value ? Convert.ToInt32(ctRow["ThangCT"]) : 0;
                            double MaLoai = Convert.ToDouble(ctRow["MaLoai"]);

                            if (MaLoai == 1)
                            {
                                double SoPS2No = Convert.ToDouble(ctRow["SoPS2No"]);
                                double SoPS = Convert.ToDouble(ctRow["SoPS"]);
                                luongTon += SoPS2No; tienTon += SoPS;
                                AddTo(tbrow, "Luong_Nhap_" + thangct, SoPS2No);
                                AddTo(tbrow, "Tien_Nhap_" + thangct, SoPS);
                                tbrow["Luong_" + thangct] = luongTon; tbrow["Tien_" + thangct] = tienTon;
                            }
                            else if (MaLoai == 8)
                            {
                                if (luongTon <= 0) continue;

                                double SoPS2Co = Convert.ToDouble(ctRow["SoPS2Co"]);
                                double SoPS = (SoPS2Co >= luongTon) ? tienTon : Math.Round(tienTon / luongTon * SoPS2Co, MidpointRounding.AwayFromZero);

                                luongTon -= SoPS2Co; tienTon -= SoPS;
                                AddTo(tbrow, "Luong_Xuat_" + thangct, SoPS2Co);
                                AddTo(tbrow, "Tien_Xuat_" + thangct, SoPS);
                                tbrow["Luong_" + thangct] = luongTon; tbrow["Tien_" + thangct] = tienTon;

                                gvUpdates.Add(Tuple.Create(SoPS, $"{ctRow["SoHieu"]}GV", Convert.ToInt64(ctRow["MaSo"])));
                            }
                        }
                        tonKhoUpdates.Add(tbrow);
                    }

                    // --- 8. LƯU TOÀN BỘ XUỐNG DATABASE BẰNG TRANSACTION (CÓ BÁO % CHI TIẾT) ---
                    this.BeginInvoke((MethodInvoker)(() => progressPanel1.Caption = "Đang cập nhật Database (90%)..."));

                    using (var conn = new OleDbConnection(connectionString))
                    {
                        conn.Open();
                        using (var tran = conn.BeginTransaction())
                        {
                            try
                            {
                                // Tổng số công việc cần làm ở bước 8 (số dòng GV + số dòng Tồn Kho)
                                int totalDbTasks = gvUpdates.Count + tonKhoUpdates.Count;
                                int currentDbTask = 0;

                                // 8.1 Cập nhật bảng ChungTu (Giá vốn)
                                if (gvUpdates.Count > 0)
                                {
                                    using (var cmd = new OleDbCommand("UPDATE ChungTu SET SoPS = ? WHERE SoHieu = ? AND CT_ID LIKE ?", conn, tran))
                                    {
                                        cmd.Parameters.Add(new OleDbParameter("?", OleDbType.Double));
                                        cmd.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar, 255));
                                        cmd.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar, 255));

                                        foreach (var u in gvUpdates)
                                        {
                                            cmd.Parameters[0].Value = u.Item1;
                                            cmd.Parameters[1].Value = u.Item2;
                                            cmd.Parameters[2].Value = $"%{u.Item3}%";
                                            cmd.ExecuteNonQuery();

                                            currentDbTask++;
                                            // Cứ 100 dòng cập nhật UI 1 lần để không làm chậm vòng lặp
                                            if (currentDbTask % 100 == 0 || currentDbTask == totalDbTasks)
                                            {
                                                int percent = 90 + (int)((double)currentDbTask / totalDbTasks * 10);
                                                this.BeginInvoke((MethodInvoker)(() => progressPanel1.Caption = $"Đang cập nhật chứng từ giá vốn: {percent}%"));
                                            }
                                        }
                                    }
                                }

                                // 8.2 Cập nhật bảng TonKho (Sử dụng DataAdapter)
                                if (tonKhoUpdates.Count > 0)
                                {
                                    using (var da = new OleDbDataAdapter("SELECT * FROM TonKho WHERE 1=0", conn))
                                    {
                                        da.SelectCommand.Transaction = tran;
                                        using (var cb = new OleDbCommandBuilder(da))
                                        {
                                            cb.QuotePrefix = "[";
                                            cb.QuoteSuffix = "]";

                                            DataTable dtUpdate = tonKhoUpdates[0].Table.Clone();
                                            foreach (var dr in tonKhoUpdates)
                                            {
                                                dtUpdate.ImportRow(dr);
                                            }

                                            // Cập nhật (DataAdapter sẽ tự động chạy ngầm nhiều lệnh UPDATE)
                                            da.Update(dtUpdate);

                                            // Cập nhật UI cho phần Tồn Kho
                                            currentDbTask += tonKhoUpdates.Count;
                                            int percent = 90 + (int)((double)currentDbTask / totalDbTasks * 10);
                                            this.BeginInvoke((MethodInvoker)(() => progressPanel1.Caption = $"Đang cập nhật tồn kho: {percent}%"));
                                        }
                                    }
                                }

                                tran.Commit();
                            }
                            catch (Exception ex)
                            {
                                tran.Rollback();
                                throw new Exception("Lỗi khi ghi dữ liệu xuống Database: " + ex.Message);
                            }
                        }
                    }

                    this.Invoke((MethodInvoker)delegate { progressPanel1.Caption = "Hoàn tất 100%"; });
                }
                catch (Exception ex)
                {
                    this.Invoke((MethodInvoker)delegate { XtraMessageBox.Show("Lỗi: " + ex.Message); });
                }
            });

            simpleButton1.Enabled = true;
            progressPanel1.Caption = "Sẵn sàng";
            XtraMessageBox.Show("Đã tính giá vốn xong!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}