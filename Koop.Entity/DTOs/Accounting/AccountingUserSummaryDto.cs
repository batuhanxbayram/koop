namespace Koop.Entity.DTOs.Accounting
{
    public class AccountingUserSummaryDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public decimal IncomeTotal { get; set; }
        public decimal ExpenseTotal { get; set; }
        public decimal PaymentTotal { get; set; }
        public decimal OutgoingTotal { get; set; }
        public decimal Balance { get; set; }
        public int RecordCount { get; set; }
    }
}
