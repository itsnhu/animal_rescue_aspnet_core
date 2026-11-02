using HeThongCuuHo.Helpers;
using HeThongCuuHo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongCuuHo.Controllers
{
    public class CuuHoController : Controller
    {
        private readonly HethongcuuhoDbContext _context;

        public CuuHoController(HethongcuuhoDbContext context)
        {
            _context = context;
        }

        // 1. Danh sách nhiệm vụ cứu hộ
        public IActionResult Index()
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            string? vaiTro = HttpContext.Session.GetString("VaiTro");

            if (vaiTro != "Admin" && vaiTro != "TinhNguyenVien")
            {
                return RedirectToAction("Index", "Home");
            }

            var ds = _context.CuuHos
                .Include(x => x.BaoCao)
                .Include(x => x.TinhNguyenVien)
                .OrderByDescending(x => x.ThoiGianNhan)
                .ToList();

            return View(ds);
        }

        // 2. Trang hiển thị danh sách TNV để phân công thủ công
        [HttpGet]
        public IActionResult PhanCong(int maBaoCao)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            var baoCao = _context.BaoCaos.FirstOrDefault(x => x.MaBaoCao == maBaoCao);

            if (baoCao == null)
                return NotFound();

            if (baoCao.TrangThai != "Chờ xử lý")
            {
                TempData["Loi"] = "Báo cáo này đã được phân công hoặc đã hoàn thành.";
                return RedirectToAction("Index");
            }

            var tinhNguyenViens = _context.TinhNguyenViens
                .Include(x => x.NguoiDung)
                .Where(x => x.TrangThai == "Sẵn sàng" && x.NguoiDung != null)
                //.Where(x => x.TrangThai == "Sẵn sàng")
                .ToList();

            ViewBag.BaoCao = baoCao;

            return View(tinhNguyenViens);
        }

        // 3. Xử lý nhận/giao nhiệm vụ thủ công
        [HttpPost]
        public IActionResult NhanNhiemVu(int maBaoCao, int maTinhNguyenVien)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            bool daPhanCong = _context.CuuHos.Any(x => x.MaBaoCao == maBaoCao);

            if (daPhanCong)
            {
                TempData["Loi"] = "Báo cáo này đã được phân công từ trước.";
                return RedirectToAction("Index");
            }

            var baoCao = _context.BaoCaos.Find(maBaoCao);
            if (baoCao == null)
            {
                TempData["Loi"] = "Không tìm thấy thông tin báo cáo.";
                return RedirectToAction("Index");
            }

            var tinhNguyenVien = _context.TinhNguyenViens.Find(maTinhNguyenVien);
            if (tinhNguyenVien == null)
            {
                TempData["Loi"] = "Không tìm thấy tình nguyện viên.";
                return RedirectToAction("Index");
            }

            if (tinhNguyenVien.TrangThai != "Sẵn sàng")
            {
                TempData["Loi"] = "Tình nguyện viên này đang thực hiện nhiệm vụ khác.";
                return RedirectToAction("Index");
            }

            // Tạo hồ sơ cứu hộ
            var cuuHo = new CuuHo
            {
                MaBaoCao = maBaoCao,
                MaTinhNguyenVien = maTinhNguyenVien,
                ThoiGianNhan = DateTime.Now,
                TrangThai = "Đang cứu hộ"
            };

            _context.CuuHos.Add(cuuHo);

            // Cập nhật trạng thái
            baoCao.TrangThai = "Đang cứu hộ";
            tinhNguyenVien.TrangThai = "Đang làm nhiệm vụ";

            var thongBao = new ThongBao
            {
                MaNguoiDung = tinhNguyenVien.MaNguoiDung,

                TieuDe = "Phân công cứu hộ",

                NoiDung = $"Bạn được phân công nhiệm vụ cứu hộ báo cáo #{baoCao.MaBaoCao}.",

                NgayGui = DateTime.Now,

                DaDoc = false,

                Loai = "Cứu hộ"
            };

            _context.ThongBaos.Add(thongBao);

            _context.SaveChanges();

            // Ghi nhật ký phân công cứu hộ
            NhatKyHelper.GhiLog(
                _context,
                HttpContext,
                "Phân công cứu hộ");

            TempData["ThongBao"] = $"Đã phân công thành công cho TNV {tinhNguyenVien.HoTen}.";
            return RedirectToAction("Index");
        }

        // 4. Công thức Haversine tính khoảng cách thực tế giữa 2 tọa độ GPS (Đơn vị: Kilômét)
        private double TinhKhoangCach(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371; // Bán kính Trái Đất (km)
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c; // Kết quả trả về tính theo km
        }

        private double ToRadians(double val)
        {
            return (Math.PI / 180) * val;
        }

        // 5. Tự động tìm TNV gần nhất và phân công
        public IActionResult TuDongPhanCong(int id)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            bool daPhanCong = _context.CuuHos.Any(x => x.MaBaoCao == id);
            if (daPhanCong)
            {
                TempData["Loi"] = "Báo cáo này đã được phân công.";
                return RedirectToAction("Index");
            }

            var baoCao = _context.BaoCaos.FirstOrDefault(x => x.MaBaoCao == id);
            if (baoCao == null)
            {
                TempData["Loi"] = "Không tìm thấy báo cáo.";
                return RedirectToAction("Index");
            }

            var danhSachTNV = _context.TinhNguyenViens
                .Include(x => x.NguoiDung)
                .Where(x => x.TrangThai == "Sẵn sàng" && x.NguoiDung != null)
                //.Where(x => x.TrangThai == "Sẵn sàng")
                .ToList();

            if (!danhSachTNV.Any())
            {
                TempData["Loi"] = "Không có tình nguyện viên nào ở trạng thái sẵn sàng.";
                return RedirectToAction("Index");
            }

            TinhNguyenVien? ganNhat = null;
            double khoangCachNhoNhat = double.MaxValue;

            foreach (var tnv in danhSachTNV)
            {
                double kc = TinhKhoangCach(baoCao.ViDo, baoCao.KinhDo, tnv.ViDo, tnv.KinhDo);

                if (kc < khoangCachNhoNhat)
                {
                    khoangCachNhoNhat = kc;
                    ganNhat = tnv;
                }
            }

            if (ganNhat == null)
            {
                TempData["Loi"] = "Không thể định vị được tình nguyện viên phù hợp.";
                return RedirectToAction("Index");
            }

            var cuuHo = new CuuHo
            {
                MaBaoCao = baoCao.MaBaoCao,
                MaTinhNguyenVien = ganNhat.MaTinhNguyenVien,
                ThoiGianNhan = DateTime.Now,
                TrangThai = "Đang cứu hộ"
            };

            _context.CuuHos.Add(cuuHo);

            baoCao.TrangThai = "Đang cứu hộ";
            ganNhat.TrangThai = "Đang làm nhiệm vụ";

            var thongBao = new ThongBao
            {
                MaNguoiDung = ganNhat.MaNguoiDung,

                TieuDe = "Phân công tự động",

                NoiDung = $"Bạn được hệ thống tự động phân công cứu hộ báo cáo #{baoCao.MaBaoCao}.",

                NgayGui = DateTime.Now,

                DaDoc = false,

                Loai = "Cứu hộ"
            };

            _context.ThongBaos.Add(thongBao);

            _context.SaveChanges();

            // Ghi nhật ký phân công cứu hộ tự động
            NhatKyHelper.GhiLog(
                _context,
                HttpContext,
                "Phân công cứu hộ");

            TempData["ThongBao"] = $"Đã tự động phân công TNV {ganNhat.HoTen} (cách hiện trường ~{khoangCachNhoNhat:0.##} km).";
            return RedirectToAction("Index");
        }
       

        // Xem vị trí hiện trường cứu hộ
        public IActionResult ViTri(int id)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            var cuuHo = _context.CuuHos
                .Include(x => x.BaoCao)
                .Include(x => x.TinhNguyenVien)
                .FirstOrDefault(x => x.MaCuuHo == id);

            if (cuuHo == null)
            {
                TempData["Loi"] = "Không tìm thấy nhiệm vụ cứu hộ.";
                return RedirectToAction(nameof(Index));
            }

            if (cuuHo.BaoCao == null)
            {
                TempData["Loi"] = "Báo cáo không có dữ liệu vị trí.";
                return RedirectToAction(nameof(Index));
            }

            return View(cuuHo);
        }

        // 6. Cập nhật trạng thái Hoàn thành cứu hộ & Khởi tạo hồ sơ Động vật
        [HttpPost]
        public IActionResult CapNhatTrangThai(int id)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin", "TinhNguyenVien"))
            {
                return RedirectToAction("Index", "Home");
            }

            var cuuHo = _context.CuuHos
                .Include(x => x.BaoCao)
                .FirstOrDefault(x => x.MaCuuHo == id);

            if (cuuHo == null)
            {
                TempData["Loi"] = "Không tìm thấy nhiệm vụ cứu hộ.";
                return RedirectToAction("Index");
            }

            if (cuuHo.TrangThai == "Hoàn thành")
            {
                TempData["Loi"] = "Nhiệm vụ này đã hoàn thành trước đó.";
                return RedirectToAction("Index");
            }

            // Cập nhật thông tin ca cứu hộ
            cuuHo.TrangThai = "Hoàn thành";
            cuuHo.ThoiGianHoanThanh = DateTime.Now;

            if (cuuHo.BaoCao != null)
            {
                cuuHo.BaoCao.TrangThai = "Hoàn thành";
            }

            // Trả Tình nguyện viên về trạng thái "Sẵn sàng"
            var tnv = _context.TinhNguyenViens.Find(cuuHo.MaTinhNguyenVien);
            if (tnv != null)
            {
                tnv.TrangThai = "Sẵn sàng";
            }

            // Tự động thêm Động vật vào Trạm Cứu hộ nếu chưa có
            bool daTonTaiDongVat = _context.DongVats.Any(x => x.MaBaoCao == cuuHo.MaBaoCao);

            if (!daTonTaiDongVat && cuuHo.BaoCao != null)
            {
                var tram = _context.TramCuuHos.FirstOrDefault();

                if (tram == null)
                {
                    TempData["Loi"] = "Cần khởi tạo ít nhất 1 Trạm Cứu Hộ trong hệ thống trước.";
                    return RedirectToAction("Index");
                }

                var dongVat = new DongVat
                {
                    Ten = "Chờ đặt tên",
                    Loai = cuuHo.BaoCao.LoaiDongVat,
                    Giong = "Chưa rõ",
                    GioiTinh = "Chưa rõ",
                    Tuoi = 0,
                    TinhTrangSucKhoe = "Cần kiểm tra y tế",
                    NgayTiepNhan = DateTime.Now,
                    MaBaoCao = cuuHo.MaBaoCao,
                    MaTram = tram.MaTram
                };

                _context.DongVats.Add(dongVat);
            }

            if (cuuHo.BaoCao != null)
            {
                var thongBao = new ThongBao
                {
                    MaNguoiDung = cuuHo.BaoCao.MaNguoiDung,

                    TieuDe = "Hoàn thành cứu hộ",

                    NoiDung = "Báo cáo cứu hộ của bạn đã được hoàn thành.",

                    NgayGui = DateTime.Now,

                    DaDoc = false,

                    Loai = "Cứu hộ"
                };

                _context.ThongBaos.Add(thongBao);
            }

            _context.SaveChanges();

            NhatKyHelper.GhiLog(
               _context, HttpContext,"Hoàn thành nhiệm vụ cứu hộ");

            TempData["ThongBao"] = "Cập nhật thành công! Đã hoàn thành nhiệm vụ và lưu hồ sơ động vật.";
            return RedirectToAction("Index");
        }

        // 7. Chi tiết nhiệm vụ cứu hộ
        [HttpGet]
        public IActionResult ChiTiet(int id)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            var cuuHo = _context.CuuHos
                .Include(x => x.BaoCao)
                .ThenInclude(b => b.NguoiDung)
                .Include(x => x.TinhNguyenVien)
                .ThenInclude(x => x.NguoiDung)
                .FirstOrDefault(x => x.MaCuuHo == id);

            if (cuuHo == null)
            {
                return NotFound();
            }

            return View(cuuHo);
        }

        [HttpPost]
        public IActionResult HoanThanhCuuHo(int maBaoCao)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin", "TinhNguyenVien"))
            {
                return RedirectToAction("Index", "Home");
            }

            // Tìm hồ sơ cứu hộ theo Mã Báo Cáo
            var cuuHo = _context.CuuHos
                .Include(x => x.BaoCao)
                .FirstOrDefault(x => x.MaBaoCao == maBaoCao);

            if (cuuHo == null)
            {
                TempData["Loi"] = "Không tìm thấy nhiệm vụ cứu hộ liên quan đến báo cáo này.";
                return RedirectToAction("ChiTiet", "BaoCao", new { id = maBaoCao });
            }

            if (cuuHo.TrangThai == "Hoàn thành")
            {
                TempData["Loi"] = "Nhiệm vụ này đã hoàn thành trước đó.";
                return RedirectToAction("ChiTiet", "BaoCao", new { id = maBaoCao });
            }

            // 1. Cập nhật trạng thái Cứu Hộ & Báo Cáo
            cuuHo.TrangThai = "Hoàn thành";
            cuuHo.ThoiGianHoanThanh = DateTime.Now;

            if (cuuHo.BaoCao != null)
            {
                cuuHo.BaoCao.TrangThai = "Hoàn thành";
            }

            // 2. Trả Tình nguyện viên về trạng thái "Sẵn sàng"
            var tnv = _context.TinhNguyenViens.Find(cuuHo.MaTinhNguyenVien);
            if (tnv != null)
            {
                tnv.TrangThai = "Sẵn sàng";
            }

            // 3. Tự động thêm Động vật vào Trạm Cứu hộ (nếu chưa có)
            bool daTonTaiDongVat = _context.DongVats.Any(x => x.MaBaoCao == cuuHo.MaBaoCao);
            if (!daTonTaiDongVat && cuuHo.BaoCao != null)
            {
                var tram = _context.TramCuuHos.FirstOrDefault();
                if (tram != null)
                {
                    var dongVat = new DongVat
                    {
                        Ten = "Chờ đặt tên",
                        Loai = cuuHo.BaoCao.LoaiDongVat,
                        Giong = "Chưa rõ",
                        GioiTinh = "Chưa rõ",
                        Tuoi = 0,
                        TinhTrangSucKhoe = "Cần kiểm tra y tế",
                        NgayTiepNhan = DateTime.Now,
                        MaBaoCao = cuuHo.MaBaoCao,
                        MaTram = tram.MaTram
                    };
                    _context.DongVats.Add(dongVat);
                }
            }

            // 4. Gửi thông báo cho người báo cáo
            if (cuuHo.BaoCao != null)
            {
                var thongBao = new ThongBao
                {
                    MaNguoiDung = cuuHo.BaoCao.MaNguoiDung,
                    TieuDe = "Hoàn thành cứu hộ",
                    NoiDung = $"Báo cáo cứu hộ #{cuuHo.MaBaoCao} đã hoàn thành.",
                    NgayGui = DateTime.Now,
                    DaDoc = false,
                    Loai = "Cứu hộ"
                };
                _context.ThongBaos.Add(thongBao);
            }

            _context.SaveChanges();

            NhatKyHelper.GhiLog(_context, HttpContext, "Hoàn thành nhiệm vụ cứu hộ");

            TempData["ThongBao"] = "Cập nhật cứu hộ thành công!";
            return RedirectToAction("ChiTiet", "BaoCao", new { id = maBaoCao });
        }

                // ==========================================================
        // BỔ SUNG: CÁC API PHỤC VỤ AJAX CHO GIAO DIỆN ĐIỀU PHỐI MƯỢT
        // ==========================================================

        [HttpGet]
        public IActionResult GetDanhSachTnvSanSang()
        {
            var ds = _context.TinhNguyenViens
                .Include(x => x.NguoiDung)
                .Where(x => x.TrangThai == "Sẵn sàng" && x.NguoiDung != null)
                .Select(x => new { 
                    maTinhNguyenVien = x.MaTinhNguyenVien, 
                    hoTen = x.HoTen, 
                    soDienThoai = x.SoDienThoai 
                })
                .ToList();
            return Json(ds);
        }

        [HttpPost]
        public IActionResult XuLyDieuPhoiAjax(int maBaoCao, int maTinhNguyenVien)
        {
            try
            {
                bool daPhanCong = _context.CuuHos.Any(x => x.MaBaoCao == maBaoCao);
                if (daPhanCong) return Json(new { success = false, message = "Báo cáo này đã được phân công từ trước." });

                var baoCao = _context.BaoCaos.Find(maBaoCao);
                if (baoCao == null) return Json(new { success = false, message = "Không tìm thấy báo cáo." });

                var tinhNguyenVien = _context.TinhNguyenViens.Find(maTinhNguyenVien);
                if (tinhNguyenVien == null) return Json(new { success = false, message = "Không tìm thấy tình nguyện viên." });

                if (tinhNguyenVien.TrangThai != "Sẵn sàng") 
                    return Json(new { success = false, message = "Tình nguyện viên này vừa nhận nhiệm vụ khác." });

                // Tạo hồ sơ cứu hộ
                var cuuHo = new CuuHo
                {
                    MaBaoCao = maBaoCao,
                    MaTinhNguyenVien = maTinhNguyenVien,
                    ThoiGianNhan = DateTime.Now,
                    TrangThai = "Đang cứu hộ"
                };
                _context.CuuHos.Add(cuuHo);

                // Cập nhật trạng thái
                baoCao.TrangThai = "Đang cứu hộ";
                tinhNguyenVien.TrangThai = "Đang làm nhiệm vụ";

                // Tạo thông báo
                var thongBao = new ThongBao
                {
                    MaNguoiDung = tinhNguyenVien.MaNguoiDung,
                    TieuDe = "Phân công cứu hộ",
                    NoiDung = $"Bạn được phân công nhiệm vụ cứu hộ báo cáo #{baoCao.MaBaoCao}.",
                    NgayGui = DateTime.Now,
                    DaDoc = false,
                    Loai = "Cứu hộ"
                };
                _context.ThongBaos.Add(thongBao);

                _context.SaveChanges();

                NhatKyHelper.GhiLog(_context, HttpContext, "Phân công cứu hộ (AJAX)");

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

    }
}