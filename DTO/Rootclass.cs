using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SaovietTax.DTO
{
    public class Thttltsuat
    {
        public string tsuat { get; set; }
        public double thtien { get; set; }
        public double tthue { get; set; }
        public object gttsuat { get; set; }
    }

    public class Ttttkhac
    {
        public string ttruong { get; set; }
        public string kdlieu { get; set; }
        public string dlieu { get; set; }
    }

    public class Cttkhac
    {
        public string ttruong { get; set; }
        public string kdlieu { get; set; }
        public string dlieu { get; set; }
    }

    public class Thttlphi
    {
        public object tlphi { get; set; }
        public object tphi { get; set; }
    }

    public class Nbttkhac
    {
        public string ttruong { get; set; }
        public string kdlieu { get; set; }
        public string dlieu { get; set; }
    }

    public class Data
    {
        public string nbmst { get; set; }
        public int khmshdon { get; set; }
        public string khhdon { get; set; }
        public int shdon { get; set; }
        public string cqt { get; set; }
        public List<Cttkhac> cttkhac { get; set; }
        public string dvtte { get; set; }
        public string hdon { get; set; }
        public string hsgcma { get; set; }
        public string hsgoc { get; set; }
        public int hthdon { get; set; }
        public int ? htttoan { get; set; }
        public string id { get; set; }
        public object idtbao { get; set; }
        public object khdon { get; set; }
        public object khhdgoc { get; set; }
        public object khmshdgoc { get; set; }
        public object lhdgoc { get; set; }
        public string mhdon { get; set; }
        public object mtdiep { get; set; }
        public string mtdtchieu { get; set; }
        public string nbdchi { get; set; }
        public object chma { get; set; }
        public object chten { get; set; }
        public object nbhdktngay { get; set; }
        public object nbhdktso { get; set; }
        public object nbhdso { get; set; }
        public object nblddnbo { get; set; }
        public object nbptvchuyen { get; set; }
        public string nbstkhoan { get; set; }
        public string nbten { get; set; }
        public string nbtnhang { get; set; }
        public object nbtnvchuyen { get; set; }
        public List<Nbttkhac> nbttkhac { get; set; }
        public DateTime ?ncma { get; set; }
        public DateTime ncnhat { get; set; }
        public string ngcnhat { get; set; }
        public DateTime ? nky { get; set; }
        public string nmdchi { get; set; }
        public string nmmst { get; set; }
        public object nmstkhoan { get; set; }
        public string nmten { get; set; }
        public object nmtnhang { get; set; }
        public object nmtnmua { get; set; }
        public List<object> nmttkhac { get; set; }
        public DateTime ntao { get; set; }
        public DateTime ntnhan { get; set; }
        public string pban { get; set; }
        public int?  ptgui { get; set; }
        public object shdgoc { get; set; }
        public int tchat { get; set; }
        public DateTime tdlap { get; set; }
        public double ? tgia { get; set; }
        public double? tgtcthue { get; set; }
        public double? tgtthue { get; set; }
        public string tgtttbchu { get; set; }
        public double tgtttbso { get; set; }
        public string thdon { get; set; }
        public int thlap { get; set; }
        public List<Thttlphi> thttlphi { get; set; }
        public List<Thttltsuat> thttltsuat { get; set; }
        public string tlhdon { get; set; }
        public double? ttcktmai { get; set; }
        public int tthai { get; set; }
        public List<Cttkhac> ttkhac { get; set; }
        public int ? tttbao { get; set; }
        public List<Ttttkhac> ttttkhac { get; set; }
        public int ttxly { get; set; }
        public string tvandnkntt { get; set; }
        public object mhso { get; set; }
        public int ? ladhddt { get; set; }
        public string mkhang { get; set; }
        public string nbsdthoai { get; set; }
        public object nbdctdtu { get; set; }
        public object nbfax { get; set; }
        public object nbwebsite { get; set; }
        public string nbcks { get; set; }
        public object nmsdthoai { get; set; }
        public object nmdctdtu { get; set; }
        public object nmcmnd { get; set; }
        public object nmcks { get; set; }
        public int ? bhphap { get; set; }
        public object hddunlap { get; set; }
        public object gchdgoc { get; set; }
        public object tbhgtngay { get; set; }
        public object bhpldo { get; set; }
        public object bhpcbo { get; set; }
        public object bhpngay { get; set; }
        public object tdlhdgoc { get; set; }
        public object tgtphi { get; set; }
        public object unhiem { get; set; }
        public object mstdvnunlhdon { get; set; }
        public object tdvnunlhdon { get; set; }
        public object nbmdvqhnsach { get; set; }
        public object nbsqdinh { get; set; }
        public object nbncqdinh { get; set; }
        public object nbcqcqdinh { get; set; }
        public object nbhtban { get; set; }
        public object nmmdvqhnsach { get; set; }
        public object nmddvchden { get; set; }
        public object nmtgvchdtu { get; set; }
        public object nmtgvchdden { get; set; }
        public object nbtnban { get; set; }
        public object dcdvnunlhdon { get; set; }
        public object dksbke { get; set; }
        public object dknlbke { get; set; }
        public string thtttoan { get; set; }
        public string msttcgp { get; set; }
        public string cqtcks { get; set; }
        public string gchu { get; set; }
        public object kqcht { get; set; }
        public object hdntgia { get; set; }
        public object tgtkcthue { get; set; }
        public double? tgtkhac { get; set; }
        public object nmshchieu { get; set; }
        public object nmnchchieu { get; set; }
        public object nmnhhhchieu { get; set; }
        public object nmqtich { get; set; }
        public object ktkhthue { get; set; }
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
        public object hdtbssrses { get; set; }
        public object hdTrung { get; set; }
        public object isHDTrung { get; set; }
    }
    public class RootObject
    {
        public List<Data> datas { get; set; }
        public int total { get; set; }
        public string state { get; set; }
        public int time { get; set; }
    }
}
