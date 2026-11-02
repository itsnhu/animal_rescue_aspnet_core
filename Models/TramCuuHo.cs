using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongCuuHo.Models
{
    [Table("TramCuuHo")]
    public class TramCuuHo
    {
        [Key]
        public int MaTram { get; set; }

        [Required]
        public string TenTram { get; set; } = string.Empty;

        [Required]
        public string DiaChi { get; set; } = string.Empty;

        public string SoDienThoai { get; set; } = string.Empty;

        public int SucChua { get; set; }

        public double ViDo { get; set; }

        public double KinhDo { get; set; }

        public virtual ICollection<DongVat>? DongVats { get; set; }
    }
}