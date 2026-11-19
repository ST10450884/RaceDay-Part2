
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApiService _apiService;

        public ProfileController(ApiService apiService)
        {
            _apiService = apiService;
        }

        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId") != null;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            try
            {
                var profile = await _apiService.GetAsync<ProfileViewModel>(
                    "api/Profiles/me");

                return View(profile ?? new ProfileViewModel());
            }
            catch (HttpRequestException ex)
                when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return View(new ProfileViewModel());
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Unable to load profile: " + ex.Message;
                return View(new ProfileViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                HttpResponseMessage response;

                try
                {
                    await _apiService.GetAsync<ProfileViewModel>(
                        "api/Profiles/me");

                    // Profile exists, so update it.
                    response = await _apiService.PutAsync(
                        "api/Profiles/me", model);
                }
                catch (HttpRequestException ex)
                    when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    // Profile does not exist, so create it.
                    response = await _apiService.PostAsync(
                        "api/Profiles", model);
                }

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "Your profile was saved successfully.";

                    return RedirectToAction("Index");
                }

                var error = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError("",
                    "Profile could not be saved: " + error);

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("",
                    "Profile operation failed: " + ex.Message);

                return View(model);
            }
        }
    }
}
