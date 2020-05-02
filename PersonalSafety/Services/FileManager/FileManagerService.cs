using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Services.FileManager
{
    public class FileManagerService : IFileManagerService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _dir;
        private readonly string folderName = "Uploads";

        public FileManagerService(IWebHostEnvironment env)
        {
            _env = env;
            _dir = _env.ContentRootPath + "\\wwwroot";
        }

        public string UploadFile(IFormFile file)
        {
            string ext = file.FileName.Split('.')[1];
            string fileName = Guid.NewGuid().ToString() + "." + ext;
            string savingLocation = Path.Combine(folderName, fileName);
            
            var path = Path.Combine(_dir, savingLocation);
            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                file.CopyTo(fileStream);
            }
            return savingLocation;
        }

        public string RetrieveFile(string guid)
        {
            throw new NotImplementedException();
        }
    }
}
