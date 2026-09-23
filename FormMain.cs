using DevExpress.Pdf;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraReports.Design;
using Newtonsoft.Json;
using SaovietTax.Database;
using SaovietTax.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SaovietTax.frmMain;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace SaovietTax
{
    public partial class FormMain : DevExpress.XtraEditors.XtraForm
    {
        public FormMain()
        {
            InitializeComponent();
        }
        #region LoadControl
        private void ThietlapMenu()
        {
            MenuStrip menuStrip = new MenuStrip();
            ToolStripMenuItem xuLyMenuItem = new ToolStripMenuItem("Xử lý");
            menuStrip.Items.Add(xuLyMenuItem);
            ToolStripMenuItem xoaThangPhatSinhItem = new ToolStripMenuItem("Xoá phát sinh tháng");
            xuLyMenuItem.DropDownItems.Add(xoaThangPhatSinhItem);
            ToolStripMenuItem xoanguyenlieu = new ToolStripMenuItem("Xoá nguyên liệu");
            xuLyMenuItem.DropDownItems.Add(xoanguyenlieu);

            for (int i = 1; i <= 12; i++)
            {
                ToolStripMenuItem xoaItem = new ToolStripMenuItem($"Tháng {i}");
                xoaItem.Click += (sender2, e2) =>
                {
                    // DeleteThang(int.Parse(xoaItem.ToString().Trim().Replace("Tháng", "")));
                };
                xoaThangPhatSinhItem.DropDownItems.Add(xoaItem);
            }


            ToolStripMenuItem item2 = new ToolStripMenuItem("Xoá phát sinh ngân hàng");
            xuLyMenuItem.DropDownItems.Add(item2);
            item2.Click += (sender2, e2) =>
            {
                //XoaDulieunganhang();
            };
            xoanguyenlieu.Click += (sender2, e2) =>
            {
                var query = @"delete from  tbNguyenLieuThanhPham";
                // var rowsAffected = ExecuteQueryResult(query, null);
                XtraMessageBox.Show($"Đã xóa dữ liệu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

        }
        private void LoadControl()
        {
            ThietlapMenu();
            //Thiết lập ngày tháng
            try
            {
                var culture = new CultureInfo("vi-VN");
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
                dtTungay.Properties.DisplayFormat.FormatString = "dd/MM/yyyy";
                dtTungay.Properties.EditFormat.FormatString = "dd/MM/yyyy";
                dtTungay.Properties.Mask.EditMask = "dd/MM/yyyy";
                dtTungay.DateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                dtDenngay.DateTime = DateTime.Now;

                dateEdit1.DateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                dateEdit2.DateTime = DateTime.Now;
            }
            catch (Exception ex)
            {
            }
        }
        private void dtTungay_EditValueChanged(object sender, EventArgs e)
        {
            DateTime selectedDate = dtTungay.DateTime;
            // Lấy ngày cuối cùng của tháng
            DateTime lastDay = new DateTime(selectedDate.Year, selectedDate.Month, DateTime.DaysInMonth(selectedDate.Year, selectedDate.Month));
            dtDenngay.DateTime = lastDay;
        }
        #endregion

        #region ConnectDB
        string password, connectionString;
        string dbPath = "";
        string mstcongty = "";
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
        public int ExecuteQueryResult(string query, params OleDbParameter[] parameters)
        {
            System.Data.DataTable dataTable = new System.Data.DataTable();

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
        private void InitDB()
        {
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


            // Đọc toàn bộ nội dung tệp
            string password = "1@35^7*9)1";
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";
            //connectionString = $@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";
            // connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database";
            //connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\S.T.E 25\S.T.E 25\DATA\importData.accdb;Persist Security Info=False";
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    lblThongbaos.Text = "Kết nối data thành công";
                    Application.DoEvents();
                    string query = "SELECT * FROM License";

                    // Tạo mảng tham số với giá trị cho câu lệnh SQL

                    var kq = ExecuteQuery(query, null);
                    if (kq.Rows.Count > 0)
                    {
                        string tencongty = kq.Rows[0]["TenCty"].ToString();
                        string fileName = Path.GetFileName(dbPath.Trim());
                        mstcongty = kq.Rows[0]["MaSoThue"].ToString();
                        lblDpPath.Text = Helpers.ConvertVniToUnicode(tencongty) + "|" + mstcongty + "|" + fileName + " | " + "Version " + "05/11/25";
                        
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }


                string tableName = "tbimport";
                string tableNamedetail = "tbimportdetail";
                string tableDinhdanh = "tbDinhdanhtaikhoan";
                string tableDinhdanhNganhang = "tbDinhdanhNganhang";
                string tableNganhang = "tbNganhang";
                string tbMatdinhghichu = "tbMatdinhghichu";
                string alterTableQuery = "ALTER TABLE TP154 ALTER COLUMN TenVattu TEXT;";
                string tableNhapkhotp = "tbNhapkhotp";
                string tableNhapkhonguyenlieu = "tbNhapkhonguyenlieu";
                string tableNhapkhotpchitiet = "tbNhapkhotpChitiet";
                string tableHoadonvao = "tableHoadonvao";
                string tbNguyenlieuTP = "tbNguyenLieuThanhPham";
                string tbLogs = "tbLogs";
                int rowsAffected = ExecuteQueryResult(alterTableQuery, null);

                // Kiểm tra xem bảng đã tồn tại hay không
                if (!TableExists(connection, tbMatdinhghichu))
                {
                    // Tạo bảng nếu chưa tồn tại
                    CreateTableMatdinhghichu(connection, tbMatdinhghichu);
                    Console.WriteLine($"Bảng '{tbMatdinhghichu}' đã được tạo thành công.");
                    //Tạo sẵn 1 dòng mặc đinh
                    var query = @"INSERT INTO tbMatdinhghichu (TK,Noidung) VALUES (?, ?)";
                    var parameters = new OleDbParameter[]
                     {
            new OleDbParameter("?", "131"),
             new OleDbParameter("?",""),
                     };
                    rowsAffected = ExecuteQueryResult(query, parameters);
                }

                if (!TableExists(connection, tbNguyenlieuTP))
                {
                    // Tạo bảng nếu chưa tồn tại
                    CreateTableNguyenLieuTP(connection, tbNguyenlieuTP);
                    Console.WriteLine($"Bảng '{tbNguyenlieuTP}' đã được tạo thành công.");
                }

                if (!TableExists(connection, tableNhapkhonguyenlieu))
                {
                    // Tạo bảng nếu chưa tồn tại
                    CreateTableNhapkhoNguyenLieu(connection, tableNhapkhonguyenlieu);
                    Console.WriteLine($"Bảng '{tableNhapkhonguyenlieu}' đã được tạo thành công.");
                }

                if (!TableExists(connection, tableNhapkhotp))
                {
                    // Tạo bảng nếu chưa tồn tại
                    CreateTableNhapkhoTP(connection, tableNhapkhotp);
                    Console.WriteLine($"Bảng '{tableNhapkhotp}' đã được tạo thành công.");
                }
                if (!TableExists(connection, tableNhapkhotpchitiet))
                {
                    // Tạo bảng nếu chưa tồn tại
                    CreateTableNhapkhoTPChitiet(connection, tableNhapkhotpchitiet);
                    Console.WriteLine($"Bảng '{tableNhapkhotpchitiet}' đã được tạo thành công.");
                }

                if (!TableExists(connection, tableHoadonvao))
                {
                    // Tạo bảng nếu chưa tồn tại
                    CreateTableHoadonvao(connection, tableHoadonvao);
                    Console.WriteLine($"Bảng '{tableHoadonvao}' đã được tạo thành công.");
                }
                if (!TableExists(connection, tableNganhang))
                {
                    // Tạo bảng nếu chưa tồn tại
                    CreateTableNganhang(connection, tableNganhang);
                    Console.WriteLine($"Bảng '{tableNganhang}' đã được tạo thành công.");
                }
                if (!TableExists(connection, tableDinhdanhNganhang))
                {
                    // Tạo bảng nếu chưa tồn tại
                    CreateTableDinhDanhNganhang(connection, tableDinhdanhNganhang);
                    Console.WriteLine($"Bảng '{tableDinhdanhNganhang}' đã được tạo thành công.");
                }
                if (!TableExists(connection, tableDinhdanh))
                {
                    // Tạo bảng nếu chưa tồn tại
                    CreateTableDinhDanh(connection, tableDinhdanh);
                    Console.WriteLine($"Bảng '{tableDinhdanh}' đã được tạo thành công.");
                }
                if (!ColumnExists(connection, "tbNguyenLieuThanhPham", "stt"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbNguyenLieuThanhPham", "stt", "NUMBER"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbRegister", "TimeTokken"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbRegister", "TimeTokken", "DATETIME"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbRegister", "tk154"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbRegister", "tk154", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbNhapkhotp", "Ghichu2"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbNhapkhotp", "Ghichu2", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbNhapkhotp", "SoHieu2"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbNhapkhotp", "SoHieu2", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbNhapkhonguyenlieu", "SoHieuTP"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbNhapkhonguyenlieu", "SoHieuTP", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbNhapkhonguyenlieu", "SoHieu"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbNhapkhonguyenlieu", "SoHieu", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbimportdetail", "Percent"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimportdetail", "Percent", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }

                if (!ColumnExists(connection, "Vattu", "TenVattu2"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "Vattu", "TenVattu2", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "Vattu", "PTGB"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "Vattu", "PTGB", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }

                if (!ColumnExists(connection, "tbimport", "Macdinhstatus"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "Macdinhstatus", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbimport", "Khmshdon"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "Khmshdon", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbimport", "idhoadon"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "idhoadon", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }

                if (!ColumnExists(connection, "tbDinhdanhtaikhoan", "Loai"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbDinhdanhtaikhoan", "Loai", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "License", "AutoNK"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "License", "AutoNK", "NUMBER"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "License", "CCCD"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "License", "CCCD", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "HoaDon", "pathInvoice"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "HoaDon", "pathInvoice", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "PhanLoaiVattu", "TKNo"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "PhanLoaiVattu", "TKNo", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "PhanLoaiVattu", "TKCo"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "PhanLoaiVattu", "TKCo", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "PhanLoaiVattu", "GhiChu"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "PhanLoaiVattu", "GhiChu", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbimport", "TgTCThue1"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "TgTCThue1", "NUMBER"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbimport", "TgTCThue2"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "TgTCThue2", "NUMBER"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbimport", "TgTCThue3"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "TgTCThue3", "NUMBER"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                //if (!ColumnExists(connection, "HeThongTK", "KyHieu"))
                //{
                //    // Nếu không tồn tại, thêm cột tkoco
                //    AddColumn(connection, "HeThongTK", "KyHieu", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                //}
                if (!ColumnExists(connection, "tbNganhang", "MaKH"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbNganhang", "MaKH", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbimport", "TVat3"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "TVat3", "NUMBER"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbimport", "TVat2"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "TVat2", "NUMBER"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbimport", "TVat"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "TVat", "NUMBER"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbimport", "Vat3"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "Vat3", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbimport", "Vat2"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "Vat2", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbDinhdanhNganhang", "TK2"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbDinhdanhNganhang", "TK2", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbDinhdanhNganhang", "SoHieu"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbDinhdanhNganhang", "SoHieu", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbNganhang", "Checked"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbNganhang", "Checked", "NUMBER"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbNganhang", "Status"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbNganhang", "Status", "NUMBER"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbNganhang", "TongTien2"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbNganhang", "TongTien2", "NUMBER"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                // Kiểm tra xem cột tkoco đã tồn tại hay chưa
                if (!ColumnExists(connection, "tbRegister", "tokken"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbRegister", "tokken", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                // Kiểm tra xem cột tkoco đã tồn tại hay chưa
                if (!ColumnExists(connection, "tbRegister", "col1"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbRegister", "col1", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbRegister", "col2"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbRegister", "col2", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbimport", "InvoiceType"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "InvoiceType", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbimport", "IsHaschild"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "IsHaschild", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                // Kiểm tra xem cột tkoco đã tồn tại hay chưa
                if (!ColumnExists(connection, "tbimport", "Path"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "Path", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbimport", "Type"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "Type", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbimport", "TPhi"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "TPhi", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbimport", "TgTCThue"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "TgTCThue", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbimport", "TgTThue"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "TgTThue", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!TableExists(connection, tableName))
                {
                    // Tạo bảng nếu chưa tồn tại
                    CreateTable(connection, tableName);
                    Console.WriteLine($"Bảng '{tableName}' đã được tạo thành công.");
                }
                else
                {
                    Console.WriteLine($"Bảng '{tableName}' đã tồn tại.");
                }
                // Kiểm tra xem bảng đã tồn tại hay không
                if (!TableExists(connection, tableNamedetail))
                {
                    // Tạo bảng nếu chưa tồn tại
                    CreateTableDetail(connection, tableNamedetail);
                    Console.WriteLine($"Bảng '{tableNamedetail}' đã được tạo thành công.");
                }
                else
                {
                    // Kiểm tra xem cột tkoco đã tồn tại hay chưa
                    if (!ColumnExists(connection, "tbimportdetail", "TKCo"))
                    {
                        // Nếu không tồn tại, thêm cột tkoco
                        AddColumn(connection, "tbimportdetail", "TKCo", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần
                        Console.WriteLine("Cột 'tkoco' đã được thêm vào bảng 'tbimportdetail'.");
                    }
                    else
                    {
                        Console.WriteLine("Cột 'tkoco' đã tồn tại trong bảng 'tbimportdetail'.");
                    }
                    //
                    if (!ColumnExists(connection, "tbimportdetail", "TTien"))
                    {
                        // Nếu không tồn tại, thêm cột tkoco
                        AddColumn(connection, "tbimportdetail", "TTien", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần
                        Console.WriteLine("Cột 'TTien' đã được thêm vào bảng 'tbimportdetail'.");
                    }
                    else
                    {
                        Console.WriteLine("Cột 'TTien' đã tồn tại trong bảng 'tbimportdetail'.");
                    }

                    Console.WriteLine($"Bảng '{tableNamedetail}' đã tồn tại.");
                }
                lblThongbaos.Text = "Kiem tra table thành công";
                Application.DoEvents();
            }

        }
        static void AddColumn(OleDbConnection connection, string tableName, string columnName, string dataType)
        {
            string sql = $"ALTER TABLE [{tableName}] ADD COLUMN [{columnName}] {dataType};";
            using (OleDbCommand command = new OleDbCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        static bool ColumnExists(OleDbConnection connection, string tableName, string columnName)
        {
            using (OleDbCommand command = new OleDbCommand($"SELECT TOP 1 * FROM [{tableName}]", connection))
            {
                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        static bool TableExists(OleDbConnection connection, string tableName)
        {
            try
            {
                // Kiểm tra sự tồn tại của bảng
                System.Data.DataTable schemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                foreach (DataRow row in schemaTable.Rows)
                {
                    if (row["TABLE_NAME"].ToString().Equals(tableName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi kiểm tra bảng: {ex.Message}");
            }
            return false;
        }
        static void CreateTableMatdinhghichu(OleDbConnection connection, string tableName)
        {
            string createTableQuery = $@"
        CREATE TABLE {tableName} (
            ID AUTOINCREMENT PRIMARY KEY,
            TK TEXT,
            Noidung TEXT 
        );";

            using (OleDbCommand command = new OleDbCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        static void CreateTableNganhang(OleDbConnection connection, string tableName)
        {
            string createTableQuery = $@"
        CREATE TABLE {tableName} (
            ID AUTOINCREMENT PRIMARY KEY,
            SHDon TEXT,
            NgayGD TEXT,
            DienGiai TEXT,
            TongTien NUMBER,
            TKNo TEXT,  
            TKCo TEXT 
        );";

            using (OleDbCommand command = new OleDbCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        static void CreateTableDinhDanh(OleDbConnection connection, string tableName)
        {
            string createTableQuery = $@"
        CREATE TABLE {tableName} (
            ID AUTOINCREMENT PRIMARY KEY,
            Type TEXT,
            KeyValue TEXT,
            TKNo TEXT,  
            TKCo TEXT,
            TKThue TEXT,
            Uutien TEXT
        );";

            using (OleDbCommand command = new OleDbCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        static void CreateTableDinhDanhNganhang(OleDbConnection connection, string tableName)
        {
            string createTableQuery = $@"
        CREATE TABLE {tableName} (
            ID AUTOINCREMENT PRIMARY KEY,  
            Noidung TEXT,
            TK TEXT 
        );";

            using (OleDbCommand command = new OleDbCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        static void CreateTableNhapkhoNguyenLieu(OleDbConnection connection, string tableName)
        {
            string createTableQuery = $@"
    CREATE TABLE {tableName} (
        ID AUTOINCREMENT PRIMARY KEY,  
        ParentId NUMBER,
        TTien NUMBER,
        SL NUMBER
    );";
            using (OleDbCommand command = new OleDbCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        static void CreateTableNhapkhoTP(OleDbConnection connection, string tableName)
        {
            string createTableQuery = $@"
    CREATE TABLE {tableName} (
        ID AUTOINCREMENT PRIMARY KEY,
        NgayLap DATETIME, 
        NgayTao DATETIME, 
        SoHieu TEXT,
        Ghichu TEXT,
        Status TEXT
    );";
            using (OleDbCommand command = new OleDbCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        static void CreateTableLogs(OleDbConnection connection, string tableName)
        {
            string createTableQuery = $@"
    CREATE TABLE {tableName} (
        ID AUTOINCREMENT PRIMARY KEY,  
        Message TEXT, 
        DateCreate DATETIME
    );";
            using (OleDbCommand command = new OleDbCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        static void CreateTableNguyenLieuTP(OleDbConnection connection, string tableName)
        {
            string createTableQuery = $@"
    CREATE TABLE {tableName} (
        ID AUTOINCREMENT PRIMARY KEY,  
        TPSoHieu TEXT,
        IDNguyenLieu NUMBER,
        SoHieuNguyenLieu TEXT,
        TiLe Number
    );";
            using (OleDbCommand command = new OleDbCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        static void CreateTableNhapkhoTPChitiet(OleDbConnection connection, string tableName)
        {
            string createTableQuery = $@"
    CREATE TABLE {tableName} (
        ID AUTOINCREMENT PRIMARY KEY,
        SoHieu TEXT, 
        SOLuong NUMBER, 
        DonGia NUMBER ,
        ParentID NUMBER
    );";
            using (OleDbCommand command = new OleDbCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        static void CreateTableHoadonvao(OleDbConnection connection, string tableName)
        {
            string createTableQuery = $@"
    CREATE TABLE {tableName} (
        ID AUTOINCREMENT PRIMARY KEY,
        NgayMua DATETIME,
        SoHieu TEXT,
        TenKH TEXT,
        MaSoHH TEXT,
        TenHH TEXT,
        SL NUMBER,
        DonGia NUMBER,
        ThanhTien NUMBER,
        Hinhthuc NUMBER,
        TKNo TEXT,
        TKCo TEXT,
        TKThue TEXT
    );";

            using (OleDbCommand command = new OleDbCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        static void CreateTable(OleDbConnection connection, string tableName)
        {
            string createTableQuery = $@"
        CREATE TABLE {tableName} (
            ID AUTOINCREMENT PRIMARY KEY,
            SHDon TEXT,
            KHHDon TEXT,
            NLap TEXT,
            Ten TEXT,
            Noidung TEXT,
            TKCo TEXT,
            TKNo TEXT,
            TkThue TEXT,
            Mst TEXT,
            Status NUMBER,
            Ngaytao TEXT,
            TongTien NUMBER,
            Vat NUMBER,
            SohieuTP TEXT
        );";

            using (OleDbCommand command = new OleDbCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        static void CreateTableDetail(OleDbConnection connection, string tableName)
        {
            string createTableQuery = $@"
        CREATE TABLE {tableName} (
            ID AUTOINCREMENT PRIMARY KEY,
            ParentId TEXT,
            SoHieu TEXT,
            SoLuong TEXT,
            DonGia TEXT,
            DVT TEXT,
            Ten TEXT ,
            MaCT TEXT,
            TKNo TEXT,
            TKCo TEXT
        );";

            using (OleDbCommand command = new OleDbCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        #endregion
        #region Tải hoá đơn cơ quan thuế
        string tokken = "";
        private void btnTaiHdCQT_Click(object sender, EventArgs e)
        {
            bool needLogin = true;
            string tokkenDB = "";
            string querykh = @" SELECT *  FROM tbRegister"; // Sử dụng ? thay cho @mst trong OleDb

          var  tbRegister = ExecuteQuery(querykh, new OleDbParameter("?", ""));
            string gettimeTokken = tbRegister.AsEnumerable().FirstOrDefault()["TimeTokken"].ToString();
            if (!string.IsNullOrEmpty(gettimeTokken))
            {
                var timpsan = DateTime.Now - DateTime.Parse(gettimeTokken);
                if (timpsan.TotalMinutes <= 10)
                {
                    needLogin = false;
                    this.tokken = tbRegister.AsEnumerable().FirstOrDefault().Field<string>("tokken");
                }
            }
            if (chkDauvao.Checked)
            {
                progressPanel1.Show();
                progressPanel1.Caption = "Đang tải hoá đơn đầu vào từ cơ quan thuế, vui lòng chờ...";
                Application.DoEvents();
            }
            if (chkDaura.Checked)
            {
                progressPanel2.Show();
                progressPanel2.Caption = "Đang tải hoá đơn đầu ra từ cơ quan thuế, vui lòng chờ...";
                Application.DoEvents();
            }
            if (needLogin == true)
            {
                using (var client = new HttpClient())
                {
                    try
                    {


                        HttpResponseMessage response = new HttpResponseMessage();
                        string url = "https://hoadondientu.gdt.gov.vn:30000/captcha";
                        try
                        {
                            response = client.GetAsync(url).Result;
                            response.EnsureSuccessStatusCode();
                        }
                        catch (Exception ex)
                        {
                            XtraMessageBox.Show(ex.Message);
                            return;
                        }
                        if (chkDauvao.Checked)
                        {
                            progressPanel1.Caption = "Đang giải mã capcha...";
                            Application.DoEvents();
                        }
                        if (chkDaura.Checked)
                        {
                            progressPanel2.Caption = "Đang giải mã capcha...";
                            Application.DoEvents();
                        }
                        //Đọc nội dung phản hồi
                        string responseBody = response.Content.ReadAsStringAsync().Result;
                        MyJson myJson = JsonConvert.DeserializeObject<MyJson>(responseBody);
                        //string filePath = "output.svg";
                        string filePath = AppDomain.CurrentDomain.BaseDirectory + "output.svg"; // Đảm bảo tệp ở cùng thư mục với chương trình
                                                                                                //Lưu chuỗi SVG vào tệp
                        File.WriteAllText(filePath, myJson.Content);
                        Thread.Sleep(2000);
                        if (chkDauvao.Checked)
                        {
                            progressPanel1.Caption = "Đang lấy thông tin tokken...";
                            Application.DoEvents();
                        }
                        if (chkDaura.Checked)
                        {
                            progressPanel2.Caption = "Đang lấy thông tin tokken...";
                            Application.DoEvents();
                        }
                        SvgCaptchaSolver solver = new SvgCaptchaSolver();
                        string result = solver.SolveCaptcha(filePath);

                        url = "https://hoadondientu.gdt.gov.vn:30000/security-taxpayer/authenticate";
                        var payload = new
                        {
                            username = txtuser.Text,
                            password = txtpass.Text,
                            cvalue = result,
                            ckey = myJson.Key
                        };
                        string json = JsonConvert.SerializeObject(payload);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        response = client.PostAsync(url, content).Result;
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {

                            XtraMessageBox.Show("Có lỗi đăng nhập hệ thống vui lòng thử lại");
                            if (chkDauvao.Checked)
                            {
                                progressPanel1.Visible = false;
                            }
                            else
                            {
                                progressPanel2.Visible = false;
                            }
                            return;
                        }

                        response.EnsureSuccessStatusCode();
                        Thread.Sleep(1000);
                        responseBody = response.Content.ReadAsStringAsync().Result;
                        var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);
                        this.tokken = tokenResponse.token;
                        string query = @"UPDATE tbRegister SET TimeTokken=? ";

                        var parameters = new OleDbParameter[]
                 {
                                new OleDbParameter("?",DateTime.Now.ToString())
                 };
                        int rowsAffected = ExecuteQueryResult(query, parameters);

                        if (chkDauvao.Checked)
                        {
                            progressPanel1.Caption = "Bắt đầu tải hoá đơn...";
                            Application.DoEvents();
                        }
                        if (chkDaura.Checked)
                        {
                            progressPanel2.Caption = "Bắt đầu tải hoá đơn...";
                            Application.DoEvents();
                        }
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show(ex.Message);
                        return;
                    }

                }
            }


            // this.tokken = "eyJhbGciOiJIUzUxMiJ9.eyJzdWIiOiIzNTAyNTUxODU2IiwidHlwZSI6MiwiZXhwIjoxNzYyNDIwNjMxLCJpYXQiOjE3NjIzMzQyMzF9.FC4eRyCg3iPEQzYksC0jI3vi-3hEIRE8c0ZSfVRrin9PJvGhHgGOzrIHMibQYKEpR0SnSCPdSn-wuqyJQl-tZQ";
            frmTaiCoQuanThue frmTaiCoQuanThue = new frmTaiCoQuanThue();
            
            frmTaiCoQuanThue.Show();
            Application.DoEvents();
            progressPanel1.Visible = false;
            progressPanel2.Visible = false;
            frmTaiCoQuanThue.Close();
            XtraMessageBox.Show("Đã tải xong hoá đơn, vui lòng kiểm tra thông tin!");
            btnChonthang.PerformClick();
        }

        #endregion

        private void FormMain_Load(object sender, EventArgs e)
        {
            //Thiết lập menu, combobox, dateedit...
            LoadControl();
            InitDB();
        }
    }
}