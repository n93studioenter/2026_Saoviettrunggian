 
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Svg;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Tesseract;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace SaovietTax
{
    public partial class VNPTInvoice : Form
    {
        private IWebDriver driver;

        public VNPTInvoice()
        {
            InitializeComponent();
        }

        // ========================= FORM LOAD =========================
        private void VNPTInvoice_Load(object sender, EventArgs e)
        {
            string username = "";
            string password = "";

            bool result = LoginToVNPT(username, password);

            if (result)
                MessageBox.Show("Đăng nhập thành công");
            else
                MessageBox.Show("Đăng nhập thất bại");
        }

        // ========================= LOGIN =========================
        public bool LoginToVNPT(string username, string password)
        {
            try
            {
                InitializeWebDriver();

                driver.Navigate().GoToUrl(
                    "https://3501677542-tt78cadmin.vnpt-invoice.com.vn/Account/LogOn?autocomplete=off");

                Thread.Sleep(3000);

                IWebElement captchaElement =
                    driver.FindElement(By.XPath("//img[contains(@src,'Captcha')]"));

                // chụp captcha từ browser
                byte[] captchaBytes = GetCaptchaImageFromElement(captchaElement);

                // OCR
                string captcha = SolveCaptchaFromBytes(captchaBytes);

                MessageBox.Show("Captcha đọc được: " + captcha);

                if (string.IsNullOrWhiteSpace(captcha))
                    return false;

                driver.FindElement(By.Id("UserName")).SendKeys(username);

                driver.FindElement(By.Id("Password")).SendKeys(password);

                driver.FindElement(By.Id("CaptchaCode")).SendKeys(captcha);

                driver.FindElement(By.XPath("//button[@type='submit']")).Click();

                Thread.Sleep(5000);

                if (!driver.Url.Contains("LogOn"))
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }

        // ========================= CHROME =========================
        private void InitializeWebDriver()
        {
            ChromeOptions options = new ChromeOptions();

            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--disable-notifications");

            driver = new ChromeDriver(options);

            driver.Manage().Window.Maximize();
        }

        // ========================= OCR CAPTCHA =========================
        private string SolveCaptchaFromBytes(byte[] imageBytes)
        {
            try
            {
                Bitmap original;

                // ===== SVG =====
                string check = System.Text.Encoding.UTF8.GetString(imageBytes);

                if (check.Contains("<svg"))
                {
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        SvgDocument svgDoc = SvgDocument.Open<SvgDocument>(ms);

                        original = svgDoc.Draw();
                    }
                }
                else
                {
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        original = new Bitmap(ms);
                    }
                }

                original.Save("1_original.png");

                Bitmap processed = PreprocessCaptchaImage(original);

                processed.Save("2_processed.png");

                string tessPath = GetTessDataPath();

                using (var engine =
                    new TesseractEngine(tessPath, "eng", EngineMode.LstmOnly))
                {
                    engine.SetVariable(
                        "tessedit_char_whitelist",
                        "0123456789");

                    engine.DefaultPageSegMode =
                        PageSegMode.SingleLine;

                    using (var pix = PixConverter.ToPix(processed))
                    {
                        using (var page = engine.Process(pix))
                        {
                            string raw = page.GetText();

                            Console.WriteLine(raw);

                            string result = new string(
                                raw.Where(char.IsDigit).ToArray());

                            if (result.Length > 4)
                                result = result.Substring(0, 4);

                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

                return "";
            }
        }

        // ========================= PREPROCESS =========================
        private Bitmap PreprocessCaptchaImage(Bitmap original)
        {
            int scale = 6;

            Bitmap resized = new Bitmap(
                original.Width * scale,
                original.Height * scale);

            using (Graphics g = Graphics.FromImage(resized))
            {
                g.InterpolationMode =
                    InterpolationMode.HighQualityBicubic;

                g.DrawImage(
                    original,
                    0,
                    0,
                    resized.Width,
                    resized.Height);
            }

            Bitmap binary = new Bitmap(
                resized.Width,
                resized.Height);

            for (int x = 0; x < resized.Width; x++)
            {
                for (int y = 0; y < resized.Height; y++)
                {
                    Color c = resized.GetPixel(x, y);

                    int gray = (c.R + c.G + c.B) / 3;

                    if (gray < 150)
                        binary.SetPixel(x, y, Color.Black);
                    else
                        binary.SetPixel(x, y, Color.White);
                }
            }

            return binary;
        }

        // ========================= SCREENSHOT CAPTCHA =========================
        private byte[] GetCaptchaImageFromElement(IWebElement element)
        {
            Screenshot screenshot =
                ((ITakesScreenshot)driver).GetScreenshot();

            using (MemoryStream mem =
                new MemoryStream(screenshot.AsByteArray))
            {
                Bitmap full = new Bitmap(mem);

                Rectangle crop = new Rectangle(
                    element.Location.X,
                    element.Location.Y,
                    element.Size.Width,
                    element.Size.Height);

                Bitmap captcha =
                    full.Clone(crop, full.PixelFormat);

                captcha.Save("captcha_crop.png");

                using (MemoryStream ms = new MemoryStream())
                {
                    captcha.Save(ms, ImageFormat.Png);

                    return ms.ToArray();
                }
            }
        }

        // ========================= TESSDATA =========================
        private string GetTessDataPath()
        {
            string tess = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "tessdata");

            if (!Directory.Exists(tess))
                throw new Exception("Không tìm thấy tessdata");

            if (!File.Exists(Path.Combine(tess, "eng.traineddata")))
                throw new Exception("Thiếu eng.traineddata");

            return tess;
        }

        // ========================= CLOSE =========================
        private void VNPTInvoice_FormClosing(
            object sender,
            FormClosingEventArgs e)
        {
            try
            {
                driver?.Quit();
            }
            catch
            {
            }
        }
    }
}
 
