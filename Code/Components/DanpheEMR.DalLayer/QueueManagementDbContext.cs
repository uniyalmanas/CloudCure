using DanpheEMR.ServerModel;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanpheEMR.DalLayer
{
    public class QueueManagementDbContext : DbContext
    {
        private readonly string _connString;
        public QueueManagementDbContext(string conn) : base()
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
        public DbSet<DepartmentModel> Department { get; set; }
        public DbSet<VisitModel> Visits { get; set; }
        public DbSet<PatientModel> Patients { get; set; }
        public DbSet<EmployeeModel> Employees { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeModel>().ToTable("EMP_Employee");
            modelBuilder.Entity<DepartmentModel>().ToTable("MST_Department");
            modelBuilder.Entity<PatientModel>().ToTable("PAT_Patient");
            modelBuilder.Entity<VisitModel>().ToTable("PAT_PatientVisits");
            modelBuilder.Entity<VisitModel>()
            .HasOne<PatientModel>(a => a.Patient)
            .WithMany(a => a.Visits)
            .HasForeignKey(s => s.PatientId);
            modelBuilder.Entity<VisitModel>()
            .HasOne<AdmissionModel>(a => a.Admission)
            .WithOne(a => a.Visit)
            .HasForeignKey<AdmissionModel>(a => a.PatientVisitId);
            modelBuilder.ApplyTriggerConfigurations();
        }
    }
}
