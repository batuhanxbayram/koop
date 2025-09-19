using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koop.Service.Services.TokenServices
{
    public class TokenSettings : ITokenSettings
    {
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public string Secret { get; set; }
        public int TokenValidityMinutes { get; set; }
        public int RefleshTokenValidityDays { get; set; }
    }
}
