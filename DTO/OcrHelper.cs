using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tesseract;

namespace SaovietTax.DTO
{
    public static class OcrHelper
    {
        // ============================================================
        // READ DIGITS FROM IMAGE — Đọc số từ ảnh (có xóa đường kẻ)
        // ============================================================
        public static string ReadDigitsFromImage(string inputImagePath,
                                                 bool removeLines = false,
                                                 int scale = 3)
        {
            string tempFolder = Path.Combine(
                Path.GetTempPath(), "SaovietTaxOcr");
            Directory.CreateDirectory(tempFolder);

            string processedPath = Path.Combine(
                tempFolder, $"{Guid.NewGuid():N}.png");

            string finalPath = processedPath;

            try
            {
                // 1. Tiền xử lý
                ImagePreprocessor.Preprocess(inputImagePath, processedPath,
                    scale: scale, threshold: 140,
                    minArea: 0, useMorphology: false);

                // 2. Xóa đường kẻ nếu cần
                if (removeLines)
                {
                    string noLine = Path.Combine(
                        tempFolder, $"{Guid.NewGuid():N}_noline.png");
                    ImagePreprocessor.RemoveLines(processedPath, noLine);
                    finalPath = noLine;
                }

                // 3. OCR
                string result = OcrEngine.RecognizeBest(finalPath, digitsOnly: true);

                // 4. Dọn kết quả
                return CleanDigits(result);
            }
            finally
            {
                // Dọn file tạm
                try
                {
                    if (File.Exists(processedPath))
                        File.Delete(processedPath);
                    if (finalPath != processedPath && File.Exists(finalPath))
                        File.Delete(finalPath);
                }
                catch { }
            }
        }

        // ============================================================
        // READ DIGITS — Đọc số (chỉ resize + grayscale)
        // ============================================================
        public static string ReadDigits(string inputImagePath)
        {
            string tempFolder = Path.Combine(
                Path.GetTempPath(), "SaovietTaxOcr");
            Directory.CreateDirectory(tempFolder);

            string processedPath = Path.Combine(
                tempFolder, $"{Guid.NewGuid():N}.png");

            try
            {
                ImagePreprocessor.PreprocessMinimal(
                    inputImagePath, processedPath, scale: 3);

                string result = OcrEngine.RecognizeBest(processedPath, digitsOnly: true);

                return CleanDigits(result);
            }
            finally
            {
                try
                {
                    if (File.Exists(processedPath))
                        File.Delete(processedPath);
                }
                catch { }
            }
        }

        // ============================================================
        // READ DIGITS WITH VOTING — Test nhiều cấu hình, chọn kết quả tốt
        // ============================================================
        public static string ReadDigitsWithVoting(string inputImagePath)
        {
            string folder = @"D:\TestVote";
            Directory.CreateDirectory(folder);

            var votes = new Dictionary<string, int>();
            var configs = new List<string>();

            var images = new Dictionary<string, string>();

            // 1. Minimal
            string pMinimal = Path.Combine(folder, "minimal.png");
            ImagePreprocessor.PreprocessMinimal(inputImagePath, pMinimal, 3);
            images["minimal"] = pMinimal;

            // 2. Threshold cố định
            foreach (var th in new[] { 120, 140, 160, 180 })
            {
                string p = Path.Combine(folder, $"th{th}.png");
                ImagePreprocessor.Preprocess(inputImagePath, p, scale: 3,
                    threshold: th, minArea: 0, useMorphology: false);
                images[$"th{th}"] = p;
            }

            // 3. Adaptive
            foreach (var bs in new[] { 11, 15, 21 })
            {
                string p = Path.Combine(folder, $"adaptive_bs{bs}.png");
                ImagePreprocessor.PreprocessAdaptive(inputImagePath, p,
                    scale: 3, blockSize: bs, c: 3);
                images[$"adaptive_bs{bs}"] = p;
            }

            string tessDataPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "tessdata");

            var psms = new[]
            {
                PageSegMode.SingleLine,
                PageSegMode.SingleWord,
            };

            foreach (var img in images)
            {
                foreach (var psm in psms)
                {
                    try
                    {
                        using (var engine = new TesseractEngine(
                            tessDataPath, "eng", EngineMode.LstmOnly))
                        {
                            engine.SetVariable("tessedit_char_whitelist", "0123456789");
                            engine.SetVariable("classify_bln_numeric_mode", "1");
                            engine.SetVariable("load_system_dawg", "0");
                            engine.SetVariable("load_freq_dawg", "0");

                            using (var pix = Pix.LoadFromFile(img.Value))
                            using (var page = engine.Process(pix, psm))
                            {
                                string text = CleanDigits(
                                    page.GetText()?.Trim() ?? "");

                                if (text.Length >= 3)
                                {
                                    if (votes.ContainsKey(text))
                                        votes[text]++;
                                    else
                                        votes[text] = 1;

                                    configs.Add($"{img.Key}|{psm} → '{text}'");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{img.Key}|{psm}: {ex.Message}");
                    }
                }
            }

            Console.WriteLine("=== KẾT QUẢ TỪNG CẤU HÌNH ===");
            foreach (var c in configs)
                Console.WriteLine(c);

            if (votes.Count == 0)
                return "";

            var winner = votes.OrderByDescending(kv => kv.Value).First();
            Console.WriteLine($"\n🏆 Kết quả: '{winner.Key}' ({winner.Value} phiếu)");

            return winner.Key;
        }

        // ============================================================
        // CLEAN DIGITS — Chỉ giữ số
        // ============================================================
        private static string CleanDigits(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            var sb = new System.Text.StringBuilder();
            foreach (char c in input)
            {
                if (char.IsDigit(c)) sb.Append(c);
            }
            return sb.ToString();
        }
    }
}