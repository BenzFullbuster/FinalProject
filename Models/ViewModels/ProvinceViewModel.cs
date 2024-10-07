using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels
{
    public class ProvinceViewModel
    {
        [Display(Name = "รหัสจังหวัด")]
        public int ProvinceId { get; set; }

        [Display(Name = "ชื่อจังหวัด")]
        public required string ProvinceName { get; set; }

        [Display(Name = "วันที่สร้าง")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ssน.}", ApplyFormatInEditMode = true)]
        public DateTimeOffset? CreatedDate { get; set; }

        [Display(Name = "วันที่แก้ไข")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ssน.}", ApplyFormatInEditMode = true, NullDisplayText = "N/A")]
        public DateTimeOffset? ModifiedDate { get; set; }

        public byte[]? RowVersion { get; set; }
    }

    public class ProvinceCreateViewModel
    {
        [Display(Name = "ชื่อจังหวัด")]
        [Required(ErrorMessage = "กรุณากรอกชื่อจังหวัด")]
        [StringLength(50, ErrorMessage = "ไม่เกิน 50 ตัวอักษร")]
        public required string ProvinceName { get; set; }
    }

    public class ProvinceEditViewModel
    {
        [Display(Name = "รหัสจังหวัด")]
        public int ProvinceId { get; set; }

        [Display(Name = "ชื่อจังหวัด")]
        [Required(ErrorMessage = "กรุณากรอกชื่อจังหวัด")]
        [StringLength(50, ErrorMessage = "ไม่เกิน 50 ตัวอักษร")]
        public required string ProvinceName { get; set; }

        public byte[]? RowVersion { get; set; }
    }
}
