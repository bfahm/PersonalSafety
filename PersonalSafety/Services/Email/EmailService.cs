using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PersonalSafety.Models;
using PersonalSafety.Options;
using PersonalSafety.Services.Otp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOptions<AppSettings> _appSettings;

        public EmailService(UserManager<ApplicationUser> userManager, IOptions<AppSettings> appSettings)
        {
            _userManager = userManager;
            _appSettings = appSettings;
        }

        public async Task<List<string>> SendConfirmMailAsync(string email)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return new List<string>();
            }

            string mailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string mailConfirmationOTP = OTPHelper.GenerateOTP(user.Id).ComputeTotp();
            List<string> emailSendingResults = new EmailServiceHelper(email, mailConfirmationToken, mailConfirmationOTP, _appSettings.Value.AppBaseUrlView, "ConfirmMail").SendEmail();

            return emailSendingResults;
        }
    }
}
