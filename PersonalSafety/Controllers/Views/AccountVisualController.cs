using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Models.ViewModels;
using PersonalSafety.Services;

namespace PersonalSafety.Controllers.Views
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AccountVisualController : Controller
    {
        private readonly IAccountBusiness _accountBusiness;

        public AccountVisualController(IAccountBusiness accountBusiness)
        {
            _accountBusiness = accountBusiness;
        }

        //--------------------------------------------------------------------
        //RESSETING PASSWORD

        [Route("ResetPassword")]
        [HttpGet]
        public IActionResult ResetPassword([FromQuery]string email, [FromQuery]string token)
        {
            if (email == null || token == null)
            {
                return RedirectToAction("ResetPasswordResult");
            }
            return View();
        }

        [Route("ResetPassword")]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel request)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountBusiness.ResetPasswordAsync(request);
                bool booleanResult = result.Status == 0 && !result.HasErrors;
                return RedirectToAction("ResetPasswordResult", new { result = booleanResult });
            }
            else
            {
                return View();
            }

        }

        [Route("ResetPasswordResult")]
        [HttpGet]
        public IActionResult ResetPasswordResult(bool result)
        {
            ViewBag.Result = result;
            return View();
        }

        //--------------------------------------------------------------------
        //RESENDING FORGET PASSWORD MAIL

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

        //--------------------------------------------------------------------
        //SENDING CONFIRMATION EMAIL

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

        //--------------------------------------------------------------------
        //CONFIRMING EMAIL

        [Route("ConfirmMail")]
        [HttpGet]
        public async Task<IActionResult> ConfirmMail(ConfirmMailViewModel request)
        {
            if (ModelState.IsValid) 
            { 
                var result = await _accountBusiness.ConfirmMailAsync(request);
                bool booleanResult = result.Status == 0 && !result.HasErrors;
                return RedirectToAction("ConfimMailResult", new { result = booleanResult });
            }
            else
            {
                return BadRequest();
            }
        }

        [Route("ConfimMailResult")]
        [HttpGet]
        public IActionResult ConfimMailResult(bool result)
        {
            ViewBag.Result = result;
            return View();
        }

    }
}