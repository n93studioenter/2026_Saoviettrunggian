using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using AForge.Video.DirectShow;
using AForge.Video;

namespace SaovietTax
{
	public partial class frmCamera: DevExpress.XtraEditors.XtraForm
	{
        public frmCamera()
		{
            InitializeComponent();
		}
        private FilterInfoCollection videoDevices; // Danh sách camera
        private VideoCaptureDevice videoSource; // Camera hiện tại
        private void frmCamera_Load(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage; // Hoặc PictureBoxSizeMode.Zoom

            // Chọn camera đầu tiên
            // Lấy danh sách camera
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
            {
                MessageBox.Show("Không tìm thấy camera.");
                return;
            }

            // Chọn camera đầu tiên
            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
            videoSource.Start(); // Bắt đầu camera
        }
        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (pictureBox1.SizeMode == PictureBoxSizeMode.StretchImage || pictureBox1.SizeMode == PictureBoxSizeMode.Zoom)
            {
                pictureBox1.Image?.Dispose(); // Giải phóng hình ảnh cũ
                pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone(); // Cập nhật hình ảnh mới
            }
        }

        private void frmCamera_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Dừng camera khi đóng form
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
        }
    }
}