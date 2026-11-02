using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongCuuHo.Models
{
    public class DonNhanNuoi
    {
        [Key]
        public int MaDon { get; set; }

        [ForeignKey("DongVat")]
        public int MaDongVat { get; set; }

        [ForeignKey("NguoiDung")]
        public int MaNguoiDung { get; set; }

        [Required]
        public DateTime NgayDangKy { get; set; }

        [Required]
        public string LyDo { get; set; } = string.Empty;

        [Required]
        public string TrangThai { get; set; } = "Chờ duyệt";

        public virtual DongVat? DongVat { get; set; }

        public virtual NguoiDung? NguoiDung { get; set; }
    }
}