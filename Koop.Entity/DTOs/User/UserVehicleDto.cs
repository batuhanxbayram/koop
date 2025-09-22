using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koop.Entity.DTOs.User
{
    public class UserVehicleDto
    {

        public Guid Id { get; set; } 
        public string FullName { get; set; }
        public string LicensePlate { get; set; }
        public string PhoneNumber { get; set; }
    }
}
