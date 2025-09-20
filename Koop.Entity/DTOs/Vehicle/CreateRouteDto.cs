using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koop.Entity.DTOs.Vehicle
{
    public class CreateRouteDto
    {
        [Required]
        [StringLength(100)]
        public string RouteName { get; set; }
    }
}
