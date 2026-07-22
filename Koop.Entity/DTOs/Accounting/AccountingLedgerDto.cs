namespace Koop.Entity.DTOs.Accounting
{
    public class AccountingLedgerDto
    {
        public Guid UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? UserName { get; set; }
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal ClosingBalance { get; set; }
        public List<AccountingTransactionDto> Transactions { get; set; } = new();
    }
}
