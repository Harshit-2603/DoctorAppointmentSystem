using System.Collections.Generic;
using AppointmentSystem.Models.Models;

namespace DoctorAppointmentSystem.ViewModels.Patient
{
    public class DoctorListViewModel
    {
        public List<ApplicationUser> Doctors { get; set; } = new();
    }
}