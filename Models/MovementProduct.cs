using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public class MovementProduct
    {
        [Key]
        public Guid MovementProductId { get; set; }
        public DateTime MovementProductDate { get; set; }
        [ForeignKey("WarehouseId")]
        public Guid WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        [ForeignKey("ReceiveGoodsId")]
        public Guid ReceiveGoodsId { get; set; }
        public ReceiveGoods? ReceiveGoods { get; set; }
        [ForeignKey("MovementTypeId")]
        public Guid MovementTypeId { get; set; }
        public MovementType? MovementType { get; set; }
        [ForeignKey("ProductId")]
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        [ForeignKey("LotId")]
        public Guid LotId { get; set; }
        public Lot? Lot { get; set; }
        public decimal Quantity { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
    }
}
