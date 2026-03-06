using Koop.Data.Context;
// Replace the obsolete method call with the updated method or logic.  
// Assuming there is a new method `LoadServiceLayer` to replace the obsolete `LoadServiceLayerExtension`.
using Koop.Data.Extensions;
using Koop.Entity.Entities;
using Koop.Service.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WebApi.Hubs;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddIdentity<AppUser, AppRole>(opt =>
{
    opt.Password.RequiredLength = 6;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireDigit = false;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireUppercase = false;
    opt.SignIn.RequireConfirmedEmail = false;
})
.AddRoles<AppRole>()
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();





builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo() { Title = "KoopApi", Version = "v1", Description = "KoopApiClient" });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
    });
}
    );


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
         builder =>
         {
             builder

                 .WithOrigins(
                    "http://localhost:5173",  
                    "http://localhost:5174",
                    "http://72.62.114.221",  
                    "http://72.62.114.221:80",
                    "http://75ymkt.com",      
                    "http://www.75ymkt.com",
                    "https://75ymkt.com",
                    "https://www.75ymkt.com"
                 )
                 .AllowAnyHeader()
                 .AllowAnyMethod()
        var dbContext = services.GetRequiredService<AppDbContext>();

        await dbContext.Database.ExecuteSqlRawAsync(@"
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Vehicles_AppUserId'
      AND object_id = OBJECT_ID('Vehicles')
      AND is_unique = 1
)
BEGIN
    DROP INDEX [IX_Vehicles_AppUserId] ON [Vehicles];
    CREATE INDEX [IX_Vehicles_AppUserId] ON [Vehicles]([AppUserId]) WHERE [AppUserId] IS NOT NULL;
END
");

        await dbContext.Database.ExecuteSqlRawAsync(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Vehicles_AspNetUsers_AppUserId1')
BEGIN
    ALTER TABLE [Vehicles] DROP CONSTRAINT [FK_Vehicles_AspNetUsers_AppUserId1];
END

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Vehicles_AppUserId1' AND object_id = OBJECT_ID('Vehicles'))
BEGIN
    DROP INDEX [IX_Vehicles_AppUserId1] ON [Vehicles];
END

IF COL_LENGTH('Vehicles', 'AppUserId1') IS NOT NULL
BEGIN
    ALTER TABLE [Vehicles] DROP COLUMN [AppUserId1];
END
");

                 .AllowCredentials(); // SignalR için zorunlu
         });
});


builder.Services.LoadServiceLayerExtension(builder.Configuration);
builder.Services.LoadDataLayerExtension(builder.Configuration);

builder.Services.AddSignalR();



var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

        
        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new AppRole { Name = roleName });
                Console.WriteLine($"'{roleName}' rolü oluþturuldu."); 
            }
        }

        var fullName = "admin";
        var adminUserName = "admin";
        var adminUser = await userManager.FindByNameAsync(adminUserName);

        if (adminUser == null)
        {
            AppUser newAdminUser = new AppUser { UserName = adminUserName ,FullName=fullName};
            
            var result = await userManager.CreateAsync(newAdminUser, "admin123");

            if (result.Succeeded)
            {
             
                await userManager.AddToRoleAsync(newAdminUser, "Admin");
                Console.WriteLine($"'{adminUserName}' kullanýcýsý oluþturuldu ve 'Admin' rolü atandý.");
            }
        }
    }
    catch (Exception ex)
    {
        // Hata durumunda konsola yazdýrma
        Console.WriteLine("Veritabaný tohumlama sýrasýnda bir hata oluþtu: " + ex.Message);
    }
}




    app.UseSwagger();
    app.UseSwaggerUI();


app.UseCors("AllowAll");
// app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.MapHub<QueueHub>("/hubs/queue");

app.Run();
