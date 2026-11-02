using Microsoft.AspNetCore.Mvc;
using HeThongCuuHo.Models;

namespace HeThongCuuHo.Controllers
{
    public class TinTucController : Controller
    {
        // Danh sách tin tức mẫu trực tiếp trong bộ nhớ (Không cần kết nối SQL)
        private static readonly List<TinTuc> DanhSachTinTuc = new List<TinTuc>
        {
            new TinTuc
            {
                MaTinTuc = 1,
                TieuDe = "Chiến dịch cứu hộ động vật bị bỏ rơi",
                TomTat = "Đội cứu hộ đã tiếp nhận thêm 5 chú chó nghiệp vụ bị thương và hỗ trợ y tế kịp thời.",
                NoiDung = "Trong tuần qua, trạm cứu hộ đã hoàn thành đợt tiếp nhận và chữa trị cho các chú chó bị thương. Các bé đã được chăm sóc y tế, phục hồi thể trạng và sẵn sàng chờ chủ mới.",
                HinhAnh = "https://images.unsplash.com/photo-1548767797-d8c844163c4c?w=600&auto=format&fit=crop",
                NgayDang = new DateTime(2026, 8, 15)
            },
            new TinTuc
            {
                MaTinTuc = 2,
                TieuDe = "Hướng dẫn chăm sóc chó mèo mùa nắng nóng",
                TomTat = "Bổ sung đủ nước, tránh cho thú cưng ra ngoài vào giờ cao điểm nắng gắt.",
                NoiDung = "Thời tiết nắng nóng dễ làm thú cưng bị sốc nhiệt. Bạn nên chuẩn bị đủ nước uống, đặt chỗ nằm thoáng mát và hạn chế dắt đi dạo vào khung giờ 10h - 16h.",
                HinhAnh = "https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=600&auto=format&fit=crop",
                NgayDang = new DateTime(2026, 8, 14)
            },
            new TinTuc
            {
                MaTinTuc = 3,
                TieuDe = "Chương trình tiêm phòng miễn phí tháng 8",
                TomTat = "Trạm cứu hộ phối hợp cùng các bác sĩ thú y tổ chức tiêm phòng dại miễn phí cho chó mèo.",
                NoiDung = "Chương trình tiêm phòng dại và khám sức khỏe tổng quát miễn phí sẽ diễn ra tại trung tâm cứu hộ vào cuối tuần này. Kính mời quý chủ nuôi đưa thú cưng tới tham gia.",
                HinhAnh = "https://images.unsplash.com/photo-1576201836106-db1758fd1c97?w=600&auto=format&fit=crop",
                NgayDang = new DateTime(2026, 8, 12)
            },
            new TinTuc
            {
                MaTinTuc = 4,
                TieuDe = "Tìm mái ấm mới cho các bé mèo mồ côi",
                TomTat = "Hiện trạm đang chăm sóc 10 bé mèo con đã được tiêm phòng và tẩy giun đầy đủ.",
                NoiDung = "Các bé mèo con bị bỏ rơi đã hoàn tất đợt cách ly y tế và tiêm phòng. Nếu bạn muốn nhận nuôi và trao cho các bé một mái ấm tình thương, hãy liên hệ với trạm ngay nhé.",
                HinhAnh = "https://images.unsplash.com/photo-1533738363-b7f9aef128ce?w=600&auto=format&fit=crop",
                NgayDang = new DateTime(2026, 8, 10)
            },
            new TinTuc
            {
                MaTinTuc = 5,
                TieuDe = "Tổng kết hoạt động cứu hộ quý II",
                TomTat = "Hơn 120 ca cứu hộ thành công và 85 bé thú cưng đã tìm được chủ nhân mới.",
                NoiDung = "Nhờ sự chung tay của cộng đồng và các tình nguyện viên, trong quý II trạm đã tiếp nhận và xử lý thành công 120 ca cứu hộ động vật gặp tai nạn hoặc bị bỏ rơi.",
                HinhAnh = "https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?w=600&auto=format&fit=crop",
                NgayDang = new DateTime(2026, 8, 5)
            },
            new TinTuc
            {
                MaTinTuc = 6,
                TieuDe = "Cẩm nang dinh dưỡng cho chó mèo sau phẫu thuật",
                TomTat = "Những lưu ý quan trọng về chế độ ăn uống giúp thú cưng nhanh hồi phục.",
                NoiDung = "Chế độ dinh dưỡng sau phẫu thuật đóng vai trò quyết định đến tốc độ lành vết thương. Hãy bổ sung các thực phẩm giàu đạm, dễ tiêu hóa và giữ môi trường sống sạch sẽ.",
                HinhAnh = "https://images.unsplash.com/photo-1583511655826-05700d52f4d9?w=600&auto=format&fit=crop",
                NgayDang = new DateTime(2026, 8, 1)
            }
        };

        // Trang danh sách Tin Tức
        public IActionResult Index(string timKiem)
        {
            var items = DanhSachTinTuc.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(timKiem))
            {
                items = items.Where(t => t.TieuDe.Contains(timKiem, StringComparison.OrdinalIgnoreCase)
                                      || (t.TomTat != null && t.TomTat.Contains(timKiem, StringComparison.OrdinalIgnoreCase)));
            }

            return View(items);
        }

        // Trang chi tiết bài viết
        public IActionResult ChiTiet(int id)
        {
            var item = DanhSachTinTuc.FirstOrDefault(t => t.MaTinTuc == id);
            if (item == null) return NotFound();
            return View(item);
        }
    }
}