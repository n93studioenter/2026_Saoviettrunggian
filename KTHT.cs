using ClosedXML.Excel;
using DevExpress.Data.Helpers;
using DevExpress.Utils.Extensions;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraMap.Native;
using DevExpress.XtraWaitForm;
using Newtonsoft.Json;
using SaovietTax.Database;
using SaovietTax.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tensorflow;
using Windows.Media.Protection.PlayReady;
using static SaovietTax.frmMain;
using static SaovietTax.KTHT;

namespace SaovietTax
{
    public partial class KTHT : DevExpress.XtraEditors.XtraForm
    {
        public class Hoadonsai
        {
            public string SoHD { get; set; }
            public string KHHD { get; set; }    
            public DateTime NgayLap { get; set; }
        }
        public KTHT()
        {
            InitializeComponent();
        }
        #region Khai báo
        public frmMain frmMain;
        private List<Hoadonsai> danhsachHdSaingay=new List<Hoadonsai>();  
        public class clsKTHT
        {
            public int STT { get; set; }
            public int Type { get; set; } //1 ,2 ,3, 4, 5
            public string STTType { get; set; }
            public string KHMS { get; set; }
            public string SoHD { get; set; }
            public string KHHD { get; set; }
            public DateTime NgayLap { get; set; }
            public DateTime NgayTai { get; set; }
            public int StatusImport { get; set; }   
            public DateTime NgayNhap { get; set; }

            public string MST { get; set; }
            public string MSTHD { get; set; }

            public string TenKH { get; set; }
            public string TenKHHD { get; set; }

            public double TienTrcThue { get; set; }
            public double TienTrcThueHD { get; set; }
            public double TongTienPhi { get; set; } 
            public double TienThue { get; set; }
            public double TienThueHD { get; set; }

            public double TongTienTT { get; set; }
            public double TongTienTTHD { get; set; }
            public string GhiChu { get; set; }  
            public bool Checked { get; set; }
            public string Path { get; set; }
            public int Khautruthue { get; set; }    
        }
        List<clsKTHT> clsKTHTs = new List<clsKTHT>();
        int DV1, DV2, DV3;
        string connectionString;
        string dbPath = "";
        DataTable tbimport, ChungTu, HoaDon, KhachHang, ChungTuLQ;
        #endregion

