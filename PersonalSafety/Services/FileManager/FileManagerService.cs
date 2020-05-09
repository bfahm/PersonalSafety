using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<FileManagerService> _logger;

        public FileManagerService(IWebHostEnvironment env, AppSettings appSettings, ILogger<FileManagerService> logger)
        {
            _appSettings = appSettings;
            _env = env;
            // Only add the specific root folder while in development, this is added automatically in production
            _dir = _env.IsDevelopment() ? _env.ContentRootPath + "\\wwwroot" : _env.ContentRootPath;
            _logger = logger;
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
                    _logger.LogInformation($"Trying to uploading to path: {path}");

                    using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
                    file.CopyTo(fileStream);
                }
            }
            return savingLocations;
        }

        public void DeleteFile(string fileName)
        {
            try
            {
                var rootPath = Path.Combine(_dir, _appSettings.AttachmentsLocation);
                var existingFilePath = Path.Combine(rootPath, fileName);
                // Check if file exists with its full path    
                if (File.Exists(existingFilePath))
                {
                    // If file found, delete it    
                    File.Delete(existingFilePath);
                }
            }
            catch (IOException ioExp)
            {
                _logger.LogInformation("Error while trying to remove file: " + fileName + "\n" + "Exception Details: " + "\n"
                    + ioExp.Message);
            }
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

            _logger.LogInformation("Unsupported Attachments of type: " + ext);
            return false;
        }
    }
}
