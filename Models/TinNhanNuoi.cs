using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongCuuHo.Models
{
    public class TinNhanNuoi
    {
        [Key]
        public int MaDon { get; set; }

        [ForeignKey("DongVat")]
        public int MaDongVat { get; set; }

        [ForeignKey("NguoiDung")]
        public int MaNguoiDung { get; set; }

        public DateTime NgayDangKy { get; set; }

        public string LyDo { get; set; }

        public string TrangThai { get; set; }

        public virtual DongVat? DongVat { get; set; }

        public virtual NguoiDung? NguoiDung { get; set; }
    }
}
