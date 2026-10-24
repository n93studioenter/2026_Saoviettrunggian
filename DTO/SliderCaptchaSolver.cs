using OpenCvSharp;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CvPoint = OpenCvSharp.Point;
using CvRect = OpenCvSharp.Rect;
using CvSize = OpenCvSharp.Size;

namespace SaovietTax.DTO
{
    public sealed class SliderCaptchaSolver
    {
        private readonly IWebDriver _driver;
        private readonly Action<string> _log;
        private readonly ChromeDriver _chrome;

        public SliderCaptchaSolver(IWebDriver driver, Action<string> log)
        {
            _driver = driver;
            _log = log;
            _chrome = driver as ChromeDriver;
        }

        // ==================================================================
        // ENTRY: một lần thử giải captcha
        // ==================================================================
        public async Task<bool> TrySolveOnceAsync()
        {
            try
            {
                // 1. Đánh dấu element
                var info = MarkSliderElements();
                if (info == null || !info.Ok)
                {
                    _log("⚠️ Captcha trượt: " + (info != null ? info.Reason : "không tìm thấy nút kéo"));
                    return false;
                }

                var js = (IJavaScriptExecutor)_driver;

                // 2. Lấy bytes ảnh
                byte[] bgBytes = ScreenshotElement(info.BgSelector);
                byte[] pieceBytes = info.PieceSelector != null ? ScreenshotElement(info.PieceSelector) : null;

                if (bgBytes == null) bgBytes = UrlToBytes(info.BgSrc);
                if (bgBytes == null)
                {
                    _log("⚠️ Không lấy được ảnh nền");
                    return false;
                }

                // 3. Tìm gap
                int? gapX = null;
                string method = "none";
                double score = 0;

                GapResult g = null;
                try
                {
                    g = GetGapFromBackgroundBytes(bgBytes);
                    gapX = g.GapX;
                    method = g.Method;
                    score = g.Score;
                }
                catch { gapX = null; }

                if ((gapX == null || gapX.Value < 15) && pieceBytes != null)
                {
                    try
                    {
                        g = GetGapFromBytes(bgBytes, pieceBytes);
                        gapX = g.GapX;
                        method = g.Method;
                        score = g.Score;
                    }
                    catch
                    {
                        g = GetGapFromBackgroundBytes(bgBytes);
                        gapX = g.GapX;
                        method = g.Method;
                        score = g.Score;
                    }
                }
                else if (gapX == null || gapX.Value < 15)
                {
                    g = GetGapFromBackgroundBytes(bgBytes);
                    gapX = g.GapX;
                    method = g.Method;
                    score = g.Score;
                }

                if (gapX == null || gapX.Value < 15)
                {
                    _log("⚠️ Không xác định được vị trí gap (" + gapX + "px)");
                    return false;
                }

                // 4. Scale natural → displayed
                var bgEl = _driver.FindElement(By.CssSelector(info.BgSelector));
                int bgDisplayedW = bgEl.Size.Width;
                int naturalW;
                using (var mat = Cv2.ImDecode(bgBytes, ImreadModes.Unchanged))
                {
                    if (mat == null || mat.Empty())
                    {
                        _log("⚠️ Decode background fail");
                        return false;
                    }
                    naturalW = mat.Cols;
                }

                double scaleX = (double)bgDisplayedW / naturalW;
                double distance = gapX.Value * scaleX;

                // 5. Điểm bắt đầu kéo
                var handleEl = _driver.FindElement(By.CssSelector(info.HandleSelector));
                var hRect = (Dictionary<string, object>)js.ExecuteScript(
                    "var r = arguments[0].getBoundingClientRect(); " +
                    "return { x: r.left, y: r.top, w: r.width, h: r.height };", handleEl);

                double hx = Convert.ToDouble(hRect["x"]);
                double hy = Convert.ToDouble(hRect["y"]);
                double hw = Convert.ToDouble(hRect["w"]);
                double hh = Convert.ToDouble(hRect["h"]);

                bool handleIsTrack = info.HandleIsTrack || hw > 120;
                double startX = handleIsTrack ? hx + Math.Min(26, Math.Max(18, hh / 2)) : hx + hw / 2;
                double startY = hy + hh / 2;

                // 6. Clamp distance
                int wrapperW = bgDisplayedW;
                if (info.WrapperSelector != null)
                {
                    try { wrapperW = _driver.FindElement(By.CssSelector(info.WrapperSelector)).Size.Width; }
                    catch { }
                }
                double maxDistance = Math.Max(10, wrapperW - 12);
                distance = Math.Max(5, Math.Min(distance, maxDistance));

                _log(string.Format("🔎 gap={0}px, kéo≈{1:F0}px, method={2}, score={3:F3}, scaleX={4:F3}",
                    gapX, distance, method, score, scaleX));

                // 7. Kéo chuột
                await DragMouseAsync(startX, startY, distance);
                await Task.Delay(1200);
                return true;
            }
            catch (Exception ex)
            {
                _log("⚠️ TrySolveOnce lỗi: " + ex.Message);
                return false;
            }
        }

