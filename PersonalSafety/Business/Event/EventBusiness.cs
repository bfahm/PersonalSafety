using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using PersonalSafety.Contracts;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Hubs.Helpers;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels.ClientVM;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalSafety.Business
{
    public class EventBusiness : IEventsBusiness
    {
        private readonly IClientRepository _clientRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IEventCategoryRepository _eventCategoryRepository;
        private readonly ILogger<EventBusiness> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventBusiness(IClientRepository clientRepository, IEventRepository eventRepository, IEventCategoryRepository eventCategoryRepository, ILogger<EventBusiness> logger, UserManager<ApplicationUser> userManager)
        {
            _clientRepository = clientRepository;
            _eventRepository = eventRepository;
            _eventCategoryRepository = eventCategoryRepository;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<APIResponse<PostEventResponseViewModel>> PostEventAsync(string userId, PostEventRequestViewModel request)
        {
            APIResponse<PostEventResponseViewModel> response = new APIResponse<PostEventResponseViewModel>();

            var nullClientCheckResult = CheckForNullClient(userId, out _);
            if (nullClientCheckResult != null)
            {
                response.WrapResponseData(nullClientCheckResult);
                return response;
            }

            var validCategoryCheckResult = CheckEventCategoryId(request.EventCategoryId);
            if (validCategoryCheckResult != null)
            {
                response.WrapResponseData(validCategoryCheckResult);
                return response;
            }

            // TODO: uncomment after implementing SignalR - if user needs to have only on ongoing event
            //if (_sosRequestRepository.UserHasOngoingRequest(userId))
            //{
            //    response.Messages.Add("You currently have ongoing requests, attempting to canceling them automatically.");
            //    var cancelResult = await CancelPendingRequestsAsync(userId, false);
            //    response.Messages.AddRange(cancelResult.Messages);
            //}

            // TODO: uncomment after implementing SignalR
            //if (!_clientHub.isConnected(userId))
            //{
            //    response.Status = (int)APIResponseCodesEnum.SignalRError;
            //    response.Messages.Add("Invalid Attempt. You do not have a valid realtime connection.");
            //    response.HasErrors = true;
            //    return response;
            //}

            // TODO: 
            //var nearestDepartment = _locationService.GetNearestDepartment(new Location(request.Longitude, request.Latitude),
            //    request.AuthorityType);

            Event newEvent = new Event
            {
                UserId = userId,
                Title = request.Title,
                Description = request.Description,
                EventCategoryId = request?.EventCategoryId,
                IsPublicHelp = request.IsPublicHelp,
                Longitude = request.Longitude,
                Latitude = request.Latitude,
                State = (int)StatesTypesEnum.Pending,
                CreationDate = DateTime.Now,
                LastModified = DateTime.Now
            };

            _eventRepository.Add(newEvent);
            _eventRepository.Save();

            ApplicationUser userAccount = await _userManager.FindByIdAsync(userId);
            // TODO: uncomment after implementing SignalR
            //var trackingResult = _clientHub.TrackSOSIdForClient(userAccount.Email, sosRequest.Id);

            //if (!trackingResult)
            //{
            //    _clientHub.RemoveClientFromTrackers(userId);

            //    // Was not able to reach the user:
            //    // revert changes:
            //    _sosRequestRepository.RemoveById(sosRequest.Id.ToString());
            //    _sosRequestRepository.Save();

            //    var errorMessage1 = "A system error occured while trying to maintain connection with the client.";
            //    var errorMessage2 = "The request was removed. Please have another try.";

            //    _logger.LogError(ConsoleFormatter.WrapSOSBusiness(errorMessage1));
            //    _logger.LogError(ConsoleFormatter.WrapSOSBusiness(errorMessage2));

            //    response.Messages.Add(errorMessage1);
            //    response.Messages.Add(errorMessage2);
            //    response.Status = (int)APIResponseCodesEnum.ServerError;
            //    response.HasErrors = true;

            //    return response;
            //}

            //_clientHub.NotifyUserSOSState(sosRequest.Id, (int)StatesTypesEnum.Pending);
            //_agentHub.NotifyNewChanges(sosRequest.Id, (int)StatesTypesEnum.Pending, sosRequest.AssignedDepartmentId);

            response.Result = new PostEventResponseViewModel
            {
                EventCategoryId = newEvent.Id
            };

            _logger.LogInformation(ConsoleFormatter.onEventStateChanged(userAccount.Email, newEvent.Id, StatesTypesEnum.Pending));
            response.Messages.Add("Your event was posted successfully.");

            return response;
        }

        private APIResponseData CheckForNullClient(string clientUsedId, out Client client)
        {
            client = _clientRepository.GetById(clientUsedId);

            if (client == null)
            {
                return new APIResponseData((int)APIResponseCodesEnum.Unauthorized,
                    new List<string>()
                        {"Error. Client unauthorized."});
            }

            return null;
        }

        private APIResponseData CheckEventCategoryId(int? categoryId)
        {
            if (categoryId != null && _eventCategoryRepository.GetById(categoryId.ToString()) == null)
            {
                return new APIResponseData((int)APIResponseCodesEnum.BadRequest,
                    new List<string>()
                        {"Error. Provide a correct category Id or leave the field null to mark as a General Event."});
            }

            return null;
        }
    }
}
