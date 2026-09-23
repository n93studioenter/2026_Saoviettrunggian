using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace SaovietTax
{
    public static class OllamaChecker
    {
        public static bool IsSameProductAI(string a, string b)
        {
            string prompt =
        $@"You are a system that compares product names in Vietnamese.

Rules:
- Ignore words like: ""vị"", ""hương"", ""loại""
- Ignore word order
- If quantity, weight, or volume is DIFFERENT, answer FALSE
- If quantity, weight, or volume is the SAME or not specified, continue comparison
- Units like g, gram, kg, ml, l are quantities
- If two names describe the same product AND same quantity, answer TRUE
- Otherwise answer FALSE
- Answer ONLY TRUE or FALSE
- Do NOT explain 


Product A: ""{a}""
Product B: ""{b}""";

            var request = new
            {
                model = "llama3.1:8b",
                prompt = prompt,
                stream = false,
                options = new
                {
                    temperature = 0
                }
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(request);

            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/json");
                string result = client.UploadString(
                    "http://localhost:11434/api/generate",
                    json
                );

                dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(result);
                string answer = obj.response.ToString().Trim().ToUpper();

                return answer == "TRUE";
            }
        }
        private static string ToJsonString(string text)
        {
            return "\"" + text
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "")
                .Replace("\n", "\\n")
                + "\"";
        }
    }

}