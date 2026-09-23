using FuzzySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SaovietTax.DTO
{
    public class VietnameseProductMatcher
    {
        public double CalculateSimilarity(string product1, string product2)
        {
            // Bước 1: Chuẩn hóa tiếng Việt
            string normalized1 = NormalizeVietnameseProduct(product1);
            string normalized2 = NormalizeVietnameseProduct(product2);

            Console.WriteLine($"Normalized 1: {normalized1}");
            Console.WriteLine($"Normalized 2: {normalized2}");

            // Bước 2: Sử dụng TokenSetRatio (tốt nhất cho product names)
            int fuzzyScore = Fuzz.TokenSetRatio(normalized1, normalized2);
            double similarity = fuzzyScore / 100.0;

            Console.WriteLine($"Fuzzy Score: {fuzzyScore} -> {similarity:P0}");

            // Bước 3: Áp dụng bonus cho các trường hợp đặc biệt
            var  check= ApplyBonusRules2(normalized1, normalized2, similarity);
            if (check == false)
                similarity = 0;
            return similarity;
        }
       
        public string NormalizeVietnameseProduct(string productName)
        {
            if (string.IsNullOrEmpty(productName))
                return string.Empty;

            // 1. Chuyển thành chữ thường
            productName = productName.ToLower().Trim();

            // 2. Chuẩn hóa đơn vị đo lường
            productName = Regex.Replace(productName, @"\b(\d+)\s*(gram|g|gam)\b", "$1g");
            productName = Regex.Replace(productName, @"\b(\d+)\s*(kg|kilo)\b", "$1kg");
            productName = Regex.Replace(productName, @"\b(\d+)\s*(ml|mililit|mili lít|mililiter)\b", "$1ml");
            // Bổ sung thêm các đơn vị khác
            productName = Regex.Replace(productName, @"\b(\d+)\s*(lít|liter|l|lit)\b", "$1l");
            productName = Regex.Replace(productName, @"\b(\d+)\s*(cm|centimet|centi mét)\b", "$1cm");
            productName = Regex.Replace(productName, @"\b(\d+)\s*(m|met|mét)\b", "$1m");
            productName = Regex.Replace(productName, @"\b(\d+)\s*\*\s*(\d+)\b", "$1x$2");
            productName = Regex.Replace(productName, @"\bSài\s*G[òo]àn?\b", "Sài Gòn", RegexOptions.IgnoreCase);
            productName = Regex.Replace(productName, @"\bSaigon\b", "Sài Gòn", RegexOptions.IgnoreCase);
            // Thêm vào hàm NormalizeProductName của bạn
            productName = Regex.Replace(productName, @"\bCookeis\b", "Cookies", RegexOptions.IgnoreCase);
            productName = Regex.Replace(productName, @"\bCookis\b", "Cookies", RegexOptions.IgnoreCase);
            productName = Regex.Replace(productName, @"\bCookie\b", "Cookies", RegexOptions.IgnoreCase);
            // Chuẩn hóa "bịch" và các biến thể thành "gói"
            productName = Regex.Replace(productName, @"\b(bịch|bich|bịch|bĩch)\b", "gói", RegexOptions.IgnoreCase);

            // Hoặc ngược lại - chuẩn hóa thành "bịch"
            productName = Regex.Replace(productName, @"\b(gói|goi|gói|gỏi|gõi|gọi)\b", "bịch", RegexOptions.IgnoreCase);
            // 3. Chuẩn hóa từ đồng nghĩa (quan trọng nhất)
            productName = StandardizeSynonyms(productName);

            // 4. Loại bỏ từ không quan trọng
            productName = RemoveStopWords(productName);
            productName= productName.Replace("'", "");
            // 5. Chuẩn hóa khoảng trắng và ký tự đặc biệt
            //productName = Regex.Replace(productName, @"[^\w\s\dgkgml]", " ");
            //productName = Regex.Replace(productName, @"\s+", " ").Trim();


            // 5. Chuẩn hóa khoảng trắng và ký tự đặc biệt
            // Loại bỏ ký tự đặc biệt nhưng giữ lại chữ, số và 'x'
            productName = Regex.Replace(productName, @"\b(\d+)\s*\*\s*(\d+)\b", "$1x$2");

            productName = Regex.Replace(productName, @"\s+", " ").Trim();
            return productName;
        }
        private string StandardizeSynonyms(string text)
        {
            var synonymMaps = new Dictionary<string, string[]>
        { 
            {"bơ", new[] {"bơ sữa", "butter", "bo"}},
            {"quy", new[] {"cookie", "biscuit", "bánh quy"}},
            {"bánh", new[] {"bánh ngọt"}},
            {"sữa", new[] {"milk"}} ,
             {"Saigon", new[] {"Sài gòn"}}
        };
             

            foreach (var map in synonymMaps)
            {
                foreach (var synonym in map.Value)
                {
                    if (text.Contains(synonym))
                    {
                        text = text.Replace(synonym, map.Key);
                    }
                }
            }

            return text;
        }
        private string RemoveStopWords(string text)
        {
            var stopWords = new[] { "của","hoặc", "từ", "đến", "loại", "dòng", "sản phẩm" };

            foreach (var stopWord in stopWords)
            {
                text = text.Replace(stopWord, "");
            }

            return text;
        }
        private bool ApplyBonusRules2(string norm1, string norm2, double currentScore)
        {
            double bonus = 0;

            // Bonus nếu cùng số lượng
            if (HasSameQuantity(norm1, norm2))
                return true;

            return false;
        }
        private double ApplyBonusRules(string norm1, string norm2, double currentScore)
        {
            double bonus = 0;

            // Bonus nếu cùng số lượng
            if (HasSameQuantity(norm1, norm2))
                bonus += 0.2;

            return Math.Min(1.0, currentScore + bonus);
        }
        private bool HasSameQuantity(string text1, string text2)
        {
            var qty1 = ExtractQuantity(text1);
            var qty2 = ExtractQuantity(text2);
            if (qty1 == null && qty2 == null)
                return true;
            return qty1 == qty2 && !string.IsNullOrEmpty(qty1);
        }

        private string ExtractQuantity(string text)
        {
            var match = Regex.Match(text, @"(\d+)\s*(g|kg|ml|l|lit|lít)");
            return match.Success ? match.Value : null;
        }

        private bool HasSameMainProduct(string text1, string text2)
        {
            var mainProducts = new[] { "bánh quy", "bánh cookie", "bánh", "kẹo", "sữa" };
            return mainProducts.Any(product => text1.Contains(product) && text2.Contains(product));
        }

        private bool HasSameAttributes(string text1, string text2)
        {
            var attributes = new[] { "bơ", "sữa", "chocolate", "hảo hạng", "cao cấp" };
            return attributes.Any(attr => text1.Contains(attr) && text2.Contains(attr));
        }

    }
}