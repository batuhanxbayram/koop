using Koop.Entity.Entities;

namespace Koop.Entity.DTOs.Accounting
{
    public class AccountingRecordDto
    {
        public long Id { get; set; }
        public long VehicleId { get; set; }
        public string LicensePlate { get; set; }
        public DateTime Date { get; set; }
        public AccountingRecordType Type { get; set; }
        public string TypeName { get; set; }
        public string Category { get; set; }
        public string? Company { get; set; }
        public string? WaybillNo { get; set; }
        public string? Description { get; set; }
        public decimal? QuantityKg { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal NetAmount { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal BalanceEffect { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
