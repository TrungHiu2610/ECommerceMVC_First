using System.ComponentModel.DataAnnotations;

namespace ECommerceMVC.ViewModels
{
    public class RegisterVM
    {
        [Key]
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage =("*"))]
        [MaxLength(20, ErrorMessage = ("Tên đăng nhập không được vượt quá 20 kí tự"))]
        public string MaKh { get; set; } = null!;

        [Required(ErrorMessage =("*"))]
        [Display(Name = "Mật khẩu")]
        [MaxLength(50,ErrorMessage =("Mật khẩu không được vượt quá 50 kí tự"))]
        [DataType(DataType.Password)]
        public string? MatKhau { get; set; }

        [Required(ErrorMessage =("*"))]
        [Display(Name = "Họ tên")]
        [MaxLength(50, ErrorMessage = ("Họ tên không được vượt quá 50 kí tự"))]
        public string HoTen { get; set; } = null!;


        [Required(ErrorMessage = ("*"))]
        [Display(Name = "Giới tính")]
        public bool GioiTinh { get; set; }

        [Required(ErrorMessage = ("*"))]
        [Display(Name = "Ngày sinh")]
        public DateTime NgaySinh { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Display(Name = "Điện thoại")]
        [RegularExpression(@"0[9873]\d{8}",ErrorMessage ="Số điện thoại không hợp lệ")]
        public string? DienThoai { get; set; }

        [Required(ErrorMessage = ("*"))]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage ="Email chưa đúng định dạng")]
        public string Email { get; set; } = null!;

        public string? Hinh { get; set; }
    }
}
