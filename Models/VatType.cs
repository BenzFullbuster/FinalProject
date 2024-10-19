using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models
{
    public class VatType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VatTypeId { get; set; }
        public required string VatTypeName { get; set; }
        [MaxLength(100)]
        public string? Description { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        [Timestamp]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public byte[]? RowVersion { get; set; }

        public ICollection<PurchaseOrder> PurchaseOrder { get; set; } = [];
        public ICollection<ReceiveGoods> ReceiveGoods { get; set; } = [];
        public ICollection<DeliveryGoods> DeliveryGoods { get; set; } = [];
        public ICollection<GoodsReturnFromCustomer> GoodsReturnFromCustomer { get; set; } = [];
        public ICollection<GoodsReturnToSupplier> GoodsReturnToSupplier { get; set; } = [];
    }
}
