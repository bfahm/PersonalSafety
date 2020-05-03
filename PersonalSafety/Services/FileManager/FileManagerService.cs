using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using PersonalSafety.Options;
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
        private readonly AppSettings _appSettings;
        private readonly string _dir;

        public FileManagerService(IWebHostEnvironment env, AppSettings appSettings)
        {
            _appSettings = appSettings;
            _env = env;
            _dir = _env.ContentRootPath + "\\wwwroot";
        }

        public List<string> UploadFiles(List<IFormFile> files)
        {
            var savingLocations = new List<string>();
            foreach(var file in files)
            {
                string ext = file.FileName.Split('.')[1];
                string fileName = Guid.NewGuid().ToString() + "." + ext;
                string savingLocation = Path.Combine(_appSettings.AttachmentsLocation, fileName);
                savingLocations.Add(savingLocation);

                var path = Path.Combine(_dir, savingLocation);
                using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
                file.CopyTo(fileStream);
            }
            return savingLocations;
        }
    }
}
