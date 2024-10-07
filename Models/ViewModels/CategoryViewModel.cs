using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels
{
    public class CategoryViewModel
    {
        [Display(Name = "รหัสหมวดสินค้า")]
        public Guid CategoryId { get; set; }

        [Display(Name = "ชื่อหมวดสินค้า")]
        public required string CategoryName { get; set; }

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

    public class CategoryCreateViewModel
    {
        [Display(Name = "ชื่อหมวดสินค้า")]
        [Required(ErrorMessage = "กรุณากรอกชื่อหมวดสินค้า")]
        [StringLength(50, ErrorMessage = "ไม่เกิน 50 ตัวอักษร")]
        public required string CategoryName { get; set; }

        [Display(Name = "คำอธิบาย")]
        [StringLength(100, ErrorMessage = "ไม่เกิน 100 ตัวอักษร")]
        public string? Description { get; set; }
    }

    public class CategoryEditViewModel
    {
        [Display(Name = "รหัสหมวดสินค้า")]
        public Guid CategoryId { get; set; }

        [Display(Name = "ชื่อหมวดสินค้า")]
        [Required(ErrorMessage = "กรุณากรอกชื่อหมวดสินค้า")]
        [StringLength(50, ErrorMessage = "ไม่เกิน 50 ตัวอักษร")]
        public required string CategoryName { get; set; }

        [Display(Name = "คำอธิบาย")]
        [StringLength(100, ErrorMessage = "ไม่เกิน 100 ตัวอักษร")]
        public string? Description { get; set; }

        public byte[]? RowVersion { get; set; }
    }
}
