using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Services.FileManager
{
    public interface IFileManagerService
    {
        List<string> UploadImages(List<IFormFile> files);
    }
}
