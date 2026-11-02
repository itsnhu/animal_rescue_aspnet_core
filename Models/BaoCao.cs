using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongCuuHo.Models
{
    public class BaoCao
    {
        [Key]
        public int MaBaoCao { get; set; }

        public int MaNguoiDung { get; set; }
        //public int? MaNguoiDung { get; set; }


        public string LoaiDongVat { get; set; } = string.Empty;

        [Display(Name = "Mức độ khẩn cấp")]
        public string MucDoKhanCap { get; set; } = "Bình thường";

        public string MoTa { get; set; } = string.Empty;

        public string? HinhAnh { get; set; }

        public double ViDo { get; set; }

        public double KinhDo { get; set; }

        public string DiaChi { get; set; } = string.Empty;

        public DateTime NgayBaoCao { get; set; }

        public string TrangThai { get; set; } = string.Empty;

        [ForeignKey(nameof(MaNguoiDung))]
        public virtual NguoiDung? NguoiDung { get; set; }

        public virtual CuuHo? CuuHo { get; set; }

        public virtual ICollection<DongVat> DongVats { get; set; }
            = new List<DongVat>();
    }
}