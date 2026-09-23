using DevExpress.XtraEditors;
using FuzzySharp;
using Microsoft.ML.OnnxRuntime;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.BiDi.Network;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.ApplicationModel.Contacts;
using static Tensorflow.tensorflow;

namespace SaovietTax
{
   
    public partial class TendoInvoice : Form
    {
        public static class ProductMap
        {
            public static Dictionary<string, string> TedoMap
                = new Dictionary<string, string>()
            {
        { "TRA4312", "SP0079" }, //Bia chai SG Lager 355 ( LAKC) 
        { "TRA4312", "SP0078" }, //Đồ uống SPRITE 600MLb4x6 PET SF FFWC
        { "DO7421", "SP0077" }, //Đồ uống FANTA ORANGE 320ML 4x6 SLEEK CAN SF FFWC
        { "TRA4312", "SP0078" }, //Đồ uống SPRITE 600MLb4x6 PET SF FFWC

            };
        }
        public TendoInvoice()
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
        
        string password, connectionString;
        string _content;
        string bearerToken;
        private static readonly HttpClient httpClient = new HttpClient();

        private void Phathanhhoadon()
        {
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            var options = new ChromeOptions();
            IWebDriver driver = new ChromeDriver(service, options);
            try
            {
                driver.Navigate().GoToUrl("https://id-v2.tendoo.vn/vi/signin");

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                driver.Quit();
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
        private void Addinvoice()
        {
            string qrq = "SELECT * FROM tbInvoiceInfo";
            var dtInvoiceInfo = ExecuteQuery(qrq, null);
            var rows = dtInvoiceInfo.Rows[0];

            string username = rows["Username"]?.ToString();
            string password = rows["Password"]?.ToString();

            string query = "SELECT * FROM tbRegister";
            string pathluu = "";
            var kq = ExecuteQuery(query, null);

            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            var options = new ChromeOptions();
            options.AddArgument("--disable-logging");
            options.AddArgument("--log-level=3");
            options.AddExcludedArgument("enable-automation");
            IWebDriver driver = new ChromeDriver(service, options);
            try
            {
                driver.Navigate().GoToUrl("https://id-v2.tendoo.vn/vi/signin");

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
                var txtUser = wait.Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(By.Id("phone_login_form_phone_number"));
                        return el.Displayed ? el : null;
                    }
                    catch { return null; }
                });

                txtUser.Clear();
                txtUser.SendKeys(username); // 👉 username của bạn

                // ===== PASSWORD =====
                var txtPass = driver.FindElement(By.Id("phone_login_form_pwd"));
                txtPass.Clear();
                txtPass.SendKeys(password);

                IWebElement button = driver.FindElement(By.XPath("//button[.//span[text()='Đăng nhập']]"));
                button.Click();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.Close();
                // driver.Quit();
            }
        }
        string businessId;
        string buyerName;
        string buyerPhone;
        string buyerId ;
        string buyerAddress;
        private async Task LayThongTinInfo()
        {
            progressPanel1.Caption="Đang lấy thông tin hệ thống...";
            //https://apiv2.tendoo.vn/user/api/v2/auth/info
            string apiUrl = "https://apiv2.tendoo.vn/user/api/v2/auth/info";
            HttpResponseMessage response =
                     await httpClient.GetAsync(apiUrl);

            string content =
                await response.Content.ReadAsStringAsync();
            JObject obj = JObject.Parse(content);

            string userId =
    obj["data"]?["user_info"]?["id"]?.ToString();

            businessId =
                obj["data"]?["business_info"]?["current_business"]?["id"]?.ToString();
            LayThongTinHoaDon();
        } 
        private async Task LayThongTinKhachHang()
        {
            buyerId = dtKhachhang.Rows[0]["contact_id"].ToString();
            //Đồng bộ khách hàng trước khi tạo hóa đơn, nếu chưa có contact_id thì tạo contact rồi mới tạo hóa đơn  
            if (string.IsNullOrEmpty(buyerId))
            {
                progressPanel1.Caption = "Đang đồng bộ khách hàng mới lên tendoo...";
                string urlapi = "https://apiv2.tendoo.vn/business/api/v2/contact/create";
                var payload = new
                {
                    is_has_invoice_contact_info = false,
                    invoice_contact_info = new
                    {
                        dvqhns_code = "",
                        bank_account = "",
                        bank_name = "",
                        identification_no = "",
                        email = "",
                        phone_number = "",
                        name = "",
                        full_address = "",
                        business_name = "",
                        tax_number = "",
                        province_id = "",
                        province_name = "",
                        district_id = "",
                        district_name = "",
                        ward_id = "",
                        ward_name = "",
                        address = ""
                    },
                    has_old_debt = false,
                    debt_type = "receivable",
                    group_of_contact_ids = new List<string>(),

                    email = dtKhachhang.Rows[0]["EMail"].ToString(),
                    phone_number = dtKhachhang.Rows[0]["Tel"].ToString(),
                    name = Helpers.ConvertVniToUnicode(dtKhachhang.Rows[0]["Ten"].ToString()),
                    role = "customer",
                    contact_code = dtKhachhang.Rows[0]["SoHieu"].ToString(),
                    birthday = "",
                    tags = new List<string>(),

                    address_info = new
                    {
                        province_id = "HCM",
                        province_name = "Thành phố Hồ Chí Minh",
                        district_id = "",
                        district_name = "",
                        ward_id = "HCM001",
                        ward_name = "Phường Vũng Tàu",
                        address_version = 1,
                        address = Helpers.ConvertVniToUnicode(dtKhachhang.Rows[0]["DiaChi"].ToString())
                    },

                    debt_record_date = (string)null,
                    is_record_transaction = false,
                    full_address1 = "Phường Vũng Tàu, Thành phố Hồ Chí Minh"
                };
                // Chuyển đổi payload thành JSON
                string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(payload);

                // Tạo nội dung request
                var contents = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // Gửi request
                HttpResponseMessage responses = await httpClient.PostAsync(urlapi, contents);
                string result = await responses.Content.ReadAsStringAsync();

                JObject jsonResponse = JObject.Parse(result);
                string contactId = jsonResponse["data"]?["id"]?.ToString();
                buyerId = contactId;
                string query = @"UPDATE KhachHang 
                             SET contact_id = ? 
                             WHERE MaSo = ?";

                var parameters = new OleDbParameter[]
                {
                new OleDbParameter("?", contactId.ToString() ?? ""),
                new OleDbParameter("?", dtKhachhang.Rows[0]["MaSo"]?.ToString() ?? "")
                };

                int rowsAffected = ExecuteQueryResult(query, parameters);
            }
            string apiUrl = $"https://apiv2.tendoo.vn/business/api/v2/contact/get-detail/{buyerId}?business_id={businessId}";
            HttpResponseMessage response =
                     await httpClient.GetAsync(apiUrl);

            string content =
                await response.Content.ReadAsStringAsync();
            JObject obj = JObject.Parse(content);
            buyerName = obj["data"]?["name"]?.ToString();
            buyerPhone = obj["data"]?["phone_number"]?.ToString();
            string address_info = obj["data"]?["address_info"].ToString();
            string avatar = obj["data"]?["avatar"]?.ToString();
            string debt_amount = obj["data"]?["debt_amount"]?.ToString();
            string option ="";
            buyerAddress = obj["data"]?["address"]?.ToString();
            string invoice_contact_info =null;
            string address_version = "0";
            //Truóc khi thực hiện hoá đơn thì sẽ kiểm tra số lượng tồn sản phẩm với số lượng đơn đặt hàng
            progressPanel1.Caption = "Đang kiểm tra số lượng tồn kho...";
            foreach (DataRow item in dtHangHoa.Rows)
            {
                int qty1 = Convert.ToInt32(item["Quantity"].ToString());
                int quantity = string.IsNullOrEmpty(item["TendoQuality"]?.ToString())
      ? 0
      : Convert.ToInt32(item["TendoQuality"].ToString());
                if (quantity < qty1)
                {
                    progressPanel1.Caption = $"Sản phẩm {Helpers.ConvertVniToUnicode(item["ItemName"].ToString())} không đủ tồn kho ({quantity} < {qty1}), đang thực hiện việc tự động nhập kho!";

                    var payload = new
                    {
                        po_type = "in",
                        note = "",
                        po_details = new[]
    {
        new
        {
            business_id = "9065ebd6-4c83-4c9c-a6d4-4937e3fda49b",
            product_id = item["TendoId"].ToString(),
            product_type = "non_variant",
            product_name = item["TendoName"].ToString(),
            sku_name = "",
            sku_code = item["TendoSku"].ToString(),
            sku_type = "stock",
            media = new object[] { },
            total_quantity = 0,
            historical_cost = 0,
            barcode = "",
            uom = item["TendoUom"].ToString(),
            sku_uom_name = item["TendoUom"].ToString(),
            sku_uom_quantity = 1,
            normal_price = 0,
            selling_price = 0,
            total_value = 0,
            unique_id = item["TendoSkuId"].ToString(),
            quantity = qty1 - quantity,  // ← Động
            new_price = 0,
            price_after_discount = 0,
            additional_item_info = new { discount_value = 0, type = "value" },
            price_info = new
            {
                pricing_original = 0,
                pricing_display = 0,
                pricing_calculated = 0,
                item_discount_amount = 0,
                allocated_discount_amount = 0,
                allocated_discount_amount_total = 0,
                internal_allocated_discount_amount = 0,
                total_amount_before_tax = 0,
                total_amount_after_tax = 0,
                total_amount = 0
            },
            pricing = 0,
            total_amount = 0,
            tax_info = new { amount = (object)null },
            sku_id = item["TendoSkuId"].ToString(),
            pricing_original = 0
        }
    },
                        po_detail_ingredient = new object[] { },
                        total_discount = 0,
                        sur_charge = 0,
                        buyer_pay = 0,
                        option = "create_po",
                        media = new object[] { },
                        payment_state = "un_paid",
                        is_debit = false,
                        invoice_type = "vat",
                        created_po_at = (object)null,
                        additional_info = new
                        {
                            discount_value = 0,
                            type = "",
                            total_tax_reduction = (object)null,
                            skip_notify_negative_stock = false
                        },
                        roundingDecimal = true,
                        price_info = new
                        {
                            tax_breakdowns = new object[] { },
                            total_amount = 0,
                            total_amount_final = 0,
                            total_tax_reduction = 0,
                            total_tax_amount = 0,
                            total_discount_amount = 0,
                            surcharge = 0
                        },
                        product_changed_lines = new object { },
                        ingredient_changed_lines = new object { },
                        status = "processing",
                        skip_verify_price_info = false,
                        purchase_price_source = "tendoo",
                        business_id = "9065ebd6-4c83-4c9c-a6d4-4937e3fda49b"
                    };
                    string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                    string urlapi= "https://apiv2.tendoo.vn/warehouse/api/v2/po/create-inbound";
                    // Tạo nội dung request
                    var contents = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    // Gửi request
                    HttpResponseMessage responses = await httpClient.PostAsync(urlapi, contents);
                    string result = await responses.Content.ReadAsStringAsync();
                    JObject jsonResponse = JObject.Parse(result);
                    string contactId = jsonResponse["data"]?["id"]?.ToString();
                   var  payload2 = new
                    {
                        id = contactId,  // Tạo mới nếu cần
                        po_type = "in",
                        contact_info = new
                        {
                            id = "00000000-0000-0000-0000-000000000000",
                            phone_number = "",
                            name = "",
                            avatar = "",
                            social_avatar = "",
                            address = "",
                            role = "",
                            tax_number = ""
                        },
                        note = "",
                        po_details = new[]
    {
        new
        {
            product_id = item["TendoId"]?.ToString() ?? "",
            sku_name = "",
            media = new object[] { },
            sku_code = item["TendoSku"]?.ToString() ?? "",
            barcode = "",
            product_name = item["TendoName"]?.ToString() ?? "",
            type = "stock_non_varriant",
            product_type = "non_variant",
            selling_price = 0,
            normal_price = 0,
            uom = item["TendoUom"]?.ToString() ?? "",
            uom_id = "00000000-0000-0000-0000-000000000000",
            pricing = 0,
            quantity =qty1- quantity,  // ← Động
            total_quantity = 0,
            transaction_type = "in",
            before_change_quantity = 0,
            tax_info = new { },
            additional_item_info = new { type = "value", discount_value = 0 },
            price_info = new
            {
                pricing_original = 0,
                pricing_display = 0,
                pricing_calculated = 0,
                item_discount_amount = 0,
                allocated_discount_amount = 0,
                allocated_discount_amount_total = 0,
                internal_allocated_discount_amount = 0,
                total_amount_before_tax = 0,
                total_amount_after_tax = 0,
                total_amount = 0
            },
            historical_cost = 0,
            total_value = 0,
            old_sku_code = item["TendoSku"]?.ToString() ?? "",
            old_product_name = item["TendoName"]?.ToString() ?? "",
            new_price = 0,
            price_after_discount = 0,
            unique_id = $"{item["TendoSkuId"]?.ToString() ?? ""}_0",
            sku_id = item["TendoSkuId"]?.ToString() ?? "",
            pricing_original = 0
        }
    },
                        po_detail_ingredient = new object[] { },
                        total_discount = 0,
                        sur_charge = 0,
                        buyer_pay = 0,
                        option = "create_po",
                        media = new object[] { },
                        payment_state = "un_paid",
                        is_debit = false,
                        invoice_type = "vat",
                        created_po_at = DateTime.UtcNow.ToString("o"),  // ISO 8601 format
                        additional_info = new
                        {
                            discount_value = 0,
                            type = "",
                            total_tax_reduction = (object)null,
                            skip_notify_negative_stock = false
                        },
                        roundingDecimal = true,
                        price_info = new
                        {
                            tax_breakdowns = new object[] { },
                            total_amount = 0,
                            total_amount_final = 0,
                            total_tax_reduction = 0,
                            total_tax_amount = 0,
                            total_discount_amount = 0,
                            surcharge = 0
                        },
                        status = "completed",
                        payment_source_id = "bb9385b1-5450-4a16-83ce-1b5b69a3aa13",
                        payment_source_name = "Tiền mặt",
                        business_id = "9065ebd6-4c83-4c9c-a6d4-4937e3fda49b"
                    };

                    string jsonData2 = Newtonsoft.Json.JsonConvert.SerializeObject(payload2);
                    string urlapi2 = $"https://apiv2.tendoo.vn/warehouse/api/v2/po/update-inbound/{contactId}";
                    // Tạo nội dung request
                    var contents2 = new StringContent(jsonData2, Encoding.UTF8, "application/json");

                    // Gửi request
                    HttpResponseMessage responses2 = await httpClient.PutAsync(urlapi2, contents2);
                    string result2 = await responses2.Content.ReadAsStringAsync();
                    JObject jsonResponse2 = JObject.Parse(result2);
                }
            } 
                Thuchienhoadon();
        }
        private async Task Thuchienhoadon()
        {
            progressPanel1.Caption = "Đang thực hiện đồng bộ hoá đơn lên tendoo...";
            // Build list_order_item - sản phẩm có sẵn trong hệ thống
            var listOrderItem = new JsonArray();
            decimal grandTotal = 0;
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // SP 1
           

            foreach(DataRow item in dtHangHoa.Rows)
            {
                decimal price1 = 0;
                int qty1 = Convert.ToInt32(item["Quantity"].ToString()); 
                string hhid=item["TendoId"].ToString();
                string api = $"https://apiv2.tendoo.vn/product/api/v2/product/seller/get-detail/{hhid}";
                var    responses =  await httpClient.GetAsync(api);
                string content =
                    await responses.Content.ReadAsStringAsync();
                JObject obj = JObject.Parse(content);
                price1 = Convert.ToDecimal(item["Amount"].ToString());
                decimal total1 = price1 * qty1;
                grandTotal += total1;
                string random =
    Guid.NewGuid().ToString("N").Substring(0, 9);
                string lineId =
    $"{Guid.NewGuid()}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{random}";
                try
                {
                    listOrderItem.Add(new JsonObject
                    {
                        ["product_id"] = hhid,
                        ["product_name"] = obj["data"]?["name"]?.ToString(),
                        ["product_images"] = new JsonArray(),
                        ["product_type"] = obj["data"]?["type"]?.ToString(),
                        ["sku_id"] = obj["data"]?["list_sku"]?[0]?["id"]?.ToString(),
                        ["sku_type"] = obj["data"]?["list_sku"]?[0]?["sku_type"]?.ToString(),
                        ["sku_name"] = obj["data"]?["list_sku"]?[0]?["name"]?.ToString(),
                        ["sku_code"] = obj["data"]?["list_sku"]?[0]?["sku_code"]?.ToString(),
                        ["range_wholesale_price"] = new JsonArray(),
                        ["product_normal_price"] = price1,
                        ["product_selling_price"] = 0,
                        ["price"] = price1,
                        ["wholesale_price"] = 0,
                        ["quantity"] = qty1,
                        ["note"] = "",
                        ["uom"] = obj["data"]?["uom"]?.ToString(),
                        ["order_item_add_on"] = new JsonArray(),
                        ["category_ids"] = new JsonArray(),
                        ["order_item_rate"] = new JsonArray(),
                        ["using_product_rate"] = true,
                        ["current_product_rate"] = null,
                        ["price_info"] = new JsonObject
                        {
                            ["quantity"] = qty1,
                            ["init_discount_percent"] = 0,
                            ["unit_price_initial"] = price1,
                            ["unit_price_discount"] = 0,
                            ["unit_price_before_tax"] = price1,
                            ["unit_price_after_tax"] = price1,
                            ["unit_tax_amount"] = 0,
                            ["total_amount_initial"] = total1,
                            ["total_amount_before_tax"] = total1,
                            ["total_amount_after_tax"] = total1,
                            ["total_amount_tax_reduce"] = 0,
                            ["total_amount_after_tax_reduce"] = total1,
                            ["tax_percent"] = -2,
                            ["tax_amount_after_discount"] = 0,
                            ["tax_amount"] = 0,
                            ["item_discount"] = 0,
                            ["discount_allocated_before_tax"] = 0,
                            ["discount_allocated_tax"] = 0,
                            ["discount_allocated_after_tax"] = 0,
                            ["discount_allocated_tax_reduce"] = 0,
                            ["discount_allocated_after_tax_reduce"] = 0,
                            ["unit_refund_amount"] = price1,
                            ["unit_tax_refund_amount"] = 0,
                            ["customer_discount_allocated_before_tax"] = 0,
                            ["customer_discount_allocated_tax"] = 0,
                            ["customer_discount_allocated_after_tax"] = 0,
                            ["customer_discount_allocated_tax_reduce"] = 0,
                            ["customer_discount_allocated_after_tax_reduce"] = 0,
                            ["order_discount_allocated_before_tax"] = 0,
                            ["order_discount_allocated_tax"] = 0,
                            ["order_discount_allocated_after_tax"] = 0,
                            ["order_discount_allocated_after_tax_reduce"] = 0,
                            ["order_discount_allocated_tax_reduce"] = 0,
                            ["tax_reduce_percent"] = 0
                        },
                        ["historical_cost"] = int.TryParse(
    obj["data"]?["list_sku"]?[0]?["historical_cost"]?.ToString(),
    out int historicalCost
) ? historicalCost : 0,
                        ["price_non_discount"] = price1,
                        ["product_version"] = obj["data"]?["version"]?.Value<int>() ?? 0,
                        ["total_amount"] = total1,
                        ["show_edit_note"] = false,
                        ["product_rate"] = new JsonArray(),
                        ["lineId"] = lineId
                    });
                }
                catch (Exception ex)
                {   
                    Console.WriteLine($"Error at Add OrderItem: {ex}");
                    // Hoặc log vào file, hoặc breakpoint ở đây
                }
            }
            
            var calculateId = Guid.NewGuid().ToString();
            // Build payload y hệt mẫu
            var payload = new JsonObject
            {
                ["calculate_id"] = calculateId, // Thêm vào đây
                ["business_id"] = businessId,
                ["ordered_grand_total"] = grandTotal,
                ["state"] = "delivering",
                ["create_method"] = "seller",
                ["payment_method"] = "cash",
                ["payment_source_id"] = "bb9385b1-5450-4a16-83ce-1b5b69a3aa13",
                ["payment_source_name"] = "Tiền mặt",
                ["payment_order_history"] = new JsonArray(),
                ["email"] = "",
                ["note"] = "",
                ["images"] = new JsonArray(),
                ["delivery_method"] = "buyer_pick_up",
                ["buyer_info"] = new JsonObject
                {
                    ["name"] = buyerName,
                    ["phone_number"] = buyerPhone,
                    ["address_info"] = null,
                    ["avatar"] = "",
                    ["debt_amount"] = 0,
                    ["option"] = "",
                    ["address"] = buyerAddress,
                    ["invoice_contact_info"] = null,
                    ["address_version"] = 0
                },
                ["list_order_item"] = listOrderItem, // SP có sẵn bỏ vào đây
                ["list_product_fast"] = new JsonArray(), // Để rỗng như mẫu
                ["list_gift"] = new JsonArray(),
                ["other_discount"] = 0,
                ["other_discount_value"] = 0,
                ["order_discount_unit"] = "value",
                ["surcharge"] = 0,
                ["additional_info"] = new JsonObject
                {
                    ["discount_type"] = "value",
                    ["given_amount"] = 0
                },
                ["highlighted_order_items"] = new JsonArray(),
                ["grand_total"] = grandTotal,
                ["is_wholesale_price"] = false,
                ["selected_promotion"] = null,
                ["valid_promotion"] = false,
                ["has_debt_amount"] = false,
                ["customer_point"] = 0,
                ["customer_point_discount"] = 0,
                ["customer_point_ratio"] = 0,
                ["is_customer_point"] = false,
                ["reservation_meta"] = null,
                ["is_open_delivery"] = true,
                ["reservation_info"] = null,
                ["has_e_invoice"] = ctents[3] == '1' ? true:false,
                ["order_invoice_body"] = new JsonObject
                {
                    ["template_code"] = null,
                    ["partner_key"] = null,
                    ["object_key"] = null,
                    ["object_type"] = null,
                    ["invoice_seri"] = null,
                    ["buyer_invoice_info"] = null
                },
                ["is_debit"] = false,
                ["price_info"] = new JsonObject
                {
                    ["price_version"] = 2,
                    ["value_added_taxes"] = new JsonObject
                    {
                        ["total"] = 0,
                        ["details"] = new JsonArray(),
                        ["full_details"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["percentage"] = -2,
                            ["amount"] = 0,
                            ["taxable_amount"] = grandTotal
                        }
                    }
                    },
                    ["total_of_items_initial"] = grandTotal,
                    ["total_of_items_before_tax"] = grandTotal,
                    ["total_of_items_after_tax"] = grandTotal,
                    ["total_of_items_after_tax_reduce"] = grandTotal,
                    ["discount_amount_before_tax"] = 0,
                    ["discount_amount_after_tax"] = 0,
                    ["discount_amount_after_tax_reduce"] = 0,
                    ["surcharge_before_tax"] = 0,
                    ["surcharge_after_tax"] = 0,
                    ["surcharge_after_tax_reduce"] = 0,
                    ["surcharge"] = 0,
                    ["delivery_unit_price_before_tax"] = 0,
                    ["delivery_fee_before_tax"] = 0,
                    ["delivery_fee_after_tax"] = 0,
                    ["delivery_fee_after_tax_reduce"] = 0,
                    ["tax_reduce_amount"] = 0,
                    ["delivery_tax_refund_amount"] = 0,
                    ["delivery_tax"] = -2,
                    ["delivery_tax_amount"] = 0,
                    ["surcharge_summary"] = new JsonArray(),
                    ["total_amount_initial"] = grandTotal,
                    ["total_amount_before_tax"] = grandTotal,
                    ["total_amount_after_tax"] = grandTotal,
                    ["all_discounts_before_tax"] = 0,
                    ["is_no_tax"] = true,
                    ["business_precision"] = new JsonObject
                    {
                        ["taxPercentScale"] = 2,
                        ["discountPercentScale"] = 2,
                        ["quantity_scale"] = 4,
                        ["unit_price_scale"] = 0,
                        ["tax_amount_scale"] = 0,
                        ["total_before_tax_scale"] = 0,
                        ["total_amount_scale"] = 0,
                        ["discount_amount_scale"] = 0
                    }
                },
                ["promotion_code"] = "",
                ["promotion_discount"] = 0,
                ["delivery_fee"] = 0,
                ["list_surcharge"] = new JsonArray(),
                ["show_other_discount"] = false,
                ["show_delivery_fee"] = false,
                ["debit"] = new JsonObject
                {
                    ["buyer_pay"] = 0,
                    ["description"] = "",
                    ["is_debit"] = false
                },
                ["buyer_id"] = buyerId
            };
            try
            {
                string json = payload.ToJsonString();
                Console.WriteLine(json);
                // 3. GỌI API TẠO ĐƠN
                var http = new HttpClient();
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJwcm9fd2ViIiwiZXhwIjoxNzgxMzEzMjg2LCJzdWIiOiIwNmEyYjg3Mi04MmJmLTQ5MzYtODUyYy0wMjhmNzk3Mzc5MjJ8Mjk4NjQ1NmEtZTlhYy00MGYxLWJjYTUtM2FkMTRkMTFkNzgwfDI5ODY0NTZhLWU5YWMtNDBmMS1iY2E1LTNhZDE0ZDExZDc4MCIsImRldmljZV9pZCI6IjI5ODY0NTZhLWU5YWMtNDBmMS1iY2E1LTNhZDE0ZDExZDc4MCIsImJ1c2luZXNzX2lkIjoiOTA2NWViZDYtNGM4My00YzljLWE2ZDQtNDkzN2UzZmRhNDliIiwicGVybWlzc2lvbl9rZXlzIjoic2hvcF9vd25lciIsInJlZnJlc2hfdG9rZW5faWQiOiI2YjI5NTcxYy1jOTNjLTRjZTgtYWFkOC03MjVhMTIzZDEyNGUiLCJzZWN1cml0eV9yb2xlcyI6MCwiYXBwX3ZlcnNpb24iOiIiLCJ1c2VyX2lkIjoiMDZhMmI4NzItODJiZi00OTM2LTg1MmMtMDI4Zjc5NzM3OTIyIn0.5Ggdl3jmwuG8fdKyM7BDJhi-G8b3xTkwnzKXpbfPF60");

                var request = new HttpRequestMessage(HttpMethod.Post, "https://apiv2.tendoo.vn/order/api/v13/seller/create-order");
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                // Thêm header X-Idempotency-Key - phải là GUID
                request.Headers.Add("X-Idempotency-Key", Guid.NewGuid().ToString());
                // Hoặc dùng key cố định: request.Headers.Add("X-Idempotency-Key", "b34b12c1-8f3c-462b-92df-4675c2cc566d");

                var response = await http.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"API Error {(int)response.StatusCode}: {result}");
                JObject obj = JObject.Parse(result);
                string id = obj["data"]?["id"]?.ToString();
                string state= obj["data"]?["state"]?.ToString();

