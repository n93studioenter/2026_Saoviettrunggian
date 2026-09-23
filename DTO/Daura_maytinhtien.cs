using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaovietTax.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    public class InvoiceRa2
    {
        public List<InvoiceRa2List> datas { get; set; }
        public int? total { get; set; }
        public string state { get; set; }
        public int? time { get; set; }
    }

    public class InvoiceRa2List
    {
        public string nbmst { get; set; }
        public int? khmshdon { get; set; }
        public string khhdon { get; set; }
        public int? shdon { get; set; }
        public string cqt { get; set; }
        public List<object> cttkhac { get; set; }
        public string hdon { get; set; }
        public string hsgoc { get; set; }
        public int? hthdon { get; set; }
        public string id { get; set; }
        public object idtbao { get; set; }
        public object idtbhgthdon { get; set; }
        public object idtbhgtrinh { get; set; }
        public object khhdgoc { get; set; }
        public object khmshdgoc { get; set; }
        public object lhdgoc { get; set; }
        public string mhdon { get; set; }
        public string mtdtchieu { get; set; }
        public string nbdchi { get; set; }
        public string nbten { get; set; }
        public string ncnhat { get; set; }
        public object ngcnhat { get; set; }
        public object nky { get; set; }
        public string nmmst { get; set; }
        public string nmten { get; set; }
        public string nmtnmua { get; set; } 
        public string ntao { get; set; }
        public string ntnhan { get; set; }
        public string pban { get; set; }
        public int? ptgui { get; set; }
        public object shdgoc { get; set; }
        public int tchat { get; set; }
        public string tdlap { get; set; }
        public double? tgtcthue { get; set; }
        public double? tgtthue { get; set; }
        public string    tgtttbchu { get; set; }
        public double? tgtttbso { get; set; }
        public string thdon { get; set; }
        public int thlap { get; set; }
        public List<TaxRate> thttltsuat { get; set; }
        public string tlhdon { get; set; }
        public double ? ttcktmai { get; set; }
        public int? tthai { get; set; }
        public int? tttbao { get; set; }
        public List<AdditionalField> ttttkhac { get; set; }
        public int ? ttxly { get; set; }
        public string tvandnkntt { get; set; }
        public int? ladhddt { get; set; }
        public string nbsdthoai { get; set; }
        public object nbcks { get; set; }
        public string nmsdthoai { get; set; }
        public string nmcccd { get; set; }
        public int? bhphap { get; set; }
        public object gchdgoc { get; set; }
        public object tbhgtngay { get; set; }
        public object bhpldo { get; set; }
        public object bhpcbo { get; set; }
        public object bhpngay { get; set; }
        public object tdlhdgoc { get; set; }
        public object tentvandnkntt { get; set; }
        public object kqcht { get; set; }
        public List<AdditionalField> nbttkhac { get; set; }
        public List<AdditionalField> nmttkhac { get; set; }
        public List<AdditionalField> ttkhac { get; set; }
        public string nmloai { get; set; }
        public int tghdmman { get; set; }
        public object tghdmmldo { get; set; }
        public int? cnhan { get; set; }
        public List<object> kqchtmloi { get; set; }
        public string nmdchi { get; set; }
        public object chma { get; set; }
        public object chten { get; set; }
        public object nmshchieu { get; set; }
        public string nbstkhoan { get; set; }
        public string nbtnhang { get; set; }
        public string nmhvtnmhang { get; set; }
        public string thtttoan { get; set; }
        public object nmmdvqhnsach { get; set; }
        public object hdhhdvu { get; set; }
        public object qrcode { get; set; }
        public object ttmstten { get; set; }
        public object ladhddtten { get; set; }
        public object hdxkhau { get; set; }
        public object hdxkptquan { get; set; }
        public object hdgktkhthue { get; set; }
        public object hdonLquans { get; set; }
        public bool tthdclquan { get; set; }
        public object pdndungs { get; set; }
        public object mtthdtbssrs { get; set; }
    }

    public class TaxRate
    {
        public string tsuat { get; set; }
        public double? thtien { get; set; }
        public double? tthue { get; set; }
        public object gttsuat { get; set; }
    }

    public class AdditionalField
    {
        public string ttruong { get; set; }
        public string kdlieu { get; set; }
        public string dlieu { get; set; }
    }

}