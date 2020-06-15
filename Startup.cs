using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailService.WebApi.Services;
using MailService.WebApi.Settings;
using MailService.WebApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MailService.WebApi
{
    public class Startup
    {
        public static string appSettingsFile = "appsettings.json";

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IConfiguration _configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (System.Diagnostics.Debugger.IsAttached)
                appSettingsFile = "appsettingsDevelopment.json";
            else
                appSettingsFile = "appsettings.json";

            // // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(appSettingsFile, false)
                .Build();

            // // configure strongly typed settings objects
            // var appSettingsSection = configuration.Get<MailSettings>();
            // services.Configure<MailSettings>(configuration.GetSection("MailSettings"));

            // var appSettings = configuration.Get<AppSettings>();
            // if we have no LDAP server location just exit
            //if (string.IsNullOrEmpty(appSettings.LdapLoc)) {
            //    Environment.Exit(9);    
            //}
                
            var mailsettingsection = configuration.GetSection("MailSettings");
            services.Configure<MailSettings>(mailsettingsection);
            var mailSettings = mailsettingsection.Get<MailSettings>();

            services.AddTransient<IMailService, Services.MailService>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSerilogRequestLogging();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
