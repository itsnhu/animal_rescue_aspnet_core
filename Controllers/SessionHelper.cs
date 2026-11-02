using Microsoft.AspNetCore.Http;

namespace HeThongCuuHo.Helpers
{
    public static class SessionHelper
    {
        public static bool DaDangNhap(HttpContext context)
        {
            return context.Session.GetInt32("MaTaiKhoan") != null;
        }

        public static bool LaAdmin(HttpContext context)
        {
            return context.Session.GetString("VaiTro") == "Admin";
        }

        public static bool LaTinhNguyenVien(HttpContext context)
        {
            return context.Session.GetString("VaiTro") == "TinhNguyenVien";
        }

        public static bool LaNguoiDung(HttpContext context)
        {
            return context.Session.GetString("VaiTro") == "NguoiDung";
        }
    }
}