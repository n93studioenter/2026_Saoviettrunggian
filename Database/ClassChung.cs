using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaovietTax.Database
{
    public class TbImport
    {
        public bool IsAdd { get; set; } 
        public int ID { get; set; }
        public string SHDon { get; set; }
        public string KHHDon { get; set; }
        public string NCC { get; set; }
        public DateTime NLap { get; set; }
        public string Ten { get; set; }
        public string Noidung { get; set; }
        public string TKCo { get; set; }
        public string TKNo { get; set; }
        public string TkThue { get; set; }
        public string Mst { get; set; }
        public double Status { get; set; }
        public string  Ngaytao { get; set; }
        public double TongTien { get; set; }
        public double Vat { get; set; }
        public string Vat2 { get; set; }
        public string Vat3 { get; set; }
        public string SohieuTP { get; set; }
        public string TPhi { get; set; }
        public double TgTCThue { get; set; }
        public double TgTCThue1 { get; set; }   
        public double TgTCThue2 { get; set; }
        public double TgTCThue3 { get; set; }
        public double TgTThue { get; set; }
        public double TVat3 { get; set; }
        public double TVat2 { get; set; }
        public double TVat { get; set; }
        public string InvoiceType { get; set; }
        public string IsHaschild { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public string Macdinhstatus { get; set; }
        public string Khmshdon { get; set; }
        public string idhoadon { get; set; }    
        public double IsImport { get; set; }
        public string hdon { get; set; }
        public int IsDieuchinh { get; set; }
        
        public List<TbImportDetail> tbImportDetails { get; set; }=new List<TbImportDetail>();   
    }
    public class TbImportDetail
    {
        public int ID { get; set; }
        public int ParentId { get; set; }
        public string SoHieu { get; set; }
        public double Soluong { get; set; }
        public double Dongia { get; set; }
        public string DVT { get; set; }
        public string Ten { get; set; }
        public string MaCT { get; set; }
        public string TKNo { get; set; }
        public string TKCo { get; set; }
        public double TTien { get; set; }
        public double SoPSGoc { get; set; }
        public double Percent { get; set; }
        public int Tchat { get; set; }
        public double Vat { get; set; } 
    }
    public class KhachHang
    {
        public int MaSo { get; set; }
        public int MaPhanLoai { get; set; }
        public string SoHieu { get; set; }
        public string Ten { get; set; }
        public string DiaChi { get; set; }
        public string MST { get; set; }
    }
    public class ChungTu
    {
        public int MaSo { get; set; }
        public int MaCT { get; set; }
        public int MaLoai { get; set; }
        public int ThangCT { get; set; }
        public string SoHieu { get; set; }
        public DateTime NgayCT { get; set; }
        public DateTime NgayGS { get; set; }
        public DateTime NgayTL { get; set; }
        public string DienGiai { get; set; }
        public int MaTKNo { get; set; }
        public int MaTKCo { get; set; }
        public double SoPS { get; set; }
        public int MaTKTCNo { get; set; }
        public int MaTKTCCo { get; set; }
        public int MaVattu { get; set; }
    }
    public class HoadonCT
    {
        public int MaSo { get; set; }
        public int Loai { get; set; }
        public int MaKhachHang { get; set; }
        public string MST { get; set; }
        public string KyHieu { get; set; }
        public string SoHD { get; set; }
        public DateTime NgayPH { get; set; }
        public double ThanhTien { get; set; }
        List<ChungTu> chungTus { get; set; } = new List<ChungTu>();
    }
    public class HoaDon
    {
        public int MaSo { get; set; }
        public int Loai { get; set; }
        public int MaKhachHang { get; set; }
        public string KyHieu { get; set; }
        public string SoHD { get; set; }
        public DateTime NgayPH { get; set; }
        public double ThanhTien { get; set; }
    }
    public class VatTu
    {
        public int MaSo { get; set; }
        public int MaPhanLoai { get; set; }
        public string TenMaPhanLoai { get; set; }
        public string SoHieu { get; set; }
        public string GhiChu { get; set; }
        public string TenVattu { get; set; }
        public string DonVi { get; set; }
        public double Dongia { get; set; }
        public double SoLuong { get; set; }
        public double ThanhTien { get; set; }
    }
    public class TbRegister
    {
        public string Name { get; set; }
        public string Hoadonpath { get; set; }
        public string Dbpath { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Tokken { get; set; }
        public DateTime TimeTokken { get; set; }
    }
    public class PhanLoaiKhachHang
    {
        public int MaSo { get; set; }
        public string SoHieu { get; set; }
        public string TenPhanLoai { get; set; }
        public double VAT { get; set; }
        public int PLCon { get; set; }
        public int PLCha { get; set; }
        public int Cap { get; set; }
    }
    public class PhanLoaiVattu
    {
        public int MaSo { get; set; }
        public string SoHieu { get; set; }
        public string TenPhanLoai { get; set; }
    }
    public class HeThongTK
    {
        public int MaSo { get; set; }
        public string SoHieu { get; set; }
        public int Cap { get; set; }
        public string Ten { get; set; }
        public int Kieu { get; set; }
        public int Loai { get; set; }
        public int TKCon { get; set; }
        public int MaNT { get; set; }
        public double DuNo_0 { get; set; }
        public double DuCo_0 { get; set; }
        public double No_1 { get; set; }
        public double Co_1 { get; set; }
        public double No_1_NT { get; set; }
        public double Co_1_NT { get; set; }
        public double DuNo_1 { get; set; }
        public double DuCo_1 { get; set; }
        public double DuNT_1 { get; set; }
        public double No_2 { get; set; }
        public double Co_2 { get; set; }
        public double No_2_NT { get; set; }
        public double Co_2_NT { get; set; }
        public double DuNo_2 { get; set; }
        public double DuCo_2 { get; set; }
        public double DuNT_2 { get; set; }
        public double No_3 { get; set; }
        public double Co_3 { get; set; }
        public double No_3_NT { get; set; }
        public double Co_3_NT { get; set; }
        public double DuNo_3 { get; set; }
        public double DuCo_3 { get; set; }
        public double DuNT_3 { get; set; }
        public double No_4 { get; set; }
        public double Co_4 { get; set; }
        public double No_4_NT { get; set; }
        public double Co_4_NT { get; set; }
        public double DuNo_4 { get; set; }
        public double DuCo_4 { get; set; }
        public double DuNT_4 { get; set; }
        public double No_5 { get; set; }
        public double Co_5 { get; set; }
        public double No_5_NT { get; set; }
        public double Co_5_NT { get; set; }
        public double DuNo_5 { get; set; }
        public double DuCo_5 { get; set; }
        public double DuNT_5 { get; set; }
        public double No_6 { get; set; }
        public double Co_6 { get; set; }
        public double No_6_NT { get; set; }
        public double Co_6_NT { get; set; }
        public double DuNo_6 { get; set; }
        public double DuCo_6 { get; set; }
        public double DuNT_6 { get; set; }
        public double No_7 { get; set; }
        public double Co_7 { get; set; }
        public double No_7_NT { get; set; }
        public double Co_7_NT { get; set; }
        public double DuNo_7 { get; set; }
        public double DuCo_7 { get; set; }
        public double DuNT_7 { get; set; }
        public double No_8 { get; set; }
        public double Co_8 { get; set; }
        public double No_8_NT { get; set; }
        public double Co_8_NT { get; set; }
        public double DuNo_8 { get; set; }
        public double DuCo_8 { get; set; }
        public double DuNT_8 { get; set; }
        public double No_9 { get; set; }
        public double Co_9 { get; set; }
        public double No_9_NT { get; set; }
        public double Co_9_NT { get; set; }
        public double DuNo_9 { get; set; }
        public double DuCo_9 { get; set; }
        public double DuNT_9 { get; set; }
        public double No_10 { get; set; }
        public double Co_10 { get; set; }
        public double No_10_NT { get; set; }
        public double Co_10_NT { get; set; }
        public double DuNo_10 { get; set; }
        public double DuCo_10 { get; set; }
        public double DuNT_10 { get; set; }
        public double No_11 { get; set; }
        public double Co_11 { get; set; }
        public double No_11_NT { get; set; }
        public double Co_11_NT { get; set; }
        public double DuNo_11 { get; set; }
        public double DuCo_11 { get; set; }
        public double DuNT_11 { get; set; }
        public double No_12 { get; set; }
        public double Co_12 { get; set; }
        public double No_12_NT { get; set; }
        public double Co_12_NT { get; set; }
        public double DuNo_12 { get; set; }
        public double DuCo_12 { get; set; }
        public double DuNT_12 { get; set; }
        public int TK_ID { get; set; }
        public int TK_ID2 { get; set; }
        public double SoDuMax { get; set; }
        public double SoDuMin { get; set; }
        public int TKCha0 { get; set; }
        public int TKCha1 { get; set; }
        public int TKCha2 { get; set; }
        public int TKCha3 { get; set; }
        public int TKCha4 { get; set; }
        public int TKCha5 { get; set; }
        public int MaTC { get; set; }
        public double KC_N { get; set; }
        public double KC_C { get; set; }
        public double DuNo { get; set; }
        public double DuCo { get; set; }
        public string GhiChu { get; set; }
        public int CapDuoi { get; set; }
        public string TenE { get; set; }
        public int TK_ID3 { get; set; }
        public string TenDA { get; set; }
        public string NhomDA { get; set; }
        public string DiaDiem { get; set; }
        public double DuToan { get; set; }
        public double Von1 { get; set; }
        public double Von2 { get; set; }
        public double Von3 { get; set; }
        public DateTime NgayKC { get; set; }
        public DateTime NgayHT { get; set; }
        public double PSNLK { get; set; }
        public double PSCLK { get; set; }
        public double PSNLK2005 { get; set; }
        public double PSCLK2005 { get; set; }
        public double PSNLK2006 { get; set; }
        public double PSCLK2006 { get; set; }
        public double PSNLK2008 { get; set; }
        public double PSCLK2008 { get; set; }
        public double PSNLK2009 { get; set; }
        public double PSCLK2009 { get; set; }
        public double PSNLK2010 { get; set; }
        public double PSCLK2010 { get; set; }
        public double PSNLK2011 { get; set; }
        public double PSCLK2011 { get; set; }
        public double PSNLK2012 { get; set; }
        public double PSCLK2012 { get; set; }
        public double PSNLK2013 { get; set; }
        public double PSCLK2013 { get; set; }
        public double PSNLK2014 { get; set; }
        public double PSCLK2014 { get; set; }
        public string THEMMOI { get; set; }
        public double PSNLK2018 { get; set; }
        public double PSCLK2018 { get; set; }
        public double PSNLK2019 { get; set; }
        public double PSCLK2019 { get; set; }
        public double PSNLK2020 { get; set; }
        public double PSCLK2020 { get; set; }
        public double PSNLK2021 { get; set; }
        public double PSCLK2021 { get; set; }
        public double PSNLK2022 { get; set; }
        public double PSCLK2022 { get; set; }
        public double PSNLK2023 { get; set; }
        public double PSCLK2023 { get; set; }
    }
    public class PhanLoai154
    {
        public int MaSo { get; set; }
        public string SoHieu { get; set; }
        public string TenPhanLoai { get; set; }
        public int PLCon { get; set; }
        public int PLCha { get; set; }
        public int Cap { get; set; }
    }
    public class TP154
    {
        public int MaSo { get; set; }
        public int MaPhanLoai { get; set; }
        public string SoHieu { get; set; }
        public string TenVattu { get; set; }
        public string DonVi { get; set; }
        public string GhiChu { get; set; }
        public double DK { get; set; }
        public double DK1 { get; set; }
        public double CK1 { get; set; }
        public double CPMVL { get; set; }
        public double CPNC { get; set; }
        public double CPM { get; set; }
        public double CPKH_1 { get; set; }
        public double CPSXC_1 { get; set; }
        public double CPKH_2 { get; set; }
        public double CPSXC_2 { get; set; }
        public double CPKH_3 { get; set; }
        public double CPSXC_3 { get; set; }
        public double CPKH_4 { get; set; }
        public double CPSXC_4 { get; set; }
        public double CPKH_5 { get; set; }
        public double CPSXC_5 { get; set; }
        public double CPKH_6 { get; set; }
        public double CPSXC_6 { get; set; }
        public double CPKH_7 { get; set; }
        public double CPSXC_7 { get; set; }
        public double CPKH_8 { get; set; }
        public double CPSXC_8 { get; set; }
        public double CPKH_9 { get; set; }
        public double CPSXC_9 { get; set; }
        public double CPKH_10 { get; set; }
        public double CPSXC_10 { get; set; }
        public double CPKH_11 { get; set; }
        public double CPSXC_11 { get; set; }
        public double CPKH_12 { get; set; }
        public double CPSXC_12 { get; set; }
        public double CPBH1 { get; set; }
        public double CPQL1 { get; set; }
        public double CPBHTT1 { get; set; }
        public double CPQLTT1 { get; set; }
        public double CPSXCTT1 { get; set; }
        public double CPNVLPB1 { get; set; }
        public double CPNCPB1 { get; set; }
        public double CPMPB1 { get; set; }
        public double CPBH2 { get; set; }
        public double CPQL2 { get; set; }
        public double CPBHTT2 { get; set; }
        public double CPQLTT2 { get; set; }
        public double CPSXCTT2 { get; set; }
        public double CPNVLPB2 { get; set; }
        public double CPNCPB2 { get; set; }
        public double CPMPB2 { get; set; }
        public double CPBH3 { get; set; }
        public double CPQL3 { get; set; }
        public double CPBHTT3 { get; set; }
        public double CPQLTT3 { get; set; }
        public double CPSXCTT3 { get; set; }
        public double CPNVLPB3 { get; set; }
        public double CPNCPB3 { get; set; }
        public double CPMPB3 { get; set; }
        public double CPBH4 { get; set; }
        public double CPQL4 { get; set; }
        public double CPBHTT4 { get; set; }
        public double CPQLTT4 { get; set; }
        public double CPSXCTT4 { get; set; }
        public double CPNVLPB4 { get; set; }
        public double CPNCPB4 { get; set; }
        public double CPMPB4 { get; set; }
        public double CPBH5 { get; set; }
        public double CPQL5 { get; set; }
        public double CPBHTT5 { get; set; }
        public double CPQLTT5 { get; set; }
        public double CPSXCTT5 { get; set; }
        public double CPNVLPB5 { get; set; }
        public double CPNCPB5 { get; set; }
        public double CPMPB5 { get; set; }
        public double CPBH6 { get; set; }
        public double CPQL6 { get; set; }
        public double CPBHTT6 { get; set; }
        public double CPQLTT6 { get; set; }
        public double CPSXCTT6 { get; set; }
        public double CPNVLPB6 { get; set; }
        public double CPNCPB6 { get; set; }
        public double CPMPB6 { get; set; }
        public double CPBH7 { get; set; }
        public double CPQL7 { get; set; }
        public double CPBHTT7 { get; set; }
        public double CPQLTT7 { get; set; }
        public double CPSXCTT7 { get; set; }
        public double CPNVLPB7 { get; set; }
        public double CPNCPB7 { get; set; }
        public double CPMPB7 { get; set; }
        public double CPBH8 { get; set; }
        public double CPQL8 { get; set; }
        public double CPBHTT8 { get; set; }
        public double CPQLTT8 { get; set; }
        public double CPSXCTT8 { get; set; }
        public double CPNVLPB8 { get; set; }
        public double CPNCPB8 { get; set; }
        public double CPMPB8 { get; set; }
        public double CPBH9 { get; set; }
        public double CPQL9 { get; set; }
        public double CPBHTT9 { get; set; }
        public double CPQLTT9 { get; set; }
        public double CPSXCTT9 { get; set; }
        public double CPNVLPB9 { get; set; }
        public double CPNCPB9 { get; set; }
        public double CPMPB9 { get; set; }
        public double CPBH10 { get; set; }
        public double CPQL10 { get; set; }
        public double CPBHTT10 { get; set; }
        public double CPQLTT10 { get; set; }
        public double CPSXCTT10 { get; set; }
        public double CPNVLPB10 { get; set; }
        public double CPNCPB10 { get; set; }
        public double CPMPB10 { get; set; }
        public double CPBH11 { get; set; }
        public double CPQL11 { get; set; }
        public double CPBHTT11 { get; set; }
        public double CPQLTT11 { get; set; }
        public double CPSXCTT11 { get; set; }
        public double CPNVLPB11 { get; set; }
        public double CPNCPB11 { get; set; }
        public double CPMPB11 { get; set; }
        public double CPBH12 { get; set; }
        public double CPQL12 { get; set; }
        public double CPBHTT12 { get; set; }
        public double CPQLTT12 { get; set; }
        public double CPSXCTT12 { get; set; }
        public double CPNVLPB12 { get; set; }
        public double CPNCPB12 { get; set; }
        public double CPMPB12 { get; set; }
        public double DT { get; set; }
        public double CPTC1 { get; set; }
        public double CPTCTT1 { get; set; }
        public double CPTC2 { get; set; }
        public double CPTCTT2 { get; set; }
        public double CPTC3 { get; set; }
        public double CPTCTT3 { get; set; }
        public double CPTC4 { get; set; }
        public double CPTCTT4 { get; set; }
        public double CPTC5 { get; set; }
        public double CPTCTT5 { get; set; }
        public double CPTC6 { get; set; }
        public double CPTCTT6 { get; set; }
        public double CPTC7 { get; set; }
        public double CPTCTT7 { get; set; }
        public double CPTC8 { get; set; }
        public double CPTCTT8 { get; set; }
        public double CPTC9 { get; set; }
        public double CPTCTT9 { get; set; }
        public double CPTC10 { get; set; }
        public double CPTCTT10 { get; set; }
        public double CPTC11 { get; set; }
        public double CPTCTT11 { get; set; }
        public double CPTC12 { get; set; }
        public double CPTCTT12 { get; set; }
        public double CPTC { get; set; }
        public int MaTK { get; set; }
        public double KPB { get; set; }
        public double SanLuong { get; set; }
    }
    public class License
    {
        public string TenCty { get; set; }

        public string DiaChi { get; set; }  
        public string MaSoThue { get; set; }    
    }
}