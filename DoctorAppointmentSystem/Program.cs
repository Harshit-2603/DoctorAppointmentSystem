using AppointmentSystem.Data.Data;
using AppointmentSystem.Models;
using AppointmentSystem.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentSystem
{
    // No-op email sender — required by Identity UI even when email confirmation is off
    public class NoOpEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
            => Task.CompletedTask;
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── Database — SQL Server ──────────────────────────────
            var connectionString = builder.Configuration
                .GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // ── Identity ───────────────────────────────────────────
            builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

            // Required by Identity UI scaffolding
            builder.Services.AddTransient<IEmailSender, NoOpEmailSender>();

            // ── MVC + Razor + MediatR ──────────────────────────────
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
                typeof(AppointmentSystem.Services.Handlers.AdminFeatures.GenerateSlotsHandler).Assembly));

            var app = builder.Build();

            // ── Seed Roles + Admin ─────────────────────────────────
            await SeedRolesAndAdminAsync(app);

            // ── Middleware Pipeline ────────────────────────────────
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();

            // ── Auto-assign Patient role on self-register ──────────
            // When a user registers via default Identity UI, they get no role.
            // This middleware catches that and assigns "Patient" automatically,
            // then refreshes the auth cookie so IsInRole() works immediately.
            app.Use(async (ctx, next) =>
            {
                if (ctx.User.Identity?.IsAuthenticated == true)
                {
                    var userManager = ctx.RequestServices
                        .GetRequiredService<UserManager<ApplicationUser>>();
                    var signInManager = ctx.RequestServices
                        .GetRequiredService<SignInManager<ApplicationUser>>();

                    var user = await userManager.GetUserAsync(ctx.User);
                    if (user != null)
                    {
                        var roles = await userManager.GetRolesAsync(user);
                        if (roles.Count == 0)
                        {
                            // Assign Patient role to self-registered users
                            user.Role = UserRole.Patient;
                            await userManager.UpdateAsync(user);
                            await userManager.AddToRoleAsync(user, "Patient");

                            // Refresh cookie so role claim is picked up immediately
                            await signInManager.RefreshSignInAsync(user);

                            // Redirect to home so layout re-renders with correct role
                            ctx.Response.Redirect("/");
                            return;
                        }
                    }
                }
                await next();
            });

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }

        // ── Seed Method ────────────────────────────────────────────
        private static async Task SeedRolesAndAdminAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

                // Step 1: Create all roles in AspNetRoles
                foreach (var roleName in new[] { "Admin", "Doctor", "Patient" })
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                        await roleManager.CreateAsync(new IdentityRole<int> { Name = roleName });
                }

                // Step 2: Fix any existing users missing a row in AspNetUserRoles
                var allUsers = userManager.Users.ToList();
                foreach (var user in allUsers)
                {
                    var assignedRoles = await userManager.GetRolesAsync(user);
                    if (assignedRoles.Count == 0)
                    {
                        var roleName = user.Role switch
                        {
                            UserRole.Admin => "Admin",
                            UserRole.Doctor => "Doctor",
                            UserRole.Patient => "Patient",
                            _ => "Patient"
                        };
                        await userManager.AddToRoleAsync(user, roleName);
                    }
                }

                // Step 3: Seed Admin user if not exists
                const string adminEmail = "admin@system.com";
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var newAdmin = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = "System Admin",
                        Role = UserRole.Admin,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(newAdmin, "Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newAdmin, "Admin");
                    }
                    else
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        foreach (var error in result.Errors)
                            logger.LogError("Admin seed error: {Code} - {Description}",
                                error.Code, error.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
    }
}