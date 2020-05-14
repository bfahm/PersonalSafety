using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Models.ViewModels.EmailPreviewVM;
using PersonalSafety.Services;

namespace PersonalSafety.Controllers.Views
{
    public class EmailPreviewController : Controller
    {
        private readonly IEmailService _emailService;
        private const string placeHolder = "PLACEHOLDER@PLACEHOLDER.COM";

        public EmailPreviewController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [Route("PreviewEmails")]
        [HttpGet]
        public IActionResult Index()
        {
            var previews = new List<EmailPreviewViewModel>();
            var methods = typeof(IEmailService).GetMethods().Where(m => m.ReturnType.Name == nameof(MailMessage));
            foreach(var method in methods)
            {
                var paramCount = method.GetParameters().Count();
                var mailMessage = (MailMessage)method.Invoke(_emailService, BuildParameterArray(paramCount));
                var viewModel = new EmailPreviewViewModel(FormatMethodName(method.Name), mailMessage);

                previews.Add(viewModel);
            }

            return View(previews);
        }

        private string[] BuildParameterArray(int count)
        {
            var result = new List<string>();
            for(int i = 0; i< count; i++)
            {
                result.Add(placeHolder);
            }
            return result.ToArray();
        }

        private string FormatMethodName(string rawName)
        {
            return System.Text.RegularExpressions.Regex.Replace(rawName, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim().Replace("Raw", "");
        }
    }
}