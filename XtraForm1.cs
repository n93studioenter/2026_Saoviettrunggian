using DevExpress.XtraEditors;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SaovietTax.Database;
using SaovietTax.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Media.Protection.PlayReady;
using static SaovietTax.frmMain;
using Keys = OpenQA.Selenium.Keys;
namespace SaovietTax
{
    public partial class XtraForm1 : DevExpress.XtraEditors.XtraForm
    {
        public async Task<DataTable> ExecuteQueryAsync(string query, params OleDbParameter[] parameters)
        {
            DataTable dataTable = new DataTable();
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
            string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");
            try
            {
                string content = File.ReadAllText(filePaths); // Đọc file bất đồng bộ
                dbPath = content;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi đọc file: " + ex.Message);
                return dataTable; // Trả về DataTable rỗng trong trường hợp lỗi
            }

            string connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password=1@35^7*9)1;";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync(); // Mở kết nối bất đồng bộ
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
                            await Task.Run(() => dataAdapter.Fill(dataTable)); // Đổ dữ liệu vào DataTable
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
        public XtraForm1()
        {
            InitializeComponent();
        }
        string dbPath = "";
        public string savedPath = "";
        string mstcongty = "";
        string tokken = "";
        List<KhachHang> tbKhachhang = new List<KhachHang>();
        List<HoaDon> tbHoaDon = new List<HoaDon>();
        List<ChungTu> tbChungTu = new List<ChungTu>();
        List<DTO.VatTu> tbVattu = new List<DTO.VatTu>();
        List<PhanLoaiVattu> ListPhanloaiVattu = new List<PhanLoaiVattu>();
        public async Task<List<DTO.VatTu>> LoadDataVattuAsync()
        {
            // Hiển thị popup loading
            List<DTO.VatTu> lstVattu = new List<DTO.VatTu>();

            try
            {
                // 1. Lấy danh sách VatTu từ database

                var queryVatTu = @"SELECT MaSo,MaPhanLoai,SoHieu,TenVattu,DonVi,GhiChu FROM Vattu";
                var ListVattu = await Task.Run(() => ExecuteQueryAsync(queryVatTu, null));

                // 2. Chuyển đổi chuỗi VNI sang Unicode (nếu cần)
                foreach (DataRow item in ListVattu.Rows)
                {
                    item["TenVattu"] = Helpers.ConvertVniToUnicode(item["TenVattu"].ToString());
                    item["DonVi"] = Helpers.ConvertVniToUnicode(item["DonVi"].ToString());
                }

                // 3. Gom nhóm tất cả MaVatTu để query TonKho 1 lần duy nhất (Batch Query)
                var maVatTuList = ListVattu.Rows
                    .Cast<DataRow>()
                    .Select(row => int.Parse(row["MaSo"].ToString()))
                    .Distinct()
                    .ToList();

                // 4. Lấy dữ liệu TonKho theo danh sách MaVatTu đã gom nhóm
                var queryTonKhoBatch = @"SELECT * FROM TonKho WHERE MaVatTu IN (" +
                                       string.Join(",", maVatTuList) + ")";
                var allTonKho = await Task.Run(() => ExecuteQueryAsync(queryTonKhoBatch, null));

                // 5. Chuyển dữ liệu TonKho thành Dictionary để truy cập nhanh bằng MaVatTu
                var tonKhoDict = allTonKho.Rows
                    .Cast<DataRow>()
                    .GroupBy(row => int.Parse(row["MaVatTu"].ToString()))
                    .ToDictionary(group => group.Key, group => group.First());

                // 6. Xử lý từng VatTu và ánh xạ dữ liệu TonKho tương ứng
                List<Task<DTO.VatTu>> vatTuTasks = new List<Task<DTO.VatTu>>();
                foreach (DataRow item in ListVattu.Rows)
                {
                    vatTuTasks.Add(Task.Run(() =>
                    {
                        var VatTu = new DTO.VatTu
                        {
                            MaSo = int.Parse(item["MaSo"].ToString()),
                            MaPhanLoai = int.Parse(item["MaPhanLoai"].ToString()),
                            TenVattu = item["TenVattu"].ToString(),
                            SoHieu = item["SoHieu"].ToString(),
                            DonVi = item["DonVi"].ToString(),
                            GhiChu = item["GhiChu"].ToString(),
                            TenMaPhanLoai = ListPhanloaiVattu.Where(m => m.MaSo.ToString() == item["MaPhanLoai"].ToString()).FirstOrDefault().TenPhanLoai
                        };

                        // Kiểm tra và lấy dữ liệu từ TonKho (nếu có)
                        if (tonKhoDict.TryGetValue(VatTu.MaSo, out DataRow tonKhoRow))
                        {
                            int cnt = 12;
                            while (cnt > 0 && tonKhoRow["Luong_" + cnt].ToString() == "0")
                            {
                                cnt--;
                            }

                            // Lấy số lượng và thành tiền
                            var soluong = tonKhoRow["Luong_" + cnt] != DBNull.Value
                                ? double.Parse(tonKhoRow["Luong_" + cnt].ToString())
                                : 0;
                            VatTu.SoLuong = soluong;

                            var thanhtien = tonKhoRow["Tien_" + cnt] != DBNull.Value
                                ? double.Parse(tonKhoRow["Tien_" + cnt].ToString())
                                : 0;
                            VatTu.ThanhTien = thanhtien;

                            // Tính đơn giá nếu có dữ liệu
                            if (soluong != 0 && thanhtien != 0)
                            {
                                VatTu.Dongia = thanhtien / soluong;
                            }
                        }
                        return VatTu;
                    }));
                }

                // 7. Đợi tất cả các Task hoàn thành và thêm vào danh sách kết quả
                var vatTus = await Task.WhenAll(vatTuTasks);
                lstVattu.AddRange(vatTus);
                //  XtraMessageBox.Show("Load vattu thanh cong");
            }
            catch (Exception ex)
            {
                // Xử lý lỗi (có thể log hoặc hiển thị thông báo)
                Console.WriteLine($"Lỗi khi tải dữ liệu: {ex.Message}");
                throw; // Re-throw nếu cần thiết
            }
            finally
            {
                // Đóng popup loading chỉ khi mọi thứ đã hoàn tất
            }

            return lstVattu;
        }
        public async Task LoadKhachhang()
        {
            string sql = "Select MaSo ,MaPhanLoai ,SoHieu ,Ten ,DiaChi ,MST  from khachhang";
            DataTable kq = await ExecuteQueryAsync(sql, null);
            List<string> dkvh = new List<string> { "Ten", "DiaChi" };
            tbKhachhang = ConvertDatatableToList<KhachHang>(kq, dkvh);
        }
        public async Task LoadHoaDon()
        {
            string sql = "Select MaSo ,MaKhachHang  ,KyHieu  ,SoHD  ,NgayPH  ,ThanhTien   from HoaDon";
            DataTable kq = await ExecuteQueryAsync(sql, null);
            tbHoaDon = ConvertDatatableToList<HoaDon>(kq, null);
        }
        public async Task LoadChungTu()
        {
            string sql = "sElect MaSo ,MaCT ,MaLoai ,ThangCT ,SoHieu ,NgayCT ,NgayGS ,NgayTL ,DienGiai ,MaTKNo ,MaTKCo ,SoPS ,MaTKTCNo ,MaTKTCCo ,MaVattu  from ChungTu";
            DataTable kq = await ExecuteQueryAsync(sql, null);
            tbChungTu = ConvertDatatableToList<ChungTu>(kq, null);
        }
        public async Task LoadPhanloaiVatTu()
        {
            string sql = "Select MaSo ,SoHieu ,TenPhanLoai from PhanLoaiVattu";
            DataTable kq = await ExecuteQueryAsync(sql, null);
            List<string> dkvh = new List<string> { "TenPhanLoai" };
            ListPhanloaiVattu = ConvertDatatableToList<PhanLoaiVattu>(kq, dkvh);
        }
        public async Task LoadCtyInfo()
        {
            string query = "SELECT * FROM tbRegister";
            var kq = await ExecuteQueryAsync(query, null);
            try
            {
                if (kq.Rows.Count > 0)
                {
                    savedPath = kq.Rows[0]["Hoadonpath"].ToString();
                    txtuser.Text = kq.Rows[0]["Username"].ToString();
                    txtpass.Text = kq.Rows[0]["Password"].ToString();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
            ControlsSetup();
        }
        private void ControlsSetup()
        {
            try
            {
                dtTungay.DateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                dtDenngay.DateTime = DateTime.Now;
            }
            catch (Exception ex)
            {
            }

            progressPanel1.Caption = "Đang xử lý...";
            progressPanel1.Description = "Vui lòng chờ...";
        }

        public List<T> ConvertDatatableToList<T>(DataTable dt, List<string> dkvh) where T : new()
        {
            List<T> list = new List<T>();

            foreach (DataRow row in dt.Rows)
            {
                T item = new T();
                foreach (var prop in typeof(T).GetProperties())
                {
                    if (dt.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                    {
                        if (dkvh != null && dkvh.Contains(prop.Name))
                        {
                            prop.SetValue(item, Helpers.ConvertVniToUnicode(row[prop.Name].ToString()));
                        }
                        else
                        {
                            prop.SetValue(item, row[prop.Name]);
                        }

                    }
                }
                list.Add(item);
            }

            return list;
        }
        public async Task LoadDanhsachdataAsync()
        {

            await LoadPhanloaiVatTu();
            tbVattu = await LoadDataVattuAsync();
            await LoadKhachhang();
            await LoadHoaDon();
            await LoadChungTu();
        }

        string connectionString;
        #region IniDB
        private async Task InitDB()
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
                    Application.DoEvents();
                    string query = "SELECT * FROM License";

                    // Tạo mảng tham số với giá trị cho câu lệnh SQL

                    var kq = await ExecuteQueryAsync(query, null);
                    if (kq.Rows.Count > 0)
                    {
                        string tencongty = kq.Rows[0]["TenCty"].ToString();
                        string fileName = Path.GetFileName(dbPath.Trim());
                        mstcongty = kq.Rows[0]["MaSoThue"].ToString();
                        lblDpPath.Text = Helpers.ConvertVniToUnicode(tencongty) + "|" + mstcongty + "|" + fileName + " | " + "Version 3.62";

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

                string tableHoadonvao = "tableHoadonvao";
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
                else
                {
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
                Application.DoEvents();
            }

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
        bool is5111 = false;
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
        #endregion

        private async void XtraForm1_Load(object sender, EventArgs e)
        {
            InitDB();
            LoadCtyInfo();
            await LoadDanhsachdataAsync();
        }

        #region  Tai co quan thuế
        public class SvgConverter
        {
            public void ConvertBase64ToSvg(string base64Data, string outputPath)
            {
                // Tách phần đầu để lấy dữ liệu Base64
                var base64 = base64Data.Substring(base64Data.IndexOf(",") + 1);

                // Giải mã dữ liệu Base64
                byte[] svgBytes = Convert.FromBase64String(base64);

                // Lưu vào tệp SVG
                File.WriteAllBytes(outputPath, svgBytes);
            }
        }

        private void Testimg2(string base64data)
        {
            string base64Data = base64data;
            string outputPath = AppDomain.CurrentDomain.BaseDirectory + "output.svg";

            SvgConverter converter = new SvgConverter();
            converter.ConvertBase64ToSvg(base64Data, outputPath);

            Console.WriteLine("Tệp SVG đã được lưu tại: " + outputPath);
            RunMain();
            var readcapcha = Readcapcha();
        }
        private void RunMain()
        {
            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "main.exe");

            try
            {
                // Kiểm tra xem tệp có tồn tại không
                if (!File.Exists(exePath))
                {
                    Console.WriteLine("Tệp main.exe không tồn tại.");
                    return;
                }

                // Tạo một Process để chạy tệp .exe
                Process process = new Process();
                process.StartInfo.FileName = exePath;
                process.StartInfo.UseShellExecute = false; // Không sử dụng shell để chạy
                process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory; // Đặt thư mục làm việc

                process.Start(); // Bắt đầu tiến trình
                Thread.Sleep(2000); // Đợi 2 giây 

                // Đóng tiến trình
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show("Tệp không tìm thấy: " + ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("Không có quyền truy cập: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi khác xảy ra: " + ex.Message);
            }
        }
        private string Readcapcha()

        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "captcha.txt"; // Đảm bảo tệp ở cùng thư mục với chương trình

            try
            {
                // Đọc nội dung từ tệp
                string content = File.ReadAllText(filePath);
                Console.WriteLine("Nội dung của captcha.txt:");
                Console.WriteLine(content);
                return content; // Trả về nội dung đã đọc
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Tệp không tồn tại.");
                return null; // Hoặc trả về một giá trị mặc định nếu tệp không tồn tại
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message);
                return null; // Hoặc trả về một giá trị mặc định
            }
        }

        public static ChromeDriver Driver { get; private set; }

        #endregion
        private void btnTaiCQThue_Click(object sender, EventArgs e)
        {
            Driver = null;
            if (Driver == null)
            {
                var options = new ChromeOptions();
                // Tắt các cảnh báo bảo mật (Safe Browsing) 

                // Tắt Safe Browsing và các tính năng bảo mật can thiệp
                options.AddArgument("--disable-features=SafeBrowsing,DownloadBubble,DownloadNotification");
                options.AddArgument("--safebrowsing-disable-extension-blacklist");
                options.AddArgument("--safebrowsing-disable-download-protection");

                options.AddUserProfilePreference("download.prompt_for_download", false);
                options.AddUserProfilePreference("safebrowsing.enabled", false);
                options.AddUserProfilePreference("safebrowsing.disable_download_protection", true);
                // Tối ưu hóa trình duyệt
                //options.AddArgument("--headless");  // Chạy không giao diện
                //options.AddArgument("--disable-gpu");
                //options.AddArgument("--no-sandbox");

                options.AddArguments(
                  "--disable-notifications",   // Tắt thông báo
                   "--start-maximized",         // Khởi động ở chế độ tối đa
                  "--disable-extensions",      // Tắt các tiện ích mở rộng
                   "--disable-infobars");       // Tắt thông báo thông tin
                //
                string downloadPath = "";
                if (chkDauvao.Checked)
                {
                    downloadPath = savedPath + "\\HDVao";
                }
                if (chkDaura.Checked)
                {
                    downloadPath = savedPath + "\\HDRa";
                }
                options.AddUserProfilePreference("download.default_directory", downloadPath);
                options.AddUserProfilePreference("download.prompt_for_download", false);
                options.AddUserProfilePreference("disable-popup-blocking", "true");
                options.AddUserProfilePreference("safebrowsing.disable_download_protection", true);
                options.AddUserProfilePreference("safebrowsing.enabled", false); // Tắt Safe Browsing hoàn toàn
                var driverPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                ChromeDriverService chromeService = ChromeDriverService.CreateDefaultService(driverPath);
                chromeService.HideCommandPromptWindow = true; // Để ẩn cửa sổ CMD của driver


                Driver = new ChromeDriver(chromeService, options);
                //
                try
                {
                    Driver.Navigate().GoToUrl("https://hoadondientu.gdt.gov.vn");

                    IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
                    js.ExecuteScript("window.scrollTo(0, 0);");
                    Thread.Sleep(1000);
                    var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(100));
                    var closeButton = wait.Until(driver => driver.FindElement(By.XPath("//span[@class='ant-modal-close-x']")));
                    closeButton.Click();
                    //
                    var loginButton = wait.Until(driver => driver.FindElement(By.XPath("//div[@class='ant-col home-header-menu-item']/span[text()='Đăng nhập']")));
                    loginButton.Click();
                    var usernameField = Driver.FindElement(By.Id("username"));
                    var passwordField = Driver.FindElement(By.Id("password"));
                    string username = txtuser.Text;
                    string password = txtpass.Text;
                    usernameField.SendKeys(username);
                    passwordField.SendKeys(password);


                    // In ra src của thẻ img
                    try
                    {

                        var imgElement = Driver.FindElements(By.XPath("//img[contains(@src, 'data:image')]"));
                        while (imgElement.Count < 2)
                        {
                            imgElement = Driver.FindElements(By.XPath("//img[contains(@src, 'data:image')]"));
                        }
                        string src = imgElement[1].GetAttribute("src");
                        while (string.IsNullOrEmpty(src))
                        {
                            src = imgElement[1].GetAttribute("src");
                        }
                        new Actions(Driver)
   .KeyDown(Keys.Tab).KeyUp(Keys.Tab)  // Tab lần 1
   .Pause(TimeSpan.FromMilliseconds(100))  // Đợi ngắn
   .KeyDown(Keys.Tab).KeyUp(Keys.Tab)  // Tab lần 2
   .Perform();

                        //Tìm capcha

                        var cvalue = Driver.FindElements(By.Id("cvalue"));

                        Testimg2(src);
                        Thread.Sleep(300);
                        string recap = Readcapcha();
                        cvalue[1].SendKeys(recap);
                        Thread.Sleep(300);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    loginButton = Driver.FindElement(By.XPath("//button[contains(span/text(), 'Đăng nhập')]"));
                    loginButton.Click();
                    wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(200));
                    wait.Until(d =>
                    d.FindElements(By.XPath("//div[contains(@class,'home-header-menu-item')]//span[text()='Đăng nhập']")).Count == 0);

                    var cookies = Driver.Manage().Cookies.AllCookies.Where(m => m.Name == "jwt");

                    foreach (var cookie in cookies)
                    {
                        Console.WriteLine($"Name: {cookie.Name}, Value: {cookie.Value}");
                        //Lưu tokken
                        string query = "UPDATE tbRegister SET tokken=? ";
                        var parametersss = new OleDbParameter[]
                        {
                            new OleDbParameter("?", cookie.Value),
                        };
                        int a = ExecuteQueryResult(query, parametersss);
                        tokken = cookie.Value;

                        try
                        {
                            XuLyTaiDauRaMayTinhTien(tokken);
                            Driver.Quit();
                        }
                        catch (Exception ex)
                        {

                        }

                    }

                }
                catch (Exception ex)
                {
                    // Driver.Close();
                    //this.Close();
                    MessageBox.Show($"Lỗi: {ex.Message}");
                }
            }
        }

        public async Task XuLyTaiDauRaMayTinhTien(string tokken)
        {
            using (var client = new HttpClient())
            {
                string formattedDate1 = dtTungay.DateTime.ToString("dd/MM/yyyyTHH:mm:ss");
                string formattedDate2 = dtDenngay.DateTime.ToString("dd/MM/yyyyTHH:mm:ss");
                string url = $@"https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&size=50&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2}";
                // Thêm Bearer token vào Header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = TimeSpan.FromSeconds(30); // Thêm timeout để tránh treo ứng dụng
                HttpResponseMessage response = client.GetAsync(url).Result;
                int retryCount = 0;
                const int maxRetries = 3;

                while (retryCount < maxRetries)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        response.EnsureSuccessStatusCode();

                        // Đọc nội dung phản hồi
                        string responseBody = response.Content.ReadAsStringAsync().Result;
                        InvoiceRa2 rootObject;
                        try
                        {
                            rootObject = JsonConvert.DeserializeObject<InvoiceRa2>(responseBody);
                            if (rootObject.datas.Count > 0)
                            {
                                   retryCount++;
                                await XuLyTaiDauRaMayTinhTienDetail(rootObject, tokken, 5);
                                //Nếu có phân trang thì tải tiếp
                                break; // Thoát khỏi vòng lặp nếu thành công
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else
                    {
                        XtraMessageBox.Show("Tải hoá dơn từ máy tính tiền   thất bại");
                    }
                }
                if (retryCount == maxRetries)
                {
                    Console.WriteLine("Đã đạt số lần thử tối đa.");
                }

            }
        }
        public async Task XuLyTaiDauRaMayTinhTienDetail(InvoiceRa2 rootObject, string tokken, int invoceType)
        {
            foreach (var item in rootObject.datas)
            {
                string url = $"https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/export-xml?nbmst={item.nbmst}&khhdon={item.khhdon}&shdon={item.shdon}&khmshdon={1}";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokken);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream")); // Định dạng nhị phân
                    string pathravao = "HDRa";
                    string filename = $"{item.shdon}_{item.khhdon}.zip";
                    string path = Path.Combine(savedPath, pathravao, dtTungay.DateTime.Month.ToString(), filename);

                    int retryCount = 0;
                    const int maxRetries = 3;

                    while (retryCount < maxRetries)
                    {
                        try
                        {
                            await Task.Delay(300); // Đợi một chút trước khi gửi yêu cầu    
                            HttpResponseMessage response = await client.GetAsync(url);

                            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                            {
                                // Nếu mã trạng thái là 500, tăng số lần thử và tiếp tục
                                retryCount++;
                                Console.WriteLine($"Lỗi 500, thử lại lần {retryCount}...");
                                continue; // Quay lại đầu vòng lặp
                            }

                            response.EnsureSuccessStatusCode(); // Ném ngoại lệ nếu không thành công

                            // Đọc nội dung phản hồi dưới dạng byte
                            var fileBytes = await response.Content.ReadAsByteArrayAsync();

                            // Lưu file ZIP
                            File.WriteAllBytes(path, fileBytes); // Sử dụng WriteAllBytes
                            Console.WriteLine($"File ZIP đã được lưu tại: {path}");
                            ExtractZipXML(path);
                            break; // Thoát khỏi vòng lặp nếu thành công
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");
                            break; // Thoát khỏi vòng lặp nếu có lỗi khác
                        }
                    }

                    if (retryCount == maxRetries)
                    {
                        Console.WriteLine("Đã đạt số lần thử tối đa.");
                    }
                }
            }
        }
        private void ExtractZipXML(string path)
        {

            try
            {
                string rootPath = Path.GetDirectoryName(path);
                string getnamefile = Path.GetFileNameWithoutExtension(path);
                string directoryPath = rootPath + @"\Giainen" + "_" + getnamefile;

                ZipFile.ExtractToDirectory(path, directoryPath);
                var files = Directory.GetFiles(directoryPath, "invoice.html", SearchOption.AllDirectories);
                string targetFilePath = Path.Combine(rootPath, getnamefile + ".html"); 
                File.Move(files.FirstOrDefault(), targetFilePath);

                //Cho XML
                 files = Directory.GetFiles(directoryPath, "invoice.xml", SearchOption.AllDirectories);
                 targetFilePath = Path.Combine(rootPath, getnamefile + ".xml");
                File.Move(files.FirstOrDefault(), targetFilePath);

                File.Delete(path);
                Directory.Delete(directoryPath, true); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi giải nén hoặc xử lý file: {ex.Message}");
            }

        }
    }
}