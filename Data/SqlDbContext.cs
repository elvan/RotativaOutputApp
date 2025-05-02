using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace RotativaOutputApp.Data
{
    /// <summary>
    /// SQL Server implementation of IDbContext
    /// </summary>
    public class SqlDbContext : IDbContext
    {
        private readonly string _connectionString;
        private SqlConnection? _connection;
        private bool _disposed = false;

        public SqlDbContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                throw new InvalidOperationException("Database connection string 'DefaultConnection' not found in configuration.");
        }

        /// <summary>
        /// Creates and opens a new SQL connection
        /// </summary>
        public SqlConnection CreateConnection()
        {
            _connection = new SqlConnection(_connectionString);
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            return _connection;
        }

        /// <summary>
        /// Creates a command for a stored procedure with parameters
        /// </summary>
        public SqlCommand CreateStoredProcCommand(string storedProcedureName, params SqlParameter[] parameters)
        {
            SqlCommand command = new SqlCommand(storedProcedureName, CreateConnection())
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return command;
        }

        /// <summary>
        /// Executes a command and returns a data reader
        /// </summary>
        public SqlDataReader ExecuteReader(SqlCommand command)
        {
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// Executes a non-query command
        /// </summary>
        public int ExecuteNonQuery(SqlCommand command)
        {
            int result = command.ExecuteNonQuery();
            if (command.Connection != null && command.Connection.State == ConnectionState.Open)
            {
                command.Connection.Close();
            }
            return result;
        }

        /// <summary>
        /// Executes a scalar command
        /// </summary>
        public object ExecuteScalar(SqlCommand command)
        {
            object result = command.ExecuteScalar();
            if (command.Connection != null && command.Connection.State == ConnectionState.Open)
            {
                command.Connection.Close();
            }
            return result;
        }

        /// <summary>
        /// Disposes of the connection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_connection != null && _connection.State == ConnectionState.Open)
                    {
                        _connection.Close();
                    }
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
