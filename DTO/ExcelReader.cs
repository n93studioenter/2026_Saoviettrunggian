using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SaovietTax.DTO
{

    public class ChiTietHoaDon
    {
        public string NoiDung { get; set; }
        public string DVT { get; set; }
        public string Ma { get; set; }
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class HoaDon
    {
        public string MaPhieuXuat { get; set; } // Thêm dòng này (VD: XVT162-1)
        public string DH { get; set; }
        public string Invoice { get; set; }
        public DateTime? Date { get; set; }

        public decimal GiaTriHopDong { get; set; }
        public decimal TongChiPhiVatTu => ChiTiets.Sum(x => x.ThanhTien);
        public decimal TyLe => GiaTriHopDong > 0 ? (TongChiPhiVatTu / GiaTriHopDong) : 0;

        public List<ChiTietHoaDon> ChiTiets { get; set; } = new List<ChiTietHoaDon>();
    }

    public static class ExcelHelper
    {
        public static List<HoaDon> Docbangkera(string filePath)
        {
            var result = new List<HoaDon>();

            string currentMaDH = null;
            string currentInvoice = null;
            decimal currentGiaTriHD = 0;
            string currentDateStr = null;
            HoaDon currentHD = null;

            using (var wb = new XLWorkbook(filePath))
            {
                IXLWorksheet ws;
                if (!wb.Worksheets.TryGetWorksheet("08.26", out ws))
                    ws = wb.Worksheet(2);

                var lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;

                // BƯỚC 1: Đọc toàn bộ dữ liệu thô
                for (int r = 2; r <= lastRow; r++)
                {
                    var row = ws.Row(r);

                    string dh = GetString(row.Cell(1));
                    string invoice = GetString(row.Cell(2));
                    string dateStr = GetString(row.Cell(3));
                    string noiDung = GetString(row.Cell(4));
                    string dvt = GetString(row.Cell(5));
                    string ma = GetString(row.Cell(6));

                    decimal sl = GetDecimal(row.Cell(7));
                    decimal donGia = GetDecimal(row.Cell(8));
                    decimal thanhTien = GetDecimal(row.Cell(9));

                    if (!string.IsNullOrWhiteSpace(dh) || !string.IsNullOrWhiteSpace(invoice))
                    {
                        currentMaDH = dh;
                        currentInvoice = invoice;
                        currentGiaTriHD = Math.Round(thanhTien, 0);
                        currentDateStr = null;
                        currentHD = null;
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(noiDung) && noiDung.Trim().Equals("Chi phí sản xuất chung", StringComparison.OrdinalIgnoreCase))
                    {
                        currentMaDH = null;
                        currentHD = null;
                        continue;
                    }

                    if (currentMaDH != null && !string.IsNullOrWhiteSpace(noiDung))
                    {
                        if (string.IsNullOrWhiteSpace(dateStr) && currentDateStr != null)
                        {
                            dateStr = currentDateStr;
                        }
                        else
                        {
                            currentDateStr = dateStr;
                        }

                        // Nếu chưa có Hóa đơn, hoặc Ngày bị thay đổi -> Tạo Hóa đơn mới
                        if (currentHD == null || dateStr != currentHD.Date?.ToString("dd/MM/yyyy"))
                        {
                            currentHD = new HoaDon
                            {
                                DH = currentMaDH,
                                Invoice = currentInvoice,
                                Date = ParseDate(dateStr),
                                GiaTriHopDong = currentGiaTriHD
                            };
                            result.Add(currentHD);
                        }

                        currentHD.ChiTiets.Add(new ChiTietHoaDon
                        {
                            NoiDung = noiDung,
                            DVT = dvt,
                            Ma = ma,
                            SoLuong = sl,
                            DonGia = Math.Round(donGia, 4),
                            ThanhTien = Math.Round(thanhTien, 0)
                        });
                    }
                }
            }

            // BƯỚC 2: Đặt mã Phiếu xuất thông minh
            // Nhóm các hóa đơn theo Invoice (Số hóa đơn)
            var groups = result.GroupBy(x => x.Invoice);
            foreach (var group in groups)
            {
                var listHD = group.ToList();
                if (listHD.Count == 1)
                {
                    // Nếu chỉ có 1 ngày duy nhất -> Không có đuôi -1
                    listHD[0].MaPhieuXuat = $"XVT{listHD[0].Invoice}";
                }
                else
                {
                    // Nếu có nhiều ngày -> Đánh số -1, -2
                    for (int i = 0; i < listHD.Count; i++)
                    {
                        listHD[i].MaPhieuXuat = $"XVT{listHD[i].Invoice}-{i + 1}";
                    }
                }
            }

            return result;
        }

        // ================= HÀM HỖ TRỢ ĐỌC Ô EXCEL =================

        private static string GetString(IXLCell cell)
        {
            try
            {
                if (cell == null || cell.IsEmpty()) return null;
                return cell.GetFormattedString()?.Trim();
            }
            catch { return null; }
        }

        private static decimal GetDecimal(IXLCell cell)
        {
            try
            {
                if (cell == null || cell.IsEmpty()) return 0;
                string formatted = cell.GetFormattedString();
                if (string.IsNullOrWhiteSpace(formatted)) return 0;

                formatted = formatted.Replace(",", "");
                if (decimal.TryParse(formatted, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    return d;
                return 0;
            }
            catch { return 0; }
        }

        private static DateTime? ParseDate(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (DateTime.TryParseExact(s, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d;
            if (DateTime.TryParse(s, out d)) return d;
            return null;
        }
    }
}