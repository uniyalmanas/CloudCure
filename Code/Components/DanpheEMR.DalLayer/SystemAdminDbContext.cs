using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DanpheEMR.ServerModel;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Data;

namespace DanpheEMR.DalLayer
{
    public class SystemAdminDbContext : DbContext
    {        
        public DbSet<DatabaseLogModel> DatabaseLog { get; set; }
        public DbSet<AdminParametersModel> AdminParameters { get; set; }
        public DbSet<LoginInformationModel> LoginInformation { get; set; }
        public DbSet<CookieAuthInfoModel> CookieInformation { get; set; }
        public DbSet<AuditTableDisplayName> AuditTableDisplayNames { get; set; }

        private readonly string _connString;
        public SystemAdminDbContext(string conn) : base()
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
            modelBuilder.Entity<DatabaseLogModel>().ToTable("SysAdmin_DBLog");
            modelBuilder.Entity<AdminParametersModel>().ToTable("SysAdmin_Parameters");
            modelBuilder.Entity<LoginInformationModel>().ToTable("DanpheLogInInformation");
            modelBuilder.Entity<CookieAuthInfoModel>().ToTable("Danphe_CookieAuthInfo");
            modelBuilder.Entity<AuditTableDisplayName>().ToTable("tbl_AuditTableDisplayName");
            modelBuilder.ApplyTriggerConfigurations();
        }

      
    }
}
