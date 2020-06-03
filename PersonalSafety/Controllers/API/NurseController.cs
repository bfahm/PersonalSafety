using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Business.Nurse;
using PersonalSafety.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Controllers.API
{
    [ApiController]
    [Authorize(Roles = Roles.ROLE_NURSE)]
    public class NurseController : ControllerBase
    {

        private readonly INurseBusiness _nurseBusiness;

        public NurseController(INurseBusiness nurseBusiness)
        {
            _nurseBusiness = nurseBusiness;
        }

        /// <summary>
        /// Get the data for a particular user as a confirmation before marking him as a victim
        /// </summary>
        [HttpGet(ApiRoutes.Nurse.Main)]
        public async Task<IActionResult> GetClientDetails(string clientEmail)
        {
            var response = await _nurseBusiness.GetClientDetails(clientEmail);

            return Ok(response);
        }

        /// <summary>
        /// Mark a user as COVID victim
        /// </summary>
        /// <remarks>
        /// As a result of this action, all users that might have been in contact with this user would receive a
        /// push notification stating that they have to stay home, and that they might be susceptible of carrying the disease.
        /// </remarks>
        [HttpPut(ApiRoutes.Nurse.Main)]
        public async Task<IActionResult> MarkClientAsPositive(string clientEmail)
        {
            var response = await _nurseBusiness.EditClientVictimState(clientEmail, true);

            return Ok(response);
        }

        /// <summary>
        /// Unmark a user from being COVID victim (undo)
        /// </summary>
        /// <remarks>
        /// This action doesn't result in any push notifications and can be used for debugging purposes.
        /// </remarks>
        [HttpPut(ApiRoutes.Nurse.Main)]
        public async Task<IActionResult> MarkClientAsNegative(string clientEmail)
        {
            var response = await _nurseBusiness.EditClientVictimState(clientEmail, false);

            return Ok(response);
        }
    }
}
