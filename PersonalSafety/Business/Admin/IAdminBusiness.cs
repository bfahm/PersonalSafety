﻿using PersonalSafety.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels.AdminVM;
using PersonalSafety.Models;

namespace PersonalSafety.Business
{
    public interface IAdminBusiness
    {
        Task<APIResponse<bool>> RegisterAgentAsync(RegisterAgentViewModel request);
        Task<APIResponse<bool>> RegisterManagerAsync(RegisterManagerViewModel request);
        Task<APIResponse<bool>> ModifyManagerAccessAsync(ModifyManagerViewModel request);
        APIResponse<Dictionary<string, object>> RetrieveTrackers();
        APIResponse<object> RetrieveConsole();
        APIResponse<bool> ResetTrackers();
        APIResponse<bool> ResetConsole();
        APIResponse<bool> ResetRescuerState(string rescuerEmail);
        APIResponse<bool> ResetClientState(string clientEmail);
        APIResponse<DistributionTreeViewModel> GetDistributionTree();
        APIResponse<List<DistributionNodeViewModel>> GetDistributionNodes();
        APIResponse<List<DistributionNodeViewModel>> GetDistributionCities();
        APIResponse<DistributionTreeViewModel> AddNewDistribution(NewDistributionRequestViewModel request);
        APIResponse<DistributionTreeViewModel> RenameDistribution(RenameDistributionRequestViewModel request);
    }
}
