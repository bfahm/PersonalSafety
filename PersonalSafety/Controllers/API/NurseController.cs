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
        /// 
        /// </summary>
        /// <remarks>
        ///
        /// ### Flow:
        ///
        /// 
        /// </remarks>
        [HttpGet(ApiRoutes.Nurse.Main)]
        public async Task<IActionResult> GetClientDetails(string clientEmail)
        {
            var response = await _nurseBusiness.GetClientDetails(clientEmail);

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        ///
        /// ### Flow:
        ///
        /// 
        /// </remarks>
        [HttpPut(ApiRoutes.Nurse.Main)]
        public async Task<IActionResult> MarkClientAsPositive(string clientEmail)
        {
            var response = await _nurseBusiness.EditClientVictimState(clientEmail, true);

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        ///
        /// ### Flow:
        ///
        /// 
        /// </remarks>
        [HttpPut(ApiRoutes.Nurse.Main)]
        public async Task<IActionResult> MarkClientAsNegative(string clientEmail)
        {
            var response = await _nurseBusiness.EditClientVictimState(clientEmail, false);

            return Ok(response);
        }
    }
}
