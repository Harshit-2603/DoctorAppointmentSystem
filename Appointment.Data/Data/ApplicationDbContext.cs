using AppointmentSystem.Models;
using AppointmentSystem.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AppointmentModel = AppointmentSystem.Models.Models.Appointment;

namespace AppointmentSystem.Data.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Availability> Availabilities { get; set; }
        public DbSet<AppointmentModel> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.DoctorProfile)
                .WithOne(d => d.User)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Availability>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Availabilities)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AppointmentModel>()
                .HasOne(ap => ap.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(ap => ap.DoctorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<AppointmentModel>()
                .HasOne(ap => ap.Patient)
                .WithMany()
                .HasForeignKey(ap => ap.PatientId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}