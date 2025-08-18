using System.Data;
using Microsoft.Data.SqlClient;

namespace AiDbMaster.Models
{
    public class DatabaseQuery
    {
        private readonly string _connectionString;

        public DatabaseQuery(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PSTreeConnection") ?? 
                throw new InvalidOperationException("Connection string 'PSTreeConnection' not found.");
        }

        public async Task<DataTable> ExecuteQueryAsync(string query)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);
            using var adapter = new SqlDataAdapter(command);
            var dataTable = new DataTable();

            try
            {
                await connection.OpenAsync();
                await Task.Run(() => adapter.Fill(dataTable));
                return dataTable;
            }
            catch (Exception ex)
            {
                throw new Exception($"Errore nell'esecuzione della query: {ex.Message}", ex);
            }
        }
    }
} 