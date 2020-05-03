using Microsoft.AspNetCore.Builder;
using System.Linq;

namespace PersonalSafety.Extensions
{
    public static class HeaderInjectorExtension
    {
        public static void UseCustomHeaderInjector(this IApplicationBuilder app)
        {
            //Allow headers required by SignalR, order is important
            app.Use((context, next) =>
            {
                context.Response.Headers["Access-Control-Allow-Origin"] = context.Request.Headers.Where(h => h.Key == "Origin").FirstOrDefault().Value.ToString();
                context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
                context.Response.Headers["Access-Control-Allow-Methods"] = "*";
                context.Response.Headers["Access-Control-Allow-Headers"] = "Authorization, X-Requested-With, Content-Type";

                return next.Invoke();
            });
        }
    }
}
