using DanpheEMR.ServerModel;
using DanpheEMR.ServerModel.MarketingReferralModel;
using Microsoft.EntityFrameworkCore;

namespace DanpheEMR.DalLayer
{
    public class MarketingReferralDbContext : DbContext
    {
        private readonly string _connString;
        public MarketingReferralDbContext(string conn) : base()
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
        public DbSet<BillingTransactionItemModel> BillingTransactionItem { get; set; }
        public DbSet<ReferralSchemeModel> ReferralScheme { get; set; }
        public DbSet<ReferringPartyModel> ReferringParty { get; set; }
        public DbSet<ReferringPartyGroupModel> ReferringPartyGroup { get; set; }
        public DbSet<ReferringOrganizationModel> ReferringOrganization { get; set; }
        public DbSet<ReferralComissionModel> ReferralComission { get; set; }
        public DbSet<BillingFiscalYear> BillingFiscalYears { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BillingTransactionItemModel>().ToTable("BIL_TXN_BillingTransactionItems");
            modelBuilder.Entity<ReferralSchemeModel>().ToTable("MKT_MST_ReferralScheme");
            modelBuilder.Entity<ReferringPartyModel>().ToTable("MKT_CFG_ReferringParty");
            modelBuilder.Entity<ReferringPartyGroupModel>().ToTable("MKT_MST_ReferringPartyGroup");
            modelBuilder.Entity<ReferringOrganizationModel>().ToTable("MKT_MST_ReferringOrganization");
            modelBuilder.Entity<ReferralComissionModel>().ToTable("MKT_TXN_ReferralCommission");
            modelBuilder.Entity<BillingFiscalYear>().ToTable("BIL_CFG_FiscalYears");
            modelBuilder.ApplyTriggerConfigurations();
        }
    }
}
