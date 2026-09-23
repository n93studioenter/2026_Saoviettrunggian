using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;


namespace SaovietTax.DTO
{
    public static class BankStatementReader
    {
        // ============================================================
        // CẤU HÌNH CỘT
        // ============================================================

        // T7.pdf có chiều rộng khoảng 782.
        //
        // STT
        // Số giao dịch
        // Ngày giao dịch
        // Ngày hiệu lực
        // Nội dung
        // Số tiền
        // Số dư
        //
        // Các giá trị này là khoảng X, không phải tọa độ tuyệt đối.
        // Nếu ngân hàng thay đổi mẫu PDF thì chỉnh ở đây.

        private const double STT_MIN_X = 0;
        private const double STT_MAX_X = 45;

        private const double TRANSACTION_MIN_X = 35;
        private const double TRANSACTION_MAX_X = 155;

        private const double DATE_MIN_X = 145;
        private const double DATE_MAX_X = 250;

        private const double EFFECTIVE_DATE_MIN_X = 235;
        private const double EFFECTIVE_DATE_MAX_X = 325;

        private const double CONTENT_MIN_X = 315;
        private const double CONTENT_MAX_X = 570;

        private const double MONEY_MIN_X = 540;

        // Khoảng sai lệch Y khi gom các Word thành một dòng
        private const double LINE_Y_TOLERANCE = 3.5;


        // ============================================================
        // PUBLIC
        // ============================================================

        /// <summary>
        /// Đọc file PDF sổ phụ ngân hàng.
        /// </summary>
        public static DataTable Read(string filePath)
        {
            using (PdfDocument document = PdfDocument.Open(filePath))
            {
                decimal? openingBalance =
                    FindOpeningBalance(document);

                return Read(document, openingBalance);
            }
        }


        /// <summary>
        /// Đọc PDF nhưng cho phép truyền số dư đầu kỳ.
        /// </summary>
        public static DataTable Read(
            string filePath,
            decimal openingBalance)
        {
            using (PdfDocument document = PdfDocument.Open(filePath))
            {
                return Read(document, openingBalance);
            }
        }


        private static DataTable Read(
            PdfDocument document,
            decimal? openingBalance)
        {
            DataTable table = CreateTable();

            BankRow current = null;

            decimal? previousBalance = openingBalance;

            foreach (Page page in document.GetPages())
            {
                List<List<Word>> lines =
                    GetLines(page);

                foreach (List<Word> originalLine in lines)
                {
                    if (originalLine == null ||
                        originalLine.Count == 0)
                        continue;

                    List<Word> line =
                        originalLine
                        .OrderBy(w => w.BoundingBox.Left)
                        .ToList();


                    // ====================================================
                    // KIỂM TRA CÓ PHẢI DÒNG GIAO DỊCH MỚI KHÔNG
                    // ====================================================

                    int? stt = FindStt(line);

                    if (stt.HasValue)
                    {
                        // Lưu giao dịch trước
                        if (current != null)
                        {
                            FinalizeRow(
                                current,
                                previousBalance);

                            AddRow(
                                table,
                                current);

                            if (current.SoDu.HasValue)
                            {
                                previousBalance =
                                    current.SoDu.Value;
                            }
                        }


                        // Tạo giao dịch mới
                        current = new BankRow();

                        current.STT = stt.Value;


                        // Đọc các thông tin nằm trên dòng đầu
                        ProcessFirstLine(
                            current,
                            line);
                    }
                    else
                    {
                        // Không có STT
                        // => đây có thể là dòng tiếp theo
                        // của Nội dung giao dịch hiện tại.

                        if (current == null)
                            continue;

                        ProcessContinuationLine(
                            current,
                            line);
                    }
                }
            }


            // ============================================================
            // LƯU GIAO DỊCH CUỐI
            // ============================================================

            if (current != null)
            {
                FinalizeRow(
                    current,
                    previousBalance);

                AddRow(
                    table,
                    current);
            }

            return table;
        }


        // ============================================================
        // TẠO DATATABLE
        // ============================================================

