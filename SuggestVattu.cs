using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SaovietTax.frmMain;

namespace SaovietTax
{
    public partial class SuggestVattu : UserControl
    {
        public event EventHandler<string> ItemSelected;
        public List<Vattugoiy> SuggestionsVT
        {
            set
            { 
                //foreach (var item in value)
                //{
                    
                //}
                gridControl1.DataSource= value; 
            }
        }
        public SuggestVattu()
        {
            InitializeComponent();
        }
        public void UpdateSuggestionsVT(List<Vattugoiy> newSuggestions)
        { 
            SuggestionsVT = newSuggestions; 
        }
        private void SuggestVattu_Load(object sender, EventArgs e)
        {

        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Hide(); // Ẩn UserControl khi nút được nhấn    
        }

        private void gridControl1_DoubleClick(object sender, EventArgs e)
        {
            DevExpress.XtraGrid.Views.Grid.GridView gridView = gridControl1.MainView as DevExpress.XtraGrid.Views.Grid.GridView;
            var hitInfo = gridView.CalcHitInfo(gridView.GridControl.PointToClient(MousePosition));


            // Kiểm tra nếu nhấp vào một ô
            if (hitInfo.InRowCell)
            {
                int columnIndex = hitInfo.Column.VisibleIndex; // Chỉ số cột
                var hiddenValue = gridView.GetRowCellValue(hitInfo.RowHandle, gridView.Columns["Ten"]);
                var hiddenValue2 = gridView.GetRowCellValue(hitInfo.RowHandle, gridView.Columns["SoHieu"]);
                ItemSelected?.Invoke(this, hiddenValue.ToString()+"|"+ hiddenValue2.ToString());
            }
        }
    }
}
