using Microsoft.AspNetCore.Builder;
using PersonalSafety.Options;

namespace PersonalSafety.Extensions
{
    /// <summary>
    /// This extension adds a custom logic to restrict access toresources under /Uploads in the site root
    /// for under-privelidged users.
    /// </summary>
    public static class CustomStaticFileExtension
    {
        public static void UseCustomStaticFile(this IApplicationBuilder app, AppSettings appSettings)
        {
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = (context) =>
                {
                    var requestedPath = context.Context.Request.Path;
                    if (!context.Context.User.Identity.IsAuthenticated && requestedPath.StartsWithSegments($"/{appSettings.AttachmentsLocation}"))
                    {
                        context.Context.Response.StatusCode = 401;
                        context.Context.Response.Redirect("/Error/401");
                    }
                }
            });
        }
    }
}
