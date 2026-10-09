using System;
using System.IO;
using Tesseract;

namespace SaovietTax.DTO
{
    public static class OcrEngine
    {
        private static string GetTessDataPath()
        {
            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "tessdata");

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException(
                    $"Không tìm thấy tessdata tại: {path}");

            string engFile = Path.Combine(path, "eng.traineddata");
            if (!File.Exists(engFile))
                throw new FileNotFoundException(
                    $"Không tìm thấy eng.traineddata tại: {engFile}");

            return path;
        }

        // ============================================================
        // RECOGNIZE — 1 PSM duy nhất
        // ============================================================
        public static string Recognize(string imagePath,
                                       bool digitsOnly = true,
                                       PageSegMode psm = PageSegMode.SingleLine)
        {
            if (!File.Exists(imagePath))
                throw new FileNotFoundException("Không tìm thấy ảnh", imagePath);

            string tessDataPath = GetTessDataPath();

            using (var engine = new TesseractEngine(
                tessDataPath, "eng", EngineMode.LstmOnly))
            {
                ConfigureEngine(engine, digitsOnly);

                using (var img = Pix.LoadFromFile(imagePath))
                using (var page = engine.Process(img, psm))
                {
                    string text = page.GetText()?.Trim() ?? "";
                    float confidence = page.GetMeanConfidence();
                    Console.WriteLine($"OCR [{psm}]: '{text}' (conf: {confidence:P0})");
                    return text;
                }
            }
        }

        // ============================================================
        // RECOGNIZE BEST — Thử nhiều PSM, chọn tốt nhất
        // ============================================================
        public static string RecognizeBest(string imagePath, bool digitsOnly = true)
        {
            var modes = new[]
            {
                PageSegMode.SingleLine,
                PageSegMode.SingleWord,
                PageSegMode.SingleBlock,
            };

            string tessDataPath = GetTessDataPath();
            string best = "";
            float bestConf = 0;

            foreach (var psm in modes)
            {
                try
                {
                    using (var engine = new TesseractEngine(
                        tessDataPath, "eng", EngineMode.LstmOnly))
                    {
                        ConfigureEngine(engine, digitsOnly);

                        using (var img = Pix.LoadFromFile(imagePath))
                        using (var page = engine.Process(img, psm))
                        {
                            string text = page.GetText()?.Trim() ?? "";
                            float conf = page.GetMeanConfidence();

                            Console.WriteLine($"  {psm,-15}: '{text}' (conf {conf:P0})");

                            if (text.Length > best.Length ||
                                (text.Length == best.Length && conf > bestConf))
                            {
                                best = text;
                                bestConf = conf;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  {psm,-15}: LỖI - {ex.Message}");
                }
            }

            return best;
        }

        // ============================================================
        // RECOGNIZE WITHOUT WHITELIST — Test không whitelist
        // ============================================================
        public static string RecognizeWithoutWhitelist(string imagePath)
        {
            string tessDataPath = GetTessDataPath();

            using (var engine = new TesseractEngine(
                tessDataPath, "eng", EngineMode.LstmOnly))
            {
                engine.SetVariable("load_system_dawg", "0");
                engine.SetVariable("load_freq_dawg", "0");

                using (var img = Pix.LoadFromFile(imagePath))
                using (var page = engine.Process(img, PageSegMode.SingleLine))
                {
                    return page.GetText()?.Trim() ?? "";
                }
            }
        }

        // ============================================================
        // CONFIGURE ENGINE
        // ============================================================
        private static void ConfigureEngine(TesseractEngine engine, bool digitsOnly)
        {
            if (digitsOnly)
            {
                engine.SetVariable("tessedit_char_whitelist", "0123456789");
                engine.SetVariable("classify_bln_numeric_mode", "1");
            }

            engine.SetVariable("load_system_dawg", "0");
            engine.SetVariable("load_freq_dawg", "0");
        }
    }
}