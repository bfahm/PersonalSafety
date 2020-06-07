using PersonalSafety.Models;
using PersonalSafety.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using Microsoft.AspNetCore.Identity;
using PersonalSafety.Hubs;
using PersonalSafety.Hubs.HubTracker;
using PersonalSafety.Hubs.Services;
using PersonalSafety.Models.ViewModels;
using PersonalSafety.Services.Location;
using Microsoft.Extensions.Logging;
using PersonalSafety.Hubs.Helpers;
using PersonalSafety.Services.Rate;

namespace PersonalSafety.Business
{
    public class SOSBusiness : ISOSBusiness
    {
        private readonly ISOSRequestRepository _sosRequestRepository;
        private readonly IPersonnelRepository _personnelRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRescuerHub _rescuerHub;
        private readonly IAgentHub _agentHub;
        private readonly IClientHub _clientHub;
        private readonly ILocationService _locationService;
        private readonly ILogger<SOSBusiness> _logger;

        public SOSBusiness(ISOSRequestRepository sosRequestRepository, IPersonnelRepository personnelRepository, IClientRepository clientRepository, IDepartmentRepository departmentRepository, UserManager<ApplicationUser> userManager, IRescuerHub rescuerHub, IAgentHub agentHub, IClientHub clientHub, ILocationService locationService, ILogger<SOSBusiness> logger)
        {
            _sosRequestRepository = sosRequestRepository;
            _personnelRepository = personnelRepository;
            _clientRepository = clientRepository;
            _departmentRepository = departmentRepository;
            _userManager = userManager;
            _rescuerHub = rescuerHub;
            _agentHub = agentHub;
            _clientHub = clientHub;
            _locationService = locationService;
            _logger = logger;
        }


        #region Main Methods

        public async Task<APIResponse<SendSOSResponseViewModel>> SendSOSRequestAsync(string userId, SendSOSRequestViewModel request)
        {
            APIResponse<SendSOSResponseViewModel> response = new APIResponse<SendSOSResponseViewModel>();

            var nullClientCheckResult = CheckForNullClient(userId, out _);
            if (nullClientCheckResult != null)
            {
                response.WrapResponseData(nullClientCheckResult);
                return response;
            }

            if (!Enum.IsDefined(typeof(AuthorityTypesEnum), request.AuthorityType))
            {
                response.Messages.Add("You provided a wrong department type, please try again.");
                response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                response.HasErrors = true;
                return response;
            }

            if (_sosRequestRepository.UserHasOngoingRequest(userId))
            {
                response.Messages.Add("You currently have ongoing requests, attempting to canceling them automatically.");
                var cancelResult = await CancelPendingRequestsAsync(userId, false);
                response.Messages.AddRange(cancelResult.Messages);
            }

            if (!_clientHub.isConnected(userId))
            {
                response.Status = (int)APIResponseCodesEnum.SignalRError;
                response.Messages.Add("Invalid Attempt. You do not have a valid realtime connection.");
                response.HasErrors = true;
                return response;
            }

            var nearestDepartment = _locationService.GetNearestDepartment(new Location(request.Longitude, request.Latitude),
                request.AuthorityType);

            SOSRequest sosRequest = new SOSRequest
            {
                UserId = userId,
                AuthorityType = request.AuthorityType,
                Longitude = request.Longitude,
                Latitude = request.Latitude,
                AssignedDepartmentId = nearestDepartment.Id
            };
            Department sosRequestDepartment = _departmentRepository.GetById(sosRequest.AssignedDepartmentId.ToString());
            
            _sosRequestRepository.Add(sosRequest);
            _sosRequestRepository.Save();

            ApplicationUser userAccount = await _userManager.FindByIdAsync(userId);
            var trackingResult = _clientHub.TrackSOSIdForClient(userAccount.Email, sosRequest.Id);

            if (!trackingResult)
            {
                _clientHub.RemoveClientFromTrackers(userId);

                // Was not able to reach the user:
                // revert changes:
                _sosRequestRepository.RemoveById(sosRequest.Id.ToString());
                _sosRequestRepository.Save();

                var errorMessage1 = "A system error occured while trying to maintain connection with the client.";
                var errorMessage2 = "The request was removed. Please have another try.";

                _logger.LogError(ConsoleFormatter.WrapSOSBusiness(errorMessage1));
                _logger.LogError(ConsoleFormatter.WrapSOSBusiness(errorMessage2));

                response.Messages.Add(errorMessage1);
                response.Messages.Add(errorMessage2);
                response.Status = (int)APIResponseCodesEnum.ServerError;
                response.HasErrors = true;

                return response;
            }

            _clientHub.NotifyUserSOSState(sosRequest.Id, (int)StatesTypesEnum.Pending);
            _agentHub.NotifyNewChanges(sosRequest.Id, (int)StatesTypesEnum.Pending, sosRequestDepartment.ToString());

            response.Result = new SendSOSResponseViewModel
            {
                RequestId = sosRequest.Id,
                RequestStateId = sosRequest.State,
                RequestStateName = ((StatesTypesEnum)sosRequest.State).ToString(),
                AssignedDepartment = nearestDepartment.ToString()
            };

            _logger.LogInformation(ConsoleFormatter.onSOSStateChanged(userAccount.Email, sosRequest.Id, StatesTypesEnum.Pending, nearestDepartment.ToString()));
            response.Messages.Add("Your request was sent successfully.");

            return response;
        }

