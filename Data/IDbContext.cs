using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace RotativaOutputApp.Data
{
    /// <summary>
    /// Interface for database context operations
    /// </summary>
    public interface IDbContext : IDisposable
    {
        /// <summary>
        /// Creates a new SqlConnection
        /// </summary>
        /// <returns>An open SqlConnection</returns>
        SqlConnection CreateConnection();
        
        /// <summary>
        /// Creates a SqlCommand for a stored procedure
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure</param>
        /// <param name="parameters">Optional parameters for the stored procedure</param>
        /// <returns>Configured SqlCommand</returns>
        SqlCommand CreateStoredProcCommand(string storedProcedureName, params SqlParameter[] parameters);
        
        /// <summary>
        /// Executes a stored procedure and returns a DataReader
        /// </summary>
        /// <param name="command">SqlCommand to execute</param>
        /// <returns>SqlDataReader with the results</returns>
        SqlDataReader ExecuteReader(SqlCommand command);
        
        /// <summary>
        /// Executes a non-query stored procedure
        /// </summary>
        /// <param name="command">SqlCommand to execute</param>
        /// <returns>Number of rows affected</returns>
        int ExecuteNonQuery(SqlCommand command);
        
        /// <summary>
        /// Executes a scalar stored procedure
        /// </summary>
        /// <param name="command">SqlCommand to execute</param>
        /// <returns>First column of first row</returns>
        object ExecuteScalar(SqlCommand command);
    }
}
