using Microsoft.AspNetCore.Mvc;

namespace HeThongCuuHo.Helpers
{
    public static class AuthorizationHelper
    {
        public static bool KiemTraDangNhap(HttpContext httpContext)
        {
            return httpContext.Session.GetInt32("MaTaiKhoan") != null;
        }

        public static bool KiemTraVaiTro(HttpContext httpContext, params string[] vaiTroChoPhep)
        {
            string? vaiTro = httpContext.Session.GetString("VaiTro");

            if (string.IsNullOrEmpty(vaiTro))
                return false;

            return vaiTroChoPhep.Contains(vaiTro);
        }
    }
}