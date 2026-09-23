using DevExpress.XtraEditors;
using Microsoft.Win32;
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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SaovietTax.frmHangHoa;

namespace SaovietTax
{
    public partial class FrmAutoTaiSetting : DevExpress.XtraEditors.XtraForm
    {
        public FrmAutoTaiSetting()
        {
            InitializeComponent();
        }
        bool allowUncheck = false;
        private void radioButton1_Click(object sender, EventArgs e)
        {
            if (allowUncheck)
            {
                radioButton1.Checked = false;
                string querys = @"UPDATE tbRegister SET taitd = ?";

                var parameterss = new OleDbParameter[]
                 {
                   new OleDbParameter("?","0"),
                 };
                int rowsAffecteds = ExecuteQueryResult(querys, parameterss);
                allowUncheck = radioButton1.Checked;
                RemoveFromStartup();
                return;
            }

            // Lật trạng thái 
            allowUncheck = radioButton1.Checked;
            string query = @"UPDATE tbRegister SET taitd = ?";

            var parameters = new OleDbParameter[]
     {
                                new OleDbParameter("?","1"),
     };
            int rowsAffected = ExecuteQueryResult(query, parameters);
            AddToStartup();
        }
        public int ExecuteQueryResult(string query, params OleDbParameter[] parameters)
        {
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Kết nối đến cơ sở dữ liệu thành công! " + query);

                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    // Thêm tham số
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    // Thực thi INSERT, UPDATE, DELETE
                    command.ExecuteNonQuery();
                }

