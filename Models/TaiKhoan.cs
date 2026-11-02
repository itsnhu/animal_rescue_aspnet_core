using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongCuuHo.Models
{
        [Table("TaiKhoan")]
        public class TaiKhoan
        {
            [Key]
            public int MaTaiKhoan { get; set; }

            [Required]
            public string TenDangNhap { get; set; }

            [Required]
            public string MatKhau { get; set; }

            public string VaiTro { get; set; }

            public bool TrangThai { get; set; }

            // Navigation
            public virtual ICollection<NguoiDung>? NguoiDungs { get; set; }
        }
    }
