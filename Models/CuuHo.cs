using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongCuuHo.Models
{
    [Table("CuuHo")]
    public class CuuHo
    {
        [Key]
        public int MaCuuHo { get; set; }

        [ForeignKey(nameof(BaoCao))]
        public int MaBaoCao { get; set; }

        [ForeignKey(nameof(TinhNguyenVien))]
        public int MaTinhNguyenVien { get; set; }

        public DateTime? ThoiGianNhan { get; set; }

        public DateTime? ThoiGianDen { get; set; }

        public DateTime? ThoiGianHoanThanh { get; set; }

        public string TrangThai { get; set; } = string.Empty;

        public virtual BaoCao? BaoCao { get; set; }

        public virtual TinhNguyenVien? TinhNguyenVien { get; set; }
    }
}