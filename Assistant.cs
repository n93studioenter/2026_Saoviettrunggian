using DevExpress.XtraEditors;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SaovietTax
{
    public partial class Assistant : XtraForm
    {
        // ============ CẤU HÌNH ============
        private bool isExpanded = false;
        private readonly Size collapsedSize = new Size(60, 60);
        private readonly Size expandedSize = new Size(760, 640);   // to hơn để xem web cho thoáng
        private readonly int margin = 15;
        private readonly int cornerRadius = 16;

        // Padding
        private readonly int padding = 14;
        private readonly int headerHeight = 50;
        private readonly int buttonHeight = 42;
        private readonly int buttonSpacing = 10;

        // Màu
        private readonly Color colorTop = Color.FromArgb(52, 152, 219);
        private readonly Color colorBottom = Color.FromArgb(41, 128, 185);

        private const string PlaceholderText = "Nhập câu hỏi của bạn...";

        // ============ DANH SÁCH AI ============
        private readonly Dictionary<string, string> aiList = new Dictionary<string, string>()
        {
            { "ChatGPT",    "https://chat.openai.com/auth/login" },
            { "Gemini",     "https://gemini.google.com/" },
            { "Claude",     "https://claude.ai/login" },
            { "Grok",       "https://grok.com/" },
            { "Copilot",    "https://copilot.microsoft.com/" },
            { "Perplexity", "https://www.perplexity.ai/" },
        };

        // ============ CONTROL AI ============
        private DevExpress.XtraEditors.ListBoxControl lstAI;

        public Assistant()
        {
            InitializeComponent();
        }

        private void Assistant_Load(object sender, EventArgs e)
        {
            SetupForm();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            UpdateRegion();
            PositionBottomRight();
            this.Invalidate();
            this.Refresh();
        }

        // ============ SETUP ============
        private void SetupForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.BackColor = colorTop;
            this.DoubleBuffered = true;

            this.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            this.LookAndFeel.UseDefaultLookAndFeel = false;

            // ===== NÚT CLOSE =====
            btnClose.Appearance.BackColor = Color.Transparent;
            btnClose.Appearance.Options.UseBackColor = true;
            btnClose.Appearance.ForeColor = Color.White;
            btnClose.Appearance.Options.UseForeColor = true;
            btnClose.Appearance.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            btnClose.Appearance.Options.UseFont = true;

            // ===== Ô NHẬP PROMPT =====
            richTextBox1.Font = new Font("Segoe UI", 11.5f, FontStyle.Regular);
            richTextBox1.BorderStyle = BorderStyle.None;
            richTextBox1.BackColor = Color.White;
            richTextBox1.ForeColor = Color.FromArgb(50, 50, 50);
            richTextBox1.Padding = new Padding(10, 8, 10, 8);

            richTextBox1.Text = PlaceholderText;
            richTextBox1.ForeColor = Color.Gray;
            richTextBox1.GotFocus += RichTextBox1_GotFocus;
            richTextBox1.LostFocus += RichTextBox1_LostFocus;

            // ===== NÚT THỰC THI =====
            btnThucthi.Appearance.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            btnThucthi.Appearance.Options.UseFont = true;
            btnThucthi.Appearance.BackColor = Color.White;
            btnThucthi.Appearance.Options.UseBackColor = true;
            btnThucthi.Appearance.ForeColor = Color.FromArgb(41, 128, 185);
            btnThucthi.Appearance.Options.UseForeColor = true;

            // ===== LISTBOX AI =====
            lstAI = new DevExpress.XtraEditors.ListBoxControl();
            lstAI.Font = new Font("Segoe UI", 10.5f);
            lstAI.Appearance.Font = new Font("Segoe UI", 10.5f);
            lstAI.Appearance.Options.UseFont = true;
            lstAI.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            lstAI.BackColor = Color.White;
            lstAI.ForeColor = Color.FromArgb(50, 50, 50);
            foreach (var key in aiList.Keys) lstAI.Items.Add(key);
            lstAI.SelectedIndexChanged += LstAI_SelectedIndexChanged;
            this.Controls.Add(lstAI);
            lstAI.BringToFront();

            // ===== WEBVIEW2 =====
            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.Controls.Add(webView21);
            webView21.BringToFront();

            // Kích thước ban đầu
            this.ClientSize = collapsedSize;
            this.MinimumSize = collapsedSize;

            HideContent();
            UpdateRegion();
            PositionBottomRight();

            this.Click += Assistant_Click;
            this.Paint += Assistant_Paint;
            this.Resize += (s, ev) => UpdateRegion();
        }

        // ============ VỊ TRÍ GÓC PHẢI DƯỚI ============
        private void PositionBottomRight()
        {
            var screen = Screen.FromPoint(Cursor.Position).WorkingArea;
            this.Location = new Point(
                screen.Right - this.Width - margin,
                screen.Bottom - this.Height - margin
            );
        }

        // ============ BO TRÒN ============
        private void UpdateRegion()
        {
            using (var path = new GraphicsPath())
            {
                if (isExpanded)
                {
                    int r = cornerRadius;
                    path.AddArc(0, 0, r, r, 180, 90);
                    path.AddArc(this.Width - r, 0, r, r, 270, 90);
                    path.AddArc(this.Width - r, this.Height - r, r, r, 0, 90);
                    path.AddArc(0, this.Height - r, r, r, 90, 90);
                    path.CloseFigure();
                }
                else
                {
                    path.AddEllipse(0, 0, this.Width, this.Height);
                }
                this.Region = new Region(path);
            }
        }

        // ============ MỞ / THU ============
        private void Assistant_Click(object sender, EventArgs e)
        {
            if (!isExpanded) ExpandForm();
        }

        private void ExpandForm()
        {
            isExpanded = true;
            this.ClientSize = expandedSize;
            PositionBottomRight();
            UpdateRegion();
            ShowContent();
            LayoutControls();
            btnClose.BringToFront();

            // Khởi tạo WebView2 lần đầu
            if (webView21.CoreWebView2 == null)
                InitWebView();

            this.Invalidate();
            this.Refresh();
        }

        private void CollapseForm()
        {
            isExpanded = false;
            HideContent();
            this.ClientSize = collapsedSize;
            PositionBottomRight();
            UpdateRegion();
            this.Invalidate();
            this.Refresh();
        }

        // ============ LAYOUT CONTROLS ============
        private void LayoutControls()
        {
            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;

            // Nút close ở góc phải trên
            btnClose.Location = new Point(w - btnClose.Width - 10, 10);

            // Vùng nội dung (dưới header, trên nút Thực thi)
            int contentTop = headerHeight;
            int contentBottom = h - buttonHeight - buttonSpacing * 2;
            int contentHeight = contentBottom - contentTop;
            int contentLeft = padding;
            int contentWidth = w - padding * 2;

            // RichTextBox chiếm nửa trên
            int rtbHeight = contentHeight / 2 - buttonSpacing / 2;
            richTextBox1.Location = new Point(contentLeft, contentTop);
            richTextBox1.Size = new Size(contentWidth, rtbHeight);

            // ListBox AI bên trái, nửa dưới
            int aiTop = contentTop + rtbHeight + buttonSpacing;
            int aiHeight = contentHeight - rtbHeight - buttonSpacing;
            int aiWidth = 150;

            lstAI.Location = new Point(contentLeft, aiTop);
            lstAI.Size = new Size(aiWidth, aiHeight);

            // WebView2 bên phải ListBox
            webView21.Location = new Point(contentLeft + aiWidth + buttonSpacing, aiTop);
            webView21.Size = new Size(contentWidth - aiWidth - buttonSpacing, aiHeight);

            // Nút Thực thi góc phải dưới
            btnThucthi.Location = new Point(
                w - btnThucthi.Width - padding,
                h - btnThucthi.Height - padding
            );
        }

        // ============ XỬ LÝ NÚT CLOSE ============
        private void btnClose_Click(object sender, EventArgs e)
        {
            CollapseForm();
        }

        // ============ ẨN / HIỆN ============
        private void ShowContent()
        {
            richTextBox1.Visible = true;
            btnThucthi.Visible = true;
            btnClose.Visible = true;
            lstAI.Visible = true;
            webView21.Visible = true;
        }

        private void HideContent()
        {
            richTextBox1.Visible = false;
            btnThucthi.Visible = false;
            btnClose.Visible = false;
            lstAI.Visible = false;
            webView21.Visible = false;
        }

        // ============ PLACEHOLDER ============
        private void RichTextBox1_GotFocus(object sender, EventArgs e)
        {
            if (richTextBox1.Text == PlaceholderText)
            {
                richTextBox1.Text = "";
                richTextBox1.ForeColor = Color.FromArgb(50, 50, 50);
            }
        }

        private void RichTextBox1_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(richTextBox1.Text))
            {
                richTextBox1.Text = PlaceholderText;
                richTextBox1.ForeColor = Color.Gray;
            }
        }

        public string GetPrompt()
        {
            string text = richTextBox1.Text;
            if (text == PlaceholderText) return "";
            return text.Trim();
        }

        // ============ VẼ ============
        private void Assistant_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Nền gradient
            using (var brush = new LinearGradientBrush(
                this.ClientRectangle, colorTop, colorBottom, 90f))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }

            if (!isExpanded)
            {
                // Icon 💬
                using (var font = new Font("Segoe UI Emoji", 22, FontStyle.Regular))
                using (var brush = new SolidBrush(Color.White))
                using (var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    e.Graphics.DrawString("💬", font, brush,
                        new RectangleF(0, 0, this.Width, this.Height), sf);
                }
            }
            else
            {
                // Tiêu đề
                using (var font = new Font("Segoe UI", 12f, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                using (var sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center
                })
                {
                    e.Graphics.DrawString("Trợ lý AI",
                        font, brush,
                        new RectangleF(padding, 0, this.Width - 50, headerHeight),
                        sf);
                }
            }
        }

        // ============ WNDPROC ============
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HTCLIENT = 1;
            const int HTCAPTION = 2;

            base.WndProc(ref m);

            if (m.Msg == WM_NCHITTEST && isExpanded && (int)m.Result == HTCLIENT)
            {
                Point p = this.PointToClient(Cursor.Position);

                // Không kéo khi click vào web / list / richtextbox / nút close
                if (webView21 != null && webView21.Visible && webView21.Bounds.Contains(p)) return;
                if (lstAI != null && lstAI.Visible && lstAI.Bounds.Contains(p)) return;
                if (richTextBox1 != null && richTextBox1.Visible && richTextBox1.Bounds.Contains(p)) return;
                if (btnClose != null && btnClose.Visible && btnClose.Bounds.Contains(p)) return;
                if (btnThucthi != null && btnThucthi.Visible && btnThucthi.Bounds.Contains(p)) return;

                m.Result = (IntPtr)HTCAPTION;
            }
        }

        // ============ WEBVIEW2 ============
        private async void InitWebView()
        {
            if (webView21.CoreWebView2 != null) return;

            var env = await CoreWebView2Environment.CreateAsync(
                userDataFolder: System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SaovietTax_WebView2")
            );
            await webView21.EnsureCoreWebView2Async(env);

            // Chặn popup mở ra ngoài → điều hướng luôn trong web
            webView21.CoreWebView2.NewWindowRequested += (s, ev) =>
            {
                ev.Handled = true;
                webView21.CoreWebView2.Navigate(ev.Uri);
            };
        }

        private async void LstAI_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = lstAI.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selected)) return;
            if (!aiList.ContainsKey(selected)) return;

            if (webView21.CoreWebView2 == null)
            {
                await webView21.EnsureCoreWebView2Async();
                if (webView21.CoreWebView2 == null)
                {
                    InitWebView();
                    // đợi khởi tạo xong
                    await System.Threading.Tasks.Task.Delay(500);
                }
            }

            if (webView21.CoreWebView2 != null)
                webView21.CoreWebView2.Navigate(aiList[selected]);
        }

        // ============ NÚT THỰC THI ============
        private void btnThucthi_Click(object sender, EventArgs e)
        {
            string prompt = GetPrompt();
            if (string.IsNullOrEmpty(prompt))
            {
                XtraMessageBox.Show("Vui lòng nhập câu hỏi!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // TODO: xử lý prompt ở đây
            // Ví dụ: gửi lên API, hoặc nhập vào web đang mở
        }
    }
}