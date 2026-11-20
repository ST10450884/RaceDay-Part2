
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly ApiService _apiService;
        private readonly IConfiguration _configuration;

        public ParticipantController(
            ApiService apiService,
            IConfiguration configuration)
        {
            _apiService = apiService;
            _configuration = configuration;
        }

        private bool IsParticipant()
        {
            return HttpContext.Session.GetInt32("UserId") != null &&
                   HttpContext.Session.GetString("UserRole") == "Participant";
        }

        private void SetApiBaseUrl()
        {
            ViewBag.ApiBaseUrl =
                _configuration["ApiSettings:BaseUrl"]?.TrimEnd('/');
        }

        // DASHBOARD
        public IActionResult Index()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            var model = new ParticipantDashboardViewModel
            {
                ParticipantName =
                    HttpContext.Session.GetString("UserEmail") ?? "Participant"
            };

            return View(model);
        }

        // BROWSE EVENTS
        [HttpGet]
        public async Task<IActionResult> Events()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            SetApiBaseUrl();

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

        // VIEW EVENT CATEGORIES
        [HttpGet]
        public async Task<IActionResult> Categories(int id)
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            SetApiBaseUrl();

            try
            {
                var events = await _apiService.GetAsync<List<EventViewModel>>(
                    "api/Events");

                var selectedEvent = events?.FirstOrDefault(e => e.EventId == id);

                if (selectedEvent == null)
                    return NotFound();

                var categories =
                    await _apiService.GetAsync<List<EventCategoryViewModel>>(
                        "api/EventCategories");

                ViewBag.EventName = selectedEvent.Name;
                ViewBag.EventId = id;
                ViewBag.EventPosterUrl = selectedEvent.PosterUrl;

                var eventCategories = categories?
                    .Where(c => c.EventId == id)
                    .ToList() ?? new List<EventCategoryViewModel>();

                return View(eventCategories);
            }
            catch
            {
                ViewBag.Error = "Unable to load event categories.";
                return View(new List<EventCategoryViewModel>());
            }
        }

        // ENROL - DISPLAY FORM
        [HttpGet]
        public async Task<IActionResult> Enrol(int id)
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            try
            {
                var categories =
                    await _apiService.GetAsync<List<EventCategoryViewModel>>(
                        "api/EventCategories");

                var category = categories?
                    .FirstOrDefault(c => c.CategoryId == id);

                if (category == null)
                    return NotFound();

                var model = new CreateEnrolmentViewModel
                {
                    CategoryId = category.CategoryId,
                    EventId = category.EventId,
                    CategoryName = category.Name
                };

                return View(model);
            }
            catch
            {
                TempData["Error"] = "Unable to load the selected category.";
                return RedirectToAction("Events");
            }
        }

        // ENROL - SUBMIT FORM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(CreateEnrolmentViewModel model)
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var response = await _apiService.PostAsync(
                    "api/Enrolments",
                    new
                    {
                        model.CategoryId,
                        model.EmergencyContactName,
                        model.EmergencyContactPhone
                    });

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "Your enrolment was submitted successfully.";

                    return RedirectToAction(
                        "Categories",
                        new { id = model.EventId });
                }

                var errorMessage =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    "",
                    "Enrolment could not be completed. Check that your profile has a date of birth and that the category is still open. " +
                    errorMessage);

                return View(model);
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Unable to contact the RaceDay API. Please try again.");

                return View(model);
            }
        }

        // MY ENROLMENTS
        [HttpGet]
        public async Task<IActionResult> MyEnrolments()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            try
            {
                var enrolments =
                    await _apiService.GetAsync<List<EnrolmentViewModel>>(
                        "api/Enrolments/my");

                return View(enrolments ?? new List<EnrolmentViewModel>());
            }
            catch
            {
                ViewBag.Error =
                    "Unable to load your enrolments. Please try again.";

                return View(new List<EnrolmentViewModel>());
            }
        }

        // MY RACE RESULTS
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            try
            {
                var results =
                    await _apiService.GetAsync<List<ResultViewModel>>(
                        "api/Results");

                return View(results ?? new List<ResultViewModel>());
            }
            catch
            {
                ViewBag.Error = "Unable to load your race results.";
                return View(new List<ResultViewModel>());
            }
        }
    }
}
