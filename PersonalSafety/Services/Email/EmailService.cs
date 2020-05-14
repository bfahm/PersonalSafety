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

        public EmailService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        #region API CALLS

        public List<string> SendWelcomeEmail(string recepientEmail, string token, string otp, string redirectTo)
        {
            return TrySend(RawWelcomeEmail(recepientEmail, token, otp, redirectTo));
        }

        public List<string> SendWelcomeEmail(string recepientEmail)
        {
            return TrySend(RawWelcomeEmail(recepientEmail));
        }

        public List<string> SendActivationEmail(string recepientEmail, string token, string otp, string redirectTo)
        {
            return TrySend(RawActivationEmail(recepientEmail, token, otp, redirectTo));
        }

        public List<string> SendPasswordResetEmail(string recepientEmail, string token, string redirectTo)
        {
            return TrySend(RawPasswordResetEmail(recepientEmail, token, redirectTo));
        }

        public List<string> SendPasswordChangedEmail(string recepientEmail)
        {
            return TrySend(RawPasswordChangedEmail(recepientEmail));
        }

        public List<string> SendEmailChangedNewEmail(string oldEmail, string newEmail, string token, string redirectTo)
        {
            return TrySend(RawEmailChangedNewEmail(oldEmail, newEmail, token, redirectTo));
        }

        public List<string> SendEmailChangedOldEmail(string oldEmail, string newEmail)
        {
            return TrySend(RawEmailChangedOldEmail(oldEmail,newEmail));
        }


        private List<string> TrySend(MailMessage mailMessage)
        {
            try
            {
                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(_appSettings.EmailsFrom, _appSettings.SMTPPassword);
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

        #endregion

        #region VIEW CALLS

        public MailMessage RawWelcomeEmail(string recepientEmail, string token, string otp, string redirectTo)
        {
            EmailBuilder emailBuilder = new EmailBuilder(_appSettings.EmailsFrom, recepientEmail, $"Welcome to {_appSettings.BrandName}");

            StringBuilder intro = new StringBuilder(EmailSnippets.BuildHi(recepientEmail));
            intro.AppendLine($"<p> Welcome to {_appSettings.BrandName}. We’re thrilled to see you here! </p>");
            intro.AppendLine($"<p> We’re confident that {_appSettings.BrandName} will help you find safety in your neighborhood, city, country, and anywhere you go. </p>");

             return emailBuilder.AddIntroduction(intro.ToString())
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
                    .AddFooter(EmailSnippets.BuildFooter(_appSettings.BrandName))
                    .Build();
        }

        public MailMessage RawWelcomeEmail(string recepientEmail)
        {
            EmailBuilder emailBuilder = new EmailBuilder(_appSettings.EmailsFrom, recepientEmail, $"Welcome to {_appSettings.BrandName}");

            StringBuilder intro = new StringBuilder(EmailSnippets.BuildHi(recepientEmail));
            intro.AppendLine($"<p> Welcome to {_appSettings.BrandName}. We’re thrilled to see you here! </p>");
            intro.AppendLine($"<p> We’re confident that {_appSettings.BrandName} will help you find safety in your neighborhood, city, country, and anywhere you go. </p>");

            return emailBuilder.AddIntroduction(intro.ToString())
                    .AddFooter(EmailSnippets.BuildFooter(_appSettings.BrandName))
                    .Build();
        }

        public MailMessage RawActivationEmail(string recepientEmail, string token, string otp, string redirectTo)
        {
            EmailBuilder emailBuilder = new EmailBuilder(_appSettings.EmailsFrom, recepientEmail, "Activate your Account");

            StringBuilder intro = new StringBuilder(EmailSnippets.BuildHi(recepientEmail));
            intro.AppendLine($"<p> You have recently requested to activate your account. If you intended to do so, proceed reading, else, discard this email safely.</p>");

            return emailBuilder.AddIntroduction(intro.ToString())
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
                    .AddFooter(EmailSnippets.BuildFooter(_appSettings.BrandName))
                    .Build();
        }

        public MailMessage RawPasswordResetEmail(string recepientEmail, string token, string redirectTo)
        {
            EmailBuilder emailBuilder = new EmailBuilder(_appSettings.EmailsFrom, recepientEmail, "Password Reset");

            StringBuilder intro = new StringBuilder(EmailSnippets.BuildHi(recepientEmail));
            intro.AppendLine($"<p> You have recently requested to reset your password. If you intended to do so, proceed reading, else, secure your account by changing the password..</p>");

            return emailBuilder.AddIntroduction(intro.ToString())
                    .AddActivationLink(new ActivationSection
                    {
                        AppBaseUrl = _appSettings.AppBaseUrlView,
                        Token = token,
                        QueryParams = EmailSnippets.BuildQuery(recepientEmail, token),
                        RedirectEndpoint = redirectTo,
                        Title = "Reset Password",
                        Description = "Click the following link, then fill the form with your new password."
                    })
                    .AddFooter(EmailSnippets.BuildFooter(_appSettings.BrandName))
                    .Build();
        }

        public MailMessage RawPasswordChangedEmail(string recepientEmail)
        {
            EmailBuilder emailBuilder = new EmailBuilder(_appSettings.EmailsFrom, recepientEmail, "Password Changed Recently");

            StringBuilder intro = new StringBuilder(EmailSnippets.BuildHi(recepientEmail));
            intro.AppendLine($"<p> There has been recently a change in your password. If you intended to do so, kindly discard this email, else, secure your account by changing the password..</p>");

            return emailBuilder.AddIntroduction(intro.ToString())
                    .AddFooter(EmailSnippets.BuildFooter(_appSettings.BrandName))
                    .Build();
        }

        public MailMessage RawEmailChangedNewEmail(string oldEmail, string newEmail, string token, string redirectTo)
        {
            EmailBuilder emailBuilder = new EmailBuilder(_appSettings.EmailsFrom, newEmail, "Activate your new email");

            StringBuilder intro = new StringBuilder(EmailSnippets.BuildHi(newEmail));
            intro.AppendLine($"<p> There has been recently a change in your account email. If you intended to do so, kindly proceed with the following instructions.</p>");

            return  emailBuilder.AddIntroduction(intro.ToString())
                    .AddActivationLink(new ActivationSection
                    {
                        AppBaseUrl = _appSettings.AppBaseUrlView,
                        Token = token,
                        QueryParams = EmailSnippets.BuildQuery(oldEmail, newEmail, token),
                        RedirectEndpoint = redirectTo,
                        Title = "Transfer to this Email",
                        Description = "Tap the below link to proceed with transfering your email configuration from the old address to the new one.."
                    })
                    .AddFooter(EmailSnippets.BuildFooter(_appSettings.BrandName))
                    .Build();
        }

        public MailMessage RawEmailChangedOldEmail(string oldEmail, string newEmail)
        {
            EmailBuilder emailBuilder = new EmailBuilder(_appSettings.EmailsFrom, oldEmail, "Email Address Changed");

            StringBuilder intro = new StringBuilder(EmailSnippets.BuildHi(oldEmail));
            intro.AppendLine($"<p> There has been recently a change in your account. </p>");
            intro.AppendLine($"<p> Your email configuration was changed from {oldEmail} to {newEmail}. </p>");
            intro.AppendLine($"<p> If you did not intend to do so, contact us as soon as possible. </p>");

            return emailBuilder.AddIntroduction(intro.ToString())
                    .AddFooter(EmailSnippets.BuildFooter(_appSettings.BrandName))
                    .Build();
        }

        #endregion
    }
}
