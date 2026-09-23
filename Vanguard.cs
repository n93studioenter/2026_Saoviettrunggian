using ClosedXML.Excel;
using DevExpress.CodeParser;
using DevExpress.Utils;
using DevExpress.Utils.Extensions;
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
using static SaovietTax.Vanguard;
using DataTable = System.Data.DataTable;
using Size = System.Drawing.Size;

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
            LoadMeaasge();
        }
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
        private void LoadMeaasge()
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
                if(excelFiles.Count==0)
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

                //var warning2 = new Warning();
                //warning2.Text = "🔵 6 hàng chưa nhập kho";
                //warning2.Color = Color.Blue;
                //month1.Warnings.Add(warning2);


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
                    foreach( var m in group)
                    {
                        warning4.Text = $"🔵 Nhóm {m.Key} Có {m.Count()} ⚠️ hàng đang âm";
                    } 
                    warning4.Color = Color.DarkCyan;
                    month1.Warnings.Add(warning4);
                }

                //Tài khoản chưa cân
                if (sumDkNo != sumDkCo)
                {
                    //warningData.HethongTK += $"Số dư đầu kỳ chưa cân {sumDkNo} -  {sumDkCo}";
                    var warning4 = new Warning();
                    warning4.Text = $"🔵 Số dư đầu kỳ chưa cân {sumDkNo} -  {sumDkCo}";
                    warning4.Color = Color.DarkSeaGreen;
                    month1.Warnings.Add(warning4);
                }
                if (sumPsNo != sumPsCo)
                {
                    //warningData.HethongTK += $"Số dư trong kỳ chưa cân {sumPsNo} -  {sumPsCo}";
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
            int warningHeight = 22;

            int leftMargin = 5;
            int rightMargin = 15;
            int bottomMargin = 5;


            // ==========================================================
            // TẠO PANEL CHO TỪNG THÁNG
            // ==========================================================

            foreach (var monthData in months)
            {
                // ------------------------------------------------------
                // Panel của tháng
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


                // Chiều rộng thực tế của vùng scroll
                int panelWidth = panelScroll.ClientSize.Width
                                 - leftMargin
                                 - rightMargin;

                if (panelWidth < 100)
                    panelWidth = 100;


                // Chiều cao theo số warning
                int panelHeight =
                    headerHeight +
                    (monthData.Warnings.Count * warningHeight) +
                    bottomMargin;

                panelControl2.Size = new Size(
                    panelWidth,
                    panelHeight
                );

                panelControl2.TabIndex = 0;


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

                // 1. Kích hoạt chế độ bọc chữ (Wrap)
                labelControl2.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
                // Quan trọng: Phải bật option này để TextOptions có hiệu lực [citation:2][citation:10]
                labelControl2.Appearance.Options.UseTextOptions = true;

                // 2. Thiết lập chế độ tự động co giãn theo chiều dọc (Vertical)
                // Điều này đảm bảo label sẽ tăng chiều cao để hiển thị toàn bộ text khi bị bọc [citation:5][citation:7]
                labelControl2.AutoSizeMode = LabelAutoSizeMode.Vertical;

                // 3. Xác định chiều rộng tối đa cho label 

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
                // ======================================================

                int warningY = headerHeight;


                foreach (var warning in monthData.Warnings)
                {
                    var lblWarning =
                        new DevExpress.XtraEditors.LabelControl();

                    lblWarning.Text = warning.Text;

                    lblWarning.Font =
                        new Font(
                            "Segoe UI",
                            7.5F,
                            FontStyle.Bold);

                    lblWarning.ForeColor =
                        warning.Color;

                    lblWarning.Appearance.BackColor =
                        Color.Transparent;

                    lblWarning.Appearance.Options.UseBackColor =
                        true;

                    lblWarning.AutoSizeMode =
                        DevExpress.XtraEditors.LabelAutoSizeMode.None;


                    // Chừa khoảng cách trái phải
                    int warningWidth =
                        panelWidth - 30;

                    if (warningWidth < 50)
                        warningWidth = 50;


                    lblWarning.Size =
                        new Size(
                            warningWidth,
                            warningHeight);


                    lblWarning.Location =
                        new Point(
                            20,
                            warningY);


                    lblWarning.Appearance.TextOptions.HAlignment =
                        DevExpress.Utils.HorzAlignment.Near;

                    lblWarning.Appearance.TextOptions.VAlignment =
                        DevExpress.Utils.VertAlignment.Center;


                    panelControl2.Controls.Add(
                        lblWarning);


                    warningY += warningHeight;
                }


                // ======================================================
                // THÊM PANEL THÁNG VÀO PANEL SCROLL
                // ======================================================

                panelScroll.Controls.Add(
                    panelControl2);


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