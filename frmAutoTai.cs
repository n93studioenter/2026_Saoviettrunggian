using ClosedXML.Excel;
using DevExpress.CodeParser;
using DevExpress.Utils.About;
using DevExpress.XtraEditors;
using DevExpress.XtraMap.Native;
using DevExpress.XtraWaitForm;
using FuzzySharp;
using Microsoft.Win32;
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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Windows.Media.Protection.PlayReady;
using static SaovietTax.frmKhachhang;
using static SaovietTax.frmMain;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using DateTime = System.DateTime;
using Process = System.Diagnostics.Process;
using TimeSpan = System.TimeSpan;
using XmlNode = System.Xml.XmlNode;

namespace SaovietTax
{
    public partial class frmAutoTai : DevExpress.XtraEditors.XtraForm
    {
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
        private Dictionary<string, (string TenVattu, string TenVattu2, string DonVi, double Dongia, double SoLuong)> vatTuLookup;

        // ==================== TOÀN CỤC (chỉ khai báo 1 lần) ====================
        private readonly Dictionary<string, (string SoHieu, double Percent)> _cacheToanCuc
            = new Dictionary<string, (string SoHieu, double Percent)>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _synonymDictionary;
        Regex regex = new Regex(@"(\d+(g|ml|L|kg)|x\d+|(\d+\s*cái))", RegexOptions.IgnoreCase);

        // Khởi tạo index 1 lần duy nhất khi load dữ liệu
        private Dictionary<string, HashSet<string>> _keywordIndex; // keyword -> list key
        private Dictionary<string, HashSet<string>> _quyCachIndex; // quyCach -> list key
        private bool _isIndexBuilt = false;
        public frmAutoTai()
        {
            InitializeComponent();
            // this.WindowState = FormWindowState.Minimized;
            // this.ShowInTaskbar = false;
            //this.ShowInTaskbar = false;

            //// 2. Thu nhỏ ngay
            //this.WindowState = FormWindowState.Minimized;

            //// 3. Chặn việc show form
            //this.Shown += (s, e) => this.Hide();

            // Tạo NotifyIcon
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

            // Khi form bị minimize, ẩn đi và chỉ hiển thị icon ở system tray
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.ShowInTaskbar = false;
            }
        }
        private bool _isExiting = false; // Thêm biến cờ

