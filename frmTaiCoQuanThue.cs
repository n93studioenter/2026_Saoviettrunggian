using ClosedXML.Excel;
using DevExpress.CodeParser;
using DevExpress.Spreadsheet;
using DevExpress.Utils;
using DevExpress.XtraCharts.Design;
using DevExpress.XtraEditors;
using DevExpress.XtraMap.Native;
using DevExpress.XtraSpreadsheet;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.VariantTypes;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SaovietTax.Database;
using SaovietTax.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Windows.Media.Protection.PlayReady;
using Windows.UI.ViewManagement;
using static SaovietTax.frmKiemtrahethong;
using static SaovietTax.frmMain;
using Keys = OpenQA.Selenium.Keys;
using XmlNode = System.Xml.XmlNode;
namespace SaovietTax
{
    public partial class frmTaiCoQuanThue : DevExpress.XtraEditors.XtraForm
    {
        public frmTaiCoQuanThue()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual; // Cài đặt vị trí thủ công

        }
        public class QLHD
        {
            public string LoaiHD { get; set; }  
            public int Totals { get; set; } // Tổng số hóa đơn  
            public int Downloaded { get; set; } // Số hóa đơn đã tải về 
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Đặt vị trí ở góc trên bên trái
            this.Location = new Point(0, 0);

            // Hoặc để ở góc phải
            // this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - this.Width, 0);
        }
        public frmMain frmMain;
        public static ChromeDriver Driver { get; private set; }
        private void Testimg2(string base64data)
        {
            string base64Data = base64data;
            string outputPath = AppDomain.CurrentDomain.BaseDirectory + "output.svg";

            SvgConverter converter = new SvgConverter();
            converter.ConvertBase64ToSvg(base64Data, outputPath);

            Console.WriteLine("Tệp SVG đã được lưu tại: " + outputPath);
            RunMain();
            var readcapcha = Readcapcha();
        }
        private string Readcapcha()

        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "captcha.txt"; // Đảm bảo tệp ở cùng thư mục với chương trình

