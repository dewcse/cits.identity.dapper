using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace CITS.Identity.Dapper
{
    public interface IDatabaseConnectionFactory
    {       
        Task<IDbConnection> CreateConnectionAsync();
    }
}
