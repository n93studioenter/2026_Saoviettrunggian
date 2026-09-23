using DevExpress.XtraEditors;
using DocumentFormat.OpenXml.Vml;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;
using Path = System.IO.Path;

namespace SaovietTax
{
    public partial class Updater : DevExpress.XtraEditors.XtraForm
    {
        public Updater()
        {
            InitializeComponent();
            this.ShowInTaskbar = true;
        }
        private static void CopyDirectory(string sourceDir, string destDir)
        {
            // Tạo thư mục đích nếu chưa tồn tại
            Directory.CreateDirectory(destDir);

            // Copy tất cả file trong thư mục nguồn
            foreach (string filePath in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(filePath);
                string destFilePath = Path.Combine(destDir, fileName);
                File.Copy(filePath, destFilePath, true);
                Console.WriteLine($"Đã copy file: {fileName}");
            }

            // Copy tất cả thư mục con (đệ quy)
            foreach (string subDirPath in Directory.GetDirectories(sourceDir))
            {
                string subDirName = Path.GetFileName(subDirPath);
                string destSubDirPath = Path.Combine(destDir, subDirName);
                CopyDirectory(subDirPath, destSubDirPath);
                Console.WriteLine($"Đã copy thư mục con: {subDirName}");
            }
        }
        private async void Updater_Load(object sender, EventArgs e)
        {
            await Task.Delay(300);
            // Set tooltip khi hover
            string appPaths = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPaths);

            // Xóa phần \bin\Debug để lấy đường dẫn gốc
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
            string filePaths = Path.Combine(rootDirectory, "hoadon", "serverpath.txt");
            string sourcePath = File.ReadAllText(filePaths);
            string destPath = Path.Combine(rootDirectory, "Tools", "Debug", "AutoUpdate");
            sourcePath = Path.Combine(sourcePath, "Tools", "Debug", "AutoUpdate");
            try
            {
                // Kiểm tra thư mục nguồn có tồn tại không
                if (!Directory.Exists(sourcePath))
                {
                    XtraMessageBox.Show($"Thư mục nguồn không tồn tại: {sourcePath}");
                    return;
                }

                // Kiểm tra thư mục đích đã tồn tại chưa
                if (Directory.Exists(destPath))
                {
                    Console.WriteLine($"Thư mục đích đã tồn tại: {destPath}");
                    Console.WriteLine("Bỏ qua copy (thư mục đã có sẵn)"); 
                }

                // Tạo thư mục đích nếu chưa tồn tại
                Directory.CreateDirectory(destPath);
                progressPanel1.Caption = "Đang tiến hành tải updater...";
                Application.DoEvents();
                // Copy toàn bộ thư mục
                CopyDirectory(sourcePath, destPath);

                //XtraMessageBox.Show($"Copy thành công từ: {sourcePath}");
                Console.WriteLine($"Đến: {destPath}");
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Lỗi khi copy: {ex.Message}");
                throw;
            }

