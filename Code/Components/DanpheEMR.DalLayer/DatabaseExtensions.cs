using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using DanpheEMR.ServerModel;
using DanpheEMR.ServerModel.PatientModels;
using DanpheEMR.ServerModel.BillingModels;
using DanpheEMR.ServerModel.MedicareModels;

namespace DanpheEMR.DalLayer
{
    public static class DatabaseExtensions
    {
        public static List<T> SqlQuery<T>(this DatabaseFacade database, string sql, params object[] parameters)
        {
            var result = new List<T>();
            var connection = database.GetDbConnection();
            using (var cmd = connection.CreateCommand())
            {
                var dbTimeout = database.GetCommandTimeout();
                cmd.CommandTimeout = dbTimeout ?? 180;
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                if (parameters != null && parameters.Length > 0)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddSafe(param);
                    }
                }

                bool opened = false;
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                    opened = true;
                }

                try
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        var columnNames = new HashSet<string>(
                            Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i)),
                            StringComparer.OrdinalIgnoreCase
                        );

                        while (reader.Read())
                        {
                            if (typeof(T).IsPrimitive || typeof(T) == typeof(string) || typeof(T) == typeof(decimal) || typeof(T) == typeof(DateTime))
                            {
                                var val = reader.GetValue(0);
                                result.Add(val == DBNull.Value ? default(T) : (T)Convert.ChangeType(val, typeof(T)));
                            }
                            else
                            {
                                var item = Activator.CreateInstance<T>();
                                foreach (var prop in properties)
                                {
                                    if (columnNames.Contains(prop.Name) && prop.CanWrite)
                                    {
                                        var val = reader[prop.Name];
                                        if (val != DBNull.Value)
                                        {
                                            prop.SetValue(item, ChangeType(val, prop.PropertyType));
                                        }
                                    }
                                }
                                result.Add(item);
                            }
                        }
                    }
                }
                finally
                {
                    cmd.Parameters.Clear();
                    if (opened && connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
            return result;
        }

        private static object ChangeType(object value, Type targetType)
        {
            Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (underlyingType.IsEnum)
            {
                if (value is string strValue)
                {
                    return Enum.Parse(underlyingType, strValue, true);
                }
                return Enum.ToObject(underlyingType, value);
            }
            if (underlyingType == typeof(Guid))
            {
                if (value is string strValue)
                {
                    return new Guid(strValue);
                }
                return (Guid)value;
            }
            return Convert.ChangeType(value, underlyingType);
        }
    
        public static void ApplyTriggerConfigurations(this ModelBuilder modelBuilder)
        {
            var triggerMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "ACC_Ledger", "TRG_Update_Ledger" },
                { "ACC_MST_LedgerGroup", "TRG_Update_LedgerGroup" },
                { "ACC_MST_MappingDetail", "TRG_ACC_UpdateMappingDetail" },
                { "ACC_Transactions", "TRG_ACC_Transactions" },
                { "BIL_CFG_BillItemPrice", "BIL_CFG_BillItemPrice_UpdateTrigger" },
                { "BIL_SYNC_BillingAccounting", "TRG_BillToAcc_BillSync" },
                { "BIL_TXN_BillingTransaction", "TRG_BillingTransaction_RestrictBillAlter" },
                { "BIL_TXN_BillingTransactionItems", "TRG_BillToAcc_BillingTxnItem_Updated" },
                { "ER_Patient", "Emergency_PoliceCase_NotificationTrigger" },
                { "LAB_TestRequisition", "PRINT_UpdateTrigger" },
                { "PAT_Patient", "TRG_PAT_History_PatientName" },
                { "PAT_PatientVisits", "PAT_PatientVisits_NotificationTrigger" },
                { "PHRM_TXN_InvoiceItems", "TRG_PHRM_TXN_InvoiceItems_UpdateGRItemPrice" }
            };

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                if (entity.ClrType == null) continue;

                var tableName = entity.GetTableName();
                if (tableName != null && triggerMappings.TryGetValue(tableName, out var triggerName))
                {
                    modelBuilder.Entity(entity.ClrType).ToTable(tableName, tb => tb.HasTrigger(triggerName));
                }
            }

            modelBuilder.ConfigureEMRRelations();
        }

        public static void ConfigureEMRRelations(this ModelBuilder modelBuilder)
        {
            // 1. Patient and Guarantor One-to-One Relation
            var patientType = modelBuilder.Model.FindEntityType(typeof(PatientModel));
            var guarantorType = modelBuilder.Model.FindEntityType(typeof(GuarantorModel));
            if (patientType != null && guarantorType != null)
            {
                modelBuilder.Entity<GuarantorModel>().ToTable("PAT_PatientGurantorInfo");
                modelBuilder.Entity<GuarantorModel>().HasKey(t => t.PatientId);
                modelBuilder.Entity<PatientModel>()
                    .HasOne<GuarantorModel>(a => a.Guarantor)
                    .WithOne(a => a.Patient)
                    .HasForeignKey<GuarantorModel>(a => a.PatientId);
            }

            // 2. Visit and Admission One-to-One Relation
            var visitType = modelBuilder.Model.FindEntityType(typeof(VisitModel));
            var admissionType = modelBuilder.Model.FindEntityType(typeof(AdmissionModel));
            if (visitType != null && admissionType != null)
            {
                modelBuilder.Entity<AdmissionModel>().ToTable("ADT_PatientAdmission");
                modelBuilder.Entity<AdmissionModel>().HasKey(t => t.PatientVisitId);
                modelBuilder.Entity<VisitModel>()
                    .HasOne<AdmissionModel>(a => a.Admission)
                    .WithOne(a => a.Visit)
                    .HasForeignKey<AdmissionModel>(a => a.PatientVisitId);
            }

            // 2b. Vitals and Visit relationship (resolves VisitPatientVisitId shadow column generation regression)
            var vitalsType = modelBuilder.Model.FindEntityType(typeof(VitalsModel));
            if (vitalsType != null && visitType != null)
            {
                modelBuilder.Entity<VitalsModel>()
                    .HasOne<VisitModel>(a => a.Visit)
                    .WithMany(v => v.Vitals)
                    .HasForeignKey(a => a.PatientVisitId);
            }

            // 2c. Patient relationship explicit foreign key configurations (resolves PatientModelPatientId shadow column generation)
            var addressType = modelBuilder.Model.FindEntityType(typeof(AddressModel));
            if (patientType != null)
            {
                if (visitType != null)
                {
                    modelBuilder.Entity<VisitModel>()
                        .HasOne<PatientModel>(a => a.Patient)
                        .WithMany(p => p.Visits)
                        .HasForeignKey(a => a.PatientId);
                }
                if (admissionType != null)
                {
                    modelBuilder.Entity<PatientModel>()
                        .HasMany<AdmissionModel>(p => p.Admissions)
                        .WithOne()
                        .HasForeignKey(a => a.PatientId);
                }
                if (addressType != null)
                {
                    modelBuilder.Entity<AddressModel>()
                        .HasOne<PatientModel>(a => a.Patient)
                        .WithMany(p => p.Addresses)
                        .HasForeignKey(a => a.PatientId);
                }
                var labRequisitionType = modelBuilder.Model.FindEntityType(typeof(LabRequisitionModel));
                if (labRequisitionType != null)
                {
                    modelBuilder.Entity<LabRequisitionModel>()
                        .HasOne<PatientModel>(a => a.Patient)
                        .WithMany(p => p.LabRequisitions)
                        .HasForeignKey(a => a.PatientId);
                }
                var imagingRequisitionType = modelBuilder.Model.FindEntityType(typeof(ImagingRequisitionModel));
                if (imagingRequisitionType != null)
                {
                    modelBuilder.Entity<ImagingRequisitionModel>()
                        .HasOne<PatientModel>(a => a.Patient)
                        .WithMany(p => p.ImagingItemRequisitions)
                        .HasForeignKey(a => a.PatientId);
                }
                var patientFilesType = modelBuilder.Model.FindEntityType(typeof(PatientFilesModel));
                if (patientFilesType != null)
                {
                    modelBuilder.Entity<PatientModel>()
                        .HasMany<PatientFilesModel>(p => p.UploadedFiles)
                        .WithOne()
                        .HasForeignKey(f => f.PatientId);
                }
                var allergyType = modelBuilder.Model.FindEntityType(typeof(AllergyModel));
                if (allergyType != null)
                {
                    modelBuilder.Entity<AllergyModel>()
                        .HasOne<PatientModel>(a => a.Patient)
                        .WithMany(p => p.Allergies)
                        .HasForeignKey(a => a.PatientId);
                }
                var insuranceType = modelBuilder.Model.FindEntityType(typeof(InsuranceModel));
                if (insuranceType != null)
                {
                    modelBuilder.Entity<InsuranceModel>()
                        .HasOne<PatientModel>(a => a.Patient)
                        .WithMany(p => p.Insurances)
                        .HasForeignKey(a => a.PatientId);
                }
                var kinType = modelBuilder.Model.FindEntityType(typeof(KinModel));
                if (kinType != null)
                {
                    modelBuilder.Entity<KinModel>()
                        .HasOne<PatientModel>(a => a.Patient)
                        .WithMany(p => p.KinEmergencyContacts)
                        .HasForeignKey(a => a.PatientId);
                }
            }



            // 3. Appointment Database Constraints
            var appointmentType = modelBuilder.Model.FindEntityType(typeof(AppointmentModel));
            if (appointmentType != null)
            {
                modelBuilder.Entity<AppointmentModel>().ToTable("PAT_Appointment");
                modelBuilder.Entity<AppointmentModel>().Property(a => a.FirstName).IsRequired().HasMaxLength(30);
                modelBuilder.Entity<AppointmentModel>().Property(a => a.LastName).IsRequired().HasMaxLength(30);
                modelBuilder.Entity<AppointmentModel>().Property(a => a.AppointmentType).IsRequired().HasMaxLength(20);
                modelBuilder.Entity<AppointmentModel>().Property(a => a.AppointmentId).ValueGeneratedOnAdd();
            }

            // 4. Employee Model Database Constraints
            var employeeType = modelBuilder.Model.FindEntityType(typeof(EmployeeModel));
            if (employeeType != null)
            {
                modelBuilder.Entity<EmployeeModel>().ToTable("EMP_Employee");
                modelBuilder.Entity<EmployeeModel>().Property(e => e.FirstName).IsRequired().HasMaxLength(30);
                modelBuilder.Entity<EmployeeModel>().Property(e => e.LastName).IsRequired().HasMaxLength(30);
                modelBuilder.Entity<EmployeeModel>().Property(e => e.EmployeeId).ValueGeneratedOnAdd();
            }

            // 5. Billing Transaction trigger mapping consistency
            var billingTxnType = modelBuilder.Model.FindEntityType(typeof(BillingTransactionModel));
            var billingTxnItemType = modelBuilder.Model.FindEntityType(typeof(BillingTransactionItemModel));
            if (billingTxnType != null && billingTxnItemType != null)
            {
                modelBuilder.Entity<BillingTransactionModel>().ToTable("BIL_TXN_BillingTransaction", tb => tb.HasTrigger("TRG_BillingTransaction_RestrictBillAlter"));
                modelBuilder.Entity<BillingTransactionItemModel>().ToTable("BIL_TXN_BillingTransactionItems", tb => tb.HasTrigger("TRG_BillToAcc_BillingTxnItem_Updated"));
                modelBuilder.Entity<BillingTransactionItemModel>()
                      .HasOne<BillingTransactionModel>(s => s.BillingTransaction)
                      .WithMany(s => s.BillingTransactionItems)
                      .HasForeignKey(s => s.BillingTransactionId);
            }

            // 6. Pharmacy Item Type and Pharmacy Item Master One-to-Many Relationship
            var phrmItemType = modelBuilder.Model.FindEntityType(typeof(PHRMItemTypeModel));
            var phrmItemMaster = modelBuilder.Model.FindEntityType(typeof(PHRMItemMasterModel));
            if (phrmItemType != null && phrmItemMaster != null)
            {
                modelBuilder.Entity<PHRMItemTypeModel>()
                    .HasMany<PHRMItemMasterModel>(t => t.Items)
                    .WithOne()
                    .HasForeignKey(i => i.ItemTypeId);
            }
        }

        public static void AddSafe(this System.Data.Common.DbParameterCollection parameterCollection, object param)
        {
            if (param is System.Data.SqlClient.SqlParameter legacyParam)
            {
                var newParam = new Microsoft.Data.SqlClient.SqlParameter();
                newParam.ParameterName = legacyParam.ParameterName;
                newParam.Value = legacyParam.Value ?? DBNull.Value;
                newParam.Direction = legacyParam.Direction;
                newParam.DbType = legacyParam.DbType;
                newParam.SqlDbType = legacyParam.SqlDbType;
                newParam.Size = legacyParam.Size;
                newParam.IsNullable = legacyParam.IsNullable;
                newParam.SourceColumn = legacyParam.SourceColumn;
                newParam.SourceVersion = legacyParam.SourceVersion;
                newParam.Precision = legacyParam.Precision;
                newParam.Scale = legacyParam.Scale;
                newParam.UdtTypeName = legacyParam.UdtTypeName;
                newParam.TypeName = legacyParam.TypeName;
                parameterCollection.Add(newParam);
            }
            else
            {
                parameterCollection.Add(param);
            }
        }
    }
}