        public APIResponse<bool> AcceptSOSRequest(int requestId, string rescuerEmail)
        {
            const int newState = (int) StatesTypesEnum.Accepted;
            APIResponse<bool> response = new APIResponse<bool>();

            var nullSOSCheckResult = CheckForNullSOS(requestId, out SOSRequest sosRequest);
            if (nullSOSCheckResult != null)
            {
                response.WrapResponseData(nullSOSCheckResult);
                return response;
            }

            var sosCorrectStateCheckResult = CheckSOSState(sosRequest, (int)StatesTypesEnum.Pending, true);
            if (sosCorrectStateCheckResult != null)
            {
                response.WrapResponseData(sosCorrectStateCheckResult);
                return response;
            }

            var nullRescuerCheckResult = CheckForNullRescuer(rescuerEmail, out ApplicationUser rescuer);
            if (nullRescuerCheckResult != null)
            {
                response.WrapResponseData(nullRescuerCheckResult);
                return response;
            }

            var sameDepartmentCheck = CheckSOSSameDepartment(ref sosRequest, ref rescuer);
            if (sameDepartmentCheck != null)
            {
                response.WrapResponseData(sameDepartmentCheck);
                return response;
            }

            var rescuerOnMissionCheckResult = CheckIfRescuerIsOnMission(rescuerEmail);
            if (rescuerOnMissionCheckResult != null)
            {
                _logger.LogInformation(ConsoleFormatter.WrapSOSBusiness($"Selected rescuer {rescuerEmail} is currently on mission."));
                response.WrapResponseData(rescuerOnMissionCheckResult);
                return response;
            }


            // Make sure that both the client and the rescuer are online before :
            var isClientConnected = _clientHub.isConnected(sosRequest.UserId);
            var isRescuerConnected = _rescuerHub.isConnected(rescuer.Id);
            if (!(isClientConnected && isRescuerConnected))
            {
                response.Status = (int) APIResponseCodesEnum.SignalRError;
                if (!isClientConnected)
                {
                    response.Messages.Add("Failed to reach the owner client, changes were not saved.");
                }

                if (!isRescuerConnected)
                {
                    response.Messages.Add("Failed to reach the chosen rescuer, changes were not saved.");
                }

                _logger.LogInformation(ConsoleFormatter.WrapSOSBusiness($"Couldn't accept the request, either owner client or selected rescuer was currently offline."));

                return response;
            }

            // First: Try Notify the Client, there shouldn't be error unless there is a logical error - bug
            var clientNotificationResult = TryNotifyOwnerClient(requestId, newState);
            if (clientNotificationResult != null)
            {
                response.WrapResponseData(clientNotificationResult);
                return response;
            }

            // Then: Try Notify the Rescuer, there shouldn't be error unless there is a logical error - bug
            var rescuerNotificationResult = TryNotifyRescuer(requestId, rescuerEmail);
            if (rescuerNotificationResult != null)
            {
                response.WrapResponseData(rescuerNotificationResult);
                return response;
            }

            // Finally: Try Notify the agent of the update. (No check needed for now)
            TryNotifyAgent(ref sosRequest, newState);

            UpdateSOSInline(ref sosRequest, newState, rescuer.Id);
            SaveChangesToRepository(ref sosRequest);

            response.WrapResponseData(FinalizeWithSuccessState(requestId, newState));
            response.Result = true;

            _logger.LogInformation(ConsoleFormatter.onSOSStateChanged(null, sosRequest.Id, StatesTypesEnum.Accepted));

            return response;
        }

