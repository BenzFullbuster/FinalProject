using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public class DeliveryGoods
    {
        [Key]
        public Guid DeliveryGoodsId { get; set; }
        [MaxLength(20)]
        public required string DeliveryGoodsCode { get; set; }
        public DateTime DeliveryGoodsDate { get; set; }
        [ForeignKey("WarehouseId")]
        public Guid WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        [ForeignKey("CustomerId")]
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }
        [ForeignKey("EmployeeId")]
        public Guid EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Vat { get; set; }
        public decimal Nettotal { get; set; }
        [MaxLength(100)]
        public string? Description { get; set; }
        [ForeignKey("StatusId")]
        public Guid StatusId { get; set; }
        public Status? Status { get; set; }
        [ForeignKey("VatTypeId")]
        public int VatTypeId { get; set; }
        public VatType? VatType { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        [Timestamp]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public byte[]? RowVersion { get; set; }

        public ICollection<DeliveryGoodsDetail> DeliveryGoodsDetail { get; set; } = [];
        public ICollection<GoodsReturnFromCustomer> GoodsReturnFromCustomer { get; set; } = [];
    }
}
