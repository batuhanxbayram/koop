using System.ComponentModel.DataAnnotations;

namespace Koop.Entity.DTOs.Accounting
{
    public class CreateAccountingMonthlySummaryDto
    {
        public Guid? UserId { get; set; }

        [Required]
        public long VehicleId { get; set; }

        [Range(1, 12)]
        public int PeriodMonth { get; set; }

        [Range(2000, 2100)]
        public int PeriodYear { get; set; }

        public decimal PreviousBalance { get; set; }

        public decimal IncomeAmount { get; set; }

        public decimal ExpenseAmount { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
