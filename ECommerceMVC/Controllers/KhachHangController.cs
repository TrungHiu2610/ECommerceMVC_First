using AutoMapper;
using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceMVC.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly Hshop2023Context db;
        private readonly IMapper _mapper;

        public KhachHangController(Hshop2023Context context, IMapper mapper)
        {
            db = context;
            _mapper = mapper;
        }

        public IActionResult Index()
        {

            return View();
        }

        public IActionResult DangKy(RegisterVM model, IFormFile fileImg)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    KhachHang khachHang = _mapper.Map<KhachHang>(model);
                    khachHang.RandomKey = MyTool.GenerateRandomKey();
                    khachHang.MatKhau = khachHang.MatKhau.ToMd5Hash(khachHang.RandomKey);
                    khachHang.HieuLuc = true; //xử lý sau khi dùng mail để active
                    khachHang.VaiTro = 0;

                    if (fileImg != null)
                    {
                        khachHang.Hinh = MyTool.UploadImage(fileImg, "KhachHang");
                    }
                    db.Add(khachHang);
                    db.SaveChanges();
                    return RedirectToAction("Index", "HangHoa");
                }
                return View();
            }
            catch(Exception ex)
            {
                TempData["Message"] = ex.Message;
                return Redirect("/404");
            }
        }
    }
}
