using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels
{
    public class WarehouseViewModel
    {
        [Display(Name = "รหัสคลังสินค้า")]
        public Guid WarehouseId { get; set; }

        [Display(Name = "ชื่อย่อ")]
        public required string WarehouseCode { get; set; }

        [Display(Name = "ชื่อคลังสินค้า")]
        public required string WarehouseName { get; set; }

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

    public class WarehouseCreateViewModel
    {
        [Display(Name = "ชื่อย่อ")]
        [Required(ErrorMessage = "กรุณากรอกชื่อย่อ")]
        [StringLength(5, ErrorMessage = "ไม่เกิน 5 ตัวอักษร")]
        public required string WarehouseCode { get; set; }

        [Display(Name = "ชื่อคลังสินค้า")]
        [Required(ErrorMessage = "กรุณากรอกชื่อคลังสินค้า")]
        [StringLength(50, ErrorMessage = "ไม่เกิน 50 ตัวอักษร")]
        public required string WarehouseName { get; set; }

        [Display(Name = "คำอธิบาย")]
        [StringLength(100, ErrorMessage = "ไม่เกิน 100 ตัวอักษร")]
        public string? Description { get; set; }
    }

    public class WarehouseEditViewModel
    {
        [Display(Name = "รหัสคลังสินค้า")]
        public Guid WarehouseId { get; set; }

        [Display(Name = "ชื่อย่อ")]
        [Required(ErrorMessage = "กรุณากรอกชื่อย่อ")]
        [StringLength(5, ErrorMessage = "ไม่เกิน 5 ตัวอักษร")]
        public required string WarehouseCode { get; set; }

        [Display(Name = "ชื่อคลังสินค้า")]
        [Required(ErrorMessage = "กรุณากรอกชื่อคลังสินค้า")]
        [StringLength(50, ErrorMessage = "ไม่เกิน 50 ตัวอักษร")]
        public required string WarehouseName { get; set; }

        [Display(Name = "คำอธิบาย")]
        [StringLength(100, ErrorMessage = "ไม่เกิน 100 ตัวอักษร")]
        public string? Description { get; set; }

        public byte[]? RowVersion { get; set; }
    }
}
