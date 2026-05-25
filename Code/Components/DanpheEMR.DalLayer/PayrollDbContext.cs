using DanpheEMR.ServerModel;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DanpheEMR.Core.Parameters;

namespace DanpheEMR.DalLayer
{
    public class PayrollDbContext :DbContext
    {

        public DbSet<AttendanceDailyTimeRecord> attendanceDailyTimeRecords { get; set; }
        public DbSet<DailyMuster> dailyMusters { get; set; }
        public DbSet<EmployeeModel> Employee { get; set; }
        public DbSet<WeekendHolidays> WeekendHolidays { get; set; }
        public DbSet<LeaveCategory> leaveCategories { get; set; }
        public DbSet<ParameterModel> parameterModels { get; set; }
        public DbSet<LeaveRuleModel> leaveRuleModels { get; set; }
        public DbSet<HolidayModel> HolidayList { get; set; }
        public DbSet<EmployeeLeaveModel> employeeLeaveModels { get; set; }
        private readonly string _connString;
        public PayrollDbContext(string conn) : base()
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
            modelBuilder.Entity<AttendanceDailyTimeRecord>().ToTable("PROLL_AttendanceDailyTimeRecord");
            modelBuilder.Entity<DailyMuster>().ToTable("PROLL_DailyMuster");
            modelBuilder.Entity<EmployeeModel>().ToTable("EMP_Employee");
            modelBuilder.Entity<WeekendHolidays>().ToTable("PROLL_MST_WeekendHolidays");
            modelBuilder.Entity<LeaveCategory>().ToTable("PROLL_MST_LeaveCategory");
            modelBuilder.Entity<ParameterModel>().ToTable("CORE_CFG_Parameters");
            modelBuilder.Entity<LeaveRuleModel>().ToTable("PROLL_MST_LeaveRules");
            modelBuilder.Entity<LeaveCategory>().ToTable("PROLL_MST_LeaveCategory");
            modelBuilder.Entity<HolidayModel>().ToTable("PROLL_MST_Holidays");
            modelBuilder.Entity<EmployeeLeaveModel>().ToTable("PROLL_EmpLeave");
            modelBuilder.ApplyTriggerConfigurations();
        }
    }
}
