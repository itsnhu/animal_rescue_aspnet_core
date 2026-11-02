namespace HeThongCuuHo.Models
{
    public class TinTuc
    {
        public int MaTinTuc { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string? TomTat { get; set; }
        public string? NoiDung { get; set; }
        public string? HinhAnh { get; set; }

        // Thêm dấu ? để cho phép Null
        public DateTime? NgayDang { get; set; } = DateTime.Now;
    }
}