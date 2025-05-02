using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Data.SqlClient;
using System.IO;

namespace RotativaOutputApp.Data
{
    /// <summary>
    /// Utility class for initializing the database schema and stored procedures
    /// </summary>
    public class DatabaseInitializer
    {
        private readonly string? _connectionString;
        private readonly string? _masterConnectionString;

        public DatabaseInitializer(IConfiguration configuration)
        {
            var server = configuration.GetConnectionString("DefaultConnection")?.Split(';')
                .FirstOrDefault(s => s.Trim().StartsWith("Server=", StringComparison.OrdinalIgnoreCase))
                ?.Split('=')[1];

            if (string.IsNullOrEmpty(server))
            {
                throw new InvalidOperationException("Server information not found in connection string.");
            }

            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string not found.");
            _masterConnectionString = $"Server={server};Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        /// <summary>
        /// Initializes the database, tables, and stored procedures
        /// </summary>
        public void Initialize()
        {
            try
            {
                CreateDatabaseIfNotExists();
                ExecuteScriptFile("DatabaseSetup.sql");
                
                // Execute individual stored procedure scripts
                ExecuteScriptFile("usp_GetAllReports.sql");
                ExecuteScriptFile("usp_GetReportById.sql");
                ExecuteScriptFile("usp_GetReportsByDateRange.sql");
                
                Console.WriteLine("Database initialization completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Creates the database if it doesn't exist
        /// </summary>
        private void CreateDatabaseIfNotExists()
        {
            // Safely extract the database name from connection string
            string? databaseNamePart = _connectionString?.Split(';')
                .FirstOrDefault(s => s?.Trim().StartsWith("Database=", StringComparison.OrdinalIgnoreCase) == true);
                
            string? databaseName = null;
            if (databaseNamePart != null) {
                var parts = databaseNamePart.Split('=');
                if (parts.Length > 1) {
                    databaseName = parts[1];
                }
            }

            if (string.IsNullOrEmpty(databaseName))
            {
                throw new InvalidOperationException("Database name not found in connection string.");
            }

            string query = $@"
                IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}')
                BEGIN
                    CREATE DATABASE [{databaseName}];
                END";

            using (var connection = new SqlConnection(_masterConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Executes a SQL script file
        /// </summary>
        private void ExecuteScriptFile(string fileName)
        {
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Scripts", fileName);

            if (!File.Exists(scriptPath))
            {
                scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Scripts", fileName);
                if (!File.Exists(scriptPath))
                {
                    throw new FileNotFoundException($"SQL script file not found: {fileName}");
                }
            }

            string script = File.ReadAllText(scriptPath);

            // Split script into batches by GO statements (properly handling line-based GO statements)
            string[] batches = System.Text.RegularExpressions.Regex.Split(
                script,
                @"^\s*GO\s*$",
                System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                foreach (string batch in batches)
                {
                    if (!string.IsNullOrWhiteSpace(batch))
                    {
                        using (var command = new SqlCommand(batch, connection))
                        {
                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error executing batch in {fileName}: {ex.Message}");
                                Console.WriteLine($"Problematic SQL: {batch}");
                                // Log errors but continue with other batches
                            }
                        }
                    }
                }
            }
        }
    }
}
