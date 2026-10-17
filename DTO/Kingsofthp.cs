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
        // ★ ĐƯỜNG DẪN SHARE CHỨA PYTHONOCR ★
        //private const string ShareOcrPath =
        //    @"\\192.168.1.90\Ke toan 2025 New\2 Copi vao dung 4\Tools\Debug\PythonOCR";
        private const string ShareOcrPath =
    @"C:\apps\SharedDLLs\PythonOCR";

        // ★ ĐƯỜNG DẪN LOCAL ★
        private static readonly string LocalOcrPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SaovietTax", "PythonOCR");

        // ═══════════════════════════════════════════════════════
        // ★ TÌM THƯ MỤC PYTHONOCR — 3 CẤP ĐỘ ★
        // ═══════════════════════════════════════════════════════
        /// <summary>
        /// Tìm thư mục PythonOCR theo thứ tự:
        /// 1. %LOCALAPPDATA%\SaovietTax\PythonOCR (đã sync)
        /// 2. Cạnh exe (BaseDirectory)
        /// 3. Share → tự copy về local
        /// </summary>
        private static string FindOcrDir()
        {
            string baseDirOcr = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PythonOCR");

            // ═══ 1. Kiểm tra version + sync từ share nếu cần ═══
            try
            {
                string shareVerFile = Path.Combine(ShareOcrPath, "version.txt");
                string localVerFile = Path.Combine(LocalOcrPath, "version.txt");

                string sv = File.Exists(shareVerFile) ? File.ReadAllText(shareVerFile).Trim() : "";
                string lv = File.Exists(localVerFile) ? File.ReadAllText(localVerFile).Trim() : "";

                if (Directory.Exists(ShareOcrPath) && sv != lv)
                {
                    Console.WriteLine($"🔄 Sync PythonOCR từ share (version {lv} → {sv})...");
                    CopyDirectory(ShareOcrPath, LocalOcrPath);
                    try { File.WriteAllText(localVerFile, sv); } catch { }
                    Console.WriteLine("✅ Sync PythonOCR xong");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Lỗi sync PythonOCR: " + ex.Message);
            }

            // ═══ 2. Local trước ═══
            if (File.Exists(Path.Combine(LocalOcrPath, "ocr_service.exe")))
                return LocalOcrPath;

            // ═══ 3. Cạnh exe ═══
            if (File.Exists(Path.Combine(baseDirOcr, "ocr_service.exe")))
                return baseDirOcr;

            // ═══ 4. Copy từ share về local nếu chưa có ═══
            if (Directory.Exists(ShareOcrPath) &&
                File.Exists(Path.Combine(ShareOcrPath, "ocr_service.exe")))
            {
                try
                {
                    Console.WriteLine("📦 Copy PythonOCR từ share về local lần đầu...");
                    CopyDirectory(ShareOcrPath, LocalOcrPath);
                    if (File.Exists(Path.Combine(LocalOcrPath, "ocr_service.exe")))
                    {
                        Console.WriteLine("✅ Copy PythonOCR OK");
                        return LocalOcrPath;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Lỗi copy PythonOCR: " + ex.Message);
                }
            }

            return null;
        }

        // ═══════════════════════════════════════════════════════
        // ★ COPY THƯ MỤC — CHỈ COPY FILE MỚI HƠN ★
        // ═══════════════════════════════════════════════════════
        private static void CopyDirectory(string source, string dest)
        {
            Directory.CreateDirectory(dest);

            foreach (var file in Directory.GetFiles(source))
            {
                string fileName = Path.GetFileName(file);
                string dst = Path.Combine(dest, fileName);

                try
                {
                    if (File.Exists(dst) &&
                        File.GetLastWriteTimeUtc(dst) >= File.GetLastWriteTimeUtc(file) &&
                        new FileInfo(dst).Length == new FileInfo(file).Length)
                    {
                        continue;
                    }
                    File.Copy(file, dst, true);
                }
                catch { }
            }

            foreach (var dir in Directory.GetDirectories(source))
            {
                string dirName = Path.GetFileName(dir);
                CopyDirectory(dir, Path.Combine(dest, dirName));
            }
        }

        // ═══════════════════════════════════════════════════════
        // ★ OCR QUA FILE IPC (request.txt / result.txt) ★
        // ═══════════════════════════════════════════════════════
        public static string OcrCaptchaByPython(byte[] captchaBytes)
        {
            try
            {
                string baseDir = FindOcrDir();

                if (string.IsNullOrEmpty(baseDir))
                {
                    Console.WriteLine("❌ Không tìm thấy PythonOCR ở local, cạnh exe, hoặc share");
                    return "";
                }

                string requestFile = Path.Combine(baseDir, "request.txt");
                string resultFile = Path.Combine(baseDir, "result.txt");
                string scriptFile = Path.Combine(baseDir, "ocr_service.py");

                if (!Directory.Exists(baseDir))
                    Directory.CreateDirectory(baseDir);

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

        // ═══════════════════════════════════════════════════════
        // ★ OCR CAPTCHA — RETRY 3 LẦN ★
        // ═══════════════════════════════════════════════════════
        public static string OcrCaptcha(byte[] captchaBytes)
        {
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

            string lastResult = OcrCaptchaOnce(captchaBytes);
            Console.WriteLine($"⚠️ OCR 3 lần đều fail — trả về lần cuối: '{lastResult}'");
            return lastResult;
        }

        // ═══════════════════════════════════════════════════════
        // ★ OCR 1 LẦN — GỌI ocr_service.exe ★
        // ═══════════════════════════════════════════════════════
        private static string OcrCaptchaOnce(byte[] captchaBytes)
        {
            try
            {
                string baseDir = FindOcrDir();

                if (string.IsNullOrEmpty(baseDir))
                {
                    Console.WriteLine("❌ Không tìm thấy PythonOCR");
                    return "";
                }

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

        // ═══════════════════════════════════════════════════════
        // ★ KIỂM TRA KẾT QUẢ OCR ★
        // ═══════════════════════════════════════════════════════
        private static bool IsValidCaptcha(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;

            code = code.Trim();

            if (code.Length != 4) return false;

            foreach (char c in code)
            {
                if (!char.IsLetterOrDigit(c)) return false;
            }

            return true;
        }

        // ═══════════════════════════════════════════════════════
        // ★ RELOAD CAPTCHA ★
        // ═══════════════════════════════════════════════════════
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

        // ═══════════════════════════════════════════════════════
        // ★ CHỜ FILE ZIP TẢI VỀ ★
        // ═══════════════════════════════════════════════════════
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