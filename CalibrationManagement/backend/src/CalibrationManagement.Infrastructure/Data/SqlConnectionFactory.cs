using Microsoft.Data.SqlClient;

namespace CalibrationManagement.Infrastructure.Data;

public interface IDbConnectionFactory
{
    SqlConnection CreateConnection();
}

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("The SQL Server connection string is empty.", nameof(connectionString));
        _connectionString = connectionString;
    }

    public SqlConnection CreateConnection() => new SqlConnection(_connectionString);
}
