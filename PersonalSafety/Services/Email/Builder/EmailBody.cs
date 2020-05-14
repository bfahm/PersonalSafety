using System;
using System.Collections.Specialized;
using System.Text;

namespace PersonalSafety.Services.Email
{
    public class EmailBody
    {   
        public string Introduction { get; set; }
        public ActivationSection ActivationSection { get; set; }
        public OtpSection OtpSection { get; set; }
        public string Footer { get; set; }

        public override string ToString()
        {
            var body = new StringBuilder();

            if (string.IsNullOrEmpty(Introduction))
                body.AppendLine(Introduction);

            if (ActivationSection != null)
                body.Append(ActivationSection.ToString());

            if (OtpSection != null)
                body.Append(OtpSection.ToString());

            if (string.IsNullOrEmpty(Footer))
                body.AppendLine(Footer);

            return body.ToString();
        }
    }

    public class ActivationSection
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string AppBaseUrl { get; set; }
        public string RedirectEndpoint { get; set; }
        public string Token { get; set; }
        public NameValueCollection QueryParams { get; set; }

        public override string ToString()
        {
            var linkString = new StringBuilder();

            UriBuilder activationLinkBuilder = new UriBuilder(AppBaseUrl + "/" + RedirectEndpoint)
            {
                Query = QueryParams.ToString()
            };

            linkString.AppendLine("</br>");
            linkString.AppendLine($"<p> {Description} </p>");
            linkString.AppendLine($"<a href= \"{activationLinkBuilder}\" >{Title}</a>");
            linkString.AppendLine("</br>");

            return linkString.ToString();
        }
    }

    public class OtpSection
    {
        public string OtpCode { get; set; }
        public string OtpDescription { get; set; }

        public override string ToString()
        {
            var otpSectionString = new StringBuilder();

            otpSectionString.AppendLine("</br>");
            otpSectionString.AppendLine($"<p> {OtpDescription} </p>");
            otpSectionString.AppendLine("<h6>" + OtpCode + "</h6>");
            
            return otpSectionString.ToString();
        }
    }
}
