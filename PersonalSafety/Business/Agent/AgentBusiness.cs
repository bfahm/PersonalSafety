using Microsoft.AspNetCore.Identity;
using PersonalSafety.Options;
using PersonalSafety.Models;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Hubs.HubTracker;
using PersonalSafety.Services;

namespace PersonalSafety.Business
{
    public class AgentBusiness : IAgentBusiness
    {
        private readonly IPersonnelRepository _personnelRepository;
        private readonly ISOSRequestRepository _sosRequestRepository;
        private readonly IClientRepository _clientRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRegistrationService _registrationService;

        public AgentBusiness(IPersonnelRepository personnelRepository, ISOSRequestRepository sosRequestRepository, IClientRepository clientRepository, UserManager<ApplicationUser> userManager, IRegistrationService registrationService)
        {
            _personnelRepository = personnelRepository;
            _sosRequestRepository = sosRequestRepository;
            _clientRepository = clientRepository;
            _userManager = userManager;
            _registrationService = registrationService;
        }

        public async Task<APIResponse<List<GetSOSRequestViewModel>>> GetRelatedRequestsAsync(string userId, int? requestState)
        {
            // Get current personnel authority type
            int authorityTypeInt = _personnelRepository.GetPersonnelAuthorityTypeInt(userId);

            // Find SOS Requests related to the request
            
            IEnumerable<SOSRequest> requests = (requestState != null) ? _sosRequestRepository.GetRelevantRequests(authorityTypeInt, (int)requestState)
                                                : _sosRequestRepository.GetRelevantRequests(authorityTypeInt);

            List <GetSOSRequestViewModel> responseViewModel = new List<GetSOSRequestViewModel>();

            foreach(var request in requests)
            {
                ApplicationUser requestOwner_Account = await _userManager.FindByIdAsync(request.UserId);
                Client requestOwner_Client = _clientRepository.GetById(request.UserId);
                responseViewModel.Add(new GetSOSRequestViewModel
                {
                    RequestId = request.Id,

                    UserEmail = requestOwner_Account.Email,
                    UserPhoneNumber = requestOwner_Account.PhoneNumber,
                    UserNationalId = requestOwner_Client.NationalId,
                    UserAge = DateTime.Today.Year - requestOwner_Client.Birthday.Year,
                    UserBloodTypeId = requestOwner_Client.BloodType,
                    UserBloodTypeName = ((BloodTypesEnum)requestOwner_Client.BloodType).ToString(),
                    UserMedicalHistoryNotes = requestOwner_Client.MedicalHistoryNotes,
                    UserSavedAddress = requestOwner_Client.CurrentAddress,

                    RequestStateId = request.State,
                    RequestStateName = ((StatesTypesEnum)request.State).ToString(),
                    RequestLocationLatitude = request.Latitude,
                    RequestLocationLongitude = request.Longitude,
                    RequestCreationDate = request.CreationDate,
                    RequestLastModified = request.LastModified
                });
            }

            APIResponse<List<GetSOSRequestViewModel>> response = new APIResponse<List<GetSOSRequestViewModel>>
            {
                Result = responseViewModel,
                HasErrors = false,
                Status = 0,
                Messages = null
            };


            return response;
        }

        public async Task<APIResponse<bool>> RegisterRescuersAsync(string userId, RegisterRescuerViewModel rescuer)
        {
            var currentAgent = _personnelRepository.GetById(userId);

            ApplicationUser newUser = new ApplicationUser
            {
                Email = rescuer.Email,
                UserName = rescuer.Email,
                FullName = rescuer.Email,
                EmailConfirmed = true,
                ForceChangePassword = true
            };

            Personnel personnel = new Personnel
            {
                PersonnelId = newUser.Id,
                DepartmentId = currentAgent.DepartmentId,
                IsRescuer = true
            };

            return await _registrationService.RegisterNewUserAsync(newUser, rescuer.Password, personnel, Roles.ROLE_PERSONNEL, Roles.ROLE_RESCUER);
        }

        public APIResponse<DepartmentDetailsViewModel> GetDepartmentDetails(string userId)
        {
            var currentAgent = _personnelRepository.GetById(userId);
            var currentAgentDepartment = _personnelRepository.GetPersonnelDepartment(userId);


            var dptDetails = new DepartmentDetailsViewModel
            {
                DepartmentId = currentAgent.DepartmentId,
                AuthorityTypeId = currentAgentDepartment.Id,
                AuthorityTypeName = ((AuthorityTypesEnum)currentAgentDepartment.Id).ToString(),
                DepartmentLongitude = currentAgentDepartment.Longitude,
                DepartmentLatitude = currentAgentDepartment.Latitude,
                AgentsEmails = _personnelRepository.GetDepartmentAgentsEmails(currentAgentDepartment.Id),
                RescuerEmails = _personnelRepository.GetDepartmentRescuersEmails(currentAgentDepartment.Id)
            };

            return new APIResponse<DepartmentDetailsViewModel> { Result = dptDetails };
        }

        public APIResponse<HashSet<RescuerConnectionInfo>> GetDepartmentOnlineRescuers(string userId)
        {
            var currentAgent = _personnelRepository.GetById(userId);
            var currentAgentDepartment = _personnelRepository.GetPersonnelDepartment(userId);
            return new APIResponse<HashSet<RescuerConnectionInfo>>
            {
                Result = TrackerHandler.RescuerConnectionInfoSet,
                Messages = new List<string> { "Number of current online rescuers in your department is: " + TrackerHandler.RescuerConnectionInfoSet.Count}
            };
        }

        public APIResponse<HashSet<RescuerConnectionInfo>> GetDepartmentDisconnectedRescuers(string userId)
        {
            var currentAgent = _personnelRepository.GetById(userId);
            var currentAgentDepartment = _personnelRepository.GetPersonnelDepartment(userId);
            return new APIResponse<HashSet<RescuerConnectionInfo>>
            {
                Result = TrackerHandler.RescuerWithPendingMissionsSet,
                Messages = new List<string> { "Number of rescuers who went offline in your department is: " + TrackerHandler.RescuerWithPendingMissionsSet.Count }
            };
        }
    }
}
