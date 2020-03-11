using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Business;
using PersonalSafety.Models.Enums;
using PersonalSafety.Models.ViewModels;

namespace PersonalSafety.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientController : ControllerBase
    {
        private readonly IAccountBusiness _accountBusiness;
        private readonly IClientBusiness _clientBusiness;

        public ClientController(IAccountBusiness accountBusiness, IClientBusiness clientBusiness)
        {
            _accountBusiness = accountBusiness;
            _clientBusiness = clientBusiness;
        }

        /// <summary>
        /// Create a new client account to be able to access his services.
        /// </summary>
        /// <remarks>
        /// ## Main Functionality
        /// All the JSON object values **are** required and must follow these rules:
        /// 
        /// - **Email** : must be unique and not used before, additionally it must follow the correct email structure
        /// - **Password** : must be complex, contain number, symbols, Capital and Small letters
        /// - **NationalId** : must be exactly of 14 digits
        /// - **PhoneNumber** : must be exactly of 11 digits
        /// - **FullName** : Must be non null
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
        [AllowAnonymous]
        [HttpPost]
        [Route("Registration/[action]")]
        public async Task<IActionResult> Register([FromBody] RegistrationViewModel request)
        {
            var authResponse = await _clientBusiness.RegisterAsync(request);

            if (authResponse.HasErrors)
            {
                return BadRequest(authResponse);
            }

            return Ok(authResponse);
        }

        /// <summary>
        /// This method returns a list of the current user emergency contacts
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// ## Main Functionality
        /// This method should be called before `Api/User/CompleteProfile` or any other calls that shows the list to the user before being able to modify them.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// *This method doesn't return any erros unless user is **UNAUTHORIZED***
        /// </remarks>
        [HttpGet]
        [Route("Registration/[action]")]
        public IActionResult GetEmergencyInfo()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = _clientBusiness.GetEmergencyInfo(currentlyLoggedInUserId);

            return Ok(response);
        }

        /// <summary>
        /// This method updates the basic information needed for the currently logged in user.
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// ## Main Functionality
        /// End-user can update his:
        /// - Current Address
        /// - Blood Type
        ///     -  O = 1
        ///     -  A = 2
        ///     -  B = 3
        ///     -  AB = 4
        /// - Medical Notes (and history)
        /// 
        /// #### **IMPORTANT:** These fields are not mandatory, and not providing any values for them will result in the fields not updated. So users can only provide data to which they want to update.
        /// 
        /// User could also modify his current Emergency Contacts **by providing new ones here**.
        /// ####**IMPORTANT:** New emergency contacts replace old ones, so there is no need for separate delete/update calls for the sake of simplicity. Just provide the new setup and it will replace the old one.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-2]**: IdentityError
        /// This is a generic error code resembles something went wrong inside the Identity Framework and can be diagnosed using the response Messages list.
        /// #### **[-4]**: NotConfrimed
        /// Could happen if the email matching the provided token was not verified.
        /// #### **[401]**: Unauthorized
        /// Could happen if the provided token in the header has expired or is not valid.
        /// </remarks>
        [HttpPut]
        [Route("Registration/[action]")]
        public IActionResult CompleteProfile([FromBody] CompleteProfileViewModel request)
        {
            //? means : If value is not null, retrieve it
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = _clientBusiness.CompleteProfile(currentlyLoggedInUserId, request);

            return Ok(response);
        }

        [HttpPut]
        [Route("SOS/[action]")]
        public IActionResult SendSOSRequest([FromBody] SendSOSRequestViewModel request)
        {
            //? means : If value is not null, retrieve it
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = _clientBusiness.SendSOSRequest(currentlyLoggedInUserId, request);

            return Ok(response);
        }
    }
}