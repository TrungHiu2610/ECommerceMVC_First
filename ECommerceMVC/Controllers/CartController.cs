using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly Hshop2023Context db;
        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();
        public CartController(Hshop2023Context context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            return View(Cart);
        }

        public IActionResult AddToCart(int maHH, int soLuong = 1, bool stayInProductsPage = true)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHH == maHH);
            // neu sp da co trong gio hang => cap nhat so luong
            if(item!=null)
            {
                var hangHoa = db.HangHoas.SingleOrDefault(p => p.MaHh == item.MaHH);
                if(hangHoa==null)
                {
                    TempData["Message"] = $"The item's id {item.MaHH} is not available";
                    return Redirect("/404");
                }
                item.SoLuong += soLuong;
            }
            // chua co thi add new
            else
            {
                var hangHoa = db.HangHoas.SingleOrDefault(p => p.MaHh == maHH);
                if (hangHoa != null)
                {
                    CartItem ct_to_add = new CartItem()
                    {
                        MaHH = hangHoa.MaHh,
                        TenHH = hangHoa.TenHh,
                        Hinh = hangHoa.Hinh ?? "avatar.jpg",
                        SoLuong = soLuong,
                        DonGia = hangHoa.DonGia ?? 0
                    };
                    gioHang.Add(ct_to_add);
                }
            }
            // cap nhat lai session gio hang va so luong
            HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            HttpContext.Session.SetInt32("CartItemAmount", gioHang.Count);
            if (stayInProductsPage)
            {
                return RedirectToAction("Index", "HangHoa");
            }
            return RedirectToAction("Index", "Cart");
        }

        public IActionResult Remove(int maHH)
        {
            // kiem tra ma sp co trong gio hang ko
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHH == maHH);
            // neu co thi xoa ra khoi gio hang va cap nhat
            if(item!=null)
            {
                gioHang.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            }
            // ko co thi bao loi
            else
            {
                TempData["Message"] = $"The item's id {maHH} is not available";
                return Redirect("/404");
            }
            HttpContext.Session.SetInt32("CartItemAmount", gioHang.Count);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult CheckOut()
        {
            var gioHang = Cart;
            // nếu ko có sản phẩm nào trong giỏ hàng thì quay về trang chủ
            if(gioHang.Count==0)
            {
                return RedirectToAction("Index","Home");
            }    
            return View(gioHang);
        }
        [HttpPost]
        public IActionResult CheckOut(CheckOutVM model)
        {
            var gioHang = Cart;
            if (ModelState.IsValid)
            {
                // lấy ra thông tin khách hàng đang đăng nhập bằng Claim
                string maKH = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;
                DateTime ngayDat = DateTime.Now;
                string? HoTen;
                string? DiaChi;
                string cachThanhToan = "COD";
                string cachVanChuyen = "TH_EXPRESS";
                int maTrangThai = 0; // xử lý sau
                string ghiChu;
                string? dienThoai;
                
                KhachHang kh = new KhachHang();
                // nếu ng dùng tích vào checkbox "Lấy thông tin giao hàng từ thông tin tài khoản?"
                if (model.SameAccountInfo)
                {
                    kh = db.KhachHangs.SingleOrDefault(p => p.MaKh == maKH);
                    HoTen = kh.HoTen;
                    DiaChi = kh.DiaChi;
                    dienThoai = kh.DienThoai;
                }
                else
                {
                    HoTen = model.HoTen;
                    DiaChi = model.DiaChi;
                    dienThoai = model.DienThoai;
                }
                ghiChu = model.GhiChu;


                HoaDon hd = new HoaDon()
                {
                    MaKh = maKH,
                    NgayDat = ngayDat,
                    HoTen = HoTen,
                    DiaChi = DiaChi,
                    DienThoai = dienThoai,
                    CachThanhToan = cachThanhToan,
                    CachVanChuyen = cachVanChuyen,
                    MaTrangThai = maTrangThai,
                    MaNv = null,
                    GhiChu = ghiChu,
                };
                
                // ***** Cách lấy ra mã hóa đơn vừa mới thêm vào database mà ko cần duyệt lại bảng HoaDon
                db.Database.BeginTransaction();
                try
                {
                    db.Database.CommitTransaction();
                    db.HoaDons.Add(hd);
                    db.SaveChanges();
                    List<ChiTietHd> lstCTHD = new List<ChiTietHd>();
                    foreach(CartItem item in gioHang)
                    {
                        ChiTietHd cthd = new ChiTietHd()
                        {
                            MaHd = hd.MaHd,
                            MaHh = item.MaHH,
                            DonGia = item.DonGia,
                            SoLuong = item.SoLuong,
                            GiamGia = 0
                        };
                        lstCTHD.Add(cthd);
                    }
                    db.ChiTietHds.AddRange(lstCTHD);
                    db.SaveChanges();

                    //Sau khi thanh toán thì clear các sản phẩm thanh toán trong giỏ hàng
                    HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());
                }
                catch
                {
                    db.Database.RollbackTransaction();
                }
            }    
            return View("Success");
        }
        
    }
}
