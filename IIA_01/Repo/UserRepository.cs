using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Dapper;
using IIA_01.Models.UserModel;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration) // Inject IConfiguration
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public User CheckLogin(string username, string password)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Username", username);
            parameters.Add("@Password", password);

            return connection.QueryFirstOrDefault<User>(
                "sp_CheckLogin",
                parameters,
                commandType: CommandType.StoredProcedure);
        }
    }
}
