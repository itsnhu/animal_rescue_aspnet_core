using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongCuuHo.Models
{
    [Table("DieuTri")]
    public class HoSoDieuTri
    {
        [Key]
        [Column("MaHoSo")]
        public int MaHoSo { get; set; }

        [ForeignKey("DongVat")]
        [Column("MaDongVat")]
        public int MaDongVat { get; set; }

        [Column("NgayKham")]
        public DateTime NgayKham { get; set; }

        [Column("ChanDoan")]
        public string? ChanDoan { get; set; }

        [Column("DieuTri")]
        public string? DieuTri { get; set; }

        [Column("ChiPhi")]
        public decimal? ChiPhi { get; set; }

        [Column("TrangThai")]
        public string? TrangThai { get; set; }

        [Column("GhiChu")]
        public string? GhiChu { get; set; }

        public virtual DongVat? DongVat { get; set; }
    }
}