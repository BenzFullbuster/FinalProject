using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public class Product
    {
        [Key]
        public Guid ProductId { get; set; }
        [MaxLength(50)]
        public required string ProductName { get; set; }
        [ForeignKey("CategoryId")]
        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }
        [ForeignKey("UnitId")]
        public Guid UnitId { get; set; }
        public Unit? Unit { get; set; }
        public decimal Sellprice { get; set; }
        [MaxLength(100)]
        public string? Description { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        [Timestamp]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public byte[]? RowVersion { get; set; }

        public ICollection<Inventory> Inventory { get; set; } = [];
        public ICollection<PurchaseOrderDetail> PurchaseOrderDetail { get; set; } = [];
        public ICollection<ReceiveGoodsDetail> ReceiveGoodsDetail { get; set; } = [];
        public ICollection<Lot> Lot { get; set; } = [];
        public ICollection<MovementProduct> MovementProduct { get; set; } = [];
        public ICollection<DeliveryGoodsDetail> DeliveryGoodsDetail { get; set; } = [];
        public ICollection<GoodsReturnFromCustomerDetail> GoodsReturnFromCustomerDetail { get; set; } = [];
        public ICollection<GoodsReturnToSupplierDetail> GoodsReturnToSupplierDetail { get; set; } = [];
    }
}
