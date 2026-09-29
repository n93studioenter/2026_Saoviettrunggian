using ClosedXML.Excel;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using Newtonsoft.Json;
using SaovietTax.Database;
using SaovietTax.DTO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SaovietTax.frmMain;
using static SaovietTax.KTHT;
using DataTable = System.Data.DataTable;
using Size = System.Drawing.Size;
using WinLabel = System.Windows.Forms.Label;

namespace SaovietTax
{
    public partial class Vanguard : DevExpress.XtraEditors.XtraForm
    {
        // =====================================================================
        // DTOs
        // =====================================================================
        public class WarningData
        {
            public int Thang { get; set; }
            public string Hoadonthieu { get; set; }
            public string Importloi { get; set; }
            public string Hangam { get; set; }
            public string HethongTK { get; set; }
            public string HoaDonThua { get; set; }
        }

        public class HoaDonNhap
        {
            public string SoHD { get; set; }
            public DateTime NLap { get; set; }
            public DateTime NDung { get; set; }
            public string KHHD { get; set; }
            public string MST { get; set; }
            public double TienTrcThue { get; set; }
            public double TienThue { get; set; }
            public double TongTienTT { get; set; }
            public int Type { get; set; }
        }

        public class Warning
        {
            public string Text { get; set; }
            public Color Color { get; set; }
            public Warning() { }
            public Warning(string text, Color color) { Text = text; Color = color; }
        }

        public class MonthWarning
        {
            public string Month { get; set; }
            public int Total { get; set; }
            public List<Warning> Warnings { get; set; } = new List<Warning>();
            public MonthWarning() { }
            public MonthWarning(string month, List<Warning> warnings)
            {
                Month = month; Warnings = warnings;
                Total = warnings?.Count ?? 0;
            }
        }

        public class VattuAm
        {
            public string MaVT { get; set; }
            public string TenVT { get; set; }
            public string MaPL { get; set; }
        }

        private class AccountBalance
        {
            public decimal DkNo, DkCo, PsNo, PsCo, CkNo, CkCo;
        }

        // =====================================================================
        // FIELDS
        // =====================================================================
        public string password, connectionString;
        public string pathThumuc = "";
        public string dbPath = "";
        private string savedPath = "";
        string tokken = "";
        int maxlogin = 1;

        List<WarningData> warningDatas = new List<WarningData>();
        public HashSet<(string Mst, string SoHD, string KyHieu, DateTime NLap, int Type)> lookupHoaDonCT { get; }
            = new HashSet<(string Mst, string SoHD, string KyHieu, DateTime NLap, int Type)>();

        DataTable dtChungtu;
        DataTable tbImport;
        DataTable gettbChungtu;
        List<ChungTuHD> lstChungTuHD = new List<ChungTuHD>();
        List<VattuAm> vattuAms = new List<VattuAm>();

        // Cache dùng chung cho pipeline
        private Dictionary<string, KhachHangInfo> _dictKhachHang;
        private Dictionary<int, List<DataRow>> _chungTuGroups;
        private Dictionary<string, ChungTuHD> _chungTuDict;
        private Dictionary<int, List<DataRow>> _gettbChungTuByMaCT;
        private Dictionary<string, DataRow> _vattuDict;
        private Dictionary<string, string> _phanLoaiDict;
        private Dictionary<int, AccountBalance> _accountBalances;
        private Dictionary<int, (List<HoaDonNhap> Vao, List<HoaDonNhap> Ra)> _excelCache;

        private System.Windows.Forms.Timer animationTimer;
        private int startY;
        private int endY;
        private DevExpress.XtraEditors.LabelControl lblThongBao;

        // =====================================================================
        // CONSTRUCTOR
        // =====================================================================
        public Vanguard()
        {
            InitializeComponent();
            this.TopMost = true;
            this.Opacity = 0;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(
                Screen.PrimaryScreen.WorkingArea.Right - this.Width,
                Screen.PrimaryScreen.WorkingArea.Bottom - this.Height);
        }

        // =====================================================================
        // ANIMATION
        // =====================================================================
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            bool complete = true;
            if (this.Opacity < 1)
            {
                this.Opacity += 0.05;
                if (this.Opacity > 1) this.Opacity = 1;
                complete = false;
            }
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

        // =====================================================================
        // LOAD
        // =====================================================================
        private void Vanguard_Load(object sender, EventArgs e)
        {
            startY = Screen.PrimaryScreen.WorkingArea.Height;
            endY = (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 2
                 + (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 3;
            this.Location = new Point(this.Location.X, startY);

            animationTimer = new System.Windows.Forms.Timer { Interval = 5 };
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();

            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string directoryPath = Path.GetDirectoryName(appPath);
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));
            string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");
            pathThumuc = rootDirectory;

            try
            {
                dbPath = File.ReadAllText(filePaths);
            }
            catch (Exception ex) { Console.WriteLine("Lỗi đọc file: " + ex.Message); }

            string password = "1@35^7*9)1";
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";

            try
            {
                var kq = ExecuteQuery("SELECT * FROM tbRegister", null);
                if (kq.Rows.Count > 0)
                {
                    savedPath = kq.Rows[0]["Hoadonpath"].ToString();

                    string pVao = Path.Combine(savedPath, $"HD{DateTime.Now.Year}", "HDVao");
                    CreateFolder(pVao);
                    string pRa = Path.Combine(savedPath, $"HD{DateTime.Now.Year}", "HDRa");
                    CreateFolder(pRa);
                }
            }
            catch (Exception ex) { XtraMessageBox.Show(ex.Message); }

            // ===== Pipeline chính =====
            try
            {
                RunWarningPipeline();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi load cảnh báo: " + ex.Message);
            }
        }

