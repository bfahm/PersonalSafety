using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PersonalSafety.Models;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using PersonalSafety.Services;
using PersonalSafety.Options;
using PersonalSafety.Contracts;
using PersonalSafety.Services.Otp;

namespace PersonalSafety.Business
{
    public class AccountBusiness : IAccountBusiness
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppSettings _appSettings;
        private readonly IPersonnelRepository _personnelRepository;
        private readonly IJwtAuthService _jwtAuthService;
        private readonly IEmailService _emailService;

        public AccountBusiness(UserManager<ApplicationUser> userManager, AppSettings appSettings, IPersonnelRepository personnelRepository, IJwtAuthService jwtAuthService, IEmailService emailService)
        {
            _userManager = userManager;
            _appSettings = appSettings;
            _personnelRepository = personnelRepository;
            _jwtAuthService = jwtAuthService;
            _emailService = emailService;
        }

        public async Task<APIResponse<string>> LoginAsync(LoginRequestViewModel request)
        {
            APIResponse<string> response = new APIResponse<string>();
            response.Status = (int)APIResponseCodesEnum.Unauthorized;
            response.Messages.Add("User/Password combination is wrong.");
            response.HasErrors = true;

            ApplicationUser user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return response;
            }

            bool userHasValidPassowrd = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!userHasValidPassowrd)
            {
                return response;
            }

            bool userHasConfirmedHisEmail = await _userManager.IsEmailConfirmedAsync(user);

            if (!userHasConfirmedHisEmail)
            {
                response.Status = (int)APIResponseCodesEnum.NotConfirmed;
                response.HasErrors = true;
                response.Messages = new List<string> { "This email was not confirmed, please activate your email first by tapping the link provided in the email we sent you then proceed." };
                return response;
            }

            
            response.Result = await _jwtAuthService.GenerateAuthenticationTokenAsync(user);
            response.Status = 0;
            response.HasErrors = false;
            response.Messages = new List<string>();
            response.Messages.Add("Success! You are now logged in.");

            IEnumerable<string> roles = await _userManager.GetRolesAsync(user);
            
            if (roles.Count() != 0)
            {
                response.Messages.Add("Displaying list of roles current user have:");
                response.Messages.AddRange(roles);


                if (roles.Where(r => r.Contains("Personnel")).Any() != false)
                {
                    response.Messages.Add("It appears that you are a working entity, displaying your authority type:");
                    string authorityTypeString = _personnelRepository.GetPersonnelAuthorityTypeString(user.Id);
                    response.Messages.Add(authorityTypeString);
                }
            }

            return response;
        }

        public async Task<APIResponse<bool>> ForgotPasswordAsync(string email)
        {
            APIResponse<bool> response = new APIResponse<bool>
            {
                Messages = new List<string> { "We got your email, if this email is registered you should get a password reset mail." }
            };

            ApplicationUser user = await _userManager.FindByEmailAsync(email);

            if(user == null)
            {
                return response;
            }

            if (!user.EmailConfirmed)
            {
                response.Messages.Add("The email provided was not confirmed, you must confirm your email first.");
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.NotConfirmed;
                return response;
            }

            string resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            List<string> emailSendingResults = new EmailServiceHelper(email, resetPasswordToken, null ,_appSettings.AppBaseUrlView, "ResetPassword").SendEmail();
            if (emailSendingResults.Count != 0)
            {
                response.Messages.AddRange(emailSendingResults);
                response.Status = (int)APIResponseCodesEnum.TechnicalError;
                response.HasErrors = true;
                return response;
            }

            response.Result = true;
            return response;
        }

        public async Task<APIResponse<bool>> ResetPasswordAsync(ResetPasswordViewModel request)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            ApplicationUser user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                response.Messages.Add("User with provided email does not exsist.");
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                return response;
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                response.Messages.Add("New Password and Confirm Password does not match, please try again.");
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                return response;
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (!result.Succeeded)
            {
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.IdentityError;
                response.Messages = result.Errors.Select(e => e.Description).ToList();
            }

            response.Result = true;
            return response;
        }

        public async Task<APIResponse<bool>> SendConfirmMailAsync(string email)
        {
            APIResponse<bool> response = new APIResponse<bool>();
            response.Messages.Add("We got your email, if this email is registered you should get an activation link and an OTP shortly.");

            var emailSendingResults = await _emailService.SendConfirmMailAsync(email);

            if (emailSendingResults != null)
            {
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.TechnicalError;
                response.Messages = new List<string>();
                response.Messages.AddRange(emailSendingResults);
            }

            response.Result = true;
            return response;
        }

        public async Task<APIResponse<bool>> ConfirmMailAsync(ConfirmMailViewModel request)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            ApplicationUser user = await _userManager.FindByEmailAsync(request.Email);

            if(user == null)
            {
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                response.Messages = new List<string> { "The email provided was not registered before, please check for typos." };
                return response;
            }

            var result = await ConfirmMailHybrid(user, request.Token);

            if (!result)
            {
                response.Messages.Add("Invalid Token");
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                return response;
            }

            response.Messages = new List<string> { "Success! Email Confirmed." };
            response.Result = true;

            return response;
        }

        public async Task<APIResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordViewModel request)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                response.Messages.Add("Could not authenticate user.");
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.Unauthorized;
                return response;
            }

            // Removed check for confirmed email here, user wont gain a token>userId if he's not confirmed.

            if(request.OldPassword == request.NewPassword)
            {
                response.Messages.Add("Your old password is matching your new one. Please choose another New Password.");
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                return response;
            }

            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.IdentityError;
                response.Messages = result.Errors.Select(e => e.Description).ToList();
                return response;
            }

            response.Messages.Add("Success! Password was changed for " + user.Email);
            response.Result = true;
            return response;
        }

        public async Task<APIResponse<bool>> ValidateUserAsync(string userId, string email)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                response.Messages.Add("User not authorized.");
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.Unauthorized;
                return response;
            }

            if(user.Email == email)
            {
                response.Result = true;
                response.Messages.Add(email + " is currently logged in.");
                return response;
            }
            else
            {
                response.Status = (int)APIResponseCodesEnum.Unauthorized;
                response.Result = false;
                response.HasErrors = true;
                response.Messages.Add("Email and Token did not match.");
                return response;
            }
        }

        private async Task<bool> ConfirmMailHybrid(ApplicationUser user, string token)
        {
            if (token.Length == OTPHelper.otpSize)
            {
                var totp = OTPHelper.GenerateOTP(user.Id);
                long timeFrame;
                bool isTokenValid = totp.VerifyTotp(token, out timeFrame);
                if (isTokenValid)
                {
                    user.EmailConfirmed = true;
                    var confirmationResult = await _userManager.UpdateAsync(user);
                    return confirmationResult.Succeeded;
                }
                
                return false;
            }
            else
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                return result.Succeeded;
            }
        }

        
    }
}
