using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koop.Entity.DTOs.Vehicle
{
    public class ReorderQueueDto
    {
       
        public List<long> OrderedVehicleIds { get; set; } = new List<long>();
    }
}
