using ClosedXML.Excel;
using DevExpress.Data.Helpers;
using DevExpress.Utils.Extensions;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraMap.Native;
using DevExpress.XtraWaitForm;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using SaovietTax.Database;
using SaovietTax.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
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
            public int Type { get; set; } 
            public int QueryType { get; set; } //1 ,2 ,3, 4, 5
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
            public int Statustype { get; set; }
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
                    //url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2}";
                    url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel?sort=tdlap:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2}";
                    filename = $"{mstcongty}_Hoadondientu.xlsx";
                    break;
                case 2: // Máy tính tiền bán ra
                case 3:
                    //url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-excel?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2}";
                    url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-excel?sort=tdlap:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2}";
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
        private async Task Taiexcelvao()
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
                    await XulyexelvaoMTT(myTokken,3,i);
                }
            }
            else
            {
                for (int i = tuthang; i <= denthang; i++)
                {
                    Xulyexel2(myTokken, 1,i);
                    // Xulyexel2(myTokken, 2,i);
                    await XulyexelraMTT(tokken, 2,i);
                } 
            }
            Dohoadon();
            // LoadDanhsachExcel();
            progressPanel1.Visible=false;
        }
        private async Task<string> TaiExcelMTTRaTheoDoan(string token, DateTime dtFrom, DateTime dtTo, CancellationToken cancellationToken)
        {
            string formattedDate1 = dtFrom.ToString("dd/MM/yyyy") + "T00:00:00";
            string formattedDate2 = dtTo.ToString("dd/MM/yyyy") + "T23:59:59";

            string url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-excel?sort=tdlap:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2}";
            string filename = $"{mstcongty}_HDDienTuMayTinhTien_{dtFrom:ddMMyyyy}_{dtTo:ddMMyyyy}.xlsx";
            string action = "Xuất excel (hóa đơn máy tính tiền bán ra)";
            string endPoint = "/tra-cuu/tra-cuu-hoa-don";

            string pathYear = $"HD{dtFrom.Year}";
            string directoryPath = Path.Combine(savedPath, pathYear, "HDRa", dtFrom.Month.ToString());
            string filePath = Path.Combine(directoryPath, filename);

            Directory.CreateDirectory(directoryPath);

            if (File.Exists(filePath))
            {
                DateTime lastWriteTime = File.GetLastWriteTime(filePath);
                if ((DateTime.Now - lastWriteTime).TotalMinutes < 30) return filePath;
            }

            int maxRetries = 3;
            int retryDelayMs = 2500;
            bool success = false;

            var cookieContainer = new System.Net.CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true,
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            using (var client = new HttpClient(handler))
            {
                // SỬA LỖI: Tăng Timeout từ 10s lên 60s
                client.Timeout = TimeSpan.FromSeconds(60);

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/155.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "vi");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://hoadondientu.gdt.gov.vn");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://hoadondientu.gdt.gov.vn/tra-cuu/tra-cuu-hoa-don");

                _captchaKey = "";
                _captchaValue = "";

                string capUrl = "https://hoadondientu.gdt.gov.vn/api/captcha";
                HttpResponseMessage resCap;

                using (var capReq = new HttpRequestMessage(HttpMethod.Get, capUrl))
                {
                    capReq.Headers.TryAddWithoutValidation("Request-Id", Guid.NewGuid().ToString());
                    capReq.Headers.TryAddWithoutValidation("End-Point", "/");
                    resCap = await client.SendAsync(capReq, cancellationToken);
                }

                string capBody = await resCap.Content.ReadAsStringAsync();

                if (!resCap.IsSuccessStatusCode)
                {
                    ToastMeaasge("● Lỗi", $"Không lấy được captcha ({(int)resCap.StatusCode})");
                    await Task.Delay(2000, cancellationToken);
                    return null;
                }

                MyJson capJson = JsonConvert.DeserializeObject<MyJson>(capBody);

                string cvalue = "";
                try
                {
                   SetCaption("Đang giải mã captcha...");
                    cvalue = SolveCaptcha(capJson.Content);
                    if (string.IsNullOrWhiteSpace(cvalue)) throw new Exception("Captcha giải ra rỗng");

                    _captchaKey = capJson.Key ?? "";
                    _captchaValue = cvalue.Trim();

                    url += $"&ckey={Uri.EscapeDataString(_captchaKey)}&cvalue={Uri.EscapeDataString(_captchaValue)}";
                    Console.WriteLine($"[CAPTCHA-Ra] ckey={_captchaKey} | cvalue={_captchaValue}");
                }
                catch (Exception ex)
                {
                    ToastMeaasge("● Lỗi", "Không giải mã được captcha: " + ex.Message);
                    return null;
                }

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                      
                        SetCaption($"Đang tải MTT bán ra {dtFrom:dd/MM} - {dtTo:dd/MM} (Lần {attempt}/{maxRetries})");
                        using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                        {
                            request.Headers.TryAddWithoutValidation("Request-Id", Guid.NewGuid().ToString());
                            request.Headers.TryAddWithoutValidation("End-Point", endPoint);
                            request.Headers.TryAddWithoutValidation("Action", Uri.EscapeDataString(action));

                            if (!string.IsNullOrEmpty(_captchaKey))
                            {
                                request.Headers.TryAddWithoutValidation("ckey", _captchaKey);
                                request.Headers.TryAddWithoutValidation("cvalue", _captchaValue);
                            }

                            var response = await client.SendAsync(request, cancellationToken);

                            if (!response.IsSuccessStatusCode)
                            {
                                string errBody = await response.Content.ReadAsStringAsync();
                                Console.WriteLine($"[MTT-Ra {(int)response.StatusCode}] {errBody}");
                            }

                            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                            {
                                if (attempt < maxRetries) { await Task.Delay(retryDelayMs * attempt, cancellationToken); continue; }
                                else { ToastMeaasge("● Lỗi", "Bị chặn 403 khi tải excel MTT bán ra"); return null; }
                            }

                            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                            {
                                if (attempt < maxRetries) { await Task.Delay(retryDelayMs * attempt, cancellationToken); continue; }
                                else { ToastMeaasge("● Lỗi", "Captcha sai hoặc hết hạn (400)."); return null; }
                            }

                            if ((int)response.StatusCode == 429 || (int)response.StatusCode == 504)
                            {
                                if (attempt < maxRetries) { await Task.Delay(retryDelayMs * attempt * 2, cancellationToken); continue; }
                                else { ToastMeaasge("● Lỗi", $"Máy chủ thuế trả về {(int)response.StatusCode}."); return null; }
                            }

                            response.EnsureSuccessStatusCode();
                            byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

                            if (fileBytes.Length < 2048) throw new Exception("File tải về quá nhỏ");
                            if (!(fileBytes.Length > 4 && fileBytes[0] == 0x50 && fileBytes[1] == 0x4B)) throw new Exception("Dữ liệu không phải Excel");

                            File.WriteAllBytes(filePath, fileBytes);
                            success = true;
                            break;
                        }
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        ToastMeaasge("● Thông báo", "Đã hủy tải file.");
                        return null;
                    }
                    catch (OperationCanceledException)
                    {
                        // SỬA LỖI: Phân biệt Timeout (mạng chậm) và Hủy (người dùng)
                        Console.WriteLine($"[Timeout] Tải MTT bán ra {dtFrom:dd/MM}-{dtTo:dd/MM} (Lần {attempt}/{maxRetries})");
                        if (attempt == maxRetries)
                        {
                            ToastMeaasge("● Lỗi", $"Timeout tải MTT bán ra {dtFrom:dd/MM}-{dtTo:dd/MM}");
                            return null;
                        }
                        await Task.Delay(retryDelayMs * attempt, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi MTT bán ra {dtFrom:dd/MM}-{dtTo:dd/MM} (Lần {attempt}/{maxRetries}): {ex.Message}");

                        if (attempt == maxRetries) { ToastMeaasge("● Lỗi", $"Không tải được MTT bán ra {dtFrom:dd/MM}-{dtTo:dd/MM}"); }
                        else { await Task.Delay(retryDelayMs * attempt, cancellationToken); }
                    }
                }
            }

            if (success) return filePath;
            return null;
        }
        public async Task XulyexelraMTT(string token, int _type,int thang, CancellationToken cancellationToken = default)
        {
            string tenfileExcel = "";
            if (_type == 1)
                tenfileExcel = "Hoadondientu";
            else if (_type == 2)
                tenfileExcel = "HDDienTuMayTinhTien";

            if (mstcongty == "8046549703")
                mstcongty = "048172000197";

            DateTime dtFrom = new DateTime((int)cbbNam.EditValue, thang, 1);
            DateTime dtTo = dtFrom.AddMonths(1).AddDays(-1);

            // ================= ĐẶC BIỆT CHO HÓA ĐƠN MÁY TÍNH TIỀN BÁN RA =================
            if (_type == 2)
            {
                DateTime monthStart = dtFrom;
                DateTime monthEnd = dtTo;
                DateTime chunkStart = monthStart;
                int chunkIndex = 1;

                string pathYear = $"HD{dtFrom.Year}";
                string directoryPath = Path.Combine(savedPath, pathYear, "HDRa", dtFrom.Month.ToString());
                Directory.CreateDirectory(directoryPath);

                string mergedFilePath = Path.Combine(directoryPath,
                    $"{mstcongty}_HDDienTuMayTinhTien_FullThang{dtFrom:MMyyyy}.xlsx");

                // ===== CHECK 30 PHÚT: NẾU FILE FULL MTT BÁN RA VỪA TẢI < 30 PHÚT THÌ BỎ QUA =====
                if (File.Exists(mergedFilePath))
                {
                    var fiFull = new FileInfo(mergedFilePath);
                    if (fiFull.Length > 2048 &&
                        (DateTime.Now - fiFull.LastWriteTime).TotalMinutes < 30)
                    {
                        Console.WriteLine($"[MTT-Ra] File Full vừa tải <30 phút, bỏ qua: {mergedFilePath}");
                        ToastMeaasge("● Thông báo", "File MTT bán ra đã tải trong 30 phút gần đây, không tải lại.");
                        return;
                    }
                }

                // Đếm số đoạn cần tải (11 ngày/đoạn)
                int expectedChunks = 0;
                DateTime temp = monthStart;
                while (temp <= monthEnd)
                {
                    expectedChunks++;
                    DateTime e = temp.AddDays(10);
                    if (e > monthEnd) e = monthEnd;
                    temp = e.AddDays(1);
                }

                var downloadedFiles = new List<string>();

                while (chunkStart <= monthEnd)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    DateTime chunkEnd = chunkStart.AddDays(10); // 11 ngày
                    if (chunkEnd > monthEnd) chunkEnd = monthEnd;

                    // Kiểm tra file đoạn này đã có chưa
                    string expectedFile = Path.Combine(directoryPath,
                        $"{mstcongty}_HDDienTuMayTinhTien_{chunkStart:ddMMyyyy}_{chunkEnd:ddMMyyyy}.xlsx");

                    if (File.Exists(expectedFile))
                    {
                        var fi = new FileInfo(expectedFile);
                        if (fi.Length > 2048)
                        {
                            Console.WriteLine($"[MTT-Ra] Đoạn {chunkIndex}/{expectedChunks} đã có, bỏ qua: {expectedFile}");
                            downloadedFiles.Add(expectedFile);
                            chunkStart = chunkEnd.AddDays(1);
                            chunkIndex++;
                            continue;
                        }
                        try { File.Delete(expectedFile); } catch { }
                    }

                    Console.WriteLine($"[MTT-Ra] Đoạn {chunkIndex}/{expectedChunks}: {chunkStart:dd/MM/yyyy} - {chunkEnd:dd/MM/yyyy}");

                    string chunkFile = await TaiExcelMTTRaTheoDoan(token, chunkStart, chunkEnd, cancellationToken);

                    if (!string.IsNullOrEmpty(chunkFile) && File.Exists(chunkFile))
                        downloadedFiles.Add(chunkFile);

                    await Task.Delay(2500, cancellationToken); // tránh 429

                    chunkStart = chunkEnd.AddDays(1);
                    chunkIndex++;
                }

                // ================= GỘP CÁC FILE ĐÃ TẢI (APPEND-ONLY + SORT + STT) =================
                // Quét TOÀN BỘ file lẻ trên đĩa (không chỉ file tải trong lần này)
                var allChunkFiles = new List<string>();
                DateTime scanDate = monthStart;
                while (scanDate <= monthEnd)
                {
                    DateTime e = scanDate.AddDays(10);
                    if (e > monthEnd) e = monthEnd;

                    string f = Path.Combine(directoryPath,
                        $"{mstcongty}_HDDienTuMayTinhTien_{scanDate:ddMMyyyy}_{e:ddMMyyyy}.xlsx");

                    if (File.Exists(f) && new FileInfo(f).Length > 2048)
                        allChunkFiles.Add(f);

                    scanDate = e.AddDays(1);
                }

                if (allChunkFiles.Count > 0)
                {
                    try
                    {
                       
                        SetCaption($"Đang gộp {allChunkFiles.Count} file lẻ → {mergedFilePath}");
                        // (1) Gộp append-only: Full cũ + tất cả file lẻ trên đĩa
                        GopFileExcelAppendOnly(allChunkFiles, mergedFilePath);
                        Console.WriteLine($"[MTT-Ra] Đã gộp {allChunkFiles.Count} file lẻ → {mergedFilePath}");

                        // (2) Sửa tiêu đề khoảng ngày cho đúng cả tháng
                        try
                        {
                            using (var wb = new ClosedXML.Excel.XLWorkbook(mergedFilePath))
                            {
                                var ws = wb.Worksheet(1);
                                var usedRange = ws.RangeUsed();
                                if (usedRange != null)
                                {
                                    foreach (var cell in usedRange.CellsUsed())
                                    {
                                        if (cell.GetString().StartsWith("Từ ngày", StringComparison.OrdinalIgnoreCase))
                                        {
                                            cell.Value = $"Từ ngày {dtFrom:dd/MM/yyyy} đến ngày {dtTo:dd/MM/yyyy}";
                                            break;
                                        }
                                    }
                                }
                                wb.Save();
                            }
                        }
                        catch (Exception exTitle)
                        {
                            Console.WriteLine($"[MTT-Ra] Không sửa được tiêu đề: {exTitle.Message}");
                        }

                        // (3) Sort lại theo ngày lập (tăng dần)
                        try
                        {
                            SortFullByNgayLap(mergedFilePath, MTT_COL_NGAY_LAP, ascending: true);
                        }
                        catch (Exception exSort)
                        {
                            Console.WriteLine($"[MTT-Ra SORT ERROR] {exSort.Message}");
                            ToastMeaasge("● Cảnh báo", "Không sort được file Full: " + exSort.Message);
                        }

                        // (4) Đánh lại STT xuyên suốt (bỏ qua nếu MTT_COL_STT <= 0)
                        try
                        {
                            RenumberSTT(mergedFilePath, MTT_COL_STT);
                        }
                        catch (Exception exStt)
                        {
                            Console.WriteLine($"[MTT-Ra STT ERROR] {exStt.Message}");
                        }

                        // (5) Chỉ xóa file lẻ khi đã đủ số đoạn
                        if (allChunkFiles.Count == expectedChunks)
                        {
                            foreach (string f in allChunkFiles)
                            {
                                try { if (File.Exists(f)) File.Delete(f); } catch { }
                            }
                            ToastMeaasge("● Thông báo", $"Đã gộp đủ {allChunkFiles.Count} file thành công.");
                        }
                        else
                        {
                            ToastMeaasge("● Cảnh báo",
                                $"Mới có {allChunkFiles.Count}/{expectedChunks} đoạn. File Full đã cập nhật (chỉ thêm), chạy lại để tải bù.");
                            Console.WriteLine($"[MTT-Ra] Giữ {allChunkFiles.Count} file lẻ để gộp bù lần sau.");
                        }

                        SetCaption($"Đã gộp xong {allChunkFiles.Count} file lẻ → {mergedFilePath}");
                    }
                    catch (Exception exMerge)
                    {
                        ToastMeaasge("● Lỗi", "Không gộp được file: " + exMerge.Message);
                        Console.WriteLine($"[MTT-Ra MERGE ERROR] {exMerge}");
                    }
                }
                else
                {
                    ToastMeaasge("● Thông báo", "Không có file nào để gộp.");
                }
                return;
            }

            // ================= LOẠI 1 (HÓA ĐƠN ĐIỆN TỬ BÁN RA) =================
            string formattedDate1 = dtFrom.ToString("dd/MM/yyyy") + "T00:00:00";
            string formattedDate2 = dtTo.ToString("dd/MM/yyyy") + "T23:59:59";

            string url, filename;
            string action = "";
            string endPoint = "/tra-cuu/tra-cuu-hoa-don";

            switch (_type)
            {
                case 1:
                    url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel?sort=tdlap:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2}";
                    filename = $"{mstcongty}_Hoadondientu.xlsx";
                    action = "Xuất excel (hóa đơn bán ra)";
                    break;
                default:
                    return;
            }

            string pathYear1 = $"HD{(int)cbbNam.EditValue}";
            string directoryPath1 = Path.Combine(savedPath, pathYear1, "HDRa",thang.ToString());
            string filePath1 = Path.Combine(directoryPath1, filename);

            Directory.CreateDirectory(directoryPath1);

            if (File.Exists(filePath1))
            {
                DateTime lastWriteTime = File.GetLastWriteTime(filePath1);
                if ((DateTime.Now - lastWriteTime).TotalMinutes < 30) return;
            }

            int maxRetries = 3;
            int retryDelayMs = 2500;
            bool success = false;

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(90); // 90s cho an toàn

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/155.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "vi");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://hoadondientu.gdt.gov.vn");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://hoadondientu.gdt.gov.vn/tra-cuu/tra-cuu-hoa-don");

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                       
                        SetCaption($"Đang tải {tenfileExcel} (Lần {attempt}/{maxRetries})");
                        using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                        {
                            request.Headers.TryAddWithoutValidation("Request-Id", Guid.NewGuid().ToString());
                            request.Headers.TryAddWithoutValidation("End-Point", endPoint);
                            request.Headers.TryAddWithoutValidation("Action", Uri.EscapeDataString(action));

                            var response = await client.SendAsync(request, cancellationToken);

                            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                            {
                                if (attempt < maxRetries) { await Task.Delay(retryDelayMs * attempt, cancellationToken); continue; }
                                else { ToastMeaasge("● Lỗi", "Bị chặn 403 khi tải excel bán ra"); return; }
                            }

                            response.EnsureSuccessStatusCode();
                            byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

                            if (fileBytes.Length < 2048) throw new Exception("File tải về quá nhỏ (có thể bị chặn hoặc không có dữ liệu)");

                            if (!(fileBytes.Length > 4 && fileBytes[0] == 0x50 && fileBytes[1] == 0x4B)) throw new Exception("Dữ liệu trả về không phải file Excel");

                            File.WriteAllBytes(filePath1, fileBytes);
                            success = true;
                            break;
                        }
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        Console.WriteLine($"Tác vụ tải {tenfileExcel} đã bị hủy bởi người dùng.");
                        ToastMeaasge("● Thông báo", "Đã hủy tải file.");
                        return;
                    }
                    catch (OperationCanceledException)
                    {
                        // Đây là lỗi Timeout (do mạng hoặc server chậm)
                        Console.WriteLine($"Timeout tải {tenfileExcel} (Lần {attempt}/{maxRetries})");
                        if (attempt == maxRetries) { ToastMeaasge("● Lỗi", "Timeout tải file excel bán ra"); return; }
                        await Task.Delay(retryDelayMs * attempt, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi khi tải {tenfileExcel} (Lần {attempt}/{maxRetries}): {ex.Message}");
                        if (attempt == maxRetries) { ToastMeaasge("● Lỗi", "Không tải được file excel bán ra"); return; }
                        await Task.Delay(retryDelayMs * attempt, cancellationToken);
                    }
                }
            }

            SetCaption($"Đã tải xong {tenfileExcel}");
        }
        public void ToastMeaasge(string title, string text)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(text)
                .SetToastDuration(ToastDuration.Short)
                .Show();
        }
        private string _captchaKey = "";
        private string _captchaValue = "";
        private void SetCaption(string caption)
        {
            try
            {
                if (string.IsNullOrEmpty(caption)) return;
                if (this.IsDisposed || this.Disposing) return;

                // ✅ Case 1: Đang chạy trên UI thread → gán trực tiếp (nhanh)
                if (!this.InvokeRequired)
                {
                    if (progressPanel1 != null && !progressPanel1.IsDisposed)
                        progressPanel1.Caption = caption;
                    return;
                }

                // ✅ Case 2: Worker thread → BeginInvoke (không chờ, không deadlock)
                this.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (this.IsDisposed || this.Disposing) return;

                        if (progressPanel1 != null && !progressPanel1.IsDisposed)
                            progressPanel1.Caption = caption;
                    }
                    catch { }
                }));
            }
            catch { }
        }
        private string SolveCaptcha(string svgContent)
        {
            try
            {
                var solver = new SvgCaptchaSolver();
                string result = solver.SolveCaptcha(svgContent);

                // string svgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "captcha.svg");
                // File.WriteAllText(svgPath, svgContent);

                // string base64 = SvgToBase64(svgContent);
                // Testimg2(base64);

                //var pythoncapcha= Readcapcha();
                return result;
            }
            catch (Exception ex)
            {
                LogToFile("SolveCaptcha", ex);
                return null;
            }
        }
        private static readonly object _logLock = new object();

        private static void LogToFile(string tag, Exception ex, string extra = "")
        {
            try
            {
                if (ex == null) return;

                lock (_logLock)
                {
                    string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                    Directory.CreateDirectory(logDir);

                    string file = Path.Combine(logDir, $"error_{DateTime.Now:yyyyMMdd}.log");

                    var sb = new StringBuilder();
                    sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{tag}] {ex.Message}");

                    if (!string.IsNullOrEmpty(extra))
                        sb.AppendLine($"  Extra: {extra}");

                    if (ex.InnerException != null)
                        sb.AppendLine($"  Inner: {ex.InnerException.Message}");

                    sb.AppendLine($"  Stack: {ex.StackTrace}");
                    sb.AppendLine();

                    File.AppendAllText(file, sb.ToString(), Encoding.UTF8);
                }
            }
            catch
            {
                // Nuốt mọi lỗi — log không được phép làm chết app
                Debug.WriteLine($"[LogToFile failed] {tag}: {ex?.Message}");
            }
        }

        private async Task<string> TaiExcelMTTTheoDoan(string token, DateTime dtFrom, DateTime dtTo, CancellationToken cancellationToken)
        {
            string formattedDate1 = dtFrom.ToString("dd/MM/yyyy") + "T00:00:00";
            string formattedDate2 = dtTo.ToString("dd/MM/yyyy") + "T23:59:59";

            string url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-excel-sold?sort=tdlap:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==8&type=purchase";
            string filename = $"{mstcongty}_HDDienTuMayTinhTien_{dtFrom:ddMMyyyy}_{dtTo:ddMMyyyy}.xlsx";
            string action = "Xuất hóa đơn (hóa đơn máy tính tiền mua vào)";
            string endPoint = "/tra-cuu/tra-cuu-hoa-don";

            string pathYear = $"HD{dtFrom.Year}";
            string directoryPath = Path.Combine(savedPath, pathYear, "HDVao", dtFrom.Month.ToString());
            string filePath = Path.Combine(directoryPath, filename);

            Directory.CreateDirectory(directoryPath);

            // Nếu file đã tồn tại và mới tải trong 30 phút → trả về luôn để gộp
            if (File.Exists(filePath))
            {
                DateTime lastWriteTime = File.GetLastWriteTime(filePath);
                if ((DateTime.Now - lastWriteTime).TotalMinutes < 30)
                    return filePath;
            }

            int maxRetries = 3;
            int retryDelayMs = 2500;
            bool success = false;

            var cookieContainer = new System.Net.CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true,
                AutomaticDecompression = System.Net.DecompressionMethods.GZip
                                       | System.Net.DecompressionMethods.Deflate
            };

            using (var client = new HttpClient(handler))
            {
                client.Timeout = TimeSpan.FromSeconds(100);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/156.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "vi");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://hoadondientu.gdt.gov.vn");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://hoadondientu.gdt.gov.vn/tra-cuu/tra-cuu-hoa-don");
                client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\"Not:A-Brand\";v=\"8\", \"Chromium\";v=\"156\", \"Google Chrome\";v=\"156\"");
                client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");

                _captchaKey = "";
                _captchaValue = "";

                // Lấy captcha
                string capUrl = "https://hoadondientu.gdt.gov.vn/api/captcha";
                HttpResponseMessage resCap;

                using (var capReq = new HttpRequestMessage(HttpMethod.Get, capUrl))
                {
                    capReq.Headers.TryAddWithoutValidation("Request-Id", Guid.NewGuid().ToString());
                    capReq.Headers.TryAddWithoutValidation("End-Point", "/");
                    resCap = await client.SendAsync(capReq, cancellationToken);
                }

                string capBody = await resCap.Content.ReadAsStringAsync();

                if (!resCap.IsSuccessStatusCode)
                {
                    if (capBody.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase))
                        ToastMeaasge("● Lỗi", "Bị WAF/403 HTML. Thử lại sau hoặc đổi IP/User-Agent");
                    else
                        ToastMeaasge("● Lỗi", $"Không lấy được captcha ({(int)resCap.StatusCode}), thử lại sau 2s");

                    await Task.Delay(2000, cancellationToken);
                    return null;
                }

                MyJson capJson = JsonConvert.DeserializeObject<MyJson>(capBody);

                string svgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "captcha.svg");
                try
                {
                    File.WriteAllText(svgPath, capJson.Content);
                }
                catch (Exception exFile)
                {
                    ToastMeaasge("● Lỗi", "Không ghi được file captcha: " + exFile.Message);
                    return null;
                }
 
                    SetCaption("Đang giải mã captcha");

                string cvalue = "";
                try
                {
                    //string svgCaptcha = File.ReadAllText(svgPath);
                    //string base64 = SvgToBase64(svgCaptcha);
                    //Testimg2(base64);
                    //cvalue = Readcapcha();
                    SetCaption("Đang giải mã captcha...");
                    cvalue = SolveCaptcha(capJson.Content);
                    if (string.IsNullOrWhiteSpace(cvalue))
                        throw new Exception("Captcha giải ra rỗng");

                    _captchaKey = capJson.Key ?? "";
                    _captchaValue = cvalue.Trim();

                    url += $"&ckey={Uri.EscapeDataString(_captchaKey)}&cvalue={Uri.EscapeDataString(_captchaValue)}";

                    Console.WriteLine($"[CAPTCHA] ckey={_captchaKey} | cvalue={_captchaValue}");
                }
                catch (Exception ex)
                {
                    ToastMeaasge("● Lỗi", "Không giải mã được captcha: " + ex.Message);
                    return null;
                }

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                       

                        using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                        {
                            request.Headers.TryAddWithoutValidation("Request-Id", Guid.NewGuid().ToString());
                            request.Headers.TryAddWithoutValidation("End-Point", endPoint);
                            request.Headers.TryAddWithoutValidation("Action", Uri.EscapeDataString(action));

                            if (!string.IsNullOrEmpty(_captchaKey))
                            {
                                request.Headers.TryAddWithoutValidation("ckey", _captchaKey);
                                request.Headers.TryAddWithoutValidation("cvalue", _captchaValue);
                            }

                            var response = await client.SendAsync(request, cancellationToken);

                            if (!response.IsSuccessStatusCode)
                            {
                                string errBody = await response.Content.ReadAsStringAsync();
                                Console.WriteLine($"[MTT {(int)response.StatusCode}] {errBody}");
                            }

                            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                            {
                                if (attempt < maxRetries)
                                {
                                    await Task.Delay(retryDelayMs * attempt, cancellationToken);
                                    continue;
                                }
                                else
                                {
                                    ToastMeaasge("● Lỗi", "Bị chặn 403 khi tải excel MTT");
                                    return null;
                                }
                            }

                            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                            {
                                if (attempt < maxRetries)
                                {
                                    await Task.Delay(retryDelayMs * attempt, cancellationToken);
                                    continue;
                                }
                                else
                                {
                                    ToastMeaasge("● Lỗi", "Captcha sai hoặc hết hạn (400).");
                                    return null;
                                }
                            }

                            if ((int)response.StatusCode == 429 || (int)response.StatusCode == 504)
                            {
                                if (attempt < maxRetries)
                                {
                                    await Task.Delay(retryDelayMs * attempt * 2, cancellationToken);
                                    continue;
                                }
                                else
                                {
                                    ToastMeaasge("● Lỗi", $"Máy chủ thuế trả về {(int)response.StatusCode}.");
                                    return null;
                                }
                            }

                            response.EnsureSuccessStatusCode();
                            byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

                            if (fileBytes.Length < 2048)
                            {
                                string errorContent = Encoding.UTF8.GetString(fileBytes);
                                throw new Exception("File tải về quá nhỏ: " + errorContent);
                            }

                            if (fileBytes.Length > 4 && fileBytes[0] == 0x50 && fileBytes[1] == 0x4B)
                            {
                                File.WriteAllBytes(filePath, fileBytes);
                                success = true;
                                break;
                            }
                            else
                            {
                                string preview = Encoding.UTF8.GetString(fileBytes, 0, Math.Min(300, fileBytes.Length));
                                throw new Exception("Dữ liệu trả về không phải file Excel: " + preview);
                            }
                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        // Kiểm tra xem có phải người dùng chủ động hủy không
                        bool isCancelledByUser = cancellationToken.IsCancellationRequested;

                        // Kiểm tra xem có phải timeout của HttpClient không (inner là TimeoutException)
                        bool isTimeout = ex.InnerException is TimeoutException;

                        Console.WriteLine($"[TaskCanceled] isCancelledByUser={isCancelledByUser}, isTimeout={isTimeout}, Type={ex.GetType().Name}");

                        if (isCancelledByUser)
                        {
                            // Người dùng chủ động hủy → thoát ngay, không retry
                            ToastMeaasge("● Thông báo", "Đã hủy tải file.");
                            return null;
                        }

                        if (isTimeout)
                        {
                            // Timeout do server chậm → coi như lỗi tạm thời, thử lại
                            Console.WriteLine($"[Timeout] Tải MTT {dtFrom:dd/MM}-{dtTo:dd/MM} quá thời gian (Lần {attempt}/{maxRetries})");

                            if (attempt == maxRetries)
                            {
                                ToastMeaasge("● Lỗi", $"Timeout tải MTT {dtFrom:dd/MM}-{dtTo:dd/MM} sau {maxRetries} lần");
                                return null;
                            }

                            await Task.Delay(retryDelayMs * attempt, cancellationToken);
                            continue;
                        }

                        // Các trường hợp OperationCanceledException khác (hiếm)
                        ToastMeaasge("● Thông báo", "Tác vụ bị hủy: " + ex.Message);
                        return null;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi MTT {dtFrom:dd/MM}-{dtTo:dd/MM} (Lần {attempt}/{maxRetries}): {ex.Message}");

                        if (attempt == maxRetries)
                        {
                            ToastMeaasge("● Lỗi", $"Không tải được MTT {dtFrom:dd/MM}-{dtTo:dd/MM}");
                        }
                        else
                        {
                            await Task.Delay(retryDelayMs * attempt, cancellationToken);
                        }
                    }
                }
            }

            if (success)
            {
                Console.WriteLine($"Tải thành công MTT: {filePath}");
                return filePath;  // TRẢ VỀ ĐƯỜNG DẪN FILE
            }

            return null;
        }
        private void RenumberSTT(string fullFilePath, int colSTT)
        {
            if (!File.Exists(fullFilePath)) return;
            if (colSTT <= 0) return;

            using (var wb = new ClosedXML.Excel.XLWorkbook(fullFilePath))
            {
                var ws = wb.Worksheet(1);
                var used = ws.RangeUsed();
                if (used == null) return;

                int lastRow = used.LastRow().RowNumber();
                if (lastRow < MTT_DATA_START_ROW) return;

                int stt = 1;
                for (int r = MTT_DATA_START_ROW; r <= lastRow; r++)
                {
                    // Bỏ qua dòng trống hoàn toàn (nếu có)
                    var usedRow = ws.Row(r).CellsUsed();
                    if (!usedRow.Any())
                        continue;

                    ws.Cell(r, colSTT).Value = stt++;
                }

                wb.Save();
                Console.WriteLine($"[STT] Đã đánh lại STT 1..{stt - 1} (cột {colSTT}).");
            }
        }
        private const int MTT_COL_STT = 1;   // ← CHỈNH nếu STT không phải cột 1
        private string BuildInvoiceKey(ClosedXML.Excel.IXLWorksheet ws, int row,
                                       int colMauSo, int colKyHieu, int colSoHD, int colNgayLap)
        {
            string mauSo = ws.Cell(row, colMauSo).GetString().Trim();
            string kyHieu = ws.Cell(row, colKyHieu).GetString().Trim();
            string soHD = ws.Cell(row, colSoHD).GetString().Trim();

            // Chuẩn hóa ngày: ưu tiên DateTime, fallback parse string
            var cellNgay = ws.Cell(row, colNgayLap);
            string ngayStr;
            if (cellNgay.DataType == ClosedXML.Excel.XLDataType.DateTime)
            {
                ngayStr = cellNgay.GetDateTime().ToString("yyyyMMdd");
            }
            else
            {
                string s = cellNgay.GetString().Trim();
                if (DateTime.TryParseExact(s,
                        new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" },
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime dt))
                    ngayStr = dt.ToString("yyyyMMdd");
                else
                    ngayStr = s; // giữ nguyên nếu không parse được
            }

            // Cả 4 đều rỗng → dòng trống
            if (string.IsNullOrEmpty(mauSo) && string.IsNullOrEmpty(kyHieu)
                && string.IsNullOrEmpty(soHD) && string.IsNullOrEmpty(ngayStr))
                return "";

            return $"{mauSo}|{kyHieu}|{soHD}|{ngayStr}";
        }
        private void GopFileExcelAppendOnly(List<string> inputFiles, string outputFile)
        {
            int headerRow = MTT_HEADER_ROW;
            int dataStartRow = MTT_DATA_START_ROW;

            var fullWb = new ClosedXML.Excel.XLWorkbook();
            var fullWs = fullWb.Worksheets.Add("HoaDonMTT");

            var existingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int outputRow = headerRow;
            int firstCol = 1, lastCol = 1;
            bool headerCopied = false;

            // ========== BƯỚC 1: ĐỌC FULL CŨ (NẾU CÓ) ==========
            if (File.Exists(outputFile))
            {
                try
                {
                    using (var oldWb = new ClosedXML.Excel.XLWorkbook(outputFile))
                    {
                        var oldWs = oldWb.Worksheet(1);
                        var used = oldWs.RangeUsed();
                        if (used != null)
                        {
                            firstCol = used.FirstColumn().ColumnNumber();
                            lastCol = used.LastColumn().ColumnNumber();
                            int oldLastRow = used.LastRow().RowNumber();

                            // Copy toàn bộ nội dung Full cũ sang workbook mới
                            for (int r = 1; r <= oldLastRow; r++)
                            {
                                for (int c = firstCol; c <= lastCol; c++)
                                    CopyCell(oldWs.Cell(r, c), fullWs.Cell(r, c - firstCol + 1));
                            }

                            // Copy độ rộng cột
                            for (int c = firstCol; c <= lastCol; c++)
                            {
                                double w = oldWs.Column(c).Width;
                                if (w > 0) fullWs.Column(c - firstCol + 1).Width = w;
                            }

                            // Xây tập khóa đã có
                            for (int r = dataStartRow; r <= oldLastRow; r++)
                            {
                                string key = BuildInvoiceKey(oldWs, r,
                                    MTT_COL_MAU_SO, MTT_COL_KY_HIEU,
                                    MTT_COL_SO_HD, MTT_COL_NGAY_LAP);
                                if (!string.IsNullOrEmpty(key))
                                    existingKeys.Add(key);
                            }

                            outputRow = oldLastRow;
                            headerCopied = true;

                            Console.WriteLine($"[MERGE] Full cũ: {existingKeys.Count} hóa đơn, {oldLastRow} dòng.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MERGE] Không đọc được Full cũ ({ex.Message}), bắt đầu mới.");
                    existingKeys.Clear();
                    outputRow = headerRow;
                    headerCopied = false;
                }
            }

            // ========== BƯỚC 2: DUYỆT FILE ĐẦU VÀO, THÊM DÒNG MỚI ==========
            int addedCount = 0, skippedCount = 0;

            foreach (string file in inputFiles)
            {
                if (!File.Exists(file)) continue;

                try
                {
                    using (var srcWb = new ClosedXML.Excel.XLWorkbook(file))
                    {
                        var srcWs = srcWb.Worksheet(1);
                        var used = srcWs.RangeUsed();
                        if (used == null) continue;

                        int lastRow = used.LastRow().RowNumber();
                        int srcFirstCol = used.FirstColumn().ColumnNumber();
                        int srcLastCol = used.LastColumn().ColumnNumber();

                        // Nếu Full chưa có gì → copy header từ file đầu tiên
                        if (!headerCopied)
                        {
                            firstCol = srcFirstCol;
                            lastCol = srcLastCol;

                            for (int r = 1; r <= headerRow; r++)
                            {
                                for (int c = firstCol; c <= lastCol; c++)
                                    CopyCell(srcWs.Cell(r, c), fullWs.Cell(r, c - firstCol + 1));
                            }
                            for (int c = firstCol; c <= lastCol; c++)
                            {
                                double w = srcWs.Column(c).Width;
                                if (w > 0) fullWs.Column(c - firstCol + 1).Width = w;
                            }

                            outputRow = headerRow;
                            headerCopied = true;
                        }

                        // Duyệt dữ liệu
                        for (int r = dataStartRow; r <= lastRow; r++)
                        {
                            string key = BuildInvoiceKey(srcWs, r,
                                MTT_COL_MAU_SO, MTT_COL_KY_HIEU,
                                MTT_COL_SO_HD, MTT_COL_NGAY_LAP);

                            if (string.IsNullOrEmpty(key)) continue;   // dòng trống

                            if (existingKeys.Contains(key))
                            {
                                skippedCount++;
                                continue;   // đã có → giữ nguyên, không thêm
                            }

                            // Chưa có → thêm vào cuối
                            outputRow++;
                            for (int c = srcFirstCol; c <= srcLastCol; c++)
                                CopyCell(srcWs.Cell(r, c),
                                         fullWs.Cell(outputRow, c - srcFirstCol + 1));

                            existingKeys.Add(key);
                            addedCount++;
                        }
                    }
                }
                catch (Exception exFile)
                {
                    Console.WriteLine($"[MERGE] Lỗi đọc file {file}: {exFile.Message}");
                }
            }

            // ========== BƯỚC 3: LƯU ==========
            if (!headerCopied)
            {
                Console.WriteLine("[MERGE] Không có dữ liệu để gộp.");
                return;
            }

            // Sao lưu Full cũ trước khi ghi đè
            if (File.Exists(outputFile))
            {
                try
                {
                    string backup = outputFile + ".bak";
                    File.Copy(outputFile, backup, overwrite: true);
                }
                catch (Exception exBk)
                {
                    Console.WriteLine($"[MERGE] Không sao lưu được: {exBk.Message}");
                }
            }

            fullWb.SaveAs(outputFile);
            Console.WriteLine($"[MERGE] +{addedCount} mới, bỏ qua {skippedCount} trùng. Tổng Full = {existingKeys.Count}.");
        }
        private void CopyCell(ClosedXML.Excel.IXLCell src, ClosedXML.Excel.IXLCell dst)
        {
            // Copy style (màu nền, font, border, căn lề)
            dst.Style.Fill = src.Style.Fill;
            dst.Style.Font = src.Style.Font;
            dst.Style.Border = src.Style.Border;
            dst.Style.Alignment = src.Style.Alignment;
            dst.Style.NumberFormat = src.Style.NumberFormat;

            // Copy giá trị
            if (src.DataType == ClosedXML.Excel.XLDataType.Text) dst.Value = src.GetString();
            else if (src.DataType == ClosedXML.Excel.XLDataType.Number) dst.Value = src.GetDouble();
            else if (src.DataType == ClosedXML.Excel.XLDataType.DateTime) dst.Value = src.GetDateTime();
            else if (src.DataType == ClosedXML.Excel.XLDataType.Boolean) dst.Value = src.GetBoolean();
            else dst.Value = src.GetFormattedString();
        }
        public async Task XulyexelvaoMTT(string token, int _type, int thang, CancellationToken cancellationToken = default)
        {
            string tenfileExcel = "";
            if (_type == 1) tenfileExcel = "File excel hoá đơn nhận mã";
            else if (_type == 2) tenfileExcel = "File excel hoá đơn không nhận mã";
            else if (_type == 3) tenfileExcel = "File Excel hoá đơn máy tính tiền";

            DateTime dtFrom = new DateTime((int)cbbNam.EditValue, thang, 1);
            DateTime dtTo = dtFrom.AddMonths(1).AddDays(-1);

            // ================= ĐẶC BIỆT CHO HÓA ĐƠN MÁY TÍNH TIỀN: CHIA ĐOẠN ≤ 11 NGÀY =================
            if (_type == 3)
            {
                DateTime monthStart = dtFrom;
                DateTime monthEnd = dtTo;
                DateTime chunkStart = monthStart;
                int chunkIndex = 1;

                string pathYear = $"HD{dtFrom.Year}";
                string directoryPath = Path.Combine(savedPath, pathYear, "HDVao", dtFrom.Month.ToString());
                Directory.CreateDirectory(directoryPath);

                string mergedFilePath = Path.Combine(directoryPath,
                    $"{mstcongty}_HDDienTuMayTinhTien_FullThang{dtFrom:MMyyyy}.xlsx");

                // ===== CHECK 30 PHÚT: NẾU FILE FULL MTT VỪA TẢI < 30 PHÚT THÌ BỎ QUA =====
                if (File.Exists(mergedFilePath))
                {
                    var fiFull = new FileInfo(mergedFilePath);
                    if (fiFull.Length > 2048 &&
                        (DateTime.Now - fiFull.LastWriteTime).TotalMinutes < 30)
                    {
                        Console.WriteLine($"[MTT] File Full vừa tải <30 phút, bỏ qua: {mergedFilePath}");
                        ToastMeaasge("● Thông báo", "File MTT đã tải trong 30 phút gần đây, không tải lại.");
                        return;
                    }
                }

                // Đếm số đoạn cần tải (11 ngày/đoạn)
                int expectedChunks = 0;
                DateTime temp = monthStart;
                while (temp <= monthEnd)
                {
                    expectedChunks++;
                    DateTime e = temp.AddDays(10); // 11 ngày
                    if (e > monthEnd) e = monthEnd;
                    temp = e.AddDays(1);
                }

                var downloadedFiles = new List<string>();

                while (chunkStart <= monthEnd)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    DateTime chunkEnd = chunkStart.AddDays(10); // 11 ngày
                    if (chunkEnd > monthEnd) chunkEnd = monthEnd;

                    // Kiểm tra file đoạn này đã có chưa
                    string expectedFile = Path.Combine(directoryPath,
                        $"{mstcongty}_HDDienTuMayTinhTien_{chunkStart:ddMMyyyy}_{chunkEnd:ddMMyyyy}.xlsx");

                    if (File.Exists(expectedFile))
                    {
                        // Chỉ cần file có dữ liệu thật là dùng luôn, không tải lại
                        var fi = new FileInfo(expectedFile);
                        if (fi.Length > 2048)
                        {
                            Console.WriteLine($"[MTT] Đoạn {chunkIndex}/{expectedChunks} đã có, bỏ qua: {expectedFile}");
                            downloadedFiles.Add(expectedFile);
                            chunkStart = chunkEnd.AddDays(1);
                            chunkIndex++;
                            continue;
                        }
                        // file rỗng/hỏng → xóa và tải lại
                        try { File.Delete(expectedFile); } catch { }
                    }

                    Console.WriteLine($"[MTT] Đoạn {chunkIndex}/{expectedChunks}: {chunkStart:dd/MM/yyyy} - {chunkEnd:dd/MM/yyyy}");

                    string chunkFile = await TaiExcelMTTTheoDoan(token, chunkStart, chunkEnd, cancellationToken);

                    if (!string.IsNullOrEmpty(chunkFile) && File.Exists(chunkFile))
                        downloadedFiles.Add(chunkFile);

                    await Task.Delay(2500, cancellationToken); // tránh 429

                    chunkStart = chunkEnd.AddDays(1);
                    chunkIndex++;
                }

                // ================= GỘP CÁC FILE ĐÃ TẢI (APPEND-ONLY + SORT + STT) =================
                // Quét TOÀN BỘ file lẻ trên đĩa (không chỉ file tải trong lần này)
                var allChunkFiles = new List<string>();
                DateTime scanDate = monthStart;
                while (scanDate <= monthEnd)
                {
                    DateTime e = scanDate.AddDays(10);
                    if (e > monthEnd) e = monthEnd;

                    string f = Path.Combine(directoryPath,
                        $"{mstcongty}_HDDienTuMayTinhTien_{scanDate:ddMMyyyy}_{e:ddMMyyyy}.xlsx");

                    if (File.Exists(f) && new FileInfo(f).Length > 2048)
                        allChunkFiles.Add(f);

                    scanDate = e.AddDays(1);
                }

                if (allChunkFiles.Count > 0)
                {
                    try
                    {
                        SetCaption("Đang gộp file Excel MTT (append-only)...");

                        // (1) Gộp append-only: Full cũ + tất cả file lẻ trên đĩa
                        GopFileExcelAppendOnly(allChunkFiles, mergedFilePath);
                        Console.WriteLine($"[MTT] Đã gộp {allChunkFiles.Count} file lẻ → {mergedFilePath}");

                        // (2) Sửa tiêu đề khoảng ngày cho đúng cả tháng
                        try
                        {
                            using (var wb = new ClosedXML.Excel.XLWorkbook(mergedFilePath))
                            {
                                var ws = wb.Worksheet(1);
                                var usedRange = ws.RangeUsed();
                                if (usedRange != null)
                                {
                                    foreach (var cell in usedRange.CellsUsed())
                                    {
                                        if (cell.GetString().StartsWith("Từ ngày", StringComparison.OrdinalIgnoreCase))
                                        {
                                            cell.Value = $"Từ ngày {dtFrom:dd/MM/yyyy} đến ngày {dtTo:dd/MM/yyyy}";
                                            break;
                                        }
                                    }
                                }
                                wb.Save();
                            }
                        }
                        catch (Exception exTitle)
                        {
                            Console.WriteLine($"[MTT] Không sửa được tiêu đề: {exTitle.Message}");
                        }

                        // (3) Sort lại theo ngày lập (tăng dần)
                        try
                        {
                            SortFullByNgayLap(mergedFilePath, MTT_COL_NGAY_LAP, ascending: true);
                        }
                        catch (Exception exSort)
                        {
                            Console.WriteLine($"[MTT SORT ERROR] {exSort.Message}");
                            ToastMeaasge("● Cảnh báo", "Không sort được file Full: " + exSort.Message);
                        }

                        // (4) Đánh lại STT xuyên suốt (bỏ qua nếu MTT_COL_STT <= 0)
                        try
                        {
                            RenumberSTT(mergedFilePath, MTT_COL_STT);
                        }
                        catch (Exception exStt)
                        {
                            Console.WriteLine($"[MTT STT ERROR] {exStt.Message}");
                        }

                        // (5) Chỉ xóa file lẻ khi đã đủ số đoạn
                        if (allChunkFiles.Count == expectedChunks)
                        {
                            foreach (string f in allChunkFiles)
                            {
                                try { if (File.Exists(f)) File.Delete(f); } catch { }
                            }
                            ToastMeaasge("● Thông báo", $"Đã gộp đủ {allChunkFiles.Count} file thành công.");
                        }
                        else
                        {
                            ToastMeaasge("● Cảnh báo",
                                $"Mới có {allChunkFiles.Count}/{expectedChunks} đoạn. File Full đã cập nhật (chỉ thêm), chạy lại để tải bù.");
                            Console.WriteLine($"[MTT] Giữ {allChunkFiles.Count} file lẻ để gộp bù lần sau.");
                        }

                        SetCaption($"Đã gộp xong: {Path.GetFileName(mergedFilePath)}");
                    }
                    catch (Exception exMerge)
                    {
                        ToastMeaasge("● Lỗi", "Không gộp được file: " + exMerge.Message);
                        Console.WriteLine($"[MTT MERGE ERROR] {exMerge}");
                    }
                }
                else
                {
                    ToastMeaasge("● Thông báo", "Không có file nào để gộp.");
                }

                return;
            }

            // ================= CÁC LOẠI 1, 2 GIỮ NGUYÊN LOGIC CŨ =================
            string formattedDate1 = dtFrom.ToString("dd/MM/yyyy") + "T00:00:00";
            string formattedDate2 = dtTo.ToString("dd/MM/yyyy") + "T23:59:59";

            string url, filename;
            string action = "";
            string endPoint = "/tra-cuu/tra-cuu-hoa-don";

            switch (_type)
            {
                case 1:
                    url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel-sold?sort=tdlap:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==5&type=purchase";
                    filename = $"{mstcongty}_HDDienTuDaCapMa.xlsx";
                    action = "Xuất excel (hóa đơn mua vào)";
                    break;
                case 2:
                    url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel-sold?sort=tdlap:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==6&type=purchase";
                    filename = $"{mstcongty}_HDDienTuKhongMa.xlsx";
                    action = "Xuất excel (hóa đơn mua vào)";
                    break;
                default:
                    return;
            }

            string pathYear1 = $"HD{(int)cbbNam.EditValue}";
            string directoryPath1 = Path.Combine(savedPath, pathYear1, "HDVao", thang.ToString());
            string filePath1 = Path.Combine(directoryPath1, filename);

            Directory.CreateDirectory(directoryPath1);

            if (File.Exists(filePath1))
            {
                DateTime lastWriteTime = File.GetLastWriteTime(filePath1);
                if ((DateTime.Now - lastWriteTime).TotalMinutes < 30) return;
            }

            int maxRetries = 3;
            int retryDelayMs = 2500;
            bool success = false;

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            using (var client = new HttpClient(handler))
            {
                client.Timeout = TimeSpan.FromSeconds(60);

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/156.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "vi");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://hoadondientu.gdt.gov.vn");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://hoadondientu.gdt.gov.vn/tra-cuu/tra-cuu-hoa-don");
                client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\"Not:A-Brand\";v=\"8\", \"Chromium\";v=\"156\", \"Google Chrome\";v=\"156\"");
                client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        SetCaption($"Đang tải {tenfileExcel} (Lần {attempt}/{maxRetries})");

                        using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                        {
                            request.Headers.TryAddWithoutValidation("Request-Id", Guid.NewGuid().ToString());
                            request.Headers.TryAddWithoutValidation("End-Point", endPoint);
                            request.Headers.TryAddWithoutValidation("Action", Uri.EscapeDataString(action));

                            var response = await client.SendAsync(request, cancellationToken);
                            sw.Stop();

                            if (!response.IsSuccessStatusCode)
                            {
                                string errBody = await response.Content.ReadAsStringAsync();
                                Console.WriteLine($"[{(int)response.StatusCode}] {errBody}");
                            }

                            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                            {
                                if (attempt < maxRetries)
                                {
                                    await Task.Delay(retryDelayMs * attempt, cancellationToken);
                                    continue;
                                }
                                else
                                {
                                    ToastMeaasge("● Lỗi", "Bị chặn 403 khi tải excel (có thể do WAF)");
                                    return;
                                }
                            }

                            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                            {
                                if (attempt < maxRetries)
                                {
                                    await Task.Delay(retryDelayMs * attempt, cancellationToken);
                                    continue;
                                }
                                else
                                {
                                    ToastMeaasge("● Lỗi", "Yêu cầu không hợp lệ (400).");
                                    return;
                                }
                            }

                            if ((int)response.StatusCode == 429 || (int)response.StatusCode == 504)
                            {
                                if (attempt < maxRetries)
                                {
                                    await Task.Delay(retryDelayMs * attempt * 2, cancellationToken);
                                    continue;
                                }
                                else
                                {
                                    ToastMeaasge("● Lỗi", $"Máy chủ thuế trả về {(int)response.StatusCode}. Thử lại sau.");
                                    return;
                                }
                            }

                            response.EnsureSuccessStatusCode();
                            byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

                            if (fileBytes.Length < 2048)
                            {
                                string errorContent = Encoding.UTF8.GetString(fileBytes);
                                throw new Exception("File tải về quá nhỏ: " + errorContent);
                            }

                            if (fileBytes.Length > 4 && fileBytes[0] == 0x50 && fileBytes[1] == 0x4B)
                            {
                                File.WriteAllBytes(filePath1, fileBytes);
                                success = true;
                                break;
                            }
                            else
                            {
                                string preview = Encoding.UTF8.GetString(fileBytes, 0, Math.Min(300, fileBytes.Length));
                                throw new Exception("Dữ liệu trả về không phải file Excel: " + preview);
                            }
                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        sw.Stop();
                        long elapsedMs = sw.ElapsedMilliseconds;
                        bool isCancelledByUser = cancellationToken.IsCancellationRequested;
                        bool isTimeout = !isCancelledByUser && elapsedMs >= (client.Timeout.TotalMilliseconds - 3000);

                        Console.WriteLine($"[TaskCanceled] elapsed={elapsedMs}ms | isCancelledByUser={isCancelledByUser} | isTimeout={isTimeout}");

                        if (isCancelledByUser)
                        {
                            ToastMeaasge("● Thông báo", "Đã hủy tải file.");
                            return;
                        }

                        if (isTimeout)
                        {
                            if (attempt == maxRetries)
                            {
                                ToastMeaasge("● Lỗi", $"Timeout tải {tenfileExcel} sau {maxRetries} lần");
                                return;
                            }
                            await Task.Delay(retryDelayMs * attempt, cancellationToken);
                            continue;
                        }

                        ToastMeaasge("● Thông báo", $"Tác vụ bị hủy không rõ nguyên nhân (sau {elapsedMs}ms)");
                        return;
                    }
                    catch (Exception ex)
                    {
                        sw.Stop();
                        Console.WriteLine($"Lỗi khi tải {tenfileExcel} (Lần {attempt}/{maxRetries}): {ex.Message}");

                        if (attempt == maxRetries)
                        {
                            ToastMeaasge("● Lỗi", "Không tải được file excel, sẽ thử lại sau 3s");
                            await Task.Delay(3000, cancellationToken);
                            if (this.InvokeRequired) this.Invoke(new Action(() => simpleButton3.PerformClick()));
                            else simpleButton3.PerformClick();
                        }
                        else
                        {
                            await Task.Delay(retryDelayMs * attempt, cancellationToken);
                        }
                    }
                }
            }

            if (success)
            {
                Console.WriteLine($"Tải thành công: {filePath1}");
                SetCaption($"Đã tải xong {tenfileExcel}");
            }
        }
        private const int MTT_HEADER_ROW = 6;   // dòng header (1-based)
        private const int MTT_DATA_START_ROW = 7;   // dòng dữ liệu đầu tiên
        private const int MTT_COL_MAU_SO = 2;   // ← CHỈNH theo mẫu Excel thực tế
        private const int MTT_COL_KY_HIEU = 3;   // ← CHỈNH
        private const int MTT_COL_SO_HD = 4;   // ← CHỈNH
        private const int MTT_COL_NGAY_LAP = 5;   // ← CHỈNH
        private void SortFullByNgayLap(string fullFilePath, int colNgayLap, bool ascending = true)
        {
            int headerRow = MTT_HEADER_ROW;
            int dataStartRow = MTT_DATA_START_ROW;

            if (!File.Exists(fullFilePath)) return;

            using (var wb = new ClosedXML.Excel.XLWorkbook(fullFilePath))
            {
                var ws = wb.Worksheet(1);
                var used = ws.RangeUsed();
                if (used == null) return;

                int lastRow = used.LastRow().RowNumber();
                int firstCol = used.FirstColumn().ColumnNumber();
                int lastCol = used.LastColumn().ColumnNumber();

                if (lastRow < dataStartRow) return;   // không có dữ liệu

                // ===== BƯỚC 1: Đọc toàn bộ dòng dữ liệu vào List =====
                var rows = new List<(DateTime Ngay, object[] Values, string[] Formats)>();

                for (int r = dataStartRow; r <= lastRow; r++)
                {
                    var cellNgay = ws.Cell(r, colNgayLap);

                    DateTime ngay;
                    if (cellNgay.DataType == ClosedXML.Excel.XLDataType.DateTime)
                    {
                        ngay = cellNgay.GetDateTime();
                    }
                    else
                    {
                        string s = cellNgay.GetString().Trim();
                        if (!DateTime.TryParseExact(s,
                                new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" },
                                System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.None,
                                out ngay))
                        {
                            ngay = DateTime.MinValue;
                        }
                    }

                    var values = new object[lastCol - firstCol + 1];
                    var formats = new string[lastCol - firstCol + 1];

                    for (int c = firstCol; c <= lastCol; c++)
                    {
                        var cell = ws.Cell(r, c);
                        values[c - firstCol] = cell.Value;
                        formats[c - firstCol] = cell.Style.NumberFormat.Format ?? "";
                    }

                    rows.Add((ngay, values, formats));
                }

                // ===== BƯỚC 2: Sort theo ngày =====
                rows = ascending
                    ? rows.OrderBy(x => x.Ngay).ToList()
                    : rows.OrderByDescending(x => x.Ngay).ToList();

                // ===== BƯỚC 3: Xóa dữ liệu cũ và ghi lại theo thứ tự mới =====
                ws.Range(dataStartRow, firstCol, lastRow, lastCol)
                  .Clear(ClosedXML.Excel.XLClearOptions.Contents);

                int outputRow = dataStartRow;
                foreach (var row in rows)
                {
                    for (int i = 0; i < row.Values.Length; i++)
                    {
                        var dst = ws.Cell(outputRow, firstCol + i);
                        dst.Value = ClosedXML.Excel.XLCellValue.FromObject(row.Values[i]);

                        // Giữ format số/ngày của ô gốc
                        if (!string.IsNullOrEmpty(row.Formats[i]))
                            dst.Style.NumberFormat.Format = row.Formats[i];
                    }
                    outputRow++;
                }

                wb.Save();
                Console.WriteLine($"[SORT] Đã sort {rows.Count} dòng theo ngày lập ({(ascending ? "tăng" : "giảm")}).");
            }
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
        public int trylogin = 3;
        private async Task Getttoken()
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

            if (needLogin || 1<2)
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
                        // ⚠ BỎ "br" vì .NET Framework không auto-decompress Brotli
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://hoadondientu.gdt.gov.vn");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://hoadondientu.gdt.gov.vn/");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua",
                            "\"Google Chrome\";v=\"140\", \"Chromium\";v=\"140\", \"Not=A?Brand\";v=\"24\"");
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

                        string svgPath = Path.Combine(Path.GetTempPath(), "captcha_selenium.svg");
                        File.WriteAllText(svgPath, capJson.Content);

                        // Lấy XSRF-TOKEN nếu có
                        //var cookies = cookieContainer.GetCookies(new Uri("https://hoadondientu.gdt.gov.vn"));
                        //string xsrfToken = cookies["XSRF-TOKEN"]?.Value;
                        //if (!string.IsNullOrEmpty(xsrfToken))
                        //{
                        //    client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
                        //    client.DefaultRequestHeaders.TryAddWithoutValidation("X-XSRF-TOKEN", xsrfToken);
                        //}

                        // ================= STEP 2: SOLVE CAPTCHA =================
                        progressPanel1.Caption = "Đang giải captcha...";
                        Application.DoEvents();

                        string cvalue = SolveCaptcha(capJson.Content);
                        if (cvalue.Length <= 5)
                        {
                            if (trylogin > 1)
                            {
                                trylogin -= 1;
                                Getttoken();
                            }
                            else
                            {
                                XtraMessageBox.Show("Không thể tải file Excel trong thời điểm này, vui lòng thử lại sau!");
                            }
                           
                        }
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
                            //XtraMessageBox.Show("Đăng nhập thất bại (401): " + loginBody);
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
           await  Getttoken(); // đảm bảo có token

            int type = 0;
            if (radDauvao.Checked)
            {
                if(selectedRow.QueryType==1 || selectedRow.QueryType==2)
                type = 4;
                else
                    if(selectedRow.QueryType == 3)
                    type = 5;
            }  
            else
            {
                if (selectedRow.QueryType == 4)
                type = 5;
                if (selectedRow.QueryType == 5)
                    type = 4;
            }

            string url = GetInvoiceUrl(type, mst, selectedRow.KHHD, selectedRow.SoHD, selectedRow.KHMS);
            string filename = $"{selectedRow.NgayLap:yyyyMMdd}_{mst}_{selectedRow.SoHD}_{selectedRow.KHHD}.zip";
            string path = Path.Combine(savedPath, yearPath, pathravao, tuthang.ToString(), filename);

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(60);

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", myTokken);
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
        int slvb6 = 0;
        double cqt_tongtien, cqt_tientrcthue, cqt_tienthue = 0;
        double vb6_tongtien, vb6_tientrcthue, vb6_tienthue = 0;
        int slhangchuanhapma = 0;
        int slhangchuaiport = 0;
        int lshoadonloi = 0;
        int slhangdaimport = 0;
        public  List<ChungTuHD> listRemain = new List<ChungTuHD>();
        private void Dohoadon()
        {
            listRemain = new List<ChungTuHD>();
            slcoquanthue = 0;
            slvb6 = 0;
            cqt_tongtien = 0;
            cqt_tientrcthue = 0;
            cqt_tienthue = 0;
            vb6_tongtien = 0;
            vb6_tientrcthue = 0;
            vb6_tienthue = 0;
            slhangchuanhapma = 0;
            slhangchuaiport = 0;
            lshoadonloi = 0;
            slhangdaimport = 0;
            progressPanel1.Visible = true;
            progressPanel1.Caption = "Đang xử lý dữ liệu";
            Application.DoEvents();
            clsKTHTs = new List<clsKTHT>();
            int tuthang = int.Parse(cbbChonthang.Text.Replace("Tháng ", ""));
            int denthang = int.Parse(cbbDenthang.Text.Replace("Tháng ", ""));
            string typeHD = "";
            typeHD = radDauvao.Checked ? "HDVao" : "HDRa";

            // === Đặt ở đầu hàm Dohoadon(), trước khi vòng lặp ===

            // 1. Dictionary để tìm hóa đơn nhanh (thay cho FirstOrDefault)
            var dictChungTu = new Dictionary<string, ChungTuHD>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in lstChungTuHD)
            {
                // Key: Ngày + Số HĐ đã bỏ số 0 đầu + Ký hiệu
                string key = $"{item.NgayCT:yyyyMMdd}|{Helpers.RemoveLeadingZeros(item.SoHieu).TrimEnd('.')}|{item.KHHD}";

                if (!dictChungTu.ContainsKey(key))
                    dictChungTu[key] = item;
            }

            // 2. Dictionary để kiểm tra mặt hàng trống (thay cho Where + foreach)
            var dictMaCT_HasEmpty = new Dictionary<string, bool>();

            if (gettbChungtu != null && gettbChungtu.Rows.Count > 0)
            {
                var groups = gettbChungtu.AsEnumerable()
                    .GroupBy(r => r["MaCT"].ToString());

                foreach (var g in groups)
                {
                    //14053
                    //bool hasEmpty = g.Any(r =>
                    //    r["MaVattu"].ToString() == "0" &&
                    //    r["MaTKTCNo"].ToString() != "5108" &&
                    //    r["MaTKTCCo"].ToString() != "14038" &&
                    //     r["MaTKTCCo"].ToString() != "14053" &&
                    //    r["MaTKTCNo"].ToString() != "160" &&
                    //     r["MaTKTCNo"].ToString() != "82" &&
                    //      r["MaTKTCCo"].ToString() != "82" &&
                    //        r["MaTKTCNo"].ToString() != "169" &&
                    //      r["MaTKTCCo"].ToString() != "169" &&
                    //      (r["MaTKTCNo"].ToString()!="0" || r["MaTKTCCo"].ToString()!="0") &&
                    //    r["MaTKTCNo"].ToString() != "161");

                    bool hasEmpty = g.Any(r =>
                     r["MaVattu"].ToString() == "0"
                     && r["SoPS"].ToString() != "0"
                     && r["SoPS"].ToString() != "0"
                     && r["MaTKTCNo"].ToString() != "5108"
                     && r["MaTKTCCo"].ToString() != "14038"
                     && r["MaTKTCCo"].ToString() != "125"
                     && r["MaTKTCCo"].ToString() != "126"
                     && (r["SoPS2No"].ToString() != "0" || r["SoPS2Co"].ToString() != "0"));
                    dictMaCT_HasEmpty[g.Key] = hasEmpty;
                }
            }

            for (int i = tuthang; i <= denthang; i++)
            { 
                string CurrentYear = $"HD{cbbNam.EditValue}";
                string directoryPath = Path.Combine(savedPath, CurrentYear, typeHD, i.ToString()).Trim();
                if (!Directory.Exists(directoryPath)) continue;

                var excelFiles = Directory.EnumerateFiles(directoryPath, "*.xlsx", SearchOption.AllDirectories)
                                         .Where(f => f.Contains(mstcongty) || f.Contains(CCCD))
                                         .ToList();
                int stt = 1;
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
                                if (radDauvao.Checked)
                                {
                                    if (excelFile.Contains("DaCapMa"))
                                    {
                                        clsKTHT.QueryType = 1;
                                    }
                                    if (excelFile.Contains("KhongMa"))
                                    {
                                        clsKTHT.QueryType = 2;
                                    }
                                    if (excelFile.Contains("MayTinhTien"))
                                    {
                                        clsKTHT.QueryType = 3;
                                    }
                                }
                                else
                                {
                                    if (excelFile.Contains("MayTinhTien"))
                                    {
                                        clsKTHT.QueryType = 4;
                                    }
                                    if (excelFile.Contains("Hoadondientu"))
                                    {
                                        clsKTHT.QueryType = 5;
                                    }
                                }
                                    clsKTHT.Statustype = 0;
                                clsKTHT.KHMS = GetCellValue(row.Cell("B"));
                                clsKTHT.KHHD = GetCellValue(row.Cell("C"));
                                clsKTHT.SoHD = GetCellValue(row.Cell("D")); 
                                if (radDauvao.Checked)
                                    clsKTHT.Type = 1;
                                if (radDaura.Checked)
                                    clsKTHT.Type = 2;
                                clsKTHT.STT = stt;
                                stt += 1;
                                if (clsKTHT.SoHD == "550")
                                {
                                    int a = 10;
                                }
                                clsKTHT.TenKH = GetCellValue(row.Cell("G"));
                                clsKTHT.NgayLap = DateTime.Parse(GetCellValue(row.Cell("E")));

                                if (clsKTHT.SoHD == "192")
                                {
                                    int ssdf = 10;
                                }
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
                                    cqt_tientrcthue += clsKTHT.TienTrcThue;
                                    cqt_tienthue += clsKTHT.TienThue;
                                }
                                else
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
                                    cqt_tientrcthue += clsKTHT.TienTrcThue;
                                    cqt_tienthue += clsKTHT.TienThue;
                                }
                                //Tìm thông tin trên list chung từ tổng hợp
                                ChungTuHD findhoadon = new ChungTuHD();
                                if (radDauvao.Checked)
                                {
                                    findhoadon = lstChungTuHD.FirstOrDefault(m => m.NgayCT.Date == clsKTHT.NgayLap && Helpers.RemoveLeadingZeros(m.SoHieu) == Helpers.RemoveLeadingZeros(clsKTHT.SoHD).TrimEnd('.') && m.KHHD == clsKTHT.KHHD && m.Type==1);
                                    //Kiểm tra cho trường hợp sai ngày, đúng mst
                                    if(findhoadon == null)
                                    {
                                        findhoadon= lstChungTuHD.FirstOrDefault(m => Helpers.RemoveLeadingZeros(m.SoHieu) == Helpers.RemoveLeadingZeros(clsKTHT.SoHD).TrimEnd('.') && m.KHHD == clsKTHT.KHHD && m.MST== clsKTHT.MST && m.Type == 1);
                                        if(findhoadon != null)
                                        {
                                            clsKTHT.GhiChu += $"Hoá đơn sai ngày chứng từ {findhoadon.NgayCT.ToShortDateString()}";
                                            clsKTHT.Statustype = 2;
                                            lshoadonloi += 1;
                                        }
                                    }
                                }
                                else
                                {
                                    findhoadon = lstChungTuHD.FirstOrDefault(m => m.NgayCT.Date == clsKTHT.NgayLap && Helpers.RemoveLeadingZeros(m.SoHieu) == Helpers.RemoveLeadingZeros(clsKTHT.SoHD).TrimEnd('.') && m.KHHD == clsKTHT.KHHD && m.Type == 2);
                                    if(findhoadon == null)
                                    {
                                        findhoadon = lstChungTuHD.FirstOrDefault(m => Helpers.RemoveLeadingZeros(m.SoHieu) == Helpers.RemoveLeadingZeros(clsKTHT.SoHD).TrimEnd('.') && m.KHHD == clsKTHT.KHHD && m.Type == 2);
                                        if (findhoadon != null)
                                        {
                                            clsKTHT.GhiChu += $"Hoá đơn sai ngày chứng từ {findhoadon.NgayCT.ToShortDateString()}";
                                            clsKTHT.Statustype = 2;
                                            lshoadonloi += 1;
                                        }
                                    }
                                }

                                if (findhoadon != null)
                                {
                                    clsKTHT.NgayNhap = findhoadon.NgayImport;
                                    clsKTHT.TenKHHD = Helpers.ConvertVniToUnicode(findhoadon.TenKH);
                                    clsKTHT.MSTHD = findhoadon.MST;
                                    clsKTHT.TienTrcThueHD = findhoadon.TienTrcThue;
                                    clsKTHT.TienThueHD = findhoadon.TienThue;
                                    clsKTHT.TongTienTTHD = findhoadon.TongTien;
                                    clsKTHT.Checked = true;
                                    vb6_tienthue += clsKTHT.TienThueHD;
                                    vb6_tientrcthue += clsKTHT.TienTrcThueHD;
                                    Thietlapghichu(clsKTHT);
                                    var getlistkt = gettbChungtu.AsEnumerable().Where(m => m["MaCT"].ToString() == findhoadon.MaCT.ToString());
                                    //Kiểm tra xem có mặt hàng nào bỏ trống không
                                    if (dictMaCT_HasEmpty.TryGetValue(findhoadon.MaCT.ToString(), out bool hasEmpty) && hasEmpty || findhoadon.Hangnull==1)
                                    {
                                        clsKTHT.GhiChu += "Có hàng chưa nhập mã";
                                        slhangchuanhapma += 1;
                                        clsKTHT.Statustype = 3;
                                    }
                                    slvb6 += 1;
                                }
                                else
                                {
                                    clsKTHT.Checked = false;
                                    clsKTHT.GhiChu = "Hoá đơn chưa được nhập";
                                    slhangchuaiport += 1;
                                    clsKTHT.Statustype = 1;
                                }
                                if (clsKTHT.Statustype == 0)
                                    slhangdaimport += 1;
                                clsKTHTs.Add(clsKTHT);

                            }
                            catch (Exception ex)
                            {
                                XtraMessageBox.Show(ex.Message + "  " + clsKTHT.SoHD);
                            }

                        }
                    }
                }
                int _type = 0;
                if (radDauvao.Checked)
                    _type = 1;
                else
                    _type = 2;  
                     listRemain = lstChungTuHD.Where(m => m.NgayCT.Month == tuthang && m.Type == _type).Where(m => !clsKTHTs.Where(j => j.NgayLap.Month == tuthang && j.Type == _type).Any(n => n.SoHD == m.SoHieu && m.KHHD == n.KHHD)).ToList();
                //Rán datasource
                foreach(var it in listRemain)
                {
                    clsKTHT clsKTHT = new clsKTHT();
                    clsKTHT.SoHD = it.SoHieu;
                    clsKTHT.KHHD = it.KHHD;
                    clsKTHT.MST = it.MST;
                    clsKTHT.NgayLap = it.NgayCT;
                    clsKTHT.Statustype = 4;
                    clsKTHT.GhiChu = "Hoá đơn nhập dư";
                    clsKTHTs.Add(clsKTHT);
                }
                
            }
            LoadDatasource();
            progressPanel1.Caption = "";
            progressPanel1.Visible = false;

            //Gán thông số
            lbltshd2.Text = slcoquanthue.ToString();
            lbltshd1.Text = slvb6.ToString();
            lblResult2.Text = $"{cqt_tientrcthue.ToString("N0")}"; 
            labelControl5.Text= $"{cqt_tienthue.ToString("N0")}";
            lblResult3.Text= $"{vb6_tientrcthue.ToString("N0")}";
            labelControl4.Text= $"{vb6_tienthue.ToString("N0")}";
           // lblwarning1.Text = slhangchuaiport.ToString();
            lblwarning2.Text = lshoadonloi.ToString();
            lblwarning3.Text=slhangchuanhapma.ToString();
           // lblwarning0.Text= $"{slhangdaimport.ToString("N0")}";
            labelControl10.AllowHtmlString = true;
            labelControl10.Text = $"<u>{slhangchuaiport.ToString("N0")}</u>";
            lblwarining4.Text = listRemain.Count.ToString();
        }
        private void LoadDatasource()
        {
            var orgiginlist = clsKTHTs;
            //if (checkEdit1.Checked)
            //{
            //    orgiginlist = orgiginlist.Where(m => m.Statustype == 0).ToList();
            //}
            //if (chkhoadonchuanhap.Checked)
            //{
            //    orgiginlist = orgiginlist.Where(m => m.Statustype == 1).ToList();
            //}
            //if (chkhoadonsaitt.Checked)
            //{
            //    orgiginlist = orgiginlist.Where(m => m.Statustype == 2).ToList();
            //}
            //if (chkhoadonthieuhang.Checked)
            //{
            //    orgiginlist = orgiginlist.Where(m => m.Statustype == 3).ToList();
            //}
            //if (chkhoadondu.Checked)
            //{
            //    orgiginlist= orgiginlist.Where(m => m.Statustype == 4).ToList();
            //}
            gridControl1.DataSource = orgiginlist;
        }
        private void Thietlapghichu(clsKTHT clsKTHT)
        {
            if (clsKTHT.Statustype == 2)
                return;
            if (clsKTHT.TienTrcThueHD == 0 && clsKTHT.TienThueHD == 0 && clsKTHT.TongTienTTHD == 0)
                return;
            if (clsKTHT.SoHD == "1007")
            {
                int a = 100;
            }
            bool haserro = false;
            if (string.IsNullOrEmpty(clsKTHT.MST))
                clsKTHT.MST = "00";
            
            if (clsKTHT.MST!= clsKTHT.MSTHD && clsKTHT.MSTHD!="...")
            {
                clsKTHT.GhiChu += "MST không khớp , ";
                haserro = true;
            }
            if (clsKTHT.TienTrcThue != clsKTHT.TienTrcThueHD)
            {
                clsKTHT.GhiChu += "Tiền trước thuế bị lệch , ";
                haserro |= true;
            }
            if (clsKTHT.TienThue != clsKTHT.TienThueHD)
            {
                clsKTHT.GhiChu += "Tiền thuế bị lệch , ";
                haserro |= true;
            }
            if (clsKTHT.TongTienTT != clsKTHT.TongTienTTHD)
            {
                clsKTHT.GhiChu += "Tổng tiền bị lệch , ";
                haserro |= true;
            }
            if (haserro)
            {
                clsKTHT.Statustype = 2;
                lshoadonloi += 1;
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

        private void chkhoadonchuanhap_CheckedChanged(object sender, EventArgs e)
        {
       
           
            LoadDatasource();
        }

        private void chkhoadonsaitt_CheckedChanged(object sender, EventArgs e)
        {
           
            LoadDatasource();
        }

        private void chkhoadonthieuhang_CheckedChanged(object sender, EventArgs e)
        {
           
            LoadDatasource();
        }

        private void btnXemchitiet_Click(object sender, EventArgs e)
        {
            string path = savedPath + @"\invoice.txt";
            int index = gridView1.FocusedRowHandle;

            // Kiểm tra có dòng nào được chọn không
            if (index < 0)
            {
                MessageBox.Show("Vui lòng chọn một hóa đơn!");
                return;
            }

            string sohd = gridView1.GetRowCellValue(index, "SoHD")?.ToString() ?? "";
            string ngaylap = DateTime.Parse(gridView1.GetRowCellValue(index, "NgayLap").ToString())
                                     .ToShortDateString();

            string content = $"{sohd}_{ngaylap}";

            // Chỉ ghi MỘT lần, ghi đè hoàn toàn
            File.WriteAllText(path, content + Environment.NewLine, new UTF8Encoding(false));

            // Cập nhật trạng thái
            string qrupdate = "UPDATE tbResponse SET Status = ?";
            var paramss = new OleDbParameter[]
            {
        new OleDbParameter("?", "1"),
            };
            int rrf = ExecuteQueryResult(qrupdate, paramss);
        }

        private void checkEdit1_CheckedChanged(object sender, EventArgs e)
        {
            
            LoadDatasource();
        }

        private void chkhoadondu_CheckedChanged(object sender, EventArgs e)
        {
            LoadDatasource();
        }

        private void labelControl10_Click(object sender, EventArgs e)
        {
            var orgiginlist = clsKTHTs;
            orgiginlist = orgiginlist.Where(m => m.Statustype == 1).ToList();
            gridControl1.DataSource = orgiginlist;
        }

        private void panelControl1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lbltshd1_Click(object sender, EventArgs e)
        {
            var orgiginlist = clsKTHTs;
            orgiginlist = orgiginlist.Where(m => m.Statustype == 0).ToList();
            gridControl1.DataSource = orgiginlist;
        }

        private void lbltshd2_Click(object sender, EventArgs e)
        {
            var orgiginlist = clsKTHTs;
            gridControl1.DataSource = orgiginlist;
        }
          
        private void lblwarning2_Click(object sender, EventArgs e)
        {
          
        }

        private void labelControl11_Click(object sender, EventArgs e)
        {
            if(labelControl11.Appearance.ForeColor== System.Drawing.Color.Blue)
            {
                var orgiginlist = clsKTHTs;
                orgiginlist = orgiginlist.Where(m => m.Statustype == 2).ToList();
                gridControl1.DataSource = orgiginlist;
                labelControl11.Appearance.ForeColor = System.Drawing.Color.Red;
                lblwarning2.Appearance.ForeColor = System.Drawing.Color.Red;
                labelControl12.Appearance.ForeColor = System.Drawing.Color.Blue;
                lblwarning3.Appearance.ForeColor = System.Drawing.Color.Blue;
                labelControl13.Appearance.ForeColor = System.Drawing.Color.Blue;
                lblwarining4.Appearance.ForeColor = System.Drawing.Color.Blue;
            }
            else
            {
                var orgiginlist = clsKTHTs;
                gridControl1.DataSource = orgiginlist;
                labelControl11.Appearance.ForeColor = System.Drawing.Color.Blue;
                lblwarning2.Appearance.ForeColor = System.Drawing.Color.Blue;
               
            }
        }

        private void labelControl12_Click(object sender, EventArgs e)
        {
            if (labelControl12.Appearance.ForeColor == System.Drawing.Color.Blue)
            {
                var orgiginlist = clsKTHTs;
                orgiginlist = orgiginlist.Where(m => m.Statustype == 3).ToList();
                gridControl1.DataSource = orgiginlist;
                labelControl12.Appearance.ForeColor = System.Drawing.Color.Red;
                lblwarning3.Appearance.ForeColor = System.Drawing.Color.Red;
                labelControl11.Appearance.ForeColor = System.Drawing.Color.Blue;
                lblwarning2.Appearance.ForeColor = System.Drawing.Color.Blue;
                labelControl13.Appearance.ForeColor = System.Drawing.Color.Blue;
                lblwarining4.Appearance.ForeColor = System.Drawing.Color.Blue;
            }
            else
            {
                var orgiginlist = clsKTHTs;
                gridControl1.DataSource = orgiginlist;
                labelControl12.Appearance.ForeColor = System.Drawing.Color.Blue;
                lblwarning3.Appearance.ForeColor = System.Drawing.Color.Blue;
            }
        }

        private void labelControl13_Click(object sender, EventArgs e)
        {
            if (labelControl13.Appearance.ForeColor == System.Drawing.Color.Blue)
            {
                var orgiginlist = clsKTHTs;
                orgiginlist = orgiginlist.Where(m => m.Statustype == 4).ToList();
                gridControl1.DataSource = orgiginlist;
                labelControl13.Appearance.ForeColor = System.Drawing.Color.Red;
                lblwarining4.Appearance.ForeColor = System.Drawing.Color.Red;
                labelControl12.Appearance.ForeColor = System.Drawing.Color.Blue;
                lblwarning3.Appearance.ForeColor = System.Drawing.Color.Blue;
                labelControl11.Appearance.ForeColor = System.Drawing.Color.Blue;
                lblwarning2.Appearance.ForeColor = System.Drawing.Color.Blue;

            }
            else
            {
                var orgiginlist = clsKTHTs;
                gridControl1.DataSource = orgiginlist;
                labelControl13.Appearance.ForeColor = System.Drawing.Color.Blue;
                lblwarining4.Appearance.ForeColor = System.Drawing.Color.Blue;
            }
        }

        DataTable gettbChungtu;
        private void KTHT_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;

           // ThietLapControl();
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
                try
                {
                    int mact = Convert.ToInt32(row["MaCT"]);
                    if(mact== 5576)
                    {
                        int dasd = 100;
                    }
                    if (processed.Contains(mact))
                        continue;

                    processed.Add(mact);

                    string maLoai = row["MaLoai"].ToString();

                    ChungTuHD item = new ChungTuHD();
                    item.MaCT = mact;
                    if(item.MaCT== 37460)
                    {
                        int aa = 100;
                    }
                    item.SoHieu = row["SoHieu"].ToString();
                    
                    if (DateTime.TryParse(row["NgayImport"]?.ToString(), out DateTime ngayImport))
                    {
                        item.NgayImport = ngayImport;
                    }
                    else
                    {
                        item.NgayImport = DateTime.MinValue; // hoặc giá trị mặc định khác
                    }
                    if (item.SoHieu == "105" && item.NgayCT.Month==6 )
                    {
                        int aaa = 10;
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
                    if (item.SoHieu == "133" && item.Type == 1)
                    {
                        int test = 10;
                    }
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
                                if (soPS != 0)
                                {
                                    if (matkno != "169")
                                    {
                                        if (SoPS2Co != 0)
                                            tienTrcThue += soPS;
                                        else
                                        {
                                            tienTrcThue += soPS; 
                                            if(maTK!="125" && maTK != "126")
                                            item.Hangnull = 1;
                                        }
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
                                if (soPS != 0)
                                {
                                    if (matkco != "169" && (SoPS2No != 0))
                                    {
                                        tienTrcThue += soPS;
                                    }
                                    else
                                    {
                                        if (maTK == "161" || maTK == "160" || maTK == "37" || maTK=="75" || maTK=="76" || maTK=="77")
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
                catch (Exception ex)
                {
                    XtraMessageBox.Show(ex.Message + Convert.ToInt32(row["MaCT"].ToString()));
                }
               
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
            public int Hangnull { get; set; }
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
            if(e.Column.FieldName== "GhiChu")
            {
                if (e.CellValue == null)
                    return;
                int rowHandle = e.RowHandle;
                var gridView = sender as GridView; 
                var getValue = e.CellValue.ToString();
                if (getValue != null)
                {
                    if (e.CellValue.ToString() == "Hoá đơn chưa được nhập")
                    {
                        e.Appearance.ForeColor = Color.Red; // Tô màu chữ đỏ
                    }
                    //Có hàng chưa nhập mã 
                    if (e.CellValue.ToString() == "Có hàng chưa nhập mã")
                    {
                        e.Appearance.ForeColor = Color.Blue; // Tô màu chữ đỏ
                    }
                }
               
            }
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
                if ( getdt1!= getdt2 && getdt2!=0)
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