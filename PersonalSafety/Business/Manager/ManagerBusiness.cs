using Microsoft.AspNetCore.Identity;
using PersonalSafety.Contracts;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels;
using PersonalSafety.Models.ViewModels.AdminVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Business
{
    public class ManagerBusiness : IManagerBusiness
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IPersonnelRepository _personnelRepository;
        private readonly ISOSRequestRepository _sosRequestRepository;
        private readonly IClientRepository _clientRepository;

        public ManagerBusiness(UserManager<ApplicationUser> userManager, IDepartmentRepository departmentRepository, IPersonnelRepository personnelRepository, ISOSRequestRepository sosRequestRepository, IClientRepository clientRepository)
        {
            _userManager = userManager;
            _departmentRepository = departmentRepository;
            _personnelRepository = personnelRepository;
            _sosRequestRepository = sosRequestRepository;
            _clientRepository = clientRepository;
        }

        public async Task<APIResponse<List<GetDepartmentDataViewModel>>> GetDepartmentsAsync(string userId)
        {
            APIResponse<List<GetDepartmentDataViewModel>> response = new APIResponse<List<GetDepartmentDataViewModel>>();
            var responseResult = new List<GetDepartmentDataViewModel>();

            var user = await _userManager.FindByIdAsync(userId);

            if(user == null)
            {
                response.Status = (int)APIResponseCodesEnum.Unauthorized;
                response.Messages.Add("You are not logged in");
                response.HasErrors = true;
                return response;
            }

            var departments = await GetListOfAllowedDepartmentsAsync(user);

            if(departments.Count() == 0)
            {
                response.Messages.Add("One of the below errors occured:");
                response.Messages.Add("You don't have enough previlages to view this content.");
                response.Messages.Add("There is no departments under your current distribution access.");
                response.HasErrors = true;
                return response;
            }

            foreach (var department in departments)
            {
                responseResult.Add(new GetDepartmentDataViewModel
                {
                    Id = department.Id,
                    DepartmentName = department.ToString(),
                    AuthorityType = department.AuthorityType,
                    AuthorityTypeName = ((AuthorityTypesEnum)department.AuthorityType).ToString(),
                    DistributionId = department.DistributionId,
                    DistributionName = department.Distribution.ToString(),
                    Longitude = department.Longitude,
                    Latitude = department.Latitude,
                    AgentsEmails = _personnelRepository.GetDepartmentAgentsEmails(department.Id),
                    RescuersEmails = _personnelRepository.GetDepartmentRescuersEmails(department.Id)
                });
            }

            response.Result = responseResult;
            return response;
        }

        public async Task<APIResponse<List<GetSOSRequestViewModel>>> GetDepartmentRequestsAsync(string userId, int departmentId, int? requestState, bool enforceClaims)
        {
            APIResponse<List<GetSOSRequestViewModel>> response = new APIResponse<List<GetSOSRequestViewModel>>();

            var user = await _userManager.FindByIdAsync(userId);
            Department department;

            if (enforceClaims)
            {
                var allowedDepartments = await GetListOfAllowedDepartmentsAsync(user);
                department = allowedDepartments.SingleOrDefault(d => d.Id == departmentId);

                if (department == null)
                {
                    response.Status = (int)APIResponseCodesEnum.Unauthorized;
                    response.Messages.Add("You don't have access to this department");
                    response.HasErrors = true;
                    return response;
                }
            }
            else
            {
                department = _departmentRepository.GetById(departmentId.ToString());
            }

            // Find SOS Requests related to the request
            IEnumerable<SOSRequest> requests = (requestState != null) ? _sosRequestRepository.GetRelevantRequests(department.AuthorityType, department.Id, (int)requestState)
                                                : _sosRequestRepository.GetRelevantRequests(department.AuthorityType, department.Id);

            List<GetSOSRequestViewModel> responseViewModel = new List<GetSOSRequestViewModel>();

            foreach (var request in requests)
            {
                ApplicationUser requestOwner_Account = await _userManager.FindByIdAsync(request.UserId);
                Client requestOwner_Client = _clientRepository.GetById(request.UserId);
                responseViewModel.Add(new GetSOSRequestViewModel
                {
                    RequestId = request.Id,

                    UserFullName = requestOwner_Account.FullName,
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

            response.Result = responseViewModel;
            return response;
        }

        private async Task<List<Department>> GetListOfAllowedDepartmentsAsync(ApplicationUser user)
        {
            var userAccessToDistribution = (await _userManager.GetClaimsAsync(user))
                                                .SingleOrDefault(c => c.Type == ClaimsStore.CLAIM_DISTRIBUTION_ACCESS);

            if (userAccessToDistribution == null)
            {
                return new List<Department>();
            }

            return _departmentRepository.GetAll(int.Parse(userAccessToDistribution.Value));
        }
    }
}
