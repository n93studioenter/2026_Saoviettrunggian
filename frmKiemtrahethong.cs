using ClosedXML.Excel;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraVerticalGrid.Native;
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
using static SaovietTax.frmKhachhang;
using static SaovietTax.frmMain;

namespace SaovietTax
{
    public partial class frmKiemtrahethong : DevExpress.XtraEditors.XtraForm
    {
        public frmKiemtrahethong()
        {
            InitializeComponent();
        }
        public frmMain frmMain;
        private bool Is5111=false;
        public class KTHeThong
        {
            public int STT { get; set; }
            public int Type { get; set; } //1 ,2 ,3, 4, 5
            public string SoHD { get; set; }
            public string KHHD { get; set; }
            public DateTime NgayLap { get; set; }
            public DateTime NgayImport { get; set; }    
            public string MST { get; set; } 
            public string TenKH { get; set; }
            public int IsHD { get; set; }   
            public int IsImport { get; set; }   
            public int IsChild { get; set; }
            public double TienTrcThue { get; set; }
            public double TienThue { get; set; }
            public double TongTienTT { get; set; }
            public int TypeMisstake1 { get; set; } = 0;
            public int TypeMisstake2 { get; set; } = 0;
            public int TypeMisstake3 { get; set; } = 0;
        }
        public List<KTHeThong> lstKiemTraHeThong = new List<KTHeThong>();
        private void frmKiemtrahethong_Load(object sender, EventArgs e)
        {
            string[] months = new string[]
        {
            "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5",
            "Tháng 6", "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10",
            "Tháng 11", "Tháng 12"
        };

            cbbChonthang.Properties.Items.AddRange(months);

            // Gán giá trị mặc định bằng tháng hiện tại
            int currentMonth = DateTime.Now.Month;
            cbbChonthang.SelectedIndex = currentMonth - 1; // Tháng hiện tại (0-indexed)
            LoadData(currentMonth ,1);

            cbbTrangThai.Properties.Items.Add("Tất cả");
            cbbTrangThai.Properties.Items.Add("Đã nhập");
            cbbTrangThai.Properties.Items.Add("Chưa nhập");

            RepositoryItemButtonEdit buttonEdit = new RepositoryItemButtonEdit();
            buttonEdit.Buttons[0].Kind = DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph;
            buttonEdit.Buttons[0].Caption = "Tải hoá đơn";
            buttonEdit.ButtonClick += ButtonEdit_ButtonClick;

            gridView1.Columns["TaiHD"].ColumnEdit = buttonEdit;
        }
        private void ButtonEdit_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            // Xử lý sự kiện khi nút "Xóa" được nhấn
            // Ví dụ: Xóa hàng hiện tại trong GridView
            int focusedRowHandle = gridView1.FocusedRowHandle;
            if (focusedRowHandle >= 0)
            {
                gridView1.DeleteRow(focusedRowHandle);
            }
        }
        DataTable tbHoaDon { get; set; }
        DataTable tbImport { get; set; }
        DataTable tbImportDetail { get; set; }
        DataTable tbChungtu { get; set; }
        public void LoadData(int month,int type)
        {
            stt = 1;
            lstKiemTraHeThong = new List<KTHeThong>(); 
             var query = "SELECT * FROM HoaDon"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHang
            tbHoaDon = frmMain.ExecuteQuery(query);

            query = "SELECT * FROM tbimport"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHang
            tbImport = frmMain.ExecuteQuery(query);

            query = "SELECT * FROM tbimportdetail"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHang
            tbImportDetail = frmMain.ExecuteQuery(query);

            query = "SELECT * FROM ChungTu"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHang
            tbChungtu = frmMain.ExecuteQuery(query);

            string pathType = "";
            pathType = type == 1 ? "HDVao" : "HDRa";
            string directoryPath = $"{frmMain.savedPath}\\{pathType}\\{month}";

            string fileName = "HDDienTuDaCapMa.xlsx"; // Hoặc .xls nếu định dạng khác
            string filePath = Path.Combine(directoryPath, fileName);
            Thucthi(filePath,1);

            string fileNam2 = "HDDienTuKhongMa.xlsx"; // Hoặc .xls nếu định dạng khác
            string filePath2 = Path.Combine(directoryPath, fileNam2);
            Thucthi(filePath2,1);

            string fileName3 = "HDDienTuMayTinhTien.xlsx"; // Hoặc .xls nếu định dạng khác
            string filePath3 = Path.Combine(directoryPath, fileName3);
            Thucthi(filePath3,2);


            gridControl1.DataSource = lstKiemTraHeThong;
            gridControl1.RefreshDataSource();   
        }
        public void LoadData2(int month, int type)
        {
            stt = 1;
            lstKiemTraHeThong = new List<KTHeThong>();
            var query = "SELECT * FROM HoaDon"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHang
            tbHoaDon = frmMain.ExecuteQuery(query);

            query = "SELECT * FROM tbimport"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHang
            tbImport = frmMain.ExecuteQuery(query);

            string pathType = "";
            pathType = type == 1 ? "HDVao" : "HDRa";
            string directoryPath = $"{frmMain.savedPath}\\{pathType}\\{month}";
            string fileName = "Hoadondientu.xlsx"; // Hoặc .xls nếu định dạng khác

            string filePath = Path.Combine(directoryPath, fileName);

            Thucthi(filePath, 1);

            string fileName3 = "HDDienTuMayTinhTien.xlsx"; // Hoặc .xls nếu định dạng khác

            string filePath3 = Path.Combine(directoryPath, fileName3);
            Thucthi(filePath3,2);
            gridControl1.DataSource = lstKiemTraHeThong;
            gridControl1.RefreshDataSource();
        }
        int stt = 1;
        bool isfirstload = true;
        public void Thucthi(string filePath,int type)
        {
           
            if (File.Exists(filePath))
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1); // Lấy worksheet đầu tiên
                   
