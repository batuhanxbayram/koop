namespace Koop.Entity.DTOs.Accounting
{
    public class AccountingUserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public List<AccountingVehicleDto> Vehicles { get; set; } = new();
        public decimal IncomeTotal { get; set; }
        public decimal ExpenseTotal { get; set; }
        public decimal PaymentTotal { get; set; }
        public decimal Balance { get; set; }
        public int RecordCount { get; set; }
    }
}
