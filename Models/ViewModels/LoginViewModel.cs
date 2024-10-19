using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models.ViewModels
{
    public class LoginViewModel
    {
        [Display(Name = "ชื่อย่อคลังสินค้า")]
        [StringLength(5, ErrorMessage = "ห้ามเกิน 5 ตัวอักษร")]
        [Required(ErrorMessage = "กรุณากรอกชื่อย่อคลังสินค้า")]
        public string WarehouseCode { get; set; } = null!;

        [Display(Name = "รหัสผู้ใช้")]
        [StringLength(50, ErrorMessage = "ห้ามเกิน 50 ตัวอักษร")]
        [Required(ErrorMessage = "กรุณากรอกรหัสผู้ใช้")]
        public string Username { get; set; } = null!;

        [Display(Name = "รหัสผ่าน")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
        public string Password { get; set; } = null!;

        [NotMapped]
        [Display(Name = "จดจำฉัน")]
        public bool RememberMe { get; set; }
    }

    //public class ForgotPasswordViewModel
    //{
    //    [Display(Name = "อีเมล")]
    //    [Required(ErrorMessage = "กรุณากรอกอีเมล")]
    //    [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
    //    public required string Email { get; set; }
    //}

    //public class ResetPasswordViewModel
    //{
    //    [Display(Name = "รหัสผ่าน")]
    //    [DataType(DataType.Password)]
    //    [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
    //    //[RegularExpression(@"^(?=.*[A-Z])(?=.*\d)[\w\d^\s]{8,}$", ErrorMessage = "รหัสผ่านต้องขึ้นต้นด้วยตัวอักษรตัวใหญ่ และมีตัวเลขอย่างน้อย 1 ตัว ทั้งนี้ต้องมีความยาวอย่างน้อย 8 ตัวอักษร")]
    //    public string? Password { get; set; }

    //    [NotMapped]
    //    [Display(Name = "ยืนยันรหัสผ่าน")]
    //    [Required(ErrorMessage = "กรุณายืนยันรหัสผ่าน")]
    //    [Compare("Password", ErrorMessage = "กรอกรหัสผ่านไม่ตรงกัน")]
    //    public string? ConfirmPassword { get; set; }

    //    public string? Token { get; set; }
    //    public string? Email { get; set; }
    //}
}
