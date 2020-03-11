using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PersonalSafety.Business;

using PersonalSafety.Helpers;
using PersonalSafety.Models;
using PersonalSafety.Models.Enums;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Business
{
    public class ClientBusiness : IClientBusiness
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClientRepository _clientRepository;
        private readonly IEmergencyContactRepository _emergencyContactRepository;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly ISOSRequestRepository _sosRequestRepository;

        public ClientBusiness(UserManager<ApplicationUser> userManager, IClientRepository clientRepository, IEmergencyContactRepository emergencyContactRepository, IOptions<AppSettings> appSettings, ISOSRequestRepository sosRequestRepository)
        {
            _userManager = userManager;
            _clientRepository = clientRepository;
            _emergencyContactRepository = emergencyContactRepository;
            _appSettings = appSettings;
            _sosRequestRepository = sosRequestRepository;
        }

        public async Task<APIResponse<bool>> RegisterAsync(RegistrationViewModel request)
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

            ApplicationUser exsistingUserFoundByPhoneNumber = _userManager.Users.Where(u => u.PhoneNumber == request.PhoneNumber).FirstOrDefault();
            if (exsistingUserFoundByPhoneNumber != null)
            {
                response.Messages.Add("User with this Phone Number was registered before.");
                response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                response.HasErrors = true;
                return response;
            }

            Client exsistingUserFoundByNationalId = _clientRepository.GetAll().Where(u => u.NationalId == request.NationalId).FirstOrDefault();
            if (exsistingUserFoundByNationalId != null)
            {
                response.Messages.Add("User with this National Id was registered before.");
                response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                response.HasErrors = true;
                return response;
            }

            ApplicationUser newUser = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber
            };

            //If the user currently registering is a client, Add the additional data to his table
            Client client = new Client
            {
                ClientId = newUser.Id,
                NationalId = request.NationalId
            };

            _clientRepository.Add(client);

            //_clientRepository.Add(client) still needs saving, but will be done automatically in the below line.
            var creationResultForAccount = await _userManager.CreateAsync(newUser, request.Password);

            if (!creationResultForAccount.Succeeded)
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

        public APIResponse<CompleteProfileViewModel> GetEmergencyInfo(string userId)
        {
            APIResponse<CompleteProfileViewModel> response = new APIResponse<CompleteProfileViewModel>();

            Client user = _clientRepository.GetById(userId);
            if (user == null)
            {
                response.Messages.Add("User not authorized.");
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.Unauthorized;
                return response;
            }

            CompleteProfileViewModel viewModel = new CompleteProfileViewModel
            {
                BloodType = user.BloodType,
                CurrentAddress = user.CurrentAddress,
                MedicalHistoryNotes = user.MedicalHistoryNotes
            };

            viewModel.EmergencyContacts = _emergencyContactRepository.GetByUserId(userId).ToList();

            response.Result = viewModel;

            return response;
        }

        public APIResponse<bool> CompleteProfile(string userId, CompleteProfileViewModel request)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            Client user = _clientRepository.GetById(userId);
            if (user == null)
            {
                response.Messages.Add("User not authorized.");
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.Unauthorized;
                return response;
            }

            // Check if user provided a value, else keep old value.
            user.CurrentAddress = string.IsNullOrEmpty(request.CurrentAddress) ? user.CurrentAddress : request.CurrentAddress;
            user.MedicalHistoryNotes = string.IsNullOrEmpty(request.MedicalHistoryNotes) ? user.MedicalHistoryNotes : request.MedicalHistoryNotes;
            user.BloodType = (request.BloodType != 0) ? request.BloodType : user.BloodType;
            user.Birthday = (request.Birthday != null) ? request.Birthday : user.Birthday;
            //_clientRepository.Save() is NOT needed since EF already updates the values inline


            // Delete functionality implemented in the same hybrid function by simply not providing values
            // Updaet functionality implemented in the same hybrid function by simply providing new values after deleting old ones
            _emergencyContactRepository.DeleteForUser(userId);
            if (request.EmergencyContacts != null)
            {
                foreach (var contact in request.EmergencyContacts)
                {
                    _emergencyContactRepository.Add(new EmergencyContact
                    {
                        Name = contact.Name,
                        PhoneNumber = contact.PhoneNumber,
                        UserId = userId
                    });
                }

                _emergencyContactRepository.Save();
            }


           response.Result = true;
            response.Messages.Add("Success! Saved your info.");
            response.Messages.Add("Current total emergency contacts: " + _emergencyContactRepository.GetByUserId(userId).Count());
            return response;
        }

        public APIResponse<bool> SendSOSRequest(string userId, SendSOSRequestViewModel request)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            Client user = _clientRepository.GetById(userId);
            if (user == null)
            {
                response.Messages.Add("User not authorized.");
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.Unauthorized;
                return response;
            }

            if(!Enum.IsDefined(typeof(AuthorityTypesEnum), request.AuthorityType))
            {
                response.Messages.Add("You provided a wrong department type, please try again.");
                response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                response.HasErrors = true;
                return response;
            }

            _sosRequestRepository.Add(new SOSRequest 
            {
                UserId = userId,
                AuthorityType = request.AuthorityType,
                Longitude = request.Longitude,
                Latitude = request.Latitude
            });

            _sosRequestRepository.Save();

            response.Result = true;
            response.Messages.Add("Your request was sent successfully.");
            
            return response;
        }
    }
}
