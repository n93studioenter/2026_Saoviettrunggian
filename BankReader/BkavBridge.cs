using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SaovietTax.BankReader
{
    #region Exceptions

    public class BkavError : Exception
    {
        public BkavError(string message) : base(message) { }
        public BkavError(string message, Exception inner) : base(message, inner) { }
    }

    public class BkavLoginRequired : BkavError
    {
        public BkavLoginRequired(string message = "Phiên Bkav chưa đăng nhập hoặc đã hết hạn.")
            : base(message) { }
    }

    public class BkavHttpError : BkavError
    {
        public int Status { get; private set; }
        public string Action { get; private set; }
        public string ResponseText { get; private set; }

        public BkavHttpError(int status, string action, string responseText = "")
            : base(string.Format("Bkav trả về HTTP {0} khi {1}.", status, action))
        {
            Status = status;
            Action = action ?? "";
            ResponseText = responseText ?? "";
        }
    }

    #endregion

    #region Constants & Helpers

    public static class BkavConst
    {
        public const string BaseUrl = "https://van.ehoadon.vn/";
        public const string ZeroGuid = "00000000-0000-0000-0000-000000000000";
        public static readonly Regex GuidRe = new Regex(
            @"\b[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    public static class BkavHelper
    {
        public static decimal D(object v)
        {
            if (v == null || v is DBNull) return 0m;
            return Convert.ToDecimal(v, CultureInfo.InvariantCulture);
        }

        public static decimal VndRound(object v)
        {
            return Math.Round(D(v), 0, MidpointRounding.AwayFromZero);
        }

        public static string ViMoney(object v)
        {
            return ((long)VndRound(v)).ToString("N0", CultureInfo.InvariantCulture).Replace(",", ".");
        }

        public static string ViDecimal(object v, int decimals = 3)
        {
            var d = D(v);
            var s = d.ToString("N" + decimals, CultureInfo.InvariantCulture); // 1,234.560
            var parts = s.Split('.');
            var whole = parts[0].Replace(",", ".");
            var frac = parts.Length > 1 ? parts[1].TrimEnd('0') : "";
            return string.IsNullOrEmpty(frac) ? whole : whole + "," + frac;
        }

        public static string NonZeroGuid(string source)
        {
            if (string.IsNullOrEmpty(source)) return "";
            foreach (Match m in BkavConst.GuidRe.Matches(source))
            {
                var g = m.Value.ToLowerInvariant();
                if (g != BkavConst.ZeroGuid) return g;
            }
            return "";
        }

        public static string PayMethodId(string value)
        {
            var t = Regex.Replace((value ?? "").Trim().ToLowerInvariant(), @"\s+", " ");
            if (t == "tm" || (t.Contains("tiền mặt") && !t.Contains("chuyển"))) return "1";
            if (t == "ck" || t.Contains("chuyển khoản") || t.Contains("ủy nhiệm")) return "2";
            if (t.Contains("tm/ck") || t.Contains("tiền mặt/chuyển")) return "3";
            return "3";
        }

        public static string FallbackTaxId(decimal rate, string taxCode = "")
        {
            var code = (taxCode ?? "").Trim().ToUpperInvariant();
            if (code == "EXEMPT") return "4";
            if (Math.Abs(rate) < 0.0001m) return "1";
            if (Math.Abs(rate - 5m) < 0.0001m) return "2";
            if (Math.Abs(rate - 10m) < 0.0001m) return "3";
            if (Math.Abs(rate - 8m) < 0.0001m) return "9";
            return "6";
        }

        public static bool ValidInvoiceSerial(string value)
        {
            var t = (value ?? "").Trim();
            if (t.Length < 3 || t.Length > 8) return false;
            if (!Regex.IsMatch(t, @"^[A-Za-z0-9][A-Za-z0-9._/\-]{2,7}$")) return false;
            return Regex.IsMatch(t, "[A-Za-z]") && Regex.IsMatch(t, @"\d");
        }

        public static string CleanText(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            var t = HttpUtility.HtmlDecode(value).Replace("\0", " ");
            t = Regex.Replace(t, @"\s+", " ").Trim(' ', '\t', '\r', '\n', '"', '\'');
            if (t.Equals("null", StringComparison.OrdinalIgnoreCase) ||
                t.Equals("none", StringComparison.OrdinalIgnoreCase) ||
                t.Equals("undefined", StringComparison.OrdinalIgnoreCase))
                return "";
            return t;
        }
    }

    #endregion

    #region Form parser

    public class AspNetForm
    {
        public Dictionary<string, string> Fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, List<KeyValuePair<string, string>>> Selects =
            new Dictionary<string, List<KeyValuePair<string, string>>>(StringComparer.OrdinalIgnoreCase);
    }

    public static class FormParser
    {
        public static AspNetForm Parse(string html)
        {
            var form = new AspNetForm();
            if (string.IsNullOrEmpty(html)) return form;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            foreach (var input in doc.DocumentNode.SelectNodes("//input[@name]") ?? Enumerable.Empty<HtmlNode>())
            {
                var name = input.GetAttributeValue("name", "");
                if (string.IsNullOrEmpty(name)) continue;
                var type = (input.GetAttributeValue("type", "text") ?? "text").ToLowerInvariant();
                if ((type == "checkbox" || type == "radio") && input.Attributes["checked"] == null)
                    continue;
                form.Fields[name] = HttpUtility.HtmlDecode(input.GetAttributeValue("value", "") ?? "");
            }

            foreach (var ta in doc.DocumentNode.SelectNodes("//textarea[@name]") ?? Enumerable.Empty<HtmlNode>())
            {
                var name = ta.GetAttributeValue("name", "");
                if (!string.IsNullOrEmpty(name))
                    form.Fields[name] = HttpUtility.HtmlDecode(ta.InnerText ?? "");
            }

            foreach (var sel in doc.DocumentNode.SelectNodes("//select[@name]") ?? Enumerable.Empty<HtmlNode>())
            {
                var name = sel.GetAttributeValue("name", "");
                if (string.IsNullOrEmpty(name)) continue;
                var options = new List<KeyValuePair<string, string>>();
                string selected = null;
                foreach (var opt in sel.SelectNodes(".//option") ?? Enumerable.Empty<HtmlNode>())
                {
                    var val = opt.GetAttributeValue("value", "") ?? "";
                    var text = HttpUtility.HtmlDecode(opt.InnerText ?? "").Trim();
                    options.Add(new KeyValuePair<string, string>(val, text));
                    if (opt.Attributes["selected"] != null)
                        selected = val;
                }
                form.Selects[name] = options;
                if (selected != null || !form.Fields.ContainsKey(name))
                    form.Fields[name] = selected ?? (options.Count > 0 ? options[0].Key : "");
            }
            return form;
        }

        public static string SuffixField(IDictionary<string, string> fields, string suffix)
        {
            foreach (var name in fields.Keys)
            {
                if (name.Equals(suffix, StringComparison.OrdinalIgnoreCase) ||
                    name.EndsWith("$" + suffix, StringComparison.OrdinalIgnoreCase) ||
                    name.EndsWith(":" + suffix, StringComparison.OrdinalIgnoreCase))
                    return name;
            }
            throw new BkavError("Bkav đã thay đổi biểu mẫu: không tìm thấy trường " + suffix);
        }

        public static string OptionalSuffix(IDictionary<string, string> fields, params string[] suffixes)
        {
            foreach (var s in suffixes)
            {
                try { return SuffixField(fields, s); }
                catch { }
            }
            return "";
        }
    }

    #endregion

    #region Main Bridge

    public class BkavBridge : IDisposable
    {
        private readonly string _profileDir;
        private readonly Action<string, IDictionary<string, object>> _onEvent;
        private readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();
        private readonly ManualResetEventSlim _stop = new ManualResetEventSlim(false);
        private readonly object _lock = new object();
        private Thread _thread;
        private IWebDriver _driver;
        private ChromeDriverService _service;
        private bool _headless = true;
        private string _mode = "MTT"; // MTT | STANDARD
        private bool _busy;

        public BkavBridge(string profileDir, Action<string, IDictionary<string, object>> onEvent = null)
        {
            _profileDir = string.IsNullOrEmpty(profileDir)
                ? Path.Combine(Path.GetTempPath(), "bkav_profile")
                : profileDir;
            Directory.CreateDirectory(_profileDir);
            _onEvent = onEvent;
        }

        public void Emit(string name, IDictionary<string, object> data = null)
        {
            try { if (_onEvent != null) _onEvent(name, data ?? new Dictionary<string, object>()); }
            catch { }
        }

        public void SetMode(string mode)
        {
            mode = (mode ?? "MTT").Trim().ToUpperInvariant();
            _mode = (mode == "STANDARD") ? "STANDARD" : "MTT";
        }

        public bool IsMtt { get { return _mode == "MTT"; } }

        #region Thread / Chrome lifecycle

        private void EnsureChrome(bool headless)
        {
            lock (_lock)
            {
                if (_thread != null && _thread.IsAlive)
                {
                    if (_headless == headless || (!_headless && headless)) return;
                    _stop.Set();
                    _thread.Join(10000);
                }
                _stop.Reset();
                _headless = headless;
                _thread = new Thread(Worker) { IsBackground = true, Name = "BkavBridge" };
                _thread.Start();
            }
        }

        private void Worker()
        {
            try
            {
                var opt = new ChromeOptions();
                opt.AddArgument("--disable-gpu");
                opt.AddArgument("--no-sandbox");
                opt.AddArgument("--disable-dev-shm-usage");
                opt.AddArgument("--user-data-dir=" + _profileDir);
                if (_headless) opt.AddArgument("--headless=new");

                _service = ChromeDriverService.CreateDefaultService();
                _service.HideCommandPromptWindow = true;
                _driver = new ChromeDriver(_service, opt);
                _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);
                _driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(45);

                while (!_stop.IsSet)
                {
                    Action job;
                    if (_queue.TryDequeue(out job))
                    {
                        try { job(); }
                        catch (Exception ex)
                        {
                            Emit("error", new Dictionary<string, object> {
                                { "message", ex.Message },
                                { "type", ex.GetType().Name }
                            });
                        }
                        finally { lock (_lock) { _busy = false; } }
                    }
                    else Thread.Sleep(80);
                }
            }
            finally
            {
                try { if (_driver != null) _driver.Quit(); } catch { }
                try { if (_service != null) _service.Dispose(); } catch { }
                _driver = null;
            }
        }

        private void Enqueue(Action job, bool headless = true)
        {
            lock (_lock)
            {
                if (_busy) throw new BkavError("Đang xử lý một thao tác Bkav khác.");
                _busy = true;
            }
            try
            {
                EnsureChrome(headless);
                _queue.Enqueue(job);
            }
            catch
            {
                lock (_lock) { _busy = false; }
                throw;
            }
        }

        #endregion

        #region A. Login + PostJson

        public void EnsureLogin(string username, string password)
        {
            if (_driver == null) throw new BkavError("Chrome chưa sẵn sàng.");

            _driver.Navigate().GoToUrl(BkavConst.BaseUrl);
            Thread.Sleep(700);

            var url = (_driver.Url ?? "").ToLowerInvariant();
            if (!url.Contains("/login") &&
                !url.EndsWith("ehoadon.vn/") &&
                !url.EndsWith("ehoadon.vn"))
                return; // đã login

            try
            {
                var userEl = FindFirst(
                    By.CssSelector("input[name*='UserName']"),
                    By.CssSelector("input[id*='UserName']"),
                    By.CssSelector("input[type='text']"));
                var passEl = FindFirst(
                    By.CssSelector("input[name*='Password']"),
                    By.CssSelector("input[id*='Password']"),
                    By.CssSelector("input[type='password']"));

                userEl.Clear(); userEl.SendKeys(username ?? "");
                passEl.Clear(); passEl.SendKeys(password ?? "");

                var btn = _driver.FindElements(By.CssSelector(
                    "input[type='submit'], button[type='submit'], .btn-login, #btnLogin")).FirstOrDefault();
                if (btn != null) btn.Click();
                else ((IJavaScriptExecutor)_driver).ExecuteScript("document.forms[0].submit();");

                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(25));
                wait.Until(d => !(d.Url ?? "").ToLowerInvariant().Contains("/login"));
            }
            catch (Exception ex)
            {
                throw new BkavLoginRequired("Không đăng nhập được Bkav: " + ex.Message);
            }
        }

        private IWebElement FindFirst(params By[] bys)
        {
            foreach (var by in bys)
            {
                var list = _driver.FindElements(by);
                if (list.Count > 0) return list[0];
            }
            throw new NoSuchElementException("Không tìm thấy control đăng nhập.");
        }

        /// <summary>
        /// Gọi API JSON của Bkav, giữ cookie phiên đăng nhập của Chrome.
        /// </summary>
        public JToken PostJson(string relativeOrAbsoluteUrl, object payload, string action)
        {
            var url = relativeOrAbsoluteUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? relativeOrAbsoluteUrl
                : BkavConst.BaseUrl.TrimEnd('/') + relativeOrAbsoluteUrl;

            var body = JsonConvert.SerializeObject(payload ?? new { });

            const string script = @"
                var callback = arguments[arguments.length - 1];
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
                        callback({ status: r.status, text: t, url: r.url });
                    });
                }).catch(function(e) {
                    callback({ status: 0, text: String(e), url: url });
                });
            ";

            var raw = ((IJavaScriptExecutor)_driver).ExecuteAsyncScript(script, url, body)
                      as Dictionary<string, object>;
            if (raw == null)
                throw new BkavError("Không nhận được phản hồi khi " + action);

            var status = Convert.ToInt32(raw["status"]);
            var text = Convert.ToString(raw["text"] ?? "");

            if (status == 401 || status == 403)
                throw new BkavLoginRequired();
            if (status >= 400)
                throw new BkavHttpError(status, action, text);

            return UnwrapResult(text, action);
        }

        private static JToken UnwrapResult(string responseText, string action)
        {
            try
            {
                var payload = JToken.Parse(responseText);
                var result = payload["d"] ?? payload;

                var obj = result as JObject;
                if (obj != null)
                {
                    var isOk = obj.Value<bool?>("isOk") ?? obj.Value<bool?>("IsOk");
                    var isError = obj.Value<bool?>("isError") ?? obj.Value<bool?>("IsError");
                    if (isOk == false || isError == true)
                    {
                        var msg = (obj["Code"] ?? obj["Message"] ?? obj["Object"] ?? ("Không thể " + action + ".")).ToString();
                        throw new BkavError(msg);
                    }
                    if (obj["Object"] != null) return obj["Object"];
                    if (obj["Result"] != null) return obj["Result"];
                    if (obj["Data"] != null) return obj["Data"];
                }
                return result;
            }
            catch (BkavError) { throw; }
            catch
            {
                throw new BkavLoginRequired();
            }
        }

        #endregion

        #region B. Create Draft

        /// <summary>
        /// document keys gợi ý:
        /// id, doc_date, buyer_name, buyer_tax_code, buyer_address, buyer_unit_name,
        /// payment_method, lines = List of { name, unit, qty, unit_price, tax_rate }
        /// invoice_config = { template_id, invoice_form, serial } (optional)
        /// </summary>
        public IDictionary<string, object> CreateDraft(
            IDictionary<string, object> document,
            string username, string password,
            bool headless = true)
        {
            IDictionary<string, object> result = null;
            Exception error = null;
            var done = new ManualResetEventSlim(false);

            Enqueue(() =>
            {
                try
                {
                    result = CreateDraftCore(document, username, password);
                }
                catch (Exception ex) { error = ex; }
                finally { done.Set(); }
            }, headless);

            done.Wait();
            if (error != null) throw error;
            return result;
        }

        private IDictionary<string, object> CreateDraftCore(
            IDictionary<string, object> document, string username, string password)
        {
            if (document == null || !document.ContainsKey("lines"))
                throw new BkavError("Chứng từ chưa có dòng hàng hóa.");

            var lines = document["lines"] as IList<object>;
            if (lines == null || lines.Count == 0)
                throw new BkavError("Chứng từ chưa có dòng hàng hóa.");

            Emit("progress", new Dictionary<string, object> { { "message", "Đang kiểm tra phiên đăng nhập Bkav..." } });
            EnsureLogin(username, password);

            // TODO: chọn template + serial theo năm / cấu hình (rút gọn: lấy từ document.invoice_config)
            var cfg = document.ContainsKey("invoice_config")
                ? document["invoice_config"] as IDictionary<string, object>
                : null;

            string templateId = cfg != null && cfg.ContainsKey("template_id") ? Convert.ToString(cfg["template_id"]) : "";
            string invoiceForm = cfg != null && cfg.ContainsKey("invoice_form") ? Convert.ToString(cfg["invoice_form"]) : "";
            string serial = cfg != null && cfg.ContainsKey("serial") ? Convert.ToString(cfg["serial"]) : "";

            if (string.IsNullOrEmpty(templateId) || string.IsNullOrEmpty(serial))
                throw new BkavError("Thiếu template_id hoặc serial trong invoice_config.");

            var invoice = BuildInvoice(document, templateId, invoiceForm, serial);
            Emit("progress", new Dictionary<string, object> { { "message", "Đang khởi tạo trang hóa đơn Bkav..." } });

            OpenInvoiceParentPage(invoice);

            string guid = BkavConst.ZeroGuid;
            string listBillId = "";
            decimal expectedGoods = 0m, expectedTax = 0m;

            int index = 0;
            foreach (var rawLine in lines)
            {
                index++;
                var line = rawLine as IDictionary<string, object>;
                if (line == null) continue;

                invoice["InvoiceGUID"] = guid;

                var modalPayload = new Dictionary<string, object>
                {
                    { "Invoice", invoice },
                    { "InvoiceDetailID", 0 },
                    { "ItemTypeID", 0 },
                    { "TypeCreateInvoice", 0 },
                    { "InvoiceOrgWithOutSystem", null }
                };
                if (!string.IsNullOrEmpty(listBillId))
                    modalPayload["ListBillID"] = listBillId;

                // Lấy HTML form chi tiết
                var modalHtml = PostHtmlJson(modalPayload);
                var formData = BuildDetailForm(modalHtml, line, invoice);

                listBillId = UpgradeListBillId(listBillId, formData.ListBillId);
                var detailResp = PostDetailForm(formData.Fields);
                listBillId = UpgradeListBillId(listBillId, ExtractListBillId(detailResp));

                if (guid == BkavConst.ZeroGuid)
                {
                    var g = BkavHelper.NonZeroGuid(Convert.ToString(detailResp));
                    if (!string.IsNullOrEmpty(g)) guid = g;
                }

                expectedGoods += formData.Amount;
                expectedTax += formData.TaxAmount;

                Emit("progress", new Dictionary<string, object> {
                    { "message", string.Format("Đã chuyển dòng hàng {0}/{1}...", index, lines.Count) }
                });
            }

            invoice["InvoiceGUID"] = guid;

            if (guid == BkavConst.ZeroGuid && !BillIdHasRealValue(listBillId))
                throw new BkavError("Bkav chưa trả về InvoiceGUID hoặc ListBillID hợp lệ.");

            // Check thuế suất
            PostJson("/WebServices/wsInvoice.asmx/CheckBillTaxRate",
                new { listBillID = listBillId }, "kiểm tra thuế suất");

            // SaveInvoice
            var savePayload = new Dictionary<string, object>
            {
                { "invoice", invoice },
                { "typeCreateInvoice", "0" },
                { "listBillID", listBillId }
            };

            var saved = PostJson("/WebServices/wsInvoice.asmx/SaveInvoice", savePayload, "lưu hóa đơn nháp");
            var savedGuid = BkavHelper.NonZeroGuid(saved != null ? saved.ToString() : "");
            if (string.IsNullOrEmpty(savedGuid) && guid != BkavConst.ZeroGuid)
                savedGuid = guid;
            if (string.IsNullOrEmpty(savedGuid))
                throw new BkavError("Bkav đã nhận lệnh lưu nhưng không trả về mã hóa đơn nháp.");

            guid = savedGuid;

            // Xác minh footer
            decimal actualGoods = expectedGoods, actualTax = expectedTax;
            string warning = "";
            try
            {
                var footer = PostJson("/WebServices/wsInvoice.asmx/LoadFooterInvoice",
                    new { InvoiceGUID = guid }, "xác minh hóa đơn nháp") as JObject;
                if (footer != null)
                {
                    actualGoods = BkavHelper.VndRound(footer.Value<object>("SumItemAmount"));
                    actualTax = BkavHelper.VndRound(footer.Value<object>("SumTaxAmount"));
                    if (actualGoods != expectedGoods || actualTax != expectedTax)
                    {
                        warning = string.Format(
                            "Tổng tiền đọc lại chưa khớp: hàng {0}/{1}, thuế {2}/{3}.",
                            BkavHelper.ViMoney(actualGoods), BkavHelper.ViMoney(expectedGoods),
                            BkavHelper.ViMoney(actualTax), BkavHelper.ViMoney(expectedTax));
                    }
                }
            }
            catch (Exception ex)
            {
                warning = "Chưa đọc lại được tổng tiền (" + ex.Message + ").";
            }

            return new Dictionary<string, object>
            {
                { "document_id", document.ContainsKey("id") ? document["id"] : null },
                { "guid", guid },
                { "template_id", templateId },
                { "template", invoiceForm },
                { "serial", serial },
                { "goods_total", (long)actualGoods },
                { "tax_total", (long)actualTax },
                { "payment_total", (long)(actualGoods + actualTax) },
                { "warning", warning },
                { "list_bill_id", listBillId },
                { "invoice_mode", _mode }
            };
        }

        private Dictionary<string, object> BuildInvoice(
            IDictionary<string, object> doc, string templateId, string form, string serial)
        {
            return new Dictionary<string, object>
            {
                { "InvoiceGUID", BkavConst.ZeroGuid },
                { "OriginalInvoiceGUID", BkavConst.ZeroGuid },
                { "InvoiceStatusID", 1 },
                { "InvoiceTypeID", "1" },
                { "SourceID", "1" },
                { "InvoiceTemplateID", templateId },
                { "InvoiceForm", form },
                { "InvoiceSerial", serial },
                { "InvoiceDate", Convert.ToString(doc.ContainsKey("doc_date") ? doc["doc_date"] : DateTime.Now.ToString("yyyy-MM-dd")) },
                { "BuyerName", Convert.ToString(doc.ContainsKey("buyer_name") ? doc["buyer_name"] : "") },
                { "BuyerTaxCode", Convert.ToString(doc.ContainsKey("buyer_tax_code") ? doc["buyer_tax_code"] : "") },
                { "BuyerUnitName", Convert.ToString(doc.ContainsKey("buyer_unit_name") ? doc["buyer_unit_name"] : "") },
                { "BuyerAddress", Convert.ToString(doc.ContainsKey("buyer_address") ? doc["buyer_address"] : "") },
                { "PayMethodID", BkavHelper.PayMethodId(Convert.ToString(doc.ContainsKey("payment_method") ? doc["payment_method"] : "tm/ck")) },
                { "CurrencyID", "VND" },
                { "ExchangeRate", 1 }
            };
        }

        private void OpenInvoiceParentPage(IDictionary<string, object> invoice)
        {
            var qs = new List<string>
            {
                "InvoiceGUID=" + Uri.EscapeDataString(Convert.ToString(invoice["InvoiceGUID"])),
                "InvoiceTypeID=" + Uri.EscapeDataString(Convert.ToString(invoice["InvoiceTypeID"])),
                "SourceID=" + Uri.EscapeDataString(Convert.ToString(invoice["SourceID"])),
                "InvoiceTemplateID=" + Uri.EscapeDataString(Convert.ToString(invoice["InvoiceTemplateID"])),
                "InvoiceSerial=" + Uri.EscapeDataString(Convert.ToString(invoice["InvoiceSerial"])),
                "IsMTT=" + (IsMtt ? "true" : "false")
            };
            var url = BkavConst.BaseUrl.TrimEnd('/') + "/InvoiceNewEdit?" + string.Join("&", qs.ToArray());
            _driver.Navigate().GoToUrl(url);
            Thread.Sleep(600);
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
                wait.Until(d => d.FindElements(By.Id("hdfInvoiceGUID")).Count > 0 ||
                                d.FindElements(By.CssSelector("[id*='InvoiceGUID']")).Count > 0);
            }
            catch { }
        }

        private string PostHtmlJson(object payload)
        {
            // Gọi endpoint trả HTML form chi tiết (tùy bản Bkav có thể khác path)
            // Thực tế Python dùng nhiều biến thể; ở đây gọi qua fetch và lấy text
            var result = PostJson("/WebServices/wsInvoice.asmx/GetUcFormInvoiceDetails", payload, "mở form dòng hàng");
            return result != null ? result.ToString() : "";
        }

        private class DetailFormResult
        {
            public Dictionary<string, string> Fields;
            public decimal Amount;
            public decimal TaxAmount;
            public string ListBillId;
        }

        private DetailFormResult BuildDetailForm(string html, IDictionary<string, object> line, IDictionary<string, object> invoice)
        {
            var form = FormParser.Parse(html);
            var fields = new Dictionary<string, string>(form.Fields, StringComparer.OrdinalIgnoreCase);

            string nameF = FormParser.OptionalSuffix(fields, "txtItemName", "ItemName");
            string unitF = FormParser.OptionalSuffix(fields, "txtUnitName", "UnitName");
            string qtyF = FormParser.OptionalSuffix(fields, "txtQty", "Qty");
            string priceF = FormParser.OptionalSuffix(fields, "txtPrice", "Price");
            string amountF = FormParser.OptionalSuffix(fields, "txtAmount", "Amount");
            string taxRateF = FormParser.OptionalSuffix(fields, "txtTaxRate", "TaxRate");
            string taxAmtF = FormParser.OptionalSuffix(fields, "txtTaxAmount", "TaxAmount");
            string taxSel = FormParser.OptionalSuffix(fields, "ddlTaxRate", "ddlTaxRateID", "TaxRateID");

            decimal qty = BkavHelper.D(line.ContainsKey("qty") ? line["qty"] : 0);
            decimal price = BkavHelper.D(line.ContainsKey("unit_price") ? line["unit_price"] : 0);
            decimal rate = BkavHelper.D(line.ContainsKey("tax_rate") ? line["tax_rate"] : 0);
            decimal amount = BkavHelper.VndRound(qty * price);
            decimal taxAmount = BkavHelper.VndRound(amount * rate / 100m);

            if (!string.IsNullOrEmpty(nameF)) fields[nameF] = Convert.ToString(line.ContainsKey("name") ? line["name"] : "");
            if (!string.IsNullOrEmpty(unitF)) fields[unitF] = Convert.ToString(line.ContainsKey("unit") ? line["unit"] : "");
            if (!string.IsNullOrEmpty(qtyF)) fields[qtyF] = BkavHelper.ViDecimal(qty, 3);
            if (!string.IsNullOrEmpty(priceF)) fields[priceF] = BkavHelper.ViDecimal(price, 0);
            if (!string.IsNullOrEmpty(amountF)) fields[amountF] = BkavHelper.ViMoney(amount);
            if (!string.IsNullOrEmpty(taxRateF)) fields[taxRateF] = rate.ToString(CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(taxAmtF)) fields[taxAmtF] = BkavHelper.ViMoney(taxAmount);
            if (!string.IsNullOrEmpty(taxSel)) fields[taxSel] = BkavHelper.FallbackTaxId(rate);

            // Ẩn các field không cần gửi
            foreach (var key in fields.Keys.ToList())
            {
                var leaf = key.Split('$').Last().Split(':').Last().ToLowerInvariant();
                if (leaf.Contains("btnaddandcontinue") || leaf == "rdbdctt" || leaf.Contains("invformorg"))
                    fields.Remove(key);
            }

            string listBill = ExtractListBillId(fields);

            return new DetailFormResult
            {
                Fields = fields,
                Amount = amount,
                TaxAmount = taxAmount,
                ListBillId = listBill
            };
        }

        private string PostDetailForm(Dictionary<string, string> fields)
        {
            // Gửi form ASP.NET (đơn giản hóa: post JSON nếu Bkav hỗ trợ, hoặc simulate submit)
            // Thực tế Python post multipart/form. Ở đây dùng JS submit trong context trang.
            // Để ổn định hơn, có thể chuyển sang context.request với form-urlencoded.
            var payload = new Dictionary<string, object>();
            foreach (var kv in fields) payload[kv.Key] = kv.Value;

            var result = PostJson("/WebServices/wsInvoice.asmx/SaveInvoiceDetail", payload, "ghi dòng hàng");
            return result != null ? result.ToString() : "";
        }

        private static string ExtractListBillId(object source)
        {
            var text = source is IDictionary<string, string>
                ? string.Join("&", ((IDictionary<string, string>)source).Select(kv => kv.Key + "=" + kv.Value))
                : Convert.ToString(source ?? "");
            var m = Regex.Match(text ?? "",
                @"(?:ListBillID|hdfInvoiceDetailID|hdfListBillID|BillID)\s*[=:]\s*[""']?([^""'&\s<]+)",
                RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value.Trim() : "";
        }

        private static bool BillIdHasRealValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            if (BkavHelper.NonZeroGuid(value) != "") return true;
            return Regex.IsMatch(value, "[1-9]");
        }

        private static string UpgradeListBillId(string existing, string candidate)
        {
            if (string.IsNullOrWhiteSpace(candidate)) return existing ?? "";
            if (BillIdHasRealValue(candidate) && !BillIdHasRealValue(existing))
                return candidate;
            if (string.IsNullOrWhiteSpace(existing)) return candidate;
            var set = new HashSet<string>((existing + "," + candidate).Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries));
            return string.Join(",", set);
        }

        #endregion

        #region C. Update Draft (khung)

        public IDictionary<string, object> UpdateDraft(
            IDictionary<string, object> document, string guid,
            string username, string password, bool headless = true)
        {
            IDictionary<string, object> result = null;
            Exception error = null;
            var done = new ManualResetEventSlim(false);

            Enqueue(() =>
            {
                try { result = UpdateDraftCore(document, guid, username, password); }
                catch (Exception ex) { error = ex; }
                finally { done.Set(); }
            }, headless);

            done.Wait();
            if (error != null) throw error;
            return result;
        }

        private IDictionary<string, object> UpdateDraftCore(
            IDictionary<string, object> document, string guid, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(guid) || !BkavConst.GuidRe.IsMatch(guid))
                throw new BkavError("Mã hóa đơn nháp không hợp lệ.");

            Emit("progress", new Dictionary<string, object> { { "message", "Đang mở hóa đơn nháp cần cập nhật..." } });
            EnsureLogin(username, password);

            var draftUrl = BkavConst.BaseUrl.TrimEnd('/') + "/InvoiceNewEdit?InvoiceGUID=" + guid +
                           (IsMtt ? "&IsMTT=true" : "&IsMTT=false");
            _driver.Navigate().GoToUrl(draftUrl);
            Thread.Sleep(800);

            // Đọc state từ trang
            var state = (Dictionary<string, object>)((IJavaScriptExecutor)_driver).ExecuteScript(@"
                var v = function(id){ var e=document.getElementById(id); return e ? String(e.value||'').trim() : ''; };
                return {
                    guid: v('hdfInvoiceGUID'),
                    status_id: v('hdfInvoiceStatusID'),
                    template_id: v('ddlInvoiceTemplate') || (document.getElementById('ddlInvoiceTemplate')||{}).value || '',
                    list_bill_id: v('hdfListBillID'),
                    original_guid: v('hdfOriginalInvoiceGUID')
                };
            ");

            var actualGuid = Convert.ToString(state.ContainsKey("guid") ? state["guid"] : "");
            if (!string.Equals(actualGuid, guid, StringComparison.OrdinalIgnoreCase))
                throw new BkavError("Bkav không mở đúng hóa đơn nháp cần cập nhật.");

            var statusId = Convert.ToString(state.ContainsKey("status_id") ? state["status_id"] : "");
            if (!string.IsNullOrEmpty(statusId) && statusId != "1")
                throw new BkavError("Hóa đơn không còn ở trạng thái nháp.");

            // Đơn giản hóa: xóa hết dòng cũ rồi ghi lại (an toàn hơn selective phức tạp)
            // (bạn có thể port thêm logic selective từ Python nếu cần tốc độ)

            // Gọi Save phần đầu bằng JS gốc nếu có
            try
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                    if (typeof InitInvoice === 'function') InitInvoice();
                    if (typeof Save === 'function') Save(false);
                ");
                Thread.Sleep(1000);
            }
            catch { }

            // TODO: xóa dòng cũ + ghi dòng mới (dùng cùng BuildDetailForm / PostDetailForm)
            // Để hoàn thiện, cần gọi GetUcFormInvoiceDetails + DeleteInvoiceDetails như Python.

            return new Dictionary<string, object>
            {
                { "document_id", document.ContainsKey("id") ? document["id"] : null },
                { "guid", guid },
                { "updated", true },
                { "invoice_mode", _mode },
                { "note", "Khung UpdateDraft đã mở đúng nháp và Save phần đầu. Cần bổ sung xóa/ghi dòng chi tiết." }
            };
        }

        #endregion

        #region D. Download PDF

        public string DownloadDraftPdf(string guid, string outputPath, string username, string password, bool headless = true)
        {
            string resultPath = null;
            Exception error = null;
            var done = new ManualResetEventSlim(false);

            Enqueue(() =>
            {
                try { resultPath = DownloadPdfCore(guid, outputPath, username, password); }
                catch (Exception ex) { error = ex; }
                finally { done.Set(); }
            }, headless);

            done.Wait();
            if (error != null) throw error;
            return resultPath;
        }

        private string DownloadPdfCore(string guid, string outputPath, string username, string password)
        {
            if (!BkavConst.GuidRe.IsMatch(guid ?? ""))
                throw new BkavError("Mã hóa đơn Bkav không hợp lệ.");

            Emit("progress", new Dictionary<string, object> { { "message", "Đang kiểm tra phiên đăng nhập Bkav..." } });
            EnsureLogin(username, password);

            var viewUrl = BkavConst.BaseUrl.TrimEnd('/') + "/ViewDetailInvoice?InvoiceGUID=" + guid;
            _driver.Navigate().GoToUrl(viewUrl);
            Thread.Sleep(1000);

            // Tìm object PDF
            string pdfUrl = null;
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    pdfUrl = Convert.ToString(((IJavaScriptExecutor)_driver).ExecuteScript(@"
                        var obj = document.querySelector('#idcontent object[type=""application/pdf""], object[data*="".pdf""]');
                        if (!obj) return '';
                        var src = obj.getAttribute('data') || obj.data || '';
                        return src ? new URL(src, location.href).href : '';
                    "));
                    if (!string.IsNullOrEmpty(pdfUrl) && pdfUrl.ToLowerInvariant().Contains(".pdf"))
                        break;
                }
                catch { }
                Thread.Sleep(800 * attempt);
            }

            if (string.IsNullOrEmpty(pdfUrl))
                throw new BkavError("Trang Bkav chưa tạo được đường dẫn PDF hóa đơn.");

            Emit("progress", new Dictionary<string, object> { { "message", "Đang tải file PDF từ Bkav..." } });

            // Fetch PDF bằng JS (giữ cookie)
            var b64 = Convert.ToString(((IJavaScriptExecutor)_driver).ExecuteAsyncScript(@"
                var callback = arguments[arguments.length - 1];
                var url = arguments[0];
                fetch(url, { method: 'GET', credentials: 'include', cache: 'no-store',
                             headers: { 'Accept': 'application/pdf,*/*;q=0.8' } })
                .then(function(r) {
                    if (!r.ok) throw new Error('HTTP ' + r.status);
                    return r.arrayBuffer();
                })
                .then(function(buf) {
                    var bytes = new Uint8Array(buf);
                    var binary = '';
                    var chunk = 0x8000;
                    for (var i = 0; i < bytes.length; i += chunk) {
                        binary += String.fromCharCode.apply(null, bytes.subarray(i, i + chunk));
                    }
                    callback(btoa(binary));
                })
                .catch(function(e) { callback('ERROR:' + String(e)); });
            ", pdfUrl));

            if (string.IsNullOrEmpty(b64) || b64.StartsWith("ERROR:"))
                throw new BkavError("Không tải được PDF: " + b64);

            var data = Convert.FromBase64String(b64);
            if (data.Length < 1000 || data[0] != (byte)'%' || data[1] != (byte)'P')
                throw new BkavError("Bkav không trả về nội dung PDF hợp lệ.");

            var output = Path.GetFullPath(outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var temp = output + ".part";
            File.WriteAllBytes(temp, data);
            if (File.Exists(output)) File.Delete(output);
            File.Move(temp, output);

            return output;
        }

        #endregion

        public void Dispose()
        {
            _stop.Set();
            try { if (_thread != null) _thread.Join(6000); } catch { }
            try { if (_driver != null) _driver.Quit(); } catch { }
            try { if (_service != null) _service.Dispose(); } catch { }
        }
    }

    #endregion
}