        // ==================================================================
        // ĐÁNH DẤU ELEMENT
        // ==================================================================
        public SliderMarkInfo MarkSliderElements()
        {
            try
            {
                var js = (IJavaScriptExecutor)_driver;
                var result = js.ExecuteScript(@"
                    (function() {
                        var norm = function(s){ return String(s || '').toLowerCase(); };
                        function visible(el) {
                            try {
                                if (!el) return false;
                                var r = el.getBoundingClientRect();
                                var st = getComputedStyle(el);
                                return r.width > 8 && r.height > 8 && st.display !== 'none'
                                    && st.visibility !== 'hidden' && st.opacity !== '0';
                            } catch(e) { return false; }
                        }
                        function clear() {
                            var sels = ['[data-vt-bg]','[data-vt-piece]','[data-vt-handle]','[data-vt-wrapper]'];
                            for (var i=0;i<sels.length;i++){
                                var list = document.querySelectorAll(sels[i]);
                                for (var j=0;j<list.length;j++){
                                    list[j].removeAttribute('data-vt-bg');
                                    list[j].removeAttribute('data-vt-piece');
                                    list[j].removeAttribute('data-vt-handle');
                                    list[j].removeAttribute('data-vt-wrapper');
                                }
                            }
                        }
                        function q(sels) {
                            for (var i=0;i<sels.length;i++) {
                                var sel = sels[i];
                                if (!sel) continue;
                                try {
                                    var found = Array.prototype.slice.call(document.querySelectorAll(sel)).filter(visible);
                                    if (found.length) return found[0];
                                } catch(e) {}
                            }
                            return null;
                        }
                        clear();

                        var bgCandidates = [
                            '.bg-image', 'img.bg-image', '.captcha-bg', '.slider-bg',
                            '.verify-img-panel img', '.verify-img-panel canvas',
                            'img[class*=""bg"" i]', 'img[src*=""captcha"" i]',
                            '[class*=""bg-image"" i]'
                        ];
                        var pieceCandidates = [
                            'img.puzzle-piece', '.piece-image', '.slider-piece', '.jigsaw-piece',
                            '.captcha-piece', '.block', 'img[class*=""piece"" i]',
                            'img[class*=""block"" i]'
                        ];
                        var wrapperCandidates = [
                            '.slider-background', '.slider-wrapper', '.slider-track',
                            '.slider-container', '.drag-track', '.slider-bar',
                            '.slide-verify-slider', '.slider',
                            '[class*=""slider"" i][class*=""background"" i]',
                            '[class*=""slider"" i][class*=""track"" i]'
                        ];
                        var handleCandidates = [
                            'div.slider-button', '.slider-btn', '.slide-button', '.slide-btn',
                            '.drag-button', '.slider-handle', '.handler', '.handle',
                            '.slider-block', '[class*=""slider"" i][class*=""button"" i]'
                        ];

                        var bg = q(bgCandidates);
                        var piece = q(pieceCandidates);
                        var wrapper = q(wrapperCandidates);
                        var handle = q(handleCandidates);

                        if (!bg) {
                            var imgs = Array.prototype.slice.call(document.querySelectorAll('img, canvas'))
                                .filter(visible)
                                .map(function(el, idx){
                                    var r = el.getBoundingClientRect();
                                    return { el: el, idx: idx, r: r, score: r.width * r.height };
                                }).sort(function(a,b){ return (b.score - a.score) || (a.idx - b.idx); });
                            var big = null;
                            for (var i=0;i<imgs.length;i++){
                                if (imgs[i].r.width >= 120 && imgs[i].r.height >= 60) { big = imgs[i]; break; }
                            }
                            if (big) bg = big.el;
                        }

                        function findBetterHandle() {
                            try {
                                var br = bg ? bg.getBoundingClientRect() : null;
                                var wr = wrapper ? wrapper.getBoundingClientRect() : null;
                                var nodes = Array.prototype.slice.call(
                                    document.querySelectorAll('button, div, span, i, a')).filter(visible);
                                var scored = nodes.map(function(el, idx){
                                    var r = el.getBoundingClientRect();
                                    var meta = norm([el.id, el.className, el.innerText,
                                        el.textContent, el.title].join(' '));
                                    var score = 0;
                                    if (/>>|››|»/.test(meta)) score += 2400;
                                    if (/slider|slide|drag|verify|handler|handle|move|button|arrow/.test(meta)) score += 900;
                                    if (r.width >= 24 && r.width <= 90 && r.height >= 24 && r.height <= 80) score += 700;
                                    if (wr) {
                                        var cy = r.top + r.height / 2;
                                        var wcy = wr.top + wr.height / 2;
                                        if (Math.abs(cy - wcy) <= Math.max(35, wr.height)) score += 450;
                                        if (r.right >= wr.left - 90 && r.left <= wr.left + 90) score += 750;
                                    }
                                    if (br) {
                                        if (r.top >= br.bottom - 35 && r.top <= br.bottom + 140) score += 450;
                                    }
                                    if (r.width > 120) score -= 1200;
                                    return { el: el, idx: idx, score: score };
                                }).filter(function(x){ return x.score > 0; })
                                  .sort(function(a,b){ return (b.score - a.score) || (a.idx - b.idx); });
                                return scored.length ? scored[0].el : null;
                            } catch(e) { return null; }
                        }

                        function looksLikeTrack(el) {
                            try {
                                if (!el) return false;
                                var r = el.getBoundingClientRect();
                                var meta = norm([el.id, el.className].join(' '));
                                return r.width > 110 || /background|track|wrapper/.test(meta);
                            } catch(e) { return false; }
                        }

                        if (!handle || looksLikeTrack(handle)) {
                            var bh = findBetterHandle();
                            if (bh) handle = bh;
                        }

                        var handleIsTrack = false;
                        if (!handle && wrapper) { handle = wrapper; handleIsTrack = true; }

                        if (!bg || !handle) {
                            return {
                                ok: false,
                                reason: 'Không tìm thấy ảnh nền hoặc nút kéo',
                                hasBg: !!bg, hasPiece: !!piece,
                                hasHandle: !!handle, hasWrapper: !!wrapper
                            };
                        }

                        bg.setAttribute('data-vt-bg', '1');
                        if (piece) piece.setAttribute('data-vt-piece', '1');
                        handle.setAttribute('data-vt-handle', '1');
                        if (wrapper) wrapper.setAttribute('data-vt-wrapper', '1');

                        return {
                            ok: true,
                            bgSelector: '[data-vt-bg=""1""]',
                            pieceSelector: piece ? '[data-vt-piece=""1""]' : null,
                            handleSelector: '[data-vt-handle=""1""]',
                            wrapperSelector: wrapper ? '[data-vt-wrapper=""1""]' : null,
                            bgSrc: bg.src || bg.currentSrc || '',
                            pieceSrc: piece ? (piece.src || piece.currentSrc || '') : '',
                            handleIsTrack: handleIsTrack
                        };
                    })();
                ");

                var dict = result as Dictionary<string, object>;
                if (dict == null) return null;

                return new SliderMarkInfo
                {
                    Ok = dict.ContainsKey("ok") && dict["ok"] is bool && (bool)dict["ok"],
                    Reason = dict.ContainsKey("reason") ? (dict["reason"] as string) : null,
                    BgSelector = dict.ContainsKey("bgSelector") ? (dict["bgSelector"] as string) : null,
                    PieceSelector = dict.ContainsKey("pieceSelector") ? (dict["pieceSelector"] as string) : null,
                    HandleSelector = dict.ContainsKey("handleSelector") ? (dict["handleSelector"] as string) : null,
                    WrapperSelector = dict.ContainsKey("wrapperSelector") ? (dict["wrapperSelector"] as string) : null,
                    BgSrc = dict.ContainsKey("bgSrc") ? (dict["bgSrc"] as string) : null,
                    PieceSrc = dict.ContainsKey("pieceSrc") ? (dict["pieceSrc"] as string) : null,
                    HandleIsTrack = dict.ContainsKey("handleIsTrack") && dict["handleIsTrack"] is bool && (bool)dict["handleIsTrack"]
                };
            }
            catch (Exception ex)
            {
                _log("⚠️ MarkSliderElements: " + ex.Message);
                return null;
            }
        }

        // ==================================================================
        // TÌM GAP TRÊN BACKGROUND (adaptive + cứng, hợp nhất)
        // ==================================================================
        public GapResult GetGapFromBackgroundBytes(byte[] bgBytes)
        {
            using (var img = Cv2.ImDecode(bgBytes, ImreadModes.Unchanged))
            {
                if (img == null || img.Empty())
                    throw new InvalidOperationException("Decode fail");

                Mat bgr = img;
                if (img.Channels() == 4)
                {
                    bgr = new Mat();
                    Cv2.CvtColor(img, bgr, ColorConversionCodes.BGRA2BGR);
                }

                Mat gray = new Mat();
                if (bgr.Channels() == 3) Cv2.CvtColor(bgr, gray, ColorConversionCodes.BGR2GRAY);
                else gray = bgr.Clone();

                try
                {
                    int wImg = gray.Cols, hImg = gray.Rows;
                    var candidates = new List<GapCandidate>();

                    foreach (int th in new[] { 8, 12, 18, 25, 32, 40, 50 })
                        CollectFromThreshold(gray, th, wImg, hImg, candidates);

                    int adaptiveThr = ComputeAdaptiveThreshold(gray, 0.03);
                    CollectFromThreshold(gray, adaptiveThr, wImg, hImg, candidates);

                    if (candidates.Count == 0)
                        throw new InvalidOperationException("Không tìm thấy ô đen/lỗ ghép");

                    int expectedSize = (int)(wImg * 0.13);
                    foreach (var c in candidates)
                        c.FinalScore = ScoreCandidate(c, wImg, hImg, expectedSize, gray);

                    var best = candidates.OrderByDescending(c => c.FinalScore).First();
                    return new GapResult
                    {
                        GapX = best.Cx,
                        Method = "dark_gap_adaptive",
                        Score = best.FinalScore
                    };
                }
                finally
                {
                    if (bgr != img) { try { bgr.Dispose(); } catch { } }
                    try { gray.Dispose(); } catch { }
                }
            }
        }

        private static void CollectFromThreshold(Mat gray, int th, int wImg, int hImg,
            List<GapCandidate> outList)
        {
            using (var mask = new Mat())
            {
                Cv2.Threshold(gray, mask, th, 255, ThresholdTypes.BinaryInv);
                try
                {
                    using (var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new CvSize(3, 3)))
                        Cv2.MorphologyEx(mask, mask, MorphTypes.Close, kernel);
                }
                catch { }

                using (var labels = new Mat())
                using (var stats = new Mat())
                using (var cents = new Mat())
                {
                    int n = Cv2.ConnectedComponentsWithStats(mask, labels, stats, cents,
                        PixelConnectivity.Connectivity8);

                    for (int i = 1; i < n; i++)
                    {
                        int x = stats.At<int>(i, 0);
                        int y = stats.At<int>(i, 1);
                        int w = stats.At<int>(i, 2);
                        int h = stats.At<int>(i, 3);
                        int area = stats.At<int>(i, 4);

                        if (area < 250) continue;
                        if (w < 18 || h < 18) continue;
                        if (w > Math.Max(120, (int)(wImg * 0.55))) continue;
                        if (h > Math.Max(120, (int)(hImg * 0.80))) continue;

                        double ratio = (double)w / Math.Max(1, h);
                        if (ratio < 0.55 || ratio > 1.85) continue;

                        double rect = (double)area / Math.Max(1, w * h);
                        if (rect < 0.45) continue;

                        outList.Add(new GapCandidate
                        {
                            X = x,
                            Y = y,
                            W = w,
                            H = h,
                            Area = area,
                            Rect = rect,
                            Ratio = ratio,
                            Threshold = th,
                            Cx = x + w / 2,
                            Cy = y + h / 2
                        });
                    }
                }
            }
        }

