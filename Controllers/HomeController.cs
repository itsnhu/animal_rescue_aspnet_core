using HeThongCuuHo.Helpers;
using HeThongCuuHo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace HeThongCuuHo.Controllers
{
    public class HomeController : Controller
    {
        private readonly HethongcuuhoDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public HomeController(HethongcuuhoDbContext context, IWebHostEnvironment env, IConfiguration config)
        {
            _context = context;
            _env = env;
            _config = config;
        }

        public IActionResult Index()
        {
            // Kiểm tra đăng nhập
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            string? vaiTro = HttpContext.Session.GetString("VaiTro");

            if (vaiTro != "Admin")
            {
                return RedirectToAction("Dashboard", "Home");
            }

            // ================= THỐNG KÊ =================

            ViewBag.SoBaoCao = _context.BaoCaos.Count();

            ViewBag.SoCuuHo = _context.CuuHos
                .Count(x => x.TrangThai != "Hoàn thành");

            ViewBag.SoDieuTri = _context.HoSoYTes.Count();

            ViewBag.SoNhanNuoi = _context.DonNhanNuois.Count();

            ViewBag.SoDongVat = _context.DongVats.Count();

            ViewBag.SoTinhNguyenVien = _context.TinhNguyenViens.Count();

            // ================= BÁO CÁO =================

            ViewBag.BaoCaoChoXuLy = _context.BaoCaos
                .Count(x => x.TrangThai == "Chờ xử lý");

            ViewBag.BaoCaoDangCuuHo = _context.BaoCaos
                .Count(x => x.TrangThai == "Đang cứu hộ");

            ViewBag.BaoCaoHoanThanh = _context.BaoCaos
                .Count(x => x.TrangThai == "Hoàn thành");

            // ================= NHẬN NUÔI =================

            ViewBag.DonChoDuyet = _context.DonNhanNuois
                .Count(x => x.TrangThai == "Chờ duyệt");

            ViewBag.DonDaDuyet = _context.DonNhanNuois
                .Count(x => x.TrangThai == "Đã duyệt");

            ViewBag.DonTuChoi = _context.DonNhanNuois
                .Count(x => x.TrangThai == "Từ chối");

            // ================= 5 BÁO CÁO MỚI =================

            ViewBag.BaoCaoMoi = _context.BaoCaos
                .OrderByDescending(x => x.NgayBaoCao)
                .Take(5)
                .ToList();

            // ================= BIỂU ĐỒ =================

            int[] duLieu = new int[12];

            for (int thang = 1; thang <= 12; thang++)
            {
                duLieu[thang - 1] = _context.BaoCaos
                    .Count(x => x.NgayBaoCao.Month == thang);
            }

            ViewBag.SoBaoCaoTheoThang = JsonSerializer.Serialize(duLieu);

            return View();
        }

        // Action mới dành riêng cho trang chủ của User
        public IActionResult Dashboard()
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            // Nếu là Admin thì vẫn trả về trang thống kê chính
            string? vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro == "Admin")
            {
                return RedirectToAction("Index", "Home");
            }

            // Nếu là User thường, trả về view Dashboard cá nhân
            return View("UserDashboard");
        }

        public IActionResult Welcome()
        {
            var dsDongVat = _context.DongVats
                .Include(x => x.TramCuuHo)
                .OrderByDescending(x => x.NgayTiepNhan)
                .ToList();

            return View(dsDongVat);
        }

        [HttpGet]
        public IActionResult GetPetsApi()
        {
            var ds = _context.DongVats
                .Include(x => x.TramCuuHo)
                .Select(x => new
                {
                    maDongVat = x.MaDongVat,
                    ten = x.Ten,
                    loai = x.Loai,
                    giong = x.Giong,
                    gioiTinh = x.GioiTinh,
                    tuoi = x.Tuoi,
                    tinhTrangSucKhoe = x.TinhTrangSucKhoe,
                    trangThai = x.TrangThai,
                    hinhAnh = x.HinhAnh,
                    tenTram = x.TramCuuHo != null ? x.TramCuuHo.TenTram : "Trạm cứu hộ chính RescuePets"
                })
                .ToList();

            return Json(ds);
        }

        [HttpPost]
        public IActionResult GuiDonNhanNuoiAjax([FromBody] DonNhanNuoiAjaxModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.HoTen) || string.IsNullOrWhiteSpace(model.SoDienThoai))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin bắt buộc!" });
            }

            try
            {
                int maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung") ?? HttpContext.Session.GetInt32("MaTaiKhoan") ?? 0;
                
                var don = new DonNhanNuoi
                {
                    MaDongVat = model.MaDongVat > 0 ? model.MaDongVat : 1,
                    MaNguoiDung = maNguoiDung > 0 ? maNguoiDung : 1, // Fallback to public guest / account 1
                    NgayDangKy = DateTime.Now,
                    LyDo = $"[Họ tên: {model.HoTen} | SĐT: {model.SoDienThoai} | Email: {model.Email} | Địa chỉ: {model.DiaChi} | Kinh nghiệm: {model.KinhNghiem}] - {model.LyDo}",
                    TrangThai = "Chờ duyệt"
                };

                _context.DonNhanNuois.Add(don);
                _context.SaveChanges();

                return Json(new { success = true, message = "Gửi đơn nhận nuôi thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult BaoCao()
        {
            // Trả về file Views/Home/BaoCao.cshtml kèm theo một Model rỗng để đổ dữ liệu form
            return View(new BaoCao());
        }

        // Bổ sung thêm hàm [HttpPost] này để hứng dữ liệu gửi lên từ form
        [HttpPost]
        public IActionResult BaoCao(BaoCao model, IFormFile? file, string? HoTenNguoiGui, string? SoDienThoai)
        {
            // Dùng nameof(model.Trường) để tránh xung đột tên với hàm BaoCao()
            ModelState.Remove(nameof(model.MaNguoiDung));
            ModelState.Remove(nameof(model.NgayBaoCao));
            ModelState.Remove(nameof(model.TrangThai));
            ModelState.Remove(nameof(model.ViDo));
            ModelState.Remove(nameof(model.KinhDo));
            ModelState.Remove(nameof(model.NguoiDung));
            ModelState.Remove(nameof(model.HinhAnh));

            if (ModelState.IsValid)
            {
                model.NgayBaoCao = DateTime.Now;
                model.TrangThai = "Chờ xử lý";

                int? maNguoiDung = HttpContext.Session.GetInt32("MaNguoiDung") ?? HttpContext.Session.GetInt32("MaTaiKhoan");
                if (maNguoiDung.HasValue && maNguoiDung.Value > 0)
                {
                    model.MaNguoiDung = maNguoiDung.Value;
                }
                else if (!string.IsNullOrEmpty(SoDienThoai))
                {
                    model.MoTa = $"[SĐT: {SoDienThoai} - Họ tên: {HoTenNguoiGui}] | " + model.MoTa;
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
                _context.SaveChanges();

                return RedirectToAction("Welcome");
            }

            ViewBag.Loi = "Dữ liệu nhập chưa hợp lệ, vui lòng kiểm tra lại!";
            return View(model);
        }

        public IActionResult VeChungToi()
        {
            return View();
        }

        public IActionResult TinTuc()
        {
            return View();
        }

        // ================= BỔ SUNG ACTION CONTACT =================
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }
        // ==========================================================

        [HttpPost]
        public async Task<IActionResult> SendContact([FromBody] ContactModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Name))
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
            }

            try
            {
                // Đọc đúng các key theo file appsettings.json của bạn
                var host = _config["SmtpSettings:Host"] ?? "smtp.gmail.com";
                var port = int.Parse(_config["SmtpSettings:Port"] ?? "587");
                var senderEmail = _config["SmtpSettings:SenderEmail"];
                var senderPassword = _config["SmtpSettings:SenderPassword"]; // Đã khớp với appsettings.json

                using (var client = new SmtpClient(host, port))
                {
                    client.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    client.EnableSsl = true;

                    var mail = new MailMessage();
                    mail.From = new MailAddress(senderEmail!, "Hệ thống RescuePets");
                    mail.To.Add("0023413114@student.dthu.edu.vn"); // Email nhận tin nhắn báo ca
                    mail.Subject = $"[RescuePets - Phản hồi/Cứu hộ] Từ: {model.Name}";
                    mail.Body = $@"
                <h3>Thông tin phản hồi / Báo ca cứu hộ mới</h3>
                <p><b>Họ và tên:</b> {model.Name}</p>
                <p><b>Số điện thoại:</b> {model.Phone}</p>
                <p><b>Email:</b> {model.Email}</p>
                <p><b>Nội dung / Địa điểm:</b></p>
                <p>{model.Message}</p>
            ";
                    mail.IsBodyHtml = true;

                    await client.SendMailAsync(mail);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ================= ĐĂNG KÝ NHẬN BẢN TIN (NEWSLETTER) =================
        [HttpPost]
        public async Task<IActionResult> SubscribeNewsletter([FromBody] NewsletterModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email))
            {
                return Json(new { success = false, message = "Vui lòng nhập địa chỉ email hợp lệ!" });
            }

            try
            {
                var host = _config["SmtpSettings:Host"] ?? "smtp.gmail.com";
                var port = int.Parse(_config["SmtpSettings:Port"] ?? "587");
                var senderEmail = _config["SmtpSettings:SenderEmail"];
                var senderPassword = _config["SmtpSettings:SenderPassword"];

                using (var client = new SmtpClient(host, port))
                {
                    client.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    client.EnableSsl = true;

                    // 1. Gửi email xác nhận đến người đăng ký
                    var confirmMail = new MailMessage();
                    confirmMail.From = new MailAddress(senderEmail!, "RescuePets - Bản Tin Cứu Hộ");
                    confirmMail.To.Add(model.Email);
                    confirmMail.Subject = "🐾 Chào mừng bạn đến với Bản Tin Cứu Hộ RescuePets!";
                    confirmMail.IsBodyHtml = true;
                    confirmMail.Body = $@"
                    <div style='max-width:600px; margin:0 auto; font-family:Arial,sans-serif;'>
                        <div style='background:linear-gradient(135deg,#0F3832,#071D2A); padding:30px; text-align:center; border-radius:16px 16px 0 0;'>
                            <div style='font-size:2.5rem;'>🐾</div>
                            <h1 style='color:#D4A373; margin:10px 0 5px; font-size:1.5rem;'>RescuePets</h1>
                            <p style='color:rgba(255,255,255,0.7); font-size:0.85rem; margin:0;'>Trạm Cứu Hộ & Bảo Trợ Động Vật</p>
                        </div>
                        <div style='background:#ffffff; padding:30px; border:1px solid #e0e0e0;'>
                            <h2 style='color:#071D2A; margin-top:0;'>Đăng ký nhận bản tin thành công! 🎉</h2>
                            <p style='color:#555; line-height:1.7;'>
                                Xin chào <strong>{model.Email}</strong>,<br><br>
                                Cảm ơn bạn đã đăng ký nhận <strong>Bản Tin Cứu Hộ</strong> từ RescuePets! 
                                Từ bây giờ, bạn sẽ nhận được những cập nhật mới nhất về:
                            </p>
                            <ul style='color:#555; line-height:2;'>
                                <li>🔔 Thông báo các ca cứu hộ khẩn cấp</li>
                                <li>📅 Lịch sự kiện nhận nuôi thú cưng</li>
                                <li>📸 Ảnh & video các bé đang phục hồi</li>
                                <li>❤️ Câu chuyện cảm động từ cộng đồng</li>
                            </ul>
                            <p style='color:#555; line-height:1.7;'>
                                Mỗi tuần, chúng tôi sẽ gửi đến bạn những thông tin hữu ích và cập nhật mới nhất từ trạm cứu hộ.
                            </p>
                            <div style='text-align:center; margin:25px 0;'>
                                <a href='https://localhost' style='background:#0F3832; color:#D4A373; text-decoration:none; padding:12px 30px; border-radius:50px; font-weight:bold; font-size:0.9rem;'>
                                    KHÁM PHÁ RESCUEPETS
                                </a>
                            </div>
                        </div>
                        <div style='background:#071D2A; padding:20px; text-align:center; border-radius:0 0 16px 16px;'>
                            <p style='color:rgba(255,255,255,0.5); font-size:0.75rem; margin:0;'>
                                © 2026 RescuePets Vietnam · Bạn nhận email này vì đã đăng ký bản tin tại RescuePets.
                            </p>
                        </div>
                    </div>";

                    await client.SendMailAsync(confirmMail);

                    // 2. Gửi thông báo cho admin về người đăng ký mới
                    var adminMail = new MailMessage();
                    adminMail.From = new MailAddress(senderEmail!, "RescuePets System");
                    adminMail.To.Add("0023413114@student.dthu.edu.vn");
                    adminMail.Subject = $"[RescuePets] Đăng ký bản tin mới: {model.Email}";
                    adminMail.IsBodyHtml = true;
                    adminMail.Body = $@"
                    <h3>📬 Có người đăng ký nhận bản tin mới!</h3>
                    <p><b>Email:</b> {model.Email}</p>
                    <p><b>Thời gian:</b> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                    <p style='color:#888;'>Email xác nhận đã được gửi tự động đến người đăng ký.</p>";

                    await client.SendMailAsync(adminMail);
                }

                return Json(new { success = true, message = "Đăng ký nhận bản tin thành công! Email xác nhận đã được gửi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi gửi email: " + ex.Message });
            }
        }

        // ================= GỬI EMAIL CẢM ƠN ỦNG HỘ TÀI CHÍNH =================
        [HttpPost]
        public async Task<IActionResult> GuiEmailUngHo([FromBody] DonationNotificationModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.LienHe))
            {
                return Json(new { success = false, message = "Thông tin không hợp lệ!" });
            }

            string email = model.LienHe.Trim();
            bool isEmail = email.Contains("@") && email.Contains(".");

            if (!isEmail)
            {
                return Json(new { success = true, isEmail = false, message = "Đã ghi nhận thông tin ủng hộ!" });
            }

            try
            {
                var host = _config["SmtpSettings:Host"] ?? "smtp.gmail.com";
                var port = int.Parse(_config["SmtpSettings:Port"] ?? "587");
                var senderEmail = _config["SmtpSettings:SenderEmail"];
                var senderPassword = _config["SmtpSettings:SenderPassword"];

                using (var client = new SmtpClient(host, port))
                {
                    client.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    client.EnableSsl = true;

                    string formattedAmount = model.SoTien > 0 ? $"{model.SoTien:N0} VNĐ" : "Tùy tâm";
                    string donorName = string.IsNullOrWhiteSpace(model.HoTen) ? "Mạnh Thường Quân" : model.HoTen;

                    // Gửi email cảm ơn đến người ủng hộ
                    var thankMail = new MailMessage();
                    thankMail.From = new MailAddress(senderEmail!, "RescuePets - Tri Ân Ủng Hộ");
                    thankMail.To.Add(email);
                    thankMail.Subject = $"❤️ Cảm ơn {donorName} đã ủng hộ trạm cứu hộ RescuePets!";
                    thankMail.IsBodyHtml = true;
                    thankMail.Body = $@"
                    <div style='max-width:600px; margin:0 auto; font-family:Arial,sans-serif; background:#f9fafb; padding:20px;'>
                        <div style='background:linear-gradient(135deg,#0F3832,#071D2A); padding:35px 25px; text-align:center; border-radius:16px 16px 0 0;'>
                            <div style='font-size:3rem; margin-bottom:10px;'>🐾❤️</div>
                            <h1 style='color:#D4A373; margin:0; font-size:1.6rem; font-family:Georgia,serif;'>THƯ CẢM ƠN TẤM LÒNG VÀNG</h1>
                            <p style='color:rgba(255,255,255,0.8); font-size:0.9rem; margin-top:5px;'>Trạm Cứu Hộ & Bảo Trợ Động Vật RescuePets</p>
                        </div>
                        <div style='background:#ffffff; padding:30px; border:1px solid #e5e7eb;'>
                            <h2 style='color:#0F3832; margin-top:0; font-size:1.25rem;'>Kính gửi Quý Mạnh Thường Quân: <span style='color:#e11d48;'>{donorName}</span>,</h2>
                            <p style='color:#4b5563; line-height:1.8; font-size:0.95rem;'>
                                Thay mặt toàn thể đội ngũ tình nguyện viên và các bé chó mèo tại <strong>RescuePets</strong>, chúng tôi xin gửi lời tri ân sâu sắc nhất đến tấm lòng nhân ái của bạn.
                            </p>
                            
                            <div style='background:#f3f4f6; border-left:4px solid #D4A373; padding:18px 20px; border-radius:8px; margin:20px 0;'>
                                <h3 style='margin:0 0 10px; color:#1f2937; font-size:1rem;'>Chi Tiết Ủng Hộ</h3>
                                <p style='margin:4px 0; color:#4b5563; font-size:0.9rem;'><strong>Người gửi:</strong> {donorName}</p>
                                <p style='margin:4px 0; color:#4b5563; font-size:0.9rem;'><strong>Email / SĐT:</strong> {email}</p>
                                <p style='margin:4px 0; color:#4b5563; font-size:0.9rem;'><strong>Số tiền ủng hộ:</strong> <span style='color:#e11d48; font-weight:bold; font-size:1.05rem;'>{formattedAmount}</span></p>
                                <p style='margin:4px 0; color:#4b5563; font-size:0.9rem;'><strong>Tài khoản nhận:</strong> MBBank · 838226969 · NGUYEN NGOC NHU</p>
                                <p style='margin:4px 0; color:#4b5563; font-size:0.9rem;'><strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                            </div>

                            <p style='color:#4b5563; line-height:1.8; font-size:0.95rem;'>
                                Mọi đóng góp của bạn đều được chúng tôi sử dụng <strong>100% minh bạch</strong> cho:
                            </p>
                            <ul style='color:#4b5563; line-height:1.9; font-size:0.95rem;'>
                                <li>💊 Chi phí phẫu thuật, thuốc men và tiêm phòng cho các ca cấp cứu.</li>
                                <li>🥣 Thức ăn hạt dinh dưỡng và đồ dùng sinh hoạt hàng ngày.</li>
                                <li>🏡 Chi phí duy trì mái ấm an toàn cho các bé trước khi tìm được gia đình mới.</li>
                            </ul>

                            <p style='color:#4b5563; line-height:1.8; font-size:0.95rem;'>
                                Chúc bạn và gia đình luôn dồi dào sức khỏe, niềm vui và hạnh phúc trong cuộc sống!
                            </p>

                            <div style='text-align:center; margin-top:25px;'>
                                <a href='https://localhost' style='background:#0F3832; color:#D4A373; text-decoration:none; padding:12px 28px; border-radius:50px; font-weight:bold; font-size:0.85rem; display:inline-block;'>
                                    THEO DÕI HOẠT ĐỘNG CỨU HỘ
                                </a>
                            </div>
                        </div>
                        <div style='background:#071D2A; padding:20px; text-align:center; border-radius:0 0 16px 16px;'>
                            <p style='color:rgba(255,255,255,0.6); font-size:0.75rem; margin:0;'>
                                © 2026 RescuePets Vietnam · Số 88 Đường Cứu Trợ, Hà Nội · Hotline: +84 (0988) 789 012
                            </p>
                        </div>
                    </div>";

                    await client.SendMailAsync(thankMail);

                    // Gửi thông báo đến email Admin
                    try
                    {
                        var adminMail = new MailMessage();
                        adminMail.From = new MailAddress(senderEmail!, "RescuePets Donation Alert");
                        adminMail.To.Add("0023413114@student.dthu.edu.vn");
                        adminMail.Subject = $"[RescuePets] Có lượt ủng hộ tài chính mới từ {donorName} ({formattedAmount})";
                        adminMail.IsBodyHtml = true;
                        adminMail.Body = $@"
                        <h3>💰 Có thông tin ủng hộ tài chính mới!</h3>
                        <p><b>Họ tên:</b> {donorName}</p>
                        <p><b>Liên hệ:</b> {email}</p>
                        <p><b>Số tiền:</b> {formattedAmount}</p>
                        <p><b>Thời gian:</b> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>";
                        await client.SendMailAsync(adminMail);
                    }
                    catch { }
                }

                return Json(new { success = true, isEmail = true, message = "Đã gửi thư cảm ơn đến email của bạn!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi gửi email: " + ex.Message });
            }
        }
    }

    public class DonationNotificationModel
    {
        public string HoTen { get; set; } = string.Empty;
        public string LienHe { get; set; } = string.Empty;
        public decimal SoTien { get; set; } = 0;
    }

    public class ContactModel
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
    }

    public class DonNhanNuoiAjaxModel
    {
        public int MaDongVat { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public string KinhNghiem { get; set; } = string.Empty;
        public string LyDo { get; set; } = string.Empty;
    }

    public class NewsletterModel
    {
        public string Email { get; set; } = string.Empty;
    }
}