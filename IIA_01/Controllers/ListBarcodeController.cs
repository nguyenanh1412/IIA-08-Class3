using IIA.Helpers;
using IIA_01.Controllers;
using IIA_01.HubSignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace IIA_02_Server_scanner.Controllers
{
    public class ListBarcodeController : Controller
    {
        private readonly ScannerRepository _main;

        private readonly IHubContext<LogHub> _hubContext;
        private readonly ILogger<HomeController> _logger;
        public ListBarcodeController(IConfiguration configuration, IHubContext<LogHub> hubContext, ILogger<HomeController> logger)
        {
            _main = new ScannerRepository(configuration);
            _hubContext = hubContext;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            return RedirectToAction("Error", "Home");
        }
    }
}
