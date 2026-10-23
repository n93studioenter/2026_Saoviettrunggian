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
using System.Drawing;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

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
            this.StartPosition = FormStartPosition.Manual;

            // Đặt kích thước form (tuỳ chỉnh) 

            // Đặt form ở góc phải dưới
            this.Location = new Point(
                Screen.PrimaryScreen.WorkingArea.Right - this.Width,
                Screen.PrimaryScreen.WorkingArea.Bottom - this.Height
            );
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
        private IWebDriver LoginBkav(string username, string password,bool ishow)
        {
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            var options = new ChromeOptions();
            // Bỏ comment nếu muốn ẩn:
            if (!ishow)
            {
                options.AddArgument("--headless=new");
            }
             
             options.AddArgument("--window-size=1920,1080");

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
        private void SetCaption(string text)
        {
            if (progressPanel1.InvokeRequired)
                progressPanel1.BeginInvoke(new Action(() => SetCaption(text)));
            else
            {
                progressPanel1.Caption = text;
                progressPanel1.Refresh();
            }
        }
        private string CreateDraftMTT(List<Product> products, string buyerMST, string invoiceDate, double taxRate, int IsMTT)
        {
            if (products == null || products.Count == 0)
                throw new Exception("Không có dòng hàng nào để tạo hoá đơn.");

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

            // ============ 1. VÀO TRANG MÁY TÍNH TIỀN (MTT) ============
            Console.WriteLine("Đang mở trang Máy tính tiền (MTT)...");
            SetCaption("Đang thực hiện tạo hoá đơn...");

            if (IsMTT == 1)
            {
                _driver.Navigate().GoToUrl(BASE_URL + "MTT?IsMTT=true");
                wait.Until(d => d.Url.Contains("MTT"));
            }
            else
            {
                _driver.Navigate().GoToUrl(BASE_URL + "QLHD");
                wait.Until(d => d.Url.Contains("QLHD"));
            }

            // ============ 2. BẤM NÚT THÊM MỚI ============
            Console.WriteLine("Đang bấm nút Thêm mới hóa đơn...");
            IWebElement btnAddNew = null;
            try
            {
                btnAddNew = wait.Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(By.Id("body_btnAddNew"));
                        return el.Displayed && el.Enabled ? el : null;
                    }
                    catch { return null; }
                });
            }
            catch
            {
                try
                {
                    btnAddNew = wait.Until(d =>
                    {
                        try
                        {
                            var el = d.FindElement(By.XPath(
                                "//a[contains(text(), 'Thêm mới')] | //button[contains(text(), 'Thêm mới')]"));
                            return el.Displayed && el.Enabled ? el : null;
                        }
                        catch { return null; }
                    });
                }
                catch { }
            }

            if (btnAddNew == null)
                throw new Exception("Không tìm thấy nút Thêm mới hoá đơn.");

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btnAddNew);

            // ============ 3. CHỜ POPUP/IFRAME XUẤT HIỆN ============
            bool isIframe = false;
            try
            {
                wait.Until(d =>
                {
                    try
                    {
                        var frames = d.FindElements(By.Id("framedialogInvoiceNewEdit"));
                        return frames.Count > 0 && frames[0].Displayed;
                    }
                    catch { return false; }
                });
                _driver.SwitchTo().Frame("framedialogInvoiceNewEdit");
                isIframe = true;
                Console.WriteLine("-> Đã tìm thấy iframe popup hóa đơn.");
            }
            catch
            {
                try
                {
                    var iframeEl = wait.Until(d =>
                    {
                        try
                        {
                            var els = d.FindElements(By.CssSelector("div.ui-dialog iframe"));
                            return els.Count > 0 && els[0].Displayed ? els[0] : null;
                        }
                        catch { return null; }
                    });
                    _driver.SwitchTo().Frame(iframeEl);
                    isIframe = true;
                    Console.WriteLine("-> Vào iframe popup qua div.ui-dialog.");
                }
                catch
                {
                    throw new Exception("Popup hoá đơn không mở — không thể tạo nháp.");
                }
            }

            try
            {
                // ============ 4. ĐIỀN THÔNG TIN NGƯỜI MUA ============
                if (!string.IsNullOrEmpty(buyerMST))
                {
                    bool mstOk = false;

                    for (int attempt = 1; attempt <= 3 && !mstOk; attempt++)
                    {
                        try
                        {
                            Console.WriteLine($"[MST] Lần thử {attempt}/3 — nhập MST: {buyerMST}");

                            // 1. Lấy ô nhập MST
                            var txtBuyerSearch = wait.Until(d =>
                            {
                                try
                                {
                                    var el = d.FindElement(By.Id("txtBuyerSearch"));
                                    return el.Displayed && el.Enabled ? el : null;
                                }
                                catch { return null; }
                            });

                            if (txtBuyerSearch == null)
                                throw new Exception("Không tìm thấy ô txtBuyerSearch.");

                            // 2. Clear sạch (Ctrl+A rồi Delete) — tránh dính text cũ
                            txtBuyerSearch.Click();
                            txtBuyerSearch.SendKeys(OpenQA.Selenium.Keys.Control + "a");
                            txtBuyerSearch.SendKeys(OpenQA.Selenium.Keys.Delete);
                            Thread.Sleep(150);

                            // 3. Gõ MST từng ký tự (50ms/ký tự) — Bkav kịp debounce autocomplete
                            foreach (char c in buyerMST)
                            {
                                txtBuyerSearch.SendKeys(c.ToString());
                                Thread.Sleep(50);
                            }

                            // 4. Chờ value thực sự đúng MST
                            wait.Until(d =>
                            {
                                try
                                {
                                    return d.FindElement(By.Id("txtBuyerSearch")).GetAttribute("value")?.Trim() == buyerMST;
                                }
                                catch { return false; }
                            });

                            // 5. Chờ gợi ý xuất hiện VÀ có nội dung chứa MST (max 8s)
                            IWebElement matchedItem = null;
                            try
                            {
                                matchedItem = new WebDriverWait(_driver, TimeSpan.FromSeconds(8)).Until(d =>
                                {
                                    try
                                    {
                                        var items = d.FindElements(By.CssSelector("#eac-container-txtBuyerSearch .eac-item"));
                                        if (items.Count == 0) return null;

                                        foreach (var it in items)
                                        {
                                            if (!it.Displayed) continue;
                                            var t = it.Text ?? "";
                                            if (t.Contains(buyerMST))
                                                return it;
                                        }
                                        return items[0].Displayed ? items[0] : null;
                                    }
                                    catch { return null; }
                                });
                            }
                            catch
                            {
                                Console.WriteLine("[MST] Không thấy gợi ý sau 8s.");
                            }

                            if (matchedItem == null)
                            {
                                Console.WriteLine("[MST] Không có gợi ý — thử lại.");
                                continue;
                            }

                            Console.WriteLine($"[MST] Click gợi ý: {matchedItem.Text}");

                            // 6. Click bằng JS (dùng element đã verify khớp MST)
                            ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                        var el = arguments[0];
                        if (el) {
                            var evtDown = new MouseEvent('mousedown', { bubbles: true, cancelable: true, view: window });
                            var evtUp   = new MouseEvent('mouseup',   { bubbles: true, cancelable: true, view: window });
                            var evtClick= new MouseEvent('click',     { bubbles: true, cancelable: true, view: window });
                            el.dispatchEvent(evtDown);
                            el.dispatchEvent(evtUp);
                            el.dispatchEvent(evtClick);
                        }
                    ", matchedItem);

                            // 7. Chờ Bkav điền tên/địa chỉ (max 6s) — không throw nếu không có
                            try
                            {
                                new WebDriverWait(_driver, TimeSpan.FromSeconds(6)).Until(d =>
                                {
                                    try
                                    {
                                        var txtName = d.FindElements(By.Id("txtBuyerName")).FirstOrDefault();
                                        var txtAddr = d.FindElements(By.Id("txtBuyerAddress")).FirstOrDefault();
                                        var nameOk = txtName != null && !string.IsNullOrEmpty(txtName.GetAttribute("value"));
                                        var addrOk = txtAddr != null && !string.IsNullOrEmpty(txtAddr.GetAttribute("value"));
                                        return nameOk || addrOk;
                                    }
                                    catch { return false; }
                                });
                            }
                            catch
                            {
                                Console.WriteLine("[MST] Bkav không tự điền tên/địa chỉ sau 6s.");
                            }

                            // 8. Verify MST đã đúng chưa
                            try
                            {
                                var valSearch = _driver.FindElement(By.Id("txtBuyerSearch")).GetAttribute("value")?.Trim() ?? "";
                                string valTaxCode = "";
                                try
                                {
                                    var elTax = _driver.FindElements(By.Id("txtBuyerTaxCode")).FirstOrDefault();
                                    if (elTax != null) valTaxCode = elTax.GetAttribute("value")?.Trim() ?? "";
                                }
                                catch { }

                                bool ok = valSearch == buyerMST || valTaxCode == buyerMST;
                                Console.WriteLine($"[MST] Verify — search='{valSearch}', taxCode='{valTaxCode}' → {(ok ? "OK" : "SAI")}");
                                mstOk = ok;
                            }
                            catch { mstOk = false; }

                            if (!mstOk)
                                Console.WriteLine("[MST] Verify thất bại — thử lại.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[MST] Lỗi lần {attempt}: {ex.Message}");
                        }
                    }

                    if (!mstOk)
                        Console.WriteLine("[MST] ⚠️ Không nhập được MST sau 3 lần thử — vẫn tiếp tục tạo hoá đơn.");
                    else
                        Console.WriteLine("[MST] ✅ Nhập MST thành công.");
                }

                // ============ 4b. ĐIỀN NGÀY HÓA ĐƠN ============
                try
                {
                    var txtDate = wait.Until(d =>
                    {
                        try
                        {
                            var el = d.FindElement(By.Id("txtInvoiceDate"));
                            return el.Displayed ? el : null;
                        }
                        catch { return null; }
                    });

                    txtDate.Click();
                    txtDate.SendKeys(OpenQA.Selenium.Keys.Control + "a");
                    txtDate.SendKeys(OpenQA.Selenium.Keys.Backspace);
                    txtDate.SendKeys(invoiceDate);
                    txtDate.SendKeys(OpenQA.Selenium.Keys.Tab);

                    wait.Until(d =>
                    {
                        try
                        {
                            return d.FindElement(By.Id("txtInvoiceDate")).GetAttribute("value") == invoiceDate;
                        }
                        catch { return false; }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi nhập ngày hoá đơn: " + ex.Message);
                }

                // ============ 5. MỞ POPUP THÊM DÒNG HÀNG ============
                var btnAddDetail = wait.Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(By.Id("MasterPlaceHolderBlank_btnAddDetails"));
                        return el.Displayed && el.Enabled ? el : null;
                    }
                    catch { return null; }
                });

                if (btnAddDetail == null)
                    throw new Exception("Không tìm thấy nút Thêm dòng hàng.");

                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btnAddDetail);

                // Chờ dialog chi tiết hiển thị
                wait.Until(d =>
                {
                    try
                    {
                        var dlg = d.FindElements(By.CssSelector("div.ui-dialog[aria-describedby='InvoiceDetailsNewEdit']"));
                        return dlg.Count > 0 && dlg[0].Displayed;
                    }
                    catch { return false; }
                });

                // Chờ iframe bên trong popup chi tiết sẵn sàng
                bool isDetailIframeFound = false;
                try
                {
                    wait.Until(d =>
                    {
                        try
                        {
                            var frames = d.FindElements(By.Id("frameInvoiceDetailsNewEdit"));
                            return frames.Count > 0 && frames[0].Displayed;
                        }
                        catch { return false; }
                    });
                    _driver.SwitchTo().Frame("frameInvoiceDetailsNewEdit");
                    isDetailIframeFound = true;
                }
                catch
                {
                    try
                    {
                        var iframeEl = wait.Until(d =>
                        {
                            try
                            {
                                var els = d.FindElements(By.CssSelector("div[aria-describedby='InvoiceDetailsNewEdit'] iframe"));
                                return els.Count > 0 && els[0].Displayed ? els[0] : null;
                            }
                            catch { return null; }
                        });
                        _driver.SwitchTo().Frame(iframeEl);
                        isDetailIframeFound = true;
                    }
                    catch { }
                }

                if (!isDetailIframeFound)
                    throw new Exception("Không vào được iframe chi tiết dòng hàng — không thể tạo nháp.");

                // ============ 6. NHẬP TỪNG DÒNG HÀNG ============
                for (int i = 0; i < products.Count; i++)
                {
                    var p = products[i];

                    // --- Tên hàng ---
                    var txtItemName = wait.Until(d =>
                    {
                        try
                        {
                            var el = d.FindElement(By.Id("txtItemName"));
                            return el.Displayed ? el : null;
                        }
                        catch { return null; }
                    });

                    txtItemName.Clear();
                    txtItemName.SendKeys(p.Name);

                    // Chờ gợi ý item (max 3s)
                    IWebElement firstSugg = null;
                    try
                    {
                        firstSugg = new WebDriverWait(_driver, TimeSpan.FromSeconds(3)).Until(d =>
                        {
                            try
                            {
                                var items = d.FindElements(By.CssSelector("#eac-container-txtItemName .eac-item"));
                                return items.Count > 0 && items[0].Displayed ? items[0] : null;
                            }
                            catch { return null; }
                        });
                    }
                    catch { }

                    if (firstSugg != null)
                        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", firstSugg);

                    // --- Đơn vị ---
                    var txtUnit = wait.Until(d =>
                    {
                        try
                        {
                            var el = d.FindElement(By.Id("txtUnitName"));
                            return el.Displayed ? el : null;
                        }
                        catch { return null; }
                    });
                    txtUnit.Clear();
                    txtUnit.SendKeys(p.Unit);

                    // --- Số lượng ---
                    var txtQty = _driver.FindElement(By.Id("txtQty"));
                    txtQty.Clear();
                    txtQty.SendKeys(p.Qty.ToString(CultureInfo.InvariantCulture));

                    // --- Đơn giá ---
                    var txtPrice = _driver.FindElement(By.Id("txtPrice"));
                    txtPrice.Clear();
                    txtPrice.SendKeys(p.Price.ToString("0", CultureInfo.InvariantCulture));
                    txtPrice.SendKeys(OpenQA.Selenium.Keys.Tab);

                    // --- Thuế suất ---
                    try
                    {
                        var ddl = new SelectElement(_driver.FindElement(By.Id("ddlTaxRate")));
                        try { ddl.SelectByText($"{p.TaxRate}%"); }
                        catch { try { ddl.SelectByValue(FallbackTaxId(p.TaxRate)); } catch { } }
                    }
                    catch { }

                    // --- Lưu dòng ---
                    if (i < products.Count - 1)
                    {
                        var btnAddContinue = wait.Until(d =>
                        {
                            try
                            {
                                var el = d.FindElement(By.Id("btnAddAndContinue"));
                                return el.Displayed && el.Enabled ? el : null;
                            }
                            catch { return null; }
                        });
                        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btnAddContinue);

                        // Chờ form reset (txtItemName rỗng)
                        wait.Until(d =>
                        {
                            try
                            {
                                return d.FindElement(By.Id("txtItemName")).GetAttribute("value") == "";
                            }
                            catch { return false; }
                        });
                    }
                    else
                    {
                        var btnAdd = wait.Until(d =>
                        {
                            try
                            {
                                var el = d.FindElement(By.Id("btnAdd"));
                                return el.Displayed && el.Enabled ? el : null;
                            }
                            catch { return null; }
                        });
                        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btnAdd);

                        // Chờ popup chi tiết đóng
                        wait.Until(d =>
                        {
                            try
                            {
                                var dialogs = d.FindElements(By.CssSelector("div.ui-dialog[aria-describedby='InvoiceDetailsNewEdit']"));
                                return dialogs.Count == 0 || !dialogs[0].Displayed;
                            }
                            catch { return true; }
                        });
                    }

                    Console.WriteLine($"Đã thêm dòng {i + 1}/{products.Count}: {p.Name}");
                }

                // ============ 7. LƯU HÓA ĐƠN ============
                _driver.SwitchTo().DefaultContent();

                if (isIframe)
                {
                    try { _driver.SwitchTo().Frame("framedialogInvoiceNewEdit"); }
                    catch { }
                }

                var btnSave = wait.Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(By.Id("btnSave"));
                        return el.Displayed && el.Enabled ? el : null;
                    }
                    catch { return null; }
                });

                if (btnSave == null)
                    throw new Exception("Không tìm thấy nút Ghi lại.");

                btnSave.Click();
                Console.WriteLine("Đã bấm Ghi lại — chờ GUID...");

                // ============ 8. LẤY GUID HÓA ĐƠN VỪA TẠO ============
                _driver.SwitchTo().DefaultContent();

                // Chờ bảng load lại
                try
                {
                    wait.Until(d => d.FindElement(By.Id("gg-table-1")).Displayed);
                }
                catch { }

                Thread.Sleep(700);

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
                int IsMTT = rows["IsMTT"] != DBNull.Value ? Convert.ToInt32(rows["IsMTT"]) : 0;
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
                {
                    progressPanel1.Caption= "Không tìm thấy hóa đơn";
                    return;
                }
                   

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
                    // Lấy thuế suất an toàn ra biến trước
                    double taxRate = 0;
                    if (row["Thuesuat"] != DBNull.Value && row["Thuesuat"] != null)
                    {
                        double.TryParse(row["Thuesuat"].ToString(), out taxRate);
                    }
                    else
                    {
                        taxRate = kq2.Rows[0]["TyLe"] != DBNull.Value ? Convert.ToDouble(kq2.Rows[0]["TyLe"]) : 0;
                    }

                        products.Add(new Product
                        {
                            Name = Helpers.ConvertVniToUnicode(kqHH.Rows[0]["TenVattu"].ToString()),
                            Unit = Helpers.ConvertVniToUnicode(kqHH.Rows[0]["DonVi"].ToString()),
                            Qty = soluong,
                            Price = soluong == 0 ? 0 : Math.Round(sops / soluong),
                            TaxRate = taxRate,
                        });
                }

                // 4. Đăng nhập
                Console.WriteLine("Đang đăng nhập Bkav...");
                progressPanel1.Caption = "Đang đăng nhập Bkav...";
                Application.DoEvents();
                _driver = LoginBkav(username, password,false);

                // 5. Xóa nháp cũ nếu có
                string idnhap = kq2.Rows[0]["IdNhap"].ToString();
                if (!string.IsNullOrEmpty(idnhap) && idnhap!="0")
                {
                    Console.WriteLine("Đang xóa nháp cũ: " + idnhap);
                    TryDeleteDraftMTT(idnhap, IsMTT);
                }

                // 6. Tạo nháp mới
                string invoiceGuid = CreateDraftMTT(
                    products,
                    dtKhachhang.Rows[0]["MST"].ToString(),
                    formattedDate,
                    Convert.ToDouble(kq2.Rows[0]["TyLe"] ?? 0),
                    IsMTT);
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
        private bool TryDeleteDraftMTT(string idnhap, int IsMTT)
        {
            if (string.IsNullOrEmpty(idnhap) || idnhap == "0") return true;

            try
            {
                Console.WriteLine($"Đang kiểm tra và xóa nháp cũ: {idnhap}...");
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

                // 1. Chuyển sang trang MTT
                if (IsMTT == 1)
                {
                    _driver.Navigate().GoToUrl(BASE_URL + "MTT?IsMTT=true");
                    Thread.Sleep(2500);
                }

                // 2. ƯU TIÊN NÚT SỬA (aeditInvoice) — KHÔNG lấy nút Xem
                var editBtns = _driver.FindElements(By.XPath(
                    $"//a[contains(@onclick, '{idnhap.ToLower()}') and contains(@class, 'aeditInvoice')]"));

                if (editBtns.Count == 0)
                {
                    Console.WriteLine("-> Không tìm thấy nút SỬA của nháp cũ. Bỏ qua.");
                    return true;
                }

                // ✅ Verify onclick đúng idnhap (chống match nhầm chuỗi con)
                var onclick = editBtns[0].GetAttribute("onclick") ?? "";
                Console.WriteLine($"[Del] Nút Sửa onclick = {onclick}");
                if (!onclick.ToLower().Contains(idnhap.ToLower()))
                {
                    Console.WriteLine("[Del] ⚠️ Nút không khớp idnhap — KHÔNG xoá.");
                    return false;
                }

                // 3. Bấm nút Sửa
                ((IJavaScriptExecutor)_driver).ExecuteScript(
                    "arguments[0].scrollIntoView(true); arguments[0].click();", editBtns[0]);

                // 4. Chờ popup Sửa mở
                Thread.Sleep(2000);
                try
                {
                    wait.Until(d => d.FindElement(By.Id("framedialogInvoiceNewEdit")).Displayed);
                    _driver.SwitchTo().Frame("framedialogInvoiceNewEdit");
                }
                catch
                {
                    Console.WriteLine("-> Lỗi: Không mở được popup Sửa hoá đơn.");
                    return false;
                }

                // 5. Bấm nút Xoá
                var btnDelete = wait.Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(By.Id("btnDeleteInvoice"));
                        return el.Displayed && el.Enabled ? el : null;
                    }
                    catch { return null; }
                });

                if (btnDelete == null)
                {
                    Console.WriteLine("-> Không thấy nút Xoá (có thể đang ở popup Xem thay vì Sửa).");
                    return false;
                }

                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btnDelete);

                // 6. Xác nhận xoá
                Thread.Sleep(1500);
                try
                {
                    var btnConfirm = _driver.FindElement(By.XPath("//button[contains(text(), 'Xoá khỏi hệ thống')]"));
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btnConfirm);
                }
                catch
                {
                    _driver.SwitchTo().DefaultContent();
                    var btnConfirmOut = wait.Until(d => d.FindElement(By.XPath("//button[contains(text(), 'Xoá khỏi hệ thống')]")));
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btnConfirmOut);
                }

                Console.WriteLine("✅ Đã xóa nháp cũ thành công.");
                Thread.Sleep(2000);
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
        private async void BkavInvoice_Load(object sender, EventArgs e)
        {
            await Task.Yield();

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

            if (_content.Contains("Huy"))
            {
                 Huyhoadon(_content.Replace("Huy_", ""));
            }
            else
            {
                if (_content.Contains("PH_"))
                    PhatHanhHD();
                else
                    Addinvoice();
            }
          
        }
        public void Huyhoadon(string idnhap)
        {
            try
            {
                progressPanel1.Invoke(new Action(() =>
                {
                    progressPanel1.Caption = "Đang huỷ hoá đơn BKAV...";
                    progressPanel1.Refresh();      // ép vẽ lại
                    Application.DoEvents();        // xử lý message queue
                }));
                var dtInvoiceInfo = ExecuteQuery("SELECT * FROM tbInvoiceInfo", null);
                var rows = dtInvoiceInfo.Rows[0];
                string username = rows["Username"]?.ToString();
                string password = rows["Password"]?.ToString();
                int IsMTT = rows["IsMTT"] != DBNull.Value ? Convert.ToInt32(rows["IsMTT"]) : 0;
                _driver = LoginBkav(username, password,false);
                Console.WriteLine("Đang xóa nháp cũ: " + idnhap);
                TryDeleteDraftMTT(idnhap, IsMTT);
                _driver.Quit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi: " + ex.Message);
                XtraMessageBox.Show("Lỗi: " + ex.Message);

            }
            finally
            {
                try { _driver?.Quit(); } catch { }
                _driver = null;
                this.Close();
            }
        }

        // ================================================================
        // ============ PHÁT HÀNH (giữ nguyên code cũ) ============
        // ================================================================
        // Thêm các using cần thiết ở đầu file nếu chưa có:
        // using System.Text.RegularExpressions;

        private void PhatHanhHD()
        {
            try
            {
                // 1. Lấy thông tin đăng nhập
                var dtInfo = ExecuteQuery("SELECT * FROM tbInvoiceInfo", null);
                var row = dtInfo.Rows[0];
                string username = row["Username"]?.ToString();
                string password = row["Password"]?.ToString();
                int IsMTT = row["IsMTT"] != DBNull.Value ? Convert.ToInt32(row["IsMTT"]) : 0;

                // 2. Đăng nhập
                SetCaption("Đang đăng nhập Bkav...");
                _driver = LoginBkav(username, password,true);
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

                // 3. Lấy GUID từ file txt (định dạng: PH_GUID_...)
                var getsplit = _content.Split('_');
                if (getsplit.Length < 2) throw new Exception("Định dạng file invoice.txt không hợp lệ.");
                string invoiceGuid = getsplit[1].Trim().ToLower();

                // 4. Mở trang danh sách hóa đơn (Bắt buộc phải qua danh sách để kích hoạt đúng luồng)
                SetCaption("Đang mở danh sách hóa đơn...");
                string listUrl = IsMTT == 1 ? BASE_URL + "MTT?IsMTT=true" : BASE_URL + "QLHD";
                _driver.Navigate().GoToUrl(listUrl);
                wait.Until(d => d.Url.Contains(IsMTT == 1 ? "MTT" : "QLHD"));
                Thread.Sleep(2000); // Chờ grid load

                // 5. Tìm đúng dòng hóa đơn và bấm nút "Xem Hóa Đơn" (viewInvoice)
                SetCaption("Đang tìm và mở hóa đơn nháp...");
                IWebElement btnView = null;
                try
                {
                    // Tìm thẻ <tr> có chứa GUID trong thuộc tính data-invoiceguid hoặc trong HTML
                    // Sau đó tìm nút có class 'viewInvoice' bên trong dòng đó.
                    btnView = wait.Until(d =>
                    {
                        try
                        {
                            // Cách 1: Tìm theo data-invoiceguid (chuẩn nhất)
                            var rows = d.FindElements(By.CssSelector($"tr[data-invoiceguid='{invoiceGuid}']"));
                            if (rows.Count > 0)
                            {
                                var btn = rows[0].FindElements(By.CssSelector(".viewInvoice, [class*='viewInvoice']"));
                                if (btn.Count > 0 && btn[0].Displayed) return btn[0];
                            }

                            // Cách 2: Fallback tìm theo onclick chứa GUID và class viewInvoice
                            var fallback = d.FindElements(By.XPath($"//a[contains(@onclick, '{invoiceGuid}') and contains(@class, 'viewInvoice')]"));
                            if (fallback.Count > 0 && fallback[0].Displayed) return fallback[0];

                            return null;
                        }
                        catch { return null; }
                    });
                }
                catch
                {
                    throw new Exception($"Không tìm thấy nút Xem Hóa Đơn cho GUID: {invoiceGuid}. Hãy kiểm tra lại trạng thái nháp trên Bkav.");
                }

                // Cuộn tới và click bằng JS để tránh bị che
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'}); arguments[0].click();", btnView);
                Console.WriteLine("Đã bấm nút Xem Hóa Đơn.");

                // 6. Chờ popup chi tiết mở ra
                SetCaption("Đang chờ cửa sổ hóa đơn mở...");
                Thread.Sleep(2000);

                // 7. Chuyển vào iframe để bấm nút Phát hành/Ký số
                // Bkav thường dùng iframe có ID là 'framedialogInvoiceNewEdit' hoặc tương tự
                bool inIframe = false;
                try
                {
                    wait.Until(d => d.FindElements(By.Id("framedialogInvoiceNewEdit")).Count > 0);
                    _driver.SwitchTo().Frame("framedialogInvoiceNewEdit");
                    inIframe = true;
                }
                catch
                {
                    try
                    {
                        // Thử tìm iframe tổng quát trong dialog
                        var iframe = wait.Until(d =>
                        {
                            var els = d.FindElements(By.CssSelector("div.ui-dialog iframe"));
                            return els.Count > 0 && els[0].Displayed ? els[0] : null;
                        });
                        _driver.SwitchTo().Frame(iframe);
                        inIframe = true;
                    }
                    catch
                    {
                        throw new Exception("Không tìm thấy khung cửa sổ hóa đơn (iframe).");
                    }
                }

                // 8. Tìm nút Phát hành / Ký số (Cần F12 trên Bkav để lấy chính xác ID)
                // Thường là btnPublish, btnSign, btnRelease, hoặc nút có text "Phát hành"
                SetCaption("Đang tìm nút Phát hành/Ký số...");
                IWebElement btnPublish = null;
                try
                {
                    btnPublish = wait.Until(d =>
                    {
                        // Ưu tiên ID (Bkav thường dùng ID cố định)
                        var byId = d.FindElements(By.Id("btnPublish"));
                        if (byId.Count == 0) byId = d.FindElements(By.Id("btnSign"));
                        if (byId.Count == 0) byId = d.FindElements(By.Id("btnRelease"));
                        if (byId.Count > 0 && byId[0].Displayed) return byId[0];

                        // Fallback theo text (ít chính xác hơn)
                        var byText = d.FindElements(By.XPath("//button[contains(text(), 'Phát hành')] | //a[contains(text(), 'Phát hành')] | //input[contains(@value, 'Phát hành')]"));
                        if (byText.Count > 0 && byText[0].Displayed) return byText[0];

                        return null;
                    });
                }
                catch
                {
                    throw new Exception("Không tìm thấy nút Phát hành. Có thể giao diện Bkav đã thay đổi, hãy F12 để kiểm tra lại ID nút.");
                }

                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btnPublish);
                Console.WriteLine("Đã bấm nút Phát hành. Chờ xử lý...");

                // 9. Xử lý popup xác nhận (nếu có)
                Thread.Sleep(1500);
                try
                {
                    _driver.SwitchTo().DefaultContent(); // Về trang chính để tìm popup confirm
                    var btnConfirm = _driver.FindElements(By.XPath("//button[contains(text(), 'Đồng ý')] | //button[contains(text(), 'Xác nhận')] | //button[contains(@class, 'btn-primary')]"));
                    if (btnConfirm.Count > 0 && btnConfirm[0].Displayed)
                    {
                        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btnConfirm[0]);
                        Console.WriteLine("Đã xác nhận popup.");
                    }
                }
                catch
                {
                    Console.WriteLine("Không có popup xác nhận hoặc đã tự động xử lý.");
                }

                // 10. CHỜ KẾT QUẢ KÝ SỐ (Quan trọng: Không dùng Thread.Sleep cứng)
                SetCaption("Đang chờ ký số (Vui lòng cắm USB Token và nhập mã PIN)...");
                Console.WriteLine("Đang chờ ký số...");

                // Chờ tối đa 60 giây để xem có thông báo thành công hoặc lỗi không
                bool isSuccess = false;
                bool isError = false;
                string errorMessage = "";

                var waitResult = new WebDriverWait(_driver, TimeSpan.FromSeconds(60));
                try
                {
                    waitResult.Until(d =>
                    {
                        try
                        {
                            // Tìm thông báo thành công (Bkav thường hiện alert hoặc text "Đã phát hành")
                            string pageSource = d.PageSource.ToLower();
                            if (pageSource.Contains("đã phát hành") || pageSource.Contains("phát hành thành công") || pageSource.Contains("cấp số hóa đơn"))
                            {
                                isSuccess = true;
                                return true;
                            }

                            // Tìm thông báo lỗi (sai PIN, hết hạn token, lỗi kết nối...)
                            var errEl = d.FindElements(By.CssSelector(".error, .alert-danger, .toast-error"));
                            if (errEl.Count > 0 && errEl[0].Displayed)
                            {
                                isError = true;
                                errorMessage = errEl[0].Text;
                                return true;
                            }

                            // Nếu popup đã đóng và quay về danh sách, có thể đã xong
                            if (d.Url.Contains("MTT") || d.Url.Contains("QLHD"))
                            {
                                // Kiểm tra xem GUID còn ở trạng thái nháp không
                                var rows = d.FindElements(By.CssSelector($"tr[data-invoiceguid='{invoiceGuid}']"));
                                if (rows.Count > 0)
                                {
                                    string rowText = rows[0].Text.ToLower();
                                    if (rowText.Contains("đã phát hành") || rowText.Contains("đph"))
                                    {
                                        isSuccess = true;
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }
                        catch { return false; }
                    });
                }
                catch (WebDriverTimeoutException)
                {
                    // Hết thời gian chờ
                }

                if (isSuccess)
                {
                    SetCaption("Phát hành thành công!");
                            ExecuteQueryResult(@"UPDATE HoaDon SET StatusPH = ? WHERE IdTemplate = ?", new OleDbParameter[]
                     {
                            new OleDbParameter("?", "1"),
                            new OleDbParameter("?", invoiceGuid),
                     });
                    MessageBox.Show("Đã hoàn tất thao tác Phát hành hóa đơn!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (isError)
                {
                    SetCaption("Phát hành thất bại!");
                    MessageBox.Show($"Lỗi phát hành: {errorMessage}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    // Trường hợp không rõ ràng, có thể do USB Token chưa cắm hoặc chưa ký xong
                    SetCaption("Chờ xác nhận...");
                    MessageBox.Show("Đã gửi lệnh phát hành nhưng chưa nhận được xác nhận tự động.\n\nVui lòng kiểm tra trực tiếp trên trình duyệt Bkav xem hóa đơn đã được cấp số chưa.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi phát hành: " + ex.Message);
                SetCaption("Lỗi phát hành!");
                MessageBox.Show("Lỗi phát hành: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Không tự động đóng Chrome ngay nếu đang chờ ký, nhưng nếu đã xong thì đóng
                try
                {
                    if (_driver != null)
                    {
                        _driver.Quit();
                    }
                }
                catch { }
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