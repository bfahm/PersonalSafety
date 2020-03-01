using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Models.ViewModels;
using PersonalSafety.Business;
using Microsoft.AspNetCore.Authorization;
using PersonalSafety.Business.Account;
using PersonalSafety.Helpers;
using PersonalSafety.Models.Enums;

namespace PersonalSafety.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountBusiness _accountBusiness;

        public AccountController(IAccountBusiness identityService)
        {
            _accountBusiness = identityService;
        }

        /// <summary>
        /// Create a new account to be able to access the system.
        /// </summary>
        /// <remarks>
        /// ## Main Functionality
        /// All the JSON object values **are** required and must follow these rules:
        /// 
        /// - **Email** : must be unique and not used before, additionally it must follow the correct email structure
        /// - **Password** : must be complex, contain number, symbols, Capital and Small letters
        /// - **NationalId** : must be exactly of 14 digits
        /// - **PhoneNumber** : must be exactly of 11 digits
        /// 
        /// After a valid attempt, this function also **automatically** sends a verification email to be used in `/api/Account/ConfirmMail` directly.
        /// **IMPORTANT:** User does not have access to any of the system's functionality till he actually verify his email.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-1]**: Invalid Request
        /// - User exsists and has registered before
        /// - Someone with the same National ID has registered before
        /// - Someone with the same Phone Number has registered before
        /// 
        /// #### **[-2]**: Identity Error
        /// This is a generic error code resembles something went wrong inside the Identity Framework and can be diagnosed using the response Messages list.
        /// </remarks>
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegistrationViewModel request)
        {
            var authResponse = await _accountBusiness.RegisterAsync(request);

            if (authResponse.HasErrors)
            {
                return BadRequest(authResponse);
            }

            return Ok(authResponse);
        }

        /// <summary>
        /// Login to the system and retrieve a token to be used in all interactions with the system
        /// </summary>
        /// <remarks>
        /// ## Main Functionality
        /// - *All the JSON object values **are** required.*
        /// - This method returns a token of type **Bearer** to be used in the header of future requests
        /// - Example:
        /// 
        /// | | | |
        /// |-|-|-|
        /// |__Authorization__| Bearer `eyfjssprn3lv)83c+c •••` |
        /// 
        /// ------------------
        /// 
        /// **IMPORTANT:** User must verify his email before proceeding.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[401]**: Unauthorized
        /// Email and Password combination does not match
        /// 
        /// #### **[-4]**: Not Confirmed
        /// Password combination is valid but the user did not verify his email.
        /// </remarks>
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequestViewModel request)
        {
            var authResponse = await _accountBusiness.LoginAsync(request);

            if (authResponse.HasErrors)
            {
                return BadRequest(authResponse);
            }

            return Ok(authResponse);
        }

        /// <summary>
        /// This function sends an email containing a special token to a registered user using the provided email.
        /// </summary>
        /// <remarks>
        /// ## Preliminary
        /// ### Resetting works through **TWO** main discrete steps, here is how it works genrally
        ///1. Make a call to `Api/Acount/ForgotPassword` which takes a specific Email as a parameter.
        ///	    - This function then sends an email with a **special token** to the user's email.
        ///	    - This **special token** can be wrapped up in a button or a link that redirects to a webpage to continue the verification process.
        ///	    - The webpage could then parse the incoming address (from the link in the body of the Email), and then extract the **special token**
        ///2. The second step is sending back the **special token** alongside the **user mail** and the **new password** to the server using `Api/Acount/ResetPassword` which compares them and does the logic of updated password for to the new one.
        ///
        /// **IMPORTANT:** For security reasons, this call requires the user to be previously verified with a confirmed mail.
        /// 
        /// ## Technical Note
        /// Unlike the Email Confirmation logic, this function only produces one type of tokens, Identity token (long complex string), to be used directly by Web end interfaces.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-3]**: Technical Error
        /// Could happend due to some problems that occured while trying to send the email. Often related to Gmail's SMTP server.
        /// 
        /// **IMPORTANT:** For security reasons, even if user provided an invalid email, there wouldn't be any indications of errors to protect from **Brute Force** attacks.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> ForgotPassword([FromQuery] string mail)
        {
            if (mail!=null)
            {
                var response = await _accountBusiness.ForgotPasswordAsync(mail);

                return Ok(response);
            }
            else
            {
                return BadRequest(new APIResponse<bool>
                {
                    Status = (int)APIResponseCodesEnum.BadRequest,
                    HasErrors = true,
                    Result = false,
                    Messages = new List<string> { "Email field cannot be empty." }
                });
            }
            
        }

        /// <summary>
        /// This function uses the token that was sent to the email to be able to update user's password to a new one. 
        /// </summary>
        /// <remarks>
        /// ## Main Functionality
        /// *All the JSON object values **are** required and must follow these rules*
        /// 
        /// ### Important Notes
        /// - The new password must be complex.
        /// - This is a one time use token and it expires once used.
        /// - Confirm Password **MUST** match New Password
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-1]**: Invalid Request
        /// - Email was not registered before
        /// - Confirm Password does not match New Password
        /// 
        /// #### **[-2]**: Identity Error
        /// This is a generic error code resembles something went wrong inside the Identity Framework and can be diagnosed using the response Messages list.
        /// </remarks>
        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel request)
        {
            var response = await _accountBusiness.ResetPasswordAsync(request);

            return Ok(response);
        }

        /// <summary>
        /// This method sends a verification email to a registered user using the provided email.
        /// </summary>
        /// <remarks>
        /// ## Preliminary
        /// ### Verification generally works through **TWO** main discrete steps.
        /// 1. Make a call to `Api/Account/SendConfirmationMail` which takes a specific Email as a parameter.
        ///	    - This function then sends an email with a **special token** to the user's email.
        ///	    - This **special token** can be wrapped up in a button or a link that redirects to a webpage to continue the verification process.
        ///	    - The webpage could then parse the incoming address (from the link in the body of the Email), and then extract the **special token**
        /// 2. The second step is sending back the **special token** alongside the **user mail** to the server using `Api/Account/ConfirmMail` which compares them and does the logic of switching the user's state to be **verified**.
        ///
        /// ## Technical Note
        /// This method produces two types of tokens simultainously:
        /// - Identity token (long complex string)
        /// - One Time 4 Digit Password (OTP)
        /// Both can be used by end devices for the `Api/Account/ConfirmMail` endpoint.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-3]**: Technical Error
        /// Could happend due to some problems that occured while trying to send the email. Often related to Gmail's SMTP server.
        /// 
        /// **IMPORTANT:** For security reasons, even if user provided an invalid email, there wouldn't be any indications of errors to protect from **Brute Force** attacks.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> SendConfirmationMail([FromQuery] string email)
        {
            if (email != null)
            {
                var response = await _accountBusiness.SendConfirmMailAsync(email);

                return Ok(response);
            }
            else
            {
                return BadRequest(new APIResponse<bool>
                {
                    Status = (int)APIResponseCodesEnum.BadRequest,
                    HasErrors = true,
                    Result = false,
                    Messages = new List<string> { "Email field cannot be empty." }
                });
            }
        }

        /// <summary>
        /// This function uses the token that was sent to the email to verify the user's information.
        /// </summary>
        /// <remarks>
        /// ## Main Functionality
        /// - *All the JSON object values **are** required.*
        /// - This method uses the token provided in the email to match it with the token and verify the user's email.
        /// - This method can also consume the 4 digit OTP in a hybrid fashion for maximum flexibility for end devices and end-developers.
        /// - Example:
        /// 
        /// | | | |
        /// |-|-|-|
        /// |__Email__| `test@test.com` |
        /// |__Token__| `eyfjssprn3lv)83c+c •••` |
        /// 
        /// ##### **OR**
        /// 
        /// | | | |
        /// |-|-|-|
        /// |__Email__| `test@test.com` |
        /// |__Token__| `1234` |
        /// 
        /// ------------------
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-1]**: Invalid Request
        /// Could happen if the token and the email did not produce a successful match.
        /// 
        /// </remarks>
        [HttpPost]
        public async Task<IActionResult> ConfirmMail([FromBody] ConfirmMailViewModel request)
        {
            var response = await _accountBusiness.ConfirmMailAsync(request);

            return Ok(response);
        }

        /// <summary>
        /// This method changes the currently logged in user's password. 
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// ## Main Functionality
        /// Unlike the other method `ResetPassword`, this method needs the user to be logged in **and** to remeber his old password to be able to update it to a new one. Also this method doesn't need any email sending logic.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-1]**: InvalidRequest
        /// Could happen if Confirm Password and New Password does not match.
        /// #### **[-2]**: IdentityError
        /// This is a generic error code resembles something went wrong inside the Identity Framework and can be diagnosed using the response Messages list.
        /// #### **[-4]**: NotConfrimed
        /// Could happen if the email matching the provided token was not verified.
        /// #### **[401]**: Unauthorized
        /// Could happen if the provided token in the header has expired or is not valid.
        /// </remarks>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel request)
        {
            //? means : If value is not null, retrieve it
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _accountBusiness.ChangePasswordAsync(currentlyLoggedInUserId, request);

            return Ok(response);
        }


        /// <summary>
        /// Helper method for checking authenticity
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// ## Main Functionality
        /// This method compares the proivded token and email to check if the user is currently logged in.
        /// 
        /// #### Technical Note:
        /// Providing an email for this function is not an extra step for validation, in face most of the times, if the token provided is not valid, this function will not work right away and will return an Http Status Code `401 UNAUTHORIZED` before trying to check for the provided email.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[401]**: Unauthorized
        /// Could happen if the provided token in the header has expired or is not valid.
        /// </remarks>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ValidateToken([FromQuery] string email)
        {

            if (email != null)
            {
                string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

                var response = await _accountBusiness.ValidateUserAsync(currentlyLoggedInUserId, email);

                return Ok(response);
            }
            else
            {
                return BadRequest(new APIResponse<bool>
                {
                    Status = (int)APIResponseCodesEnum.BadRequest,
                    HasErrors = true,
                    Result = false,
                    Messages = new List<string> { "Email field cannot be empty." }
                });
            }
        }
    }
}