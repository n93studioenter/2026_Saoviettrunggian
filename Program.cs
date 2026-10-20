using Microsoft.Win32;
using System;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace SaovietTax
{
    static class Program
    {
        // ★ ĐƯỜNG DẪN TUYỆT ĐỐI TỚI SharedDLLs ★
        //private static readonly string SharedDllsPath = @"C:\apps\SharedDLLs";
        private static readonly string SharedDllsPath = @"\\192.168.1.90\Ke toan 2025 New\2 Copi vao dung 4\Tools\Debug";
        static Mutex mutex;

        [STAThread]
        static void Main(string[] args)
        {
            // ★ ĐĂNG KÝ RESOLVER TRƯỚC KHI LÀM BẤT CỨ GÌ ★
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            bool isAutoStart = args.Length > 0 && args[0] == " - autostart";
            try
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (isAutoStart)
            {
                SaveConfig("Mode", "2");
                Application.Run(new frmMain());
            }
            else
            {
                SaveConfig("Mode", "1");
                string appPath = Assembly.GetExecutingAssembly().Location;
                string directoryPath = Path.GetDirectoryName(appPath);
                string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));
                string filePaths = Path.Combine(rootDirectory, "hoadon", "status.txt");
                string content = "1";

                // ★ GỌI DEVEXPRESS QUA REFLECTION để tránh using DevExpress ở đầu file ★
                try
                {
                    var settingsType = Type.GetType(
                        "DevExpress.XtraEditors.WindowsFormsSettings, DevExpress.XtraEditors");
                    if (settingsType != null)
                    {
                        var method = settingsType.GetMethod("SetDPIAware",
                            BindingFlags.Public | BindingFlags.Static);
                        method?.Invoke(null, null);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khởi tạo DevExpress: " + ex.Message);
                }

                try
                {
                    content = File.ReadAllText(filePaths);
                }
                catch (Exception ex)
                {
                }

                // ★ CÁC FORM DEVEXPRESS ĐƯỢC TẠO QUA REFLECTION ★
                // (vì nếu new trực tiếp, C# compiler sẽ cần DevExpress reference → OK,
                //  vì đã có resolver, nhưng vẫn an toàn hơn nếu dùng reflection)
                switch (content.Trim())
                {
                    case "1": Application.Run(CreateForm("SaovietTax.frmMain")); break;
                    case "2": Application.Run(CreateForm("SaovietTax.KTHT")); break;
                    case "3": Application.Run(CreateForm("SaovietTax.frmTaihoadonvb")); break;
                    case "4": Application.Run(CreateForm("SaovietTax.Form3")); break;
                    case "5": Application.Run(CreateForm("SaovietTax.APIInvoice")); break;
                    case "6": Application.Run(CreateForm("SaovietTax.BkavInvoice")); break;
                    case "7": Application.Run(CreateForm("SaovietTax.VNPTInvoice")); break;
                    case "8": Application.Run(CreateForm("SaovietTax.TendoInvoice")); break;
                    case "9": Application.Run(CreateForm("SaovietTax.frmQrcode")); break;
                    case "10": Application.Run(CreateForm("SaovietTax.vb6Xemhoadon")); break;
                    case "11": Application.Run(CreateForm("SaovietTax.Vanguard")); break;
                    case "12": Application.Run(CreateForm("SaovietTax.Updater")); break;
                    case "13": Application.Run(CreateForm("SaovietTax.ExportExcelBC")); break;
                    case "14": Application.Run(CreateForm("SaovietTax.frmKiemtraUpdate")); break;
                    case "15": Application.Run(CreateForm("SaovietTax.frmTuKhoaHang")); break;
                }
            }
        }

        // ★ HÀM TẠO FORM QUA REFLECTION — tránh new trực tiếp
        private static Form CreateForm(string fullTypeName)
        {
            var type = Type.GetType(fullTypeName + ", SaovietTax");
            if (type == null)
                throw new Exception("Không tìm thấy form: " + fullTypeName);
            return (Form)Activator.CreateInstance(type);
        }

        // ★ HÀM RESOLVER — TÌM DLL TRONG SharedDLLs ★
        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name;

            // 1. Tìm trong SharedDLLs (DLL chính)
            var path = Path.Combine(SharedDllsPath, name + ".dll");
            if (File.Exists(path))
                return Assembly.LoadFrom(path);

            // 2. Tìm theo culture (de, fr, ja, zh-cn, zh-tw...)
            var culture = Thread.CurrentThread.CurrentUICulture.Name;
            if (!string.IsNullOrEmpty(culture))
            {
                path = Path.Combine(SharedDllsPath, culture, name + ".dll");
                if (File.Exists(path))
                    return Assembly.LoadFrom(path);
            }

            // 3. Fallback: tìm trong thư mục chạy (DLL riêng của công ty)
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name + ".dll");
            if (File.Exists(path))
                return Assembly.LoadFrom(path);

            return null;
        }

        public static void SaveConfig(string key, string value)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings[key] != null)
                config.AppSettings.Settings[key].Value = value;
            else
                config.AppSettings.Settings.Add(key, value);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public static string connectionString;

        public static System.Data.DataTable ExecuteQuery(string query, params OleDbParameter[] parameters)
        {
            System.Data.DataTable dataTable = new System.Data.DataTable();
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        if (parameters != null)
                            command.Parameters.AddRange(parameters);

                        using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(command))
                            dataAdapter.Fill(dataTable);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return dataTable;
        }

        static void AddToStartup()
        {
            string appPath = Assembly.GetExecutingAssembly().Location;
            string directoryPath = Path.GetDirectoryName(appPath);
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));
            string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");
            string pathThumuc = Path.Combine(rootDirectory);
            string dbPath = "";
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
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";

            string queryGetdetail = @"SELECT * FROM tbregister";
            DataTable tbImportdetails = ExecuteQuery(queryGetdetail);
            string appName = tbImportdetails.Rows[0].Field<string>("Username");
            string exePath = $"\"{Application.ExecutablePath}\" -autostart";

            RegistryKey rk = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (rk.GetValue(appName) == null)
                rk.SetValue(appName, exePath);
        }
    }
}