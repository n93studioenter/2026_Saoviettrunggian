using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Data.OleDb;
using System.Reflection;
using System.IO;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraTreeList;

namespace SaovietTax
{
	public partial class frmTaikhoan: DevExpress.XtraEditors.XtraForm
	{
        public frmTaikhoan()
		{
            InitializeComponent();
		}
        public frmMain frmMain { get; set; }
        private List<Item> CreateData()
        {
            List<Item> dsTk = new List<Item>();
            string query = $"SELECT * FROM HeThongTK ";
            var parameterss = new OleDbParameter[]
         {
                    new OleDbParameter("?","1"),
            };
            var kq = ExecuteQuery(query, parameterss);
            if (kq.Rows.Count > 0)
            {
                foreach(DataRow item in kq.Rows)
                {
                    if (item["SoHieu"].ToString().StartsWith("112"))
                    {
                        dsTk.Add(new Item { Id = (int)item["MaSo"], Name = (string)item["SoHieu"] + "|" + Helpers.ConvertVniToUnicode((string)item["Ten"]), ParentId = (int)item["TKCha0"] });
                    }
                }
            }
            return dsTk;
    //        return new List<Item>
    //{
    //    new Item { Id = 1, Name = "Root 1", ParentId = 0 },
    //    new Item { Id = 2, Name = "Child 1.1", ParentId = 1 },
    //    new Item { Id = 3, Name = "Child 1.1.1", ParentId = 2 }, // Con của Child 1.1
    //    new Item { Id = 4, Name = "Child 1.1.2", ParentId = 2 }, // Con của Child 1.1
    //    new Item { Id = 5, Name = "Child 1.2", ParentId = 1 },
    //    new Item { Id = 6, Name = "Root 2", ParentId = 0 },
    //    new Item { Id = 7, Name = "Child 2.1", ParentId = 6 },
    //    new Item { Id = 8, Name = "Child 2.1.1", ParentId = 7 } // Con của Child 2.1
    //};
      }
        public class Item
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int ParentId { get; set; } // Để xác định cấu trúc cây
        }
        private void frmTaikhoan_Load(object sender, EventArgs e)
        {
            var data = CreateData();
            treeList1.DataSource = data;
            treeList1.ParentFieldName = "ParentId"; // Thiết lập mối quan hệ cha-con
            treeList1.KeyFieldName = "Id"; // Thuộc tính khóa
            treeList1.ExpandAll();

            //LoadList();
        }
        private void LoadList()
        {
            string query = $"SELECT SoHieu FROM HeThongTK  ";
                var parameterss = new OleDbParameter[]
             {
                    new OleDbParameter("?","1"), 
                };
                var kq = ExecuteQuery(query, parameterss);
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

        private void treeList1_AfterExpand(object sender, DevExpress.XtraTreeList.NodeEventArgs e)
        {
          
        }

        private void treeList1_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
        {
            TreeList treeList = sender as TreeList;
            if (treeList == null) return;

            // Lấy node hiện tại đang được focus
            TreeListNode focusedNode = treeList.FocusedNode;
            if (focusedNode != null)
            {
                // Lấy giá trị của thuộc tính "Id" từ node
                // Bạn cần ép kiểu về kiểu dữ liệu của Id (ví dụ: int)
                int nodeId = (int)focusedNode.GetValue("Id");
                string nodeName = focusedNode.GetValue("Name")?.ToString();
                textEdit1.Text = nodeName.Split('|')[0];
                textEdit2.Text = nodeName.Split('|')[1]; 
                txtID.Text = nodeId.ToString();
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            textEdit1.Text="";
            textEdit2.Text="";
            txtID.Text = "";
        }

        private void btnGhi_Click(object sender, EventArgs e)
        {
                 //       string query = @"UPDATE HeThongTK SET KyHieu=?  WHERE  MaSo=?";
                 //       var parameters = new OleDbParameter[]
                 //{
                 //                           new OleDbParameter("?",txtKyHieu.Text), 
                 //                              new OleDbParameter("?",txtID.Text),
                 //};
                 //       int rowsAffected = ExecuteQueryResult(query, parameters);
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {

        }

        private void btnThoat_Click(object sender, EventArgs e)
        {

        }

        private void treeList1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            frmMain.KyHieu = txtKyHieu.Text;
            frmMain.tknh = textEdit1.Text+"-"+textEdit2.Text;
            this.Close();
        }
    }
}