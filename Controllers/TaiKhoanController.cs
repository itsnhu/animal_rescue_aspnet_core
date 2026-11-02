using HeThongCuuHo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace HeThongCuuHo.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly HethongcuuhoDbContext _context;

        public TaiKhoanController(HethongcuuhoDbContext context)
        {
            _context = context;
        }

        // Hiển thị trang đăng nhập
        [HttpGet]
        public IActionResult DangNhap()
        {
            // Đã bỏ truyền ViewModel, trả về View rỗng
            return View();
        }

        // Xử lý đăng nhập (Cách 1: Hứng trực tiếp tham số)
        [HttpPost]
        public IActionResult DangNhap(string tenDangNhap, string matKhau)
        {
            // 1. Kiểm tra dữ liệu rỗng
            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(matKhau))
            {
                ViewBag.Loi = "Tên đăng nhập hoặc mật khẩu không được để trống!";
                return View();
            }

            // 2. Kiểm tra Database
            var taiKhoan = _context.TaiKhoans.FirstOrDefault(x =>
                x.TenDangNhap == tenDangNhap &&
                x.MatKhau == matKhau &&
                x.TrangThai == true);

            if (taiKhoan == null)
            {
                ViewBag.Loi = "Tên đăng nhập hoặc mật khẩu không đúng!";
                return View();
            }

            // 3. Lưu Session
            HttpContext.Session.SetInt32("MaTaiKhoan", taiKhoan.MaTaiKhoan);
            HttpContext.Session.SetString("TenDangNhap", taiKhoan.TenDangNhap ?? "User");
            HttpContext.Session.SetString("VaiTro", taiKhoan.VaiTro ?? "NguoiDung");

            // Chuyển đến Dashboard
            return RedirectToAction("Index", "Home");
        }

        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(DangNhap));
        }

        // ================= ĐỔI MẬT KHẨU =================

        [HttpGet]
        public IActionResult DoiMatKhau()
        {
            if (HttpContext.Session.GetInt32("MaTaiKhoan") == null)
            {
                return RedirectToAction("DangNhap");
            }

            return View();
        }

        [HttpPost]
        public IActionResult DoiMatKhau(
            string matKhauCu,
            string matKhauMoi,
            string xacNhanMatKhau)
        {
            // 1. Kiểm tra dữ liệu rỗng
            if (string.IsNullOrEmpty(matKhauCu) || string.IsNullOrEmpty(matKhauMoi) || string.IsNullOrEmpty(xacNhanMatKhau))
            {
                ViewBag.Loi = "Vui lòng nhập đầy đủ thông tin mật khẩu.";
                return View();
            }

            int? maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null)
            {
                return RedirectToAction("DangNhap");
            }

            var taiKhoan = _context.TaiKhoans.Find(maTaiKhoan);
            if (taiKhoan == null)
            {
                return RedirectToAction("DangNhap");
            }

            // 2. Kiểm tra mật khẩu cũ
            if (taiKhoan.MatKhau != matKhauCu)
            {
                ViewBag.Loi = "Mật khẩu hiện tại không đúng.";
                return View();
            }

            // 3. Kiểm tra xác nhận mật khẩu
            if (matKhauMoi != xacNhanMatKhau)
            {
                ViewBag.Loi = "Xác nhận mật khẩu không khớp.";
                return View();
            }

            // 4. Cập nhật mật khẩu mới
            taiKhoan.MatKhau = matKhauMoi;
            _context.SaveChanges();

            TempData["ThongBao"] = "Đổi mật khẩu thành công.";
            return RedirectToAction("Index", "Home");
        }
    }
}