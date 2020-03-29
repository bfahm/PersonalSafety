using PersonalSafety.Options;
using PersonalSafety.Models;
using PersonalSafety.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using Microsoft.AspNetCore.Identity;

namespace PersonalSafety.Business
{
    public class SOSBusiness : ISOSBusiness
    {
        private readonly ISOSRequestRepository _sosRequestRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public SOSBusiness(ISOSRequestRepository sosRequestRepository, UserManager<ApplicationUser> userManager)
        {
            _sosRequestRepository = sosRequestRepository;
            _userManager = userManager;
        }

        public async Task<APIResponse<bool>> UpdateSOSRequestAsync(int requestId, int newStatus, string issuerId)
        {
            APIResponse<bool> response = new APIResponse<bool>();
            SOSRequest sosRequest = _sosRequestRepository.GetById(requestId.ToString());

            if (sosRequest == null)
            {
                response.Messages.Add("The SOS Request you are trying to modify was not found, did you mistype the ID?");
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.NotFound;
                return response;
            }

            // If user is not a personnel (meaning he's a client), he must only modify his own requests.
            if(!await _userManager.IsInRoleAsync(await _userManager.FindByIdAsync(issuerId), Roles.ROLE_PERSONNEL))
            {
                if(sosRequest.UserId != issuerId)
                {
                    response.Messages.Add("You are not authorized to modify this SOS Request.");
                    response.HasErrors = true;
                    response.Status = (int)APIResponseCodesEnum.Unauthorized;
                    return response;
                }
            }

            sosRequest.State = newStatus;
            sosRequest.LastModified = DateTime.Now;
            _sosRequestRepository.Update(sosRequest);
            _sosRequestRepository.Save();

            response.Messages.Add("Success!");
            response.Messages.Add("SOS Request of Id: " + requestId + " was modified.");
            response.HasErrors = false;
            return response;
        }
    }
}
