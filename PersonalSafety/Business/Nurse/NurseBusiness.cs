using Microsoft.AspNetCore.Identity;
using PersonalSafety.Contracts;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalSafety.Business.Nurse
{
    public class NurseBusiness : INurseBusiness
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClientRepository _clientRepository;

        public NurseBusiness(UserManager<ApplicationUser> userManager, IClientRepository clientRepository)
        {
            _userManager = userManager;
            _clientRepository = clientRepository;
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
    }
}
