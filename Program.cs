using System.Text;
using AutoRentalApi.Data;
using AutoRentalApi.Models;
using AutoRentalApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddMemoryCache();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true, ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidAudience = builder.Configuration["Jwt:Audience"], IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)), ClockSkew = TimeSpan.FromSeconds(30) };
});
builder.Services.AddAuthorization();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    if (!await db.Users.AnyAsync())
    {
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
        foreach (var (username, password, role) in new[] { ("admin", "Admin123!", "Admin"), ("manager", "Manager123!", "Manager"), ("analyst", "Analyst123!", "Analyst") })
        {
            var user = new AppUser { Username = username, Role = role };
            user.PasswordHash = hasher.HashPassword(user, password);
            db.Users.Add(user);
        }
        await db.SaveChangesAsync();
    }
    if (!await db.Cars.AnyAsync())
    {
        db.Cars.AddRange(new Car { RegistrationNumber = "А123АА77", Brand = "Kia", Model = "Rio", Year = 2022 }, new Car { RegistrationNumber = "В456ВВ77", Brand = "Hyundai", Model = "Solaris", Year = 2023 }, new Car { RegistrationNumber = "С789СС77", Brand = "Skoda", Model = "Octavia", Year = 2021 });
        db.Tariffs.AddRange(new Tariff { Name = "Эконом", PricePerDay = 2500 }, new Tariff { Name = "Комфорт", PricePerDay = 3800 }, new Tariff { Name = "Бизнес", PricePerDay = 6000 });
        db.Customers.AddRange(new Customer { FullName = "Иванов Иван Иванович", Phone = "+7-900-000-00-01", Email = "ivanov@example.com" }, new Customer { FullName = "Петрова Анна Сергеевна", Phone = "+7-900-000-00-02", Email = "petrova@example.com" });
        await db.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
