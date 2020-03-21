﻿using Microsoft.AspNetCore.Identity;
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
    public class PersonnelBusiness : IPersonnelBusiness
    {
        private readonly IPersonnelRepository _personnelRepository;
        private readonly ISOSRequestRepository _sosRequestRepository;
        private readonly IClientRepository _clientRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public PersonnelBusiness(IPersonnelRepository personnelRepository, ISOSRequestRepository sosRequestRepository, IClientRepository clientRepository, UserManager<ApplicationUser> userManager)
        {
            _personnelRepository = personnelRepository;
            _sosRequestRepository = sosRequestRepository;
            _clientRepository = clientRepository;
            _userManager = userManager;
        }

        public async Task<APIResponse<List<GetSOSRequestViewModel>>> GetRelatedRequestsAsync(string userId, int? requestState)
        {
            // Get current personnel authority type
            int authorityTypeInt = _personnelRepository.GetPersonnelAuthorityTypeInt(userId);

            // Find SOS Requests related to the request
            
            IEnumerable<SOSRequest> requests = (requestState != null) ? _sosRequestRepository.GetRelevantRequests(authorityTypeInt, (int)requestState)
                                                : _sosRequestRepository.GetRelevantRequests(authorityTypeInt);

            List <GetSOSRequestViewModel> responseViewModel = new List<GetSOSRequestViewModel>();

            foreach(var request in requests)
            {
                ApplicationUser requestOwner_Account = await _userManager.FindByIdAsync(request.UserId);
                Client requestOwner_Client = _clientRepository.GetById(request.UserId);
                responseViewModel.Add(new GetSOSRequestViewModel
                {
                    RequestId = request.Id,

                    UserEmail = requestOwner_Account.Email,
                    UserPhoneNumber = requestOwner_Account.PhoneNumber,
                    UserNationalId = requestOwner_Client.NationalId,
                    UserAge = DateTime.Today.Year - requestOwner_Client.Birthday.Year,
                    UserBloodTypeId = requestOwner_Client.BloodType,
                    UserBloodTypeName = ((BloodTypesEnum)requestOwner_Client.BloodType).ToString(),
                    UserMedicalHistoryNotes = requestOwner_Client.MedicalHistoryNotes,
                    UserSavedAddress = requestOwner_Client.CurrentAddress,

                    RequestStateId = request.State,
                    RequestStateName = ((StatesTypesEnum)request.State).ToString(),
                    RequestLocationLatitude = request.Latitude,
                    RequestLocationLongitude = request.Longitude,
                    RequestCreationDate = request.CreationDate,
                    RequestLastModified = request.LastModified
                });
            }

            APIResponse<List<GetSOSRequestViewModel>> response = new APIResponse<List<GetSOSRequestViewModel>>
            {
                Result = responseViewModel,
                HasErrors = false,
                Status = 0,
                Messages = null
            };


            return response;
        }
    }
}
