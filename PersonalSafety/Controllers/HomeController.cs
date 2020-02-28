using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Models.ViewModels;
using PersonalSafety.Services;

namespace PersonalSafety.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        private readonly IAccountBusiness _accountBusiness;

        public HomeController(IAccountBusiness accountBusiness)
        {
            _accountBusiness = accountBusiness;
        }


        [Route("")]
        [Route("home")]
        [Route("index")]
        [Route("home/index")]
        [Route("api")]
        [Route("api/home")]
        [Route("api/home/index")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        [Route("ForgotPassword")]
        [HttpGet]
        public IActionResult ForgotPassword([FromQuery]string email, [FromQuery]string token)
        {
            if(email == null|| token == null)
            {
                return RedirectToAction("PasswordResetResult");
            }
            return View();
        }

        [Route("ForgotPassword")]
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ResetPasswordViewModel request)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountBusiness.ResetPasswordAsync(request);
                bool booleanResult = result.Status == 0 && !result.HasErrors;
                return RedirectToAction("PasswordResetResult", new { result = booleanResult });
            }
            else
            {
                return View();
            }
            
        }

        [Route("PasswordResetResult")]
        [HttpGet]
        public IActionResult PasswordResetResult(bool result)
        {
            ViewBag.Result = result;
            return View();
        }

        [Route("ForgotPasswordMail")]
        [HttpGet]
        public IActionResult ForgotPasswordMail()
        {
            return View();
        }

        [Route("ForgotPasswordMail")]
        [HttpPost]
        public async Task<IActionResult> ForgotPasswordMail(string email)
        {
            var result = await _accountBusiness.ForgotPasswordAsync(email);
            bool booleanResult = result.Status == 0 && !result.HasErrors;
            return RedirectToAction("ForgotPasswordMailResult", new { result = booleanResult });
        }

        [Route("ForgotPasswordMailResult")]
        [HttpGet]
        public IActionResult ForgotPasswordMailResult(bool result)
        {
            ViewBag.Result = result;
            return View();
        }

        [Route("ConfirmMail")]
        [HttpGet]
        public async Task<IActionResult> ConfirmMail(ConfirmMailViewModel request)
        {
            var result = await _accountBusiness.ConfirmMailAsync(request);
            bool booleanResult = result.Status == 0 && !result.HasErrors;
            return RedirectToAction("ConfimMailResult", new { result = booleanResult });
        }

        [Route("ConfimMailResult")]
        [HttpGet]
        public IActionResult ConfimMailResult(bool result)
        {
            ViewBag.Result = result;
            return View();
        }

        [Route("SendConfirmationMail")]
        [HttpGet]
        public IActionResult SendConfirmationMail()
        {
            return View();
        }

        [Route("SendConfirmationMail")]
        [HttpPost]
        public async Task<IActionResult> SendConfirmationMail(string email)
        {
            var result = await _accountBusiness.SendConfirmMailAsync(email);
            bool booleanResult = result.Status == 0 && !result.HasErrors;
            return RedirectToAction("SendConfirmationMailResult", new { result = booleanResult });
        }

        [Route("SendConfirmationMailResult")]
        [HttpGet]
        public IActionResult SendConfirmationMailResult(bool result)
        {
            ViewBag.Result = result;
            return View();
        }

    }
}