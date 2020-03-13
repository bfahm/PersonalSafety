using PersonalSafety.Helpers;
using PersonalSafety.Models;
using PersonalSafety.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Business
{
    public class SOSBusiness : ISOSBusiness
    {
        private readonly ISOSRequestRepository _sosRequestRepository;

        public SOSBusiness(ISOSRequestRepository sosRequestRepository)
        {
            _sosRequestRepository = sosRequestRepository;
        }

        public APIResponse<bool> UpdateSOSRequest(int requestId, int newStatus)
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
