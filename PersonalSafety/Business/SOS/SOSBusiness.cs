using PersonalSafety.Options;
using PersonalSafety.Models;
using PersonalSafety.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using Microsoft.AspNetCore.Identity;
using PersonalSafety.Hubs;
using PersonalSafety.Hubs.Services;
using PersonalSafety.Models.ViewModels;

namespace PersonalSafety.Business
{
    public class SOSBusiness : ISOSBusiness
    {
        private readonly ISOSRequestRepository _sosRequestRepository;
        private readonly IClientRepository _clientRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRescuerHub _rescuerHub;
        private readonly IAgentHub _agentHub;
        private readonly IClientHub _clientHub;

        public SOSBusiness(ISOSRequestRepository sosRequestRepository, IClientRepository clientRepository, UserManager<ApplicationUser> userManager, IRescuerHub rescuerHub, IAgentHub agentHub, IClientHub clientHub)
        {
            _sosRequestRepository = sosRequestRepository;
            _clientRepository = clientRepository;
            _userManager = userManager;
            _rescuerHub = rescuerHub;
            _agentHub = agentHub;
            _clientHub = clientHub;
        }


        public async Task<APIResponse<SendSOSResponseViewModel>> SendSOSRequestAsync(string userId, SendSOSRequestViewModel request)
        {
            APIResponse<SendSOSResponseViewModel> response = new APIResponse<SendSOSResponseViewModel>();

            Client user = _clientRepository.GetById(userId);
            if (user == null)
            {
                response.Messages.Add("User not authorized.");
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.Unauthorized;
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
                response.Messages.Add("You currently have an ongoing request, cancel it first to be able to send a new request.");
                response.Status = (int)APIResponseCodesEnum.InvalidRequest;
                response.HasErrors = true;
                return response;
            }

            SOSRequest sosRequest = new SOSRequest
            {
                UserId = userId,
                AuthorityType = request.AuthorityType,
                Longitude = request.Longitude,
                Latitude = request.Latitude,
                AssignedDepartmentId = 1 // TODO: Fix this line when assigining to the nearest dpt
            };

            _sosRequestRepository.Add(sosRequest);
            _sosRequestRepository.Save();

            ApplicationUser userAccount = await _userManager.FindByIdAsync(userId);
            var trackingResult = _clientHub.TrackSOSIdForClient(userAccount.Email, sosRequest.Id);

            if (!trackingResult)
            {
                _clientHub.RemoveClientFromTrackers(sosRequest.Id);

                // Was not able to reach the user:
                // revert changes:
                _sosRequestRepository.RemoveById(sosRequest.Id.ToString());
                _sosRequestRepository.Save();

                response.Status = (int)APIResponseCodesEnum.SignalRError;
                response.Messages.Add("Invalid Attempt. You do not have a valid realtime connection.");
                response.Messages.Add("Your request was deleted.");
                return response;
            }

            _clientHub.NotifyUserSOSState(sosRequest.Id, (int)StatesTypesEnum.Pending);
            await _agentHub.NotifyNewChanges(sosRequest.Id, (int)StatesTypesEnum.Pending);



            response.Result = new SendSOSResponseViewModel
            {
                RequestId = sosRequest.Id,
                RequestStateId = sosRequest.State,
                RequestStateName = ((StatesTypesEnum)sosRequest.State).ToString(),
            };

            response.Messages.Add("Your request was sent successfully.");

            return response;
        }

