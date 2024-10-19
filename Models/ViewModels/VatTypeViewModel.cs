using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels
{
    public class VatTypeViewModel
    {
        [Display(Name = "รหัสประเภทภาษี")]
        public int VatTypeId { get; set; }

        [Display(Name = "ชื่อประเภทภาษี")]
        public required string VatTypeName { get; set; }

        [Display(Name = "คำอธิบาย")]
        [DisplayFormat(NullDisplayText = "N/A")]
        public string? Description { get; set; }

        [Display(Name = "วันที่สร้าง")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ssน.}", ApplyFormatInEditMode = true)]
        public DateTimeOffset? CreatedDate { get; set; }

        [Display(Name = "วันที่แก้ไข")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ssน.}", ApplyFormatInEditMode = true, NullDisplayText = "N/A")]
        public DateTimeOffset? ModifiedDate { get; set; }
        
        public byte[]? RowVersion { get; set; }
    }

    public class VatTypeCreateViewModel
    {
        [Display(Name = "ชื่อประเภทภาษี")]
        [Required(ErrorMessage = "กรุณากรอกชื่อประเภทภาษี")]
        [StringLength(50, ErrorMessage = "ไม่เกิน 50 ตัวอักษร")]
        //[RegularExpression(@"^[\w\sก-๙]+$", ErrorMessage = "เฉพาะตัวอักษรภาษาอังกฤษ ตัวอักษรไทย และตัวเลขเท่านั้น")]
        public required string VatTypeName { get; set; }

        [Display(Name = "คำอธิบาย")]
        [StringLength(100, ErrorMessage = "ไม่เกิน 100 ตัวอักษร")]
        public string? Description { get; set; }
    }

    public class VatTypeEditViewModel
    {
        [Display(Name = "รหัสประเภทภาษี")]
        public int VatTypeId { get; set; }

        [Display(Name = "ชื่อประเภทภาษี")]
        [Required(ErrorMessage = "กรุณากรอกชื่อประเภทภาษี")]
        [StringLength(50, ErrorMessage = "ไม่เกิน 50 ตัวอักษร")]
        public required string VatTypeName { get; set; }

        [Display(Name = "คำอธิบาย")]
        [StringLength(100, ErrorMessage = "ไม่เกิน 100 ตัวอักษร")]
        public string? Description { get; set; }

        public byte[]? RowVersion { get; set; }
    }
}