        private static int ComputeAdaptiveThreshold(Mat gray, double percentile)
        {
            using (var hist = new Mat())
            {
                Cv2.CalcHist(new[] { gray }, new[] { 0 }, null, hist, 1, new[] { 256 },
                    new[] { new Rangef(0, 256) });
                long total = (long)gray.Rows * gray.Cols;
                long target = (long)(total * percentile);
                long cum = 0;
                for (int i = 0; i < 256; i++)
                {
                    cum += (long)hist.At<float>(i);
                    if (cum >= target) return Math.Max(15, Math.Min(i, 60));
                }
            }
            return 25;
        }

        private static double ScoreCandidate(GapCandidate c, int wImg, int hImg,
            int expectedSize, Mat gray)
        {
            if (c.Ratio < 0.55 || c.Ratio > 1.85) return double.MinValue;
            if (c.Cx < wImg * 0.05 || c.Cx > wImg * 0.95) return double.MinValue;
            if (c.Cy < hImg * 0.05 || c.Cy > hImg * 0.95) return double.MinValue;

            double dyFromCenter = Math.Abs(c.Cy - hImg / 2.0) / hImg;
            if (dyFromCenter > 0.40) return double.MinValue;

            double avgGray;
            using (var sub = new Mat(gray, new CvRect(c.X, c.Y, c.W, c.H)))
                avgGray = Cv2.Mean(sub).Val0;
            if (avgGray > 50) return double.MinValue;

            double sizeScore = 1.0 - Math.Abs(c.W - expectedSize) / (double)Math.Max(1, expectedSize);
            double sizeScoreH = 1.0 - Math.Abs(c.H - expectedSize) / (double)Math.Max(1, expectedSize);
            double centerYScore = 1.0 - dyFromCenter;
            double densityScore = c.Rect;
            double darknessScore = 1.0 - avgGray / 60.0;
            double posXScore = 1.0 - Math.Abs(c.Cx - wImg * 0.5) / (wImg * 0.5);

            int edgePenalty = 0;
            if (c.X <= 2 || c.Y <= 2 || c.X + c.W >= wImg - 2 || c.Y + c.H >= hImg - 2)
                edgePenalty += 5;

            return sizeScore * 3.0
                 + sizeScoreH * 1.0
                 + centerYScore * 2.0
                 + densityScore * 2.5
                 + darknessScore * 2.0
                 + posXScore * 1.0
                 - edgePenalty;
        }