        public async Task<APIResponse<bool>> UpdateSOSRequestAsync(int requestId, int newStatus, string issuerId, string rescuerEmail = null)
        {
            APIResponse<bool> response = new APIResponse<bool>();
            SOSRequest sosRequest = _sosRequestRepository.GetById(requestId.ToString());

            if (sosRequest == null)
            {
                response.Messages.Add("The SOS Request you are trying to modify was not found, did you mistype the ID?");
                response.HasErrors = true;
                response.Status = (int)APIResponseCodesEnum.NotFound;
                return response;
            }

            // If user is not a personnel (meaning he's a client), he must only modify his own requests.
            if(!await _userManager.IsInRoleAsync(await _userManager.FindByIdAsync(issuerId), Roles.ROLE_PERSONNEL))
            {
                if(sosRequest.UserId != issuerId)
                {
                    response.Messages.Add("You are not authorized to modify this SOS Request.");
                    response.HasErrors = true;
                    response.Status = (int)APIResponseCodesEnum.Unauthorized;
                    return response;
                }
            }

            sosRequest.State = newStatus;
            sosRequest.LastModified = DateTime.Now;

            // If the request was changed to a success state, save the rescuer data
            if (newStatus == (int) StatesTypesEnum.Accepted || newStatus == (int) StatesTypesEnum.Solved)
            {
                var rescuer = await _userManager.FindByEmailAsync(rescuerEmail);
                if (rescuer == null)
                {
                    response.Messages.Add("Error. Failed to notify the provided rescuer. Check the email for typos.");
                    response.Status = (int)APIResponseCodesEnum.BadRequest;
                    response.HasErrors = true;
                    return response;
                }

                if (!_rescuerHub.IsIdle(rescuer.Email))
                {
                    response.Messages.Add("Error. Current rescuer is on a mission and is not Idle.");
                    response.Status = (int)APIResponseCodesEnum.BadRequest;
                    response.HasErrors = true;
                    return response;
                }

                sosRequest.AssignedRescuerId = rescuer.Id;

                if (newStatus == (int) StatesTypesEnum.Solved)
                {
                    _rescuerHub.MakeRescuerIdle(rescuerEmail);
                }
            }
            

            // First: Notify the Client
            var notifierResult = _clientHub.NotifyUserSOSState(requestId, newStatus);
            // If notifier failed to reach user (he was offline)..
            // No need to revert the request if the user went offline, and the request was SOLVED
            if (!notifierResult && newStatus != (int)StatesTypesEnum.Solved)
            {
                response.Messages.Add("Failed to notify the user about the change, it appears he lost the connection.");
                response.Messages.Add("Request state was reverted to: 'Orphaned'");
                sosRequest.State = (int)StatesTypesEnum.Orphaned;

                _sosRequestRepository.Update(sosRequest);
                _sosRequestRepository.Save();

                return response;
            }

            // Then: Notify the agent of the update.
            await _agentHub.NotifyNewChanges(requestId, newStatus);

            // Finally: Notify the Rescuer with any changes if he were supposed to be notified
            // No need to re notify the rescuer if he had just solved the request himself (and to escape unintended bugs within trackers)
            if ((!string.IsNullOrEmpty(rescuerEmail) || newStatus == (int)StatesTypesEnum.Canceled) && newStatus != (int)StatesTypesEnum.Solved)
            {
                // WARNING: Smelly code incoming --
                /* Explanation:
                    This code block is accessed in the case of user canceling the request
                        If so: parameter rescuerEmail would be null, and we will try to fetch the rescuer email from the database (SOSRequest table)
                    OR
                    Any other rescuer-related logic (hence parameter rescuerEmail shouldn't be null)
                        If so: use the parameter rescuerEmail
                 */

                
                if (newStatus == (int) StatesTypesEnum.Canceled)
                {
                    if (sosRequest.AssignedRescuerId != null)
                    {
                        rescuerEmail = _userManager.FindByIdAsync(sosRequest?.AssignedRescuerId).Result.Email;
                        sosRequest.AssignedRescuerId = null;
                    }

                    _rescuerHub.MakeRescuerIdle(rescuerEmail);
                } // if the state was not: Canceled, rescuerEmail field should be the one in the function parameter

                var rescuerNotifierResult = _rescuerHub.NotifyNewChanges(requestId, rescuerEmail);

                if (newStatus == (int) StatesTypesEnum.Canceled)
                {
                    // If user is trying to cancel, no need to check if the rescuer is still online.
                    goto done;
                }
                
                if (!rescuerNotifierResult)
                {
                    response.Status = (int) APIResponseCodesEnum.SignalRError;
                    response.Messages.Add("Failed to notify the rescuer about the change, it appears he is not online.");
                    response.Messages.Add("Reverting Changes...");
                    response.Messages.Add("Changes were reverted back to 'Pending'.");

                    sosRequest.State = (int) StatesTypesEnum.Pending;

                    _sosRequestRepository.Update(sosRequest);
                    _sosRequestRepository.Save();

                    return response;
                }
                
            }

            done:
            _sosRequestRepository.Update(sosRequest);
            _sosRequestRepository.Save();

            // Remove the client from the trackers at terminating states
            if (sosRequest.State == (int) StatesTypesEnum.Solved ||
                sosRequest.State == (int) StatesTypesEnum.Canceled ||
                sosRequest.State == (int) StatesTypesEnum.Orphaned)
            {
                _clientHub.RemoveClientFromTrackers(requestId);
            }

            response.Messages.Add("Success!");
            response.Messages.Add("SOS Request of Id: " + requestId + " was modified.");
            response.HasErrors = false;
            return response;
        }