        public async Task<APIResponse<bool>> CancelSOSRequestAsync(int requestId, string clientUserId, bool notifyClient)
        {
            const int newState = (int)StatesTypesEnum.Canceled;
            APIResponse<bool> response = new APIResponse<bool>();

            var nullSOSCheckResult = CheckForNullSOS(requestId, out SOSRequest sosRequest);
            if (nullSOSCheckResult != null)
            {
                response.WrapResponseData(nullSOSCheckResult);
                return response;
            }

            var authorizedUserCheck = CheckClientHaveAccessToSOS(sosRequest, clientUserId);
            if (authorizedUserCheck != null)
            {
                response.WrapResponseData(authorizedUserCheck);
                return response;
            }

            var sosCorrectStateCheckResult = CheckSOSState(sosRequest, (int)StatesTypesEnum.Solved, false);
            if (sosCorrectStateCheckResult != null)
            {
                response.WrapResponseData(sosCorrectStateCheckResult);
                return response;
            }

            // First: Try Notify the Rescuer and make him idle, no need to break the execution if rescuer was offline at the time of sending
            // the notification, he will be release and made idle again once he's online.
            var assignedRescuer = await _userManager.FindByIdAsync(sosRequest.AssignedRescuerId ?? "");
            var rescuerNotificationResult = TryNotifyRescuer(requestId, assignedRescuer?.Email);
            if (rescuerNotificationResult != null)
            {
                response.WrapResponseData(rescuerNotificationResult);
                response.Messages.Add("Either there was no rescuer assigned to this request yet, or he was currently offline.");
            }
            _rescuerHub.MakeRescuerIdle(assignedRescuer?.Email);

            if (notifyClient)
            {
                // Then: Try Notify the Client and remove from trackers, no need to break the execution if rescuer was offline at the time of notifying
                var clientNotificationResult = TryNotifyOwnerClient(requestId, newState);
                if (clientNotificationResult != null)
                {
                    response.WrapResponseData(clientNotificationResult);
                }
                _clientHub.RemoveClientFromTrackers(clientUserId);
            }
            
            // Finally: Try Notify the agent of the update. (No check needed for now)
            TryNotifyAgent(ref sosRequest, newState);

            UpdateSOSInline(ref sosRequest, newState, null);
            SaveChangesToRepository(ref sosRequest);

            response.WrapResponseData(FinalizeWithSuccessState(requestId, newState));
            response.Status = (int) APIResponseCodesEnum.Ok;
            response.Result = true;

            _logger.LogInformation(ConsoleFormatter.onSOSStateChanged(null, sosRequest.Id, StatesTypesEnum.Canceled));

            return response;
        }

        public async Task<APIResponse<bool>> CancelPendingRequestsAsync(string userId, bool notifyClient)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            var ongoingRequests = _sosRequestRepository.GetOngoingRequest(userId).ToList();

            if (!ongoingRequests.Any())
            {
                response.Messages.Add("You did not have any pending requests.");
                return response;
            }

            foreach (var request in ongoingRequests)
            {
                var updateResult = await CancelSOSRequestAsync(request.Id, userId, notifyClient);
                if (updateResult.HasErrors)
                {
                    return updateResult;
                }
            }

            response.Messages.Add("You just canceled " + ongoingRequests.Count() + " requests.");
            return response;
        }

