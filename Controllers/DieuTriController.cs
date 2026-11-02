using HeThongCuuHo.Helpers;
using HeThongCuuHo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HeThongCuuHo.Controllers
{
    public class DieuTriController : Controller
    {
        private readonly HethongcuuhoDbContext _context;

        public DieuTriController(HethongcuuhoDbContext context)
        {
            _context = context;
        }

        // Danh sách hồ sơ điều trị
        public IActionResult Index()
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin", "TinhNguyenVien"))
            {
                return RedirectToAction("Index", "Home");
            }

            // Gọi DbSet HoSoYTes (đã map vào bảng DieuTri)
            var dsDieuTri = _context.HoSoYTes
                .Include(x => x.DongVat)
                .ToList();

            return View(dsDieuTri);
        }

        // Hiển thị form thêm
        [HttpGet]
        public IActionResult Them(int? maDongVat)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin", "TinhNguyenVien"))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.DongVat = _context.DongVats.ToList();

            HoSoDieuTri model = new HoSoDieuTri();

            if (maDongVat != null)
            {
                model.MaDongVat = maDongVat.Value;
                model.NgayKham = DateTime.Now;
            }

            return View(model);
        }

        public IActionResult ChiTiet(int id)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin", "TinhNguyenVien"))
            {
                return RedirectToAction("Index", "Home");
            }

            var hs = _context.HoSoYTes
                .Include(x => x.DongVat)
                .FirstOrDefault(x => x.MaHoSo == id);

            if (hs == null)
            {
                return NotFound();
            }

            return View(hs);
        }

        [HttpGet]
        public IActionResult Sua(int id)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin", "TinhNguyenVien"))
            {
                return RedirectToAction("Index", "Home");
            }

            var hs = _context.HoSoYTes.Find(id);

            if (hs == null)
            {
                return NotFound();
            }

            ViewBag.DongVat = _context.DongVats.ToList();

            return View(hs);
        }

        [HttpPost]
        public IActionResult Sua(HoSoDieuTri model)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin", "TinhNguyenVien"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DongVat = _context.DongVats.ToList();
                return View(model);
            }

            var hs = _context.HoSoYTes.Find(model.MaHoSo);

            if (hs == null)
            {
                TempData["Loi"] = "Không tìm thấy hồ sơ điều trị.";
                return RedirectToAction(nameof(Index));
            }

            hs.MaDongVat = model.MaDongVat;
            hs.ChanDoan = model.ChanDoan;
            hs.DieuTri = model.DieuTri;
            hs.NgayKham = model.NgayKham;
            hs.ChiPhi = model.ChiPhi;
            hs.TrangThai = model.TrangThai;
            hs.GhiChu = model.GhiChu;

            _context.SaveChanges();

            // Ghi nhật ký cập nhật điều trị
            NhatKyHelper.GhiLog(
                _context,
                HttpContext,
                "Cập nhật điều trị");

            TempData["ThongBao"] = "Đã cập nhật hồ sơ.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Them(HoSoDieuTri model)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin", "TinhNguyenVien"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DongVat = _context.DongVats.ToList();
                return View(model);
            }

            var dongVat = _context.DongVats.Find(model.MaDongVat);

            if (dongVat == null)
            {
                TempData["Loi"] = "Không tìm thấy động vật.";
                return RedirectToAction(nameof(Index));
            }

            dongVat.TinhTrangSucKhoe = "Đang điều trị";

            _context.HoSoYTes.Add(model);
            _context.SaveChanges();

            // Ghi nhật ký thêm mới / cập nhật điều trị
            NhatKyHelper.GhiLog(
                _context,
                HttpContext,
                "Cập nhật điều trị");

            TempData["ThongBao"] = "Thêm hồ sơ điều trị thành công.";

            return RedirectToAction(nameof(Index));
        }

        // Cập nhật hoàn thành đơn lẻ (Giữ nguyên)
        public IActionResult HoanThanh(int id)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin", "TinhNguyenVien"))
            {
                return RedirectToAction("Index", "Home");
            }

            var hs = _context.HoSoYTes
                .Include(x => x.DongVat)
                .FirstOrDefault(x => x.MaHoSo == id);

            if (hs == null)
            {
                TempData["Loi"] = "Không tìm thấy hồ sơ điều trị.";
                return RedirectToAction(nameof(Index));
            }

            hs.GhiChu = "Đã khỏi";
            hs.TrangThai = "Hoàn thành";

            if (hs.DongVat != null)
            {
                hs.DongVat.TinhTrangSucKhoe = "Đã khỏe";
            }

            _context.SaveChanges();

            // Ghi nhật ký hoàn thành điều trị
            NhatKyHelper.GhiLog(
                _context,
                HttpContext,
                "Cập nhật điều trị");

            TempData["ThongBao"] = "Điều trị hoàn thành.";

            return RedirectToAction(nameof(Index));
        }

        // >>> MỚI THÊM: Cập nhật hoàn thành HÀNG LOẠT (Dành cho AJAX) <<<
        [HttpPost]
        public IActionResult HoanThanhHangLoat(List<int> ids)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin", "TinhNguyenVien"))
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này." });
            }

            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "Chưa chọn hồ sơ nào." });

            try
            {
                // Lấy danh sách hồ sơ cần cập nhật lên RAM trước
                var allHoSo = _context.HoSoYTes.Include(x => x.DongVat).ToList();
                
                var dsHoSoCanDuyet = allHoSo.Where(x => ids.Contains(x.MaHoSo) 
                                       && x.TrangThai != "Hoàn thành" 
                                       && x.GhiChu != "Đã khỏi").ToList();

                if (!dsHoSoCanDuyet.Any())
                    return Json(new { success = false, message = "Không tìm thấy hồ sơ hợp lệ để cập nhật." });

                foreach (var item in dsHoSoCanDuyet)
                {
                    item.TrangThai = "Hoàn thành";
                    item.GhiChu = "Đã khỏi"; 
                    
                    if (item.DongVat != null)
                    {
                        item.DongVat.TinhTrangSucKhoe = "Đã khỏe";
                    }
                }

                _context.SaveChanges();
                
                NhatKyHelper.GhiLog(_context, HttpContext, "Cập nhật điều trị hàng loạt");
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi Server: " + ex.Message });
            }
        }
    }
}