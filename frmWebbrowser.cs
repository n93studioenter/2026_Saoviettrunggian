using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaovietTax
{
    public partial class frmWebbrowser: Form
    {
        public frmWebbrowser()
        {
            InitializeComponent();
        }

       public string filep { get; set; }
        private async void simpleButton1_Click(object sender, EventArgs e)
        {
    //        // 1. Lấy kích thước thật (chuẩn hơn dùng documentElement)
    //        string result = await webView21.ExecuteScriptAsync(@"
    //    Math.max(
    //        document.body.scrollHeight,
    //        document.documentElement.scrollHeight
    //    ) + ',' +
    //    Math.max(
    //        document.body.scrollWidth,
    //        document.documentElement.scrollWidth
    //    );
    //");

    //        result = result.Replace("\"", "");
    //        var parts = result.Split(',');

    //        int height = int.Parse(parts[0]);
    //        int width = int.Parse(parts[1]);

    //        // 2. Resize WebView2
    //        webView21.Width = width;
    //        webView21.Height = height;

    //        // 3. QUAN TRỌNG: chờ render lại
    //        await Task.Delay(800); // tăng lên cho chắc

    //        // 4. Force repaint (rất quan trọng)
    //        await webView21.ExecuteScriptAsync("window.scrollTo(0, document.body.scrollHeight)");
    //        await Task.Delay(300);

    //        // 5. Capture
    //        using (var ms = new MemoryStream())
    //        {
    //            await webView21.CoreWebView2.CapturePreviewAsync(
    //                Microsoft.Web.WebView2.Core.CoreWebView2CapturePreviewImageFormat.Png,
    //                ms
    //            );

    //            Clipboard.SetImage(Image.FromStream(ms));
    //        }

    //        MessageBox.Show("Đã copy FULL!");
        }
        private async void frmWebbrowser_Load(object sender, EventArgs e)
        {
            //await webView21.EnsureCoreWebView2Async();

            //webView21.Source = new Uri(filep);
        }
    }
}
