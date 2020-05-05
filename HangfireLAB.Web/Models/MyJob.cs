using System;

namespace HangfireLAB.Web.Models
{
    public class MyJob
    {
        public static void 即時任務()
        {
            Console.WriteLine($"執行即時任務:{DateTime.Now:hh:mm}");
        }
    }
}