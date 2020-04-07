using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using PersonalSafety.Controllers.API;
using PersonalSafety.Options;
using PersonalSafety.Services;

namespace PersonalSafety.Extensions
{
    public class GithubUpdateCheckerFilter : IActionFilter
    {
        private readonly AppSettings _appSettings;
        private readonly IGithubUpdateService _githubUpdateService;
        private readonly IWebHostEnvironment _environment;
        private const string CookieLastCheckedKey = "update_service_last_checked";
        private const string CookieIsUpdatedKey = "update_service_is_updated";
        private const string QueryNeedsUpdate = "needsUpdate";

        public GithubUpdateCheckerFilter(AppSettings appSettings, IGithubUpdateService githubUpdateService, IWebHostEnvironment env)
        {
            _appSettings = appSettings;
            _githubUpdateService = githubUpdateService;
            _environment = env;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Escape the logic if in development environment.
            if (_environment.IsDevelopment()) return;

            DateTime.TryParse(context.HttpContext.Request.Cookies[CookieLastCheckedKey], out var cookieLastCheckedValue);
            bool.TryParse(context.HttpContext.Request.Cookies[CookieIsUpdatedKey], out var cookieIsUpdatedValue);
            var containsNeedsUpdateQuery = context.HttpContext.Request.Query.ContainsKey(QueryNeedsUpdate);
            
            var parametersToAdd = new Dictionary<string, string> { { QueryNeedsUpdate, "true" } };
            var baseUri = _appSettings.AppBaseUrlView;
            var newUri = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(baseUri, parametersToAdd);
            
            // If already user has the notification, do nothing
            if (containsNeedsUpdateQuery) return;

            // When time comes, check for updates (avoid redundancy)
            if (DateTime.Now.Subtract(cookieLastCheckedValue).TotalHours >= 1)
            {
                var isUpToDate = _githubUpdateService.IsApplicationUpToDate().Result;
                context.HttpContext.Response.Cookies.Append(CookieLastCheckedKey, DateTime.Now.ToString());
                if (!isUpToDate)
                {
                    context.HttpContext.Response.Cookies.Append(CookieIsUpdatedKey, "false");
                    context.HttpContext.Response.Redirect(newUri);
                    return;
                }

                context.HttpContext.Response.Cookies.Append(CookieIsUpdatedKey, "true");
                context.HttpContext.Response.Redirect(context.HttpContext.Request.Path);
                return;
            }

            // If time hasn't passed and user had removed the query, check the cookie
            if (!cookieIsUpdatedValue)
            {
                context.HttpContext.Response.Redirect(newUri);
            }
        }

        public void OnActionExecuting(ActionExecutingContext context){}
    }
}
