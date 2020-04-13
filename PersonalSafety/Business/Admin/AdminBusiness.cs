using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PersonalSafety.Models;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels.AdminVM;
using PersonalSafety.Services;

namespace PersonalSafety.Business
{
    public class AdminBusiness : IAdminBusiness
    {
        private readonly IRegistrationService _registrationService;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IPersonnelRepository _personnelRepository;

        public AdminBusiness(IRegistrationService registrationService, IDepartmentRepository departmentRepository, IPersonnelRepository personnelRepository)
        {
            _registrationService = registrationService;
            _departmentRepository = departmentRepository;
            _personnelRepository = personnelRepository;
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
    }
}