            try
            {
                // Đọc nội dung từ tệp
                string content = File.ReadAllText(filePath);
                Console.WriteLine("Nội dung của captcha.txt:");
                Console.WriteLine(content);
                return content; // Trả về nội dung đã đọc
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Tệp không tồn tại.");
                return null; // Hoặc trả về một giá trị mặc định nếu tệp không tồn tại
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message);
                return null; // Hoặc trả về một giá trị mặc định
            }
        }
        private void RunMain()
        {
            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "main.exe");

            try
            {
                // Kiểm tra xem tệp có tồn tại không
                if (!File.Exists(exePath))
                {
                    Console.WriteLine("Tệp main.exe không tồn tại.");
                    return;
                }

                // Tạo một Process để chạy tệp .exe
                Process process = new Process();
                process.StartInfo.FileName = exePath;
                process.StartInfo.UseShellExecute = false; // Không sử dụng shell để chạy
                process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory; // Đặt thư mục làm việc

                process.Start(); // Bắt đầu tiến trình
                Thread.Sleep(2000); // Đợi 2 giây 

                // Đóng tiến trình
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show("Tệp không tìm thấy: " + ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("Không có quyền truy cập: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi khác xảy ra: " + ex.Message);
            }
        }
        public class SvgConverter
        {
            public void ConvertBase64ToSvg(string base64Data, string outputPath)
            {
                // Tách phần đầu để lấy dữ liệu Base64
                var base64 = base64Data.Substring(base64Data.IndexOf(",") + 1);

                // Giải mã dữ liệu Base64
                byte[] svgBytes = Convert.FromBase64String(base64);

                // Lưu vào tệp SVG
                File.WriteAllBytes(outputPath, svgBytes);
            }
        }
        public void Xulydaura1(string tokken)
        {
            GDTClient.UpdateToken(tokken);
            using (var client = new HttpClient())
            {
                // Đặt URL
                string formattedDate1 = frmMain.dtFrom.ToString("dd/MM/yyyyTHH:mm:ss");
                string formattedDate2 = frmMain.dtTo.ToString("dd/MM/yyyyTHH:mm:ss");
                string url = $@"https://hoadondientu.gdt.gov.vn:30000/query/invoices/sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&size=50&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2}";

             
                try
                {
                 
                    string responseBody = "";
                    try
                    {
                         responseBody = Task.Run(async () => await GDTClient.GetJsonAsync(url)).Result;
                    }
                    catch(Exception ex)
                    {
                        XtraMessageBox.Show(ex.Message);
                    }
                    InvoiceRa2 rootObject;
                    try
                    {
                        rootObject = JsonConvert.DeserializeObject<InvoiceRa2>(responseBody);
                    }
                    catch (Exception ex)
                    {
                        rootObject = JsonConvert.DeserializeObject<InvoiceRa2>(responseBody);
                    }
                    if(rootObject == null)
                    {
                        return;
                    }
                    QLHD qLHD = new QLHD();
                    qLHD.LoaiHD = "Hóa đơn điện tử";
                    qLHD.Totals += rootObject.total.Value;
                    lstDaura.Add(qLHD);
                    XulyRa2ML(rootObject, tokken,4);
                    while (!string.IsNullOrEmpty(rootObject.state))
                    {
                        url = $@"https://hoadondientu.gdt.gov.vn:30000/query/invoices/sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&size=50&state={rootObject.state}&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2}";
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokken);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        try
                        {
                            responseBody = Task.Run(async () => await GDTClient.GetJsonAsync(url)).Result;
                            rootObject = JsonConvert.DeserializeObject<InvoiceRa2>(responseBody);
                            XulyRa2ML(rootObject, tokken,4);
                        }
                        catch (Exception ex)
                        {
                            // Xử lý lỗi nếu cần
                        }
                    }
                    gridControl2.DataSource = lstDaura;
                    Console.WriteLine("Response Body:");
                    Console.WriteLine(responseBody);
                   
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                }
            }
        }
        public void Xulydaura2(string tokken)
        {
            GDTClient.UpdateToken(tokken);
            using (var client = new HttpClient())
            {
                // Đặt URL
                int counthd = 0;
                string formattedDate1 = frmMain.dtFrom.ToString("dd/MM/yyyyTHH:mm:ss");
                string formattedDate2 = frmMain.dtTo.ToString("dd/MM/yyyyTHH:mm:ss");
                string url = $@"https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&size=50&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2}";
                // Thêm Bearer token vào Header
             
                int retryCount = 0;
                const int maxRetries = 3;

                try
                {
                    // Gửi yêu cầu GET

                    // Đọc nội dung phản hồi
                    string responseBody = Task.Run(async () => await GDTClient.GetJsonAsync(url)).Result;
                    InvoiceRa2 rootObject;
                    try
                    {
                        rootObject = JsonConvert.DeserializeObject<InvoiceRa2>(responseBody);
                        retryCount = maxRetries;


                    }
                    catch (Exception ex)
                    {
                        rootObject = JsonConvert.DeserializeObject<InvoiceRa2>(responseBody);
                    }
                    if (rootObject == null)
                    {
                        return;
                    }
                    QLHD qLHD = new QLHD();
                    qLHD.LoaiHD = "Máy tính tiền";
                    qLHD.Totals += rootObject.total.Value;
                    counthd += rootObject.datas.Count;
                    lstDaura.Add(qLHD);
                    XulyRa2ML(rootObject, tokken, 5);
                    while (!string.IsNullOrEmpty(rootObject.state))
                    {
                        //url = $@"https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&size=50&state={rootObject.state}&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2}";
                        url = $@"https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&size=50&state={rootObject.state}&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2}";
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokken);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        try
                        {
                            responseBody = Task.Run(async () => await GDTClient.GetJsonAsync(url)).Result;
                            rootObject = JsonConvert.DeserializeObject<InvoiceRa2>(responseBody);
                            qLHD.Totals += rootObject.total.Value;
                            counthd += rootObject.datas.Count;
                            XulyRa2ML(rootObject, tokken, 5);
                        }
                        catch (Exception ex)
                        {
                            // Xử lý lỗi nếu cần
                        }
                    }
                    gridControl2.DataSource = lstDaura;
                    Console.WriteLine("Response Body:");
                    Console.WriteLine(responseBody);
                    frmMain.tokken = tokken;
                    this.Close();
                    //Driver.Quit();
                    //this.Close();
                }
                catch (HttpRequestException e)
                {
                    XtraMessageBox.Show(e.Message);
                    // Driver.Quit();
                    //this.Close();
                }

            }
        }

        private void Xulyexel(string token, Data item, int type)
        {
            GDTClient.UpdateToken(token);
            // Tối ưu: Tính toán datetime một lần
            DateTime dtFrom = new DateTime(frmMain.dtFrom.Year, frmMain.dtFrom.Month, 1);
            DateTime dtTo = dtFrom.AddMonths(1).AddDays(-1);

            // Tối ưu: Format string một lần
            string formattedDate1 = dtFrom.ToString("dd/MM/yyyyTHH:mm:ss");
            string formattedDate2 = dtTo.ToString("dd/MM/yyyyTHH:mm:ss");

            // Tối ưu: Dùng switch case thay vì nhiều if
            string url, filename;
            switch (type)
            {
                case 1:
                    url = $@"https://hoadondientu.gdt.gov.vn:30000/query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==5%20%20%20%20&type=purchase";
                    filename = "HDDienTuDaCapMa.xlsx";
                    break;
                case 2:
                    url = $@"https://hoadondientu.gdt.gov.vn:30000/query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==6%20%20%20%20&type=purchase";
                    filename = "HDDienTuKhongMa.xlsx";
                    break;
                case 3:
                    url = $@"https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==8%20%20%20%20&type=purchase";
                    filename = "HDDienTuMayTinhTien.xlsx";
                    break;
                default:
                    return;
            }

            string directoryPath = Path.Combine(frmMain.savedPath, "HDVao", frmMain.dtFrom.Month.ToString());
            string filePath = Path.Combine(directoryPath, filename);

            // Tối ưu: Đảm bảo thư mục tồn tại trước
            Directory.CreateDirectory(directoryPath);

            // Xóa file cũ nếu tồn tại
            if (File.Exists(filePath))
                File.Delete(filePath);

            using (var client = new HttpClient())
            {
                
                try
                {
                    // Tối ưu: Bỏ Thread.Sleep không cần thiết
                    GDTClient.DownloadFile_Sync(url, filePath, token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");
                }
            }
        }
        private void Xulyexelold(string token,Data item,int type)
        {
            DateTime dtFrom = new DateTime(frmMain.dtFrom.Year, frmMain.dtFrom.Month, 1);
            DateTime dtTo = dtFrom.AddMonths(1).AddDays(-1); // Lấy ngày cuối cùng của tháng

            string formattedDate1 = dtFrom.ToString("dd/MM/yyyyTHH:mm:ss");
            string formattedDate2 = dtTo.ToString("dd/MM/yyyyTHH:mm:ss");
            //https://hoadondientu.gdt.gov.vn:30000/query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge=01/04/2025T00:00:00;tdlap=le=30/04/2025T23:59:59;ttxly==5%20%20%20%20&type=purchase

            string url = "";
            if (type == 1)
                url = @"https://hoadondientu.gdt.gov.vn:30000/query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge=" + formattedDate1 + ";tdlap=le=" + formattedDate2 + ";ttxly==5%20%20%20%20&type=purchase";
            if (type == 2)
                url = @"https://hoadondientu.gdt.gov.vn:30000/query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge=" + formattedDate1 + ";tdlap=le=" + formattedDate2 + ";ttxly==6%20%20%20%20&type=purchase";
            if (type == 3)
                url = @"https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge=" + formattedDate1 + ";tdlap=le=" + formattedDate2 + ";ttxly==8%20%20%20%20&type=purchase";
            string filename = "";
            if(type==1)
                filename= "HDDienTuDaCapMa.xlsx";
            if(type == 2)
                filename = "HDDienTuKhongMa.xlsx";
            if (type == 3)
                filename = "HDDienTuMayTinhTien.xlsx";
            string path = frmMain.savedPath + @"\" + "HDVao" + @"\" + frmMain.dtFrom.Month + @"\" + filename;
            //Xóa tat ca file excel truc khi tải
            string directoryPath = Path.Combine(frmMain.savedPath, "HDVao", frmMain.dtFrom.Month.ToString());
            string deletpath = Path.Combine(directoryPath, filename);
            // Xóa tất cả các tệp Excel trong thư mục
            if (File.Exists(deletpath))
                File.Delete(deletpath);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream")); // Định dạng nhị phân 
                try
                {
                    Thread.Sleep(300); // Đợi một chút trước khi gửi yêu cầu    
                    HttpResponseMessage response =  client.GetAsync(url).Result;
                    response.EnsureSuccessStatusCode(); // Ném ngoại lệ nếu không thành công
                    var fileBytes =  response.Content.ReadAsByteArrayAsync().Result;

                    // Lưu file ZIP
                    File.WriteAllBytes(path, fileBytes); // Sử dụng WriteAllBytes
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");
                }
            }
        }
        //private async Task Xulyexel2(string token, InvoiceRa2List item, int type)
        //{
        //    DateTime dtFrom = new DateTime(frmMain.dtFrom.Year, frmMain.dtFrom.Month, 1);
        //    string formattedDate1 = new DateTime(frmMain.dtFrom.Year, frmMain.dtFrom.Month, 1).ToString("dd/MM/yyyyTHH:mm:ss");
        //    string formattedDate2 = dtFrom.AddMonths(1).AddDays(-1).ToString("dd/MM/yyyyTHH:mm:ss");
        //    //https://hoadondientu.gdt.gov.vn:30000/query/invoices/export-excel?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge=01/06/2025T00:00:00;tdlap=le=30/06/2025T23:59:59


        //    string url = "";
        //    if (type == 1)
        //        url = @"https://hoadondientu.gdt.gov.vn:30000/query/invoices/export-excel?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge=" + formattedDate1 + ";tdlap=le=" + formattedDate2;
        //    if (type == 2)
        //        url = @"https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/export-excel?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge=" + formattedDate1 + ";tdlap=le=" + formattedDate2;

        //    string filename = "";
        //    if (type == 1)
        //        filename = "Hoadondientu.xlsx";
        //    if (type == 2)
        //        filename = "HDDienTuMayTinhTien.xlsx";
        //    if (type == 3)
        //        filename = "HDDienTuMayTinhTien.xlsx";
        //    string path = frmMain.savedPath + @"\" + "HDRa" + @"\" + frmMain.dtFrom.Month + @"\" + filename;
        //    //Xóa tat ca file excel truc khi tải
        //    string directoryPath = Path.Combine(frmMain.savedPath, "HDRa", frmMain.dtFrom.Month.ToString());
        //    string deletpath = Path.Combine(directoryPath, filename);
        //    // Xóa tất cả các tệp Excel trong thư mục
        //    if (Directory.Exists(directoryPath))
        //    {
        //        var excelFiles = Directory.GetFiles(directoryPath, "*.xlsx");
        //        foreach (var file in excelFiles)
        //        {
        //            File.Delete(file);
        //        }
        //    }

        //    using (var client = new HttpClient())
        //    {
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream")); // Định dạng nhị phân 
        //        try
        //        {
        //            Thread.Sleep(300); // Đợi một chút trước khi gửi yêu cầu    
        //            HttpResponseMessage response = await client.GetAsync(url);
        //            response.EnsureSuccessStatusCode(); // Ném ngoại lệ nếu không thành công
        //            var fileBytes = await response.Content.ReadAsByteArrayAsync();

        //            // Lưu file ZIP
        //            File.WriteAllBytes(path, fileBytes); // Sử dụng WriteAllBytes
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");
        //        }
        //    }
        //}

        private void Xulyexel2(string token, InvoiceRa2List item, int type)
        {
            GDTClient.UpdateToken(token);
            DateTime dtFrom = new DateTime(frmMain.dtFrom.Year, frmMain.dtFrom.Month, 1);
            string formattedDate1 = dtFrom.ToString("dd/MM/yyyyTHH:mm:ss");
            string formattedDate2 = dtFrom.AddMonths(1).AddDays(-1).ToString("dd/MM/yyyyTHH:mm:ss");

            string url = "";
            if (type == 1)
                url = @"https://hoadondientu.gdt.gov.vn:30000/query/invoices/export-excel?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge=" + formattedDate1 + ";tdlap=le=" + formattedDate2;
            if (type == 2)
                url = @"https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/export-excel?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge=" + formattedDate1 + ";tdlap=le=" + formattedDate2;

            string filename = "";
            if (type == 1)
                filename = "Hoadondientu.xlsx";
            if (type == 2 || type == 3)
                filename = "HDDienTuMayTinhTien.xlsx";

            string path = Path.Combine(frmMain.savedPath, "HDRa", frmMain.dtFrom.Month.ToString(), filename);

            // Xóa tất cả file excel trước khi tải
            string directoryPath = Path.Combine(frmMain.savedPath, "HDRa", frmMain.dtFrom.Month.ToString());
            string deletpath = Path.Combine(directoryPath, filename);
            if (File.Exists(deletpath))
                File.Delete(deletpath);

            using (var client = new HttpClient())
            { 
                try
                {
                    // Tối ưu: Bỏ Thread.Sleep không cần thiết
                    GDTClient.DownloadFile_Sync(url, path, token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");
                }
            }
        }
        public void Xulydauvao1(string tokken, int type)
        {
            using (var client = new HttpClient())
            {
                // Đặt URL
                string formattedDate1 = frmMain.dtFrom.ToString("dd/MM/yyyyTHH:mm:ss");
                string formattedDate2 = frmMain.dtTo.ToString("dd/MM/yyyyTHH:mm:ss");
                string url = "";
                int ttxly = 0;

                if (type == 6)
                {
                    ttxly = 5;
                }
                if (type == 8)
                {
                    ttxly = 6;
                }

                url = @"https://hoadondientu.gdt.gov.vn:30000/query/invoices/purchase?sort=tdlap:desc,khmshdon:asc,shdon:desc&size=50&search=tdlap=ge=" + formattedDate1 + ";tdlap=le=" + formattedDate2 + ";ttxly==" + ttxly;

                // Thêm Bearer token vào Header
                GDTClient.UpdateToken(tokken);  // Token chỉ set 1 lần


                try
                {
                    // Gửi yêu cầu GET
                    string responseBody = Task.Run(async () => await GDTClient.GetJsonAsync(url)).Result;

                    // Đảm bảo phản hồi thành công
                    var rootObject = JsonConvert.DeserializeObject<RootObject>(responseBody);

                   // RootObject rootObject = JsonConvert.DeserializeObject<RootObject>(responseBody);
                    if(rootObject== null )
                    {
                        return;
                    }
                    QLHD QLHD = new QLHD();
                    if(type==6)
                        QLHD.LoaiHD = "Đã cấp mã hóa đơn";
                    if (type == 8)
                        QLHD.LoaiHD = "Cục Thuế đã nhận không mã";
                    QLHD.Totals += rootObject.total;
                    XulyDataXML(rootObject, tokken, type);

                    while (!string.IsNullOrEmpty(rootObject.state))
                    {
                        url = @"https://hoadondientu.gdt.gov.vn:30000/query/invoices/purchase?sort=tdlap:desc,khmshdon:asc,shdon:desc&size=50&state=" + rootObject.state + "&search=tdlap=ge=" + formattedDate1 + ";tdlap=le=" + formattedDate2 + ";ttxly==" + ttxly;
                       
                        responseBody = Task.Run(async () => await GDTClient.GetJsonAsync(url)).Result;
                        rootObject = JsonConvert.DeserializeObject<RootObject>(responseBody);
                        QLHD.Totals += rootObject.total;
                        XulyDataXML(rootObject, tokken, type);
                    }
                    lstDauvao.Add(QLHD);  
                    Console.WriteLine("Response Body:");
                    Console.WriteLine(responseBody);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                }
            }
        }
        public async Task Xulydauvaomaytinhtien(string tokken, int type)
        {
            using (var client = new HttpClient())
            {
                // Đặt URL
                string formattedDate1 = frmMain.dtFrom.ToString("dd/MM/yyyyTHH:mm:ss");
                string formattedDate2 = frmMain.dtTo.ToString("dd/MM/yyyyTHH:mm:ss");
                string url = "";
                int ttxly = 0;
                if (type == 10)
                {
                    ttxly = 8;
                }
                url = @"https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/purchase?sort=tdlap:desc,khmshdon:asc,shdon:desc&size=50&search=tdlap=ge=" + formattedDate1 + ";tdlap=le=" + formattedDate2 + ";ttxly==" + ttxly;
                // Thêm Bearer token vào Header
                GDTClient.UpdateToken(tokken);  // Token chỉ set 1 lần

                try
                { 
                    string responseBody = Task.Run(async () => await GDTClient.GetJsonAsync(url)).Result;
                    // Đọc nội dung phản hồi
                    RootObject rootObject;
                    try
                    {
                        rootObject = JsonConvert.DeserializeObject<RootObject>(responseBody);
                    }
                    catch (Exception ex)
                    {
                        rootObject = JsonConvert.DeserializeObject<RootObject>(responseBody);
                        this.Close();
                    }
                    QLHD QLHD = new QLHD();
                    QLHD.LoaiHD = "Máy tính tiền";
                    QLHD.Totals += rootObject.total;
                    XulyDataXML(rootObject, tokken, type);
                    while (!string.IsNullOrEmpty(rootObject.state))
                    {
                        url = @"https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/purchase?sort=tdlap:desc,khmshdon:asc,shdon:desc&size=50&state=" + rootObject.state + "&search=tdlap=ge=" + formattedDate1 + ";tdlap=le=" + formattedDate2 + ";ttxly==5";
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokken);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        try
                        {
                            responseBody = Task.Run(async () => await GDTClient.GetJsonAsync(url)).Result;
                            rootObject = JsonConvert.DeserializeObject<RootObject>(responseBody);
                            XulyDataXML(rootObject, tokken, type);
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    lstDauvao.Add(QLHD);
                    QLHD QLHDs= new QLHD();
                    QLHDs.LoaiHD = "Tổng cộng";
                    QLHDs.Totals = lstDauvao.Sum(x => x.Totals);
                    lstDauvao.Add(QLHDs);
                    gridControl1.DataSource = lstDauvao;
                    Console.WriteLine("Response Body:");
                    Console.WriteLine(responseBody);
                    this.Close();
                    // Driver.Quit();
                    // this.Close();
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                }
            }
        }
        string dbPath = "";
        private DataTable ExecuteQuery(string query, params OleDbParameter[] parameters)
        {
            DataTable dataTable = new DataTable();
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);

            // Xóa phần \bin\Debug để lấy đường dẫn gốc
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
            string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");
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
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                try
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
        public void GetdetailXML2(string nbmst, string khhdon, string shdon, string tokken)
        {
            string url = $"https://hoadondientu.gdt.gov.vn:30000/query/invoices/detail?nbmst={nbmst}&khhdon={khhdon}&shdon={shdon}&khmshdon=1";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    // Gửi yêu cầu GET đồng bộ
                    Thread.Sleep(400);
                    HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();

                    // Đảm bảo phản hồi thành công
                    response.EnsureSuccessStatusCode();

                    // Đọc nội dung phản hồi đồng bộ
                    string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var rootObject = JsonConvert.DeserializeObject<Invoice>(responseBody);

                    string query = "SELECT * FROM tbimport WHERE SHDon=? AND KHHDon=? AND Mst=?";
                    // Tìm ID Cha mới nhất
                    var parameterss = new OleDbParameter[]
                    {
                new OleDbParameter("?", shdon),
                new OleDbParameter("?", khhdon),
                new OleDbParameter("?", nbmst)
                    };
                    var kq2 = ExecuteQuery(query, parameterss);
                    string getcode = "";

                    if (kq2.Rows.Count > 0)
                    {
                        // Xử lý tên
                        bool hasVattu = false;
                        foreach (var it in rootObject.Hdhhdvu)
                        {
                            if (!hasVattu)
                            {
                                // Update nội dung cho Parent
                                query = "UPDATE tbimport SET Noidung=? WHERE ID=?";
                                var parametersss = new OleDbParameter[]
                                {
                            new OleDbParameter("?", Helpers.ConvertUnicodeToVni(it.Ten)),
                            new OleDbParameter("?", kq2.Rows[0]["ID"]),
                                };
                                ExecuteQueryResult(query, parametersss);
                                hasVattu = true;
                            }

                            // Chèn chi tiết hóa đơn
                            query = @"
                    INSERT INTO tbimportdetail (ParentId, SoHieu, SoLuong, DonGia, DVT, Ten, MaCT, TKNo, TKCo, TTien)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                            var parameters = new OleDbParameter[]
                            {
                        new OleDbParameter("?", kq2.Rows[0]["ID"]),
                        new OleDbParameter("?", getcode),
                        new OleDbParameter("?", it.Sluong),
                        new OleDbParameter("?", it.Dgia),
                        new OleDbParameter("?", Helpers.ConvertUnicodeToVni(it.Dvtinh)),
                        new OleDbParameter("?", Helpers.ConvertUnicodeToVni(it.Ten)),
                        new OleDbParameter("?", ""),
                        new OleDbParameter("?", kq2.Rows[0]["TKNo"]),
                        new OleDbParameter("?", kq2.Rows[0]["TKCo"]),
                        new OleDbParameter("?", it.Thtien),
                            };
                            try
                            {
                                ExecuteQueryResult(query, parameters);
                            }
                            catch(Exception ex)
                            {
                                var aa = ex.Message;
                            }
                         
                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                }
            }
        }
        public async void GetdetailXML(string nbmst, string khhdon, string shdon, string tokken)
        {
            string url = @"https://hoadondientu.gdt.gov.vn:30000/query/invoices/detail?nbmst=" + nbmst + "&khhdon=" + khhdon + "&shdon=" + shdon + "&khmshdon=1";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    // Gửi yêu cầu GET  
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Đảm bảo phản hồi thành công
                    response.EnsureSuccessStatusCode();

                    // Đọc nội dung phản hồi
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var rootObject = JsonConvert.DeserializeObject<Invoice>(responseBody);
                    //Tìm ID Cha mới nhất
                    string query = "SELECT *   FROM tbimport where SHDon=? and KHHDon=? and Mst= ?"; // Giả sử có cột DateAdded
                    var parameterss = new OleDbParameter[]
                      {
                            new OleDbParameter("?",shdon),
                            new OleDbParameter("?",khhdon),
                            new OleDbParameter("?",nbmst)
                      };
                    var kq2 = ExecuteQuery(query, parameterss);
                    string getcode = "";
                    if (kq2.Rows.Count > 0)
                    {
                        foreach (var it in rootObject.Hdhhdvu)
                        {
                            //Xử lý tên
                            bool hasVattu = false;
                             
                            //Update nội dung cho Parent
                            query = @"Update tbimport set Noidung=? where ID=?";
                            var parameters = new OleDbParameter[]
                             {
                        new OleDbParameter("?",Helpers.ConvertUnicodeToVni(rootObject.Hdhhdvu.FirstOrDefault().Ten)),
                        new OleDbParameter("?", kq2.Rows[0]["ID"]),
                             };
                            int resl = ExecuteQueryResult(query, parameters);

                            query = @"
                        INSERT INTO tbimportdetail (ParentId, SoHieu, SoLuong, DonGia, DVT, Ten,MaCT,TKNo,TKCo,TTien)
                        VALUES (?, ?, ?, ?, ?, ?,?,?,?,?)";

                            parameters = new OleDbParameter[]
                            {
                        new OleDbParameter("?", kq2.Rows[0]["ID"]),
                        new OleDbParameter("?", getcode),
                        new OleDbParameter("?", it.Sluong),
                        new OleDbParameter("?", it.Dgia),
                        new OleDbParameter("?", Helpers.ConvertUnicodeToVni(it.Dvtinh)),
                        new OleDbParameter("?", Helpers.ConvertUnicodeToVni(it.Ten)),
                        new OleDbParameter("?", ""),
                        new OleDbParameter("?", kq2.Rows[0]["TKNo"]),
                        new OleDbParameter("?", kq2.Rows[0]["TKCo"]),
                        new OleDbParameter("?", it.Thtien),
                            };

                            resl = ExecuteQueryResult(query, parameters);
                        }

                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                }
            }

        }
        string password, connectionString;
        private int ExecuteQueryResult(string query, params OleDbParameter[] parameters)
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
        bool xlExcel2 = false;
        bool xlExcel1 = false;
        bool xlExcel3 = false;
        public  void XulyDataXML(RootObject rootObject,string tokken,int invoceType)
        {
            if (xlExcel1 == false)
            {
                xlExcel1 = true;
                Xulyexel(tokken, rootObject.datas.FirstOrDefault(), 1);
            }
            if (xlExcel2 == false)
            {
                xlExcel2 = true;
                Xulyexel(tokken, rootObject.datas.FirstOrDefault(), 2);
            }
            if (xlExcel3 == false)
            {
                xlExcel3 = true;
                Xulyexel(tokken, rootObject.datas.FirstOrDefault(), 3);
            }
            foreach (var item in rootObject.datas)
            { 
                InsertTbImport(item, invoceType);
              
                // GetdetailXML(item.nbmst, item.khhdon, item.shdon.ToString(),tokken);
            }
        }
        bool xrlExcel1 = false; bool xrlExcel2 = false;
        public void XulyRa2ML(InvoiceRa2 rootObject, string tokken,int invoceType)
        {

            foreach (var item in rootObject.datas)
            {
                try
                {
                    if (xlExcel1 == false)
                    {
                        xlExcel1 = true;
                        Xulyexel2(tokken, item, 1);
                    }
                    if (xlExcel2 == false)
                    {
                        xlExcel2 = true;
                        Xulyexel2(tokken, item, 2);
                    }
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show(ex.Message);
                }
                try
                {
                    if (item.shdon.Value == 64)
                    {
                        int a = 10;
                    }
                    InsertTbImport2(item, tokken, invoceType,item.khmshdon.ToString());
                   
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show(ex.Message);
                }
               
            }
        }
       
        private bool CheckExistKH(string mst, string ten,string cccd)
        {
            //Trường hợp ko có mst , tên và cccd
            if(string.IsNullOrEmpty(mst) && (string.IsNullOrEmpty(ten)) && string.IsNullOrEmpty(cccd))
            {
                return false;
            }
            //Nếu có Mã s61 thuế
            if (!string.IsNullOrEmpty(mst))
            {
                if (existingKhachHang.AsEnumerable().Any(row => row.Field<string>("MST") == mst))
                {
                    return true;
                } 
            }
            else
            {

                if (!string.IsNullOrEmpty(cccd))
                {
                    if (existingKhachHang.AsEnumerable().Any(row => row.Field<string>("MST") == cccd))
                    {
                        return true;
                    }
                }
                //Trường hợp tìm teo tên
                //if (existingKhachHang.AsEnumerable().Any(row => Helpers.RemoveVietnameseDiacritics(Helpers.ConvertVniToUnicode(row.Field<string>("Ten"))).ToLower() == Helpers.RemoveVietnameseDiacritics(Helpers.ConvertVniToUnicode(ten)).ToLower()))
                //{
                //    return true;
                //} 
                if (existingKhachHang.AsEnumerable().Any(row => Helpers.ConvertVniToUnicode(row.Field<string>("Ten")).ToLower() == Helpers.ConvertVniToUnicode(ten).ToLower()))
                {
                    return true;
                }
            }

            return false;
        }
        DataTable existingKhachHang = new DataTable();
        DataTable existingTbImport = new DataTable();
        DataTable existingTbChungtu = new DataTable();
        DataTable tbHoaDon { get; set; }
        DataTable tbChungtu { get; set; }
        string csohieu = "";
        static string GenerateAbbreviation(string fullName, List<string> existingNames)
        {
            // Tách tên thành từng phần
            string[] nameParts = fullName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string abbreviation = "";

            // Tạo viết tắt
            foreach (string part in nameParts)
            {
                abbreviation += part[0].ToString().ToLower();
            }

            // Kiểm tra sự tồn tại của viết tắt
            int counter = 1;
            string uniqueAbbreviation = abbreviation.ToUpper();

            while (existingNames.Contains(uniqueAbbreviation))
            {
                uniqueAbbreviation = abbreviation + "-" + counter;
                counter++;
            }

            return uniqueAbbreviation;
        }
        public void InitCustomer(int Maphanloai, string Sohieu, string Ten, string Diachi, string Mst,string cccd,string sdt)
        {
            if (string.IsNullOrEmpty(sdt))
                sdt = "xxx";
            int randNumber = 0;
            Random random = new Random();

            //Xử lý địa chỉ
            string diachiKHVni = !string.IsNullOrEmpty(Diachi) ? Helpers.ConvertUnicodeToVni(Diachi) : Helpers.ConvertUnicodeToVni("Bổ sung địa chỉ");
             
            if (string.IsNullOrEmpty(Mst))
            {
                //Truong hợp ko có mst và cccd
                if (string.IsNullOrEmpty(cccd))
                {
                    Sohieu = GenerateAbbreviation(Helpers.ConvertVniToUnicode(Ten), existingKhachHang.AsEnumerable().Select(row => row.Field<string>("SoHieu")).ToList()).ToUpper();
                    csohieu = Sohieu;
                    Mst = "00";

                    //Xử lý khi số hiệu bị trùng
                    int suffix = 1;
                    string originalSohieu = Sohieu;

                    while (existingKhachHang.AsEnumerable().Any(row => row.Field<string>("SoHieu") == Sohieu))
                    {
                        Sohieu = $"{originalSohieu}_{suffix}";
                        suffix++;
                    }
                }
                //Không có mst nhưng có cccd
                else
                {
                    Sohieu= cccd.Substring(cccd.Length - 6);
                    Mst = cccd;
                }
            }
            else
            {
                Sohieu = Helpers.GetLastFourDigits(Mst.Replace("-", ""));

                string tenKHVni = Helpers.ConvertUnicodeToVni(Ten);
                 
                //Xử lý khi số hiệu bị trùng
                if (existingKhachHang.AsEnumerable().Any(row => row.Field<string>("SoHieu") == Sohieu))
                {
                    Sohieu = "0" + Sohieu;
                }
                if (existingKhachHang.AsEnumerable().Any(row => row.Field<string>("SoHieu") == Sohieu))
                {
                    Sohieu = "00" + Sohieu;
                }
            }
            //Nếu tồn tại so hiệu r, sẽ thêm kí tự
            if (existingKhachHang.AsEnumerable().Any(row => row.Field<string>("SoHieu") == Sohieu))
            {
                Sohieu = Sohieu+"_1";
            }


            if (frmMain.mstcongty == "3501972322")
            {
                int lastid = 0;
                string qr = "SELECT MAX(MaSo) FROM KhachHang";
               var getlastid= ExecuteQuery(qr).Rows[0]["Expr1000"].ToString();
                string query = @"
        INSERT INTO KhachHang (MaSo,MaPhanLoai,SoHieu,Ten,DiaChi,MST,Tel)
        VALUES (?,?,?,?,?,?,?)";


                // Khai báo mảng tham số với đủ 10 tham số
                OleDbParameter[] parameters = new OleDbParameter[]
                {
               new OleDbParameter("?", int.Parse(getlastid)+1),
               new OleDbParameter("?", Maphanloai),
               new OleDbParameter("?", Sohieu),
               new OleDbParameter("?", Ten),
               new OleDbParameter("?", diachiKHVni),
               new OleDbParameter("?", Mst),
               new OleDbParameter("?", sdt),
                };

                // Thực thi truy vấn và lấy kết quả
                try
                {
                    int a = ExecuteQueryResult(query, parameters);
                    query = "SELECT * FROM KhachHang"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHang
                    existingKhachHang = ExecuteQuery(query);
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show(ex.Message + "    " + Ten + "   " + cccd);
                }
            }
            else
            {

                string query = @"
        INSERT INTO KhachHang (MaPhanLoai,SoHieu,Ten,DiaChi,MST,Tel)
        VALUES (?,?,?,?,?,?)";


                // Khai báo mảng tham số với đủ 10 tham số
                OleDbParameter[] parameters = new OleDbParameter[]
                {
               new OleDbParameter("?", Maphanloai),
               new OleDbParameter("?", Sohieu),
               new OleDbParameter("?", Ten),
               new OleDbParameter("?", diachiKHVni),
               new OleDbParameter("?", Mst),
               new OleDbParameter("?", sdt),
                };

                // Thực thi truy vấn và lấy kết quả
                try
                {
                    int a = ExecuteQueryResult(query, parameters);
                    query = "SELECT * FROM KhachHang"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHang
                    existingKhachHang = ExecuteQuery(query);
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show(ex.Message + "    " + Ten + "   " + cccd);
                }
            }
        }
        
        public void InsertTbImport(Data item,int invoceType)
        {
            if (item.shdon.ToString() == "41371")
            {
                int a = 10;
            }
            string directoryPath = Path.Combine(frmMain.savedPath, "HDVao", frmMain.dtFrom.Month.ToString());

            var excelFiles = Directory.EnumerateFiles(directoryPath, "*.xlsx", SearchOption.AllDirectories).ToList();
            foreach (var excelFile in excelFiles)
            {
                using (var workbook = new XLWorkbook(excelFile))

                {
                    var worksheet = workbook.Worksheet(1); // Lấy sheet đầu tiên
                    foreach (var row in worksheet.RowsUsed().Skip(3)) // Bỏ qua 6 hàng đầu tiên
                    {
                        var getSHHD = row.Cell("C").Value.ToString(); // Lấy giá trị của cột A trong hàng hiện tại
                        var getSohd = row.Cell("D").Value.ToString(); // Lấy giá trị của cột C trong hàng hiện tại 
                        var GetNLap = row.Cell("E").Value.ToString();
                        if (item.shdon.ToString() == getSohd && item.khhdon == getSHHD)
                        {
                            var getreturndate = item.ntao - DateTime.Parse(GetNLap);
                            if (getreturndate.Days == 0)
                            {
                                item.ntao = DateTime.Parse(GetNLap);
                            }
                            else
                            {
                                if (getreturndate.Days > 0)
                                {
                                    if (item.ntao != DateTime.Parse(GetNLap))
                                        item.ntao = DateTime.Parse(GetNLap);
                                }
                                else
                                {
                                    int loai = 10;
                                }
                            }
                        }
                    }
                }
            }
           
            if (existingTbImport.AsEnumerable().Any(row => row.Field<string>("SHDon").ToString() == item.shdon.ToString()  && row.Field<DateTime>("NLap").Date == item.ntao.Date))
            {
               // if (existingTbImport.AsEnumerable().Any(row => row.Field<string>("idhoadon") == item.id))
                    return;
            }
            //if (existingTbChungtu.AsEnumerable().Any(row => row.Field<string>("SoHD").ToString() == item.shdon.ToString() && row.Field<string>("KyHieu").ToString() == item.khhdon && row.Field<DateTime>("NgayPH").Month == item.ntao.Month))
            //{
            //    return;
            //}


            var getHD = (from h in tbHoaDon.AsEnumerable()
                         join c in tbChungtu.AsEnumerable()
                         on h.Field<string>("SoHD") equals c.Field<string>("SoHieu")
                         where c.Field<DateTime>("NgayCT") == item.ntao
                         //&& h["KyHieu"].ToString() == item.khhdon
                         && Helpers.RemoveLeadingZeros(h["SoHD"].ToString()).TrimEnd('.') == Helpers.RemoveLeadingZeros(item.shdon.ToString()).TrimEnd('.')
                         select h).FirstOrDefault();
             
            if (getHD != null)
            {
                //Kiem tra them makh
                //Lấy mst
                var findCus= existingKhachHang.AsEnumerable().Where(m => m.Field<int>("MaSo") == getHD.Field<int>("MaKhachHang")).FirstOrDefault();
                if(findCus != null)
                {
                    if (findCus.Field<string>("MST") == item.nbmst)
                    {
                        return;
                    }
                }
               
            }


            int type = frmMain.type;
            string query = @"
            INSERT INTO tbImport (SHDon, KHHDon, NLap, Ten, Noidung,TKNo,TKCo, TkThue, Mst, Status, Ngaytao, TongTien, Vat, SohieuTP,TPhi,TgTCThue,TgTThue,Type,InvoiceType,IsHaschild,TVat,Vat2,TVat2,Vat3,TVat3,TgTCThue1,TgTCThue2,TgTCThue3,Khmshdon,idhoadon,hdon)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";

            string newTen = Helpers.ConvertUnicodeToVni(item.nbten);
            string newNoidung = "";
            //Lấy tài khoản từ mất định
            string tkno = "";
            string tkco = "";
            string tkthue = "";
            string querykh = @" SELECT *  FROM tbDinhdanhtaikhoan"; // Sử dụng ? thay cho @mst trong OleDb
            if (CheckExistKH(item.nbmst, newTen,"") == false)
            {
                int maphanloai = 0;
                maphanloai = type == 1 ? 2 : 3; //1 là mua, 2 là bán 
                InitCustomer(maphanloai, item.khhdon, newTen, item.nbdchi, item.nbmst,"","");
            }
            //Cập nhật thông tin
            else
            {

            }
                var result = ExecuteQuery(querykh, new OleDbParameter("?", ""));
             
            if (result.Rows.Count > 0)
            {
                foreach (DataRow row in result.Rows)
                {
                    if (type == 1)
                    {
                        if (invoceType == 8)
                        {
                            if (row["KeyValue"].ToString().Contains("Ưu tiên vào"))
                            {
                                tkno = "6422";
                                tkco = row["TKCo"].ToString();
                                tkthue = row["TKThue"].ToString();
                                break;
                            }
                        }
                        else
                        {
                            if (row["KeyValue"].ToString().Contains("Ưu tiên vào"))
                            {
                                tkno = row["TKNo"].ToString();
                                tkco = row["TKCo"].ToString();
                                tkthue = row["TKThue"].ToString();
                                break;
                            }
                        }
                    }
                    if (type == 2)
                    {
                        if (row["KeyValue"].ToString().Contains("Ưu tiên ra"))
                        {
                            tkno = row["TKNo"].ToString();
                            tkco = row["TKCo"].ToString();
                            tkthue = row["TKThue"].ToString();
                            break;
                        }
                    }

                }
            }

            string tgtkcthue = "";
            if(item.tgtcthue != null)
            {
                tgtkcthue = item.tgtcthue.ToString();
            }
            else
            {
                if(item.thdon!=null && item.thdon.Contains("Hóa đơn bán hàngs"))
                { 
                    tgtkcthue = "0";
                }
                else
                {
                    if (item.tgtkcthue != null)
                    {
                        tgtkcthue = item.tgtkcthue.ToString();
                    }
                    else
                    {
                        if (item.ttttkhac.Count > 0)
                        {
                            tgtkcthue = item.ttttkhac.Count > 2 ? item.ttttkhac[0].dlieu.ToString() : "0";
                        }
                        else
                        {
                            tgtkcthue = "0";
                        }
                    }
                }
               
            }
            DateTime nl = new DateTime();
            if(item.shdon == 50819)
            {
                int a = 10;
            }
            if (invoceType == 8)
            {
                if (item.ntao.Month != DateTime.Now.Month)
                {

                    nl = item.ntao;
                }
                else
                {
                    nl = item.tdlap.AddDays(1);
                }
            }
            else
            {
                nl = item.ntao;
            }
            string getMST = "";
            if (item.nmmst != null)
            {
                getMST = item.nmmst;
            }
            else
            {
                //Lấy ma số thuế từ số hiệu khách hàng
                querykh = @" SELECT *  FROM KhachHang where Ten=?"; // Sử dụng ? thay cho @mst trong OleDb
                result = ExecuteQuery(querykh, new OleDbParameter("?", newTen));
                if (result.Rows.Count > 0)
                {
                    getMST = result.Rows[0]["SoHieu"].ToString();
                }
            }
            string dateTimeString = nl.ToString();
            DateTime utcDateTime = DateTime.Parse(dateTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);
            DateTime dateTime = DateTime.Parse(dateTimeString);
            string formattedDate = utcDateTime.ToShortDateString();

            string vat = "0";
            string vat2 = "0";  
            string vat3 = "0";
            double tvat=0, tvat2=0, tvat3 = 0;
            double TienTrcthue = 0;
            double TienThue = 0;

            if (item.tgtcthue.HasValue)
            {
                TienTrcthue = double.Parse(item.tgtcthue.Value.ToString());
            }
            else
            {
                if (item.tgtkcthue!=null)
                {
                    TienTrcthue = double.Parse(item.tgtkcthue.ToString());
                }
                else
                {
                    TienTrcthue =0;
                }
            }
                //TienTrcthue = item.t ? double.Parse(tgtkcthue) : item.tgtttbso;
            TienThue = item.tgtthue != null ? double.Parse(item.tgtthue.ToString()) : 0;
            //Xoá đi dòng thuê thừa
            if (item.thttltsuat.Count>0 && item.thttltsuat[0].tsuat.ToString().Replace("%", "") == "0")
            {
                item.thttltsuat.Remove(item.thttltsuat[0]);
            }
            double thtien1=0, thtien2=0, thtien3 = 0;
            if (item.thttltsuat.Count() > 0 )
            {
                //if(item.thttltsuat.Count>=1)
                //{
                //    vat = item.thttltsuat[0].tsuat.ToString().Replace("%", "");
                //    tvat = item.thttltsuat[0].tthue;
                //    thtien1=item.thttltsuat[0].thtien;
                //    if (vat == "KCT")
                //        vat = "0";
                //}
                //if (item.thttltsuat.Count >= 2)
                //{
                //    vat2 = item.thttltsuat[1].tsuat.ToString().Replace("%", "");
                //    tvat2 = item.thttltsuat[1].tthue;
                //    thtien2 = item.thttltsuat[1].thtien;

                //}
                //if (item.thttltsuat.Count >= 3)
                //{
                //    vat3 = item.thttltsuat[2].tsuat.ToString().Replace("%", "");
                //    tvat3 = item.thttltsuat[2].tthue;
                //    thtien3 = item.thttltsuat[2].thtien;
                //}

                foreach (var it in item.thttltsuat)
                {
                    var index = item.thttltsuat.IndexOf(it);
                    string currentVat = item.thttltsuat[index].tsuat.ToString().Replace("%", "");
                    if (currentVat == "0" || vat == "KCT" || it.thtien==0 || currentVat== "KKKNT")
                        continue;
                    if (vat == "0")
                    {
                        vat = currentVat;
                        tvat = item.thttltsuat[index].tthue;
                        thtien1 = item.thttltsuat[index].thtien;
                    }
                    else
                    {
                        if (vat2 == "0")
                        {
                            vat2 = currentVat;
                            tvat2 = item.thttltsuat[index].tthue;
                            thtien2 = item.thttltsuat[index].thtien;

                        }
                        else
                        {
                            if (vat3 == "0")
                            {
                                vat3 = currentVat;
                                tvat3 = item.thttltsuat[index].tthue;
                                thtien3 = item.thttltsuat[index].thtien;

                            }
                        }
                    }
                }
            }
            else
            {
                if (TienThue != 0 && TienTrcthue != 0)
                {
                    vat = Math.Round((TienThue / TienTrcthue) * 100).ToString();
                }
                else
                    vat = "0";
            }
            if (vat == "KCT" || vat == "KKKNT")
            {
                vat = "0";
            }
            //Nếu tiền thuế =0 thì lấy tiền thuế gốc
            if (tvat == 0)
                tvat = TienThue;
            //Kiểm tra tien phi
            double tienphi = 0;
            if (item.tgtphi != null && item.thttlphi.Count > 0)
            {
                foreach (var tp in item.thttlphi)
                {
                    if(tp.tphi != null)
                    tienphi += (double)tp.tphi;
                }
            }
            else
            {
                if (item.tgtphi!=null && double.Parse(item.tgtphi.ToString()) != 0)
                {
                    tienphi += (double)item.tgtphi;
                }
            }
            //Xử lý nghị quyết
            if (item.gchu != null && item.gchu.Contains("Đã giảm"))
            {
                TienTrcthue = item.tgtttbso;
            }
            OleDbParameter[] parameters = new OleDbParameter[]
                    {
                new OleDbParameter("?", item.shdon),
                new OleDbParameter("?", item.khhdon),
                new OleDbParameter("?", formattedDate),
                new OleDbParameter("?", newTen),
                new OleDbParameter("?", newNoidung),
                new OleDbParameter("?",tkno),
                new OleDbParameter("?",tkco),
                new OleDbParameter("?",tkthue),
                new OleDbParameter("?", item.nbmst),
                new OleDbParameter("?", "0"),
                new OleDbParameter("?", DateTime.Now.ToShortDateString()),
                new OleDbParameter("?", item.tgtttbso),
                new OleDbParameter("?",vat),
                new OleDbParameter("?",""),
                new OleDbParameter("?",tienphi.ToString()),
                new OleDbParameter("?",Math.Round(TienTrcthue)),
                new OleDbParameter("?",Math.Round(TienThue)),
                //  new OleDbParameter("?",item.thttltsuat[0].thtien.ToString()),
                //new OleDbParameter("?",item.thttltsuat[0].tthue.ToString()),
                new OleDbParameter("?", type),
                new OleDbParameter("?", invoceType),
                new OleDbParameter("?","1"),
                new OleDbParameter("?",tvat),
                new OleDbParameter("?",vat2), 
                new OleDbParameter("?",tvat2),
                new OleDbParameter("?",vat3),
                new OleDbParameter("?",tvat3),
                new OleDbParameter("?",thtien1),
                new OleDbParameter("?",thtien2),
                new OleDbParameter("?",thtien3),
                new OleDbParameter("?",item.khmshdon.ToString()),
                new OleDbParameter("?",item.id.ToString()),
                new OleDbParameter("?",item.hdon),
                    };

            try
            {
                int a = ExecuteQueryResult(query, parameters);
                stt += 1;
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message + "    " + item.shdon);
            }

        }
        private void ReadExcelFile(string filePath)
        {
            using (var workbook = new XLWorkbook(filePath))

            {
                var worksheet = workbook.Worksheet(1); // Lấy sheet đầu tiên
                foreach (var row in worksheet.RowsUsed().Skip(3)) // Bỏ qua 6 hàng đầu tiên
                {
                    var getSHHD = row.Cell("C").Value.ToString(); // Lấy giá trị của cột A trong hàng hiện tại
                    var getSohd = row.Cell("D").Value.ToString(); // Lấy giá trị của cột C trong hàng hiện tại
                    var getMST = row.Cell("F").Value.ToString(); // Lấy giá trị của cột B trong hàng hiện tại
                    var GetNLap = row.Cell("E").Value.ToString();
                   
                }
            }
        }
        public void InsertTbImport2(InvoiceRa2List item, string tokken, int invoceType,string Khmshdon)
        {
            if (item.shdon.ToString() == "2368")
            {
                int a = 10;
            }
            string directoryPath = Path.Combine(frmMain.savedPath, "HDRa", frmMain.dtFrom.Month.ToString());

            var excelFiles = Directory.EnumerateFiles(directoryPath, "*.xlsx", SearchOption.AllDirectories).ToList();
            foreach (var excelFile in excelFiles)
            {
                using (var workbook = new XLWorkbook(excelFile))

                {
                    var worksheet = workbook.Worksheet(1); // Lấy sheet đầu tiên
                    foreach (var row in worksheet.RowsUsed().Skip(3)) // Bỏ qua 6 hàng đầu tiên
                    {
                        var getSHHD = row.Cell("C").Value.ToString(); // Lấy giá trị của cột A trong hàng hiện tại
                        var getSohd = row.Cell("D").Value.ToString(); // Lấy giá trị của cột C trong hàng hiện tại 
                        var GetNLap = row.Cell("E").Value.ToString();
                        if(item.shdon.ToString()==getSohd && item.khhdon==getSHHD)
                        {
                            var getreturndate = DateTime.Parse(DateTime.Parse(item.ntao).ToShortDateString()) - DateTime.Parse(GetNLap);
                            if (getreturndate.Days == 0)
                            {
                                item.ntao = GetNLap;
                            }
                            else
                            {
                                if (getreturndate.Days <= 1 && getreturndate.Days>0)
                                {
                                    if (item.ntao != GetNLap)
                                        item.ntao = GetNLap;
                                }
                                else
                                {
                                    int loai = 10;
                                }
                            }
                        }
                    }
                }
            }
            //Kiểm tra tồn tại trước khi thêm mới
            if (existingTbImport.AsEnumerable().Any(row => row.Field<string>("SHDon").ToString() == item.shdon.ToString() && row.Field<string>("KHHDon").ToString() == item.khhdon.ToString() && row.Field<DateTime>("NLap").Date == DateTime.Parse(item.ntao).Date))
            {
                if (item.shdon.ToString() == "147")
                {
                    int a = 10;
                }
                //if (existingTbImport.AsEnumerable().Any(row => row.Field<string>("idhoadon") == item.id))
                return;
            }

            var getHD = (from h in tbHoaDon.AsEnumerable()
                         join c in tbChungtu.AsEnumerable()
                         on h.Field<string>("SoHD") equals c.Field<string>("SoHieu")
                         where c.Field<DateTime>("NgayCT") == DateTime.Parse(item.ntao) 
                         && Helpers.RemoveLeadingZeros(h["SoHD"].ToString()).TrimEnd('.') == Helpers.RemoveLeadingZeros(item.shdon.ToString()).TrimEnd('.')
                         select h).FirstOrDefault();
            if (getHD != null)
            {
                if (item.shdon.ToString() == "147")
                {
                    int a = 10;
                }
                return;
            }
            //if (existingTbChungtu.AsEnumerable().Any(row => row.Field<string>("SoHD").ToString() == item.shdon.ToString() && row.Field<string>("KyHieu").ToString() == item.khhdon && row.Field<DateTime>("NgayPH").Month == DateTime.Parse(item.ntao).Month))
            //{
            //    return;
            //}
            int type = frmMain.type;
            string query = @"
            INSERT INTO tbImport (SHDon, KHHDon, NLap, Ten, Noidung,TKNo,TKCo, TkThue, Mst, Status, Ngaytao, TongTien, Vat, SohieuTP,TPhi,TgTCThue,TgTThue,Type,InvoiceType,IsHaschild,TVat,Vat2,TVat2,Vat3,TVat3,TgTCThue1,TgTCThue2,TgTCThue3,Khmshdon,idhoadon,hdon)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";

            string newTen = "";
            if (!string.IsNullOrEmpty(item.nmten))
            {
                newTen = Helpers.ConvertUnicodeToVni(item.nmten);
            }
            else
            {
                if (!string.IsNullOrEmpty(item.nmtnmua))
                {
                    newTen = Helpers.ConvertUnicodeToVni(item.nmtnmua);
                }
                else
                {
                    if (!string.IsNullOrEmpty(item.nmhvtnmhang))
                    {
                        newTen = Helpers.ConvertUnicodeToVni(item.nmhvtnmhang);
                    }
                }
            }
            string getMST = "";
            //Insert khach hàng
            if (CheckExistKH(item.nmmst, newTen,item.nmcccd) == false)
            {
                if(!Helpers.ConvertVniToUnicode(newTen).Contains("không lấy hóa đơn"))
                {
                    int maphanloai = 0;
                    maphanloai = type == 1 ? 2 : 3; //1 là mua, 2 là bán 
                    InitCustomer(maphanloai, item.khhdon, newTen, item.nmdchi, item.nmmst, item.nmcccd, item.nmsdthoai);
                }
                //Lấy kh là khách lẻ
                else
                {
                    newTen = "Khaùch leû khoâng laáy hoaù ñôn";
                    getMST = "KL";
                }
            }
            //Cập nhật lại CCCD
            else
            {
                if (!string.IsNullOrEmpty(item.nmcccd))
                {
                    var getKH = existingKhachHang.AsEnumerable().Where(row => Helpers.RemoveVietnameseDiacritics(Helpers.ConvertVniToUnicode(row.Field<string>("Ten"))).ToLower() == Helpers.RemoveVietnameseDiacritics(Helpers.ConvertVniToUnicode(newTen)).ToLower()).FirstOrDefault();
                    string mst = getKH["MST"].ToString();
                    string tel = item.nmsdthoai!=null? item.nmsdthoai:"...";
                    string Maso = getKH["MaSo"].ToString();
                    if (mst == "00" || mst == "...")
                    {

                        string Sohieu = item.nmcccd.Substring(item.nmcccd.Length - 6);
                        string  querykhmst = "UPDATE KhachHang SET MST=?,SoHieu=?,Tel=?,MaPhanLoai=? where MaSo=? ";
                        var parametersss = new OleDbParameter[]
                        {
                            new OleDbParameter("?", item.nmcccd),
                            new OleDbParameter("?", Sohieu),
                            new OleDbParameter("?", tel),
                               new OleDbParameter("?","3"),
                            new OleDbParameter("?", Maso),
                        };
                        try
                        {
                            int a = ExecuteQueryResult(querykhmst, parametersss);
                        }
                        catch(Exception ex)
                        {
                           // XtraMessageBox.Show(ex.Message + "    " + item.shdon);
                        }
                    }
                }
                else
                {
                    var getKH = existingKhachHang.AsEnumerable().Where(row => Helpers.RemoveVietnameseDiacritics(Helpers.ConvertVniToUnicode(row.Field<string>("Ten"))).ToLower() == Helpers.RemoveVietnameseDiacritics(Helpers.ConvertVniToUnicode(newTen)).ToLower()).FirstOrDefault();
                    if (getKH != null)
                    {
                        getMST = getKH["SoHieu"].ToString();
                    }
                }

            }
                string newNoidung = Helpers.ConvertUnicodeToVni("");
            //Lấy tài khoản từ mất định
            string tkno = "";
            string tkco = "";
            string tkthue = "";
            string querykh = @" SELECT *  FROM tbDinhdanhtaikhoan"; // Sử dụng ? thay cho @mst trong OleDb

            var result = ExecuteQuery(querykh, new OleDbParameter("?", ""));
            if (result.Rows.Count > 0)
            {
                foreach (DataRow row in result.Rows)
                {
                    if (type == 1)
                    {
                        if (row["KeyValue"].ToString().Contains("Ưu tiên vào"))
                        {
                            tkno = row["TKNo"].ToString();
                            tkco = row["TKCo"].ToString();
                            tkthue = row["TKThue"].ToString();
                            break;
                        }
                    }
                    if (type == 2)
                    {
                        if (row["KeyValue"].ToString().Contains("Ưu tiên ra"))
                        {
                            tkno = row["TKNo"].ToString();
                            tkco = row["TKCo"].ToString();
                            tkthue = row["TKThue"].ToString();
                            break;
                        }
                    }

                }
            }

            string tgtkcthue = "";
            string dateTimeString = item.ntao;
            DateTime utcDateTime = DateTime.Parse(dateTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);
            DateTime dateTime = DateTime.Parse(dateTimeString);
            string formattedDate = utcDateTime.ToShortDateString();
            
            if (item.nmmst != null)
            {
                getMST = item.nmmst;
            }
            else
            {
                if (!string.IsNullOrEmpty(item.nmcccd))
                {
                    getMST = item.nmcccd;
                }
                else
                {
                    querykh = @" SELECT *  FROM KhachHang where Ten=?"; // Sử dụng ? thay cho @mst trong OleDb
                    result = ExecuteQuery(querykh, new OleDbParameter("?", newTen));
                    if (result.Rows.Count > 0)
                    {
                        getMST = result.Rows[0]["SoHieu"].ToString();
                    }
                }
                    //Lấy ma số thuế từ số hiệu khách hàng
                  
            }

            string vat = "0";
            string vat2 = "0";
            string vat3 = "0";
            double tvat = 0, tvat2 = 0, tvat3 = 0;
            double TienTrcthue = 0;
            double TienThue = 0;
            if (item.tgtcthue.HasValue)
            {
                TienTrcthue = double.Parse(item.tgtcthue.Value.ToString());
            }
            else
            {
                TienTrcthue = 0;
            }
            if (item.tgtthue != null)
            {
                TienThue=double.Parse(item.tgtthue.ToString());
            }
            else
            {
                TienThue = 0;
            }
            //Xoá dòng thuế thừa
            if(item.thttltsuat.Count>0 &&  item.thttltsuat[0].tsuat.ToString().Replace("%", "") == "0")
            {
                item.thttltsuat.Remove(item.thttltsuat[0]); 
            }
            //TienThue = item.tgtthue != null ? double.Parse(item.tgtthue.ToString()) : 0;
            double thtien1 = 0, thtien2 = 0, thtien3 = 0;
            if (item.thttltsuat.Count() > 0)
            {
                if (item.thttltsuat.Count == 1)
                {
                    vat = item.thttltsuat[0].tsuat.ToString().Replace("%", "");
                    tvat = item.thttltsuat[0].tthue.Value;
                     thtien1 = item.thttltsuat[0].thtien ?? 0;
                    double tthue = item.tgtthue.Value;
                    tvat = tthue;
                }
                else
                {

                    vat = item.thttltsuat[0].tsuat.ToString().Replace("%", "");
                    tvat = item.thttltsuat[0].tthue.Value;
                    thtien1 = item.thttltsuat[0].thtien ?? 0;
                    double tthue = item.tgtthue.Value;
                    if (item.thttltsuat.Count >= 2)
                    {
                        vat2 = item.thttltsuat[1].tsuat.ToString().Replace("%", "");
                        tvat2 = item.thttltsuat[1].tthue.Value;
                        thtien2 = item.thttltsuat[1].thtien ?? 0;
                    }
                    if (item.thttltsuat.Count >= 3)
                    {
                        vat3 = item.thttltsuat[2].tsuat.ToString().Replace("%", "");
                        tvat3 = item.thttltsuat[2].tthue.Value;
                        thtien3 = item.thttltsuat[2].thtien ?? 0;
                    }
                }
               
            }
            else
            {
                if (TienThue != 0 && TienTrcthue != 0)
                {
                    vat = Math.Round((TienThue / TienTrcthue) * 100).ToString();
                }
                else
                    vat = "0";
            }
            if (string.IsNullOrEmpty(newTen) && string.IsNullOrEmpty(getMST))
            {
                getMST = "...";
                newTen = Helpers.ConvertUnicodeToVni("Khách lẻ");

            }

            if (vat == "KCT" || vat== "KKKNT")
            {
                vat = "0";
            }
            int vats = 0;
             if(tvat==0)
                tvat = TienThue;

            OleDbParameter[] parameters = new OleDbParameter[]
            {
                new OleDbParameter("?", item.shdon),
                new OleDbParameter("?", item.khhdon),
                new OleDbParameter("?", formattedDate),
                new OleDbParameter("?", newTen),
                new OleDbParameter("?", newNoidung),
                new OleDbParameter("?",tkno),
                new OleDbParameter("?",tkco),
                new OleDbParameter("?",tkthue),
                new OleDbParameter("?",getMST),
                new OleDbParameter("?", "0"),
                new OleDbParameter("?", DateTime.Now.ToShortDateString()),
                new OleDbParameter("?", item.tgtttbso),
                new OleDbParameter("?", vat),
                new OleDbParameter("?",""),
                new OleDbParameter("?", "0"),
                new OleDbParameter("?", Math.Round(TienTrcthue)),
                new OleDbParameter("?", Math.Round(TienThue)),
                new OleDbParameter("?", type),
                new OleDbParameter("?", invoceType),
                new OleDbParameter("?", "1"),
                new OleDbParameter("?", tvat),
                new OleDbParameter("?", vat2),
                new OleDbParameter("?", tvat2),
                new OleDbParameter("?", vat3),
                new OleDbParameter("?", tvat3),
                new OleDbParameter("?", thtien1),
                new OleDbParameter("?", thtien2),
                new OleDbParameter("?", thtien3),
                new OleDbParameter("?", Khmshdon),
                new OleDbParameter("?", item.id),
                new OleDbParameter("?", item.hdon),
            };

            try
            {
                int a = ExecuteQueryResult(query, parameters);
                
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message + "    " + item.shdon);
            }

        }
        int stt = 1;
        private List<QLHD> lstDauvao = new List<QLHD>();
        private List<QLHD> lstDaura = new List<QLHD>();

        private void UpdateDataexcel()
        {
           
        }
        private void loadbak()
        {
            if (frmMain.type == 1)
            {
                xtraTabControl1.SelectedTabPageIndex = 0;
            }
            else
            {
                xtraTabControl1.SelectedTabPageIndex = 1;
            }
            var query = @"SELECT * FROM PhanLoaiVattu ORDER BY TenPhanLoai";
            var dt = ExecuteQuery(query, null);
            query = "SELECT * FROM KhachHang"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHang
            existingKhachHang = ExecuteQuery(query);
            //existingTbImport
            query = "SELECT * FROM tbimport"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHang
            existingTbImport = ExecuteQuery(query);

            //Danh sách chứng từ
            query = "SELECT * FROM HoaDon"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHang
            tbHoaDon = ExecuteQuery(query);
            query = "SELECT * FROM ChungTu"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHang
            tbChungtu = ExecuteQuery(query);
            // XulyFileExcel();

            // return;
            Driver = null;
            if (Driver == null)
            {
                var options = new ChromeOptions();
                // Tắt các cảnh báo bảo mật (Safe Browsing) 

                // Tắt Safe Browsing và các tính năng bảo mật can thiệp
                options.AddArgument("--disable-features=SafeBrowsing,DownloadBubble,DownloadNotification");
                options.AddArgument("--safebrowsing-disable-extension-blacklist");
                options.AddArgument("--safebrowsing-disable-download-protection");

                options.AddUserProfilePreference("download.prompt_for_download", false);
                options.AddUserProfilePreference("safebrowsing.enabled", false);
                options.AddUserProfilePreference("safebrowsing.disable_download_protection", true);
                // Tối ưu hóa trình duyệt
                //options.AddArgument("--headless");  // Chạy không giao diện
                //options.AddArgument("--disable-gpu");
                //options.AddArgument("--no-sandbox");

                options.AddArguments(
                  "--disable-notifications",   // Tắt thông báo
                   "--start-maximized",         // Khởi động ở chế độ tối đa
                  "--disable-extensions",      // Tắt các tiện ích mở rộng
                   "--disable-infobars");       // Tắt thông báo thông tin
                //
                string downloadPath = "";
                if (frmMain.type == 1)
                {
                    downloadPath = frmMain.savedPath + "\\HDVao";
                }
                if (frmMain.type == 2)
                {
                    downloadPath = frmMain.savedPath + "\\HDRa";
                }
                options.AddUserProfilePreference("download.default_directory", downloadPath);
                options.AddUserProfilePreference("download.prompt_for_download", false);
                options.AddUserProfilePreference("disable-popup-blocking", "true");
                options.AddUserProfilePreference("safebrowsing.disable_download_protection", true);
                options.AddUserProfilePreference("safebrowsing.enabled", false); // Tắt Safe Browsing hoàn toàn
                var driverPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                ChromeDriverService chromeService = ChromeDriverService.CreateDefaultService(driverPath);
                chromeService.HideCommandPromptWindow = true; // Để ẩn cửa sổ CMD của driver


                Driver = new ChromeDriver(chromeService, options);
                //
                try
                {
                    Driver.Navigate().GoToUrl("https://hoadondientu.gdt.gov.vn");

                    IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
                    js.ExecuteScript("window.scrollTo(0, 0);");
                    Thread.Sleep(1000);
                    var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(100));
                    var closeButton = wait.Until(driver => driver.FindElement(By.XPath("//span[@class='ant-modal-close-x']")));
                    closeButton.Click();
                    //
                    var loginButton = wait.Until(driver => driver.FindElement(By.XPath("//div[@class='ant-col home-header-menu-item']/span[text()='Đăng nhập']")));
                    loginButton.Click();
                    var usernameField = Driver.FindElement(By.Id("username"));
                    var passwordField = Driver.FindElement(By.Id("password"));
                    string username = frmMain.username;
                    string password = frmMain.pasword;
                    usernameField.SendKeys(username);
                    passwordField.SendKeys(password);


                    // In ra src của thẻ img
                    try
                    {

                        var imgElement = Driver.FindElements(By.XPath("//img[contains(@src, 'data:image')]"));
                        while (imgElement.Count < 2)
                        {
                            imgElement = Driver.FindElements(By.XPath("//img[contains(@src, 'data:image')]"));
                        }
                        string src = imgElement[1].GetAttribute("src");
                        while (string.IsNullOrEmpty(src))
                        {
                            src = imgElement[1].GetAttribute("src");
                        }
                        new Actions(Driver)
   .KeyDown(Keys.Tab).KeyUp(Keys.Tab)  // Tab lần 1
   .Pause(TimeSpan.FromMilliseconds(100))  // Đợi ngắn
   .KeyDown(Keys.Tab).KeyUp(Keys.Tab)  // Tab lần 2
   .Perform();

                        //Tìm capcha

                        var cvalue = Driver.FindElements(By.Id("cvalue"));

                        Testimg2(src);
                        Thread.Sleep(300);
                        string recap = Readcapcha();
                        cvalue[1].SendKeys(recap);
                        Thread.Sleep(300);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    loginButton = Driver.FindElement(By.XPath("//button[contains(span/text(), 'Đăng nhập')]"));
                    loginButton.Click();
                    wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(200));
                    //chờ khi nao dang nhap xong
                    //                var button = wait.Until(d =>
                    //d.FindElement(By.CssSelector("button.ant-btn-icon-only i[aria-label='icon: user']"))
                    // .FindElement(By.XPath("./parent::button")));
                    wait.Until(d =>
                    d.FindElements(By.XPath("//div[contains(@class,'home-header-menu-item')]//span[text()='Đăng nhập']")).Count == 0);
                    //DoTask = int.Parse(comboBoxEdit1.SelectedItem.ToString());
                    //Endtask = int.Parse(comboBoxEdit2.SelectedItem.ToString()); 

                    var cookies = Driver.Manage().Cookies.AllCookies.Where(m => m.Name == "jwt");

                    foreach (var cookie in cookies)
                    {
                        Console.WriteLine($"Name: {cookie.Name}, Value: {cookie.Value}");
                        //Lưu tokken
                        query = "UPDATE tbRegister SET tokken=? ";
                        var parametersss = new OleDbParameter[]
                        {
                            new OleDbParameter("?", cookie.Value),
                        };
                        int a = ExecuteQueryResult(query, parametersss);
                        frmMain.tokken = cookie.Value;

                        try
                        {
                            Driver.Quit();
                        }
                        catch (Exception ex)
                        {

                        }
                        if (frmMain.type == 1)
                        {
                            Xulydauvao1(cookie.Value, 6);
                            Xulydauvao1(cookie.Value, 8);
                            //Cap nhatdata excel
                            UpdateDataexcel();
                            Xulydauvaomaytinhtien(cookie.Value, 10);
                        }
                        if (frmMain.type == 2)
                        {
                            Xulydaura1(cookie.Value);
                            Thread.Sleep(1000);
                            Xulydaura2(cookie.Value);
                        }

                    }

                }
                catch (Exception ex)
                {
                    // Driver.Close();
                    //this.Close();
                    MessageBox.Show($"Lỗi: {ex.Message}");
                }
            }

            // Thêm cột với tiêu đề 
            XtraMessageBox.Show("Đã tải xong dữ liệu hóa đơn, vui lòng kiểm tra lại dữ liệu trong phần mềm kế toán.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (Driver != null)
            {
                try
                {
                    Driver.Quit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi khi đóng trình duyệt: " + ex.Message);
                }
            }
            Driver.Quit();
        }
        private static readonly HttpClient clients = new HttpClient(new HttpClientHandler()
        {
            UseProxy = false,
            PreAuthenticate = true,
            UseDefaultCredentials = false
        })
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        // ======== 2. Class GDTClient static (đặt ở file riêng, ví dụ GDTClient.cs) ========
        public static class GDTClient
        {
            private static readonly HttpClient _client;

            static GDTClient()
            {
                var handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    UseProxy = false
                };

                _client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(40) };

                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.ConnectionClose = false; // Keep-Alive
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            }

            public static void UpdateToken(string token)
                => _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            public static async Task<string> GetJsonAsync(string url, int maxRetries = 3)
            {
                for (int i = 0; i <= maxRetries; i++)
                {
                    try
                    {
                        var sw = Stopwatch.StartNew();
                        var response = await _client.GetAsync(url);
                        string json = await response.Content.ReadAsStringAsync();
                        sw.Stop();

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"GDT OK → {sw.ElapsedMilliseconds}ms");
                            return json;
                        }

                        // 401 → token sai → không retry
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                            throw new UnauthorizedAccessException("Token hết hạn hoặc sai!");

                        // Các lỗi khác (500, 503…) → retry
                        Console.WriteLine($"GDT lỗi {response.StatusCode} → retry {i + 1}/{maxRetries}");
                    }
                    catch (TaskCanceledException) when (i < maxRetries)
                    {
                        Console.WriteLine($"Timeout → retry {i + 1}/{maxRetries}");
                    }
                    catch (Exception ex) when (i < maxRetries)
                    {
                        Console.WriteLine($"Lỗi mạng → retry {i + 1}/{maxRetries}: {ex.Message}");
                    }

                    if (i < maxRetries)
                        await Task.Delay(500 * (i + 1)); // backoff: 500ms, 1000ms, 1500ms
                }

                throw new Exception("Gọi API GDT thất bại sau nhiều lần thử");
            }
            public static void DownloadFile_Sync(string url, string savePath, string token = null)
            {
                if (!string.IsNullOrEmpty(token))
                    UpdateToken(token);

                Task.Run(() =>
                {
                    var sw = Stopwatch.StartNew();
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, url);
                        request.Headers.Accept.Clear();
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                        var response = _client.SendAsync(request).GetAwaiter().GetResult();
                        response.EnsureSuccessStatusCode();

                        using (var stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                        using (var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920))
                        {
                            stream.CopyTo(fs);
                        }

                        sw.Stop();
                      //  XtraMessageBox.Show($"Tải thành công!\n{Path.GetFileName(savePath)}\nThời gian: {sw.ElapsedMilliseconds} ms", "Thành công");
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show($"Lỗi tải file:\n{ex.Message}");
                    }
                });
            }
        }
        private void frmTaiCoQuanThue_Load(object sender, EventArgs e)
        {
            var query = @"SELECT * FROM PhanLoaiVattu ORDER BY TenPhanLoai";
            var dt = ExecuteQuery(query, null);
            query = "SELECT * FROM KhachHang"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHang
            existingKhachHang = frmMain.tbKhachhang;
            //existingTbImport
            query = "SELECT * FROM tbimport"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHang
            existingTbImport = ExecuteQuery(query, null);

            //Danh sách chứng từ
            query = "SELECT * FROM HoaDon"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHaxng
            tbHoaDon = frmMain.existingTbHoadon;
            query = "SELECT * FROM ChungTu"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHang
            tbChungtu = frmMain.existingTbChungtu;
             

            string cookie = frmMain.tokken;
              query = "UPDATE tbRegister SET tokken=? ";
            var parametersss = new OleDbParameter[]
            {
                            new OleDbParameter("?", cookie),
            };
            int a = ExecuteQueryResult(query, parametersss);
            if (frmMain.type == 1)
            {
                Xulydauvao1(cookie, 6);
                Xulydauvao1(cookie, 8);
                //Cap nhatdata excel
                UpdateDataexcel();
                Xulydauvaomaytinhtien(cookie, 10);
            }
            if (frmMain.type == 2)
            {
                Xulydaura1(cookie);
                Thread.Sleep(1000);
                Xulydaura2(cookie);
            }
           
        }

        private void spreadsheetControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue ==13)
            {
                //var cell = spreadsheetControl1.ActiveCell;
                //string value = spreadsheetControl1.ActiveWorksheet.GetCellValue(0,0).ToString();

                //// In ra console hoặc hiển thị thông báo
                //Console.WriteLine($"Enter tại {cell.GetReferenceA1()}, giá trị: {value}");

                //// (Tùy chọn) Ngăn di chuyển xuống ô khác
                //e.Handled = true;
                e.Handled = true;
                frmTaikhoan frmTaikhoan = new frmTaikhoan();
                frmTaikhoan.Show();

            }
        }
    }
}