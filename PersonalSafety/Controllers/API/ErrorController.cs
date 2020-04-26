using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Contracts;

namespace PersonalSafety.Controllers.API
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        [Route("Error/{statusCode}")]
        [HttpGet]
        public object HttpStatusCodeHandler(int statusCode)
        {
            APIResponse<string> response = new APIResponse<string>();

            response.Status = statusCode;

            switch (statusCode)
            {
                case 404:
                    response.Messages.Add("The requested url could not be found");
                    break;
                case 401:
                    response.Messages.Add("You are not authorized. Please login or register to continue.");
                    break;
            }
            return response;

        }
    }
}