using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models
{
    public class GoodsReturnFromCustomerDetail
    {
        [Key]
        public Guid GoodsReturnFromCustomerDetailId { get; set; }
        [ForeignKey("ProductId")]
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        public decimal Quantity { get; set; }
        public decimal Unitprice { get; set; }
        public decimal Amount { get; set; }
        [ForeignKey("GoodsReturnFromCustomerId")]
        public Guid GoodsReturnFromCustomerId { get; set; }
        public GoodsReturnFromCustomer? GoodsReturnFromCustomer { get; set; }
    }
}
