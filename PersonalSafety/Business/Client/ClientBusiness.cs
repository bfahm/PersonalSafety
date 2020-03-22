using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PersonalSafety.Hubs;
using PersonalSafety.Hubs.HubTracker;
using PersonalSafety.Models;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Models.ViewModels;
using PersonalSafety.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Services.Registration;

namespace PersonalSafety.Business
{
    public class ClientBusiness : IClientBusiness
    {
        private readonly IClientRepository _clientRepository;
        private readonly IEmergencyContactRepository _emergencyContactRepository;
        private readonly ISOSRequestRepository _sosRequestRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMainHub _mainHub;
        private readonly IFacebookAuthService _facebookAuthService;
        private readonly IJwtAuthService _jwtAuthService;
        private readonly IRegistrationService _registrationService;

        public ClientBusiness(IClientRepository clientRepository, IEmergencyContactRepository emergencyContactRepository, ISOSRequestRepository sosRequestRepository, UserManager<ApplicationUser> userManager, IMainHub mainHub, IFacebookAuthService facebookAuthService, IJwtAuthService jwtAuthService, IRegistrationService registrationService)
        {
            _clientRepository = clientRepository;
            _emergencyContactRepository = emergencyContactRepository;
            _sosRequestRepository = sosRequestRepository;
            _userManager = userManager;
            _mainHub = mainHub;
            _facebookAuthService = facebookAuthService;
            _jwtAuthService = jwtAuthService;
            _registrationService = registrationService;
        }

        public async Task<APIResponse<bool>> RegisterAsync(RegistrationViewModel request)
        {
            ApplicationUser newUser = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber
            };

            Client client = new Client
            {
                ClientId = newUser.Id,
                NationalId = request.NationalId
            };

            return await _registrationService.RegisterNewUserAsync(newUser, request.Password, client);
        }

        public async Task<APIResponse<string>> LoginWithFacebookAsync(string accessToken)
        {
            APIResponse<string> response = new APIResponse<string>();

            FacebookTokenValidationResult tokenValidationResult = await _facebookAuthService.ValidateAccessTokenAsync(accessToken);
            if (!tokenValidationResult.Data.IsValid)
            {
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.FacebookAuthError;
                response.Messages.Add("An error occured while trying to validate the provided access token.");
                return response;
            }

            FacebookUserInfoResult userInfo = await _facebookAuthService.GetUserInfoAsync(accessToken);

            ApplicationUser user = await _userManager.FindByEmailAsync(userInfo.Email);
            if(user == null)
            {
                response.Messages.Add("This access token does not map to a registered user, use the access token to register the user first and try logging in again.");
                return response;
            }

            response.Result = await _jwtAuthService.GenerateAuthenticationTokenAsync(user);
            response.Messages.Add("Success, use the JWToken to access the api in the future requests");
            return response;
        }

        public async Task<APIResponse<bool>> RegisterWithFacebookAsync(RegistrationWithFacebookViewModel request)
        {
            FacebookTokenValidationResult tokenValidationResult = await _facebookAuthService.ValidateAccessTokenAsync(request.accessToken);
            if (!tokenValidationResult.Data.IsValid)
            {
                APIResponse<bool> response = new APIResponse<bool>();
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.FacebookAuthError;
                response.Messages.Add("An error occured while trying to validate the provided access token.");
                return response;
            }

            FacebookUserInfoResult userInfo = await _facebookAuthService.GetUserInfoAsync(request.accessToken);

            ApplicationUser newUser = new ApplicationUser
            {
                Email = userInfo.Email,
                UserName = userInfo.Email,
                FullName = userInfo.FirstName + " " + userInfo.LastName,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = true // no need to confirm user email, since it is already confirmed using facebook
            };

            Client client = new Client
            {
                ClientId = newUser.Id,
                NationalId = request.NationalId
            };

            return await _registrationService.RegisterNewUserAsync(newUser, null, client);
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

        public async Task<APIResponse<SendSOSResponseViewModel>> SendSOSRequestAsync(string userId, SendSOSRequestViewModel request)
        {
            APIResponse<SendSOSResponseViewModel> response = new APIResponse<SendSOSResponseViewModel>();

            Client user = _clientRepository.GetById(userId);
            if (user == null)
            {
                response.Messages.Add("User not authorized.");
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.Unauthorized;
                return response;
            }

            if (!_mainHub.isConnected(request.ConnectionId))
            {
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.SignalRError;
                response.Messages.Add("The provided connection id is not valid anymore, establish a new connection and try again.");
                return response;
            }

            if(!Enum.IsDefined(typeof(AuthorityTypesEnum), request.AuthorityType))
            {
                response.Messages.Add("You provided a wrong department type, please try again.");
                response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                response.HasErrors = true;
                return response;
            }

            SOSRequest sosRequest = new SOSRequest
            {
                UserId = userId,
                AuthorityType = request.AuthorityType,
                Longitude = request.Longitude,
                Latitude = request.Latitude
            };

            _sosRequestRepository.Add(sosRequest);
            _sosRequestRepository.Save();

            AddToTracker(request.ConnectionId, await _userManager.FindByIdAsync(userId), sosRequest.Id);

            response.Result = new SendSOSResponseViewModel { 
                RequestId = sosRequest.Id,
                RequestStateId = sosRequest.State,
                RequestStateName = ((StatesTypesEnum)sosRequest.State).ToString(),
            };

            response.Messages.Add("Your request was sent successfully.");
            
            return response;
        }

        private void AddToTracker(string connectionId, ApplicationUser user, int requestId)
        {
            SOSInfo currentRequest = new SOSInfo
            {
                ConnectionId = connectionId,
                UserId = user.Id,
                UserEmail = user.Email,
                SOSId = requestId
            };

            SOSHandler.SOSInfoSet.Add(currentRequest);
        }
    }
}
