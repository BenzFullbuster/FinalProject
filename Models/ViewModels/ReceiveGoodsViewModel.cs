using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FinalProject.Models.ViewModels
{
    public class ReceiveGoodsViewModel
    {
        [Display(Name = "รหัสใบรับสินค้า")]
        public Guid ReceiveGoodsId { get; set; }

        [Display(Name = "เลขที่")]
        public required string ReceiveGoodsCode { get; set; }

        [Display(Name = "ว/ด/ป")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ReceiveGoodsDate { get; set; }

        public Supplier? Supplier { get; set; }

        [Display(Name = "ผู้จำหน่าย")]
        public string? SupplierName { get; set; }

        [Display(Name = "ที่อยู่")]
        [DisplayFormat(NullDisplayText = "N/A")]
        public string? SupplierAddress { get; set; }

        [Display(Name = "เบอร์โทรศัพท์")]
        [DisplayFormat(NullDisplayText = "N/A")]
        public string? SupplierPhoneNumber { get; set; }

        [Display(Name = "เลขประจำตัวผู้เสียภาษี")]
        public string? SupplierTaxNumber { get; set; }
        public string? FormattedTaxNumber
        {
            get
            {
                return SupplierTaxNumber != null
                    ? Regex.Replace(SupplierTaxNumber, @"(\d{1})(\d{4})(\d{5})(\d{2})(\d{1})", "$1 $2 $3 $4 $5")
                    : null;
            }
        }

        [Display(Name = "อ้างอิง")]
        public string? PurchaseOrderCode { get; set; }

        [Display(Name = "พนักงาน")]
        public string? EmployeeName { get; set; }

        [Display(Name = "ยอดรวม")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Subtotal { get; set; }

        [Display(Name = "ภาษีมูลค่าเพิ่ม 7%")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Vat { get; set; }

        [Display(Name = "ยอดรวมสุทธิ")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Nettotal { get; set; }

        [NotMapped]
        public string? NettotalText { get; set; }

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

        [Display(Name = "สถานะ")]
        public string? StatusName { get; set; }

        [Display(Name = "ประเภทภาษี")]
        public string? VatTypeName { get; set; }

        public List<ReceiveGoodsDetailViewModel> ReceiveGoodsDetails { get; set; } = [];
    }

    public class ReceiveGoodsCreateViewModel
    {
        [Display(Name = "เลขที่ใบรับสินค้า")]
        [Required(ErrorMessage = "กรุณากรอกเลขที่ใบรับสินค้า")]
        [StringLength(20, ErrorMessage = "ไม่เกิน 20 ตัวอักษร")]
        public string? ReceiveGoodsCode { get; set; }

        [Display(Name = "ว/ด/ป")]
        [Required(ErrorMessage = "กรุณากรอกวันที่ใบรับสินค้า")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ReceiveGoodsDate { get; set; }

        [Display(Name = "คลังสินค้า")]
        public Guid WarehouseId { get; set; }

        [Display(Name = "ผู้จำหน่าย")]
        [Required(ErrorMessage = "กรุณาเลือกผู้จำหน่าย")]
        public Guid SupplierId { get; set; }

        [Display(Name = "อ้างอิงใบสั่งซื้อ")]
        public Guid? PurchaseOrderId { get; set; }

        [Display(Name = "พนักงาน")]
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }

        [Display(Name = "ยอดรวม")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Subtotal { get; set; }

        [Display(Name = "ภาษีมูลค่าเพิ่ม 7%")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Vat { get; set; }

        [Display(Name = "ยอดรวมสุทธิ")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Nettotal { get; set; }

        [Display(Name = "หมายเหตุ")]
        [StringLength(100, ErrorMessage = "ไม่เกิน 100 ตัวอักษร")]
        public string? Description { get; set; }

        [Display(Name = "สถานะ")]
        public Guid StatusId { get; set; }

        [Display(Name = "ประเภทภาษี")]
        public int VatTypeId { get; set; }

        public List<ReceiveGoodsDetailViewModel> ReceiveGoodsDetails { get; set; } = [];
    }

    public class ReceiveGoodsEditViewModel
    {
        [Display(Name = "รหัสใบรับสินค้า")]
        public Guid ReceiveGoodsId { get; set; }

        [Display(Name = "เลขที่ใบรับสินค้า")]
        [Required(ErrorMessage = "กรุณากรอกเลขที่ใบรับสินค้า")]
        [StringLength(20, ErrorMessage = "ไม่เกิน 20 ตัวอักษร")]
        public string? ReceiveGoodsCode { get; set; }

        [Display(Name = "ว/ด/ป")]
        [Required(ErrorMessage = "กรุณากรอกวันที่ใบรับสินค้า")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ReceiveGoodsDate { get; set; }

        [Display(Name = "คลังสินค้า")]
        public Guid WarehouseId { get; set; }

        [Display(Name = "ผู้จำหน่าย")]
        [Required(ErrorMessage = "กรุณาเลือกผู้จำหน่าย")]
        public Guid SupplierId { get; set; }
        public string? SupplierName { get; set; }

        [Display(Name = "อ้างอิงใบสั่งซื้อ")]
        public Guid? PurchaseOrderId { get; set; }
        public string? PurchaseOrderCode { get; set; }

        [Display(Name = "พนักงาน")]
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }

        [Display(Name = "ยอดรวม")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Subtotal { get; set; }

        [Display(Name = "ภาษีมูลค่าเพิ่ม 7%")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Vat { get; set; }

        [Display(Name = "ยอดรวมสุทธิ")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Nettotal { get; set; }

        [Display(Name = "หมายเหตุ")]
        [StringLength(100, ErrorMessage = "ไม่เกิน 100 ตัวอักษร")]
        public string? Description { get; set; }

        [Display(Name = "สถานะ")]
        public Guid StatusId { get; set; }
        public string? StatusName { get; set; }

        [Display(Name = "ประเภทภาษี")]
        public int VatTypeId { get; set; }
        public string? VatTypeName { get; set; }

        public byte[]? RowVersion { get; set; }

        public List<ReceiveGoodsDetailViewModel> ReceiveGoodsDetails { get; set; } = [];
    }

    public class ReceiveGoodsDetailViewModel
    {
        public Guid? ReceiveGoodsDetailId { get; set; }

        [Display(Name = "รายละเอียดสินค้า")]
        [Required(ErrorMessage = "กรุณาเลือกสินค้า")]
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }

        [Display(Name = "จำนวน")]
        [Required(ErrorMessage = "กรุณากรอกจำนวน")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Quantity { get; set; }

        [Display(Name = "ราคาต่อหน่วย")]
        [Required(ErrorMessage = "กรุณากรอกราคาต่อหน่วย")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Unitprice { get; set; }

        [Display(Name = "จำนวนเงิน")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Amount { get; set; }

        public Guid? ReceiveGoodsId { get; set; }
        public ReceiveGoods? ReceiveGoods { get; set; }
    }
}