                    foreach (var row in worksheet.RowsUsed().Skip(3)) // 6 vì chỉ số dòng bắt đầu từ 0
                    {
                        KTHeThong KTHeThong = new KTHeThong();
                        KTHeThong.STT = stt;
                        KTHeThong.KHHD = row.Cell(3).GetString();
                        KTHeThong.SoHD = row.Cell(4).GetString();
                        try
                        {
                            if (type == 1)
                            {
                                KTHeThong.TienTrcThue = double.Parse(row.Cell(11).GetString());
                                KTHeThong.TienThue = double.Parse(row.Cell(12).GetString());
                                KTHeThong.TongTienTT = double.Parse(row.Cell(15).GetString());
                            }
                            else
                            {
                                KTHeThong.TienTrcThue = double.TryParse(row.Cell(12).GetString(), out var tienTrcThue) ? tienTrcThue : 0;
                                KTHeThong.TienThue = double.TryParse(row.Cell(13).GetString(), out var tienThue) ? tienThue : 0;
                                KTHeThong.TongTienTT = double.TryParse(row.Cell(15).GetString(), out var tongTienTT) ? tongTienTT : 0;
                            }
                        }
                        catch(Exception ex)
                        {

                        }

                            DateTime Nl = DateTime.MinValue;
                        try
                        {
                            DateTime.TryParse(row.Cell(5).GetString(), out Nl);
                        }
                        catch (Exception ex)
                        {

                        }
                        if (Nl != DateTime.MinValue)
                            KTHeThong.NgayLap = Nl;
                        KTHeThong.MST = row.Cell(6).GetString();
                        KTHeThong.TenKH = row.Cell(7).GetString();
                        //Lấy trạng thái
                        //Kiểm tra trong hoá đơn trước
                        var getHD2 = tbHoaDon.AsEnumerable()
                            .Where(m => m["SoHD"].ToString() == KTHeThong.SoHD
                             && m["KyHieu"].ToString() == KTHeThong.KHHD
                             && DateTime.Parse(m["NgayPH"].ToString()).ToString("dd/MM/yyyy") == KTHeThong.NgayLap.ToString("dd/MM/yyyy")).FirstOrDefault();
                        var getHD = (from h in tbHoaDon.AsEnumerable()
                                     join c in tbChungtu.AsEnumerable()
                                     on h.Field<string>("SoHD") equals c.Field<string>("SoHieu")
                                     where c.Field<DateTime>("NgayCT") == KTHeThong.NgayLap
                                     && h["KyHieu"].ToString()== KTHeThong.KHHD
                                     && h["SoHD"].ToString() == KTHeThong.SoHD
                                     select h).FirstOrDefault();
                        if (getHD!=null)
                        {
                            KTHeThong.IsHD = getHD.Field<int>("MaSo");
                            //Kiểm tra các lỗi
                            //Kiểm tra tiền trước thuế
                            double Tientrcthue = getHD.Field<double>("ThanhTien");
                            if (KTHeThong.TienTrcThue != Tientrcthue)
                            {
                                KTHeThong.TypeMisstake1 = 1;
                            }
                            //Kiểm tra tiền thuế
                            //Lấy dòng tk thuế
                            double getTienthue = 0;
                            //14038
                            var getRowtax = tbChungtu.AsEnumerable().Where(m=>m.Field<string>("SoHieu")== KTHeThong.SoHD && (m.Field<int>("MaTKNo") == 5108 || m.Field<int>("MaTKCo") == 14038)  ).FirstOrDefault();
                            if (getRowtax != null)
                            {
                                 getTienthue = getRowtax.Field<double>("SoPS");
                                if(KTHeThong.TienThue!= getTienthue)
                                {
                                    KTHeThong.TypeMisstake2 = 1;
                                }
                            }
                            if(KTHeThong.TongTienTT!=(Tientrcthue+ getTienthue))
                            {
                                KTHeThong.TypeMisstake3 = 1;
                            }
                        }
                        var getImport= tbImport.AsEnumerable()
                            .Where(m => m["SHDon"].ToString() == KTHeThong.SoHD
                             && m["KHHDon"].ToString() == KTHeThong.KHHD
                             && DateTime.Parse(m["NLap"].ToString()).ToString("dd/MM/yyyy") == KTHeThong.NgayLap.ToString("dd/MM/yyyy")).FirstOrDefault();
                        if (getImport!=null)
                        {
                            KTHeThong.IsImport = getImport.Field<int>("ID");
                        }
                        stt += 1;
                        lstKiemTraHeThong.Add(KTHeThong);
                    }

                }
            }
            else
            {
                Console.WriteLine("File not found.");
            }
        }
        private void gridView1_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            int rowHandle = e.RowHandle;
            var gridView = sender as GridView;

            KTHeThong rowData = gridView.GetRow(rowHandle) as KTHeThong;

            if (e.Column.FieldName == "TienTrcThue")
            {
                if (rowData.TypeMisstake1 == 1)
                {
                    e.Appearance.ForeColor = Color.Red; // Tô màu chữ đỏ
                }
            }
            if (e.Column.FieldName == "TienThue")
            {
                if (rowData.TypeMisstake2 == 1)
                {
                    e.Appearance.ForeColor = Color.Red; // Tô màu chữ đỏ
                }
            }
            if (e.Column.FieldName == "TongTienTT")
            {
                if (rowData.TypeMisstake3 == 1)
                {
                    e.Appearance.ForeColor = Color.Red; // Tô màu chữ đỏ
                }
            }
            if (e.Column.FieldName == "IsImport")
            {
                var getvalue = e.CellValue.ToString();
                if(int.Parse(getvalue) !=0)
                {
                    //Kiểm tra xem có chi tiết hay không
                    var checkDetail=tbImportDetail.AsEnumerable().Where(m=>m.Field<string>("ParentId")==getvalue.ToString()).Count();
                    if (checkDetail > 0)
                    {
                        e.Appearance.BackColor = Color.Blue; // Tô màu chữ đỏ
                    }
                    else
                    {
                        e.Appearance.BackColor = Color.Red; // Tô màu chữ đỏ
                    }
                    e.Appearance.ForeColor = Color.White;
                  //  e.DisplayText = string.Empty; // Không hiển thị văn bản
                }
                else
                {
                    e.Appearance.BackColor = Color.White; // Tô màu chữ đỏ
                   // e.DisplayText = string.Empty; // Không hiển thị văn bản
                }

            }
            if (e.Column.FieldName == "IsHD")
            {
                var getvalue = e.CellValue.ToString();
                if (int.Parse(getvalue)!=0)
                {
                    e.Appearance.BackColor = Color.DarkGreen; // Tô màu chữ đỏ
                 //   e.DisplayText = string.Empty; // Không hiển thị văn bản
                }
                else
                {
                    e.Appearance.BackColor = Color.White; // Tô màu chữ đỏ
                  //  e.DisplayText = string.Empty; // Không hiển thị văn bản
                }
                e.Appearance.ForeColor = Color.White;
            }
        }
        private void LoadControl()
        {

            int currentMonth = int.Parse(cbbChonthang.Text.Replace("Tháng", ""));
            if (radDauvao.Checked)
                LoadData(currentMonth, 1);
            else
                LoadData2(currentMonth, 2);

        }
        private void cbbChonthang_SelectedIndexChanged(object sender, EventArgs e)
        {
            stt = 1;
            LoadControl();
        }

        private void radDauvao_CheckedChanged(object sender, EventArgs e)
        {
            stt = 1;
            LoadControl();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {

        }
    }
}