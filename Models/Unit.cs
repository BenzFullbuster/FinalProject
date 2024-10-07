using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public class Unit
    {
        [Key]
        public Guid UnitId { get; set; }
        [MaxLength(50)]
        public required string UnitName { get; set; }
        [MaxLength(100)]
        public string? Description { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        [Timestamp]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public byte[]? RowVersion { get; set; }

        public ICollection<Product> Product { get; set; } = [];
    }
}
