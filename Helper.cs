using SaovietTax.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ClosedXML.Excel.XLPredefinedFormat;

namespace SaovietTax
{

    public static class Helpers
    {
        static string WaitFile(string folder, string fileNameContains)
        {
            for (int i = 0; i < 60; i++)
            {
                var file = Directory.GetFiles(folder)
                    .FirstOrDefault(f =>
                        Path.GetFileName(f).Contains(fileNameContains)
                        && !f.EndsWith(".crdownload"));

                if (file != null)
                {
                    // thử mở để chắc chắn file đã tải xong
                    try
                    {
                        using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None))
                        {
                            return file;
                        }
                    }
                    catch
                    {
                    }
                }

                Thread.Sleep(1000);
            }

            throw new Exception("Không tìm thấy file tải về");
        }
        public static string RemoveLeadingZeros(string invoiceNumber)
        {
            if (string.IsNullOrEmpty(invoiceNumber))
                return invoiceNumber;

            return Regex.Replace(invoiceNumber, "^0+", "");
        }
        public static T MapTo<T>(DataRow row) where T : new()
        {
            T obj = new T();
            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                if (row.Table.Columns.Contains(prop.Name))
                {
                    object value = row[prop.Name];

                    if (value != DBNull.Value)
                    {
                        prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType), null);
                    }
                }
            }
            return obj;
        }
        public static string ExtractNumber(string input)
        {
            // Tìm số sau từ SO, HD, HOA DON hoặc HDS
            var match = Regex.Match(input, @"(?:so|hd|hoa don|hds)\s*([-\d]+)", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                // Trích xuất số từ chuỗi (ví dụ: lấy 184 từ HD184)
                string extractedNumber = Regex.Match(match.Value, @"\d+").Value;
                return extractedNumber;
            }

            return "Không tìm thấy số";
        }
        public static List<string> FindCommonAdjacentPhrases(string str1, string str2, int minWords = 2)
        {
            // Chuẩn hóa chuỗi: loại bỏ khoảng trắng thừa và chuyển về chữ thường
            string[] words1 = str1.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] words2 = str2.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            List<string> commonPhrases = new List<string>();

            // Duyệt qua tất cả các cụm từ có thể trong chuỗi 1
            for (int i = 0; i <= words1.Length - minWords; i++)
            {
                // Tạo cụm từ với độ dài từ minWords đến tối đa có thể
                for (int length = minWords; length <= words1.Length - i; length++)
                {
                    string[] phraseToFind = words1.Skip(i).Take(length).ToArray();
                    string phraseStr = string.Join(" ", phraseToFind);

                    // Kiểm tra xem cụm từ này có tồn tại trong chuỗi 2 không
                    if (ContainsPhrase(words2, phraseToFind))
                    {
                        commonPhrases.Add(phraseStr);
                    }
                }
            }

            // Loại bỏ các cụm từ con (nếu có cụm từ dài hơn chứa nó)
            return commonPhrases
                .Distinct()
                .OrderByDescending(p => p.Split(' ').Length)
                .Where(p => !commonPhrases.Any(q => q != p && q.Contains(p)))
                .ToList();
        }
        private static bool ContainsPhrase(string[] words, string[] phrase)
        {
            if (phrase.Length > words.Length) return false;

            for (int i = 0; i <= words.Length - phrase.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < phrase.Length; j++)
                {
                    if (!string.Equals(words[i + j], phrase[j], StringComparison.OrdinalIgnoreCase))
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return true;
            }

            return false;
        }
        public static List<string> FindTwoWordPhrases(string str1, string str2)
        {
            List<string> commonPhrases = new List<string>();

            // Tách các từ
            var words1 = str1.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var words2 = str2.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Duyệt qua từng cụm từ hai từ trong words1
            for (int i = 0; i < words1.Length - 1; i++)
            {
                string phrase = words1[i] + " " + words1[i + 1];

                // Kiểm tra xem cụm từ có xuất hiện trong str2
                if (str2.ToLower().Contains(phrase))
                {
                    if (!commonPhrases.Contains(phrase)) // Đảm bảo không trùng lặp
                    {
                        commonPhrases.Add(phrase);
                    }
                }
            }

            return commonPhrases;
        }
        public static string ExtractName(string input)
        {
            // Danh sách từ khóa
            List<string> keywords = new List<string> { "Chuyen", "TKThe", "FT", "thue" };

            // Tạo chuỗi từ khóa cho biểu thức chính quy
            string keywordsPattern = string.Join("|", keywords);

            // Biểu thức chính quy để tìm tên, không phân biệt chữ hoa chữ thường
            string pattern = $@"(?<=\d\s)([A-Za-z\s]+)(?=\s*(?:{keywordsPattern}))";
            Match match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                return match.Value.Trim();
            }

            return "Không tìm thấy tên.";
        }
        public static string GetName(string input)
        {
            // Biểu thức chính quy để tìm tên công ty
            string companyPattern = @"(?:CÔNG TY TNHH|CT TNHH|CT)\s+([A-Z\s]+)";
            Match companyMatch = Regex.Match(input, companyPattern, RegexOptions.IgnoreCase);

            if (companyMatch.Success)
            {
                // Trả về tên công ty sau từ khóa
                return "Tên công ty: " + companyMatch.Groups[1].Value.Trim();
            }

            // Biểu thức chính quy để tìm tên người
            string personPattern = @"([A-Z]+\s+[A-Z]+\s+[A-Z]+)";
            Match personMatch = Regex.Match(input, personPattern);

            if (personMatch.Success)
            {
                return "Tên người: " + personMatch.Value.Trim();
            }

            return "Không tìm thấy tên.";
        }
        public static string RemoveVietnameseDiacritics(string str)
        {
            if(str == null)
                return "";
            // Mảng chứa ký tự có dấu
            str = str.ToLower();
            str = Regex.Replace(str, "[àáạảãâầấậẩẫăằắặẳẵ]", "a");
            str = Regex.Replace(str, "[èéẹẻẽêềếệểễ]", "e");
            str = Regex.Replace(str, "[ìíịỉĩ]", "i");
            str = Regex.Replace(str, "[òóọỏõôồốộổỗơờớợởỡ]", "o");
            str = Regex.Replace(str, "[ùúụủũưừứựửữ]", "u");
            str = Regex.Replace(str, "[ỳýỵỷỹ]", "y");
            str = Regex.Replace(str, "đ", "d");

            // Thay thế khoảng trắng bằng dấu gạch ngang
           // str = Regex.Replace(str, " ", "-");
            //str = str.Replace(",", " ");
            //str = str.Replace("."," ");
            // Thay thế tất cả các âm "o" có dấu thành "o" không dấu
            str = str.Replace("ó", "o");
            str = str.Replace("ò", "o");
            str = str.Replace("õ", "o");
            str = str.Replace("ọ", "o");
            str = str.Replace("ỏ", "o");
            str = str.Replace("ô", "o");
            str = str.Replace("ơ", "o");
            str = str.Replace("'", "");
            return str;
        }
        public static string GetLastFourDigits(string input)
        {
            // Tìm vị trí của dấu '-'
            int dashIndex = input.IndexOf('-');

            // Nếu có dấu '-' trong chuỗi, lấy phần trước đó
            if (dashIndex != -1)
            {
                input = input.Substring(0, dashIndex);
            }

            // Lấy 4 ký tự cuối cùng
            if (input.Length >= 4)
            {
                return input.Substring(input.Length - 4);
            }
            else
            {
                return input; // Trả về toàn bộ chuỗi nếu độ dài nhỏ hơn 4
            }
        }
        public static class StringWordSimilarity
        {

            public static double CalculateSimilarity(string product1, string product2)
            {
                // Kiểm tra chuỗi rỗng/null
                if (string.IsNullOrWhiteSpace(product1)) return 0;
                if (string.IsNullOrWhiteSpace(product2)) return 0;

                // Chuẩn hóa chuỗi
                product1 = NormalizeProductName(product1);
                product2 = NormalizeProductName(product2);

                // Nếu giống nhau hoàn toàn sau chuẩn hóa
                if (product1.Equals(product2)) return 100;

                // Tách thành các token
                var tokens1 = TokenizeProductName(product1);
                var tokens2 = TokenizeProductName(product2);

                // Kiểm tra số có khác nhau không (bao gồm cả đơn vị)
                if (HasDifferentNumberUnits(tokens1, tokens2))
                    return 0;

                // Tính similarity chỉ trên phần chữ (bỏ qua số và đơn vị)
                var textParts1 = tokens1.Where(t => !IsNumberWithUnit(t)).ToArray();
                var textParts2 = tokens2.Where(t => !IsNumberWithUnit(t)).ToArray();

                var commonWords = textParts1.Intersect(textParts2).Count();
                var totalUniqueWords = textParts1.Union(textParts2).Count();

                return totalUniqueWords == 0 ? 100 : (double)commonWords / totalUniqueWords * 100;
            }

            private static string NormalizeProductName(string input)
            {
                // Chuẩn hóa khoảng trắng và ký tự đặc biệt
                input = Regex.Replace(input, @"[^\w\s]", " ");
                input = Regex.Replace(input, @"\s+", " ").Trim();
                return input.ToLower();
            }

            private static string[] TokenizeProductName(string input)
            {
                return input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }

            private static bool HasDifferentNumberUnits(string[] tokens1, string[] tokens2)
            {
                var numbers1 = tokens1.Where(IsNumberWithUnit).Select(NormalizeNumber).ToArray();
                var numbers2 = tokens2.Where(IsNumberWithUnit).Select(NormalizeNumber).ToArray();

                // Nếu cả hai không có số → không có số khác nhau
                if (!numbers1.Any() && !numbers2.Any()) return false;

                // Nếu một bên có số, một bên không → coi như khác
                if (numbers1.Any() != numbers2.Any()) return true;

                // So sánh các số đã được chuẩn hóa
                return !numbers1.SequenceEqual(numbers2);
            }

            private static bool IsNumberWithUnit(string token)
            {
                // Nhận diện token có chứa số (có thể kèm đơn vị)
                return Regex.IsMatch(token, @"\d+");
            }

            private static string NormalizeNumber(string token)
            {
                // Chuẩn hóa số: giữ lại chỉ phần số (bỏ đơn vị)
                return Regex.Match(token, @"\d+").Value;
            }
        }

        private static readonly Dictionary<char, string> UnicodeToVniMap = new Dictionary<char, string>
{
   
    {'À', "AØ"},{'\'', ""}, {'Á', "AÙ"}, {'Â', "AÂ"}, {'Ã', "AÕ"},
            {'È', "EØ"}, {'É', "EÙ"}, {'Ê', "EÂ"}, {'Ì', "Ì"},
            {'Í', "Í"}, {'Ò', "OØ"}, {'Ó', "OÙ"}, {'Ô', "OÂ"},
            {'Õ', "OÕ"}, {'Ù', "UØ"}, {'Ú', "UÙ"}, {'Ý', "YÙ"},
            {'à', "aø"}, {'á', "aù"}, {'â', "aâ"}, {'ã', "aõ"},
            {'è', "eø"}, {'é', "eù"}, {'ê', "eâ"}, {'ì', "ì"},
            {'í', "í"}, {'ò', "oø"}, {'ó', "où"}, {'ô', "oâ"},
            {'õ', "oõ"}, {'ù', "uø"}, {'ú', "uù"}, {'ý', "yù"},
            {'Ă', "AÊ"}, {'ă', "aê"}, {'Đ', "Ñ"}, {'đ', "ñ"},
            {'Ĩ', "Ó"}, {'ĩ', "ó"}, {'Ũ', "UÕ"}, {'ũ', "uõ"},
            {'Ơ', "Ô"}, {'ơ', "ô"}, {'Ư', "Ö"}, {'ư', "ö"},
            {'Ạ', "AÏ"}, {'ạ', "aï"}, {'Ả', "AÛ"}, {'ả', "aû"},
            {'Ấ', "AÁ"}, {'ấ', "aá"}, {'Ầ', "AÀ"}, {'ầ', "aà"},
            {'Ẩ', "AÅ"}, {'ẩ', "aå"}, {'Ẫ', "AÃ"}, {'ẫ', "aã"},
            {'Ậ', "AÄ"}, {'ậ', "aä"}, {'Ắ', "AÉ"}, {'ắ', "aé"},
            {'Ằ', "AÈ"}, {'ằ', "aè"}, {'Ẳ', "AÚ"}, {'ẳ', "aú"},
            {'Ẵ', "AÜ"}, {'ẵ', "aü"}, {'Ặ', "AË"}, {'ặ', "aë"},
            {'Ẹ', "EÏ"}, {'ẹ', "eï"}, {'Ẻ', "EÛ"}, {'ẻ', "eû"},
            {'Ẽ', "EÕ"}, {'ẽ', "eõ"}, {'Ế', "EÁ"}, {'ế', "eá"},
            {'Ề', "EÀ"}, {'ề', "eà"}, {'Ể', "EÅ"}, {'ể', "eå"},
            {'Ễ', "EÃ"}, {'ễ', "eã"}, {'Ệ', "EÄ"}, {'ệ', "eä"},
            {'Ỉ', "Æ"}, {'ỉ', "æ"}, {'Ị', "Ò"}, {'ị', "ò"},
            {'Ọ', "OÏ"}, {'ọ', "oï"}, {'Ỏ', "OÛ"}, {'ỏ', "oû"},
            {'Ố', "OÁ"}, {'ố', "oá"}, {'Ồ', "OÀ"}, {'ồ', "oà"},
            {'Ổ', "OÅ"}, {'ổ', "oå"}, {'Ỗ', "OÃ"}, {'ỗ', "oã"},
            {'Ộ', "OÄ"}, {'ộ', "oä"}, {'Ớ', "ÔÙ"}, {'ớ', "ôù"},
            {'Ờ', "ÔØ"}, {'ờ', "ôø"}, {'Ở', "ÔÛ"}, {'ở', "ôû"},
            {'Ỡ', "ÔÕ"}, {'ỡ', "ôõ"}, {'Ợ', "ÔÏ"}, {'ợ', "ôï"},
            {'Ụ', "UÏ"}, {'ụ', "uï"}, {'Ủ', "UÛ"}, {'ủ', "uû"},
            {'Ứ', "ÖÙ"}, {'ứ', "öù"}, {'Ừ', "ÖØ"}, {'ừ', "öø"},
            {'Ử', "ÖÛ"}, {'ử', "öû"}, {'Ữ', "ÖÕ"}, {'ữ', "öõ"},
            {'Ự', "ÖÏ"}, {'ự', "öï"}, {'Ỳ', "YØ"}, {'ỳ', "yø"},
            {'Ỵ', "Î"}, {'ỵ', "î"}, {'Ỷ', "YÛ"}, {'ỷ', "yû"},
            {'Ỹ', "YÕ"}, {'ỹ', "yõ"}
};

        public static string InsertZero(string text)
        {
            int lenght = int.Parse(text);
            return lenght.ToString("D8");
        }
        static string RemoveLeadingSpecialCharacters(string input)
        {
            if (input == null)
                return "";
            // Sử dụng LINQ để lấy các ký tự không phải là ký tự đặc biệt
            return new string(input.SkipWhile(c => !char.IsLetterOrDigit(c)).ToArray());
        }
        public static string RemoveDiacritics(string input)
        {
            var normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var ch in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
        public static string NormalizeVietnameseStringew(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            // 1. Chuẩn hoá Unicode
            input = input.Normalize(NormalizationForm.FormC);

            // 2. Bỏ dấu tiếng Việt
            input = RemoveDiacritics(input);

            // 3. Đưa toàn bộ về lowercase
            input = input.ToLower();

            // 4. Bỏ nội dung trong ngoặc
            //input = Regex.Replace(input, @"\(.*?\)", "");

            // 5. Xử lý ký tự 'x' (dạng * hoặc X hoặc ×)
            input = input.Replace("*", "x");
            input = Regex.Replace(input, @"\s*[xX×]\s*", "x");

            // 6. Chuẩn hóa đơn vị dung tích
            input = input.Replace(" lit", "l")
                         .Replace(" l", "l")
                         .Replace("ml", " ml")
                         .Replace("l ", "l ");

            // 7. Tách số và chữ (455ml → 455 ml)
            input = Regex.Replace(input, @"(\d+)([a-zA-Z])", "$1 $2");
            input = Regex.Replace(input, @"([a-zA-Z])(\d+)", "$1 $2");

            // 8. Bỏ tab và ký tự thừa
            input = input.Replace("\t", "");

            // 9. Chuẩn hóa khoảng trắng
            input = Regex.Replace(input, @"\s+", " ").Trim();

            return input;
        }

        public static string NormalizeVietnameseString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            input = input.Replace("*", "x");
            input = input.Replace(" lít", "l");
            input = input.Replace("( ", "(");
            // 4. Bỏ nội dung trong ngoặc
           // input = Regex.Replace(input, @"\(.*?\)", "");
           // input= Regex.Replace(input, @"\*(\d+)", "x$1");
 
            // 7. Tách số và chữ (455ml → 455 ml)
           // input = Regex.Replace(input, @"(\d+)([a-zA-Z])", "$1 $2");
           // input = Regex.Replace(input, @"([a-zA-Z])(\d+)", "$1 $2");
            //input = Regex.Replace(input, @"(\d+)\s*x\s*(\d+)\s*ml", "$2 ml x $1");

            //input = RemoveLeadingSpecialCharacters(input);
            //Bỏ đi tab
            input = input.Replace("\t", ""); // Thay thế ký tự tab bằng chuỗi rỗng
            input = input.Normalize(NormalizationForm.FormC);

            return input.Trim();
        }
        public static string ConvertUnicodeToVni(string input)
        {

            input = NormalizeVietnameseString(input);
            StringBuilder output = new StringBuilder();

            foreach (char c in input)
            {
                string character = c.ToString();
               // Console.WriteLine($"Current character: {character}");

                if (UnicodeToVniMap.ContainsKey(c))
                {
                    output.Append(UnicodeToVniMap[c]); // Thay thế bằng ký tự VNI tương ứng
                }
                else
                {
                    output.Append(c); // Giữ nguyên ký tự nếu không có trong bảng ánh xạ
                }
            }

            return output.ToString();
        }

        private static readonly Dictionary<string, char> VniToUnicodeMap = new Dictionary<string, char>
{
    // ==================== CHỮ THƯỜNG ====================
    
    // ----- A -----
    {"aø", 'à'}, {"aù", 'á'}, {"aû", 'ả'}, {"aõ", 'ã'}, {"aï", 'ạ'},
    {"aâ", 'â'}, {"aá", 'ấ'}, {"aà", 'ầ'}, {"aå", 'ẩ'}, {"aã", 'ẫ'}, {"aä", 'ậ'},
    {"aê", 'ă'}, {"aé", 'ắ'}, {"aè", 'ằ'}, {"aú", 'ẳ'}, {"aü", 'ẵ'}, {"aë", 'ặ'},
    
    // ----- E -----
    {"eø", 'è'}, {"eù", 'é'}, {"eû", 'ẻ'}, {"eõ", 'ẽ'}, {"eï", 'ẹ'},
    {"eâ", 'ê'}, {"eá", 'ế'}, {"eà", 'ề'}, {"eå", 'ể'}, {"eã", 'ễ'}, {"eä", 'ệ'},
    
    // ----- I -----
    {"ì", 'ì'}, {"í", 'í'}, {"æ", 'ỉ'}, {"ò", 'ị'},
    // LƯU Ý: "ó" KHÔNG có ở đây vì "ó" thuộc nhóm O (xem bên dưới)
    // Trước đây bị sai: {"ó", 'ĩ'} → đã xóa
    
    // ----- O -----
    {"oø", 'ò'}, {"où", 'ó'}, {"oû", 'ỏ'}, {"oõ", 'õ'}, {"oï", 'ọ'},
    {"oâ", 'ô'}, {"oá", 'ố'}, {"oà", 'ồ'}, {"oå", 'ổ'}, {"oã", 'ỗ'}, {"oä", 'ộ'},
    {"ô", 'ơ'}, {"ôù", 'ớ'}, {"ôø", 'ờ'}, {"ôû", 'ở'}, {"ôõ", 'ỡ'}, {"ôï", 'ợ'},
    
    // ----- U -----
    {"uø", 'ù'}, {"uù", 'ú'}, {"uû", 'ủ'}, {"uõ", 'ũ'}, {"uï", 'ụ'},
    {"ö", 'ư'}, {"öù", 'ứ'}, {"öø", 'ừ'}, {"öû", 'ử'}, {"öõ", 'ữ'}, {"öï", 'ự'},
    
    // ----- Y -----
    {"yø", 'ỳ'}, {"yù", 'ý'}, {"yû", 'ỷ'}, {"yõ", 'ỹ'}, {"yï", 'ỵ'},
    
    // ----- D -----
    {"ñ", 'đ'},
    
    // ==================== CHỮ HOA ====================
    
    // ----- A -----
    {"AØ", 'À'}, {"AÙ", 'Á'}, {"AÛ", 'Ả'}, {"AÕ", 'Ã'}, {"AÏ", 'Ạ'},
    {"AÂ", 'Â'}, {"AÁ", 'Ấ'}, {"AÀ", 'Ầ'}, {"AÅ", 'Ẩ'}, {"AÃ", 'Ẫ'}, {"AÄ", 'Ậ'},
    {"AÊ", 'Ă'}, {"AÉ", 'Ắ'}, {"AÈ", 'Ằ'}, {"AÚ", 'Ẳ'}, {"AÜ", 'Ẵ'}, {"AË", 'Ặ'},
    
    // ----- E -----
    {"EØ", 'È'}, {"EÙ", 'É'}, {"EÛ", 'Ẻ'}, {"EÕ", 'Ẽ'}, {"EÏ", 'Ẹ'},
    {"EÂ", 'Ê'}, {"EÁ", 'Ế'}, {"EÀ", 'Ề'}, {"EÅ", 'Ể'}, {"EÃ", 'Ễ'}, {"EÄ", 'Ệ'},
    
    // ----- I -----
    {"Ì", 'Ì'}, {"Í", 'Í'}, {"Æ", 'Ỉ'}, {"Ò", 'Ị'},
    // LƯU Ý: "Ó" KHÔNG có ở đây vì "Ó" thuộc nhóm O (xem bên dưới)
    // Trước đây bị sai: {"Ó", 'Ĩ'} → đã xóa
    
    // ----- O -----
    {"OØ", 'Ò'}, {"OÙ", 'Ó'}, {"OÛ", 'Ỏ'}, {"OÕ", 'Õ'}, {"OÏ", 'Ọ'},
    {"OÂ", 'Ô'}, {"OÁ", 'Ố'}, {"OÀ", 'Ồ'}, {"OÅ", 'Ổ'}, {"OÃ", 'Ỗ'}, {"OÄ", 'Ộ'},
    {"Ô", 'Ơ'}, {"ÔÙ", 'Ớ'}, {"ÔØ", 'Ờ'}, {"ÔÛ", 'Ở'}, {"ÔÕ", 'Ỡ'}, {"ÔÏ", 'Ợ'},
    
    // ----- U -----
    {"UØ", 'Ù'}, {"UÙ", 'Ú'}, {"UÛ", 'Ủ'}, {"UÕ", 'Ũ'}, {"UÏ", 'Ụ'},
    {"Ö", 'Ư'}, {"ÖÙ", 'Ứ'}, {"ÖØ", 'Ừ'}, {"ÖÛ", 'Ử'}, {"ÖÕ", 'Ữ'}, {"ÖÏ", 'Ự'},
    
    // ----- Y -----
    {"YØ", 'Ỳ'}, {"YÙ", 'Ý'}, {"YÛ", 'Ỷ'}, {"YÕ", 'Ỹ'}, {"YÏ", 'Ỵ'},
    
    // ----- D -----
    {"Ñ", 'Đ'},
};

        public static string ConvertVniToUnicode(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            input = NormalizeVietnameseString(input);
            StringBuilder output = new StringBuilder(input.Length);
            int i = 0;

            while (i < input.Length)
            {
                // Ưu tiên match 2 ký tự trước (aø, aù, oø, où, ...)
                if (i + 1 < input.Length)
                {
                    string two = input.Substring(i, 2);
                    if (VniToUnicodeMap.TryGetValue(two, out char mapped2))
                    {
                        output.Append(mapped2);
                        i += 2;
                        continue;
                    }
                }

                // Sau đó match 1 ký tự
                string one = input[i].ToString();
                if (VniToUnicodeMap.TryGetValue(one, out char mapped1))
                {
                    output.Append(mapped1);
                    i += 1;
                    continue;
                }

                // Không match → giữ nguyên
                output.Append(input[i]);
                i += 1;
            }

            return output.ToString();
        }



        public static string VniToUni(string str)
        {
            // Bảng ánh xạ VNI sang Unicode
            string[] VNI = {
    "aù", "aø", "aû", "aõ", "aï", "aâ", "aê", "aá", "aà", "aå", "aã", "aä", "aé", "aè", "aú", "aü", "aë",
    "AÙ", "AØ", "AÛ", "AÕ", "AÏ", "AÂ", "AÊ", "AÁ", "AÀ", "AÅ", "AÃ", "AÄ", "AÉ", "AÈ", "AÚ", "AÜ", "AË",
    "eù", "eø", "eû", "eõ", "eï", "eâ", "eá", "eà", "eå", "eã", "eä",
    "EÙ", "EØ", "EÛ", "EÕ", "EÏ", "EÂ", "EÁ", "EÀ", "EÅ", "EÃ", "EÄ",
    "í ", "ì ", "æ ", "ó ", "ò ",
    "Í ", "Ì ", "Æ ", "Ó ", "Ò ",
    "où", "oø", "oû", "oõ", "oï", "oâ", "ô", "oá", "oà", "oå", "oã", "oä", "ôù", "ôø", "ôû", "ôõ", "ôï",
    "OÙ", "OØ", "OÛ", "OÕ", "OÏ", "OÂ", "Ô ", "OÁ", "OÀ", "OÅ", "OÃ", "OÄ", "ÔÙ", "ÔØ", "ÔÛ", "ÔÕ", "ÔÏ",
    "uù", "uø", "uû", "uõ", "uï", "ö ", "öù", "öø", "öû", "öõ", "öï",
    "UÙ", "UØ", "UÛ", "UÕ", "UÏ", "Ö ", "ÖÙ", "ÖØ", "ÖÛ", "ÖÕ", "ÖÏ",
    "yù", "yø", "yû", "yõ", "î ",
    "YÙ", "YØ", "YÛ", "YÕ", "Î ",
    "ñ ", "Ñ "
};

            string[] UNI = {
    "E1", "E0", "1EA3", "E3", "1EA1", "E2", "103", "1EA5", "1EA7", "1EA9", "1EAB", "1EAD", "1EAF", "1EB1", "1EB3", "1EB5", "1EB7",
    "C1", "C0", "1EA2", "C3", "1EA0", "C2", "102", "1EA4", "1EA6", "1EA8", "1EAA", "1EAC", "1EAE", "1EB0", "1EB2", "1EB4", "1EB6",
    "E9", "E8", "1EBB", "1EBD", "1EB9", "EA", "1EBF", "1EC1", "1EC3", "1EC5", "1EC7",
    "C9", "C8", "1EBA", "1EBC", "1EB8", "CA", "1EBE", "1EC0", "1EC2", "1EC4", "1EC6",
    "ED", "EC", "1EC9", "129", "1ECB",
    "CD", "CC", "1EC8", "128", "1ECA",
    "F3", "F2", "1ECF", "F5", "1ECD", "F4", "1A1", "1ED1", "1ED3", "1ED5", "1ED7", "1ED9", "1EDB", "1EDD", "1EDF", "1EE1", "1EE3",
    "D3", "D2", "1ECE", "D5", "1ECC", "D4", "1A0", "1ED0", "1ED2", "1ED4", "1ED6", "1ED8", "1EDA", "1EDC", "1EDE", "1EE0", "1EE2",
    "FA", "F9", "1EE7", "169", "1EE5", "1B0", "1EE9", "1EEB", "1EED", "1EEF", "1EF1",
    "DA", "D9", "1EE6", "168", "1EE4", "1AF", "1EE8", "1EEA", "1EEC", "1EEE", "1EF0",
    "FD", "1EF3", "1EF7", "1EF9", "1EF5",
    "DD", "1EF2", "1EF6", "1EF8", "1EF4",
    "111", "110"
};
            StringBuilder sUni = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {
                // Kiểm tra cặp ký tự (2 ký tự)
                if (i + 1 < str.Length)
                {
                    string pair = str.Substring(i, 2);
                    int index = Array.IndexOf(VNI, pair);
                    if (index >= 0)
                    {
                        sUni.Append(ConvertToUnicodeChar(UNI[index]));
                        i++; // Bỏ qua ký tự thứ hai trong cặp
                        continue;
                    }
                }

                // Kiểm tra ký tự đơn (1 ký tự)
                string single = str[i].ToString() + " ";
                int singleIndex = Array.IndexOf(VNI, single);
                if (singleIndex >= 0)
                {
                    sUni.Append(ConvertToUnicodeChar(UNI[singleIndex]));
                }
                else
                {
                    // Giữ nguyên ký tự nếu không có trong bảng ánh xạ
                    sUni.Append(str[i]);
                }
            }

            return sUni.ToString();
        }

        private static char ConvertToUnicodeChar(string hexCode)
        {
            int code = int.Parse(hexCode, System.Globalization.NumberStyles.HexNumber);
            return (char)code;
        }
    }
}
