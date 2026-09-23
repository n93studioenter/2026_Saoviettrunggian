using ClosedXML.Excel;
using DevExpress.XtraEditors;
using DevExpress.XtraMap.Native;
using DocumentFormat.OpenXml.Spreadsheet;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tensorflow;

namespace SaovietTax
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }
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
        public class ChungTuInfo
        {
            public string MaCT { get; set; }
            public int ThangCT { get; set; }
            public string SoHieu { get; set; }
            public DateTime NgayCT { get; set; }
            public DateTime NgayGS { get; set; }
            public string DienGiai { get; set; }
            public double SoPS { get; set; }
            public string GhiChu { get; set; }
            public string SoHieuTK { get; set; }
            public string SoHieuTK1 { get; set; }
            public string MaTKTCNo { get; set; }
            public string MaTKTCCo { get; set; }
            public string SH1 { get; set; }
        }
        private List<ChungTuInfo> ConvertDataTableToList(DataTable dt)
        {
            List<ChungTuInfo> list = new List<ChungTuInfo>();

            foreach (DataRow row in dt.Rows)
            {
                ChungTuInfo item = new ChungTuInfo
                {
                    MaCT = row["MaCT"]?.ToString(),
                    ThangCT = Convert.ToInt32(row["ThangCT"]),
                    SoHieu = row["SoHieu"]?.ToString(),
                    NgayCT = Convert.ToDateTime(row["NgayCT"]),
                    NgayGS = Convert.ToDateTime(row["NgayGS"]),
                    DienGiai = row["DienGiai"]?.ToString(),
                    SoPS = Convert.ToDouble(row["SoPS"]),
                    GhiChu = row["GhiChu"]?.ToString(),
                    SoHieuTK = row["SoHieuTK"]?.ToString(),
                    SoHieuTK1 = row["SoHieuTK1"]?.ToString(),
                    MaTKTCNo = row["MaTKTCNo"]?.ToString(),
                    MaTKTCCo = row["MaTKTCCo"]?.ToString(),
                    SH1 = row["SH1"]?.ToString()
                };
                list.Add(item);
            }

            return list;
        }
        public DataTable GetHeThongTKData(int thangTu, int thangDen)
        {
            DataTable dt = new DataTable(); 

            // Tạo danh sách các tháng cần lấy
            List<int> months = new List<int>();
            for (int i = thangTu; i <= thangDen; i++)
            {
                months.Add(i);
            }

            // Xây dựng cột PsNo: No_1 + No_2 + ...
            string psNoColumns = string.Join(" + ", months.Select(m => $"HeThongTK.No_{m}"));
            string psCoColumns = string.Join(" + ", months.Select(m => $"HeThongTK.Co_{m}"));

            // Xây dựng điều kiện WHERE cho số dư cuối kỳ
            string ckNoColumn = $"HeThongTK.DuNo_{thangDen}";
            string ckCoColumn = $"HeThongTK.DuCo_{thangDen}";

            // Xây dựng điều kiện kiểm tra phát sinh trong kỳ
            string psNoCondition = string.Join(" + ", months.Select(m => $"HeThongTK.No_{m}"));
            string psCoCondition = string.Join(" + ", months.Select(m => $"HeThongTK.Co_{m}"));

            string strSQL = $@"
    SELECT DISTINCTROW 
        HeThongTK.SoHieu, 
        HeThongTK.Cap, 
        HeThongTK.Ten, 
        HeThongTK.Kieu, 
        HeThongTK.Loai, 
        HeThongTK.DuNo_0 AS DkNo, 
        HeThongTK.DuCo_0 AS DkCo, 
        ({psNoColumns}) AS PsNo, 
        ({psCoColumns}) AS PsCo, 
        KC_N, 
        KC_C, 
        {ckNoColumn} AS CkNo, 
        {ckCoColumn} AS CkCo 
    FROM 
        HeThongTK 
    WHERE 
        ((HeThongTK.MaTC = 0 Or HeThongTK.MaTC = HeThongTK.MaSo) OR ((TK_ID3 MOD 10) >= 1)) 
        AND (HeThongTK.Loai > 0) 
        AND Cap <= 1 
        AND ({ckNoColumn} <> 0 OR {ckCoColumn} <> 0 
            OR ({psNoCondition}) <> 0 
            OR ({psCoCondition}) <> 0) 
        AND HeThongTK.Cap = 1";

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                using (OleDbCommand cmd = new OleDbCommand(strSQL, conn))
                {
                    using (OleDbDataAdapter da = new OleDbDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            return dt;
        }
        public DataTable GetChungTuData(int thangTu, int thangDen, string maTK)
        {
            DataTable dt = new DataTable();

            // Tạo pattern LIKE: ví dụ "111%" từ "111"
            string likePattern = maTK + "%";

            string sql = @"
            SELECT DISTINCTROW 
                ChungTu.MaCT, 
                ChungTu.ThangCT, 
                ChungTu.SoHieu, 
                ChungTu.NgayCT, 
                ChungTu.NgayGS, 
                ChungTu.DienGiai, 
                ChungTu.SoPS, 
                ChungTu.GhiChu, 
                HeThongTK.SoHieu AS SoHieuTK, 
                HeThongTK_1.SoHieu AS SoHieuTK1, 
                ChungTu.MaTKTCNo, 
                ChungTu.MaTKTCCo, 
                IIF(HeThongTK.SoHieu LIKE ?, '0', '1') + CStr(10 + ChungTu.ThangCT) + CStr(ChungTu.SoHieu) AS SH1
            FROM 
                HeThongTK AS HeThongTK_3 
                RIGHT JOIN (HeThongTK AS HeThongTK_2 
                    RIGHT JOIN (HeThongTK AS HeThongTK_1 
                        RIGHT JOIN (HeThongTK 
                            RIGHT JOIN ChungTu ON HeThongTK.MaSo = ChungTu.MaTKTCNo)
                        ON HeThongTK_1.MaSo = ChungTu.MaTKTCCo)
                    ON HeThongTK_2.MaSo = ChungTu.MaTKNo)
                ON HeThongTK_3.MaSo = ChungTu.MaTKCo
            WHERE 
                ChungTu.SoPS <> 0 
                AND ((HeThongTK.SoHieu LIKE ?) OR (HeThongTK_1.SoHieu LIKE ?))
                AND (ChungTu.ThangCT >= ? AND ChungTu.ThangCT <= ?)
                AND (ChungTu.MaLoai <> 4 OR (ChungTu.MaLoai = 4 AND ChungTu.MaTKNo <> ChungTu.MaTkco))
            ORDER BY 
                ChungTu.ThangCT, 
                ChungTu.NgayGS, 
                IIF(HeThongTK.SoHieu LIKE ?, '0', '1') + CStr(10 + ChungTu.ThangCT) + CStr(ChungTu.SoHieu)";

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                {
                    // Thêm tham số - thứ tự theo dấu ? trong câu SQL
                    cmd.Parameters.AddWithValue("@p1", likePattern);  // cho IIF đầu tiên
                    cmd.Parameters.AddWithValue("@p2", likePattern);  // cho điều kiện WHERE thứ nhất
                    cmd.Parameters.AddWithValue("@p3", likePattern);  // cho điều kiện WHERE thứ hai
                    cmd.Parameters.AddWithValue("@p4", thangTu);      // tháng từ
                    cmd.Parameters.AddWithValue("@p5", thangDen);     // tháng đến
                    cmd.Parameters.AddWithValue("@p6", likePattern);  // cho ORDER BY

                    conn.Open();
                    using (OleDbDataAdapter da = new OleDbDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            return dt;
        }
        public class SoNhatKyChung
        {
            public int MS { get; set; }
            public string MaCT { get; set; }
            public string SoHieu { get; set; }
            public DateTime NgayCT { get; set; }
            public DateTime NgayGS { get; set; }
            public string DienGiai { get; set; }
            public double SumOfSoPS { get; set; }
            public string SoHieuTK { get; set; }
            public string TenTK { get; set; }
            public int LoaiPS { get; set; }  // -1: Nợ, 1: Có
        }
        public List<SoNhatKyChung> ConvertToList(DataTable dt)
        {
            List<SoNhatKyChung> list = new List<SoNhatKyChung>();

            foreach (DataRow row in dt.Rows)
            {
                SoNhatKyChung item = new SoNhatKyChung
                {
                    MS = row["MS"] != DBNull.Value ? Convert.ToInt32(row["MS"]) : 0,
                    MaCT = row["MaCT"]?.ToString() ?? "",
                    SoHieu = row["ChungTu.SoHieu"]?.ToString() ?? "",
                    NgayCT = row["NgayCT"] != DBNull.Value ? Convert.ToDateTime(row["NgayCT"]) : DateTime.MinValue,
                    NgayGS = row["NgayGS"] != DBNull.Value ? Convert.ToDateTime(row["NgayGS"]) : DateTime.MinValue,
                    DienGiai = row["DienGiai"]?.ToString() ?? "",
                    SumOfSoPS = row["SumOfSoPS"] != DBNull.Value ? Convert.ToDouble(row["SumOfSoPS"]) : 0,
                    SoHieuTK = row["HeThongTK.SoHieu"]?.ToString() ?? "",
                    TenTK = row["Ten"]?.ToString() ?? "",
                    LoaiPS = row["LoaiPS"] != DBNull.Value ? Convert.ToInt32(row["LoaiPS"]) : 0
                };
                list.Add(item);
            }

            return list;
        }
        public class ChungTuModel
        {
            public int MS { get; set; }
            public int MaCT { get; set; }
            public string SoHieu { get; set; }
            public DateTime NgayCT { get; set; }
            public DateTime NgayGS { get; set; }
            public string DienGiai { get; set; }
            public decimal SumOfSoPS { get; set; }
            public string SoHieuTK { get; set; }
            public string TenTK { get; set; }
            public int LoaiPS { get; set; }
        }
        public List<ChungTuModel> ConvertToList1(DataTable dt)
        {
            var list = new List<ChungTuModel>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ChungTuModel
                {
                    MS = row["MS"] != DBNull.Value ? Convert.ToInt32(row["MS"]) : 0,
                    MaCT = row["MaCT"] != DBNull.Value ? Convert.ToInt32(row["MaCT"]) : 0,
                    SoHieu = row["ChungTu.SoHieu"]?.ToString(),
                    NgayCT = row["NgayCT"] != DBNull.Value ? Convert.ToDateTime(row["NgayCT"]) : DateTime.MinValue,
                    NgayGS = row["NgayGS"] != DBNull.Value ? Convert.ToDateTime(row["NgayGS"]) : DateTime.MinValue,
                    DienGiai = row["DienGiai"]?.ToString(),
                    SumOfSoPS = row["SumOfSoPS"] != DBNull.Value ? Convert.ToDecimal(row["SumOfSoPS"]) : 0,
                    SoHieuTK = row["HeThongTK.SoHieu"]?.ToString(),
                    TenTK = row["Ten"]?.ToString(),
                    LoaiPS = row["LoaiPS"] != DBNull.Value ? Convert.ToInt32(row["LoaiPS"]) : 0
                });
            }

            return list;
        }
        public DataTable Getbc1(int thangTu, int thangDen)
        {
            DataTable dt = new DataTable(); 

            string sql = $@"
        SELECT DISTINCTROW 
            First(ChungTu.MaSo) AS MS,
            ChungTu.MaCT, 
            ChungTu.SoHieu, 
            ChungTu.NgayCT, 
            ChungTu.NgayGS, 
            ChungTu.DienGiai, 
            Sum(SoPS) AS SumOfSoPS, 
            HeThongTK.SoHieu, 
            HeThongTK.Ten, 
            -1 AS LoaiPS 
        FROM ChungTu 
        INNER JOIN HeThongTK ON ChungTu.MaTKNo = HeThongTK.MaSo  
        WHERE SoPS <> 0 
            AND (MaTKTCNo) > 0 
            AND ((HeThongTK.Loai) > 0) 
            AND ((ChungTu.MaLoai <> 4) OR (ChungTu.MaLoai = 4 AND ChungTu.MaTKNo <> ChungTu.MaTKCo)) 
            AND (ThangCT >= {thangTu} AND ThangCT <= {thangDen}) 
        GROUP BY 
            ChungTu.MaCT, 
            ChungTu.SoHieu, 
            ChungTu.NgayCT, 
            ChungTu.NgayGS, 
            ChungTu.DienGiai, 
            HeThongTK.SoHieu, 
            HeThongTK.Ten
        
        UNION 
        
        SELECT DISTINCTROW 
            First(ChungTu.MaSo) AS MS,
            ChungTu.MaCT, 
            ChungTu.SoHieu, 
            ChungTu.NgayCT, 
            ChungTu.NgayGS, 
            ChungTu.DienGiai, 
            Sum(SoPS) AS SumOfSoPS, 
            HeThongTK.SoHieu, 
            HeThongTK.Ten, 
            1 AS LoaiPS 
        FROM ChungTu 
        INNER JOIN HeThongTK ON ChungTu.MaTKCo = HeThongTK.MaSo  
        WHERE SoPS <> 0 
            AND (MaTKTCCo) > 0 
            AND ((HeThongTK.Loai) > 0) 
            AND ((ChungTu.MaLoai <> 4) OR (ChungTu.MaLoai = 4 AND ChungTu.MaTKNo <> ChungTu.MaTKCo)) 
            AND (ThangCT >= {thangTu} AND ThangCT <= {thangDen}) 
        GROUP BY 
            ChungTu.MaCT, 
            ChungTu.SoHieu, 
            ChungTu.NgayCT, 
            ChungTu.NgayGS, 
            ChungTu.DienGiai, 
            HeThongTK.SoHieu, 
            HeThongTK.Ten";

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                {
                    conn.Open();
                    using (OleDbDataAdapter da = new OleDbDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            return dt;
        }
        private void btnExoprt_Click(object sender, EventArgs e)
        {

            //Kiểm tra có thư mục Tailieu chua

            string quer = "SELECT * FROM License";
            var getLcv = ExecuteQuery(quer, null);
            int NamTC = int.Parse(getLcv.Rows[0]["NamTC"].ToString());


            DataTable rs = GetHeThongTKData(int.Parse(comboBoxEdit1.Text), int.Parse(comboBoxEdit2.Text));

            rs.DefaultView.Sort = "SoHieu ASC";
            rs = rs.DefaultView.ToTable();

            if (checkEdit1.Checked)
            {
                List<SoNhatKyChung> lstSonkChung = ConvertToList(Getbc1(int.Parse(comboBoxEdit1.Text), int.Parse(comboBoxEdit2.Text)))
                  .OrderBy(x => x.NgayGS)
                  .ThenBy(x => x.MaCT)
                  .ToList();
                string filePath = Path.Combine(pathluu, "SoNhatKyChung.xlsx");
                using (XLWorkbook workbook = new XLWorkbook())
                {
                    var sheet = workbook.Worksheets.Add("Sổ nhật ký chung");
                    // === DÒNG 1: TÊN CÔNG TY ===
                    sheet.Cell("A1").Value = "Tên đơn vị";
                    sheet.Cell("A1").Style.Font.FontSize = 10;

                    sheet.Cell("B1").Value = Helpers.ConvertVniToUnicode(getLcv.Rows[0]["TenCty"].ToString());
                    sheet.Cell("B1").Style.Font.FontSize = 10;
                    sheet.Cell("B1").Style.Font.Bold = true;
                    sheet.Range("B1:E1").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    //MST
                    sheet.Cell("A2").Value = $"MST:{getLcv.Rows[0]["MaSoThue"].ToString()}";
                    sheet.Cell("A2").Style.Font.FontSize = 10;
                    sheet.Cell("A2").Style.Font.Bold = true;
                    sheet.Range("A2:D2").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    //Mẫu số
                    sheet.Cell("G1").Value = "Mẫu số S03a-DNN";
                    sheet.Cell("G1").Style.Font.FontSize = 8;
                    sheet.Cell("G1").Style.Font.Bold = true;
                    sheet.Range("G1:H1").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    //Ban hành
                    sheet.Cell("G2").Value = "(Ban hành theo TT 133/2016/TT-BTC ngày 26/08/2016 của BTC)";
                    sheet.Cell("G2").Style.Font.FontSize = 8;
                    var range = sheet.Range("G2:H3");
                    range.Merge();
                    range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    range.Style.Alignment.WrapText = true;

                    //So nhat ky chung
                    sheet.Cell("A4").Value = "SỔ NHẬT KÝ CHUNG";
                    sheet.Cell("A4").Style.Font.FontSize = 14;
                    sheet.Cell("A4").Style.Font.Bold = true;
                    sheet.Cell("A4").Style.Font.FontColor = XLColor.FromArgb(31, 73, 125); ; // thêm màu xanh
                    range = sheet.Range("A4:H4");
                    range.Merge();
                    range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    //Ngày tháng
                    sheet.Cell("A5").Value = $"Từ tháng {comboBoxEdit1.Text}/{NamTC} đến tháng {comboBoxEdit2.Text}/{NamTC} ";
                    sheet.Cell("A5").Style.Font.FontSize = 12;
                    range = sheet.Range("A5:H5");
                    range.Merge();
                    range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    // === DÒNG 6: HEADER BẢNG ===
                    sheet.Cell("A7").Value = "TT";
                    sheet.Cell("A7").Style.Font.Bold = true;
                    sheet.Cell("A7").Style.Font.FontSize = 10;
                    range = sheet.Range("A7:A8");
                    range.Merge();
                    range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    //Ngày GS
                    sheet.Cell("B7").Value = "Ngày GS";
                    sheet.Cell("B7").Style.Font.Bold = true;
                    sheet.Cell("B7").Style.Font.FontSize = 12;
                    range = sheet.Range("B7:B8");
                    range.Merge();
                    range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    //Chứng từ
                    sheet.Cell("C7").Value = "Chứng từ";
                    sheet.Cell("C7").Style.Font.Bold = true;
                    sheet.Cell("C7").Style.Font.FontSize = 12;
                    sheet.Range("C7:D7").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    //Số CT
                    sheet.Cell("C8").Value = "Chứng từ";
                    sheet.Cell("C8").Style.Font.Bold = true;
                    sheet.Cell("C8").Style.Font.FontSize = 12;
                    //Ngày CT
                    sheet.Cell("D8").Value = "Ngày CT";
                    sheet.Cell("D8").Style.Font.Bold = true;
                    sheet.Cell("D8").Style.Font.FontSize = 12;
                    //Diễn giải
                    sheet.Cell("E7").Value = "Diễn giải";
                    sheet.Cell("E7").Style.Font.Bold = true;
                    sheet.Cell("E7").Style.Font.FontSize = 12;
                    range = sheet.Range("E7:E8");
                    range.Merge();
                    range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    //Tài khoản
                    sheet.Cell("F7").Value = "Tài khoản";
                    sheet.Cell("F7").Style.Font.Bold = true;
                    sheet.Cell("F7").Style.Font.FontSize = 12;
                    range = sheet.Range("F7:F8");
                    range.Merge();
                    range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    //Só tiền
                    sheet.Cell("G7").Value = "Số tiền";
                    sheet.Cell("G7").Style.Font.Bold = true;
                    sheet.Cell("G7").Style.Font.FontSize = 12;
                    range = sheet.Range("G7:H7");
                    range.Merge();
                    range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    //Phát sinh nợ
                    sheet.Cell("G8").Value = "Phát sinh nợ";
                    sheet.Cell("G8").Style.Font.Bold = true;
                    sheet.Cell("G8").Style.Font.FontSize = 12;
                    //Phát sinh có  
                    sheet.Cell("H8").Value = "Phát sinh có";
                    sheet.Cell("H8").Style.Font.Bold = true;
                    sheet.Cell("H8").Style.Font.FontSize = 12;
                    range = sheet.Range("A7:H8");

                    // Viền ngoài
                    range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    // Viền bên trong
                    range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //Bắt đầu từ ô A8
                    var listdata = Getbc1(int.Parse(comboBoxEdit1.Text), int.Parse(comboBoxEdit2.Text)).AsEnumerable().AsDataView().ToTable();
                    var rs2 = ConvertToList1(listdata)
            .OrderBy(m => m.NgayGS)
            .ThenBy(m => m.MaCT)
            .ToList();

                    var groupbylist = rs2
        .GroupBy(m => m.MaCT)
        .Select(g => new
        {
            MaCT = g.Key,
            Items = g.ToList()
        })
        .ToList();
                    int startrow = 9;
                    int endrow = 9;
                    int stt = 1;
                    int currentRow = sheet.LastRowUsed().RowNumber() + 1;
                    decimal totalPsNo = 0;
                    decimal totalPsCo = 0;
                    foreach (var group in groupbylist)
                    {
                        //Fill dòng đầu tiên

                        sheet.Cell(currentRow, 1).Value = stt; // Số thứ tự
                        sheet.Cell(currentRow, 2).Value = group.Items.FirstOrDefault()?.NgayGS.ToString("dd/MM/yyyy"); // Ngày GS 
                        sheet.Cell(currentRow, 3).Value = group.Items.FirstOrDefault()?.SoHieu; // Số hiệu chứng từ
                        sheet.Cell(currentRow, 4).Value = group.Items.FirstOrDefault()?.NgayCT.ToString("dd/MM/yyyy"); // Ngày CT
                        sheet.Cell(currentRow, 5).Value = Helpers.ConvertVniToUnicode(group.Items.FirstOrDefault()?.DienGiai); // Diễn giải
                                                                                                                               //Them cac dong con
                        range = sheet.Range($"A{currentRow}:H{currentRow}");
                        range.Style.Border.TopBorder = XLBorderStyleValues.Dashed;
                        range.Style.Border.BottomBorder = XLBorderStyleValues.Dashed;

                        foreach (var item in group.Items.OrderBy(m => m.LoaiPS))
                        {
                            currentRow += 1;
                            sheet.Cell(currentRow, 5).Value = Helpers.ConvertVniToUnicode(item.TenTK); // Tài khoản
                            sheet.Cell(currentRow, 6).Value = item.SoHieuTK; // Tên tài khoản
                            if (item.LoaiPS == -1) // Nợ
                            {
                                sheet.Cell(currentRow, 7).Value = item.SumOfSoPS.ToString("#,##0").Replace(",", "."); // Số tiền phát sinh nợ
                                totalPsNo += item.SumOfSoPS;
                            }
                            else if (item.LoaiPS == 1) // Có
                            {
                                sheet.Cell(currentRow, 8).Value = item.SumOfSoPS.ToString("#,##0").Replace(",", "."); // Số tiền phát sinh có
                                totalPsCo += item.SumOfSoPS;
                            }
                        }
                        endrow = currentRow;
                        stt++;
                        currentRow += 1;
                    }
                    
                    sheet.Cell(currentRow, 6).Value = $"Tổng phát sinh";
                    var cellNo = sheet.Cell(currentRow, 7);
                    cellNo.Value = totalPsNo.ToString("#,##0").Replace(",", ".");
                    cellNo.Style.Font.FontColor = XLColor.FromArgb(31, 73, 125); ; // màu xanh

                    var cellCo = sheet.Cell(currentRow, 8);
                    cellCo.Value = totalPsCo.ToString("#,##0").Replace(",", ".");
                    cellCo.Style.Font.FontColor = XLColor.FromArgb(31, 73, 125); ; // màu xanh
                                                                                   // Set border dọc (bên trái + bên phải của mỗi cell)
                                                                                   // viền trên + dưới nét đứt
                                                                                   //range.Style.Border.TopBorder = XLBorderStyleValues.Dashed;
                                                                                   //range.Style.Border.BottomBorder = XLBorderStyleValues.Dashed;

                    range = sheet.Range($"A{startrow}:H{endrow}");
                    // viền trái + phải nét liền (nếu cần)
                    range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    sheet.Range(startrow, 3, endrow, 3)
             .Style.Font.FontColor = XLColor.FromArgb(31, 73, 125); ;

                    sheet.Column(1).Width = 10;
                    sheet.Column(2).Width = 10;  // Đối ứng
                    sheet.Column(3).Width = 15;  // Đối ứng
                    sheet.Column(4).Width = 15;  // Phát sinh nợ
                    sheet.Column(5).Width = 50;  // Phát sinh có
                    sheet.Column(6).Width = 15;  // Phát sinh có
                    sheet.Column(7).Width = 15;  // Đối ứng
                    sheet.Column(8).Width = 15;  // Phát sinh nợ

                    currentRow += 2;
                    var now = DateTime.Now;
                    int lastDay = DateTime.DaysInMonth(NamTC, int.Parse(comboBoxEdit2.Text));

                    sheet.Cell(currentRow, 7).Value = $"Ngày {lastDay} tháng {comboBoxEdit2.Text} năm {NamTC}";
                    range = sheet.Range($"G{currentRow}:H{currentRow}");
                    range.Merge();
                    range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    currentRow += 1;
                    sheet.Cell(currentRow, 2).Value = "Người lập biểu";
                    sheet.Cell($"B{currentRow}").Style.Font.Bold = true;
                    sheet.Cell(currentRow, 4).Value = "Kế toán trưởng";
                    sheet.Cell($"D{currentRow}").Style.Font.Bold = true;

                    sheet.Cell(currentRow, 7).Value = "Giám đốc";
                    sheet.Cell($"G{currentRow}").Style.Font.Bold = true;
                    range = sheet.Range($"G{currentRow}:H{currentRow}");
                    range.Merge();
                    range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    currentRow += 1;
                    sheet.Cell(currentRow, 2).Value = "(Ký, họ tên)";
                    sheet.Cell(currentRow, 4).Value = "(Ký, họ tên)";
                    range = sheet.Range($"G{currentRow}:H{currentRow}");
                    sheet.Cell(currentRow, 7).Value = "(Ký, họ tên)";
                    range.Merge();
                    range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    try
                    {
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("File đang mở : " + ex.Message);
                        return; // Dừng quá trình nếu không thể xóa file cũ
                    }
                    workbook.SaveAs(filePath); 
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                     
                }

            }

            if (checkEdit2.Checked)
            {


                
                DataTable rsluyke = GetHeThongTKData(1, int.Parse(comboBoxEdit2.Text));

                //Lấy ra danh sach tài khoản có số dư cuối kỳ khác 0 hoặc phát sinh trong kỳ


                string tJoinLike = @"SELECT DISTINCTROW ChungTu.MaCT, ChungTu.ThangCT, ChungTu.SoHieu, ChungTu.NgayCT, ChungTu.NgayGS, ChungTu.DienGiai, ChungTu.SoPS, ChungTu.GhiChu, HeThongTK.SoHieu, HeThongTK_1.SoHieu, ChungTu.MaTKTCNo, ChungTu.MaTKTCCo, IIF(HethongTK.SoHieu LIKE '111%','0','1')+Cstr(10+ChungTu.ThangCT)+ChungTu.SoHieu AS SH1
FROM HeThongTK AS HeThongTK_3 RIGHT JOIN (HeThongTK AS HeThongTK_2 RIGHT JOIN (HeThongTK AS HeThongTK_1 RIGHT JOIN (HeThongTK RIGHT JOIN ChungTu ON HeThongTK.MaSo = ChungTu.MaTKTCNo) ON HeThongTK_1.MaSo = ChungTu.MaTKTCCo) ON HeThongTK_2.MaSo = ChungTu.MaTKNo) ON HeThongTK_3.MaSo = ChungTu.MaTKCo
WHERE SoPS<>0 AND ((HethongTK.SoHieu LIKE '111%') Or (HethongTK_1.SoHieu LIKE '111%')) And  (ThangCT>=1 AND ThangCT<=4)  AND (Chungtu.MaLoai<>4 OR (Chungtu.MaLoai=4 AND Chungtu.MaTKNo<>Chungtu.MaTkco))
ORDER BY ThangCT, ChungTu.NgayGS, IIF(HethongTK.SoHieu LIKE '111%','0','1')+Cstr(10+ChungTu.ThangCT)+ChungTu.SoHieu;
";





                string filePath = Path.Combine(pathluu, "QSocai.xlsx");

                using (XLWorkbook workbook = new XLWorkbook())
                {
                    // Sheet 1
                    foreach (DataRow item in rs.Rows)
                    {
                        if (!lstTk.Contains(item["SoHieu"].ToString()))
                            continue;
                        var item2 = rsluyke.Select($"SoHieu='{item["SoHieu"].ToString()}'").FirstOrDefault();
                        var sheet = workbook.Worksheets.Add(item["SoHieu"].ToString());

                        // === DÒNG 1: TÊN CÔNG TY === 

                        sheet.Cell("A1").Value = Helpers.ConvertVniToUnicode(getLcv.Rows[0]["TenCty"].ToString());
                        sheet.Cell("A1").Style.Font.FontSize = 10;
                        sheet.Cell("A1").Style.Font.Bold = true;
                        sheet.Range("A1:E1").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        //MST
                        sheet.Cell("A2").Value = $"MST:{getLcv.Rows[0]["MaSoThue"].ToString()}";
                        sheet.Cell("A2").Style.Font.FontSize = 10;
                        sheet.Cell("A2").Style.Font.Bold = true;
                        sheet.Range("A2:D2").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        // === DÒNG 3: TIÊU ĐỀ SỔ CÁI ===
                        sheet.Cell("A3").Value = "SỔ CÁI TÀI KHOẢN";
                        sheet.Cell("A3").Style.Font.Bold = true;
                        sheet.Cell("A3").Style.Font.FontSize = 16;
                        sheet.Cell("A3").Style.Font.FontColor =XLColor.FromArgb(31, 73, 125);;
                        sheet.Range("A3:E3").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        sheet.Cell("A4").Value = item["SoHieu"].ToString();
                        sheet.Cell("A4").Style.Font.FontSize = 12;
                        sheet.Range("A4:E4").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        sheet.Cell("A5").Value = $"Từ tháng {1} đến tháng {4}";
                        sheet.Cell("A5").Style.Font.FontSize = 12;
                        sheet.Range("A5:C5").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        string dkNo = Convert.ToString(item["DKNo"]);
                        string dkCo = Convert.ToString(item["DkCo"]);
                        string soDuDauKy = !string.IsNullOrEmpty(dkNo) ? dkNo : dkCo;
                        //.ToString("#,##0")
                        sheet.Cell("D5").Value = $"Số dư đầu kỳ ";
                        sheet.Cell("D5").Style.Font.FontSize = 12;
                        sheet.Cell("E5").Value = $"{double.Parse(soDuDauKy).ToString("#,##0").Replace(",", ".")} ";
                        sheet.Cell("E5").Style.Font.FontSize = 12;
                        sheet.Cell("E5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        // === DÒNG 6: HEADER BẢNG ===
                        sheet.Cell("A6").Value = "STT";
                        sheet.Cell("B6").Value = "Diễn giải";
                        sheet.Cell("C6").Value = "Đối ứng";
                        sheet.Cell("D6").Value = "Phát sinh nợ";
                        sheet.Cell("E6").Value = "Phát sinh có";

                        // Format header
                        var headerRange = sheet.Range("A6:E6");
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        // === DÒNG DATA ===
                        int currentRow = 7; // Bắt đầu từ dòng 7  
                                            //Lấy ra danh sách tk bên trái
                        var tkhoan = item["SoHieu"].ToString();
                        DataTable dt = GetChungTuData(int.Parse(comboBoxEdit1.Text), int.Parse(comboBoxEdit2.Text), tkhoan);
                        var result = ConvertDataTableToList(dt);
                        List<string> lstChung = new List<string>();
                        var lstLeft = result.Where(m => !m.SoHieuTK.StartsWith(tkhoan.ToString())).ToList();

                        // Group by theo 3 chữ số đầu của SoHieuTK
                        var groupedData = lstLeft
                            .GroupBy(m => m.SoHieuTK.Length >= 3 ? m.SoHieuTK.Substring(0, 3) : m.SoHieuTK)
                            .Select(g => new
                            {
                                SoHieuTK_Nhom = g.Key,
                                TongSoPS = g.Sum(m => m.SoPS),
                                SoLuong = g.Count(),
                                ChiTietTK = g.Select(x => x.SoHieuTK).Distinct().ToList()  // Các TK con
                            })
                            .OrderBy(x => x.SoHieuTK_Nhom)
                            .ToList();

                        // Hiển thị
                        foreach (var group in groupedData)
                        {
                            Console.WriteLine($"Nhóm TK: {group.SoHieuTK_Nhom}, Tổng PS: {group.TongSoPS:N0}");
                            Console.WriteLine($"  - Các TK con: {string.Join(", ", group.ChiTietTK)}");
                            if (group.SoHieuTK_Nhom != "" && !lstChung.Contains(group.SoHieuTK_Nhom))
                            {
                                lstChung.Add(group.SoHieuTK_Nhom);
                            }
                        }
                        //Bên right
                        var lstRight = result.Where(m => !m.SoHieuTK1.StartsWith(tkhoan.ToString())).ToList();

                        // Group by theo 3 chữ số đầu của SoHieuTK
                        var groupedData2 = lstRight
                            .GroupBy(m => m.SoHieuTK1.Length >= 3 ? m.SoHieuTK1.Substring(0, 3) : m.SoHieuTK1)
                            .Select(g => new
                            {
                                SoHieuTK_Nhom = g.Key,
                                TongSoPS = g.Sum(m => m.SoPS),
                                SoLuong = g.Count(),
                                ChiTietTK = g.Select(x => x.SoHieuTK1).Distinct().ToList()  // Các TK con
                            })
                            .OrderBy(x => x.SoHieuTK_Nhom)
                            .ToList();

                        // Hiển thị
                        foreach (var group in groupedData2)
                        {
                            Console.WriteLine($"Nhóm TK: {group.SoHieuTK_Nhom}, Tổng PS: {group.TongSoPS:N0}");
                            Console.WriteLine($"  - Các TK con: {string.Join(", ", group.ChiTietTK)}");
                            if (group.SoHieuTK_Nhom != "" && !lstChung.Contains(group.SoHieuTK_Nhom))
                            {
                                if (!group.SoHieuTK_Nhom.Contains(tkhoan))
                                    lstChung.Add(group.SoHieuTK_Nhom);
                            }
                        }
                        //Bên trái plus
                        var lstLeftPlus = result.Where(m => m.SoHieuTK.StartsWith(tkhoan.ToString()) && m.GhiChu.Contains(",")).ToList();
                        double tongpsleftplus = lstLeftPlus.Sum(m => m.SoPS);
                        if (lstLeftPlus != null && lstLeftPlus.Count > 0)
                        {
                            var groupedData3 = lstLeftPlus
                           .GroupBy(m => m.SoHieuTK.Length >= 3 ? m.SoHieuTK.Substring(0, 3) : m.SoHieuTK)
                           .Select(g => new
                           {
                               SoHieuTK_Nhom = g.Key,
                               TongSoPS = g.Sum(m => m.SoPS),
                               SoLuong = g.Count(),
                               ChiTietTK = g.Select(x => x.SoHieuTK).Distinct().ToList(),  // Các TK con,
                               Ghichu = g.Select(x => x.GhiChu).Distinct().FirstOrDefault()  // Các TK con,
                           })
                           .OrderBy(x => x.SoHieuTK_Nhom)
                           .ToList();
                            // Hiển thị
                            foreach (var group in groupedData3)
                            {
                                Console.WriteLine($"Nhóm TK: {group.SoHieuTK_Nhom}, Tổng PS: {group.TongSoPS:N0}");
                                Console.WriteLine($"  - Các TK con: {string.Join(", ", group.ChiTietTK)}");
                                if (group.SoHieuTK_Nhom != "" && !lstChung.Contains(group.SoHieuTK_Nhom))
                                {
                                    if (!group.Ghichu.StartsWith(tkhoan))
                                        lstChung.Add(group.Ghichu);
                                }
                            }
                        }

                        //Bên phải plus
                        var lstRightplus = result.Where(m => m.SoHieuTK1.StartsWith(tkhoan.ToString()) && m.GhiChu.Contains(",")).ToList();
                        double tongpsrightplus = lstRightplus.Sum(m => m.SoPS);
                        if (lstRightplus != null && lstRightplus.Count > 0)
                        {
                            var groupedData4 = lstRightplus
                           .GroupBy(m => m.SoHieuTK1.Length >= 3 ? m.SoHieuTK1.Substring(0, 3) : m.SoHieuTK1)
                           .Select(g => new
                           {
                               SoHieuTK_Nhom = g.Key,
                               TongSoPS = g.Sum(m => m.SoPS),
                               SoLuong = g.Count(),
                               ChiTietTK = g.Select(x => x.SoHieuTK1).Distinct().ToList(),  // Các TK con,
                               Ghichu = g.Select(x => x.GhiChu).Distinct().FirstOrDefault()  // Các TK con,
                           })
                           .OrderBy(x => x.SoHieuTK_Nhom)
                           .ToList();
                            // Hiển thị
                            foreach (var group in groupedData4)
                            {
                                Console.WriteLine($"Nhóm TK: {group.SoHieuTK_Nhom}, Tổng PS: {group.TongSoPS:N0}");
                                Console.WriteLine($"  - Các TK con: {string.Join(", ", group.ChiTietTK)}");
                                if (group.SoHieuTK_Nhom != "" && !lstChung.Contains(group.SoHieuTK_Nhom))
                                {
                                    if (!group.Ghichu.Contains(tkhoan))
                                        lstChung.Add(group.Ghichu);
                                }
                            }
                        }

                        lstChung = lstChung.OrderBy(m => m).ToList();

                        int stt = 1;
                        foreach (var it in lstChung)
                        {
                            sheet.Cell(currentRow, 1).Value = stt;
                            sheet.Cell(currentRow, 2).Value = $"Đối ứng theo tài khoản {it}";
                            sheet.Cell(currentRow, 3).Value = it;
                            var fleft = groupedData.Where(m => m.SoHieuTK_Nhom.StartsWith(it.ToString())).FirstOrDefault();
                            var fright = groupedData2.Where(m => m.SoHieuTK_Nhom.StartsWith(it.ToString())).FirstOrDefault();
                            if (fright != null)
                                sheet.Cell(currentRow, 4).Value = fright.TongSoPS.ToString("#,##0").Replace(",", ".");
                            if (fleft != null)
                            {
                                sheet.Cell(currentRow, 5).Value = fleft.TongSoPS.ToString("#,##0").Replace(",", "."); 
                            }
                            if (tongpsleftplus > 0 && it.Contains(","))
                            {
                                sheet.Cell(currentRow, 4).Value = tongpsleftplus.ToString("#,##0").Replace(",", ".");
                                
                            }
                            if (tongpsrightplus > 0 && it.Contains(","))
                            {
                                sheet.Cell(currentRow, 5).Value = tongpsrightplus.ToString("#,##0").Replace(",", ".");
                              
                            }
                            sheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            sheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            stt++;
                            currentRow++;
                        }
                        // Thêm border cho toàn bộ table (từ header đến hết data)
                        try
                        {
                            if (currentRow > 7) // Kiểm tra nếu có dữ liệu để áp dụng border     {
                            {
                                var tableRange = sheet.Range(7, 1, currentRow - 1, 5);
                                tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Lỗi khi áp dụng border: " + ex.Message);
                        }
                        var color = XLColor.FromArgb(31, 73, 125);

                        sheet.Cell(currentRow, 3).Value = "Tổng phát sinh";
                        sheet.Cell(currentRow, 4).Value = double.Parse(item["PsNo"].ToString()).ToString("#,##0").Replace(",", ".");
                        sheet.Cell(currentRow,4).Style.Font.Bold = true;
                        sheet.Cell(currentRow,5).Style.Font.Bold = true;
                        sheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        sheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        sheet.Cell(currentRow, 5).Value = double.Parse(item["PsCo"].ToString()).ToString("#,##0").Replace(",", ".");
                        sheet.Cell(currentRow, 4).Style.Font.FontColor = color;
                        sheet.Cell(currentRow, 5).Style.Font.FontColor = color;
                        sheet.Cell(currentRow + 1, 3).Value = "Phát sinh luỷ kế";
                        sheet.Cell(currentRow + 1, 4).Value = double.Parse(item2["PsNo"].ToString()).ToString("#,##0").Replace(",", ".");
                        sheet.Cell(currentRow + 1, 5).Value = double.Parse(item2["PsCo"].ToString()).ToString("#,##0").Replace(",", ".");
                        sheet.Cell(currentRow + 1, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        sheet.Cell(currentRow + 1, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        sheet.Cell(currentRow + 1, 4).Style.Font.FontColor = color;
                        sheet.Cell(currentRow + 1, 4).Style.Font.Bold = true;
                        sheet.Cell(currentRow + 1, 5).Style.Font.FontColor = color;
                        sheet.Cell(currentRow + 1, 5).Style.Font.Bold = true;
                        sheet.Cell(currentRow + 2, 3).Value = "Số dư cuối kỳ";
                        double ckNo = double.Parse(item2["CKNo"].ToString());
                        sheet.Cell(currentRow + 2, 4).Value = ckNo != 0 ? ckNo.ToString("#,##0").Replace(",", ".") : "";
                        double ckCo = double.Parse(item2["CKCo"].ToString());
                        sheet.Cell(currentRow + 2, 5).Value = ckCo != 0 ? ckCo.ToString("#,##0").Replace(",", ".") : "";
                        sheet.Cell(currentRow + 2, 4).Style.Font.FontColor = color;
                        sheet.Cell(currentRow + 2, 4).Style.Font.Bold = true;
                        sheet.Cell(currentRow+2, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        sheet.Cell(currentRow + 2, 5).Style.Font.FontColor = color;
                        sheet.Cell(currentRow + 2, 5).Style.Font.Bold = true;
                        sheet.Cell(currentRow+2, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        sheet.Column(1).Width = 5;   // STT
                                                     //sheet.Column(2).AdjustToContents(1, 200);
                        sheet.Column(2).Width = 40;  // Đối ứng
                        sheet.Column(3).Width = 15;  // Đối ứng
                        sheet.Column(4).Width = 15;  // Phát sinh nợ
                        sheet.Column(5).Width = 15;  // Phát sinh có
                                                     // Auto-fit cột
                                                     //sheet.Columns().AdjustToContents();
                        sheet.Column(2).Style.Alignment.WrapText = true;
                        sheet.Style.Font.FontName = "Times New Roman";

                        var now = DateTime.Now;
                        int lastDay = DateTime.DaysInMonth(NamTC, int.Parse(comboBoxEdit2.Text));
                        currentRow += 3;
                        sheet.Cell(currentRow, 3).Value = $"Ngày {lastDay} tháng {comboBoxEdit2.Text} năm {NamTC}";
                       var range = sheet.Range($"C{currentRow}:D{currentRow}");
                        range.Merge();
                        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        currentRow += 1;
                        sheet.Cell(currentRow, 2).Value = "Người lập biểu";
                        sheet.Cell($"B{currentRow}").Style.Font.Bold = true;
                        sheet.Cell($"B{currentRow}").Style.Font.Bold = true;

                        sheet.Cell(currentRow, 3).Value = "Giám đốc";
                        sheet.Cell($"C{currentRow}").Style.Font.Bold = true;
                        range = sheet.Range($"C{currentRow}:D{currentRow}");
                        range.Merge();
                        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    }

                    try
                    {
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("File đang mở : " + ex.Message);
                        return; // Dừng quá trình nếu không thể xóa file cũ
                    }
                    workbook.SaveAs(filePath);
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true }); 
                }  
            }
            if (checkEdit3.Checked)
            {


                //var getcttk = GetChungTu(connectionString, int.Parse(comboBoxEdit1.Text), int.Parse(comboBoxEdit2.Text), "111");
                string filePathw = Path.Combine(pathluu, "SochitietTK.xlsx");
                
                using (XLWorkbook workbook = new XLWorkbook())
                {
                    // Sheet 1
                    foreach (DataRow item in rs.Rows)
                    {
                        if (!lstTk.Contains(item["SoHieu"].ToString()))
                            continue;
                        if (item["SoHieu"].ToString() == "333")
                        {
                            int aa = 10;
                        }
                        var sheet = workbook.Worksheets.Add(item["SoHieu"].ToString());
                        sheet.Cell("A1").Value = "Tên đơn vị";
                        sheet.Cell("A1").Style.Font.FontSize = 10;

                        sheet.Cell("B1").Value = Helpers.ConvertVniToUnicode(getLcv.Rows[0]["TenCty"].ToString());
                        sheet.Cell("B1").Style.Font.FontSize = 10;
                        sheet.Cell("B1").Style.Font.Bold = true;
                        sheet.Range("B1:E1").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        //MST
                        sheet.Cell("A2").Value = $"MST:{getLcv.Rows[0]["MaSoThue"].ToString()}";
                        sheet.Cell("A2").Style.Font.FontSize = 10;
                        sheet.Cell("A2").Style.Font.Bold = true;
                        sheet.Range("A2:D2").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        //Mẫu số
                        sheet.Cell("F1").Value = "Mẫu số S03a-DNN";
                        sheet.Cell("F1").Style.Font.FontSize = 8;
                        sheet.Cell("F1").Style.Font.Bold = true;
                        sheet.Range("F1:G1").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        //Ban hành
                        sheet.Cell("F2").Value = "(Ban hành theo TT 133/2016/TT-BTC ngày 26/08/2016 của BTC)";
                        sheet.Cell("F2").Style.Font.FontSize = 8;
                        var range = sheet.Range("F2:G3");
                        range.Merge();
                        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        range.Style.Alignment.WrapText = true;

                        //So nhat ky chung
                        sheet.Cell("A4").Value = "SỔ CHI TIẾT TÀI KHOẢN";
                        sheet.Cell("A4").Style.Font.FontSize = 14;
                        sheet.Cell("A4").Style.Font.Bold = true;
                        sheet.Cell("A4").Style.Font.FontColor =XLColor.FromArgb(31, 73, 125);; // thêm màu xanh
                        range = sheet.Range("A4:H4");
                        range.Merge();
                        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //Dùng  cho
                        sheet.Cell("A5").Value = "(Dùng cho hình thức kế toán nhật ký chung)";
                        sheet.Cell("A5").Style.Font.FontSize = 11;
                        sheet.Cell("A5").Style.Font.Bold = true;
                        sheet.Cell("A5").Style.Font.FontColor =XLColor.FromArgb(31, 73, 125);; // thêm màu xanh
                        range = sheet.Range("A5:H5");
                        range.Merge();
                        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;


                        //Ngày tháng
                        sheet.Cell("A6").Value = $"Từ tháng {comboBoxEdit1.Text}/{NamTC} đến tháng {comboBoxEdit2.Text}/{NamTC} ";
                        sheet.Cell("A6").Style.Font.FontSize = 12;
                        range = sheet.Range("A6:H6");
                        range.Merge();
                        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //Ten tk
                        string qr = @"SELECT * FROM HeThongTK   WHERE SoHieu = ?  ";
                        var par = new OleDbParameter[]
                          {
    new OleDbParameter("?", item["SoHieu"].ToString()), // Ensure 'type' is correctly defined 
                          };
                        var tentk = ExecuteQuery(qr, par).Rows[0];
                        sheet.Cell("A7").Value = $"{tentk["Sohieu"].ToString()} - {Helpers.ConvertVniToUnicode(tentk["Ten"].ToString())}";
                        sheet.Cell("A7").Style.Font.FontSize = 12;
                        range = sheet.Range("A7:H7");
                        range.Merge();
                        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //Tieu đề cột
                        //Ngày GS
                        sheet.Cell("A8").Value = "Ngày GS";
                        sheet.Cell("A8").Style.Font.Bold = true;
                        sheet.Cell("A8").Style.Font.FontSize = 12;
                        range = sheet.Range("A8:A9");
                        range.Merge();
                        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //chung tu
                        sheet.Cell("B8").Value = "Chứng từ";
                        sheet.Cell("B8").Style.Font.Bold = true;
                        sheet.Cell("B8").Style.Font.FontSize = 12;
                        sheet.Range("B8:C8").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        //Số CT
                        sheet.Cell("B9").Value = "Số CT";
                        sheet.Cell("B9").Style.Font.Bold = true;
                        sheet.Cell("B9").Style.Font.FontSize = 12;
                        //Ngày CT
                        sheet.Cell("C9").Value = "Ngày CT";
                        sheet.Cell("C9").Style.Font.Bold = true;
                        sheet.Cell("C9").Style.Font.FontSize = 12;
                        var data = GetSoNhatKy(int.Parse(comboBoxEdit1.Text), int.Parse(comboBoxEdit2.Text), item["SoHieu"].ToString());
                        //Diễn giải
                        sheet.Cell("D8").Value = "Diễn giải";
                        sheet.Cell("D8").Style.Font.Bold = true;
                        sheet.Cell("D8").Style.Font.FontSize = 12;
                        range = sheet.Range("D8:D9");
                        range.Merge();
                        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //TK đối ứng
                        sheet.Cell("E8").Value = "TK đối ứng";
                        sheet.Cell("E8").Style.Font.Bold = true;
                        sheet.Cell("E8").Style.Font.FontSize = 12;
                        range = sheet.Range("E8:E9");
                        range.Merge();
                        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //Số tiền
                        sheet.Cell("F8").Value = "Số tiền";
                        sheet.Cell("F8").Style.Font.Bold = true;
                        sheet.Cell("F8").Style.Font.FontSize = 12;
                        range = sheet.Range("F8:G8");
                        range.Merge();
                        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //
                        sheet.Cell("F9").Value = "Nợ";
                        sheet.Cell("F9").Style.Font.Bold = true;
                        sheet.Cell("F9").Style.Font.FontSize = 12;
                        sheet.Cell("G9").Value = "Có";
                        sheet.Cell("G9").Style.Font.Bold = true;
                        sheet.Cell("G9").Style.Font.FontSize = 12;

                        range = sheet.Range("A8:G9");

                        // Viền ngoài
                        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        // Viền bên trong
                        range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        //Tính luỹ kế
                        //Tính luỹ kế theo dư nợ 

                        var dataLK = GetSoNhatKy(1, int.Parse(comboBoxEdit2.Text), item["SoHieu"].ToString());
                        var totalTPSNo = data.Where(m => m.TKNo.StartsWith(item["SoHieu"].ToString())).Sum(m => m.SoPS);
                        var totalTPSCo = data.Where(m => m.TKCo.StartsWith(item["SoHieu"].ToString())).Sum(m => m.SoPS);
                        var totaldot = data.Where(m => m.GhiChu.Contains(",")).Sum(m => m.SoPS);
                        // totalTPSCo+= totaldot;  
                        var totalLkNo = dataLK.Where(m => m.TKNo.StartsWith(item["SoHieu"].ToString())).Sum(m => m.SoPS);
                        var totalLKCo = dataLK.Where(m => m.TKCo.StartsWith(item["SoHieu"].ToString())).Sum(m => m.SoPS);

                        //Số dư đầu kỳ = Dư nợ đầu kỳ - Dư có đầu kỳ
                        //Lấy ra số dư cuối kỳ của tháng trước làm số dư đầu kỳ 
                        string queryCheckVatTu = @"SELECT * FROM HeThongTK 
           WHERE SoHieu = ?  ";
                        var parameterss = new OleDbParameter[]
                           {
    new OleDbParameter("?", item["SoHieu"].ToString()), // Ensure 'type' is correctly defined 
                           };

                        // Execute the query with the parameters 
                        decimal duno0 = decimal.Parse(ExecuteQuery(queryCheckVatTu, parameterss).Rows[0]["DuNo_0"].ToString());
                        parameterss = new OleDbParameter[]
                           {
    new OleDbParameter("?", item["SoHieu"].ToString()), // Ensure 'type' is correctly defined 
                           };

                        decimal duco0 = decimal.Parse(ExecuteQuery(queryCheckVatTu, parameterss).Rows[0]["DuCo_0"].ToString());
                        decimal sodu = 0;
                        //if (duno0 > duco0)
                        //{
                        //    sodu = duno0 - duco0;
                        //}
                        //else
                        //{
                        //    sodu = (duco0 - duno0);
                        //}
                        int loainoco = 0;
                        sodu = duno0 - duco0;
                        if (sodu > 0)
                        {
                            loainoco = 1;
                        }
                        else if (sodu < 0)
                        {
                           // sodu= sodu * (-1);  
                            loainoco = 2;
                        }
                        var datatahngtrc = GetSoNhatKy(1, int.Parse(comboBoxEdit1.Text) - 1, item["SoHieu"].ToString());
                        var totalTPSNotrc = datatahngtrc.Where(m => m.TKNo.StartsWith(item["SoHieu"].ToString())).Sum(m => m.SoPS);
                        var totalTPSCoTrc = datatahngtrc.Where(m => m.TKCo.StartsWith(item["SoHieu"].ToString())).Sum(m => m.SoPS);
                        decimal ducocuoiky = 0;
                        decimal dunocuiky = 0;

                        decimal sodudauky = 0;
                        if (totalTPSNotrc > totalTPSCoTrc)
                        {
                            sodudauky = sodu + totalTPSNotrc - totalTPSCoTrc;
                        }
                        else
                        {
                            sodudauky = sodu + totalTPSCoTrc - totalTPSNotrc;
                        }

                        //Số dư cuối kỳ = Số dư đầu kỳ + Phát sinh Nợ - Phát sinh Có
                        //if (totalTPSNo > totalTPSCo)
                        //{
                        //    dunocuiky = sodudauky + totalTPSNo - totalTPSCo;
                        //}
                        //else
                        //{
                        //    ducocuoiky = sodudauky + totalTPSCo - totalTPSNo;
                        //}
                        dunocuiky = sodudauky + totalTPSNo - totalTPSCo;
                        sheet.Cell("E10").Value = $"Số dư đầu kỳ : ";
                        sheet.Cell("E10").Style.Font.FontSize = 11;
                        sheet.Cell("E10").Style.Font.Bold = true; 
                        if(loainoco==1)
                        {
                            sheet.Cell("F10").Value = sodudauky.ToString("#,##0").Replace(",", ".");
                            sheet.Cell("F10").Style.Font.FontSize = 11;
                            sheet.Cell("F10").Style.Font.Bold = true;
                        }
                        else
                        {
                            sheet.Cell("G10").Value =  (sodudauky*-1).ToString("#,##0").Replace(",", ".");
                            sheet.Cell("G10").Style.Font.FontSize = 11;
                            sheet.Cell("G10").Style.Font.Bold = true;
                        }
                            //var soducuoiky = sodudauky + totalTPSNo - totalTPSCo;

                            var groupbylistbymonth = data.GroupBy(m => m.ThangCT)
                .Select(g => new
                {
                    MaCT = g.Key,
                    Items = g.ToList()
                })
                .ToList();

                        var groupbylist = data.GroupBy(m => m.MaCT)
            .Select(g => new
            {
                MaCT = g.Key,
                Items = g.ToList()
            })
            .ToList();
                        int startrow = 0;
                        int endrow = 9;
                        int stt = 1;
                        int currentRow = 0;
                        currentRow = 11;
                        startrow = currentRow;
                        foreach (var gm in groupbylistbymonth)
                        {
                            var newgroupbylist = groupbylist.Where(m => m.Items.Any(x => x.ThangCT == gm.MaCT)).ToList();
                            decimal totalcol7 = 0;
                            foreach (var group in newgroupbylist)
                            {
                                //if (group.Items.Count == 1)
                                //{
                                //    continue;
                                //}

                                //Lấy ra nhóm thuế trc
                                int loaithue = group.Items.Any(m => m.TKNo == "1331") ? 1 : 2;
                                //Lấy ra dòng thuế
                                List<SoNhatKy> getdongthue = new List<SoNhatKy>();
                                if (loaithue == 1)
                                {
                                    getdongthue = group.Items.Where(m => m.TKNo == "1331").ToList();
                                }
                                else
                                {
                                    getdongthue = group.Items.Where(m => m.TKCo == "33311").ToList();
                                }
                                if (getdongthue != null && getdongthue.Count > 0 && item["SoHieu"].ToString() != "333")
                                {
                                    sheet.Cell(currentRow, 1).Value = getdongthue.FirstOrDefault()?.NgayGS;
                                    sheet.Cell(currentRow, 2).Value = getdongthue.FirstOrDefault()?.SoHieu;
                                    sheet.Cell(currentRow, 3).Value = getdongthue.FirstOrDefault().NgayCT;
                                    sheet.Cell(currentRow, 4).Value = Helpers.ConvertVniToUnicode(getdongthue.FirstOrDefault().DienGiai);
                                    sheet.Cell(currentRow, 5).Value = loaithue == 1 ? "1331" : "33311";
                                    if (loaithue == 1)
                                    {
                                        sheet.Cell(currentRow, 7).Value = getdongthue.Sum(m => m.SoPS).ToString("#,##0").Replace(",", ".");
                                        totalcol7 += getdongthue.Sum(m => m.SoPS);
                                        currentRow += 1;
                                    }
                                    else
                                    {
                                        sheet.Cell(currentRow, 6).Value = getdongthue.Sum(m => m.SoPS).ToString("#,##0").Replace(",", ".");
                                        currentRow += 1;
                                    }
                                }

                                var getrowremain = group.Items.Where(m => m.TKNo != "1331" && ((m.TKCo != "33311" && item["SoHieu"].ToString() != "333") || (m.TKCo == "33311" && item["SoHieu"].ToString() == "333"))).ToList();
                                if (getrowremain != null && getrowremain.Count > 0)
                                {
                                    //Nhóm theo remain nữa
                                    var reminagroup = getrowremain.GroupBy(m => !m.TKNo.Contains(item["SoHieu"].ToString()) ? m.TKNo : m.TKCo).ToList()
                                        .Select(g => new
                                        {
                                            SoHieu = g.Key,
                                            Items = g.ToList()
                                        })
                                        .ToList();
                                    foreach (var itemremain in reminagroup)
                                    {
                                        sheet.Cell(currentRow, 1).Value = itemremain.Items.FirstOrDefault().NgayGS;
                                        sheet.Cell(currentRow, 2).Value = itemremain.Items.FirstOrDefault().SoHieu;
                                        sheet.Cell(currentRow, 3).Value = itemremain.Items.FirstOrDefault().NgayCT;
                                        sheet.Cell(currentRow, 4).Value = Helpers.ConvertVniToUnicode(itemremain.Items.FirstOrDefault().DienGiai);
                                        sheet.Cell(currentRow, 5).Value = !itemremain.Items.Any(m => m.TKNo.StartsWith(item["SoHieu"].ToString())) ? itemremain.Items.FirstOrDefault().TKNo : itemremain.Items.FirstOrDefault().TKCo;
                                        if (!itemremain.Items.Any(m => m.TKNo.StartsWith(item["SoHieu"].ToString())))
                                        {
                                            sheet.Cell(currentRow, 7).Value = itemremain.Items.Sum(m => m.SoPS).ToString("#,##0").Replace(",", ".");
                                            totalcol7 += itemremain.Items.Sum(m => m.SoPS);
                                            currentRow += 1;
                                        }
                                        else
                                        {
                                            sheet.Cell(currentRow, 6).Value = itemremain.Items.Sum(m => m.SoPS).ToString("#,##0").Replace(",", ".");
                                            currentRow += 1;
                                        }


                                    }

                                }
                            }
                            sheet.Cell(currentRow, 1).Value = "Tổng phát sinh  tháng " + gm.MaCT;
                            sheet.Cell($"A{currentRow}").Style.Font.Bold = true;
                            sheet.Cell($"A{currentRow}").Style.Font.FontSize = 11;
                            sheet.Range($"A{currentRow}:E{currentRow}").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            sheet.Cell(currentRow, 6).Value = gm.Items.Where(m => m.TKNo.StartsWith(item["SoHieu"].ToString())).Sum(m => m.SoPS).ToString("#,##0").Replace(",", ".");
                            sheet.Cell(currentRow, 7).Value = totalcol7.ToString("#,##0").Replace(",", ".");
                            currentRow += 1;

                        }

                        sheet.Cell(currentRow, 1).Value = "Tổng phát sinh";
                        sheet.Cell($"A{currentRow}").Style.Font.Bold = true;
                        sheet.Cell($"A{currentRow}").Style.Font.FontSize = 11;
                        sheet.Range($"A{currentRow}:E{currentRow}").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        //Tong ps nợ 
                        sheet.Cell($"F{currentRow}").Style.Font.Bold = true;
                        sheet.Cell($"F{currentRow}").Style.Font.FontSize = 11;
                        sheet.Cell(currentRow, 6).Value = totalTPSNo.ToString("#,##0").Replace(",", ".");
                        //Tong ps có 
                        sheet.Cell($"G{currentRow}").Style.Font.Bold = true;
                        sheet.Cell($"G{currentRow}").Style.Font.FontSize = 11;
                        sheet.Cell(currentRow, 7).Value = totalTPSCo.ToString("#,##0").Replace(",", ".");
                        //Dòng 2
                        currentRow += 1;
                        sheet.Cell(currentRow, 1).Value = "Phát sinh luỹ kế";
                        sheet.Cell($"A{currentRow}").Style.Font.Bold = true;
                        sheet.Cell($"A{currentRow}").Style.Font.FontSize = 11;
                        sheet.Range($"A{currentRow}:E{currentRow}").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        //Tong ps nợ 
                        sheet.Cell($"F{currentRow}").Style.Font.Bold = true;
                        sheet.Cell($"F{currentRow}").Style.Font.FontSize = 11;
                        sheet.Cell(currentRow, 6).Value = totalLkNo.ToString("#,##0").Replace(",", ".");
                        //Tong ps có 
                        sheet.Cell($"G{currentRow}").Style.Font.Bold = true;
                        sheet.Cell($"G{currentRow}").Style.Font.FontSize = 11;
                        sheet.Cell(currentRow, 7).Value = totalLKCo.ToString("#,##0").Replace(",", ".");
                        //Dòng 3
                        currentRow += 1;
                        sheet.Cell(currentRow, 1).Value = "Số dư cuối kỳ";
                        sheet.Cell($"A{currentRow}").Style.Font.Bold = true;
                        sheet.Cell($"A{currentRow}").Style.Font.FontSize = 11;
                        sheet.Range($"A{currentRow}:E{currentRow}").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        //Tong ps nợ 
                        sheet.Cell($"F{currentRow}").Style.Font.Bold = true;
                        sheet.Cell($"F{currentRow}").Style.Font.FontSize = 11;
                        sheet.Cell($"G{currentRow}").Style.Font.Bold = true;
                        sheet.Cell($"G{currentRow}").Style.Font.FontSize = 11;
                        if (dunocuiky > 0)
                        {
                            sheet.Cell(currentRow, 6).Value = dunocuiky.ToString("#,##0").Replace(",", ".");
                        }
                        else
                        {
                            if (dunocuiky != 0)
                                sheet.Cell(currentRow, 7).Value = (dunocuiky * (-1)).ToString("#,##0").Replace(",", ".");
                            else
                                sheet.Cell(currentRow, 7).Value = ducocuoiky.ToString("#,##0").Replace(",", ".");
                        }
                        sheet.Cell($"A{currentRow}").Style.Font.Bold = true;
                        range = sheet.Range($"A{startrow}:G{currentRow}");
                        // viền trái + phải nét liền (nếu cần)
                        range.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        range.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        range.Style.Border.TopBorder = XLBorderStyleValues.Dashed;
                        range.Style.Border.BottomBorder = XLBorderStyleValues.Dashed;
                        sheet.Column(1).Width = 8;   // STT
                        sheet.Column(2).Width = 15;  // Đối ứng
                        sheet.Column(3).Width = 15;  // Đối ứng
                        sheet.Column(4).Width = 25;  // Phát sinh nợ
                        sheet.Column(5).Width = 10;  // Phát sinh có
                        sheet.Column(6).Width = 15;  // Phát sinh có
                        sheet.Column(7).Width = 15;  // Phát sinh có
                        sheet.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        sheet.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        currentRow += 2;
                        var now = DateTime.Now;
                        int lastDay = DateTime.DaysInMonth(NamTC, int.Parse(comboBoxEdit2.Text));

                        sheet.Cell(currentRow, 6).Value = $"Ngày {lastDay} tháng {comboBoxEdit2.Text} năm {NamTC}";
                        range = sheet.Range($"F{currentRow}:G{currentRow}");
                        range.Merge();
                        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        currentRow += 1;
                        sheet.Cell(currentRow, 2).Value = "Người lập biểu";
                        sheet.Cell($"B{currentRow}").Style.Font.Bold = true;
                        sheet.Cell(currentRow, 4).Value = "Kế toán trưởng";
                        sheet.Cell($"D{currentRow}").Style.Font.Bold = true;

                        sheet.Cell(currentRow, 6).Value = "Giám đốc";
                        sheet.Cell($"F{currentRow}").Style.Font.Bold = true;
                        range = sheet.Range($"F{currentRow}:G{currentRow}");
                        range.Merge();
                        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        currentRow += 1;
                        sheet.Cell(currentRow, 2).Value = "(Ký, họ tên)";
                        sheet.Cell(currentRow, 4).Value = "(Ký, họ tên)";
                        range = sheet.Range($"F{currentRow}:G{currentRow}");
                        sheet.Cell(currentRow, 6).Value = "(Ký, họ tên)";
                        range.Merge();
                        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    }

                    workbook.SaveAs(filePathw);
                    try
                    {
                        if (File.Exists(filePathw))
                        {
                            File.Delete(filePathw);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("File đang mở : " + ex.Message);
                        return; // Dừng quá trình nếu không thể xóa file cũ
                    }
                    workbook.SaveAs(filePathw);
                    Process.Start(new ProcessStartInfo(filePathw) { UseShellExecute = true });
                }
            }
           Application.Exit();
        }


        public class SoNhatKy
        {
            public string MaCT { get; set; }
            public int ThangCT { get; set; }
            public string SoHieu { get; set; }
            public DateTime? NgayCT { get; set; }
            public DateTime? NgayGS { get; set; }
            public string DienGiai { get; set; }
            public decimal SoPS { get; set; }
            public string GhiChu { get; set; }
            public string TKNo { get; set; }
            public string TKCo { get; set; }
            public string SH1 { get; set; }
        }
        public List<SoNhatKy> GetSoNhatKy(int tuThang, int denThang, string taiKhoan)
        {
            var list = new List<SoNhatKy>();

            string sql = $@"
SELECT 
    ChungTu.MaCT, 
    ChungTu.ThangCT, 
    ChungTu.SoHieu, 
    ChungTu.NgayCT, 
    ChungTu.NgayGS, 
    ChungTu.DienGiai, 
    ChungTu.SoPS, 
    ChungTu.GhiChu, 
    HeThongTK.SoHieu AS TKNo, 
    HeThongTK_1.SoHieu AS TKCo, 
    IIF(HethongTK.SoHieu LIKE '{taiKhoan}*','0','1')
        + Cstr(10+ChungTu.ThangCT)
        + ChungTu.SoHieu AS SH1
FROM HeThongTK AS HeThongTK_3 
RIGHT JOIN (HeThongTK AS HeThongTK_2 
RIGHT JOIN (HeThongTK AS HeThongTK_1 
RIGHT JOIN (HeThongTK 
RIGHT JOIN ChungTu ON HeThongTK.MaSo = ChungTu.MaTKTCNo) 
ON HeThongTK_1.MaSo = ChungTu.MaTKTCCo) 
ON HeThongTK_2.MaSo = ChungTu.MaTKNo) 
ON HeThongTK_3.MaSo = ChungTu.MaTKCo
WHERE 
    SoPS<>0 
    AND (
        (HethongTK.SoHieu IS NOT NULL AND HethongTK.SoHieu LIKE '{taiKhoan}%')
        OR 
        (HethongTK_1.SoHieu IS NOT NULL AND HethongTK_1.SoHieu LIKE '{taiKhoan}%')
    )
    AND (ThangCT >= {tuThang} AND ThangCT <= {denThang})
    AND (
        Chungtu.MaLoai<>4 
        OR (Chungtu.MaLoai=4 AND Chungtu.MaTKNo<>Chungtu.MaTkco)
    )
ORDER BY 
    ThangCT, 
    ChungTu.NgayGS, 
    IIF(HethongTK.SoHieu LIKE '{taiKhoan}*','0','1')
        + Cstr(10+ChungTu.ThangCT)
        + ChungTu.SoHieu";

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();

                using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                using (OleDbDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new SoNhatKy
                        {
                            MaCT = rd["MaCT"]?.ToString(),
                            ThangCT = Convert.ToInt32(rd["ThangCT"]),
                            SoHieu = rd["SoHieu"]?.ToString(),
                            NgayCT = rd["NgayCT"] as DateTime?,
                            NgayGS = rd["NgayGS"] as DateTime?,
                            DienGiai = rd["DienGiai"]?.ToString(),
                            SoPS = Convert.ToDecimal(rd["SoPS"]),
                            GhiChu = rd["GhiChu"]?.ToString(),
                            TKNo = rd["TKNo"]?.ToString(),
                            TKCo = rd["TKCo"]?.ToString(),
                            SH1 = rd["SH1"]?.ToString()
                        });
                    }
                }
            }

            return list;
        }
        //Tuc la so chi tiet tk
        public class ChungTuData
        {
            public string MaCT { get; set; }
            public int ThangCT { get; set; }
            public string SoHieu { get; set; }
            public DateTime NgayCT { get; set; }
            public DateTime NgayGS { get; set; }
            public string DienGiai { get; set; }
            public decimal SoPS { get; set; }
            public string GhiChu { get; set; }
            public string SH1 { get; set; }
        }
        public List<ChungTuData> GetChungTu(string connectionString, int tuThang, int denThang, string soTK)
        {
            List<ChungTuData> result = new List<ChungTuData>();

            // Bước 1: Kiểm tra kết nối và dữ liệu thô
            string query = @"
        SELECT 
            c.MaCT, c.ThangCT, c.SoHieu, c.NgayCT, c.NgayGS, 
            c.DienGiai, c.SoPS, c.GhiChu, c.MaLoai, c.MaTKNo, c.MaTKCo,
            tk1.SoHieu AS SoHieuTK,
            tk2.SoHieu AS SoHieuTK1
        FROM (ChungTu c
        LEFT JOIN HeThongTK tk1 ON tk1.MaSo = c.MaTKTCNo)
        LEFT JOIN HeThongTK tk2 ON tk2.MaSo = c.MaTKTCCo";

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();

                // Kiểm tra tổng số bản ghi trong ChungTu
                using (OleDbCommand cmdCount = new OleDbCommand("SELECT COUNT(*) FROM ChungTu", conn))
                {
                    int totalRecords = Convert.ToInt32(cmdCount.ExecuteScalar());
                    Console.WriteLine($"Tổng số bản ghi trong ChungTu: {totalRecords}");
                }

                // Kiểm tra từng điều kiện WHERE
                Console.WriteLine("\n=== KIỂM TRA ĐIỀU KIỆN ===");

                // 1. Kiểm tra SoPS <> 0
                using (OleDbCommand cmd = new OleDbCommand("SELECT COUNT(*) FROM ChungTu WHERE SoPS <> 0", conn))
                {
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    Console.WriteLine($"SoPS <> 0: {count} records");
                }

                // 2. Kiểm tra điều kiện tháng
                using (OleDbCommand cmd = new OleDbCommand($"SELECT COUNT(*) FROM ChungTu WHERE ThangCT BETWEEN {tuThang} AND {denThang}", conn))
                {
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    Console.WriteLine($"ThangCT BETWEEN {tuThang} AND {denThang}: {count} records");
                }

                // 3. Kiểm tra điều kiện MaLoai
                using (OleDbCommand cmd = new OleDbCommand(@"SELECT COUNT(*) FROM ChungTu 
            WHERE (MaLoai <> 4 OR (MaLoai = 4 AND MaTKNo <> MaTKCo))", conn))
                {
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    Console.WriteLine($"Điều kiện MaLoai: {count} records");
                }

                // 4. Kiểm tra JOIN với HeThongTK
                using (OleDbCommand cmd = new OleDbCommand(@"
            SELECT COUNT(*) 
            FROM (ChungTu c
            LEFT JOIN HeThongTK tk1 ON tk1.MaSo = c.MaTKTCNo)
            LEFT JOIN HeThongTK tk2 ON tk2.MaSo = c.MaTKTCCo
            WHERE tk1.SoHieu LIKE @tk OR tk2.SoHieu LIKE @tk", conn))
                {
                    cmd.Parameters.AddWithValue("@tk", soTK + "%");
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    Console.WriteLine($"Điều kiện tk LIKE '{soTK}%': {count} records");
                }

                // 5. Chạy truy vấn đầy đủ
                string fullQuery = query + @"
            WHERE c.SoPS <> 0 
                AND (tk1.SoHieu LIKE ? OR tk2.SoHieu LIKE ?)
                AND c.ThangCT BETWEEN ? AND ?
                AND (c.MaLoai <> 4 OR (c.MaLoai = 4 AND c.MaTKNo <> c.MaTKCo))";

                using (OleDbCommand cmd = new OleDbCommand(fullQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@p1", soTK + "*");
                    cmd.Parameters.AddWithValue("@p2", soTK + "*");
                    cmd.Parameters.AddWithValue("@p3", tuThang);
                    cmd.Parameters.AddWithValue("@p4", denThang);

                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string soHieuTK = reader["SoHieuTK"]?.ToString() ?? "";
                            string prefix = soHieuTK.StartsWith(soTK) ? "0" : "1";
                            string thangValue = (10 + Convert.ToInt32(reader["ThangCT"])).ToString();
                            string sh1 = prefix + thangValue + reader["SoHieu"].ToString();

                            result.Add(new ChungTuData
                            {
                                MaCT = reader["MaCT"].ToString(),
                                ThangCT = Convert.ToInt32(reader["ThangCT"]),
                                SoHieu = reader["SoHieu"].ToString(),
                                NgayCT = Convert.ToDateTime(reader["NgayCT"]),
                                NgayGS = Convert.ToDateTime(reader["NgayGS"]),
                                DienGiai = reader["DienGiai"].ToString(),
                                SoPS = Convert.ToDecimal(reader["SoPS"]),
                                GhiChu = reader["GhiChu"].ToString(),
                                SH1 = sh1
                            });
                        }
                    }
                }

                Console.WriteLine($"\n=== KẾT QUẢ CUỐI CÙNG: {result.Count} records ===");
            }

            return result;
        }
        private void comboBoxEdit1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxEdit2.Text = comboBoxEdit1.Text;    
        }
        public static List<string> lstTk=new List<string>();
        string pathluu { get; set; }    
        private void Form3_Load(object sender, EventArgs e)
        {
           
            // Clear trước nếu cần
            comboBoxEdit1.Properties.Items.Clear();

            // Thêm từ tháng 1 -> 12
            for (int i = 1; i <= 12; i++)
            {
                comboBoxEdit1.Properties.Items.Add(i);
            }

            // Mặc định chọn tháng 1
            comboBoxEdit1.SelectedIndex = 0;

            // Clear trước nếu cần
            comboBoxEdit2.Properties.Items.Clear();

            // Thêm từ tháng 1 -> 12
            for (int i = 1; i <= 12; i++)
            {
                comboBoxEdit2.Properties.Items.Add(i);
            }

            // Mặc định chọn tháng 1
            comboBoxEdit2.SelectedIndex = 0;

            string dbPath = "";
            string password = "1@35^7*9)1";
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);

            // Xóa phần \bin\Debug để lấy đường dẫn gốc
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));
           
            // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
            string filePaths = Path.Combine(rootDirectory, "Hoadon", "dpPath.txt");
            string pathThumuc = Path.Combine(rootDirectory);
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
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";

            //Đọc file txt
            string filePath = Path.Combine(rootDirectory, "Hoadon", "export.txt");
            try
            {
                string content = File.ReadAllText(filePath);
                var getslitContent = content.Split('_');
                if (getslitContent[0] == "1")
                {
                    checkEdit1.Checked = true;
                }
                if (getslitContent[1] == "1")
                {
                    checkEdit2.Checked = true;
                }
                if (getslitContent[2] == "1")
                {
                    checkEdit3.Checked = true; 
                }
                //Lay ngay thang
                var getsplitdate= getslitContent[3].Split('/');
                comboBoxEdit1.Text = getsplitdate[0];
                getsplitdate = getslitContent[4].Split('/');
                comboBoxEdit2.Text = getsplitdate[0];

                //Lay danh sach tk  
                if(getslitContent.Length > 5)
                {
                    var getlisttk = getslitContent[5].Split('|');
                    lstTk = getlisttk.ToList();
                }
                  
                string query = "SELECT * FROM tbRegister";
                var kq = ExecuteQuery(query, null);
                query = "SELECT * FROM License";
                var kq2 = ExecuteQuery(query, null);
                try
                {
                    if (kq.Rows.Count > 0)
                    {
                        pathluu = kq.Rows[0]["Hoadonpath"].ToString();
                        pathluu = Directory.GetParent(pathluu).FullName;
                        pathluu = Path.Combine(pathluu, $"Tailieu\\Soketoan{kq2.Rows[0]["NamTC"].ToString()}");

                        //Lấy nam taichinh
                    }
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show(ex.Message);
                }

                btnExoprt.PerformClick();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi đọc file: " + ex.Message);
            }


         
            // Tạo mảng tham số với giá trị cho câu lệnh SQL

           

        }
    }
}
    