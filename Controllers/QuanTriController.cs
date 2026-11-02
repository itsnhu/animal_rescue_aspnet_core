using HeThongCuuHo.Helpers;
using Microsoft.AspNetCore.Mvc;


namespace HeThongCuuHo.Controllers
{
    public class QuanTriController : Controller
    {
        public IActionResult Index()
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}
