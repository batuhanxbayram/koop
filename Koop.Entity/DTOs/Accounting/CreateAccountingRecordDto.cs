using Koop.Entity.Entities;
using System.ComponentModel.DataAnnotations;

namespace Koop.Entity.DTOs.Accounting
{
    public class CreateAccountingRecordDto
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public AccountingRecordType Type { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; }

        [StringLength(150)]
        public string? Company { get; set; }

        [StringLength(100)]
        public string? WaybillNo { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public decimal? QuantityKg { get; set; }

        public decimal? UnitPrice { get; set; }

        public decimal? Amount { get; set; }
    }
}
