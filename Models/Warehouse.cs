using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public class Warehouse
    {
        [Key]
        public Guid WarehouseId { get; set; }
        [MaxLength(5)]
        public required string WarehouseCode { get; set; }
        [MaxLength(50)]
        public required string WarehouseName { get; set; }
        [MaxLength(100)]
        public string? Description { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        [Timestamp]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public byte[]? RowVersion { get; set; }

        public ICollection<Inventory> Inventory { get; set; } = [];
        public ICollection<EmployeeWarehouse> EmployeeWarehouse { get; set; } = [];
        public ICollection<PurchaseOrder> PurchaseOrder { get; set; } = [];
        public ICollection<ReceiveGoods> ReceiveGoods { get; set; } = [];
        public ICollection<Lot> Lot { get; set; } = [];
        public ICollection<MovementProduct> MovementProduct { get; set; } = [];
        public ICollection<DeliveryGoods> DeliveryGoods { get; set; } = [];
        public ICollection<GoodsReturnFromCustomer> GoodsReturnFromCustomer { get; set; } = [];
        public ICollection<GoodsReturnToSupplier> GoodsReturnToSupplier { get; set; } = [];
    }
}
