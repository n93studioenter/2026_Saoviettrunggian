using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SaovietTax.frmMain;

namespace SaovietTax
{
    public partial class HanghoaControl: UserControl
    {
        public event EventHandler<string> ItemSelected;

        public List<HangHoaTK> Suggestions
        {
            set
            {
                listBox1.Items.Clear();
                listView1.Items.Clear();
                foreach (var item in value)
                {
                    listBox1.Items.Add(item.SoHieu + "|" + Helpers.ConvertVniToUnicode(item.TenVattu)+"|"+item.Dongia+"|"+item.SoLuong+"|"+item.ThanhTien); // Hiển thị tên

                    listView1.Items.Add(new ListViewItem(new[] { item.SoHieu, item.TenVattu, item.Dongia.ToString("N0"), item.SoLuong.ToString("N0"), item.ThanhTien.ToString("N0"),Helpers.ConvertVniToUnicode(item.MaPhanLoai.ToString()) }));
                }
            }
        }
        public DataTable Suggestions2
        {
            set
            {
                listView1.Items.Clear();
                foreach (DataRow r in value.Rows)
                {
                    listView1.Items.Add(new ListViewItem(new[] { r["MaSo"].ToString(),r["SoHieu"].ToString(),Helpers.ConvertVniToUnicode(r["TenPhanLoai"].ToString()) }));
                }
            }
        }
        public HanghoaControl()
        {
            InitializeComponent(); 
            this.BringToFront();
            // listBox1.DrawMode = DrawMode.OwnerDrawFixed; // Hoặc OwnerDrawVariable

            listBox1.DoubleClick += ListBox1_DoubleClick;
            listView1.Columns.Add("Số hiệu",100);
            listView1.Columns.Add("Tên",350);
            listView1.Columns.Add("Đơn giá", 150);
            listView1.Columns.Add("Số lượng", 100);
            listView1.Columns.Add("Thành tiền", 120);
            listView1.Columns.Add("Tên phân loại",300);
            int totalWidth = listView1.ClientSize.Width; // Lấy chiều rộng của ListView

            // Tính toán chiều rộng của các cột theo tỷ lệ phần trăm
            listView1.Columns[0].Width = (int)(totalWidth * 0.1); // 15%
            listView1.Columns[1].Width = (int)(totalWidth * 0.31); // 35%
            listView1.Columns[2].Width = (int)(totalWidth * 0.09); // 20%
            listView1.Columns[3].Width = (int)(totalWidth * 0.09); // 10%
            listView1.Columns[4].Width = (int)(totalWidth * 0.09); // 10%
            listView1.Columns[5].Width = (int)(totalWidth * 0.17); // 10%
            // Thiết lập chế độ hiển thị
            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.OwnerDraw = true;

            // Thêm một số mục mẫu nếu cần

        }

        private void ListBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                // Lấy SoHieu từ mục đã chọn
                string selectedItem = listBox1.SelectedItem.ToString();
                string soHieu = selectedItem.Split('|')[0].Trim(); // Tách SoHieu ra
                string tenVatTu = selectedItem.Split('|')[1].Trim(); // Tách tên vật tư ra
                ItemSelected?.Invoke(this, soHieu); // Gửi SoHieu
                Hide();
            }
        }
        int type = 0;
        public void UpdateSuggestions(List<HangHoaTK> newSuggestions)
        {
            Suggestions = newSuggestions;
            type = 1;
            lblTieude.Text = "Danh sách hàng hóa";
            // Hiển thị UserControl nếu có gợi ý
            if (listBox1.Items.Count > 0)
            {
                this.Show();
            }
            else
            {
                this.Hide();
            }
        }
        public void UpdateSuggestions2(DataTable newSuggestions)
        {
            Suggestions2 = newSuggestions;
            type = 2;
            lblTieude.Text = "Danh sách nhóm hàng hóa";
            // Hiển thị UserControl nếu có gợi ý
            if (listBox1.Items.Count > 0)
            {
                this.Show();
            }
            else
            {
                this.Hide();
            }
        }
        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            //if (e.Index < 0) return;
            //string itemText = listBox1.Items[e.Index].ToString();
            //string[] parts = itemText.Split('|');
            //e.DrawBackground();

            //// Vẽ phần đầu tiên với màu xanh
            //e.Graphics.DrawString(parts[0], new Font(e.Font, FontStyle.Regular), Brushes.Blue, e.Bounds);

            //// Vẽ phần thứ hai với màu đỏ
            //if (parts.Length > 1)
            //{
            //    float textWidth = e.Graphics.MeasureString(parts[0], e.Font).Width;
            //    e.Graphics.DrawString(parts[1], e.Font, Brushes.Black, e.Bounds.X + textWidth, e.Bounds.Y);
            //}

            //// Kết thúc vẽ
            //e.DrawFocusRectangle();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (type == 1)
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    ListViewItem selectedItem = listView1.SelectedItems[0];

                    // Lấy dữ liệu từ hàng đã chọn
                    string soHieu = selectedItem.SubItems[0].Text;
                    string tenVatTu = selectedItem.SubItems[1].Text;
                    string donGia = selectedItem.SubItems[2].Text;
                    string soLuong = selectedItem.SubItems[3].Text;
                    string thanhTien = selectedItem.SubItems[4].Text;
                    ItemSelected?.Invoke(this, soHieu); // Gửi SoHieu
                    Hide();
                }
            }
            else
            { 
                if (listView1.SelectedItems.Count > 0)
                {
                    ListViewItem selectedItem = listView1.SelectedItems[0]; 
                    string maso= selectedItem.SubItems[0].Text;
                    ItemSelected?.Invoke(this, maso); // Gửi SoHieu
                    Hide();
                }
            }
        }

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.GreenYellow, e.Bounds);

            // Tạo phông chữ đậm
            using (Font boldFont = new Font(e.Font, FontStyle.Bold))
            {
                // Vẽ văn bản với phông chữ đậm
                e.Graphics.DrawString(e.Header.Text, boldFont, Brushes.Black, e.Bounds);
            }
        }

        private void listView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true; // Vẽ mặc định để hiển thị văn bản

        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
