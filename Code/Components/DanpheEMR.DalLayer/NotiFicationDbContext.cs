using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DanpheEMR.ServerModel;
using DanpheEMR.ServerModel.NotificationModels;

namespace DanpheEMR.DalLayer
{
    public class NotiFicationDbContext : DbContext
    {
        
        public DbSet<NotificationViewModel> Notifications { get; set; }
        public DbSet<VisitModel> PatientVisits { get; set; }

        private readonly string _connString;
        public NotiFicationDbContext(string conn) : base()
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
            modelBuilder.Entity<NotificationViewModel>().ToTable("CORE_Notification");
            modelBuilder.Entity<VisitModel>().ToTable("PAT_PatientVisits");
            modelBuilder.ApplyTriggerConfigurations();
        }
    }
}
