using ListenEventGrid.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ListenEventGrid.Services.InfoService
{
    public class InfoService : IInfoService
    {
        private string _sqlConnectionString;
        public InfoService(string sqlConnectionString)
        {
            _sqlConnectionString = sqlConnectionString;
        }
        public async Task CreateInfo(CreateInfoDto info, ILogger logger)
        {
            logger.LogInformation($"Creating Info record with name: {info.Name}");

            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                connection.Open();
                try
                {
                    // UPSERT query
                    var query = @$"
                        INSERT INTO Info (Name, Age)
                        VALUES (@Name, @Age);
                    ";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", info.Name != null ? info.Name : DBNull.Value);
                        command.Parameters.AddWithValue("@Age", info.Name != null ? info.Age : DBNull.Value);

                        using (await command.ExecuteReaderAsync())
                        {
                            Console.WriteLine($"Creating Info with name {info.Name} completed.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"Something went wrong while creating Info with name {info.Name} - Exception: {ex}");
                }
            }
            logger.LogInformation($"Creating Info with name {info.Name} completed successfully.");
        }
    }
}
