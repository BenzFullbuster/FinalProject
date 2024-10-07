using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models
{
    public class District
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DistrictId { get; set; }
        [MaxLength(50)]
        public required string DistrictName { get; set; }
        [ForeignKey("ProvinceId")]
        public int ProvinceId { get; set; }
        public Province? Province { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        [Timestamp]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public byte[]? RowVersion { get; set; }

        public ICollection<Subdistrict> Subdistrict { get; set; } = [];
        public ICollection<Employee> Employee { get; set; } = [];
        public ICollection<Supplier> Supplier { get; set; } = [];
        public ICollection<Customer> Customer { get; set; } = [];
    }
}
