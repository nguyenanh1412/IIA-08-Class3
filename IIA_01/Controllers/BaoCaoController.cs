using System.Threading.Tasks;
using IIA_01.HubSignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IIS_CountSheep.Controllers
{
    public class BaoCaoController : Controller
    {
        private readonly IHubContext<LogHub> _hubContext;
        private readonly CountSheepRepository _sheep;
        public BaoCaoController(IConfiguration configuration, IHubContext<LogHub> hubContext)
        {
            _hubContext = hubContext;
            _sheep = new CountSheepRepository(configuration);
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> BaoCaoTheoThoiGian(DateTime? FromDate, DateTime? ToDate)
        {
            // Giả lập dữ liệu xuất cừu theo từng thời điểm
            if (FromDate is null && ToDate is null)
            {
                return View("BaoCaoTheoThoiGian", new List<Sheep>());
            }
            var groupedData = await _sheep.XuatBaoCaoThoiGian(FromDate, ToDate);

            return View("BaoCaoTheoThoiGian", groupedData);
        }
        public async Task<IActionResult> ExportBaoCaoTheoThoiGian(DateTime? FromDate, DateTime? ToDate)
        {
            // Giả lập dữ liệu xuất cừu theo từng thời điểm
            if (FromDate is null && ToDate is null)
            {
                return View("BaoCaoTheoThoiGian", new List<Sheep>());
            }
            var groupedData = await _sheep.XuatBaoCaoThoiGian(FromDate, ToDate);
            // Gọi service để xuất Excel
            byte[] fileContent = _sheep.ExportPeopleToExcel(groupedData);

            // Trả về file Excel cho người dùng
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PeopleList.xlsx");
        }
        [HttpGet]
        public async Task<IActionResult> ExportBaoCaoTheoMau(DateTime? FromDate, DateTime? ToDate, string mauCuu)
        {
            // Giả lập dữ liệu xuất cừu theo từng thời điểm
            if (FromDate is null && ToDate is null)
            {
                return View("BaoCaoTheoThoiGian", new List<Sheep>());
            }
            var groupedData = await _sheep.XuatBaoCaoMau(FromDate, ToDate, mauCuu);
            // Gọi service để xuất Excel
            byte[] fileContent = _sheep.ExportPeopleToExcel(groupedData);

            // Trả về file Excel cho người dùng
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PeopleList.xlsx");
        }
        public async Task<IActionResult> BaoCaoTheoMauAsync(DateTime? FromDate, DateTime? ToDate, string mauCuu)
        {
            // Giả lập dữ liệu xuất cừu theo từng thời điểm
            if (FromDate is null && ToDate is null)
            {
                return View("BaoCaoTheoMau", new List<Sheep>());
            }
            var groupedData = await _sheep.XuatBaoCaoMau(FromDate, ToDate, mauCuu);
            return View("BaoCaoTheoMau", groupedData);
        }
        private List<ExportDataGroup> GroupExportDataByTimeInterval(List<SheepExportData> exportData, int intervalInSeconds)
        {
            var groupedData = new List<ExportDataGroup>();
            var currentTime = exportData.Max(x => x.Time);

            // Nhóm các cột thời gian theo từng khoảng interval (ví dụ 5 giây)
            for (int i = 0; i < exportData.Count; i++)
            {
                var timeIntervalStart = currentTime.AddSeconds(-(i * intervalInSeconds));
                var exportCount = exportData
                    .Where(x => x.Time <= timeIntervalStart && x.Time > timeIntervalStart.AddSeconds(-intervalInSeconds))
                    .Sum(x => x.ExportCount);

                groupedData.Add(new ExportDataGroup
                {
                    TimeInterval = timeIntervalStart,
                    ExportCount = exportCount
                });
            }

            return groupedData;
        }
    }
}
