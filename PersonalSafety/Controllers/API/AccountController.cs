﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Models.ViewModels;
using PersonalSafety.Business;
using Microsoft.AspNetCore.Authorization;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Examples;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.Net.Http.Headers;
using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels.AccountVM;

namespace PersonalSafety.Controllers.API
{
    [Route(ApiRoutes.Default)]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountBusiness _accountBusiness;

        public AccountController(IAccountBusiness accountBusiness)
        {
            _accountBusiness = accountBusiness;
        }

        /// <summary>
        /// Login to the system and retrieve a token to be used in all interactions with the system
        /// </summary>
        /// <remarks>
        /// ## Main Functionality
        ///
        /// The return object of the function should contain two inner objects:
        /// - "authenticationDetails": contains the authentication details to be used further in the system and they are:
        ///     - "token": A regular **Bearer Token** that doesn't live long enough and needs to be refreshed every now and then.
        ///     - "refreshToken": A token to be used to request a new **Bearer Token** when it expires. (More info about this in `/RefreshToken`)
        /// - "accountDetails": contains basic details about the account that was just authenticated, and these are:
        ///     - Full Name
        ///     - User Email
        ///     - List of Roles the user have
        ///
        ///
        /// ## Details about usage of Bearer tokens
        /// 
        /// This method returns a token of type **Bearer** to be used in the header of future requests, for example:
        ///
        /// ```
        /// Authorization        Bearer `eyfjssprn3lv)83c+c •••`
        /// ```
        /// 
        /// #### Note that users must verify his email before proceeding.
        ///
        /// ## Flow
        /// - Save these information to be used later in another method:
        ///     - Email that user provided to attempt to login
        ///     - Password Reset Token that was received instead of the expected `Bearer` token.
        /// - Navigate the user to a page where he could reset his password containing
        ///     - New Password Field
        ///     - Confirm Password Field
        /// - Use these data to fill the password reset object of `/ResetPassword` method
        /// - Confirm and wait for password reset confirmation
        /// - Redirect the user back to the login page where he can provide his email and his **new password**
        /// - Confirm that the status of the return object is `"status": 0` representing no issues in the process of logging in.
        /// - Confirm that the result of the return object starts with `ey•••` representing that the token is a `Bearer` token.
        /// - If the logged in user is a working entity, retrieve mre info about his account through `/Personnel/GetBasicInfo`
        /// 
        /// ## First Login Situations
        /// The following applies to these types of users:
        ///  - **Agents** registered via **Admins**
        ///  - **Rescuers** registered via **Agents**
        ///
        /// These types of users are assigned a password by their managers, hence, this password should be changed upon first login attempt.
        /// 
        /// Upon the first login attempt, these users will receive a token starting with something like `CfDJ8prn3lv)83c+c •••`, in contrary, normal login tokens starts with `ey •••`.
        /// This token represents a **password reset** token. Additionally, the status of the return object is `"status": -2`. The code -2 represents an issue regarding user's authentication.
        ///
        /// **IMPORTANT:** This token will not be accepted as a `Bearer` token to enable usage of any of the services provided the system, in other words, this token does not represent a "Logged In" status.
        ///
        /// ------------------
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[401]**: Unauthorized
        /// Email and Password combination does not match
        /// 
        /// #### **[-2]**: Identity Error
        /// Occurs if the user should reset his password to continue
        /// 
        /// #### **[-4]**: Not Confirmed
        /// Password combination is valid but the user did not verify his email.
        ///
        /// </remarks>
        [HttpPost]
        [SwaggerRequestExample(typeof(LoginRequestViewModel), typeof(LoginNormalUserExample))]
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
        /// Requests a new Bearer token when the existing one expires
        /// </summary>
        /// <remarks>
        /// ### Main Functionality
        /// This method works by providing the existing expired **Bearer Token** along with the **Refresh Token** that was granted to the user upon his successful login attempt.
        ///
        /// The function tries matching the provided pair and returns a new pair if they actually match.
        ///
        /// ### Things to note:
        /// - Refresh token have much longer life expectancy whereas **Bearer Token** are session dependent and needs refreshing frequently.
        /// - The function will only work if the **Bearer Token** has actually expired.
        /// - The function will fail to work if the **Refresh Token**:
        ///     - Has been used before
        ///     - Was invalidated by the system
        ///     - Has expired
        ///     - Is Invalid (Typos)
        /// - To check whether the saved **Bearer Token** has expired, use `/ValidateToken`
        /// 
        /// ### Flow
        /// - Login normally through `/Login` by providing the correct credentials
        /// - Save the returned **Bearer Token** and **Refresh Token** somewhere safe
        ///     - **Be sure to encrypt the saved Refresh Token before saving it locally, because it can be used to retrieve Bearer Tokens as long as doesn't expire, and it take a long time to expire.**
        /// - Use the **Bearer Token** for system services that needs authentication until it gets expired
        /// - When the token gets expired (the system returns a status code of HTTP 401), use the function to generate a new pair by providing:
        ///     - The expired **Bearer Token**
        ///     - The valid saved **Refresh Token** after decrypting it
        /// - If this function fails (mainly due to **Refresh Token** has typos or has expired): **LOG USER OUT**
        /// - Else: Save the new pair and resend the (HTTP 401) failing request.
        ///
        /// ------------------
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[404]**: Not Found
        /// - The refresh token might have been used before.
        /// - The refresh token might have been invalidated by the system.
        /// - The refresh token has expired.
        /// 
        /// #### **[400]**: Bad Request
        /// Token pairs does not match.
        /// 
        /// #### **[-2]**: IdentityError
        /// Corrupt user account information.
        ///
        /// </remarks>
        [HttpPost]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestViewModel request)
        { 
            var authResponse = await _accountBusiness.RefreshTokenAsync(request);

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
        /// ### Resetting works through **TWO** main discrete steps, here is how it works generally
        ///1. Make a call to `Api/Account/ForgotPassword` which takes a specific Email as a parameter.
        ///	    - This function then sends an email with a **special token** to the user's email.
        ///	    - This **special token** can be wrapped up in a button or a link that redirects to a webpage to continue the verification process.
        ///	    - The webpage could then parse the incoming address (from the link in the body of the Email), and then extract the **special token**
        ///2. The second step is sending back the **special token** alongside the **user mail** and the **new password** to the server using `Api/Account/ResetPassword` which compares them and does the logic of updated password for to the new one.
        ///
        /// **IMPORTANT:** For security reasons, this call requires the user to be previously verified with a confirmed mail.
        /// 
        /// ## Remarks
        /// This request is handled by the front end portal and processed autotmatically.
        /// 
        /// ## Technical Note
        /// Unlike the Email Confirmation logic, this function only produces one type of tokens, Identity token (long complex string), to be used directly by Web end interfaces.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-3]**: Technical Error
        /// Could happen due to some problems that occured while trying to send the email. Often related to Gmail's SMTP server.
        /// 
        /// **IMPORTANT:** For security reasons, even if user provided an invalid email, there wouldn't be any indications of errors to protect from **Brute Force** attacks.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> ForgotPassword([FromQuery] string mail)
        {
            /* New NameScheme is in the form of [verb] -> [submit][verb]
             * This function will be an exception wont be renamed to avoid breaking mobile end code.
             */
            if (mail!=null)
            {
                var response = await _accountBusiness.ResetPasswordAsync(mail);

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
            var response = await _accountBusiness.SubmitResetPasswordAsync(request);

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
        /// This method produces two types of tokens simultaneously:
        /// - Identity token (long complex string)
        /// - One Time 4 Digit Password (OTP)
        /// Both can be used by end devices for the `Api/Account/ConfirmMail` endpoint.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-3]**: Technical Error
        /// Could happen due to some problems that occured while trying to send the email. Often related to Gmail's SMTP server.
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
        public async Task<IActionResult> SubmitConfirmation([FromBody] ConfirmMailViewModel request)
        {
            var response = await _accountBusiness.SubmitConfirmationAsync(request);

            return Ok(response);
        }

        /// <summary>
        /// Request a change email event that is processed via the new email
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// ## Remarks
        /// This request is handled by the front end portal and processed autotmatically.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-2]**: IdentityError
        /// Email address provided is taken before.
        /// /// #### **[-3]**: Technical Error
        /// Could happen due to some problems that occured while trying to send the email. Often related to Gmail's SMTP server.
        /// #### **[401]**: Unauthorized
        /// Could happen if the provided token in the header has expired or is not valid.
        /// </remarks>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangeEmail([FromQuery] string newEmail)
        {
            string currentlyLoggedInUserId = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            var response = await _accountBusiness.ChangeEmailAsync(currentlyLoggedInUserId, newEmail);

            return Ok(response);
        }

        
        /// <summary>
        /// This method changes the currently logged in user's password. 
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// ## Main Functionality
        /// Unlike the other method `ResetPassword`, this method needs the user to be logged in **and** to remember his old password to be able to update it to a new one. Also this method doesn't need any email sending logic.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-1]**: InvalidRequest
        /// Could happen if Confirm Password and New Password does not match.
        /// #### **[-2]**: IdentityError
        /// This is a generic error code resembles something went wrong inside the Identity Framework and can be diagnosed using the response Messages list.
        /// #### **[-4]**: NotConfirmed
        /// Could happen if the email matching the provided token was not verified.
        /// #### **[401]**: Unauthorized
        /// Could happen if the provided token in the header has expired or is not valid.
        /// </remarks>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel request)
        {
            string currentlyLoggedInUserId = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            var response = await _accountBusiness.ChangePasswordAsync(currentlyLoggedInUserId, request);

            return Ok(response);
        }

        /// <summary>
        /// Helper method for checking authenticity
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// ## Main Functionality
        /// This method compares the provided token and email to check if the user is currently logged in.
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
        public async Task<IActionResult> ValidateToken()
        {
            var token = Request.Headers[HeaderNames.Authorization].ToString().Split(" ")[1];

            var response = await _accountBusiness.ValidateTokenAsync(token);

            return Ok(response);
        }

        /// <summary>
        /// Get information about the logged in account as a Personnel
        /// </summary>
        /// <remarks>
        /// ### Expected Result
        /// - Information about the Personnel Department
        /// - Information about the Personnel Authority Type
        ///
        /// *Note that Agents and Rescuers are both considered 'Personnel' and are the only people allowed to use this method.*
        ///
        /// </remarks>
        [HttpGet(ApiRoutes.Account.PersonnelBasicInfo)]
        [Authorize(Roles = Roles.ROLE_PERSONNEL + "," + Roles.ROLE_NURSE)]
        public async Task<IActionResult> GetBasicInfo()
        {
            string currentlyLoggedInUserId = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            var response = await _accountBusiness.GetBasicInfoAsync(currentlyLoggedInUserId);

            return Ok(response);
        }
    }
}