        public async Task<APIResponse<bool>> CancelPendingRequests(string userId)
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
                var updateResult = await UpdateSOSRequestAsync(request.Id, (int) StatesTypesEnum.Canceled, userId);
                if (updateResult.HasErrors)
                {
                    return updateResult;
                }
            }

            response.Messages.Add("You just canceled " + ongoingRequests.Count() + " requests.");
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

            var sosCorrectStateCheckResult = CheckSOSState(sosRequest, (int)StatesTypesEnum.Pending, false);
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

            var rescuerOnMissionCheckResult = CheckIfRescuerIsOnMission(rescuerEmail);
            if (rescuerOnMissionCheckResult != null)
            {
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

                return response;
            }

            // First: Try Notify the Rescuer, there shouldn't be error unless there is a logical error - bug
            var rescuerNotificationResult = TryNotifyRescuer(requestId, rescuerEmail);
            if (rescuerNotificationResult != null)
            {
                response.WrapResponseData(rescuerNotificationResult);
                return response;
            }

            // Then: Try Notify the Client, there shouldn't be error unless there is a logical error - bug
            var clientNotificationResult = TryNotifyOwnerClient(requestId, newState);
            if (clientNotificationResult != null)
            {
                response.WrapResponseData(clientNotificationResult);
                return response;
            }

            // Finally: Try Notify the agent of the update. (No check needed for now)
            TryNotifyAgent(requestId, newState);

            UpdateSOSInline(ref sosRequest, newState, rescuer.Id);
            SaveChangesToRepository(ref sosRequest);

            response.WrapResponseData(FinalizedWithSuccessState(requestId, newState));
            response.Result = true;
            return response;
        }

        public async Task<APIResponse<bool>> CancelSOSRequestAsync(int requestId, string clientUserId)
        {
            const int newState = (int)StatesTypesEnum.Canceled;
            APIResponse<bool> response = new APIResponse<bool>();

            var nullSOSCheckResult = CheckForNullSOS(requestId, out SOSRequest sosRequest);
            if (nullSOSCheckResult != null)
            {
                response.WrapResponseData(nullSOSCheckResult);
                return response;
            }

            var authorizedUserCheck = CheckHaveAccessToSOS(sosRequest, clientUserId);
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

            // Then: Try Notify the Client and remove from trackers, no need to break the execution if rescuer was offline at the time of notifying
            var clientNotificationResult = TryNotifyOwnerClient(requestId, newState);
            if (clientNotificationResult != null)
            {
                response.WrapResponseData(clientNotificationResult);
            }
            _clientHub.RemoveClientFromTrackers(requestId);

            // Finally: Try Notify the agent of the update. (No check needed for now)
            TryNotifyAgent(requestId, newState);

            UpdateSOSInline(ref sosRequest, newState, null);
            SaveChangesToRepository(ref sosRequest);

            response.WrapResponseData(FinalizedWithSuccessState(requestId, newState));
            response.Status = (int) APIResponseCodesEnum.Ok;
            response.Result = true;
            return response;
        }


        private async void TryNotifyAgent(int requestId, int newStatus)
        {
            // TODO: add here checks related to specifying agent by department
            await _agentHub.NotifyNewChanges(requestId, newStatus);
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

        private APIResponseData CheckSOSState(SOSRequest sosRequest, int state, bool isInTheState)
        {
            if(!isInTheState && sosRequest.State != state)
            {
                return new APIResponseData((int)APIResponseCodesEnum.BadRequest,
                    new List<string>()
                        {"Error. The SOS Request you are trying to modify is not in the "+ (StatesTypesEnum)state +" state."});

            }
            
            if (isInTheState && sosRequest.State == state)
            {
                return new APIResponseData((int)APIResponseCodesEnum.BadRequest,
                    new List<string>()
                        {"Error. The SOS Request you are trying to modify is in the "+ (StatesTypesEnum)state +" state."});
            }

            return null;
        }

        private APIResponseData CheckHaveAccessToSOS(SOSRequest sosRequest, string clientUserId)
        {
            if (sosRequest.UserId != clientUserId)
            {
                return new APIResponseData((int)APIResponseCodesEnum.Unauthorized,
                    new List<string>()
                        {"You are not authorized to modify this SOS Request."});
            }

            return null;
        }


        private APIResponseData FinalizedWithSuccessState(int requestId, int newState)
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
    }
}
