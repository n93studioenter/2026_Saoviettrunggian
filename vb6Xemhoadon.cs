using DevExpress.XtraEditors;
using DevExpress.XtraWaitForm;
using Newtonsoft.Json;
using SaovietTax.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
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
using static Google.Protobuf.Reflection.FieldOptions.Types;
using static SaovietTax.APIInvoice;
using static SaovietTax.frmMain;

namespace SaovietTax
{
    public partial class vb6Xemhoadon : DevExpress.XtraEditors.XtraForm
    {
        public vb6Xemhoadon()
        {
            InitializeComponent(); 
        }
        public string connectionString { get; set; }
        string dbPath, dbName, pathThumuc;
        public string txtusername, txtpassword;
        private void vb6Xemhoadon_Load(object sender, EventArgs e)
        {
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


            // Đọc toàn bộ nội dung tệp
            string password = "1@35^7*9)1";
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";


            string query = "SELECT * FROM tbRegister";
            var kq = ExecuteQuery(query, null);
            savedPath = kq.Rows[0]["Hoadonpath"].ToString();
            txtusername = kq.Rows[0]["Username"].ToString();
            txtpassword = kq.Rows[0]["Password"].ToString();


            string filePath = Path.Combine(rootDirectory, "Hoadon", "invoice.txt");
            string _content = File.ReadAllText(filePath);
            string origincontent = _content;
            if (File.Exists(_content))
            {
                frmXemhoadonInvoicse frmXemhoadonInvoicse = new frmXemhoadonInvoicse();
                frmXemhoadonInvoicse.path= _content;
                frmXemhoadonInvoicse.invoicse = this;
                frmXemhoadonInvoicse.ShowDialog();
            }
            else
            {
                string directory = Path.GetDirectoryName(_content);
                string fileName = Path.GetFileName(_content);

                int index = fileName.IndexOf('_');
                if (index >= 0)
                {
                    fileName = fileName.Substring(index + 1);
                }

                _content = Path.Combine(directory, fileName);
                if (File.Exists(_content))
                {
                    frmXemhoadonInvoicse frmXemhoadonInvoicse = new frmXemhoadonInvoicse();
                    frmXemhoadonInvoicse.path = _content;
                    frmXemhoadonInvoicse.invoicse = this;
                    frmXemhoadonInvoicse.ShowDialog();
                }
                else
                {
                    var _contentpdf = Path.ChangeExtension(origincontent, ".pdf");
                    if (File.Exists(_contentpdf))
                    { 
                        frmXemhoadonInvoicse frmXemhoadonInvoicse = new frmXemhoadonInvoicse();
                        frmXemhoadonInvoicse.path = _content;
                        frmXemhoadonInvoicse.invoicse = this;
                        frmXemhoadonInvoicse.ShowDialog();
                    }
                    //Nếu ko có hoá đơn gốc thì tải hoá đơn
                    else
                    {
                        string fileNames = System.IO.Path.GetFileNameWithoutExtension(origincontent);
                        // => "20260917_3500370861_24283_C26TTD"

                        // Pattern: ngày_<số hóa đơn>_<mã>_<ký hiệu>
                        var match = Regex.Match(fileNames, @"^(\d+)_(\d+)_(\d+)_([A-Za-z0-9]+)$");

                        if (match.Success)
                        {
                            string ngay = match.Groups[1].Value;        // 20260917
                            string ma = match.Groups[2].Value;    // 3500370861
                            string soHoaDon = match.Groups[3].Value;          // 24283
                            string kyHieu = match.Groups[4].Value;      // C26TTD
                            int loai = 0;
                            if (origincontent.Contains("HDVao"))
                                loai = 1;
                            else
                                loai = 2;
                            try
                            {
                                TaihoadonDbclick(soHoaDon, ma, kyHieu, "1", loai, ngay);
                            }
                            catch(Exception ex)
                            {
                                TaihoadonDbclick(soHoaDon, ma, kyHieu, "2", loai, ngay);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Tên file không đúng định dạng!");
                        }
                    }
                }
                    
               
            }
        }
        string savedPath { get; set; }  
        private async Task TaihoadonDbclick(string SHDon,string Mst,string KHHDon,string Khmshdon,int typs,string ngaytao)
        {

            System.Windows.Forms.WebBrowser webBrowser1 = new System.Windows.Forms.WebBrowser
            {
                Dock = DockStyle.Fill
            };

            // Lấy MST
            string mst = "";
            if (typs == 1)
            {
                mst = Mst;
            }
            else
            {
                string qr = "SELECT * FROM tbRegister";
                var kq2 = ExecuteQuery(qr, null);
                mst = kq2.Rows[0]["Username"].ToString();
            }

            if (mst == "8046549703")
                mst = "048172000197";

            string pathravao = typs==1 ? "HDVao" : "HDRa";
            string fn = $"{ngaytao}_{mst}_{SHDon}_{Khmshdon}.html";
            DateTime ngay = DateTime.ParseExact(ngaytao, "yyyyMMdd", null);

            int tuthang = ngay.Month;

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

            // Chưa có file → tải mới
            await GetloginToken(); // đảm bảo có token

            int type = 0;
            if (typs == 1) type = 4;
            else
                type = 5;

            string url = GetInvoiceUrl(type, mst, KHHDon, SHDon, Khmshdon);
            //https://hoadondientu.gdt.gov.vn/api/query/invoices/export-xml?nbmst=0100520429-001&khhdon=C26TBR&shdon=42416&khmshdon=1

            string filename = $"{ngaytao:yyyyMMdd}_{mst}_{SHDon}_{KHHDon}.zip";
            string path = Path.Combine(savedPath, yearPath, pathravao, tuthang.ToString(), filename);

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(60);

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.tokken);
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
                       // frmCongTrinh.Show(); 
                        frmCongTrinh.BringToFront();
                        frmCongTrinh.Activate();
                        frmCongTrinh.Controls.Add(webBrowser1);
                        this.Hide();
                        webBrowser1.Navigate("file:///" + targetFilePath.Replace("\\", "/"));
                        frmXemhoadonInvoicse frmXemhoadonInvoicse = new frmXemhoadonInvoicse();
                        frmXemhoadonInvoicse.path = targetFilePath;
                        frmXemhoadonInvoicse.invoicse = this;
                        frmXemhoadonInvoicse.ShowDialog();
                        this.Close();
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
        private void Testimg2(string base64data)
        {
            string base64Data = base64data;
            string outputPath = AppDomain.CurrentDomain.BaseDirectory + "output.svg";

            SvgConverter converter = new SvgConverter();
            converter.ConvertBase64ToSvg(base64Data, outputPath);

            Console.WriteLine("Tệp SVG đã được lưu tại: " + outputPath);
            RunMain();
            //var readcapcha = Readcapcha();
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
                System.Diagnostics.Process process = new System.Diagnostics.Process();
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
        private async Task GetloginToken()
        {
            try
            {
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
                    client.Timeout = TimeSpan.FromSeconds(30);

                    // ===== User-Agent mới hơn (nên cập nhật thường xuyên) =====
                    string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36";

                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", ua);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "vi-VN,vi;q=0.9,en-US;q=0.8,en;q=0.7");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://hoadondientu.gdt.gov.vn");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://hoadondientu.gdt.gov.vn/");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\"Google Chrome\";v=\"140\", \"Chromium\";v=\"140\", \"Not=A?Brand\";v=\"24\"");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
                    client.DefaultRequestHeaders.ExpectContinue = false;

                    // Helper thêm header đặc thù GDT
                    void AddGdtHeaders(HttpRequestMessage req, string endPoint = "/", string action = "")
                    {
                        req.Headers.TryAddWithoutValidation("Request-Id", Guid.NewGuid().ToString());
                        req.Headers.TryAddWithoutValidation("End-Point", endPoint);
                        req.Headers.TryAddWithoutValidation("Action", string.IsNullOrEmpty(action) ? "" : Uri.EscapeDataString(action));
                    }

                    string capUrl = "https://hoadondientu.gdt.gov.vn/api/captcha";
                    HttpResponseMessage resCap;

                    using (var capReq = new HttpRequestMessage(HttpMethod.Get, capUrl))
                    {
                        AddGdtHeaders(capReq, "/");
                        resCap = await client.SendAsync(capReq);
                    }

                    if (!resCap.IsSuccessStatusCode)
                    {
                        string errBody = await resCap.Content.ReadAsStringAsync();
                        // Kiểm tra có phải HTML/WAF không
                       
                        Thread.Sleep(2000);
                        return;
                    }

                    string capBody = await resCap.Content.ReadAsStringAsync();
                    MyJson capJson = JsonConvert.DeserializeObject<MyJson>(capBody);

                    if (string.IsNullOrEmpty(capJson?.Key) || string.IsNullOrEmpty(capJson?.Content))
                    {
                        Thread.Sleep(2000);
                        return;
                    }

                    string svgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "captcha.svg");
                    File.WriteAllText(svgPath, capJson.Content);

                    // XSRF-TOKEN (nếu có)
                    var cookies = cookieContainer.GetCookies(new Uri("https://hoadondientu.gdt.gov.vn"));
                    string xsrfToken = cookies["XSRF-TOKEN"]?.Value;
                    if (!string.IsNullOrEmpty(xsrfToken))
                    {
                        client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("X-XSRF-TOKEN", xsrfToken);
                    }



                    string svgCaptcha = File.ReadAllText(svgPath);
                    string base64 = SvgToBase64(svgCaptcha);
                    Testimg2(base64);
                    //Thread.Sleep(200);
                    string cvalue = Readcapcha();
                    // SvgCaptchaSolver solver = new SvgCaptchaSolver();
                    //tring cvalue = solver.SolveCaptcha(svgPath)?.Trim() ?? "";

                    if (string.IsNullOrEmpty(cvalue) || cvalue.Length < 4) // thường captcha GDT 4-6 ký tự
                    {
                        Thread.Sleep(1500);
                        return;
                    }

                  
                    string loginUrl = "https://hoadondientu.gdt.gov.vn/api/security-taxpayer/authenticate";

                    var payload = new
                    {
                        username = txtusername.Trim(),
                        password = txtpassword,
                        cvalue = cvalue,
                        ckey = capJson.Key
                    };

                    string jsonPayload = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    HttpResponseMessage loginRes;
                    using (var loginReq = new HttpRequestMessage(HttpMethod.Post, loginUrl))
                    {
                        AddGdtHeaders(loginReq, "/", ""); // End-Point = /
                        loginReq.Content = content;
                        loginRes = await client.SendAsync(loginReq);
                    }

                    string loginBody = await loginRes.Content.ReadAsStringAsync();

                    // Kiểm tra HTML/WAF
                    if (loginBody.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase))
                    {
                        Thread.Sleep(3000);
                        return;
                    }

                    if (loginRes.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        // Thử parse message
                        try
                        {
                            var errObj = JsonConvert.DeserializeObject<LoginResponse2>(loginBody);
                            if (errObj?.message?.Contains("Tên đăng nhập hoặc mật khẩu") == true)
                            {
                                XtraMessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!");
                                return;
                            }
                        }
                        catch { }

                        if (maxlogin < 3)
                        {
                            Thread.Sleep(2000);
                            maxlogin++;
                            return;
                        }
                        else
                        {
                            XtraMessageBox.Show("Đăng nhập thất bại quá nhiều lần (401)");
                            return;
                        }
                    }

                    if (!loginRes.IsSuccessStatusCode)
                    {
                        
                        return;
                    }

                    // Lấy token
                    var tokenData = JsonConvert.DeserializeObject<TokenResponse>(loginBody);
                    if (string.IsNullOrEmpty(tokenData?.token))
                    {
                        // Fallback tìm token kiểu JWT
                        var match = System.Text.RegularExpressions.Regex.Match(loginBody, @"""(eyJ[A-Za-z0-9_\-]+\.[A-Za-z0-9_\-]+\.[A-Za-z0-9_\-]+)""");
                        if (match.Success)
                            this.tokken = match.Groups[1].Value;
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        this.tokken = tokenData.token;
                    }

                    // ================= STEP 4: PROFILE =================

                    // Lưu thời gian token
                    ExecuteQueryResult(
                        "UPDATE tbRegister SET TimeTokken=?",
                        new OleDbParameter[] { new OleDbParameter("?", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) }
                    );

                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi đăng nhập hệ thống thuế: " + ex.Message);
            }
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
        string tokken { get; set; }
        int maxlogin = 0;
        private void vb6Xemhoadon_FormClosing(object sender, FormClosingEventArgs e)
        {
           
        }

        private void vb6Xemhoadon_FormClosed(object sender, FormClosedEventArgs e)
        {
           
        }
    }
}