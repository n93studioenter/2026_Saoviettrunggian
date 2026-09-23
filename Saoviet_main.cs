using DevExpress.XtraCharts.Design;
using DevExpress.XtraEditors;
using DevExpress.XtraMap.Native;
using Newtonsoft.Json;
using SaovietTax.Database;
using SaovietTax.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Media.Protection.PlayReady;
using static SaovietTax.frmMain;

namespace SaovietTax
{
    public partial class Saoviet_main : DevExpress.XtraEditors.XtraForm
    {
        public Saoviet_main()
        {
            InitializeComponent();
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Globalization.CultureInfo culture2 = new System.Globalization.CultureInfo("vi-VN");
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture2;
            this.KeyPreview = true; // Đặt KeyPreview thành true
        }

        #region Helpers
        public static class GDTClient
        {
            private static readonly HttpClient _client;

            static GDTClient()
            {
                var handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    UseProxy = false
                };

                _client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(40) };

                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.ConnectionClose = false; // Keep-Alive
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            }

            public static void UpdateToken(string token)
                => _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            public static async Task<string> GetJsonAsync(string url, int maxRetries = 3)
            {
                for (int i = 0; i <= maxRetries; i++)
                {
                    try
                    {
                        var sw = Stopwatch.StartNew();
                        var response = await _client.GetAsync(url);
                        string json = await response.Content.ReadAsStringAsync();
                        sw.Stop();

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"GDT OK → {sw.ElapsedMilliseconds}ms");
                            return json;
                        }

                        // 401 → token sai → không retry
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                            throw new UnauthorizedAccessException("Token hết hạn hoặc sai!");

                        // Các lỗi khác (500, 503…) → retry
                        Console.WriteLine($"GDT lỗi {response.StatusCode} → retry {i + 1}/{maxRetries}");
                    }
                    catch (TaskCanceledException) when (i < maxRetries)
                    {
                        Console.WriteLine($"Timeout → retry {i + 1}/{maxRetries}");
                    }
                    catch (Exception ex) when (i < maxRetries)
                    {
                        Console.WriteLine($"Lỗi mạng → retry {i + 1}/{maxRetries}: {ex.Message}");
                    }

