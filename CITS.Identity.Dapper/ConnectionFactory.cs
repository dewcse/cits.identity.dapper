using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace CITS.Identity.Dapper
{
    public class ConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly string _connectionString;

        public ConnectionFactory(string connectionString) => _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

        public async Task<IDbConnection> CreateConnectionAsync()
        {
            try
            {
                var connString =  new NpgsqlConnection(_connectionString);
                await connString.OpenAsync();
                return connString;
            }
            catch
            {
                throw;
            }
        }
    }
}
