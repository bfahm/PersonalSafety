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
using Microsoft.EntityFrameworkCore;

namespace PersonalSafety.Business
{
    public class AdminBusiness : IAdminBusiness
    {
        private readonly IRegistrationService _registrationService;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IPersonnelRepository _personnelRepository;
        private readonly IDistributionRepository _distributionRepository;
        private readonly IHubTools _hubTools;

        public AdminBusiness(IRegistrationService registrationService, IDepartmentRepository departmentRepository, IPersonnelRepository personnelRepository, IDistributionRepository distributionRepository, IHubTools hubTools)
        {
            _registrationService = registrationService;
            _departmentRepository = departmentRepository;
            _personnelRepository = personnelRepository;
            _distributionRepository = distributionRepository;
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

                if (!_distributionRepository.IsCity(request.DistributionId))
                {
                    response.Messages.Add("Error: The provided distribution ID is not a leaf in the node, you must provide a city Id.");
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
                    DistributionId = request.DistributionId
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

        public APIResponse<DistributionTreeViewModel> GetDistributionTree()
        {
            APIResponse<DistributionTreeViewModel> response = new APIResponse<DistributionTreeViewModel>();
            var root = _distributionRepository.GetRootDistribution();
            response.Result = _distributionRepository.GetDistributionTree(root?.Id??0, true);
            return response;
        }

        public APIResponse<DistributionTreeViewModel> AddNewDistribution(NewDistributionRequestViewModel request)
        {
            APIResponse<DistributionTreeViewModel> response = new APIResponse<DistributionTreeViewModel>();
            
            if (!_distributionRepository.DoesNodeExist(request.ParentId))
            {
                response.Messages.Add("The provided Parent Node does not exist.");
                response.Status = (int)APIResponseCodesEnum.NotFound;
                response.HasErrors = true;
                return response;
            }

            _distributionRepository.Add(new Distribution
            {
                ParentId = request.ParentId,
                // Automatically calculate the type of the child by knowing the number of the parents of the proivided heritage:
                // Adding "1" to get the type of the child.
                Type = _distributionRepository.GetNumberOfParents(request.ParentId) + 1, 
                Value = request.Value
            });

            _distributionRepository.Save();

            response.Result = _distributionRepository.GetDistributionTree(request.ParentId, false);
            response.Messages.Add("Success, here is the new state of the provided Parent node.");
            response.Messages.Add("Retrieve the children nodes using /GetDistributionTree");

            return response;
        }

        public APIResponse<DistributionTreeViewModel> RenameDistribution(RenameDistributionRequestViewModel request)
        {
            APIResponse<DistributionTreeViewModel> response = new APIResponse<DistributionTreeViewModel>();

            if (!_distributionRepository.DoesNodeExist(request.Id))
            {
                response.Messages.Add("The provided Node does not exist.");
                response.Status = (int)APIResponseCodesEnum.NotFound;
                response.HasErrors = true;
                return response;
            }

            var distToBeUpdated = _distributionRepository.GetById(request.Id.ToString());
            distToBeUpdated.Value = request.Value;


            _distributionRepository.Update(distToBeUpdated);
            _distributionRepository.Save();

            response.Result = _distributionRepository.GetDistributionTree(distToBeUpdated.Id, false);
            response.Messages.Add("Success, here is the new state of the provided node.");
            response.Messages.Add("Retrieve the children nodes using /GetDistributionTree");

            return response;
        }
    }
}
