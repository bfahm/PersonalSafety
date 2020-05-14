using System.Net.Mail;

namespace PersonalSafety.Services.Email
{
    public interface IEmailBodyBuilder
    {
        IEmailBodyBuilder AddIntroduction(string introduction);
        IEmailBodyBuilder AddFooter(string footer);
        IEmailBodyBuilder AddActivationLink(ActivationLink activationLink);
        IEmailBodyBuilder AddOtp(Otp otp);
        MailMessage Build();
    }
}
