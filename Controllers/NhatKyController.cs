using HeThongCuuHo.Helpers;
using HeThongCuuHo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongCuuHo.Controllers
{
    public class NhatKyController : Controller
    {
        private readonly HethongcuuhoDbContext _context;

        public NhatKyController(HethongcuuhoDbContext context)
        {
            _context = context;
        }

        // GET: /NhatKy
        public async Task<IActionResult> Index(
            string? timKiem,
            string? trangThai,
            string? tenEntity,
            DateTime? tuNgay,
            DateTime? denNgay,
            int page = 1)
        {
            // Kiểm tra quyền Admin
            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            var query = _context.NhatKyHoatDongs
                .Include(x => x.NguoiDung)
                .AsQueryable();

            // 1. Tìm kiếm theo từ khóa (Tên người dùng, Hành động, Chi tiết, IP)
            if (!string.IsNullOrWhiteSpace(timKiem))
            {
                timKiem = timKiem.Trim().ToLower();
                query = query.Where(x =>
                    (x.TenNguoiDung != null && x.TenNguoiDung.ToLower().Contains(timKiem)) ||
                    x.HanhDong.ToLower().Contains(timKiem) ||
                    (x.ChiTiet != null && x.ChiTiet.ToLower().Contains(timKiem)) ||
                    (x.DiaChiIP != null && x.DiaChiIP.Contains(timKiem))
                );
            }

            // 2. Lọc theo trạng thái (Thành công / Thất bại)
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(x => x.TrangThai == trangThai);
            }

            // 3. Lọc theo thực thể bị tác động (BaoCao, CuuHo, NguoiDung...)
            if (!string.IsNullOrWhiteSpace(tenEntity))
            {
                query = query.Where(x => x.TenEntity == tenEntity);
            }

            // 4. Lọc theo khoảng thời gian
            if (tuNgay.HasValue)
            {
                query = query.Where(x => x.ThoiGian >= tuNgay.Value.Date);
            }

            if (denNgay.HasValue)
            {
                // Lấy hết đến 23:59:59 của ngày kết thúc
                DateTime endOfDay = denNgay.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.ThoiGian <= endOfDay);
            }

            // Sắp xếp nhật ký mới nhất lên đầu
            query = query.OrderByDescending(x => x.ThoiGian);

            // Phân trang đơn giản (20 bản ghi/trang)
            int pageSize = 20;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var dsLog = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Lưu dữ liệu tìm kiếm ra ViewBag để hiển thị trên Form Lọc
            ViewBag.TimKiem = timKiem;
            ViewBag.TrangThai = trangThai;
            ViewBag.TenEntity = tenEntity;
            ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = denNgay?.ToString("yyyy-MM-dd");
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(dsLog);
        }

        // POST: /NhatKy/XoaLogCu (Xóa nhật ký cũ hơn 30 ngày để giảm tải CSDL)
        [HttpPost]
        public async Task<IActionResult> XoaLogCu(int soNgay = 30)
        {
            if (!AuthorizationHelper.KiemTraVaiTro(HttpContext, "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            DateTime ngayHan = DateTime.Now.AddDays(-soNgay);
            var logsCu = await _context.NhatKyHoatDongs
                .Where(x => x.ThoiGian < ngayHan)
                .ToListAsync();

            if (logsCu.Any())
            {
                _context.NhatKyHoatDongs.RemoveRange(logsCu);
                await _context.SaveChangesAsync();

                // Ghi vết hành động dọn dẹp nhật ký
                NhatKyHelper.GhiLog(_context, HttpContext, "Dọn dẹp nhật ký", $"Đã xóa {logsCu.Count} bản ghi cũ hơn {soNgay} ngày");

                TempData["ThongBao"] = $"Đã dọn dẹp thành công {logsCu.Count} bản ghi nhật ký cũ.";
            }
            else
            {
                TempData["ThongBao"] = "Không có bản ghi nhật ký nào cũ hơn số ngày quy định.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}