        public void Xulyexel(string token, int type, int thang)
        {
            DateTime dtFrom = new DateTime((int)cbbNam.EditValue, thang, 1);
            DateTime dtTo = dtFrom.AddMonths(1).AddDays(-1);

            string formattedDate1 = dtFrom.ToString("dd/MM/yyyy") + "T00:00:00";
            string formattedDate2 = dtTo.ToString("dd/MM/yyyy") + "T23:59:59";

            string url = "";
            string filename = "";
            string action = "Xuất excel (hóa đơn mua vào)";

            switch (type)
            {
                case 1: // Có mã
                    url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==5&type=purchase";
                    filename = $"{mstcongty}_HDDienTuDaCapMa.xlsx";
                    break;
                case 2: // Không mã
                    url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==6&type=purchase";
                    filename = $"{mstcongty}_HDDienTuKhongMa.xlsx";
                    break;
                case 3: // Máy tính tiền
                    url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==8&type=purchase";
                    filename = $"{mstcongty}_HDDienTuMayTinhTien.xlsx";
                    action = "Xuất excel (hóa đơn máy tính tiền mua vào)";
                    break;
                default:
                    return;
            }

            string currentYear = $"HD{cbbNam.EditValue}";
            string directoryPath = Path.Combine(savedPath, currentYear, "HDVao", dtFrom.Month.ToString());
            Directory.CreateDirectory(directoryPath);
            string path = Path.Combine(directoryPath, filename);

            // Nếu file đã tồn tại trong ngày hôm nay thì bỏ qua
            if (File.Exists(path))
            {
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.CreationTime.Date >= DateTime.Now.Date)
                {
                    Console.WriteLine($"File '{filename}' còn mới, bỏ qua.");
                    return;
                }
            }

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(90);

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "vi-VN,vi;q=0.9");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://hoadondientu.gdt.gov.vn");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://hoadondientu.gdt.gov.vn/tra-cuu/tra-cuu-hoa-don");
                client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\"Google Chrome\";v=\"140\", \"Chromium\";v=\"140\", \"Not=A?Brand\";v=\"24\"");
                client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");

                try
                {
                    progressPanel1.Caption = $"Đang tải file Excel tháng {thang}...";
                    Application.DoEvents();

                    using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        request.Headers.TryAddWithoutValidation("Request-Id", Guid.NewGuid().ToString());
                        request.Headers.TryAddWithoutValidation("End-Point", "/tra-cuu/tra-cuu-hoa-don");
                        request.Headers.TryAddWithoutValidation("Action", Uri.EscapeDataString(action));

                        HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();

                        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            XtraMessageBox.Show($"Bị 403 khi tải Excel tháng {thang}");
                            return;
                        }

                        response.EnsureSuccessStatusCode();

                        byte[] fileBytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();

                        if (fileBytes == null || fileBytes.Length < 2048)
                        {
                            XtraMessageBox.Show($"File Excel tháng {thang} tải về quá nhỏ hoặc lỗi");
                            return;
                        }

                        // Kiểm tra header ZIP/xlsx
                        if (!(fileBytes.Length > 4 && fileBytes[0] == 0x50 && fileBytes[1] == 0x4B))
                        {
                            string preview = Encoding.UTF8.GetString(fileBytes, 0, Math.Min(200, fileBytes.Length));
                            XtraMessageBox.Show($"Dữ liệu không phải file Excel: {preview}");
                            return;
                        }

                        File.WriteAllBytes(path, fileBytes);
                        progressPanel1.Caption = $"Đã tải xong Excel tháng {thang}";
                    }
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show($"Đã xảy ra lỗi khi tải file Excel tháng {thang}: {ex.Message}");
                }
            }
        }
        public void Xulyexel2(string token, int type, int thang)
        {
            DateTime dtFrom = new DateTime((int)cbbNam.EditValue, thang, 1);
            DateTime dtTo = dtFrom.AddMonths(1).AddDays(-1);

            string formattedDate1 = dtFrom.ToString("dd/MM/yyyy") + "T00:00:00";
            string formattedDate2 = dtTo.ToString("dd/MM/yyyy") + "T23:59:59";

            string url = "";
            string filename = "";
            string action = "Xuất excel (hóa đơn bán ra)";

            switch (type)
            {
                case 1: // Hóa đơn điện tử bán ra
                    url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2}";
                    filename = $"{mstcongty}_Hoadondientu.xlsx";
                    break;
                case 2: // Máy tính tiền bán ra
                case 3:
                    url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-excel?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2}";
                    filename = $"{mstcongty}_HDDienTuMayTinhTien.xlsx";
                    action = "Xuất excel (hóa đơn máy tính tiền bán ra)";
                    break;
                default:
                    return;
            }

            string currentYear = $"HD{cbbNam.EditValue}";
            string directoryPath = Path.Combine(savedPath, currentYear, "HDRa", dtFrom.Month.ToString());
            Directory.CreateDirectory(directoryPath);
            string path = Path.Combine(directoryPath, filename);

            // Nếu file đã tồn tại trong ngày hôm nay thì bỏ qua
            if (File.Exists(path))
            {
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.CreationTime.Date >= DateTime.Now.Date)
                {
                    Console.WriteLine($"File '{filename}' còn mới, bỏ qua.");
                    return;
                }
            }

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(90);

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "vi-VN,vi;q=0.9");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://hoadondientu.gdt.gov.vn");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://hoadondientu.gdt.gov.vn/tra-cuu/tra-cuu-hoa-don");
                client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\"Google Chrome\";v=\"140\", \"Chromium\";v=\"140\", \"Not=A?Brand\";v=\"24\"");
                client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");

                try
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        request.Headers.TryAddWithoutValidation("Request-Id", Guid.NewGuid().ToString());
                        request.Headers.TryAddWithoutValidation("End-Point", "/tra-cuu/tra-cuu-hoa-don");
                        request.Headers.TryAddWithoutValidation("Action", Uri.EscapeDataString(action));

                        HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();

                        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            Console.WriteLine($"Bị 403 khi tải Excel đầu ra tháng {thang}");
                            return;
                        }

                        response.EnsureSuccessStatusCode();

                        byte[] fileBytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();

                        if (fileBytes == null || fileBytes.Length < 2048)
                        {
                            Console.WriteLine($"File Excel đầu ra tháng {thang} quá nhỏ");
                            return;
                        }

                        if (!(fileBytes.Length > 4 && fileBytes[0] == 0x50 && fileBytes[1] == 0x4B))
                        {
                            string preview = Encoding.UTF8.GetString(fileBytes, 0, Math.Min(200, fileBytes.Length));
                            Console.WriteLine($"Không phải file Excel: {preview}");
                            return;
                        }

                        File.WriteAllBytes(path, fileBytes);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi tải Excel đầu ra tháng {thang}: {ex.Message}");
                }
            }
        }
        private void Taiexcelvao()
        {
            progressPanel1.Visible = true;
            progressPanel1.Caption = "Đang tải file Excel";
            //Lấy tháng của từ tháng
            int tuthang = int.Parse(cbbChonthang.Text.Replace("Tháng ", ""));
            int denthang = int.Parse(cbbDenthang.Text.Replace("Tháng ", ""));
            if (radDauvao.Checked)
            {
                for(int i=tuthang;i<=denthang; i++)
                {
                    Xulyexel(myTokken, 1,i);
                    Xulyexel(myTokken, 2,i);
                    Xulyexel(myTokken, 3,i);
                }
            }
            else
            {
                for (int i = tuthang; i <= denthang; i++)
                {
                    Xulyexel2(myTokken, 1,i);
                    Xulyexel2(myTokken, 2,i);
                }
                   
            }
            LoadDanhsachExcel();
            progressPanel1.Visible=false;
        }
        double TongTienExcel = 0;
        double TongTienTrcthueExcel = 0;
        private void LoadDanhsachExcel2()
        {
            progressPanel1.Visible = true;
            progressPanel1.Caption = "Đang xử lý dữ liệu";
            Application.DoEvents();
            string qr = "SELECT * FROM tbGhichuHT";
            dtGhichuht = ExecuteQuery(qr, null);
            TongTienExcel = 0;
            TongTienTrcthueExcel = 0;
            //Tải file excel về trước


            DV1 = DV2= DV3 = 0;
            clsKTHTs = new List<clsKTHT>();
            string typeHD = "";
            typeHD = radDauvao.Checked ? "HDVao" : "HDRa";
            int tuthang = int.Parse(cbbChonthang.Text.Replace("Tháng ", ""));
            int denthang= int.Parse(cbbDenthang.Text.Replace("Tháng ", ""));

            string sqlv = "SELECT DISTINCTROW KyHieu,SoHD,ChungTu.NgayCT as NgayPH,MatHang,SoLuong,ThanhTien,KhachHang.Ten,KhachHang.MST,ChungTu.SoHieu,SoPS,KhachHang.DiaChi,TyLe,HTTT,MauSo,MaCT,HoaDon.MaSo,KCT FROM  (HoaDon INNER JOIN ChungTu ON HoaDon.MaSo=ChungTu.MaSo) LEFT JOIN KhachHang ON HoaDon.MaKhachHang=KhachHang.MaSo  WHERE Loai=-1 AND HD=1 AND  (ThangCT>=? AND ThangCT<=?)  AND (HDBL=0 OR KCT=0) AND (HoaDon.DC=0 OR HD=1) ORDER BY NgayPH,MaCT";
            var parameters = new OleDbParameter[]
                     {
            new OleDbParameter("?",tuthang),
            new OleDbParameter("?",denthang),
                     };
            var kqvao = ExecuteQuery(sqlv, parameters);

            //Ra
            string sqlr = "SELECT DISTINCTROW HoaDon.KyHieu,SoHD,ChungTu.NgayCT as NgayPH,MatHang,SoLuong,ThanhTien,KhachHang.Ten,KhachHang.MST,ChungTu.SoHieu,IIF(TK_ID=3007,SoPS,-SoPS) AS Thue,ChungTu.MauSoHD as DiaChi,TyLe,HTTT,MauSo,MaCT,KCT FROM  ((HoaDon INNER JOIN ChungTu ON HoaDon.MaSo=ChungTu.MaSo) LEFT JOIN HethongTK ON ChungTu.MaTKCo=HethongTK.MaSo) LEFT JOIN KhachHang ON HoaDon.MaKhachHang=KhachHang.MaSo  WHERE HoaDon.Loai=1 AND  (ThangCT>=? AND ThangCT<=?)  AND (HoaDon.DC=0 OR HD=1) ORDER BY NgayPH";
            parameters = new OleDbParameter[]
                    {
            new OleDbParameter("?",tuthang),
            new OleDbParameter("?",denthang),
                    };
            var kqra = ExecuteQuery(sqlr, parameters);

            for (int i=tuthang;i<=denthang;i++)
            {
                string directoryPath = Path.Combine(savedPath, typeHD,i.ToString()).Trim();


                if (Directory.Exists(directoryPath))
                {
                    var excelFiles = Directory.EnumerateFiles(directoryPath, "*.xlsx", SearchOption.AllDirectories).ToList();

                    int STT = 1;
                    int totalrow = 0;
                    int dong = 1;

                    foreach (var excelFile in excelFiles)
                    {
                        if (!excelFile.Contains(mstcongty))
                        {
                            continue;
                        }
                        using (var workbook = new XLWorkbook(excelFile))
                        {
                            var worksheet = workbook.Worksheet(1); // Lấy sheet đầu tiên
                            foreach (var row in worksheet.RowsUsed().Skip(3)) {
                                totalrow += 1;
                            }

                        }

                    }
                        foreach (var excelFile in excelFiles)
                    {
                        if (!excelFile.Contains(mstcongty))
                        {
                            continue;
                        }
                        using (var workbook = new XLWorkbook(excelFile))

                        {
                            var worksheet = workbook.Worksheet(1); // Lấy sheet đầu tiên
                           
                            foreach (var row in worksheet.RowsUsed().Skip(3)) // Bỏ qua 6 hàng đầu tiên
                            {
                                clsKTHT clsKTHT = new clsKTHT();
                                progressPanel1.Caption = $"Đang đọc dòng thứ {dong}/{totalrow}";
                                Application.DoEvents();
                                if (typeHD == "HDVao")
                                {
                                    if (excelFile.Contains("HDDienTuDaCapMa"))
                                    {
                                        DV1 += 1;
                                        clsKTHT.Type = 1;
                                        clsKTHT.STTType = "";
                                    }
                                    else
                                    {
                                        if (excelFile.Contains("HDDienTuKhongMa"))
                                        {
                                            DV2 += 1;
                                            clsKTHT.Type = 2;
                                            clsKTHT.STTType = "";
                                        }
                                        else
                                        {
                                            DV3 += 1;
                                            clsKTHT.Type = 3;
                                            clsKTHT.STTType = "";
                                        }
                                    }
                                }
                                else
                                {
                                    if (excelFile.Contains("Hoadondientu"))
                                    {
                                        DV1 += 1;
                                        clsKTHT.Type = 4;
                                        clsKTHT.STTType = "";
                                    }
                                    else
                                    {
                                        DV2 += 1;
                                        clsKTHT.Type = 5;
                                        clsKTHT.STTType = "";
                                    }
                                }

                                clsKTHT.STT = STT;
                                clsKTHT.GhiChu = "";
                                clsKTHT.KHMS = row.Cell("B").Value.ToString();
                                clsKTHT.KHHD = row.Cell("C").Value.ToString();
                                clsKTHT.SoHD = row.Cell("D").Value.ToString();
                                if (clsKTHT.SoHD == "16198")
                                {
                                    int ao = 10;
                                }
                                if (clsKTHT.SoHD == "1")
                                {
                                    int a = 10;
                                }
                                if (radDauvao.Checked)
                                {
                                    clsKTHT.MST = row.Cell("F").Value.ToString();
                                    clsKTHT.TongTienPhi = row.Cell("N").Value.ToString() != "" ? Math.Round(double.Parse(row.Cell("N").Value.ToString())) : 0;
                                }
                                else
                                    clsKTHT.MST = row.Cell("H").Value.ToString();
                                clsKTHT.TenKH = row.Cell("G").Value.ToString();
                                clsKTHT.NgayLap = DateTime.Parse(row.Cell("E").Value.ToString());

                                //Kiểm tra xem đã có tải hoá đơn chưa
                                DataRow getrow = tbimport.AsEnumerable().ToList()
        .Where(m => Helpers.RemoveLeadingZeros(m.Field<string>("SHDon")) == Helpers.RemoveLeadingZeros(clsKTHT.SoHD)
                     && m.Field<DateTime>("NLap").ToString("dd/MM/yy") == clsKTHT.NgayLap.ToString("dd/MM/yy"))
        .FirstOrDefault();

                                if (getrow != null)
                                {
                                    clsKTHT.Path = getrow.Field<string>("Path") != null ? getrow.Field<string>("Path").ToString() : "";
                                    clsKTHT.StatusImport = int.Parse(getrow["Status"].ToString());
                                    clsKTHT.NgayTai = DateTime.Parse(getrow["NgayTao"].ToString());
                                    clsKTHT.Khautruthue= getrow["Khautruthue"] != null ? int.Parse(getrow["Khautruthue"].ToString()) : 0;
                                }
                                //Lấy ra danh sách hoadontruoc

                                //Kiểm tra hoá đơn đã nhập chưa
                                var getHD = (from c in ChungTu.AsEnumerable()
                                             where c.Field<DateTime>("NgayCT").Date == clsKTHT.NgayLap.Date
                                                 && Helpers.RemoveLeadingZeros(c["SoHieu"].ToString()).TrimEnd('.') == Helpers.RemoveLeadingZeros(clsKTHT.SoHD).TrimEnd('.') 
                                             select new { ChungTu = c }).ToList();
                                getHD = getHD.Distinct().ToList();

                                var tt2 = (from h in HoaDon.AsEnumerable()
                                          join c in ChungTu.AsEnumerable()
                                          on h.Field<int>("MaSo") equals c.Field<int>("MaSo")
                                          where c.Field<DateTime>("NgayCT").Date == clsKTHT.NgayLap.Date
                                              //&& h["KyHieu"].ToString() == clsKTHT.KHHD
                                              && Helpers.RemoveLeadingZeros(h["SoHD"].ToString()).TrimEnd('.') == Helpers.RemoveLeadingZeros(clsKTHT.SoHD).TrimEnd('.')
                                          select new {ChungTu=c, Hoadon = h }).ToList();

                                string getmstF = row.Cell("F").Value.ToString();
                                var getcusf = KhachHang.AsEnumerable().Where(m => m.Field<string>("MST") == getmstF).FirstOrDefault();
                                //var checkhd = tt2.ToList().Where(m => m.Hoadon.Field<int>("MaKhachHang") == getcusf.Field<int>("MaSo")).ToList(); 
                                if (getHD != null && getHD.Count > 0)
                                {
                                    clsKTHT.NgayNhap = getHD.FirstOrDefault().ChungTu.Field<DateTime>("NgayCT");
                                    clsKTHT.Checked = true;
                                    //Lấy tiền truoc thuê
                                    if (radDauvao.Checked)
                                    {
                                        if (clsKTHT.SoHD == "7294")
                                        {
                                            int a = 10;
                                        }
                                        //Kiểm tra xem có phải 711 không
                                        //if (getHD.AsEnumerable().Any(m => m.ChungTu.Field<int>("MaTKCo") == 169))
                                        //{
                                        //    double tien = 0;
                                        //    tien = getHD.AsEnumerable().Where(m => m.ChungTu.Field<int>("MaTKNo") != 5108 && m.ChungTu.Field<int>("MaTKCo") == 0).Distinct().ToList().Sum(m => m.ChungTu.Field<double>("SoPS"));
                                        //    double tien711 = getHD.AsEnumerable().Where(m => m.ChungTu.Field<int>("MaTKNo") != 5108 && m.ChungTu.Field<int>("MaTKCo") == 169).Distinct().ToList().Sum(m => m.ChungTu.Field<double>("SoPS"));
                                        //    clsKTHT.TienTrcThueHD = tien - tien711;
                                        //}
                                        //else
                                        //{
                                        //    clsKTHT.TienTrcThueHD = getHD.AsEnumerable().Where(m => m.ChungTu.Field<int>("MaTKNo") != 5108 && m.ChungTu.Field<int>("MaTKCo")!=0).Distinct().ToList().Sum(m => m.ChungTu.Field<double>("SoPS"));
                                        //}

                                        //clsKTHT.TienThueHD = getHD.AsEnumerable().Where(m => m.ChungTu.Field<int>("MaTKNo") == 5108).Distinct().ToList().Sum(m => m.ChungTu.Field<double>("SoPS"));
                                        //clsKTHT.TongTienTTHD = clsKTHT.TienTrcThueHD + clsKTHT.TienThueHD;

                                        var getdata = kqvao.AsEnumerable().Where(m => Helpers.RemoveLeadingZeros(m.Field<string>("SoHD")).TrimEnd('.') == Helpers.RemoveLeadingZeros(clsKTHT.SoHD).TrimEnd('.') && m.Field<DateTime>("NgayPH") == clsKTHT.NgayLap && m.Field<string>("MST")== getmstF).ToList();
                                        if (getdata != null)
                                        {
                                            clsKTHT.TienTrcThueHD = getdata.Sum(m=>m.Field<double>("ThanhTien"));
                                            clsKTHT.TienThueHD = getdata.Sum(m => m.Field<double>("SoPS"));
                                            clsKTHT.TongTienTTHD = clsKTHT.TienTrcThueHD + clsKTHT.TienThueHD;
                                        }
                                    }
                                    else
                                    {
                                        var getdata = kqra.AsEnumerable().Where(m => Helpers.RemoveLeadingZeros(m.Field<string>("SoHD")).TrimEnd('.') == Helpers.RemoveLeadingZeros(clsKTHT.SoHD).TrimEnd('.') && m.Field<DateTime>("NgayPH") == clsKTHT.NgayLap).ToList();
                                        if (getdata != null)
                                        {
                                            clsKTHT.TienTrcThueHD = getdata.Sum(m => m.Field<double>("ThanhTien"));
                                            clsKTHT.TienThueHD = getdata.Sum(m => m.Field<double>("Thue"));
                                            clsKTHT.TongTienTTHD = clsKTHT.TienTrcThueHD + clsKTHT.TienThueHD;
                                        }

                                        //clsKTHT.TienTrcThueHD = getHD.AsEnumerable().Where(m => m.ChungTu.Field<int>("MaTKCo") != 14038 && m.ChungTu.Field<int>("MaTKCo")!=0).Distinct().ToList().Sum(m => m.ChungTu.Field<double>("SoPS"));
                                        //clsKTHT.TienThueHD = getHD.AsEnumerable().Where(m => m.ChungTu.Field<int>("MaTKCo") == 14038).Distinct().ToList().Sum(m => m.ChungTu.Field<double>("SoPS"));
                                        //clsKTHT.TongTienTTHD = clsKTHT.TienTrcThueHD + clsKTHT.TienThueHD;
                                    }
                                    //Nếu tiền âm thì bỏ check

                                    //Tìm khách hàng
                                    string loaict;
                                    if (radDauvao.Checked)
                                        loaict = "1,0,4";
                                    else
                                        loaict = "8";
                                        var tt = (from h in HoaDon.AsEnumerable()
                                                  join c in ChungTu.AsEnumerable()
                                                  on h.Field<int>("MaSo") equals c.Field<int>("MaSo")
                                                  where c.Field<DateTime>("NgayCT").Date == clsKTHT.NgayLap.Date
                                                      //&& h["KyHieu"].ToString() == clsKTHT.KHHD
                                                      && Helpers.RemoveLeadingZeros(h["SoHD"].ToString()).TrimEnd('.') == Helpers.RemoveLeadingZeros(clsKTHT.SoHD).TrimEnd('.')
                                                      && loaict.ToString().Contains(c["MaLoai"].ToString())
                                                  select new { Hoadon = h }).ToList();

                                    if (tt != null && tt.Count > 0)
                                    {
                                        if (radDauvao.Checked)
                                        {
                                           
                                            if (getcusf != null)
                                            {
                                                var checkany = tt.ToList().Where(m => m.Hoadon.Field<int>("MaKhachHang") == getcusf.Field<int>("MaSo")).FirstOrDefault();
                                                if(checkany != null)
                                                {
                                                    clsKTHT.MSTHD = getcusf.Field<string>("MST");
                                                    clsKTHT.TenKHHD = Helpers.ConvertVniToUnicode(getcusf.Field<string>("Ten"));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var fkh = KhachHang.AsEnumerable().Where(m => m.Field<int>("MaSo") == tt.FirstOrDefault().Hoadon.Field<int>("MaKhachHang")).FirstOrDefault();
                                            if (fkh != null)
                                            {
                                                clsKTHT.MSTHD = fkh.Field<string>("MST");
                                                clsKTHT.TenKHHD = Helpers.ConvertVniToUnicode(fkh.Field<string>("Ten"));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        int a = 10;
                                    }
                                }
                                else
                                    clsKTHT.Checked = false;

                                var cellK6 = worksheet.Cell("K6").Value.ToString();

                                if (cellK6 == "Căn cước công dân")
                                {
                                    if (!string.IsNullOrEmpty(row.Cell("L").Value.ToString()))
                                        clsKTHT.TienTrcThue = Math.Round(double.Parse(row.Cell("L").Value.ToString()));
                                    if (!string.IsNullOrEmpty(row.Cell("M").Value.ToString()))
                                        clsKTHT.TienThue = Math.Round(double.Parse(row.Cell("M").Value.ToString()));
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(row.Cell("K").Value.ToString()))
                                        clsKTHT.TienTrcThue = Math.Round(double.Parse(row.Cell("K").Value.ToString()));
                                    if (!string.IsNullOrEmpty(row.Cell("L").Value.ToString()))
                                        clsKTHT.TienThue = Math.Round(double.Parse(row.Cell("L").Value.ToString()));
                                }
                                clsKTHT.TongTienTT = double.Parse(row.Cell("O").Value.ToString());
                                if (clsKTHT.Checked)
                                    TongTienExcel += clsKTHT.TongTienTT;
                                //if (clsKTHT.TienTrcThue < 0)
                                //    clsKTHT.Checked = false;
                                //Điền ghi chú
                                if (clsKTHT.Checked)
                                {
                                    if (clsKTHT.TienTrcThue != clsKTHT.TienTrcThueHD)
                                    {
                                        if (clsKTHT.TienTrcThue != 0)
                                        {
                                            if (clsKTHT.TienTrcThue > clsKTHT.TienTrcThueHD)
                                                clsKTHT.GhiChu += $"Tiền trước thuế bị lệch {clsKTHT.TienTrcThue - clsKTHT.TienTrcThueHD} đ  Tiền gốc là  {clsKTHT.TienTrcThue} đ";
                                            else
                                                // Giả sử TienThueHD và TienThue là kiểu decimal hoặc double
                                                clsKTHT.GhiChu += $"Tiền trước thuế bị lệch: {(clsKTHT.TienTrcThueHD - clsKTHT.TienTrcThue):N0} Tiền gốc là: {clsKTHT.TienTrcThue:N0} đ";
                                        }
                                        //Trường hợp k thuế và lệch tổng tiền

                                        else
                                        {
                                            if(clsKTHT.TienTrcThueHD!= clsKTHT.TongTienTT)
                                            {
                                                clsKTHT.GhiChu += $"Tiền trước thuế bị lệch: {(clsKTHT.TongTienTT - clsKTHT.TienTrcThueHD):N0} Tiền gốc là: {clsKTHT.TongTienTT:N0} đ";
                                            }
                                        }
                                        
                                    }
                                    if ((clsKTHT.TienThue != 0 && clsKTHT.TienThueHD != 0) && clsKTHT.TienThue != clsKTHT.TienThueHD)
                                    {
                                        if (clsKTHT.TienThue > clsKTHT.TienThueHD)
                                            clsKTHT.GhiChu += $"Tiền thuế bị lệch {clsKTHT.TienThue - clsKTHT.TienThueHD} đ  Tiền gốc là  {clsKTHT.TienThue} đ";
                                        else
                                            // Giả sử TienThueHD và TienThue là kiểu decimal hoặc double
                                            clsKTHT.GhiChu += $"Tiền thuế bị lệch: {(clsKTHT.TienThueHD - clsKTHT.TienThue):N0} Tiền gốc là: {clsKTHT.TienThue:N0} đ";
                                    }
                                    if (!string.IsNullOrEmpty(clsKTHT.MST) && !string.IsNullOrEmpty(clsKTHT.MSTHD) && clsKTHT.MST != clsKTHT.MSTHD)
                                    {
                                        clsKTHT.GhiChu += $"MST thuế không đúng, MST gốc là {clsKTHT.MST}"; 
                                    }
                                }
                                else
                                {
                                    var checkds = dtGhichuht.AsEnumerable().Where(m => Helpers.RemoveLeadingZeros(m.Field<string>("SoHD")).TrimEnd('.') == Helpers.RemoveLeadingZeros(clsKTHT.SoHD).TrimEnd('.') && m.Field<DateTime>("NgayLap").Date == clsKTHT.NgayLap.Date).FirstOrDefault();
                                    if(checkds != null)
                                    {
                                        clsKTHT.GhiChu += checkds.Field<string>("Noidung");
                                    }
                                    else
                                    {
                                        //Kiểm tra lấy từ ghi chú tbimport
                                        var checkimport=tbimport.AsEnumerable().Where(m => Helpers.RemoveLeadingZeros(m.Field<string>("SHDon")).TrimEnd('.') == Helpers.RemoveLeadingZeros(clsKTHT.SoHD).TrimEnd('.') && m.Field<DateTime>("NLap").Date == clsKTHT.NgayLap.Date).FirstOrDefault();
                                        if (checkimport != null)
                                        {
                                            if (checkimport["Status"].ToString() !="3")
                                                clsKTHT.GhiChu = "Hoá đơn chưa được nhập";
                                            else
                                            {
                                                //if (getHD.Count > 0)
                                                //    clsKTHT.GhiChu = Helpers.ConvertVniToUnicode(checkimport.Field<string>("Noidung"));
                                                //else
                                                //    clsKTHT.GhiChu = "Hoá đơn chưa được nhập";
                                                clsKTHT.GhiChu = Helpers.ConvertVniToUnicode(checkimport.Field<string>("Noidung"));
                                            }
                                            //clsKTHT.GhiChu = checkimport.Field<string>("Noidung");
                                        }
                                        else
                                        {
                                            clsKTHT.GhiChu = "Hoá đơn chưa được nhập";
                                        }
                                       
                                    }
                                }
                                if (getHD.Count == 1)
                                {
                                    clsKTHT.GhiChu = "Hoá đơn bị thiếu thông tin hàng";
                                }

                                 STT += 1;
                                clsKTHTs.Add(clsKTHT);
                                dong += 1;
                                if (dong >= totalrow)
                                    dong = totalrow;
                            }
                           
                        }
                    }
                }

            }


            clsKTHTs = clsKTHTs.OrderBy(m => m.NgayLap).ToList();
            //Lập lại stt
            int stt = 1;
            foreach(var it in clsKTHTs)
            {
                it.STT = stt;
                stt += 1;
            }
            double sumtt = clsKTHTs.Where(m=>m.Checked).Sum(m => m.TongTienTTHD);
            clsKTHT clsKTHT2 = new clsKTHT();
            clsKTHT2.TenKHHD = "Tổng tiền";
            clsKTHT2.TienTrcThueHD = clsKTHTs.Where(m => m.Checked).Sum(m => m.TienTrcThueHD);
            clsKTHT2.TienThueHD = clsKTHTs.Where(m => m.Checked).Sum(m => m.TienThueHD);
            clsKTHT2.TongTienTTHD = sumtt;
            //clsKTHT2.TongTienTT = TongTienExcel;
           //clsKTHTs.Add(clsKTHT2);

            clsKTHT clsKTHT2ex = new clsKTHT();
            clsKTHT2ex.TenKHHD = "Tổng tiền excel";
            // clsKTHT2ex.TongTienTTHD = sumtt;
            clsKTHT2ex.TienTrcThueHD = clsKTHTs.Where(m => m.Checked).Sum(m => m.TienTrcThue);
            clsKTHT2ex.TienThueHD = clsKTHTs.Where(m => m.Checked).Sum(m => m.TienThue);
            clsKTHT2ex.TongTienTTHD = TongTienExcel;

            //Thêm hoá đơn thừa
            int typecheck = 0;
            if (radDauvao.Checked)
                typecheck = 1;
            else
                typecheck = 8;
                var gethoadontheothang = ChungTu.AsEnumerable().Where(m => m["MaLoai"].ToString() == typecheck.ToString()).OrderByDescending(m => m.Field<DateTime>("NgayCT")).Where(m => m.Field<DateTime>("NgayCT").Month >= tuthang && m.Field<DateTime>("NgayCT").Month <= denthang && !m.Field<string>("SoHieu").Contains("GV") ).GroupBy(m => m.Field<string>("SoHieu")).Select(g => g.First())  // Lấy bản ghi đầu tiên của mỗi nhóm
        .ToList(); ;
            var getdifferent = gethoadontheothang
     .Where(m => !clsKTHTs.Any(n => Helpers.RemoveLeadingZeros(n.SoHD).TrimEnd(',') == Helpers.RemoveLeadingZeros(m.Field<string>("SoHieu")).TrimEnd('.'))).ToList();

            foreach (DataRow g in getdifferent)
            {
                clsKTHT gplus = new clsKTHT();
                gplus.SoHD = g.Field<string>("SoHieu");
                gplus.NgayLap= g.Field<DateTime>("NgayCT");
                gplus.GhiChu = "Hoá đơn nhập dư";
                clsKTHTs.Add(gplus);
            }


            // clsKTHTs.Add(clsKTHT2ex);
            gridControl1.DataSource = clsKTHTs;
            gridControl1.RefreshDataSource();
            lblResult3.Text = $"{clsKTHT2.TienTrcThueHD.ToString("N0")}";
            lblResult2.Text = $"{clsKTHT2ex.TienTrcThueHD.ToString("N0")}";
            labelControl4.Text = $"{clsKTHT2.TienThueHD.ToString("N0")}";
            labelControl5.Text = $"{clsKTHT2ex.TienThueHD.ToString("N0")}";
            lbltshd1.Text = clsKTHTs.Where(m=>m.Checked).Count().ToString();
            lbltshd2.Text = clsKTHTs.Where(m=>m.STT!=0).Count().ToString();

            //lấy từ vb
            //Nếu là đầu vào
            //if (radDauvao.Checked)
            //{
            //    string sqlvao = "SELECT DISTINCTROW KyHieu,SoHD,ChungTu.NgayCT as NgayPH,MatHang,SoLuong,ThanhTien,KhachHang.Ten,KhachHang.MST,ChungTu.SoHieu,SoPS,KhachHang.DiaChi,TyLe,HTTT,MauSo,MaCT,HoaDon.MaSo,KCT FROM  (HoaDon INNER JOIN ChungTu ON HoaDon.MaSo=ChungTu.MaSo) LEFT JOIN KhachHang ON HoaDon.MaKhachHang=KhachHang.MaSo  WHERE Loai=-1 AND HD=1 AND  (ThangCT>=? AND ThangCT<=?)  AND (HDBL=0 OR KCT=0) AND (HoaDon.DC=0 OR HD=1) ORDER BY NgayPH,MaCT";
            //     parameters = new OleDbParameter[]
            //             {
            //new OleDbParameter("?",tuthang),
            //new OleDbParameter("?",denthang),
            //             };
            //    var kq = ExecuteQuery(sqlvao, parameters);
            //    double sumTientrcthuevb = kq.AsEnumerable().Sum(m => m.Field<double>("ThanhTien"));
            //    double sumTienThuevb = kq.AsEnumerable().Sum(m => m.Field<double>("SoPS"));
            //    lblResult1.Text = $"Tổng tiền trước thuế {sumTientrcthuevb.ToString("N0")} | Tổng tiền thuế {sumTienThuevb.ToString("N0")}";
            //    lblResult3.Text = $"Tổng tiền trước thuế {clsKTHT2.TienTrcThueHD.ToString("N0")} | Tổng tiền thuế {clsKTHT2.TienThueHD.ToString("N0")}";
            //    lblResult2.Text = $"Tổng tiền trước thuế {clsKTHT2ex.TienTrcThueHD.ToString("N0")} | Tổng tiền thuế {clsKTHT2ex.TienThueHD.ToString("N0")}";
            //}
            //else
            //{
            //    string sqlra = "SELECT DISTINCTROW HoaDon.KyHieu,SoHD,ChungTu.NgayCT as NgayPH,MatHang,SoLuong,ThanhTien,KhachHang.Ten,KhachHang.MST,ChungTu.SoHieu,IIF(TK_ID=3007,SoPS,-SoPS) AS Thue,ChungTu.MauSoHD as DiaChi,TyLe,HTTT,MauSo,MaCT,KCT FROM  ((HoaDon INNER JOIN ChungTu ON HoaDon.MaSo=ChungTu.MaSo) LEFT JOIN HethongTK ON ChungTu.MaTKCo=HethongTK.MaSo) LEFT JOIN KhachHang ON HoaDon.MaKhachHang=KhachHang.MaSo  WHERE HoaDon.Loai=1 AND  (ThangCT>=? AND ThangCT<=?)  AND (HoaDon.DC=0 OR HD=1) ORDER BY NgayPH";
            //     parameters = new OleDbParameter[]
            //             {
            //new OleDbParameter("?",tuthang),
            //new OleDbParameter("?",denthang),
            //             };
            //    var kq = ExecuteQuery(sqlra, parameters);
            //    double sumTientrcthuevb = kq.AsEnumerable().Sum(m => m.Field<double>("ThanhTien"));
            //    double sumTienThuevb = kq.AsEnumerable().Sum(m => m.Field<double>("Thue"));
            //    lblResult1.Text = $"Tổng tiền trước thuế {sumTientrcthuevb.ToString("N0")} | Tổng tiền thuế {sumTienThuevb.ToString("N0")}";
            //    lblResult3.Text = $"Tổng tiền trước thuế {clsKTHT2.TienTrcThueHD.ToString("N0")} | Tổng tiền thuế {clsKTHT2.TienThueHD.ToString("N0")}";
            //    lblResult2.Text = $"Tổng tiền trước thuế {clsKTHT2ex.TienTrcThueHD.ToString("N0")} | Tổng tiền thuế {clsKTHT2ex.TienThueHD.ToString("N0")}";
            //}
            progressPanel1.Visible = false;

        }
        private void LoadDanhsachExcel()
        {
            if (mstcongty == "8046549703")
                mstcongty = "048172000197";
            progressPanel1.Visible = true;
            progressPanel1.Caption = "Đang xử lý dữ liệu";
            Application.DoEvents();

            try
            {
                // Load ghi chú hệ thống
                dtGhichuht = ExecuteQuery("SELECT * FROM tbGhichuHT", null);
                DataTable tbchungtu = ExecuteQuery("SELECT * FROM ChungTu", null);
                TongTienExcel = 0;
                DV1 = DV2 = DV3 = 0;
                clsKTHTs = new List<clsKTHT>();

                string typeHD = radDauvao.Checked ? "HDVao" : "HDRa";
                int tuthang = int.Parse(cbbChonthang.Text.Replace("Tháng ", ""));
                int denthang = int.Parse(cbbDenthang.Text.Replace("Tháng ", ""));
                string TTHD = "";
                // Query dữ liệu từ DB để đối chiếu
                var parameters = new OleDbParameter[] { new OleDbParameter("?", tuthang), new OleDbParameter("?", denthang) };

                DataTable kqvao = radDauvao.Checked
                    ? ExecuteQuery(
                        "SELECT DISTINCTROW KyHieu,SoHD,ChungTu.NgayCT as NgayPH,MatHang,SoLuong,ThanhTien,KhachHang.Ten,KhachHang.MST,ChungTu.SoHieu,SoPS,KhachHang.DiaChi,TyLe,HTTT,MauSo,MaCT,HoaDon.MaSo,KCT " +
                        "FROM (HoaDon INNER JOIN ChungTu ON HoaDon.MaSo=ChungTu.MaSo) LEFT JOIN KhachHang ON HoaDon.MaKhachHang=KhachHang.MaSo " +
                        "WHERE Loai=-1 AND HD=1 AND (ThangCT>=? AND ThangCT<=?) AND (HDBL=0 OR KCT=0) AND (HoaDon.DC=0 OR HD=1) ORDER BY NgayPH,MaCT", parameters)
                    : null;

                DataTable kqra = !radDauvao.Checked
                    ? ExecuteQuery(
                        "SELECT DISTINCTROW HoaDon.MaSo,HoaDon.KyHieu,SoHD,ChungTu.NgayCT as NgayPH,MatHang,SoLuong,ThanhTien,KhachHang.Ten,KhachHang.MST,ChungTu.SoHieu,IIF(TK_ID=3007,SoPS,-SoPS) AS Thue,ChungTu.MauSoHD as DiaChi,TyLe,HTTT,MauSo,MaCT,KCT " +
                        "FROM ((HoaDon INNER JOIN ChungTu ON HoaDon.MaSo=ChungTu.MaSo) LEFT JOIN HethongTK ON ChungTu.MaTKCo=HethongTK.MaSo) LEFT JOIN KhachHang ON HoaDon.MaKhachHang=KhachHang.MaSo " +
                        "WHERE HoaDon.Loai=1 AND (ThangCT>=? AND ThangCT<=?) AND (HoaDon.DC=0 OR HD=1) ORDER BY NgayPH", parameters)
                    : null;

                int STT = 1;
                long totalRows = 0; // Tổng dòng cần xử lý (cho progress)
                long currentRow = 0;

                // Bước 1: Đếm tổng số dòng dữ liệu trong tất cả file Excel hợp lệ
                for (int i = tuthang; i <= denthang; i++)
                {
                    string CurrentYear = $"HD{cbbNam.EditValue}";
                    string directoryPath = Path.Combine(savedPath, CurrentYear, typeHD, i.ToString()).Trim();
                    if (!Directory.Exists(directoryPath)) continue;

                    var excelFiles = Directory.EnumerateFiles(directoryPath, "*.xlsx", SearchOption.AllDirectories)
                                             .Where(f => f.Contains(mstcongty) || f.Contains(CCCD))
                                             .ToList();
                    if(excelFiles.Count==0)
                    {
                        XtraMessageBox.Show($"Không tìm thấy file Excel trong thư mục {directoryPath}, vui lòng bấm cập nhật file Excel", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        
                    }
                    foreach (var file in excelFiles)
                    {
                        using (var workbook = new XLWorkbook(file))
                        {
                            var worksheet = workbook.Worksheet(1);
                            totalRows += worksheet.RowsUsed().Skip(3).Count();
                        }
                    }
                }

                // Bước 2: Xử lý từng file và từng dòng
                for (int i = tuthang; i <= denthang; i++)
                {
                    string CurrentYear = $"HD{cbbNam.EditValue}";
                    string directoryPath = Path.Combine(savedPath, CurrentYear, typeHD, i.ToString()).Trim();
                    if (!Directory.Exists(directoryPath)) continue;

                    var excelFiles = Directory.EnumerateFiles(directoryPath, "*.xlsx", SearchOption.AllDirectories)
                                             .Where(f => f.Contains(mstcongty) || f.Contains(CCCD))
                                             .ToList();

                    foreach (var excelFile in excelFiles)
                    {
                        using (var workbook = new XLWorkbook(excelFile))
                        {
                            var worksheet = workbook.Worksheet(1);

                            foreach (var row in worksheet.RowsUsed().Skip(3))
                            {
                                try
                                {
                                    currentRow++;
                                    if (currentRow % 50 == 0 || currentRow == totalRows) // Giảm tần suất DoEvents
                                    {
                                        progressPanel1.Caption = $"Đang đọc dòng thứ {currentRow:N0}/{totalRows:N0}";
                                        Application.DoEvents();
                                    }

                                    var clsKTHT = new clsKTHT { STT = STT++, GhiChu = "" };

                                    // Xác định loại hóa đơn và đếm DV
                                    if (typeHD == "HDVao")
                                    {
                                        if (excelFile.Contains("HDDienTuDaCapMa")) {
                                            DV1++; clsKTHT.Type = 1; 
                                            TTHD= GetCellValue(row.Cell("R")).ToString();
                                            clsKTHT.STTType = TTHD;
                                        }
                                        else if (excelFile.Contains("HDDienTuKhongMa")) {
                                            DV2++; clsKTHT.Type = 2; 
                                            TTHD = GetCellValue(row.Cell("R")).ToString();
                                            clsKTHT.STTType = TTHD;
                                        }
                                        else { 
                                            DV3++; clsKTHT.Type = 3;
                                            TTHD = GetCellValue(row.Cell("P")).ToString();
                                            clsKTHT.STTType = TTHD;
                                        }
                                    }
                                    else
                                    {
                                        if (excelFile.Contains("Hoadondientu")) { DV1++; clsKTHT.Type = 4;
                                            TTHD = GetCellValue(row.Cell("R")).ToString();
                                            clsKTHT.STTType = TTHD; }
                                        else { DV2++; clsKTHT.Type = 5;
                                            TTHD = GetCellValue(row.Cell("P")).ToString();
                                            clsKTHT.STTType = TTHD; }
                                    }

                                    // Đọc dữ liệu từ Excel
                                    clsKTHT.KHMS = GetCellValue(row.Cell("B"));
                                    clsKTHT.KHHD = GetCellValue(row.Cell("C"));
                                    clsKTHT.SoHD = GetCellValue(row.Cell("D")); 
                                    if (clsKTHT.SoHD == "1139" )
                                    {
                                        int a = 10;
                                    }
                                    clsKTHT.TenKH = GetCellValue(row.Cell("G"));
                                    clsKTHT.NgayLap = DateTime.Parse(GetCellValue(row.Cell("E")));

                                    clsKTHT.MST = radDauvao.Checked
                                        ? GetCellValue(row.Cell("F"))
                                        : GetCellValue(row.Cell("H"));




                                    if (radDauvao.Checked && double.TryParse(GetCellValue(row.Cell("N")), out double phi))
                                        clsKTHT.TongTienPhi = Math.Round(phi);

                                    // Đọc tiền từ Excel
                                    string k6 = worksheet.Cell("K6").Value.ToString();
                                    string colTrcThue = k6 == "Căn cước công dân" ? "L" : "K";
                                    string colThue = k6 == "Căn cước công dân" ? "M" : "L";

                                    clsKTHT.TienTrcThue = ParseDouble(row.Cell(colTrcThue));
                                    clsKTHT.TienThue = ParseDouble(row.Cell(colThue));
                                    clsKTHT.TongTienTT = ParseDouble(row.Cell("O"));
                                    if (clsKTHT.Checked) TongTienExcel += clsKTHT.TongTienTT;

                                    // Kiểm tra đã import chưa
                                    var imported = tbimport.AsEnumerable()
                                        .FirstOrDefault(m => Helpers.RemoveLeadingZeros(m.Field<string>("SHDon")) == Helpers.RemoveLeadingZeros(clsKTHT.SoHD)
                                                          && m.Field<DateTime>("NLap").Date == clsKTHT.NgayLap.Date);

                                    if (imported != null)
                                    {
                                        clsKTHT.Path = imported.Field<string>("Path") ?? "";
                                        clsKTHT.StatusImport = Convert.ToInt32(imported["Status"]);
                                        clsKTHT.NgayTai = DateTime.Parse(imported["NgayTao"].ToString());
                                        clsKTHT.Khautruthue = !string.IsNullOrEmpty(imported["Khautruthue"].ToString()) ? int.Parse(imported["Khautruthue"].ToString()) : 0;
                                    }

                                    // Tìm hóa đơn trong hệ thống
                                    List<DataRow> matchedHD = new List<DataRow>();
                                    if (radDauvao.Checked)
                                    {
                                        //matchedHD= kqvao.AsEnumerable()
                                        //.Where(m => Helpers.RemoveLeadingZeros(m.Field<string>("SoHD")).TrimEnd('.') == Helpers.RemoveLeadingZeros(clsKTHT.SoHD).TrimEnd('.')
                                        //         && m.Field<DateTime>("NgayPH").Date == clsKTHT.NgayLap.Date && m.Field<string>("KyHieu") == clsKTHT.KHHD)
                                        //.ToList();
                                        matchedHD = kqvao.AsEnumerable()
                                      .Where(m => Helpers.RemoveLeadingZeros(m.Field<string>("SoHD")).TrimEnd('.') == Helpers.RemoveLeadingZeros(clsKTHT.SoHD).TrimEnd('.')
                                              && m.Field<string>("KyHieu") == clsKTHT.KHHD)
                                      .ToList();
                                    }
                                    else
                                    {
                                        matchedHD = kqra.AsEnumerable()
                                        .Where(m => Helpers.RemoveLeadingZeros(m.Field<string>("SoHD")).TrimEnd('.') == Helpers.RemoveLeadingZeros(clsKTHT.SoHD).TrimEnd('.')
                                                && m.Field<string>("KyHieu") == clsKTHT.KHHD)
                                        .ToList();
                                    }
                                      
                                    if (radDauvao.Checked)
                                    {
                                        matchedHD= matchedHD.Where(m=> m.Field<string>("MST") == clsKTHT.MST).ToList(); 
                                    } 
                                        //Đếm số chứng từ
                                        int countct = 0;
                                    if (matchedHD.Count > 0)
                                    {
                                        countct = tbChungtu.AsEnumerable().Where(m => m.Field<string>("SoHieu") == RemoveLeadingZeros(matchedHD.FirstOrDefault().Field<string>("SoHD"))).Count();
                                    }

                                    if (matchedHD != null && matchedHD.Any())
                                    {
                                        clsKTHT.Checked = true;
                                        clsKTHT.NgayNhap = matchedHD.First().Field<DateTime>("NgayPH");

                                        if(clsKTHT.Khautruthue!=1)
                                        {
                                            clsKTHT.TienTrcThueHD = matchedHD.Sum(m => m.Field<double>("ThanhTien"));
                                            //Tính lại tiền trước thúe
                                            double getmaso= double.Parse(matchedHD.First()["MaSo"].ToString());
                                            string Mact = tbchungtu.AsEnumerable().Where(m => m["MaSo"].ToString() == getmaso.ToString()).FirstOrDefault()["MaCT"].ToString();
                                            var getlistct = tbchungtu.AsEnumerable().Where(m => m["MaCT"].ToString() == Mact.ToString() && m["MaTKCo"].ToString()!= "14038" && m["MaVattu"].ToString() != "0").ToList();
                                            double ttien = getlistct.Sum(m => double.Parse(m["SoPS"].ToString()));
                                            clsKTHT.TienTrcThueHD = ttien;
                                            clsKTHT.TienThueHD = matchedHD.Sum(m => radDauvao.Checked ? m.Field<double>("SoPS") : m.Field<double>("Thue"));
                                            clsKTHT.TongTienTTHD = clsKTHT.TienTrcThueHD + clsKTHT.TienThueHD;
                                        }
                                        else
                                        {
                                            var timHDKhautru = tbHoadon.AsEnumerable().Where(m => m["SoHD"].ToString() == clsKTHT.SoHD && m.Field<DateTime>("NgayPH").Date == clsKTHT.NgayLap.Date).ToList().Sum(m => double.Parse(m["ThanhTien"].ToString()));
                                            clsKTHT.TienTrcThueHD = timHDKhautru;
                                            clsKTHT.GhiChu = "Hoá đơn không khấu trừ thuế";
                                        }
                                            // Tìm MST và tên KH từ DB
                                            var khFromDB = KhachHang.AsEnumerable()
                                                .FirstOrDefault(k => k.Field<string>("MST") == clsKTHT.MST);

                                        if (khFromDB != null)
                                        {
                                            clsKTHT.MSTHD = khFromDB.Field<string>("MST");
                                            clsKTHT.TenKHHD = Helpers.ConvertVniToUnicode(khFromDB.Field<string>("Ten"));
                                        }
                                        //Trường hợp không có MST thì tìm Số hiệu khách hàng từ hoá đơn
                                        else
                                        {
                                            int getMaso = 0;
                                            if (radDauvao.Checked)
                                            {
                                                getMaso = tbChungtu.AsEnumerable().Where(m => m.Field<int>("MaTKNo") == 5108 && m.Field<int>("MaCT") == matchedHD.FirstOrDefault().Field<int>("MaCT")).FirstOrDefault().Field<int>("MaSo");
                                            }
                                            else
                                            {
                                                getMaso = tbChungtu.AsEnumerable().Where(m => m.Field<int>("MaTKCo") == 14038 && m.Field<int>("MaCT") == matchedHD.FirstOrDefault().Field<int>("MaCT")).FirstOrDefault().Field<int>("MaSo");
                                            }
                                               
                                            var findmkh = tbHoadon.AsEnumerable().Where(m => m.Field<int>("MaSo") == getMaso).FirstOrDefault().Field<int>("MaKhachHang");
                                            var kh = KhachHang.AsEnumerable().Where(m => m.Field<int>("MaSo") == findmkh).FirstOrDefault();
                                            if (kh != null)
                                            {
                                                clsKTHT.MSTHD = kh.Field<string>("SoHieu");
                                                clsKTHT.TenKHHD = Helpers.ConvertVniToUnicode(kh.Field<string>("Ten"));
                                            }
                                        }
                                        //Nhập sai ngày
                                        if (clsKTHT.NgayLap.Date != clsKTHT.NgayNhap.Date)
                                        {
                                            clsKTHT.GhiChu += $"Ngày nhập bị sai";
                                            Hoadonsai hoadonsai = new Hoadonsai
                                            {
                                                SoHD = clsKTHT.SoHD,
                                                KHHD = clsKTHT.KHHD,
                                                NgayLap = clsKTHT.NgayLap
                                            };  
                                            danhsachHdSaingay.Add(hoadonsai);
                                        }
                                    }
                                    else
                                    {
                                        clsKTHT.Checked = false;
                                    }

                                    // Ghi chú chênh lệch hoặc chưa nhập
                                    if (clsKTHT.Khautruthue != 1 || 1<2)
                                    {
                                        if (clsKTHT.Checked)
                                        {
                                            if (clsKTHT.TienTrcThue != clsKTHT.TienTrcThueHD && clsKTHT.TienTrcThue != 0)
                                            {
                                                double diff = clsKTHT.TienTrcThueHD - clsKTHT.TienTrcThue;
                                                clsKTHT.GhiChu += $"Tiền trước thuế bị lệch: {diff:N0} đ (gốc: {clsKTHT.TienTrcThue:N0} đ); ";
                                            }
                                            else if (clsKTHT.TienTrcThue == 0 && clsKTHT.TienTrcThueHD != clsKTHT.TongTienTT)
                                            {
                                                clsKTHT.GhiChu += $"Tiền trước thuế bị lệch: {(clsKTHT.TongTienTT - clsKTHT.TienTrcThueHD):N0} đ (gốc: {clsKTHT.TongTienTT:N0} đ); ";
                                            }

                                            if (clsKTHT.TienThue != 0 && clsKTHT.TienThueHD != 0 && clsKTHT.TienThue != clsKTHT.TienThueHD)
                                            {
                                                double diff = clsKTHT.TienThueHD - clsKTHT.TienThue;
                                                clsKTHT.GhiChu += $"Tiền thuế bị lệch: {diff:N0} đ (gốc: {clsKTHT.TienThue:N0} đ); ";
                                            }

                                            if (!string.IsNullOrEmpty(clsKTHT.MST) && !string.IsNullOrEmpty(clsKTHT.MSTHD) && clsKTHT.MST != clsKTHT.MSTHD)
                                            {
                                                clsKTHT.GhiChu += $"MST không đúng (gốc: {clsKTHT.MST}); ";
                                            }
                                            if (countct == 1)
                                            {
                                                clsKTHT.GhiChu += $"Hoá đơn thiếu thông tin hàng hoá ";
                                            }
                                            if (imported != null)
                                            {
                                                if (imported.Field<string>("Noidung").Contains("Ñieàu chænh"))
                                                {
                                                    clsKTHT.GhiChu = Helpers.ConvertVniToUnicode(imported.Field<string>("Noidung"));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var note = dtGhichuht.AsEnumerable()
                                                .FirstOrDefault(m => Helpers.RemoveLeadingZeros(m.Field<string>("SoHD")).TrimEnd('.') == Helpers.RemoveLeadingZeros(clsKTHT.SoHD).TrimEnd('.')
                                                                  && m.Field<DateTime>("NgayLap").Date == clsKTHT.NgayLap.Date);

                                            if (note != null)
                                                clsKTHT.GhiChu = note.Field<string>("Noidung");
                                            else if (imported != null)
                                                clsKTHT.GhiChu = imported["Status"].ToString() == "3"
                                                    ? Helpers.ConvertVniToUnicode(imported.Field<string>("Noidung"))
                                                    : "Hoá đơn chưa được nhập";
                                            else
                                                clsKTHT.GhiChu = "Hoá đơn chưa được nhập";
                                        }
                                    }
                                    else
                                    {

                                    }

                                        //                               var checktrung = tbChungtu.AsEnumerable().Where(m => m["SoHieu"].ToString() == clsKTHT.SoHD && DateTime.Parse(m["NgayCT"].ToString()).Date== clsKTHT.NgayLap.Date)
                                        //.GroupBy(m => new {
                                        //    SoHieu = m["SoHieu"],
                                        //    NgayCT = m["NgayCT"]
                                        //})
                                        //.Where(g => g.Select(r => r["MaCT"]).Distinct().Count() > 1)
                                        //.ToList();

                                        //                               if (checktrung.Any())
                                        //                               {
                                        //                                   clsKTHT.GhiChu = "Hoá đơn nhập trùng, "+ clsKTHT.GhiChu;
                                        //                               }
                                        clsKTHTs.Add(clsKTHT);
                                }
                                catch (Exception ex)
                                {
                                    XtraMessageBox.Show(ex.Message +"   ");
                                }
                                 

                            }
                        }
                    }
                }

                // Sắp xếp và đánh lại STT
                clsKTHTs = clsKTHTs.OrderBy(m => m.NgayLap).ThenBy(m => m.STT).ToList();
                for (int i = 0; i < clsKTHTs.Count; i++) clsKTHTs[i].STT = i + 1;

                // Thêm hóa đơn thừa (nhập dư trong hệ thống)
                int maLoai = radDauvao.Checked ? 1 : 8;
                var hdThua = ChungTu.AsEnumerable()
                    .Where(m => m["MaLoai"].ToString() == maLoai.ToString()
                             && m.Field<DateTime>("NgayCT").Month >= tuthang
                             && m.Field<DateTime>("NgayCT").Month <= denthang
                             && !m.Field<string>("SoHieu").Contains("GV"))
                    .GroupBy(m => m.Field<string>("SoHieu"))
                    .Select(g => g.OrderByDescending(r => r.Field<DateTime>("NgayCT")).First())
                    .Where(m => !clsKTHTs.Any(n => Helpers.RemoveLeadingZeros(n.SoHD).TrimEnd('.') == Helpers.RemoveLeadingZeros(m.Field<string>("SoHieu")).TrimEnd('.')))
                    .ToList();

                try
                {
                    foreach (var row in hdThua)
                    {
                        clsKTHTs.Add(new clsKTHT
                        {
                            SoHD = row.Field<string>("SoHieu"),
                            NgayLap = row.Field<DateTime>("NgayCT"),
                            GhiChu = "Hoá đơn nhập dư",
                            Checked = true,
                            //TienThueHD = double.Parse(tbChungtu.AsEnumerable().Where(m => m["SoHieu"].ToString() == row.Field<string>("SoHieu") && m["MaTKTCNo"].ToString() == "5108").FirstOrDefault()["SoPS"].ToString()),
                           // TienTrcThueHD = row.Field<double>("SoPS"),
                        });
                    }
                }
               catch(Exception ex)
                {
                    XtraMessageBox.Show(ex.Message);
                }

                // Cập nhật giao diện
                gridControl1.DataSource = clsKTHTs;
                gridControl1.RefreshDataSource();

                var checkedItems = clsKTHTs.Where(m => m.Checked);
                double sumTrcThueHD = checkedItems.Sum(m => m.TienTrcThueHD);
                double sumThueHD = checkedItems.Sum(m => m.TienThueHD);
                double sumTongHD = checkedItems.Sum(m => m.TongTienTTHD);

                lblResult3.Text = sumTrcThueHD.ToString("N0");
                lblResult2.Text = checkedItems.Sum(m => m.TienTrcThue).ToString("N0");
                labelControl4.Text = sumThueHD.ToString("N0");
                labelControl5.Text = checkedItems.Sum(m => m.TienThue).ToString("N0");
                lbltshd1.Text = checkedItems.Count().ToString();
                lbltshd2.Text = clsKTHTs.Count(m => m.STT != 0).ToString();
            }
            catch(Exception ex)
            {
               
            }
            finally
            {
                progressPanel1.Visible = false;
            }
        }

        // Hàm hỗ trợ nhỏ (không tính là tách hàm lớn)
        private string GetCellValue(IXLCell cell)
        {
            if (cell == null )
                return "";

            string value = cell.Value.ToString();
            if (value == null)
                return "";

            return value.Trim();
        }
        private double ParseDouble(IXLCell cell)
        {
            return double.TryParse(GetCellValue(cell).Replace(",", ""), out double val) ? Math.Round(val) : 0;
        }
        private void LoadControl()
        {
            string[] months = new string[]
     {
            "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5",
            "Tháng 6", "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10",
            "Tháng 11", "Tháng 12"
     };

            cbbChonthang.Properties.Items.AddRange(months);
            int currentMonth = DateTime.Now.Month;
            cbbChonthang.SelectedIndex = currentMonth - 1; // Tháng hiện tại (0-indexed)

            cbbDenthang.Properties.Items.AddRange(months);
            cbbDenthang.SelectedIndex = currentMonth - 1; // Tháng hiện tại (0-indexed)
        }
        DataTable dtGhichuht;
        public string pathThumuc = "";
        private void LoadData()
        {
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);

            // Xóa phần \bin\Debug để lấy đường dẫn gốc
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
            string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");
            pathThumuc = Path.Combine(rootDirectory);
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

            //string qr = "SELECT * FROM tbimport";
            //tbimport = ExecuteQuery(qr, null);

            //  qr = "SELECT * FROM ChungTu";
            //ChungTu = ExecuteQuery(qr, null);

            //  qr = "SELECT * FROM HoaDon";
            //HoaDon = ExecuteQuery(qr, null);

            //qr = "SELECT * FROM KhachHang";
            //KhachHang = ExecuteQuery(qr, null);

            //qr = "SELECT * FROM ChungTuLQ";
            //ChungTuLQ = ExecuteQuery(qr, null);

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
            System.Data.DataTable dataTable = new System.Data.DataTable();

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
        private string myTokken = "";
        bool needLogin = true;
        public string tokken { get; set; } = "";
        private async void Getttoken()
        {
            progressPanel1.Visible = true;
            progressPanel1.Caption = "Đang lấy thông tin token";
            Application.DoEvents();

            string querykh = @"SELECT * FROM tbRegister";
            var tbRegister = ExecuteQuery(querykh, new OleDbParameter("?", ""));

            string gettimeTokken = tbRegister.AsEnumerable().FirstOrDefault()?["TimeTokken"]?.ToString();

            // Bạn có thể bật lại đoạn check token còn hạn nếu muốn
            // if (!string.IsNullOrEmpty(gettimeTokken))
            // {
            //     var timpsan = DateTime.Now - DateTime.Parse(gettimeTokken);
            //     if (timpsan.TotalMinutes <= 10)
            //     {
            //         needLogin = false;
            //         myTokken = tbRegister.AsEnumerable().FirstOrDefault().Field<string>("tokken");
            //     }
            // }

            if (needLogin)
            {
                try
                {
                    var cookieContainer = new CookieContainer();
                    var handler = new HttpClientHandler()
                    {
                        UseCookies = true,
                        CookieContainer = cookieContainer,
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate ,
                        AllowAutoRedirect = true
                    };

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(30);

                        // Header giống trình duyệt
                        client.DefaultRequestHeaders.Clear();
                        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "vi-VN,vi;q=0.9,en-US;q=0.8,en;q=0.7");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://hoadondientu.gdt.gov.vn");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://hoadondientu.gdt.gov.vn/");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\"Google Chrome\";v=\"140\", \"Chromium\";v=\"140\", \"Not=A?Brand\";v=\"24\"");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
                        client.DefaultRequestHeaders.ExpectContinue = false;

                        // ================= STEP 1: GET CAPTCHA =================
                        progressPanel1.Caption = "Đang lấy captcha...";
                        Application.DoEvents();

                        string capUrl = "https://hoadondientu.gdt.gov.vn/api/captcha";

                        HttpResponseMessage resCap;
                        using (var capReq = new HttpRequestMessage(HttpMethod.Get, capUrl))
                        {
                            capReq.Headers.TryAddWithoutValidation("Request-Id", Guid.NewGuid().ToString());
                            capReq.Headers.TryAddWithoutValidation("End-Point", "/");
                            capReq.Headers.TryAddWithoutValidation("Action", "");
                            resCap = await client.SendAsync(capReq);
                        }

                        if (!resCap.IsSuccessStatusCode)
                        {
                            XtraMessageBox.Show("Không lấy được captcha");
                            progressPanel1.Visible = false;
                            return;
                        }

                        string capBody = await resCap.Content.ReadAsStringAsync();

                        // Kiểm tra WAF
                        if (capBody.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) )
                        {
                            XtraMessageBox.Show("Bị WAF chặn khi lấy captcha");
                            progressPanel1.Visible = false;
                            return;
                        }

                        MyJson capJson = JsonConvert.DeserializeObject<MyJson>(capBody);
                        if (capJson == null || string.IsNullOrEmpty(capJson.Key) || string.IsNullOrEmpty(capJson.Content))
                        {
                            XtraMessageBox.Show("Captcha response không hợp lệ");
                            progressPanel1.Visible = false;
                            return;
                        }

                        string svgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "captcha.svg");
                        File.WriteAllText(svgPath, capJson.Content);

                        // Lấy XSRF-TOKEN nếu có
                        var cookies = cookieContainer.GetCookies(new Uri("https://hoadondientu.gdt.gov.vn"));
                        string xsrfToken = cookies["XSRF-TOKEN"]?.Value;
                        if (!string.IsNullOrEmpty(xsrfToken))
                        {
                            client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
                            client.DefaultRequestHeaders.TryAddWithoutValidation("X-XSRF-TOKEN", xsrfToken);
                        }

                        // ================= STEP 2: SOLVE CAPTCHA =================
                        progressPanel1.Caption = "Đang giải captcha...";
                        Application.DoEvents();

                        SvgCaptchaSolver solver = new SvgCaptchaSolver();
                        string cvalue = solver.SolveCaptcha(svgPath)?.Trim() ?? "";

                        if (string.IsNullOrEmpty(cvalue) || cvalue.Length < 4)
                        {
                            XtraMessageBox.Show("Không giải được captcha");
                            progressPanel1.Visible = false;
                            return;
                        }

                        // ================= STEP 3: LOGIN =================
                        progressPanel1.Caption = "Đang đăng nhập...";
                        Application.DoEvents();

                        string loginUrl = "https://hoadondientu.gdt.gov.vn/api/security-taxpayer/authenticate";

                        var payload = new
                        {
                            username = tbRegister.Rows[0]["Username"].ToString(),
                            password = tbRegister.Rows[0]["Password"].ToString(),
                            cvalue = cvalue,
                            ckey = capJson.Key
                        };

                        var content = new StringContent(
                            JsonConvert.SerializeObject(payload),
                            Encoding.UTF8,
                            "application/json"
                        );

                        HttpResponseMessage loginRes;
                        using (var loginReq = new HttpRequestMessage(HttpMethod.Post, loginUrl))
                        {
                            loginReq.Headers.TryAddWithoutValidation("Request-Id", Guid.NewGuid().ToString());
                            loginReq.Headers.TryAddWithoutValidation("End-Point", "/");
                            loginReq.Headers.TryAddWithoutValidation("Action", "");
                            loginReq.Content = content;
                            loginRes = await client.SendAsync(loginReq);
                        }

                        string loginBody = await loginRes.Content.ReadAsStringAsync();

                        // Kiểm tra WAF
                        if (loginBody.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) )
                        {
                            XtraMessageBox.Show("Bị WAF chặn khi đăng nhập");
                            progressPanel1.Visible = false;
                            return;
                        }

                        if (loginRes.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            XtraMessageBox.Show("Đăng nhập thất bại (401): " + loginBody);
                            progressPanel1.Visible = false;
                            return;
                        }

                        if (!loginRes.IsSuccessStatusCode)
                        {
                            XtraMessageBox.Show($"Đăng nhập lỗi HTTP {(int)loginRes.StatusCode}: {loginBody.Substring(0, Math.Min(300, loginBody.Length))}");
                            progressPanel1.Visible = false;
                            return;
                        }

                        var tokenData = JsonConvert.DeserializeObject<TokenResponse>(loginBody);
                        if (tokenData == null || string.IsNullOrEmpty(tokenData.token))
                        {
                            // Fallback tìm JWT
                            var match = System.Text.RegularExpressions.Regex.Match(loginBody, @"""(eyJ[A-Za-z0-9_\-]+\.[A-Za-z0-9_\-]+\.[A-Za-z0-9_\-]+)""");
                            if (match.Success)
                                this.tokken = match.Groups[1].Value;
                            else
                            {
                                XtraMessageBox.Show("Không tìm thấy token trong response");
                                progressPanel1.Visible = false;
                                return;
                            }
                        }
                        else
                        {
                            this.tokken = tokenData.token;
                        }

                        myTokken = this.tokken;

                        // ================= STEP 4: PROFILE =================
                        try
                        {
                            // Sửa URL: thêm /api
                            using (var req = new HttpRequestMessage(HttpMethod.Get, "https://hoadondientu.gdt.gov.vn/api/security-taxpayer/profile"))
                            {
                                req.Headers.TryAddWithoutValidation("Request-Id", Guid.NewGuid().ToString());
                                req.Headers.TryAddWithoutValidation("End-Point", "/");
                                req.Headers.TryAddWithoutValidation("Action", "");
                                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.tokken);

                                var profRes = await client.SendAsync(req);
                                if (profRes.IsSuccessStatusCode)
                                {
                                    string profBody = await profRes.Content.ReadAsStringAsync();
                                    var prof = JsonConvert.DeserializeObject<ProfileResponse>(profBody);

                                    if (prof != null && !string.IsNullOrEmpty(prof.password_expire))
                                    {
                                        DateTime expireDate = DateTime.Parse(prof.password_expire);
                                        TimeSpan remain = expireDate - DateTime.Now;

                                        if (remain.TotalDays <= 0)
                                        {
                                            XtraMessageBox.Show(
                                                $"Mật khẩu đã hết hạn ngày {expireDate:dd/MM/yyyy}.",
                                                "Hết hạn!",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Warning);
                                            progressPanel1.Visible = false;
                                            return;
                                        }
                                        else if (remain.TotalDays <= 3)
                                        {
                                            XtraMessageBox.Show(
                                                $"⚠ Mật khẩu sẽ hết hạn sau {remain.Days} ngày!\nNgày: {expireDate:dd/MM/yyyy}",
                                                "Cảnh báo",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Warning);
                                        }
                                        else if (remain.TotalDays <= 7)
                                        {
                                            XtraMessageBox.Show(
                                                $"Mật khẩu sắp hết hạn {expireDate:dd/MM/yyyy} (còn {remain.Days} ngày)",
                                                "Cảnh báo",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Warning);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            XtraMessageBox.Show("Không kiểm tra được ngày hết hạn: " + ex.Message);
                        }

                        // ================= SAVE TOKEN TIME =================
                        ExecuteQueryResult(
                            "UPDATE tbRegister SET TimeTokken=?",
                            new OleDbParameter[]
                            {
                        new OleDbParameter("?", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                            }
                        );

                        // Gọi tải excel sau khi có token
                        Taiexcelvao();

                        Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("Lỗi đăng nhập hệ thống thuế: " + ex.Message);
                }
            }

            progressPanel1.Visible = false;
        }
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            Getttoken();
           

        }

        private void btnExportExcelVao_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Excel Files|*.xlsx";
            saveDialog.Title = "Export to Excel";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                gridView1.ExportToXlsx(saveDialog.FileName);
                XtraMessageBox.Show("Export thành công!");
            }
        }

        private async void gridControl1_DoubleClick(object sender, EventArgs e)
        {
            DevExpress.XtraGrid.Views.Grid.GridView gridView = gridControl1.MainView as DevExpress.XtraGrid.Views.Grid.GridView;
            var hitInfo = gridView.CalcHitInfo(gridView.GridControl.PointToClient(MousePosition));
            var selectedRow = gridView.GetRow(gridView.FocusedRowHandle) as clsKTHT;

            if (!hitInfo.InRowCell || selectedRow == null)
                return;

            System.Windows.Forms.WebBrowser webBrowser1 = new System.Windows.Forms.WebBrowser
            {
                Dock = DockStyle.Fill
            };

            // Lấy MST
            string mst = "";
            if (radDauvao.Checked)
            {
                mst = selectedRow.MST;
            }
            else
            {
                string qr = "SELECT * FROM tbRegister";
                var kq2 = ExecuteQuery(qr, null);
                mst = kq2.Rows[0]["Username"].ToString();
            }

            if (mst == "8046549703")
                mst = "048172000197";

            string pathravao = radDauvao.Checked ? "HDVao" : "HDRa";
            string fn = $"{selectedRow.NgayLap:yyyyMMdd}_{mst}_{selectedRow.SoHD}_{selectedRow.KHHD}.html";
            int tuthang = int.Parse(cbbChonthang.Text.Replace("Tháng ", ""));

            string query = "SELECT * FROM License";
            var kq = ExecuteQuery(query, null);
            string yearPath = $"HD{kq.Rows[0]["NamTC"]}";
            string ph = Path.Combine(savedPath, yearPath, pathravao, tuthang.ToString(), fn);
            string hiddenValue = ph;

            // Kiểm tra file có tồn tại không
            if (!File.Exists(hiddenValue))
            {
                Match match = Regex.Match(hiddenValue, @"\\Hoadon\\.*$", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    hiddenValue = pathThumuc + match.Value;
                }
            }

            // Nếu đã có file HTML thì mở luôn
            if (!string.IsNullOrEmpty(hiddenValue) && File.Exists(hiddenValue))
            {
                frmWebbrowser frmCongTrinh = new frmWebbrowser();
                frmCongTrinh.Text = Path.GetFileNameWithoutExtension(hiddenValue);
                string filePath = hiddenValue.Replace(".xml", ".html");
                frmCongTrinh.filep = filePath;
                frmCongTrinh.Show();
                frmCongTrinh.BringToFront();
                frmCongTrinh.Activate();
                frmCongTrinh.Controls.Add(webBrowser1);
                webBrowser1.Navigate("file:///" + filePath.Replace("\\", "/"));
                return;
            }

            // Chưa có file → tải mới
            Getttoken(); // đảm bảo có token

            int type = 0;
            if (selectedRow.Type == 1) type = 4;
            else if (selectedRow.Type == 2) type = 6;
            else if (selectedRow.Type == 3) type = 5;
            else if (selectedRow.Type == 4) type = 4;
            else if (selectedRow.Type == 5) type = 5;

            string url = GetInvoiceUrl(type, mst, selectedRow.KHHD, selectedRow.SoHD, selectedRow.KHMS);
            string filename = $"{selectedRow.NgayLap:yyyyMMdd}_{mst}_{selectedRow.SoHD}_{selectedRow.KHHD}.zip";
            string path = Path.Combine(savedPath, yearPath, pathravao, tuthang.ToString(), filename);

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(60);

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.myTokken);
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "vi-VN,vi;q=0.9");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://hoadondientu.gdt.gov.vn");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://hoadondientu.gdt.gov.vn/tra-cuu/tra-cuu-hoa-don");
                client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\"Google Chrome\";v=\"140\", \"Chromium\";v=\"140\", \"Not=A?Brand\";v=\"24\"");
                client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");

                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    Application.UseWaitCursor = true;

                    string action = (type == 5 || type == 10)
                        ? "Xuất xml (hóa đơn máy tính tiền mua vào)"
                        : "Xuất xml (hóa đơn mua vào)";

                    using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        request.Headers.TryAddWithoutValidation("Request-Id", Guid.NewGuid().ToString());
                        request.Headers.TryAddWithoutValidation("End-Point", "/tra-cuu/tra-cuu-hoa-don");
                        request.Headers.TryAddWithoutValidation("Action", Uri.EscapeDataString(action));

                        HttpResponseMessage response = await client.SendAsync(request);

                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            XtraMessageBox.Show("Có lỗi, vui lòng thử lại.");
                            return;
                        }

                        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            XtraMessageBox.Show("Bị chặn 403 khi tải XML hóa đơn.");
                            return;
                        }

                        response.EnsureSuccessStatusCode();

                        byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

                        if (fileBytes == null || fileBytes.Length < 100)
                        {
                            XtraMessageBox.Show("File tải về rỗng hoặc quá nhỏ.");
                            return;
                        }

                        // Lưu file ZIP
                        File.WriteAllBytes(path, fileBytes);
                        Console.WriteLine($"File ZIP đã được lưu tại: {path}");

                        // Giải nén
                        string rootPath = Path.GetDirectoryName(path);
                        string getnamefile = Path.GetFileNameWithoutExtension(path);
                        string directoryPath = Path.Combine(rootPath, "Giainen_" + getnamefile);

                        if (Directory.Exists(directoryPath))
                            Directory.Delete(directoryPath, true);

                        ZipFile.ExtractToDirectory(path, directoryPath);

                        var files = Directory.GetFiles(directoryPath, "invoice.html", SearchOption.AllDirectories);
                        if (files.Length == 0)
                        {
                            // Thử tìm file .html bất kỳ
                            files = Directory.GetFiles(directoryPath, "*.html", SearchOption.AllDirectories);
                        }

                        if (files.Length == 0)
                        {
                            XtraMessageBox.Show("Không tìm thấy file invoice.html trong ZIP.");
                            return;
                        }

                        string targetFilePath = Path.Combine(rootPath, getnamefile + ".html");
                        if (File.Exists(targetFilePath))
                            File.Delete(targetFilePath);

                        File.Move(files.First(), targetFilePath);

                        // Xóa file tạm
                        try
                        {
                            File.Delete(path);
                            Directory.Delete(directoryPath, true);
                        }
                        catch { }

                        // Mở form xem
                        frmWebbrowser frmCongTrinh = new frmWebbrowser();
                        frmCongTrinh.filep = targetFilePath;
                        frmCongTrinh.Show();
                        frmCongTrinh.BringToFront();
                        frmCongTrinh.Activate();
                        frmCongTrinh.Controls.Add(webBrowser1);

                        selectedRow.Path = targetFilePath;
                        webBrowser1.Navigate("file:///" + targetFilePath.Replace("\\", "/"));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi: {ex.Message}");
                    XtraMessageBox.Show("Không thể tải file xuống, vui lòng thử lại.\n" + ex.Message);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    Application.UseWaitCursor = false;
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        public string GetInvoiceUrl(int invoiceType, string nbmst, string khhdon, string shdon, string Khmshdon)
        {
            string url;

            if (invoiceType == 4 || invoiceType == 6 || invoiceType == 8)
            {
                // Hóa đơn thường
                url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-xml?nbmst={Uri.EscapeDataString(nbmst)}&khhdon={Uri.EscapeDataString(khhdon)}&shdon={Uri.EscapeDataString(shdon)}&khmshdon={Uri.EscapeDataString(Khmshdon ?? "")}";
            }
            else if (invoiceType == 5 || invoiceType == 10)
            {
                // Máy tính tiền
                url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-xml?nbmst={Uri.EscapeDataString(nbmst)}&khhdon={Uri.EscapeDataString(khhdon)}&shdon={Uri.EscapeDataString(shdon)}&khmshdon={Uri.EscapeDataString(Khmshdon ?? "")}";
            }
            else
            {
                throw new ArgumentException("Loại hóa đơn không hợp lệ: " + invoiceType);
            }

            return url;
        }
        private void simpleButton6_Click(object sender, EventArgs e)
        {
            // simpleButton1.PerformClick();
            gridControl1.DataSource = null;
            gridControl1.RefreshDataSource();
            //LoadDanhsachExcel();
            Dohoadon();

        }
        int slcoquanthue = 0;
        private void Dohoadon()
        {
            slcoquanthue = 0;
            progressPanel1.Visible = true;
            progressPanel1.Caption = "Đang xử lý dữ liệu";
            Application.DoEvents();
            clsKTHTs = new List<clsKTHT>();
            int tuthang = int.Parse(cbbChonthang.Text.Replace("Tháng ", ""));
            int denthang = int.Parse(cbbDenthang.Text.Replace("Tháng ", ""));
            string typeHD = "";
            typeHD = radDauvao.Checked ? "HDVao" : "HDRa";
            for (int i = tuthang; i <= denthang; i++)
            {
                string CurrentYear = $"HD{cbbNam.EditValue}";
                string directoryPath = Path.Combine(savedPath, CurrentYear, typeHD, i.ToString()).Trim();
                if (!Directory.Exists(directoryPath)) continue;

                var excelFiles = Directory.EnumerateFiles(directoryPath, "*.xlsx", SearchOption.AllDirectories)
                                         .Where(f => f.Contains(mstcongty) || f.Contains(CCCD))
                                         .ToList();

                foreach (var excelFile in excelFiles)
                {
                    using (var workbook = new XLWorkbook(excelFile))
                    {
                        var worksheet = workbook.Worksheet(1);

                        foreach (var row in worksheet.RowsUsed().Skip(3))
                        {
                            clsKTHT clsKTHT = new clsKTHT();
                            try
                            {
                                slcoquanthue += 1;
                                clsKTHT.KHMS = GetCellValue(row.Cell("B"));
                                clsKTHT.KHHD = GetCellValue(row.Cell("C"));
                                clsKTHT.SoHD = GetCellValue(row.Cell("D"));
                                 
                                if (clsKTHT.SoHD == "218288")
                                {
                                    int a = 10;
                                }
                                clsKTHT.TenKH = GetCellValue(row.Cell("G"));
                                clsKTHT.NgayLap = DateTime.Parse(GetCellValue(row.Cell("E")));

                                clsKTHT.MST = radDauvao.Checked
                                    ? GetCellValue(row.Cell("F"))
                                    : GetCellValue(row.Cell("H"));

                                if (radDauvao.Checked)
                                {
                                    if (excelFile.Contains("MayTinhTien"))
                                    {
                                        if (!string.IsNullOrEmpty(row.Cell("L").Value.ToString()))
                                            clsKTHT.TienTrcThue = Math.Round(double.Parse(row.Cell("L").Value.ToString()));
                                        if (!string.IsNullOrEmpty(row.Cell("M").Value.ToString()))
                                            clsKTHT.TienThue = Math.Round(double.Parse(row.Cell("M").Value.ToString()));
                                        if (!string.IsNullOrEmpty(row.Cell("O").Value.ToString()))
                                            clsKTHT.TongTienTT = Math.Round(double.Parse(row.Cell("O").Value.ToString()));

                                        clsKTHT.STTType = GetCellValue(row.Cell("P")).ToString();
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(row.Cell("K").Value.ToString()))
                                            clsKTHT.TienTrcThue = Math.Round(double.Parse(row.Cell("K").Value.ToString()));
                                        if (!string.IsNullOrEmpty(row.Cell("L").Value.ToString()))
                                            clsKTHT.TienThue = Math.Round(double.Parse(row.Cell("L").Value.ToString()));
                                        if (!string.IsNullOrEmpty(row.Cell("O").Value.ToString()))
                                            clsKTHT.TongTienTT = Math.Round(double.Parse(row.Cell("O").Value.ToString()));
                                        clsKTHT.STTType = GetCellValue(row.Cell("R")).ToString();
                                    }

                                    //Trường hợp ko có thuế
                                    if (clsKTHT.TienTrcThue == 0 && clsKTHT.TienThue == 0)
                                    {
                                        clsKTHT.TienTrcThue = clsKTHT.TongTienTT;
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(row.Cell("L").Value.ToString()))
                                        clsKTHT.TienTrcThue = Math.Round(double.Parse(row.Cell("L").Value.ToString()));
                                    if (!string.IsNullOrEmpty(row.Cell("M").Value.ToString()))
                                        clsKTHT.TienThue = Math.Round(double.Parse(row.Cell("M").Value.ToString()));
                                    if (!string.IsNullOrEmpty(row.Cell("O").Value.ToString()))
                                        clsKTHT.TongTienTT = Math.Round(double.Parse(row.Cell("O").Value.ToString()));
                                    clsKTHT.STTType = GetCellValue(row.Cell("P")).ToString();
                                    //Trường hợp ko có thuế
                                    if (clsKTHT.TienTrcThue == 0 && clsKTHT.TienThue == 0)
                                    {
                                        clsKTHT.TienTrcThue = clsKTHT.TongTienTT;
                                    }
                                }
                                //Tìm thông tin trên list chung từ tổng hợp
                                ChungTuHD findhoadon = new ChungTuHD();
                                if (radDauvao.Checked)
                                    //findhoadon = lstChungTuHD.FirstOrDefault(m => m.NgayCT.Date == clsKTHT.NgayLap && m.SoHieu == clsKTHT.SoHD && m.KHHD == clsKTHT.KHHD && m.MST == clsKTHT.MST);
                                    findhoadon = lstChungTuHD.FirstOrDefault(m => m.NgayCT.Date == clsKTHT.NgayLap && m.SoHieu == clsKTHT.SoHD && m.KHHD == clsKTHT.KHHD );
                                else
                                    findhoadon = lstChungTuHD.FirstOrDefault(m => m.NgayCT.Date == clsKTHT.NgayLap && m.SoHieu == clsKTHT.SoHD && m.KHHD == clsKTHT.KHHD);

                                if (findhoadon != null)
                                {
                                    clsKTHT.NgayNhap = findhoadon.NgayImport;
                                    clsKTHT.TenKHHD = Helpers.ConvertVniToUnicode(findhoadon.TenKH);
                                    clsKTHT.MSTHD = findhoadon.MST;
                                    clsKTHT.TienTrcThueHD = findhoadon.TienTrcThue;
                                    clsKTHT.TienThueHD = findhoadon.TienThue;
                                    clsKTHT.TongTienTTHD = findhoadon.TongTien;
                                    Thietlapghichu(clsKTHT);
                                }
                                else
                                {
                                    clsKTHT.GhiChu = "Hoá đơn chưa được nhập";
                                }
                               
                                clsKTHTs.Add(clsKTHT);

                            }
                            catch (Exception ex)
                            {
                                XtraMessageBox.Show(ex.Message + "  " + clsKTHT.SoHD);
                            }

                        }
                    }
                }

                //Rán datasource
                gridControl1.DataSource = clsKTHTs;
            }
            progressPanel1.Caption = "";
            progressPanel1.Visible = false;

            //Gán thông số
            lbltshd2.Text = slcoquanthue.ToString();
        }
        private void Thietlapghichu(clsKTHT clsKTHT)
        {
            if (string.IsNullOrEmpty(clsKTHT.MST))
                clsKTHT.MST = "00";
            if (clsKTHT.MST!= clsKTHT.MSTHD)
            {
                clsKTHT.GhiChu += "MST không khớp , ";
            }
            if (clsKTHT.TienTrcThue != clsKTHT.TienTrcThueHD)
            {
                clsKTHT.GhiChu += "Tiền trước thuế bị lệch , ";
            }
            if (clsKTHT.TienThue != clsKTHT.TienThueHD)
            {
                clsKTHT.GhiChu += "Tiền thuế bị lệch , ";
            }
            if (clsKTHT.TongTienTT != clsKTHT.TongTienTTHD)
            {
                clsKTHT.GhiChu += "Tổng tiền bị lệch , ";
            }
        }
        private void simpleButton2_Click(object sender, EventArgs e)
        {
            cbbChonthang.EditValue = "Tháng 1";
            cbbDenthang.EditValue = "Tháng 3";
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            cbbChonthang.EditValue = "Tháng 4";
            cbbDenthang.EditValue = "Tháng 6";
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            cbbChonthang.EditValue = "Tháng 7";
            cbbDenthang.EditValue = "Tháng 9";
        }

        private void simpleButton5_Click(object sender, EventArgs e)
        {
            cbbChonthang.EditValue = "Tháng 10";
            cbbDenthang.EditValue = "Tháng 12";
        }

        private void gridView1_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {

           string qr = "SELECT * FROM tbGhichuHT";
            dtGhichuht = ExecuteQuery(qr, null);

            var selectedRow = gridView1.GetRow(e.RowHandle) as clsKTHT;
            //Tìm trong danh sách đã có chưa
            var checkds = dtGhichuht.AsEnumerable().Where(m => m.Field<string>("SoHD") == selectedRow.SoHD && m.Field<DateTime>("NgayLap").Date == selectedRow.NgayLap.Date).FirstOrDefault();
                if (checkds != null)
            {
                var query = @"UPDATE tbGhichuHT  set Noidung=? where ID=?";
                var parameters = new OleDbParameter[]
                 { 
               new OleDbParameter("?",selectedRow.GhiChu),
                 new OleDbParameter("?",checkds.Field<int>("ID"))
                 };
                var rowsAffected = ExecuteQueryResult(query, parameters);
            }
            else
            {
                var query = @"INSERT INTO tbGhichuHT (SoHD,NgayLap,Noidung) VALUES (?, ?,?)";
                var parameters = new OleDbParameter[]
                 {
            new OleDbParameter("?",selectedRow.SoHD),
             new OleDbParameter("?",selectedRow.NgayLap),
               new OleDbParameter("?",selectedRow.GhiChu),
                 };
              var  rowsAffected = ExecuteQueryResult(query, parameters);
            }
        }

        private void KTHT_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (frmMain != null)
                if (frmMain.typetxt == "2")
                    this.frmMain.Close();
        }
        public class ThongKe
        {
            public string Name { get; set; }
            public double TienTrcThue { get; set; }
            public double TienThue { get; set; }    
            public double TongTien { get; set; }
        }
        string mstcongty = "";
        string CCCD=""; 
        string savedPath = "";
        string user = "";
        string password = "";
        DataTable tbChungtu;
        DataTable tbHoadon;

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            try
            {
                string currentYear = $"HD{cbbNam.EditValue}";
                string currentpath = Path.Combine(savedPath, currentYear,
                    radDauvao.Checked ? "HDVao" : "HDRa",
                    cbbChonthang.EditValue.ToString().Replace("Tháng","").Trim());

                // Mở thư mục
                System.Diagnostics.Process.Start("explorer.exe", currentpath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở thư mục: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // Trong form load hoặc phương thức khởi tạo
        private void LoadComboBoxYear()
        {
            try
            {
                // Xóa items cũ nếu có
                cbbNam.Properties.Items.Clear();

                // Lấy năm hiện tại
                int currentYear = DateTime.Now.Year;

                // Thêm các năm từ 2000 đến năm hiện tại
                for (int year = 2000; year <= currentYear; year++)
                {
                    cbbNam.Properties.Items.Add(year);
                }

                // Chọn năm hiện tại mặc định
                cbbNam.SelectedItem = currentYear;

                // Hoặc có thể format hiển thị nếu cần
                // cbbNam.Properties.Items.Add($"Năm {year}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách năm: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Gọi phương thức này trong Form_Load
    
        private void ThietLapControl()
        {
            //Disable gõ text
            cbbChonthang.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            cbbDenthang.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            cbbNam.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            cbbDenthang.Properties.AccessibleDefaultActionDescription = "Chọn tháng đến";
            //Thiết lập kích thước  
            //(int)(this.ClientSize.Width * 0.28);
            cbbChonthang.Width = (int)(this.ClientSize.Width * 0.06);
            cbbDenthang.Width = (int)(this.ClientSize.Width * 0.06);
            labelControl7.Location=new Point(cbbChonthang.Location.X + cbbChonthang.Width + 5, labelControl7.Location.Y);
            cbbDenthang.Location = new Point(labelControl7.Location.X + labelControl7.Width + 5, cbbDenthang.Location.Y);
            labelControl8.Location = new Point(cbbDenthang.Location.X + cbbDenthang.Width + 5, labelControl8.Location.Y);   
            cbbNam.Location = new Point(labelControl8.Location.X + labelControl8.Width + 5, cbbNam.Location.Y);
            simpleButton6.Location = new Point(cbbNam.Location.X + cbbNam.Width + 5, simpleButton6.Location.Y);
        }

        private void cbbDenthang_SelectedIndexChanged(object sender, EventArgs e)
        {
            int tuthang = int.Parse(cbbChonthang.Text.Replace("Tháng ", ""));
            int denthang = int.Parse(cbbDenthang.Text.Replace("Tháng ", ""));
            if (denthang < tuthang)
            {
                cbbChonthang.EditValue = cbbDenthang.EditValue;
            }
            // cbbDenthang.EditValue = cbbChonthang.EditValue;
        }

        private void KTHT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.G)
            {
                var dd = danhsachHdSaingay;
                //Tìm MaSo từ  Hoadon trước
                foreach(var it in danhsachHdSaingay)
                {
                    var getmst = tbHoadon.AsEnumerable().Where(m => m["SoHD"].ToString() == it.SoHD && m["KyHieu"].ToString()==it.KHHD).FirstOrDefault();
                    if (getmst != null)
                    {
                         //Tìm tiếp danh sách chungtu từ masao
                         var getMaCT=tbChungtu.AsEnumerable().Where(m => m["MaSo"].ToString() == getmst["MaSo"].ToString()).FirstOrDefault();
                         var getMaCTGV= tbChungtu.AsEnumerable().Where(m => m["SoHieu"].ToString() == $"{getMaCT["SoHieu"].ToString()}GV" && m["NgayCT"].ToString()== getMaCT["NgayCT"].ToString()).FirstOrDefault();
                        if (getMaCT != null)
                        {
                            //Lấy danh sách chungtu liên quan từ MaCT
                            var getListCT = tbChungtu.AsEnumerable().Where(m => m["MaCT"].ToString() == getMaCT["MaCT"].ToString()).ToList();
                            foreach(var item in getListCT)
                            {
                                string query = @"UPDATE ChungTu SET NgayCT=?, NgayGS=? where MaSo=? ";

                                var parameters = new OleDbParameter[]
                         {
               new OleDbParameter("?", it.NgayLap),
                 new OleDbParameter("?", it.NgayLap),
                   new OleDbParameter("?", item["MaSo"].ToString()),
                         };
                                int rowsAffected = ExecuteQueryResult(query, parameters);
                            }
                        }

                        if (getMaCTGV != null)
                        {
                            //Lấy danh sách chungtu liên quan từ MaCT
                            var getListCT = tbChungtu.AsEnumerable().Where(m => m["MaCT"].ToString() == getMaCTGV["MaCT"].ToString()).ToList();
                            foreach (var item in getListCT)
                            {
                                string query = @"UPDATE ChungTu SET NgayCT=?, NgayGS=? where MaSo=? ";

                                var parameters = new OleDbParameter[]
                         {
               new OleDbParameter("?", it.NgayLap),
                 new OleDbParameter("?", it.NgayLap),
                   new OleDbParameter("?", item["MaSo"].ToString()),
                         };
                                int rowsAffected = ExecuteQueryResult(query, parameters);
                            }
                        }
                    }
                } 
            }
        }
        List<ChungTuHD> lstChungTuHD = new List<ChungTuHD>();
        private void KTHT_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;

            ThietLapControl();
            LoadComboBoxYear();
            progressPanel1.Description = "";
            LoadData();

            //Lấy tooken 1 lần đầu tiên

            LoadControl();
            string query = "SELECT * FROM License";

            // Tạo mảng tham số với giá trị cho câu lệnh SQL

            var kq = ExecuteQuery(query, null);
            mstcongty = kq.Rows[0]["MaSoThue"].ToString();
            CCCD = kq.Rows[0]["CCCD"].ToString();
            int namtc = int.Parse(kq.Rows[0]["NamTC"].ToString());
            cbbNam.SelectedItem = namtc;

            // simpleButton1.PerformClick();

            query = "SELECT * FROM tbRegister";
            // Tạo mảng tham số với giá trị cho câu lệnh SQL

            kq = ExecuteQuery(query, null);
            savedPath = kq.Rows[0]["Hoadonpath"].ToString();
            user = kq.Rows[0]["Username"].ToString();
            password = kq.Rows[0]["Password"].ToString();
            progressPanel1.Visible = false;

            // 1. Query (nên chỉ lấy cột cần thiết)
            DataTable tbChungtu = ExecuteQuery("SELECT MaCT, MaLoai, SoHieu, MaTKTCNo,MaTKTCCo, SoPS,NgayImport FROM ChungTu", null);
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
            foreach (DataRow r in tbChungtu.Rows)
            {
                int maCT = Convert.ToInt32(r["MaCT"]);
                if (!chungTuGroups.ContainsKey(maCT))
                    chungTuGroups[maCT] = new List<DataRow>();

                chungTuGroups[maCT].Add(r);
            }

            // 4. Query join
            DataTable tonghop = ExecuteQuery(@"
    SELECT c.MaCT, c.MaLoai,c.NgayCT,c.ThangCT, c.SoHieu,c.NgayImport, h.MaKhachHang,h.KyHieu
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
                item.NgayImport= DateTime.Parse(row["NgayImport"].ToString());
                if (item.SoHieu == "35")
                {
                    int test = 10;
                }
                item.KHHD= row["KyHieu"].ToString();
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

                        if (maTK == "14038")
                            tienThue += soPS;
                        else
                            tienTrcThue += soPS;
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

                        if (maTK == "5108")
                            tienThue += soPS;
                        else
                            tienTrcThue += soPS;
                    }

                    item.TienTrcThue = tienTrcThue;
                    item.TienThue = tienThue;
                    item.TongTien = tienTrcThue + tienThue;
                }
                lstChungTuHD.Add(item);
            }
        }

        public class KhachHangInfo
        {
            public string MST { get; set; }
            public string Ten { get; set; }
        } 
        public class ChungTuHD
        {
            public int Type { get; set; }
            public int MaSo { get; set; }
            public int MaCT { get; set; }
            public string SoHieu { get; set; }
            public string KHHD {  get; set; }   
            public DateTime NgayCT { get; set; } 
            public string TenKH { get; set; }
            public string MST { get; set; } 
            public double TongTien { get; set; }
            public double TienTrcThue { get; set; }
            public double TienThue { get; set; }    
            public DateTime NgayImport { get; set; }
        } 
        DataTable tonghop { get; set; }
        private void radDauvao_CheckedChanged(object sender, EventArgs e)
        {
           
        }

        private void gridView1_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            var selectedRow = gridView1.GetRow(e.RowHandle) as clsKTHT;
            if (selectedRow.Khautruthue == 1)
                return;
            if (e.Column.FieldName == "STTType")
            {
                if (e.CellValue == null)
                    return;
                //Tìm type
                int rowHandle = e.RowHandle;
                var gridView = sender as GridView;
                var getype = gridView.GetRowCellValue(rowHandle,"Type");
                var getValue = e.CellValue.ToString();
          
                if (e.CellValue.ToString() == "Hóa đơn mới")
                { 
                    e.Appearance.ForeColor = Color.Black; // Tô màu chữ đỏ
                }
                else
                {
                    e.Appearance.BackColor = Color.Red;
                    e.Appearance.ForeColor = Color.White; // Tô màu chữ đỏ
                }
            }
            //Kiểm tra tiền trước thuế
            if (e.Column.FieldName == "TienTrcThueHD")
            {
                var gridView = sender as GridView;
                int rowHandle = e.RowHandle;
                double getdt1 = (double)gridView1.GetRowCellValue(rowHandle, "TienTrcThue");
                double gettt = (double)gridView1.GetRowCellValue(rowHandle, "TongTienTT");
                //TongTienPhi 
                double getdt2 = double.Parse(e.CellValue.ToString());
                if ( getdt1!= getdt2)
                {
                    if (getdt1 != 0)
                    {
                        e.Appearance.ForeColor = Color.Red; // Tô màu chữ đỏ
                    }
                    else
                    {
                        if (getdt2 != gettt)
                        {
                            e.Appearance.ForeColor = Color.Red; // Tô màu chữ đỏ
                        }
                    }
                   
                }
            }
            //Kiểm tra tiền  thuế
            if (e.Column.FieldName == "TienThueHD")
            {
                var gridView = sender as GridView;
                int rowHandle = e.RowHandle;
                double getdt1 = (double)gridView1.GetRowCellValue(rowHandle, "TienThue");
                double getdt2 =double.Parse(e.CellValue.ToString());
                if (getdt1 != 0 && getdt2 != 0 &&  getdt1 != getdt2)
                {
                    e.Appearance.ForeColor = Color.Red; // Tô màu chữ đỏ
                }
            }
            if (e.Column.FieldName == "TongTienTTHD")
            {
                var gridView = sender as GridView;
                int rowHandle = e.RowHandle;
                double getdt1 = (double)gridView1.GetRowCellValue(rowHandle, "TongTienTT");
                double getdt2 = double.Parse(e.CellValue.ToString());
                if (getdt1 != 0 && getdt2 != 0 && getdt1 != getdt2)
                {
                    e.Appearance.ForeColor = Color.Red; // Tô màu chữ đỏ
                }
            }
            if (e.Column.FieldName == "MSTHD")
            {
                var gridView = sender as GridView;
                int rowHandle = e.RowHandle;
                if(gridView.GetRowCellValue(rowHandle, "MST")!=null)
                {
                    string getdt1 = gridView.GetRowCellValue(rowHandle, "MST").ToString();
                    string getdt2 = e.CellValue?.ToString();
                    if (!string.IsNullOrEmpty(getdt1) && !string.IsNullOrEmpty(getdt2) && getdt1 != getdt2)
                    {
                        e.Appearance.ForeColor = Color.Red; // Tô màu chữ đỏ
                    }
                }
               
            }
            if (e.Column.FieldName == "NgayTai")
            {
                var gridView = sender as GridView;
                int rowHandle = e.RowHandle;
                if (Convert.ToDateTime(e.CellValue) != DateTime.MinValue) // 01/01/01
                {
                    if (selectedRow.StatusImport != -1)
                    {
                        e.Appearance.ForeColor = Color.White;
                        e.Appearance.BackColor = Color.Red; // Tô màu chữ đỏ
                    }
                    if (selectedRow.StatusImport == -1)
                    {
                        e.Appearance.ForeColor = Color.Black;
                        e.Appearance.BackColor = Color.Yellow; // Tô màu chữ đỏ
                    }
                 
                }
            }
            if (e.Column.FieldName == "NgayNhap")
            {
                var gridView = sender as GridView;
                int rowHandle = e.RowHandle;
                if (Convert.ToDateTime(e.CellValue) != DateTime.MinValue) // 01/01/01
                {
                    e.Appearance.ForeColor = Color.White;
                    e.Appearance.BackColor = Color.Green; // Tô màu chữ đỏ
                }
            }
        }

        private void gridView1_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "NgayNhap" || e.Column.FieldName == "NgayTai") // Thay thế "NgayNhap" bằng tên cột của bạn
            {
                // Kiểm tra giá trị ngày nhập
                if (Convert.ToDateTime(e.Value) == DateTime.MinValue) // 01/01/01
                {
                    e.DisplayText = "";
                }
               
            }
            //TienTrcThueHD
            if (e.Column.FieldName == "TienTrcThueHD" || e.Column.FieldName == "TienThueHD" || e.Column.FieldName == "TongTienTTHD") // Thay thế "NgayNhap" bằng tên cột của bạn
            {
                // Kiểm tra giá trị ngày nhập
                if (Convert.ToDouble(e.Value) ==0) // 01/01/01
                {
                    e.DisplayText = "";
                }

            }
        }

        private void radDaura_CheckedChanged(object sender, EventArgs e)
        {
          
        }

        private void cbbChonthang_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbbDenthang.EditValue= cbbChonthang.EditValue;
        }
    }
}