using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PersonalSafety.Business;
using PersonalSafety.Helpers;
using PersonalSafety.Hubs;
using PersonalSafety.Hubs.HubTracker;
using PersonalSafety.Models.Enums;
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
        private readonly ISOSRealtimeHelper _sosRealtimeHelper;
        private readonly IPersonnelHub _personnelHub;

        public ClientController(IClientBusiness clientBusiness, ISOSBusiness sosBusiness, ISOSRealtimeHelper sosRealtimeHelper, IPersonnelHub personnelHub)
        {
            _clientBusiness = clientBusiness;
            _sosBusiness = sosBusiness;
            _sosRealtimeHelper = sosRealtimeHelper;
            _personnelHub = personnelHub;
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

            var response = await _clientBusiness.SendSOSRequestAsync(currentlyLoggedInUserId, request);

            if (!response.HasErrors)
            {
                _sosRealtimeHelper.NotifyUserSOSState(response.Result.RequestId, (int)StatesTypesEnum.Pending);
                await _personnelHub.NotifyNewChanges(response.Result.RequestId, (int)StatesTypesEnum.Pending);
            }
            
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
        public IActionResult CancelSOSRequest([FromQuery] int requestId)
        {
            var response = _sosBusiness.UpdateSOSRequest(requestId, (int)StatesTypesEnum.Canceled);

            // Notify user about the change
            var notifierResult = _sosRealtimeHelper.NotifyUserSOSState(requestId, (int)StatesTypesEnum.Canceled);
            _personnelHub.NotifyNewChanges(requestId, (int)StatesTypesEnum.Canceled);
            if (notifierResult)
            {
                // Unsubscribe user from future notifications
                int removed = SOSHandler.SOSInfoSet.RemoveWhere(r => r.SOSId == requestId);

                if (removed == 0)
                {
                    response.Messages.Add("Failed to remove user from the tracker, it appears that the request was corrupt.");
                }
                return Ok(response);
            }

            
            var fallback_response = _sosBusiness.UpdateSOSRequest(requestId, (int)StatesTypesEnum.Orphaned);
            fallback_response.Messages.Add("Orphan request detected.");
            fallback_response.Messages.Add("Failed to notify the user about the change, it appears he lost the connection.");

            return Ok(fallback_response);



        }
    }
}