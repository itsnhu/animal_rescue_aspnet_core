using HeThongCuuHo.Helpers;
using HeThongCuuHo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongCuuHo.Controllers
{
    public class TramCuuHoController : Controller
    {
        private readonly HethongcuuhoDbContext _context;

        public TramCuuHoController(HethongcuuhoDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string tuKhoa)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
                return RedirectToAction("Index", "Login");

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin"))
                return RedirectToAction("Index", "Home");

            var ds = _context.TramCuuHos
                .Include(x => x.DongVats)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                ds = ds.Where(x =>
                    x.TenTram.Contains(tuKhoa) ||
                    x.DiaChi.Contains(tuKhoa));
            }

            ViewBag.TuKhoa = tuKhoa;

            return View(ds.ToList());
        }

        [HttpGet]
        public IActionResult Them()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Them(TramCuuHo model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.TramCuuHos.Add(model);

            _context.SaveChanges();

            TempData["ThongBao"] = "Đã thêm trạm cứu hộ.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Sua(int id)
        {
            var tram = _context.TramCuuHos.Find(id);

            if (tram == null)
                return NotFound();

            return View(tram);
        }

        [HttpPost]
        public IActionResult Sua(TramCuuHo model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.TramCuuHos.Update(model);

            _context.SaveChanges();

            TempData["ThongBao"] = "Cập nhật thành công.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ChiTiet(int id)
        {
            var tram = _context.TramCuuHos
                .Include(x => x.DongVats)
                .FirstOrDefault(x => x.MaTram == id);

            if (tram == null)
                return NotFound();

            return View(tram);
        }
    }
}