using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GradDisplayMain.Models;
using GradDisplayMain.Services;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GradDisplayMain
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            // save to the disk
            try
            {
                var mainFilename = env.WebRootPath + "\\" + "resources" + "\\" + "log" + "\\" + "log.txt";
                using (var fs = new System.IO.FileStream(mainFilename, System.IO.FileMode.OpenOrCreate))
                {
                    if (fs.Length > 0)
                    {
                        fs.Seek(0, System.IO.SeekOrigin.End);
                    }

                    var respectedTime = DateTime.Now.ToString();

                    using (var stream = new System.IO.StreamWriter(fs))
                    {
                        stream.WriteLine(respectedTime + " ----------------------------------- Started -----------------------------------");

                        stream.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write("Error: " + e.Message);
                // ignore any io error
            }

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Set the Context Connectionstring
            ApplicationDbContext.ConnectionString = Configuration["Data:CeremonialProfile:ConnectionString"];
            AdministratorRequirement.ConnectionString = Configuration["Data:CeremonialProfile:ConnectionString"];
            GradConfigDbContext.ConnectionString = Configuration["Data:CeremonialProfile:ConnectionString"];
            GraduateDbContext.ConnectionString = Configuration["Data:CeremonialProfile:ConnectionString"];
            TelepromptDbContext.ConnectionString = Configuration["Data:CeremonialProfile:ConnectionString"];
            QueueDbContext.ConnectionString = Configuration["Data:CeremonialProfile:ConnectionString"];


            // Add framework services.
            services.AddEntityFramework()
                .AddEntityFrameworkSqlServer()
                .AddDbContext<ApplicationDbContext>()
                .AddDbContext<GraduateDbContext>()
                .AddDbContext<TelepromptDbContext>()
                .AddDbContext<QueueDbContext>()
                .AddDbContext<GradConfigDbContext>();

            /* 
                 .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration["Data:CeremonialProfile:ConnectionString"]))
                .AddDbContext<QueueDbContext>(options =>
                    options.UseSqlServer(Configuration["Data:CeremonialProfile:ConnectionString"]))
                .AddDbContext<TelepromptDbContext>(options =>
                    options.UseSqlServer(Configuration["Data:CeremonialProfile:ConnectionString"]))
                .AddDbContext<GraduateDbContext>(options =>
                    options.UseSqlServer(Configuration["Data:CeremonialProfile:ConnectionString"]))
                .AddDbContext<GradConfigDbContext>(options =>
                    options.UseSqlServer(Configuration["Data:CeremonialProfile:ConnectionString"])); */



            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdministratorRequirement", policy => policy.AddRequirements(new AdministratorRequirement()));
            });

            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    //template: "{controller=Home}/{action=Index}/{id?}");
                    template: "{controller=Graduate}/{action=Index}/{id?}"
                );
        });
        }
    }
}
