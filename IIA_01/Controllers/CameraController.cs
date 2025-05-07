using IIA_01.HubSignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MvCameraControl;

namespace IIA_Camera_Web.Controllers
{
    public class CameraController : Controller
    {
        private readonly IHubContext<LogHub> _hubContext;
        private CameraHub _camHub;
        private readonly CameraReposotory _cam;
        Thread receiveThread = null;

        private bool isGrabbing;
        public CameraController(IConfiguration configuration, IHubContext<LogHub> hubContext, IHubContext<CameraHub> camHub, CameraReposotory cam)
        {
            _hubContext = hubContext;
            //_cam = new CameraReposotory(configuration,hubContext);
            _cam = cam;
        }
        public IActionResult Index()
        {
            var list = _cam.RefreshDeviceList();
            return View(list);
        }
        [HttpGet]
        public IActionResult ConnectCam(string serial)
        {
            //Connect cam
            var deviceInfo = _cam.RefreshDeviceList().FirstOrDefault(x => x.SerialNumber == serial);
            IDevice device = null;
            try
            {
                device = DeviceFactory.CreateDevice(deviceInfo);
            }
            catch (Exception ex)
            {
                throw;
            }
            var res = _cam.bnOpen_Click(device);

            return Ok(res);
        }
        public IActionResult DisconnectCam()
        {
            _cam.bnClose_Click();
            return Ok();
        }
        public IActionResult Capture()
        {
            var isCap =  _cam.bnSaveJpg_Click();
            return Ok(isCap);
        }
        public IActionResult BurtShooting(int imageNum, int secNum)
        {
            var isCapBurt = _cam.BurtShooting(imageNum, secNum);
            return Ok(isCapBurt);
        }
        public IActionResult StopBurt()
        {
            var StopBurt = _cam.StopBurt();
            return Ok(StopBurt);
        }
    }
}