        // ==================================================================
        // MATCH TEMPLATE piece → bg
        // ==================================================================
        public GapResult GetGapFromBytes(byte[] bgBytes, byte[] pieceBytes)
        {
            using (var bg = Cv2.ImDecode(bgBytes, ImreadModes.Unchanged))
            using (var piece = Cv2.ImDecode(pieceBytes, ImreadModes.Unchanged))
            {
                if (bg == null || bg.Empty() || piece == null || piece.Empty())
                    throw new InvalidOperationException("Decode fail");

                Mat bgGray, piGray, mask;
                NormalizeForMatch(bg, out bgGray, out var _unused1);
                NormalizeForMatch(piece, out piGray, out mask);

                try
                {
                    if (piGray.Rows >= bgGray.Rows || piGray.Cols >= bgGray.Cols)
                        throw new InvalidOperationException("Piece lớn hơn background");

                    using (var bgEdge = new Mat())
                    using (var piEdge = new Mat())
                    {
                        Cv2.Canny(bgGray, bgEdge, 60, 160);
                        Cv2.Canny(piGray, piEdge, 60, 160);

                        double best = double.NegativeInfinity;
                        int bestX = 0;
                        string bestName = "";

                        Action<Mat, Mat, TemplateMatchModes, string, Mat> TryMatch = (a, b, m, name, mk) =>
                        {
                            try
                            {
                                using (var res = new Mat())
                                {
                                    if (mk != null) Cv2.MatchTemplate(a, b, res, m, mk);
                                    else Cv2.MatchTemplate(a, b, res, m);
                                    CvPoint ml;
                                    double mv;
                                    Cv2.MinMaxLoc(res, out _, out mv, out _, out ml);
                                    if (mv > best) { best = mv; bestX = ml.X; bestName = name; }
                                }
                            }
                            catch { }
                        };

                        if (mask != null && mask.Size() == piGray.Size())
                            TryMatch(bgGray, piGray, TemplateMatchModes.CCoeffNormed, "gray_mask", mask);
                        TryMatch(bgEdge, piEdge, TemplateMatchModes.CCoeffNormed, "edge", null);
                        TryMatch(bgGray, piGray, TemplateMatchModes.CCoeffNormed, "gray", null);

                        if (string.IsNullOrEmpty(bestName))
                            throw new InvalidOperationException("matchTemplate failed");

                        return new GapResult { GapX = bestX, Method = bestName, Score = best };
                    }
                }
                finally
                {
                    try { bgGray.Dispose(); } catch { }
                    try { piGray.Dispose(); } catch { }
                    try { if (mask != null) mask.Dispose(); } catch { }
                }
            }
        }

