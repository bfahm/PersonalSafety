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
using PersonalSafety.Services.Registration;

namespace PersonalSafety.Business
{
    public class AdminBusiness : IAdminBusiness
    {
        private readonly IRegistrationService _registrationService;

        public AdminBusiness(IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        public async Task<APIResponse<bool>> RegisterPersonnelAsync(RegisterPersonnelViewModel request)
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

            ApplicationUser newUser = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email,
                FullName = request.FullName,
                EmailConfirmed = true
            };

            Personnel personnel = new Personnel
            {
                PersonnelId = newUser.Id,
                AuthorityType = request.AuthorityType
            };

            return await _registrationService.RegisterNewUserAsync(newUser, request.Password, personnel);
        }
    }
}
