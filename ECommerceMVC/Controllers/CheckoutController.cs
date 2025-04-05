using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.Services;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceMVC.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly Hshop2023Context db;
        private readonly PaypalClient _paypalClient;
        private readonly IVNPayServices _vnpayService;

        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();
        public CheckoutController(Hshop2023Context context, PaypalClient paypalClient, IVNPayServices vnpayService)
        {
            db = context;
            _paypalClient = paypalClient;
            _vnpayService = vnpayService;
        }

        #region Checkout
        [Authorize]
        [HttpGet]
        public IActionResult CheckOut()
        {
            var gioHang = Cart;
            TempData["Null"] = 1;
            // nếu ko có sản phẩm nào trong giỏ hàng thì quay về trang chủ
            if (gioHang.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.PaypalClientId = _paypalClient.ClientId;
            return View(gioHang);
        }
        [Authorize]
        [HttpPost]
        public IActionResult CheckOut(CheckOutVM model, string paymentMethod = MySetting.PAYMENTTYPE_COD)
        {
            var gioHang = Cart;
            if (ModelState.IsValid)
            {
                if (paymentMethod == "THANH TOÁN VNPAY")
                {
                    var vnPayModel = new VNPaymentRequestModel()
                    {
                        Amount = Cart.Sum(p => p.ThanhTien)*100,
                        Description = $"{model.HoTen} - {model.DienThoai}",
                        FullName = model.HoTen,
                        CreatedDate = DateTime.Now,
                        OrderId = new Random().Next(10000, 100000).ToString()
                    };

                    return Redirect(_vnpayService.CreateRequestUrl(HttpContext, vnPayModel));
                }
                // lấy ra thông tin khách hàng đang đăng nhập bằng Claim
                string maKH = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;
                DateTime ngayDat = DateTime.Now;
                string? HoTen;
                string? DiaChi;
                string cachThanhToan = paymentMethod;
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
                    foreach (CartItem item in gioHang)
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
                }
                catch
                {
                    db.Database.RollbackTransaction();
                }
            }
            return RedirectToAction("PaymentSuccess");
        }
        #endregion

        #region Paypal payment
        [Authorize]
        [HttpPost("/Checkout/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            // khai bao cac variable se truyen vao api gom: value, currency, reference
            var value = Cart.Sum(p => p.ThanhTien).ToString();
            var currency = "USD";
            var reference = "DH" + DateTime.Now.Ticks.ToString();

            try
            {
                var response = await _paypalClient.CreateOrder(value, currency, reference);

                return Ok(response);
            }
            catch (Exception ex)
            {
                var err = new { ex.GetBaseException().Message };
                return BadRequest(err);
            }

        }
        [Authorize]
        [HttpPost("/Checkout/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string orderId, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderId);

                // Luu don hang vao db

                return Ok(response);
            }
            catch (Exception ex)
            {
                var err = new { ex.GetBaseException().Message };
                return BadRequest(err);
            }

        }

        #endregion

        #region VNPay payment

        [Authorize]
        public IActionResult VNPayPaymentCallBack()
        {
            var response = _vnpayService.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                string err = $"Lỗi thanh toán VNPay: {response.VnPayResponseCode}";
                return RedirectToAction("PaymentFail", new { err = err });
            }

            // lưu đơn hàng vào db

            return RedirectToAction("PaymentSuccess", new { paymentMethod = "VNPay" });
        }
        #endregion

        [Authorize]
        public IActionResult PaymentSuccess(string paymentMethod = "COD")
        {
            //Sau khi thanh toán thì clear các sản phẩm thanh toán trong giỏ hàng
            HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());
            TempData["Message"] = $"Thanh toán {paymentMethod} thành công";
            return View("PaymentStatus");
        }
        [Authorize]
        public IActionResult PaymentFail(string? err = null)
        {
            //Nếu thanh toán thất bại thì ko clear các sản phẩm thanh toán trong giỏ hàng
            TempData["Message"] = err;
            return View("PaymentStatus");
        }
    }
}
