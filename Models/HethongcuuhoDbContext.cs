using Microsoft.EntityFrameworkCore;

namespace HeThongCuuHo.Models
{
    public class HethongcuuhoDbContext : DbContext
    {
        public HethongcuuhoDbContext(DbContextOptions<HethongcuuhoDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<BaoCao> BaoCaos { get; set; }
        public DbSet<CuuHo> CuuHos { get; set; }
        public DbSet<DongVat> DongVats { get; set; }
        public DbSet<TinhNguyenVien> TinhNguyenViens { get; set; }
        public DbSet<HoSoDieuTri> HoSoYTes { get; set; }
        public DbSet<LuuTru> LuuTrus { get; set; }
        public DbSet<TramCuuHo> TramCuuHos { get; set; }
        public DbSet<DonNhanNuoi> DonNhanNuois { get; set; }
        public DbSet<ThongBao> ThongBaos { get; set; }
        public DbSet<NhatKyHoatDong> NhatKyHoatDongs { get; set; }

        public DbSet<TinTuc> TinTucs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaiKhoan>().ToTable("TaiKhoan");
            modelBuilder.Entity<NguoiDung>().ToTable("NguoiDung");
            modelBuilder.Entity<BaoCao>().ToTable("BaoCao");
            modelBuilder.Entity<CuuHo>().ToTable("CuuHo");
            modelBuilder.Entity<DongVat>().ToTable("DongVat");
            modelBuilder.Entity<TinhNguyenVien>().ToTable("TinhNguyenVien");
            modelBuilder.Entity<HoSoDieuTri>().ToTable("DieuTri");
            modelBuilder.Entity<TramCuuHo>().ToTable("TramCuuHo");
            //modelBuilder.Entity<TinNhanNuoi>().ToTable("DonNhanNuoi");
            modelBuilder.Entity<DonNhanNuoi>().ToTable("DonNhanNuoi");
            modelBuilder.Entity<LuuTru>().ToTable("LuuTru");
            modelBuilder.Entity<ThongBao>().ToTable("ThongBao");
            modelBuilder.Entity<TinTuc>().HasKey(t => t.MaTinTuc);

        }
    }
}