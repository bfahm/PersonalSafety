using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PersonalSafety.Helpers;
using PersonalSafety.Models;
using PersonalSafety.Models.Enums;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PersonalSafety.Business.Account
{
    public class AccountBusiness : IAccountBusiness
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IEmergencyContactRepository _emergencyContactRepository;

        public AccountBusiness(UserManager<ApplicationUser> userManager, JwtSettings jwtSettings, IOptions<AppSettings> appSettings, IEmergencyContactRepository emergencyContactRepository)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings;
            _appSettings = appSettings;
            _emergencyContactRepository = emergencyContactRepository;
        }

        public async Task<APIResponse<bool>> RegisterAsync(RegistrationViewModel request)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            ApplicationUser exsistingUserFoundByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (exsistingUserFoundByEmail != null)
            {
                response.Messages.Add("User with this email address already exsists.");
                response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                response.HasErrors = true;
                return response;
            }

            ApplicationUser exsistingUserFoundByNationalId = _userManager.Users.Where(u => u.NationalId == request.NationalId).FirstOrDefault();
            if (exsistingUserFoundByNationalId != null)
            {
                response.Messages.Add("User with this National Id was registered before.");
                response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                response.HasErrors = true;
                return response;
            }

            ApplicationUser exsistingUserFoundByPhoneNumber = _userManager.Users.Where(u => u.PhoneNumber == request.PhoneNumber).FirstOrDefault();
            if (exsistingUserFoundByPhoneNumber != null)
            {
                response.Messages.Add("User with this Phone Number was registered before.");
                response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                response.HasErrors = true;
                return response;
            }

            ApplicationUser newUser = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email,
                FullName = request.FullName,
                NationalId = request.NationalId,
                PhoneNumber = request.PhoneNumber
            };

            var creationResult = await _userManager.CreateAsync(newUser, request.Password);

            if (!creationResult.Succeeded)
            {
                response.Messages = creationResult.Errors.Select(e => e.Description).ToList();
                response.Status = (int)APIResponseCodesEnum.IdentityError;
                response.HasErrors = true;
                return response;
            }

            var confirmationMailResult = await SendConfirmMailAsync(request.Email);

            response.Messages.Add("Successfully created a new user with email " + request.Email);
            response.Messages.Add("Please check your email for activation links before you continue.");
            response.Messages.AddRange(confirmationMailResult.Messages);
            response.Result = true;
            return response;
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
            
            response.Result = GenerateAuthenticationResult(user);
            response.Status = 0;
            response.HasErrors = false;
            response.Messages = null;

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
            List<string> emailSendingResults = new EmailHelper(email, resetPasswordToken, null ,_appSettings.Value.AppBaseUrlView, "ResetPassword").SendEmail();
            if (emailSendingResults != null)
            {
                response.Messages.AddRange(emailSendingResults);
                response.Status = (int)APIResponseCodesEnum.TechnicalError;
                response.HasErrors = true;
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

            ApplicationUser user = await _userManager.FindByEmailAsync(email);

            if(user == null)
            {
                return response;
            }
            
            string mailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string mailConfirmationOTP = OTPHelper.GenerateOTP(user.Id).ComputeTotp();
            List<string> emailSendingResults = new EmailHelper(email, mailConfirmationToken, mailConfirmationOTP ,_appSettings.Value.AppBaseUrlView, "ConfirmMail").SendEmail();
            
            if(emailSendingResults != null)
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
            }

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

        private string GenerateAuthenticationResult(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // Things to be included and encoded in the token
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("id", user.Id)
                }),
                // Token will expire 2 hours from which it was created
                Expires = DateTime.UtcNow.AddHours(2),
                //
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
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
