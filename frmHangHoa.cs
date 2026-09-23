using DevExpress.Utils;
using DevExpress.Utils.Extensions;
using DevExpress.Xpo.DB.Helpers;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Mask.Design;
using DevExpress.XtraGrid.Views.Grid;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using SaovietTax.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using Windows.UI.Xaml.Controls;
using static iText.IO.Image.Jpeg2000ImageData;
using static SaovietTax.frmMain;

namespace SaovietTax
{
    public partial class frmHangHoa : DevExpress.XtraEditors.XtraForm
    {
        public int Typeform { get; set; }  
        public VatTu dtoVatTu { get; set; }
        public int hoverRowHandle { get; set; }
        public frmHangHoa()
        {
            InitializeComponent();
            dtoVatTu = new VatTu();
          

        }
        public void GridStripRow(DevExpress.XtraGrid.Views.Grid.GridView gridView)
        {
           
            if (gridView != null)
            {
                // Kích hoạt kiểu dáng hàng chẵn và lẻ
                gridView.OptionsView.EnableAppearanceEvenRow = true;
                gridView.OptionsView.EnableAppearanceOddRow = true;

                // Thiết lập màu sắc cho hàng chẵn
                gridView.Appearance.EvenRow.BackColor = System.Drawing.Color.FromArgb(168, 255, 253);

                gridView.Appearance.EvenRow.ForeColor = System.Drawing.Color.Black; // Màu chữ cho hàng chẵn

                // Thiết lập màu sắc cho hàng lẻ
                gridView.Appearance.OddRow.BackColor = System.Drawing.Color.White; // Màu nền cho hàng lẻ
                gridView.Appearance.OddRow.ForeColor = System.Drawing.Color.Black; // Màu chữ cho hàng lẻ


            }
        }
        bool isload = false;
        private void LoadData(int Maso, string keysearch)
        {
            if(Typeform==2 && !isload)
            {
                isload = true;
                Reload();
                return;
                //Rectangle screen = Screen.FromControl(this).WorkingArea;

                //int x = (int)(screen.Left + screen.Width / 3.5);
                //int y = screen.Top + (screen.Height - this.Height) / 2;

                //this.Location = new Point(x, y);
            }
                var aa = frmMain.lstvtgoiy;
            string query = "";
            if (Maso != -1)
            {
                if (Typeform == 1)
                {
                    var mylstvt= frmMain.lstvt.Where(m => m.MaPhanLoai == Maso && (string.IsNullOrEmpty(keysearch) || (Helpers.RemoveVietnameseDiacritics(m.TenVattu).ToLower().Contains(Helpers.RemoveVietnameseDiacritics(keysearch).ToLower())) || m.SoHieu.ToLower().Contains(keysearch.ToLower())));
                    //Chỉ lọc khi không có filter
                    foreach(var vt in mylstvt)
                    { 
                        var productSimilarity = frmMain.CompareProduct(vt.TenVattu.ToLower(), frmMain.TenVTMain.ToLower());
                        vt.Real = productSimilarity;
                    }
                    gridControl1.DataSource = mylstvt.OrderByDescending(m=>m.Percent).OrderByDescending(m => m.Real).ToList();
                    GridStripRow(gridView1);
                }
               
            }
            else
            {

                var datasource = frmMain.lstvt.Where(m => (m.TenVattu.ToLower().Contains(keysearch.ToLower())) || m.SoHieu.ToLower().Contains(keysearch.ToLower()));
                var ddd = comboBoxEdit1.Text;
                if (frmMain.lstvtgoiy!=null && frmMain.lstvtgoiy.Count>0 && Typeform==2 && ddd!= "Tất cả")
                {
                    datasource = datasource.Where(m =>
        frmMain.lstvtgoiy.Any(n => n.SoHieu == m.SoHieu)
    );
                    gridControl1.DataSource = datasource;
                }
                else
                {
                    var mylstvt = frmMain.lstvt.Where(m => (m.TenVattu.ToLower().Contains(keysearch.ToLower())) || m.SoHieu.ToLower().Contains(keysearch.ToLower()));

                    foreach (var vt in mylstvt)
                    {
                        if (frmMain.TenVTMain != null)
                        {
                            var productSimilarity = frmMain.CompareProduct(vt.TenVattu.ToLower(), frmMain.TenVTMain.ToLower());
                            vt.Real = productSimilarity;
                        }
                     
                    }
                    gridControl1.DataSource = mylstvt.OrderByDescending(m => m.Percent).OrderByDescending(m => m.Real).ToList();
                }
                    GridStripRow(gridView1);
            }
           
        }
        public void Reload()
        {
            string queryCheckVatTu = @"SELECT * FROM Vattu WHERE LCase(SoHieu) = LCase(?) AND LCase(DonVi) = LCase(?)";
            var parameterss = new OleDbParameter[]
            {
                new OleDbParameter("?",dtoVatTu.SoHieu!=null?dtoVatTu.SoHieu.ToLower():""),
                 new OleDbParameter("?",dtoVatTu.DonVi!=null?Helpers.ConvertUnicodeToVni(dtoVatTu.DonVi.ToLower()):"")
               };
            var kq = ExecuteQuery(queryCheckVatTu, parameterss);
            if (kq.Rows.Count == 0)
            {
                txtMaSo.Text = "0";
            }
            else
            {
                txtMaSo.Text = kq.Rows[0]["MaSo"].ToString();
            }
             txtSohieu.Text = dtoVatTu.SoHieu;
            txtTenvattu.Text = dtoVatTu.TenVattu;
            textEdit2.Text = dtoVatTu.TenVattu;
            txtDonvi.Text = dtoVatTu.DonVi;
            txtGhichu.Text= dtoVatTu.GhiChu;
            var datasource = frmMain.lstvt.AsEnumerable();
            foreach(var item in frmMain.lstvt.AsEnumerable())
            {
               var vt= frmMain.lstvtgoiy.Where(m => m.SoHieu == item.SoHieu).FirstOrDefault();

                if(vt!=null)
                {
                    if(item.TenVattu.ToLower() == "cò mổ gn6. 20mm dr,w tq")
                    {
                        int a = 10;
                    }
                    VietnameseProductMatcher matcher = new VietnameseProductMatcher();
                    string normalizedTen = matcher.NormalizeVietnameseProduct(item.TenVattu);
                    string normalizedTen2 = matcher.NormalizeVietnameseProduct(txtTenvattu.Text);
                    var kiemtracodong = HasParentheses(normalizedTen2);
                    if (kiemtracodong)
                    {
                       // normalizedTen2 = RemoveParentheses(normalizedTen2); 
                    }
                    var percent= frmMain.CompareProductNew(normalizedTen.ToLower(), normalizedTen2.ToLower());
                    if(normalizedTen.ToLower() != normalizedTen2.ToLower() && percent==100)
                    {
                        percent -= 1;
                    }
                    item.Percent = percent;
                }
            }
            if (frmMain.lstvtgoiy != null && frmMain.lstvtgoiy.Count > 0)
                        {
                            datasource = datasource.Where(m =>
                frmMain.lstvtgoiy.Any(n => n.SoHieu == m.SoHieu)
            );
            }
            else
            {
                datasource =null;
            }
            if (datasource != null)
            {
                 gridControl1.DataSource = datasource
                .OrderByDescending(m => m.Percent)
                .ThenByDescending(m => m.SoLuong)   // nếu SoLuong càng lớn càng ưu tiên
                .ToList();
                int getmavtu = datasource.OrderByDescending(m => m.Percent).FirstOrDefault().MaPhanLoai;
                var selectedItem = comboBoxEdit1.Properties.Items.Cast<Item>().FirstOrDefault(i => i.Id == getmavtu);
                if (selectedItem != null)
                {
                    comboBoxEdit1.SelectedItem = selectedItem;
                }
            }
            else
                gridControl1.DataSource = null;
            DevExpress.XtraGrid.Views.Grid.GridView view = gridControl1.MainView as DevExpress.XtraGrid.Views.Grid.GridView;
            for (int i = 0; i < view.RowCount; i++)
            {
                // Lấy giá trị của cột STT
                if (view.GetRowCellValue(i, "SoHieu").ToString().ToLower() == txtSohieu.Text.ToLower())
                {
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        if (gridView1.RowCount > i) // Kiểm tra số lượng dòng
                        {
                            gridView1.FocusedRowHandle = i; // Đặt focus
                            gridView1.MakeRowVisible(i); // Cuộn đến dòng
                            gridView1.SelectRow(i); // Chọn dòng
                            textEdit1.Text = gridView1.GetRowCellValue(i, "TenVattu").ToString();
                            sohieuvt = gridView1.GetRowCellValue(i, "SoHieu").ToString();
                            // txtSearch.Focus();
                        }
                    });
                    return;
                }
            }
            GridStripRow(gridView1);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Kiểm tra phím tắt (ví dụ: Ctrl + N)
            if (keyData == (Keys.Control | Keys.G))
            {
                btnGhi.PerformClick(); // Gọi sự kiện nhấn nút
                return true; // Đã xử lý phím
            }
            return base.ProcessCmdKey(ref msg, keyData); // Chuyển tiếp cho xử lý tiếp
        }
        private bool firstload = true;
        private void frmHangHoa_Load(object sender, EventArgs e)
        {
            
        }

        public class Item
        {
            public string Name { get; set; }
            public int Id { get; set; }

            public override string ToString()
            {
                return Name; // Hiển thị tên trong ComboBox
            }
        }
        string dbPath = "";
        private DataTable ExecuteQuery(string query, params OleDbParameter[] parameters)
        {
            DataTable dataTable = new DataTable();
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);

            // Xóa phần \bin\Debug để lấy đường dẫn gốc
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
            string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");
            try
            {
                string content = File.ReadAllText(filePaths);
                dbPath = content;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi đọc file: " + ex.Message);
            }
            string connectionString = "";
            string password = "1@35^7*9)1";
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    Console.WriteLine("Kết nối đến cơ sở dữ liệu thành công!");

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
        private int ExecuteQueryResult(string query, params OleDbParameter[] parameters)
        {
            string connectionString = "";
            string password = "1@35^7*9)1";
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";
            DataTable dataTable = new DataTable();

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Kết nối đến cơ sở dữ liệu thành công!");

                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    // Thêm các tham số vào command
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    int rowsAffected = command.ExecuteNonQuery(); // Thực thi câu lệnh
                    return rowsAffected;
                }
            }

            return -1;
        }

        private void comboBoxEdit1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (comboBoxEdit1.SelectedItem != null && !firstload && Typeform==1)
            if (comboBoxEdit1.SelectedItem != null && !firstload )
            {
                // Lấy phần tử được chọn
                var selectedItem = comboBoxEdit1.SelectedItem as Item;

                if (selectedItem != null)
                {
                    int selectedId = selectedItem.Id; // Lấy giá trị Id 
                    //frmMain.currentselectId = comboBoxEdit1.SelectedIndex;
                    LoadData(selectedId, txtSearch.Text);
                }
            }
        }
        public frmMain frmMain;
        public bool isChange = false;
        private void gridControl1_DoubleClick(object sender, EventArgs e)
        {
            
        }
        public class TbImportDetail
        {
            public int ID { get; set; }
            public int ParentId { get; set; }
            public string SoHieu { get; set; }
            public double Soluong { get; set; }
            public double Dongia { get; set; }
            public string DVT { get; set; }
            public string Ten { get; set; }
            public string MaCT { get; set; }
            public string TKNo { get; set; }
            public string TKCo { get; set; }
            public double TTien { get; set; }
            public double SoPSGoc { get; set; }
            public double Percent { get; set; }
            public int Tchat { get; set; }
            public double Vat { get; set; }
        }
        private void btnGhi_Click(object sender, EventArgs e)
        {
            //Kiểm tra mã này đã thêm chưa
            if (frmMain.lstvt.Any(m => m.SoHieu.ToLower().Trim() == txtSohieu.Text.ToLower().Trim()))
            {
                XtraMessageBox.Show("Mã đã được thêm trong hệ thống, vui lòng nhập mã khác!");
                return;
            }

            int selectedId = 0;
            var selectedItem = comboBoxEdit1.SelectedItem as Item;
          

            // Xác định xem đây là thêm mới hay cập nhật
            bool isInsert = txtMaSo.Text == "0";
            string query;
            OleDbParameter[] parameters;

            if (isInsert)
            {
                if (selectedItem.Id == 0 || selectedItem.Id == -1)
                {
                    XtraMessageBox.Show("Vui lòng chọn danh mục");
                    return;
                }
                if (selectedItem != null)
                {
                    selectedId = selectedItem.Id; // Lấy giá trị Id  
                }
                query = @"INSERT INTO Vattu (MaPhanLoai, SoHieu, TenVattu, DonVi, GhiChu) VALUES (?, ?, ?, ?, ?)";
                parameters = new OleDbParameter[]
                {
            new OleDbParameter("?", selectedId),
            new OleDbParameter("?", txtSohieu.Text),
            new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtTenvattu.Text)),
            new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtDonvi.Text)),
            new OleDbParameter("?", string.IsNullOrEmpty(txtGhichu.Text)?"...":txtGhichu.Text)
                };
                frmMain.lstvt.Add(new VatTu
                {
                    MaPhanLoai = selectedId,
                    SoHieu = txtSohieu.Text,
                    TenVattu = txtTenvattu.Text,
                    DonVi = txtDonvi.Text,
                    GhiChu = string.IsNullOrEmpty(txtGhichu.Text) ? "..." : txtGhichu.Text
                });
            }
            else
            {
                query = @"UPDATE Vattu SET MaPhanLoai=?, SoHieu=?, TenVattu=?, DonVi=?, GhiChu=? WHERE MaSo=?";
                parameters = new OleDbParameter[]
                {
            new OleDbParameter("?", selectedId),
            new OleDbParameter("?", txtSohieu.Text),
            new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtTenvattu.Text)),
            new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtDonvi.Text)),
            new OleDbParameter("?", txtGhichu.Text),
            new OleDbParameter("?", txtMaSo.Text)
                };
            }

            // Thực hiện truy vấn
            int rowsAffected = 0;
            try
            {
                 rowsAffected = ExecuteQueryResult(query, parameters);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
                return;
            }

            // [Optional] Xử lý kết quả trả về (ví dụ: thông báo thành công/thất bại)
            if (rowsAffected > 0)
            {

                if (Typeform == 2)
                {
                    var gv = frmMain.Typechon == 1 ? frmMain.gv2 : frmMain.gv4;
                    int rowHandle = hoverRowHandle;

                    if (rowHandle >= 0 && !gv.IsGroupRow(rowHandle))
                    {
                      
                        var rowObj = gv.GetRow(rowHandle);
                   
                        if (rowObj != null)
                        {
                            var type = rowObj.GetType();
                            string ten = (string)type.GetProperty("Ten").GetValue(rowObj);
                            string sohieu= (string)type.GetProperty("SoHieu").GetValue(rowObj);
                            int ID= (int)type.GetProperty("ID").GetValue(rowObj);
                            string DVT = (string)type.GetProperty("DVT").GetValue(rowObj);
                            TbImportDetail TbImportDetail = rowObj as TbImportDetail;

                            type.GetProperty("SoHieu")
                                ?.SetValue(rowObj, txtSohieu.Text);

                            type.GetProperty("DVT")
                                ?.SetValue(rowObj, txtDonvi.Text);

                            type.GetProperty("Ten")
                                ?.SetValue(rowObj, txtTenvattu.Text);

                            gv.RefreshRow(rowHandle);

                            string query2 = @"UPDATE tbimportdetail SET Ten=?,SoHieu=?,DVT=?  WHERE ID=?";
                            var parameters2 = new OleDbParameter[]
                             {
                                 new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtTenvattu.Text)),
                                 new OleDbParameter("?", txtSohieu.Text),
                                 new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtDonvi.Text)),
                                 new OleDbParameter("?", ID)
                             };
                            rowsAffected = ExecuteQueryResult(query2, parameters2);
                        }
                    }
                }
                if (Typeform ==1)
                {
                    var gv = frmMain.Typechon == 1 ? frmMain.gv2 : frmMain.gv4;
                    int rowHandle = gv.FocusedRowHandle;

                    if (rowHandle >= 0 && !gv.IsGroupRow(rowHandle))
                    {

                        var rowObj = gv.GetRow(rowHandle);

                        if (rowObj != null)
                        {
                            var type = rowObj.GetType();
                            string ten = (string)type.GetProperty("Ten").GetValue(rowObj);
                            string sohieu = (string)type.GetProperty("SoHieu").GetValue(rowObj);
                            int ID = (int)type.GetProperty("ID").GetValue(rowObj);
                            string DVT = (string)type.GetProperty("DVT").GetValue(rowObj);
                            TbImportDetail TbImportDetail = rowObj as TbImportDetail;

                            type.GetProperty("SoHieu")
                                ?.SetValue(rowObj, txtSohieu.Text);

                            type.GetProperty("DVT")
                                ?.SetValue(rowObj, txtDonvi.Text);

                            type.GetProperty("Ten")
                                ?.SetValue(rowObj, txtTenvattu.Text);

                            gv.RefreshRow(rowHandle);

                            string query2 = @"UPDATE tbimportdetail SET Ten=?,SoHieu=?,DVT=?  WHERE ID=?";
                            var parameters2 = new OleDbParameter[]
                             {
                                 new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtTenvattu.Text)),
                                 new OleDbParameter("?", txtSohieu.Text),
                                 new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtDonvi.Text)),
                                 new OleDbParameter("?", ID)
                             };
                            rowsAffected = ExecuteQueryResult(query2, parameters2);
                        }
                    }
                }
                else
                {
                    frmMain.hiddenValue = txtSohieu.Text;
                    frmMain.hiddenValue2 = txtDonvi.Text;
                    frmMain.hiddenValue3 = txtTenvattu.Text;
                    isChange = true;
                }
                    this.Close();

                LoadData(selectedItem.Id, txtSearch.Text);
                 RefreshData();
                DevExpress.XtraGrid.Views.Grid.GridView view = gridControl1.MainView as DevExpress.XtraGrid.Views.Grid.GridView; // Lấy GridView
                //for (int i = 0; i < view.RowCount; i++)
                //{
                //    // Lấy giá trị của cột STT
                //    if (view.GetRowCellValue(i, "SoHieu").ToString() == txtSohieu.Text)
                //    {
                //        view.FocusedRowHandle = i; // Chọn dòng
                //        view.SelectRow(i); // Chọn dòng
                //        view.MakeRowVisible(i); // Cuộn tới dòng đã chọn
                //        return; // Thoát sau khi tìm thấy
                //    }
                //}
            }
            else
            {
                MessageBox.Show(isInsert ? "Thêm mới thất bại!" : "Cập nhật thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void RefreshData()
        {
            txtMaSo.Text = "0";
            txtSohieu.Text = "";
            txtTenvattu.Text = "";
            txtDonvi.Text = "";
            txtGhichu.Text = "";
            gridControl2.DataSource = null;
        }
        private void btnThem_Click(object sender, EventArgs e)
        {
            RefreshData();
        }
        private class TonKho
        {
            public int MaSo { get; set; }
            public double SoLuong { get; set; }
            public double DonGia { get; set; }
            public double ThanhTien { get; set; }
        }
        private void gridView1_RowClick(object sender, RowClickEventArgs e)
        {
            // Lấy chỉ số hàng đã click
            int rowHandle = e.RowHandle;

            // Lấy dữ liệu từ hàng
            var value = gridView1.GetRowCellValue(rowHandle, "SoHieu").ToString();
            txtSohieu.Text = value;
            sohieuvt = value;
            txtTenvattu.Text = gridView1.GetRowCellValue(rowHandle, "TenVattu").ToString();
            textEdit1.Text = gridView1.GetRowCellValue(rowHandle, "TenVattu").ToString();
            txtDonvi.Text = gridView1.GetRowCellValue(rowHandle, "DonVi").ToString();
            txtGhichu.Text = gridView1.GetRowCellValue(rowHandle, "GhiChu").ToString();
            txtMaSo.Text = gridView1.GetRowCellValue(rowHandle, "MaSo").ToString();

            string query = @" SELECT *  FROM TonKho where MaVatTu= ? ";
            var parameterss = new OleDbParameter[]
            {
                new OleDbParameter("?",txtMaSo.Text)
               };
            var kq = ExecuteQuery(query, parameterss);
            List<TonKho> lstTonkho = new List<TonKho>();
            if (kq.Rows.Count > 0)
            {
                try
                {
                    TonKho tk = new TonKho();
                    int cnt = 12;
                    while (kq.Rows[0]["Luong_" + cnt].ToString() == "0")
                    {
                        cnt += 1;
                    }
                    tk.SoLuong = kq.Rows[0]["Luong_" + cnt] != null ? double.Parse(kq.Rows[0]["Luong_" + cnt].ToString()) : 0;
                    tk.ThanhTien = kq.Rows[0]["Tien_" + cnt] != null ? double.Parse(kq.Rows[0]["Tien_" + cnt].ToString()) : 0;
                    if (tk.SoLuong != 0 && tk.ThanhTien != 0)
                    {
                        tk.DonGia = Math.Round(double.Parse(kq.Rows[0]["Tien_" + cnt].ToString()) / tk.SoLuong, 2);
                        lstTonkho.Add(tk);
                    }
                }
                catch (Exception ex)
                {
                   // XtraMessageBox.Show(ex.Message);
                }

            }
            gridControl2.DataSource = lstTonkho;
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {

            DialogResult result = XtraMessageBox.Show(
        "Bạn có chắc chắn muốn xóa vật tư này?",
        "Xác Nhận",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                var query = @"delete from Vattu where MaSo=?";
                var parameters = new OleDbParameter[]
               {
            new OleDbParameter("?", txtMaSo.Text)
               };
                int rowsAffected = ExecuteQueryResult(query, parameters);
                var selectedItem = comboBoxEdit1.SelectedItem as Item;

                if (selectedItem != null)
                {
                    int selectedId = selectedItem.Id; // Lấy giá trị Id 
                    frmMain.lstvt.Remove(frmMain.lstvt.FirstOrDefault(m => m.MaSo.ToString() == txtMaSo.Text));
                    frmMain.LoadVT();
                    LoadData(selectedId, txtSearch.Text);
                    gridView1.RefreshData();
                }
            }

        }

        private void gridView1_DataSourceChanged(object sender, EventArgs e)
        {

        }

        private void frmHangHoa_Load_1(object sender, EventArgs e)
        {
            //gridView1.OptionsFind.AlwaysVisible = true; // Kích hoạt thanh tìm kiếm

            var query = @"SELECT * FROM PhanLoaiVattu ORDER BY TenPhanLoai";
            var dt = ExecuteQuery(query, null);
            if (dt != null && dt.Rows.Count > 0)
            {
                comboBoxEdit1.Properties.Items.Clear(); // Xóa các mục cũ
                comboBoxEdit1.Properties.Items.Add(new Item { Name = "Tất cả", Id = -1 });
                foreach (DataRow row in dt.Rows)
                {
                    // Thêm từng mục vào ComboBoxEdit
                    comboBoxEdit1.Properties.Items.Add(new Item
                    {
                        Name = Helpers.ConvertVniToUnicode(row["SoHieu"].ToString()) + " - " + Helpers.ConvertVniToUnicode(row["TenPhanLoai"].ToString()),
                        Id = Convert.ToInt32(row["MaSo"])
                    });
                }  
                txtSohieu.Text = dtoVatTu.SoHieu;
                txtTenvattu.Text = dtoVatTu.TenVattu;
                textEdit2.Text = dtoVatTu.TenVattu;
                txtDonvi.Text = dtoVatTu.DonVi;
                txtdongia.Text = dtoVatTu.Dongia != 0 ? dtoVatTu.Dongia.ToString() : "";    
                txtsoluong.Text = dtoVatTu.SoLuong != 0 ? dtoVatTu.SoLuong.ToString() : ""; 
                comboBoxEdit1.Properties.NullText = "Chọn Tài khoản";
                comboBoxEdit1.Properties.TextEditStyle = TextEditStyles.DisableTextEditor; // Ngăn người dùng nhập trực tiếp
                int idsl = 0;
                if (comboBoxEdit1.Properties.Items.Count > 0)
                {
                    foreach (Item item in comboBoxEdit1.Properties.Items)
                    {
                        if (item.Id == frmMain.currentselectId)
                        {
                            if (frmMain.currentselectId == 0)
                            {
                                bool isfind = false;
                                foreach (DataRow it in dt.AsEnumerable().OrderBy(m => m["MaSo"]).CopyToDataTable().Rows)
                                {
                                    if (isfind)
                                        break;
                                    var getghichu = it["Ghichu"].ToString().Split(',').ToList();
                                    foreach (var it2 in getghichu)
                                    {
                                        if (string.IsNullOrEmpty(it2))
                                            continue;
                                        if (txtTenvattu.Text.ToLower().Contains(it2.ToLower()))
                                        {
                                            item.Id = it.Field<int>("MaSo");
                                            item.Name = it.Field<string>("SoHieu");
                                            isfind = true;
                                            break;
                                        }
                                    }

                                }
                            }
                            idsl = comboBoxEdit1.Properties.Items.IndexOf(item);
                            break;
                        }
                    }
                    //Tạm thời đóng cho chọn tất ca
                    if (Typeform == 2)
                        comboBoxEdit1.SelectedIndex = idsl; // Chọn phần tử đầu
                    else
                        comboBoxEdit1.SelectedIndex = 0; // Chọn phần tử đầu tiên
                    var selectedItem = comboBoxEdit1.SelectedItem as Item;
                    firstload = false;
                    LoadData(selectedItem.Id, txtSearch.Text);
                }
            }
            else
            {
                comboBoxEdit1.Properties.Items.Clear(); // Xóa dữ liệu cũ
                comboBoxEdit1.Properties.NullText = "Không có tài khoản nào";
            }
            //
            //Load data vat tu

            //Kiểm tra xem là sp moi hay cũ
            string queryCheckVatTu = @"SELECT * FROM Vattu WHERE LCase(SoHieu) = LCase(?) AND LCase(DonVi) = LCase(?)";
            var parameterss = new OleDbParameter[]
            {
                new OleDbParameter("?",dtoVatTu.SoHieu!=null?dtoVatTu.SoHieu.ToLower():""),
                 new OleDbParameter("?",dtoVatTu.DonVi!=null?Helpers.ConvertUnicodeToVni(dtoVatTu.DonVi.ToLower()):"")
               };
            var kq = ExecuteQuery(queryCheckVatTu, parameterss);
            if (kq.Rows.Count == 0)
            {
                txtMaSo.Text = "0";
            }
            else
            {
                txtMaSo.Text = kq.Rows[0]["MaSo"].ToString();
                txtGhichu.Text = kq.Rows[0]["GhiChu"].ToString();
                int mapl = int.Parse(kq.Rows[0]["MaPhanLoai"].ToString());

                //comboBoxEdit1.SelectedItem=
                foreach (Item item in comboBoxEdit1.Properties.Items)
                {
                    if (item.Id == mapl && Typeform==2)
                    {
                        comboBoxEdit1.EditValue = item; // Chọn mục theo ID
                        break; // Thoát khỏi vòng lặp
                    }
                }
            }
            
            DevExpress.XtraGrid.Views.Grid.GridView view = gridControl1.MainView as DevExpress.XtraGrid.Views.Grid.GridView;
            for (int i = 0; i < view.RowCount; i++)
            {
                // Lấy giá trị của cột STT
                if (view.GetRowCellValue(i, "SoHieu").ToString().ToLower() == txtSohieu.Text.ToLower())
                {
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        if (gridView1.RowCount > i) // Kiểm tra số lượng dòng
                        {
                            gridView1.FocusedRowHandle = i; // Đặt focus
                            gridView1.MakeRowVisible(i); // Cuộn đến dòng
                            gridView1.SelectRow(i); // Chọn dòng
                            textEdit1.Text = gridView1.GetRowCellValue(i, "TenVattu").ToString();
                            sohieuvt = gridView1.GetRowCellValue(i, "SoHieu").ToString();
                           // txtSearch.Focus();
                        }
                    });
                    return;
                }
            }
           
        }
        string sohieuvt = "";
        private void btnSearch_Click(object sender, EventArgs e)
        {
            var selectedItem = comboBoxEdit1.SelectedItem as Item;
            LoadData(selectedItem.Id, txtSearch.Text);
        }
            
        private void txtSearch_EditValueChanged(object sender, EventArgs e)
        {
            var selectedItem = comboBoxEdit1.SelectedItem as Item;
            LoadData(selectedItem.Id, txtSearch.Text);
        }

        private void frmHangHoa_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.ToString() == "Escape")
            {
                this.Close();
            }
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
               
            }
        }

        private void frmHangHoa_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
          
        }

        private void gridControl1_MouseDown(object sender, MouseEventArgs e)
        {
          
        }

        private void gridControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                EventArgs args = new EventArgs();
                gridControl1_DoubleClick_1(sender, e);

            }
        }

        private void frmHangHoa_Activated(object sender, EventArgs e)
        {
            
        }
       public FileImportDetail2 currentrow { get; set; }
        private void gridControl1_DoubleClick_1(object sender, EventArgs e)
        {
            DevExpress.XtraGrid.Views.Grid.GridView gridView = gridControl1.MainView as DevExpress.XtraGrid.Views.Grid.GridView;
            var hitInfo = gridView.CalcHitInfo(gridView.GridControl.PointToClient(MousePosition));

            
            // Kiểm tra nếu nhấp vào một ô
            if (hitInfo.InRowCell)
            {
                int columnIndex = hitInfo.Column.VisibleIndex; // Chỉ số cột
                FileImportDetail rowData = gridView.GetRow(hitInfo.RowHandle) as FileImportDetail;
                // Lấy giá trị trong ô đã nhấp
                var hiddenValue = gridView.GetRowCellValue(hitInfo.RowHandle, gridView.Columns["SoHieu"]);
                var hiddenValue2 = gridView.GetRowCellValue(hitInfo.RowHandle, gridView.Columns["DonVi"]);
                var hiddenValue3 = gridView.GetRowCellValue(hitInfo.RowHandle, gridView.Columns["TenVattu"]); 
                frmMain.hiddenValue = hiddenValue.ToString();
                frmMain.hiddenValue2 = hiddenValue2.ToString();
                frmMain.hiddenValue3 = hiddenValue3.ToString();
                double newDongia = 0;
                double newsl = 0;
                //Kiểm tra có khác đơn vị tính mà cùng tên không
                bool changePrice = false;
                if (currentrow != null)
                {
                    //Kiem tra them setting
                    string qr = @"select * FROM tbSetting";

                    var kq = ExecuteQuery(qr);
                    int rs = 0;
                    if (kq.Rows.Count > 0 &&
                        kq.Rows[0]["QuyDoiDVT"] != DBNull.Value &&
                        !string.IsNullOrWhiteSpace(kq.Rows[0]["QuyDoiDVT"].ToString()))
                    {
                        int.TryParse(kq.Rows[0]["QuyDoiDVT"].ToString(), out rs);
                    }

                    if (currentrow.Ten== hiddenValue3.ToString() && currentrow.DVT!= hiddenValue2 && rs==1)
                    { 
                        string value = XtraInputBox.Show(
                       "Nhập dữ liệu:",
                       "Nhập số lượng quy đổi",
                       ""
                    );

                        if (value != null)
                        {
                            if (double.Parse(value) > 0)
                            {
                                changePrice = true;
                                double newTT = currentrow.TTien / currentrow.Soluong;
                                newDongia = newTT / double.Parse(value);
                                newsl = double.Parse(value) * currentrow.Soluong;
                                var query = @"UPDATE tbimportdetail SET  SoHieu=?,DVT=?,Ten=?,SoLuong=?,DonGia=? where ID=? ";
                                var parameters = new OleDbParameter[]
                                {
                                        new OleDbParameter("?", hiddenValue),
                                        new OleDbParameter("?",  Helpers.ConvertUnicodeToVni(currentrow.DVT.ToString())) ,
                                        new OleDbParameter("?", Helpers.ConvertUnicodeToVni(hiddenValue3.ToString())),
                                         new OleDbParameter("?", newsl),
                                          new OleDbParameter("?", newDongia),
                                        new OleDbParameter("?", currentrow.ID)
                                };
                                var rowsAffected = ExecuteQueryResult(query, parameters);
                            }
                            else
                            {
                                changePrice = true;
                                double newTT = currentrow.TTien / currentrow.Soluong;
                                newsl = currentrow.Soluong / double.Parse(value)*(-1) ;
                                newDongia = currentrow.TTien / newsl;
                                
                                var query = @"UPDATE tbimportdetail SET  SoHieu=?,DVT=?,Ten=?,SoLuong=?,DonGia=? where ID=? ";
                                var parameters = new OleDbParameter[]
                                {
                                        new OleDbParameter("?", hiddenValue),
                                        new OleDbParameter("?",  Helpers.ConvertUnicodeToVni(currentrow.DVT.ToString())) ,
                                        new OleDbParameter("?", Helpers.ConvertUnicodeToVni(hiddenValue3.ToString())),
                                         new OleDbParameter("?", newsl),
                                          new OleDbParameter("?", newDongia),
                                        new OleDbParameter("?", currentrow.ID)
                                };
                                var rowsAffected = ExecuteQueryResult(query, parameters);
                            }
                           
                        }
                        else
                        {
                            var query = @"UPDATE tbimportdetail SET  SoHieu=?,DVT=?,Ten=? where ID=? ";
                            var parameters = new OleDbParameter[]
                            {
                                        new OleDbParameter("?", hiddenValue),
                                        new OleDbParameter("?",  Helpers.ConvertUnicodeToVni(hiddenValue2.ToString())) ,
                                        new OleDbParameter("?", Helpers.ConvertUnicodeToVni(hiddenValue3.ToString())),
                                        new OleDbParameter("?", currentrow.ID)
                            };
                            var rowsAffected = ExecuteQueryResult(query, parameters);
                        }
                    }
                    else
                    {
                        var query = @"UPDATE tbimportdetail SET  SoHieu=?,DVT=?,Ten=? where ID=? ";
                        var parameters = new OleDbParameter[]
                        {
                                        new OleDbParameter("?", hiddenValue),
                                        new OleDbParameter("?",  Helpers.ConvertUnicodeToVni(hiddenValue2.ToString())) ,
                                        new OleDbParameter("?", Helpers.ConvertUnicodeToVni(hiddenValue3.ToString())),
                                        new OleDbParameter("?", currentrow.ID)
                        };
                        var rowsAffected = ExecuteQueryResult(query, parameters);
                    }
                }

              
                isChange = true;
                //
                if (Typeform == 2 || Typeform==1)
                {
                    var gv = frmMain.Typechon == 1 ? frmMain.gv2 : frmMain.gv4;
                    var query = @"UPDATE tbimportdetail SET  SoHieu=?,DVT=?,Ten=? where ID=? ";
                    var parameters = new OleDbParameter[]
                    {
                                        new OleDbParameter("?", hiddenValue),
                                        new OleDbParameter("?",  Helpers.ConvertUnicodeToVni(hiddenValue2.ToString())) ,
                                        new OleDbParameter("?", Helpers.ConvertUnicodeToVni(hiddenValue3.ToString())),
                                        new OleDbParameter("?", currentrow.ID)
                    };
                    var rowsAffected = ExecuteQueryResult(query, parameters);
                    if (Typeform == 2)
                    {
                        int rowHandle = hoverRowHandle;

                        if (rowHandle >= 0 && !gv.IsGroupRow(rowHandle))
                        {
                            var rowObj = gv.GetRow(rowHandle);

                            if (rowObj != null)
                            {
                                var type = rowObj.GetType();

                                type.GetProperty("SoHieu")
                                    ?.SetValue(rowObj, hiddenValue);

                                type.GetProperty("DVT")
                                    ?.SetValue(rowObj, hiddenValue2);

                                type.GetProperty("Ten")
                                    ?.SetValue(rowObj, hiddenValue3);
                                if (changePrice)
                                {
                                    type.GetProperty("Dongia")
                                 ?.SetValue(rowObj, newDongia);
                                    type.GetProperty("Soluong")
                                ?.SetValue(rowObj, newsl);
                                }
                                gv.RefreshRow(rowHandle);
                            }
                        }
                    }
                   
                    var ddd = frmMain.lstrowSohieu;
                    if (ddd.Count > 1)
                    {
                        DialogResult result = XtraMessageBox.Show("Có " + ddd.Count + " sản phẩm khác đang trùng tên với sản phẩm đang sửa, bạn có muốn cập nhật luôn mã mới?",
                                        "Xác nhận",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            frmMain.lstrowSohieu = ddd;
                            frmMain.CapnhatlistSoHieu(ddd);
                        }
                    }
                }
                this.Close();
            }
        }

        private void btnThoat_Click_1(object sender, EventArgs e)
        {

        }

        private void simpleButton1_Click_1(object sender, EventArgs e)
        {
            DialogResult result = XtraMessageBox.Show(
     "Bạn có chắc 2 sản phẩm này là 1?",
     "Xác Nhận",
     MessageBoxButtons.YesNo,
     MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                string query = @"UPDATE Vattu SET TenVattu2=?  WHERE SoHieu=?";
                var parameters = new OleDbParameter[]
                 {
            new OleDbParameter("?", Helpers.ConvertUnicodeToVni(textEdit2.Text)),
            new OleDbParameter("?", sohieuvt)
                 };
                int rowsAffected = ExecuteQueryResult(query, parameters);
                frmMain._lookupByTenPhu[textEdit2.Text] = sohieuvt;
            }
        }

        private void frmHangHoa_MouseHover(object sender, EventArgs e)
        {

        }

        private void gridView1_MouseEnter(object sender, EventArgs e)
        {

        }
        public int mouseState = 0;  
        private void frmHangHoa_MouseEnter(object sender, EventArgs e)
        {
            mouseState = 1;
        }

        private void panelControl1_MouseEnter(object sender, EventArgs e)
        {

        }

        private void frmHangHoa_MouseLeave(object sender, EventArgs e)
        {
            
        }

        private void frmHangHoa_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Typeform == 1)
            {
                frmMain.isHHPopupOpen = false;
            }
            
        }

        private void btnSearch_Click_1(object sender, EventArgs e)
        {

        }

        private void gridControl1_Click(object sender, EventArgs e)
        {

        }
    }
}