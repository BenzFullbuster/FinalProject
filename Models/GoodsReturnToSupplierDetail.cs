using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models
{
    public class GoodsReturnToSupplierDetail
    {
        [Key]
        public Guid GoodsReturnToSupplierDetailId { get; set; }
        [ForeignKey("ProductId")]
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        public decimal Quantity { get; set; }
        public decimal Unitprice { get; set; }
        public decimal Amount { get; set; }
        [ForeignKey("GoodsReturnToSupplierId")]
        public Guid GoodsReturnToSupplierId { get; set; }
        public GoodsReturnToSupplier? GoodsReturnToSupplier { get; set; }
    }
}