                    if (i < maxRetries)
                        await Task.Delay(500 * (i + 1)); // backoff: 500ms, 1000ms, 1500ms
                }

                throw new Exception("Gọi API GDT thất bại sau nhiều lần thử");
            }
            public static void DownloadFile_Sync(string url, string savePath, string token = null)
            {
                if (!string.IsNullOrEmpty(token))
                    UpdateToken(token);

                Task.Run(() =>
                {
                    var sw = Stopwatch.StartNew();
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, url);
                        request.Headers.Accept.Clear();
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                        var response = _client.SendAsync(request).GetAwaiter().GetResult();
                        response.EnsureSuccessStatusCode();

                        using (var stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                        using (var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920))
                        {
                            stream.CopyTo(fs);
                        }

                        sw.Stop();
                        XtraMessageBox.Show($"Tải thành công!\n{Path.GetFileName(savePath)}\nThời gian: {sw.ElapsedMilliseconds} ms", "Thành công");
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show($"Lỗi tải file:\n{ex.Message}");
                    }
                });
            }
        }
        #endregion
        #region Fields
        string connectionString { get; set; }
        string dbPath { get; set; }

        //Khách hàng
        DataTable tbKhachhang { get; set; }
        private ILookup<string, KhachHang> khachhang_lookupBySoHieu;
        private ILookup<string, KhachHang> khachhang_lookupByMST;
        List<KhachHang> lstKhachhangs =new List<KhachHang>();

        //Vật tư
        List<DTO.VatTu> lstVattu = new List<DTO.VatTu>();
        private ILookup<string, DTO.VatTu> vattu_lookupten;
        DataTable ListPhanloaiVattu;

        List<HoadonCT> lstHoadonCT { get; set; }    = new List<HoadonCT>();
        // Vẫn dễ đọc, dễ hiểu, nhưng đúng bản chất
        public HashSet<(string MST, string SoHD, DateTime NgayPH)> lookupHoaDonCT { get; }
            = new HashSet<(string MST, string SoHD, DateTime NgayPH)>();

        DataTable tbimport { get; set; }
        List<TbImport> lsttbimport = new List<TbImport>();

        DataTable tbimportdetail { get; set; }
        List<TbImportDetail> lsttbimportdetail = new List<TbImportDetail>();

        DataTable tbreTbRegister { get; set; }
        TbRegister TbRegister { get; set; } 
        DataTable tbLicense { get; set; }
      SaovietTax.Database.License license { get; set; }   
        public string tokken { get; set; }
        public string mstcongty { get; set; }   
        public string savedPath { get; set; }   
        #endregion
        #region db config
        private void Getconnectionstring()
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
            string password = "1@35^7*9)1";
            //  connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.16.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";
            OleDbConnection connection = new OleDbConnection(connectionString);
            if (!TestConnection(connectionString))
            {
                // Nếu 16.0 không hoạt động, thử 12.0
                connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";

                if (!TestConnection(connectionString))
                {
                    Console.WriteLine("Cả hai provider đều không kết nối được");
                    connectionString = null; // Hoặc xử lý lỗi phù hợp
                }
            }
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
        private bool TestConnection(string connectionString)
        {
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        #endregion
        #region load db
        private void LoadDCustomer()
        {
            var query = "SELECT MaSo,MaPhanLoai,SoHieu,Ten ,DiaChi,MST   FROM KhachHang";
            tbKhachhang = ExecuteQuery(query);
            // GỌI EXTENSION METHOD ĐÚNG
            lstKhachhangs = tbKhachhang.ToList<KhachHang>();
            //khachhang_lookupBySoHieu = lstKhachhangs.ToLookup(kh => kh.SoHieu?.ToLower().Trim());
            //khachhang_lookupByMST = lstKhachhangs.ToLookup(kh => kh.MST?.ToLower().Trim());
        }
      
        private void LoadHoadonCT()
        {
            string query = @"
            SELECT 
                hd.*,ct.*
            FROM 
                Hoadon hd
            INNER JOIN 
                Chungtu ct ON hd.MaSo = ct.MaSo
            WHERE 
                hd.KyHieu <> '...'";
            var data= ExecuteQuery(query);
            foreach (DataRow item in data.Rows)
            {
                // Lấy trực tiếp các field cần thiết
                string soHD = item["SoHD"].ToString()?.Trim() ?? "";
                DateTime ngayPH = DateTime.Parse(item["NgayPH"].ToString()).Date;
                int maKhachHang = int.Parse(item["MaKhachHang"].ToString());

                // Lấy MST từ danh sách khách hàng (giả sử lstKhachhangs đã load sẵn)
                string mst = lstKhachhangs
                    .FirstOrDefault(m => m.MaSo == maKhachHang)?
                    .MST?.Trim() ?? "";

                // KIỂM TRA TRÙNG NGAY TẠI ĐÂY (nếu cần chặn khi import)
                var key = (mst, soHD, ngayPH);
                if (lookupHoaDonCT.Contains(key))
                {
                    // Hóa đơn trùng → báo lỗi hoặc bỏ qua
                    Console.WriteLine($"Trùng: {mst} - {soHD} - {ngayPH:dd/MM/yyyy}");
                    continue; // hoặc throw, hoặc ghi log...
                }

                // Nếu không trùng → thêm vào HashSet để lần sau kiểm tra
                lookupHoaDonCT.Add(key);

                // Nếu bạn VẪN CẦN lưu object HoadonCT để dùng sau (in, hiển thị, ký số...)
                // thì mới tạo và add vào list. Còn không thì BỎ QUA luôn bước này!
                // → 90% trường hợp kiểm tra trùng import: KHÔNG CẦN!
            }

            //Cách dùng    bool daTonTai = lookupHoaDonCT.Contains((mst, soHD, ngayPH.Date));
        }
        private void Loadtbimport()
        {
            var query = "SELECT *   FROM tbimport";
            tbimport = ExecuteQuery(query);
            // GỌI EXTENSION METHOD ĐÚNG
            try
            {
                lsttbimport = tbimport.ToList<TbImport>();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
        }
        private void Loadtbimportdetail()
        {
            var query = "SELECT *   FROM tbimportdetail";
            tbimportdetail = ExecuteQuery(query);
            // GỌI EXTENSION METHOD ĐÚNG
            try
            {
                lsttbimportdetail = tbimportdetail.ToList<TbImportDetail>();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);    
            }
        }
        private void LoadRegister()
        {
            var query = "SELECT *   FROM tbRegister";
            tbreTbRegister = ExecuteQuery(query);
            // GỌI EXTENSION METHOD ĐÚNG
            try
            {
                TbRegister = tbreTbRegister.ToList<TbRegister>().FirstOrDefault();
                if (TbRegister != null)
                {
                    txtuser.Text = TbRegister.Username;
                    txtpass.Text = TbRegister.Password;
                    savedPath = TbRegister.Hoadonpath;
                }   
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
          
        }
        private void LoadLicense()
        {
            var query = "SELECT *   FROM License";
            tbLicense = ExecuteQuery(query);
            // GỌI EXTENSION METHOD ĐÚNG
            try
            {
                license = tbLicense.ToList<SaovietTax.Database.License>().FirstOrDefault();
                if (license != null)
                {
                    mstcongty = license.MaSoThue;
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
        }
        public async Task<List<DTO.VatTu>> LoadDataVattuAsync()
        {
            // Hiển thị popup loading
            List<DTO.VatTu> lstVattu = new List<DTO.VatTu>();

            try
            {
                // 1. Lấy danh sách VatTu từ database

                var queryVatTu = @"SELECT * FROM Vattu";
                var ListVattu = await Task.Run(() => ExecuteQuery(queryVatTu, null));
                var queryMaphanloai = @"SELECT * FROM PhanLoaiVattu";
                ListPhanloaiVattu = await Task.Run(() => ExecuteQuery(queryMaphanloai, null));

                // 2. Chuyển đổi chuỗi VNI sang Unicode (nếu cần)
                foreach (DataRow item in ListVattu.Rows)
                {
                    item["TenVattu"] = Helpers.ConvertVniToUnicode(item["TenVattu"].ToString());
                    item["TenVattu2"] = Helpers.ConvertVniToUnicode(item["TenVattu2"].ToString());
                    item["DonVi"] = Helpers.ConvertVniToUnicode(item["DonVi"].ToString());
                }

                // 3. Gom nhóm tất cả MaVatTu để query TonKho 1 lần duy nhất (Batch Query)
                var maVatTuList = ListVattu.Rows
                    .Cast<DataRow>()
                    .Select(row => int.Parse(row["MaSo"].ToString()))
                    .Distinct()
                    .ToList();
                if (maVatTuList.Count == 0)
                    return new List<DTO.VatTu>();
                // 4. Lấy dữ liệu TonKho theo danh sách MaVatTu đã gom nhóm
                var queryTonKhoBatch = @"SELECT * FROM TonKho WHERE MaVatTu IN (" +
                                       string.Join(",", maVatTuList) + ")";
                var allTonKho = await Task.Run(() => ExecuteQuery(queryTonKhoBatch, null));

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
                            TenVattu2 = item["TenVattu2"].ToString(),
                            SoHieu = item["SoHieu"].ToString(),
                            DonVi = item["DonVi"].ToString(),
                            GhiChu = item["GhiChu"].ToString(),
                            TenMaPhanLoai = ListPhanloaiVattu.AsEnumerable().Where(m => m["MaSo"].ToString() == item["MaPhanLoai"].ToString()).FirstOrDefault()["TenPhanLoai"].ToString(),
                            PTGB = item["PTGB"].ToString(),
                        };

                        // Kiểm tra và lấy dữ liệu từ TonKho (nếu có)
                        if (tonKhoDict.TryGetValue(VatTu.MaSo, out DataRow tonKhoRow))
                        {
                            int cnt = 12;
                            //while (cnt > 0 && tonKhoRow["Luong_" + cnt].ToString() == "0")
                            //{
                            //    cnt--;
                            //}

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
        #endregion
        #region Init database
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
            // connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";


            connectionString = $@"Provider=Microsoft.ACE.OLEDB.16.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    Application.DoEvents();
                    string query = "SELECT * FROM License";

                    // Tạo mảng tham số với giá trị cho câu lệnh SQL

                    var kq = ExecuteQuery(query, null);
                    if (kq.Rows.Count > 0)
                    {
                        string tencongty = kq.Rows[0]["TenCty"].ToString();
                        string fileName = Path.GetFileName(dbPath.Trim());
                       
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
                string GhichuHT = "tbGhichuHT";
                int rowsAffected = ExecuteQueryResult(alterTableQuery, null);

                // Kiểm tra xem bảng đã tồn tại hay không
                if (!TableExists(connection, GhichuHT))
                {
                    CreateTableGhiChuHT(connection, GhichuHT);
                }
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
                if (!ColumnExists(connection, "tbimport", "hdon"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "hdon", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
                }
                if (!ColumnExists(connection, "tbimport", "IsImport"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "tbimport", "IsImport", "NUMBER"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
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
                if (!ColumnExists(connection, "HoaDon", "Ghichuhd"))
                {
                    // Nếu không tồn tại, thêm cột tkoco
                    AddColumn(connection, "HoaDon", "Ghichuhd", "TEXT"); // Bạn có thể thay đổi kiểu dữ liệu nếu cần 
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
        static void CreateTableGhiChuHT(OleDbConnection connection, string tableName)
        {
            string createTableQuery = $@"
        CREATE TABLE {tableName} (
            ID AUTOINCREMENT PRIMARY KEY,
            SoHD TEXT,
            NgayLap DATETIME,
            Noidung TEXT 
        );";

            using (OleDbCommand command = new OleDbCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
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
        #region Control, loaddata
        private async void LoadData()
        {
            LoadDCustomer();
            //Load danh sách vật tư
            lstVattu = await LoadDataVattuAsync();
            vattu_lookupten = lstVattu.ToLookup(kh => kh.TenVattu?.ToLower().Trim());
            LoadHoadonCT();
            Loadtbimport();
            LoadRegister();
        }
        private void LoadCommon()
        {
            SetVietnameseCulture();
           
           
        }
        private void SetVietnameseCulture()
        {
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("vi-VN");
            // var files = Directory.EnumerateFiles(savedPath + @"\HDVao", "*.xml", SearchOption.AllDirectories).ToList();

            try
            {
                var vietnamCulture = new CultureInfo("vi-VN");
                dtTungay.Properties.DisplayFormat.FormatString = "dd/MM/yyyy";
                dtTungay.Properties.EditFormat.FormatString = "dd/MM/yyyy";
                dtTungay.Properties.Mask.EditMask = "dd/MM/yyyy";
                dtTungay.DateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                dtDenngay.DateTime = DateTime.Now;
                dateEdit1.DateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                dateEdit2.DateTime = DateTime.Now;

                dateEdit1.Properties.DisplayFormat.Format = vietnamCulture.DateTimeFormat;
                dateEdit1.Properties.EditFormat.Format = vietnamCulture.DateTimeFormat;
                 
                dateEdit1.Properties.VistaDisplayMode = DevExpress.Utils.DefaultBoolean.True; // lịch đẹp
                dateEdit1.Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.True;
                dateEdit1.Properties.NullText = "Chọn ngày...";
            }
            catch (Exception ex)
            {
            }

            progressPanel1.Caption = "Đang xử lý...";
            //progressPanel1.Description = "Vui lòng chờ...";
        }
        #endregion
        private async void Saoviet_main_Load(object sender, EventArgs e)
        {
            Getconnectionstring();

            //Tạo hoặc thêm mới table, field
            InitDB();

            //Load danh sách data từ database
            LoadData();
            LoadCommon();
        }
        #region Event


        /// <summary>
        /// Lấy tokken từ trang cơ quan thuế
        /// </summary>

        private void GetTokken()
        {
            progressPanel1.Show();
            bool needLogin = true;
            string tokkenDB = "";
            string querykh = @" SELECT *  FROM tbRegister"; // Sử dụng ? thay cho @mst trong OleDb

            tbreTbRegister = ExecuteQuery(querykh, new OleDbParameter("?", ""));
            string gettimeTokken = tbreTbRegister.AsEnumerable().FirstOrDefault()["TimeTokken"].ToString();
            if (!string.IsNullOrEmpty(gettimeTokken))
            {
                var timpsan = DateTime.Now - DateTime.Parse(gettimeTokken);
                if (timpsan.TotalMinutes <= 30)
                {
                    needLogin = false;
                    this.tokken = tbreTbRegister.AsEnumerable().FirstOrDefault().Field<string>("tokken");
                }
            }

            if (needLogin == true)
            {
                using (var client = new HttpClient())
                {
                    try
                    {

                        string rs = "";
                        string url = "https://hoadondientu.gdt.gov.vn:30000/captcha";
                        try
                        {
                            rs = Task.Run(async () => await GDTClient.GetJsonAsync(url)).Result;
                        }
                        catch (Exception ex)
                        {
                            XtraMessageBox.Show(ex.Message);
                            return;
                        }

                        //Đọc nội dung phản hồi
                        MyJson myJson = JsonConvert.DeserializeObject<MyJson>(rs);
                        //string filePath = "output.svg";
                        string filePath = AppDomain.CurrentDomain.BaseDirectory + "output.svg"; // Đảm bảo tệp ở cùng thư mục với chương trình
                                                                                                //Lưu chuỗi SVG vào tệp
                        File.WriteAllText(filePath, myJson.Content);
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
                        var response = client.PostAsync(url, content).Result;
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {

                            XtraMessageBox.Show("Có lỗi đăng nhập hệ thống vui lòng thử lại");
                            btnTaiHdCQT.PerformClick();
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
                        string responseBody = response.Content.ReadAsStringAsync().Result;
                        var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);
                        this.tokken = tokenResponse.token;
                        string query = @"UPDATE tbRegister SET TimeTokken=? ";

                        var parameters = new OleDbParameter[]
                 {
                                   new OleDbParameter("?", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))

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
            else
            {
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
            }
        }
        private void XulytaiExcel()
        {
            if (chkDauvao.Checked)
            {
                progressPanel1.Caption = "Đang tải HDDienTuDaCapMa.xlsx ";
                Application.DoEvents();
                Xulyexelvao(tokken, 1);
                progressPanel1.Caption = "Đang tải HDDienTuKhongMa.xlsx ";
                Application.DoEvents();
                Xulyexelvao(tokken, 2);
                progressPanel1.Caption = "Đang tải HDDienTuMayTinhTien.xlsx ";
                Application.DoEvents();
                Xulyexelvao(tokken, 3);
                progressPanel1.Caption = "Đang đọc dữ liệu excel";
                Application.DoEvents();

                //Đọc excel và tải hoá đơn về
                //DocfileExcelVao();
            }
        }
        public void Xulyexelvao(string token, int _type)
        {
            GDTClient.UpdateToken(token);
            // Tối ưu: Tính toán datetime một lần
            DateTime dtFrom = new DateTime(dtTungay.DateTime.Year, dtTungay.DateTime.Month, 1);
            DateTime dtTo = dtFrom.AddMonths(1).AddDays(-1);

            // Tối ưu: Format string một lần
            string formattedDate1 = dtFrom.ToString("dd/MM/yyyyTHH:mm:ss");
            string formattedDate2 = dtTo.ToString("dd/MM/yyyyTHH:mm:ss");

            // Tối ưu: Dùng switch case thay vì nhiều if
            string url, filename;
            switch (_type)
            {
                case 1:
                    url = $@"https://hoadondientu.gdt.gov.vn:30000/query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==5%20%20%20%20&type=purchase";
                    filename = $"{mstcongty}_HDDienTuDaCapMa.xlsx";
                    break;
                case 2:
                    url = $@"https://hoadondientu.gdt.gov.vn:30000/query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==6%20%20%20%20&type=purchase";
                    filename = $"{mstcongty}_HDDienTuKhongMa.xlsx";
                    break;
                case 3:
                    url = $@"https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==8%20%20%20%20&type=purchase";
                    filename = $"{mstcongty}_HDDienTuMayTinhTien.xlsx";
                    break;
                default:
                    return;
            }

            string directoryPath = Path.Combine(savedPath, "HDVao", dtTungay.DateTime.Month.ToString());
            string filePath = Path.Combine(directoryPath, filename);

            // Tối ưu: Đảm bảo thư mục tồn tại trước
            Directory.CreateDirectory(directoryPath);

            // Xóa file cũ nếu tồn tại
            if (File.Exists(filePath))
            {
                DateTime lastWriteTime = File.GetLastWriteTime(filePath);
                TimeSpan timeDifference = DateTime.Now - lastWriteTime;

                if (timeDifference.TotalMinutes > 30)
                {
                    File.Delete(filePath);
                    Console.WriteLine($"Đã xóa file: {filePath}");
                }
                else
                {
                    Console.WriteLine($"File chưa đủ 30 phút để xóa. Thời gian còn lại: {30 - timeDifference.TotalMinutes:F1} phút");
                    return;
                }
            }

            try
            { 
                progressPanel1.Caption = $"Đang tải {filePath} ";
                GDTClient.DownloadFile_Sync(url, filePath, token);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");
            }
        }
        //Tải dữ liệu cơ quan thuế
        private async void btnTaiHdCQT_Click(object sender, EventArgs e)
        {
            GetTokken();
            XulytaiExcel();
        }
        #endregion

        private void btnChonthang_Click(object sender, EventArgs e)
        {

        }
    }
}