            string exe = Path.Combine(destPath, "AutoUpdate.exe");

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c start \"\" \"{exe}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                Environment.Exit(0);
            }
            catch( Exception ex )
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exe,
                    UseShellExecute = true,  // 👈 QUAN TRỌNG: phải là true
                    WorkingDirectory = Path.GetDirectoryName(exe)
                });

                Environment.Exit(0);
            }
           
            // Hiển thị trạng thái loading lên giao diện  

            try
            {
                // Chạy quá trình cập nhật trong background
                string duongDanVietStar =  ThucHienCapNhat();

                // Kiểm tra kết quả
                if (!string.IsNullOrEmpty(duongDanVietStar))
                {
                    // Thông báo thành công
                    //XtraMessageBox.Show(
                    //    "Cập nhật thành công!",
                    //    "Thông báo",
                    //    MessageBoxButtons.OK,
                    //    MessageBoxIcon.Information
                    //);

                    // Chạy ứng dụng VietStar mới
                    ProcessStartInfo thongTinMo = new ProcessStartInfo
                    {
                        FileName = duongDanVietStar,
                        UseShellExecute = true
                    };
                    Process.Start(thongTinMo);
                }
                else
                {
                    XtraMessageBox.Show(
                        "Không tìm thấy file cập nhật!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
            catch (Exception loi)
            {
                XtraMessageBox.Show(
                    $"Lỗi cập nhật: {loi.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // Đóng form updater
                this.Close();
            }
        }

        private string ThucHienCapNhat()
        {
            // ================================================================
            // BƯỚC 1: Lấy version từ file trên server
            // ================================================================
            string version = "";
            string duongDanVersionServer = @"\\192.168.1.90\Ke toan 2025 New\1 Copi vao dung 1\Tools\version.txt";

            if (File.Exists(duongDanVersionServer))
            {
                version = File.ReadAllText(duongDanVersionServer).Trim();
                //progressPanel1.Caption = $"Đang tải phiên bản {version}, vui lòng chờ";
                System.Diagnostics.Debug.WriteLine("Version: " + version);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Không tìm thấy file version.txt trên server");
            }

            // ================================================================
            // BƯỚC 2: Lấy đường dẫn thư mục hiện tại của ứng dụng
            // ================================================================
            string duongDanThuMucExe = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string duongDanThuMucProject = Directory.GetParent(duongDanThuMucExe).Parent.FullName;

            // ================================================================
            // BƯỚC 3: Ghi version vào thư mục Hoadon
            // ================================================================
            string duongDanThuMucHoaDon = Path.Combine(duongDanThuMucProject, "Hoadon");
            string duongDanVersionHoaDon = Path.Combine(duongDanThuMucHoaDon, "version.txt");

            if (File.Exists(duongDanVersionHoaDon))
            {
                File.WriteAllText(duongDanVersionHoaDon, version);
                System.Diagnostics.Debug.WriteLine("Đã ghi version vào thư mục Hoadon");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Không tìm thấy file version.txt trong thư mục Hoadon");
            }

            // ================================================================
            // BƯỚC 4: Copy các file cần thiết từ server về máy
            // ================================================================
            string duongDanRoot = @"\\192.168.1.90\Ke toan 2025 New\1 Copi vao dung 1";
            string duongDanThuMucDich = duongDanThuMucExe;

            string[] danhSachFileCanCopy =
            {
                "SaovietTax.exe.Config",
                "SaovietTax.application",
                "SaovietTax.exe.manifest",
                "SaovietTax.exe",
                "SaovietTax.pdb"
            };

            try
            {
                // Tìm tất cả thư mục Debug nằm trong thư mục Tools
                var danhSachThuMucDebug = Directory.GetDirectories(
                    duongDanRoot,
                    "Debug",
                    SearchOption.AllDirectories
                ).Where(duongDan => duongDan.EndsWith(
                    Path.Combine("Tools", "Debug"),
                    StringComparison.OrdinalIgnoreCase
                ));

                // Tạo thư mục đích nếu chưa tồn tại
                Directory.CreateDirectory(duongDanThuMucDich);

                // Duyệt qua từng thư mục Debug tìm được
                foreach (var duongDanThuMucDebug in danhSachThuMucDebug)
                {
                    System.Diagnostics.Debug.WriteLine("Tìm thấy thư mục Debug: " + duongDanThuMucDebug);

                    // Copy từng file
                    foreach (var tenFile in danhSachFileCanCopy)
                    {
                        string duongDanFileNguon = Path.Combine(duongDanThuMucDebug, tenFile);

                        if (File.Exists(duongDanFileNguon))
                        {
                            string duongDanFileDich = Path.Combine(duongDanThuMucDich, tenFile);
                            File.Copy(duongDanFileNguon, duongDanFileDich, true);
                            XtraMessageBox.Show("Đã copy: " + tenFile);
                        }
                        else
                        {
                            XtraMessageBox.Show("Không tìm thấy file: " + tenFile);
                        }
                    }

                    // Chỉ copy ở thư mục Debug đầu tiên tìm được
                    break;
                }

                XtraMessageBox.Show("Hoàn thành copy các file chính.");
            }
            catch (Exception loi)
            {
                XtraMessageBox.Show("Lỗi khi copy file: " + loi.Message);
            }

            // ================================================================
            // BƯỚC 5: Copy file VietStar.exe mới nhất
            // ================================================================
            string duongDanVietStarDaCopy = "";

            try
            {
                // Tìm file VietStar.exe mới nhất
                var fileVietStarMoiNhat = new DirectoryInfo(duongDanRoot)
                    .GetFiles("*.exe", SearchOption.AllDirectories)
                    .Where(file => file.Name.IndexOf("VietStar", StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderByDescending(file => file.LastWriteTime)
                    .FirstOrDefault();

                if (fileVietStarMoiNhat != null)
                {
                    // Copy file VietStar vào thư mục project
                    string duongDanFileDich = Path.Combine(duongDanThuMucProject, fileVietStarMoiNhat.Name);
                    File.Copy(fileVietStarMoiNhat.FullName, duongDanFileDich, true);
                    duongDanVietStarDaCopy = duongDanFileDich;

                    XtraMessageBox.Show("Đã copy VietStar: " + duongDanFileDich);
                }
                else
                {
                    XtraMessageBox.Show("Không tìm thấy file VietStar.exe");
                }
            }
            catch (Exception loi)
            {
                XtraMessageBox.Show("Lỗi khi copy VietStar: " + loi.Message);
            }

            // Trả về đường dẫn của file VietStar đã copy
            return duongDanVietStarDaCopy;
        }
    }
}