using System;
using Hangfire;
using HangfireLAB.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace HangfireLAB.Web.Controllers
{
    public class JobController : Controller
    {
        public JsonResult AddJob1([FromServices]RecurringJobManager jobManager)
        {
            string id = Guid.NewGuid().ToString("N");
            jobManager.AddOrUpdate(id,()=>MyJob.即時任務(),Cron.Minutely);

            return Json(new {success = true, id});
        }
    }
}