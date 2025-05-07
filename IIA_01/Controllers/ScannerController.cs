using Microsoft.AspNetCore.Mvc;
using IIA.Helpers;
using IIA_01.Models.LogModel;
using IIA_01.HubSignalR;
using Microsoft.AspNetCore.SignalR;
using IIA_01.Controllers;
using Newtonsoft.Json;
using IIA_02_Server_scanner.Models.ScannerModel;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http.HttpResults;

namespace YourProject.Controllers
{
    public class ScannerController : Controller
    {
        private readonly ScannerRepository _main;

        private readonly IHubContext<LogHub> _hubContext;
        private readonly ILogger<HomeController> _logger;
        public ScannerController(IConfiguration configuration, IHubContext<LogHub> hubContext, ILogger<HomeController> logger)
        {
            _main = new ScannerRepository(configuration);
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult Scanner()
        {
            if (UserHelper.IsLoggedIn(HttpContext))
                return View();
            else return RedirectToAction("Error", "Home");
        }

        [HttpGet]
        public async Task<string> GetInforByCode(string textInput, string createby)
        {
            _logger.LogInformation("GetInforByCode: " + textInput);
            var result = await _main.GetBarcodeByMaVach(textInput);
            var logg = new LogViewModel();
            if (result != null && !string.IsNullOrEmpty(result.BarcodeImageUrl))
            {
                logg = new LogViewModel()
                {
                    MaVach = result.MaVach,
                    CreatedDate = DateTime.Now,
                    BarcodeImageUrl = result.BarcodeImageUrl,
                    CreatedBy = createby
                };
                await _main.InsertLogAsync(logg);
                await _hubContext.Clients.All.SendAsync("ReceiveLog", logg);
                return result.BarcodeImageUrl;
            }
            logg = new LogViewModel()
            {
                MaVach = textInput,
                CreatedDate = DateTime.Now,
                BarcodeImageUrl = "",
                CreatedBy = createby
            };
            await _main.InsertLogAsync(logg);
            await _hubContext.Clients.All.SendAsync("ReceiveLog", logg);
            return "";
        }

        [HttpPost]
        public IActionResult Create(BarcodeDataModel model)
        {
            if (UserHelper.IsLoggedIn(HttpContext) && model.BarcodeImageFile != null)
            {
                _logger.LogInformation("Create: " + JsonConvert.SerializeObject(model) + " - " + model.BarcodeImageFile.FileName);
                // Đường dẫn lưu ảnh
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Lưu file
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.BarcodeImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.BarcodeImageFile.CopyTo(fileStream);
                }

                // Lưu thông tin vào DB
                var data = new BarcodeDataModel()
                {
                    MaVach = model.MaVach,
                    BarcodeImageUrl = "/uploads/" + uniqueFileName,
                    CreatedDate = DateTime.Now,
                    CreatedBy = UserHelper.GetUsername(HttpContext)
                };
                _logger.LogInformation("Create Data Svae: " + JsonConvert.SerializeObject(data));
                _main.AddBarcodeAsync(data);

                return RedirectToAction("Index", "ListBarcode");
            }

            return RedirectToAction("Error", "Home");
        }

        [HttpPost]
        public IActionResult Edit(BarcodeDataModel model)
        {
            if (UserHelper.IsLoggedIn(HttpContext))
            {
                _logger.LogInformation("Edit: " + JsonConvert.SerializeObject(model) + " - " + model.BarcodeImageFile.FileName);
                // Đường dẫn lưu ảnh
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var url = model.BarcodeImageUrl;
                if (model.BarcodeImageFile != null)
                {
                    // Lưu file
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.BarcodeImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.BarcodeImageFile.CopyTo(fileStream);
                    }
                    url = "/uploads/" + uniqueFileName;
                }

                // Lưu thông tin vào DB
                var data = new BarcodeDataModel()
                {
                    Id = model.Id,
                    MaVach = model.MaVach,
                    BarcodeImageUrl = url,
                    CreatedDate = DateTime.Now,
                    CreatedBy = UserHelper.GetUsername(HttpContext)
                };
                _logger.LogInformation("Edit Data Svae: " + JsonConvert.SerializeObject(data));
                _main.UpdateBarcodeAsync(data);

                return RedirectToAction("Index", "ListBarcode");
            }

            return RedirectToAction("Error", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Delete: " + id);
                // Gọi repository để xóa mã vạch
                await _main.DeleteBarcodeAsync(id);

                return Ok("Xóa thành công");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home");
            }
        }

    }
}
