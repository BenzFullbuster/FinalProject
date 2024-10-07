using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public class EmployeeWarehouse
    {
        [ForeignKey("EmployeeId")]
        public Guid EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        [ForeignKey("WarehouseId")]
        public Guid WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        public bool Approval { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }
}
