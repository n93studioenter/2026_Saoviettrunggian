using ClosedXML.Excel;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaovietTax
{

    #region Class baco1 cáo
    public class QChiTiet
    {
        public int STT { get; set; }
        public string DienGiai { get; set; }
        public string DVT { get; set; }
        public string MaSo { get; set; }
        public double LuongDK { get; set; }
        public double TienDK { get; set; }
        public double LuongNhap  { get; set; }
        public double TienNhap { get; set; }
        public double LuongXuat { get; set; }
        public double TienXuat { get; set; }
        public double LuongCK { get; set; }
        public double TienCK { get; set; }
        public double DonGia {  get; set; } 
    }
    #endregion
    public partial class ExportExcelBC : DevExpress.XtraEditors.XtraForm
    {
        string password, connectionString;
        public ExportExcelBC()
        {
            InitializeComponent();
        }
       private void GetConnectionString()
        {
            string dbPath = "";
            string password = "1@35^7*9)1";
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);

            // Xóa phần \bin\Debug để lấy đường dẫn gốc
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
            string filePaths = Path.Combine(rootDirectory, "Hoadon", "dpPath.txt");
            string pathThumuc = Path.Combine(rootDirectory);
            //MessageBox.Show(pathThumuc);
            try
            {
                string content = File.ReadAllText(filePaths);
                dbPath = content;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi đọc file: " + ex.Message);
            }
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";
        }
        private void ExportExcelBC_Load(object sender, EventArgs e)
        {
            GetConnectionString();
            ExportTonkho();
        }
        public System.Data.DataTable ExecuteQuery(string query, params OleDbParameter[] parameters)
        {
            System.Data.DataTable dataTable = new System.Data.DataTable();


            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        // Thêm các tham số vào command
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(command))
                        {
                            dataAdapter.Fill(dataTable);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }

            return dataTable; // Trả về DataTable chứa dữ liệu
        }
        List<QChiTiet> qChiTiets = new List<QChiTiet>();    
        private void ExportTonkho()
        {
            string query = "SELECT * FROM QChitiet";
            var kq = ExecuteQuery(query, null);
            int stt = 1;
            foreach(DataRow row in kq.Rows)
            {
                QChiTiet QChiTiet=new QChiTiet();
                QChiTiet.STT = stt;
                QChiTiet.DienGiai = row["TenVattu"].ToString();
                QChiTiet.DVT = row["DonVi"].ToString();
                QChiTiet.MaSo= row["SoHieu"].ToString();
                QChiTiet.LuongDK = double.Parse(row["DKLuong"].ToString());
                QChiTiet.TienDK = double.Parse(row["DKTien"].ToString());
                QChiTiet.TienNhap = double.Parse(row["TienNhap"].ToString());
                QChiTiet.LuongNhap = double.Parse(row["LuongNhap"].ToString());
                QChiTiet.TienXuat = double.Parse(row["TienXuat"].ToString());
                QChiTiet.LuongXuat = double.Parse(row["LuongXuat"].ToString());
                QChiTiet.LuongCK = double.Parse(row["CKLuong"].ToString());
                QChiTiet.TienCK = double.Parse(row["CKTien"].ToString());
                try
                {
                    QChiTiet.DonGia = Math.Round(QChiTiet.TienCK / QChiTiet.LuongCK);
                }
                catch(Exception ex) 
                {
                    QChiTiet.DonGia = 0;
                }
                stt += 1;
                qChiTiets.Add(QChiTiet);    
            }
                 var lstgroup = qChiTiets
                .GroupBy(x => x.MaSo)
                .Select(g => new QChiTiet
                {
                    STT = 0, // hoặc có thể đánh số lại sau
                    DienGiai = g.First().DienGiai, // Lấy từ phần tử đầu tiên (nếu các phần tử cùng MaSo có cùng DienGiai)
                    MaSo = g.Key, 
                    DVT = g.First().DVT, // Lấy từ phần tử đầu tiên
                    LuongDK = g.Sum(x => x.LuongDK),
                    TienDK = g.Sum(x => x.TienDK),
                    TienNhap = g.Sum(x => x.TienNhap),
                    LuongNhap = g.Sum(x => x.LuongNhap),
                    TienXuat = g.Sum(x => x.TienXuat),
                    LuongXuat = g.Sum(x => x.LuongXuat),
                    LuongCK = g.Sum(x => x.LuongCK),
                    TienCK = g.Sum(x => x.TienCK),
                    DonGia=g.Sum(x => x.DonGia),    
                })
                .ToList();
            XuatRaExcelVoiClosedXML(lstgroup, @"D:\Export.xlsx");
            this.Close();
        }


        public void XuatRaExcelVoiClosedXML(List<QChiTiet> duLieu, string duongDanFile)
        {
            int stt = 1;
            try
            {
              
                // 1. Tạo workbook
                using (var tapTin = new XLWorkbook())
                {
                    
                    // 2. Thêm sheet và đặt tên là "Báo cáo"
                    var bangTinh = tapTin.Worksheets.Add("Báo cáo");

                    // 3. Ghi tiêu đề (dòng 1)
                    bangTinh.Cell(1, 1).Value = "STT";
                    bangTinh.Cell(1, 2).Value = "Mã Số";
                    bangTinh.Cell(1, 3).Value = "Diễn Giải";
                    bangTinh.Cell(1, 4).Value = "ĐVT";
                    bangTinh.Cell(1, 5).Value = "Lượng ĐK";
                    bangTinh.Cell(1, 6).Value = "Tiền ĐK";
                    bangTinh.Cell(1, 7).Value = "Lượng Nhập";
                    bangTinh.Cell(1, 8).Value = "Tiền Nhập";
                    bangTinh.Cell(1, 9).Value = "Lượng Xuất";
                    bangTinh.Cell(1, 10).Value = "Tiền Xuất";
                    bangTinh.Cell(1, 11).Value = "Lượng CK";
                    bangTinh.Cell(1, 12).Value = "Tiền CK";
                    bangTinh.Cell(1, 13).Value = "Đơn giá";
                    // Định dạng tiêu đề
                    var vungTieuDe = bangTinh.Range(1, 1, 1, 13);
                    vungTieuDe.Style.Font.Bold = true;
                    vungTieuDe.Style.Fill.BackgroundColor = XLColor.LightGray;
                    vungTieuDe.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // 4. Ghi dữ liệu
                    int dong = 2;
                 
                    foreach (var item in duLieu)
                    {
                        if (stt == 57)
                        {
                            int dsad = 10;
                        }
                        bangTinh.Cell(dong, 1).Value = stt;
                        bangTinh.Cell(dong, 2).Value = item.MaSo;
                        bangTinh.Cell(dong, 3).Value = Helpers.ConvertVniToUnicode(item.DienGiai);
                        bangTinh.Cell(dong, 4).Value = Helpers.ConvertVniToUnicode(item.DVT);
                        bangTinh.Cell(dong, 5).Value = item.LuongDK;
                        bangTinh.Cell(dong, 6).Value = item.TienDK;
                        bangTinh.Cell(dong, 7).Value = item.LuongNhap;
                        bangTinh.Cell(dong, 8).Value = item.TienNhap;
                        bangTinh.Cell(dong, 9).Value = item.LuongXuat;
                        bangTinh.Cell(dong, 10).Value = item.TienXuat;
                        bangTinh.Cell(dong, 11).Value = item.LuongCK;
                        bangTinh.Cell(dong, 12).Value = item.TienCK;
                        try
                        {
                            bangTinh.Cell(dong, 13).Value = item.DonGia;
                        }
                        catch(Exception ex)
                        {
                            bangTinh.Cell(dong, 13).Value = 0;
                        }
                        dong++;
                        stt++;
                    }

                    // 5. Chỉnh độ rộng từng cột
                    bangTinh.Column(1).Width = 5;
                    bangTinh.Column(2).Width = 15;
                    bangTinh.Column(3).Width = 35;
                    bangTinh.Column(4).Width = 10;
                    bangTinh.Column(5).Width = 12;
                    bangTinh.Column(6).Width = 15;
                    bangTinh.Column(7).Width = 12;
                    bangTinh.Column(8).Width = 15;
                    bangTinh.Column(9).Width = 12;
                    bangTinh.Column(10).Width = 15;
                    bangTinh.Column(11).Width = 12;
                    bangTinh.Column(12).Width = 15;
                    bangTinh.Column(13).Width = 15;
                    // 6. Định dạng số
                    bangTinh.Columns(5, 13).Style.NumberFormat.Format = "#,##0";

                    // 7. Kẻ khung
                    var vungBang = bangTinh.Range(1, 1, dong - 1, 13);
                    vungBang.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    vungBang.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // 8. Lưu file
                    tapTin.SaveAs(duongDanFile);
                }

                // *** 9. MỞ FILE EXCEL SAU KHI XUẤT ***
                // Kiểm tra file đã tồn tại chưa
                if (File.Exists(duongDanFile))
                {
                    // Mở file bằng ứng dụng mặc định (Excel)
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = duongDanFile,
                        UseShellExecute = true // Quan trọng: dùng shell để mở bằng ứng dụng mặc định
                    });

                    MessageBox.Show($"Xuất Excel thành công!\nĐang mở file: {duongDanFile}", "Thành công",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Lỗi: Không tìm thấy file {duongDanFile}", "Lỗi",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất Excel: {ex.Message} {stt}", "Lỗi",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}