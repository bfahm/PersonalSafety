using PersonalSafety.Options;
using PersonalSafety.Models;
using PersonalSafety.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PersonalSafety.Hubs;
using PersonalSafety.Hubs.HubTracker;
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
            // No need to re notify the rescuer if he had just accepted the request himself (and to escape unintended bugs within trackers)
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
    }
}