        // =====================================================================
        // PIPELINE CHÍNH — thay thế LoadMessageOld/LoadMessage/LoadMeaasgeold2/LoadGrid
        // =====================================================================
        DataTable tbHttk { get; set; }
        private void RunWarningPipeline()
        {
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            // -------------------------------------------------------------
            // STEP 1: BUILD CHUNGTUHD (chỉ 1 lần)
            // -------------------------------------------------------------
            BuildChungTuHD();

            // -------------------------------------------------------------
            // STEP 2: LOAD STATIC DATA SONG SONG
            // -------------------------------------------------------------
            DataTable dtHoaDonCT = null, dtKhachHang = null, dtImport = null,
                      dtTonKho = null, dtVattu = null, dtPhanLoai = null;

            Parallel.Invoke(
                () => dtHoaDonCT = ExecuteQuery("SELECT MaSo, KyHieu, NgayPH, MaKhachHang FROM HoaDon", null),
                () => dtKhachHang = ExecuteQuery("SELECT MaSo, MST, Ten FROM KhachHang", null),
                () => dtImport = ExecuteQuery("SELECT * FROM tbImport", null),
                () => dtTonKho = ExecuteQuery("SELECT * FROM TonKho", null),
                () => dtVattu = ExecuteQuery("SELECT * FROM Vattu", null),
                () => dtPhanLoai = ExecuteQuery("SELECT * FROM PhanLoaiVattu", null)
            );

            tbImport = dtImport;
            gettbChungtu = ExecuteQuery("SELECT MaCT, MaLoai, SoHieu, MaTKTCNo, MaTKTCCo, SoPS, SoPS2No, SoPS2Co, NgayImport, MaVattu FROM ChungTu", null);
            string qrGethttk = "select MaSo,SoHieu,Cap,Loai,TK_ID,TK_ID2 from HeThongTK";
            tbHttk = ExecuteQuery(qrGethttk, null);
            // -------------------------------------------------------------
            // STEP 3: BUILD LOOKUP + DICTIONARIES (1 lần)
            // -------------------------------------------------------------
            BuildLookups(dtKhachHang, dtImport, dtTonKho, dtVattu, dtPhanLoai);

            // -------------------------------------------------------------
            // STEP 4: LOAD ACCOUNT BALANCE — 1 QUERY DUY NHẤT
            // -------------------------------------------------------------
            _accountBalances = LoadAccountBalances(currentYear, currentMonth);

            // -------------------------------------------------------------
            // STEP 5: LOAD EXCEL SONG SONG — 1 LẦN
            // -------------------------------------------------------------
            _excelCache = LoadAllExcel(savedPath, currentYear, currentMonth);

            // -------------------------------------------------------------
            // STEP 6: XỬ LÝ TỪNG THÁNG (KHÔNG query DB, KHÔNG đọc file)
            // -------------------------------------------------------------
            var months = new List<MonthWarning>(currentMonth);
            for (int i = currentMonth; i >= 1; i--)
            {
                if (!_excelCache.TryGetValue(i, out var cache)) continue;
                if (cache.Vao.Count == 0 && cache.Ra.Count == 0) continue;

                var monthData = ProcessMonth(i, currentYear, cache.Vao, cache.Ra);
                months.Add(monthData);
            }

            // -------------------------------------------------------------
            // STEP 7: RENDER UI
            // -------------------------------------------------------------
            RenderMonths(months);
        }

