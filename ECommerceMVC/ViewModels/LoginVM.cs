using System.ComponentModel.DataAnnotations;

namespace ECommerceMVC.ViewModels
{
    public class LoginVM
    {
        [Key]
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = ("*"))]
        [MaxLength(20, ErrorMessage = ("Tên đăng nhập không được vượt quá 20 kí tự"))]
        public string MaKh { get; set; } = null!;

        [Required(ErrorMessage = ("*"))]
        [Display(Name = "Mật khẩu")]
        [MaxLength(50, ErrorMessage = ("Mật khẩu không được vượt quá 50 kí tự"))]
        [DataType(DataType.Password)]
        public string? MatKhau { get; set; }
    }
}
