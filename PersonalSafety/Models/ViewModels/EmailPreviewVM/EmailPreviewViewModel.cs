using System.Net.Mail;

namespace PersonalSafety.Models.ViewModels.EmailPreviewVM
{
    public class EmailPreviewViewModel
    {
        public EmailPreviewViewModel(string type, MailMessage mailMessage)
        {
            Type = type;
            Body = mailMessage.Body;
        }
        
        public string Type { get; set; }
        public string Body{ get; set; }
    }
}
