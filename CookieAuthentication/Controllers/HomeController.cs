using CookieAuthentication.Models;
using Google.Authenticator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CookieAuthentication.Controllers
{
    [Authorize(Roles ="Admin,User")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.Remarks = (TempData["remarks"]!= null) ? TempData["remarks"].ToString() : "TFA";
            return View();
        }

        [Authorize(Roles ="Admin")]
        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public IActionResult TFA()
        {
            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            var ScanDetails = tfa.GenerateSetupCode("CookieAuthentication", "AccountTitle", "SecretKey", false, 3);
            ViewBag.ManualKey = ScanDetails.ManualEntryKey;
            ViewBag.QRImage = ScanDetails.QrCodeSetupImageUrl;
            ViewBag.ErrorMessage = (TempData["remarks"] != null) ? TempData["remarks"].ToString() : "" ;
            return View();
        }
        [HttpPost]
        public IActionResult TFA(string txtScannedCode)
        {
            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            string remarks = string.Empty;
            bool isValid  = tfa.ValidateTwoFactorPIN("SecretKey", txtScannedCode, false);
            if(isValid)
            {
                remarks = "TFA Successful";
                TempData["remarks"] = remarks;
                return RedirectToAction("Index", "Home");
            }
            TempData["remarks"] = $"{txtScannedCode} is invalid, Try Agian!";
            return RedirectToAction("TFA");
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}