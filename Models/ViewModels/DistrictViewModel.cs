using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels
{
    public class DistrictViewModel
    {
        [Display(Name = "รหัสเขต/อำเภอ")]
        public int DistrictId { get; set; }

        [Display(Name = "ชื่อเขต/อำเภอ")]
        public required string DistrictName { get; set; }

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

    public class DistrictCreateViewModel
    {
        [Display(Name = "ชื่อเขต/อำเภอ")]
        public required string DistrictName { get; set; }

        [Display(Name = "ชื่อจังหวัด")]
        [Required(ErrorMessage = "กรุณาเลือกชื่อจังหวัด")]
        public int ProvinceId { get; set; }
    }

    public class DistrictEditViewModel
    {
        [Display(Name = "รหัสเขต/อำเภอ")]
        public int DistrictId { get; set; }

        [Display(Name = "ชื่อเขต/อำเภอ")]
        public required string DistrictName { get; set; }

        [Display(Name = "ชื่อจังหวัด")]
        [Required(ErrorMessage = "กรุณาเลือกชื่อจังหวัด")]
        public int ProvinceId { get; set; }
        public string? ProvinceName { get; set; }

        public byte[]? RowVersion { get; set; }
    }
}