        private static void NormalizeForMatch(Mat img, out Mat gray, out Mat mask)
        {
            mask = null;
            if (img.Channels() == 4)
            {
                var ch = Cv2.Split(img);
                var alpha = ch[3];

                // ✅ SỬA: dùng Mat thay vì out
                using (var pts = new Mat())
                {
                    Cv2.FindNonZero(alpha, pts);

                    if (pts != null && !pts.Empty())
                    {
                        var r = Cv2.BoundingRect(pts);
                        int x0 = Math.Max(0, r.X - 2);
                        int y0 = Math.Max(0, r.Y - 2);
                        int x1 = Math.Min(img.Cols, r.X + r.Width + 2);
                        int y1 = Math.Min(img.Rows, r.Y + r.Height + 2);

                        var roi = new CvRect(x0, y0, x1 - x0, y1 - y0);

                        using (var cropped = new Mat(img, roi).Clone())
                        using (var alphaCropped = new Mat(alpha, roi).Clone())
                        {
                            mask = alphaCropped.Clone();   // giữ lại sau khi dispose

                            using (var bgr = new Mat())
                            {
                                Cv2.CvtColor(cropped, bgr, ColorConversionCodes.BGRA2BGR);
                                gray = new Mat();
                                Cv2.CvtColor(bgr, gray, ColorConversionCodes.BGR2GRAY);
                                Cv2.GaussianBlur(gray, gray, new CvSize(3, 3), 0);
                            }
                        }
                        return;
                    }
                }

                // Fallback: dùng toàn ảnh
                using (var bgr2 = new Mat())
                {
                    Cv2.CvtColor(img, bgr2, ColorConversionCodes.BGRA2BGR);
                    gray = new Mat();
                    Cv2.CvtColor(bgr2, gray, ColorConversionCodes.BGR2GRAY);
                    Cv2.GaussianBlur(gray, gray, new CvSize(3, 3), 0);
                }
                return;
            }

            if (img.Channels() == 3)
            {
                gray = new Mat();
                Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
                Cv2.GaussianBlur(gray, gray, new CvSize(3, 3), 0);
                return;
            }

            gray = new Mat();
            Cv2.GaussianBlur(img, gray, new CvSize(3, 3), 0);
        }

