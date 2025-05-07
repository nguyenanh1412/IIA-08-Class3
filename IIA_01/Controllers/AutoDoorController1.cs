using IIA.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace IIA_AutoDoor.Controllers
{
    public class AutoDoorController : Controller
    {
        public IActionResult Index()
        {
            if (UserHelper.IsLoggedIn(HttpContext))
            {
                return View();
            }
            else return RedirectToAction("Error", "Home");
        }
    }
}
