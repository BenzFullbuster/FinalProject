using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public class Province
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProvinceId { get; set; }
        [MaxLength(50)]
        public required string ProvinceName { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        [Timestamp]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public byte[]? RowVersion { get; set; }

        public ICollection<District> District { get; set; } = [];
        public ICollection<Employee> Employee { get; set; } = [];
        public ICollection<Supplier> Supplier { get; set; } = [];
        public ICollection<Customer> Customer { get; set; } = [];
    }
}
