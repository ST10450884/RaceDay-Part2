
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using System.Text.Json;

namespace RaceDay.Mvc.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly ApiService _apiService;
        private readonly IConfiguration _configuration;

        public OrganiserController(
            ApiService apiService,
            IConfiguration configuration)
        {
            _apiService = apiService;
            _configuration = configuration;
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetInt32("UserId") != null &&
                   HttpContext.Session.GetString("UserRole") == "Organiser";
        }

        private int? OrganiserId =>
            HttpContext.Session.GetInt32("UserId");

        private string ApiBaseUrl =>
            _configuration["ApiSettings:BaseUrl"] ?? "";

        private void SetPosterViewData()
        {
            ViewBag.ApiBaseUrl = ApiBaseUrl;
        }

        private async Task<string?> UploadPoster(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            var response = await _apiService.UploadFileAsync(
                "api/EventPosters/upload", file);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    "Poster upload failed: " +
                    await response.Content.ReadAsStringAsync());
            }

            using var document = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync());

            if (!document.RootElement.TryGetProperty(
                    "posterUrl", out var posterUrl))
            {
                throw new Exception("The API did not return a poster URL.");
            }

            return posterUrl.GetString();
        }

        private async Task DeletePoster(string? posterUrl)
        {
            if (string.IsNullOrWhiteSpace(posterUrl))
                return;

            string blobName = posterUrl
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Last();

            var response = await _apiService.DeleteAsync(
                $"api/EventPosters/{Uri.EscapeDataString(blobName)}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    "Unable to delete the old poster: " +
                    await response.Content.ReadAsStringAsync());
            }
        }

        // ==========================================
        // DASHBOARD
        // ==========================================

        public IActionResult Index()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            ViewBag.OrganiserEmail =
                HttpContext.Session.GetString("UserEmail") ?? "Organiser";

            return View();
        }

        // ==========================================
        // EVENT MANAGEMENT
        // ==========================================

        [HttpGet]
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

            SetPosterViewData();
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

            if (model.PosterFile != null &&
                model.PosterFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "PosterFile",
                    "The poster must not exceed 5 MB.");
            }

            if (!ModelState.IsValid)
            {
                SetPosterViewData();
                return View(model);
            }

            string? uploadedPosterUrl = null;

            try
            {
                uploadedPosterUrl = await UploadPoster(model.PosterFile);

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
                    model.Status,
                    PosterUrl = uploadedPosterUrl
                };

                var response = await _apiService.PostAsync(
                    "api/Events", eventData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Event created successfully!";
                    return RedirectToAction("Events");
                }

                if (uploadedPosterUrl != null)
                    await DeletePoster(uploadedPosterUrl);

                ModelState.AddModelError(
                    "",
                    "Unable to create event: " +
                    await response.Content.ReadAsStringAsync());

                SetPosterViewData();
                return View(model);
            }
            catch (Exception ex)
            {
                if (uploadedPosterUrl != null)
                {
                    try
                    {
                        await DeletePoster(uploadedPosterUrl);
                    }
                    catch
                    {
                        // Preserve the original error.
                    }
                }

                ModelState.AddModelError("", ex.Message);
                SetPosterViewData();
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
                    Status = eventItem.Status,
                    PosterUrl = eventItem.PosterUrl
                };

                ViewBag.EventId = id;
                SetPosterViewData();

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
        public async Task<IActionResult> EditEvent(
            int id,
            CreateEventViewModel model)
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

            if (model.PosterFile != null &&
                model.PosterFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "PosterFile",
                    "The poster must not exceed 5 MB.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.EventId = id;
                SetPosterViewData();
                return View(model);
            }

            string? uploadedPosterUrl = null;

            try
            {
                var existingEvent = await _apiService.GetAsync<EventViewModel>(
                    $"api/Events/{id}");

                if (existingEvent == null ||
                    existingEvent.OrganiserId != OrganiserId)
                    return NotFound();

                // Keep the existing poster unless a replacement is uploaded.
                string? finalPosterUrl = existingEvent.PosterUrl;

                if (model.PosterFile != null && model.PosterFile.Length > 0)
                {
                    uploadedPosterUrl = await UploadPoster(model.PosterFile);
                    finalPosterUrl = uploadedPosterUrl;
                }

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
                    model.Status,
                    PosterUrl = finalPosterUrl
                };

                var response = await _apiService.PutAsync(
                    $"api/Events/{id}", eventData);

                if (response.IsSuccessStatusCode)
                {
                    // Delete the previous poster only after the event update succeeds.
                    if (uploadedPosterUrl != null &&
                        !string.IsNullOrWhiteSpace(existingEvent.PosterUrl))
                    {
                        await DeletePoster(existingEvent.PosterUrl);
                    }

                    TempData["Success"] = "Event updated successfully!";
                    return RedirectToAction("Events");
                }

                if (uploadedPosterUrl != null)
                    await DeletePoster(uploadedPosterUrl);

                ModelState.AddModelError(
                    "",
                    "Unable to update event: " +
                    await response.Content.ReadAsStringAsync());

                model.PosterUrl = existingEvent.PosterUrl;
                ViewBag.EventId = id;
                SetPosterViewData();

                return View(model);
            }
            catch (Exception ex)
            {
                if (uploadedPosterUrl != null)
                {
                    try
                    {
                        await DeletePoster(uploadedPosterUrl);
                    }
                    catch
                    {
                        // Preserve the original error.
                    }
                }

                ModelState.AddModelError("", ex.Message);
                ViewBag.EventId = id;
                SetPosterViewData();

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
                {
                    if (!string.IsNullOrWhiteSpace(eventItem.PosterUrl))
                    {
                        try
                        {
                            await DeletePoster(eventItem.PosterUrl);
                        }
                        catch
                        {
                            // Event deletion succeeded; poster cleanup can be handled separately.
                        }
                    }

                    TempData["Success"] = "Event deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Unable to delete the event.";
                }

                return RedirectToAction("Events");
            }
            catch
            {
                TempData["Error"] = "An error occurred while deleting the event.";
                return RedirectToAction("Events");
            }
        }

        // ==========================================
        // CATEGORY MANAGEMENT
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            try
            {
                var categories =
                    await _apiService.GetAsync<List<EventCategoryViewModel>>(
                        "api/EventCategories/organiser");

                var organiserEvents = await GetOrganiserEvents();

                foreach (var category in categories ??
                         new List<EventCategoryViewModel>())
                {
                    var matchingEvent = organiserEvents.FirstOrDefault(
                        e => e.EventId == category.EventId);

                    category.EventName = matchingEvent?.Name ?? "Unknown Event";
                }

                return View(categories ?? new List<EventCategoryViewModel>());
            }
            catch
            {
                ViewBag.Error = "Unable to load event categories.";
                return View(new List<EventCategoryViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateCategory()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            await LoadOrganiserEvents();

            if (((List<EventViewModel>)ViewBag.Events).Count == 0)
            {
                TempData["Error"] = "Create an event before adding categories.";
                return RedirectToAction("Events");
            }

            return View(new EventCategoryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(
            EventCategoryViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                await LoadOrganiserEvents();
                return View(model);
            }

            try
            {
                var selectedEvent = await _apiService.GetAsync<EventViewModel>(
                    $"api/Events/{model.EventId}");

                if (selectedEvent == null ||
                    selectedEvent.OrganiserId != OrganiserId)
                {
                    ModelState.AddModelError(
                        "EventId",
                        "Please select one of your own events.");

                    await LoadOrganiserEvents();
                    return View(model);
                }

                var categoryData = new
                {
                    model.EventId,
                    model.Name,
                    model.DistanceKm,
                    model.Fee,
                    model.MinimumAge,
                    model.Capacity,
                    model.StartTime
                };

                var response = await _apiService.PostAsync(
                    "api/EventCategories", categoryData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Event category created successfully!";
                    return RedirectToAction("Categories");
                }

                ModelState.AddModelError(
                    "",
                    "Unable to create category: " +
                    await response.Content.ReadAsStringAsync());

                await LoadOrganiserEvents();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadOrganiserEvents();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            try
            {
                var category =
                    await _apiService.GetAsync<EventCategoryViewModel>(
                        $"api/EventCategories/{id}");

                if (category == null)
                    return NotFound();

                var eventItem = await _apiService.GetAsync<EventViewModel>(
                    $"api/Events/{category.EventId}");

                if (eventItem == null ||
                    eventItem.OrganiserId != OrganiserId)
                    return NotFound();

                await LoadOrganiserEvents();
                category.EventName = eventItem.Name;

                return View(category);
            }
            catch
            {
                TempData["Error"] = "Unable to load the selected category.";
                return RedirectToAction("Categories");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(
            int id,
            EventCategoryViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                await LoadOrganiserEvents();
                return View(model);
            }

            try
            {
                var categoryData = new
                {
                    CategoryId = id,
                    model.EventId,
                    model.Name,
                    model.DistanceKm,
                    model.Fee,
                    model.MinimumAge,
                    model.Capacity,
                    model.StartTime
                };

                var response = await _apiService.PutAsync(
                    $"api/EventCategories/{id}", categoryData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Event category updated successfully!";
                    return RedirectToAction("Categories");
                }

                ModelState.AddModelError(
                    "",
                    "Unable to update category: " +
                    await response.Content.ReadAsStringAsync());

                await LoadOrganiserEvents();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadOrganiserEvents();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            try
            {
                var category =
                    await _apiService.GetAsync<EventCategoryViewModel>(
                        $"api/EventCategories/{id}");

                if (category == null)
                    return NotFound();

                var eventItem = await _apiService.GetAsync<EventViewModel>(
                    $"api/Events/{category.EventId}");

                if (eventItem == null ||
                    eventItem.OrganiserId != OrganiserId)
                    return NotFound();

                category.EventName = eventItem.Name;
                return View(category);
            }
            catch
            {
                TempData["Error"] = "Unable to load the selected category.";
                return RedirectToAction("Categories");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategoryConfirmed(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            try
            {
                var response = await _apiService.DeleteAsync(
                    $"api/EventCategories/{id}");

                if (response.IsSuccessStatusCode)
                    TempData["Success"] = "Event category deleted successfully!";
                else
                    TempData["Error"] =
                        "Unable to delete category: " +
                        await response.Content.ReadAsStringAsync();
            }
            catch
            {
                TempData["Error"] =
                    "An error occurred while deleting the category.";
            }

            return RedirectToAction("Categories");
        }

        private async Task<List<EventViewModel>> GetOrganiserEvents()
        {
            var allEvents =
                await _apiService.GetAsync<List<EventViewModel>>("api/Events");

            return allEvents?
                .Where(e => e.OrganiserId == OrganiserId)
                .ToList() ?? new List<EventViewModel>();
        }

        private async Task LoadOrganiserEvents()
        {
            ViewBag.Events = await GetOrganiserEvents();
        }

        // ==========================================
        // ENROLMENT MANAGEMENT
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Enrolments()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            try
            {
                var enrolments =
                    await _apiService.GetAsync<List<OrganiserEnrolmentViewModel>>(
                        "api/Enrolments/organiser");

                return View(enrolments ??
                    new List<OrganiserEnrolmentViewModel>());
            }
            catch
            {
                ViewBag.Error = "Unable to load enrolments. Please try again.";
                return View(new List<OrganiserEnrolmentViewModel>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEnrolmentStatus(
            int id,
            string status)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            try
            {
                var response = await _apiService.PutAsync(
                    $"api/Enrolments/{id}/status",
                    new { Status = status });

                if (response.IsSuccessStatusCode)
                    TempData["Success"] =
                        "Enrolment status updated successfully.";
                else
                    TempData["Error"] =
                        "Unable to update enrolment status: " +
                        await response.Content.ReadAsStringAsync();
            }
            catch
            {
                TempData["Error"] =
                    "An error occurred while updating the enrolment.";
            }

            return RedirectToAction("Enrolments");
        }

        // ==========================================
        // RESULTS MANAGEMENT
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Results()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            try
            {
                var results = await _apiService.GetAsync<List<ResultViewModel>>(
                    "api/Results");

                return View(results ?? new List<ResultViewModel>());
            }
            catch
            {
                ViewBag.Error = "Unable to load race results.";
                return View(new List<ResultViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateResult()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            await LoadConfirmedEnrolments();
            return View(new ResultViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateResult(ResultViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                await LoadConfirmedEnrolments();
                return View(model);
            }

            try
            {
                var response = await _apiService.PostAsync(
                    "api/Results",
                    new
                    {
                        model.EnrolmentId,
                        model.FinishTimeSeconds,
                        model.OverallPosition,
                        model.CategoryPosition,
                        model.Status
                    });

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Race result recorded successfully!";
                    return RedirectToAction("Results");
                }

                ModelState.AddModelError(
                    "",
                    "Unable to record result: " +
                    await response.Content.ReadAsStringAsync());

                await LoadConfirmedEnrolments();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadConfirmedEnrolments();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditResult(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            try
            {
                var result = await _apiService.GetAsync<ResultViewModel>(
                    $"api/Results/{id}");

                if (result == null)
                    return NotFound();

                return View(result);
            }
            catch
            {
                TempData["Error"] = "Unable to load the result.";
                return RedirectToAction("Results");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditResult(
            int id,
            ResultViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var response = await _apiService.PutAsync(
                    $"api/Results/{id}",
                    new
                    {
                        model.FinishTimeSeconds,
                        model.OverallPosition,
                        model.CategoryPosition,
                        model.Status
                    });

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Result updated successfully!";
                    return RedirectToAction("Results");
                }

                ModelState.AddModelError(
                    "",
                    "Unable to update result: " +
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
        public async Task<IActionResult> DeleteResult(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            try
            {
                var result = await _apiService.GetAsync<ResultViewModel>(
                    $"api/Results/{id}");

                if (result == null)
                    return NotFound();

                return View(result);
            }
            catch
            {
                TempData["Error"] = "Unable to load the selected result.";
                return RedirectToAction("Results");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteResultConfirmed(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            try
            {
                var response = await _apiService.DeleteAsync(
                    $"api/Results/{id}");

                if (response.IsSuccessStatusCode)
                    TempData["Success"] = "Result deleted successfully!";
                else
                    TempData["Error"] =
                        "Unable to delete result: " +
                        await response.Content.ReadAsStringAsync();
            }
            catch
            {
                TempData["Error"] =
                    "An error occurred while deleting the result.";
            }

            return RedirectToAction("Results");
        }

        private async Task LoadConfirmedEnrolments()
        {
            var enrolments =
                await _apiService.GetAsync<List<OrganiserEnrolmentViewModel>>(
                    "api/Enrolments/organiser");

            ViewBag.Enrolments =
                (enrolments ?? new List<OrganiserEnrolmentViewModel>())
                .Where(e => string.Equals(
                    e.Status,
                    "Confirmed",
                    StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}
