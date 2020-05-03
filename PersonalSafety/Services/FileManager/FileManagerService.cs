using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using PersonalSafety.Options;
using System;
using System.Collections.Generic;
using System.IO;

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

        public List<string> UploadImages(List<IFormFile> files)
        {
            var savingLocations = new List<string>();
            foreach(var file in files)
            {
                var ext = "";
                var extSplit = file.FileName.Split('.');
                if (extSplit.Length > 1)
                    ext = extSplit[1];

                if (IsSupported(ext))
                {
                    string fileName = Guid.NewGuid().ToString() + "." + ext;
                    string savingLocation = Path.Combine(_appSettings.AttachmentsLocation, fileName);
                    savingLocations.Add(savingLocation);

                    var path = Path.Combine(_dir, savingLocation);
                    using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
                    file.CopyTo(fileStream);
                }
            }
            return savingLocations;
        }

        private bool IsSupported(string ext)
        {
            List<string> supportedFileTypes = new List<string> 
            {
                "jpg",
                "jpeg",
                "png"
            };

            ext = ext.ToLower();
            if (supportedFileTypes.Contains(ext))
                return true;

            return false;
        }
    }
}
