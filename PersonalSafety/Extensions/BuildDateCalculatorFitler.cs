using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PersonalSafety.Extensions
{
    public class BuildDateCalculatorFitler : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;
            byte[] bytes = new byte[2048];
            using (FileStream file = new FileStream(Assembly.GetEntryAssembly().Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                file.Read(bytes, 0, bytes.Length);
            }
            Int32 headerPos = BitConverter.ToInt32(bytes, peHeaderOffset);
            Int32 secondsSince1970 = BitConverter.ToInt32(bytes, headerPos + linkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime dateTimeUTC = dt.AddSeconds(secondsSince1970);
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUTC, TimeZoneInfo.Local);
            var result = localTime.ToString("dd-MMM-yyyy hh:mm:sstt") + " " + TimeZoneInfo.Local.Id;

            context.HttpContext.Response.Cookies.Append("LastBuildDateServerSide", result);
            context.HttpContext.Response.Cookies.Append("LastBuildDateNotifyServerSide", true.ToString());
        }

        public void OnActionExecuting(ActionExecutingContext context){ }
    }
}
