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

        public IActionResult AddToCart(int maHH, int soLuong = 1)
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
            return RedirectToAction("Index");
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
    }
}
