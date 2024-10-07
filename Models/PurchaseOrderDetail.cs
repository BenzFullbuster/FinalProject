using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public class PurchaseOrderDetail
    {
        [Key]
        public Guid PurchaseOrderDetailId { get; set; }
        [ForeignKey("ProductId")]
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        public decimal Quantity { get; set; }
        public decimal Unitprice { get; set; }
        public decimal Amount { get; set; }
        [ForeignKey("PurchaseOrderId")]
        public Guid PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }
    }
}
