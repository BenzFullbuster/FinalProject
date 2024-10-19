using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models
{
    public class Subdistrict
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubdistrictId { get; set; }
        [MaxLength(50)]
        public required string SubdistrictName { get; set; }
        public required string Zipcode { get; set; }
        [ForeignKey("DistrictId")]
        public int DistrictId { get; set; }
        public District? District { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        [Timestamp]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public byte[]? RowVersion { get; set; }

        public ICollection<Employee> Employee { get; set; } = [];
        public ICollection<Supplier> Supplier { get; set; } = [];
        public ICollection<Customer> Customer { get; set; } = [];
    }
}
