using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PersonalSafety.Contracts;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Hubs;
using PersonalSafety.Hubs.HubTracker;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels;

namespace PersonalSafety.Business
{
    public class RescuerBusiness : IRescuerBusiness
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISOSRequestRepository _sosRequestRepository;
        private readonly IClientRepository _clientRepository;
        private readonly ISOSBusiness _sosBusiness;

        public RescuerBusiness(UserManager<ApplicationUser> userManager, ISOSRequestRepository sosRequestRepository, IClientRepository clientRepository, ISOSBusiness sosBusiness)
        {
            _userManager = userManager;
            _sosRequestRepository = sosRequestRepository;
            _clientRepository = clientRepository;
            _sosBusiness = sosBusiness;
        }

        public async Task<APIResponse<GetSOSRequestViewModel>> GetSOSRequestDetailsAsync(string userId, int requestId)
        {
            APIResponse<GetSOSRequestViewModel> response = new APIResponse<GetSOSRequestViewModel>();

            SOSRequest sosRequest = _sosRequestRepository.GetById(requestId.ToString());

            var validationResult = ValidateAccessToRequest(userId, requestId);
            if (validationResult != null)
            {
                response.WrapResponseData(validationResult);
                return response;
            }

            ApplicationUser requestOwner_Account = await _userManager.FindByIdAsync(sosRequest.UserId);
            Client requestOwner_Client = _clientRepository.GetById(sosRequest.UserId);
            GetSOSRequestViewModel responseViewModel = new GetSOSRequestViewModel
            {
                RequestId = sosRequest.Id,

                UserEmail = requestOwner_Account.Email,
                UserPhoneNumber = requestOwner_Account.PhoneNumber,
                UserNationalId = requestOwner_Client.NationalId,
                UserAge = DateTime.Today.Year - requestOwner_Client.Birthday.Year,
                UserBloodTypeId = requestOwner_Client.BloodType,
                UserBloodTypeName = ((BloodTypesEnum) requestOwner_Client.BloodType).ToString(),
                UserMedicalHistoryNotes = requestOwner_Client.MedicalHistoryNotes,
                UserSavedAddress = requestOwner_Client.CurrentAddress,

                RequestStateId = sosRequest.State,
                RequestStateName = ((StatesTypesEnum) sosRequest.State).ToString(),
                RequestLocationLatitude = sosRequest.Latitude,
                RequestLocationLongitude = sosRequest.Longitude,
                RequestCreationDate = sosRequest.CreationDate,
                RequestLastModified = sosRequest.LastModified
            };

            response.Result = responseViewModel;
            return response;
        }

        private APIResponseData ValidateAccessToRequest(string userId, int requestId)
        {
            SOSRequest sosRequest = _sosRequestRepository.GetById(requestId.ToString());

            if (sosRequest == null)
            {
                return new APIResponseData((int)APIResponseCodesEnum.NotFound, new List<string> { "The requested SOS Request could not be found. Make sure you are using the correct Id." });
            }

            if (sosRequest.State != (int)StatesTypesEnum.Accepted)
            {
                return new APIResponseData((int)APIResponseCodesEnum.BadRequest, new List<string> { "This SOS Request was not accepted yet by your manager." });
            }

            if (sosRequest.AssignedRescuerId != userId)
            {
                return new APIResponseData((int)APIResponseCodesEnum.Unauthorized, new List<string> { "This SOS Request was not assigned to you." });
            }

            return null;
        }
    }
}
