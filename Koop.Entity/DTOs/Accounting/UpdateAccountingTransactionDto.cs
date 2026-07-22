using Koop.Entity.Entities;
using System.ComponentModel.DataAnnotations;

namespace Koop.Entity.DTOs.Accounting
{
    public class UpdateAccountingTransactionDto
    {
        [Required]
        public DateTime TransactionDate { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public AccountingTransactionType Type { get; set; }

        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
        public decimal Amount { get; set; }
    }
}
