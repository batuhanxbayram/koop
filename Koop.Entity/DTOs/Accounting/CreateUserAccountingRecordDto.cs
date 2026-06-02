namespace Koop.Entity.DTOs.Accounting
{
    public class CreateUserAccountingRecordDto : CreateAccountingRecordDto
    {
        public long? VehicleId { get; set; }
    }
}
