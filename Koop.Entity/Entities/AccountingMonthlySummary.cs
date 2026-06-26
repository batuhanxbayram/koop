using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Koop.Entity.Entities
{
    public class AccountingMonthlySummary
    {
        [Key]
        public long Id { get; set; }

        public Guid? UserId { get; set; }

        [ForeignKey("UserId")]
        public AppUser? User { get; set; }

        public long? VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }

        [Required]
        [StringLength(20)]
        public string PlateNumber { get; set; } = string.Empty;

        public int PeriodMonth { get; set; }

        public int PeriodYear { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PreviousBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal IncomeAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ExpenseAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalBalance { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public Guid? CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public AppUser? CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
