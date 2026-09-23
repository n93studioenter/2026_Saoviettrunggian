using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Mask.Design;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraRichEdit.API.Native;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaovietTax
{
    public partial class frmCongtrinh : DevExpress.XtraEditors.XtraForm
    {
        public frmCongtrinh()
        {
            InitializeComponent();
        }
        public class VatTu
        {
            public int MaSo { get; set; }   
            public int MaPhanLoai { get; set; }
            public string SoHieu { get; set; }
            public string TenVattu { get; set; }
            public string DonVi { get; set; }
        }
        public VatTu dtoVatTu { get; set; }
        private void LoadData(int id, string keysearch)
        { 
            string query;
            List<OleDbParameter> parameters = new List<OleDbParameter>();

            if (id != 0)
            {
                parameters.Add(new OleDbParameter("?", id)); // Thêm tham số MaPhanLoai
                query = "SELECT * FROM TP154 WHERE MaPhanLoai=?";
            }
            else
            {
                query = "SELECT * FROM TP154";
            }

            if (!string.IsNullOrEmpty(keysearch))
            {
                // Chuyển keysearch thành chữ thường và thêm ký tự % vào trước và sau
                keysearch = "%" + Helpers.ConvertUnicodeToVni(keysearch) + "%";
                query += (id == 0) ? " WHERE LCase(TenVattu) like LCase(?)" : " AND LCase(TenVattu) like LCase(?)";
                parameters.Add(new OleDbParameter("?", keysearch)); // Thêm tham số TenVattu
            }

            // Thực hiện truy vấn
            var kq = ExecuteQuery(query, parameters.ToArray());

            // Xử lý dữ liệu
            for (int i = 0; i < kq.Rows.Count; i++)
            {
                kq.Rows[i]["TenVattu"] = Helpers.ConvertVniToUnicode(kq.Rows[i]["TenVattu"].ToString());
                kq.Rows[i]["DonVi"] = Helpers.ConvertVniToUnicode(kq.Rows[i]["DonVi"].ToString());
                kq.Rows[i]["GhiChu"] = Helpers.ConvertVniToUnicode(kq.Rows[i]["GhiChu"].ToString());
            }

            gridControl1.DataSource = kq;
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
        private void frmCongtrinh_Load(object sender, EventArgs e)
        {
          //  gridView1.OptionsFind.AlwaysVisible = true; // Kích hoạt thanh tìm kiếm

            string query = @"SELECT * FROM PhanLoai154 ORDER BY TenPhanLoai";
            var dt = ExecuteQuery(query, null);
            if (dt != null && dt.Rows.Count > 0)
            {
                comboBoxEdit1.Properties.Items.Clear(); // Xóa các mục cũ
                comboBoxEdit1.Properties.Items.Add(new Item { Name = "Tất cả", Id = 0 });
                foreach (DataRow row in dt.Rows)
                {
                    // Thêm từng mục vào ComboBoxEdit
                    comboBoxEdit1.Properties.Items.Add(new Item
                    {
                        Name = Helpers.ConvertVniToUnicode(row["SoHieu"].ToString()) + " - " + Helpers.ConvertVniToUnicode(row["TenPhanLoai"].ToString()),
                        Id = Convert.ToInt32(row["MaSo"])
                    });
                }

                comboBoxEdit1.Properties.NullText = "Chọn Tài khoản";
                comboBoxEdit1.Properties.TextEditStyle = TextEditStyles.DisableTextEditor; // Ngăn người dùng nhập trực tiếp
                if (comboBoxEdit1.Properties.Items.Count > 0)
                {
                    comboBoxEdit1.SelectedIndex = frmMain.currentselectId; // Chọn phần tử đầu tiên
                    var selectedItem = comboBoxEdit1.SelectedItem as Item;

                    LoadData(selectedItem.Id,txtSearch.Text);
                }
            }
            else
            {
                comboBoxEdit1.Properties.Items.Clear(); // Xóa dữ liệu cũ
                comboBoxEdit1.Properties.NullText = "Không có tài khoản nào";
            }
            //
            //Load data vat tu
            //txtSohieu.Text = dtoVatTu.SoHieu;
            //txtTenvattu.Text = dtoVatTu.TenVattu;
            //txtDonvi.Text = dtoVatTu.DonVi;
            //Kiểm tra xem là sp moi hay cũ
          
            string queryCheckVatTu = @"SELECT * FROM TP154 WHERE LCase(SoHieu) = LCase(?)";
            var getsplit = dtoVatTu.SoHieu.Split('|');
            if (getsplit.Count() > 1)
            {
                var parameterss = new OleDbParameter[]
            {
                new OleDbParameter("?",getsplit[1]),
               };
                var kq = ExecuteQuery(queryCheckVatTu, parameterss);
                if (kq.Rows.Count == 0)
                {
                    txtId.Text = "0";
                }
                else
                {
                    txtSohieu.Text = kq.Rows[0]["SoHieu"].ToString();
                    txtTenvattu.Text = Helpers.ConvertVniToUnicode(kq.Rows[0]["TenVattu"].ToString());
                    txtDonvi.Text = Helpers.ConvertVniToUnicode(kq.Rows[0]["DonVi"].ToString());
                    txtId.Text = kq.Rows[0]["MaSo"].ToString();
                    txtGhichu.Text = Helpers.ConvertVniToUnicode(kq.Rows[0]["GhiChu"].ToString());
                    int mapl = int.Parse(kq.Rows[0]["MaPhanLoai"].ToString());
                    //
                    var matc = kq.Rows[0]["MaTK"].ToString();
                     query = @" SELECT *  FROM HeTHongTK where MaTC= ? ";
                     parameterss = new OleDbParameter[]
                    {
                new OleDbParameter("?",matc)
                       };
                     kq = ExecuteQuery(query, parameterss);
                    txtTaikhoan.Text = kq.Rows[0]["SoHieu"].ToString();
                    


                    //comboBoxEdit1.SelectedItem=
                    foreach (Item item in comboBoxEdit1.Properties.Items)
                    {
                        if (item.Id == mapl)
                        {
                            comboBoxEdit1.EditValue = item; // Chọn mục theo ID
                            break; // Thoát khỏi vòng lặp
                        }
                    }
                }

            }

            DevExpress.XtraGrid.Views.Grid.GridView view = gridControl1.MainView as DevExpress.XtraGrid.Views.Grid.GridView;
            for (int i = 0; i < view.RowCount; i++)
            {
                // Lấy giá trị của cột STT
                if (view.GetRowCellValue(i, "SoHieu").ToString() == txtSohieu.Text)
                {
                    view.FocusedRowHandle = i; // Chọn dòng
                    view.SelectRow(i); // Chọn dòng
                    return; // Thoát sau khi tìm thấy
                }
            }
            // LoadData();
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
        public frmMain frmMain;
        private void gridControl1_DoubleClick(object sender, EventArgs e)
        {
            GridView gridView = gridControl1.MainView as GridView;
            var hitInfo = gridView.CalcHitInfo(gridView.GridControl.PointToClient(MousePosition));


            // Kiểm tra nếu nhấp vào một ô
            if (hitInfo.InRowCell)
            {
                int columnIndex = hitInfo.Column.VisibleIndex; // Chỉ số cột
                
                // Lấy giá trị trong ô đã nhấp
                var hiddenValue = gridView.GetRowCellValue(hitInfo.RowHandle, gridView.Columns["SoHieu"]);
                frmMain.hiddenValue = hiddenValue.ToString();
                this.Close();
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            if(txtTenvattu.Text == "")
            {
                XtraMessageBox.Show("Vui lòng nhập tên vật tư");
                return;
            }   
            if (txtSohieu.Text == "")
            {
                XtraMessageBox.Show("Vui lòng nhập số hiệu");
                return;
            }
            if (txtDonvi.Text=="")
            {
                XtraMessageBox.Show("Vui lòng nhập đơn vị");
                return;
            }
            if(txtGhichu.Text == "")
            {
                txtGhichu.Text = "aaa";
            }
            if (txtId.Text == "0" || txtId.Text=="") 
            {
                double matk = 0;
                double tk = 0;
                string query = @" SELECT *  FROM HeTHongTK where SoHieu= ? ";
                var parameterss = new OleDbParameter[]
                {
                new OleDbParameter("?",txtTaikhoan.Text)
                   };
                var kq = ExecuteQuery(query, parameterss);
                if(kq.Rows.Count==0)
                {
                    XtraMessageBox.Show("Mã tài khoản không hợp lệ");
                    return;
                }
                else
                {
                    matk = int.Parse(kq.Rows[0]["MaTC"].ToString());
                }
                    query = @"
        INSERT INTO TP154 (MaPhanLoai,SoHieu,TenVattu,DonVi,GhiChu,DK,MaTK)
        VALUES (?,?,?,?,?,?,?)";
                int selectedId = 0;
                var selectedItem = comboBoxEdit1.SelectedItem as Item;
                if (selectedItem.Id == 0)
                {
                    XtraMessageBox.Show("Vui lòng chọn danh mục");
                    return;
                }
                if (selectedItem != null)
                {
                    selectedId = selectedItem.Id; // Lấy giá trị Id  
                }

                // Khai báo mảng tham số với đủ 10 tham số
                OleDbParameter[] parameters = new OleDbParameter[]
                {
        new OleDbParameter("?", selectedId),
          new OleDbParameter("?", txtSohieu.Text),
        new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtTenvattu.Text)),
        new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtDonvi.Text)), 
          new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtGhichu.Text)),
             new OleDbParameter("?",tk),
               new OleDbParameter("?",matk),
                };

                // Thực thi truy vấn và lấy kết quả
                int a = ExecuteQueryResult(query, parameters);
                LoadData(selectedId, txtSearch.Text);
            }
            else
            {
                double matk = 0;
                double tk = 0;
                string query = @" SELECT *  FROM HeTHongTK where SoHieu= ? ";
                var parameterss = new OleDbParameter[]
                {
                new OleDbParameter("?",txtTaikhoan.Text)
                   };
                var kq = ExecuteQuery(query, parameterss);
                if (kq.Rows.Count == 0)
                {
                    XtraMessageBox.Show("Mã tài khoản không hợp lệ");
                    return;
                }
                else
                {
                    matk = int.Parse(kq.Rows[0]["MaTC"].ToString());
                }
                query = @"
        Update TP154 set SoHieu=?,TenVattu=?,DonVi=?,GhiChu=?,MaTK=? where MaSo=?";
                  
                // Khai báo mảng tham số với đủ 10 tham số
                OleDbParameter[] parameters = new OleDbParameter[]
                { 
          new OleDbParameter("?", txtSohieu.Text),
        new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtTenvattu.Text)),
        new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtDonvi.Text)),
          new OleDbParameter("?", Helpers.ConvertUnicodeToVni(txtGhichu.Text)), 
               new OleDbParameter("?",matk),
                 new OleDbParameter("?",txtId.Text),
                };

                // Thực thi truy vấn và lấy kết quả
                int a = ExecuteQueryResult(query, parameters);
                int selectedId = 0;
                var selectedItem = comboBoxEdit1.SelectedItem as Item; 
                if (selectedItem != null)
                {
                    selectedId = selectedItem.Id; // Lấy giá trị Id  
                }
                LoadData(selectedId, txtSearch.Text);
            }
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        bool isAdd = true;
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            isAdd = true;
            txtId.Text = "0";
            txtSohieu.Text = "";
            txtTenvattu.Text = "";
            txtDonvi.Text = "";
            txtDonvi.Text = "";
            txtGhichu.Text = "";
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            DialogResult result = XtraMessageBox.Show("Bạn có chắc muốn xoá công trình này?",
       "Xác Nhận",
       MessageBoxButtons.YesNo,
       MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                //Phải kiểm tra xem công trình này có phát sinh chưa
                string query = @" SELECT *  FROM TP154 where MaSo= ? ";
                var parameterss = new OleDbParameter[]
                {
                new OleDbParameter("?",txtId.Text)
                   };

                var kq = ExecuteQuery(query, parameterss);
                int maso = kq.Rows[0].Field<int>("MaSo");

                query = @"select * from ChungTu  where MaTP=?";
                 var parameters = new OleDbParameter[]
                {
                 new OleDbParameter("?",txtId.Text),
                };
                kq = ExecuteQuery(query, parameters);

                if (kq.Rows.Count > 0)
                {
                    XtraMessageBox.Show("Tài khoản đã có phát sinh trong chứng từ, không thể xoá!");
                    return;
                }

                query = @" Delete from TP154  where MaSo=?";
                // Khai báo mảng tham số với đủ 10 tham số
                 parameters = new OleDbParameter[]
                {
                 new OleDbParameter("?",txtId.Text),
                };
                int a = ExecuteQueryResult(query, parameters);
                LoadData(0, txtSearch.Text);
            }

        }

        private void gridView1_RowClick(object sender, RowClickEventArgs e)
        {
            // Lấy chỉ số hàng đã click
            int rowHandle = e.RowHandle;

            // Lấy dữ liệu từ hàng
            var value = gridView1.GetRowCellValue(rowHandle, "SoHieu").ToString();
            txtSohieu.Text = value;
            txtTenvattu.Text = gridView1.GetRowCellValue(rowHandle, "TenVattu").ToString();
            txtDonvi.Text = gridView1.GetRowCellValue(rowHandle, "DonVi").ToString();
            txtGhichu.Text = gridView1.GetRowCellValue(rowHandle, "GhiChu").ToString();
            txtId.Text = gridView1.GetRowCellValue(rowHandle, "MaSo").ToString();
            var matc = gridView1.GetRowCellValue(rowHandle, "MaTK").ToString();
            string query = @" SELECT *  FROM HeTHongTK where MaTC= ? ";
            var parameterss = new OleDbParameter[]
            {
                new OleDbParameter("?",matc) 
               };
            var kq = ExecuteQuery(query, parameterss);
            txtTaikhoan.Text = kq.Rows[0]["SoHieu"].ToString();
            // Thực hiện hành động với dữ liệu
            isAdd = false;
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
            if (comboBoxEdit1.SelectedItem != null)
            {
                // Lấy phần tử được chọn
                var selectedItem = comboBoxEdit1.SelectedItem as Item;

                if (selectedItem != null)
                {
                    int selectedId = selectedItem.Id; // Lấy giá trị Id 
                    frmMain.currentselectId = comboBoxEdit1.SelectedIndex;
                    LoadData(selectedId, txtSearch.Text);
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            var selectedItem = comboBoxEdit1.SelectedItem as Item;
            int selectedId = selectedItem.Id; // Lấy giá trị Id 
            LoadData(selectedId, txtSearch.Text);
        }

        private void txtSearch_EditValueChanged(object sender, EventArgs e)
        {

        }
    }
}