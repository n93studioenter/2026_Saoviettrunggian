using ClosedXML.Excel;
using DevExpress.XtraEditors;
using DevExpress.XtraWaitForm;
using Newtonsoft.Json;
using SaovietTax.Database;
using SaovietTax.DTO;
using Serilog.Parsing;
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
using static SaovietTax.frmAutoTai;
using static SaovietTax.frmMain;

namespace SaovietTax
{
    public partial class frmAutotaiNew : DevExpress.XtraEditors.XtraForm
    {
        public frmAutotaiNew()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Minimized;
            InitializeNotifyIcon();
        }
        private void InitializeNotifyIcon()
        {
            // Tạo NotifyIcon
            notifyIcon = new NotifyIcon();

            // Set icon (bạn cần có file .ico hoặc dùng icon mặc định)
            notifyIcon.Icon = SystemIcons.Application; // Hoặc load từ file: new Icon("app.ico")

            // Set tooltip khi hover
            string appPaths = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPaths);

            // Xóa phần \bin\Debug để lấy đường dẫn gốc
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
            string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");
            string pathThumuc = Path.Combine(rootDirectory);
            string dbPath = "";
            //MessageBox.Show(pathThumuc);
            try
            {
                string content = File.ReadAllText(filePaths);
                string dbName = Path.GetFileNameWithoutExtension(content);
                notifyIcon.Text = dbName;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi đọc file: " + ex.Message);
            }

            string appPath = Application.StartupPath;

            // Đi lên 2 cấp để đến thư mục Resources trong source
            // Ví dụ: bin\Debug\ → ..\..\Resources\favicon.ico
            string iconPath = Path.Combine(appPath, @"..\..\Resources\favicon.ico");
            // Kiểm tra file tồn tại
            if (File.Exists(iconPath))
            {
                notifyIcon.Icon = new Icon(iconPath);
            }
            else
            {
                notifyIcon.Icon = SystemIcons.Application;
            }
            // Tạo menu chuột phải
            contextMenu = new ContextMenuStrip();

            // Thêm các menu item
            contextMenu.Items.Add("Hiện ứng dụng", null, ShowApp_Click);
            contextMenu.Items.Add("-"); // Separator
            contextMenu.Items.Add("Thoát", null, ExitApp_Click);

            // Gán menu cho NotifyIcon
            notifyIcon.ContextMenuStrip = contextMenu;

            // Xử lý sự kiện click đúp chuột vào icon
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            // Hiển thị NotifyIcon
            notifyIcon.Visible = true;

