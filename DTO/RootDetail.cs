using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SaovietTax.DTO
{
    public class Lephi
    {
        public string tlphi { get; set; }
        public double ? tphi { get; set; }
    }
    public class Invoice
    {
        [JsonPropertyName("nbmst")]
        public string Nbmst { get; set; }

        [JsonPropertyName("khmshdon")]
        public int ? Khmshdon { get; set; }

        [JsonPropertyName("khhdon")]
        public string Khhdon { get; set; }

        [JsonPropertyName("shdon")]
        public int ? Shdon { get; set; }

        [JsonPropertyName("cqt")]
        public string Cqt { get; set; }

        [JsonPropertyName("cttkhac")]
        public List<CustomField> Cttkhac { get; set; } = new List<CustomField>(); // Khởi tạo để tránh null

        [JsonPropertyName("dvtte")]
        public string Dvtte { get; set; }

        [JsonPropertyName("hdon")]
        public string Hdon { get; set; }

        [JsonPropertyName("hsgcma")]
        public string Hsgcma { get; set; }

        [JsonPropertyName("hsgoc")]
        public string Hsgoc { get; set; }

        [JsonPropertyName("hthdon")]
        public int ? Hthdon { get; set; }

        [JsonPropertyName("htttoan")]
        public int ? Httoan { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("idtbao")]
        public object Idtbao { get; set; } // Nullable object

        [JsonPropertyName("khdon")]
        public object Khdon { get; set; } // Nullable object

        [JsonPropertyName("khhdgoc")]
        public object Khhdgoc { get; set; } // Nullable object

        [JsonPropertyName("khmshdgoc")]
        public object Khmshdgoc { get; set; } // Nullable object

        [JsonPropertyName("lhdgoc")]
        public object Lhdgoc { get; set; } // Nullable object

        [JsonPropertyName("mhdon")]
        public string Mhdon { get; set; }

        [JsonPropertyName("mtdiep")]
        public object Mtdiep { get; set; } // Nullable object

        [JsonPropertyName("mtdtchieu")]
        public string Mtdtchieu { get; set; }

        [JsonPropertyName("nbdchi")]
        public string Nbdchi { get; set; }

        [JsonPropertyName("chma")]
        public object Chma { get; set; } // Nullable object

        [JsonPropertyName("chten")]
        public object Chten { get; set; } // Nullable object

        [JsonPropertyName("nbhdktngay")]
        public object Nbhdktngay { get; set; } // Nullable object

        [JsonPropertyName("nbhdktso")]
        public object Nbhdktso { get; set; } // Nullable object

        [JsonPropertyName("nbhdso")]
        public object Nbhdso { get; set; } // Nullable object

        [JsonPropertyName("nblddnbo")]
        public object Nblddnbo { get; set; } // Nullable object

        [JsonPropertyName("nbptvchuyen")]
        public object Nbptvchuyen { get; set; } // Nullable object

        [JsonPropertyName("nbstkhoan")]
        public string Nbstkhoan { get; set; }

        [JsonPropertyName("nbten")]
        public string Nbten { get; set; }

        [JsonPropertyName("nbtnhang")]
        public string Nbtnhang { get; set; }

        [JsonPropertyName("nbtnvchuyen")]
        public object Nbtnvchuyen { get; set; } // Nullable object

        [JsonPropertyName("nbttkhac")]
        public List<object> Nbttkhac { get; set; } = new List<object>(); // Khởi tạo để tránh null

        [JsonPropertyName("ncma")]
        public DateTime ? Ncma { get; set; }

        [JsonPropertyName("ncnhat")]
        public DateTime Ncnhat { get; set; }

        [JsonPropertyName("ngcnhat")]
        public string Ngcnhat { get; set; }
        public object nky { get; set; }


        [JsonPropertyName("nky")]
        public DateTime Nky { get; set; }

        [JsonPropertyName("nmdchi")]
        public string Nmdchi { get; set; }

        [JsonPropertyName("nmmst")]
        public string Nmmst { get; set; }

        [JsonPropertyName("nmstkhoan")]
        public object Nmstkhoan { get; set; } // Nullable object

        [JsonPropertyName("nmten")]
        public string Nmten { get; set; }

        [JsonPropertyName("nmtnhang")]
        public object Nmtnhang { get; set; } // Nullable object

        [JsonPropertyName("nmtnmua")]
        public object Nmtnmua { get; set; } // Nullable object

        [JsonPropertyName("nmttkhac")]
        public List<object> Nmttkhac { get; set; } = new List<object>(); // Khởi tạo để tránh null

        [JsonPropertyName("ntao")]
        public DateTime Ntao { get; set; }

        [JsonPropertyName("ntnhan")]
        public DateTime Ntnhan { get; set; }

        [JsonPropertyName("pban")]
        public string Pban { get; set; }

        [JsonPropertyName("ptgui")]
        public int ? Ptgui { get; set; }

        [JsonPropertyName("shdgoc")]
        public object Shdgoc { get; set; } // Nullable object

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("tchat")]
        public int ? Tchat { get; set; }

        [JsonPropertyName("tdlap")]
        public DateTime Tdlap { get; set; }

        [JsonPropertyName("tgia")]
        public double ? Tgia { get; set; }

        [JsonPropertyName("tgtcthue")]
        public double ? Tgtcthue { get; set; }

        [JsonPropertyName("tgtthue")]
        public double ? Tgtthue { get; set; }

        [JsonPropertyName("tgtttbchu")]
        public string Tgtttbchu { get; set; }

        [JsonPropertyName("tgtttbso")]
        public double ? Tgtttbso { get; set; }

        [JsonPropertyName("thdon")]
        public string Thdon { get; set; }

        [JsonPropertyName("thlap")]
        public int ? Thlap { get; set; }

        [JsonPropertyName("thttlphi")]
        public List<Lephi> Thttlphi { get; set; } = new List<Lephi>(); // Khởi tạo để tránh null

        [JsonPropertyName("thttltsuat")]
        public List<TaxRateTotal> Thttltsuat { get; set; } = new List<TaxRateTotal>(); // Khởi tạo để tránh null

        [JsonPropertyName("tlhdon")]
        public string Tlhdon { get; set; }

        [JsonPropertyName("ttcktmai")]
        public object Ttcktmai { get; set; } // Nullable object

        [JsonPropertyName("tthai")]
        public int Tthai { get; set; }

        [JsonPropertyName("ttkhac")]
        public List<CustomField> Ttkhac { get; set; } = new List<CustomField>(); // Khởi tạo để tránh null

        [JsonPropertyName("tttbao")]
        public int ? Tttbao { get; set; }

        [JsonPropertyName("ttttkhac")]
        public List<object> Ttttkhac { get; set; } = new List<object>(); // Khởi tạo để tránh null

        [JsonPropertyName("ttxly")]
        public int ? Ttxly { get; set; }

        [JsonPropertyName("tvandnkntt")]
        public string Tvandnkntt { get; set; }

        [JsonPropertyName("mhso")]
        public object Mhso { get; set; } // Nullable object

        [JsonPropertyName("ladhddt")]
        public int ? Ladhddt { get; set; }

        [JsonPropertyName("mkhang")]
        public string Mkhang { get; set; }

        [JsonPropertyName("nbsdthoai")]
        public string Nbsdthoai { get; set; }

        [JsonPropertyName("nbdctdtu")]
        public object Nbdctdtu { get; set; } // Nullable object

        [JsonPropertyName("nbfax")]
        public object Nbfax { get; set; } // Nullable object

        [JsonPropertyName("nbwebsite")]
        public object Nbwebsite { get; set; } // Nullable object

        [JsonPropertyName("nbcks")]
        public string Nbcks { get; set; } // JSON string lồng nhau

        [JsonPropertyName("nmsdthoai")]
        public object Nmsdthoai { get; set; } // Nullable object

        [JsonPropertyName("nmdctdtu")]
        public object Nmdctdtu { get; set; } // Nullable object

        [JsonPropertyName("nmcmnd")]
        public object Nmcmnd { get; set; } // Nullable object

        [JsonPropertyName("nmcks")]
        public object Nmcks { get; set; } // Nullable object

        [JsonPropertyName("bhphap")]
        public int? Bhphap { get; set; } // Nullable int

        [JsonPropertyName("hddunlap")]
        public object Hddunlap { get; set; } // Nullable object

        [JsonPropertyName("gchdgoc")]
        public object Gchdgoc { get; set; } // Nullable object

        [JsonPropertyName("tbhgtngay")]
        public object Tbhgtngay { get; set; } // Nullable object

        [JsonPropertyName("bhpldo")]
        public object Bhpldo { get; set; } // Nullable object

        [JsonPropertyName("bhpcbo")]
        public object Bhpcbo { get; set; } // Nullable object

        [JsonPropertyName("bhpngay")]
        public object Bhpngay { get; set; } // Nullable object

        [JsonPropertyName("tdlhdgoc")]
        public object Tdlhdgoc { get; set; } // Nullable object

        [JsonPropertyName("tgtphi")]
        public double? Tgtphi { get; set; } // Nullable object

        [JsonPropertyName("unhiem")]
        public object Unhiem { get; set; } // Nullable object

        [JsonPropertyName("mstdvnunlhdon")]
        public object Mstdvnunlhdon { get; set; } // Nullable object

        [JsonPropertyName("tdvnunlhdon")]
        public object Tdvnunlhdon { get; set; } // Nullable object

        [JsonPropertyName("nbmdvqhnsach")]
        public object Nbmdvqhnsach { get; set; } // Nullable object

        [JsonPropertyName("nbsqdinh")]
        public object Nbsqdinh { get; set; } // Nullable object

        [JsonPropertyName("nbncqdinh")]
        public object Nbncqdinh { get; set; } // Nullable object

        [JsonPropertyName("nbcqcqdinh")]
        public object Nbcqcqdinh { get; set; } // Nullable object

        [JsonPropertyName("nbhtban")]
        public object Nbhtban { get; set; } // Nullable object

        [JsonPropertyName("nmmdvqhnsach")]
        public object Nmmdvqhnsach { get; set; } // Nullable object

        [JsonPropertyName("nmddvchden")]
        public object Nmddvchden { get; set; } // Nullable object

        [JsonPropertyName("nmtgvchdtu")]
        public object Nmtgvchdtu { get; set; } // Nullable object

        [JsonPropertyName("nmtgvchdden")]
        public object Nmtgvchdden { get; set; } // Nullable object

        [JsonPropertyName("nbtnban")]
        public object Nbtnban { get; set; } // Nullable object

        [JsonPropertyName("dcdvnunlhdon")]
        public object Dcdvnunlhdon { get; set; } // Nullable object

        [JsonPropertyName("dksbke")]
        public object Dksbke { get; set; } // Nullable object

        [JsonPropertyName("dknlbke")]
        public object Dknlbke { get; set; } // Nullable object

        [JsonPropertyName("thtttoan")]
        public string Thtttoan { get; set; }

        [JsonPropertyName("msttcgp")]
        public string Msttcgp { get; set; }

        [JsonPropertyName("cqtcks")]
        public string Cqtcks { get; set; } // JSON string lồng nhau

        [JsonPropertyName("gchu")]
        public string Gchu { get; set; }

        [JsonPropertyName("kqcht")]
        public object Kqcht { get; set; } // Nullable object

        [JsonPropertyName("hdntgia")]
        public object Hdntgia { get; set; } // Nullable object

        [JsonPropertyName("tgtkcthue")]
        public object Tgtkcthue { get; set; } // Nullable object

        [JsonPropertyName("tgtkhac")]
        public object Tgtkhac { get; set; } // Nullable object

        [JsonPropertyName("nmshchieu")]
        public object Nmshchieu { get; set; } // Nullable object

        [JsonPropertyName("nmnchchieu")]
        public object Nmnchchieu { get; set; } // Nullable object

        [JsonPropertyName("nmnhhhchieu")]
        public object Nmnhhhchieu { get; set; } // Nullable object

        [JsonPropertyName("nmqtich")]
        public object Nmqtich { get; set; } // Nullable object

        [JsonPropertyName("ktkhthue")]
        public object Ktkhthue { get; set; } // Nullable object

        [JsonPropertyName("hdhhdvu")]
        public List<InvoiceLineItem> Hdhhdvu { get; set; } = new List<InvoiceLineItem>(); // Khởi tạo để tránh null

        [JsonPropertyName("qrcode")]
        public string Qrcode { get; set; }

        [JsonPropertyName("ttmstten")]
        public object Ttmstten { get; set; } // Nullable object

        [JsonPropertyName("ladhddtten")]
        public object Ladhddtten { get; set; } // Nullable object

        [JsonPropertyName("hdxkhau")]
        public object Hdxkhau { get; set; } // Nullable object

        [JsonPropertyName("hdxkptquan")]
        public object Hdxkptquan { get; set; } // Nullable object

        [JsonPropertyName("hdgktkhthue")]
        public object Hdgktkhthue { get; set; } // Nullable object

        [JsonPropertyName("hdonLquans")]
        public object HdonLquans { get; set; } // Nullable object

        [JsonPropertyName("tthdclquan")]
        public bool Tthdclquan { get; set; }

        [JsonPropertyName("pdndungs")]
        public object Pdndungs { get; set; } // Nullable object

        [JsonPropertyName("hdtbssrses")]
        public object Hdtbssrses { get; set; } // Nullable object

        [JsonPropertyName("hdTrung")]
        public object HdTrung { get; set; } // Nullable object

        [JsonPropertyName("isHDTrung")]
        public object IsHDTrung { get; set; } // Nullable object
    }

    // Lớp cho các trường tùy chỉnh (CustomField)
    public class CustomField
    {
        [JsonPropertyName("ttruong")]
        public string Ttruong { get; set; }

        [JsonPropertyName("kdlieu")]
        public string Kdlieu { get; set; }

        [JsonPropertyName("dlieu")]
        public string Dlieu { get; set; }
    }

    // Lớp cho tổng thuế suất (TaxRateTotal)
    public class TaxRateTotal
    {
        [JsonPropertyName("tsuat")]
        public string Tsuat { get; set; }

        [JsonPropertyName("thtien")]
        public double ? Thtien { get; set; }

        [JsonPropertyName("tthue")]
        public double ? Tthue { get; set; }

        [JsonPropertyName("gttsuat")]
        public object Gttsuat { get; set; } // Nullable object
    }

    // Lớp cho chi tiết hàng hóa/dịch vụ (InvoiceLineItem)
    public class InvoiceLineItem
    {
        [JsonPropertyName("idhdon")]
        public string Idhdon { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("dgia")]
        public double ? Dgia { get; set; }

        [JsonPropertyName("dvtinh")]
        public string Dvtinh { get; set; }

        [JsonPropertyName("ltsuat")]
        public string Ltsuat { get; set; }

        [JsonPropertyName("sluong")]
        public double ? Sluong { get; set; }

        [JsonPropertyName("stbchu")]
        public object Stbchu { get; set; } // Nullable object

        [JsonPropertyName("stckhau")]
        public object Stckhau { get; set; } // Nullable object

        [JsonPropertyName("stt")]
        public int ? Stt { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

        [JsonPropertyName("tchat")]
        public int ?  Tchat { get; set; }

        [JsonPropertyName("ten")]
        public string Ten { get; set; }

        [JsonPropertyName("thtcthue")]
        public object Thtcthue { get; set; } // Nullable object

        [JsonPropertyName("thtien")]
        public double ?Thtien { get; set; }

        [JsonPropertyName("tlckhau")]
        public object Tlckhau { get; set; } // Nullable object

        [JsonPropertyName("tsuat")]
        public double ? Tsuat { get; set; }

        [JsonPropertyName("tthue")]
        public object Tthue { get; set; } // Nullable object

        [JsonPropertyName("sxep")]
        public int ? Sxep { get; set; }

        [JsonPropertyName("ttkhac")]
        public List<CustomField> Ttkhac { get; set; } = new List<CustomField>(); // Khởi tạo để tránh null

        [JsonPropertyName("dvtte")]
        public object Dvtte { get; set; } // Nullable object

        [JsonPropertyName("tgia")]
        public object Tgia { get; set; } // Nullable object
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("tchat")]
        public int tchat { get; set; } // Nullable object

        [JsonPropertyName("tthhdtrung")]
        public List<object> Tthhdtrung { get; set; } = new List<object>(); // Khởi tạo để tránh null
    }
}
