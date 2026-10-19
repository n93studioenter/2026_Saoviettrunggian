using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaovietTax
{
    public partial class OcrPdf : DevExpress.XtraEditors.XtraForm
    {
        public OcrPdf()
        {
            InitializeComponent();
        }

        private void OcrPdf_Load(object sender, EventArgs e)
        {
            // Xóa dữ liệu cũ (nếu có)
            comboBoxEdit1.Properties.Items.Clear();

            // Thêm danh sách ngân hàng
            comboBoxEdit1.Properties.Items.AddRange(new object[] {
        "MB Bank",
        "ACB",
        "Agribank",
        "HDBank",
        "VietinBank",
        "Shinhan Bank",
        "Sacombank"
    });

            // Chọn mặc định ngân hàng đầu tiên
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

                try
                {
                    string readerDir = @"C:\apps\SharedDLLs\SaoKeReader";
                    if (!Directory.Exists(readerDir))
                        Directory.CreateDirectory(readerDir);

                    string requestFilePath = Path.Combine(readerDir, "request.txt");

                    // 1. Ghi đường dẫn PDF vào file request.txt (UTF-8 không BOM)
                    var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                    File.WriteAllText(requestFilePath, selectedPdfPath.Trim(), utf8NoBom);

                    // 2. Lấy ngân hàng đang chọn
                    string selectedBank = comboBoxEdit1.EditValue?.ToString();
                    if (string.IsNullOrEmpty(selectedBank))
                    {
                        MessageBox.Show("Vui lòng chọn ngân hàng trước khi xuất file!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 3. Ánh xạ tên ngân hàng sang tên file exe
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
                        default:
                            MessageBox.Show("Ngân hàng chưa được hỗ trợ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                    }

                    // 4. Gọi file exe chạy ngầm (ẩn cmd)
                    string exePath = Path.Combine(readerDir, exeName);
                    if (!File.Exists(exePath))
                    {
                        MessageBox.Show($"Không tìm thấy file tool:\n{exePath}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    progressPanel1.Caption="Đang tiến hành convert vui lòng chờ...";
                    // =========================================================
                    // [MỚI] DỌN DẸP CÁC TIẾN TRÌNH DocSaoKe ĐANG CHẠY NGẦM (NẾU CÓ)
                    // =========================================================
                    try
                    {
                        // Lấy tên file không có đuôi .exe (VD: "DocSaoKe_ACB")
                        string processName = Path.GetFileNameWithoutExtension(exeName);
                        Process[] runningProcesses = Process.GetProcessesByName(processName);
                        foreach (Process p in runningProcesses)
                        {
                            p.Kill();
                            p.WaitForExit(); // Đợi tiến trình cũ tắt hẳn
                        }
                    }
                    catch
                    {
                        // Bỏ qua lỗi nếu không thể kill (VD: do phân quyền)
                    }
                    // =========================================================

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = exePath,
                        WorkingDirectory = readerDir, // Quan trọng: để exe đọc request.txt cùng thư mục
                        CreateNoWindow = true,        // Ẩn cửa sổ cmd
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };

                    Process process = Process.Start(psi);
                    process.WaitForExit(); // Đợi tool Python chạy xong

                    // 5. XỬ LÝ FILE EXCEL SAU KHI PYTHON XUẤT XONG
                    DirectoryInfo dirInfo = new DirectoryInfo(readerDir);
                    FileInfo latestExcel = dirInfo.GetFiles("*.xlsx")
                                                  .OrderByDescending(f => f.LastWriteTime)
                                                  .FirstOrDefault();

                    if (latestExcel != null)
                    {
                        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        string destFilePath = Path.Combine(desktopPath, "saokenganhang.xlsx");

                        if (File.Exists(destFilePath))
                        {
                            File.Delete(destFilePath);
                        }

                        // Dời file sang Desktop
                        File.Move(latestExcel.FullName, destFilePath);

                        MessageBox.Show("Đã xuất file Excel thành công!\nFile lưu tại: " + destFilePath,
                                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Tool đã chạy xong nhưng không tìm thấy file Excel nào được tạo ra trong thư mục SaoKeReader!\n(Vui lòng kiểm tra lại code Python xem có xuất file đúng thư mục không)",
                                        "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Có lỗi xảy ra:\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}