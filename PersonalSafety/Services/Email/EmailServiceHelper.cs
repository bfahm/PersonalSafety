using PersonalSafety.Services.Otp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace PersonalSafety.Services
{
    public class EmailServiceHelper
    {
        public string RecipientMail { get; set; }
        public string Token { get; set; }
        public string BaseUrl { get; set; }
        public string ActivationLink { get; set; }
        public string OTP { get; set; }

        public EmailServiceHelper(string recipientMail, string token, string otp, string baseUrl, string endpoint)
        {
            RecipientMail = recipientMail;
            Token = token;
            BaseUrl = baseUrl;
            OTP = otp;
            ActivationLink = BaseUrl + "/" + endpoint + "?email=" + RecipientMail + "&token=" + Uri.EscapeDataString(Token);
        }

        public List<string> SendEmail()
        {
            try
            {
                using MailMessage mail = new MailMessage
                {
                    From = new MailAddress("personalsafety20@gmail.com"),
                    Subject = "Confirmation for Personal Safety",
                    Body = GenerateMailBody(),
                    IsBodyHtml = true
                };
                
                mail.To.Add(RecipientMail);

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("personalsafety20@gmail.com", "Test@123");
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(mail);
                }
                return new List<string>();
            }catch(Exception ex)
            {
                List<string> result = new List<string>
                {
                    ex.Message, 
                    ex.InnerException?.ToString(),
                    ex.StackTrace
                };
                return result;
            }
            
        }

        private string GenerateMailBody()
        {
            var body = new StringBuilder();
            body.AppendFormat("Hello, {0}\n", RecipientMail);
            body.AppendLine(@"It appears that you requested a confirmation request for your PersonalSafety Account, ");
            body.AppendLine("<a href=\" " + ActivationLink + " \">tap here to continue.</a>");

            if(OTP != null)
            {
                body.AppendLine("You can also validate your request using this OTP which is valid for " + OTPHelper.validFor + " seconds.");
                body.AppendLine("<b>" + OTP + "</b>");
            }
            return body.ToString();
        }
    }
}