        // ==================================================================
        // KÉO CHUỘT — easing cubic + overshoot
        // ==================================================================
        public async Task DragMouseAsync(double startX, double startY, double distance)
        {
            if (_chrome != null)
            {
                try
                {
                    await DragWithCdpAsync(startX, startY, distance);
                    return;
                }
                catch (Exception ex)
                {
                    _log("⚠️ CDP drag lỗi: " + ex.Message + " — fallback JS");
                }
            }
            await DragWithJsAsync(startX, startY, distance);
        }

        private async Task DragWithCdpAsync(double startX, double startY, double distance)
        {
            var cd = _chrome;
            int sx = (int)startX, sy = (int)startY;

            cd.ExecuteCdpCommand("Input.dispatchMouseEvent", new Dictionary<string, object>
            {
                { "type", "mousePressed" },
                { "x", sx }, { "y", sy },
                { "button", "left" }, { "buttons", 1 }, { "clickCount", 1 }
            });
            await Task.Delay(30);

            const int steps = 34;
            for (int i = 1; i <= steps; i++)
            {
                double t = (double)i / steps;
                double eased = 1 - (1 - t) * (1 - t) * (1 - t);
                double x = sx + distance * eased;
                double y = sy + (i % 2 == 1 ? 1.5 : -1.0);

                cd.ExecuteCdpCommand("Input.dispatchMouseEvent", new Dictionary<string, object>
                {
                    { "type", "mouseMoved" },
                    { "x", (int)Math.Round(x) }, { "y", (int)Math.Round(y) },
                    { "button", "left" }, { "buttons", 1 }
                });
                await Task.Delay(10 + (i % 3) * 4);
            }

            // Overshoot +2px rồi lùi
            cd.ExecuteCdpCommand("Input.dispatchMouseEvent", new Dictionary<string, object>
            {
                { "type", "mouseMoved" },
                { "x", (int)(sx + distance + 2) }, { "y", sy },
                { "button", "left" }, { "buttons", 1 }
            });
            await Task.Delay(60);

            cd.ExecuteCdpCommand("Input.dispatchMouseEvent", new Dictionary<string, object>
            {
                { "type", "mouseMoved" },
                { "x", (int)(sx + distance) }, { "y", sy },
                { "button", "left" }, { "buttons", 1 }
            });
            await Task.Delay(120);

            cd.ExecuteCdpCommand("Input.dispatchMouseEvent", new Dictionary<string, object>
            {
                { "type", "mouseReleased" },
                { "x", (int)(sx + distance) }, { "y", sy },
                { "button", "left" }, { "buttons", 0 }, { "clickCount", 1 }
            });
        }

