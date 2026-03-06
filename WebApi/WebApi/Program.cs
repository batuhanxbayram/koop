using Koop.Data.Context;
using Koop.Data.Extensions;
using Koop.Entity.Entities;
using Koop.Service.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WebApi.Hubs;

var builder = WebApplication.CreateBuilder(args);

// --- SERVICES ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();

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
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "http://72.62.114.221",
                "http://75ymkt.com",
                "https://75ymkt.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // SignalR için gerekli
    });
});

builder.Services.LoadServiceLayerExtension(builder.Configuration);
builder.Services.LoadDataLayerExtension(builder.Configuration);

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<AppDbContext>();

    try
    {


        await dbContext.Database.MigrateAsync();
        // 1. HAYALET KOLONLARI TEMİZLE (AppUserId1 Hatası İçin)
        // Bu işlemi async metodun içinde senkron çalıştırıyoruz çünkü Program.cs akışındayız.
        dbContext.Database.ExecuteSqlRaw(@"
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

            IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Vehicles_AppUserId' AND object_id = OBJECT_ID('Vehicles') AND is_unique = 1)
            BEGIN
                DROP INDEX [IX_Vehicles_AppUserId] ON [Vehicles];
                CREATE INDEX [IX_Vehicles_AppUserId] ON [Vehicles]([AppUserId]) WHERE [AppUserId] IS NOT NULL;
            END
        ");


        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            if (!roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new AppRole { Name = roleName }).GetAwaiter().GetResult();
            }
        }

        var adminUserName = "admin";
        var adminUser = userManager.FindByNameAsync(adminUserName).GetAwaiter().GetResult();

        if (adminUser == null)
        {
            var newAdminUser = new AppUser { UserName = adminUserName, FullName = "admin", Email = "admin@koop.com", EmailConfirmed = true };
            var result = userManager.CreateAsync(newAdminUser, "admin123").GetAwaiter().GetResult();

            if (result.Succeeded)
            {
                userManager.AddToRoleAsync(newAdminUser, "Admin").GetAwaiter().GetResult();
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Veritabanı işlemleri sırasında hata: " + ex.Message);
    }
}


app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<QueueHub>("/hubs/queue");

app.Run();