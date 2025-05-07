using System.Data;
using System.Data.SqlClient;
using Dapper;
using NCalc;
using IIA_01.Models.LogModel;
using IIA_02_Server_scanner.Models.ScannerModel;
using System.Text.RegularExpressions;

public class ScannerRepository
{
    private readonly string _connectionString;

    public ScannerRepository(IConfiguration configuration) // Inject IConfiguration
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    public async Task<BarcodeDataModel> GetBarcodeByMaVach(string maVach)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var parameters = new DynamicParameters();
            parameters.Add("@MaVach", maVach);

            var result =await connection.QueryFirstOrDefaultAsync<BarcodeDataModel>(
                "sp_GetBarcodeByMaVach",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }
    }

    public async Task<List<BarcodeDataModel>> GetListBarcode()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var parameters = new DynamicParameters();

            var result = await connection.QueryAsync<BarcodeDataModel>(
                "sp_GetListBarcode",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }
    }
    public async Task AddBarcodeAsync(BarcodeDataModel input)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var parameters = new DynamicParameters();
            parameters.Add("@MaVach", input.MaVach);
            parameters.Add("@BarcodeImageUrl", input.BarcodeImageUrl);
            parameters.Add("@CreatedBy", input.CreatedBy);

            var result = await connection.QueryFirstOrDefaultAsync<BarcodeDataModel>(
                "sp_InsertBarcode",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }

    public async Task UpdateBarcodeAsync(BarcodeDataModel input)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", input.Id);
            parameters.Add("@MaVach", input.MaVach);
            parameters.Add("@BarcodeImageUrl", input.BarcodeImageUrl);
            parameters.Add("@UpdatedBy", input.CreatedBy);

            await connection.ExecuteAsync(
                "sp_UpdateBarcode",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }
    public async Task DeleteBarcodeAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            await connection.ExecuteAsync(
                "sp_DeleteBarcode", // Tên stored procedure xóa
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }



    public async Task InsertLogAsync(LogViewModel input)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var parameters = new DynamicParameters();
            parameters.Add("@MaVach", input.MaVach);
            parameters.Add("@BarcodeImageUrl", input.BarcodeImageUrl);
            parameters.Add("@CreatedBy", input.CreatedBy);

            await connection.ExecuteAsync(
                "sp_InsertLogScaner",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }


    public List<LogViewModel> GetLogs()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var logs = connection.Query<LogViewModel>(
                "GetLogs",
                commandType: CommandType.StoredProcedure
            ).ToList();

            return logs;
        }
    }

}
