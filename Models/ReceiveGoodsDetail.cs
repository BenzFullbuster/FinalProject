using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public class ReceiveGoodsDetail
    {
        [Key]
        public Guid ReceiveGoodsDetailId { get; set; }
        [ForeignKey("ProductId")]
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        public decimal Quantity { get; set; }
        public decimal Unitprice { get; set; }
        public decimal Amount { get; set; }
        [ForeignKey("ReceiveGoodsId")]
        public Guid ReceiveGoodsId { get; set; }
        public ReceiveGoods? ReceiveGoods { get; set; }
    }
}
