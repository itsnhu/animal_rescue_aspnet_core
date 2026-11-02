using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongCuuHo.Models
{
    [Table("NguoiDung")] 
    public class NguoiDung
    {
        [Key]
        public int MaNguoiDung { get; set; }

        public string? HoTen { get; set; }

        public string? SoDienThoai { get; set; }

        public string? Email { get; set; }

        public string? DiaChi { get; set; }

        // BỔ SUNG THUỘC TÍNH NÀY
        public string? AnhDaiDien { get; set; }

        [ForeignKey("TaiKhoan")]
        public int? MaTaiKhoan { get; set; }

        public virtual TaiKhoan? TaiKhoan { get; set; }

        public virtual ICollection<BaoCao>? BaoCaos { get; set; }

        public virtual ICollection<DonNhanNuoi>? DonNhanNuois { get; set; }

        public virtual ICollection<NhatKyHoatDong>? NhatKyHoatDongs { get; set; }
    }
}