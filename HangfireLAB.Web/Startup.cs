using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard.Management;
using Hangfire.MemoryStorage;
using HangfireLAB.Web.Filters;
using HangfireLAB.Web.Models;
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
            services.AddHangfire(config =>
            {
                // 使用 memory storage
                config.UseMemoryStorage();
                // 使用 console
                config.UseConsole();
                config.UseManagementPages(c => c.AddJobs(GetModuleTypes()));

                config.UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.ServerCount) //服务器数量
                    .UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.RecurringJobCount) //任务数量
                    .UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.RetriesCount) //重试次数
                    //.UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.EnqueuedCountOrNull)//队列数量
                    //.UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.FailedCountOrNull)//失败数量
                    .UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.EnqueuedAndQueueCount) //队列数量
                    .UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.ScheduledCount) //计划任务数量
                    .UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.ProcessingCount) //执行中的任务数量
                    .UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.SucceededCount) //成功作业数量
                    .UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.FailedCount) //失败数量
                    .UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.DeletedCount) //删除数量
                    .UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.AwaitingCount); //等待任务数量
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

            // 加入 hangfire 的server實體，可重複此行加入多個實體
            app.UseHangfireServer();

            // 加入 hangfire 控制面板
            app.UseHangfireDashboard(
                pathMatch: "/hangfire",
                options: new DashboardOptions() { 
                    // 進入 hangfire dashboard 的授權規則 (有沒有權限看 dashboard 就看這個邏輯怎麼設定)
                    Authorization = new[] { new MyAuthFilter() }
                }
            );

            // add enqueue job demo
            var jobId = backgroundJobs.Enqueue(() => Console.WriteLine("Fire-and-forget!"));

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

        public static Type[] GetModuleTypes()
        {
            var moduleTypes = GetApplicationTypes();
            return moduleTypes;
        }
        private static Type[] GetApplicationTypes()
        {
            var assemblies = new[] { typeof(MyJob).Assembly };
            var types = assemblies.SelectMany(f => f.GetTypes()).ToArray();
            return types;
        }
    }
}