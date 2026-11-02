using HeThongCuuHo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongCuuHo.Controllers
{
    public class DonNhanNuoiController : Controller
    {
        private readonly HethongcuuhoDbContext _context;

        public DonNhanNuoiController(HethongcuuhoDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var ds = _context.DonNhanNuois
                .Include(d => d.DongVat)
                .Include(d => d.NguoiDung)
                .ToList();

            return View(ds);
        }

        public IActionResult ChiTiet(int id)
        {
            var don = _context.DonNhanNuois
                .Include(d => d.DongVat)
                .Include(d => d.NguoiDung)
                .FirstOrDefault(d => d.MaDon == id);

            if (don == null)
            {
                return NotFound();
            }

            return View(don);
        }

        public IActionResult Duyet(int id)
        {
            var don = _context.DonNhanNuois
                .FirstOrDefault(d => d.MaDon == id);

            if (don == null)
            {
                return NotFound();
            }

            // Duyệt đơn
            don.TrangThai = "Đã duyệt";

            // Cập nhật trạng thái động vật
            var dongVat = _context.DongVats
                .FirstOrDefault(d => d.MaDongVat == don.MaDongVat);

            if (dongVat != null)
            {
                dongVat.TrangThai = "Đã nhận nuôi";
            }

            // Từ chối các đơn khác của cùng động vật
            var cacDonKhac = _context.DonNhanNuois
                .Where(d => d.MaDongVat == don.MaDongVat
                         && d.MaDon != don.MaDon)
                .ToList();

            foreach (var item in cacDonKhac)
            {
                item.TrangThai = "Từ chối";
            }

            _context.SaveChanges();

            TempData["ThongBao"] = "Đã duyệt đơn nhận nuôi.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult TuChoi(int id)
        {
            var don = _context.DonNhanNuois
                .FirstOrDefault(d => d.MaDon == id);

            if (don == null)
            {
                return NotFound();
            }

            don.TrangThai = "Từ chối";

            _context.SaveChanges();

            TempData["ThongBao"] = "Đã từ chối đơn.";

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Create(int maDongVat)
        {
            var don = new DonNhanNuoi
            {
                MaDongVat = maDongVat,
                NgayDangKy = DateTime.Now,
                TrangThai = "Chờ duyệt"
            };

            return View(don);
        }

        [HttpPost]
        public IActionResult Create(DonNhanNuoi don)
        {
            if (ModelState.IsValid)
            {
                don.NgayDangKy = DateTime.Now;
                don.TrangThai = "Chờ duyệt";

                _context.DonNhanNuois.Add(don);
                _context.SaveChanges();

                TempData["ThongBao"] = "Gửi đơn nhận nuôi thành công!";

                return RedirectToAction(nameof(Index));
            }

            return View(don);
        }

    }
}