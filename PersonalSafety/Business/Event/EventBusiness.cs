using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PersonalSafety.Contracts;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Hubs;
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
        private readonly IClientHub _clientHub;
        private readonly IDistributionRepository _distributionRepository;
        private readonly IClientTrackingRepository _clientTrackingRepository;

        public EventBusiness(IClientRepository clientRepository, IEventRepository eventRepository, IEventCategoryRepository eventCategoryRepository, IFileManagerService fileManager, ILocationService locationService, IPushNotificationsService pushNotificationsService, ILogger<EventBusiness> logger, UserManager<ApplicationUser> userManager, IClientHub clientHub, IDistributionRepository distributionRepository, IClientTrackingRepository clientTrackingRepository)
        {
            _clientRepository = clientRepository;
            _eventRepository = eventRepository;
            _eventCategoryRepository = eventCategoryRepository;
            _fileManager = fileManager;
            _locationService = locationService;
            _pushNotificationsService = pushNotificationsService;
            _logger = logger;
            _userManager = userManager;
            _clientHub = clientHub;
            _distributionRepository = distributionRepository;
            _clientTrackingRepository = clientTrackingRepository;
        }

        // Client Actions

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

            _clientTrackingRepository.Add(new ClientTracking
            {
                ClientId = client.ClientId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Time = DateTime.Now
            });

            _clientTrackingRepository.Save();
            response.Messages.Add($"Your tracking log was saved.");

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

            var validCategoryCheckResult = CheckEventCategoryId(ref request);
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
                EventId = newEvent.Id,
                AssignedCityId = nearestCity.Id,
                AssignedCityName = nearestCity.ToString()
            };

            _logger.LogInformation(ConsoleFormatter.onEventStateChanged(userAccount.Email, newEvent.Id, StatesTypesEnum.Pending));
            response.Messages.Add("Your event was posted successfully.");

            return response;
        }

        public async Task<APIResponse<List<EventDetailedViewModel>>> GetEventsDetailedAsync(string userId, int? filter)
        {
            APIResponse<List<EventDetailedViewModel>> response = new APIResponse<List<EventDetailedViewModel>>();

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


            var viewModelResult = new List<EventDetailedViewModel>();

            foreach (var result in databaseResult)
            {
                var eventOwner = await _userManager.FindByIdAsync(result.UserId);

                viewModelResult.Add(new EventDetailedViewModel
                {
                    Id = result.Id,
                    Title = result.Title,
                    UserName = eventOwner?.FullName,
                    IsPublicHelp = result.IsPublicHelp,
                    IsValidated = result.IsValidated,
                    Votes = result.Votes,
                    ThumbnailUrl = result.ThumbnailUrl,
                    CreationDate = result.CreationDate,
                    Description = result.Description,
                    EventCategoryId = result.EventCategoryId,
                    EventCategoryName = result.EventCategory?.Title,
                    LastModified = result.LastModified,
                    Latitude = result.Latitude,
                    Longitude = result.Longitude
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

        public async Task<APIResponse<bool>> CancelEventByIdAsync(string userId, int eventId)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            var updateResult = await UpdateEventAsync(userId, eventId, StatesTypesEnum.Canceled);

            if(updateResult != null)
            {
                response.WrapResponseData(updateResult);
                return response;
            }

            response.Result = true;
            response.Messages.Add("Your event was successfully canceled.");

            return response;
        }

        public async Task<APIResponse<bool>> SolveEventByIdAsync(string userId, int eventId)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            var updateResult = await UpdateEventAsync(userId, eventId, StatesTypesEnum.Solved);

            if (updateResult != null)
            {
                response.WrapResponseData(updateResult);
                return response;
            }

            response.Result = true;
            response.Messages.Add("Your event was successfully solved.");

            return response;
        }

        private async Task<APIResponseData> UpdateEventAsync(string userId, int eventId, StatesTypesEnum newState)
        {
            var nullClientCheckResult = CheckForNullClient(userId, out _);
            if (nullClientCheckResult != null)
                return nullClientCheckResult;

            var nullCEventCheckResult = CheckForNullEvent(eventId, out Event existingEvent);
            if (nullCEventCheckResult != null)
                return nullCEventCheckResult;

            // If the to-be-updated-to state is solved, check if the event is not canceled, and vice versa
            var correctStateCheckResult = CheckForEventCorrectState(existingEvent, 
                (newState == StatesTypesEnum.Solved) ? StatesTypesEnum.Canceled : StatesTypesEnum.Solved);
            if (correctStateCheckResult != null)
                return correctStateCheckResult;

            existingEvent.State = (int)newState;

            _eventRepository.Update(existingEvent);
            _eventRepository.Save();

            ApplicationUser userAccount = await _userManager.FindByIdAsync(userId);

            await NotifyNearbyClients(existingEvent.NearestCityId);

            var signalRSignal = (newState == StatesTypesEnum.Canceled) ? -1 : -2;
            await _clientHub.SendToEventRoom(userAccount.Email, eventId, signalRSignal, signalRSignal);

            _logger.LogInformation(ConsoleFormatter.onEventStateChanged(userAccount.Email, existingEvent.Id, StatesTypesEnum.Canceled));
            
            return null;
        }

        // Managerial Actions

        public async Task<APIResponse<List<EventDetailedViewModel>>> GetEventsForManagerAsync(string managerUserId)
        {
            APIResponse<List<EventDetailedViewModel>> response = new APIResponse<List<EventDetailedViewModel>>();

            var allowedCities = await GetManagerAllowedCities(managerUserId);

            var allowedEvents = _eventRepository.GetEventsByCities(allowedCities.Select(ac => ac.Id).ToList());

            List<EventDetailedViewModel> viewModel = new List<EventDetailedViewModel>();
            foreach(var _event in allowedEvents)
            {
                var eventOwner = await _userManager.FindByIdAsync(_event.UserId);

                viewModel.Add(new EventDetailedViewModel
                {
                    Id = _event.Id,
                    Title = _event.Title,
                    UserName = eventOwner?.FullName,
                    IsPublicHelp = _event.IsPublicHelp,
                    IsValidated = _event.IsValidated,
                    Votes = _event.Votes,
                    ThumbnailUrl = _event.ThumbnailUrl,
                    CreationDate = _event.CreationDate,
                    Description = _event.Description,
                    EventCategoryId = _event.EventCategoryId,
                    EventCategoryName = _event.EventCategory?.Title,
                    LastModified = _event.LastModified,
                    Latitude = _event.Latitude,
                    Longitude = _event.Longitude
                });
            }
            
            response.Result = viewModel;
            return response;
        }

        public async Task<APIResponse<bool>> UpdateEventValidity(string managerUserId, int eventId, bool toValidate)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            var nullCEventCheckResult = CheckForNullEvent(eventId, out Event existingEvent);
            if (nullCEventCheckResult != null)
            {
                response.WrapResponseData(nullCEventCheckResult);
                return response;
            }

            var managerAccessCheckResult = await CheckManagerHaveAccessToEvent(managerUserId, existingEvent);
            if (managerAccessCheckResult != null)
            {
                response.WrapResponseData(managerAccessCheckResult);
                return response;
            }

            existingEvent.IsValidated = toValidate;
            _eventRepository.Update(existingEvent);
            _eventRepository.Save();

            response.Result = true;
            response.Messages.Add($"Success: Event with Id {existingEvent.Id} validation was changed to {existingEvent.IsValidated}.");

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

        private APIResponseData CheckForNullEvent(int eventId, out Event existingEvent)
        {
            existingEvent = _eventRepository.GetById(eventId.ToString());

            if (existingEvent == null)
            {
                return new APIResponseData((int)APIResponseCodesEnum.NotFound,
                    new List<string>()
                        {"Error. Event Not Found."});
            }

            return null;
        }

        private APIResponseData CheckForEventCorrectState(Event existingEvent, StatesTypesEnum neededState)
        {
            if (existingEvent.State != (int)neededState)
            {
                return new APIResponseData((int)APIResponseCodesEnum.BadRequest,
                    new List<string>()
                        {$"Error. The event is already {(StatesTypesEnum)existingEvent.State}."});
            }

            return null;
        }

        private APIResponseData CheckEventCategoryId(ref PostEventRequestViewModel request)
        {
            var requestCategoryId = request.EventCategoryId ?? 0;

            var categories = _eventCategoryRepository.GetAll();
            var yourStoriesCategory = categories.First(c => c.Title == "Your Stories");
            var nearbyStoriesCategory = categories.First(c => c.Title == "Nearby Stories");

            if(requestCategoryId == 0 || requestCategoryId == yourStoriesCategory.Id 
                || requestCategoryId == nearbyStoriesCategory.Id)
            {
                request.EventCategoryId = null;
                return null;
            }else if (categories.FirstOrDefault(c => c.Id == requestCategoryId) != null)
            {
                return null;
            }
            else
            {
                return new APIResponseData((int)APIResponseCodesEnum.BadRequest,
                    new List<string>()
                        {"Error. Provide a correct category Id or leave the field null to mark as a General Event."});
            }
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

        private async Task<APIResponseData> CheckManagerHaveAccessToEvent(string managerUserId, Event requestedEvent)
        {
            var allowedCitites = await GetManagerAllowedCities(managerUserId);
            var allowedCititesIds = allowedCitites.Select(ac => ac.Id);

            if (!allowedCititesIds.Contains(requestedEvent.NearestCityId))
            {
                return new APIResponseData((int)APIResponseCodesEnum.Unauthorized,
                    new List<string>()
                        {"Error. You don't have access to this event."});
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
                    await _pushNotificationsService.SendNotification(registraionKey, publicEventsInCityDictionary);
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

        private async Task<List<Distribution>> GetManagerAllowedCities(string managerUserId)
        {
            var managerAccount = await _userManager.FindByIdAsync(managerUserId);
            var userAccessToDistribution = int.Parse((await _userManager.GetClaimsAsync(managerAccount))
                                                .SingleOrDefault(c => c.Type == ClaimsStore.CLAIM_DISTRIBUTION_ACCESS).Value);

            return _distributionRepository.GetGrantedCities(userAccessToDistribution);
        }

        #endregion
    }
}
