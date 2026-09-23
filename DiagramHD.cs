using DevExpress.Diagram.Core;
using DevExpress.Utils;
using DevExpress.XtraDiagram;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaovietTax
{
    public partial class DiagramHD : DevExpress.XtraEditors.XtraForm
    {
        public DiagramHD()
        {
            InitializeComponent();
        }

        private void DiagramHD_Load(object sender, EventArgs e)
        {
            var shape = new DiagramShape
            {
                Content = "Khởi tạo hóa đơn",
                Shape = BasicShapes.Rectangle,
                Position = new PointFloat(100, 100)
            };
            diagramControl1.Items.Add(shape);

        }
    }
}