namespace Koop.Entity.DTOs.Accounting
{
    public class AccountingSummaryDto
    {
        public long VehicleId { get; set; }
        public string LicensePlate { get; set; }
        public string? DriverName { get; set; }
        public decimal IncomeTotal { get; set; }
        public decimal ExpenseTotal { get; set; }
        public decimal PaymentTotal { get; set; }
        public decimal Balance { get; set; }
        public int RecordCount { get; set; }
    }
}
