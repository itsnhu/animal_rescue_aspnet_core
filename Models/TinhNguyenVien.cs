using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongCuuHo.Models
{
    public class TinhNguyenVien
    {
        [Key]
        public int MaTinhNguyenVien { get; set; }

        public string HoTen { get; set; }

        public string SoDienThoai { get; set; }

        public double ViDo { get; set; }

        public double KinhDo { get; set; }

        public string TrangThai { get; set; }

        // Thêm
        [ForeignKey("NguoiDung")]
        public int MaNguoiDung { get; set; }

        public virtual NguoiDung? NguoiDung { get; set; }

        public virtual ICollection<CuuHo>? CuuHos { get; set; }
    }
}
