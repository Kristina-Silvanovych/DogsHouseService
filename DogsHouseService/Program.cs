using DogsHouseService.Data;
using DogsHouseService.Services;
using DogsHouseService.Middleware;
using DogsHouseService.DTOs;
using DogsHouseService.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DogsHouseService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers().AddNewtonsoftJson();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // DbContext (SQLite)
            builder.Services.AddDbContext<DogsDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=dogs.db"));

            // Register repository & validator
            builder.Services.AddScoped<IDogRepository, DogRepository>();
            builder.Services.AddScoped<IValidator<CreateDogRequest>, CreateDogRequestValidator>();

            // Configure rate limit options (from appsettings)
            builder.Services.Configure<RateLimitOptions>(builder.Configuration.GetSection("RateLimiting"));

            var app = builder.Build();

            // Ensure DB created and seeded
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DogsDbContext>();
                db.Database.EnsureCreated();
            }

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();

            // Rate limiting middleware
            app.UseSimpleRateLimiting();

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
