﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using PersonalSafety.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PersonalSafety.Contracts;
using Microsoft.Extensions.Logging;

namespace PersonalSafety.Extensions
{
    public static class ExceptionMiddlewareExtension
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILogger<Startup> logger)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null) 
                    {
                        logger.LogError(contextFeature.Error.StackTrace);

                        await context.Response.WriteAsync(new APIResponse<string>
                        {
                            Result = null,
                            Status = context.Response.StatusCode,
                            HasErrors = true,
                            Messages = new List<string> { contextFeature.Error.Message },
                        }.ToString()); ;
                    }
                });
            });
        }
    }
}
