namespace Koop.Entity.DTOs.Accounting
{
    public class AccountingVehicleDto
    {
        public long Id { get; set; }
        public string LicensePlate { get; set; }
        public string? DriverName { get; set; }
        public Guid? AppUserId { get; set; }
        public string? UserFullName { get; set; }
    }
}
