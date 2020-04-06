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
using PersonalSafety.Hubs.Services;
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
        private readonly IRescuerHub _rescuerHub;

        public AgentController(IAgentBusiness agentBusiness, ISOSBusiness sosBusiness, IClientHub clientHub, IRescuerHub rescuerHub)
        {
            _agentBusiness = agentBusiness;
            _sosBusiness = sosBusiness;
            _clientHub = clientHub;
            _rescuerHub = rescuerHub;
        }

        /// <summary>
        /// Retrieve details about the department of the current Agent
        /// </summary>
        /// <remarks>
        /// ### Functionality
        /// Retrieve basic data about the agent's data, including but not limited to:
        /// - Department Location
        /// - Department Authority type
        /// - Department Workers (Rescuers and Agents)
        /// </remarks>
        [HttpGet]
        [Route("Department/[action]")]
        public IActionResult GetDepartmentDetails()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = _agentBusiness.GetDepartmentDetails(currentlyLoggedInUserId);

            return Ok(response);
        }

        /// <summary>
        /// Registers a new rescuer and adds him to the current department.
        /// </summary>
        /// <remarks>
        /// ### Functionality
        ///
        /// Registers a new rescuer with basic data:
        ///
        /// - His Name
        /// - Email
        /// - Password
        ///
        /// The rescuer is automatically assigned to the same department of the agent registering him, and is assigned the same authority type too.
        /// </remarks>
        [HttpPost]
        [Route("Rescuer/[action]")]
        public async Task<IActionResult> RegisterRescuer(RegisterRescuerViewModel request)
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _agentBusiness.RegisterRescuersAsync(currentlyLoggedInUserId, request);

            return Ok(response);
        }

        /// <summary>
        /// Returns a list of online rescuers in the current department.
        /// </summary>
        /// <remarks>
        /// - **Online Rescuers:** Rescuers who are currently having a valid connection with the system's realtime hub.
        /// - Returned information also include the current job that is assigned to the user.
        ///     - Current Job is represented in the form of the `ID` of the SOSRequest.
        ///     - If the `ID = 0` then the rescuer is currently **Idling**
        ///     - If the `ID = x` then the rescuer is currently assigned the SOSRequest of `Id = x`
        /// </remarks>
        [HttpGet]
        [Route("Rescuer/[action]")]
        public IActionResult GetOnlineRescuers()
        {
            string currentlyLoggedInUserId = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            var response = _agentBusiness.GetDepartmentOnlineRescuers(currentlyLoggedInUserId);

            return Ok(response);
        }

        /// <summary>
        /// Returns a list of online rescuers who disconnected while being on a job.
        /// </summary>
        /// <remarks>
        /// - **Disconnected Rescuers:** Rescuers who were assigned a Job, but got **temporarily disconnected** from the system's realtime hub
        /// - Returned information also include the  job that was assigned to the user in the form of the `ID` of the SOSRequest.
        /// - **IMPORTANT NOTE:** Disconnected Rescuers **are not** offline rescuers.
        /// </remarks>
        [HttpGet]
        [Route("Rescuer/[action]")]
        public IActionResult GetDisconnectedRescuers()
        {
            string currentlyLoggedInUserId = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            var response = _agentBusiness.GetDepartmentDisconnectedRescuers(currentlyLoggedInUserId);

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
        public async Task<IActionResult> AcceptSOSRequest([FromQuery] int requestId, [FromQuery] string rescuerEmail)
        {
            string currentlyLoggedInUserId = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            var response = await _sosBusiness.UpdateSOSRequestAsync(requestId, (int)StatesTypesEnum.Accepted, currentlyLoggedInUserId, rescuerEmail??"");

            return Ok(response);
        }

        /// <summary>
        /// Resets a request to its initial state (Pending)
        /// </summary>
        /// <remarks>
        /// ### Functionality:
        /// 
        /// - Reset Request to Pending
        /// - Remove any Assigned Rescuers from SOSRequest Table
        /// - Make any Assigned Rescuers Idle - by finding them from database
        /// - Make any Assigned Rescuers Idle - by finding them from trackers
        /// - Fix Client tracker by:
        ///     * If he exist in a tracker (found by email) -> Assign the requestId back to him
        ///     * Else: pass
        ///
        /// </remarks>
        [HttpPut]
        [Route("SOS/[action]")]
        public async Task<IActionResult> ResetSOSRequest([FromQuery] int requestId)
        {
            var response = await _agentBusiness.ResetSOSRequest(requestId);

            return Ok(response);
        }
    }
}