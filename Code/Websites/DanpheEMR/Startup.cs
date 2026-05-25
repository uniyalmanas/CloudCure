using Audit.SqlServer.Providers;
using DanpheEMR.CommonTypes;
using DanpheEMR.Controllers.Settings.DTO;
using DanpheEMR.Core.Caching;
using DanpheEMR.Core.Configuration;
using DanpheEMR.DalLayer;
using DanpheEMR.DependencyInjection;
using DanpheEMR.Security;
using DanpheEMR.ServerModel;
using DanpheEMR.Services;
using DanpheEMR.Services.Billing;
using DanpheEMR.Services.ClaimManagement;
using DanpheEMR.Services.Dispensary;
using DanpheEMR.Services.DispensaryTransfer;
using DanpheEMR.Services.IMU;
using DanpheEMR.Services.Inventory.InventoryDonation;
using DanpheEMR.Services.LIS;
using DanpheEMR.Services.Maternity;
using DanpheEMR.Services.Medicare;
using DanpheEMR.Services.Pharmacy.Mapper.PurchaseOrder;
using DanpheEMR.Services.Pharmacy.PharmacyPO;
using DanpheEMR.Services.Pharmacy.Rack;
using DanpheEMR.Services.Pharmacy.SupplierLedger;
using DanpheEMR.Services.ProcessConfirmation;
using DanpheEMR.Services.QueueManagement;
using DanpheEMR.Services.SSF;
using DanpheEMR.Services.Utilities;
using DanpheEMR.Services.Vaccination;
using DanpheEMR.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace DanpheEMR
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        public IWebHostEnvironment CurrentEnvironment { get; set; }

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            CurrentEnvironment = env;
            
            //check audit is enable or disable             
            if ((!string.IsNullOrEmpty(Configuration["IsAuditEnable"])) && Convert.ToBoolean(Configuration["IsAuditEnable"]) == true)
            {
                string adminConstr = Configuration["ConnectionStringAdmin"];
                SqlConnectionStringBuilder conBuilderObj = new SqlConnectionStringBuilder(adminConstr);
                string encPwd = conBuilderObj.Password;
                if (!string.IsNullOrEmpty(encPwd))  //check is it encrypted or not
                {
                    string decPwd = DecryptPassword(encPwd);
                    conBuilderObj.Password = decPwd;
                }
                Audit.Core.Configuration.DataProvider = new SqlDataProvider()
                {
                    ConnectionString = conBuilderObj.ConnectionString,
                    Schema = "dbo",
                    TableName = "DanpheAudit",
                    IdColumnName = "AuditId",
                    JsonColumnName = "Data",
                    LastUpdatedDateColumnName = "LastUpdatedDate"
                };
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Allow synchronous IO for Kestrel & IIS
            services.Configure<KestrelServerOptions>(options => {
                options.AllowSynchronousIO = true;
            });
            services.Configure<IISServerOptions>(options => {
                options.AllowSynchronousIO = true;
            });

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.HttpOnly = true;
            });

            services.AddDanpheServices(Configuration);

            services.AddAutoMapper(typeof(MappingProfile));
            services.AddAutoMapper(typeof(PurchaseOrderMappingProfile));

            services.AddOptions();
            services.Configure<MyConfiguration>(Configuration);

            services.AddSwaggerAndJwtServices(Configuration);

            //For DanpheEMR Database connectionstring..
            string connStr = Configuration["Connectionstring"];
            SqlConnectionStringBuilder connStringBuilder = new SqlConnectionStringBuilder(connStr);
            string encPassword = connStringBuilder.Password;
            if (!string.IsNullOrEmpty(encPassword))
            {
                string decrypted = DecryptPassword(encPassword);
                connStringBuilder.Password = decrypted;
                Configuration["Connectionstring"] = connStringBuilder.ToString();
            }

            //For DanpheAdmin Database connectionstring.
            string connStrAdmin = Configuration["ConnectionStringAdmin"];
            SqlConnectionStringBuilder connStringBuilder2 = new SqlConnectionStringBuilder(connStrAdmin);
            string encPwd_Admin = connStringBuilder2.Password;
            if (!string.IsNullOrEmpty(encPwd_Admin))
            {
                string decPwd_Admin = DecryptPassword(encPwd_Admin);
                connStringBuilder2.Password = decPwd_Admin;
                Configuration["ConnectionStringAdmin"] = connStringBuilder2.ToString();
            }

            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                });

            string connString = Configuration["Connectionstring"];
            int cacheExpMins = Convert.ToInt32(Configuration["CacheExpirationMinutes"]);
            services.AddSingleton<DanpheCache>(new DanpheCache(connString, cacheExpMins));

            //used in LabReportExport to differntiate abnormal values based on this parameter
            bool highlightAbnormalLabResult = Convert.ToBoolean(Configuration["highlightAbnormalLabResult"]);

            //AuditTrail Enable and Disable 
            string auditValueStr = Configuration["IsAuditEnable"];
            if (!string.IsNullOrEmpty(auditValueStr))
            {
                bool IsAuditEnable = Convert.ToBoolean(auditValueStr);
                if (IsAuditEnable == false)
                {
                    Audit.Core.Configuration.AuditDisabled = true;
                }
            }
            else
            {
                Audit.Core.Configuration.AuditDisabled = true;
            }

            services.AddSingleton<RBAC>(new RBAC(connString, cacheExpMins));
            services.AddSingleton<NepaliDate>(new NepaliDate(connString));

            string storagePath = CurrentEnvironment.WebRootPath + "\\" + Configuration["FileStorageRelativeLocation"];
            services.AddSingleton<FileUploader>(new FileUploader(storagePath));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseMiddleware<RewindMiddleWare>();
            app.Use(async (context, next) =>
            {
                DanpheEMR.Utilities.HttpContextHelper.Current = context;
                await next();
            });
            app.UseSession();
            app.UseMiddleware<DanpheEMR.Utilities.SessionTokenMiddleware>();

            // Serve static files before routing
            app.UseFileServer();

            // set a home page
            DefaultFilesOptions defaultoptions = new DefaultFilesOptions();
            defaultoptions.DefaultFileNames.Clear();
            defaultoptions.DefaultFileNames.Add("UI/Main.html");
            app.UseDefaultFiles(defaultoptions);
            app.UseStaticFiles();

            string isDevEnv = Configuration["environment:isdevelopment"];

            if (bool.Parse(isDevEnv))
            {
                var provider = new PhysicalFileProvider(
                               Path.Combine(env.ContentRootPath, "wwwroot\\DanpheApp\\node_modules")
                           );
                var options = new FileServerOptions();
                options.RequestPath = "/node_modules";
                options.StaticFileOptions.FileProvider = provider;
                options.EnableDirectoryBrowsing = true;
                app.UseFileServer(options);

                //Use Swagger
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DanpheEMR-APIs-v1");
                });
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("DefaultRoute", "{controller}/{action}");
                endpoints.MapControllerRoute(name: "Default", pattern: "{controller}/{action}", defaults: new { controller = "Account", action = "Login" });
            });
        }

        private string DecryptPassword(string encryptedPwd)
        {
            string retVal = DanpheEMR.Security.RBAC.DecryptPassword(encryptedPwd);
            return retVal;
        }
    }
}
