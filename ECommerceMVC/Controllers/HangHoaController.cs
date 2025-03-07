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

            if(maLoai.HasValue)
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
    }
}
