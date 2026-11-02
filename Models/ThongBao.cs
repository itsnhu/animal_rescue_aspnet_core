using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongCuuHo.Models
{
    [Table("ThongBao")]
    public class ThongBao
    {
        [Key]
        public int MaThongBao { get; set; }

        [ForeignKey("NguoiDung")]
        public int MaNguoiDung { get; set; }

        [StringLength(255)]
        public string? TieuDe { get; set; }

        public string? NoiDung { get; set; }

        // Tự động gán thời gian hiện tại khi khởi tạo đối tượng ThongBao
        public DateTime NgayGui { get; set; } = DateTime.Now;

        // Mặc định thông báo mới tạo sẽ ở trạng thái chưa đọc (false)
        public bool DaDoc { get; set; } = false;

        [StringLength(50)]
        public string? Loai { get; set; } // Ví dụ: "BaoCao", "NhanNuoi", "TinhNguyenVien"

        public virtual NguoiDung? NguoiDung { get; set; }
    }
}