        // =====================================================================
        // BUILD CHUNGTUHD — chỉ chạy 1 lần
        // =====================================================================
        private void BuildChungTuHD()
        {
            lstChungTuHD.Clear();
            var chungTuGroups = new Dictionary<int, List<DataRow>>();

            var allChungTu = ExecuteQuery(
                "SELECT MaCT, MaLoai, SoHieu, MaTKTCNo, MaTKTCCo, SoPS, SoPS2No, SoPS2Co, NgayImport, MaVattu FROM ChungTu",
                null);

            foreach (DataRow r in allChungTu.Rows)
            {
                int maCT = Convert.ToInt32(r["MaCT"]);
                if (!chungTuGroups.TryGetValue(maCT, out var list))
                {
                    list = new List<DataRow>();
                    chungTuGroups[maCT] = list;
                }
                list.Add(r);
            }

            var tonghop = ExecuteQuery(@"
                SELECT c.MaCT, c.MaLoai, c.NgayCT, c.NgayGS, c.ThangCT, c.SoHieu, c.NgayImport,
                       h.MaKhachHang, h.KyHieu
                FROM ChungTu c
                INNER JOIN HoaDon h ON c.Maso = h.Maso", null);

            var tbKhachHang = ExecuteQuery("SELECT MaSo, MST, Ten FROM KhachHang", null);
            var dictKhachHang = new Dictionary<string, (string MST, string Ten)>();
            foreach (DataRow r in tbKhachHang.Rows)
            {
                string maSo = r["MaSo"].ToString();
                if (!dictKhachHang.ContainsKey(maSo))
                    dictKhachHang[maSo] = (r["MST"].ToString(), r["Ten"].ToString());
            }

            var processed = new HashSet<int>();

            foreach (DataRow row in tonghop.Rows)
            {
                int mact = Convert.ToInt32(row["MaCT"]);
                if (!processed.Add(mact)) continue;

                string maLoai = row["MaLoai"].ToString();
                var item = new ChungTuHD
                {
                    MaCT = mact,
                    SoHieu = row["SoHieu"].ToString(),
                    KHHD = row["KyHieu"].ToString(),
                    NgayCT = DateTime.Parse(row["NgayCT"].ToString())
                };
                if (item.SoHieu == "192" && item.NgayCT.Month==9)
                {
                    int test = 10;
                }
                if (DateTime.TryParseExact(row["NgayImport"]?.ToString(), "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ngayImport))
                    item.NgayImport = ngayImport;
                else
                    item.NgayImport = DateTime.MinValue;

                if (maLoai == "0" || maLoai == "1") item.Type = 1;
                else if (maLoai == "8") item.Type = 2;
                else item.Type = 0;

                string maKH = row["MaKhachHang"].ToString();
                if (dictKhachHang.TryGetValue(maKH, out var kh))
                {
                    item.MST = kh.MST;
                    item.TenKH = kh.Ten;
                }

                if ((maLoai == "8" || maLoai == "0" || maLoai == "1")
                    && chungTuGroups.TryGetValue(mact, out var rows))
                {
                    double tienTrcThue = 0, tienThue = 0;

                    foreach (var r in rows)
                    {
                        double soPS = Convert.ToDouble(r["SoPS"]);

                        if (maLoai == "8")
                        {
                            string maTK = r["MaTKTCCo"].ToString();
                            string matkno = r["MaTKTCNo"].ToString();
                            double soPS2Co = Convert.ToDouble(r["SoPS2Co"]);
                            string Mavt= r["MaVattu"].ToString();
                            if (maTK == "14038") tienThue += soPS;
                            else 
                            if (soPS != 0 && matkno != "169" && soPS2Co != 0)
                            {
                                tienTrcThue += soPS;
                            }
                            else
                            {
                                if (soPS != 0 && soPS2Co == 0)
                                {
                                    tienTrcThue += soPS;
                                    if(maTK != "169" && Mavt=="0")
                                        item.Hangnull = 1;
                                }
                            }
                        }
                        else // maLoai == 0 hoặc 1
                        {
                            string maTK = r["MaTKTCNo"].ToString();
                            string matkco = r["MaTKTCCo"].ToString();
                            double soPS2No = Convert.ToDouble(r["SoPS2No"]);

                            if (maTK == "5108") tienThue += soPS;
                            else if (soPS != 0)
                            {
                                if (matkco != "169" && soPS2No != 0)
                                    tienTrcThue += soPS;
                                else 
                                if (maTK == "161" || maTK == "160" || maTK=="76" || maTK=="37")
                                    tienTrcThue += soPS;

                                if (matkco == "169")
                                    tienTrcThue -= soPS;
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

        // =====================================================================
        // BUILD LOOKUPS — 1 lần duy nhất
        // =====================================================================
        private void BuildLookups(DataTable dtKhachHang, DataTable dtImport,
                                  DataTable dtTonKho, DataTable dtVattu, DataTable dtPhanLoai)
        {
            // ---- Khách hàng dict ----
            _dictKhachHang = dtKhachHang.AsEnumerable()
                .GroupBy(r => r["MaSo"].ToString())
                .ToDictionary(g => g.Key, g => new KhachHangInfo
                {
                    MST = g.First()["MST"].ToString(),
                    Ten = g.First()["Ten"].ToString()
                });

            // ---- ChungTuHD dict (tra cứu nhanh) ----
            _chungTuDict = lstChungTuHD
                .GroupBy(m => $"{Helpers.RemoveLeadingZeros(m.SoHieu).TrimEnd('.')}|{m.KHHD}")
                .ToDictionary(g => g.Key, g => g.First());

            // ---- ChungTu theo MaCT ----
            _gettbChungTuByMaCT = gettbChungtu.AsEnumerable()
                .GroupBy(r => Convert.ToInt32(r["MaCT"]))
                .ToDictionary(g => g.Key, g => g.ToList());

            // ---- Vattu dict ----
            _vattuDict = dtVattu.AsEnumerable()
                .GroupBy(r => r["MaSo"].ToString())
                .ToDictionary(g => g.Key, g => g.First());

            // ---- Phân loại vattu ----
            _phanLoaiDict = dtPhanLoai.AsEnumerable()
                .GroupBy(r => r["MaSo"].ToString())
                .ToDictionary(g => g.Key, g => g.First()["SoHieu"].ToString());

            // ---- Lookup hóa đơn ----
            lookupHoaDonCT.Clear();
            DataTable dtAllHoaDon = ExecuteQuery(@"
                SELECT hd.SoHD, hd.KyHieu, hd.NgayPH, hd.MaKhachHang, kh.MST, ct.NgayCT, ct.MaLoai
                FROM ((Hoadon hd
                    INNER JOIN Chungtu ct ON hd.MaSo = ct.MaSo)
                    INNER JOIN KhachHang kh ON hd.MaKhachHang = kh.MaSo)
                WHERE hd.KyHieu <> '...'", null);

            foreach (DataRow row in dtAllHoaDon.Rows)
            {
                string soHD = Helpers.RemoveLeadingZeros(row["SoHD"]?.ToString() ?? "")
                                    .Replace(".", "").Trim();
                soHD = RemoveLeadingZeros(soHD);

                string kyHieu = row["KyHieu"]?.ToString() ?? "";
                DateTime ngayPH = ((DateTime)row["NgayCT"]).Date;
                string mst = row["MST"]?.ToString() ?? "";
                int maLoai = int.Parse(row["MaLoai"]?.ToString() ?? "1");
                maLoai = (maLoai == 8) ? 2 : 1;

                lookupHoaDonCT.Add((mst, soHD, kyHieu, ngayPH, maLoai));
            }
        }

        // =====================================================================
        // LOAD ACCOUNT BALANCE — 1 QUERY DUY NHẤT cho tất cả tháng
        // =====================================================================
        private Dictionary<int, AccountBalance> LoadAccountBalances(int year, int maxMonth)
        {
            var result = new Dictionary<int, AccountBalance>();

            // Build dynamic SELECT
            var cols = new List<string>();
            for (int m = 1; m <= maxMonth; m++)
            {
                cols.Add($"DuNo_{m - 1} AS DkNo_{m}");
                cols.Add($"DuCo_{m - 1} AS DkCo_{m}");
                cols.Add($"No_{m} AS PsNo_{m}");
                cols.Add($"Co_{m} AS PsCo_{m}");
                cols.Add($"DuNo_{m} AS CkNo_{m}");
                cols.Add($"DuCo_{m} AS CkCo_{m}");
            }

            string query = $@"
                SELECT Cap, {string.Join(", ", cols)}
                FROM HeThongTK
                WHERE (MaTC = 0 OR MaTC = MaSo OR TK_ID3 MOD 10 >= 1)
                  AND Loai > 0
                  AND Cap = 0";

            DataTable dt;
            try { dt = ExecuteQuery(query, null); }
            catch
            {
                // fallback: nếu query fail, gọi từng tháng như cũ
                return LoadAccountBalancesFallback(year, maxMonth);
            }

            for (int m = 1; m <= maxMonth; m++)
            {
                decimal Sum(string name) => dt.AsEnumerable()
                    .Where(r => r[name] != DBNull.Value)
                    .Sum(r => Convert.ToDecimal(r[name]));

                result[m] = new AccountBalance
                {
                    DkNo = Sum($"DkNo_{m}"),
                    DkCo = Sum($"DkCo_{m}"),
                    PsNo = Sum($"PsNo_{m}"),
                    PsCo = Sum($"PsCo_{m}"),
                    CkNo = Sum($"CkNo_{m}"),
                    CkCo = Sum($"CkCo_{m}")
                };
            }
            return result;
        }

        private Dictionary<int, AccountBalance> LoadAccountBalancesFallback(int year, int maxMonth)
        {
            var result = new Dictionary<int, AccountBalance>();
            for (int m = 1; m <= maxMonth; m++)
            {
                var dt = GetHeThongTK(m, year);
                decimal Sum(string col) => dt.AsEnumerable()
                    .Where(r => r["Cap"]?.ToString() == "0" && r[col] != DBNull.Value)
                    .Sum(r => Convert.ToDecimal(r[col]));

                result[m] = new AccountBalance
                {
                    DkNo = Sum("DkNo"),
                    DkCo = Sum("DkCo"),
                    PsNo = Sum("PsNo"),
                    PsCo = Sum("PsCo"),
                    CkNo = Sum("CkNo"),
                    CkCo = Sum("CkCo")
                };
            }
            return result;
        }

        // =====================================================================
        // LOAD EXCEL SONG SONG
        // =====================================================================
        private Dictionary<int, (List<HoaDonNhap> Vao, List<HoaDonNhap> Ra)>
            LoadAllExcel(string savedPath, int year, int maxMonth)
        {
            var result = new ConcurrentDictionary<int, (List<HoaDonNhap>, List<HoaDonNhap>)>();
            string pathYear = $"HD{year}";

            Parallel.For(1, maxMonth + 1, month =>
            {
                string pathVao = Path.Combine(savedPath, pathYear, "HDVao", month.ToString());
                string pathRa = Path.Combine(savedPath, pathYear, "HDRa", month.ToString());

                var vao = LoadExcelFolder(pathVao, isVao: true);
                var ra = LoadExcelFolder(pathRa, isVao: false);

                result[month] = (vao, ra);
            });

            return result.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private List<HoaDonNhap> LoadExcelFolder(string folder, bool isVao)
        {
            var list = new List<HoaDonNhap>();
            if (!Directory.Exists(folder)) return list;

            IEnumerable<string> files;
            try { files = Directory.EnumerateFiles(folder, "*.xlsx", SearchOption.AllDirectories); }
            catch { return list; }

            foreach (var file in files)
            {
                try
                {
                     var wb = new XLWorkbook(file);
                    var ws = wb.Worksheet(1);
                    bool isMayTinhTien = file.Contains("MayTinhTien");

                    foreach (var row in ws.RowsUsed().Skip(3))
                    {
                        try
                        {
                            string soHD = Helpers.RemoveLeadingZeros(row.Cell("D").Value.ToString()).Trim();
                            
                            if (!DateTime.TryParse(row.Cell("E").Value.ToString(), out var nLap)) continue;
                            if (soHD == "192" && nLap.Date.Month == 9 && !isVao)
                            {
                                int test = 10;
                            }
                                string kyHieu = row.Cell("C").Value.ToString();
                            string mst = isVao ? row.Cell("F").Value.ToString() : row.Cell("H").Value.ToString();

                            double tienTrcThue, tienThue, tongTien;
                            if (isMayTinhTien)
                            {
                                tienTrcThue = ParseMoney(row.Cell("L").Value.ToString());
                                tienThue = ParseMoney(row.Cell("M").Value.ToString());
                                tongTien = ParseMoney(row.Cell("O").Value.ToString());
                            }
                            else
                            {
                                string colT = isVao ? "K" : "K";
                                string colThue = isVao ? "L" : "L";
                                tienTrcThue = ParseMoney(row.Cell(colT).Value.ToString());
                                tienThue = ParseMoney(row.Cell(colThue).Value.ToString());
                                tongTien = ParseMoney(row.Cell("O").Value.ToString());
                            }

                            list.Add(new HoaDonNhap
                            {
                                SoHD = soHD,
                                NLap = nLap,
                                KHHD = kyHieu,
                                MST = mst,
                                TienTrcThue = tienTrcThue,
                                TienThue = tienThue,
                                TongTienTT = tongTien,
                                Type=isVao==true?1:2
                            });
                        }
                        catch { }
                    }
                }
                catch { }
            }
            return list;
        }

        // =====================================================================
        // XỬ LÝ TỪNG THÁNG — KHÔNG query DB, KHÔNG đọc file
        // =====================================================================
        private MonthWarning ProcessMonth(int month, int year, List<HoaDonNhap> lstVao, List<HoaDonNhap> lstRa)
        {
            var monthData = new MonthWarning
            {
                Month = $"{month:00}/{year}",
                Warnings = new List<Warning>()
            };

            // ---- 1. Tồn kho âm ----
            string hangam = CheckNegativeInventory(month, monthData);

            // ---- 2. Tài khoản ----
            var acc = _accountBalances.TryGetValue(month, out var b) ? b : new AccountBalance();

            // ---- 3. Đối chiếu hóa đơn ----
            var inv = CheckInvoices(month, lstVao, lstRa);

            // ---- 4. Import lỗi ----
            var getimportloi = tbImport.AsEnumerable()
                .Where(m => m.Field<DateTime>("NLap").Date.Month == month && m["Status"].ToString() == "2")
                .ToList();

            // ---- 5. WARNINGS ----
            if (inv.HdChuaNhapVao > 0 || inv.HdChuaNhapRa > 0)
            {
                var parts = new List<string>();
                if (inv.HdChuaNhapVao > 0) parts.Add($"{inv.HdChuaNhapVao} HĐ đầu vào");
                if (inv.HdChuaNhapRa > 0) parts.Add($"{inv.HdChuaNhapRa} HĐ đầu ra");

                monthData.Warnings.Add(new Warning
                {
                    Text = $"🔴 {string.Join(" , ", parts)} chưa nhập",
                    Color = Color.FromArgb(220, 53, 69)
                });
            }

            if (inv.HdSaiThongTin > 0)
            {
                monthData.Warnings.Add(new Warning
                {
                    Text = $"🟠 {inv.HdSaiThongTin} hóa đơn sai thông tin: {inv.DsSai.TrimEnd(',')}",
                    Color = Color.DarkBlue
                });
            }

            if (inv.HdNullhang > 0)
            {
                monthData.Warnings.Add(new Warning
                {
                    Text = $"🟠 {inv.HdNullhang} hoá đơn bị thiếu mã hàng : {inv.DsNullhang.TrimEnd(',')}",
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

            if (acc.DkNo != acc.DkCo)
            {
                monthData.Warnings.Add(new Warning
                {
                    Text = $"🔵 Số dư đầu kỳ chưa cân: {acc.DkNo} - {acc.DkCo}",
                    Color = Color.FromArgb(23, 162, 184)
                });
            }

            if (acc.PsNo != acc.PsCo)
            {
                monthData.Warnings.Add(new Warning
                {
                    Text = $"🔵 Số dư trong kỳ chưa cân: {acc.PsNo} - {acc.PsCo}",
                    Color = Color.FromArgb(40, 167, 69)
                });
            }

            if (inv.DsNhapDuVao.Count > 0)
            {
                monthData.Warnings.Add(new Warning
                {
                    Text = $"🟣 HĐ đầu vào nhập dư: {string.Join("  ", inv.DsNhapDuVao.Select(x => $"[{x}]"))}",
                    Color = Color.DarkOrange
                });
            }

            if (inv.DsNhapDuRa.Count > 0)
            {
                monthData.Warnings.Add(new Warning
                {
                    Text = $"🟣 HĐ đầu ra nhập dư: {string.Join(",", inv.DsNhapDuRa.Select(x => $"[{x}]"))}",
                    Color = Color.FromArgb(111, 66, 193)
                });
            }

            if (!string.IsNullOrEmpty(hangam))
            {
                var dsMaHang = hangam.TrimEnd(',');
                monthData.Warnings.Add(new Warning
                {
                    Text = $"🟡 Có {vattuAms.Count} ⚠️ mặt hàng đang âm: {dsMaHang}",
                    Color = Color.Green
                });
            }

            monthData.Total = monthData.Warnings.Count;
            return monthData;
        }

        // =====================================================================
        // CHECK NEGATIVE INVENTORY
        // =====================================================================
        private string CheckNegativeInventory(int month, MonthWarning monthData)
        {
            string columnName = $"Luong_{month}";
            var sb = new StringBuilder();

            vattuAms = new List<VattuAm>(); // reset local

            if (!_vattuDict.Any()) return "";

            // Tìm cột tồn kho
            var dtTonKho = ExecuteQuery($"SELECT MaVatTu, {columnName} FROM TonKho", null);
            if (dtTonKho == null) return "";

            foreach (DataRow row in dtTonKho.Rows)
            {
                object value = row[columnName];
                if (value == null || value == DBNull.Value) continue;

                double soLuong;
                try { soLuong = Convert.ToDouble(value); } catch { continue; }
                if (soLuong >= 0) continue;

                string maVT = row["MaVatTu"].ToString();
                if (!_vattuDict.TryGetValue(maVT, out var vt)) continue;

                sb.Append(vt["SoHieu"]).Append(',');

                vattuAms.Add(new VattuAm
                {
                    MaVT = vt["SoHieu"].ToString(),
                    TenVT = vt["TenVattu"].ToString(),
                    MaPL = _phanLoaiDict.TryGetValue(vt["MaPhanLoai"].ToString(), out var pl) ? pl : ""
                });
            }

            return sb.ToString();
        }

        // =====================================================================
        // CHECK INVOICES — dùng dict/hashset
        // =====================================================================
        private class InvoiceCheckResult
        {
            public int HdChuaNhapVao, HdChuaNhapRa, HdSaiThongTin, HdNullhang;
            public string DsSai = "";
            public string DsNullhang = "";
            public List<string> DsNhapDuVao = new List<string>();
            public List<string> DsNhapDuRa = new List<string>();
        }

        private InvoiceCheckResult CheckInvoices(int month, List<HoaDonNhap> lstVao, List<HoaDonNhap> lstRa)
        {
            var r = new InvoiceCheckResult();
            var sbSai = new StringBuilder();
            var sbNull = new StringBuilder();

            void CheckOne(HoaDonNhap hd, int type)
            {
                // Kiểm tra tồn tại trong lookup
                if (!KiemtrahoadonCT(hd.SoHD, hd.KHHD, hd.NLap, hd.MST, type))
                {
                    if (type == 1) r.HdChuaNhapVao++;
                    else r.HdChuaNhapRa++;
                }

                string key = $"{Helpers.RemoveLeadingZeros(hd.SoHD).TrimEnd('.')}|{hd.KHHD}";
                if (!_chungTuDict.TryGetValue(key, out var ct)) return;

                // Kiểm tra thiếu mã hàng
                if (_gettbChungTuByMaCT.TryGetValue(ct.MaCT, out var rows))
                {
                    bool checkNullHang = rows.Any(m =>
                        m["SoPS"].ToString() != "0"
                        && m["MaTKTCNo"].ToString()!= "5108"
                        && m["MaTKTCCo"].ToString() != "14038"
                        && m["MaTKTCCo"].ToString() != "82"
                        && m["MaTKTCCo"].ToString() != "169"
                             && m["MaTKTCCo"].ToString() != "125"
                                  && m["MaTKTCCo"].ToString() != "126"
                        && m["MaTKTCNo"].ToString() != "160"
                        && m["MaTKTCNo"].ToString() != "161"
                        && (m["MaTKTCNo"].ToString() != "0" || m["MaTKTCCo"].ToString() != "0")
                        && m["MaTKTCNo"].ToString() != "75"
                        && m["MaTKTCNo"].ToString() != "76"
                        && m["MaTKTCNo"].ToString() != "77"
                          && m["MaTKTCNo"].ToString() != "37"
                        && m["MaVattu"].ToString() == "0");

                    if (checkNullHang)
                    {
                        r.HdNullhang++;
                        sbNull.Append(type == 1 ? $"[{hd.SoHD}](v) , " : $"{hd.SoHD}(r), ");
                    }
                }

                string lyDo = CompareHoaDon(hd, ct);
                if (lyDo != null)
                {
                    r.HdSaiThongTin++;
                    if (type == 1)
                        sbSai.Append($"◆ {hd.SoHD}(v)({lyDo})\n");
                    else
                        sbSai.Append($"◆ {hd.SoHD}(r)({lyDo})\n");
                }
            }

            foreach (var hd in lstVao) CheckOne(hd, 1);
            foreach (var hd in lstRa) CheckOne(hd, 2);

            r.DsSai = sbSai.ToString();
            r.DsNullhang = sbNull.ToString();

            // Hóa đơn nhập dư — dùng HashSet
            var setVao = lstVao.Select(x => x.SoHD).ToHashSet();
            var setRa = lstRa.Select(x => x.SoHD).ToHashSet();
            if (month == 1)
            {
                int dasda = 100;
            }
            r.DsNhapDuVao = lookupHoaDonCT
                .Where(m => m.NLap.Month == month && m.Type == 1 && !setVao.Contains(m.SoHD))
                .Select(m => m.SoHD).ToList();

            r.DsNhapDuRa = lookupHoaDonCT
                .Where(m => m.NLap.Month == month && m.Type == 2 && !setRa.Contains(m.SoHD))
                .Select(m => m.SoHD).ToList();

            return r;
        }

        // =====================================================================
        // RENDER UI
        // =====================================================================
        private void RenderMonths(List<MonthWarning> months)
        {
            panelControl1.SuspendLayout();
            panelControl1.Controls.Clear();
            panelControl1.BackColor = Color.FromArgb(245, 247, 250);

            var panelScroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 247, 250)
            };
            panelControl1.Controls.Add(panelScroll);

            panelScroll.SuspendLayout();
            panelScroll.CreateControl();

            int yPos = 10;
            const int leftMargin = 10;
            const int rightMargin = 26;

            panelControl1.PerformLayout();
            panelScroll.PerformLayout();
            int baseWidth = panelControl1.ClientSize.Width;   // ✅ có giá trị
            int panelWidth = baseWidth - leftMargin - rightMargin - 20;
            if (panelWidth < 200) panelWidth = 200;

            foreach (var monthData in months)
            {
                var card = CreateMonthCard(monthData, panelWidth, leftMargin, yPos);
                panelScroll.Controls.Add(card);
                yPos += card.Height + 12;
            }

            panelScroll.AutoScrollMinSize = new Size(0, yPos + 10);
            panelScroll.HorizontalScroll.Enabled = false;
            panelScroll.HorizontalScroll.Visible = false;

            panelScroll.ResumeLayout();
            panelControl1.ResumeLayout();
        }

        private Panel CreateMonthCard(MonthWarning monthData, int panelWidth, int leftMargin, int yPos)
        {
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

            // ---- HEADER ----
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

            var icon = new DevExpress.XtraEditors.SvgImageBox
            {
                Location = new Point(14, 12),
                Size = new Size(28, 28),
                SvgImage = GetCalendarSvg(hasWarning)
            };
            header.Controls.Add(icon);

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

            // ---- BODY ----
            var body = new FlowLayoutPanel
            {
                Location = new Point(0, headerHeight),
                Width = panelWidth,
                AutoSize = false,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(14, 8, 14, 12),
                BackColor = Color.White
            };

            int warningWidth = panelWidth - 28 - 12;
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
                    AutoSize = false,
                    BackColor = Color.Transparent
                };
                row.Controls.Add(lbl);

                body.Controls.Add(row);
                bodyY += rowHeight + 4;
            }

            body.Height = bodyY + body.Padding.Top + body.Padding.Bottom;

            card.Controls.Add(header);
            card.Controls.Add(body);

            card.Height = headerHeight + body.Height;
            card.Width = panelWidth;
            return card;
        }

        // =====================================================================
        // HELPERS
        // =====================================================================
        private int MeasureRowHeight(string text, int width, int minHeight)
        {
            if (string.IsNullOrEmpty(text)) return minHeight;
             var font = new Font("Segoe UI", 9F);
            Size measured = TextRenderer.MeasureText(
                text, font, new Size(width, int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            return Math.Max(measured.Height + 10, minHeight);
        }

        private int GetLabelHeight(DevExpress.XtraEditors.LabelControl lbl, int width, int minHeight)
        {
            if (string.IsNullOrEmpty(lbl.Text)) return minHeight;
            Size measured = TextRenderer.MeasureText(
                lbl.Text, lbl.Font, new Size(width, int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            return Math.Max(measured.Height + 6, minHeight);
        }

        private static double ParseMoney(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;
            return double.TryParse(s, out double v) ? Math.Round(v) : 0;
        }
        public static List<ChungTuHD> dsHoadonloi=new List<ChungTuHD>();

        private static string CompareHoaDon(HoaDonNhap hd, ChungTuHD ct)
        {
            if(ct.SoHieu=="550" && ct.NgayCT.Month == 8)
            {
                int aa = 100;
            }
            var loi = new List<string>();
            if(ct.TienThue==0 && ct.TienThue==0 && ct.TongTien == 0)
            {
                return null;
            }
            if (hd.TienTrcThue != 0 && hd.TienTrcThue != ct.TienTrcThue && hd.NLap == ct.NgayCT && hd.Type==ct.Type)
                loi.Add("Tiền trước thuế bị lệch");
            if (hd.TienThue != 0 && hd.TienThue != ct.TienThue && hd.NLap == ct.NgayCT && hd.Type == ct.Type)
                loi.Add("Tiền thuế bị lệch");
            if (hd.Type == 1)
            {
                if (ct.NgayCT.Date != hd.NLap.Date && loi.Count == 0 && hd.Type == ct.Type && hd.MST==ct.MST)
                {
                    loi.Add($"Ngày chứng từ bị sai {ct.NgayCT.Date.ToShortDateString()}, ngày đúng là {hd.NLap.Date.ToShortDateString()}");
                    ct.NgayCT = hd.NLap.Date;
                    dsHoadonloi.Add(ct);
                }
            }
            else
            {
                if (ct.NgayCT.Date != hd.NLap.Date && loi.Count == 0 && hd.Type == ct.Type)
                {
                    loi.Add($"Ngày chứng từ bị sai {ct.NgayCT.Date.ToShortDateString()},  ngày đúng là {hd.NLap.Date.ToShortDateString()}");
                    ct.NgayCT = hd.NLap.Date;
                    dsHoadonloi.Add(ct);
                }
            }
            //if (ct.Hangnull == 1)
            //{
            //    loi.Add($"Thiếu mã hàng");
            //}
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
             var ms = new MemoryStream(Encoding.UTF8.GetBytes(svg));
            return SvgImage.FromStream(ms);
        }

        private void CreateFolder(string path)
        {
            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                for (int month = 1; month <= 12; month++)
                {
                    string folderPath = Path.Combine(path, month.ToString("D1"));
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tạo thư mục: {ex.Message}");
                throw;
            }
        }

        public static string RemoveLeadingZeros(string invoiceNumber)
        {
            if (string.IsNullOrEmpty(invoiceNumber)) return invoiceNumber;
            return Regex.Replace(invoiceNumber, "^0+", "");
        }

        private bool KiemtrahoadonCT(string SoHD, string KyHieu, DateTime NLap, string Mst, int type)
        {
            if (Mst == "KL") Mst = "00";
            if (Mst.Length < 10)
                return lookupHoaDonCT.Any(m => m.SoHD == SoHD && m.KyHieu == KyHieu && m.NLap == NLap && m.Type == type);
            return lookupHoaDonCT.Contains((Mst, SoHD, KyHieu, NLap, type));
        }

        public DataTable ExecuteQuery(string query, params OleDbParameter[] parameters)
        {
            var dataTable = new DataTable();
             var connection = new OleDbConnection(connectionString);
            try
            {
                connection.Open();
                 var command = new OleDbCommand(query, connection);
                if (parameters != null) command.Parameters.AddRange(parameters);
                 var dataAdapter = new OleDbDataAdapter(command);
                dataAdapter.Fill(dataTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dataTable;
        }

        // =====================================================================
        // GetHeThongTK — giữ lại làm fallback
        // =====================================================================
        public DataTable GetHeThongTK(int thang, int nam)
        {
            var dt = new DataTable();
            string colDkNo = $"DuNo_{thang - 1}", colDkCo = $"DuCo_{thang - 1}";
            string colPsNo = $"No_{thang}", colPsCo = $"Co_{thang}";
            string colCkNo = $"DuNo_{thang}", colCkCo = $"DuCo_{thang}";

            string query = $@"
                SELECT DISTINCTROW HeThongTK.SoHieu, HeThongTK.Cap, HeThongTK.Ten,
                       HeThongTK.Kieu, HeThongTK.Loai,
                       HeThongTK.{colDkNo} AS DkNo, HeThongTK.{colDkCo} AS DkCo,
                       HeThongTK.{colPsNo} AS PsNo, HeThongTK.{colPsCo} AS PsCo,
                       HeThongTK.KC_N, HeThongTK.KC_C,
                       HeThongTK.{colCkNo} AS CkNo, HeThongTK.{colCkCo} AS CkCo
                FROM HeThongTK
                WHERE ((HeThongTK.MaTC = 0 OR HeThongTK.MaTC = HeThongTK.MaSo)
                       OR (HeThongTK.TK_ID3 MOD 10 >= 1))
                  AND HeThongTK.Loai > 0
                  AND HeThongTK.Cap <= 2
                  AND (HeThongTK.{colCkNo} <> 0 OR HeThongTK.{colCkCo} <> 0
                       OR HeThongTK.{colPsNo} <> 0 OR HeThongTK.{colPsCo} <> 0)";

            try
            {
                 var conn = new OleDbConnection(connectionString);
                 var cmd = new OleDbCommand(query, conn);
                conn.Open();
                 var da = new OleDbDataAdapter(cmd);
                da.Fill(dt);
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}"); }
            return dt;
        }

        // =====================================================================
        // FORM EVENTS
        // =====================================================================
        private void simpleButton1_Click(object sender, EventArgs e) => this.Close();
        private void Vanguard_Shown(object sender, EventArgs e)
        {
            this.Activate();
            this.Focus();
            this.BringToFront();
            this.TopMost = true;
        }

        private void svgImageBox1_Click(object sender, EventArgs e)
        {
            //Thực hiện fix lỗi ngày 
            foreach (var d in dsHoadonloi)
            {
                try
                {
                    string query = @"UPDATE ChungTu SET NgayCT = ?,NgayGS=? where  MaCT = ?";

                    var parameters = new OleDbParameter[]
             {
                                new OleDbParameter("?",d.NgayCT),
                                  new OleDbParameter("?",d.NgayCT),
                                    new OleDbParameter("?",d.MaCT),
             };
                    int rowsAffected = ExecuteQueryResult(query, parameters);
                }
                catch(Exception ex)
                {

                }
            }
            XtraMessageBox.Show("Đã thực hiện sửa lỗi ngày chứng từ bị sai");
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

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            //Thực hiện fix lỗi ngày 
            foreach (var d in dsHoadonloi)
            {
                try
                {
                    string query = @"UPDATE ChungTu SET NgayCT = ?,NgayGS=? where  MaCT = ?";

                     var parameters = new OleDbParameter[]
                     {
                                        new OleDbParameter("?",d.NgayCT),
                                          new OleDbParameter("?",d.NgayCT),
                                            new OleDbParameter("?",d.MaCT),
                     };
                    int rowsAffected = ExecuteQueryResult(query, parameters);
                }
                catch (Exception ex)
                {

                }
            }
            XtraMessageBox.Show("Đã thực hiện sửa lỗi ngày chứng từ bị sai");
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}