        // Dọn dẹp khi form đóng
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Dọn dẹp NotifyIcon
            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
                notifyIcon = null;
            }

            // Gọi base trước khi thoát
            base.OnFormClosing(e);

            // Thoát ứng dụng
            Application.Exit();
            // Environment.Exit(0); // KHÔNG nên dùng ở đây
        }
        //protected override void SetVisibleCore(bool value)
        //{
        //    base.SetVisibleCore(false); // ❗ chặn hiển thị
        //}
        string password, connectionString;
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
        int trylogin = 0;
        string tokken = "";
        string mstcongty = "";
        string savedPath = "";
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
                    txtUsername.Text = tbRegister.Rows[0].Field<string>("Username");
                    txtPassword.Text = tbRegister.Rows[0].Field<string>("Password");
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
                    txtToken.Text = tokenResponse.token;
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
                    richTextBox1.Text = ex.Message;
                    Application.DoEvents();
                }

            }
        }
        int type = 0;
        List<string> lstfile = new List<string>();
        public void MessagteToast(string msg)
        {
            currentState.Text = $"{msg}";
            Application.DoEvents();
        }
        // Tạo lịch chạy lúc 10h sáng
        public void ScheduleAt10AM()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            string cmd = $"schtasks /create /tn \"MyAppDaily\" /tr \"{exePath}\" /sc daily /st 10:33 /f";
            Process.Start("cmd", "/c " + cmd).WaitForExit();
        }

        // Xóa lịch
        public void RemoveSchedule()
        {
            string cmd = $"schtasks /delete /tn \"MyAppDaily\" /f";
            Process.Start("cmd", "/c " + cmd).WaitForExit();
        }
        public async Task DocfileExcelRaAsync(string mstcongty, string savedPath, string originpath)
        {
            LoadHoadonCT();
            Loadtbimport();

            string querykh = @" SELECT *  FROM tbimport";
            tbimport = ExecuteQuery(querykh, new OleDbParameter("?", ""));
            querykh = @" SELECT *  FROM Chungtu";
            tbchungtu = ExecuteQuery(querykh, new OleDbParameter("?", ""));

            string directoryPath = Path.Combine(savedPath, DateTime.Now.Month.ToString());

            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"❌ Thư mục không tồn tại: {directoryPath}");
                return;
            }

            var excelFiles = Directory.EnumerateFiles(directoryPath, "*.xlsx", SearchOption.AllDirectories)
                                      .Where(m => m.Contains(mstcongty)).ToList();

            int tongsohodadon = excelFiles.Count;
            int i = 1;

            foreach (var excelFile in excelFiles)
            {
                using (var workbook = new XLWorkbook(excelFile))
                {
                    var worksheet = workbook.Worksheet(1);

                    foreach (var row in worksheet.RowsUsed().Skip(3))
                    {
                        string khhd = row.Cell("B").Value.ToString();
                        string getSHHD = row.Cell("C").Value.ToString();
                        string getSohd = RemoveLeadingZeros(row.Cell("D").Value.ToString());
                        string GetNLap = row.Cell("E").Value.ToString();
                        string mstnb = row.Cell("F").Value.ToString();

                        stateDetail.Text = $"Đang tải hoá đơn {getSohd}";
                        Application.DoEvents();

                        DateTime getdate = DateTime.Parse(GetNLap);

                        string checkfile = Path.Combine(savedPath,
                            $"{getdate:yyyyMMdd}_{mstcongty}_{getSohd}_{getSHHD}.xml");

                        if (File.Exists(checkfile))
                        {
                            Console.WriteLine("File đã import");
                            continue;
                        }

                        var checkExist = tbimport.AsEnumerable()
                            .Where(m => m.Field<string>("SHDon") == getSohd &&
                                        m.Field<DateTime>("Nlap").Date == getdate.Date)
                            .FirstOrDefault();

                        if (checkExist != null)
                        {
                            Console.WriteLine("File đã import");
                            continue;
                        }

                        var checkExistct = tbchungtu.AsEnumerable()
                            .Where(m => m.Field<string>("SoHieu") == getSohd &&
                                        m.Field<DateTime>("NgayCT").Date == getdate.Date)
                            .FirstOrDefault();

                        if (checkExistct != null)
                        {
                            Console.WriteLine("File đã import");
                            continue;
                        }

                        string url = "";

                        if (i == 1)
                        {
                            url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-xml?nbmst={mstnb}&khhdon={getSHHD}&shdon={getSohd}&khmshdon={khhd}";
                        }

                        if (i == 2)
                        {
                            url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-xml?nbmst={mstnb}&khhdon={getSHHD}&shdon={getSohd}&khmshdon={khhd}";
                        }

                        lookupTbImport.Add((mstcongty, getSohd, getdate.Date, 1));

                        string filename = $"{getdate:yyyyMMdd}_{mstcongty}_{getSohd}_{getSHHD}.zip";
                        string path = Path.Combine(directoryPath, filename);

                        string filenamexml = $"{getdate:yyyyMMdd}_{mstcongty}_{getSohd}_{getSHHD}.xml";
                        string pathxml = Path.Combine(directoryPath, filenamexml);

                        if (!File.Exists(path) && !File.Exists(pathxml))
                        {
                            bool isDownloaded = await DownloadFileWithRetryAsync(
                                url,
                                path,
                                tokken,
                                soluottai,
                                thoigiantai);

                            if (isDownloaded)
                            {
                                Console.WriteLine($"✅ Tải file thành công: {filename}");
                                richTextBox1.Text = $"✅ Tải file thành công: {filename}";
                                Application.DoEvents();

                                ExtractZipXML(path);

                                Application.DoEvents();
                            }
                            else
                            {
                                Console.WriteLine($"❌ Tải file thất bại sau 3 lần thử: {filename}");
                                richTextBox1.Text = $"❌ Tải file thất bại sau 3 lần thử: {filename}";
                                Application.DoEvents();
                            }
                        }
                    }
                }

                i++;
            }
        }
        public void DocfileExcelRa(string mstcongty, string savedPath, string originpath)
        {
            LoadHoadonCT();
            Loadtbimport();

            string querykh = @" SELECT *  FROM tbimport";
            tbimport = ExecuteQuery(querykh, new OleDbParameter("?", ""));
            querykh = @" SELECT *  FROM Chungtu";
            tbchungtu = ExecuteQuery(querykh, new OleDbParameter("?", ""));

            string directoryPath = Path.Combine(savedPath, DateTime.Now.Month.ToString());

            // ========================================
            // KIỂM TRA THƯ MỤC TỒN TẠI
            // ========================================
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"❌ Thư mục không tồn tại: {directoryPath}");
                return;
            }

            var excelFiles = Directory.EnumerateFiles(directoryPath, "*.xlsx", SearchOption.AllDirectories)
                                      .Where(m => m.Contains(mstcongty)).ToList();

            int tongsohodadon = excelFiles.Count;
            int i = 1;

            foreach (var excelFile in excelFiles)
            {
                using (var workbook = new XLWorkbook(excelFile))
                {
                    var worksheet = workbook.Worksheet(1);
                    foreach (var row in worksheet.RowsUsed().Skip(3))
                    {
                        string khhd = row.Cell("B").Value.ToString();
                        string getSHHD = row.Cell("C").Value.ToString();
                        string getSohd = RemoveLeadingZeros(row.Cell("D").Value.ToString());
                        string GetNLap = row.Cell("E").Value.ToString();
                        string mstnb = row.Cell("F").Value.ToString();

                        stateDetail.Text = $"Đang tải hoá đơn {getSohd} ";
                        Application.DoEvents();

                        DateTime getdate = DateTime.Parse(GetNLap);

                        // Kiểm tra file đã tải
                        var checkfile = savedPath + "\\" + getdate.ToString("yyyyMMdd") + "_" + mstcongty + "_" + getSohd + "_" + getSHHD + ".xml";
                        if (File.Exists(checkfile))
                        {
                            Console.WriteLine("File đã import");
                            continue;
                        }

                        var checkExist = tbimport.AsEnumerable()
                            .Where(m => m.Field<string>("SHDon") == getSohd &&
                                        m.Field<DateTime>("Nlap").Date == getdate.Date)
                            .FirstOrDefault();
                        if (checkExist != null)
                        {
                            Console.WriteLine("File đã import");
                            continue;
                        }

                        var checkExistct = tbchungtu.AsEnumerable()
                            .Where(m => m.Field<string>("SoHieu") == getSohd &&
                                        m.Field<DateTime>("NgayCT").Date == getdate.Date)
                            .FirstOrDefault();
                        if (checkExistct != null)
                        {
                            Console.WriteLine("File đã import");
                            continue;
                        }

                        // Tạo URL
                        string url = "";
                        if (i == 1)
                        {
                            url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-xml?nbmst={mstnb}&khhdon={getSHHD}&shdon={getSohd}&khmshdon={khhd}";
                        }
                        if (i == 2)
                        {
                            url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-xml?nbmst={mstnb}&khhdon={getSHHD}&shdon={getSohd}&khmshdon={khhd}";
                        }

                        lookupTbImport.Add((mstcongty, getSohd, getdate.Date, 1));

                        string filename = $"{getdate.ToString("yyyyMMdd")}_{mstcongty}_{getSohd}_{getSHHD}.zip";
                        string path = Path.Combine(directoryPath, filename);
                        string filenamexml = $"{getdate.ToString("yyyyMMdd")}_{mstcongty}_{getSohd}_{getSHHD}.xml";
                        string pathxml = Path.Combine(directoryPath, filenamexml);

                        // Kiểm tra nếu hoá đơn chưa được tải
                        if (!File.Exists(path) && !File.Exists(pathxml))
                        {
                            // ========================================
                            // TẢI FILE VỚI RETRY 3 LẦN
                            // ========================================
                            bool isDownloaded = DownloadFileWithRetryRa(url, path, tokken, soluottai, thoigiantai);

                            if (isDownloaded)
                            {
                                Console.WriteLine($"✅ Tải file thành công: {filename}");
                                richTextBox1.Text = $"✅ Tải file thành công: {filename}";
                                Application.DoEvents();
                                ExtractZipXML(path); // Giải nén file ZIP
                                Application.DoEvents();
                            }
                            else
                            {
                                richTextBox1.Text = $"❌ Tải file thất bại sau 3 lần thử: {filename}";
                                Application.DoEvents();
                                Console.WriteLine($"❌ Tải file thất bại sau 3 lần thử: {filename}");
                            }
                        }
                    }
                }
                i++;
            }
        }

        // ========================================
        // HÀM TẢI FILE VỚI RETRY
        // ========================================
        private bool DownloadFileWithRetryRa(string url, string filePath, string token, int maxRetry = 3, int timeoutSeconds = 5)
        {
            int retryCount = 0;
            bool isDownloaded = false;

            while (retryCount < maxRetry && !isDownloaded)
            {
                retryCount++;
                Console.WriteLine($"Lần thử {retryCount}/{maxRetry} - Đang tải: {Path.GetFileName(filePath)}");
                richTextBox1.Text = $"Lần thử {retryCount}/{maxRetry} - Đang tải: {Path.GetFileName(filePath)}";
                Application.DoEvents();
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(thoigiantai);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                        // Sử dụng Task để kiểm soát timeout
                        var task = client.GetAsync(url);

                        if (task.Wait(TimeSpan.FromSeconds(thoigiantai)))
                        {
                            HttpResponseMessage response = task.Result;

                            if (response.IsSuccessStatusCode)
                            {
                                var fileBytes = response.Content.ReadAsByteArrayAsync().Result;

                                // Lưu file
                                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096))
                                {
                                    fileStream.Write(fileBytes, 0, fileBytes.Length);
                                }

                                isDownloaded = true;
                                Console.WriteLine($"✅ Tải thành công: {Path.GetFileName(filePath)}");
                                richTextBox1.Text = $"✅ Tải thành công: {Path.GetFileName(filePath)}";
                                Application.DoEvents();
                            }
                            else
                            {
                                Console.WriteLine($"❌ Lỗi HTTP: {response.StatusCode} - {response.ReasonPhrase}");
                                richTextBox1.Text = $"❌ Lỗi HTTP: {response.StatusCode} - {response.ReasonPhrase}";
                                Application.DoEvents();
                            }
                        }
                        else
                        {
                            // Timeout
                            Console.WriteLine($"⏰ Timeout! Lần thử {retryCount}/{maxRetry}");
                            richTextBox1.Text = $"⏰ Timeout! Lần thử {retryCount}/{maxRetry}";
                            Application.DoEvents();
                            client.CancelPendingRequests();
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    Console.WriteLine($"⏰ Request bị hủy do timeout: {ex.Message}");
                    richTextBox1.Text = $"⏰ Request bị hủy do timeout: {ex.Message}";
                    Application.DoEvents();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Lỗi: {ex.Message}");
                    richTextBox1.Text = $"❌ Lỗi: {ex.Message}";
                    Application.DoEvents();
                }

                // Nếu chưa tải thành công và còn lượt thử
                if (!isDownloaded && retryCount < maxRetry)
                {
                    // Tăng dần thời gian chờ: 5s, 10s, 15s
                    int waitSeconds = 1;
                    Console.WriteLine($"⏳ Chờ {waitSeconds} giây trước khi thử lại...");
                    richTextBox1.Text = $"⏳ Chờ {waitSeconds} giây trước khi thử lại...";
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(waitSeconds * 1000);
                }
            }

            return isDownloaded;
        }
        public void DocfileExcelRaOld(string mstcongty, string savedPath, string originpath)
        {
            LoadHoadonCT();
            Loadtbimport();

            string querykh = @" SELECT *  FROM tbimport";
            tbimport = ExecuteQuery(querykh, new OleDbParameter("?", ""));
            querykh = @" SELECT *  FROM Chungtu";
            tbchungtu = ExecuteQuery(querykh, new OleDbParameter("?", ""));

            string directoryPath = Path.Combine(savedPath, DateTime.Now.Month.ToString());
            var excelFiles = Directory.EnumerateFiles(directoryPath, "*.xlsx", SearchOption.AllDirectories).Where(m => m.Contains(mstcongty)).ToList();

            int tongsohodadon = excelFiles.Count;
            int i = 1;
            foreach (var excelFile in excelFiles)
            {
                using (var workbook = new XLWorkbook(excelFile))

                {
                    var worksheet = workbook.Worksheet(1); // Lấy sheet đầu tiên
                    foreach (var row in worksheet.RowsUsed().Skip(3)) // Bỏ qua 6 hàng đầu tiên
                    {
                        string khhd = row.Cell("B").Value.ToString(); // Lấy giá trị của cột A trong hàng hiện tại
                        string getSHHD = row.Cell("C").Value.ToString(); // Lấy giá trị của cột A trong hàng hiện tại
                        string getSohd = RemoveLeadingZeros(row.Cell("D").Value.ToString()); // Lấy giá trị của cột C trong hàng hiện tại 
                        string GetNLap = row.Cell("E").Value.ToString();
                        string mstnb = row.Cell("F").Value.ToString();

                        stateDetail.Text = $"Đang tải hoá đơn {getSohd} ";
                        Application.DoEvents();
                        if (getSohd == "3423")
                        {
                            int a = 10;
                        }
                        //Kiểm tra từ ngày đến ngày
                        DateTime getdate = DateTime.Parse(GetNLap);

                        //Kiểm tra file đã tải rồi
                        var checkfile = savedPath + "\\" + getdate.ToString("yyyyMMdd") + "_" + mstcongty + "_" + getSohd + "_" + getSHHD + ".xml";
                        if (File.Exists(checkfile))
                        {
                            Console.WriteLine("File đã import");
                            continue;
                        }
                        // var checkFile=tbimport.AsEnumerable().Where()
                        var checkExist = tbimport.AsEnumerable().Where(m => m.Field<string>("SHDon") == getSohd && m.Field<DateTime>("Nlap").Date == getdate.Date).FirstOrDefault();
                        if (checkExist != null)
                        {
                            Console.WriteLine("File đã import");
                            continue;
                        }
                        var checkExistct = tbchungtu.AsEnumerable().Where(m => m.Field<string>("SoHieu") == getSohd && m.Field<DateTime>("NgayCT").Date == getdate.Date).FirstOrDefault();
                        if (checkExistct != null)
                        {
                            Console.WriteLine("File đã import");
                            continue;
                        }


                        //Tải file xml
                        string url = "";
                        if (i == 1)
                        {
                            url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-xml?nbmst={mstnb}&khhdon={getSHHD}&shdon={getSohd}&khmshdon={khhd}";
                        }
                        if (i == 2)
                        {

                            url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-xml?nbmst={mstnb}&khhdon={getSHHD}&shdon={getSohd}&khmshdon={khhd}";
                        }
                        //https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-xml?nbmst=036084000738&khhdon=C25MVN&shdon=211&khmshdon=2
                        //https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-xml?nbmst=3502386218&khhdon=C25MAA&shdon=3294&khmshdon=2

                        lookupTbImport.Add((mstcongty, getSohd, getdate.Date, 1));
                        string pathravao = "HDRa";
                        string filename = $"{getdate.ToString("yyyyMMdd")}_{mstcongty}_{getSohd}_{getSHHD}.zip";
                        string path = Path.Combine(directoryPath, filename);
                        string filenamexml = $"{getdate.ToString("yyyyMMdd")}_{mstcongty}_{getSohd}_{getSHHD}.xml";
                        string pathxml = Path.Combine(directoryPath, filenamexml);
                        //Kiểm tra nếu hoá đơn chưa dc tải thì tải về
                        if (!File.Exists(path) && !File.Exists(pathxml))
                        {
                            using (var client = new HttpClient())
                            {
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokken);
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                                client.Timeout = TimeSpan.FromSeconds(10);
                                try
                                {

                                    HttpResponseMessage response = client.GetAsync(url).Result; // Sử dụng .Result
                                    response.EnsureSuccessStatusCode(); // Ném ngoại lệ nếu không thành công

                                    // Đọc nội dung phản hồi dưới dạng byte
                                    var fileBytes = response.Content.ReadAsByteArrayAsync().Result;

                                    // Lưu file ZIP bằng FileStream
                                    using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096))
                                    {
                                        fileStream.Write(fileBytes, 0, fileBytes.Length);
                                    }

                                    Console.WriteLine($"File ZIP đã được lưu tại: {path}");
                                    ExtractZipXML(path); // Giải nén file ZIP 
                                    Application.DoEvents();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");
                                }
                            }
                        }
                    }
                }
                i++;
            }
        }
        DataTable tbimport;
        DataTable tbchungtu;
        List<KhachHang> lstKhachhangs = new List<KhachHang>();
        public DataTable tbKhachhang = new DataTable();
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

        DataTable tbimports { get; set; }
        List<TbImport> lsttbimport = new List<TbImport>();
        public HashSet<(string Mst, string SoHD, string KyHieu, DateTime NLap, int Type)> lookupHoaDonCT { get; }
         = new HashSet<(string Mst, string SoHD, string KyHieu, DateTime NLap, int Type)>();
        // KHAI BÁO NGOÀI HÀM (ở cấp độ class)
        private HashSet<(string MST, string SHDon, DateTime NLap, int Type)> lookupTbImport
             = new HashSet<(string MST, string SHDon, DateTime NLap, int Type)>();
        private void Loadtbimport()
        {
            var query = "SELECT *   FROM tbimport";
            tbimports = ExecuteQuery(query);
            // GỌI EXTENSION METHOD ĐÚNG
            try
            {
                lsttbimport = tbimports.ToList<TbImport>();
                lookupTbImport = new HashSet<(string MST, string SHDon, DateTime NLap, int Type)>(
                    lsttbimport.Select(x => (x.Mst ?? "", x.SHDon ?? "", x.NLap, int.Parse(x.Type)))
                );
            }
            catch (Exception ex)
            {
                // XtraMessageBox.Show(ex.Message);
                richTextBox1.Text = ex.Message;
                Application.DoEvents();
            }
        }
        public async Task DocfileExcelVaoAsynce(string mstcongty, string savedPath, string originpath)
        {
            LoadHoadonCT();
            Loadtbimport();

            string querykh = @" SELECT *  FROM tbimport";
            tbimport = ExecuteQuery(querykh, new OleDbParameter("?", ""));
            querykh = @" SELECT *  FROM Chungtu";
            tbchungtu = ExecuteQuery(querykh, new OleDbParameter("?", ""));

            string directoryPath = Path.Combine(savedPath, DateTime.Now.Month.ToString());
            var excelFiles = Directory.EnumerateFiles(directoryPath, "*.xlsx", SearchOption.AllDirectories)
                                      .Where(m => m.Contains(mstcongty)).ToList();

            int i = 1;
            foreach (var excelFile in excelFiles)
            {
                using (var workbook = new XLWorkbook(excelFile))
                {
                    var worksheet = workbook.Worksheet(1);

                    foreach (var row in worksheet.RowsUsed().Skip(3))
                    {
                        string khhd = row.Cell("B").Value.ToString();
                        string getSHHD = row.Cell("C").Value.ToString();
                        string getSohd = RemoveLeadingZeros(row.Cell("D").Value.ToString());
                        string GetNLap = row.Cell("E").Value.ToString();
                        string mstnb = row.Cell("F").Value.ToString();

                        DateTime getdate = DateTime.Parse(GetNLap);

                        bool daTonTai = lookupHoaDonCT.Contains((mstnb, getSohd, getSHHD, getdate.Date, 1));
                        bool daTonTaiimport = lookupTbImport.Contains((mstnb, getSohd, getdate.Date, 1));

                        if (daTonTai || daTonTaiimport)
                            continue;

                        var checkExist = tbimport.AsEnumerable()
                            .Where(m => m.Field<string>("SHDon") == getSohd &&
                                        m.Field<string>("Mst") == mstnb &&
                                        m.Field<DateTime>("Nlap").Date == getdate.Date)
                            .FirstOrDefault();

                        if (checkExist != null)
                        {
                            Console.WriteLine("File đã import");
                            continue;
                        }

                        var checkExistct = tbchungtu.AsEnumerable()
                            .Where(m => m.Field<string>("SoHieu") == getSohd &&
                                        m.Field<DateTime>("NgayCT").Date == getdate.Date)
                            .FirstOrDefault();

                        if (checkExistct != null)
                        {
                            Console.WriteLine("File đã import");
                            continue;
                        }

                        string url = "";

                        if (i == 1 || i == 2)
                        {
                            url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-xml?nbmst={mstnb}&khhdon={getSHHD}&shdon={getSohd}&khmshdon={khhd}";
                        }

                        if (i == 3)
                        {
                            url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-xml?nbmst={mstnb}&khhdon={getSHHD}&shdon={getSohd}&khmshdon={khhd}";
                        }

                        string filename = $"{getdate:yyyyMMdd}_{mstnb}_{getSohd}_{getSHHD}.zip";
                        string path = Path.Combine(directoryPath, filename);

                        string filenamexml = $"{getdate:yyyyMMdd}_{mstnb}_{getSohd}_{getSHHD}.xml";
                        string pathxml = Path.Combine(directoryPath, filenamexml);

                        string folderpath = Path.Combine(directoryPath);

                        stateDetail.Text = $"Đang tải hóa đơn {getSohd}";
                        Application.DoEvents();

                        lookupTbImport.Add((mstnb, getSohd, getdate.Date, 1));

                        if (!File.Exists(path) && !File.Exists(pathxml))
                        {
                            bool isDownloaded = await DownloadFileWithRetryAsync(
                                url,
                                path,
                                tokken,
                                soluottai,
                                thoigiantai);

                            if (isDownloaded)
                            {
                                Console.WriteLine($"✅ Tải file thành công: {filename}");
                                richTextBox1.Text = $"✅ Tải file thành công: {filename}";
                                Application.DoEvents();

                                ExtractZipXML(path);

                                Application.DoEvents();
                            }
                            else
                            {
                                Console.WriteLine($"❌ Tải file thất bại sau 3 lần thử: {filename}");
                                richTextBox1.Text = $"❌ Tải file thất bại sau 3 lần thử: {filename}";
                                Application.DoEvents();

                                if (i == 2)
                                {
                                    // Nếu GetKNMXML đã chuyển sang async
                                    await GetKNMXMLAsync(
                                        mstnb,
                                        getSHHD,
                                        getSohd,
                                        tokken,
                                        getdate,
                                        folderpath,
                                        filename);

                                    // Nếu GetKNMXML chưa chuyển async thì dùng:
                                    // GetKNMXML(mstnb, getSHHD, getSohd, tokken, getdate, folderpath, filename);
                                }
                            }
                        }
                    }
                }

                i++;
            }
        }
        public async Task<bool> DownloadFileWithRetryAsync(
    string url,
    string filePath,
    string token,
    int soluottai,
    int thoigiantai)
        {
            int retryCount = 0;

            while (retryCount < soluottai)
            {
                retryCount++;

                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(thoigiantai);

                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);

                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                        richTextBox1.Text = $"Lần thử {retryCount}/{soluottai}";
                        Application.DoEvents();

                        HttpResponseMessage response = await client.GetAsync(
                            url,
                            HttpCompletionOption.ResponseHeadersRead);

                        if (response.IsSuccessStatusCode)
                        {
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

                        richTextBox1.Text = $"HTTP {(int)response.StatusCode}";
                        Application.DoEvents();
                    }
                }
                catch (TaskCanceledException)
                {
                    richTextBox1.Text = $"Timeout lần {retryCount}/{soluottai}";
                    Application.DoEvents();
                }
                catch (Exception ex)
                {
                    richTextBox1.Text = ex.Message;
                    Application.DoEvents();
                }

                if (retryCount < soluottai)
                {
                    await Task.Delay(1000);
                }
            }

            return false;
        }
        public void DocfileExcelVao(string mstcongty, string savedPath, string originpath)
        {
            LoadHoadonCT();
            Loadtbimport();

            string querykh = @" SELECT *  FROM tbimport";
            tbimport = ExecuteQuery(querykh, new OleDbParameter("?", ""));
            querykh = @" SELECT *  FROM Chungtu";
            tbchungtu = ExecuteQuery(querykh, new OleDbParameter("?", ""));

            string directoryPath = Path.Combine(savedPath, DateTime.Now.Month.ToString());
            var excelFiles = Directory.EnumerateFiles(directoryPath, "*.xlsx", SearchOption.AllDirectories)
                                      .Where(m => m.Contains(mstcongty)).ToList();

            int i = 1;
            foreach (var excelFile in excelFiles)
            {
                using (var workbook = new XLWorkbook(excelFile))
                {
                    var worksheet = workbook.Worksheet(1);
                    foreach (var row in worksheet.RowsUsed().Skip(3))
                    {
                        string khhd = row.Cell("B").Value.ToString();
                        string getSHHD = row.Cell("C").Value.ToString();
                        string getSohd = RemoveLeadingZeros(row.Cell("D").Value.ToString());
                        string GetNLap = row.Cell("E").Value.ToString();
                        string mstnb = row.Cell("F").Value.ToString();

                        DateTime getdate = DateTime.Parse(GetNLap);

                        // Kiểm tra đã tồn tại chưa
                        bool daTonTai = lookupHoaDonCT.Contains((mstnb, getSohd, getSHHD, getdate.Date, 1));
                        bool daTonTaiimport = lookupTbImport.Contains((mstnb, getSohd, getdate.Date, 1));
                        if (daTonTai || daTonTaiimport)
                        {
                            continue;
                        }

                        // Kiểm tra trong tbimport
                        var checkExist = tbimport.AsEnumerable()
                            .Where(m => m.Field<string>("SHDon") == getSohd &&
                                        m.Field<string>("Mst") == mstnb &&
                                        m.Field<DateTime>("Nlap").Date == getdate.Date)
                            .FirstOrDefault();
                        if (checkExist != null)
                        {
                            Console.WriteLine("File đã import");
                            continue;
                        }

                        // Kiểm tra trong Chungtu
                        var checkExistct = tbchungtu.AsEnumerable()
                            .Where(m => m.Field<string>("SoHieu") == getSohd &&
                                        m.Field<DateTime>("NgayCT").Date == getdate.Date)
                            .FirstOrDefault();
                        if (checkExistct != null)
                        {
                            Console.WriteLine("File đã import");
                            continue;
                        }

                        // Tạo URL
                        string url = "";
                        if (i == 1 || i == 2)
                        {
                            url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-xml?nbmst={mstnb}&khhdon={getSHHD}&shdon={getSohd}&khmshdon={khhd}";
                        }
                        if (i == 3)
                        {
                            url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-xml?nbmst={mstnb}&khhdon={getSHHD}&shdon={getSohd}&khmshdon={khhd}";
                        }

                        string pathravao = "HDVao";
                        string filename = $"{getdate.ToString("yyyyMMdd")}_{mstnb}_{getSohd}_{getSHHD}.zip";
                        string path = Path.Combine(directoryPath, filename);
                        string filenamexml = $"{getdate.ToString("yyyyMMdd")}_{mstnb}_{getSohd}_{getSHHD}.xml";
                        string pathxml = Path.Combine(directoryPath, filenamexml);
                        string folderpath = Path.Combine(directoryPath);
                        stateDetail.Text = $"Đang tải hoa đơn {getSohd}";
                        Application.DoEvents();
                        lookupTbImport.Add((mstnb, getSohd, getdate.Date, 1));

                        // Kiểm tra nếu hoá đơn chưa được tải
                        if (!File.Exists(path) && !File.Exists(pathxml))
                        {
                            // ========================================
                            // TẢI FILE VỚI TIMEOUT VÀ RETRY 3 LẦN
                            // ========================================
                            bool isDownloaded = DownloadFileWithRetry(url, path, tokken, soluottai, thoigiantai);

                            if (isDownloaded)
                            {
                                Console.WriteLine($"✅ Tải file thành công: {filename}");
                                richTextBox1.Text = $"✅ Tải file thành công: {filename}";
                                Application.DoEvents();
                                ExtractZipXML(path); // Giải nén file ZIP
                                Application.DoEvents();
                            }
                            else
                            {
                                Console.WriteLine($"❌ Tải file thất bại sau 3 lần thử: {filename}");
                                richTextBox1.Text = $"❌ Tải file thất bại sau 3 lần thử: {filename}";
                                Application.DoEvents();
                                // Nếu là loại 2, thử tải XML thay thế
                                if (i == 2)
                                {
                                    GetKNMXML(mstnb, getSHHD, getSohd, tokken, getdate, folderpath, filename);
                                }
                            }
                        }
                    }
                }
                i++;
            }
        }

        // ========================================
        // HÀM TẢI FILE VỚI TIMEOUT VÀ RETRY
        // ========================================
        private bool DownloadFileWithRetry(string url, string filePath, string token, int maxRetry = 3, int timeoutMinutes = 5)
        {
            int retryCount = 0;
            bool isDownloaded = false;

            while (retryCount < maxRetry && !isDownloaded)
            {
                retryCount++;
                Console.WriteLine($"Lần thử {retryCount}/{maxRetry} - Đang tải: {Path.GetFileName(filePath)}");

                try
                {
                    using (var client = new HttpClient())
                    {
                        // Set timeout
                        client.Timeout = TimeSpan.FromSeconds(thoigiantai);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                        // Sử dụng Task để kiểm soát timeout
                        var task = client.GetAsync(url);

                        if (task.Wait(TimeSpan.FromSeconds(thoigiantai)))
                        {
                            HttpResponseMessage response = task.Result;

                            if (response.IsSuccessStatusCode)
                            {
                                var fileBytes = response.Content.ReadAsByteArrayAsync().Result;

                                // Lưu file
                                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096))
                                {
                                    fileStream.Write(fileBytes, 0, fileBytes.Length);
                                }

                                isDownloaded = true;
                                Console.WriteLine($"✅ Tải thành công: {Path.GetFileName(filePath)}");
                                richTextBox1.Text = $"✅ Tải thành công: {Path.GetFileName(filePath)}";
                                Application.DoEvents();
                            }
                            else
                            {
                                Console.WriteLine($"❌ Lỗi HTTP: {response.StatusCode} - {response.ReasonPhrase}");
                                richTextBox1.Text = $"❌ Lỗi HTTP: {response.StatusCode} - {response.ReasonPhrase}";
                                Application.DoEvents();
                            }
                        }
                        else
                        {
                            // Timeout
                            Console.WriteLine($"⏰ Timeout! Lần thử {retryCount}/{maxRetry}");
                            richTextBox1.Text = $"⏰ Timeout! Lần thử {retryCount}/{maxRetry}";
                            Application.DoEvents();
                            client.CancelPendingRequests();
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    Console.WriteLine($"⏰ Request bị hủy do timeout: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Lỗi: {ex.Message}");
                }

                // Nếu chưa tải thành công và còn lượt thử
                if (!isDownloaded && retryCount < maxRetry)
                {
                    // Tăng dần thời gian chờ: 5s, 10s, 15s, ...
                    int waitSeconds = 2;
                    Console.WriteLine($"⏳ Chờ {waitSeconds} giây trước khi thử lại...");
                    richTextBox1.Text = $"⏳ Chờ {waitSeconds} giây trước khi thử lại...";
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(waitSeconds * 1000);
                }
            }

            return isDownloaded;
        }
        public async Task GetKNMXMLAsync(string nbmst, string khhdon, string shdon, string tokken, DateTime GetNLap, string path, string filename)
        {
            GDTClient.UpdateToken(tokken);
            string url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/detail?nbmst={nbmst}&khhdon={khhdon}&shdon={shdon}&khmshdon=1";

            using (var client = new HttpClient())
            {
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokken);
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    // Gửi yêu cầu GET đồng bộ
                    string responseBody = Task.Run(async () => await GDTClient.GetJsonAsync(url)).Result;
                    var rootObject = JsonConvert.DeserializeObject<Invoice>(responseBody);
                    // Tạo phần tử gốc <HDon>
                    TaoFileXmlChiCoDLHDon(path, filename.Replace(".zip", ""), rootObject, GetNLap);

                    string ph = Path.Combine(path, filename.Replace(".zip", "_KNM.xml"));
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                }
            }
        }
        public void GetKNMXML(string nbmst, string khhdon, string shdon, string tokken, DateTime GetNLap, string path, string filename)
        {
            GDTClient.UpdateToken(tokken);
            string url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/detail?nbmst={nbmst}&khhdon={khhdon}&shdon={shdon}&khmshdon=1";

            using (var client = new HttpClient())
            {
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokken);
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    // Gửi yêu cầu GET đồng bộ
                    string responseBody = Task.Run(async () => await GDTClient.GetJsonAsync(url)).Result;
                    var rootObject = JsonConvert.DeserializeObject<Invoice>(responseBody);
                    // Tạo phần tử gốc <HDon>
                    TaoFileXmlChiCoDLHDon(path, filename.Replace(".zip", ""), rootObject, GetNLap);

                    string ph = Path.Combine(path, filename.Replace(".zip", "_KNM.xml"));
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                }
            }
        }
        public static class GDTClient2
        {
            private static readonly HttpClient _client;

            static GDTClient2()
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
            // Thay đổi phương thức thành async 
            public static async Task DownloadFileAsync(
     string url,
     string savePath,
     string token = null,
     DateTime dt = default,
     Action<bool, string, long> completionCallback = null)
            {
                if (!string.IsNullOrEmpty(token))
                    UpdateToken(token);

                const int maxRetries = 3;
                int retryCount = 0;

                var sw = Stopwatch.StartNew();

                while (retryCount < maxRetries)
                {
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, url);
                        request.Headers.Accept.Clear();
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                        // Thêm các header khác nếu cần

                        HttpResponseMessage response = new HttpResponseMessage();

                        try
                        {
                            response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false); // QUAN TRỌNG: Không capture UI context 
                            response.EnsureSuccessStatusCode();
                        }
                        catch (Exception ex)
                        {
                            // XtraMessageBox.Show(ex.Message);
                            await Task.Delay(1000); // 2s, 4s, 6s
                        }


                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
                        {
                            await stream.CopyToAsync(fs);
                        }

                        sw.Stop();
                        Console.WriteLine($"Tải thành công: {Path.GetFileName(savePath)} - Thời gian: {sw.ElapsedMilliseconds} ms");

                        ExtractZipXMLAsynce(savePath); // Giải nén file ZIP
                        currentProgress2 += 1;
                        completionCallback?.Invoke(true, $"Tải thành công: {Path.GetFileName(savePath)}", currentProgress2);

                        return; // Thành công → thoát hẳn
                    }
                    catch (Exception ex) when (retryCount < maxRetries - 1) // Chỉ retry nếu còn lượt
                    {
                        retryCount++;
                        Console.WriteLine($"Lỗi tải file lần {retryCount}: {ex.Message}. Thử lại sau 2 giây...");

                        // Optional: delay tăng dần (exponential backoff)
                        await Task.Delay(1000); // 2s, 4s, 6s

                        // Nếu là lỗi mạng/timeout thì tiếp tục retry, các lỗi khác có thể không muốn retry
                        // Bạn có thể lọc cụ thể hơn:
                        // if (ex is HttpRequestException || ex is TaskCanceledException) { ... }
                    }
                }

                // Nếu ra khỏi vòng lặp nghĩa là đã thử 3 lần vẫn thất bại
                sw.Stop();
                string errorMsg = $"Tải file thất bại sau {maxRetries} lần thử: {Path.GetFileName(savePath)}";
                Console.WriteLine(errorMsg);
                completionCallback?.Invoke(false, errorMsg, currentProgress2);

                // Có thể throw hoặc không tùy nhu cầu
                throw new Exception(errorMsg);
            }
        }
        public async Task DocfileExcelVaoAsync(string mstcongty, string savedPath, string originpath)
        {
            GDTClient.UpdateToken(tokken);
            LoadHoadonCT();
            Loadtbimport();
            string querykh = @" SELECT *  FROM tbimport"; // Sử dụng ? thay cho @mst trong OleDb
            tbimport = ExecuteQuery(querykh, new OleDbParameter("?", ""));
            querykh = @" SELECT *  FROM Chungtu"; // Sử dụng ? thay cho @mst trong OleDb
            tbchungtu = ExecuteQuery(querykh, new OleDbParameter("?", ""));

            //  string directoryPath = Path.Combine(savedPath, "HDVao", DateTime.Now.Month.ToString());
            string directoryPath = Path.Combine(savedPath, DateTime.Now.Month.ToString());
            var excelFiles = Directory.EnumerateFiles(directoryPath, "*.xlsx", SearchOption.AllDirectories).Where(m => m.Contains(mstcongty)).ToList();
            int totalInvoices = 0;

            int i = 1;
            // Đếm tổng số hóa đơn cần xử lý (để hiển thị tiến độ chính xác)
            foreach (var excelFile in excelFiles)
            {
                using (var workbook = new XLWorkbook(excelFile))
                {
                    var worksheet = workbook.Worksheet(1);
                    foreach (var row in worksheet.RowsUsed().Skip(3))
                    {
                        string GetNLap = row.Cell("E").Value.ToString();
                        string getSohd = Helpers.RemoveLeadingZeros(row.Cell("D").Value.ToString()); // Lấy giá trị của cột C trong hàng hiện tại 
                        string mstnb = row.Cell("F").Value.ToString();
                        if (DateTime.TryParse(GetNLap, out DateTime getdate))
                        {
                            DateTime gd = DateTime.Parse(GetNLap);
                            bool daTonTai = lookupHoaDonCT.Contains((mstnb, getSohd, "", gd.Date, 1));
                            bool daTonTaiimport = lookupTbImport.Contains((mstnb, getSohd, gd.Date, 1));
                            if (daTonTai || daTonTaiimport)
                            {
                                continue;
                            }
                            totalInvoices++;
                        }
                    }
                }
            }

            foreach (var excelFile in excelFiles)
            {
                using (var workbook = new XLWorkbook(excelFile))

                {
                    var worksheet = workbook.Worksheet(1); // Lấy sheet đầu tiên
                    foreach (var row in worksheet.RowsUsed().Skip(3)) // Bỏ qua 6 hàng đầu tiên
                    {
                        string khhd = row.Cell("B").Value.ToString(); // Lấy giá trị của cột A trong hàng hiện tại
                        string getSHHD = row.Cell("C").Value.ToString(); // Lấy giá trị của cột A trong hàng hiện tại
                        string getSohd = RemoveLeadingZeros(row.Cell("D").Value.ToString()); // Lấy giá trị của cột C trong hàng hiện tại 
                        string GetNLap = row.Cell("E").Value.ToString();
                        string mstnb = row.Cell("F").Value.ToString();

                        //Kiểm tra từ ngày đến ngày
                        DateTime getdate = DateTime.Parse(GetNLap);
                        bool daTonTai = lookupHoaDonCT.Contains((mstnb, getSohd, getSHHD, getdate.Date, 1));
                        bool daTonTaiimport = lookupTbImport.Contains((mstnb, getSohd, getdate.Date, 1));
                        if (daTonTai || daTonTaiimport)
                        {
                            continue;
                        }

                        //Kiểm tra xem hoá đơn đã có trong bảng tbimport chưa


                        // var checkFile=tbimport.AsEnumerable().Where()
                        var checkExist = tbimport.AsEnumerable().Where(m => m.Field<string>("SHDon") == getSohd && m.Field<string>("Mst") == mstnb && m.Field<DateTime>("Nlap").Date == getdate.Date).FirstOrDefault();
                        if (checkExist != null)
                        {
                            Console.WriteLine("File đã import");
                            continue;
                        }
                        if (getSohd == "71881")
                        {
                            int a = 10;
                        }
                        var checkExistct = tbchungtu.AsEnumerable().Where(m => m.Field<string>("SoHieu") == getSohd && m.Field<DateTime>("NgayCT").Date == getdate.Date).FirstOrDefault();
                        if (checkExistct != null)
                        {
                            Console.WriteLine("File đã import");
                            continue;
                        }
                        //Tải file xml
                        string url = "";
                        if (i == 1 || i == 2)
                        {
                            url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-xml?nbmst={mstnb}&khhdon={getSHHD}&shdon={getSohd}&khmshdon={khhd}";
                        }
                        if (i == 3)
                        {
                            url = $"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-xml?nbmst={mstnb}&khhdon={getSHHD}&shdon={getSohd}&khmshdon={khhd}";
                        }


                        string pathravao = "HDVao";
                        string filename = $"{mstnb}_{getSohd}_{getSHHD}.zip";
                        string path = Path.Combine(directoryPath, filename);
                        string filenamexml = $"{mstnb}_{getSohd}_{getSHHD}.xml";
                        string pathxml = Path.Combine(directoryPath, filenamexml);
                        //Kiểm tra nếu hoá đơn chưa dc tải thì tải về
                        if (!File.Exists(path) && !File.Exists(pathxml))
                        {
                            try
                            {
                                // Tối ưu: Bỏ Thread.Sleep không cần thiết 
                                await GDTClient2.DownloadFileAsync(
                                url: url,
                                savePath: path,
                                token: tokken,
                                dt: getdate,
                                completionCallback: (success, message, progressCount) =>  // THÊM CALLBACK
                                {
                                    // CHỈ CHẠY KHI FILE TẢI XONG THẬT SỰ
                                    if (success)
                                    {
                                        // Cập nhật UI
                                        //progressPanel1.Invoke(new Action(async () =>
                                        //{


                                        //    if (currentProgress2 == totalInvoices)
                                        //    {

                                        //    }

                                        //}));

                                        Console.WriteLine($"✅ Đã tải xong: {message}");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"❌ Lỗi: {message}");
                                    }
                                }
                            );
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");

                                //if (i == 2)
                                //    GetKNMXML(mstnb, getSHHD, getSohd, tokken, getdate, folderpath, filename);

                            }
                        }
                    }
                }
                i++;
            }
        }
        static int currentProgress2 = 0;
        private static void ExtractZipXMLAsynce(string path)
        {

            try
            {
                while (File.Exists(path))
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


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi giải nén hoặc xử lý file: {ex.Message}");
            }

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
                    richTextBox1.Text = "File mới tải chưa đủ 30 phút.";
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

                        richTextBox1.Text = $"Đang tải ({retry}/{maxRetry})...";
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

                        richTextBox1.Text = "Tải xong.";
                        Application.DoEvents();

                        return true;
                    }
                }
                catch (TaskCanceledException)
                {
                    richTextBox1.Text = $"Timeout ({retry}/{maxRetry})";
                }
                catch (Exception ex)
                {
                    richTextBox1.Text = ex.Message;
                }

                if (retry < maxRetry)
                    await Task.Delay(1000);
            }

            return false;
        }

        public void Xulyexelvaoold(string token, int _type)
        {
            // Tối ưu: Tính toán datetime một lần
            DateTime dtFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            //DateTime dtTo = dtFrom.AddMonths(1).AddDays(-1);
            DateTime dtTo = DateTime.Now;

            // Tối ưu: Format string một lần
            string formattedDate1 = dtFrom.ToString("dd/MM/yyyyTHH:mm:ss");
            string formattedDate2 = dtTo.ToString("dd/MM/yyyyTHH:mm:ss");

            // Tối ưu: Dùng switch case thay vì nhiều if
            string url, filename;
            switch (_type)
            {
                case 1:
                    url = $@"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==5%20%20%20%20&type=purchase";
                    filename = $"{mstcongty}_HDDienTuDaCapMa.xlsx";
                    break;
                case 2:
                    url = $@"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==6%20%20%20%20&type=purchase";
                    filename = $"{mstcongty}_HDDienTuKhongMa.xlsx";
                    break;
                case 3:
                    url = $@"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==8%20%20%20%20&type=purchase";
                    filename = $"{mstcongty}_HDDienTuMayTinhTien.xlsx";
                    break;
                default:
                    return;
            }
            string currentYear = $"HD{DateTime.Now.Year}";
            string directoryPath = Path.Combine(savedPath, currentYear, "HDVao", DateTime.Now.Month.ToString());
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

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                try
                {
                    // Tối ưu: Bỏ Thread.Sleep không cần thiết
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    //progressPanel1.Caption = $"Đang tải {filePath} ";
                    Application.DoEvents();
                    response.EnsureSuccessStatusCode();

                    var fileBytes = response.Content.ReadAsByteArrayAsync().Result;
                    File.WriteAllBytes(filePath, fileBytes);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");
                }
            }
        }
        public void Xulyexelvao(string token, int _type)
        {
            // Tối ưu: Tính toán datetime một lần
            DateTime dtFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime dtTo = DateTime.Now;

            // Tối ưu: Format string một lần
            string formattedDate1 = dtFrom.ToString("dd/MM/yyyyTHH:mm:ss");
            string formattedDate2 = dtTo.ToString("dd/MM/yyyyTHH:mm:ss");

            // Tối ưu: Dùng switch case thay vì nhiều if
            string url, filename;
            switch (_type)
            {
                case 1:
                    url = $@"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==5%20%20%20%20&type=purchase";
                    filename = $"{mstcongty}_HDDienTuDaCapMa.xlsx";
                    break;
                case 2:
                    url = $@"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==6%20%20%20%20&type=purchase";
                    filename = $"{mstcongty}_HDDienTuKhongMa.xlsx";
                    break;
                case 3:
                    url = $@"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-excel-sold?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge={formattedDate1};tdlap=le={formattedDate2};ttxly==8%20%20%20%20&type=purchase";
                    filename = $"{mstcongty}_HDDienTuMayTinhTien.xlsx";
                    break;
                default:
                    return;
            }

            string currentYear = $"HD{DateTime.Now.Year}";
            string directoryPath = Path.Combine(savedPath, currentYear, "HDVao", DateTime.Now.Month.ToString());
            string filePath = Path.Combine(directoryPath, filename);

            // Tối ưu: Đảm bảo thư mục tồn tại trước
            Directory.CreateDirectory(directoryPath);

            // Kiểm tra file cũ
            if (File.Exists(filePath))
            {
                DateTime lastWriteTime = File.GetLastWriteTime(filePath);
                TimeSpan timeDifference = DateTime.Now - lastWriteTime;

                if (timeDifference.TotalMinutes > 30)
                {
                    File.Delete(filePath);
                    Console.WriteLine($"Đã xóa file cũ: {filePath}");
                }
                else
                {
                    Console.WriteLine($"File chưa đủ 30 phút để xóa. Thời gian còn lại: {30 - timeDifference.TotalMinutes:F1} phút");
                    richTextBox1.Text = $"File chưa đủ 30 phút để xóa. Thời gian còn lại: {30 - timeDifference.TotalMinutes:F1} phút";
                    Application.DoEvents();
                    return;
                }
            }

            // ========================================
            // THÊM TIMEOUT VÀ RETRY 3 LẦN
            // ========================================
            int maxRetry = 3;
            int retryCount = 0;
            bool isDownloaded = false;

            while (retryCount < maxRetry && !isDownloaded)
            {
                retryCount++;
                Console.WriteLine($"Lần thử {retryCount}/{maxRetry} - Đang tải file: {filename}");

                try
                {
                    // Tạo HttpClient với timeout
                    using (var client = new HttpClient())
                    {
                        // Set timeout 5 phút (300 giây)
                        client.Timeout = TimeSpan.FromSeconds(15);


                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                        // Sử dụng async/await hoặc Task để có thể hủy khi timeout
                        var task = client.GetAsync(url);

                        // Chờ response với timeout
                        if (task.Wait(TimeSpan.FromSeconds(15)))
                        {
                            HttpResponseMessage response = task.Result;

                            if (response.IsSuccessStatusCode)
                            {
                                var fileBytes = response.Content.ReadAsByteArrayAsync().Result;
                                File.WriteAllBytes(filePath, fileBytes);

                                Console.WriteLine($"✅ Tải file thành công: {filename}");
                                richTextBox1.Text = $"✅ Tải file thành công: {filename}";
                                Application.DoEvents();
                                isDownloaded = true;
                            }
                            else
                            {
                                Console.WriteLine($"❌ Lỗi HTTP: {response.StatusCode} - {response.ReasonPhrase}");
                                richTextBox1.Text = $"❌ Lỗi HTTP: {response.StatusCode} - {response.ReasonPhrase}";
                                Application.DoEvents();
                            }
                        }
                        else
                        {
                            // Timeout
                            Console.WriteLine($"⏰ Timeout! Lần thử {retryCount}/{maxRetry}");
                            richTextBox1.Text = $"⏰ Timeout! Lần thử {retryCount}/{maxRetry}";
                            Application.DoEvents();
                            // Hủy request
                            client.CancelPendingRequests();
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    Console.WriteLine($"⏰ Request bị hủy do timeout: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Lỗi: {ex.Message}");
                }

                // Nếu chưa tải thành công và còn lượt thử
                if (!isDownloaded && retryCount < maxRetry)
                {
                    // Chờ 5 giây trước khi thử lại
                    int waitSeconds = 1; // Tăng dần thời gian chờ: 5s, 10s, 15s
                    Console.WriteLine($"⏳ Chờ {waitSeconds} giây trước khi thử lại...");
                    richTextBox1.Text = $"⏳ Chờ {waitSeconds} giây trước khi thử lại...";
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(waitSeconds * 1000);
                }
            }

            if (!isDownloaded)
            {
                Console.WriteLine($"❌ Không thể tải file sau {maxRetry} lần thử: {filename}");
                richTextBox1.Text = $"⏰ Timeout! Lần thử  {retryCount} / {maxRetry}";
                Application.DoEvents();
            }
        }
        public void Xulyexelraold(string token, int _type)
        {
            // Tối ưu: Tính toán datetime một lần
            DateTime dtFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime dtTo = dtFrom.AddMonths(1).AddDays(-1);

            // Tối ưu: Format string một lần
            string formattedDate1 = dtFrom.ToString("dd/MM/yyyyTHH:mm:ss");
            string formattedDate2 = dtTo.ToString("dd/MM/yyyyTHH:mm:ss");

            // Tối ưu: Dùng switch case thay vì nhiều if
            string url, filename;
            switch (_type)
            {
                case 1:
                    url = @"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge=" + formattedDate1 + ";tdlap=le=" + formattedDate2;
                    filename = $"{mstcongty}_Hoadondientu.xlsx";
                    break;
                case 2:
                    url = @"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-excel?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge=" + formattedDate1 + ";tdlap=le=" + formattedDate2;
                    filename = $"{mstcongty}_HDDienTuMayTinhTien.xlsx";
                    break;
                default:
                    return;
            }
            string currentYear = $"HD{DateTime.Now.Year}";
            string directoryPath = Path.Combine(savedPath, currentYear, "HDRa", DateTime.Now.Month.ToString());
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

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                try
                {
                    // Tối ưu: Bỏ Thread.Sleep không cần thiết
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    // progressPanel1.Caption = $"Đang tải {filePath} ";
                    Application.DoEvents();
                    response.EnsureSuccessStatusCode();

                    var fileBytes = response.Content.ReadAsByteArrayAsync().Result;
                    File.WriteAllBytes(filePath, fileBytes);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");
                }
            }
        }
        int soluottai = 0;
        int thoigiantai = 0;
        private async void frmAutoTai_Load(object sender, EventArgs e)

        {
            stateDetail.Text = "...";
            getconnectstring();

            MessagteToast("Đang đăng nhập hệ thống thuế...");
            Gettokken();
            var query = "SELECT * FROM KhachHang"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHang
            tbKhachhang = ExecuteQuery(query);
            string querydd = @" SELECT *  FROM tbDinhdanhtaikhoan"; // Sử dụng ? thay cho @mst trong OleDb
            tbDinhDanhtaikhoan = ExecuteQuery(querydd, new OleDbParameter("?", ""));
            tbLicense = ExecuteQuery("SELECT * FROM License", null);

            MessagteToast("Đang tải excel hoá đơn điện tử...");
            Task<bool> t1 = XulyexelvaoAsync(tokken, 1);
            Task<bool> t2 = XulyexelvaoAsync(tokken, 2);
            Task<bool> t3 = XulyexelvaoAsync(tokken, 3);

            bool[] result = await Task.WhenAll(t1, t2, t3);

            // Đến đây chắc chắn cả 3 request đã hoàn thành 
            type = 1;
            string querykh = @" SELECT *  FROM tbRegister"; // Sử dụng ? thay cho @mst trong OleDb

            var tbRegister = ExecuteQuery(querykh, new OleDbParameter("?", ""));
            string originpath = tbRegister.Rows[0]["Hoadonpath"].ToString();
            string currentYear = $"HD{DateTime.Now.Year}";
            string directoryPath = Path.Combine(tbRegister.Rows[0]["Hoadonpath"].ToString(), currentYear, "HDVao");
            MessagteToast("Đang tải hoá đơn đầu vào..");
            //DocfileExcelVao(tbRegister.Rows[0]["Username"].ToString(), directoryPath, originpath);
            //      await DocfileExcelVaoAsynce(
            //tbRegister.Rows[0]["Username"].ToString(),
            //directoryPath,
            //originpath);

            UpdateStatus("📥 Đang tải và xử lý hoá đơn đầu vào...");
            string username = tbRegister.Rows[0]["Username"].ToString();
            var invoicesVao = await GetListHoaDonCanTai(username, originpath, 1);
            await TaiHangLoatHoaDon(invoicesVao, "đầu vào");

            //Xử lý ra 
            Task<bool> t1r = XulyexelraAsync(tokken, 1);
            Task<bool> t2r = XulyexelraAsync(tokken, 2);

            bool[] resultra = await Task.WhenAll(t1r, t2r);

            directoryPath = Path.Combine(tbRegister.Rows[0]["Hoadonpath"].ToString(), currentYear, "HDRa");
            MessagteToast("Đang tải hoá đơn đầu ra..");
            //DocfileExcelRa(tbRegister.Rows[0]["Username"].ToString(), directoryPath, originpath);
            //        await DocfileExcelRaAsync(
            //tbRegister.Rows[0]["Username"].ToString(),
            //directoryPath,
            //originpath);

            var invoicesRa = await GetListHoaDonCanTai(username, originpath, 2);
            await TaiHangLoatHoaDon(invoicesRa, "đầu ra");

            //XtraMessageBox.Show("Đã hoàn thành tự động tải hoá đơn"+ mstcongty); 
            //XulylietkeHoaDon(1);
           // XulylietkeHoaDon(2);
        }
        private List<TbImport> lstdsVao = new List<TbImport>();

        private List<TbImport> lstdsRa = new List<TbImport>();
        DataTable ListPhanloaiVattu;
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
                    try
                    {
                        // Lưu trữ dữ liệu cần thiết để tránh closure issues
                        var maSo = int.Parse(item["MaSo"].ToString());
                        var maPhanLoai = int.Parse(item["MaPhanLoai"].ToString());
                        var tenVattu = item["TenVattu"].ToString();
                        var tenVattu2 = item["TenVattu2"].ToString();
                        var soHieu = item["SoHieu"].ToString();
                        var donVi = item["DonVi"].ToString();
                        var ghiChu = item["GhiChu"].ToString();
                        var tenMaPhanLoai = ListPhanloaiVattu.AsEnumerable()
                            .Where(m => m["MaSo"].ToString() == item["MaPhanLoai"].ToString())
                            .FirstOrDefault()?["TenPhanLoai"].ToString() ?? string.Empty;
                        var ptgb = item["PTGB"].ToString();

                        var task = Task.Run(() =>
                        {
                            var VatTu = new DTO.VatTu
                            {
                                MaSo = maSo,
                                MaPhanLoai = maPhanLoai,
                                TenVattu = tenVattu,
                                TenVattu2 = tenVattu2,
                                SoHieu = soHieu,
                                DonVi = donVi,
                                GhiChu = ghiChu,
                                TenMaPhanLoai = tenMaPhanLoai,
                                PTGB = ptgb,
                            };

                            // Kiểm tra và lấy dữ liệu từ TonKho (nếu có)
                            if (tonKhoDict.TryGetValue(VatTu.MaSo, out DataRow tonKhoRow))
                            {
                                int cnt = 12;

                                // Lấy số lượng và thành tiền
                                var soluong = tonKhoRow["Luong_" + cnt] != DBNull.Value
                                    ? double.Parse(tonKhoRow["Luong_" + cnt].ToString())
                                    : 0;
                                VatTu.SoLuong = soluong;
                                //Tìm số lượng thông qua tbchungtu


                                var thanhtien = tonKhoRow["Tien_" + cnt] != DBNull.Value
                                   ? double.Parse(tonKhoRow["Tien_" + cnt].ToString())
                                   : 0;
                                VatTu.ThanhTien = thanhtien;

                                // Tính đơn giá nếu có dữ liệu
                                if (soluong != 0 && thanhtien != 0)
                                {
                                    VatTu.Dongia = thanhtien / soluong;
                                }
                                try
                                {
                                    if (existingTbChungtu != null)
                                    {
                                        var findlstct = existingTbChungtu.AsEnumerable().Where(m => int.Parse(m["MaVattu"].ToString()) == VatTu.MaSo && double.Parse(m["SoPS"].ToString()) != 0 && double.Parse(m["SoPS2Co"].ToString()) != 0 && !m["SoHieu"].ToString().Contains("V")).ToList().LastOrDefault();
                                        if (findlstct != null)
                                        {
                                            if (findlstct["MaSo"].ToString() == "174028")
                                            {
                                                int dd = 10;
                                            }
                                            double SoPS2Co = double.Parse(findlstct["SoPS2Co"].ToString());
                                            double SoPS = double.Parse(findlstct["SoPS"].ToString());
                                            if (SoPS2Co > 0)
                                                VatTu.Dongia2 = Math.Round(findlstct.Field<double>("SoPS") / SoPS2Co);
                                            else
                                                VatTu.Dongia2 = 0;
                                        }
                                        //if (VatTu.Dongia2 != 0)
                                        //    VatTu.Dongia = VatTu.Dongia2;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    // XtraMessageBox.Show($"Lỗi khi tính đơn giá vật tư {VatTu.Dongia2}: {ex.Message}");
                                    richTextBox1.Text = $"Lỗi khi tính đơn giá vật tư {VatTu.Dongia2}: {ex.Message}";
                                    Application.DoEvents();
                                }
                            }
                            return VatTu;
                        });

                        vatTuTasks.Add(task);
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show($"Lỗi khi khởi tạo Task vật tư: {ex.Message}");
                    }
                }

                // 7. Đợi tất cả các Task hoàn thành và thêm vào danh sách kết quả
                try
                {
                    var vatTus = await Task.WhenAll(vatTuTasks);
                    lstVattu.AddRange(vatTus.Where(v => v != null));
                }
                catch (AggregateException aggEx)
                {
                    foreach (var ex in aggEx.InnerExceptions)
                    {
                        XtraMessageBox.Show($"Lỗi khi xử lý vật tư: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show($"Lỗi khi xử lý vật tư: {ex.Message}");
                }


                //  XtraMessageBox.Show("Load vattu thanh cong");
            }
            catch (Exception ex)
            {
                // Xử lý lỗi (có thể log hoặc hiển thị thông báo)
                Console.WriteLine($"Lỗi khi tải dữ liệu: {ex.Message} ");
                throw; // Re-throw nếu cần thiết
            }
            finally
            {
                // Đóng popup loading chỉ khi mọi thứ đã hoàn tất
            }
            // BuildFastLookup();

            _lookupByTenChinh = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _lookupByTenPhu = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _lookupByTenChinhs = new Dictionary<string, VatTuInfo>(StringComparer.OrdinalIgnoreCase);

            vatTuLookup = lstVattu
           .ToDictionary(v => v.SoHieu, v => (v.TenVattu, v.TenVattu2, v.DonVi, v.Dongia, v.SoLuong));
            foreach (var kvp in vatTuLookup)
            {
                string sohieu = kvp.Key;
                if (sohieu == "TBC-001")
                {
                    int test = 10;
                }

                // DÙNG CHÍNH XÁC HÀM NormalizeForLookup
                string key1 = Helpers.NormalizeVietnameseString(kvp.Value.TenVattu.Trim());

                VietnameseProductMatcher vietnameseProductMatcher = new VietnameseProductMatcher();
                key1 = vietnameseProductMatcher.NormalizeVietnameseProduct(key1);

                if (!string.IsNullOrEmpty(key1))
                {
                    _lookupByTenChinh[key1] = sohieu;
                    _lookupByTenChinhs[key1] = new VatTuInfo
                    {
                        Ma = sohieu,
                        DonViTinh = kvp.Value.DonVi
                    };
                }

                if (!string.IsNullOrEmpty(kvp.Value.TenVattu2))
                {
                    string key2 = Helpers.NormalizeVietnameseString(kvp.Value.TenVattu2.Trim());
                    if (!string.IsNullOrEmpty(key2))
                        _lookupByTenPhu[key2] = sohieu;
                }
            }
            InitializeVatTuOptimization();
            return lstVattu;
        }
        private Dictionary<string, (string TenChuan, string TenPhuChuan, string QuyCach, string DonVi, double Dongia, double soluong)> _optimizedVatTu;

        private void InitializeVatTuOptimization()
        {
            _optimizedVatTu = new Dictionary<string, (string, string, string, string, double, double)>();
            Regex regex = new Regex(@"(\d+(g|ml|L|kg)|x\d+|(\d+\s*cái))", RegexOptions.IgnoreCase);

            foreach (var item in vatTuLookup)
            {
                string ten1 = Helpers.NormalizeVietnameseString(item.Value.TenVattu);
                string ten2 = Helpers.NormalizeVietnameseString(item.Value.TenVattu2);
                string quyCach = regex.Match(ten1).Value;

                _optimizedVatTu[item.Key] = (ten1, ten2, quyCach, item.Value.DonVi, item.Value.Dongia, item.Value.SoLuong);
            }
        }
        private async Task XulylietkeHoaDon(int type)
        {
            lstvt = await LoadDataVattuAsync();
            string pathType = type == 1 ? "HDVao" : "HDRa";
            int fromMonth = DateTime.Now.Month;
            int toMonth = DateTime.Now.Month;
            string pathYear = $"HD{DateTime.Now.Year}";
            // 2. Gom tất cả file XML
            List<string> allFiles = new List<string>();
            for (int m = fromMonth; m <= toMonth; m++)
            {
                string monthFolder = Path.Combine(savedPath, pathYear, pathType, m.ToString());
                if (Directory.Exists(monthFolder))
                {
                    var filesInMonth = Directory.GetFiles(monthFolder, "*.xml", SearchOption.TopDirectoryOnly);
                    allFiles.AddRange(filesInMonth);
                }
            }
            List<TbImport> allInvoicesToSave = new List<TbImport>();
            int batchSize = 10;
            for (int i = 0; i < allFiles.Count; i += batchSize)
            {
                var batch = allFiles.Skip(i).Take(batchSize);

                // Tạo các task đọc file
                var tasks = batch.Select(file => DocfileXmlOne(file, 1, type));

                // --- ĐOẠN QUAN TRỌNG: Hứng kết quả từ các file vừa đọc ---
                TbImport[] results = await Task.WhenAll(tasks);

                foreach (var item in results)
                {
                    if (item != null) // Chỉ lấy những file bóc tách thành công và không trùng
                    {
                        allInvoicesToSave.Add(item);

                        // Cập nhật vào danh sách hiển thị trên giao diện (Grid)
                        if (type == 1) lstdsVao.Add(item);
                        else lstdsRa.Add(item);
                    }
                }

                // Cập nhật UI
                Application.DoEvents();
            }
            if (allInvoicesToSave.Count > 0)
            {
                await SaveAllInvoicesBulk(allInvoicesToSave, type == 1 ? 1 : 2);

            }
        }
        private async Task SaveAllInvoicesBulk(List<TbImport> invoices, int type)
        {
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                await conn.OpenAsync();
                using (OleDbTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        string sqlParent = @"INSERT INTO tbImport (SHDon, KHHDon, NLap, Ten, Noidung, TKNo, TKCo, TkThue, Mst, [Status], Ngaytao, TongTien, Vat, TPhi, TgTCThue, TgTThue, [Type], InvoiceType, IsHaschild, TVat, Vat2, TVat2, Vat3, TVat3, TgTCThue1, TgTCThue2, TgTCThue3, Khmshdon, hdon, [Path]) 
                                     VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";

                        string sqlDetail = @"INSERT INTO tbimportdetail (ParentId, SoHieu, SoLuong, DonGia, DVT, Ten, MaCT, TKNo, TKCo, TTien, [Percent], Tchat,SoPSGoc,VAT) 
                                     VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?)";

                        foreach (var item in invoices)
                        {
                            int parentID = 0;

                            // 1. Lưu Hóa đơn chính (Parent)
                            using (OleDbCommand cmdParent = new OleDbCommand(sqlParent, conn, trans))
                            {
                                cmdParent.Parameters.AddWithValue("?", item.SHDon ?? "");
                                cmdParent.Parameters.AddWithValue("?", item.KHHDon ?? "");
                                cmdParent.Parameters.AddWithValue("?", item.NLap);
                                cmdParent.Parameters.AddWithValue("?", item.Ten ?? "");
                                cmdParent.Parameters.AddWithValue("?", item.Noidung ?? "");
                                cmdParent.Parameters.AddWithValue("?", item.TKNo ?? "");
                                cmdParent.Parameters.AddWithValue("?", item.TKCo ?? "");
                                cmdParent.Parameters.AddWithValue("?", item.TkThue ?? "");
                                cmdParent.Parameters.AddWithValue("?", item.Mst ?? "");
                                cmdParent.Parameters.AddWithValue("?", "0"); // Status
                                cmdParent.Parameters.AddWithValue("?", DateTime.Now.ToShortDateString());
                                cmdParent.Parameters.AddWithValue("?", item.TongTien);
                                cmdParent.Parameters.AddWithValue("?", item.Vat);
                                cmdParent.Parameters.AddWithValue("?", item.TPhi ?? "0");
                                cmdParent.Parameters.AddWithValue("?", Math.Round(item.TgTCThue));
                                cmdParent.Parameters.AddWithValue("?", Math.Round(item.TgTThue));
                                cmdParent.Parameters.AddWithValue("?", type);
                                cmdParent.Parameters.AddWithValue("?", "0"); // InvoiceType
                                cmdParent.Parameters.AddWithValue("?", "1"); // IsHaschild
                                cmdParent.Parameters.AddWithValue("?", item.TVat);
                                cmdParent.Parameters.AddWithValue("?", item.Vat2 ?? "0");
                                cmdParent.Parameters.AddWithValue("?", item.TVat2);
                                cmdParent.Parameters.AddWithValue("?", item.Vat3 ?? "0");
                                cmdParent.Parameters.AddWithValue("?", item.TVat3);
                                cmdParent.Parameters.AddWithValue("?", item.TgTCThue1);
                                cmdParent.Parameters.AddWithValue("?", item.TgTCThue2);
                                cmdParent.Parameters.AddWithValue("?", item.TgTCThue3);
                                cmdParent.Parameters.AddWithValue("?", item.Khmshdon ?? "");
                                cmdParent.Parameters.AddWithValue("?", item.hdon ?? "");
                                cmdParent.Parameters.AddWithValue("?", item.Path ?? "");

                                await cmdParent.ExecuteNonQueryAsync();
                            }

                            // 2. Lấy ID tự tăng vừa tạo
                            using (OleDbCommand cmdId = new OleDbCommand("SELECT @@IDENTITY", conn, trans))
                            {
                                var objId = await cmdId.ExecuteScalarAsync();
                                parentID = Convert.ToInt32(objId);
                            }

                            // 3. Lưu chi tiết hàng hóa (Details)
                            foreach (var dt in item.tbImportDetails)
                            {
                                using (OleDbCommand cmdDetail = new OleDbCommand(sqlDetail, conn, trans))
                                {
                                    cmdDetail.Parameters.AddWithValue("?", parentID);
                                    cmdDetail.Parameters.AddWithValue("?", dt.SoHieu ?? "");
                                    cmdDetail.Parameters.AddWithValue("?", dt.Soluong);
                                    cmdDetail.Parameters.AddWithValue("?", dt.Dongia);
                                    cmdDetail.Parameters.AddWithValue("?", dt.DVT ?? "");
                                    cmdDetail.Parameters.AddWithValue("?", dt.Ten ?? "");
                                    cmdDetail.Parameters.AddWithValue("?", ""); // MaCT
                                    cmdDetail.Parameters.AddWithValue("?", dt.TKNo ?? "");
                                    cmdDetail.Parameters.AddWithValue("?", dt.TKCo ?? "");
                                    cmdDetail.Parameters.AddWithValue("?", dt.TTien);
                                    cmdDetail.Parameters.AddWithValue("?", dt.Percent);
                                    cmdDetail.Parameters.AddWithValue("?", dt.Tchat);
                                    cmdDetail.Parameters.AddWithValue("?", dt.TTien);
                                    cmdDetail.Parameters.AddWithValue("?", dt.Vat);
                                    await cmdDetail.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        // Chốt giao dịch: Ghi toàn bộ xuống ổ cứng
                        trans.Commit();
                        this.Close();
                        //XtraMessageBox.Show($"Đã lưu thành công tổng cộng {invoices.Count} hóa đơn vào Database!", "Thông báo");
                    }
                    catch (Exception ex)
                    {
                        // Nếu có bất kỳ lỗi nào, hủy bỏ toàn bộ để tránh dữ liệu rác
                        trans.Rollback();
                        XtraMessageBox.Show("Lỗi hệ thống khi lưu hàng loạt: " + ex.Message, "Lỗi Database");
                    }
                }
            }
        }
        bool isAddhd = true;
        public string GenerateAbbreviation(string fullName, List<string> existingNames)
        {
            // Tách tên thành từng phần
            string[] nameParts = fullName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string abbreviation = "";

            // Tạo viết tắt
            foreach (string part in nameParts)
            {
                abbreviation += part[0].ToString().ToLower();
            }

            // Kiểm tra sự tồn tại của viết tắt
            int counter = 1;
            string uniqueAbbreviation = abbreviation.ToUpper();

            while (existingNames.Contains(uniqueAbbreviation))
            {
                uniqueAbbreviation = abbreviation + "-" + counter;
                counter++;
            }

            return uniqueAbbreviation;
        }
        private bool Kiemtrahoadon(string SHDon, DateTime NLap, string MST, int type)
        {
            // Tạo tuple từ 3 tham số
            var key = (MST, SHDon, NLap, type);

            // Kiểm tra trong lookup
            return lookupTbImport.Contains(key);
        }
        private bool KiemtrahoadonCT(string SoHD, string KyHieu, DateTime NLap, string Mst, int tpye)
        {
            if (Mst == "KL")
                Mst = "00";
            if (Mst.Length < 10)
                return lookupHoaDonCT.Any(m => m.SoHD == SoHD && m.KyHieu == KyHieu && m.NLap == NLap && m.Type == type);
            return lookupHoaDonCT.Contains((Mst, SoHD, KyHieu, NLap, tpye));
        }
        private bool CheckExistKH(string mst)
        {

            //Nếu có Mã s61 thuế
            if (!string.IsNullOrEmpty(mst))
            {
                if (tbKhachhang.AsEnumerable().Any(row => row.Field<string>("MST") == mst || row.Field<string>("SoHieu") == mst))
                {
                    return true;
                }
            }

            return false;
        }
        string csohieu = "";
        public void InitCustomer(int Maphanloai, string Sohieu, string Ten, string Diachi, string Mst, string cccd, string sdt)
        {
            if (string.IsNullOrEmpty(sdt))
                sdt = "xxx";
            int randNumber = 0;
            Random random = new Random();

            //Xử lý địa chỉ
            string diachiKHVni = !string.IsNullOrEmpty(Diachi) ? Helpers.ConvertUnicodeToVni(Diachi) : Helpers.ConvertUnicodeToVni("Bổ sung địa chỉ");

            if (string.IsNullOrEmpty(Mst))
            {
                //Truong hợp ko có mst và cccd
                if (string.IsNullOrEmpty(cccd))
                {

                    Sohieu = GenerateAbbreviation(Helpers.ConvertVniToUnicode(Ten), tbKhachhang.AsEnumerable().Select(row => row.Field<string>("SoHieu")).ToList()).ToUpper();
                    csohieu = Sohieu;
                    Mst = "00";

                    //Xử lý khi số hiệu bị trùng
                    int suffix = 1;
                    string originalSohieu = Sohieu;

                    while (tbKhachhang.AsEnumerable().Any(row => row.Field<string>("SoHieu") == Sohieu))
                    {
                        Sohieu = $"{originalSohieu}_{suffix}";
                        suffix++;
                    }
                }
                //Không có mst nhưng có cccd
                else
                {
                    Sohieu = cccd.Substring(cccd.Length - 6);
                    Mst = cccd;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(Sohieu))
                {
                    Sohieu = Helpers.GetLastFourDigits(Mst.Replace("-", ""));

                    string tenKHVni = Helpers.ConvertUnicodeToVni(Ten);

                    //Xử lý khi số hiệu bị trùng
                    if (tbKhachhang.AsEnumerable().Any(row => row.Field<string>("SoHieu") == Sohieu))
                    {
                        Sohieu = "0" + Sohieu;
                    }
                    if (tbKhachhang.AsEnumerable().Any(row => row.Field<string>("SoHieu") == Sohieu))
                    {
                        Sohieu = "00" + Sohieu;
                    }
                }
            }
            //Nếu tồn tại so hiệu r, sẽ thêm kí tự
            if (tbKhachhang.AsEnumerable().Any(row => row.Field<string>("SoHieu") == Sohieu))
            {
                Sohieu = Sohieu + "_1";
            }
            if (Mst == Sohieu && Mst.Length <= 8)
            {
                Mst = "00";
            }
            else
            {
                if (Mst.Length > 8)
                {
                    Sohieu = Helpers.GetLastFourDigits(Mst.Replace("-", ""));
                    //Kiểm tra SoHieu co trung thêm 1 lần
                    if (tbKhachhang.AsEnumerable().Any(row => row.Field<string>("SoHieu") == Sohieu))
                    {
                        Sohieu = "0" + Sohieu;
                    }
                    if (tbKhachhang.AsEnumerable().Any(row => row.Field<string>("SoHieu") == Sohieu))
                    {
                        Sohieu = "00" + Sohieu;
                    }
                }
            }

            if (mstcongty == "3501972322")
            {
                int lastid = 0;
                string qr = "SELECT MAX(MaSo) FROM KhachHang";
                var getlastid = ExecuteQuery(qr).Rows[0]["Expr1000"].ToString();
                string query = @"
        INSERT INTO KhachHang (MaSo,MaPhanLoai,SoHieu,Ten,DiaChi,MST,Tel)
        VALUES (?,?,?,?,?,?,?)";


                // Khai báo mảng tham số với đủ 10 tham số
                OleDbParameter[] parameters = new OleDbParameter[]
                {
               new OleDbParameter("?", int.Parse(getlastid)+1),
               new OleDbParameter("?", Maphanloai),
               new OleDbParameter("?", Sohieu),
               new OleDbParameter("?", Ten),
               new OleDbParameter("?", diachiKHVni),
               new OleDbParameter("?", Mst),
               new OleDbParameter("?", sdt),
                };

                // Thực thi truy vấn và lấy kết quả
                try
                {
                    int a = ExecuteQueryResult(query, parameters);
                    query = "SELECT * FROM KhachHang"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHang
                    tbKhachhang = ExecuteQuery(query);
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show(ex.Message + "    " + Ten + "   " + cccd);
                }
            }
            else
            {

                string query = @"
        INSERT INTO KhachHang (MaPhanLoai,SoHieu,Ten,DiaChi,MST,Tel)
        VALUES (?,?,?,?,?,?)";


                // Khai báo mảng tham số với đủ 10 tham số
                OleDbParameter[] parameters = new OleDbParameter[]
                {
               new OleDbParameter("?", Maphanloai),
               new OleDbParameter("?", Sohieu),
               new OleDbParameter("?", Ten),
               new OleDbParameter("?", diachiKHVni),
               new OleDbParameter("?", Mst),
               new OleDbParameter("?", sdt),
                };

                // Thực thi truy vấn và lấy kết quả
                try
                {
                    int a = ExecuteQueryResult(query, parameters);
                    query = "SELECT * FROM KhachHang"; // Giả sử bạn muốn lấy tất cả dữ liệu từ bảng KhachHang
                    tbKhachhang = ExecuteQuery(query);
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show(ex.Message + "    " + Ten + "   " + cccd);
                }
            }
        }
        private async Task<TbImport> DocfileXmlOne(string pathXml, int stt, int type)
        {
            if (pathXml.Contains("427"))
            {
                int kiemtra = 10;
            }
            isAddhd = true;

            if (pathXml.Contains("html"))
                return null;

            TbImport tbImport = null;
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Parse; // Cho phép phân tích DTD
                settings.XmlResolver = null; // Ngăn chặn việc tải DTD từ bên ngoài để bảo mật và tốc độ
                settings.Async = true;
                using (var xmlReader = XmlReader.Create(pathXml, new XmlReaderSettings { Async = true }))
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlReader);
                    XmlNode root = xmlDoc.DocumentElement;
                    if (root == null) return null;

                    // 1. Khai báo các Node cha để tối ưu truy vấn (tránh dùng // liên tục)
                    XmlNode ttChung = root.SelectSingleNode("//TTChung");
                    XmlNode nBan = root.SelectSingleNode("//NBan");
                    XmlNode nMua = root.SelectSingleNode("//NMua");
                    XmlNode ttToan = root.SelectSingleNode("//TToan");
                    XmlNode THDon = root.SelectSingleNode("//THDon");
                    if (ttChung == null || ttToan == null) return null;

                    tbImport = new TbImport { Path = pathXml };


                    if (Helpers.NormalizeVietnameseString(THDon.InnerText.ToLower()).Contains("hóa đơn giá trị gia tăng") || Helpers.NormalizeVietnameseString(THDon.InnerText.ToLower()).Contains("hóa đơn điện tử giá trị gia tăng"))
                    {
                        tbImport.hdon = "01";
                    }
                    if (Helpers.NormalizeVietnameseString(THDon.InnerText.ToLower()).Contains("hóa đơn bán hàng"))
                    {
                        tbImport.hdon = "02";
                    }

                    // 2. Xử lý NLap và nội dung điều chỉnh
                    if (DateTime.TryParse(ttChung.SelectSingleNode("NLap")?.InnerText, out DateTime nLap))
                        tbImport.NLap = nLap;

                    // Kiểm tra trong khoảng ngày 
                    // Nội dung điều chỉnh từ TTKhac
                    var ttKhacNodes = ttChung.SelectNodes("TTKhac/TTin");
                    if (ttKhacNodes != null)
                    {
                        foreach (XmlNode node in ttKhacNodes)
                        {
                            string dLieu = node.SelectSingleNode("DLieu")?.InnerText;
                            if (!string.IsNullOrEmpty(dLieu) && dLieu.IndexOf("điều chỉnh", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                tbImport.Noidung = Helpers.ConvertUnicodeToVni(dLieu);
                                break;
                            }
                        }
                    }

                    // 3. Thông tin số hóa đơn & Ký hiệu
                    tbImport.SHDon = Helpers.RemoveLeadingZeros(ttChung.SelectSingleNode("SHDon")?.InnerText);
                    if (tbImport.SHDon == "1" && tbImport.KHHDon == "C26MKK")
                    {
                        int test = 10;
                    }
                    tbImport.KHHDon = ttChung.SelectSingleNode("KHHDon")?.InnerText;
                    //Lấy thông tin thay thế

                    string TPhi = ttToan.SelectSingleNode("//TPhi")?.InnerText;
                    if (!string.IsNullOrEmpty(TPhi))
                    {
                        tbImport.TPhi = double.Parse(TPhi).ToString();
                    }
                    // 4. Phân loại Mua/Bán và Kiểm tra MST công ty
                    if (type == 1) // Hóa đơn đầu vào
                    {
                        GetMST();
                        // if (mstcongty != nMua?.SelectSingleNode("MST")?.InnerText && CCCD != nMua?.SelectSingleNode("MST")?.InnerText) return null;
                        tbImport.Ten = Helpers.ConvertUnicodeToVni(nBan?.SelectSingleNode("Ten")?.InnerText ?? "");
                        tbImport.Mst = nBan?.SelectSingleNode("MST")?.InnerText ?? "";
                    }
                    else // Hóa đơn đầu ra
                    {
                        //if (mstcongty != nBan?.SelectSingleNode("MST")?.InnerText)
                        //{
                        //    return null;
                        //}

                        string tenDoiTac =
  !string.IsNullOrWhiteSpace(nMua?.SelectSingleNode("Ten")?.InnerText)
      ? nMua.SelectSingleNode("Ten").InnerText
      : nMua?.SelectSingleNode("HVTNMHang")?.InnerText;

                        tbImport.Ten = !string.IsNullOrEmpty(tenDoiTac) ? Helpers.ConvertUnicodeToVni(tenDoiTac) : "";
                        tbImport.Mst = nMua?.SelectSingleNode("MST")?.InnerText ?? nMua?.SelectSingleNode("CCCDan")?.InnerText ?? "";

                        // Xử lý khách lẻ
                        if (string.IsNullOrEmpty(tbImport.Ten) || tbImport.Ten.Contains("khaùch khoâng laáy hoùa ñôn") || tbImport.Ten.Contains("Ngöôøi mua khoâng laáy hoùa ñôn") || tbImport.Ten.Contains("Khaùch leû"))
                        {
                            var kl = tbKhachhang.AsEnumerable().FirstOrDefault(m => m.Field<string>("SoHieu") == "KL");
                            if (kl != null) { tbImport.Ten = kl.Field<string>("Ten"); tbImport.Mst = "KL"; }
                        }
                    }

                    // 5. Tạo Số hiệu tự động nếu không có MST

                    //Kiểm tra tồn tại khách hàng


                    if (string.IsNullOrEmpty(tbImport.Mst) && !string.IsNullOrEmpty(tbImport.Ten))
                    {
                        var existingSoHieus = tbKhachhang.AsEnumerable().Where(m => m.Field<string>("Ten").ToLower() == tbImport.Ten.ToLower()).Select(r => r.Field<string>("SoHieu")).ToList();
                        if (existingSoHieus == null || existingSoHieus.Count == 0)
                        {
                            string sohieuBase = GenerateAbbreviation(Helpers.ConvertVniToUnicode(tbImport.Ten), existingSoHieus).ToUpper();
                            string finalSH = sohieuBase;
                            int suffix = 1;
                            while (tbKhachhang.AsEnumerable().Any(r => r.Field<string>("SoHieu") == finalSH))
                                finalSH = $"{sohieuBase}_{suffix++}";
                            tbImport.Mst = finalSH;
                        }
                        else
                        {
                            tbImport.Mst = existingSoHieus.FirstOrDefault();
                        }

                    }

                    // 6. Kiểm tra trùng hóa đơn (Cache & Database)
                    var currentList = type == 1 ? lstdsVao : lstdsRa;
                    var importList = type == 1 ? lstImportVao : lstImportRa;
                    if (currentList.Any(m => m.SHDon == tbImport.SHDon && m.NLap.Date == tbImport.NLap.Date && m.Mst == tbImport.Mst)) return null;
                    if (importList.Any(m => m.SHDon == tbImport.SHDon && m.NLap.Date == tbImport.NLap.Date)) return null;
                    if ((Kiemtrahoadon(tbImport.SHDon, tbImport.NLap, tbImport.Mst, type)) || KiemtrahoadonCT(tbImport.SHDon, tbImport.KHHDon, tbImport.NLap, tbImport.Mst, type))
                    {
                        isAddhd = false;
                        return null;
                    }

                    // 7. Khởi tạo khách hàng mới
                    if (tbImport.Mst != "KL" && !CheckExistKH(tbImport.Mst))
                    {
                        XmlNode doiTacNode = (type == 1) ? nBan : nMua;
                        string dChi = doiTacNode?.SelectSingleNode("DChi")?.InnerText ?? "";
                        string sdt = doiTacNode?.SelectSingleNode("SDThoai")?.InnerText ?? "";
                        if (!string.IsNullOrEmpty(tbImport.Mst) && !string.IsNullOrEmpty(tbImport.Ten) && !CheckExistKH(tbImport.Mst))
                            InitCustomer(type == 1 ? 2 : 3, tbImport.Mst, tbImport.Ten, dChi, tbImport.Mst, "", sdt);
                        else
                        {
                            var kl = tbKhachhang.AsEnumerable().FirstOrDefault(m => m.Field<string>("SoHieu") == "KL");
                            if (kl != null) { tbImport.Ten = kl.Field<string>("Ten"); tbImport.Mst = "KL"; }
                        }
                    }

                    // 8. Định danh tài khoản
                    string kw = type == 1 ? "Ưu tiên vào" : "Ưu tiên ra";
                    var authRow = tbDinhDanhtaikhoan.AsEnumerable().FirstOrDefault(r => r.Field<string>("KeyValue")?.Contains(kw) == true);
                    if (authRow != null)
                    {
                        tbImport.TKNo = authRow["TKNo"]?.ToString();
                        tbImport.TKCo = authRow["TKCo"]?.ToString();
                        tbImport.TkThue = authRow["TKThue"]?.ToString();
                    }
                    tbImport.Status = 0;
                    tbImport.Ngaytao = DateTime.Now.ToShortDateString();

                    // 9. Tiền thanh toán và Thuế suất
                    tbImport.TongTien = double.Parse(ttToan.SelectSingleNode("TgTTTBSo")?.InnerText ?? "0");
                    tbImport.TgTCThue = double.Parse(ttToan.SelectSingleNode("TgTCThue")?.InnerText ?? "0");
                    tbImport.TgTThue = double.Parse(ttToan.SelectSingleNode("TgTThue")?.InnerText ?? "0");

                    tbImport.Vat = 0;
                    tbImport.Vat2 = "0";
                    tbImport.Vat3 = "0";
                    var thueNodes = ttToan.SelectNodes("THTTLTSuat//LTSuat");
                    if (thueNodes != null)
                    {
                        for (int i = 0; i < thueNodes.Count; i++)
                        {
                            XmlNode n = thueNodes[i];
                            string tsStr = n.SelectSingleNode("TSuat")?.InnerText ?? "";
                            double ttien = double.Parse(n.SelectSingleNode("ThTien")?.InnerText ?? "0");
                            double tthue = Math.Round(double.Parse(n.SelectSingleNode("TThue")?.InnerText ?? "0"));
                            double vVal = (tsStr == "KCT" || tsStr == "KKKNT") ? 0 : double.Parse(tsStr.Replace("%", ""));
                            if ((tsStr == "KCT" && ttien == 0) || (tsStr == "KKKNT" && ttien == 0) || (tsStr == "0%" && ttien == 0) || ttien == 0)
                                continue;
                            if (tbImport.TgTCThue1 == 0)
                            {
                                tbImport.TgTCThue1 = ttien; tbImport.TVat = tthue; tbImport.Vat = vVal;
                            }
                            else
                            {
                                if (tbImport.TgTCThue2 == 0)
                                {
                                    tbImport.TgTCThue2 = ttien; tbImport.TVat2 = tthue; tbImport.Vat2 = vVal.ToString();
                                }
                                else
                                {
                                    if (tbImport.TgTCThue3 == 0)
                                    {
                                        tbImport.TgTCThue3 = ttien; tbImport.TVat3 = tthue; tbImport.Vat3 = vVal.ToString();
                                    }
                                }
                            }
                            //if (i == 0) { tbImport.TgTCThue1 = ttien; tbImport.TVat = tthue; tbImport.Vat = vVal; }
                            //else if (i == 1) { tbImport.TgTCThue2 = ttien; tbImport.TVat2 = tthue; tbImport.Vat2 = vVal.ToString(); }
                            //else if (i == 2) { tbImport.TgTCThue3 = ttien; tbImport.TVat3 = tthue; tbImport.Vat3 = vVal.ToString(); }
                        }
                    }
                    //Xử lý dong thuế thưa
                    //if (tbImport.Vat2 != "0" && !string.IsNullOrEmpty(tbImport.Vat2) && tbImport.Vat == 0)
                    //{
                    //    tbImport.Vat = double.Parse(tbImport.Vat2);
                    //    tbImport.TVat = tbImport.TVat2;
                    //    tbImport.Vat2 = "0";
                    //    tbImport.TVat2 = 0;
                    //}
                    // 10. Loại hóa đơn (01: GTGT, 02: Bán hàng)
                    tbImport.Khmshdon = root.SelectSingleNode("//KHMSHDon")?.InnerText;
                    string thDon = Helpers.NormalizeVietnameseString(root.SelectSingleNode("//THDon")?.InnerText?.ToLower() ?? "");
                    //      tbImport.hdon = thDon.Contains("ban hang") ? "02" : "01";

                    // 11. Chi tiết hàng hóa (HHDVu)
                    var hhdNodes = root.SelectNodes("//HHDVu");
                    cacheMatHangTrongHoaDon = new Dictionary<string, TbImportDetail>(StringComparer.OrdinalIgnoreCase);
                    double finalTotal = 0;
                    foreach (XmlNode node in hhdNodes)
                    {
                        string ten = "";
                        try
                        {
                            string tenGoc = node.SelectSingleNode("THHDVu")?.InnerText;
                            ten = tenGoc;
                            if (ten == "Hộp xích xe máy C110-SXT")
                            {
                                int test = 10;
                            }
                            if (Loaiborow(tenGoc)) continue;

                            int tchat = int.Parse(node.SelectSingleNode("TChat")?.InnerText ?? "0");
                            if (tenGoc.Contains("Chiết khấu") && tchat != 3)
                            {
                                tchat = 3;
                            }
                            bool daGiam = tenGoc.Contains("Đã giảm");
                            if (tchat == 4 && !daGiam) continue;

                            TbImportDetail dt = new TbImportDetail
                            {
                                Tchat = tchat,
                                Ten = tenGoc,
                                TKNo = tbImport.TKNo,
                                TKCo = tbImport.TKCo,
                                // Thêm kiểm tra null cho DVTinh
                                DVT = CapitalizeFirstLetters(Helpers.ConvertUnicodeToVni(node.SelectSingleNode("DVTinh")?.InnerText ?? "")),
                                // Sử dụng SafeParse để không bao giờ bị văng lỗi
                                Soluong = SafeParse(node.SelectSingleNode("SLuong")?.InnerText),
                                Dongia = SafeParse(node.SelectSingleNode("DGia")?.InnerText),
                                TTien = SafeParse(node.SelectSingleNode("ThTien")?.InnerText),
                                SoPSGoc = SafeParse(node.SelectSingleNode("ThTien")?.InnerText),
                                Vat = SafeParse(node.SelectSingleNode("TSuat")?.InnerText?.Replace("%", ""))
                            };
                            if (string.IsNullOrEmpty(dt.DVT))
                            {
                                var findvt = lstvt.FirstOrDefault(m => m.TenVattu.ToLower() == dt.Ten.ToLower());
                                if (findvt != null)
                                {
                                    dt.DVT = Helpers.ConvertUnicodeToVni(findvt.DonVi);
                                }
                            }
                            finalTotal += dt.TTien;
                            if (daGiam)
                            {
                                Match m = Regex.Match(tenGoc, @"\d{1,3}(?:\.\d{3})*(?:,\d+)?");
                                if (m.Success) dt.TTien = double.Parse(m.Value.Replace(".", ""));
                            }

                            // Fuzzy Match & Cache
                            string keyCache = NormalizeVietnameseString(dt.Ten);
                            if (cacheMatHangTrongHoaDon.TryGetValue(keyCache, out var cached))
                            {
                                dt.SoHieu = cached.SoHieu; dt.Percent = cached.Percent;
                            }
                            else
                            {
                                Xulysohieuvattu(dt);
                                cacheMatHangTrongHoaDon[keyCache] = dt;
                            }

                            if (type == 1 && (tchat == 3 || daGiam)) dt.TKCo = "711";
                            dt.Ten = Helpers.ConvertUnicodeToVni(dt.Ten);
                            dt.Percent = Math.Round(dt.Percent);
                            tbImport.tbImportDetails.Add(dt);
                        }
                        catch (Exception ex)
                        {
                            XtraMessageBox.Show(ten);
                        }

                    }
                    //Tiến hành làm tròn và phân bổ thằng cuối cùng
                    foreach (var lt in tbImport.tbImportDetails)
                    {
                        lt.TTien = Math.Round(lt.TTien);
                    }
                    double sodu = Math.Round(finalTotal) - tbImport.tbImportDetails.Sum(m => m.TTien);
                    if (sodu > 0 && sodu <= 1)
                    {
                        tbImport.tbImportDetails.LastOrDefault().TTien += sodu;
                    }

                    //Thưc hiện thiếu tiền so với trc thuế
                    if (tbImport.TgTCThue != finalTotal)
                    {
                        sodu = tbImport.TgTCThue - finalTotal;
                        if (sodu > 0 && sodu <= 1)
                        {
                            tbImport.tbImportDetails.LastOrDefault().TTien += sodu;
                            if (tbImport.TgTCThue1 + sodu == tbImport.TgTCThue)
                            {
                                tbImport.TgTCThue1 += sodu;
                            }
                        }
                    }
                    // 12. Hoàn tất & Lưu
                    if (string.IsNullOrEmpty(tbImport.Noidung) && tbImport.tbImportDetails.Count > 0)
                        tbImport.Noidung = tbImport.tbImportDetails[0].Ten;

                    //Thông tin thay thế
                    string SHDCLQuan = ttChung.SelectSingleNode("//SHDCLQuan")?.InnerText;
                    if (!string.IsNullOrEmpty(SHDCLQuan))
                    {
                        try
                        {
                            DateTime NLHDCLQuan = DateTime.Parse(ttChung.SelectSingleNode("//NLHDCLQuan")?.InnerText);
                            string KHHDCLQuan = ttChung.SelectSingleNode("//KHHDCLQuan")?.InnerText;
                            tbImport.Noidung = Helpers.ConvertUnicodeToVni($"Thay thế cho ký hiệu hóa đơn {KHHDCLQuan}, số hóa đơn {SHDCLQuan}, ngày lập {NLHDCLQuan.ToShortDateString()}");
                        }
                        catch (Exception ex)
                        {

                        }

                    }
                    if (type == 1) { Xuly711(tbImport); }
                    else { /*Xuly5211(tbImport);*/ }

                    //await SaveDataXmlOne(tbImport, type);
                    stt++;
                }
            }
            catch (Exception ex) { XtraMessageBox.Show($"Lỗi xử lý file {pathXml}: {ex.Message}"); }
            finally { sothutu++; }
            //Trước khi import thêm vào cho lookupTbImport
            var keys = NormalizeTbImportKey(tbImport.Mst, tbImport.SHDon, tbImport.NLap, type);
            lookupTbImport.Add(keys);
            if (isAddhd == true)
                return tbImport;
            else
                return null;
        }
        string CapitalizeFirstLetters(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input; // Kiểm tra chuỗi rỗng hoặc null

            return char.ToUpper(input[0]) + input.Substring(1);
        }
        private bool Loaiborow(string name)
        {
            if (name.ToLower().Contains("điều chỉnh"))
                return true;
            return false;
        }
        private string NormalizeNameForSearch(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // 1. Chuẩn hóa dấu tiếng Việt (nếu có hàm Helpers.NormalizeVietnameseString)
            string result = Helpers.NormalizeVietnameseString(input);

            // 2. Chuyển về chữ thường
            result = result.ToLower().Trim();

            // 3. Thay thế các từ đồng nghĩa
            if (_synonymDictionary != null)
            {
                foreach (var synonym in _synonymDictionary)
                {
                    // Thay thế từ khóa
                    if (result.Contains(synonym.Key))
                    {
                        result = result.Replace(synonym.Key, synonym.Value);
                    }
                }
            }

            // 4. Xóa các ký tự đặc biệt thừa
            result = Regex.Replace(result, @"[^\w\s]", " ");

            // 5. Xóa khoảng trắng thừa
            result = Regex.Replace(result, @"\s+", " ").Trim();

            return result;
        }

        // Hàm mở rộng: Thêm từ đồng nghĩa động
        public void AddSynonym(string original, string normalized)
        {
            if (_synonymDictionary == null)
                _synonymDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!_synonymDictionary.ContainsKey(original.ToLower()))
            {
                _synonymDictionary[original.ToLower()] = normalized.ToLower();
            }
        }

        // Hàm mở rộng: Thêm nhiều từ đồng nghĩa cùng lúc
        public void AddSynonyms(Dictionary<string, string> synonyms)
        {
            if (_synonymDictionary == null)
                _synonymDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in synonyms)
            {
                if (!_synonymDictionary.ContainsKey(item.Key.ToLower()))
                {
                    _synonymDictionary[item.Key.ToLower()] = item.Value.ToLower();
                }
            }
        }
        private void BuildIndexes()
        {
            if (_isIndexBuilt) return;

            _keywordIndex = new Dictionary<string, HashSet<string>>();
            _quyCachIndex = new Dictionary<string, HashSet<string>>();

            foreach (var kvp in _optimizedVatTu)
            {
                // Index theo từ khóa (chỉ lấy từ dài >= 3 ký tự)
                var words = kvp.Value.TenChuan.Split(new[] { ' ', '-', ',', ';', '/' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length >= 3);

                foreach (var word in words)
                {
                    if (!_keywordIndex.ContainsKey(word))
                        _keywordIndex[word] = new HashSet<string>();
                    _keywordIndex[word].Add(kvp.Key);
                }

                // Index theo quy cách
                if (!string.IsNullOrEmpty(kvp.Value.QuyCach))
                {
                    if (!_quyCachIndex.ContainsKey(kvp.Value.QuyCach))
                        _quyCachIndex[kvp.Value.QuyCach] = new HashSet<string>();
                    _quyCachIndex[kvp.Value.QuyCach].Add(kvp.Key);
                }
            }

            _isIndexBuilt = true;
        }
        private void AddSynonymIfNeeded(string original, string normalized)
        {
            if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(normalized))
                return;

            if (!_synonymDictionary.ContainsKey(original.ToLower()))
            {
                _synonymDictionary[original.ToLower()] = normalized.ToLower();
            }
        }
        private void InitializeSynonymDictionary()
        {
            _synonymDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            AddSynonymIfNeeded("sài gòn", "saigon");
            AddSynonymIfNeeded("sai gon", "saigon");
            AddSynonymIfNeeded("sài gòn", "saigon");

            // ✅ GIỮ LẠI: Thương hiệu
            AddSynonymIfNeeded("cocacola", "coca cola");
            AddSynonymIfNeeded("cô ca", "coca cola");
            AddSynonymIfNeeded("cô ca cô la", "coca cola");
            AddSynonymIfNeeded("pesi", "pepsi");
            AddSynonymIfNeeded("redbull", "red bull");
        }
        private void Xulysohieuvattu(TbImportDetail tbImportDetail)
        {
            if (tbImportDetail == null || string.IsNullOrEmpty(tbImportDetail.Ten))
                return;

            if (!_isIndexBuilt) BuildIndexes();
            if (_synonymDictionary == null) InitializeSynonymDictionary();

            string originalTen = tbImportDetail.Ten?.Trim() ?? "";
            string normalizedTen = NormalizeNameForSearch(originalTen);
            string quyCach = regex.Match(normalizedTen).Value;
            string donViTinh = tbImportDetail.DVT?.Trim()?.ToLower() ?? "";

            Console.WriteLine($"🔍 Đang tìm: {normalizedTen}");
            Console.WriteLine($"   Quy cách: '{quyCach}'");

            double minPercent = 80;

            // ========== 1. TÌM CHÍNH XÁC ==========
            var exactMatch = _optimizedVatTu
                .FirstOrDefault(kvp =>
                    (NormalizeNameForSearch(kvp.Value.TenChuan) == normalizedTen ||
                     NormalizeNameForSearch(kvp.Value.TenPhuChuan) == normalizedTen) &&
                    (string.IsNullOrEmpty(quyCach) || kvp.Value.QuyCach == quyCach));

            if (!exactMatch.Equals(default(KeyValuePair<string, (string, string, string, string, double, double)>)))
            {
                tbImportDetail.SoHieu = exactMatch.Key;
                tbImportDetail.Percent = 100;
                tbImportDetail.DVT = exactMatch.Value.DonVi;
                Console.WriteLine($"✅ Tìm chính xác: {exactMatch.Value.TenChuan}");
                return;
            }

            // ========== 2. TÁCH TỪ KHÓA ==========
            var words = normalizedTen.Split(new[] { ' ', '-', ',', ';', '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length >= 3)
                .ToList();

            Console.WriteLine($"   Từ khóa: {string.Join(", ", words)}");

            var phrases = new List<string>();
            for (int i = 0; i < words.Count - 1; i++)
            {
                string phrase = words[i] + " " + words[i + 1];
                if (phrase.Length >= 5)
                {
                    phrases.Add(phrase);
                }
            }

            Console.WriteLine($"   Cụm từ: {string.Join(", ", phrases)}");

            // ========== 3. SÀNG LỌC ỨNG VIÊN ==========
            var candidateKeys = new HashSet<string>();

            foreach (var word in words)
            {
                if (_keywordIndex != null && _keywordIndex.ContainsKey(word))
                {
                    foreach (var key in _keywordIndex[word])
                    {
                        candidateKeys.Add(key);
                    }
                }
            }

            foreach (var phrase in phrases)
            {
                if (_keywordIndex != null && _keywordIndex.ContainsKey(phrase))
                {
                    foreach (var key in _keywordIndex[phrase])
                    {
                        candidateKeys.Add(key);
                    }
                }
            }

            if (!string.IsNullOrEmpty(quyCach) && _quyCachIndex != null && _quyCachIndex.ContainsKey(quyCach))
            {
                foreach (var key in _quyCachIndex[quyCach])
                {
                    candidateKeys.Add(key);
                }
            }

            Console.WriteLine($"   Số ứng viên tìm được: {candidateKeys.Count}");

            if (!candidateKeys.Any())
            {
                int count = 0;
                foreach (var kvp in _optimizedVatTu)
                {
                    if (count >= 100) break;
                    count++;
                    candidateKeys.Add(kvp.Key);
                }
                Console.WriteLine($"   Fallback: lấy 100 item đầu tiên");
            }

            // ========== 4. TÍNH ĐIỂM ==========
            var results = new List<(string Key, double Percent, string TenChuan, string QuyCach, string DonVi, int MatchCount)>();

            foreach (var key in candidateKeys)
            {
                if (_optimizedVatTu.TryGetValue(key, out var vatTu))
                {
                    string tenChuanHoa = NormalizeNameForSearch(vatTu.TenChuan);
                    string tenKhongNgoac = ExtractMainName(tenChuanHoa);
                    string tenHoaDonKhongNgoac = ExtractMainName(normalizedTen);

                    // So sánh tên không ngoặc
                    int tokenScoreNoBracket = Fuzz.TokenSetRatio(tenKhongNgoac, tenHoaDonKhongNgoac);
                    int partialScoreNoBracket = Fuzz.PartialRatio(tenKhongNgoac, tenHoaDonKhongNgoac);
                    double percentNoBracket = Math.Max(tokenScoreNoBracket, partialScoreNoBracket);

                    // So sánh tên đầy đủ
                    int tokenScore = Fuzz.TokenSetRatio(tenChuanHoa, normalizedTen);
                    int partialScore = Fuzz.PartialRatio(tenChuanHoa, normalizedTen);
                    double percent = Math.Max(tokenScore, partialScore);

                    // Lấy điểm cao nhất
                    double finalPercent = Math.Max(percent, percentNoBracket);

                    // Đếm số từ khóa khớp
                    int matchCount = 0;
                    foreach (var word in words)
                    {
                        if (tenChuanHoa.Contains(word) || tenKhongNgoac.Contains(word))
                            matchCount++;
                    }

                    finalPercent += matchCount * 5;

                    // So sánh quy cách
                    string quyCachTrongKho = vatTu.QuyCach?.ToLower()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(quyCach) && !string.IsNullOrEmpty(quyCachTrongKho))
                    {
                        if (quyCachTrongKho == quyCach || quyCachTrongKho.Contains(quyCach))
                        {
                            finalPercent += 20;
                        }
                    }

                    // ========== GIỚI HẠN ĐIỂM ==========
                    // Kiểm tra xem có phải đang khớp qua tên không ngoặc không
                    bool isMatchByNoBracket = percentNoBracket > 80 && percentNoBracket > percent;

                    if (isMatchByNoBracket)
                    {
                        // Nếu khớp qua tên không ngoặc (thiếu thông tin trong ngoặc)
                        // Giới hạn tối đa 90%
                        if (finalPercent > 90)
                        {
                            finalPercent = 90;
                        }
                    }
                    else if (percent > 80)
                    {
                        // Nếu khớp qua tên đầy đủ nhưng không chính xác 100%
                        // Giới hạn tối đa 95%
                        if (finalPercent > 95)
                        {
                            finalPercent = 95;
                        }
                    }

                    // Debug
                    if (vatTu.TenChuan.Contains("Tiger") || vatTu.TenChuan.Contains("tiger"))
                    {
                        Console.WriteLine($"   Kiểm tra: {vatTu.TenChuan}");
                        Console.WriteLine($"      Điểm: {finalPercent}%");
                        Console.WriteLine($"      Không ngoặc: {percentNoBracket}%");
                        Console.WriteLine($"      Từ khớp: {matchCount}");
                        Console.WriteLine($"      Quy cách: '{quyCachTrongKho}'");
                        Console.WriteLine($"      Khớp không ngoặc: {isMatchByNoBracket}");
                    }

                    if (finalPercent >= minPercent)
                    {
                        results.Add((key, Math.Min(finalPercent, 100), vatTu.TenChuan, vatTu.QuyCach, vatTu.DonVi, matchCount));
                    }
                }
            }

            // ========== 5. CHỌN KẾT QUẢ ==========
            if (results.Any())
            {
                var sorted = results
                    .OrderByDescending(x => x.MatchCount)
                    .ThenByDescending(x => x.Percent)
                    .ToList();

                var best = sorted.First();
                tbImportDetail.SoHieu = best.Key;
                tbImportDetail.Percent = best.Percent;
                tbImportDetail.DVT = best.DonVi;

                Console.WriteLine($"✅ Tìm thấy: {best.TenChuan}");
                Console.WriteLine($"   Độ tương đồng: {best.Percent}%");
                Console.WriteLine($"   Quy cách: {best.QuyCach}");

                if (sorted.Count > 1)
                {
                    Console.WriteLine($"   📋 Các kết quả khác:");
                    foreach (var item in sorted.Skip(1).Take(3))
                    {
                        Console.WriteLine($"      - {item.TenChuan} (Điểm: {item.Percent}%)");
                    }
                }
            }
            else
            {
                tbImportDetail.SoHieu = GenerateResultString(Helpers.NormalizeVietnameseString(normalizedTen));
                tbImportDetail.Percent = 0;
                Console.WriteLine($"❌ Không tìm thấy vật tư cho: {normalizedTen}");

            }
        }
        private string ExtractMainName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return fullName;

            // Bỏ phần trong ngoặc đơn, ngoặc vuông, ngoặc nhọn
            string result = Regex.Replace(fullName, @"\(.*?\)", "").Trim();
            result = Regex.Replace(result, @"\[.*?\]", "").Trim();
            result = Regex.Replace(result, @"\{.*?\}", "").Trim();

            // Xóa khoảng trắng thừa
            result = Regex.Replace(result, @"\s+", " ").Trim();

            return result;
        }

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
        public int sothutu = 1;
        public void Xuly711(TbImport fileImport)
        {
            var getlist711 = fileImport.tbImportDetails.Where(m => m.TKCo == "711").ToList();

            if (getlist711.Count > 0 && tbLicense.Rows[0].Field<string>("col711") == "1")
            {
                List<TbImportDetail> listdathuchien = new List<TbImportDetail>();

                //Trường hợp chỉ có 1 dòng 711
                //Lấy ra dòng 711;
                if (getlist711.Count == 1)
                {
                    var get711 = getlist711.Where(m => m.TKCo == "711").FirstOrDefault();
                    if (get711 != null)
                    {
                        string pattern = @"\d+"; // Tìm một hoặc nhiều chữ số

                        Match match = Regex.Match(get711.Ten, pattern);
                        //Kiển tra xem có phải chiết khấu có % không

                        //Tính lại giá tiền cho các dòng
                        var remainlist = fileImport.tbImportDetails.Where(m => m != get711 && !string.IsNullOrEmpty(m.DVT) && m.Dongia != 0).ToList();
                        int index = 0;
                        double sumtotal = 0;

                        double totalth = remainlist.Sum(m => m.TTien);
                        double total711 = get711.TTien;
                        foreach (var it2 in remainlist)
                        {
                            if (index < remainlist.Count - 1)
                            {
                                it2.TTien = Math.Round(it2.TTien - (total711 * it2.TTien / totalth));
                            }
                            else
                            {
                                it2.TTien = Math.Round(it2.TTien - (total711 * it2.TTien / totalth));
                                double reTotal = Math.Round(fileImport.TgTCThue - remainlist.Sum(m => m.TTien));
                                if (reTotal > 0)
                                    it2.TTien += reTotal;
                                if (reTotal == -1)
                                    it2.TTien -= 1;
                            }
                            //Cập nhật vô database

                            index += 1;
                        }
                        //Xoá đi dòng 711
                        fileImport.tbImportDetails.Remove(get711);
                    }

                }
                //Trường hợp có nhiều 711
                else
                {
                    var get771s = getlist711.Where(m => m.TKCo == "711").ToList();
                    int finddata = 0;
                    foreach (var i7 in get771s)
                    {
                        //string pattern = @"(\d+[,.]?\d*)%|chiết khấu\s*(\d+)";


                        //var match = Regex.Match(i7.Ten, pattern);
                        //double percent = 0;
                        //if (match.Success)
                        //{
                        //    string soChietKhau = match.Groups[1].Value;
                        //    if (string.IsNullOrEmpty(soChietKhau))
                        //    {
                        //        soChietKhau = match.Groups[2].Value;
                        //    }
                        //    soChietKhau = soChietKhau.Replace(",", ".");
                        //    percent = double.Parse(soChietKhau);
                        //    foreach (var ftt in fileImport.tbImportDetails.Where(m => m.TKCo != "711"))
                        //    {
                        //        var sodu = Math.Round(ftt.TTien * percent / 100) - i7.TTien;
                        //        if (sodu >= 0 && sodu <= 1)
                        //        {
                        //            finddata += 1;
                        //            ftt.TTien = ftt.TTien - i7.TTien;
                        //            listdathuchien.Add(i7);
                        //        }
                        //    }

                        //}
                        fileImport.tbImportDetails.Remove(i7);
                    }
                    //Trường hợp ko tìm dc thì buộc phải tìm % để phân bổ



                    //if (finddata != get771s.Count)
                    if (1 < 2)
                    {

                        //Tính lại giá tiền cho các dòng
                        var remainlist = fileImport.tbImportDetails.Where(m => m.TKCo != "711" && !string.IsNullOrEmpty(m.DVT) && m.Dongia != 0).ToList();
                        double totalth = remainlist.Sum(m => m.TTien);
                        double total711 = get771s.Sum(m => m.TTien);
                        if (total711 < 0)
                        {
                            total711 = -total711;
                        }
                        int index = 0;
                        foreach (var it2 in remainlist)
                        {
                            if (remainlist.Count > 1)
                            {
                                if (index < remainlist.Count - 1)
                                {
                                    it2.TTien = Math.Round(it2.TTien - (total711 * it2.TTien / totalth));
                                }
                                else
                                {
                                    it2.TTien = Math.Round(it2.TTien - (total711 * it2.TTien / totalth));
                                    double reTotal = Math.Round(fileImport.TgTCThue - remainlist.Sum(m => m.TTien));
                                    it2.TTien += reTotal;
                                }
                            }
                            else
                            {
                                it2.TTien = Math.Round(it2.TTien - (total711 * it2.TTien / totalth));
                            }
                            index += 1;
                        }

                    }
                }

            }
            else
            {
                if (getlist711.Count > 1)
                {
                    var getfirst = getlist711.FirstOrDefault();
                    var lstremain = getlist711.Skip(1).ToList();
                    //Cập nhật lại tổng tiền cho first
                    getfirst.TTien = getfirst.TTien + lstremain.Sum(m => m.TTien);
                    if (getfirst.TTien < 0)
                    {
                        getfirst.TTien = -getfirst.TTien;
                    }
                    //Xoá các dòng thừa
                    foreach (var it in lstremain)
                    {
                        fileImport.tbImportDetails.Remove(it);
                    }
                }
            }
        }
        List<string> accountCodes = new List<string>(new[] { "5113", "5112", "5111" });
        public void Xuly5211(TbImport fileImport)
        {
            if (tbLicense.Rows[0].Field<string>("col711ra") == "1")
            {
                //Trường hợp có 5113
                var kiemtra5113 = fileImport.tbImportDetails.Where(m => m.TKCo.Contains("5113")).FirstOrDefault();
                if (kiemtra5113 != null)
                {
                    var find5211 = fileImport.tbImportDetails.Where(m => m.Tchat == 3).ToList();
                    if (find5211 != null && find5211.Count > 0)
                    {
                        //Cập nhật giảm trừ cho tk 5113 đầu tiên
                        kiemtra5113.TTien -= find5211.Sum(m => m.TTien);
                        //Cập nhật trong database 
                        //Thực hiện xoá dòng 5211
                        foreach (var r52 in find5211)
                        {
                            fileImport.tbImportDetails.Remove(r52);
                        }
                        return;
                    }
                }
            }
            foreach (var code in accountCodes)
            {
                if (fileImport.tbImportDetails.Any(m => m.TKCo.Contains(code)))
                {
                    var find5211 = fileImport.tbImportDetails.Where(m => m.Tchat == 3).ToList();
                    //Xoá và gộp 5211
                    var first5211 = find5211.FirstOrDefault();
                    var lst5211remain = find5211.Where(m => m != first5211).ToList();
                    if (first5211 != null)
                    {
                        first5211.TTien += lst5211remain.Sum(m => m.TTien);
                        first5211.TKNo = fileImport.tbImportDetails
                            .FirstOrDefault(m => m.TKCo.Contains(code))?.TKCo;
                        first5211.TKCo = "";
                    }
                    //Xoá remain
                    foreach (var it in lst5211remain)
                    {
                        fileImport.tbImportDetails.Remove(it);
                    }
                    return;
                }
            }
        }
        private void GetMST()
        {
            string query = "SELECT * FROM License";

            // Tạo mảng tham số với giá trị cho câu lệnh SQL

            var kq = ExecuteQuery(query, null);
            if (kq.Rows.Count > 0)
            {
                MSTCongTY = kq.Rows[0]["MaSoThue"].ToString();
                CCCD = kq.Rows[0]["CCCD"].ToString();
            }
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
                    richTextBox1.Text = $"File chưa đủ 30 phút để tải lại.";
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

                        richTextBox1.Text = $"Lần thử {retry}/{maxRetry} - Đang tải {filename}";

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

                        richTextBox1.Text = $"✅ Tải thành công: {filename}";

                        return true;
                    }
                }
                catch (TaskCanceledException)
                {
                    richTextBox1.Text = $"⏰ Timeout ({retry}/{maxRetry})";
                }
                catch (Exception ex)
                {
                    richTextBox1.Text = $"❌ {ex.Message}";
                }

                if (retry < maxRetry)
                {
                    richTextBox1.Text = $"⏳ Chờ 3 giây rồi thử lại...";
                    await Task.Delay(3000);
                }
            }

            richTextBox1.Text = $"❌ Không thể tải file sau {maxRetry} lần thử.";
            return false;
        }
        public void Xulyexelra(string token, int _type)
        {
            // Tối ưu: Tính toán datetime một lần
            DateTime dtFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime dtTo = dtFrom.AddMonths(1).AddDays(-1);

            // Tối ưu: Format string một lần
            string formattedDate1 = dtFrom.ToString("dd/MM/yyyyTHH:mm:ss");
            string formattedDate2 = dtTo.ToString("dd/MM/yyyyTHH:mm:ss");

            // Tối ưu: Dùng switch case thay vì nhiều if
            string url, filename;
            switch (_type)
            {
                case 1:
                    url = @"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-excel?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge=" + formattedDate1 + ";tdlap=le=" + formattedDate2;
                    filename = $"{mstcongty}_Hoadondientu.xlsx";
                    break;
                case 2:
                    url = @"https://hoadondientu.gdt.gov.vn/api/sco-query/invoices/export-excel?sort=tdlap:desc,khmshdon:asc,shdon:desc&search=tdlap=ge=" + formattedDate1 + ";tdlap=le=" + formattedDate2;
                    filename = $"{mstcongty}_HDDienTuMayTinhTien.xlsx";
                    break;
                default:
                    return;
            }

            string currentYear = $"HD{DateTime.Now.Year}";
            string directoryPath = Path.Combine(savedPath, currentYear, "HDRa", DateTime.Now.Month.ToString());
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
                    Console.WriteLine($"Đã xóa file cũ: {filePath}");
                }
                else
                {
                    Console.WriteLine($"File chưa đủ 30 phút để xóa. Thời gian còn lại: {30 - timeDifference.TotalMinutes:F1} phút");
                    return;
                }
            }

            // ========================================
            // THÊM TIMEOUT VÀ RETRY 3 LẦN
            // ========================================
            int maxRetry = 3;
            int retryCount = 0;
            bool isDownloaded = false;

            while (retryCount < maxRetry && !isDownloaded)
            {
                retryCount++;
                Console.WriteLine($"Lần thử {retryCount}/{maxRetry} - Đang tải file: {filename}");
                richTextBox1.Text = $"Lần thử {retryCount}/{maxRetry} - Đang tải file: {filename}";
                Application.DoEvents();
                try
                {
                    using (var client = new HttpClient())
                    {
                        // Set timeout 5 phút (300 giây)
                        client.Timeout = TimeSpan.FromSeconds(10);


                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                        // Sử dụng Task để có thể kiểm soát timeout
                        var task = client.GetAsync(url);

                        // Chờ response với timeout
                        if (task.Wait(TimeSpan.FromSeconds(10)))
                        {
                            HttpResponseMessage response = task.Result;

                            if (response.IsSuccessStatusCode)
                            {
                                var fileBytes = response.Content.ReadAsByteArrayAsync().Result;
                                File.WriteAllBytes(filePath, fileBytes);

                                Console.WriteLine($"✅ Tải file thành công: {filename}");
                                richTextBox1.Text = $"✅ Tải file thành công: {filename}";
                                Application.DoEvents();
                                isDownloaded = true;
                            }
                            else
                            {
                                Console.WriteLine($"❌ Lỗi HTTP: {response.StatusCode} - {response.ReasonPhrase}");
                                richTextBox1.Text = $"❌ Lỗi HTTP: {response.StatusCode} - {response.ReasonPhrase}";
                                Application.DoEvents();
                            }
                        }
                        else
                        {
                            // Timeout
                            Console.WriteLine($"⏰ Timeout! Lần thử {retryCount}/{maxRetry}");
                            richTextBox1.Text = $"✅ Tải file thành công:  {filename}";
                            Application.DoEvents();
                            // Hủy request
                            client.CancelPendingRequests();
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    Console.WriteLine($"⏰ Request bị hủy do timeout: {ex.Message}");
                    richTextBox1.Text = $"⏰ Request bị hủy do timeout: {ex.Message}";
                    Application.DoEvents();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Lỗi: {ex.Message}");
                    richTextBox1.Text = $"❌ Lỗi: {ex.Message}";
                    Application.DoEvents();
                }

                // Nếu chưa tải thành công và còn lượt thử
                if (!isDownloaded && retryCount < maxRetry)
                {
                    // Chờ 5 giây trước khi thử lại (tăng dần)
                    int waitSeconds = 3;
                    Console.WriteLine($"⏳ Chờ {waitSeconds} giây trước khi thử lại...");
                    richTextBox1.Text = $"⏳ Chờ {waitSeconds} giây trước khi thử lại...";
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(waitSeconds * 1000);
                }
            }

            if (!isDownloaded)
            {
                Console.WriteLine($"❌ Không thể tải file sau {maxRetry} lần thử: {filename}");
                richTextBox1.Text = $"❌ Không thể tải file sau {maxRetry} lần thử: {filename}";
                Application.DoEvents();
            }
        }
        // Đặt ở đầu file, trong namespace, ngoài class frmAutoTai
        public class InvoiceInfo
        {
            public string Mst { get; set; }          // Mã số thuế
            public string SHHD { get; set; }         // Ký hiệu hóa đơn
            public string Sohd { get; set; }         // Số hóa đơn
            public DateTime NLap { get; set; }       // Ngày lập
            public string Khhd { get; set; }         // Khung hóa đơn
            public string DirectoryPath { get; set; } // Đường dẫn lưu file
        }
        /// <summary>
        /// Cập nhật trạng thái lên UI an toàn từ mọi thread
        /// </summary>
        private void UpdateStatus(string message)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        richTextBox1.Text = message;
                        currentState.Text = message;
                        Application.DoEvents();
                    }));
                }
                else
                {
                    richTextBox1.Text = message;
                    currentState.Text = message;
                    Application.DoEvents();
                }
            }
            catch (ObjectDisposedException)
            {
                // Form đã đóng, bỏ qua
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi UpdateStatus: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách hóa đơn cần tải từ file Excel
        /// </summary>
        private List<InvoiceInfo> GetInvoicesToDownload(string mstcongty, string savedPath)
        {
            var result = new List<InvoiceInfo>();

            try
            {
                // Tạo đường dẫn thư mục
                string currentYear = $"HD{DateTime.Now.Year}";
                string directoryPath = Path.Combine(savedPath, DateTime.Now.Month.ToString());

                if (!Directory.Exists(directoryPath))
                {
                    UpdateStatus($"❌ Thư mục không tồn tại: {directoryPath}");
                    return result;
                }

                // Lấy danh sách file Excel
                var excelFiles = Directory.EnumerateFiles(directoryPath, "*.xlsx", SearchOption.AllDirectories)
                                          .Where(m => m.Contains(mstcongty)).ToList();

                if (excelFiles.Count == 0)
                {
                    UpdateStatus("📭 Không tìm thấy file Excel nào");
                    return result;
                }

                foreach (var excelFile in excelFiles)
                {
                    using (var workbook = new XLWorkbook(excelFile))
                    {
                        var worksheet = workbook.Worksheet(1);
                        foreach (var row in worksheet.RowsUsed().Skip(3))
                        {
                            try
                            {
                                string khhd = row.Cell("B").Value.ToString();
                                string getSHHD = row.Cell("C").Value.ToString();
                                string getSohd = RemoveLeadingZeros(row.Cell("D").Value.ToString());
                                string GetNLap = row.Cell("E").Value.ToString();
                                string mstnb = row.Cell("F").Value.ToString();

                                if (!DateTime.TryParse(GetNLap, out DateTime getdate))
                                    continue;

                                // Kiểm tra trùng trong cache
                                if (lookupHoaDonCT.Contains((mstnb, getSohd, getSHHD, getdate.Date, 1)))
                                    continue;
                                if (lookupTbImport.Contains((mstnb, getSohd, getdate.Date, 1)))
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
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Lỗi xử lý dòng: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Lỗi GetInvoicesToDownload: {ex.Message}");
            }

            return result;
        }
        /// <summary>
        /// Tải 1 hóa đơn đơn lẻ
        /// </summary>
        private void DownloadSingleInvoice(InvoiceInfo invoice)
        {
            if (invoice == null) return;

            try
            {
                // Tạo URL
                string url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-xml?nbmst={invoice.Mst}&khhdon={invoice.SHHD}&shdon={invoice.Sohd}&khmshdon={invoice.Khhd}";

                // Tạo đường dẫn file
                string filename = $"{invoice.NLap:yyyyMMdd}_{invoice.Mst}_{invoice.Sohd}_{invoice.SHHD}.zip";
                string path = Path.Combine(invoice.DirectoryPath, filename);
                string pathxml = Path.Combine(invoice.DirectoryPath, filename.Replace(".zip", ".xml"));

                // Kiểm tra đã tồn tại
                if (File.Exists(path) || File.Exists(pathxml))
                {
                    return;
                }

                // Tải file
                bool isDownloaded = DownloadFileWithRetry(url, path, tokken);

                if (isDownloaded)
                {
                    // Giải nén
                    ExtractZipXML(path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tải HĐ {invoice.Sohd}: {ex.Message}");
            }
        }
        /// <summary>
        /// Tải hóa đơn đầu vào từ file Excel (song song)
        /// </summary>
        private void DocfileExcelVao_Background(string mstcongty, string savedPath, string originpath)
        {
            try
            {
                // Load dữ liệu cache
                LoadHoadonCT();
                Loadtbimport();

                // Lấy danh sách hóa đơn cần tải
                var invoices = GetInvoicesToDownload(mstcongty, savedPath);

                if (invoices.Count == 0)
                {
                    UpdateStatus("📭 Không có hóa đơn mới để tải");
                    return;
                }

                UpdateStatus($"📥 Bắt đầu tải {invoices.Count} hóa đơn (song song 5 luồng)...");

                int completed = 0;
                object lockObj = new object();

                // Tải song song tối đa 5 hóa đơn cùng lúc
                Parallel.ForEach(invoices, new ParallelOptions { MaxDegreeOfParallelism = 5 }, invoice =>
                {
                    try
                    {
                        DownloadSingleInvoice(invoice);

                        lock (lockObj)
                        {
                            completed++;
                            if (completed % 10 == 0 || completed == invoices.Count)
                            {
                                UpdateStatus($"⏳ Đã tải {completed}/{invoices.Count} hóa đơn");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus($"❌ Lỗi HĐ {invoice.Sohd}: {ex.Message}");
                    }
                });

                UpdateStatus($"✅ Hoàn thành! Đã tải {completed}/{invoices.Count} hóa đơn");
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Xử lý hóa đơn đầu vào sau khi tải xong
        /// </summary>
        /// <summary>
        /// Tải hóa đơn đầu vào (song song) và xử lý sau khi tải
        /// </summary>
        private async Task TaiHoaDonVao(string mstcongty, string savedPath, string originpath)

        {
            try
            {
                // 1. Load dữ liệu cache để kiểm tra trùng
                LoadHoadonCT();
                Loadtbimport();

                // 2. Lấy danh sách hóa đơn cần tải từ Excel
                var invoices = GetInvoicesToDownload(mstcongty, savedPath);

                if (invoices.Count == 0)
                {
                    UpdateStatus("📭 Không có hóa đơn đầu vào mới để tải");
                    return;
                }

                UpdateStatus($"📥 Bắt đầu tải {invoices.Count} hóa đơn đầu vào (song song 5 luồng)...");

                // 3. Tải song song
                int completed = 0;
                int downloaded = 0;
                object lockObj = new object();

                Parallel.ForEach(invoices, new ParallelOptions { MaxDegreeOfParallelism = 5 }, invoice =>
                {
                    try
                    {
                        // Tải 1 hóa đơn
                        DownloadSingleInvoice(invoice);

                        lock (lockObj)
                        {
                            completed++;
                            downloaded++;
                            if (completed % 10 == 0 || completed == invoices.Count)
                            {
                                UpdateStatus($"⏳ Đã tải {completed}/{invoices.Count} hóa đơn vào");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus($"❌ Lỗi HĐ {invoice.Sohd}: {ex.Message}");
                    }
                });

                UpdateStatus($"✅ Đã tải xong {downloaded} hóa đơn đầu vào!");

                // 4. ✅ XỬ LÝ SAU KHI TẢI XONG
                if (downloaded > 0)
                {
                    UpdateStatus($"📊 Bắt đầu xử lý {downloaded} hóa đơn đầu vào...");
                    // XuLyHoaDonVaoSauKhiTai(invoices);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Lỗi TaiHoaDonVao: {ex.Message}");
            }
        }
        /// <summary>
        /// Tải hóa đơn đầu ra (song song) và xử lý sau khi tải
        /// </summary>
        private async Task TaiHoaDonRa(string mstcongty, string savedPath, string originpath)

        {
            try
            {
                // 1. Load dữ liệu cache
                LoadHoadonCT();
                Loadtbimport();

                // 2. Lấy danh sách hóa đơn cần tải từ Excel
                var invoices = GetInvoicesToDownloadRa(mstcongty, savedPath);

                if (invoices.Count == 0)
                {
                    UpdateStatus("📭 Không có hóa đơn đầu ra mới để tải");
                    return;
                }

                UpdateStatus($"📥 Bắt đầu tải {invoices.Count} hóa đơn đầu ra (song song 5 luồng)...");

                // 3. Tải song song
                int completed = 0;
                int downloaded = 0;
                object lockObj = new object();

                Parallel.ForEach(invoices, new ParallelOptions { MaxDegreeOfParallelism = 5 }, invoice =>
                {
                    try
                    {
                        // Tải 1 hóa đơn ra
                        DownloadSingleInvoiceRa(invoice);

                        lock (lockObj)
                        {
                            completed++;
                            downloaded++;
                            if (completed % 10 == 0 || completed == invoices.Count)
                            {
                                UpdateStatus($"⏳ Đã tải {completed}/{invoices.Count} hóa đơn ra");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus($"❌ Lỗi HĐ {invoice.Sohd}: {ex.Message}");
                    }
                });

                UpdateStatus($"✅ Đã tải xong {downloaded} hóa đơn đầu ra!");

                // 4. ✅ XỬ LÝ SAU KHI TẢI XONG
                if (downloaded > 0)
                {
                    UpdateStatus($"📊 Bắt đầu xử lý {downloaded} hóa đơn đầu ra...");
                    //XuLyHoaDonRaSauKhiTai(invoices);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Lỗi TaiHoaDonRa: {ex.Message}");
            }
        }
        /// <summary>
        /// Lấy danh sách hóa đơn đầu ra cần tải từ file Excel
        /// </summary>
        private List<InvoiceInfo> GetInvoicesToDownloadRa(string mstcongty, string savedPath)
        {
            var result = new List<InvoiceInfo>();

            try
            {
                // Đường dẫn thư mục HDRa
                string currentYear = $"HD{DateTime.Now.Year}";
                string directoryPath = Path.Combine(savedPath, currentYear, "HDRa", DateTime.Now.Month.ToString());

                if (!Directory.Exists(directoryPath))
                {
                    UpdateStatus($"❌ Thư mục không tồn tại: {directoryPath}");
                    return result;
                }

                // Lấy danh sách file Excel
                var excelFiles = Directory.EnumerateFiles(directoryPath, "*.xlsx", SearchOption.AllDirectories)
                                          .Where(m => m.Contains(mstcongty)).ToList();

                if (excelFiles.Count == 0)
                {
                    UpdateStatus("📭 Không tìm thấy file Excel đầu ra nào");
                    return result;
                }

                foreach (var excelFile in excelFiles)
                {
                    using (var workbook = new XLWorkbook(excelFile))
                    {
                        var worksheet = workbook.Worksheet(1);
                        foreach (var row in worksheet.RowsUsed().Skip(3))
                        {
                            try
                            {
                                string khhd = row.Cell("B").Value.ToString();
                                string getSHHD = row.Cell("C").Value.ToString();
                                string getSohd = RemoveLeadingZeros(row.Cell("D").Value.ToString());
                                string GetNLap = row.Cell("E").Value.ToString();
                                string mstnb = row.Cell("F").Value.ToString();

                                if (!DateTime.TryParse(GetNLap, out DateTime getdate))
                                    continue;

                                // Kiểm tra trùng cho đầu ra (type = 2)
                                if (lookupHoaDonCT.Contains((mstnb, getSohd, getSHHD, getdate.Date, 2)))
                                    continue;
                                if (lookupTbImport.Contains((mstnb, getSohd, getdate.Date, 2)))
                                    continue;

                                // Kiểm tra file đã tồn tại
                                string filename = $"{getdate:yyyyMMdd}_{mstnb}_{getSohd}_{getSHHD}.xml";
                                if (File.Exists(Path.Combine(directoryPath, filename)))
                                    continue;

                                result.Add(new InvoiceInfo
                                {
                                    Mst = mstnb,
                                    SHHD = getSHHD,
                                    Sohd = getSohd,
                                    NLap = getdate,
                                    Khhd = khhd,
                                    DirectoryPath = directoryPath
                                });
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Lỗi xử lý dòng: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Lỗi GetInvoicesToDownloadRa: {ex.Message}");
            }

            return result;
        }
        /// <summary>
        /// Tải 1 hóa đơn đầu ra
        /// </summary>
        private void DownloadSingleInvoiceRa(InvoiceInfo invoice)
        {
            if (invoice == null) return;

            try
            {
                // Tạo URL cho đầu ra (khác với đầu vào)
                string url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-xml?nbmst={invoice.Mst}&khhdon={invoice.SHHD}&shdon={invoice.Sohd}&khmshdon={invoice.Khhd}";

                // Tạo đường dẫn file
                string filename = $"{invoice.NLap:yyyyMMdd}_{invoice.Mst}_{invoice.Sohd}_{invoice.SHHD}.zip";
                string path = Path.Combine(invoice.DirectoryPath, filename);
                string pathxml = Path.Combine(invoice.DirectoryPath, filename.Replace(".zip", ".xml"));

                if (File.Exists(path) || File.Exists(pathxml))
                {
                    return;
                }

                bool isDownloaded = DownloadFileWithRetry(url, path, tokken);

                if (isDownloaded)
                {
                    ExtractZipXML(path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tải HĐ ra {invoice.Sohd}: {ex.Message}");
            }
        }
        /// <summary>
        /// Xử lý hóa đơn đầu vào sau khi tải xong
        /// </summary>
        //private void XuLyHoaDonVaoSauKhiTai(List<InvoiceInfo> invoices)
        //{
        //    try
        //    {
        //        // Lọc chỉ lấy những hóa đơn đã tải thành công (có file XML)
        //        var validInvoices = invoices.Where(inv =>
        //        {
        //            string filename = $"{inv.NLap:yyyyMMdd}_{inv.Mst}_{inv.Sohd}_{inv.SHHD}.xml";
        //            string xmlPath = Path.Combine(inv.DirectoryPath, filename);
        //            return File.Exists(xmlPath);
        //        }).ToList();

        //        if (validInvoices.Count == 0)
        //        {
        //            UpdateStatus("📭 Không có hóa đơn nào để xử lý");
        //            return;
        //        }

        //        UpdateStatus($"📊 Bắt đầu xử lý {validInvoices.Count} hóa đơn đầu vào...");

        //        int processed = 0;
        //        int errorCount = 0;
        //        object lockObj = new object();

        //        // Xử lý song song
        //        Parallel.ForEach(validInvoices, new ParallelOptions { MaxDegreeOfParallelism = 5 }, invoice =>
        //        {
        //            try
        //            {
        //                // Đường dẫn file XML
        //                string filename = $"{invoice.NLap:yyyyMMdd}_{invoice.Mst}_{invoice.Sohd}_{invoice.SHHD}.xml";
        //                string xmlPath = Path.Combine(invoice.DirectoryPath, filename);

        //                if (File.Exists(xmlPath))
        //                {
        //                    // Đọc và xử lý file XML (type = 1 là đầu vào)
        //                    TbImport tbImport = DocfileXmlOne(xmlPath, 0, 1);

        //                    if (tbImport != null)
        //                    {
        //                        // Kiểm tra trùng trước khi lưu
        //                        if (!Kiemtrahoadon(tbImport.SHDon, tbImport.NLap, tbImport.Mst, 1))
        //                        {
        //                            // Lưu vào database
        //                            SaveTbImportToDatabase(tbImport, 1);

        //                            // Thêm vào lookup để tránh trùng
        //                            lookupTbImport.Add((tbImport.Mst, tbImport.SHDon, tbImport.NLap, 1));

        //                            lock (lockObj)
        //                            {
        //                                processed++;
        //                                if (processed % 10 == 0 || processed == validInvoices.Count)
        //                                {
        //                                    UpdateStatus($"⏳ Đã xử lý {processed}/{validInvoices.Count} hóa đơn vào");
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            UpdateStatus($"⏭️ HĐ {tbImport.SHDon} đã tồn tại, bỏ qua");
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                lock (lockObj)
        //                {
        //                    errorCount++;
        //                }
        //                UpdateStatus($"❌ Lỗi xử lý HĐ {invoice.Sohd}: {ex.Message}");
        //            }
        //        });

        //        UpdateStatus($"✅ Hoàn thành xử lý {processed} hóa đơn đầu vào! (Lỗi: {errorCount})");
        //    }
        //    catch (Exception ex)
        //    {
        //        UpdateStatus($"❌ Lỗi XuLyHoaDonVaoSauKhiTai: {ex.Message}");
        //    }
        //} 
        /// <summary>
        /// Tải hàng loạt hóa đơn XML từ danh sách (song song)
        /// </summary>
        /// <summary>
        /// Tải hàng loạt hóa đơn XML từ danh sách (song song, có retry)
        /// </summary>
        private async Task TaiHangLoatHoaDon(List<InvoiceInfo> invoices, string typeName)
        {
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
                            bool success = DownloadSingleInvoiceSync(invoice);

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
        /// <summary>
        /// Tải 1 hóa đơn XML, trả về true nếu thành công
        /// </summary>
        /// <summary>
        /// Tải 1 hóa đơn XML, có retry 3 lần nếu thất bại
        /// </summary>
        /// 

        /// <summary>
        /// Tải 1 hóa đơn XML, có retry 3 lần nếu thất bại
        /// </summary>
        private bool DownloadSingleInvoiceSync(InvoiceInfo invoice)
        {
            if (invoice == null) return false;

            int maxRetry = 3;
            int retryCount = 0;

            while (retryCount < maxRetry)
            {
                retryCount++;

                try
                {
                    string url = $"https://hoadondientu.gdt.gov.vn/api/query/invoices/export-xml?nbmst={invoice.Mst}&khhdon={invoice.SHHD}&shdon={invoice.Sohd}&khmshdon={invoice.Khhd}";

                    string filename = $"{invoice.NLap:yyyyMMdd}_{invoice.Mst}_{invoice.Sohd}_{invoice.SHHD}.zip";
                    string path = Path.Combine(invoice.DirectoryPath, filename);
                    string pathxml = Path.Combine(invoice.DirectoryPath, filename.Replace(".zip", ".xml"));

                    // Kiểm tra đã tồn tại
                    if (File.Exists(path) || File.Exists(pathxml))
                    {
                        return true;
                    }

                    bool isDownloaded = DownloadFileWithRetry(url, path, tokken, 2);

                    if (isDownloaded)
                    {
                        ExtractZipXML(path);
                        return true;
                    }
                    else
                    {
                        if (retryCount < maxRetry)
                        {
                            UpdateStatus($"🔄 Đang thử lại HĐ {invoice.Sohd} lần {retryCount + 1}/{maxRetry}...");
                            Thread.Sleep(2000 * retryCount);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (retryCount < maxRetry)
                    {
                        UpdateStatus($"⚠️ Lỗi HĐ {invoice.Sohd} lần {retryCount}/{maxRetry}: {ex.Message}");
                        Thread.Sleep(2000 * retryCount);
                    }
                    else
                    {
                        UpdateStatus($"❌ HĐ {invoice.Sohd} thất bại sau {maxRetry} lần thử: {ex.Message}");
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Lấy danh sách hóa đơn cần tải từ file Excel
        /// </summary>
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
                        UpdateStatus($"❌ Thư mục không tồn tại: {directoryPath}");
                        return result;
                    }

                    // Lấy tất cả file Excel
                    var excelFiles = Directory.EnumerateFiles(directoryPath, "*.xlsx", SearchOption.AllDirectories)
                                              .Where(m => m.Contains(mstcongty)).ToList();

                    if (excelFiles.Count == 0)
                    {
                        UpdateStatus($"📭 Không tìm thấy file Excel {folderName}");
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
                                        UpdateStatus($"📋 Đã đọc {result.Count} hóa đơn cần tải...");
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
                    UpdateStatus($"❌ Lỗi GetListHoaDonCanTai: {ex.Message}");
                }

                return result;
            });
        }
    }

}