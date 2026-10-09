using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Drawing;
using System.IO;

namespace SaovietTax.DTO
{
    public static class ImagePreprocessor
    {
        // ============================================================
        // PREPROCESS MINIMAL — Chỉ resize + grayscale
        // ============================================================
        public static string PreprocessMinimal(string inputPath, string outputPath,
                                               int scale = 3)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Không tìm thấy ảnh", inputPath);

            using (var src = CvInvoke.Imread(inputPath))
            {
                if (src.IsEmpty)
                    throw new Exception("Không đọc được ảnh: " + inputPath);

                var resized = new Mat();
                CvInvoke.Resize(src, resized, new Size(0, 0),
                    scale, scale, Inter.Cubic);

                var gray = new Mat();
                if (resized.NumberOfChannels == 3)
                    CvInvoke.CvtColor(resized, gray, ColorConversion.Bgr2Gray);
                else if (resized.NumberOfChannels == 4)
                    CvInvoke.CvtColor(resized, gray, ColorConversion.Bgra2Gray);
                else
                    resized.CopyTo(gray);

                CvInvoke.Imwrite(outputPath, gray);

                resized.Dispose();
                gray.Dispose();
            }

            return outputPath;
        }

        // ============================================================
        // PREPROCESS — Threshold cố định
        // ============================================================
        public static string Preprocess(string inputPath, string outputPath,
                                        int scale = 3,
                                        int threshold = 140,
                                        int minArea = 0,
                                        bool useMorphology = false)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Không tìm thấy ảnh", inputPath);

            using (var src = CvInvoke.Imread(inputPath))
            {
                if (src.IsEmpty)
                    throw new Exception("Không đọc được ảnh: " + inputPath);

                var resized = new Mat();
                CvInvoke.Resize(src, resized, new Size(0, 0),
                    scale, scale, Inter.Cubic);

                var gray = new Mat();
                if (resized.NumberOfChannels == 3)
                    CvInvoke.CvtColor(resized, gray, ColorConversion.Bgr2Gray);
                else if (resized.NumberOfChannels == 4)
                    CvInvoke.CvtColor(resized, gray, ColorConversion.Bgra2Gray);
                else
                    resized.CopyTo(gray);

                var binary = new Mat();
                CvInvoke.Threshold(gray, binary, threshold, 255,
                    ThresholdType.Binary);

                double whiteRatio = (double)CvInvoke.CountNonZero(binary)
                                    / (binary.Rows * binary.Cols);
                Console.WriteLine($"[Preprocess] th={threshold}, whiteRatio={whiteRatio:F2}");

                if (!IsBackgroundWhite(binary))
                {
                    CvInvoke.BitwiseNot(binary, binary);
                    whiteRatio = (double)CvInvoke.CountNonZero(binary)
                                 / (binary.Rows * binary.Cols);
                    Console.WriteLine($"[Preprocess] Đảo ngược → whiteRatio={whiteRatio:F2}");
                }

                if (useMorphology)
                {
                    using (var kernel = new Mat(2, 2, DepthType.Cv8U, 1))
                    {
                        kernel.SetTo(new MCvScalar(1));
                        var cleaned = new Mat();
                        CvInvoke.MorphologyEx(binary, cleaned, MorphOp.Open,
                            kernel, new Point(-1, -1), 1,
                            BorderType.Default, new MCvScalar());
                        cleaned.CopyTo(binary);
                        cleaned.Dispose();
                    }
                }

                if (minArea > 0)
                {
                    var inverted = new Mat();
                    CvInvoke.BitwiseNot(binary, inverted);

                    var contours = new VectorOfVectorOfPoint();
                    var hierarchy = new Mat();
                    CvInvoke.FindContours(inverted, contours, hierarchy,
                        RetrType.External, ChainApproxMethod.ChainApproxSimple);

                    var mask = new Mat(binary.Size, DepthType.Cv8U, 1);
                    mask.SetTo(new MCvScalar(0));

                    for (int i = 0; i < contours.Size; i++)
                    {
                        double area = CvInvoke.ContourArea(contours[i]);
                        if (area >= minArea)
                            CvInvoke.DrawContours(mask, contours, i,
                                new MCvScalar(255), -1);
                    }

                    CvInvoke.BitwiseNot(mask, mask);
                    mask.CopyTo(binary);

                    inverted.Dispose();
                    contours.Dispose();
                    hierarchy.Dispose();
                    mask.Dispose();
                }

                CvInvoke.Imwrite(outputPath, binary);

                resized.Dispose();
                gray.Dispose();
                binary.Dispose();
            }

