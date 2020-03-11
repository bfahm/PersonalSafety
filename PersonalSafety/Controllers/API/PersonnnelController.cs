using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Business;
using PersonalSafety.Models.Enums;

namespace PersonalSafety.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Personnel")]
    public class PersonnnelController : ControllerBase
    {
        private readonly IPersonnelBusiness _personnelBusiness;

        public PersonnnelController(IPersonnelBusiness personnelBusiness)
        {
            _personnelBusiness = personnelBusiness;
        }

        /// <summary>
        /// This method returns a list of all Requests that relates to current Personnel Authority.
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// 
        /// ### Result is a list of objects that will have the following properties:
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
        /// ## Possible Result Codes in case of Errors:
        /// *This method doesn't return any erros unless user is **UNAUTHORIZED***
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetAllAuthorityRequests()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _personnelBusiness.GetRelatedRequestsAsync(currentlyLoggedInUserId, null);

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
        /// ## Possible Result Codes in case of Errors:
        /// *This method doesn't return any erros unless user is **UNAUTHORIZED***
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetAuthorityPendingRequests()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _personnelBusiness.GetRelatedRequestsAsync(currentlyLoggedInUserId, (int)StatesTypesEnum.Pending);

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
        /// ## Possible Result Codes in case of Errors:
        /// *This method doesn't return any erros unless user is **UNAUTHORIZED***
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetAuthorityAcceptedRequests()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _personnelBusiness.GetRelatedRequestsAsync(currentlyLoggedInUserId, (int)StatesTypesEnum.Accepted);

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
        /// ## Possible Result Codes in case of Errors:
        /// *This method doesn't return any erros unless user is **UNAUTHORIZED***
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetAuthoritySolvedRequests()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _personnelBusiness.GetRelatedRequestsAsync(currentlyLoggedInUserId, (int)StatesTypesEnum.Solved);

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
        /// ## Possible Result Codes in case of Errors:
        /// *This method doesn't return any erros unless user is **UNAUTHORIZED***
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetAuthorityCanceledRequests()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _personnelBusiness.GetRelatedRequestsAsync(currentlyLoggedInUserId, (int)StatesTypesEnum.Canceled);

            return Ok(response);
        }
    }
}