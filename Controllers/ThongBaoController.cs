using HeThongCuuHo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongCuuHo.Controllers
{
    public class ThongBaoController : Controller
    {
        private readonly HethongcuuhoDbContext _context;

        public ThongBaoController(HethongcuuhoDbContext context)
        {
            _context = context;
        }

        // Danh sách thông báo
        public IActionResult Index()
        {
            int maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung") ?? 0;

            string? vaiTro = HttpContext.Session.GetString("VaiTro");

            var ds = _context.ThongBaos
                .Include(x => x.NguoiDung)
                .AsQueryable();

            // Admin xem tất cả
            if (vaiTro != "Admin")
            {
                ds = ds.Where(x => x.MaNguoiDung == maNguoiDung);
            }

            return View(
                ds.OrderByDescending(x => x.NgayGui).ToList()
            );
        }

        // Đếm thông báo chưa đọc
        public IActionResult DemChuaDoc()
        {
            int maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung") ?? 0;

            string? vaiTro = HttpContext.Session.GetString("VaiTro");

            int dem;

            if (vaiTro == "Admin")
            {
                dem = _context.ThongBaos
                    .Count(x => !x.DaDoc);
            }
            else
            {
                dem = _context.ThongBaos
                    .Count(x =>
                        x.MaNguoiDung == maNguoiDung &&
                        !x.DaDoc);
            }

            return Content(dem.ToString());
        }
        // Trả về 5 thông báo mới nhất
        public IActionResult LayThongBaoMoi()
        {
            int maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung") ?? 0;

            string? vaiTro = HttpContext.Session.GetString("VaiTro");

            var ds = _context.ThongBaos.AsQueryable();

            if (vaiTro != "Admin")
            {
                ds = ds.Where(x => x.MaNguoiDung == maNguoiDung);
            }

            var ketQua = ds
                .OrderByDescending(x => x.NgayGui)
                .Take(5)
                .Select(x => new
                {
                    x.MaThongBao,
                    x.NoiDung,
                    x.NgayGui,
                    x.DaDoc
                })
                .ToList();

            return Json(ketQua);
        }

        // Đánh dấu đã đọc
        public IActionResult DaDoc(int id)
        {
            int maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung") ?? 0;

            string? vaiTro = HttpContext.Session.GetString("VaiTro");

            var tb = _context.ThongBaos.Find(id);

            if (tb == null)
                return RedirectToAction(nameof(Index));

            if (vaiTro != "Admin" &&
                tb.MaNguoiDung != maNguoiDung)
            {
                return RedirectToAction(nameof(Index));
            }

            tb.DaDoc = true;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult DaDocTatCa()
        {
            int maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung") ?? 0;

            string? vaiTro = HttpContext.Session.GetString("VaiTro");

            var ds = _context.ThongBaos.AsQueryable();

            if (vaiTro != "Admin")
            {
                ds = ds.Where(x => x.MaNguoiDung == maNguoiDung);
            }

            foreach (var item in ds)
            {
                item.DaDoc = true;
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        private void TaoThongBao(int maNguoiDung, string noiDung)
        {
            ThongBao tb = new ThongBao
            {
                MaNguoiDung = maNguoiDung,
                NoiDung = noiDung,
                NgayGui = DateTime.Now,
                DaDoc = false
            };

            _context.ThongBaos.Add(tb);
            _context.SaveChanges();
        }
    }
}