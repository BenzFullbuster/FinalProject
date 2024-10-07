using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels
{
    public class UnitViewModel
    {
        [Display(Name = "รหัสหน่วยนับ")]
        public Guid UnitId { get; set; }

        [Display(Name = "ชื่อหน่วยนับ")]
        public required string UnitName { get; set; }

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

    public class UnitCreateViewModel
    {
        [Display(Name = "ชื่อหน่วยนับ")]
        [Required(ErrorMessage = "กรุณากรอกชื่อหน่วยนับ")]
        [StringLength(50, ErrorMessage = "ไม่เกิน 50 ตัวอักษร")]
        //[RegularExpression(@"^[\w\sก-๙]+$", ErrorMessage = "เฉพาะตัวอักษรภาษาอังกฤษ ตัวอักษรไทย และตัวเลขเท่านั้น")]
        public required string UnitName { get; set; }

        [Display(Name = "คำอธิบาย")]
        [StringLength(100, ErrorMessage = "ไม่เกิน 100 ตัวอักษร")]
        public string? Description { get; set; }
    }

    public class UnitEditViewModel
    {
        [Display(Name = "รหัสหน่วยนับ")]
        public Guid UnitId { get; set; }

        [Display(Name = "ชื่อหน่วยนับ")]
        [Required(ErrorMessage = "กรุณากรอกชื่อหน่วยนับ")]
        [StringLength(50, ErrorMessage = "ไม่เกิน 50 ตัวอักษร")]
        public required string UnitName { get; set; }

        [Display(Name = "คำอธิบาย")]
        [StringLength(100, ErrorMessage = "ไม่เกิน 100 ตัวอักษร")]
        public string? Description { get; set; }

        public byte[]? RowVersion { get; set; }
    }
}