        private static DataTable CreateTable()
        {
            DataTable dt = new DataTable(
                "BankStatement");

            dt.Columns.Add(
                "STT",
                typeof(int));

            dt.Columns.Add(
                "SoGiaoDich",
                typeof(string));

            dt.Columns.Add(
                "NgayGiaoDich",
                typeof(DateTime));

            dt.Columns.Add(
                "NgayHieuLuc",
                typeof(DateTime));

            dt.Columns.Add(
                "NoiDung",
                typeof(string));

            dt.Columns.Add(
                "TienRut",
                typeof(decimal));

            dt.Columns.Add(
                "TienGui",
                typeof(decimal));

            dt.Columns.Add(
                "SoDu",
                typeof(decimal));

            return dt;
        }


        // ============================================================
        // XỬ LÝ DÒNG ĐẦU CỦA GIAO DỊCH
        // ============================================================

        private static void ProcessFirstLine(
            BankRow row,
            List<Word> line)
        {
            List<Word> sorted =
                line
                .OrderBy(w => w.BoundingBox.Left)
                .ToList();


            // --------------------------------------------------------
            // SỐ GIAO DỊCH
            // --------------------------------------------------------

            Word transactionWord =
                sorted.FirstOrDefault(w =>
                {
                    double x =
                        w.BoundingBox.Left;

                    return x >= TRANSACTION_MIN_X &&
                           x < TRANSACTION_MAX_X &&
                           IsTransactionNumber(w.Text);
                });

            if (transactionWord != null)
            {
                row.SoGiaoDich =
                    transactionWord.Text.Trim();
            }


            // --------------------------------------------------------
            // NGÀY
            // --------------------------------------------------------

            foreach (Word word in sorted)
            {
                string text =
                    word.Text.Trim();

                if (string.IsNullOrWhiteSpace(text))
                    continue;

                double x =
                    word.BoundingBox.Left;


                // Ngày giao dịch
                if (x >= DATE_MIN_X &&
                    x < DATE_MAX_X)
                {
                    if (IsDate(text))
                    {
                        row.NgayGiaoDich =
                            ParseDate(text);
                    }
                    else if (IsTime(text))
                    {
                        AddTimeToTransactionDate(
                            row,
                            text);
                    }
                }


                // Ngày hiệu lực
                else if (
                    x >= EFFECTIVE_DATE_MIN_X &&
                    x < EFFECTIVE_DATE_MAX_X)
                {
                    if (IsDate(text))
                    {
                        row.NgayHieuLuc =
                            ParseDate(text);
                    }
                }
            }


            // --------------------------------------------------------
            // NỘI DUNG
            // --------------------------------------------------------

            List<string> content =
                sorted
                .Where(w =>
                {
                    double x =
                        w.BoundingBox.Left;

                    return x >= CONTENT_MIN_X &&
                           x < MONEY_MIN_X;
                })
                .Select(w => w.Text.Trim())
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x))
                .ToList();


            AddContent(
                row,
                content);


            // --------------------------------------------------------
            // TIỀN
            // --------------------------------------------------------

