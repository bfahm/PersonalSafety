using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace PersonalSafety.Helpers
{
    public class EmailHelper
    {
        public EmailHelper(string recepientMail, string token)
        {
            //SmtpClient client = new SmtpClient("smtp.gmail.com");
            //client.UseDefaultCredentials = true;
            //client.Credentials = new NetworkCredential("personalsafety20@gmail.com", "Test@123");
            //client.Port = 587;
            //client.EnableSsl = true;

            //MailMessage mailMessage = new MailMessage();
            //mailMessage.From = new MailAddress("personalsafety20@gmail.com");
            ////mailMessage.To.Add(recepientMail);
            //mailMessage.To.Add("temagi9081@xhyemail.com");
            //mailMessage.Body = token;
            //mailMessage.Subject = "Forgot Password";
            //mailMessage.IsBodyHtml = false;
            //client.Send(mailMessage);

            Otherway();
        }

        public void Otherway()
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("personalsafety20@gmail.com");
                mail.To.Add("temagi9081@xhyemail.com");
                mail.Subject = "Hello World";
                mail.Body = "<h1>Hello</h1>";
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
            }
        }
    }
}
