using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaovietTax.Database
{
    // Lớp dữ liệu đầu vào (Input)
    public class ProductPair
    {
        // Tên sản phẩm thứ nhất
        public string ProductName1 { get; set; }

        // Tên sản phẩm thứ hai
        public string ProductName2 { get; set; }

        // Nhãn (True/False) mà con người đã gán
        public bool IsSame { get; set; }
        public float IsIdentical { get; set; }

    }

    // Lớp dự đoán (Prediction)
    public class ProductPairPrediction
    {
        public bool Prediction { get; set; }
        public float Probability { get; set; }
        public float Score { get; set; }
    }
}
