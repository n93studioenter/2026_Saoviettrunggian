using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SaovietTax.BankReader
{
    public class BankTransaction
    {
        public DateTime TransactionDate { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal Debit { get; set; }

        public decimal Credit { get; set; }

        public decimal Balance { get; set; }

        // Thuộc tính tiện dùng cho kế toán
        public decimal Amount
        {
            get
            {
                return Debit > 0 ? Debit : Credit;
            }
        }

        public bool IsDebit
        {
            get { return Debit > 0; }
        }

        public bool IsCredit
        {
            get { return Credit > 0; }
        }
    }
    public class VibPdfReader
    {
        public static List<BankTransaction> Read(string pdfPath)
        {
            var list = new List<BankTransaction>();

            using (PdfReader reader = new PdfReader(pdfPath))
            using (PdfDocument pdfDoc = new PdfDocument(reader))
            {
                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    var strategy = new SimpleTextExtractionStrategy();
                    var page = pdfDoc.GetPage(i);
                    var text = PdfTextExtractor.GetTextFromPage(page, strategy);

                    var lines = text.Split('\n')
                                    .Select(x => x.Trim())
                                    .Where(x => !string.IsNullOrWhiteSpace(x))
                                    .ToList();

                    for (int j = 0; j < lines.Count; j++)
                    {
                        // chỉ xử lý dòng bắt đầu bằng ngày
                        if (!System.Text.RegularExpressions.Regex.IsMatch(lines[j], @"^\d{2}/\d{2}/\d{4}"))
                            continue;

                        string dateStr = lines[j].Substring(0, 10);

                        DateTime date = DateTime.ParseExact(
                            dateStr,
                            "dd/MM/yyyy",
                            CultureInfo.InvariantCulture);

                        string description = "";
                        decimal debit = 0;
                        decimal credit = 0;
                        decimal balance = 0;

                        int k = j + 1;

                        // gom description cho tới khi gặp dòng có tiền
                        while (k < lines.Count)
                        {
                            var moneyMatch = System.Text.RegularExpressions.Regex.Match(
                                lines[k],
                                @"(\d{1,3}(,\d{3})+)\s+(\d{1,3}(,\d{3})+)$");

                            if (moneyMatch.Success)
                            {
                                decimal amount = ParseMoney(moneyMatch.Groups[1].Value);
                                balance = ParseMoney(moneyMatch.Groups[3].Value);

                                // xác định debit/credit theo biến động balance
                                if (list.Count > 0)
                                {
                                    if (balance < list.Last().Balance)
                                        debit = amount;
                                    else
                                        credit = amount;
                                }
                                else
                                {
                                    credit = amount;
                                }

                                break;
                            }

                            description += " " + lines[k];
                            k++;
                        }

                        list.Add(new BankTransaction
                        {
                            TransactionDate = date,
                            Description = description.Trim(),
                            Debit = debit,
                            Credit = credit,
                            Balance = balance
                        });
                    }
                }
            }

            return list;
        }

        static decimal ParseMoney(string s)
        {
            s = s.Replace(",", "");
            decimal.TryParse(s, out decimal result);
            return result;
        }
    }
}