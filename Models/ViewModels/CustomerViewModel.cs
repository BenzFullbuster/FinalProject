using FinalProject.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace FinalProject.Models.ViewModels
{
    public class CustomerViewModel
    {
        [Display(Name = "รหัสลูกค้า")]
        public Guid CustomerId { get; set; }

        [Display(Name = "ชื่อลูกค้า")]
        public required string CustomerName { get; set; }

        [Display(Name = "ที่อยู่")]
        [DisplayFormat(NullDisplayText = "N/A")]
        public string? Address { get; set; }

        [Display(Name = "จังหวัด")]
        [DisplayFormat(NullDisplayText = "N/A")]
        public string? ProvinceName { get; set; }

        [Display(Name = "เขต/อำเภอ")]
        [DisplayFormat(NullDisplayText = "N/A")]
        public string? DistrictName { get; set; }

        [Display(Name = "แขวง/ตำบล")]
        [DisplayFormat(NullDisplayText = "N/A")]
        public string? SubdistrictName { get; set; }

        [Display(Name = "รหัสไปรษณีย์")]
        [DisplayFormat(NullDisplayText = "N/A")]
        public string? Zipcode { get; set; }

        [Display(Name = "เบอร์โทรศัพท์")]
        [DisplayFormat(NullDisplayText = "N/A")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "อีเมล")]
        [DisplayFormat(NullDisplayText = "N/A")]
        public string? Email { get; set; }

        [Display(Name = "เลขประจำตัวผู้เสียภาษี")]
        public string? Taxnumber { get; set; }

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

        [NotMapped]
        [Display(Name = "")]
        public string? Address1 => FormatAddressHelper.GetFormattedAddress(Address, ProvinceName, DistrictName, SubdistrictName, Zipcode, includeAddress: false);

        [NotMapped]
        [Display(Name = "ที่อยู่")]
        [DisplayFormat(NullDisplayText = "N/A")]
        public string? FullAddress => FormatAddressHelper.GetFormattedAddress(Address, ProvinceName, DistrictName, SubdistrictName, Zipcode, includeAddress: true);

        [NotMapped]
        [Display(Name = "เลขประจำตัวผู้เสียภาษี")]
        public string? FormatTaxNumber => Taxnumber != null
            ? Regex.Replace(Taxnumber, @"(\d{1})(\d{4})(\d{5})(\d{2})(\d{1})", "$1 $2 $3 $4 $5")
            : null;
    }

    public class CustomerCreateViewModel
    {
        [Display(Name = "ชื่อลูกค้า")]
        [StringLength(100, ErrorMessage = "ไม่เกิน 100 ตัวอักษร")]
        [Required(ErrorMessage = "กรุณากรอกชื่อลูกค้า")]
        public required string CustomerName { get; set; }

        [Display(Name = "ที่อยู่")]
        [StringLength(100, ErrorMessage = "ไม่เกิน 100 ตัวอักษร")]
        public string? Address { get; set; }

        [Display(Name = "จังหวัด")]
        public int? ProvinceId { get; set; }

        [Display(Name = "เขต/อำเภอ")]
        public int? DistrictId { get; set; }

        [Display(Name = "แขวง/ตำบล")]
        public int? SubdistrictId { get; set; }

        [Display(Name = "รหัสไปรษณีย์")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "กรุณากรอกเฉพาะตัวเลข 5 หลัก")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "กรุณากรอกเฉพาะตัวเลข 5 หลัก")]
        public string? Zipcode { get; set; }

        [Display(Name = "เบอร์โทรศัพท์")]
        [StringLength(12, ErrorMessage = "ห้ามเกิน 12 หลัก")]
        [RegularExpression(@"^\d{1,2}-\d{4}-\d{4}$", ErrorMessage = "รูปแบบเบอร์โทรศัพท์ไม่ถูกต้อง")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "อีเมล")]
        [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
        public string? Email { get; set; }

        [Display(Name = "เลขประจำตัวผู้เสียภาษี")]
        [StringLength(17, MinimumLength = 17, ErrorMessage = "ไม่เกิน 13 หลัก")]
        [Required(ErrorMessage = "กรุณากรอกเลขประจำตัวผู้เสียภาษี 13 ตัวเลข")]
        [RegularExpression(@"^\d{1} \d{4} \d{5} \d{2} \d{1}$", ErrorMessage = "กรุณากรอกเฉพาะตัวเลข 13 หลัก")]
        public required string Taxnumber { get; set; }

        [Display(Name = "คำอธิบาย")]
        [StringLength(100, ErrorMessage = "ไม่เกิน 100 ตัวอักษร")]
        public string? Description { get; set; }
    }

    public class CustomerEditViewModel
    {
        [Display(Name = "รหัสลูกค้า")]
        public Guid CustomerId { get; set; }

        [Display(Name = "ชื่อลูกค้า")]
        [StringLength(100, ErrorMessage = "ไม่เกิน 100 ตัวอักษร")]
        [Required(ErrorMessage = "กรุณากรอกชื่อลูกค้า")]
        public required string CustomerName { get; set; }

        [Display(Name = "ที่อยู่")]
        [StringLength(100, ErrorMessage = "ไม่เกิน 100 ตัวอักษร")]
        public string? Address { get; set; }

        [Display(Name = "จังหวัด")]
        public int? ProvinceId { get; set; }
        public string? ProvinceName { get; set; }

        [Display(Name = "เขต/อำเภอ")]
        public int? DistrictId { get; set; }
        public string? DistrictName { get; set; }

        [Display(Name = "แขวง/ตำบล")]
        public int? SubdistrictId { get; set; }
        public string? SubdistrictName { get; set; }

        [Display(Name = "รหัสไปรษณีย์")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "กรุณากรอกเฉพาะตัวเลข 5 หลัก")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "กรุณากรอกเฉพาะตัวเลข 5 หลัก")]
        public string? Zipcode { get; set; }

        [Display(Name = "เบอร์โทรศัพท์")]
        [StringLength(12, ErrorMessage = "ห้ามเกิน 12 หลัก")]
        [RegularExpression(@"^\d{1,2}-\d{4}-\d{4}$", ErrorMessage = "รูปแบบเบอร์โทรศัพท์ไม่ถูกต้อง")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "อีเมล")]
        [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
        public string? Email { get; set; }

        [Display(Name = "เลขประจำตัวผู้เสียภาษี")]
        [StringLength(17, MinimumLength = 17, ErrorMessage = "ไม่เกิน 13 หลัก")]
        [Required(ErrorMessage = "กรุณากรอกเลขประจำตัวผู้เสียภาษี 13 ตัวเลข")]
        [RegularExpression(@"^\d{1} \d{4} \d{5} \d{2} \d{1}$", ErrorMessage = "กรุณากรอกเฉพาะตัวเลข 13 หลัก")]
        public required string Taxnumber { get; set; }

        [Display(Name = "คำอธิบาย")]
        [StringLength(100, ErrorMessage = "ไม่เกิน 100 ตัวอักษร")]
        public string? Description { get; set; }

        public byte[]? RowVersion { get; set; }
    }
}
