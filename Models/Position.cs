using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public class Position
    {
        [Key]
        public Guid PositionId { get; set; }
        [MaxLength(50)]
        public required string PositionName { get; set; }
        [MaxLength(100)]
        public string? Description { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        [Timestamp]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public byte[]? RowVersion { get; set; }

        public ICollection<Employee> Employee { get; set; } = [];
    }
}
