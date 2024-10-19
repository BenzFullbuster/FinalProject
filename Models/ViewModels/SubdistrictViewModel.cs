using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels
{
    public class SubdistrictViewModel
    {
        [Display(Name = "รหัสแขวง/ตำบล")]
        public int SubdistrictId { get; set; }

        [Display(Name = "ชื่อแขวง/ตำบล")]
        public required string SubdistrictName { get; set; }

        [Display(Name = "ชื่อเขต/อำเภอ")]
        public required string DistrictName { get; set; }

        [Display(Name = "รหัสไปรษณีย์")]
        public required string Zipcode { get; set; }

        [Display(Name = "วันที่สร้าง")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ssน.}", ApplyFormatInEditMode = true)]
        public DateTimeOffset? CreatedDate { get; set; }

        [Display(Name = "วันที่แก้ไข")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ssน.}", ApplyFormatInEditMode = true, NullDisplayText = "N/A")]
        public DateTimeOffset? ModifiedDate { get; set; }

        public byte[]? RowVersion { get; set; }
    }

    public class SubdistrictCreateViewModel
    {
        [Display(Name = "ชื่อแขวง/ตำบล")]
        public required string SubdistrictName { get; set; }

        [Display(Name = "ชื่อเขต/อำเภอ")]
        [Required(ErrorMessage = "กรุณาเลือกชื่อเขต/อำเภอ")]
        public int DistrictId { get; set; }

        [Display(Name = "รหัสไปรษณีย์")]
        [Required(ErrorMessage = "กรุณากรอกรหัสไปรษณีย์")]
        [StringLength(5, ErrorMessage = "ไม่เกิน 5 ตัวอักษร")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "เฉพาะตัวเลขเท่านั้น")]
        public required string Zipcode { get; set; }
    }

    public class SubdistrictEditViewModel
    {
        [Display(Name = "รหัสแขวง/ตำบล")]
        public int SubdistrictId { get; set; }

        [Display(Name = "ชื่อแขวง/ตำบล")]
        public required string SubdistrictName { get; set; }

        [Display(Name = "ชื่อเขต/อำเภอ")]
        [Required(ErrorMessage = "กรุณาเลือกชื่อเขต/อำเภอ")]
        public int DistrictId { get; set; }
        public string? DistrictName { get; set; }

        [Display(Name = "รหัสไปรษณีย์")]
        [Required(ErrorMessage = "กรุณากรอกรหัสไปรษณีย์")]
        [StringLength(5, ErrorMessage = "ไม่เกิน 5 ตัวอักษร")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "เฉพาะตัวเลขเท่านั้น")]
        public required string Zipcode { get; set; }

        public byte[]? RowVersion { get; set; }
    }
}
