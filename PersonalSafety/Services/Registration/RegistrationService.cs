using Microsoft.AspNetCore.Identity;
using PersonalSafety.Contracts;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PersonalSafety.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClientRepository _clientRepository;
        private readonly IEmailService _emailService;

        public RegistrationService(UserManager<ApplicationUser> userManager, IClientRepository clientRepository, IEmailService emailService)
        {
            _userManager = userManager;
            _clientRepository = clientRepository;
            _emailService = emailService;
        }

        public async Task<APIResponse<bool>> RegisterClientAsync(ApplicationUser applicationUser, string password, Client client)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            var emailDuplicationResult = await CheckEmailDuplication(applicationUser.Email);
            if (emailDuplicationResult != null) 
            {
                response.WrapResponseData(emailDuplicationResult);
                return response;
            }

            var phoneDuplicationResult = CheckPhoneNumberDuplication(applicationUser.PhoneNumber);
            if (phoneDuplicationResult != null) 
            {
                response.WrapResponseData(phoneDuplicationResult);
                return response; 
            }
            
            var nationalIdDuplicationResult = CheckNationalIdDuplication(client.NationalId);
            if (nationalIdDuplicationResult != null) 
            {
                response.WrapResponseData(nationalIdDuplicationResult);
                return response;
            }

            //_clientRepository.Add(client) still needs saving, but will be done automatically while creating the user below.
            _clientRepository.Add(client);

            IdentityResult creationResultForAccount;
            // Case: Registration normally
            if (!string.IsNullOrEmpty(password)) { creationResultForAccount = await _userManager.CreateAsync(applicationUser, password);}
            // Case: Registration using facebook (no password needed)
            else { creationResultForAccount = await _userManager.CreateAsync(applicationUser); }

            var identityCheckResult = CheckIdentityResult(creationResultForAccount);
            if (identityCheckResult != null) 
            {
                response.WrapResponseData(identityCheckResult);
                return response;
            }

            response.Messages.Add("Successfully created a new client with email " + applicationUser.Email);

            if (!string.IsNullOrEmpty(password))  // normal registration
            {
                var confirmationMailResult = await _emailService.SendConfirmMailAsync(applicationUser.Email);
                response.Messages.Add("Please check your email for activation links before you continue.");
                response.Messages.AddRange(confirmationMailResult);
            }
            else // registration was done using facebook
            {
                response.Messages.Add("User has registered using his Facebook account with no passwords saved to his account.");
            }
            
            
            response.Result = true;
            return response;
        }

        public async Task<APIResponse<bool>> RegisterWorkingEntityAsync(ApplicationUser applicationUser, string password, EntityAdder entityAdder, string[] roles, Claim[] claims)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            var emailDuplicationResult = await CheckEmailDuplication(applicationUser.Email);
            if (emailDuplicationResult != null) 
            {
                response.WrapResponseData(emailDuplicationResult);
                return response;
            }

            entityAdder?.Invoke();

            var creationResultForAccount = await _userManager.CreateAsync(applicationUser, password);
            var identityCheckResult = CheckIdentityResult(creationResultForAccount);
            if (identityCheckResult != null)
            {
                response.WrapResponseData(identityCheckResult);
                return response;
            }

            if (roles != null)
            {
                var addToRoleResult = await _userManager.AddToRolesAsync(applicationUser, roles);
                var roleAddingCheckResult = CheckIdentityResult(addToRoleResult);
                if (roleAddingCheckResult != null)
                {
                    response.WrapResponseData(roleAddingCheckResult);
                    return response;
                }
            }

            if(claims != null)
            {
                var addToClaimResult = await _userManager.AddClaimsAsync(applicationUser, claims);
                var ClaimAddingCheckResult = CheckIdentityResult(addToClaimResult);
                if (ClaimAddingCheckResult != null)
                {
                    response.WrapResponseData(ClaimAddingCheckResult);
                    return response;
                }
            }

            // TODO: send just congratulation email using the service here
            response.Messages.Add("Successfully created a new entity with email " + applicationUser.Email);
            return response;
        }

        // Helpers and checkers

        private async Task<APIResponseData> CheckEmailDuplication(string email)
        {
            if(await _userManager.FindByEmailAsync(email) != null)
            {
                APIResponseData data = new APIResponseData((int)APIResponseCodesEnum.InvalidRequest, 
                    new List<string> { "User with this email address already exsists." });

                return data;
            }
            return null;
        }

        private APIResponseData CheckPhoneNumberDuplication(string phoneNumber)
        {
            if (_userManager.Users.FirstOrDefault(u => u.PhoneNumber == phoneNumber) != null)
            {
                APIResponseData data = new APIResponseData((int)APIResponseCodesEnum.InvalidRequest, 
                    new List<string> { "User with this Phone Number was registered before." });
                
                return data;
            }
            return null;
        }

        private APIResponseData CheckNationalIdDuplication(string nationalId)
        {
            if (_clientRepository.GetByNationalId(nationalId) != null)
            {
                APIResponseData data = new APIResponseData((int)APIResponseCodesEnum.InvalidRequest, 
                    new List<string> { "User with this National Id was registered before." });

                return data;
            }
            return null;
        }

        private APIResponseData CheckIdentityResult(IdentityResult result)
        {
            if (!result.Succeeded)
            {
                APIResponseData data = new APIResponseData((int)APIResponseCodesEnum.IdentityError,
                    result.Errors.Select(e => e.Description).ToList());

                return data;
            }
            return null;
        }

        public delegate void EntityAdder();
    }
}
