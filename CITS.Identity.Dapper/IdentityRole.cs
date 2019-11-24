using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace CITS.Identity.Dapper
{
	public class IdentityRole
	{
        public string Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public string ConcurrencyStamp { get; set; }
        internal List<Claim> Claims { get; set; }
    }
}
