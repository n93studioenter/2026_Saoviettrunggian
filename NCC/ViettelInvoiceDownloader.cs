using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SaovietTax.NCC
{
    public class ViettelInvoiceDownloader
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://vinvoice.viettel.vn";
        private readonly string _username = "0100109106-997"; // Thay bằng user của bạn
        private readonly string _password = "123456a@A";      // Thay bằng pass của bạn

        public ViettelInvoiceDownloader()
        {
            _httpClient = new HttpClient();

            // 1. Thêm Header Xác thực Basic Auth
            var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);

            // 2. Các Header cần thiết khác
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
        }

        public async Task<string> DownloadInvoiceAsync(string sellerTaxCode, string reservationCode, string fileType = "xml")
        {
            try
            {
                // Endpoint đúng theo tài liệu
                string endpoint = $"{_baseUrl}/InvoiceAPI/InvoiceWS/downloadInvoice/{sellerTaxCode}";

                var requestData = new
                {
                    supplierTaxCode = sellerTaxCode, // Có thể thêm hoặc trùng với parameter trên URL
                    reservationCode = reservationCode,
                    fileType = fileType
                };

                string jsonRequest = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                Console.WriteLine($"📤 Gửi request tới: {endpoint}");
                Console.WriteLine($"📦 Dữ liệu: {jsonRequest}");

                HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("✅ Tải hóa đơn thành công!");
                    return result;
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Lỗi: {response.StatusCode} - {error}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception: {ex.Message}");
                return null;
            }
        }
    }
}