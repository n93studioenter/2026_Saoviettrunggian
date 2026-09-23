using DevExpress.XtraEditors;
using DevExpress.XtraMap.Native;
using DevExpress.XtraReports.Design;
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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SaovietTax.frmMain;

namespace SaovietTax
{
    public partial class frmTaihoadonvb : DevExpress.XtraEditors.XtraForm
    {
        public frmTaihoadonvb()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;

            // Đặt kích thước form (tuỳ chỉnh) 

            // Đặt form ở góc phải dưới
            this.Location = new Point(
                Screen.PrimaryScreen.WorkingArea.Right - this.Width,
                Screen.PrimaryScreen.WorkingArea.Bottom - this.Height
            );
        }
       public string hdlink { get; set; }
        public string GetInvoiceUrl(int invoiceType, string nbmst, string khhdon, string shdon, string Khmshdon)
        {
            string url;

            if (invoiceType == 4 || invoiceType == 6 || invoiceType == 8)
            {
                url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-xml?nbmst={nbmst}&khhdon={khhdon}&shdon={shdon}&khmshdon={Khmshdon}";
            }
            else if (invoiceType == 5 || invoiceType == 10)
            {
                url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-xml?nbmst={nbmst}&khhdon={khhdon}&shdon={shdon}&khmshdon={Khmshdon}";
            }
            else
            {
                throw new ArgumentException("Loại hóa đơn không hợp lệ.");
            }

            return url;
        }
        bool needlogin=true;

