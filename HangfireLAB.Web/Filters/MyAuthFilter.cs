using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace HangfireLAB.Web.Filters
{
    public class MyAuthFilter : IDashboardAuthorizationFilter {
        public bool Authorize([NotNull] DashboardContext context)
        {
            var id = context.GetHttpContext().Session.GetString("Id");
            if (id == "art") return true;

            return false;
        }
    }
}