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
                string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PythonOCR");
                string requestFile = Path.Combine(baseDir, "request.txt");
                string resultFile = Path.Combine(baseDir, "result.txt");
                string scriptFile = Path.Combine(baseDir, "ocr_service.py");

                if (!Directory.Exists(baseDir))
                {
                    Directory.CreateDirectory(baseDir);
                }

                var processes = Process.GetProcessesByName("python");
                if (processes.Length == 0 && File.Exists(scriptFile))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "python",
                        Arguments = $"\"{scriptFile}\"",
                        WorkingDirectory = baseDir,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false
                    };
                    Process.Start(startInfo);
                    System.Threading.Thread.Sleep(2000);
                }

                if (File.Exists(resultFile))
                {
                    File.Delete(resultFile);
                }

                string base64Data = "data:image/png;base64," + Convert.ToBase64String(captchaBytes);
                File.WriteAllText(requestFile, base64Data);

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
                    System.Threading.Thread.Sleep(30);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi giao tiếp với Python OCR: " + ex.Message);
            }
            return "";
        }

        /// <summary>
        /// OCR CAPTCHA với retry nội bộ 3 lần (dùng ddddocr qua service Python)
        /// </summary>
        public static string OcrCaptcha(byte[] captchaBytes)
        {
            // ═══ THỬ TỐI ĐA 3 LẦN (ddddocr có random nhẹ, đôi khi lần 2 mới đúng) ═══
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                string result = OcrCaptchaOnce(captchaBytes);

                if (IsValidCaptcha(result))
                {
                    Console.WriteLine($"✅ OCR lần {attempt}: '{result}'");
                    return result;
                }

                Console.WriteLine($"⚠️ OCR lần {attempt} không hợp lệ: '{result}'");
                System.Threading.Thread.Sleep(100);
            }

            // Trả về kết quả cuối cùng dù không hợp lệ (để caller biết)
            string lastResult = OcrCaptchaOnce(captchaBytes);
            Console.WriteLine($"⚠️ OCR 3 lần đều fail — trả về lần cuối: '{lastResult}'");
            return lastResult;
        }

        /// <summary>
        /// Gọi service Python OCR 1 lần (không retry)
        /// </summary>
        private static string OcrCaptchaOnce(byte[] captchaBytes)
        {
            try
            {
                string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PythonOCR");
                string requestFile = Path.Combine(baseDir, "request.txt");
                string resultFile = Path.Combine(baseDir, "result.txt");
                string readyFile = Path.Combine(baseDir, "ready.flag");
                string exeFile = Path.Combine(baseDir, "ocr_service.exe");

                if (!Directory.Exists(baseDir))
                    Directory.CreateDirectory(baseDir);

                // ✅ 1. Kiểm tra service đang chạy
                if (Process.GetProcessesByName("ocr_service").Length == 0)
                {
                    if (!File.Exists(exeFile))
                    {
                        Console.WriteLine($"❌ Không tìm thấy: {exeFile}");
                        return "";
                    }

                    if (File.Exists(readyFile)) File.Delete(readyFile);

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = exeFile,
                        WorkingDirectory = baseDir,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false
                    });

                    // ✅ 2. Chờ ready.flag
                    DateTime start = DateTime.Now;
                    while (!File.Exists(readyFile) && (DateTime.Now - start).TotalSeconds < 60)
                        System.Threading.Thread.Sleep(100);

                    if (!File.Exists(readyFile))
                    {
                        Console.WriteLine("❌ OCR service không khởi động được sau 60s");
                        return "";
                    }
                }

                // ✅ 3. Xóa kết quả cũ + request cũ
                if (File.Exists(resultFile)) File.Delete(resultFile);
                if (File.Exists(requestFile)) File.Delete(requestFile);

                // ✅ 4. Ghi request
                string base64Data = "data:image/png;base64," + Convert.ToBase64String(captchaBytes);
                File.WriteAllText(requestFile, base64Data);

                // ✅ 5. Chờ kết quả (tối đa 5s)
                DateTime waitStart = DateTime.Now;
                while ((DateTime.Now - waitStart).TotalSeconds < 5)
                {
                    if (File.Exists(resultFile))
                    {
                        string result = File.ReadAllText(resultFile).Trim();
                        if (!string.IsNullOrEmpty(result))
                            return result;
                    }
                    System.Threading.Thread.Sleep(30);
                }

                Console.WriteLine("⚠️ Timeout chờ OCR");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi OCR: " + ex.Message);
            }
            return "";
        }

        /// <summary>
        /// Kiểm tra kết quả OCR có hợp lệ không:
        /// - Đúng 4 ký tự
        /// - Chỉ chứa chữ cái + số
        /// </summary>
        private static bool IsValidCaptcha(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;

            code = code.Trim();

            // Captcha VNPT luôn 4 ký tự
            if (code.Length != 4) return false;

            // Chỉ chứa chữ cái + số
            foreach (char c in code)
            {
                if (!char.IsLetterOrDigit(c)) return false;
            }

            return true;
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