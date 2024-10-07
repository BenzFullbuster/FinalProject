using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public class MovementType
    {
        [Key]
        public Guid MovementTypeId { get; set; }
        [MaxLength(50)]
        public required string MovementTypeName { get; set; }
        [MaxLength(100)]
        public string? Description { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        [Timestamp]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public byte[]? RowVersion { get; set; }

        public ICollection<MovementProduct> MovementProduct { get; set; } = [];
    }
}