                string query = @"UPDATE HoaDon 
                             SET TendoHDid = ? ,
                             TendoHDState = ?
                             WHERE MaSo = ?";

                var parameters = new OleDbParameter[]
                {
                new OleDbParameter("?", id ?? ""),
                new OleDbParameter("?", state ?? ""),
                new OleDbParameter("?", dtHoaDon.Rows[0]["HOADON.MaSo"]?.ToString() ?? "")
                };

                int rowsAffected = ExecuteQueryResult(query, parameters);


                Console.WriteLine(result);
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"Response: {result}");
                progressPanel1.Caption = "Tao hoá đơn thành công trên Tendoo!";
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                progressPanel1.Caption = "Lỗi khi tạo payload: " + ex.Message;
                this.Close();
            }
           
            this.Close();
        }
        DataTable dtKhachhang;
        DataTable dtHoaDon;
        DataTable dtChungTu;
        DataTable dtHangHoa = new DataTable();
        private async Task LayThongTinHoaDon()
        {
            progressPanel1.Caption = "Đang lấy thông tin hoá đơn...";
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            string filePath = Path.Combine(rootDirectory, "Hoadon", "invoice.txt");
            _content = File.ReadAllText(filePath);
            var getsplit = _content.Split('_');

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

            dtHoaDon  = ExecuteQuery(qrTimct, parameterss);

            //Lấy danh sách chứng từ từ MaCT
            var sql = "SELECT * FROM KhachHang WHERE MaSo = ?";
            parameterss = new OleDbParameter[]
          {
    new OleDbParameter("?",  dtHoaDon.Rows[0]["MaKhachHang"]),
          };
            dtKhachhang = ExecuteQuery(sql, parameterss);

            sql = "SELECT * FROM ChungTu WHERE MaCT = ?";
            var param = new OleDbParameter[]
            {
                new OleDbParameter("?", dtHoaDon.Rows[0]["MaCT"])
            };
            try
            {
                dtChungTu = ExecuteQuery(sql, param);
                dtHangHoa.Columns.Add("ItemCode", typeof(string));
                dtHangHoa.Columns.Add("ItemName", typeof(string));
                dtHangHoa.Columns.Add("UnitName", typeof(string));
                dtHangHoa.Columns.Add("UnitPrice", typeof(decimal));
                dtHangHoa.Columns.Add("Quantity", typeof(double));
                dtHangHoa.Columns.Add("Amount", typeof(decimal));
                dtHangHoa.Columns.Add("TendoName", typeof(string));
                dtHangHoa.Columns.Add("TendoSKU", typeof(string));
                dtHangHoa.Columns.Add("TendoUom", typeof(string));
                dtHangHoa.Columns.Add("TendoId", typeof(string));
                dtHangHoa.Columns.Add("TendoPrice", typeof(double));
                dtHangHoa.Columns.Add("TendoQuality", typeof(int));
                dtHangHoa.Columns.Add("TendoSkuId", typeof(string));
            }  
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lấy thông tin chứng từ: " + ex.Message);
                return;
            }
            foreach (DataRow row in dtChungTu.Rows)
            {
                try
                {
                    if (row["MaVattu"].ToString().Trim() == "0")
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
                    string tenhh = kqHangHoa.Rows[0]["TenVattu"].ToString();
                    string donvitinh = kqHangHoa.Rows[0]["DonVi"].ToString();
                    string TendoName = kqHangHoa.Rows[0]["TendoName"].ToString();
                    string TendoSku = kqHangHoa.Rows[0]["TendoSku"].ToString();
                    string TendoUom = kqHangHoa.Rows[0]["TendoUom"].ToString();
                    string TendoId = kqHangHoa.Rows[0]["TendoId"].ToString();
                    double price = kqHangHoa.Rows[0]["TendoPrice"].ToString() == "" ? 0 : Convert.ToDouble(kqHangHoa.Rows[0]["TendoPrice"]);
                    int tendoQuality = kqHangHoa.Rows[0]["TendoQuality"].ToString() == "" ? 0 : Convert.ToInt32(kqHangHoa.Rows[0]["TendoQuality"]); 
                    string TendoSkuId = kqHangHoa.Rows[0]["TendoSkuId"].ToString();
                    dtHangHoa.Rows.Add("", tenhh, donvitinh, 0, soluong, sops, TendoName, TendoSku, TendoUom, TendoId, price, tendoQuality, TendoSkuId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lấy thông tin hàng hóa: " + ex.Message);
                }
            }

            LayThongTinKhachHang();
        }
        private async Task AddInvoice()
        { 
             LayThongTinInfo(); 
            
        }
        public string tokken { get; set; }
        private async void TendoInvoice_Load(object sender, EventArgs e)
        {
            progressPanel1.Caption="Đang kết nối với hệ thống Tendoo...";
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
            string userDataFolder = Path.Combine(
                Application.StartupPath,
                "TendooProfile"
            );

            var env = await CoreWebView2Environment.CreateAsync(
                null,
                userDataFolder
            );

            await webView21.EnsureCoreWebView2Async(env);

            webView21.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;

            webView21.Source = new Uri("https://seller-v2.tendoo.vn/");
        }

        private async void CoreWebView2_NavigationCompleted(
      object sender,
      CoreWebView2NavigationCompletedEventArgs e)
        {
            progressPanel1.Caption = "Đang đăng nhập hệ thống Tendoo...";
            string url = webView21.Source.ToString();

            // Nếu đã ở POS thì lấy token và gọi API
            if (url.Contains("/sales/pos"))
            {
                await LayBearerToken();
                return;
            }

            await Task.Delay(3000);

            // Lấy thông tin từ database
            string qrq = "SELECT * FROM tbInvoiceInfo";
            var dtInvoiceInfo = ExecuteQuery(qrq, null);
            var rows = dtInvoiceInfo.Rows[0];
            string username = rows["Username"]?.ToString();
            string pass = rows["Password"]?.ToString();

            string js = $@"
(async function () {{

    if (localStorage.getItem('@access_token')) {{

        location.href =
        'https://seller-v2.tendoo.vn/sales/pos';

        return;
    }}

    function setReactInputValue(selector, value) {{

        const input = document.querySelector(selector);

        if (!input) return;

        const nativeInputValueSetter =
            Object.getOwnPropertyDescriptor(
                window.HTMLInputElement.prototype,
                'value'
            ).set;

        nativeInputValueSetter.call(input, value);

        input.dispatchEvent(new Event('input', {{
            bubbles: true
        }}));

        input.dispatchEvent(new Event('change', {{
            bubbles: true
        }}));
    }}

    setReactInputValue(
        '#phone_login_form_phone_number',
        '{username}'
    );

    await new Promise(r => setTimeout(r, 800));

    setReactInputValue(
        '#phone_login_form_pwd',
        '{pass}'
    );

    await new Promise(r => setTimeout(r, 1000));

    const btn =
        document.querySelector('button[type=submit]');

    if (btn) {{

        btn.click();

        setTimeout(() => {{

            location.href =
            'https://seller-v2.tendoo.vn/sales/pos';

        }}, 1000);
    }}

}})();
";

            await webView21.CoreWebView2.ExecuteScriptAsync(js);
        }

        // Hàm lấy Bearer Token
        private async Task LayBearerToken()
        {
            string jsLayToken = @"
        (function() {
            var token = localStorage.getItem('@access_token');
            if (token) return token;
            return null;
        })();
    ";

            try
            {
                string token = await webView21.CoreWebView2.ExecuteScriptAsync(jsLayToken);
                token = token?.Trim('"');

                if (!string.IsNullOrEmpty(token) && token != "null")
                {
                    bearerToken = token;
                    _content = token;
                    tokken = token;
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    // Lưu token
                    // File.WriteAllText("token.txt", token);

                    // Đóng WebView2 vì không cần nữa
                    progressPanel1.Caption = "Đã lấy được token...";
                    webView21.Visible = false; 
                    string appPath = Assembly.GetExecutingAssembly().Location;

                    // Lấy thư mục chứa ứng dụng
                    string directoryPath = Path.GetDirectoryName(appPath);
                    string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

                    string filePath = Path.Combine(rootDirectory, "Hoadon", "invoice.txt");
                      ctents = File.ReadAllText(filePath);
                    var getsplit = ctents.Split('_'); 
                    if (ctents.Contains("KH_"))
                    {
                        progressPanel1.Caption = "Đang thực hiện đồng bộ khách hàng mới...";
                        var sql = "SELECT * FROM KhachHang WHERE MaSo = ?";
                        var parameterss = new OleDbParameter[]
                      {
    new OleDbParameter("?",  getsplit[1].ToString()),
                      };
                        var datakh = ExecuteQuery(sql, parameterss);
                        //Sau khi có thông tin khách hàng thì đồng bộ lên tendo, add khách hàng vao tendo rồi mới add hóa đơn vào tendo
                        string urlapi = "https://apiv2.tendoo.vn/business/api/v2/contact/create";
                        var payload = new
                        {
                            is_has_invoice_contact_info = false, 
                            invoice_contact_info = new
                            {
                                dvqhns_code = "",
                                bank_account = "",
                                bank_name = "",
                                identification_no = "",
                                email ="",
                                phone_number ="",
                                name = "",
                                full_address = "",
                                business_name = "",
                                tax_number = "",
                                province_id = "",
                                province_name = "",
                                district_id = "",
                                district_name = "",
                                ward_id = "",
                                ward_name = "",
                                address = ""
                            }, 
                            has_old_debt = false,
                            debt_type = "receivable",
                            group_of_contact_ids = new List<string>(),

                            email = datakh.Rows[0]["EMail"].ToString(),
                            phone_number = datakh.Rows[0]["Tel"].ToString(),
                            name = Helpers.ConvertVniToUnicode(datakh.Rows[0]["Ten"].ToString()),
                            role = "customer",
                            contact_code = datakh.Rows[0]["SoHieu"].ToString(),
                            birthday = "",
                            tags = new List<string>(),

                            address_info = new
                            {
                                province_id = "HCM",
                                province_name = "Thành phố Hồ Chí Minh",
                                district_id = "",
                                district_name = "",
                                ward_id = "HCM001",
                                ward_name = "Phường Vũng Tàu",
                                address_version = 1,
                                address = Helpers.ConvertVniToUnicode(datakh.Rows[0]["DiaChi"].ToString())
                            },

                            debt_record_date = (string)null,
                            is_record_transaction = false,
                            full_address1 = "Phường Vũng Tàu, Thành phố Hồ Chí Minh"
                        };
                        // Chuyển đổi payload thành JSON
                        string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(payload);

                        // Tạo nội dung request
                        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                        // Gửi request
                        HttpResponseMessage response = await httpClient.PostAsync(urlapi, content);
                        string result = await response.Content.ReadAsStringAsync();

                        JObject jsonResponse = JObject.Parse(result);
                        string contactId = jsonResponse["data"]?["id"]?.ToString();

                        string query = @"UPDATE KhachHang 
                             SET contact_id = ? 
                             WHERE MaSo = ?";

                        var parameters = new OleDbParameter[]
                        {
                new OleDbParameter("?", contactId.ToString() ?? ""),
                new OleDbParameter("?", datakh.Rows[0]["MaSo"]?.ToString() ?? "")
                        };

                        int rowsAffected = ExecuteQueryResult(query, parameters);

                        // Kiểm tra kết quả
                        if (response.IsSuccessStatusCode)
                        {
                            progressPanel1.Caption = "Thực hiện thành công...";
                            Console.WriteLine("Thành công: " + result);

                        }
                        else
                        {
                            progressPanel1.Caption = "Có lỗi...";
                            //Console.WriteLine("Lỗi: " + result);
                        }
                        this.Close();
                    }
                    else
                    {
                        if (ctents.Contains("SP_"))
                        {
                            progressPanel1.Caption = "Đang thực hiện đồng bộ sản phẩm mới...";
                            var sql = "SELECT * FROM Vattu WHERE MaSo = ?";
                            var parameterss = new OleDbParameter[]
                          {
    new OleDbParameter("?",  getsplit[1].ToString()),
                          };
                            var datavattu = ExecuteQuery(sql, parameterss);

                            string urlapi = "https://apiv2.tendoo.vn/product/api/v1/product/create";
                            var payload = new
                            {
                                business_id = "9065ebd6-4c83-4c9c-a6d4-4937e3fda49b",
                                uom = Helpers.ConvertVniToUnicode(datavattu.Rows[0]["DonVi"].ToString()),
                                tax_selected = (string)null,  // Hoặc ghi đơn giản: tax_selected = null, 
                                name = Helpers.ConvertVniToUnicode(datavattu.Rows[0]["TenVattu"].ToString()),
                                client_id = Guid.NewGuid().ToString(),  // Random mỗi lần gọi
                                description = "",
                                description_rtf = "",
                                images = Array.Empty<string>(),
                                is_active = true,
                                priority = 1,
                                sku_code = "",  
                                barcode = "",
                                product_type = "non_variant",
                                tag_id = "00000000-0000-0000-0000-000000000000",
                                tag_name = "",
                                product_add_on_group_ids = Array.Empty<string>(),
                                show_on_store = true,
                                show_price = false,
                                has_ingredient = false,
                                has_rate = false,
                                tax_percent = (int?)null,
                                personal_income_tax_percent = (int?)null,
                                apply_tax_discount = (int?)null,
                                business_sector_code = (string)null,
                                category_ids = Array.Empty<string>(),
                                list_sku = new[]
                                   {
                                        new
                                        {
                                            id = "00000000-0000-0000-0000-000000000000",
                                            range_wholesale_price = Array.Empty<string>(),
                                            sku_type =datavattu.Rows[0]["DonVi"].ToString()=="1" ? "stock" : "non_stock",
                                            selling_price = 0,
                                            recipe = Array.Empty<string>(),
                                            historical_cost = 0,
                                            hide_sku = false,
                                            normal_price = 0,
                                            uom = Helpers.ConvertVniToUnicode(datavattu.Rows[0]["DonVi"].ToString()),
                                            name ="",
                                            business_id = "9065ebd6-4c83-4c9c-a6d4-4937e3fda49b",
                                            media = Array.Empty<string>(),
                                            barcode = "",
                                            sku_code =datavattu.Rows[0]["SoHieu"].ToString(),
                                            wholesale_price = 0,
                                            total_quantity = 0,
                                            is_active = true,
                                            product_id = (string)null,
                                            priority = 1,
                                            attribute_types = Array.Empty<string>(),
                                            number_attribute_type = 0
                                        }
                                    },
                                                        product_code = datavattu.Rows[0]["SoHieu"].ToString()
                            };
                            string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(payload);

                            // Tạo nội dung request
                            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                            // Gửi request
                            HttpResponseMessage response = await httpClient.PostAsync(urlapi, content);
                            string result = await response.Content.ReadAsStringAsync();

                            JObject jsonResponse = JObject.Parse(result);
                            string contactId = jsonResponse["data"]?["id"]?.ToString();
                            string name = jsonResponse["data"]?["name"]?.ToString();
                            string product_code = jsonResponse["data"]?["product_code"]?.ToString();
                            string uom = jsonResponse["data"]?["uom"]?.ToString();
                            string normal_price = jsonResponse["data"]?["list_sku"]?[0]?["normal_price"]?.ToString();
                            string query = @"UPDATE Vattu 
                 SET TendoName = ?, 
                     TendoSku = ?, 
                     TendoUom = ?,
                     TendoPrice = ?,  
                     TendoId = ? 
                 WHERE MaSo = ?";

                            var parameters = new OleDbParameter[]
                            {
                                   new OleDbParameter("?", name ?? ""),
                                   new OleDbParameter("?", product_code ?? ""),
                                      new OleDbParameter("?", uom ?? ""),
                                        new OleDbParameter("?", normal_price ?? "0"),
                new OleDbParameter("?", contactId.ToString() ?? ""),
                new OleDbParameter("?", datavattu.Rows[0]["MaSo"]?.ToString() ?? "")
                            };

                            int rowsAffected = ExecuteQueryResult(query, parameters);
                            progressPanel1.Caption = "Sucess...";
                            this.Close();
                        }
                        else
                        {
                            if (ctents.Contains("TT_"))
                            {
                                progressPanel1.Caption = "Đang thực hiện cập nhật trạng thái hoá đơn...";
                                string urlapi = $"https://apiv2.tendoo.vn/order/api/v1/seller/check-cancel-order-precondition/{getsplit[1]}";

                                // Option A: Explicitly send empty content
                                var emptyContent = new StringContent("", Encoding.UTF8, "application/json");
                                HttpResponseMessage response = await httpClient.PostAsync(urlapi, emptyContent);

                                // Option B: Send empty JSON object
                                var emptyJson = new StringContent("{}", Encoding.UTF8, "application/json");
                                HttpResponseMessage responses = await httpClient.PostAsync(urlapi, emptyJson);

                                string result = await responses.Content.ReadAsStringAsync();
                                urlapi = $"https://apiv2.tendoo.vn/order/api/v8/seller/update-order/{getsplit[1]}";
                                var payload = new
                                {
                                    state = "complete",
                                    payment_method = "Tiền mặt",
                                    payment_source_id = "bb9385b1-5450-4a16-83ce-1b5b69a3aa13",
                                    payment_source_name = "Tiền mặt",
                                    debit = new
                                    {
                                        buyer_pay = getsplit[2],
                                        description = "",
                                        is_debit = false
                                    }
                                };

                                string jsonPayload = JsonConvert.SerializeObject(payload);
                                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                                // Gửi request
                                response = await httpClient.PutAsync(urlapi, content);
                                result = await response.Content.ReadAsStringAsync();

                                JObject jsonResponse = JObject.Parse(result);
                                progressPanel1.Caption = "Đã cập nhật trạng thái hoá đơn...";


                                try
                                {


                                    string query = @"UPDATE HoaDon  
                            SET  TendoHDState = ?
                             WHERE TendoHDid = ?";

                                    var parameters = new OleDbParameter[]
                                    {
                new OleDbParameter("?", "complete"),
                new OleDbParameter("?",getsplit[1])
                                    };

                                    int rowsAffected = ExecuteQueryResult(query, parameters);

                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Lỗi khi cập nhật trạng thái hoá đơn: " + ex.Message);
                                }
                                this.Close();
                            }
                            else
                            {
                                if (ctents.Contains("Huy_"))
                                { 
                                    string urlapi = $"https://apiv2.tendoo.vn/order/api/v8/seller/update-order/{getsplit[1]}";

                                    // Tạo payload object
                                    var payload = new
                                    {
                                        state = "cancel",
                                        debit = (object)null,
                                        additional_info = new { },
                                        payment_source_id = (object)null,
                                        payment_source_name = (object)null,
                                        reservation_info = (object)null,
                                        is_customer_point = false,
                                        cancel_transaction = new[] { "business_transaction" },
                                        is_remove_transaction_when_cancel = true
                                    };

                                    // Serialize thành JSON
                                    string jsonPayload = JsonConvert.SerializeObject(payload);
                                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                                    // Gửi request
                                    HttpResponseMessage response = await httpClient.PutAsync(urlapi, content);
                                    string result = await response.Content.ReadAsStringAsync();
                                    this.Close();
                                }
                                else
                                {
                                    await Dongbokhachhang();
                                }
                            }
                        } 
                    }
                    //await Dongbokhachhang();
                    // await Dongbosanpham();
                    // await AddInvoice();
                    // Gọi API
                    // await GoiApiVoiToken();
                }
                else
                {
                    MessageBox.Show("Không lấy được token!", "Lỗi");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }
        string ctents;
        // Hàm gọi API với Bearer Token
        private async Task GoiApiVoiToken()
        {
            try
            {
                // Cấu hình HttpClient
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");


                // Ví dụ 2: Lấy danh sách hóa đơn
               // await Dongbokhachhang();
                // await Dongbosanpham();
                 


                string appPath = Assembly.GetExecutingAssembly().Location;

                // Lấy thư mục chứa ứng dụng
                string directoryPath = Path.GetDirectoryName(appPath);
                string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

                string filePath = Path.Combine(rootDirectory, "Hoadon", "invoice.txt");
                _content = File.ReadAllText(filePath);
                var getsplit = _content.Split('_');

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
                DataTable dtHangHoa = new DataTable();
                dtHangHoa.Columns.Add("ItemCode", typeof(string));
                dtHangHoa.Columns.Add("ItemName", typeof(string));
                dtHangHoa.Columns.Add("UnitName", typeof(string));
                dtHangHoa.Columns.Add("UnitPrice", typeof(decimal));
                dtHangHoa.Columns.Add("Quantity", typeof(decimal));
                dtHangHoa.Columns.Add("Amount", typeof(decimal));
                dtHangHoa.Columns.Add("TendoName", typeof(string));
                dtHangHoa.Columns.Add("TendoSKU", typeof(string));
                dtHangHoa.Columns.Add("TendoUom", typeof(string));
                dtHangHoa.Columns.Add("TendoId", typeof(string));
                foreach (DataRow row in kq3.Rows)
                {
                    try
                    {
                        if (row["MaVattu"].ToString().Trim() == "0")
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
                        double dongia = Math.Round(sops / soluong);
                        string tenhh = kqHangHoa.Rows[0]["TenVattu"].ToString();
                        string donvitinh = kqHangHoa.Rows[0]["DonVi"].ToString();
                        string TendoName= kqHangHoa.Rows[0]["TendoName"].ToString();
                        string TendoSku= kqHangHoa.Rows[0]["TendoSku"].ToString();
                        string TendoUom= kqHangHoa.Rows[0]["TendoUom"].ToString();
                        string TendoId= kqHangHoa.Rows[0]["TendoId"].ToString();
                        dtHangHoa.Rows.Add("", tenhh, donvitinh, dongia, soluong, sops, TendoName, TendoSku, TendoUom, TendoId);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi lấy thông tin hàng hóa: " + ex.Message);
                    }
                }

                foreach (DataRow row in dtHangHoa.Rows)
                {
                    if (!string.IsNullOrEmpty(row["TendoId"].ToString()))
                    {
                        string apiUrl = $"https://apiv2.tendoo.vn/product/api/v2/product/seller/get-detail/{row["TendoId"].ToString()}";
                        HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
                        string content = await response.Content.ReadAsStringAsync();
                        if (response.IsSuccessStatusCode)
                        {
                            JObject obj = JObject.Parse(content);
                            string name = obj["data"]?["name"]?.ToString() ?? "N/A";
                            string sku = obj["data"]?["list_sku"]?.FirstOrDefault()?["sku_code"]?.ToString() ?? "N/A";
                            string uom = obj["data"]?["uom"]?.ToString() ?? "N/A";
                            MessageBox.Show($"Chi tiết sản phẩm:\nTên: {name}\nSKU: {sku}\nĐơn vị tính: {uom}", "Thông tin sản phẩm");
                        }
                        else
                        {
                            MessageBox.Show($"Lỗi khi lấy chi tiết sản phẩm: {response.StatusCode}", "Lỗi");
                        }
                    }
                    if (string.IsNullOrEmpty(row["TendoName"].ToString()) ||
                        string.IsNullOrEmpty(row["TendoSKU"].ToString()) ||
                        string.IsNullOrEmpty(row["TendoUom"].ToString()))
                    {
                        MessageBox.Show($"Sản phẩm '{row["ItemName"]}' chưa được map với sản phẩm Tendoo. Vui lòng kiểm tra lại.", "Cảnh báo");
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gọi API: {ex.Message}", "Lỗi");
            }
        }
        public class ProductSimple
        {
            public string Name { get; set; }

            public string SKU { get; set; }

            public string Uom { get; set; }
            public Guid Id { get; set; } = Guid.NewGuid();
            public double Normalprice { get; set; } 
            public int quantity { get; set; }
            public string SkuId { get; set; }
        }
        public class CustomerSimple
        {
            public string Name { get; set; } 
            public string BusinessName { get; set; }    
            public string MST { get; set; } 
            public string Identity { get; set; }
            public Guid contact_id { get; set; } = Guid.NewGuid();
        }
        private async Task Dongbokhachhang()
        {
            List<CustomerSimple> allCustomers =
              new List<CustomerSimple>();
            try
            {
                int currentPage = 1;

                int totalPages = 1;

                do
                {
                    string apiUrl = $"https://apiv2.tendoo.vn/business/api/v1/contact/get-list?page={currentPage}&page_size=10&search=&sort=is_active%20desc&has_analytic=true&state=delivering&business_id=9065ebd6-4c83-4c9c-a6d4-4937e3fda49b";

                    HttpResponseMessage response =
                        await httpClient.GetAsync(apiUrl);

                    string content =
                        await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show(
                            $"Lỗi: {response.StatusCode}\n{content}",
                            "Lỗi");

                        break;
                    }

                    JObject obj = JObject.Parse(content);

                    // lấy sản phẩm
                    List<CustomerSimple> customers =
                     obj["data"]
                     .Select(x => new CustomerSimple
                     {
                         Name = x["name"]?.ToString(),

                         contact_id = x["id"] != null
                             ? Guid.Parse(x["id"].ToString())
                             : Guid.NewGuid(),

                         BusinessName = x["invoice_contact_info"]?["business_name"]?.ToString(),

                         MST = x["invoice_contact_info"]?["tax_number"]?.ToString(),

                         Identity = x["invoice_contact_info"]?["identification_no"]?.ToString()
                     })
                     .ToList();

                    // add vào list tổng
                    allCustomers.AddRange(customers);

                    // lấy tổng page
                    totalPages =
                        (int?)obj["meta"]?["total_pages"] ?? 1;

                    currentPage++;

                }
                while (currentPage <= totalPages);
                progressPanel1.Caption = $"Lấy thành công {allCustomers.Count} khách hàng";
                //MessageBox.Show(
                //    $"Lấy thành công {allCustomers.Count} khách hàng",
                //    "Thành công");
                string qrq = "SELECT * FROM KhachHang";
                var dtkhachhang = ExecuteQuery(qrq, null);
                int stepdo = 0;
                List<string> matchedCustomers = new List<string>();
                foreach (DataRow row in dtkhachhang.Rows)
                {
                    string tenkh = Helpers.RemoveVietnameseDiacritics(
                        Helpers.ConvertVniToUnicode(
                            row["Ten"]?.ToString() ?? ""
                        )
                        .Normalize(NormalizationForm.FormC)
                        .ToLower()
                        .Trim()
                    );

                    string mst = row["MST"]?.ToString()?.Trim() ?? "";


                    foreach (var kh in allCustomers)
                    {
                        string khName = Helpers.RemoveVietnameseDiacritics(
                            (kh.Name ?? "")
                            .Normalize(NormalizationForm.FormC)
                            .ToLower()
                            .Trim()
                        );

                        string businessName = Helpers.RemoveVietnameseDiacritics(
                          (kh.BusinessName ?? "")
                            .Normalize(NormalizationForm.FormC)
                            .ToLower()
                            .Trim()
                        );
                        if (!string.IsNullOrEmpty(businessName) && !matchedCustomers.Contains(businessName))
                            matchedCustomers.Add(businessName);
                        
                        string khMst = kh.MST?.Trim() ?? "";
                        string khIdentity = kh.Identity?.Trim() ?? "";

                        bool isMatch =
                               tenkh == khName
                            || tenkh == businessName
                            || (!string.IsNullOrWhiteSpace(mst) && mst == khMst)
                            || (!string.IsNullOrWhiteSpace(mst) && mst == khIdentity);

                        if (isMatch)
                        {
                            string query = @"UPDATE KhachHang 
                             SET contact_id = ? 
                             WHERE MaSo = ?";

                            var parameters = new OleDbParameter[]
                            {
                new OleDbParameter("?", kh.contact_id.ToString() ?? ""),
                new OleDbParameter("?", row["MaSo"]?.ToString() ?? "")
                            };

                            int rowsAffected = ExecuteQueryResult(query, parameters);

                            stepdo++;

                            break; // tránh update nhiều lần
                        }
                    }
                }

                // Sau khi đồng bộ khách hàng xong thì mới đồng bộ sản phẩm, nhưng phải kiểm tra xem khách hàng từ database có trên tendo không, nếu không có thì sẽ không đồng bộ sản phẩm vì sẽ không map được khách hàng với hóa đơn
                
                await Dongbosanpham();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");

            }

           
        }
        // Lấy danh sách hóa đơn
        private async Task Dongbosanpham()
        {
            //Lấy danh sách sản phẩm từ tendoo để map với sản phẩm của sao việt
            List<ProductSimple> allProducts =
                new List<ProductSimple>(); 
            try
            {
                int currentPage = 1;

                int totalPages = 1;

                do
                {
                    string apiUrl =
                        $"https://apiv2.tendoo.vn/product/api/v1/product/online/get-list" +
                        $"?business_id=9065ebd6-4c83-4c9c-a6d4-4937e3fda49b" +
                        $"&page={currentPage}" +
                        $"&page_size=10" +
                        $"&sort=&name=&category_ids=";

                    HttpResponseMessage response =
                        await httpClient.GetAsync(apiUrl);

                    string content =
                        await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show(
                            $"Lỗi: {response.StatusCode}\n{content}",
                            "Lỗi");

                        break;
                    }

                    JObject obj = JObject.Parse(content);

                    // lấy sản phẩm
                    List<ProductSimple> products =
                        obj["data"]
                        .Select(x => new ProductSimple
                        {
                            Name = x["name"]?.ToString(),

                            SKU = x["list_sku"]?
                                        .FirstOrDefault()?["sku_code"]?
                                        .ToString(),

                            Uom = x["uom"]?.ToString(),
                            Id = x["id"] != null ? Guid.Parse(x["id"].ToString()) : Guid.NewGuid(),
                            Normalprice= x["list_sku"]?
                                        .FirstOrDefault()?["normal_price"]?
                                        .ToObject<double>() ?? 0,
                             quantity = x["list_sku"]?
                            .FirstOrDefault()?["po_details"]?
                            ["quantity"]?
                            .ToObject<int>() ?? 0,
                                SkuId = x["list_sku"]?
                            .FirstOrDefault()?["po_details"]?
                            ["sku_id"]?
                            .ToString() ?? "0"
                        })
                        .ToList();

                    // add vào list tổng
                    allProducts.AddRange(products);

                    // lấy tổng page
                    totalPages =
                        (int?)obj["meta"]?["total_pages"] ?? 1;

                    currentPage++;

                }
                while (currentPage <= totalPages);
                progressPanel1.Caption = $"Lấy thành công {allProducts.Count} sản phẩm";
                //MessageBox.Show(
                //    $"Lấy thành công {allProducts.Count} sản phẩm",
                //    "Thành công");
                 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
                 
            }

            //Lấy ra danh sách sản phẩm để map với sản phẩm của sao việt
            string qrq = "SELECT * FROM Vattu";
            var dtVattu = ExecuteQuery(qrq, null);
            double globalBestScore = 0;

            string globalMessage = "";

            foreach (DataRow row in dtVattu.Rows)
            {
                string tensp =
                    Helpers.ConvertVniToUnicode(
                        row["TenVattu"]?.ToString() ?? "");

                string masp =
                    row["SoHieu"]?.ToString() ?? "";

                double bestScore = 0;

                ProductSimple bestProduct = null;

                // duyệt toàn bộ tendoo
                foreach (var sp in allProducts)
                {
                    string getTen = sp.Name ?? "";

                    string localName =
                        tensp.ToLower().Trim();

                    string localCode =
                        masp.ToLower().Trim();

                    string tedoName =
                        getTen.ToLower().Trim();
                    if(tedoName== "bia corona extra 300ml 1x24 ow bottle ( chai)" && localName== "bia corona extra 300ml 1x24")
                    {
                        int afsdfsd = 10;
                    }
                    if( tedoName.Contains(localName) && tedoName.Length> localName.Length)
                    {
                        bestProduct = sp;
                        bestScore = 100;
                        break; // nếu đã chứa thì không cần so sánh nữa
                    }
                    double score1 =
                        CalculateSimilarity(
                            localName,
                            tedoName);

                    double score2 =
                        CalculateSimilarity(
                            localCode,
                            tedoName);

                    double score3 =
                        Fuzz.TokenSortRatio(
                            localName,
                            tedoName);

                    double finalScore =
                        Math.Max(score1,
                        Math.Max(score2, score3));

                    // chỉ giữ score cao nhất
                    if (finalScore > bestScore)
                    {
                        bestScore = finalScore;
                        bestProduct = sp;
                    }
                }

                // SAU KHI duyệt xong allProducts
                // mới show 1 lần
                if (bestProduct != null)
                {
                    //MessageBox.Show(
                    //    $"Vật tư: {tensp}" +
                    //    $"\nTEDO: {bestProduct.Name}" +
                    //    $"\nScore cao nhất: {bestScore}%");
                    if (bestScore > 80)
                    {
                        string query = @"UPDATE Vattu SET  TendoName =?, TendoSku=?, TendoUom=?,TendoId=?,TendoPrice=?,TendoQuality=?,TendoSkuId=? WHERE MaSo = ?";
                        var parameters = new OleDbParameter[]
                         {
                        new OleDbParameter("?", bestProduct.Name),
                        new OleDbParameter("?", bestProduct.SKU),
                        new OleDbParameter("?", bestProduct.Uom),
                        new OleDbParameter("?", bestProduct.Id.ToString()),
                        new OleDbParameter("?", bestProduct.Normalprice),
                        new OleDbParameter("?", bestProduct.quantity),
                        new OleDbParameter("?", bestProduct.SkuId), 
                        new OleDbParameter("?", row["MaSo"].ToString()),

                         };
                        int rowsAffected = ExecuteQueryResult(query, parameters);
                    }
                }
               
            }
            await AddInvoice();
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
        public static double CalculateSimilarity(string str1, string str2)
        {
            int distance = LevenshteinDistance2(str1, str2);
            int maxLength = Math.Max(str1.Trim().Length, str2.Trim().Length);

            if (maxLength == 0) return 100.0; // Trường hợp cả hai chuỗi đều rỗng

            return (1.0 - (double)distance / maxLength) * 100.0;
        }
        private static int LevenshteinDistance2(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            var d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; i++) d[i, 0] = i;
            for (int j = 0; j <= m; j++) d[0, j] = j;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }
        // Lấy thông tin cửa hàng
        private async Task GetStoreInfo()
        {
            try
            {
                string apiUrl = "https://seller-v2.tendoo.vn/api/store/info";

                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
                string content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Thông tin cửa hàng:\n{content}", "Thành công");
                }
                else
                {
                    MessageBox.Show($"Lỗi: {response.StatusCode}", "Lỗi");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }

        // Tạo hóa đơn mới (ví dụ)
        private async Task CreateInvoice()
        {
            try
            {
                string apiUrl = "https://seller-v2.tendoo.vn/api/invoices/create";

                // Dữ liệu hóa đơn mẫu
                var invoiceData = new
                {
                    customer_name = "Khách hàng A",
                    customer_phone = "0987654321",
                    items = new[]
                    {
                        new { product_id = 1, quantity = 2, price = 100000 }
                    },
                    total = 200000
                };

                string jsonData = System.Text.Json.JsonSerializer.Serialize(invoiceData);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                string result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Tạo hóa đơn thành công!\n{result}", "Thành công");
                }
                else
                {
                    MessageBox.Show($"Lỗi: {response.StatusCode}\n{result}", "Lỗi");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }

        // Hàm POST dữ liệu
        private async Task PostData(string url, object data)
        {
            try
            {
                string jsonData = System.Text.Json.JsonSerializer.Serialize(data);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(url, content);
                string result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Thành công: {result}", "Thông báo");
                }
                else
                {
                    MessageBox.Show($"Lỗi: {response.StatusCode}\n{result}", "Lỗi");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }

        private void progressPanel1_Click(object sender, EventArgs e)
        {

        }

        // Hàm GET dữ liệu (tổng quát)
        private async Task GetData(string url)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                string content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Parse JSON ở đây
                    var data = System.Text.Json.JsonSerializer.Deserialize<object>(content);
                    MessageBox.Show($"Dữ liệu: {content}", "Thành công");
                }
                else
                {
                    MessageBox.Show($"Lỗi: {response.StatusCode}", "Lỗi");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }
    }
}