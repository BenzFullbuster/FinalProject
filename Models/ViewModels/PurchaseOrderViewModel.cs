using FinalProject.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace FinalProject.Models.ViewModels
{
    public class PurchaseOrderViewModel
    {
        [Display(Name = "รหัสใบสั่งซื้อ")]
        public Guid PurchaseOrderId { get; set; }

        [Display(Name = "เลขที่ใบสั่งซื้อ")]
        public required string PurchaseOrderCode { get; set; }

        [Display(Name = "ว/ด/ป")]
        public DateTime PurchaseOrderDate { get; set; }

        [Display(Name = "คลังพนักงาน")]
        public string? WarehouseName { get; set; }

        public Supplier? Supplier { get; set; }

        [NotMapped]
        [Display(Name = "ผู้จำหน่าย")]
        public string? SupplierName { get; set; }

        [NotMapped]
        [Display(Name = "ที่อยู่")]
        [DisplayFormat(NullDisplayText = "N/A")]
        public string? SupplierAddress => FormatAddressHelper.GetFormattedAddress(Supplier?.Address, Supplier?.Province?.ProvinceName, Supplier?.District?.DistrictName, Supplier?.Subdistrict?.SubdistrictName, Supplier?.Subdistrict?.Zipcode, includeAddress: true);

        [NotMapped]
        [Display(Name = "เลขประจำตัวผู้เสียภาษี")]
        public string? TaxNumber => Supplier!.Taxnumber != null
            ? Regex.Replace(Supplier!.Taxnumber, @"(\d{1})(\d{4})(\d{5})(\d{2})(\d{1})", "$1 $2 $3 $4 $5")
            : null;

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

        public ICollection<PurchaseOrderDetailViewModel> PurchaseOrderDetails { get; set; } = [];
    }

    public class PurchaseOrderCreateViewModel
    {
        [Display(Name = "เลขที่ใบสั่งซื้อ")]
        [Required(ErrorMessage = "กรุณากรอกเลขที่ใบสั่งซื้อ")]
        [StringLength(20, ErrorMessage = "ไม่เกิน 20 ตัวอักษร")]
        public string? PurchaseOrderCode { get; set; }

        [Display(Name = "ว/ด/ป")]
        [Required(ErrorMessage = "กรุณากรอกวันที่ใบสั่งซื้อ")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime PurchaseOrderDate { get; set; }

        [Display(Name = "คลังพนักงาน")]
        public Guid WarehouseId { get; set; }

        [Display(Name = "ผู้จำหน่าย")]
        [Required(ErrorMessage = "กรุณาเลือกผู้จำหน่าย")]
        public Guid SupplierId { get; set; }

        [Display(Name = "พนักงาน")]
        public Guid EmployeeId { get; set; }

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

        public ICollection<PurchaseOrderDetailViewModel> PurchaseOrderDetails { get; set; } = [];
    }

    public class PurchaseOrderEditViewModel
    {
        [Display(Name = "รหัสใบสั่งซื้อ")]
        public Guid PurchaseOrderId { get; set; }

        [Display(Name = "เลขที่ใบสั่งซื้อ")]
        [Required(ErrorMessage = "กรุณากรอกเลขที่ใบสั่งซื้อ")]
        [StringLength(20, ErrorMessage = "ไม่เกิน 20 ตัวอักษร")]
        public required string PurchaseOrderCode { get; set; }

        [Display(Name = "ว/ด/ป")]
        [Required(ErrorMessage = "กรุณากรอกวันที่ใบสั่งซื้อ")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime PurchaseOrderDate { get; set; }

        [Display(Name = "คลังพนักงาน")]
        public Guid WarehouseId { get; set; }
        public string? WarehouseName { get; set; }

        [Display(Name = "ผู้จำหน่าย")]
        [Required(ErrorMessage = "กรุณาเลือกผู้จำหน่าย")]
        public Guid SupplierId { get; set; }
        public string? SupplierName { get; set; }

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

        public ICollection<PurchaseOrderDetailViewModel> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetailViewModel>();
    }

    public class PurchaseOrderDetailViewModel
    {
        public Guid PurchaseOrderDetailId { get; set; }

        [Display(Name = "พนักงาน")]
        [Required(ErrorMessage = "กรุณาเลือกพนักงาน")]
        public Guid ProductId { get; set; }

        [Display(Name = "รายละเอียดพนักงาน")]
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

        public Guid PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }
    }
}
