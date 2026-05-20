using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Koop.Entity.Entities
{
    public class AccountingRecord
    {
        [Key]
        public long Id { get; set; }

        public long VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }

        public DateTime Date { get; set; }

        public AccountingRecordType Type { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; }

        [StringLength(150)]
        public string? Company { get; set; }

        [StringLength(100)]
        public string? WaybillNo { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? QuantityKg { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceEffect { get; set; }

        public Guid? CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public AppUser? CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
