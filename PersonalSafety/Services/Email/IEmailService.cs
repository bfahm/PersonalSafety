using System.Collections.Generic;

namespace PersonalSafety.Services
{
    public interface IEmailService
    {
        List<string> SendWelcomeEmail(string recepientEmail, string token, string otp, string redirectTo);
        List<string> SendWelcomeEmail(string recepientEmail);

        List<string> SendActivationEmail(string recepientEmail, string token, string otp, string redirectTo);
        List<string> SendPasswordResetEmail(string recepientEmail, string token, string redirectTo);
        List<string> SendPasswordChangedEmail(string recepientEmail);

        List<string> SendEmailChangedNewEmail(string oldEmail, string newEmail, string token, string redirectTo);
        List<string> SendEmailChangedOldEmail(string oldEmail, string newEmail);
    }
}
