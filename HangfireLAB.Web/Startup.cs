using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.MemoryStorage;
using HangfireLAB.Web.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HangfireLAB.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddSession();
            services.AddHangfire(config => {
                // 設定使用MemoryStorage
                config.UseMemoryStorage();
                // 支援Console(選用)
                config.UseConsole();
            });

            // DI
            services.AddSingleton(s => new RecurringJobManager());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IBackgroundJobClient backgroundJobs)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            
            // 加入Hangfire伺服器
            app.UseHangfireServer();

            // 加入Hangfire控制面板
            app.UseHangfireDashboard(
                pathMatch: "/hangfire",
                options: new DashboardOptions() { // 使用自訂的認證過濾器
                    Authorization = new[] { new MyAuthFilter() }
                }
            );
            
            // add enqueue job demo
            backgroundJobs.Enqueue(() => Console.WriteLine("Hello world from Hangfire!"));
            // add cron job demo
            RecurringJob.AddOrUpdate("some-id", () => Console.WriteLine(DateTime.Now), Cron.Minutely);

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}