                // Lấy ID vừa thêm bằng @@IDENTITY
                using (OleDbCommand idCommand = new OleDbCommand("SELECT @@IDENTITY", connection))
                {
                    object result = idCommand.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }
        public int ExecuteQueryResult2(string query, params OleDbParameter[] parameters)
        {
            using (OleDbConnection connection = new OleDbConnection(connectionString2))
            {
                connection.Open();
                Console.WriteLine("Kết nối đến cơ sở dữ liệu thành công! " + query);

                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    // Thêm tham số
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    // Thực thi INSERT, UPDATE, DELETE
                    command.ExecuteNonQuery();
                }

                // Lấy ID vừa thêm bằng @@IDENTITY
                using (OleDbCommand idCommand = new OleDbCommand("SELECT @@IDENTITY", connection))
                {
                    object result = idCommand.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }
        public void AddToStartup()
        {
            try
            {
                // 1. Lấy đường dẫn file EXE
                string exePath = Application.ExecutablePath;

                // 2. Kiểm tra file tồn tại
                if (!File.Exists(exePath))
                {
                    XtraMessageBox.Show($"❌ File EXE không tồn tại:\n{exePath}");
                    return;
                }

                // 3. Bọc trong dấu ngoặc kép + thêm tham số
                string exeWithArgs = $"\"{exePath}\" -autostart";

                // 4. Lấy tên hiển thị (không có dấu cách)
                string queryGetdetail = @"SELECT * FROM tbregister";
                DataTable tbImportdetails = ExecuteQuery(queryGetdetail);
                string appName = tbImportdetails.Rows[0].Field<string>("Username");

                // 5. Mở Registry Run
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (rk == null)
                    {
                        XtraMessageBox.Show("❌ Không thể mở Registry!");
                        return;
                    }

                    // 6. Xóa entry cũ nếu có
                    if (rk.GetValue(appName) != null)
                    {
                        rk.DeleteValue(appName);
                        XtraMessageBox.Show($"🔄 Đã xóa entry cũ: {appName}");
                    }

                    // 7. Thêm entry mới
                    rk.SetValue(appName, exeWithArgs);

                    // 8. Hiển thị thông tin để kiểm tra
                    XtraMessageBox.Show($"✅ Đã đăng ký Startup!\n\n" +
                                       $"Tên: {appName}\n" +
                                       $"Đường dẫn: {exeWithArgs}\n\n" +
                                       $"📌 Kiểm tra lại: regedit → Run",
                                       "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"❌ Lỗi: {ex.Message}");
            }
        }
        public void RemoveFromStartup()
        {
            try
            {
                string queryGetdetail = @"SELECT * FROM tbregister";
                DataTable tbImportdetails = ExecuteQuery(queryGetdetail);

                if (tbImportdetails.Rows.Count > 0)
                {
                    string appName = tbImportdetails.Rows[0].Field<string>("Username");

                    RegistryKey rk = Registry.CurrentUser.OpenSubKey(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                    // Kiểm tra và xoá
                    if (rk.GetValue(appName) != null)
                    {
                        rk.DeleteValue(appName, false);
                        string query = @"UPDATE tbRegister SET taitd = ?";

                        var parameters = new OleDbParameter[]
                 {
                                new OleDbParameter("?","0"),
                 };
                        int rowsAffected = ExecuteQueryResult(query, parameters);
                    }
                    else
                    {
                        //MessageBox.Show($"Không tìm thấy '{appName}' trong Startup.", "Thông báo",
                        //              MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    //MessageBox.Show("Không tìm thấy thông tin đăng ký.", "Lỗi",
                    //              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Lỗi khi xoá khỏi Startup: {ex.Message}", "Lỗi",
                //              MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    Console.WriteLine(ex.Message);
                }

            }

            return dataTable; // Trả về DataTable chứa dữ liệu
        }
        string connectionString = "";
        string connectionString2 = "";
        string dbPath = "";
        string filename = "";
        private void FrmAutoTaiSetting_Load(object sender, EventArgs e)
        {
           
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);

            // Xóa phần \bin\Debug để lấy đường dẫn gốc
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));
            // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
            string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");  
            //MessageBox.Show(pathThumuc);
            try
            {
                string content = File.ReadAllText(filePaths); 
                dbPath = content;
                string fullName = dbPath.Substring(dbPath.LastIndexOf('\\') + 1);
                // fullName = "Thanh Huong BD2026.mdb"

                string fn = fullName.Substring(0, fullName.LastIndexOf('.')).Trim();
                filename = fn;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi khi đọc file: " + ex.Message);
            }
            string password = "1@35^7*9)1";
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";


            string querykh = @" SELECT *  FROM tbRegister"; // Sử dụng ? thay cho @mst trong OleDb

            DataTable tbRegister = ExecuteQuery(querykh, new OleDbParameter("?", ""));
            string getTudong = tbRegister.Rows[0].Field<string>("taitd");
            if (string.IsNullOrEmpty(getTudong))
            {
                // chkTudong.Checked = true;
                //chkTudong_CheckedChanged(chkTudong, EventArgs.Empty);
                radioButton1.PerformClick();
            }
            else
            {
                bool istaitudong = tbRegister.Rows[0].Field<string>("taitd") == "1" ? true : false;
                allowUncheck = istaitudong;
                radioButton1.Checked = istaitudong;
            }
            //Time tải
            string Moctg1 = tbRegister.Rows[0].Field<string>("Moctg1");
            string Moctg2 = tbRegister.Rows[0].Field<string>("Moctg2");
            string Moctg3 = tbRegister.Rows[0].Field<string>("Moctg3");
            string isresitry = tbRegister.Rows[0]["IsRegisTry"].ToString();
            chkThietlaptong.Checked= isresitry=="1"?true:false;
            int Soluottai = !string.IsNullOrEmpty(tbRegister.Rows[0]["Soluottai"].ToString()) ?int.Parse(tbRegister.Rows[0]["Soluottai"].ToString()):0;
            if (!string.IsNullOrEmpty(Moctg1))
            {
                chkTime1.Checked = true;
                txtTime1.Text = Moctg1;
                txtTime1.Enabled = true;    
            }
            if (!string.IsNullOrEmpty(Moctg2))
            {
                chkTime2.Checked = true;
                txtTime2.Text = Moctg2;
                txtTime2.Enabled = true;
            }
            if (!string.IsNullOrEmpty(Moctg3))
            {
                chkTime3.Checked = true;
                txtTime3.Text = Moctg3;
                txtTime3.Enabled = true;
            }
            txtSolantai.Text = Soluottai.ToString();
            int thoigiancho = !string.IsNullOrEmpty(tbRegister.Rows[0]["Thoigiantai"].ToString()) ? int.Parse(tbRegister.Rows[0]["Thoigiantai"].ToString()) : 0;
            txtThoigiancho.Text = thoigiancho.ToString();   
        }

        private void chkTime1_CheckedChanged(object sender, EventArgs e)
        {
            txtTime1.Enabled = chkTime1.Checked;
            if (chkTime1.Checked == false)
            {
                txtTime1.Text = "";
            }
        }

        private void chkTime2_CheckedChanged(object sender, EventArgs e)
        {
            txtTime2.Enabled = chkTime2.Checked;
            if (chkTime2.Checked == false)
            {
                txtTime2.Text = "";
            }
        }

        private void chkTime3_CheckedChanged(object sender, EventArgs e)
        {
            txtTime3.Enabled = chkTime3.Checked;
            if (chkTime3.Checked == false)
            {
                txtTime3.Text = "";
            }
        }

        private void txtTime1_EditValueChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTime1.Text))
            {
                string query = "UPDATE tbsetting SET Moctg1 = ?";
                // Khai báo mảng tham số với đủ 10 tham số
                OleDbParameter[] parameters = new OleDbParameter[]
                {
        new OleDbParameter("?", txtTime1.Text)
                };

                // Thực thi truy vấn và lấy kết quả
                int a = ExecuteQueryResult(query, parameters);
                ScheduleHelper ScheduleHelper = new ScheduleHelper();
                DeleteSchedule($"{filename}_L1");
            }
        }

        private void txtTime2_EditValueChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTime2.Text))
            {
                string query = "UPDATE tbRegister SET Moctg2 = ?";
                // Khai báo mảng tham số với đủ 10 tham số
                OleDbParameter[] parameters = new OleDbParameter[]
                {
        new OleDbParameter("?", txtTime2.Text)
                };

                // Thực thi truy vấn và lấy kết quả
                int a = ExecuteQueryResult(query, parameters);
                ScheduleHelper ScheduleHelper = new ScheduleHelper();
                DeleteSchedule($"{filename}_L2");
            }
        }

        private void txtTime3_EditValueChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTime3.Text))
            {
                string query = "UPDATE tbRegister SET Moctg3 = ?";
                // Khai báo mảng tham số với đủ 10 tham số
                OleDbParameter[] parameters = new OleDbParameter[]
                {
        new OleDbParameter("?", txtTime3.Text)
                };

                // Thực thi truy vấn và lấy kết quả
                int a = ExecuteQueryResult(query, parameters);
                ScheduleHelper ScheduleHelper = new ScheduleHelper();
                DeleteSchedule($"{filename}_L3");
            }
        }

        private void txtSolantai_EditValueChanged(object sender, EventArgs e)
        {
            string query = "UPDATE tbRegister SET Soluottai = ?";
            // Khai báo mảng tham số với đủ 10 tham số
            OleDbParameter[] parameters = new OleDbParameter[]
            {
        new OleDbParameter("?", txtSolantai.Text)
            };

            // Thực thi truy vấn và lấy kết quả
            int a = ExecuteQueryResult(query, parameters);
        }

        private void txtTime1_Validated(object sender, EventArgs e)
        {
            string query = "UPDATE tbRegister SET Moctg1 = ?";
            // Khai báo mảng tham số với đủ 10 tham số
            OleDbParameter[] parameters = new OleDbParameter[]
            {
        new OleDbParameter("?", txtTime1.Text)
            };

            // Thực thi truy vấn và lấy kết quả
            int a = ExecuteQueryResult(query, parameters);
            ScheduleHelper ScheduleHelper = new ScheduleHelper();
            DateTime time = DateTime.Parse(txtTime1.Text);

            int hour = time.Hour;   // 10
            int minute = time.Minute; // 30
            try
            {
                CreateSchedule($"{filename}_L1", hour, minute);
            }
            catch(Exception ex)
            {
                XtraMessageBox.Show(ex.Message);    
            }
        }

        private void txtTime2_Validated(object sender, EventArgs e)
        {

            string query = "UPDATE tbRegister SET Moctg2 = ?";
            // Khai báo mảng tham số với đủ 10 tham số
            OleDbParameter[] parameters = new OleDbParameter[]
            {
        new OleDbParameter("?", txtTime2.Text)
            };

            // Thực thi truy vấn và lấy kết quả
            int a = ExecuteQueryResult(query, parameters);
            ScheduleHelper ScheduleHelper = new ScheduleHelper();
            DateTime time = DateTime.Parse(txtTime2.Text);

            int hour = time.Hour;   // 10
            int minute = time.Minute; // 30
            CreateSchedule($"{filename}_L2", hour, minute);
        }

        private void txtTime3_Validated(object sender, EventArgs e)
        {
            string query = "UPDATE tbRegister SET Moctg3 = ?";
            // Khai báo mảng tham số với đủ 10 tham số
            OleDbParameter[] parameters = new OleDbParameter[]
            {
        new OleDbParameter("?", txtTime3.Text)
            };

            // Thực thi truy vấn và lấy kết quả
            int a = ExecuteQueryResult(query, parameters);
            ScheduleHelper ScheduleHelper = new ScheduleHelper();
            DateTime time = DateTime.Parse(txtTime3.Text);

            int hour = time.Hour;   // 10
            int minute = time.Minute; // 30
            CreateSchedule($"{filename}_L3", hour, minute);
        }
        public static void CreateSchedule(string taskName, int hour, int minute = 0)
        {
            try
            {
                string exePath = Application.ExecutablePath;
                string timeStr = $"{hour:D2}:{minute:D2}";

                // Xóa task cũ (nếu có)
                RunSchTasks($"/delete /tn \"{taskName}\" /f", false);

                // Chuỗi thực thi
                string tr = $"\\\"{exePath}\\\" -autostart";

                // Tạo task
                string args =
                    $"/create /tn \"{taskName}\" " +
                    $"/tr \"{tr}\" " +
                    $"/sc daily " +
                    $"/st {timeStr} " +
                    $"/f";

                // Debug xem lệnh tạo ra
                XtraMessageBox.Show(args);

                RunSchTasks(args, true);

                XtraMessageBox.Show($"Đã tạo lịch '{taskName}' lúc {timeStr} mỗi ngày.");
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.ToString());
            }
        }

        private static void RunSchTasks(string arguments, bool throwIfError)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process p = Process.Start(psi))
            {
                string output = p.StandardOutput.ReadToEnd();
                string error = p.StandardError.ReadToEnd();

                p.WaitForExit();

                if (p.ExitCode != 0 && throwIfError)
                {
                    throw new Exception(
                        $"ExitCode: {p.ExitCode}\r\n\r\n" +
                        $"Output:\r\n{output}\r\n\r\n" +
                        $"Error:\r\n{error}"
                    );
                }
            }
        }
        // ========================================
        // HÀM XÓA LỊCH
        // ========================================
        public static void DeleteSchedule(string taskName)
        {
            try
            {
                string cmd = $"schtasks /delete /tn \"{taskName}\" /f";
                Process.Start("cmd", "/c " + cmd).WaitForExit();
                XtraMessageBox.Show($"✅ Đã xóa lịch '{taskName}'!");
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"❌ Lỗi xóa: {ex.Message}");
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void txtThoigiancho_EditValueChanged(object sender, EventArgs e)
        {
            string query = "UPDATE tbRegister SET Thoigiantai = ?";
            // Khai báo mảng tham số với đủ 10 tham số
            OleDbParameter[] parameters = new OleDbParameter[]
            {
        new OleDbParameter("?", txtThoigiancho.Text)
            };

            // Thực thi truy vấn và lấy kết quả
            int a = ExecuteQueryResult(query, parameters);
        }

        private void chkThietlaptong_CheckedChanged(object sender, EventArgs e)
        {
            if (chkThietlaptong.Checked)
            {
                //Gỡ tải khi khoi dong máy
                string qr = @"SELECT * FROM tbregister";
                DataTable getRegister = ExecuteQuery(qr);
                if (getRegister.Rows[0]["taitd"].ToString() == "1")
                {
                    RemoveFromStartup();
                }
                string querys = @"UPDATE tbRegister SET IsRegistry = ?";

                var parameterss = new OleDbParameter[]
                 {
                   new OleDbParameter("?",chkThietlaptong.Checked?1:0),
                 };
                int rowsAffecteds = ExecuteQueryResult(querys, parameterss);
                string dbPath = Path.Combine("\\\\192.168.1.90\\Ke toan 2025 New\\1 Copi vao dung 1\\Hoadon", "Tooldb.accdb");
                //string dbPath = Path.Combine("D:\\", "Tooldb.accdb");
                connectionString2 = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};";
                string queryGetdetail = @"SELECT * FROM tbCompany";
                DataTable tbImportdetails = ExecuteQuery2(queryGetdetail);
                string getmst = getRegister.Rows[0]["Username"].ToString();
                //Kiem tra đã thêm chưa

                string querycheck = @"SELECT COUNT(*) FROM tbCompany WHERE MST = ? and Saoviet =?";
                string computerName = Environment.MachineName;

                var parameterss2 = new OleDbParameter[]
               {
                   new OleDbParameter("?",getmst),
                    new OleDbParameter("?",computerName),
               };
                DataTable getTablecheck = ExecuteQuery2(querycheck, parameterss2);
                int count = Convert.ToInt32(getTablecheck.Rows[0][0]);

                if (count == 0)
                {
                    // 1. Sửa câu lệnh INSERT - bỏ cột Dauvao bị trùng
                    string qrInsert = @"INSERT INTO tbCompany (Name, Dbpath, FolderPath, MST, STT, Status, IsRun, Dauvao, Daura, Saoviet) 
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                    string namecongty = getRegister.Rows[0]["Dbpath"].ToString().Split('\\')[4].Replace(".MDB", "");
                 
                    var parameters = new OleDbParameter[]
                    {
                        new OleDbParameter("?", namecongty),                                      // 1. Name
                        new OleDbParameter("?", getRegister.Rows[0]["Dbpath"].ToString()),       // 2. Dbpath
                        new OleDbParameter("?", getRegister.Rows[0]["Hoadonpath"].ToString()),   // 3. FolderPath
                        new OleDbParameter("?", getmst),                                         // 4. MST
                        new OleDbParameter("?", 1),                                              // 5. STT
                        new OleDbParameter("?", 1),                                              // 6. Status
                        new OleDbParameter("?", 1),                                              // 7. IsRun  <-- THÊM VÀO
                        new OleDbParameter("?", 1),                                              // 8. Dauvao
                        new OleDbParameter("?", 1),                                              // 9. Daura
                        new OleDbParameter("?", computerName)                               // 10. Saoviet
                    };

                    int rowsAffected = ExecuteQueryResult2(qrInsert, parameters);
                }
            }
            else
            {
                string qr = @"SELECT * FROM tbregister";
                DataTable getRegister = ExecuteQuery(qr);

                string querys = @"UPDATE tbRegister SET IsRegistry = ?";
                var pra = new OleDbParameter[]
                   {
                        new OleDbParameter("?", "0"),                               // 10. Saoviet
                   };
                int redd= ExecuteQueryResult(querys, pra);
                var parameterss = new OleDbParameter[]
                 {
                   new OleDbParameter("?",chkThietlaptong.Checked?1:0),
                 };
                string query = "DELETE FROM [tbCompany] WHERE [MST] = ? AND [Saoviet] = ?";
                string computerName = Environment.MachineName;
                var parameters = new OleDbParameter[]
                  {                                // 9. Daura
                        new OleDbParameter("?", getRegister.Rows[0]["Username"].ToString()),
                        new OleDbParameter("?", computerName)                               // 10. Saoviet
                  };

                int rowsAffected = ExecuteQueryResult2(query, parameters);
            }
        }

        public System.Data.DataTable ExecuteQuery2(string query, params OleDbParameter[] parameters)
        {
            System.Data.DataTable dataTable = new System.Data.DataTable();

            using (OleDbConnection connection = new OleDbConnection(connectionString2))
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
                    Console.WriteLine(ex.Message);
                }

            }

            return dataTable; // Trả về DataTable chứa dữ liệu
        }
    }
}