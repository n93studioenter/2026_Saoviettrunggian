using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

namespace SaovietTax.DTO
{
    public class ViettelInvoiceService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://api-vinvoice.viettel.vn/services/einvoiceapplication/api/";
        private readonly string _username;
        private readonly string _password;
        private readonly string _supplierTaxCode;

        public ViettelInvoiceService(string username, string password, string supplierTaxCode)
        {
            _username = username;
            _password = password;
            _supplierTaxCode = supplierTaxCode;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.Timeout = TimeSpan.FromMinutes(5);

            var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);
        }

        // ============ LẤY DANH SÁCH HÓA ĐƠN ============

        /// <summary>
        /// Lấy hóa đơn đầu ra (hóa đơn công ty xuất cho khách hàng)
        /// </summary>
        public async Task<List<Invoices>> GetOutputInvoicesAsync(DateTime startDate, DateTime endDate, int rowPerPage = 100)
        {
            var allInvoices = new List<Invoices>();
            var pageNum = 1;
            var totalPages = 1;

            do
            {
                var request = new InvoiceSearchRequest
                {
                    StartDate = startDate.ToString("yyyy-MM-ddTHH:mm:ss.fff+07:00"),
                    EndDate = endDate.ToString("yyyy-MM-ddTHH:mm:ss.fff+07:00"),
                    RowPerPage = rowPerPage,
                    PageNum = pageNum,
                    InvoiceType = null,
                    TemplateCode = null,
                    InvoiceSeri = null,
                    // Đầu ra: không cần BuyerTaxCode để lấy tất cả
                };

                var result = await GetInvoicesResponseAsync(request, _supplierTaxCode);

                if (result?.Invoices != null && result.Invoices.Any())
                {
                    allInvoices.AddRange(result.Invoices);
                    totalPages = result.TotalRows > 0
                        ? (int)Math.Ceiling((double)result.TotalRows / rowPerPage)
                        : 1;
                }
                else
                {
                    break;
                }

                pageNum++;
            }
            while (pageNum <= totalPages);

            return allInvoices;
        }

        /// <summary>
        /// Lấy hóa đơn đầu vào (hóa đơn công ty nhận từ nhà cung cấp)
        /// </summary>
        public async Task<List<Invoices>> GetInputInvoicesAsync(DateTime startDate, DateTime endDate, string supplierTaxCode = null, int rowPerPage = 100)
        {
            var allInvoices = new List<Invoices>();
            var pageNum = 1;
            var totalPages = 1;

            do
            {
                var request = new InvoiceSearchRequest
                {
                    StartDate = startDate.ToString("yyyy-MM-ddTHH:mm:ss.fff+07:00"),
                    EndDate = endDate.ToString("yyyy-MM-ddTHH:mm:ss.fff+07:00"),
                    RowPerPage = rowPerPage,
                    PageNum = pageNum,
                    InvoiceType = null,
                    TemplateCode = null,
                    InvoiceSeri = null,
                    // ✅ Đầu vào: SupplierTaxCode là MST nhà cung cấp
                    SupplierTaxCode = supplierTaxCode,
                    // ✅ BuyerTaxCode là MST công ty bạn (người mua)
                    BuyerTaxCode = _supplierTaxCode
                };

                // ✅ Gọi API với SupplierTaxCode trên URL
                var result = await GetInvoicesResponseAsync(request, supplierTaxCode ?? _supplierTaxCode);

                if (result?.Invoices != null && result.Invoices.Any())
                {
                    allInvoices.AddRange(result.Invoices);
                    totalPages = result.TotalRows > 0
                        ? (int)Math.Ceiling((double)result.TotalRows / rowPerPage)
                        : 1;
                }
                else
                {
                    break;
                }

                pageNum++;
            }
            while (pageNum <= totalPages);

            return allInvoices;
        }

        /// <summary>
        /// Lấy hóa đơn từ một nhà cung cấp cụ thể (đầu vào)
        /// </summary>
        public async Task<List<Invoices>> GetInvoicesBySupplierAsync(DateTime startDate, DateTime endDate, string buyerTaxCode, int rowPerPage = 100)
        {
            var allInvoices = new List<Invoices>();
            var pageNum = 1;
            var totalPages = 1;

            do
            {
                var request = new InvoiceSearchRequest
                {
                    StartDate = startDate.ToString("yyyy-MM-ddTHH:mm:ss.fff+07:00"),
                    EndDate = endDate.ToString("yyyy-MM-ddTHH:mm:ss.fff+07:00"),
                    RowPerPage = rowPerPage,
                    PageNum = pageNum,
                    BuyerTaxCode = buyerTaxCode,
                    InvoiceType = null,
                    TemplateCode = null,
                    InvoiceSeri = null
                };

                var result = await GetInvoicesResponseAsync(request, _supplierTaxCode);

                if (result?.Invoices != null && result.Invoices.Any())
                {
                    allInvoices.AddRange(result.Invoices);
                    totalPages = result.TotalRows > 0
                        ? (int)Math.Ceiling((double)result.TotalRows / rowPerPage)
                        : 1;
                }
                else
                {
                    break;
                }

                pageNum++;
            }
            while (pageNum <= totalPages);

            return allInvoices;
        }

        private async Task<InvoiceResponse> GetInvoicesResponseAsync(InvoiceSearchRequest request, string supplierTaxCode)
        {
            try
            {
                var url = $"{_baseUrl}InvoiceAPI/InvoiceUtilsWS/getInvoices/{supplierTaxCode}";

                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Error: {response.StatusCode} - {error}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<InvoiceResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? new InvoiceResponse { Invoices = new List<Invoices>() };
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi gọi API Viettel V2: {ex.Message}", ex);
            }
        }

        // ============ LẤY CHI TIẾT 1 HÓA ĐƠN ============

        public async Task<InvoiceDetail> GetInvoiceDetailAsync(string invoiceNo)
        {
            try
            {
                var request = new InvoiceSearchRequest
                {
                    SupplierTaxCode = _supplierTaxCode,
                    InvoiceNo = invoiceNo,
                    StartDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-ddTHH:mm:ss.fff+07:00"),
                    EndDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff+07:00"),
                    RowPerPage = 1,
                    PageNum = 1
                };

                var url = $"{_baseUrl}InvoiceAPI/InvoiceUtilsWS/getInvoices/{_supplierTaxCode}";
                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API Error: {response.StatusCode} - {responseJson}");
                }

                 var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                if (root.TryGetProperty("errorCode", out var errorCode) && errorCode.ValueKind != JsonValueKind.Null)
                {
                    var errorMsg = root.TryGetProperty("description", out var desc) ? desc.GetString() : "Unknown error";
                    throw new Exception($"API returned error: {errorCode} - {errorMsg}");
                }

                if (root.TryGetProperty("invoices", out var invoicesElement))
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var invoices = JsonSerializer.Deserialize<List<InvoiceDetail>>(invoicesElement.GetRawText(), options);
                    return invoices?.FirstOrDefault();
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy chi tiết hóa đơn {invoiceNo}: {ex.Message}", ex);
            }
        }

        // ============ TẢI FILE XML ============

        public async Task<string> DownloadInvoiceXmlAsync(string invoiceSeri, string invoiceNumber, string templateCode, string transactionUuid, string savePath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(invoiceSeri))
                    throw new Exception("invoiceSeri không được để trống");

                if (string.IsNullOrEmpty(invoiceNumber))
                    throw new Exception("invoiceNumber không được để trống");

                if (string.IsNullOrEmpty(transactionUuid))
                    throw new Exception("transactionUuid không được để trống");

                var url = $"{_baseUrl}InvoiceAPI/InvoiceUtilsWS/getInvoiceRepresentationFile";

                var request = new
                {
                    supplierTaxCode = _supplierTaxCode,
                    invoiceSeri = invoiceSeri,
                    invoiceNumber = invoiceNumber,
                    templateCode = templateCode,
                    transactionUuid = transactionUuid,
                    fileType = "101"  // 101 = XML
                };

                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API Error: {response.StatusCode} - {responseJson}");
                }

                 var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                if (root.TryGetProperty("code", out var code) && code.GetInt32() != 0)
                {
                    var message = root.TryGetProperty("message", out var msg) ? msg.GetString() : "Unknown error";
                    throw new Exception($"API Error: {message}");
                }

                if (root.TryGetProperty("Object", out var objectElement))
                {
                    var objectString = objectElement.GetString();
                    if (!string.IsNullOrEmpty(objectString))
                    {
                         var objectDoc = JsonDocument.Parse(objectString);
                        var objectRoot = objectDoc.RootElement;

                        if (objectRoot.TryGetProperty("XML", out var xmlElement))
                        {
                            var xmlBase64 = xmlElement.GetString();
                            if (!string.IsNullOrEmpty(xmlBase64))
                            {
                                var xmlBytes = Convert.FromBase64String(xmlBase64);
                                var xmlContent = Encoding.UTF8.GetString(xmlBytes);

                                if (!string.IsNullOrEmpty(savePath))
                                {
                                    if (!Directory.Exists(savePath))
                                        Directory.CreateDirectory(savePath);

                                    var fileName = $"{invoiceSeri}{invoiceNumber}_{DateTime.Now:yyyyMMddHHmmss}.xml";
                                    var fullPath = Path.Combine(savePath, fileName);
                                    File.WriteAllText(fullPath, xmlContent, Encoding.UTF8);
                                    return fullPath;
                                }
                                return xmlContent;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(savePath))
                {
                    var debugPath = Path.Combine(savePath, $"{invoiceSeri}{invoiceNumber}_debug.json");
                    File.WriteAllText(debugPath, responseJson);
                }

                return responseJson;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải file XML cho hóa đơn {invoiceSeri}{invoiceNumber}: {ex.Message}", ex);
            }
        }

        // ============ TẢI FILE PDF ============

        public async Task<string> DownloadInvoicePdfAsync(string invoiceSeri, string invoiceNumber, string templateCode, string transactionUuid, string savePath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(invoiceSeri))
                    throw new Exception("invoiceSeri không được để trống");

                if (string.IsNullOrEmpty(invoiceNumber))
                    throw new Exception("invoiceNumber không được để trống");

                if (string.IsNullOrEmpty(transactionUuid))
                    throw new Exception("transactionUuid không được để trống");

                var url = $"{_baseUrl}InvoiceAPI/InvoiceUtilsWS/getInvoiceRepresentationFile";

                var request = new
                {
                    supplierTaxCode = _supplierTaxCode,
                    invoiceSeri = invoiceSeri,
                    invoiceNumber = invoiceNumber,
                    templateCode = templateCode,
                    transactionUuid = transactionUuid,
                    fileType = "102"  // 102 = PDF
                };

                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API Error: {response.StatusCode} - {responseJson}");
                }

                 var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                if (root.TryGetProperty("code", out var code) && code.GetInt32() != 0)
                {
                    var message = root.TryGetProperty("message", out var msg) ? msg.GetString() : "Unknown error";
                    throw new Exception($"API Error: {message}");
                }

                if (root.TryGetProperty("Object", out var objectElement))
                {
                    var objectString = objectElement.GetString();
                    if (!string.IsNullOrEmpty(objectString))
                    {
                         var objectDoc = JsonDocument.Parse(objectString);
                        var objectRoot = objectDoc.RootElement;

                        if (objectRoot.TryGetProperty("PDF", out var pdfElement))
                        {
                            var pdfBase64 = pdfElement.GetString();
                            if (!string.IsNullOrEmpty(pdfBase64))
                            {
                                var pdfBytes = Convert.FromBase64String(pdfBase64);

                                if (!string.IsNullOrEmpty(savePath))
                                {
                                    if (!Directory.Exists(savePath))
                                        Directory.CreateDirectory(savePath);

                                    var fileName = $"{invoiceSeri}{invoiceNumber}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                                    var fullPath = Path.Combine(savePath, fileName);
                                    File.WriteAllBytes(fullPath, pdfBytes);
                                    return fullPath;
                                }
                                return Convert.ToBase64String(pdfBytes);
                            }
                        }
                    }
                }

                return responseJson;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải file PDF cho hóa đơn {invoiceSeri}{invoiceNumber}: {ex.Message}", ex);
            }
        }

        // ============ LẤY CHI TIẾT + TẢI XML CHO NHIỀU HÓA ĐƠN ============

        public async Task<List<InvoiceDetail>> GetInvoiceDetailsWithXmlAsync(List<string> invoiceNos, string xmlSavePath = null, Action<string, int, int> onProgress = null)
        {
            var results = new List<InvoiceDetail>();
            var total = invoiceNos.Count;
            var processed = 0;

            foreach (var invoiceNo in invoiceNos)
            {
                try
                {
                    processed++;
                    onProgress?.Invoke(invoiceNo, processed, total);

                    var detail = await GetInvoiceDetailAsync(invoiceNo);
                    if (detail != null)
                    {
                        try
                        {
                            var xmlPath = await DownloadInvoiceXmlAsync(
                                detail.InvoiceSeri,
                                detail.InvoiceNumber,
                                detail.TemplateCode,
                                detail.TransactionUuid,
                                xmlSavePath
                            );
                            detail.XmlPath = xmlPath;
                        }
                        catch (Exception ex)
                        {
                            detail.XmlError = ex.Message;
                        }

                        results.Add(detail);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Lỗi xử lý {invoiceNo}: {ex.Message}");
                }
            }

            return results;
        }
    }

    // ============================================
    // MODELS
    // ============================================

    public class InvoiceSearchRequest
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int RowPerPage { get; set; }
        public int PageNum { get; set; }
        public string InvoiceType { get; set; }
        public string BuyerTaxCode { get; set; }
        public string SupplierTaxCode { get; set; }
        public string TemplateCode { get; set; }
        public string InvoiceSeri { get; set; }
        public string InvoiceNo { get; set; }
    }

    public class InvoiceResponse
    {
        public string ErrorCode { get; set; }
        public string Description { get; set; }
        public int TotalRows { get; set; }
        public List<Invoices> Invoices { get; set; }
    }

    public class Invoices
    {
        public long InvoiceId { get; set; }
        public string InvoiceType { get; set; }
        public string AdjustmentType { get; set; }
        public string TemplateCode { get; set; }
        public string InvoiceSeri { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceNo { get; set; }
        public string Currency { get; set; }
        public decimal Total { get; set; }
        public decimal TotalBeforeTax { get; set; }
        public decimal TaxAmount { get; set; }
        public string IssueDateStr { get; set; }
        public int State { get; set; }
        public string SupplierTaxCode { get; set; }
        public string BuyerTaxCode { get; set; }
        public string BuyerName { get; set; }
        public int PaymentStatus { get; set; }
        public string PaymentStatusName { get; set; }
        public string TransactionUuid { get; set; }
        public string OriginalInvoiceId { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentTime { get; set; }
        public string CustomerId { get; set; }
        public string ContractNo { get; set; }

        public DateTime IssueDate => DateTime.TryParse(IssueDateStr, out var date) ? date : DateTime.MinValue;
    }

    public class InvoiceDetailResponse
    {
        public string ErrorCode { get; set; }
        public string Description { get; set; }
        public int TotalRow { get; set; }
        public List<InvoiceDetail> Invoices { get; set; }
    }

    public class InvoiceDetail : Invoices
    {
        public List<InvoiceItem> Items { get; set; }
        public List<InvoiceMetadata> Metadata { get; set; }
        public string XmlContent { get; set; }
        public string XmlPath { get; set; }
        public string XmlError { get; set; }
    }

    public class InvoiceItem
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public decimal VatRate { get; set; }
        public decimal VatAmount { get; set; }
        public string Unit { get; set; }
    }

    public class InvoiceMetadata
    {
        public int InvoiceCustomFieldId { get; set; }
        public string KeyTag { get; set; }
        public string KeyLabel { get; set; }
        public string ValueType { get; set; }
        public string StringValue { get; set; }
        public decimal? NumberValue { get; set; }
        public DateTime? DateValue { get; set; }
    }
}