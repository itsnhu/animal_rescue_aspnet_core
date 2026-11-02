using HeThongCuuHo.Helpers;
using HeThongCuuHo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HeThongCuuHo.Controllers
{
    public class DongVatController : Controller
    {
        private readonly HethongcuuhoDbContext _context;
        public DongVatController(HethongcuuhoDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            var ds = _context.DongVats
                .Include(x => x.BaoCao)
                .Include(x => x.HoSoYTes)
                .OrderByDescending(x => x.NgayTiepNhan)
                .ToList();

            return View(ds);
        }

        public IActionResult ChiTiet(int id)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            var dongVat = _context.DongVats
                .Include(x => x.BaoCao)
                .Include(x => x.HoSoYTes)
                .FirstOrDefault(x => x.MaDongVat == id);

            if (dongVat == null)
                return NotFound();

            return View(dongVat);
        }

        [HttpGet]
        public IActionResult Sua(int id)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
                return RedirectToAction("Index", "Login");

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin", "Tình nguyện viên", "TinhNguyenVien"))
            {
                return RedirectToAction("Index", "Home");
            }
            var dongVat = _context.DongVats.Find(id);
            if (dongVat == null)
                return NotFound();

            ViewBag.DanhSachTram = new SelectList(_context.TramCuuHos, "MaTram", "TenTram", dongVat.MaTram);

            return View(dongVat);
        }

        [HttpPost]
        public IActionResult Sua(DongVat model, IFormFile? hinhAnhFile)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
                return RedirectToAction("Index", "Login");

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin", "Tình nguyện viên", "TinhNguyenVien"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
                return View(model);

            var dongVat = _context.DongVats.Find(model.MaDongVat);
            if (dongVat == null)
            {
                return NotFound();
            }

            try
            {
                dongVat.Ten = model.Ten;
                dongVat.Loai = model.Loai;
                dongVat.Giong = model.Giong;
                dongVat.GioiTinh = model.GioiTinh;
                dongVat.Tuoi = model.Tuoi;
                dongVat.TinhTrangSucKhoe = model.TinhTrangSucKhoe;

                if (model.MaTram > 0)
                {
                    dongVat.MaTram = model.MaTram;
                }

                // 📸 XỬ LÝ UPLOAD ẢNH MỚI (NẾU CÓ)
                if (hinhAnhFile != null && hinhAnhFile.Length > 0)
                {
                    // 1. Xóa ảnh cũ trên server nếu tồn tại
                    if (!string.IsNullOrEmpty(dongVat.HinhAnh))
                    {
                        var anhCuPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", dongVat.HinhAnh.TrimStart('/'));
                        if (System.IO.File.Exists(anhCuPath))
                        {
                            System.IO.File.Delete(anhCuPath);
                        }
                    }

                    // 2. Lưu ảnh mới vào wwwroot/images/dongvat
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(hinhAnhFile.FileName);
                    string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/dongvat");

                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    string filePath = Path.Combine(uploadDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        hinhAnhFile.CopyTo(stream);
                    }

                    // 3. Cập nhật đường dẫn vào CSDL
                    dongVat.HinhAnh = "/images/dongvat/" + fileName;
                }

                _context.SaveChanges();
                TempData["ThongBao"] = "Cập nhật thành công.";
            }
            catch (Exception)
            {
                TempData["Loi"] = "Có lỗi xảy ra khi cập nhật cơ sở dữ liệu.";
                return View(model);
            }

            return RedirectToAction("Index");
        }

        public IActionResult ChuyenDieuTri(int id)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
                return RedirectToAction("Index", "Login");

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin") && !AuthorizationHelper.KiemTraVaiTro(HttpContext, "Tình nguyện viên"))
                return RedirectToAction("Index", "Home");

            var dongVat = _context.DongVats.Find(id);
            if (dongVat == null)
            {
                TempData["Loi"] = "Không tìm thấy động vật.";
                return RedirectToAction("Index");
            }

            return RedirectToAction("Them", "DieuTri", new { maDongVat = id });
        }

        [HttpPost]
        public IActionResult Xoa(int id)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
                return RedirectToAction("Index", "Login");

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin"))
                return RedirectToAction("Index", "Home");

            var dongVat = _context.DongVats.Find(id);
            if (dongVat == null)
                return NotFound();

            bool daDieuTri = _context.HoSoYTes.Any(x => x.MaDongVat == id);
            if (daDieuTri)
            {
                TempData["Loi"] = "Động vật đã có hồ sơ điều trị, không thể xóa.";
                return RedirectToAction("Index");
            }

            try
            {
                // Xóa file ảnh trên ổ đĩa khi xóa bản ghi động vật
                if (!string.IsNullOrEmpty(dongVat.HinhAnh))
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", dongVat.HinhAnh.TrimStart('/'));
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                _context.DongVats.Remove(dongVat);
                _context.SaveChanges();
                TempData["ThongBao"] = "Đã xóa thành công.";
            }
            catch (Exception)
            {
                TempData["Loi"] = "Không thể xóa do vướng ràng buộc dữ liệu.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ThemMoi()
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
                return RedirectToAction("Index", "Login");

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin", "Tình nguyện viên", "TinhNguyenVien"))
                return RedirectToAction("Index", "Home");

            ViewBag.DanhSachTram = new SelectList(_context.TramCuuHos, "MaTram", "TenTram");

            return View("Them");
        }

        [HttpPost]
        public IActionResult ThemMoi(DongVat model, IFormFile? hinhAnhFile)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
                return RedirectToAction("Index", "Login");

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin", "Tình nguyện viên", "TinhNguyenVien"))
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
                return View("Them", model);

            try
            {
                // 📸 XỬ LÝ UPLOAD ẢNH THÊM MỚI
                if (hinhAnhFile != null && hinhAnhFile.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(hinhAnhFile.FileName);
                    string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/dongvat");

                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    string filePath = Path.Combine(uploadDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        hinhAnhFile.CopyTo(stream);
                    }

                    model.HinhAnh = "/images/dongvat/" + fileName;
                }

                model.NgayTiepNhan = DateTime.Now;

                // GÁN MÃ TRẠM MẶC ĐỊNH LÀ 1 (khớp với MaTram = 1 trong SQL)
                if (model.MaTram <= 0)
                {
                    model.MaTram = 1;
                }

                _context.DongVats.Add(model);
                _context.SaveChanges();

                TempData["ThongBao"] = "Thêm mới động vật thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                string chiTietLoi = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                TempData["Loi"] = "Lỗi lưu dữ liệu: " + chiTietLoi;
                return View("Them", model);
            }
        }

        // Trang danh sách thú cưng hiển thị cho người dùng chọn nhận nuôi
        [HttpGet]
        public IActionResult DanhSach()
        {
            var danhSach = _context.DongVats
                .Where(x => x.TinhTrangSucKhoe != "Đã nhận nuôi")
                .OrderByDescending(x => x.NgayTiepNhan)
                .ToList();

            return View(danhSach);
        }
    }
}