using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongCuuHo.Models
{
    [Table("LuuTru")]
    public class LuuTru
    {
        [Key]
        public int MaLuuTru { get; set; }

        [ForeignKey("DongVat")]
        public int MaDongVat { get; set; }

        [ForeignKey("TramCuuHo")]
        public int MaTram { get; set; }

        public DateTime NgayBatDau { get; set; }

        public DateTime? NgayKetThuc { get; set; }

        public string TrangThai { get; set; }

        public string? GhiChu { get; set; }

        public virtual DongVat? DongVat { get; set; }

        public virtual TramCuuHo? TramCuuHo { get; set; }
    }
}