using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Options
{
    public class AppSettings
    {
        public string AppBaseUrl { get; set; }
        public string AppBaseUrlView { get; set; }
        public string AppVersion { get; set; }
        public string AttachmentsLocation { get; set; }
        public string EmailsFrom { get; set; }
        public string SMTPPassword { get; set; }
        public string BrandName { get; set; }
    }
}
