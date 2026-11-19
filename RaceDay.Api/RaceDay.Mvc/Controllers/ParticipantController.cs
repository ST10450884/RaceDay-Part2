
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Mvc.Controllers
{
    public class ParticipantController : Controller
    {
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (userId == null || role != "Participant")
            {
                return RedirectToAction("Login", "Auth");
            }

            return Content("Welcome to the RaceDay Participant Dashboard!");
        }
    }
}
