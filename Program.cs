using DevExpress.XtraEditors;
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

namespace SaovietTax {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        static Mutex mutex;

        [STAThread]
        static void Main(string[] args) { 

            //Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

           // mutex = new Mutex(false, "Global\\MyCompany_MyProduct_SingleInstance");
            bool isAutoStart = args.Length > 0 && args[0] == "-autostart"; 
            try
            { 

                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                //if (isAutoStart)
                //    mutex.ReleaseMutex();
            }
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            //ServicePointManager.DefaultConnectionLimit = 100;
            //ServicePointManager.Expect100Continue = false;
            // Số + tiền theo en-US (1,234,567.89)


            // QUAN TRỌNG: Đăng ký event dịch string – đây là cách chính thức!
            // AddToStartup(); // 👈 GỌI Ở ĐÂY
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (isAutoStart)
            { 
                //mutex.WaitOne();
                SaveConfig("Mode", "2");
                Application.Run(new frmMain());
              
               // Application.Run(new frmAutotaiNew());
            }
            else
            {
                SaveConfig("Mode", "1");
                string appPath = Assembly.GetExecutingAssembly().Location;

                // Lấy thư mục chứa ứng dụng
                string directoryPath = Path.GetDirectoryName(appPath);

                // Xóa phần \bin\Debug để lấy đường dẫn gốc
                string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

                // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
                string filePaths = Path.Combine(rootDirectory, "hoadon", "status.txt");
                string content = "1";

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Bắt buộc DevExpress nhận biết DPI đúng
                WindowsFormsSettings.SetDPIAware();   // Dòng quan trọng nhất!

                // Force font chuẩn cho toàn app (nếu cần)
                // WindowsFormsSettings.DefaultFont = new Font("Tahoma", 8.25f);

                // Force font cho grid header (nếu vẫn to)
                //DevExpress.Utils.AppearanceObject.DefaultFont = new Font("Tahoma", 8.25f);
                try
                {
                    content = File.ReadAllText(filePaths);
                }
                catch (Exception ex)
                {

                }

                //if (content.Trim() == "1")
                //    Application.Run(new frmMain());
                //else
                //{
                //    if (content.Trim() == "2")
                //        Application.Run(new KTHT());
                //    else
                //    {
                //        if (content.Trim() == "3")
                //            Application.Run(new frmTaihoadonvb());
                //        else
                //        {
                //            if (content.Trim() == "4")
                //                Application.Run(new Form3());
                //            else
                //            {
                //                if (content.Trim() == "5")
                //                    Application.Run(new APIInvoice());
                //                else
                //                {
                //                    if (content.Trim() == "6")
                //                        Application.Run(new BkavInvoice());
                //                }
                //            }
                //        }
                //    }
                //}

                switch (content.Trim())
                {
                    case "1": Application.Run(new frmMain()); break;
                    case "2": Application.Run(new KTHT()); break;
                    case "3": Application.Run(new frmTaihoadonvb()); break;
                    case "4": Application.Run(new Form3()); break;
                    case "5": Application.Run(new APIInvoice()); break;
                    case "6": Application.Run(new BkavInvoice()); break;
                    case "7": Application.Run(new VNPTInvoice()); break;
                    case "8": Application.Run(new TendoInvoice()); break;
                    case "9": Application.Run(new frmQrcode()); break;
                    case "10": Application.Run(new vb6Xemhoadon()); break;
                    case "11": Application.Run(new Vanguard()); break;
                    case "12": Application.Run(new Updater()); break;
                    case "13": Application.Run(new ExportExcelBC()); break;
                    case "14": Application.Run(new frmKiemtraUpdate()); break;
                    case "15": Application.Run(new frmTuKhoaHang()); break;
                }
            }
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
        static void AddToStartup()
        {
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);

            // Xóa phần \bin\Debug để lấy đường dẫn gốc
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
            string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");
            string  pathThumuc = Path.Combine(rootDirectory);
            string dbPath = "";
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


            // Đọc toàn bộ nội dung tệp
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

        //static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        //{
        //    MessageBox.Show($"Lỗi UI Thread: {e.Exception.Message}");
        //}

        //static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        //{
        //    Exception ex = (Exception)e.ExceptionObject;
        //    File.WriteAllText("error.log", $"Lỗi không xử lý: {ex.Message}\n{ex.StackTrace}");
        //}
    }
}
