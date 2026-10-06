using DevExpress.XtraEditors;
using System;
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
        private readonly Size expandedSize = new Size(460, 500);   // to hơn chút cho thoáng
        private readonly int margin = 15;
        private readonly int cornerRadius = 16;

        // Padding
        private readonly int padding = 14;           // lề ngoài
        private readonly int headerHeight = 50;      // chiều cao thanh tiêu đề
        private readonly int buttonHeight = 42;      // chiều cao nút Thực thi
        private readonly int buttonSpacing = 10;     // khoảng cách giữa các thành phần

        // Màu
        private readonly Color colorTop = Color.FromArgb(52, 152, 219);
        private readonly Color colorBottom = Color.FromArgb(41, 128, 185);

        private const string PlaceholderText = "Nhập câu hỏi của bạn...";

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
            // Padding bên trong RichTextBox
            richTextBox1.Padding = new Padding(10, 8, 10, 8);

            // Placeholder
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
            LayoutControls();          // ← Căn vị trí + padding
            btnClose.BringToFront();
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

        // ============ LAYOUT CONTROLS (có padding) ============
        private void LayoutControls()
        {
            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;

            // Nút close ở góc phải trên, cách lề 10px
            btnClose.Location = new Point(w - btnClose.Width - 10, 10);

            // RichTextBox: 
            //   - trên: headerHeight (50)
            //   - dưới: chừa chỗ cho nút Thực thi (buttonHeight + buttonSpacing)
            //   - trái/phải: padding (14)
            int rtbTop = headerHeight;
            int rtbBottom = h - buttonHeight - buttonSpacing * 2;
            richTextBox1.Location = new Point(padding, rtbTop);
            richTextBox1.Size = new Size(
                w - padding * 2,
                rtbBottom - rtbTop
            );

            // Nút Thực thi: góc phải dưới, cách lề padding
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
        }

        private void HideContent()
        {
            richTextBox1.Visible = false;
            btnThucthi.Visible = false;
            btnClose.Visible = false;
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
                // Tiêu đề — canh giữa theo chiều dọc của header
                using (var font = new Font("Segoe UI", 12f, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                using (var sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center
                })
                {
                    e.Graphics.DrawString("Trợ lý",
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
                Point clientPoint = this.PointToClient(Cursor.Position);
                Rectangle closeArea = new Rectangle(
                    btnClose.Left - 5, btnClose.Top - 5,
                    btnClose.Width + 10, btnClose.Height + 10);

                if (closeArea.Contains(clientPoint))
                    return;

                m.Result = (IntPtr)HTCAPTION;
            }
        }

        private void btnThucthi_Click(object sender, EventArgs e)
        {

        }
    }
}