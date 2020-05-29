using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PersonalSafety.Contracts;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Hubs.Helpers;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels.ClientVM;
using PersonalSafety.Services.FileManager;
using PersonalSafety.Services.Location;
using PersonalSafety.Services.PushNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Business
{
    public class EventBusiness : IEventsBusiness
    {
        private readonly IClientRepository _clientRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IEventCategoryRepository _eventCategoryRepository;
        private readonly IFileManagerService _fileManager;
        private readonly ILocationService _locationService;
        private readonly IPushNotificationsService _pushNotificationsService;
        private readonly ILogger<EventBusiness> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventBusiness(IClientRepository clientRepository, IEventRepository eventRepository, IEventCategoryRepository eventCategoryRepository, IFileManagerService fileManager, ILocationService locationService, IPushNotificationsService pushNotificationsService, ILogger<EventBusiness> logger, UserManager<ApplicationUser> userManager)
        {
            _clientRepository = clientRepository;
            _eventRepository = eventRepository;
            _eventCategoryRepository = eventCategoryRepository;
            _fileManager = fileManager;
            _locationService = locationService;
            _pushNotificationsService = pushNotificationsService;
            _logger = logger;
            _userManager = userManager;
        }

        public APIResponse<string> UpdateLastKnownLocation(string userId, LocationViewModel request)
        {
            APIResponse<string> response = new APIResponse<string>();

            var nullClientCheckResult = CheckForNullClient(userId, out Client client);
            if (nullClientCheckResult != null)
            {
                response.WrapResponseData(nullClientCheckResult);
                return response;
            }

            var nearestCity = _locationService.GetNearestCity(new Location(request.Longitude, request.Latitude));

            client.LastKnownCityId = nearestCity.Id;
            _clientRepository.Update(client);
            _clientRepository.Save();

            response.Result = nearestCity.ToString();
            response.Messages.Add($"Your assigned city was updated to {nearestCity.Value}.");

            return response;
        }

        public APIResponse<bool> UpdateDeviceRegistraionKey(string userId, DeviceRegistrationViewModel request)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            var nullClientCheckResult = CheckForNullClient(userId, out Client client);
            if (nullClientCheckResult != null)
            {
                response.WrapResponseData(nullClientCheckResult);
                return response;
            }

            client.DeviceRegistrationKey = request.DeviceRegistrationKey;
            _clientRepository.Update(client);
            _clientRepository.Save();

            response.Result = true;
            response.Messages.Add($"Your device key was updated. You are currently subscribed for future notifications.");

            return response;
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

            var validEventTitleCheckResult = CheckForNullTitle(ref request);
            if (validEventTitleCheckResult != null)
            {
                response.WrapResponseData(validEventTitleCheckResult);
                return response;
            }

            var validCategoryCheckResult = CheckEventCategoryId(request.EventCategoryId);
            if (validCategoryCheckResult != null)
            {
                response.WrapResponseData(validCategoryCheckResult);
                return response;
            }

            var nearestCity = _locationService.GetNearestCity(new Location(request.Longitude, request.Latitude));

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
                NearestCityId = nearestCity.Id,
                State = (int)StatesTypesEnum.Pending,
                CreationDate = DateTime.Now,
                LastModified = DateTime.Now,
                ThumbnailUrl = uploadResult
            };

            _eventRepository.Add(newEvent);
            _eventRepository.Save();

            ApplicationUser userAccount = await _userManager.FindByIdAsync(userId);

            if (newEvent.IsPublicHelp)
                await NotifyNearbyClients(newEvent.NearestCityId);
            
            response.Result = new PostEventResponseViewModel
            {
                EventCategoryId = newEvent.Id,
                AssignedCityId = nearestCity.Id,
                AssignedCityName = nearestCity.ToString()
            };

            _logger.LogInformation(ConsoleFormatter.onEventStateChanged(userAccount.Email, newEvent.Id, StatesTypesEnum.Pending));
            response.Messages.Add("Your event was posted successfully.");

            return response;
        }

        public async Task<APIResponse<List<EventMinifiedViewModel>>> GetEventsAsync(string userId, int? filter)
        {
            APIResponse<List<EventMinifiedViewModel>> response = new APIResponse<List<EventMinifiedViewModel>>();

            var nullClientCheckResult = CheckForNullClient(userId, out _);
            if (nullClientCheckResult != null)
            {
                response.WrapResponseData(nullClientCheckResult);
                return response;
            }

            EventCategory eventCategory = (filter != null) ? _eventCategoryRepository.GetById(filter.ToString()) : null;

            List<Event> databaseResult;
            if (eventCategory != null)
            {
                if (eventCategory.Title == "Your Stories")
                {
                    databaseResult = _eventRepository.GetUserEvents(userId);
                }
                else if (eventCategory.Title == "Nearby Stories")
                {
                    // If user did not have a last known city, he would get all events instead.
                    databaseResult = _eventRepository.GetEventsByCityId(_clientRepository.GetById(userId).LastKnownCityId ?? 0);
                }
                else
                {
                    databaseResult = _eventRepository.GetFilteredEvents(eventCategory.Id);
                }
            }
            else
            {
                // Event Category was invalid or not found, so get all events instead
                databaseResult = _eventRepository.GetAll().ToList();
            }


            var viewModelResult = new List<EventMinifiedViewModel>();

            foreach(var result in databaseResult)
            {
                var eventOwner = await _userManager.FindByIdAsync(result.UserId);

                viewModelResult.Add(new EventMinifiedViewModel
                {
                    Id = result.Id,
                    Title = result.Title,
                    UserName = eventOwner?.FullName,
                    IsPublicHelp = result.IsPublicHelp,
                    IsValidated = result.IsValidated,
                    Votes = result.Votes,
                    ThumbnailUrl = result.ThumbnailUrl,
                    CreationDate = result.CreationDate
                });
            }

            response.Result = viewModelResult;
            return response;
        }

        public async Task<APIResponse<EventDetailedViewModel>> GetEventByIdAsync(int eventId)
        {
            var response = new APIResponse<EventDetailedViewModel>();
            
            var databaseResult = _eventRepository.GetById(eventId.ToString());

            var nullDbResultCheck = CheckForNullDatabaseResult(ref databaseResult);
            if (nullDbResultCheck != null)
            {
                response.WrapResponseData(nullDbResultCheck);
                return response;
            }

            var eventOwner = await _userManager.FindByIdAsync(databaseResult.UserId);
            var viewModelResult = new EventDetailedViewModel 
            {
                Id = databaseResult.Id,
                Title = databaseResult.Title,
                Description = databaseResult.Description,
                UserName = eventOwner?.FullName,

                IsPublicHelp = databaseResult.IsPublicHelp,
                IsValidated = databaseResult.IsValidated,
                Votes = databaseResult.Votes,

                ThumbnailUrl = databaseResult.ThumbnailUrl,
                
                CreationDate = databaseResult.CreationDate,
                LastModified = databaseResult.LastModified,
                
                EventCategoryId = databaseResult.EventCategoryId,
                EventCategoryName = databaseResult.EventCategory?.Title,
                
                Latitude = databaseResult.Latitude,
                Longitude = databaseResult.Longitude
            };

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

        private APIResponseData CheckForNullTitle(ref PostEventRequestViewModel request)
        {
            if (request.Title == null || request.Title.Length == 0)
            {
                return new APIResponseData((int)APIResponseCodesEnum.BadRequest,
                    new List<string>()
                        {"Error. You must provide a title for your event."});
            }

            return null;
        }

        private APIResponseData CheckForNullDatabaseResult(ref Event requestedEvent)
        {
            if (requestedEvent == null)
            {
                return new APIResponseData((int)APIResponseCodesEnum.NotFound,
                    new List<string>()
                        {"Error. The requested event was not found."});
            }

            return null;
        }

        #endregion

        #region Private Helpers

        private async Task NotifyNearbyClients(int nearestCityId)
        {
            var nearbyClients = _clientRepository.GetClientsByCityId(nearestCityId);

            var publicEventsInCity = _eventRepository.GetPublicEventsByCityId(nearestCityId);

            //var publicEventsInCityJson = JsonSerializer.Serialize(publicEventsInCity);
            var publicEventsInCityDictionary = ConvertToDictionary(publicEventsInCity);

            foreach (var client in nearbyClients)
            {
                var registraionKey = client.DeviceRegistrationKey;
                if (registraionKey != null && registraionKey.Length > 0)
                {
                    await _pushNotificationsService.TrySendData(registraionKey, publicEventsInCityDictionary);
                }
            }
        }

        private Dictionary<string, string> ConvertToDictionary(List<Event> listOfEvents)
        {
            var returnDictionary = new Dictionary<string, string>();
            
            foreach(var _event in listOfEvents)
            {
                returnDictionary.Add(_event.Id.ToString(), $"{_event.Latitude}_{_event.Longitude}");
            }

            return returnDictionary;
        }

        #endregion

        #region To Be Used With SignalR

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

        //---------------------------------------------------------------------------------------------

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

        #endregion

    }
}
