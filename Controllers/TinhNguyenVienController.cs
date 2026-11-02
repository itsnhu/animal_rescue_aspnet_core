using HeThongCuuHo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongCuuHo.Controllers
{
    public class TinhNguyenVienController : Controller
    {
        private readonly HethongcuuhoDbContext _context;

        public TinhNguyenVienController(HethongcuuhoDbContext context)
        {
            _context = context;
        }

        // Danh sách
        // Danh sách
        public IActionResult Index(string tuKhoa, string trangThai)
        {
            if (HttpContext.Session.GetInt32("MaTaiKhoan") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var vaiTro = HttpContext.Session.GetString("VaiTro");
            int? maND = HttpContext.Session.GetInt32("MaNguoiDung");

            // 1. Dành cho Admin hoặc Tình nguyện viên (giữ nguyên logic cũ của bạn)
            if (vaiTro == "Admin" || vaiTro == "TinhNguyenVien")
            {
                var dsTNV = _context.TinhNguyenViens.AsQueryable();

                if (vaiTro == "TinhNguyenVien")
                {
                    dsTNV = dsTNV.Where(x => x.MaNguoiDung == maND);
                }

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    dsTNV = dsTNV.Where(x =>
                        x.HoTen.Contains(tuKhoa) ||
                        x.SoDienThoai.Contains(tuKhoa));
                }

                if (!string.IsNullOrWhiteSpace(trangThai))
                {
                    dsTNV = dsTNV.Where(x => x.TrangThai == trangThai);
                }

                ViewBag.TuKhoa = tuKhoa;
                ViewBag.TrangThai = trangThai;

                // Trả về view quản lý tình nguyện viên dành cho Admin/TNV
                return View("Index", dsTNV.ToList());
            }

            // 2. Dành cho Người dùng thông thường: Xem tình nguyện viên nhận ca cứu hộ của chính họ
            var danhSachCuuHoCuaToi = _context.CuuHos
                .Include(x => x.BaoCao)
                .Include(x => x.TinhNguyenVien)
                .ThenInclude(t => t.NguoiDung)
                .Where(x => x.BaoCao != null && x.BaoCao.MaNguoiDung == maND)
                .OrderByDescending(x => x.ThoiGianNhan)
                .ToList();

            // Bạn có thể tạo một View riêng cho người dùng, ví dụ: "NguoiDungIndex" hoặc truyền Model là danh sách CuuHo
            return View("NguoiDungIndex", danhSachCuuHoCuaToi);
        }

        [HttpGet]
        public IActionResult Them()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Them(TinhNguyenVien model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.TinhNguyenViens.Add(model);

            _context.SaveChanges();

            TempData["ThongBao"] = "Đã thêm tình nguyện viên.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Sua(int id)
        {
            var tnv = _context.TinhNguyenViens.Find(id);

            if (tnv == null)
                return NotFound();

            return View(tnv);
        }

        [HttpPost]
        public IActionResult Sua(TinhNguyenVien model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.TinhNguyenViens.Update(model);

            _context.SaveChanges();

            TempData["ThongBao"] = "Cập nhật thành công.";

            return RedirectToAction(nameof(Index));
        }

        // Xem vị trí trên bản đồ
        public IActionResult ViTri(int id)
        {
            var tnv = _context.TinhNguyenViens
                .FirstOrDefault(x => x.MaTinhNguyenVien == id);

            if (tnv == null)
                return NotFound();

            return View(tnv);
        }

        // Thống kê số lần cứu hộ
        public IActionResult ThongKe(int id)
        {
            var tnv = _context.TinhNguyenViens
                .FirstOrDefault(x => x.MaTinhNguyenVien == id);

            if (tnv == null)
                return NotFound();

            ViewBag.SoLanCuuHo = _context.CuuHos
                .Count(x => x.MaTinhNguyenVien == id);

            return View(tnv);
        }

        // Theo dõi đơn tham gia của tài khoản đang đăng nhập
        [HttpGet]
        public IActionResult TheoDoiThamGia()
        {
            // Lấy MaNguoiDung từ Session (giả sử lúc đăng nhập bạn lưu Session với key là "MaNguoiDung")
            int? maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung");

            // Nếu chưa đăng nhập, chuyển hướng về trang đăng nhập
            if (maNguoiDung == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Lấy danh sách tình nguyện viên/đơn đăng ký tương ứng với MaNguoiDung hiện tại
            var danhSachDon = _context.TinhNguyenViens
                .Where(x => x.MaNguoiDung == maNguoiDung.Value)
                .ToList();

            return View(danhSachDon);
        }
    }
}