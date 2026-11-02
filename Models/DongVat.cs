using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongCuuHo.Models
{
    [Table("DongVat")]
    public class DongVat
    {
        [Key]
        public int MaDongVat { get; set; }

        public string Ten { get; set; } = string.Empty;

        public string Loai { get; set; } = string.Empty;

        public string Giong { get; set; } = string.Empty;

        public string GioiTinh { get; set; } = string.Empty;

        public int Tuoi { get; set; }

        public string TinhTrangSucKhoe { get; set; } = string.Empty;

        public string TrangThai { get; set; } = "Đang điều trị"; // Đang điều trị, Sẵn sàng nhận nuôi, Đã nhận nuôi

        public string? HinhAnh { get; set; } // Thêm trường HinhAnh để upload & hiển thị

        public DateTime NgayTiepNhan { get; set; } = DateTime.Now;

        [ForeignKey("BaoCao")]
        public int? MaBaoCao { get; set; } // int? cho phép null nếu tiếp nhận trực tiếp

        public virtual BaoCao? BaoCao { get; set; }

        [ForeignKey("TramCuuHo")]
        public int MaTram { get; set; }

        public virtual TramCuuHo? TramCuuHo { get; set; }

        public virtual ICollection<HoSoDieuTri>? HoSoYTes { get; set; }

        public virtual ICollection<DonNhanNuoi>? DonNhanNuois { get; set; }
    }
}