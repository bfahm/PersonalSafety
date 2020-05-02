using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Services.FileManager;

namespace PersonalSafety.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileManagerService _fileManager;

        public FileController(IFileManagerService fileManager)
        {
            _fileManager = fileManager;
        }

        [HttpPost]
        public IActionResult UploadFile(IFormFile file)
        {
            var response = _fileManager.UploadFile(file);
            return Ok(response);
        }
    }
}