            return outputPath;
        }

        // ============================================================
        // PREPROCESS ADAPTIVE — Adaptive Threshold
        // ============================================================
        public static string PreprocessAdaptive(string inputPath, string outputPath,
                                                int scale = 3,
                                                int blockSize = 15,
                                                int c = 3)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Không tìm thấy ảnh", inputPath);

            if (blockSize % 2 == 0) blockSize++;

            using (var src = CvInvoke.Imread(inputPath))
            {
                if (src.IsEmpty)
                    throw new Exception("Không đọc được ảnh");

                var resized = new Mat();
                CvInvoke.Resize(src, resized, new Size(0, 0),
                    scale, scale, Inter.Cubic);

                var gray = new Mat();
                if (resized.NumberOfChannels == 3)
                    CvInvoke.CvtColor(resized, gray, ColorConversion.Bgr2Gray);
                else if (resized.NumberOfChannels == 4)
                    CvInvoke.CvtColor(resized, gray, ColorConversion.Bgra2Gray);
                else
                    resized.CopyTo(gray);

                var binary = new Mat();
                CvInvoke.AdaptiveThreshold(gray, binary, 255,
                    AdaptiveThresholdType.GaussianC,
                    ThresholdType.Binary,
                    blockSize, c);

                if (!IsBackgroundWhite(binary))
                {
                    CvInvoke.BitwiseNot(binary, binary);
                    Console.WriteLine("[Adaptive] Đảo ngược");
                }

                double whiteRatio = (double)CvInvoke.CountNonZero(binary)
                                    / (binary.Rows * binary.Cols);
                Console.WriteLine($"[Adaptive] bs={blockSize}, c={c}, whiteRatio={whiteRatio:F2}");

                CvInvoke.Imwrite(outputPath, binary);

                resized.Dispose();
                gray.Dispose();
                binary.Dispose();
            }

            return outputPath;
        }

        // ============================================================
        // IS BACKGROUND WHITE
        // ============================================================
        private static bool IsBackgroundWhite(Mat binary)
        {
            int w = binary.Cols;
            int h = binary.Rows;
            int cornerSize = Math.Max(5, Math.Min(w, h) / 10);

            var corners = new[]
            {
                new Rectangle(0, 0, cornerSize, cornerSize),
                new Rectangle(w - cornerSize, 0, cornerSize, cornerSize),
                new Rectangle(0, h - cornerSize, cornerSize, cornerSize),
                new Rectangle(w - cornerSize, h - cornerSize, cornerSize, cornerSize)
            };

            int totalWhite = 0, totalPixels = 0;

            foreach (var rect in corners)
            {
                var safeRect = Rectangle.Intersect(rect, new Rectangle(0, 0, w, h));
                if (safeRect.Width <= 0 || safeRect.Height <= 0) continue;

                using (var corner = new Mat(binary, safeRect))
                {
                    totalWhite += CvInvoke.CountNonZero(corner);
                    totalPixels += corner.Rows * corner.Cols;
                }
            }

            if (totalPixels == 0) return true;

            double ratio = (double)totalWhite / totalPixels;
            return ratio > 0.5;
        }

        // ============================================================
        // INVERT IMAGE
        // ============================================================
        public static void InvertImage(string inputPath, string outputPath)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Không tìm thấy ảnh", inputPath);

            using (var src = CvInvoke.Imread(inputPath))
            {
                if (src.IsEmpty) return;

                var inverted = new Mat();
                CvInvoke.BitwiseNot(src, inverted);
                CvInvoke.Imwrite(outputPath, inverted);
                inverted.Dispose();
            }
        }

        // ============================================================
        // REMOVE LINES
        // ============================================================
        public static void RemoveLines(string inputPath, string outputPath,
                                       int horizontalLength = 25,
                                       int verticalLength = 25)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Không tìm thấy ảnh", inputPath);

            using (var src = CvInvoke.Imread(inputPath))
            {
                if (src.IsEmpty) return;

                var gray = new Mat();
                if (src.NumberOfChannels == 3)
                    CvInvoke.CvtColor(src, gray, ColorConversion.Bgr2Gray);
                else if (src.NumberOfChannels == 4)
                    CvInvoke.CvtColor(src, gray, ColorConversion.Bgra2Gray);
                else
                    src.CopyTo(gray);

                var inverted = new Mat();
                CvInvoke.BitwiseNot(gray, inverted);

                var binary = new Mat();
                CvInvoke.Threshold(inverted, binary, 0, 255,
                    ThresholdType.Binary | ThresholdType.Otsu);

                using (var hKernel = new Mat(1, horizontalLength, DepthType.Cv8U, 1))
                using (var vKernel = new Mat(verticalLength, 1, DepthType.Cv8U, 1))
                {
                    hKernel.SetTo(new MCvScalar(1));
                    vKernel.SetTo(new MCvScalar(1));

                    var horizontal = new Mat();
                    CvInvoke.MorphologyEx(binary, horizontal, MorphOp.Open,
                        hKernel, new Point(-1, -1), 1,
                        BorderType.Default, new MCvScalar());

                    var vertical = new Mat();
                    CvInvoke.MorphologyEx(binary, vertical, MorphOp.Open,
                        vKernel, new Point(-1, -1), 1,
                        BorderType.Default, new MCvScalar());

                    var lines = new Mat();
                    CvInvoke.Add(horizontal, vertical, lines);

                    var result = new Mat();
                    CvInvoke.Subtract(binary, lines, result);

                    CvInvoke.BitwiseNot(result, result);
                    CvInvoke.Imwrite(outputPath, result);

                    horizontal.Dispose();
                    vertical.Dispose();
                    lines.Dispose();
                    result.Dispose();
                }

                gray.Dispose();
                inverted.Dispose();
                binary.Dispose();
            }
        }

        // ============================================================
        // TEST ALL COMBINATIONS — Tự test tất cả cấu hình
        // ============================================================
        public static void TestAllCombinations(string inputPath)
        {
            string folder = @"D:\TestAllComb";
            Directory.CreateDirectory(folder);

            var images = new System.Collections.Generic.Dictionary<string, string>();

            // 1. Ảnh gốc
            images["original"] = inputPath;

            // 2. Minimal (resize + grayscale)
            string pMinimal = Path.Combine(folder, "minimal.png");
            PreprocessMinimal(inputPath, pMinimal, 3);
            images["minimal"] = pMinimal;

            // 3. Threshold cố định
            foreach (var th in new[] { 100, 120, 140, 160, 180 })
            {
                string p = Path.Combine(folder, $"th{th}.png");
                Preprocess(inputPath, p, scale: 3, threshold: th,
                    minArea: 0, useMorphology: false);
                images[$"th{th}"] = p;
            }

            // 4. Adaptive
            int[] blockSizes = { 11, 15, 21 };
            int[] cValues = { 2, 3, 5 };
            foreach (var bs in blockSizes)
            {
                foreach (var c in cValues)
                {
                    string p = Path.Combine(folder, $"adaptive_bs{bs}_c{c}.png");
                    PreprocessAdaptive(inputPath, p, scale: 3, blockSize: bs, c: c);
                    images[$"adaptive_bs{bs}_c{c}"] = p;
                }
            }

            Console.WriteLine("=== ĐÃ TẠO ẢNH TEST ===");
            Console.WriteLine($"Folder: {folder}\n");

            System.Diagnostics.Process.Start("explorer.exe", folder);
        }
    }
}