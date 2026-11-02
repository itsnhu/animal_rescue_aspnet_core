using HeThongCuuHo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HeThongCuuHo.Controllers
{
    public class ProfileController : Controller
    {
        private readonly HethongcuuhoDbContext _context;

        public ProfileController(HethongcuuhoDbContext context)
        {
            _context = context;
        }

        // 1. Hiển thị trang hồ sơ cá nhân
        public IActionResult Index()
        {
            int? maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung");

            if (maNguoiDung == null)
                return RedirectToAction("Index", "Login");

            var nd = _context.NguoiDungs
                             .FirstOrDefault(x => x.MaNguoiDung == maNguoiDung);

            if (nd == null)
                return NotFound();

            return View(nd);
        }

        // 2. Form chỉnh sửa thông tin cá nhân
        public IActionResult Sua()
        {
            int? maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung");

            if (maNguoiDung == null)
                return RedirectToAction("Index", "Login");

            var model = _context.NguoiDungs.Find(maNguoiDung);

            if (model == null)
                return NotFound();

            return View(model);
        }

        // 3. Lưu thông tin chỉnh sửa
        [HttpPost]
        public IActionResult Sua(NguoiDung model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var nd = _context.NguoiDungs.Find(model.MaNguoiDung);

            if (nd == null)
                return NotFound();

            nd.HoTen = model.HoTen;
            nd.Email = model.Email;
            nd.SoDienThoai = model.SoDienThoai;
            nd.DiaChi = model.DiaChi;

            _context.SaveChanges();

            // ĐÃ SỬA LỖI TẠI ĐÂY: Bỏ đoạn check MaTaiKhoan (vì nó là int)
            HttpContext.Session.SetString("TenDangNhap", nd.HoTen ?? "User");

            TempData["ThongBao"] = "Cập nhật hồ sơ thành công.";

            return RedirectToAction(nameof(Index));
        }

        // 4. Xử lý đổi ảnh đại diện (AJAX)
        [HttpPost]
        public async Task<IActionResult> DoiAvatar(IFormFile file)
        {
            int? maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung");
            if (maNguoiDung == null)
                return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });

            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn file ảnh." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return Json(new { success = false, message = "Chỉ hỗ trợ file ảnh (JPG, PNG, GIF)." });

            if (file.Length > 2 * 1024 * 1024) // 2MB
                return Json(new { success = false, message = "Kích thước ảnh không được vượt quá 2MB." });

            var nd = _context.NguoiDungs.Find(maNguoiDung);
            if (nd == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng." });

            try
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Tạo tên file mới ngẫu nhiên để tránh trùng lặp
                var newFileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadFolder, newFileName);

                // Lưu file vào thư mục
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(nd.AnhDaiDien))
                {
                    var oldFilePath = Path.Combine(uploadFolder, nd.AnhDaiDien);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Cập nhật DB
                nd.AnhDaiDien = newFileName;
                _context.SaveChanges();

                // CẬP NHẬT SESSION ĐỂ HEADER LẤY ẢNH MỚI
                HttpContext.Session.SetString("AnhDaiDien", newFileName);

                var imageUrl = Url.Content("~/uploads/avatars/" + newFileName);
                return Json(new { success = true, url = imageUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi Server: " + ex.Message });
            }
        }
    }
}