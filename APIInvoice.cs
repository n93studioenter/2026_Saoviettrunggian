using DevExpress.XtraEditors;
using Microsoft.Playwright;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SaovietTax.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tensorflow;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Cookie = System.Net.Cookie; 

namespace SaovietTax
{
    public partial class APIInvoice : Form
    {
        public APIInvoice()
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
        public class LoginResponse2
        {
            public string timestamp { get; set; }
            public string message { get; set; }
            public string details { get; set; }
            public string path { get; set; }
        }
        public class LoginResponse
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string refresh_token { get; set; }
            public int expires_in { get; set; }
            public string scope { get; set; }
            public long iat { get; set; }
            public string invoice_cluster { get; set; }
            public int type { get; set; }
            public string jti { get; set; }
        }
        public class UseCookie
        {
             public string __cf_bm { get; set; }
            public string JSESSIONID { get; set; }
            public string access_token { get; set; }
            public string session_token { get; set; }
        }
        public static LoginResponse loginResponse { get; set; } = new LoginResponse();
        public static UseCookie useCookie { get; set; } = new UseCookie();
        public static ChromeDriver Driver { get; private set; }
        private async Task Login()
        {
            string qrq = "SELECT * FROM tbInvoiceInfo";
            var dtInvoiceInfo = ExecuteQuery(qrq, null);
            var row = dtInvoiceInfo.Rows[0];

            string username = row["Username"]?.ToString();
            string password = row["Password"]?.ToString();

            var url = "https://vinvoice.viettel.vn/api/auth/login";

            using (HttpClientHandler handler = new HttpClientHandler())
            {
                // Tùy chọn: tự động xử lý cookie
                handler.UseCookies = true;
                handler.CookieContainer = new CookieContainer();

                using (HttpClient client = new HttpClient(handler))
                {
                    // giống Postman
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                                    var json = $@"{{
                        ""username"": ""{username}"",
                        ""password"": ""{password}"",
                        ""rememberMe"": false,
                        ""captcha"": """"
                    }}";
                    progressPanel1.Caption = "Thực hiện đăng nhập...";
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(url, content);
                    if(response.StatusCode== System.Net.HttpStatusCode.Unauthorized)
                    {
                        progressPanel1.Caption = "Thông tin đăng nhập không chính xác...";
                        Application.DoEvents();
                        Thread.Sleep(3000);
                        Application.Exit();
                    }
                    // *** CÁCH LẤY COOKIE ***
                    // Lấy tất cả cookies từ response
                    var cookies = handler.CookieContainer.GetCookies(new Uri(url));

                    foreach (System.Net.Cookie cookie in cookies)
                    {
                        //MessageBox.Show($"Cookie: {cookie.Name} = {cookie.Value}");

                        // Nếu bạn muốn lấy riêng cookie __cf_bm
                        if (cookie.Name == "__cf_bm")
                        {
                            string cf_bm_value = cookie.Value;
                            useCookie.__cf_bm = cf_bm_value;
                        }
                        if (cookie.Name == "JSESSIONID")
                        {
                            string JSESSIONID_value = cookie.Value;
                            useCookie.JSESSIONID = JSESSIONID_value;
                        }
                        if (cookie.Name == "access_token")
                        {
                            string access_token_value = cookie.Value;
                            useCookie.access_token = access_token_value;
                        }
                        if (cookie.Name == "session_token")
                        {
                            string session_token_value = cookie.Value;
                            useCookie.session_token = session_token_value;
                        }
                    }

                    var result = await response.Content.ReadAsStringAsync();
                    loginResponse = JsonConvert.DeserializeObject<LoginResponse>(result);
                    if (loginResponse != null)
                    {
                        progressPanel1.Caption = "Đăng nhập hệ thống thành công...";
                        Application.DoEvents();
                        //TestChukyso(); 
                        switch (_content)
                        {
                            case "1":
                                btnGetTemplate.PerformClick();
                                break;
                            case string s when s.StartsWith("KT"):
                                progressPanel1.Caption = "Đang đồng bộ trạng thái hoá đơn...";
                                Application.DoEvents();
                                //Lấy danh sahch hóa đơn đang ở trạng thái nháp 
                                var sqlcheck = @"
SELECT *
FROM HoaDon
WHERE IdNhap IS NOT NULL
  AND IdNhap <> ''"; 
                                var checkupdate = ExecuteQuery(sqlcheck, null);
                                foreach(DataRow r in checkupdate.Rows)
                                {
                                    string idnhap = r["IdNhap"].ToString();
                                    string api = $"https://vinvoice.viettel.vn/api/cluster5/services/einvoiceapplication/api/invoice/search-invoice-by-id/{idnhap}/draft";

                                    var rsp = await client.GetAsync(api);
                                    var rs = await rsp.Content.ReadAsStringAsync();
                                    string StatusPH = "";
                                    if (rsp.IsSuccessStatusCode)
                                    {
                                        StatusPH = "0";
                                    }
                                    else
                                    {
                                        StatusPH = "1";
                                    }
                                    var updateQr = @"UPDATE HoaDon  SET StatusPH = ?  WHERE MaSo =?";
                                    var updateParameters = new OleDbParameter[]
                                    {
        new OleDbParameter("?", StatusPH), // Cập nhật giá trị StatusPH     
        new OleDbParameter("?", r["MaSo"]),

                                    };
                                    var updateRowsAffected = ExecuteQueryResult(updateQr, updateParameters);
                                }
                               this.Close();    
                                break;
                            case string s when s.StartsWith("PH_"):

                                int idinvoice = int.Parse(s.Split('_')[1]);

                                getcardid = idinvoice.ToString();

                                simpleButton5.PerformClick();

                                break;

                            case string s when s.StartsWith("Huy_"):
                                 idinvoice = int.Parse(s.Split('_')[1]);
                                var payload = new[]
                                {
                                    new
                                    {
                                        id = idinvoice
                                    }
                                };
                                 json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);

                                var saveUrl = "https://vinvoice.viettel.vn/api/cluster5/services/einvoiceapplication/api/invoice/delete";

                                var request = new HttpRequestMessage(HttpMethod.Delete, saveUrl)
                                {
                                    Content = new StringContent(
                                        json,
                                        Encoding.UTF8,
                                        "application/json"
                                    )
                                };

                                 response = await client.SendAsync(request);

                                 result = await response.Content.ReadAsStringAsync();
                                string query = "SELECT * FROM tbRegister";
                                string pathluu = "";
                                var kq = ExecuteQuery(query, null);
                                try
                                {
                                    if (kq.Rows.Count > 0)
                                    {
                                        pathluu = kq.Rows[0]["Hoadonpath"].ToString();
                                        pathluu = Directory.GetParent(pathluu).FullName;
                                        pathluu = Path.Combine(pathluu, $"HoaDon/HdNhap");
                                        // ✅ kiểm tra tồn tại
                                       string finalpath= Path.Combine(pathluu, $"{idinvoice}.pdf");
                                        try
                                        {
                                            if (File.Exists(finalpath))
                                            {
                                                File.Delete(finalpath);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            XtraMessageBox.Show(ex.Message);
                                        }
                                        //Lấy nam taichinh
                                    }
                                }
                                catch (Exception ex)
                                {
                                    XtraMessageBox.Show(ex.Message);
                                }
                                Application.Exit();
                                break;
                            
                            default:

                                simpleButton3.PerformClick();

                                break;
                        }
                    }
                }
            }
        }
        private void LoadCertificateInfo(string certString)
        {

           // string certString = "Viettel-CA SHA2,MIIE7jCCA9agAwIBAgIQVAT//rcDP7MktyjTwuvAmjANBgkqhkiG9w0BAQsFADA/MRgwFgYDVQQDDA9WaWV0dGVsLUNBIFNIQTIxFjAUBgNVBAoMDVZpZXR0ZWwgR3JvdXAxCzAJBgNVBAYTAlZOMB4XDTI1MDkwNjAyNDYwMFoXDTI2MDcwNjAyNDYwMFowgYkxCzAJBgNVBAYTAlZOMR0wGwYDVQQHDBRCw4AgUuG7ikEgVsWoTkcgVMOAVTE7MDkGA1UEAwwyQ8OUTkcgVFkgVE5ISCBDxqAgxJBJ4buGTiBM4bqgTkggVFLhu4xORyBUw41OIFZJTkExHjAcBgoJkiaJk/IsZAEBDA5NU1Q6MzUwMjQyMjU5MzCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAOOWrlHk9p0CLoXvXbHjsB2Fo5v1QJDPYulfSLjr1sh01lGv+dZ+lPVbNtrzcY9to5VmTbuzdtZH+Qt5kJ259CrbBfG5O/Jy605/CtN6ar9k/yAbakA5WYz00bsY678sT5+NoTK8hDRlTKoUSGbK//PIOtjoyNypAVQ6WA7pY8UwwyIyI6DlQMyTYsHbLJkf0+UD40kK7/Mt35haT+ig4drNvCKGPJqYKglGiDWHi6scjckFVv5Y8k2CKpX4f68Ivr4rEJDFCIKkBJTzGIirPCGVkr0x/e2wMLytm0ImXfsu9JIs0OOThY3tsXGk8AaNnaYCT744iGlCttEQU3XlNDMCAwEAAaOCAZkwggGVMAwGA1UdEwEB/wQCMAAwHwYDVR0jBBgwFoAUQ9U1AIu+B7rjTeYeJFlWiFu+zEoweQYIKwYBBQUHAQEEbTBrMEIGCCsGAQUFBzAChjZodHRwOi8vdmlldHRlbC1jYS52bi9kb3dubG9hZHMvc3ViL1ZpZXR0ZWwtQ0FfU0hBMi5jcnQwJQYIKwYBBQUHMAGGGWh0dHA6Ly9vY3NwLnZpZXR0ZWwtY2Eudm4wMwYDVR0lBCwwKgYIKwYBBQUHAwIGCCsGAQUFBwMEBgorBgEEAYI3CgMMBggrBgEFBQcDJDCBhAYDVR0fBH0wezB5oDKgMIYuaHR0cDovL2NybC52aWV0dGVsLWNhLnZuL1ZpZXR0ZWwtQ0EtU0hBMi0yLmNybKJDpEEwPzEYMBYGA1UEAwwPVmlldHRlbC1DQSBTSEEyMRYwFAYDVQQKDA1WaWV0dGVsIEdyb3VwMQswCQYDVQQGEwJWTjAdBgNVHQ4EFgQUzivJFzXn5ur7hOeWAr8vhYlIXbcwDgYDVR0PAQH/BAQDAgXgMA0GCSqGSIb3DQEBCwUAA4IBAQBmOe9tZxCRB029WW/HY8OFzjo16HLUp+jsXsC+2LcNBCPqBlWLbLxGZgDlLyL3siOhg1iNPOI3U6fM89TVNk2oGFj5GbazlECgT+x2ml+cUsyd4W4af7lCfuaO93ToXxPosAz/5bJ43KHZQOd6hWskeBlsFhxsP99Ue4oOV56aHm+ks9FgR6Do/3sNZdTLrqtE7gIo1T09bmb2oYC8Pl58212fHcXZEl90OJq4dzkxwPSvbtK1jHvUwWMwTTS2L7e1CxbXeLDoV3FGMFiV0vsISEX+hzuUirkJE+iTJMC33wBn6f78mKgYg1VJt7Pbojars3PWJzghCSF6zs/JeFFn";
            // tách phần base64
            string base64 = certString.Split(',')[1];

            // decode
            byte[] bytes = Convert.FromBase64String(base64);

            // load cert
            var cert = new X509Certificate2(bytes);

            // đọc thông tin
            Console.WriteLine(cert.Subject);   // Tên công ty + MST
            Console.WriteLine(cert.Issuer);    // CA (Viettel-CA)
            Console.WriteLine(cert.NotBefore); // Ngày bắt đầu
            Console.WriteLine(cert.NotAfter);  // Ngày hết hạn
        }
        private void TestChukyso2()
        {
            // 1. Mở store
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            var cert = store.Certificates
                .Cast<X509Certificate2>()
                .Where(c => c.HasPrivateKey)
                .Where(c => c.Issuer.Contains("Viettel"))
                .FirstOrDefault();

            if (cert == null)
            {
                Console.WriteLine("❌ Không tìm thấy certificate Viettel");
                return;
            }

            // 2. Lấy certString
            byte[] certBytes = cert.Export(X509ContentType.Cert);
            string certString = Convert.ToBase64String(certBytes);

            // 3. Dữ liệu cần ký
            string dataToSign = "591468950|Hello Viettel Sign";
            byte[] data = Encoding.UTF8.GetBytes(dataToSign);

            byte[] signature;
            byte[] hash;

            // 4. Ký
            using (RSA rsa = cert.GetRSAPrivateKey())
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    hash = sha256.ComputeHash(data);
                }
                signature = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }

            string signatureBase64 = Convert.ToBase64String(signature);
            string hashBase64 = Convert.ToBase64String(hash);

            // 5. Tạo XML chữ ký TỐI GIẢN (chỉ có cái cần thiết)
            string xmlSignature = $@"<Signature>
  <SignatureValue>{signatureBase64}</SignatureValue>
  <DigestValue>{hashBase64}</DigestValue>
  <X509Certificate>{certString}</X509Certificate>
</Signature>";

            // 6. Mã hóa Base64 toàn bộ XML
            string hashValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlSignature));

            Console.WriteLine("✔ hashValue đã tạo:");
            Console.WriteLine(hashValue);

            // 7. In kết quả API
            Console.WriteLine("\n========== KẾT QUẢ GỬI API ==========");
            Console.WriteLine("{");
            Console.WriteLine("  \"lstInvoiceDTO\": [{");
            Console.WriteLine($"    \"id\": 591468950,");
            Console.WriteLine($"    \"certString\": \"{certString}\",");
            Console.WriteLine($"    \"serial\": \"{cert.SerialNumber}\",");
            Console.WriteLine($"    \"hashValue\": \"{hashValue}\"");
            Console.WriteLine("  }]");
            Console.WriteLine("}");

            store.Close();
        }

        private async Task TestChukyso(int idInvoice)
        {
            // 1. Mở store
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            // Lấy TẤT CẢ cert Viettel có private key
            var allCerts = store.Certificates
                .Cast<X509Certificate2>()
                .Where(c => c.HasPrivateKey)
                .Where(c => c.Issuer.Contains("Viettel"))
                .Where(c => c.NotAfter > DateTime.Now)
                .ToList();

            if (allCerts.Count == 0)
            {
                Console.WriteLine("❌ Không tìm thấy certificate Viettel còn hiệu lực");
                return;
            }

            // In ra thông tin từng cert để debug
            Console.WriteLine($"Tìm thấy {allCerts.Count} cert:\n");
            foreach (var c in allCerts)
            {
                string b64 = Convert.ToBase64String(c.Export(X509ContentType.Cert));
                Console.WriteLine($"- Serial: {c.SerialNumber}");
                Console.WriteLine($"  Issuer: {c.Issuer}");
                Console.WriteLine($"  NotAfter: {c.NotAfter:yyyy-MM-dd}");
                Console.WriteLine($"  Base64 length: {b64.Length}");
                Console.WriteLine($"  HasPrivateKey: {c.HasPrivateKey}");
                Console.WriteLine();
            }

            // Chọn cert có Base64 DÀI NHẤT (đó là cert 2048 bit)
            var cert = allCerts
                .OrderByDescending(c => Convert.ToBase64String(c.Export(X509ContentType.Cert)).Length)
                .FirstOrDefault();

            Console.WriteLine($"✅ Chọn cert có độ dài Base64 lớn nhất (khóa 2048 bit)");
            Console.WriteLine($"✔ Serial: {cert.SerialNumber}");
            Console.WriteLine($"✔ NotAfter: {cert.NotAfter:yyyy-MM-dd}");

            // ========== LẤY certString ==========
            string issuerName = "Viettel-CA SHA2";
            byte[] certBytes = cert.Export(X509ContentType.Cert);
            string certBase64 = Convert.ToBase64String(certBytes);
            string certString = $"{issuerName},{certBase64}";
            certString = certString.Replace("\n", "").Replace("\r", "").Replace(" ", "");

            Console.WriteLine($"📝 Độ dài certString: {certString.Length} ký tự (phải > 1200 nếu là cert đúng)");

            // ========== GỌI API before-sign ==========
            int invoiceId = idInvoice;
            string invoiceXml = null;
            string transactionId = null;

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);

                // ========== THÊM ACCESS_TOKEN VÀO HEADER ==========
                // Lấy access_token từ loginResponse (đã có sau khi login)
                if (loginResponse == null || string.IsNullOrEmpty(loginResponse.access_token))
                {
                    Console.WriteLine("❌ Chưa có access_token. Hãy đăng nhập trước!");
                    return;
                }

                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {loginResponse.access_token}");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.Add("X-Session-Token", useCookie.session_token);
                client.DefaultRequestHeaders.Add("Referer", "https://vinvoice.viettel.vn/invoice-management/invoice-draft");
                client.DefaultRequestHeaders.Add("Origin", "https://vinvoice.viettel.vn");

                var beforeRequest = new
                {
                    invoiceIds = new[] { invoiceId },
                    issueDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    certString = certString,
                    serial = cert.SerialNumber,
                    source = "WEB"
                };

                var jsonBefore = JsonConvert.SerializeObject(beforeRequest);
                Console.WriteLine("\n📤 Before-sign request:");
                Console.WriteLine($"certString length: {certString.Length}");
                Console.WriteLine($"serial: {cert.SerialNumber}");
                Console.WriteLine($"access_token: {loginResponse.access_token?.Substring(0, Math.Min(50, loginResponse.access_token?.Length ?? 0))}...");

                var contentBefore = new StringContent(jsonBefore, Encoding.UTF8, "application/json");

                try
                {
                    var responseBefore = await client.PostAsync("https://vinvoice.viettel.vn/api/cluster5/services/einvoiceapplication/api/invoice/draft/release-list-invoice/usb-token/before-sign", contentBefore);
                    var responseBeforeString = await responseBefore.Content.ReadAsStringAsync();

                    Console.WriteLine($"📥 Before-sign response status: {responseBefore.StatusCode}");
                    Console.WriteLine($"📥 Before-sign response body: {responseBeforeString}");

                    if (!responseBefore.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"❌ before-sign thất bại: {responseBefore.StatusCode}");
                        return;
                    }

                    // Lấy nội dung hóa đơn từ response
                    dynamic beforeResult = JsonConvert.DeserializeObject(responseBeforeString);

                    // Thử các field có thể chứa nội dung
                    invoiceXml = beforeResult?.dataToSign ??
                                beforeResult?.xmlContent ??
                                beforeResult?.content ??
                                beforeResult?.invoiceXml;

                    transactionId = beforeResult?.transactionId;

                    if (string.IsNullOrEmpty(invoiceXml))
                    {
                        Console.WriteLine("⚠️ Response không có dataToSign, dùng chính request JSON làm dữ liệu ký");
                        invoiceXml = jsonBefore;
                    }
                    else
                    {
                        Console.WriteLine($"✔ Đã lấy được nội dung hóa đơn (độ dài: {invoiceXml.Length})");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Lỗi: {ex.Message}");
                    return;
                }
            }

            // ========== TẠO HASH VÀ KÝ ==========
            byte[] data = Encoding.UTF8.GetBytes(invoiceXml);
            byte[] hash;
            byte[] signature;

            using (RSA rsa = cert.GetRSAPrivateKey())
            {
                if (rsa == null)
                {
                    Console.WriteLine("❌ Không lấy được private key");
                    return;
                }

                Console.WriteLine($"🔐 Key size: {rsa.KeySize} bits");

                using (SHA1 sha1 = SHA1.Create())
                {
                    hash = sha1.ComputeHash(data);
                }

                signature = rsa.SignHash(hash, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
                Console.WriteLine($"✅ Ký thành công");
            }

            string digestValueBase64 = Convert.ToBase64String(hash);
            string signatureValueBase64 = Convert.ToBase64String(signature);

            // ========== TẠO XML CHỮ KÝ ==========
            string xmlSignature = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
  <SignedInfo>
    <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments""/>
    <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1""/>
    <Reference URI="""">
      <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1""/>
      <DigestValue>{digestValueBase64}</DigestValue>
    </Reference>
  </SignedInfo>
  <SignatureValue>{signatureValueBase64}</SignatureValue>
  <KeyInfo>
    <X509Data>
      <X509Certificate>{certBase64}</X509Certificate>
    </X509Data>
  </KeyInfo>
</Signature>";

            string hashValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlSignature));

            // ========== GỌI API after-sign ==========
            using (var client = new HttpClient())
            {
                // ========== THÊM ACCESS_TOKEN VÀO HEADER ==========
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {loginResponse.access_token}");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.Add("X-Session-Token", useCookie.session_token);
                client.DefaultRequestHeaders.Add("Referer", "https://vinvoice.viettel.vn/invoice-management/invoice-draft");
                client.DefaultRequestHeaders.Add("Origin", "https://vinvoice.viettel.vn");

                var afterRequest = new
                {
                    lstInvoiceDTO = new[]
                    {
                new
                {
                    id = invoiceId,
                    certString = certString,
                    serial = cert.SerialNumber,
                    hashValue = hashValue
                }
            }
                };

                Console.WriteLine("\n========== GỬI API after-sign ==========");
                var afterJson = JsonConvert.SerializeObject(afterRequest);
                var contentAfter = new StringContent(afterJson, Encoding.UTF8, "application/json");
                XtraMessageBox.Show(contentAfter.ToString()); 
                try
                {
                    var responseAfter = await client.PostAsync("https://vinvoice.viettel.vn/api/cluster5/services/einvoiceapplication/api/invoice/draft/release-invoices/usb-token/after-sign", contentAfter);
                    var responseAfterString = await responseAfter.Content.ReadAsStringAsync();

                    Console.WriteLine($"📥 After-sign response status: {responseAfter.StatusCode}");
                    Console.WriteLine($"📥 After-sign response: {responseAfterString}");

                    if (responseAfter.IsSuccessStatusCode)
                    {
                        Console.WriteLine("\n✅ PHÁT HÀNH HÓA ĐƠN THÀNH CÔNG!");
                    }
                    else
                    {
                        Console.WriteLine($"\n❌ after-sign thất bại: {responseAfter.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Lỗi: {ex.Message}");
                }
            }

            store.Close();
        }

        // Hàm lấy nội dung XML của hóa đơn theo ID
        private string GetInvoiceXmlById(int invoiceId)
        {
            // TODO: Gọi API của Viettel để lấy nội dung hóa đơn
            // GET /api/invoice/draft/{invoiceId}
            // Hoặc lấy từ database của bạn

            // Tạm thời trả về sample (cần thay bằng API thật)
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<Invoice>
  <Info>Hóa đơn số 591468950</Info>
  <Amount>50000000</Amount>
</Invoice>";
        }
        private void TestGetCertString()
        {
            // 1. Lấy certificate từ Windows Store
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            var cert = store.Certificates
                .Cast<X509Certificate2>()
                .Where(c => c.HasPrivateKey)
                .Where(c => c.Issuer.Contains("Viettel-CA SHA2")) // Lọc đúng cert mới
                .Where(c => c.NotAfter > DateTime.Now) // Còn hiệu lực
                .FirstOrDefault();

            if (cert == null)
            {
                Console.WriteLine("❌ Không tìm thấy certificate Viettel-CA SHA2 còn hiệu lực");
                return;
            }

            // 2. Lấy Issuer name (phần CN=...)
            string issuerCn = cert.Issuer
                .Split(',')
                .FirstOrDefault(part => part.Trim().StartsWith("CN="))
                ?.Replace("CN=", "") ?? "Viettel-CA SHA2";

            Console.WriteLine($"Issuer CN: {issuerCn}");

            // 3. Xuất certificate sang Base64
            byte[] certBytes = cert.Export(X509ContentType.Cert);
            string certBase64 = Convert.ToBase64String(certBytes);

            // 4. Ghép thành certString hoàn chỉnh
            string certString = $"{issuerCn},{certBase64}";

            Console.WriteLine("✅ certString:");
            Console.WriteLine(certString);
            Console.WriteLine($"📝 Độ dài: {certString.Length} ký tự");

            store.Close();
        }
        public string GetSession(string username, string password)
        {
            string loginUrl = "https://van.ehoadon.vn/";
            var cookieJar = new CookieContainer();

            // 1. GET trang login lấy VIEWSTATE và EVENTVALIDATION
            var getReq = (HttpWebRequest)WebRequest.Create(loginUrl);
            getReq.CookieContainer = cookieJar;
            getReq.Method = "GET";
            getReq.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

            string html;
            using (var response = (HttpWebResponse)getReq.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                html = reader.ReadToEnd();
            }

            // Lấy __VIEWSTATE
            string viewState = Regex.Match(html, @"__VIEWSTATE"" value=""([^""]+)""").Groups[1].Value;
            // Lấy __EVENTVALIDATION
            string eventVal = Regex.Match(html, @"__EVENTVALIDATION"" value=""([^""]+)""").Groups[1].Value;

            // 2. POST login với đầy đủ field
            string postData = string.Format(
                "__VIEWSTATE={0}&__EVENTVALIDATION={1}&txtUserName={2}&txtPassword={3}&btnLogin=Đăng nhập",
                Uri.EscapeDataString(viewState),
                Uri.EscapeDataString(eventVal),
                username,
                password
            );

            byte[] bytes = Encoding.UTF8.GetBytes(postData);

            var postReq = (HttpWebRequest)WebRequest.Create(loginUrl);
            postReq.CookieContainer = cookieJar;
            postReq.Method = "POST";
            postReq.ContentType = "application/x-www-form-urlencoded";
            postReq.ContentLength = bytes.Length;
            postReq.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
            postReq.Referer = loginUrl;
            postReq.AllowAutoRedirect = false;  // QUAN TRỌNG: Không tự động redirect

            using (var stream = postReq.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            // Đọc response
            using (var response = (HttpWebResponse)postReq.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                string resultHtml = reader.ReadToEnd();

                // In ra trang hiện tại để debug
                Console.WriteLine("Response contains 'Đăng nhập' : " + resultHtml.Contains("Đăng nhập"));
                Console.WriteLine("Response contains 'QLHD' : " + resultHtml.Contains("QLHD"));
            }

            // Lấy session cookie
            foreach (Cookie c in cookieJar.GetCookies(new Uri(loginUrl)))
            {
                Console.WriteLine($"{c.Name} = {c.Value}");
                if (c.Name == "ASP.NET_SessionId")
                    return c.Value;
            }

            return null;
        }
        public void TestBrowser()
        {
            try
            {
                // ===== TĂNG PRIORITY CHO PROCESS =====
                System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.High;

                // ===== SET WORKING DIRECTORY =====
                string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Directory.SetCurrentDirectory(exeDir);

                // Delay ổn định
                Thread.Sleep(3000);

                if (Driver == null)
                {
                    var options = new ChromeOptions();

                    // ===== ARGUMENTS CẦN THIẾT =====
                    options.AddArgument("--no-sandbox");
                    options.AddArgument("--disable-setuid-sandbox");
                    options.AddArgument("--disable-dev-shm-usage");
                    options.AddArgument("--disable-gpu");
                    options.AddArgument("--disable-software-rasterizer");
                    options.AddArgument("--disable-features=VizDisplayCompositor");
                    options.AddArgument("--disable-blink-features=AutomationControlled");
                    options.AddArgument("--disable-extensions");
                    options.AddArgument("--disable-background-timer-throttling");
                    options.AddArgument("--disable-backgrounding-occluded-windows");
                    options.AddArgument("--disable-renderer-backgrounding");
                    options.AddArgument("--disable-infobars");
                    options.AddArgument("--start-maximized");
                    options.AddArgument("--log-level=3");
                    options.AddArgument("--silent");

                    // ===== PROFILE TẠM THỜI =====
                    string tempProfile = Path.Combine(Path.GetTempPath(), "ChromeVB6_" + Guid.NewGuid().ToString());
                    options.AddArgument($"--user-data-dir={tempProfile}");

                    // Tắt Safe Browsing
                    options.AddArgument("--safebrowsing-disable-download-protection");
                    options.AddArgument("--safebrowsing-disable-extension-blacklist");
                    options.AddUserProfilePreference("safebrowsing.enabled", false);
                    options.AddUserProfilePreference("safebrowsing.disable_download_protection", true);

                    string downloadPath = Path.GetTempPath();
                    options.AddUserProfilePreference("download.default_directory", downloadPath);
                    options.AddUserProfilePreference("download.prompt_for_download", false);
                    options.AddUserProfilePreference("disable-popup-blocking", "true");

                    // ===== KIỂM TRA chromedriver.exe =====
                    string chromeDriverPath = Path.Combine(exeDir, "chromedriver.exe");
                    if (!File.Exists(chromeDriverPath))
                    {
                        MessageBox.Show($"Không tìm thấy chromedriver.exe tại: {chromeDriverPath}");
                        return;
                    }

                    // ===== KHỞI TẠO DRIVER =====
                    ChromeDriverService service = ChromeDriverService.CreateDefaultService(exeDir);
                    service.HideCommandPromptWindow = true;
                    service.SuppressInitialDiagnosticInformation = true;

                    Driver = new ChromeDriver(service, options);

                    Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
                    Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);

                    Thread.Sleep(2000);

                    // Navigate
                    Driver.Navigate().GoToUrl("https://hoadondientu.gdt.gov.vn");

                    // ... code xử lý tiếp của bạn
                }
            }
            catch (Exception ex)
            {
                string logPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "error.log");
                File.AppendAllText(logPath, $"{DateTime.Now}: {ex.ToString()}\r\n");
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }

        private void RestartProcess()
        {
            // Khởi động lại process với môi trường mới
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = Assembly.GetExecutingAssembly().Location,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            };
            System.Diagnostics.Process.Start(startInfo);
            Environment.Exit(0);
        }
        private async  void APIInvoice_Load(object sender, EventArgs e)
        {
            //LoadCertificateInfo("Viettel-CASHA2,MIIEdTCCA12gAwIBAgIQVAT//rcDP7MktyjTwv3hEjANBgkqhkiG9w0BAQsFADA/MRgwFgYDVQQDDA9WaWV0dGVsLUNBIFNIQTIxFjAUBgNVBAoMDVZpZXR0ZWwgR3JvdXAxCzAJBgNVBAYTAlZOMB4XDTI2MDIxMDAzMzgwMFoXDTI4MDIxMDAzMzgwMFowgZQxCzAJBgNVBAYTAlZOMR0wGwYDVQQHDBRCw4AgUuG7ikEgVsWoTkcgVMOAVTFGMEQGA1UEAww9Q8OUTkcgVFkgVE5ISCBUSMavxqBORyBN4bqgSSBT4bqiTiBYVeG6pFQgVMOCTiDEkOG7qEMgVEjhu4pOSDEeMBwGCgmSJomT8ixkAQEMDk1TVDozNTAyNDEyNjY5MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDcHMBQ9TGwpTVIn8aLPUnkzlxBAi3rhinHW3H+ayohgwmyhNbfGHiHXDmJrWcYHf+UeNCHlG2KrO/5z7IL7jfwdzkwvOX68HOAmKV5F7PoYlxbLGJNizMYAoEVk1QLemkxx6JNE2IVKlSKMrKIe4IIGW8yV2Z7folnNSLKOCRRnwIDAQABo4IBmTCCAZUwDAYDVR0TAQH/BAIwADAfBgNVHSMEGDAWgBRD1TUAi74HuuNN5h4kWVaIW77MSjB5BggrBgEFBQcBAQRtMGswQgYIKwYBBQUHMAKGNmh0dHA6Ly92aWV0dGVsLWNhLnZuL2Rvd25sb2Fkcy9zdWIvVmlldHRlbC1DQV9TSEEyLmNydDAlBggrBgEFBQcwAYYZaHR0cDovL29jc3AudmlldHRlbC1jYS52bjAzBgNVHSUELDAqBggrBgEFBQcDAgYIKwYBBQUHAwQGCisGAQQBgjcKAwwGCCsGAQUFBwMkMIGEBgNVHR8EfTB7MHmgMqAwhi5odHRwOi8vY3JsLnZpZXR0ZWwtY2Eudm4vVmlldHRlbC1DQS1TSEEyLTIuY3JsokOkQTA/MRgwFgYDVQQDDA9WaWV0dGVsLUNBIFNIQTIxFjAUBgNVBAoMDVZpZXR0ZWwgR3JvdXAxCzAJBgNVBAYTAlZOMB0GA1UdDgQWBBT9h9792TjUnd+bEg0Dyu1V67moMjAOBgNVHQ8BAf8EBAMCBeAwDQYJKoZIhvcNAQELBQADggEBAND1s5vWjelHocG3nF8sSzTOJN1nLKOD8ec42dJ+nSh55tFLmQIQ7C0ThZipZu5l01l3z3pnbK7v2RcEx2FV6Vpj2LW/VIXG/e2H4NII8ADz0n9wLfQTABxhngy2jAVci3rcHWd7/WnH7GqDeU9j2ScwHbH3NZDkz5D+VyzJYcXVFUMzKWwNiXq2h0hFOZnAtPJFAwefiptMeRH58hNs9ro9HvqhVrbEinRcv21Xpp0R1PVNJ400wHBiFadAlgjvChWGKv/8aykM1wp7QnWiFoauhkfAAsesa/6kB0Tbdv6PReTX1667elLe1ABlhXKQUitmaUKiL6FrsSzgfti4pNc=");

            progressPanel1.Caption= "Đang đăng nhập hệ thống vinvoice.viettel.vn...";
            Application.DoEvents(); 
            //using (HttpClient client = new HttpClient())
            //{
            //    string url = $"https://mst.vn/api/company/{"3502495312"}";
            //    var res = await client.GetStringAsync(url); 
            //}



            //await LoginWithSelenium();   // Gọi hàm login mới
            string dbPath = "";
            string password = "1@35^7*9)1";
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);

