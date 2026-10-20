using DevExpress.XtraEditors;
using iText.Kernel.Pdf;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SaovietTax
{
    public partial class OcrPdf : DevExpress.XtraEditors.XtraForm
    {
        // Giữ path file đã mở khóa (nếu có) để xóa sau
        private string _tempUnlockedPdf = null;

        public OcrPdf()
        {
            InitializeComponent();
        }

        private void OcrPdf_Load(object sender, EventArgs e)
        {
            comboBoxEdit1.Properties.Items.Clear();
            comboBoxEdit1.Properties.Items.AddRange(new object[] {
                "MB Bank", "ACB", "Agribank", "HDBank", "VietinBank",
                "Shinhan Bank", "Sacombank", "BIDV", "Techcombank"
            });
            comboBoxEdit1.SelectedIndex = 0;
        }

        private void btnChonfile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn file sao kê PDF";
                ofd.Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*";
                ofd.Multiselect = false;
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                string selectedPdfPath = ofd.FileName;

                // 1. Kiểm tra password → trả về path dùng được (gốc hoặc đã unlock)
                string workingPdfPath = PreparePdfForRead(selectedPdfPath);
                if (string.IsNullOrEmpty(workingPdfPath))
                    return; // Hủy / sai pass

                try
                {
                    string readerDir = @"C:\apps\SharedDLLs\SaoKeReader";
                    if (!Directory.Exists(readerDir))
                        Directory.CreateDirectory(readerDir);

                    string requestFilePath = Path.Combine(readerDir, "request.txt");
                    var utf8NoBom = new UTF8Encoding(false);
                    // Ghi path ĐÃ MỞ KHÓA
                    File.WriteAllText(requestFilePath, workingPdfPath.Trim(), utf8NoBom);

                    string selectedBank = comboBoxEdit1.EditValue?.ToString();
                    if (string.IsNullOrEmpty(selectedBank))
                    {
                        MessageBox.Show("Vui lòng chọn ngân hàng trước khi xuất file!",
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string exeName = "";
                    switch (selectedBank)
                    {
                        case "MB Bank": exeName = "DocSaoKe_MB.exe"; break;
                        case "ACB": exeName = "DocSaoKe_ACB.exe"; break;
                        case "Agribank": exeName = "DocSaoKe_Agribank.exe"; break;
                        case "HDBank": exeName = "DocSaoKe_HDBank.exe"; break;
                        case "VietinBank": exeName = "DocSaoKe_VietinBank.exe"; break;
                        case "Shinhan Bank": exeName = "DocSaoKe_ShinhanBank.exe"; break;
                        case "Sacombank": exeName = "DocSaoKe_Sacombank.exe"; break;
                        case "BIDV": exeName = "DocSaoKe_BIDV.exe"; break;
                        case "Techcombank": exeName = "DocSaoKe_Techcombank.exe"; break;
                        default:
                            MessageBox.Show("Ngân hàng chưa được hỗ trợ!", "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                    }

                    string exePath = Path.Combine(readerDir, exeName);
                    if (!File.Exists(exePath))
                    {
                        MessageBox.Show($"Không tìm thấy file tool:\n{exePath}", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    progressPanel1.Caption = "Đang tiến hành convert vui lòng chờ...";

                    // Kill process cũ
                    try
                    {
                        string processName = Path.GetFileNameWithoutExtension(exeName);
                        foreach (Process p in Process.GetProcessesByName(processName))
                        {
                            p.Kill();
                            p.WaitForExit(3000);
                        }
                    }
                    catch { }

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = exePath,
                        WorkingDirectory = readerDir,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };

                    using (Process process = Process.Start(psi))
                    {
                        process.WaitForExit();
                    }

                    // Lấy Excel mới nhất
                    DirectoryInfo dirInfo = new DirectoryInfo(readerDir);
                    FileInfo latestExcel = dirInfo.GetFiles("*.xlsx")
                        .OrderByDescending(f => f.LastWriteTime)
                        .FirstOrDefault();

                    if (latestExcel != null)
                    {
                        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        string destFilePath = Path.Combine(desktopPath, "saokenganhang.xlsx");
                        if (File.Exists(destFilePath))
                            File.Delete(destFilePath);
                        File.Move(latestExcel.FullName, destFilePath);

                        MessageBox.Show("Đã xuất file Excel thành công!\nFile lưu tại: " + destFilePath,
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Tool đã chạy xong nhưng không tìm thấy file Excel trong SaoKeReader!",
                            "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Có lỗi xảy ra:\n" + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // Xóa file tạm đã mở khóa
                    CleanupTempPdf();
                }
            }
        }

        /// <summary>
        /// Kiểm tra PDF có password không.
        /// - Không khóa → trả về path gốc
        /// - Có khóa → hỏi password, mở khóa ra file tạm, trả về path tạm
        /// - Hủy / sai 3 lần → trả về null
        /// </summary>
        private string PreparePdfForRead(string pdfPath)
        {
            // 1) Thử mở không password
            try
            {
                using (var reader = new PdfReader(pdfPath))
                {
                    reader.SetUnethicalReading(true);
                    using (var doc = new PdfDocument(reader))
                    {
                        // Mở được, không cần password
                        return pdfPath;
                    }
                }
            }
            catch (iText.Kernel.Exceptions.BadPasswordException)
            {
                // Cần password → hỏi bên dưới
            }
            catch (Exception)
            {
                // Lỗi khác: vẫn trả path gốc, để tool Python báo
                return pdfPath;
            }

            // 2) Hỏi password tối đa 3 lần
            for (int attempts = 0; attempts < 3; attempts++)
            {
                string password = XtraInputBox.Show(
                    "File PDF sao kê đang được bảo vệ bằng mật khẩu.\nVui lòng nhập mật khẩu:",
                    "Yêu cầu mật khẩu PDF",
                    "");

                if (string.IsNullOrEmpty(password))
                    return null; // Cancel

                try
                {
                    string unlocked = UnlockPdfToTemp(pdfPath, password);
                    if (!string.IsNullOrEmpty(unlocked))
                    {
                        _tempUnlockedPdf = unlocked;
                        return unlocked;
                    }
                }
                catch (iText.Kernel.Exceptions.BadPasswordException)
                {
                    XtraMessageBox.Show(
                        $"Mật khẩu không đúng. Còn {2 - attempts} lần thử.",
                        "Sai mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("Lỗi mở PDF: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }

            XtraMessageBox.Show("Nhập sai mật khẩu quá 3 lần.", "Dừng",
                MessageBoxButtons.OK, MessageBoxIcon.Stop);
            return null;
        }

        /// <summary>
        /// Mở PDF có password và ghi ra file tạm KHÔNG mật khẩu.
        /// </summary>
        private string UnlockPdfToTemp(string encryptedPath, string password)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "SaoKeUnlock");
            Directory.CreateDirectory(tempDir);

            string tempFile = Path.Combine(tempDir,
                Path.GetFileNameWithoutExtension(encryptedPath) + "_unlocked.pdf");

            // Xóa file cũ cùng tên nếu có
            if (File.Exists(tempFile))
                File.Delete(tempFile);

            var props = new ReaderProperties()
                .SetPassword(Encoding.UTF8.GetBytes(password));

            using (var reader = new PdfReader(encryptedPath, props))
            {
                reader.SetUnethicalReading(true);
                // Writer không set encryption → file output không còn password
                using (var writer = new PdfWriter(tempFile))
                using (var pdfDoc = new PdfDocument(reader, writer))
                {
                    // Copy toàn bộ trang sang file mới (không mã hóa)
                }
            }

            if (!File.Exists(tempFile) || new FileInfo(tempFile).Length < 100)
                throw new Exception("Không tạo được file PDF đã mở khóa.");

            return tempFile;
        }

        private void CleanupTempPdf()
        {
            try
            {
                if (!string.IsNullOrEmpty(_tempUnlockedPdf) && File.Exists(_tempUnlockedPdf))
                    File.Delete(_tempUnlockedPdf);
            }
            catch { }
            _tempUnlockedPdf = null;
        }
    }
}