using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public class Inventory
    {
        [ForeignKey("ProductId")]
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        [ForeignKey("WarehouseId")]
        public Guid WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        public decimal Balance { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }
}
