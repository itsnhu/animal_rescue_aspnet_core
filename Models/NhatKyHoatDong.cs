using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongCuuHo.Models
{
    [Table("NhatKyHoatDong")]
    public class NhatKyHoatDong
    {
        [Key]
        public int MaNhatKy { get; set; }

        public int? MaNguoiDung { get; set; }

        [StringLength(100)]
        public string? TenNguoiDung { get; set; }

        [Required]
        [StringLength(100)]
        public string HanhDong { get; set; } = string.Empty;

        [StringLength(50)]
        public string? TenEntity { get; set; }

        public int? MaDoiTuong { get; set; }

        public string? ChiTiet { get; set; }

        [StringLength(20)]
        public string? TrangThai { get; set; } = "Thành công";

        public DateTime ThoiGian { get; set; } = DateTime.Now;

        [StringLength(45)]
        public string? DiaChiIP { get; set; }

        public string? TrinhDuyet { get; set; }

        [ForeignKey("MaNguoiDung")]
        public virtual NguoiDung? NguoiDung { get; set; }
    }
}