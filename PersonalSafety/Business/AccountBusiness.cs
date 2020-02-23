using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PersonalSafety.Helpers;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PersonalSafety.Services
{
    public class AccountBusiness : IAccountBusiness
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly IOptions<AppSettings> _appSettings;

        public AccountBusiness(UserManager<ApplicationUser> userManager, JwtSettings jwtSettings, IOptions<AppSettings> appSettings)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings;
            _appSettings = appSettings;
        }

        public async Task<APIResponse<string>> LoginAsync(LoginRequestViewModel request)
        {
            APIResponse<string> response = new APIResponse<string>();
            response.Status = 401;

            ApplicationUser user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                response.Messages.Add("User with provided email does not exsist.");
                return response;
            }


            bool userHasValidPassowrd = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!userHasValidPassowrd)
            {
                response.Messages.Add("User/Password combination is wrong.");
                return response;
            }

            //TODO: Check if user's email is confirmed first
            response.Result = GenerateAuthenticationResult(user);
            response.HasErrors = false;
            response.Status = 200;

            return response;
        }

        public async Task<APIResponse<string>> RegisterAsync(RegistrationRequestViewModel request)
        {
            APIResponse<string> response = new APIResponse<string>();

            ApplicationUser exsistingUserFoundByEmail = await _userManager.FindByEmailAsync(request.Email);
            if(exsistingUserFoundByEmail != null)
            {
                response.Messages.Add("User with this email address already exsists.");
                return response;
            }

            ApplicationUser exsistingUserFoundByNationalId = _userManager.Users.Where(u => u.NationalId == request.NationalId).FirstOrDefault();
            if (exsistingUserFoundByNationalId != null)
            {
                response.Messages.Add("User with this National Id was registered before.");
                return response;
            }

            ApplicationUser exsistingUserFoundByPhoneNumber = _userManager.Users.Where(u => u.PhoneNumber == request.PhoneNumber).FirstOrDefault();
            if (exsistingUserFoundByPhoneNumber != null)
            {
                response.Messages.Add("User with this Phone Number was registered before.");
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
                return response;
            }

            response.Result = GenerateAuthenticationResult(newUser);
            response.HasErrors = false;
            response.Messages.Add("Successfully created a new user with email" + request.Email);

            return response;
        }

        public async Task<APIResponse<string>> ForgotPasswordAsync(string email)
        {
            APIResponse<string> response = new APIResponse<string>
            {
                Messages = new List<string> { "We got your email, if this email is registered and confirmed you should get a password reset mail." }
            };

            ApplicationUser user = await _userManager.FindByEmailAsync(email);

            if (!user.EmailConfirmed)
            {
                response.Messages.Add("The email provided was not confirmed, you must confirm your email first.");
                return response;
            }

            if (user != null)
            {
                string resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                List<string> emailSendingResults = new EmailHelper(email, resetPasswordToken, _appSettings.Value.AppBaseUrl).SendEmail();
                response.Messages.AddRange(emailSendingResults);
            }

            return response;
        }

        public async Task<APIResponse<string>> ResetPasswordAsync(ResetPasswordViewModel request)
        {
            APIResponse<string> response = new APIResponse<string>();

            ApplicationUser user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                response.Messages.Add("User with provided email does not exsist.");
                return response;
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                response.Messages.Add("New Password and Confirm Password does not match, please try again.");
                return response;
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            response.HasErrors = !result.Succeeded;
            response.Messages = result.Errors.Select(e => e.Description).ToList();

            return response;
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
    }
}