            // Xóa phần \bin\Debug để lấy đường dẫn gốc
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
            string filePaths = Path.Combine(rootDirectory, "Hoadon", "dpPath.txt");
            string pathThumuc = Path.Combine(rootDirectory);
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
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";
            //Đọc file txt
            string filePath = Path.Combine(rootDirectory, "Hoadon", "invoice.txt");
            _content = File.ReadAllText(filePath);
            await Login();
        }
        string _content;
        private IWebDriver driver;
        private async Task LoginWithSelenium2()
        {
            string qrq = "SELECT * FROM tbInvoiceInfo";
            var dtInvoiceInfo = ExecuteQuery(qrq, null);
            var row = dtInvoiceInfo.Rows[0];

            try
            {
                // ===== KHỞI TẠO TRÌNH DUYỆT =====
                System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.High;

                string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Directory.SetCurrentDirectory(exeDir);

                Thread.Sleep(3000);

                if (driver == null)
                {
                    var options = new ChromeOptions();

                    options.AddArgument("--no-sandbox");
                    options.AddArgument("--disable-setuid-sandbox");
                    options.AddArgument("--disable-dev-shm-usage");
                    options.AddArgument("--disable-gpu");
                    options.AddArgument("--disable-software-rasterizer");
                    options.AddArgument("--disable-features=VizDisplayCompositor");
                    options.AddArgument("--disable-blink-features=AutomationControlled");
                    options.AddArgument("--disable-extensions");
                    options.AddArgument("--disable-background-timer-throttling");
                    options.AddArgument("--disable-backgrounding-occluded-windows");
                    options.AddArgument("--disable-renderer-backgrounding");
                    options.AddArgument("--disable-infobars");
                    options.AddArgument("--start-maximized");
                    options.AddArgument("--log-level=3");
                    options.AddArgument("--silent");
                    options.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                    string tempProfile = Path.Combine(Path.GetTempPath(), "ChromeVB6_" + Guid.NewGuid().ToString());
                    options.AddArgument($"--user-data-dir={tempProfile}");

                    options.AddArgument("--safebrowsing-disable-download-protection");
                    options.AddArgument("--safebrowsing-disable-extension-blacklist");
                    options.AddUserProfilePreference("safebrowsing.enabled", false);
                    options.AddUserProfilePreference("safebrowsing.disable_download_protection", true);

                    string downloadPath = Path.GetTempPath();
                    options.AddUserProfilePreference("download.default_directory", downloadPath);
                    options.AddUserProfilePreference("download.prompt_for_download", false);
                    options.AddUserProfilePreference("disable-popup-blocking", "true");

                    string chromeDriverPath = Path.Combine(exeDir, "chromedriver.exe");
                    if (!File.Exists(chromeDriverPath))
                    {
                        MessageBox.Show($"Không tìm thấy chromedriver.exe tại: {chromeDriverPath}");
                        return;
                    }

                    ChromeDriverService service = ChromeDriverService.CreateDefaultService(exeDir);
                    service.HideCommandPromptWindow = true;
                    service.SuppressInitialDiagnosticInformation = true;

                    driver = new ChromeDriver(service, options);
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
                    driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);

                    Thread.Sleep(2000);
                }

                // ===== ĐĂNG NHẬP =====
                lblStatus.Text = "Đang mở trang đăng nhập...";
                driver.Navigate().GoToUrl("https://vinvoice.viettel.vn/account/login");

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(25));

                IWebElement txtUsername = wait.Until(d => d.FindElement(By.CssSelector("input[placeholder*='Nhập tên đăng nhập của bạn *']")));
                IWebElement txtPassword = driver.FindElement(By.CssSelector("input[type='password']"));
                IWebElement btnLogin = driver.FindElement(By.CssSelector("button[type='submit']"));

                txtUsername.Clear();
                txtUsername.SendKeys(row["Username"].ToString());

                txtPassword.Clear();
                txtPassword.SendKeys(row["Password"].ToString());

                btnLogin.Click();

                // Chờ đăng nhập thành công
                wait.Until(d => d.Url.Contains("vinvoice.viettel.vn") && !d.Url.ToLower().Contains("login"));

                await Task.Delay(3000);

                // ===== CHUYỂN ĐẾN TRANG DRAFT =====
                lblStatus.Text = "Đang chuyển đến trang hóa đơn nháp...";
                driver.Navigate().GoToUrl("https://vinvoice.viettel.vn/invoice-management/invoice-draft");
                await Task.Delay(4000); // Chờ trang load

                // ===== XỬ LÝ POPUP (SAU KHI CHUYỂN TRANG) =====
                lblStatus.Text = "Đang xử lý popup...";
                richTextBox1.AppendText("Đang xử lý popup trên trang draft...\n");

                try
                {
                    // Hàm xử lý popup chung
                    bool popupClosed = false;

                    // Thử click nút Đóng trong modal-footer
                    try
                    {
                        var closeButton = driver.FindElement(By.XPath("//div[@class='modal-footer']//button[contains(., 'Đóng')]"));
                        if (closeButton.Displayed && closeButton.Enabled)
                        {
                            // Tích vào checkbox "Không hiển thị lại lần sau" nếu có
                            try
                            {
                                var dontShowAgain = driver.FindElement(By.Id("viewNoti"));
                                if (!dontShowAgain.Selected)
                                {
                                    dontShowAgain.Click();
                                    richTextBox1.AppendText("✓ Đã chọn 'Không hiển thị lại lần sau'\n");
                                }
                            }
                            catch { }

                            closeButton.Click();
                            richTextBox1.AppendText("✓ Đã đóng popup (nút Đóng)\n");
                            popupClosed = true;
                            await Task.Delay(500);
                        }
                    }
                    catch { }

                    // Nếu chưa đóng, thử nút X
                    if (!popupClosed)
                    {
                        try
                        {
                            var xButton = driver.FindElement(By.CssSelector("button.close span[aria-hidden='true']"));
                            if (xButton.Displayed && xButton.Enabled)
                            {
                                xButton.Click();
                                richTextBox1.AppendText("✓ Đã đóng popup (nút X)\n");
                                popupClosed = true;
                                await Task.Delay(500);
                            }
                        }
                        catch { }
                    }

                    // Dùng JavaScript để ẩn popup (cách cuối)
                    if (!popupClosed)
                    {
                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                        js.ExecuteScript(@"
                    var popup = document.querySelector('jhi-popup-notification');
                    if(popup) popup.remove();
                    var modal = document.querySelector('.modal');
                    if(modal) modal.remove();
                    var backdrop = document.querySelector('.modal-backdrop');
                    if(backdrop) backdrop.remove();
                ");
                        richTextBox1.AppendText("✓ Đã xóa popup bằng JavaScript\n");
                    }
                }
                catch (Exception ex)
                {
                    richTextBox1.AppendText($"⚠️ Lỗi xử lý popup: {ex.Message}\n");
                }

                await Task.Delay(1000);

                // ===== LẤY COOKIES =====
                lblStatus.Text = "Đăng nhập thành công! Đang lấy cookies...";

                var allCookies = driver.Manage().Cookies.AllCookies;

                richTextBox1.Clear();
                richTextBox1.AppendText($"✅ Đăng nhập thành công!\n");
                richTextBox1.AppendText($"Đã vào trang: {driver.Url}\n");
                richTextBox1.AppendText($"Tìm thấy {allCookies.Count} cookies:\n\n");

                foreach (var cookie in allCookies.OrderBy(c => c.Name))
                {
                    lstallCookies[cookie.Name] = cookie.Value;
                    richTextBox1.AppendText($"Name : {cookie.Name}\n");
                    richTextBox1.AppendText($"Value: {cookie.Value}\n");
                    richTextBox1.AppendText($"Domain: {cookie.Domain}\n");
                    richTextBox1.AppendText(new string('-', 60) + "\n");
                }

                string cookieString = string.Join("; ", allCookies.Select(c => $"{c.Name}={c.Value}"));
                richTextBox1.AppendText("\n=== COOKIE STRING ===\n" + cookieString);

                MessageBox.Show($"Thành công! Đã vào trang hóa đơn nháp.\nCookies: {allCookies.Count}", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                string logPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "error.log");
                File.AppendAllText(logPath, $"{DateTime.Now}: {ex.ToString()}\r\n");

                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Lỗi: " + ex.Message;
            }
        }
        private async Task LoginWithSelenium(string cardid)
        {
            string qrq = "SELECT * FROM tbInvoiceInfo";
            var dtInvoiceInfo = ExecuteQuery(qrq, null);
            var row = dtInvoiceInfo.Rows[0];

            try
            {
                // ===== KHỞI TẠO TRÌNH DUYỆT =====
                string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Directory.SetCurrentDirectory(exeDir);

                if (driver == null)
                {
                    var options = new ChromeOptions();
                    options.AddArgument("--no-sandbox");
                    options.AddArgument("--disable-dev-shm-usage");
                    options.AddArgument("--disable-gpu");
                    options.AddArgument("--disable-extensions");
                    options.AddArgument("--start-maximized");
                    options.AddArgument("--log-level=3");

                    string tempProfile = Path.Combine(Path.GetTempPath(), "Chrome_" + Guid.NewGuid().ToString());
                    options.AddArgument($"--user-data-dir={tempProfile}");
                    options.AddUserProfilePreference("safebrowsing.enabled", false);

                    ChromeDriverService service = ChromeDriverService.CreateDefaultService(exeDir);
                    service.HideCommandPromptWindow = true;

                    driver = new ChromeDriver(service, options);
                    driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
                }

                // ===== ĐĂNG NHẬP =====
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

                driver.Navigate().GoToUrl("https://vinvoice.viettel.vn/account/login");

                // Điền thông tin đăng nhập
                wait.Until(d => d.FindElement(By.CssSelector("input[placeholder*='Nhập tên đăng nhập']"))).SendKeys(row["Username"].ToString());
                driver.FindElement(By.CssSelector("input[type='password']")).SendKeys(row["Password"].ToString());
                driver.FindElement(By.CssSelector("button[type='submit']")).Click();

                // Chờ đăng nhập xong (không delay cứng)
                wait.Until(d => d.Url.Contains("vinvoice.viettel.vn") && !d.Url.Contains("/login"));

                // Chuyển thẳng đến trang draft (không delay)
                driver.Navigate().GoToUrl("https://vinvoice.viettel.vn/invoice-management/invoice-draft");
                Thread.Sleep(1000);
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
                var btnClose = driver.FindElement(By.XPath("//button[@aria-label='Close']"));
                btnClose.Click();
                Thread.Sleep(1000);
                //wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
                // btnClose = driver.FindElement(By.XPath("//button[@aria-label='Close']"));
                //btnClose.Click();
                //Thread.Sleep(1000);
                try
                {
                    // chờ button tồn tại + click được luôn
                    var allButtons = driver.FindElements(By.XPath("//button[i[contains(@class,'fa-cloud-upload')]]"));

                    if (allButtons.Count > 1)
                    {
                        allButtons[1].Click();
                    }
                    this.Close();
                }
                catch(Exception ex)
                {

                }
                // Chờ trang draft load + xử lý popup cùng lúc
                try
                {
                    // Chờ popup xuất hiện (nếu có) trong 3 giây
                    var closeBtn = wait.Until(d =>
                    {
                        try
                        {
                            var btn = d.FindElement(By.XPath("//div[@class='modal-footer']//button[contains(., 'Đóng')]"));
                            return btn.Displayed ? btn : null;
                        }
                        catch { return null; }
                    });

                    if (closeBtn != null)
                    {
                        // Tích checkbox nếu có (không cần check Selected)
                        try { driver.FindElement(By.Id("viewNoti")).Click(); } catch { }
                        closeBtn.Click();
                    }
                }
                catch (WebDriverTimeoutException)
                {
                    // Không có popup, tiếp tục
                }
              
                // ===== LẤY COOKIES =====
                var allCookies = driver.Manage().Cookies.AllCookies;

                richTextBox1.Clear();
                richTextBox1.AppendText($"✅ Thành công! {allCookies.Count} cookies\n");

                foreach (var cookie in allCookies)
                {
                    lstallCookies[cookie.Name] = cookie.Value;
                    richTextBox1.AppendText($"{cookie.Name}={cookie.Value}\n");
                }

                MessageBox.Show($"Hoàn thành! Cookies: {allCookies.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
                this.Close();
            }
        }
        private Dictionary<string, string> lstallCookies = new Dictionary<string, string>();   // Lưu tất cả cookie
        private async void simpleButton1_Click(object sender, EventArgs e)
        {
          
            var baseUrl = "https://vinvoice.viettel.vn";
            var cookieContainer = new CookieContainer();

            using (var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true
            })
            using (var client = new HttpClient(handler))
            {
                // 1. Thêm cookies đã có vào CookieContainer
                var uri = new Uri(baseUrl);

                // Thêm __cf_bm
                cookieContainer.Add(uri, new Cookie("__cf_bm", useCookie.__cf_bm));

                // Thêm JSESSIONID
                cookieContainer.Add(uri, new Cookie("JSESSIONID", useCookie.JSESSIONID));

                // 2. Thêm headers (access_token và session_token)
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {useCookie.access_token}");
                client.DefaultRequestHeaders.Add("X-Session-Token", useCookie.session_token);

                // 3. Gọi API search (cookies sẽ tự động được gửi từ CookieContainer)
                var searchUrl = "https://vinvoice.viettel.vn/api/cluster5/services/einvoiceapplication/api/product/search?page=0&size=10&productCode.contains=&productName.contains=&unitName.contains=&sort=createdDate%2Cdesc";

                var response = await client.GetAsync(searchUrl);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Thành công!\n{result}");
                }
                else
                {
                    MessageBox.Show($"Lỗi {response.StatusCode}:\n{result}");
                }

                // Debug: Kiểm tra cookies đã được gửi
                Console.WriteLine("Cookies in container:");
                var cookies = cookieContainer.GetCookies(uri);
                foreach (Cookie cookie in cookies)
                {
                    Console.WriteLine($"{cookie.Name} = {cookie.Value}");
                }
            }
        }

        private async Task<string> CallApiWithAllCookies(string fullUrl)
        {
            if (lstallCookies.Count == 0)
            {
                MessageBox.Show("Chưa có cookie. Vui lòng đăng nhập lại.");
                return "No cookie";
            }

            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);   // hoặc Post nếu cần

                    // Tạo Cookie string từ tất cả cookie đã lưu
                    string cookieHeader = string.Join("; ", lstallCookies.Select(c => $"{c.Key}={c.Value}"));
                    cookieHeader = "ga=GA1.1.2010400788.1776829182; _ga_XPBRQ19161=GS2.1.s1776829181$o1$g0$t1776829181$j60$l0$h0; access_token=eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX25hbWUiOiIzNTAyNDEyNjY5Iiwic2NvcGUiOlsib3BlbmlkIl0sImV4cCI6MTc3NjgzMDM4MSwidHlwZSI6MSwiaWF0IjoxNzc2ODI5MTgxLCJpbnZvaWNlX2NsdXN0ZXIiOiJjbHVzdGVyNSIsImF1dGhvcml0aWVzIjpbIlJPTEVfVVNFUiJdLCJqdGkiOiJjM2M5Njc5MC1lNjE1LTQ3ZmMtYTkwNS0wMDU2ZmEyMzRhMTQiLCJjbGllbnRfaWQiOiJ3ZWJfYXBwIn0.BMP67lEVbkTFINyxIaTaibmqs1ilhDNYzLYlho8CI3dfe2l50O5HkHKlB8qi35lPVS8Hj9wuwv_91rK1jREwyzpx2JVrik5LiSDXHFHySHDzL6zFx_zXf-0D-XQW6hXQeYMR24AcYrBg47pR747UGVXwHT1faq7xiuBsJXJ2BZ3neDB06NQrrhmPtbLr7zEbkwo2ZNY74xu8B1BpfdTfkvOaB9hut2vgGb_Q5UH98cpFELQGJdxiMX7eH3zLo40g2_-Q1DPhJq61cYnIj5F3YVmtGUYXkv131Wb8-ToxIdwq91vpirtceHOvFgmGjNVKu-fT9G55T5k3e5u-ubVjQA; session_token=eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX25hbWUiOiIzNTAyNDEyNjY5Iiwic2NvcGUiOlsib3BlbmlkIl0sImF0aSI6ImMzYzk2NzkwLWU2MTUtNDdmYy1hOTA1LTAwNTZmYTIzNGExNCIsImV4cCI6MTc3NzQzMzk4MSwidHlwZSI6MSwiaWF0IjoxNzc2ODI5MTgxLCJpbnZvaWNlX2NsdXN0ZXIiOiJjbHVzdGVyNSIsImF1dGhvcml0aWVzIjpbIlJPTEVfVVNFUiJdLCJqdGkiOiJiOWExNDMzZC0wMDk5LTQ3ZTMtOGU1ZS01NGNkZTBhMTg5NzIiLCJjbGllbnRfaWQiOiJ3ZWJfYXBwIn0.dYSXxe0NKhWMyiRE-d0Z2MmN5YhtshDwsi5lgbYWNAQNS0CWtsFTYz1bZcQQhe0sgIern06FQWekqvQha5KEzWyox2F7ffqeZNd9QlRQuy3IpA7QkMVYywkXpjy1tCCZNfxaI-KAQXWIQdRlGWi5AkpeyTjSKcwiK1_-ZpMq10b-g33HL_5B6uYVb4nkmgzjWpvOVDKN5Zb-0BoJFvx6_QvfyDZfSVoMuEDkk4iqddsNj-dO0O0aJlqHOinR61AUuMjeprX3v0nYdRVwY95xaC1P4DxjGgSMiINjgNXU9MuATMsKZpnXrmLIdj3WxvtTkIjla3CauGioEddPx7l_eg; JSESSIONID=Zct53cLKDDgd6aXzhcWkntT8Y1d8hy5bZ2QyuQCB; showPopup=0; __cf_bm=PSkGdoZqa2b032gCkx.MPb4YmwzDlqwFh3asQAjjRBA-1776829218.7304292-1.0.1.1-pQY9qYScyeJcIWneYLQsxKfyAMD_1rBMr.U0BVn5xr.4l67lRTL87IfAP7Hm8tdXP5Oiogql0EHj1iz53a9qb0u0Xpqt5xn7TFCtB5PmvA6NYOafYopCpt5mh.RLevab";
                    request.Headers.Add("Cookie", cookieHeader);

                    // Header bổ sung quan trọng
                    request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                    request.Headers.Add("Accept", "application/json, text/plain, */*");
                    request.Headers.Add("Referer", "https://vinvoice.viettel.vn/");

                    var response = await client.SendAsync(request);
                    string result = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return result;
                    }
                    else
                    {
                        richTextBox1.AppendText($"\nLỗi {(int)response.StatusCode}: {result}\n");
                        return $"Error {(int)response.StatusCode}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gọi API: " + ex.Message);
                return "Exception: " + ex.Message;
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            string url= "https://vinvoice.viettel.vn/api/cluster5/services/einvoiceapplication/api/product/search?page=0&size=10&productCode.contains=&productName.contains=&unitName.contains=&sort=createdDate%2Cdesc";
            var apiResult = CallApiWithAllCookies(url).GetAwaiter().GetResult();
        }
        string password, connectionString;
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
        private async void simpleButton3_Click(object sender, EventArgs e)
        {
            progressPanel1.Caption = "Đang thực hiện tạo hoá đơn nháp...";
            Application.DoEvents();

            string query = "SELECT * FROM tbRegister";
            string pathluu = "";    
            var kq = ExecuteQuery(query, null); 
            try
            {
                if (kq.Rows.Count > 0)
                {
                    pathluu = kq.Rows[0]["Hoadonpath"].ToString();
                    pathluu = Directory.GetParent(pathluu).FullName;
                    pathluu = Path.Combine(pathluu, $"HoaDon/HdNhap");
                    // ✅ kiểm tra tồn tại
                    if (!Directory.Exists(pathluu))
                    {
                        Directory.CreateDirectory(pathluu);
                    }
                    //Lấy nam taichinh
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
            //Lấy tbgetphieu
            var getsplit = _content.Split('_');
            progressPanel1.Caption = "Đang lấy thông tin hoá đơn...";
            Application.DoEvents();
            // Có thể bạn nên JOIN theo ID, không phải MaSo
            var qrTimct = @"
    SELECT ChungTu.*,HOADON.*
    FROM ChungTu 
    INNER JOIN HOADON ON HOADON.MaSo = ChungTu.MaSo 
    WHERE ChungTu.SoHieu = ? AND KyHieu = ? AND NgayCT=? ";

            var parameterss = new OleDbParameter[]
            {
    new OleDbParameter("?", getsplit[0]),
    new OleDbParameter("?", getsplit[1]),
    new OleDbParameter("?", getsplit[2])
            };

            var kq2 = ExecuteQuery(qrTimct, parameterss);

            string sqlgp = @"
    SELECT tbGetphieu.*
    FROM tbGetphieu
    INNER JOIN HOADON ON CStr(HOADON.MaSo) = tbGetphieu.MaCT
    WHERE HOADON.MaSo = @MaSo";

            parameterss = new OleDbParameter[]
          {
    new OleDbParameter(" ? ", kq2.Rows[0]["HOADON.MaSo"].ToString()),
          };

            var kqgphieu = ExecuteQuery(sqlgp, parameterss);
             

            //Lấy danh sách chứng từ từ MaCT
            var sql = "SELECT * FROM KhachHang WHERE MaSo = ?";
            parameterss = new OleDbParameter[]
          {
    new OleDbParameter("?",  kq2.Rows[0]["MaKhachHang"]),
          };
            var dtKhachhang = ExecuteQuery(sql, parameterss);

            sql = "SELECT * FROM ChungTu WHERE MaCT = ?";
            var param = new OleDbParameter[]
            {
                new OleDbParameter("?", kq2.Rows[0]["MaCT"])
            };
            //Lấy data khách hàng Từ MaKhachHang 
            var kq3 = ExecuteQuery(sql, param);
            //add datatable hang hoa
            DataTable dtHangHoa = new DataTable();
            dtHangHoa.Columns.Add("ItemCode", typeof(string));
            dtHangHoa.Columns.Add("ItemName", typeof(string));
            dtHangHoa.Columns.Add("UnitName", typeof(string));
            dtHangHoa.Columns.Add("UnitPrice", typeof(decimal));
            dtHangHoa.Columns.Add("Quantity", typeof(decimal));
            dtHangHoa.Columns.Add("Amount", typeof(decimal));
            foreach (DataRow row in kq3.Rows)
            {
                try
                {
                    if( row["MaVattu"].ToString().Trim() == "0")
                    {
                        continue; // Bỏ qua nếu MaVattu trống
                    }
                    string sqlHangHoa = "SELECT * FROM Vattu WHERE MaSo = ?";
                    var paramHangHoa = new OleDbParameter[]
                    {
                    new OleDbParameter("?", row["MaVattu"])
                    };
                    var kqHangHoa = ExecuteQuery(sqlHangHoa, paramHangHoa);
                    double sops = row["SoPS"].ToString() == "" ? 0 : Convert.ToDouble(row["SoPS"]);
                    double soluong = row["SoPS2Co"].ToString() == "" ? 0 : Convert.ToDouble(row["SoPS2Co"]);
                    double dongia = Math.Round((sops / soluong),2);
                    string tenhh = kqHangHoa.Rows[0]["TenVattu"].ToString();
                    string donvitinh = kqHangHoa.Rows[0]["DonVi"].ToString();
                    dtHangHoa.Rows.Add("", tenhh, donvitinh, dongia, soluong, sops);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lấy thông tin hàng hóa: " + ex.Message);
                }
            }
            var itemInfo = dtHangHoa.AsEnumerable()
    .Select((row, index) => new
    {
        lineNumber = index + 1,
        itemCode = "",
        itemName = Helpers.ConvertVniToUnicode(row["ItemName"].ToString()),
        unitName = Helpers.ConvertVniToUnicode(row["UnitName"].ToString()),
        unitPrice = Convert.ToDouble(row["UnitPrice"]),
        quantity = Convert.ToDouble(row["Quantity"]),
        itemTotalAmountWithoutVat = Convert.ToDouble(row["Amount"]),
        selection = 1
    }).ToArray();
            progressPanel1.Caption = "Đang thực hiện tạo hoá đơn ...";
            Application.DoEvents();
            var baseUrl = "https://vinvoice.viettel.vn";
            var cookieContainer = new CookieContainer();

            using (var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true
            })
            using (var client = new HttpClient(handler))
            {
                // 1. Thêm cookies
                var uri = new Uri(baseUrl);
                if(useCookie.__cf_bm!=null)
                cookieContainer.Add(uri, new Cookie("__cf_bm", useCookie.__cf_bm));
                if(useCookie.JSESSIONID!=null)
                cookieContainer.Add(uri, new Cookie("JSESSIONID", useCookie.JSESSIONID));

                // 2. Thêm headers
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {useCookie.access_token}");
                client.DefaultRequestHeaders.Add("X-Session-Token", useCookie.session_token);
                client.DefaultRequestHeaders.Add("Referer", "https://vinvoice.viettel.vn/invoice-management/invoice-draft");
                client.DefaultRequestHeaders.Add("Origin", "https://vinvoice.viettel.vn");

                // 3. Tạo JSON data (toàn bộ payload)
                dynamic dataJson = new ExpandoObject();
                dataJson.buyerTaxCode = null;   // khởi tạo sẵn

                //Kiểm tra xem update hay tạo mới
                var sqlcheck = @"select * from HoaDon  WHERE MaSo =?";
                var paramcheck = new OleDbParameter[]
                { 
        new OleDbParameter("?", kq2.Rows[0]["HOADON.MaSo"].ToString()),

                };
                var checkupdate = ExecuteQuery(sqlcheck, paramcheck);
                if (!string.IsNullOrEmpty(checkupdate.Rows[0]["IdNhap"].ToString()) && checkupdate.Rows[0]["IdNhap"].ToString()!="..." && checkupdate.Rows[0]["IdNhap"].ToString() != "0")
                {
                    dataJson.id= checkupdate.Rows[0]["IdNhap"].ToString();
                    dataJson.transactionUuid = null;
                }
                //Tìm template theo mã đơn hàng
                var sqlTemplate = @"select * from tbInvoiceTemplate  WHERE ID =?";
                var paramtemplate = new OleDbParameter[]
                {
        new OleDbParameter("?",getsplit[3].ToString()),

                };
                var tbtemplate = ExecuteQuery(sqlTemplate, paramtemplate);

                if(kqgphieu.Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(kqgphieu.Rows[0]["CCCD"].ToString()) && kqgphieu.Rows[0]["CCCD"].ToString() != "...")
                    {
                        dataJson.buyerIdNo = kqgphieu.Rows[0]["CCCD"].ToString();
                        dataJson.buyerIdType = 1;
                        dataJson.buyerIdTypeName = "CCCD";
                    }

                }

                dataJson.invoiceType = "1";
                dataJson.templateCode = tbtemplate.Rows[0]["Code"].ToString();
                dataJson.invoiceSeri = tbtemplate.Rows[0]["KHHD"].ToString();
                if (dtKhachhang.Rows[0]["MST"] != null && dtKhachhang.Rows[0]["MST"].ToString() != "..." && dtKhachhang.Rows[0]["MST"].ToString() != "00")
                    dataJson.buyerTaxCode = dtKhachhang.Rows[0]["MST"].ToString();
                if (kq2.Rows[0]["Nguoimuahang"].ToString() != "...")
                    dataJson.buyerName = Helpers.ConvertVniToUnicode(kq2.Rows[0]["Nguoimuahang"].ToString());
                else
                {
                    if (dataJson.buyerTaxCode == null)
                        dataJson.buyerName = "Bán cho người tiêu dùng";
                }
                if (dtKhachhang.Rows[0]["DiaChi"].ToString() != "...")
                    dataJson.buyerAddress = Helpers.ConvertVniToUnicode(dtKhachhang.Rows[0]["DiaChi"].ToString());
                //else
                //    dataJson.buyerAddress = "...";
                    dataJson.totalAmountWithoutVAT = kq3.AsEnumerable().Where(m => m["MaTKTCCo"].ToString() != "14038").Sum(m => Convert.ToDouble(m["SoPS"]));
                dataJson.totalVATAmount = kq3.AsEnumerable().Where(m => m["MaTKTCCo"].ToString() == "14038").Sum(m => Convert.ToDouble(m["SoPS"]));
                dataJson.discountAmount = 0;
                dataJson.totalAmountWithVAT = kq3.AsEnumerable().Sum(m => Convert.ToDouble(m["SoPS"]));
                dataJson.totalAmountAfterDiscount = kq3.AsEnumerable().Where(m => m["MaTKTCCo"].ToString() != "14038").Sum(m => Convert.ToDouble(m["SoPS"]));
                dataJson.totalServiceChargeAmount = 0;
                dataJson.totalExciseTaxAmount = 0;
                dataJson.currencyCode = "VND";
                if (kq2.Rows[0]["Nguoimuahang"].ToString() != "...")
                {
                    dataJson.buyerViewStatus = 0;
                }
                else
                {
                    dataJson.buyerViewStatus = 1;
                }

                    dataJson.invoiceTemplateId = int.Parse(getsplit[3].ToString());
                dataJson.paymentMethod = 3;
                if (kqgphieu.Rows.Count > 0)
                {
                    if (kqgphieu.Rows[0]["TenCty"].ToString() != "...")
                        dataJson.buyerUnitName = Helpers.ConvertVniToUnicode(kqgphieu.Rows[0]["TenCty"].ToString());
                }
            
                dataJson.paymentMethodName = "TM/CK";
                dataJson.domain = null;
                dataJson.autoCreatePdfInstance = 0;
                dataJson.invoiceTypeId = 5;
                dataJson.listProduct = new
                {
                    itemInfo = itemInfo,
                    invoiceTaxBreakdowns = new[]
                    {
                        new
                        {
                            vatPercentage = kq2.Rows[0]["TyLe"].ToString(),
                            vatTaxAmount = kq3.AsEnumerable().Where(m => m["MaTKTCCo"].ToString() == "14038").Sum(m => Convert.ToDouble(m["SoPS"])),
                            vatTaxableAmount = kq3.AsEnumerable().Where(m => m["MaTKTCCo"].ToString() != "14038").Sum(m => Convert.ToDouble(m["SoPS"]))
                        }
                    }
                 };
                dataJson.listInfoUpdate = new object[] { };
                dataJson.exchangeRate = 1;
                dataJson.listElectricityWater = new object[] { };
                dataJson.totalAmountWithTaxInWords = "Bảy mươi tám nghìn bảy trăm sáu mươi đồng";
                dataJson.hdbtscInfo = null;
                dataJson.fileSpecification = null;
                dataJson.listFuelInfo = new object[] { };
                dataJson.source = "WEB";
                 

                // 4. Tạo MultipartFormDataContent với field "data"
                var formData = new MultipartFormDataContent();
                var settings = new JsonSerializerSettings();
                settings.FloatFormatHandling = FloatFormatHandling.DefaultValue;

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(dataJson,settings);
                formData.Add(new StringContent(json, System.Text.Encoding.UTF8, "application/json"), "data");

                // 5. Gọi API
                var saveUrl = "https://vinvoice.viettel.vn/api/cluster5/services/einvoiceapplication/api/invoice/draft/save";
                var response = await client.PostAsync(saveUrl, formData);
                var result = await response.Content.ReadAsStringAsync();
                XtraMessageBox.Show(result);

                if (response.IsSuccessStatusCode)
                {
                    progressPanel1.Caption = "Tạo hóa đơn thành công...";
                    Application.DoEvents();
                    var jObj = JObject.Parse(result);
                    long invoiceId = jObj["data"]["id"].Value<long>();
                    var updateQr = @"UPDATE HoaDon  SET IdNhap = ?,IdTemplate =? WHERE MaSo =?";
                    var updateParameters = new OleDbParameter[]
                    {
        new OleDbParameter("?", invoiceId.ToString()), // Cập nhật giá trị TiLe
        new OleDbParameter("?", getsplit[3].ToString()),
        new OleDbParameter("?", kq2.Rows[0]["HOADON.MaSo"].ToString()),

                    };
                    var updateRowsAffected = ExecuteQueryResult(updateQr, updateParameters);
                   // MessageBox.Show($"Tạo hóa đơn thành công!\n{result}");

                    var url = "https://vinvoice.viettel.vn/api/cluster5/services/einvoiceapplication/api/invoice/gen-pdf-invoice?isDraft=0";

                    var json2 = JsonConvert.SerializeObject(dataJson);

                    var content = new StringContent(json2, Encoding.UTF8, "application/json");

                    // ⚠️ QUAN TRỌNG: thêm cookie/token giống browser
                    //client.DefaultRequestHeaders.Add("Cookie", "YOUR_COOKIE_HERE");

                    var response2 = await client.PostAsync(url, content);

                    var pdfBytes = await response2.Content.ReadAsByteArrayAsync();
                    string path = Path.Combine(pathluu, $"{invoiceId.ToString()}.pdf");
                    File.WriteAllBytes(path, pdfBytes);
                    // mở file
                    Process.Start(new ProcessStartInfo(path)
                    {
                        UseShellExecute = true
                    });

                    var upd = @"UPDATE tbResponse  SET Status = ?";
                    var paas = new OleDbParameter[]
                    {
        new OleDbParameter("?", "1")
                    };
                    var rrs = ExecuteQueryResult(upd, paas);
                    Application.Exit();
                }
                else
                {
                    progressPanel1.Caption = $"Lỗi {response.StatusCode}:\n{result}";
                    Application.DoEvents();
                   // MessageBox.Show($"Lỗi {response.StatusCode}:\n{result}");
                }
            }
        }

        private async void btnGetTemplate_Click(object sender, EventArgs e)
        {
            var baseUrl = "https://vinvoice.viettel.vn";
            var cookieContainer = new CookieContainer();

            using (var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true
            })
            using (var client = new HttpClient(handler))
            {
                // 1. Thêm cookies đã có vào CookieContainer
                var uri = new Uri(baseUrl);

                // Thêm __cf_bm
                if (useCookie.__cf_bm!= null)
                cookieContainer.Add(uri, new Cookie("__cf_bm", useCookie.__cf_bm));

                // Thêm JSESSIONID
                if (useCookie.JSESSIONID != null)
                    cookieContainer.Add(uri, new Cookie("JSESSIONID", useCookie.JSESSIONID));

                // 2. Thêm headers (access_token và session_token)
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {useCookie.access_token}");
                client.DefaultRequestHeaders.Add("X-Session-Token", useCookie.session_token);

                // 3. Gọi API search (cookies sẽ tự động được gửi từ CookieContainer)
                var searchUrl = "https://vinvoice.viettel.vn/api/cluster5/services/einvoiceapplication/api/management-order-template/search-template-order?page=0&size=10&invoiceTypeId.equals=&invoiceName.contains=&barcodeType.equals=&status.equals=&sort=id%2Cdesc&sort=createdDate";

                var response = await client.GetAsync(searchUrl);
                var result = await response.Content.ReadAsStringAsync();
                progressPanel1.Caption = "Thực hiện lấy danh sách mẫu hoá đơn...";
                Application.DoEvents();
                if (response.IsSuccessStatusCode)
                {
                    var list = JObject.Parse(result)["data"]["content"]
                    .Select(x => new
                    {
                        Id = (long)x["id"],
                        TemplateCode = (string)x["templateCode"],
                        InvoiceName = (string)x["invoiceName"],
                        status= (int)x["status"]
                    })
                    .Where(m=>m.status==1).ToList();
                    foreach(var item in list)
                    {
                        string getKHHd = "";
                        //Lấy từ api KHHD
                        searchUrl = "https://vinvoice.viettel.vn/api/cluster5/services/einvoiceapplication/api/invoice-release/serial/search?page=0&size=10&createdDate.greaterThanOrEqual=2018-04-01T17%3A00%3A00.000Z&createdDate.lessThanOrEqual=" + DateTime.Now.ToString("yyyy-MM-dd") + "T17%3A00%3A00.000Z&sort=id%2Cdesc";
                        response = await client.GetAsync(searchUrl);
                        result = await response.Content.ReadAsStringAsync();
                        if (response.IsSuccessStatusCode)
                        {
                            var lists = JObject.Parse(result)["data"]["content"]
                               .Select(x => new
                               {
                                   Id = (int)x["id"],
                                   serialNo = (string)x["serialNo"],
                                   invoiceTemplateId= (int)x["invoiceTemplateId"],
                               })
                                .Where(m => m.invoiceTemplateId == item.Id).FirstOrDefault();
                            getKHHd= lists != null ? lists.serialNo : "";
                        }

                        string sqlCheck = "SELECT * FROM tbInvoiceTemplate WHERE Id = @Id";
                        var paramHangHoa = new OleDbParameter[]
                  {
                    new OleDbParameter("?", item.Id.ToString())
                  };
                        var kqHangHoa = ExecuteQuery(sqlCheck, paramHangHoa);
                        if (kqHangHoa.Rows.Count==0)
                        {
                            string sqlInsert = "INSERT INTO tbInvoiceTemplate (ID, Code, Name,KHHD) VALUES (?, ?, ?,?)";
                            var updateParameters = new OleDbParameter[]
                   {
        new OleDbParameter("?", item.Id.ToString()), // Cập nhật giá trị TiLe
        new OleDbParameter("?", item.TemplateCode),
        new OleDbParameter("?", item.InvoiceName),
         new OleDbParameter("?", getKHHd)

                   };
                            var updateRowsAffected = ExecuteQueryResult(sqlInsert, updateParameters);


                            var updateQr = @"UPDATE tbResponse  SET Status = ?";
                            var paas = new OleDbParameter[]
                            {
        new OleDbParameter("?", "1") 
                            };
                            var rrs = ExecuteQueryResult(updateQr, paas);
                        }
                        // richTextBox1.AppendText($"ID: {item.Id}, TemplateCode: {item.TemplateCode}, InvoiceName: {item.InvoiceName}\n");
                    }
                   // MessageBox.Show($"Thành công!\n{result}");
                   Application.Exit();
                }
                else
                {
                    progressPanel1.Caption = $"Lỗi {response.StatusCode}";
                    MessageBox.Show($"Lỗi {response.StatusCode}:\n{result}");
                }

                 
            }
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {

        }
        string getcardid="";
        private void simpleButton5_Click(object sender, EventArgs e)
        {
            //TestBrowser();
            LoginWithSelenium(getcardid);
        }

        private void progressPanel1_Click(object sender, EventArgs e)
        {

        }

        public int ExecuteQueryResult(string query, params OleDbParameter[] parameters)
        {
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Kết nối đến cơ sở dữ liệu thành công!");

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

    }
}
