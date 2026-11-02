using HeThongCuuHo.Models;
using Microsoft.AspNetCore.Http;

namespace HeThongCuuHo.Helpers
{
    public static class NhatKyHelper
    {
        public static void GhiLog(
            HethongcuuhoDbContext context,
            HttpContext httpContext,
            string hanhDong,
            string? chiTiet = null,
            string? tenEntity = null,
            int? maDoiTuong = null,
            string trangThai = "Thành công",
            string? tenNguoiDungThu = null)
        {
            try
            {
                int? maNguoiDung = httpContext.Session.GetInt32("MaNguoiDung");
                string? tenNguoiDungSession = httpContext.Session.GetString("TenDangNhap")
                                              ?? httpContext.Session.GetString("TenNguoiDung");

                // Lấy tên tài khoản từ Session hoặc từ tham số truyền vào (trường hợp đăng nhập thất bại)
                string? tenHienThi = !string.IsNullOrEmpty(tenNguoiDungSession) ? tenNguoiDungSession : tenNguoiDungThu;

                var log = new NhatKyHoatDong
                {
                    MaNguoiDung = (maNguoiDung.HasValue && maNguoiDung.Value > 0) ? maNguoiDung : null,
                    TenNguoiDung = tenHienThi,
                    HanhDong = hanhDong,
                    TenEntity = tenEntity,
                    MaDoiTuong = maDoiTuong,
                    ChiTiet = chiTiet,
                    TrangThai = trangThai,
                    ThoiGian = DateTime.Now,
                    DiaChiIP = httpContext.Connection.RemoteIpAddress?.ToString(),
                    TrinhDuyet = httpContext.Request.Headers["User-Agent"].ToString()
                };

                context.NhatKyHoatDongs.Add(log);
                context.SaveChanges();
            }
            catch
            {
                // Bỏ qua lỗi ghi log để không gián đoạn ứng dụng
            }
        }
    }
}