using PersonalSafety.Options;
using PersonalSafety.Services.Email;
using PersonalSafety.Services.Email.Builder;
using PersonalSafety.Services.Otp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace PersonalSafety.Services
{
    public class EmailService : IEmailService
    {
        private readonly AppSettings _appSettings;
        private const string _from = "personalsafety20@gmail.com";
        private const string _password = "Test@123";
        private const string _brand = "Personal Safety";

        public EmailService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public List<string> SendWelcomeEmail(string recepientEmail, string token, string otp, string redirectTo)
        {
            EmailBuilder emailBuilder = new EmailBuilder(_from, recepientEmail, $"Welcome to {_brand}");

            StringBuilder intro = new StringBuilder(EmailSnippets.BuildHi(recepientEmail));
            intro.AppendLine($"<p> Welcome to {_brand}. We’re thrilled to see you here! </p>");
            intro.AppendLine($"<p> We’re confident that {_brand} will help you find safety in your neighborhood, city, country, and anywhere you go. </p>");

            MailMessage mailMessage = emailBuilder.AddIntroduction(intro.ToString())
                    .AddActivationLink(new ActivationSection
                    {
                        AppBaseUrl = _appSettings.AppBaseUrlView,
                        Token = token,
                        QueryParams = EmailSnippets.BuildQuery(recepientEmail, token),
                        RedirectEndpoint = redirectTo,
                        Title = "Activate Here",
                        Description = "Initially, kindly activate your account below by clicking 'Activate Here' to be able to use our services to its extent."
                    })
                    .AddOtp(new OtpSection
                    {
                        OtpCode = otp,
                        OtpDescription = "You can also validate your request using this OTP which is valid for " + OTPHelper.validFor + " seconds."
                    })
                    .AddFooter(EmailSnippets.BuildFooter(_brand))
                    .Build();

            return TrySend(mailMessage);
        }

        public List<string> SendWelcomeEmail(string recepientEmail)
        {
            EmailBuilder emailBuilder = new EmailBuilder(_from, recepientEmail, $"Welcome to {_brand}");

            StringBuilder intro = new StringBuilder(EmailSnippets.BuildHi(recepientEmail));
            intro.AppendLine($"<p> Welcome to {_brand}. We’re thrilled to see you here! </p>");
            intro.AppendLine($"<p> We’re confident that {_brand} will help you find safety in your neighborhood, city, country, and anywhere you go. </p>");

            MailMessage mailMessage = emailBuilder.AddIntroduction(intro.ToString())
                    .AddFooter(EmailSnippets.BuildFooter(_brand))
                    .Build();

            return TrySend(mailMessage);
        }

        public List<string> SendActivationEmail(string recepientEmail, string token, string otp, string redirectTo)
        {
            EmailBuilder emailBuilder = new EmailBuilder(_from, recepientEmail, "Activate your Account");

            StringBuilder intro = new StringBuilder(EmailSnippets.BuildHi(recepientEmail));
            intro.AppendLine($"<p> You have recently requested to activate your account. If you intended to do so, proceed reading, else, discard this email safely.</p>");

            MailMessage mailMessage = emailBuilder.AddIntroduction(intro.ToString())
                    .AddActivationLink(new ActivationSection
                    {
                        AppBaseUrl = _appSettings.AppBaseUrlView,
                        Token = token,
                        QueryParams = EmailSnippets.BuildQuery(recepientEmail, token),
                        RedirectEndpoint = redirectTo,
                        Title = "Activate Here",
                        Description = "Activate your account below by clicking 'Activate Here'. The link will take you to the confirmation page and will confirm your account automatically."
                    })
                    .AddOtp(new OtpSection
                    {
                        OtpCode = otp,
                        OtpDescription = "You can also validate your request using this OTP which is valid for " + OTPHelper.validFor + " seconds."
                    })
                    .AddFooter(EmailSnippets.BuildFooter(_brand))
                    .Build();

            return TrySend(mailMessage);
        }

        public List<string> SendPasswordResetEmail(string recepientEmail, string token, string redirectTo)
        {
            EmailBuilder emailBuilder = new EmailBuilder(_from, recepientEmail, "Password Reset");

            StringBuilder intro = new StringBuilder(EmailSnippets.BuildHi(recepientEmail));
            intro.AppendLine($"<p> You have recently requested to reset your password. If you intended to do so, proceed reading, else, secure your account by changing the password..</p>");

            MailMessage mailMessage = emailBuilder.AddIntroduction(intro.ToString())
                    .AddActivationLink(new ActivationSection
                    {
                        AppBaseUrl = _appSettings.AppBaseUrlView,
                        Token = token,
                        QueryParams = EmailSnippets.BuildQuery(recepientEmail, token),
                        RedirectEndpoint = redirectTo,
                        Title = "Reset Password",
                        Description = "Click the following link, then fill the form with your new password."
                    })
                    .AddFooter(EmailSnippets.BuildFooter(_brand))
                    .Build();

            return TrySend(mailMessage);
        }

        public List<string> SendPasswordChangedEmail(string recepientEmail)
        {
            EmailBuilder emailBuilder = new EmailBuilder(_from, recepientEmail, "Password Changed Recently");

            StringBuilder intro = new StringBuilder(EmailSnippets.BuildHi(recepientEmail));
            intro.AppendLine($"<p> There has been recently a change in your password. If you intended to do so, kindly discard this email, else, secure your account by changing the password..</p>");

            MailMessage mailMessage = emailBuilder.AddIntroduction(intro.ToString())
                    .AddFooter(EmailSnippets.BuildFooter(_brand))
                    .Build();

            return TrySend(mailMessage);
        }

        public List<string> SendEmailChangedNewEmail(string oldEmail, string newEmail, string token, string redirectTo)
        {
            EmailBuilder emailBuilder = new EmailBuilder(_from, newEmail, "Activate your new Email");

            StringBuilder intro = new StringBuilder(EmailSnippets.BuildHi(newEmail));
            intro.AppendLine($"<p> There has been recently a change in your account email. If you intended to do so, kindly proceed with the following instructions.</p>");

            MailMessage mailMessage = emailBuilder.AddIntroduction(intro.ToString())
                    .AddActivationLink(new ActivationSection
                    {
                        AppBaseUrl = _appSettings.AppBaseUrlView,
                        Token = token,
                        QueryParams = EmailSnippets.BuildQuery(oldEmail, newEmail, token),
                        RedirectEndpoint = redirectTo,
                        Title = "Transfer to this Email",
                        Description = "Tap the below link to proceed with transfering your email configuration from the old address to the new one.."
                    })
                    .AddFooter(EmailSnippets.BuildFooter(_brand))
                    .Build();

            return TrySend(mailMessage);
        }

        public List<string> SendEmailChangedOldEmail(string oldEmail, string newEmail)
        {
            EmailBuilder emailBuilder = new EmailBuilder(_from, oldEmail, "Email Address Changed");

            StringBuilder intro = new StringBuilder(EmailSnippets.BuildHi(oldEmail));
            intro.AppendLine($"<p> There has been recently a change in your account. </p>");
            intro.AppendLine($"<p> Your email configuration was changed from {oldEmail} to {newEmail}. </p>");
            intro.AppendLine($"<p> If you did not intend to do so, contact us as soon as possible. </p>");

            MailMessage mailMessage = emailBuilder.AddIntroduction(intro.ToString())
                    .AddFooter(EmailSnippets.BuildFooter(_brand))
                    .Build();

            return TrySend(mailMessage);
        }


        private List<string> TrySend(MailMessage mailMessage)
        {
            try
            {
                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(_from, _password);
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(mailMessage);
                }

                return new List<string>();
            }
            catch (Exception ex)
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
    }
}
