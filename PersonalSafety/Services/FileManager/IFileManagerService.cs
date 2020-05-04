using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace PersonalSafety.Services.FileManager
{
    public interface IFileManagerService
    {
        List<string> UploadImages(List<IFormFile> files);
        void DeleteFile(string fileName);
    }
}
