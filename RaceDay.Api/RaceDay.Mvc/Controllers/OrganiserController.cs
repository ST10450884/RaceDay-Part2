
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly ApiService _apiService;

        public OrganiserController(ApiService apiService)
        {
            _apiService = apiService;
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetInt32("UserId") != null &&
                   HttpContext.Session.GetString("UserRole") == "Organiser";
        }

        private int? OrganiserId =>
            HttpContext.Session.GetInt32("UserId");

        public IActionResult Index()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            ViewBag.OrganiserEmail =
                HttpContext.Session.GetString("UserEmail") ?? "Organiser";

            return View();
        }

        public async Task<IActionResult> Events()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            try
            {
                var allEvents =
                    await _apiService.GetAsync<List<EventViewModel>>("api/Events");

                var organiserEvents = allEvents?
                    .Where(e => e.OrganiserId == OrganiserId)
                    .ToList() ?? new List<EventViewModel>();

                return View(organiserEvents);
            }
            catch
            {
                ViewBag.Error = "Unable to load events. Please try again.";
                return View(new List<EventViewModel>());
            }
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            return View(new CreateEventViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            if (model.EventDate.HasValue &&
                model.RegistrationCloseDate.HasValue &&
                model.RegistrationCloseDate >= model.EventDate)
            {
                ModelState.AddModelError(
                    "RegistrationCloseDate",
                    "Registration closing date must be before the event date.");
            }

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var eventData = new
                {
                    OrganiserId,
                    model.Name,
                    model.Description,
                    model.EventType,
                    EventDate = model.EventDate!.Value,
                    model.Venue,
                    model.City,
                    model.Province,
                    RegistrationCloseDate = model.RegistrationCloseDate!.Value,
                    model.MaxParticipants,
                    model.Status
                };

                var response = await _apiService.PostAsync("api/Events", eventData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Event created successfully!";
                    return RedirectToAction("Events");
                }

                ModelState.AddModelError("",
                    "Unable to create event: " +
                    await response.Content.ReadAsStringAsync());

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            try
            {
                var eventItem = await _apiService.GetAsync<EventViewModel>(
                    $"api/Events/{id}");

                if (eventItem == null || eventItem.OrganiserId != OrganiserId)
                    return NotFound();

                var model = new CreateEventViewModel
                {
                    Name = eventItem.Name,
                    Description = eventItem.Description,
                    EventType = eventItem.EventType,
                    EventDate = eventItem.EventDate,
                    Venue = eventItem.Venue,
                    City = eventItem.City,
                    Province = eventItem.Province,
                    RegistrationCloseDate = eventItem.RegistrationCloseDate,
                    MaxParticipants = eventItem.MaxParticipants,
                    Status = eventItem.Status
                };

                ViewBag.EventId = id;
                return View(model);
            }
            catch
            {
                TempData["Error"] = "Unable to load the event.";
                return RedirectToAction("Events");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, CreateEventViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            if (model.EventDate.HasValue &&
                model.RegistrationCloseDate.HasValue &&
                model.RegistrationCloseDate >= model.EventDate)
            {
                ModelState.AddModelError(
                    "RegistrationCloseDate",
                    "Registration closing date must be before the event date.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.EventId = id;
                return View(model);
            }

            try
            {
                var existingEvent = await _apiService.GetAsync<EventViewModel>(
                    $"api/Events/{id}");

                if (existingEvent == null ||
                    existingEvent.OrganiserId != OrganiserId)
                    return NotFound();

                var eventData = new
                {
                    OrganiserId,
                    model.Name,
                    model.Description,
                    model.EventType,
                    EventDate = model.EventDate!.Value,
                    model.Venue,
                    model.City,
                    model.Province,
                    RegistrationCloseDate = model.RegistrationCloseDate!.Value,
                    model.MaxParticipants,
                    model.Status
                };

                var response = await _apiService.PutAsync(
                    $"api/Events/{id}", eventData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Event updated successfully!";
                    return RedirectToAction("Events");
                }

                ModelState.AddModelError("",
                    "Unable to update event: " +
                    await response.Content.ReadAsStringAsync());

                ViewBag.EventId = id;
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.EventId = id;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            try
            {
                var eventItem = await _apiService.GetAsync<EventViewModel>(
                    $"api/Events/{id}");

                if (eventItem == null || eventItem.OrganiserId != OrganiserId)
                    return NotFound();

                return View(eventItem);
            }
            catch
            {
                TempData["Error"] = "Unable to load the selected event.";
                return RedirectToAction("Events");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEventConfirmed(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            try
            {
                var eventItem = await _apiService.GetAsync<EventViewModel>(
                    $"api/Events/{id}");

                if (eventItem == null || eventItem.OrganiserId != OrganiserId)
                    return NotFound();

                var response = await _apiService.DeleteAsync($"api/Events/{id}");

                if (response.IsSuccessStatusCode)
                    TempData["Success"] = "Event deleted successfully!";
                else
                    TempData["Error"] = "Unable to delete the event.";

                return RedirectToAction("Events");
            }
            catch
            {
                TempData["Error"] = "An error occurred while deleting the event.";
                return RedirectToAction("Events");
            }
        }
    }
}
