using System;
using System.Linq;
using HeThongCuuHo.Helpers;
using HeThongCuuHo.Models;
using Microsoft.AspNetCore.Mvc;

namespace HeThongCuuHo.Controllers
{
    public class ThongKeController : Controller
    {
        private readonly HethongcuuhoDbContext _context;

        public ThongKeController(HethongcuuhoDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? nam)
        {
            if (!AuthorizationHelper.KiemTraDangNhap(HttpContext))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            int namChon = nam ?? DateTime.Now.Year;
            ViewBag.NamChon = namChon;

            var model = new ThongKeViewModel
            {
                // 1. Thống kê chỉ số tổng quan (KPIs)
                TongSoBaoCao = _context.BaoCaos.Count(),
                CaCuuHoHoanThanh = _context.CuuHos.Count(x => x.TrangThai == "Hoàn thành"),
                TongSoDongVat = _context.DongVats.Count(),
                DongVatDaDuocNhanNuoi = _context.DongVats.Count(x => x.TrangThai == "Đã nhận nuôi"),

                // 2. Thống kê ca cứu hộ theo 12 tháng (Đã fix lỗi Nullable DateTime)
                CaCuuHoTheoThang = Enumerable.Range(1, 12).Select(m => new ThongKeTheoThang
                {
                    Thang = $"T{m}",
                    SoLuong = _context.CuuHos.Count(x => x.ThoiGianNhan.HasValue
                                                      && x.ThoiGianNhan.Value.Year == namChon
                                                      && x.ThoiGianNhan.Value.Month == m)
                }).ToList(),

                // 3. Thống kê cơ cấu Động vật theo Loài
                DongVatTheoLoai = _context.DongVats
                    .GroupBy(x => x.Loai)
                    .Select(g => new ThongKeTheoLoai
                    {
                        TenLoai = string.IsNullOrEmpty(g.Key) ? "Khác" : g.Key,
                        SoLuong = g.Count()
                    }).ToList(),

                // 4. Thống kê Động vật theo Trạng thái
                TrangThaiDongVat = _context.DongVats
                    .GroupBy(x => x.TrangThai)
                    .Select(g => new ThongKeTheoTrangThai
                    {
                        TrangThai = string.IsNullOrEmpty(g.Key) ? "Chưa rõ" : g.Key,
                        SoLuong = g.Count()
                    }).ToList()
            };

            return View(model);
        }
    }
}