using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaovietTax
{
    public partial class frmKiemtraUpdate : DevExpress.XtraEditors.XtraForm
    {
        public frmKiemtraUpdate()
        {
            InitializeComponent();
            this.Visible= false;
            this.ShowInTaskbar = false;
        }
        private void UpdateGogoleDrive()
        {
            string exeDir = Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location);
             
            string fileId = "1qj3mjQkA1o6QEsellIQ09_vrzq1ETCOL";
            string fileName = "VietstarDriver.zip";
            string saveFolder = exeDir;
            string zipPath = Path.Combine(saveFolder, fileName);
            string exePath = Path.Combine(saveFolder, "VietstarDriver.exe");
            string downloadUrl = $"https://drive.google.com/uc?export=download&id={fileId}";

            try
            {
                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(saveFolder))
                    Directory.CreateDirectory(saveFolder);

                // Xóa file ZIP cũ nếu có
                if (File.Exists(zipPath))
                    File.Delete(zipPath);

                // ========== BƯỚC 1: TẢI FILE ==========
                using (var client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                    client.DownloadFile(downloadUrl, zipPath);

                    // Kiểm tra file có bị lỗi không
                    FileInfo fileInfo = new FileInfo(zipPath);
                    if (fileInfo.Length < 1024)
                    {
                        string content = File.ReadAllText(zipPath);
                        if (content.Contains("Quota exceeded") || content.Contains("virus scan") || content.Contains("Google Drive"))
                        {
                            File.Delete(zipPath);
                            DownloadWithConfirm(fileId, zipPath);
                            return;
                        }
                    }
                }

                // ========== BƯỚC 2: GIẢI NÉN ==========
                //MessageBox.Show($"✅ Tải thành công!\nĐang giải nén...", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ExtractAllFiles(zipPath, saveFolder);

                // ========== BƯỚC 3: CHẠY FILE EXE BẰNG CMD VÀ ĐÓNG ỨNG DỤNG ==========
                if (File.Exists(exePath))
                {
                    // Chạy file EXE bằng CMD (không bị treo)
                    ProcessStartInfo psi = new ProcessStartInfo()
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c start \"\" \"{exePath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                    Process.Start(psi);

                    // Thông báo và đóng ứng dụng hiện tại
                    //MessageBox.Show(
                    //    $"✅ Cập nhật thành công!\n" +
                    //    $"Đang mở VietstarDriver.exe...\n" +
                    //    "Ứng dụng sẽ tự đóng.",
                    //    "Thành công",
                    //    MessageBoxButtons.OK,
                    //    MessageBoxIcon.Information
                    //);

                    // Đóng ứng dụng hiện tại
                    Application.Exit();
                }
                else
                {
                    MessageBox.Show($"❌ Không tìm thấy VietstarDriver.exe sau khi giải nén!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ========== GIẢI NÉN TẤT CẢ FILE (KHÔNG TẠO THƯ MỤC CON) ==========
        private void ExtractAllFiles(string zipPath, string extractFolder)
        {
            try
            {
                if (!File.Exists(zipPath))
                {
                    MessageBox.Show("❌ Không tìm thấy file ZIP!", "Lỗi");
                    return;
                }

                int extractedCount = 0;

                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (string.IsNullOrEmpty(entry.Name))
                            continue;

                        string destinationPath = Path.Combine(extractFolder, entry.Name);

                        if (File.Exists(destinationPath))
                            File.Delete(destinationPath);

                        entry.ExtractToFile(destinationPath, overwrite: true);
                        extractedCount++;
                    }
                }

                // Xóa file ZIP sau khi giải nén
                if (File.Exists(zipPath))
                    File.Delete(zipPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi giải nén:\n{ex.Message}", "Lỗi");
            }
        }

        // ========== TẢI KHI BỊ CHẶN ==========
        private void DownloadWithConfirm(string fileId, string savePath)
        {
            try
            {
                string downloadUrl = $"https://drive.google.com/uc?export=download&id={fileId}";

                using (var client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                    string html = client.DownloadString(downloadUrl);

                    Match match = Regex.Match(html, @"<a[^>]+href=""([^""]+download[^""]+)""", RegexOptions.IgnoreCase);
                    if (!match.Success)
                    {
                        match = Regex.Match(html, @"/uc\?export=download[^""]*&confirm=[^""]*");
                    }

                    if (match.Success)
                    {
                        string confirmUrl = match.Groups[1].Value;
                        if (!confirmUrl.StartsWith("http"))
                        {
                            confirmUrl = "https://drive.google.com" + confirmUrl;
                        }

                        client.DownloadFile(confirmUrl, savePath);

                        MessageBox.Show($"✅ Tải thành công!\nĐang giải nén...", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ExtractAllFiles(savePath, Path.GetDirectoryName(savePath));

                        // Chạy file EXE sau khi giải nén
                        string exePath = Path.Combine(Path.GetDirectoryName(savePath), "VietstarDriver.exe");
                        if (File.Exists(exePath))
                        {
                            ProcessStartInfo psi = new ProcessStartInfo()
                            {
                                FileName = "cmd.exe",
                                Arguments = $"/c start \"\" \"{exePath}\"",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                WindowStyle = ProcessWindowStyle.Hidden
                            };
                            Process.Start(psi);

                            MessageBox.Show($"✅ Cập nhật thành công!\nĐang mở VietstarDriver.exe...", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Application.Exit();
                        }
                    }
                    else
                    {
                        MessageBox.Show("❌ Không tìm thấy link tải. Vui lòng thử lại sau.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void frmKiemtraUpdate_Load(object sender, EventArgs e)
        {
            UpdateGogoleDrive();
        }
    }
}