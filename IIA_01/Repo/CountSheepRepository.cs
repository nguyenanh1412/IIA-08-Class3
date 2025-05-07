using System.Data;
using System.Data.SqlClient;
using Dapper;
using NCalc;
using IIA_01.Models.LogModel;
using IIA_02_Server_scanner.Models.ScannerModel;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using System;

public class CountSheepRepository
{
    private readonly string _connectionString;

    public CountSheepRepository(IConfiguration configuration) // Inject IConfiguration
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    public async Task<BarcodeDataModel> GetBarcodeByMaVach(string maVach)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var parameters = new DynamicParameters();
            parameters.Add("@MaVach", maVach);

            var result = await connection.QueryFirstOrDefaultAsync<BarcodeDataModel>(
                "sp_GetBarcodeByMaVach",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }
    }

    public async Task<int> InsertAnimalRecord(Sheep input)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var parameters = new DynamicParameters();
            parameters.Add("@TimeCreated", DateTime.Now);
            parameters.Add("@Color", input.Color);
            parameters.Add("@MeatWeightKg", input.MeatWeightKg);
            parameters.Add("@WoolWeightKg", input.WoolWeightKg);


            var result = await connection.ExecuteScalarAsync<int>(
                "InsertAnimalRecord",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }
    }
    public async Task<List<Sheep>> GetAllSheeps()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var parameters = new DynamicParameters();
            var result = await connection.QueryAsync<Sheep>(
                "GetAllAnimalRecords",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }
    }
    
    public async Task<int> XuatCuu(int ID)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ID", ID);


            var result = await connection.ExecuteScalarAsync<int>(
                "ExportSheep",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }
    }
    public async Task<List<Sheep>> XuatBaoCaoThoiGian(DateTime? FromDate, DateTime? ToDate)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var result = await connection.QueryAsync<Sheep>(
                "GetSheepExportReport",
                new { FromDate = FromDate.Value, ToDate = ToDate.Value },
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }
    }
    public byte[] ExportPeopleToExcel(List<Sheep> people)
    {
        using (var package = new ExcelPackage())
        {
            // Tạo worksheet
            var worksheet = package.Workbook.Worksheets.Add("People");

            // Thêm tiêu đề cho cột
            worksheet.Cells[1, 1].Value = "STT";
            worksheet.Cells[1, 2].Value = "Thời Gian Xuất";
            worksheet.Cells[1, 3].Value = "Màu Cừu";
            worksheet.Cells[1, 4].Value = "Trọng Lượng Thịt (kg)";
            worksheet.Cells[1, 5].Value = "Trọng Lượng Lông (kg)";
            worksheet.Cells[1, 6].Value = "Trạng thái";

            // Thêm dữ liệu vào các ô
            for (int i = 0; i < people.Count; i++)
            {
                // Thêm số thứ tự
                worksheet.Cells[i + 2, 1].Value = i + 1; // Số thứ tự từ 1 đến N

                worksheet.Cells[i + 2, 2].Value = people[i].NgayXuat.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cells[i + 2, 3].Value = people[i].Color;
                worksheet.Cells[i + 2, 4].Value = people[i].MeatWeightKg;
                worksheet.Cells[i + 2, 5].Value = people[i].WoolWeightKg;
                worksheet.Cells[i + 2, 6].Value = people[i].Status;
            }

            // Thiết lập kiểu dữ liệu (tuỳ chọn)
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Lưu file Excel vào bộ nhớ tạm (MemoryStream)
            using (var memoryStream = new MemoryStream())
            {
                package.SaveAs(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
    public async Task<List<Sheep>> XuatBaoCaoMau(DateTime? FromDate, DateTime? ToDate, string mauCuu)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var result = await connection.QueryAsync<Sheep>(
                "GetSheepExportReportTheoMau",
                new { FromDate = FromDate.Value, ToDate = ToDate.Value, MauCuu = mauCuu },
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }
    }

}
