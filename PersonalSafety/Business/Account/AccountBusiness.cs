using Microsoft.AspNetCore.Identity;
using PersonalSafety.Models;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Services;
using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels.AccountVM;
using PersonalSafety.Services.Otp;
using Microsoft.Extensions.Logging;

namespace PersonalSafety.Business
{
    public class AccountBusiness : IAccountBusiness
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILoginService _loginService;
        private readonly IEmailService _emailService;
        private readonly IPersonnelRepository _personnelRepository;
        private readonly ILogger<AccountBusiness> _logger;

        public AccountBusiness(UserManager<ApplicationUser> userManager, ILoginService loginService, IEmailService emailService, IPersonnelRepository personnelRepository, ILogger<AccountBusiness> logger)
        {
            _userManager = userManager;
            _loginService = loginService;
            _emailService = emailService;
            _personnelRepository = personnelRepository;
            _logger = logger;
        }

        public async Task<APIResponse<LoginResponseViewModel>> LoginAsync(LoginRequestViewModel request)
        {
            APIResponse<LoginResponseViewModel> response = new APIResponse<LoginResponseViewModel>
            {
                Status = (int) APIResponseCodesEnum.Unauthorized,
                HasErrors = true
            };
            response.Messages.Add("User/Password combination is wrong.");

            ApplicationUser user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return response;
            }

