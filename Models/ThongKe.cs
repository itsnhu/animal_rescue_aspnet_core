using System.Collections.Generic;

namespace HeThongCuuHo.Models
{
    public class ThongKeViewModel
    {
        // Các con số tổng quan (KPIs)
        public int TongSoBaoCao { get; set; }
        public int CaCuuHoHoanThanh { get; set; }
        public int TongSoDongVat { get; set; }
        public int DongVatDaDuocNhanNuoi { get; set; }

        // Dữ liệu cho biểu đồ
        public List<ThongKeTheoThang> CaCuuHoTheoThang { get; set; } = new();
        public List<ThongKeTheoLoai> DongVatTheoLoai { get; set; } = new();
        public List<ThongKeTheoTrangThai> TrangThaiDongVat { get; set; } = new();
    }

    public class ThongKeTheoThang
    {
        public string Thang { get; set; } = string.Empty;
        public int SoLuong { get; set; }
    }

    public class ThongKeTheoLoai
    {
        public string TenLoai { get; set; } = string.Empty;
        public int SoLuong { get; set; }
    }

    public class ThongKeTheoTrangThai
    {
        public string TrangThai { get; set; } = string.Empty;
        public int SoLuong { get; set; }
    }
}