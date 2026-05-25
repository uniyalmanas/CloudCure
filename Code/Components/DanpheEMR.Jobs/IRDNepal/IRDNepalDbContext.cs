using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DanpheEMR.Jobs.IRDNepal;
using DanpheEMR.ServerModel;

namespace DanpheEMR.Jobs.IRDNepal
{
    class IRDNepalDbContext : DbContext
    {
        private readonly string _connString;
        public IRDNepalDbContext(string conn) : base()
        {
            _connString = conn;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connString);
            }
        }


        public DbSet<IRD_Common_InvoiceModel> IrdCommonInvoiceSets { get; set; }
        public DbSet<BillingTransactionModel> BillingTransactions { get; set; }
        public DbSet<BillInvoiceReturnModel> BillReturnRequests { get; set; }
      

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IRD_Common_InvoiceModel>().ToTable("IRD_Sync_Invoices_Common");
            modelBuilder.Entity<BillingTransactionModel>().ToTable("BIL_TXN_BillingTransaction");
            modelBuilder.Entity<BillInvoiceReturnModel>().ToTable("BIL_TXN_InvoiceReturn");
            modelBuilder.Entity<PatientModel>().ToTable("PAT_Patient");
        }

    }
}
