using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace SaovietTax
{
    public class SmartPDFReader
    {
        // =================== HEURISTIC FUNCTIONS ===================

        /// <summary>
        /// Trích xuất mô tả từ dòng text bằng heuristic
        /// </summary>
        private string ExtractDescriptionHeuristic(string line, string date)
        {
            try
            {
                // Xóa phần ngày khỏi dòng
                string remaining = line.Replace(date, "").Trim();

                // Tìm vị trí số tiền đầu tiên
                var amountMatches = Regex.Matches(remaining, @"[\d,]+\.?\d*");
                if (amountMatches.Count > 0)
                {
                    // Lấy phần trước số tiền đầu tiên
                    int firstAmountIndex = amountMatches[0].Index;
                    string description = remaining.Substring(0, firstAmountIndex).Trim();

                    // Loại bỏ ký tự đặc biệt ở đầu/cuối
                    description = description.TrimStart('-', '.', ' ', '\t');
                    description = description.TrimEnd('-', '.', ' ', '\t');

                    return description;
                }

                // Nếu không tìm thấy số tiền, trả về phần còn lại (loại bỏ các số lẻ)
                string cleaned = Regex.Replace(remaining, @"\d+", "").Trim();
                cleaned = Regex.Replace(cleaned, @"\s+", " "); // Chuẩn hóa khoảng trắng

                return cleaned.Length > 100 ? cleaned.Substring(0, 100) + "..." : cleaned;
            }
            catch
            {
                return "Không xác định";
            }
        }

        /// <summary>
        /// Parse số tiền bằng heuristic dựa trên format phổ biến
        /// </summary>
        private void ParseAmountsHeuristic(string line, ref decimal debit, ref decimal credit, ref decimal balance)
        {
            try
            {
                // Tìm tất cả số tiền trong dòng (format VN: 1.000.000 hoặc 1,000,000)
                var matches = Regex.Matches(line, @"[\d,\.]+");
                var amounts = new List<decimal>();

                foreach (Match match in matches)
                {
                    // Bỏ qua nếu là phần của ngày tháng
                    if (Regex.IsMatch(match.Value, @"^\d{1,2}[/\-\.]\d{1,2}[/\-\.]\d{2,4}$"))
                        continue;

                    // Bỏ qua nếu là số quá nhỏ (dưới 1000, có thể là số TK, mã giao dịch)
                    if (decimal.TryParse(match.Value.Replace(",", "").Replace(".", ""), out decimal amount))
                    {
                        if (amount > 999) // Chỉ lấy số tiền thực sự
                            amounts.Add(amount);
                    }
                }

                // Logic heuristic dựa trên số lượng số tiền tìm được
                if (amounts.Count >= 3)
                {
                    // Format thường gặp: Debit | Credit | Balance
                    // Hoặc Credit | Debit | Balance tùy bank

                    // Heuristic 1: Số cuối thường là số dư
                    balance = amounts[amounts.Count - 1];

                    // Heuristic 2: Trong sao kê, thường có 2 cột số tiền trước số dư
                    if (amounts.Count >= 3)
                    {
                        debit = amounts[amounts.Count - 3];
                        credit = amounts[amounts.Count - 2];

                        // Heuristic 3: Nếu cả debit và credit đều > 0, có thể sai
                        // Trong 1 dòng giao dịch, thường chỉ có 1 cái > 0
                        if (debit > 0 && credit > 0)
                        {
                            // Có thể format là Credit | Debit | Balance
                            // Hoặc ngược lại. Kiểm tra context
                            if (line.ToLower().Contains("chi") || line.ToLower().Contains("nợ"))
                            {
                                // Giữ nguyên
                            }
                            else
                            {
                                // Reset và thử cách khác
                                debit = 0;
                                credit = 0;
                                balance = amounts[amounts.Count - 1];

                                // Thử pattern: chỉ có 1 số tiền + số dư
                                if (amounts.Count == 2)
                                {
                                    credit = amounts[0]; // Mặc định là credit
                                    balance = amounts[1];
                                }
                            }
                        }
                    }
                }
                else if (amounts.Count == 2)
                {
                    // Format: Số tiền | Số dư
                    credit = amounts[0]; // Mặc định là thu vào
                    balance = amounts[1];

                    // Kiểm tra xem có phải là chi tiêu không
                    if (line.ToLower().Contains("chi") || line.ToLower().Contains("nợ") ||
                        line.ToLower().Contains("trừ") || line.ToLower().Contains("-"))
                    {
                        debit = amounts[0];
                        credit = 0;
                    }
                }
                else if (amounts.Count == 1)
                {
                    // Chỉ có số dư
                    balance = amounts[0];
                }
            }
            catch
            {
                // Giữ giá trị mặc định
            }
        }

        /// <summary>
        /// Phân tích cú pháp dòng giao dịch nâng cao
        /// </summary>
        private Transaction ParseTransactionAdvanced(string line)
        {
            var transaction = new Transaction();

            try
            {
                // Pattern 1: DD/MM/YYYY followed by text then amounts
                string pattern1 = @"(\d{2}/\d{2}/\d{4})\s+(.+?)\s+([\d,\.]+)\s+([\d,\.]+)\s+([\d,\.]+)";
                string pattern2 = @"(\d{2}/\d{2}/\d{4})\s+(.+?)\s+(-?[\d,\.]+)\s+(-?[\d,\.]+)";
                string pattern3 = @"(\d{2}-\d{2}-\d{4})\s+(.+?)\s+([\d\.,]+)";

                Match match;
                if ((match = Regex.Match(line, pattern1)).Success)
                {
                    transaction.Date = ParseDate(match.Groups[1].Value);
                    transaction.Description = match.Groups[2].Value.Trim();
                    transaction.Debit = ParseAmount(match.Groups[3].Value);
                    transaction.Credit = ParseAmount(match.Groups[4].Value);
                    transaction.Balance = ParseAmount(match.Groups[5].Value);
                }
                else if ((match = Regex.Match(line, pattern2)).Success)
                {
                    transaction.Date = ParseDate(match.Groups[1].Value);
                    transaction.Description = match.Groups[2].Value.Trim();

                    string amount1 = match.Groups[3].Value;
                    string amount2 = match.Groups[4].Value;

                    // Xác định đâu là debit, credit
                    if (amount1.StartsWith("-"))
                    {
                        transaction.Debit = Math.Abs(ParseAmount(amount1));
                        transaction.Credit = ParseAmount(amount2);
                    }
                    else if (amount2.StartsWith("-"))
                    {
                        transaction.Credit = ParseAmount(amount1);
                        transaction.Debit = Math.Abs(ParseAmount(amount2));
                    }
                    else
                    {
                        // Không có dấu âm, cần heuristic
                        decimal amt1 = ParseAmount(amount1);
                        decimal amt2 = ParseAmount(amount2);

                        // Thường số lớn hơn là balance
                        if (amt1 > amt2)
                        {
                            transaction.Credit = amt2;
                            transaction.Balance = amt1;
                        }
                        else
                        {
                            transaction.Credit = amt1;
                            transaction.Balance = amt2;
                        }
                    }
                }
                else
                {
                    // Fallback to heuristic method
                    var dateMatch = Regex.Match(line, @"\d{2}/\d{2}/\d{4}");
                    if (dateMatch.Success)
                    {
                        transaction.Date = ParseDate(dateMatch.Value);
                        transaction.Description = ExtractDescriptionHeuristic(line, dateMatch.Value);
                        //ParseAmountsHeuristic(line, ref transaction.Debit,
                        //    ref transaction.Credit, ref transaction.Balance);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi parse dòng: {line}");
                Console.WriteLine($"Error: {ex.Message}");
            }

            return transaction;
        }

        /// <summary>
        /// Parse số tiền từ chuỗi
        /// </summary>
        private decimal ParseAmount(string amountStr)
        {
            if (string.IsNullOrWhiteSpace(amountStr))
                return 0;

            // Loại bỏ ký tự không phải số, dấu chấm, dấu phẩy
            string clean = Regex.Replace(amountStr, @"[^\d\.,\-]", "");

            // Xác định separator
            int lastComma = clean.LastIndexOf(',');
            int lastDot = clean.LastIndexOf('.');

            if (lastComma > lastDot && lastComma == clean.Length - 3)
            {
                // Format: 1.234,56 (phần thập phân dùng dấu phẩy)
                clean = clean.Replace(".", "").Replace(",", ".");
            }
            else if (lastDot > lastComma && lastDot == clean.Length - 3)
            {
                // Format: 1,234.56 (phần thập phân dùng dấu chấm)
                clean = clean.Replace(",", "");
            }
            else
            {
                // Format VN: 1.234.567 hoặc 1,234,567
                clean = clean.Replace(",", "").Replace(".", "");
            }

            // Parse số âm
            bool isNegative = clean.StartsWith("-");
            if (isNegative) clean = clean.Substring(1);

            if (decimal.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return isNegative ? -result : result;
            }

            return 0;
        }

        /// <summary>
        /// Parse ngày từ chuỗi
        /// </summary>
        private DateTime ParseDate(string dateStr)
        {
            string[] formats = {
                "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy",
                "dd.MM.yyyy", "d.M.yyyy", "yyyy/MM/dd", "yyyy-MM-dd"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateStr, format,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Tự động detect format của ngân hàng
        /// </summary>
        private BankFormat DetectBankFormat(List<string> lines)
        {
            foreach (var line in lines.Take(20)) // Kiểm tra 20 dòng đầu
            {
                // Vietcombank pattern
                if (line.Contains("VIETCOMBANK") || line.Contains("NGAN HANG TMCP NGOAI THUONG"))
                    return BankFormat.Vietcombank;

                // Techcombank pattern
                if (line.Contains("TECHCOMBANK") || Regex.IsMatch(line, @"TCB\s+\d+"))
                    return BankFormat.Techcombank;

                // BIDV pattern
                if (line.Contains("BIDV") || line.Contains("NGAN HANG DAU TU VA PHAT TRIEN"))
                    return BankFormat.BIDV;

                // VIB pattern (từ file của bạn)
                if (line.Contains("VIB") || line.Contains("VIETNAM INTERNATIONAL BANK"))
                    return BankFormat.VIB;

                // MBBank pattern
                if (line.Contains("MB BANK") || line.Contains("NGAN HANG QUAN DOI"))
                    return BankFormat.MBBank;

                // ACB pattern
                if (line.Contains("ACB") || line.Contains("NGAN HANG TMCP A CHAU"))
                    return BankFormat.ACB;
            }

            // Default format: Date | Description | Debit | Credit | Balance
            return BankFormat.Standard;
        }

        /// <summary>
        /// Parse theo format của từng ngân hàng
        /// </summary>
        private Transaction ParseByBankFormat(string line, BankFormat format)
        {
            switch (format)
            {
                case BankFormat.VIB:
                    return ParseVIBFormat(line);

                case BankFormat.Vietcombank:
                    return ParseVietcombankFormat(line);

                case BankFormat.Techcombank:
                    return ParseTechcombankFormat(line);

                default:
                    return ParseTransactionAdvanced(line);
            }
        }

        private Transaction ParseVIBFormat(string line)
        {
            // Format VIB từ file bạn cung cấp:
            // "26/12/2025 VO THI HUYEN chuyen tien 280,000 424,860,065"
            var transaction = new Transaction();

            try
            {
                // Tách theo khoảng trắng
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 4)
                {
                    // Phần 1: Ngày
                    transaction.Date = ParseDate(parts[0]);

                    // Phần cuối: Số dư
                    transaction.Balance = ParseAmount(parts[parts.Length - 1]);

                    // Phần trước số dư: Số tiền giao dịch
                    transaction.Credit = ParseAmount(parts[parts.Length - 2]);

                    // Tất cả phần giữa: Mô tả
                    var descParts = parts.Skip(1).Take(parts.Length - 3);
                    transaction.Description = string.Join(" ", descParts);
                }
            }
            catch
            {
                // Fallback
            }

            return transaction;
        }

        private Transaction ParseVietcombankFormat(string line)
        {
            // Format Vietcombank thường có cấu trúc bảng rõ ràng
            var transaction = new Transaction();

            try
            {
                // Pattern: Date | Time | Description | Debit | Credit | Balance
                string pattern = @"(\d{2}/\d{2}/\d{4})\s+(\d{2}:\d{2}:\d{2})\s+(.+?)\s+([\d,\.]+)\s+([\d,\.]+)\s+([\d,\.]+)";
                var match = Regex.Match(line, pattern);

                if (match.Success)
                {
                    transaction.Date = ParseDate(match.Groups[1].Value);
                    transaction.Description = match.Groups[3].Value.Trim();
                    transaction.Debit = ParseAmount(match.Groups[4].Value);
                    transaction.Credit = ParseAmount(match.Groups[5].Value);
                    transaction.Balance = ParseAmount(match.Groups[6].Value);
                }
            }
            catch
            {
                // Fallback
            }

            return transaction;
        }

        private Transaction ParseTechcombankFormat(string line)
        {
            // Format Techcombank
            var transaction = new Transaction();

            try
            {
                // Techcombank thường có: Date | Description | Amount | Balance
                string pattern = @"(\d{2}/\d{2}/\d{4})\s+(.+?)\s+(-?[\d,\.]+)\s+([\d,\.]+)";
                var match = Regex.Match(line, pattern);

                if (match.Success)
                {
                    transaction.Date = ParseDate(match.Groups[1].Value);
                    transaction.Description = match.Groups[2].Value.Trim();

                    string amount = match.Groups[3].Value;
                    if (amount.StartsWith("-"))
                    {
                        transaction.Debit = Math.Abs(ParseAmount(amount));
                    }
                    else
                    {
                        transaction.Credit = ParseAmount(amount);
                    }

                    transaction.Balance = ParseAmount(match.Groups[4].Value);
                }
            }
            catch
            {
                // Fallback
            }

            return transaction;
        }
    }

    // =================== ENUMS AND MODELS ===================

    public enum BankFormat
    {
        Standard,
        VIB,
        Vietcombank,
        Techcombank,
        BIDV,
        MBBank,
        ACB,
        VPBank,
        Sacombank
    }

    public class Transaction
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }

        public decimal Amount => Debit > 0 ? -Debit : Credit;

        public override string ToString()
        {
            return $"{Date:dd/MM/yyyy} | {Description} | " +
                   $"Nợ: {Debit:N0} | Có: {Credit:N0} | Số dư: {Balance:N0}";
        }
    }
}