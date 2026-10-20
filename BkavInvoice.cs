using DevExpress.XtraEditors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SaovietTax.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaovietTax
{
    public partial class BkavInvoice : Form
    {
        // ============ FIELDS CẤP CLASS ============
        private IWebDriver _driver;
        private string _connectionString;
        private string _content;
        private const string BASE_URL = "https://van.ehoadon.vn/";
        private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

        public BkavInvoice()
        {
            InitializeComponent();
        }

        // ============ DB HELPERS (giữ nguyên) ============
        public DataTable ExecuteQuery(string query, params OleDbParameter[] parameters)
        {
            var dt = new DataTable();
            using (var conn = new OleDbConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new OleDbCommand(query, conn))
                    {
                        if (parameters != null) cmd.Parameters.AddRange(parameters);
                        using (var adapter = new OleDbDataAdapter(cmd))
                            adapter.Fill(dt);
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
            return dt;
        }

        public int ExecuteQueryResult(string query, params OleDbParameter[] parameters)
        {
            using (var conn = new OleDbConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new OleDbCommand(query, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    cmd.ExecuteNonQuery();
                }
                using (var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn))
                    return Convert.ToInt32(idCmd.ExecuteScalar());
            }
        }

        public class Product
        {
            public string Name { get; set; }
            public string Unit { get; set; }
            public double Qty { get; set; }
            public double Price { get; set; }
            public double TaxRate { get; set; }  // ← thêm để build detail
        }

        // ================================================================
        // ============ HÀM ĐĂNG NHẬP DÙNG CHUNG ============
        // ================================================================
        private IWebDriver LoginBkav(string username, string password)
        {
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            var options = new ChromeOptions();
            // Bỏ comment nếu muốn ẩn:
            // options.AddArgument("--headless=new");
            // options.AddArgument("--window-size=1920,1080");

            var driver = new ChromeDriver(service, options);
            try
            {
                driver.Navigate().GoToUrl(BASE_URL);
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

                // Username
                var txtUser = wait.Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(By.Id("txtUserName"));
                        return el.Displayed ? el : null;
                    }
                    catch { return null; }
                });
                txtUser.Clear();
                txtUser.SendKeys(username ?? "");

                // Password
                var txtPass = driver.FindElement(By.Id("txtPassword"));
                txtPass.Clear();
                txtPass.SendKeys(password ?? "");

                // Click login
                driver.FindElement(By.Id("btnLogin")).Click();

                // Chờ vào QLHD
                wait.Until(d => d.Url.Contains("QLHD"));
                Console.WriteLine("✅ Login Bkav thành công");
                return driver;
            }
            catch (Exception ex)
            {
                try { driver.Quit(); } catch { }
                throw new Exception("Đăng nhập Bkav thất bại: " + ex.Message, ex);
            }
        }

        // Đảm bảo có driver sống
        private bool EnsureDriver()
        {
            if (_driver == null) return false;
            try { var _ = _driver.Url; return true; }
            catch
            {
                try { _driver.Quit(); } catch { }
                _driver = null;
                return false;
            }
        }

        // ================================================================
        // ============ TẢI PDF (ĐÃ SỬA BUG COOKIE) ============
        // ================================================================
        private string DownloadInvoicePdf(IWebDriver driver, string invoiceGuid, string savePath)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            // 1. Mở ViewDetailInvoice
            driver.Navigate().GoToUrl(BASE_URL + "ViewDetailInvoice?InvoiceGUID=" + invoiceGuid);

            // 2. Chờ object PDF
            var objectTag = wait.Until(d =>
            {
                try
                {
                    var el = d.FindElement(By.CssSelector("#idcontent object[data*='.pdf'], object[type='application/pdf']"));
                    return el.Displayed ? el : null;
                }
                catch { return null; }
            });

            string relativePath = objectTag.GetAttribute("data");
            if (string.IsNullOrEmpty(relativePath))
                throw new Exception("Không tìm thấy đường dẫn PDF.");

            string fullPdfUrl = new Uri(new Uri(driver.Url), relativePath).ToString();
            Console.WriteLine("PDF URL: " + fullPdfUrl);

            // 3. Fetch trong trang để giữ cookie (sửa bug WebClient)
            var b64 = ((IJavaScriptExecutor)driver).ExecuteAsyncScript(@"
                var cb = arguments[arguments.length - 1];
                fetch(arguments[0], { credentials: 'include', cache: 'no-store' })
                    .then(r => { if (!r.ok) throw new Error('HTTP ' + r.status); return r.arrayBuffer(); })
                    .then(buf => {
                        var b = new Uint8Array(buf), s = '';
                        for (var i = 0; i < b.length; i += 0x8000)
                            s += String.fromCharCode.apply(null, b.subarray(i, i + 0x8000));
                        cb(btoa(s));
                    })
                    .catch(e => cb('ERROR:' + e));
            ", fullPdfUrl) as string;

            if (string.IsNullOrEmpty(b64) || b64.StartsWith("ERROR:"))
                throw new Exception("Không tải được PDF: " + b64);

            byte[] pdfBytes = Convert.FromBase64String(b64);
            if (pdfBytes.Length < 1000 || pdfBytes[0] != (byte)'%')
                throw new Exception("File tải về không phải PDF hợp lệ.");

            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            File.WriteAllBytes(savePath, pdfBytes);
            Console.WriteLine($"✅ Đã tải PDF: {savePath} ({pdfBytes.Length} bytes)");
            return savePath;
        }

        // ================================================================
        // ============ POST JSON TRONG TRANG (giống Grok) ============
        // ================================================================
        private JToken PostJson(string relativeOrAbsoluteUrl, object payload, string action)
        {
            var url = relativeOrAbsoluteUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? relativeOrAbsoluteUrl
                : BASE_URL.TrimEnd('/') + relativeOrAbsoluteUrl;

            var body = JsonConvert.SerializeObject(payload ?? new { });

            const string script = @"
                var cb = arguments[arguments.length - 1];
                var url = arguments[0], body = arguments[1];
                fetch(url, {
                    method: 'POST',
                    credentials: 'same-origin',
                    headers: {
                        'Accept': 'application/json, text/javascript, */*; q=0.01',
                        'Content-Type': 'application/json; charset=UTF-8',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: body
                }).then(function(r) {
                    return r.text().then(function(t) {
                        cb({ status: r.status, text: t, url: r.url });
                    });
                }).catch(function(e) {
                    cb({ status: 0, text: String(e), url: url });
                });
            ";

            var raw = ((IJavaScriptExecutor)_driver).ExecuteAsyncScript(script, url, body)
                      as Dictionary<string, object>;
            if (raw == null)
                throw new Exception("Không nhận được phản hồi khi " + action);

            int status = Convert.ToInt32(raw["status"]);
            string text = Convert.ToString(raw["text"] ?? "");

            if (status == 401 || status == 403)
                throw new Exception("Phiên Bkav đã hết hạn. Cần đăng nhập lại.");
            if (status >= 400)
                throw new Exception($"Bkav trả HTTP {status} khi {action}: {text.Substring(0, Math.Min(500, text.Length))}");

            // Unwrap { d: {...} } hoặc { isOk, isError, Object }
            try
            {
                var payloadJson = JToken.Parse(text);
                var result = payloadJson["d"] ?? payloadJson;

                if (result is JObject obj)
                {
                    var isOk = obj.Value<bool?>("isOk") ?? obj.Value<bool?>("IsOk");
                    var isError = obj.Value<bool?>("isError") ?? obj.Value<bool?>("IsError");
                    if (isOk == false || isError == true)
                    {
                        var msg = (obj["Code"] ?? obj["Message"] ?? obj["Object"] ?? ("Không thể " + action)).ToString();
                        throw new Exception(msg);
                    }
                    if (obj["Object"] != null) return obj["Object"];
                    if (obj["Result"] != null) return obj["Result"];
                    if (obj["Data"] != null) return obj["Data"];
                }
                return result;
            }
            catch (Exception) when (text.TrimStart().StartsWith("<"))
            {
                // Bkav trả HTML (đôi khi) → không parse được → trả null
                return JValue.CreateNull();
            }
        }

        // ================================================================
        // ============ LẤY DANH SÁCH MẪU + KÝ HIỆU ============
        // ================================================================
        private List<JObject> GetTemplates()
        {
            var result = PostJson("/WebServices/wsInvoice.asmx/GetInvoiceTemplate",
                new { InvoiceTypeID = "1", IsSearch = true, IsMTT = false },
                "lấy mẫu hóa đơn");
            var list = new List<JObject>();
            if (result is JArray arr)
                foreach (var item in arr.OfType<JObject>()) list.Add(item);
            else if (result is JObject o && o["Rows"] is JArray rows)
                foreach (var item in rows.OfType<JObject>()) list.Add(item);
            return list;
        }

        private List<JObject> GetSerials(string templateId)
        {
            var result = PostJson("/WebServices/wsInvoice.asmx/GetInvoiceSerial",
                new { InvoiceTypeID = "1", InvoiceTemplateID = templateId, ChangeTypeID = 0, IsSearch = true, IsMTT = false },
                "lấy ký hiệu hóa đơn");
            var list = new List<JObject>();
            if (result is JArray arr)
                foreach (var item in arr.OfType<JObject>()) list.Add(item);
            else if (result is JObject o && o["Rows"] is JArray rows)
                foreach (var item in rows.OfType<JObject>()) list.Add(item);
            return list;
        }

        // Chọn mẫu/ký hiệu đầu tiên (đơn giản) — bạn có thể nâng cấp lọc theo năm
        private (string templateId, string templateForm, string serial) PickTemplateAndSerial()
        {
            var templates = GetTemplates();
            if (templates.Count == 0)
                throw new Exception("Tài khoản chưa có mẫu hóa đơn đang sử dụng.");

            var tpl = templates[0];
            string templateId = tpl["InvoiceTemplateID"]?.ToString() ?? "";
            string templateForm = tpl["InvoiceForm"]?.ToString() ?? "";

            var serials = GetSerials(templateId);
            string serial = "";
            foreach (var s in serials)
            {
                var raw = s["InvoiceSerial"]?.ToString() ?? "";
                // Lọc bỏ ID nội bộ (dài > 8 ký tự)
                if (raw.Length >= 3 && raw.Length <= 8 && raw.Any(char.IsLetter) && raw.Any(char.IsDigit))
                {
                    serial = raw;
                    break;
                }
            }
            if (string.IsNullOrEmpty(serial))
                throw new Exception($"Mẫu {templateForm} không có ký hiệu phù hợp.");

            return (templateId, templateForm, serial);
        }

        // ================================================================
        // ============ TẠO HÓA ĐƠN NHÁP (theo Grok) ============
        // ================================================================
        private string CreateDraftMTT(List<Product> products, string buyerMST, string invoiceDate, double taxRate)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

            // ============ 1. VÀO TRANG MÁY TÍNH TIỀN (MTT) ============
            Console.WriteLine("Đang mở trang Máy tính tiền (MTT)...");
            _driver.Navigate().GoToUrl(BASE_URL + "MTT?IsMTT=true");
            wait.Until(d => d.Url.Contains("MTT"));
            Thread.Sleep(2500);

            // ============ 2. BẤM NÚT THÊM MỚI ============
            Console.WriteLine("Đang bấm nút Thêm mới hóa đơn...");
            try
            {
                var btnAddNew = wait.Until(d => d.FindElement(By.Id("body_btnAddNew")));
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btnAddNew);
            }
            catch
            {
                var btnAddNew = wait.Until(d => d.FindElement(By.XPath("//a[contains(text(), 'Thêm mới')] | //button[contains(text(), 'Thêm mới')]")));
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btnAddNew);
            }

            // ============ 3. CHỜ POPUP/IFRAME XUẤT HIỆN ============
            bool isIframe = false;
            try
            {
                wait.Until(d => {
                    var frames = d.FindElements(By.Id("framedialogInvoiceNewEdit"));
                    return frames.Count > 0 && frames[0].Displayed;
                });
                _driver.SwitchTo().Frame("framedialogInvoiceNewEdit");
                isIframe = true;
                Console.WriteLine("-> Đã tìm thấy iframe popup hóa đơn.");
            }
            catch
            {
                Console.WriteLine("-> Không có iframe chính, thao tác trực tiếp trên trang.");
            }

            try
            {
                // ============ 4. ĐIỀN THÔNG TIN NGƯỜI MUA & NGÀY ============
                if (!string.IsNullOrEmpty(buyerMST))
                {
                    try
                    {
                        var txtBuyerSearch = wait.Until(d => d.FindElement(By.Id("txtBuyerSearch")));

                        // 1. Click vào ô để lấy focus
                        txtBuyerSearch.Click();
                        Thread.Sleep(300);

                        // 2. Gõ MST người mua
                        txtBuyerSearch.SendKeys(buyerMST);
                        Thread.Sleep(500);

                        // 3. MẸO CỦA BKAV: Bôi đen toàn bộ (Ctrl + A) để kích hoạt hiện gợi ý
                        txtBuyerSearch.SendKeys(OpenQA.Selenium.Keys.Control + "a");

                        // 4. Chờ danh sách gợi ý xuất hiện
                        try
                        {
                            wait.Until(d => {
                                var items = d.FindElements(By.CssSelector("#eac-container-txtBuyerSearch .eac-item"));
                                return items.Count > 0 && !string.IsNullOrEmpty(items[0].Text);
                            });
                        }
                        catch { Thread.Sleep(1500); }

                        // 5. DÙNG JAVASCRIPT CLICK VÀO DÒNG GỢI Ý ĐẦU TIÊN (Bỏ qua màng chắn)
                        try
                        {
                            string jsClick = @"
                        var container = document.getElementById('eac-container-txtBuyerSearch');
                        if (container) {
                            var items = container.querySelectorAll('.eac-item');
                            if (items.length > 0) {
                                var evt = new MouseEvent('mousedown', { bubbles: true, cancelable: true, view: window });
                                items[0].dispatchEvent(evt);
                                items[0].click();
                                return 'Clicked';
                            }
                        }
                        return 'Not Found';
                    ";
                            var clickResult = ((IJavaScriptExecutor)_driver).ExecuteScript(jsClick)?.ToString();
                            Console.WriteLine("Kết quả click gợi ý: " + clickResult);
                        }
                        catch (Exception ex) { Console.WriteLine("Lỗi JS click: " + ex.Message); }

                        // 6. Chờ Bkav điền Tên và Địa chỉ
                        Thread.Sleep(2000);
                    }
                    catch (Exception ex) { Console.WriteLine("Lỗi khi nhập MST: " + ex.Message); }
                }

                // Điền ngày hóa đơn
                try
                {
                    var txtDate = _driver.FindElement(By.Id("txtInvoiceDate"));
                    txtDate.Click();
                    txtDate.SendKeys(OpenQA.Selenium.Keys.Control + "a");
                    txtDate.SendKeys(OpenQA.Selenium.Keys.Backspace);
                    txtDate.SendKeys(invoiceDate);
                    txtDate.SendKeys(OpenQA.Selenium.Keys.Tab);
                    Thread.Sleep(500);
                }
                catch { }

                // ============ 5. MỞ POPUP THÊM DÒNG HÀNG ============
                var btnAddDetail = wait.Until(d => d.FindElement(By.Id("MasterPlaceHolderBlank_btnAddDetails")));
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btnAddDetail);

                // Đợi popup chi tiết (div.ui-dialog) hiển thị
                wait.Until(d => d.FindElement(By.CssSelector("div.ui-dialog[aria-describedby='InvoiceDetailsNewEdit']")).Displayed);
                Thread.Sleep(2000); // Chờ Bkav load iframe bên trong popup

                // TÌM VÀ SWITCH VÀO IFRAME CỦA POPUP CHI TIẾT (Tự động dò ID)
                bool isDetailIframeFound = false;
                try
                {
                    _driver.SwitchTo().Frame("frameInvoiceDetailsNewEdit");
                    isDetailIframeFound = true;
                }
                catch
                {
                    try
                    {
                        // Nếu id bị đổi, tìm thẻ iframe nằm trong div dialog
                        var iframeElement = _driver.FindElement(By.CssSelector("div[aria-describedby='InvoiceDetailsNewEdit'] iframe"));
                        _driver.SwitchTo().Frame(iframeElement);
                        isDetailIframeFound = true;
                    }
                    catch
                    {
                        Console.WriteLine("-> Không tìm thấy iframe popup chi tiết. Có thể Bkav đổi giao diện.");
                    }
                }

                // ============ 6. NHẬP TỪNG DÒNG HÀNG ============
                for (int i = 0; i < products.Count; i++)
                {
                    var p = products[i];

                    var txtItemName = wait.Until(d => d.FindElement(By.Id("txtItemName")));
                    txtItemName.Clear();
                    txtItemName.SendKeys(p.Name);
                    Thread.Sleep(300);

                    var sugg = _driver.FindElements(By.CssSelector("#eac-container-txtItemName .eac-item"));
                    if (sugg.Count > 0) ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", sugg[0]);

                    var txtUnit = _driver.FindElement(By.Id("txtUnitName"));
                    txtUnit.Clear();
                    txtUnit.SendKeys(p.Unit);

                    var txtQty = _driver.FindElement(By.Id("txtQty"));
                    txtQty.Clear();
                    txtQty.SendKeys(p.Qty.ToString(CultureInfo.InvariantCulture));

                    var txtPrice = _driver.FindElement(By.Id("txtPrice"));
                    txtPrice.Clear();
                    txtPrice.SendKeys(p.Price.ToString("0", CultureInfo.InvariantCulture));
                    txtPrice.SendKeys(OpenQA.Selenium.Keys.Tab);
                    Thread.Sleep(200);

                    // Thuế suất
                    try
                    {
                        var ddl = new SelectElement(_driver.FindElement(By.Id("ddlTaxRate")));
                        try { ddl.SelectByText($"{taxRate}%"); }
                        catch { try { ddl.SelectByValue(FallbackTaxId(taxRate)); } catch { } }
                    }
                    catch { }

                    // Lưu dòng
                    if (i < products.Count - 1)
                    {
                        _driver.FindElement(By.Id("btnAddAndContinue")).Click();
                        wait.Until(d => d.FindElement(By.Id("txtItemName")).GetAttribute("value") == "");
                    }
                    else
                    {
                        _driver.FindElement(By.Id("btnAdd")).Click();
                        Thread.Sleep(1500); // Chờ popup đóng lại
                    }
                    Console.WriteLine($"Đã thêm dòng {i + 1}/{products.Count}: {p.Name}");
                }

                // ============ 7. LƯU HÓA ĐƠN ============
                _driver.SwitchTo().DefaultContent();

                // Trở lại iframe chính để bấm nút Ghi lại
                if (isIframe)
                {
                    try { _driver.SwitchTo().Frame("framedialogInvoiceNewEdit"); }
                    catch { }
                }

                var btnSave = wait.Until(d => d.FindElement(By.Id("btnSave")));
                btnSave.Click();
                Console.WriteLine("Đã bấm Ghi lại (Lưu hóa đơn)");
                Thread.Sleep(4000); // Chờ 4s để bảng danh sách bên ngoài load lại

                // ============ 8. LẤY GUID HÓA ĐƠN VỪA TẠO ============
                _driver.SwitchTo().DefaultContent();

                // Chờ bảng load lại
                try
                {
                    wait.Until(d => d.FindElement(By.Id("gg-table-1")).Displayed);
                }
                catch { }

                Thread.Sleep(2000);

                var firstRow = _driver.FindElement(By.CssSelector("#gg-table-1 tbody tr:first-child"));
                var checkbox = firstRow.FindElement(By.CssSelector(".CheckOne input"));
                string invoiceGuid = checkbox.GetAttribute("data-invoiceguid");

                Console.WriteLine($"✅ Đã tạo nháp: GUID = {invoiceGuid}");
                return invoiceGuid;
            }
            finally
            {
                try { _driver.SwitchTo().DefaultContent(); } catch { }
            }
        }
        // ================================================================
        // ============ TRA CỨU THÔNG TIN MST (API công khai) ============
        // ================================================================
        private (string name, string address) LookupTaxInfo(string taxCode)
        {
            try
            {
                using (var http = new HttpClient())
                {
                    http.Timeout = TimeSpan.FromSeconds(20);
                    http.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    http.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");

                    var body = new StringContent(
                        $"{{\"taxcode\":\"{taxCode}\"}}",
                        Encoding.UTF8, "application/json");

                    var resp = http.PostAsync(
                        BASE_URL.TrimEnd('/') + "/WebServices/WsGetUnitInfor.asmx/GetUnitInforByTaxCode",
                        body).GetAwaiter().GetResult();
                    var json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var obj = JObject.Parse(json);
                    var o = obj["d"]?["Object"];
                    return (
                        (string)o?["TenChinhThuc"] ?? "",
                        (string)o?["DiaChiGiaoDichChinh"] ?? ""
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lookup MST lỗi: " + ex.Message);
                return ("", "");
            }
        }

        private string FallbackTaxId(double rate)
        {
            if (Math.Abs(rate) < 0.001) return "1";
            if (Math.Abs(rate - 5) < 0.001) return "2";
            if (Math.Abs(rate - 10) < 0.001) return "3";
            if (Math.Abs(rate - 8) < 0.001) return "9";
            return "6";
        }

        // ================================================================
        // ============ ADD INVOICE (sửa lại) ============
        // ================================================================
        private async void Addinvoice()
        {
            try
            {
                // 1. Lấy username/password từ DB
                var dtInvoiceInfo = ExecuteQuery("SELECT * FROM tbInvoiceInfo", null);
                var rows = dtInvoiceInfo.Rows[0];
                string username = rows["Username"]?.ToString();
                string password = rows["Password"]?.ToString();

                // 2. Đọc _content = "SoHieu_KyHieu_NgayCT"
                var getsplit = _content.Split('_');
                var qrTimct = @"
                    SELECT ChungTu.*, HOADON.*
                    FROM ChungTu INNER JOIN HOADON ON HOADON.MaSo = ChungTu.MaSo
                    WHERE ChungTu.SoHieu = ? AND KyHieu = ? AND NgayCT = ?";
                var kq2 = ExecuteQuery(qrTimct, new OleDbParameter[]
                {
                    new OleDbParameter("?", getsplit[0]),
                    new OleDbParameter("?", getsplit[1]),
                    new OleDbParameter("?", getsplit[2]),
                });
                if (kq2.Rows.Count == 0)
                    throw new Exception("Không tìm thấy chứng từ.");

                DateTime date = DateTime.ParseExact(kq2.Rows[0]["NgayCT"].ToString(), "dd/MM/yy hh:mm:ss tt", CultureInfo.InvariantCulture);
                string formattedDate = date.ToString("dd/MM/yyyy");

                // 3. Lấy danh sách sản phẩm
                var kq3 = ExecuteQuery("SELECT * FROM ChungTu WHERE MaCT = ?",
                    new[] { new OleDbParameter("?", kq2.Rows[0]["MaCT"]) });
                var dtKhachhang = ExecuteQuery("SELECT * FROM KhachHang WHERE MaSo = ?",
                    new[] { new OleDbParameter("?", kq2.Rows[0]["MaKhachHang"]) });

                var products = new List<Product>();
                foreach (DataRow row in kq3.Rows)
                {
                    if (row["MaVattu"].ToString().Trim() == "0") continue;
                    var kqHH = ExecuteQuery("SELECT * FROM Vattu WHERE MaSo = ?",
                        new[] { new OleDbParameter("?", row["MaVattu"]) });
                    if (kqHH.Rows.Count == 0) continue;

                    double sops = string.IsNullOrEmpty(row["SoPS"].ToString()) ? 0 : Convert.ToDouble(row["SoPS"]);
                    double soluong = string.IsNullOrEmpty(row["SoPS2Co"].ToString()) ? 0 : Convert.ToDouble(row["SoPS2Co"]);
                    products.Add(new Product
                    {
                        Name = Helpers.ConvertVniToUnicode(kqHH.Rows[0]["TenVattu"].ToString()),
                        Unit = Helpers.ConvertVniToUnicode(kqHH.Rows[0]["DonVi"].ToString()),
                        Qty = soluong,
                        Price = soluong == 0 ? 0 : Math.Round(sops / soluong),
                        TaxRate = Convert.ToDouble(kq2.Rows[0]["TyLe"] ?? 0),
                    });
                }

                // 4. Đăng nhập
                Console.WriteLine("Đang đăng nhập Bkav...");
                _driver = LoginBkav(username, password);

                // 5. Xóa nháp cũ nếu có
                string idnhap = kq2.Rows[0]["IdNhap"].ToString();
                if (!string.IsNullOrEmpty(idnhap) && idnhap!="0")
                {
                    Console.WriteLine("Đang xóa nháp cũ: " + idnhap);
                    TryDeleteDraftMTT(idnhap);
                }

                // 6. Tạo nháp mới
                string invoiceGuid = CreateDraftMTT(
                    products,
                    dtKhachhang.Rows[0]["MST"].ToString(),
                    formattedDate,
                    Convert.ToDouble(kq2.Rows[0]["TyLe"] ?? 0));

                // 7. Lưu GUID
                ExecuteQueryResult(@"UPDATE HoaDon SET IdNhap = ? WHERE MaSo = ?", new OleDbParameter[]
                {
                    new OleDbParameter("?", invoiceGuid),
                    new OleDbParameter("?", kq2.Rows[0]["HOADON.MaSo"].ToString()),
                });

                // 8. Tải PDF
                var kq = ExecuteQuery("SELECT * FROM tbRegister", null);
                string hoadonPath = "";
                if (kq.Rows.Count > 0)
                {
                    hoadonPath = kq.Rows[0]["Hoadonpath"].ToString();
                    hoadonPath = Directory.GetParent(hoadonPath).FullName;
                    hoadonPath = Path.Combine(hoadonPath, "HoaDon/HdNhap");
                    Directory.CreateDirectory(hoadonPath);
                }
                string pdfPath = Path.Combine(hoadonPath, $"{kq2.Rows[0]["HOADON.MaSo"]}.pdf");
                DownloadInvoicePdf(_driver, invoiceGuid, pdfPath);

                // 9. Mở file
                Process.Start(new ProcessStartInfo { FileName = pdfPath, UseShellExecute = true });

                // 10. Đóng
                _driver.Quit();
                _driver = null;
                this.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi: " + ex.Message);
                XtraMessageBox.Show("Lỗi: " + ex.Message);
                try { _driver?.Quit(); } catch { }
                _driver = null;
            }
        }

        private void TryDeleteDraft(string idnhap)
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
                _driver.Navigate().GoToUrl(BASE_URL + "QLHD");
                Thread.Sleep(1500);

                var editBtn = wait.Until(d => d.FindElement(By.XPath(
                    $"//a[contains(@onclick, '{idnhap.ToLower()}') and contains(@class, 'aeditInvoice')]")));
                editBtn.Click();

                var popups = _driver.FindElements(By.ClassName("pop"));
                var dialog = popups.FirstOrDefault(p => p.Displayed);
                if (dialog == null) return;
                _driver.SwitchTo().Frame(dialog.FindElement(By.Id("framedialogInvoiceNewEdit")));

                _driver.FindElement(By.Id("btnDeleteInvoice")).Click();
                wait.Until(d => d.FindElement(By.XPath("//div[contains(text(), 'Bạn có chắc chắn muốn Xoá')]")));
                _driver.FindElement(By.XPath("//button[contains(text(), 'Xoá khỏi hệ thống')]")).Click();
                _driver.SwitchTo().DefaultContent();
                Console.WriteLine("Đã xóa nháp cũ");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Không xóa được nháp cũ (bỏ qua): " + ex.Message);
                try { _driver.SwitchTo().DefaultContent(); } catch { }
            }
        }
        private bool TryDeleteDraftMTT(string idnhap)
        {
            if (string.IsNullOrEmpty(idnhap) || idnhap == "0") return true; // Không có gì để xóa

            try
            {
                Console.WriteLine($"Đang kiểm tra và xóa nháp cũ: {idnhap}...");
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

                // 1. Chuyển sang trang MTT
                _driver.Navigate().GoToUrl(BASE_URL + "MTT?IsMTT=true");
                Thread.Sleep(2500); // Chờ bảng load

                // 2. Tìm nút Xem/Sửa của hóa đơn (Dùng FindElements để không văng lỗi nếu không thấy)
                var editBtns = _driver.FindElements(By.XPath(
                    $"//a[contains(@onclick, '{idnhap.ToLower()}') and (contains(@class, 'aeditInvoice') or contains(@class, 'aviewInvoice'))]"));

                // Nếu không tìm thấy nút -> Hóa đơn đã bị xóa hoặc không tồn tại
                if (editBtns.Count == 0)
                {
                    Console.WriteLine("-> Không tìm thấy nháp cũ trên lưới (Có thể đã bị xóa). Bỏ qua.");
                    return true;
                }

                // 3. Nếu tìm thấy, bấm vào nút
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true); arguments[0].click();", editBtns[0]);

                // 4. Chờ popup mở ra
                Thread.Sleep(2000);
                try
                {
                    wait.Until(d => d.FindElement(By.Id("framedialogInvoiceNewEdit")).Displayed);
                    _driver.SwitchTo().Frame("framedialogInvoiceNewEdit");
                }
                catch
                {
                    Console.WriteLine("-> Lỗi: Không mở được popup hóa đơn nháp.");
                    return false;
                }

                // 5. Bấm nút Xóa hóa đơn
                var btnDelete = wait.Until(d => d.FindElement(By.Id("btnDeleteInvoice")));
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btnDelete);

                // 6. Xác nhận xóa
                Thread.Sleep(1500);
                try
                {
                    var btnConfirm = _driver.FindElement(By.XPath("//button[contains(text(), 'Xoá khỏi hệ thống')]"));
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btnConfirm);
                }
                catch
                {
                    // Thử tìm ngoài iframe
                    _driver.SwitchTo().DefaultContent();
                    var btnConfirmOut = wait.Until(d => d.FindElement(By.XPath("//button[contains(text(), 'Xoá khỏi hệ thống')]")));
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btnConfirmOut);
                }

                Console.WriteLine("✅ Đã xóa nháp cũ thành công.");
                Thread.Sleep(2000); // Chờ Bkav xóa xong
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("-> Bỏ qua xóa nháp (Lỗi: " + ex.Message + ")");
                try { _driver.SwitchTo().DefaultContent(); } catch { }
                return false;
            }
        }
        // ================================================================
        // ============ LOAD FORM ============
        // ================================================================
        private void BkavInvoice_Load(object sender, EventArgs e)
        {
            string dbPath = "";
            const string dbPassword = "1@35^7*9)1";
            string appPath = Assembly.GetExecutingAssembly().Location;
            string directoryPath = Path.GetDirectoryName(appPath);
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            try
            {
                dbPath = File.ReadAllText(Path.Combine(rootDirectory, "Hoadon", "dpPath.txt"));
            }
            catch (Exception ex) { Console.WriteLine("Lỗi đọc dpPath.txt: " + ex.Message); }

            _connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={dbPassword};";

            try
            {
                _content = File.ReadAllText(Path.Combine(rootDirectory, "Hoadon", "invoice.txt"));
            }
            catch (Exception ex) { Console.WriteLine("Lỗi đọc invoice.txt: " + ex.Message); }

            if (_content.Contains("PH_"))
                PhatHanhHD();
            else
                Addinvoice();
        }

        // ================================================================
        // ============ PHÁT HÀNH (giữ nguyên code cũ) ============
        // ================================================================
        private void PhatHanhHD()
        {
            try
            {
                // 1. Lấy thông tin đăng nhập
                var dtInfo = ExecuteQuery("SELECT * FROM tbInvoiceInfo", null);
                var row = dtInfo.Rows[0];
                string username = row["Username"]?.ToString();
                string password = row["Password"]?.ToString();

                // 2. Đăng nhập
                Console.WriteLine("Đang đăng nhập để phát hành...");
                _driver = LoginBkav(username, password);
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

                // 3. Lấy GUID từ file txt (định dạng: PH_GUID_...)
                var getsplit = _content.Split('_');
                string invoiceGuid = getsplit[1].ToLower();

                // 4. Chuyển sang trang MTT để tìm hóa đơn
                _driver.Navigate().GoToUrl("https://van.ehoadon.vn/MTT?IsMTT=true");
                wait.Until(d => d.Url.Contains("MTT"));
                Thread.Sleep(2000);

                // 5. Tìm nút Xem/Sửa của hóa đơn đó và click
                var viewBtn = wait.Until(d => d.FindElement(By.XPath(
                    $"//a[contains(@onclick, '{invoiceGuid}') and (contains(@class, 'aviewInvoice') or contains(@class, 'aeditInvoice'))]")));

                // Dùng JS click để tránh bị che
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true); arguments[0].click();", viewBtn);

                // 6. Chờ popup mở ra
                wait.Until(d => d.FindElement(By.Id("dialogViewDetail")).Displayed || d.FindElement(By.Id("framedialogInvoiceNewEdit")).Displayed);
                Thread.Sleep(1500);

                // 7. Tìm và bấm nút Phát hành (Tùy giao diện Bkav có thể là btnPublish, btnSign, btnRelease...)
                // Mở iframe chứa popup
                string frameId = _driver.FindElements(By.Id("framedialogInvoiceNewEdit")).Count > 0 ? "framedialogInvoiceNewEdit" : "framedialogViewDetail";
                _driver.SwitchTo().Frame(frameId);

                // Tìm nút Phát hành (Bạn có thể F12 để xem chính xác ID của nút này)
                var btnPublish = wait.Until(d => d.FindElement(By.XPath("//button[contains(text(), 'Phát hành')] | //a[contains(text(), 'Phát hành')] | //*[@id='btnPublish'] | //*[@id='btnSign']")));

                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btnPublish);
                Console.WriteLine("Đã bấm nút Phát hành. Vui lòng chờ...");

                // 8. Xử lý popup xác nhận (nếu có)
                Thread.Sleep(1500);
                try
                {
                    // Chuyển về trang mặc định để tìm popup xác nhận (thường popup này hiện đè lên trên)
                    _driver.SwitchTo().DefaultContent();
                    var btnConfirm = wait.Until(d => d.FindElement(By.XPath("//button[contains(text(), 'Đồng ý')] | //button[contains(text(), 'Xác nhận')] | //button[contains(@class, 'btn-primary')]")));
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btnConfirm);
                    Console.WriteLine("Đã xác nhận phát hành.");
                }
                catch
                {
                    Console.WriteLine("Không có popup xác nhận hoặc đã tự động ký.");
                }

                // 9. Chờ quá trình ký số (USB Token) hoàn tất
                Console.WriteLine("Đang chờ ký số (nhớ cắm USB Token)...");
                Thread.Sleep(5000);

                MessageBox.Show("Đã hoàn tất thao tác Phát hành!", "Thông báo");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi phát hành: " + ex.Message);
                MessageBox.Show("Lỗi phát hành: " + ex.Message);
            }
            finally
            {
                try { _driver?.Quit(); } catch { }
                _driver = null;
                this.Close();
            }
        }

        // ================================================================
        // ============ ĐÓNG FORM ============
        // ================================================================
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try { _driver?.Quit(); } catch { }
            try { _driver?.Dispose(); } catch { }
            _driver = null;
            base.OnFormClosing(e);
        }
    }
}