        private async Task DragWithJsAsync(double startX, double startY, double distance)
        {
            var js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript(@"
                (function(sx, sy, dist) {
                    var target = document.elementFromPoint(sx, sy);
                    if (!target) return;
                    var steps = 25;
                    var step = dist / steps;
                    function fire(el, type, x, y, buttons) {
                        var ev = new MouseEvent(type, {
                            bubbles: true, cancelable: true, view: window,
                            clientX: x, clientY: y, button: 0, buttons: buttons
                        });
                        el.dispatchEvent(ev);
                    }
                    fire(target, 'mousedown', sx, sy, 1);
                    var cx = sx;
                    for (var i = 0; i < steps; i++) {
                        cx += step;
                        var cy = sy + (Math.random() * 2 - 1);
                        fire(document, 'mousemove', cx, cy, 1);
                    }
                    fire(document, 'mouseup', cx, sy, 0);
                })(arguments[0], arguments[1], arguments[2]);
            ", (int)startX, (int)startY, (int)distance);
            await Task.Delay(300);
        }

        // ==================================================================
        // ENSURE PUZZLE VỀ 0
        // ==================================================================
        public async Task EnsurePuzzleAtStartAsync()
        {
            try
            {
                var js = (IJavaScriptExecutor)_driver;
                js.ExecuteScript(@"
                    (function() {
                        var pcSels = ['img.puzzle-piece', '[data-vt-piece=""1""]'];
                        for (var i=0;i<pcSels.length;i++) {
                            var list = document.querySelectorAll(pcSels[i]);
                            for (var j=0;j<list.length;j++) {
                                try { list[j].style.left = '0px'; } catch(e) {}
                            }
                        }
                        var hSels = ['div.slider-button', '.slider-btn', '[data-vt-handle=""1""]'];
                        for (var k=0;k<hSels.length;k++) {
                            var hl = document.querySelectorAll(hSels[k]);
                            for (var m=0;m<hl.length;m++) {
                                try { hl[m].style.left = '0px'; } catch(e) {}
                            }
                        }
                        var prs = document.querySelectorAll('div.slider-progress');
                        for (var n=0;n<prs.length;n++) {
                            try { prs[n].style.width = '0px'; } catch(e) {}
                        }
                    })();
                ");
            }
            catch (Exception ex)
            {
                _log("⚠️ EnsurePuzzleAtStart: " + ex.Message);
            }
            await Task.Delay(300);
        }

        // ==================================================================
        // HELPERS
        // ==================================================================
        private byte[] ScreenshotElement(string selector)
        {
            try
            {
                var el = _driver.FindElement(By.CssSelector(selector));
                var takes = el as ITakesScreenshot;
                if (takes != null) return takes.GetScreenshot().AsByteArray;

                // Fallback: chụp full page rồi crop
                var full = ((ITakesScreenshot)_driver).GetScreenshot().AsByteArray;
                using (var ms = new MemoryStream(full))
                using (var bmp = new Bitmap(ms))
                {
                    var loc = el.Location;
                    var size = el.Size;
                    int x = Math.Max(0, loc.X);
                    int y = Math.Max(0, loc.Y);
                    int w = Math.Min(size.Width, bmp.Width - x);
                    int h = Math.Min(size.Height, bmp.Height - y);
                    if (w <= 0 || h <= 0) return null;

                    using (var crop = new Bitmap(w, h))
                    using (var g = Graphics.FromImage(crop))
                    {
                        g.DrawImage(bmp, new Rectangle(0, 0, w, h), new Rectangle(x, y, w, h), GraphicsUnit.Pixel);
                        using (var outMs = new MemoryStream())
                        {
                            crop.Save(outMs, System.Drawing.Imaging.ImageFormat.Png);
                            return outMs.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log("⚠️ ScreenshotElement: " + ex.Message);
                return null;
            }
        }

        private byte[] UrlToBytes(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;
            if (url.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    int idx = url.IndexOf(',');
                    return idx > 0 ? Convert.FromBase64String(url.Substring(idx + 1)) : null;
                }
                catch { return null; }
            }
            try
            {
                using (var http = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(10) })
                    return http.GetByteArrayAsync(url).GetAwaiter().GetResult();
            }
            catch { return null; }
        }

        // ==================================================================
        // DTOs
        // ==================================================================
        public sealed class SliderMarkInfo
        {
            public bool Ok;
            public string Reason;
            public string BgSelector;
            public string PieceSelector;
            public string HandleSelector;
            public string WrapperSelector;
            public string BgSrc;
            public string PieceSrc;
            public bool HandleIsTrack;
        }

        public sealed class GapResult
        {
            public int GapX;
            public string Method;
            public double Score;
        }

        private sealed class GapCandidate
        {
            public int X, Y, W, H, Area, Cx, Cy, Threshold;
            public double Rect, Ratio, FinalScore;
        }
    }
}