            bool userHasValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!userHasValidPassword)
            {
                return response;
            }

            response.Messages = new List<string>(); // Reset messages since user had the correct combination.
            bool userHasConfirmedHisEmail = await _userManager.IsEmailConfirmedAsync(user);

            if (!userHasConfirmedHisEmail)
            {
                response.Status = (int)APIResponseCodesEnum.NotConfirmed;
                response.HasErrors = true;
                response.Messages = new List<string> { "This email was not confirmed, please activate your email first by tapping the link provided in the email we sent you then proceed." };
                return response;
            }

            AccountDetailsViewModel accountDetailsViewModel = new AccountDetailsViewModel();
            AuthenticationDetailsViewModel authenticationDetailsViewModel = new AuthenticationDetailsViewModel();
            LoginResponseViewModel responseViewModel = new LoginResponseViewModel();

            if (user.ForceChangePassword)
            {
                var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                authenticationDetailsViewModel.Token = passwordResetToken;
                responseViewModel = new LoginResponseViewModel
                    { AuthenticationDetails = authenticationDetailsViewModel };

                response.Result = responseViewModel;
                response.Status = (int)APIResponseCodesEnum.IdentityError;
                response.HasErrors = false;
                response.Messages = new List<string>
                {
                    "It appears it is time to change you password, use this token to update your password, then try logging in again.",
                    "Use /ResetPasswordAsync to update your password."
                };
                return response;
            }

            authenticationDetailsViewModel = await _loginService.GenerateAuthenticationDetailsAsync(user);
            responseViewModel.AuthenticationDetails = authenticationDetailsViewModel;

            accountDetailsViewModel.Email = user.Email;
            accountDetailsViewModel.FullName = user.FullName;
            accountDetailsViewModel.Roles = await _userManager.GetRolesAsync(user);

            if (accountDetailsViewModel.Roles.Any())
            {
                response.Messages.Add("If you are a working entity, retrieve more data about your account through the endpoint below:");
                response.Messages.Add("api/Account/Personnel/GetBasicInfo");
            }
            
            responseViewModel.AccountDetails = accountDetailsViewModel;

            response.Result = responseViewModel;
            response.Status = 0;
            response.HasErrors = false;
            response.Messages.Add("Success! You are now logged in.");

            _logger.LogInformation("Login Attempt with " + request.Email);

            return response;
        }

        public async Task<APIResponse<AuthenticationDetailsViewModel>> RefreshTokenAsync(RefreshTokenRequestViewModel request)
        {
            APIResponse<AuthenticationDetailsViewModel> response = new APIResponse<AuthenticationDetailsViewModel>();

            var validationResult = _loginService.ValidateRefreshToken(request.Token, request.RefreshToken, out var validatedToken);

            if (validationResult != null)
            {
                response.WrapResponseData(validationResult);
                return response;
            }

            // All checks passed, generate a new token for the user.
            var user = await _userManager.FindByIdAsync(validatedToken.Claims.Single(x => x.Type == "id").Value);
            if (user == null)
            {
                response.Status = (int)APIResponseCodesEnum.IdentityError;
                response.HasErrors = true;
                response.Messages.Add("Error: An error occured while trying to retrieve your account details.");
                return response;
            }

            response.Result = await _loginService.GenerateAuthenticationDetailsAsync(user);
            response.Messages.Add("Success! Here is your updated authentication data.");
            return response;
        }

        public async Task<APIResponse<bool>> ResetPasswordAsync(string email)
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
            List<string> emailSendingResults = _emailService.SendPasswordResetEmail(email, resetPasswordToken, "ResetPassword");
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

        public async Task<APIResponse<bool>> SubmitResetPasswordAsync(ResetPasswordViewModel request)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            ApplicationUser user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                response.Messages.Add("User with provided email does not exist.");
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
                return response;
            }

            user.ForceChangePassword = false;
            await _userManager.UpdateAsync(user);

            response.Messages = new List<string>{"Success! Password was changed successfully."};
            response.Result = true;
            return response;
        }

        public async Task<APIResponse<bool>> SendConfirmMailAsync(string email)
        {
            APIResponse<bool> response = new APIResponse<bool>();
            response.Messages.Add("We got your email, if this email is registered you should get an activation link and an OTP shortly.");

            ApplicationUser user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return response;
            }

            string mailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string mailConfirmationOTP = OTPHelper.GenerateOTP(user.Id).ComputeTotp();
            List<string> emailSendingResults = _emailService.SendActivationEmail(email, mailConfirmationToken, mailConfirmationOTP, "ConfirmMail");

            if (emailSendingResults.Count() != 0)
            {
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.TechnicalError;
                response.Messages = new List<string>();
                response.Messages.AddRange(emailSendingResults);
            }

            response.Result = true;
            return response;
        }

        public async Task<APIResponse<bool>> SubmitConfirmationAsync(ConfirmMailViewModel request)
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

        public async Task<APIResponse<bool>> ChangeEmailAsync(string userId, string newEmail)
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

            if ((await _userManager.FindByEmailAsync(newEmail)) != null)
            {
                response.Messages.Add("The provided new email address was taken before.");
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.IdentityError;
                return response;
            }

            string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            List<string> emailSendingResults = _emailService.SendEmailChangedNewEmail(user.Email, newEmail, confirmationToken, "ChangeEmail");

            if (emailSendingResults.Count != 0)
            {
                response.Messages.AddRange(emailSendingResults);
                response.Status = (int)APIResponseCodesEnum.TechnicalError;
                response.HasErrors = true;
            }

            response.Messages.Add($"Please check your new email {user.Email} for confimation links.");
            response.Result = true;
            return response;
        }

        public async Task<APIResponse<bool>> SubmitChangeEmailAsync(ChangeEmailViewModel request)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            ApplicationUser user = await _userManager.FindByEmailAsync(request.OldEmail);

            if (user == null)
            {
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                response.Messages = new List<string> { "The email provided was not registered before, please check for typos." };
                return response;
            }

            user.EmailConfirmed = false;
            var result = await ConfirmMailHybrid(user, request.Token);

            if (!result)
            {
                response.Messages.Add("Invalid Token");
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                return response;
            }

            user.Email = request.NewEmail;
            user.UserName = request.NewEmail;
            await _userManager.UpdateAsync(user);

            List<string> emailSendingResults = _emailService.SendEmailChangedOldEmail(request.OldEmail, request.NewEmail);

            if (emailSendingResults.Count != 0)
            {
                response.Messages.AddRange(emailSendingResults);
                response.Status = (int)APIResponseCodesEnum.TechnicalError;
            }

            response.Messages = new List<string> { "Success! Email Changed." };
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

            List<string> emailSendingResults = _emailService.SendPasswordChangedEmail(user.Email);

            if (emailSendingResults.Count() != 0)
            {
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.TechnicalError;
                response.Messages.AddRange(emailSendingResults);
            }

            return response;
        }

        public async Task<APIResponse<bool>> ValidateTokenAsync(string token)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            var validatedToken = _loginService.GetPrincipalFromToken(token);
            var user = await _userManager.FindByIdAsync(validatedToken.Claims.Single(x => x.Type == "id").Value);
            if (user == null)
            {
                response.Messages.Add("User not authorized.");
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.Unauthorized;
                return response;
            }

            response.Result = true;
            response.Messages.Add("Your token is valid and did not expire.");
            response.Messages.Add("Currently logged in user email is: " + user.Email);
            
            return response;
        }

        // Only allowed to be used by personnel..
        public APIResponse<AccountBasicInfoViewModel> GetBasicInfo(string userId)
        {
            APIResponse<AccountBasicInfoViewModel> response = new APIResponse<AccountBasicInfoViewModel>();

            var personnelDepartment = _personnelRepository.GetPersonnelDepartment(userId);

            AccountBasicInfoViewModel viewModel = new AccountBasicInfoViewModel
            {
                AuthorityTypeName = _personnelRepository.GetPersonnelAuthorityTypeString(userId),
                DepartmentName = personnelDepartment?.ToString(),
                DepartmentId = personnelDepartment?.Id
            };

            response.Result = viewModel;
            return response;
        }

        private async Task<bool> ConfirmMailHybrid(ApplicationUser user, string token)
        {
            if (token.Length == OTPHelper.otpSize)
            {
                var totp = OTPHelper.GenerateOTP(user.Id);
                bool isTokenValid = totp.VerifyTotp(token, out _);
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
