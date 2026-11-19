
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly ApiService _apiService;

        public ParticipantController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (userId == null || role != "Participant")
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new ParticipantDashboardViewModel
            {
                ParticipantName = HttpContext.Session.GetString("UserEmail") ?? "Participant"
            };

            return View(model);
        }

        public async Task<IActionResult> Events()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (userId == null || role != "Participant")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var events = await _apiService.GetAsync<List<EventViewModel>>(
                    "api/Events");

                return View(events ?? new List<EventViewModel>());
            }
            catch
            {
                ViewBag.Error = "Unable to load events. Please try again.";
                return View(new List<EventViewModel>());
            }
        }
    }
}