        public async Task<APIResponse<bool>> SolveSOSRequestAsync(int requestId, string rescuerUserId)
        {
            const int newState = (int)StatesTypesEnum.Solved;
            APIResponse<bool> response = new APIResponse<bool>();

            var nullSOSCheckResult = CheckForNullSOS(requestId, out SOSRequest sosRequest);
            if (nullSOSCheckResult != null)
            {
                response.WrapResponseData(nullSOSCheckResult);
                return response;
            }
            
            var authorizedUserCheck = CheckRescuerHaveAccessToSOS(sosRequest, rescuerUserId);
            if (authorizedUserCheck != null)
            {
                response.WrapResponseData(authorizedUserCheck);
                return response;
            }

            var sosCorrectStateCheckResult = CheckSOSState(sosRequest, (int)StatesTypesEnum.Accepted, true);
            if (sosCorrectStateCheckResult != null)
            {
                response.WrapResponseData(sosCorrectStateCheckResult);
                return response;
            }

            // First: Release rescuer and make him idle again.
            var assignedRescuer = await _userManager.FindByIdAsync(sosRequest.AssignedRescuerId ?? "");
            _rescuerHub.MakeRescuerIdle(assignedRescuer?.Email);

            // Then: Try Notify the Client and remove from trackers, no need to break the execution if rescuer was offline at the time of notifying
            var clientNotificationResult = TryNotifyOwnerClient(requestId, newState);
            if (clientNotificationResult != null)
            {
                response.WrapResponseData(clientNotificationResult);
            }
            _clientHub.RemoveClientFromTrackers(sosRequest.UserId);

            // Finally: Try Notify the agent of the update. (No check needed for now)
            TryNotifyAgent(ref sosRequest, newState);

            UpdateSOSInline(ref sosRequest, newState, sosRequest.AssignedRescuerId);
            SaveChangesToRepository(ref sosRequest);

            response.WrapResponseData(FinalizeWithSuccessState(requestId, newState));
            response.Messages.Add("Success. You are now idling.");
            response.Status = (int)APIResponseCodesEnum.Ok;
            response.Result = true;

            _logger.LogInformation(ConsoleFormatter.onSOSStateChanged(assignedRescuer?.Email, sosRequest.Id, StatesTypesEnum.Solved));

            return response;
        }

        public APIResponse<bool> ResetSOSRequest(int requestId)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            var nullSOSCheckResult = CheckForNullSOS(requestId, out SOSRequest sosRequest);
            if (nullSOSCheckResult != null)
            {
                response.WrapResponseData(nullSOSCheckResult);
                return response;
            }

            var resetRescuersTrackersResult = ResetRescuerTrackersByRequestId(requestId);
            if (resetRescuersTrackersResult != null)
            {
                response.WrapResponseData(resetRescuersTrackersResult);
            }
            

            UpdateSOSInline(ref sosRequest, (int)StatesTypesEnum.Pending, null);
            SaveChangesToRepository(ref sosRequest);
            response.Messages.Add("Request was reset to pending.");
            response.Messages.Add("Removed Assigned Rescuers from the database.");

            // Reassign user to trackers
            
            var clientConnectionInfo = TrackerHandler.ClientConnectionInfoSet.FirstOrDefault(c => c.UserId == sosRequest.UserId);
            if (clientConnectionInfo != null)
            {
                clientConnectionInfo.SOSId = requestId;
                response.Messages.Add("Reattached Client to trackers.");
            }
            else
            {
                _sosRequestRepository.RemoveById(sosRequest.Id.ToString());
                _sosRequestRepository.Save();

                response.Messages.Add("Could not reach client by email, request was deleted.");
            }

