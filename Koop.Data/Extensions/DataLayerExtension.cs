using Koop.Data.Context;
using Koop.Data.Repositories.Abstracts;
using Koop.Data.Repositories.Concrates;
using Koop.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koop.Data.Extensions
{
    public static class DataLayerExtension
    {

        public static IServiceCollection LoadDataLayerExtension(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IRepository,Repository>();
            services.AddDbContextPool<AppDbContext>(opt => opt.UseSqlServer(config.GetConnectionString("DefaultConnection")));
           
  


            return services;
        }
    }
}