            // Khi form load, ẩn form (nếu cần) 
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            // Double click vào icon → hiện form
            ShowApp();
        }

        private void ShowApp_Click(object sender, EventArgs e)
        {
            ShowApp();
        }

        private void ShowApp()
        {
            // Hiện form
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true; // Hiện lại trên taskbar
            this.BringToFront();

            // Focus vào form
            this.Activate();
        }

        private void ExitApp_Click(object sender, EventArgs e)
        {
            // Thoát ứng dụng
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
               // this.ShowInTaskbar = false;
            }
        }
        #region Properties
        string password, connectionString;
        DataTable tbimport;
        DataTable tbchungtu;
        List<KhachHang> lstKhachhangs = new List<KhachHang>();
        public DataTable tbKhachhang = new DataTable();
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;
        private BindingList<FileImport> lstImportVao = new BindingList<FileImport>();
        private BindingList<FileImport> lstImportRa = new BindingList<FileImport>();
        private string MSTCongTY = "";
        private string CCCD = "";
        private Dictionary<string, TbImportDetail> cacheMatHangTrongHoaDon;
        public List<DTO.VatTu> lstvt = new List<DTO.VatTu>();
        private Dictionary<string, string> _lookupByTenChinh;
        private Dictionary<string, VatTuInfo> _lookupByTenChinhs;
        public Dictionary<string, string> _lookupByTenPhu;
        public DataTable existingTbChungtu;
        DataTable existingTbHeThongTK;
        public DataTable existingTbHoadon;
        DataTable tbNhapkhonguyenlieu;
        DataTable tbTonkho;
        DataTable tbNhapkhotp;
        DataTable tbRegister;
        DataTable tbLicense;
        DataTable tbDinhDanhtaikhoan = new DataTable();
        int trylogin = 0;
        string tokken = "";
        string mstcongty = "";
        string savedPath = "";
        int soluottai = 0;
        int thoigiantai = 0;
        static int totalInvoices = 0;
        static int currentProgress = 0;
        public HashSet<(string Mst, string SoHD, string KyHieu, DateTime NLap, int Type)> lookupHoaDonCT { get; }
 = new HashSet<(string Mst, string SoHD, string KyHieu, DateTime NLap, int Type)>();
        private HashSet<(string MST, string SHDon, DateTime NLap, int Type)> lookupTbImport
           = new HashSet<(string MST, string SHDon, DateTime NLap, int Type)>();
        private HashSet<(string MST, string SHDon, DateTime NLap, int Type)> lookupTbImportCQT
         = new HashSet<(string MST, string SHDon, DateTime NLap, int Type)>();
        private (string MST, string SHDon, DateTime NLap, int Types) NormalizeTbImportKey(
 string mst, string shDon, DateTime nLap, int Types)
        {
            return (
                (mst ?? "").Trim(),
                Helpers.RemoveLeadingZeros(shDon ?? "").Trim(),
                nLap.Date,
                Types
            );
        }
        #endregion

        #region Database
        public void getconnectstring()
        {
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);

            // Xóa phần \bin\Debug để lấy đường dẫn gốc
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
            string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");
            string pathThumuc = Path.Combine(rootDirectory);
            string dbPath = "";
            //MessageBox.Show(pathThumuc);
            try
            {
                string content = File.ReadAllText(filePaths);
                dbPath = content;
                string fullName = dbPath.Substring(dbPath.LastIndexOf('\\') + 1);
                // fullName = "Thanh Huong BD2026.mdb"

                this.Text = fullName.Substring(0, fullName.LastIndexOf('.')).Trim();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi đọc file: " + ex.Message);
            }

            // Đọc toàn bộ nội dung tệp
            string password = "1@35^7*9)1";
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";
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
        #endregion


        #region init
        private void LoadHoadonCT()
        {
            string query = @"
        SELECT 
            hd.SoHD,
            hd.KyHieu,
            hd.NgayPH,
            hd.MaKhachHang,
            ct.NgayCT,
            ct.MaLoai
        FROM 
            Hoadon hd
        INNER JOIN 
            Chungtu ct ON hd.MaSo = ct.MaSo
        WHERE 
            hd.KyHieu <> '...'";

            var data = ExecuteQuery(query);

            // 🔥 lookup KHÁCH HÀNG (O(1))
            var khachHangMstLookup = lstKhachhangs.ToDictionary(
                k => k.MaSo,
                k => (k.MST ?? "").Trim()
            );

            lookupHoaDonCT.Clear();

            foreach (DataRow item in data.Rows)
            {
                string soHD = Helpers.RemoveLeadingZeros(
                    item["SoHD"]?.ToString() ?? ""
                ).Trim();
                string KyHieu = item["KyHieu"]?.ToString() ?? "";
                DateTime ngayPH = ((DateTime)item["NgayCT"]).Date;

                int maKhachHang = (int)item["MaKhachHang"];

                string mst = khachHangMstLookup.TryGetValue(maKhachHang, out var v)
                    ? v
                    : "";
                int Maloai = int.Parse(item["MaLoai"]?.ToString());
                if (Maloai == 8)
                {
                    Maloai = 2;
                }
                else
                {
                    Maloai = 1;
                }
                soHD = soHD.Replace(".", "").Trim();
                soHD = RemoveLeadingZeros(soHD);
                lookupHoaDonCT.Add((mst, soHD, KyHieu, ngayPH, Maloai));
            }
        }
        private void BuildLookupTbImport()
        {
            string query = "SELECT * FROM tbimport";
            DataTable tbimports = ExecuteQuery(query);
            lookupTbImport = new HashSet<(string MST, string SHDon, DateTime NLap, int Type)>();
            lookupTbImportCQT = new HashSet<(string MST, string SHDon, DateTime NLap, int Type)>();
            foreach (DataRow row in tbimports.Rows)
            {
                var key = NormalizeTbImportKey(
                    row["Mst"] == DBNull.Value ? "" : row["Mst"].ToString(),
                    row["SHDon"] == DBNull.Value ? "" : row["SHDon"].ToString(),
                    row["NLap"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["NLap"]),
                    row["Type"] == DBNull.Value ? 0 : int.Parse(row["Type"].ToString())
                    );

                lookupTbImport.Add(key);
                var keys = NormalizeTbImportKey(
                     row["Mst"] == DBNull.Value ? "" : row["Mst"].ToString(),
                     row["SHDon"] == DBNull.Value ? "" : row["SHDon"].ToString(),
                     row["NLap"] == DBNull.Value ? DateTime.MinValue
                                                : Convert.ToDateTime(row["NLap"]),
                      row["Type"] == DBNull.Value ? 0 : int.Parse(row["Type"].ToString())
                 );
                lookupTbImportCQT.Add(keys);
            }
        }
        private void LoadDatatable()
        {
            var query = "SELECT * FROM KhachHang"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHang
            tbKhachhang = ExecuteQuery(query);
            string querydd = @" SELECT *  FROM tbDinhdanhtaikhoan"; // Sử dụng ? thay cho @mst trong OleDb
            tbDinhDanhtaikhoan = ExecuteQuery(querydd, new OleDbParameter("?", ""));
            tbLicense = ExecuteQuery("SELECT * FROM License", null);
            LoadHoadonCT();
            BuildLookupTbImport();
        }
        public void Gettokken()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = new HttpResponseMessage();
                    string url = "https://hoadondientu.gdt.gov.vn/api/captcha";
                    int retry = 0;
                    int maxRetry = 10; // thử tối đa 10 lần

                    while (retry < maxRetry)
                    {
                        try
                        {
                            progressPanel1.Caption = $"Đăng nhập lần thứ {retry + 1}";
                            response = client.GetAsync(url).Result;

                            if (response.IsSuccessStatusCode)
                            {
                                byte[] captchaBytes = response.Content.ReadAsByteArrayAsync().Result;

                                if (captchaBytes.Length > 0)
                                {
                                    // ✅ ĐÃ CÓ CAPTCHA → THOÁT
                                    break;
                                }
                            }
                        }
                        catch
                        {
                            // bỏ qua, thử lại
                        }

                        retry++;
                        Thread.Sleep(1000); // ⏳ chờ 1 giây rồi thử lại
                    }

                    if (response == null || !response.IsSuccessStatusCode)
                    {
                        progressPanel1.Caption = $"Đăng nhập thất bại";
                        return;
                    }
                    //Đọc nội dung phản hồi
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    MyJson myJson = JsonConvert.DeserializeObject<MyJson>(responseBody);
                    //string filePath = "output.svg";
                    string filePath = AppDomain.CurrentDomain.BaseDirectory + "output.svg"; // Đảm bảo tệp ở cùng thư mục với chương trình
                                                                                            //Lưu chuỗi SVG vào tệp
                    File.WriteAllText(filePath, myJson.Content);
                    Thread.Sleep(50);

                    SvgCaptchaSolver solver = new SvgCaptchaSolver();
                    string result = solver.SolveCaptcha(filePath);

                    url = "https://hoadondientu.gdt.gov.vn/api/security-taxpayer/authenticate";
                    string querykh = @" SELECT *  FROM tbRegister"; // Sử dụng ? thay cho @mst trong OleDb
                    var tbRegister = ExecuteQuery(querykh, new OleDbParameter("?", ""));
                    soluottai = tbRegister.Rows[0]["Soluottai"] == DBNull.Value
     ? 0
     : Convert.ToInt32(tbRegister.Rows[0]["Soluottai"]);

                    thoigiantai = tbRegister.Rows[0]["Thoigiantai"] == DBNull.Value
                       ? 0
                       : Convert.ToInt32(tbRegister.Rows[0]["Thoigiantai"]);
                    savedPath = tbRegister.Rows[0]["Hoadonpath"].ToString();
                    mstcongty = tbRegister.Rows[0]["Username"].ToString();
                    var payload = new
                    {
                        username = tbRegister.Rows[0].Field<string>("Username"),
                        password = tbRegister.Rows[0].Field<string>("Password"),
                        cvalue = result,
                        ckey = myJson.Key
                    };
                    string json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = client.PostAsync(url, content).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Thread.Sleep(1000);
                        trylogin += 1;
                        if (trylogin > 3)
                        {
                            // XtraMessageBox.Show("Không thể đăng nhập vui lòng thử lại");
                            lblCurrent.Text = "Không thể đăng nhập vui lòng thử lại";
                            return;
                        }
                        Thread.Sleep(1000);
                        Gettokken();
                    }

                    response.EnsureSuccessStatusCode();
                    Thread.Sleep(50);
                    responseBody = response.Content.ReadAsStringAsync().Result;
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);
                    this.tokken = tokenResponse.token;
                    lblCurrent.Text = "Đăng nhập thành công...";
                    progressPanel1.Visible = false;
                    var query = @"UPDATE tbRegister SET TimeTokken=? ";

                    var parameters = new OleDbParameter[]
             {
                                   new OleDbParameter("?", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))

             };
                    int rowsAffected = ExecuteQueryResult(query, parameters);

                }
                catch (Exception ex)
                {
                    // XtraMessageBox.Show(ex.Message); 
                    Application.DoEvents();
                }

            }
        }
        #endregion


        #region Xử lý excel

        public async Task<bool> XulyexelvaoAsync(string token, int _type)
        {
            DateTime dtFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime dtTo = DateTime.Now;

            string formattedDate1 = dtFrom.ToString("dd/MM/yyyyTHH:mm:ss");
            string formattedDate2 = dtTo.ToString("dd/MM/yyyyTHH:mm:ss");

            string url, filename;

            switch (_type)
            {
                case 1:
                    url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==5%20%20%20%20&type=purchase";
                    filename = $"{mstcongty}_HDDienTuDaCapMa.xlsx";
                    break;

                case 2:
                    url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==6%20%20%20%20&type=purchase";
                    filename = $"{mstcongty}_HDDienTuKhongMa.xlsx";
                    break;

                case 3:
                    url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==8%20%20%20%20&type=purchase";
                    filename = $"{mstcongty}_HDDienTuMayTinhTien.xlsx";
                    break;

                default:
                    return false;
            }

            string currentYear = $"HD{DateTime.Now.Year}";
            string directoryPath = Path.Combine(savedPath, currentYear, "HDVao", DateTime.Now.Month.ToString());
            Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, filename);

            if (File.Exists(filePath))
            {
                TimeSpan ts = DateTime.Now - File.GetLastWriteTime(filePath);

                if (ts.TotalMinutes < 30)
                {
                    if (_type == 1)
                        lblExcelV1.Text = "Đã tải xong";

                    if (_type == 2)
                        lblExcelV2.Text = "Đã tải xong";

                    if (_type == 3)
                        lblExcelV3.Text = "Đã tải xong";
                    return true;
                }

                File.Delete(filePath);
            }

            const int maxRetry = 3;

            for (int retry = 1; retry <= maxRetry; retry++)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(15);

                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);

                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                        //richTextBox1.Text = $"Đang tải ({retry}/{maxRetry})...";
                        Application.DoEvents();

                        HttpResponseMessage response = await client.GetAsync(
                            url,
                            HttpCompletionOption.ResponseHeadersRead);

                        response.EnsureSuccessStatusCode();

                        using (var fs = new FileStream(filePath,
                            FileMode.Create,
                            FileAccess.Write,
                            FileShare.None))
                        {
                            await response.Content.CopyToAsync(fs);
                        }

                        if (_type == 1)
                            lblExcelV1.Text = "Đã tải xong";

                        if (_type == 2)
                            lblExcelV2.Text = "Đã tải xong";

                        if (_type == 3)
                            lblExcelV3.Text = "Đã tải xong";

                        return true;
                    }
                }
                catch (TaskCanceledException)
                {
                    // richTextBox1.Text = $"Timeout ({retry}/{maxRetry})";
                }
                catch (Exception ex)
                {
                    //richTextBox1.Text = ex.Message;
                }

                if (retry < maxRetry)
                    await Task.Delay(1000);
            }

            return false;
        }
        #endregion
        public class HoadonTai
        {
            public int Stt { get; set; }
            public string SoHd { get; set; }
            public string Khhd { get; set; }
            public DateTime NLap { get; set; }
            public string Status { get; set; } // "Đang tải", "Success", "Fail"
            public string Mst { get; set; }    // Thêm Mst để xác định
            public string DirectoryPath { get; set; }
            public int RetryCount { get; set; } // Số lần thử
        }
        private BindingList<HoadonTai> bindingListHoaDon = new BindingList<HoadonTai>();
        private async void frmAutotaiNew_Load(object sender, EventArgs e)
        {
            lblCurrent.Text = "Kết nối cơ sở dữ liệu...";
            getconnectstring();
            LoadDatatable();
            progressPanel1.Caption = "Đang đăng nhập hệ thống thuế";
            lblCurrent.Text = "Đang lấy token...";
            Gettokken();
            //Tải 3 file excel hoá đơn đầu vào
            Task<bool> t1 = XulyexelvaoAsync(tokken, 1);
            Task<bool> t2 = XulyexelvaoAsync(tokken, 2);
            Task<bool> t3 = XulyexelvaoAsync(tokken, 3);

            bool[] result = await Task.WhenAll(t1, t2, t3);
            lblCurrent.Text = "Tải excel đầu vào thành công, bắt đầu tiến hành tải hoá đơn...";

            //Đếm số lượng hoá đơn 
            string pathYear = $"HD{DateTime.Now.Year}";
            string directoryPath = Path.Combine(savedPath, pathYear, "HDVao", DateTime.Now.Month.ToString());

            var excelFiles = Directory.EnumerateFiles(directoryPath, "*.xlsx", SearchOption.AllDirectories).Where(m => m.Contains(mstcongty)).ToList();
            int Stt = 1;
            foreach (var excelFile in excelFiles)
            {
                using (var workbook = new XLWorkbook(excelFile))
                {
                    var worksheet = workbook.Worksheet(1);
                   
                    foreach (var row in worksheet.RowsUsed().Skip(3))
                    {
                        string GetNLap = row.Cell("E").Value.ToString();
                        string getkhhd = row.Cell("C").Value.ToString();
                        string getSohd = Helpers.RemoveLeadingZeros(row.Cell("D").Value.ToString()); // Lấy giá trị của cột C trong hàng hiện tại 
                        if (getSohd == "905")
                        {
                            int findhd = 10;
                        }
                        string mstnb = row.Cell("F").Value.ToString();
                        if (DateTime.TryParse(GetNLap, out DateTime getdate))
                        {
                            DateTime gd = DateTime.Parse(GetNLap);
                            bool daTonTai = lookupHoaDonCT.Contains((mstnb, getSohd, getkhhd, gd.Date, 1));
                            bool daTonTaiimport = lookupTbImportCQT.Contains((mstnb, getSohd, gd.Date, 1));
                            if (daTonTai || daTonTaiimport)
                            {
                                continue;
                            }
                            HoadonTai hoadonTai = new HoadonTai();
                            hoadonTai.Stt = Stt;
                            hoadonTai.SoHd = getSohd;
                            hoadonTai.Khhd = getkhhd;
                            hoadonTai.NLap = gd;
                            bindingListHoaDon.Add(hoadonTai);
                            totalInvoices++;
                            Stt++;
                        }
                    }
                }
            }

            int getSoluongfile = totalInvoices;
            simpleButton1.Text = $"Tổng số hoá đơn đầu vào cần tải là  {getSoluongfile}";

            gridControl1.DataSource = bindingListHoaDon;
            //
            string querykh = @" SELECT *  FROM tbRegister"; // Sử dụng ? thay cho @mst trong OleDb

            var tbRegister = ExecuteQuery(querykh, new OleDbParameter("?", ""));
            string originpath = tbRegister.Rows[0]["Hoadonpath"].ToString();
            string username = tbRegister.Rows[0]["Username"].ToString();
            //var invoicesVao = await GetListHoaDonCanTai(username, originpath, 1);
            // await TaiHangLoatHoaDon(invoicesVao, "đầu vào");

            Task<bool> t1r = XulyexelraAsync(tokken, 1);
            Task<bool> t2r = XulyexelraAsync(tokken, 2);

           result = await Task.WhenAll(t1r, t2r);
            lblCurrent.Text = "Tải excel đầu ra thành công, bắt đầu tiến hành tải hoá đơn...";
            var invoicesRa = await GetListHoaDonCanTai(username, originpath, 2);
            await TaiHangLoatHoaDon(invoicesRa, "đầu ra");
        }
        public async Task<bool> XulyexelraAsync(string token, int _type)
        {
            DateTime dtFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime dtTo = dtFrom.AddMonths(1).AddDays(-1);

            string formattedDate1 = dtFrom.ToString("dd/MM/yyyyTHH:mm:ss");
            string formattedDate2 = dtTo.ToString("dd/MM/yyyyTHH:mm:ss");

            string url, filename;

            switch (_type)
            {
                case 1:
                    url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2}";
                    filename = $"{mstcongty}_Hoadondientu.xlsx";
                    break;

                case 2:
                    url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-excel?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2}";
                    filename = $"{mstcongty}_HDDienTuMayTinhTien.xlsx";
                    break;

                default:
                    return false;
            }

            string currentYear = $"HD{DateTime.Now.Year}";
            string directoryPath = Path.Combine(savedPath, currentYear, "HDRa", DateTime.Now.Month.ToString());
            Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, filename);

            if (File.Exists(filePath))
            {
                TimeSpan ts = DateTime.Now - File.GetLastWriteTime(filePath);

                if (ts.TotalMinutes < 30)
                {
                    return true;
                }

                File.Delete(filePath);
            }

            const int maxRetry = 3;

            for (int retry = 1; retry <= maxRetry; retry++)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(10);

                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);

                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/octet-stream"));


                        HttpResponseMessage response = await client.GetAsync(
                            url,
                            HttpCompletionOption.ResponseHeadersRead);

                        response.EnsureSuccessStatusCode();

                        using (var fs = new FileStream(
                            filePath,
                            FileMode.Create,
                            FileAccess.Write,
                            FileShare.None))
                        {
                            await response.Content.CopyToAsync(fs);
                        }


                        return true;
                    }
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception ex)
                {
                }

                if (retry < maxRetry)
                {
                    await Task.Delay(3000);
                }
            }

            return false;
        }
        #region Xulyhoadon
        private async Task<List<InvoiceInfo>> GetListHoaDonCanTai(string mstcongty, string savedPath, int type)
        {
            return await Task.Run(() =>
            {
                var result = new List<InvoiceInfo>();

                try
                {
                    // Đường dẫn: HDVao hoặc HDRa
                    string folderName = type == 1 ? "HDVao" : "HDRa";
                    string currentYear = $"HD{DateTime.Now.Year}";
                    string directoryPath = Path.Combine(savedPath, currentYear, folderName, DateTime.Now.Month.ToString());

                    if (!Directory.Exists(directoryPath))
                    {
                       // UpdateStatus($"❌ Thư mục không tồn tại: {directoryPath}");
                        return result;
                    }

                    // Lấy tất cả file Excel
                    var excelFiles = Directory.EnumerateFiles(directoryPath, "*.xlsx", SearchOption.AllDirectories)
                                              .Where(m => m.Contains(mstcongty)).ToList();

                    if (excelFiles.Count == 0)
                    {
                        //UpdateStatus($"📭 Không tìm thấy file Excel {folderName}");
                        return result;
                    }

                    foreach (var excelFile in excelFiles)
                    {
                        using (var workbook = new XLWorkbook(excelFile))
                        {
                            var worksheet = workbook.Worksheet(1);
                            int rowIndex = 0;

                            foreach (var row in worksheet.RowsUsed().Skip(3))
                            {
                                rowIndex++;
                                try
                                {
                                    string khhd = row.Cell("B").Value.ToString();
                                    string getSHHD = row.Cell("C").Value.ToString();
                                    string getSohd = RemoveLeadingZeros(row.Cell("D").Value.ToString());
                                    string GetNLap = row.Cell("E").Value.ToString();
                                    string mstnb = row.Cell("F").Value.ToString();

                                    if (!DateTime.TryParse(GetNLap, out DateTime getdate))
                                        continue;

                                    // Kiểm tra trùng
                                    bool daTonTai = lookupHoaDonCT.Contains((mstnb, getSohd, getSHHD, getdate.Date, type));
                                    bool daTonTaiImport = lookupTbImport.Contains((mstnb, getSohd, getdate.Date, type));

                                    if (daTonTai || daTonTaiImport)
                                        continue;

                                    // Kiểm tra file đã tồn tại
                                    string filename = $"{getdate:yyyyMMdd}_{mstnb}_{getSohd}_{getSHHD}.xml";
                                    if (File.Exists(Path.Combine(directoryPath, filename)))
                                        continue;

                                    // Thêm vào danh sách
                                    result.Add(new InvoiceInfo
                                    {
                                        Mst = mstnb,
                                        SHHD = getSHHD,
                                        Sohd = getSohd,
                                        NLap = getdate,
                                        Khhd = khhd,
                                        DirectoryPath = directoryPath
                                    });

                                    // Log mỗi 100 hóa đơn
                                    if (result.Count % 100 == 0)
                                    {
                                       // UpdateStatus($"📋 Đã đọc {result.Count} hóa đơn cần tải...");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Lỗi dòng {rowIndex}: {ex.Message}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                   // UpdateStatus($"❌ Lỗi GetListHoaDonCanTai: {ex.Message}");
                }

                return result;
            });
        }
        private async Task TaiHangLoatHoaDon(List<InvoiceInfo> invoices, string typeName)
        {
            int type = 0;
            if(typeName== "đầu ra")
            {
                type = 2;
            }
            else
            {
                type = 1;
            }
                try
                {
                    if (invoices == null || invoices.Count == 0)
                    {
                        UpdateStatus($"📭 Không có hóa đơn {typeName} để tải");
                        return;
                    }

                    UpdateStatus($"📥 Bắt đầu tải {invoices.Count} hóa đơn {typeName} (song song 5 luồng, mỗi HĐ thử tối đa 3 lần)...");

                    int total = invoices.Count;
                    int downloaded = 0;
                    int failed = 0;
                    object lockObj = new object();

                    var stopwatch = Stopwatch.StartNew();

                    await Task.Run(() =>
                    {
                        Parallel.ForEach(invoices, new ParallelOptions { MaxDegreeOfParallelism = 5 }, invoice =>
                        {
                            try
                            {
                                // Gọi hàm tải 1 hóa đơn (có retry bên trong)
                                bool success = DownloadSingleInvoiceSync(invoice, type);

                                lock (lockObj)
                                {
                                    if (success)
                                    {
                                        downloaded++;
                                    }
                                    else
                                    {
                                        failed++;
                                    }

                                    if ((downloaded + failed) % 10 == 0 || (downloaded + failed) == total)
                                    {
                                        UpdateStatus($"⏳ Đã xử lý {downloaded + failed}/{total} hóa đơn {typeName} (✅ {downloaded} thành công, ❌ {failed} thất bại)");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                lock (lockObj)
                                {
                                    failed++;
                                    UpdateStatus($"❌ Lỗi HĐ {invoice.Sohd}: {ex.Message}");
                                }
                            }
                        });
                    });

                    stopwatch.Stop();

                    UpdateStatus($"✅ Hoàn thành tải {typeName}! Đã tải: {downloaded}/{total} hóa đơn, thất bại: {failed}, thời gian: {stopwatch.Elapsed.TotalSeconds:F1}s");

                    if (downloaded > 0)
                    {
                        UpdateStatus($"📊 Bắt đầu xử lý {downloaded} hóa đơn {typeName}...");
                        // await XuLyHoaDonDaTai(invoices, typeName);
                    }

                    if (failed > 0)
                    {
                        UpdateStatus($"⚠️ Có {failed} hóa đơn {typeName} thất bại sau 3 lần thử!");
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"❌ Lỗi TaiHangLoatHoaDon {typeName}: {ex.Message}");
                }
        }
        private bool DownloadSingleInvoiceSync(InvoiceInfo invoice,int type)
        {
            if (invoice == null) return false;

            // ✅ Cập nhật trạng thái "Đang tải"
            UpdateHoaDonStatus(invoice.Sohd, "Đang tải...", $"⏳ Đang tải HĐ {invoice.Sohd}...");

            int maxRetry = 3;
            int retryCount = 0;
            int fileIndex = 1; // Đánh dấu loại file Excel (1, 2, 3)
            while (retryCount < maxRetry)
            {
                retryCount++;

                try
                {
                    string url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-xml?nbmst={invoice.Mst}&khhdon={invoice.SHHD}&shdon={invoice.Sohd}&khmshdon={invoice.Khhd}";

                    if (type == 1) // Đầu vào (HDVao)
                    {
                        if (fileIndex == 1 || fileIndex == 2)
                        {
                            // Hóa đơn điện tử có mã và không mã
                            url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-xml?nbmst={invoice.Mst}&khhdon={invoice.SHHD}&shdon={invoice.Sohd}&khmshdon={invoice.Khhd}";
                        }
                        else // fileIndex == 3
                        {
                            // Hóa đơn máy tính tiền
                            url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-xml?nbmst={invoice.Mst}&khhdon={invoice.SHHD}&shdon={invoice.Sohd}&khmshdon={invoice.Khhd}";
                        }
                    }
                    else // Đầu ra (HDRa)
                    {
                        if (fileIndex == 1)
                        {
                            // Hóa đơn điện tử đầu ra
                            url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-xml?nbmst={invoice.Mst}&khhdon={invoice.SHHD}&shdon={invoice.Sohd}&khmshdon={invoice.Khhd}";
                        }
                        else // fileIndex == 2
                        {
                            // Hóa đơn máy tính tiền đầu ra
                            url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-xml?nbmst={invoice.Mst}&khhdon={invoice.SHHD}&shdon={invoice.Sohd}&khmshdon={invoice.Khhd}";
                        }
                    }

                    string filename = $"{invoice.NLap:yyyyMMdd}_{invoice.Mst}_{invoice.Sohd}_{invoice.SHHD}.zip";
                    string path = Path.Combine(invoice.DirectoryPath, filename);
                    string pathxml = Path.Combine(invoice.DirectoryPath, filename.Replace(".zip", ".xml"));

                    // Kiểm tra đã tồn tại
                    if (File.Exists(path) || File.Exists(pathxml))
                    {
                        // ✅ Cập nhật thành công
                        UpdateHoaDonStatus(invoice.Sohd, "Success", $"✅ HĐ {invoice.Sohd} đã tồn tại!");
                        return true;
                    }

                    bool isDownloaded = DownloadFileWithRetry(url, path, tokken, 2);

                    if (isDownloaded)
                    {
                        ExtractZipXML(path);
                        // ✅ Cập nhật thành công
                        UpdateHoaDonStatus(invoice.Sohd, "Success", $"✅ Tải thành công HĐ {invoice.Sohd}!");
                        return true;
                    }
                    else
                    {
                        if (retryCount < maxRetry)
                        {
                            UpdateHoaDonStatus(invoice.Sohd, $"Thử lại {retryCount + 1}/{maxRetry}", $"🔄 Thử lại HĐ {invoice.Sohd} lần {retryCount + 1}/{maxRetry}...");
                            Thread.Sleep(2000 * retryCount);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (retryCount < maxRetry)
                    {
                        UpdateHoaDonStatus(invoice.Sohd, $"Lỗi lần {retryCount}", $"⚠️ Lỗi HĐ {invoice.Sohd} lần {retryCount}: {ex.Message}");
                        Thread.Sleep(2000 * retryCount);
                    }
                    else
                    {
                        // ❌ Cập nhật thất bại
                        UpdateHoaDonStatus(invoice.Sohd, "Fail", $"❌ HĐ {invoice.Sohd} thất bại sau {maxRetry} lần: {ex.Message}");
                    }
                }
            }

            // ❌ Cập nhật thất bại
            UpdateHoaDonStatus(invoice.Sohd, "Fail", $"❌ HĐ {invoice.Sohd} thất bại sau {maxRetry} lần!");
            return false;
        }
        private static void ExtractZipXML(string path)
        {

            try
            {

                Application.DoEvents();
                string rootPath = Path.GetDirectoryName(path);
                string getnamefile = Path.GetFileNameWithoutExtension(path);
                string directoryPath = rootPath + @"\Giainen" + "_" + getnamefile;

                ZipFile.ExtractToDirectory(path, directoryPath);

                var files = Directory.GetFiles(directoryPath, "invoice.html", SearchOption.AllDirectories);
                string targetFilePath = Path.Combine(rootPath, getnamefile + ".html");
                File.Move(files.FirstOrDefault(), targetFilePath);

                //xml
                var filesxml = Directory.GetFiles(directoryPath, "invoice.xml", SearchOption.AllDirectories);
                string targetFilePathxml = Path.Combine(rootPath, getnamefile + ".xml");
                File.Move(filesxml.FirstOrDefault(), targetFilePathxml);

                File.Delete(path);
                Directory.Delete(directoryPath, true);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi giải nén hoặc xử lý file: {ex.Message}");
            }

        }
        private void UpdateStatus(string message)
        {
            //try
            //{
            //    if (this.InvokeRequired)
            //    {
            //        this.Invoke(new Action(() =>
            //        {
            //            lblCurrent.Text = message; 
            //            Application.DoEvents();
            //        }));
            //    }
            //    else
            //    {
            //        lblCurrent.Text = message; 
            //        Application.DoEvents();
            //    }
            //}
            //catch (ObjectDisposedException)
            //{
            //    // Form đã đóng, bỏ qua
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Lỗi UpdateStatus: {ex.Message}");
            //}
        }

        private void panelControl1_Paint(object sender, PaintEventArgs e)
        {

        }

        private bool DownloadFileWithRetry(string url, string filePath, string token, int maxRetry = 3)
        {
            int retryCount = 0;
            bool isDownloaded = false;

            // ✅ Kiểm tra thoigiantai hợp lệ
            int timeoutSeconds = (thoigiantai > 0) ? thoigiantai : 30; // Mặc định 30 giây nếu = 0

            while (retryCount < maxRetry && !isDownloaded)
            {
                retryCount++;
                Console.WriteLine($"Lần thử {retryCount}/{maxRetry} - Đang tải: {Path.GetFileName(filePath)}");

                try
                {
                    using (var client = new HttpClient())
                    {
                        // ✅ Set timeout hợp lệ
                        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                        try
                        {
                            // ✅ Dùng async/await thay vì task.Wait()
                            var response = client.GetAsync(url).GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {
                                var fileBytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();

                                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096))
                                {
                                    fileStream.Write(fileBytes, 0, fileBytes.Length);
                                }

                                isDownloaded = true;
                                Console.WriteLine($"✅ Tải thành công: {Path.GetFileName(filePath)}");
                                SafeUpdateStatus($"✅ Tải thành công: {Path.GetFileName(filePath)}");
                            }
                            else
                            {
                                Console.WriteLine($"❌ Lỗi HTTP: {response.StatusCode} - {response.ReasonPhrase}");
                                SafeUpdateStatus($"❌ Lỗi HTTP: {response.StatusCode} - {response.ReasonPhrase}");
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            Console.WriteLine($"⏰ Timeout! Lần thử {retryCount}/{maxRetry}");
                            SafeUpdateStatus($"⏰ Timeout! Lần thử {retryCount}/{maxRetry}");
                        }
                        catch (OperationCanceledException)
                        {
                            Console.WriteLine($"⏰ Request bị hủy! Lần thử {retryCount}/{maxRetry}");
                            SafeUpdateStatus($"⏰ Request bị hủy! Lần thử {retryCount}/{maxRetry}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Lỗi: {ex.Message}");
                            SafeUpdateStatus($"❌ Lỗi: {ex.Message}");
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    Console.WriteLine("❌ HttpClient đã bị dispose!");
                    break; // Thoát vòng lặp
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Lỗi: {ex.Message}");
                    SafeUpdateStatus($"❌ Lỗi: {ex.Message}");
                }

                // Nếu chưa tải thành công và còn lượt thử
                if (!isDownloaded && retryCount < maxRetry)
                {
                    int waitSeconds = retryCount * 2; // 2s, 4s, 6s
                    Console.WriteLine($"⏳ Chờ {waitSeconds} giây trước khi thử lại...");
                    SafeUpdateStatus($"⏳ Chờ {waitSeconds} giây trước khi thử lại...");

                    try
                    {
                        Thread.Sleep(waitSeconds * 1000);
                    }
                    catch (ThreadInterruptedException)
                    {
                        Console.WriteLine("⏹️ Đã dừng chờ!");
                        break;
                    }
                }
            }

            return isDownloaded;
        }
        private void SafeUpdateStatus(string message)
        {
            try
            {
                if (this == null || this.IsDisposed)
                    return;

                if (this.InvokeRequired && this.IsHandleCreated)
                {
                    this.Invoke(new Action(() =>
                    {
                        if (!this.IsDisposed && lblCurrent != null && lblCurrent.IsHandleCreated)
                        {
                            lblCurrent.Text = message;
                            Application.DoEvents();
                        }
                    }));
                }
                else
                {
                    if (!this.IsDisposed && lblCurrent != null && lblCurrent.IsHandleCreated)
                    {
                        lblCurrent.Text = message;
                        Application.DoEvents();
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Form đã đóng, bỏ qua
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi SafeUpdateStatus: {ex.Message}");
            }
        }
        /// <summary>
        /// Cập nhật trạng thái cho 1 hóa đơn trong GridView
        /// </summary>
        private void UpdateHoaDonStatus(string soHd, string status, string message = "")
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => UpdateHoaDonStatus(soHd, status, message)));
                    return;
                }

                // Tìm hóa đơn trong BindingList
                var item = bindingListHoaDon.FirstOrDefault(x => x.SoHd == soHd);
                if (item != null)
                {
                    item.Status = status;

                    // Cập nhật message nếu có
                    if (!string.IsNullOrEmpty(message))
                    {
                        lblCurrent.Text = message;
                    }

                    // Refresh GridView
                    gridView1.RefreshRow(bindingListHoaDon.IndexOf(item));
                    gridView1.RefreshData();

                    // Cập nhật số lượng
                    UpdateStatusCounts();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi UpdateHoaDonStatus: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật nhiều hóa đơn cùng lúc
        /// </summary>
        private void UpdateHoaDonStatusBatch(Dictionary<string, string> statusDict)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => UpdateHoaDonStatusBatch(statusDict)));
                    return;
                }

                foreach (var kvp in statusDict)
                {
                    var item = bindingListHoaDon.FirstOrDefault(x => x.SoHd == kvp.Key);
                    if (item != null)
                    {
                        item.Status = kvp.Value;
                    }
                }

                gridView1.RefreshData();
                UpdateStatusCounts();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi UpdateHoaDonStatusBatch: {ex.Message}");
            }
        }

        /// <summary>
        /// Đếm và hiển thị số lượng Success/Fail
        /// </summary>
        private void UpdateStatusCounts()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(UpdateStatusCounts));
                    return;
                }

                int total = bindingListHoaDon.Count;
                int successCount = bindingListHoaDon.Count(x => x.Status == "Success");
                int failCount = bindingListHoaDon.Count(x => x.Status == "Fail");
                int pendingCount = total - successCount - failCount;

                simpleButton1.Text = $"Tổng: {total} | ✅ Success: {successCount} | ❌ Fail: {failCount} | ⏳ Pending: {pendingCount}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi UpdateStatusCounts: {ex.Message}");
            }
        }
        #endregion
    }
}