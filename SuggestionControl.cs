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
using SaovietTax.DTO;

namespace SaovietTax
{
    public partial class SuggestionControl : UserControl
    {
        public event EventHandler<string> ItemSelected;
        public int type = 0;
        public List<Vattugoiy> SuggestionsVT
        {
            set
            {
                listBox1.Items.Clear();
                listView1.Items.Clear();
                foreach (var item in value)
                {
                    listBox1.Items.Add(item.SoHieu + "-" + Helpers.ConvertVniToUnicode(item.Ten)); // Hiển thị tên
                    listView1.Items.Add(new ListViewItem(new[] { item.SoHieu + " | " + item.Percent + "%"+" "+item.DonVi, Helpers.ConvertVniToUnicode(item.Ten)  }));
                }
            }
        }
        public List<HeThongTK> Suggestions
        {
            set
            {
                listBox1.Items.Clear();
                listView1.Items.Clear();
                foreach (var item in value)
                {
                    listBox1.Items.Add(item.SoHieu + "-" + Helpers.ConvertVniToUnicode(item.Ten)); // Hiển thị tên
                    listView1.Items.Add(new ListViewItem(new[] { item.SoHieu, Helpers.ConvertVniToUnicode(item.Ten) }));
                }
            }
        }

        public SuggestionControl()
        {
            InitializeComponent();
            this.BringToFront();
            listBox1.DoubleClick += ListBox1_DoubleClick;

            listView1.View = View.Details;
            listView1.Columns.Add("Số hiệu", 100);
            listView1.Columns.Add("Tên", 350);

            int totalWidth = listView1.ClientSize.Width; // Lấy chiều rộng của ListView

            // Tính toán chiều rộng của các cột theo tỷ lệ phần trăm
            listView1.Columns[0].Width = (int)(totalWidth * 0.3); // 15%
            listView1.Columns[1].Width = (int)(totalWidth * 0.525); // 35%
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.OwnerDraw = true;
        }

        private void ListBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                // Lấy SoHieu từ mục đã chọn
                string selectedItem = listBox1.SelectedItem.ToString();
                string soHieu = selectedItem.Split('-')[0].Trim(); // Tách SoHieu ra

                ItemSelected?.Invoke(this, soHieu); // Gửi SoHieu
                Hide();
            }
        }

        public void UpdateSuggestions(List<HeThongTK> newSuggestions)
        {
            type = 1;
            Suggestions = newSuggestions;

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
        public void UpdateSuggestionsVT(List<Vattugoiy> newSuggestions)
        {
            type = 2;
            SuggestionsVT = newSuggestions;

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
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listView1.SelectedItems[0];

                // Lấy dữ liệu từ hàng đã chọn
                string soHieu = selectedItem.SubItems[0].Text;
                string tenVatTu = selectedItem.SubItems[1].Text; 
                if(type==1)
                ItemSelected?.Invoke(this, soHieu); // Gửi SoHieu
                else
                    ItemSelected?.Invoke(this, soHieu+"|"+tenVatTu); // Gửi SoHieu
                Hide();
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
            this.Hide(); // Ẩn UserControl khi nút được nhấn    
        }

        private void SuggestionControl_Load(object sender, EventArgs e)
        {

        }
    }

}