using Microsoft.AspNetCore.Http;
using PersonalSafety.Contracts;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels;
using PersonalSafety.Options;
using PersonalSafety.Services.FileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Business.Category
{
    public class CategoryBusiness : ICategoryBusiness
    {
        private readonly IEventCategoryRepository _eventCategoryRepository;
        private readonly IFileManagerService _fileManager;
        private readonly AppSettings _appSettings;

        public CategoryBusiness(IEventCategoryRepository eventCategoryRepository, IFileManagerService fileManager, AppSettings appSettings)
        {
            _eventCategoryRepository = eventCategoryRepository;
            _fileManager = fileManager;
            _appSettings = appSettings;
        }

        public APIResponse<List<EventCategory>> GetEventCategories()
        {
            return new APIResponse<List<EventCategory>> { Result = _eventCategoryRepository.GetAll().ToList() };
        }

        public APIResponse<bool> NewEventCategory(NewEventCategoryViewModel request)
        {
            APIResponse<bool> response = new APIResponse<bool>();

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

            EventCategory category = new EventCategory
            {
                Title = request.Title,
                Description = request.Description,
                ThumbnailUrl = uploadResult
            };

            _eventCategoryRepository.Add(category);
            _eventCategoryRepository.Save();

            response.Messages.Add("A new category was successfully added to the system.");
            return response;
        }

        public APIResponse<bool> UpdateEventCategory(UpdateEventCategoryViewModel request)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            var existingCategory = _eventCategoryRepository.GetById(request.Id.ToString());
            if (existingCategory == null)
            {
                response.HasErrors = true;
                response.Messages.Add("The requested category Id was not found.");
                response.Status = (int)APIResponseCodesEnum.NotFound;
                return response;
            }

            if(request.Thumbnail != null)
            {
                var uploadResult = _fileManager.UploadImages(new List<IFormFile> { request.Thumbnail });
                if (uploadResult.Count != 1)
                {
                    response.HasErrors = true;
                    response.Messages.Add("An error occured while trying to upload the attachment.");
                    response.Status = (int)APIResponseCodesEnum.BadRequest;
                    return response;
                }
                else
                {
                    existingCategory.ThumbnailUrl = uploadResult[0];
                    response.Messages.Add("The thumbnail was updated.");
                }

                _fileManager.DeleteFile(existingCategory.ThumbnailUrl.Split(_appSettings.AttachmentsLocation + "\\")[1]);
            }

            existingCategory.Title = request.Title ?? existingCategory.Title;
            existingCategory.Description = request.Description ?? existingCategory.Description;

            _eventCategoryRepository.Update(existingCategory);
            _eventCategoryRepository.Save();

            response.Messages.Add("The category was successfully updated.");
            return response;
        }

        public APIResponse<bool> DeleteEventCategory(int categoryId)
        {
            APIResponse<bool> response = new APIResponse<bool>();

            var existingCategory = _eventCategoryRepository.GetById(categoryId.ToString());
            if (existingCategory == null)
            {
                response.HasErrors = true;
                response.Messages.Add("The requested category Id was not found.");
                response.Status = (int)APIResponseCodesEnum.NotFound;
                return response;
            }

            if(existingCategory.ThumbnailUrl != null)
            {
                _fileManager.DeleteFile(existingCategory.ThumbnailUrl.Split(_appSettings.AttachmentsLocation + "\\")[1]);
            }
            
            _eventCategoryRepository.RemoveById(categoryId.ToString());
            _eventCategoryRepository.Save();

            response.Messages.Add("The category was successfully removed.");
            return response;
        }
    }
}
