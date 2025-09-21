using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koop.Entity.DTOs.Vehicle
{
  

    public class VehicleDto
    {
        public long Id { get; set; }
        public string LicensePlate { get; set; }
        public string? DriverName { get; set; } 
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public Guid AppUserId { get; set; }
        public string UserFullName { get; set; }
    }
}
