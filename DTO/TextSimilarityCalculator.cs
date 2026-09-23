using iText.Kernel.Pdf.Canvas.Wmf;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaovietTax.DTO
{
    public class InputData
    {
        public string Text { get; set; }
    }

    public class TextSimilarityCalculator
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;

        public TextSimilarityCalculator()
        {
            _mlContext = new MLContext();

            // Tạo một danh sách với dữ liệu thử nghiệm
            var sampleData = new List<InputData>
        {
             new InputData { Text = "Máy tính xách tay HP Pavilion" },
            new InputData { Text = "Laptop HP Pavilion" } 
        };

            // Tạo pipeline để chuyển đổi văn bản thành đặc trưng
            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(InputData.Text));

            // Fit mô hình với dữ liệu mẫu
            var trainingData = _mlContext.Data.LoadFromEnumerable(sampleData);
            _model = pipeline.Fit(trainingData);
        }

        public float[] GetFeatures(string text)
        {
            var input = new InputData { Text = text };
            var predictionData = _mlContext.Data.LoadFromEnumerable(new[] { input });
            var transformedData = _model.Transform(predictionData);
            return transformedData.GetColumn<float[]>("Features").FirstOrDefault();
        }

        public double CalculateCosineSimilarity(float[] vecA, float[] vecB)
        {
            double magnitudeA = Math.Sqrt(vecA.Sum(a => a * a));
            double magnitudeB = Math.Sqrt(vecB.Sum(b => b * b));

            if (magnitudeA == 0 || magnitudeB == 0)
            {
                return 0; // Hoặc throw exception
            }

            double dotProduct = vecA.Zip(vecB, (a, b) => a * b).Sum();
            return dotProduct / (magnitudeA * magnitudeB);
        }
    }


}
