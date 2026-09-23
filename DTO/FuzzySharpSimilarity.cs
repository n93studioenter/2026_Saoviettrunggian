using FuzzySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaovietTax.DTO
{
    public class FuzzySharpSimilarity

    {
        public double CalculateSimilarity(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return 0;

            // Sử dụng Ratio cho similarity tổng thể
            int score = Fuzz.Ratio(text1, text2);
            return score / 100.0;
        }

        public double CalculatePartialSimilarity(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return 0;

            // Partial Ratio tốt hơn cho string có độ dài khác nhau
            int score = Fuzz.PartialRatio(text1, text2);
            return score / 100.0;
        }

        public double CalculateTokenSimilarity(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return 0;

            // Token Sort Ratio tốt cho thứ tự từ khác nhau
            int score = Fuzz.TokenSortRatio(text1, text2);
            return score / 100.0;
        }

        public double CalculateTokenSetSimilarity(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return 0;

            // Token Set Ratio tốt nhất cho product matching
            int score = Fuzz.TokenSetRatio(text1, text2);
            return score / 100.0;
        }

        public double CalculateWeightedSimilarity(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return 0;

            // Kết hợp nhiều phương pháp với trọng số
            double[] scores = new double[4];

            scores[0] = Fuzz.Ratio(text1, text2) / 100.0;              // 20%
            scores[1] = Fuzz.PartialRatio(text1, text2) / 100.0;       // 20%
            scores[2] = Fuzz.TokenSortRatio(text1, text2) / 100.0;     // 30%
            scores[3] = Fuzz.TokenSetRatio(text1, text2) / 100.0;      // 30%

            double[] weights = { 0.2, 0.2, 0.3, 0.3 };

            return scores.Zip(weights, (s, w) => s * w).Sum();
        }
    }
}
