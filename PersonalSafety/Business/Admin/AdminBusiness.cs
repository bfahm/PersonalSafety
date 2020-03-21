using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PersonalSafety.Business;
using PersonalSafety.Options;
using PersonalSafety.Models;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Helpers;

namespace PersonalSafety.Business
{
    public class AdminBusiness : IAdminBusiness
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPersonnelRepository _personnelRepository;
        private readonly IOptions<AppSettings> _appSettings;

        public AdminBusiness(UserManager<ApplicationUser> userManager, IPersonnelRepository personnelRepository, IOptions<AppSettings> appSettings)
        {
            _userManager = userManager;
            _personnelRepository = personnelRepository;
            _appSettings = appSettings;
        }

        public async Task<APIResponse<bool>> RegisterPersonnelAsync(RegisterPersonnelViewModel request)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            ApplicationUser exsistingUserFoundByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (exsistingUserFoundByEmail != null)
            {
                response.Messages.Add("User with this email address already exsists.");
                response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                response.HasErrors = true;
                return response;
            }

            ApplicationUser newUser = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email,
                FullName = request.FullName
            };

            // Check if provided authority type is valid
            if(!Enum.IsDefined(typeof(AuthorityTypesEnum), request.AuthorityType))
            {
                response.Messages.Add("User must be assigned to a valid department.");
                response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                response.HasErrors = true;
                return response;
            }

            Personnel personnel = new Personnel
            {
                PersonnelId = newUser.Id,
                AuthorityType = request.AuthorityType
            };

            _personnelRepository.Add(personnel);

            //_personnelRepository.Add(client) still needs saving, but will be done automatically in the below line.
            var creationResultForAccount = await _userManager.CreateAsync(newUser, request.Password);
            var addToRoleResult = await _userManager.AddToRoleAsync(newUser, "Personnel");

            if (!creationResultForAccount.Succeeded || !addToRoleResult.Succeeded)
            {
                response.Messages = creationResultForAccount.Errors.Select(e => e.Description).ToList();
                response.Status = (int)APIResponseCodesEnum.IdentityError;
                response.HasErrors = true;
                return response;
            }

            AccountBusiness accountBusiness = new AccountBusiness(_userManager, _appSettings);
            var confirmationMailResult = await accountBusiness.SendConfirmMailAsync(request.Email);

            response.Messages.Add("Successfully created a new user with email " + request.Email);
            response.Messages.Add("Please check your email for activation links before you continue.");
            response.Messages.AddRange(confirmationMailResult.Messages);
            response.Result = true;
            return response;
        }
    }
}
