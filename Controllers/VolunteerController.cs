using Microsoft.AspNetCore.Mvc;

namespace HeThongCuuHo.Controllers
{
    public class VolunteerController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("VaiTro") != "TinhNguyenVien")
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }
    }
}