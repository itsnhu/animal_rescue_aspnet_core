using HeThongCuuHo.Helpers;
using HeThongCuuHo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace HeThongCuuHo.Controllers
{
    public class LoginController : Controller
    {
        private readonly HethongcuuhoDbContext _context;
        private readonly IConfiguration _configuration;

        public LoginController(HethongcuuhoDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Nếu đã đăng nhập thì chuyển thẳng vào trang chủ
            if (HttpContext.Session.GetInt32("MaTaiKhoan") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // ================= XỬ LÝ ĐĂNG NHẬP =================
        [HttpPost]
        public IActionResult Index(string tenDangNhap, string matKhau, bool remember)
        {
            // 1. Kiểm tra dữ liệu đầu vào (Manual Validation)
            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(matKhau))
            {
                ViewBag.Loi = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                return View();
            }

            // 2. Kiểm tra tài khoản trong Database
            var taiKhoan = _context.TaiKhoans.FirstOrDefault(x =>
                x.TenDangNhap == tenDangNhap &&
                x.MatKhau == matKhau &&
                x.TrangThai == true);

            if (taiKhoan == null)
            {
                ViewBag.Loi = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View();
            }

            // 3. Lấy đúng MaNguoiDung từ bảng NguoiDung
            int maNguoiDungThucTe;
            var nguoiDung = _context.NguoiDungs.FirstOrDefault(x => x.MaTaiKhoan == taiKhoan.MaTaiKhoan);
            
            if (nguoiDung != null)
            {
                maNguoiDungThucTe = nguoiDung.MaNguoiDung; 
            }
            else
            {
                // Nếu tài khoản chưa có thông tin, tự động tạo mới
                var moiNguoiDung = new NguoiDung
                {
                    MaTaiKhoan = taiKhoan.MaTaiKhoan,
                    HoTen = taiKhoan.TenDangNhap,
                    SoDienThoai = ""
                };
                _context.NguoiDungs.Add(moiNguoiDung);
                _context.SaveChanges();
                
                maNguoiDungThucTe = moiNguoiDung.MaNguoiDung;
                nguoiDung = moiNguoiDung; 
            }

            // 4. Lưu Session
            HttpContext.Session.SetInt32("MaTaiKhoan", taiKhoan.MaTaiKhoan);
            HttpContext.Session.SetInt32("MaNguoiDung", maNguoiDungThucTe);
            HttpContext.Session.SetString("TenDangNhap", taiKhoan.TenDangNhap ?? "User");
            HttpContext.Session.SetString("VaiTro", taiKhoan.VaiTro ?? "NguoiDung");

            // Lưu ảnh đại diện vào Session
            HttpContext.Session.SetString("AnhDaiDien", nguoiDung?.AnhDaiDien ?? "");

            if (remember)
            {
                HttpContext.Session.SetString("IsRemembered", "True");
            }

            // 5. Ghi nhật ký và chuyển hướng
            NhatKyHelper.GhiLog(_context, HttpContext, "Đăng nhập hệ thống");

            return RedirectToAction("Index", "Home");
        }

        // ================= ĐĂNG KÝ =================
        [HttpGet]
        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DangKy(
            string hoTen,
            string soDienThoai,
            string email,
            string diaChi,
            string tenDangNhap,
            string matKhau,
            string xacNhanMatKhau) // <-- BỔ SUNG THAM SỐ NÀY ĐỂ KHỚP VỚI FORM
        {
            // 1. Kiểm tra xác nhận mật khẩu (Bảo mật Server-side)
            if (matKhau != xacNhanMatKhau)
            {
                ViewBag.Loi = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            // 2. Kiểm tra tên đăng nhập đã tồn tại
            bool tonTai = _context.TaiKhoans.Any(x => x.TenDangNhap == tenDangNhap);

            if (tonTai)
            {
                ViewBag.Loi = "Tên đăng nhập đã tồn tại.";
                return View();
            }

            // 3. Tạo tài khoản
            TaiKhoan tk = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhau = matKhau,
                VaiTro = "NguoiDung",
                TrangThai = true
            };

            _context.TaiKhoans.Add(tk);
            _context.SaveChanges();

            // 4. Tạo người dùng
            NguoiDung nd = new NguoiDung
            {
                HoTen = hoTen,
                SoDienThoai = soDienThoai,
                Email = email,
                DiaChi = diaChi,
                MaTaiKhoan = tk.MaTaiKhoan
            };

            _context.NguoiDungs.Add(nd);
            _context.SaveChanges();

            TempData["ThongBao"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Index");
        }

        // ================= QUÊN MẬT KHẨU =================
        [HttpGet]
        public IActionResult QuenMatKhau()
        {
            return View();
        }

        // 1. Xử lý AJAX gửi mã OTP
        [HttpPost]
        public IActionResult GuiMaOTP([FromBody] OtpRequestModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.TenDangNhap) || string.IsNullOrEmpty(model.Email))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ tên đăng nhập và email!" });
            }

            var taiKhoan = _context.TaiKhoans.FirstOrDefault(x => x.TenDangNhap == model.TenDangNhap);
            if (taiKhoan == null)
            {
                return Json(new { success = false, message = "Tên đăng nhập không tồn tại trong hệ thống!" });
            }

            var nguoiDung = _context.NguoiDungs.FirstOrDefault(x => x.MaTaiKhoan == taiKhoan.MaTaiKhoan && x.Email == model.Email);
            if (nguoiDung == null)
            {
                return Json(new { success = false, message = "Email không khớp với tài khoản này!" });
            }

            // Tạo mã OTP ngẫu nhiên 6 chữ số
            string otp = new Random().Next(100000, 999999).ToString();

            // Lưu vào Session
            HttpContext.Session.SetString("OTP_Code", otp);
            HttpContext.Session.SetString("OTP_User", model.TenDangNhap);

            try
            {
                // Gửi email thực tế
                GuiEmailThucTe(model.Email, "Mã xác thực khôi phục mật khẩu",
                    $"Xin chào {model.TenDangNhap},\n\nMã OTP khôi phục mật khẩu của bạn là: {otp}\n\nVui lòng không chia sẻ mã này cho người khác.\n\nTrân trọng,\nHệ thống Cứu hộ");

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Không thể gửi email. Lỗi: " + ex.Message });
            }
        }

        // 2. Xử lý kiểm tra OTP và đổi mật khẩu
        [HttpPost]
        public IActionResult QuenMatKhau(string tenDangNhap, string email, string maOTP, string matKhauMoi, string xacNhanMatKhau)
        {
            // Kiểm tra mã OTP trong Session
            string? sessionOtp = HttpContext.Session.GetString("OTP_Code");
            string? sessionUser = HttpContext.Session.GetString("OTP_User");

            if (string.IsNullOrEmpty(sessionOtp) || sessionOtp != maOTP || sessionUser != tenDangNhap)
            {
                ViewBag.Loi = "Mã OTP không chính xác hoặc đã hết hạn.";
                return View();
            }

            if (matKhauMoi != xacNhanMatKhau)
            {
                ViewBag.Loi = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            var taiKhoan = _context.TaiKhoans.FirstOrDefault(x => x.TenDangNhap == tenDangNhap);
            if (taiKhoan == null)
            {
                ViewBag.Loi = "Không tìm thấy tài khoản.";
                return View();
            }

            var nguoiDung = _context.NguoiDungs.FirstOrDefault(x => x.MaTaiKhoan == taiKhoan.MaTaiKhoan && x.Email == email);
            if (nguoiDung == null)
            {
                ViewBag.Loi = "Email không đúng.";
                return View();
            }

            // Cập nhật mật khẩu mới
            taiKhoan.MatKhau = matKhauMoi;
            _context.SaveChanges();

            // Xóa session OTP sau khi dùng xong
            HttpContext.Session.Remove("OTP_Code");
            HttpContext.Session.Remove("OTP_User");

            TempData["ThongBao"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";
            return RedirectToAction("Index");
        }

        // 3. Class hứng dữ liệu JSON
        public class OtpRequestModel
        {
            public string TenDangNhap { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        // 4. Hàm gửi email qua SMTP
        private void GuiEmailThucTe(string emailNhan, string tieuDe, string noiDung)
        {
            var senderEmail = _configuration["SmtpSettings:SenderEmail"];
            var senderPassword = _configuration["SmtpSettings:SenderPassword"];
            var host = _configuration["SmtpSettings:Host"];
            int port = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");

            System.Diagnostics.Debug.WriteLine($"EMAIL: [{senderEmail}] - PASS: [{senderPassword}]");

            if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword) || string.IsNullOrEmpty(host))
            {
                throw new Exception("Chưa cấu hình SmtpSettings trong appsettings.json!");
            }

            var message = new System.Net.Mail.MailMessage();
            message.From = new System.Net.Mail.MailAddress(senderEmail, "Hệ thống Cứu hộ");
            message.To.Add(new System.Net.Mail.MailAddress(emailNhan));
            message.Subject = tieuDe;
            message.Body = noiDung;
            message.IsBodyHtml = false;

            using (var client = new System.Net.Mail.SmtpClient(host, port))
            {
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(senderEmail, senderPassword);
                client.Send(message);
            }
        }

        public IActionResult DangXuat()
        {
            NhatKyHelper.GhiLog(_context, HttpContext, "Đăng xuất khỏi hệ thống");
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}