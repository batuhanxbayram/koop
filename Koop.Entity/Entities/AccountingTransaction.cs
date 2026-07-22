using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Koop.Entity.Entities
{
    public class AccountingTransaction
    {
        [Key]
        public long Id { get; set; }

        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public AppUser User { get; set; } = null!;

        public DateTime TransactionDate { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public AccountingTransactionType TransactionType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public Guid? CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public AppUser? CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
