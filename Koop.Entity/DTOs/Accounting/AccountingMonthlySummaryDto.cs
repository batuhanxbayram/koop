namespace Koop.Entity.DTOs.Accounting
{
    public class AccountingMonthlySummaryDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public DateTime MonthStart { get; set; }
        public DateTime MonthEnd { get; set; }
        public decimal IncomeTotal { get; set; }
        public decimal ExpenseTotal { get; set; }
        public decimal PaymentTotal { get; set; }
        public decimal OutgoingTotal { get; set; }
        public decimal ProfitLoss { get; set; }
        public decimal RunningBalance { get; set; }
        public int RecordCount { get; set; }
    }
}
