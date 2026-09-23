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
    public class AcbPdfReader
    {
        public static List<BankTransaction> Read(string pdfPath)
        {
            var list = new List<BankTransaction>();

            using (PdfReader reader = new PdfReader(pdfPath))
            using (PdfDocument pdfDoc = new PdfDocument(reader))
            {
                var sb = new StringBuilder();

                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                    sb.AppendLine(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i)));

                string text = sb.ToString();

                text = text.Replace('\u00A0', ' ');
                var lines = text.Split('\n')
                                .Select(x => x.Trim())
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .ToList();

                for (int i = 0; i < lines.Count; i++)
                {
                    // CHỈ match dòng đúng format ACB:
                    // dd/MM/yyyy  SốGD  amount  balance
                    var match = Regex.Match(lines[i],
                        @"^(\d{2}/\d{2}/\d{4})\s+\d+\s+(\d{1,3}(?:\.\d{3})+)\s+(\d{1,3}(?:\.\d{3})+)$");

                    if (!match.Success)
                        continue;

                    DateTime date = DateTime.ParseExact(
                        match.Groups[1].Value,
                        "dd/MM/yyyy",
                        CultureInfo.InvariantCulture);

                    decimal amount = ParseMoney(match.Groups[2].Value);
                    decimal balance = ParseMoney(match.Groups[3].Value);

                    decimal debit = 0;
                    decimal credit = 0;

                    if (list.Count > 0)
                    {
                        if (balance < list.Last().Balance)
                            debit = amount;
                        else
                            credit = amount;
                    }
                    else
                    {
                        debit = amount;
                    }

                    // Lấy description phía trên (cho đến khi gặp dòng bắt đầu bằng ngày)
                    string description = "";
                    int j = i - 1;

                    while (j >= 0 && !Regex.IsMatch(lines[j], @"^\d{2}/\d{2}/\d{4}"))
                    {
                        description = lines[j] + " " + description;
                        j--;
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

            return list;
        }

        static decimal ParseMoney(string s)
        {
            s = s.Replace(".", "");
            decimal.TryParse(s, out decimal result);
            return result;
        }
    }
}
