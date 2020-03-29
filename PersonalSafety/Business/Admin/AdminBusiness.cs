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
using PersonalSafety.Services;

namespace PersonalSafety.Business
{
    public class AdminBusiness : IAdminBusiness
    {
        private readonly IRegistrationService _registrationService;
        private readonly IDepartmentRepository _departmentRepository;

        public AdminBusiness(IRegistrationService registrationService, IDepartmentRepository departmentRepository)
        {
            _registrationService = registrationService;
            _departmentRepository = departmentRepository;
        }

        public async Task<APIResponse<bool>> RegisterAgentAsync(RegisterAgentViewModel request)
        {
            // Check if provided authority type is valid
            if (!Enum.IsDefined(typeof(AuthorityTypesEnum), request.AuthorityType))
            {
                APIResponse<bool> response = new APIResponse<bool>();
                response.Messages.Add("User must be assigned to a valid department.");
                response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                response.HasErrors = true;
                return response;
            }

            // Create department to put the agent in:
            Department department = new Department
            {
                AuthorityType = request.AuthorityType,
                Latitude = request.DepartmentLatitude,
                Longitude = request.DepartmentLongitude
            };

            _departmentRepository.Add(department);
            _departmentRepository.Save();

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

            return await _registrationService.RegisterNewUserAsync(newUser, request.Password, personnel);
        }
    }
}
