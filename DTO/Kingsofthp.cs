using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SaovietTax.DTO
{
    public static class Kingsofthp
    {
        /// <summary>
        /// Gửi dữ liệu ảnh captcha (dạng byte[]) sang service Python để giải mã thông qua file IPC (request.txt / result.txt)
        /// </summary>
        public static string OcrCaptchaByPython(byte[] captchaBytes)
        {
            try
            {
                // Thư mục PythonOCR nằm chung trong thư mục bin\Debug (hoặc bin\Release) của ứng dụng C#
                string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PythonOCR");
                string requestFile = Path.Combine(baseDir, "request.txt");
                string resultFile = Path.Combine(baseDir, "result.txt");
                string scriptFile = Path.Combine(baseDir, "ocr_service.py"); // 👈 Chuyển sang chạy file .py trực tiếp

                // Đảm bảo thư mục PythonOCR luôn tồn tại
                if (!Directory.Exists(baseDir))
                {
                    Directory.CreateDirectory(baseDir);
                }

                // 👉 TỰ ĐỘNG KHỞI ĐỘNG PYTHON NẾU CHƯA CHẠY HOẶC BỊ TẮT
                var processes = Process.GetProcessesByName("python");
                if (processes.Length == 0 && File.Exists(scriptFile))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "python",
                        Arguments = $"\"{scriptFile}\"",
                        WorkingDirectory = baseDir,
                        CreateNoWindow = true,          // 👈 Ẩn hoàn toàn cửa sổ CMD
                        WindowStyle = ProcessWindowStyle.Hidden, // 👈 Ẩn cửa sổ
                        UseShellExecute = false
                    };
                    Process.Start(startInfo);
                    System.Threading.Thread.Sleep(2000); // 👈 Tăng thời gian chờ lên 2 giây để Python kịp load mô hình ddddocr
                }

                // Xóa file kết quả cũ nếu có để tránh đọc nhầm kết quả của lần trước
                if (File.Exists(resultFile))
                {
                    File.Delete(resultFile);
                }

                // Chuyển mảng byte ảnh thành định dạng Base64 và ghi vào file request.txt
                string base64Data = "data:image/png;base64," + Convert.ToBase64String(captchaBytes);
                File.WriteAllText(requestFile, base64Data);

                // Vòng lặp chờ kết quả từ Python Service (Timeout tối đa 3 giây)
                DateTime start = DateTime.Now;
                while ((DateTime.Now - start).TotalSeconds < 3)
                {
                    if (File.Exists(resultFile))
                    {
                        string result = File.ReadAllText(resultFile).Trim();
                        if (!string.IsNullOrEmpty(result))
                        {
                            return result;
                        }
                    }
                    System.Threading.Thread.Sleep(30); // Kiểm tra lại sau mỗi 30ms
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi giao tiếp với Python OCR: " + ex.Message);
            }
            return "";
        }
        public static void ReloadCaptcha(IWebDriver driver, WebDriverWait wait)
        {
            try
            {
                var captchaImg = driver.FindElement(By.Id("captcha"));
                captchaImg.Click();
                System.Threading.Thread.Sleep(300);
            }
            catch
            {
                try
                {
                    driver.Navigate().Refresh();
                    System.Threading.Thread.Sleep(500);
                }
                catch { }
            }
        }

        public static string WaitForZip(string shdon, string folder, int timeoutSeconds)
        {
            DateTime start = DateTime.Now;

            while ((DateTime.Now - start).TotalSeconds < timeoutSeconds)
            {
                var downloading = Directory.GetFiles(folder, "*.crdownload")
                    .Concat(Directory.GetFiles(folder, "*.tmp"))
                    .ToList();

                if (downloading.Count > 0)
                {
                    System.Threading.Thread.Sleep(500);
                    continue;
                }

                var zipFiles = Directory.GetFiles(folder, "*.zip");

                foreach (var zip in zipFiles)
                {
                    string fileName = Path.GetFileName(zip);

                    if (fileName.Contains(shdon))
                    {
                        DateTime t = File.GetLastWriteTime(zip);
                        if (t > start)
                            return zip;
                    }
                }

                System.Threading.Thread.Sleep(500);
            }

            return null;
        }
    }
}