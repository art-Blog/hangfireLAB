using System;
using Hangfire.Console;
using Hangfire.Server;

namespace HangfireLAB.Web.Models
{
    public class MyJob
    {
        public static void 即時任務(PerformContext context)
        {
            context.WriteLine($"執行即時任務:{DateTime.Now:hh:mm}");
        }
    }
}