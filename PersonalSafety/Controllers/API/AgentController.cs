using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Business;
using PersonalSafety.Contracts;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Models.ViewModels;

namespace PersonalSafety.Controllers.API
{
    [ApiController]
    [Authorize(Roles = Roles.ROLE_AGENT)]
    public class AgentController : ControllerBase
    {
        private readonly IAgentBusiness _agentBusiness;
        private readonly ISOSBusiness _sosBusiness;

        public AgentController(IAgentBusiness agentBusiness, ISOSBusiness sosBusiness)
        {
            _agentBusiness = agentBusiness;
            _sosBusiness = sosBusiness;
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
        [HttpGet(ApiRoutes.Agent.Department)]
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
        ///
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-1]**: Invalid Request
        /// - User exists and has registered before
        /// 
        /// #### **[-2]**: Identity Error
        /// This is a generic error code resembles something went wrong inside the Identity Framework and can be diagnosed using the response Messages list.
        /// </remarks>
        [HttpPost(ApiRoutes.Agent.Resucer)]
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
        [HttpGet(ApiRoutes.Agent.Resucer)]
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
        [HttpGet(ApiRoutes.Agent.Resucer)]
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
        [HttpGet(ApiRoutes.Agent.SOS)]
        public async Task<IActionResult> GetAllRequests()
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
        [HttpGet(ApiRoutes.Agent.SOS)]
        public async Task<IActionResult> GetPendingRequests()
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
        [HttpGet(ApiRoutes.Agent.SOS)]
        public async Task<IActionResult> GetAcceptedRequests()
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
        [HttpGet(ApiRoutes.Agent.SOS)]
        public async Task<IActionResult> GetSolvedRequests()
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
        [HttpGet(ApiRoutes.Agent.SOS)]
        public async Task<IActionResult> GetCanceledRequests()
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
        /// Mark a request as Accepted and assign it to one of the online rescuers.
        /// 
        /// ## Possible Result Codes in case of Errors:
        ///
        /// #### **[400]**: Bad Request
        ///
        /// - Provided rescuer is not online.
        /// - Provided rescuer is already on a mission (not idle).
        /// #### **[401]**: Unauthorized
        /// Could happen if the provided token in the header has expired or is not valid.
        /// </remarks>
        [HttpPut(ApiRoutes.Agent.SOS)]
        public IActionResult AcceptSOSRequest([FromQuery] int requestId, [FromQuery] string rescuerEmail)
        {
            var response = _sosBusiness.AcceptSOSRequest(requestId, rescuerEmail??"");

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
        /// ## Possible Result Codes in case of Errors:
        /// This method doesn't return any errors per se, although it returns a list of messages that represents the outcome of each attempt in the above functionality.
        /// 
        /// </remarks>
        [HttpPut(ApiRoutes.Agent.SOS)]
        public IActionResult ResetSOSRequest([FromQuery] int requestId)
        {
            var response = _sosBusiness.ResetSOSRequest(requestId);

            return Ok(response);
        }
    }
}