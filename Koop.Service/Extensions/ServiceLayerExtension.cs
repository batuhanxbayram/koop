using Koop.Service.Services.TokenService;
using Koop.Service.Services.TokenServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Koop.Service.Extensions
{
    public static  class ServiceLayerExtension
    {

        [Obsolete]
        public static IServiceCollection LoadServiceLayerExtension(this IServiceCollection services, IConfiguration cfg)

        {
            var assembly = Assembly.GetExecutingAssembly();

            services.Configure<TokenSettings>(cfg.GetSection("JWT"));
            services.AddTransient<ITokenService, TokenService>();
            

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }
            ).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
            {
                opt.SaveToken = true;
                opt.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["JWT:Secret"])),
                    ValidateLifetime = false,
                    ValidIssuer = cfg["JWT:Issuer"],
                    ValidAudience = cfg["JWT:Audience"],
                    ClockSkew = TimeSpan.Zero
                };


            });


            return services;
        }



    }
}
