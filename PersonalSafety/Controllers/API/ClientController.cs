using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Business;
using PersonalSafety.Hubs;
using PersonalSafety.Hubs.HubTracker;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Hubs.Services;
using PersonalSafety.Models.ViewModels;

namespace PersonalSafety.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientController : ControllerBase
    {
        private readonly IClientBusiness _clientBusiness;
        private readonly ISOSBusiness _sosBusiness;

        public ClientController(IClientBusiness clientBusiness, ISOSBusiness sosBusiness)
        {
            _clientBusiness = clientBusiness;
            _sosBusiness = sosBusiness;
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
        /// Instantly log in with user's exsisting Facebook Account
        /// </summary>
        /// <remarks>
        /// ## Flow
        /// 1. Client Side user taps "Login with Facebook" button.
        /// 2. Client Side app finishes the heavy lifting of retrieving `access_token` **in the background** and saves it somewhere.
        /// 3. Client Side app sends a request to this method, ataching the `access_token` in the URI query.
        /// 4. If user was registered before, he is granted a JWT token, exactly similar to the one retrieved via `/Account/Login`
        ///     - If user was not registered before: the returned JWT token would be `null`, 
        ///     - Client Side app then utilizes `/RegisterViaFacebook` instead, also in the background and without user interaction.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-6]**: FacebookAuthError
        /// - Accesss Token is invalid or has expired
        /// </remarks>
        [AllowAnonymous]
        [HttpPost]
        [Route("Registration/[action]")]
        public async Task<IActionResult> LoginViaFacebook([FromQuery] string accessToken)
        {
            var authResponse = await _clientBusiness.LoginWithFacebookAsync(accessToken);

            if (authResponse.HasErrors)
            {
                return BadRequest(authResponse);
            }

            return Ok(authResponse);
        }

        /// <summary>
        /// Register user using his exsisting Facebook Account
        /// </summary>
        /// <remarks>
        /// ## Flow
        /// 
        /// *This method exsists because registration to our services require other sensitive information that doesn't simply exsist, like the user's National Id and his Phone Number.*
        /// 
        /// 1. Client Side app redirects user to a page similar to the ordinary registration form, except that, this page won't ask user about information other than his:
        ///     - `NationalId`
        ///     - `PhoneNumber`
        /// 2. Client Side user finishes the form and taps "submit"
        /// 3. Client Side app sends a request to this method, ataching the `access_token` in the body, along his other info.
        /// 4. Upon `Status=0` response, meaning user with was successfully registered, Client Side app sends a new request, again, in the background, to `/LoginViaFacebook` which should return a valid JWT token, exactly similar to the one retrieved from `/Account/Login`.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-6]**: FacebookAuthError
        /// - Accesss Token is invalid or has expired
        /// #### **[-1]**: Invalid Request
        /// - User exsists and has registered before
        /// - Someone with the same National ID has registered before
        /// - Someone with the same Phone Number has registered before
        /// </remarks>
        [AllowAnonymous]
        [HttpPost]
        [Route("Registration/[action]")]
        public async Task<IActionResult> RegisterViaFacebook([FromBody] RegistrationWithFacebookViewModel request)
        {
            var authResponse = await _clientBusiness.RegisterWithFacebookAsync(request);

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
        /// - User Birthday in format similar to: `yyyy-mm-dd`
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

        /// <summary>
        /// This method sends an SOS request for the current user.
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// ## Main Functionality
        /// User should be logged in so critical information about him could be assigned to the sent request. This action sends additional dynamic info about the user:
        /// - Authority Type:
        ///     - Police : 1
        ///     - Ambulance : 2
        ///     - Firefighting : 3
        ///     - TowTruck : 4
        /// - Request Location (Longitude and Latitude)
        /// - **(IMPORTANT)** ConnectionId: is an important string for tracking down the request evolution, new requests won't be sent unless a valid connection is established first with the server's SignalR endpoint.
        /// 
        /// #### **IMPORTANT:** These fields are not mandatory, and not providing any of the values will result in the request not being completed
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-1]**: InvalidRequest
        /// User failed to provide complete information.
        /// #### **[401]**: Unauthorized
        /// Could happen if the provided token in the header has expired or is not valid.
        /// </remarks>
        [HttpPost]
        [Route("SOS/[action]")]
        public async Task<IActionResult> SendSOSRequest([FromBody] SendSOSRequestViewModel request)
        {
            //? means : If value is not null, retrieve it
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _sosBusiness.SendSOSRequestAsync(currentlyLoggedInUserId, request);

            return Ok(response);
        }

        /// <summary>
        /// This method cancels an SOS request via its Id.
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// ## Main Functionality
        /// User must only provide the Id of the request he/she wants to mark it as canceled, additionally **if** his connection is still maintained, the connectionId will be removed from the tracker.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[404]**: Notfound
        /// Could happen if the provided Id does not match an exsisting request.
        /// </remarks>
        [HttpPut]
        [Route("SOS/[action]")]
        public async Task<IActionResult> CancelSOSRequest([FromQuery] int requestId)
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _sosBusiness.UpdateSOSRequestAsync(requestId, (int)StatesTypesEnum.Canceled, currentlyLoggedInUserId);

            return Ok(response);
        }

        /// <summary>
        /// Cancel any request that's waiting an outcome.
        /// </summary>
        /// <remarks>
        /// ### Functionality
        /// Searches for any pending request and cancels it. This unlocks the ability send new requests for clients.
        /// </remarks>
        [HttpPut]
        [Route("SOS/[action]")]
        public async Task<IActionResult> CancelPendingRequests()
        {
            string currentlyLoggedInUserId = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            var response = await _sosBusiness.CancelPendingRequests(currentlyLoggedInUserId);

            return Ok(response);
        }
    }
}