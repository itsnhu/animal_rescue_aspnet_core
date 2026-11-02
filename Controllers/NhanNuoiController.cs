using HeThongCuuHo.Helpers;
using HeThongCuuHo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongCuuHo.Controllers
{
    public class NhanNuoiController : Controller
    {
        private readonly HethongcuuhoDbContext _context;

        public NhanNuoiController(HethongcuuhoDbContext context)
        {
            _context = context;
        }

        // Danh sách đơn nhận nuôi
        public IActionResult Index()
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            string? vaiTro = HttpContext.Session.GetString("VaiTro");

            var ds = _context.DonNhanNuois
                .Include(x => x.DongVat)
                .Include(x => x.NguoiDung)
                .AsQueryable();

            // Người dùng chỉ xem đơn của mình
            if (vaiTro == "NguoiDung")
            {
                int maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung") ?? 0;

                ds = ds.Where(x => x.MaNguoiDung == maNguoiDung);
            }

            return View(ds.ToList());
        }

        public IActionResult ChiTiet(int id)
        {

            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            var don = _context.DonNhanNuois
                .Include(x => x.DongVat)
                .Include(x => x.NguoiDung)
                .FirstOrDefault(x => x.MaDon == id);

            if (don == null)
                return NotFound();

            return View(don);
        }

        public IActionResult Duyet(int id)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            var don = _context.DonNhanNuois
                .Include(x => x.DongVat)
                .FirstOrDefault(x => x.MaDon == id);

            // Chuyển kiểm tra null lên trên cùng để tránh lỗi NullReferenceException
            if (don == null)
                return NotFound();

            if (don.TrangThai != "Chờ duyệt")
            {
                TempData["Loi"] = "Đơn này đã được xử lý.";
                return RedirectToAction(nameof(Index));
            }

            // Cập nhật trạng thái đơn
            don.TrangThai = "Đã duyệt";

            // Cập nhật trạng thái động vật
            if (don.DongVat != null)
            {
                don.DongVat.TinhTrangSucKhoe = "Đã nhận nuôi";
            }

            _context.SaveChanges();

            //=====================
            // Tạo thông báo
            //=====================

            ThongBao tb = new ThongBao
            {
                MaNguoiDung = don.MaNguoiDung,
                NoiDung = "Đơn nhận nuôi của bạn đã được duyệt.",
                NgayGui = DateTime.Now,
                DaDoc = false
            };

            _context.ThongBaos.Add(tb);
            _context.SaveChanges();

            // Ghi nhật ký hoạt động duyệt đơn nhận nuôi
            NhatKyHelper.GhiLog(
                _context,
                HttpContext,
                "Duyệt đơn nhận nuôi");

            TempData["ThongBao"] = "Đã duyệt đơn nhận nuôi.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult TuChoi(int id)
        {

            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            var don = _context.DonNhanNuois
                .FirstOrDefault(x => x.MaDon == id);

            if (don == null)
                return NotFound();

            if (don.TrangThai != "Chờ duyệt")
            {
                TempData["Loi"] = "Đơn này đã được xử lý.";
                return RedirectToAction(nameof(Index));
            }

            don.TrangThai = "Từ chối";

            _context.SaveChanges();

            //=====================
            // Tạo thông báo
            //=====================

            ThongBao tb = new ThongBao
            {
                MaNguoiDung = don.MaNguoiDung,
                NoiDung = "Đơn nhận nuôi của bạn đã bị từ chối.",
                NgayGui = DateTime.Now,
                DaDoc = false
            };

            _context.ThongBaos.Add(tb);
            _context.SaveChanges();

            TempData["ThongBao"] = "Đã từ chối đơn nhận nuôi.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Sua(int id)
        {

            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            var don = _context.DonNhanNuois.FirstOrDefault(x => x.MaDon == id);

            if (don == null)
                return NotFound();

            ViewBag.NguoiDung = _context.NguoiDungs.ToList();

            return View(don);
        }

        [HttpPost]
        public IActionResult Sua(DonNhanNuoi model)
        {

            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.NguoiDung = _context.NguoiDungs.ToList();
                return View(model);
            }

            _context.DonNhanNuois.Update(model);
            _context.SaveChanges();

            TempData["ThongBao"] = "Cập nhật đơn nhận nuôi thành công.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Them(int maDongVat)
        {

            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "NguoiDung"))
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new DonNhanNuoi
            {
                MaDongVat = maDongVat,
                NgayDangKy = DateTime.Now,
                TrangThai = "Chờ duyệt"
            };

            int maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung") ?? 0;

            model.MaNguoiDung = maNguoiDung;

            return View(model);
        }

        [HttpPost]
        public IActionResult Them(DonNhanNuoi model)
        {

            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "NguoiDung"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.NguoiDung = _context.NguoiDungs.ToList();
                return View(model);
            }

            model.MaNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung") ?? 0;

            model.NgayDangKy = DateTime.Now;

            model.TrangThai = "Chờ duyệt";

            bool daDangKy = _context.DonNhanNuois.Any(x => x.MaDongVat == model.MaDongVat &&
                                                           x.MaNguoiDung == model.MaNguoiDung &&
                                                           x.TrangThai == "Chờ duyệt");

            if (daDangKy)
            {
                TempData["Loi"] = "Bạn đã gửi đơn nhận nuôi.";
                return RedirectToAction(nameof(Index));
            }

            _context.DonNhanNuois.Add(model);

            _context.SaveChanges();

            TempData["ThongBao"] = "Đăng ký nhận nuôi thành công.";

            ModelState.Remove(nameof(DonNhanNuoi.MaNguoiDung));
            ModelState.Remove(nameof(DonNhanNuoi.NgayDangKy));
            ModelState.Remove(nameof(DonNhanNuoi.TrangThai));

            return RedirectToAction(nameof(Index));
        }

        // Trong NhanNuoiController.cs
        public IActionResult TheoDoiDon()
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            int maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung") ?? 0;

            // Bổ sung .Include(x => x.DongVat) để lấy được tên thú cưng hiển thị ra View
            var dsDon = _context.DonNhanNuois
                .Include(x => x.DongVat)
                .Where(x => x.MaNguoiDung == maNguoiDung)
                .OrderByDescending(x => x.NgayDangKy)
                .ToList();

            return View(dsDon);
        }
        
        [HttpPost]
public IActionResult DuyetHangLoat(List<int> ids)
{
    if (ids == null || !ids.Any())
        return Json(new { success = false, message = "Chưa chọn đơn nào." });

    try
    {
        // GIẢI PHÁP: Lấy danh sách Chờ duyệt lên RAM trước để tránh lỗi dịch câu lệnh SQL (WITH)
        var tatCaChoDuyet = _context.DonNhanNuois
                                    .Where(x => x.TrangThai == "Chờ duyệt")
                                    .ToList(); 

        // Lọc trên RAM
        var dsDonCanDuyet = tatCaChoDuyet.Where(x => ids.Contains(x.MaDon)).ToList();

        if (!dsDonCanDuyet.Any())
            return Json(new { success = false, message = "Không tìm thấy đơn hợp lệ để duyệt." });

        foreach (var don in dsDonCanDuyet)
        {
            don.TrangThai = "Đã duyệt";
            
            // Nếu logic duyệt đơn lẻ của bạn có cập nhật thêm gì (ví dụ: đổi trạng thái Động vật) thì chép vào đây
        }

        _context.SaveChanges();
        
        return Json(new { success = true });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
    }
}
    }
}