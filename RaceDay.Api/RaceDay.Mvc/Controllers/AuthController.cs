
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using System.Net.Http.Json;

namespace RaceDay.Mvc.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApiService _apiService;

        public AuthController(ApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _apiService.PostAsync(
                "api/Auth/register",
                new
                {
                    model.Email,
                    model.Password
                });

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Registration successful. Please log in.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("",
                "Registration failed. This email may already be registered.");

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _apiService.PostAsync(
                "api/Auth/login",
                new
                {
                    model.Email,
                    model.Password
                });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (result == null)
            {
                ModelState.AddModelError("", "Unable to process login response.");
                return View(model);
            }

            HttpContext.Session.SetInt32("UserId", result.UserId);
            HttpContext.Session.SetString("UserRole", result.Role);
            HttpContext.Session.SetString("UserEmail", result.Email);

            if (result.Role == "Organiser")
                return RedirectToAction("Index", "Organiser");

            return RedirectToAction("Index", "Participant");
        }
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }
    }

    public class LoginResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public string Message { get; set; } = "";
    }
}
