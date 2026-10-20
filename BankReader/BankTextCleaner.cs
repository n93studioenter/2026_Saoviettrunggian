using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SaovietTax.BankReader
{
    public static class BankTextCleaner
    {
        public static string CleanBankDescription(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string text = input.Trim();

            // Chuẩn hóa khoảng trắng, tab và xuống dòng
            text = Regex.Replace(text, @"\s+", " ");

            // Xóa dấu phân cách thừa ở cuối
            text = Regex.Replace(text, @"[\s#;|]+$", "");

            // Xóa chuỗi ngày-giờ tự động ở cuối:
            // Ví dụ: 060126-17:49:46 hoặc 030826-14:53:57
            text = Regex.Replace(
                text,
                @"\s+\d{6}-\d{2}:\d{2}:\d{2}$",
                "",
                RegexOptions.IgnoreCase
            );

            // Xóa mã tham chiếu dạng chữ-số dài ở cuối.
            // Chỉ áp dụng cho các mẫu mã đã xác định,
            // không xóa số hóa đơn/hợp đồng thông thường.
            text = Regex.Replace(
                text,
                @"\s+\b(?=[A-Z0-9]{12,}\b)(?=[A-Z0-9]*[A-Z])(?=[A-Z0-9]*\d)[A-Z0-9]+\b$",
                "",
                RegexOptions.IgnoreCase
            );

            // Làm sạch dấu phân cách còn sót lại
            text = Regex.Replace(text, @"[\s#;|]+$", "");

            return text.Trim();
        }

        public static string ToCsvField(string input)
        {
            string text = CleanBankDescription(input);

            // Escape dấu nháy kép theo chuẩn CSV
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }
    }
}