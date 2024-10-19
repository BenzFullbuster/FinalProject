﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models
{
    public class Supplier
    {
        [Key]
        public Guid SupplierId { get; set; }
        [MaxLength(100)]
        public required string SupplierName { get; set; }
        [MaxLength(100)]
        public string? Address { get; set; }
        [ForeignKey("ProvinceId")]
        public int? ProvinceId { get; set; }
        public Province? Province { get; set; }
        [ForeignKey("DistrictId")]
        public int? DistrictId { get; set; }
        public District? District { get; set; }
        [ForeignKey("SubdistrictId")]
        public int? SubdistrictId { get; set; }
        public Subdistrict? Subdistrict { get; set; }
        [MaxLength(5)]
        public string? Zipcode { get; set; }
        [MaxLength(12)]
        public string? PhoneNumber { get; set; }
        [MaxLength(255)]
        public string? Email { get; set; }
        [MaxLength(13)]
        public required string Taxnumber { get; set; }
        [MaxLength(100)]
        public string? Description { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        [Timestamp]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public byte[]? RowVersion { get; set; }

        public ICollection<PurchaseOrder> PurchaseOrder { get; set; } = [];
        public ICollection<ReceiveGoods> ReceiveGoods { get; set; } = [];
        public ICollection<GoodsReturnToSupplier> GoodsReturnToSupplier { get; set; } = [];
    }
}
