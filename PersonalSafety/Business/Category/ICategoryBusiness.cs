using PersonalSafety.Contracts;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels;
using System.Collections.Generic;

namespace PersonalSafety.Business
{
    public interface ICategoryBusiness
    {
        APIResponse<List<EventCategory>> GetEventCategories();
        APIResponse<bool> NewEventCategory(NewEventCategoryViewModel request);
        APIResponse<bool> UpdateEventCategory(UpdateEventCategoryViewModel request);
        APIResponse<bool> DeleteEventCategory(int categoryId);
    }
}
