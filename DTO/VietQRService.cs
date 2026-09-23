using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaovietTax.DTO
{
    public class VietQRResponse
    {
        public string code { get; set; }
        public string desc { get; set; }
        public BusinessData data { get; set; }
        public Metadata metadata { get; set; }
    }

    public class BusinessData
    {
        public string id { get; set; }
        public string name { get; set; }
        public string internationalName { get; set; }
        public string shortName { get; set; }
        public string address { get; set; }
        public string status { get; set; }
    }

    public class Metadata
    {
        public string disclaimer { get; set; }
        public string source { get; set; }
        public DateTime updatedAt { get; set; }
        public string contact { get; set; }
    }
}
