using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels
{
    public class ProductViewModel
    {
        [Display(Name = "รหัสสินค้า")]
        public Guid ProductId { get; set; }

        [Display(Name = "ชื่อสินค้า")]
        public required string ProductName { get; set; }

        [Display(Name = "หมวดสินค้า")]
        public required string CategoryName { get; set; }

        [Display(Name = "หน่วยนับ")]
        public required string UnitName { get; set; }

        [Display(Name = "ราคาขาย")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Sellprice { get; set; }

        [Display(Name = "คงเหลือ")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Balance { get; set; }

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

    public class ProductCreateViewModel
    {
        [Display(Name = "ชื่อสินค้า")]
        [Required(ErrorMessage = "กรุณากรอกชื่อสินค้า")]
        [StringLength(50, ErrorMessage = "ไม่เกิน 50 ตัวอักษร")]
        public required string ProductName { get; set; }

        [Display(Name = "หมวดสินค้า")]
        [Required(ErrorMessage = "กรุณาเลือกหมวดสินค้า")]
        public Guid CategoryId { get; set; }

        [Display(Name = "หน่วยนับ")]
        [Required(ErrorMessage = "กรุณาเลือกหน่วยนับ")]
        public Guid UnitId { get; set; }

        [Display(Name = "ราคาขาย")]
        [Required(ErrorMessage = "กรุณากรอกราคาขาย")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Sellprice { get; set; }

        [Display(Name = "คำอธิบาย")]
        [StringLength(100, ErrorMessage = "ไม่เกิน 100 ตัวอักษร")]
        public string? Description { get; set; }
    }

    public class ProductEditViewModel
    {
        [Display(Name = "รหัสสินค้า")]
        public Guid ProductId { get; set; }

        [Display(Name = "ชื่อสินค้า")]
        [Required(ErrorMessage = "กรุณากรอกชื่อสินค้า")]
        [StringLength(50, ErrorMessage = "ไม่เกิน 50 ตัวอักษร")]
        public required string ProductName { get; set; }

        [Display(Name = "หมวดสินค้า")]
        [Required(ErrorMessage = "กรุณาเลือกหมวดสินค้า")]
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }

        [Display(Name = "หน่วยนับ")]
        [Required(ErrorMessage = "กรุณาเลือกหน่วยนับ")]
        public Guid UnitId { get; set; }
        public string? UnitName { get; set; }

        [Display(Name = "ราคาขาย")]
        [Required(ErrorMessage = "กรุณากรอกราคาขาย")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Sellprice { get; set; }

        [Display(Name = "คำอธิบาย")]
        [StringLength(100, ErrorMessage = "ไม่เกิน 100 ตัวอักษร")]
        public string? Description { get; set; }

        public byte[]? RowVersion { get; set; }
    }
}
