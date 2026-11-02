using HeThongCuuHo.Helpers;
using HeThongCuuHo.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongCuuHo.Controllers
{
    public class BaoCaoController : Controller
    {
        private readonly HethongcuuhoDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BaoCaoController(HethongcuuhoDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Hiển thị danh sách báo cáo
        public IActionResult Index(string timKiem, string trangThai)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            var dsBaoCao = _context.BaoCaos.AsQueryable();

            // Nếu là Người dùng thì chỉ xem báo cáo của chính mình
            string? vaiTro = HttpContext.Session.GetString("VaiTro");

            if (vaiTro == "NguoiDung")
            {
                int maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung") ?? 0;
                dsBaoCao = dsBaoCao.Where(x => x.MaNguoiDung == maNguoiDung);
            }

            // Tìm kiếm theo Loại động vật, Địa chỉ, Trạng thái hoặc Mức độ khẩn cấp
            if (!string.IsNullOrEmpty(timKiem))
            {
                dsBaoCao = dsBaoCao.Where(x =>
                    x.LoaiDongVat.Contains(timKiem) ||
                    x.DiaChi.Contains(timKiem) ||
                    x.TrangThai.Contains(timKiem) ||
                    x.MucDoKhanCap.Contains(timKiem));
            }

            if (!string.IsNullOrEmpty(trangThai))
            {
                dsBaoCao = dsBaoCao.Where(x => x.TrangThai == trangThai);
            }

            // ƯU TIÊN: Đưa các ca "Rất khẩn cấp" lên đầu, sau đó sắp xếp theo ngày mới nhất
            var kếtQuả = dsBaoCao
                .OrderByDescending(x => x.MucDoKhanCap == "Rất khẩn cấp")
                .ThenByDescending(x => x.NgayBaoCao)
                .ToList();

            return View(kếtQuả);
        }

        // GET: BaoCao/Them
        [HttpGet]
        public IActionResult Them()
        {
            // Cho phép khách vãng lai và người dùng đã đăng nhập truy cập form gửi báo cáo
            return View();
        }

        // Lưu dữ liệu báo cáo mới (Tương thích với Model hiện tại không sửa đổi)
        /// Lưu dữ liệu báo cáo mới (Tương thích với Model hiện tại không sửa đổi)
        [HttpPost]
        public IActionResult Them(BaoCao model, IFormFile? file, string? HoTenNguoiGui, string? SoDienThoai)
        {
            // Kiểm tra thông tin đăng nhập từ Session
            int? maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            int finalMaNguoiDung = 0;

            // Luôn truy vấn lấy chính xác MaNguoiDung từ MaTaiKhoan để tránh bị lệch ID
            if (maTaiKhoan.HasValue && maTaiKhoan.Value > 0)
            {
                var nguoiDung = _context.NguoiDungs.FirstOrDefault(x => x.MaTaiKhoan == maTaiKhoan.Value);
                if (nguoiDung != null)
                {
                    finalMaNguoiDung = nguoiDung.MaNguoiDung; // Lấy đúng MaNguoiDung thực tế trong bảng
                    HttpContext.Session.SetInt32("MaNguoiDung", finalMaNguoiDung);
                }
                else
                {
                    // Nếu tài khoản đăng nhập nhưng chưa có hồ sơ trong bảng NguoiDung, tự động tạo mới
                    var taiKhoan = _context.TaiKhoans.Find(maTaiKhoan.Value);
                    var moiNguoiDung = new NguoiDung
                    {
                        MaTaiKhoan = maTaiKhoan.Value,
                        HoTen = taiKhoan != null ? taiKhoan.TenDangNhap : "Người dùng",
                        SoDienThoai = SoDienThoai ?? ""
                    };
                    _context.NguoiDungs.Add(moiNguoiDung);
                    _context.SaveChanges();

                    finalMaNguoiDung = moiNguoiDung.MaNguoiDung;
                    HttpContext.Session.SetInt32("MaNguoiDung", finalMaNguoiDung);
                }
            }

            // Gán vào model
            if (finalMaNguoiDung > 0)
            {
                model.MaNguoiDung = finalMaNguoiDung;
            }
            else
            {
                model.MaNguoiDung = 0; // Khách vãng lai dùng MaNguoiDung = 0 có sẵn

                string thongTinKhach = $"[Khách vãng lai] Họ tên: {HoTenNguoiGui ?? "Không rõ"} - SĐT: {SoDienThoai ?? "Không có"}\n";
                model.MoTa = thongTinKhach + (model.MoTa ?? "");
            }

            model.NgayBaoCao = DateTime.Now;
            model.TrangThai = "Chờ xử lý";

            ModelState.Remove(nameof(BaoCao.MaNguoiDung));
            ModelState.Remove(nameof(BaoCao.NgayBaoCao));
            ModelState.Remove(nameof(BaoCao.TrangThai));
            ModelState.Remove(nameof(BaoCao.NguoiDung));

            if (string.IsNullOrEmpty(SoDienThoai) && finalMaNguoiDung <= 0)
            {
                ModelState.AddModelError("", "Vui lòng nhập số điện thoại liên hệ đối với khách vãng lai.");

                // ➔ Chỉ định rõ đường dẫn View để ép hệ thống giữ nguyên giao diện không có Layout Admin
                return View("~/Views/BaoCao/Them.cshtml", model);
            }

            if (!ModelState.IsValid)
            {
                // ➔ Chỉ định rõ đường dẫn View
                return View("~/Views/BaoCao/Them.cshtml", model);
            }

            if (file != null && file.Length > 0)
            {
                string tenFile = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string thuMuc = Path.Combine(_env.WebRootPath, "uploads");

                if (!Directory.Exists(thuMuc))
                {
                    Directory.CreateDirectory(thuMuc);
                }

                string duongDan = Path.Combine(thuMuc, tenFile);

                using (var stream = new FileStream(duongDan, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                model.HinhAnh = tenFile;
            }

            _context.BaoCaos.Add(model);

            // 1. Gửi thông báo cho Admin
            ThongBao tb = new ThongBao
            {
                MaNguoiDung = 1,
                NoiDung = "Có báo cáo cứu hộ mới cần xử lý.",
                NgayGui = DateTime.Now,
                DaDoc = false
            };
            _context.ThongBaos.Add(tb);

            // 2. GỬI THÔNG BÁO CHO CHÍNH NGƯỜI DÙNG VỪA BÁO CÁO
            if (model.MaNguoiDung > 0)
            {
                ThongBao tbNguoiGui = new ThongBao
                {
                    MaNguoiDung = model.MaNguoiDung,
                    NoiDung = "Báo cáo cứu hộ của bạn đã được tiếp nhận.",
                    NgayGui = DateTime.Now,
                    DaDoc = false
                };
                _context.ThongBaos.Add(tbNguoiGui);
            }

            _context.SaveChanges();

            // Ghi nhật ký hoạt động tạo báo cáo cứu hộ
            NhatKyHelper.GhiLog(
                _context,
                HttpContext,
                "Tạo báo cáo cứu hộ");

            TempData["ThongBao"] = "Gửi báo cáo cứu hộ thành công!";

            // ➔ Hướng đi sau khi thành công
            return RedirectToAction("Them", "BaoCao");
        }

        // GET: BaoCao/Sua/5
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

            var baoCao = _context.BaoCaos.Find(id);

            if (baoCao == null)
                return NotFound();

            string? vaiTro = HttpContext.Session.GetString("VaiTro");

            if (vaiTro == "NguoiDung")
            {
                int maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung") ?? 0;

                if (baoCao.MaNguoiDung != maNguoiDung)
                {
                    return RedirectToAction("Index");
                }
            }

            return View(baoCao);
        }

        // POST: BaoCao/Sua/5
        [HttpPost]
        public IActionResult Sua(BaoCao model, IFormFile? file)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.Remove(nameof(BaoCao.MaNguoiDung));
            ModelState.Remove(nameof(BaoCao.NgayBaoCao));
            ModelState.Remove(nameof(BaoCao.TrangThai));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var baoCao = _context.BaoCaos.Find(model.MaBaoCao);

            if (baoCao == null)
            {
                return NotFound();
            }

            // Cập nhật các thông tin
            baoCao.LoaiDongVat = model.LoaiDongVat;
            baoCao.MucDoKhanCap = model.MucDoKhanCap; // Bổ sung cập nhật Mức độ khẩn cấp
            baoCao.MoTa = model.MoTa;
            baoCao.DiaChi = model.DiaChi;
            baoCao.ViDo = model.ViDo;
            baoCao.KinhDo = model.KinhDo;

            if (file != null && file.Length > 0)
            {
                string tenFile = Guid.NewGuid().ToString()
                             + Path.GetExtension(file.FileName);

                string thuMuc = Path.Combine(_env.WebRootPath, "uploads");

                if (!Directory.Exists(thuMuc))
                {
                    Directory.CreateDirectory(thuMuc);
                }

                // Xóa ảnh cũ
                if (!string.IsNullOrEmpty(baoCao.HinhAnh))
                {
                    string anhCu = Path.Combine(thuMuc, baoCao.HinhAnh);

                    if (System.IO.File.Exists(anhCu))
                    {
                        System.IO.File.Delete(anhCu);
                    }
                }

                string duongDan = Path.Combine(thuMuc, tenFile);

                using (var stream = new FileStream(duongDan, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                baoCao.HinhAnh = tenFile;
            }

            _context.SaveChanges();

            TempData["ThongBao"] = "Cập nhật thành công!";

            return RedirectToAction("Index");
        }

        // GET: BaoCao/Xoa/5
        [HttpGet]
        public IActionResult Xoa(int id)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            var baoCao = _context.BaoCaos.Find(id);

            if (baoCao == null)
            {
                return NotFound();
            }

            return View(baoCao);
        }

        // POST: BaoCao/Xoa/5
        [HttpPost]
        [ActionName("Xoa")]
        public IActionResult XoaXacNhan(int id)
        {
            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            var baoCao = _context.BaoCaos.Find(id);

            if (baoCao == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(baoCao.HinhAnh))
            {
                string duongDan = Path.Combine(
                    _env.WebRootPath,
                    "uploads",
                    baoCao.HinhAnh);

                if (System.IO.File.Exists(duongDan))
                {
                    System.IO.File.Delete(duongDan);
                }
            }

            bool daXuLy = _context.CuuHos.Any(x => x.MaBaoCao == id);

            if (daXuLy)
            {
                TempData["ThongBao"] = "Không thể xóa báo cáo đã được xử lý.";
                return RedirectToAction("Index");
            }

            _context.BaoCaos.Remove(baoCao);
            _context.SaveChanges();

            TempData["ThongBao"] = "Đã xóa báo cáo thành công!";

            return RedirectToAction("Index");
        }

        // Xem chi tiết báo cáo
        public IActionResult ChiTiet(int id)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            var baoCao = _context.BaoCaos
                .FirstOrDefault(x => x.MaBaoCao == id);

            if (baoCao == null)
                return NotFound();

            string? vaiTro = HttpContext.Session.GetString("VaiTro");

            if (vaiTro == "NguoiDung")
            {
                int maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung") ?? 0;

                if (baoCao.MaNguoiDung != maNguoiDung)
                {
                    return RedirectToAction("Index");
                }
            }

            var cuuHo = _context.CuuHos
                .Include(x => x.TinhNguyenVien)
                .FirstOrDefault(x => x.MaBaoCao == id);

            ViewBag.CuuHo = cuuHo;

            return View(baoCao);
        }
    }
}