            ProcessMoney(
                row,
                sorted);
        }


        // ============================================================
        // XỬ LÝ DÒNG TIẾP THEO
        // ============================================================

        private static void ProcessContinuationLine(
            BankRow row,
            List<Word> line)
        {
            List<Word> sorted =
                line
                .OrderBy(w => w.BoundingBox.Left)
                .ToList();


            // --------------------------------------------------------
            // NỘI DUNG
            // --------------------------------------------------------

            List<string> content =
                sorted
                .Where(w =>
                {
                    double x =
                        w.BoundingBox.Left;

                    return x >= CONTENT_MIN_X &&
                           x < MONEY_MIN_X;
                })
                .Select(w => w.Text.Trim())
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x))
                .ToList();


            AddContent(
                row,
                content);


            // --------------------------------------------------------
            // TIỀN
            //
            // Thông thường chỉ dòng cuối của giao dịch
            // có tiền + số dư.
            // --------------------------------------------------------

            ProcessMoney(
                row,
                sorted);
        }


        // ============================================================
        // XỬ LÝ TIỀN
        // ============================================================

        private static void ProcessMoney(
            BankRow row,
            List<Word> line)
        {
            List<MoneyWord> money =
                line
                .Where(w =>
                    w.BoundingBox.Left >= MONEY_MIN_X)
                .Select(w =>
                {
                    decimal value;

                    if (!TryParseMoney(
                        w.Text,
                        out value))
                    {
                        return null;
                    }

                    return new MoneyWord
                    {
                        Value = value,

                        X = w.BoundingBox.Left
                    };
                })
                .Where(x => x != null)
                .OrderBy(x => x.X)
                .ToList();


            if (money.Count == 0)
                return;


            // --------------------------------------------------------
            // Trường hợp bình thường:
            //
            //     102.000.000   147.852.674
            //     ^ amount      ^ balance
            //
            // Số cuối cùng = Số dư
            // Số trước đó = Số tiền giao dịch
            // --------------------------------------------------------

            if (money.Count >= 2)
            {
                decimal balance =
                    money[money.Count - 1].Value;

                decimal amount =
                    money[money.Count - 2].Value;

                row.SoDu = balance;

                row.SoTien = amount;
            }
            else
            {
                // Một số trường hợp parser có thể gom
                // thiếu một số.
                //
                // Ta tạm giữ số này.
                row.PossibleMoney =
                    money[0].Value;
            }
        }


        // ============================================================
        // HOÀN TẤT GIAO DỊCH
        // ============================================================

        private static void FinalizeRow(
            BankRow row,
            decimal? previousBalance)
        {
            // Nếu chưa lấy được số dư
            // nhưng có tiền tạm thời
            if (!row.SoDu.HasValue &&
                row.PossibleMoney.HasValue)
            {
                // Không đủ thông tin để xác định
                // số dư.
            }


            // Không có số tiền
            if (!row.SoTien.HasValue)
                return;


            // Không có số dư
            if (!row.SoDu.HasValue)
                return;


            decimal amount =
                row.SoTien.Value;

            decimal balance =
                row.SoDu.Value;


            // ========================================================
            // XÁC ĐỊNH TIỀN RÚT / TIỀN GỬI
            // ========================================================

            if (previousBalance.HasValue)
            {
                decimal oldBalance =
                    previousBalance.Value;


                // Số dư tăng
                // => tiền gửi
                if (balance > oldBalance)
                {
                    row.TienGui = amount;
                    row.TienRut = 0;
                }


                // Số dư giảm
                // => tiền rút
                else if (balance < oldBalance)
                {
                    row.TienRut = amount;
                    row.TienGui = 0;
                }


                // Số dư không đổi
                else
                {
                    row.TienRut = 0;
                    row.TienGui = 0;
                }
            }
            else
            {
                // Không có số dư trước đó.
                //
                // Không thể xác định chính xác
                // rút hay gửi.
                row.TienRut = 0;
                row.TienGui = 0;
            }
        }


        // ============================================================
        // THÊM VÀO DATATABLE
        // ============================================================

        private static void AddRow(
            DataTable table,
            BankRow row)
        {
            if (row == null)
                return;


            if (row.STT <= 0)
                return;


            DataRow dr =
                table.NewRow();


            dr["STT"] =
                row.STT;


            dr["SoGiaoDich"] =
                row.SoGiaoDich ??
                "";


            dr["NgayGiaoDich"] =
                row.NgayGiaoDich.HasValue
                    ? (object)row.NgayGiaoDich.Value
                    : DBNull.Value;


            dr["NgayHieuLuc"] =
                row.NgayHieuLuc.HasValue
                    ? (object)row.NgayHieuLuc.Value
                    : DBNull.Value;


            dr["NoiDung"] =
                CleanContent(
                    row.NoiDung);


            dr["TienRut"] =
                row.TienRut ??
                0;


            dr["TienGui"] =
                row.TienGui ??
                0;


            dr["SoDu"] =
                row.SoDu ??
                0;


            table.Rows.Add(dr);
        }


        // ============================================================
        // GOM DÒNG
        // ============================================================

        private static List<List<Word>> GetLines(
            Page page)
        {
            List<Word> words =
                page.GetWords()
                    .ToList();


            // Sắp xếp từ trên xuống dưới.
            //
            // PdfPig dùng hệ tọa độ Bottom/Top từ dưới lên.
            //
            // Top càng lớn => càng gần phía trên.

            words = words
                .OrderByDescending(
                    w => w.BoundingBox.Top)
                .ThenBy(
                    w => w.BoundingBox.Left)
                .ToList();


            List<List<Word>> lines =
                new List<List<Word>>();


            foreach (Word word in words)
            {
                List<Word> line =
                    lines.FirstOrDefault(x =>
                    {
                        if (x.Count == 0)
                            return false;

                        double y =
                            x[0].BoundingBox.Top;

                        return Math.Abs(
                            y -
                            word.BoundingBox.Top)
                            <= LINE_Y_TOLERANCE;
                    });


                if (line == null)
                {
                    lines.Add(
                        new List<Word>
                        {
                        word
                        });
                }
                else
                {
                    line.Add(word);
                }
            }


            return lines;
        }


        // ============================================================
        // TÌM STT
        // ============================================================

        private static int? FindStt(
            List<Word> line)
        {
            foreach (Word word in line)
            {
                double x =
                    word.BoundingBox.Left;


                if (x < STT_MIN_X ||
                    x > STT_MAX_X)
                    continue;


                string text =
                    word.Text.Trim();


                int stt;


                if (!int.TryParse(
                    text,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out stt))
                {
                    continue;
                }


                // STT trong file này từ 1 -> 235.
                if (stt >= 1 &&
                    stt <= 10000)
                {
                    return stt;
                }
            }


            return null;
        }


        // ============================================================
        // SỐ GIAO DỊCH
        // ============================================================

        private static bool IsTransactionNumber(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;


            text = text.Trim();


            // Loại bỏ một số ký tự đặc biệt
            // có thể xuất hiện trong mã giao dịch.
            if (IsDate(text))
                return false;


            if (IsTime(text))
                return false;


            // Mã giao dịch thường:
            // FT...
            // LD...
            // TT...
            // PD...
            // 050015150952-20260731
            //
            // Không phải số thuần.

            bool hasLetter =
                text.Any(char.IsLetter);


            bool hasDigit =
                text.Any(char.IsDigit);


            return hasLetter && hasDigit;
        }


        // ============================================================
        // NGÀY
        // ============================================================

        private static bool IsDate(
            string text)
        {
            DateTime date;


            return DateTime.TryParseExact(
                text.Trim(),
                "dd-MM-yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
        }


        private static DateTime ParseDate(
            string text)
        {
            return DateTime.ParseExact(
                text.Trim(),
                "dd-MM-yyyy",
                CultureInfo.InvariantCulture);
        }


        // ============================================================
        // TIME
        // ============================================================

        private static bool IsTime(
            string text)
        {
            TimeSpan time;


            return TimeSpan.TryParseExact(
                text.Trim(),
                @"hh\:mm\:ss",
                CultureInfo.InvariantCulture,
                out time);
        }


        private static void AddTimeToTransactionDate(
            BankRow row,
            string text)
        {
            if (!row.NgayGiaoDich.HasValue)
                return;


            TimeSpan time;


            if (!TimeSpan.TryParse(
                text,
                out time))
                return;


            row.NgayGiaoDich =
                row.NgayGiaoDich.Value
                    .Date
                    .Add(time);
        }


        // ============================================================
        // MONEY
        // ============================================================

        private static bool TryParseMoney(
            string text,
            out decimal value)
        {
            value = 0;


            if (string.IsNullOrWhiteSpace(text))
                return false;


            text = text.Trim();


            // PDF có thể có ký tự lạ
            // ở cuối số.
            text = text
                .Replace(",", "")
                .Replace(" ", "")
                .Trim();


            // File T7 dùng dấu . làm phân cách hàng nghìn.
            //
            // 102.000.000
            // 15.400
            // 207.706.435

            return decimal.TryParse(
                text,
                NumberStyles.Number,
                new CultureInfo("vi-VN"),
                out value);
        }


        // ============================================================
        // CONTENT
        // ============================================================

        private static void AddContent(
            BankRow row,
            List<string> content)
        {
            foreach (string text in content)
            {
                if (string.IsNullOrWhiteSpace(text))
                    continue;


                string value =
                    text.Trim();


                // Tránh duplicate
                // khi PDF có text overlap.
                if (row.NoiDung.Count > 0 &&
                    row.NoiDung.Last()
                        .Equals(
                            value,
                            StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }


                row.NoiDung.Add(
                    value);
            }
        }


        private static string CleanContent(
            List<string> content)
        {
            if (content == null ||
                content.Count == 0)
            {
                return "";
            }


            StringBuilder sb =
                new StringBuilder();


            foreach (string text in content)
            {
                if (string.IsNullOrWhiteSpace(text))
                    continue;


                string value =
                    text.Trim();


                if (sb.Length > 0)
                    sb.Append(" ");


                sb.Append(value);
            }


            return sb.ToString()
                .Replace("  ", " ")
                .Trim();
        }


        // ============================================================
        // ĐỌC SỐ DƯ ĐẦU KỲ
        // ============================================================

        private static decimal? FindOpeningBalance(
            PdfDocument document)
        {
            Page firstPage =
                document.GetPages()
                    .FirstOrDefault();


            if (firstPage == null)
                return null;


            List<Word> words =
                firstPage.GetWords()
                    .OrderByDescending(
                        w => w.BoundingBox.Top)
                    .ThenBy(
                        w => w.BoundingBox.Left)
                    .ToList();


            // Tìm text:
            //
            // Số dư đầu kỳ: 45.852.674
            //
            // Vì PDF có thể tách thành:
            //
            // Số
            // dư
            // đầu
            // kỳ
            // :
            // 45.852.674
            //
            // nên ta tìm một số tiền nằm gần
            // vùng header.

            for (int i = 0;
                 i < words.Count;
                 i++)
            {
                string text =
                    words[i].Text.Trim();


                if (!text.Equals(
                    "đầu",
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }


                // Tìm các Word tiếp theo gần đó
                // trong phạm vi khoảng 100 px.

                double y =
                    words[i].BoundingBox.Top;


                var candidates =
                    words
                    .Where(w =>
                        Math.Abs(
                            w.BoundingBox.Top -
                            y) < 15)
                    .Where(w =>
                        w.BoundingBox.Left >
                        words[i].BoundingBox.Left)
                    .OrderBy(
                        w => w.BoundingBox.Left)
                    .ToList();


                foreach (Word candidate in candidates)
                {
                    decimal value;


                    if (TryParseMoney(
                        candidate.Text,
                        out value))
                    {
                        // Số dư đầu kỳ trong file:
                        // 45.852.674
                        //
                        // Không nhận số quá lớn
                        // hoặc bằng 0.

                        if (value > 0)
                            return value;
                    }
                }
            }


            // Fallback:
            //
            // Tìm tất cả số tiền ở header.
            // Số dư đầu kỳ nằm trước số dư cuối kỳ.

            List<decimal> headerMoney =
                words
                .Where(w =>
                    w.BoundingBox.Top > 900)
                .Select(w =>
                {
                    decimal value;

                    if (TryParseMoney(
                        w.Text,
                        out value))
                    {
                        return (decimal?)value;
                    }

                    return null;
                })
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Where(x => x > 0)
                .ToList();


            if (headerMoney.Count > 0)
            {
                // Theo cấu trúc T7.pdf:
                // số dư đầu kỳ xuất hiện trước
                // số dư cuối kỳ.

                return headerMoney.First();
            }


            return null;
        }


        // ============================================================
        // CLASS NỘI BỘ
        // ============================================================

        private class BankRow
        {
            public int STT { get; set; }


            public string SoGiaoDich
            {
                get;
                set;
            }


            public DateTime? NgayGiaoDich
            {
                get;
                set;
            }


            public DateTime? NgayHieuLuc
            {
                get;
                set;
            }


            public List<string> NoiDung
            {
                get;
                set;
            }
                = new List<string>();


            // Số tiền giao dịch
            public decimal? SoTien
            {
                get;
                set;
            }


            // Số dư sau giao dịch
            public decimal? SoDu
            {
                get;
                set;
            }


            // Nếu chỉ bắt được một số tiền
            public decimal? PossibleMoney
            {
                get;
                set;
            }


            public decimal? TienRut
            {
                get;
                set;
            }


            public decimal? TienGui
            {
                get;
                set;
            }
        }


        private class MoneyWord
        {
            public double X
            {
                get;
                set;
            }


            public decimal Value
            {
                get;
                set;
            }
        }
    }
}