            response.Result = true;
            return response;
        }

        public APIResponse<bool> RateRescuerAsync(string userId, int requestId, int rate)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            var nullSOSCheckResult = CheckForNullSOS(requestId, out SOSRequest sosRequest);
            if (nullSOSCheckResult != null)
            {
                response.WrapResponseData(nullSOSCheckResult);
                return response;
            }

            var authorizedUserCheck = CheckClientHaveAccessToSOS(sosRequest, userId);
            if (authorizedUserCheck != null)
            {
                response.WrapResponseData(authorizedUserCheck);
                return response;
            }

            var sosCorrectStateCheckResult = CheckSOSState(sosRequest, (int)StatesTypesEnum.Solved, true);
            if (sosCorrectStateCheckResult != null)
            {
                response.WrapResponseData(sosCorrectStateCheckResult);
                return response;
            }

            var rescuerData = _personnelRepository.GetById(sosRequest.AssignedRescuerId);
            if(rescuerData == null)
            {
                response.Status = (int)APIResponseCodesEnum.ServerError;
                response.HasErrors = true;
                response.Messages.Add("An error occured while retrieving the assigned Rescuer account data.");
                return response;
            }

            var rescuerRate = new Rate
            {
                RateAverage = rescuerData.RateAverage,
                RateCount = rescuerData.RateCount
            };

            if (rate > 5)
            {
                rate = 5;
                response.Messages.Add("WARNING: Out of bounds rating score. Your rating was adjusted to the maximum bounds.");
            } else if(rate < 1)
            {
                rate = 1;
                response.Messages.Add("WARNING: Out of bounds rating score. Your rating was adjusted to the minimum bounds.");
            }

            RateHelper.UpdateUserRate(ref rescuerRate, rate);

            rescuerData.RateAverage = rescuerRate.RateAverage;
            rescuerData.RateCount = rescuerRate.RateCount;

            _personnelRepository.Update(rescuerData);
            _personnelRepository.Save();
            
            response.Status = (int)APIResponseCodesEnum.Ok;
            response.Result = true;
            response.Messages.Add($"Rescuer rate updated. His new rate is {rescuerData.RateAverage}.");

            return response;
        }

        public APIResponse<List<GetSOSRequestForUserViewModel>> GetSOSRequestsHistory(string userId)
        {
            var response = new APIResponse<List<GetSOSRequestForUserViewModel>>();
            var responseViewModel = new List<GetSOSRequestForUserViewModel>(); 

            var nullClientCheckResult = CheckForNullClient(userId, out Client client);
            if (nullClientCheckResult != null)
            {
                response.WrapResponseData(nullClientCheckResult);
                return response;
            }

            var userRequests = _sosRequestRepository.GetRequestsForUser(client.ClientId);

            foreach(var request in userRequests)
            {
                responseViewModel.Add(new GetSOSRequestForUserViewModel
                {
                    RequestId = request.Id,
                    RequestStateId = request.State,
                    RequestStateName = ((StatesTypesEnum)request.State).ToString(),
                    AuthorityTypeId = request.AuthorityType,
                    AuthorityTypeName = ((AuthorityTypesEnum)request.AuthorityType).ToString(),
                    RequestCreationDate = request.CreationDate,
                    RequestLastModified = request.LastModified,
                    RequestLocationLatitude = request.Longitude,
                    RequestLocationLongitude = request.Latitude
                });
            }

            response.Result = responseViewModel;
            return response;
        }

        #endregion

        #region Private Helper Methods

        #region SignalR Related Methods

        private void TryNotifyAgent(ref SOSRequest sosRequest, int newStatus)
        {
            var sosRequestDepartment = _departmentRepository.GetById(sosRequest.AssignedDepartmentId.ToString());
            _agentHub.NotifyNewChanges(sosRequest.Id, newStatus, sosRequestDepartment.ToString());
        }

        private APIResponseData TryNotifyRescuer(int requestId, string rescuerEmail)
        {
            var notifierResult = _rescuerHub.NotifyNewChanges(requestId, rescuerEmail);

            if (!notifierResult)
            {
                return new APIResponseData((int)APIResponseCodesEnum.SignalRError,
                    new List<string>()
                        {"An error occured while trying to reach this rescuer."});
            }

            return null;
        }

        private APIResponseData TryNotifyOwnerClient(int requestId, int newStatus)
        {
            var notifierResult = _clientHub.NotifyUserSOSState(requestId, newStatus);
            
            if (!notifierResult)
            {
                return new APIResponseData((int)APIResponseCodesEnum.SignalRError,
                    new List<string>()
                        {"An error occured while trying to reach the owner client."});
            }

            return null;
        }

        private APIResponseData CheckIfRescuerIsOnMission(string rescuerEmail)
        {
            if (!_rescuerHub.IsIdle(rescuerEmail))
            {
                return new APIResponseData((int)APIResponseCodesEnum.BadRequest,
                    new List<string>()
                        {"Error. Current rescuer is on a mission."});
            }

            return null;
        }

        private APIResponseData ResetRescuerTrackersByRequestId(int requestId)
        {
            List<string> cumulativeMessages = new List<string>();

            // Reset rescuer trackers
            // Try find rescuer from connectionInfoSet
            var assignedRescuerConnectionInfo = TrackerHandler.RescuerConnectionInfoSet.Where(r => r.CurrentJob == requestId).ToList();
            if (!assignedRescuerConnectionInfo.Any())
            {
                cumulativeMessages.Add("No rescuers related to this SOSRequest were tracked in the main trackers.");
            }

            foreach (var rescuer in assignedRescuerConnectionInfo)
            {
                rescuer.CurrentJob = 0;
                cumulativeMessages.Add("Rescuer " + rescuer.UserEmail + " has been set to idling.");
            }

            //Try find rescuer from pendingMissionsConnectionInfoSet
            assignedRescuerConnectionInfo = TrackerHandler.RescuerWithPendingMissionsSet
                .Where(r => r.CurrentJob == requestId).ToList();

            if (!assignedRescuerConnectionInfo.Any())
            {
                cumulativeMessages.Add("No rescuers related to this SOSRequest were tracked in the pending trackers.");
            }

            foreach (var rescuer in assignedRescuerConnectionInfo)
            {
                TrackerHandler.RescuerWithPendingMissionsSet.Remove(rescuer);
                cumulativeMessages.Add("Rescuer " + rescuer.UserEmail + " has been set to idling. He was offline before being idle.");
            }


            return new APIResponseData((int)APIResponseCodesEnum.Ok, cumulativeMessages);
        }

        #endregion

        #region Private Checkers

        private APIResponseData CheckForNullRescuer(string rescuerEmail, out ApplicationUser rescuer)
        {
            rescuer = _userManager.FindByEmailAsync(rescuerEmail).Result;
            if (rescuer == null)
            {
                return new APIResponseData((int)APIResponseCodesEnum.NotFound,
                    new List<string>()
                        {"Error. No Rescuer was found with the provided Email. Check the email for typos."});
            }

            return null;
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

        private APIResponseData CheckForNullSOS(int requestId, out SOSRequest sosRequest)
        {
            sosRequest = _sosRequestRepository.GetById(requestId.ToString());

            if (sosRequest == null)
            {
                return new APIResponseData((int) APIResponseCodesEnum.NotFound,
                    new List<string>()
                        {"The SOS Request you are trying to modify was not found, did you mistype the ID?"});
            }

            return null;
        }

        private APIResponseData CheckSOSSameDepartment(ref SOSRequest sosRequest, ref ApplicationUser rescuer)
        {
            var personnelDpt = _personnelRepository.GetPersonnelDepartment(rescuer.Id);

            if (sosRequest.AssignedDepartmentId != personnelDpt.Id)
            {
                return new APIResponseData((int)APIResponseCodesEnum.BadRequest,
                    new List<string>()
                        {"Error: The SOS Request you are trying to access was not assigned to this rescuer's department."});
            }

            return null;
        }

        private APIResponseData CheckSOSState(SOSRequest sosRequest, int state, bool isInTheState)
        {
            if(isInTheState && sosRequest.State != state)
            {
                return new APIResponseData((int)APIResponseCodesEnum.BadRequest,
                    new List<string>()
                        {"Error. The SOS Request you are trying to modify is not in the "+ (StatesTypesEnum)state +" state."});

            }
            
            if (!isInTheState && sosRequest.State == state)
            {
                return new APIResponseData((int)APIResponseCodesEnum.BadRequest,
                    new List<string>()
                        {"Error. The SOS Request you are trying to modify is in the "+ (StatesTypesEnum)state +" state."});
            }

            return null;
        }

        private APIResponseData CheckClientHaveAccessToSOS(SOSRequest sosRequest, string clientUserId)
        {
            if (sosRequest.UserId != clientUserId)
            {
                return new APIResponseData((int)APIResponseCodesEnum.Unauthorized,
                    new List<string>()
                        {"You are not authorized to modify this SOS Request."});
            }

            return null;
        }

        private APIResponseData CheckRescuerHaveAccessToSOS(SOSRequest sosRequest, string rescuerUserId)
        {
            if (sosRequest.AssignedRescuerId != rescuerUserId)
            {
                return new APIResponseData((int)APIResponseCodesEnum.Unauthorized,
                    new List<string>()
                        {"You were not assigned to this request."});
            }

            return null;
        }

        #endregion

        #region General Usage

        private APIResponseData FinalizeWithSuccessState(int requestId, int newState)
        {
            var messages = new List<string>
            {
                "Success!",
                "SOS Request of Id: " + requestId + " was " + ((StatesTypesEnum)newState) + "."
            };
            return new APIResponseData((int)APIResponseCodesEnum.Ok, messages);
        }

        private void UpdateSOSInline(ref SOSRequest sosRequest, int newState, string rescuerId)
        {
            sosRequest.State = newState;
            sosRequest.AssignedRescuerId = rescuerId;
            sosRequest.LastModified = DateTime.Now;
        }

        private void SaveChangesToRepository(ref SOSRequest sosRequest)
        {
            _sosRequestRepository.Update(sosRequest);
            _sosRequestRepository.Save();
        }

        #endregion

        #endregion
    }
}
