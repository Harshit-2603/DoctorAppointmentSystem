using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;
using AppointmentSystem.Data.Data;
using AppointmentSystem.Models.Models;

namespace DoctorAppointmentSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Connection String Configuration
            var connectionString = builder.Configuration
                .GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // 2. DbContext Registration
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // 3. Identity Configuration - Fixed for ApplicationUser and int keys
            // This fix addresses the "No service for type UserManager" error
            builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

            // 4. MediatR Registration
            // This is required to make the Handlers in AppointmentSystem.Services work
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
                typeof(Program).Assembly,
                // Explicitly pointing to a handler in your Services project ensures all handlers are found
                typeof(AppointmentSystem.Services.Handlers.AdminFeatures.CreateUserHandler).Assembly
            ));

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // 5. Middleware Pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Authentication must come BEFORE Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            app.Run();
        }
    }
}