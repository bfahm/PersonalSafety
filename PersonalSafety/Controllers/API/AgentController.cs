using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PersonalSafety.Business;
using PersonalSafety.Contracts;
using PersonalSafety.Hubs;
using PersonalSafety.Hubs.HubTracker;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Models.ViewModels;

namespace PersonalSafety.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.ROLE_AGENT)]
    public class AgentController : ControllerBase
    {
        private readonly IAgentBusiness _agentBusiness;
        private readonly ISOSBusiness _sosBusiness;
        private readonly IClientHub _clientHub;

        public AgentController(IAgentBusiness personnelBusiness, ISOSBusiness sosBusiness, IClientHub clientHub)
        {
            _agentBusiness = personnelBusiness;
            _sosBusiness = sosBusiness;
            _clientHub = clientHub;
        }

        [HttpPost]
        [Route("Rescuer/[action]")]
        public async Task<IActionResult> RegisterRescuer(RegisterRescuerViewModel request)
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _agentBusiness.RegisterRescuersAsync(currentlyLoggedInUserId, request);

            return Ok(response);
        }

        [HttpGet]
        [Route("Rescuer/[action]")]
        public IActionResult GetDepartmentDetails()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = _agentBusiness.GetDepartmentDetails(currentlyLoggedInUserId);

            return Ok(response);
        }

        /// <summary>
        /// This method returns a list of all Requests that relates to current Personnel Authority.
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// 
        /// ### Result is a list of objects that will have the following properties:
        /// - RequestId
        /// - UserEmail
        /// - UserPhoneNumber
        /// - UserNationalId
        /// - UserAge *(Automatically calculated from his birthday)*
        /// - UserBloodTypeId
        /// - UserBloodTypeName
        /// - UserMedicalHistoryNotes
        /// - UserSavedAddress *(Note that this should be different from the request longitude and latitude, as this just represent user's home address)*
        /// - RequestStateId *(0/1/2/3)*
        /// - RequestStateName *("Pending" / "Accepted" / "Solved" / "Canceled")*
        /// - RequestLocationLongitude
        /// - RequestLocationLatitude
        /// 
        /// **IMPORTANT NOTICE:** all requests are ordered by their state (Pending > Accepted > Solved > Canceled) and then by their creation date.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// *This method doesn't return any erros unless user is **UNAUTHORIZED***
        /// </remarks>
        [HttpGet]
        [Route("SOS/[action]")]
        public async Task<IActionResult> GetAllAuthorityRequests()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _agentBusiness.GetRelatedRequestsAsync(currentlyLoggedInUserId, null);

            return Ok(response);
        }

        /// <summary>
        /// This method returns a list of Pending Requests that relates to current Personnel Authority.
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// 
        /// ### *For returned list of object properties, see /GetAllAuthorityRequests documentation*
        /// 
        /// **IMPORTANT NOTICE:** all requests are ordered by their creation date.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// *This method doesn't return any erros unless user is **UNAUTHORIZED***
        /// </remarks>
        [HttpGet]
        [Route("SOS/[action]")]
        public async Task<IActionResult> GetAuthorityPendingRequests()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _agentBusiness.GetRelatedRequestsAsync(currentlyLoggedInUserId, (int)StatesTypesEnum.Pending);

            return Ok(response);
        }

        /// <summary>
        /// This method returns a list of Accepted Requests that relates to current Personnel Authority.
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// 
        /// ### *For returned list of object properties, see /GetAllAuthorityRequests documentation*
        /// 
        /// **IMPORTANT NOTICE:** all requests are ordered by their creation date.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// *This method doesn't return any erros unless user is **UNAUTHORIZED***
        /// </remarks>
        [HttpGet]
        [Route("SOS/[action]")]
        public async Task<IActionResult> GetAuthorityAcceptedRequests()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _agentBusiness.GetRelatedRequestsAsync(currentlyLoggedInUserId, (int)StatesTypesEnum.Accepted);

            return Ok(response);
        }

        /// <summary>
        /// This method returns a list of Solved Requests that relates to current Personnel Authority.
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// 
        /// ### *For returned list of object properties, see /GetAllAuthorityRequests documentation*
        /// 
        /// **IMPORTANT NOTICE:** all requests are ordered by their creation date.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// *This method doesn't return any erros unless user is **UNAUTHORIZED***
        /// </remarks>
        [HttpGet]
        [Route("SOS/[action]")]
        public async Task<IActionResult> GetAuthoritySolvedRequests()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _agentBusiness.GetRelatedRequestsAsync(currentlyLoggedInUserId, (int)StatesTypesEnum.Solved);

            return Ok(response);
        }

        /// <summary>
        /// This method returns a list of Canceled Requests that relates to current Personnel Authority.
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// 
        /// ### *For returned list of object properties, see /GetAllAuthorityRequests documentation*
        /// 
        /// **IMPORTANT NOTICE:** all requests are ordered by their creation date.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// *This method doesn't return any erros unless user is **UNAUTHORIZED***
        /// </remarks>
        [HttpGet]
        [Route("SOS/[action]")]
        public async Task<IActionResult> GetAuthorityCanceledRequests()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _agentBusiness.GetRelatedRequestsAsync(currentlyLoggedInUserId, (int)StatesTypesEnum.Canceled);

            return Ok(response);
        }

        /// <summary>
        /// This method accepts a requests using its Id.
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// 
        /// ## Main Functionality
        /// Gives a Personnel the ability to accept a request using its id.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// *This method doesn't return any erros unless user is **UNAUTHORIZED***
        /// </remarks>
        [HttpPut]
        [Route("SOS/[action]")]
        public async Task<IActionResult> AcceptSOSRequest([FromQuery] int requestId)
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _sosBusiness.UpdateSOSRequestAsync(requestId, (int)StatesTypesEnum.Accepted, currentlyLoggedInUserId);

            var notifierResult = _clientHub.NotifyUserSOSState(requestId, (int)StatesTypesEnum.Accepted);
            if (notifierResult)
            {
                return Ok(response);
            }

            response = await _sosBusiness.UpdateSOSRequestAsync(requestId, (int)StatesTypesEnum.Orphaned, currentlyLoggedInUserId);
            response.Messages.Add("Failed to notify the user about the change, it appears he lost the connection.");
            response.Messages.Add("Reverting Changes...");
            response.Messages.Add("Changes were reverted back to 'Orphaned'.");
            return Ok(response);
        }
    }
}