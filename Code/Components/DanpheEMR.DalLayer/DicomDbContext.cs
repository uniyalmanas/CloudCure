using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DanpheEMR.ServerModel;
using Microsoft.EntityFrameworkCore;

namespace DanpheEMR.DalLayer
{
    public class DicomDbContext : DbContext
    {
        private readonly string _connString;
        public DicomDbContext(string conn) : base()
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

        public DbSet<PatientStudyModel> PatientStudies { get; set; }
        public DbSet<DicomFileInfoModel> DicomFiles { get; set; }
        public DbSet<SeriesInfoModel> Series { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PatientStudyModel>().ToTable("DCM_PatientStudy");
            modelBuilder.Entity<DicomFileInfoModel>().ToTable("DCM_DicomFiles");
            modelBuilder.Entity<SeriesInfoModel>().ToTable("DCM_Series");
            modelBuilder.ApplyTriggerConfigurations();
        }
    }
}
