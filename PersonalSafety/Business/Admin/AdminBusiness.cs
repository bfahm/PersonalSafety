using PersonalSafety.Models;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Hubs;
using PersonalSafety.Hubs.Helpers;
using PersonalSafety.Hubs.HubTracker;
using PersonalSafety.Models.ViewModels.AdminVM;
using PersonalSafety.Services;

namespace PersonalSafety.Business
{
    public class AdminBusiness : IAdminBusiness
    {
        private readonly IRegistrationService _registrationService;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IPersonnelRepository _personnelRepository;
        private readonly IAdminHub _adminHub;
        private readonly IHubTools _hubTools;

        public AdminBusiness(IRegistrationService registrationService, IDepartmentRepository departmentRepository, IPersonnelRepository personnelRepository, IAdminHub adminHub, IHubTools hubTools)
        {
            _registrationService = registrationService;
            _departmentRepository = departmentRepository;
            _personnelRepository = personnelRepository;
            _adminHub = adminHub;
            _hubTools = hubTools;
        }

        public APIResponse<List<GetDepartmentDataViewModel>> GetDepartments()
        {
            APIResponse<List<GetDepartmentDataViewModel>> response = new APIResponse<List<GetDepartmentDataViewModel>>();
            var responseResult = new List<GetDepartmentDataViewModel>();

            var departments = _departmentRepository.GetAll();

            foreach (var department in departments)
            {
                responseResult.Add(new GetDepartmentDataViewModel
                {
                    Id = department.Id,
                    AuthorityType = department.AuthorityType,
                    AuthorityTypeName = ((AuthorityTypesEnum)department.AuthorityType).ToString(),
                    City = department.City,
                    CityName = ((CitiesEnum)department.City).ToString(),
                    Longitude = department.Longitude,
                    Latitude = department.Latitude,
                    AgentsEmails = _personnelRepository.GetDepartmentAgentsEmails(department.Id),
                    RescuersEmails = _personnelRepository.GetDepartmentRescuersEmails(department.Id)
                });    
            }

            response.Result = responseResult;
            return response;
        }

        public async Task<APIResponse<bool>> RegisterAgentAsync(RegisterAgentViewModel request)
        {
            APIResponse<bool> response = new APIResponse<bool>();
            Department department;

            if (request.ExistingDepartmentId != 0)
            {
                department = _departmentRepository.GetById(request.ExistingDepartmentId.ToString());
                if (department == null)
                {
                    response.Messages.Add("The department id you provided was not found.");
                    response.Status = (int)APIResponseCodesEnum.NotFound;
                    response.HasErrors = true;
                    return response;
                }
            }
            else
            {
                // Check if provided authority type is valid
                if (!Enum.IsDefined(typeof(AuthorityTypesEnum), request.AuthorityType))
                {
                    response.Messages.Add("Department must be assigned to a valid authority type.");
                    response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                    response.HasErrors = true;
                    return response;
                }

                // Create department to put the agent in:
                department = new Department
                {
                    AuthorityType = request.AuthorityType,
                    Latitude = request.DepartmentLatitude,
                    Longitude = request.DepartmentLongitude,
                    City = request.DepartmentCity
                };

                _departmentRepository.Add(department);
                _departmentRepository.Save();
            }

            // Then create the agent:
            ApplicationUser newUser = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email,
                FullName = request.FullName,
                EmailConfirmed = true,
                ForceChangePassword = true
            };

            Personnel personnel = new Personnel
            {
                PersonnelId = newUser.Id,
                DepartmentId = department.Id
            };

            return await _registrationService.RegisterNewUserAsync(newUser, request.Password, personnel, Roles.ROLE_PERSONNEL, Roles.ROLE_AGENT);
        }

        public APIResponse<Dictionary<string, object>> RetrieveTrackers()
        {
            var trackerLists = typeof(TrackerHandler).GetFields().Where(f=>f.Name != "ConsoleSet");
            var trackerListsValues = new Dictionary<string, object>();
            foreach (var fieldInfo in trackerLists)
            {
                var value = fieldInfo.GetValue(typeof(TrackerHandler));
                if (value != null)
                {
                    trackerListsValues.Add(fieldInfo.Name, value);
                }
            }

            return new APIResponse<Dictionary<string, object>>
            {
                Result = trackerListsValues
            };
        }

        public APIResponse<object> RetrieveConsole()
        {
            var trackerLists = typeof(TrackerHandler).GetFields().Single(f => f.Name == "ConsoleSet")
                .GetValue(typeof(TrackerHandler));
            
            return new APIResponse<object>
            {
                Result = trackerLists
            };
        }

        public APIResponse<bool> ResetTrackers()
        {
            TrackerHandler.InitializeTrackers();
            _hubTools.PrintToConsole("Trackers Reset.");
            return new APIResponse<bool>
            {
                Result = true
            };
        }

        public APIResponse<bool> ResetConsole()
        {
            TrackerHandler.InitializeConsoleLog();
            return new APIResponse<bool>
            {
                Result = true
            };
        }

        public APIResponse<bool> ResetRescuerState(string rescuerEmail)
        {
            TrackerHandler.RescuerConnectionInfoSet.RemoveWhere(r => r.UserEmail == rescuerEmail);
            TrackerHandler.RescuerWithPendingMissionsSet.RemoveWhere(r => r.UserEmail == rescuerEmail);
            TrackerHandler.AllConnectionInfoSet.RemoveWhere(r => r.UserEmail == rescuerEmail);
            _hubTools.PrintToConsole(rescuerEmail, "was forced offline.");
            return new APIResponse<bool>
            {
                Result = true
            };
        }

        public APIResponse<bool> ResetClientState(string clientEmail)
        {
            TrackerHandler.ClientConnectionInfoSet.RemoveWhere(r => r.UserEmail == clientEmail);
            TrackerHandler.AllConnectionInfoSet.RemoveWhere(r => r.UserEmail == clientEmail);
            _hubTools.PrintToConsole(clientEmail, "was forced offline.");
            return new APIResponse<bool>
            {
                Result = true
            };
        }
    }
}
