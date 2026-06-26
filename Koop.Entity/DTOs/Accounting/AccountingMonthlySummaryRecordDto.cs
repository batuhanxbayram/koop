namespace Koop.Entity.DTOs.Accounting
{
    public class AccountingMonthlySummaryRecordDto
    {
        public long Id { get; set; }
        public Guid? UserId { get; set; }
        public string? UserFullName { get; set; }
        public long? VehicleId { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public string PeriodName { get; set; } = string.Empty;
        public decimal PreviousBalance { get; set; }
        public decimal IncomeAmount { get; set; }
        public decimal ExpenseAmount { get; set; }
        public decimal TotalBalance { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
