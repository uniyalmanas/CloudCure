using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DanpheEMR.ServerModel;

namespace DanpheEMR.DalLayer
{
    public class AppointmentDbContext : DbContext
    {
        public DbSet<AppointmentModel> Appointments { get; set; }
        public DbSet<VisitModel> Visit { get; set; }
        public DbSet<EmployeeModel> Employees { get; set; }
        private readonly string _connString;
        public AppointmentDbContext(string conn) : base()
        {
            _connString = conn;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connString, sqlServerOptions => sqlServerOptions.CommandTimeout(180));
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppointmentModel>().ToTable("PAT_Appointment");
            modelBuilder.Entity<VisitModel>().ToTable("PAT_PatientVisits");
            modelBuilder.Entity<EmployeeModel>().ToTable("EMP_Employee");

            modelBuilder.ApplyTriggerConfigurations();
        }

    }
}

