using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using PersonalSafety.Contracts;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Hubs.Helpers;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels.ClientVM;
using PersonalSafety.Services.FileManager;
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
        private readonly IFileManagerService _fileManager;
        private readonly ILogger<EventBusiness> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventBusiness(IClientRepository clientRepository, IEventRepository eventRepository, IEventCategoryRepository eventCategoryRepository, IFileManagerService fileManager, ILogger<EventBusiness> logger, UserManager<ApplicationUser> userManager)
        {
            _clientRepository = clientRepository;
            _eventRepository = eventRepository;
            _eventCategoryRepository = eventCategoryRepository;
            _fileManager = fileManager;
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

            string uploadResult = null;

            if (request.Thumbnail != null)
            {
                var uploadResults = _fileManager.UploadImages(new List<IFormFile> { request.Thumbnail });
                if (uploadResults.Count != 1)
                {
                    response.HasErrors = true;
                    response.Messages.Add("An error occured while trying to upload the attachment.");
                    response.Status = (int)APIResponseCodesEnum.BadRequest;
                    return response;
                }
                else
                {
                    uploadResult = uploadResults[0];
                }
            }

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
                LastModified = DateTime.Now,
                ThumbnailUrl = uploadResult
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

        public async Task<APIResponse<List<EventMinifiedViewModel>>> GetEventsAsync(string userId, EventFiltersEnum filter, int? cateogryId)
        {
            APIResponse<List<EventMinifiedViewModel>> response = new APIResponse<List<EventMinifiedViewModel>>();

            var nullClientCheckResult = CheckForNullClient(userId, out Client client);
            if (nullClientCheckResult != null)
            {
                response.WrapResponseData(nullClientCheckResult);
                return response;
            }

            var databaseResult = _eventRepository.GetFilteredEvents(userId, filter, cateogryId);
            var viewModelResult = new List<EventMinifiedViewModel>();

            foreach(var result in databaseResult)
            {
                var eventOwner = await _userManager.FindByIdAsync(result.UserId);

                viewModelResult.Add(new EventMinifiedViewModel
                {
                    Title = result.Title,
                    UserName = eventOwner?.FullName,
                    IsPublicHelp = result.IsPublicHelp,
                    IsValidated = result.IsValidated,
                    Votes = result.Votes,
                    ThumbnailUrl = result.ThumbnailUrl
                });
            }

            response.Result = viewModelResult;
            return response;
        }

        #region Private Checkers

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

        #endregion
    }
}
