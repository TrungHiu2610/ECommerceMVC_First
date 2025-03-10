using ECommerceMVC.Data;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceMVC.Controllers
{
    public class HangHoaController : Controller
    {
        private readonly Hshop2023Context db;

        public HangHoaController(Hshop2023Context context) => db = context;
        public IActionResult Index(int? maLoai)
        {
            var hangHoas = db.HangHoas.AsQueryable();

            if (maLoai.HasValue)
            {
                hangHoas = hangHoas.Where(h => h.MaLoai == maLoai);
            }

            var data = hangHoas.Select(d => new HangHoaVM
            {
                MaHH = d.MaHh,
                TenHH = d.TenHh,
                HinhAnh = d.Hinh ?? "",
                DonGia = d.DonGia ?? 0,
                MoTaNgan = d.MoTaDonVi ?? "",
                TenLoai = d.MaLoaiNavigation.TenLoai
            });
            return View(data);
        }

        public IActionResult Search(string? query)
        {
            var hangHoas = db.HangHoas.AsQueryable();

            if (query != null)
            {
                hangHoas = hangHoas.Where(h => h.TenHh.Contains(query));
            }

            var data = hangHoas.Select(d => new HangHoaVM
            {
                MaHH = d.MaHh,
                TenHH = d.TenHh,
                HinhAnh = d.Hinh ?? "",
                DonGia = d.DonGia ?? 0,
                MoTaNgan = d.MoTaDonVi ?? "",
                TenLoai = d.MaLoaiNavigation.TenLoai
            });

            return View(data);
        }

        public IActionResult Detail(int maHH)
        {
            var hangHoas = db.HangHoas.AsQueryable();

            HangHoa? product = hangHoas.FirstOrDefault(p => p.MaHh == maHH);

            // hàng hóa cùng loại
            List<HangHoaVM> lstRelatedProducts = new List<HangHoaVM>();
            if (product != null)
            {
                lstRelatedProducts = hangHoas.Where(d => d.MaHh != maHH && d.MaLoai == product.MaLoai)
                                             .Select(d => new HangHoaVM
                                             {
                                                 MaHH = d.MaHh,
                                                 TenHH = d.TenHh,
                                                 HinhAnh = d.Hinh ?? "",
                                                 DonGia = d.DonGia ?? 0,
                                                 MoTaNgan = d.MoTaDonVi ?? "",
                                                 TenLoai = d.MaLoaiNavigation.TenLoai
                                             }).Take(4).ToList();

                HangHoaVM product_to_find = new HangHoaVM()
                {
                    MaHH = product.MaHh,
                    TenHH = product.TenHh,
                    HinhAnh = product.Hinh ?? "avatar.jpg",
                    TenLoai = (product.MaLoaiNavigation != null) ? product.MaLoaiNavigation.TenLoai : "Default",
                    MoTaNgan = product.MoTa ?? "No description",
                    DonGia = product.DonGia ?? 0
                };
                ViewBag.lstRelatedProducts = lstRelatedProducts;
                return View(product_to_find);
            }
            TempData["Message"] = $"The product's id {maHH} is not avaiable";
            return Redirect("/404");
        }
    }
}
