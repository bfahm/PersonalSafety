using System;
using System.Collections.Specialized;
using System.Text;

namespace PersonalSafety.Services.Email
{
    public class EmailBody
    {   
        public string Introduction { get; set; }
        public string Footer { get; set; }
        public ActivationLink ActivationLink { get; set; }
        public Otp Otp { get; set; }

        public override string ToString()
        {
            var body = new StringBuilder();

            if (string.IsNullOrEmpty(Introduction) || string.IsNullOrEmpty(Footer))
                throw new InvalidEmailBodyException("Email Body could not be compiled. Fields are not complete.");

            body.AppendLine(Introduction);

            if (ActivationLink != null)
            {
                UriBuilder activationLinkBuilder = new UriBuilder(ActivationLink.AppBaseUrl + "/" + ActivationLink.RedirectEndpoint)
                {
                    Query = ActivationLink.QueryParams.ToString()
                };

                body.AppendLine("</br>");
                body.AppendLine($"<p> {ActivationLink.Description} </p>");
                body.AppendLine($"<a href= \"{activationLinkBuilder}\" >{ActivationLink.Title}</a>");
                body.AppendLine("</br>");
            }

            if (Otp != null)
            {
                body.AppendLine("</br>");
                body.AppendLine($"<p> {Otp.OtpDescription} </p>");
                body.AppendLine("<h6>" + Otp.OtpCode + "</h6>");
            }

            body.AppendLine(Footer);

            return body.ToString();
        }
    }

    public class ActivationLink
    {
        public string AppBaseUrl { get; set; }
        public string RedirectEndpoint { get; set; }
        public string Token { get; set; }
        public NameValueCollection QueryParams { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class Otp
    {
        public string OtpCode { get; set; }
        public string OtpDescription { get; set; }
    }

    [Serializable()]
    public class InvalidEmailBodyException : Exception
    {
        public InvalidEmailBodyException() : base() { }
        public InvalidEmailBodyException(string message) : base(message) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client.
        protected InvalidEmailBodyException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
