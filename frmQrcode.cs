using DevExpress.XtraEditors;
using DocumentFormat.OpenXml.Office2010.CustomUI;
using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaovietTax
{
    public partial class frmQrcode : DevExpress.XtraEditors.XtraForm
    {
        public frmQrcode()
        {
            InitializeComponent();
         //   this.StartPosition = FormStartPosition.Manual;

            // Đặt kích thước form (tuỳ chỉnh) 

            // Đặt form ở góc phải dưới
            //this.Location = new Point(
            //    Screen.PrimaryScreen.WorkingArea.Right - (int)(this.Width * 1.5),
            //    Screen.PrimaryScreen.WorkingArea.Top + this.Height/2
            //);
        }
        public static Bitmap GenerateQRCode(string text)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

            QRCode qrCode = new QRCode(qrCodeData);

            return qrCode.GetGraphic(10);
        }
        public static string TCVN3ToUnicode(string value)
        {
            string tcvn3 =
                "µ¸¶·¹¨¾»¼½Æ©ÇÊÈÉË®ÌÐÎÏÑªÒÕÓÔÖ×ÝØÜÞßãáâä«åèæçéêíëìîïóñòô­õøö÷ùúýûüþÿ";

            string unicode =
                "àáảãạăằắẳẵặâầấẩẫậđèéẻẽẹêềếểễệìíỉĩịòóỏõọôồốổỗộơờớởỡợùúủũụưừứửữựỳýỷỹỵ";

            for (int i = 0; i < tcvn3.Length; i++)
            {
                value = value.Replace(tcvn3[i], unicode[i]);
            }

            return value;
        }
        private void frmQrcode_Load(object sender, EventArgs e)
        {
            string dbPath = "";
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);

            // Xóa phần \bin\Debug để lấy đường dẫn gốc
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
            string filePaths = Path.Combine(rootDirectory, "Hoadon", "dpPath.txt");
            string pathThumuc = Path.Combine(rootDirectory);
            string filePath = Path.Combine(rootDirectory, "Hoadon", "invoice.txt");
            string _content = File.ReadAllText(
       filePath,
       Encoding.Default
   );
            pictureBoxQR.Image = GenerateQRCode(_content);
            var getsplit=_content.Split('|');
            labelControl1.Text= getsplit[0];    
            labelControl2.Text= Helpers.ConvertVniToUnicode(getsplit[1]);
            try
            {
                labelControl3.Text = Helpers.ConvertVniToUnicode(getsplit[2]);
            }
            catch { }
            pictureBoxQR.SizeMode = PictureBoxSizeMode.StretchImage;
        }
    }
}