using Microsoft.AspNetCore.Identity;
using PersonalSafety.Contracts;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels;
using PersonalSafety.Models.ViewModels.NurseVM;
using PersonalSafety.Services.PushNotification;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalSafety.Business.Nurse
{
    public class NurseBusiness : INurseBusiness
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClientRepository _clientRepository;
        private readonly IClientTrackingRepository _clientTrackingRepository;
        private readonly IPushNotificationsService _pushNotificationsService;

        public NurseBusiness(UserManager<ApplicationUser> userManager, IClientRepository clientRepository, IClientTrackingRepository clientTrackingRepository, IPushNotificationsService pushNotificationsService)
        {
            _userManager = userManager;
            _clientRepository = clientRepository;
            _clientTrackingRepository = clientTrackingRepository;
            _pushNotificationsService = pushNotificationsService;
        }

        public async Task<APIResponse<GetUserDataViewModel>> GetClientDetails(string clientEmail)
        {
            APIResponse<GetUserDataViewModel> response = new APIResponse<GetUserDataViewModel>();

            var clientAccount = await _userManager.FindByEmailAsync(clientEmail);
            if (clientAccount == null)
            {
                var responseData = new APIResponseData((int)APIResponseCodesEnum.NotFound,
                    new List<string>()
                        {"Error. Client not found."});

                response.WrapResponseData(responseData);
                return response;
            }

            var client = _clientRepository.GetById(clientAccount.Id);

            if (client == null)
            {
                var responseData = new APIResponseData((int)APIResponseCodesEnum.BadRequest,
                    new List<string>()
                        {"Error. Invalid client account."});

                response.WrapResponseData(responseData);
                return response;
            }

            GetUserDataViewModel responseViewModel = new GetUserDataViewModel
            {
                UserId = clientAccount.Id,
                UserFullName = clientAccount.FullName,
                UserEmail = clientAccount.Email,
                UserPhoneNumber = clientAccount.PhoneNumber,
                UserNationalId = client.NationalId,
                UserAge = DateTime.Today.Year - client.Birthday.Year,
                UserBloodTypeId = client.BloodType,
                UserBloodTypeName = ((BloodTypesEnum)client.BloodType).ToString(),
                UserMedicalHistoryNotes = client.MedicalHistoryNotes,
                UserSavedAddress = client.CurrentAddress
            };

            response.Result = responseViewModel;
            return response;
        }

        public async Task<APIResponse<VictimStateViewModel>> EditClientVictimState(string clientEmail, bool isVictim)
        {
            APIResponse<VictimStateViewModel> response = new APIResponse<VictimStateViewModel>();
            var responseViewModel = new VictimStateViewModel();

            var clientAccount = await _userManager.FindByEmailAsync(clientEmail);
            if (clientAccount == null)
            {
                var responseData = new APIResponseData((int)APIResponseCodesEnum.NotFound,
                    new List<string>()
                        {"Error. Client not found."});

                response.WrapResponseData(responseData);
                return response;
            }

            var client = _clientRepository.GetById(clientAccount.Id);

            if (client == null)
            {
                var responseData = new APIResponseData((int)APIResponseCodesEnum.BadRequest,
                    new List<string>()
                        {"Error. Invalid client account."});

                response.WrapResponseData(responseData);
                return response;
            }

            client.IsCoronaVictim = isVictim;
            _clientRepository.Update(client);

            responseViewModel.VictimEmail = clientAccount.Email;

            var susceptibleAccountsIds = _clientTrackingRepository.GetSusceptibleAccounts(client.ClientId);

            foreach(var accountId in susceptibleAccountsIds)
            {
                var susceptibleClientData = _clientRepository.GetById(accountId);
                var susceptibleAccountData = await _userManager.FindByIdAsync(accountId);

                client.IsCoronaSusceptible = isVictim;
                _clientRepository.Update(client);

                responseViewModel.SusceptibleEmails.Add(susceptibleAccountData.Email);

                if (isVictim)
                    await _pushNotificationsService.SendNotification(susceptibleClientData.DeviceRegistrationKey, "COVID-19 Alert",
                        "It appears like visited an Epicenter lately. Please stay home the next 14 days to avoid potentially infecting others.");
            }

            _clientRepository.Save();

            string state = isVictim ? "POSITIVE" : "NEGATIVE";
            response.Messages.Add($"These accounts where marked as {state}.");
            response.Result = responseViewModel;

            return response;
        }
    }
}