        string connectionString;
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
        string mytokken = "";
        public string tokken { get; set; } = "";
        private async Task Getttoken()
        {
            string querykh = @" SELECT *  FROM tbRegister"; // Sử dụng ? thay cho @mst trong OleDb

            var tbRegister = ExecuteQuery(querykh, new OleDbParameter("?", ""));
            string gettimeTokken = tbRegister.AsEnumerable().FirstOrDefault()["TimeTokken"].ToString();
            if (!string.IsNullOrEmpty(gettimeTokken))
            {
                var timpsan = DateTime.Now - DateTime.Parse(gettimeTokken);
                if (timpsan.TotalMinutes <= 1)
                {
                    needlogin = false;
                    mytokken = tbRegister.AsEnumerable().FirstOrDefault().Field<string>("tokken");
                }
            }

            // Kiểm tra nếu cần đăng nhập
            if (needlogin)
            {
                int maxAttempts = 3;
                int attemptCount = 0;
                bool loginSuccess = false;

                while (attemptCount < maxAttempts && !loginSuccess)
                {
                    attemptCount++;
                    progressPanel1.Caption = $"Đang đăng nhập lần {attemptCount}/{maxAttempts}...";
                    Application.DoEvents();

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
                                progressPanel1.Caption = $"Lần {attemptCount}: Không lấy được captcha";
                                continue; // Thử lại lần tiếp theo
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
                            Application.DoEvents();

                            SvgCaptchaSolver solver = new SvgCaptchaSolver();
                            string cvalue = solver.SolveCaptcha(svgPath);

                            if (string.IsNullOrEmpty(cvalue))
                            {
                                progressPanel1.Caption = $"Lần {attemptCount}: Không giải được captcha";
                                continue; // Thử lại lần tiếp theo
                            }

                            // ================= STEP 3: LOGIN ================= 
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

                            // GỬI REQUEST LOGIN
                            var loginRes = await client.PostAsync(loginUrl, content);

                            if (loginRes.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                string err = await loginRes.Content.ReadAsStringAsync();
                                progressPanel1.Caption = $"Lần {attemptCount}: Đăng nhập thất bại (401): {err}";

                                // Nếu là lỗi 401, thường là do captcha sai, nên thử lại
                                continue;
                            }

                            if (!loginRes.IsSuccessStatusCode)
                            {
                                string err = await loginRes.Content.ReadAsStringAsync();
                                progressPanel1.Caption = $"Lần {attemptCount}: Đăng nhập thất bại: {loginRes.StatusCode} - {err}";
                                continue; // Thử lại lần tiếp theo
                            }

                            string loginBody = await loginRes.Content.ReadAsStringAsync();
                            var tokenData = JsonConvert.DeserializeObject<TokenResponse>(loginBody);
                            this.tokken = tokenData.token;
                            mytokken = this.tokken;

                            // Đăng nhập thành công
                            loginSuccess = true;
                            progressPanel1.Caption = "Đăng nhập thành công!";

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
                                // Không làm gián đoạn luồng chính nếu check profile thất bại
                                Console.WriteLine("Không kiểm tra được ngày hết hạn: " + ex.Message);
                            }

                            // ================= SAVE TOKEN TIME =================
                            ExecuteQueryResult(
                                "UPDATE tbRegister SET TimeTokken=?",
                                new OleDbParameter[]
                                {
                            new OleDbParameter("?", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                                }
                            );

                            Application.DoEvents();
                        }
                    }
                    catch (Exception ex)
                    {
                        progressPanel1.Caption = $"Lần {attemptCount}: Lỗi đăng nhập: {ex.Message}";

                        // Nếu là lần thử cuối cùng, hiển thị thông báo lỗi
                        if (attemptCount == maxAttempts)
                        {
                            XtraMessageBox.Show(
                                $"Đăng nhập thất bại sau {maxAttempts} lần thử.\nLỗi cuối cùng: {ex.Message}",
                                "Lỗi đăng nhập",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                        }

                        // Đợi 1-2 giây trước khi thử lại (tránh spam request)
                        if (attemptCount < maxAttempts)
                        {
                            await Task.Delay(2000);
                        }
                    }
                }

                // Xử lý khi đăng nhập thất bại sau tất cả các lần thử
                if (!loginSuccess)
                {
                    progressPanel1.Caption = $"Đăng nhập thất bại sau {maxAttempts} lần thử";
                    // Có thể hiển thị thông báo hoặc thực hiện hành động khác ở đây
                }
            }
        }
        string dbPath = "";
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
        private void LoadData()
        {
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
 
        }
        string mstcongty = "";
        string savedPath = "";
        string user = "";
        string password = "";
        string namtc = "";
        private async void frmTaihoadonvb_Load(object sender, EventArgs e)
        {
            progressPanel1.Caption = "Đang tải hoá đơn...";
           simpleButton1.PerformClick();
        }

        private void frmTaihoadonvb_FormClosed(object sender, FormClosedEventArgs e)
        {
           
        }

        private async void simpleButton1_Click(object sender, EventArgs e)
        { 
            LoadData();

            string query = "SELECT * FROM License";

            // Tạo mảng tham số với giá trị cho câu lệnh SQL

            var kq = ExecuteQuery(query, null);
            mstcongty = kq.Rows[0]["MaSoThue"].ToString();
            namtc = kq.Rows[0]["NamTC"].ToString();
            query = "SELECT * FROM tbRegister";
            // Tạo mảng tham số với giá trị cho câu lệnh SQL

            kq = ExecuteQuery(query, null);
            savedPath = kq.Rows[0]["Hoadonpath"].ToString();
            user = kq.Rows[0]["Username"].ToString();
            password = kq.Rows[0]["Password"].ToString();

             await Getttoken();

            string qr = "SELECT * FROM HoaDon";
            DataTable tbHoadon = ExecuteQuery(qr, null);
            string filePath = Path.Combine(savedPath, "invoice.txt");
            //filePath = hdlink;
            if (File.Exists(filePath) )
            {
                try
                {
                    string content = File.ReadAllText(filePath).Trim();
                    string mst = "";
                    var getsplit = content.Split('_');
                    string sokh = "1";
                    string sohd = Helpers.RemoveLeadingZeros(getsplit[2]);
                    string khhd = getsplit[3];
                    if (content.Contains("HDVao"))
                    {
                        mst = mstcongty;
                    }
                    else
                    {
                        mst = getsplit[0];
                        var findmauhd = tbHoadon.AsEnumerable().Where(m => m.Field<string>("KyHieu") == khhd && Helpers.RemoveLeadingZeros(m.Field<string>("SoHD")) == sohd).FirstOrDefault();
                        if (findmauhd != null)
                        {
                            double tt = findmauhd.Field<double>("ThanhTien");
                            if (tt == 0)
                            {
                                sokh = "2";
                            }
                        }

                    }

                    //
                    string pathravao = content.Contains("HDVao")  ? "HDVao" : "HDRa";
                    string fn = $"{mst}_{sohd}_{khhd}.zip";
                    var getthangsplit = getsplit[0].Split('\\');
                    string thang = getthangsplit[getthangsplit.Length - 2];
                    int tuthang = int.Parse(thang);
                    string yearpath = $"HD{namtc}";
                    string path = Path.Combine(savedPath, yearpath, pathravao, tuthang.ToString(), fn);
                    string url2 = GetInvoiceUrl(4, mst, khhd, sohd, sokh);
                    string url1 = GetInvoiceUrl(5, mst, khhd, sohd, sokh);
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", mytokken);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                        // Danh sách các URL cần thực hiện
                        var urls = new string[] { url1, url2 }; // Thay thế url1 và url2 bằng các URL thực tế

                        foreach (var url in urls)
                        {
                            try
                            {
                                string rootPath = Path.GetDirectoryName(path);
                                string getnamefile = Path.GetFileNameWithoutExtension(path);
                                string directoryPath = Path.Combine(rootPath, "Giainen_" + getnamefile);
                                string targetFilePath = Path.Combine(rootPath, getnamefile );
                                if (File.Exists(targetFilePath))
                                {
                                    this.Close();
                                }
                                HttpResponseMessage response = await client.GetAsync(url);
                                response.EnsureSuccessStatusCode(); // Ném ngoại lệ nếu không thành công

                                // Đọc nội dung phản hồi dưới dạng byte
                                var fileBytes = await response.Content.ReadAsByteArrayAsync();

                                // Lưu file ZIP bằng FileStream
                                using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                                {
                                    await fileStream.WriteAsync(fileBytes, 0, fileBytes.Length);
                                }

                                Console.WriteLine($"File ZIP đã được lưu tại: {path}");

                                try
                                {

                                    ZipFile.ExtractToDirectory(path, directoryPath);
                                    var files = Directory.GetFiles(directoryPath, "invoice.html", SearchOption.AllDirectories);


                                    if (files.Length > 0)
                                    {
                                        File.Move(files.FirstOrDefault(), targetFilePath);
                                        File.Delete(path);
                                        Directory.Delete(directoryPath, true);
                                        Console.WriteLine($"File đã được xử lý từ URL: {url}");
                                        this.Close();
                                        break;

                                    }
                                    else
                                    {
                                        Console.WriteLine("Không tìm thấy file invoice.html để xử lý.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Lỗi khi giải nén hoặc xử lý file: {ex.Message}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Đã xảy ra lỗi với URL {url}: {ex.Message}");

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Đã xảy ra lỗi khi đọc file: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("File status.txt không tồn tại tại: " + filePath);
            }

            this.Close();
        }

        private void progressPanel1_Click(object sender, EventArgs e)
        {

        }
    }
}