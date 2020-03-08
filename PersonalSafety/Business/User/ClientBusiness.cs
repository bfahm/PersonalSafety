using Microsoft.AspNetCore.Identity;
using PersonalSafety.Business.User;
using PersonalSafety.Helpers;
using PersonalSafety.Models;
using PersonalSafety.Models.Enums;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Business.User
{
    public class ClientBusiness : IClientBusiness
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClientRepository _clientRepository;
        private readonly IEmergencyContactRepository _emergencyContactRepository;

        public ClientBusiness(UserManager<ApplicationUser> userManager, IClientRepository clientRepository, IEmergencyContactRepository emergencyContactRepository)
        {
            _userManager = userManager;
            _clientRepository = clientRepository;
            _emergencyContactRepository = emergencyContactRepository;
        }

        public APIResponse<CompleteProfileViewModel> GetEmergencyInfo(string userId)
        {
            APIResponse<CompleteProfileViewModel> response = new APIResponse<CompleteProfileViewModel>();

            Client user =  _clientRepository.GetById(userId);
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
            user.CurrentAddress = request.CurrentAddress ?? user.CurrentAddress;
            user.BloodType = (request.BloodType != 0) ? request.BloodType : user.BloodType;
            user.MedicalHistoryNotes = request.MedicalHistoryNotes ?? user.MedicalHistoryNotes;
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


            int addedContactsCount = request.EmergencyContacts?.Count ?? 0;
            response.Result = true;
            response.Messages.Add("Added " + addedContactsCount + " new emergency contatcts to currently logged in user");
            response.Messages.Add("Current total emergency contacts: " + _emergencyContactRepository.GetByUserId(userId).Count());
            return response;
        }
    }
}
