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
    public class UserBusiness : IUserBusiness
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmergencyContactRepository _emergencyContactRepository;

        public UserBusiness(UserManager<ApplicationUser> userManager, IEmergencyContactRepository emergencyContactRepository)
        {
            _userManager = userManager;
            _emergencyContactRepository = emergencyContactRepository;
        }

        public APIResponse<IEnumerable<EmergencyContact>> GetEmergencyContacts(string userId)
        {
            APIResponse<IEnumerable<EmergencyContact>> response = new APIResponse<IEnumerable<EmergencyContact>>();
            response.Result = _emergencyContactRepository.GetByUserId(userId);

            return response;
        }

        public async Task<APIResponse<bool>> CompleteProfileAsync(string userId, CompleteProfileViewModel request)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                response.Messages.Add("User not authorized.");
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.Unauthorized;
                return response;
            }

            if (!user.EmailConfirmed)
            {
                response.Messages.Add("Your account is not yet verified, please verify it through your email then proceed.");
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.NotConfirmed;
                return response;
            }

            // Check if user provided a value, else keep old value.
            user.CurrentAddress = request.CurrentAddress ?? user.CurrentAddress;
            user.BloodType = (request.BloodType != 0) ? request.BloodType : user.BloodType;
            user.MedicalHistoryNotes = request.MedicalHistoryNotes ?? user.MedicalHistoryNotes;

            if (request.EmergencyContacts != null)
            {
                _emergencyContactRepository.DeleteForUser(userId);
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

            var result = await _userManager.UpdateAsync(user);


            if (!result.Succeeded)
            {
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.IdentityError;
                response.Messages = result.Errors.Select(e => e.Description).ToList();
            }

            int addedContactsCount = request.EmergencyContacts?.Count ?? 0;
            response.Result = true;
            response.Messages.Add("Added " + addedContactsCount + " new emergency contatcts to user with email " + user.Email);
            response.Messages.Add("Current total emergency contacts: " + _emergencyContactRepository.GetByUserId(userId).Count());
            return response;
        }
    }
}
