using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace SaovietTax.DTO
{
    public static class PdfKeCotTieuDe
    {
        // ============================================================
        // MODEL
        // ============================================================

        private class TextItem
        {
            public string Text;
            public float X1;
            public float X2;
            public float Y1;
            public float Y2;
            public float CenterX;
            public float CenterY;

            public float Width
            {
                get { return X2 - X1; }
            }
        }

        private class PdfLine
        {
            public float X1;
            public float Y1;
            public float X2;
            public float Y2;

            public bool IsVertical
            {
                get
                {
                    return Math.Abs(X1 - X2) <= 1.5f &&
                           Math.Abs(Y2 - Y1) > 5;
                }
            }

            public bool IsHorizontal
            {
                get
                {
                    return Math.Abs(Y1 - Y2) <= 1.5f &&
                           Math.Abs(X2 - X1) > 5;
                }
            }

            public float X
            {
                get { return (X1 + X2) / 2f; }
            }

            public float Y
            {
                get { return (Y1 + Y2) / 2f; }
            }
        }

        // ============================================================
        // THAM SỐ CẤU HÌNH (tinh chỉnh được)
        // ============================================================

        // Dung sai Y khi nhóm text thành hàng
        private const float DungSaiY = 3f;

        // Dung sai X khi kiểm tra đường dọc đã có
        private const float DuongDocDungSai = 8f;

        // Số cột tối thiểu / tối đa chấp nhận
        private const int MinCols = 3;
        private const int MaxCols = 15;

        // Khoảng cách tối thiểu giữa 2 biên cột
        private const float MinColGap = 15f;

        // Độ rộng tối thiểu của bảng (theo X)
        private const float MinTableWidth = 100f;

        // ============================================================
        // HÀM CHÍNH
        // ============================================================

        public static void XuLyPdf(
            string inputPdf,
            string outputPdf)
        {
            PdfReader reader = null;
            PdfStamper stamper = null;

            try
            {
                reader = new PdfReader(inputPdf);

                stamper = new PdfStamper(
                    reader,
                    new System.IO.FileStream(
                        outputPdf,
                        System.IO.FileMode.Create));

                for (int pageNumber = 1;
                     pageNumber <= reader.NumberOfPages;
                     pageNumber++)
                {
                    XuLyTrang(reader, stamper, pageNumber);
                }
            }
            finally
            {
                if (stamper != null) stamper.Close();
                if (reader != null) reader.Close();
            }
        }

        // ============================================================
        // XỬ LÝ TỪNG TRANG
        // ============================================================

        private static void XuLyTrang(
            PdfReader reader,
            PdfStamper stamper,
            int pageNumber)
        {
            // 1. Lấy text thô
            List<TextItem> rawTexts = LayText(reader, pageNumber);
            if (rawTexts.Count == 0) return;

            // 2. Gộp các mảnh text cùng dòng (giảm nhiễu)
            List<TextItem> texts = GopTextCungDong(rawTexts);

            // 3. Lấy đường kẻ vector (nếu có sẵn trong PDF)
            List<PdfLine> lines = LayDuongKe(reader, pageNumber);

            // 4. TỰ ĐỘNG PHÁT HIỆN CỘT
            float headerTop, headerBottom;
            List<float> columnX = PhatHienBienCot(
                texts, out headerTop, out headerBottom);

            if (columnX.Count < MinCols)
                return;

            // 5. Kiểm tra đường dọc đã có chưa
            List<float> missingLines = new List<float>();
            foreach (float x in columnX)
            {
                if (!CoDuongDoc(lines, x, headerBottom, headerTop))
                    missingLines.Add(x);
            }

            if (missingLines.Count == 0)
                return;

            // 6. Vẽ đường dọc trong vùng header
            PdfContentByte canvas =
                stamper.GetOverContent(pageNumber);

            canvas.SaveState();
            canvas.SetLineWidth(0.5f);
            canvas.SetColorStroke(BaseColor.BLACK);

            foreach (float x in missingLines)
            {
                canvas.MoveTo(x, headerBottom);
                canvas.LineTo(x, headerTop);
                canvas.Stroke();
            }

            canvas.RestoreState();
        }

        // ============================================================
        // TỰ ĐỘNG PHÁT HIỆN CỘT
        // ------------------------------------------------------------
        // Trả về: danh sách X của các biên cột + vùng header
        // ============================================================

        private static List<float> PhatHienBienCot(
            List<TextItem> texts,
            out float headerTop,
            out float headerBottom)
        {
            headerTop = 0;
            headerBottom = 0;

            // 1. Nhóm text theo Y (mỗi nhóm = 1 hàng)
            List<List<TextItem>> rows = NhomTheoY(texts, DungSaiY);

            // 2. Tìm hàng "giống header" nhất
            List<TextItem> headerRow = TimHangHeader(rows);

            if (headerRow == null || headerRow.Count < MinCols)
                return new List<float>();

            // 3. Xác định vùng header
            headerTop = headerRow.Max(t => t.Y2) + 5;
            headerBottom = headerRow.Min(t => t.Y1) - 5;

            // 4. Sắp xếp theo X
            headerRow = headerRow.OrderBy(t => t.CenterX).ToList();

            // 5. Tìm biên cột
            List<float> bienCot = new List<float>();

            // Biên trái cùng
            bienCot.Add(headerRow[0].X1 - 5);

            // Biên giữa các text
            for (int i = 0; i < headerRow.Count - 1; i++)
            {
                float x = (headerRow[i].X2 + headerRow[i + 1].X1) / 2f;
                bienCot.Add(x);
            }

            // Biên phải cùng
            bienCot.Add(headerRow[headerRow.Count - 1].X2 + 5);

            // 6. Lọc biên quá gần nhau
            List<float> ketQua = new List<float>();
            foreach (float x in bienCot)
            {
                if (ketQua.Count == 0 ||
                    Math.Abs(x - ketQua[ketQua.Count - 1]) > MinColGap)
                {
                    ketQua.Add(x);
                }
            }

            return ketQua;
        }

        // ============================================================
        // NHÓM TEXT THEO Y
        // ============================================================

        private static List<List<TextItem>> NhomTheoY(
            List<TextItem> texts,
            float dungSaiY)
        {
            List<List<TextItem>> groups = new List<List<TextItem>>();

            foreach (TextItem t in texts.OrderBy(x => x.CenterY))
            {
                List<TextItem> g = groups.FirstOrDefault(gr =>
                    Math.Abs(gr[0].CenterY - t.CenterY) <= dungSaiY);

                if (g == null)
                {
                    g = new List<TextItem>();
                    groups.Add(g);
                }

                g.Add(t);
            }

            return groups;
        }

        // ============================================================
        // TÌM HÀNG HEADER
        // ------------------------------------------------------------
        // Điểm = số item * độ rộng X / độ dài text trung bình
        // Hàng header thường: nhiều item, trải rộng, text ngắn
        // ============================================================

        private static List<TextItem> TimHangHeader(
            List<List<TextItem>> rows)
        {
            List<TextItem> best = null;
            double bestScore = -1;

            foreach (var row in rows)
            {
                // Lọc theo số item
                if (row.Count < MinCols || row.Count > MaxCols)
                    continue;

                float xMin = row.Min(t => t.X1);
                float xMax = row.Max(t => t.X2);
                float xRange = xMax - xMin;

                // Bảng phải đủ rộng
                if (xRange < MinTableWidth)
                    continue;

                var avgLen = row.Average(t => t.Text.Length);

                // Text header thường ngắn
                if (avgLen > 30)
                    continue;

                // Điểm: nhiều item + rộng + text ngắn
                var score = row.Count * xRange / Math.Max(avgLen, 1);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = row;
                }
            }

            return best;
        }

        // ============================================================
        // GỘP TEXT CÙNG DÒNG
        // ============================================================

        private static List<TextItem> GopTextCungDong(
            List<TextItem> texts)
        {
            List<TextItem> result = new List<TextItem>();

            List<TextItem> sorted =
                texts.OrderBy(t => t.CenterY)
                     .ThenBy(t => t.X1)
                     .ToList();

            bool[] used = new bool[sorted.Count];

            for (int i = 0; i < sorted.Count; i++)
            {
                if (used[i]) continue;

                List<TextItem> group = new List<TextItem> { sorted[i] };
                used[i] = true;

                for (int j = i + 1; j < sorted.Count; j++)
                {
                    if (used[j]) continue;

                    if (Math.Abs(sorted[j].CenterY - sorted[i].CenterY) > 2)
                        continue;

                    float lastX2 = group.Max(g => g.X2);
                    if (sorted[j].X1 - lastX2 > 15)
                        continue;

                    group.Add(sorted[j]);
                    used[j] = true;
                }

                if (group.Count == 1)
                {
                    result.Add(group[0]);
                }
                else
                {
                    float minX = group.Min(g => g.X1);
                    float maxX = group.Max(g => g.X2);
                    float minY = group.Min(g => g.Y1);
                    float maxY = group.Max(g => g.Y2);

                    result.Add(new TextItem
                    {
                        Text = string.Join("",
                            group.OrderBy(g => g.X1).Select(g => g.Text)),
                        X1 = minX,
                        X2 = maxX,
                        Y1 = minY,
                        Y2 = maxY,
                        CenterX = (minX + maxX) / 2f,
                        CenterY = group.Average(g => g.CenterY)
                    });
                }
            }

            return result;
        }

        // ============================================================
        // LẤY TEXT + TỌA ĐỘ
        // ============================================================

        private static List<TextItem> LayText(
            PdfReader reader,
            int pageNumber)
        {
            TextPositionListener listener = new TextPositionListener();
            PdfReaderContentParser parser = new PdfReaderContentParser(reader);
            parser.ProcessContent(pageNumber, listener);
            return listener.Items;
        }

        // ============================================================
        // LẤY ĐƯỜNG VECTOR
        // ============================================================

        private static List<PdfLine> LayDuongKe(
            PdfReader reader,
            int pageNumber)
        {
            LineListener listener = new LineListener();
            PdfReaderContentParser parser = new PdfReaderContentParser(reader);
            parser.ProcessContent(pageNumber, listener);
            return listener.Lines;
        }

        // ============================================================
        // KIỂM TRA ĐÃ CÓ ĐƯỜNG DỌC
        // ============================================================

        private static bool CoDuongDoc(
            List<PdfLine> lines,
            float x,
            float yBottom,
            float yTop)
        {
            foreach (PdfLine line in lines)
            {
                if (!line.IsVertical)
                    continue;

                if (Math.Abs(line.X - x) > DuongDocDungSai)
                    continue;

                float lineBottom = Math.Min(line.Y1, line.Y2);
                float lineTop = Math.Max(line.Y1, line.Y2);

                float overlap =
                    Math.Min(lineTop, yTop)
                    -
                    Math.Max(lineBottom, yBottom);

                if (overlap >= 5)
                    return true;
            }

            return false;
        }

        // ============================================================
        // TEXT LISTENER
        // ============================================================

        private class TextPositionListener : IRenderListener
        {
            public List<TextItem> Items = new List<TextItem>();

            public void BeginTextBlock() { }
            public void EndTextBlock() { }
            public void RenderImage(ImageRenderInfo renderInfo) { }

            public void RenderText(TextRenderInfo renderInfo)
            {
                string text = renderInfo.GetText();
                if (String.IsNullOrWhiteSpace(text)) return;

                LineSegment ascent = renderInfo.GetAscentLine();
                LineSegment descent = renderInfo.GetDescentLine();

                float x1 = ascent.GetStartPoint()[0];
                float x2 = ascent.GetEndPoint()[0];
                float y1 = descent.GetStartPoint()[1];
                float y2 = ascent.GetEndPoint()[1];

                if (x2 < x1) { float t = x1; x1 = x2; x2 = t; }
                if (y2 < y1) { float t = y1; y1 = y2; y2 = t; }

                Items.Add(new TextItem
                {
                    Text = text,
                    X1 = x1,
                    X2 = x2,
                    Y1 = y1,
                    Y2 = y2,
                    CenterX = (x1 + x2) / 2f,
                    CenterY = (y1 + y2) / 2f
                });
            }
        }

        // ============================================================
        // LINE LISTENER
        // ============================================================

        private class LineListener : IExtRenderListener
        {
            private iTextSharp.text.pdf.parser.Vector moveTo;
            private iTextSharp.text.pdf.parser.Vector lineTo;

            public List<PdfLine> Lines = new List<PdfLine>();

            public void BeginTextBlock() { }
            public void EndTextBlock() { }
            public void RenderText(TextRenderInfo renderInfo) { }
            public void RenderImage(ImageRenderInfo renderInfo) { }

            public void ModifyPath(PathConstructionRenderInfo renderInfo)
            {
                switch (renderInfo.Operation)
                {
                    case PathConstructionRenderInfo.MOVETO:
                        {
                            IList<float> data = renderInfo.SegmentData;
                            if (data != null && data.Count >= 2)
                            {
                                moveTo = new iTextSharp.text.pdf.parser.Vector(
                                    data[0], data[1], 1);
                            }
                            lineTo = null;
                        }
                        break;

                    case PathConstructionRenderInfo.LINETO:
                        {
                            IList<float> data = renderInfo.SegmentData;
                            if (data != null && data.Count >= 2 && moveTo != null)
                            {
                                lineTo = new iTextSharp.text.pdf.parser.Vector(
                                    data[0], data[1], 1);
                            }
                        }
                        break;

                    default:
                        moveTo = null;
                        lineTo = null;
                        break;
                }
            }

            public iTextSharp.text.pdf.parser.Path
                RenderPath(PathPaintingRenderInfo renderInfo)
            {
                if (moveTo != null && lineTo != null &&
                    renderInfo.Operation != PathPaintingRenderInfo.NO_OP)
                {
                    var from = moveTo.Cross(renderInfo.Ctm);
                    var to = lineTo.Cross(renderInfo.Ctm);

                    float x1 = from[0];
                    float y1 = from[1];
                    float x2 = to[0];
                    float y2 = to[1];

                    float dx = x2 - x1;
                    float dy = y2 - y1;

                    // Lấy cả đường dọc và đường ngang
                    if (Math.Abs(dx) <= 2 && Math.Abs(dy) > 5)
                    {
                        Lines.Add(new PdfLine
                        {
                            X1 = x1,
                            Y1 = y1,
                            X2 = x2,
                            Y2 = y2
                        });
                    }
                    else if (Math.Abs(dy) <= 2 && Math.Abs(dx) > 5)
                    {
                        Lines.Add(new PdfLine
                        {
                            X1 = x1,
                            Y1 = y1,
                            X2 = x2,
                            Y2 = y2
                        });
                    }
                }

                moveTo = null;
                lineTo = null;
                return null;
            }

            public void ClipPath(int rule) { }
        }
    }
}