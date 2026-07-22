using Koop.Entity.Entities;

namespace Koop.Entity.DTOs.Accounting
{
    public class AccountingTransactionDto
    {
        public long Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public AccountingTransactionType Type { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal RunningBalance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
