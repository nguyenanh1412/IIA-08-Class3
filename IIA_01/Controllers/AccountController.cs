using Microsoft.AspNetCore.Mvc;

namespace YourProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepo;

        public AccountController(IConfiguration configuration)
        {
            _userRepo = new UserRepository(configuration);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            var user = _userRepo.CheckLogin(username, password);

            if (user != null)
            {
                HttpContext.Session.SetString("User", user.Username); // Use HttpContext.Session
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
            return View();
        }

        public ActionResult Logout()
        {
            HttpContext.Session.Clear(); // Use HttpContext.Session
            return RedirectToAction("Index", "Home");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddControllersWithViews();
        }
    }
}
