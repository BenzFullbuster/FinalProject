using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public class Lot
    {
        [Key]
        public Guid LotId { get; set; }
        [MaxLength(20)]
        public required string LotNumber { get; set; }
        public DateTime LotDate { get; set; }
        [ForeignKey("WarehouseId")]
        public Guid WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        [ForeignKey("ReceiveGoodsId")]
        public Guid ReceiveGoodsId { get; set; }
        public ReceiveGoods? ReceiveGoods { get; set; }
        [ForeignKey("ProductId")]
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        public decimal Quantity { get; set; }
        public decimal Balance { get; set; }
        public decimal Buyprice { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
    }
}
