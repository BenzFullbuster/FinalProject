using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public class DeliveryGoodsDetail
    {
        [Key]
        public Guid DeliveryGoodsDetailId { get; set; }
        [ForeignKey("ProductId")]
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        public decimal Quantity { get; set; }
        public decimal Unitprice { get; set; }
        public decimal Amount { get; set; }
        [ForeignKey("DeliveryGoodsId")]
        public Guid DeliveryGoodsId { get; set; }
        public DeliveryGoods? DeliveryGoods { get; set; }
    }
}
