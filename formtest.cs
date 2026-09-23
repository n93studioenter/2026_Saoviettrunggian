using DevExpress.XtraEditors;
using SaovietTax.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaovietTax
{
    public partial class formtest : DevExpress.XtraEditors.XtraForm
    {
        private ViettelInvoiceService _service;
        private List<Invoices> _invoices = new List<Invoices>();
        private string _xmlSavePath = @"C:\InvoiceXML";

        public formtest()
        {
            InitializeComponent();
        }

        private async void formtest_Load(object sender, EventArgs e)
        {
            try
            {
                // Khởi tạo service
                _service = new ViettelInvoiceService(
                    username: "3502550210",
                    password: "Van123@@",
                    supplierTaxCode: "3502550210"
                );

                var startDate = new DateTime(2026, 7, 1);
                var endDate = new DateTime(2026, 7, 29);

                if (!Directory.Exists(_xmlSavePath))
                    Directory.CreateDirectory(_xmlSavePath);

                // ============ LẤY HÓA ĐƠN ĐẦU VÀO ============
                Log("📥 Đang lấy danh sách hóa đơn đầu vào (từ nhà cung cấp)...");

                // Cách 1: Lấy từ tất cả nhà cung cấp
                // _invoices = await _service.GetInputInvoicesAsync(startDate, endDate);

                // Cách 2: Lấy từ một nhà cung cấp cụ thể
                string supplierTaxCode = "MST_NHA_CUNG_CAP"; // Thay bằng MST nhà cung cấp
                _invoices = await _service.GetInputInvoicesAsync(startDate, endDate, supplierTaxCode);

                Log($"✅ Tổng số hóa đơn đầu vào: {_invoices.Count}");

                // Hiển thị lên Grid
                DisplayInvoices(_invoices);

                // ============ LẤY CHI TIẾT + TẢI XML CHO 10 HÓA ĐƠN ĐẦU ============
                var topInvoices = _invoices.Take(10).ToList();
                var invoiceNos = topInvoices.Select(x => x.InvoiceNo).ToList();

                Log("\n📄 Đang lấy chi tiết và tải XML cho 10 hóa đơn đầu...");

                var details = await _service.GetInvoiceDetailsWithXmlAsync(
                    invoiceNos,
                    _xmlSavePath,
                    (invoiceNo, current, total) =>
                    {
                        Log($"⏳ Đang xử lý {current}/{total}: {invoiceNo}");
                    }
                );

                // ============ HIỂN THỊ KẾT QUẢ ============
                Log($"\n✅ Đã xử lý {details.Count}/{invoiceNos.Count} hóa đơn");

                foreach (var detail in details)
                {
                    Log($"\n📋 Hóa đơn: {detail.InvoiceNo}");
                    Log($"   - Người bán: {detail.SupplierTaxCode}");
                    Log($"   - Người mua: {detail.BuyerName}");
                    Log($"   - Tổng tiền: {detail.Total:N0} VND");
                    Log($"   - VAT: {detail.TaxAmount:N0} VND");

                    if (!string.IsNullOrEmpty(detail.XmlPath))
                        Log($"   - ✅ XML: {detail.XmlPath}");
                    else if (!string.IsNullOrEmpty(detail.XmlError))
                        Log($"   - ❌ Lỗi XML: {detail.XmlError}");

                    if (detail.Items != null && detail.Items.Any())
                    {
                        Log($"   - Số dòng hàng: {detail.Items.Count}");
                        foreach (var item in detail.Items.Take(3))
                        {
                            Log($"      * {item.ItemName} x {item.Quantity} = {item.Amount:N0}đ");
                        }
                        if (detail.Items.Count > 3)
                            Log($"      ... và {detail.Items.Count - 3} dòng khác");
                    }
                }

                // ============ THỐNG KÊ ============
                Log("\n📊 THỐNG KÊ:");
                Log($"   - Tổng số hóa đơn đầu vào: {_invoices.Count}");
                Log($"   - Tổng giá trị: {_invoices.Sum(x => x.Total):N0} VND");
                Log($"   - Tổng VAT: {_invoices.Sum(x => x.TaxAmount):N0} VND");

                Log("\n✅ Hoàn thành!");
            }
            catch (Exception ex)
            {
                Log($"❌ LỖI: {ex.Message}");
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayInvoices(List<Invoices> invoices)
        {
            if (gridControl1 != null)
            {
                gridControl1.DataSource = invoices;
            }
        }

        private void Log(string message)
        {
            if (memoEdit1 != null && !memoEdit1.IsDisposed)
            {
                memoEdit1.AppendText(message + Environment.NewLine);
                Application.DoEvents();
            }
            Console.WriteLine(message);
        }

        // ============ NÚT TẢI XML CHO TẤT CẢ HÓA ĐƠN ============

        private async void btnDownloadAllXml_Click(object sender, EventArgs e)
        {
            if (_invoices == null || !_invoices.Any())
            {
                MessageBox.Show("Chưa có danh sách hóa đơn!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Bạn có chắc muốn tải XML cho tất cả {_invoices.Count} hóa đơn?\nThời gian có thể mất vài phút.",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
                return;

            try
            {
                btnDownloadAllXml.Enabled = false;
                Log("\n📥 Đang tải XML cho tất cả hóa đơn...");

                var processed = 0;
                var total = _invoices.Count;
                var successCount = 0;
                var failCount = 0;

                foreach (var inv in _invoices)
                {
                    processed++;
                    Log($"⏳ Đang tải {processed}/{total}: {inv.InvoiceNo}");

                    try
                    {
                        var xmlPath = await _service.DownloadInvoiceXmlAsync(
                            inv.InvoiceSeri,
                            inv.InvoiceNumber,
                            inv.TemplateCode,
                            inv.TransactionUuid,
                            _xmlSavePath
                        );
                        Log($"   ✅ Đã lưu: {xmlPath}");
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        Log($"   ❌ Lỗi: {ex.Message}");
                        failCount++;
                    }
                }

                Log($"\n✅ Hoàn thành! Thành công: {successCount}, Thất bại: {failCount}");
                MessageBox.Show($"Đã tải XML cho {successCount}/{total} hóa đơn!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log($"❌ LỖI: {ex.Message}");
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnDownloadAllXml.Enabled = true;
            }
        }

        // ============ NÚT LẤY CHI TIẾT 1 HÓA ĐƠN ============

        private async void btnGetDetail_Click(object sender, EventArgs e)
        {
            string invoiceNo = txtInvoiceNo.Text.Trim();

            if (string.IsNullOrEmpty(invoiceNo))
            {
                MessageBox.Show("Vui lòng nhập số hóa đơn!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnGetDetail.Enabled = false;
                Log($"\n📄 Đang lấy chi tiết hóa đơn: {invoiceNo}");

                var detail = await _service.GetInvoiceDetailAsync(invoiceNo);

                if (detail != null)
                {
                    Log($"✅ Đã lấy chi tiết hóa đơn {invoiceNo}");
                    Log($"   - Người bán: {detail.SupplierTaxCode}");
                    Log($"   - Người mua: {detail.BuyerName}");
                    Log($"   - Tổng tiền: {detail.Total:N0} VND");
                    Log($"   - VAT: {detail.TaxAmount:N0} VND");
                    Log($"   - TemplateCode: {detail.TemplateCode}");
                    Log($"   - InvoiceSeri: {detail.InvoiceSeri}");
                    Log($"   - InvoiceNumber: {detail.InvoiceNumber}");
                    Log($"   - TransactionUuid: {detail.TransactionUuid}");

                    if (detail.Items != null && detail.Items.Any())
                    {
                        Log($"   - Số dòng hàng: {detail.Items.Count}");
                        foreach (var item in detail.Items.Take(5))
                        {
                            Log($"      * {item.ItemName} x {item.Quantity} = {item.Amount:N0}đ");
                        }
                    }

                    // Tải XML
                    try
                    {
                        var xmlPath = await _service.DownloadInvoiceXmlAsync(
                            detail.InvoiceSeri,
                            detail.InvoiceNumber,
                            detail.TemplateCode,
                            detail.TransactionUuid,
                            _xmlSavePath
                        );
                        Log($"   - ✅ XML: {xmlPath}");
                    }
                    catch (Exception ex)
                    {
                        Log($"   - ❌ Lỗi XML: {ex.Message}");
                    }

                    // Tải PDF
                    try
                    {
                        var pdfPath = await _service.DownloadInvoicePdfAsync(
                            detail.InvoiceSeri,
                            detail.InvoiceNumber,
                            detail.TemplateCode,
                            detail.TransactionUuid,
                            _xmlSavePath
                        );
                        Log($"   - ✅ PDF: {pdfPath}");
                    }
                    catch (Exception ex)
                    {
                        Log($"   - ❌ Lỗi PDF: {ex.Message}");
                    }
                }
                else
                {
                    Log($"❌ Không tìm thấy hóa đơn {invoiceNo}");
                }
            }
            catch (Exception ex)
            {
                Log($"❌ LỖI: {ex.Message}");
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGetDetail.Enabled = true;
            }
        }

        // ============ NÚT LÀM MỚI ============

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            //if (memoEdit1 != null)
            //    memoEdit1.Clear();
            //await formtest_Load(sender, e);
        }
    }
}