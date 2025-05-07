using System;
using System.Threading.Tasks;
using IIA_01.HubSignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace IIS_CountSheep.Controllers
{
    public class CountSheepController : Controller
    {
        private readonly IHubContext<LogHub> _hubContext;
        private readonly CountSheepRepository _sheep;
        public CountSheepController(IConfiguration configuration, IHubContext<LogHub> hubContext) {
            _hubContext = hubContext;
            _sheep = new CountSheepRepository(configuration);
        }
        public async Task<IActionResult> Index()
        {
            var ListSheep = await _sheep.GetAllSheeps();
            return View(ListSheep);
        }
        private static readonly Random random = new Random();
        [HttpGet]
        public async Task<IActionResult> CountSheep(int quantity)
        {
            List<Sheep> sheepList = new List<Sheep>();

            for (int i = 0; i < quantity; i++)
            {
                Sheep newSheep = new Sheep();
                sheepList.Add(newSheep);
                newSheep.Id = await _sheep.InsertAnimalRecord(newSheep);
                await _hubContext.Clients.All.SendAsync("ReceiveSheepInfo", GenerateSheepHtml(newSheep));

                Console.WriteLine($"Sheep {i + 1}: {newSheep}");
            }
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> XuatCuu(int ID)
        {
            await _sheep.XuatCuu(ID);
            return Ok();
        }
        private string GenerateSheepHtml(Sheep sheep)
        {
            return $@"
            <div id=""{sheep.Id}"" class=""sheep-card bg-white rounded-lg shadow-md overflow-hidden"">
                <div class=""p-4"">
                    <h3 class=""font-medium text-lg mb-1"">Cừu {sheep.Color}</h3>
                    <p class=""text-gray-600 text-sm"">Cân nặng: {sheep.MeatWeightKg}Kg</p>
                    <p class=""text-gray-600 text-sm"">Cân lông: {sheep.WoolWeightKg}Kg</p>
                </div>
            </div>
            ";
        }
    }
}
