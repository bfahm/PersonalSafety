using PersonalSafety.Models;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Models.ViewModels;
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
        private readonly IRegistrationService _registrationService;
        private readonly IManagerBusiness _managerBusiness;

        public AgentBusiness(IPersonnelRepository personnelRepository, IRegistrationService registrationService, IManagerBusiness managerBusiness)
        {
            _personnelRepository = personnelRepository;
            _registrationService = registrationService;
            _managerBusiness = managerBusiness;
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
                DistributionId = currentAgentDepartment.DistributionId,
                DistributionName = currentAgentDepartment.Distribution.ToString(),
                AgentsEmails = _personnelRepository.GetDepartmentAgentsEmails(currentAgentDepartment.Id),
                RescuerEmails = _personnelRepository.GetDepartmentRescuersEmails(currentAgentDepartment.Id)
            };

            return new APIResponse<DepartmentDetailsViewModel> { Result = dptDetails };
        }

        public async Task<APIResponse<bool>> RegisterRescuersAsync(string userId, RegisterRescuerViewModel rescuer)
        {
            var currentAgent = _personnelRepository.GetById(userId);

            ApplicationUser newUser = new ApplicationUser
            {
                Email = rescuer.Email,
                UserName = rescuer.Email,
                FullName = rescuer.FirstName + " " + rescuer.LastName,
                EmailConfirmed = true,
                ForceChangePassword = true
            };

            Personnel personnel = new Personnel
            {
                PersonnelId = newUser.Id,
                DepartmentId = currentAgent.DepartmentId,
                IsRescuer = true
            };

            return await _registrationService.RegisterWorkingEntityAsync(newUser, rescuer.Password, () => _personnelRepository.Add(personnel), new string[] { Roles.ROLE_PERSONNEL, Roles.ROLE_RESCUER }, null);
        }

        public APIResponse<List<RescuerConnectionInfo>> GetDepartmentOnlineRescuers(string userId)
        {
            var currentAgentDepartment = _personnelRepository.GetPersonnelDepartment(userId);
            var result = TrackerHandler.RescuerConnectionInfoSet.Where(r => r.DepartmentId == currentAgentDepartment.Id)
                .ToList();

            return new APIResponse<List<RescuerConnectionInfo>>
            {
                Result = result,
                Messages = new List<string> { "Number of current online rescuers in your department is: " + result.Count()}
            };
        }

        public APIResponse<List<RescuerConnectionInfo>> GetDepartmentDisconnectedRescuers(string userId)
        {
            var currentAgentDepartment = _personnelRepository.GetPersonnelDepartment(userId);

            var result = TrackerHandler.RescuerWithPendingMissionsSet.Where(r => r.DepartmentId == currentAgentDepartment.Id)
                .ToList();

            return new APIResponse<List<RescuerConnectionInfo>>
            {
                Result = result,
                Messages = new List<string> { "Number of rescuers who went offline in your department is: " + result.Count }
            };
        }

        public async Task<APIResponse<List<GetSOSRequestViewModel>>> GetRequestsByStateAsync(string userId, int? requestState)
        {
            // Get current agent department
            var currentAgentDepartment = _personnelRepository.GetPersonnelDepartment(userId);

            return await _managerBusiness.GetDepartmentRequestsAsync(userId, currentAgentDepartment.Id, requestState, false);
        }
    }
}
