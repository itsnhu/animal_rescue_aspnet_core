using HeThongCuuHo.Helpers;
using HeThongCuuHo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HeThongCuuHo.Controllers
{
    public class LuuTruController : Controller
    {
        private readonly HethongcuuhoDbContext _context;

        public LuuTruController(HethongcuuhoDbContext context)
        {
            _context = context;
        }

        // Danh sách lưu trú
        public IActionResult Index()
        {
            var ds = _context.LuuTrus
                .Include(x => x.DongVat)
                .Include(x => x.TramCuuHo)
                .ToList();

            return View(ds);
        }

        // Chi tiết
        public IActionResult ChiTiet(int id)
        {
            var luuTru = _context.LuuTrus
                .Include(x => x.DongVat)
                .Include(x => x.TramCuuHo)
                .FirstOrDefault(x => x.MaLuuTru == id);

            if (luuTru == null)
                return NotFound();

            return View(luuTru);
        }

        // Form thêm
        [HttpGet]
        public IActionResult Them()
        {
            ViewBag.DongVat = _context.DongVats.ToList();
            ViewBag.Tram = _context.TramCuuHos.ToList();

            return View(new LuuTru { NgayBatDau = DateTime.Now });
        }

        // Lưu thêm
        [HttpPost]
        public IActionResult Them(LuuTru model)
        {
            // Gán trạng thái MẶC ĐỊNH TRƯỚC khi kiểm tra ModelState để tránh lỗi Required
            model.TrangThai = "Đang lưu trú";

            // Xóa lỗi validate của các Navigation Property không do form gửi lên trực tiếp
            ModelState.Remove(nameof(LuuTru.DongVat));
            ModelState.Remove(nameof(LuuTru.TramCuuHo));
            ModelState.Remove(nameof(LuuTru.TrangThai));

            if (!ModelState.IsValid)
            {
                ViewBag.DongVat = _context.DongVats.ToList();
                ViewBag.Tram = _context.TramCuuHos.ToList();
                return View(model);
            }

            _context.LuuTrus.Add(model);
            _context.SaveChanges();

            // Ghi nhật ký hoạt động
            NhatKyHelper.GhiLog(_context, HttpContext, "Thêm thông tin lưu trú");

            TempData["ThongBao"] = "Đã thêm thông tin lưu trú.";
            return RedirectToAction(nameof(Index));
        }

        // Form sửa
        [HttpGet]
        public IActionResult Sua(int id)
        {
            var model = _context.LuuTrus.Find(id);

            if (model == null)
                return NotFound();

            ViewBag.DongVat = _context.DongVats.ToList();
            ViewBag.Tram = _context.TramCuuHos.ToList();

            return View(model);
        }

        // Lưu sửa
        [HttpPost]
        public IActionResult Sua(LuuTru model)
        {
            ModelState.Remove(nameof(LuuTru.DongVat));
            ModelState.Remove(nameof(LuuTru.TramCuuHo));
            ModelState.Remove(nameof(LuuTru.TrangThai));

            if (!ModelState.IsValid)
            {
                ViewBag.DongVat = _context.DongVats.ToList();
                ViewBag.Tram = _context.TramCuuHos.ToList();
                return View(model);
            }

            var existing = _context.LuuTrus.Find(model.MaLuuTru);
            if (existing == null)
            {
                return NotFound();
            }

            // Cập nhật an toàn từng trường tránh mất dữ liệu
            existing.MaDongVat = model.MaDongVat;
            existing.MaTram = model.MaTram;
            existing.NgayBatDau = model.NgayBatDau;
            existing.NgayKetThuc = model.NgayKetThuc;
            existing.GhiChu = model.GhiChu;
            //existing.TrangThai = model.TrangThai;

            _context.SaveChanges();

            // Ghi nhật ký hoạt động
            NhatKyHelper.GhiLog(_context, HttpContext, "Cập nhật lưu trú");

            TempData["ThongBao"] = "Đã cập nhật.";
            return RedirectToAction(nameof(Index));
        }

        // Kết thúc lưu trú
        public IActionResult KetThuc(int id)
        {
            var model = _context.LuuTrus
                .Include(x => x.DongVat)
                .FirstOrDefault(x => x.MaLuuTru == id);

            if (model == null)
                return NotFound();

            model.NgayKetThuc = DateTime.Now;
            model.TrangThai = "Đã kết thúc";

            if (model.DongVat != null)
            {
                model.DongVat.TrangThai = "Sẵn sàng nhận nuôi";
            }

            _context.SaveChanges();

            // Ghi nhật ký hoạt động
            NhatKyHelper.GhiLog(_context, HttpContext, "Kết thúc lưu trú");

            TempData["ThongBao"] = "Đã kết thúc lưu trú.";
            return RedirectToAction(nameof(Index));
        }

        // Xóa đơn lẻ (Giữ nguyên)
        public IActionResult Xoa(int id)
        {
            var model = _context.LuuTrus.Find(id);

            if (model == null)
                return NotFound();

            _context.LuuTrus.Remove(model);
            _context.SaveChanges();

            // Ghi nhật ký hoạt động
            NhatKyHelper.GhiLog(_context, HttpContext, "Xóa thông tin lưu trú");

            TempData["ThongBao"] = "Đã xóa.";

            return RedirectToAction(nameof(Index));
        }

        // >>> MỚI THÊM: Xử lý Xóa Hàng Loạt (AJAX) <<<
        [HttpPost]
        public IActionResult XoaHangLoat(List<int> ids)
        {
            // SỬA LỖI TẠI ĐÂY: Dùng HttpContext thay vì Context
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này." });
            }

            if (ids == null || !ids.Any())
            {
                return Json(new { success = false, message = "Chưa chọn bản ghi nào để xóa." });
            }

            try
            {
                // Lấy toàn bộ lên RAM trước để tránh lỗi cú pháp SQL (WITH)
                var tatCaLuuTru = _context.LuuTrus.ToList();
                
                // Lọc ra các bản ghi cần xóa theo ID được chọn
                var dsCanXoa = tatCaLuuTru.Where(x => ids.Contains(x.MaLuuTru)).ToList();

                if (!dsCanXoa.Any())
                {
                    return Json(new { success = false, message = "Không tìm thấy bản ghi lưu trú nào hợp lệ." });
                }

                _context.LuuTrus.RemoveRange(dsCanXoa);
                _context.SaveChanges();

                // Ghi nhật ký hoạt động
                NhatKyHelper.GhiLog(_context, HttpContext, "Xóa hàng loạt thông tin lưu trú");

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi Server: " + ex.Message });
            }
        }
    }
}