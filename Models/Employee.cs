using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public class Employee
    {
        [Key]
        public Guid EmployeeId { get; set; }
        [MaxLength(100)]
        public required string Username { get; set; }
        [MaxLength(255)]
        public required string Password { get; set; }
        [MaxLength(255)]
        public required string Salt { get; set; }
        [MaxLength(100)]
        public required string EmployeeName { get; set; }
        [ForeignKey("PositionId")]
        public Guid PositionId { get; set; }
        public Position? Position { get; set; }
        [MaxLength(100)]
        public string? Address { get; set; }
        [ForeignKey("ProvinceId")]
        public int? ProvinceId { get; set; }
        public Province? Province { get; set; }
        [ForeignKey("DistrictId")]
        public int? DistrictId { get; set; }
        public District? District { get; set; }
        [ForeignKey("SubdistrictId")]
        public int? SubdistrictId { get; set; }
        public Subdistrict? Subdistrict { get; set; }
        [MaxLength(5)]
        public string? Zipcode { get; set; }
        [MaxLength(12)]
        public string? PhoneNumber { get; set; }
        [MaxLength(255)]
        public required string Email { get; set; }
        [MaxLength(13)]
        public required string Taxnumber { get; set; }
        [MaxLength(100)]
        public string? Description { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        [Timestamp]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public byte[]? RowVersion { get; set; }

        public ICollection<EmployeeWarehouse> EmployeeWarehouse { get; set; } = [];
        public ICollection<PurchaseOrder> PurchaseOrder { get; set; } = [];
        public ICollection<ReceiveGoods> ReceiveGoods { get; set; } = [];
        public ICollection<DeliveryGoods> DeliveryGoods { get; set; } = [];
        public ICollection<GoodsReturnFromCustomer> GoodsReturnFromCustomer { get; set; } = [];
        public ICollection<GoodsReturnToSupplier> GoodsReturnToSupplier { get; set; } = [];
    }
}
