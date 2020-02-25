using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace PersonalSafety.Helpers
{
    public class EmailHelper
    {
        public string RecepientMail { get; set; }
        public string Token { get; set; }
        public string BaseUrl { get; set; }
        public string ActivationLink { get; set; }

        public EmailHelper(string recepientMail, string token, string baseUrl)
        {
            RecepientMail = recepientMail;
            Token = token;
            BaseUrl = baseUrl;
            ActivationLink = BaseUrl + "/Home/ForgotPassword?email=" + RecepientMail + "&token=" + Uri.EscapeDataString(Token);
        }

        public List<string> SendEmail()
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("personalsafety20@gmail.com");
                    mail.To.Add(RecepientMail);
                    mail.Subject = "Confirmation for Personal Safety";

                    mail.Body = GenerateMailBody();
                    mail.IsBodyHtml = true;

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
                    return null;
                }
            }catch(Exception ex)
            {
                List<string> result = new List<string>();
                result.Add(ex.Message);
                result.Add(ex.InnerException.ToString());
                return result;
            }
            
        }

        private string GenerateMailBody()
        {
            var body = new StringBuilder();
            body.AppendFormat("Hello, {0}\n", RecepientMail);
            body.AppendLine(@"It appears that you requested a confirmation request for your PersonalSafety Account, ");
            body.AppendLine("<a href=\" " + ActivationLink + " \">tap here to continue.</a>");

            return body.ToString();
        }
    }
}
