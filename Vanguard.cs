using ClosedXML.Excel;
using DevExpress.CodeParser;
using DevExpress.Utils;
using DevExpress.Utils.Extensions;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using DevExpress.XtraWaitForm;
using DocumentFormat.OpenXml.Drawing.Charts;
using Newtonsoft.Json;
using SaovietTax.Database;
using SaovietTax.DTO;
using SaovietTax.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.RightsManagement;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SaovietTax.frmMain;
using static SaovietTax.KTHT;
using static SaovietTax.Vanguard;
using DataTable = System.Data.DataTable;
using Size = System.Drawing.Size;
using WinLabel = System.Windows.Forms.Label;

namespace SaovietTax
{
    public partial class Vanguard : DevExpress.XtraEditors.XtraForm
    {
        public class WarningData
        {
            public int Thang { get; set; }
            public string Hoadonthieu { get; set; } 
            public string Importloi { get; set; }
            public string Hangam {  get; set; } 
            public string HethongTK { get; set; }   
            public string HoaDonThua {  get; set; } 
        }
        
        public Vanguard()
        {
            InitializeComponent();
           // this.ShowInTaskbar = false; // 👈 Không hiển thị trên Taskbar
            this.TopMost = true;
            this.Opacity = 0; // Bắt đầu trong suốt

            this.StartPosition = FormStartPosition.Manual;

            // Đặt kích thước form (tuỳ chỉnh) 

            // Đặt form ở góc phải dưới
            this.Location = new Point(
                Screen.PrimaryScreen.WorkingArea.Right - this.Width,
                Screen.PrimaryScreen.WorkingArea.Bottom - this.Height
            );
        }
        string savedPath = "";
        public string password, connectionString;
        public string pathThumuc = "";
        public string dbPath = "";
        List<WarningData> warningDatas = new List<WarningData>();
        public HashSet<(string Mst, string SoHD, string KyHieu, DateTime NLap, int Type)> lookupHoaDonCT { get; }
        = new HashSet<(string Mst, string SoHD, string KyHieu, DateTime NLap, int Type)>();
        DataTable dtChungtu { get; set; }
        DataTable tbImport { get; set; }
        public class HoaDonNhap
        {
            public string SoHD { get; set; }
            public DateTime NLap { get; set; }
            public string KHHD { get; set; }          // Mã khách hàng / Số hiệu HĐ
            public string MST { get; set; }           // Mã số thuế
            public double TienTrcThue { get; set; }   // Tiền trước thuế
            public double TienThue { get; set; }      // Tiền thuế
            public double TongTienTT { get; set; }    // Tổng tiền thanh toán
        }
        public DataTable GetHeThongTK(int thang, int nam)
        {
            DataTable dt = new DataTable();

            // Tạo tên cột động theo tháng
            string colDkNo = $"DuNo_{thang - 1}";  // Dư nợ đầu kỳ
            string colDkCo = $"DuCo_{thang - 1}";  // Dư có đầu kỳ
            string colPsNo = $"No_{thang}";        // Phát sinh Nợ
            string colPsCo = $"Co_{thang}";        // Phát sinh Có
            string colCkNo = $"DuNo_{thang}";      // Dư nợ cuối kỳ
            string colCkCo = $"DuCo_{thang}";      // Dư có cuối kỳ

            string query = $@"
        SELECT DISTINCTROW 
            HeThongTK.SoHieu, 
            HeThongTK.Cap, 
            HeThongTK.Ten, 
            HeThongTK.Kieu, 
            HeThongTK.Loai, 
            HeThongTK.{colDkNo} AS DkNo, 
            HeThongTK.{colDkCo} AS DkCo, 
            HeThongTK.{colPsNo} AS PsNo, 
            HeThongTK.{colPsCo} AS PsCo, 
            HeThongTK.KC_N, 
            HeThongTK.KC_C, 
            HeThongTK.{colCkNo} AS CkNo, 
            HeThongTK.{colCkCo} AS CkCo
        FROM HeThongTK
        WHERE (
            (HeThongTK.MaTC = 0 OR HeThongTK.MaTC = HeThongTK.MaSo) 
            OR (HeThongTK.TK_ID3 MOD 10 >= 1)
        ) 
        AND (HeThongTK.Loai > 0)  
        AND HeThongTK.Cap <= 2 
        AND (
            HeThongTK.{colCkNo} <> 0 
            OR HeThongTK.{colCkCo} <> 0 
            OR HeThongTK.{colPsNo} <> 0 
            OR HeThongTK.{colPsCo} <> 0
        )";

            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    using (OleDbCommand cmd = new OleDbCommand(query, conn))
                    {
                        conn.Open();
                        using (OleDbDataAdapter da = new OleDbDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }

            return dt;
        }
        private void CreateFolder(string path)
        {
            try
            {
                // Kiểm tra đường dẫn có tồn tại không
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                // Tạo danh sách các tháng cần kiểm tra (1-12)
                for (int month = 1; month <= 12; month++)
                {
                    // Tạo tên thư mục với định dạng 2 chữ số (01, 02, ..., 12)
                    string folderName = month.ToString("D1");
                    string folderPath = Path.Combine(path, folderName);

                    // Kiểm tra nếu thư mục chưa tồn tại thì tạo mới
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                        Console.WriteLine($"Đã tạo thư mục: {folderPath}");
                    }
                    else
                    {
                        Console.WriteLine($"Thư mục đã tồn tại: {folderPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tạo thư mục: {ex.Message}");
                throw;
            }
        }
        private System.Windows.Forms.Timer animationTimer;
        private int startY;
        private int endY;
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            bool complete = true;

            // Fade in
            if (this.Opacity < 1)
            {
                this.Opacity += 0.05;
                if (this.Opacity > 1) this.Opacity = 1;
                complete = false;
            }

            // Slide up
            if (this.Location.Y > endY)
            {
                this.Location = new Point(this.Location.X, this.Location.Y - 20);
                if (this.Location.Y < endY) this.Location = new Point(this.Location.X, endY);
                complete = false;
            }

            if (complete)
            {
                animationTimer.Stop();
                animationTimer.Dispose();
            }
        }
        // Sử dụng: 
        private void Vanguard_Load(object sender, EventArgs e)
        {
            // Lấy vị trí từ dưới lên
            startY = Screen.PrimaryScreen.WorkingArea.Height;
            endY = (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 2 + (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 3;

            this.Location = new Point(this.Location.X, startY);

            // Bắt đầu animation
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 5;
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);

            // Xóa phần \bin\Debug để lấy đường dẫn gốc
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
            string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");
            pathThumuc = Path.Combine(rootDirectory);
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
            string query = "SELECT * FROM tbRegister";
            // Tạo mảng tham số với giá trị cho câu lệnh SQL

            var kq = ExecuteQuery(query, null);
            try
            {
                if (kq.Rows.Count > 0)
                {
                    savedPath = kq.Rows[0]["Hoadonpath"].ToString();

                    //Kiểm tra va tạo đầu vào
                    string pVao = Path.Combine(savedPath, $"HD{DateTime.Now.Year}", "HDVao");
                    CreateFolder(pVao);

                    //Kiểm tra va tạo đầu ra
                    string pRa = Path.Combine(savedPath, $"HD{DateTime.Now.Year}", "HDRa");
                    CreateFolder(pRa);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
            //Chạy code tải excel
            // TaiExcel();

            loadCheckdata();
            LoadMessageOld();


        }
        DataTable gettbChungtu;
        private void loadCheckdata()
        { // 1. Query (nên chỉ lấy cột cần thiết)
            gettbChungtu = ExecuteQuery("SELECT MaCT, MaLoai, SoHieu, MaTKTCNo,MaTKTCCo, SoPS,SoPS2No,SoPS2Co,NgayImport,MaVattu FROM ChungTu", null);
            DataTable tbHoadon = ExecuteQuery("SELECT Maso, MaSo, KyHieu, NgayPH, MaKhachHang FROM HoaDon", null);
            DataTable tbKhachHang = ExecuteQuery("SELECT MaSo, MST, Ten FROM KhachHang", null);

            // 2. Dictionary khách hàng
            Dictionary<string, KhachHangInfo> dictKhachHang = new Dictionary<string, KhachHangInfo>();
            foreach (DataRow r in tbKhachHang.Rows)
            {
                string maSo = r["MaSo"].ToString();
                if (!dictKhachHang.ContainsKey(maSo))
                {
                    dictKhachHang.Add(maSo, new KhachHangInfo
                    {
                        MST = r["MST"].ToString(),
                        Ten = r["Ten"].ToString()
                    });
                }
            }

            // 3. Group ChungTu theo MaCT
            Dictionary<int, List<DataRow>> chungTuGroups = new Dictionary<int, List<DataRow>>();
            foreach (DataRow r in gettbChungtu.Rows)
            {
                int maCT = Convert.ToInt32(r["MaCT"]);
                if (!chungTuGroups.ContainsKey(maCT))
                    chungTuGroups[maCT] = new List<DataRow>();

                chungTuGroups[maCT].Add(r);
            }

            // 4. Query join
            DataTable tonghop = ExecuteQuery(@"
    SELECT c.MaCT, c.MaLoai,c.NgayCT,c.NgayGS,c.ThangCT, c.SoHieu,c.NgayImport, h.MaKhachHang,h.KyHieu
    FROM ChungTu c
    INNER JOIN HoaDon h ON c.Maso = h.Maso", null);

            // 5. Xử lý

            HashSet<int> processed = new HashSet<int>();

            foreach (DataRow row in tonghop.Rows)
            {
                int mact = Convert.ToInt32(row["MaCT"]);
                if (processed.Contains(mact))
                    continue;

                processed.Add(mact);

                string maLoai = row["MaLoai"].ToString();

                ChungTuHD item = new ChungTuHD();
                item.MaCT = mact;
                item.SoHieu = row["SoHieu"].ToString();
                if (DateTime.TryParseExact(
          row["NgayImport"]?.ToString(),
          "dd/MM/yyyy",
          CultureInfo.InvariantCulture,
          DateTimeStyles.None,
          out DateTime ngayImport))
                {
                    item.NgayImport = ngayImport;
                }
                else
                {
                    item.NgayImport = DateTime.MinValue;
                }
                if (item.SoHieu == "6111")
                {
                    int test = 10;
                }
                item.KHHD = row["KyHieu"].ToString();
                item.NgayCT = DateTime.Parse(row["NgayCT"].ToString());
                // Type
                if (maLoai == "0" || maLoai == "1")
                    item.Type = 1;
                else if (maLoai == "8")
                    item.Type = 2;
                else
                    item.Type = 0;

                // Khách hàng
                string maKH = row["MaKhachHang"].ToString();
                if (dictKhachHang.ContainsKey(maKH))
                {
                    item.MST = dictKhachHang[maKH].MST;
                    item.TenKH = dictKhachHang[maKH].Ten;
                }

                // Tính tiền chỉ khi MaLoai = 8
                if (maLoai == "8" && chungTuGroups.ContainsKey(mact))
                {
                    double tienTrcThue = 0;
                    double tienThue = 0;

                    List<DataRow> rows = chungTuGroups[mact];
                    for (int i = 0; i < rows.Count; i++)
                    {
                        DataRow r = rows[i];
                        double soPS = Convert.ToDouble(r["SoPS"]);
                        string maTK = r["MaTKTCCo"].ToString();
                        string matkno = r["MaTKTCNo"].ToString();
                        double SoPS2Co = Convert.ToDouble(r["SoPS2Co"]);
                        if (maTK == "14038")
                            tienThue += soPS;
                        else
                        {
                            if (soPS > 0)
                            {
                                if (matkno != "169" && SoPS2Co > 0)
                                {
                                    tienTrcThue += soPS;
                                }
                            } 

                        }
                    }

                    item.TienTrcThue = tienTrcThue;
                    item.TienThue = tienThue;
                    item.TongTien = tienTrcThue + tienThue;
                }
                if ((maLoai == "0" || maLoai == "1") && chungTuGroups.ContainsKey(mact))
                {
                    double tienTrcThue = 0;
                    double tienThue = 0;

                    List<DataRow> rows = chungTuGroups[mact];
                    for (int i = 0; i < rows.Count; i++)
                    {
                        DataRow r = rows[i];
                        double soPS = Convert.ToDouble(r["SoPS"]);
                        string maTK = r["MaTKTCNo"].ToString();
                        string matkco = r["MaTKTCCo"].ToString();
                        double SoPS2No = Convert.ToDouble(r["SoPS2No"]);
                        if (maTK == "5108")
                            tienThue += soPS;
                        else
                        {
                            if (soPS > 0)
                            {
                                if (matkco != "169" && (SoPS2No > 0))
                                {
                                    tienTrcThue += soPS;
                                }
                                else
                                {
                                    if (maTK == "161" || maTK == "160")
                                    {
                                        tienTrcThue += soPS;
                                    }
                                }
                                if (matkco == "169")
                                {
                                    tienTrcThue -= soPS;
                                }
                            }

                        }


                    }

                    item.TienTrcThue = tienTrcThue;
                    item.TienThue = tienThue;
                    item.TongTien = tienTrcThue + tienThue;
                }
                lstChungTuHD.Add(item);
            }
        }
        List<ChungTuHD> lstChungTuHD = new List<ChungTuHD>();
        DevExpress.XtraEditors.LabelControl lblThongBao;
        // ===== Class Warning =====
        public class Warning
        {
            public string Text { get; set; }
            public Color Color { get; set; }

            public Warning() { }

            public Warning(string text, Color color)
            {
                Text = text;
                Color = color;
            }
        }

        // ===== Class MonthWarning =====
        public class MonthWarning
        {
            public string Month { get; set; }
            public int Total { get; set; }
            public List<Warning> Warnings { get; set; } = new List<Warning>();

            public MonthWarning() { }

            public MonthWarning(string month, List<Warning> warnings)
            {
                Month = month;
                Warnings = warnings;
                Total = warnings?.Count ?? 0;
            }
        }
        public class VattuAm
        {
            public string MaVT { get; set; }
            public string TenVT { get; set; }
            public string MaPL { get; set; }
        }
        List<VattuAm> vattuAms { get; set; }
        private int MeasureRowHeight(string text, int width, int minHeight)
        {
            if (string.IsNullOrEmpty(text))
                return minHeight;

            Size proposed = new Size(width, int.MaxValue);

            // ⭐ Dùng font khớp với font của label warning
            using (var font = new Font("Segoe UI", 9F))
            {
                Size measured = TextRenderer.MeasureText(
                    text,
                    font,
                    proposed,
                    TextFormatFlags.WordBreak | TextFormatFlags.NoPadding
                );

                int h = measured.Height + 10;   // padding trên dưới
                return Math.Max(h, minHeight);
            }
        }
        private void LoadMessageOld()
        {
            // ============================================================
            // 1. LOAD CHỨNG TỪ + BUILD LOOKUP
            // ============================================================
            string queryct = @"
                SELECT 
                    hd.SoHD, hd.KyHieu, hd.NgayPH, hd.MaKhachHang,
                    kh.MST, ct.NgayCT, ct.MaLoai
                FROM 
                    ((Hoadon hd 
                    INNER JOIN Chungtu ct ON hd.MaSo = ct.MaSo)
                    INNER JOIN KhachHang kh ON hd.MaKhachHang = kh.MaSo)
                WHERE hd.KyHieu <> '...'";

            dtChungtu = ExecuteQuery(queryct);

            lookupHoaDonCT.Clear();

            foreach (DataRow item in dtChungtu.Rows)
            {
                string soHD = Helpers.RemoveLeadingZeros(item["SoHD"]?.ToString() ?? "")
                                      .Replace(".", "").Trim();
                string kyHieu = item["KyHieu"]?.ToString() ?? "";
                DateTime ngayPH = ((DateTime)item["NgayCT"]).Date;
                string mst = item["MST"]?.ToString() ?? "";
                int maLoai = int.Parse(item["MaLoai"]?.ToString() ?? "0");
                int type = (maLoai == 8) ? 2 : 1;

                lookupHoaDonCT.Add((mst, soHD, kyHieu, ngayPH, type));
            }

            // Dictionary tra cứu nhanh chứng từ
            var chungTuDict = lstChungTuHD
                .GroupBy(m => $"{Helpers.RemoveLeadingZeros(m.SoHieu).TrimEnd('.')}|{m.KHHD}")
                .ToDictionary(g => g.Key, g => g.First());

            // ============================================================
            // 2. LOAD DATA TĨNH
            // ============================================================
            var tbImport = ExecuteQuery("SELECT * FROM tbImport");
            var dtTonKho = ExecuteQuery("SELECT * FROM TonKho");
            var dtVattu = ExecuteQuery("SELECT * FROM Vattu");
            var phanloaivt = ExecuteQuery("SELECT * FROM PhanLoaiVattu");

            var vattuDict = dtVattu.AsEnumerable()
                .GroupBy(r => r["MaSo"].ToString())
                .ToDictionary(g => g.Key, g => g.First());

            var phanLoaiDict = phanloaivt.AsEnumerable()
                .GroupBy(r => r["MaSo"].ToString())
                .ToDictionary(g => g.Key, g => g.First()["SoHieu"].ToString());

            // ============================================================
            // 3. XỬ LÝ TỪNG THÁNG (mới nhất → cũ nhất)
            // ============================================================
            var months = new List<MonthWarning>();
            int currentMonth = DateTime.Now.Month;
            int currentYear = DateTime.Now.Year;
            string pathYear = $"HD{currentYear}";

            for (int i = currentMonth; i >= 1; i--)
            {
                
                //Kiểm tra xem có file excel vào ra ko
                string dirVaos = Path.Combine(savedPath, pathYear, "HDVao", i.ToString());
                if (Directory.Exists(dirVaos))
                {
                    var filesVao = Directory.EnumerateFiles(dirVaos, "*.xlsx", SearchOption.AllDirectories);
                    int count = filesVao.Count();
                    if (count == 0)
                        continue;
                }


                vattuAms = new List<VattuAm>(); // reset mỗi tháng

                var monthData = new MonthWarning
                {
                    Month = $"{i:00}/{currentYear}",
                    Warnings = new List<Warning>()
                };

                // ---------- 3.1 TỒN KHO ÂM ----------
                string hangam = "";
                string columnName = $"Luong_{i}";

                foreach (DataRow row in dtTonKho.Rows)
                {
                    object value = row[columnName];
                    if (value == null || value == DBNull.Value) continue;

                    double soLuong = Convert.ToDouble(value);
                    if (soLuong >= 0) continue;

                    string maVT = row["MaVatTu"].ToString();
                    if (!vattuDict.TryGetValue(maVT, out var getvattu)) continue;

                    hangam += getvattu["SoHieu"] + ",";

                    vattuAms.Add(new VattuAm
                    {
                        MaVT = getvattu["SoHieu"].ToString(),
                        TenVT = getvattu["TenVattu"].ToString(),
                        MaPL = phanLoaiDict.TryGetValue(getvattu["MaPhanLoai"].ToString(), out var pl)
                                ? pl : ""
                    });
                }

                // ---------- 3.2 TÀI KHOẢN ----------
                var result = GetHeThongTK(i, currentYear);

                decimal sumDkNo = 0, sumDkCo = 0, sumPsNo = 0,
                        sumPsCo = 0, sumCkNo = 0, sumCkCo = 0;

                foreach (DataRow m in result.Rows)
                {
                    if (m["Cap"].ToString() != "0") continue;

                    if (m["DkNo"] != DBNull.Value && m["DkNo"] != null) sumDkNo += Convert.ToDecimal(m["DkNo"]);
                    if (m["DkCo"] != DBNull.Value && m["DkCo"] != null) sumDkCo += Convert.ToDecimal(m["DkCo"]);
                    if (m["PsNo"] != DBNull.Value && m["PsNo"] != null) sumPsNo += Convert.ToDecimal(m["PsNo"]);
                    if (m["PsCo"] != DBNull.Value && m["PsCo"] != null) sumPsCo += Convert.ToDecimal(m["PsCo"]);
                    if (m["CkNo"] != DBNull.Value && m["CkNo"] != null) sumCkNo += Convert.ToDecimal(m["CkNo"]);
                    if (m["CkCo"] != DBNull.Value && m["CkCo"] != null) sumCkCo += Convert.ToDecimal(m["CkCo"]);
                }

                // ---------- 3.3 ĐỌC EXCEL ----------
                var lstVao = new List<HoaDonNhap>();
                var lstRa = new List<HoaDonNhap>();

                // --- HĐ vào ---
                string dirVao = Path.Combine(savedPath, pathYear, "HDVao", i.ToString());
                if (Directory.Exists(dirVao))
                {
                    var filesVao = Directory.EnumerateFiles(dirVao, "*.xlsx", SearchOption.AllDirectories);
                    foreach (var excelFile in filesVao)
                    {
                        try
                        {
                            using (var workbook = new XLWorkbook(excelFile))
                            {
                                var ws = workbook.Worksheet(1);
                                foreach (var row in ws.RowsUsed().Skip(3))
                                {
                                    try
                                    {
                                        string khhd = row.Cell("B").Value.ToString();
                                        string shhd = row.Cell("C").Value.ToString();
                                        string sohd = Helpers.RemoveLeadingZeros(row.Cell("D").Value.ToString());
                                        string nLapStr = row.Cell("E").Value.ToString();
                                        string mstnb = row.Cell("F").Value.ToString();

                                        if (!DateTime.TryParse(nLapStr, out DateTime nLap)) continue;

                                        double tienTrcThue = 0, tienThue = 0, tongTienTT = 0;

                                        if (excelFile.Contains("MayTinhTien"))
                                        {
                                            tienTrcThue = ParseMoney(row.Cell("L").Value.ToString());
                                            tienThue = ParseMoney(row.Cell("M").Value.ToString());
                                            tongTienTT = ParseMoney(row.Cell("O").Value.ToString());
                                        }
                                        else
                                        {
                                            tienTrcThue = ParseMoney(row.Cell("K").Value.ToString());
                                            tienThue = ParseMoney(row.Cell("L").Value.ToString());
                                            tongTienTT = ParseMoney(row.Cell("O").Value.ToString());
                                        }

                                        lstVao.Add(new HoaDonNhap
                                        {
                                            SoHD = sohd,
                                            NLap = nLap,
                                            KHHD = shhd,
                                            MST = mstnb,
                                            TienTrcThue = tienTrcThue,
                                            TienThue = tienThue,
                                            TongTienTT = tongTienTT
                                        });
                                    }
                                    catch { }
                                }
                            }
                        }
                        catch { }
                    }
                }

                // --- HĐ ra ---
                string dirRa = Path.Combine(savedPath, pathYear, "HDRa", i.ToString());
                if (Directory.Exists(dirRa))
                {
                    var filesRa = Directory.EnumerateFiles(dirRa, "*.xlsx", SearchOption.AllDirectories);
                    foreach (var excelFile in filesRa)
                    {
                        try
                        {
                            using (var workbook = new XLWorkbook(excelFile))
                            {
                                var ws = workbook.Worksheet(1);
                                foreach (var row in ws.RowsUsed().Skip(3))
                                {
                                    try
                                    {
                                        string getkhhd = row.Cell("C").Value.ToString();
                                        string sohd = Helpers.RemoveLeadingZeros(row.Cell("D").Value.ToString());
                                        string nLapStr = row.Cell("E").Value.ToString();
                                        string mstnm = row.Cell("H").Value.ToString();

                                        if (!DateTime.TryParse(nLapStr, out DateTime nLap)) continue;

                                        double tienTrcThue = ParseMoney(row.Cell("L").Value.ToString());
                                        double tienThue = ParseMoney(row.Cell("M").Value.ToString());
                                        double tongTienTT = ParseMoney(row.Cell("O").Value.ToString());

                                        lstRa.Add(new HoaDonNhap
                                        {
                                            SoHD = sohd,
                                            NLap = nLap,
                                            KHHD = getkhhd,
                                            MST = mstnm,
                                            TienTrcThue = tienTrcThue,
                                            TienThue = tienThue,
                                            TongTienTT = tongTienTT
                                        });
                                    }
                                    catch { }
                                }
                            }
                        }
                        catch { }
                    }
                }

                // ---------- 3.4 ĐỐI CHIẾU ----------


                //Mã hàng bị mất
                foreach (var hd in lstVao)
                {

                }

                    int hdChuaNhapVao = 0, hdChuaNhapRa = 0, hdSaiThongTin = 0,hdNullhang=0;
                string dsHdVao = "", dsHdRa = "", dsSai = "",dsNullhang="";

                foreach (var hd in lstVao)
                { 
                    if (hd.SoHD == "12856")
                    {
                        int yest = 10;
                    }
                    if (!KiemtrahoadonCT(hd.SoHD, hd.KHHD, hd.NLap, hd.MST, 1))
                    {
                        hdChuaNhapVao++;
                        dsHdVao += hd.SoHD + ",";
                    }

                    string key = $"{Helpers.RemoveLeadingZeros(hd.SoHD).TrimEnd('.')}|{hd.KHHD}";
                    if (chungTuDict.TryGetValue(key, out var ct))
                    {
                        //Kiểm tra hàng ko ma
                        var getlist = gettbChungtu.AsEnumerable().Where(m => m["MaCT"].ToString() == ct.MaCT.ToString());
                        var checknullhang = getlist.AsEnumerable().Any(m => (m["SoPS2No"].ToString() != "0" || m["SoPS2Co"].ToString() != "0") && m["SoPS"].ToString() != "0" && m["MaVattu"].ToString()=="0");
                        if (checknullhang)
                        {
                            hdNullhang ++;
                            dsNullhang += $"[{hd.SoHD}](v) , "; 
                        }
                        string lyDo = CompareHoaDon(hd, ct);
                        if (lyDo != null)
                        {
                            hdSaiThongTin++;
                            dsSai += $"[{hd.SoHD}](v)({lyDo}), ";
                        }
                    }
                }

                foreach (var hd in lstRa)
                {
                    if (!KiemtrahoadonCT(hd.SoHD, hd.KHHD, hd.NLap, hd.MST, 2))
                    {
                        hdChuaNhapRa++;
                        dsHdRa += hd.SoHD + ",";
                    }

                    string key = $"{Helpers.RemoveLeadingZeros(hd.SoHD).TrimEnd('.')}|{hd.KHHD}";
                    if (chungTuDict.TryGetValue(key, out var ct))
                    {
                        //Kiểm tra hàng ko ma
                        var getlist = gettbChungtu.AsEnumerable().Where(m => m["MaCT"].ToString() == ct.MaCT.ToString());
                        var checknullhang = getlist.AsEnumerable().Any(m => (m["SoPS2No"].ToString() != "0" || m["SoPS2Co"].ToString() != "0") && m["SoPS"].ToString() != "0" && m["MaVattu"].ToString() == "0");
                        if (checknullhang)
                        {
                            hdNullhang++;
                            dsNullhang += $"{hd.SoHD}(r), ";
                        }
                        string lyDo = CompareHoaDon(hd, ct);
                        if (lyDo != null)
                        {
                            hdSaiThongTin++;
                            dsSai += $"{hd.SoHD}(r)({lyDo}), ";
                        }
                    }
                }

                var dsNhapDuVao = lookupHoaDonCT
                    .Where(m => m.NLap.Month == i && m.Type == 1)
                    .Where(m => !lstVao.Any(x => x.SoHD == m.SoHD))
                    .Select(m => m.SoHD)
                    .ToList();

                var dsNhapDuRa = lookupHoaDonCT
                    .Where(m => m.NLap.Month == i && m.Type == 2)
                    .Where(m => !lstRa.Any(x => x.SoHD == m.SoHD))
                    .Select(m => m.SoHD)
                    .ToList();

                // ---------- 3.5 IMPORT LỖI ----------
                var getimportloi = tbImport.AsEnumerable()
                    .Where(m => m.Field<DateTime>("NLap").Date.Month == i)
                    .Where(m => m["Status"].ToString() == "2")
                    .ToList();

                // ---------- 3.6 WARNING ----------
                if (hdChuaNhapVao > 0 || hdChuaNhapRa > 0)
                {
                    var parts = new List<string>();
                    if (hdChuaNhapVao > 0) parts.Add($"{hdChuaNhapVao} HĐ đầu vào");
                    if (hdChuaNhapRa > 0) parts.Add($"{hdChuaNhapRa} HĐ đầu ra");

                    monthData.Warnings.Add(new Warning
                    {
                        Text = $"🔴 {string.Join(" , ", parts)} chưa nhập",
                        Color = Color.FromArgb(220, 53, 69)
                    });
                }

                if (hdSaiThongTin > 0)
                {
                    monthData.Warnings.Add(new Warning
                    {
                        Text = $"🟠 {hdSaiThongTin} hóa đơn sai thông tin: {dsSai.TrimEnd(',')}",
                        Color = Color.DarkBlue
                    });
                }
                if (hdNullhang > 0)
                {
                    monthData.Warnings.Add(new Warning
                    {
                        Text = $"🟠 {hdNullhang} hoá đơn bị thiếu mã hàng : {dsNullhang.TrimEnd(',')}",
                        Color = Color.DarkMagenta
                    });
                }
                if (getimportloi.Count > 0)
                {
                    var listimportloi = string.Join(",", getimportloi.Select(r => r["SHDon"].ToString()));
                    monthData.Warnings.Add(new Warning
                    {
                        Text = $"🔵 {getimportloi.Count} import lỗi: {listimportloi}",
                        Color = Color.FromArgb(13, 110, 253)
                    });
                }

                

                if (sumDkNo != sumDkCo)
                {
                    monthData.Warnings.Add(new Warning
                    {
                        Text = $"🔵 Số dư đầu kỳ chưa cân: {sumDkNo} - {sumDkCo}",
                        Color = Color.FromArgb(23, 162, 184)
                    });
                }

                if (sumPsNo != sumPsCo)
                {
                    monthData.Warnings.Add(new Warning
                    {
                        Text = $"🔵 Số dư trong kỳ chưa cân: {sumPsNo} - {sumPsCo}",
                        Color = Color.FromArgb(40, 167, 69)
                    });
                }

                if (dsNhapDuVao.Count > 0)
                {
                    monthData.Warnings.Add(new Warning
                    {
                        Text = $"🟣 HĐ đầu vào nhập dư: {string.Join("  ", dsNhapDuVao.Select(x => $"[{x}]"))}",
                        Color = Color.DarkOrange
                    });
                }

                if (dsNhapDuRa.Count > 0)
                {
                    monthData.Warnings.Add(new Warning
                    {
                        Text = $"🟣 HĐ đầu ra nhập dư: {string.Join(",", dsNhapDuRa.Select(x => $"[{x}]"))}",
                        Color = Color.FromArgb(111, 66, 193)
                    });
                }
                //if (!string.IsNullOrEmpty(hangam))
                //{
                //    var group = vattuAms.GroupBy(m => m.MaPL);
                //    foreach (var g in group)
                //    {
                //        monthData.Warnings.Add(new Warning
                //        {
                //            Text = $"🟡 Nhóm {g.Key} có {g.Count()} ⚠️ hàng đang âm",
                //            Color = Color.DarkGreen
                //        });
                //    }
                //}
                if (!string.IsNullOrEmpty(hangam))
                {
                    // Bỏ dấu phẩy cuối
                    var dsMaHang = hangam.TrimEnd(',');

                    monthData.Warnings.Add(new Warning
                    {
                        Text = $"🟡 Có {vattuAms.Count} ⚠️ mặt hàng đang âm: {dsMaHang}",
                        Color = Color.Green
                    });
                }
                months.Add(monthData);
            }

            // ============================================================
            // 4. RENDER GIAO DIỆN
            // ============================================================
            panelControl1.Controls.Clear();
            panelControl1.BackColor = Color.FromArgb(245, 247, 250);

            var panelScroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 247, 250)
            };
            panelControl1.Controls.Add(panelScroll);

            int yPos = 10;
            const int leftMargin = 10;
            const int rightMargin = 26;

            panelScroll.CreateControl();

            int panelWidth = panelScroll.ClientSize.Width - leftMargin - rightMargin;
            if (panelWidth < 200) panelWidth = 200;

            // months[0] = tháng hiện tại → hiển thị trên cùng
            foreach (var monthData in months)
            {
                // ====================================================
                // CARD
                // ====================================================
                var card = new Panel
                {
                    Width = panelWidth,
                    BackColor = Color.White,
                    Location = new Point(leftMargin, yPos)
                };

                card.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    using (var shadow = new SolidBrush(Color.FromArgb(18, 0, 0, 0)))
                        g.FillRectangle(shadow, new Rectangle(2, 2, card.Width - 4, card.Height - 4));

                    using (var pen = new Pen(Color.FromArgb(225, 228, 232), 1))
                        g.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                };

                bool hasWarning = monthData.Warnings.Count > 0;

                // ====================================================
                // HEADER
                // ====================================================
                int headerHeight = 52;

                var header = new Panel
                {
                    Location = new Point(0, 0),
                    Width = panelWidth,
                    Height = headerHeight,
                    BackColor = hasWarning
                        ? Color.FromArgb(255, 250, 250)
                        : Color.FromArgb(248, 255, 248)
                };

                // Icon lịch
                var icon = new DevExpress.XtraEditors.SvgImageBox
                {
                    Location = new Point(14, 12),
                    Size = new Size(28, 28),
                    SvgImage = GetCalendarSvg(hasWarning)
                };
                header.Controls.Add(icon);

                // Label tháng
                var lblMonth = new WinLabel
                {
                    Text = monthData.Month,
                    Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(33, 37, 41),
                    Location = new Point(52, 14),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };
                header.Controls.Add(lblMonth);

                // Badge
                if (hasWarning)
                {
                    var badge = new WinLabel
                    {
                        Text = $"  {monthData.Warnings.Count} cảnh báo  ",
                        Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                        ForeColor = Color.White,
                        BackColor = Color.FromArgb(220, 53, 69),
                        Location = new Point(52 + lblMonth.PreferredWidth + 14, 17),
                        AutoSize = true,
                        Padding = new Padding(4, 2, 4, 2)
                    };
                    header.Controls.Add(badge);
                }
                else
                {
                    var ok = new WinLabel
                    {
                        Text = "✓ Không có cảnh báo",
                        Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                        ForeColor = Color.FromArgb(40, 167, 69),
                        Location = new Point(52 + lblMonth.PreferredWidth + 14, 18),
                        AutoSize = true,
                        BackColor = Color.Transparent
                    };
                    header.Controls.Add(ok);
                }

                // ====================================================
                // BODY — đo chiều cao từng row thủ công
                // ====================================================
                var body = new FlowLayoutPanel
                {
                    Location = new Point(0, headerHeight),
                    Width = panelWidth,
                    AutoSize = false,                    // ⭐ tắt AutoSize
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    Padding = new Padding(14, 8, 14, 12),
                    BackColor = Color.White
                };

                int warningWidth = panelWidth - 28 - 12;      // trừ padding + thanh màu
                int bodyY = 0;

                foreach (var w in monthData.Warnings)
                {
                    int rowHeight = MeasureRowHeight(w.Text, warningWidth, minHeight: 26);

                    var row = new Panel
                    {
                        Location = new Point(0, bodyY),
                        Width = panelWidth - 28,
                        Height = rowHeight,
                        BackColor = Color.Transparent
                    };

                    var bar = new Panel
                    {
                        Location = new Point(0, 0),
                        Size = new Size(4, rowHeight),
                        BackColor = w.Color
                    };
                    row.Controls.Add(bar);

                    var lbl = new WinLabel
                    {
                        Text = w.Text,
                        Font = new Font("Segoe UI", 9F),
                        ForeColor = w.Color,
                        Location = new Point(8, 4),
                        Size = new Size(warningWidth, rowHeight - 8),
                        AutoSize = false,                    // ⭐ tắt AutoSize
                        BackColor = Color.Transparent
                    };
                    row.Controls.Add(lbl);

                    body.Controls.Add(row);
                    bodyY += rowHeight + 4;
                }

                body.Height = bodyY + body.Padding.Top + body.Padding.Bottom;

                // ====================================================
                // ADD VÀO CARD
                // ====================================================
                card.Controls.Add(header);
                card.Controls.Add(body);

                card.Height = headerHeight + body.Height;
                card.Width = panelWidth;

                panelScroll.Controls.Add(card);

                yPos += card.Height + 12;
            }

            panelScroll.AutoScrollMinSize = new Size(0, yPos + 10);
            panelScroll.HorizontalScroll.Enabled = false;
            panelScroll.HorizontalScroll.Visible = false;
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        private static double ParseMoney(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;
            return double.TryParse(s, out double v) ? Math.Round(v) : 0;
        }

        private static string CompareHoaDon(HoaDonNhap hd, ChungTuHD ct)
        {
            var loi = new List<string>();

            if (hd.TienTrcThue != 0 && hd.TienTrcThue != ct.TienTrcThue)
                loi.Add("Tiền trước thuế bị lệch");

            if (hd.TienThue != 0 && hd.TienThue != ct.TienThue)
                loi.Add("Tiền thuế bị lệch");

            if (ct.NgayCT.Date != hd.NLap.Date && loi.Count == 0)
                loi.Add("Ngày chứng từ bị sai");

            return loi.Count > 0 ? string.Join("; ", loi) : null;
        }

        private static SvgImage GetCalendarSvg(bool hasWarning)
        {
            string color = hasWarning ? "#DC3545" : "#28A745";
            string svg = $@"
<svg xmlns='http://www.w3.org/2000/svg' width='24' height='24' viewBox='0 0 24 24'>
  <rect x='3' y='4' width='18' height='18' rx='3' fill='none' stroke='{color}' stroke-width='2'/>
  <line x1='3' y1='10' x2='21' y2='10' stroke='{color}' stroke-width='2'/>
  <line x1='8' y1='2' x2='8' y2='6' stroke='{color}' stroke-width='2'/>
  <line x1='16' y1='2' x2='16' y2='6' stroke='{color}' stroke-width='2'/>
</svg>";

            using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(svg)))
                return DevExpress.Utils.Svg.SvgImage.FromStream(ms);
        }

        private void LoadMeaasgeold2()
        {
            vattuAms = new List<VattuAm>();
            string queryct = @"
                SELECT 
                    hd.SoHD,
                    hd.KyHieu,
                    hd.NgayPH,
                    hd.MaKhachHang,
                    kh.MST, 
                    ct.NgayCT,
                    ct.MaLoai
                FROM 
                    ((Hoadon hd 
                    INNER JOIN 
                    Chungtu ct ON hd.MaSo = ct.MaSo)
                    INNER JOIN 
                    KhachHang kh ON hd.MaKhachHang = kh.MaSo)
                WHERE 
                    hd.KyHieu <> '...'";

            dtChungtu = ExecuteQuery(queryct);

            lookupHoaDonCT.Clear();

            foreach (DataRow item in dtChungtu.Rows)
            {
                string soHD = Helpers.RemoveLeadingZeros(
                    item["SoHD"]?.ToString() ?? ""
                ).Trim();
                string KyHieu = item["KyHieu"]?.ToString() ?? "";
                DateTime ngayPH = ((DateTime)item["NgayCT"]).Date;

                int maKhachHang = (int)item["MaKhachHang"];

                string mst = item["MST"]?.ToString() ?? "";
                int Maloai = int.Parse(item["MaLoai"]?.ToString());
                if (Maloai == 8)
                {
                    Maloai = 2;
                }
                else
                {
                    Maloai = 1;
                }
                soHD = soHD.Replace(".", "").Trim();
                soHD = RemoveLeadingZeros(soHD);
                lookupHoaDonCT.Add((mst, soHD, KyHieu, ngayPH, Maloai));
            }


            string qrip = "SELECT * FROM tbImport";
            tbImport = ExecuteQuery(qrip);
            string qrtonkho = "select * from TonKho";
            var dtTonKho = ExecuteQuery(qrtonkho);
            string qrvt = "select * from Vattu";
            var dtVattu = ExecuteQuery(qrvt);
            string hangam = "";
            qrip = "SELECT * FROM PhanLoaiVattu";
            var phanloaivt = ExecuteQuery(qrip);
            var months = new List<MonthWarning>();
            for (int i = DateTime.Now.Month; i >= 1; i--)
            {
                var result = GetHeThongTK(i, DateTime.Now.Year);
                // Tổng DkNo
                var sumDkNo = result.AsEnumerable()
                    .Where(m => m["DkNo"] != DBNull.Value
                                && m["DkNo"] != null
                                && m["Cap"].ToString() == "0")
                    .Sum(m => Convert.ToDecimal(m["DkNo"]));

                // Tổng DkCo
                var sumDkCo = result.AsEnumerable()
                    .Where(m => m["DkCo"] != DBNull.Value
                                && m["DkCo"] != null
                                && m["Cap"].ToString() == "0")
                    .Sum(m => Convert.ToDecimal(m["DkCo"]));

                // Tổng PsNo
                var sumPsNo = result.AsEnumerable()
                    .Where(m => m["PsNo"] != DBNull.Value
                                && m["PsNo"] != null
                                && m["Cap"].ToString() == "0")
                    .Sum(m => Convert.ToDecimal(m["PsNo"]));

                // Tổng PsCo
                var sumPsCo = result.AsEnumerable()
                    .Where(m => m["PsCo"] != DBNull.Value
                                && m["PsCo"] != null
                                && m["Cap"].ToString() == "0")
                    .Sum(m => Convert.ToDecimal(m["PsCo"]));

                // Tổng CkNo
                var sumCkNo = result.AsEnumerable()
                    .Where(m => m["CkNo"] != DBNull.Value
                                && m["CkNo"] != null
                                && m["Cap"].ToString() == "0")
                    .Sum(m => Convert.ToDecimal(m["CkNo"]));

                // Tổng CkCo
                var sumCkCo = result.AsEnumerable()
                    .Where(m => m["CkCo"] != DBNull.Value
                                && m["CkCo"] != null
                                && m["Cap"].ToString() == "0")
                    .Sum(m => Convert.ToDecimal(m["CkCo"]));

                //Kiểm tra tồn kho
                hangam = "";
                string columnName = $"Luong_{i}";
                foreach (DataRow row in dtTonKho.Rows)
                {
                    // Lấy giá trị theo tên cột động
                    object value = row[columnName];
                    if (value != DBNull.Value && value != null)
                    {
                        double soLuong = Convert.ToDouble(value);
                        if (soLuong < 0)
                        {
                            var getvattu = dtVattu.AsEnumerable().Where(m => m["MaSo"].ToString() == row["MaVatTu"].ToString()).FirstOrDefault();
                            if (getvattu != null)
                            {
                                hangam += getvattu["SoHieu"] + ",";
                                VattuAm VattuAm = new VattuAm();
                                VattuAm.MaVT = getvattu["SoHieu"].ToString();
                                VattuAm.TenVT = getvattu["TenVattu"].ToString();
                                VattuAm.MaPL = phanloaivt.AsEnumerable().Where(m => m["MaSo"].ToString() == getvattu["MaPhanLoai"].ToString()).FirstOrDefault()["SoHieu"].ToString();
                                vattuAms.Add(VattuAm);
                            }
                        }
                    }
                }


                List<HoaDonNhap> lstvao = new List<HoaDonNhap>();
                List<HoaDonNhap> lstRa = new List<HoaDonNhap>();
                int hdchuanhapvao = 0;
                string dshdvaochunhap = "";
                int hdchuanhapra = 0;
                string dshdrachunhap = "";
                int tongvao = 0;
                int tongra = 0;
                string hdnhapduDauvao = "";
                string hdnhapduDaura = "";

                string dshdsaithongtin = "";
                int hdsaithongtin = 0;

                var qr = tbImport.AsEnumerable()
                .Where(m => m.Field<DateTime>("NLap").Date.Month == i)
                .Where(m => m["Status"].ToString() == "2");

                DataTable getimportloi = new DataTable();
                if (qr.Any())
                {
                    getimportloi = qr.CopyToDataTable();
                }
                else
                {
                    // Tạo DataTable rỗng với cấu trúc giống tbImport
                    getimportloi = tbImport.Clone();
                }
                string pathYear = $"HD{DateTime.Now.Year}";

                //Đọc  hoá đơn đầu vào
                string directoryPath2 = Path.Combine(savedPath, pathYear, "HDVao", i.ToString());

                var excelFiles = Directory.EnumerateFiles(directoryPath2, "*.xlsx", SearchOption.AllDirectories).ToList();
                if (excelFiles.Count == 0)
                    continue;
                int j = 1;
                foreach (var excelFile in excelFiles)
                {
                    using (var workbook = new XLWorkbook(excelFile))

                    {
                        var worksheet = workbook.Worksheet(1); // Lấy sheet đầu tiên
                        foreach (var row in worksheet.RowsUsed().Skip(3)) // Bỏ qua 6 hàng đầu tiên
                        {
                            try
                            {
                                string khhd = row.Cell("B").Value.ToString(); // Lấy giá trị của cột A trong hàng hiện tại
                                string getSHHD = row.Cell("C").Value.ToString(); // Lấy giá trị của cột A trong hàng hiện tại
                                string getSohd = Helpers.RemoveLeadingZeros(row.Cell("D").Value.ToString()); // Lấy giá trị của cột C trong hàng hiện tại 
                                string GetNLap = row.Cell("E").Value.ToString();
                                string mstnb = row.Cell("F").Value.ToString();
                                if (getSohd == "12856")
                                {
                                    int asdsd = 10;
                                }

                                double TienTrcThue = 0;
                                double TienThue = 0;
                                double TongTienTT = 0;
                                if (excelFile.Contains("MayTinhTien"))
                                {
                                    if (!string.IsNullOrEmpty(row.Cell("L").Value.ToString()))
                                        TienTrcThue = Math.Round(double.Parse(row.Cell("L").Value.ToString()));
                                    if (!string.IsNullOrEmpty(row.Cell("M").Value.ToString()))
                                        TienThue = Math.Round(double.Parse(row.Cell("M").Value.ToString()));
                                    if (!string.IsNullOrEmpty(row.Cell("O").Value.ToString()))
                                        TongTienTT = Math.Round(double.Parse(row.Cell("O").Value.ToString()));

                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(row.Cell("K").Value.ToString()))
                                        TienTrcThue = Math.Round(double.Parse(row.Cell("K").Value.ToString()));
                                    if (!string.IsNullOrEmpty(row.Cell("L").Value.ToString()))
                                        TienThue = Math.Round(double.Parse(row.Cell("L").Value.ToString()));
                                    if (!string.IsNullOrEmpty(row.Cell("O").Value.ToString()))
                                        TongTienTT = Math.Round(double.Parse(row.Cell("O").Value.ToString()));
                                }

                                DateTime getdate = DateTime.Parse(GetNLap);
                                HoaDonNhap HoaDonNhap = new HoaDonNhap();
                                HoaDonNhap.SoHD = getSohd;
                                HoaDonNhap.NLap = getdate;
                                lstvao.Add(HoaDonNhap);

                                if (!KiemtrahoadonCT(getSohd, getSHHD, getdate, mstnb, 1))
                                {
                                    hdchuanhapvao += 1;
                                    dshdvaochunhap += getSohd + ",";
                                }
                                ChungTuHD findhoadon = new ChungTuHD();
                                findhoadon = lstChungTuHD.FirstOrDefault(m => m.NgayCT.Date == getdate.Date && Helpers.RemoveLeadingZeros(m.SoHieu) == Helpers.RemoveLeadingZeros(getSohd).TrimEnd('.') && m.KHHD == getSHHD);
                                if (findhoadon != null)
                                {
                                    bool saithongtin = false;
                                    string lydosai = "";
                                    if (TienTrcThue != findhoadon.TienTrcThue && TienTrcThue != 0)
                                    {
                                        lydosai += "Tiền trước thuế bị lệch";
                                        saithongtin = true;
                                    }
                                    if (TienThue != findhoadon.TienThue && TienThue != 0)
                                    {
                                        if (lydosai == "")
                                            lydosai += "Tiền  thuế bị lệch";
                                        saithongtin = true;
                                    }

                                    if (saithongtin)
                                    {
                                        dshdsaithongtin += $"{getSohd}(v)({lydosai}) ,";
                                        hdsaithongtin += 1;
                                    }
                                }
                                else
                                {
                                    //Trường hợp null có thể do ngày sai, bỏ điều kiện ngày
                                    findhoadon = lstChungTuHD.FirstOrDefault(m => Helpers.RemoveLeadingZeros(m.SoHieu) == Helpers.RemoveLeadingZeros(getSohd).TrimEnd('.') && m.KHHD == getSHHD);
                                    if (findhoadon != null)
                                    {
                                        bool saithongtin = false;
                                        string lydosai = "";
                                        if (findhoadon.NgayCT.Date != getdate.Date)
                                        {
                                            lydosai += "Ngày chứng từ bị sai";
                                            saithongtin = true;
                                        }
                                        if (saithongtin)
                                        {
                                            dshdsaithongtin += $"{getSohd}(v)({lydosai}) ,";
                                            hdsaithongtin += 1;
                                        }
                                    }
                                }
                                tongvao += 1;
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                    j++;
                }

                var getchungtuthang = lookupHoaDonCT.Where(m => m.NLap.Month == i && m.Type == 1).ToList();
                foreach (var it in getchungtuthang)
                {
                    var check = !lstvao.Any(m => m.SoHD == it.SoHD);
                    if (check)
                    {
                        hdnhapduDauvao += it.SoHD + ",";
                    }
                }

                //Đọc hoa don dau ra
                string directoryPathra = Path.Combine(savedPath, pathYear, "HDRa", i.ToString());
                var excelFilesra = Directory.EnumerateFiles(directoryPathra, "*.xlsx", SearchOption.AllDirectories).ToList();

                foreach (var excelFile in excelFilesra)
                {
                    using (var workbook = new XLWorkbook(excelFile))
                    {
                        var worksheet = workbook.Worksheet(1);
                        foreach (var row in worksheet.RowsUsed().Skip(3))
                        {
                            string GetNLap = row.Cell("E").Value.ToString();
                            string getSohd = Helpers.RemoveLeadingZeros(row.Cell("D").Value.ToString()); // Lấy giá trị của cột C trong hàng hiện tại 
                            string getkhhd = row.Cell("C").Value.ToString();
                            double TienTrcThue = 0;
                            double TienThue = 0;
                            double TongTienTT = 0;
                            if (!string.IsNullOrEmpty(row.Cell("L").Value.ToString()))
                                TienTrcThue = Math.Round(double.Parse(row.Cell("L").Value.ToString()));
                            if (!string.IsNullOrEmpty(row.Cell("M").Value.ToString()))
                                TienThue = Math.Round(double.Parse(row.Cell("M").Value.ToString()));
                            if (!string.IsNullOrEmpty(row.Cell("O").Value.ToString()))
                                TongTienTT = Math.Round(double.Parse(row.Cell("O").Value.ToString()));
                            if (getSohd == "104")
                            {
                                int dngg = 10;
                            }
                            string mstnm = row.Cell("H").Value.ToString();

                            if (DateTime.TryParse(GetNLap, out DateTime getdate))
                            {
                                DateTime gd = DateTime.Parse(GetNLap);
                                HoaDonNhap HoaDonNhap = new HoaDonNhap();
                                HoaDonNhap.SoHD = getSohd;
                                HoaDonNhap.NLap = gd;
                                lstRa.Add(HoaDonNhap);
                                if (!KiemtrahoadonCT(getSohd, getkhhd, getdate, mstnm, 2))
                                {
                                    hdchuanhapra += 1;
                                    dshdrachunhap += getSohd + ",";
                                }
                                ChungTuHD findhoadon = new ChungTuHD();
                                findhoadon = lstChungTuHD.FirstOrDefault(m => m.NgayCT.Date == getdate.Date && Helpers.RemoveLeadingZeros(m.SoHieu) == Helpers.RemoveLeadingZeros(getSohd).TrimEnd('.') && m.KHHD == getkhhd);
                                if (findhoadon != null)
                                {
                                    bool saithongtin = false;
                                    string lydosai = "";
                                    if (TienTrcThue != findhoadon.TienTrcThue && TienTrcThue != 0)
                                    {
                                        lydosai += "Tiền trước thuế bị lệch";
                                        saithongtin = true;
                                    }
                                    if (TienThue != findhoadon.TienThue && TienThue != 0)
                                    {
                                        if (lydosai == "")
                                            lydosai += "Tiền thuế bị lệch";
                                        saithongtin = true;
                                    }

                                    if (saithongtin)
                                    {
                                        dshdsaithongtin += $"{getSohd}(r)({lydosai}) ,";
                                        hdsaithongtin += 1;
                                    }
                                }
                                else
                                {
                                    //Trường hợp null có thể do ngày sai, bỏ điều kiện ngày
                                    findhoadon = lstChungTuHD.FirstOrDefault(m => Helpers.RemoveLeadingZeros(m.SoHieu) == Helpers.RemoveLeadingZeros(getSohd).TrimEnd('.') && m.KHHD == getkhhd);
                                    if (findhoadon != null)
                                    {
                                        bool saithongtin = false;
                                        string lydosai = "";
                                        if (findhoadon.NgayCT.Date != getdate.Date)
                                        {
                                            lydosai += "Ngày chứng từ bị sai";
                                            saithongtin = true;
                                        }
                                        if (saithongtin)
                                        {
                                            dshdsaithongtin += $"{getSohd}(r)({lydosai}) ,";
                                            hdsaithongtin += 1;
                                        }
                                    }
                                }
                            }
                            tongra += 1;
                        }
                    }
                }
                var getchungtuthangra = lookupHoaDonCT.Where(m => m.NLap.Month == i && m.Type == 2).ToList();
                foreach (var it in getchungtuthangra)
                {
                    var check = !lstRa.Any(m => m.SoHD == it.SoHD);
                    if (check)
                    {
                        hdnhapduDaura += it.SoHD + ",";
                    }
                }



                var month1 = new MonthWarning();
                month1.Month = $"{i.ToString("00")}/{DateTime.Now.Year}";
                month1.Warnings = new List<Warning>();

                var warning1 = new Warning();
                if (hdchuanhapvao > 0)
                {
                    if (hdchuanhapra > 0)
                    {
                        warning1.Text += $"🔴 {hdchuanhapvao} Hóa đơn đầu vào  , {hdchuanhapra} Hóa đơn đầu ra chưa nhập ";
                    }
                    else
                    {
                        warning1.Text = $"🔴 {hdchuanhapvao} Hóa đơn đầu vào chưa nhập ";
                    }
                }
                else
                {
                    if (hdchuanhapra > 0)
                    {
                        warning1.Text = $"🔴 {hdchuanhapra} Hóa đơn đầu ra chưa nhập";
                    }
                }

                warning1.Color = Color.Red;
                month1.Warnings.Add(warning1);

                //Sai thông tin 
                if (hdsaithongtin > 0)
                {
                    var warning3 = new Warning();
                    warning3.Text = $"🔵 {hdsaithongtin} hoá đơn sai thông tin  : {dshdsaithongtin}";
                    warning3.Color = Color.DarkMagenta;
                    month1.Warnings.Add(warning3);

                }

                //Import lỗi 
                if (getimportloi.Rows.Count > 0)
                {
                    var warning3 = new Warning();
                    var listimportloi = "";
                    foreach (DataRow row in getimportloi.Rows)
                    {
                        listimportloi += row["SHDon"].ToString() + ",";
                    }
                    warning3.Text = $"🔵 {getimportloi.Rows.Count} import lỗi : {listimportloi}";
                    warning3.Color = Color.Blue;
                    month1.Warnings.Add(warning3);

                }
                //Hàng âm

                if (!string.IsNullOrEmpty(hangam))
                {
                    var group = vattuAms.GroupBy(m => m.MaPL);
                    var warning4 = new Warning();
                    foreach (var m in group)
                    {
                        warning4.Text = $"🔵 Nhóm {m.Key} Có {m.Count()} ⚠️ hàng đang âm";
                    }
                    warning4.Color = Color.DarkCyan;
                    month1.Warnings.Add(warning4);
                }

                //Tài khoản chưa cân
                if (sumDkNo != sumDkCo)
                {
                    var warning4 = new Warning();
                    warning4.Text = $"🔵 Số dư đầu kỳ chưa cân {sumDkNo} -  {sumDkCo}";
                    warning4.Color = Color.DarkSeaGreen;
                    month1.Warnings.Add(warning4);
                }
                if (sumPsNo != sumPsCo)
                {
                    var warning4 = new Warning();
                    warning4.Text = $"🔵 Số dư trong kỳ chưa cân {sumPsNo} -  {sumPsCo}";
                    warning4.Color = Color.GreenYellow;
                    month1.Warnings.Add(warning4);
                }
                if (!string.IsNullOrEmpty(hdnhapduDauvao))
                {
                    var warning4 = new Warning();
                    warning4.Text = $"🔵 Hoá đơn đầu vào nhập dư : {hdnhapduDauvao} ";
                    warning4.Color = Color.BlueViolet;
                    month1.Warnings.Add(warning4);
                }
                if (!string.IsNullOrEmpty(hdnhapduDaura))
                {
                    var warning4 = new Warning();
                    warning4.Text = $"🔵 Hoá đơn đầu ra nhập dư : {hdnhapduDaura} ";
                    warning4.Color = Color.BlueViolet;
                    month1.Warnings.Add(warning4);
                }
                month1.Total = month1.Warnings.Count;
                months.Add(month1);
            }

            // ===== Tính Total =====
            foreach (var month in months)
            {
                month.Total = month.Warnings.Count;
            }


            // ==========================================================
            // TẠO KHU VỰC SCROLL HIỂN THỊ CÁC THÁNG
            // ==========================================================

            // Xóa giao diện cũ
            panelControl1.Controls.Clear();

            // Panel chứa danh sách tháng
            Panel panelScroll = new Panel();
            panelScroll.Name = "panelScrollMonth";
            panelScroll.Dock = DockStyle.Fill;
            panelScroll.AutoScroll = true;
            panelScroll.BackColor = Color.White;

            // Thêm panel scroll vào panel chính
            panelControl1.Controls.Add(panelScroll);


            // ==========================================================
            // CÁC THÔNG SỐ GIAO DIỆN
            // ==========================================================

            int yPos = 5;

            int headerHeight = 45;
            int warningHeight = 22;  // chiều cao tối thiểu, sẽ tăng nếu text wrap
            int warningSpacing = 4;  // khoảng cách giữa các warning

            int leftMargin = 5;
            int rightMargin = 15;
            int bottomMargin = 8;


            // ==========================================================
            // TẠO PANEL CHO TỪNG THÁNG
            // ==========================================================

            foreach (var monthData in months)
            {
                // ------------------------------------------------------
                // Panel của tháng (chưa set Size, sẽ set sau khi đo warning)
                // ------------------------------------------------------

                var panelControl2 = new DevExpress.XtraEditors.PanelControl();

                panelControl2.Name =
                    $"panelControl_{monthData.Month.Replace("/", "_")}";

                panelControl2.Appearance.BackColor = Color.White;
                panelControl2.Appearance.Options.UseBackColor = true;

                panelControl2.Location = new Point(
                    leftMargin,
                    yPos
                );

                panelControl2.TabIndex = 0;

                // Chiều rộng thực tế của vùng scroll
                int panelWidth = panelScroll.ClientSize.Width
                                 - leftMargin
                                 - rightMargin;

                if (panelWidth < 100)
                    panelWidth = 100;


                // ======================================================
                // ICON LỊCH
                // ======================================================

                var svgImageBox2 = new DevExpress.XtraEditors.SvgImageBox();

                svgImageBox2.Location = new Point(5, 5);
                svgImageBox2.Name = "svgImageBox2";
                svgImageBox2.Size = new System.Drawing.Size(50, 35);
                svgImageBox2.BackColor = Color.Transparent;

                string svgCalendarGreen = @"
<svg xmlns='http://www.w3.org/2000/svg'
     width='24'
     height='24'
     viewBox='0 0 24 24'>

    <rect x='3'
          y='4'
          width='18'
          height='18'
          rx='2'
          fill='none'
          stroke='#4CAF50'
          stroke-width='2'/>

    <line x1='3'
          y1='10'
          x2='21'
          y2='10'
          stroke='#4CAF50'
          stroke-width='2'/>

    <line x1='8'
          y1='2'
          x2='8'
          y2='6'
          stroke='#4CAF50'
          stroke-width='2'/>

    <line x1='16'
          y1='2'
          x2='16'
          y2='6'
          stroke='#4CAF50'
          stroke-width='2'/>

</svg>";


                using (System.IO.MemoryStream stream =
                       new System.IO.MemoryStream(
                           System.Text.Encoding.UTF8.GetBytes(
                               svgCalendarGreen)))
                {
                    svgImageBox2.SvgImage =
                        DevExpress.Utils.Svg.SvgImage.FromStream(stream);
                }

                panelControl2.Controls.Add(svgImageBox2);


                // ======================================================
                // LABEL THÁNG
                // ======================================================

                var labelControl2 =
                    new DevExpress.XtraEditors.LabelControl();

                labelControl2.Appearance.Font =
                    new System.Drawing.Font(
                        "Tahoma",
                        9F,
                        System.Drawing.FontStyle.Bold);

                labelControl2.Appearance.Options.UseFont = true;

                labelControl2.Location =
                    new System.Drawing.Point(65, 10);

                labelControl2.Name = "labelControl2";

                labelControl2.Text = monthData.Month;

                panelControl2.Controls.Add(labelControl2);


                // ======================================================
                // LABEL TỔNG CẢNH BÁO
                // ======================================================

                var labelControl3 =
                    new DevExpress.XtraEditors.LabelControl();

                labelControl3.Location =
                    new System.Drawing.Point(170, 12);

                labelControl3.Name = "labelControl3";

                labelControl3.Text =
                    $"{monthData.Total} cảnh báo";

                labelControl3.ForeColor =
                    Color.DarkRed;

                panelControl2.Controls.Add(labelControl3);


                // ======================================================
                // DANH SÁCH CẢNH BÁO
                // Tạo + đo chiều cao từng cái trước, rồi mới tính Size panel
                // ======================================================

                int warningY = headerHeight;
                int warningWidth = panelWidth - 40;  // 20 trái + 20 phải

                if (warningWidth < 50)
                    warningWidth = 50;

                foreach (var warning in monthData.Warnings)
                {
                    var lblWarning = new DevExpress.XtraEditors.LabelControl();

                    lblWarning.Text = warning.Text;
                    lblWarning.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
                    lblWarning.ForeColor = warning.Color;

                    lblWarning.Appearance.BackColor = Color.Transparent;
                    lblWarning.Appearance.Options.UseBackColor = true;

                    // === BẮT BUỘC để wrap text ===
                    lblWarning.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
                    lblWarning.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
                    lblWarning.Appearance.Options.UseTextOptions = true;   // ⚠️ RẤT QUAN TRỌNG

                    lblWarning.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
                    lblWarning.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;

                    // === Tính chiều cao động theo nội dung ===
                    int actualHeight = GetLabelHeight(lblWarning, warningWidth, warningHeight);

                    lblWarning.Size = new Size(warningWidth, actualHeight);
                    lblWarning.Location = new Point(20, warningY);

                    panelControl2.Controls.Add(lblWarning);

                    warningY += actualHeight + warningSpacing;
                }


                // ======================================================
                // BÂY GIỜ MỚI SET SIZE CHO PANEL (đủ cao để chứa hết warning)
                // ======================================================

                int panelHeight = warningY + bottomMargin;

                panelControl2.Size = new Size(
                    panelWidth,
                    panelHeight
                );


                // ======================================================
                // THÊM PANEL THÁNG VÀO PANEL SCROLL
                // ======================================================

                panelScroll.Controls.Add(panelControl2);


                // Vị trí tháng tiếp theo
                yPos += panelControl2.Height + 5;
            }


            // ==========================================================
            // CẬP NHẬT SCROLL
            // ==========================================================

            panelScroll.AutoScrollMinSize =
                new Size(
                    0,
                    yPos + 5);
            panelScroll.HorizontalScroll.Enabled = false;
            panelScroll.HorizontalScroll.Visible = false;
        }
        private void LoadMessage()
        {
            try
            {
                vattuAms = new List<VattuAm>();
                lookupHoaDonCT.Clear();

                // ===================== LOAD DỮ LIỆU GỐC =====================
                LoadChungTuLookup();
                tbImport = ExecuteQuery("SELECT * FROM tbImport");
                var dtTonKho = ExecuteQuery("SELECT * FROM TonKho");
                var dtVattu = ExecuteQuery("SELECT * FROM Vattu");
                var phanloaivt = ExecuteQuery("SELECT * FROM PhanLoaiVattu");

                var months = new List<MonthWarning>();
                int currentYear = DateTime.Now.Year;
                int currentMonth = DateTime.Now.Month;

                for (int month = currentMonth; month >= 1; month--)
                {
                    var monthWarning = ProcessMonth(month, currentYear, dtTonKho, dtVattu, phanloaivt);
                    months.Add(monthWarning);
                }

                // ===================== BUILD UI =====================
                BuildWarningUI(months);
            }
            catch (Exception ex)
            {
                // Log hoặc hiển thị lỗi
                XtraMessageBox.Show($"Lỗi khi load cảnh báo: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region ===================== LOAD DỮ LIỆU =====================

        private void LoadChungTuLookup()
        {
            string query = @"
        SELECT hd.SoHD, hd.KyHieu, hd.NgayPH, hd.MaKhachHang, kh.MST, ct.NgayCT, ct.MaLoai
        FROM ((Hoadon hd 
            INNER JOIN Chungtu ct ON hd.MaSo = ct.MaSo)
            INNER JOIN KhachHang kh ON hd.MaKhachHang = kh.MaSo)
        WHERE hd.KyHieu <> '...'";

            var dt = ExecuteQuery(query);

            foreach (DataRow row in dt.Rows)
            {
                string soHD = Helpers.RemoveLeadingZeros(row["SoHD"]?.ToString() ?? "")
                                    .Replace(".", "")
                                    .Trim();
                soHD = RemoveLeadingZeros(soHD);

                string kyHieu = row["KyHieu"]?.ToString() ?? "";
                DateTime ngayPH = ((DateTime)row["NgayCT"]).Date;
                string mst = row["MST"]?.ToString() ?? "";
                int maLoai = int.Parse(row["MaLoai"]?.ToString() ?? "1");
                maLoai = (maLoai == 8) ? 2 : 1;

                lookupHoaDonCT.Add((mst, soHD, kyHieu, ngayPH, maLoai));
            }
        }

        #endregion

        #region ===================== XỬ LÝ TỪNG THÁNG =====================

        private MonthWarning ProcessMonth(int month, int year, DataTable dtTonKho, DataTable dtVattu, DataTable phanloaivt)
        {
            var monthWarning = new MonthWarning
            {
                Month = $"{month:00}/{year}",
                Warnings = new List<Warning>()
            };

            // 1. Kiểm tra tồn kho âm
            CheckNegativeInventory(month, dtTonKho, dtVattu, phanloaivt, monthWarning);

            // 2. Kiểm tra cân đối tài khoản
            CheckAccountBalance(month, year, monthWarning);

            // 3. Xử lý hóa đơn đầu vào + đầu ra
            ProcessInvoices(month, year, monthWarning);

            // 4. Import lỗi
            CheckImportErrors(month, monthWarning);

            monthWarning.Total = monthWarning.Warnings.Count;
            return monthWarning;
        }

        private void CheckNegativeInventory(int month, DataTable dtTonKho, DataTable dtVattu, DataTable phanloaivt, MonthWarning monthWarning)
        {
            string columnName = $"Luong_{month}";
            var negativeItems = new List<VattuAm>();

            foreach (DataRow row in dtTonKho.Rows)
            {
                if (row[columnName] == DBNull.Value || row[columnName] == null) continue;

                double soLuong = Convert.ToDouble(row[columnName]);
                if (soLuong >= 0) continue;

                var vt = dtVattu.AsEnumerable()
                    .FirstOrDefault(m => m["MaSo"].ToString() == row["MaVatTu"].ToString());

                if (vt == null) continue;

                string maPL = phanloaivt.AsEnumerable()
                    .FirstOrDefault(m => m["MaSo"].ToString() == vt["MaPhanLoai"].ToString())?["SoHieu"]?.ToString() ?? "";

                negativeItems.Add(new VattuAm
                {
                    MaVT = vt["SoHieu"].ToString(),
                    TenVT = vt["TenVattu"].ToString(),
                    MaPL = maPL
                });
            }

            if (negativeItems.Count == 0) return;

            vattuAms.AddRange(negativeItems);

            var groups = negativeItems.GroupBy(x => x.MaPL);
            foreach (var g in groups)
            {
                monthWarning.Warnings.Add(new Warning
                {
                    Text = $"⚠️ Nhóm {g.Key}: {g.Count()} mặt hàng đang âm",
                    Color = Color.FromArgb(0, 150, 136) // Teal
                });
            }
        }

        private void CheckAccountBalance(int month, int year, MonthWarning monthWarning)
        {
            var result = GetHeThongTK(month, year);
            if (result == null || result.Rows.Count == 0) return;

            decimal Sum(string col) => result.AsEnumerable()
                .Where(r => r["Cap"]?.ToString() == "0" && r[col] != DBNull.Value)
                .Sum(r => Convert.ToDecimal(r[col]));

            decimal sumDkNo = Sum("DkNo");
            decimal sumDkCo = Sum("DkCo");
            decimal sumPsNo = Sum("PsNo");
            decimal sumPsCo = Sum("PsCo");

            if (sumDkNo != sumDkCo)
            {
                monthWarning.Warnings.Add(new Warning
                {
                    Text = $"⚖️ Số dư đầu kỳ chưa cân: {sumDkNo:N0} - {sumDkCo:N0}",
                    Color = Color.FromArgb(76, 175, 80)
                });
            }

            if (sumPsNo != sumPsCo)
            {
                monthWarning.Warnings.Add(new Warning
                {
                    Text = $"⚖️ Phát sinh trong kỳ chưa cân: {sumPsNo:N0} - {sumPsCo:N0}",
                    Color = Color.FromArgb(139, 195, 74)
                });
            }
        }

        private void ProcessInvoices(int month, int year, MonthWarning monthWarning)
        {
            if (month == 7)
            {
                int test = 10;
            }
            var lstVao = new List<HoaDonNhap>();
            var lstRa = new List<HoaDonNhap>();
            var sbSaiThongTin = new StringBuilder();
            var sbVaoChuaNhap = new StringBuilder();
            var sbRaChuaNhap = new StringBuilder();
            var sbVaoNhapDu = new StringBuilder();
            var sbRaNhapDu = new StringBuilder();

            int hdChuaNhapVao = 0, hdChuaNhapRa = 0, hdSaiThongTin = 0;

            string pathYear = $"HD{year}";
            string pathVao = Path.Combine(savedPath, pathYear, "HDVao", month.ToString());
            string pathRa = Path.Combine(savedPath, pathYear, "HDRa", month.ToString());

            // ---- Đầu vào ----
            if (Directory.Exists(pathVao))
            {
                foreach (var file in Directory.EnumerateFiles(pathVao, "*.xlsx", SearchOption.AllDirectories))
                {
                    ProcessExcelFile(file, isVao: true, lstVao, ref hdChuaNhapVao, sbVaoChuaNhap,
                                     ref hdSaiThongTin, sbSaiThongTin);
                }
            }

            // ---- Đầu ra ----
            if (Directory.Exists(pathRa))
            {
                foreach (var file in Directory.EnumerateFiles(pathRa, "*.xlsx", SearchOption.AllDirectories))
                {
                    ProcessExcelFile(file, isVao: false, lstRa, ref hdChuaNhapRa, sbRaChuaNhap,
                                     ref hdSaiThongTin, sbSaiThongTin);
                }
            }

            // Hóa đơn nhập dư
            var ctVao = lookupHoaDonCT.Where(m => m.NLap.Month == month && m.Type == 1).ToList();
            foreach (var it in ctVao)
                if (!lstVao.Any(m => m.SoHD == it.SoHD))
                    sbVaoNhapDu.Append(it.SoHD).Append(",");

            var ctRa = lookupHoaDonCT.Where(m => m.NLap.Month == month && m.Type == 2).ToList();
            foreach (var it in ctRa)
                if (!lstRa.Any(m => m.SoHD == it.SoHD))
                    sbRaNhapDu.Append(it.SoHD).Append(",");

            // Thêm warning
            if (hdChuaNhapVao > 0 || hdChuaNhapRa > 0)
            {
                string text = "🔴 ";
                if (hdChuaNhapVao > 0) text += $"{hdChuaNhapVao} HĐ đầu vào chưa nhập";
                if (hdChuaNhapRa > 0) text += (hdChuaNhapVao > 0 ? "  •  " : "") + $"{hdChuaNhapRa} HĐ đầu ra chưa nhập";

                monthWarning.Warnings.Add(new Warning { Text = text, Color = Color.FromArgb(244, 67, 54) });
            }

            if (hdSaiThongTin > 0)
            {
                monthWarning.Warnings.Add(new Warning
                {
                    Text = $"🔵 {hdSaiThongTin} hóa đơn sai thông tin: {sbSaiThongTin}",
                    Color = Color.FromArgb(156, 39, 176)
                });
            }

            if (sbVaoNhapDu.Length > 0)
            {
                monthWarning.Warnings.Add(new Warning
                {
                    Text = $"🟣 HĐ đầu vào nhập dư: {sbVaoNhapDu}",
                    Color = Color.FromArgb(103, 58, 183)
                });
            }

            if (sbRaNhapDu.Length > 0)
            {
                monthWarning.Warnings.Add(new Warning
                {
                    Text = $"🟣 HĐ đầu ra nhập dư: {sbRaNhapDu}",
                    Color = Color.FromArgb(103, 58, 183)
                });
            }
        }

        private void ProcessExcelFile(string filePath, bool isVao,
            List<HoaDonNhap> list, ref int countChuaNhap, StringBuilder sbChuaNhap,
            ref int countSai, StringBuilder sbSai)
        {
            try
            {
                 var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheet(1);

                foreach (var row in worksheet.RowsUsed().Skip(3))
                {
                    try
                    {
                        string soHD = Helpers.RemoveLeadingZeros(row.Cell("D").Value.ToString()).Trim();
                        string kyHieu = row.Cell(isVao ? "C" : "C").Value.ToString();
                        string ngayStr = row.Cell("E").Value.ToString();
                        string mst = isVao ? row.Cell("F").Value.ToString() : row.Cell("H").Value.ToString();

                        if (!DateTime.TryParse(ngayStr, out DateTime ngayLap)) continue;

                        list.Add(new HoaDonNhap { SoHD = soHD, NLap = ngayLap });

                        int type = isVao ? 1 : 2;
                        if (!KiemtrahoadonCT(soHD, kyHieu, ngayLap, mst, type))
                        {
                            countChuaNhap++;
                            sbChuaNhap.Append(soHD).Append(",");
                        }

                        // Kiểm tra sai thông tin tiền / ngày
                        CheckInvoiceInfo(row, filePath, soHD, kyHieu, ngayLap, isVao, ref countSai, sbSai);
                    }
                    catch { /* bỏ qua dòng lỗi */ }
                }
            }
            catch (Exception ex)
            {
                // Log file lỗi nếu cần
                System.Diagnostics.Debug.WriteLine($"Lỗi đọc file {filePath}: {ex.Message}");
            }
        }

        private void CheckInvoiceInfo(IXLRow row, string filePath, string soHD, string kyHieu, DateTime ngayLap,
            bool isVao, ref int countSai, StringBuilder sbSai)
        {
            double tienTrcThue = 0, tienThue = 0;

            if (filePath.Contains("MayTinhTien"))
            {
                double.TryParse(row.Cell("L").Value.ToString(), out tienTrcThue);
                double.TryParse(row.Cell("M").Value.ToString(), out tienThue);
            }
            else
            {
                double.TryParse(row.Cell(isVao ? "K" : "L").Value.ToString(), out tienTrcThue);
                double.TryParse(row.Cell(isVao ? "L" : "M").Value.ToString(), out tienThue);
            }

            tienTrcThue = Math.Round(tienTrcThue);
            tienThue = Math.Round(tienThue);

            var find = lstChungTuHD.FirstOrDefault(m =>
                m.NgayCT.Date == ngayLap.Date &&
                Helpers.RemoveLeadingZeros(m.SoHieu) == Helpers.RemoveLeadingZeros(soHD).TrimEnd('.') &&
                m.KHHD == kyHieu);

            if (find == null)
            {
                // Thử bỏ điều kiện ngày
                find = lstChungTuHD.FirstOrDefault(m =>
                    Helpers.RemoveLeadingZeros(m.SoHieu) == Helpers.RemoveLeadingZeros(soHD).TrimEnd('.') &&
                    m.KHHD == kyHieu);

                if (find != null && find.NgayCT.Date != ngayLap.Date)
                {
                    countSai++;
                    sbSai.Append($"{soHD}({(isVao ? "v" : "r")})(Ngày CT sai), ");
                }
                return;
            }

            bool sai = false;
            var lyDo = new List<string>();

            if (tienTrcThue != 0 && tienTrcThue != find.TienTrcThue)
            {
                lyDo.Add("Tiền trước thuế lệch");
                sai = true;
            }
            if (tienThue != 0 && tienThue != find.TienThue)
            {
                lyDo.Add("Tiền thuế lệch");
                sai = true;
            }

            if (sai)
            {
                countSai++;
                sbSai.Append($"{soHD}({(isVao ? "v" : "r")})({string.Join(" + ", lyDo)}), ");
            }
        }

        private void CheckImportErrors(int month, MonthWarning monthWarning)
        {
            var errors = tbImport.AsEnumerable()
                .Where(m => m.Field<DateTime>("NLap").Month == month && m["Status"]?.ToString() == "2")
                .ToList();

            if (errors.Count == 0) return;

            var sb = new StringBuilder();
            foreach (var row in errors)
                sb.Append(row["SHDon"]).Append(",");

            monthWarning.Warnings.Add(new Warning
            {
                Text = $"🔵 {errors.Count} import lỗi: {sb}",
                Color = Color.FromArgb(33, 150, 243)
            });
        }

        #endregion

        #region ===================== BUILD GIAO DIỆN ĐẸP =====================

        private void BuildWarningUI(List<MonthWarning> months)
        {
            panelControl1.Controls.Clear();
            panelControl1.SuspendLayout();

            var panelScroll = new Panel
            {
                Name = "panelScrollMonth",
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(12)
            };
            panelControl1.Controls.Add(panelScroll);

            int yPos = 12;
            int leftMargin = 12;
            int rightMargin = 20;

            foreach (var monthData in months)
            {
                int panelWidth = Math.Max(panelScroll.ClientSize.Width - leftMargin - rightMargin, 320);

                // ===== Card tháng =====
                var card = new DevExpress.XtraEditors.PanelControl
                {
                    Appearance = { BackColor = Color.White },
                    BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple,
                    Location = new Point(leftMargin, yPos),
                    Padding = new Padding(0),
                    LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat }
                };
                card.Appearance.BorderColor = Color.FromArgb(220, 225, 230);

                // Header nền nhẹ
                var headerPanel = new Panel
                {
                    BackColor = Color.FromArgb(248, 250, 252),
                    Dock = DockStyle.Top,
                    Height = 48
                };
                card.Controls.Add(headerPanel);

                // Icon lịch
                var svgCalendar = CreateCalendarSvg();
                svgCalendar.Location = new Point(14, 8);
                headerPanel.Controls.Add(svgCalendar);

                // Label tháng
                var lblMonth = new DevExpress.XtraEditors.LabelControl
                {
                    Text = monthData.Month,
                    Location = new Point(70, 14),
                    Appearance = {
                Font = new Font("Segoe UI Semibold", 11F),
                ForeColor = Color.FromArgb(33, 37, 41)
            }
                };
                headerPanel.Controls.Add(lblMonth);

                // Tổng cảnh báo
                var lblTotal = new DevExpress.XtraEditors.LabelControl
                {
                    Text = monthData.Total == 0 ? "Không có cảnh báo" : $"{monthData.Total} cảnh báo",
                    Location = new Point(170, 16),
                    Appearance = {
                Font = new Font("Segoe UI", 9F),
                ForeColor = monthData.Total == 0 ? Color.FromArgb(76, 175, 80) : Color.FromArgb(220, 53, 69)
            }
                };
                headerPanel.Controls.Add(lblTotal);

                // ===== Danh sách warning =====
                int warningY = 56;
                int warningWidth = panelWidth - 36;

                if (monthData.Warnings.Count == 0)
                {
                    var lblEmpty = new DevExpress.XtraEditors.LabelControl
                    {
                        Text = "✓ Tháng này ổn định",
                        Location = new Point(18, warningY),
                        Appearance = {
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(108, 117, 125)
                }
                    };
                    card.Controls.Add(lblEmpty);
                    warningY += 28;
                }
                else
                {
                    foreach (var w in monthData.Warnings)
                    {
                        var lbl = new DevExpress.XtraEditors.LabelControl
                        {
                            Text = w.Text,
                            Location = new Point(18, warningY),
                            AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None,
                            Appearance = {
                        Font = new Font("Segoe UI", 8.5F),
                        ForeColor = w.Color,
                        TextOptions = {
                            WordWrap = DevExpress.Utils.WordWrap.Wrap,
                            HAlignment = DevExpress.Utils.HorzAlignment.Near,
                            VAlignment = DevExpress.Utils.VertAlignment.Top
                        }
                    }
                        };
                        lbl.Appearance.Options.UseTextOptions = true;
                        lbl.Appearance.Options.UseFont = true;
                        lbl.Appearance.Options.UseForeColor = true;

                        int h = GetLabelHeight(lbl, warningWidth, 22);
                        lbl.Size = new Size(warningWidth, h);
                        card.Controls.Add(lbl);

                        warningY += h + 8;
                    }
                }

                card.Size = new Size(panelWidth, warningY + 12);
                panelScroll.Controls.Add(card);

                yPos += card.Height + 12;
            }

            panelScroll.AutoScrollMinSize = new Size(0, yPos + 20);
            panelScroll.HorizontalScroll.Enabled = false;
            panelScroll.HorizontalScroll.Visible = false;

            panelControl1.ResumeLayout();
        }

        private DevExpress.XtraEditors.SvgImageBox CreateCalendarSvg()
        {
            string svg = @"
<svg xmlns='http://www.w3.org/2000/svg' width='28' height='28' viewBox='0 0 24 24'>
  <rect x='3' y='4' width='18' height='18' rx='2' fill='none' stroke='#4CAF50' stroke-width='1.8'/>
  <line x1='3' y1='10' x2='21' y2='10' stroke='#4CAF50' stroke-width='1.8'/>
  <line x1='8' y1='2' x2='8' y2='6' stroke='#4CAF50' stroke-width='1.8'/>
  <line x1='16' y1='2' x2='16' y2='6' stroke='#4CAF50' stroke-width='1.8'/>
</svg>";

            var box = new DevExpress.XtraEditors.SvgImageBox
            {
                Size = new Size(32, 32),
                BackColor = Color.Transparent
            };

             var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
            box.SvgImage = DevExpress.Utils.Svg.SvgImage.FromStream(stream);
            return box;
        }

        #endregion
        // ==========================================================
        // HÀM ĐO CHIỀU CAO LABEL THEO NỘI DUNG (có word-wrap)
        // ==========================================================
        private int GetLabelHeight(
            DevExpress.XtraEditors.LabelControl lbl,
            int width,
            int minHeight)
        {
            if (string.IsNullOrEmpty(lbl.Text))
                return minHeight;

            // Dùng TextRenderer để đo chính xác theo cách WinForms render
            Size proposed = new Size(width, int.MaxValue);

            Size measured = TextRenderer.MeasureText(
                lbl.Text,
                lbl.Font,
                proposed,
                TextFormatFlags.WordBreak | TextFormatFlags.NoPadding
            );

            int h = measured.Height + 6;   // padding trên dưới
            return Math.Max(h, minHeight);
        }
        
        string tokken = "";
        int maxlogin = 1;
        private async void GetToken()
        {
            try
            {
                // ===== HttpClient + CookieContainer =====
                var cookieContainer = new CookieContainer();
                var handler = new HttpClientHandler()
                {
                    UseCookies = true,
                    CookieContainer = cookieContainer,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    AllowAutoRedirect = true
                };

                using (var client = new HttpClient(handler))
                {
                    // Set timeout
                    client.Timeout = TimeSpan.FromSeconds(30);

                    // ===== Header giống trình duyệt =====
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                    client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                    client.DefaultRequestHeaders.Add("Accept-Language", "vi-VN,vi;q=0.9,en-US;q=0.8,en;q=0.7");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                    client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                    client.DefaultRequestHeaders.Add("Origin", "https://hoadondientu.gdt.gov.vn");
                    client.DefaultRequestHeaders.Add("Referer", "https://hoadondientu.gdt.gov.vn/");
                    client.DefaultRequestHeaders.ExpectContinue = false;

                    // ================= STEP 1: GET CAPTCHA =================
                    Application.DoEvents();

                    string capUrl = "https://hoadondientu.gdt.gov.vn/api/captcha";
                    var resCap = await client.GetAsync(capUrl);

                    if (!resCap.IsSuccessStatusCode)
                    {
                        //Tiến hành đăng nhập lại sau 2s 
                        return;
                    }

                    string capBody = await resCap.Content.ReadAsStringAsync();
                    MyJson capJson = JsonConvert.DeserializeObject<MyJson>(capBody);

                    string svgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "captcha.svg");
                    File.WriteAllText(svgPath, capJson.Content);

                    // ===== LẤY XSRF-TOKEN (NẾU CÓ) =====
                    string xsrfToken = null;

                    // Lấy từ CookieContainer
                    var cookies = cookieContainer.GetCookies(new Uri("https://hoadondientu.gdt.gov.vn"));
                    xsrfToken = cookies["XSRF-TOKEN"]?.Value;

                    // Nếu có thì thêm vào header, không có thì bỏ qua
                    if (!string.IsNullOrEmpty(xsrfToken))
                    {
                        client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
                        client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", xsrfToken);
                    }

                    // ================= STEP 2: SOLVE CAPTCHA =================

                    SvgCaptchaSolver solver = new SvgCaptchaSolver();
                    string cvalue = solver.SolveCaptcha(svgPath);

                    if (string.IsNullOrEmpty(cvalue))
                    { 

                        return;
                    }

                    // ================= STEP 3: LOGIN ================= 
                    Application.DoEvents();

                    string loginUrl = "https://hoadondientu.gdt.gov.vn/api/security-taxpayer/authenticate";

                    var payload = new
                    {
                        //username = txtuser.Text,
                        //password = txtpass.Text,
                        cvalue = cvalue,
                        ckey = capJson.Key
                    };

                    var content = new StringContent(
                        JsonConvert.SerializeObject(payload),
                        Encoding.UTF8,
                        "application/json"
                    );

                    // GỬI REQUEST LOGIN
                    var loginRes = await client.PostAsync(loginUrl, content); 
                    Application.DoEvents();
                    if (loginRes.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        string err = await loginRes.Content.ReadAsStringAsync();
                        //Tiến hành đăng nhập lại sau 2s
                        if (maxlogin < 3)
                        {
                            Thread.Sleep(2000);
                            GetToken();
                            maxlogin += 1;
                            return;
                        } 
                    }

                    loginRes.EnsureSuccessStatusCode();

                    string loginBody = await loginRes.Content.ReadAsStringAsync();
                    var tokenData = JsonConvert.DeserializeObject<TokenResponse>(loginBody);
                    this.tokken = tokenData.token;

                    // ================= STEP 4: PROFILE =================
                    try
                    {
                        var req = new HttpRequestMessage(
                            HttpMethod.Get,
                            "https://hoadondientu.gdt.gov.vn/api/security-taxpayer/profile"
                        );

                        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.tokken);
                        var profRes = await client.SendAsync(req);

                        if (profRes.IsSuccessStatusCode)
                        {
                            string profBody = await profRes.Content.ReadAsStringAsync();
                            var prof = JsonConvert.DeserializeObject<ProfileResponse>(profBody);

                            if (!string.IsNullOrEmpty(prof.password_expire))
                            {
                                DateTime expireDate = DateTime.Parse(prof.password_expire);
                                TimeSpan remain = expireDate - DateTime.Now;

                                if (remain.TotalDays <= 0)
                                {
                                    XtraMessageBox.Show(
                                        $"Mật khẩu đã hết hạn ngày {expireDate:dd/MM/yyyy}.",
                                        "Hết hạn!",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning
                                    );
                                    return;
                                }
                                else if (remain.TotalDays <= 3)
                                {
                                    XtraMessageBox.Show(
                                        $"⚠ Mật khẩu sẽ hết hạn sau {remain.Days} ngày!\nNgày: {expireDate:dd/MM/yyyy}",
                                        "Cảnh báo",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning
                                    );
                                }
                                else if (remain.TotalDays <= 7)
                                {
                                    XtraMessageBox.Show($"Mật khẩu sắp hết hạn {expireDate:dd/MM/yyyy} (còn {remain.Days} ngày)");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                       
                    } 
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi đăng nhập hệ thống thuế: " + ex.Message);
            }
        }
        private void TaiExcel()
        {
        } 
        private void LoadGrid()
        {
            string queryct = @"
                                       SELECT 
                        hd.SoHD,
                        hd.KyHieu,
                        hd.NgayPH,
                        hd.MaKhachHang,
                        kh.MST, 
                        ct.NgayCT,
                        ct.MaLoai
                    FROM 
                        ((Hoadon hd 
                        INNER JOIN 
                        Chungtu ct ON hd.MaSo = ct.MaSo)
                        INNER JOIN 
                        KhachHang kh ON hd.MaKhachHang = kh.MaSo)
                    WHERE 
                        hd.KyHieu <> '...'";

            dtChungtu = ExecuteQuery(queryct);

            lookupHoaDonCT.Clear();

            foreach (DataRow item in dtChungtu.Rows)
            {
                string soHD = Helpers.RemoveLeadingZeros(
                    item["SoHD"]?.ToString() ?? ""
                ).Trim();
                string KyHieu = item["KyHieu"]?.ToString() ?? "";
                DateTime ngayPH = ((DateTime)item["NgayCT"]).Date;

                int maKhachHang = (int)item["MaKhachHang"];

                string mst = item["MST"]?.ToString() ?? "";
                int Maloai = int.Parse(item["MaLoai"]?.ToString());
                if (Maloai == 8)
                {
                    Maloai = 2;
                }
                else
                {
                    Maloai = 1;
                }
                soHD = soHD.Replace(".", "").Trim();
                soHD = RemoveLeadingZeros(soHD);
                lookupHoaDonCT.Add((mst, soHD, KyHieu, ngayPH, Maloai));
            }

            string qrip = "SELECT * FROM tbImport";
            tbImport = ExecuteQuery(qrip);


            string qrtonkho = "select * from TonKho";
            var dtTonKho = ExecuteQuery(qrtonkho);
            string qrvt = "select * from Vattu";
            var dtVattu = ExecuteQuery(qrvt);
            string hangam = "";
            for (int i = DateTime.Now.Month; i >= 1; i--)
            {
                //Kiểm tra tồn kho
                hangam = "";
                string columnName = $"Luong_{i}";
                foreach (DataRow row in dtTonKho.Rows)
                {
                    // Lấy giá trị theo tên cột động
                    object value = row[columnName];
                    if (value != DBNull.Value && value != null)
                    {
                        double soLuong = Convert.ToDouble(value);
                        if (soLuong < 0)
                        {
                            var getvattu = dtVattu.AsEnumerable().Where(m => m["MaSo"].ToString() == row["MaVatTu"].ToString()).FirstOrDefault();
                            if (getvattu != null)
                            {
                                hangam += getvattu["SoHieu"] + ",";
                            }
                        }
                    }
                }

                var result = GetHeThongTK(i, DateTime.Now.Year);
                // Tổng DkNo
                var sumDkNo = result.AsEnumerable()
                    .Where(m => m["DkNo"] != DBNull.Value
                                && m["DkNo"] != null
                                && m["Cap"].ToString() == "0")
                    .Sum(m => Convert.ToDecimal(m["DkNo"]));

                // Tổng DkCo
                var sumDkCo = result.AsEnumerable()
                    .Where(m => m["DkCo"] != DBNull.Value
                                && m["DkCo"] != null
                                && m["Cap"].ToString() == "0")
                    .Sum(m => Convert.ToDecimal(m["DkCo"]));

                // Tổng PsNo
                var sumPsNo = result.AsEnumerable()
                    .Where(m => m["PsNo"] != DBNull.Value
                                && m["PsNo"] != null
                                && m["Cap"].ToString() == "0")
                    .Sum(m => Convert.ToDecimal(m["PsNo"]));

                // Tổng PsCo
                var sumPsCo = result.AsEnumerable()
                    .Where(m => m["PsCo"] != DBNull.Value
                                && m["PsCo"] != null
                                && m["Cap"].ToString() == "0")
                    .Sum(m => Convert.ToDecimal(m["PsCo"]));

                // Tổng CkNo
                var sumCkNo = result.AsEnumerable()
                    .Where(m => m["CkNo"] != DBNull.Value
                                && m["CkNo"] != null
                                && m["Cap"].ToString() == "0")
                    .Sum(m => Convert.ToDecimal(m["CkNo"]));

                // Tổng CkCo
                var sumCkCo = result.AsEnumerable()
                    .Where(m => m["CkCo"] != DBNull.Value
                                && m["CkCo"] != null
                                && m["Cap"].ToString() == "0")
                    .Sum(m => Convert.ToDecimal(m["CkCo"]));


                int hdchuanhapvao = 0;
                int hdchuanhapra = 0;
                int tongvao = 0;
                int tongra = 0;
                string importvaoloi = "";
                string importraloi = "";
                string hdnhapduDauvao = "";
                string hdnhapduDaura = "";
                List<HoaDonNhap> lstvao = new List<HoaDonNhap>();
                List<HoaDonNhap> lstRa = new List<HoaDonNhap>();
                //Lấy danh sách hoá đơn import lỗi
                var qr = tbImport.AsEnumerable()
                .Where(m => m.Field<DateTime>("NLap").Date.Month == i)
                .Where(m => m["Status"].ToString() == "2");

                DataTable getimportloi = new DataTable();
                if (qr.Any())
                {
                    getimportloi = qr.CopyToDataTable();
                }
                else
                {
                    // Tạo DataTable rỗng với cấu trúc giống tbImport
                    getimportloi = tbImport.Clone();
                }

                //Tìm đọc file excel từng tháng
                string pathYear = $"HD{DateTime.Now.Year}";
                string directoryPath2 = Path.Combine(savedPath, pathYear, "HDVao", i.ToString());

                var excelFiles = Directory.EnumerateFiles(directoryPath2, "*.xlsx", SearchOption.AllDirectories).ToList();

                int j = 1;
                foreach (var excelFile in excelFiles)
                {
                    using (var workbook = new XLWorkbook(excelFile))

                    {
                        var worksheet = workbook.Worksheet(1); // Lấy sheet đầu tiên
                        foreach (var row in worksheet.RowsUsed().Skip(3)) // Bỏ qua 6 hàng đầu tiên
                        {
                            try
                            {
                                string khhd = row.Cell("B").Value.ToString(); // Lấy giá trị của cột A trong hàng hiện tại
                                string getSHHD = row.Cell("C").Value.ToString(); // Lấy giá trị của cột A trong hàng hiện tại
                                string getSohd = Helpers.RemoveLeadingZeros(row.Cell("D").Value.ToString()); // Lấy giá trị của cột C trong hàng hiện tại 
                                string GetNLap = row.Cell("E").Value.ToString();
                                string mstnb = row.Cell("F").Value.ToString();
                                DateTime getdate = DateTime.Parse(GetNLap);
                                HoaDonNhap HoaDonNhap = new HoaDonNhap();
                                HoaDonNhap.SoHD = getSohd;
                                HoaDonNhap.NLap = getdate;
                                lstvao.Add(HoaDonNhap);

                                if (!KiemtrahoadonCT(getSohd, getSHHD, getdate, mstnb, 1))
                                {
                                    hdchuanhapvao += 1;
                                }
                                tongvao += 1;
                            }
                            catch (Exception ex)
                            {

                            }

                        }
                    }
                    j++;
                }
                var getchungtuthang = lookupHoaDonCT.Where(m => m.NLap.Month == i && m.Type == 1).ToList();
                foreach (var it in getchungtuthang)
                {
                    var check = !lstvao.Any(m => m.SoHD == it.SoHD);
                    if (check)
                    {
                        hdnhapduDauvao += it.SoHD + ",";
                    }
                }

                string directoryPathra = Path.Combine(savedPath, pathYear, "HDRa", i.ToString());
                var excelFilesra = Directory.EnumerateFiles(directoryPathra, "*.xlsx", SearchOption.AllDirectories).ToList();

                foreach (var excelFile in excelFilesra)
                {
                    using (var workbook = new XLWorkbook(excelFile))
                    {
                        var worksheet = workbook.Worksheet(1);
                        foreach (var row in worksheet.RowsUsed().Skip(3))
                        {
                            string GetNLap = row.Cell("E").Value.ToString();
                            string getSohd = Helpers.RemoveLeadingZeros(row.Cell("D").Value.ToString()); // Lấy giá trị của cột C trong hàng hiện tại 
                            string getkhhd = row.Cell("C").Value.ToString();
                            if (getSohd == "104")
                            {
                                int dngg = 10;
                            }
                            string mstnm = row.Cell("H").Value.ToString();

                            if (DateTime.TryParse(GetNLap, out DateTime getdate))
                            {
                                DateTime gd = DateTime.Parse(GetNLap);
                                HoaDonNhap HoaDonNhap = new HoaDonNhap();
                                HoaDonNhap.SoHD = getSohd;
                                HoaDonNhap.NLap = gd;
                                lstRa.Add(HoaDonNhap);
                                if (!KiemtrahoadonCT(getSohd, getkhhd, getdate, mstnm, 2))
                                {
                                    hdchuanhapra += 1;
                                }
                            }
                            tongra += 1;
                        }
                    }
                }

                var getchungtuthangra = lookupHoaDonCT.Where(m => m.NLap.Month == i && m.Type == 2).ToList();
                foreach (var it in getchungtuthangra)
                {
                    var check = !lstRa.Any(m => m.SoHD == it.SoHD);
                    if (check)
                    {
                        hdnhapduDaura += it.SoHD + ",";
                    }
                }
                if (excelFiles.Count > 0 || excelFilesra.Count > 0)
                {
                    WarningData warningData = new WarningData();
                    warningData.Thang = i;
                    warningData.Hoadonthieu = $"{hdchuanhapvao} hd đầu vào, {hdchuanhapra} hd đầu ra";

                    //Import lỗi 
                    if (getimportloi.Rows.Count > 0)
                    {
                        foreach (DataRow item in getimportloi.Rows)
                        {
                            if (item["Type"].ToString() == "1")
                                importvaoloi += item["SHDon"].ToString() + ",";
                            else
                                importraloi += item["SHDon"].ToString() + ",";
                        }
                    }
                    warningData.Importloi = !string.IsNullOrEmpty(importvaoloi) ? $"Đầu vào  : {importvaoloi}" : "";
                    warningData.Importloi += !string.IsNullOrEmpty(importraloi) ? $" Đầu ra  : {importraloi}" : "";
                    warningData.HoaDonThua = !string.IsNullOrEmpty(hdnhapduDauvao) ? $"Đv : {hdnhapduDauvao}" : "";
                    warningData.HoaDonThua += !string.IsNullOrEmpty(hdnhapduDaura) ? $"Đr : {hdnhapduDaura}" : "";

                    if (sumDkNo != sumDkCo)
                    {
                        warningData.HethongTK += $"Số dư đầu kỳ chưa cân {sumDkNo} -  {sumDkCo}";
                    }
                    if (sumPsNo != sumPsCo)
                    {
                        warningData.HethongTK += $"Số dư trong kỳ chưa cân {sumPsNo} -  {sumPsCo}";
                    }
                    if (sumCkNo != sumCkCo)
                    {
                        warningData.HethongTK += $"Số dư cuối kỳ chưa cân {sumCkNo} -  {sumCkCo}";
                    }
                    warningData.Hangam = hangam;
                    warningDatas.Add(warningData);
                }
            }

            gridControl1.DataSource = warningDatas.OrderByDescending(m => m.Thang);
        }
        private bool KiemtrahoadonCT(string SoHD, string KyHieu, DateTime NLap, string Mst, int type)
        {
            if (Mst == "KL")
                Mst = "00";
            if (Mst.Length < 10)
                return lookupHoaDonCT.Any(m => m.SoHD == SoHD && m.KyHieu == KyHieu && m.NLap == NLap && m.Type == type);
            return lookupHoaDonCT.Contains((Mst, SoHD, KyHieu, NLap, type));
        }
        public static string RemoveLeadingZeros(string invoiceNumber)
        {
            if (string.IsNullOrEmpty(invoiceNumber))
                return invoiceNumber;

            return Regex.Replace(invoiceNumber, "^0+", "");
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

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Vanguard_Shown(object sender, EventArgs e)
        {
            this.Activate();
            this.Focus();
            this.BringToFront();
            this.TopMost = true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();    
        }
    }
}