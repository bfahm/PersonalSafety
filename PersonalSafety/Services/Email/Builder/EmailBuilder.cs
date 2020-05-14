using System.Net.Mail;

namespace PersonalSafety.Services.Email.Builder
{
    public class EmailBuilder : IEmailBodyBuilder
    {
        private EmailBody _emailBody;
        private readonly string _recepientEmail;
        private readonly string _fromEntity;
        private readonly string _subject;

        public EmailBuilder(string fromEntity, string recepientEmail, string subject)
        {
            _emailBody = new EmailBody();
            _recepientEmail = recepientEmail;
            _fromEntity = fromEntity;
            _subject = subject;
        }

        public IEmailBodyBuilder AddIntroduction(string introduction)
        {
            _emailBody.Introduction = introduction;
            return this;
        }

        public IEmailBodyBuilder AddFooter(string footer)
        {
            _emailBody.Footer = footer;
            return this;
        }

        public IEmailBodyBuilder AddActivationLink(ActivationLink activationLink)
        {
            _emailBody.ActivationLink = activationLink;
            return this;
        }

        public IEmailBodyBuilder AddOtp(Otp otp)
        {
            _emailBody.Otp = otp;
            return this;
        }

        public MailMessage Build()
        {
            MailMessage mail = new MailMessage
            {
                From = new MailAddress(_fromEntity),
                Subject = _subject,
                Body = _emailBody.ToString(), // The toString function adds up all the fields into a one large string variable
                IsBodyHtml = true
            };
            mail.To.Add(_recepientEmail);

            return mail;
        }
    }
}
