using AutoMapper;
using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

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

        [HttpGet]

        #region Sign in
        public IActionResult DangKy()
        {

            return View();
        }

        [HttpPost]
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
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return Redirect("/404");
            }
        }
        #endregion

        #region Login
        [HttpGet]
        public IActionResult DangNhap(string? ReturnURL)
        {
            ViewBag.ReturnURL = ReturnURL;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DangNhap(LoginVM model, string? ReturnURL)
        {
            try
            {
                ViewBag.ReturnURL = ReturnURL;
                if (ModelState.IsValid)
                {
                    KhachHang? kh = db.KhachHangs.SingleOrDefault(x => x.MaKh == model.MaKh);
                    // mã kh ko tồn tại
                    if (kh == null)
                    {
                        ModelState.AddModelError("Error", "Sai thông tin đăng nhập");
                    }
                    else
                    {
                        // tài khoản kh bị khóa/hiệu lực = false
                        if (!kh.HieuLuc)
                        {
                            ModelState.AddModelError("Error", "Tài khoản đã bị khóa. Vui lòng liên hệ Admin để hỗ trợ");
                        }
                        else
                        {
                            // kiểm tra mật khẩu kh nhập bằng cách Encrypt nó và kiểm tra với database
                            if (model.MatKhau.ToMd5Hash(kh.RandomKey) != kh.MatKhau)
                            {
                                ModelState.AddModelError("Error", "Sai thông tin đăng nhập");
                            }
                            else
                            {
                                // khai báo claim 
                                List<Claim> lstClaim = new List<Claim>()
                                {
                                    new Claim(ClaimTypes.Email, kh.Email),
                                    new Claim(ClaimTypes.Name, kh.HoTen),
                                    new Claim(MySetting.CLAIM_CUSTOMERID, kh.MaKh),

                                    // claim động gì đó
                                    new Claim(ClaimTypes.Role, "Customer"),
                                };

                                // khai báo identity
                                ClaimsIdentity identity = new ClaimsIdentity(lstClaim, CookieAuthenticationDefaults.AuthenticationScheme);

                                // khai báo claim principal
                                ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                                await HttpContext.SignInAsync(principal);

                                if (Url.IsLocalUrl(ReturnURL))
                                {
                                    return Redirect(ReturnURL);
                                }
                                return Redirect("/");
                            }
                        }
                    }
                }
                return View();
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return Redirect("/404");
            }
        }
        #endregion

        [Authorize]
        public IActionResult Profile()
        {
            ProfileVM kh = new ProfileVM()
            {
                MaKh = HttpContext.User.FindFirstValue(MySetting.CLAIM_CUSTOMERID),
                HoTen = HttpContext.User.Identity.Name,
                Email = HttpContext.User.FindFirstValue(ClaimTypes.Email)
            };
            return View(kh);
        }

        [Authorize]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }    
    }
}
