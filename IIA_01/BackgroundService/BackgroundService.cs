using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

public class BackgroundReportService : BackgroundService
{
    public BackgroundReportService(IConfiguration cf)
    {
        reportService = new CountSheepRepository(cf);
    }
    private CountSheepRepository reportService;
    private readonly string _baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "ExportedReports");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await GenerateReportsAsync(reportService, stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Delay 5 phút
        }
    }

    private async Task GenerateReportsAsync(CountSheepRepository reportService, CancellationToken stoppingToken)
    {
        try
        {
            var now = DateTime.Now;
            var folderName = now.ToString("yyyyMMdd_HHmmss");
            var folderPath = Path.Combine(_baseFolder, folderName);
            Directory.CreateDirectory(folderPath);

            // Xuất tổng số lượng cừu
            var TongSo = await reportService.XuatBaoCaoThoiGian( DateTime.Now.AddMinutes(-5), DateTime.Now);
            var reportAll = reportService.ExportPeopleToExcel(TongSo);
            File.WriteAllBytes(Path.Combine(folderPath, "TongSoLuongCuu.xlsx"), reportAll);

            // Xuất theo màu
            var colors = new[] { "Trắng", "Đen", "Xám" };
            foreach (var color in colors)
            {
                var theMau = await reportService.XuatBaoCaoMau(DateTime.Now.AddMinutes(-5), DateTime.Now, color);
                var report = reportService.ExportPeopleToExcel(theMau);
                File.WriteAllBytes(Path.Combine(folderPath, $"SoLuong_{color}.xlsx"), report);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi tạo báo cáo tự động: " + ex.Message);
        }
    }
}
